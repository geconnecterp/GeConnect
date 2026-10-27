# Plan de integración Clover en Caja

> **Contrato financiero vigente — 2026-10-27, CLV-25 / bitácora 1.20.** En json_valores: rb_importe es el total cobrado, rb_rec es el porcentaje de la opción validada y rb_aux es el importe monetario del recargo/descuento. Los cálculos y el resumen usan rb_aux; ambos ajustes conservan el signo. Las referencias históricas a rb_rec como monto quedan sustituidas. La firma y columnas consumidas de Clover_Pay se conservan; la actualización se verificó con SP/POS simulados. Desplegar Caja y API juntas y probar emisión fiscal con el SP corregido por el DBA.

> **Decisión vigente de flujo — 2026-10-26, CLV-20.** Se elimina la consulta y validación de estado POS en todo el circuito. Cada respuesta de Clover_Pay es independiente, se expone como RespuestaDto y agrega su fila; al quedar saldo cero se finaliza sin consulta. Se conserva el cálculo, catálogo, controles normales de Caja y logs. El diario y su recuperación automática descritos en las etapas anteriores quedan sustituidos; los archivos existentes se preservan sin utilizarlos para bloquear. No hay reintentos automáticos ni garantía nueva de idempotencia duradera. Las secciones fechadas anteriores se mantienen como trazabilidad, no como instrucciones vigentes frente a CLV-20. Pruebas sólo con SQL/POS simulados; coordinar antes de pagos físicos.

Fecha: 2026-10-23. Estado: integración de aplicación implementada, conectada a SP y verificada con respuestas simuladas. Pendientes de DBA y aceptación con SQL/POS desplegados detallados al final. No se realizaron cobros, consultas a SQL ni llamadas al POS. Documento fuente leído directamente desde el archivo entregado por el responsable, sin atribuirlo a una consulta exitosa de NotebookLM.

## Fuentes y precedencia

- Documento: D:/#Documentos/#GECO/Caja/20261023 Pago Clover.docx. Se leyeron sus 14 párrafos no vacíos y la maqueta embebida; no contiene comentarios, notas ni firma completa del SP de pago. Las referencias siguientes D4 a D14 corresponden a esos párrafos, contando título como D1.
- Charla: D:/Sis25/git/GeConnect/doc/20261023 - Transcripcion charla CLOVER.TXT. Se omiten conversaciones personales ajenas al módulo.
- BitacoraUnicaReglasCaja.md v1.5 como antecedente del comportamiento actual; CLV-01 a CLV-08 registran lo obtenido ahora. Este plan no convierte puntos pendientes en decisiones aprobadas.
- Código revisado y modificado exclusivamente en D:/Sis25/git/GeConnect/src. La primera fase fue sólo análisis; el avance de esta fecha documenta el modal y las pruebas implementados.

## Resultado del contraste

| Tema | Documento | Charla | Consecuencia de diseño |
| --- | --- | --- | --- |
| Entrada al modal | Monto sugerido es el remanente de factura/cobranza (D4) | Ejemplo de pago mixto 150 = 50 MP + 50 Clover + 50 restantes (230-234) | Sugerir el saldo actual; admitir el diseño de pagos parciales sin finalizar hasta que quede cubierto. Confirmar edición del monto y secuencia exacta de medios mixtos antes de implementar sus efectos bancarios. |
| Instrumento | Opciones desde SPGECO_CAJA_Valores_Ins_Opciones por ins_id; G00 es el caso Clover (D5) | El mismo dispositivo puede estar asignado a caja 1 y caja 2 (143-153) | No usar descripciones para decidir el flujo real ni fijar G00 como categoría tcf_id. Contexto de PV/sucursal obtenido del servidor. Coordinar concurrencia sobre dispositivo compartido. |
| Cuotas | cuota es cantidad real; pos_plan es el código que recibe el POS; mostrar pos_desc y recargo_porcentaje (D5) | Cantidad de cuotas y recargo calculado antes de cobrar (75-82) | Transportar ambos campos sin confundirlos. La asignación de rb_opcion_cuota se toma del retorno/contrato, no se supone igual a pos_plan. |
| Recargo | Round(monto * recargo_porcentaje / 100, 2); POS recibe monto + recargo (D6-D7) | rb_importe contiene el total; rb_rec sólo el recargo; se mencionan descuentos negativos (264-322) | Cálculo decimal del servidor y resumen sincronizado. No sumar el recargo dos veces ni rechazarlo como excedente. Mantener signo sólo según catálogo/contrato válido. |
| QR | Bit para el SP de pago (D8) | El dispositivo muestra QR; entorno de demostración puede simularlo (9-14,99-123) | Switch previo al envío; no inventar un generador QR web ni exigir cuotas compatibles por suposición. |
| Pagar POS | SPGECO_CAJA_Clover_Pay con paymentid vacío (D11) | Activa el dispositivo; puede cancelarse allí antes de pagar (1,99-137) | Es la acción que solicita el cobro. Bloquear el formulario durante la espera. No reintentar un envío incierto. |
| Carga manual | Mismo SP; ID obligatorio, mínimo 3 caracteres alfanuméricos uppercase (D9,D13) | Recupera sólo un pago existente pendiente; rechaza inexistente o imputado (58-95,147-158) | Normalizar a mayúsculas y validar el ID; consultar al servidor. No completar ceros hasta 15 como en Transferencia, ni crear un pago manual sin validación real. |
| Resultado | Interpretar respuesta/respuesta_msj y cargar los recibo_valores devueltos (D11-D13) | Retorna ID, lote/cupón, cuota, fecha, importe POS, cashback y tipo financiero (90-95) | Éxito HTTP no acredita cobro. Separar resultado de operación y filas canónicas. Conservar ID y datos exactos; no sustituir por lo escrito en el formulario. |
| Pago aprobado | Datos no editables ni eliminables; no cancelar el pago; finalizar si cubre el total (D11) | Se explica la finalización automática tras respuesta correcta; luego impresión fiscal (217-260) | Disparar una sola finalización común cuando no quede saldo y todas las validaciones pasen. Si hay saldo, seguir completándolo con lo aprobado bloqueado. |
| Volver | Cancela la carga (D14) | Cancelación remota puede responder OK y dejar el POS activo (159-183) | Volver sólo abandona la preparación sin cobro. No equivaler cierre de modal, aborto HTTP, cancelación de solicitud y reversión bancaria. |
| Espera | No fija un tiempo | Aproximadamente cinco minutos, dejando resolver a API/POS (133-137,272-273) | Timeout específico para Clover en todas las capas, con margen exterior; auditar proxy/IIS/hosting. No cambiar globalmente otros pagos. |
| Alcance futuro | No desarrolla administración ni anulación | Cancelar remoto y mensajes welcome/thank aún requieren trabajo (159-193); supervisor para ID se menciona como posibilidad, no requisito actual (241-245) | No incluir cashback, anulación/reintegro ni nuevos permisos de supervisor como si estuvieran especificados. No confundir cobro con gestión del POS. |

