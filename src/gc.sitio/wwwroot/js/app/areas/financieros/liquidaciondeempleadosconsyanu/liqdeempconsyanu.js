$(function () {

	$(document).on("click", "#btnImprimirDetalle", ImprimirDetalle);
	$(document).on("click", "#btnAnularLiqDeEmp", AnularLiquidacion);
	$(document).on("click", "#btnCancelar", ControlaCancelar);

	$("#pagEstado").on("change", function () {
		var div = $("#divPaginacion");
		cargaPaginacion();
	});

	InicializarCamposEnFiltros();

	$("#chkDesdeHasta").prop('checked', true);
	$("#chkDesdeHasta").trigger("change");
	$("#chkDesdeHasta").prop("disabled", true);

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
		dataBak = "";
		pagina = 1;
		BuscarLiquidacionDeEmpleados(pagina);
	});

	funcCallBack = BuscarLiquidacionDeEmpleados;
});

function ControlaCancelar() {
	$("#divFiltros").removeClass("collapse").addClass("show");
	$("#divDetalle").collapse("hide");
	$("#tbGridLiqDeEmpDetalle tbody").empty();
	$("#tbGridLiqDeEmp tbody").empty();
	$(".leyenda-titulo").hide();
	InicializarDatosEnSesion();
}

function InicializarDatosEnSesion() {
	var data = {};
	PostGen(data, inicializarDatosEnSesionUrl, function (obj) {
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
		}
	});
}

function AnularLiquidacion() {
	if (le_compte_selected == "" || le_compte_selected == null || le_compte_selected == undefined) {
		AbrirMensaje("ATENCIÓN", "Debe seleccionar una Liquidación.", function () {
			$("#msjModal").modal("hide");
			return true;
		}, false, ["Aceptar"], "error!", null);
	}
	else if (le_compte_anulada.toUpperCase() == "S") {
		AbrirMensaje("ATENCIÓN", "La Liquidación seleccionada ya se encuentra anulada.", function () {
			$("#msjModal").modal("hide");
			return true;
		}, false, ["Aceptar"], "error!", null);
	}
	else {
		AbrirMensaje("ATENCIÓN", `¿Está seguro que desea anular la Liquidación N° ${le_compte_selected}?`, function (e) {
			$("#msjModal").modal("hide");
			switch (e) {
				case "SI":
					handlerAnularLiquidacion(le_compte_selected);
					break;
				case "NO":
					break;
				default: //NO
					break;
			}
			return true;

		}, true, ["Aceptar", "Cancelar"], "question!", null);
	}
}

function handlerAnularLiquidacion(le_compte_selected) {
	AbrirWaiting(`Anulando Liquidación N° ${le_compte_selected}`);
	var id = le_compte_selected;
	let data = { id };
	PostGen(data, anularLiquidacionDeEmpleadoURL, function (obj) {
		CerrarWaiting();
		AbrirMensaje("ÉXITO", `La Liquidación N° ${le_compte_selected} ha sido anulada correctamente.`, function () {
			$("#msjModal").modal("hide");
			BuscarLiquidacionDeEmpleados(pagina);
			return true;
		}, false, ["Aceptar"], "success!", null);
		return true;
	}, function (obj) {
		CerrarWaiting();
		ControlaMensajeError(obj.responseText);
	});
}

function ImprimirDetalle() {
	var filas = $("#tbGridLiqDeEmpDetalle tbody tr").length;
	if (filas == 0) {
		AbrirMensaje("ATENCIÓN", "No hay datos para imprimir.", function () {
			$("#msjModal").modal("hide");
			return true;
		}, false, ["Aceptar"], "error!", null);
	}
	else {
		ReseteoDeReportes();
		setTimeout(() => {
			var id = le_compte_selected;
			let data = { id };
			cargarReporteEnArre(41, data, "DETALLE DE LIQUIDACIÓN DE HABERES", "", "");
			invocacionGestorDoc({});
		}, 500);
	}
}

function ReseteoDeReportes() {
	console.log("Reseto de reportes");
	ReporteResetArre();
}

function InicializarCamposEnFiltros() {
	$("#Date1, #Date2").on("blur", ValidarFechasClick);
	$("#lbChkDesdeHasta").text("Desde / Hasta");

	$("#Date1").prop("disabled", false);
	$("#Date2").prop("disabled", false);
	$("#divFiltros").collapse("show");
	$("#divDetalle").collapse("hide");
}

function ValidarFechasClick() {
	const desde = $("#Date1").val();
	const hasta = $("#Date2").val();

	if (desde && hasta && desde > hasta) {
		AbrirMensaje("ATENCIÓN", "El valor de Fecha Desde no puede ser mayor a Fecha Hasta, revise.", function () {
			$("#msjModal").modal("hide");
			$("#Date1").val($("#Date2").val());
			return true;
		}, false, ["Aceptar"], "error!", null);
	}
}

function cargaPaginacion() {
	$("#divPaginacion").pagination({
		items: totalRegs,
		itemsOnPage: pagRegs,
		cssStyle: "dark-theme",
		currentPage: pagina,
		onPageClick: function (num) {
			BuscarLiquidacionDeEmpleados(num);
		}
	});
	$("#pagEstado").val(false);
	$("#divFiltro").collapse("hide")
	return true;
}

function BuscarLiquidacionDeEmpleados(pag) {
	AbrirWaiting();
	var desde = $("#Date1").val();
	var hasta = $("#Date2").val();
	var data1 = { desde, hasta };
	var buscaNew = true;
	var sort = null;
	var sortDir = null
	pagina = pag;
	var data2 = { sort, sortDir, pag, buscaNew }
	var data = $.extend({}, data1, data2);
	PostGenHtml(data, buscarLiquidacionesDeEmpleadosURL, function (obj) {
		CerrarWaiting();
		$("#divLiqDeEmp").html(obj);
		$("#divLiqDeEmpDetalle").empty();
		$("#divFiltros").removeClass("show").addClass("collapse");
		$("#divDetalle").collapse("show");
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
				$("#divPaginacion").removeClass("collapse");
			}

		});
		le_compte_selected = "";
		CerrarWaiting();
		return true
	}, function (obj) {
		ControlaMensajeError(obj.message);
		CerrarWaiting();
	});
}

function selectReg(x, gridId) {
	$("#" + gridId + " tbody tr").each(function (index) {
		$(this).removeClass("selected-row");
		$(this).removeClass("selectedEdit-row");
	});
	$(x).addClass("selected-row");
	if (gridId === "tbGridLiqDeEmp") {
		let leCompte = $(x).data("le-compte");
		let leCompteAnulada = $(x).data("le-anulada");
		le_compte_selected = leCompte;
		le_compte_anulada = leCompteAnulada;
		CargarDetalleDeLiquidacion(leCompte);
	}
}

function CargarDetalleDeLiquidacion(leCompte) {
	AbrirWaiting(`Cargando detalle de Liquidación N° ${leCompte}`)
	var data = { leCompte };
	PostGenHtml(data, cargarDetalleDeLiquidacionUrl, function (obj) {
		CerrarWaiting();
		$("#divLiqDeEmpDetalle").html(obj);
		return true
	}, function (obj) {
		CerrarWaiting();
		console.log(obj);
		ControlaMensajeError(obj.responseText);
	});
}