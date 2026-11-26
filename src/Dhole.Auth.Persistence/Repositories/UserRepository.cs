using CustomCodeFramework.Core.Pagination;
using CustomCodeFramework.Postgres.EntityFramework.Repositories;
using Dhole.Auth.Application.Abstractions.Repositories;
using Dhole.Auth.Application.Abstractions.Permissions;
using Dhole.Auth.Application.Permissions;
using Dhole.Auth.Application.Users;
using Dhole.Auth.Contracts.Users;
using Dhole.Auth.Domain.Users.Entities;
using Dhole.Auth.Persistence.DbContexts;
using Microsoft.EntityFrameworkCore;

namespace Dhole.Auth.Persistence.Repositories;

public sealed class UserRepository(ServiceDbContext dbContext, IEffectivePermissionService? permissionService = null)
    : EfRepository<User, Guid>(dbContext),
        IUserRepository
{
    public Task<User?> GetByUserNameAsync(
        string userName,
        CancellationToken cancellationToken = default
    )
    {
        var value = userName.Trim();

        return dbContext.Users.FirstOrDefaultAsync(
            x => x.UserName == value && !x.IsDeleted,
            cancellationToken
        );
    }

    public Task<User?> GetByEmailAsync(string email, CancellationToken cancellationToken = default)
    {
        var value = email.Trim();

        return dbContext.Users.FirstOrDefaultAsync(
            x => x.Email == value && !x.IsDeleted,
            cancellationToken
        );
    }

    public Task<User?> GetWithRolesAsync(Guid userId, CancellationToken cancellationToken = default)
    {
        return dbContext
            .Users.Include(x => x.Roles)
            .FirstOrDefaultAsync(x => x.Id == userId && !x.IsDeleted, cancellationToken);
    }

    public Task<User?> GetWithScopesAsync(
        Guid userId,
        CancellationToken cancellationToken = default
    )
    {
        return dbContext
            .Users.Include(x => x.Scopes)
            .FirstOrDefaultAsync(x => x.Id == userId && !x.IsDeleted, cancellationToken);
    }

    public Task<User?> GetWithRolesAndScopesAsync(
        Guid userId,
        CancellationToken cancellationToken = default
    )
    {
        return dbContext
            .Users.Include(x => x.Roles)
            .Include(x => x.Scopes)
            .FirstOrDefaultAsync(x => x.Id == userId && !x.IsDeleted, cancellationToken);
    }

    public Task<bool> ExistsByUserNameAsync(
        string userName,
        CancellationToken cancellationToken = default
    )
    {
        var value = userName.Trim();

        return dbContext.Users.AnyAsync(
            x => x.UserName == value && !x.IsDeleted,
            cancellationToken
        );
    }

    public Task<bool> ExistsByEmailAsync(
        string email,
        CancellationToken cancellationToken = default
    )
    {
        var value = email.Trim();

        return dbContext.Users.AnyAsync(x => x.Email == value && !x.IsDeleted, cancellationToken);
    }

    public async Task<PagedResult<UserDto>> GetPagedAsync(
        PageRequest page,
        string? search = null,
        bool? isActive = null,
        bool? isLocked = null,
        CancellationToken cancellationToken = default
    )
    {
        var query = dbContext.Users.AsNoTracking().Where(x => !x.IsDeleted);

        if (!string.IsNullOrWhiteSpace(search))
        {
            var value = search.Trim().ToLower();

            query = query.Where(x =>
                x.UserName.ToLower().Contains(value)
                || x.Email.ToLower().Contains(value)
                || x.DisplayName.ToLower().Contains(value)
            );
        }

        if (isActive.HasValue)
        {
            query = query.Where(x => x.IsActive == isActive.Value);
        }

        if (isLocked.HasValue)
        {
            query = query.Where(x => x.IsLocked == isLocked.Value);
        }

        var total = await query.CountAsync(cancellationToken);

        var rows = await query
            .OrderBy(x => x.DisplayName)
            .Skip(page.Skip)
            .Take(page.PageSize)
            .Select(x => new
            {
                x.Id,
                x.UserName,
                x.Email,
                x.DisplayName,
                x.UserType,
                x.IsActive,
                x.IsLocked,
                x.LastLoginAt,
            })
            .ToListAsync(cancellationToken);

        var items = rows
            .Select(x => new UserDto(
                x.Id,
                x.UserName,
                x.Email,
                x.DisplayName,
                x.UserType,
                x.UserType.ToString(),
                x.IsActive,
                x.IsLocked,
                x.LastLoginAt,
                ProtectedSeedUserGuard.IsProtected(x.Email)
            ))
            .ToList();

        return PagedResult<UserDto>.Create(items, page.PageNumber, page.PageSize, total);
    }

    public async Task<IReadOnlyCollection<UserRoleDto>> GetUserRolesAsync(
        Guid userId,
        CancellationToken cancellationToken = default
    )
    {
        return await (
            from userRole in dbContext.UserRoles.AsNoTracking()
            join user in dbContext.Users.AsNoTracking() on userRole.UserId equals user.Id
            join role in dbContext.Roles.AsNoTracking() on userRole.RoleId equals role.Id
            where userRole.UserId == userId && !user.IsDeleted && !role.IsDeleted
            orderby role.Name
            select new UserRoleDto(userRole.UserId, role.Id, role.Name)
        ).ToListAsync(cancellationToken);
    }

    public async Task<IReadOnlyCollection<UserScopeDto>> GetUserScopesAsync(
        Guid userId,
        CancellationToken cancellationToken = default
    )
    {
        return await (
            from userScope in dbContext.UserScopes.AsNoTracking()
            join user in dbContext.Users.AsNoTracking() on userScope.UserId equals user.Id
            join scope in dbContext.Scopes.AsNoTracking() on userScope.ScopeId equals scope.Id
            where userScope.UserId == userId && !user.IsDeleted
            orderby scope.Code
            select new UserScopeDto(userScope.UserId, scope.Id, scope.Code, scope.Name)
        ).ToListAsync(cancellationToken);
    }

    public async Task<UserPermissionsDto> GetUserPermissionsAsync(
        Guid userId,
        CancellationToken cancellationToken = default
    )
    {
        var roles = await GetUserRolesAsync(userId, cancellationToken);

        var directScopes = await GetUserScopesAsync(userId, cancellationToken);

        // Use the same authoritative permissions as JWT issuance and Auth's
        // request principal, including automatic team grants. Direct grants
        // remain separately editable in the existing scopes tab.
        var service = permissionService ?? new EffectivePermissionService(new AuthPermissionReadRepository(dbContext));
        var permissions = await service.GetAsync(userId, cancellationToken);
        var effectiveCodes = permissions.EffectiveScopes.ToArray();
        var effectiveScopes = await dbContext.Scopes.AsNoTracking()
            .Where(scope => scope.IsActive && effectiveCodes.Contains(scope.Code))
            .OrderBy(scope => scope.Code)
            .Select(scope => new UserScopeDto(userId, scope.Id, scope.Code, scope.Name))
            .ToListAsync(cancellationToken);

        return new UserPermissionsDto(userId, roles, directScopes, effectiveScopes);
    }
}
