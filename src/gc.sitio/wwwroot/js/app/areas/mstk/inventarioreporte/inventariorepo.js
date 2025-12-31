var invNroSeleccionado = "";
var inveIdSeleccionado = "";
var invtIdSeleccionado = "";
$(function () {
	InicializarCamposEnFiltros(false);

	$("#Date1, #Date2").on("blur", ValidarFechasClick);

	$("#btnBuscar").on("click", function () {
		if (validarFechas()) {
			BuscarInventarios();
		} else {
			AbrirMensaje("ATENCIÓN", "Problemas con las fechas, por favor verifique.", function () {
				$("#msjModal").modal("hide");
				return true;
			}, false, ["Aceptar"], "error!", null);
		}
	});

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
});

function BuscarInventarios() {
	var data = {};
	AbrirWaiting("Inicializando presentación de vista de reporte de inventario...");
	PostGenHtml(data, inicializarPantallPrincipalURL, function (obj) {
		$("#divDetalle").html(obj);
		$("#divFiltros").collapse("hide");
		$("#divDetalle").collapse("show");
		$("#btnTabInventarioReporte").addClass("tab-disabled");
		AgregarHandlerEnTabs();
		CerrarWaiting();
		CargarInventarioLista();
		setTimeout(() => {
			CargarEventosABotonesEnDivPrincipal();
		}, 500);
		return true
	});
}

function AgregarHandlerEnTabs() {
	// Cuando el usuario hace click en "Inventarios"
	$('#btnTabInventarioLista').on('shown.bs.tab', function () {
		$('#btnImprimir').hide();
	});

	// Cuando el usuario hace click en "Reporte"
	$('#btnTabInventarioReporte').on('shown.bs.tab', function () {
		$('#btnImprimir').show();
	});

}

function CargarEventosABotonesEnDivPrincipal() {
	$(document).on("click", "#btnRepoStkVsConteo", CargarTabRepoStkVsConteo);
	$(document).on("click", "#btnRepoValorPorSec", CargarTabRepoValorPorSec);
	$(document).on("click", "#btnRepoValorPorRub", CargarTabRepoValorPorRub);
	$(document).on("click", "#btnRepoValorDetalle", CargarTabRepoValorDetalle);
	$(document).on("click", "#btnRepoConteoPorUsu", CargarTabRepoConteoPorUsu);
}

function CargarTabRepoStkVsConteo() { }

function CargarTabRepoValorDetalle() { }

function CargarTabRepoConteoPorUsu() { }

function CargarTabRepoValorPorRub() {
	var inv_nro = invNroSeleccionado;
	if (inv_nro == "") {
		AbrirMensaje("ATENCIÓN", "Debe seleccionar un inventario.", function () {
			$("#msjModal").modal("hide");
			return true;
		}, false, ["Aceptar"], "error!", null);
	}
	else {
		AbrirWaiting("Cargando datos...");
		var data = { inv_nro };
		PostGenHtml(data, inicializarTabRepoValorPorRubURL, function (obj) {
			$("#divReporte").html(obj);
			let tab = new bootstrap.Tab(document.querySelector("#btnTabInventarioReporte"));
			setTimeout(() => {
				$(document).off("click", "#btnImprimir").on("click", "#btnImprimir", ControlaImprRepoValorPorRub);
			}, 300);
			tab.show();
			$("#btnImprimir").show();
			CerrarWaiting();
			return true
		});
	}
}

function CargarTabRepoValorPorSec() {
	var inv_nro = invNroSeleccionado;
	if (inv_nro == "") {
		AbrirMensaje("ATENCIÓN", "Debe seleccionar un inventario.", function () {
			$("#msjModal").modal("hide");
			return true;
		}, false, ["Aceptar"], "error!", null);
	}
	else {
		AbrirWaiting("Cargando datos...");
		var data = { inv_nro };
		PostGenHtml(data, inicializarTabRepoValorPorSecURL, function (obj) {
			$("#divReporte").html(obj);
			let tab = new bootstrap.Tab(document.querySelector("#btnTabInventarioReporte"));
			setTimeout(() => {
				$(document).off("click", "#btnImprimir").on("click", "#btnImprimir", ControlaImprRepoValorPorSec);
			}, 300);
			tab.show();
			$("#btnImprimir").show();
			CerrarWaiting();
			return true
		});
	}
}

function ControlaImprRepoValorPorRub() {
	var filas = $("#tbGridInvValorPorRub tbody tr[data-inv-nro]").length;
	if (filas == 0) {
		AbrirMensaje("ATENCIÓN", "No hay datos para imprimir.", function () {
			$("#msjModal").modal("hide");
			return true;
		}, false, ["Aceptar"], "error!", null);
	}
	else {
		AbrirWaiting("Imprimiendo listado...");
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
				CerrarWaiting();
				ImpimirRepoValorPorRub();
			}
		});
	}
}

