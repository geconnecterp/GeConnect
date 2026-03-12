// ======================================================================
// OR LISTA - MÓDULO DE ITINERARIO POR BOX Y RUBRO
// ======================================================================

$(function () {
    console.log('✅ Módulo OR Lista inicializado');

    // Inicializar eventos
    inicializarEventos();

    // Cargar vista por defecto (BOX)
    cargarVistaOrByBox();
});

// ======================================================================
// INICIALIZACIÓN DE EVENTOS
// ======================================================================

function inicializarEventos() {
    console.log('🔧 Inicializando eventos de OR Lista...');

    // Evento cambio de radiobutton BOX
    $("#radioBox").on("change", function () {
        if ($(this).is(":checked")) {
            console.log('📦 Cambiando a vista BOX');
            
            // ✅ Limpiar cache de RUBRO al cambiar a BOX
            if (typeof OR_CACHE_API !== 'undefined') {
                OR_CACHE_API.limpiarRubro();
            }
            
            // ✅ Ocultar botón continuar al cambiar de vista
            ocultarBotonContinuar();
            
            cargarVistaOrByBox();
        }
    });

    // Evento cambio de radiobutton RUBRO
    $("#radioRub").on("change", function () {
        if ($(this).is(":checked")) {
            console.log('🏷️ Cambiando a vista RUBRO');
            
            // ✅ Limpiar cache de BOX al cambiar a RUBRO
            if (typeof OR_CACHE_API !== 'undefined') {
                OR_CACHE_API.limpiarBox();
            }
            
            // ✅ Ocultar botón continuar al cambiar de vista
            ocultarBotonContinuar();
            
            cargarVistaOrByRubro();
        }
    });

    // ✅ NUEVO: Evento del botón Continuar
    $("#btnContinuar").on("click", function () {
        console.log('🚀 Botón Continuar presionado');
        procesarContinuar();
    });

    console.log('✅ Eventos de OR Lista inicializados');
}

// ======================================================================
// FUNCIONES DE CARGA DE VISTAS
// ======================================================================

/**
 * Carga la vista de lista de OR por BOX
 */
