# Facturación electrónica en ticket de 80 mm

Implementación inicial B del 2026-11-21; ticket A incorporado el 2026-11-26. Referencia funcional: BitacoraUnicaReglasCaja.md, EMI-20 a EMI-31. El diseño común vigente se describe al final de este documento.

## Configuración y precedencia

El contexto del SP `SPGECO_CAJA_DATOS` incorpora `ctrl_papel`: `A4`, `TK` o `EM`. El iniciador admite una declaración opcional del papel físico del puesto en su JSON local:

```json
"impresionFormatoFE": "TK",
"impresionModo": "IMPRESORA"
```

Regla confirmada el 2026-11-26: **`ctrl_papel` de DATOS decide el formato**. `impresionFormatoFE` admite A4/TK y ahora declara el papel físico preparado por el instalador, sólo como segundo control. Conserva el nombre JSON para no romper los iniciadores existentes, pero ya no selecciona ni reemplaza el formato del SP. Se normaliza al leer el archivo, se valida al preparar el puesto y se transporta por el pase de un solo uso, sesión y cookie protegida existentes.

Al recibir DATOS, si GECO indica A4 y el puesto declara TK, o a la inversa, se presenta un aviso informativo antes del menú y queda visible en la portada, menú y vistas autenticadas que comparten el layout. El aviso indica ambos valores y que se mantendrá el formato de GECO. No bloquea la operación, cambia la impresora ni agrega otra confirmación al cobro. Una nueva lectura que coincide limpia el aviso anterior. El control no se aplica al controlador fiscal; EM es un canal de envío, no un papel físico.

Sin declaración local se utiliza el SP, sin poder contrastar el papel instalado. Si DATOS no informa papel, se conserva A4 por compatibilidad, nunca se toma TK del puesto como reemplazo; con declaración local se avisa que falta el dato del servidor. Un código de SP desconocido informa error y no se sustituye por el dato local. Este contraste usa configuración declarada: **no inspecciona el hardware, la cola de Windows ni las preferencias efectivas de Chrome**.

Son decisiones diferentes: `facturacion` y `ctrl_id` determinan la modalidad fiscal; `ctrl_papel` determina la representación y `impresionFormatoFE` sólo la contrasta con el papel físico declarado; `impresionModo` determina pantalla o impresora. El contraste afecta sólo al circuito FE, no al acuse de Custodia ni a los recibos.

El CASE provisorio del SP aportado devuelve TK para ctrl_id=0. Esto no confirma que 0 sea FE: Caja conserva -1 como FE y 50 como controlador fiscal. Para probar ticket, DATOS debe informar TK y conservar el identificador real de FE. Declarar TK en el puesto no fuerza ese formato si el SP informa A4. La identificación definitiva del controlador y el modelo de ctrl_papel quedan para el analista/DBA. No se modificó ningún SP ni se reclasificó 0.

`EM` queda reconocido en el contrato del SP. Si resulta seleccionado, se informa que el circuito de email requiere definición; no se imprime ni se anuncia un email enviado. TK admite comprobantes de letra A y B; otros tipos informan formato no disponible.

## Diseño y datos

- Papel PDF de 80 mm, orientación nativa, márgenes laterales de 4 mm y largo calculado según el contenido.
- Logo de empresa configurado en Reportes, emisor, comprobante, cliente y detalle; descripción en líneas de hasta 44 caracteres sin descartar el texto restante.
- Orden numérico de cmd_item, estable para los registros que no lo informan. Cantidades, precio final cmd_pvta e importe cmd_subtotal_con_iva se toman del SP.
- Bonificaciones, percepciones, total, IVA e impuestos internos conservan los valores y signos recibidos. La fila II de FE_PER se informa mediante el importe de impuestos internos del encabezado, sin sumarla otra vez al total.
- El ticket no calcula el vuelto ni muestra pagos recibidos que no están en estos SP. El bloque RECIBIMOS/CAMBIO de las referencias requiere su contrato de datos si se desea incorporarlo.
- CAE/vencimiento y QR de la FE registrada al pie. El ticket exige autorización informada y un QR generable; no utiliza valores ficticios de respaldo. Se reutiliza el constructor QR existente después de validar esos campos.
- Tickets extremadamente largos se dividen por bloques completos en páginas PDF de menos de 200 pulgadas, con cabecera de continuación y totales/QR en la última. No se omiten artículos.

R067 y R068 conservan los selectores existentes de A y B y reciben `Parametros.ctrl_papel`. La petición se construye en servidor desde DATOS conservado en el contexto de Caja; no se incorpora una elección libre de papel al request del navegador ni una consulta SQL adicional por impresión. Las llamadas de otros consumidores sin ctrl_papel mantienen A4.

