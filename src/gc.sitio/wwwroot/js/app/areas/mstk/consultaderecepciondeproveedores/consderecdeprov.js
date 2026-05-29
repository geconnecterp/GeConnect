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
		const [ok, msg] = ValidarFechasFiltro();
		if (!ok) {
			AbrirMensaje("ATENCIÓN", msg, function () {
				$("#msjModal").modal("hide");
				return true;
			}, false, ["Aceptar"], "error!", null);
		}
		else {
			BuscarRecepciones();
		}
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
	var proveedores = lProv.join(",");
	if (proveedores == "")
		proveedores = "%";
	var suc = ObtenerSucursalesSeleccionadasConTexto("SucursalesList", "listaSucursales").ids;
	var data = { ctaId: proveedores, fechaD, fechaH, suc };
	AbrirWaiting("Espere un momento mientras se presenta las recepciones del proveedor en el periodo seleccionado...");
	PostGenHtml(data, consultarRPProvUrl, function (obj) {
		$("#divRecepciones").html(obj);
		AjustarAlturaTabla("divRecepciones");
		EvaluarBotonImprimir("navs-top-rec");
		$(document).on('shown.bs.tab', 'button[data-bs-toggle="tab"]', function (e) {
			const tabId = $(e.target).attr("data-bs-target").replace("#", "");
			EvaluarBotonImprimir(tabId);
		});
		CerrarWaiting();
	});
}

const TabToTableMap = {
	"navs-top-rec": "#tabRecepciones",
	"navs-top-det": "#tabDetalle",
};

function ObtenerSucursalesSeleccionadasConTexto(sucList, suc) {

	let ids = [];
	let textos = [];

	// 1) Obtener sucursales seleccionadas en el ListBox
	$("#" + sucList + " option").each(function () {
		ids.push($(this).val());
		textos.push($(this).text());
	});

	// 2) Si NO hay ninguna seleccionada → devolver TODAS las del DropDownList
	if (ids.length === 0) {
		$("#" + suc + " option").each(function () {
			const val = $(this).val();
			const txt = $(this).text();

			if (val && val !== "") {
				ids.push(val);
				textos.push(txt);
			}
		});
	}

	return {
		ids: ids.join(","),
		textos: textos.join(", ")
	};
}


function EvaluarBotonImprimir(tabId) {
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

	// Guardamos el tab actual para imprimir
	$("#btnImprimir").data("tab-activo", tabId);
}

var rp_compte_seleccionado = null;
var cta_id_seleccionada = null;

function SeleccionarPeriodo(x, grid) {
	var $row = $(x);

	// Seleccionar visualmente la fila
	selectReg(x, grid);

	// Obtener valores desde los atributos data-*
	var rp = $row.data("rp-compte");
	var cta = $row.data("cta-id");
	consultarRPPDetalle(rp, cta);

	rp_compte_seleccionado = rp;
	cta_id_seleccionada = cta;
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

$("#btnImprimir").on("click", function () {
	const tabId = $(this).data("tab-activo");
	ImprimirSegunTab(tabId);
});

function ReseteoDeReportes() {
	console.log("Reseto de reportes");
	ReporteResetArre();
}

function ImprimirSegunTab(tabId) {

	switch (tabId) {
		case "navs-top-rec":
			ImprimirRecepciones();
			break;

		case "navs-top-det":
			ImprimirRecepcionesDetalle();
			break;
	}
}

function ImprimirRecepciones() {
	AbrirWaiting();
	var tipoReporte = 1;
	var data = { tipoReporte };
	PostGen(data, setearTipoDeReporteUrl, function (obj) {
		CerrarWaiting();
		if (obj.error === true) {
			CerrarWaiting();
			AbrirMensaje("ATENCIÓN", obj.msg, function () {
				$("#msjModal").modal("hide");
				return true;
			}, false, ["Aceptar"], "error!", null);
		}
		else {
			HandlerImprimirRecepciones();
		}
	});
}

function ImprimirRecepcionesDetalle() {
	AbrirWaiting();
	var tipoReporte = 2;
	var data = { tipoReporte };
	PostGen(data, setearTipoDeReporteUrl, function (obj) {
		CerrarWaiting();
		if (obj.error === true) {
			CerrarWaiting();
			AbrirMensaje("ATENCIÓN", obj.msg, function () {
				$("#msjModal").modal("hide");
				return true;
			}, false, ["Aceptar"], "error!", null);
		}
		else {
			HandlerImprimirRecepcionesDetalle();
		}
	});
}

function HandlerImprimirRecepciones() {
	ReseteoDeReportes();
	setTimeout(() => {
		var lProv = [];
		var fechaD = $("#Desde").val();
		var fechaH = $("#Hasta").val();
		$("#Rel01List").children().each(function (i, item) { lProv.push($(item).val()) });
		var proveedores = lProv.join(";");

		let admId = administracion;

		var data = { ctaId: proveedores, fechaD, fechaH };
		cargarReporteEnArre(7, data, "Reporte de Recepción de Proveedores", "", admId);
		invocacionGestorDoc({});
	}, 500);
}

function HandlerImprimirRecepcionesDetalle() {
	ReseteoDeReportes();
	setTimeout(() => {
		var data = { cmptId: rp_compte_seleccionado, ctaId: cta_id_seleccionada };
		let admId = administracion;
		cargarReporteEnArre(8, data, "Reporte de Detalle de Recepción de Proveedores", "", admId);
		invocacionGestorDoc({});
	}, 500);
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

	$("#chkDesdeHasta").prop('checked', true);
	$("#chkDesdeHasta").trigger("change");
	$("#chkDesdeHasta").prop("disabled", true);

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

function ValidarFechasFiltro() {

	let fDesde = $("#Desde").val();
	let fHasta = $("#Hasta").val();

	// 1) Validar que existan
	if (!fDesde || !fHasta) {
		return [false, "Debe seleccionar ambas fechas."];
	}

	// Convertir a Date
	let dDesde = new Date(fDesde);
	let dHasta = new Date(fHasta);

	// 2) Validar fechas inválidas
	if (isNaN(dDesde.getTime()) || isNaN(dHasta.getTime())) {
		return [false, "Alguna de las fechas no es válida."];
	}

	// 3) Validar Desde < Hasta
	if (dDesde > dHasta) {
		return [false, "La fecha 'Desde' no puede ser mayor que la fecha 'Hasta'."];
	}

	// 4) Validar diferencia máxima de 60 días
	let diffMs = dHasta - dDesde;
	let diffDias = diffMs / (1000 * 60 * 60 * 24);

	if (diffDias > 60) {
		return [false, "El rango de fechas no puede superar los 60 días."];
	}

	return [true, ""];
}
