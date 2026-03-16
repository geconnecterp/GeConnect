// ======================================================================
// INICIALIZACIÓN DEL MÓDULO
// ======================================================================

$(function () {
    console.log('✅ Módulo OR inicializado');

    // Inicializar componentes
    inicializarEventos();

    // Cargar datos iniciales
    cargarOrdenesReparto();
});

// ======================================================================
// FUNCIONES DE INICIALIZACIÓN
// ======================================================================

/**
 * Inicializa todos los event listeners del módulo
 */
function inicializarEventos() {
    console.log('🔧 Inicializando eventos...');

    // Evento de click en filas del grid
    $('#tbGridOrdenesReparto tbody').on('click', 'tr[data-or-compte]', function (e) {
        e.preventDefault();
        seleccionarOrden($(this));
    });

    // Evento de botón refrescar (si existe)
    $('#btnRefrescar').on('click', function (e) {
        e.preventDefault();
        console.log('🔄 Refrescando órdenes...');
        cargarOrdenesReparto(true);
    });

    // Evento de botón continuar
    $('#btnContinuar').on('click', function (e) {
        e.preventDefault();
        continuarConOrden();
    });

    // Evento de doble tap para móviles
    let lastTap = 0;
    $('#tbGridOrdenesReparto tbody').on('touchend', 'tr[data-or-compte]', function (e) {
        const currentTime = new Date().getTime();
        const tapLength = currentTime - lastTap;

        if (tapLength < 500 && tapLength > 0) {
            // Doble tap detectado
            const $row = $(this);
            seleccionarOrden($row);
            verDetalleOrden($row.data('or-compte'));
        }

        lastTap = currentTime;
    });

    console.log('✅ Eventos inicializados correctamente');
}

// ======================================================================
// FUNCIONES PRINCIPALES - CARGA DE DATOS
// ======================================================================

/**
 * Carga las órdenes de reparto desde el servidor
 * @param {boolean} forzarRecarga - Indica si se debe forzar la recarga ignorando cache
 */
function cargarOrdenesReparto(forzarRecarga = false) {
    console.log('📡 Iniciando carga de órdenes de reparto...');

    // Mostrar indicador de carga
    mostrarIndicadorCarga();

    // Realizar petición AJAX
    $.ajax({
        url: ObtenerOrdenesRepartoUrl,
        type: 'POST',
        contentType: 'application/json',
        dataType: 'json',
        timeout: 30000, // 30 segundos timeout
        beforeSend: function () {
            // Deshabilitar botón de refrescar durante la carga
            $('#btnRefrescar').prop('disabled', true);
        },
        success: function (response) {
            console.log('✅ Respuesta recibida:', response);
            procesarRespuestaOrdenes(response);
        },
        error: function (xhr, status, error) {
            console.error('❌ Error al cargar órdenes:', error);
            manejarErrorCarga(xhr, status, error);
        },
        complete: function () {
            // Rehabilitar botón de refrescar
            $('#btnRefrescar').prop('disabled', false);
            ocultarIndicadorCarga();
        }
    });
}

/**
 * Procesa la respuesta del servidor y renderiza los datos
 * @param {object} response - Respuesta del servidor
 */
function procesarRespuestaOrdenes(response) {
    console.log('🔄 Procesando respuesta...');

    if (!response) {
        AbrirMensaje(
            "ERROR",
            "Respuesta del servidor inválida",
            function () {
                $("#msjModal").modal("hide");
                return true;
            },
            false,
            ["Aceptar"],
            "error!",
            null
        );
        return;
    }

    if (response.success) {
        const ordenes = response.data || [];
        console.log(`📦 Total de órdenes recibidas: ${ordenes.length}`);

        // Actualizar cache
        OR.cache.ordenesActuales = ordenes;

        // Renderizar las órdenes en el grid
        renderizarOrdenes(ordenes);

        // Actualizar contadores y estadísticas
        actualizarEstadisticas(ordenes);
    } else {
        const mensaje = response.message || 'Error desconocido al obtener órdenes';
        console.warn('⚠️ Error en respuesta:', mensaje);
        AbrirMensaje(
            "ADVERTENCIA",
            mensaje,
            function () {
                $("#msjModal").modal("hide");
                return true;
            },
            false,
            ["Aceptar"],
            "warn!",
            null
        );
        renderizarOrdenesVacio();
    }
}

