## [1.15.0] - 2026-11-19 (DH-20261119-OPERATIONS-SCOPES-CONTEXT-246 · Auth scopes y contexto empresarial Operations)

- MINOR: Operations muestra los 16 scopes con nombres explícitos en español y descripciones específicas en Auth; se conservan intactos los códigos existentes.
- Crea roles Operaciones · Administrador de equipos, Validador de Aperturas y Coordinación Origen con permisos mínimos, sin inscribir ni modificar usuarios automáticamente.
- Seed idempotente en Auth; los scopes se asignan manualmente a usuarios mediante roles existentes y las cachés de permisos se invalidan al reconciliarlos.
- DholeAuthService 1.14.0 → 1.15.0; ecosistema 1.78.0 → 1.90.0. Release coordinado con DholeOperationsService. CI/staging pendientes; producción no autorizada.

## [1.14.0] - 2026-11-17 (DH-20261117-OPS-DUAL-VALIDATION-208 · Auth Operations)

- MINOR: scopes Operations disponibles para asignación manual en Auth, sin crear cuentas, membresías ni grants nominales automáticos.
- Directorio de usuarios internos activos y consulta de elegibilidad protegidas con operations.team.manage; sin secretos ni datos de contacto.
- Auth 1.13.0 → 1.14.0; global 1.77.1 → 1.78.0.

## [1.13.0] - 2026-11-16 (SRM banking permissions · master backport)

- MINOR DH-20261116-SRM-SUPPLIER-FINANCE-176: scopes explícitos `srm.supplier.bank.read` y `srm.supplier.bank.manage` para lectura y modificación restringida de cuentas bancarias del proveedor. Release coordinado con Config 1.8.0, Web 1.63.0 y System 1.64.0.
- Backport a master de funcionalidades SRM revisadas y compiladas en staging; autorización de producción otorgada expresamente el 16 de noviembre de 2026.

## [1.11.0] - 2026-11-16 (Permisos independientes LCL propios)

- MINOR DH-20261116-OWN-LCL-SCOPES-163: separa pricing.own-lcl-consolidation.create, .update y .view para asignación independiente mediante Auth; el permiso create ya no describe autorización de edición o lectura.
- Los nuevos scopes se incorporan al seed idempotente sin otorgar privilegios operativos automáticamente a todos los roles. La administración de roles y usuarios existente permite asignarlos.
- Auth 1.9.3 → 1.11.0; Pricing 1.21.0; Web 1.61.0; global 1.62.0.

## [1.9.3] - 2026-11-13

- PATCH DH-20261113-PRODUCTION-DEPLOY-SKIP-135: elimina la condición de exclusión por mensaje del job de producción. Push a master y ejecución manual activan el despliegue estándar de Auth; se conservan CI, Docker y health.
- Release coordinado: Auth 1.9.2 → 1.9.3, CRM 1.28.2 → 1.28.3, Web 1.48.3 → 1.48.4 y ecosistema 1.48.3 → 1.48.4. CI y staging antes de promover a producción. Usuario autorizó corregir y publicar el 2026-11-13.

## [1.9.2] - 2026-11-13

- PATCH DH-20261113-CRM-COMMERCIAL-LOG-ACL-134: release coordinado que asegura visibilidad de prospectos en Bitácora CRM. Se conservan scopes `crm.bitacora.read` y `crm.bitacora.read-all`, sin ampliar permisos.
- Auth 1.9.1 → 1.9.2; CRM 1.28.1 → 1.28.2; Web 1.48.2 → 1.48.3; ecosistema 1.48.2 → 1.48.3. Producción no autorizada.

## [1.9.1] - 2026-11-13

- PATCH DH-20261113-CRM-COMMERCIAL-LOG-FILTER-133: sincronización de versión de Auth/CRM con el ajuste del filtro de tipos de Bitácora de Web; sin cambios adicionales de permisos o contratos. Mantiene REST, realtime y ACL.
- Auth 1.9.0 → 1.9.1; CRM 1.28.0 → 1.28.1; Web 1.48.0 → 1.48.2; global 1.48.2. Producción no autorizada.

## [1.9.0] - 2026-11-13

- MINOR DH-20261113-CRM-COMMERCIAL-LOG-131: scopes `crm.bitacora.read` y `crm.bitacora.read-all` para consultar registros comerciales completados, con ACL por responsable, cuenta y prospecto; compatibilidad con `crm.activity.read`.
- Auth 1.8.1 → 1.9.0; CRM 1.27.4 → 1.28.0; Web 1.47.4 → 1.48.0; ecosistema 1.48.0. Producción no autorizada.

## [1.8.1] - 2026-11-11

- PATCH DH-20261111-CRM-RESPONSIBLES-SCOPES-086: directorio de responsables CRM filtrado por scopes efectivos crm.activity.read, crm.activity.read-all o crm.followup.manage, incluyendo permisos directos, roles activos y SuperUsuario, sin exigir rol Vendedor.
- Ecosistema Dhole 1.29.1. REST/SignalR conservados. CI y staging aprobados; promoción a master solicitada y autorizada.

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
