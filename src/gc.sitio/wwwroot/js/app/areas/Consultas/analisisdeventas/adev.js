var sucursales_ids_desde_filtros = null;
let tabsDetallePendientes = 0;
var mes_selected = null;
var periodo_selected = null;

const TabToTableMap = {
	"navs-top-mens": "#tbAnaVtaMes",
	"navs-top-diario": "#tbAnaVtaDetalleDiario",
	"navs-top-hora": "#tbAnaVtaDetalleHora",
	"navs-top-sucursal": "#tbAnaVtaDetalleSucursal",
	"navs-top-cierre": "#tbAnaVtaDetalleCierre",
	"navs-top-anual": "#tbAnaVtaDetalleAnual"
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
			var lSuc = [];
			$("#SucursalesList").children().each(function (i, item) { lSuc.push($(item).val()) });
			if (lSuc.length == 0) {
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

function InicializarPantallaPrincipal() {
	var lSucTExt = [];
	$("#SucursalesList").children().each(function (i, item) { lSucTExt.push($(item).text()) });
	var sucursalesText = lSucTExt.join(", ");
	var desde = $("#Desde").val();
	var hasta = $("#Hasta").val();
	AbrirWaiting("Cargando información..."); 
	PostGenHtml({ sucursalesText, desde, hasta }, inicializarPantallPrincipalURL, function (obj) {
		$("#divDetalle").html(obj);
		$(document).on('shown.bs.tab', 'button[data-bs-toggle="tab"]', function (e) {
			const tabId = $(e.target).attr("data-bs-target").replace("#", "");
			EvaluarBotonImprimir(tabId);
		});
		$(document).on("click", "#btnTabDetalleMes", function () {
			// Esperamos un tick para que Bootstrap active el tab
			setTimeout(function () {
				EvaluarBotonImprimir("navs-top-diario");
			}, 50);
		});
		$("#divFiltros").collapse("hide");
		$("#divDetalle").collapse("show");
		CerrarWaiting();
		setTimeout(() => {
			CargarAnalisisDeVentaMensual();
			CargarAnalisisDeVentaAnual();
		}, 200);
		return true
	});
}

function EvaluarBotonImprimir(tabId) {

	const tablaSelector = TabToTableMap[tabId];
	if (!tablaSelector) {
		$("#btnImprimir").hide();
		return;
	}

	const $tabla = $(tablaSelector);

	// Si la tabla no existe o no tiene filas de datos
	if ($tabla.length === 0 || $tabla.find("tbody tr").length === 0) {
		$("#btnImprimir").hide();
		return;
	}

	// Si tiene datos → mostrar botón
	$("#btnImprimir").show();

	// Guardamos el tab actual para imprimir
	$("#btnImprimir").data("tab-activo", tabId);
}

$("#btnImprimir").on("click", function () {
	const tabId = $(this).data("tab-activo");
	ImprimirSegunTab(tabId);
});

function ImprimirSegunTab(tabId) {

	switch (tabId) {
		case "navs-top-mens":
			ImprimirMensual();
			break;

		case "navs-top-diario":
			ImprimirDetalleDiario();
			break;

		case "navs-top-hora":
			ImprimirDetalleHora();
			break;

		case "navs-top-sucursal":
			ImprimirDetalleSucursal();
			break;

		case "navs-top-cierre":
			ImprimirDetalleCierre();
			break;

		case "navs-top-anual":
			ImprimirAnual();
			break;
	}
}

function ImprimirMensual() {
}

function ImprimirDetalleDiario() {
}

function ImprimirDetalleHora() {
}

function ImprimirDetalleSucursal() {
}

function ImprimirDetalleCierre() {
}

function ImprimirAnual() {
}

function CargarAnalisisDeVentaAnual() {
	var lSuc = [];
	$("#SucursalesList").children().each(function (i, item) { lSuc.push($(item).val()) });
	var sucursalesIds = lSuc.join(",");
	sucursales_ids_desde_filtros = sucursalesIds;
	var data = {
		Desde: $("#Desde").val(),
		Hasta: $("#Hasta").val(),
		Sucursales: sucursalesIds,
	}
	AbrirWaiting("Obteniendo Análisis Anual...");
	PostGenHtml(data, buscarAnalisisDeVentasAnualURL, function (obj) {
		$("#divAnalisisDetalleAnual").html(obj);
		InicializarEventosAnalisisDeVentaAnual();
		CerrarWaiting();
		return true
	});
}

function InicializarEventosAnalisisDeVentaAnual() {
	$(document).off("click", "#tbAnaVtaDetalleAnual tbody tr");
	$(document).on("click", "#tbAnaVtaDetalleAnual tbody tr", function (e) {

		if ($(e.target).is("button, a, .btn, i")) return;

		const $nuevaFila = $(this);
		ProcesarSeleccionFilaEnAnalisisDeVentaAnual($nuevaFila);
	});
}

function ProcesarSeleccionFilaEnAnalisisDeVentaAnual($fila) {
	// Quitar selección previa
	$("#tbAnaVtaDetalleAnual tbody tr").removeClass("selected-row");
	// Marcar fila seleccionada
	$fila.addClass("selected-row");
}

function CargarAnalisisDeVentaMensual() {
	var lSuc = [];
	$("#SucursalesList").children().each(function (i, item) { lSuc.push($(item).val()) });
	var sucursalesIds = lSuc.join(",");
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
	$(document).off("click", "#tbAnaVtaMes tbody tr");
	$(document).on("click", "#tbAnaVtaMes tbody tr", function (e) {

		if ($(e.target).is("button, a, .btn, i")) return;

		const $nuevaFila = $(this);
		ProcesarSeleccionFilaEnAnalisisDeVentaMensual($nuevaFila);
	});
}

function ProcesarSeleccionFilaEnAnalisisDeVentaMensual($fila) {
	// Quitar selección previa
	$("#tbAnaVtaMes tbody tr").removeClass("selected-row");
	// Marcar fila seleccionada
	$fila.addClass("selected-row");

	mes_selected = $fila.data("mes");
	periodo_selected = $fila.data("periodo");

	if (mes_selected && periodo_selected) {
		CargarAnalisisDeVentaDetalleMes(mes_selected, periodo_selected);
	}
}

function CargarAnalisisDeVentaDetalleMes(mes, periodo) {
	PostGenHtml({ mes, periodo, sucursales: sucursales_ids_desde_filtros }, cargarDetalleMesURL, function (obj) {
		$("#divAnalisisDetalleMes").html(obj);
		CargarTabsDelDetalleMes(mes, periodo, sucursales_ids_desde_filtros);
		EvaluarBotonImprimir("navs-top-mens");
		return true;
	});
}

function CargarTabsDelDetalleMes(mes, periodo, sucursales) {
	AbrirWaiting("Cargando datos..."); // ← abrir al inicio
	tabsDetallePendientes = 4; // ← cantidad de tabs a cargar
	//Diario
	CargarTabsDelDetalleMesDiario(mes, periodo, sucursales);
	//Hora
	CargarTabsDelDetalleMesHora(mes, periodo, sucursales);
	//Sucursal
	CargarTabsDelDetalleMesSucursal(mes, periodo, sucursales);
	//Cierre
	CargarTabsDelDetalleMesCierre(mes, periodo, sucursales);

	FinalizarCargaDetalle(); // ← marcar como completado
}

function CargarTabsDelDetalleMesDiario(mes, periodo, sucursales) {
	PostGenHtml({ mes, periodo, sucursales }, cargarDetalleMesDiarioURL, function (obj) {
		$("#divDetalleMesDiario").html(obj);
		InicializarEventosAnalisisDeVentaDetalleDiario();
		FinalizarCargaDetalle(); 
		return true;
	});
}

function CargarTabsDelDetalleMesHora(mes, periodo, sucursales) {
	PostGenHtml({ mes, periodo, sucursales }, cargarDetalleMesHoraURL, function (obj) {
		$("#divDetalleMesHora").html(obj);
		InicializarEventosAnalisisDeVentaDetalleHora();
		FinalizarCargaDetalle(); 
		return true;
	});
}

function CargarTabsDelDetalleMesSucursal(mes, periodo, sucursales) {
	PostGenHtml({ mes, periodo, sucursales }, cargarDetalleMesSucursalURL, function (obj) {
		$("#divDetalleMesSucursal").html(obj);
		InicializarEventosAnalisisDeVentaDetalleSucursal();
		FinalizarCargaDetalle(); 
		return true;
	});
}

function CargarTabsDelDetalleMesCierre(mes, periodo, sucursales) {
	PostGenHtml({ mes, periodo, sucursales }, cargarDetalleMesCierreURL, function (obj) {
		$("#divDetalleMesCierres").html(obj);
		InicializarEventosAnalisisDeVentaDetalleCierres();
		FinalizarCargaDetalle();
		return true;
	});
}

function FinalizarCargaDetalle() {
	tabsDetallePendientes--;

	if (tabsDetallePendientes <= 0) {
		CerrarWaiting();
	}
}

function InicializarEventosAnalisisDeVentaDetalleDiario() {
	$(document).off("click", "#tbAnaVtaDetalleDiario tbody tr");
	$(document).on("click", "#tbAnaVtaDetalleDiario tbody tr", function (e) {

		if ($(e.target).is("button, a, .btn, i")) return;

		const $nuevaFila = $(this);
		ProcesarSeleccionFilaEnAnalisisDeVentaDetalleDiario($nuevaFila);
	});
}

function ProcesarSeleccionFilaEnAnalisisDeVentaDetalleDiario($fila) {
	// Quitar selección previa
	$("#tbAnaVtaDetalleDiario tbody tr").removeClass("selected-row");
	// Marcar fila seleccionada
	$fila.addClass("selected-row");
}

function InicializarEventosAnalisisDeVentaDetalleHora() {
	$(document).off("click", "#tbAnaVtaDetalleHora tbody tr");
	$(document).on("click", "#tbAnaVtaDetalleHora tbody tr", function (e) {

		if ($(e.target).is("button, a, .btn, i")) return;

		const $nuevaFila = $(this);
		ProcesarSeleccionFilaEnAnalisisDeVentaDetalleHora($nuevaFila);
	});
}

function ProcesarSeleccionFilaEnAnalisisDeVentaDetalleHora($fila) {
	// Quitar selección previa
	$("#tbAnaVtaDetalleHora tbody tr").removeClass("selected-row");
	// Marcar fila seleccionada
	$fila.addClass("selected-row");
}

function InicializarEventosAnalisisDeVentaDetalleSucursal() {
	$(document).off("click", "#tbAnaVtaDetalleSucursal tbody tr");
	$(document).on("click", "#tbAnaVtaDetalleSucursal tbody tr", function (e) {

		if ($(e.target).is("button, a, .btn, i")) return;

		const $nuevaFila = $(this);
		ProcesarSeleccionFilaEnAnalisisDeVentaDetalleSucursal($nuevaFila);
	});
}

function ProcesarSeleccionFilaEnAnalisisDeVentaDetalleSucursal($fila) {
	// Quitar selección previa
	$("#tbAnaVtaDetalleSucursal tbody tr").removeClass("selected-row");
	// Marcar fila seleccionada
	$fila.addClass("selected-row");
}

function InicializarEventosAnalisisDeVentaDetalleCierres() {
	$(document).off("click", "#tbAnaVtaDetalleCierre tbody tr");
	$(document).on("click", "#tbAnaVtaDetalleCierre tbody tr", function (e) {

		if ($(e.target).is("button, a, .btn, i")) return;

		const $nuevaFila = $(this);
		ProcesarSeleccionFilaEnAnalisisDeVentaDetalleCierres($nuevaFila);
	});
}

function ProcesarSeleccionFilaEnAnalisisDeVentaDetalleCierres($fila) {
	// Quitar selección previa
	$("#tbAnaVtaDetalleCierre tbody tr").removeClass("selected-row");
	// Marcar fila seleccionada
	$fila.addClass("selected-row");
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

function ControlalistaSucursalesSelected() {
	var item = $("#listaSucursales").val();
	var desc = $("#listaSucursales option:selected").text();
	if ($("#SucursalesList").has('option:contains("' + item + '")').length === 0 && $("#SucursalesList").has('option:contains("' + desc + '")').length === 0) {
		var opc = "<option value=" + item + ">" + desc + "</option>"
		$("#SucursalesList").append(opc);
	}
}