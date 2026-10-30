# NC por devolución — mapeo de entrada y revisión para DBA

Fecha: 2026-10-29. Complemento técnico de `BitacoraUnicaReglasCaja.md`, DEV-06/07/08/09/10/11/12/13/14/15 y FAC-17/20/21/22/23/24. La bitácora conserva la autoridad funcional.

**Aclaración vigente — 2026-10-29:** el responsable rectificó el número informado por el tester: el correcto es `0203-00013796` (4 + guion + 8). El cero final de `0203-000137960` era un error del ejemplo. La propuesta de modificar el parseo de B_Producto_Datos queda retirada; sus posiciones originales son correctas para este contrato. cm_ib textual y los pendientes de combos/impuestos de las secciones 1–4 son temas independientes.

## Estado de la corrección en Caja

Corregido el mapeo exclusivo de `NotaCreditoController.MapearProductosModeloNC_aModeloFact`. El constructor genérico de `ProductoFactJsonDto` dejaba el combo vacío y los bultos en cero; ahora se copian los datos recibidos de `NCProductoBuscarResponseDto`. Con los SP corregidos de las 21:33/21:36, un único serializador arma el JSON canónico para agregar productos manualmente y calcular la devolución.

| Columna del SELECT de NC_B_Producto | JSON hacia NC_B_Producto y Calcula_Filas | JSON devuelto por el cálculo y enviado a confirmar |
| --- | --- | --- |
| `cmd_cmb` | `cmb` | `cmb` |
| `cmd_cmb_id` | `cmb_id` | `cmb_id` |
| `cmd_cmb_dto` | `cmb_dto` | `cmb_dto` |
| `cmd_cmb_cant` | `cmb_cant` | `cmb_cant` |
| `cmd_cmb_desc` | `cmb_desc` | `cmb_desc` |
| `bultos` | `bultos` | Lo devuelto por el cálculo |

Los OPENJSON corregidos de **ambos SP leen cmb_***. La salida del SELECT de NC_B_Producto todavía utiliza alias cmd_cmb_*; el DTO conserva esos nombres para leer sus columnas y las respuestas de la API. Al armar json_p, Caja adapta estructuralmente las cinco propiedades a cmb_*, sin reemplazar texto JSON ni cambiar sus valores. No salen propiedades cmd_* en ese parámetro. La confirmación usa literalmente la salida del cálculo, sin aplicar ese adaptador ni proyectarla nuevamente al DTO. Esto sustituye el contrato de entrada cmd_cmb_* observado en los SQL de las 20:42.

Se conservan ceros iniciales, espacios de los textos, cantidades decimales y signos. La ausencia de un dato numérico nullable mantiene la compatibilidad de entrada con cero; no se inventa un combo cuando el origen devuelve `0` o datos vacíos. El mapeo no convierte unidades ni divide impuestos por la cantidad. No se cambia el DTO compartido ni el comportamiento de otros módulos.

## Fuentes contrastadas

Definiciones vigentes entregadas por el responsable el 2026-10-28:

- `SPGECO_CAJA_NC_B_Producto`, script 21:33:35: adjunto deafdf41-b384-4134-b819-640f88408259. OPENJSON usa cmb_* y la comparación del ID ya no utiliza LEN.
- `SPGECO_CAJA_Ope_Calcula_Filas`, script 21:36:57: texto completo recibido como respuesta del responsable a la consulta de contrato. OPENJSON usa cmb_*; se mantiene el bloque de Seteo y la condición por origen descritos más abajo.

Antecedentes: los scripts 20:42:42 y 20:42:59 (adjuntos be011920-7be4-421d-92fc-1b19bd4d85d4 y bb31bc71-b359-4e74-b06c-623019e06838) usaban cmd_cmb_* en OPENJSON. Las referencias de línea son aproximadas; los fragmentos identifican los bloques a revisar en la versión efectivamente desplegada. No se modificó, ejecutó ni desplegó ninguno de estos scripts desde la aplicación.

## 1. El cálculo reinicia el combo y no lo recupera para una devolución

