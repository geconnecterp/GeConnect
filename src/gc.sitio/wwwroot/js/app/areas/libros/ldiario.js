let ldDataBak = {};

$(function () {
    inicializarComponentesLd();

    configurarBotonesLd();

    configurarFiltrosLd();
    inicializarRangoUltimoMesLd();
    // Inicializar vista
    inicializarVistaLd();
});

function inicializarVistaLd() {
    // Deshabilitar botón de impresión hasta tener resultados
    $("#btnImprimir").prop("disabled", true);

    // MODIFICAR: Simplificar el manejo de paneles - mostrar filtro y ocultar resultados directamente
    $("#divFiltro").collapse("show");
    $("#divDetalle").collapse("hide");

    // MANTENER: Configuración de funciones de callback para paginación
    funcCallBack = buscarLD;

    // MANTENER: Evento de cambio para paginación
    $("#pagEstado").on("change", function () {
        const div = $("#divPaginacion");
        presentaPaginacion(div);
    });
}

function inicializarRangoUltimoMesLd() {
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
    $('#RangoFC').prop('checked', true);

    // Habilitar los campos de fecha
    $('input[name="Desde"], input[name="Hasta"]').prop('disabled', false);
    $('input[name="DesdeFC"], input[name="HastaFC"]').prop('disabled', false);

    // Formatear fechas para los inputs
    const desdeFormateado = formatoFecha(fechaDesde);
    const hastaFormateado = formatoFecha(fechaActual);

    // Asignar valores a los campos de fecha
    $('input[name="Desde"]').val(desdeFormateado);
    $('input[name="Hasta"]').val(hastaFormateado);
    $('input[name="DesdeFC"]').val(desdeFormateado);
    $('input[name="HastaFC"]').val(hastaFormateado);

    // Actualizar el datepicker para que muestre las fechas seleccionadas
    // Es importante hacer esto DESPUÉS de establecer los valores en los inputs
    $('input[name="Desde"]').datepicker('update', desdeFormateado);
    $('input[name="Hasta"]').datepicker('update', hastaFormateado);
    $('input[name="DesdeFC"]').datepicker('update', desdeFormateado);
    $('input[name="HastaFC"]').datepicker('update', hastaFormateado);
}

function configurarFiltrosLd() {
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

    // Evento para checkbox de Fecha de Carga
    $('#RangoFC').on('change', function () {
        const isChecked = $(this).prop('checked');
        $('input[name="DesdeFC"], input[name="HastaFC"]').prop('disabled', !isChecked);

        // Limpiar mensajes de error al desactivar el rango
        if (!isChecked) {
            $("#fechaError").remove();
        } else {
            // Validar fechas al activar el rango (si ambas tienen valor)
            validarRangoFechasC();
        }
    });

    // Eventos para validación de fechas
    $('input[name="Desde"], input[name="Hasta"]').on('change', function () {
        if ($("#Rango").is(":checked")) {
            validarRangoFechas();
        }
    });

    // Eventos para validación de fechas
    $('input[name="DesdeFC"], input[name="HastaFC"]').on('change', function () {
        if ($("#RangoFC").is(":checked")) {
            validarRangoFechasC();
        }
    });

    // Inicializar estados de los campos según checkboxes
    toggleComponent('Rango', 'input[name="Desde"]');
    toggleComponent('Rango', 'input[name="Hasta"]');

    // Inicializar estados de los campos según checkboxes para Fecha de Carga
    toggleComponent('RangoFC', 'input[name="DesdeFC"]');
    toggleComponent('RangoFC', 'input[name="HastaFC"]');

}

function inicializarComponentesLd() {
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
    $.datepicker.setDefaults($.datepicker.regional["es"]);
}

