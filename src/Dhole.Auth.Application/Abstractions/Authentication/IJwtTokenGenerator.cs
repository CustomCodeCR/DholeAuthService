namespace Dhole.Auth.Application.Abstractions.Authentication;

public interface IJwtTokenGenerator
{
    string Generate(
        Guid userId,
        Guid sessionId,
        string userType,
        string email,
        string userName,
        IReadOnlyCollection<string> roles,
        IReadOnlyCollection<string> scopes,
        int tokenVersion,
        DateTime expiresAt
    );
}
