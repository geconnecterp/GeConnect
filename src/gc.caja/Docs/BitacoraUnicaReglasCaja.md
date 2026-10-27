# GECO Caja — Bitácora única de reglas de negocio consolidadas

**Identificador:** CAJA-BITACORA-UNICA  
**Versión vigente:** 1.20 — 2026-10-27
**Ámbito:** gc.caja y sus servicios compartidos de Caja.  
**Archivo canónico:** gc.caja/Docs/BitacoraUnicaReglasCaja.md, dentro de D:/Sis25/git/GeConnect/src.  
**Cuaderno de referencia:** Arquitectura de Configuración y Acceso al Sistema de Caja.  
**Destino:** https://notebook.google.com/notebook/00678ebe-1adf-4b1e-b230-21488e92ae71
**Publicación:** revisión 1.20 local; carga en NotebookLM pendiente por bloqueo de acceso del navegador. No se afirma que el cuaderno ya contenga esta revisión.

## 1. Autoridad, alcance y uso

**GOB-01 — Fuente maestra.** Por pedido expreso del responsable funcional, esta bitácora reúne las reglas acordadas y es el primer punto de consulta para resolver contradicciones con fuentes iniciales. Conserva los antecedentes: no elimina los requerimientos originales ni sustituye sus detalles no modificados. “Consolidado” no significa que cada integración haya sido probada en producción.

**GOB-02 — Precedencia.** Aplicar, en este orden: (1) decisiones posteriores explícitas del responsable, incorporadas al historial; (2) reglas confirmadas de esta versión; (3) revisión específica más reciente del módulo, cuando esta bitácora no resuelva el punto; (4) documentos iniciales. Una respuesta generada en un chat, una inferencia o el comportamiento accidental del código no crean una regla de negocio. Si dos reglas de igual autoridad siguen siendo incompatibles, registrar pendiente y solicitar decisión. No extender una excepción de un módulo a todos los demás.

**GOB-03 — Evidencia.** Cada bloque indica su origen:
- **C:** decisión explícita del responsable en la conversación de desarrollo, consolidada al corte de esta versión.
- **F:** requisito documental recuperado del cuaderno, con documento identificado. No implica auditoría de cada línea del código o del SP desplegado.
- **T:** contrato o comportamiento técnico observado en el repositorio; no sustituye una decisión funcional pendiente.
- **P:** pendiente o limitación; no debe presentarse como resuelto.

**GOB-04 — Mantenimiento único.** Actualizar este mismo archivo y su historial ante nuevas decisiones; mantener una sola versión vigente en NotebookLM. La copia cargada allí es una publicación, no una sincronización automática con Git. Tras cada actualización verificar versión y contenido procesado. Si la plataforma exige reemplazar la fuente, verificar la nueva antes de retirar la versión anterior, siguiendo los permisos aplicables. No crear documentos competidores llamados “reglas finales”, “última bitácora”, etc.

**GOB-05 — Responsabilidades.** El responsable funcional decide las reglas. El desarrollo mantiene aplicación, contratos y pruebas. El DBA modifica los SP y resuelve dudas sobre persistencia/SQL; esta consolidación no autoriza cambios de SP. No incluir contraseñas, tokens, certificados privados ni datos reales de clientes en esta bitácora. Aclaración del responsable del 2026-10-24: el tester también cumple el rol de DBA y valida la red de SP auxiliares. Las observaciones del desarrollo sobre un SP aislado son alertas para contrastar, no prueba de que el circuito completo carezca de un control. Concentrar las correcciones en la aplicación y la integración; no condicionar su avance a cambios SQL inferidos. [C]

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

**FAC-16 — Cantidad previa en búsqueda de productos.** Al elegir Productos, conservar el campo de carga. Una cantidad positiva entera o con hasta tres decimales (punto decimal), con signo + final opcional, se combina con el código seleccionado: 4.350 y producto 031225 producen 4.350+031225; 2.357+ produce 2.357+031225. Aplicar esta mejora sólo a productos sin código de barras; si el seleccionado tiene EAN, advertir sin cargar y conservar exactamente la entrada previa. Cero, negativos, exceso de decimales o signos inválidos no cargan ni reemplazan lo escrito. Sin cantidad previa, mantener la carga automática de uno para producto sin EAN y la preparación del código para confirmación manual para producto con EAN. Cancelar la búsqueda conserva la entrada. Usar el servicio existente y sus reglas de presentación/bulto, conservando el origen de búsqueda avanzada; no multiplicar cantidades en el navegador. [C: 2026-10-08; T: 23 escenarios JS con solicitudes simuladas; integración real pendiente de prueba del operador]


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

**PAG-08 — Registración de cobros electrónicos realizados.** Al pulsar Finalizar, si los valores cargados incluyen tarjeta, Mercado Pago/Mercado Libre o Clover, registrar directamente sin preguntar si desea pagar ni ofrecer Cancelar en una confirmación final: el responsable informa que el cobro real ya ocurrió al cargar el valor. Aplica a Facturación CR/CF, Cobranza Diferida CD, Cobranza en Cuenta Corriente CC y Cambio/Ingreso de Valores CV/IV cuando su catálogo habilite esos medios, incluyendo pagos mixtos y NC. Conservar todas las validaciones, contratos, autorizaciones y resultados existentes; bloquear duplicados desde la preparación de CD, antes de obtener los comprobantes a cancelar. Sin esos medios se conserva la confirmación habitual. Este cambio no llama al proveedor para cobrar o reintegrar ni convierte el rechazo de la registración en una devolución bancaria. [C: responsable funcional, 2026-10-09; T: pruebas aisladas]

**PAG-09 — Captura temporal para tarjeta/MP/Clover.** Por decisión del responsable, hasta disponer del módulo específico, reutilizar el formulario de transferencia para capturar un cobro realizado externamente: importe, referencia (mínimo cinco caracteres) y fecha, con controles existentes de transferencia. Mostrar “Detalle del cobro realizado” y conservar tcf_id e ins_id originales del catálogo, sin convertir tarjeta en BA ni sustituir el instrumento por un banco. En ventas/cobranzas, enviar la referencia en rb_dato3_valor con el formato de transferencia (relleno hasta 15 posiciones) y la fecha en rb_fecha_valor; no inventar id_externo, autorización del proveedor ni integración POS. Es una registración manual transitoria, no ejecución bancaria. Detectar TC, TD y MP y las descripciones explícitas de tarjeta/Clover/Mercado Pago o Libre; no inventar códigos de Clover. La prueba real de aceptación de ese contrato por API/SP y la integración del proveedor permanecen pendientes. [C: responsable funcional, 2026-10-09; T/P]

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