// ======================================================================
// FUNCIONES DE RENDERIZADO
// ======================================================================

/**
 * Renderiza las órdenes en el grid
 * @param {Array} ordenes - Array de órdenes de reparto
 */
function renderizarOrdenes(ordenes) {
    console.log('🎨 Renderizando órdenes en el grid...');

    const $tbody = $('#tbGridOrdenesReparto tbody');
    $tbody.empty();

    if (!ordenes || ordenes.length === 0) {
        renderizarOrdenesVacio();
        return;
    }

    // Crear fragmento para mejor performance
    const fragment = document.createDocumentFragment();

    ordenes.forEach((orden, index) => {
        const $row = crearFilaOrden(orden, index);
        fragment.appendChild($row[0]);
    });

    // Insertar todas las filas de una vez
    $tbody.append(fragment);

    // Aplicar animación de entrada
    aplicarAnimacionEntrada();

    console.log(`✅ ${ordenes.length} órdenes renderizadas correctamente`);
}

/**
 * Crea una fila de orden para el grid
 * @param {object} orden - Objeto orden de reparto
 * @param {number} index - Índice de la orden
 * @returns {jQuery} Elemento jQuery de la fila
 */
function crearFilaOrden(orden, index) {
    // Formatear datos
    const numeroOrden = orden.or_compte || 'N/A';
    const repartidor = orden.rp_nombre || 'Sin asignar';
    const fecha = formatearFecha(orden.or_fecha);
    const estado = orden.ore_id || 'S/E';

    // Crear fila con data attributes
    const $row = $('<tr>')
        .attr('data-or-compte', orden.or_compte)
        .attr('data-estado', estado)
        .attr('data-index', index)
        .addClass('fade-in')
        .css('animation-delay', `${index * 0.05}s`);

    // Columna 1: N° Orden con badge
    const $colNumero = $('<td>')
        .addClass('text-center td-compact')
        .html(crearBadgeOrden(numeroOrden, estado));

    // Columna 2: Repartidor
    const $colRepartidor = $('<td>')
        .addClass('td-compact')
        .text(repartidor);

    // Columna 3: Fecha
    const $colFecha = $('<td>')
        .addClass('text-center td-compact')
        .text(fecha);

    // Ensamblar fila
    $row.append($colNumero, $colRepartidor, $colFecha);

    return $row;
}

/**
 * Crea un badge para el número de orden según su estado
 * @param {string} numero - Número de orden
 * @param {string} estado - Estado de la orden (O=Pendiente, T=Terminado, etc)
 * @returns {string} HTML del badge
 */
function crearBadgeOrden(numero, estado) {
    let claseEstado = 'bg-primary';
    let textoEstado = '';

    switch (estado.toUpperCase()) {
        case 'O':
            claseEstado = 'bg-warning';
            textoEstado = 'title="Orden Pendiente"';
            break;
        case 'T':
            claseEstado = 'bg-success';
            textoEstado = 'title="Orden Terminada"';
            break;
        case 'C':
            claseEstado = 'bg-danger';
            textoEstado = 'title="Orden Cancelada"';
            break;
        default:
            claseEstado = 'bg-primary';
            textoEstado = 'title="Orden Activa"';
    }

    return `<span class="badge ${claseEstado}" ${textoEstado}>${numero}</span>`;
}

/**
 * Renderiza mensaje cuando no hay órdenes
 */