D11 precisa la finalización automática; supera la ambigüedad del último clic planteada en la charla y amplía PAG-08, que sólo quitaba la pregunta posterior. PAG-09 permanece como solución temporal para otros instrumentos hasta su sustitución específica; Clover real debe salir de ese recorrido.

## Hallazgos concretos en la implementación actual

1. No se encontraron adaptadores ni constantes para SPGECO_CAJA_Clover_Pay o SPGECO_CAJA_Valores_Ins_Opciones en los proyectos revisados. El trabajo requiere API/servicio/DTO además del modal; no es sólo una mejora de vista.
2. pagoFactura.js construye valores electrónicos mediante la captura temporal: referencia de cinco caracteres, relleno a 15 posiciones, cuota por defecto, rb_rec = 0 e id_externo vacío. Esa representación no cumple el nuevo contrato Clover y debe tener una rama específica anterior al fallback genérico.
3. agregarFilaValor muestra Eliminar para las filas convencionales y eliminarValor no identifica un cobro real protegido. volverACalculoFactura conserva borradores, pero no impide salir después de un cobro aprobado. Hace falta una política común basada en el estado canónico del cobro, también en servidor.
4. La lógica actual no dispara finalizar automáticamente al completar la carga de un pago electrónico. Sólo omite la pregunta cuando el operador pulsa Finalizar.
5. El catálogo de ReglasCreditoPago debe reconocer el ins_id canónico devuelto por el SP. Si cambia respecto de G00, el contrato debe definir la relación y la validación del servidor; no abrir una excepción que acepte cualquier instrumento.
6. El total exigible de CheckoutController parte de subtotales/deuda. Se debe integrar el recargo financiero canónico una sola vez en frontend y servidor, también al validar NC y vuelto. El controlador hoy rechaza todo rb_rec distinto de cero en CD: conservar ese límite hasta decisión funcional explícita.
7. No reutilizar ciegamente ObtenerValoresPendientes: ApiPagoFactServicio.ObtenerValoresPendientes usa SP_CAJA_COTIZACION, que apunta a SPGECO_CAJA_B_Cotizaciones; además su DTO tiene ins_id int?, incompatible con un instrumento alfanumérico como G00. Verificar/reparar ese recorrido sólo si se confirma su incumbencia; para Clover se propone un contrato específico con ins_id string.
8. El SQL genérico de Repository.EjecutarLstSpExt tiene CommandTimeout 600. CheckoutServicio usa HelperAPI, que crea HttpClient sin timeout específico, y el envío final JS tiene 120 segundos. No trasladar los 90 segundos del servicio AdministradorCaja a Checkout: son clientes distintos. La operación nueva necesita revisar la cadena completa, sin atribuir al POS un timeout de una capa intermedia.

## Flujo propuesto

1. Abrir desde el instrumento Clover habilitado por el catálogo del módulo; sugerir remanente. Solicitar las opciones de cuota al servidor con ins_id.
2. Seleccionar cuota, mostrar porcentaje, recargo y total. Cálculo validado en servidor con la opción vigente. La pantalla no decide importes bancarios mediante parámetros manipulables.
3. Pagar POS: validar sesión/PV/cliente/operación, categoría, saldo, restricciones del módulo y configuración; enviar paymentid vacío, total, pos_plan y QR según firma real. Carga manual: enviar el ID normalizado al mismo SP, sin nuevo cobro; validar pertenencia e imputabilidad en backend.
4. Estado en espera: una solicitud por intento, modal protegido, sin acciones incompatibles. La devolución de error explícito del POS permite corregir/reintentar según resultado conocido. Desconexión/timeout dejan el intento incierto; no reintento bancario automático.
5. Resultado aprobado y filas válidas: guardar canónicamente pago e identificación externa en servidor, asociados a la operación, antes de mostrarlos como aplicados. No aceptar un booleano protegido enviado por el navegador como prueba de aprobación. Rechazar duplicado por ID real en servidor y exigir garantía de imputación única del SP final.
6. Fijar importe, recargo, lote/cupón, cuota, fecha, tcf_id e id_externo devueltos. Sin edición/eliminación ni desplazamiento a otro cliente. Otros valores/NC sólo pueden ajustarse dentro de los límites compatibles con el cobro ya hecho; no alterar mercancía/deuda para deshacerlo.
7. Recalcular base, recargo financiero total, total exigible, valores, NC y diferencia. Si resta saldo, continuar con otros medios. Si no resta y la operación es válida, llamar una sola vez al finalizar común de Caja, sin consulta adicional.
8. Finalizar usa el circuito actual SPGECO_CAJA_Ope_Confirmar y stock/impresión/navegación según módulo. No reemplazarlo por otro finalizar Clover. Registrar pago, emitir comprobante y reconocer éxito son etapas diferentes: un fallo de impresión tampoco habilita repetir el débito.
9. Si Caja no termina de registrar después del cobro, conservarlo como aprobado pendiente y ofrecer recuperación/reintento de registración, sin pedir otra tarjeta. Tras reinicio, el SP de recuperación y el respaldo local de IDs/contexto permiten reconstruir el estado; no confiar sólo en memoria de pestaña. La persistencia y la reconciliación exactas dependen del contrato del backend.