El SP FE_D aportado calcula cmd_subtotal_con_iva desde cmd_pvta. Se corrige también la representación del unitario en B/A4, que sumaba IVA de nuevo. Se utiliza cmd_pvta directamente; no se recalculan subtotales ni totales fiscales. La distribución A4 permanece con su paginación existente.

## Publicación y aceptación

Publicar gc.api para devolver el DTO con ctrl_papel, gc.api.report/GCReportes para generar el formato, gc.caja para resolverlo e iniciador para transmitir la declaración física. Incluir las dependencias de cada publicación. No requiere cambios en gc.sitio ni Pocket. Para el cambio del 2026-11-26, publicar gc.caja y sus dependencias, vistas y scripts. Si el iniciador ya transmite impresionFormatoFE, no cambia su contrato ni requiere un binario nuevo por este contraste; actualizar el JSON con el papel físico declarado y reingresar. La publicación inicial del ticket conserva los requisitos de API/GCReportes indicados arriba.

Tras cambiar el JSON o actualizar la configuración del puesto, cerrar sesión y abrir nuevamente mediante el iniciador. Una sesión anterior puede conservar DATOS anterior. El campo no se configura en el appsettings del servidor web.

Antes de activar impresión silenciosa, instalar el controlador del modelo de comandera y comprobar el papel de 80 mm tanto en Windows como en la vista previa de Chrome. Se mantiene la impresora predeterminada de Windows. La selección de destino no garantiza que Chrome use el tamaño correcto. Validar una copia manual y otra automática del mismo PDF, con logo, todos los productos, importes y QR legible; no emitir otra factura para probar la impresión.

La marca/modelo todavía no se conoce. Los 4 mm de margen y el largo PDF son una base de prueba, no una aceptación del avance/corte físico de papel. Comprobar especialmente tickets extensos y el manejo de tamaño personalizado por el controlador. No se cambian políticas, impresora predeterminada ni preferencias de Chrome desde la aplicación.

## Diagnóstico y pruebas

El log [REPORTES][FORMATO_FE] muestra caja, sucursal, ctrl_id, ctrl_papel, PapelFisicoPuesto, formato resuelto y fuente SP_CAJA_DATOS/COMPATIBILIDAD. [CAJA][PAPEL_DIVERGENTE] diagnostica el contraste al recibir DATOS y [REPORTES][PAPEL_DIVERGENTE] lo registra al solicitar el PDF; ambos muestran los dos valores y el aviso. El request existente [REPORTES][REQUEST] incluye Parametros.ctrl_papel como JSON legible. [TICKET_B] registra cantidad de productos, ancho y páginas; no registra el contenido PDF. Un error de salida conserva el registro de la operación y permite revisar/reintentar sólo su impresión.

Se compilaron Caja, API, Reportes e iniciador. Se ejecutaron 106 verificaciones privadas de contratos, precedencia, controlador con servicios sustituidos, conservación del pase/sesión/cookie y errores; seis invocaciones reales --validar del iniciador. Se generaron 16 PDFs con datos ficticios: B de 1, 2, 27, 100 y 500 productos, normales/extensos, signos, importes máximos y A4 de A/B con/sin parámetro. Se verificaron ancho, márgenes, ausencia de giro y recorte, identidad/orden de productos y QR embebido. Los QR de dos imágenes a 203 dpi se decodificaron correctamente; los A4 con/sin parámetro conservaron igual texto y se contrastó el unitario B. Se inspeccionaron las imágenes de los tickets.

Pruebas, datos y evidencias están en .artifacts/ticket-b-20261121, excluidos de Git. No se invocaron SP, no se emitieron comprobantes, no se confirmaron pagos ni se enviaron impresiones en aquella entrega inicial. Los apartados siguientes registran la aceptación integrada del contraste de papel y la incorporación de A. Siguen pendientes la generación integrada del ticket con un documento real, su aceptación física en comandera y email.

Referencia técnica del QR: https://www.arca.gob.ar/fe/qr/documentos/QRespecificaciones.pdf

## Verificación del contraste, 2026-11-26

Compilación de Caja y 73 verificaciones privadas con servicios sustituidos: fuente SP en ambos sentidos, A4 por compatibilidad sin usar la declaración local como reemplazo, pase de un uso/cookie, reporte B/A y límites A/TK/EM, aviso de ingreso sin bloquearlo, exclusión CF y conservación de la advertencia CAEA. Cinco casos del parcial Razor renderizado por HTTP contrastan aviso visible/oculto. Chrome confirma ambos sentidos de divergencia y la desaparición del aviso al coincidir. No se modificaron ni ejecutaron SP, no se generaron operaciones financieras ni se enviaron impresiones. Evidencia privada en .artifacts/papel-contraste-20261126, excluida de Git. Aceptación en Caja publicada y con hardware real pendiente.

