namespace Dhole.Auth.Application.Abstractions.Sessions;

public interface ICurrentSessionContext
{
    bool IsAuthenticated { get; }
    Guid? UserId { get; }
    Guid? SessionId { get; }
    string? Email { get; }
    string? UserName { get; }
    string? UserType { get; }
    int? TokenVersion { get; }
    IReadOnlyCollection<string> Roles { get; }
    IReadOnlyCollection<string> Scopes { get; }
    bool HasRole(string role);
    bool HasScope(string scope);
}
