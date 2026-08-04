namespace Dhole.Auth.Persistence.Seed;

internal static class ReportsScopes
{
    public static IReadOnlyCollection<ScopeSeedDefinition> All =>
        [
            ScopeCatalog.Create(
                "reports.templates.create",
                "Crear plantillas de reportes",
                "Permite crear plantillas HTML de reportes desde el diseñador visual."
            ),
            ScopeCatalog.Create(
                "reports.templates.view",
                "Ver plantillas de reportes",
                "Permite consultar plantillas, su configuración y vista previa PDF."
            ),
            ScopeCatalog.Create(
                "reports.templates.update",
                "Modificar plantillas de reportes",
                "Permite modificar plantillas HTML y regenerar su vista previa PDF."
            ),
            ScopeCatalog.Create(
                "reports.templates.delete",
                "Eliminar plantillas de reportes",
                "Permite eliminar plantillas de reportes."
            ),
            ScopeCatalog.Create(
                "reports.reports.generate",
                "Generar reportes",
                "Permite generar y descargar reportes PDF, XLSX y CSV desde una plantilla."
            ),
        ];
}
