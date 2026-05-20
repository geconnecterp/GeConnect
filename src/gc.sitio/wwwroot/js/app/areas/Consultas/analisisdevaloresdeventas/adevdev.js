var mes_selected = null;
var periodo_selected = null;
let tabsDetallePendientes = 0;

const TabToTableMap = {
	"navs-top-mens": "#tbAnaDeValDeVtaMes",
	"navs-top-diario": "#tbAnaDeValVtaDetalleDiario",
	"navs-top-pv": "#tbAnaDeValVtaDetallePV",
	"navs-top-cashback": "#tbAnaDeValVtaDetalleCB",
};

$(function () {
	InicializarCamposEnFiltros(false);
	$(document).on("change", "#listaSucursales", ControlalistaSucursalesSelected);
	$("#SucursalesList").on("dblclick", 'option', function () { $(this).remove(); })

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
		if (validarFechasAnalisis()) {
			var sucursalesIds = ObtenerSucursalesSeleccionadas();
			if (!sucursalesIds || sucursalesIds.length == 0) {
				AbrirMensaje("ATENCIÓN", "Debe al menos seleccionar una sucursal.", function () {
					$("#msjModal").modal("hide");
					return true;
				}, false, ["Aceptar"], "error!", null);
			} else {
				InicializarPantallaPrincipal();
			}
		} else {
			AbrirMensaje("ATENCIÓN", "Problemas con las fechas, por favor verifique.", function () {
				$("#msjModal").modal("hide");
				return true;
			}, false, ["Aceptar"], "error!", null);
		}
	});
});

function CargarAnalisisDeVentaMensual() {
	var suc = ObtenerSucursalesSeleccionadasConTexto();
	var sucursalesIds = suc.ids;
	sucursales_ids_desde_filtros = sucursalesIds;
	var data = {
		Desde: $("#Desde").val(),
		Hasta: $("#Hasta").val(),
		Sucursales: sucursalesIds,
	}
	AbrirWaiting("Obteniendo Análisis Mensual...");
	PostGenHtml(data, buscarAnalisisDeVentasMensualURL, function (obj) {
		$("#divAnalisisMensual").html(obj);
		InicializarEventosAnalisisDeVentaMensual();
		EvaluarBotonImprimir("navs-top-mens");
		CerrarWaiting();
		return true
	});
}

function InicializarEventosAnalisisDeVentaMensual() {
	$(document).off("click", "#tbAnaDeValDeVtaMes tbody tr");
	$(document).on("click", "#tbAnaDeValDeVtaMes tbody tr", function (e) {

		if ($(e.target).is("button, a, .btn, i")) return;

		const $nuevaFila = $(this);
		ProcesarSeleccionFilaEnAnalisisDeVentaMensual($nuevaFila);
	});
}

function ProcesarSeleccionFilaEnAnalisisDeVentaMensual($fila) {
	// Quitar selección previa
	$("#tbAnaDeValDeVtaMes tbody tr").removeClass("selected-row");
	// Marcar fila seleccionada
	$fila.addClass("selected-row");

	mes_selected = $fila.data("mes");
	periodo_selected = $fila.data("periodo");
}

function InicializarPantallaPrincipal() {
	var suc = ObtenerSucursalesSeleccionadasConTexto();
	var sucursalesText = suc.textos;
	var desde = $("#Desde").val();
	var hasta = $("#Hasta").val();
	AbrirWaiting("Cargando información...");
	PostGenHtml({ sucursalesText, desde, hasta }, inicializarPantallPrincipalURL, function (obj) {
		$("#divDetalle").html(obj);
		mes_selected = null;
		periodo_selected = null;
		$(document).on('shown.bs.tab', 'button[data-bs-toggle="tab"]', function (e) {
			const tabId = $(e.target).attr("data-bs-target").replace("#", "");
			EvaluarBotonImprimir(tabId);
		});
		$(document).on("click", "#btnTabDetalleMes", function () {
			// Esperamos un tick para que Bootstrap active el tab
			setTimeout(function () {
				CargarAnalisisDeValoresDeVentaDetalleMes();
				//EvaluarBotonImprimir("navs-top-diario");
			}, 500);
		});
		$("#divFiltros").collapse("hide");
		$("#divDetalle").collapse("show");
		CerrarWaiting();
		setTimeout(() => {
			CargarAnalisisDeVentaMensual();
		}, 200);
		return true
	});
}

function FinalizarCargaDetalle() {
	tabsDetallePendientes--;

	if (tabsDetallePendientes <= 0) {
		CerrarWaiting();
	}
}

