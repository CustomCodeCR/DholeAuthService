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
        AddDomainEventsToOutbox();
        return base.SaveChangesAsync(cancellationToken);
    }

    public override int SaveChanges()
    {
        AppendRealtimeUserInvalidations();
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
        // User business events identify meaningful security/permission mutations.
        // Login heartbeat changes are deliberately excluded.
        var users = ChangeTracker.Entries<User>()
            .Where(x => x.State is EntityState.Added or EntityState.Modified &&
                        x.Entity.DomainEvents.Count > 0)
            .Select(x => x.Entity).ToArray();
        foreach (var user in users)
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
                PayloadJson = AuthRealtimeInvalidation.Serialize(
                    eventId, user.Id, user.TokenVersion, correlationId, timestamp),
                CorrelationId = correlationId,
                Status = OutboxMessageStatus.Pending,
                RetryCount = 0,
                CreatedAtUtc = timestamp.UtcDateTime
            });
        }
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
