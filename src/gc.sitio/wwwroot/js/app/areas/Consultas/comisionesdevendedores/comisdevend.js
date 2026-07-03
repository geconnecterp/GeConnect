let _pedidoLoading = false;
let tabsDetallePendientes = 0;
const TabToTableMap = {
	"navs-top-detven": "#tbDetalleDeVentas",
	"navs-top-resven": "#tbResumenDeVentas",
};

$(function () {
	InicializarCamposEnFiltros(false);
	InicializaEventos();

});

function InicializarPantallaPrincipal() {
	var desde = $("#Desde").val();
	var hasta = $("#Hasta").val();
	AbrirWaiting("Cargando información...");
	PostGenHtml({ desde, hasta }, inicializarPantallPrincipalURL, function (obj) {
		$("#divDetalle").html(obj);
		$(document).on('shown.bs.tab', 'button[data-bs-toggle="tab"]', function (e) {
			const tabId = $(e.target).attr("data-bs-target").replace("#", "");
			EvaluarBotonImprimir(tabId);
		});
		tabsDetallePendientes = 2;
		$("#divFiltros").collapse("hide");
		$("#divDetalle").collapse("show");

		setTimeout(() => {
			AbrirWaiting("Cargando información...");
			CargarSeccionDetalleDeVentas();
			CargarSeccionResumenDeVentas();
		}, 200);
		return true
	});
}

function CargarSeccionDetalleDeVentas() {
	var desde = $("#Desde").val();
	var hasta = $("#Hasta").val();
	PostGenHtml({ desde, hasta }, buscarComisionesVentasDetalleURL, function (obj) {
		$("#divDetalleDeVentas").html(obj);
		InicializarEventosDetalleDeVentas();
		EvaluarBotonImprimir("navs-top-detven");
		FinalizarCargaDetalle();
		CerrarWaiting();
		return true
	});
}

function CargarSeccionResumenDeVentas() {
	var desde = $("#Desde").val();
	var hasta = $("#Hasta").val();
	PostGenHtml({ desde, hasta }, buscarComisionesVentasResumenURL, function (obj) {
		$("#divResumenDeVentas").html(obj);
		InicializarEventosResumenDeVentas();
		FinalizarCargaDetalle();
		CerrarWaiting();
		return true
	});
}

function InicializarEventosDetalleDeVentas() {
	$(document).off("click", "#tbDetalleDeVentas tbody tr");
	$(document).on("click", "#tbDetalleDeVentas tbody tr", function (e) {

		if ($(e.target).is("button, a, .btn, i")) return;

		const $nuevaFila = $(this);
		ProcesarSeleccionFilaEnDetalleDeVentas($nuevaFila);
	});
}

function InicializarEventosResumenDeVentas() {
	$(document).off("click", "#tbResumenDeVentas tbody tr");
	$(document).on("click", "#tbResumenDeVentas tbody tr", function (e) {

		if ($(e.target).is("button, a, .btn, i")) return;

		const $nuevaFila = $(this);
		ProcesarSeleccionFilaEnResumenDeVentas($nuevaFila);
	});
}

function ProcesarSeleccionFilaEnDetalleDeVentas($fila) {
	// Quitar selección previa
	$("#tbDetalleDeVentas tbody tr").removeClass("selected-row");
	// Marcar fila seleccionada
	$fila.addClass("selected-row");
}

function ProcesarSeleccionFilaEnResumenDeVentas($fila) {
	// Quitar selección previa
	$("#tbResumenDeVentas tbody tr").removeClass("selected-row");
	// Marcar fila seleccionada
	$fila.addClass("selected-row");
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

function FinalizarCargaDetalle() {
	tabsDetallePendientes--;

	if (tabsDetallePendientes <= 0) {
		CerrarWaiting();
	}
}

function InicializarCamposEnFiltros(vieneDeCancelar) {
	if (!vieneDeCancelar) {
	}

	$("#chkDesdeHasta").prop('checked', true);
	$("#chkDesdeHasta").trigger("change");
	$("#chkDesdeHasta").prop("disabled", true);

	$("#btnImprimir").hide();

	$("#divFiltros").collapse("show");
	$("#divDetalle").collapse("hide");
}

function InicializaEventos() {
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
		const msg = validarFechasAnalisis();
		if (msg == "OK") {
			InicializarPantallaPrincipal();
		} else {
			AbrirMensaje("ATENCIÓN", msg, function () {
				$("#msjModal").modal("hide");
				return true;
			}, false, ["Aceptar"], "error!", null);
		}
	});
}

function validarFechasAnalisis() {
	const desdeInput = document.getElementById("Desde");
	const hastaInput = document.getElementById("Hasta");

	const desde = new Date(desdeInput.value);
	const hasta = new Date(hastaInput.value);

	// Si alguna fecha no está cargada
	if (isNaN(desde) || isNaN(hasta)) {
		return "Faltan completar las fechas.";
	}

	// Validar que Desde <= Hasta
	if (desde > hasta) {
		return "La fecha 'Desde' no puede ser mayor que la fecha 'Hasta'.";
	}

	// Validar que la diferencia no supere 60 días
	const diffMs = hasta - desde;
	const diffDias = diffMs / (1000 * 60 * 60 * 24);

	if (diffDias > 60) {
		return "El rango de fechas no puede superar los 60 días.";
	}

	return "OK";
}