Estados mínimos: preparación -> esperando POS -> aprobado pendiente -> registrando Caja -> registrado. Ramas separadas para cancelado/rechazado e incierto. Carga manual recupera un aprobado pendiente; no es transición que ejecute otro cobro. Ventana bloqueada y antes de salir se advierte, pero no puede garantizarse impedir cierre del navegador/corte de luz; la recuperación resuelve esa situación.

## Secuencia de implementación y alcance

1. Contratos y ejemplos del analista/DBA. Confirmar esquema de opciones y firma de pago, retornos y errores. Fijar garantías de recuperación/concurrencia, antes de ejecutar cualquier cobro.
2. Adaptador del servidor y datos tipados: gc.infraestructura, API/Core y servicio de Caja. Contexto desde sesión, antiforgery, campos permitidos, importes decimales, validación del catálogo, plazo propio de Clover e intentos identificables. No modificar SP ni tocar gc.sitio.
3. Componente reutilizable: parcial de Pago para el modal y JS específico Clover conectado a pagoFactura.js. Encabezado Golden, campos alineados, total destacado, switch QR, acciones POS y recuperación claramente separadas, teclado/Enter y foco coherentes con Caja. Importe sugerido; recargo y total de sólo lectura.
4. Integración de valores canónicos, recargos y protección de aprobados. Validar también en CheckoutController y en los flujos de reanudación/limpieza, sin depender sólo del icono de candado.
5. Finalización automática común: motor en Facturación CR/CF, Cobranza Diferida CD y Cobranza CtaCte CC, únicamente si el catálogo habilita Clover. CD sigue sin recargo; no habilitar cuotas con costo por tener el modal disponible. Para CV/IV, revisar catálogo y contrato propio Ope_Cv_IV antes de extender la integración; comparten componente/política, no necesariamente el mismo finalizar ni la misma contrapartida. ND/NC/FS emiten comprobantes y no incorporan este cobro sólo por su nombre. No implementar Gestión POS, anulación Clover o MP en esta etapa.
6. Pruebas simuladas de extremos y regresiones; luego prueba manual recuperando ID pendiente facilitado por el analista. Prueba POS/QR coordinada con entorno de prueba y operador. No se usó el dispositivo ni se ejecutó SP durante este análisis.

## Matriz de aceptación prevista

- Opciones vacías/errores bloquean cobro; distinguir cuota real de pos_plan; no reemplazar un plan no vigente.
- Base 50000.23 y 12 % producen recargo 6000.03 y total 56000.26 con redondeo habitual a centavos. La maqueta muestra 6000.02/56000.25: no copiar su inconsistencia. Definir el criterio de medio centavo entre frontend/servidor/DBA antes de programarlo.
- Plan sin recargo, recargo positivo y descuento autorizado negativo; sumar por fila y mantener rb_importe total y rb_rec firmado, sin duplicarlos.
- Manual: ID de 3 caracteres válido; minúsculas normalizadas; espacios internos/signos/menos de 3 rechazados; inexistente, imputado, distinto PV, duplicado en operación o concurrencia en dos puestos rechazados según contrato.
- Respuesta HTTP 200 con error de SP no aplica valor; éxito sin filas/ID/monto coherente no dispara finalización ni otro cobro. Conservar el resultado incierto y reconciliar.
- Aprobación POS y QR, rechazo, cancelación física y espera de aproximadamente cinco minutos; respuesta tardía tras timeout; dos clics no cobran dos veces; POS compartido entre puestos.
- Pago parcial queda bloqueado y conserva saldo; último pago que cubre saldo finaliza exactamente una vez. Mezcla NC/efectivo/Clover respeta vuelto y validaciones previas.
- Cobranza Diferida bloquea planes con recargo antes de contactar al dispositivo. No ofrecer un plan para después rechazarlo tras cobrar.
- Fallo de SP final, desconexión entre aprobación y registro, reinicio web/navegador y fallo fiscal: recuperar sin repetir débito, duplicar valor ni perder ID.
- Intento de modificar importe/recargo/ID por request directo al servidor o de reusar una aprobación de otro cliente/operación no se acepta.
- Comprobantes FE/CF, recibos y navegación conservan las reglas vigentes de cada módulo.

Verificación de viabilidad realizada: extracción completa del texto/imagen; revisión de contratos locales; ejecución aislada de evaluarReglasPago real con VM de Node: cobro 110 sobre base 100 se rechaza como excedente si no se integra el recargo 10, y se acepta con total exigible 110. No demuestra integración real ni cobertura del futuro componente.

## Datos que faltan para implementar la conexión real

- Firma y SELECT de SPGECO_CAJA_Clover_Pay ya recibidos y representados por DTOs. Faltan ejecuciones de ejemplo en éxito/error/manual y las correcciones/garantías SQL detalladas en el avance de esta fecha.
- Definición de SPGECO_CAJA_Valores_Ins_Opciones ya recibida: cuota, recargo_porcentaje, pos_plan y pos_desc. Se usa G00; falta resolver char(2), recibir filas de ejemplo y confirmar el catálogo/relación del dispositivo con PV. Clover_Pay devuelve @cuota como rb_opcion_cuota; la petición debe distinguir ese plan de las cuotas reales.
- Mapeo recibido: ID/lote/cupón en rb_dato1/2/3; rb_importe desde @monto, pos_importe desde amount/100 y pos_cashback. Faltan ejemplos y resolver la asignación vacía del ID en POS y el importe de recuperación. No usar cashback ni ignorar discrepancias para forzar un total.
- Confirmar que la versión desplegada del SP final toma rb_rec y marca como imputado el ID de forma atómica: el analista dijo que debía completar el tratamiento del recargo.
- Confirmar garantía de recuperación tras cobro aprobado antes de respuesta al navegador, y medio de consultar estado de un intento incierto si todavía no existe ID conocido. paymentid vacío no aporta por sí solo idempotencia de un nuevo envío.
- Confirmar semántica de pago mixto y edición de monto: el documento inicia el cobro al pulsar Pagar POS; en la charla “todavía no genero la operación” puede referirse a la registración de Caja. No convertir esa frase en un sistema de cobros diferidos en lote sin validación del responsable.

