$(function () {
    // Inicializar eventos del documento
    initializeDocumentEvents();

    // Cargar proveedores automáticamente después de 500ms
    setTimeout(function () {
        cargarProveedores();
    }, 500);

    // Exponer funciones globales (mantener solo las necesarias)
    window.ActualizarPP = {
        obtenerProveedoresSeleccionados: obtenerProveedoresSeleccionados,
        cargarProductosProveedor: cargarProductosProveedor,
        recargarProveedores: cargarProveedores // Solo para casos de error/recarga
    };
});

/**
 * Inicializa los eventos del documento usando delegación
 */
function initializeDocumentEvents() {
    // Event delegation para elementos dinámicos
    $(document).on('change', '#selectAllProveedores', function () {
        $('.proveedor-check').prop('checked', $(this).prop('checked'));
        actualizarContadores();
    });

    $(document).on('change', '.proveedor-check', function () {
        actualizarContadores();
    });

    $(document).on('click', '.proveedor-row', function (e) {
        if (e.target.type === 'checkbox') return;

        const ctaId = $(this).data('cta-id');
        if (ctaId) {
            cargarProductosProveedor(ctaId);
            // Cambiar a la pestaña de productos
            $('#productos-tab').tab('show');
        }
    });

    // Escuchar evento personalizado de selección de proveedor
    $(document).on('proveedorSeleccionado', function (e) {
        const { ctaId, denominacion } = e.detail;
        console.log('Proveedor seleccionado:', denominacion, ctaId);
    });
}

/**
 * Carga la vista parcial de proveedores automáticamente
 */
async function cargarProveedores() {
    const $container = $('#proveedoresContainer');

    try {
        // Mostrar spinner optimizado
        mostrarSpinnerCarga($container, 'Cargando proveedores con productos para actualizar...');

        const response = await $.ajax({
            url: '/Productos/ActualizarPP/CargarProveedores',
            type: 'POST',
            timeout: 30000, // 30 segundos timeout
            headers: {
                'RequestVerificationToken': $('input[name="__RequestVerificationToken"]').val() || ''
            }
        });

        $container.html(response);

        // Inicializar contadores después de cargar
        actualizarContadores();

        console.log('Proveedores cargados exitosamente');

    } catch (error) {
        console.error('Error al cargar proveedores:', error);

        // Determinar tipo de error
        let errorMessage = 'Error desconocido al cargar proveedores';
        if (error.status === 0) {
            errorMessage = 'Error de conexión. Verifique su conexión a internet.';
        } else if (error.status >= 500) {
            errorMessage = 'Error del servidor. Intente nuevamente en unos momentos.';
        } else if (error.status === 404) {
            errorMessage = 'Recurso no encontrado.';
        } else if (error.timeout) {
            errorMessage = 'Tiempo de espera agotado. La operación tardó demasiado.';
        }

        mostrarErrorConRecarga($container, errorMessage);
    }
}

/**
 * Carga los productos de un proveedor específico
 * @param {string} ctaId - ID del proveedor
 */
async function cargarProductosProveedor(ctaId) {
    const $container = $('#productosContainer');

    try {
        mostrarSpinnerCarga($container, 'Obteniendo productos del proveedor...');

        const response = await $.ajax({
            url: '/Productos/ActualizarPP/ObtenerProductosProveedor',
            type: 'POST',
            contentType: 'application/x-www-form-urlencoded; charset=UTF-8',
            data: { ctaId: ctaId },
            timeout: 15000,
            headers: {
                'RequestVerificationToken': $('input[name="__RequestVerificationToken"]').val() || ''
            }
        });

        // Ahora response es HTML, no JSON
        $container.html(response);

    } catch (error) {
        console.error('Error al cargar productos:', error);
        mostrarErrorConRecarga($container, 'Error al cargar los productos.', function () {
            cargarProductosProveedor(ctaId);
        });
    }
}

