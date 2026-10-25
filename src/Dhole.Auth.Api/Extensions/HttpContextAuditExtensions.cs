using System.Security.Claims;

namespace Dhole.Auth.Api.Extensions;

public static class HttpContextAuditExtensions
{
    public static Guid? GetCurrentUserId(this HttpContext httpContext)
    {
        return GetGuidClaim(
            httpContext.User,
            "sub",
            "user_id",
            "nameidentifier",
            ClaimTypes.NameIdentifier
        );
    }

    public static Guid? GetCurrentSessionId(this HttpContext httpContext)
    {
        return GetGuidClaim(httpContext.User, "session_id", "sessionId", "sid");
    }

    public static Guid? GetImpersonatorUserId(this HttpContext httpContext)
    {
        return GetGuidClaim(httpContext.User, "impersonator_user_id");
    }

    public static bool IsImpersonating(this HttpContext httpContext)
    {
        var value = httpContext.User.FindFirst("impersonation")?.Value;

        return string.Equals(value, "true", StringComparison.OrdinalIgnoreCase)
            || httpContext.GetImpersonatorUserId() is not null;
    }

    private static Guid? GetGuidClaim(ClaimsPrincipal principal, params string[] claimTypes)
    {
        foreach (var claimType in claimTypes)
        {
            var raw = principal.FindFirst(claimType)?.Value;

            if (Guid.TryParse(raw, out var value))
            {
                return value;
            }
        }

        return null;
    }
}
