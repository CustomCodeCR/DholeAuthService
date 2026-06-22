using CustomCodeFramework.Core.Results;

namespace Dhole.Auth.Domain.Shared;

public static class AuthErrors
{
    public static readonly Error EmailRequired = new(
        "Auth.EmailRequired",
        "El correo electrónico es obligatorio."
    );

    public static readonly Error InvalidEmail = new(
        "Auth.InvalidEmail",
        "El correo electrónico no es válido."
    );

    public static readonly Error UserInactive = new(
        "Auth.UserInactive",
        "El usuario se encuentra inactivo."
    );

    public static readonly Error UserLocked = new(
        "Auth.UserLocked",
        "El usuario se encuentra bloqueado."
    );

    public static readonly Error RoleInactive = new(
        "Auth.RoleInactive",
        "El rol se encuentra inactivo."
    );

    public static readonly Error ScopeInactive = new(
        "Auth.ScopeInactive",
        "El permiso se encuentra inactivo."
    );

    public static readonly Error InvalidCredentials = new(
        "Auth.InvalidCredentials",
        "Las credenciales proporcionadas no son válidas."
    );

    public static readonly Error SessionRevoked = new(
        "Auth.SessionRevoked",
        "La sesión ha sido revocada."
    );

    public static readonly Error UserNameAlreadyExists = new(
        "Auth.UserNameAlreadyExists",
        "El nombre de usuario ya existe."
    );

    public static readonly Error EmailAlreadyExists = new(
        "Auth.EmailAlreadyExists",
        "El correo electrónico ya existe."
    );

    public static readonly Error UserNotFound = new(
        "Auth.UserNotFound",
        "No se encontró el usuario solicitado."
    );

    public static readonly Error RoleNotFound = new(
        "Auth.RoleNotFound",
        "No se encontró el rol solicitado."
    );

    public static readonly Error ScopeNotFound = new(
        "Auth.ScopeNotFound",
        "No se encontró el permiso solicitado."
    );

    public static readonly Error RoleAlreadyExists = new(
        "Auth.RoleAlreadyExists",
        "Ya existe un rol con el mismo nombre."
    );

    public static readonly Error SessionNotFound = new(
        "Auth.SessionNotFound",
        "No se encontró la sesión solicitada."
    );

    public static readonly Error ScopeAlreadyExists = new(
        "Auth.ScopeAlreadyExists",
        "Ya existe un permiso con el mismo código."
    );

    public static readonly Error SystemRoleCannotBeDeleted = new(
        "Auth.SystemRoleCannotBeDeleted",
        "Los roles del sistema no pueden eliminarse."
    );

    public static readonly Error LoginRateLimited = new(
        "Auth.LoginRateLimited",
        "Se ha excedido la cantidad máxima de intentos de inicio de sesión. Intente nuevamente más tarde."
    );

    public static readonly Error InvalidRefreshToken = new(
        "Auth.InvalidRefreshToken",
        "El token de actualización no es válido."
    );

    public static readonly Error UserAlreadyAssignedToRole = new(
        "Auth.UserAlreadyAssignedToRole",
        "El usuario ya tiene asignado ese rol."
    );

    public static readonly Error UserAlreadyAssignedToScope = new(
        "Auth.UserAlreadyAssignedToScope",
        "El usuario ya tiene asignado ese permiso."
    );

    public static readonly Error RoleAlreadyAssignedToScope = new(
        "Auth.RoleAlreadyAssignedToScope",
        "El rol ya tiene asignado ese permiso."
    );

    public static readonly Error PasswordRequired = new(
        "Auth.PasswordRequired",
        "La contraseña es obligatoria."
    );

    public static readonly Error InvalidPassword = new(
        "Auth.InvalidPassword",
        "La contraseña no cumple con los requisitos de seguridad."
    );

    public static readonly Error SessionExpired = new(
        "Auth.SessionExpired",
        "La sesión ha expirado."
    );

    public static readonly Error RefreshTokenExpired = new(
        "Auth.RefreshTokenExpired",
        "El token de actualización ha expirado."
    );

    public static readonly Error CannotDeleteYourOwnUser = new(
        "Auth.CannotDeleteYourOwnUser",
        "No es posible eliminar el usuario actualmente autenticado."
    );

    public static readonly Error CannotDeactivateYourOwnUser = new(
        "Auth.CannotDeactivateYourOwnUser",
        "No es posible desactivar el usuario actualmente autenticado."
    );

    public static readonly Error CannotLockYourOwnUser = new(
        "Auth.CannotLockYourOwnUser",
        "No es posible bloquear el usuario actualmente autenticado."
    );
}