En Calcula_Filas, el bloque «Seteo» asigna `cmb = 'N'`, `cmb_dto = 0` y `cmb_cant = 0` (líneas 253–255), incluso cuando la entrada contenía un combo válido.

La aplicación posterior de promociones se ejecuta únicamente si `LEN(ISNULL(@tco_id_ori,'')) < 3` y el tipo corresponde a FT/TK (línea 356). Para una devolución con origen `006`, esa condición no se cumple.

Al crear `json_subtotal`, el SQL sólo incorpora CO cuando `cmb = 'S'` y `cmb_dto <> 0`, con importe `SUM(cmb_dto) * -1` (líneas 553 y siguientes). Así, aunque Caja ya envíe correctamente los datos históricos, el script entregado puede eliminar la bonificación de la NC.

**Revisión solicitada al DBA:** definir el tratamiento de los combos del comprobante original en la devolución total y parcial, conservar su distribución histórica y garantizar una salida consistente de `json_p` y `json_subtotal`. No resolverlo desde Caja borrando la referencia de origen ni recalculando promociones vigentes como si fuera una venta nueva.

## 2. El detalle de devolución consultado ya trae el combo en cero

La prueba real previa utilizó la Factura B 0001-00000067, producto 031515, seis unidades. El request de confirmación de esa factura contenía `cmb=S`, `cmb_id=006787`, importe 8201.87, cantidad 6 y descripción; el SP respondió OK. No se inspeccionó la persistencia interna de la red SQL.

En esa prueba anterior a los nuevos SQL, al consultar el detalle para devolver, la web recibió `cmd_cmb="0"`, `cmd_cmb_id="0     "` y `cmd_cmb_desc="0"`.

NC_B_Producto obtiene esos campos desde `comptes_vtas_d` mediante MAX/SUM con respaldo ISNULL(...,0) (líneas 245–249). Por eso el mapeo C# corregido conserva lo recibido, pero no puede reconstruir un combo que la consulta ya entrega ausente.

**Revisión solicitada al DBA:** rastrear dónde se guardaron los campos de la factura, contrastar los valores de `comptes_vtas_d.cmd_cmb`, `cmd_cmb_id`, `cmd_cmb_dto`, `cmd_cmb_cant` y `cmd_cmb_desc`, y revisar la lectura/escritura de esos datos en la red de confirmación. Distinguir una pérdida al guardar de una pérdida al consultar. En Ope_Confirmar, contrastar los nombres `cmb_*` recibidos con el mapeo hacia columnas `cmd_cmb_*`.

## 3. Unidad del impuesto interno p_in

NC_B_Producto devuelve `MAX(v.cmd_in) AS p_in` (línea 234). Calcula_Filas usa `p_pvta - p_in` para obtener el neto unitario (línea 293) y luego `cantidad_tot * p_in` para el impuesto total (línea 319).

Valores observados antes de esta corrección:

| Dato | Factura original | Devolución calculada |
| --- | ---: | ---: |
| p_in enviado al cálculo | 2591.88 | 15551.28 |
| Cantidad | 6 | 6 |
| cm_ii | 15551.28 | 93307.68 |
| Total con conceptos | 73816.87 | 82018.74, sin CO |

En la devolución aparecieron `p_pneto=-1554.9504132231`, `p_iva=-326.54`, `cm_gravado=-9329.70` y `cm_iva=-1959.24`. Esos signos se preservan; convertirlos en positivos escondería la discrepancia.

**Revisión solicitada al DBA:** confirmar si cmd_in es unitario o total y alinear el valor devuelto como p_in con la unidad consumida por Calcula_Filas, incluyendo devoluciones parciales y cantidades fraccionarias. Caja copia ese campo sin dividirlo, multiplicarlo ni aplicar valor absoluto.

## 4. Comparación del ID corregida en el SQL recibido

El NC_B_Producto de las 20:42 comparaba `Len(IsNull(cmd_cmb_id,'')) = @cmb_id`. El script corregido de las 21:33 compara `IsNull(cmd_cmb_id,'') = @cmb_id` (aproximadamente línea 188). Esa observación queda resuelta en el texto entregado; la prueba de ejecución real y de selección de todos los integrantes del combo corresponde al tester/DBA.