/**
 * Obtiene los proveedores seleccionados
 * @returns {Array} Array de IDs de proveedores seleccionados
 */
function obtenerProveedoresSeleccionados() {
    return $('.proveedor-check:checked').map(function () {
        return $(this).val();
    }).get();
}

/**
 * Función unificada para actualizar todos los contadores
 */
function actualizarContadores() {
    const seleccionados = $('.proveedor-check:checked').length;
    const total = $('.proveedor-check').length;

    // Actualizar contadores en la interfaz
    $('#selectedCount').text(seleccionados);
    $('#contadorSeleccionados').text(seleccionados);

    // Actualizar estado del checkbox principal
    const $selectAll = $('#selectAllProveedores');
    if ($selectAll.length) {
        if (seleccionados === 0) {
            $selectAll.prop('indeterminate', false).prop('checked', false);
        } else if (seleccionados === total) {
            $selectAll.prop('indeterminate', false).prop('checked', true);
        } else {
            $selectAll.prop('indeterminate', true).prop('checked', false);
        }
    }

    // Habilitar/deshabilitar botón confirmar
    $('#btnConfirmarActualizacion').prop('disabled', seleccionados === 0);
}

// ===== FUNCIONES DE UTILIDAD OPTIMIZADAS =====

/**
 * Muestra un spinner de carga optimizado
 * @param {jQuery} $container - Contenedor
 * @param {string} message - Mensaje de carga
 */
function mostrarSpinnerCarga($container, message) {
    $container.html(`
        <div class="loading-container">
            <div class="spinner-container">
                <div class="spinner-border spinner-border-golden" role="status">
                    <span class="visually-hidden">Cargando...</span>
                </div>
                <p class="mt-3 text-muted mb-0">${message}</p>
            </div>
        </div>
    `);
}

/**
 * Muestra error con opción de recarga
 * @param {jQuery} $container - Contenedor
 * @param {string} message - Mensaje de error
 * @param {Function} retryFunction - Función de reintento (opcional)
 */
function mostrarErrorConRecarga($container, message, retryFunction = null) {
    const retryFunctionName = retryFunction ? 'retryFunction()' : 'ActualizarPP.recargarProveedores()';

    $container.html(`
        <div class="alert alert-danger" role="alert">
            <div class="d-flex align-items-center">
                <i class="fas fa-exclamation-triangle me-2"></i>
                <div class="flex-grow-1">
                    <strong>Error:</strong> ${message}
                </div>
                <button class="btn btn-sm btn-outline-danger" onclick="${retryFunctionName}">
                    <i class="fas fa-redo me-1"></i>Reintentar
                </button>
            </div>
        </div>
    `);

    // Si se pasa una función de reintento, agregarla al contexto global temporalmente
    if (retryFunction) {
        window.retryFunction = retryFunction;
    }
}

/**
 * Muestra alerta informativa
 * @param {jQuery} $container - Contenedor  
 * @param {string} message - Mensaje informativo
 */
function mostrarAlertaInfo($container, message) {
    $container.html(`
        <div class="alert alert-info" role="alert">
            <i class="fas fa-info-circle me-2"></i>
            ${message}
        </div>
    `);
}

// Funciones para los botones
async function confirmarActualizacion() {
    const proveedoresSeleccionados = ActualizarPP.obtenerProveedoresSeleccionados();

    if (proveedoresSeleccionados.length === 0) {
        AbrirMensaje("Validación", 'Debe seleccionar al menos un proveedor para confirmar la actualización.',
            () => $("#msjModal").modal("hide"),
            false, ["Aceptar"], "warn!", null);
        return;
    }

    // Limpiar estado del modal antes de mostrar confirmación
    limpiarEstadoModal();

    // Confirmar antes de proceder
    AbrirMensaje("Confirmar Actualización",
        `¿Confirmar actualización de <strong>${proveedoresSeleccionados.length}</strong> proveedores?<br>
         <small class="text-muted">Esta acción aplicará los cambios de precios definitivamente.</small>`,
        async (resp) => {
            $("#msjModal").modal("hide");
            if (resp === "SI") {                
                await ejecutarConfirmacionActualizacion(proveedoresSeleccionados);
            }
        },
        true, ["Confirmar", "Cancelar"], "warn!", null);
}

