# NC por devolución — mapeo de entrada y revisión para DBA

Fecha: 2026-10-28. Complemento técnico de `BitacoraUnicaReglasCaja.md`, DEV-06/07 y FAC-17/20/21. La bitácora conserva la autoridad funcional.

## Estado de la corrección en Caja

Corregido el mapeo exclusivo de `NotaCreditoController.MapearProductosModeloNC_aModeloFact`. El constructor genérico de `ProductoFactJsonDto` dejaba el combo vacío y los bultos en cero; ahora se copian los datos recibidos de `NCProductoBuscarResponseDto`.

| Dato de NC_B_Producto | JSON de entrada a Calcula_Filas | JSON devuelto por el cálculo y enviado a confirmar |
| --- | --- | --- |
| `cmd_cmb` | `cmd_cmb` | `cmb` |
| `cmd_cmb_id` | `cmd_cmb_id` | `cmb_id` |
| `cmd_cmb_dto` | `cmd_cmb_dto` | `cmb_dto` |
| `cmd_cmb_cant` | `cmd_cmb_cant` | `cmb_cant` |
| `cmd_cmb_desc` | `cmd_cmb_desc` | `cmb_desc` |
| `bultos` | `bultos` | Lo devuelto por el cálculo |

El OPENJSON entregado de Calcula_Filas confirma que **la entrada usa cmd_cmb_***. No corresponde renombrarla a cmb_*; esos nombres pertenecen a su salida. La confirmación usa literalmente esa salida, sin proyectarla nuevamente al DTO.

Se conservan ceros iniciales, espacios de los textos, cantidades decimales y signos. La ausencia de un dato numérico nullable mantiene la compatibilidad de entrada con cero; no se inventa un combo cuando el origen devuelve `0` o datos vacíos. El mapeo no convierte unidades ni divide impuestos por la cantidad. No se cambia el DTO compartido ni el comportamiento de otros módulos.

## Fuentes contrastadas

Definiciones completas entregadas por el responsable el 2026-10-28, con fechas de script 20:42:42 y 20:42:59:

- `SPGECO_CAJA_NC_B_Producto`: adjunto be011920-7be4-421d-92fc-1b19bd4d85d4.
- `SPGECO_CAJA_Ope_Calcula_Filas`: adjunto bb31bc71-b359-4e74-b06c-623019e06838.

Las líneas siguientes corresponden a esos textos. El DBA debe contrastarlas con la versión efectivamente desplegada. No se modificó, ejecutó ni desplegó ninguno de estos scripts durante la corrección.

## 1. El cálculo reinicia el combo y no lo recupera para una devolución

En Calcula_Filas, el bloque «Seteo» asigna `cmb = 'N'`, `cmb_dto = 0` y `cmb_cant = 0` (líneas 253–255), incluso cuando la entrada contenía un combo válido.

La aplicación posterior de promociones se ejecuta únicamente si `LEN(ISNULL(@tco_id_ori,'')) < 3` y el tipo corresponde a FT/TK (línea 356). Para una devolución con origen `006`, esa condición no se cumple.

Al crear `json_subtotal`, el SQL sólo incorpora CO cuando `cmb = 'S'` y `cmb_dto <> 0`, con importe `SUM(cmb_dto) * -1` (líneas 553 y siguientes). Así, aunque Caja ya envíe correctamente los datos históricos, el script entregado puede eliminar la bonificación de la NC.

**Revisión solicitada al DBA:** definir el tratamiento de los combos del comprobante original en la devolución total y parcial, conservar su distribución histórica y garantizar una salida consistente de `json_p` y `json_subtotal`. No resolverlo desde Caja borrando la referencia de origen ni recalculando promociones vigentes como si fuera una venta nueva.

## 2. El detalle de devolución consultado ya trae el combo en cero

La prueba real previa utilizó la Factura B 0001-00000067, producto 031515, seis unidades. El request de confirmación de esa factura contenía `cmb=S`, `cmb_id=006787`, importe 8201.87, cantidad 6 y descripción; el SP respondió OK. No se inspeccionó la persistencia interna de la red SQL.

Al consultar el detalle para devolver, la web recibió `cmd_cmb="0"`, `cmd_cmb_id="0     "` y `cmd_cmb_desc="0"`.

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

## 4. Condición del combo al cargar productos manualmente

NC_B_Producto contiene `Len(IsNull(cmd_cmb_id,'')) = @cmb_id` (línea 188). Compara la longitud del identificador con el identificador del combo: para 006787, la longitud es 6. Revisar esa comparación y la selección de los productos asociados a la misma promoción. Es una observación concreta del código recibido; el ajuste SQL queda a cargo del DBA.

## Pruebas de la aplicación

La nueva regresión falló antes de corregir, al comprobar la bandera del combo en la entrada del SP. Después pasaron **245 comprobaciones** de la suite de JSON, incluidas 92 nuevas verificaciones para NC:

- AA y DV a través del controlador real SeguirCalculo, sesión y servicios reales de cálculo/confirmación con repositorio SQL simulado.
- Entrada cmd_cmb_* completa; importe positivo y negativo del combo; p_in, IVA y neto conservados; bultos y cantidades fraccionarias sin truncar.
- Salida cmb_* conservada literalmente hasta @json_p; sin agregar cmd_cmb_* a la salida ni reconstruir el JSON calculado.
- CO negativo hasta @json_subtotal; ceros iniciales y espacios finales; origen sin combo conservado como tal.

Pasaron además 163 pruebas de cálculo/confirmación con signos, 385 del normalizador, 47 de carga NC, 51 HTTP/Razor/API/Checkout y las pruebas JS de finalización, avisos, mensajes y retorno de NC. La compilación aislada finalizó sin errores; existen advertencias previas en la solución.

Los SP de las pruebas devuelven fixtures controladas. Esto verifica el mapeo y transporte de Caja; **no acredita que los SQL entregados ya calculen correctamente la devolución**.

Para reproducir: ejecutar `gc.caja/Tests/ProductosJson.Integracion.Tests.ps1` desde la raíz de src. Genera en `.artifacts/caja-productos-json/evidencia` los archivos `nc-entrada-calculo.json`, `nc-salida-confirmacion.json` y `nc-subtotal-confirmacion.json`. La salida coincide byte por byte con la fixture de productos, SHA256 `4fd4a0b2c66caa0c09ece89562237b0999884d7b3d33bd20d87b8f1d92b9ff5e`.

## Aceptación pendiente

Recompilar/reiniciar gc.caja para incorporar el mapeo. El DBA debe confirmar los puntos anteriores en su versión desplegada y revisar el caso total y los parciales. Después repetir el cálculo real con el combo recuperado y los importes conciliados; recién entonces preparar la emisión con confirmación personal del responsable.

No se emitió ninguna NC, no hubo devolución de dinero ni llamadas al POS, y no se alteraron SP. La factura de prueba anterior permanece registrada; su devolución sigue sin emitir.
