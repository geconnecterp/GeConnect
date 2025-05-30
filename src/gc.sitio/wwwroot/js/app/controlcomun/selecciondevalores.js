/**
 * Seleccion de Valores
 * 
 * Este módulo se encarga de la seleccion de valores financieros
 */

$(function () {
	var tcf_id_selected = "";
	$(document).on("click", "#btnAceptarAgregarValor", btnAceptarAgregarValorValidar);
	$(document).on("click", "#btnCancelarAgregarValor", btnCancelarAgregarValorValidar);
	$(document).on("keyup", "#txtNroTransferencia", ControlaKeyUpNroTransferencia);
	$(document).on("keyup", "#txtNroCheque", ControlaKeyUpNroCheque);
});

function ControlaKeyUpNroTransferencia(e) {
	if (e.which == 13 || e.which == 109) {
		var aux = $("#txtNroTransferencia").inputmask('unmaskedvalue').padStart(10, '0');
		$("#txtNroTransferencia").val(aux);
		$("#Fecha").trigger("focus");
	}
}

function ControlaKeyUpNroCheque(e) {
	if (e.which == 13 || e.which == 109) {
		var aux = $("#txtNroCheque").inputmask('unmaskedvalue').padStart(6, '0');
		$("#txtNroCheque").val(aux);
		$("#Fecha").trigger("focus");
	}
}

function btnAceptarAgregarValorValidar() {
	console.log("btnAceptarAgregarValorValidar");
}

function btnCancelarAgregarValorValidar() {
	activarGrilla("tbFinanciero");
	activarGrilla("tbTipoCuentaFin");
	$(".activable").prop("disabled", true);
	$("#btnCancelarAgregarValor").hide();
}
/*
p:
{ app, importe, valor_a_nombre_de, valores }
Donde:
	- app: Identificador del tipo de cuenta financiera.
	- importe: Importe saldo sugerido, este monto si es mayor que cero debe ser sugerido por la app al momento en que se seleccione un valor de una cuenta financiera
	- valor_a_nombre_de: es una ocurrencia string que debemos utilizar cuando se emiten cheques.
	- valores: datos ya cargados en origen, con la idea de hacer algún control.
*/
function invocarModalDeSeleccionDeValores(p) {
	var app = p.app;
	var data = { app };
	PostGenHtml(data, abrirComponenteDeSeleccionDeValoresUrl, function (obj) {
		$("#modalSeleccionValores").html(obj);
		$("#modalSeleccionValores").show();
		$("#seleccionDeValoresModal").modal("show");
	});
}

function seleccionarTipoFin(x) {
	seleccionarGrilla(x, 'tbTipoCuentaFin');
	var tcf_id = x.cells[1].innerText.trim();
	tcf_id_selected = tcf_id;
	var data = { tcf_id };
	PostGenHtml(data, cargarCtaFinParaSeleccionDeValoresUrl, function (obj) {
		$("#divFinancieros").html(obj);
	});
	PostGenHtml(data, cargarSeccionEdicionEnSeleccionDeValoresUrl, function (obj) {
		$("#divSeccionEditable").html(obj);
		switch (tcf_id) {
			case "BA"://Transferencias
				$("#txtNroTransferencia").inputmask("9999999999");
				var now = moment().format('yyyy-MM-DD');
				var min = moment();
				var max = moment().add(1, 'months');
				$("#Fecha").attr('min', min.format('yyyy-MM-DD'));
				$("#Fecha").attr('max', max.format('yyyy-MM-DD'));
				$("#Fecha").val(now);
				getMaskForMoneyType("#Importe");
				break;
			case "EC"://Emision de cheques
				$("#txtNroCheque").inputmask("999999");
				var now = moment().format('yyyy-MM-DD');
				var min = moment();
				var max = moment().add(12, 'months');
				$("#Fecha").attr('min', min.format('yyyy-MM-DD'));
				$("#Fecha").attr('max', max.format('yyyy-MM-DD'));
				$("#Fecha").val(now);
				getMaskForMoneyType("#Importe");
				break;
			case "EF": //Efectivo
				getMaskForMoneyType("#Importe");
				break;
			default:
		}
	});
}

function onChangeFecha(x) {
}

function seleccionarFinancieroDbl(x) {
	desactivarGrilla("tbFinanciero");
	desactivarGrilla("tbTipoCuentaFin");
	$("#btnCancelarAgregarValor").show();
	$(".activable").prop("disabled", false);
}

function seleccionarFinanciero(x) {
	if (tcf_id_selected == "CH") {
		var ctaf_id = x.cells[2].innerText.trim();
		var cta_id = ctaIdSelected;
		var data = { ctaf_id, cta_id };
		console.log(data);
		PostGenHtml(data, cargarGrillaFinancieroCarteraEnSeleccionDeValoresUrl, function (obj) {
			$("#divSeccionEditable").html(obj);
		});
	}
	seleccionarGrilla(x, 'tbFinanciero');
}

function desactivarGrilla(gridId) {
	$("#" + gridId + "").addClass("disable-table-rows");
	$(".table-wrapper").css("overflow", "hidden");
}

function activarGrilla(gridId) {
	$("#" + gridId + "").removeClass("disable-table-rows");
	$(".table-wrapper").css("overflow", "auto");

}

function seleccionarValoresEnCartera(x) {
	seleccionarGrilla(x, 'tbValoresEnCartera');
}

function seleccionarGrilla(x, grilla) {
	$("#" + grilla + " tbody tr").each(function (index) {
		$(this).removeClass("selected-row");
		$(this).removeClass("selectedEdit-row");
	});
	$(x).addClass("selected-row");
}

function getMaskForMoneyType(selector) {
	$(selector).inputmask({
		alias: 'numeric',
		groupSeparator: '.',
		radixPoint: ',',
		digits: 2,
		digitsOptional: false,
		allowMinus: false,
		prefix: '',
		suffix: '',
		rightAlign: true,
		unmaskAsNumber: true
	});
}