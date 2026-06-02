// ============================================
// GESTOR PRINCIPAL DEL MÓDULO DE FACTURACIÓN
// ============================================

// ====== VARIABLES GLOBALES ======
let clienteSeleccionado = null;
let modoEdicionCliente = false; // Control de modo edición
let busquedaEnProceso = false; // ✅ NUEVO: Control de búsquedas concurrentes
let ajaxActual = null; // ✅ NUEVO: Referencia al AJAX en curso para cancelación
// ✅ NUEVA VARIABLE DE CONFIGURACIÓN
let autoConfirmarClienteUnico = true; // ← Control del comportamiento
// ========================================
// FUNCIONES DE MENSAJES AL USUARIO
// ========================================

// ========================================
// FUNCIONES DE MENSAJES AL USUARIO
// ========================================

function mostrarMensajeError(mensaje) {
    console.error('💬 Mostrando mensaje de error al usuario');
    console.error(`   Mensaje: "${mensaje}"`);

    // ✅ INTEGRACIÓN CON SISTEMA DE MENSAJES DEL PROYECTO
    AbrirMensaje(
        "Error de Búsqueda", // ✅ CAMBIO: Título más específico y descriptivo
        mensaje,
        function () {
            $("#msjModal").modal("hide");
        },
        false, // No mostrar botón cancelar
        ["Aceptar"],
        "error!", // Tipo de icono
        null
    );
}

function mostrarMensajeExito(mensaje) {
    console.log('💬 Mostrando mensaje de éxito al usuario');
    console.log(`   Mensaje: "${mensaje}"`);

    // ✅ INTEGRACIÓN CON SISTEMA DE MENSAJES DEL PROYECTO
    AbrirMensaje(
        "Operación Exitosa", // ✅ CAMBIO: Título más específico
        mensaje,
        function () {
            $("#msjModal").modal("hide");
        },
        false, // No mostrar botón cancelar
        ["Aceptar"],
        "succ!", // Tipo de icono
        null
    );
}

function mostrarMensajeInformacion(mensaje) {
    console.log('💬 Mostrando mensaje de INFORMACIÓN al usuario');
    console.log(`   Mensaje: "${mensaje}"`);

    // ✅ INTEGRACIÓN CON SISTEMA DE MENSAJES DEL PROYECTO
    AbrirMensaje(
        "Información", // ✅ CAMBIO: Título más específico
        mensaje,
        function () {
            $("#msjModal").modal("hide");
        },
        false, // No mostrar botón cancelar
        ["Aceptar"],
        "info!", // Tipo de icono
        null
    );
}


// ========================================
// ✅ NUEVO: GESTIÓN DE ESTADO DE BÚSQUEDA
// ========================================

/**
 * Bloquea la interfaz de búsqueda para prevenir acciones concurrentes
 */
function bloquearInterfazBusqueda() {
    console.log('🔒 Bloqueando interfaz de búsqueda...');

    busquedaEnProceso = true;

    // Deshabilitar y proteger input
    $('#txtBuscarCliente')
        .prop('readonly', true)
        .addClass('pe-none') // Prevent pointer events
        .css('opacity', '0.6');

    // Deshabilitar botón de búsqueda
    $('#btnBuscarCliente')
        .prop('disabled', true)
        .html('<i class="bx bx-loader-alt bx-spin"></i> Buscando...');

    // Deshabilitar botones de acción
    $('#btnListaPrecios, #btnNuevoCliente').prop('disabled', true);

    console.log('✅ Interfaz bloqueada correctamente');
}

/**
 * Desbloquea la interfaz de búsqueda después de completar la operación
 */
function desbloquearInterfazBusqueda() {
    console.log('🔓 Desbloqueando interfaz de búsqueda...');

    busquedaEnProceso = false;
    ajaxActual = null;

    // Restaurar input
    $('#txtBuscarCliente')
        .prop('readonly', false)
        .removeClass('pe-none')
        .css('opacity', '1');

    // Restaurar botón de búsqueda
    $('#btnBuscarCliente')
        .prop('disabled', false)
        .html('<i class="bx bx-search"></i> Buscar');

    // Restaurar botones de acción según estado
    actualizarEstadoBotonesAccion();

    console.log('✅ Interfaz desbloqueada correctamente');
}

/**
 * Actualiza el estado de los botones de acción según el cliente seleccionado
 */
function actualizarEstadoBotonesAccion() {
    if (clienteSeleccionado) {
        $('#btnListaPrecios').prop('disabled', false);
        $('#btnNuevoCliente').prop('disabled', true);
    } else {
        $('#btnListaPrecios').prop('disabled', true);
        $('#btnNuevoCliente').prop('disabled', false);
    }
}

/**
 * Cancela la búsqueda AJAX en curso si existe
 */
function cancelarBusquedaActual() {
    if (ajaxActual) {
        console.log('⚠️ Cancelando búsqueda AJAX en curso...');
        ajaxActual.abort();
        ajaxActual = null;
    }
}

// ====== INICIALIZACIÓN ======
$(function () {
    console.log('🚀 Módulo de Facturación Cargado');

    // ✅ NUEVO: Inicializar lista de precios por defecto
    if (typeof admLp_id === 'undefined') {
        window.admLp_id = "001"; // Mayorista por defecto
        console.log('⚠️ admLp_id no estaba definida, se inicializó en "001"');
    }

    console.log(`✅ Lista de precios inicial: ${admLp_id}`);

    inicializaEventosFact();
    inicializaVistaFact();
});

