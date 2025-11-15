$(function () {
	$("#pagEstado").on("change", function () {
		var div = $("#divPaginacion");
		cargaPaginacion();
	});

	$(document).on("click", "#btnCancelar", ControlaCancelar);
	$(document).on("click", "#btnImprimir", ControlaImprimirSelected);
	$(document).on("change", "#listaTipoClientes", ControlalistaTipoClientesSelected);
	$(document).on("change", "#listaTipoProveedores", ControlalistaTipoProveedoresSelected);
	$(document).on("change", "#listaTipoComptes", ControlalistaTipoComptesSelected);
	//btnImprimir

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
		BuscarVencimientos(pagina);
	});

	$("#TipoClientesList").on("dblclick", 'option', function () { $(this).remove(); })
	$("#TipoProveedoresList").on("dblclick", 'option', function () { $(this).remove(); })
	$("#TipoComptesList").on("dblclick", 'option', function () { $(this).remove(); })

	funcCallBack = BuscarVencimientos;
});

function ControlaImprimirSelected() {
	if ($("#tbGridVencimientos > tbody > tr").length === 0) {
		AbrirMensaje("ATENCIÓN", "No hay datos generar el reporte.", function () {
			$("#msjModal").modal("hide");
			return true;
		}, false, ["Aceptar"], "error!", null);
	}
	else {
		ImprimirListaVencimientos_Generada();
	}
}

function ImprimirListaVencimientos_Generada() {
	ReseteoDeReportes();
	setTimeout(() => {
		let fv = $("#chkDesdeHastaVenc")[0].checked;
		let fvDesde = $("#FechaVencDesde").val();
		let fvhasta = $("#FechaVencHasta").val();
		var fvDesdePrint = moment($("#FechaVencDesde").val()).format('DD/MM/yyyy')
		var fvHastaPrint = moment($("#FechaVencHasta").val()).format('DD/MM/yyyy')
		let fg = $("#chkDesdeHastaGen")[0].checked;
		let fgDesde = $("#FechaGenDesde").val();
		let fghasta = $("#FechaGenHasta").val();
		var fgDesdePrint = moment($("#FechaGenDesde").val()).format('DD/MM/yyyy')
		var fgHastaPrint = moment($("#FechaGenHasta").val()).format('DD/MM/yyyy')
		let id_ctc = $("#chkTipoClientes")[0].checked;
		let ctc_list = [];
		if ($("#chkTipoClientes").is(":checked")) {
			$("#TipoClientesList").children().each(function (i, item) { ctc_list.push($(item).val()) });
		}
		let id_ope = $("#chkTipoProveedores")[0].checked;
		let ope_list = [];
		if ($("#chkTipoProveedores").is(":checked")) {
			$("#TipoProveedoresList").children().each(function (i, item) { ope_list.push($(item).val()) });
		}
		let id_tco = $("#chkTipoComptes")[0].checked;
		let tco_list = [];
		if ($("#chkTipoComptes").is(":checked")) {
			$("#TipoComptesList").children().each(function (i, item) { tco_list.push($(item).val()) });
		}
		let data = {
			fv, fvDesde, fvDesdePrint, fvhasta, fvHastaPrint,
			fg, fgDesde, fgDesdePrint, fghasta, fgHastaPrint,
			id_ctc, ctc_list, id_ope, ope_list, id_tco, tco_list
		};
		cargarReporteEnArre(43, data, "CONSULTA VENCIMIENTOS POR TIPO DE CUENTA Y TIPO DE COMPROBANTE", "", "");
		invocacionGestorDoc({});
	}, 500);
}

function ReseteoDeReportes() {
	console.log("Reseto de reportes");
	ReporteResetArre();
}

function ControlalistaTipoClientesSelected() {
	var item = $("#listaTipoClientes").val();
	var desc = $("#listaTipoClientes option:selected").text();
	if ($("#TipoClientesList").has('option:contains("' + item + '")').length === 0 && $("#TipoClientesList").has('option:contains("' + desc + '")').length === 0) {
		var opc = "<option value=" + item + ">" + desc + "</option>"
		$("#TipoClientesList").append(opc);
	}
}

function ControlalistaTipoProveedoresSelected() {
	var item = $("#listaTipoProveedores").val();
	var desc = $("#listaTipoProveedores option:selected").text();
	if ($("#TipoProveedoresList").has('option:contains("' + item + '")').length === 0 && $("#TipoProveedoresList").has('option:contains("' + desc + '")').length === 0) {
		var opc = "<option value=" + item + ">" + desc + "</option>"
		$("#TipoProveedoresList").append(opc);
	}
}

