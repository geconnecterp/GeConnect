# Salida de comprobantes de GECO Caja

La emisión/registración de una operación y la salida de su documento son etapas independientes. La salida común conserva los generadores de PDF y el circuito del controlador fiscal.

## Configuración del puesto

Agregar al `cajasettings.json` que lee el iniciador:

```json
"impresionModo": "IMPRESORA"
```

Valores: `IMPRESORA` solicita impresión sin presentar el documento; `PANTALLA` abre el PDF. Se transmite en el pase y se conserva en el contexto protegido de la estación. No se configura el nombre de una impresora: se utiliza la predeterminada de Windows del operador. Cambiarla en Windows determina el nuevo destino según las políticas del navegador.

Publicar las nuevas versiones de Caja y del iniciador. Al actualizar el JSON, cerrar la sesión y volver a entrar mediante el iniciador, como ante otros cambios de configuración del puesto. Una configuración anterior sin esta propiedad sigue siendo válida y ahora utiliza impresión directa. `PANTALLA` debe establecerse expresamente en el puesto; el valor legado `AppSettings:NotaCreditoReporteModo` deja de decidir la salida de NC/ND/FS. El campo `facturacion` y el controlador validado siguen determinando si corresponde un comprobante electrónico.

## Preparación de Windows y Chrome

No es necesario configurar políticas para compilar, desarrollar ni usar Pantalla. Sólo los puestos o desarrolladores que prueben impresión directa necesitan esta preparación. Chrome es el navegador principal; para impresión silenciosa se requiere versión 144 o posterior. La configuración silenciosa afecta todas las impresiones de ese navegador, no sólo Caja.

La herramienta `gc.caja.iniciador/Configurar-Impresion.ps1` permite consultar, habilitar y restaurar estas dos políticas. Crea un respaldo antes de modificar valores y no cambia la impresora de Windows, otras políticas ni permisos del sistema. Usa HKCU: ejecutarla para el mismo usuario de Windows que utiliza Chrome. Los permisos necesarios dependen de la administración del equipo. Sistemas también puede distribuir las políticas de equipo mediante sus mecanismos habituales.

```powershell
& '.\Configurar-Impresion.ps1' -Navegador Chrome -Accion Estado
& '.\Configurar-Impresion.ps1' -Navegador Chrome -Accion Habilitar
& '.\Configurar-Impresion.ps1' -Navegador Chrome -Accion Restaurar
```

Si Windows tiene bloqueada la ejecución de scripts, conservar esa restricción. Un administrador puede aplicar sólo los ajustes mediante estas órdenes nativas, verificando y respaldando primero sus valores anteriores:

```powershell
reg.exe add "HKCU\Software\Policies\Google\Chrome" /v SilentPrintingEnabled /t REG_DWORD /d 1 /f
reg.exe add "HKCU\Software\Policies\Google\Chrome" /v PrintPreviewUseSystemDefaultPrinter /t REG_DWORD /d 1 /f
```

Luego abrir manualmente `chrome://policy`, pulsar «Volver a cargar políticas» y comprobar ambos valores `true`, estado «Correcto». Para Edge se usan las políticas equivalentes bajo `HKCU\Software\Policies\Microsoft\Edge`; la herramienta admite `-Navegador Edge` o `Ambos`. No se cierra ni reinicia el navegador automáticamente.

No hace falta habilitar scripts permanentemente, desactivar protecciones, instalar extensiones ni mantener un servidor local de impresión. Una política de equipo puede prevalecer sobre HKCU; Sistemas debe resolver el valor efectivo. Una impresora virtual predeterminada tiene el comportamiento propio de su controlador: no equivale a salida en papel.

## Módulos y recuperación

- Facturación de contado, venta con pago diferido, NC por devolución y ND/NC/FS: conservan la generación actual y utilizan la salida común sólo cuando el servidor indica que corresponde el documento electrónico.
- Custodia: tras una entrega registrada solicita directamente la impresión de su acuse. Se eliminó el visor incrustado y su enlace antiguo. Si el puesto configura expresamente PANTALLA, se utiliza el visor común en otra pestaña; no se vuelve a incrustar un PDF en Custodia. Reimprimir sólo vuelve a consultar el acuse existente.
- CD y CC: utilizan el circuito de recibos, sin convertir el cobro en una factura. Se conectan cuando existe un reporte de recibo disponible y configurado.

El aviso común ofrece Ver PDF y Reimprimir. Ante un error conserva en `sessionStorage` referencias de los últimos diez documentos durante un máximo de 24 horas, separadas por estación, sucursal, caja y operador. No conserva en ese almacenamiento el PDF, credenciales ni importes. Se puede recuperar el documento al navegar por Caja en la misma pestaña/perfil/origen. No es una cola durable de impresión ni un historial central de reimpresiones.

