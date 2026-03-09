let _pedidoLoading = false;
let orCompteSeleccionado = null;

$(function () {
    InicializaPantallaOrdenDeReparto();
    InicializaEventosPedido();
});

function InicializaPantallaOrdenDeReparto() {
    // INICIALIZAMOS PANELES
    if ($("#divDetalle").is(":visible")) {
        $("#divDetalle").collapse("hide");
    }
    $("#divFiltro").collapse("show");

    initPeriodoFechas();

    // Etiquetas de filtros
    $("#lbChkDesdeHasta").text("Periodo");
    $("#lbEstados").text("Estado"); // Estados
    $("#lbRepartidores").text("Repartidores"); // Repartidores

    $("#chkDesdeHasta").on("click", function () {
        if ($("#chkDesdeHasta").is(":checked")) {
            $("#Desde").prop("disabled", false);
            $("#Hasta").prop("disabled", false);
        } else {
            $("#Desde").prop("disabled", true);
            $("#Hasta").prop("disabled", true);
        }
    });

    $("#chkEstados").on("click", function () {
        if ($("#chkEstados").is(":checked")) {
            $("#listaEstados").prop("disabled", false);
            $("#EstadosList").prop("disabled", false);
            $("#listaEstados").trigger("focus");
        }
        else {
            $("#listaEstados").prop("disabled", true).val("");
            $("#EstadosList").prop("disabled", true).empty();
        }
    });

    $("#chkRepartidores").on("click", function () {
        if ($("#chkRepartidores").is(":checked")) {
            $("#listaRepartidores").prop("disabled", false);
            $("#RepartidoresList").prop("disabled", false);
            $("#listaRepartidores").trigger("focus");
        }
        else {
            $("#listaRepartidores").prop("disabled", true).val("");
            $("#RepartidoresList").prop("disabled", true).empty();
        }
    });

    $("#EstadosList").on("dblclick", 'option', function () { $(this).remove(); })
    $("#RepartidoresList").on("dblclick", 'option', function () { $(this).remove(); })

    $(document).on("change", "#listaEstados", ControlalistaEstadosSelected);
    $(document).on("change", "#listaRepartidores", ControlalistaRepartidoresSelected);
}

function initPeriodoFechas() {
    const hoy = new Date();
    const base = new Date(hoy.getFullYear(), hoy.getMonth(), hoy.getDate());
    const hasta = new Date(base);
    hasta.setDate(hasta.getDate() + 30);

    const format = (d) => {
        const y = d.getFullYear();
        const m = String(d.getMonth() + 1).padStart(2, '0');
        const day = String(d.getDate()).padStart(2, '0');
        return `${y}-${m}-${day}`;
    };

    $("#Desde").val(format(base));
    $("#Hasta").val(format(hasta));

    const enabled = $("#chkDesdeHasta").is(":checked");
    $("#Desde").prop("disabled", !enabled);
    $("#Hasta").prop("disabled", !enabled);
}

function ControlalistaEstadosSelected() {
    var item = $("#listaEstados").val();
    var desc = $("#listaEstados option:selected").text();
    if ($("#EstadosList").has('option:contains("' + item + '")').length === 0 && $("#EstadosList").has('option:contains("' + desc + '")').length === 0) {
        var opc = "<option value=" + item + ">" + desc + "</option>"
        $("#EstadosList").append(opc);
    }
}

function ControlalistaRepartidoresSelected() {
    var item = $("#listaRepartidores").val();
    var desc = $("#listaRepartidores option:selected").text();
    if ($("#RepartidoresList").has('option:contains("' + item + '")').length === 0 && $("#RepartidoresList").has('option:contains("' + desc + '")').length === 0) {
        var opc = "<option value=" + item + ">" + desc + "</option>"
        $("#RepartidoresList").append(opc);
    }
}

function InicializaEventosPedido() {
    $("#pagEstado").on("change", function () {
        var div = $("#divPaginacion");
        presentaPaginacion(div);
    });

    // Buscar
    $("#btnBuscar").on("click", function () {
        buscarOrdenesDeReparto(this);
    });
    funcCallBack = buscarOrdenesDeReparto;
}

async function buscarOrdenesDeReparto(btn, pag = 1) {
    if (_pedidoLoading) return;
    _pedidoLoading = true;

    const $btn = $(btn);
    const originalHtml = $btn.html();
    setBtnLoading($btn, true);

    try {
        const filtros = buildQueryFilters(pag);
        const url = buscarOrdenesDeRepartoUrl;
        const urlInitView = inicializarViewUrl;

        PostGenHtml({}, urlInitView, function (html) {
            $("#divDetalle").html(html).collapse("show");
            $("#divFiltro").collapse("hide");

			CargarOrdenesDeReparto(filtros, url);
        });

        
    } catch (e) {
        console.error("Error al buscar pedidos de clientes:", e);
        $("#divDetalle").html('<div class="alert alert-danger py-2 mb-0">No se pudo obtener la información.</div>').collapse("show");
    } finally {
        setBtnLoading($btn, false, originalHtml);
        _pedidoLoading = false;
    }
}

