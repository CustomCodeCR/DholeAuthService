# Changelog

## [1.3.0] - 2026-11-10

- MINOR DH-20261110-CRM-ADMIN-CALENDAR-069: Auth interno transmite roles vigentes y el directorio activo requerido para consulta de todas las agendas CRM por administradores. Preserva los permisos Auth del release 1.2.2.
- Ecosistema 1.18.0; CI y staging pendientes, sin autorización de producción.

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
