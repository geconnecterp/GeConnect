// ════════════════════════════════════════════════════════════
// GESTOR DE MODAL DE CÁLCULO DE FACTURA
// ════════════════════════════════════════════════════════════
// VERSIÓN v6.0 - Integración completa con prodfact.js
// ════════════════════════════════════════════════════════════

// ════════════════════════════════════════════════════════════
// INICIALIZACIÓN DEL MODAL
// ════════════════════════════════════════════════════════════
$(function () {
    console.log('📊 Modal de Cálculo de Factura inicializado v6.0');
    inicializarEventosCalculoFactura();
});

// ════════════════════════════════════════════════════════════
// EVENTOS
// ════════════════════════════════════════════════════════════
function inicializarEventosCalculoFactura() {
    console.log('🔧 Configurando eventos del modal de cálculo...');
    
    // Botón VOLVER
    $('#btnVolverCalculoFactura').on('click', function () {
        console.log('🔙 Volver a carga de productos');
        cerrarModalCalculoFactura();
    });

    // Botón DIFERIR PAGO (en header de tabla)
    $('#btnDiferirPago').on('click', function () {
        console.log('⏱️ Diferir pago');
        mostrarModalDiferirPago();
    });

    // Botón DIFERIR FACTURA (en footer)
    $('#btnDiferirFactura').on('click', function () {
        console.log('💾 Diferir factura completa');
        confirmarDiferirFactura();
    });

    // Botón PAGAR
    $('#btnPagarFactura').on('click', function () {
        console.log('💰 Proceder al pago');
        procesarPagoFactura();
    });
    
    console.log('✅ Eventos del modal de cálculo configurados');
}

// ════════════════════════════════════════════════════════════
// ABRIR MODAL CON DATOS
// ════════════════════════════════════════════════════════════
/**
 * ✅ ACTUALIZADO v8.0: Abre el modal con los datos calculados
 * NUEVO: Hidrata datos del cliente en el header
 * 
 * @param {Object} datosCalculo - Respuesta de CalcularFilas
 * @param {string} datosCalculo.json_subtotal - JSON con subtotales
 * @param {string} datosCalculo.json_sorteo - JSON con sorteos
 * @param {string} datosCalculo.json_p - JSON con productos procesados
 */
function abrirModalCalculoFactura(datosCalculo) {
    console.log('═══════════════════════════════════════════════════');
    console.log('📊 ABRIR MODAL CÁLCULO DE FACTURA v8.0');
    console.log('═══════════════════════════════════════════════════');
    console.log('Datos recibidos:', datosCalculo);

    try {
        // ❶ Parsear JSONs
        const subtotales = parsearJSON(datosCalculo.json_subtotal, 'Subtotales');
        const sorteos = parsearJSON(datosCalculo.json_sorteo, 'Sorteos');
        const productos = parsearJSON(datosCalculo.json_p, 'Productos');

        console.log('📋 Subtotales parseados:', subtotales);
        console.log('🎁 Sorteos parseados:', sorteos);
        console.log('📦 Productos parseados:', productos);

        // ❷ Validar que haya datos
        if (!subtotales || subtotales.length === 0) {
            console.warn('⚠️ No hay subtotales para mostrar');
            mostrarMensajeAdvertencia('No se pudieron calcular los totales correctamente');
            return;
        }

        // ❸ ✅ NUEVO: Hidratar datos del cliente en el header
        hidratarDatosClienteCalculo();

        // ❹ Cargar conceptos en la tabla
        cargarConceptosCalculoFactura(subtotales);

        // ❺ Cargar sorteos
        cargarSorteosCalculoFactura(sorteos);

        // ❻ Abrir modal
        $('#modalCalculoFactura').modal('show');

        console.log('✅ Modal de cálculo abierto correctamente');
        console.log('═══════════════════════════════════════════════════');
    } catch (error) {
        console.error('═══════════════════════════════════════════════════');
        console.error('❌ ERROR AL ABRIR MODAL DE CÁLCULO');
        console.error('═══════════════════════════════════════════════════');
        console.error('Error:', error);
        console.error('Stack:', error.stack);
        
        mostrarMensajeError('Error al cargar los datos de cálculo. Por favor, intente nuevamente.');
    }
}

