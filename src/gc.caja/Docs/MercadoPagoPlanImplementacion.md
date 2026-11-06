# Mercado Pago Point en Caja: contraste y plan de implementación

Fecha: 2026-11-05. Estado: implementado y verificado mediante simulaciones privadas en D:/Sis25/git/GeConnect/src. Pendiente aceptación con catálogo/SP/Point reales coordinada con el responsable. No se ejecutaron SP, cobros, recuperaciones manuales ni confirmaciones fiscales en esta entrega.

## Fuentes y precedencia

- Fuente N24: **20261105 Pago MercadoPago.docx**, leída directamente en el visor de fuentes de NotebookLM, cuaderno Arquitectura de Configuración y Acceso al Sistema de Caja: https://notebook.google.com/notebook/00678ebe-1adf-4b1e-b230-21488e92ae71. Se leyó el contenido original de la fuente; no se tomó una respuesta generada del chat como contrato.
- Definición de **dbo.SPGECO_CAJA_MP_Pay**, entregada por el responsable en esta conversación, Script Date 5/11/2026 18:34:03. Análisis estático; no se verificó su despliegue en la base ni se ejecutó el ALTER PROCEDURE recibido.
- BitacoraUnicaReglasCaja.md: PAG-01 a PAG-09, CLV-20/23/25, FAC-17/20/21 y reglas específicas CD/CC/CV/IV. La bitácora local vigente es posterior a la última publicación verificada en NotebookLM; las decisiones locales posteriores conservan su precedencia.
- Implementación actual de Clover: opciones y contexto de servidor, modelo/vista de pago, integración de grilla, Checkout, transportes, logs y registro compartido de dependencias.

## Contraste funcional

| Aspecto | Clover vigente | Mercado Pago Point documentado y SP recibido | Diseño |
| --- | --- | --- | --- |
| Medio e instrumento | T1; instrumento resuelto por catálogo | T2 en el SP; devuelve M00 | T2 abre su modal propio; validar M00 contra el catálogo. No decidir por una descripción ni convertir la categoría genérica MP en T2. |
| Monto | Remanente y parciales según motor | Remanente de factura/cobranza | Sugerir el saldo real. Conservar pagos parciales y mixtos del motor; revalidar saldo y NC en servidor antes de enviar. |
| Opciones | Valores_Ins_Opciones, cuota/pos_plan separados | Mismo SP para M00; mostrar pos_desc y recargo_porcentaje | Compartir cálculo/opciones neutrales. Enviar pos_plan como @cuota. |
| Tipo | QR booleano | @type smallint: 1 Débito, 2 Crédito, 3 QR | Cuota real mayor a 1 obliga Crédito; con cuota real 1 habilitar los tres tipos. Validar también en API; nunca enviar el valor por defecto 0. |
| Ajuste financiero | rb_importe total, rb_rec porcentaje, rb_aux dinero | @monto se inserta y devuelve; no devuelve rb_rec/rb_aux | Armar esos dos campos desde la opción validada y el cálculo del servidor, como el contrato financiero común vigente. |
| Pago nuevo | Clover_Pay, paymentid vacío | MP_Pay, paymentid vacío; SP_Orders y sondeo SP_Orders_Get internos | La aplicación invoca MP_Pay; no implementa otra conexión REST, consulta de estado Point, webhook ni generación de QR en pantalla. |
| Manual | Clover actual bloquea 3 por la rama SQL | Documento mínimo 3 uppercase; SP consulta sólo con LEN > 3 | Con el SP recibido, impedir envío manual de exactamente 3; admitir 4 a 30. Habilitar 3 sólo después de corregir y contrastar la rama de consulta. |
| Datos de retorno | ID, lote y cupón | ID de pago/referencia en rb_dato1; order_id repartido en rb_dato2 y rb_dato3 | Mostrar ID de pago y Orden MP. Conservar ambas partes sin tratarlas como lote/cupón ni truncar la orden de 60 caracteres. |
| Aprobación | RespuestaDto; un valor por respuesta | respuesta/respuesta_msj y una fila final de valores | Adaptar al mismo contrato HTTP tradicional, conservar el mensaje del SP y la identidad del valor recibido. |
| Registro final | Ope_Confirmar compartido, automático cuando cubre saldo | Igual comportamiento exigido | Agregar el pago una sola vez; si queda pendiente permitir completar; al cubrirlo finalizar sin pregunta ni nuevo cobro. |

## Firma SQL recibida

