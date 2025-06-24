// Variables de control global
let saldoAnterior = 0;
let saldoActual = 0;
let bssDataBak = {};

$(function () {
    inicializarComponentesBss();

    configurarBotonesBss();

    configurarFiltrosBss();
    inicializarRangoUltimoMes();
    // Inicializar vista
    inicializarVistaBss();
});

function configurarFiltrosBss() {
    // Forzar selección y deshabilitación del checkbox de ejercicio
    $('#chkEjercicio').prop('checked', true).prop('disabled', true);
    $('#Eje_nro').prop('disabled', false);

    // Prevenir clics en checkboxes que deben estar siempre habilitados
    $('#chkEjercicio').on('click', function (e) {
        e.preventDefault();
        return false;
    });

    // Evento para checkbox de incluir asientos temporales
    $('#chkIncluirTemp').on('change', function () {
        const isChecked = $(this).prop('checked');
        $('#incluirTemporales').val(isChecked ? "true" : "false");
    });

    // Evento para checkbox de rango de fechas
    $('#Rango').on('change', function () {
        const isChecked = $(this).prop('checked');
        $('input[name="Desde"], input[name="Hasta"]').prop('disabled', !isChecked);

        // Limpiar mensajes de error al desactivar el rango
        if (!isChecked) {
            $("#fechaError").remove();
        } else {
            // Validar fechas al activar el rango (si ambas tienen valor)
            validarRangoFechas();
        }
    });

    // Eventos para validación de fechas
    $('input[name="Desde"], input[name="Hasta"]').on('change', function () {
        if ($("#Rango").is(":checked")) {
            validarRangoFechas();
        }
    });

    // Inicializar estados de los campos según checkboxes
    toggleComponent('Rango', 'input[name="Desde"]');
    toggleComponent('Rango', 'input[name="Hasta"]');
}

function inicializarComponentesBss() {
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

    //// Inicializar el rango de fechas al último mes
    //// Esta función debe ejecutarse DESPUÉS de inicializar los datepickers
    //setTimeout(inicializarRangoUltimoMes, 100);

    //// Asegurarse que la validación de fechas se ejecute después de inicializar
    //setTimeout(function () {
    //    if ($("#Rango").is(":checked")) {
    //        validarRangoFechas();
    //    }
    //}, 200);

    // Forzar la localización regional para las fechas
    $.datepicker.setDefaults($.datepicker.regional["es"]);

   
}

function configurarBotonesBss() {
    // Botón de cancelar
    $("#btnCancel").on("click", function () {
        window.location.href = homeBSSUrl;
    });

    // Botón de búsqueda
    $("#btnBuscar").on("click", function () {
        // Validar campos obligatorios
        if (!validarCamposObligatorios()) {
            return;
        }

        // Es una nueva búsqueda, no resguardamos la búsqueda anterior
        mayorDataBak = {};   

        // Es una búsqueda por filtro, siempre será página 1
        pagina = 1;

        // Realizar búsqueda
        buscarBSS();
    });

    // Usar mousedown en lugar de click para evitar conflictos con el collapse
    $("#btnDetalle").on("mousedown", analizaEstadoBtnDetalleBss);
    $("#btnFiltro").on("mousedown", function () {
        if ($("#divFiltro").is(":hidden")) {
            $("#divDetalle").collapse("hide");
        }
    });

    // Botón de imprimir
    $(document).on("click", ".btnImprimir", function () {
        imprimirBSS();
    });
}

function analizaEstadoBtnDetalleBss() {
    if ($("#divDetalle").is(":visible")) {
        // HAY QUE LIMPIAR 
        bssDataBak = {};
        limpiarBss();     
    }
}

function limpiarBss() {
    // Vaciar el panel que contiene el asiento
    $("#divBss").empty();

    // Restablecer variables de control
    filaClicDoble = null;
    EntidadSelect = "";
    EntidadEstado = "";

    inicializarVistaBss();
}

function inicializarVistaBss() {
    // Deshabilitar botón de impresión hasta tener resultados
    $("#btnImprimir").prop("disabled", true);

    // MODIFICAR: Simplificar el manejo de paneles - mostrar filtro y ocultar resultados directamente
    $("#divFiltro").collapse("show");
    $("#divDetalle").collapse("hide");

    // MANTENER: Configuración de funciones de callback para paginación
    funcCallBack = buscarBSS;

    // MANTENER: Evento de cambio para paginación
    $("#pagEstado").on("change", function () {
        const div = $("#divPaginacion");
        presentaPaginacion(div);
    });
}