**ADM-05 — Cierre general.** Sólo si caja_habilitadas = S (ADM-11), mostrar la sección de cierre y la grilla de puestos abiertos de la sucursal de login mediante SPGECO_Caja_PV_Abiertos / ObtenerPVAbiertos: número de cierre, punto de venta y cajero. Si hay al menos un puesto abierto, deshabilitar Cierre General. Permitir actualizar. Reconsultar desde el servidor al confirmar antes de invocar SPGECO_CAJA_Grl_Cierre / CierreCajaGral (usu_id, adm_id). Si la consulta falla o es inválida, bloquear el cierre; una falla no equivale a lista vacía. Mostrar resultado de cierre o mensaje de rechazo. [F: N22; T: refuerzo en Caja]

**ADM-06 — Confirmación y resultados.** POST con antiforgery; bloquear controles durante confirmación/envío. Impedir solicitudes simultáneas de gestión general de una misma sucursal dentro de la instancia y recordar 24 horas el resultado exitoso/incierto del mismo identificador de solicitud. No reintentar automáticamente después de timeout, desconexión o respuesta inválida. Mostrar resultado incierto y pedir verificación operativa antes de repetir. La exclusión y memoria de solicitudes son locales al proceso web; no garantizan idempotencia persistente entre reinicios ni coordinación entre servidores. [T]

**ADM-07 — Salida.** Habilitación y cierre general muestran su resultado en Administración. Si el resultado es OK, ambas acciones quedan bloqueadas y, al aceptar o cerrar el mensaje informativo, se regresa automáticamente a la pantalla general de inicio. Un error permanece en Administración; un resultado incierto bloquea ambas acciones y conserva consulta y salida manual al inicio para verificar antes de repetir. No abrir un PV ni cerrar la sesión automáticamente por éxito de gestión general. Esto no cambia CIE-01. [C: responsable funcional, 2026-10-09]

**ADM-08 — Alcance futuro.** Gestión POS Clover; Anulación de Operaciones Clover; Anulación de Operaciones MP; CAEA Consultar; CAEA Solicitar; CAEA Pasar Novedades. No quedan implementadas ni activas en esta etapa. Debe resolverse individualmente si se distribuyen o centralizan; no extender automáticamente a ellas las decisiones de las primeras dos opciones. [C/P]

**ADM-09 — Migración del Central.** No modificar gc.sitio, su implementación original, SP ni registros de menú central en esta etapa. Posteriormente retirar desde los registros de ítems de menú vta_cajas_grl_hab y vta_cajas_grl_cierr, según la decisión y despliegue coordinado del responsable. Preparar observaciones al dueño del original; no se han enviado. [C/F: N22]

**ADM-10 — Validación y límite pendiente.** Se verificó compilación de Caja, 31 casos aislados de servicio/controlador y pantalla real con API simulada: consulta fallida, puestos abiertos, confirmación y número de proceso. No se ejecutó habilitación/cierre real ni se auditó el SQL desplegado. DBA debe confirmar que el SP de cierre valida de forma atómica ausencia de puestos abiertos y estado de la sucursal, incluso ante aperturas concurrentes y múltiples instancias. La reconsulta de Caja reduce el riesgo pero no reemplaza esa garantía SQL. [T/P]

**ADM-11 — Estado explícito de habilitación general.** Consultar SPGECO_CAJA_DATOS, mediante ObtenerDatosCF con el caja_id configurado de la estación, y verificar pertenencia a la sucursal del login. El campo correcto, confirmado por el responsable, es **caja_habilitadas** (plural): **S** oculta Apertura General y presenta Cierre General y Puestos abiertos; **N** presenta sólo Apertura General y no consulta ni muestra puestos abiertos o cierre general. Una grilla vacía no determina habilitación general. Si el campo falta, es inválido, la consulta falla o la sucursal/puesto no coinciden, no habilitar operaciones por suposición. Antes de ejecutar, reconsultar DATOS; para cerrar, exigir S y volver a consultar puestos vacíos. No se modifica el contrato compartido ni SP. [C: 2026-10-06; T]

**PUE-08 — Operar Caja y alternativa Z.** Si Valida_Ingreso permite el ingreso normal (resultado 0), continuar el circuito operativo sin mostrar el selector de Reimpresión Z por el solo hecho de que exista un controlador compatible. Si informa caja cerrada (resultado 3), conservar la alternativa explícita de Z cuando el controlador sea compatible. La sesión autenticada no implica caja abierta. Se mantiene el acceso propio del módulo de reimpresión; esta decisión corrige la interrupción del ingreso, no redefine la impresión fiscal de cierre. [C: aclaraciones del responsable 2026-10-06; T]

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

## 20. Clover como instrumento integrado

Estado vigente (CLV-20 y CLV-25): integración de opciones, pago y confirmación común sin consulta ni validación del estado POS/diario. El contrato financiero vigente es **rb_importe = total cobrado, rb_rec = porcentaje, rb_aux = importe monetario del ajuste**, confirmado por responsable/DBA el 2026-10-27. Las referencias anteriores a rb_rec como importe se conservan como antecedentes y no gobiernan nuevas implementaciones. Las pruebas reales de recuperación manual e imputación están en CLV-21/22; la actualización del contrato de CLV-25 se verificó con SP/POS simulados y no certifica una nueva emisión fiscal real. T1 abre Clover y el catálogo determina su instrumento. Detalle: CloverPlanImplementacion.md.

**CLV-01 — Opciones.** Monto sugerido = remanente de factura/cobranza. Consultar SPGECO_CAJA_Valores_Ins_Opciones con el ins_id real del catálogo de T1. G00 aparece en el documento inicial, pero no es un código impuesto por la aplicación; G0 también funciona si es el configurado. Conservar cuota real y pos_plan como datos distintos; al POS se envía pos_plan. Mostrar pos_desc y recargo_porcentaje. Clover_Pay recibe pos_plan como @cuota y lo devuelve en rb_opcion_cuota; no sustituirlo por cuota real. [F: L23, párrafos 4-5; P: integración]

**CLV-02 — Importes y porcentaje.** Recargo monetario = Round(monto base * recargo_porcentaje / 100, 2). POS recibe base + recargo. En json_valores, rb_importe contiene el total cobrado, rb_rec contiene el porcentaje exacto de la opción validada del catálogo, y rb_aux contiene el importe monetario calculado de esa fila. Integrar rb_aux una sola vez al exigible y mostrarlo en Recargos/Descuentos; no sumar porcentajes como dinero ni inferirlos desde importes redondeados. Si la opción es un descuento, conservar el signo en ambos campos. CD mantiene la prohibición de ajustes financieros. Esta asignación sustituye la anterior rb_rec=importe, rb_aux=0. [C: aclaración DBA/responsable, 2026-10-27; CLV-25; F: opciones L23/L24]

