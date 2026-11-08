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
