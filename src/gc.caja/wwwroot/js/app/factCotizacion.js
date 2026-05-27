// ════════════════════════════════════════════════════════════
// GESTOR DE COTIZACIONES
// ════════════════════════════════════════════════════════════
// VERSIÓN v1.8 - FIX: Modal se cierra correctamente
// ════════════════════════════════════════════════════════════
// CARACTERÍSTICAS:
// ✅ Radiobuttons nativos para selección única
// ✅ Click en fila activa el radiobutton automáticamente
// ✅ Validación robusta con datos del DOM
// ✅ Bloqueo visual durante operaciones asíncronas
// ✅ FIX: Modal se cierra ANTES de bloquear pantalla
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
    console.log('💰 Módulo de Cotizaciones inicializado v1.8 - FIX MODAL');
    inicializarEventosCotizaciones();
    inicializarProteccionCierreCotizaciones();
});

// ════════════════════════════════════════════════════════════
// PROTECCIÓN CONTRA CIERRE DURANTE CARGA
// ════════════════════════════════════════════════════════════

/**
 * ✅ Prevenir cierre del modal durante operaciones críticas
 */
function inicializarProteccionCierreCotizaciones() {
    $('#modalCotizaciones').on('hide.bs.modal', function (e) {
        if ($('#overlayDiferimiento').length > 0 && $('#overlayDiferimiento').is(':visible')) {
            console.warn('⚠️ Intento de cerrar modal durante operación - BLOQUEADO');
            e.preventDefault();
            return false;
        }
    });
}

