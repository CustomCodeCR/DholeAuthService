using CustomCodeFramework.Cqrs.Queries;
using Dhole.Auth.Application.Abstractions.Repositories;
using Dhole.Auth.Contracts.Scopes;

namespace Dhole.Auth.Application.Feature.Scopes.GetScopesForSelect;

public sealed class GetScopesForSelectQueryHandler(IScopeRepository scopes)
    : IQueryHandler<GetScopesForSelectQuery, IReadOnlyCollection<ScopeSelectDto>>
{
    public Task<IReadOnlyCollection<ScopeSelectDto>> HandleAsync(
        GetScopesForSelectQuery query,
        CancellationToken cancellationToken = default
    )
    {
        return scopes.GetForSelectAsync(cancellationToken);
    }
}
