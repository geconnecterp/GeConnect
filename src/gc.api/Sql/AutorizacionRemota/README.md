# Autorización remota - scripts SQL

Los scripts se ejecutan sobre la base corporativa `geco_0000`.

## Instalación limpia

1. Ejecutar `01_Instalacion_Limpia.sql`.
2. Ejecutar `03_Procedimientos.sql`.

La instalación limpia elimina las tablas transaccionales del módulo y todos sus datos.

## Migración desde el catálogo anterior

1. Realizar un respaldo de la base.
2. Ejecutar `02_Migracion_DerCodigo.sql`.
3. Ejecutar `03_Procedimientos.sql`.

La migración conserva las solicitudes y resoluciones. Antes de eliminar
`sauth_TiposAutorizacion`, traduce `CodigoTipo` a `DerCodigo` utilizando el
`der_codigo` configurado en el catálogo anterior. Si encuentra solicitudes sin
una equivalencia válida, cancela toda la transacción.