## Estado de documentación

La bitácora local se actualiza para registrar estas fuentes y el plan, distinguiendo requisitos nuevos de implementación pendiente. No se publicó esta revisión en NotebookLM: su herramienta de navegación no logró verificar la URL y detuvo el acceso. No se declara sincronización ni contraste remoto realizados.

## Avance de implementación del 2026-10-23

El responsable autorizó implementar el modal y probar con respuestas simuladas; aclaró que esta instalación está en etapa de pruebas. Se recibió además la definición SQL de Clover_Pay en el adjunto f0632145-96a6-4d13-981c-9823c770ea50/Texto pegado.txt y la definición de Valores_Ins_Opciones en el mensaje. No se ejecutó ninguno de los SP ni se modificó SQL.

### Implementado y comprobado

- Parcial _detalleClover.cshtml y CSS Golden responsive: monto sugerido, cuotas, recargo/descuento de sólo lectura, total, QR, recuperación por ID, estado y Volver. Enter avanza sin iniciar un cobro por accidente.
- cloverPago.js: estados de preparación, espera, aprobado, cancelado/rechazado e incierto; cálculo decimal en centavos con BigInt, separación de cuota real/pos_plan, filtrado sin recargos para CD, doble envío bloqueado y validación estricta del retorno. El redondeo de medio centavo es provisional, alejándose de cero, y queda limitado a simulaciones hasta validar el contrato definitivo.
- cloverPagoVista.js: enlace DOM y transporte HTTP preparado. El transporte operativo sólo recibe el rechazo explícito de CloverController; no contiene simulaciones ni acceso bancario. Los 330 segundos de espera del cliente son preparación, no prueba de que la cadena real ya tolere ese tiempo.
- G00 abre el nuevo modal desde Facturación CR/CF, CD, CC y la pantalla CV/IV, sin caer en Transferencia. Las demás tarjetas conservan el comportamiento anterior. CV/IV no heredan la confirmación de Facturación.
- DTOs alfanuméricos para opciones, preparación y fila del SELECT realmente entregado. Se conserva respuesta, ID en rb_dato1_valor, lote, cupón, plan, fecha, rb_importe, pos_importe, pos_cashback y tcf_id.
- CloverController autenticado, POST con antiforgery y respuesta CLOVER_NO_CONECTADO/HTTP 503 incondicional. No tiene cliente HTTP del proveedor, repositorio SQL, rama que cobre ni un flag para habilitarlo desde el navegador. El catálogo no se inventa en las pantallas operativas.
- Fixtures sólo en Tests. El host local de pruebas renderiza el Razor real y carga fixtures locales; una aprobación simulada emite FINALIZAR o COMPLETAR como señal comprobable, pero no llama a Checkout ni agrega valores al pago de Caja.
- 58 escenarios JS y 19 verificaciones HTTP/Razor/contratos aprobados. Regresiones: 99 escenarios de finalización, 16 DOC, 43 NC y 32 reglas de pago más navegación CD/doble confirmación. Compilación aislada correcta; advertencias existentes en la primera compilación de dependencias, cero errores. La última compilación incremental del host informó cero advertencias y errores.

### Cómo reproducir las pruebas

Desde D:/Sis25/git/GeConnect/src:

```powershell
node gc.caja/Tests/Clover.Tests.cjs
./gc.caja/Tests/Clover.Servidor.Tests.ps1
./gc.caja/Tests/Clover.Servidor.Tests.ps1 -Serve
```

La última orden abre un host exclusivo de pruebas en http://127.0.0.1:7293. No necesita credenciales reales; la identidad ficticia sólo existe en el ejecutable de Tests. Elegir escenario y operación, pulsar Abrir Clover y probar monto/cuotas/QR/ID. Para espera, pulsar Aprobar espera simulada. Reiniciar simulación descarta sólo ese escenario ficticio; no representa anulación de un pago real. Los recursos del simulador se sirven únicamente por el host de Tests.

El SDK instalado era 11.0.100-rc.2 y produjo un conflicto de endpoints DotNetWatch al generar recursos del host. Se deshabilitó StaticWebAssets exclusivamente en el proyecto efímero de Tests; UseStaticFiles sirve los recursos de wwwroot de Caja directamente. No se modificó el SDK, la instalación de Visual Studio ni la configuración de producción.

### Observaciones concretas al DBA sobre los SP recibidos