**CLV-03 — POS y recuperación.** SPGECO_CAJA_Clover_Pay usa paymentid vacío al solicitar pago POS; QR es un bit. La recuperación manual llama al mismo SP con ID obligatorio de al menos tres caracteres alfanuméricos, normalizado a mayúsculas. Debe validar un pago existente y pendiente, no crear otro cobro ni reutilizar uno imputado. No aplicar la referencia libre de cinco caracteres/relleno de Transferencia a este ID. [F: L23, párrafos 8-13; charla 58-95 y 147-158]

**CLV-04 — Valores canónicos.** Interpretar respuesta y respuesta_msj del SP; cargar los recibo_valores devueltos, conservando el ID externo y demás campos conforme al SELECT SQL recibido. El cajero no puede editarlos, eliminarlos ni cancelar el pago aprobado. También validar en servidor: una marca en JS no prueba aprobación. CLV-11/12 implementan la validación canónica, persistencia y recuperación en aplicación; aceptación del SQL desplegado pendiente. [F: L23, párrafos 11-13; P/T: esquema y protección pendientes]

**CLV-05 — Finalización.** Si el pago aprobado cubre todo el saldo, finalizar automáticamente la operación común de Caja una sola vez, sin pregunta ni último clic. Si resta saldo, conservar el aprobado y completar el resto. SP de cobro Clover no sustituye SPGECO_CAJA_Ope_Confirmar. Implementado en CLV-11 con pruebas simuladas; no extiende la finalización automática a cualquier medio electrónico. [F: L23, párrafo 11; charla 217-260; P: implementación]

**CLV-06 — Cancelación e incertidumbre.** Volver cancela la preparación sin pago; no revierte un cobro aprobado. La charla demuestra cancelación física en POS y reporta que una cancelación remota puede responder OK sin detener la espera. No prometer reversión por cerrar modal/abortar HTTP. Espera prevista de aproximadamente cinco minutos requiere revisar todos los timeouts. Resultado incierto exige verificar/recuperar, sin repetir automáticamente el cobro. [F: L23, párrafo 14; charla 99-193 y 272-273; P: contrato de consulta de estado]

**CLV-07 — Alcance.** Integrar como medio del motor común de Facturación CR/CF, CD y CC sólo si el catálogo lo habilita; preservar emisión/stock/recibo y navegación de cada módulo. Extender a CV/IV requiere revisar su catálogo y confirmación propia. Cashback, Gestión POS, cancelación remota, anulación y autorización futura para editar ID no se consideran resueltos. No modificar SP ni gc.sitio desde el plan. [F: L23; charla 95 y 159-245; P: contratos de extensión]

**CLV-08 — Pendientes para ejecutar.** Recibidas firma y definición de Clover_Pay y Valores_Ins_Opciones. Faltan ejemplos de ejecución, correcciones de longitud/ID y garantías de imputación única, conciliación tras timeout, contexto del dispositivo compartido y tratamiento de rb_rec en SQL desplegado. La integración autorizada admite monto parcial y pagos mixtos; conservar los pendientes del SP y la conciliación. Revisión estática y una prueba aislada de total con recargo realizadas; no se anuncia integración probada. [T/P]

**CLV-09 — Antecedente: etapa de simulación autorizada (superada por CLV-11).** Por instrucción del responsable, implementar modal y pruebas sin ejecutar cobros. El controlador Clover rechaza incondicionalmente la conexión operativa, incluso por POST directo; fixtures e identidad ficticia pertenecen sólo al host Tests de loopback. En simulación se comprueban aprobación/rechazo/cancelación, recuperación, recargos, respuesta incierta, doble envío y señales FINALIZAR/COMPLETAR; ninguna señal se registra en Checkout. G00 sale del fallback de Transferencia. Protección real en servidor, recargos finales, persistencia e integración POS siguen pendientes. 58 escenarios JS y 19 verificaciones HTTP/Razor/contrato aprobados, además de regresiones existentes y compilación aislada. [C: autorización 2026-10-23; T/P]

**CLV-10 — Discrepancias del SQL entregado.** La firma inicial de Opciones recibía ins_id char(2) frente a G00; el responsable entregó la corrección a char(3) el 2026-10-24 y se verificó desplegada junto con sus tres filas reales (CLV-18). Esa discrepancia queda resuelta. Clover_Pay utiliza LEN(paymentid) > 3 para consultar: con longitud tres cobraría. El prototipo bloquea ese ID y no modifica el SP. SELECT @paymentid = @paymentid no copia el ID del resultado POS, por lo que un OK sin identificación se clasifica incierto. Las demás diferencias entre instrumento guardado/devuelto, importe/plan de recuperación, concurrencia y aprobación result/state están documentadas para DBA en CloverPlanImplementacion.md; no se consideran resueltas por este cambio de firma. [F: SQL entregados 2026-10-23; T/P]

**CLV-11 — Integración completa autorizada en desarrollo.** El responsable pidió conectar todas las capas para pruebas del tester. Opciones llama SPGECO_CAJA_Valores_Ins_Opciones; POS y recuperación llaman SPGECO_CAJA_Clover_Pay; la finalización conserva SPGECO_CAJA_Ope_Confirmar. Caja deriva contexto y deuda de sesión, reconsulta catálogo/NC/estado PV antes de cobrar y valida importe/porcentaje/plan en API. Se aplican valores canónicos protegidos, sumando una sola vez el recargo al exigible; rb_importe incluye recargo y rb_rec contiene exclusivamente ese ajuste. Si cubre saldo finaliza automáticamente, sin pregunta; los parciales permiten completar. CR/CF, CD, CC y CV/IV mantienen su circuito propio. CD continúa sin recargos. Los SP no fueron modificados. [C: responsable 2026-10-23; T: integración simulada]