// ====== EVENTOS PRINCIPALES ======
function inicializaEventosFact() {
    // ========================================
    // MODAL IDENTIFICAR CLIENTE
    // ========================================

    // Abrir modal identificar cliente (solo para pruebas o apertura manual)
    $('#btnAbrirIdentificarCliente').on('click', function () {
        abrirModalIdentificarCliente();
    });

    // Buscar cliente (botón)
    $('#btnBuscarCliente').on('click', function () {
        buscarCliente();
    });

    // ✅ MODIFICADO: Buscar cliente (Enter) - Con protección contra búsquedas concurrentes
    $('#txtBuscarCliente').on('keypress', function (e) {
        if (e.which === 13) {
            e.preventDefault();

            // ✅ NUEVO: Validar si hay una búsqueda en proceso
            if (busquedaEnProceso) {
                console.warn('⚠️ Búsqueda ya en proceso - Acción bloqueada');
                return;
            }

            const criterioBusqueda = $(this).val().trim();

            console.log('═══════════════════════════════════════════════════');
            console.log('⌨️ ENTER EN CAMPO BUSCAR CLIENTE v4.0');
            console.log(`   Criterio ingresado: "${criterioBusqueda}"`);
            console.log(`   busquedaEnProceso: ${busquedaEnProceso}`);

            if (criterioBusqueda === '') {
                console.log('✅ CASO 1: Campo vacío - Abrir modal Nuevo CF');
                abrirModalClienteNuevo();
            } else {
                console.log('✅ CASO 2: Campo con texto - Buscar cliente');
                buscarCliente();
            }
        }
    });

    // ✅ NUEVO: Prevenir edición durante búsqueda (seguridad adicional)
    $('#txtBuscarCliente').on('input', function (e) {
        if (busquedaEnProceso) {
            console.warn('⚠️ Intento de edición durante búsqueda - Bloqueado');
            $(this).val($(this).data('lastValue') || '');
            return false;
        }
        $(this).data('lastValue', $(this).val());
    });

    // Lista de precios
    $('#btnListaPrecios').on('click', function () {
        console.log('🛍️ Abrir Lista de Precios...');
        // TODO: Implementar lógica
    });

    // Nuevo cliente
    $('#btnNuevoCliente').on('click', function () {
        console.log('➕ Crear Nuevo Cliente...');
        abrirModalClienteNuevo();
    });

    // ✅ NUEVO: Salir al menú principal de caja
    $('#btnSalirFacturacion').on('click', function () {
        console.log('🚪 Usuario solicitó salir al menú principal...');
        confirmarSalidaAlMenu();
    });

    // Cancelar (solo limpia el modal)
    $('#btnCancelarCliente').on('click', function () {
        // ✅ NUEVO: Cancelar búsqueda en curso antes de limpiar
        if (busquedaEnProceso) {
            cancelarBusquedaActual();
            desbloquearInterfazBusqueda();
        }
        limpiarModalCliente();
    });

    // Seguir (confirmar cliente)
    $('#btnSeguirCliente').on('click', function () {
        if (clienteSeleccionado) {
            confirmarCliente(clienteSeleccionado);
        }
    });

    // ========================================
    // PREVENIR CIERRE ACCIDENTAL DEL MODAL
    // ========================================

    $('#modalIdentificarCliente').on('hide.bs.modal', function (e) {
        const disparadorId = e.relatedTarget ? e.relatedTarget.id : null;
        const cierresPermitidos = ['btnCancelarCliente', 'btnSalirFacturacion'];
        const esCierrePermitido = cierresPermitidos.includes(disparadorId);

        // ══════════════════════════════════════════════════════════════════════
        // ✅ CORREGIDO v2.0: Permitir cierre si hay cliente seleccionado
        // ══════════════════════════════════════════════════════════════════════
        // PROBLEMA ORIGINAL:
        //   - Bloqueaba cierre incluso cuando búsqueda terminó exitosamente
        //   - Validaba busquedaEnProceso ANTES de AJAX complete
        //   - Causaba mensaje "Espere a que finalice la búsqueda" innecesario
        //
        // SOLUCIÓN:
        //   - Si clienteSeleccionado existe, la búsqueda fue exitosa
        //   - Permitir cierre aunque busquedaEnProceso = true
        //   - Solo bloquear si búsqueda en curso SIN resultado
        // ══════════════════════════════════════════════════════════════════════

        if (busquedaEnProceso && !clienteSeleccionado) {
            // ✅ SOLO bloquear si:
            //    - Hay búsqueda en proceso Y
            //    - NO hay cliente seleccionado (búsqueda no completada exitosamente)
            e.preventDefault();
            console.warn('⚠️ No se puede cerrar el modal durante una búsqueda en curso');
            mostrarMensajeError('Espere a que finalice la búsqueda en curso');
            return;
        }

        // ✅ CASO ESPECIAL: Si hay cliente, la búsqueda fue exitosa
        // Permitir cierre aunque busquedaEnProceso = true (complete aún no ejecutado)
        if (clienteSeleccionado) {
            console.log('✅ Cierre permitido: Cliente seleccionado exitosamente');
            console.log(`   Cliente: ${clienteSeleccionado.denominacion || 'N/A'}`);
            console.log(`   busquedaEnProceso: ${busquedaEnProceso} (ignorado porque hay cliente)`);
            return; // ← Permitir cierre inmediatamente
        }

        // ✅ Validación de cierre no autorizado (solo si NO hay cliente)
        if (!esCierrePermitido && !clienteSeleccionado) {
            e.preventDefault();
            console.warn('⚠️ Cierre no autorizado - Debe seleccionar un cliente o usar CANCELAR/SALIR');
        }
    });

    // ========================================
    // MODAL CLIENTE UPDATE (NUEVO/EDITAR)
    // ========================================

    // Validación en tiempo real del select
    $('#selTipoDocumento, #selSexoCliente').on('change', function () {
        validarCampo($(this));
        if ($(this).attr('id') === 'selTipoDocumento') {
            ajustarPlaceholderSegunTipo();
        }
    });

    // Validación en tiempo real de inputs
    $('#txtNumeroDocumento, #txtApellidoCliente, #txtNombreCliente, #txtEmailCliente, #txtMovilCliente').on('input', function () {
        validarCampo($(this));
    });

    // Formatear número de documento y móvil (solo números)
    $('#txtNumeroDocumento, #txtMovilCliente').on('input', function () {
        let valor = $(this).val().replace(/\D/g, '');
        $(this).val(valor);
    });

    // Validar email al perder foco
    $('#txtEmailCliente').on('blur', function () {
        validarEmail($(this));
    });

    // Cancelar modal cliente update
    $('#btnCancelarClienteUpdate').on('click', function () {
        cerrarModalClienteUpdate();
    });

    // Guardar cliente (submit del form)
    $('#formClienteUpdate').on('submit', function (e) {
        e.preventDefault();

        if (validarFormularioCliente()) {
            guardarCliente();
        }
    });

    // ✅ NUEVO: Editar cliente (solo para Consumidores Finales)
    $('#btnEditarCliente').on('click', function () {
        console.log('✏️ Editar Consumidor Final...');
        abrirModalClienteEditar();
    });

    // ✅ INTEGRACIÓN CON MÓDULO DE PRODUCTOS DE FACTURACIÓN
    $(document).on('volverAIdentificarCliente', function () {
        console.log('📡 EVENTO RECIBIDO: volverAIdentificarCliente');
        setTimeout(() => {
            abrirModalIdentificarCliente();
        }, 400);
    });
}

// ====== INICIALIZACIÓN DE VISTA ======
function inicializaVistaFact() {
    console.log('🚀 Inicializando módulo de Facturación...');

    setTimeout(() => {
        abrirModalIdentificarCliente();
        console.log('✅ Modal de Identificar Cliente abierto automáticamente');
    }, 300);
}

// ========================================
// MODAL IDENTIFICAR CLIENTE - FUNCIONES
// ========================================

function abrirModalIdentificarCliente() {
    // Resetear estado
    limpiarModalCliente();

    // ✅ SOLUCIÓN SIMPLE: Usar jQuery para mostrar el modal
    $('#modalIdentificarCliente').modal('show');

    // Focus en campo de búsqueda
    setTimeout(() => {
        $('#txtBuscarCliente').trigger("focus");
    }, 500);
}

/**
 * ✅ MODIFICADO: Limpia completamente el modal de identificar cliente
 * 
 * Esta función se ejecuta cuando el usuario hace clic en CANCELAR.
 * Debe restaurar el modal al estado inicial (#1) independientemente
 * del estado actual.
 * 
 * Estados que maneja:
 * - Estado 1: Inicial (sin búsqueda)
 * - Estado 2: Con texto sin buscar
 * - Estado 3: Cliente único encontrado
 * - Estado 4: Grilla de múltiples clientes
 * - Estado 5: Cargando desde grilla (con loader)
 * - Estado 6: Cliente cargado desde grilla
 */
