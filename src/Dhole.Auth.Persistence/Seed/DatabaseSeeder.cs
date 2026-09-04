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
    private readonly SuperAdminSeedOptions _superAdmin = superAdminOptions.Value;

    public async Task SeedAsync(CancellationToken cancellationToken = default)
    {
        await SeedRolesAsync(cancellationToken);
        await SeedScopesAsync(cancellationToken);
        await AssignAllScopesToSuperUserAsync(cancellationToken);
        await EnsurePricingWorkspaceScopeAsync(cancellationToken);
        await SeedSuperAdminAsync(cancellationToken);
    }

    private async Task SeedRolesAsync(CancellationToken cancellationToken)
    {
        if (
            !await dbContext.Roles.AnyAsync(
                x => x.Name == AuthConstants.SystemRoles.Administrator,
                cancellationToken
            )
        )
        {
            var administrator = Role.Create(
                AuthConstants.SystemRoles.Administrator,
                "Rol administrador del sistema.",
                isSystemRole: true,
                createdBy: null
            );

            await dbContext.Roles.AddAsync(administrator, cancellationToken);
        }

        if (
            !await dbContext.Roles.AnyAsync(
                x => x.Name == AuthConstants.SystemRoles.SuperUser,
                cancellationToken
            )
        )
        {
            var superUser = Role.Create(
                AuthConstants.SystemRoles.SuperUser,
                "Rol superusuario con todos los permisos activos.",
                isSystemRole: true,
                createdBy: null
            );

            await dbContext.Roles.AddAsync(superUser, cancellationToken);
        }

        if (
            !await dbContext.Roles.AnyAsync(
                x => x.Name == AuthConstants.SystemRoles.Pricing,
                cancellationToken
            )
        )
        {
            var pricing = Role.Create(
                AuthConstants.SystemRoles.Pricing,
                "Rol operativo base de Pricing. Las vistas y acciones adicionales se habilitan por scope.",
                isSystemRole: true,
                createdBy: null
            );

            await dbContext.Roles.AddAsync(pricing, cancellationToken);
        }

        await dbContext.SaveChangesAsync(cancellationToken);
    }

    private async Task SeedScopesAsync(CancellationToken cancellationToken)
    {
        var definitions = AuthScopeCatalog.Scopes
            .GroupBy(x => x.Code.Trim().ToLowerInvariant(), StringComparer.OrdinalIgnoreCase)
            .Select(x => x.First())
            .ToDictionary(
                x => x.Code.Trim().ToLowerInvariant(),
                StringComparer.OrdinalIgnoreCase
            );

        var existingScopes = await dbContext.Scopes.ToListAsync(cancellationToken);
        var affectedUserIds = new HashSet<Guid>();

        foreach (var scope in existingScopes)
        {
            if (!definitions.TryGetValue(scope.Code, out var definition))
            {
                continue;
            }

            scope.UpdateDefinition(definition.Name, definition.Description);
        }

        var existingCodes = existingScopes
            .Select(x => x.Code)
            .ToHashSet(StringComparer.OrdinalIgnoreCase);

        foreach (var definition in definitions.Values)
        {
            if (existingCodes.Contains(definition.Code))
            {
                continue;
            }

            await dbContext.Scopes.AddAsync(
                Scope.Create(definition.Code, definition.Name, definition.Description),
                cancellationToken
            );
        }

        var obsoleteScopeIds = existingScopes
            .Where(x => !definitions.ContainsKey(x.Code))
            .Select(x => x.Id)
            .ToArray();

        if (obsoleteScopeIds.Length > 0)
        {
            var directUsers = await dbContext.UserScopes
                .Where(x => obsoleteScopeIds.Contains(x.ScopeId))
                .Select(x => x.UserId)
                .Distinct()
                .ToListAsync(cancellationToken);
            affectedUserIds.UnionWith(directUsers);

            var affectedRoleIds = await dbContext.RoleScopes
                .Where(x => obsoleteScopeIds.Contains(x.ScopeId))
                .Select(x => x.RoleId)
                .Distinct()
                .ToListAsync(cancellationToken);

            if (affectedRoleIds.Count > 0)
            {
                var roleUsers = await dbContext.UserRoles
                    .Where(x => affectedRoleIds.Contains(x.RoleId))
                    .Select(x => x.UserId)
                    .Distinct()
                    .ToListAsync(cancellationToken);
                affectedUserIds.UnionWith(roleUsers);
            }

            await dbContext.UserScopes
                .Where(x => obsoleteScopeIds.Contains(x.ScopeId))
                .ExecuteDeleteAsync(cancellationToken);
            await dbContext.RoleScopes
                .Where(x => obsoleteScopeIds.Contains(x.ScopeId))
                .ExecuteDeleteAsync(cancellationToken);
            await dbContext.Scopes
                .Where(x => obsoleteScopeIds.Contains(x.Id))
                .ExecuteDeleteAsync(cancellationToken);
        }

        await dbContext.SaveChangesAsync(cancellationToken);

        foreach (var userId in affectedUserIds)
        {
            await permissionCache.RemoveAsync(userId, cancellationToken);
        }
    }

    private async Task AssignAllScopesToSuperUserAsync(CancellationToken cancellationToken)
    {
        var superUserRole = await dbContext
            .Roles.Include(x => x.Scopes)
            .FirstOrDefaultAsync(
                x => x.Name == AuthConstants.SystemRoles.SuperUser,
                cancellationToken
            );

        if (superUserRole is null)
        {
            return;
        }

        var activeScopeIds = await dbContext
            .Scopes.Where(x => x.IsActive)
            .Select(x => x.Id)
            .ToListAsync(cancellationToken);

        foreach (var scopeId in activeScopeIds)
        {
            superUserRole.AssignScope(scopeId, assignedBy: null);
        }

        await dbContext.SaveChangesAsync(cancellationToken);
    }

    private async Task EnsurePricingWorkspaceScopeAsync(CancellationToken cancellationToken)
    {
        var pricingRole = await dbContext
            .Roles.Include(x => x.Scopes)
            .FirstOrDefaultAsync(
                x => x.Name == AuthConstants.SystemRoles.Pricing,
                cancellationToken
            );

        if (pricingRole is null)
        {
            return;
        }

        var workspaceScopeId = await dbContext
            .Scopes.Where(x => x.IsActive && x.Code == PricingWorkspaceScope)
            .Select(x => (Guid?)x.Id)
            .FirstOrDefaultAsync(cancellationToken);

        if (workspaceScopeId is null)
        {
            return;
        }

        pricingRole.AssignScope(workspaceScopeId.Value, assignedBy: null);
        await dbContext.SaveChangesAsync(cancellationToken);

        // Permissions beyond the base workspace are intentionally NOT assigned here.
        // Inbox, import review, logistics news, rates, costs and terms remain scope-gated.
        var pricingUserIds = await dbContext
            .UserRoles.Where(x => x.RoleId == pricingRole.Id)
            .Select(x => x.UserId)
            .Distinct()
            .ToListAsync(cancellationToken);

        foreach (var userId in pricingUserIds)
        {
            await permissionCache.RemoveAsync(userId, cancellationToken);
        }
    }

    private async Task SeedSuperAdminAsync(CancellationToken cancellationToken)
    {
        if (
            string.IsNullOrWhiteSpace(_superAdmin.Email)
            || string.IsNullOrWhiteSpace(_superAdmin.UserName)
            || string.IsNullOrWhiteSpace(_superAdmin.Password)
        )
        {
            return;
        }

        var email = _superAdmin.Email.Trim().ToLowerInvariant();
        var userName = _superAdmin.UserName.Trim();
        var displayName = string.IsNullOrWhiteSpace(_superAdmin.DisplayName)
            ? userName
            : _superAdmin.DisplayName.Trim();

        var exists = await dbContext.Users.AnyAsync(
            x => x.Email == email || x.UserName == userName,
            cancellationToken
        );

        if (exists)
        {
            return;
        }

        var superUserRole = await dbContext.Roles.FirstOrDefaultAsync(
            x => x.Name == AuthConstants.SystemRoles.SuperUser,
            cancellationToken
        );

        if (superUserRole is null)
        {
            return;
        }

        var passwordHash = passwordHasher.Hash(_superAdmin.Password);

        var user = User.Create(
            userName,
            email,
            displayName,
            UserType.Internal,
            passwordHash,
            createdBy: null
        );

        user.AssignRole(superUserRole.Id, assignedBy: null);

        await dbContext.Users.AddAsync(user, cancellationToken);
        await dbContext.SaveChangesAsync(cancellationToken);
    }
}
