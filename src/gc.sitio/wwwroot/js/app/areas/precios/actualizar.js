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
        actualizarContadorSeleccionados();
    });

    $(document).on('change', '.proveedor-check', function () {
        actualizarContadorSeleccionados();
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

        // Inicializar contador después de cargar
        actualizarContadorSeleccionados();

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
 * Actualiza el contador de elementos seleccionados
 */
function actualizarContadorSeleccionados() {
    const seleccionados = $('.proveedor-check:checked').length;
    const total = $('.proveedor-check').length;

    // Actualizar contador en la interfaz
    $('#selectedCount').text(seleccionados);

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