function renderizarOrdenesVacio() {
    const $tbody = $('#tbGridOrdenesReparto tbody');
    $tbody.empty();

    const $row = $('<tr>').addClass('no-data-row');
    const $col = $('<td>')
        .attr('colspan', '3')
        .addClass('text-center text-muted py-4')
        .html('<i class="bx bx-info-circle me-2"></i>No hay órdenes de reparto disponibles');

    $row.append($col);
    $tbody.append($row);

    console.log('ℹ️ Renderizado mensaje: Sin órdenes disponibles');
}

// ======================================================================
// FUNCIONES DE INTERACCIÓN
// ======================================================================

/**
 * Selecciona una orden en el grid
 * @param {jQuery} $row - Fila jQuery seleccionada
 */
function seleccionarOrden($row) {
    if (!$row || $row.length === 0) return;

    // Remover selección previa
    $('#tbGridOrdenesReparto tbody').find('tr').removeClass('selected');

    // Agregar selección actual
    $row.addClass('selected');

    // Guardar orden seleccionada en cache
    const ordenId = $row.data('or-compte');
    OR.cache.ordenSeleccionada = OR.cache.ordenesActuales.find(
        orden => orden.or_compte === ordenId
    );

    console.log('✅ Orden seleccionada:', ordenId);

    // Mostrar botón continuar
    $('#btnContinuar').fadeIn(OR.config.animacionDuracion);

    // Disparar evento personalizado para otros módulos
    $(document).trigger('ordenSeleccionada', [OR.cache.ordenSeleccionada]);

    // Feedback haptic para móviles
    if ('vibrate' in navigator) {
        navigator.vibrate(50);
    }
}

/**
 * Ver detalle de una orden (placeholder para futuras implementaciones)
 * @param {string} ordenId - ID de la orden
 */
function verDetalleOrden(ordenId) {
    console.log('🔍 Ver detalle de orden:', ordenId);
    AbrirMensaje(
        "INFORMACIÓN",
        `Detalle de orden ${ordenId} - Próximamente`,
        function () {
            $("#msjModal").modal("hide");
            return true;
        },
        false,
        ["Aceptar"],
        "info!",
        null
    );
}

/**
 * ✅ MODIFICADO: Procesa la acción de continuar con la orden seleccionada
 * Ahora incluye validación de usuario antes de continuar
 */
function continuarConOrden() {
    console.log('🚀 Continuar con orden...');

    // 1. Validar que hay una orden seleccionada
    if (!OR.cache.ordenSeleccionada) {
        AbrirMensaje(
            "ADVERTENCIA",
            "Por favor, seleccione una orden primero",
            function () {
                $("#msjModal").modal("hide");
                return true;
            },
            false,
            ["Aceptar"],
            "warn!",
            null
        );
        return;
    }

    // 2. Obtener ID de la orden
    const ordenId = OR.cache.ordenSeleccionada.or_compte;

    // 3. Validar que existe el ID de orden
    if (!ordenId) {
        console.error('❌ ID de orden no disponible');
        AbrirMensaje(
            "ERROR",
            "No se pudo obtener el ID de la orden",
            function () {
                $("#msjModal").modal("hide");
                return true;
            },
            false,
            ["Aceptar"],
            "error!",
            null
        );
        return;
    }

    // 4. Obtener ID de usuario desde variable global
    const usuarioId = typeof usuarioAuth !== 'undefined' ? usuarioAuth : null;

    // 5. Validar que existe el ID de usuario
    if (!usuarioId) {
        console.error('❌ ID de usuario no disponible');
        AbrirMensaje(
            "ERROR",
            "No se pudo identificar el usuario. Por favor, inicie sesión nuevamente.",
            function () {
                $("#msjModal").modal("hide");
                return true;
            },
            false,
            ["Aceptar"],
            "error!",
            null
        );
        return;
    }

    console.log(`📦 Validando usuario ${usuarioId} para orden ${ordenId}...`);

    // 6. Llamar a función de validación
    validarUsuarioParaOrden(ordenId, usuarioId);
}

