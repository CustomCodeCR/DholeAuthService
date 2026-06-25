namespace Dhole.Auth.Persistence.Seed;

internal static class ScrapingScopes
{
    public static IReadOnlyCollection<ScopeSeedDefinition> All =>
        [
            // Sources
            ScopeCatalog.Create(
                "scraping.sources.view",
                "Ver fuentes de scraping",
                "Permite consultar las fuentes de scraping registradas."
            ),
            ScopeCatalog.Create(
                "scraping.sources.create",
                "Crear fuentes de scraping",
                "Permite crear nuevas fuentes de scraping."
            ),
            ScopeCatalog.Create(
                "scraping.sources.update",
                "Actualizar fuentes de scraping",
                "Permite modificar la información de las fuentes de scraping."
            ),
            ScopeCatalog.Create(
                "scraping.sources.delete",
                "Eliminar fuentes de scraping",
                "Permite eliminar fuentes de scraping."
            ),
            ScopeCatalog.Create(
                "scraping.sources.set-active",
                "Activar o inactivar fuentes de scraping",
                "Permite activar o inactivar fuentes de scraping."
            ),
            ScopeCatalog.Create(
                "scraping.sources.mark-review",
                "Marcar fuente para revisión",
                "Permite marcar una fuente de scraping como pendiente de revisión de mapeo."
            ),
            ScopeCatalog.Create(
                "scraping.sources.clear-review",
                "Limpiar revisión de fuente",
                "Permite limpiar el estado de revisión de una fuente de scraping."
            ),
            // Credentials
            ScopeCatalog.Create(
                "scraping.credentials.view",
                "Ver credenciales de scraping",
                "Permite consultar las credenciales asociadas a fuentes de scraping."
            ),
            ScopeCatalog.Create(
                "scraping.credentials.create",
                "Crear credenciales de scraping",
                "Permite crear credenciales para fuentes de scraping."
            ),
            ScopeCatalog.Create(
                "scraping.credentials.update",
                "Actualizar credenciales de scraping",
                "Permite modificar credenciales de scraping."
            ),
            ScopeCatalog.Create(
                "scraping.credentials.delete",
                "Eliminar credenciales de scraping",
                "Permite eliminar credenciales de scraping."
            ),
            ScopeCatalog.Create(
                "scraping.credentials.rotate",
                "Rotar credenciales de scraping",
                "Permite rotar la referencia secreta de una credencial de scraping."
            ),
            ScopeCatalog.Create(
                "scraping.credentials.set-active",
                "Activar o inactivar credenciales de scraping",
                "Permite activar o inactivar credenciales de scraping."
            ),
            // Jobs
            ScopeCatalog.Create(
                "scraping.jobs.view",
                "Ver trabajos de scraping",
                "Permite consultar trabajos de scraping."
            ),
            ScopeCatalog.Create(
                "scraping.jobs.create",
                "Crear trabajos de scraping",
                "Permite crear trabajos de scraping."
            ),
            ScopeCatalog.Create(
                "scraping.jobs.start",
                "Iniciar trabajos de scraping",
                "Permite iniciar trabajos de scraping."
            ),
            ScopeCatalog.Create(
                "scraping.jobs.complete",
                "Completar trabajos de scraping",
                "Permite marcar trabajos de scraping como completados."
            ),
            ScopeCatalog.Create(
                "scraping.jobs.fail",
                "Marcar trabajos de scraping como fallidos",
                "Permite marcar trabajos de scraping como fallidos."
            ),
            ScopeCatalog.Create(
                "scraping.jobs.cancel",
                "Cancelar trabajos de scraping",
                "Permite cancelar trabajos de scraping."
            ),
            ScopeCatalog.Create(
                "scraping.jobs.retry",
                "Reintentar trabajos de scraping",
                "Permite reintentar trabajos de scraping."
            ),
            // Runs
            ScopeCatalog.Create(
                "scraping.runs.view",
                "Ver ejecuciones de scraping",
                "Permite consultar ejecuciones de scraping."
            ),
            ScopeCatalog.Create(
                "scraping.runs.create",
                "Crear ejecuciones de scraping",
                "Permite crear ejecuciones asociadas a trabajos de scraping."
            ),
            ScopeCatalog.Create(
                "scraping.runs.start",
                "Iniciar ejecuciones de scraping",
                "Permite iniciar ejecuciones de scraping."
            ),
            ScopeCatalog.Create(
                "scraping.runs.complete",
                "Completar ejecuciones de scraping",
                "Permite marcar ejecuciones de scraping como completadas."
            ),
            ScopeCatalog.Create(
                "scraping.runs.fail",
                "Marcar ejecuciones de scraping como fallidas",
                "Permite marcar ejecuciones de scraping como fallidas."
            ),
            ScopeCatalog.Create(
                "scraping.runs.retry",
                "Reintentar ejecuciones de scraping",
                "Permite reintentar ejecuciones de scraping fallidas."
            ),
            // Evidences
            ScopeCatalog.Create(
                "scraping.evidences.view",
                "Ver evidencias de scraping",
                "Permite consultar evidencias generadas durante ejecuciones de scraping."
            ),
            ScopeCatalog.Create(
                "scraping.evidences.create",
                "Crear evidencias de scraping",
                "Permite registrar evidencias generadas por scraping."
            ),
            ScopeCatalog.Create(
                "scraping.evidences.delete",
                "Eliminar evidencias de scraping",
                "Permite eliminar evidencias de scraping."
            ),
            // Rate candidates
            ScopeCatalog.Create(
                "scraping.rate-candidates.view",
                "Ver candidatos de tarifa",
                "Permite consultar candidatos de tarifa obtenidos por scraping."
            ),
            ScopeCatalog.Create(
                "scraping.rate-candidates.create",
                "Crear candidatos de tarifa",
                "Permite registrar candidatos de tarifa obtenidos por scraping."
            ),
            ScopeCatalog.Create(
                "scraping.rate-candidates.update",
                "Actualizar candidatos de tarifa",
                "Permite modificar información de candidatos de tarifa."
            ),
            ScopeCatalog.Create(
                "scraping.rate-candidates.normalize",
                "Normalizar candidatos de tarifa",
                "Permite normalizar candidatos de tarifa obtenidos por scraping."
            ),
            ScopeCatalog.Create(
                "scraping.rate-candidates.mark-review",
                "Marcar candidato para revisión",
                "Permite marcar candidatos de tarifa como pendientes de revisión."
            ),
            ScopeCatalog.Create(
                "scraping.rate-candidates.approve",
                "Aprobar candidatos de tarifa",
                "Permite aprobar candidatos de tarifa para su uso posterior."
            ),
            ScopeCatalog.Create(
                "scraping.rate-candidates.reject",
                "Rechazar candidatos de tarifa",
                "Permite rechazar candidatos de tarifa."
            ),
            ScopeCatalog.Create(
                "scraping.rate-candidates.send-to-pricing",
                "Enviar candidato a pricing",
                "Permite enviar candidatos de tarifa aprobados al servicio de pricing."
            ),
            // Extraction rules
            ScopeCatalog.Create(
                "scraping.extraction-rules.view",
                "Ver reglas de extracción",
                "Permite consultar reglas de extracción de scraping."
            ),
            ScopeCatalog.Create(
                "scraping.extraction-rules.create",
                "Crear reglas de extracción",
                "Permite crear reglas de extracción para fuentes de scraping."
            ),
            ScopeCatalog.Create(
                "scraping.extraction-rules.update",
                "Actualizar reglas de extracción",
                "Permite modificar reglas de extracción de scraping."
            ),
            ScopeCatalog.Create(
                "scraping.extraction-rules.delete",
                "Eliminar reglas de extracción",
                "Permite eliminar reglas de extracción de scraping."
            ),
            ScopeCatalog.Create(
                "scraping.extraction-rules.approve",
                "Aprobar reglas de extracción",
                "Permite aprobar reglas de extracción de scraping."
            ),
            ScopeCatalog.Create(
                "scraping.extraction-rules.reject",
                "Rechazar reglas de extracción",
                "Permite rechazar reglas de extracción de scraping."
            ),
            ScopeCatalog.Create(
                "scraping.extraction-rules.set-active",
                "Activar o inactivar reglas de extracción",
                "Permite activar o inactivar reglas de extracción de scraping."
            ),
        ];
}
