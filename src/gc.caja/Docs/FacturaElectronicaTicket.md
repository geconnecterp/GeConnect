# Facturación electrónica en ticket de 80 mm

Implementación inicial del 2026-11-21, limitada al formato B. Referencia funcional: BitacoraUnicaReglasCaja.md, EMI-20 a EMI-22.

## Configuración y precedencia

El contexto del SP `SPGECO_CAJA_DATOS` incorpora `ctrl_papel`: `A4`, `TK` o `EM`. El iniciador admite una preferencia opcional del puesto en su JSON local:

```json
"impresionFormatoFE": "TK",
"impresionModo": "IMPRESORA"
```

`impresionFormatoFE` admite A4/TK; se normaliza al leer el archivo y se valida tanto en el iniciador como al preparar el puesto. Se transmite por el pase de un solo uso, sesión y cookie protegida existentes. Para conservar la elección del SP, omitir `impresionFormatoFE`. Prioridad: preferencia explícita del puesto, luego `ctrl_papel` de DATOS; si faltan ambos, A4 por compatibilidad. Un código desconocido informa error.

Son decisiones diferentes: `facturacion` y `ctrl_id` determinan la modalidad fiscal; `impresionFormatoFE`/`ctrl_papel` determinan la representación; `impresionModo` determina pantalla o impresora. La preferencia afecta sólo al PDF FE, no al acuse de Custodia ni a los recibos.

El CASE provisorio del SP aportado devuelve TK para ctrl_id=0. Esto no confirma que 0 sea FE: Caja conserva -1 como FE y 50 como controlador fiscal. Para probar ticket con el contrato actual, mantener el identificador real de FE y seleccionar TK en el puesto. La identificación definitiva del controlador y el modelo de ctrl_papel quedan para el analista/DBA. No se modificó ningún SP ni se reclasificó 0.

`EM` queda reconocido en el contrato del SP. Si resulta seleccionado, se informa que el circuito de email requiere definición; no se imprime ni se anuncia un email enviado. Un puesto con preferencia explícita A4/TK utiliza esa preferencia. La plantilla A en ticket queda para el siguiente incremento: una petición TK para A informa que el formato aún no está disponible.

## Diseño y datos

- Papel PDF de 80 mm, orientación nativa, márgenes laterales de 4 mm y largo calculado según el contenido.
- Logo de empresa configurado en Reportes, emisor, comprobante, cliente y detalle; descripción en líneas de hasta 44 caracteres sin descartar el texto restante.
- Orden numérico de cmd_item, estable para los registros que no lo informan. Cantidades, precio final cmd_pvta e importe cmd_subtotal_con_iva se toman del SP.
- Bonificaciones, percepciones, total, IVA e impuestos internos conservan los valores y signos recibidos. La fila II de FE_PER se informa mediante el importe de impuestos internos del encabezado, sin sumarla otra vez al total.
- El ticket no calcula el vuelto ni muestra pagos recibidos que no están en estos SP. El bloque RECIBIMOS/CAMBIO de las referencias requiere su contrato de datos si se desea incorporarlo.
- CAE/vencimiento y QR de la FE registrada al pie. El ticket exige autorización informada y un QR generable; no utiliza valores ficticios de respaldo. Se reutiliza el constructor QR existente después de validar esos campos.
- Tickets extremadamente largos se dividen por bloques completos en páginas PDF de menos de 200 pulgadas, con cabecera de continuación y totales/QR en la última. No se omiten artículos.

R068 conserva el selector existente del reporte B y recibe `Parametros.ctrl_papel`. La petición se construye en servidor desde el contexto inicial; no se incorpora una elección libre de papel al request del navegador ni una consulta SQL adicional por impresión. Las llamadas de otros consumidores sin ctrl_papel mantienen A4.

El SP FE_D aportado calcula cmd_subtotal_con_iva desde cmd_pvta. Se corrige también la representación del unitario en B/A4, que sumaba IVA de nuevo. Se utiliza cmd_pvta directamente; no se recalculan subtotales ni totales fiscales. La distribución A4 permanece con su paginación existente.

## Publicación y aceptación

Publicar gc.api para devolver el DTO con ctrl_papel, gc.api.report/GCReportes para generar el formato, gc.caja para resolverlo e iniciador para transmitir la preferencia. Incluir las dependencias de cada publicación. No requiere cambios en gc.sitio ni Pocket. Si sólo se usa la elección del SP y el iniciador anterior, omitir la preferencia local.

Tras cambiar el JSON o actualizar la configuración del puesto, cerrar sesión y abrir nuevamente mediante el iniciador. Una sesión anterior puede conservar DATOS anterior. El campo no se configura en el appsettings del servidor web.

Antes de activar impresión silenciosa, instalar el controlador del modelo de comandera y comprobar el papel de 80 mm tanto en Windows como en la vista previa de Chrome. Se mantiene la impresora predeterminada de Windows. La selección de destino no garantiza que Chrome use el tamaño correcto. Validar una copia manual y otra automática del mismo PDF, con logo, todos los productos, importes y QR legible; no emitir otra factura para probar la impresión.

La marca/modelo todavía no se conoce. Los 4 mm de margen y el largo PDF son una base de prueba, no una aceptación del avance/corte físico de papel. Comprobar especialmente tickets extensos y el manejo de tamaño personalizado por el controlador. No se cambian políticas, impresora predeterminada ni preferencias de Chrome desde la aplicación.

## Diagnóstico y pruebas

El log [REPORTES][FORMATO_FE] muestra caja, sucursal, ctrl_id, ctrl_papel, preferencia, formato resuelto y fuente. El request existente [REPORTES][REQUEST] incluye Parametros.ctrl_papel como JSON legible. [TICKET_B] registra cantidad de productos, ancho y páginas; no registra el contenido PDF. Un error de salida conserva el registro de la operación y permite revisar/reintentar sólo su impresión.

Se compilaron Caja, API, Reportes e iniciador. Se ejecutaron 106 verificaciones privadas de contratos, precedencia, controlador con servicios sustituidos, conservación del pase/sesión/cookie y errores; seis invocaciones reales --validar del iniciador. Se generaron 16 PDFs con datos ficticios: B de 1, 2, 27, 100 y 500 productos, normales/extensos, signos, importes máximos y A4 de A/B con/sin parámetro. Se verificaron ancho, márgenes, ausencia de giro y recorte, identidad/orden de productos y QR embebido. Los QR de dos imágenes a 203 dpi se decodificaron correctamente; los A4 con/sin parámetro conservaron igual texto y se contrastó el unitario B. Se inspeccionaron las imágenes de los tickets.

Pruebas, datos y evidencias están en .artifacts/ticket-b-20261121, excluidos de Git. No se invocaron SP, no se emitieron comprobantes, no se confirmaron pagos ni se enviaron impresiones. Pendientes: prueba integrada con DATOS real y sesión recompilada, publicación y aceptación física en la comandera elegida; ticket A y email.

Referencia técnica del QR: https://www.arca.gob.ar/fe/qr/documentos/QRespecificaciones.pdf
