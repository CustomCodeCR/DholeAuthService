using CustomCodeFramework.Core.Pagination;
using CustomCodeFramework.Postgres.EntityFramework.Repositories;
using Dhole.Auth.Application.Abstractions.Repositories;
using Dhole.Auth.Contracts.Sessions;
using Dhole.Auth.Domain.Sessions.Entities;
using Dhole.Auth.Persistence.DbContexts;
using Microsoft.EntityFrameworkCore;

namespace Dhole.Auth.Persistence.Repositories;

public sealed class SessionRepository(ServiceDbContext dbContext)
    : EfRepository<Session, Guid>(dbContext),
        ISessionRepository
{
    public Task<Session?> GetByRefreshTokenHashAsync(
        string refreshTokenHash,
        CancellationToken cancellationToken = default
    )
    {
        return dbContext.Sessions.FirstOrDefaultAsync(
            x => x.RefreshTokenHash == refreshTokenHash,
            cancellationToken
        );
    }

    public async Task<PagedResult<SessionDto>> GetPagedByUserAsync(
        Guid userId,
        PageRequest page,
        CancellationToken cancellationToken = default
    )
    {
        var query = dbContext.Sessions.AsNoTracking().Where(x => x.UserId == userId);

        var total = await query.CountAsync(cancellationToken);

        var items = await query
            .OrderByDescending(x => x.CreatedAt)
            .Skip(page.Skip)
            .Take(page.PageSize)
            .Select(x => new SessionDto(
                x.Id,
                x.UserId,
                x.IpAddress,
                x.UserAgent,
                x.CreatedAt,
                x.LastUsedAt,
                x.ExpiresAt,
                x.IsRevoked,
                x.RevokedAt,
                x.RevocationReason
            ))
            .ToListAsync(cancellationToken);

        return PagedResult<SessionDto>.Create(items, page.PageNumber, page.PageSize, total);
    }

    public async Task<IReadOnlyCollection<SessionDto>> GetActiveByUserAsync(
        Guid userId,
        CancellationToken cancellationToken = default
    )
    {
        var now = DateTime.UtcNow;

        return await dbContext
            .Sessions.AsNoTracking()
            .Where(x => x.UserId == userId && !x.IsRevoked && x.ExpiresAt > now)
            .OrderByDescending(x => x.LastUsedAt)
            .Select(x => new SessionDto(
                x.Id,
                x.UserId,
                x.IpAddress,
                x.UserAgent,
                x.CreatedAt,
                x.LastUsedAt,
                x.ExpiresAt,
                x.IsRevoked,
                x.RevokedAt,
                x.RevocationReason
            ))
            .ToListAsync(cancellationToken);
    }

    public async Task<IReadOnlyCollection<Session>> GetActiveEntitiesByUserAsync(
        Guid userId,
        CancellationToken cancellationToken = default
    )
    {
        var now = DateTime.UtcNow;

        return await dbContext
            .Sessions.Where(x => x.UserId == userId && !x.IsRevoked && x.ExpiresAt > now)
            .ToListAsync(cancellationToken);
    }
}
