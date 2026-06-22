using Dhole.Auth.Application.Abstractions.Authentication;
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
    IOptions<SuperAdminSeedOptions> superAdminOptions
)
{
    private readonly SuperAdminSeedOptions _superAdmin = superAdminOptions.Value;

    public async Task SeedAsync(CancellationToken cancellationToken = default)
    {
        await SeedRolesAsync(cancellationToken);
        await SeedScopesAsync(cancellationToken);
        await AssignAllScopesToSuperUserAsync(cancellationToken);
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

        await dbContext.SaveChangesAsync(cancellationToken);
    }

    private async Task SeedScopesAsync(CancellationToken cancellationToken)
    {
        foreach (var scopeDefinition in AuthScopeCatalog.Scopes)
        {
            var code = scopeDefinition.Code.Trim().ToLowerInvariant();

            var exists = await dbContext.Scopes.AnyAsync(x => x.Code == code, cancellationToken);

            if (exists)
            {
                continue;
            }

            var scope = Scope.Create(
                scopeDefinition.Code,
                scopeDefinition.Name,
                scopeDefinition.Description
            );

            await dbContext.Scopes.AddAsync(scope, cancellationToken);
        }

        await dbContext.SaveChangesAsync(cancellationToken);
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