function buscarBSS(pag = 1) {
    AbrirWaiting("Consultando el Balance de Sumas y Saldos...");

    // Desactivamos los botones de acción
    $("#btnImprimir").prop("disabled", true);

    // SIMPLIFICADO: Ocultamos filtro inmediatamente
    $("#divFiltro").collapse("hide");

    // Obtenemos los valores de los campos del filtro
    var data1 = obtenerParametrosBusquedaBss();

    // Guardamos parámetros para el reporte
    cargarReporteEnArre(14, data1, "Balance de Sumas y Saldos", "", administracion);
    // Verificar si cambió la búsqueda o solo la página
    var buscaNew = JSON.stringify(bssDataBak) !== JSON.stringify(data1);

    if (buscaNew === false) {
        // Son iguales las condiciones, solo cambia de página
        pagina = pag;
    } else {
        // Cambiaron las condiciones, vuelve a página 1
        bssDataBak = JSON.parse(JSON.stringify(data1));
        pagina = 1;
        pag = 1;
    }

    // Agregamos parámetros adicionales
    var data2 = {
        pagina: pagina,
        buscaNew: buscaNew
    };

    // Combinamos todos los parámetros
    var data = $.extend({}, data1, data2);

    limpiarBss();

    // Realizamos la petición al servidor
    PostGenHtml(data, obtenerBSSUrl, function (obj) {
        // Mostramos el resultado en el div correspondiente
        $("#divBss").html(obj);

        // SIMPLIFICADO: Mostramos resultados directamente
        $("#divDetalle").collapse("show");

        // Obtenemos metadata adicional (paginación, saldos)
        PostGen(data, buscarMetadataURL, function (obj) {
            if (obj.error === true) {
                ControlaMensajeError(obj.msg);
            } else {
                //// Guardamos valores para reportes
                //saldoAnterior = obj.metadata.saldoAnterior || 0;
                //saldoActual = obj.metadata.saldoActual || 0;

                // Configuramos paginación
                totalRegs = obj.metadata.totalCount;
                pags = obj.metadata.totalPages;
                pagRegs = obj.metadata.pageSize;

                // Activamos paginación
                $("#pagEstado").val(true).trigger("change");

                // Habilitamos botón de impresión
                $("#btnImprimir").prop("disabled", false);
            }
        });

        CerrarWaiting();
    }, function (obj) {
        // En caso de error, mostrar mensaje y restablecer el filtro
        ControlaMensajeError(obj.message);
        $("#divFiltro").collapse("show");
        CerrarWaiting();
    });
}


/**
 * Obtiene los parámetros para la búsqueda
 * @returns {Object} Objeto con los parámetros para la búsqueda
 */
function obtenerParametrosBusquedaBss() {
    return {
        eje_nro: $("#Eje_nro").val(),
        incluirTemporales: $("#chkIncluirTemp").is(":checked"),
        rango: $("#Rango").is(":checked"),
        desde: $("#Rango").is(":checked") ? $("input[name='Desde']").val() : null,
        hasta: $("#Rango").is(":checked") ? $("input[name='Hasta']").val() : null
    };
}

function imprimirBSS() {
    // Obtener los parámetros base del formulario
    const params = obtenerParametrosBusquedaBss();

    // Preparar datos para el gestor de impresión
    const data = {
        modulo: "BalanceSS",
        parametros: {
            eje_nro: params.eje_nro,
            desde: params.desde,
            hasta: params.hasta,
            incluirTemporales: params.incluirTemporales
        },
        titulo: "Balance de Sumas y Saldos",
        subTitulo: `Ejercicio: ${$("#Eje_nro option:selected").text()}`,
        observacion: ""
    };

    // Invocar gestor documental
    invocacionGestorDoc(data);
}

// Función para inicializar el rango de fechas al último mes
function inicializarRangoUltimoMes() {
    // Obtener fecha actual
    const fechaActual = new Date();

    // Calcular fecha de hace 30 días
    const fechaDesde = new Date();
    fechaDesde.setDate(fechaActual.getDate() - 30);

    // Formatear fechas en formato dd/mm/yyyy (formato español)
    const formatoFecha = (fecha) => {
        const dia = ('0' + fecha.getDate()).slice(-2);
        const mes = ('0' + (fecha.getMonth() + 1)).slice(-2);
        const anio = fecha.getFullYear();
        return `${dia}/${mes}/${anio}`;
    };

    // Marcar el checkbox de rango
    $('#Rango').prop('checked', true);

    // Habilitar los campos de fecha
    $('input[name="Desde"], input[name="Hasta"]').prop('disabled', false);

    // Formatear fechas para los inputs
    const desdeFormateado = formatoFecha(fechaDesde);
    const hastaFormateado = formatoFecha(fechaActual);

    // Asignar valores a los campos de fecha
    $('input[name="Desde"]').val(desdeFormateado);
    $('input[name="Hasta"]').val(hastaFormateado);

    // Actualizar el datepicker para que muestre las fechas seleccionadas
    // Es importante hacer esto DESPUÉS de establecer los valores en los inputs
    $('input[name="Desde"]').datepicker('update', desdeFormateado);
    $('input[name="Hasta"]').datepicker('update', hastaFormateado);
}

/**
* Valida los campos obligatorios antes de realizar una búsqueda
* @returns {boolean} True si todos los campos obligatorios están completos
*/
function validarCamposObligatorios() {
    let camposFaltantes = [];
    let mensajesError = [];

    // Validar ejercicio (siempre obligatorio)
    if (!$("#Eje_nro").val()) {
        camposFaltantes.push("Ejercicio contable");
    }

    // Validar rango de fechas si está habilitado
    if ($("#Rango").is(":checked")) {
        const fechaDesde = $("input[name='Desde']").val();
        const fechaHasta = $("input[name='Hasta']").val();

        if (!fechaDesde) {
            camposFaltantes.push("Fecha desde");
        }

        if (!fechaHasta) {
            camposFaltantes.push("Fecha hasta");
        }

        // Si ambas fechas están presentes, validar que Desde no sea mayor a Hasta
        if (fechaDesde && fechaHasta) {
            // Convertir las fechas a objetos Date para comparación
            const desde = parseFechaES(fechaDesde);
            const hasta = parseFechaES(fechaHasta);

            if (desde && hasta && desde > hasta) {
                mensajesError.push("La fecha Desde no puede ser mayor a la fecha Hasta");
            }
        }
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