/**
 * Ejecuta la confirmación de actualización llamando al controller
 * @param {Array} ctasId - Array de IDs de proveedores seleccionados
 */
async function ejecutarConfirmacionActualizacion(ctasId) {
    const $btn = $('#btnConfirmarActualizacion');

    try {
        // Estado de carga
        $btn.prop('disabled', true)
            .html('<i class="bx bx-loader-alt bx-spin me-2"></i><span>Procesando actualización...</span>');

        console.log('Confirmando actualización para proveedores:', ctasId);

        const response = await $.ajax({
            url: ConfirmarProveedoresUrl,
            type: 'POST',
            traditional: true, // Para enviar arrays correctamente
            data: { ctasId: ctasId },
            timeout: 60000, // 1 minuto para operaciones de actualización
            headers: {
                'RequestVerificationToken': $('input[name="__RequestVerificationToken"]').val() || ''
            }
        });

        console.log('Respuesta del servidor:', response);

        // Procesar respuesta según el patrón del controller optimizado
        procesarRespuestaConfirmacion(response, $btn, ctasId.length);

    } catch (error) {
        console.error('Error AJAX en confirmación:', error);

        let errorMessage = 'Error de comunicación con el servidor';
        if (error.status === 0) {
            errorMessage = 'Error de conexión. Verifique su conexión a internet.';
        } else if (error.status >= 500) {
            errorMessage = 'Error interno del servidor. Intente nuevamente.';
        } else if (error.status === 404) {
            errorMessage = 'Servicio de confirmación no encontrado.';
        } else if (error.timeout) {
            errorMessage = 'Tiempo de espera agotado. La operación puede haber tardado demasiado.';
        }

        manejarErrorConfirmacion(errorMessage, $btn);
    }
}

/**
 * Procesa la respuesta del servidor según el patrón del controller
 * @param {Object} response - Respuesta del servidor
 * @param {jQuery} $btn - Botón de confirmación
 * @param {number} cantidadProveedores - Cantidad de proveedores procesados
 */
function procesarRespuestaConfirmacion(response, $btn, cantidadProveedores) {
    // Validar estructura de respuesta
    if (!response || typeof response !== 'object') {
        manejarErrorConfirmacion('Respuesta inválida del servidor', $btn);
        return;
    }

    // Verificar errores críticos primero
    if (response.error === true) {
        manejarErrorConfirmacion(response.msg || 'Error crítico en el servidor', $btn);
        return;
    }

    // Verificar advertencias y autenticación
    if (response.warn === true) {
        // Si auth = true, es un problema de autenticación
        if (response.auth === true) {
            manejarErrorAutenticacion($btn);
            return;
        }

        // Si auth = false o undefined, es una advertencia de negocio
        manejarAdvertenciaConfirmacion(response.msg || 'Advertencia del sistema', $btn);
        return;
    }

    // Si no hay errores ni advertencias, es éxito
    manejarExitoConfirmacion(response.msg || 'Actualización completada exitosamente', $btn, cantidadProveedores);
}

/**
 * Maneja errores de confirmación
 */
function manejarErrorConfirmacion(mensaje, $btn) {
    // Limpiar estado del modal antes de mostrar error
    limpiarEstadoModal();

    AbrirMensaje("Error", mensaje,
        () => $("#msjModal").modal("hide"),
        false, ["Aceptar"], "error!", null);

    // Restaurar botón
    restaurarBotonConfirmar($btn);
}

/**
 * Maneja advertencias de confirmación (no relacionadas con auth)
 */