/**
 * ✅ NUEVO v8.0: Hidrata datos del cliente en el header del modal de cálculo
 * Copia los datos desde el modal de productos (sufijo "Prod") al modal de cálculo (sufijo "Calc")
 */
function hidratarDatosClienteCalculo() {
    console.log('═══════════════════════════════════════════════════');
    console.log('📝 HIDRATAR DATOS DEL CLIENTE EN MODAL CÁLCULO');
    console.log('═══════════════════════════════════════════════════');
    
    // ❶ Mapeo de IDs: Origen (Prod) → Destino (Calc)
    const mapeoIds = {
        'txtClienteNombreProd': 'txtClienteNombreCalc',
        'txtClienteIdProd': 'txtClienteIdCalc',
        'txtClienteDomicilioProd': 'txtClienteDomicilioCalc',
        'txtCondicionAfipProd': 'txtCondicionAfipCalc',
        'txtClienteCuitProd': 'txtClienteCuitCalc',
        'txtClienteEmailProd': 'txtClienteEmailCalc',
        'txtClienteMovilProd': 'txtClienteMovilCalc'
    };
    
    // ❷ Copiar valores de cada campo
    Object.keys(mapeoIds).forEach(function(idOrigen) {
        const idDestino = mapeoIds[idOrigen];
        const valorOrigen = $(`#${idOrigen}`).val() || '';
        
        $(`#${idDestino}`).val(valorOrigen);
        
        console.log(`   ✅ ${idOrigen} → ${idDestino}: "${valorOrigen}"`);
    });
    
    // ❸ Actualizar badge de tipo de comprobante
    const badgeOrigenTexto = $('#badgeTipoComprobante').text().trim();
    const badgeOrigenHtml = $('#badgeTipoComprobante').html();
    
    $('#badgeTipoComprobanteCalc').html(badgeOrigenHtml);
    
    console.log(`   ✅ Badge tipo comprobante: "${badgeOrigenTexto}"`);
    console.log('═══════════════════════════════════════════════════');
    console.log('✅ Datos del cliente hidratados correctamente');
    console.log('═══════════════════════════════════════════════════');
}

/**
 * ✅ NUEVO v6.0: Parsea JSON de forma segura
 * 
 * @param {string} jsonString - String JSON a parsear
 * @param {string} descripcion - Descripción para logs
 * @returns {Array|Object|null} - Datos parseados o null si falla
 */
function parsearJSON(jsonString, descripcion) {
    try {
        if (!jsonString || jsonString.trim() === '') {
            console.warn(`⚠️ ${descripcion}: JSON vacío o nulo`);
            return [];
        }
        
        const datos = JSON.parse(jsonString);
        console.log(`✅ ${descripcion} parseado exitosamente:`, datos);
        
        return datos;
    } catch (error) {
        console.error(`❌ Error parseando ${descripcion}:`, error);
        console.error(`   JSON recibido: "${jsonString}"`);
        
        return [];
    }
}

// ════════════════════════════════════════════════════════════
// CARGAR CONCEPTOS EN TABLA
// ════════════════════════════════════════════════════════════
/**
 * ✅ ACTUALIZADO v7.0: Carga conceptos en tabla con cálculo CORRECTO del total
 * 
 * CAMBIOS CRÍTICOS:
 * - Usa campo "importe" como fuente principal (prioridad sobre "total")
 * - Suma TODOS los importes sin excepciones
 * - El total del footer se calcula INDEPENDIENTEMENTE de las descripciones
 * 
 * Detecta filas de SUBTOTAL y TOTAL para aplicar estilos (solo visual)
 */
