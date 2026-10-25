namespace Dhole.Auth.Persistence.Seed;

internal static class NotificationsScopes
{
    public static IReadOnlyCollection<ScopeSeedDefinition> All =>
        [
            ScopeCatalog.Create(
                "notifications.messages.create",
                "Crear notificaciones",
                "Permite crear notificaciones inmediatas y programadas para cualquiera de los canales habilitados."
            ),
            ScopeCatalog.Create(
                "notifications.messages.view",
                "Ver notificaciones",
                "Permite consultar notificaciones, destinatarios, estados e intentos de entrega."
            ),
            ScopeCatalog.Create(
                "notifications.templates.manage",
                "Administrar plantillas de notificación",
                "Permite crear, modificar, activar, desactivar y eliminar plantillas de notificación."
            ),
            ScopeCatalog.Create(
                "notifications.history.view",
                "Ver historial de notificaciones",
                "Permite consultar el historial por entidad y por destinatario."
            ),
        ];
}
