using CustomCodeFramework.Api.Responses;
using CustomCodeFramework.Core.Pagination;
using CustomCodeFramework.Cqrs.Dispatching;
using Dhole.Auth.Api.Authorization;
using Dhole.Auth.Api.Extensions;
using Dhole.Auth.Application.Roles.AssignScopesToRole;
using Dhole.Auth.Application.Roles.CreateRole;
using Dhole.Auth.Application.Roles.DeleteRole;
using Dhole.Auth.Application.Roles.GetRoles;
using Dhole.Auth.Application.Roles.GetRoleScopes;
using Dhole.Auth.Application.Roles.GetRolesForSelect;
using Dhole.Auth.Application.Roles.RevokeScopesFromRole;
using Dhole.Auth.Application.Roles.SetRoleActive;
using Dhole.Auth.Application.Roles.UpdateRole;
using Dhole.Auth.Contracts.Roles;

namespace Dhole.Auth.Api.Endpoints;

public static class RoleEndpoints
{
    public static IEndpointRouteBuilder MapRoleEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/auth/roles").WithTags("Roles").RequireAuthorization();

        group.MapGet(
            "/",
            async (
                int pageNumber,
                int pageSize,
                string? search,
                bool? isActive,
                IQueryDispatcher dispatcher,
                CancellationToken cancellationToken
            ) =>
            {
                var result = await dispatcher.DispatchAsync(
                    new GetRolesQuery(PageRequest.Create(pageNumber, pageSize), search, isActive),
                    cancellationToken
                );

                return EndpointResults.FromPaged(result);
            }
        ).RequireScope(AuthScopeNames.RolesView);

        group.MapGet(
            "/select",
            async (IQueryDispatcher dispatcher, CancellationToken cancellationToken) =>
            {
                var result = await dispatcher.DispatchAsync(
                    new GetRolesForSelectQuery(),
                    cancellationToken
                );

                return Results.Ok(ApiResponse<IReadOnlyCollection<RoleSelectDto>>.Ok(result));
            }
        ).RequireScope(AuthScopeNames.RolesView);

        group.MapGet(
            "/{roleId:guid}/scopes",
            async (Guid roleId, IQueryDispatcher dispatcher, CancellationToken cancellationToken) =>
            {
                var result = await dispatcher.DispatchAsync(
                    new GetRoleScopesQuery(roleId),
                    cancellationToken
                );

                return Results.Ok(ApiResponse<IReadOnlyCollection<RoleScopeDto>>.Ok(result));
            }
        ).RequireScope(AuthScopeNames.RolesView);

        group.MapPost(
            "/",
            async (
                CreateRoleRequest request,
                ICommandDispatcher dispatcher,
                HttpContext httpContext,
                CancellationToken cancellationToken
            ) =>
            {
                var result = await dispatcher.DispatchAsync(
                    new CreateRoleCommand(
                        request.Name,
                        request.Description,
                        request.IsSystemRole,
                        httpContext.GetCurrentUserId()
                    ),
                    cancellationToken
                );

                return EndpointResults.FromResult(result, httpContext);
            }
        ).RequireScope(AuthScopeNames.RolesCreate);

        group.MapPut(
            "/{roleId:guid}",
            async (
                Guid roleId,
                UpdateRoleRequest request,
                ICommandDispatcher dispatcher,
                HttpContext httpContext,
                CancellationToken cancellationToken
            ) =>
            {
                var result = await dispatcher.DispatchAsync(
                    new UpdateRoleCommand(roleId, request.Name, request.Description, httpContext.GetCurrentUserId()),
                    cancellationToken
                );

                return EndpointResults.FromResult(result, httpContext);
            }
        ).RequireScope(AuthScopeNames.RolesUpdate);

        group.MapPatch(
            "/{roleId:guid}/active",
            async (
                Guid roleId,
                SetRoleActiveRequest request,
                ICommandDispatcher dispatcher,
                HttpContext httpContext,
                CancellationToken cancellationToken
            ) =>
            {
                var result = await dispatcher.DispatchAsync(
                    new SetRoleActiveCommand(roleId, request.IsActive, httpContext.GetCurrentUserId()),
                    cancellationToken
                );

                return EndpointResults.FromResult(result, httpContext);
            }
        ).RequireScope(AuthScopeNames.RolesSetActive);

        group.MapDelete(
            "/{roleId:guid}",
            async (
                Guid roleId,
                ICommandDispatcher dispatcher,
                HttpContext httpContext,
                CancellationToken cancellationToken
            ) =>
            {
                var result = await dispatcher.DispatchAsync(
                    new DeleteRoleCommand(roleId, httpContext.GetCurrentUserId()),
                    cancellationToken
                );

                return EndpointResults.FromResult(result, httpContext);
            }
        ).RequireScope(AuthScopeNames.RolesDelete);

        group.MapPost(
            "/{roleId:guid}/scopes",
            async (
                Guid roleId,
                RoleBulkScopeRequest request,
                ICommandDispatcher dispatcher,
                HttpContext httpContext,
                CancellationToken cancellationToken
            ) =>
            {
                var result = await dispatcher.DispatchAsync(
                    new AssignScopesToRoleCommand(roleId, request.ScopeIds, httpContext.GetCurrentUserId()),
                    cancellationToken
                );

                return EndpointResults.FromResult(result, httpContext);
            }
        ).RequireScope(AuthScopeNames.RolesScopesAssign);

        group.MapPost(
            "/{roleId:guid}/scopes/revoke",
            async (
                Guid roleId,
                RoleBulkScopeRequest request,
                ICommandDispatcher dispatcher,
                HttpContext httpContext,
                CancellationToken cancellationToken
            ) =>
            {
                var result = await dispatcher.DispatchAsync(
                    new RevokeScopesFromRoleCommand(roleId, request.ScopeIds, httpContext.GetCurrentUserId()),
                    cancellationToken
                );

                return EndpointResults.FromResult(result, httpContext);
            }
        ).RequireScope(AuthScopeNames.RolesScopesRevoke);

        return app;
    }

    private sealed record CreateRoleRequest(string Name, string? Description, bool IsSystemRole);

    private sealed record UpdateRoleRequest(string Name, string? Description);

    private sealed record SetRoleActiveRequest(bool IsActive);

    private sealed record RoleBulkScopeRequest(IReadOnlyCollection<Guid> ScopeIds);
}
