namespace Dhole.Auth.Persistence.Seed;
// Operations users and grants are never seeded; only the catalogue is synchronized.
internal static class OperationsScopes
{
    public static IReadOnlyCollection<ScopeSeedDefinition> All =>
    [
        ScopeCatalog.Create("operations.case.create", "Operations · case.create", "Permiso Operations: operations.case.create."),
        ScopeCatalog.Create("operations.case.read", "Operations · case.read", "Permiso Operations: operations.case.read."),
        ScopeCatalog.Create("operations.case.read-all", "Operations · case.read-all", "Permiso Operations: operations.case.read-all."),
        ScopeCatalog.Create("operations.team.manage", "Operations · team.manage", "Permiso Operations: operations.team.manage."),
        ScopeCatalog.Create("operations.assignment.manage", "Operations · assignment.manage", "Permiso Operations: operations.assignment.manage."),
        ScopeCatalog.Create("operations.aperture.validation.assign", "Operations · aperture.validation.assign", "Permiso Operations: operations.aperture.validation.assign."),
        ScopeCatalog.Create("operations.aperture.validation.review", "Operations · aperture.validation.review", "Permiso Operations: operations.aperture.validation.review."),
        ScopeCatalog.Create("operations.document.manage", "Operations · document.manage", "Permiso Operations: operations.document.manage."),
        ScopeCatalog.Create("operations.origin.read", "Operations · origin.read", "Permiso Operations: operations.origin.read."),
        ScopeCatalog.Create("operations.origin.departure.confirm", "Operations · origin.departure.confirm", "Permiso Operations: operations.origin.departure.confirm."),
        ScopeCatalog.Create("operations.internal-note.read", "Operations · internal-note.read", "Permiso Operations: operations.internal-note.read."),
        ScopeCatalog.Create("operations.internal-note.create", "Operations · internal-note.create", "Permiso Operations: operations.internal-note.create."),
        ScopeCatalog.Create("operations.carrier.connection.read", "Operations · carrier.connection.read", "Permiso Operations: operations.carrier.connection.read."),
        ScopeCatalog.Create("operations.carrier.connection.manage", "Operations · carrier.connection.manage", "Permiso Operations: operations.carrier.connection.manage."),
        ScopeCatalog.Create("operations.audit.read", "Operations · auditoría · Consultar", "Permite leer el historial de equipos y expedientes de Operations. No concede permisos de modificación."),
        ScopeCatalog.Create("operations.realtime.read", "Operations · realtime.read", "Permiso Operations: operations.realtime.read."),
    ];
}
