# GECO Caja — Bitácora única de reglas de negocio consolidadas

**Identificador:** CAJA-BITACORA-UNICA  
**Versión vigente:** 1.1 — 2026-10-05  
**Ámbito:** gc.caja y sus servicios compartidos de Caja.  
**Archivo canónico:** gc.caja/Docs/BitacoraUnicaReglasCaja.md, dentro de D:/Sis25/git/GeConnect/src.  
**Cuaderno de referencia:** Arquitectura de Configuración y Acceso al Sistema de Caja.  
**Destino:** https://notebook.google.com/notebook/00678ebe-1adf-4b1e-b230-21488e92ae71

## 1. Autoridad, alcance y uso

**GOB-01 — Fuente maestra.** Por pedido expreso del responsable funcional, esta bitácora reúne las reglas acordadas y es el primer punto de consulta para resolver contradicciones con fuentes iniciales. Conserva los antecedentes: no elimina los requerimientos originales ni sustituye sus detalles no modificados. “Consolidado” no significa que cada integración haya sido probada en producción.

**GOB-02 — Precedencia.** Aplicar, en este orden: (1) decisiones posteriores explícitas del responsable, incorporadas al historial; (2) reglas confirmadas de esta versión; (3) revisión específica más reciente del módulo, cuando esta bitácora no resuelva el punto; (4) documentos iniciales. Una respuesta generada en un chat, una inferencia o el comportamiento accidental del código no crean una regla de negocio. Si dos reglas de igual autoridad siguen siendo incompatibles, registrar pendiente y solicitar decisión. No extender una excepción de un módulo a todos los demás.

**GOB-03 — Evidencia.** Cada bloque indica su origen:
- **C:** decisión explícita del responsable en la conversación de desarrollo, consolidada al corte de esta versión.
- **F:** requisito documental recuperado del cuaderno, con documento identificado. No implica auditoría de cada línea del código o del SP desplegado.
- **T:** contrato o comportamiento técnico observado en el repositorio; no sustituye una decisión funcional pendiente.
- **P:** pendiente o limitación; no debe presentarse como resuelto.

**GOB-04 — Mantenimiento único.** Actualizar este mismo archivo y su historial ante nuevas decisiones; mantener una sola versión vigente en NotebookLM. La copia cargada allí es una publicación, no una sincronización automática con Git. Tras cada actualización verificar versión y contenido procesado. Si la plataforma exige reemplazar la fuente, verificar la nueva antes de retirar la versión anterior, siguiendo los permisos aplicables. No crear documentos competidores llamados “reglas finales”, “última bitácora”, etc.

**GOB-05 — Responsabilidades.** El responsable funcional decide las reglas. El desarrollo mantiene aplicación, contratos y pruebas. El DBA modifica los SP y resuelve dudas sobre persistencia/SQL; esta consolidación no autoriza cambios de SP. No incluir contraseñas, tokens, certificados privados ni datos reales de clientes en esta bitácora.

## 2. Mapa de módulos y conceptos que no deben confundirse

| Módulo o concepto | Cometido | Regla distintiva |
| --- | --- | --- |
| Facturación | Venta de productos, cálculo y pago | El vuelto es efectivo; tras confirmar permanece en Facturación |
| Diferir pago | Emitir una venta cuyo cobro queda pendiente | No equivale a diferir la factura; luego se cobra mediante CD |
| Cobranza Diferida (CD) | Cancelar facturas pendientes de pago | No vuelve a mover stock |
| Cobranza en Cuenta Corriente (CC) | Imputar cobranza a documentos de cuenta corriente | Informa recibo; no presenta Factura A/B en “Emite” |
| NC imputada como pago | Consumir saldo a favor existente | No emite una NC nueva |
| NC por devolución | Devolver productos de un comprobante de origen | Módulo separado; tiene tratamiento de inventario |
| ND, NC y FS | Ajustes/servicios por conceptos | Cuenta registrada, sin movimiento de productos físicos |
| Anulación de Cobranza | Revertir un recibo seleccionado | No es emisión de factura ni NC por devolución |
| Cambio e Ingreso de Valores | Cambiar instrumentos o ingresarlos | No muestra “Emite” |
| Rendiciones parciales y Cierre | Rendir valores y cerrar la caja | No confundir cierre operativo con Reimpresión Z |
| Reimpresión Z | Solicitar informes Z históricos al controlador compatible | No genera un nuevo cierre operativo |

**GLO-01 — Siglas.** “CF” puede significar consumidor final, controlador fiscal o un código de operación según el campo. Siempre indicar el contexto. No deducir el modo de emisión a partir de co_tipo ni de un nombre de cliente. “NC” emitida y crédito disponible para imputar son procesos diferentes. [C/T]

## 3. Puesto local, ingreso, operador y menú

**PUE-01 — Configuración del puesto.** La aplicación web corre en un servidor; el archivo cajasettings.json pertenece a la estación del cajero. El servidor no debe leer su propio disco para simular la configuración local del navegador remoto. El iniciador local lee el JSON de esa PC, identifica el puesto, entrega el contexto al servidor y abre el navegador. [C; T: README del iniciador]

**PUE-02 — Iniciador básico.** No requiere un servidor HTTP permanente en cada puesto, servicio residente ni esquema de claves públicas/privadas. La identificación inicial del puesto precede al login del cajero; luego se validan usuario, sucursal y caja. El JSON se considera administrado por la organización: este diseño no acredita criptográficamente la identidad física de una PC. [C/T]

**PUE-03 — Contrato actual de arranque.** El ejecutable hace POST de preparación y recibe un pase de un solo uso con vigencia de dos minutos. El navegador consume el pase y llega al login sin conservarlo en la URL final. El contexto protegido del puesto persiste en cookie; cambiar de puesto exige cerrar sesión. Actualizar configuración exige volver a lanzar el iniciador. Nombre de equipo como identificación por defecto; la IP configurada conserva su significado específico. [T]

**PUE-04 — Despliegue.** Publicación autocontenida win-x64 evita instalar .NET en cada PC. Una publicación dependiente requiere el runtime .NET 8 x64, no el SDK. El icono forma parte del iniciador. URL configurada debe apuntar a la aplicación web, incluyendo subaplicación si corresponde. Una respuesta HTML donde se espera JSON exige revisar despliegue/ruta; no cambiar el parser para aceptar HTML. [T]

**PUE-05 — Contexto y apertura.** Autenticación, habilitación general de cajas, apertura del puesto y pertenencia a sucursal son comprobaciones distintas. La apertura general no acredita que el PV del operador esté abierto. Conservar los identificadores de proceso y cierre devueltos por el servidor; no inventarlos desde la vista. Validar disponibilidad del puesto para el usuario. [F: N01; C]

