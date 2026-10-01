# Iniciador de GECO Caja y respaldo del puesto

## Puesta en marcha

1. Publicar `gc.caja` en el servidor con HTTPS y su configuración habitual de API.
2. Publicar el iniciador para Windows:

   ```powershell
   dotnet publish gc.caja.iniciador/gc.caja.iniciador.csproj -c Release -r win-x64 --self-contained true -p:PublishSingleFile=true -o .artifacts/iniciador
   ```

3. Copiar la publicación del iniciador a cada PC. Junto al ejecutable, colocar su `cajasettings.json` local. Conservar los campos existentes y agregar `servidor` con la URL HTTPS de Caja. Ver `cajasettings.ejemplo.json` (sus valores son ejemplos).
4. Crear un acceso directo al ejecutable y usar siempre el mismo navegador/perfil. Si el archivo se conserva en otra carpeta, el acceso directo puede indicar:

   ```text
   gc.caja.iniciador.exe --config "C:\Sitios\CajaConfig\cajasettings.json" --servidor "https://caja.suempresa.local"
   ```

El iniciador necesita permiso de lectura sobre ese archivo. No requiere permisos de administrador, servicio Windows, servidor localhost ni instalación de .NET si se publica autocontenido. Usa el nombre de Windows para identificar la PC; se puede establecer `estacionId` explícitamente en el JSON si se necesita un nombre estable diferente. `IP` conserva el significado de la configuración actual, no se reemplaza por la IP del servidor.

El certificado HTTPS del servidor debe ser confiable en los puestos. El iniciador no omite su validación.

## Flujo

El ejecutable lee y valida el JSON, entrega sólo la configuración del puesto al servidor y obtiene un pase de dos minutos y un solo uso. Abre el navegador, el servidor consume el pase y redirige al login sin conservar el pase en la URL visible. Luego el ejecutable termina. El operador se autentica y el circuito existente valida usuario, sucursal y punto de venta.

El navegador recuerda el puesto mediante una cookie protegida durante 30 días, independiente de la sesión del operador. Cerrar sesión no borra el puesto. Ejecutar de nuevo el iniciador actualiza su configuración; cambiar a otro puesto exige cerrar previamente la sesión. Esta versión supone un archivo local administrado por la empresa: no incluye certificación criptográfica de la identidad física de la PC.

`AppSettings:RutaFileCaja` ya no se utiliza en el ingreso ni para guardar productos en el servidor. Al abrir una instalación sin puesto identificado, se muestra la indicación de usar el iniciador; no se toma como alternativa el JSON del servidor.

En una instalación con varias instancias del servidor se necesita afinidad de sesión y persistir/compartir las claves de Data Protection de ASP.NET. El pase inicial vive en memoria durante dos minutos; si el servidor reinicia, ejecutar de nuevo el iniciador.

## Último detalle

IndexedDB guarda en segundo plano el código y la cantidad total en unidades de cada fila. Se reemplaza el detalle completo en una transacción al agregar, acumular o quitar productos, incluida la eliminación de la última fila. Una revisión evita que dos pestañas sobrescriban silenciosamente el mismo respaldo.

La separación es por origen web, estación, sucursal, caja y operador. El cliente no forma parte de la clave: Último detalle también permite una venta similar para otro cliente. Se conserva después del pago y al volver a identificar un cliente; el primer producto de una nueva venta sustituye el detalle anterior. Los diferimientos que antes limpiaban el respaldo siguen limpiándolo.

Recuperar exige una grilla vacía. El servidor consulta otra vez cada producto con cliente, lista autorizada, canal, descuento y sucursal actuales. La cantidad ya está en unidades: se envía con `bulto=false` y se conserva explícitamente en la respuesta de recuperación, porque el SP actual multiplica por la presentación aun con ese indicador. Los precios y demás datos del producto siguen siendo los consultados al servidor. Se omiten productos rechazados por negocio; un error de comunicación conserva el respaldo original. No se reutilizan precios ni vínculos de prefacturas/cotizaciones anteriores.

