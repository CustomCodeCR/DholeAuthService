namespace Dhole.Auth.Persistence.Seed;

internal static class AgentScopes
{
    public static IReadOnlyCollection<ScopeSeedDefinition> All =>
        [
            ScopeCatalog.Create(
                "agent.providers.view",
                "Ver proveedores Agent",
                "Permite consultar navieras y proveedores configurados en Dhole Agent."
            ),
            ScopeCatalog.Create(
                "agent.providers.manage",
                "Administrar proveedores Agent",
                "Permite crear, actualizar, activar e inactivar navieras y proveedores de Dhole Agent."
            ),
            ScopeCatalog.Create(
                "agent.definitions.view",
                "Ver definiciones Agent",
                "Permite consultar las definiciones de extracción disponibles en Dhole Agent."
            ),
            ScopeCatalog.Create(
                "agent.definitions.manage",
                "Administrar definiciones Agent",
                "Permite crear, actualizar, activar e inactivar definiciones de extracción."
            ),
            ScopeCatalog.Create(
                "agent.credentials.view",
                "Ver credenciales Agent",
                "Permite consultar las referencias de credenciales configuradas para proveedores Agent."
            ),
            ScopeCatalog.Create(
                "agent.credentials.manage",
                "Administrar credenciales Agent",
                "Permite crear, actualizar, activar e inactivar referencias de credenciales para proveedores Agent."
            ),
            ScopeCatalog.Create(
                "agent.credentials.verify",
                "Verificar credenciales Agent",
                "Permite verificar las credenciales configuradas para proveedores de Dhole Agent."
            ),
            ScopeCatalog.Create(
                "agent.routes.manage",
                "Administrar rutas de extracción Agent",
                "Permite consultar, crear, actualizar y eliminar las rutas de un perfil de extracción."
            ),
            ScopeCatalog.Create(
                "agent.equipment.manage",
                "Administrar equipos de extracción Agent",
                "Permite consultar, crear, actualizar y eliminar los equipos y contenedores de un perfil de extracción."
            ),
            ScopeCatalog.Create(
                "agent.capture-rules.manage",
                "Administrar reglas de captura Agent",
                "Permite consultar, crear, actualizar, eliminar y probar las reglas de captura de endpoints de un perfil de extracción."
            ),
            ScopeCatalog.Create(
                "agent.extraction-fields.manage",
                "Administrar campos de extracción Agent",
                "Permite consultar, crear, actualizar y eliminar los campos de un perfil de extracción."
            ),
            ScopeCatalog.Create(
                "agent.prompts.manage",
                "Administrar prompts de extracción Agent",
                "Permite generar la vista previa del prompt de un perfil de extracción."
            ),
            ScopeCatalog.Create(
                "agent.browser-profiles.view",
                "Ver perfiles de navegador Agent",
                "Permite consultar el estado de los perfiles de navegador persistentes."
            ),
            ScopeCatalog.Create(
                "agent.browser-profiles.authenticate",
                "Autenticar perfiles de navegador Agent",
                "Permite iniciar o renovar la autenticación de perfiles de navegador."
            ),
            ScopeCatalog.Create(
                "agent.schedules.view",
                "Ver programaciones Agent",
                "Permite consultar las extracciones programadas de Dhole Agent."
            ),
            ScopeCatalog.Create(
                "agent.schedules.create",
                "Crear programaciones Agent",
                "Permite programar nuevas extracciones automáticas."
            ),
            ScopeCatalog.Create(
                "agent.schedules.update",
                "Actualizar programaciones Agent",
                "Permite modificar, activar o inactivar programaciones existentes."
            ),
            ScopeCatalog.Create(
                "agent.schedules.delete",
                "Eliminar programaciones Agent",
                "Reserva el permiso para eliminar programaciones Agent cuando la operación esté habilitada."
            ),
            ScopeCatalog.Create(
                "agent.schedules.execute",
                "Ejecutar programaciones Agent",
                "Permite disparar manualmente una programación configurada."
            ),
            ScopeCatalog.Create(
                "agent.executions.view",
                "Ver ejecuciones Agent",
                "Permite consultar el historial y estado de las extracciones ejecutadas."
            ),
            ScopeCatalog.Create(
                "agent.executions.create",
                "Crear ejecuciones Agent",
                "Permite iniciar extracciones manuales desde Dhole Agent."
            ),
            ScopeCatalog.Create(
                "agent.executions.cancel",
                "Cancelar ejecuciones Agent",
                "Permite cancelar extracciones Agent pendientes o en ejecución."
            ),
        ];
}
