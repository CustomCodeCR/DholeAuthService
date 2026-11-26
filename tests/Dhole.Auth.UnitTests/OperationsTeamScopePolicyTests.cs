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

    private sealed class FakeRepo(UserPermissionData data) : IAuthPermissionReadRepository
    {
        public Task<UserPermissionData?> GetUserPermissionDataAsync(Guid userId, CancellationToken cancellationToken = default)
            => Task.FromResult<UserPermissionData?>(data);
        public Task<IReadOnlyCollection<string>> GetAllActiveScopeCodesAsync(CancellationToken cancellationToken = default)
            => Task.FromResult<IReadOnlyCollection<string>>([]);
    }
}