function limpiarModalCliente() {
    console.log('═══════════════════════════════════════════════════');
    console.log('🧹 LIMPIAR MODAL CLIENTE - INICIO');
    console.log('═══════════════════════════════════════════════════');
    console.log('Estado actual antes de limpiar:');
    console.log('   - txtBuscarCliente:', $('#txtBuscarCliente').val());
    console.log('   - clienteSeleccionado:', clienteSeleccionado);
    console.log('   - busquedaEnProceso:', busquedaEnProceso);
    console.log('   - cardDatosCliente visible:', $('#cardDatosCliente').is(':visible'));
    console.log('   - cardGrillaClientes existe:', $('#cardGrillaClientes').length > 0);
    console.log('   - loaderClienteTemp existe:', $('#loaderClienteTemp').length > 0);

    // ✅ NUEVO: Cancelar búsqueda en curso
    if (busquedaEnProceso) {
        cancelarBusquedaActual();
    }

    // ❶ LIMPIAR VARIABLE GLOBAL
    clienteSeleccionado = null;
    console.log('✅ Variable global clienteSeleccionado limpiada');

    // ❷ LIMPIAR CAMPO DE BÚSQUEDA
    $('#txtBuscarCliente').val('').removeData('lastValue');
    console.log('✅ Campo de búsqueda limpiado');

    // ❸ LIMPIAR VALORES DE LOS INPUTS (sin eliminar el HTML)
    $('#txtNombre, #txtClienteId, #txtDomicilio, #txtCondicionAfip, #txtTipoNumero, #txtEmite, #txtEmail, #txtMovil').val('');
    console.log('✅ Valores de inputs limpiados');

    // ❹ OCULTAR CARD DE DATOS DEL CLIENTE
    $('#cardDatosCliente')
        .removeClass('show')
        .hide();
    console.log('✅ Card de datos oculto');

    // ❺ MOSTRAR EL CARD-BODY ORIGINAL (si estaba oculto por el loader)
    const $cardBody = $('#cardDatosCliente .card-body');
    if ($cardBody.length > 0) {
        $cardBody.show();
        console.log('✅ Card-body restaurado (si estaba oculto)');
    }

    // ❻ ELIMINAR LOADER TEMPORAL (si existe)
    const $loader = $('#loaderClienteTemp');
    if ($loader.length > 0) {
        $loader.remove();
        console.log('✅ Loader temporal eliminado');
    }

    // ❼ LIMPIAR Y OCULTAR GRILLA DE MÚLTIPLES CLIENTES
    const $grilla = $('#cardGrillaClientes');
    if ($grilla.length > 0) {
        $grilla
            .removeClass('show')
            .hide()
            .empty(); // Vaciar contenido HTML
        console.log('✅ Grilla de clientes limpiada y ocultada');
    }

    // ❽ MOSTRAR ALERT DE "SIN CLIENTE"
    $('#alertSinCliente')
        .removeClass('hide')
        .show();
    console.log('✅ Alert "sin cliente" mostrado');

    // ❾ DESHABILITAR BOTÓN SEGUIR
    $('#btnSeguirCliente').prop('disabled', true);
    console.log('✅ Botón SEGUIR deshabilitado');

    // ✅ NUEVO: Ocultar botón EDITAR
    $('#btnEditarCliente').hide();
    console.log('✅ Botón EDITAR ocultado');

    // ✅ NUEVO: Desbloquear interfaz
    desbloquearInterfazBusqueda();


    // ❿ LIMPIAR SESIÓN DEL SERVIDOR
    limpiarSesionClientesBuscados();

    console.log('═══════════════════════════════════════════════════');
    console.log('✅ LIMPIAR MODAL CLIENTE - FINALIZADO');
    console.log('═══════════════════════════════════════════════════');
}

// ====== BÚSQUEDA DE CLIENTE ======
/**
 * ✅ MODIFICADO v4.0: Búsqueda de cliente con protección completa contra concurrencia
 */
function buscarCliente() {
    console.log('═══════════════════════════════════════════════════');
    console.log('🔍 BUSCAR CLIENTE v4.0');
    console.log('═══════════════════════════════════════════════════');

    // ✅ NUEVO: Validar si ya hay una búsqueda en proceso
    if (busquedaEnProceso) {
        console.warn('⚠️ Ya hay una búsqueda en proceso - Solicitud rechazada');
        mostrarMensajeError('Ya hay una búsqueda en proceso. Por favor espere.');
        return;
    }

    const criterioBusqueda = $('#txtBuscarCliente').val().trim();
    console.log(`   Criterio: "${criterioBusqueda}"`);

    if (!criterioBusqueda) {
        console.warn('⚠️ Criterio de búsqueda vacío');
        mostrarMensajeError('Por favor, ingrese CUIT, DNI o ID del cliente');
        return;
    }

    // ✅ NUEVO: Bloquear interfaz ANTES de iniciar AJAX
    bloquearInterfazBusqueda();

    const url = typeof BuscarClienteUrl !== 'undefined' && BuscarClienteUrl
        ? BuscarClienteUrl
        : '/Facturacion/Cliente/BuscarCliente';

    console.log(`📡 Iniciando búsqueda en: ${url}`);

    // ✅ NUEVO: Guardar referencia al AJAX para posible cancelación
    ajaxActual = $.ajax({
        url: url,
        type: 'POST',
        data: { criterio: criterioBusqueda },
        timeout: 30000, // ✅ NUEVO: Timeout de 30 segundos
        success: function (response) {
            console.log('✅ Respuesta recibida del servidor');
            console.log('   response.ok:', response.ok);
            console.log('   cantidadResultados:', response.cantidadResultados);

            if (response.ok) {
                const cantidadResultados = response.cantidadResultados || 0;
                $("#txtBuscarCliente").val("");

                // ══════════════════════════════════════════════════════
                // ✅ MODIFICACIÓN: Lógica de confirmación automática
                // ══════════════════════════════════════════════════════
                if (cantidadResultados === 1 && response.cliente) {
                    console.log('✅ Cliente único encontrado');

                    // ✅ DECISIÓN: ¿Auto-confirmar o mostrar?
                    if (autoConfirmarClienteUnico) {
                        console.log('⚡ Modo AUTO-CONFIRMACIÓN activado');
                        confirmarClienteAutomaticamente(response.cliente);
                    } else {
                        console.log('👁️ Modo MANUAL activado - Mostrar datos');
                        mostrarDatosCliente(response.cliente);
                    }
                }
                // ══════════════════════════════════════════════════════
                // ✅ SIN CAMBIOS: Lógica de múltiples resultados
                // ══════════════════════════════════════════════════════
                else if (cantidadResultados > 1) {
                    console.log(`✅ Múltiples clientes encontrados: ${cantidadResultados}`);
                    cargarGrillaClientes();
                }
                // ══════════════════════════════════════════════════════
                // ✅ SIN CAMBIOS: Lógica de sin resultados
                // ══════════════════════════════════════════════════════
                else {
                    console.warn('⚠️ No se encontraron clientes');
                    mostrarMensajeInformacion('No se encontraron clientes');
                    limpiarVista();
                }
            } else {
                console.info('❌ Error en respuesta:', response.mensaje);
                mostrarMensajeInformacion(response.mensaje || 'Cliente no encontrado');
                limpiarVista();
            }
        },
        error: function (xhr, status, error) {
            console.error('❌ ERROR AJAX');
            console.error('   Status:', status);
            console.error('   HTTP Status:', xhr.status);
            console.error('   Error:', error);

            // ✅ NUEVO: No mostrar error si fue cancelación manual
            if (status === 'abort') {
                console.log('⚠️ Búsqueda cancelada por el usuario');
                return;
            }

            let mensaje = 'Error al buscar el cliente';

            // ✅ NUEVO: Usar función centralizada
            if (esSesionExpirada(xhr.status)) {
                manejarSesionExpirada();
                return;
            }

            if (status === 'timeout') {
                mensaje = 'La búsqueda tardó demasiado tiempo. Intente nuevamente.';
            } else if (xhr.status === 404) {
                mensaje = 'Servicio no encontrado';
            } else if (xhr.status === 500) {
                mensaje = 'Error interno del servidor';
            }

            mostrarMensajeError(mensaje);
            limpiarVista();
        },
        complete: function (xhr, status) {
            console.log('🏁 Búsqueda finalizada');
            console.log('   Status:', status);

            // ✅ NUEVO: Desbloquear interfaz SIEMPRE (éxito o error)
            desbloquearInterfazBusqueda();

            console.log('═══════════════════════════════════════════════════');
        }
    });
}



// ====== CARGAR GRILLA DE CLIENTES (AJAX) ======
/**
 * ✅ MODIFICADO v2.0: Cargar grilla con protección de interfaz
 */
