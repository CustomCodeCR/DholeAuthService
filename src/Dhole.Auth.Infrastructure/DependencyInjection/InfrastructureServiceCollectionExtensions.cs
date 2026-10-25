using System.Security.Claims;
using CustomCodeFramework.Auth.DependencyInjection;
using CustomCodeFramework.Mongo.DependencyInjection;
using CustomCodeFramework.Redis.DependencyInjection;
using Dhole.Auth.Application.Abstractions.Authentication;
using Dhole.Auth.Application.Abstractions.Mongo;
using Dhole.Auth.Application.Abstractions.Permissions;
using Dhole.Auth.Application.Abstractions.Repositories;
using Dhole.Auth.Application.Abstractions.Security;
using Dhole.Auth.Application.Abstractions.Sessions;
using Dhole.Auth.Infrastructure.Authentication;
using Dhole.Auth.Infrastructure.Mongo;
using Dhole.Auth.Infrastructure.Permissions;
using Dhole.Auth.Infrastructure.Security;
using Dhole.Auth.Infrastructure.Sessions;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Dhole.Auth.Infrastructure.DependencyInjection;

public static class InfrastructureServiceCollectionExtensions
{
    private const string UserIdClaimType = "user_id";
    private const string SessionIdClaimType = "session_id";
    private const string TokenVersionClaimType = "token_version";
    private const string ScopeClaimType = "scope";
    private const string RoleClaimType = "role";

    public static IServiceCollection AddInfrastructure(
        this IServiceCollection services,
        IConfiguration configuration
    )
    {
        services.AddCustomCodeAuth(configuration);

        services.PostConfigure<AuthenticationOptions>(options =>
        {
            options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
            options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
            options.DefaultScheme = JwtBearerDefaults.AuthenticationScheme;
        });

        services.PostConfigure<JwtBearerOptions>(
            JwtBearerDefaults.AuthenticationScheme,
            options =>
            {
                var originalOnTokenValidated = options.Events?.OnTokenValidated;

                options.Events ??= new JwtBearerEvents();
                options.Events.OnTokenValidated = async context =>
                {
                    if (originalOnTokenValidated is not null)
                    {
                        await originalOnTokenValidated(context);
                    }

                    await ValidateSessionAndRefreshPrincipalAsync(context);
                };
            }
        );

        services.AddCustomCodeRedis(configuration);
        services.AddCustomCodeMongo(configuration);

        services.Configure<LoginRateLimitOptions>(
            configuration.GetSection(LoginRateLimitOptions.SectionName)
        );

        services.AddScoped<ILoginRateLimiter, RedisLoginRateLimiter>();

        services.AddScoped<IPasswordHasher, PasswordHasher>();
        services.AddScoped<IRefreshTokenGenerator, RefreshTokenGenerator>();
        services.AddScoped<IJwtTokenGenerator, JwtTokenGenerator>();
        services.AddScoped<ICurrentSessionContext, CurrentSessionContext>();

        services.AddScoped<IRevokedTokenStore, RedisRevokedTokenStore>();
        services.AddScoped<IActiveSessionStore, RedisActiveSessionStore>();
        services.AddScoped<IEffectivePermissionCache, RedisEffectivePermissionCache>();

        services.AddScoped<IAuthLoginAttemptLogWriter, AuthLoginAttemptLogWriter>();
        services.AddScoped<IAuthUserSecuritySnapshotWriter, AuthUserSecuritySnapshotWriter>();

        return services;
    }

    private static async Task ValidateSessionAndRefreshPrincipalAsync(
        TokenValidatedContext context
    )
    {
        var principal = context.Principal;

        if (principal is null)
        {
            context.Fail("Token sin principal válido.");
            return;
        }

        if (!TryGetGuidClaim(principal, SessionIdClaimType, allowNameIdentifierFallback: false, out var sessionId))
        {
            context.Fail("Token sin session_id válido.");
            return;
        }

        if (!TryGetGuidClaim(principal, UserIdClaimType, allowNameIdentifierFallback: true, out var userId))
        {
            context.Fail("Token sin user_id válido.");
            return;
        }

        if (!TryGetIntClaim(principal, TokenVersionClaimType, out var tokenVersion))
        {
            context.Fail("Token sin token_version válido.");
            return;
        }

        var cancellationToken = context.HttpContext.RequestAborted;
        var services = context.HttpContext.RequestServices;

        var sessionRepository = services.GetRequiredService<ISessionRepository>();
        var userRepository = services.GetRequiredService<IUserRepository>();
        var effectivePermissionService = services.GetRequiredService<IEffectivePermissionService>();

        var session = await sessionRepository.GetByIdAsync(sessionId, cancellationToken);

        if (session is null || session.UserId != userId)
        {
            context.Fail("La sesión del token no existe o no pertenece al usuario.");
            return;
        }

        if (!session.CanBeUsed(DateTime.UtcNow))
        {
            context.Fail("La sesión fue revocada o expiró.");
            return;
        }

        var user = await userRepository.GetByIdAsync(userId, cancellationToken);

        if (user is null)
        {
            context.Fail("El usuario del token no existe.");
            return;
        }

        if (!user.IsActive || user.IsLocked)
        {
            context.Fail("El usuario está inactivo o bloqueado.");
            return;
        }

        if (user.TokenVersion != tokenVersion)
        {
            context.Fail("El token ya no está vigente para este usuario.");
            return;
        }

        var currentPermissions = await effectivePermissionService.GetAsync(userId, cancellationToken);

        RefreshPrincipalClaims(
            principal,
            currentPermissions.Roles,
            currentPermissions.EffectiveScopes
        );
    }

    private static bool TryGetGuidClaim(
        ClaimsPrincipal principal,
        string claimType,
        bool allowNameIdentifierFallback,
        out Guid value
    )
    {
        var raw = principal.FindFirst(claimType)?.Value;

        if (string.IsNullOrWhiteSpace(raw) && allowNameIdentifierFallback)
        {
            raw = principal.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        }

        return Guid.TryParse(raw, out value);
    }

    private static bool TryGetIntClaim(
        ClaimsPrincipal principal,
        string claimType,
        out int value
    )
    {
        var raw = principal.FindFirst(claimType)?.Value;
        return int.TryParse(raw, out value);
    }

    private static void RefreshPrincipalClaims(
        ClaimsPrincipal principal,
        IReadOnlyCollection<string> roles,
        IReadOnlyCollection<string> scopes
    )
    {
        var identity = principal.Identities.FirstOrDefault(x => x.IsAuthenticated);

        if (identity is null)
        {
            return;
        }

        RemoveClaims(identity, RoleClaimType);
        RemoveClaims(identity, ClaimTypes.Role);
        RemoveClaims(identity, ScopeClaimType);

        foreach (var role in roles.Where(x => !string.IsNullOrWhiteSpace(x)).Distinct(StringComparer.OrdinalIgnoreCase))
        {
            identity.AddClaim(new Claim(RoleClaimType, role));
            identity.AddClaim(new Claim(ClaimTypes.Role, role));
        }

        foreach (var scope in scopes.Where(x => !string.IsNullOrWhiteSpace(x)).Distinct(StringComparer.OrdinalIgnoreCase))
        {
            identity.AddClaim(new Claim(ScopeClaimType, scope));
        }
    }

    private static void RemoveClaims(ClaimsIdentity identity, string claimType)
    {
        foreach (var claim in identity.FindAll(claimType).ToList())
        {
            identity.RemoveClaim(claim);
        }
    }
}