/**
 * ✅ NUEVA FUNCIÓN: Valida el usuario para procesar la orden
 * @param {string} orCompte - ID del comprobante de orden
 * @param {string} usuId - ID del usuario
 */
function validarUsuarioParaOrden(orCompte, usuId) {
    console.log('🔐 Iniciando validación de usuario...');

    // Mostrar indicador de espera
    AbrirWaiting('Validando usuario...');

    // Realizar petición AJAX
    $.ajax({
        url: ValidarUsuarioUrl,
        type: 'POST',
        data: {
            orCompte: orCompte,
            usuId: usuId
        },
        timeout: 30000,
        success: function (response) {
            CerrarWaiting();
            console.log('✅ Respuesta de validación recibida:', response);
            procesarRespuestaValidacion(response, orCompte);
        },
        error: function (xhr, status, error) {
            CerrarWaiting();
            console.error('❌ Error al validar usuario:', error);

            let mensajeError = 'Error al validar usuario';

            switch (xhr.status) {
                case 401:
                    mensajeError = 'Sesión expirada. Por favor, inicie sesión nuevamente.';
                    AbrirMensaje(
                        "ERROR",
                        mensajeError,
                        function () {
                            $("#msjModal").modal("hide");
                            window.location.href = '/seguridad/token/login';
                            return true;
                        },
                        false,
                        ["Aceptar"],
                        "error!",
                        null
                    );
                    return;
                case 403:
                    mensajeError = 'No tiene permisos para procesar esta orden.';
                    break;
                case 404:
                    mensajeError = 'Servicio de validación no disponible.';
                    break;
                case 500:
                    mensajeError = 'Error en el servidor. Intente nuevamente.';
                    break;
                default:
                    mensajeError = `Error: ${error || 'Desconocido'}`;
            }

            AbrirMensaje(
                "ERROR",
                mensajeError,
                function () {
                    $("#msjModal").modal("hide");
                    return true;
                },
                false,
                ["Aceptar"],
                "error!",
                null
            );
        }
    });
}

/**
 * ✅ NUEVA FUNCIÓN: Procesa la respuesta de validación de usuario
 * @param {object} response - Respuesta del servidor
 * @param {string} orCompte - ID de la orden
 */
function procesarRespuestaValidacion(response, orCompte) {
    console.log('🔄 Procesando respuesta de validación...');

    // Validar respuesta
    if (!response) {
        AbrirMensaje(
            "ERROR",
            "Respuesta de validación inválida",
            function () {
                $("#msjModal").modal("hide");
                return true;
            },
            false,
            ["Aceptar"],
            "error!",
            null
        );
        return;
    }

    if (!response.success) {
        // ⚠️ Validación fallida - Mostrar warning
        const mensaje = response.message || 'Usuario no autorizado para procesar esta orden';
        console.warn('⚠️ Validación fallida:', mensaje);

        AbrirMensaje(
            "ADVERTENCIA",
            mensaje,
            function () {
                $("#msjModal").modal("hide");
                return true;
            },
            false,
            ["Aceptar"],
            "warn!",
            null
        );
        return;
    }

    // ✅ Validación exitosa
    console.log('✅ Usuario validado correctamente');

    procesarOrden(orCompte);
}

/**
 * ✅ MODIFICADO: Procesa la orden después de validar el usuario
 * Redirige a la vista de lista de OR para continuar con el proceso
 * @param {string} orCompte - ID del comprobante de orden
 */
