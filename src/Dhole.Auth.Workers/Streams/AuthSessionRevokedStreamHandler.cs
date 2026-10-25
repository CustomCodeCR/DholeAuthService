using CustomCodeFramework.Redis.Streams.Abstractions;
using CustomCodeFramework.Redis.Streams.Messages;

namespace Dhole.Auth.Worker.Streams;

internal sealed class AuthSessionRevokedStreamHandler(
    ILogger<AuthSessionRevokedStreamHandler> logger
) : IRedisStreamMessageHandler
{
    public string MessageType => "auth.session.revoked";

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