Una repetición automática de la misma respuesta o un doble clic no vuelven a imprimir. Reimprimir es una acción explícita; nunca vuelve a confirmar venta, cobro o entrega. Una solicitud interrumpida requiere revisar la cola de Windows antes de repetir. La salida respeta el destino de navegación propio de cada módulo.

El mensaje «Impresión solicitada» acredita que se invocó la impresión desde el navegador, no que la impresora produjo papel. Chrome no siempre notifica `afterprint` al imprimir un PDF: no se usa ese evento para declarar éxito físico ni un fallo financiero. El visor de impresión queda fuera de pantalla y se conserva temporalmente para preparar el trabajo. El operador debe atender los errores físicos en Windows/impresora.

## Recibos: disponibilidad del reporte

Esta instalación sólo tiene los reportes A/B (67/68). No se agrega un ID ficticio ni se reutiliza Factura A/B para un recibo. La salida de CD/CC está preparada para claves `RECIBO_CD`, `RECIBO_CC` o respaldo `RECIBO`, con un reporte real en la API. El adaptador recibe el número `rb_compte` y el ámbito `adm_id`; antes de habilitar la clave hay que contrastar el ID, los parámetros y el formato con el responsable del reporte. Sin ese contrato y PDF no se anuncia impresión de un recibo. No se modificaron SP.

## Diagnóstico y verificación

La consola y el log de Caja registran `[IMPRESION]` con solicitud, identificación de documento, modo y estado; el endpoint de diagnóstico requiere autenticación y antiforgery. El contenido de PDF y los tokens no se registran.

Prueba inicial del 2026-11-04: Chrome 156, políticas recargadas por el responsable, predeterminada HP Ink Tank 310 series. Windows observó trabajo 2 de una página y el responsable confirmó la hoja con «GECO CAJA - PRUEBA DE IMPRESION / SIN VALOR FISCAL». No se registraron operaciones ni se llamaron SP en estas pruebas. Verificado también Pantalla sin otra impresión. 36 verificaciones de JavaScript, 22 del contrato de estación/pase/sesión y cinco del iniciador con lectura de archivos ficticios. Compilación aislada de Caja e iniciador sin errores; advertencias previas de dependencias separadas de fallos. Pruebas y evidencias bajo `.artifacts`, excluidas de Git.

Quedan para aceptación posterior con los PDF y equipos de destino: Edge, cambio de impresora predeterminada, documentos extensos/formatos físicos y recibos cuando se proporcione su reporte. La prueba física inicial de Chrome no reemplaza esa matriz.

Referencia funcional: [Bitácora única de Caja](BitacoraUnicaReglasCaja.md), EMI-05 a EMI-09.


## Corrección de aceptación del 2026-11-04

Se inspeccionó la pantalla real del responsable con entrega 00-00014485 ya registrada: modo de puesto null, mensaje «Comprobante disponible en pantalla» y visor incrustado visible. El respaldo anterior a Pantalla era el origen de esta discrepancia con la salida solicitada. Se establece IMPRESORA como respaldo común en servidor y navegador, se retira el visor de Custodia y se unifican los modos devueltos por NC/ND/FS. Sólo la preferencia explícita PANTALLA activa una vista del PDF. La entrega existente no se vuelve a registrar. Tras recompilar el responsable, se verificó en la instancia real el modo IMPRESORA y la ausencia tanto del visor incrustado como de su enlace; no se volvió a confirmar una entrega ni a imprimir el acuse en esa inspección; las comprobaciones privadas reproducen el modo ausente y ejecutan el handler real del acuse sin un visor en la vista.

## Área imprimible del acuse de Custodia (2026-11-04)

La salida silenciosa horizontal se observó recortada en la HP, aunque el PDF original se renderizaba completo. Generar el horizontal sin Rotate=90 no resolvió la prueba física. El acuse se adapta a A4 vertical nativo, con aproximadamente 12 mm de margen de contenido, pie separado del borde y ocho columnas distribuidas dentro de ese ancho. Se conservan datos, importes, firmas, totales por moneda y repetición de encabezados.

El responsable confirma completa y alineada la hoja ficticia vertical enviada por el mismo componente de impresión directa, sin mostrar el PDF. Se verifican además 150 rendiciones en cuatro páginas, sin recortes de texto en la geometría del PDF. El ajuste se aplica exclusivamente al generador CustodiaAcusePdf de gc.api.core, utilizado por gc.api. Recompilar y reiniciar la API; no requiere actualizar Caja ni el iniciador por este ajuste. La página que ya está abierta puede conservar el PDF anterior en memoria: su reimpresión no sirve para juzgar el generador recién cargado. Las solicitudes nuevas de PDF usan el nuevo formato. No se cambia la orientación global de la impresora para evitar afectar las facturas y los demás comprobantes.
