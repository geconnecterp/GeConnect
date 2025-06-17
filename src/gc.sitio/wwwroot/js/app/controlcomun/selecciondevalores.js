/**
 * Seleccion de Valores
 * 
 * Este módulo se encarga de la seleccion de valores financieros
 */

$(function () {
	var tcf_id_selected = "";
	var importe = 0;
	var valor_a_nombre_de = "";
	var valores = [];
	/* 	propiedades locales del componente para luego ser usadas y enviarse para almacenar los items*/
	var ctaf_id = "";
	var ctaf_denominacion = "";
	var ban_razon_social = "";
	/**/

	$(document).on("click", "#btnAceptarAgregarValor", btnAceptarAgregarValorValidar);
	$(document).on("click", "#btnCancelarAgregarValor", btnCancelarAgregarValorValidar);
	$(document).on("keyup", "#txtNroTransferencia", ControlaKeyUpNroTransferencia);
	$(document).on("focusout", "#txtNroTransferencia", ControlaFocusOutNroTransferencia);
	$(document).on("keyup", "#txtNroCheque", ControlaKeyUpNroCheque);
	$(document).on("focusout", "#txtNroCheque", ControlaFocusOutNroCheque);
	$(document).on("click", "#chkAutomatico", ControlachkAutomatico);
	$(document).on("keyup", "#Fecha", ControlaKeyUpFecha);
	$(document).on("keyup", "#Concepto", ControlaKeyUpConcepto);
	$(document).on("keyup", "#ANombreDe", ControlaKeyUpANombreDe);
	$(document).on("keyup", "#Importe", ControlaKeyUpImporte);
	//
});

function ControlaFocusOutNroTransferencia() {
	var aux = $("#txtNroTransferencia").inputmask('unmaskedvalue').padStart(10, '0');
	$("#txtNroTransferencia").val(aux);
	$("#Fecha").trigger("focus");
}

function ControlaFocusOutNroCheque() {
	var aux = $("#txtNroCheque").inputmask('unmaskedvalue').padStart(6, '0');
	$("#txtNroCheque").val(aux);
	$("#Fecha").trigger("focus");
}

function ControlaKeyUpImporte(e) {
	if (e.which == 13 || e.which == 109) {
		btnAceptarAgregarValorValidar();
	}
}

function ControlaKeyUpANombreDe(e) {
	if (e.which == 13 || e.which == 109) {
		$("#Importe").trigger("focus");
	}
}

function ControlaKeyUpConcepto(e) {
	if (e.which == 13 || e.which == 109) {
		$("#Importe").trigger("focus");
	}
}

function ControlaKeyUpFecha(e) {
	if (e.which == 13 || e.which == 109) {
		switch (tcf_id_selected) {
			case "BA"://Transferencias
				$("#Concepto").trigger("focus");
				break;
			case "EC"://Emision de cheques
				$("#ANombreDe").trigger("focus");
				break;
			case "EF": //Efectivo
				break;
			default:
		}
	}
}

function ControlachkAutomatico() {
	if ($(this).is(":checked")) {
		$("#txtNroCheque").prop("disabled", true);
		$("#txtNroCheque").val("000000");
	}
	else {
		$("#txtNroCheque").prop("disabled", false);
	}
}

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
	if (ValidarAntesDeAgregarUnItemEnValor(tcf_id_selected)) {
		var listaObjValor = [];
		switch (tcf_id_selected) {
			case "BA"://Transferencias
				var newItem = new ObjValor(ctaf_id, ctaf_denominacion, tcf_id_selected, " ", " ", ban_razon_social, "Banco", $("#txtNroTransferencia").inputmask('unmaskedvalue'), "N° Transferencia", " ", " ", $("#Importe").inputmask('unmaskedvalue'), $("#Fecha").val(),
					" ", 0, " ", " ", " ", " ", 0, " ");
				listaObjValor.push(newItem);
				break;
			case "EC"://Emision de cheques
				var automatico = "N";
				if ($("#chkAutomatico")[0].checked)
					automatico = "S";

				var newItem = new ObjValor(ctaf_id, ctaf_denominacion, tcf_id_selected, " ", automatico, ban_razon_social, "Banco", $("#txtNroCheque").inputmask('unmaskedvalue'), "N° Cheque", " ", " ", $("#Importe").inputmask('unmaskedvalue'), $("#Fecha").val(),
					" ", 0, " ", " ", $("#ANombreDe").val(), " ", 0, " ");
				listaObjValor.push(newItem);
				break;
			case "EF": //Efectivo
				var newItem = new ObjValor(ctaf_id, ctaf_denominacion, tcf_id_selected, " ", " ", " ", " ", " ", " ", " ", " ", $("#Importe").inputmask('unmaskedvalue'), $("#Fecha").val(),
					" ", 0, " ", " ", " ", " ", 0, " ");
				listaObjValor.push(newItem);
				break;
			case "CH": //Cartera
				$("#tbValoresEnCartera").find('tr').each(function (i, el) {
					var td = $(this).find('td');
					if (td.eq(0)[0]) {
						if (td.eq(5)[0].children[0].checked) {
							var newItem = new ObjValor(ctaf_id, ctaf_denominacion, tcf_id_selected, " ", " ", td.eq(2).text(), td.eq(6).text(), td.eq(3).text(), td.eq(7).text(), td.eq(8).text(), td.eq(9).text(), td.eq(4).text().replace(',', ''), td.eq(13).text(),
								td.eq(10).text(), td.eq(11).text(), td.eq(12).text(), $("#CtaID").val(), " ", " ", 0, " ");
							listaObjValor.push(newItem);
						}
					}
				});
				break;
			default:
		}
		AgregarValorValidar(listaObjValor, tcf_id_selected);
	}
}