| Parámetro | Tipo | Origen previsto |
| --- | --- | --- |
| @adm_id | char(4) | Sucursal del puesto/contexto autenticado |
| @caja_id | char(4) | Puesto validado en servidor |
| @caja_nro_proceso | varchar(15) | Proceso vigente |
| @caja_nro_cierre | smallint | Cierre vigente |
| @cta_id | varchar(10) | Cliente de sesión; respetar consumidor final sin cuenta |
| @monto | decimal(15,2) | Monto base más ajuste, validado en servidor |
| @cashback | decimal(15,2), defecto 0 | 0m explícito; no hay función cashback solicitada |
| @cuota | smallint, defecto 1 | pos_plan, no cantidad real de cuotas |
| @type | smallint, defecto 0 | 1/2/3 según selección válida; 0 no se envía |
| @paymentid | varchar(30), defecto vacío | Vacío para cobrar; ID normalizado para recuperar |

El SELECT devuelve respuesta, respuesta_msj, cta_id, ins_id, rb_dato1_valor, rb_dato2_valor, rb_dato3_valor, rb_opcion_cuota, rb_fecha_valor, rb_importe, pos_importe, pos_cashback y tcf_id. La columna ins_id devuelve M00 porque @ins_id no se asigna; la inserción interna usa TD/TC/QR y T2. Se conserva el contrato de salida recibido, sin sustituir M00 por los instrumentos que el SP usa internamente.

Mapeo previsto de json_valores: conservar ins_id, los tres datos, cuota, fecha e importe devueltos; id_externo toma el ID de pago de rb_dato1_valor. rb_rec contiene el porcentaje exacto de la opción y rb_aux el ajuste monetario calculado, ambos con su signo. rb_cupon_manual distingue pago nuevo N de recuperación S; rb_ch_dif y rb_estado siguen N en la confirmación de Caja. La numeración la gestiona el motor existente. No sustituir rb_importe por pos_importe; este último queda disponible para diagnóstico. No confundir el estado interno P de recibos_anticipados con el estado enviado al confirmar.

## Qué debe compartirse y qué permanece por proveedor

Se compartirán los cálculos en centavos/decimal, opción y ajuste monetario, contexto autorizado, identificación de valores aprobados, resumen/grilla, protección contra edición o eliminación y finalización. Los adaptadores de Clover y MP conservan SP, parámetros, validación de ID y presentación de datos propios.

La implementación resolvió cuatro dependencias que impedían una copia aislada del modal:

1. pagoFactura.js calculaba total neto y ajustes sólo desde valor.clover. Ahora el helper neutral incluye el valor canónico MP en totales, grilla, eliminación y salidas.
2. Checkout y la prevalidación compartida suman ajustes de instrumentos T1/T2, conservando adaptadores distintos. Se verificó equivalencia de órdenes MP → Clover y Clover → MP.
3. CV/IV identifica el proveedor por el catálogo de sesión y protege el valor de ambos; MP no recibe un identificador de operación Clover.
4. T2 abre su modal antes del fallback. La captura transitoria se conserva para otros medios; un catálogo MP vacío bloquea la preparación con aviso.

No sumar indiscriminadamente todos los rb_aux: reconocer los ajustes de los proveedores integrados desde el catálogo y su valor canónico. La suma debe ser idéntica en navegador, prevalidación de cada proveedor y confirmación final.

## Plan ejecutado y aceptación pendiente

### 1. Cerrar el contrato y preparar la integración

- Modelar Tipo como enum/valor 1, 2, 3; conservar cuota real y pos_plan por separado.
- Contrastar catálogo real T2/M00 y opciones, sin ejecutar MP_Pay. Consultar estado operativo de Caja por el circuito vigente; no añadir una consulta de estado del dispositivo.
- Precisar la recuperación de IDs de tres caracteres con el DBA. Mientras el SP use LEN > 3, bloquear ese caso con mensaje claro antes de invocar el servicio.
- Definir timeouts coherentes entre navegador, Caja, API y SQL. El SP sondea hasta 60 segundos, además de lo que demoren SP_Orders/SP_Orders_Get; no prometer respuesta en exactamente un minuto ni cortar antes de su espera. Conservar ausencia de reintentos automáticos de cobro.

### 2. Generalizar sólo el núcleo compartido necesario

- Extraer helpers de importes, ajustes por proveedor y valores aprobados, conservando el comportamiento de Clover.
- Aplicar ese cálculo a pagoFactura.js, Checkout y prevalidaciones de ambos proveedores; adaptar CV/IV cuando su catálogo habilite T2.
- Mantener valores aprobados con su identidad, proveedor y datos del SP. No reconstruirlos desde inputs editables ni usar una marca JS como prueba bancaria.
- Mantener la regla de CD: sólo opciones y valores sin ajuste financiero; el backend también la valida.

