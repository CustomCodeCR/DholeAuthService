using Dhole.Auth.Application.Abstractions.Permissions;
using Dhole.Auth.Application.Permissions;
using Dhole.Auth.Domain.Shared;

namespace Dhole.Auth.UnitTests;

[TestClass]
public sealed class CrmEffectivePermissionTests
{
    private static readonly string[] CreditScopes =
    [
        "crm.credit.read", "crm.credit.manage", "crm.credit.approve",
        "crm.contract.read", "crm.contract.manage", "crm.account.read-all"
    ];

    [TestMethod]
    public async Task ActiveSuperUser_HasAllActiveCreditScopes_EvenWithoutExplicitRoleScopeLinks()
    {
        var user = Guid.NewGuid();
        var repo = new FakePermissionsRepository(
            new UserPermissionData(user, true, false,
                [AuthConstants.SystemRoles.SuperUser], [], []), CreditScopes);
        var permissions = await new EffectivePermissionService(repo).GetAsync(user);
        foreach (var scope in CreditScopes)
            CollectionAssert.Contains(permissions.EffectiveScopes.ToArray(), scope);
    }

    [TestMethod]
    public async Task ActiveSuperUser_ReceivesCrmQuotationScopes_WithoutDirectGrants()
    {
        var user = Guid.NewGuid();
        var activeScopes = new[] { "crm.quote.read", "crm.quote.link" };
        var repo = new FakePermissionsRepository(
            new UserPermissionData(user, true, false,
                [AuthConstants.SystemRoles.SuperUser], [], []), activeScopes);
        var permissions = await new EffectivePermissionService(repo).GetAsync(user);

        CollectionAssert.Contains(permissions.EffectiveScopes.ToArray(), "crm.quote.read");
        CollectionAssert.Contains(permissions.EffectiveScopes.ToArray(), "crm.quote.link");
    }

    [TestMethod]
    public async Task OrdinaryAdministrator_DoesNotInheritCreditApproval()
    {
        var user = Guid.NewGuid();
        var repo = new FakePermissionsRepository(
            new UserPermissionData(user, true, false,
                [AuthConstants.SystemRoles.Administrator], ["crm.credit.read"], []), CreditScopes);
        var permissions = await new EffectivePermissionService(repo).GetAsync(user);
        CollectionAssert.DoesNotContain(permissions.EffectiveScopes.ToArray(), "crm.credit.approve");
    }

    [TestMethod]
    public async Task InactiveSuperUser_DoesNotReceiveCreditScopes()
    {
        var user = Guid.NewGuid();
        var repo = new FakePermissionsRepository(
            new UserPermissionData(user, false, false,
                [AuthConstants.SystemRoles.SuperUser], [], []), CreditScopes);
        var permissions = await new EffectivePermissionService(repo).GetAsync(user);
        Assert.AreEqual(0, permissions.EffectiveScopes.Count);
    }

    private sealed class FakePermissionsRepository(
        UserPermissionData user,
        IReadOnlyCollection<string> activeScopes) : IAuthPermissionReadRepository
    {
        public Task<UserPermissionData?> GetUserPermissionDataAsync(
            Guid userId, CancellationToken cancellationToken = default) =>
            Task.FromResult<UserPermissionData?>(user.UserId == userId ? user : null);

        public Task<IReadOnlyCollection<string>> GetAllActiveScopeCodesAsync(
            CancellationToken cancellationToken = default) =>
            Task.FromResult(activeScopes);
    }
}