## Pruebas de la aplicación

La regresión inicial reprodujo la pérdida del combo. Tras cambiar el contrato, la prueba adicional de carga manual falló porque NC_B_Producto recibía cmd_cmb_*; después del adaptador común pasaron **269 comprobaciones** de JSON, incluidas 116 verificaciones específicas de NC:

- AA y DV a través del controlador real SeguirCalculo, sesión y servicios reales de cálculo/confirmación con repositorio SQL simulado.
- Entrada cmb_* completa en consulta manual y cálculo, sin claves cmd_*; importe positivo y negativo del combo; p_in, IVA y neto conservados; bultos y cantidades fraccionarias sin truncar.
- Servicio API real de consulta NC con repositorio simulado y respuesta con alias cmd_cmb_*, leída correctamente por la web y conservada en sesión.
- Salida cmb_* conservada literalmente hasta @json_p; sin agregar cmd_cmb_* a la salida ni reconstruir el JSON calculado.
- CO negativo hasta @json_subtotal; ceros iniciales y espacios finales; origen sin combo conservado como tal.

En esta revisión pasaron además 163 pruebas de cálculo/confirmación con signos, 385 del normalizador, 47 de carga NC y las pruebas JS de finalización, avisos, mensajes y retorno de NC. La compilación aislada finalizó sin errores; existen advertencias previas en la solución.

Los SP de las pruebas devuelven fixtures controladas. Esto verifica el mapeo y transporte de Caja; **no acredita que los SQL entregados ya calculen correctamente la devolución**.

Validación interna privada (GOB-06): los scripts, fixtures y evidencias permanecen sólo en la PC de desarrollo y no se distribuyen en Git. Para reproducir localmente: ejecutar `gc.caja/Tests/ProductosJson.Integracion.Tests.ps1` desde la raíz de src. Genera en `.artifacts/caja-productos-json/evidencia` los archivos `nc-entrada-busqueda.json`, `nc-entrada-calculo.json`, `nc-salida-confirmacion.json` y `nc-subtotal-confirmacion.json`. La salida coincide byte por byte con la fixture de productos, SHA256 `4fd4a0b2c66caa0c09ece89562237b0999884d7b3d33bd20d87b8f1d92b9ff5e`.


## Repetición real del cálculo — 2026-10-29

Tras la recompilación del responsable, se repite en el navegador la devolución total de la Factura B 0001-00000067: caja 0001, proceso 00-06583, cierre 1, seis unidades de 031515. El request y response reales del cálculo están registrados a las 00:58:20, hora de Buenos Aires.

Se confirma el mapeo desplegado: json_p contiene cmb_* sin propiedades cmd_* y conserva bultos=1. El resumen de detalle recuperado ya entrega bandera "0", ID "0     " y descripción "0"; Caja envía exactamente esos datos mediante los nombres canónicos. No está disponible allí el combo 006787 de la factura original.

| Dato | Factura original | Cálculo real de la devolución |
| --- | ---: | ---: |
| Subtotal | 82018.74 | 82018.74 |
| CO | -8201.87 | Ausente |
| Total | 73816.87 | 82018.74 |
| p_in | 2591.88 | 15551.28 |
| cm_ii | 15551.28 | 93307.68 |
| p_iva | 1922.61 | -326.54 |
| cm_iva | 11535.67 | -1959.24 |

La respuesta contiene cmb=N, ID "0     ", importe/cantidad de combo en cero. Sus 36 campos y los cinco importes negativos llegan con los mismos valores desde la API hasta la web. Esta prueba no llega a Ope_Confirmar: se cancela el borrador desde la interfaz, sin NC, devolución de dinero ni POS. No se cambia código de aplicación ni SQL en esta repetición.

