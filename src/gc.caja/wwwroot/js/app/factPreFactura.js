// ════════════════════════════════════════════════════════════
// GESTOR DE PRE-FACTURAS
// ════════════════════════════════════════════════════════════
// VERSIÓN v2.4 - Mensajes unificados con mostrarMensajeEstado
// ════════════════════════════════════════════════════════════

// ════════════════════════════════════════════════════════════
// VARIABLES GLOBALES
// ════════════════════════════════════════════════════════════
// ✅ AHORA (selección múltiple)
let preFacturasSeleccionadas = [];  // ← Array de pre-facturas seleccionadas
let preFacturasDisponibles = [];

// ════════════════════════════════════════════════════════════
// INICIALIZACIÓN
// ════════════════════════════════════════════════════════════
$(function () {
    console.log('📄 Módulo de Pre-Facturas inicializado v2.4 UNIFICADO');
    inicializarEventosPreFacturas();
});

// ════════════════════════════════════════════════════════════
// EVENTOS - ACTUALIZADO v3.2
// ════════════════════════════════════════════════════════════
function inicializarEventosPreFacturas() {
    console.log('🔧 Configurando eventos de pre-facturas v3.2...');

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

    // ✅ CORREGIDO v3.2: Click en fila (excluye checkbox y su celda)
    $(document).on('click', '#tbodyPreFacturas tr:not(#rowSinPreFacturas)', function (e) {
        // ⚠️ CRÍTICO: Ignorar click en checkbox o su celda contenedora
        if ($(e.target).is('input[type="checkbox"]') ||
            $(e.target).hasClass('td-checkbox') ||
            $(e.target).closest('.td-checkbox').length > 0) {
            console.log('🚫 Click en checkbox ignorado por evento de fila');
            return; // ← Salir sin procesar
        }

        const preId = $(this).data('pre-id');
        if (preId) {
            console.log('📋 Click en fila (no checkbox):', preId);
            seleccionarPreFactura(preId);
        }
    });

    // ✅ CORREGIDO v3.2: Checkbox individual (sin interferencia)
    $(document).on('change', '#tbodyPreFacturas .chk-prefactura', function (e) {
        e.stopPropagation(); // ← Prevenir burbujeo al evento de fila

        const preId = $(this).closest('tr').data('pre-id');
        console.log(`☑️ Checkbox individual ${preId} - Nuevo estado:`, $(this).is(':checked'));

        seleccionarPreFactura(preId);
    });

    // Botón Cancelar
    $('#btnCancelarPreFactura').on('click', function () {
        console.log('❌ Cancelar selección de pre-facturas');
        cerrarModalPreFacturas();
    });

    // Botón SELECCIONAR
    $('#btnSeguirPreFactura').on('click', function () {
        console.log('✅ Confirmar pre-facturas seleccionadas');
        confirmarPreFacturas();
    });

    console.log('✅ Eventos de pre-facturas v3.2 configurados');
}

// ════════════════════════════════════════════════════════════
// ABRIR MODAL - ACTUALIZADO v3.2
// ════════════════════════════════════════════════════════════

/**
 * ✅ ACTUALIZADO v3.2: Abre el modal con soporte para selección múltiple
 * ACTUALIZADO v2.4: Uso de mostrarMensajeEstado
 */
function abrirModalPreFacturas() {
    console.log('═══════════════════════════════════════════════════');
    console.log('📄 ABRIR MODAL PRE-FACTURAS v2.4 (Múltiple)');
    console.log('═══════════════════════════════════════════════════');

    // ❶ Validar que haya cliente seleccionado
    if (!clienteActualFactura) {
        console.error('❌ No hay cliente seleccionado');
        // ✅ ACTUALIZADO v2.4: Uso de mostrarMensajeEstado
        mostrarMensajeEstado('Debe identificar un cliente antes de cargar una pre-factura', 'danger');
        return;
    }

    console.log('   Cliente actual:', clienteActualFactura.denominacion);

    // ❷ Resetear selección múltiple
    preFacturasSeleccionadas = [];
    $('#btnSeguirPreFactura')
        .prop('disabled', true)
        .html(`<i class='bx bx-check-circle'></i> SELECCIONAR`);

    $('#chkSeleccionarTodos').prop('checked', false).prop('indeterminate', false);

    // ❸ Mostrar modal
    $('#modalPreFacturas').modal('show');

    // ❹ Cargar pre-facturas
    cargarPreFacturas();
}

