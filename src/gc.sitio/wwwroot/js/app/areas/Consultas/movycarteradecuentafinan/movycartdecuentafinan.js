const TabToTableMap = {
	"navs-top-mov": "#tbMovimientos",
	"navs-top-car": "#tbCartera",
};


$(function () {
	InicializarCamposEnFiltros(false);
	InicializaEventos();

});

function ControlalistaCuentaFinSelected() {

}

function ControlalistaTipoCuentaSelected() {
	var tcf_id = $("#listaTipoCuenta").val();
	if (tcf_id && tcf_id != "") {
		PostGenHtml({ tcf_id }, actualizarListaCuentaFinancieraURL, function (obj) {
			$("#divListaCuentaFin").html(obj);
			$(document).off("change", "#listaCuentaFin");
			$(document).on("change", "#listaCuentaFin", ControlalistaCuentaFinSelected);
			if ($("#chkCuentaFin").is(":checked")) {
				$("#listaCuentaFin").prop("disabled", false);
			} else {
				$("#listaCuentaFin").prop("disabled", true);
			}
			return true
		});
	}
}

function InicializaEventos() {
	$(document).off("change", "#listaTipoCuenta");
	$(document).on("change", "#listaTipoCuenta", ControlalistaTipoCuentaSelected);
	/*$("#btnImprimir").prop("disabled", true);*/

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
			var cuentaFinSeleccionada = $("#listaCuentaFin").val();
			if (!cuentaFinSeleccionada || cuentaFinSeleccionada == "") {
				AbrirMensaje("ATENCIÓN", "Debe al menos seleccionar una cuenta financiera.", function () {
					$("#msjModal").modal("hide");
					$("#listaCuentaFin").trigger('focus');
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
}

let cargasPendientes = 0;

function InicializarPantallaPrincipal() {
	var tipoCuentaSeleccionada = $("#listaTipoCuenta").val();
	var cuentaFinSeleccionada = $("#listaCuentaFin").val();
	var tipoCuentaTexto = $("#listaTipoCuenta option:selected").text();
	var cuentaFinTexto = $("#listaCuentaFin option:selected").text();
	var desde = $("#Desde").val();
	var hasta = $("#Hasta").val();
	AbrirWaiting("Cargando información...");
	PostGenHtml({
		TipoCuenta: tipoCuentaSeleccionada,
		TipoCuentaTexto: tipoCuentaTexto,
		CuentaFinanciera: cuentaFinSeleccionada,
		CuentaFinancieraTexto: cuentaFinTexto,
		desde,
		hasta
	}, inicializarPantallPrincipalURL, function (obj) {
		$("#divDetalle").html(obj);
		$(document).on('shown.bs.tab', 'button[data-bs-toggle="tab"]', function (e) {
			const tabId = $(e.target).attr("data-bs-target").replace("#", "");
			EvaluarBotonImprimir(tabId);
		});
		$("#divFiltros").collapse("hide");
		$("#divDetalle").collapse("show");
		// Inicializamos el contador de cargas
		cargasPendientes = 2;
		// Lanzamos ambas cargas
		CargarSeccionMovimientos();
		CargarSeccionCartera();
		return true
	});
}

function CargarSeccionMovimientos() {
	var tipo_filtro = "R";
	var ct_tipo = "%";
	var desde = $("#Desde").val();
	var hasta = $("#Hasta").val();
	var ctaf_id = $("#listaCuentaFin").val();
	var data = { FechaDesde: desde, FechaHasta: hasta, ctaf_id, tipo_filtro, ct_tipo };
	PostGenHtml(data, obtenerMovimientosListaURL, function (obj) {
		$("#divMovimientos").html(obj);
		//Seguir aca con los eventos
		EvaluarBotonImprimir("navs-top-mov");
		finalizarCarga(); // <-- descontamos una carga
		return true
	});
}

function CargarSeccionCartera() {
	var ctaf_id = $("#listaCuentaFin").val();
	var data = { ctaf_id };
	PostGenHtml(data, obtenerCarterasListaURL, function (obj) {
		$("#divCartera").html(obj);
		//Seguir aca con los eventos
		EvaluarBotonImprimir("navs-top-car");
		finalizarCarga(); // <-- descontamos una carga
		return true
	});
}

function finalizarCarga() {
	cargasPendientes--;

	if (cargasPendientes === 0) {
		CerrarWaiting();
	}
}

function EvaluarBotonImprimir(tabId) {
	console.log("Evaluando botón imprimir para tab:", tabId);

	const tablaSelector = TabToTableMap[tabId];
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
	$("#btnImprimir").data("tab-activo", tabId);
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
	$("#lbTipoCuenta").text("Tipo de Cuenta");
	$("#lbCuentaFin").text("Cuenta Financiera");

	$("#listaTipoCuenta").val("");
	$("#listaCuentaFin").val("");
	$("#TipoCuentaList").empty();
	$("#CuentaFinList").empty();

	$("#divFiltros").collapse("show");
	$("#divDetalle").collapse("hide");

	$("#listaTipoCuenta").prop("disabled", true);
	$("#listaCuentaFin").prop("disabled", true);

	$("#chkTipoCuenta").on("click", function () {
		if ($("#chkTipoCuenta").is(":checked")) {
			$("#listaTipoCuenta").prop("disabled", false);
			$("#listaTipoCuenta").trigger("focus");
		}
		else {
			$("#listaTipoCuenta").prop("disabled", true);
		}
	});

	$("#chkCuentaFin").on("click", function () {
		if ($("#chkCuentaFin").is(":checked")) {
			$("#listaCuentaFin").prop("disabled", false);
			$("#listaCuentaFin").trigger("focus");
		}
		else {
			$("#listaCuentaFin").prop("disabled", true);
		}
	});
}

function ReseteoDeReportes() {
	console.log("Reseto de reportes");
	ReporteResetArre();
}

$("#btnImprimir").on("click", function () {
	const tabId = $(this).data("tab-activo");
	ImprimirSegunTab(tabId);
});

function ImprimirSegunTab(tabId) {

	switch (tabId) {
		case "navs-top-mov":
			ImprimirConsCtaCteFinanciera();
			break;

		case "navs-top-car":
			ImprimirDetalleDeValoresEnCartera();
			break;
	}
}

function ImprimirConsCtaCteFinanciera() {
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
			HandlerImprimirConsCtaCteFinanciera();
		}
	});
}

