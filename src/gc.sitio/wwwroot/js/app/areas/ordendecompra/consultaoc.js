$(function () {
    InicializaPantalla();
});

function InicializaPantalla() {
    $("#chkDesdeHasta").prop('checked', true);
    $("#chkDesdeHasta").trigger("change");
    $("#chkDesdeHasta").prop("disabled", true);
    $("#Date1").prop("disabled", false);
    $("#Date2").prop("disabled", false);
    $("#lbChkDesdeHasta").text("Fechas");
    $("#lbRel01").text("Proveedor");
    $("#lbRel03").text("Sucursal");
    $("#lbRel02").text("Estado");
    $(".activable").prop("disabled", true);
    $("#btnDetalle").prop("disabled", true);
    $("#divFiltro").collapse("show")
    var fecha = moment().format('yyyy-MM-DD');
    $("#Date2").val(fecha)
    fecha = moment($("#FechaEntrega").val()).add(-30, 'day').format('yyyy-MM-DD');
	$("#Date1").val(fecha)
	funcCallBack = BuscarOrdenesDeCompra;
    CerrarWaiting();
    return true;
}

$("#Rel03").autocomplete({
	source: function (request, response) {

		data = { prefix: request.term }; Rel03

		$.ajax({
			url: autoComRel03Url,
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
		if ($("#Rel03List").has('option:contains("' + ui.item.id + '")').length === 0) {
			$("#Rel03Item").val(ui.item.id);
			var opc = "<option value=" + ui.item.id + ">" + ui.item.value + "</option>"
			$("#Rel03List").append(opc);
		}
		return true;
	}
});

function BuscarOrdenesDeCompra(pag = 1) {
	AbrirWaiting();
	var Buscar = "";
	var Date1 = $("#Date1").val();
	var Date2 = $("#Date2").val();
	var Id = "";
	var Id2 = "";
	var Rel01 = [];
	var Rel02 = [];
	var Rel03 = [];
	$("#Rel01List").children().each(function (i, item) { Rel01.push($(item).val()) });
	$("#Rel02List").children().each(function (i, item) { Rel02.push($(item).val()) });
	$("#Rel03List").children().each(function (i, item) {
		var aux = { Id: $(item).val(), Descripcion: $(item).text() };
		Rel03.push(aux);
	});

	var buscaNew = true;
	pagina = pag;
	Pagina = pag;
	var sort = "oc_id";
	var sortDir = "DESC"
	var data2 = { sort, sortDir, Pagina, buscaNew }
	var data1 = { Buscar, Id, Id2, Date1, Date2, Rel01, Rel02, Rel03 };
	var data = $.extend({}, data1, data2);
	PostGenHtml(data, buscarOrdenesDeCompraURL, function (obj) {
		$("#divListaProducto").html(obj);
		$("#divDetalle").collapse("show");
		//addInCellEditHandler();
		//addInCellLostFocusHandler();
		//addInCellKeyDownHandler();
		//AddEventListenerToGrid("tbListaProducto");
		//tableUpDownArrow();
		PostGen({}, buscarMetadataURL, function (obj) {
			if (obj.error === true) {
				AbrirMensaje("ATENCIÓN", obj.msg, function () {
					$("#msjModal").modal("hide");
					return true;
				}, false, ["Aceptar"], "error!", null);
			}
			else {
				totalRegs = obj.metadata.totalCount;
				pags = obj.metadata.totalPages;
				pagRegs = obj.metadata.pageSize;

				$("#pagEstado").val(true).trigger("change");
			}

		});

		BuscarProductosTabOC();

		$("#btnDetalle").prop("disabled", false);
		MostrarDatosDeCuenta(true);
		CargarTopesDeOC();
		CerrarWaiting();
		viendeDesdeBusquedaDeProducto = false;
		return true
	});
}