function cargarConceptosCalculoFactura(subtotales) {
    console.log('═══════════════════════════════════════════════════');
    console.log('📝 CARGAR CONCEPTOS EN TABLA v7.0');
    console.log('═══════════════════════════════════════════════════');

    const $tbody = $('#tbodyConceptosCalculo');
    $tbody.empty();

    let totalFinal = 0;

    // ❶ Validar que haya datos
    if (!subtotales || subtotales.length === 0) {
        console.warn('⚠️ No hay conceptos para mostrar');
        $tbody.html(`
            <tr>
                <td colspan="2" class="text-center text-muted py-4">
                    <i class='bx bx-error-circle'></i> No hay conceptos para mostrar
                </td>
            </tr>
        `);
        $('#tdTotalFinal').text('$ 0.00');
        return;
    }

    console.log(`📊 Total de conceptos a procesar: ${subtotales.length}`);

    // ❷ Recorrer cada subtotal y generar filas
    subtotales.forEach(function (concepto, index) {
        // ✅ CORREGIDO: Priorizar "importe" sobre "total"
        const descripcion = concepto.concepto || concepto.descripcion || 'Sin descripción';
        const importe = parseFloat(concepto.importe || concepto.total || 0);

        console.log(`   [${index}] ${descripcion}: $ ${importe.toFixed(2)}`);

        // ✅ CRÍTICO: Sumar TODOS los importes sin excepción
        totalFinal += importe;

        // ❸ Determinar clases especiales (SOLO para estilos visuales)
        let rowClass = '';
        const descripcionUpper = descripcion.toUpperCase();

        if (descripcionUpper.includes('SUBTOTAL')) {
            rowClass = 'row-subtotal';
        } else if (descripcionUpper.includes('TOTAL')) {
            rowClass = 'row-total';
        }

        // ❹ Generar HTML de la fila
        const row = `
            <tr class="${rowClass}">
                <td class="text-start">${escapeHtml(descripcion)}</td>
                <td class="text-end">$ ${formatearNumero(importe, 2)}</td>
            </tr>
        `;

        $tbody.append(row);
    });

    // ❺ Actualizar el total final en el footer
    $('#tdTotalFinal').text(`$ ${formatearNumero(totalFinal, 2)}`);

    console.log('═══════════════════════════════════════════════════');
    console.log(`✅ ${subtotales.length} conceptos cargados`);
    console.log(`💰 TOTAL FINAL CALCULADO: $ ${formatearNumero(totalFinal, 2)}`);
    console.log('═══════════════════════════════════════════════════');
}

// ════════════════════════════════════════════════════════════
// CARGAR SORTEOS
// ════════════════════════════════════════════════════════════
/**
 * ✅ ACTUALIZADO v6.0: Carga sorteos en panel lateral
 */
function cargarSorteosCalculoFactura(sorteos) {
    console.log('🎁 Cargando sorteos...');

    const $tbody = $('#tbodySorteos');
    $tbody.empty();

    // ❶ Sin sorteos
    if (!sorteos || sorteos.length === 0) {
        $tbody.html(`
            <tr>
                <td class="text-center text-muted py-4">
                    <i class='bx bx-info-circle'></i> No hay sorteos disponibles
                </td>
            </tr>
        `);
        return;
    }

    // ❷ Recorrer sorteos y generar filas
    sorteos.forEach(function (sorteo) {
        const descripcion = sorteo.descripcion || sorteo.nombre || 'Sorteo sin nombre';
        const detalle = sorteo.detalle || sorteo.observacion || '';

        const row = `
            <tr>
                <td>
                    <div class="d-flex align-items-center">
                        <i class='bx bx-gift text-warning fs-4 me-2'></i>
                        <div>
                            <strong>${escapeHtml(descripcion)}</strong>
                            ${detalle ? `<br><small class="text-muted">${escapeHtml(detalle)}</small>` : ''}
                        </div>
                    </div>
                </td>
            </tr>
        `;

        $tbody.append(row);
    });

    console.log(`✅ ${sorteos.length} sorteos cargados`);
}

// ════════════════════════════════════════════════════════════
// CERRAR MODAL
// ════════════════════════════════════════════════════════════
/**
 * ✅ ACTUALIZADO v6.0: Cierra el modal y limpia datos
 */
function cerrarModalCalculoFactura() {
    console.log('🔙 Cerrando modal de cálculo...');
    
    // Cerrar modal
    $('#modalCalculoFactura').modal('hide');
    
    // Limpiar tablas
    $('#tbodyConceptosCalculo').empty();
    $('#tbodySorteos').empty();
    $('#tdTotalFinal').text('$ 0.00');
    
    console.log('✅ Modal de cálculo cerrado');
}

