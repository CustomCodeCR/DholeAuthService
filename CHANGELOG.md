## [1.9.3] - 2026-11-13

- PATCH DH-20261113-PRODUCTION-DEPLOY-SKIP-135: release coordinado para habilitar el job de producción de Auth en master; la rama develop ya carecía de exclusiones por mensaje de commit. Se preservan Docker, health, REST y realtime.
- Auth 1.9.2 → 1.9.3, CRM 1.28.2 → 1.28.3, Web 1.48.3 → 1.48.4, ecosistema 1.48.3 → 1.48.4. CI y staging requeridos; publicación solicitada por usuario el 2026-11-13.

## [1.9.2] - 2026-11-13

- PATCH DH-20261113-CRM-COMMERCIAL-LOG-ACL-134: release coordinado que refuerza la ACL de prospectos en la Bitácora CRM. Sin cambios adicionales de permisos: se conservan `crm.bitacora.read` y `crm.bitacora.read-all`.
- Auth 1.9.1 → 1.9.2; CRM 1.28.1 → 1.28.2; Web 1.48.2 → 1.48.3; ecosistema 1.48.2 → 1.48.3. CI y staging pendientes; producción no autorizada.

## [1.9.1] - 2026-11-13

- PATCH DH-20261113-CRM-COMMERCIAL-LOG-FILTER-133: sincronización universal de versionamiento para completar el release de Bitácora CRM (filtros de tipo visibles desde el primer acceso) tras la integración inicial 131, sin nuevas mutaciones en permisos o contratos del servicio.
- Auth 1.9.0 → 1.9.1; CRM 1.28.0 → 1.28.1; Web 1.48.0 → 1.48.2; ecosistema 1.48.0 → 1.48.2. Producción no autorizada.

## [1.9.0] - 2026-11-13

- MINOR DH-20261113-CRM-COMMERCIAL-LOG-131: scopes `crm.bitacora.read` y `crm.bitacora.read-all` para lectura de actividades comerciales completadas con ACL por vendedor, cliente y prospecto. Compatibilidad con `crm.activity.read` y `crm.activity.read-all` existentes; no se autoconceden permisos elevados.
- Auth 1.8.1 → 1.9.0; CRM 1.27.4 → 1.28.0; Web 1.47.4 → 1.48.0; ecosistema → 1.48.0. CI/staging pendientes, producción no autorizada.

## [1.8.1] - 2026-11-11

- PATCH DH-20261111-CRM-RESPONSIBLES-SCOPES-086: el directorio interno de responsables CRM filtra usuarios internos activos por crm.activity.read, crm.activity.read-all o crm.followup.manage, incluyendo permisos directos, heredados de roles activos y superusuarios. Ya no depende del nombre del rol Vendedor.
- Ecosistema Dhole 1.28.1; REST y SignalR conservados. CI/staging pendientes. Despliegue productivo no autorizado.

## [1.8.0] - 2026-11-10

- MINOR DH-20261110-CRM-ACCOUNT-CATEGORY-SCOPE-082: El catálogo de Auth registra crm.account.category.manage para autorizar la clasificación de clientes sin otorgarlo a vendedores por defecto.
- Pruebas CI, staging y autorización de producción pendientes.

## [1.7.0] - 2026-11-10

- MINOR DH-20261110-CRM-CONTACTS-SELLERS-078: amplía el directorio filtrado de vendedores para contemplar Vendedor, Vendedores, Vendedor Supervisor, Vendedor Jefe y SuperUsuario (solo usuarios internos activos).
- No cambia autorizaciones REST ni credenciales; continúa el filtrado seguro introducido en 1.6.4.

## [1.6.4] - 2026-11-10

- PATCH DH-20261110-CRM-SELLERS-COMMITMENTS-078: el directorio interno CRM devuelve únicamente usuarios activos con rol Vendedor/Vendedores vigente; la paginación y el filtro por nombre se aplican sobre vendedores autorizados.
- Versión global 1.20.1; conserva protección con clave de servicio, consultas REST existentes y notificaciones realtime. CI y staging pendientes.

## [1.6.3] - 2026-11-10

- PATCH DH-20261110-CRM-GLOBAL-AGENDAS-076: registra crm.activity.read-all en el catálogo Auth; SuperUsuario recibe el permiso mediante scopes efectivos. La lectura global adicional por combinación crm.account.read-all + crm.lead.read-all se valida en CRM. No amplía el directorio público.
- Release Dhole 1.19.5; compilación, CI y staging pendientes.

# Changelog

## [1.6.2] - 2026-11-10

- DH-20261110-CRM-PROSPECT-SELLER-072: agrega crm.lead.assign para asignación de prospectos en CRM; el rol Administrador recibe lectura total de prospectos, creación, edición y asignación, y acceso al directorio Auth para seleccionar vendedores.
- SuperUsuario obtiene el nuevo scope por el sembrado existente; vendedores normales sin cambios. Ecosistema Dhole 1.18.2.


## [1.6.1] - 2026-11-10

- DH-20261110-CRM-QUOTE-AUTH-SCOPES-070: registra `crm.quote.read` y `crm.quote.link` requeridos por cotizaciones vinculadas de prospectos CRM y Pricing.
- El seeder de Auth incorpora ambos scopes al rol SuperUsuario e invalida permisos efectivos en caché tras iniciar; sin bypass para usuarios normales.
- Release Dhole 1.18.0 coordinado, sin alterar los cambios de agendas/administradores de 1.6.0.