// ════════════════════════════════════════════════════════════
// EVENTOS
// ════════════════════════════════════════════════════════════
function inicializarEventosCotizaciones() {
    console.log('🔧 Configurando eventos de cotizaciones v1.8...');

    // ✅ NUEVO: Click en fila selecciona el radiobutton
    $(document).on('click', '#tbodyCotizaciones tr.cotizacion-row', function (e) {
        // Evitar doble trigger si se hizo click directo en el radio
        if ($(e.target).is('input[type="radio"]')) {
            return;
        }

        const $radio = $(this).find('.radio-cotizacion');
        if ($radio.length > 0 && !$radio.prop('disabled')) {
            $radio.prop('checked', true).trigger('change');
        }
    });

    // ✅ NUEVO: Cambio en radiobutton actualiza selección
    $(document).on('change', '.radio-cotizacion', function () {
        const preId = $(this).val();
        if (preId && preId !== 'undefined') {
            console.log('💰 Cotización seleccionada via radio:', preId);
            seleccionarCotizacion(preId);
        }
    });

    // ✅ Botón Cancelar
    $('#btnCancelarCotizacion').on('click', function () {
        console.log('❌ Cancelar selección de cotización');
        cerrarModalCotizaciones();
    });

    // ✅ Botón Confirmar
    $('#btnConfirmarCotizacion').on('click', function () {
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
    console.log('💰 ABRIR MODAL COTIZACIONES v1.8');
    console.log('═══════════════════════════════════════════════════');

    // ❶ Validar que haya cliente seleccionado
    if (!clienteActualFactura) {
        console.error('❌ No hay cliente seleccionado');
        mostrarMensajeEstado('Debe identificar un cliente antes de cargar una cotización', 'danger');
        return;
    }

    // ❷ RESTRICCIÓN: Solo clientes registrados
    if (!clienteActualFactura.id || clienteActualFactura.id === 'CF' || clienteActualFactura.id === '0') {
        console.error('❌ Cliente no válido para cotizaciones');
        mostrarMensajeEstado(
            'Las cotizaciones solo están disponibles para clientes registrados. Los consumidores finales no pueden tener cotizaciones.',
            'warning',
            7000
        );
        return;
    }

    console.log('   Cliente actual:', clienteActualFactura.denominacion);
    console.log('   CTA_ID:', clienteActualFactura.id);

    // ❸ Mostrar nombre del cliente
    $('#lblClienteCotizacion').text(clienteActualFactura.denominacion || 'Sin nombre');

    // ❹ Resetear selección
    cotizacionSeleccionada = null;
    $('#btnConfirmarCotizacion').prop('disabled', true);

    // ❺ Mostrar modal
    $('#modalCotizaciones').modal('show');

    // ❻ Cargar cotizaciones
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
    console.log('📡 CARGAR COTIZACIONES DESDE SERVIDOR v1.8');
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

    console.log(`   URL: ${url}`);
    console.log(`   CTA_ID: ${clienteActualFactura.id}`);

    // ❸ Llamada AJAX
    $.ajax({
        url: url,
        type: 'POST',
        data: {
            cta_id: clienteActualFactura.id
        },
        dataType: 'json',
        timeout: 15000,
        success: function (response) {
            console.log('═══════════════════════════════════════════════════');
            console.log('✅ RESPUESTA RECIBIDA DEL SERVIDOR');
            console.log('═══════════════════════════════════════════════════');

            // ✅ Validación de respuesta
            if (!response || typeof response !== 'object') {
                console.error('❌ Respuesta inválida del servidor');
                mostrarErrorCargarCotizaciones('Respuesta inválida del servidor');
                return;
            }

            if (!response.ok) {
                console.error('❌ Error del servidor:', response.mensaje);
                mostrarErrorCargarCotizaciones(response.mensaje || 'Error al cargar cotizaciones');
                return;
            }

            // ✅ Validar array de cotizaciones
            if (!response.cotizaciones || !Array.isArray(response.cotizaciones)) {
                console.error('❌ cotizaciones no es un array');
                mostrarErrorCargarCotizaciones('Error en el formato de datos recibidos');
                return;
            }

            if (response.cotizaciones.length === 0) {
                console.log('ℹ️ No hay cotizaciones disponibles');
                mostrarSinCotizaciones();
                return;
            }

            // ✅ Éxito: Guardar y renderizar
            console.log(`✅ Se recibieron ${response.cotizaciones.length} cotizaciones`);
            cotizacionesDisponibles = response.cotizaciones;
            renderizarCotizaciones(response.cotizaciones);
        },
        error: function (xhr, status, error) {
            console.log('═══════════════════════════════════════════════════');
            console.error('❌ ERROR EN LLAMADA AJAX');
            console.error(`   Status: ${status}`);
            console.error(`   Error: ${error}`);
            console.error(`   HTTP Status: ${xhr.status}`);
            console.log('═══════════════════════════════════════════════════');

            // Usar interceptor de sesiones
            if (esSesionExpirada(xhr.status)) {
                console.warn('⚠️ Sesión expirada detectada');
                return;
            }

            let mensajeError = 'Error de comunicación con el servidor';

            if (xhr.status === 500) {
                mensajeError = 'Error interno del servidor. Contacte al administrador.';
            } else if (xhr.status === 404) {
                mensajeError = 'Servicio no encontrado. Verifique la configuración.';
            } else if (status === 'timeout') {
                mensajeError = 'Se agotó el tiempo de espera. Intente nuevamente.';
            } else if (xhr.status === 0) {
                mensajeError = 'No se pudo conectar con el servidor. Verifique su conexión.';
            }

            mostrarErrorCargarCotizaciones(mensajeError);
        }
    });
}

// ════════════════════════════════════════════════════════════
// RENDERIZAR COTIZACIONES
// ════════════════════════════════════════════════════════════
/**
 * ✅ ACTUALIZADO v1.8: Renderiza cotizaciones con radiobuttons nativos
 */
function renderizarCotizaciones(cotizaciones) {
    console.log('═══════════════════════════════════════════════════');
    console.log('📝 RENDERIZANDO COTIZACIONES v1.8 CON RADIOBUTTONS');
    console.log(`   Total a renderizar: ${cotizaciones.length}`);
    console.log('═══════════════════════════════════════════════════');

    const $tbody = $('#tbodyCotizaciones');
    $tbody.empty();

    let countExitosos = 0;
    let countErrores = 0;

    cotizaciones.forEach(function (cot, index) {
        try {
            // ✅ Validación robusta de campos
            if (!cot || typeof cot !== 'object') {
                console.warn(`⚠️ Cotización [${index}] es inválida:`, cot);
                countErrores++;
                return;
            }

            // ✅ Normalización de campos
            const preId = cot.pre_id?.toString().trim() || `COT-${index}`;
            const descripcion = cot.pre_descripcion?.trim() || 'Sin descripción';

            // ✅ Fecha
            let fecha = '-';
            if (cot.pre_fecha) {
                try {
                    const fechaObj = new Date(cot.pre_fecha);
                    if (!isNaN(fechaObj.getTime())) {
                        const dia = fechaObj.getDate().toString().padStart(2, '0');
                        const mes = (fechaObj.getMonth() + 1).toString().padStart(2, '0');
                        const anio = fechaObj.getFullYear();
                        fecha = `${dia}/${mes}/${anio}`;
                    }
                } catch (ex) {
                    console.warn(`⚠️ Error al parsear fecha de cotización [${index}]:`, ex);
                }
            }

            const condicionPago = cot.pre_obs_pago?.trim() || 'Sin especificar';
            const importe = cot.importe || 0;
            const estado = cot.pree_desc?.trim() || '';
            const estadoBadge = estado ? `<span class="badge bg-golden-light text-golden-dark badge-compact ms-2">${escapeHtml(estado)}</span>` : '';

            // ✅ Log detallado (primeras 3)
            if (index < 3) {
                console.log(`   [${index}] pre_id: ${preId}`);
                console.log(`      - Descripción: ${descripcion}`);
                console.log(`      - Importe: $${importe}`);
            }

            // ═══════════════════════════════════════════════════════════════
            // ✅ CONSTRUCCIÓN DE FILA CON RADIOBUTTON
            // ═══════════════════════════════════════════════════════════════
            const row = `
                <tr data-pre-id="${preId}" 
                    data-index="${index}"
                    class="cotizacion-row compact-row"
                    style="cursor: pointer;"
                    title="Click para seleccionar la cotización ${preId}">
                    
                    <!-- ✅ COLUMNA RADIOBUTTON -->
                    <td class="text-center align-middle">
                        <input type="radio" 
                               name="radioCotizacion" 
                               class="radio-cotizacion form-check-input" 
                               value="${preId}"
                               id="radio_${preId}"
                               style="cursor: pointer; width: 1.2rem; height: 1.2rem;">
                    </td>
                    
                    <td class="text-center fw-bold text-golden-dark align-middle">
                        ${escapeHtml(preId)}
                    </td>
                    
                    <td class="text-truncate align-middle" title="${escapeHtml(descripcion)}">
                        ${escapeHtml(descripcion)}
                        ${estadoBadge}
                    </td>
                    
                    <td class="text-center align-middle">
                        <i class='bx bx-calendar text-muted' style="font-size: 0.9rem;"></i>
                        ${escapeHtml(fecha)}
                    </td>
                    
                    <td class="text-truncate align-middle" title="${escapeHtml(condicionPago)}">
                        <i class='bx bx-credit-card text-muted' style="font-size: 0.9rem;"></i>
                        ${escapeHtml(condicionPago)}
                    </td>
                    
                    <td class="text-end fw-bold text-success align-middle">
                        <i class='bx bx-dollar' style="font-size: 0.9rem;"></i>
                        ${formatearNumero(importe, 2)}
                    </td>
                </tr>
            `;

            $tbody.append(row);
            countExitosos++;

        } catch (ex) {
            console.error(`❌ Error al renderizar cotización [${index}]:`, ex);
            countErrores++;
        }
    });

    console.log('═══════════════════════════════════════════════════');
    console.log(`✅ RENDERIZADO COMPLETADO`);
    console.log(`   - Exitosos: ${countExitosos}`);
    console.log(`   - Errores: ${countErrores}`);
    console.log('═══════════════════════════════════════════════════');

    // ✅ Si no se renderizó ninguna, mostrar mensaje
    if (countExitosos === 0) {
        mostrarSinCotizaciones();
    }
}

// ════════════════════════════════════════════════════════════
// SELECCIÓN DE COTIZACIÓN
// ════════════════════════════════════════════════════════════
/**
 * ✅ ACTUALIZADO v1.8: Selecciona cotización y actualiza UI
 */
function seleccionarCotizacion(preId) {
    console.log('═══════════════════════════════════════════════════');
    console.log(`💰 SELECCIONAR COTIZACIÓN v1.8`);
    console.log(`   pre_id recibido: "${preId}"`);
    console.log('═══════════════════════════════════════════════════');

    // ❶ Validación de entrada
    if (!preId || preId === 'undefined' || preId === 'null') {
        console.error('❌ pre_id inválido');
        return;
    }

    // ❷ Remover selección visual anterior
    $('#tbodyCotizaciones tr').removeClass('table-success selected-cotizacion');

    // ❸ Marcar fila como seleccionada
    const $row = $(`#tbodyCotizaciones tr[data-pre-id="${preId}"]`);

    if ($row.length === 0) {
        console.error('❌ No se encontró la fila en el DOM');
        return;
    }

    $row.addClass('table-success selected-cotizacion');

    // ❹ Buscar datos completos en el array
    console.log('🔍 Buscando en array cotizacionesDisponibles...');
    console.log(`   Total disponibles: ${cotizacionesDisponibles.length}`);

    const cotizacion = cotizacionesDisponibles.find(cot => {
        const cotPreId = cot.pre_id?.toString().trim();
        const preIdBuscar = preId.toString().trim();
        return cotPreId === preIdBuscar;
    });

    if (!cotizacion) {
        console.error('❌ Cotización no encontrada en el array');
        console.error(`   Buscando pre_id: "${preId}"`);
        console.error(`   IDs disponibles:`, cotizacionesDisponibles.map(c => c.pre_id));
        mostrarMensajeEstado('Error: No se pudieron obtener los datos de la cotización', 'danger');
        return;
    }

    // ❺ Guardar selección
    cotizacionSeleccionada = cotizacion;

    // ❻ Habilitar botón Confirmar
    $('#btnConfirmarCotizacion').prop('disabled', false);

    console.log('═══════════════════════════════════════════════════');
    console.log('✅ COTIZACIÓN SELECCIONADA EXITOSAMENTE');
    console.log('   Datos completos:', cotizacionSeleccionada);
    console.log('═══════════════════════════════════════════════════');
}

// ════════════════════════════════════════════════════════════
// CONFIRMAR COTIZACIÓN
// ════════════════════════════════════════════════════════════
/**
 * ✅ FIX v1.8: Confirma la cotización con cierre correcto del modal
 */
function confirmarCotizacion() {
    console.log('═══════════════════════════════════════════════════');
    console.log('✅ CONFIRMAR COTIZACIÓN SELECCIONADA v1.8 - FIX MODAL');
    console.log('═══════════════════════════════════════════════════');

    // ❶ Validación robusta
    if (!cotizacionSeleccionada || typeof cotizacionSeleccionada !== 'object') {
        console.error('❌ cotizacionSeleccionada es null o inválido');
        mostrarMensajeEstado('Debe seleccionar una cotización antes de continuar', 'warning');
        return;
    }

    // ❷ Validar pre_id
    const preId = cotizacionSeleccionada.pre_id?.toString().trim();

    if (!preId || preId === '' || preId === 'undefined') {
        console.error('❌ pre_id no válido en cotizacionSeleccionada');
        mostrarMensajeEstado('Error: Cotización sin código válido', 'danger');
        return;
    }

    console.log('   Cotización pre_id:', preId);
    console.log('   Descripción:', cotizacionSeleccionada.pre_descripcion || 'N/A');

    // ❸ Validar función de carga
    if (typeof buscarProductoPorCodigo !== 'function') {
        console.error('❌ Función buscarProductoPorCodigo no está definida');
        mostrarMensajeEstado('Error: Función de carga no disponible. Verifique que prodfact.js esté cargado.', 'danger', 7000);
        return;
    }

    // ═══════════════════════════════════════════════════════════════
    // ✅ FIX CRÍTICO v1.8: CERRAR MODAL ANTES DE BLOQUEAR PANTALLA
    // ═══════════════════════════════════════════════════════════════

    console.log('🔄 Cerrando modal ANTES de bloquear pantalla...');

    // ❹ Cerrar modal primero
    $('#modalCotizaciones').modal('hide');

    // ❺ Esperar a que el modal se cierre completamente (evento 'hidden.bs.modal')
    $('#modalCotizaciones').one('hidden.bs.modal', function () {
        console.log('✅ Modal cerrado completamente - Iniciando carga de cotización');

        // ❻ AHORA sí bloqueamos la pantalla (sin el overlay el modal ya se cerró)
        bloquearPantallaDiferimiento('Cargando cotización...');

        // ❼ Invocar carga con pequeña demora
        setTimeout(function () {
            try {
                console.log('═══════════════════════════════════════════════════');
                console.log('📡 INVOCANDO buscarProductoPorCodigo');
                console.log(`   - tipoValor: 'C'`);
                console.log(`   - valor: '${preId}'`);
                console.log('═══════════════════════════════════════════════════');

                buscarProductoPorCodigo(
                    'C',            // tipoValor = C (Cotización)
                    preId,          // valor = pre_id
                    1,              // cantidad
                    true,           // bulto
                    'cotizacion'    // origen
                );

                // ❽ Timeout de seguridad
                setTimeout(function () {
                    desbloquearPantallaDiferimiento();
                    console.log('⚠️ Desbloqueo por timeout de seguridad (15s)');
                }, 15000);

            } catch (ex) {
                console.error('❌ Error al invocar buscarProductoPorCodigo:', ex);
                desbloquearPantallaDiferimiento();
                mostrarMensajeEstado('Error al cargar la cotización', 'danger');
            }
        }, 100); // Demora reducida a 100ms ya que el modal ya está cerrado
    });
}

// ════════════════════════════════════════════════════════════
// CERRAR MODAL
// ════════════════════════════════════════════════════════════
/**
 * ✅ Cierra el modal y limpia datos
 */
function cerrarModalCotizaciones() {
    console.log('🔙 Cerrando modal de cotizaciones...');

    // ❶ Cerrar modal
    $('#modalCotizaciones').modal('hide');

    // ❷ Limpiar datos
    cotizacionSeleccionada = null;
    cotizacionesDisponibles = [];

    // ❸ Restaurar botones
    $('#btnConfirmarCotizacion').prop('disabled', true);

    // ❹ Desmarcar radiobuttons
    $('.radio-cotizacion').prop('checked', false);

    // ❺ Remover selección visual
    $('#tbodyCotizaciones tr').removeClass('table-success selected-cotizacion');

    // ❻ Asegurar desbloqueo
    if ($('#overlayDiferimiento').length > 0) {
        console.log('⚠️ Overlay detectado al cerrar modal - Limpiando...');
        desbloquearPantallaDiferimiento();
    }

    console.log('✅ Modal cerrado');
}

// ════════════════════════════════════════════════════════════
// HELPERS
// ════════════════════════════════════════════════════════════

/**
 * ✅ Muestra mensaje cuando no hay cotizaciones
 */
function mostrarSinCotizaciones() {
    console.log('ℹ️ Mostrando mensaje: No hay cotizaciones disponibles');

    $('#tbodyCotizaciones').html(`
        <tr id="rowSinCotizaciones">
            <td colspan="6" class="text-center text-muted py-5">
                <div class="loading-container">
                    <i class='bx bx-dollar-circle text-golden' style="font-size: 3.5rem;"></i>
                    <p class="mb-0 mt-3">
                        <strong class="text-golden-dark">No hay cotizaciones disponibles</strong>
                    </p>
                    <small class="text-muted d-block mt-2">
                        <i class='bx bx-info-circle'></i>
                        El cliente no tiene cotizaciones pendientes de facturar
                    </small>
                </div>
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
            <td colspan="6" class="p-0">
                <div class="alert alert-danger border-danger m-3" style="border-width: 0 0 0 4px;">
                    <div class="d-flex align-items-center">
                        <i class='bx bx-error-circle' style="font-size: 2.5rem; margin-right: 1rem;"></i>
                        <div>
                            <strong>Error al cargar cotizaciones</strong>
                            <p class="mb-0 mt-1">${escapeHtml(mensaje)}</p>
                        </div>
                    </div>
                </div>
            </td>
        </tr>
    `);
}

// ════════════════════════════════════════════════════════════
// GESTIÓN DE BLOQUEO DE PANTALLA
// ════════════════════════════════════════════════════════════

/**
 * ✅ Bloquea la interfaz durante operaciones asíncronas
 */
function bloquearPantallaDiferimiento(mensaje = 'Procesando...') {
    console.log('🔒 Bloqueando pantalla para diferimiento...');

    if ($('#overlayDiferimiento').length === 0) {
        const overlay = `
            <div id="overlayDiferimiento" class="loading-overlay">
                <div class="loading-content">
                    <div class="spinner-border text-golden" role="status" style="width: 3rem; height: 3rem;">
                        <span class="visually-hidden">Cargando...</span>
                    </div>
                    <p class="loading-message mt-3 mb-0 fw-bold text-golden-dark" id="mensajeDiferimiento">
                        ${mensaje}
                    </p>
                </div>
            </div>
        `;
        $('body').append(overlay);
    } else {
        $('#mensajeDiferimiento').text(mensaje);
        $('#overlayDiferimiento').fadeIn(200);
    }

    $('#btnConfirmarCotizacion, #btnCancelarCotizacion').prop('disabled', true);
    $('#modalCotizaciones').data('bs-keyboard', false);

    console.log('✅ Pantalla bloqueada');
}

/**
 * ✅ Desbloquea la interfaz
 */
function desbloquearPantallaDiferimiento() {
    console.log('🔓 Desbloqueando pantalla...');

    $('#overlayDiferimiento').fadeOut(300, function () {
        $(this).remove();
    });

    $('#btnConfirmarCotizacion, #btnCancelarCotizacion').prop('disabled', false);
    $('#modalCotizaciones').data('bs-keyboard', true);

    console.log('✅ Pantalla desbloqueada');
}

// ════════════════════════════════════════════════════════════
// CALLBACKS PÚBLICOS
// ════════════════════════════════════════════════════════════

/**
 * ✅ Callback para invocar desde prodfact.js
 */
window.onCotizacionCargadaCompleta = function (exito, mensaje) {
    console.log('═══════════════════════════════════════════════════');
    console.log('🎯 CALLBACK: onCotizacionCargadaCompleta');
    console.log(`   Éxito: ${exito}`);
    console.log(`   Mensaje: ${mensaje || 'N/A'}`);
    console.log('═══════════════════════════════════════════════════');

    desbloquearPantallaDiferimiento();

    if (mensaje) {
        const tipo = exito ? 'success' : 'danger';
        mostrarMensajeEstado(mensaje, tipo);
    }

    cotizacionSeleccionada = null;
};

/**
 * ✅ Función de conveniencia para desbloqueo directo
 */
window.desbloquearCotizacion = function () {
    desbloquearPantallaDiferimiento();
};