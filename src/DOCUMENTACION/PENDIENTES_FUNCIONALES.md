# GECO - Registro de funcionalidades pendientes

Este documento centraliza funcionalidades futuras o diferidas para evitar que se pierdan entre las correcciones de los distintos módulos. No reemplaza el análisis funcional ni la autorización de implementación de cada punto.

## Seguridad y gestión de usuarios

### SEC-001 - Blanqueo administrativo de contraseña

- **Estado:** Implementado en código; pendiente de instalación DBA y validación funcional.
- **Sistema inicial:** `gc.sitio`.
- **Alcance:** Reemplazar la antigua opción "Imprimir CARD" por "Blanquear clave".
- **Reglas acordadas:**
  - Requiere un derecho específico de seguridad.
  - El operador no puede blanquear su propia contraseña.
  - La operación debe quedar auditada indicando usuario afectado y operador ejecutor.
  - El blanqueo no debe desbloquear automáticamente al usuario.
  - La contraseña temporal debe obligar a definir una nueva contraseña en el siguiente ingreso.
  - No se utilizará el mecanismo especial `##newuser##` como forma de autenticación.

### SEC-002 - Cambio obligatorio posterior al blanqueo

- **Estado:** Implementado en código; pendiente de instalación DBA y validación funcional.
- **Alcance:** Vista exclusiva para ingresar y confirmar una nueva contraseña después de autenticarse con una credencial temporal.
- **Reglas acordadas:**
  - No habilitar el menú ni otros módulos mientras el cambio sea obligatorio.
  - Aplicar la política de longitud, complejidad y vigencia almacenada en la configuración de seguridad.
  - Cerrar la sesión al completar el cambio y exigir una nueva autenticación.
  - La marca de cambio obligatorio debe ser explícita y no inferirse desde textos, contraseñas conocidas o eventos de auditoría.

### SEC-003 - Desbloqueo administrativo de usuario

- **Estado:** Implementado en código; pendiente de instalación DBA y validación funcional.
- **Alcance:** Nueva acción en Gestión de Usuarios.
- **Reglas acordadas:**
  - Requiere un derecho específico, independiente del derecho de blanqueo.
  - No permite operar sobre el propio usuario.
  - Solo se habilita cuando el usuario seleccionado está bloqueado.
  - Debe limpiar el estado de bloqueo y los intentos fallidos que correspondan, conservando auditoría.
  - No modifica la contraseña del usuario.

### SEC-004 - Verificación de pertenencia del email

- **Estado:** Futuro.
- **Alcance:** Validar que el email informado pertenece realmente al usuario.
- **Flujo preliminar:**
  - Enviar un código de un solo uso al email informado.
  - Solicitar el código en una vista de verificación.
  - Registrar destino normalizado, fecha, vencimiento, intentos y resultado.
  - Guardar el código únicamente mediante representación segura; nunca en texto plano.
  - Distinguir email informado de email verificado.

### SEC-005 - Verificación de pertenencia del celular

- **Estado:** Futuro.
- **Alcance:** Validar el número mediante un código de un solo uso enviado por SMS.
- **Reglas preliminares:** Equivalentes a SEC-004, incluyendo vencimiento, límite de intentos, auditoría y protección del código.
- **Dependencia externa:** Definición del proveedor de SMS y sus costos/licencias.

### SEC-006 - Entrega segura de credenciales temporales

- **Estado:** Futuro; dependiente de SEC-004 y/o SEC-005.
- **Alcance:** Sustituir la contraseña temporal común por una credencial individual de un solo uso, enviada a un canal previamente verificado.
- **Alternativas previstas:** Email verificado, celular verificado o ambos según política.

### SEC-007 - Invalidación general de sesiones por cambios de seguridad

- **Estado:** Futuro.
- **Alcance:** Invalidar sesiones existentes cuando se cambia o blanquea una contraseña, considerando `gc.sitio`, Pocket, Caja, Autorizaciones y demás clientes.
- **Nota:** Debe integrarse con la definición general de sesiones simultáneas y restricciones particulares por sistema.

## Plataforma

### PLA-001 - Política general de sesiones y múltiples pestañas

- **Estado:** Pendiente de definición transversal.
- **Alcance:** Tiempo parametrizable, cierre por expiración, múltiples pestañas, múltiples navegadores y restricciones específicas por aplicación.
- **Criterio preliminar:** `gc.sitio` puede admitir concurrencia aislada; Caja y otros sistemas sensibles pueden requerir exclusividad.

### PLA-002 - Configuración de reportes administrada en base de datos

- **Estado:** Analizado; pendiente de ejecución planificada.
- **Alcance:** Migración transparente de las configuraciones de reportes desde archivos hacia tablas y procedimientos propios, cargando por módulo solo los reportes disponibles para su sesión.

## Criterios de mantenimiento del registro

- Incorporar cada nueva funcionalidad diferida con un identificador estable.
- Registrar dependencias, riesgos y sistema afectado.
- Cambiar el estado a **En análisis**, **En ejecución**, **Validación** o **Completado** cuando corresponda.
- No registrar secretos, contraseñas, tokens, códigos reales ni datos personales.