**CLV-12 — Diario e idempotencia.** La API conserva solicitudes, respuestas, aprobaciones y registro final en un directorio durable del servidor, configurable con Clover:DirectorioEstado. No es configuración ni respaldo de productos del puesto. Escribir antes del efecto externo y usar exclusión de archivo por sucursal/PV; el mismo intento devuelve su resultado, un ID queda asociado a una operación, la confirmación repetida idéntica recupera su resultado. Conservar montos, plan, ID, lote/cupón, fecha y recargo canónicos. Un cobro incierto impide otro POS; recuperación manual debe conservar monto y plan del intento. Un registro final incierto exige conciliación: no se reejecuta automáticamente. Cierre del navegador no anula un pago. Para varias instancias de API, todas deben compartir este mismo directorio y exclusión; no se declara coordinación de un dispositivo físico compartido entre distintos PV sin garantía del proveedor/DBA. [C/T/P: conciliación SQL y POS compartido] **Antecedente sustituido por CLV-20 el 2026-10-26: no aplicar el diario ni sus bloqueos/recuperaciones al circuito vigente.**

**CLV-13 — Recuperación y límites verificados.** Restaurar aprobaciones, otros valores y NC previas al reingresar al mismo contexto; reconsultar el catálogo. Si el SP final ya registró, recuperar el comprobante almacenado sin volver a cobrar ni registrar. Si hay error conocido de registración, permitir corregir/reintentar sólo el registro; incertidumbre requiere verificar. Conservar descuentos financieros negativos en rb_rec al normalizar OPENJSON; el resto de conversiones existentes permanece. Pasaron 119 verificaciones de servicio/persistencia/contratos, 19 HTTP/Razor/API/Checkout y escenarios JS con regresiones de pagos. Son pruebas con respuestas simuladas; no certifican el SP desplegado, stock fiscal ni POS real. [T] **Antecedente sustituido por CLV-20 el 2026-10-26: no aplicar el diario ni sus bloqueos/recuperaciones al circuito vigente.**

**CLV-14 — Registro compartido de dependencias.** ApiPagoFactServicio depende de CloverServicio y éste del diario CloverRegistro. Registrar la cadena completa en gc.api.infra.Extensions.ServicioExtensions.AddServicios, común a gc.api, gc.api.report y gc.api.gen; no sólo en Program de gc.api. Mantener Clover scoped y el diario singleton, respetar Clover:DirectorioEstado y usar ContentRoot/App_Data/Clover como alternativa. La creación es diferida: registrar/validar el host no crea el diario. Una regresión reproduce la AggregateException informada y verifica 35 casos de construcción, configuración y scopes con los servicios reales, sin invocar SQL ni POS. [T: corrección 2026-10-24] **Antecedente sustituido por CLV-20 el 2026-10-26: no aplicar el diario ni sus bloqueos/recuperaciones al circuito vigente.**

**CLV-15 — Acceso por medio T1 y datos del catálogo.** POS - Clover es tcf_id T1. Al seleccionarlo, abrir su propio modal antes del recorrido genérico de instrumentos, incluso si la consulta heredada devolvió DEF/Sin instrumentos disponibles o una lista vacía. No usar el formulario temporal de Transferencia para T1. El servidor revalida que T1 esté habilitado y resuelve su instrumento real mediante SPGECO_CAJA_Valores_Ins; si no se indicó uno, requiere un único código configurado. Consultar sus cuotas con ese código, conservarlo hasta Clover_Pay y no transformar G0 en G00 ni viceversa. Sin instrumento/cuotas, mostrar el aviso dentro de Clover y no habilitar cobros. Aplica al motor CR/CF/CD/CC y a CV/IV. Reproducción observada en Facturación con T1 y DEF; lectura de catálogo real sin filas para G0/G00 ni opciones asociadas. El responsable coordina su carga con analista/DBA. Verificados 65 escenarios JS, 124 de servicio, 21 HTTP y apertura visual del Razor con/sin catálogo en host aislado; sin pagos reales. [C/T: 2026-10-24]

**CLV-16 — Coordinación de pruebas con el POS.** El dispositivo de pruebas está en poder del tester en Córdoba. Antes de solicitar un pago Clover, avisar al responsable y esperar su confirmación de que el tester está preparado. Abrir el modal y consultar estado, instrumento o cuotas no autoriza a pulsar Pagar POS. En la revisión del 2026-10-24 no se envió ningún cobro: Estado confirmó pendiente=false, incierto=false y aprobados vacío. La lectura del catálogo ya encontró G00 / POS Clover, pero no opciones para G0/G00; la API compilada a las 09:32 todavía contenía la validación anterior, frente al fuente actualizado a las 12:30. Recompilación/reinicio de Caja y API y configuración de cuotas pendientes antes de certificar el circuito. [C/T: 2026-10-24]

**CLV-17 — Modal sobre la pantalla de pagos.** El componente compartido Clover debe quedar por encima de la pantalla de pagos, con la jerarquía utilizada por los demás detalles (modal 5100, diálogo 5101, fondo 5099), por debajo de los mensajes y el teclado de Caja. La prueba real de Facturación detectó que el CSS anterior usaba 1090 frente a modalPago=5070: el modal estaba abierto en el DOM, pero oculto visualmente. Corregido en pago-clover.css, compartido por CR/CF/CD/CC y CV/IV; verificación visual en Facturación, elemento al frente, Volver y reapertura sin pérdida del total. Los 65 escenarios simulados siguen aprobados. Tras recompilar API y Caja, no se reproduce la excepción de la versión anterior; las opciones reales siguen vacías y el pago permanece deshabilitado. No se ejecutaron cobros, recuperación manual ni confirmación de factura. [T: 2026-10-24]

**CLV-18 — Cuotas G00 y firma actual de opciones.** SPGECO_CAJA_Valores_Ins_Opciones recibe @ins_id char(3); la API conserva los tres caracteres y usa SqlDbType.Char, Size=3. Las opciones se cargan dinámicamente: En 1 Pago / cuota=1 / recargo=0 / pos_plan=1; En 2 Pago / cuota=2 / recargo=13.25 / pos_plan=2; EN 3 PAGO / cuota=3 / recargo=19.17 / pos_plan=3 son las filas entregadas y verificadas al corte, no valores fijos de producción. Recalcular recargo y total al cambiar monto o cuota; conservar pos_plan y cuota como datos distintos. T1 es tcf_id, no co_tipo: pasar CR/CF/CD/CC/CV/IV según la operación; el ejemplo T1 devuelve filas porque el SELECT actual sólo filtra ins_id. CD mantiene exclusivamente opciones sin recargo. Verificadas firma y tres filas invocando el SP real con G00, CR, C0004580 y 0000 y usando GenericDataMapper<CloverOpcionDto>. Con el proyecto principal detenido por el responsable, se verificó el modal real en host aislado con una copia de esas opciones: sobre 100, totales 100/113.25/119.17; sobre 50 en dos pagos, recargo=6.63 y total=56.63. Pasaron 68 JS, 124 integración simulada y 21 HTTP simulados; compilación sin errores. No se invocaron cobros reales ni confirmación real; host y pestaña aislados cerrados. [F/C/T: SP y filas suministrados 2026-10-24]

