## [1.8.1] - 2026-11-11

- PATCH DH-20261111-CRM-RESPONSIBLES-SCOPES-086: directorio de responsables CRM filtrado por scopes efectivos crm.activity.read, crm.activity.read-all o crm.followup.manage, incluyendo permisos directos, roles activos y SuperUsuario, sin exigir rol Vendedor.
- Ecosistema Dhole 1.27.1. REST/SignalR conservados. CI y staging pendientes, producción no autorizada.

## [1.8.0] - 2026-11-10

- MINOR DH-20261110-CRM-ACCOUNT-CATEGORY-SCOPE-082: El catálogo de Auth registra crm.account.category.manage para autorizar la clasificación de clientes sin otorgarlo a vendedores por defecto.
- Pruebas CI, staging y autorización de producción pendientes.

## [1.3.0] - 2026-11-10

- MINOR DH-20261110-CRM-CONTACTS-ROLE-MULTISELECT-079: El directorio CRM incluye exclusivamente usuarios internos activos con rol Vendedor, Vendedores, Vendedor Supervisor, Vendedor Jefe o SuperUsuario; filtrado previo a paginación.
- Se mantienen API REST y realtime existentes. Validación por CI y staging antes de confirmar despliegue.

## [1.2.6] - 2026-11-10

- PATCH DH-20261110-CRM-SELLERS-COMMITMENTS-078: el directorio interno CRM devuelve únicamente usuarios activos con rol Vendedor/Vendedores vigente; la paginación y el filtro por nombre se aplican sobre vendedores autorizados.
- Versión global 1.20.1; conserva protección con clave de servicio, consultas REST existentes y notificaciones realtime. CI y staging pendientes.

## [1.2.5] - 2026-11-10

- PATCH DH-20261110-CRM-GLOBAL-AGENDAS-076: incorpora crm.activity.read-all al catálogo de Auth y expone por canal interno autenticado el directorio de usuarios activos para la agenda CRM; no publica el directorio a clientes sin autorización.
- Release Dhole 1.19.5; CI y staging pendientes, sin autorización de producción.

# Changelog

## [1.2.4] - 2026-11-10

- Ajuste de referencia constante AuthConstants.Scopes.UserView para compilación de asignación de prospectos.

- DH-20261110-CRM-PROSPECT-SELLER-072: agrega crm.lead.assign para asignación de prospectos en CRM; el rol Administrador recibe lectura total de prospectos, creación, edición y asignación, y acceso al directorio Auth para seleccionar vendedores.
- SuperUsuario obtiene el nuevo scope por el sembrado existente; los vendedores normales no reciben permisos adicionales. Ecoversión Dhole 1.17.16.


## [1.2.2] - 2026-11-10

- DH-20261110-CRM-QUOTE-AUTH-SCOPES-070: registra `crm.quote.read` y `crm.quote.link` en el catálogo Auth, requerido por las cotizaciones del prospecto en CRM.
- Al arrancar, el sembrado de scopes y la asignación al rol SuperUsuario actualizan los permisos efectivos e invalidan su caché; usuarios normales conservan permisos explícitos.
- Ecosistema Dhole 1.17.15; sin modificaciones a CRM ni bypass de autorización.


## [1.2.1] - 2026-11-05

- Alinea el SemVer y deploys automáticos del componente al release Dhole 1.3.1.
- Actualiza metadatos nativos y mantiene despliegues a staging por develop y producción por master.

## [1.2.0] - 2026-11-05

- Incorporates universal Dhole SemVer policy with GitHub Actions checks.
- Synchronizes service assembly metadata with the ecosystem version.
- Adds permanent Codex instructions and coordinated release requirements.
