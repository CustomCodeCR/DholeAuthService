# Changelog

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
