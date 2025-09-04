$(function () {
	$("#chkCuentaBanco").prop('checked', true);
	$("#chkCuentaBanco").trigger("change");
	$("#chkCuentaBanco").prop("disabled", true);
	$("#listaCuentaBanco").prop("disabled", false);

	$(document).on("change", "#btnCancelar", btnCancelarClick);
	$(document).on("change", "#listaCuentaBanco", ControlalistaCuentaBancoSelected);

	$("#CuentaBancoList").on("dblclick", 'option', function () { $(this).remove(); })

	$("#btnBuscar").on("click", function () {
		ctafIdSelected = $("#listaCuentaBanco").val();
		ctafDenominacionSelected = $("#listaCuentaBanco option:selected").text();
		PosicionarseEnTabVencimientoChequeEmitido();
	});

	$("#btnFiltro").on("click", function () {
		if ($("#divFiltros").hasClass("show")) {
			$("#divFiltros").collapse("hide");
		}
		else {
			$("#divFiltros").collapse("show");
		}
	});

	$('#tabsFinancieroBancos button[data-bs-toggle="tab"]').on('shown.bs.tab', function (event) {
		const tabId = $(event.target).attr('id'); // ID del botón clickeado
		const targetPane = $(event.target).data('bsTarget'); // Ej: #navs-top-home

		console.log('Tab activado:', tabId);
		console.log('Contenido mostrado:', targetPane);

		// Ejemplo: lógica condicional
		switch (tabId) {
			case 'btnTabVencimientoChequeEmitido':
				console.log("btnTabVencimientoChequeEmitido");
				break;
			case 'btnTabLibroBancoDetalle':
				console.log("btnTabLibroBancoDetalle");
				break;
			case 'btnTabLibroBancoResumen':
				console.log("btnTabLibroBancoResumen");
				break;
			case 'btnTabHistoricoLibro':
				console.log("btnTabHistoricoLibro");
				break;
			case 'btnTabExtractoBancario':
				console.log("btnTabExtractoBancario");
				SetearCamposExtractoBancario()
				break;
			// etc...
		}
	});

});

const tabs = {
	cheques: 'btnTabVencimientoChequeEmitido',
	libroDetalle: 'btnTabLibroBancoDetalle',
	libroResumen: 'btnTabLibroBancoResumen',
	historico: 'btnTabHistoricoLibro',
	extracto: 'btnTabExtractoBancario'
};

function SetearCamposExtractoBancario() {
	var now = moment().format('yyyy-MM-DD');
	var now2 = moment().subtract(30, 'days');
	$("#fechaDesdeExtractoBancario").val(now2.format('yyyy-MM-DD'));
	$("#fechaHastaExtractoBancario").val(now);
}
//const tab = bootstrap.Tab.getOrCreateInstance($('#btnTabVencimientoChequeEmitido')[0]);
//tab.show();
function ActivarTabPorId(idBotonTab) {
	if (!idBotonTab) return;

	const botonTab = document.getElementById(idBotonTab);
	if (!botonTab) {
		console.warn(`No se encontró el botón con ID: ${idBotonTab}`);
		return;
	}

	const instanciaTab = bootstrap.Tab.getOrCreateInstance(botonTab);
	instanciaTab.show();
}

function PosicionarseEnTabExtractoBancario() {
	AbrirWaiting();
	var data = {};
	PostGenHtml(data, posicionarseEnTabExtractoBancarioURL, function (obj) {
		CerrarWaiting();
		$("#divVencimientoChequeEmitido").html(obj);
		$("#CtafId").val(ctafIdSelected);
		$("#CtafDesc").val(ctafDenominacionSelected);
		$("#divFiltros").removeClass("show").addClass("collapse");
		$("#divDetalle").collapse("show");
		ActivarTabPorId(tabs.cheques);
		$("#btnCancelar").on("click", function () {
			btnCancelarClick();
		});

		CerrarWaiting();
		return true
	}, function (obj) {
		ControlaMensajeError(obj.message);
		CerrarWaiting();
	});
}


function PosicionarseEnTabVencimientoChequeEmitido() {
	AbrirWaiting();
	var data = {};
	PostGenHtml(data, posicionarseEnTabVencimientoChequeEmitidoURL, function (obj) {
		CerrarWaiting();
		$("#divVencimientoChequeEmitido").html(obj);
		$("#CtafId").val(ctafIdSelected);
		$("#CtafDesc").val(ctafDenominacionSelected);
		$("#divFiltros").removeClass("show").addClass("collapse");
		$("#divDetalle").collapse("show");
		ActivarTabPorId(tabs.cheques);
		$("#btnCancelar").on("click", function () {
			btnCancelarClick();
		});
		
		CerrarWaiting();
		return true
	}, function (obj) {
		ControlaMensajeError(obj.message);
		CerrarWaiting();
	});
}

function ControlalistaCuentaBancoSelected() {
	var item = $("#listaCuentaBanco").val();
	var desc = $("#listaCuentaBanco option:selected").text();
	$("#CuentaBancoList").empty();
	var opc = "<option value=" + item + ">" + desc + "</option>"
	$("#CuentaBancoList").append(opc);
}

function btnCancelarClick() {
	$("#divFiltros").removeClass("collapse").addClass("show");
	$("#divDetalle").collapse("hide");
	$("#chkCFO").prop('checked', false);
	$("#chkCFO").trigger("change");
	$("#chkCFD").prop('checked', false);
	$("#chkCFD").trigger("change");
	$("#chkTT").prop('checked', false);
	$("#chkTT").trigger("change");
	$("#chkUsu").prop('checked', false);
	$("#chkUsu").trigger("change");
	$("#CFOList").empty();
	$("#CFDList").empty();
	$("#TTList").empty();
	$("#UsuList").empty();
	$("#listaCFO").val("");
	$("#listaCFD").val();
	$("#listaTT").val();
	$("#listaUsu").val();
	$("#listaCFO").prop("disabled", true);
	$("#listaCFD").prop("disabled", true);
	$("#listaTT").prop("disabled", true);
	$("#listaUsu").prop("disabled", true);
	$("#CFOList").prop("disabled", true);
	$("#CFDList").prop("disabled", true);
	$("#TTList").prop("disabled", true);
	$("#UsuList").prop("disabled", true);
	$("#btnCancel").on("click", function () {
		btnCancelarClick();
	});
	InicializarDatosEnSesion();
}