using CustomCodeFramework.Cqrs.Queries;
using Dhole.Auth.Contracts.Scopes;

namespace Dhole.Auth.Application.Feature.Scopes.GetScopesForSelect;

public sealed record GetScopesForSelectQuery : IQuery<IReadOnlyCollection<ScopeSelectDto>>;
