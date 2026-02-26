var modoNuevoPedido = false;
var modoModificacionPedido = false;

var campoEnEdicionPedido = null;
let procesandoCampo = false;

// Variable para guardar estado original del pedido
let _pedidoOriginal = null;

const fmtCurrency = (v) =>
    new Intl.NumberFormat('es-AR', { style: 'currency', currency: 'ARS', maximumFractionDigits: 2 }).format(v ?? 0);

const fmtPercent = (v) => {
    // v puede venir como 0.354 o 35.4 -> normalizamos a fracción
    const frac = (Math.abs(v) > 1) ? (v / 100) : v;
    return new Intl.NumberFormat('es-AR', { style: 'percent', minimumFractionDigits: 2, maximumFractionDigits: 2 }).format(frac ?? 0);
};

$(function () {
    InicializaPantallaPedido();
    InicializaEventosPedido();
});

function InicializaPantallaPedido() {
    // INICIALIZAMOS PANELES
    if ($("#divDetalle").is(":visible")) {
        $("#divDetalle").collapse("hide");
    }
    $("#divFiltro").collapse("show");
    // ✅ Activar botón de nuevo presupuesto
    $("#btnAbmNuevo").prop("disabled", false);

    // Configurar el evento click para el botón Cancelar/Inicializar
    $("#btnAbmCancelar").on("click", function (e) {
        cancelarOperacion(e);
    });

    $("#btnCancel").on("click", function () {
        window.location.href = homePresup;
    });

    $("#btnAbmAceptar, #btnAbmCancelar").prop("disabled", true).hide();

    // Inicializa el período de fechas (hoy / hoy + 30 días)
    initPeriodoFechas();

    // Etiquetas de filtros
    $("#lbChkDesdeHasta").text("Periodo");
    $("#lbRel01").text("Cliente"); // Rel01
    $("#lbEstados").text("Estado"); // Estados
    $("#lbVendedores").text("Vendedores"); // Vendedores"
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

    $("#chkVendedores").on("click", function () {
        if ($("#chkVendedores").is(":checked")) {
            $("#listaVendedores").prop("disabled", false);
            $("#VendedoresList").prop("disabled", false);
            $("#listaVendedores").trigger("focus");
        }
        else {
            $("#listaVendedores").prop("disabled", true).val("");
            $("#VendedoresList").prop("disabled", true).empty();
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
    $("#VendedoresList").on("dblclick", 'option', function () { $(this).remove(); })
    $("#RepartidoresList").on("dblclick", 'option', function () { $(this).remove(); })
    $("#Rel01List").on("dblclick", 'option', function () { $(this).remove(); })

    $("#Rel01").on("click", function () { $(this).val(""); });

    $(document).on("change", "#listaEstados", ControlalistaEstadosSelected);
    $(document).on("change", "#listaVendedores", ControlalistaVendedoresSelected);
    $(document).on("change", "#listaRepartidores", ControlalistaRepartidoresSelected);
}

function ControlalistaEstadosSelected() {
    var item = $("#listaEstados").val();
    var desc = $("#listaEstados option:selected").text();
    if ($("#EstadosList").has('option:contains("' + item + '")').length === 0 && $("#EstadosList").has('option:contains("' + desc + '")').length === 0) {
        var opc = "<option value=" + item + ">" + desc + "</option>"
        $("#EstadosList").append(opc);
    }
}

function ControlalistaVendedoresSelected() {
    var item = $("#listaVendedores").val();
    var desc = $("#listaVendedores option:selected").text();
    if ($("#VendedoresList").has('option:contains("' + item + '")').length === 0 && $("#VendedoresList").has('option:contains("' + desc + '")').length === 0) {
        var opc = "<option value=" + item + ">" + desc + "</option>"
        $("#VendedoresList").append(opc);
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

$("#Rel01").autocomplete({
    source: function (request, response) {

        data = { prefix: request.term }; /*Rel01*/

        $.ajax({
            url: autoComRel01Url,
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
        //ctaIdSelected = ui.item.id;
        //ctaDescSelected = ui.item.value;
        if ($("#Rel01List").has('option:contains("' + ui.item.id + '")').length === 0) {
            $("#Rel01Item").val(ui.item.id);
            var opc = "<option value=" + ui.item.id + ">" + ui.item.value + "</option>"
            $("#Rel01List").append(opc);
        }
        return true;
    }
});

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

function InicializaEventosPedido() {

    // Buscar
    $("#btnBuscar").on("click", function () {
        buscarPedidosDeCliente(this);
    });
    funcCallBack = buscarPedidosDeCliente;

}

function cancelarOperacion(e) {
    console.log('🔄 Cancelando operación de presupuesto...');

    // ✅ PASO 1: Resetear modos de edición
    modoNuevoPedido = false;
    modoModificacionPedido = false;
    campoEnEdicionPedido = null;
    _pedidoOriginal = null;

    if ($("#divDetalle").is(":visible") && $("#divFiltro").is(":not(:visible)")) {
        $("#divFiltro").collapse("show");
        $("#divDetalle").collapse("hide");
    }

    // ✅ PASO 2: Vaciar y ocultar divs de datos y productos
    $("#divPresDatos, #divPresProds").empty().hide();

    // ✅ PASO 3: Determinar si hay fila seleccionada en el grid de búsqueda
    const $filaSeleccionada = $("#tbGridPedido tbody tr.selected-row");
    const hayPedidoSeleccionado = $filaSeleccionada.length > 0;

    // ✅ PASO 4: Restaurar botones ABM según contexto
    if (hayPedidoSeleccionado) {
        // Si hay un presupuesto seleccionado, mantener habilitados Modificar y Eliminar
        const pceId = $filaSeleccionada.data('pce-id') || 'P';
        const estadosEditables = ['P'];
        const permite = estadosEditables.includes(pceId);

        $("#btnAbmModif").prop("disabled", !permite);
        $("#btnAbmElimi").prop("disabled", !permite);
        $("#btnAbmNuevo").prop("disabled", false);
        $("#btnImprimir").prop("disabled", false);

    } else {
        // Si no hay selección, solo habilitar Nuevo
        $("#btnAbmNuevo").prop("disabled", false);
        $("#btnAbmModif, #btnAbmElimi, #btnImprimir").prop("disabled", true);
        let data = {};
        cargarReporteEnArre(62, data, "Pedido de Cliente");
    }

    // ✅ PASO 5: Desactivar y ocultar botones de confirmación
    $("#btnAbmAceptar, #btnAbmCancelar").prop("disabled", true).hide();

    // ✅ PASO 6: Deshabilitar botón de agregar productos
    //$("#btnAgregarCProducto").prop("disabled", true);

    // ✅ PASO 7: Limpiar clases de edición en el grid (mantener selección)
    $("#tbGridPedido tbody tr").removeClass("selectedEdit-row").removeClass("selected-row");

    console.log('✅ Operación cancelada - Vista reinicializada');

    $("#divPedido").removeClass("table-wrapper-100").addClass("table-wrapper-300");

    //// ✅ PASO 8: Redirección si es necesario
    //if (e && $(e.target).is("#btnAbmCancelar") && typeof homePresup !== 'undefined') {
    //    console.log('🔀 Redirigiendo a:', homePresup);
    //    window.location.href = homePresup;
    //}
}

let _pedidoLoading = false;

async function buscarPedidosDeCliente(btn, pag = 1) {
    if (_pedidoLoading) return;
    _pedidoLoading = true;

    const $btn = $(btn);
    const originalHtml = $btn.html();
    setBtnLoading($btn, true);

    try {
        const filtros = buildQueryFilters(pag);
        const url = buscarPedidosUrl;

        PostGenHtml(filtros, url, function (html) {
            $("#divDetalle").html(html).collapse("show");
            $("#divFiltro").collapse("hide");

            //configurarEventosSeleccionPres();
            $("#btnAbmAceptar").prop("disabled", true).show();
            $("#btnAbmCancelar").prop("disabled", false).show();

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
    } catch (e) {
        console.error("Error al buscar pedidos de clientes:", e);
        $("#divDetalle").html('<div class="alert alert-danger py-2 mb-0">No se pudo obtener la información.</div>').collapse("show");
    } finally {
        setBtnLoading($btn, false, originalHtml);
        _pedidoLoading = false;
    }
}

function setBtnLoading($btn, loading, originalHtml) {
    if (!$btn || !$btn.length) return;
    if (loading) {
        $btn.prop("disabled", true).html('<span class="spinner-border spinner-border-sm me-1"></span>Buscando...');
    } else {
        $btn.prop("disabled", false).html(originalHtml ?? "Buscar");
    }
}

function buildQueryFilters(pag) {
    const usaPeriodo = $("#chkDesdeHasta").is(":checked");
    const fechaD = usaPeriodo ? $("#Desde").val() : null;
    const fechaH = usaPeriodo ? $("#Hasta").val() : null;

    var rel01 = [];
    $("#Rel01List").children().each(function (i, item) { rel01.push($(item).val()) });
    
    var rel02 = [];
    $("#EstadosList").children().each(function (i, item) { rel02.push($(item).val()) });

    var rel03 = [];
    $("#VendedoresList").children().each(function (i, item) { rel03.push($(item).val()) });

    var rel04 = [];
    $("#RepartidoresList").children().each(function (i, item) { rel04.push($(item).val()) });

    return {
        Registros: 200,
        Pagina: pag,
        FechaD: fechaD || null,
        FechaH: fechaH || null,
        Rel01: rel01.length ? rel01 : null,
        Rel02: rel02.length ? rel02 : null,
        Rel03: rel03.length ? rel03 : null,
        Rel04: rel04.length ? rel03 : null,
    };
}