$(function () {
	$("#pagEstado").on("change", function () {
		var div = $("#divPaginacion");
		cargaPaginacion();
	});

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
	var tImpuesto = $("#listaTipoImpuestos").val(); 
	return tImpuesto && tImpuesto.length > 0 ? true : false;
}

function BuscarCertificados(pag) {
	if (ValidarFiltrosSeleccionados()) {
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
		AbrirMensaje("ATENCIÓN", "Debe al menos seleccioanr una opcion de filtro.", function () {
			$("#msjModal").modal("hide");
			return true;
		}, false, ["Aceptar"], "error!", null);
	}
}

function InicializarCamposEnFiltros() {
	$("#btnImprimir").hide();
	$("#lbTipoImpuestos").text("Impuesto");
	$("#chkCertNoRet").prop('checked', true);
	$("#chkCertNoRet").trigger("change");
	$("#chkCertNoRet").prop("disabled", true);
	$("#listaTipoImpuestos").val("");
	$("#chkCertNoRet").prop('checked', false);
	$("#chkCertNoPercep").prop('checked', false);
	$("#chkNoVencidos").prop('checked', false);
	$("#chkVencidos").prop('checked', false);
}