## Contraste integrado local aceptado, 2026-11-26

Con Caja recompilada y DATOS real (caja 0001, sucursal 0000, ctrl_id=-1, ctrl_papel=A4), el ingreso con declaración TK muestra el aviso antes del menú, permite continuar y conserva el aviso visible; el log registra ambos valores. El reingreso con una copia de configuración A4 abre el menú sin aviso; ambos paneles quedan ocultos/vacíos y no se registran nuevas divergencias. Se mantiene proceso/cierre, sin generar reportes, comprobantes, cobros ni impresiones. Detalle canónico EMI-24, bitácora 1.88.

La instalación de iniciador utilizada habitualmente era anterior a impresionFormatoFE: se usó el binario recompilado para estos ensayos. Para uso normal debe actualizarse ese iniciador y declararse el papel físico correcto. La prueba A4 usó una copia privada: el archivo habitual no se cambió y sigue declarando TK. Evidencias en .artifacts/papel-integrado-20261126, excluidas de Git. Queda validado el contraste integrado local; la impresión física en comandera y el despliegue remoto siguen pendientes.

## Instalación local actualizada y arranque normal, 2026-11-26

Se completó el pendiente del apartado anterior: el iniciador instalado de D:/Sitios/GC/Inicializador se actualizó desde una publicación Release win-x64 autocontenida, con respaldo y comparación SHA-256. El JSON habitual C:/Sitios/CajaConfig/cajasettings.json ahora declara A4; no se alteraron sus demás datos. El ejecutable instalado valida ese archivo y su arranque normal abre la ventana de Chrome. Reutilizando la sesión autenticada del mismo puesto, Operar Caja llega al menú sin aviso y sin nuevas divergencias en el log. Los accesos directos existentes se conservaron. No hubo emisión, cobro ni impresión. Respaldos y evidencia privados en .artifacts/iniciador-instalado-20261126. Trazabilidad canónica EMI-25, bitácora 1.89. La aceptación física del ticket en comandera sigue pendiente.

## Ticket A incorporado, 2026-11-26

El reporte 67 acepta TK y utiliza un generador específico A sobre el soporte interno de ticket de 80 mm. B conserva su entrada pública y presentación. Caja permite ambas letras al preparar el request; ctrl_papel del SP mantiene su autoridad y la declaración local sólo lo contrasta. A4 no cambia de formato y EM sigue sin implementar.

A usa el precio neto cmd_pneto, cantidad e importe neto cmd_subtotal recibidos, sin deducirlos del precio final ni volver a calcular impuestos. La versión inicial conservaba hasta diez decimales significativos en el unitario; el ajuste común de EMI-31 presenta entre dos y cuatro sólo en el texto del precio, conservando el dato recibido. Importes de renglón y totales se presentan a dos, conforme a su contrato. La bonificación se informa con IVA como la entrega FE_D, sin descontarla nuevamente del neto. El pie muestra no gravado/exento/gravado del encabezado, filas de IVA de FE_IVA con orden=2, total IVA, II y percepciones y total final. Se conservan los signos recibidos. FE_IVA no informa bases individuales por alícuota: no se reconstruyen; para agregarlas deberá confirmarse ese contrato con el analista/DBA. No se incluyen pagos/vuelto por carecer de su fuente, igual que en B.

Las consultas de IVA y percepciones del ticket A no convierten un fallo de transporte en lista vacía: se informa error y se reintenta sólo la generación del documento existente. Si hay IVA no cero sin desglose válido, tampoco se genera un PDF incompleto. Los límites de CAE, QR, logo, mezcla de identidades y descripciones/paginación se comparten con B. El log distingue TICKET_A/TICKET_B.

Compilan Caja/Core y gc.api.report en salidas aisladas. Se ejecutan 54 verificaciones C# y se generan 20 PDFs simulados: A con 1 a 500 productos normales/extensos, signos, exentos, bonificaciones y límites, y casos de regresión. 1.643 comprobaciones PDF, dos QR rasterizados a 203 dpi y revisión visual correctos. Seis referencias B/A4 conservan texto/dimensiones; B de dos artículos conserva además imagen idéntica a 160 dpi. No se ejecutan SP, emiten/cobran operaciones ni envían impresiones. Evidencias y muestra de prueba en .artifacts/ticket-a-20261126 (privado/excluido de Git).

