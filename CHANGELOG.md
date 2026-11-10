# Changelog

## [1.3.0] - 2026-11-10

- MINOR DH-20261110-CRM-ADMIN-CALENDAR-069: los administradores activos de Auth pueden consultar agendas, actividades, seguimientos, etiquetas y usuarios de todas las agendas CRM; vendedores conservan su aislamiento. El detalle de seguimientos periódicos muestra un título real y datos de la política existente, manteniendo minutas y acuerdos históricos cuando los hay.
- La autorización de lectura está validada desde Auth; crear, completar, cancelar y reprogramar mantienen permisos de escritura existentes. Componentes coordinados: Auth 1.3.0, CRM 1.5.0, Web 1.18.0, System 1.18.0. Staging/CI pendientes, producción no desplegada.

## [1.2.1] - 2026-11-05

- Alinea el SemVer y deploys automáticos del componente al release Dhole 1.3.1.
- Actualiza metadatos nativos y mantiene despliegues a staging por develop y producción por master.

## [1.2.0] - 2026-11-05

- Incorporates universal Dhole SemVer policy with GitHub Actions checks.
- Synchronizes service assembly metadata with the ecosystem version.
- Adds permanent Codex instructions and coordinated release requirements.