**PUE-06 — Menú con cierre pendiente.** En el estado restringido por cierre pendiente se permiten Cierre del puesto, Cobranza Diferida y Anula Cobranza según sus permisos. Administrador de Caja permanece disponible por sesión autenticada, independientemente de la apertura del puesto (ADM-01). La habilitación de Reimpresión Z tiene su condición específica de controlador; no habilitar el resto de las ventas por esta excepción. [C; F: N01 como antecedente]

**PUE-07 — Cuenta del operador.** Avatar discreto con iniciales y acceso a los datos del operador y cambio de clave, coherente con gc.sitio. Perfil, identidad y permisos no se editan libremente desde Caja. Reutilizar controles y política de seguridad existentes; clave actual/nueva/confirmación, sin recortar espacios ni cambiar mayúsculas. Clave temporal o vencida exige cambio antes de operar. Cambio exitoso invalida la sesión y requiere reingreso; fallo no informa éxito ni reintenta automáticamente. [C/T: CuentaOperador.md]

## 4. Emisión fiscal, recibos y mensajes compartidos

**EMI-01 — Valores de configuración.** En el contrato actual de CajaSettings: facturacion = 1 significa FE; facturacion = 2 significa controlador fiscal. El valor 0 NO está definido como controlador fiscal. tipoCnnCF es otro enum: 0 sin conexión, 1 IP, 2 USB. No mezclar ambos campos. Los requerimientos originales no bastaban para deducir estos números; aquí se documenta el contrato verificado. [T]

**EMI-02 — Resolver presentación.** La información de controlador validada por servidor prima cuando existe: ctrl_id = -1 se presenta como FE, ctrl_id = 50 como CF; otro controlador recibe tratamiento neutral según soporte. Sólo ante ausencia de ese dato se usa el modo local validado como respaldo. No decir FE por defecto ante valores desconocidos. [C/T: PresentacionComprobante.cs]

**EMI-03 — Mensaje de finalización.** Todos los módulos que emiten comprobantes deben adecuar texto y acciones al modo real. FE puede ofrecer su documento digital cuando está disponible. CF no debe anunciar emisión electrónica o PDF inexistente; indicar el resultado conocido y la verificación del comprobante físico cuando corresponda. Un OK lógico no acredita por sí mismo impresión física. [C]

**EMI-04 — Procesos sin factura.** Ocultar “Emite” en Cobranza en Cuenta Corriente, Anulación de Cobranza y Cambio e Ingreso de Valores. No mostrar Factura B heredada del cliente. Una cobranza puede tener recibo aunque no emita una nueva factura fiscal. Mostrar el número de recibo devuelto, nunca fabricarlo. [C]

## 5. Facturación, productos, precios, respaldo y sorteos

**FAC-01 — Identificar cliente.** El check “Confirmar automáticamente” inicia marcado en la búsqueda de cliente de Facturación. Distinguir cliente registrado de consumidor final; mantener origen, cuenta y documento correctos durante toda la operación. Cambiar cliente no autoriza reutilizar sus condiciones de crédito o precios. [C]

**FAC-02 — Lista de precios autorizada.** Cambiar antes de cargar productos. No solicitar autorización para una selección vacía o idéntica a la actual. Sólo una aprobación explícita válida aplica la lista devuelta por el servidor. Durante solicitud pendiente bloquear cambios incompatibles; rechazo, vencimiento o falla no aplican la lista. Un HTTP 409 debe mostrar el motivo devuelto y permitir recuperación controlada. La incidencia original de conectividad no convierte todo 409 en un único diagnóstico. [C/T]

**FAC-03 — Respaldo transparente.** Guardar el detalle en IndexedDB del perfil del navegador del puesto, no en un JSON del servidor. Guardar al agregar, modificar o quitar productos, incluida eliminación del último. Datos mínimos: identificador del producto y cantidad total en unidades. Clave del respaldo: origen web, estación, sucursal, caja y operador; no cliente, para permitir reutilizar los productos con otro cliente. [C/T]

**FAC-04 — Último Detalle.** Recuperar sólo con grilla vacía. Reconsultar cada producto con el cliente y las condiciones actuales: lista autorizada, canal, descuentos y sucursal. No restaurar precios viejos, identidad del cliente anterior ni vínculos de cotización/prefactura. Respetar la cantidad devuelta por el servicio y evitar multiplicarla nuevamente por bulto. [C/T]

**FAC-05 — Vida del respaldo.** Confirmar una venta o volver a identificar cliente no elimina por sí mismo el último detalle. Al comenzar un detalle nuevo, el primer producto sustituye al anterior; las limpiezas propias de otros flujos siguen su contrato. Una falla técnica al recuperar conserva el respaldo y registra diagnóstico. Los rechazos de negocio de productos se informan y no se confunden con éxito completo. [C/T]

**FAC-06 — Límites del almacenamiento.** Es local al navegador y origen: otro puerto, perfil, equipo o limpieza de datos puede impedir recuperarlo. Guardado transaccional y control de revisión evitan sobrescrituras silenciosas entre pestañas. Advertir si queda escritura pendiente; no prometer conservación de un último cambio aún no confirmado por el almacenamiento ante corte eléctrico. No equivale a respaldo central. [T]

**FAC-07 — Sorteos de Facturación.** Leer so_sorteo como identificador conservando ceros y so_desc como descripción. Ejemplo ficticio: [{"so_sorteo":"0060","so_desc":"Sorteo de ejemplo"}]. Mostrar los sorteos devueltos en cálculo y vista de pago, sin “Sorteo sin nombre” por buscar otra propiedad. Sin sorteos, ocultar/limpiar el bloque para no arrastrar los de otra venta. Conservar el JSON contractual hacia confirmación. No extender esta presentación a NC por devolución, donde la fuente dispone descartar sorteos. [C; F: N02–N06, N11]


**FAC-08 — Captura y cantidades.** Cargar/validar artículos por el servicio de productos, desde código, escáner, búsqueda o cantidad+código. Distinguir productos de balanza y unidades de presentación. Convertir bultos/presentaciones una sola vez: una cantidad ya normalizada al recuperar detalle no se multiplica nuevamente. Filas con identificadores únicos y respaldo completo de todos los ítems. [F: N02, N07; C/T: recuperación actual]

**FAC-09 — Sumarización.** suma_producto = false conserva filas separadas; true permite sumarizar ítems compatibles. Las revisiones excluyen cotizaciones, prefacturas y productos pesables/balanza de esa sumarización. No fusionar filas sólo por p_id si el origen o las condiciones impiden hacerlo. [F: N03/N06]

