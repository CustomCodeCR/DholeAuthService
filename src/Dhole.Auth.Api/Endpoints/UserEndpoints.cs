using CustomCodeFramework.Api.Responses;
using CustomCodeFramework.Core.Pagination;
using CustomCodeFramework.Cqrs.Dispatching;
using Dhole.Auth.Api.Authorization;
using Dhole.Auth.Application.Users.AssignRolesToUser;
using Dhole.Auth.Application.Users.AssignScopesToUser;
using Dhole.Auth.Application.Users.ChangeUserPassword;
using Dhole.Auth.Application.Users.CreateUser;
using Dhole.Auth.Application.Users.DeleteUser;
using Dhole.Auth.Application.Users.GetUserPermissions;
using Dhole.Auth.Application.Users.GetUserRoles;
using Dhole.Auth.Application.Users.GetUsers;
using Dhole.Auth.Application.Users.GetUserScopes;
using Dhole.Auth.Application.Users.RevokeRolesFromUser;
using Dhole.Auth.Application.Users.RevokeScopesFromUser;
using Dhole.Auth.Application.Users.SetUserActive;
using Dhole.Auth.Application.Users.SetUserLocked;
using Dhole.Auth.Application.Users.UpdateUser;
using Dhole.Auth.Contracts.Users;
using Dhole.Auth.Domain.Users.Enums;

namespace Dhole.Auth.Api.Endpoints;

public static class UserEndpoints
{
    public static IEndpointRouteBuilder MapUserEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/auth/users").WithTags("Users").RequireAuthorization();

        group.MapGet(
            "/",
            async (
                int pageNumber,
                int pageSize,
                string? search,
                bool? isActive,
                bool? isLocked,
                IQueryDispatcher dispatcher,
                CancellationToken cancellationToken
            ) =>
            {
                var result = await dispatcher.DispatchAsync(
                    new GetUsersQuery(
                        PageRequest.Create(pageNumber, pageSize),
                        search,
                        isActive,
                        isLocked
                    ),
                    cancellationToken
                );

                return EndpointResults.FromPaged(result);
            }
        ).RequireScope(AuthScopeNames.UsersView);

        group.MapGet(
            "/{userId:guid}/roles",
            async (Guid userId, IQueryDispatcher dispatcher, CancellationToken cancellationToken) =>
            {
                var result = await dispatcher.DispatchAsync(
                    new GetUserRolesQuery(userId),
                    cancellationToken
                );

                return Results.Ok(ApiResponse<IReadOnlyCollection<UserRoleDto>>.Ok(result));
            }
        ).RequireScope(AuthScopeNames.UsersView);

        group.MapGet(
            "/{userId:guid}/scopes",
            async (Guid userId, IQueryDispatcher dispatcher, CancellationToken cancellationToken) =>
            {
                var result = await dispatcher.DispatchAsync(
                    new GetUserScopesQuery(userId),
                    cancellationToken
                );

                return Results.Ok(ApiResponse<IReadOnlyCollection<UserScopeDto>>.Ok(result));
            }
        ).RequireScope(AuthScopeNames.UsersView);

        group.MapGet(
            "/{userId:guid}/permissions",
            async (Guid userId, IQueryDispatcher dispatcher, CancellationToken cancellationToken) =>
            {
                var result = await dispatcher.DispatchAsync(
                    new GetUserPermissionsQuery(userId),
                    cancellationToken
                );

                return Results.Ok(ApiResponse<UserPermissionsDto>.Ok(result));
            }
        ).RequireScope(AuthScopeNames.UsersView);

        group.MapPost(
            "/",
            async (
                CreateUserRequest request,
                ICommandDispatcher dispatcher,
                HttpContext httpContext,
                CancellationToken cancellationToken
            ) =>
            {
                if (!TryParseUserType(request.UserType, out var userType))
                {
                    return Results.BadRequest(
                        ApiErrorResponse.Create(
                            "Auth.InvalidUserType",
                            "El tipo de usuario no es válido. Use Internal o External.",
                            httpContext.TraceIdentifier
                        )
                    );
                }

                var result = await dispatcher.DispatchAsync(
                    new CreateUserCommand(
                        request.UserName,
                        request.Email,
                        request.DisplayName,
                        userType,
                        request.Password,
                        null
                    ),
                    cancellationToken
                );

                return EndpointResults.FromResult(result, httpContext);
            }
        ).RequireScope(AuthScopeNames.UsersCreate);

        group.MapPut(
            "/{userId:guid}",
            async (
                Guid userId,
                UpdateUserRequest request,
                ICommandDispatcher dispatcher,
                HttpContext httpContext,
                CancellationToken cancellationToken
            ) =>
            {
                var result = await dispatcher.DispatchAsync(
                    new UpdateUserCommand(
                        userId,
                        request.UserName,
                        request.Email,
                        request.DisplayName,
                        null
                    ),
                    cancellationToken
                );

                return EndpointResults.FromResult(result, httpContext);
            }
        ).RequireScope(AuthScopeNames.UsersUpdate);

