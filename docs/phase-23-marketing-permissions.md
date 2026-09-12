# FASE 23 — Permisos de Mercadeo

Auth conserva todos los scopes CMS existentes y agrega los 14 scopes granulares definidos para Mercadeo.

El seeder crea el rol de sistema `Mercadeo` y le asigna todos los scopes activos con prefijo `cms.`. No se asignan scopes de Pricing, Auth, Config u otros microservicios automáticamente.

La asignación es idempotente: al iniciar Auth se crean scopes faltantes, se garantiza el rol y se incorporan scopes CMS nuevos sin borrar asignaciones adicionales hechas explícitamente. Los usuarios que ya tienen el rol invalidan su caché de permisos para recibir la nueva matriz.

No se requiere migración de esquema; los cambios son datos de autorización administrados por el seeder existente.