**FAC-10 — Cotizaciones y prefacturas.** Cotización: seleccionar una, con grilla vacía; su carga bloquea agregar otros productos conforme al flujo documentado. Prefacturas: permitir seleccionar una o varias disponibles. Mantener sus referencias y condiciones contractuales; no tratarlas como un Último Detalle sin origen. [F: N02/N03/N06]

**FAC-11 — Factura emitida.** La revisión incorpora recuperar productos de un comprobante ya emitido mediante B_Producto_Datos, tipo V y referencia tco_id + cm_compte + cm_repetido. Recuperar un detalle para otra operación no equivale a cobrar nuevamente el comprobante original ni a reimprimirlo. [F: N17]

**FAC-12 — Diferir factura.** Genera una prefactura mediante SPGECO_CAJA_Prefacturas_Carga. No es una venta confirmada, no emite comprobante fiscal, no genera el asiento de caja de una venta ni descuenta stock. [F: N02]

**FAC-13 — Diferir pago.** co_tipo = DP emite la venta con pago pendiente; sí registra la operación y afecta stock. La fuente indica json_valores, json_cancela y json_union vacíos ("{}") para ese paso. Luego CD cobra lo pendiente sin descontar inventario otra vez. No usar los términos diferir pago y diferir factura como sinónimos. [F: N02/N05; C]

**FAC-14 — Venta y validación fiscal.** Facturación de contado usa CR para cliente registrado y CF para consumidor final. La revisión N05 prescribe validación de PV antes de confirmar (tipo_llamada F), luego Ope_Confirmar, stock cuando corresponda y presentación del comprobante. El resultado de Valida_PV se interpreta según controlador: 0 permite continuar; para FE, 1 es el camino CAEA previsto por el servicio, no un permiso universal para ignorar errores; para Hasar 2G, 1 indica comprobante abierto cancelado según esa fuente. Respuestas de bloqueo impiden confirmar. No ejecutar esta secuencia de venta al entrar a Reimpresión Z. [F: N05]

**FAC-15 — Autorizaciones.** El cambio de lista requiere supervisor. Las revisiones exigen autorización para eliminar ítems salvo su excepción documentada de presupuesto. Mantener las autorizaciones aplicables aunque se unifique la interfaz. Ningún permiso se deduce de ocultar un botón o de un resultado HTTP exitoso sin aprobación funcional. [F: N02/N03/N06; C]

## 6. Motor de pago: importes, vuelto y excedentes

**PAG-01 — Catálogo y autoridad.** Habilitar instrumentos según catálogo devuelto para operación, cliente y caja; no sólo según botones visibles. El servidor debe validar categoría e identificador, importes, NC, tope y plazos. Un instrumento desconocido no se acepta por una categoría manipulada en el navegador. [C/T]

**PAG-02 — Magnitudes.** Total a cancelar = importe final exigible de la operación, con los ajustes que correspondan. Total aplicado = suma de medios ingresados + NC imputadas, sin contar una NC dos veces. Diferencia = total a cancelar menos total aplicado. Mostrar moneda con $, dos decimales y cómputos coherentes en centavos; no rechazar igualdad por residuos de coma flotante. [C]

**PAG-03 — Vuelto.** Diferencia negativa representa vuelto sólo si existe efectivo suficiente para cubrir el excedente. No debe aparecer el aviso genérico “Monto elevado” para un efectivo válido que genera vuelto. Pantalla conserva efectivo recibido y vuelto; SP de confirmación recibe efectivo NETO que queda en caja. No enviar el vuelto como otra fila negativa ni descontarlo de cheques, documentos o NC. [C explícita]

**PAG-04 — Ejemplo normativo.** Total $85.000, efectivo recibido $90.000: diferencia -$5.000, vuelto $5.000, suma de rb_importe de efectivo enviada $85.000. Con total $85.000, NC $30.000 y efectivo $60.000: enviar NC $30.000 y efectivo neto $55.000, vuelto $5.000. La NC mantiene su importe imputado. [C]

**PAG-05 — Excepción de cheques.** En CD y CC solamente, se admite excedente cuando TODOS los valores son cheques y no hay NC imputadas, documentos ni otro medio. Deben cumplir tope y fechas. Se envía el valor completo de los cheques; no reducirlos como si fueran vuelto. Esta excepción no aplica a Facturación. [C explícita]

| Operación | Pago exacto | Exceso cubierto por efectivo | Exceso exclusivamente con cheques | Otro exceso |
| --- | --- | --- | --- | --- |
| Facturación | Admitir si pasa controles | Admitir como vuelto; efectivo neto al SP | Rechazar | Rechazar |
| Cobranza Diferida | Admitir si pasa controles | Admitir como vuelto; efectivo neto al SP | Admitir dentro de tope, sin NC/otros medios | Rechazar |
| Cobranza CtaCte | Admitir si pasa controles | Admitir como vuelto; efectivo neto al SP | Admitir dentro de tope, sin NC/otros medios | Rechazar |

**PAG-06 — Combinaciones.** Cheques con otros instrumentos no habilitan excedente de cheques. Si la combinación incluye efectivo, sólo se admite el excedente cubierto por ese efectivo. Documento $80 + efectivo $30 para total $100: documento $80, efectivo neto $20, vuelto $10, sujeto a crédito/plazo. Documento solo $110 para total $100: rechazar. Cheque + NC con exceso y sin efectivo: rechazar. [C/T: aplicación del criterio confirmado de vuelto]

**PAG-07 — Falta de pago.** Diferencia positiva no permite finalizar como pago completo. Diferir es una operación explícita diferente, no un bypass de validaciones. Una diferencia cero permite finalizar si pasan las demás reglas y debe mantener disponible el acceso a las NC. [C/F]

## 7. Cheques, documentos, crédito y vencimientos

**CRE-01 — Cliente registrado.** Cheques y documentos requieren cliente registrado con origen C y cta_id válido. Consumidor final puede pagar por instrumentos habilitados que no requieran cuenta; no habilitar cheque o DOC sólo porque exista un documento de identidad. [C/T]

**CRE-02 — Tope.** Controlar ctac_tope_credito. El motor actual valida la suma de cheques y documentos de la operación contra ese tope, sin duplicar el cupo por instrumento. No usar un tope por cada fila. Si se pretendiera separar cupos o descontar deuda previa adicionalmente al valor devuelto, requiere aclaración del contrato y del DBA: no inferirla. [C: exigencia de tope; T: agregación compartida]

**CRE-03 — Plazos.** El JSON fp del cliente define fp_id = H para cheque y D para documento. fp_dias tiene mínimo válido 1. Vencimiento permitido desde hoy hasta hoy + fp_dias, inclusive, para la forma correspondiente. DOC inicia con hoy y no puede ser anterior. Un plazo de 1 permite hoy o mañana; no significa que hoy esté prohibido. [C]

