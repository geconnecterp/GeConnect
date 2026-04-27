// ════════════════════════════════════════════════════════════
// GESTOR DE COTIZACIONES
// ════════════════════════════════════════════════════════════
// VERSIÓN v1.0 - Modal de selección de cotizaciones
// ════════════════════════════════════════════════════════════

// ════════════════════════════════════════════════════════════
// VARIABLES GLOBALES
// ════════════════════════════════════════════════════════════
let cotizacionSeleccionada = null;
let cotizacionesDisponibles = [];

// ════════════════════════════════════════════════════════════
// INICIALIZACIÓN
// ════════════════════════════════════════════════════════════
$(function () {
    console.log('💰 Módulo de Cotizaciones inicializado v1.0');
    inicializarEventosCotizaciones();
});

// ════════════════════════════════════════════════════════════
// EVENTOS
// ════════════════════════════════════════════════════════════
function inicializarEventosCotizaciones() {
    console.log('🔧 Configurando eventos de cotizaciones...');
    
    // Click en fila de la tabla
    $(document).on('click', '#tbodyCotizaciones tr:not(#rowSinCotizaciones)', function () {
        const cpfNro = $(this).data('cpf-nro');
        if (cpfNro) {
            console.log('💰 Cotización seleccionada:', cpfNro);
            seleccionarCotizacion(cpfNro);
        }
    });
    
    // Botón de acción en fila
    $(document).on('click', '.btn-seleccionar-cotizacion', function (e) {
        e.stopPropagation();
        const cpfNro = $(this).closest('tr').data('cpf-nro');
        if (cpfNro) {
            seleccionarCotizacion(cpfNro);
        }
    });
    
    // Botón Cancelar
    $('#btnCancelarCotizacion').on('click', function () {
        console.log('❌ Cancelar selección de cotización');
        cerrarModalCotizaciones();
    });
    
    // Botón Seguir
    $('#btnSeguirCotizacion').on('click', function () {
        console.log('✅ Confirmar cotización seleccionada');
        confirmarCotizacion();
    });
    
    console.log('✅ Eventos de cotizaciones configurados');
}

// ════════════════════════════════════════════════════════════
// ABRIR MODAL
// ════════════════════════════════════════════════════════════
/**
 * ✅ Abre el modal y carga las cotizaciones disponibles
 */
function abrirModalCotizaciones() {
    console.log('═══════════════════════════════════════════════════');
    console.log('💰 ABRIR MODAL COTIZACIONES');
    console.log('═══════════════════════════════════════════════════');
    
    // ❶ Validar que haya cliente seleccionado
    if (!clienteActualFactura) {
        console.error('❌ No hay cliente seleccionado');
        mostrarMensajeError('Debe identificar un cliente antes de cargar una cotización');
        return;
    }
    
    console.log('   Cliente actual:', clienteActualFactura.denominacion);
    
    // ❷ Resetear selección
    cotizacionSeleccionada = null;
    $('#btnSeguirCotizacion').prop('disabled', true);
    
    // ❸ Mostrar modal
    $('#modalCotizaciones').modal('show');
    
    // ❹ Cargar cotizaciones
    cargarCotizaciones();
}

// ════════════════════════════════════════════════════════════
// CARGAR COTIZACIONES
// ════════════════════════════════════════════════════════════
/**
 * ✅ Obtiene las cotizaciones desde el servidor
 */
function cargarCotizaciones() {
    console.log('═══════════════════════════════════════════════════');
    console.log('📡 CARGAR COTIZACIONES DESDE SERVIDOR');
    console.log('═══════════════════════════════════════════════════');
    
    // ❶ Mostrar loader
    $('#tbodyCotizaciones').html(`
        <tr>
            <td colspan="6" class="text-center py-5">
                <i class='bx bx-loader-alt bx-spin bx-lg text-golden'></i>
                <p class="mb-0 mt-2 text-muted">
                    <strong>Cargando cotizaciones...</strong>
                </p>
            </td>
        </tr>
    `);
    
    // ❷ URL del endpoint
    const url = typeof ObtenerCotizacionesUrl !== 'undefined' && ObtenerCotizacionesUrl
        ? ObtenerCotizacionesUrl
        : '/Facturacion/ProductoFact/ObtenerCotizaciones';
    
    // ❸ Llamada AJAX
    $.ajax({
        url: url,
        type: 'POST',
        data: {
            cta_id: clienteActualFactura.id
        },
        dataType: 'json',
        timeout: 15000,
        success: function(response) {
            console.log('✅ Cotizaciones recibidas:', response);
            
            if (!response.ok) {
                mostrarErrorCargarCotizaciones(response.mensaje || 'Error al cargar cotizaciones');
                return;
            }
            
            if (!response.cotizaciones || response.cotizaciones.length === 0) {
                mostrarSinCotizaciones();
                return;
            }
            
            // ❹ Guardar y renderizar
            cotizacionesDisponibles = response.cotizaciones;
            renderizarCotizaciones(response.cotizaciones);
        },
        error: function(xhr, status, error) {
            console.error('❌ Error al cargar cotizaciones:', error);
            
            // Usar interceptor de sesiones
            if (esSesionExpirada(xhr.status)) {
                return;
            }
            
            mostrarErrorCargarCotizaciones('Error de comunicación con el servidor');
        }
    });
}

/**
 * ✅ Renderiza las cotizaciones en la tabla
 */
