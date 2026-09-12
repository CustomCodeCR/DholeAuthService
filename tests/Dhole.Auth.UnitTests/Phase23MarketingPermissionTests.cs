using System.Collections;
using Dhole.Auth.Domain.Shared;
using Dhole.Auth.Persistence.Seed;

namespace Dhole.Auth.UnitTests;

[TestClass]
public sealed class Phase23MarketingPermissionTests
{
    private static readonly string[] RequiredMarketingScopes =
    [
        "cms.navigation.edit",
        "cms.collections.edit",
        "cms.forms.view",
        "cms.forms.edit",
        "cms.submissions.view",
        "cms.leads.view",
        "cms.leads.edit",
        "cms.meetings.view",
        "cms.meetings.edit",
        "cms.campaigns.view",
        "cms.campaigns.edit",
        "cms.redirects.edit",
        "cms.reviews.submit",
        "cms.reviews.approve",
    ];

    [TestMethod]
    public void MarketingRole_HasExpectedName()
    {
        Assert.AreEqual("Mercadeo", AuthConstants.SystemRoles.Marketing);
    }

    [TestMethod]
    public void ContentScopeCatalog_ContainsEveryPhase23Scope()
    {
        var catalogType = typeof(DatabaseSeeder).Assembly.GetType("Dhole.Auth.Persistence.Seed.ContentScopes");
        Assert.IsNotNull(catalogType);

        var allProperty = catalogType.GetProperty("All", System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Static);
        Assert.IsNotNull(allProperty);

        var values = (IEnumerable?)allProperty.GetValue(null);
        Assert.IsNotNull(values);

        var codes = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        foreach (var value in values)
        {
            if (value is null) continue;
            var code = value.GetType().GetProperty("Code")?.GetValue(value)?.ToString();
            if (!string.IsNullOrWhiteSpace(code)) codes.Add(code);
        }

        foreach (var expected in RequiredMarketingScopes)
        {
            Assert.IsTrue(codes.Contains(expected), $"Missing FASE 23 scope: {expected}");
        }

        Assert.IsTrue(codes.Contains("cms.view"), "Legacy CMS scopes must remain available.");
        Assert.IsTrue(codes.Contains("cms.edit"), "Legacy CMS scopes must remain available.");
        Assert.IsTrue(codes.Contains("cms.publish"), "Legacy CMS scopes must remain available.");
    }
}