function procesarOrden(orCompte) {
    console.log('🚀 Procesando orden:', orCompte);

    // Validar que existe el ID de orden
    if (!orCompte) {
        console.error('❌ ID de orden no disponible');
        AbrirMensaje(
            "ERROR",
            "No se pudo obtener el ID de la orden",
            function () {
                $("#msjModal").modal("hide");
                return true;
            },
            false,
            ["Aceptar"],
            "error!",
            null
        );
        return;
    }

    // Mostrar indicador de carga durante la transición
    AbrirWaiting('Cargando orden de reparto...');

    // Feedback haptic para dispositivos móviles
    if ('vibrate' in navigator) {
        navigator.vibrate([100, 50, 100]);
    }

    // Construir URL para la action CargaORLista
    const url = `${AbrirOrListaUrl}?or_compte=${encodeURIComponent(orCompte)}`;

    console.log(`📍 Navegando a: ${url}`);

    // Redirigir a la vista de lista de OR
    window.location.href = url;
}

// ======================================================================
// FUNCIONES DE UTILIDAD - FORMATEO
// ======================================================================

/**
 * Formatea una fecha para mostrar en el grid
 * @param {string|Date} fecha - Fecha a formatear
 * @returns {string} Fecha formateada
 */
function formatearFecha(fecha) {
    if (!fecha) return 'N/A';

    try {
        const fechaObj = new Date(fecha);
        const dia = String(fechaObj.getDate()).padStart(2, '0');
        const mes = String(fechaObj.getMonth() + 1).padStart(2, '0');
        const anio = fechaObj.getFullYear();

        return `${dia}/${mes}/${anio}`;
    } catch (error) {
        console.error('Error al formatear fecha:', error);
        return 'Fecha inválida';
    }
}

/**
 * Obtiene la fecha/hora actual formateada
 * @returns {string} Fecha y hora actual
 */
function obtenerFechaHoraActual() {
    const ahora = new Date();
    const dia = String(ahora.getDate()).padStart(2, '0');
    const mes = String(ahora.getMonth() + 1).padStart(2, '0');
    const anio = ahora.getFullYear();
    const hora = String(ahora.getHours()).padStart(2, '0');
    const minuto = String(ahora.getMinutes()).padStart(2, '0');

    return `${dia}/${mes}/${anio} ${hora}:${minuto}`;
}

// ======================================================================
// FUNCIONES DE UI - FEEDBACK VISUAL
// ======================================================================

/**
 * Muestra indicador de carga en el grid
 */
function mostrarIndicadorCarga() {
    const $tbody = $('#tbGridOrdenesReparto tbody');
    $tbody.empty();

    const $row = $('<tr>');
    const $col = $('<td>')
        .attr('colspan', '3')
        .addClass('text-center py-4')
        .html(`
            <div class="spinner-border spinner-border-golden" role="status">
                <span class="visually-hidden">Cargando...</span>
            </div>
            <p class="mt-2 mb-0 text-muted">Cargando órdenes de reparto...</p>
        `);

    $row.append($col);
    $tbody.append($row);
}

/**
 * Oculta el indicador de carga
 */
function ocultarIndicadorCarga() {
    // El indicador se elimina al renderizar los datos o el mensaje de error
}

/**
 * Aplica animación de entrada a las filas
 */
function aplicarAnimacionEntrada() {
    const $filas = $('#tbGridOrdenesReparto tbody').find('tr[data-or-compte]');

    $filas.each(function (index) {
        const $fila = $(this);
        setTimeout(() => {
            $fila.addClass('animate-in');
        }, index * 50);
    });
}

/**
 * Actualiza las estadísticas del footer (si existe)
 * @param {Array} ordenes - Array de órdenes
 */
function actualizarEstadisticas(ordenes) {
    // Actualizar total de órdenes
    $(OR.dom.totalOrdenes).text(ordenes.length);

    // Actualizar fecha de última actualización
    $(OR.dom.ultimaActualizacion).text(obtenerFechaHoraActual());
}

// ======================================================================
// FUNCIONES DE MANEJO DE ERRORES
// ======================================================================

