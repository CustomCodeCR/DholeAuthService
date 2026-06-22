using CustomCodeFramework.Persistence.Abstractions;
using Dhole.Auth.Contracts.Scopes;
using Dhole.Auth.Domain.Scopes.Entities;

namespace Dhole.Auth.Application.Abstractions.Repositories;

public interface IScopeRepository : IRepository<Scope, Guid>
{
    Task<Scope?> GetByCodeAsync(string code, CancellationToken cancellationToken = default);

    Task<bool> ExistsByCodeAsync(string code, CancellationToken cancellationToken = default);

    Task<IReadOnlyCollection<ScopeSelectDto>> GetForSelectAsync(
        CancellationToken cancellationToken = default
    );
}
