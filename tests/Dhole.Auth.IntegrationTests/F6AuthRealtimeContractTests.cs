using System.Text.Json;
using Dhole.Auth.Persistence.Messaging;
using Microsoft.Extensions.Configuration;

namespace Dhole.Auth.IntegrationTests;

[TestClass]
public sealed class F6AuthRealtimeContractTests
{
    private static IConfiguration Flags(bool approved, string release = AuthRealtimeInvalidation.ReleaseId)
    {
        return new ConfigurationBuilder().AddInMemoryCollection(new Dictionary<string, string?>
        {
            ["Features:UniversalRealtimeEnabled"] = "true",
            ["Realtime:Outbox:Enabled"] = "true",
            ["Realtime:BatchApproved"] = approved ? "true" : "false",
            ["Realtime:ReleaseId"] = release
        }).Build();
    }

    [TestMethod]
    public void Flags_off_wrong_release_or_null_fail_closed()
    {
        Assert.IsFalse(AuthRealtimeInvalidation.IsEnabled(null));
        Assert.IsFalse(AuthRealtimeInvalidation.IsEnabled(Flags(false)));
        Assert.IsFalse(AuthRealtimeInvalidation.IsEnabled(Flags(true, "mismatched")));
        var flags = Flags(true);
        Assert.IsTrue(AuthRealtimeInvalidation.IsEnabled(flags));
        flags["Realtime:Outbox:Enabled"] = "false";
        Assert.IsFalse(AuthRealtimeInvalidation.IsEnabled(flags));
    }

    [TestMethod]
    public void Minimal_event_hides_auth_credentials_emails_and_role_names()
    {
        var eventId = Guid.NewGuid();
        var userId = Guid.NewGuid();
        var json = AuthRealtimeInvalidation.Serialize(eventId, userId, 7,
            "test-f6-auth", DateTimeOffset.UtcNow);
        using var parsed = JsonDocument.Parse(json);
        var data = parsed.RootElement;
        Assert.AreEqual(AuthRealtimeInvalidation.Type, data.GetProperty("eventType").GetString());
        Assert.AreEqual($"auth:user:{userId:D}", data.GetProperty("audienceRef").GetString());
        Assert.AreEqual(8L, data.GetProperty("aggregateVersion").GetInt64());
        CollectionAssert.AreEquivalent(new[] { "changedFields" },
            data.GetProperty("payload").EnumerateObject().Select(x => x.Name).ToArray());
        foreach (var forbidden in new[] {"email", "password", "hash", "role", "scope", "token", "secret"})
            Assert.IsFalse(json.Contains(forbidden, StringComparison.OrdinalIgnoreCase),
                $"Disallowed auth field: {forbidden}");
    }

    [TestMethod]
    public void Reject_invalid_events_and_allow_initial_version()
    {
        var id = Guid.NewGuid();
        using var doc = JsonDocument.Parse(AuthRealtimeInvalidation.Serialize(id, id, 0,
            "created", DateTimeOffset.UtcNow));
        Assert.AreEqual(1L, doc.RootElement.GetProperty("aggregateVersion").GetInt64());
        Assert.ThrowsExactly<ArgumentException>(() =>
            AuthRealtimeInvalidation.Serialize(Guid.Empty, id, 1, "x", DateTimeOffset.UtcNow));
        Assert.ThrowsExactly<ArgumentException>(() =>
            AuthRealtimeInvalidation.Serialize(id, id, -1, "x", DateTimeOffset.UtcNow));
        Assert.ThrowsExactly<ArgumentException>(() =>
            AuthRealtimeInvalidation.Serialize(id, id, 1, "", DateTimeOffset.UtcNow));
    }
}


// Targeted security events are intentionally separate from resource invalidations.
// They must be delivered to a blocked user even when owner REST access is denied.
[TestClass]
public sealed class AuthSecurityRealtimeContractTests
{
    [TestMethod]
    public void Auth_security_revoke_is_scoped_to_one_session_without_credentials()
    {
        var userId = Guid.NewGuid();
        var sessionId = Guid.NewGuid();
        var evt = Guid.NewGuid();
        var json = AuthRealtimeInvalidation.SerializeSecurity(
            evt, userId, sessionId, "forceLogout", evt.ToString("D"), DateTimeOffset.UtcNow);
        using var document = JsonDocument.Parse(json);
        var root = document.RootElement;
        Assert.AreEqual(AuthRealtimeInvalidation.SecurityType, root.GetProperty("eventType").GetString());
        Assert.AreEqual(userId.ToString("D"), root.GetProperty("aggregateId").GetString());
        Assert.AreEqual(sessionId.ToString("D"), root.GetProperty("targetSessionId").GetString());
        Assert.AreEqual("forceLogout", root.GetProperty("action").GetString());
        Assert.AreEqual($"auth:user:{userId:D}", root.GetProperty("audienceRef").GetString());
        foreach (var forbidden in new[] { "password", "secret", "refreshToken", "email", "accessToken" })
            Assert.IsFalse(json.Contains(forbidden, StringComparison.OrdinalIgnoreCase));
    }

    [TestMethod]
    public void Auth_security_envelope_rejects_invalid_actions_and_actors()
    {
        var id = Guid.NewGuid();
        Assert.ThrowsExactly<ArgumentException>(() =>
            AuthRealtimeInvalidation.SerializeSecurity(id, id, null, "logoutEveryone",
                id.ToString("D"), DateTimeOffset.UtcNow));
        Assert.ThrowsExactly<ArgumentException>(() =>
            AuthRealtimeInvalidation.SerializeSecurity(id, Guid.Empty, null, "forceLogout",
                id.ToString("D"), DateTimeOffset.UtcNow));
        Assert.ThrowsExactly<ArgumentException>(() =>
            AuthRealtimeInvalidation.SerializeSecurity(id, id, Guid.Empty, "forceLogout",
                id.ToString("D"), DateTimeOffset.UtcNow));
    }
}
