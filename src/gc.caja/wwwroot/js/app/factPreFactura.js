// ════════════════════════════════════════════════════════════
// GESTOR DE PRE-FACTURAS
// ════════════════════════════════════════════════════════════
// VERSIÓN v1.0 - Modal de selección de pre-facturas
// ════════════════════════════════════════════════════════════

// ════════════════════════════════════════════════════════════
// VARIABLES GLOBALES
// ════════════════════════════════════════════════════════════
let preFacturaSeleccionada = null;
let preFacturasDisponibles = [];

// ════════════════════════════════════════════════════════════
// INICIALIZACIÓN
// ════════════════════════════════════════════════════════════
$(function () {
    console.log('📄 Módulo de Pre-Facturas inicializado v1.0');
    inicializarEventosPreFacturas();
});

// ════════════════════════════════════════════════════════════
// EVENTOS
// ════════════════════════════════════════════════════════════
function inicializarEventosPreFacturas() {
    console.log('🔧 Configurando eventos de pre-facturas...');
    
    // Checkbox "Solo Pendientes"
    $('#chkSoloPendientes').on('change', function () {
        console.log('🔄 Filtro Solo Pendientes:', $(this).is(':checked'));
        cargarPreFacturas();
    });
    
    // Checkbox "Seleccionar Todos"
    $('#chkSeleccionarTodos').on('change', function () {
        const checked = $(this).is(':checked');
        console.log('☑️ Seleccionar todos:', checked);
        toggleSeleccionarTodos(checked);
    });
    
    // Click en fila de la tabla
    $(document).on('click', '#tbodyPreFacturas tr:not(#rowSinPreFacturas)', function () {
        const preId = $(this).data('pre-id');
        if (preId) {
            console.log('📋 Pre-factura seleccionada:', preId);
            seleccionarPreFactura(preId);
        }
    });
    
    // Checkbox individual de fila
    $(document).on('change', '#tbodyPreFacturas .chk-prefactura', function (e) {
        e.stopPropagation();
        const preId = $(this).closest('tr').data('pre-id');
        const checked = $(this).is(':checked');
        
        console.log(`☑️ Checkbox fila ${preId}:`, checked);
        
        if (checked) {
            seleccionarPreFactura(preId);
        } else {
            deseleccionarPreFactura();
        }
    });
    
    // Botón Cancelar
    $('#btnCancelarPreFactura').on('click', function () {
        console.log('❌ Cancelar selección de pre-factura');
        cerrarModalPreFacturas();
    });
    
    // Botón Seguir
    $('#btnSeguirPreFactura').on('click', function () {
        console.log('✅ Confirmar pre-factura seleccionada');
        confirmarPreFactura();
    });
    
    console.log('✅ Eventos de pre-facturas configurados');
}

// ════════════════════════════════════════════════════════════
// ABRIR MODAL
// ════════════════════════════════════════════════════════════
/**
 * ✅ Abre el modal y carga las pre-facturas disponibles
 */
function abrirModalPreFacturas() {
    console.log('═══════════════════════════════════════════════════');
    console.log('📄 ABRIR MODAL PRE-FACTURAS');
    console.log('═══════════════════════════════════════════════════');
    
    // ❶ Validar que haya cliente seleccionado
    if (!clienteActualFactura) {
        console.error('❌ No hay cliente seleccionado');
        mostrarMensajeError('Debe identificar un cliente antes de cargar una pre-factura');
        return;
    }
    
    console.log('   Cliente actual:', clienteActualFactura.denominacion);
    
    // ❷ Resetear selección
    preFacturaSeleccionada = null;
    $('#btnSeguirPreFactura').prop('disabled', true);
    $('#chkSeleccionarTodos').prop('checked', false);
    
    // ❸ Mostrar modal
    $('#modalPreFacturas').modal('show');
    
    // ❹ Cargar pre-facturas
    cargarPreFacturas();
}

// ════════════════════════════════════════════════════════════
// CARGAR PRE-FACTURAS
// ════════════════════════════════════════════════════════════
/**
 * ✅ Obtiene las pre-facturas desde el servidor
 */