Para probar este incremento en Caja, recompilar y levantar/publicar gc.caja y gc.api.report/GCReportes con sus dependencias. No cambia el SP, contrato API, IDs de reportes ni iniciador; no modificar la configuración habitual A4 sólo para abrir la muestra de prueba. La lectura integrada de un comprobante ya emitido y la impresión física siguen pendientes. El siguiente paso acordado es revisar y ajustar visualmente los aspectos comunes de A y B.

## Generación con documento real y corrección de lectura, 2026-11-26

Se solicita a Reportes el ticket de la factura A 0001-00000075 ya existente, sin confirmar otra operación. El request usa reporte 67 y ctrl_papel=TK sólo para esa generación; la configuración habitual de Caja permanece en A4 y no se activa la salida de impresión. Se entrega el PDF corregido para pantalla y se solicita su apertura en el panel de Codex.

La revisión con datos reales descubre que las bases de percepciones se mostraban como cero: el SP devuelve base, el DTO expone Base y la lectura de ObtenerFEPer era sensible a mayúsculas. La llamada al repositorio ahora usa ignoreCase:true exclusivamente para FePerResDto; no se cambian SP ni se reconstruyen valores. El PDF regenerado desde una compilación aislada de Reportes conserva 59.581,05 como base de ambas percepciones, 27 productos sin pérdidas/duplicados, total 74.950,81 y la misma identidad fiscal que el A4 histórico. Márgenes y QR embebido/renderizado a 203 dpi correctos. La instancia aislada se detiene al finalizar.

Para incorporar este último arreglo en la instancia habitual, recompilar y volver a levantar gc.api.report/GCReportes con su Core actualizado; otros hosts que se publiquen deben llevar sus dependencias coherentes. No se modifican Caja, iniciador o JSON por este defecto. No se emite, cobra ni imprime. Evidencias privadas en .artifacts/ticket-a-real-20261126. Detalle canónico EMI-27, bitácora 1.91. La generación del ticket con SP reales está verificada; siguen pendientes el recorrido de reimpresión desde Caja y la aceptación física en comandera, además de la revisión visual común A/B.

## Instancia habitual renovada y bases verificadas, 2026-11-26

Tras recompilar y reiniciar Reportes, se repite la generación en https://localhost:7275/api/reportes/generate, sin usar una instancia auxiliar. HTTP 200/resultado 0. El ticket A de 0001-00000075 muestra ambas bases de percepciones en 59.581,05, IVA percibido 1.697,79 e IB 1.787,43. Conserva sus 27 productos, total 74.950,81, dimensiones de 80 mm y QR/identidad fiscal del documento anterior. Se verifican márgenes y lectura del QR sobre el PDF y sobre su imagen a 203 dpi. Se solicita abrir el PDF actualizado para pantalla; no se emite, cobra ni imprime otra operación. La configuración habitual sigue intacta. Evidencias privadas en .artifacts/ticket-a-normal-20261126, con el fallo anterior archivado en antes-reinicio. Aceptación canónica EMI-29, bitácora 1.93. Queda cerrado el pendiente de carga del Core corregido en la instancia habitual; los ajustes visuales comunes A/B y las pruebas físicas siguen pendientes.
## Presentación común A/B aprobada, 2026-11-26

Implementada en FacturaTicketPdf, compartida por los selectores de ticket existentes. Se mantienen 80 mm, márgenes de 4 mm y QR de 35 mm. El logo usa hasta 14 mm de alto, sin deformación. Tipo/letra/código se reúnen en una línea cuando cabe; número, leyenda y fecha conservan jerarquía y todos los datos del emisor y cliente permanecen disponibles.

Cada producto muestra código y descripción completos con continuación, cantidad por precio y el importe a la derecha en negrita, seguido por alícuotas en tamaño secundario y bonificación cuando se informa. El unitario usa dos a cuatro decimales visibles; cantidad tres; importes dos. A utiliza cmd_pneto y cmd_subtotal; B utiliza cmd_pvta y cmd_subtotal_con_iva. No se deducen importes a partir del unitario impreso ni se mutan los DTO. La bonificación de A se identifica con IVA, sin restarla nuevamente del neto.

El pie muestra concepto e importe de percepción en la misma fila, base y alícuota debajo y total/moneda destacados. A mantiene netos e IVA discriminado; los conceptos de IVA reconocidos sólo normalizan el separador decimal para lectura, conservando importes y sin inferir bases. B mantiene precios finales y el bloque de transparencia fiscal: IVA Contenido y Otros Impuestos Nacionales Indirectos con los impuestos internos disponibles en el encabezado; no se agregan impuestos que el contrato no devuelve. La fila II de FE_PER no se suma otra vez. CAE, vencimiento y QR permanecen al final. No se incluyen pagos/vuelto sin contrato.

