namespace Dhole.Auth.Persistence.Seed;

internal static class PricingScopes
{
    public static IReadOnlyCollection<ScopeSeedDefinition> All =>
        [
            ScopeCatalog.Create(
                "pricing.fcl-rates.create",
                "Crear tarifas FCL",
                "Permite crear tarifas FCL manualmente."
            ),
            ScopeCatalog.Create(
                "pricing.fcl-rates.view",
                "Ver tarifas FCL",
                "Permite consultar tarifas FCL."
            ),
            ScopeCatalog.Create(
                "pricing.fcl-rates.update",
                "Actualizar tarifas FCL",
                "Permite actualizar montos y condiciones de tarifas FCL."
            ),
            ScopeCatalog.Create(
                "pricing.fcl-rates.delete",
                "Eliminar tarifas FCL",
                "Permite eliminar tarifas FCL."
            ),
            ScopeCatalog.Create(
                "pricing.fcl-rates.set-active",
                "Activar/Inactivar tarifas FCL",
                "Permite activar o inactivar tarifas FCL."
            ),
            ScopeCatalog.Create(
                "pricing.fcl-rates.expire",
                "Vencer tarifas FCL",
                "Permite marcar tarifas FCL como vencidas."
            ),
            ScopeCatalog.Create(
                "pricing.fcl-rate-imports.create",
                "Importar tarifas FCL",
                "Permite subir PDF, Excel o CSV y enviarlo a extracción de datos."
            ),
            ScopeCatalog.Create(
                "pricing.fcl-rate-imports.view",
                "Ver importaciones FCL",
                "Permite consultar importaciones, filas extraídas e incidencias."
            ),
            ScopeCatalog.Create(
                "pricing.fcl-rate-imports.approve",
                "Aprobar importaciones FCL",
                "Permite aprobar una importación y convertir filas válidas en tarifas FCL."
            ),
            ScopeCatalog.Create(
                "pricing.fcl-rate-imports.reject",
                "Rechazar importaciones FCL",
                "Permite rechazar importaciones de tarifas FCL."
            ),
            ScopeCatalog.Create(
                "pricing.fcl-rate-imports.delete",
                "Eliminar importaciones FCL",
                "Permite eliminar importaciones de tarifas FCL."
            ),
            ScopeCatalog.Create(
                "pricing.fcl-decisions.create",
                "Crear decisión FCL",
                "Permite generar recomendación tarifaria FCL."
            ),
            ScopeCatalog.Create(
                "pricing.fcl-decisions.view",
                "Ver decisiones FCL",
                "Permite consultar decisiones tarifarias FCL."
            ),
            ScopeCatalog.Create(
                "pricing.dashboard.view",
                "Ver dashboard de Pricing",
                "Permite ver indicadores del módulo de Pricing."
            ),
            ScopeCatalog.Create(
                "pricing.selects.view",
                "Ver selects de Pricing",
                "Permite consultar opciones de selección del módulo de Pricing."
            ),
        ];
}