### 3. Implementar el modal MP y sus estados de interfaz

- Monto sugerido editable; cuotas del catálogo; recargo y total de lectura; tipos Débito/Crédito/QR; Pagar MP; ID y Cargar manual MP; Volver.
- Con más de una cuota real seleccionar Crédito y deshabilitar Débito/QR con explicación visible. Al volver a una cuota, habilitar los tres tipos sin alterar innecesariamente una selección válida.
- Aplicar la regla por cuota, nunca por el número pos_plan: un plan 9 puede representar una cuota.
- ID como texto uppercase tanto en navegador como en servidor, sin relleno ni conversión numérica. Máximo contractual 30; mínimo técnico 4 hasta resolver la discrepancia del SP.
- Estilo de Caja, importes con $, foco/selección del monto y recorrido de teclado. Enter no debe iniciar un cobro accidental mientras se editan campos. Mismo orden de capas de Clover: ventana sobre el pago, avisos y teclado por encima.
- Estados simples y explícitos de UI: carga, preparación, espera, aprobado, rechazo e incertidumbre. No incorporar un framework de workflow, diario durable de POS ni tablas nuevas.
- Volver cancela la preparación; no promete anular un pago aprobado ni detener el Point cerrando el modal.

### 4. Conectar Caja y API al SP

- Controlador MercadoPago de Caja con contexto de sesión, antiforgery, catálogo/cuotas, saldo y NC revalidados.
- Cliente HTTP y servicio MP separados del adaptador Clover, con acciones de opciones y procesamiento y respuesta tradicional RespuestaDto.
- Registrar las dependencias en la extensión común a gc.api, gc.api.report y gc.api.gen; cliente HTTP en Caja con timeout coherente.
- En un resultado exitoso construir el valor canónico desde la fila del SP y el ajuste validado. Una respuesta sin estructura o identidad utilizable se informa para verificar, sin inventar un aprobado ni volver a cobrar.
- Registrar [MP][REQUEST/RESPONSE/SIN_RESPONSE] y la frontera SQL con correlación, duración y JSON legible, usando CajaJsonDiagnostico. No doble serialización para los logs, Replace de barras, cabeceras de autorización ni tokens en salida.

### 5. Integrar grilla y finalización

- T2 abre MP antes del fallback genérico. Incluir el componente y sus scripts versionados en Inicio/Facturación, PDiferido, CobranzaCtaCte y CambioValores, condicionado al catálogo.
- Mostrar pago aprobado bloqueado, ID de pago, Orden MP y ajuste; impedir editar/quitar/cambiar cliente o salir descartándolo. Conservar los demás medios y las NC.
- Soportar varios MP, varios Clover y combinaciones con NC/efectivo sin repetir ni perder ajustes. Deduplicar la incorporación de una respuesta y el disparo de confirmación; el SP conserva su responsabilidad de imputación bancaria.
- Al quedar cubierto el saldo, confirmar por el circuito común una sola vez, sin pregunta y sin reenviar MP_Pay. Respetar emisión, stock y navegación de cada módulo.
- Si Ope_Confirmar/fisco falla, conservar el cobro MP como aprobado y distinguir registración pendiente; reintentar sólo la registración cuando corresponda. No convertir un error fiscal en rechazo/devolución del pago.

### 6. Verificar y desplegar

Pruebas y fixtures privados en .artifacts, sin publicación en Git. Primero simulaciones de SP/transporte y navegador aislado; luego lectura del catálogo real; finalmente recuperación manual y cobro Point coordinados con el responsable/tester. Ningún ensayo real de pago se dispara como parte del análisis o de consultar cuotas.

| Escenario de aceptación | Resultado esperado |
| --- | --- |
| Una cuota y tipos 1/2/3 | Todos válidos; conservar el pos_plan real |
| Varias cuotas y tipo manipulado a Débito/QR | Rechazo antes del SP, también en API |
| Base 9954.12 y 19.17% | Ajuste 1908.20, total 11862.32, rb_rec=19.17 y rb_aux=1908.20 |
| Dos MP o MP + Clover con recargos | Cada rb_aux una sola vez; mismo saldo en ambos órdenes y en Checkout |
| MP + NC + efectivo con vuelto | NC intacta, MP intacto y efectivo neto por regla común |
| CD | Opciones/valores con ajuste bloqueados; operación y navegación propias conservadas |
| ID a1b / ID ab12 | A1B bloqueado con el SP actual; AB12 es consulta manual, no cobro nuevo |
| ID inválido/inexistente/ya imputado | Mensaje del SP; no agregar fila ni confirmar |
| Doble clic o respuesta repetida | Una solicitud desde la UI, una fila y una finalización; no prometer idempotencia durable no contratada |
| Pérdida de respuesta o éxito sin ID utilizable | Verificar/recuperar, sin reintento bancario automático |
| Error fiscal después de aprobación | Cobro preservado y registración pendiente, sin segundo cargo |
| Regresión | Clover, transferencia, efectivo, NC y DOC conservan su funcionamiento; json_p/signos inalterados |

