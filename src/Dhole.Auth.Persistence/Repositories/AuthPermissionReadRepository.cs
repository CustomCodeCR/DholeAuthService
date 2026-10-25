using Dhole.Auth.Application.Abstractions.Permissions;
using Dhole.Auth.Domain.Roles.Entities;
using Dhole.Auth.Domain.Scopes.Entities;
using Dhole.Auth.Domain.Users.Entities;
using Dhole.Auth.Persistence.DbContexts;
using Microsoft.EntityFrameworkCore;

namespace Dhole.Auth.Persistence.Repositories;

public sealed class AuthPermissionReadRepository(ServiceDbContext dbContext)
    : IAuthPermissionReadRepository
{
    public async Task<UserPermissionData?> GetUserPermissionDataAsync(
        Guid userId,
        CancellationToken cancellationToken = default
    )
    {
        var user = await dbContext
            .Users.AsNoTracking()
            .FirstOrDefaultAsync(x => x.Id == userId, cancellationToken);

        if (user is null)
        {
            return null;
        }

        var activeRoles = await (
            from userRole in dbContext.Set<UserRole>().AsNoTracking()
            join role in dbContext.Set<Role>().AsNoTracking() on userRole.RoleId equals role.Id
            where userRole.UserId == userId && role.IsActive && !role.IsDeleted
            select role.Name
        ).ToListAsync(cancellationToken);

        var directScopes = await (
            from userScope in dbContext.Set<UserScope>().AsNoTracking()
            join scope in dbContext.Set<Scope>().AsNoTracking() on userScope.ScopeId equals scope.Id
            where userScope.UserId == userId && scope.IsActive
            select scope.Code
        ).ToListAsync(cancellationToken);

        var roleScopes = await (
            from userRole in dbContext.Set<UserRole>().AsNoTracking()
            join role in dbContext.Set<Role>().AsNoTracking() on userRole.RoleId equals role.Id
            join roleScope in dbContext.Set<RoleScope>().AsNoTracking()
                on role.Id equals roleScope.RoleId
            join scope in dbContext.Set<Scope>().AsNoTracking() on roleScope.ScopeId equals scope.Id
            where userRole.UserId == userId && role.IsActive && !role.IsDeleted && scope.IsActive
            select scope.Code
        ).ToListAsync(cancellationToken);

        return new UserPermissionData(
            user.Id,
            user.IsActive,
            user.IsLocked,
            activeRoles.Distinct().OrderBy(x => x).ToList(),
            directScopes.Distinct().OrderBy(x => x).ToList(),
            roleScopes.Distinct().OrderBy(x => x).ToList()
        );
    }

    public async Task<IReadOnlyCollection<string>> GetAllActiveScopeCodesAsync(
        CancellationToken cancellationToken = default
    )
    {
        return await dbContext
            .Scopes.AsNoTracking()
            .Where(x => x.IsActive)
            .OrderBy(x => x.Code)
            .Select(x => x.Code)
            .ToListAsync(cancellationToken);
    }
}