// ════════════════════════════════════════════════════════════
// FUNCIONES DE NEGOCIO - DIFERIMIENTO
// ════════════════════════════════════════════════════════════

/**
 * ✅ CORREGIDO v9.1: Confirma y ejecuta el diferimiento de factura
 * Usa funciones de mensaje de siteGen.js
 */
function confirmarDiferirFactura() {
    console.log('═══════════════════════════════════════════════════');
    console.log('💾 CONFIRMAR DIFERIR FACTURA v9.1');
    console.log('═══════════════════════════════════════════════════');

    // ❶ Confirmar acción con el cajero
    AbrirMensaje(
        "¿Diferir Factura?",
        `<div class="text-start">
            <p class="mb-2"><i class='bx bx-info-circle text-info'></i> Esta acción creará una <strong>Factura Diferida (Pre-Factura)</strong>:</p>
            <ul class="list-unstyled ms-3">
                <li class="mb-1"><i class='bx bx-check text-success'></i> Se guardará la venta temporalmente</li>
                <li class="mb-1"><i class='bx bx-x text-danger'></i> NO se afectará el stock</li>
                <li class="mb-1"><i class='bx bx-x text-danger'></i> NO se generará comprobante fiscal</li>
                <li class="mb-1"><i class='bx bx-time-five text-warning'></i> El cliente podrá volver más tarde a finalizar la compra</li>
            </ul>
        </div>`,
        function(respuesta) {
            $("#msjModal").modal("hide");
            
            if (respuesta === "SI") {
                setTimeout(() => {
                    ejecutarDiferirFactura();
                }, 300);
            }
        },
        true, // Es confirmación
        ["Sí, Diferir Factura", "Cancelar"],
        "info!",
        null
    );
}

/**
 * ✅ ACTUALIZADO v9.2: Ejecuta la llamada AJAX para diferir factura
 * NUEVO: Flujo de limpieza y reinicio correcto
 */