function cargarPreFacturas() {
    console.log('═══════════════════════════════════════════════════');
    console.log('📡 CARGAR PRE-FACTURAS DESDE SERVIDOR');
    console.log('═══════════════════════════════════════════════════');
    
    // ❶ Obtener filtro
    const soloPendientes = $('#chkSoloPendientes').is(':checked');
    console.log(`   Filtro: Solo Pendientes = ${soloPendientes}`);
    
    // ❷ Mostrar loader
    $('#tbodyPreFacturas').html(`
        <tr>
            <td colspan="7" class="text-center py-5">
                <i class='bx bx-loader-alt bx-spin bx-lg text-golden'></i>
                <p class="mb-0 mt-2 text-muted">
                    <strong>Cargando pre-facturas...</strong>
                </p>
            </td>
        </tr>
    `);
    
    // ❸ URL del endpoint
    const url = typeof ObtenerPreFacturasUrl !== 'undefined' && ObtenerPreFacturasUrl
        ? ObtenerPreFacturasUrl
        : '/Facturacion/ProductoFact/ObtenerPreFacturas';
    
    // ❹ Llamada AJAX
    $.ajax({
        url: url,
        type: 'POST',
        data: {
            cta_id: clienteActualFactura.id,
            solo_pendientes: soloPendientes
        },
        dataType: 'json',
        timeout: 15000,
        success: function(response) {
            console.log('✅ Pre-facturas recibidas:', response);
            
            if (!response.ok) {
                mostrarErrorCargarPreFacturas(response.mensaje || 'Error al cargar pre-facturas');
                return;
            }
            
            if (!response.prefacturas || response.prefacturas.length === 0) {
                mostrarSinPreFacturas();
                return;
            }
            
            // ❺ Guardar y renderizar
            preFacturasDisponibles = response.prefacturas;
            renderizarPreFacturas(response.prefacturas);
        },
        error: function(xhr, status, error) {
            console.error('❌ Error al cargar pre-facturas:', error);
            
            // Usar interceptor de sesiones
            if (esSesionExpirada(xhr.status)) {
                return;
            }
            
            mostrarErrorCargarPreFacturas('Error de comunicación con el servidor');
        }
    });
}

/**
 * ✅ Renderiza las pre-facturas en la tabla
 */
function renderizarPreFacturas(prefacturas) {
    console.log('📝 Renderizando pre-facturas...');
    console.log(`   Total: ${prefacturas.length}`);
    
    const $tbody = $('#tbodyPreFacturas');
    $tbody.empty();
    
    prefacturas.forEach(function(pf, index) {
        const preId = pf.pre_id || '';
        const cliente = pf.cta_denominacion || 'Sin nombre';
        const documento = pf.cta_documento || '';
        const fecha = pf.pre_fecha || '';
        const sector = pf.sector_desc || '';
        
        const row = `
            <tr data-pre-id="${preId}" 
                data-index="${index}"
                class="prefactura-row">
                <td class="text-center fw-bold">${escapeHtml(preId)}</td>
                <td>${escapeHtml(cliente)}</td>
                <td class="text-center">${escapeHtml(documento)}</td>
                <td class="text-center">${escapeHtml(fecha)}</td>
                <td>${escapeHtml(sector)}</td>
                <td class="text-center">
                    <input type="checkbox" 
                           class="form-check-input chk-prefactura"
                           data-pre-id="${preId}">
                </td>
                <td class="text-center">
                    <button type="button" 
                            class="btn btn-sm btn-success"
                            onclick="seleccionarPreFactura('${preId}')"
                            title="Seleccionar esta pre-factura">
                        <i class='bx bx-check-circle'></i>
                    </button>
                </td>
            </tr>
        `;
        
        $tbody.append(row);
    });
    
    console.log('✅ Pre-facturas renderizadas');
}

/**
 * ✅ Muestra mensaje cuando no hay pre-facturas
 */
function mostrarSinPreFacturas() {
    console.log('ℹ️ No hay pre-facturas disponibles');
    
    $('#tbodyPreFacturas').html(`
        <tr id="rowSinPreFacturas">
            <td colspan="7" class="text-center text-muted py-5">
                <i class='bx bx-file-blank bx-lg text-golden'></i>
                <p class="mb-0 mt-2">
                    <strong>No hay pre-facturas disponibles</strong><br>
                    <small>Cambie el filtro o verifique que existan pre-facturas pendientes</small>
                </p>
            </td>
        </tr>
    `);
}

/**
 * ✅ Muestra error al cargar pre-facturas
 */
function mostrarErrorCargarPreFacturas(mensaje) {
    console.error('❌ Error al cargar pre-facturas:', mensaje);
    
    $('#tbodyPreFacturas').html(`
        <tr>
            <td colspan="7">
                <div class="alert alert-danger m-3">
                    <i class='bx bx-error-circle'></i> ${escapeHtml(mensaje)}
                </div>
            </td>
        </tr>
    `);
}

// ════════════════════════════════════════════════════════════
// SELECCIÓN DE PRE-FACTURA
// ════════════════════════════════════════════════════════════
/**
 * ✅ Selecciona una pre-factura
 */
