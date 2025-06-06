﻿$(function () {
	$("#pagEstado").on("change", function () {
		var div = $("#divPaginacion");
		presentaPaginacion(div);
	});

	$(document).on("dblclick", "#" + Grids.GridBanco + " tbody tr", function () {
		x = $(this);
		ejecutaDblClickGrid(x, Grids.GridBanco);
	});

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
		buscarBancos(pagina);
	});

	InicializaPantallaAbmBancos();
	funcCallBack = buscarBancos;
	return true;
});

function InicializaPantallaAbmBancos() {
	var tb = $("#tbGridBanco tbody tr");
	if (tb.length === 0) {
		$("#divFiltro").collapse("show")
	}


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
		selectRegCli(regSelected, Grids.GridBanco);
	}
	return true;

}


function selectRegDbl(x, gridId) {
	AbrirWaiting("Espere mientras se busca la información solicitada...");
	$("#" + gridId + " tbody tr").each(function (index) {
		$(this).removeClass("selectedEdit-row");
	});
	$(x).addClass("selectedEdit-row");
	regSelected = x;

	switch (gridId) {
		case Grids.GridBanco:
			var ctaf_id = x[0].cells[0].innerText.trim();
			if (ctaf_id !== "") {
				ctafId = ctaf_id;
				BuscarBanco(ctaf_id);
				activarBotones(true);
				$(".activable").prop("disabled", true);
				$("#btnDetalle").prop("disabled", false);
				$("#divFiltro").collapse("hide");
				$("#divDetalle").collapse("show");
				$("#IdSelected").val(ctafId);
				posicionarRegOnTop(x);
			}
			break;
		default:
	}
}

function NuevoBanco() {
	var data = {};
	PostGenHtml(data, nuevoBancoUrl, function (obj) {
		$("#divDatosBanco").html(obj);
		$(".nav-link").prop("disabled", true);
		$(".activable").prop("disabled", false);
		$("#Banco_Ctaf_Id").prop("disabled", false);
		desactivarGrilla(Grids.GridBanco);
		accionBotones(AbmAction.ALTA, Tabs.TabBanco);
		$("#divFiltro").collapse("hide");
		$("#divDetalle").collapse("show");
		$("#Banco_Ban_Razon_Social").focus();
		CerrarWaiting();
	}, function (obj) {
		ControlaMensajeError(obj.message);
		CerrarWaiting();
	});
}

function ModificaBanco(tabAct, mainGrid) {
	var mensaje = PuedoModificar(tabAct);
	if (mensaje !== "") {
		AbrirMensaje("ATENCIÓN", mensaje, function () {
			$("#msjModal").modal("hide");
			return true;
		}, false, ["Aceptar"], "error!", null);
	}
	else {
		accionBotones(AbmAction.MODIFICACION, Tabs.TabBanco);
		tipoDeOperacion = AbmAction.MODIFICACION;
		SetearDestinoDeOperacion(tabAct);
		$(".nav-link").prop("disabled", true);
		$(".activable").prop("disabled", false);
		desactivarGrilla(mainGrid);
		$("#Banco_Ban_Razon_Social").focus();
	}
}

function cargaPaginacion() {
	$("#divPaginacion").pagination({
		items: totalRegs,
		itemsOnPage: pagRegs,
		cssStyle: "dark-theme",
		currentPage: pagina,
		onPageClick: function (num) {
			buscarBancos(num);
		}
	});
	$("#pagEstado").val(false);
	$("#divFiltro").collapse("hide")
	return true;
}

function ObtenerDatosDeBancoParaJson(destinoDeOperacion, tipoDeOperacion) {
	var ctaf_id = $("#Banco_Ctaf_Id").val();
	var ban_razon_social = $("#Banco_Ban_Razon_Social").val();
	var ban_cuit = $("#Banco_Ban_Cuit").val();
	var tcb_id = $("#listaTcb").val();
	var tcb_desc = $("#listaTcb option:selected").text();
	var ban_cuenta_nro = $("#Banco_Ban_Cuenta_Nro").val();
	var ban_cuenta_cbu = $("#Banco_Ban_Cuenta_Cbu").val();
	var mon_codigo = $("#listaMoneda").val();
	var mon_desc = $("#listaMoneda option:selected").text();
	var ban_che_nro = $("#Banco_Ban_Che_Nro").val();  
	var ban_che_desde = $("#Banco_Ban_Che_Desde").val();
	var ban_che_hasta = $("#Banco_Ban_Che_Hasta").val();
	var ccb_id = $("#listaCcb").val();
	var ccb_desc = $("#listaCcb option:selected").text();
	var ccb_id_diferido = $("#listaCcbDif").val();
	var ccb_desc_diferido = $("#listaCcbDif option:selected").text();
	var ctag_id = $("#listaCtaGas").val();
	var ctag_denominacion = $("#listaCtaGas option:selected").text();
	var data = {
		ctaf_id, ban_razon_social, ban_cuit, tcb_id, tcb_desc, ban_cuenta_nro, ban_cuenta_cbu, mon_codigo, mon_desc, ban_che_nro, ban_che_desde,
		ban_che_hasta, ccb_id, ccb_desc, ccb_id_diferido, ccb_desc_diferido, ctag_id, ctag_denominacion, destinoDeOperacion, tipoDeOperacion
	}
	return data;
}

function buscarBancos(pag, esBaja = false) {
	AbrirWaiting();
	var buscar = "";
	var id = "";
	var id2 = "";
	var r01 = [];
	var r02 = [];

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
	if ($("#chkRel02").is(":checked")) {
		$("#Rel02List").children().each(function (i, item) { r02.push($(item).val()) });
	}

	var data1 = {
		id, id2,
		rel01: r01,
		rel02: r02,
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

function BuscarBanco(ctafId) {
	var data = { ctafId };
	AbrirWaiting();
	PostGenHtml(data, buscarBancoUrl, function (obj) {
		$("#divDatosBanco").html(obj);
		$("#IdSelected").val($("#Banco_Ctaf_Id").val());
		$(".activable").prop("disabled", true);
		CerrarWaiting();
	}, function (obj) {
		ControlaMensajeError(obj.message);
		CerrarWaiting();
	});
}