function cargarGrillaClientes() {
    console.log('═══════════════════════════════════════════════════');
    console.log('📋 CARGAR GRILLA DE CLIENTES v2.0');
    console.log('═══════════════════════════════════════════════════');

    $('#cardDatosCliente').removeClass('show').hide();
    $('#alertSinCliente').addClass('hide').hide();

    const urlTraerGrilla = typeof TraerGrillaClientesUrl !== 'undefined' && TraerGrillaClientesUrl
        ? TraerGrillaClientesUrl
        : '/Facturacion/Cliente/TraerGrillaClientes';

    console.log(`📡 URL de grilla: ${urlTraerGrilla}`);

    if ($('#cardGrillaClientes').length === 0) {
        $('#alertSinCliente').after('<div class="card card-golden" id="cardGrillaClientes"></div>');
        console.log('✅ Contenedor de grilla creado');
    }

    $('#cardGrillaClientes').html(`
        <div class="text-center py-5">
            <i class='bx bx-loader-alt bx-spin' style='font-size: 3rem; color: #f0ad4e;'></i>
            <p class="mt-3 text-muted">Cargando resultados...</p>
        </div>
    `).show();

    // ✅ NUEVO: Mantener interfaz bloqueada durante carga de grilla
    console.log('🔒 Interfaz permanece bloqueada durante carga de grilla');

    $.ajax({
        url: urlTraerGrilla,
        type: 'POST',
        dataType: 'html',
        timeout: 30000,
        success: function (htmlGrilla) {
            console.log('✅ Grilla cargada correctamente');
            $('#cardGrillaClientes').html(htmlGrilla).show().removeClass('hide').addClass('show');
            $('#btnSeguirCliente').prop('disabled', true);
            attachGrillaEventos();

            // ✅ NUEVO: Desbloquear interfaz después de cargar grilla
            desbloquearInterfazBusqueda();
        },
        error: function (xhr, status) {
            console.error('❌ Error al cargar grilla');
            console.error('   Status:', status);

            // ✅ NUEVO: Usar función centralizada
            if (esSesionExpirada(xhr.status)) {
                manejarSesionExpirada();
                return;
            }

            let mensajeError = 'Error al cargar la grilla de clientes';
            if (xhr.status === 500) mensajeError = 'Error interno del servidor';
            else if (status === 'timeout') mensajeError = 'La carga tardó demasiado tiempo';

            $('#cardGrillaClientes').html(`
                <div class="alert alert-danger m-3">
                    <i class='bx bx-error-circle'></i> ${mensajeError}
                </div>
            `);

            // ✅ NUEVO: Desbloquear interfaz en caso de error
            desbloquearInterfazBusqueda();
        }
    });

    console.log('═══════════════════════════════════════════════════');
}

// ====== VALIDAR CLIENTE ANTES DE SELECCIONAR ======
function validarClienteAntesDeSeleccionar($row) {
    const origen = $row.data('cta-origen');
    const origenDesc = $row.data('cta-origen-desc');
    const nombre = $row.data('cta-nombre');
    const documento = $row.data('cta-documento');

    if (origen && origen.toUpperCase() === 'N') {
        mostrarMensajeError(
            `⚠️ CLIENTE NO HABILITADO\n\n` +
            `El cliente "${nombre}" NO ESTÁ HABILITADO para operar.`
        );
        return false;
    }

    if (origen && origen.toUpperCase() === 'F') {
        if (!documento || documento.toString().trim() === '') {
            mostrarMensajeError(
                `⚠️ DATOS INCOMPLETOS\n\n` +
                `El consumidor final "${nombre}" no tiene número de documento registrado.`
            );
            return false;
        }
    }

    return true;
}

// ════════════════════════════════════════════════════════════════
// ✅ NUEVA FUNCIÓN - Agregar después de validarClienteAntesDeSeleccionar()
// Ubicación: Después de línea 447
// ════════════════════════════════════════════════════════════════

/**
 * ✅ NUEVA v1.0: Valida si un cliente puede ser confirmado automáticamente
 * 
 * @param {object} cliente - Objeto cliente de la respuesta del servidor
 * @returns {object} { valido: boolean, mensaje: string }
 */
function validarClienteParaAutoConfirmacion(cliente) {
    console.log('🔍 Validando cliente para auto-confirmación...');

    // ❶ VALIDACIÓN BÁSICA: Objeto existe
    if (!cliente) {
        return {
            valido: false,
            mensaje: 'Datos del cliente no disponibles'
        };
    }

    const origen = (cliente.origen || '').toUpperCase();

    // ❷ VALIDACIÓN: Cliente NO habilitado
    if (origen === 'N') {
        return {
            valido: false,
            mensaje: `El cliente "${cliente.denominacion}" no está habilitado para operar`
        };
    }

    // ❸ VALIDACIÓN: Consumidor Final sin documento
    if (origen === 'F') {
        const documento = (cliente.documento || '').toString().trim();
        if (!documento || documento === '') {
            return {
                valido: false,
                mensaje: `El consumidor final "${cliente.denominacion}" no tiene documento registrado`
            };
        }
    }

    // ❹ VALIDACIÓN: Cliente registrado sin ID
    if (origen === 'C') {
        const id = (cliente.id || '').toString().trim();
        if (!id || id === '') {
            return {
                valido: false,
                mensaje: 'El cliente no tiene ID válido'
            };
        }
    }

    // ✅ TODAS LAS VALIDACIONES PASARON
    console.log('✅ Cliente válido para auto-confirmación');
    return {
        valido: true,
        mensaje: ''
    };
}

// ====== EVENTOS DE LA GRILLA ======
function attachGrillaEventos() {
    $(document).off('dblclick', '.cliente-row');
    $(document).off('click', '.btn-seleccionar-cliente');
    $(document).off('click', '#btnCerrarGrilla');

    $(document).on('dblclick', '.cliente-row', function () {
        const $row = $(this);

        if (!validarClienteAntesDeSeleccionar($row)) {
            return;
        }

        seleccionarClienteDesdeGrilla($row);
    });

    $(document).on('click', '.btn-seleccionar-cliente', function (e) {
        e.stopPropagation();
        const $row = $(this).closest('.cliente-row');

        if ($row.length === 0) {
            mostrarMensajeError('Error: No se pudo identificar el cliente seleccionado');
            return;
        }

        if (!validarClienteAntesDeSeleccionar($row)) {
            return;
        }

        seleccionarClienteDesdeGrilla($row);
    });

    $(document).on('click', '#btnCerrarGrilla', function () {
        limpiarVista();
    });
}

// ====== SELECCIONAR CLIENTE DESDE GRILLA ======
/**
 * ✅ MODIFICADO v2.0: Selección desde grilla con bloqueo de interfaz
 */
