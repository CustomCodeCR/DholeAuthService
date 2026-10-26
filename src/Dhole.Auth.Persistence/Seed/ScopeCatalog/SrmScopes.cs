namespace Dhole.Auth.Persistence.Seed;

internal static class SrmScopes
{
    public static IReadOnlyCollection<ScopeSeedDefinition> All =>
    [
        ScopeCatalog.Create("srm.supplier.read", "SRM · Proveedores · Consultar", "Permiso específico de SRM: srm.supplier.read."),
        ScopeCatalog.Create("srm.supplier.create", "SRM · Proveedores · Crear", "Permiso específico de SRM: srm.supplier.create."),
        ScopeCatalog.Create("srm.supplier.update", "SRM · Proveedores · Editar", "Permiso específico de SRM: srm.supplier.update."),
        ScopeCatalog.Create("srm.supplier.archive", "SRM · Proveedores · Archivar", "Permiso específico de SRM: srm.supplier.archive."),
        ScopeCatalog.Create("srm.performance.read", "SRM · Evaluaciones · Consultar", "Permiso específico de SRM: srm.performance.read."),
        ScopeCatalog.Create("srm.performance.manage", "SRM · Evaluaciones · Administrar", "Permiso específico de SRM: srm.performance.manage."),
        ScopeCatalog.Create("srm.contract.read", "SRM · Contratos · Consultar", "Permiso específico de SRM: srm.contract.read."),
        ScopeCatalog.Create("srm.contract.manage", "SRM · Contratos · Administrar", "Permiso específico de SRM: srm.contract.manage."),
    ];
}
