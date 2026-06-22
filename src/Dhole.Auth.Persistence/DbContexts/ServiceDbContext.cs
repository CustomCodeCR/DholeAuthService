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

namespace Dhole.Auth.Persistence.DbContexts;

public sealed class ServiceDbContext(DbContextOptions<ServiceDbContext> options)
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
        AddDomainEventsToOutbox();
        return base.SaveChangesAsync(cancellationToken);
    }

    public override int SaveChanges()
    {
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

                outboxMessages.Add(
                    CreateAuditOutboxMessage(
                        originalEventId: domainEvent.EventId,
                        correlationId: correlationId,
                        eventName: eventName,
                        payloadJson: payloadJson,
                        sourceService: SourceService,
                        entityType: ResolveEntityType(eventName),
                        entityId: ResolveEntityId(domainEvent),
                        action: ResolveAction(eventName),
                        userId: ResolveUserId(domainEvent)
                    )
                );
            }

            aggregateRoot.ClearDomainEvents();
        }

        OutboxMessages.AddRange(outboxMessages);
    }

    private static OutboxMessage CreateAuditOutboxMessage(
        Guid originalEventId,
        Guid correlationId,
        string eventName,
        string payloadJson,
        string sourceService,
        string? entityType,
        Guid? entityId,
        string action,
        Guid? userId
    )
    {
        var current = AuditExecutionContextAccessor.Current;

        var auditPayload = new
        {
            EventId = originalEventId,
            CorrelationId = correlationId,
            SourceService = sourceService,
            EntityType = entityType,
            EntityId = entityId,
            Action = action,
            EventType = eventName,
            UserId = userId ?? current?.UserId,
            UserName = current?.UserName,
            IpAddress = current?.IpAddress,
            UserAgent = current?.UserAgent,
            OccurredAt = DateTime.UtcNow,
            BeforeJson = (string?)null,
            AfterJson = (string?)null,
            PayloadJson = payloadJson,
            Metadata = (string?)null,
            ErrorMessage = (string?)null,
            StackTrace = (string?)null,
            Details = Array.Empty<object>(),
        };

        return new OutboxMessage
        {
            EventId = Guid.NewGuid(),
            EventType = "Dhole.AuditLogs.Contracts.AuditEvents.RegisterAuditEventRequest",
            EventName = "audit.event.registered",
            SourceService = sourceService,
            PayloadJson = JsonSerializer.Serialize(auditPayload),
            HeadersJson = null,
            CorrelationId = correlationId.ToString(),
            Status = OutboxMessageStatus.Pending,
            RetryCount = 0,
            ErrorMessage = null,
            CreatedAtUtc = DateTime.UtcNow,
        };
    }

    private static string? ResolveEntityType(string eventName)
    {
        if (eventName.Contains(".session.", StringComparison.OrdinalIgnoreCase))
        {
            return "Session";
        }

        if (eventName.Contains(".user.", StringComparison.OrdinalIgnoreCase))
        {
            return "User";
        }

        if (eventName.Contains(".role.", StringComparison.OrdinalIgnoreCase))
        {
            return "Role";
        }

        if (eventName.Contains(".scope.", StringComparison.OrdinalIgnoreCase))
        {
            return "Scope";
        }

        return null;
    }

    private static string ResolveAction(string eventName)
    {
        if (eventName.EndsWith(".created", StringComparison.OrdinalIgnoreCase))
        {
            return "created";
        }

        if (eventName.EndsWith(".updated", StringComparison.OrdinalIgnoreCase))
        {
            return "updated";
        }

        if (eventName.EndsWith(".deleted", StringComparison.OrdinalIgnoreCase))
        {
            return "deleted";
        }

        if (eventName.EndsWith(".activated", StringComparison.OrdinalIgnoreCase))
        {
            return "activated";
        }

        if (eventName.EndsWith(".inactivated", StringComparison.OrdinalIgnoreCase))
        {
            return "inactivated";
        }

        if (eventName.EndsWith(".blocked", StringComparison.OrdinalIgnoreCase))
        {
            return "blocked";
        }

        if (eventName.EndsWith(".unblocked", StringComparison.OrdinalIgnoreCase))
        {
            return "unblocked";
        }

        if (eventName.EndsWith(".password_changed", StringComparison.OrdinalIgnoreCase))
        {
            return "password_changed";
        }

        if (eventName.EndsWith(".scope_assigned", StringComparison.OrdinalIgnoreCase))
        {
            return "scope_assigned";
        }

        if (eventName.EndsWith(".scope_revoked", StringComparison.OrdinalIgnoreCase))
        {
            return "scope_revoked";
        }

        if (eventName.EndsWith(".role_assigned", StringComparison.OrdinalIgnoreCase))
        {
            return "role_assigned";
        }

        if (eventName.EndsWith(".role_revoked", StringComparison.OrdinalIgnoreCase))
        {
            return "role_revoked";
        }

        if (eventName.EndsWith(".refreshed", StringComparison.OrdinalIgnoreCase))
        {
            return "refreshed";
        }

        if (eventName.EndsWith(".revoked", StringComparison.OrdinalIgnoreCase))
        {
            return "revoked";
        }

        if (eventName.EndsWith(".logged_out", StringComparison.OrdinalIgnoreCase))
        {
            return "logged_out";
        }

        return "unknown";
    }

    private static Guid? ResolveEntityId(object domainEvent)
    {
        var properties = domainEvent.GetType().GetProperties();

        var preferredNames = new[] { "SessionId", "UserId", "RoleId", "ScopeId", "EntityId", "Id" };

        foreach (var name in preferredNames)
        {
            var property = properties.FirstOrDefault(x =>
                x.Name.Equals(name, StringComparison.OrdinalIgnoreCase)
            );

            var value = GetGuidValue(property?.GetValue(domainEvent));

            if (value.HasValue)
            {
                return value.Value;
            }
        }

        var fallbackProperty = properties.FirstOrDefault(x =>
            x.Name.EndsWith("Id", StringComparison.OrdinalIgnoreCase)
        );

        return GetGuidValue(fallbackProperty?.GetValue(domainEvent));
    }

    private static Guid? ResolveUserId(object domainEvent)
    {
        var properties = domainEvent.GetType().GetProperties();

        var preferredNames = new[]
        {
            "CreatedBy",
            "UpdatedBy",
            "DeletedBy",
            "AssignedBy",
            "RevokedBy",
            "LoggedOutBy",
            "BlockedBy",
            "UnblockedBy",
        };

        foreach (var name in preferredNames)
        {
            var property = properties.FirstOrDefault(x =>
                x.Name.Equals(name, StringComparison.OrdinalIgnoreCase)
            );

            var value = GetGuidValue(property?.GetValue(domainEvent));

            if (value.HasValue)
            {
                return value.Value;
            }
        }

        return AuditExecutionContextAccessor.Current?.UserId;
    }

    private static Guid? GetGuidValue(object? value)
    {
        if (value is null)
        {
            return null;
        }

        if (value is Guid guid && guid != Guid.Empty)
        {
            return guid;
        }

        return null;
    }
}
