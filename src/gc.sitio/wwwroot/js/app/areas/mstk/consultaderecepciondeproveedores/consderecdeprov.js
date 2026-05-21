$(function () {
	InicializarCamposEnFiltros();

	$(document).on("change", "#listaSucursales", ControlalistaSucursalesSelected);

	$("#SucursalesList").on("dblclick", 'option', function () { $(this).remove(); })
	$("#Rel01List").on("dblclick", 'option', function () { $(this).remove(); })

	$("#btnFiltro").on("click", function () {
		if ($("#divFiltros").hasClass("show")) {
			$("#divFiltros").collapse("hide");
			$("#divDetalle").collapse("show");
		}
		else {
			$("#divFiltros").collapse("show");
			$("#divDetalle").collapse("hide");
		}
	});

	$("#btnBuscar").on("click", function () {
		BuscarRecepciones();
	});
});

function BuscarRecepciones() {
	var data = {};
	PostGenHtml(data, abrirPantallaPrincipalUrl, function (obj) {
		$("#divDetalle").html(obj);
		$("#divFiltros").collapse("hide");
		$("#divDetalle").collapse("show");
		CargarRecepcionesDeProveedores();
	});
}

function CargarRecepcionesDeProveedores() {
	var lProv = [];
	var fechaD = $("#Desde").val();
	var fechaH = $("#Hasta").val();
	$("#Rel01List").children().each(function (i, item) { lProv.push($(item).val()) });
	var proveedores = lProv.join(";");

	var data = { ctaId: proveedores, fechaD, fechaH };
	AbrirWaiting("Espere un momento mientras se presenta las recepciones del proveedor en el periodo seleccionado...");
	PostGenHtml(data, consultarRPProvUrl, function (obj) {
		$("#divRecepciones").html(obj);
		AjustarAlturaTabla("divRecepciones");
		EvaluarBotonImprimir();
		CerrarWaiting();
	});
}

function EvaluarBotonImprimir() {
	// Buscar cualquier tabla dentro de los contenedores
	var $tabla = $("#divRecepciones table, #divDetalle table").first();

	if ($tabla.length === 0) {
		$("#btnImprimir").hide();
		return;
	}

	var cantidadFilas = $tabla.find("tbody tr").length;

	if (cantidadFilas > 0) {
		$("#btnImprimir").show();       // ← ESTO ES LO QUE FALTABA
		$("#btnImprimir").prop("disabled", false);
	} else {
		$("#btnImprimir").hide();
	}
}

function SeleccionarPeriodo(x, grid) {
	var $row = $(x);

	// Seleccionar visualmente la fila
	selectReg(x, grid);

	// Obtener valores desde los atributos data-*
	var rp = $row.data("rp-compte");
	var cta = $row.data("cta-id");
	consultarRPPDetalle(rp, cta);
	//switch (tabAbm) {
	//	//case 1:
	//	//    break;
	//	//case 2:
	//	//    break;
	//	case 3:
	//		consultaCmpteDetalle(rp, cta);
	//		break;
	//	case 4:
	//		consultaOPPDetalle(rp, cta);
	//		break;
	//	case 5:
	//		consultarRPPDetalle(rp, cta);
	//		break;
	//	default:
	//		return false;
	//}
	//se llama el detalle de comprobantes de un mes especifico
}

function consultarRPPDetalle(rp, cta) {
	var data = { cmptId: rp, ctaId: cta };

	AbrirWaiting("Espere un momento mientras se presenta el detalle de la Recepción seleccionada...");
	PostGenHtml(data, consultarRPProvDetUrl, function (obj) {
		$("#divRecepcionesDetalle").html(obj);
		AjustarAlturaTabla("divRecepcionesDetalle");
		CerrarWaiting();
	});
}

function AjustarAlturaTabla() {
	// Altura visible del viewport
	var altoPantalla = window.innerHeight;

	// Altura que querés restar (botones, filtros, márgenes, etc.)
	var offset = 250; // ajustalo a tu layout

	var altoFinal = altoPantalla - offset;

	$("#divRecepciones .table-wrapper-200, #divRecepciones .table-wrapper-400")
		.css("max-height", altoFinal + "px")
		.css("overflow-y", "auto");
}

function AjustarAlturaTabla(contenedorId) {

	// Altura visible del viewport
	var altoPantalla = window.innerHeight;

	// Ajustá este offset según tu layout
	var offset = 250;

	var altoFinal = altoPantalla - offset;

	// Buscar cualquier wrapper dentro del contenedor
	$("#" + contenedorId + " .table-responsive")
		.css("max-height", altoFinal + "px")
		.css("overflow-y", "auto");
}

$("#Rel01").autocomplete({
	source: function (request, response) {

		data = { prefix: request.term }; Rel01

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
		if ($("#Rel01List").has('option:contains("' + ui.item.id + '")').length === 0) {
			$("#Rel01Item").val(ui.item.id);
			var opc = "<option value=" + ui.item.id + ">" + ui.item.value + "</option>"
			$("#Rel01List").append(opc);
		}
		return true;
	}
});

function ControlalistaSucursalesSelected() {
	var item = $("#listaSucursales").val();
	var desc = $("#listaSucursales option:selected").text();
	if ($("#SucursalesList").has('option:contains("' + item + '")').length === 0 && $("#SucursalesList").has('option:contains("' + desc + '")').length === 0) {
		var opc = "<option value=" + item + ">" + desc + "</option>"
		$("#SucursalesList").append(opc);
	}
}

function InicializarCamposEnFiltros() {
	$("#btnImprimir").hide();
	$("#lbChkDesdeHasta").text("Fecha")
	$("#lbSucursales").text("Sucursales");
	$("#lbRel01").text("Proveedores");

	$("#SucursalesList").empty();
	$("#Rel01List").empty();

	$("#listaSucursales").val("");
	$("#Rel01Item").val("");

	$("#SucursalesList").prop("disabled", true);
	$("#listaSucursales").prop("disabled", true);

	$("#chkSucursales").prop('checked', false);
	$("#chkSucursales").trigger("change");
	$("#chkRel01").prop('checked', false);
	$("#chkRel01").trigger("change");

	$("#divFiltros").collapse("show");
	$("#divDetalle").collapse("hide");

	HandlerCheckBox();
}

function HandlerCheckBox() {
	$("#chkSucursales").on("click", function () {
		if ($("#chkSucursales").is(":checked")) {
			$("#listaSucursales").prop("disabled", false);
			$("#SucursalesList").prop("disabled", false);
			$("#listaSucursales").trigger("focus");
		}
		else {
			$("#listaSucursales").prop("disabled", true);
			$("#SucursalesList").prop("disabled", true);
			$("#listaSucursales").val("");
			$("#SucursalesList").empty();
		}
	});
}