## [1.6.0] - 2026-11-10

- MINOR DH-20261110-CRM-ADMIN-CALENDAR-069: los administradores activos de Auth pueden consultar agendas, actividades, seguimientos, etiquetas y usuarios de todas las agendas CRM; vendedores conservan su aislamiento. El detalle de seguimientos periódicos muestra un título real y datos de la política existente, manteniendo minutas y acuerdos históricos cuando los hay.
- La autorización de lectura está validada desde Auth; crear, completar, cancelar y reprogramar mantienen permisos de escritura existentes. Componentes coordinados: Auth 1.6.0, CRM 1.8.0, Web 1.18.0, System 1.18.0. Staging/CI pendientes, producción no desplegada.

## [1.5.0] - 2026-11-09 (candidate: realtime crítico Auth/Notifications)

- MINOR: emitir eventos de seguridad por Outbox transaccional en cambios de usuario, bloqueos, contraseñas, permisos y revocación de sesiones concretas. Redis Streams mantiene evento minimal sin tokens ni datos privados.
- Ecosistema candidato 1.17.0, release `DH-20261109-RT-CRITICAL-056`. Sin merge a develop/master, despliegue o certificación WSS multiusuario. CI y staging pendientes.


## [1.4.0] - 2026-11-09 (NotificationsHub universal realtime pilot; candidate)

- MINOR: Preserve existing producer contract and Outbox/REST integration for the NotificationsHub-based pilot.
- Ecosystem candidate 1.14.0 / 16 Dhole repositories; CustomCodeFramework excluded. Release `rt-20261107-01`, isolated branch `feature/rt-notificationshub-20261109-01`.
- Staging compile, deployment and post-deploy security audit pending; `develop`, `master` and production not changed by this candidate.


## [1.3.6] - 2026-11-08 (Dhole realtime coordinated hardening)

- PATCH 1.13.15 ecosystem release alignment with F2/F6 AOF read-only checks and F10 branch-preservation fix. Service candidate unchanged; realtime flags OFF and gates F6–F10 not formally approved.



## [1.3.5] - 2026-11-08 (F9 candidate)

- PATCH de versionamiento universal Dhole 1.13.14. Excepción F3 limitada a revisión de código con pruebas TLS/WSS aisladas; sin aprobación de staging, cambios funcionales ni despliegue.



## [1.3.4] - 2026-11-08 (release candidato F3)

- Actualización PATCH de metadatos y versión global 1.13.13 asociada al ensayo automático HTTPS/WSS de Gateway, sin cambios funcionales adicionales; ramas principales intactas.



## [1.3.3] - 2026-11-08 (F6/F9 staging-audit candidate)

- Sincroniza Dhole Realtime `rt-20261107-01` a versión global `1.13.12` tras incorporar auditoría real de persistencia en staging. PATCH 1.3.2 → 1.3.3; no modifica rutas, eventos, datos, contratos REST ni flags. No se fusiona hasta completar gates formales.



## [1.3.2] - 2026-11-08 (candidato F9, sin publicar)

- Sincroniza release global `rt-20261107-01` a `1.13.11` tras resolver los hallazgos de CI de Config, Reports y Gateway. Bump PATCH de DholeAuthService `1.3.1` a `1.3.2`, sin modificar REST, eventos, precios ni datos; flags realtime OFF y PR F9 aún DRAFT.



## [1.3.1] - 2026-11-08 (candidato F9, sin publicación)

- Sincroniza metadatos de versión global del batch Dhole Realtime `rt-20261107-01` a `1.13.10`; actualización PATCH independiente de DholeAuthService `1.3.0` → `1.3.1`. No cambia contratos REST, datos, lógica de negocio ni flags; requiere gates F0–F8 y CI de SHA final antes de merge.



## [1.3.0] - 2026-11-08 (F6.04 candidato; sin publicar)

- Auth agrega invalidaciones mínimas y sin credenciales a su Outbox transaccional cuando cambian usuario, permisos o seguridad; no transmite nombres, emails, contraseñas, roles ni claims.
- Usa TokenVersion más uno para orden de recurso, no produce eventos por accesos exitosos (heartbeat) y crea un único evento por usuario por SaveChanges. El worker enruta al stream exclusivo dhole.auth.realtime.v1.
- Cambios apagados por flags de batch y release ID; la API REST y las políticas actuales permanecen intactas. Pruebas F6.04 de identidad, versión y banderas.
- Versión candidata del componente 1.2.1 → 1.3.0, global 1.13.0. Sin merges ni despliegue.


## [1.2.1] - 2026-11-05

- Estandariza los despliegues automáticos de staging (develop) y producción (master), sin autorización manual.
- Sincroniza la versión nativa y la versión global de Dhole mediante la validación SemVer.


### Notas conservadas de master


- Alinea el SemVer y deploys automáticos del componente al release Dhole 1.3.1.
- Actualiza metadatos nativos y mantiene despliegues a staging por develop y producción por master.


## [1.2.0] - 2026-11-05

- Incorporates universal Dhole SemVer policy with GitHub Actions checks.
- Synchronizes service assembly metadata with the ecosystem version.
- Adds permanent Codex instructions and coordinated release requirements.