function manejarAdvertenciaConfirmacion(mensaje, $btn) {
    // Limpiar estado del modal antes de mostrar error
    limpiarEstadoModal();

    AbrirMensaje("Advertencia", mensaje,
        () => $("#msjModal").modal("hide"),
        false, ["Aceptar"], "warn!", null);

    // Restaurar botón
    restaurarBotonConfirmar($btn);
}

/**
 * Maneja error de autenticación con redirección a home
 */
function manejarErrorAutenticacion($btn) {
    // Limpiar estado del modal antes de mostrar error
    limpiarEstadoModal();

    AbrirMensaje("Sesión Expirada", "Su sesión ha terminado. Debe volver a autenticarse.",
        () => {
            $("#msjModal").modal("hide");
            // Redirigir a home usando variable global
            window.location.href = home;
        },
        false, ["Aceptar"], "warn!", null);

    // Restaurar botón (aunque no se verá por la redirección)
    restaurarBotonConfirmar($btn);
}

/**
 * Maneja éxito de confirmación
 */
function manejarExitoConfirmacion(mensaje, $btn, cantidadProveedores) {
    // Cambiar estado visual del botón a completado
    $btn.html('<i class="bx bxs-check-circle text-success me-2"></i><span class="text-success">COMPLETADO</span>')
        .removeClass('btn-success')
        .addClass('btn-outline-success')
        .prop('disabled', true);

    // Limpiar selecciones
    $('.proveedor-check').prop('checked', false);
    $('#selectAllProveedores').prop('checked', false);
    actualizarContadores();

    // Limpiar estado del modal antes de mostrar éxito
    limpiarEstadoModal();

    // Mostrar mensaje de éxito
    AbrirMensaje("Actualización Completada",
        `${mensaje}<br><small class="text-muted">Se procesaron ${cantidadProveedores} proveedores exitosamente.</small>`,
        () => {
            $("#msjModal").modal("hide");
            // Recargar grid para reflejar cambios
            ActualizarPP.recargarProveedores();
        },
        false, ["Aceptar"], "succ!", null);

    console.log('Actualización completada exitosamente');
}

/**
 * Función utilitaria para restaurar el estado original del botón
 * @param {jQuery} $btn - Botón a restaurar
 */
function restaurarBotonConfirmar($btn) {
    $btn.prop('disabled', false)
        .html('<i class="bx bx-check-circle me-2"></i><div class="d-flex flex-column"><span class="fw-bold">CONFIRMAR</span><small class="opacity-75">Aplicar cambios</small></div>');
}
// Función cancelar permanece igual
function cancelarActualizacion() {
    // Limpiar estado del modal antes de mostrar cancelación
    limpiarEstadoModal();

    AbrirMensaje("Cancelar Selección", '¿Cancelar y descartar todos los cambios?',
        () => {
            $("#msjModal").modal("hide");
            // Limpiar selecciones
            $('.proveedor-check').prop('checked', false);
            $('#selectAllProveedores').prop('checked', false);
            actualizarContadores();

            console.log('Actualizaciones canceladas');
        },
        false,
        ["Continuar", "Cancelar"],
        "warn!",
        null
    );
}

/**
* Limpia el estado visual del modal antes de mostrar un nuevo mensaje
*/
function limpiarEstadoModal() {
    // Limpiar clases de estado del modal
    const $modal = $('#msjModal');
    if ($modal.length) {
        // Remover clases de estado de Bootstrap
        $modal.removeClass('modal-error modal-warning modal-success modal-info modal-danger');

        // Limpiar header del modal
        const $modalHeader = $modal.find('.modal-header');
        if ($modalHeader.length) {
            $modalHeader.removeClass('bg-danger bg-warning bg-success bg-info text-white text-dark');
        }

        // Limpiar contenido del modal
        const $modalBody = $modal.find('.modal-body');
        if ($modalBody.length) {
            $modalBody.removeClass('text-danger text-warning text-success text-info');
        }
    }
}