function ImprimirDetalleDeValoresEnCartera() {
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
			HandlerImprimirDetalleDeValoresEnCartera();
		}
	});
}

function HandlerImprimirConsCtaCteFinanciera() {
	ReseteoDeReportes();
	setTimeout(() => {
		var tipo_filtro = "R";
		var ct_tipo = "%";
		var desde = $("#Desde").val();
		var hasta = $("#Hasta").val();
		var ctaf_id = $("#listaCuentaFin").val();
		var ctaf_desc = $("#listaCuentaFin option:selected").text();
		var data = { FechaDesde: desde, FechaHasta: hasta, ctaf_id, ctaf_desc, tipo_filtro, ct_tipo };
		cargarReporteEnArre(83, data, "Consulta de Cuenta Corriente Financiera", "", "");
		invocacionGestorDoc({});
	}, 500);
}

function HandlerImprimirDetalleDeValoresEnCartera() {
	ReseteoDeReportes();
	setTimeout(() => {
		var ctaf_id = $("#listaCuentaFin").val();
		var ctaf_desc = $("#listaCuentaFin option:selected").text();
		var data = { ctaf_id, ctaf_desc };
		cargarReporteEnArre(84, data, "Detalle de Valores en Cartera", "", "");
		invocacionGestorDoc({});
	}, 500);
}