Compilar Caja y los hosts API en salidas aisladas y desplegar sus versiones compatibles. No descartar cobros ya aprobados al actualizar. El rollback debe preservar sus referencias para conciliación; deshabilitar sólo la entrada nueva por catálogo/configuración si se necesita, sin convertirla silenciosamente en transferencia.

## Observaciones concretas al DBA

1. **ID manual de tres caracteres:** el documento dice mínimo 3, pero `If Len(isnull(ltrim(rtrim(@paymentid)),'')) > 3` deja exactamente tres en SP_Orders. Para cumplir el requisito la rama debe considerar también longitud tres; la modificación SQL la decide y realiza el DBA. La aplicación no debe agregar caracteres al ID ni llamar a ese SP como consulta con una longitud que activa cobro.
2. **Salidas de error/timeout:** el sondeo está fuera de una guarda @respuesta=0 y puede continuar después de un fallo de creación. Si las consultas de orden quedan vacías, el control final basado en EXISTS tampoco cubre ese caso expresamente. Revisar esas rutas en su red de SP y asegurar resultado explícito/ID en los éxitos. Son observaciones estáticas, no fallos reproducidos del servicio de MP ni una auditoría de los SP internos.
3. **Diferencias de representación documentadas:** inserción TD/TC/QR frente a salida M00; order_id dividido en dos; rb_importe=@monto frente a pos_importe consultado. Se respetará el SELECT recibido. El tester/DBA contrasta su aceptación al imputar, sin que la aplicación sustituya esos datos o añada una validación de estado bancario.

## Resultado de implementación y pruebas

- Modal Razor propio en las cuatro vistas que consumen pagos: Facturación, Cobranza Diferida, Cuenta Corriente y Cambio/Ingreso de Valores, condicionado a T2 del catálogo. Reutiliza estilo de Caja, modelo neutral de importes y adapter MP separado; no añade otra confirmación para registrar un pago ya aprobado.
- Cadena conectada: MercadoPagoController de Caja → MercadoPagoApiServicio → ApiMercadoPagoController → MercadoPagoServicio → SPGECO_CAJA_MP_Pay. Opciones usan los tres SP de catálogo/opciones existentes. Registro de dependencias común a hosts API; sin validación de estado Point, diario ni retry bancario.
- 59 verificaciones C# privadas: firma/tipos SQL, cero decimal, cuota/plan/tipo, IDs, mapeo financiero y orden completa, respuesta tradicional, rechazos, timeout sin repetición, API cliente JSON/logs, bloqueo de tres en Caja, DI, Clover y combinación MP/Clover/NC/efectivo con vuelto neto.
- 60 verificaciones JavaScript privadas: importes, tipos por cuota real, uppercase/IDs, estados, parciales y completos, errores posteriores, CD, múltiples proveedores, JSON canónico, saldo independiente del orden, deduplicación y finalización única, grilla protegida y CV/IV.
- Navegador Chrome contra host aislado 127.0.0.1:7293: modal Razor compilado y scripts reales con transporte ficticio inyectado sólo en el laboratorio; verificados recargo 1908.20 / total 11862.32, Crédito forzado para tres cuotas, A1B bloqueado, ERROR123 rechazado sin fila, aprobación y pago parcial de 4000. Host detenido al terminar.
- Pruebas, scripts, respaldos, logs y evidencia quedan sólo en .artifacts/mercadopago-20261105 (ignorado por Git). No hay fixture, respuesta simulada ni endpoint de laboratorio en el código de producción.
- Compilaciones aisladas de gc.caja, gc.api, gc.api.report y gc.api.gen sin errores. Se conservan advertencias existentes fuera de este alcance.
- Bitácora local actualizada a 1.54. Publicación en NotebookLM pendiente; no se presenta el cuaderno como actualizado.

## Próximo ensayo operativo

Recompilar y reiniciar Caja y la API con versiones compatibles. Consultar catálogo T2/M00 y opciones reales; si faltan registros, informar al DBA sin sustituir el instrumento. Avisar al responsable antes de intentar un cobro Point para coordinar con el tester; no se realizó ninguno en esta entrega. Recuperación de pago real, reutilización, impresión y confirmación fiscal de MP quedan pendientes de ese circuito. No se modificaron SP ni gc.sitio.
