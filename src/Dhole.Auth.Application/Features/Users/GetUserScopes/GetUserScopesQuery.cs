using CustomCodeFramework.Cqrs.Queries;
using Dhole.Auth.Contracts.Users;

namespace Dhole.Auth.Application.Users.GetUserScopes;

public sealed record GetUserScopesQuery(Guid UserId) : IQuery<IReadOnlyCollection<UserScopeDto>>;
