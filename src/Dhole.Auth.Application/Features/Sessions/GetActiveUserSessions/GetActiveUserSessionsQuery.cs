using CustomCodeFramework.Cqrs.Queries;
using Dhole.Auth.Contracts.Sessions;

namespace Dhole.Auth.Application.Feature.Sessions.GetActiveUserSessions;

public sealed record GetActiveUserSessionsQuery(Guid UserId)
    : IQuery<IReadOnlyCollection<SessionDto>>;
