using CustomCodeFramework.Core.Pagination;
using CustomCodeFramework.Cqrs.Queries;
using Dhole.Auth.Application.Abstractions.Repositories;
using Dhole.Auth.Contracts.Sessions;

namespace Dhole.Auth.Application.Feature.Sessions.GetUserSessions;

public sealed class GetUserSessionsQueryHandler(ISessionRepository sessions)
    : IQueryHandler<GetUserSessionsQuery, PagedResult<SessionDto>>
{
    public Task<PagedResult<SessionDto>> HandleAsync(
        GetUserSessionsQuery query,
        CancellationToken cancellationToken = default
    )
    {
        return sessions.GetPagedByUserAsync(query.UserId, query.Page, cancellationToken);
    }
}
