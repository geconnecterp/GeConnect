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


    // Mejorar el comportamiento de click en filas para incluir el checkbox
    $(document).on('click', '#tbAAJ tr', function (e) {
        // Evitar que se active si se hizo click directamente en el checkbox
        if (!$(e.target).is('.asiento-check') && !$(e.target).is('input[type="checkbox"]')) {
            // Encontrar el checkbox dentro de la fila y cambiar su estado
            var checkbox = $(this).find('.asiento-check');
            checkbox.prop('checked', !checkbox.prop('checked'));

            // Desmarcar todos los demás
            $('.asiento-check').not(checkbox).prop('checked', false);

            // Actualizar el estado y cargar detalles
            actualizarEstadoAjusta();
        }
    });

    // Modificar el manejador de click en checkboxes individuales para mostrar detalles
    $(document).on('click', '.asiento-check', function (e) {
        e.stopPropagation(); // Evitar que el click en el checkbox active la fila

        // Desmarcar todos los demás checkboxes si este se marcó
        if ($(this).prop('checked')) {
            $('.asiento-check').not(this).prop('checked', false);
        }

        // Actualizar el estado y cargar detalles
        actualizarEstadoAjusta();

        // Actualizar el estado del checkbox general
        $('#checkAllAAJ').prop('checked', $('.asiento-check:checked').length === $('.asiento-check').length);
    });

    // Modificar el manejador del checkbox general para reflejar cambios en detalle
    $(document).on('click', '#checkAllAAJ', function () {
        var isChecked = $(this).prop('checked');
        $('.asiento-check').prop('checked', isChecked);

        // Actualizar el estado y cargar detalles solo si hay algún checkbox marcado
        if (isChecked && $('.asiento-check').length > 0) {
            // Marcamos solo el primero para mostrar sus detalles
            $('.asiento-check').prop('checked', false);
            $('.asiento-check:first').prop('checked', true);
        }

        actualizarEstadoAjusta();
    });

    
});

// Función para actualizar el estado Ajusta y cargar los detalles
function actualizarEstadoAjusta() {
    // Obtener todos los checkboxes marcados
    var checkedBoxes = $('.asiento-check:checked');

    // Si hay al menos un checkbox marcado, buscar los detalles
    if (checkedBoxes.length > 0) {
        // Tomamos el primer checkbox marcado para mostrar sus detalles
        // (se puede modificar esta lógica si se requiere otro comportamiento)
        var selectedCcbId = $(checkedBoxes[0]).data('ccb-id');
        var ejercicio = $("#Eje_nro").val();

        // Limpiamos el div de detalle antes de cargar nuevos datos
        $("#divAajDet").empty();

        // Mostramos indicador de carga
        $("#divAajDet").html('<div class="text-center"><i class="bx bx-loader bx-spin text-primary" style="font-size: 2rem;"></i><p>Cargando detalles...</p></div>');

        // Invocamos al controlador para obtener los detalles
        var data = {
            eje_nro: ejercicio,
            ccb_id: selectedCcbId
        };

        // Realizar la petición AJAX
        PostGenHtml(data, buscarRegistrosAsAjCcbUrl, function (html) {
            // Actualizar el div con la respuesta
            $("#divAajDet").html(html);

            // Inicializar componentes que pudieran necesitar reinicialización
            $('.datepicker').datepicker({
                format: 'dd/mm/yyyy',
                autoclose: true,
                language: 'es',
                todayHighlight: true,
                orientation: 'bottom'
            });

        }, function (error) {
            // Manejo de errores
            $("#divAajDet").html('<div class="alert alert-danger">Error al cargar los detalles: ' + error.message + '</div>');
        });
    } else {
        // Si no hay checkboxes seleccionados, limpiar el área de detalle
        $("#divAajDet").empty();
        $("#divAajDet").html('<div class="alert alert-info">Seleccione una cuenta para ver sus detalles.</div>');
    }
}

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
    AbrirWaiting("Consultando los asientos de ajuste por inflación. Espere, puede tardar. Sea paciente...");

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
    $("#divAajDet").empty(); // Limpiar también el área de detalle
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

// Esta función se llama cuando se hace click en una fila del grid
function selectReg(elem, tableId) {
    // Si se hizo click en una fila de la tabla principal de asientos
    if (tableId === 'tbAAJ') {
        // Desmarcar todas las filas
        $("#" + tableId + " tr").removeClass("selected-row");

        // Marcar esta fila como seleccionada
        $(elem).addClass("selected-row");

        // Encontrar el checkbox en esta fila y marcarlo
        var checkbox = $(elem).find('.asiento-check');

        // Desmarcar todos los demás checkboxes
        $('.asiento-check').prop('checked', false);

        // Marcar este checkbox
        checkbox.prop('checked', true);

        // Actualizar el estado y cargar detalles
        actualizarEstadoAjusta();
    }
}