function ControlaImprRepoValorPorSec() {
	var filas = $("#tbGridInvValorPorSec tbody tr[data-inv-nro]").length;
	if (filas == 0) {
		AbrirMensaje("ATENCIÓN", "No hay datos para imprimir.", function () {
			$("#msjModal").modal("hide");
			return true;
		}, false, ["Aceptar"], "error!", null);
	}
	else {
		AbrirWaiting("Imprimiendo listado...");
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
				CerrarWaiting();
				ImpimirRepoValorPorSec();
			}
		});
	}
}

function ImpimirRepoValorPorRub() {
	ReseteoDeReportes();
	setTimeout(() => {
		var inv_nro = invNroSeleccionado;
		var data = { inv_nro };
		cargarReporteEnArre(59, data, "VALORIZADO POR RUBROS", "", "");
		invocacionGestorDoc({});
	}, 500);
}

function ImpimirRepoValorPorSec() {
	ReseteoDeReportes();
	setTimeout(() => {
		var inv_nro = invNroSeleccionado;
		var data = { inv_nro };
		cargarReporteEnArre(58, data, "VALORIZADO POR SECTORES", "", "");
		invocacionGestorDoc({});
	}, 500);
}

function selectReg(x, gridId) {
	$("#" + gridId + " tbody tr").each(function (index) {
		$(this).removeClass("selected-row");
		$(this).removeClass("selectedEdit-row");
	});
	$(x).addClass("selected-row");
	if (gridId == 'tbGridInventario') {
		invNroSeleccionado = x.getAttribute("data-inv-nro");
		inveIdSeleccionado = x.getAttribute("data-inve-id");
		invtIdSeleccionado = x.getAttribute("data-invt-id");
	}
}

function CargarInventarioLista() {
	TaskManager.start();
	var desde = $("#Desde").val();
	var hasta = $("#Hasta").val();
	const estadoBool = $("#chkEstados").is(":checked");
	let inve_id = "%";
	if (estadoBool) {
		inve_id = $("#listaEstados").val();
	}
	
	const sucursalBool = $("#chkSucursales").is(":checked");
	let adm_id = "%";
	if (sucursalBool) {
		adm_id = $("#listaSucursales").val();
	}
	else {
		if (!sucursalBool && $("#listaSucursales").val() != "") {
			adm_id = $("#listaSucursales").val();
		}
	}
	
	var data = { desde, hasta, adm_id, inve_id };
	PostGenHtml(data, buscarInventarioListaURL, function (obj) {
		$("#divGrillaInventarios").html(obj);
		TaskManager.end();
		return true
	});
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
	$("#lbEstados").text("Estado");

	$("#chkSucursales").prop('checked', false);
	$("#chkSucursales").trigger("change");
	$("#chkEstados").prop('checked', false);
	$("#chkEstados").trigger("change");

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

	if (!vieneDeCancelar) {
		HandlerCheckBox();
	}
}

function HandlerCheckBox() {
	$("#chkSucursales").on("click", function () {
		if ($("#chkSucursales").is(":checked")) {
			$("#listaSucursales").prop("disabled", false);
			$("#listaSucursales").trigger("focus");
		}
		else {
			$("#listaSucursales").prop("disabled", true);
			$("#listaSucursales").val("");
		}
	});
	$("#chkEstados").on("click", function () {
		if ($("#chkEstados").is(":checked")) {
			$("#listaEstados").prop("disabled", false);
			$("#listaEstados").trigger("focus");
		}
		else {
			$("#listaEstados").prop("disabled", true);
			$("#listaEstados").val("");
		}
	});
}

function validarFechas() {
	let desde = $("#Desde").val();
	let hasta = $("#Hasta").val();

	if (!desde || !hasta) return false;

	let fechaDesde = new Date(desde);
	let fechaHasta = new Date(hasta);

	return !(fechaDesde > fechaHasta);
}

function ValidarFechasClick() {
	const desde = $("#Desde").val();
	const hasta = $("#Hasta").val();

	if (desde && hasta && desde > hasta) {
		AbrirMensaje("ATENCIÓN", "El valor de Fecha Desde no puede ser mayor a Fecha Hasta, revise.", function () {
			$("#msjModal").modal("hide");
			$("#Desde").val($("#Hasta").val());
			return true;
		}, false, ["Aceptar"], "error!", null);
	} else {
	}
}

function ReseteoDeReportes() {
	console.log("Reseto de reportes");
	ReporteResetArre();
}