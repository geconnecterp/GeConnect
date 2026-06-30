$(function () {
    InicializaPantalla();
    InicializaEventos();
});

function InicializaEventos() {
    $(document).off("click", "#btnImprimir");
    $(document).on("click", "#btnImprimir", function () {
        ImprimirDetalle();
    });

    $("#btnImprimir").hide();

    $("#btnBuscar").on("click", function () {
        if (!ValidarRangoFechas()) return;
        BuscarMovimientosDeCtaDta();
    });
}

function ImprimirDetalle() {
    ReseteoDeReportes();
    setTimeout(() => {
        const filtros = buildQueryFilters();
        let data = { ctag_list: filtros.ctag_list.join(","), desde: filtros.desde, hasta: filtros.hasta };
        cargarReporteEnArre(85, data, "Movimiento de Cuentas Directas", "", "");
        invocacionGestorDoc({});
    }, 500);
}

function ReseteoDeReportes() {
    console.log("Reseto de reportes");
    ReporteResetArre();
}

function ValidarRangoFechas() {
    const desdeStr = $("#Desde").val();
    const hastaStr = $("#Hasta").val();

    // Validar existencia
    if (!desdeStr || !hastaStr) {
        AbrirMensaje("ATENCIÓN", "Debe seleccionar ambas fechas.", function () {
            $("#msjModal").modal("hide");
            return true;
        }, false, ["Aceptar"], "error!", null);
        return false;
    }

    const desde = new Date(desdeStr);
    const hasta = new Date(hastaStr);

    // Validar fechas inválidas
    if (isNaN(desde.getTime()) || isNaN(hasta.getTime())) {
        AbrirMensaje("ATENCIÓN", "Alguna de las fechas no es válida.", function () {
            $("#msjModal").modal("hide");
            return true;
        }, false, ["Aceptar"], "error!", null);
        return false;
    }

    // Regla 1: Desde > Hasta
    if (desde > hasta) {
        AbrirMensaje("ATENCIÓN", "La fecha Desde no puede ser mayor que la fecha Hasta.", function () {
            $("#msjModal").modal("hide");
            return true;
        }, false, ["Aceptar"], "error!", null);
        return false;
    }

    // Regla 2: Diferencia mayor a 1 año
    const diffMs = hasta - desde;
    const diffDias = diffMs / (1000 * 60 * 60 * 24);

    if (diffDias > 365) {
        AbrirMensaje("ATENCIÓN", "El rango de fechas no puede superar 1 año.", function () {
            $("#msjModal").modal("hide");
            return true;
        }, false, ["Aceptar"], "error!", null);
        return false;
    }

    return true;
}

function BuscarMovimientosDeCtaDta() {
    try {
        AbrirWaiting("Buscando Movimientos de Cuentas Directas...")
        const filtros = buildQueryFilters();
        const url = buscarMovimientoDeCtaDtaURL;

        PostGenHtml(filtros, url, function (html) {
            $("#divDetalle").html(html).collapse("show");
            $("#divFiltros").collapse("hide");
            EvaluarBotonImprimir();
            CerrarWaiting();
        });
    } catch (e) {
        console.error("Error al buscar movimientos de cuentas directas:", e);
        $("#divDetalle").html('<div class="alert alert-danger py-2 mb-0">No se pudo obtener la información.</div>').collapse("show");
    } finally {
    }
}

function EvaluarBotonImprimir() {
    console.log("Evaluando botón imprimir");

    const tablaSelector = "#tbMovimientos";
    if (!tablaSelector) {
        $("#btnImprimir").hide();
        return;
    }

    const $tabla = $(tablaSelector);

    if ($tabla.length === 0) {
        $("#btnImprimir").hide();
        return;
    }

    // Buscar filas reales (NO fila-vacia)
    const filasReales = $tabla.find("tbody tr").not(".fila-vacia");

    if (filasReales.length === 0) {
        // No hay datos reales → ocultar
        console.log("No hay filas reales, ocultando botón imprimir");
        $("#btnImprimir").hide();
        return;
    }

    // Si tiene datos reales → mostrar botón
    $("#btnImprimir").show();

    // Guardamos el tab actual para imprimir
    $("#btnImprimir").data("tab-activo", tablaSelector);
}

function buildQueryFilters() {
    const fechaD = $("#Desde").val();
    const fechaH = $("#Hasta").val();

    var rel01 = [];
    $("#Rel01List").children().each(function (i, item) { rel01.push($(item).val()) });
    if (rel01.length == 0)
        rel01.push("%");

    return {
        desde: fechaD || null,
        hasta: fechaH || null,
        ctag_list: rel01.length ? rel01 : null,
    };
}

function imprimirDetalle() { }

function InicializaPantalla() {
    // INICIALIZAMOS PANELES
    if ($("#divDetalle").is(":visible")) {
        $("#divDetalle").collapse("hide");
    }
    $("#divFiltros").collapse("show");

    // Etiquetas de filtros
    $("#lbChkDesdeHasta").text("Periodo");
    $("#lbRel01").text("Cuenta Gastos"); // Rel01

    $("#chkDesdeHasta")
        .prop("checked", true)
        .prop("disabled", true);

    $("#Desde").prop("disabled", false);
    $("#Hasta").prop("disabled", false);

    $("#Rel01List").on("dblclick", 'option', function () { $(this).remove(); })
    $("#Rel01").on("click", function () { $(this).val(""); });
}

$("#Rel01").autocomplete({
    source: function (request, response) {

        data = { prefix: request.term }; /*Rel01*/

        $.ajax({
            url: autoComRel011Url,
            type: "POST",
            dataType: "json",
            data: data,
            success: function (obj) {
                response($.map(obj, function (item) {
                    var texto = item.descripcion;
                    return { label: texto, value: item.descripcion, id: item.id, prov: item.provId };
                }));
            }
        })
    },
    minLength: 3,
    select: function (event, ui) {
        if ($("#Rel01List").has('option:contains("' + ui.item.id + '")').length === 0) {
            $("#Rel01Item").val(ui.item.id);
            var opc = "<option value=" + ui.item.id + ">" + ui.item.value + "</option>"
            $("#Rel01List").append(opc);
        }
        return true;
    }
});