$(function () {
	$("#chkCuentaBanco").prop('checked', true);
	$("#chkCuentaBanco").trigger("change");
	$("#chkCuentaBanco").prop("disabled", true);
	$("#listaCuentaBanco").prop("disabled", false);

	$(document).on("change", "#btnCancelar", btnCancelarClick);
	$(document).on("change", "#listaCuentaBanco", ControlalistaCuentaBancoSelected);
	$(document).on("click", "#btnBuscarExtractoBancario", ControlaBuscarExtractoBancarioClick);
	$(document).on("click", "#btnImprimirExtractoBancario", ControlaImpimirExtractoBancarioClick);
	$(document).on("click", "#btnBuscarHistoricoLibro", ControlaBuscarHistoricoLibroClick);
	$(document).on("click", "#btnImprimirHistoricoLibro", ControlaImpimirHistoricoLibroClick);
	//

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
				SetearCamposHistoricoLibro();
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

function ReseteoDeReportes() {
	console.log("Reseto de reportes");
	ReporteResetArre();
}

function ControlaImpimirHistoricoLibroClick() {
	var filas = $("#tbGridHistoricoLibro tbody tr").length;
	if (filas == 0) {
		AbrirMensaje("ATENCIÓN", "No hay datos para imprimir.", function () {
			$("#msjModal").modal("hide");
			return true;
		}, false, ["Aceptar"], "error!", null);
	}
	else {
		AbrirWaiting("Imprimiendo listado...");
		var tipoReporte = 4;
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
				ImpimirHistoricoLibro();
			}
		});
	}
}

function ControlaImpimirExtractoBancarioClick() {
	var filas = $("#tbGridExtractoBancario tbody tr").length;
	if (filas == 0) {
		AbrirMensaje("ATENCIÓN", "No hay datos para imprimir.", function () {
			$("#msjModal").modal("hide");
			return true;
		}, false, ["Aceptar"], "error!", null);
	}
	else {
		AbrirWaiting("Imprimiendo listado...");
		var tipoReporte = 5;
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
				ImpimirExtractoBancario();
			}
		});
	}
}

function ImpimirHistoricoLibro() {
	ReseteoDeReportes();
	setTimeout(() => {
		var desde = $("#fechaDesdeHistoricoLibro").val();
		var hasta = $("#fechaHastaHistoricoLibro").val();
		var Date1Print = moment($("#fechaDesdeHistoricoLibro").val()).format('DD/MM/yyyy')
		var Date2Print = moment($("#fechaHastaHistoricoLibro").val()).format('DD/MM/yyyy')
		var ctaf_id = ctafIdSelected;
		const tipo_filtro = $('input[name="tipoFiltro"]:checked').val();
		let data = { desde, hasta, ctaf_id, tipo_filtro, Date1Print, Date2Print };
		cargarReporteEnArre(30, data, "HISTORICO LIBRO BANCO", "", "");
		invocacionGestorDoc({});
	}, 500);
}

function ImpimirExtractoBancario() {
	ReseteoDeReportes();
	setTimeout(() => {
		var desde = $("#fechaDesdeExtractoBancario").val();
		var hasta = $("#fechaHastaExtractoBancario").val();
		var Date1Print = moment($("#fechaDesdeExtractoBancario").val()).format('DD/MM/yyyy')
		var Date2Print = moment($("#fechaHastaExtractoBancario").val()).format('DD/MM/yyyy')
		var ctaf_id = ctafIdSelected;
		let data = { desde, hasta, ctaf_id, Date1Print, Date2Print };
		cargarReporteEnArre(31, data, "EXTRACTO BANCARIO", "", "");
		invocacionGestorDoc({});
	}, 500);
}

function ControlaBuscarHistoricoLibroClick() {
	AbrirWaiting();
	var ctaf_id = ctafIdSelected;
	var FechaDesde = $("#fechaDesdeHistoricoLibro").val();
	var FechaHasta = $("#fechaHastaHistoricoLibro").val();
	const tipo_filtro = $('input[name="tipoFiltro"]:checked').val();
	var data = { ctaf_id, FechaDesde, FechaHasta, tipo_filtro };
	PostGenHtml(data, buscarHistoricoLibroURL, function (obj) {
		CerrarWaiting();
		$("#divHistoricoLibro").html(obj);
		return true
	}, function (obj) {
		ControlaMensajeError(obj.message);
		CerrarWaiting();
	});
}

function ControlaBuscarExtractoBancarioClick() {
	AbrirWaiting();
	var ctaf_id = $("#listaCuentaBanco").val();
	var FechaDesde = $("#fechaDesdeExtractoBancario").val();
	var FechaHasta = $("#fechaHastaExtractoBancario").val();
	var data = { ctaf_id, FechaDesde, FechaHasta };
	PostGenHtml(data, buscarExtractoBancarioURL, function (obj) {
		CerrarWaiting();
		$("#divExtractoBancario").html(obj);
		return true
	}, function (obj) {
		ControlaMensajeError(obj.message);
		CerrarWaiting();
	});
}

function SetearCamposHistoricoLibro() {
	var now = moment().format('yyyy-MM-DD');
	var now2 = moment().subtract(30, 'days');
	$("#fechaDesdeHistoricoLibro").val(now2.format('yyyy-MM-DD'));
	$("#fechaHastaHistoricoLibro").val(now);
}

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