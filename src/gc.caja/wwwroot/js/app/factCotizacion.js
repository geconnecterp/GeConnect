// ════════════════════════════════════════════════════════════
// GESTOR DE COTIZACIONES
// ════════════════════════════════════════════════════════════
// VERSIÓN v1.2 CORREGIDA
// RESTRICCIÓN: Solo se puede seleccionar UNA cotización
// BLOQUEO: Al cargar una cotización, se bloquea la grilla
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
    console.log('💰 Módulo de Cotizaciones inicializado v1.2 CORREGIDA');
    inicializarEventosCotizaciones();
});

// ════════════════════════════════════════════════════════════
// EVENTOS
// ════════════════════════════════════════════════════════════
function inicializarEventosCotizaciones() {
    console.log('🔧 Configurando eventos de cotizaciones...');
    
    // ✅ NUEVO: Click en fila de la tabla (delegación de eventos)
    $(document).on('click', '#tbodyCotizaciones tr.cotizacion-row', function () {
        const preId = $(this).data('pre-id');
        if (preId && preId !== 'undefined') {
            console.log('📋 Cotización seleccionada (click en fila):', preId);
            seleccionarCotizacion(preId);
        }
    });
    
    // Botón Cancelar
    $('#btnCancelarCotizacion').on('click', function () {
        console.log('❌ Cancelar selección de cotización');
        cerrarModalCotizaciones();
    });
    
    // Botón Confirmar
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
 * ✅ Abre el modal de cotizaciones
 * VALIDACIÓN CRÍTICA: La grilla debe estar vacía
 */
function abrirModalCotizaciones() {
    console.log('═══════════════════════════════════════════════════');
    console.log('💰 ABRIR MODAL COTIZACIONES');
    console.log('═══════════════════════════════════════════════════');
    
    // ❶ VALIDACIÓN CRÍTICA: Grilla debe estar vacía
    if (productosFactura && productosFactura.length > 0) {
        console.error('❌ Grilla no vacía - No se puede cargar cotización');
        mostrarMensajeError(
            'No se puede cargar una cotización.\n\n' +
            'La grilla debe estar vacía. Por favor, elimine los productos actuales.'
        );
        return;
    }
    
    // ❷ Validar que haya cliente seleccionado
    if (!clienteActualFactura) {
        console.error('❌ No hay cliente seleccionado');
        mostrarMensajeError('Debe identificar un cliente antes de cargar una cotización');
        return;
    }
    
    // ❸ RESTRICCIÓN: Solo clientes registrados (no consumidor final)
    if (!clienteActualFactura.id || clienteActualFactura.id === 'CF' || clienteActualFactura.id === '0') {
        console.error('❌ Cliente no válido para cotizaciones');
        mostrarMensajeError(
            'Las cotizaciones solo están disponibles para clientes registrados.\n\n' +
            'Los consumidores finales no pueden tener cotizaciones.'
        );
        return;
    }
    
    console.log('   Cliente actual:', clienteActualFactura.denominacion);
    console.log('   CTA_ID:', clienteActualFactura.id);
    
    // ❹ Mostrar nombre del cliente en el modal
    $('#lblClienteCotizacion').text(clienteActualFactura.denominacion || 'Sin nombre');
    
    // ❺ Resetear selección
    cotizacionSeleccionada = null;
    $('#btnConfirmarCotizacion').prop('disabled', true);
    
    // ❻ Mostrar modal
    $('#modalCotizaciones').modal('show');
    
    // ❼ Cargar cotizaciones
    cargarCotizaciones();
}

// ════════════════════════════════════════════════════════════
// HELPERS (DEBEN ESTAR ANTES DE SER USADAS)
// ════════════════════════════════════════════════════════════

/**
 * ✅ Muestra mensaje cuando no hay cotizaciones
 */
function mostrarSinCotizaciones() {
    console.log('ℹ️ Mostrando mensaje: No hay cotizaciones disponibles');

    $('#tbodyCotizaciones').html(`
        <tr id="rowSinCotizaciones">
            <td colspan="6" class="text-center text-muted py-5">
                <i class='bx bx-dollar-circle bx-lg text-golden'></i>
                <p class="mb-0 mt-2">
                    <strong>No hay cotizaciones disponibles</strong><br>
                    <small>El cliente no tiene cotizaciones pendientes</small>
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
// CARGAR COTIZACIONES
// ════════════════════════════════════════════════════════════
/**
 * ✅ ACTUALIZADO v1.2: Obtiene las cotizaciones desde el servidor
 */
function cargarCotizaciones() {
    console.log('═══════════════════════════════════════════════════');
    console.log('📡 CARGAR COTIZACIONES DESDE SERVIDOR v1.2');
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
        success: function(response) {
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
        error: function(xhr, status, error) {
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
 * ✅ ACTUALIZADO v1.2: Renderiza las cotizaciones en la tabla
 * CORREGIDO: Usa campos correctos del DTO (pre_*)
 */
function renderizarCotizaciones(cotizaciones) {
    console.log('═══════════════════════════════════════════════════');
    console.log('📝 RENDERIZANDO COTIZACIONES v1.2 CORREGIDA');
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
                        // Formatear como DD/MM/YYYY
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

            // ═══════════════════════════════════════════════════════════════
            // ✅ LOG DETALLADO (solo primeras 3)
            // ═══════════════════════════════════════════════════════════════
            if (index < 3) {
                console.log(`   [${index}] pre_id: ${preId}`);
                console.log(`      - Descripción: ${descripcion}`);
                console.log(`      - Fecha: ${fecha}`);
                console.log(`      - Importe: $${importe}`);
            }

            // ═══════════════════════════════════════════════════════════════
            // ✅ CONSTRUCCIÓN DE FILA HTML CON DOBLE MECANISMO DE SELECCIÓN
            // ═══════════════════════════════════════════════════════════════
            const row = `
                <tr data-pre-id="${preId}" 
                    data-index="${index}"
                    class="cotizacion-row"
                    style="cursor: pointer;"
                    title="Click para seleccionar">
                    <td class="text-center fw-bold">${escapeHtml(preId)}</td>
                    <td>${escapeHtml(descripcion)}</td>
                    <td class="text-center">${escapeHtml(fecha)}</td>
                    <td>${escapeHtml(condicionPago)}</td>
                    <td class="text-end fw-semibold text-success">$ ${formatearNumero(importe, 2)}</td>
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
    
    // ✅ AGREGAR EVENTO A BOTONES DE SELECCIÓN (delegación de eventos)
    $(document).off('click', '.btn-seleccionar-cotizacion').on('click', '.btn-seleccionar-cotizacion', function (e) {
        e.stopPropagation(); // Evitar que se dispare el click de la fila
        const preId = $(this).data('pre-id');
        if (preId) {
            console.log('📋 Cotización seleccionada (click en botón):', preId);
            seleccionarCotizacion(preId);
        }
    });
}

// ════════════════════════════════════════════════════════════
// SELECCIÓN DE COTIZACIÓN
// ════════════════════════════════════════════════════════════
/**
 * ✅ ACTUALIZADO v1.2: Selecciona una cotización (solo UNA)
 * CORREGIDO: Busca por pre_id con logs mejorados
 */
function seleccionarCotizacion(preId) {
    console.log('═══════════════════════════════════════════════════');
    console.log(`💰 SELECCIONAR COTIZACIÓN`);
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
        
        mostrarMensajeError('Error: No se pudieron obtener los datos de la cotización');
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
 * ✅ ACTUALIZADO v1.2: Confirma la cotización seleccionada
 * CORREGIDO: Validación robusta antes de acceder a propiedades
 */
function confirmarCotizacion() {
    console.log('═══════════════════════════════════════════════════');
    console.log('✅ CONFIRMAR COTIZACIÓN SELECCIONADA v1.2 CORREGIDA');
    console.log('═══════════════════════════════════════════════════');

    // ❶ VALIDACIÓN ROBUSTA DE VARIABLE GLOBAL
    if (!cotizacionSeleccionada || typeof cotizacionSeleccionada !== 'object') {
        console.error('❌ cotizacionSeleccionada es null o inválido');
        console.error('   Valor actual:', cotizacionSeleccionada);
        mostrarMensajeError('Debe seleccionar una cotización antes de continuar');
        return;
    }

    // ❷ VALIDACIÓN ROBUSTA: Obtener pre_id del objeto
    const preId = cotizacionSeleccionada.pre_id?.toString().trim();
    
    if (!preId || preId === '' || preId === 'undefined') {
        console.error('❌ pre_id no válido en cotizacionSeleccionada');
        console.error('   Objeto completo:', cotizacionSeleccionada);
        mostrarMensajeError('Error: Cotización sin código válido');
        return;
    }

    console.log('   Cotización pre_id:', preId);
    console.log('   Descripción:', cotizacionSeleccionada.pre_descripcion || 'N/A');
    console.log('   Importe:', cotizacionSeleccionada.importe || 0);

    // ❸ VALIDACIÓN ADICIONAL: Verificar que existe buscarProductoPorCodigo
    if (typeof buscarProductoPorCodigo !== 'function') {
        console.error('❌ Función buscarProductoPorCodigo no está definida');
        mostrarMensajeError('Error: Función de carga no disponible. Verifique que prodfact.js esté cargado.');
        return;
    }

    // ❹ Cerrar modal
    cerrarModalCotizaciones();

    // ❺ Cargar cotización mediante la función existente en prodfact.js
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
        preId,          // ✅ valor = pre_id
        1,              // cantidad
        true,           // bulto
        'cotizacion'    // ⚠️ CRÍTICO: origen de carga = 'cotizacion'
    );

    console.log('⚠️ Modo de bloqueo de grilla: COTIZACIÓN activado');
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
    $('#btnConfirmarCotizacion').prop('disabled', true);
    
    console.log('✅ Modal cerrado');
}
