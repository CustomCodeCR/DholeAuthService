namespace Dhole.Auth.Persistence.Seed;

internal static class PricingScopes
{
    public static IReadOnlyCollection<ScopeSeedDefinition> All =>
        [
            // Costs
            ScopeCatalog.Create(
                "pricing.cost.create",
                "Crear costos",
                "Permite crear costos base de Pricing."
            ),
            ScopeCatalog.Create(
                "pricing.cost.view",
                "Ver costos",
                "Permite consultar costos base de Pricing."
            ),
            ScopeCatalog.Create(
                "pricing.cost.update",
                "Actualizar costos",
                "Permite modificar costos base de Pricing."
            ),
            ScopeCatalog.Create(
                "pricing.cost.delete",
                "Eliminar costos",
                "Permite eliminar costos base de Pricing."
            ),
            ScopeCatalog.Create(
                "pricing.cost.set-active",
                "Activar/Inactivar costos",
                "Permite activar o inactivar costos base de Pricing."
            ),
            ScopeCatalog.Create(
                "pricing.cost.select",
                "Seleccionar costos",
                "Permite consultar costos para listas de selección."
            ),
            // Rate term catalog
            ScopeCatalog.Create(
                "pricing.rate-term.create",
                "Crear condiciones de tarifa",
                "Permite crear ítems reutilizables de Incluye, Sujeto a y No incluye."
            ),
            ScopeCatalog.Create(
                "pricing.rate-term.view",
                "Ver condiciones de tarifa",
                "Permite consultar el catálogo de condiciones comerciales."
            ),
            ScopeCatalog.Create(
                "pricing.rate-term.update",
                "Actualizar condiciones de tarifa",
                "Permite modificar categoría, texto y orden de las condiciones comerciales."
            ),
            ScopeCatalog.Create(
                "pricing.rate-term.delete",
                "Eliminar condiciones de tarifa",
                "Permite eliminar condiciones comerciales del catálogo."
            ),
            ScopeCatalog.Create(
                "pricing.rate-term.set-active",
                "Activar/Inactivar condiciones de tarifa",
                "Permite activar o inactivar condiciones comerciales."
            ),
            ScopeCatalog.Create(
                "pricing.rate-term.select",
                "Seleccionar condiciones de tarifa",
                "Permite consultar condiciones activas para crear una tarifa."
            ),
            // Imported FCL rates
            ScopeCatalog.Create(
                "pricing.import-fcl-rate.create",
                "Importar tarifas FCL",
                "Permite subir archivos PDF, Excel o CSV para extraer tarifas FCL."
            ),
            ScopeCatalog.Create(
                "pricing.import-fcl-rate.view",
                "Ver tarifas FCL importadas",
                "Permite consultar tarifas FCL importadas y su estado."
            ),
            ScopeCatalog.Create(
                "pricing.import-fcl-rate.delete",
                "Eliminar tarifas FCL importadas",
                "Permite eliminar tarifas FCL importadas."
            ),
            ScopeCatalog.Create(
                "pricing.import-fcl-rate.approve",
                "Aprobar tarifas FCL importadas",
                "Permite aprobar tarifas FCL importadas."
            ),
            ScopeCatalog.Create(
                "pricing.import-fcl-rate.reject",
                "Rechazar tarifas FCL importadas",
                "Permite rechazar tarifas FCL importadas."
            ),
            ScopeCatalog.Create(
                "pricing.import-fcl-rate.create-as-rate",
                "Crear tarifa desde importación FCL",
                "Permite convertir una tarifa FCL importada en una tarifa oficial."
            ),
            // Rates
            ScopeCatalog.Create(
                "pricing.rate.create",
                "Crear tarifas",
                "Permite crear tarifas oficiales de Pricing."
            ),
            ScopeCatalog.Create(
                "pricing.rate.view",
                "Ver tarifas",
                "Permite consultar tarifas oficiales de Pricing."
            ),
            ScopeCatalog.Create(
                "pricing.rate.update",
                "Actualizar tarifas",
                "Permite modificar tarifas oficiales de Pricing."
            ),
            ScopeCatalog.Create(
                "pricing.rate.delete",
                "Eliminar tarifas",
                "Permite eliminar tarifas oficiales de Pricing."
            ),
            ScopeCatalog.Create(
                "pricing.rate.set-active",
                "Activar/Inactivar tarifas",
                "Permite activar o inactivar tarifas oficiales de Pricing."
            ),
            ScopeCatalog.Create(
                "pricing.rate.select",
                "Seleccionar tarifas",
                "Permite consultar tarifas para listas de selección."
            ),
            ScopeCatalog.Create(
                "pricing.rate.approve-low-margin",
                "Aprobar margen bajo",
                "Permite aprobar tarifas con margen menor al mínimo permitido."
            ),
            ScopeCatalog.Create(
                "pricing.rate.approve-freight",
                "Aprobar flete",
                "Permite aprobar o modificar manualmente la venta del flete internacional."
            ),
            ScopeCatalog.Create(
                "pricing.rate.report.generate",
                "Generar documento de tarifa",
                "Permite generar y descargar documentos comerciales de tarifas mediante las plantillas de Reports."
            ),
            // FCL rate details
            ScopeCatalog.Create(
                "pricing.fcl-rate-detail.create",
                "Agregar detalle FCL",
                "Permite agregar detalles FCL a una tarifa oficial."
            ),
            ScopeCatalog.Create(
                "pricing.fcl-rate-detail.update",
                "Actualizar detalle FCL",
                "Permite modificar detalles FCL de una tarifa oficial."
            ),
            ScopeCatalog.Create(
                "pricing.fcl-rate-detail.delete",
                "Eliminar detalle FCL",
                "Permite eliminar detalles FCL de una tarifa oficial."
            ),
            // Rate cost details
            ScopeCatalog.Create(
                "pricing.rate-cost-detail.create",
                "Agregar costo a tarifa",
                "Permite agregar costos aplicados a una tarifa oficial."
            ),
            ScopeCatalog.Create(
                "pricing.rate-cost-detail.update",
                "Actualizar costo de tarifa",
                "Permite modificar costos aplicados a una tarifa oficial."
            ),
            ScopeCatalog.Create(
                "pricing.rate-cost-detail.delete",
                "Eliminar costo de tarifa",
                "Permite eliminar costos aplicados a una tarifa oficial."
            ),
            // FCL decisions
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
                "pricing.fcl-decisions.delete",
                "Eliminar decisión FCL",
                "Permite eliminar decisiones tarifarias FCL."
            ),
        ];
}