function renderizarCotizaciones(cotizaciones) {
    console.log('📝 Renderizando cotizaciones...');
    console.log(`   Total: ${cotizaciones.length}`);
    
    const $tbody = $('#tbodyCotizaciones');
    $tbody.empty();
    
    cotizaciones.forEach(function(cot, index) {
        const cpfNro = cot.cpf_nro || '';
        const descripcion = cot.cpf_descripcion || 'Sin descripción';
        const fecha = cot.cpf_fecha || '';
        const obsPago = cot.obs_pago || '';
        const importe = parseFloat(cot.cpf_importe || 0);
        
        const row = `
            <tr data-cpf-nro="${cpfNro}" 
                data-index="${index}"
                class="cotizacion-row">
                <td class="text-center fw-bold">${escapeHtml(cpfNro)}</td>
                <td>${escapeHtml(descripcion)}</td>
                <td class="text-center">${escapeHtml(fecha)}</td>
                <td>${escapeHtml(obsPago)}</td>
                <td class="text-end fw-semibold">$ ${formatearNumero(importe, 2)}</td>
                <td class="text-center">
                    <button type="button" 
                            class="btn btn-sm btn-success btn-seleccionar-cotizacion"
                            title="Seleccionar esta cotización">
                        <i class='bx bx-check-circle'></i>
                    </button>
                </td>
            </tr>
        `;
        
        $tbody.append(row);
    });
    
    console.log('✅ Cotizaciones renderizadas');
}

/**
 * ✅ Muestra mensaje cuando no hay cotizaciones
 */
function mostrarSinCotizaciones() {
    console.log('ℹ️ No hay cotizaciones disponibles');
    
    $('#tbodyCotizaciones').html(`
        <tr id="rowSinCotizaciones">
            <td colspan="6" class="text-center text-muted py-5">
                <i class='bx bx-dollar-circle bx-lg text-golden'></i>
                <p class="mb-0 mt-2">
                    <strong>No hay cotizaciones disponibles</strong><br>
                    <small>Verifique que existan cotizaciones para este cliente</small>
                </p>
            </td>
        </tr>
    `);
}

/**
 * ✅ Muestra error al cargar cotizaciones
 */
function mostrarErrorCargarCotizaciones(mensaje) {
    console.error('❌ Error al cargar cotizaciones:', mensaje);
    
    $('#tbodyCotizaciones').html(`
        <tr>
            <td colspan="6">
                <div class="alert alert-danger m-3">
                    <i class='bx bx-error-circle'></i> ${escapeHtml(mensaje)}
                </div>
            </td>
        </tr>
    `);
}

// ════════════════════════════════════════════════════════════
// SELECCIÓN DE COTIZACIÓN
// ════════════════════════════════════════════════════════════
/**
 * ✅ Selecciona una cotización
 */
function seleccionarCotizacion(cpfNro) {
    console.log(`💰 Seleccionar cotización: ${cpfNro}`);
    
    // ❶ Remover selecciones anteriores
    $('#tbodyCotizaciones tr').removeClass('selected-cotizacion');
    
    // ❷ Marcar como seleccionada
    const $row = $(`#tbodyCotizaciones tr[data-cpf-nro="${cpfNro}"]`);
    $row.addClass('selected-cotizacion');
    
    // ❸ Buscar datos completos
    const cotizacion = cotizacionesDisponibles.find(c => c.cpf_nro === cpfNro);
    
    if (!cotizacion) {
        console.error('❌ Cotización no encontrada en el array');
        return;
    }
    
    // ❹ Guardar selección
    cotizacionSeleccionada = cotizacion;
    
    // ❺ Habilitar botón Seguir
    $('#btnSeguirCotizacion').prop('disabled', false);
    
    console.log('✅ Cotización seleccionada:', cotizacionSeleccionada);
}

// ════════════════════════════════════════════════════════════
// CONFIRMAR COTIZACIÓN
// ════════════════════════════════════════════════════════════
/**
 * ✅ Confirma la cotización seleccionada y la carga
 */
function confirmarCotizacion() {
    console.log('═══════════════════════════════════════════════════');
    console.log('✅ CONFIRMAR COTIZACIÓN SELECCIONADA');
    console.log('═══════════════════════════════════════════════════');
    
    if (!cotizacionSeleccionada) {
        console.error('❌ No hay cotización seleccionada');
        mostrarMensajeError('Debe seleccionar una cotización');
        return;
    }
    
    console.log('   Cotización:', cotizacionSeleccionada.cpf_nro);
    
    // ❶ Cerrar modal
    cerrarModalCotizaciones();
    
    // ❷ Cargar cotización mediante la función existente en prodfact.js
    // Usar el mismo flujo que el botón "Cotización" del modal de productos
    buscarProductoPorCodigo(
        'C',                                  // tipoValor = C (Cotización)
        cotizacionSeleccionada.cpf_nro,       // valor = Número de cotización
        1,                                     // cantidad
        true,                                  // bulto
        'cotizacion'                           // origen de carga
    );
}

// ════════════════════════════════════════════════════════════
// CERRAR MODAL
// ════════════════════════════════════════════════════════════
/**
 * ✅ Cierra el modal y limpia datos
 */
function cerrarModalCotizaciones() {
    console.log('🔙 Cerrando modal de cotizaciones...');
    
    // Cerrar modal
    $('#modalCotizaciones').modal('hide');
    
    // Limpiar datos
    cotizacionSeleccionada = null;
    cotizacionesDisponibles = [];
    
    // Restaurar botones
    $('#btnSeguirCotizacion').prop('disabled', true);
    
    console.log('✅ Modal cerrado');
}
