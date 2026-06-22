namespace Dhole.Auth.Persistence.Seed;

internal static class ConfigScopes
{
    public static IReadOnlyCollection<ScopeSeedDefinition> All =>
        [
            // Catalog groups
            ScopeCatalog.Create(
                "config.catalog-groups.create",
                "Crear catálogos",
                "Permite crear grupos de catálogos."
            ),
            ScopeCatalog.Create(
                "config.catalog-groups.view",
                "Ver catálogos",
                "Permite ver grupos de catálogos."
            ),
            ScopeCatalog.Create(
                "config.catalog-groups.update",
                "Actualizar catálogos",
                "Permite actualizar grupos de catálogos."
            ),
            ScopeCatalog.Create(
                "config.catalog-groups.delete",
                "Eliminar catálogos",
                "Permite eliminar grupos de catálogos."
            ),
            ScopeCatalog.Create(
                "config.catalog-groups.set-active",
                "Activar/Inactivar catálogos",
                "Permite activar o inactivar grupos de catálogos."
            ),
            // Catalog items
            ScopeCatalog.Create(
                "config.catalog-items.create",
                "Crear items de catálogo",
                "Permite crear items dentro de un catálogo."
            ),
            ScopeCatalog.Create(
                "config.catalog-items.view",
                "Ver items de catálogo",
                "Permite ver items de catálogos."
            ),
            ScopeCatalog.Create(
                "config.catalog-items.update",
                "Actualizar items de catálogo",
                "Permite actualizar items de catálogos."
            ),
            ScopeCatalog.Create(
                "config.catalog-items.delete",
                "Eliminar items de catálogo",
                "Permite eliminar items de catálogos."
            ),
            ScopeCatalog.Create(
                "config.catalog-items.set-active",
                "Activar/Inactivar items de catálogo",
                "Permite activar o inactivar items de catálogos."
            ),
            ScopeCatalog.Create(
                "config.catalog-items.change-sort-order",
                "Cambiar orden de items",
                "Permite cambiar el orden de los items dentro de un catálogo."
            ),
            // Selects / lookups
            ScopeCatalog.Create(
                "config.catalog-selects.view",
                "Ver selects de catálogos",
                "Permite consultar catálogos en formato de selección."
            ),
            ScopeCatalog.Create(
                "config.catalog-items.validate",
                "Validar items de catálogo",
                "Permite validar si un item existe y está activo dentro de un catálogo."
            ),
        ];
}
