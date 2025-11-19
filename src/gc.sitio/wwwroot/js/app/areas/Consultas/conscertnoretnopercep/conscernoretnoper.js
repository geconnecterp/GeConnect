$(function () {
	$("#pagEstado").on("change", function () {
		var div = $("#divPaginacion");
		cargaPaginacion();
	});

	$(document).on("click", "#btnImprimir", ControlaImprimirSelected);

	InicializarCamposEnFiltros();

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
		BuscarCertificados(pagina);
	});

	funcCallBack = BuscarCertificados;
});

function ControlaImprimirSelected() {
	if ($("#tbGridCertificados > tbody > tr").length === 0) {
		AbrirMensaje("ATENCIÓN", "No hay datos generar el reporte.", function () {
			$("#msjModal").modal("hide");
			return true;
		}, false, ["Aceptar"], "error!", null);
	}
	else {
		ImprimirListaCertificados_Generada();
	}
}

function ImprimirListaCertificados_Generada() {
	ReseteoDeReportes();
	setTimeout(() => {
		var imp_id = $("#listaTipoImpuestos").val();
		var ret = $("#chkCertNoRet")[0].checked;
		var per = $("#chkCertNoPercep")[0].checked;
		var no_vencido = $("#chkNoVencidos")[0].checked;
		var vencido = $("#chkVencidos")[0].checked;
		var data = { imp_id, ret, per, no_vencido, vencido };
		cargarReporteEnArre(44, data, "CONSULTA VENCIMIENTOS POR TIPO DE CUENTA Y TIPO DE COMPROBANTE", "", "");
		invocacionGestorDoc({});
	}, 500);
}

function cargaPaginacion() {
	$("#divPaginacion").pagination({
		items: totalRegs,
		itemsOnPage: pagRegs,
		cssStyle: "dark-theme",
		currentPage: pagina,
		onPageClick: function (num) {
			BuscarCertificados(num);
		}
	});
	$("#pagEstado").val(false);
	$("#divFiltro").collapse("hide")
	return true;
}

function ValidarFiltrosSeleccionados() {
	var ret = $("#chkCertNoRet")[0].checked;
	var per = $("#chkCertNoPercep")[0].checked;
	if (!ret && !per) {
		return "Debe seleccionar al menos una de las dos opciones de No Retención o No Percepción";
	}
	var tImpuesto = $("#listaTipoImpuestos").val(); 
	return tImpuesto && tImpuesto.length > 0 ? "" : "Debe al menos seleccionar un tipo de impuesto.";
}

function BuscarCertificados(pag) {
	var retMsj = ValidarFiltrosSeleccionados();
	if (retMsj == "") {
		AbrirWaiting("Cargando certificados...");
		var imp_id = $("#listaTipoImpuestos").val();
		var ret = $("#chkCertNoRet")[0].checked;
		var per = $("#chkCertNoPercep")[0].checked;
		var no_vencido = $("#chkNoVencidos")[0].checked;
		var vencido = $("#chkVencidos")[0].checked;
		var data1 = { imp_id, ret, per, no_vencido, vencido };
		var buscaNew = true;
		var sort = null;
		var sortDir = null
		pagina = pag;
		var data2 = { sort, sortDir, pag, buscaNew }
		var data = $.extend({}, data1, data2);
		PostGenHtml(data, buscarCertificadosURL, function (obj) {
			CerrarWaiting();
			$("#divGrillaCertificados").html(obj);
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
			if ($("#tbGridCertificados > tbody > tr").length > 0) {
				$("#btnImprimir").show();
			}
			else {
				$("#btnImprimir").hide();
			}
			CerrarWaiting();
			return true
		}, function (obj) {
			ControlaMensajeError(obj.message);
			CerrarWaiting();
		});
	}
	else {
		AbrirMensaje("ATENCIÓN", retMsj, function () {
			$("#msjModal").modal("hide");
			return true;
		}, false, ["Aceptar"], "error!", null);
	}
}

function InicializarCamposEnFiltros() {
	$("#btnImprimir").hide();
	$("#lbTipoImpuestos").text("Impuesto");
	$("#lbCertNoRet").text("Certificados de No Retención (proveedores)");
	$("#lbCertNoPercep").text("Certificados de No Percepción (Clientes)");
	$("#lbNoVencidos").text("No Vencidos");
	$("#lbVencidos").text("Vencidos");
	$("#chkTipoImpuesto").prop('checked', true);
	$("#chkTipoImpuesto").trigger("change");
	$("#chkTipoImpuesto").prop("disabled", true);
	$("#listaTipoImpuestos").prop("disabled", false);
	$("#listaTipoImpuestos").val("");
	$("#chkCertNoRet").prop('checked', false);
	$("#chkCertNoPercep").prop('checked', false);
	$("#chkNoVencidos").prop('checked', false);
	$("#chkVencidos").prop('checked', false);
	$("#divFiltros").collapse("show");
	$("#divDetalle").collapse("hide");

}