**Verificación de CLV-18 en Caja autenticada — 2026-10-24.** Tras la recompilación del responsable, se probó el circuito principal https://localhost:7257, caja 0001, proceso 00-06578, cierre 1 y cliente C0004580. Una factura de dos productos por 111420.68 llegó a T1 y el modal obtuvo las tres cuotas desde la API y el catálogo reales, sin fixtures: cuota 1, recargo 0 y total 111420.68; cuota 2, recargo 14763.24 y total 126183.92; cuota 3, recargo 21359.34 y total 132780.02. Edición parcial a 50 en dos pagos: recargo 6.63, total 56.63. Importe 0 o 111420.69 deshabilita Pagar POS al ser cero o superar el saldo. Restaurado el importe 111420.68. No se pulsó Pagar POS, Cargar manual ni Finalizar; no se emitió factura ni se certifica el circuito bancario. Modal abierto para continuar únicamente después de coordinar con el tester. Captura clover-cuotas-caja-real-20261024.jpg. [T: navegador, Caja/API/SP reales de desarrollo]

**CLV-19 — Diagnóstico legible de request/response.** Por solicitud del responsable, registrar en el log existente de Caja las llamadas CloverOpciones y CloverProcesar (CloverEstado fue retirado por CLV-20), y ConfirmarOperacionCaja cuando interviene Clover. Prefijos [CLOVER][REQUEST] y [CLOVER][RESPONSE], identificador de llamada compartido, URL, estado HTTP y duración; el adaptador Clover distingue SIN_RESPONSE ante fallos de red/timeout y conserva la excepción sin reintentar. Cuerpo JSON con sangría, sin serializar por segunda vez el string del request. Contexto/pago son objetos y valores/uniones son arreglos. CloverJsonDiagnostico expande JSON anidado únicamente en la representación del log; la confirmación conserva sus campos json_* como texto según el contrato existente. No borrar globalmente barras ni comillas de los datos; no modificar el cuerpo enviado para embellecerlo. No registrar cabeceras ni token; ocultar también el token actual si la respuesta lo refleja. El request de confirmación en Checkout y CV/IV usa la vista legible cuando hay Clover. El responsable deja los detalles de persistencia en base a cargo del tester/DBA; no se alteraron SQL ni registros existentes. Pasaron 28 pruebas del adaptador con HttpMessageHandler simulado y 26 HTTP de host aislado, incluida la confirmación por su cliente real y la conservación de idempotencia. Compilación sin errores; sin cobros ni SP reales. [C/T: 2026-10-26]

**CLV-20 — Pagos Clover independientes, sin validación de estado POS.** Por decisión explícita del responsable/analista del 2026-10-26, no consultar ni validar estado Clover al entrar al pago, abrir el instrumento, procesar cada cobro o finalizar. Se retiran CloverEstado y la recuperación automática del diario; los archivos históricos existentes se preservan y no gobiernan nuevas operaciones. El SP determina el resultado de cada solicitud; la API expone RespuestaDto (resultado, resultado_msj, resultado_id). El SP vigente devuelve columnas respuesta/respuesta_msj y la fila de recibo_valores: se adaptan a ese contrato sin modificar SQL; resultado_id contiene el arreglo de valores de esta respuesta. No imponer verificaciones bancarias adicionales de estado, cliente, plan, lote o cashback sobre un éxito del SP. Conservar datos entregados y validar únicamente estructura de valor para poder mostrarlo. Se mantienen contexto autenticado, catálogo/cuotas vigentes, monto/recargo, saldo, NC y validación operativa normal del PV (distinta del estado POS). Cada éxito agrega su propia fila; se admiten varios pagos Clover parciales junto a los otros medios habilitados. Recalcular recargos una sola vez y finalizar automáticamente cuando el pendiente sea cero, sin confirmación y sin volver a cobrar. La respuesta duplicada en la vista no duplica fila ni dispara otro envío de finalización; bloquear doble clic durante la solicitud y no reintentar automáticamente ante pérdida de respuesta. La API no promete idempotencia duradera con el diario retirado; conciliación y efectos bancarios corresponden a la red de SP. Se conserva recuperación manual por ID vía Clover_Pay y la coordinación con Córdoba de CLV-16. Esta decisión prevalece sobre CLV-10 en cuanto a validaciones adicionales de respuesta y sustituye CLV-12/13/14; sus pruebas anteriores quedan como antecedente. [C: responsable y explicación del analista; T: corrección compartida CR/CF/CD/CC/CV/IV]

**Primera prueba operativa acordada — CLV-20.** El responsable indica comenzar por Cargar manual con un código de transacción ya generado por Clover, para recuperar e imputar el pago existente. No iniciar un pago POS nuevo como primer ensayo. Coordinar la prueba con el responsable y disponer del ID real y monto/cuotas correspondientes antes de invocar el SP real. [C: 2026-10-26]

**Verificación CLV-20.** Compilación aislada de Caja/API sin errores; 65 pruebas JS, 61 de servicio/contratos, 28 HTTP/Razor/API/Checkout, 28 de logs y 30 de dependencias compartidas (212). También pasan regresiones de finalización directa (99), NC (43), reglas de pago (32 y retornos CD) y documento (16). En navegador, host aislado 7293: dos parciales de base 40/60, bruto 44.80/67.20 y recargo 4.80/7.20 alcanzan saldo cero y una única finalización automática. Cargar manual con ID ficticio MANUALTEST001 recupera valor 100 e imputa mediante la confirmación común, sin llamada POS adicional; consola sin errores. Capturas clover-sin-estado-20261026.jpg y clover-id-manual-20261026.jpg. Todos los pagos, SP y confirmaciones de estas verificaciones son simulados; no se contacta el POS de Córdoba. Host detenido al terminar; compilaciones en .artifacts sin bloquear VS.

**Reproducción CLV-20.** En Caja autenticada 7257, Facturación, C4580 (C0004580), Último Detalle: producto 051929, cantidad 0.125, total 4817.80. Al entrar a Pagar, antes de seleccionar un medio, CloverEstado llamó ComprobarContexto contra un archivo anterior de otro detalle y lanzó la excepción informada. No se envió pago ni confirmación. Se retira ese prerrequisito en las capas de vista, Caja y API. Las comprobaciones posteriores usan SP/POS simulados; no certifican un cobro físico. Publicación NotebookLM pendiente.

