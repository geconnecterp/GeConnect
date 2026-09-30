# Mi cuenta y contraseña en Pocket

Implementación en D:\Sis25\git\GeConnect\src. Reutiliza los servicios de seguridad de GECO y agrega la interfaz y el control de acceso de Pocket. No se modificaron stored procedures. La validación contra la base real y la aceptación visual en el colector quedan pendientes.

## Funcionalidad

- Avatar con iniciales, nombre y sucursal desde los claims de la sesión. Componente único en Inicio y módulos operativos, sin llamadas adicionales ni imágenes externas.
- Mi cuenta muestra identidad de solo lectura. El formulario permite cambiar la contraseña actual o establecer la definitiva cuando existe un cambio obligatorio.
- La política vigente se consulta al abrir la pantalla. Los requisitos y la coincidencia se muestran en el navegador; la API y los SP conservan la validación definitiva.
- Contraseña vencida o temporal: el middleware impide acceder a otras operaciones, incluso por AJAX. Permite el cambio correspondiente y cerrar sesión.
- Éxito: cierra la autenticación, elimina la cookie del JWT de Pocket y limpia la sesión. Se debe ingresar con la nueva contraseña.
- Rechazo funcional: conserva la sesión y muestra el mensaje del servidor. Ante una respuesta incierta, no reenvía automáticamente y solicita volver a ingresar.
- Desde las vistas operativas, el menú advierte antes de abrir la cuenta o cerrar sesión. Es una advertencia conservadora, no un guardado automático ni un detector de cambios específico de cada módulo.

## Integración y seguridad

Los endpoints existentes son GET api/apitoken/politica-clave, POST api/apitoken/cambio-clave y POST api/apitoken/cambio-clave-forzada.

El formulario envía exclusivamente las contraseñas y el antiforgery token al controlador de Pocket. No permite elegir usuario objetivo, sucursal ni tipo de sesión. El controlador incorpora el origen GC.POCKET en el request a la API. La API limita ese campo a canales conocidos y lo transmite al parámetro @origen ya existente; los consumidores anteriores conservan GC.SITIO como valor predeterminado.

Las cookies de autenticación y del JWT de Pocket se marcan seguras y HttpOnly. El acceso debe realizarse mediante HTTPS. Se incorpora antiforgery al login y a los cambios de contraseña.

El repositorio deja de volcar parámetros de SPGECO_USU_Clave_* en consola. Para otros SP, el formateador de diagnóstico oculta parámetros cuyo nombre contiene clave, password o token. Los diagnósticos operativos restantes se conservan.

La interfaz conserva la identidad Golden, incorpora controles de 44 px y adapta la cuenta a una columna en pantallas angostas. El avatar se integra en la cabecera compacta de TR y OR.

## Archivos principales

- gc.pocket.site/Areas/Seguridad/Controllers/CuentaController.cs
- gc.pocket.site/Models/Cuenta: identidad, reglas de acceso, middleware y limpieza de sesión.
- gc.pocket.site/Areas/Seguridad/Views/Cuenta/Index.cshtml
- gc.pocket.site/Views/Shared/_loginAuth.cshtml y _headerApp.cshtml
- gc.pocket.site/wwwroot/js/app/seguridad y estilos cuenta-pocket.css / pocket-cuenta-menu.css.
- DTOs y servicio de seguridad compartidos, ApiTokenController y diagnóstico del repositorio.

## Verificación reproducible

Desde la raíz src:

    node gc.pocket.site/Tests/Cuenta.Tests.cjs
    node gc.pocket.site/Tests/CuentaMenu.Tests.cjs
    pwsh -File gc.pocket.site/Tests/Cuenta.Servidor.Tests.ps1
    node gc.pocket.site/Tests/InventarioConteo.Tests.cjs
    node gc.pocket.site/Tests/ORColeccion.Tests.cjs

Las pruebas usan servicios y respuestas simulados. Las compilaciones de revisión se realizan en bin/cuenta-review sin arrancar la aplicación.

## Prueba de aceptación pendiente

1. Compilar y levantar Pocket y la API actualizados. Reingresar para renovar la sesión.
2. Abrir el avatar desde Inicio, TR, OR e Inventario. Comprobar menú, Volver, ausencia de recortes y altura de cabecera en escritorio y Honeywell.
3. Probar contraseña actual incorrecta, nueva fuera de política y confirmación diferente.
4. Con una cuenta de prueba, guardar un cambio correcto y verificar cierre de sesión e ingreso con la nueva contraseña.
5. Verificar contraseña vencida, cambio obligatorio y temporal vencida. Intentar una URL operativa y una petición AJAX mientras el cambio es obligatorio.
6. Comprobar doble pulsación, pérdida de conexión y aviso al salir de una carga pendiente.
7. Confirmar con la base desplegada la política, el historial, los mensajes y el registro del origen GC.POCKET.

## Límites y observaciones

- No incluye administración de usuarios, blanqueo de terceros ni edición de permisos o datos personales.
- El cierre implementado es de la sesión local de Pocket. No se implementó revocación global de otros dispositivos: la API emite credencial_version, pero queda pendiente acordar y aplicar su comprobación global.
- El transporte compartido HelperAPI existente omite la validación del certificado del servidor. No se alteró globalmente para evitar romper despliegues con certificados propios; debe corregirse coordinando certificados confiables antes de considerar certificada la seguridad del transporte.
- No se alteraron las reglas de carga de TR, OR ni Inventario, ni los SP del DBA.
