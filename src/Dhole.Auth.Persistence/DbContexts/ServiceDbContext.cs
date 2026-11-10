using System.Text.Json;
using CustomCodeFramework.Core.Domain.Entities;
using CustomCodeFramework.Messaging.Inbox;
using CustomCodeFramework.Messaging.Outbox;
using CustomCodeFramework.Postgres.EntityFramework.Configurations;
using CustomCodeFramework.Postgres.EntityFramework.DbContexts;
using Dhole.Auth.Domain.Roles.Entities;
using Dhole.Auth.Domain.Scopes.Entities;
using Dhole.Auth.Domain.Sessions.Entities;
using Dhole.Auth.Domain.Users.Entities;
using Dhole.Auth.Domain.Users.Events;
using Dhole.Auth.Domain.Sessions.Events;
using Dhole.Auth.Persistence.Auditing;
using Dhole.Auth.Persistence.Messaging;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;

namespace Dhole.Auth.Persistence.DbContexts;

public sealed class ServiceDbContext(
    DbContextOptions<ServiceDbContext> options, IConfiguration? realtimeConfiguration = null)
    : AppDbContextBase(options)
{
    private const string SourceService = "DholeAuthService";

    public DbSet<User> Users => Set<User>();
    public DbSet<UserRole> UserRoles => Set<UserRole>();
    public DbSet<UserScope> UserScopes => Set<UserScope>();

    public DbSet<Role> Roles => Set<Role>();
    public DbSet<RoleScope> RoleScopes => Set<RoleScope>();

    public DbSet<Scope> Scopes => Set<Scope>();
    public DbSet<Session> Sessions => Set<Session>();

    public DbSet<OutboxMessage> OutboxMessages => Set<OutboxMessage>();
    public DbSet<InboxMessage> InboxMessages => Set<InboxMessage>();

    public override Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        AppendRealtimeUserInvalidations();
        AppendRealtimeSecurityInvalidations();
        AddDomainEventsToOutbox();
        return base.SaveChangesAsync(cancellationToken);
    }

    public override int SaveChanges()
    {
        AppendRealtimeUserInvalidations();
        AppendRealtimeSecurityInvalidations();
        AddDomainEventsToOutbox();
        return base.SaveChanges();
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.HasDefaultSchema("auth");

        base.OnModelCreating(modelBuilder);

        modelBuilder.ApplyConfigurationsFromAssembly(typeof(ServiceDbContext).Assembly);

        modelBuilder.ApplyConfiguration(new OutboxMessageConfiguration());
        modelBuilder.ApplyConfiguration(new InboxMessageConfiguration());
    }

    private void AppendRealtimeUserInvalidations()
    {
        if (!AuthRealtimeInvalidation.IsEnabled(realtimeConfiguration))
            return;
        ChangeTracker.DetectChanges();
        // Includes role/scope assignment and revocation even when the User
        // aggregate is not tracked by the command handler. No login heartbeat.
        var affected = ChangeTracker.Entries<User>()
            .Where(x => x.State is EntityState.Added or EntityState.Modified or EntityState.Deleted &&
                        x.Entity.DomainEvents.Count > 0)
            .Select(x => x.Entity.Id)
            .Concat(ChangeTracker.Entries<UserRole>()
                .Where(x => x.State is EntityState.Added or EntityState.Deleted)
                .Select(x => x.Entity.UserId))
            .Concat(ChangeTracker.Entries<UserScope>()
                .Where(x => x.State is EntityState.Added or EntityState.Deleted)
                .Select(x => x.Entity.UserId))
            .Where(id => id != Guid.Empty)
            .Distinct()
            .ToArray();
        foreach (var userId in affected)
        {
            var eventId = Guid.NewGuid();
            var timestamp = DateTimeOffset.UtcNow;
            var correlationId =
                (AuditExecutionContextAccessor.Current?.CorrelationId ?? eventId).ToString("D");
            OutboxMessages.Add(new OutboxMessage
            {
                EventId = eventId,
                EventType = AuthRealtimeInvalidation.Type,
                EventName = AuthRealtimeInvalidation.Type,
                SourceService = SourceService,
                // The historical serializer adds one; never read auth secrets.
                PayloadJson = AuthRealtimeInvalidation.Serialize(
                    eventId, userId,
                    AuthRealtimeInvalidation.NextVersionHint() - 1,
                    correlationId, timestamp),
                CorrelationId = correlationId,
                Status = OutboxMessageStatus.Pending,
                RetryCount = 0,
                CreatedAtUtc = timestamp.UtcDateTime
            });
        }
    }

    private void AppendRealtimeSecurityInvalidations()
    {
        if (!AuthRealtimeInvalidation.IsEnabled(realtimeConfiguration))
            return;
        ChangeTracker.DetectChanges();

        // Never publish on login/refresh alone. Only persisted security changes.
        var users = ChangeTracker.Entries<User>()
            .Where(e => e.State is EntityState.Added or EntityState.Modified or EntityState.Deleted &&
                        e.Entity.DomainEvents.Count > 0)
            .Select(e => e.Entity)
            .ToArray();
        var changedUsers = users.Select(e => e.Id)
            .Concat(ChangeTracker.Entries<UserRole>()
                .Where(e => e.State is EntityState.Added or EntityState.Deleted)
                .Select(e => e.Entity.UserId))
            .Concat(ChangeTracker.Entries<UserScope>()
                .Where(e => e.State is EntityState.Added or EntityState.Deleted)
                .Select(e => e.Entity.UserId))
            .Where(id => id != Guid.Empty)
            .Distinct();

        foreach (var userId in changedUsers)
        {
            var tracked = users.FirstOrDefault(u => u.Id == userId);
            var forceLogout = tracked is not null &&
                (tracked.IsLocked || !tracked.IsActive || tracked.IsDeleted ||
                 tracked.DomainEvents.Any(e => e is UserPasswordChangedDomainEvent));
            QueueSecurityEvent(userId, null, forceLogout ? "forceLogout" : "refreshPermissions");
        }

        // One-session revocation must not log out another active session of that user.
        foreach (var session in ChangeTracker.Entries<Session>()
            .Where(e => e.State == EntityState.Modified && e.Entity.IsRevoked &&
                        e.Entity.DomainEvents.Any(d =>
                            d is SessionRevokedDomainEvent or SessionLoggedOutDomainEvent))
            .Select(e => e.Entity))
            QueueSecurityEvent(session.UserId, session.Id, "forceLogout");
    }

    private void QueueSecurityEvent(Guid userId, Guid? sessionId, string action)
    {
        var eventId = Guid.NewGuid();
        var at = DateTimeOffset.UtcNow;
        var correlationId =
            (AuditExecutionContextAccessor.Current?.CorrelationId ?? eventId).ToString("D");
        OutboxMessages.Add(new OutboxMessage
        {
            EventId = eventId,
            EventType = AuthRealtimeInvalidation.SecurityType,
            EventName = AuthRealtimeInvalidation.SecurityType,
            SourceService = SourceService,
            PayloadJson = AuthRealtimeInvalidation.SerializeSecurity(
                eventId, userId, sessionId, action, correlationId, at),
            CorrelationId = correlationId,
            Status = OutboxMessageStatus.Pending,
            RetryCount = 0,
            CreatedAtUtc = at.UtcDateTime
        });
    }

    private void AddDomainEventsToOutbox()
    {
        var aggregateRoots = ChangeTracker
            .Entries()
            .Select(x => x.Entity)
            .OfType<AggregateRoot<Guid>>()
            .Where(x => x.DomainEvents.Count > 0)
            .ToList();

        if (aggregateRoots.Count == 0)
        {
            return;
        }

        var outboxMessages = new List<OutboxMessage>();

        foreach (var aggregateRoot in aggregateRoots)
        {
            foreach (var domainEvent in aggregateRoot.DomainEvents)
            {
                var eventType = DomainEventOutboxMapper.GetEventType(domainEvent);
                var eventName = DomainEventOutboxMapper.GetEventName(domainEvent);
                var payloadJson = JsonSerializer.Serialize(domainEvent, domainEvent.GetType());
                var correlationId =
                    AuditExecutionContextAccessor.Current?.CorrelationId ?? Guid.NewGuid();

                outboxMessages.Add(
                    new OutboxMessage
                    {
                        EventId = domainEvent.EventId,
                        EventType = eventType,
                        EventName = eventName,
                        SourceService = SourceService,
                        PayloadJson = payloadJson,
                        HeadersJson = null,
                        CorrelationId = correlationId.ToString(),
                        Status = OutboxMessageStatus.Pending,
                        RetryCount = 0,
                        ErrorMessage = null,
                        CreatedAtUtc = DateTime.UtcNow,
                    }
                );

            }

            aggregateRoot.ClearDomainEvents();
        }

        OutboxMessages.AddRange(outboxMessages);
    }
}
