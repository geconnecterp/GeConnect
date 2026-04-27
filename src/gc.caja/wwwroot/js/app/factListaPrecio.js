// ════════════════════════════════════════════════════════════
// GESTOR DE CAMBIO DE LISTA DE PRECIOS
// ════════════════════════════════════════════════════════════
// VERSIÓN v1.0 - Modal de cambio de lista de precios
// ════════════════════════════════════════════════════════════

// ════════════════════════════════════════════════════════════
// VARIABLES GLOBALES
// ════════════════════════════════════════════════════════════
let listaPreciosSeleccionada = null;
let listaPreciosDisponibles = [];

// ════════════════════════════════════════════════════════════
// INICIALIZACIÓN
// ════════════════════════════════════════════════════════════
$(function () {
    console.log('📋 Módulo de Lista de Precios inicializado v1.0');
    inicializarEventosListaPrecios();
});

// ════════════════════════════════════════════════════════════
// EVENTOS
// ════════════════════════════════════════════════════════════
function inicializarEventosListaPrecios() {
    console.log('🔧 Configurando eventos de lista de precios...');
    
    // Botón abrir modal (desde modal identificar cliente)
    $('#btnListaPrecios').on('click', function () {
        console.log('📋 Abrir modal cambiar lista de precios...');
        abrirModalListaPrecios();
    });
    
    // Select de lista de precios
    $('#selListaPreciosModal').on('change', function () {
        const lpId = $(this).val();
        console.log(`📋 Lista seleccionada en dropdown: ${lpId}`);
        seleccionarListaPrecioEnListbox(lpId);
    });
    
    // Click en item del listbox
    $(document).on('click', '#listboxListasPrecios .list-group-item:not(:disabled)', function () {
        const lpId = $(this).data('lp-id');
        console.log(`📋 Lista seleccionada en listbox: ${lpId}`);
        seleccionarListaPrecio(lpId);
    });
    
    // Botón Cancelar
    $('#btnCancelarCambiarLP').on('click', function () {
        console.log('❌ Cancelar cambio de lista de precios');
        cerrarModalListaPrecios();
    });
    
    // Botón Confirmar
    $('#btnConfirmarCambiarLP').on('click', function () {
        console.log('✅ Confirmar cambio de lista de precios');
        confirmarCambioListaPrecios();
    });
    
    console.log('✅ Eventos de lista de precios configurados');
}

// ════════════════════════════════════════════════════════════
// ABRIR MODAL
// ════════════════════════════════════════════════════════════
/**
 * ✅ Abre el modal y carga las listas de precios disponibles
 */
function abrirModalListaPrecios() {
    console.log('═══════════════════════════════════════════════════');
    console.log('📋 ABRIR MODAL CAMBIAR LISTA DE PRECIOS');
    console.log('═══════════════════════════════════════════════════');
    
    // ❶ Resetear selección
    listaPreciosSeleccionada = null;
    $('#btnConfirmarCambiarLP').prop('disabled', true);
    
    // ❷ Mostrar modal
    $('#modalCambiarListaPrecios').modal('show');
    
    // ❸ Cargar listas de precios
    cargarListasPrecios();
}

// ════════════════════════════════════════════════════════════
// CARGAR LISTAS DE PRECIOS
// ════════════════════════════════════════════════════════════
/**
 * ✅ Obtiene las listas de precios desde el servidor
 */
function cargarListasPrecios() {
    console.log('📡 Cargando listas de precios desde servidor...');
    
    // ❶ Mostrar loader
    $('#listboxListasPrecios').html(`
        <button type="button" 
                class="list-group-item list-group-item-action" 
                disabled>
            <i class='bx bx-loader-alt bx-spin'></i> Cargando listas de precios...
        </button>
    `);
    
    $('#selListaPreciosModal').prop('disabled', true);
    
    // ❷ URL del endpoint (debe existir en el controlador)
    const url = typeof ObtenerListasPreciosUrl !== 'undefined' && ObtenerListasPreciosUrl
        ? ObtenerListasPreciosUrl
        : '/Facturacion/ProductoFact/ObtenerListasPrecios';
    
    // ❸ Llamada AJAX
    $.ajax({
        url: url,
        type: 'POST',
        dataType: 'json',
        timeout: 10000,
        success: function(response) {
            console.log('✅ Listas de precios recibidas:', response);
            
            if (!response.ok || !response.listas || response.listas.length === 0) {
                mostrarErrorCargarListas(response.mensaje || 'No hay listas de precios disponibles');
                return;
            }
            
            // ❹ Guardar listas
            listaPreciosDisponibles = response.listas;
            
            // ❺ Renderizar listas
            renderizarListasPrecios(response.listas, response.lp_actual);
        },
        error: function(xhr, status, error) {
            console.error('❌ Error al cargar listas de precios:', error);
            
            // Usar interceptor de sesiones de siteGen.js
            if (esSesionExpirada(xhr.status)) {
                return; // El interceptor se encargará
            }
            
            mostrarErrorCargarListas('Error al cargar las listas de precios');
        }
    });
}

