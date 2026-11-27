using Dhole.Auth.Application.Abstractions.Permissions;
using Dhole.Auth.Application.Permissions;
using Dhole.Auth.Persistence.Seed;

namespace Dhole.Auth.UnitTests;

[TestClass]
public sealed class OperationsTeamScopePolicyTests
{
    [TestMethod]
    public void MembershipUnionAndRemovalDoNotConferAdministrativeScopes()
    {
        var user = Guid.NewGuid();
        var other = Guid.NewGuid();
        var eligible = new HashSet<Guid> { user };
        OperationsTeamScopePolicy.Membership[] memberships =
        [
            new(user, "APERTURAS", false),
            new(user, "COORDINACION_ORIGEN", true),
            new(user, "COORDINACION_ORIGEN", false),
            new(other, "APERTURAS", true)
        ];
        var all = OperationsTeamScopePolicy.Resolve(memberships, eligible);
        Assert.IsTrue(all.Contains(new(user, "operations.case.create")));
        Assert.IsTrue(all.Contains(new(user, "operations.origin.update")));
        Assert.IsTrue(all.Contains(new(user, "operations.audit.read")));
        Assert.IsFalse(all.Any(g => g.UserId == other || g.ScopeCode is "operations.team.manage" or "operations.finance.manage" or "operations.case.read-all"));
        var remaining = OperationsTeamScopePolicy.Resolve([memberships[0], memberships[2]], eligible);
        Assert.IsTrue(remaining.Contains(new(user, "operations.origin.update")));
        Assert.IsFalse(remaining.Contains(new(user, "operations.audit.read")));
        Assert.IsEmpty(OperationsTeamScopePolicy.Resolve([], eligible));
    }

    [TestMethod]
    public async Task EffectivePermissionsCombineIndependentGrantSources()
    {
        var user = Guid.NewGuid();
        var direct = new[] { "operations.case.read", "crm.account.read" };
        var role = new[] { "operations.document.manage" };
        var team = OperationsTeamScopePolicy.Scopes("COORDINACION_ORIGEN", false);
        var first = await new EffectivePermissionService(new FakeRepo(new(user, true, false,
            ["Operativo Logístico"], direct, role, team))).GetAsync(user);
        CollectionAssert.Contains(first.EffectiveScopes.ToArray(), "operations.origin.update");
        CollectionAssert.Contains(first.EffectiveScopes.ToArray(), "crm.account.read");
        var after = await new EffectivePermissionService(new FakeRepo(new(user, true, false,
            ["Operativo Logístico"], direct, role, []))).GetAsync(user);
        CollectionAssert.DoesNotContain(after.EffectiveScopes.ToArray(), "operations.origin.update");
        CollectionAssert.Contains(after.EffectiveScopes.ToArray(), "operations.case.read");
        CollectionAssert.Contains(after.EffectiveScopes.ToArray(), "operations.document.manage");
    }

    [TestMethod]
    public void CrossTeamResponsibilitiesAreExplicitAndReadOnly()
    {
        var user = Guid.NewGuid();
        var eligible = new HashSet<Guid> { user };
        var leader = OperationsTeamScopePolicy.Resolve([new(user, "APERTURAS", true, "OPERATIONS_HEAD")], eligible);
        Assert.IsTrue(leader.Contains(new(user, "operations.case.read-all")));
        Assert.IsFalse(leader.Contains(new(user, "operations.case.update")));
        Assert.IsFalse(leader.Contains(new(user, "operations.team.manage")));
        var origin = OperationsTeamScopePolicy.Resolve([new(user, "COORDINACION_ORIGEN", true, "ORIGIN_SUPERVISOR")], eligible);
        Assert.IsTrue(origin.Contains(new(user, "operations.origin.read-all")));
        Assert.IsFalse(origin.Contains(new(user, "operations.case.read-all")));
        Assert.IsEmpty(OperationsTeamScopePolicy.Resolve([new(user, "APERTURAS", true, "ORIGIN_SUPERVISOR")], eligible));
        Assert.IsEmpty(OperationsTeamScopePolicy.Resolve([new(user, "APERTURAS", true, "UNKNOWN")], eligible));
        Assert.IsFalse(OperationsTeamScopePolicy.Scopes("DOCUMENTACION", false).Contains("operations.case.read-all"));
        Assert.IsTrue(OperationsTeamScopePolicy.Scopes("TRANSITO", false).Contains("operations.realtime.read"));
    }

    [TestMethod]
    public void DepartmentScopesDoNotGrantTeamAdministrationOrDelete()
    {
        CollectionAssert.Contains(OperationsTeamScopePolicy.Scopes("TRANSITO", false), "operations.transit.manage");
        CollectionAssert.Contains(OperationsTeamScopePolicy.Scopes("PANAMA", false), "operations.panama.manage");
        CollectionAssert.Contains(OperationsTeamScopePolicy.Scopes("COSTA_RICA", false), "operations.costa-rica.manage");
        CollectionAssert.Contains(OperationsTeamScopePolicy.Scopes("COORDINACION_ORIGEN", false, teamName: "Origen · Centroamérica"), "operations.central-america.manage");
        CollectionAssert.Contains(OperationsTeamScopePolicy.Scopes("COORDINACION_ORIGEN", false, teamName: "Origen · FTL"), "operations.land.manage");
        CollectionAssert.DoesNotContain(OperationsTeamScopePolicy.Scopes("COORDINACION_ORIGEN", false), "operations.land.manage");
        CollectionAssert.DoesNotContain(OperationsTeamScopePolicy.Scopes("APERTURAS", true), "operations.case.delete");
        CollectionAssert.DoesNotContain(OperationsTeamScopePolicy.Scopes("APERTURAS", true), "operations.team.member.assign");
    }

    private sealed class FakeRepo(UserPermissionData data) : IAuthPermissionReadRepository
    {
        public Task<UserPermissionData?> GetUserPermissionDataAsync(Guid userId, CancellationToken cancellationToken = default)
            => Task.FromResult<UserPermissionData?>(data);
        public Task<IReadOnlyCollection<string>> GetAllActiveScopeCodesAsync(CancellationToken cancellationToken = default)
            => Task.FromResult<IReadOnlyCollection<string>>([]);
    }
}