function seleccionarClienteDesdeGrilla($row) {
    console.log('═══════════════════════════════════════════════════');
    console.log('👆 SELECCIONAR CLIENTE DESDE GRILLA v2.0');
    console.log('═══════════════════════════════════════════════════');

    if (!$row || $row.length === 0) {
        mostrarMensajeError('Error: No se pudo acceder a los datos de la fila seleccionada');
        return;
    }

    const datosCliente = {
        id: $row.data('cta-id'),
        nombre: $row.data('cta-nombre'),
        domicilio: $row.data('cta-domicilio'),
        tdocId: $row.data('cta-tdoc-id'),
        tdocDesc: $row.data('cta-tdoc-desc'),
        documento: $row.data('cta-documento'),
        email: $row.data('cta-email'),
        movil: $row.data('cta-movil'),
        origen: $row.data('cta-origen'),
        origenDesc: $row.data('cta-origen-desc')
    };

    console.log('📊 Datos extraídos de la fila:', datosCliente);

    if (!datosCliente.id && !datosCliente.documento) {
        mostrarMensajeError('Error: No se pudo identificar el ID del cliente');
        return;
    }

    if (!datosCliente.origen || datosCliente.origen.toString().trim() === '') {
        mostrarMensajeError('Error: Los datos de origen del cliente están incompletos');
        return;
    }

    let criterioBusqueda = '';
    const origenUpper = datosCliente.origen.toUpperCase();

    if (origenUpper === 'C') {
        criterioBusqueda = datosCliente.id;
        console.log(`   Cliente registrado - ID: ${criterioBusqueda}`);
    } else if (origenUpper === 'F') {
        if (!datosCliente.documento || datosCliente.documento.toString().trim() === '') {
            mostrarMensajeError('El consumidor final no tiene documento registrado');
            return;
        }
        criterioBusqueda = datosCliente.documento.toString();
        console.log(`   Consumidor final - Doc: ${criterioBusqueda}`);
    } else {
        criterioBusqueda = datosCliente.id;
        console.log(`   Otro tipo - ID: ${criterioBusqueda}`);
    }

    // ✅ NUEVO: Bloquear interfaz ANTES de iniciar carga
    bloquearInterfazBusqueda();

    $('#cardGrillaClientes').removeClass('show').hide().empty();
    $('#alertSinCliente').hide();

    const $cardBody = $('#cardDatosCliente .card-body');
    const loaderMensaje = origenUpper === 'C'
        ? `ID: ${criterioBusqueda}`
        : `${datosCliente.tdocDesc}: ${criterioBusqueda}`;

    if ($cardBody.length > 0) {
        $cardBody.hide();

        if ($('#loaderClienteTemp').length === 0) {
            $cardBody.after(`
                <div id="loaderClienteTemp" class="card-body text-center py-5">
                    <i class='bx bx-loader-alt bx-spin' style='font-size: 3rem; color: #f0ad4e;'></i>
                    <p class="mt-3 text-muted fw-semibold">Cargando datos del cliente...</p>
                    <small class="text-muted">${loaderMensaje}</small>
                </div>
            `);
        }
    }

    $('#cardDatosCliente').show();
    $('#btnSeguirCliente').prop('disabled', true);

    console.log('🔒 Interfaz bloqueada - Iniciando carga de cliente');
    console.log('═══════════════════════════════════════════════════');

    buscarClientePorId(criterioBusqueda);
}

// ====== BUSCAR CLIENTE POR ID ======
/**
 * ✅ MODIFICADO v2.0: Búsqueda por ID con gestión de estado
 */
function buscarClientePorId(clienteId) {
    console.log('═══════════════════════════════════════════════════');
    console.log('🔍 BUSCAR CLIENTE POR ID v2.0');
    console.log(`   ID: ${clienteId}`);
    console.log('═══════════════════════════════════════════════════');

    if (!clienteId || clienteId.toString().trim() === '') {
        console.error('❌ ID de cliente inválido');
        mostrarMensajeError('Error: ID de cliente inválido');
        limpiarVista();
        desbloquearInterfazBusqueda(); // ✅ NUEVO
        return;
    }

    const url = typeof BuscarClienteUrl !== 'undefined' && BuscarClienteUrl
        ? BuscarClienteUrl
        : '/Facturacion/Cliente/BuscarCliente';

    console.log(`📡 URL: ${url}`);

    // ✅ NUEVO: Guardar referencia para posible cancelación
    ajaxActual = $.ajax({
        url: url,
        type: 'POST',
        data: { criterio: clienteId },
        timeout: 30000,
        success: function (response) {
            console.log('✅ Respuesta recibida');

            if (!response.ok) {
                mostrarMensajeError(response.mensaje || 'Error al cargar datos del cliente');
                limpiarVista();
                return;
            }

            const cantidadResultados = response.cantidadResultados || 0;

            if (cantidadResultados !== 1) {
                mostrarMensajeError('Error: No se pudieron obtener los datos del cliente');
                limpiarVista();
                return;
            }

            if (!response.cliente) {
                mostrarMensajeError('Error: Los datos del cliente no están disponibles');
                limpiarVista();
                return;
            }

            // ══════════════════════════════════════════════════════
            // ✅ MODIFICACIÓN: Lógica de confirmación automática
            // ══════════════════════════════════════════════════════
            console.log('✅ Cliente cargado correctamente desde grilla');

            if (autoConfirmarClienteUnico) {
                console.log('⚡ Auto-confirmando cliente desde grilla');
                confirmarClienteAutomaticamente(response.cliente);
            } else {
                console.log('👁️ Mostrando datos de cliente desde grilla');
                mostrarDatosCliente(response.cliente);
            }
        },
        error: function (xhr, status) {
            console.error('❌ ERROR AJAX');
            console.error('   Status:', status);
            console.error('   HTTP Status:', xhr.status);

            // ✅ NUEVO: No mostrar error si fue cancelación
            if (status === 'abort') {
                console.log('⚠️ Búsqueda cancelada');
                return;
            }

            // ✅ NUEVO: Usar función centralizada
            if (esSesionExpirada(xhr.status)) {
                manejarSesionExpirada();
                return;
            }

            let mensaje = 'Error al cargar los datos del cliente';
            if (status === 'timeout') mensaje = 'La búsqueda tardó demasiado tiempo';
            else if (xhr.status === 404) mensaje = 'Servicio no encontrado';
            else if (xhr.status === 500) mensaje = 'Error interno del servidor';

            mostrarMensajeError(mensaje);
            limpiarVista();
        },
        complete: function (xhr, status) {
            console.log('🏁 Búsqueda por ID finalizada');
            console.log('   Status:', status);

            // ✅ NUEVO: Desbloquear interfaz SIEMPRE
            desbloquearInterfazBusqueda();

            console.log('═══════════════════════════════════════════════════');
        }
    });
}

// ====== MOSTRAR DATOS DEL CLIENTE ======
/**
 * ✅ MODIFICADO v2.0: Mostrar datos con actualización de estado de botones
 */
function mostrarDatosCliente(cliente) {
    console.log('═══════════════════════════════════════════════════');
    console.log('📊 MOSTRAR DATOS DEL CLIENTE v2.0');
    console.log('═══════════════════════════════════════════════════');

    if ($('#cardGrillaClientes').length > 0) {
        $('#cardGrillaClientes').removeClass('show').hide().empty();
    }

    $('#alertSinCliente').hide();
    $('#loaderClienteTemp').remove();





    const $cardBody = $('#cardDatosCliente .card-body');
    if ($cardBody.length > 0) {
        $cardBody.show();
    }

    const origenUpper = (cliente.origen || '').toUpperCase();
    const idDisplay = origenUpper === 'C' ? (cliente.id || '') : 'N/A';

    const tipoNumeroDisplay = (cliente.tdocDesc && cliente.documento)
        ? `${cliente.tdocDesc} ${cliente.documento}`
        : (cliente.tipoNumero || '');

    console.log('📝 Llenando campos:');
    console.log('   Nombre:', cliente.denominacion);
    console.log('   ID:', idDisplay);
    console.log('   Origen:', origenUpper);

    $('#txtNombre').val(cliente.denominacion || '');
    $('#txtClienteId').val(idDisplay);
    $('#txtDomicilio').val(cliente.domicilio || '');
    $('#txtCondicionAfip').val(cliente.condicionAfip || '');
    $('#txtTipoNumero').val(tipoNumeroDisplay);
    $('#txtEmite').val(cliente.emite || '');
    $('#txtEmail').val(cliente.email || '');
    $('#txtMovil').val(cliente.movil || '');

    const esConsumidorFinal = cliente.origen && cliente.origen.toUpperCase() === 'F';

    if (esConsumidorFinal) {
        $('#btnEditarCliente').fadeIn(300);
        console.log('✅ Botón EDITAR mostrado (Consumidor Final)');
    } else {
        $('#btnEditarCliente').fadeOut(300);
        console.log('✅ Botón EDITAR oculto (Cliente Registrado)');
    }

    $('#cardDatosCliente').show().removeClass('hide').addClass('show');
    $('#btnSeguirCliente').prop('disabled', false);

    clienteSeleccionado = cliente;

    // ✅ NUEVO: Actualizar estado de botones según cliente
    actualizarEstadoBotonesAccion();

    console.log('✅ Datos del cliente mostrados correctamente');
    console.log('═══════════════════════════════════════════════════');
}