function cargarVistaOrByBox() {
    console.log('📡 Cargando lista OR por BOX...');

    // Validar parámetros
    if (!orCompteActual || orCompteActual.trim() === '') {
        AbrirMensaje(
            "ERROR",
            "No se encontró el número de orden de reparto",
            function() {
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

    if (!admAuth || admAuth.trim() === '') {
        AbrirMensaje(
            "ERROR",
            "No se encontró la administración del usuario",
            function() {
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

    if (!usuAuth || usuAuth.trim() === '') {
        AbrirMensaje(
            "ERROR",
            "No se encontró el ID del usuario",
            function() {
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

    // Mostrar indicador de carga
    AbrirWaiting('Cargando itinerario por BOX...');

    // Preparar datos
    const datos = {
        or_compte: orCompteActual,
        adm: admAuth,
        usu: usuAuth
    };

    // Realizar petición
    PostGenHtml(datos, PresentarListaORbyBoxUrl, function (html) {
        console.log('✅ Vista BOX cargada correctamente');
        $("#contenedorListaOR").html(html);
        CerrarWaiting();

        // Inicializar eventos de la tabla
        inicializarEventosTablaBox();
    }, function (xhr, status, error) {
        console.error('❌ Error al cargar vista BOX:', error);
        CerrarWaiting();
        manejarErrorCarga(xhr, status, error);
    });
}

/**
 * Carga la vista de lista de OR por RUBRO
 */
function cargarVistaOrByRubro() {
    console.log('📡 Cargando lista OR por RUBRO...');

    // Validar parámetros
    if (!orCompteActual || orCompteActual.trim() === '') {
        AbrirMensaje(
            "ERROR",
            "No se encontró el número de orden de reparto",
            function() {
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

    if (!admAuth || admAuth.trim() === '') {
        AbrirMensaje(
            "ERROR",
            "No se encontró la administración del usuario",
            function() {
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

    if (!usuAuth || usuAuth.trim() === '') {
        AbrirMensaje(
            "ERROR",
            "No se encontró el ID del usuario",
            function() {
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

    // Mostrar indicador de carga
    AbrirWaiting('Cargando itinerario por RUBRO...');

    // Preparar datos
    const datos = {
        or_compte: orCompteActual,
        adm: admAuth,
        usu: usuAuth
    };

    // Realizar petición
    PostGenHtml(datos, PresentarListaORbyRubroUrl, function (html) {
        console.log('✅ Vista RUBRO cargada correctamente');
        $("#contenedorListaOR").html(html);
        CerrarWaiting();

        // Inicializar eventos de la tabla
        inicializarEventosTablaRubro();
    }, function (xhr, status, error) {
        console.error('❌ Error al cargar vista RUBRO:', error);
        CerrarWaiting();
        manejarErrorCarga(xhr, status, error);
    });
}

// ======================================================================
// EVENTOS DE INTERACCIÓN CON TABLAS
// ======================================================================

/**
 * ✅ MODIFICADO: Inicializa eventos para la tabla de BOX con cache y habilita botón
 */
function inicializarEventosTablaBox() {
    $("#tbOrBoxList tbody").off("click", "tr").on("click", "tr", function () {
        const $row = $(this);
        
        // Obtener datos del row
        const boxId = $row.data("box-id");
        const depoId = $row.data("depo-id");
        const conteo = $row.data("conteo");

        console.log('📦 BOX seleccionado:', { boxId, depoId, conteo });

        // ✅ Guardar en cache usando la API global
        if (typeof OR_CACHE_API !== 'undefined') {
            OR_CACHE_API.guardarBox(boxId, depoId, conteo);
        } else {
            console.error('❌ OR_CACHE_API no disponible');
        }

        // Aplicar selección visual
        $("#tbOrBoxList tbody tr").removeClass("selected-row");
        $row.addClass("selected-row");

        // ✅ Habilitar botón continuar
        habilitarBotonContinuar();

        // Feedback haptic para móviles
        if ('vibrate' in navigator) {
            navigator.vibrate(50);
        }

        // ✅ Disparar evento personalizado con datos del cache
        $(document).trigger('boxSeleccionado', [OR_CACHE_API.obtenerBox()]);
    });
}

/**
 * ✅ MODIFICADO: Inicializa eventos para la tabla de RUBRO con cache y habilita botón
 */
function inicializarEventosTablaRubro() {
    $("#tbOrRubList tbody").off("click", "tr").on("click", "tr", function () {
        const $row = $(this);
        
        // Obtener datos del row
        const rubId = $row.data("rub-id");
        const rubgId = $row.data("rubg-id");
        const conteo = $row.data("conteo");

        console.log('🏷️ RUBRO seleccionado:', { rubId, rubgId, conteo });

        // ✅ Guardar en cache usando la API global
        if (typeof OR_CACHE_API !== 'undefined') {
            OR_CACHE_API.guardarRubro(rubId, rubgId, conteo);
        } else {
            console.error('❌ OR_CACHE_API no disponible');
        }

        // Aplicar selección visual
        $("#tbOrRubList tbody tr").removeClass("selected-row");
        $row.addClass("selected-row");

        // ✅ Habilitar botón continuar
        habilitarBotonContinuar();

        // Feedback haptic para móviles
        if ('vibrate' in navigator) {
            navigator.vibrate(50);
        }

        // ✅ Disparar evento personalizado con datos del cache
        $(document).trigger('rubroSeleccionado', [OR_CACHE_API.obtenerRubro()]);
    });
}

// ======================================================================
// ✅ NUEVAS FUNCIONES DE CONTROL DEL BOTÓN CONTINUAR
// ======================================================================

/**
 * Habilita y muestra el botón Continuar con animación
 */
function habilitarBotonContinuar() {
    const $btnContinuar = $("#btnContinuar");
    
    if ($btnContinuar.length === 0) {
        console.warn('⚠️ Botón Continuar no encontrado en el DOM');
        return;
    }

    // Habilitar y mostrar con fadeIn
    $btnContinuar.prop('disabled', false).fadeIn(300);
    
    console.log('✅ Botón Continuar habilitado');
}

/**
 * Oculta el botón Continuar con animación
 */
function ocultarBotonContinuar() {
    const $btnContinuar = $("#btnContinuar");
    
    if ($btnContinuar.length === 0) {
        return;
    }

    // Ocultar con fadeOut
    $btnContinuar.fadeOut(300, function() {
        $(this).prop('disabled', true);
    });
    
    console.log('🔒 Botón Continuar ocultado');
}

/**
 * Deshabilita el botón Continuar sin ocultarlo
 */
function deshabilitarBotonContinuar() {
    const $btnContinuar = $("#btnContinuar");
    
    if ($btnContinuar.length === 0) {
        return;
    }

    $btnContinuar.prop('disabled', true);
    
    console.log('🔒 Botón Continuar deshabilitado');
}

// ======================================================================
// ✅ NUEVA FUNCIÓN: PROCESAR CONTINUAR
// ======================================================================

/**
 * Procesa la acción del botón Continuar según el tipo de vista activa
 */
function procesarContinuar() {
    console.log('🚀 Procesando acción Continuar...');

    // Determinar qué vista está activa
    const vistaActiva = $("#radioBox").is(":checked") ? "BOX" : "RUB";
    
    console.log(`📋 Vista activa: ${vistaActiva}`);

    // Validar que hay datos en cache según la vista
    if (vistaActiva === "BOX") {
        if (typeof OR_CACHE_API === 'undefined' || !OR_CACHE_API.validarBox()) {
            AbrirMensaje(
                "ADVERTENCIA",
                "Por favor, seleccione un BOX antes de continuar",
                function() {
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

        // Obtener datos del BOX
        const boxData = OR_CACHE_API.obtenerBox();
        console.log('📦 Datos del BOX a procesar:', boxData);

        // TODO: Implementar lógica específica para BOX
        procesarContinuarBox(boxData);

    } else {
        if (typeof OR_CACHE_API === 'undefined' || !OR_CACHE_API.validarRubro()) {
            AbrirMensaje(
                "ADVERTENCIA",
                "Por favor, seleccione un RUBRO antes de continuar",
                function() {
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

        // Obtener datos del RUBRO
        const rubroData = OR_CACHE_API.obtenerRubro();
        console.log('🏷️ Datos del RUBRO a procesar:', rubroData);

        // TODO: Implementar lógica específica para RUBRO
        procesarContinuarRubro(rubroData);
    }
}

/**
 * ✅ IMPLEMENTADO: Procesa continuar con BOX seleccionado
 * Navega a la vista de carga de carrito con el BOX_ID
 * @param {object} boxData - Datos del BOX desde cache
 */
function procesarContinuarBox(boxData) {
    console.log('📦 Procesando continuar con BOX:', boxData);

    // Validar que tenemos el ID del BOX
    if (!boxData.box_id) {
        console.error('❌ BOX ID no disponible');
        AbrirMensaje(
            "ERROR",
            "No se pudo obtener el ID del BOX seleccionado",
            function() {
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

    // Mostrar indicador de carga
    AbrirWaiting('Cargando productos del BOX...');

    // Feedback haptic para dispositivos móviles
    if ('vibrate' in navigator) {
        navigator.vibrate([100, 50, 100]);
    }

    // ✅ Construir URL con parámetro box_id
    const url = `${ORCargaCarritoUrl}?box_id=${encodeURIComponent(boxData.box_id)}`;

    console.log(`📍 Navegando a: ${url}`);

    // Redirigir a la vista de carga de carrito
    window.location.href = url;
}

/**
 * ✅ IMPLEMENTADO: Procesa continuar con RUBRO seleccionado
 * Navega a la vista de carga de carrito con el RUB_ID
 * @param {object} rubroData - Datos del RUBRO desde cache
 */
function procesarContinuarRubro(rubroData) {
    console.log('🏷️ Procesando continuar con RUBRO:', rubroData);

    // Validar que tenemos el ID del RUBRO
    if (!rubroData.rub_id) {
        console.error('❌ RUBRO ID no disponible');
        AbrirMensaje(
            "ERROR",
            "No se pudo obtener el ID del RUBRO seleccionado",
            function() {
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

    // Mostrar indicador de carga
    AbrirWaiting('Cargando productos del RUBRO...');

    // Feedback haptic para dispositivos móviles
    if ('vibrate' in navigator) {
        navigator.vibrate([100, 50, 100]);
    }

    // ✅ Construir URL con parámetro rub_id
    const url = `${ORCargaCarritoUrl}?rub_id=${encodeURIComponent(rubroData.rub_id)}`;

    console.log(`📍 Navegando a: ${url}`);

    // Redirigir a la vista de carga de carrito
    window.location.href = url;
}

// ======================================================================
// MANEJO DE ERRORES
// ======================================================================

/**
 * Maneja errores en las peticiones AJAX
 */
function manejarErrorCarga(xhr, status, error) {
    let mensajeError = 'Error al cargar la vista';

    switch (xhr.status) {
        case 401:
            mensajeError = 'Sesión expirada. Por favor, inicie sesión nuevamente.';
            AbrirMensaje(
                "ERROR",
                mensajeError,
                function() {
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
            mensajeError = 'No tiene permisos para acceder a esta información.';
            break;
        case 404:
            mensajeError = 'Servicio no encontrado.';
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
        function() {
            $("#msjModal").modal("hide");
            return true;
        },
        false,
        ["Aceptar"],
        "error!",
        null
    );
}

// ======================================================================
// ✅ FUNCIONES PÚBLICAS PARA ACCESO EXTERNO
// ======================================================================

/**
 * Obtiene el BOX actualmente seleccionado
 * @returns {object|null} Datos del BOX o null
 */
function obtenerBoxActual() {
    if (typeof OR_CACHE_API !== 'undefined') {
        return OR_CACHE_API.obtenerBox();
    }
    console.error('❌ OR_CACHE_API no disponible');
    return null;
}

/**
 * Obtiene el RUBRO actualmente seleccionado
 * @returns {object|null} Datos del RUBRO o null
 */
function obtenerRubroActual() {
    if (typeof OR_CACHE_API !== 'undefined') {
        return OR_CACHE_API.obtenerRubro();
    }
    console.error('❌ OR_CACHE_API no disponible');
    return null;
}

// Exponer funciones públicamente
window.OR_LISTA_API = {
    obtenerBoxActual: obtenerBoxActual,
    obtenerRubroActual: obtenerRubroActual,
    // ✅ NUEVO: Exponer control del botón
    habilitarBoton: habilitarBotonContinuar,
    ocultarBoton: ocultarBotonContinuar,
    deshabilitarBoton: deshabilitarBotonContinuar
};

console.log('🎉 Módulo orCoreLista.js cargado - Versión 1.2.0');
console.log('✅ API pública expuesta como window.OR_LISTA_API');