function CargarAnalisisDeValoresDeVentaDetalleMes() {
	PostGenHtml({}, cargarDetalleMesURL, function (obj) {
		$("#divAnalisisDetalleMes").html(obj);
		CargarTabsDelDetalleMes();
		InicializarEventosDetalleMes();
		//EvaluarBotonImprimir("navs-top-mens");
		return true;
	});
}

function InicializarEventosDetalleMes() {
	$(document).off('shown.bs.tab', '#tabsAnalisisDeVentasDetalleMes button[data-bs-toggle="tab"]');

	$(document).on('shown.bs.tab', '#tabsAnalisisDeVentasDetalleMes button[data-bs-toggle="tab"]', function (e) {

		const targetId = $(e.target).data("bs-target");
		let target = "";
		switch (targetId) {

			case "#navs-top-diario":
				target = targetId.replace("#", "");
				CargarTabsDelDetalleMesDiario(true, target);
				CargarTabsDelDetalleMesPV(false, target);
				CargarTabsDelDetalleMesCB(false, target);
				break;

			case "#navs-top-pv":
				target = targetId.replace("#", "");
				CargarTabsDelDetalleMesPV(true, target);
				CargarTabsDelDetalleMesDiario(false, target);
				CargarTabsDelDetalleMesCB(false, target);
				break;

			case "#navs-top-cashback":
				target = targetId.replace("#", "");
				CargarTabsDelDetalleMesCB(true, target);
				CargarTabsDelDetalleMesPV(false, target);
				CargarTabsDelDetalleMesDiario(false, target);
				break;
		}
	});
}

function CargarTabsDelDetalleMes() {
	//Diario
	CargarTabsDelDetalleMesDiario(true, "navs-top-diario");
	//Hora
	CargarTabsDelDetalleMesPV(false, "");
	//Sucursal
	CargarTabsDelDetalleMesCB(false, "");
	//Cierre

	//setTimeout(() => {
	//	EvaluarBotonImprimir("navs-top-diario");
	//}, 1000);

}

function CargarTabsDelDetalleMesDiario(cargar, tabId) {
	if (cargar) {
		var suc = ObtenerSucursalesSeleccionadasConTexto();
		var sucursalesIds = suc.ids;
		var desde = $("#Desde").val();
		var hasta = $("#Hasta").val();
		var data = {
			Desde: desde,
			Hasta: hasta,
			Sucursales: sucursalesIds,
		};
		AbrirWaiting("Cargando datos...");
		PostGenHtml(data, cargarDetalleMesDiarioURL, function (obj) {
			$("#divDetalleMesDiario").html(obj);
			InicializarEventosAnalisisDeValoresDeVentaDetalleDiario();
			EvaluarBotonImprimir(tabId);
			CerrarWaiting();
			return true;
		});
	}
	else {
		$("#divDetalleMesDiario").empty();
	}
}

function InicializarEventosAnalisisDeValoresDeVentaDetalleDiario() {
	$(document).off("click", "#tbAnaDeValVtaDetalleDiario tbody tr");
	$(document).on("click", "#tbAnaDeValVtaDetalleDiario tbody tr", function (e) {

		if ($(e.target).is("button, a, .btn, i")) return;

		const $nuevaFila = $(this);
		ProcesarSeleccionFilaEnAnalisisDeValoresDeVentaDetalleDiario($nuevaFila);
	});
}

function ProcesarSeleccionFilaEnAnalisisDeValoresDeVentaDetalleDiario($fila) {
	// Quitar selección previa
	$("#tbAnaDeValVtaDetalleDiario tbody tr").removeClass("selected-row");
	// Marcar fila seleccionada
	$fila.addClass("selected-row");
}

function CargarTabsDelDetalleMesPV(cargar, tabId) {
	if (cargar) {
		var suc = ObtenerSucursalesSeleccionadasConTexto();
		var sucursalesIds = suc.ids;
		var desde = $("#Desde").val();
		var hasta = $("#Hasta").val();
		var data = {
			Desde: desde,
			Hasta: hasta,
			Sucursales: sucursalesIds,
		};
		AbrirWaiting("Cargando datos...");
		PostGenHtml(data, cargarDetalleMesPVURL, function (obj) {
			$("#divDetalleMesPV").html(obj);
			InicializarEventosAnalisisDeValoresDeVentaDetallePV();
			EvaluarBotonImprimir(tabId);
			CerrarWaiting();
			return true;
		});
	}
	else {
		$("#divDetalleMesPV").empty();
	}
}