function ValidarAntesDeAgregarUnItemEnValor(tcfIdSelected) {
	var esValido = true;
	var mensaje = "";
	switch (tcf_id_selected) {
		case "BA"://Transferencias
			var nroTransferencia = $("#txtNroTransferencia").inputmask('unmaskedvalue');
			if (nroTransferencia.length < 10) {
				mensaje = "El campo 'Número de Transferencia' tiene un valor no válido. (Ej: '1234567890') - Diez dígitos numéricos";
				esValido = false
				break;
			}
			var importe = $("#Importe").inputmask('unmaskedvalue');
			if (importe <= 0) {
				mensaje = "El campo 'Importe' tiene un valor no válido, debe ser mayor a 0.";
				esValido = false
				break;
			}
			break;
		case "EC"://Emision de cheques
			var nroCheque = $("#txtNroCheque").inputmask('unmaskedvalue');
			if (nroCheque.length < 6) {
				mensaje = "El campo 'Número de Cheque' tiene un valor no válido. (Ej: '123456') - Seis dígitos numéricos";
				esValido = false
				break;
			}
			if ($("#ANombreDe").val() == "") {
				mensaje = "El campo 'A nombre de' tiene un valor no válido.";
				esValido = false
				break;
			}
			var importe = $("#Importe").inputmask('unmaskedvalue');
			if (importe <= 0) {
				mensaje = "El campo 'Importe' tiene un valor no válido, debe ser mayor a 0.";
				esValido = false
				break;
			}
			break;
		case "EF": //Efectivo
			var importe = $("#Importe").inputmask('unmaskedvalue');
			if (importe <= 0) {
				mensaje = "El campo 'Importe' tiene un valor no válido, debe ser mayor a 0.";
				esValido = false
				break;
			}
			break;
		case "CH": //Cartera
			var ids = [];
			$("#tbValoresEnCartera").find('tr').each(function (i, el) {
				var td = $(this).find('td');
				if (td.eq(0)[0]) {
					if (td.eq(5)[0].children[0].checked)
						ids.push(td.eq(2).text());
				}
			});
			if (ids.length <= 0) {
				mensaje = "Debe seleccionar al menos un Valor en Cartera.";
				esValido = false
				break;
			}
			break;
		default:
	}
	if (!esValido) {
		AbrirMensaje("ATENCIÓN", mensaje, function () {
			$("#msjModal").modal("hide");
			return true;
		}, false, ["Aceptar"], "error!", null);
	}
	return esValido;
}

function AgregarValorValidar(dataObjectArray, dataType) {
	var DataObject = dataObjectArray;
	var DataType = dataType;
	var data = { DataObject, DataType };
	PostGen(data, agregarItemAColeccionDeValoresUrl, function (obj) {
		if (obj.error === true) {
			AbrirMensaje("ATENCIÓN", obj.msg, function () {
				$("#msjModal").modal("hide");
				return true;
			}, false, ["Aceptar"], "error!", null);
		}
		else {
			LimpiarDatosEnSeccionEdicion(dataType);
			ControlaMensajeInfo("Se han agegado los valores correctamente.");
		}
	});
}

function LimpiarDatosEnSeccionEdicion(dataType) {
	switch (dataType) {
		case "BA"://Transferencias
			$("#txtNroTransferencia").val("");
			var now = moment().format('yyyy-MM-DD');
			$("#Fecha").val(now);
			$("#Concepto").val("");
			$("#Importe").val(0);
			document.getElementById("txtNroTransferencia").focus();
			break;
		case "EC"://Emision de cheques
			$("#txtNroCheque").val("");
			var now = moment().format('yyyy-MM-DD');
			$("#Fecha").val(now);
			$("#ANombreDe").val("");
			$("#Importe").val(0);
			document.getElementById("txtNroCheque").focus();
			break;
		case "EF": //Efectivo
			$("#Importe").val(0);
			document.getElementById("Importe").focus();
			break;
		default:
	}
}