**CLV-21 — Verificación real de recuperación manual y uppercase.** El responsable autoriza probar primero códigos erróneos/inexistentes y luego la transacción existente D0DWA08TN7F8T, solicitando capturas. En Caja 7257 autenticada, usuario jjbenz, sucursal 0000, caja 0001, proceso 00-06580/cierre 1, Facturación C0004580/Último Detalle por 4817.80: escribir abc presenta ABC y deshabilita Cargar manual por mínimo técnico cuatro; ab-1 presenta AB-1 y mantiene bloqueado por formato. No se envían esos dos casos al SP. Escribir noexiste00001 y zzzzzzzzzzzzz produce uppercase tanto en el campo como en el request Caja/API y en @paymentid. Cada consulta manual devuelve resultado=-1 y «El Pago que desea cargar no existe.»; no agrega filas ni altera el saldo. Después escribir d0dwa08tn7f8t produce D0DWA08TN7F8T; Clover_Pay responde OK, instrumento J14, lote 000001, cupón 000122, rb_cupon_manual=S, rb_importe=4817.80, rb_rec=0. La grilla incorpora una fila, saldo cero y una única finalización automática sin confirmación. SP de confirmación devuelve resultado=0 y Factura B / tco_id 006 / 0001-00000063. Capturas clover-error-noexiste00001-20261026.jpg, clover-error-zzzz-20261026.jpg y clover-manual-factura-b-63-20261026.jpg; request/response legibles conservados en .artifacts/clover-manual-20261026. No se pulsó Pagar POS ni se solicitó un cobro nuevo; se recuperó una transacción existente y se registró la operación de prueba. No repetir el ID para probar nuevamente la misma imputación. [C/T: 2026-10-26]

**CLV-21 / dato para tester-DBA.** En esa respuesta exitosa, pos_importe=43087.32 mientras rb_importe=4817.80. Se respetó el valor de recibo devuelto por el SP según CLV-20, sin agregar validación de estado ni sustituirlo por pos_importe. No se declara resuelta la interpretación de imputación parcial, saldo restante o marca de transacción completa: el tester/DBA debe contrastarlo con su red de SP. Esta evidencia no modifica SQL ni crea una regla de parcialidad.

**CLV-21 / corrección local de parámetro.** El log reveló @cashback con Value nulo al construir SqlParameter con el literal decimal cero. Se reproduce con una regresión aislada y se corrige el parámetro a SqlDbType.Decimal, Precision=15, Scale=2, Value=0m explícitos. Se mantienen cobro sin cashback y el resto de parámetros. Pasan 66 verificaciones de servicio/contratos, incluida la regresión en cinco tipos de operación; no se repite la transacción real. Recompilar los hosts API consumidores para desplegar este ajuste local. [T: sin cambios de SP]

**CLV-22 — Rechazo de reutilización de transacción imputada, verificación real.** A pedido explícito del responsable se vuelve a consultar, sólo por Cargar manual, D0DWA08TN7F8T luego de su imputación en Factura B 0001-00000063. Se reinicia la identificación de C0004580 y se prepara otro borrador: producto 051929, cantidad 0.250, total 9635.59. Se conserva monto manual 4817.80, una cuota, sin recargo, para observar el resultado sin cubrir el nuevo total. La operación nueva es ebc390b3-d468-42be-a525-52f8a2c89d3c y el intento afd3bbe1-4180-4ec7-b287-71ed317b9db4; difieren de los de la primera imputación. El SP rechaza con resultado=-1, resultado_id vacío y texto exacto «El Pago que desea cargar ya se imputo». La API comunica el rechazo y la vista lo muestra; grilla sin valores, total otros valores=0, saldo=9635.59 y Finalizar deshabilitado. No se emite otra factura ni se envía confirmación final, no se pulsa Pagar POS ni se ejecuta un cobro nuevo. Este rechazo proviene del SP; no depende de reintroducir consultas de estado o del diario local. Captura clover-ya-imputado-20261026.jpg y logs reutilizacion-request-response-caja.log / reutilizacion-request-response-sp.log en .artifacts/clover-manual-20261026. Se conserva el mensaje visible para revisión. No se modifican código operativo ni SP en esta prueba. [C/T: 2026-10-26]

**CLV-23 — Cobro aprobado y error de emisión son resultados distintos.** El responsable informa el caso de C0009872: producto por 9635.59, percepción IIBB=79.63 y percepción IVA=238.90, total base=9954.12; tres pagos al 19.17%, recargo=1908.20, rb_importe=11862.32. ConfirmarOperacionCaja devuelve resultado=-2, resultado_id vacío y errores CAE 10048/10061. El responsable confirma que el popup apareció y luego quedó la fila Clover «Aprobado»: corresponde al cobro aprobado previamente por Clover, no prueba emisión fiscal ni registración completa en Caja. No cancelar, quitar, volver a cobrar ni cambiar ese pago a rechazado por el fallo posterior. No reintroducir consultas de estado POS. Se aclara el texto compartido de errores de registración cuando hay medios electrónicos: «No se pudo completar la operación de Caja», con cobro aprobado y emisión/registración pendiente, detalle completo del servidor y advertencia de conservar los pagos. Al cerrar, persiste un aviso en el pago; al confirmar con éxito o iniciar otra operación se limpia. La fila Clover muestra «Cobro aprobado» y un tooltip que distingue el resultado de Caja. El controlador conserva el código resultado del SP al devolver el error; no transforma -2 en éxito ni descarta pagos/detalle. El resto de medios y el flujo de confirmación conservan su contrato. [C: aclaración y capturas del responsable; T: 2026-10-26]

**CLV-24 — Resumen financiero por fila.** Actualizar los renglones visibles Recargos y Descuentos desde actualizarTotalesPago, sin depender de que se consulte o edite una NC. La actualización anterior estaba incorrectamente ubicada dentro de obtenerMaximoImputableCreditoNC: el saldo incluía el ajuste pero el renglón podía mostrar cero. Sumar rb_aux positivos a los recargos base y el valor absoluto de negativos a los descuentos base, en centavos (contrato actualizado por CLV-25; hasta 2026-10-26 ese monto se ubicaba en rb_rec). No mutar conceptosPago.recargos/descuentos ni el valor aprobado, para evitar acumular otra vez al recalcular. En el caso del responsable: base=9954.12, Recargos=1908.20, Otros valores=11862.32, Diferencia=0. Dos Clover con ajustes de distinto signo mantienen ambos renglones y un único efecto neto. CD conserva la prohibición de recargos financieros. [T: 2026-10-26]