/**
 * ✅ Renderiza las listas de precios en el select y listbox
 */
function renderizarListasPrecios(listas, lpActual) {
    console.log('📝 Renderizando listas de precios...');
    console.log(`   Total listas: ${listas.length}`);
    console.log(`   Lista actual: ${lpActual}`);
    
    // ❶ Limpiar select
    const $select = $('#selListaPreciosModal');
    $select.empty().append('<option value="">-- Seleccione una lista --</option>');
    
    // ❷ Limpiar listbox
    const $listbox = $('#listboxListasPrecios');
    $listbox.empty();
    
    // ❸ Recorrer listas y generar HTML
    listas.forEach(function(lista) {
        const lpId = lista.lp_id || '';
        const lpDesc = lista.lp_desc || 'Sin descripción';
        const esActual = lpId === lpActual;
        
        // Agregar al select
        const optionHtml = `<option value="${lpId}">${lpDesc}</option>`;
        $select.append(optionHtml);
        
        // Agregar al listbox
        const itemHtml = `
            <button type="button" 
                    class="list-group-item list-group-item-action d-flex justify-content-between align-items-center ${esActual ? 'active' : ''}"
                    data-lp-id="${lpId}"
                    data-lp-desc="${escapeHtml(lpDesc)}">
                <span>
                    <i class='bx ${esActual ? 'bx-check-circle' : 'bx-purchase-tag'}'></i>
                    ${escapeHtml(lpDesc)}
                </span>
                <span class="badge-lp-codigo">${lpId}</span>
            </button>
        `;
        $listbox.append(itemHtml);
    });
    
    // ❹ Habilitar select
    $select.prop('disabled', false);
    
    // ❺ Si hay una lista actual, seleccionarla
    if (lpActual) {
        seleccionarListaPrecio(lpActual);
    }
    
    console.log('✅ Listas de precios renderizadas');
}

/**
 * ✅ Muestra error al cargar listas
 */
function mostrarErrorCargarListas(mensaje) {
    console.error('❌ Error al cargar listas:', mensaje);
    
    $('#listboxListasPrecios').html(`
        <div class="alert alert-danger m-2">
            <i class='bx bx-error-circle'></i> ${mensaje}
        </div>
    `);
    
    $('#selListaPreciosModal').prop('disabled', true);
}

// ════════════════════════════════════════════════════════════
// SELECCIÓN DE LISTA DE PRECIOS
// ════════════════════════════════════════════════════════════
/**
 * ✅ Selecciona una lista de precios
 */
function seleccionarListaPrecio(lpId) {
    console.log(`📋 Seleccionando lista de precios: ${lpId}`);
    
    // ❶ Remover selección anterior
    $('#listboxListasPrecios .list-group-item').removeClass('active');
    
    // ❷ Marcar como seleccionada
    const $item = $(`#listboxListasPrecios .list-group-item[data-lp-id="${lpId}"]`);
    $item.addClass('active');
    
    // ❸ Actualizar select
    $('#selListaPreciosModal').val(lpId);
    
    // ❹ Guardar selección
    const lpDesc = $item.data('lp-desc') || '';
    listaPreciosSeleccionada = {
        lp_id: lpId,
        lp_desc: lpDesc
    };
    
    // ❺ Habilitar botón confirmar
    $('#btnConfirmarCambiarLP').prop('disabled', false);
    
    console.log('✅ Lista seleccionada:', listaPreciosSeleccionada);
}

/**
 * ✅ Selecciona lista desde el dropdown
 */
function seleccionarListaPrecioEnListbox(lpId) {
    if (!lpId) {
        $('#listboxListasPrecios .list-group-item').removeClass('active');
        listaPreciosSeleccionada = null;
        $('#btnConfirmarCambiarLP').prop('disabled', true);
        return;
    }
    
    seleccionarListaPrecio(lpId);
}

// ════════════════════════════════════════════════════════════
// CONFIRMAR CAMBIO
// ════════════════════════════════════════════════════════════
/**
 * ✅ Confirma el cambio de lista de precios
 */
