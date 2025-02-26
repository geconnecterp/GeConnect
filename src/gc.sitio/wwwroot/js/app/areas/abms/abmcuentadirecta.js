$(function () {
	$("#pagEstado").on("change", function () {
		var div = $("#divPaginacion");
		presentaPaginacion(div);
	});

	$(document).on("dblclick", "#" + Grids.GridCuentaDirecta + " tbody tr", function () {
		x = $(this);
		ejecutaDblClickGrid(x, Grids.GridCuentaDirecta);
	});

	/*ABM Botones*/
	$("#btnAbmNuevo").on("click", function () { btnNuevoClick(); });
	$("#btnAbmModif").on("click", function () { btnModiClick(); });
	$("#btnAbmElimi").on("click", function () { btnBajaClick(); });
	$("#btnAbmAceptar").on("click", function () { btnSubmitClick(); });
	$("#btnAbmCancelar").on("click", function () { btnCancelClick(); });

	$("#btnDetalle").on("mousedown", analizaEstadoBtnDetalle);
	$("#btnDetalle").prop("disabled", true);
	$("#btnAbmAceptar").hide();
	$("#btnAbmCancelar").hide();
	$("#btnCancel").on("click", function () {
		$("#btnFiltro").trigger("click");
	});

	$("#btnBuscar").on("click", function () {
		dataBak = "";
		pagina = 1;
		buscarCuentasDirectas(pagina);
	});

	InicializaPantallaAbmCuentaDirecta();
	funcCallBack = buscarCuentasDirectas;
	return true;
});

function InicializaPantallaAbmCuentaDirecta() {
	var tb = $("#tbGridCuentaDirecta tbody tr");
	if (tb.length === 0) {
		$("#divFiltro").collapse("show")
	}

	$("#lbRel01").text("Tipo");
	$("#lbChkDescr").text("Denominación");
	$("#lbDescr").html("Desc");

	$("#lbChkDesdeHasta").text("ID Cuenta");

	$("#IdSelected").val("");
	$(".activable").prop("disabled", true);
	activarBotones(false);
	CerrarWaiting();
	return true;
}

function analizaEstadoBtnDetalle() {
	var res = $("#divDetalle").hasClass("show");
	if (res === true) {
		selectRegCli(regSelected, Grids.GridCuentaDirecta);
	}
	return true;

}

function buscarCuentasDirectas(pag, esBaja = false) {
	AbrirWaiting();
	var buscar = "";
	var id = "";
	var id2 = "";
	var r01 = [];

	if ($("#chkDescr").is(":checked")) {
		buscar = $("#Buscar").val();
	}
	if ($("#chkDesdeHasta").is(":checked")) {
		id = $("#Id").val();
		id2 = $("#Id2").val();
	}
	if ($("#chkRel01").is(":checked")) {
		$("#Rel01List").children().each(function (i, item) { r01.push($(item).val()) });
	}

	var data1 = {
		id, id2,
		rel01: r01,
		rel02: [],
		rel03: [],
		"fechaD": null, //"0001-01-01T00:00:00",
		"fechaH": null, //"0001-01-01T00:00:00",
		buscar
	};

	var buscaNew = JSON.stringify(dataBak) != JSON.stringify(data1)
	if (esBaja)
		buscaNew = true;

	if (buscaNew === false) {
		//son iguales las condiciones cambia de pagina
		pagina = pag;
	}
	else {
		dataBak = data1;
		pagina = 1;
		pag = 1;
	}

	var sort = null;
	var sortDir = null

	var data2 = { sort, sortDir, pag, buscaNew }

	var data = $.extend({}, data1, data2);

	PostGenHtml(data, buscarUrl, function (obj) {
		$("#divGrilla").html(obj);
		$("#divFiltro").collapse("hide")
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
				$(".activable").prop("disabled", true);
			}

		});
		CerrarWaiting();
	}, function (obj) {
		ControlaMensajeError(obj.message);
		CerrarWaiting();
	});
}

function NuevaCuentaDirecta() {
	var data = {};
	PostGenHtml(data, nuevaCuentaDirectaUrl, function (obj) {
		$("#divDatosCuentaDirecta").html(obj);
		$(".nav-link").prop("disabled", true);
		$(".activable").prop("disabled", false);
		$("#CuentaDirecta_Ctag_Id").prop("disabled", false);
		desactivarGrilla(Grids.GridCuentaDirecta);
		accionBotones(AbmAction.ALTA, Tabs.TabCuentaDirecta);
		$("#divFiltro").collapse("hide");
		$("#divDetalle").collapse("show");
		$("#CuentaDirecta_Ctag_Denominacion").focus();
		CerrarWaiting();
	}, function (obj) {
		ControlaMensajeError(obj.message);
		CerrarWaiting();
	});
}

function selectRegDbl(x, gridId) {
	AbrirWaiting("Espere mientras se busca el elemento seleccionado...");
	$("#" + gridId + " tbody tr").each(function (index) {
		$(this).removeClass("selectedEdit-row");
	});
	$(x).addClass("selectedEdit-row");
	regSelected = x;

	switch (gridId) {
		case Grids.GridCuentaDirecta:
			var ctag_id = x[0].cells[0].innerText.trim();
			var tcg_id = x[0].cells[3].innerText.trim();
			if (ctag_id !== "") {
				ctagIdRow = x[0];
				ctagId = ctag_id;
				BuscarCuentaDirecta(ctag_id, tcg_id);
				/*ActualizarTitulo();*/
				activarBotones(true);
				$(".activable").prop("disabled", true);
				$("#btnDetalle").prop("disabled", false);
				$("#divFiltro").collapse("hide");
				$("#divDetalle").collapse("show");
				$("#IdSelected").val(ctagId);
				posicionarRegOnTop(x);
			}
			break;
		default:
			break;
	}
}

function BuscarCuentaDirecta(ctagId, tcgId) {
	var data = { ctagId, tcgId };
	AbrirWaiting();
	PostGenHtml(data, buscarCuentaDirectaUrl, function (obj) {
		$("#divDatosCuentaDirecta").html(obj);
		$("#IdSelected").val($("#CuentaDirecta_Ctag_Id").val());
		$(".activable").prop("disabled", true);
		CerrarWaiting();
	}, function (obj) {
		ControlaMensajeError(obj.message);
		CerrarWaiting();
	});
}