**CRE-04 — Información insuficiente.** No inventar un plazo o cupo permisivo si faltan condiciones válidas. El motor actual rechaza tope no positivo, configuración inválida o plazo menor que 1. Si hay varias entradas del mismo tipo, utiliza el menor plazo válido y rechaza si alguna es inválida; es un criterio técnico conservador que debe revisarse con el contrato si aparecen duplicados legítimos. [T; P: semántica de duplicados]

**DOC-01 — Interfaz.** Documento en Cuenta Corriente se carga mediante modal coherente con Efectivo, con monto sugerido igual al remanente y fecha de vencimiento editable dentro de sus límites. No usar prompt nativo de JavaScript. [C]

**DOC-02 — Contrato vigente del documento simulado.** Categoría tcf_id = DO, instrumento ins_id = DOC. En CD, co_tipo = CD. json_valores:
- ins_id: "DOC".
- rb_importe: monto acordado a documentar.
- rb_fecha_valor: fecha de vencimiento elegida, NO 0.
- rb_rec: 0.
- rb_opcion_cuota: 1.
- rb_cupon_manual, rb_ch_dif, rb_estado: "N".
- rb_aux: 0.
- rb_dato1_valor, rb_dato2_valor, rb_dato3_valor, id_externo: "".

La aclaración técnica y la confirmación posterior del responsable prevalecen sobre el texto antiguo que indicaba rb_fecha_valor = 0. [C; F: N19]

## 8. Créditos / NC y débitos de cuenta corriente

**NC-01 — Tres importes diferentes.** cv_importe_ori es el importe histórico con el que nació el documento. cv_importe es el saldo realmente disponible al consultar. El importe imputado es lo que se decide aplicar en esta operación. Conservar una copia inmutable del saldo disponible inicial para editar, quitar y restaurar; nunca restaurar desde cv_importe_ori. Aplica tanto a débitos seleccionados para cobrar como a créditos utilizados en pagos. [C]

**NC-02 — Presentación.** En la grilla informar, con detalle discreto, importe original, disponible real, imputado y remanente. No rotular como “disponible” el monto de origen. Ejemplo: original $100.000, disponible $30.000, imputado $10.000, remanente $20.000; retirar la imputación restaura $30.000, no $100.000. [C]

**NC-03 — NC opcionales.** Cada fila permite Modificar y Quitar, con lápiz verde y flecha de retorno roja como en Cobranza CtaCte. El selector común muestra disponibles, imputados y remanentes y permite recuperar los retirados. No obligar a administrar todas las NC para cambiar una sola. [C]

**NC-04 — NC obligatorias.** No permitir retirar las obligatorias ni eludir su aplicación mediante modificaciones. Conservar las autorizaciones aplicables y validar nuevamente en servidor. No suponer que toda NC devuelta es obligatoria. [C]

**NC-05 — Recalcular juntos.** Toda modificación actualiza grilla, total de NC, otros valores, diferencia y botones. Quitar efectivo elimina sólo ese pago: no borra NC ni deja un resumen que contradiga la grilla. Acceso a los créditos permanece disponible aunque cubran todo y la diferencia sea cero. [C]

**NC-06 — Respetar decisiones.** Una NC opcional retirada no se vuelve a aplicar automáticamente al abrir otro medio de pago dentro de la misma operación. Volver entre pantallas de esa operación conserva la selección. Volver a identificar cliente inicia una operación nueva, incluso si se elige la misma cuenta: limpiar imputaciones transitorias y consultar otra vez los saldos disponibles. [C]

**NC-07 — Validación final.** Impedir duplicados, importes negativos o superiores al saldo y retiro de obligatorias. Recuperar desde servidor las referencias y saldos autoritativos, sin aceptar ciegamente los del navegador. No fijar como regla saldos observados de una cuenta de pruebas: pueden cambiar. [C/T]

**NC-08 — Alcance compartido.** Aplicar la gestión a todos los módulos que imputan créditos y cuyo catálogo los habilita. No añadir esta gestión de pagos a un módulo sólo porque emite una NC. Los débitos de Cobranza CtaCte respetan la misma diferencia entre original, saldo disponible e importe seleccionado. [C]

**NC-09 — Autorización para consumidor final.** Si el catálogo y el flujo permiten imputar créditos a consumidor final, la fuente exige autorización del administrador de cajas. Conservar ese control; no confundir una NC autorizada con permitir cheques o documentos sin cuenta registrada. [F: N02; C: conservar autorizaciones]

## 9. Cobranza Diferida

**CD-01 — Identidad y pendientes.** Cobrar los comprobantes pendientes del cliente elegido. Un consumidor final con factura diferida debe poder recuperarla mediante su documento cuando no tiene cta_id; no confundirlo con otra cuenta registrada que comparta documento. Mantener origen y documento normalizados en la consulta. [C/T]

**CD-02 — Sin pendientes.** Una lista vacía válida es un resultado normal, también cuando se entra desde el menú restringido por cierre pendiente. Informar sin pendientes y permitir retorno; no intentar abrir una selección vacía ni confirmar. Un error técnico o respuesta inválida no debe disfrazarse de lista vacía. Limpiar resultados anteriores antes de consultar otro cliente. [C/T]

**CD-03 — Confirmación.** co_tipo = CD. En SPGECO_CAJA_Ope_Confirmar, json_p, json_subtotal y json_sorteos se envían como "{}": no se están vendiendo artículos nuevamente. No ejecutar SPGECO_STK_Carga, porque la salida de mercadería ocurrió al emitir la factura original con pago diferido. [C; F: N04]

**CD-04 — Fechas de imputación.** En json_cancela, cv_fecha_carga se completa con cv_fecha_vto de los pendientes seleccionados. En json_union de créditos, cv_fecha_carga corresponde a cv_fecha_carga de SPGECO_CAJA_Valores_NC. No intercambiar ambos mapeos. [C explícita]

**CD-05 — Sin recargos financieros.** No permitir medios o instrumentos que generen recargos en CD. DOC se documenta según DOC-02. Los demás instrumentos siguen catálogo, tope, plazos, vuelto y excepciones comunes. [C/F]

**CD-06 — Salida.** Tras confirmación exitosa mostrar recibo y consultar pendientes actuales del cliente: si quedan, permanecer en Cobranza Diferida; si no queda ninguno, volver al menú. Si falla esa consulta, no concluir falsamente que ya no quedan: mantener una salida recuperable e informar el problema. [C explícita; T]

## 10. Cobranza en Cuenta Corriente