function confirmarCambioListaPrecios() {
    console.log('═══════════════════════════════════════════════════');
    console.log('✅ CONFIRMAR CAMBIO DE LISTA DE PRECIOS');
    console.log('═══════════════════════════════════════════════════');
    
    if (!listaPreciosSeleccionada) {
        console.error('❌ No hay lista seleccionada');
        mostrarMensajeError('Debe seleccionar una lista de precios');
        return;
    }
    
    console.log('   Lista seleccionada:', listaPreciosSeleccionada);
    
    // ❶ Mostrar confirmación
    AbrirMensaje(
        "Confirmar Cambio",
        `¿Está seguro que desea cambiar a la lista de precios:<br><br>` +
        `<strong>${listaPreciosSeleccionada.lp_desc}</strong> (${listaPreciosSeleccionada.lp_id})?<br><br>` +
        `<small class="text-muted">Los precios de los productos se actualizarán con esta lista.</small>`,
        function () {
            $("#msjModal").modal("hide");
            ejecutarCambioListaPrecios();
        },
        true,
        ["Sí, cambiar", "Cancelar"],
        "warn!",
        null
    );
}

/**
 * ✅ Ejecuta el cambio de lista de precios
 */
function ejecutarCambioListaPrecios() {
    console.log('📡 Ejecutando cambio de lista de precios...');
    
    // ❶ Deshabilitar botones
    $('#btnConfirmarCambiarLP').prop('disabled', true).html('<i class="bx bx-loader-alt bx-spin"></i> Cambiando...');
    $('#btnCancelarCambiarLP').prop('disabled', true);
    
    // ❷ URL del endpoint
    const url = typeof CambiarListaPreciosUrl !== 'undefined' && CambiarListaPreciosUrl
        ? CambiarListaPreciosUrl
        : '/Facturacion/ProductoFact/CambiarListaPrecios';
    
    // ❸ Llamada AJAX
    $.ajax({
        url: url,
        type: 'POST',
        data: {
            lp_id: listaPreciosSeleccionada.lp_id
        },
        success: function(response) {
            console.log('✅ Respuesta del servidor:', response);
            
            if (response.ok) {
                // Cerrar modal
                cerrarModalListaPrecios();
                
                // Mostrar mensaje de éxito
                mostrarMensajeExito(
                    `Lista de precios cambiada exitosamente a:<br>` +
                    `<strong>${listaPreciosSeleccionada.lp_desc}</strong>`
                );
                
                // TODO: Actualizar variable global LP_Id si existe
                if (typeof window.LP_Id !== 'undefined') {
                    window.LP_Id = listaPreciosSeleccionada.lp_id;
                }
            } else {
                mostrarMensajeError(response.mensaje || 'Error al cambiar lista de precios');
                $('#btnConfirmarCambiarLP').prop('disabled', false).html('<i class="bx bx-check-circle"></i> Cambiar LP');
                $('#btnCancelarCambiarLP').prop('disabled', false);
            }
        },
        error: function(xhr, status, error) {
            console.error('❌ Error al cambiar lista de precios:', error);
            
            // Usar interceptor de sesiones
            if (esSesionExpirada(xhr.status)) {
                return;
            }
            
            mostrarMensajeError('Error al cambiar la lista de precios');
            $('#btnConfirmarCambiarLP').prop('disabled', false).html('<i class="bx bx-check-circle"></i> Cambiar LP');
            $('#btnCancelarCambiarLP').prop('disabled', false);
        }
    });
}

// ════════════════════════════════════════════════════════════
// CERRAR MODAL
// ════════════════════════════════════════════════════════════
/**
 * ✅ Cierra el modal y limpia datos
 */
function cerrarModalListaPrecios() {
    console.log('🔙 Cerrando modal lista de precios...');
    
    // Cerrar modal
    $('#modalCambiarListaPrecios').modal('hide');
    
    // Limpiar datos
    listaPreciosSeleccionada = null;
    
    // Restaurar botones
    $('#btnConfirmarCambiarLP').prop('disabled', true).html('<i class="bx bx-check-circle"></i> Cambiar LP');
    $('#btnCancelarCambiarLP').prop('disabled', false);
    
    console.log('✅ Modal cerrado');
}

//// ════════════════════════════════════════════════════════════
//// HELPERS
//// ════════════════════════════════════════════════════════════

///**
// * Escapa HTML (reutiliza función de siteGen.js si existe)
// */
//function escapeHtml(texto) {
//    if (typeof window.escapeHtml === 'function') {
//        return window.escapeHtml(texto);
//    }
    
//    if (!texto) return '';
//    const map = {
//        '&': '&amp;',
//        '<': '&lt;',
//        '>': '&gt;',
//        '"': '&quot;',
//        "'": '&#039;'
//    };
//    return texto.replace(/[&<>"']/g, m => map[m]);
//}

///**
// * Muestra mensaje de error
// */
//function mostrarMensajeError(mensaje) {
//    if (typeof window.mostrarMensajeError === 'function') {
//        window.mostrarMensajeError(mensaje);
//    } else {
//        console.error('💬 Error:', mensaje);
//        alert(mensaje);
//    }
//}