// ════════════════════════════════════════════════════════════
// SECCIÓN: HELPERS (DEBEN ESTAR ANTES DE SER USADAS)
// ════════════════════════════════════════════════════════════

/**
 * ✅ ACTUALIZADO v3.2: colspan ajustado a 6 columnas
 */
function mostrarSinPreFacturas() {
    console.log('ℹ️ Mostrando mensaje: No hay pre-facturas disponibles');

    $('#tbodyPreFacturas').html(`
        <tr id="rowSinPreFacturas">
            <td colspan="6" class="text-center text-muted py-5">
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
 * ✅ ACTUALIZADO v3.2: colspan ajustado a 6 columnas
 */
function mostrarErrorCargarPreFacturas(mensaje) {
    console.error('❌ Error al cargar pre-facturas:', mensaje);

    $('#tbodyPreFacturas').html(`
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
 * ✅ ACTUALIZADO v3.2: Renderiza sin columna "Acción"
 */
function renderizarPreFacturas(prefacturas) {
    console.log('═══════════════════════════════════════════════════');
    console.log('📝 RENDERIZANDO PRE-FACTURAS v3.2');
    console.log(`   Total a renderizar: ${prefacturas.length}`);
    console.log('═══════════════════════════════════════════════════');

    const $tbody = $('#tbodyPreFacturas');
    $tbody.empty();

    let countExitosos = 0;
    let countErrores = 0;

    prefacturas.forEach(function (pf, index) {
        try {
            // Validación
            if (!pf || typeof pf !== 'object') {
                console.warn(`⚠️ Pre-factura [${index}] es inválida:`, pf);
                countErrores++;
                return;
            }

            // Normalización de campos
            const preId = pf.cpf_nro?.toString().trim() || `PF-${index}`;
            const cliente = pf.cpf_nombre?.trim() || 'Sin nombre';
            const documento = pf.cpf_documento?.toString().trim() || '-';
            const fecha = pf.cpf_fecha?.trim() || '-';
            const sector = pf.sec_desc?.trim() || 'Sin sector';

            if (index < 3) {
                console.log(`   [${index}] ID: ${preId} | Cliente: ${cliente}`);
            }

            // ✅ ACTUALIZADO v3.2: Sin columna "Acción", clase "td-checkbox" añadida
            const row = `
                <tr data-pre-id="${preId}" 
                    data-index="${index}"
                    class="prefactura-row">
                    <td class="text-center fw-bold">${escapeHtml(preId)}</td>
                    <td>${escapeHtml(cliente)}</td>
                    <td class="text-center">${escapeHtml(documento)}</td>
                    <td class="text-center">${escapeHtml(fecha)}</td>
                    <td>${escapeHtml(sector)}</td>
                    <td class="text-center td-checkbox">
                        <input type="checkbox" 
                               class="form-check-input chk-prefactura"
                               data-pre-id="${preId}"
                               title="Seleccionar pre-factura">
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

    if (countExitosos === 0) {
        mostrarSinPreFacturas();
    }
}

// ════════════════════════════════════════════════════════════
// SELECCIÓN DE PRE-FACTURAS - VERSIÓN MÚLTIPLE v3.0
// ════════════════════════════════════════════════════════════

/**
 * ✅ NUEVO v3.0: Permite selección múltiple de pre-facturas
 * @param {string} preId - cpf_nro de la pre-factura
 */
function seleccionarPreFactura(preId) {
    console.log(`📋 Toggle selección pre-factura: ${preId}`);

    // ❶ Obtener la fila
    const $row = $(`#tbodyPreFacturas tr[data-pre-id="${preId}"]`);

    if (!$row.length) {
        console.error('❌ Fila no encontrada');
        return;
    }

    // ❷ Verificar si ya está seleccionada
    const estaSeleccionada = $row.hasClass('selected-prefactura');

    if (estaSeleccionada) {
        // ═══ DESELECCIONAR ═══
        console.log(`   ➖ Deseleccionando: ${preId}`);

        // Remover visualmente
        $row.removeClass('selected-prefactura');
        $row.find('.chk-prefactura').prop('checked', false);

        // Remover del array
        preFacturasSeleccionadas = preFacturasSeleccionadas.filter(
            pf => pf.cpf_nro?.toString() !== preId.toString()
        );

    } else {
        // ═══ SELECCIONAR ═══
        console.log(`   ➕ Seleccionando: ${preId}`);

        // Buscar objeto completo
        const prefactura = preFacturasDisponibles.find(
            pf => pf.cpf_nro?.toString() === preId.toString()
        );

        if (!prefactura) {
            console.error('❌ Pre-factura no encontrada en array disponible');
            return;
        }

        // Añadir visualmente
        $row.addClass('selected-prefactura');
        $row.find('.chk-prefactura').prop('checked', true);

        // Añadir al array (evitar duplicados)
        const yaExiste = preFacturasSeleccionadas.some(
            pf => pf.cpf_nro?.toString() === preId.toString()
        );

        if (!yaExiste) {
            preFacturasSeleccionadas.push(prefactura);
        }
    }

    // ❸ Actualizar UI
    actualizarEstadoSeleccion();

    console.log(`✅ Total seleccionadas: ${preFacturasSeleccionadas.length}`);
}

/**
 * ✅ ACTUALIZADO v3.1: Texto cambiado a "SELECCIONAR"
 */
function actualizarEstadoSeleccion() {
    const totalSeleccionadas = preFacturasSeleccionadas.length;

    console.log(`🔄 Actualizando UI - ${totalSeleccionadas} seleccionadas`);

    // ❶ Habilitar/deshabilitar botón SELECCIONAR
    $('#btnSeguirPreFactura').prop('disabled', totalSeleccionadas === 0);

    // ❷ ✅ ACTUALIZADO v3.1: Texto "SELECCIONAR" con contador
    const textoBoton = totalSeleccionadas > 0
        ? `<i class='bx bx-check-circle'></i> SELECCIONAR (${totalSeleccionadas})`
        : `<i class='bx bx-check-circle'></i> SELECCIONAR`;

    $('#btnSeguirPreFactura').html(textoBoton);

    // ❸ Sincronizar checkbox "Seleccionar Todos"
    const totalDisponibles = $('#tbodyPreFacturas tr[data-pre-id]').length;
    const todosMarcados = totalSeleccionadas === totalDisponibles && totalDisponibles > 0;

    $('#chkSeleccionarTodos').prop('checked', todosMarcados);

    // ❹ Estado indeterminado
    if (totalSeleccionadas > 0 && !todosMarcados) {
        $('#chkSeleccionarTodos').prop('indeterminate', true);
    } else {
        $('#chkSeleccionarTodos').prop('indeterminate', false);
    }

    // ❺ Actualizar contador en header
    const $badge = $('#spanContadorSeleccion');
    if (totalSeleccionadas > 0) {
        $badge.text(`${totalSeleccionadas} seleccionada${totalSeleccionadas > 1 ? 's' : ''}`).show();
    } else {
        $badge.hide();
    }
}

/**
 * ✅ NUEVO v3.0: Deselecciona UNA pre-factura específica
 * @param {string} preId - cpf_nro de la pre-factura
 */
function deseleccionarPreFacturaIndividual(preId) {
    console.log(`➖ Deseleccionar individual: ${preId}`);

    // Remover visualmente
    const $row = $(`#tbodyPreFacturas tr[data-pre-id="${preId}"]`);
    $row.removeClass('selected-prefactura');
    $row.find('.chk-prefactura').prop('checked', false);

    // Remover del array
    preFacturasSeleccionadas = preFacturasSeleccionadas.filter(
        pf => pf.cpf_nro?.toString() !== preId.toString()
    );

    // Actualizar UI
    actualizarEstadoSeleccion();
}

/**
 * ✅ ACTUALIZADO v3.0: Deselecciona TODAS las pre-facturas
 */
function deseleccionarPreFactura() {
    console.log('🔄 Deseleccionar TODAS las pre-facturas');

    $('#tbodyPreFacturas tr').removeClass('selected-prefactura');
    $('#tbodyPreFacturas .chk-prefactura').prop('checked', false);
    preFacturasSeleccionadas = [];

    actualizarEstadoSeleccion();
}

/**
 * ✅ NUEVO v3.0: Toggle seleccionar/deseleccionar TODOS
 * @param {boolean} checked - Estado del checkbox global
 */
function toggleSeleccionarTodos(checked) {
    console.log(`☑️ Seleccionar Todos: ${checked}`);

    if (checked) {
        // ═══ SELECCIONAR TODOS ═══
        console.log(`   Seleccionando ${preFacturasDisponibles.length} pre-facturas...`);

        // Limpiar selección actual
        preFacturasSeleccionadas = [];

        // Iterar sobre filas visibles
        $('#tbodyPreFacturas tr[data-pre-id]').each(function () {
            const $row = $(this);
            const preId = $row.data('pre-id');

            // Marcar visualmente
            $row.addClass('selected-prefactura');
            $row.find('.chk-prefactura').prop('checked', true);

            // Buscar objeto completo
            const prefactura = preFacturasDisponibles.find(
                pf => pf.cpf_nro?.toString() === preId.toString()
            );

            if (prefactura) {
                preFacturasSeleccionadas.push(prefactura);
            }
        });

    } else {
        // ═══ DESELECCIONAR TODOS ═══
        deseleccionarPreFactura();
    }

    actualizarEstadoSeleccion();

    console.log(`✅ Total seleccionadas: ${preFacturasSeleccionadas.length}`);
}

// ════════════════════════════════════════════════════════════
// CONFIRMAR PRE-FACTURAS - VERSIÓN MÚLTIPLE v3.0
// ════════════════════════════════════════════════════════════

/**
 * ✅ ACTUALIZADO v3.0: Confirma múltiples pre-facturas seleccionadas
 * Renombrado de singular a plural
 * ACTUALIZADO v2.4: Uso de mostrarMensajeEstado
 */
function confirmarPreFacturas() {
    console.log('═══════════════════════════════════════════════════');
    console.log('✅ CONFIRMAR PRE-FACTURAS SELECCIONADAS v2.4 UNIFICADO');
    console.log('═══════════════════════════════════════════════════');

    // ❶ VALIDACIÓN: ¿Hay selecciones?
    if (!preFacturasSeleccionadas || preFacturasSeleccionadas.length === 0) {
        console.error('❌ No hay pre-facturas seleccionadas');
        // ✅ ACTUALIZADO v2.4: Uso de mostrarMensajeEstado
        mostrarMensajeEstado('Debe seleccionar al menos una pre-factura', 'warning');
        return;
    }

    // ❷ EXTRAER cpf_nros del array en memoria (más confiable que DOM)
    const cpf_nros = preFacturasSeleccionadas
        .map(pf => pf.cpf_nro?.toString().trim())
        .filter(cpfNro => cpfNro && cpfNro !== 'undefined' && cpfNro !== 'null');

    console.log(`📋 Total pre-facturas válidas: ${cpf_nros.length}`);
    console.log(`   CPF_NROs: ${cpf_nros.join(', ')}`);

    // ❸ VALIDACIÓN FINAL
    if (cpf_nros.length === 0) {
        console.error('❌ No se pudo extraer ningún cpf_nro válido');
        // ✅ ACTUALIZADO v2.4: Uso de mostrarMensajeEstado
        mostrarMensajeEstado('Error al procesar pre-facturas seleccionadas', 'danger');
        return;
    }

    // ❹ CERRAR MODAL
    cerrarModalPreFacturas();

    // ❺ INVOCAR ENDPOINT PARA CARGAR MÚLTIPLES PRE-FACTURAS
    cargarProductosDePrefacturas(cpf_nros);
}

// ════════════════════════════════════════════════════════════
// CARGAR PRODUCTOS DE PRE-FACTURAS
// ════════════════════════════════════════════════════════════
/**
 * ✅ NUEVO v2.4: Carga productos de múltiples pre-facturas
 * Invoca el endpoint ObtenerProductosDatosPrefactura
 * ACTUALIZADO v2.4: Uso de mostrarMensajeEstado
 */
function cargarProductosDePrefacturas(cpf_nros) {
    console.log('═══════════════════════════════════════════════════');
    console.log('📦 CARGAR PRODUCTOS DE PRE-FACTURAS v2.4');
    console.log(`   Total a procesar: ${cpf_nros.length}`);
    console.log('═══════════════════════════════════════════════════');

    // ❶ VALIDACIÓN DE ENTRADA
    if (!cpf_nros || !Array.isArray(cpf_nros) || cpf_nros.length === 0) {
        console.error('❌ Lista de cpf_nros inválida');
        // ✅ ACTUALIZADO v2.4: Uso de mostrarMensajeEstado
        mostrarMensajeEstado('No hay pre-facturas para procesar', 'warning');
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
        success: function (response) {
            console.log('═══════════════════════════════════════════════════');
            console.log('✅ RESPUESTA RECIBIDA - PRODUCTOS DE PRE-FACTURAS');
            console.log('═══════════════════════════════════════════════════');
            console.log('Response:', response);

            ocultarLoaderCalculando();

            // ❺ VALIDACIÓN DE RESPUESTA
            if (!response || typeof response !== 'object') {
                console.error('❌ Respuesta inválida del servidor');
                mostrarMensajeEstado('Respuesta inválida del servidor', 'danger');
                return;
            }

            if (!response.ok) {
                console.error('❌ Error del servidor:', response.mensaje);
                mostrarMensajeEstado(response.mensaje || 'Error al cargar productos', 'danger');
                return;
            }

            // ❻ VALIDAR QUE HAYA PRODUCTOS
            if (!response.producto || !Array.isArray(response.producto) || response.producto.length === 0) {
                console.warn('⚠️ No se recibieron productos');
                // ✅ ACTUALIZADO v2.4: Uso de mostrarMensajeEstado
                mostrarMensajeEstado('No se encontraron productos en las pre-facturas seleccionadas', 'info', 7000);
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
                ? `${response.mensaje} - Errores: ${response.errores.join(', ')}`
                : response.mensaje;

            // ✅ ACTUALIZADO v2.4: Uso de mostrarMensajeEstado
            mostrarMensajeEstado(mensaje, 'success', 7000);

            console.log('═══════════════════════════════════════════════════');
            console.log('✅ PRODUCTOS DE PRE-FACTURAS CARGADOS EXITOSAMENTE');
            console.log('═══════════════════════════════════════════════════');
        },
        error: function (xhr, status, error) {
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

            // ✅ ACTUALIZADO v2.4: Uso de mostrarMensajeEstado (aunque esté en modal)
            mostrarMensajeEstado(mensajeError, 'danger', 7000);
        }
    });
}

// ════════════════════════════════════════════════════════════
// CERRAR MODAL - ACTUALIZADO v3.0
// ════════════════════════════════════════════════════════════

/**
 * ✅ ACTUALIZADO v3.1: Texto cambiado a "SELECCIONAR"
 */
function cerrarModalPreFacturas() {
    console.log('🔙 Cerrando modal de pre-facturas v3.1...');

    // Cerrar modal
    $('#modalPreFacturas').modal('hide');

    // Limpiar array múltiple
    preFacturasSeleccionadas = [];
    preFacturasDisponibles = [];

    // ✅ ACTUALIZADO v3.1: Restaurar con texto "SELECCIONAR"
    $('#btnSeguirPreFactura')
        .prop('disabled', true)
        .html(`<i class='bx bx-check-circle'></i> SELECCIONAR`);

    $('#chkSeleccionarTodos').prop('checked', false).prop('indeterminate', false);
    // ✅ REQUERIMIENTO: Restablecer el filtro "Solo Pendientes" al cerrar
    $('#chkSoloPendientes').prop('checked', true);

    console.log('✅ Modal cerrado y datos limpiados');
}