function ejecutarDiferirFactura() {
    console.log('📡 Invocando /ProductoFact/DiferirFactura...');

    // ❶ Mostrar loader
    AbrirWaiting("Creando Factura Diferida...<br><small class='text-muted'>Por favor espere</small>");

    // ❷ Llamada AJAX
    $.ajax({
        url: DiferirFacturaUrl,
        type: 'POST',
        dataType: 'json',
        timeout: 30000,
        success: function (response) {
            CerrarWaiting();

            console.log('═══════════════════════════════════════════════════');
            console.log('✅ RESPUESTA DE DIFERIR FACTURA v9.2');
            console.log('═══════════════════════════════════════════════════');
            console.log('Response:', response);

            if (!response.ok) {
                console.error('❌ Error en respuesta:', response.mensaje);

                AbrirMensaje(
                    "Error al Diferir Factura",
                    response.mensaje || 'No se pudo crear la factura diferida',
                    function () {
                        $("#msjModal").modal("hide");
                    },
                    false,
                    ["Aceptar"],
                    "error!",
                    null
                );
                return;
            }

            // ✅ ÉXITO
            console.log('✅ Factura diferida creada exitosamente');
            console.log(`   ID: ${response.prefactura_id}`);
            console.log(`   Mensaje: ${response.mensaje}`);

            AbrirMensaje(
                "¡Factura Diferida Creada!",
                `<div class="text-center">
                    <div class="mb-3">
                        <i class='bx bx-check-circle text-success' style="font-size: 4rem;"></i>
                    </div>
                    <h4 class="text-golden mb-3">${response.mensaje}</h4>
                    <p class="text-muted mb-0">El cliente podrá retomar esta compra más tarde</p>
                </div>`,
                function () {
                    $("#msjModal").modal("hide");

                    // ═══════════════════════════════════════════════════
                    // ✅ NUEVO v9.2: FLUJO DE LIMPIEZA Y REINICIO CORRECTO
                    // ═══════════════════════════════════════════════════

                    setTimeout(() => {
                        console.log('═══════════════════════════════════════════════════');
                        console.log('🔄 INICIANDO REINICIO DEL MÓDULO DE VENTAS');
                        console.log('═══════════════════════════════════════════════════');

                        // ❶ PASO 1: Cerrar modal de cálculo
                        cerrarModalCalculoFactura();
                        console.log('✅ Paso 1: Modal de cálculo cerrado');

                        // ❷ PASO 2: Esperar cierre completo del modal (300ms)
                        setTimeout(() => {

                            // ❸ PASO 3: Limpiar completamente el módulo de ventas
                            if (typeof limpiarVentaCompleta === 'function') {
                                limpiarVentaCompleta();
                                console.log('✅ Paso 2: Módulo de ventas limpiado');
                            } else {
                                console.error('❌ Función limpiarVentaCompleta no existe');
                            }

                            // ❹ PASO 4: Esperar limpieza completa (200ms)
                            setTimeout(() => {

                                // ❺ PASO 5: Abrir modal de identificar cliente
                                if (typeof abrirModalIdentificarCliente === 'function') {
                                    abrirModalIdentificarCliente();
                                    console.log('✅ Paso 3: Modal de identificar cliente abierto');
                                } else {
                                    console.error('❌ Función abrirModalIdentificarCliente no existe');
                                }

                                console.log('═══════════════════════════════════════════════════');
                                console.log('✅ REINICIO COMPLETADO - Listo para nueva venta');
                                console.log('═══════════════════════════════════════════════════');

                            }, 200); // ← Esperar limpieza

                        }, 300); // ← Esperar cierre de modal

                    }, 300); // ← Esperar cierre de mensaje de éxito
                },
                false,
                ["Aceptar"],
                "succ!",
                null
            );
        },
        error: function (xhr, status, error) {
            CerrarWaiting();

            console.error('═══════════════════════════════════════════════════');
            console.error('❌ ERROR EN AJAX DIFERIR FACTURA');
            console.error(`   Status: ${status}`);
            console.error(`   Error: ${error}`);
            console.error(`   HTTP Status: ${xhr.status}`);
            console.error('═══════════════════════════════════════════════════');

            if (esSesionExpirada(xhr.status)) {
                manejarSesionExpirada('No se pudo diferir la factura porque su sesión ha expirado.');
                return;
            }

            let mensajeError = 'Error de comunicación con el servidor';

            if (xhr.status === 500) {
                mensajeError = 'Error interno del servidor. Contacte al administrador.';
            } else if (xhr.status === 0) {
                mensajeError = 'No se pudo conectar con el servidor. Verifique su conexión.';
            } else if (xhr.responseJSON && xhr.responseJSON.mensaje) {
                mensajeError = xhr.responseJSON.mensaje;
            }

            AbrirMensaje(
                "Error de Comunicación",
                mensajeError,
                function () {
                    $("#msjModal").modal("hide");
                },
                false,
                ["Aceptar"],
                "error!",
                null
            );
        }
    });
}

/**
 * ✅ CORREGIDO v9.1: Modal de diferir pago con advertencias
 * Emite factura fiscal sin cobrar
 */
function mostrarModalDiferirPago() {
    console.log('═══════════════════════════════════════════════════');
    console.log('⏱️ MOSTRAR MODAL DIFERIR PAGO v9.1');
    console.log('═══════════════════════════════════════════════════');

    // ❶ Confirmar acción con ADVERTENCIAS críticas
    AbrirMensaje(
        "⚠️ ¿Diferir Pago?",
        `<div class="text-start">
            <div class="alert alert-danger mb-3">
                <i class='bx bx-error-alt'></i> <strong>OPERACIÓN DE ALTA COMPLEJIDAD</strong>
            </div>
            <p class="mb-2">Esta acción <strong>EMITIRÁ LA FACTURA FISCAL</strong> pero <strong>SIN COBRAR</strong>:</p>
            <ul class="list-unstyled ms-3">
                <li class="mb-1"><i class='bx bx-check text-success'></i> ✅ Se generará el comprobante fiscal</li>
                <li class="mb-1"><i class='bx bx-check text-success'></i> ✅ Se afectará el stock</li>
                <li class="mb-1"><i class='bx bx-check text-success'></i> ✅ Se registrará en Libro IVA Ventas</li>
                <li class="mb-1"><i class='bx bx-check text-warning'></i> ⚠️ El pago quedará PENDIENTE en cuenta del cliente</li>
                <li class="mb-1"><i class='bx bx-check text-warning'></i> ⚠️ Se imprimirá el comprobante</li>
            </ul>
            <div class="alert alert-warning mt-3 mb-0">
                <i class='bx bx-info-circle'></i> Esta operación NO puede deshacerse fácilmente
            </div>
        </div>`,
        function(respuesta) {
            $("#msjModal").modal("hide");
            
            if (respuesta === "SI") {
                setTimeout(() => {
                    ejecutarDiferirPago();
                }, 300);
            }
        },
        true, // Es confirmación
        ["Sí, Emitir Factura sin Cobrar", "Cancelar"],
        "warn!",
        null
    );
}