La corrección del mapeo queda comprobada en ejecución. Continúan pendientes la recuperación del combo histórico, el tratamiento de la bonificación en cálculo y la unidad del impuesto interno; no se atribuye a Caja la ausencia de una promoción que ya llega en cero desde el detalle. Evidencia privada en `.artifacts/caja-nc-combo-real-20261029`: request/response, detalle, salida original, subtotales, comparación e informe. No se debe confundir esa evidencia real de cálculo con las confirmaciones simuladas de las pruebas automatizadas.

## Aceptación pendiente

Recompilar/reiniciar gc.caja para incorporar el mapeo. El DBA debe confirmar los puntos anteriores en su versión desplegada y revisar el caso total y los parciales. Después repetir el cálculo real con el combo recuperado y los importes conciliados; recién entonces preparar la emisión con confirmación personal del responsable.

No se emitió ninguna NC, no hubo devolución de dinero ni llamadas al POS, y no se alteraron SP. La factura de prueba anterior permanece registrada; su devolución sigue sin emitir.


## 5. Numeración del comprobante — rectificación del ejemplo, 2026-10-29

El responsable corrige la evidencia anterior: la referencia válida es `0203-00013796`, con cuatro dígitos de PV, guion y ocho de número. No corresponde tratar este caso como un comprobante histórico de nueve dígitos ni deducir otro formato por la declaración varchar(15).

Para tipo 082 y repetición 0, el valor usado en la copia tipo V es:

```text
082 | 0203-00013796 | 0
```

Tiene diecisiete caracteres. El bloque recibido de SPGECO_CAJA_B_Producto_Datos lo separa correctamente sin modificaciones:

```sql
tco_id = substring(@valor,1,3)
cm_compte = substring(@valor,4,13)
cm_repetido = substring(@valor,17,1)
```

**La propuesta anterior de cambiar a longitud variable queda retirada. No hay cambio SQL necesario por este motivo.** No se modifica ningún SP. Se conservan los antecedentes en las revisiones de la bitácora y en los archivos privados, identificados como rectificados.

Caja vuelve a limitar el ingreso a cuatro dígitos de PV y ocho del número, manteniendo los ceros y completando por izquierda las entradas más cortas. Se elimina la ampliación a nueve/diez y el supuesto mensaje de incompatibilidad histórica; permanece la validación estructural del formato y de la repetición para impedir claves malformadas. El comprobante correcto carga por el recorrido V habitual. cm_ib sigue string nullable, y se mantienen los logs y el retorno al menú.

Verificación local: 264 comprobaciones (147 integración, 32 JS/vistas, 42 carga factura, 22 identificación, 21 menú), con el número correcto hasta parámetros/constructores simulados y lectura literal de la clave para repeticiones 0, 2 y 9. Caja/Razor compila sin errores. La consulta real sigue pendiente: localhost:7257 rechazó la conexión; el responsable avisa que recompilará/levantará para continuar sin emitir NC. Evidencia privada .artifacts/caja-comprobante-origen/numeracion-4-8-simulada.json y rectificacion-4-8-resultado.txt. No se confunde esa evidencia simulada con una factura encontrada en el servidor.

## 6. Cómo contrastar las pruebas con los logs — 2026-10-29

Se mantienen los registros de diagnóstico de la aplicación, sin editar SP. La propuesta anterior de numeración de la sección 5 quedó retirada tras corregirse el ejemplo. Recompilar y desplegar Caja para aplicar el formato confirmado; la API conserva cm_ib textual y los logs ya agregados.

Buscar el prefijo `[CAJA][COMPROBANTE]` en los archivos configurados para cada host:

- `gc.caja.log`: ENTRADA con lo ingresado por el operador y Solicitud del request web; VALIDACION REQUEST/RESPONSE en Factura emitida; resultado de validación en NC; detalle de la copia admitida y BLOQUEADO para una clave incompatible.
- `gc.api.log`: SP REQUEST con parámetros y SP RESPONSE con filas para SPGECO_CAJA_NC_Valida, SPGECO_CAJA_NC_B_Producto y SPGECO_CAJA_B_PRODUCTO_DATOS, este último sólo para tipo V. Cada par usa la misma Llamada. En una excepción se registra SP ERROR con esa misma Llamada, conservando el error y sin reintentos.