// ====== LIMPIAR VISTA ======
function limpiarVista() {
    $('#txtNombre, #txtClienteId, #txtDomicilio, #txtCondicionAfip, #txtTipoNumero, #txtEmite, #txtEmail, #txtMovil').val('');
    $('#cardDatosCliente').removeClass('show').hide();

    const $cardBody = $('#cardDatosCliente .card-body');
    if ($cardBody.length > 0) {
        $cardBody.show();
    }

    $('#loaderClienteTemp').remove();

    if ($('#cardGrillaClientes').length > 0) {
        $('#cardGrillaClientes').removeClass('show').hide().empty();
    }

    $('#alertSinCliente').removeClass('hide').show();
    $('#btnSeguirCliente').prop('disabled', true);
    $('#btnEditarCliente').hide();

    clienteSeleccionado = null;

    // ✅ NUEVO: Actualizar botones
    actualizarEstadoBotonesAccion();
}

// ========================================
// MODAL CLIENTE UPDATE - FUNCIONES
// ========================================

function abrirModalClienteNuevo() {
    modoEdicionCliente = false;
    limpiarFormularioCliente();

    $('#lblTituloClienteUpdate').html('<i class="bx bx-user-plus"></i> Nuevo CF');
    $('#lblBotonAccion').text('Cargar CF');

    $('#modalClienteUpdate').modal('show');

    setTimeout(() => {
        $('#selTipoDocumento').trigger("focus");
    }, 500);
}

function abrirModalClienteEditar() {
    if (!clienteSeleccionado) {
        mostrarMensajeError('No hay cliente seleccionado para editar');
        return;
    }

    if (!clienteSeleccionado.origen || clienteSeleccionado.origen.toUpperCase() !== 'F') {
        mostrarMensajeError('Solo se pueden editar Consumidores Finales');
        return;
    }

    const urlObtenerCliente = typeof ObtenerClienteActualUrl !== 'undefined' && ObtenerClienteActualUrl
        ? ObtenerClienteActualUrl
        : '/Facturacion/Cliente/ObtenerClienteActual';

    $.ajax({
        url: urlObtenerCliente,
        type: 'POST',
        timeout: 10000,
        success: function (response) {
            if (!response.ok) {
                mostrarMensajeError(response.mensaje || 'Error al obtener datos del cliente');
                return;
            }

            if (!response.cliente) {
                mostrarMensajeError('Los datos del cliente no están disponibles');
                return;
            }

            const datosCliente = response.cliente;

            modoEdicionCliente = true;
            limpiarFormularioCliente(true);

            $('#lblTituloClienteUpdate').html('<i class="bx bx-edit"></i> Editar Consumidor Final');
            $('#lblBotonAccion').text('Actualizar');

            $('#txtClienteIdUpdate').val(datosCliente.id || '');
            $('#selTipoDocumento').val(datosCliente.tipoDocumento || '96');
            $('#txtNumeroDocumento').val(datosCliente.numeroDocumento || '').prop('readonly', true);

            let apellidoFinal = '';
            if (datosCliente.apellido && datosCliente.apellido.trim() !== '') {
                apellidoFinal = datosCliente.apellido.trim();
            } else if (datosCliente.cta_denominacion && datosCliente.cta_denominacion.trim() !== '') {
                const partes = datosCliente.cta_denominacion.split(' ').filter(p => p.trim() !== '');
                apellidoFinal = partes.length > 0 ? partes[0] : '';
            }
            $('#txtApellidoCliente').val(apellidoFinal);

            let nombreFinal = '';
            if (datosCliente.nombre && datosCliente.nombre.trim() !== '') {
                nombreFinal = datosCliente.nombre.trim();
            } else if (datosCliente.cta_denominacion && datosCliente.cta_denominacion.trim() !== '') {
                const partes = datosCliente.cta_denominacion.split(' ').filter(p => p.trim() !== '');
                nombreFinal = partes.length > 1 ? partes.slice(1).join(' ') : '';
            }
            $('#txtNombreCliente').val(nombreFinal);

            $('#selSexoCliente').val(datosCliente.sexo || 'M');
            $('#txtDomicilioCliente').val(datosCliente.domicilio || '');
            $('#txtEmailCliente').val(datosCliente.email || '');
            $('#txtMovilCliente').val(datosCliente.movil || '');

            ajustarPlaceholderSegunTipo(false);

            $('#modalClienteUpdate').modal('show');

            setTimeout(() => {
                $('#txtApellidoCliente').trigger("focus").trigger("select");
            }, 500);
        },
        error: function (xhr, status) {
            // ✅ NUEVO: Usar función centralizada
            if (esSesionExpirada(xhr.status)) {
                manejarSesionExpirada();
                return;
            }

            let mensajeError = 'Error al cargar datos del cliente desde el servidor';
            if (status === 'timeout') mensajeError = 'La solicitud tardó demasiado tiempo';
            else if (xhr.status === 500) mensajeError = 'Error interno del servidor';
            else if (xhr.status === 404) mensajeError = 'Servicio no encontrado';

            mostrarMensajeError(mensajeError);
        }
    });
}

function limpiarFormularioCliente(preservarModoEdicion = false) {
    $('#formClienteUpdate')[0].reset();
    $('#txtClienteIdUpdate').val('');
    $('#selTipoDocumento').val('96');
    $('#selSexoCliente').val('M');
    $('#formClienteUpdate .form-control, #formClienteUpdate .form-select').removeClass('is-valid is-invalid');
    $('#btnCargarCliente').removeClass('processing').prop('disabled', false);
    $('#txtNumeroDocumento').prop('readonly', false);
    ajustarPlaceholderSegunTipo(true);

    if (!preservarModoEdicion) {
        modoEdicionCliente = false;
    }
}

function confirmarCliente(cliente) {
    if (!cliente) {
        mostrarMensajeError('No hay cliente seleccionado');
        return;
    }

    $('#modalIdentificarCliente').modal('hide');

    setTimeout(() => {
        $(document).trigger('clienteConfirmado', [cliente]);
    }, 400);
}

// ════════════════════════════════════════════════════════════════
// ✅ NUEVA FUNCIÓN - Agregar después de confirmarCliente()
// Ubicación: Después de línea 933
// ════════════════════════════════════════════════════════════════

/**
 * ✅ NUEVA v1.0: Confirma cliente automáticamente sin mostrar datos
 * 
 * Flujo:
 * 1. Valida que el cliente pueda ser auto-confirmado
 * 2. Guarda en clienteSeleccionado
 * 3. Cierra modal inmediatamente
 * 4. Dispara evento 'clienteConfirmado'
 * 
 * @param {object} cliente - Objeto cliente de la búsqueda
 */