function btnCancelarAgregarValorValidar() {
	LimpiarDatosEnSeccionEdicion(tcf_id_selected);
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
	importe = p.importe;
	valor_a_nombre_de = p.valor_a_nombre_de;
	valores = p.valores;
	var data = { app, importe, valor_a_nombre_de, valores };
	PostGenHtml(data, abrirComponenteDeSeleccionDeValoresUrl, function (obj) {
		$("#modalSeleccionValores").html(obj);
		$("#modalSeleccionValores").show();
		$("#seleccionDeValoresModal").modal("show");
		$('#seleccionDeValoresModal').on('hidden.bs.modal', function (e) {
			// Disparo evento para actualizar lo que tenga que actualizar en origen
			$("#UpdateValores").val(true).trigger("change");
		});
		$("#btnAceptarAgregarValor").prop("disabled", true);
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
				$("#Importe").val(importe);
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
				$("#ANombreDe").val(valor_a_nombre_de);
				$("#Importe").val(importe);
				break;
			case "EF": //Efectivo
				getMaskForMoneyType("#Importe");
				$("#Importe").val(importe);
				break;
			default:
		}
	});
}

function onChangeFecha(x) {
}

function seleccionarFinancieroDbl(x) {
	$("#btnCancelarAgregarValor").show();
	$(".activable").prop("disabled", false);
	ctaf_id = x.cells[2].innerText.trim();
	ctaf_denominacion = x.cells[0].innerText.trim();
	ban_razon_social = x.cells[4].innerText.trim();
	DarFocoEnBaseAlaSeleccion();
	$("#btnAceptarAgregarValor").prop("disabled", false);
}

function DarFocoEnBaseAlaSeleccion() {
	switch (tcf_id_selected) {
		case "BA"://Transferencias
			document.getElementById("txtNroTransferencia").focus();
			break;
		case "EC"://Emision de cheques
			document.getElementById("txtNroCheque").focus();
			break;
		case "EF": //Efectivo
			document.getElementById("Importe").focus();
			break;
		default:
	}
}

function seleccionarFinanciero(x) {
	if (tcf_id_selected == "CH") {
		ctaf_id = x.cells[2].innerText.trim();
		var data = { ctaf_id };
		PostGenHtml(data, cargarGrillaFinancieroCarteraEnSeleccionDeValoresUrl, function (obj) {
			$("#divSeccionEditable").html(obj);
			if ($("#tbValoresEnCartera tbody tr").length <= 0) {
				ControlaMensajeInfo("Sin resultados para la Cartera seleccionada.");
			}
			AgregarHandlerAHeaderEnGrillaValoresEnCartera();
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

class ObjValor {
	constructor(ctaf_id, ctaf_denominacion, tcf_id, tipo, automatico, op_dato1_valor, op_dato1_desc, op_dato2_valor, op_dato2_desc, op_dato3_valor, op_dato3_desc, op_importe, op_fecha_valor,
		fc_compte, fc_item, fc_dia_movi, fc_cta_id, fc_anombre, concepto_valor, resultado, resultado_msj) {
		this.ctaf_id = ctaf_id;
		this.ctaf_denominacion = ctaf_denominacion;
		this.tcf_id = tcf_id;
		this.tipo = tipo;
		this.automatico = automatico;
		this.op_dato1_valor = op_dato1_valor;
		this.op_dato1_desc = op_dato1_desc;
		this.op_dato2_valor = op_dato2_valor;
		this.op_dato2_desc = op_dato2_desc;
		this.op_dato3_valor = op_dato3_valor;
		this.op_dato3_desc = op_dato3_desc;
		this.op_importe = op_importe;
		this.op_fecha_valor = op_fecha_valor;
		this.fc_compte = fc_compte;
		this.fc_item = fc_item;
		this.fc_dia_movi = fc_dia_movi;
		this.fc_cta_id = fc_cta_id;
		this.fc_anombre = fc_anombre;
		this.concepto_valor = concepto_valor;
		this.resultado = resultado;
		this.resultado_msj = resultado_msj;
	}
};

function AgregarHandlerAHeaderEnGrillaValoresEnCartera() {
	var dataTable = document.getElementById('tbValoresEnCartera');
	var checkItAll = dataTable.querySelector('input[name="select_all"]');
	var inputs = dataTable.querySelectorAll('tbody>tr>td>input');
	checkItAll.addEventListener('change', function () {
		if (checkItAll.checked) {
			inputs.forEach(function (input) {
				input.checked = true;
			});
		}
		else {
			inputs.forEach(function (input) {
				input.checked = false;
			});
		}
	});
}