using CustomCodeFramework.Core.Pagination;
using CustomCodeFramework.Cqrs.Dispatching;
using Dhole.Auth.Api.Authorization;
using Dhole.Auth.Application.Feature.Sessions.GetActiveUserSessions;
using Dhole.Auth.Application.Feature.Sessions.GetUserSessions;
using Dhole.Auth.Application.Sessions.RevokeSession;
using Dhole.Auth.Application.Sessions.RevokeUserSessions;

namespace Dhole.Auth.Api.Endpoints;

public static class SessionEndpoints
{
    public static IEndpointRouteBuilder MapSessionEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/auth/sessions").WithTags("Sessions").RequireAuthorization();

        group.MapGet(
            "/users/{userId:guid}",
            async (
                Guid userId,
                int pageNumber,
                int pageSize,
                IQueryDispatcher dispatcher,
                CancellationToken cancellationToken
            ) =>
            {
                var result = await dispatcher.DispatchAsync(
                    new GetUserSessionsQuery(userId, PageRequest.Create(pageNumber, pageSize)),
                    cancellationToken
                );

                return Results.Ok(result);
            }
        ).RequireScope(AuthScopeNames.SessionsView);

        group.MapGet(
            "/users/{userId:guid}/active",
            async (Guid userId, IQueryDispatcher dispatcher, CancellationToken cancellationToken) =>
            {
                var result = await dispatcher.DispatchAsync(
                    new GetActiveUserSessionsQuery(userId),
                    cancellationToken
                );

                return Results.Ok(result);
            }
        ).RequireScope(AuthScopeNames.SessionsView);

        group.MapPatch(
            "/{sessionId:guid}/revoke",
            async (
                Guid sessionId,
                RevokeSessionRequest request,
                ICommandDispatcher dispatcher,
                CancellationToken cancellationToken
            ) =>
            {
                var result = await dispatcher.DispatchAsync(
                    new RevokeSessionCommand(sessionId, request.RevokedBy, request.Reason),
                    cancellationToken
                );

                return result.IsSuccess ? Results.NoContent() : Results.BadRequest(result.Error);
            }
        ).RequireScope(AuthScopeNames.SessionsRevoke);

        group.MapPatch(
            "/users/{userId:guid}/revoke",
            async (
                Guid userId,
                RevokeSessionRequest request,
                ICommandDispatcher dispatcher,
                CancellationToken cancellationToken
            ) =>
            {
                var result = await dispatcher.DispatchAsync(
                    new RevokeUserSessionsCommand(userId, request.RevokedBy, request.Reason),
                    cancellationToken
                );

                return result.IsSuccess ? Results.NoContent() : Results.BadRequest(result.Error);
            }
        ).RequireScope(AuthScopeNames.SessionsRevokeAll);

        return app;
    }

    private sealed record RevokeSessionRequest(Guid? RevokedBy, string? Reason);
}
