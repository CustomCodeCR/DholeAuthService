using CustomCodeFramework.Core.Pagination;
using CustomCodeFramework.Cqrs.Queries;
using Dhole.Auth.Contracts.Sessions;

namespace Dhole.Auth.Application.Feature.Sessions.GetUserSessions;

public sealed record GetUserSessionsQuery(Guid UserId, PageRequest Page)
    : IQuery<PagedResult<SessionDto>>;