1. Valores_Ins_Opciones declara @ins_id char(2), pero el caso documentado G00 tiene tres caracteres. Puede truncarse a G0 y no devolver sus opciones. No arreglarlo truncando el código en Caja.
2. Clover_Pay activa consulta manual únicamente con LEN(paymentid) > 3. Un ID de exactamente tres caracteres, válido en el documento, entra a SP_Payment (cobro). Hasta corregirlo, el prototipo no envía recuperación con longitud tres; no se convierte el ID en vacío ni se rellena artificialmente.
3. La asignación SELECT @paymentid = @paymentid no copia la columna paymentid de @clov_pay. En pago POS se entra con vacío y se devolvería/guardaría vacío. La simulación sin-id demuestra que ese supuesto OK debe quedar incierto, sin finalizar ni volver a cobrar.
4. Se calcula @ins_id desde instrumentos, pero el INSERT de recibos_anticipados utiliza cardTransaction_cardSymbol (o QR) en lugar de @ins_id. Aclarar consistencia del código guardado con el devuelto y con el catálogo habilitado.
5. En recuperación manual, rb_importe se devuelve desde @monto y la cuota desde @cuota solicitados; no se toman necesariamente del registro pendiente. Comparar con pos_importe y el registro original antes de aceptar; falta confirmar cómo recuperar el recargo original y el plan sin reconstruirlos desde datos arbitrarios de la pantalla.
6. El control de pendiente mostrado filtra por caja_nro_proceso y paymentid; no prueba por sí mismo pertenencia a cliente/PV/cierre ni reserva exclusiva frente a dos solicitudes. Confirmar garantías del SP final y de SP_Payment_Get. El documento de esta tarea no autoriza cambios al SP.
7. rb_compte/external reference se componen con MAX de operación y COUNT de anticipados sin exclusión visible. Revisar concurrencia y correlación ante dos solicitudes y POS compartido antes de habilitar reintentos.
8. El SELECT no incluye rb_rec ni id_externo separados. El ID disponible es rb_dato1_valor; falta confirmar su asociación exacta en Ope_Confirmar. El recargo del modelo es el calculado en la preparación simulada, no un dato certificado por el proveedor o el servidor real.
9. El SP comprueba err_code/err_message/err_type, pero no se ve una condición explícita de aprobación basada en result/state. Confirmar qué devuelve SP_Payment en cancelación, error u offline y cuándo puede insertarse un anticipo.

### Límite de la etapa inicial 1.7 (antecedente, superada por el avance 1.8)

La señal de finalización se prueba, pero no está conectada a la registración de Caja ni a persistencia/recuperación real de aprobaciones. Tampoco se incorporó la validación canónica bancaria al servidor ni se modificó el total final de Checkout para recargos Clover reales. No se declara idempotencia persistente, recuperación tras reinicio del servidor ni soporte real de POS compartido. Esos puntos siguen en el plan y requieren resolver los SP/ejemplos de respuesta antes de conectar el cobro, incluso en la instalación de pruebas.


## Integración 1.8 — aplicación lista para pruebas con el entorno del tester

Autorización posterior del responsable: conectar SP y funcionalidades completas en desarrollo, sin modificar los procedimientos. Sustituye el bloqueo HTTP 503 de la primera etapa. No se ejecutó SQL ni POS real durante esta implementación.

### Circuito conectado

1. Caja autentica, deriva sucursal/PV/proceso/cierre/cliente/operación desde sesión y consulta el catálogo vigente.
2. CloverOpciones en API ejecuta SPGECO_CAJA_Valores_Ins_Opciones; Procesar vuelve a comprobar opciones y calcula el recargo con decimal y redondeo alejándose de cero.
3. Prevalidar productos, lista de precios, deuda seleccionada, NC, otros valores, plazos/tope y estado PV. CD sólo admite opciones sin ajuste financiero.
4. CloverProcesar llama SPGECO_CAJA_Clover_Pay con monto bruto, plan POS, QR y cashback cero. Paymentid vacío solicita POS; ID normalizado recupera un pago existente. El ID de longitud tres sigue bloqueado por el defecto conocido del SP.
5. Guardar en API antes de invocar y después de recibir; validar respuesta de negocio, cliente, ID, lote/cupón/fecha, plan, total POS e instrumento. Respuesta incompleta o discrepante queda incierta; no se fabrica una aprobación.
6. Grilla bloquea modificación/eliminación del aprobado. El resumen suma rb_rec una sola vez al total exigible y el JSON lleva rb_importe bruto y rb_rec exclusivamente como recargo o descuento firmado. Ejemplo base 100, ajuste 12: total 112 y rb_rec 12. Base 100, descuento 5: total 95 y rb_rec -5. Se preserva ese signo al normalizar OPENJSON.
7. Al cubrir saldo, disparar el finalizar existente una sola vez, sin confirmación adicional. Parciales permiten completar. CR/CF, CD, CC y CV/IV conservan su confirmación/contrapartida, emisión, stock y navegación.
8. Checkout compara todas las aprobaciones con el diario antes del SP final, impidiendo omisión, duplicación o modificación. La API protege también el SP final y conserva su resultado para reenvíos idénticos.
9. Reingreso repone canónicos, otros valores/NC y catálogo. Si el registro ya está confirmado se recupera el comprobante sin ejecutar nuevamente el SP. Fallo conocido de registración permite corregirla; resultado incierto se bloquea para conciliación.

Se extrajeron las validaciones existentes de NC a PagoConCreditosController para compartir exactamente las mismas reglas antes y después del cobro. El cambio no introduce una segunda política de NC.

### Despliegue

- Compilar y desplegar **gc.api y gc.caja** juntos, con sus dependencias. Refrescar sin recompilar no incorpora los controladores/servicios nuevos.
- Configurar en API **Clover:DirectorioEstado** con una carpeta durable fuera del directorio de publicación; ejemplo D:/GecoDatos/Caja/Clover. Si no se define, usa App_Data/Clover dentro del ContentRoot de API. La identidad del servicio necesita lectura, escritura y exclusión de archivos. Respaldar esta carpeta y conservarla entre versiones; no eliminarla para destrabar un cobro.
- Ningún secreto/PAN/CVV se captura en la web. El diario contiene IDs, importes y contexto operativo y debe tener permisos de servicio. No subir sus datos al versionador; App_Data/Clover tiene .gitignore.
- Si hay varias instancias de API deben compartir la MISMA carpeta y exclusión de archivos, o migrar este diario a persistencia transaccional coordinada con DBA. No utilizar diarios independientes para los mismos puestos.
- SQL existente espera hasta 600 segundos; HTTP exclusivo de Clover en Caja espera 660 y navegador 690. No hay reintento bancario automático. En IIS/proxy externo verificar que el timeout de solicitud permita ese recorrido (mínimo 12 minutos si hay proxy fuera de proceso). No se cambian globalmente los timeouts de los demás medios.
- En SQL deben existir los SP y el catálogo G00. **La aplicación no altera ni suple las correcciones SQL pendientes**: char(2) frente a G00, asignación del ID POS, rama manual >3, instrumento guardado/devuelto, recargo original de recuperaciones, aprobación real/result-state e imputación única. Ver lista DBA anterior.
- Si opciones no devuelve G00, muestra error y no cobra. Si el SP devuelve OK con ID vacío, conserva estado incierto y no vuelve a cobrar. Son controles de diagnóstico, no integración desactivada por un flag.
- La exclusión implementada es por sucursal/PV. La concurrencia de distintos PV que comparten físicamente el mismo Clover debe coordinarla el servicio/POS o un contrato del DBA que identifique el dispositivo. No se afirma que una exclusión de PV resuelva esa configuración.

