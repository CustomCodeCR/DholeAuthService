using CustomCodeFramework.Redis.Streams.Abstractions;
using CustomCodeFramework.Redis.Streams.Messages;

namespace Dhole.Auth.Worker.Streams;

internal sealed class AuthSessionRefreshedStreamHandler(
    ILogger<AuthSessionRefreshedStreamHandler> logger
) : IRedisStreamMessageHandler
{
    public string MessageType => "auth.session.refreshed";

    public Task HandleAsync(
        RedisStreamEnvelope envelope,
        CancellationToken cancellationToken = default
    )
    {
        logger.LogInformation(
            "Received auth event {MessageType} with id {MessageId}.",
            envelope.MessageType,
            envelope.MessageId
        );

        return Task.CompletedTask;
    }
}
