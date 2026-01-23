$(function () {
	$("#chkCuentaBanco").prop('checked', true);
	$("#chkCuentaBanco").trigger("change");
	$("#chkCuentaBanco").prop("disabled", true);
	$("#listaCuentaBanco").prop("disabled", false);

	$(document).on("change", "#btnCancelar", btnCancelarClick);
	$(document).on("change", "#listaCuentaBanco", ControlalistaCuentaBancoSelected);
	$(document).on("click", "#btnBuscarVencChequeEmitido", ControlaBuscarVencChequeEmitidoClick);
	$(document).on("click", "#btnImprimirVencChequeEmitido", ControlaImprimirVencChequeEmitidoClick);
	$(document).on("click", "#btnBuscarExtractoBancario", ControlaBuscarExtractoBancarioClick);
	$(document).on("click", "#btnImprimirExtractoBancario", ControlaImpimirExtractoBancarioClick);
	$(document).on("click", "#btnBuscarHistoricoLibro", ControlaBuscarHistoricoLibroClick);
	$(document).on("click", "#btnImprimirHistoricoLibro", ControlaImpimirHistoricoLibroClick);
	$(document).on("click", "#btnBuscarLibroBancoResumen", ControlaBuscarLibroBancoResumenClick);
	$(document).on("click", "#btnImprimirLibroBancoResumen", ControlaImprimirLibroBancoResumenClick);
	$(document).on("click", "#btnBuscarLibroBancoDetalle", ControlaBuscarLibroBancoDetalleClick);
	$(document).on("click", "#btnImprimirLibroBancoDetalle", ControlaImprimirLibroBancoDetalleClick);
	$(document).on("click", "#btnCancel", btnCancelarClick);

	$("#CuentaBancoList").on("dblclick", 'option', function () { $(this).remove(); })

	$("#btnBuscar").on("click", function () {
		ctafIdSelected = $("#listaCuentaBanco").val();
		if (ctafIdSelected == "") {
			AbrirMensaje("ATENCIÓN", "Debe seleccionar una cuenta banco.", function () {
				$("#msjModal").modal("hide");
				$("#listaCuentaBanco").trigger("focus");
				return true;
			}, false, ["Aceptar"], "error!", null);
		}
		else {
			ctafDenominacionSelected = $("#listaCuentaBanco option:selected").text();
			LimpiarDivs();
			PosicionarseEnTabVencimientoChequeEmitido();
		}
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
				SetearCamposVencChequeEmitido();
				break;
			case 'btnTabLibroBancoDetalle':
				console.log("btnTabLibroBancoDetalle");
				SetearCamposLibroBancoDetalle();
				break;
			case 'btnTabLibroBancoResumen':
				console.log("btnTabLibroBancoResumen");
				SetearCamposLibroBancoResumen();
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

function LimpiarDivs() {
	$("#divExtractoBancario").empty();
	$("#divHistoricoLibro").empty();
	$("#divLibroBancoResumen").empty();
	$("#divLibroBancoDetalle").empty();
	$("#divVencimientoChequeEmitido").empty();
}

function ControlaImprimirLibroBancoDetalleClick() {
	var filasGrillaCero = $("#tabGrillaCero tbody tr").length;
	var filasGrillaUno = $("#tabGrillaUno tbody tr").length;
	var filasGrillaDos = $("#tabGrillaDos tbody tr").length;
	if (filasGrillaCero == 0 && filasGrillaUno == 0 && filasGrillaDos == 0) {
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
				ImpimirLibroBancoDetalleClick();
			}
		});
	}
}


function selectReg(x, gridId) {
	$("#" + gridId + " tbody tr").each(function (index) {
		$(this).removeClass("selected-row");
		$(this).removeClass("selectedEdit-row");
	});
	$(x).addClass("selected-row");
	if (gridId == "tbGridCheques") {
		console.log(x.childNodes);
		var ctaf_id = ctafIdSelected;
		var desde = x.childNodes[1].innerText;
		var hasta = x.childNodes[1].innerText;
		var data = { ctaf_id, desde, hasta };
		PostGenHtml(data, buscarVencimientoChequeEmitidoListaURL, function (obj) {
			$("#divListaCheques").html(obj);
			return true
		}, function (obj) {
			ControlaMensajeError(obj.message);
		});
	}
}

function ControlaBuscarVencChequeEmitidoClick() {
	AbrirWaiting();
	var ctaf_id = ctafIdSelected;
	var desde = $("#fechaDesdeVencChequeEmitido").val();
	var hasta = $("#fechaHastaVencChequeEmitido").val();
	var data = { ctaf_id, desde, hasta };
	PostGenHtml(data, buscarVencimientoChequeEmitidoURL, function (obj) {
		CerrarWaiting();
		$("#divVencimientoChequeEmitido").html(obj);
		var filas = $("#tbGridCheques tbody tr").length;
		if (filas == 0) {
			AbrirMensaje("ATENCIÓN", "No hay datos de Vencimiento de Cheques Emitidos.", function () {
				$("#msjModal").modal("hide");
				return true;
			}, false, ["Aceptar"], "error!", null);
		}
		else {
			// Seleccionar la primera fila y disparar el click
			var primera = $("#tbGridCheques tbody tr").first();
			primera.trigger("click");
			return true
		}
	}, function (obj) {
		ControlaMensajeError(obj.message);
		CerrarWaiting();
	});
}

function ControlaBuscarLibroBancoDetalleClick() {
	AbrirWaiting();
	var ctaf_id = ctafIdSelected;
	var hasta = $("#fechaHastaLibroBancoDetalle").val();
	var data = { ctaf_id, hasta };
	PostGenHtml(data, obtenerLibroDetalleURL, function (obj) {
		CerrarWaiting();
		$("#divLibroBancoDetalle").html(obj);
		return true
	}, function (obj) {
		ControlaMensajeError(obj.message);
		CerrarWaiting();
	});
}

function ControlaImprimirVencChequeEmitidoClick() {
	var filas = $("#tbGridCheques tbody tr").length;
	if (filas == 0) {
		AbrirMensaje("ATENCIÓN", "No hay datos para imprimir.", function () {
			$("#msjModal").modal("hide");
			return true;
		}, false, ["Aceptar"], "error!", null);
	}
	else {
		AbrirWaiting("Imprimiendo listado...");
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
				CerrarWaiting();
				ImpimirVencChequeEmitidoClick();
			}
		});
	}
}

