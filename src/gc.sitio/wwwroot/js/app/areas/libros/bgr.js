let bgrDataBak = {};

$(function () {
    inicializarComponentesBgr();

    configurarBotonesBgr();

    configurarFiltrosBgr();
    
    // Inicializar vista
    inicializarVistaBgr();
});

function inicializarVistaBgr() {
    // Deshabilitar botón de impresión hasta tener resultados
    $("#btnImprimir").prop("disabled", true);

    // MODIFICAR: Simplificar el manejo de paneles - mostrar filtro y ocultar resultados directamente
    $("#divFiltro").collapse("show");
    $("#divDetalle").collapse("hide");   
}
function configurarFiltrosBgr() {
    // Forzar selección y deshabilitación del checkbox de ejercicio
    $('#chkEjercicio').prop('checked', true).prop('disabled', true);
    $('#Eje_nro').prop('disabled', false);

    // Prevenir clics en checkboxes que deben estar siempre habilitados
    $('#chkEjercicio').on('click', function (e) {
        e.preventDefault();
        return false;
    });
}

function inicializarComponentesBgr() {
    // Inicializar selectores con Select2 si está disponible
    if ($.fn.select2) {
        $("#Eje_nro").select2({
            placeholder: "Seleccione ejercicio",
            width: '100%'
        });
    }    
}

function validarCamposObligatoriosBgr() {
    let camposFaltantes = [];
    let mensajesError = [];

    // Validar ejercicio (siempre obligatorio)
    if (!$("#Eje_nro").val()) {
        camposFaltantes.push("Ejercicio contable");
    }

    
    // Si faltan campos o hay errores de validación, mostrar mensaje y devolver false
    if (camposFaltantes.length > 0 || mensajesError.length > 0) {
        let mensaje = "";

        if (camposFaltantes.length > 0) {
            mensaje += "Para realizar la búsqueda debe seleccionar valores para: " + camposFaltantes.join(", ");
        }

        if (mensajesError.length > 0) {
            if (mensaje) mensaje += "<br><br>";
            mensaje += mensajesError.join("<br>");
        }

        AbrirMensaje(
            "ATENCIÓN",
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
        return false;
    }

    return true;
}

function obtenerParametrosBusquedaBgr() {
    return {
        eje_nro: $("#Eje_nro").val(),        
    };
}

function limpiarBgr() {
    // Vaciar el panel que contiene el asiento
    $("#divBgr").empty();

    // Restablecer variables de control
    filaClicDoble = null;
    EntidadSelect = "";
    EntidadEstado = "";

    inicializarVistaBgr();
}

function buscarBgr(pag = 1) {
    AbrirWaiting("Consultando el Balance General...");

    // Desactivamos los botones de acción
    $("#btnImprimir").prop("disabled", true);

    // SIMPLIFICADO: Ocultamos filtro inmediatamente
    $("#divFiltro").collapse("hide");

    // Obtenemos los valores de los campos del filtro
    var data1 = obtenerParametrosBusquedaBgr();

    // Guardamos parámetros para el reporte
    cargarReporteEnArre(16, data1, "Balance General", "", "");
    // Verificar si cambió la búsqueda o solo la página
    
    limpiarBgr();

    // Realizamos la petición al servidor
    PostGenHtml(data1, obtenerBgrUrl, function (obj) {
        // Mostramos el resultado en el div correspondiente
        $("#divBgr").html(obj);

        // SIMPLIFICADO: Mostramos resultados directamente
        $("#divDetalle").collapse("show");

       
        // Habilitamos botón de impresión
        $("#btnImprimir").prop("disabled", false);
        CerrarWaiting();
    }, function (obj) {
        // En caso de error, mostrar mensaje y restablecer el filtro
        ControlaMensajeError(obj.message);
        $("#divFiltro").collapse("show");
        CerrarWaiting();
    });
}

function analizaEstadoBtnDetalleBgr() {
    if ($("#divDetalle").is(":visible")) {
        // HAY QUE LIMPIAR 
        bgrDataBak = {};
        limpiarBgr();
    }
}

function configurarBotonesBgr() {
    // Botón de cancelar
    $("#btnCancel").on("click", function () {
        window.location.href = homeBgrUrl;
    });

    // Botón de búsqueda
    $("#btnBuscar").on("click", function () {
        // Validar campos obligatorios
        if (!validarCamposObligatoriosBgr()) {
            return;
        }

        // Es una nueva búsqueda, no resguardamos la búsqueda anterior
        bgrDataBak = {};

        // Es una búsqueda por filtro, siempre será página 1
        pagina = 1;

        // Realizar búsqueda
        buscarBgr();
    });

    // Usar mousedown en lugar de click para evitar conflictos con el collapse
    $("#btnDetalle").on("mousedown", analizaEstadoBtnDetalleBgr);
    $("#btnFiltro").on("mousedown", function () {
        if ($("#divFiltro").is(":hidden")) {
            $("#divDetalle").collapse("hide");
        }
    });

    // Botón de imprimir
    $(document).on("click", ".btnImprimir", function () {
        imprimirBgr();
    });    
}


function imprimirBgr() {
    // Obtener los parámetros base del formulario
    const params = obtenerParametrosBusquedaBgr();

    // Preparar datos para el gestor de impresión
    const data = arrRepoParams[16 - 1];

    // Invocar gestor documental
    invocacionGestorDoc(data);
}