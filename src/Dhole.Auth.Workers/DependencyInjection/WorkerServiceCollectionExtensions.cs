using CustomCodeFramework.Messaging.DependencyInjection;
using CustomCodeFramework.Messaging.Outbox.DependencyInjection;
using CustomCodeFramework.Redis.DependencyInjection;
using CustomCodeFramework.Redis.Streams.DependencyInjection;
using CustomCodeFramework.Workers.DependencyInjection;
using Dhole.Auth.Worker.Outbox;
using Dhole.Auth.Worker.Streams;
using Dhole.Auth.Worker.Workers;

namespace Dhole.Auth.Worker.DependencyInjection;

public static class WorkerServiceCollectionExtensions
{
    public static IServiceCollection AddAuthWorker(
        this IServiceCollection services,
        IConfiguration configuration
    )
    {
        services.AddCustomCodeRedis(configuration);
        services.AddCustomCodeRedisStreams(configuration);

        services.AddCustomCodeMessaging(configuration);
        services.AddCustomCodeMessagingOutbox(configuration);

        services.AddCustomCodeOutboxProcessor<OutboxProcessor>();
        services.AddCustomCodeInboxProcessor<InboxProcessor>();
        services.AddCustomCodeMessagingOutboxHostedServices();

        services.AddCustomCodeRedisStreamConsumerBackgroundService();

        services.AddCustomCodeRedisStreamHandler<AuthUserBlockedStreamHandler>();

        services.AddCustomCodeRedisStreamHandler<AuthSessionCreatedStreamHandler>();
        services.AddCustomCodeRedisStreamHandler<AuthSessionRefreshedStreamHandler>();
        services.AddCustomCodeRedisStreamHandler<AuthSessionRevokedStreamHandler>();
        services.AddCustomCodeRedisStreamHandler<AuthSessionLoggedOutStreamHandler>();

        services.AddCustomCodeWorkers(configuration);
        services.AddCustomCodePeriodicWorker<SessionCleanupWorker>();

        return services;
    }
}