**CLV-23/24 — Verificación y alcance fiscal pendiente.** Pasan 46 escenarios NC/pago, 8 de error de registración (incluyendo -2, preservación del cobro, completar sólo registro y limpieza del aviso), 65 Clover, 99 de finalización directa, 16 DOC, 32 reglas y 35 HTTP/Razor/API/Checkout. Compilación aislada sin errores. Se reprodujo visualmente con los parciales y funciones reales en el host 7293, exclusivamente con datos/SP/POS/AFIP simulados. Capturas clover-cobro-aprobado-error-emision-20261026.jpg y clover-recargo-y-registro-pendiente-20261026.jpg. La prueba HTTP confirma que el request conserva importe bruto/recargo y los tres subtotales originales, y que una respuesta -2 no genera éxito ni vuelve a invocar Clover_Pay; un resultado posterior simulado OK completa el registro con el mismo detalle. No se contactó el POS, no se repitió la operación real y no se tocaron SP. El JSON entregado por el responsable cierra aritméticamente; no contiene el payload fiscal final enviado a AFIP. Queda para tester/DBA revisar en su red de SP la incorporación de rb_rec a ImpTotal, ImpNeto, ImpIVA y BaseImp de AlicIva, sin atribuir un campo específico ni inventar modificaciones de json_p/json_subtotal desde Caja. No se declara corregida la causa fiscal 10048/10061. Evidencia: .artifacts/clover-afip-20261026. Recompilar gc.caja para desplegar las correcciones de vista/controlador. Publicación NotebookLM pendiente.

**CLV-25 — Contrato financiero corregido por DBA.** El responsable entrega nuevamente SPGECO_CAJA_Clover_Pay el 2026-10-27 y la aclaración del DBA: «en rb_rec debes enviar el porcentaje de recargo, y en rb_aux el recargo». Se confirma que la firma del SP de cobro sigue recibiendo @monto con total incluido el ajuste, @cuota con pos_plan y los demás parámetros existentes; su SELECT sigue entregando respuesta/respuesta_msj, instrumento, ID/lote/cupón, cuota, fecha, importe y datos POS. No se agregan parámetros SQL ni se exigen nuevas columnas de retorno. La API arma el valor de confirmación con rb_rec desde opcion.recargo_porcentaje validada en el servidor y rb_aux desde el importe de recargo calculado; rb_importe se conserva del retorno del SP. La preparación de Clover conserva recargo como monto monetario interno; no se confunde ese DTO con json_valores. El transporte de resultado continúa con RespuestaDto.

**CLV-25 — Aplicación compartida.** En CR/CF/CD/CC, calcular saldo, resumen, base recuperada y total exigible desde rb_aux; conservar rb_rec para porcentaje y observaciones. La base recuperada se calcula en centavos, evitando residuos de punto flotante. En CV/IV preservar ambos campos durante copia y serialización y mostrar en las observaciones el porcentaje y su importe. El normalizador de confirmación conserva el signo de rb_rec y rb_aux en descuentos; las restantes conversiones numéricas no cambian. CD rechaza valores con cualquiera de los dos ajustes distinto de cero y continúa filtrando opciones financieras en el catálogo. Sin consultas de estado POS ni reintroducción de diario. El caso previo de 9954.12 al 19.17% se confirma como rb_importe=11862.32, rb_rec=19.17 y rb_aux=1908.20. La base, percepciones y demás JSON de artículos/subtotales no se rehacen desde la aplicación por este cambio. Esta aclaración prevalece sobre ejemplos y pruebas históricas donde rb_rec representaba dinero.

**CLV-25 — Verificación y despliegue.** Compilación aislada de Caja/API sin errores; 71 comprobaciones de servicio/contratos, 66 escenarios Clover JS, 46 NC/pago, 8 errores de registro, 39 HTTP/Razor/API/confirmación (incluida serialización real CV/IV), 29 logs, 99 finalización directa, 32 reglas y 16 DOC. Se verifican varios pagos parciales con porcentaje individual, ajuste monetario sumado una sola vez, tasa no inferida cuando un recargo redondea a cero, y descuento con porcentaje/importe diferentes y signo preservado hasta OPENJSON. Ejemplo generado por la API/confirmación simuladas: .artifacts/clover-contrato-20261027/json_valores-ejemplo.json. No se ejecutan SP reales, cobros ni nuevas emisiones fiscales en esta corrección; no se modifica ni despliega SQL. La aceptación fiscal con el SP corregido por el DBA queda pendiente de prueba real coordinada. Desplegar y reiniciar Caja y los hosts API consumidores en conjunto, usando la pantalla/JS actualizada; no mezclar los significados de campos de versiones anteriores ni repetir cobros ya aprobados para actualizar un JSON. Bitácora local 1.20; publicación NotebookLM pendiente. [C/T: 2026-10-27]

Fuentes locales nuevas: L23 = D:/#Documentos/#GECO/Caja/20261023 Pago Clover.docx (archivo entregado; no leído desde NotebookLM). Charla = D:/Sis25/git/GeConnect/doc/20261023 - Transcripcion charla CLOVER.TXT. L24 = SPGECO_CAJA_Clover_Pay y aclaración financiera entregados por el responsable/DBA el 2026-10-27, adjunto dffe6865-a540-4716-b79f-dab0958700b2/Texto pegado.txt. No se carga la charla completa ni sus conversaciones personales en la bitácora.

## 21. Historial y procedimiento de cambio