**CC-01 — Selección.** Consultar documentos de la cuenta registrada y permitir seleccionar importes según saldo disponible. Tipo de comprobante debe mostrar descripción, no código; retirar columnas Cliente y Tipo Documento de la grilla solicitada. Aplicar NC-01 a restauraciones. [C; F: N08]

**CC-02 — Pago.** Aplicar matriz PAG-05, tope, plazos, restricción de consumidor final y gestión de créditos. La suma de NC y valores debe coincidir con lo exigible salvo las excepciones de vuelto o cheques puros. [C]

**CC-03 — Resultado.** Ocultar “Emite”; al confirmar presentar número de recibo con estilo común de confirmación, no una leyenda genérica ni una nueva factura. Después volver al menú principal. [C]


**CC-04 — Contrato y stock.** co_tipo = CC. En json_cancela, cv_fecha_carga toma cv_fecha_carga de SPGECO_CAJA_B_CtaCte, no el vencimiento usado en CD. En json_union toma cv_fecha_carga de SPGECO_CAJA_Valores_NC. No vuelve a actualizar stock. La selección parcial se limita al saldo disponible actual, aunque una fuente anterior lo llame ambiguamente “importe original”. [F: N08/N17; C: NC-01]

## 11. NC por devolución

**DEV-01 — Origen y validación.** Identificar comprobante original con tipo, número y repetido cuando corresponda; validar mediante SPGECO_CAJA_NC_Valida con contexto de proceso/cierre conforme revisión vigente. No confundir selección de comprobante repetido con creación duplicada de NC. [F: N09–N11]

**DEV-02 — Banderas.** nc_sin_detalle = 1 impide emitir; nc_fecha_supero_dias = 1 impide por plazo. nc_ya_emitida = 1 advierte, pero por sí solo no bloquea devoluciones parciales sucesivas permitidas. Respetar cantidades y validaciones del servicio. [F: N10, N11, N17]

**DEV-03 — Destino.** Preguntar si se registra en CtaCte únicamente cuando nc_ctacte = 1, nc_dv_dist = 0 y nc_dv_pago_diferido = 0. Si nc_dv_dist = 1 o nc_dv_pago_diferido = 1, forzar DV sin esa pregunta. Los tipos documentados son AA para devolución de dinero y DV para registro en CtaCte. [F: N10/N11]

**DEV-04 — Productos y stock.** La revisión vigente de NC_B_Producto distingue respuesta 0 (cargar), positiva (cargar con aviso) y negativa (no cargar e informar). Se permite carga parcial o total según contrato. NC por devolución sí tiene actualización de inventario después de confirmar; la fuente especifica SPGECO_STK_Carga tipo FV. No trasladar a este módulo la prohibición de stock de ND/NC/FS o CD. Sorteos del cálculo se descartan en este flujo. [F: N11; contrato exacto desplegado a cargo del DBA]

**DEV-05 — Emisión.** Mensajes y acciones deben reflejar FE o controlador real según EMI-01 a EMI-03. No afirmar FE cuando el puesto usa controlador. [C]

## 12. Nota de Débito, Nota de Crédito y Factura de Servicio

**NDS-01 — Cuenta habilitada.** Exigir cuenta registrada con cta_id válido. El comportamiento actual permite NC y FS a cliente origen C; ND, NC y FS a proveedor origen P. Rechaza orígenes F/N/Q. Que una búsqueda general devuelva una fila no habilita operar con ella. No reutilizar la enumeración histórica de orígenes como permiso general. [C: exigir cuenta registrada; T: matriz vigente]

**NDS-02 — Conceptos.** Capturar concepto, neto, IVA y cantidad; calcular mediante el motor compartido según contrato de conceptos. NC requiere comprobante de origen; ND/FS no arrastran esa referencia. Impacta cuenta corriente y no se ejecuta SPGECO_STK_Carga. [F: N12; C/T]

**NDS-03 — Operación con teclado.** Enter avanza Neto → IVA → Cantidad → Concepto → Agregar. En Cantidad y Concepto seleccionar el texto existente al recibir foco para reemplazarlo cómodamente. Validar antes de agregar. Mostrar importes con $, no ARS. Evitar efectos de selección sobre cuentas no habilitadas. [C]

**NDS-04 — Emisión.** Usar el modo real del puesto para mensajes; no reutilizar un éxito de FE indiscriminadamente. La frase antigua relativa sólo a impresión FE no justifica presentarse como FE en un controlador. [C]

## 13. Anulación, cambio de valores, rendiciones y cierre

**ANU-01 — Anulación de Cobranza.** Cliente registrado; búsqueda de recibos por fecha inicial hoy, editable. Elegir un único recibo, confirmar con texto claro, signos de interrogación correctos, número e importe. Ocultar Emite. Bloquear la vista mientras confirma y después del éxito para impedir repetir por doble clic. Éxito retorna al menú. La fuente asigna el ajuste a la caja del recibo original; no afecta stock. Referencias: SPGECO_CAJA_B_Cobranzas y SPGECO_Opc_Anula_Cobranza. [C; F: N14]

**VAL-01 — Cambio e Ingreso de Valores.** Ocultar Emite. Mantener catálogo y autorizaciones del módulo, cuenta registrada y datos de instrumentos. Fuente: consulta Valores_MP para CV y confirmación Ope_Cv_IV con tipo CV/IV, json_valores y contexto/usuario autorizante. Cambio de valores y mero ingreso son operaciones distintas; no inventar una devolución de efectivo en un ingreso. No afecta stock. [C; F: N16]

**REN-01 — Rendición parcial.** Requiere caja abierta. Seleccionar instrumentos, cargar cantidades de denominaciones y calcular subtotales/total sin editar libremente denominaciones. Fuentes: Rend_Ins tipo P, REND_Ins_Nominaciones y Rend_Carga con json_rendiciones. No afecta stock ni equivale a cerrar caja. [F: N13]

**CIE-01 — Cierre individual del puesto.** Verificar pendientes de la caja/cierre correspondiente; si existen facturas diferidas que impiden cerrar, mostrarlas y no confirmar el cierre. Rendición final por instrumentos/denominaciones, catálogo tipo F y confirmación mediante Apertura_Cierre según contrato. No mueve productos. Tras cierre exitoso cerrar la sesión operativa y volver al acceso. No cambiar estas reglas por las de Reimpresión Z. [F: N15; T: documentación de Z]

## 14. Reimpresión Z

**Z-01 — Habilitación.** Módulo independiente, sólo para el controlador actualmente soportado ctrl_id = 50 y caja de la sucursal válida. Botón desactivado si no corresponde. No exige abrir la caja para reimprimir históricos; puede estar disponible en estados de acceso completo, parcial o cierre restringido. No desactivar por esta regla el botón de cierre operativo general. [C/T: ReimpresionZ.md]

