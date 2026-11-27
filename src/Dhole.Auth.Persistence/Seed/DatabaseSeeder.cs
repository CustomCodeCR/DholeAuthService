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
    private const string CompetitorAnalysisScope = "pricing.competitor-analysis.manage";
    // Storage API uses its own file permissions. Grant only upload and content download
    // to Pricing; the analysis-specific scope still gates the UI and Pricing endpoints.
    private const string StorageFilesCreateScope = "storage.files.create";
    private const string StorageFilesDownloadScope = "storage.files.download";
    private static readonly string[] PricingAverageScopes =
    [
        "pricing.market-benchmark.view",
        "pricing.market-benchmark.calculate",
        "pricing.auto-pricing.view",
        "pricing.auto-pricing.calculate",
    ];
    private const string RateRequestCreateScope = "pricing.rate-request.create";
    private const string RateRequestViewSelectedScope = "pricing.rate-request.view-selected";
    private const string RateRequestViewAllScope = "pricing.rate-request.view-all";

    private readonly SuperAdminSeedOptions _superAdmin = superAdminOptions.Value;

    public async Task SeedAsync(CancellationToken cancellationToken = default)
    {
        await SeedRolesAsync(cancellationToken);
        await SeedScopesAsync(cancellationToken);
        await RetireUnrequestedAutoCreatedRolesAsync(cancellationToken);
        await EnsureAdministratorCredentialScopesAsync(cancellationToken);
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

        foreach (var name in OperationsRolePolicy.BusinessRoles)
        {
            if (!await dbContext.Roles.IgnoreQueryFilters().AnyAsync(r => r.Name == name, cancellationToken))
                await dbContext.Roles.AddAsync(Role.Create(name, "Rol de negocio de Operaciones.", isSystemRole: false), cancellationToken);
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

    /// <summary>
    /// Create narrowly scoped Dynamics roles. Role membership is NEVER automatic;
    /// administrators may assign these roles through DholeAuth.
    /// The standard SuperUsuario seed independently receives all active scopes.
    /// </summary>
    /// <summary>
    /// Retire only the exact seven formerly auto-created roles disallowed by the
    /// business owner. Keep all scopes and unrelated roles. User role memberships
    /// are removed so permissions and realtime caches cannot retain obsolete grants.
    /// Soft-deleting Role emits the existing immutable role-deleted audit Outbox event.
    /// </summary>
    private async Task RetireUnrequestedAutoCreatedRolesAsync(CancellationToken cancellationToken)
    {
        var obsoleteNames = OperationsRolePolicy.DeprecatedAutoCreated;
        var roles = await dbContext.Roles
            .Where(role => !role.IsDeleted && obsoleteNames.Contains(role.Name))
            .ToListAsync(cancellationToken);

        if (roles.Count == 0) return;

        var roleIds = roles.Select(role => role.Id).ToArray();
        var assignments = await dbContext.UserRoles
            .Where(assignment => roleIds.Contains(assignment.RoleId))
            .ToListAsync(cancellationToken);
        var affectedUsers = assignments.Select(assignment => assignment.UserId).Distinct().ToArray();

        dbContext.UserRoles.RemoveRange(assignments);
        foreach (var role in roles) role.Delete(deletedBy: null);
        await dbContext.SaveChangesAsync(cancellationToken);

        foreach (var userId in affectedUsers)
            await permissionCache.RemoveAsync(userId, cancellationToken);
    }

    private async Task EnsureAdministratorCredentialScopesAsync(CancellationToken cancellationToken)
    {
        var administratorRole = await dbContext.Roles
            .Include(x => x.Scopes)
            .FirstOrDefaultAsync(
                x => x.Name == AuthConstants.SystemRoles.Administrator,
                cancellationToken
            );

        if (administratorRole is null)
        {
            return;
        }

        var credentialScopeCodes = new[]
        {
            AuthConstants.Scopes.UserChangePassword,
            AuthConstants.Scopes.UserSendCredentials,
            AuthConstants.Scopes.UserImpersonate,
            AuthConstants.Scopes.UserView,
            "crm.lead.read",
            "crm.lead.read-all",
            "crm.lead.create",
            "crm.lead.update",
            "crm.lead.assign",
        };

        var credentialScopeIds = await dbContext.Scopes
            .Where(x => x.IsActive && credentialScopeCodes.Contains(x.Code))
            .Select(x => x.Id)
            .ToListAsync(cancellationToken);

        foreach (var scopeId in credentialScopeIds)
        {
            administratorRole.AssignScope(scopeId, assignedBy: null);
        }

        await dbContext.SaveChangesAsync(cancellationToken);

        var userIds = await dbContext.UserRoles
            .Where(x => x.RoleId == administratorRole.Id)
            .Select(x => x.UserId)
            .Distinct()
            .ToListAsync(cancellationToken);

        foreach (var userId in userIds)
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

        var superUserIds = await dbContext.UserRoles
            .Where(x => x.RoleId == superUserRole.Id)
            .Select(x => x.UserId)
            .Distinct()
            .ToListAsync(cancellationToken);

        foreach (var userId in superUserIds)
        {
            await permissionCache.RemoveAsync(userId, cancellationToken);
        }
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

        // Every member of the Pricing role may inspect and calculate market
        // suggestions. Applying, overriding and approving sales remain separately gated.
        var grantedCodes = PricingAverageScopes
            .Concat([PricingWorkspaceScope, CompetitorAnalysisScope, StorageFilesCreateScope, StorageFilesDownloadScope])
            .ToArray();
        var grantedScopeIds = await dbContext.Scopes
            .Where(x => x.IsActive && grantedCodes.Contains(x.Code))
            .Select(x => new { x.Code, x.Id })
            .ToListAsync(cancellationToken);

        if (grantedScopeIds.Count != grantedCodes.Length)
        {
            throw new InvalidOperationException(
                "Faltan scopes requeridos para el rol Pricing (Average, análisis de competencia o Storage).");
        }

        foreach (var scope in grantedScopeIds)
        {
            pricingRole.AssignScope(scope.Id, assignedBy: null);
        }
        await dbContext.SaveChangesAsync(cancellationToken);

        // Only the four Average read/calculate permissions are base permissions.
        // Cost, tariff mutation, margin approval, and Auto Pricing apply/override/approve
        // continue to require their own explicitly assigned scopes.
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

        var superUserRole = await dbContext.Roles.FirstOrDefaultAsync(
            x => x.Name == AuthConstants.SystemRoles.SuperUser,
            cancellationToken
        );

        if (superUserRole is null)
        {
            return;
        }

        var existingUser = await dbContext.Users
            .Include(x => x.Roles)
            .FirstOrDefaultAsync(
                x => x.Email == email || x.UserName == userName,
                cancellationToken
            );

        if (existingUser is not null)
        {
            existingUser.AssignRole(superUserRole.Id, assignedBy: null);
            await dbContext.SaveChangesAsync(cancellationToken);
            await permissionCache.RemoveAsync(existingUser.Id, cancellationToken);
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
        await permissionCache.RemoveAsync(user.Id, cancellationToken);
    }
}
