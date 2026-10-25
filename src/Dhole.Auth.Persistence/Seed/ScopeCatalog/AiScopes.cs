namespace Dhole.Auth.Persistence.Seed;

internal static class AiScopes
{
    public static IReadOnlyCollection<ScopeSeedDefinition> All =>
        [
            // Connections
            ScopeCatalog.Create(
                "ai.connection.create",
                "Crear conexiones de IA",
                "Permite crear conexiones con proveedores de inteligencia artificial."
            ),
            ScopeCatalog.Create(
                "ai.connection.view",
                "Ver conexiones de IA",
                "Permite consultar conexiones con proveedores de inteligencia artificial."
            ),
            ScopeCatalog.Create(
                "ai.connection.update",
                "Actualizar conexiones de IA",
                "Permite modificar conexiones con proveedores de inteligencia artificial."
            ),
            ScopeCatalog.Create(
                "ai.connection.delete",
                "Eliminar conexiones de IA",
                "Permite eliminar conexiones con proveedores de inteligencia artificial."
            ),
            ScopeCatalog.Create(
                "ai.connection.set-active",
                "Activar/Inactivar conexiones de IA",
                "Permite activar o inactivar conexiones con proveedores de inteligencia artificial."
            ),
            ScopeCatalog.Create(
                "ai.connection.test",
                "Probar conexiones de IA",
                "Permite verificar la conectividad con proveedores de inteligencia artificial."
            ),
            ScopeCatalog.Create(
                "ai.connection.discover-models",
                "Descubrir modelos de IA",
                "Permite consultar los modelos disponibles en una conexión de inteligencia artificial."
            ),

            // Models
            ScopeCatalog.Create(
                "ai.model.create",
                "Crear modelos de IA",
                "Permite registrar modelos de inteligencia artificial."
            ),
            ScopeCatalog.Create(
                "ai.model.view",
                "Ver modelos de IA",
                "Permite consultar modelos de inteligencia artificial."
            ),
            ScopeCatalog.Create(
                "ai.model.update",
                "Actualizar modelos de IA",
                "Permite modificar modelos de inteligencia artificial."
            ),
            ScopeCatalog.Create(
                "ai.model.delete",
                "Eliminar modelos de IA",
                "Permite eliminar modelos de inteligencia artificial."
            ),
            ScopeCatalog.Create(
                "ai.model.set-active",
                "Activar/Inactivar modelos de IA",
                "Permite activar o inactivar modelos de inteligencia artificial."
            ),

            // Profiles
            ScopeCatalog.Create(
                "ai.profile.create",
                "Crear perfiles de IA",
                "Permite crear perfiles de ejecución de inteligencia artificial."
            ),
            ScopeCatalog.Create(
                "ai.profile.view",
                "Ver perfiles de IA",
                "Permite consultar perfiles de ejecución de inteligencia artificial."
            ),
            ScopeCatalog.Create(
                "ai.profile.update",
                "Actualizar perfiles de IA",
                "Permite modificar perfiles de ejecución de inteligencia artificial."
            ),
            ScopeCatalog.Create(
                "ai.profile.delete",
                "Eliminar perfiles de IA",
                "Permite eliminar perfiles de ejecución de inteligencia artificial."
            ),
            ScopeCatalog.Create(
                "ai.profile.set-active",
                "Activar/Inactivar perfiles de IA",
                "Permite activar o inactivar perfiles de ejecución de inteligencia artificial."
            ),
            ScopeCatalog.Create(
                "ai.profile.configure-models",
                "Configurar modelos de perfiles de IA",
                "Permite configurar los modelos principales y de respaldo de un perfil de inteligencia artificial."
            ),

            // Prompt templates
            ScopeCatalog.Create(
                "ai.prompt-template.create",
                "Crear plantillas de prompt",
                "Permite crear plantillas de prompt para inteligencia artificial."
            ),
            ScopeCatalog.Create(
                "ai.prompt-template.view",
                "Ver plantillas de prompt",
                "Permite consultar plantillas de prompt para inteligencia artificial."
            ),
            ScopeCatalog.Create(
                "ai.prompt-template.update",
                "Actualizar plantillas de prompt",
                "Permite modificar plantillas de prompt para inteligencia artificial."
            ),
            ScopeCatalog.Create(
                "ai.prompt-template.delete",
                "Eliminar plantillas de prompt",
                "Permite eliminar plantillas de prompt para inteligencia artificial."
            ),
            ScopeCatalog.Create(
                "ai.prompt-template.set-active",
                "Activar/Inactivar plantillas de prompt",
                "Permite activar o inactivar plantillas de prompt para inteligencia artificial."
            ),

            // Executions
            ScopeCatalog.Create(
                "ai.execution.view",
                "Ver ejecuciones de IA",
                "Permite consultar ejecuciones de inteligencia artificial y sus intentos."
            ),
            ScopeCatalog.Create(
                "ai.execution.execute",
                "Ejecutar inteligencia artificial",
                "Permite ejecutar chat, respuestas estructuradas y embeddings."
            ),
            ScopeCatalog.Create(
                "ai.execution.cancel",
                "Cancelar ejecuciones de IA",
                "Permite cancelar ejecuciones de inteligencia artificial."
            ),
        ];
}