### Evidencia de aceptación local

- Clover.Integracion.Tests.ps1: 119 comprobaciones de contratos/parametrización de SP, QR/manual, bruto/recargo, firma de descuento, dos Clover parciales, duplicados, manipulación, CD sin recargo, aislamiento, reinicio, incertidumbre y confirmación persistida. SQL/POS sustituidos sólo en Tests.
- Clover.Servidor.Tests.ps1: 19 verificaciones HTTP, render Razor real, BFF -> API HTTP -> adaptador de SP simulado y Checkout real -> confirmación. El json_valores observado lleva rb_importe 112 y rb_rec 12; un reenvío no genera otro cobro.
- Clover.Tests.cjs: 60 escenarios de modelo/DOM y funciones reales del motor para preservar ID/lote/cupón, sumar el recargo una vez y finalizar sólo al cubrir el saldo.
- Regresiones aprobadas: 99 de finalización, 16 DOC, 43 NC, 32 reglas más 4 regresos CD/doble confirmación/recarga. Compilaciones de Caja/API aisladas sin errores; advertencias preexistentes en la compilación inicial de dependencias.
- No se certifica el proveedor, el SP instalado, la impresión fiscal ni una operación bancaria real: esa aceptación corresponde al entorno de pruebas del tester con los SP corregidos.

Comandos adicionales desde src:

```powershell
./gc.caja/Tests/Clover.Integracion.Tests.ps1
./gc.caja/Tests/Clover.Servidor.Tests.ps1
node gc.caja/Tests/Clover.Tests.cjs
```

La bitácora 1.8 está actualizada localmente. Publicación en NotebookLM sigue pendiente; no se declara sincronización remota.


## Corrección de arranque compartido — 2026-10-24

El usuario informó AggregateException al construir gc.api.report: AddServicios registraba ApiPagoFactServicio, que ahora requiere CloverServicio, pero CloverServicio y CloverRegistro se habían agregado únicamente en Program de gc.api. La compilación previa de gc.api no cubría ese defecto del registro utilizado por otros hosts.

Se centralizaron ambos registros en gc.api.infra/Extensions/ServicioExtensions.cs y se quitó el registro particular de gc.api/Program.cs. CloverServicio conserva lifetime scoped y CloverRegistro singleton; TryAdd evita duplicados y respeta sustituciones explícitas. La fábrica del diario usa IConfiguration e IHostEnvironment del host y conserva Clover:DirectorioEstado o ContentRoot/App_Data/Clover. No crea carpetas al registrar o validar el contenedor; sólo al resolver el diario por primera vez.

Regresión reproducible: ./gc.caja/Tests/Clover.Dependencias.Tests.ps1. Sus 35 verificaciones reproducen el fallo sin registro y comprueban el registro común utilizado por gc.api, gc.api.report y gc.api.gen, la construcción de ApiPagoFactServicio real, validación del grafo completo, scopes, diario singleton y configuración de ruta. El DbContext permanece desconectado; no se invocan SP ni POS. Las salidas de compilación se aíslan en .artifacts/clover-di para no bloquear Visual Studio.

Aclaración de alcance del responsable: el tester también es DBA y conoce la red de SP auxiliares. Las observaciones SQL anteriores son alertas para contrastar con él, sin asumir ausencias de validación en el circuito completo ni alterar SP desde este desarrollo. La corrección de esta fecha se limita al registro de la aplicación y sus pruebas. La bitácora vigente pasa a 1.9 local; publicación NotebookLM pendiente.


## Corrección del acceso POS Clover T1 — 2026-10-24

Se reprodujo en la factura abierta: el catálogo de medios presenta tcf_id T1 / POS - Clover; la búsqueda de instrumentos devolvía DEF / Sin instrumentos disponibles. La condición anterior sólo reconocía ins_id G00 y derivaba al formulario temporal de Transferencia. Una consulta SELECT de lectura al catálogo de desarrollo no encontró G0 ni G00 en instrumentos ni filas para ellos en instrumentos_opciones. No se modificó SQL ni se ejecutaron pagos; el responsable avisó que coordinará la carga del dato con el analista y el DBA.

El motor ahora reconoce el medio T1 y abre directamente el Razor Clover. Sus accesos alternativos de instrumento, alta directa y CV/IV tienen el mismo desvío, antes del fallback electrónico temporal. DEF nunca se envía como instrumento de cobro. La API comprueba T1 en el catálogo y resuelve el código de instrumento disponible: usa el solicitado si está habilitado o el único configurado para T1. Si no existe o hay ambigüedad, informa el problema dentro del modal. No impone G0/G00 ni renombra los códigos recibidos. Las cuotas definen el instrumento que se conservará en la solicitud; el identificador canónico del valor aprobado sigue siendo el devuelto por Clover_Pay.

Para completar las pruebas del entorno real, las consultas SPGECO_CAJA_Valores_Ins(T1, operación, cliente, sucursal) y SPGECO_CAJA_Valores_Ins_Opciones deben devolver el instrumento/opciones configurados por el analista/DBA. El detalle exige cuota, recargo_porcentaje, pos_plan y pos_desc. No es necesario retocar el código de Caja sólo porque el identificador válido sea G0 en lugar del G00 del documento inicial.

