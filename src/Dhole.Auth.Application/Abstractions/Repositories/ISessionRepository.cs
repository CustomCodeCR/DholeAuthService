using CustomCodeFramework.Core.Pagination;
using CustomCodeFramework.Persistence.Abstractions;
using Dhole.Auth.Contracts.Sessions;
using Dhole.Auth.Domain.Sessions.Entities;

namespace Dhole.Auth.Application.Abstractions.Repositories;

public interface ISessionRepository : IRepository<Session, Guid>
{
    Task<Session?> GetByRefreshTokenHashAsync(
        string refreshTokenHash,
        CancellationToken cancellationToken = default
    );

    Task<PagedResult<SessionDto>> GetPagedByUserAsync(
        Guid userId,
        PageRequest page,
        CancellationToken cancellationToken = default
    );

    Task<IReadOnlyCollection<SessionDto>> GetActiveByUserAsync(
        Guid userId,
        CancellationToken cancellationToken = default
    );

    Task<IReadOnlyCollection<Session>> GetActiveEntitiesByUserAsync(
        Guid userId,
        CancellationToken cancellationToken = default
    );
}