function confirmarClienteAutomaticamente(cliente) {
    console.log('═══════════════════════════════════════════════════');
    console.log('⚡ CONFIRMACIÓN AUTOMÁTICA DE CLIENTE');
    console.log('═══════════════════════════════════════════════════');
    console.log('   Denominación:', cliente.denominacion);
    console.log('   Origen:', cliente.origen);
    console.log('   ID:', cliente.id);

    // ❶ VALIDACIÓN DE SEGURIDAD
    const validacion = validarClienteParaAutoConfirmacion(cliente);

    if (!validacion.valido) {
        console.error('❌ Validación fallida:', validacion.mensaje);
        mostrarMensajeError(validacion.mensaje);
        limpiarVista();
        return;
    }

    // ❷ GUARDAR CLIENTE EN VARIABLE GLOBAL
    clienteSeleccionado = cliente;
    console.log('✅ Cliente guardado en clienteSeleccionado');

    // ❸ CERRAR MODAL INMEDIATAMENTE
    $('#modalIdentificarCliente').modal('hide');
    console.log('✅ Modal cerrado');

    // ❹ DISPARAR EVENTO DESPUÉS DEL CIERRE
    // Usamos el mismo setTimeout que confirmarCliente() para consistencia
    setTimeout(() => {
        console.log('📡 Disparando evento clienteConfirmado...');
        $(document).trigger('clienteConfirmado', [cliente]);
        console.log('✅ Evento disparado - Control transferido a prodfact.js');
    }, 400); // ← Mismo delay que confirmarCliente()

    console.log('═══════════════════════════════════════════════════');
}

function limpiarSesionClientesBuscados() {
    const urlLimpiarSesion = typeof LimpiarSesionClientesUrl !== 'undefined' && LimpiarSesionClientesUrl
        ? LimpiarSesionClientesUrl
        : '/Facturacion/Cliente/LimpiarSesionClientes';

    $.ajax({
        url: urlLimpiarSesion,
        type: 'POST',
        async: true,
        timeout: 5000,
        success: function (response) {
            if (response && response.ok) {
                console.log('✅ Sesión de clientes limpiada en el servidor');
            }
        },
        error: function () {
            console.warn('⚠️ No se pudo limpiar la sesión en el servidor');
        }
    });
}

function confirmarSalidaAlMenu() {
    if (clienteSeleccionado) {
        AbrirMensaje(
            "Confirmar Salida",
            "¿Está seguro que desea salir al menú principal?\n\nSe perderá el cliente seleccionado.",
            function () {
                $("#msjModal").modal("hide");
                redirigirAlMenu();
            },
            true,
            ["Sí, salir", "Cancelar"],
            "warning",
            null
        );
    } else {
        redirigirAlMenu();
    }
}

function redirigirAlMenu() {
    limpiarSesionClientesBuscados();

    const urlMenu = typeof MenuCajaUrl !== 'undefined' && MenuCajaUrl
        ? MenuCajaUrl
        : '/Home/Index';

    setTimeout(() => {
        window.location.href = urlMenu;
    }, 300);
}

function cerrarModalClienteUpdate() {
    $('#modalClienteUpdate').modal('hide');
    limpiarFormularioCliente();
}

function ajustarPlaceholderSegunTipo(limpiarValor = true) {
    const tipoSeleccionado = $('#selTipoDocumento').val();
    const $inputNumero = $('#txtNumeroDocumento');

    let placeholder = 'Ingrese el número de documento...';
    let maxLength = 20;
    let clasesCss = 'form-control form-control-lg fw-bold jsteclado';

    switch (tipoSeleccionado) {
        case '80': case '86':
            placeholder = 'Ej: 20123456789 (sin guiones)';
            maxLength = 11;
            clasesCss += ' jsinteger';
            break;
        case '87': case '89': case '90': case '95': case '96':
            placeholder = 'Ej: 12345678';
            maxLength = 8;
            clasesCss += ' jsinteger';
            break;
        case '91': case '94':
            placeholder = 'Ej: ABC123456';
            maxLength = 15;
            break;
        case '99':
            placeholder = 'No aplica';
            maxLength = 1;
            break;
    }

    $inputNumero.attr('placeholder', placeholder).attr('maxlength', maxLength).attr('class', clasesCss);

    if (limpiarValor) {
        $inputNumero.val('');
    }
}

/**
 * ✅ ACTUALIZADO v5.0: Guarda o actualiza un Consumidor Final
 * 
 * CAMBIOS v5.0:
 * - Agregado: Envío de apellido y nombre separados
 * - Agregado: Envío de sexo
 * - Agregado: Manejo de modo EDITAR vs NUEVO
 * - Agregado: Actualización de clienteSeleccionado con datos del servidor
 * - Mejorado: Logs detallados para debugging
 * - Mejorado: Manejo de errores completo
 * 
 * Flujo:
 * 1. Obtiene datos del formulario
 * 2. Valida campos obligatorios
 * 3. Determina URL del endpoint
 * 4. Realiza petición AJAX POST
 * 5. Procesa respuesta del servidor
 * 6. Actualiza clienteSeleccionado en JavaScript
 * 7. Cierra modal y muestra datos actualizados
 */
function guardarCliente() {
    console.log('═══════════════════════════════════════════════════');
    console.log('💾 GUARDAR CLIENTE v5.0');
    console.log('═══════════════════════════════════════════════════');

    // ❶ OBTENER DATOS DEL FORMULARIO
    const clienteData = {
        id: $('#txtClienteIdUpdate').val(), // ← Vacío si es NUEVO, con valor si es EDITAR
        abm: modoEdicionCliente ? 'M' : 'A', // ← ✅ CRÍTICO
        apellido: $('#txtApellidoCliente').val().trim(),
        nombre: $('#txtNombreCliente').val().trim(),
        sexo: $('#selSexoCliente').val(),
        tipoDocumento: $('#selTipoDocumento').val(),
        numeroDocumento: $('#txtNumeroDocumento').val().trim(),
        domicilio: $('#txtDomicilioCliente').val().trim(),
        email: $('#txtEmailCliente').val().trim(),
        movil: $('#txtMovilCliente').val().trim()
    };

    console.log('📊 Datos del cliente a guardar:', clienteData);
    console.log(`   Modo: ${clienteData.id ? 'EDITAR (ABM=M)' : 'NUEVO (ABM=A)'}`);

    // ❷ VALIDAR DATOS REQUERIDOS
    if (!clienteData.apellido || !clienteData.nombre || !clienteData.sexo ||
        !clienteData.tipoDocumento || !clienteData.numeroDocumento) {
        console.error('❌ Validación fallida - Campos obligatorios vacíos');
        mostrarMensajeError('Por favor, complete todos los campos obligatorios (*)');
        return;
    }

    // ❸ URL DE ACTUALIZACIÓN
    const urlActualizar = typeof ActualizarConsumidorFinalUrl !== 'undefined' && ActualizarConsumidorFinalUrl
        ? ActualizarConsumidorFinalUrl
        : '/Facturacion/Cliente/ActualizarConsumidorFinal';

    console.log(`📡 URL de actualización: ${urlActualizar}`);

    // ❹ DESHABILITAR BOTÓN MIENTRAS SE PROCESA
    $('#btnCargarCliente').addClass('processing').prop('disabled', true);

    // ❺ REALIZAR PETICIÓN AJAX
    $.ajax({
        url: urlActualizar,
        type: 'POST',
        data: clienteData, // ← Se envía como FormData automáticamente
        timeout: 15000,
        success: function (response) {
            console.log('═══════════════════════════════════════════════════');
            console.log('✅ RESPUESTA DEL SERVIDOR RECIBIDA');
            console.log('═══════════════════════════════════════════════════');
            console.log('   response.ok:', response.ok);
            console.log('   response.mensaje:', response.mensaje);

            if (response.ok) {
                console.log('✅ Cliente guardado exitosamente en el servidor');

                // ❻ OBTENER DATOS ACTUALIZADOS DEL SERVIDOR
                const datosActualizados = response.cliente || {};

                console.log('📊 Datos actualizados desde servidor:', datosActualizados);

                // ❼ MOSTRAR MENSAJE DE ÉXITO
                mostrarMensajeExito(response.mensaje || 'Consumidor Final actualizado correctamente');

                // ❽ CERRAR MODAL DESPUÉS DE 1.5 SEGUNDOS
                setTimeout(() => {
                    cerrarModalClienteUpdate();

                    // ❾ CONSTRUIR OBJETO CLIENTE ACTUALIZADO
                    const clienteActualizado = {
                        ...clienteSeleccionado, // Mantener datos base

                        // ✅ Datos actualizados (priorizar servidor, fallback a form)
                        nombre: datosActualizados.nombre || `${clienteData.apellido}, ${clienteData.nombre}`,
                        apellido: datosActualizados.apellido || clienteData.apellido,
                        nombreSolo: datosActualizados.nombreSolo || clienteData.nombre,
                        sexo: datosActualizados.sexo || clienteData.sexo,
                        domicilio: datosActualizados.domicilio || clienteData.domicilio,
                        email: datosActualizados.email || clienteData.email,
                        movil: datosActualizados.movil || clienteData.movil,

                        // ✅ Campos derivados del tipo de documento
                        tdocDesc: obtenerDescripcionTipoDoc(clienteData.tipoDocumento),
                        tdocId: clienteData.tipoDocumento,
                        documento: datosActualizados.numeroDocumento || clienteData.numeroDocumento,
                        tipoNumero: `${obtenerDescripcionTipoDoc(clienteData.tipoDocumento)} ${clienteData.numeroDocumento}`
                    };

                    console.log('═══════════════════════════════════════════════════');
                    console.log('📊 CLIENTE ACTUALIZADO v5.0');
                    console.log('═══════════════════════════════════════════════════');
                    console.log('   Nombre completo:', clienteActualizado.nombre);
                    console.log('   Apellido:', clienteActualizado.apellido);
                    console.log('   Nombre:', clienteActualizado.nombreSolo);
                    console.log('   Sexo:', clienteActualizado.sexo);
                    console.log('   Documento:', clienteActualizado.documento);
                    console.log('   Email:', clienteActualizado.email);
                    console.log('   Móvil:', clienteActualizado.movil);
                    console.log('═══════════════════════════════════════════════════');

                    // ❿ ACTUALIZAR VARIABLE GLOBAL
                    clienteSeleccionado = clienteActualizado;

                    // ⓫ MOSTRAR DATOS ACTUALIZADOS EN EL MODAL DE IDENTIFICAR
                    mostrarDatosCliente(clienteActualizado);
                }, 1500);
            } else {
                // ⓬ ERROR REPORTADO POR EL SERVIDOR
                console.error('❌ Error al guardar:', response.mensaje);
                mostrarMensajeError(response.mensaje || 'Error al actualizar el cliente');
                $('#btnCargarCliente').removeClass('processing').prop('disabled', false);
            }
        },
        error: function (xhr, status, error) {
            console.error('❌ ERROR AJAX AL GUARDAR CLIENTE');

            // ✅ NUEVO: Usar función centralizada
            if (esSesionExpirada(xhr.status)) {
                manejarSesionExpirada('No se pudo guardar el cliente porque su sesión ha expirado.');
                return;
            }

            let mensajeError = 'Error al comunicarse con el servidor';
            if (xhr.status === 400) {
                mensajeError = 'Datos inválidos. Por favor, verifique los campos.';
            } else if (xhr.status === 500) {
                mensajeError = 'Error interno del servidor. Contacte al administrador.';
            } else if (xhr.responseJSON && xhr.responseJSON.mensaje) {
                mensajeError = xhr.responseJSON.mensaje;
            }

            mostrarMensajeError(mensajeError);
            $('#btnCargarCliente').removeClass('processing').prop('disabled', false);
        }
    });
}

