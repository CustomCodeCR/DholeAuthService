using Dhole.Auth.Application.Abstractions.Authentication;

namespace Dhole.Auth.Workers.Security;

internal sealed class WorkerJwtTokenGenerator : IJwtTokenGenerator
{
    public string Generate(
        Guid userId,
        Guid sessionId,
        string userType,
        string email,
        string userName,
        IReadOnlyCollection<string> roles,
        IReadOnlyCollection<string> scopes,
        int tokenVersion,
        DateTime expiresAt
    )
    {
        return string.Empty;
    }
}
