# Changelog

## [1.3.0] - 2026-11-08 (F6.04 candidato; sin publicar)

- Auth agrega invalidaciones mínimas y sin credenciales a su Outbox transaccional cuando cambian usuario, permisos o seguridad; no transmite nombres, emails, contraseñas, roles ni claims.
- Usa TokenVersion más uno para orden de recurso, no produce eventos por accesos exitosos (heartbeat) y crea un único evento por usuario por SaveChanges. El worker enruta al stream exclusivo dhole.auth.realtime.v1.
- Cambios apagados por flags de batch y release ID; la API REST y las políticas actuales permanecen intactas. Pruebas F6.04 de identidad, versión y banderas.
- Versión candidata del componente 1.2.1 → 1.3.0, global 1.13.0. Sin merges ni despliegue.

## [1.2.1] - 2026-11-05

- Estandariza los despliegues automáticos de staging (develop) y producción (master), sin autorización manual.
- Sincroniza la versión nativa y la versión global de Dhole mediante la validación SemVer.

## [1.2.0] - 2026-11-05

- Incorporates universal Dhole SemVer policy with GitHub Actions checks.
- Synchronizes service assembly metadata with the ecosystem version.
- Adds permanent Codex instructions and coordinated release requirements.