/**
 * ✅ ACTUALIZADO v9.2: Ejecuta la llamada AJAX para diferir pago
 * NUEVO: Flujo de limpieza y reinicio correcto
 */
function ejecutarDiferirPago() {
    console.log('📡 Invocando /ProductoFact/DiferirPago...');

    // ❶ Mostrar loader
    AbrirWaiting("Emitiendo Factura con Pago Diferido...<br><small class='text-muted'>Por favor espere, esto puede tardar unos momentos</small>");

    // ❷ Llamada AJAX
    $.ajax({
        url: DiferirPagoUrl,
        type: 'POST',
        dataType: 'json',
        timeout: 30000,
        success: function (response) {
            CerrarWaiting();

            console.log('═══════════════════════════════════════════════════');
            console.log('✅ RESPUESTA DE DIFERIR PAGO v9.2');
            console.log('═══════════════════════════════════════════════════');
            console.log('Response:', response);

            if (!response.ok) {
                console.error('❌ Error en respuesta:', response.mensaje);

                AbrirMensaje(
                    "Error al Diferir Pago",
                    response.mensaje || 'No se pudo emitir la factura',
                    function () {
                        $("#msjModal").modal("hide");
                    },
                    false,
                    ["Aceptar"],
                    "error!",
                    null
                );
                return;
            }

            // ✅ ÉXITO
            console.log('✅ Factura emitida con pago diferido');
            console.log(`   Comprobante: ${response.comprobante.letra} ${response.comprobante.numero}`);
            console.log(`   ID: ${response.comprobante.id}`);
            console.log(`   Identificador completo: ${response.comprobante.identificador_completo}`);

            AbrirMensaje(
                "¡Factura Emitida!",
                `<div class="text-center">
                    <div class="mb-3">
                        <i class='bx bx-receipt text-success' style='font-size: 4rem;'></i>
                    </div>
                    <h4 class="text-golden mb-3">${response.mensaje}</h4>
                    <div class="alert alert-info">
                        <strong>Comprobante ${response.comprobante.letra}</strong><br>
                        Nro: <strong>${response.comprobante.numero}</strong><br>
                        ID: <strong>${response.comprobante.id}</strong>
                    </div>
                    <p class="text-muted mb-0">
                        <i class='bx bx-printer'></i> El comprobante se imprimirá automáticamente
                    </p>
                </div>`,
                function () {
                    $("#msjModal").modal("hide");

                    // ═══════════════════════════════════════════════════
                    // ✅ NUEVO v9.2: FLUJO DE LIMPIEZA Y REINICIO CORRECTO
                    // ═══════════════════════════════════════════════════

                    setTimeout(() => {
                        console.log('═══════════════════════════════════════════════════');
                        console.log('🔄 INICIANDO REINICIO DEL MÓDULO DE VENTAS');
                        console.log('═══════════════════════════════════════════════════');

                        // ❶ DISPARAR IMPRESIÓN DEL COMPROBANTE (si aplica)
                        if (response.debe_imprimir) {
                            console.log('🖨️ Iniciando impresión de comprobante...');
                            imprimirComprobante(response.comprobante);
                        }

                        // ❷ PASO 1: Cerrar modal de cálculo
                        cerrarModalCalculoFactura();
                        console.log('✅ Paso 1: Modal de cálculo cerrado');

                        // ❸ PASO 2: Esperar cierre completo del modal (300ms)
                        setTimeout(() => {

                            // ❹ PASO 3: Limpiar completamente el módulo de ventas
                            if (typeof limpiarVentaCompleta === 'function') {
                                limpiarVentaCompleta();
                                console.log('✅ Paso 2: Módulo de ventas limpiado');
                            } else {
                                console.error('❌ Función limpiarVentaCompleta no existe');
                            }

                            // ❺ PASO 4: Esperar limpieza completa (200ms)
                            setTimeout(() => {

                                // ❻ PASO 5: Abrir modal de identificar cliente
                                if (typeof abrirModalIdentificarCliente === 'function') {
                                    abrirModalIdentificarCliente();
                                    console.log('✅ Paso 3: Modal de identificar cliente abierto');
                                } else {
                                    console.error('❌ Función abrirModalIdentificarCliente no existe');
                                }

                                console.log('═══════════════════════════════════════════════════');
                                console.log('✅ REINICIO COMPLETADO - Listo para nueva venta');
                                console.log('═══════════════════════════════════════════════════');

                            }, 200); // ← Esperar limpieza

                        }, 300); // ← Esperar cierre de modal

                    }, 300); // ← Esperar cierre de mensaje de éxito
                },
                false,
                ["Aceptar"],
                "succ!",
                null
            );
        },
        error: function (xhr, status, error) {
            CerrarWaiting();

            console.error('═══════════════════════════════════════════════════');
            console.error('❌ ERROR EN AJAX DIFERIR PAGO');
            console.error(`   Status: ${status}`);
            console.error(`   Error: ${error}`);
            console.error(`   HTTP Status: ${xhr.status}`);
            console.error('═══════════════════════════════════════════════════');

            if (esSesionExpirada(xhr.status)) {
                manejarSesionExpirada('No se pudo diferir el pago porque su sesión ha expirado.');
                return;
            }

            let mensajeError = 'Error de comunicación con el servidor';

            if (xhr.status === 500) {
                mensajeError = 'Error interno del servidor. Contacte al administrador.';
            } else if (xhr.status === 0) {
                mensajeError = 'No se pudo conectar con el servidor. Verifique su conexión.';
            } else if (xhr.responseJSON && xhr.responseJSON.mensaje) {
                mensajeError = xhr.responseJSON.mensaje;
            }

            AbrirMensaje(
                "Error de Comunicación",
                mensajeError,
                function () {
                    $("#msjModal").modal("hide");
                },
                false,
                ["Aceptar"],
                "error!",
                null
            );
        }
    });
}