Pruebas: 65 escenarios JS (incluidos T1 sin instrumento, DEF y G0), 124 del servicio/persistencia y 21 HTTP/Razor/API/Checkout aprobados. Regresiones: 99 finalización, 16 DOC, 43 NC y 32 reglas más navegación/doble envío. Verificación visual de /ruta-clover en el host aislado: seleccionar T1 abre Pago Clover, sin catálogo informa ausencia de cuotas y con opciones carga monto/cuotas/QR/recuperación; base 100 y plan 12 % muestran recargo 12 y total 112. No se pulsó Pagar POS en el navegador ni se usó un dispositivo real. La página de verificación y el transporte SQL simulado sólo existen en Tests.

Recompilar y reiniciar gc.caja y gc.api; después recargar la pantalla. Las compilaciones y los hosts de pruebas usan .artifacts para no bloquear Visual Studio. Bitácora vigente 1.10 local; publicación NotebookLM pendiente.

## Prueba del entorno de desarrollo y coordinación POS — 2026-10-24

Se descartó el borrador anterior de Facturación y se reinició desde Operar Caja. La sesión anterior permitía cargar productos sin contexto completo de puesto, pero Clover/Estado rechazó esa preparación antes de cobrar. Al validar el puesto se reconocieron caja 0001, proceso 00-06578 y cierre 1; se seleccionó nuevamente C0004580 y se cargaron dos productos por 111420.68. T1 abrió el modal propio Pago Clover. La consulta Estado confirmó ok=true, incierto=false, pendiente=false y aprobados vacío.

La solicitud de opciones quedó detenida en la versión anterior de API: la excepción informada por el responsable dice «El instrumento solicitado no corresponde a Clover.». Ese literal está en gc.api/bin/Debug/net8.0/gc.api.core.dll compilada a las 09:32 y no en CloverServicio.cs actualizado a las 12:30. Se solicitó recompilar y reiniciar gc.api y gc.caja antes de repetir. Una consulta SELECT posterior encontró G00 / POS Clover en instrumentos, sin filas para G0/G00 en instrumentos_opciones. Configurar y verificar las cuotas con el analista/DBA; no alterar los SP desde esta intervención ni afirmar integración POS aprobada.

El responsable aclaró que el POS está con el tester en Córdoba. Avisar y esperar coordinación antes de Pagar POS; hasta entonces limitarse a la preparación, consultas y cálculos del modal. No se enviaron solicitudes de pago, recuperación manual ni confirmación de factura. Las pestañas de aplicaciones se cerraron durante el reinicio de Visual Studio. Bitácora vigente 1.11 local; publicación NotebookLM pendiente.

## Verificación tras recompilación y corrección visual — 2026-10-24

Se verificaron gc.api.core.dll compilada a las 12:57:28 y gc.caja.dll a las 12:57:37: la DLL API ya contiene la validación actual y no el mensaje anterior. Con sesión renovada, caja 0001 y cliente C0004580, una nueva factura de dos productos por 111420.68 abrió T1 / Pago Clover sin la excepción previa. La respuesta de opciones es vacía: el catálogo de lectura conserva G00 / POS Clover y no contiene filas G0/G00 en instrumentos_opciones; el modal muestra el aviso correspondiente y deja deshabilitados cuotas y Pagar POS. Falta obtener las opciones reales antes de probar cálculos del entorno y el circuito completo.

La captura real reveló un defecto visual no cubierto por la página aislada anterior: Clover tenía z-index 1090 y modalPago 5070, por lo que la ventana abierta no se veía. Se corrigió únicamente wwwroot/css/pages/pago-clover.css al nivel común de detalles: modal 5100, diálogo 5101 y fondo 5099. El mismo componente es utilizado por CR/CF/CD/CC y CV/IV; se verificó visualmente Facturación con modalPago abierto y se comprobó que elementFromPoint dentro del diálogo corresponde a modalDetalleClover. Volver cerró Clover sin valores registrados, Agregar siguió habilitado y T1 reabrió correctamente conservando el total. Captura local: clover-real-modal-corregido-20261024.jpg en la carpeta de visualizaciones de esta conversación.

Los 65 escenarios de Clover.Tests.cjs pasan con transporte simulado. Cambio CSS servido tras refrescar, sin detener Visual Studio ni modificar SP. No se pulsó Pagar POS ni Cargar manual, ni se confirmó factura. Se mantiene la coordinación previa con responsable y tester de Córdoba. Bitácora vigente 1.12 local; publicación NotebookLM pendiente.

## Cuotas entregadas y contrato char(3) verificado — 2026-10-24

El responsable entregó el SP actualizado con @ins_id char(3) y tres opciones G00: cuota 1, 0 %, pos_plan 1, En 1 Pago; cuota 2, 13.25 %, pos_plan 2, En 2 Pago; cuota 3, 19.17 %, pos_plan 3, EN 3 PAGO. Se verificó sys.parameters.max_length=3 y se ejecutó únicamente SPGECO_CAJA_Valores_Ins_Opciones con G00 / CR / C0004580 / 0000. El mismo GenericDataMapper<CloverOpcionDto> utilizado por Repository convirtió correctamente las tres filas, incluido pos_plan. La aplicación no modificó SQL. La discrepancia inicial char(2)/G00 queda cerrada; los pendientes ajenos a esa firma permanecen bajo revisión del tester/DBA.

El ejemplo del responsable usa T1 en el segundo parámetro. T1 identifica el medio; co_tipo conserva CR/CF/CD/CC/CV/IV según el contexto. El SELECT entregado no utiliza co_tipo, pero la aplicación mantiene su contrato y revalidación del catálogo por operación. Se ajustó el SqlParameter de opciones de VarChar(3) a Char(3), y se actualizó el comentario del DTO. El modal ya carga las cuotas desde la API: no se incorporaron porcentajes fijos ni alternativas sintéticas en producción. CD sigue admitiendo sólo la opción sin recargo.