| Versión | Fecha | Cambio y autoridad |
| --- | --- | --- |
| 1.20 | 2026-10-27 | CLV-25: rb_rec=porcentaje, rb_aux=importe, rb_importe=total. Alineados API, saldos, resumen, CV/IV y signo de descuentos. Pruebas simuladas y compilación sin errores; emisión fiscal real pendiente. Publicación NotebookLM pendiente. |
| 1.19 | 2026-10-26 | CLV-23/24: diferenciar cobro aprobado de emisión fallida, aviso persistente y código -2; corregir recargos/descuentos visibles sin duplicarlos. Regresiones y prueba visual aislada. Causa fiscal pendiente tester/DBA; sin modificaciones SQL ni cobros. Publicación NotebookLM pendiente. |
| 1.18 | 2026-10-26 | CLV-22: reutilización del ID D0DWA08TN7F8T en otra factura rechazada por SP con resultado -1; grilla vacía, saldo intacto y sin segunda emisión. Captura y logs conservados; sin cambios de código ni SP. Publicación NotebookLM pendiente. |
| 1.17 | 2026-10-26 | CLV-21: prueba manual real con dos rechazos del SP, uppercase hasta SQL y Factura B 0001-00000063; capturas y logs. Diferencia pos_importe/rb_importe registrada para tester/DBA. Corregido cashback decimal cero con regresión simulada. Publicación NotebookLM pendiente. |
| 1.16 | 2026-10-26 | CLV-20: retirar estado POS/diario, RespuestaDto, múltiples pagos y auto-finalización; preservar históricos y logs. Pruebas simuladas y compilación, sin SQL/POS reales. Publicación NotebookLM pendiente. |
| 1.15 | 2026-10-26 | Logs request/response Clover y confirmación con JSON legible, correlación, HTTP/duración y token omitido. Request serializado una sola vez; contratos y SQL conservados. 28 pruebas de diagnóstico y 26 HTTP con SP/POS simulados aprobadas. Publicación NotebookLM pendiente. |
| 1.14 | 2026-10-24 | Prueba en Caja autenticada con API/SP reales de desarrollo: carga de las tres cuotas, cálculo sobre 111420.68, parcial con redondeo y bloqueo de cero/exceso correctos. Sin cobros POS, recuperación manual ni confirmación de factura. Evidencia visual guardada; publicación NotebookLM pendiente. |
| 1.13 | 2026-10-24 | Opciones Clover char(3) y cuotas G00 reales 1/2/3 con 0/13.25/19.17 %. Contrato API ajustado; firma y mapeo del SP comprobados en lectura; modal/cálculos verificados en host aislado. 68 JS, 124 integración, 21 HTTP; sin cobros reales. Se cierra sólo la discrepancia char(2)/G00. Publicación NotebookLM pendiente. |
| 1.12 | 2026-10-24 | API y Caja recompiladas: excepción anterior superada. T1 abre Clover en entorno real; corregida capa CSS oculta detrás de modalPago. Verificados apertura visible, Volver y reapertura, sin pagos; 65 escenarios simulados aprobados. Instrumento G00 presente, opciones/cuotas aún sin datos. Publicación NotebookLM pendiente. |
| 1.11 | 2026-10-24 | Prueba de desarrollo reiniciada desde Operar Caja: contexto válido, T1 abre modal Clover y estado sin cobros. G00 presente y cuotas aún ausentes en lectura del catálogo; DLL API anterior pendiente de recompilar/reiniciar. Regla CLV-16: coordinar con responsable y tester de Córdoba antes de Pagar POS. Sin cobros ni cambios SQL. Publicación NotebookLM pendiente. |
| 1.10 | 2026-10-24 | POS Clover T1 abre modal propio aun con DEF/catálogo vacío. Instrumento y opciones resueltos desde catálogo sin imponer G0/G00; datos pendientes a coordinar por responsable con analista/DBA. 65 JS, 124 servicio y 21 HTTP, prueba visual aislada; sin pagos reales. Publicación NotebookLM pendiente. |
| 1.9 | 2026-10-24 | Aclaración de responsabilidades: tester/DBA valida la red de SP; alertas de procedimientos aislados no equivalen a defectos del circuito completo. Corrección de registro DI Clover centralizado para gc.api, gc.api.report y gc.api.gen; 35 verificaciones sin SQL/POS. Publicación NotebookLM pendiente. |
| 1.8 | 2026-10-23 | Responsable autoriza integración completa en desarrollo: SP de opciones y Clover_Pay conectados, diario API, canónicos, recargos firmados, finalización y recuperación. 119 verificaciones de servicio y 19 HTTP completas con SP simulados; sin cobros ni SQL real. Desplegar Caja y API; pendientes SQL/DBA conservados. Publicación NotebookLM pendiente. |
| 1.7 | 2026-10-23 | Modal Clover y contratos implementados con transporte real bloqueado; 58 escenarios simulados y 19 verificaciones HTTP/Razor. SQL recibidos y discrepancias char(2)/G00, ID manual de tres y paymentid sin asignación registradas para DBA. Sin SP ejecutados, pagos ni registros de Caja. Publicación NotebookLM pendiente. |
| 1.6 | 2026-10-23 | Contraste directo de documento Clover L23, charla y código. CLV-01 a CLV-08 y plan de implementación; confirma requisitos de ID manual, cuotas/pos_plan, recargo y auto finalización Clover. Sin cambios de implementación ni pruebas bancarias; contrato de SP pendiente. Revisión sólo local; publicación NotebookLM pendiente. |
| 1.5 | 2026-10-09 | PAG-08/09: Finalizar sin consulta para cobros electrónicos cargados; captura manual temporal mediante formulario de transferencia, preservando medio e instrumento. 99 escenarios JS simulados, regresiones de NC/documentos/pagos; no se realizaron cobros ni operaciones reales. Integración específica de proveedor y aceptación API/SP pendientes. |
| 1.4 | 2026-10-09 | ADM-07: regreso al inicio tras cerrar el mensaje OK de habilitación/cierre general, solicitado por el responsable; errores e incertidumbre permanecen en Administración. 5 escenarios de estado y 12 flujos de navegación con HTTP simulado aprobados; sin operaciones reales. |
| 1.3 | 2026-10-08 | FAC-16: cantidad previa opcional en Productos sin EAN; conserva recorridos anteriores, avisos y entrada al cancelar/rechazar. 23 escenarios de JavaScript con solicitudes simuladas aprobados. |
| 1.2 | 2026-10-06 | ADM-11: estado desde caja_habilitadas plural; secciones S/N y revalidación servidor. PUE-08: ingreso normal sin selector Z, conservado para caja cerrada. 53 verificaciones de servidor, 5 escenarios UI S/N y 4 regresiones de ingreso; compilación aislada por instancia del usuario en ejecución. |
| 1.1 | 2026-10-05 | Aclaración del analista adoptada por el responsable: Administración distribuida sin apertura; primera etapa apertura/cierre general (ADM-01 a ADM-10); lectura N22 de Sistema GECO, comparación con original, implementación y pruebas aisladas. PUE-06, CIE-01 y UX-02 distinguen cierre de puesto y cierre general. |
| 1.0 | 2026-10-05 | Creación solicitada por el responsable funcional. Consolidación de decisiones de desarrollo, 21 fuentes antecedentes del cuaderno y contratos locales; precedencia, discrepancias, aceptación y pendientes explícitos. |

Para cada cambio futuro: identificar regla por ID, registrar texto anterior/nuevo o regla reemplazada, motivo, decisión y fecha verificable, módulos afectados, pruebas y limitaciones. Actualizar versión y publicar la misma bitácora en NotebookLM. Una propuesta pendiente debe seguir etiquetada P hasta su aprobación; no convertirla en regla confirmada sólo por haber sido agregada al documento.