Los documentos JSON se muestran con sangría y saltos de línea, sin envolver todo el contenido como un string escapado. Los escapes legítimos dentro de los datos permanecen. Los productos JSON anidados se expanden sólo en la representación de diagnóstico de la solicitud; el parámetro real no se modifica. cm_ib se observa como texto en la respuesta. Una búsqueda sin filas muestra [] y se distingue de un error.

En BLOQUEADO, `cm_compte` contiene la referencia completa, `clave_no_enviada` muestra la composición propuesta y `enviado_al_sp` es false. La lectura simulada registrada durante el análisis anterior se retira del código vigente al rectificar el formato; sus ejemplos privados anteriores no son respuestas del SP. No se genera un SP REQUEST del detalle si se detuvo antes de invocarlo. Se conserva el detalle que el operador ya tenía cargado.

No se repite el diagnóstico V para cada escaneo normal de productos. No se incluyen tokens/credenciales en estas entradas ni se cambia configuración de archivos o niveles del logger. En el checkout local, log4net ya dirige los archivos a D:/Sitios/Logs/GC; cada entorno debe consultar su configuración efectiva.

Verificación privada: 134 comprobaciones locales (incluyen numeración, cm_ib y 38 controles de diagnóstico), Caja/API compiladas sin errores. El ejemplo .artifacts/caja-comprobante-origen/log-comprobantes-ejemplo.txt está generado por pruebas simuladas e identificado como tal; no se escribió en los logs operativos ni se ejecutó SQL real. Las filas de ese ejemplo no acreditan el comprobante del tester. Pruebas y evidencia fuera de Git, sin emisión de NC/factura ni POS.


## 7. Prueba real de identificación y logs — 2026-10-29

**Antecedente con dato de entrada erróneo:** con Caja/API recompiladas, a las 17:49:52 (hora de Buenos Aires) se consultó por la interfaz NC `082 / 0203-000137960`. El responsable luego rectificó ese cero agregado: este ensayo no corresponde al comprobante válido `0203-00013796`. El request real a SPGECO_CAJA_NC_Valida conserva el número completo; utiliza el proceso 00-06583/cierre 1 desde la caja 0001. El log contiene SP REQUEST a las 17:49:52,484 y SP RESPONSE a las 17:49:52,652 con Llamada 55e67c9391214ad4bfba2d136111d931; la Solicitud web es 0HNPLVN0I0SBB:00000043.

La respuesta es [] (cero filas), y la vista muestra que no encontró un comprobante habilitado. No hubo SP ERROR ni excepción de cm_ib, pero no se acredita el mapeo de ese campo con una fila real porque la consulta no devolvió ninguna. El recorrido se detiene antes de detalle, cálculo y confirmación: no se emite NC, se devuelve dinero ni se contacta POS.

El responsable propone que la diferencia de PV podría explicar el resultado y consultará al Analista. El SP entregado compara tipo/número y hace joins con tipos_comprobantes/condiciones_afip; su WHERE no restringe directamente por caja/PV actual. La base efectiva, la disponibilidad del comprobante y su distribución por sucursal no se inspeccionaron. Se entrega la evidencia para contrastar; no afirmar que el comprobante no existe ni atribuir el resultado a un filtro de puesto no observado.

Evidencia privada .artifacts/nc-082-real-20261029: parámetros y respuesta JSON, extractos de gc.api/gc.caja e informe. Captura guardada con la referencia íntegra y el mensaje. Sin cambios de aplicación/SP, sin tests publicados ni commit/push.


## 8. Estado después de la rectificación

Vigente: formato 4-guion-8 y SP original; sin propuesta de modificación de numeración al DBA. cm_ib textual, logs y salida al menú conservados. Los pendientes de datos de combos/impuestos de otra factura de prueba permanecen independientes. Las pruebas privadas anteriores de referencias de nueve/diez dígitos eran simulaciones basadas en el dato entonces confirmado, hoy rectificado; no prueban que esas referencias existan. Los logs reales anteriores se conservan sin reescribirlos. La siguiente prueba operativa debe usar exclusivamente `082 / 0203-00013796`, sin emitir NC; su resultado real se agrega en el apartado 9.