Se incorporaron las filas suministradas exclusivamente a fixtures y tres regresiones JS que comprueban recargo, bruto, cuota real, pos_plan, G00 y CR enviados al transporte simulado. Las 68 verificaciones JS, 124 de servicio/integración y 21 HTTP pasan; las compilaciones aisladas de Caja/API terminaron sin errores, con advertencias existentes de la solución. El proyecto principal estaba detenido por el responsable. La verificación visual usó el Razor y los controladores reales en 127.0.0.1:7293, con todos los SP de ese host simulados y una copia JSON de las opciones consultadas; no certifica una prueba bancaria ni del circuito autenticado del proyecto principal.

En el navegador: base 100 y planes 1/2/3 muestran recargos 0/13.25/19.17 y totales 100/113.25/119.17. Editar base a 50 y elegir dos pagos muestra recargo 6.63 y total 56.63. Captura de tres pagos: clover-cuotas-configuradas-20261024.jpg en las visualizaciones de la conversación. No se pulsó Pagar POS ni Cargar manual. Servidor aislado y pestaña cerrados al finalizar; la instancia del usuario permanece detenida para recompilar en Visual Studio.

Reproducción visual opcional: ./gc.caja/Tests/Clover.Servidor.Tests.ps1 -Serve -OpcionesJson <archivo JSON de opciones>. Este parámetro sólo funciona en la demostración aislada y no cambia los fixtures de las regresiones normales. No indicar credenciales ni conexiones en ese JSON. Bitácora vigente 1.13 local; publicación NotebookLM pendiente.

## Aceptación de cuotas en la instancia autenticada de Caja — 2026-10-24

El responsable recompiló y levantó los proyectos y pidió probar las cuotas antes de coordinar cualquier cobro con el tester de Córdoba. Se utilizó la sesión autenticada de Caja en https://localhost:7257, caja 0001 / proceso 00-06578 / cierre 1. Desde el menú principal se abrió Facturación, se identificó C0004580 y se cargaron 034964 por 1 unidad y 078353 por 2.450 unidades: total 111420.68. No se usó el host de simulación ni se levantaron procesos adicionales.

El circuito principal de Caja, API y SP de desarrollo devolvió las tres opciones y las mostró en el modal Clover. Base 111420.68: un pago, recargo 0 y total 111420.68; dos pagos, recargo 14763.24 y total 126183.92; tres pagos, recargo 21359.34 y total 132780.02. La edición a 50 en dos pagos dio recargo 6.63 y total 56.63. Los importes 111420.69 (excede saldo por un centavo) y 0 deshabilitaron Pagar POS. Se restauró el importe completo y se guardó una captura con tres pagos en clover-cuotas-caja-real-20261024.jpg, dentro de las visualizaciones de la conversación.

No se pulsó Pagar POS, Cargar manual ni Finalizar. La grilla permanece sin valores registrados y la factura no se confirmó. El modal queda abierto para continuar cuando el responsable coordine con el tester; este resultado certifica la carga/cálculo de opciones del entorno principal de desarrollo, no el débito bancario, aprobación POS, canonización de una respuesta real ni la confirmación final. No hubo cambios de código ni SQL en esta prueba. Bitácora vigente 1.14 local; publicación NotebookLM pendiente.

## Diagnóstico HTTP de Clover y confirmación — 2026-10-26

El responsable solicitó ver request y response sin los escapes duplicados de una cadena JSON y aclaró que los detalles que el tester observa en base quedan a cargo de éste. Se agregó diagnóstico en CloverApiServicio para opciones, estado y procesamiento y en CheckoutServicio.FinalizarCompra cuando clover_operacion_id está informado. El log existente de Caja recibe [CLOVER][REQUEST], [CLOVER][RESPONSE] y, para fallos de transporte del adaptador Clover, [CLOVER][SIN_RESPONSE]. Request/response comparten Llamada, con acción, URL, estado HTTP y duración. La configuración actual gc.caja/log4net.config dirige los mensajes a d:/Sitios/Logs/GC/gc.caja.log, Trace y consola; no se creó un destino de datos externo ni un nuevo flag de activación.

El cuerpo se serializa una sola vez como application/json. Las pruebas capturan el HttpRequestMessage efectivo y verifican raíz objeto, contexto/pago objetos y valores/uniones arreglos. CloverJsonDiagnostico muestra el JSON con sangría y expande objetos/arreglos que venían contenidos como texto únicamente en la vista del log. Los DTO, el cuerpo HTTP, los json_* de confirmación y sus datos literales se conservan; no se aplicó un Replace indiscriminado de barras. El request anterior de Checkout y el de CV/IV utilizan esa vista legible cuando contienen Clover. No se cambió la red de SP, el diario, las reglas de aprobación o las solicitudes ya registradas.

No se vuelcan cabeceras ni token de autenticación; la vista del log también omite el token actual si una respuesta de error lo refleja. Las respuestas HTTP de error, no JSON, inválidas o vacías se registran antes del tratamiento previo correspondiente. Un error de red conserva su excepción y no provoca reenvío automático ni se presenta como respuesta del proveedor.

Verificación: ./gc.caja/Tests/Clover.Log.Tests.ps1 aprobó 28 casos con HttpMessageHandler simulado; ./gc.caja/Tests/Clover.Servidor.Tests.ps1 aprobó 26 HTTP/Razor/API/SP/Checkout con SQL/POS simulados. Se añadió la llamada por el CheckoutServicio real para registrar de nuevo una confirmación idéntica: se recupera el resultado y los contadores de registro/POS no aumentan. Se verificaron logs de ambos sentidos, token omitido, json_valores legible como arreglo y conservación del JSON original para el SP. Compilación aislada sin errores. No hubo SQL, cobros ni confirmaciones reales en esta intervención.

Ejemplo reproducible con datos ficticios: .artifacts/clover-log-tests/ejemplo-clover.log. Las salidas usan .artifacts para no bloquear los procesos del usuario. Recompilar y reiniciar gc.caja para activar la nueva versión de gc.caja.core; no hace falta cambiar SP ni configurar otro destino de log. Bitácora vigente 1.15 local; publicación NotebookLM pendiente.
