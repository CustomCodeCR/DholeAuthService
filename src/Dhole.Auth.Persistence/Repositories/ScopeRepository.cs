using CustomCodeFramework.Postgres.EntityFramework.Repositories;
using Dhole.Auth.Application.Abstractions.Repositories;
using Dhole.Auth.Contracts.Scopes;
using Dhole.Auth.Domain.Scopes.Entities;
using Dhole.Auth.Persistence.DbContexts;
using Microsoft.EntityFrameworkCore;

namespace Dhole.Auth.Persistence.Repositories;

public sealed class ScopeRepository(ServiceDbContext dbContext)
    : EfRepository<Scope, Guid>(dbContext),
        IScopeRepository
{
    public Task<Scope?> GetByCodeAsync(string code, CancellationToken cancellationToken = default)
    {
        var value = code.Trim().ToLowerInvariant();

        return dbContext.Scopes.FirstOrDefaultAsync(x => x.Code == value, cancellationToken);
    }

    public Task<bool> ExistsByCodeAsync(string code, CancellationToken cancellationToken = default)
    {
        var value = code.Trim().ToLowerInvariant();

        return dbContext.Scopes.AnyAsync(x => x.Code == value, cancellationToken);
    }

    public async Task<IReadOnlyCollection<ScopeSelectDto>> GetForSelectAsync(
        CancellationToken cancellationToken = default
    )
    {
        return await dbContext
            .Scopes.AsNoTracking()
            .Where(x => x.IsActive)
            .OrderBy(x => x.Code)
            .Select(x => new ScopeSelectDto(x.Id, x.Code, x.Name))
            .ToListAsync(cancellationToken);
    }
}
