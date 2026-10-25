using CustomCodeFramework.Core.Pagination;
using CustomCodeFramework.Postgres.EntityFramework.Repositories;
using Dhole.Auth.Application.Abstractions.Repositories;
using Dhole.Auth.Contracts.Roles;
using Dhole.Auth.Domain.Roles.Entities;
using Dhole.Auth.Persistence.DbContexts;
using Microsoft.EntityFrameworkCore;

namespace Dhole.Auth.Persistence.Repositories;

public sealed class RoleRepository(ServiceDbContext dbContext)
    : EfRepository<Role, Guid>(dbContext),
        IRoleRepository
{
    public Task<Role?> GetByNameAsync(string name, CancellationToken cancellationToken = default)
    {
        var value = name.Trim();

        return dbContext
            .Roles.Include(x => x.Scopes)
            .FirstOrDefaultAsync(x => x.Name == value && !x.IsDeleted, cancellationToken);
    }

    public Task<Role?> GetWithScopesAsync(
        Guid roleId,
        CancellationToken cancellationToken = default
    )
    {
        return dbContext
            .Roles.Include(x => x.Scopes)
            .FirstOrDefaultAsync(x => x.Id == roleId && !x.IsDeleted, cancellationToken);
    }

    public Task<bool> ExistsByNameAsync(string name, CancellationToken cancellationToken = default)
    {
        var value = name.Trim();

        return dbContext.Roles.AnyAsync(x => x.Name == value && !x.IsDeleted, cancellationToken);
    }

    public async Task<PagedResult<RoleDto>> GetPagedAsync(
        PageRequest page,
        string? search = null,
        bool? isActive = null,
        CancellationToken cancellationToken = default
    )
    {
        var query = dbContext.Roles.AsNoTracking().Where(x => !x.IsDeleted);

        if (!string.IsNullOrWhiteSpace(search))
        {
            var value = search.Trim().ToLower();

            query = query.Where(x =>
                x.Name.ToLower().Contains(value)
                || (x.Description != null && x.Description.ToLower().Contains(value))
            );
        }

        if (isActive.HasValue)
        {
            query = query.Where(x => x.IsActive == isActive.Value);
        }

        var total = await query.CountAsync(cancellationToken);

        var items = await query
            .OrderBy(x => x.Name)
            .Skip(page.Skip)
            .Take(page.PageSize)
            .Select(x => new RoleDto(x.Id, x.Name, x.Description, x.IsSystemRole, x.IsActive))
            .ToListAsync(cancellationToken);

        return PagedResult<RoleDto>.Create(items, page.PageNumber, page.PageSize, total);
    }

    public async Task<IReadOnlyCollection<RoleSelectDto>> GetForSelectAsync(
        CancellationToken cancellationToken = default
    )
    {
        return await dbContext
            .Roles.AsNoTracking()
            .Where(x => x.IsActive && !x.IsDeleted)
            .OrderBy(x => x.Name)
            .Select(x => new RoleSelectDto(x.Id, x.Name))
            .ToListAsync(cancellationToken);
    }

    public async Task<IReadOnlyCollection<RoleScopeDto>> GetRoleScopesAsync(
        Guid roleId,
        CancellationToken cancellationToken = default
    )
    {
        return await (
            from roleScope in dbContext.RoleScopes.AsNoTracking()
            join role in dbContext.Roles.AsNoTracking() on roleScope.RoleId equals role.Id
            join scope in dbContext.Scopes.AsNoTracking() on roleScope.ScopeId equals scope.Id
            where roleScope.RoleId == roleId && !role.IsDeleted
            orderby scope.Code
            select new RoleScopeDto(roleScope.RoleId, scope.Id, scope.Code, scope.Name)
        ).ToListAsync(cancellationToken);
    }
}