function validarFormularioCliente() {
    // TODO: Implementar validación completa
    return true;
}

function validarCampo($campo) {
    // TODO: Implementar validación de campo
}

function validarEmail($campo) {
    // TODO: Implementar validación de email
}

function obtenerDescripcionTipoDoc(tdocId) {
    const tipos = {
        '80': 'CUIT', '86': 'CUIL', '87': 'CDI', '89': 'LE', '90': 'LC',
        '91': 'CI Extranjera', '94': 'Pasaporte', '95': 'CI Bs. As. RNP',
        '96': 'D.N.I.', '99': 'Sin Identificar'
    };
    return tipos[tdocId] || 'Desconocido';
}

/**
* ✅ NUEVA: Actualiza la lista de precios según el tipo de cliente
* @param {string} tipoCliente - "FINAL" o "REGISTRADO"
* @param {object} clienteData - Datos del cliente (opcional)
*/
function actualizarListaPreciosGlobal(tipoCliente, clienteData = null) {
    console.log('═══════════════════════════════════════════════════');
    console.log('🔄 ACTUALIZAR LISTA DE PRECIOS GLOBAL');
    console.log('═══════════════════════════════════════════════════');
    console.log(`   Tipo de cliente: ${tipoCliente}`);

    // ✅ Lógica de negocio para determinar lista de precios
    if (tipoCliente === "FINAL") {
        // Consumidor final → Lista de precios 002 (Minorista)
        admLp_id = "002";
        console.log('   → Consumidor Final: Lista de precios MINORISTA (002)');
    } else if (tipoCliente === "REGISTRADO") {
        // Cliente registrado → Verificar su configuración
        if (clienteData && clienteData.lp_id) {
            admLp_id = clienteData.lp_id;
            console.log(`   → Cliente Registrado: Lista de precios ${clienteData.lp_id}`);
        } else {
            // Por defecto: Lista de precios 001 (Mayorista)
            admLp_id = "001";
            console.log('   → Cliente Registrado (sin config): Lista de precios MAYORISTA (001)');
        }
    } else {
        // Caso por defecto
        admLp_id = "001";
        console.log('   → Caso por defecto: Lista de precios MAYORISTA (001)');
    }

    console.log(`✅ Lista de precios actualizada globalmente: ${admLp_id}`);
    console.log('═══════════════════════════════════════════════════');
}

/**
* ✅ ACTUALIZADO: Confirma el cliente seleccionado
* Ahora actualiza la lista de precios global
*/
function confirmarClienteSeleccionado() {
    console.log('═══════════════════════════════════════════════════');
    console.log('✅ CONFIRMAR CLIENTE SELECCIONADO');
    console.log('═══════════════════════════════════════════════════');

    const clienteData = obtenerClienteSeleccionadoUI();

    if (!clienteData) {
        mostrarMensajeError('Debe seleccionar un cliente');
        return;
    }

    // ✅ NUEVO: Actualizar lista de precios según tipo de cliente
    const tipoCliente = clienteData.esConsumidorFinal ? "FINAL" : "REGISTRADO";
    actualizarListaPreciosGlobal(tipoCliente, clienteData);

    // Disparar evento para que prodfact.js reaccione
    $(document).trigger('clienteConfirmado', [clienteData]);

    // Cerrar modal
    $('#modalIdentificarCliente').modal('hide');

    console.log('✅ Cliente confirmado y lista de precios actualizada');
    console.log('═══════════════════════════════════════════════════');
}

// ========================================
// ✅ NUEVO: FUNCIONES DE SESIÓN
// ========================================

/**
 * Verifica si el código de estado HTTP corresponde a sesión expirada
 * @param {number} statusCode - Código HTTP
 * @returns {boolean}
 */
function esSesionExpirada(statusCode) {
    return statusCode === 401 || statusCode === 403;
}

/**
 * Maneja el escenario de sesión expirada
 * @param {string} mensajeAdicional - Mensaje adicional opcional
 */
function manejarSesionExpirada(mensajeAdicional = '') {
    console.error('🚨 SESIÓN EXPIRADA DETECTADA');

    const mensaje = mensajeAdicional
        ? `${mensajeAdicional}\n\nSerá redirigido al login.`
        : 'Su sesión ha expirado. Será redirigido al login.';

    AbrirMensaje(
        "Sesión Expirada",
        mensaje,
        function () {
            window.location.href = '/Account/Login?returnUrl=' + encodeURIComponent(window.location.pathname);
        },
        false,
        ["Aceptar"],
        "warning",
        null
    );
}