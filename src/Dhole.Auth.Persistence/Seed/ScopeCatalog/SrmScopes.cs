namespace Dhole.Auth.Persistence.Seed;

internal static class SrmScopes
{
    public static IReadOnlyCollection<ScopeSeedDefinition> All =>
    [
        ScopeCatalog.Create("srm.supplier.read", "srm.supplier.read", "Permiso específico de SRM: srm.supplier.read."),
        ScopeCatalog.Create("srm.supplier.create", "srm.supplier.create", "Permiso específico de SRM: srm.supplier.create."),
        ScopeCatalog.Create("srm.supplier.update", "srm.supplier.update", "Permiso específico de SRM: srm.supplier.update."),
        ScopeCatalog.Create("srm.supplier.archive", "srm.supplier.archive", "Permiso específico de SRM: srm.supplier.archive."),
        ScopeCatalog.Create("srm.performance.read", "srm.performance.read", "Permiso específico de SRM: srm.performance.read."),
        ScopeCatalog.Create("srm.performance.manage", "srm.performance.manage", "Permiso específico de SRM: srm.performance.manage."),
        ScopeCatalog.Create("srm.contract.read", "srm.contract.read", "Permiso específico de SRM: srm.contract.read."),
        ScopeCatalog.Create("srm.contract.manage", "srm.contract.manage", "Permiso específico de SRM: srm.contract.manage."),
    ];
}
