let aajDataBak = {};

$(function () {
    inicializaComponentesAsientoAjuste();
    configurarBotonesAsientoAjuste();


    // Manejar el check/uncheck de todos los elementos
    $(document).on('click', '#checkAllAAJ', function () {
        var isChecked = $(this).prop('checked');
        $('.asiento-check').prop('checked', isChecked);
        actualizarEstadoAjusta();
    });

    // Manejar el check/uncheck individual
    $(document).on('click', '.asiento-check', function (e) {
        e.stopPropagation(); // Evitar que el click en el checkbox active la fila
        actualizarEstadoAjusta();

        // Verificar si todos están seleccionados para actualizar checkAllAAJ
        var totalChecks = $('.asiento-check').length;
        var totalChecked = $('.asiento-check:checked').length;
        $('#checkAllAAJ').prop('checked', totalChecks === totalChecked);
    });

    // Botón de confirmar ajustes
    $(document).on('click', '#btnConfirmarAjuste', function () {
        confirmarAjustes();
    });

    // Botón de cancelar ajustes
    $(document).on('click', '#btnCancelarAjuste', function () {
        cancelarAjustes();
    });
});


function inicializaComponentesAsientoAjuste() {
    // Inicializar selectores con Select2 si está disponible
    if ($.fn.select2) {
        $("#Eje_nro").select2({
            placeholder: "Seleccione ejercicio",
            width: '100%'
        });
    }

    // Configurar datepickers para fechas con localización explícita
    $.fn.datepicker.dates['es'] = {
        days: ["Domingo", "Lunes", "Martes", "Miércoles", "Jueves", "Viernes", "Sábado"],
        daysShort: ["Dom", "Lun", "Mar", "Mié", "Jue", "Vie", "Sáb"],
        daysMin: ["Do", "Lu", "Ma", "Mi", "Ju", "Vi", "Sa"],
        months: ["Enero", "Febrero", "Marzo", "Abril", "Mayo", "Junio", "Julio", "Agosto", "Septiembre", "Octubre", "Noviembre", "Diciembre"],
        monthsShort: ["Ene", "Feb", "Mar", "Abr", "May", "Jun", "Jul", "Ago", "Sep", "Oct", "Nov", "Dic"],
        today: "Hoy",
        clear: "Borrar",
        format: "dd/mm/yyyy",
        titleFormat: "MM yyyy",
        weekStart: 1
    };

    $('.datepicker').datepicker({
        format: 'dd/mm/yyyy',
        autoclose: true,
        language: 'es',
        todayHighlight: true,
        orientation: 'bottom'
    });

    // Forzar la localización regional para las fechas
    $.datepicker.setDefaults($.datepicker.regional["es"]);
}

function configurarBotonesAsientoAjuste() {
    // Botón de cancelar
    $("#btnCancel").on("click", function () {
        window.location.href = homeBSSUrl;
    });

    // Botón de búsqueda
    $("#btnBuscar").on("click", function () {
        
        // Es una nueva búsqueda, no resguardamos la búsqueda anterior
        mayorDataBak = {};

        // Es una búsqueda por filtro, siempre será página 1
        pagina = 1;

        // Realizar búsqueda
        buscaraaj();
    });

    // Usar mousedown en lugar de click para evitar conflictos con el collapse
    $("#btnDetalle").on("mousedown", analizaEstadoBtnDetalleaaj);
    $("#btnFiltro").on("mousedown", function () {
        if ($("#divFiltro").is(":hidden")) {
            $("#divDetalle").collapse("hide");
        }
    });

    // Botón de imprimir
    $(document).on("click", ".btnImprimir", function () {
        imprimiraaj();
    });
}

function analizaEstadoBtnDetalleaaj() {
    if ($("#divDetalle").is(":visible")) {
        // HAY QUE LIMPIAR 
        bssDataBak = {};
        limpiarAaj();
    }
}

function buscaraaj() {
    AbrirWaiting("Consultando los asientos de ajuste por inflación...");

    // Desactivamos los botones de acción
    $("#btnImprimir").prop("disabled", true);

    // SIMPLIFICADO: Ocultamos filtro inmediatamente
    $("#divFiltro").collapse("hide");

    // Obtenemos el ejercicio seleccionado
    var eje_nro = $("#Eje_nro").val();

    // Validar que se haya seleccionado un ejercicio
    if (!eje_nro) {
        CerrarWaiting();
        AbrirMensaje(
            "Validación",
            "Debe seleccionar un ejercicio contable.",
            function () {
                $("#msjModal").modal("hide");
            },
            false,
            ["Aceptar"],
            "warn!",
            null
        );
        return;
    }

    let data = { eje_nro };

    // Guardamos parámetros para el reporte
    cargarReporteEnArre(21, data, "Asientos de Ajuste por Inflación", "", administracion);

    limpiarAaj();

    // Realizamos la petición al servidor
    PostGenHtml(data, buscarAsientosAjusteUrl, function (obj) {
        // Mostramos el resultado en el div correspondiente
        $("#divAaj").html(obj);

        // Mostramos resultados directamente
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

// Función auxiliar para limpiar el área de resultados
function limpiarAaj() {
    $("#divAaj").empty();
    $("#btnFilter").collapse("show");
}

// Función para obtener los parámetros de búsqueda
function obtenerParametrosBusquedaaaj() {
    return {
        eje_nro: $("#Eje_nro").val(),
        // Otros parámetros de búsqueda que se requieran
    };
}

function imprimiraaj() {
    // Invocar gestor documental
    invocacionGestorDoc({});
}

// Manejar el check/uncheck de todos los elementos
$(document).on('click', '#checkAllAAJ', function () {
    var isChecked = $(this).prop('checked');
    $('.asiento-check').prop('checked', isChecked);
    actualizarEstadoAjusta();
});

// Manejar el check/uncheck individual
$(document).on('click', '.asiento-check', function (e) {
    e.stopPropagation(); // Evitar que el click en el checkbox active la fila
    actualizarEstadoAjusta();

    // Verificar si todos están seleccionados para actualizar checkAllAAJ
    var totalChecks = $('.asiento-check').length;
    var totalChecked = $('.asiento-check:checked').length;
    $('#checkAllAAJ').prop('checked', totalChecks === totalChecked);
});

// Botón de confirmar ajustes
$(document).on('click', '#btnConfirmarAjuste', function () {
    confirmarAjustes();
});

// Botón de cancelar ajustes
$(document).on('click', '#btnCancelarAjuste', function () {
    cancelarAjustes();
});