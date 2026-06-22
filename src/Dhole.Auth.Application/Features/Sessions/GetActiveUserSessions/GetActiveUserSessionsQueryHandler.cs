using CustomCodeFramework.Cqrs.Queries;
using Dhole.Auth.Application.Abstractions.Repositories;
using Dhole.Auth.Contracts.Sessions;

namespace Dhole.Auth.Application.Feature.Sessions.GetActiveUserSessions;

public sealed class GetActiveUserSessionsQueryHandler(ISessionRepository sessions)
    : IQueryHandler<GetActiveUserSessionsQuery, IReadOnlyCollection<SessionDto>>
{
    public Task<IReadOnlyCollection<SessionDto>> HandleAsync(
        GetActiveUserSessionsQuery query,
        CancellationToken cancellationToken = default
    )
    {
        return sessions.GetActiveByUserAsync(query.UserId, cancellationToken);
    }
}