function InicializarEventosAnalisisDeValoresDeVentaDetallePV() {
	$(document).off("click", "#tbAnaDeValVtaDetallePV tbody tr");
	$(document).on("click", "#tbAnaDeValVtaDetallePV tbody tr", function (e) {

		if ($(e.target).is("button, a, .btn, i")) return;

		const $nuevaFila = $(this);
		ProcesarSeleccionFilaEnAnalisisDeValoresDeVentaDetallePV($nuevaFila);
	});
}

function ProcesarSeleccionFilaEnAnalisisDeValoresDeVentaDetallePV($fila) {
	// Quitar selección previa
	$("#tbAnaDeValVtaDetallePV tbody tr").removeClass("selected-row");
	// Marcar fila seleccionada
	$fila.addClass("selected-row");
}

function CargarTabsDelDetalleMesCB(cargar, tabId) {
	if (cargar) {
		var suc = ObtenerSucursalesSeleccionadasConTexto();
		var sucursalesIds = suc.ids;
		var desde = $("#Desde").val();
		var hasta = $("#Hasta").val();
		var data = {
			Desde: desde,
			Hasta: hasta,
			Sucursales: sucursalesIds,
		};
		AbrirWaiting("Cargando datos...");
		PostGenHtml(data, cargarDetalleMesCBURL, function (obj) {
			$("#divDetalleMesCashback").html(obj);
			InicializarEventosAnalisisDeValoresDeVentaDetalleCB();
			EvaluarBotonImprimir(tabId);
			CerrarWaiting();
			return true;
		});
	}
	else {
		$("#divDetalleMesCashback").empty();
	}
}

function InicializarEventosAnalisisDeValoresDeVentaDetalleCB() {
	$(document).off("click", "#tbAnaDeValVtaDetallePV tbody tr");
	$(document).on("click", "#tbAnaDeValVtaDetallePV tbody tr", function (e) {

		if ($(e.target).is("button, a, .btn, i")) return;

		const $nuevaFila = $(this);
		ProcesarSeleccionFilaEnAnalisisDeValoresDeVentaDetallePV($nuevaFila);
	});
}

function EvaluarBotonImprimir(tabId) {
	console.log("Evaluando botón imprimir para tab:", tabId);
	const tablaSelector = TabToTableMap[tabId];
	if (!tablaSelector) {
		console.log("tablaSelector:", tablaSelector);
		$("#btnImprimir").hide();
		return;
	}

	const $tabla = $(tablaSelector);

	// Si la tabla no existe o no tiene filas de datos
	if ($tabla.length === 0 || $tabla.find("tbody tr").length === 0) {
		console.log("$tabla.length:", $tabla.length);
		console.log("$tabla.find(tbody tr).length:", $tabla.find("tbody tr").length);
		$("#btnImprimir").hide();
		return;
	}

	// Si tiene datos → mostrar botón
	$("#btnImprimir").show();

	// Guardamos el tab actual para imprimir
	$("#btnImprimir").data("tab-activo", tabId);
}

function ControlalistaSucursalesSelected() {
	var item = $("#listaSucursales").val();
	var desc = $("#listaSucursales option:selected").text();
	if ($("#SucursalesList").has('option:contains("' + item + '")').length === 0 && $("#SucursalesList").has('option:contains("' + desc + '")').length === 0) {
		var opc = "<option value=" + item + ">" + desc + "</option>"
		$("#SucursalesList").append(opc);
	}
}

function validarFechasAnalisis() {
	const desdeInput = document.getElementById("Desde");
	const hastaInput = document.getElementById("Hasta");

	const desde = new Date(desdeInput.value);
	const hasta = new Date(hastaInput.value);

	// Si alguna fecha no está cargada, no validamos todavía
	if (isNaN(desde) || isNaN(hasta)) {
		return true;
	}

	if (desde > hasta) {
		return false;
	}

	return true;
}

function ObtenerSucursalesSeleccionadas() {

	// 1) Obtener sucursales seleccionadas en el ListBox
	let seleccionadas = [];
	$("#SucursalesList option").each(function () {
		seleccionadas.push($(this).val());
	});

	// 2) Si NO hay ninguna seleccionada → devolver TODAS las del DropDownList
	if (seleccionadas.length === 0) {
		$("#listaSucursales option").each(function () {
			const val = $(this).val();
			if (val && val !== "") {
				seleccionadas.push(val);
			}
		});
	}

	// 3) Devolver como string separado por comas
	return seleccionadas.join(",");
}

