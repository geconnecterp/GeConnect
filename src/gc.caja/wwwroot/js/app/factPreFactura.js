// ════════════════════════════════════════════════════════════
// GESTOR DE PRE-FACTURAS
// ════════════════════════════════════════════════════════════
// VERSIÓN v2.3 - CORREGIDA: Funciones ordenadas correctamente
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
    console.log('📄 Módulo de Pre-Facturas inicializado v2.3 CORREGIDA');
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
// SECCIÓN: HELPERS (DEBEN ESTAR ANTES DE SER USADAS)
// ════════════════════════════════════════════════════════════

/**
 * ✅ Muestra mensaje cuando no hay pre-facturas
 */
function mostrarSinPreFacturas() {
    console.log('ℹ️ Mostrando mensaje: No hay pre-facturas disponibles');

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
// CARGAR PRE-FACTURAS (✅ VERSIÓN ÚNICA Y CORREGIDA)
// ════════════════════════════════════════════════════════════
/**
 * ✅ ACTUALIZADO v2.3: Obtiene las pre-facturas desde el servidor
 * CORREGIDO: Eliminada duplicación de código
 */
function cargarPreFacturas() {
    console.log('═══════════════════════════════════════════════════');
    console.log('📡 CARGAR PRE-FACTURAS DESDE SERVIDOR v2.3');
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

    console.log(`   URL: ${url}`);

    // ❹ Llamada AJAX
    $.ajax({
        url: url,
        type: 'POST',
        data: {
            solo_pendientes: soloPendientes
        },
        dataType: 'json',
        timeout: 15000,
        success: function (response) {
            console.log('═══════════════════════════════════════════════════');
            console.log('✅ RESPUESTA RECIBIDA DEL SERVIDOR');
            console.log('═══════════════════════════════════════════════════');
            console.log('Response completo:', response);

            // ✅ VALIDACIÓN ROBUSTA DE RESPUESTA
            if (!response || typeof response !== 'object') {
                console.error('❌ Respuesta inválida del servidor');
                mostrarErrorCargarPreFacturas('Respuesta inválida del servidor');
                return;
            }

            // ✅ VALIDAR PROPIEDAD 'ok'
            if (response.ok === false || response.ok === 'false') {
                console.warn(`⚠️ Error del servidor: ${response.mensaje}`);
                mostrarErrorCargarPreFacturas(response.mensaje || 'Error al cargar pre-facturas');
                return;
            }

            // ✅ VALIDAR ARRAY DE PRE-FACTURAS
            if (!response.prefacturas) {
                console.warn('⚠️ No se recibió el array de pre-facturas');
                mostrarSinPreFacturas();
                return;
            }

            if (!Array.isArray(response.prefacturas)) {
                console.error('❌ prefacturas no es un array:', typeof response.prefacturas);
                mostrarErrorCargarPreFacturas('Error en el formato de datos recibidos');
                return;
            }

            if (response.prefacturas.length === 0) {
                console.log('ℹ️ No hay pre-facturas disponibles');
                mostrarSinPreFacturas();
                return;
            }

            // ❺ ÉXITO: Guardar y renderizar
            console.log(`✅ Se recibieron ${response.prefacturas.length} pre-facturas`);
            preFacturasDisponibles = response.prefacturas;
            renderizarPreFacturas(response.prefacturas);
        },
        error: function (xhr, status, error) {
            console.log('═══════════════════════════════════════════════════');
            console.error('❌ ERROR EN LLAMADA AJAX');
            console.error(`   Status: ${status}`);
            console.error(`   Error: ${error}`);
            console.error(`   HTTP Status: ${xhr.status}`);
            console.log('═══════════════════════════════════════════════════');

            // ✅ Usar interceptor de sesiones
            if (esSesionExpirada(xhr.status)) {
                console.warn('⚠️ Sesión expirada detectada');
                return;
            }

            // ✅ MENSAJES ESPECÍFICOS SEGÚN ERROR
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

            mostrarErrorCargarPreFacturas(mensajeError);
        }
    });
}

// ════════════════════════════════════════════════════════════
// RENDERIZAR PRE-FACTURAS
// ════════════════════════════════════════════════════════════
/**
 * ✅ CORREGIDO v2.3: Renderiza las pre-facturas en la tabla
 * ACTUALIZADO: Usa los campos correctos de la BD (cpf_*)
 */
function renderizarPreFacturas(prefacturas) {
    console.log('═══════════════════════════════════════════════════');
    console.log('📝 RENDERIZANDO PRE-FACTURAS v2.3');
    console.log(`   Total a renderizar: ${prefacturas.length}`);
    console.log('═══════════════════════════════════════════════════');

    const $tbody = $('#tbodyPreFacturas');
    $tbody.empty();

    let countExitosos = 0;
    let countErrores = 0;

    prefacturas.forEach(function (pf, index) {
        try {
            // ✅ VALIDACIÓN ROBUSTA DE CAMPOS
            if (!pf || typeof pf !== 'object') {
                console.warn(`⚠️ Pre-factura [${index}] es inválida:`, pf);
                countErrores++;
                return; // ← Continue en forEach
            }

            // ✅ NORMALIZACIÓN DE CAMPOS CON FALLBACK (usando cpf_*)
            const preId = pf.cpf_nro?.toString().trim() || `PF-${index}`;
            const cliente = pf.cpf_nombre?.trim() || 'Sin nombre';
            const documento = pf.cpf_documento?.toString().trim() || '-';
            const fecha = pf.cpf_fecha?.trim() || '-';
            const sector = pf.sec_desc?.trim() || 'Sin sector';

            // ✅ LOG DETALLADO (solo primeras 3)
            if (index < 3) {
                console.log(`   [${index}] ID: ${preId} | Cliente: ${cliente}`);
            }

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
            countExitosos++;

        } catch (ex) {
            console.error(`❌ Error al renderizar pre-factura [${index}]:`, ex);
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
        mostrarSinPreFacturas();
    }
}

// ════════════════════════════════════════════════════════════
// SELECCIÓN DE PRE-FACTURAS
// ════════════════════════════════════════════════════════════
/**
 * ✅ CORREGIDO v2.3: Selecciona una pre-factura
 * ACTUALIZADO: Busca por cpf_nro en lugar de pre_id
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

    // ③ Buscar datos completos (usando cpf_nro)
    const prefactura = preFacturasDisponibles.find(pf => pf.cpf_nro === preId);

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
// CONFIRMAR PRE-FACTURAS
// ════════════════════════════════════════════════════════════
/**
 * ✅ ACTUALIZADO v2.3: Confirma múltiples pre-facturas seleccionadas
 * NUEVO: Soporte para carga múltiple de pre-facturas
 */
function confirmarPreFactura() {
    console.log('═══════════════════════════════════════════════════');
    console.log('✅ CONFIRMAR PRE-FACTURAS SELECCIONADAS v2.3');
    console.log('═══════════════════════════════════════════════════');

    // ❶ VALIDACIÓN ROBUSTA: Obtener filas seleccionadas del DOM
    const $filasSeleccionadas = $('#tbodyPreFacturas tr.selected-prefactura, #tbodyPreFacturas .chk-prefactura:checked').closest('tr');
    
    console.log(`   Filas seleccionadas en DOM: ${$filasSeleccionadas.length}`);

    // ❷ VALIDACIÓN: ¿Hay selección?
    if ($filasSeleccionadas.length === 0) {
        console.error('❌ No hay pre-facturas seleccionadas');
        mostrarMensajeError('Debe seleccionar al menos una pre-factura');
        return;
    }

    // ❸ EXTRAER cpf_nros DEL DOM (más robusto que usar variable global)
    const cpf_nros = [];
    
    $filasSeleccionadas.each(function() {
        const cpfNro = $(this).data('pre-id'); // ← data-pre-id contiene cpf_nro
        
        if (cpfNro && cpfNro !== 'undefined' && cpfNro !== 'null') {
            cpf_nros.push(cpfNro.toString().trim());
            console.log(`   ✅ Pre-factura agregada: ${cpfNro}`);
        } else {
            console.warn('⚠️ Fila sin cpf_nro válido:', $(this).html());
        }
    });

    // ❹ VALIDACIÓN FINAL
    if (cpf_nros.length === 0) {
        console.error('❌ No se pudo extraer ningún cpf_nro válido');
        mostrarMensajeError('Error al procesar pre-facturas seleccionadas');
        return;
    }

    console.log(`📋 Total pre-facturas válidas: ${cpf_nros.length}`);
    console.log(`   CPF_NROs: ${cpf_nros.join(', ')}`);

    // ❺ CERRAR MODAL
    cerrarModalPreFacturas();

    // ❻ INVOCAR ENDPOINT PARA CARGAR MÚLTIPLES PRE-FACTURAS
    cargarProductosDePrefacturas(cpf_nros);
}

// ════════════════════════════════════════════════════════════
// CARGAR PRODUCTOS DE PRE-FACTURAS
// ════════════════════════════════════════════════════════════
/**
 * ✅ NUEVO v2.3: Carga productos de múltiples pre-facturas
 * Invoca el endpoint ObtenerProductosDatosPrefactura
 */
function cargarProductosDePrefacturas(cpf_nros) {
    console.log('═══════════════════════════════════════════════════');
    console.log('📦 CARGAR PRODUCTOS DE PRE-FACTURAS v2.3');
    console.log(`   Total a procesar: ${cpf_nros.length}`);
    console.log('═══════════════════════════════════════════════════');

    // ❶ VALIDACIÓN DE ENTRADA
    if (!cpf_nros || !Array.isArray(cpf_nros) || cpf_nros.length === 0) {
        console.error('❌ Lista de cpf_nros inválida');
        mostrarMensajeError('No hay pre-facturas para procesar');
        return;
    }

    // ❷ URL DEL ENDPOINT
    const url = typeof ObtenerProductosDatosPrefacturaUrl !== 'undefined' && ObtenerProductosDatosPrefacturaUrl
        ? ObtenerProductosDatosPrefacturaUrl
        : '/Facturacion/ProductoFact/ObtenerProductosDatosPrefactura';

    console.log(`   URL: ${url}`);

    // ❸ MOSTRAR LOADER
    mostrarLoaderCalculando(); // ← Reutilizar loader de prodfact.js

    // ❹ LLAMADA AJAX
    $.ajax({
        url: url,
        type: 'POST',
        contentType: 'application/json',
        data: JSON.stringify(cpf_nros), // ← Enviar array directo (no objeto)
        dataType: 'json',
        timeout: 30000, // 30 segundos (puede haber múltiples pre-facturas)
        success: function(response) {
            console.log('═══════════════════════════════════════════════════');
            console.log('✅ RESPUESTA RECIBIDA - PRODUCTOS DE PRE-FACTURAS');
            console.log('═══════════════════════════════════════════════════');
            console.log('Response:', response);

            ocultarLoaderCalculando();

            // ❺ VALIDACIÓN DE RESPUESTA
            if (!response || typeof response !== 'object') {
                console.error('❌ Respuesta inválida del servidor');
                mostrarMensajeError('Respuesta inválida del servidor');
                return;
            }

            if (!response.ok) {
                console.error('❌ Error del servidor:', response.mensaje);
                mostrarMensajeError(response.mensaje || 'Error al cargar productos');
                return;
            }

            // ❻ VALIDAR QUE HAYA PRODUCTOS
            if (!response.producto || !Array.isArray(response.producto) || response.producto.length === 0) {
                console.warn('⚠️ No se recibieron productos');
                mostrarMensajeAdvertencia('No se encontraron productos en las pre-facturas seleccionadas');
                return;
            }

            // ❼ PROCESAR PRODUCTOS (igual que en prodfact.js)
            console.log(`✅ ${response.producto.length} productos recibidos`);
            
            // Iterar y agregar cada producto a la grilla
            response.producto.forEach((producto, index) => {
                console.log(`   [${index + 1}] ${producto.p_desc} - Cant: ${producto.cantidad_tot}`);
                
                // Agregar a grilla usando función de prodfact.js
                agregarProductoAGrilla(producto);
            });

            // ❽ MENSAJE DE ÉXITO
            const mensaje = response.errores && response.errores.length > 0
                ? `${response.mensaje}\n\nErrores:\n${response.errores.join('\n')}`
                : response.mensaje;

            mostrarMensajeExito(mensaje);

            console.log('═══════════════════════════════════════════════════');
            console.log('✅ PRODUCTOS DE PRE-FACTURAS CARGADOS EXITOSAMENTE');
            console.log('═══════════════════════════════════════════════════');
        },
        error: function(xhr, status, error) {
            console.log('═══════════════════════════════════════════════════');
            console.error('❌ ERROR AL CARGAR PRODUCTOS DE PRE-FACTURAS');
            console.error(`   Status: ${status}`);
            console.error(`   Error: ${error}`);
            console.error(`   HTTP Status: ${xhr.status}`);
            console.log('═══════════════════════════════════════════════════');

            ocultarLoaderCalculando();

            // Usar interceptor de sesiones
            if (esSesionExpirada(xhr.status)) {
                manejarSesionExpirada('No se pudieron cargar las pre-facturas porque su sesión ha expirado.');
                return;
            }

            let mensajeError = 'Error al cargar productos de pre-facturas. Por favor, intente nuevamente.';

            if (xhr.status === 500) {
                mensajeError = 'Error interno del servidor. Contacte al administrador.';
            } else if (xhr.status === 404) {
                mensajeError = 'Servicio no encontrado. Verifique la configuración.';
            } else if (status === 'timeout') {
                mensajeError = 'Se agotó el tiempo de espera. Intente nuevamente.';
            }

            mostrarMensajeError(mensajeError);
        }
    });
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