function seleccionarPreFactura(preId) {
    console.log(`📋 Seleccionar pre-factura: ${preId}`);
    
    // ❶ Remover selecciones anteriores
    $('#tbodyPreFacturas tr').removeClass('selected-prefactura');
    $('#tbodyPreFacturas .chk-prefactura').prop('checked', false);
    
    // ❷ Marcar como seleccionada
    const $row = $(`#tbodyPreFacturas tr[data-pre-id="${preId}"]`);
    $row.addClass('selected-prefactura');
    $row.find('.chk-prefactura').prop('checked', true);
    
    // ❸ Buscar datos completos
    const prefactura = preFacturasDisponibles.find(pf => pf.pre_id === preId);
    
    if (!prefactura) {
        console.error('❌ Pre-factura no encontrada en el array');
        return;
    }
    
    // ❹ Guardar selección
    preFacturaSeleccionada = prefactura;
    
    // ❺ Habilitar botón Seguir
    $('#btnSeguirPreFactura').prop('disabled', false);
    
    console.log('✅ Pre-factura seleccionada:', preFacturaSeleccionada);
}

/**
 * ✅ Deselecciona la pre-factura actual
 */
function deseleccionarPreFactura() {
    console.log('🔄 Deseleccionar pre-factura');
    
    $('#tbodyPreFacturas tr').removeClass('selected-prefactura');
    $('#tbodyPreFacturas .chk-prefactura').prop('checked', false);
    preFacturaSeleccionada = null;
    $('#btnSeguirPreFactura').prop('disabled', true);
}

/**
 * ✅ Toggle seleccionar/deseleccionar todos
 */
function toggleSeleccionarTodos(checked) {
    // ⚠️ En este caso, "seleccionar todos" no tiene sentido
    // porque solo se puede cargar UNA pre-factura a la vez
    // Dejamos la funcionalidad deshabilitada
    
    console.warn('⚠️ Seleccionar todos no implementado (solo se puede seleccionar una pre-factura)');
    $('#chkSeleccionarTodos').prop('checked', false);
}

// ════════════════════════════════════════════════════════════
// CONFIRMAR PRE-FACTURA
// ════════════════════════════════════════════════════════════
/**
 * ✅ Confirma la pre-factura seleccionada y la carga
 */
function confirmarPreFactura() {
    console.log('═══════════════════════════════════════════════════');
    console.log('✅ CONFIRMAR PRE-FACTURA SELECCIONADA');
    console.log('═══════════════════════════════════════════════════');
    
    if (!preFacturaSeleccionada) {
        console.error('❌ No hay pre-factura seleccionada');
        mostrarMensajeError('Debe seleccionar una pre-factura');
        return;
    }
    
    console.log('   Pre-factura:', preFacturaSeleccionada.pre_id);
    
    // ❶ Cerrar modal
    cerrarModalPreFacturas();
    
    // ❷ Cargar pre-factura mediante la función existente en prodfact.js
    // Usar el mismo flujo que el botón "Pre-Factura" del modal de productos
    buscarProductoPorCodigo(
        'F',                              // tipoValor = F (Pre-Factura)
        preFacturaSeleccionada.pre_id,    // valor = ID de pre-factura
        1,                                 // cantidad
        true,                              // bulto
        'prefactura'                       // origen de carga
    );
}

// ════════════════════════════════════════════════════════════
// CERRAR MODAL
// ════════════════════════════════════════════════════════════
/**
 * ✅ Cierra el modal y limpia datos
 */
function cerrarModalPreFacturas() {
    console.log('🔙 Cerrando modal de pre-facturas...');
    
    // Cerrar modal
    $('#modalPreFacturas').modal('hide');
    
    // Limpiar datos
    preFacturaSeleccionada = null;
    preFacturasDisponibles = [];
    
    // Restaurar botones
    $('#btnSeguirPreFactura').prop('disabled', true);
    $('#chkSeleccionarTodos').prop('checked', false);
    
    console.log('✅ Modal cerrado');
}

//// ════════════════════════════════════════════════════════════
//// HELPERS
//// ════════════════════════════════════════════════════════════

//function escapeHtml(texto) {
//    if (typeof window.escapeHtml === 'function') {
//        return window.escapeHtml(texto);
//    }
//    if (!texto) return '';
//    const map = { '&': '&amp;', '<': '&lt;', '>': '&gt;', '"': '&quot;', "'": '&#039;' };
//    return texto.replace(/[&<>"']/g, m => map[m]);
//}

//function mostrarMensajeError(mensaje) {
//    if (typeof window.mostrarMensajeError === 'function') {
//        window.mostrarMensajeError(mensaje);
//    } else {
//        console.error('💬 Error:', mensaje);
//        AbrirMensaje("Error", mensaje, function () {
//            $("#msjModal").modal("hide")´¿
//        }, false, ["Aceptar"], "error!", null);
//    }
//}