**Z-02 — Rango por número.** Desde y Hasta enteros, mínimo 1, máximo técnico actual 99999; Desde <= Hasta y Hasta - Desde <= 5. Desde 5000 hasta 5005 es válido e incluye seis números. No reinterpretar como “máximo cinco cierres”. El cierre 0 es inválido. [C explícita; T: máximo técnico]

**Z-03 — Rango por fecha.** Desde <= Hasta, diferencia <= 7 días. Hasta no posterior a hoy ni anterior a hoy menos cinco años; la restricción de cinco años aplica a Hasta en el contrato actual. Fechas ISO en vista y yyMMdd hacia el SP según integración actual. Los extremos se tratan como rango; ejemplo 01/10 a 08/10 tiene diferencia de siete días. [C: intervalo; F/T: antigüedad/formato]

**Z-04 — Contrato recibido del DBA/responsable.** SPGECO_CAJA_Z recibe @caja_id varchar(4), @usu_id varchar(10), @adm_id varchar(10), @xfecha bit, @desde varchar(10), @hasta varchar(10). @xfecha = 1 por fecha, 0 por número. Devuelve SELECT resultado, resultado_msj. Usa SP_ReportarZetasPorFecha o SP_ReportarZetasPorNumeroZeta del servicio fiscal. No reducir la llamada a los tres parámetros de una descripción inicial. [C: SP suministrado]

**Z-05 — Seguridad operativa.** Consultar datos de controlador sin ejecutar Valida_PV para esta pantalla, pues puede tener efectos sobre documentos fiscales abiertos. Bloquear solicitudes simultáneas/doble envío; sin reintento automático tras resultado incierto. Reimpresión no modifica stock, no abre caja y no crea una venta. [T]

**Z-06 — Alcance de OK y pendientes DBA.** El SP entregado puede devolver 0/OK sin comprobar impresión física: sólo ejecuta el bloque fiscal para ctrl_id 50 y no analiza impresora/Fiscal de @h2g_z; el control de respuesta vacía está comentado. @ambiente se consulta pero no se usa. Revisar caja_ip vacía, errores del servicio, formato de fechas, documento fiscal abierto y validación de rangos en SQL. La aplicación limita entradas; no se ha modificado ese SP. [C/T/P]

## 15. Confirmación, navegación y experiencia común

**UX-01 — Guardado.** Bloquear acciones incompatibles mientras la confirmación está en curso y no liberar la pantalla por un temporizador más corto que la solicitud activa. Tras éxito, impedir confirmar otra vez el mismo estado. Manejar errores explícitos sin presentar éxito. [C/T]

**UX-02 — Navegación posterior.** Facturación permanece en su flujo, sin regreso automático ni atajos de éxito al menú principal. CD permanece si hay pendientes del cliente; vuelve al menú si no quedan. CC y Anulación regresan al menú tras éxito. El cierre individual del puesto termina sesión. El cierre general de la sucursal pertenece a Administración (ADM-07). No aplicar una salida única a todos los módulos. [C/F]

**UX-03 — Navegador.** Una página web no puede impedir de manera absoluta editar URL, recargar o salir del navegador. La protección de la vista y el aviso de salida durante guardado ayudan, pero no son idempotencia persistente del servidor. Pantalla completa/kiosco requiere definición aparte; no se considera implementada por esta bitácora. [T/P]

**UX-04 — Diseño y errores.** Mantener estilo Golden de Caja, modales comunes y controles accesibles por teclado. Mostrar mensajes útiles y contexto para diagnóstico, sin credenciales. Un modal vacío o resumen incoherente no es una respuesta funcional aceptable. [C]

## 16. Desempates explícitos frente a antecedentes

| Tema ambiguo o anterior | Criterio que prevalece | Referencia |
| --- | --- | --- |
| “facturacion 0 es controlador” | 1 FE / 2 CF; 0 no definido como CF | EMI-01 |
| Documento sin fecha / rb_fecha_valor = 0 | Vencimiento elegido entre hoy y límite del cliente | DOC-02, CRE-03 |
| Restaurar crédito al importe original | Restaurar saldo disponible copiado de cv_importe | NC-01 |
| Toda diferencia negativa impide pagar | Admitir vuelto cubierto por efectivo; enviar neto | PAG-03 |
| Cheques mezclados permiten excedente | Sólo cheques puros en CD/CC, sin NC ni otros medios; efectivo tiene su propia regla | PAG-05/06 |
| Facturación también admite exceso de cheques puros | No; excepción exclusiva de CD/CC | PAG-05 |
| Crédito retirado se vuelve a cargar al agregar pago | No dentro de la misma operación | NC-06 |
| Reidentificar la misma cuenta conserva NC editadas | Nueva operación, nueva consulta de saldos | NC-06 |
| Consumidor final nunca puede cobrar su diferido | Sí puede, con identidad/documento correctos y medios permitidos | CD-01, CRE-01 |
| Todos los procesos emiten Factura A/B | Cobranza informa recibo; ocultar Emite donde no aplica | EMI-04 |
| Todos los éxitos vuelven al menú | Salida específica por módulo | UX-02 |
| Configuración y detalle local se leen en servidor | Configuración mediante iniciador; detalle en IndexedDB del puesto | PUE-01, FAC-03 |
| Menú restringido sólo cierre | También CD y Anula Cobranza; Z según compatibilidad; Administración independiente de apertura | PUE-06, ADM-01 |
| Reimpresión: máximo cinco cierres | Diferencia numérica máxima cinco, seis extremos inclusivos | Z-02 |
| Reimpresión: OK garantiza impresión | OK no acredita papel ni respuesta fiscal válida | Z-06 |
| Toda NC está prohibida si ya existe otra | NC previa advierte; puede haber devolución parcial posterior válida | DEV-02 |
| Orígenes devueltos por búsqueda habilitan ND/NC/FS | Cuenta registrada y matriz C/P vigente | NDS-01 |

## 17. Casos de aceptación que deben conservarse

Son escenarios normativos para pruebas; esta publicación documental no afirma haber ejecutado transacciones reales.

