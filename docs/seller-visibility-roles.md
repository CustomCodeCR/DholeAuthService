# Roles de visibilidad comercial

La visibilidad de solicitudes y tarifas de Ventas se separa en tres niveles:

- **Vendedor**: consulta únicamente su propia gestión mediante `pricing.rate-request.create`.
- **Vendedor Supervisor**: consulta su propia gestión y los vendedores asignados explícitamente mediante `pricing.rate-request.view-selected`.
- **Vendedor Jefe**: consulta la gestión de todos los vendedores mediante `pricing.rate-request.view-all`.

La asignación de vendedores concretos no se almacena en el rol. Pricing mantiene la relación usuario supervisor -> vendedores visibles. El scope `pricing.rate-request.visibility.manage` habilita la administración de esas relaciones.

Estos permisos son de visibilidad. No conceden por sí mismos la capacidad de aceptar o rechazar tarifas pertenecientes a otro vendedor.
