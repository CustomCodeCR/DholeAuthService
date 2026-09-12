using Dhole.Auth.Application.Abstractions.Authentication;
using Dhole.Auth.Application.Abstractions.Permissions;
using Dhole.Auth.Domain.Roles.Entities;
using Dhole.Auth.Domain.Scopes.Entities;
using Dhole.Auth.Domain.Shared;
using Dhole.Auth.Domain.Users.Entities;
using Dhole.Auth.Domain.Users.Enums;
using Dhole.Auth.Persistence.DbContexts;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;

namespace Dhole.Auth.Persistence.Seed;

public sealed class DatabaseSeeder(
    ServiceDbContext dbContext,
    IPasswordHasher passwordHasher,
    IEffectivePermissionCache permissionCache,
    IOptions<SuperAdminSeedOptions> superAdminOptions
)
{
    private const string PricingWorkspaceScope = "pricing.workspace.access";
    private const string RateRequestCreateScope = "pricing.rate-request.create";
    private const string RateRequestViewSelectedScope = "pricing.rate-request.view-selected";
    private const string RateRequestViewAllScope = "pricing.rate-request.view-all";
    private const string SellerSupervisorRole = "Vendedor Supervisor";
    private const string SellerChiefRole = "Vendedor Jefe";
    private const string CmsScopePrefix = "cms.";

    private readonly SuperAdminSeedOptions _superAdmin = superAdminOptions.Value;

    public async Task SeedAsync(CancellationToken cancellationToken = default)
    {
        await SeedRolesAsync(cancellationToken);
        await SeedScopesAsync(cancellationToken);
        await EnsureMarketingRoleScopesAsync(cancellationToken);
        await EnsureSellerVisibilityRoleScopesAsync(cancellationToken);
        await AssignAllScopesToSuperUserAsync(cancellationToken);
        await EnsurePricingWorkspaceScopeAsync(cancellationToken);
        await SeedSuperAdminAsync(cancellationToken);
    }

    private async Task SeedRolesAsync(CancellationToken cancellationToken)
    {
        if (!await dbContext.Roles.AnyAsync(x => x.Name == AuthConstants.SystemRoles.Administrator, cancellationToken))
        {
            await dbContext.Roles.AddAsync(Role.Create(AuthConstants.SystemRoles.Administrator, "Rol administrador del sistema.", true, null), cancellationToken);
        }

        if (!await dbContext.Roles.AnyAsync(x => x.Name == AuthConstants.SystemRoles.SuperUser, cancellationToken))
        {
            await dbContext.Roles.AddAsync(Role.Create(AuthConstants.SystemRoles.SuperUser, "Rol superusuario con todos los permisos activos.", true, null), cancellationToken);
        }

        if (!await dbContext.Roles.AnyAsync(x => x.Name == AuthConstants.SystemRoles.Pricing, cancellationToken))
        {
            await dbContext.Roles.AddAsync(Role.Create(AuthConstants.SystemRoles.Pricing, "Rol operativo base de Pricing. Las vistas y acciones adicionales se habilitan por scope.", true, null), cancellationToken);
        }

        if (!await dbContext.Roles.AnyAsync(x => x.Name == AuthConstants.SystemRoles.Marketing, cancellationToken))
        {
            await dbContext.Roles.AddAsync(Role.Create(AuthConstants.SystemRoles.Marketing, "Rol de Mercadeo para administrar el CMS mediante scopes cms.*.", true, null), cancellationToken);
        }

        if (!await dbContext.Roles.AnyAsync(x => x.Name == SellerSupervisorRole, cancellationToken))
        {
            await dbContext.Roles.AddAsync(Role.Create(SellerSupervisorRole, "Vendedor que puede consultar sus solicitudes y tarifas, más las de vendedores que le sean asignados explícitamente.", true, null), cancellationToken);
        }

        if (!await dbContext.Roles.AnyAsync(x => x.Name == SellerChiefRole, cancellationToken))
        {
            await dbContext.Roles.AddAsync(Role.Create(SellerChiefRole, "Vendedor jefe con visibilidad de las solicitudes y tarifas de todos los vendedores.", true, null), cancellationToken);
        }

        await dbContext.SaveChangesAsync(cancellationToken);
    }

    private async Task SeedScopesAsync(CancellationToken cancellationToken)
    {
        var definitions = AuthScopeCatalog.Scopes
            .GroupBy(x => x.Code.Trim().ToLowerInvariant(), StringComparer.OrdinalIgnoreCase)
            .Select(x => x.First())
            .ToDictionary(x => x.Code.Trim().ToLowerInvariant(), StringComparer.OrdinalIgnoreCase);

        var existingScopes = await dbContext.Scopes.ToListAsync(cancellationToken);
        var affectedUserIds = new HashSet<Guid>();

        foreach (var scope in existingScopes)
        {
            if (definitions.TryGetValue(scope.Code, out var definition))
            {
                scope.UpdateDefinition(definition.Name, definition.Description);
            }
        }

        var existingCodes = existingScopes.Select(x => x.Code).ToHashSet(StringComparer.OrdinalIgnoreCase);
        foreach (var definition in definitions.Values)
        {
            if (!existingCodes.Contains(definition.Code))
            {
                await dbContext.Scopes.AddAsync(Scope.Create(definition.Code, definition.Name, definition.Description), cancellationToken);
            }
        }

        var obsoleteScopeIds = existingScopes.Where(x => !definitions.ContainsKey(x.Code)).Select(x => x.Id).ToArray();
        if (obsoleteScopeIds.Length > 0)
        {
            affectedUserIds.UnionWith(await dbContext.UserScopes.Where(x => obsoleteScopeIds.Contains(x.ScopeId)).Select(x => x.UserId).Distinct().ToListAsync(cancellationToken));
            var affectedRoleIds = await dbContext.RoleScopes.Where(x => obsoleteScopeIds.Contains(x.ScopeId)).Select(x => x.RoleId).Distinct().ToListAsync(cancellationToken);
            if (affectedRoleIds.Count > 0)
            {
                affectedUserIds.UnionWith(await dbContext.UserRoles.Where(x => affectedRoleIds.Contains(x.RoleId)).Select(x => x.UserId).Distinct().ToListAsync(cancellationToken));
            }

            await dbContext.UserScopes.Where(x => obsoleteScopeIds.Contains(x.ScopeId)).ExecuteDeleteAsync(cancellationToken);
            await dbContext.RoleScopes.Where(x => obsoleteScopeIds.Contains(x.ScopeId)).ExecuteDeleteAsync(cancellationToken);
            await dbContext.Scopes.Where(x => obsoleteScopeIds.Contains(x.Id)).ExecuteDeleteAsync(cancellationToken);
        }

        await dbContext.SaveChangesAsync(cancellationToken);
        foreach (var userId in affectedUserIds)
        {
            await permissionCache.RemoveAsync(userId, cancellationToken);
        }
    }

    private async Task EnsureMarketingRoleScopesAsync(CancellationToken cancellationToken)
    {
        var marketingRole = await dbContext.Roles.Include(x => x.Scopes)
            .FirstOrDefaultAsync(x => x.Name == AuthConstants.SystemRoles.Marketing, cancellationToken);
        if (marketingRole is null) return;

        var cmsScopeIds = await dbContext.Scopes
            .Where(x => x.IsActive && x.Code.StartsWith(CmsScopePrefix))
            .Select(x => x.Id)
            .ToListAsync(cancellationToken);

        foreach (var scopeId in cmsScopeIds)
        {
            marketingRole.AssignScope(scopeId, assignedBy: null);
        }

        await dbContext.SaveChangesAsync(cancellationToken);

        var userIds = await dbContext.UserRoles.Where(x => x.RoleId == marketingRole.Id)
            .Select(x => x.UserId).Distinct().ToListAsync(cancellationToken);
        foreach (var userId in userIds)
        {
            await permissionCache.RemoveAsync(userId, cancellationToken);
        }
    }

    private async Task EnsureSellerVisibilityRoleScopesAsync(CancellationToken cancellationToken)
    {
        var scopeIds = await dbContext.Scopes
            .Where(x => x.IsActive && new[] { RateRequestCreateScope, RateRequestViewSelectedScope, RateRequestViewAllScope }.Contains(x.Code))
            .ToDictionaryAsync(x => x.Code, x => x.Id, cancellationToken);

        var roleScopes = new Dictionary<string, string[]>(StringComparer.OrdinalIgnoreCase)
        {
            [SellerSupervisorRole] = [RateRequestCreateScope, RateRequestViewSelectedScope],
            [SellerChiefRole] = [RateRequestCreateScope, RateRequestViewAllScope],
        };

        foreach (var definition in roleScopes)
        {
            var role = await dbContext.Roles.Include(x => x.Scopes).FirstOrDefaultAsync(x => x.Name == definition.Key, cancellationToken);
            if (role is null) continue;

            foreach (var scopeCode in definition.Value)
            {
                if (scopeIds.TryGetValue(scopeCode, out var scopeId)) role.AssignScope(scopeId, assignedBy: null);
            }

            var userIds = await dbContext.UserRoles.Where(x => x.RoleId == role.Id).Select(x => x.UserId).Distinct().ToListAsync(cancellationToken);
            foreach (var userId in userIds) await permissionCache.RemoveAsync(userId, cancellationToken);
        }

        await dbContext.SaveChangesAsync(cancellationToken);
    }

    private async Task AssignAllScopesToSuperUserAsync(CancellationToken cancellationToken)
    {
        var superUserRole = await dbContext.Roles.Include(x => x.Scopes)
            .FirstOrDefaultAsync(x => x.Name == AuthConstants.SystemRoles.SuperUser, cancellationToken);
        if (superUserRole is null) return;

        var activeScopeIds = await dbContext.Scopes.Where(x => x.IsActive).Select(x => x.Id).ToListAsync(cancellationToken);
        foreach (var scopeId in activeScopeIds) superUserRole.AssignScope(scopeId, assignedBy: null);
        await dbContext.SaveChangesAsync(cancellationToken);
    }

    private async Task EnsurePricingWorkspaceScopeAsync(CancellationToken cancellationToken)
    {
        var pricingRole = await dbContext.Roles.Include(x => x.Scopes)
            .FirstOrDefaultAsync(x => x.Name == AuthConstants.SystemRoles.Pricing, cancellationToken);
        if (pricingRole is null) return;

        var workspaceScopeId = await dbContext.Scopes.Where(x => x.IsActive && x.Code == PricingWorkspaceScope)
            .Select(x => (Guid?)x.Id).FirstOrDefaultAsync(cancellationToken);
        if (workspaceScopeId is null) return;

        pricingRole.AssignScope(workspaceScopeId.Value, assignedBy: null);
        await dbContext.SaveChangesAsync(cancellationToken);

        var pricingUserIds = await dbContext.UserRoles.Where(x => x.RoleId == pricingRole.Id)
            .Select(x => x.UserId).Distinct().ToListAsync(cancellationToken);
        foreach (var userId in pricingUserIds) await permissionCache.RemoveAsync(userId, cancellationToken);
    }

    private async Task SeedSuperAdminAsync(CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(_superAdmin.Email) || string.IsNullOrWhiteSpace(_superAdmin.UserName) || string.IsNullOrWhiteSpace(_superAdmin.Password)) return;

        var email = _superAdmin.Email.Trim().ToLowerInvariant();
        var userName = _superAdmin.UserName.Trim();
        var displayName = string.IsNullOrWhiteSpace(_superAdmin.DisplayName) ? userName : _superAdmin.DisplayName.Trim();

        var superUserRole = await dbContext.Roles.FirstOrDefaultAsync(x => x.Name == AuthConstants.SystemRoles.SuperUser, cancellationToken);
        if (superUserRole is null) return;

        var existingUser = await dbContext.Users.Include(x => x.Roles)
            .FirstOrDefaultAsync(x => x.Email == email || x.UserName == userName, cancellationToken);
        if (existingUser is not null)
        {
            existingUser.AssignRole(superUserRole.Id, assignedBy: null);
            await dbContext.SaveChangesAsync(cancellationToken);
            await permissionCache.RemoveAsync(existingUser.Id, cancellationToken);
            return;
        }

        var user = User.Create(userName, email, displayName, UserType.Internal, passwordHasher.Hash(_superAdmin.Password), createdBy: null);
        user.AssignRole(superUserRole.Id, assignedBy: null);
        await dbContext.Users.AddAsync(user, cancellationToken);
        await dbContext.SaveChangesAsync(cancellationToken);
        await permissionCache.RemoveAsync(user.Id, cancellationToken);
    }
}
