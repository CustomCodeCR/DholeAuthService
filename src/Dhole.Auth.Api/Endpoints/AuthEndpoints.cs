using CustomCodeFramework.Cqrs.Dispatching;
using Dhole.Auth.Api.Authorization;
using Dhole.Auth.Api.Extensions;
using Dhole.Auth.Application.Auth.Login;
using Dhole.Auth.Application.Auth.RefreshToken;
using Dhole.Auth.Application.Auth.StartImpersonation;
using Dhole.Auth.Application.Auth.StopImpersonation;
using Dhole.Auth.Application.Users.ChangeUserPassword;
using Dhole.Auth.Domain.Shared;

namespace Dhole.Auth.Api.Endpoints;

public static class AuthEndpoints
{
    public static IEndpointRouteBuilder MapAuthEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/auth").WithTags("Auth");

        group.MapPost(
            "/login",
            async (
                LoginRequest request,
                ICommandDispatcher dispatcher,
                HttpContext httpContext,
                CancellationToken cancellationToken
            ) =>
            {
                var command = new LoginCommand(
                    request.Email,
                    request.Password,
                    httpContext.Connection.RemoteIpAddress?.ToString(),
                    httpContext.Request.Headers.UserAgent.ToString()
                );

                var result = await dispatcher.DispatchAsync(command, cancellationToken);

                return result.IsSuccess
                    ? Results.Ok(result.Value)
                    : Results.BadRequest(result.Error);
            }
        );

        group.MapPost(
            "/change-password",
            async (
                ChangeOwnPasswordRequest request,
                ICommandDispatcher dispatcher,
                HttpContext httpContext,
                CancellationToken cancellationToken
            ) =>
            {
                var currentUserId = httpContext.GetCurrentUserId();
                if (currentUserId is null)
                {
                    return Results.Unauthorized();
                }

                var result = await dispatcher.DispatchAsync(
                    new ChangeUserPasswordCommand(
                        currentUserId.Value,
                        request.Password,
                        currentUserId.Value
                    ),
                    cancellationToken
                );

                return result.IsSuccess
                    ? Results.NoContent()
                    : Results.BadRequest(result.Error);
            }
        ).RequireAuthorization();

        group.MapPost(
            "/impersonation/{userId:guid}",
            async (
                Guid userId,
                ICommandDispatcher dispatcher,
                HttpContext httpContext,
                CancellationToken cancellationToken
            ) =>
            {
                var actorUserId = httpContext.GetCurrentUserId();
                if (actorUserId is null)
                {
                    return Results.Unauthorized();
                }

                if (httpContext.IsImpersonating())
                {
                    return Results.BadRequest(AuthErrors.NestedImpersonationNotAllowed);
                }

                var result = await dispatcher.DispatchAsync(
                    new StartImpersonationCommand(
                        actorUserId.Value,
                        userId,
                        httpContext.Connection.RemoteIpAddress?.ToString(),
                        httpContext.Request.Headers.UserAgent.ToString()
                    ),
                    cancellationToken
                );

                return result.IsSuccess
                    ? Results.Ok(result.Value)
                    : Results.BadRequest(result.Error);
            }
        )
        .RequireAuthorization()
        .RequireScope(AuthScopeNames.UsersImpersonate);

        group.MapPost(
            "/impersonation/stop",
            async (
                ICommandDispatcher dispatcher,
                HttpContext httpContext,
                CancellationToken cancellationToken
            ) =>
            {
                var currentUserId = httpContext.GetCurrentUserId();
                var sessionId = httpContext.GetCurrentSessionId();
                var impersonatorUserId = httpContext.GetImpersonatorUserId();

                if (
                    currentUserId is null
                    || sessionId is null
                    || impersonatorUserId is null
                    || !httpContext.IsImpersonating()
                )
                {
                    return Results.BadRequest(AuthErrors.NotImpersonating);
                }

                var result = await dispatcher.DispatchAsync(
                    new StopImpersonationCommand(
                        currentUserId.Value,
                        sessionId.Value,
                        impersonatorUserId.Value
                    ),
                    cancellationToken
                );

                return result.IsSuccess
                    ? Results.NoContent()
                    : Results.BadRequest(result.Error);
            }
        ).RequireAuthorization();

        group.MapPost(
            "/refresh",
            async (
                RefreshTokenRequest request,
                ICommandDispatcher dispatcher,
                HttpContext httpContext,
                CancellationToken cancellationToken
            ) =>
            {
                var command = new RefreshTokenCommand(
                    request.RefreshToken,
                    httpContext.Connection.RemoteIpAddress?.ToString(),
                    httpContext.Request.Headers.UserAgent.ToString()
                );

                var result = await dispatcher.DispatchAsync(command, cancellationToken);

                return result.IsSuccess
                    ? Results.Ok(result.Value)
                    : Results.BadRequest(result.Error);
            }
        );

        return app;
    }

    private sealed record LoginRequest(string Email, string Password);

    private sealed record ChangeOwnPasswordRequest(string Password);

    private sealed record RefreshTokenRequest(string RefreshToken);
}