| Caso | Resultado esperado |
| --- | --- |
| Total 85.000, efectivo 90.000 | Vuelto 5.000; efectivo SP 85.000 |
| Total 85.000, NC 30.000, efectivo 60.000 | Vuelto 5.000; NC 30.000 + efectivo SP 55.000 |
| Facturación total 100, cheque 110 | Rechazar excedente, aunque tenga tope |
| CD/CC total 100, sólo cheque 110, tope 120 y plazo válido | Admitir 110 completo; no tratar 10 como vuelto |
| CD/CC total 100, cheque 100 + NC 10 | Rechazar; no son cheques puros ni hay efectivo |
| Total 100, cheque 80 + efectivo 30, tope/plazo válidos | Vuelto 10; cheque 80 + efectivo SP 20 |
| Total 100, DOC 80 + efectivo 30, tope/plazo válidos | Vuelto 10; DOC 80 + efectivo SP 20 |
| Tope 100, cheque 60 + DOC 50 | Rechazar suma 110 |
| Consumidor final intenta cheque/DOC | Rechazar; otros medios según catálogo |
| fp_dias 1, vencimiento hoy/mañana/pasado mañana | Admitir hoy y mañana; rechazar pasado mañana |
| DOC fecha ayer | Rechazar |
| Original NC 100.000, disponible 30.000, imputar/quitar 10.000 | Disponible restaurado 30.000 |
| NC retirada y abrir modal Efectivo | No vuelve sola |
| Volver a identificar misma cuenta | No persiste una imputación de la operación anterior |
| Eliminar efectivo con NC aplicada | NC y resumen permanecen coherentes |
| CD sin pendientes: respuesta [] | Estado vacío normal, sin excepción |
| CD error de consulta / respuesta inválida | Informar error; no decir “sin pendientes” |
| Confirmar CD con/sin pendientes posteriores | Permanecer / volver al menú |
| Emisión en controlador fiscal | No anunciar FE ni ofrecer PDF inexistente |
| Z números 5000..5005 / 5000..5006 / 0..1 | Admitir / rechazar / rechazar |
| Sorteo con so_desc | Mostrar su nombre en cálculo/pago |
| Último Detalle con nuevo cliente | Mismos productos/cantidades, precios y condiciones consultados nuevamente |

## 18. Pendientes y límites que no deben ocultarse

**PEN-01 — DBA/fiscal.** Quedan las preguntas de Z detalladas en Z-06; resultado OK no prueba salida física. El diagnóstico de XML/DTD comunicado en desarrollo no autoriza habilitar DTD indiscriminadamente ni modificar SP por esta documentación.

**PEN-02 — Garantía de una sola operación.** Bloqueos de interfaz y exclusión temporal no acreditan idempotencia duradera entre instancias, recargas, reinicios o respuesta perdida después de confirmar. Requiere contrato transaccional explícito antes de prometer ausencia total de duplicados o reintentar automáticamente.

**PEN-03 — Crédito.** La agregación cheques + documentos y el tratamiento conservador de fp duplicado son la implementación actual documentada. Cualquier cambio de cupo agregado a cupos separados o de interpretación de deuda previa exige decisión funcional y contrato del servicio.

**PEN-04 — Verificación.** Las suites locales son evidencia técnica parcial. No equiparar pruebas con servicios simulados a confirmaciones reales contra SP, impresión fiscal o recuperación ante corte eléctrico. La presente tarea consolida y publica documentación; no cambia reglas ejecutables ni ejecuta cobros.

**PEN-05 — Cobertura.** Las fuentes de QR/AFIP y los contratos completos de cada SP siguen siendo antecedentes técnicos. No se inventan aquí especificaciones fiscales ausentes ni requisitos legales nuevos. Ante un detalle no consolidado, consultar la revisión específica y citar el punto exacto.

## 18 bis. Administrador de Caja — gestión distribuida de sucursal

**ADM-01 — Acceso sin apertura.** Administrador de Caja debe estar disponible con sesión autenticada y sucursal de login, aunque no exista apertura general ni apertura del puesto, o haya un cierre individual pendiente. Entrar a Administración no abre el puesto, no ejecuta Valida_PV ni exige estado operativo fiscal. La apertura general y el cierre general pasan a Caja por su operación distribuida por sucursal. No dependen de que sigan existiendo los ítems del menú central. [C: aclaración del responsable/analista, 2026-10-05]

**ADM-02 — Primera etapa y separación.** Implementar Apertura General de Cajas y Cierre General de Cajas dentro de Administrador. La habilitación general no sustituye la apertura individual; el cierre general no sustituye rendición ni cierre individual, ni es Reimpresión Z. En el inicio se permite elegir Operar Caja o Administrador antes de iniciar el circuito operativo del puesto. Se conserva el acceso Administrador en el menú operativo. [C/F: N22; T: integración Caja]

**ADM-03 — Contexto de sucursal.** Usuario y administración se obtienen de la sesión autenticada en el servidor. No aceptar estos identificadores desde un formulario ni utilizar una sucursal elegida en el navegador. Las consultas y operaciones se envían a la API configurada de esa instalación. No recorrer ni coordinar automáticamente otras sucursales. [C/F: N22]

**ADM-04 — Habilitación general.** Solicitar confirmación explícita indicando sucursal. Invocar SPGECO_CAJA_Grl_Habilitacion mediante el endpoint existente HabilitarCajaGral con usu_id y adm_id. Si el resultado explícito es cero, informar habilitación correcta y número de proceso de cobro devuelto. El documento llama a los retornos respuesta/ respuesta_msj/ respuesta_id; la API implementada los expone como resultado/ resultado_msj/ resultado_id. No inventar un número ni informar éxito si falta el contrato; un éxito sin proceso requiere verificar el estado antes de repetir. [F: N22; T: contrato API existente]

**ADM-05 — Cierre general.** Mostrar grilla de puestos abiertos de la sucursal de login mediante SPGECO_Caja_PV_Abiertos / ObtenerPVAbiertos: número de cierre, punto de venta y cajero. Si hay al menos un puesto abierto, deshabilitar Cierre General. Permitir actualizar. Reconsultar desde el servidor al confirmar antes de invocar SPGECO_CAJA_Grl_Cierre / CierreCajaGral (usu_id, adm_id). Si la consulta falla o es inválida, bloquear el cierre; una falla no equivale a lista vacía. Mostrar resultado de cierre o mensaje de rechazo. [F: N22; T: refuerzo en Caja]

**ADM-06 — Confirmación y resultados.** POST con antiforgery; bloquear controles durante confirmación/envío. Impedir solicitudes simultáneas de gestión general de una misma sucursal dentro de la instancia y recordar 24 horas el resultado exitoso/incierto del mismo identificador de solicitud. No reintentar automáticamente después de timeout, desconexión o respuesta inválida. Mostrar resultado incierto y pedir verificación operativa antes de repetir. La exclusión y memoria de solicitudes son locales al proceso web; no garantizan idempotencia persistente entre reinicios ni coordinación entre servidores. [T]

**ADM-07 — Salida.** El resultado se muestra en Administración. Después de éxito o resultado incierto, ambas acciones quedan bloqueadas en esa pantalla para evitar repetición; se conserva consulta y salida al inicio. No abrir un PV ni cerrar la sesión automáticamente por éxito de gestión general. Esto no cambia CIE-01. [T: decisión de integración; no requisito añadido al SP]

