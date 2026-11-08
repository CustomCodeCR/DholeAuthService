using System.Text.Json;
using Microsoft.Extensions.Configuration;

namespace Dhole.Auth.Persistence.Messaging;

/// <summary>
/// F6.04: only resource invalidation metadata. Auth password hashes, roles,
/// scopes, tokens, email and claims MUST NEVER enter realtime streams.
/// </summary>
public static class AuthRealtimeInvalidation
{
    public const string ReleaseId = "rt-20261107-01";
    public const string Stream = "dhole.auth.realtime.v1";
    public const string Type = "auth.user.changed.v1";

    public static bool IsEnabled(IConfiguration? config) =>
        config?.GetValue<bool>("Features:UniversalRealtimeEnabled") == true &&
        config.GetValue<bool>("Realtime:Outbox:Enabled") &&
        config.GetValue<bool>("Realtime:BatchApproved") &&
        string.Equals(config["Realtime:ReleaseId"], ReleaseId, StringComparison.Ordinal);

    public static string Serialize(Guid eventId, Guid userId, long tokenVersion,
        string correlationId, DateTimeOffset utc)
    {
        if (eventId == Guid.Empty || userId == Guid.Empty ||
            tokenVersion < 0 || tokenVersion == long.MaxValue ||
            string.IsNullOrWhiteSpace(correlationId) || utc.Offset != TimeSpan.Zero)
            throw new ArgumentException("Invalid Auth realtime invalidation.");
        return JsonSerializer.Serialize(new
        {
            eventId,
            eventType = Type,
            schemaVersion = 1,
            aggregateType = "user",
            aggregateId = userId.ToString("D"),
            // Initial tokenVersion 0 becomes resource version 1.
            aggregateVersion = tokenVersion + 1,
            occurredAtUtc = utc,
            producer = "DholeAuthService",
            correlationId,
            audienceRef = $"auth:user:{userId:D}",
            payload = new { changedFields = new[] { "identity" } }
        });
    }
}
