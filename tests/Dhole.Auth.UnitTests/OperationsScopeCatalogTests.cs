using System.Collections;
using System.Reflection;
using Dhole.Auth.Persistence.Seed;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Dhole.Auth.UnitTests;

[TestClass]
public sealed class OperationsScopeCatalogTests
{
    [TestMethod]
    public void EveryOperationsPermissionIsVisibleInTheSpanishAuthCatalog()
    {
        var catalog = typeof(DatabaseSeeder).Assembly.GetType(
            "Dhole.Auth.Persistence.Seed.AuthScopeCatalog", throwOnError: true)!;
        var definitions = (IEnumerable)catalog
            .GetProperty("Scopes", BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic)!
            .GetValue(null)!;
        var scopes = definitions.Cast<object>().Select(x => new
        {
            Code = (string)x.GetType().GetProperty("Code")!.GetValue(x)!,
            Name = (string)x.GetType().GetProperty("Name")!.GetValue(x)!
        }).Where(x => x.Code.StartsWith("operations.", StringComparison.Ordinal)).ToArray();

        Assert.AreEqual(16, scopes.Length, "Every Operations backend scope must appear in Auth.");
        Assert.IsTrue(scopes.All(x => x.Name.StartsWith("Operaciones · ", StringComparison.Ordinal)),
            "Operations permissions must have discoverable Spanish labels.");
        CollectionAssert.Contains(scopes.Select(x => x.Code).ToArray(), "operations.team.manage");
        CollectionAssert.Contains(scopes.Select(x => x.Code).ToArray(), "operations.aperture.validation.assign");
        CollectionAssert.Contains(scopes.Select(x => x.Code).ToArray(), "operations.aperture.validation.review");
        CollectionAssert.Contains(scopes.Select(x => x.Code).ToArray(), "operations.audit.read");
    }

    [TestMethod]
    public void RolePoolOnlyContainsTheTwoApprovedLogisticsRoles()
    {
        CollectionAssert.AreEquivalent(
            new[] { "Operador Logístico", "Supervisor Operativo" },
            OperationsRolePolicy.AssignmentEligible);
    }

    [TestMethod]
    public void StartupRetiresOnlyTheSevenUnrequestedRoles()
    {
        CollectionAssert.AreEquivalent(
            new[] {
                "Dynamics", "Dynamics Seguridad",
                "Operaciones · Administrador de equipos",
                "Operaciones · Validador de Aperturas",
                "Operaciones · Coordinación Origen",
                "Vendedor Jefe", "Vendedor Supervisor",
            },
            OperationsRolePolicy.DeprecatedAutoCreated);
        var seeder = typeof(DatabaseSeeder);
        Assert.IsNotNull(seeder.GetMethod("RetireUnrequestedAutoCreatedRolesAsync",
            BindingFlags.NonPublic | BindingFlags.Instance));
        Assert.IsNull(seeder.GetMethod("EnsureOperationsRolesAsync",
            BindingFlags.NonPublic | BindingFlags.Instance));
        Assert.IsNull(seeder.GetMethod("EnsureDynamicsAdministrationRolesAsync",
            BindingFlags.NonPublic | BindingFlags.Instance));
    }
}