        group.MapPatch(
            "/{userId:guid}/password",
            async (
                Guid userId,
                ChangeUserPasswordRequest request,
                ICommandDispatcher dispatcher,
                HttpContext httpContext,
                CancellationToken cancellationToken
            ) =>
            {
                var result = await dispatcher.DispatchAsync(
                    new ChangeUserPasswordCommand(userId, request.Password, null),
                    cancellationToken
                );

                return EndpointResults.FromResult(result, httpContext);
            }
        ).RequireScope(AuthScopeNames.UsersChangePassword);

        group.MapPatch(
            "/{userId:guid}/active",
            async (
                Guid userId,
                SetUserActiveRequest request,
                ICommandDispatcher dispatcher,
                HttpContext httpContext,
                CancellationToken cancellationToken
            ) =>
            {
                var result = await dispatcher.DispatchAsync(
                    new SetUserActiveCommand(userId, request.IsActive, null),
                    cancellationToken
                );

                return EndpointResults.FromResult(result, httpContext);
            }
        ).RequireScope(AuthScopeNames.UsersSetActive);

        group.MapPatch(
            "/{userId:guid}/locked",
            async (
                Guid userId,
                SetUserLockedRequest request,
                ICommandDispatcher dispatcher,
                HttpContext httpContext,
                CancellationToken cancellationToken
            ) =>
            {
                var result = await dispatcher.DispatchAsync(
                    new SetUserLockedCommand(userId, request.IsLocked, request.Reason, null),
                    cancellationToken
                );

                return EndpointResults.FromResult(result, httpContext);
            }
        ).RequireScope(AuthScopeNames.UsersSetLocked);

        group.MapDelete(
            "/{userId:guid}",
            async (
                Guid userId,
                ICommandDispatcher dispatcher,
                HttpContext httpContext,
                CancellationToken cancellationToken
            ) =>
            {
                var result = await dispatcher.DispatchAsync(
                    new DeleteUserCommand(userId, null),
                    cancellationToken
                );

                return EndpointResults.FromResult(result, httpContext);
            }
        ).RequireScope(AuthScopeNames.UsersDelete);

        group.MapPost(
            "/{userId:guid}/roles",
            async (
                Guid userId,
                UserBulkRoleRequest request,
                ICommandDispatcher dispatcher,
                HttpContext httpContext,
                CancellationToken cancellationToken
            ) =>
            {
                var result = await dispatcher.DispatchAsync(
                    new AssignRolesToUserCommand(userId, request.RoleIds, null),
                    cancellationToken
                );

                return EndpointResults.FromResult(result, httpContext);
            }
        ).RequireScope(AuthScopeNames.UsersRolesAssign);

        group.MapPost(
            "/{userId:guid}/roles/revoke",
            async (
                Guid userId,
                UserBulkRoleRequest request,
                ICommandDispatcher dispatcher,
                HttpContext httpContext,
                CancellationToken cancellationToken
            ) =>
            {
                var result = await dispatcher.DispatchAsync(
                    new RevokeRolesFromUserCommand(userId, request.RoleIds, null),
                    cancellationToken
                );

                return EndpointResults.FromResult(result, httpContext);
            }
        ).RequireScope(AuthScopeNames.UsersRolesRevoke);

        group.MapPost(
            "/{userId:guid}/scopes",
            async (
                Guid userId,
                UserBulkScopeRequest request,
                ICommandDispatcher dispatcher,
                HttpContext httpContext,
                CancellationToken cancellationToken
            ) =>
            {
                var result = await dispatcher.DispatchAsync(
                    new AssignScopesToUserCommand(userId, request.ScopeIds, null),
                    cancellationToken
                );

                return EndpointResults.FromResult(result, httpContext);
            }
        ).RequireScope(AuthScopeNames.UsersScopesAssign);

        group.MapPost(
            "/{userId:guid}/scopes/revoke",
            async (
                Guid userId,
                UserBulkScopeRequest request,
                ICommandDispatcher dispatcher,
                HttpContext httpContext,
                CancellationToken cancellationToken
            ) =>
            {
                var result = await dispatcher.DispatchAsync(
                    new RevokeScopesFromUserCommand(userId, request.ScopeIds, null),
                    cancellationToken
                );

                return EndpointResults.FromResult(result, httpContext);
            }
        ).RequireScope(AuthScopeNames.UsersScopesRevoke);

        return app;
    }

    private static bool TryParseUserType(string? value, out UserType userType)
    {
        userType = default;

        if (string.IsNullOrWhiteSpace(value))
        {
            return false;
        }

        return Enum.TryParse(value.Trim(), ignoreCase: true, out userType);
    }

    private sealed record CreateUserRequest(
        string UserName,
        string Email,
        string DisplayName,
        string UserType,
        string Password
    );

    private sealed record UpdateUserRequest(string UserName, string Email, string DisplayName);

    private sealed record ChangeUserPasswordRequest(string Password);

    private sealed record SetUserActiveRequest(bool IsActive);

    private sealed record SetUserLockedRequest(bool IsLocked, string? Reason);

    private sealed record UserBulkRoleRequest(IReadOnlyCollection<Guid> RoleIds);

    private sealed record UserBulkScopeRequest(IReadOnlyCollection<Guid> ScopeIds);
}
