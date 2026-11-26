namespace Dhole.Auth.Persistence.Seed;

// Scope definitions are synchronized on Auth startup. Memberships are always assigned explicitly.
internal static class OperationsScopes
{
    public const string TeamManage = "operations.team.manage";
    public const string CaseRead = "operations.case.read";
    public const string CaseReadAll = "operations.case.read-all";
    public const string AuditRead = "operations.audit.read";
    public const string ApertureAssign = "operations.aperture.validation.assign";
    public const string ApertureReview = "operations.aperture.validation.review";
    public const string RealtimeRead = "operations.realtime.read";
    public const string OriginRead = "operations.origin.read";
    public const string OriginDepartureConfirm = "operations.origin.departure.confirm";

    public static IReadOnlyCollection<ScopeSeedDefinition> All =>
    [
        ScopeCatalog.Create("operations.case.create", "Operaciones · Trámites · Crear", "Permite abrir expedientes IDTRA autorizados."),
        ScopeCatalog.Create(CaseRead, "Operaciones · Trámites · Consultar asignados", "Consulta expedientes donde el usuario participa o está asignado."),
        ScopeCatalog.Create(CaseReadAll, "Operaciones · Trámites · Consultar todos", "Consulta transversal de expedientes; requiere también operations.case.read."),
        ScopeCatalog.Create(TeamManage, "Operaciones · Equipos · Administrar integrantes", "Permite configurar equipos y asignar empleados activos desde Auth. Sincroniza los scopes operativos derivados de la membresía sin modificar permisos directos."),
        ScopeCatalog.Create("operations.assignment.manage", "Operaciones · Asignaciones · Administrar", "Gestiona la asignación de etapas y responsables autorizados."),
        ScopeCatalog.Create(ApertureAssign, "Operaciones · Aperturas · Asignar dos validadores", "Selecciona dos revisores diferentes del pool activo."),
        ScopeCatalog.Create(ApertureReview, "Operaciones · Aperturas · Aprobar o solicitar cambios", "Permite emitir una decisión individual sobre la validación asignada."),
        ScopeCatalog.Create("operations.document.manage", "Operaciones · Documentos · Administrar", "Registra y gestiona versiones de documentos de expedientes autorizados."),
        ScopeCatalog.Create("operations.case.update", "Operaciones · Trámites · Editar", "Edita los datos de apertura de expedientes autorizados."),
        ScopeCatalog.Create("operations.aperture.update", "Operaciones · Aperturas · Actualizar", "Autoriza acciones operativas de actualización de apertura sujetas a acceso al expediente."),
        ScopeCatalog.Create("operations.origin.update", "Operaciones · Coordinación origen · Actualizar", "Actualiza hitos, planificación y peso real de expedientes asignados."),
        ScopeCatalog.Create("operations.cost.read", "Operaciones · Costos · Consultar", "Consulta costos del expediente autorizado sin autorizar ventas o márgenes."),
        ScopeCatalog.Create("operations.finance.manage", "Operaciones · Finanzas · Consultar información comercial", "Autoriza datos de venta y condiciones comerciales en expedientes autorizados."),
        ScopeCatalog.Create(OriginRead, "Operaciones · Coordinación origen · Consultar", "Permite consultar el avance de origen de expedientes autorizados."),
        ScopeCatalog.Create(OriginDepartureConfirm, "Operaciones · Coordinación origen · Confirmar salida", "Permite confirmar hitos de salida según las reglas operativas."),
        ScopeCatalog.Create("operations.internal-note.read", "Operaciones · Notas internas · Consultar", "Consulta las notas internas de expedientes autorizados."),
        ScopeCatalog.Create("operations.internal-note.create", "Operaciones · Notas internas · Crear", "Agrega notas internas con trazabilidad de autor."),
        ScopeCatalog.Create("operations.carrier.connection.read", "Operaciones · Navieras · Consultar integraciones", "Consulta el catálogo de conexiones marítimas."),
        ScopeCatalog.Create("operations.carrier.connection.manage", "Operaciones · Navieras · Administrar integraciones", "Administra capacidades y conexiones autorizadas."),
        ScopeCatalog.Create(AuditRead, "Operaciones · Auditoría · Consultar", "Permite consultar eventos auditados de equipos y expedientes; no autoriza modificaciones."),
        ScopeCatalog.Create(RealtimeRead, "Operaciones · Tiempo real · Suscribirse", "Autoriza acceso a notificaciones SignalR de expedientes visibles para el usuario.")
    ];
}