function ControlaImprimirLibroBancoResumenClick() {
	var filas = $("#tbGridCuentaFin tbody tr").length;
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
				ImpimirLibroBancoResumen();
			}
		});
	}
}

function ImpimirLibroBancoDetalleClick() {
	ReseteoDeReportes();
	setTimeout(() => {
		var hasta = $("#fechaHastaLibroBancoDetalle").val();
		var ctaf_id = ctafIdSelected;
		var ctaf_desc = ctafDenominacionSelected;
		var Date1Print = moment($("#fechaHastaLibroBancoDetalle").val()).format('DD/MM/yyyy')
		var data = { hasta, ctaf_id, ctaf_desc, Date1Print };
		cargarReporteEnArre(28, data, "LIBRO BANCO DETALLE", "", "");
		invocacionGestorDoc({});
	}, 500);
}

function ImpimirVencChequeEmitidoClick() {
	ReseteoDeReportes();
	setTimeout(() => {
		var desde = $("#fechaDesdeVencChequeEmitido").val();
		var hasta = $("#fechaHastaVencChequeEmitido").val();
		var ctaf_id = ctafIdSelected;
		var ctaf_desc = ctafDenominacionSelected;
		var tipo_fecha = "V";
		var Date1Print = moment($("#fechaDesdeVencChequeEmitido").val()).format('DD/MM/yyyy')
		var Date2Print = moment($("#fechaHastaVencChequeEmitido").val()).format('DD/MM/yyyy')
		var data = { desde, hasta, ctaf_id, ctaf_desc, tipo_fecha, Date1Print, Date2Print };
		cargarReporteEnArre(27, data, "VENCIMIENTO DE CHEQUES EMITIDOS", "", "");
		invocacionGestorDoc({});
	}, 500);
}

