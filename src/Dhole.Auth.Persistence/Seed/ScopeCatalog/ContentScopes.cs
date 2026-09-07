namespace Dhole.Auth.Persistence.Seed;

internal static class ContentScopes
{
    public static IReadOnlyCollection<ScopeSeedDefinition> All =>
        [
            ScopeCatalog.Create("cms.view", "Ver Content", "Permite consultar contenidos, revisiones, medios, taxonomías, menús y ajustes de Content."),
            ScopeCatalog.Create("cms.create", "Crear contenido", "Permite crear contenidos en Content."),
            ScopeCatalog.Create("cms.edit", "Editar contenido", "Permite editar contenidos y medios, enviar a revisión, archivar, restaurar revisiones y crear o editar taxonomías."),
            ScopeCatalog.Create("cms.delete", "Eliminar contenido", "Permite eliminar contenidos y taxonomías."),
            ScopeCatalog.Create("cms.publish", "Publicar contenido", "Permite publicar, programar publicaciones y retirar contenidos publicados."),
            ScopeCatalog.Create("cms.media.upload", "Cargar medios de Content", "Permite cargar archivos y registrar referencias de medios en Content."),
            ScopeCatalog.Create("cms.media.delete", "Eliminar medios de Content", "Permite eliminar referencias de medios de Content."),
            ScopeCatalog.Create("cms.settings.edit", "Administrar ajustes de Content", "Permite modificar ajustes y administrar menús de navegación de Content."),
            // Preserve the complete Content contract. These specialized scopes are
            // not independently enforced by the current Content API endpoints.
            ScopeCatalog.Create("cms.pages.edit", "Editar páginas de Content", "Scope específico de páginas definido por Content; actualmente no sustituye cms.edit en la API."),
            ScopeCatalog.Create("cms.news.edit", "Editar noticias de Content", "Scope específico de noticias definido por Content; actualmente no sustituye cms.edit en la API."),
            ScopeCatalog.Create("cms.banners.edit", "Editar banners de Content", "Scope específico de banners definido por Content; actualmente no sustituye cms.edit en la API."),
            ScopeCatalog.Create("cms.seo.edit", "Editar SEO de Content", "Scope específico de SEO definido por Content; actualmente no sustituye cms.edit en la API."),
        ];
}
