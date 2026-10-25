using CustomCodeFramework.Cqrs.Dispatching;
using Dhole.Auth.Api.Authorization;
using Dhole.Auth.Application.Feature.Scopes.GetScopesForSelect;

namespace Dhole.Auth.Api.Endpoints;

public static class ScopeEndpoints
{
    public static IEndpointRouteBuilder MapScopeEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/auth/scopes").WithTags("Scopes").RequireAuthorization();

        group.MapGet(
            "/select",
            async (IQueryDispatcher dispatcher, CancellationToken cancellationToken) =>
            {
                var result = await dispatcher.DispatchAsync(
                    new GetScopesForSelectQuery(),
                    cancellationToken
                );

                return Results.Ok(result);
            }
        ).RequireScope(AuthScopeNames.ScopesView);

        return app;
    }
}