function ImpimirLibroBancoResumen() {
	ReseteoDeReportes();
	setTimeout(() => {
		var hasta = $("#fechaHastaLibroBancoResumen").val();
		var ctaf_id = ctafIdSelected;
		var Date1Print = moment($("#fechaHastaLibroBancoResumen").val()).format('DD/MM/yyyy')
		var data = { hasta, ctaf_id, Date1Print };
		cargarReporteEnArre(29, data, "LIBRO BANCO RESUMEN", "", "");
		invocacionGestorDoc({});
	}, 500);
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

function ControlaBuscarLibroBancoResumenClick() {
	AbrirWaiting();
	var ctaf_id = ctafIdSelected;
	var hasta = $("#fechaHastaLibroBancoResumen").val();
	var data = { ctaf_id, hasta };
	PostGenHtml(data, obtenerLibroResumenURL, function (obj) {
		CerrarWaiting();
		$("#divLibroBancoResumen").html(obj);
		var filas = $("#tabGrillaUno tbody tr").length;
		if (filas === 0) {
			$("#containerGrillaUno").addClass("container-auto");
		} else {
			$("#containerGrillaUno").removeClass("container-auto");
		}
		filas = $("#tabGrillaDos tbody tr").length;
		if (filas === 0) {
			$("#containerGrillaDos").addClass("container-auto");
		} else {
			$("#containerGrillaDos").removeClass("container-auto");
		}
		return true
	}, function (obj) {
		ControlaMensajeError(obj.message);
		CerrarWaiting();
	});
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
		var filas = $("#tbGridHistoricoLibro tbody tr").length;
		if (filas === 0) {
			$("#containerGridHistoricoLibro").addClass("container-auto");
		} else {
			$("#containerGridHistoricoLibro").removeClass("container-auto");
		}
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
		var filas = $("#tbGridExtractoBancario tbody tr").length;
		if (filas === 0) {
			$("#containerGridExtractoBancario").addClass("container-auto");
		} else {
			$("#containerGridExtractoBancario").removeClass("container-auto");
		}
		return true
	}, function (obj) {
		ControlaMensajeError(obj.message);
		CerrarWaiting();
	});
}

function SetearCamposLibroBancoResumen(){
	var now = moment().format('yyyy-MM-DD');
	$("#fechaHastaLibroBancoResumen").val(now);
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

function SetearCamposLibroBancoDetalle() {
	var now = moment().format('yyyy-MM-DD');
	$("#fechaHastaLibroBancoDetalle").val(now);
}

function SetearCamposVencChequeEmitido() {
	var now = moment().format('yyyy-MM-DD');
	var now2 = moment().subtract(30, 'days');
	var now3 = moment().add(120, 'days');
	$("#fechaDesdeVencChequeEmitido").val(now2.format('yyyy-MM-DD'));
	$("#fechaHastaVencChequeEmitido").val(now3.format('yyyy-MM-DD'));
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
		//$("#divVencimientoChequeEmitido").html(obj);
		$("#CtafId").val(ctafIdSelected);
		$("#CtafDesc").val(ctafDenominacionSelected);
		$("#divFiltros").removeClass("show").addClass("collapse");
		$("#divDetalle").collapse("show");
		ActivarTabPorId(tabs.cheques);
		$("#btnCancelar").on("click", function () {
			btnCancelarClick();
		});
		SetearCamposVencChequeEmitido();
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
	$("#CuentaBancoList").empty();
	$("#listaCuentaBanco").val("");
	$("#btnCancel").on("click", function () {
		btnCancelarClick();
	});
}

const formatter = new Intl.NumberFormat('de-DE', {
	minimumFractionDigits: 2,
	maximumFractionDigits: 2
});

function FormatearValores(grilla, idx) {
	$(grilla).find('tr').each(function (i, el) {
		var td = $(this).find('td');
		if (td.length > 0) {
			for (var i = 0; i < idx.length; i++) {
				if (td[idx[i]].innerText !== undefined) {
					td[idx[i]].innerText = formatter.format(td[idx[i]].innerText);
				}
			}
		}
	});
}