function ObtenerSucursalesSeleccionadasConTexto() {

	let ids = [];
	let textos = [];

	// 1) Obtener sucursales seleccionadas en el ListBox
	$("#SucursalesList option").each(function () {
		ids.push($(this).val());
		textos.push($(this).text());
	});

	// 2) Si NO hay ninguna seleccionada → devolver TODAS las del DropDownList
	if (ids.length === 0) {
		$("#listaSucursales option").each(function () {
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
		case "navs-top-mens":
			ImprimirMensual();
			break;

		case "navs-top-diario":
			ImprimirDetalleDiario();
			break;

		case "navs-top-pv":
			ImprimirDetallePV();
			break;

		case "navs-top-cashback":
			ImprimirDetalleCashback();
			break;

	}
}

function ImprimirMensual() {
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
			HandlerImprimirMensual();
		}
	});
}

function HandlerImprimirMensual() {
	ReseteoDeReportes();
	setTimeout(() => {
		var suc = ObtenerSucursalesSeleccionadasConTexto();
		var sucursalesIds = suc.ids;
		var sucursalesTextos = suc.textos;
		var data = {
			Desde: $("#Desde").val(),
			Hasta: $("#Hasta").val(),
			Sucursales: sucursalesIds,
			SucursalesTextos: sucursalesTextos
		}
		cargarReporteEnArre(75, data, "Análisis de Valores de Venta Mensual", "", "");
		invocacionGestorDoc({});
	}, 500);
}

function ImprimirDetalleDiario() {
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
			HandlerImprimirDetalleDiario();
		}
	});
}

function HandlerImprimirDetalleDiario() {
	ReseteoDeReportes();
	setTimeout(() => {
		var suc = ObtenerSucursalesSeleccionadasConTexto();
		var sucursalesIds = suc.ids;
		var sucursalesTextos = suc.textos;
		var data = {
			Desde: $("#Desde").val(),
			Hasta: $("#Hasta").val(),
			Sucursales: sucursalesIds,
			SucursalesTextos: sucursalesTextos
		}
		cargarReporteEnArre(76, data, "Análisis de Valores de Venta Diario", "", "");
		invocacionGestorDoc({});
	}, 500);
}

function ImprimirDetallePV() {
	AbrirWaiting();
	var tipoReporte = 3;
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
			HandlerImprimirDetallePV();
		}
	});
}

function HandlerImprimirDetallePV() {
	ReseteoDeReportes();
	setTimeout(() => {
		var suc = ObtenerSucursalesSeleccionadasConTexto();
		var sucursalesIds = suc.ids;
		var sucursalesTextos = suc.textos;
		var data = {
			Desde: $("#Desde").val(),
			Hasta: $("#Hasta").val(),
			Sucursales: sucursalesIds,
			SucursalesTextos: sucursalesTextos
		}
		cargarReporteEnArre(77, data, "Análisis de Valores de Venta por PV", "", "");
		invocacionGestorDoc({});
	}, 500);
}

function ImprimirDetalleCashback() {
	AbrirWaiting();
	var tipoReporte = 4;
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
			HandlerImprimirDetalleCashback();
		}
	});
}

function HandlerImprimirDetalleCashback() {
	ReseteoDeReportes();
	setTimeout(() => {
		var suc = ObtenerSucursalesSeleccionadasConTexto();
		var sucursalesIds = suc.ids;
		var sucursalesTextos = suc.textos;
		var data = {
			Desde: $("#Desde").val(),
			Hasta: $("#Hasta").val(),
			Sucursales: sucursalesIds,
			SucursalesTextos: sucursalesTextos
		}
		cargarReporteEnArre(78, data, "Análisis de Valores de Venta por PV", "", "");
		invocacionGestorDoc({});
	}, 500);
}

function InicializarCamposEnFiltros(vieneDeCancelar) {
	if (!vieneDeCancelar) {
	}

	$("#chkDesdeHasta").prop('checked', true);
	$("#chkDesdeHasta").trigger("change");
	$("#chkDesdeHasta").prop("disabled", true);

	$("#btnImprimir").hide();
	$("#lbChkDesdeHasta").text("Fecha");
	$("#lbSucursales").text("Sucursal");

	$("#chkSucursales").prop('checked', true);
	$("#chkSucursales").trigger("change");
	$("#chkSucursales").prop("disabled", true);

	$("#listaSucursales").val("");
	$("#listaEstados").val("");

	$("#divFiltros").collapse("show");
	$("#divDetalle").collapse("hide");

	let sucSele = $("#SucursalSeleccionada").val();
	$("#listaSucursales").val(sucSele);

	setTimeout(() => {
		let habilitado = $("#HabilitarCambioDeSucursalSeleccionada").val();
		if ($("#HabilitarCambioDeSucursalSeleccionada").val() == "False")
			$("#divListaSucursales").find("input, select, textarea, button").prop("disabled", true);
		else
			$("#divListaSucursales").find("input, select, textarea, button").prop("disabled", false);
	}, 500);
}