/**
 * ✅ ACTUALIZADO v9.1: Imprime el comprobante de venta
 * TODO: Integrar con sistema de impresión fiscal
 * 
 * @param {Object} comprobante - Datos del comprobante a imprimir
 */
function imprimirComprobante(comprobante) {
    console.log('═══════════════════════════════════════════════════');
    console.log('🖨️ IMPRIMIR COMPROBANTE');
    console.log('═══════════════════════════════════════════════════');
    console.log('Datos del comprobante:', comprobante);

    // TODO: Implementar según sistema de impresión fiscal
    // Opciones:
    // 1. Llamar a servicio de impresión fiscal (controlador fiscal)
    // 2. Generar PDF y enviarlo a impresora
    // 3. Enviar a servicio de facturación electrónica

    console.log('⚠️ TODO: Integrar con sistema de impresión fiscal');
    console.log(`   Comprobante: ${comprobante.letra} ${comprobante.numero}`);
    console.log(`   ID: ${comprobante.id}`);
    console.log(`   Identificador: ${comprobante.identificador_completo}`);
    console.log('═══════════════════════════════════════════════════');
    
    // Placeholder: Mostrar alerta de que se debe imprimir
    AbrirMensaje(
        "Imprimir Comprobante",
        `<div class="text-center">
            <i class='bx bx-printer' style='font-size: 3rem;'></i>
            <p class="mt-3">Comprobante ${comprobante.letra} ${comprobante.numero}</p>
            <small class="text-muted">La impresión debe ser implementada</small>
        </div>`,
        function() {
            $("#msjModal").modal("hide");
        },
        false,
        ["Aceptar"],
        "info!",
        null
    );
}

