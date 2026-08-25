# Blanqueo de contraseña y desbloqueo — entrega al DBA

## Objetivo

Incorporar el blanqueo administrativo de contraseña, el cambio obligatorio posterior y el desbloqueo de usuarios. La migración es incremental e idempotente: no elimina datos ni modifica contraseñas existentes durante su instalación.

## Orden de despliegue

1. Confirmar que ya fue instalado `01_Cambio_Clave.sql`.
2. Ejecutar `02_Blanqueo_Clave_Desbloqueo.sql` sobre `geco_0000`.
3. Crear o identificar en el catálogo corporativo dos derechos independientes:
   - Blanquear contraseña.
   - Desbloquear usuario.
4. Informar sus valores `der_codigo` en `seguridad_configuracion`:

```sql
UPDATE dbo.seguridad_configuracion
SET seg_derecho_blanquear_clave = '<DER_CODIGO_BLANQUEO>',
    seg_derecho_desbloquear_usuario = '<DER_CODIGO_DESBLOQUEO>',
    seg_usuario_modificacion = '<USUARIO_DBA>',
    seg_fecha_modificacion = SYSDATETIME()
WHERE seg_id = 1;
```

5. Asignar cada derecho a los perfiles o usuarios autorizados mediante el mecanismo habitual de GECO.
6. Configurar la contraseña temporal únicamente en el entorno seguro de la API con la clave `SecurityOperations__TemporaryPassword`. No guardar su valor en Git ni en este documento.
7. Publicar/reiniciar primero la API y luego `gc.sitio`.

## Artefactos incorporados

### Columnas de `usuarios`

- `usu_cambio_clave_obligatorio`
- `usu_cambio_clave_motivo`
- `usu_cambio_clave_fecha`
- `usu_cambio_clave_vencimiento`
- `usu_cambio_clave_operacion_id`
- `usu_version_credencial`

### Columnas auxiliares

- `usuarios_auditoria.usa_usu_ejecutor`
- `seguridad_configuracion.seg_clave_temporal_vigencia_horas`
- `seguridad_configuracion.seg_derecho_blanquear_clave`
- `seguridad_configuracion.seg_derecho_desbloquear_usuario`

### Procedimientos

- `SPGECO_SEG_Configuracion_Obtener` — ampliado.
- `SPGECO_USU_Seguridad_Estado`
- `SPGECO_USU_Clave_Blanquear`
- `SPGECO_USU_Clave_Forzada_Cambiar`
- `SPGECO_USU_Desbloquear`

## Controles posteriores

```sql
SELECT seg_clave_temporal_vigencia_horas,
       seg_derecho_blanquear_clave,
       seg_derecho_desbloquear_usuario
FROM dbo.seguridad_configuracion
WHERE seg_id = 1;

SELECT name
FROM sys.procedures
WHERE name IN
(
    'SPGECO_USU_Seguridad_Estado',
    'SPGECO_USU_Clave_Blanquear',
    'SPGECO_USU_Clave_Forzada_Cambiar',
    'SPGECO_USU_Desbloquear'
);
```

## Reglas preservadas

- El blanqueo y el desbloqueo son operaciones independientes.
- El operador no puede actuar sobre sí mismo.
- La API vuelve a comprobar los derechos; ocultar el botón no es el control de seguridad.
- La contraseña temporal vence según configuración y exige una nueva contraseña.
- El cambio definitivo continúa la misma identificación de operación iniciada por el blanqueo.
- Se auditan usuario afectado, operador, sucursal, origen, IP, resultado y operación.
- No se crean derechos automáticamente porque el catálogo y sus asignaciones son responsabilidad del DBA.