function validarCamposObligatoriosLd() {
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
                mensajesError.push("Fecha Asiento: La fecha Desde no puede ser mayor a la fecha Hasta");
            }
        }
    }

    // Validar rango de fechas si está habilitado
    if ($("#RangoFC").is(":checked")) {
        const fechaDesdeFC = $("input[name='DesdeFC']").val();
        const fechaHastaFC = $("input[name='HastaFC']").val();

        if (!fechaDesdeFC) {
            camposFaltantes.push("Fecha desde");
        }

        if (!fechaHastaFC) {
            camposFaltantes.push("Fecha hasta");
        }

        // Si ambas fechas están presentes, validar que Desde no sea mayor a Hasta
        if (fechaDesdeFC && fechaHastaFC) {
            // Convertir las fechas a objetos Date para comparación
            const desdeFC = parseFechaES(fechaDesdeFC);
            const hastaFC = parseFechaES(fechaHastaFC);

            if (desdeFC && hastaFC && desdeFC > hastaFC) {
                mensajesError.push("Fechas Carga: La fecha Desde no puede ser mayor a la fecha Hasta");
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

function obtenerParametrosBusquedaLD() {
    return {
        eje_nro: $("#Eje_nro").val(),
        conTemporales: $("#chkIncluirTemp").is(":checked"),
        rango: $("#Rango").is(":checked"),
        desde: $("#Rango").is(":checked") ? $("input[name='Desde']").val() : null,
        hasta: $("#Rango").is(":checked") ? $("input[name='Hasta']").val() : null,
        rangoFC: $("#RangoFC").is(":checked"),
        desdeFC: $("#RangoFC").is(":checked") ? $("input[name='DesdeFC']").val() : null,
        hastaFC: $("#RangoFC").is(":checked") ? $("input[name='HastaFC']").val() : null,
    };
}

function limpiarLD() {
    // Vaciar el panel que contiene el asiento
    $("#divLD").empty();
    $("#divDiarioResumen").empty().html('<span class="text - danger">SIN REGISTROS</span>')

    // Restablecer variables de control
    filaClicDoble = null;
    EntidadSelect = "";
    EntidadEstado = "";

    inicializarVistaLd();
}

function buscarLD(pag = 1) {
    AbrirWaiting("Consultando el Libro Diario...");

    // Desactivamos los botones de acción
    $("#btnImprimir").prop("disabled", true);

    // SIMPLIFICADO: Ocultamos filtro inmediatamente
    $("#divFiltro").collapse("hide");

    // Obtenemos los valores de los campos del filtro
    var data1 = obtenerParametrosBusquedaLD();

    // Guardamos parámetros para el reporte
    cargarReporteEnArre(13, data1, "Libro Diario", "", "");
    // Verificar si cambió la búsqueda o solo la página
    var buscaNew = JSON.stringify(ldDataBak) !== JSON.stringify(data1);

    if (buscaNew === false) {
        // Son iguales las condiciones, solo cambia de página
        pagina = pag;
    } else {
        // Cambiaron las condiciones, vuelve a página 1
        ldDataBak = JSON.parse(JSON.stringify(data1));
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

    limpiarLD();

    // Realizamos la petición al servidor
    PostGenHtml(data, obtenerLDUrl, function (obj) {
        // Mostramos el resultado en el div correspondiente
        $("#divLD").html(obj);

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

function analizaEstadoBtnDetalleLd() {
    if ($("#divDetalle").is(":visible")) {
        // HAY QUE LIMPIAR 
        ldDataBak = {};
        limpiarLD();
    }
}

function configurarBotonesLd() {
    // Botón de cancelar
    $("#btnCancel").on("click", function () {
        window.location.href = homeLDUrl;
    });

    // Botón de búsqueda
    $("#btnBuscar").on("click", function () {
        // Validar campos obligatorios
        if (!validarCamposObligatoriosLd()) {
            return;
        }

        // Es una nueva búsqueda, no resguardamos la búsqueda anterior
        ldDataBak = {};

        // Es una búsqueda por filtro, siempre será página 1
        pagina = 1;

        // Realizar búsqueda
        buscarLD();
    });

    // Usar mousedown en lugar de click para evitar conflictos con el collapse
    $("#btnDetalle").on("mousedown", analizaEstadoBtnDetalleLd);
    $("#btnFiltro").on("mousedown", function () {
        if ($("#divFiltro").is(":hidden")) {
            $("#divDetalle").collapse("hide");
        }
    });

    // Botón de imprimir
    $(document).on("click", ".btnImprimir", function () {
        imprimirLD();
    });

    // Manejar el clic en la pestaña Diario
    $("#tabDiarioResumen").on("click", function () {
        // Verificar si hay parámetros guardados para el informe
        if ($("#divDiarioResumen").text().includes("SIN REGISTROS") && Object.keys(arrRepoParams[13 - 1]).length > 0) {
            if (arrRepoParams[12] && arrRepoParams[12].parametros) {   
                let parametrs = arrRepoParams[12].parametros;
                cargarReporteEnArre(15, parametrs, "Libro Diario Resumen", "", administracion);

                cargarResumenLibroDiario(arrRepoParams[12].parametros);
            } else {
                // Si no hay parámetros guardados, mostrar mensaje
                AbrirMensaje("Atención", "No hay parámetros definidos para generar el informe. Por favor, configure los filtros primero.",
                    function () { $("#msjModal").modal("hide"); },
                    false, ["Aceptar"], "warn!", null);
            }
        }
    });
}


// Función para cargar el resumen del libro diario
function cargarResumenLibroDiario(parametros) {
    AbrirWaiting("Cargando resumen del Libro Diario...");

    PostGenHtml(parametros, obtenerLDUResumenrl+"?pagina="+pagina, function (response) {
        // Insertar la respuesta en el contenedor adecuado
        $("#divDiarioResumen").html(response);

        // Si hay un elemento de paginación, configurarlo
        if ($("#divPaginacion1").length) {
            totalRegs = parametros.totalRegistros || 0;
            pagRegs = parametros.regs || 10;
            pagina = parametros.pagina || 1;
            funcCallBack = function (num) {
                parametros.pagina = num;
                cargarResumenLibroDiario(parametros);
            };
            presentaPaginacion($("#divPaginacion1"));
        }

        CerrarWaiting();
    }, function (error) {
        CerrarWaiting();
        AbrirMensaje("Error", "Ocurrió un error al cargar el resumen del Libro Diario. Por favor, intente nuevamente.",
            function () { $("#msjModal").modal("hide"); },
            false, ["Aceptar"], "error!", null);
    });
}


function imprimirLD() {
    // Obtener los parámetros base del formulario
    const params = obtenerParametrosBusquedaLD();

    // Preparar datos para el gestor de impresión
    const data = {
        modulo: "LDiario",
        parametros: {
            eje_nro: params.eje_nro,
            desde: params.desde,
            hasta: params.hasta,
            incluirTemporales: params.incluirTemporales
        },
        titulo: "Libro Diario",
        subTitulo: `Ejercicio: ${$("#Eje_nro option:selected").text()}`,
        observacion: ""
    };

    // Invocar gestor documental
    invocacionGestorDoc(data);
}