/**
 * Maneja errores en la carga de datos
 * @param {object} xhr - Objeto XMLHttpRequest
 * @param {string} status - Estado del error
 * @param {string} error - Mensaje de error
 */
function manejarErrorCarga(xhr, status, error) {
    console.error('❌ Error detallado:', {
        status: xhr.status,
        statusText: xhr.statusText,
        responseText: xhr.responseText,
        error: error
    });

    let mensajeError = 'Error al cargar las órdenes de reparto';

    // Mensajes específicos según tipo de error
    switch (xhr.status) {
        case 0:
            mensajeError = 'Sin conexión a internet. Verifica tu conexión.';
            break;
        case 401:
            mensajeError = 'Sesión expirada. Por favor, inicia sesión nuevamente.';
            // Mostrar mensaje y redirigir
            AbrirMensaje(
                "ERROR",
                mensajeError,
                function () {
                    $("#msjModal").modal("hide");
                    window.location.href = '/seguridad/token/login';
                    return true;
                },
                false,
                ["Aceptar"],
                "error!",
                null
            );
            return;
        case 403:
            mensajeError = 'No tienes permisos para acceder a esta información.';
            break;
        case 404:
            mensajeError = 'El servicio no está disponible.';
            break;
        case 500:
            mensajeError = 'Error en el servidor. Intenta nuevamente más tarde.';
            break;
        case 'timeout':
            mensajeError = 'La solicitud tardó demasiado. Verifica tu conexión.';
            break;
        default:
            mensajeError = `Error: ${error || 'Desconocido'}`;
    }

    AbrirMensaje(
        "ERROR",
        mensajeError,
        function () {
            $("#msjModal").modal("hide");
            return true;
        },
        false,
        ["Aceptar"],
        "error!",
        null
    );
    renderizarOrdenesVacio();
}

// ======================================================================
// FUNCIONES PÚBLICAS - API DEL MÓDULO
// ======================================================================

/**
 * API pública del módulo OR
 */
window.OR = window.OR || {};

Object.assign(window.OR, {
    // Funciones principales
    recargar: () => cargarOrdenesReparto(true),
    obtenerOrdenSeleccionada: () => OR.cache.ordenSeleccionada,
    obtenerOrdenes: () => OR.cache.ordenesActuales,

    // Funciones de búsqueda/filtrado
    buscarOrden: (ordenId) => {
        return OR.cache.ordenesActuales.find(o => o.or_compte === ordenId);
    },

    // Funciones de utilidad
    limpiarSeleccion: () => {
        $('#tbGridOrdenesReparto tbody').find('tr').removeClass('selected');
        OR.cache.ordenSeleccionada = null;
        $('#btnContinuar').fadeOut(OR.config.animacionDuracion);
    }
});

console.log('📦 API del módulo OR expuesta globalmente');

// ======================================================================
// CSS ADICIONAL INLINE PARA ANIMACIONES
// ======================================================================

// Inyectar estilos de animación si no existen
if (!document.getElementById('or-animations-style')) {
    const style = document.createElement('style');
    style.id = 'or-animations-style';
    style.textContent = `
        @keyframes fadeInUp {
            from {
                opacity: 0;
                transform: translateY(10px);
            }
            to {
                opacity: 1;
                transform: translateY(0);
            }
        }

        #tbGridOrdenesReparto tbody tr.fade-in {
            animation: fadeInUp 0.3s ease-out forwards;
            opacity: 0;
        }

        #tbGridOrdenesReparto tbody tr.animate-in {
            opacity: 1;
        }

        /* Efecto de pulsación en touch */
        #tbGridOrdenesReparto tbody tr:active {
            background-color: rgba(212, 175, 55, 0.3) !important;
        }
    `;
    document.head.appendChild(style);
}

// ======================================================================
// FIN DEL MÓDULO
// ======================================================================

console.log('🎉 Módulo orCore.js cargado completamente - Versión 1.0.3');