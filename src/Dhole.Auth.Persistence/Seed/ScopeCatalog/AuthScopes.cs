namespace Dhole.Auth.Persistence.Seed;

internal static class AuthScopes
{
    public static IReadOnlyCollection<ScopeSeedDefinition> All =>
        [
            ScopeCatalog.Create("auth.users.create", "Crear usuarios", "Permite crear usuarios."),
            ScopeCatalog.Create("auth.users.view", "Ver usuarios", "Permite ver usuarios."),
            ScopeCatalog.Create(
                "auth.users.update",
                "Actualizar usuarios",
                "Permite actualizar usuarios."
            ),
            ScopeCatalog.Create(
                "auth.users.delete",
                "Eliminar usuarios",
                "Permite eliminar usuarios."
            ),
            ScopeCatalog.Create(
                "auth.users.set-active",
                "Activar/Inactivar usuarios",
                "Permite activar o inactivar usuarios."
            ),
            ScopeCatalog.Create(
                "auth.users.set-locked",
                "Bloquear/Desbloquear usuarios",
                "Permite bloquear o desbloquear usuarios."
            ),
            ScopeCatalog.Create(
                "auth.users.change-password",
                "Cambiar contraseña de usuarios",
                "Permite cambiar contraseñas de usuarios."
            ),
            ScopeCatalog.Create(
                "auth.users.roles.assign",
                "Asignar roles a usuarios",
                "Permite asignar roles a usuarios."
            ),
            ScopeCatalog.Create(
                "auth.users.roles.revoke",
                "Revocar roles de usuarios",
                "Permite revocar roles de usuarios."
            ),
            ScopeCatalog.Create(
                "auth.users.scopes.assign",
                "Asignar permisos a usuarios",
                "Permite asignar permisos directos a usuarios."
            ),
            ScopeCatalog.Create(
                "auth.users.scopes.revoke",
                "Revocar permisos de usuarios",
                "Permite revocar permisos directos de usuarios."
            ),
            ScopeCatalog.Create("auth.roles.create", "Crear roles", "Permite crear roles."),
            ScopeCatalog.Create("auth.roles.view", "Ver roles", "Permite ver roles."),
            ScopeCatalog.Create(
                "auth.roles.update",
                "Actualizar roles",
                "Permite actualizar roles."
            ),
            ScopeCatalog.Create("auth.roles.delete", "Eliminar roles", "Permite eliminar roles."),
            ScopeCatalog.Create(
                "auth.roles.set-active",
                "Activar/Inactivar roles",
                "Permite activar o inactivar roles."
            ),
            ScopeCatalog.Create(
                "auth.roles.scopes.assign",
                "Asignar permisos a roles",
                "Permite asignar permisos a roles."
            ),
            ScopeCatalog.Create(
                "auth.roles.scopes.revoke",
                "Revocar permisos de roles",
                "Permite revocar permisos de roles."
            ),
            ScopeCatalog.Create("auth.scopes.view", "Ver permisos", "Permite ver permisos."),
            ScopeCatalog.Create(
                "auth.scopes.set-active",
                "Activar/Inactivar permisos",
                "Permite activar o inactivar permisos."
            ),
            ScopeCatalog.Create("auth.sessions.view", "Ver sesiones", "Permite ver sesiones."),
            ScopeCatalog.Create(
                "auth.sessions.revoke",
                "Revocar sesiones",
                "Permite revocar sesiones."
            ),
            ScopeCatalog.Create(
                "auth.sessions.revoke-all",
                "Revocar todas las sesiones",
                "Permite revocar todas las sesiones de un usuario."
            ),
        ];
}
