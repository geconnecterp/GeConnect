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
// FUNCIONES DE NEGOCIO (TODO: Implementar)
// ════════════════════════════════════════════════════════════

/**
 * ⚠️ TODO: Implementar modal de diferir pago
 */
function mostrarModalDiferirPago() {
    console.log('⚠️ TODO: Implementar modal de diferir pago');
    mostrarMensajeAdvertencia('Funcionalidad de diferir pago en desarrollo');
}

/**
 * ⚠️ TODO: Implementar confirmación de diferir factura
 */
function confirmarDiferirFactura() {
    console.log('⚠️ TODO: Implementar confirmación de diferir factura');
    mostrarMensajeAdvertencia('Funcionalidad de diferir factura en desarrollo');
}

/**
 * ✅ NUEVO v6.0: Procesa el pago de la factura
 * 
 * TODO: Abrir siguiente modal (Pago)
 */
function procesarPagoFactura() {
    console.log('═══════════════════════════════════════════════════');
    console.log('💰 PROCESAR PAGO DE FACTURA');
    console.log('═══════════════════════════════════════════════════');
    
    // TODO: Abrir modal de medios de pago
    console.log('⚠️ TODO: Abrir modal de medios de pago');
    mostrarMensajeAdvertencia('Funcionalidad de pago en desarrollo');
    
    // Ejemplo de lo que vendría:
    // cerrarModalCalculoFactura();
    // abrirModalMediosPago(datosFactura);
}