function CargarOrdenesDeReparto(filtros, url) {
    AbrirWaiting("Cargando ordenes de reparto...");
    PostGenHtml(filtros, url, function (html) {
        CerrarWaiting();
        $("#divListaOrdenesDeReparto").html(html);

        configurarEventosSeleccionListaOR();

        PostGen({}, buscarMetadataURL, function (obj) {
            if (obj.error === true) {
                AbrirMensaje("ATENCIÓN", obj.msg, function () {
                    $("#msjModal").modal("hide");
                    return true;
                }, false, ["Aceptar"], "error!", null);
            } else {
                totalRegs = obj.metadata.totalCount;
                pags = obj.metadata.totalPages;
                pagRegs = obj.metadata.pageSize;
                $("#pagEstado").val(true).trigger("change");
            }
        });
    });
}

$(document).on("click", "#btnAgregarOR, #btnModificarOR", function () {
    $("#vistaListaOR").addClass("d-none");
    $("#vistaEditarOR").removeClass("d-none");
});

$(document).on("click", "#btnConfirmarOR, #btnCancelarOR", function () {
    $("#vistaEditarOR").addClass("d-none");
    $("#vistaListaOR").removeClass("d-none");
});

$(document).on("click", "#btnConsolidar", function () {
    $("#vistaListaOR").addClass("d-none");
    $("#vistaConsolidarOR").removeClass("d-none");
});

$(document).on("click", "#btnConsolidarOR, #btnCancelarConsolidar, #btnReasignar", function () {
    $("#vistaConsolidarOR").addClass("d-none");
    $("#vistaListaOR").removeClass("d-none");
});


function configurarEventosSeleccionListaOR() {
}

function buildQueryFilters(pag) {
    const usaPeriodo = $("#chkDesdeHasta").is(":checked");
    const fechaD = usaPeriodo ? $("#Desde").val() : null;
    const fechaH = usaPeriodo ? $("#Hasta").val() : null;

    var rel01 = [];
    $("#EstadosList").children().each(function (i, item) { rel01.push($(item).val()) });

    var rel02 = [];
    $("#RepartidoresList").children().each(function (i, item) { rel02.push($(item).val()) });

    return {
        Registros: 200,
        Pagina: pag,
        FechaD: fechaD || null,
        FechaH: fechaH || null,
        Rel01: rel01.length ? rel01 : null,
        Rel02: rel02.length ? rel02 : null,
    };
}

function setBtnLoading($btn, loading, originalHtml) {
    if (!$btn || !$btn.length) return;
    if (loading) {
        $btn.prop("disabled", true).html('<span class="spinner-border spinner-border-sm me-1"></span>Buscando...');
    } else {
        $btn.prop("disabled", false).html(originalHtml ?? "Buscar");
    }
}

function configurarEventosSeleccionListaOR() {
    $(document).off("click", "#tbGridOrdenDeReparto tbody tr");
    $(document).on("click", "#tbGridOrdenDeReparto tbody tr", function (e) {
        if (!$(e.target).is("button, a, .btn, i")) {
            var $this = $(this);
            var fueSeleccionado = $this.hasClass("selected-row");

            $("#tbGridOrdenDeReparto tbody tr").removeClass("selected-row");

            if (!fueSeleccionado) {
                $this.addClass("selected-row");
                let orCompte = $this.data("or-compte");
                let oreId = $this.data("ore-id");
                orCompteSeleccionado = orCompte;
                if (orCompte) {
                    //Poder hacer algo, como por ejemplo, habilitar o no botones dependiendo del estado de la OR
                    CargarPedidosDelReparto(orCompte);
                    ConfigurarEstadoDeBotonesEnTabOrdenDeReparto(orCompte, oreId);
                }
            }
        }
    });
}

function CargarPedidosDelReparto(orCompte) {
    AbrirWaiting("Cargar pedidos de la orden de reparto...");
    const url = obtenerPedidosDeLaOrdenDeRepartoUrl;
    PostGenHtml({ orCompte: orCompte }, url, function (html) {
        $("#divListaPedidosDeCliente").html(html);
        CerrarWaiting();
        //Evaluar estados de los botones
    });
}

function ConfigurarEstadoDeBotonesEnTabOrdenDeReparto(orCompte, oreId) {
}