function ControlalistaTipoComptesSelected() {
	var item = $("#listaTipoComptes").val();
	var desc = $("#listaTipoComptes option:selected").text();
	if ($("#TipoComptesList").has('option:contains("' + item + '")').length === 0 && $("#TipoComptesList").has('option:contains("' + desc + '")').length === 0) {
		var opc = "<option value=" + item + ">" + desc + "</option>"
		$("#TipoComptesList").append(opc);
	}
}

function cargaPaginacion() {
	$("#divPaginacion").pagination({
		items: totalRegs,
		itemsOnPage: pagRegs,
		cssStyle: "dark-theme",
		currentPage: pagina,
		onPageClick: function (num) {
			BuscarVencimientos(num);
		}
	});
	$("#pagEstado").val(false);
	$("#divFiltro").collapse("hide")
	return true;
}

function BuscarVencimientos(pag) {
	if (ValidarFiltrosSeleccionados()) {
		AbrirWaiting("Cargando vencimientos...");
		var fv = $("#chkDesdeHastaVenc")[0].checked;
		var fvDesde = $("#FechaVencDesde").val();
		var fvhasta = $("#FechaVencHasta").val();
		var fg = $("#chkDesdeHastaGen")[0].checked;
		var fgDesde = $("#FechaGenDesde").val();
		var fghasta = $("#FechaGenHasta").val();
		var id_ctc = $("#chkTipoClientes")[0].checked;
		var ctc_list = [];
		if ($("#chkTipoClientes").is(":checked")) {
			$("#TipoClientesList").children().each(function (i, item) { ctc_list.push($(item).val()) });
		}
		var id_ope = $("#chkTipoProveedores")[0].checked;
		var ope_list = [];
		if ($("#chkTipoProveedores").is(":checked")) {
			$("#TipoProveedoresList").children().each(function (i, item) { ope_list.push($(item).val()) });
		}
		var id_tco = $("#chkTipoComptes")[0].checked;
		var tco_list = [];
		if ($("#chkTipoComptes").is(":checked")) {
			$("#TipoComptesList").children().each(function (i, item) { tco_list.push($(item).val()) });
		}
		var data1 = { fv, fvDesde, fvhasta, fg, fgDesde, fghasta, id_ctc, ctc_list, id_ope, ope_list, id_tco, tco_list };
		var buscaNew = true;
		var sort = null;
		var sortDir = null
		pagina = pag;
		var data2 = { sort, sortDir, pag, buscaNew }
		var data = $.extend({}, data1, data2);
		PostGenHtml(data, buscarVencimientosURL, function (obj) {
			CerrarWaiting();
			$("#divGrillaVencimientos").html(obj);
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
			if ($("#tbGridVencimientos > tbody > tr").length > 0) {
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

function ValidarFiltrosSeleccionados() {
	let fv = $("#chkDesdeHastaVenc")[0].checked;
	let fg = $("#chkDesdeHastaGen")[0].checked;
	let id_ctc = $("#chkTipoClientes")[0].checked;
	let id_ope = $("#chkTipoProveedores")[0].checked;
	let id_tco = $("#chkTipoComptes")[0].checked;
	if (fv || fg || id_ctc || id_ope || id_tco) {
		return true;
	}
	else {
		return false;
	}
}

function ControlaCancelar() {
	$("#divFiltros").removeClass("collapse").addClass("show");
	$("#divDetalle").collapse("hide");
	//$("#tbGridAnticipoFinEmpDetalle tbody").empty();
	//InicializarDatosEnSesion();
	ResetDeFiltros();
}

function ResetDeFiltros() {
	$("#TipoClientesList").empty();
	$("#TipoProveedoresList").empty();
	$("#TipoComptesList").empty();
	$("#listaTipoClientes").val("");
	$("#listaTipoProveedores").val("");
	$("#listaTipoComptes").val("");
	
	$("#chkDesdeHastaVenc").prop('checked', false);
	$("#chkDesdeHastaVenc").trigger("change");

	$("#chkDesdeHastaGen").prop('checked', false);
	$("#chkDesdeHastaGen").trigger("change");

	$("#chkTipoClientes").prop('checked', false);
	$("#chkTipoClientes").trigger("change");

	$("#chkTipoProveedores").prop('checked', false);
	$("#chkTipoProveedores").trigger("change");

	$("#chkTipoComptes").prop('checked', false);
	$("#chkTipoComptes").trigger("change");

	$("#listaTipoClientes").prop("disabled", true);
	$("#listaTipoProveedores").prop("disabled", true);
	$("#listaTipoComptes").prop("disabled", true);

	$("#FechaVencDesde").prop("disabled", true);
	$("#FechaVencHasta").prop("disabled", true);

	$("#FechaGenDesde").prop("disabled", true);
	$("#FechaGenHasta").prop("disabled", true);
}

function InicializarCamposEnFiltros() {
	$("#btnImprimir").hide();
	$("#FechaVencDesde, #FechaVencHasta").on("blur", function () {
		ValidarFechasClick("#FechaVencDesde", "#FechaVencHasta", "Fecha de Vencimiento");
	});

	$("#FechaGenDesde, #FechaGenHasta").on("blur", function () {
		ValidarFechasClick("#FechaGenDesde", "#FechaGenHasta", "Fecha de Generación");
	});
	$("#chkDesdeHastaVenc").on("click", function () {
		if ($("#chkDesdeHastaVenc").is(":checked")) {
			$("#FechaVencDesde").prop("disabled", false);
			$("#FechaVencHasta").prop("disabled", false);
			$("#FechaVencDesde").trigger("focus");
		}
		else {
			$("#FechaVencDesde").prop("disabled", true);
			$("#FechaVencHasta").prop("disabled", true);
		}
	});
	$("#chkDesdeHastaGen").on("click", function () {
		if ($("#chkDesdeHastaGen").is(":checked")) {
			$("#FechaGenDesde").prop("disabled", false);
			$("#FechaGenHasta").prop("disabled", false);
			$("#FechaGenDesde").trigger("focus");
		}
		else {
			$("#FechaGenDesde").prop("disabled", true);
			$("#FechaGenHasta").prop("disabled", true);
		}
	});
	$("#chkTipoClientes").on("click", function () {
		if ($("#chkTipoClientes").is(":checked")) {
			$("#listaTipoClientes").prop("disabled", false);
			$("#TipoClientesList").prop("disabled", false);
			$("#listaTipoClientes").trigger("focus");
		}
		else {
			$("#listaTipoClientes").prop("disabled", true);
			$("#TipoClientesList").prop("disabled", true);
			$("#listaTipoClientes").val("");
			$("#TipoClientesList").empty();
		}
	});
	$("#chkTipoProveedores").on("click", function () {
		if ($("#chkTipoProveedores").is(":checked")) {
			$("#listaTipoProveedores").prop("disabled", false);
			$("#TipoProveedoresList").prop("disabled", false);
			$("#listaTipoProveedores").trigger("focus");
		}
		else {
			$("#listaTipoProveedores").prop("disabled", true);
			$("#TipoProveedoresList").prop("disabled", true);
			$("#listaTipoProveedores").val("");
			$("#TipoProveedoresList").empty();
		}
	});
	$("#chkTipoComptes").on("click", function () {
		if ($("#chkTipoComptes").is(":checked")) {
			$("#listaTipoComptes").prop("disabled", false);
			$("#TipoComptesList").prop("disabled", false);
			$("#listaTipoComptes").trigger("focus");
		}
		else {
			$("#listaTipoComptes").prop("disabled", true);
			$("#TipoComptesList").prop("disabled", true);
			$("#listaTipoComptes").val("");
			$("#TipoComptesList").empty();
		}
	});

	$("#lbChkDesdeHastaVenc").text("Fecha de Vencimiento");
	$("#lbChkDesdeHastaGen").text("Fecha de Generación");
	$("#lbTipoClientes").text("Clientes");
	$("#lbTipoProveedores").text("Proveedores");
	$("#lbTipoComptes").text("Tipo Comprobantes");

	$("#divFiltros").collapse("show");
	$("#divDetalle").collapse("hide");
}

function ValidarFechasClick(desde, hasta, label) {
	const fdesde = $(desde).val();
	const fhasta = $(hasta).val();

	if (fdesde && fhasta && fdesde > fhasta) {
		AbrirMensaje("ATENCIÓN", `(${label})El valor de Fecha Desde no puede ser mayor a Fecha Hasta, revise.`, function () {
			$("#msjModal").modal("hide");
			$(desde).val($(hasta).val());
			return true;
		}, false, ["Aceptar"], "error!", null);
	}
}