// ════════════════════════════════════════════════════════════
// GESTOR DE COTIZACIONES
// ════════════════════════════════════════════════════════════
// VERSIÓN v1.6 - Bloqueo de pantalla durante diferimiento
// ════════════════════════════════════════════════════════════
// CARACTERÍSTICAS:
// ✅ Mensajes unificados con mostrarMensajeEstado
// ✅ Bloqueo visual durante operaciones asíncronas
// ✅ Protección contra múltiples clics
// ✅ Prevención de cierre accidental durante carga
// ✅ Desbloqueo automático en éxito/error/timeout
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
    console.log('💰 Módulo de Cotizaciones inicializado v1.5 UNIFICADO');
    inicializarEventosCotizaciones();
    inicializarProteccionCierreCotizaciones(); // ✅ NUEVO
});

// ════════════════════════════════════════════════════════════
// PROTECCIÓN CONTRA CIERRE DURANTE CARGA
// ════════════════════════════════════════════════════════════

/**
 * ✅ Prevenir cierre del modal durante operaciones críticas
 */
function inicializarProteccionCierreCotizaciones() {
    $('#modalCotizaciones').on('hide.bs.modal', function (e) {
        // Si hay un overlay activo, prevenir cierre
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
    console.log('🔧 Configurando eventos de cotizaciones...');

    // ✅ CORREGIDO: Click en fila de la tabla (usa data-pre-id)
    $(document).on('click', '#tbodyCotizaciones tr.cotizacion-row', function () {
        const preId = $(this).data('pre-id'); // ✅ CORREGIDO
        if (preId && preId !== 'undefined') {
            console.log('💰 Cotización seleccionada (click en fila):', preId);
            seleccionarCotizacion(preId);
        }
    });

    // ✅ CORREGIDO: Botón de acción en fila
    $(document).on('click', '.btn-seleccionar-cotizacion', function (e) {
        e.stopPropagation();
        const preId = $(this).data('pre-id'); // ✅ CORREGIDO
        if (preId) {
            console.log('📋 Cotización seleccionada (click en botón):', preId);
            seleccionarCotizacion(preId);
        }
    });

    // Botón Cancelar
    $('#btnCancelarCotizacion').on('click', function () {
        console.log('❌ Cancelar selección de cotización');
        cerrarModalCotizaciones();
    });

    // ✅ CORREGIDO: Botón Confirmar (no Seguir)
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
 * ACTUALIZADO v1.5: Uso de mostrarMensajeEstado
 */
function abrirModalCotizaciones() {
    console.log('═══════════════════════════════════════════════════');
    console.log('💰 ABRIR MODAL COTIZACIONES v1.5');
    console.log('═══════════════════════════════════════════════════');

    // ❶ Validar que haya cliente seleccionado
    if (!clienteActualFactura) {
        console.error('❌ No hay cliente seleccionado');
        // ✅ ACTUALIZADO v1.5: Uso de mostrarMensajeEstado
        mostrarMensajeEstado('Debe identificar un cliente antes de cargar una cotización', 'danger');
        return;
    }

    // ❷ RESTRICCIÓN: Solo clientes registrados (no consumidor final)
    if (!clienteActualFactura.id || clienteActualFactura.id === 'CF' || clienteActualFactura.id === '0') {
        console.error('❌ Cliente no válido para cotizaciones');
        // ✅ ACTUALIZADO v1.5: Uso de mostrarMensajeEstado
        mostrarMensajeEstado(
            'Las cotizaciones solo están disponibles para clientes registrados. Los consumidores finales no pueden tener cotizaciones.',
            'warning',
            7000
        );
        return;
    }

    console.log('   Cliente actual:', clienteActualFactura.denominacion);
    console.log('   CTA_ID:', clienteActualFactura.id);

    // ❸ Mostrar nombre del cliente en el modal
    $('#lblClienteCotizacion').text(clienteActualFactura.denominacion || 'Sin nombre');

    // ❹ Resetear selección
    cotizacionSeleccionada = null;
    $('#btnConfirmarCotizacion').prop('disabled', true); // ✅ CORREGIDO

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
    console.log('📡 CARGAR COTIZACIONES DESDE SERVIDOR v1.4');
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
            console.log('Response completo:', response);

            // ✅ VALIDACIÓN DE RESPUESTA
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

            // ✅ VALIDAR ARRAY DE COTIZACIONES
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

            // ✅ ÉXITO: Guardar y renderizar
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
 * ✅ ACTUALIZADO v1.4: Renderiza las cotizaciones con estilos golden
 * CORREGIDO: Uso consistente de pre_id
 */
function renderizarCotizaciones(cotizaciones) {
    console.log('═══════════════════════════════════════════════════');
    console.log('📝 RENDERIZANDO COTIZACIONES v1.4 GOLDEN CORREGIDA');
    console.log(`   Total a renderizar: ${cotizaciones.length}`);
    console.log('═══════════════════════════════════════════════════');

    const $tbody = $('#tbodyCotizaciones');
    $tbody.empty();

    let countExitosos = 0;
    let countErrores = 0;

    cotizaciones.forEach(function (cot, index) {
        try {
            // ✅ VALIDACIÓN ROBUSTA DE CAMPOS
            if (!cot || typeof cot !== 'object') {
                console.warn(`⚠️ Cotización [${index}] es inválida:`, cot);
                countErrores++;
                return;
            }

            // ═══════════════════════════════════════════════════════════════
            // ✅ NORMALIZACIÓN DE CAMPOS CORREGIDA
            // ═══════════════════════════════════════════════════════════════

            // ✅ CÓDIGO: pre_id (KEY)
            const preId = cot.pre_id?.toString().trim() || `COT-${index}`;

            // ✅ DESCRIPCIÓN: pre_descripcion
            const descripcion = cot.pre_descripcion?.trim() || 'Sin descripción';

            // ✅ FECHA: pre_fecha (puede venir como string ISO o Date)
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

            // ✅ CONDICIÓN DE PAGO: pre_obs_pago
            const condicionPago = cot.pre_obs_pago?.trim() || 'Sin especificar';

            // ✅ IMPORTE: importe (decimal)
            const importe = cot.importe || 0;

            // ✅ ESTADO: pree_desc
            const estado = cot.pree_desc?.trim() || '';
            const estadoBadge = estado ? `<span class="badge bg-golden-light text-golden-dark badge-compact ms-2">${escapeHtml(estado)}</span>` : '';

            // ═══════════════════════════════════════════════════════════════
            // ✅ LOG DETALLADO (solo primeras 3)
            // ═══════════════════════════════════════════════════════════════
            if (index < 3) {
                console.log(`   [${index}] pre_id: ${preId}`);
                console.log(`      - Descripción: ${descripcion}`);
                console.log(`      - Fecha: ${fecha}`);
                console.log(`      - Estado: ${estado}`);
                console.log(`      - Importe: $${importe}`);
            }

            // ═══════════════════════════════════════════════════════════════
            // ✅ CONSTRUCCIÓN DE FILA CON ESTILOS GOLDEN
            // ═══════════════════════════════════════════════════════════════
            const row = `
                <tr data-pre-id="${preId}" 
                    data-index="${index}"
                    class="cotizacion-row compact-row"
                    style="cursor: pointer;"
                    title="Click para seleccionar la cotización ${preId}">
                    
                    <td class="text-center fw-bold text-golden-dark">${escapeHtml(preId)}</td>
                    
                    <td class="text-truncate" title="${escapeHtml(descripcion)}">
                        ${escapeHtml(descripcion)}
                        ${estadoBadge}
                    </td>
                    
                    <td class="text-center">
                        <i class='bx bx-calendar text-muted' style="font-size: 0.9rem;"></i>
                        ${escapeHtml(fecha)}
                    </td>
                    
                    <td class="text-truncate" title="${escapeHtml(condicionPago)}">
                        <i class='bx bx-credit-card text-muted' style="font-size: 0.9rem;"></i>
                        ${escapeHtml(condicionPago)}
                    </td>
                    
                    <td class="text-end fw-bold text-success">
                        <i class='bx bx-dollar' style="font-size: 0.9rem;"></i>
                        ${formatearNumero(importe, 2)}
                    </td>
                    
                    <td class="text-center">
                        <button type="button" 
                                class="btn btn-success btn-sm btn-seleccionar-cotizacion"
                                data-pre-id="${preId}"
                                title="Seleccionar esta cotización">
                            <i class='bx bx-check-circle'></i> Seleccionar
                        </button>
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

    // ✅ SI NO SE RENDERIZÓ NINGUNA, MOSTRAR MENSAJE
    if (countExitosos === 0) {
        mostrarSinCotizaciones();
    }
}

// ════════════════════════════════════════════════════════════
// SELECCIÓN DE COTIZACIÓN
// ════════════════════════════════════════════════════════════
/**
 * ✅ ACTUALIZADO v1.5: Selecciona una cotización
 * CORREGIDO: Usa pre_id en lugar de cpf_nro
 * ACTUALIZADO v1.5: Uso de mostrarMensajeEstado
 */
function seleccionarCotizacion(preId) {
    console.log('═══════════════════════════════════════════════════');
    console.log(`💰 SELECCIONAR COTIZACIÓN v1.5`);
    console.log(`   pre_id recibido: "${preId}"`);
    console.log('═══════════════════════════════════════════════════');

    // ❶ VALIDACIÓN DE ENTRADA
    if (!preId || preId === 'undefined' || preId === 'null') {
        console.error('❌ pre_id inválido');
        return;
    }

    // ❷ Remover TODAS las selecciones anteriores
    $('#tbodyCotizaciones tr').removeClass('table-success selected-cotizacion');

    // ❸ Marcar como seleccionada
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

        if (cotPreId === preIdBuscar) {
            console.log(`   ✅ MATCH encontrado: ${cotPreId}`);
            return true;
        }
        return false;
    });

    if (!cotizacion) {
        console.error('❌ Cotización no encontrada en el array');
        console.error(`   Buscando pre_id: "${preId}"`);
        console.error(`   IDs disponibles en array:`, cotizacionesDisponibles.map(c => c.pre_id));

        // ✅ ACTUALIZADO v1.5: Uso de mostrarMensajeEstado
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
 * ✅ ACTUALIZADO v1.6: Confirma la cotización seleccionada con bloqueo de pantalla
 * CORREGIDO: Usa pre_id y validación robusta
 * NUEVO: Implementa bloqueo durante carga
 */
function confirmarCotizacion() {
    console.log('═══════════════════════════════════════════════════');
    console.log('✅ CONFIRMAR COTIZACIÓN SELECCIONADA v1.6 CON BLOQUEO');
    console.log('═══════════════════════════════════════════════════');

    // ❶ VALIDACIÓN ROBUSTA DE VARIABLE GLOBAL
    if (!cotizacionSeleccionada || typeof cotizacionSeleccionada !== 'object') {
        console.error('❌ cotizacionSeleccionada es null o inválido');
        console.error('   Valor actual:', cotizacionSeleccionada);
        mostrarMensajeEstado('Debe seleccionar una cotización antes de continuar', 'warning');
        return;
    }

    // ❷ VALIDACIÓN ROBUSTA: Obtener pre_id del objeto
    const preId = cotizacionSeleccionada.pre_id?.toString().trim();

    if (!preId || preId === '' || preId === 'undefined') {
        console.error('❌ pre_id no válido en cotizacionSeleccionada');
        console.error('   Objeto completo:', cotizacionSeleccionada);
        mostrarMensajeEstado('Error: Cotización sin código válido', 'danger');
        return;
    }

    console.log('   Cotización pre_id:', preId);
    console.log('   Descripción:', cotizacionSeleccionada.pre_descripcion || 'N/A');
    console.log('   Importe:', cotizacionSeleccionada.importe || 0);

    // ❸ VALIDACIÓN ADICIONAL: Verificar que existe buscarProductoPorCodigo
    if (typeof buscarProductoPorCodigo !== 'function') {
        console.error('❌ Función buscarProductoPorCodigo no está definida');
        mostrarMensajeEstado('Error: Función de carga no disponible. Verifique que prodfact.js esté cargado.', 'danger', 7000);
        return;
    }

    // ❹ ✅ NUEVO: BLOQUEAR PANTALLA ANTES DE CARGAR
    bloquearPantallaDiferimiento('Cargando cotización...');

    // ❺ Cerrar modal (el overlay permanecerá visible)
    $('#modalCotizaciones').modal('hide');

    // ❻ Pequeña demora para permitir transición del modal
    setTimeout(function () {
        try {
            // ❼ Cargar cotización mediante la función existente en prodfact.js
            console.log('═══════════════════════════════════════════════════');
            console.log('📡 INVOCANDO buscarProductoPorCodigo');
            console.log(`   - tipoValor: 'C'`);
            console.log(`   - valor: '${preId}'`);
            console.log(`   - cantidad: 1`);
            console.log(`   - bulto: true`);
            console.log(`   - origen: 'cotizacion'`);
            console.log('═══════════════════════════════════════════════════');

            buscarProductoPorCodigo(
                'C',            // tipoValor = C (Cotización)
                preId,          // valor = pre_id
                1,              // cantidad
                true,           // bulto
                'cotizacion'    // origen de carga = 'cotizacion'
            );

            console.log('⚠️ Modo de bloqueo de grilla: COTIZACIÓN activado');

            // ❽ ✅ NOTA: El desbloqueo se realizará en el callback de success/error
            //    de buscarProductoPorCodigo (debe modificarse en prodfact.js)
            //    Por seguridad, agregamos timeout de respaldo (15 segundos)
            setTimeout(function () {
                desbloquearPantallaDiferimiento();
                console.log('⚠️ Desbloqueo por timeout de seguridad (15s)');
            }, 15000);

        } catch (ex) {
            console.error('❌ Error al invocar buscarProductoPorCodigo:', ex);
            desbloquearPantallaDiferimiento();
            mostrarMensajeEstado('Error al cargar la cotización', 'danger');
        }
    }, 300);
}

// ════════════════════════════════════════════════════════════
// CERRAR MODAL
// ════════════════════════════════════════════════════════════
/**
 * ✅ ACTUALIZADO v1.6: Cierra el modal y limpia datos (con desbloqueo de seguridad)
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

    // ❹ ✅ NUEVO: Asegurar desbloqueo de pantalla por seguridad
    if ($('#overlayDiferimiento').length > 0) {
        console.log('⚠️ Overlay detectado al cerrar modal - Limpiando...');
        desbloquearPantallaDiferimiento();
    }

    console.log('✅ Modal cerrado');
}

// ════════════════════════════════════════════════════════════
// HELPERS ACTUALIZADOS CON ESTILOS GOLDEN
// ════════════════════════════════════════════════════════════

/**
 * ✅ ACTUALIZADO v1.4: Muestra mensaje cuando no hay cotizaciones
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
 * ✅ ACTUALIZADO v1.4: Muestra error al cargar cotizaciones
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
 * @param {string} mensaje - Mensaje a mostrar durante el bloqueo
 */
function bloquearPantallaDiferimiento(mensaje = 'Procesando...') {
    console.log('🔒 Bloqueando pantalla para diferimiento...');

    // ❶ Crear overlay si no existe
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
        // ❷ Actualizar mensaje si ya existe
        $('#mensajeDiferimiento').text(mensaje);
        $('#overlayDiferimiento').fadeIn(200);
    }

    // ❸ Deshabilitar botones críticos
    $('#btnConfirmarCotizacion, #btnCancelarCotizacion, .btn-seleccionar-cotizacion').prop('disabled', true);

    // ❹ Prevenir cierre del modal con ESC
    $('#modalCotizaciones').data('bs-keyboard', false);

    console.log('✅ Pantalla bloqueada');
}

/**
 * ✅ Desbloquea la interfaz después de completar la operación
 */
function desbloquearPantallaDiferimiento() {
    console.log('🔓 Desbloqueando pantalla...');

    // ❶ Remover overlay con animación
    $('#overlayDiferimiento').fadeOut(300, function () {
        $(this).remove();
    });

    // ❷ Rehabilitar botones
    $('#btnConfirmarCotizacion, #btnCancelarCotizacion, .btn-seleccionar-cotizacion').prop('disabled', false);

    // ❸ Restaurar cierre con ESC
    $('#modalCotizaciones').data('bs-keyboard', true);

    console.log('✅ Pantalla desbloqueada');
}

// ════════════════════════════════════════════════════════════
// CALLBACKS PÚBLICOS PARA DESBLOQUEO
// ════════════════════════════════════════════════════════════

/**
 * ✅ Callback para invocar desde prodfact.js cuando se completa la carga
 * @param {boolean} exito - Indica si la carga fue exitosa
 * @param {string} mensaje - Mensaje de resultado (opcional)
 */
window.onCotizacionCargadaCompleta = function (exito, mensaje) {
    console.log('═══════════════════════════════════════════════════');
    console.log('🎯 CALLBACK: onCotizacionCargadaCompleta');
    console.log(`   Éxito: ${exito}`);
    console.log(`   Mensaje: ${mensaje || 'N/A'}`);
    console.log('═══════════════════════════════════════════════════');

    // ❶ Desbloquear pantalla
    desbloquearPantallaDiferimiento();

    // ❷ Mostrar mensaje si corresponde
    if (mensaje) {
        const tipo = exito ? 'success' : 'danger';
        mostrarMensajeEstado(mensaje, tipo);
    }

    // ❸ Limpiar variables de sesión
    cotizacionSeleccionada = null;
};

/**
 * ✅ Función de conveniencia para desbloqueo directo
 */
window.desbloquearCotizacion = function () {
    desbloquearPantallaDiferimiento();
};