Se generaron cuatro muestras principales ficticias de A/B con 3 y 27 productos, señaladas PRUEBA - SIN VALOR FISCAL; no son comprobantes emitidos. Se verificaron además signos, bonificaciones, descripción/precio/importe extensos y regresiones A4. Doce casos sin mutación y 488 comprobaciones PDF correctas; los dos A4 conservaron texto y dimensiones. Los cuatro QR se decodificaron embebidos y a 203 dpi. Se inspeccionaron las comparaciones visuales completas. Reportes compila en salida aislada con cero errores. Evidencia privada en .artifacts/tickets-comunes-20261126, excluida de Git; sin SQL, servicios reales de negocio, emisión, cobro o impresión.

Recompilar y levantar/publicar gc.api.report/GCReportes con su Core actualizado para usar este incremento. No requiere cambios de Caja, API de negocio, iniciador, JSON o SP. Las muestras locales permiten revisar la presentación sin activar la salida automática de Caja. No se sustituye ni detiene la instancia abierta. Quedan pendientes la aceptación visual del responsable y la prueba física en el modelo de comandera que se defina. Trazabilidad canónica EMI-31, bitácora 1.95; publicación NotebookLM pendiente.

## Control temporal de reportes, 2026-11-27

En el menú principal de Caja se incorpora CONTROL DE REPORTES, identificado como Temporal. URL dentro de la aplicación: Facturacion/ConsultaComprobantes. Recompilar/publicar gc.caja con vistas y wwwroot para disponer de este acceso. No requiere cambios de API, SP, iniciador o JSON; Reportes debe conservar su versión vigente con los generadores A/B.

Uso: elegir tipo de comprobante, ingresar PV y número (4-guion-8 con ceros completados), Buscar, elegir repetición cuando haya varias y pulsar Ver A4, Ver ticket de 80 mm o Comparar ambos. Los PDF se presentan en visores en la propia vista. Volver al menú principal usa menu=true. Se admiten comprobantes A/B de la sucursal autenticada, incluidos otros PV. La identidad/importe/repetición visibles provienen de la cabecera; se releen antes de generar y la letra determina el reporte en servidor.

Sólo se reutiliza la consulta de cabecera del circuito de NC por Devolución, sin sus contextos de productos ni sus restricciones para emitir una NC. La herramienta no emite, cobra ni modifica comprobantes. La elección manual A4/TK es una excepción exclusiva de revisión: no sustituye la regla normal de ctrl_papel del SP ni modifica el papel físico declarado por el iniciador. Se excluye el componente común de salida/impresión en esta vista, aunque el navegador puede ofrecer sus controles PDF nativos. La app no llama a print ni registra salidas de impresión desde este módulo. Las consultas y los PDF no se almacenan en caché; al cambiar datos o limpiar se liberan los visores y sus URLs.

Diagnóstico: [REPORTES][CONSULTA_TEMPORAL] registra REQUEST/RESPONSE de búsqueda y generación en JSON legible, sin token ni contenido PDF. Generar usa las mismas configuración, credenciales de sesión y rutas de Reportes que Caja. No se agrega una conexión SQL ni se cambia el SP.

Validación: 65 casos HTTP con servicios simulados y la misma vista Razor compilada, incluyendo rutas /gccaja, identidad/repeticiones, permisos, antiforgery, fallos y conservación de la operación. Chrome muestra ambos PDF y permite cambiar a B; se verifican errores, limpieza y selección explícita de repetición. En el navegador integrado los controles funcionaron, pero no se renderizó el visor PDF nativo; la comprobación visual de los PDF fue en Chrome. Todo el material de prueba está excluido de Git en .artifacts/consulta-comprobantes-20261127. No hubo SQL real, nuevas operaciones ni impresión. Prueba integrada con documento existente pendiente de recompilar/levantar Caja. Bitácora 1.96, EMI-32.

Retirada futura: quitar el botón data-action=consulta-reportes de _ModalMenu, la variable ConsultaComprobantesUrl de Home/Index y su case/excepción de habilitación en caja.js; retirar ConsultaComprobantesController, ConsultaComprobantesRequest, la carpeta de vista ConsultaComprobantes y consultaComprobantes.js/consulta-comprobantes.css. El indicador ViewData["OmitirSalidaComprobantes"] puede retirarse del layout cuando deje de tener consumidores, restaurando la condición autenticada original. No modificar ProductoFactController.Impresion, los generadores ni los flujos normales de emisión/impresión para retirar esta herramienta.
