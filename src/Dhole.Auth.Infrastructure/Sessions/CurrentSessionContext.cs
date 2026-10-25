using CustomCodeFramework.Auth.Abstractions;
using Dhole.Auth.Application.Abstractions.Sessions;

namespace Dhole.Auth.Infrastructure.Sessions;

internal sealed class CurrentSessionContext(ICurrentUserService currentUser)
    : ICurrentSessionContext
{
    public bool IsAuthenticated => currentUser.IsAuthenticated;
    public Guid? UserId => Guid.TryParse(currentUser.UserId, out var value) ? value : null;
    public Guid? SessionId => Guid.TryParse(currentUser.SessionId, out var value) ? value : null;
    public string? Email => currentUser.Email;
    public string? UserName => currentUser.UserName;
    public string? UserType => currentUser.UserType;
    public int? TokenVersion => currentUser.TokenVersion;
    public IReadOnlyCollection<string> Roles => currentUser.Roles;
    public IReadOnlyCollection<string> Scopes => currentUser.Scopes;

    public bool HasRole(string role)
    {
        return Roles.Contains(role, StringComparer.OrdinalIgnoreCase);
    }

    public bool HasScope(string scope)
    {
        return currentUser.HasScope(scope);
    }
}
