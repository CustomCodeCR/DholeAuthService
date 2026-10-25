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
            ScopeCatalog.Create("cms.settings.edit", "Administrar ajustes de Content", "Permite modificar ajustes de Content."),
            ScopeCatalog.Create("cms.pages.edit", "Editar páginas de Content", "Permite editar páginas de Content."),
            ScopeCatalog.Create("cms.news.edit", "Editar noticias de Content", "Permite editar noticias de Content."),
            ScopeCatalog.Create("cms.banners.edit", "Editar banners de Content", "Permite editar banners y contenido promocional."),
            ScopeCatalog.Create("cms.seo.edit", "Editar SEO de Content", "Permite administrar metadatos SEO de Content."),

            // FASE 23 — permisos granulares de Mercadeo. Los scopes históricos se conservan
            // para compatibilidad; ContentService acepta el scope específico o su equivalente legacy.
            ScopeCatalog.Create("cms.navigation.edit", "Editar navegación", "Permite administrar menús y navegación del sitio."),
            ScopeCatalog.Create("cms.collections.edit", "Editar collections", "Permite administrar collections y sus elementos."),
            ScopeCatalog.Create("cms.forms.view", "Ver formularios", "Permite consultar formularios de Mercadeo y sus campos."),
            ScopeCatalog.Create("cms.forms.edit", "Editar formularios", "Permite crear, modificar y eliminar formularios y campos."),
            ScopeCatalog.Create("cms.submissions.view", "Ver submissions", "Permite consultar envíos recibidos desde formularios."),
            ScopeCatalog.Create("cms.leads.view", "Ver leads", "Permite consultar leads de Mercadeo."),
            ScopeCatalog.Create("cms.leads.edit", "Editar leads", "Permite crear, actualizar, tocar y eliminar leads de Mercadeo."),
            ScopeCatalog.Create("cms.meetings.view", "Ver reuniones", "Permite consultar tipos y solicitudes de reunión."),
            ScopeCatalog.Create("cms.meetings.edit", "Editar reuniones", "Permite administrar tipos, solicitudes y estados de reunión."),
            ScopeCatalog.Create("cms.campaigns.view", "Ver campañas", "Permite consultar campañas de Mercadeo."),
            ScopeCatalog.Create("cms.campaigns.edit", "Editar campañas", "Permite crear, modificar y eliminar campañas de Mercadeo."),
            ScopeCatalog.Create("cms.redirects.edit", "Editar redirects", "Permite crear, modificar y eliminar redirects del sitio."),
            ScopeCatalog.Create("cms.reviews.submit", "Enviar a revisión", "Permite enviar contenido al flujo de aprobación."),
            ScopeCatalog.Create("cms.reviews.approve", "Aprobar contenido", "Permite aprobar o rechazar revisiones de contenido."),
        ];
}
