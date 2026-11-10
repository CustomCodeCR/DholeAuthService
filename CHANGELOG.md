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