**ADM-08 — Alcance futuro.** Gestión POS Clover; Anulación de Operaciones Clover; Anulación de Operaciones MP; CAEA Consultar; CAEA Solicitar; CAEA Pasar Novedades. No quedan implementadas ni activas en esta etapa. Debe resolverse individualmente si se distribuyen o centralizan; no extender automáticamente a ellas las decisiones de las primeras dos opciones. [C/P]

**ADM-09 — Migración del Central.** No modificar gc.sitio, su implementación original, SP ni registros de menú central en esta etapa. Posteriormente retirar desde los registros de ítems de menú vta_cajas_grl_hab y vta_cajas_grl_cierr, según la decisión y despliegue coordinado del responsable. Preparar observaciones al dueño del original; no se han enviado. [C/F: N22]

**ADM-10 — Validación y límite pendiente.** Se verificó compilación de Caja, 31 casos aislados de servicio/controlador y pantalla real con API simulada: consulta fallida, puestos abiertos, confirmación y número de proceso. No se ejecutó habilitación/cierre real ni se auditó el SQL desplegado. DBA debe confirmar que el SP de cierre valida de forma atómica ausencia de puestos abiertos y estado de la sucursal, incluso ante aperturas concurrentes y múltiples instancias. La reconsulta de Caja reduce el riesgo pero no reemplaza esa garantía SQL. [T/P]

## 19. Fuentes y trazabilidad

### Fuentes del cuaderno existentes al consolidar

| ID | Título exacto |
| --- | --- |
| N01 | 20260330 Requerimientos de logueo y Vista Principal de Caja v04.docx |
| N02 | 20260513 Facturación y PAGO.docx |
| N03 | 20260513 Revisión Facturación y PAGO.docx |
| N04 | 20260528 Cobro de Facturas Pendientes.docx |
| N05 | 20260601 Revisión Facturación y PAGO 2.docx |
| N06 | 20260601 Revisión Facturación y PAGO.docx |
| N07 | 20260608 Revisión de Cajas 2026.docx |
| N08 | 20260625 Cobranza de CtaCte.docx |
| N09 | 20260629 NC por Devolución.docx |
| N10 | 20260702 NC por Devolución.docx |
| N11 | 20260703 NC por Devolución.docx |
| N12 | 20260714 ND NC y FS.docx |
| N13 | 20260731 Rendiciones Parciales de Caja.docx |
| N14 | 20260803 Anulación de Cobranza.docx |
| N15 | 20260807 Cierre de Caja.docx |
| N16 | 20260809 Cambios e Ingresos de valores.docx |
| N17 | 20260914 Revisión CAJA.docx |
| N18 | 20260928 REIMPRECIÓN Z.docx |
| N19 | Aclaración Técnica - Fecha Vencimiento Documentos CtaCte |
| N20 | QRespecificaciones AFIP.pdf |
| N21 | Reimpresión Z |
| N22 | 202604 Habilitación y Cierre General de Cajas SISTEMA GECO - MARCELO.docx (cuaderno Sistema GECO) |

N22 fue leído directamente en el cuaderno Sistema GECO: https://notebook.google.com/notebook/8255a421-b014-4d5a-b472-c034116be4eb. Sus referencias a ubicación en Ventas/Central quedan sustituidas para Caja por ADM-01 y ADM-09; su flujo de habilitación y cierre se conserva.

La extracción consultada en NotebookLM identifica estas fuentes. Las respuestas del modelo sirven para localizar requisitos, no son aprobaciones adicionales. Esta versión se apoya también en las decisiones explícitas del responsable conservadas en la conversación: saldo cv_importe frente a original, DOC con vencimiento, vuelto neto, cheques puros, navegación de CD, configuración local/iniciador, IndexedDB, mensajes fiscales, grillas y teclado. Se registra la fecha de consolidación; no se inventan fechas individuales para mensajes sin fecha verificable.

### Evidencia técnica local complementaria

Rutas relativas a D:/Sis25/git/GeConnect/src:
- gc.caja.iniciador/README.md: despliegue, configuración del puesto y respaldo.
- gc.caja/Docs/CuentaOperador.md: cuenta y cambio de clave.
- gc.caja/Docs/AdministradorCaja.md: implementación, observaciones para el dueño del original y límites de prueba.
- gc.caja/Docs/ReimpresionZ.md: integración, límites y preguntas al DBA.
- gc.infraestructura/EntidadesComunes/Options/CajaSettings.cs: enums de configuración.
- gc.caja/Models/PresentacionComprobante.cs: presentación de emisión.
- gc.caja.core/Servicios/Implementacion/Cajas/ReglasCreditoPago.cs: reglas compartidas de crédito.
- gc.caja.core/Servicios/Implementacion/Cajas/FactDiferidaServicio.cs: consulta de pendientes.
- gc.caja/Areas/Facturacion/Controllers/NotaDebitoCreditoController.cs: matriz y validaciones de conceptos.
- gc.caja/Tests/ReglasPago.Casos.json: matriz de pagos.
- gc.caja/Tests: suites de NC, documentos, vuelto, presentación, cuenta, sorteo, respaldo, autorización y navegación.

Los documentos técnicos amplían el “cómo”; esta bitácora concentra el “qué debe ocurrir”. Si una implementación contradice una decisión confirmada, registrar el defecto y corregirlo: no cambiar silenciosamente la regla para justificar el código.

## 20. Historial y procedimiento de cambio

| Versión | Fecha | Cambio y autoridad |
| --- | --- | --- |
| 1.1 | 2026-10-05 | Aclaración del analista adoptada por el responsable: Administración distribuida sin apertura; primera etapa apertura/cierre general (ADM-01 a ADM-10); lectura N22 de Sistema GECO, comparación con original, implementación y pruebas aisladas. PUE-06, CIE-01 y UX-02 distinguen cierre de puesto y cierre general. |
| 1.0 | 2026-10-05 | Creación solicitada por el responsable funcional. Consolidación de decisiones de desarrollo, 21 fuentes antecedentes del cuaderno y contratos locales; precedencia, discrepancias, aceptación y pendientes explícitos. |

Para cada cambio futuro: identificar regla por ID, registrar texto anterior/nuevo o regla reemplazada, motivo, decisión y fecha verificable, módulos afectados, pruebas y limitaciones. Actualizar versión y publicar la misma bitácora en NotebookLM. Una propuesta pendiente debe seguir etiquetada P hasta su aprobación; no convertirla en regla confirmada sólo por haber sido agregada al documento.