## 9. Aceptación real con la referencia corregida — 2026-10-29

Con Caja/API recompiladas, NC_Valida devuelve una fila para `082 / 0203-00013796`: request 19:48:45,394 / response 19:48:45,595 (Buenos Aires), Llamada 261d21fe653b48308804721e27aa079b. cm_ib="NN" se conserva como varchar/string y el mapper no lanza excepción. NC_B_Producto recibe el número exacto y adm_id de origen 0007: request 19:48:55,689 / response 19:49:01,902, Llamada d25d451e26474808a80f00545fd4930f. Devuelve 44 productos cargados en la vista. Se cancela el borrador y regresa automáticamente al menú principal (`/?menu=True#`), sin emitir NC. La búsqueda real correcta supera el bloqueo observado con el cero agregado por error; no fue necesario cambiar el puesto ni modificar SQL. Evidencia privada .artifacts/nc-082-corregida-real-20261029.

Como prueba nueva autorizada se copia ese detalle a precios actuales y se prepara una venta de 50 renglones/44 artículos distintos. El responsable confirma por interfaz la Factura B 0001-00000068, pagada sólo con efectivo por 106384.01. Los json_p y json_subtotal de cálculo y confirmación coinciden estructuralmente con precisión decimal, incluido orden y valores; no hay cmd_cmb* en confirmación. La primera NC queda preparada con quince artículos por 45094.25; se verifican cantidades/totales y los impuestos internos de dos gaseosas contra la factura, sin emisión. Los componentes impositivos del queso agregado difieren .01 entre gravado/IVA por redondeo de dos renglones. Al corte inicial de la revisión 1.37 las tres emisiones y sus remanentes estaban pendientes de la acción final del responsable; el circuito se completa a continuación. Esto no acredita una resolución de los pendientes de combos/impuestos de la factura de prueba anterior. Evidencia privada .artifacts/factura-B-68-tres-NC-20261029; sin cambios de aplicación/SP ni pruebas publicadas.


## 10. Tres NC consecutivas y salida al menú operativo — 2026-10-29

El responsable confirma las tres emisiones de la Factura B 0001-00000068: NC B 0001-00000003 por 45094.25 (quince artículos), B 0001-00000004 por 32748.88 (quince) y B 0001-00000005 por 28540.88 (catorce). En conjunto restituyen 44 artículos distintos/cantidad 56.910 de los 50 renglones y el total 106384.01 de la factura. Los conjuntos no se superponen. json_p y json_subtotal son iguales estructuralmente, incluido orden/nombres/valores, entre cada cálculo real y la solicitud recibida por API. Para la comparación textual de descripciones se lee el log API en CP1252 y los archivos Caja/evidencias en UTF8; no se reescriben logs operativos.

NC_B_Producto devuelve 29 artículos pendientes después de la primera NC, catorce después de la segunda y [] después de la tercera. Última respuesta: 20:12:54,721 (Buenos Aires), Llamada 1501c06cc7f74dac80a5d066283cc8ef. La vista queda con cero productos, aviso explícito y Seguir deshabilitado. No se emite una cuarta NC; se limpia el borrador de esa consulta.

A pedido del responsable se refuerza la salida de una NC emitida: tanto Aceptar como cerrar su aviso con X solicitan el menú operativo con menu=true, sin respaldo a una vista inicial, mediante replace y una sola navegación. Se conserva el retorno interno VOLVER para corregir antes de emitir y el tratamiento de errores. Se verifica la salida en la segunda emisión y el cierre con X en la tercera sin pulsar Operar Caja. Pasan 21 verificaciones previas más treinta escenarios de Aceptar/X, rutas y reportes; JS válido. No se modifican SP ni el contrato financiero. Pruebas y evidencias permanecen privadas en .artifacts/factura-B-68-tres-NC-20261029 y Tests; sin commit/push. NotebookLM pendiente.