No aparecen ventanas ni descargas durante el guardado normal. Un fallo de almacenamiento muestra un aviso discreto y se intenta nuevamente al siguiente cambio. Cerrar mientras quedan escrituras pendientes o fallidas muestra la advertencia normal del navegador para evitar pérdida inadvertida.

Usar un perfil normal y mantener estable la URL del sitio. Cambiar puerto/dominio/perfil crea otro almacenamiento; borrar datos del sitio o el perfil elimina el respaldo. El guardado espera la finalización de la transacción y solicita durabilidad estricta cuando el navegador lo admite. No reemplaza un respaldo corporativo ni garantiza el último cambio si el corte ocurre antes de confirmar la escritura.

Los JSON de productos anteriores no se borran ni se importan automáticamente. Antes del cambio de versión, finalizar las operaciones en curso y conservar esos archivos para consulta. El nuevo Último detalle empieza con el primer detalle cargado en el navegador.

## Verificación reproducible

- `dotnet run --project gc.caja.iniciador -- --config <ruta> --servidor <url> --validar`: valida acceso a disco sin iniciar sesión.
- `--sin-abrir`: imprime un pase efímero para diagnóstico en lugar de abrir el navegador. No guardar ni compartir ese enlace.
- `gc.caja/Tests/Puesto.Respaldo.Servidor.Tests.ps1`: pruebas del pase, aislamiento y recuperación con API simulada.
- `node gc.caja/Tests/Respaldo.Navegador.Servir.cjs`: abre un arnés en `http://127.0.0.1:8099`, con una base IndexedDB separada. Ejecutar las pruebas, recargar y verificar persistencia.
- `node gc.caja/Tests/Respaldo.Integracion.Tests.cjs`: integración entre la grilla, recuperación, cargas masivas y limpieza.
- `node gc.caja/Tests/FacturaEmitida.Tests.cjs`: regresión de la grilla, acumulación y filas recuperadas.

No confirmar ventas ni imprimir durante las pruebas de respaldo.

## Icono del iniciador

El ejecutable incorpora el icono propio de GECO Caja (caja registradora dorada con símbolo verde de inicio). Los accesos directos creados al `.exe` lo usan automáticamente, sin distribuir un archivo de icono aparte. Si un acceso existente tiene un icono elegido manualmente, seleccionar **Propiedades → Cambiar icono** y elegir el ejecutable publicado.

El original está en `Assets/geco-caja-iniciador.png`; `Assets/geco-caja-iniciador.ico` incluye tamaños de 16 a 256 píxeles con transparencia. Para regenerar el ICO en Windows: `powershell -NoProfile -File gc.caja.iniciador/Assets/Generar-Icono.ps1`. La propiedad `ApplicationIcon` lo integra tanto al compilar como al publicar.

Diseño generado con la herramienta integrada de imágenes: caja registradora compacta dorada, pantalla verde, cuatro teclas claras y distintivo circular verde con triángulo blanco de inicio; fondo transparente, sin texto, legible en tamaños pequeños.

## Diagnóstico de la conexión

`Servidor` debe apuntar a la raíz HTTPS de **gc.caja**, incluida la subaplicación si existe (por ejemplo `https://servidor/gccaja`), sin agregar la página de login. No apunta a gc.api ni a gc.sitio.

El iniciador necesita que el servidor tenga publicada la versión que incorpora `EstacionController` y el servicio del puesto. El endpoint `POST Seguridad/Estacion/Preparar` debe responder JSON antes de autenticar al operador. Si responde una redirección al login o HTML, el iniciador lo informa y no abre el navegador: revisar la publicación y el enrutamiento en IIS/proxy. Actualizar solamente el ejecutable local no agrega este endpoint al servidor. No desactivar la autenticación general de Caja ni la validación HTTPS.

Una solicitud de diagnóstico con cuerpo `{}` a ese endpoint debe devolver **400** de validación, porque faltan los datos del puesto; una configuración válida devuelve **200** con un pase. Una respuesta **302** al login no es válida para esta etapa.
