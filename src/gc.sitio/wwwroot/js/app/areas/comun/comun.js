/*
////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
INICIO 
MÉTODOS COMUNTES DE MODULOS:
	*) comprobantedecompra.js
	*) odepagodirecta.js
 */

function ControlaGotFocusOtroTributo_alicuota(x) {
	x.target.select()
}

function ControlaGotFocusOtroTributo_base_imp(x) {
	x.target.select()
}

function ControlaKeyUpOtroTributo_base_imp(e) {
	if (e.which == 13 || e.which == 109) {
		$("#OtroTributo_alicuota").trigger("focus");
	}
}

function ControlaKeyUpConceptoFacturado_concepto(e) {
	if (e.which == 13 || e.which == 109) {
		$("#listaIvaSit").trigger("focus");
	}
}

function ControlaKeyUpConceptoFacturado_subtotal(e) {
	if (e.which == 13 || e.which == 109) {
		$("#ConceptoFacturado_iva").trigger("focus");
	}
}

function ControlaGotFocusConceptoFacturado_subtotal(x) {
	x.target.select()
}

function ControlaKeyUpConceptoFacturado_iva(e) {
	if (e.which == 13 || e.which == 109) {
		$("#btnAgregar").trigger("focus");
	}
}

function ControlaGotFocusConceptoFacturado_iva(x) {
	x.target.select()
}

function ActualizarOtroTributo(id, val) {
	var tco_id = tcoIdSelected;
	var data = { id, val, idOtroTributoSeleccionado, tco_id };
	PostGen(data, editarItemEnOtrosConceptosUrl, function (obj) {
		if (obj.error === true) {
			AbrirMensaje("ATENCIÓN", obj.msg, function () {
				$("#msjModal").modal("hide");
				return true;
			}, false, ["Aceptar"], "error!", null);
		}
		else {
			CargarGrillaTotales(); //Actualizo grilla de totales con info del BE
			//Actualizar valores en la grilla
			$("#tbGridOtroTributo").find('tr').each(function (i, el) {
				var td = $(this).find('td');
				if (td.length > 0 && td[0].innerText !== undefined && td[0].innerText === idOtroTributoSeleccionado && idOtroTributoSeleccionado == idOtroTributoSeleccionadoFocusOut) {
					$("#" + td[4].id).val(formatter.format(obj.data.importe));
				}
			});

		}
	});
}

function AgregarOtroTributo() {
	if (ValidarCamposModalOT()) {
		AbrirWaiting();
		var insId = $("#listaOtroTrib option:selected").val();
		var baseImp = $("#OtroTributo_base_imp").inputmask('unmaskedvalue');
		var alicuota = $("#OtroTributo_alicuota").inputmask('unmaskedvalue');
		var importe = $("#OtroTributo_importe").val();
		var data = { insId, baseImp, alicuota, importe };
		PostGen(data, agregarOtroTributoUrl, function (obj) {
			CerrarWaiting();
			if (obj.error === true) {
				ControlaMensajeError(obj.msg);
			}
			else {
				CargarGrillaOtrosTributos();
				CargarGrillaTotales();
				LimpiarCamposEnModalCargaOT();
			}
		});
	};
}

function CargarGrillaOtrosTributosDesdeSeleccion(afip_id, cm_cuit, tco_id, cm_compte) {
	var data = { afip_id, cm_cuit, tco_id, cm_compte };
	PostGenHtml(data, cargarOtrosTributosDesdeSeleccionUrl, function (obj) {
		$("#divOtrosTributos").html(obj);
		addInCellKeyDownHandler();
		tableUpDownArrow();
		addInCellGotFocusHandler();
		addInCellEditHandler();
		addInCellLostFocusHandler();
		FormatearValores(tbGridOtroTributo, [2, 3, 4]);
		addMaskInEditableCells();
		desactivarGrilla(tbGridOtroTributo);
		$("#btnAgregarOtroTributo").prop("disabled", true);
		return true
	});
}

function CargarGrillaOtrosTributos() {
	var tco_id = $("#listaTCompte").val();
	tcoIdSelected = tco_id;
	var data = { tco_id };
	PostGenHtml(data, cargarOtrosTributosUrl, function (obj) {
		$("#divOtrosTributos").html(obj);
		addInCellKeyDownHandler();
		tableUpDownArrow();
		addInCellGotFocusHandler();
		addInCellEditHandler();
		addInCellLostFocusHandler();
		FormatearValores(tbGridOtroTributo, [2, 3, 4]);
		addMaskInEditableCells();
		return true
	});
}

function CalcularImporteOT() {
	var baseImp = $("#OtroTributo_base_imp").inputmask('unmaskedvalue');
	var aliCuota = $("#OtroTributo_alicuota").inputmask('unmaskedvalue');
	if ($("#OtroTributo_base_imp").val() != "" && $("#OtroTributo_alicuota").val() != "") {
		//var calc = ((baseImp * aliCuota) / 100) + baseImp;
		var calc = ((baseImp * aliCuota) / 100);
		$("#OtroTributo_importe").val(calc.toFixed(2));
	}
	else if ($("#OtroTributo_base_imp").val() != "" && $("#OtroTributo_alicuota").val() == "") {
		$("#OtroTributo_alicuota").val("0");
		$("#OtroTributo_importe").val(baseImp);
	}
}

function ValidarCamposModalOT() {
	if ($("#listaOtroTrib").val() == "") {
		ControlaMensajeWarning("Debe seleccionar un valor de 'Otro tributo.");
		return false;
	}
	if ($("#OtroTributo_base_imp").val() == "") {
		ControlaMensajeWarning("El campo 'Base imponible' es obligatorio.");
		return false;
	}
	if ($("#OtroTributo_alicuota").val() == "") {
		ControlaMensajeWarning("El campo 'Alicuota' es obligatorio.");
		return false;
	}
	if ($("#OtroTributo_importe").val() == "") {
		ControlaMensajeWarning("El campo 'Importe' es obligatorio.");
		return false;
	}
	return true;
}

function quitarOtroTributo(e) {
	var id = $(e).attr("data-interaction");
	var data = { id };
	PostGenHtml(data, quitarItemEnOtrosTributosUrl, function (obj) {
		$("#divOtrosTributos").html(obj);
		addInCellKeyDownHandler();
		tableUpDownArrow();
		addInCellGotFocusHandler();
		addInCellEditHandler();
		addInCellLostFocusHandler();
		FormatearValores(tbGridOtroTributo, [2, 3, 4]);
		addMaskInEditableCells();
		CargarGrillaTotales();
	});
}

function addInCellKeyDownHandler() {
	$("#tbGridOtroTributo").on('keydown', 'td[contenteditable]', function (e) {
		if (isNaN(String.fromCharCode(e.which)) && !(keysAceptadas.indexOf(e.which) != -1) && !(e.shiftKey && (e.which == 37 || e.which == 39))) e.preventDefault();
	});
}

function addInCellGotFocusHandler() {
	$("#tbGridOtroTributo").on('focusin', 'td[contenteditable]', function (e) {

		cellValueTemp = $("#" + this.id).text();
		idOtroTributoSeleccionado = $("#" + this.id).attr("data-interaction")
		if (e.target) {
			cellIndexTemp = e.target.cellIndex;
		}
	});
}

function addInCellEditHandler() {
	$("#tbGridOtroTributo").on('input', 'td[contenteditable]', function (e) {
		var val = $("#" + this.id).text();
		if (this.id.includes("alicuota")) {
		}
		else if (this.id.includes("base_imp") || this.id.includes("importe")) {
		}
	});
}

function addInCellLostFocusHandler() {
	$("#tbGridOtroTributo").on('focusout', 'td[contenteditable]', function (e) {
		var actualiza = true;
		if (cellValueTemp == $("#" + this.id).text())
			actualiza = false;
		if (this.id.includes("alicuota")) {
			var valor = $("#" + this.id).inputmask('unmaskedvalue');
		}
		else if (this.id.includes("base_imp") || this.id.includes("importe")) {
			var valor = $("#" + this.id).inputmask('unmaskedvalue');
		}
		if (actualiza) {
			idOtroTributoSeleccionadoFocusOut = $(this).attr("data-interaction");
			ActualizarOtroTributo(this.id, valor);
		}
	});
}

function addMaskInEditableCells() {
	if ($("#tbGridOtroTributo").length != 0) {
		$("#tbGridOtroTributo").find('tr').each(function (i, el) {
			var td = $(this).find('td');
			if (td.length > 0) {
				if (td[2].id.includes("base_imp_")) {
					getMaskForMoneyType("#" + td[2].id);
				}
				if (td[4].id.includes("importe_")) {
					getMaskForMoneyType("#" + td[4].id);
				}
				if (td[3].id.includes("alicuota_")) {
					getMaskForDiscountType("#" + td[3].id);
				}
			}
		});
	}
}

function AbrirModalAgregarOtroTributo() {
	var tco_id = $("#listaTCompte option:selected").val()
	if (tco_id == "") {
		AbrirMensaje("ATENCIÓN", "Debe seleccionar un tipo de Comprobante.", function () {
			$("#msjModal").modal("hide");
			$("listaTCompte").trigger("focus");
			return true;
		}, false, ["Aceptar"], "error!", null);
	}
	else {
		AbrirWaiting();
		var datos = { tco_id };
		PostGenHtml(datos, obtenerDatosModalOtrosTributosUrl, function (obj) {
			$("#divModalCargaOT").html(obj);
			$('#modalCargaOT').modal({
				backdrop: 'static',
			});
			$('#modalCargaOT').modal('show');

			$("#OtroTributo_base_imp").inputmask({
				alias: 'numeric',
				groupSeparator: '.',
				radixPoint: ',',
				digits: 2,
				digitsOptional: false,
				allowMinus: false,
				prefix: '',
				suffix: '',
				rightAlign: true,
				unmaskAsNumber: true // Devuelve un número al obtener el valor
			});
			$("#OtroTributo_base_imp").on('focusout', function (e) {
				CalcularImporteOT();
			});

			$("#OtroTributo_alicuota").inputmask({
				alias: 'numeric',
				groupSeparator: '.',
				radixPoint: ',',
				digits: 2,
				digitsOptional: false,
				allowMinus: false,
				prefix: '',
				suffix: '',
				rightAlign: true,
				unmaskAsNumber: true // Devuelve un número al obtener el valor
			});
			$("#OtroTributo_alicuota").on('focusout', function (e) {
				CalcularImporteOT();
			});
			$("#OtroTributo_importe").mask("000.000.000.000,00", { reverse: true });

			setTimeout(() => {
				$(".inputEditable").on("keypress", analizaEnterInput)
				document.getElementById("listaOtroTrib").focus();
			}, 500);
			CerrarWaiting();
			return true
		});
	}
}

function AbrirModalConceptoFacturado() {
	var tco_id = $("#listaTCompte option:selected").val();
	if (tco_id == "") {
		AbrirMensaje("ATENCIÓN", "Debe seleccionar un tipo de Comprobante.", function () {
			$("#msjModal").modal("hide");
			$("listaTCompte").trigger("focus");
			return true;
		}, false, ["Aceptar"], "error!", null);
	}
	else {
		AbrirWaiting();
		var datos = { tco_id };
		PostGenHtml(datos, obtenerDatosModalConceptoFacturadoUrl, function (obj) {
			$("#divModalCargaIVA").html(obj);
			$('#modalCargaIVA').modal({
				backdrop: 'static',
			});
			$('#modalCargaIVA').modal('show');

			$("#ConceptoFacturado_subtotal").inputmask({
				alias: 'numeric',
				groupSeparator: '.',
				radixPoint: ',',
				digits: 2,
				digitsOptional: false,
				allowMinus: false,
				prefix: '',
				suffix: '',
				rightAlign: true,
				unmaskAsNumber: true // Devuelve un número al obtener el valor
			});
			$("#ConceptoFacturado_subtotal").on('focusout', function (e) {
				CalcularIva(e);
			});

			$("#ConceptoFacturado_iva").inputmask({
				alias: 'numeric',
				groupSeparator: '.',
				radixPoint: ',',
				digits: 2,
				digitsOptional: false,
				allowMinus: false,
				prefix: '',
				suffix: '',
				rightAlign: true,
				unmaskAsNumber: true // Devuelve un número al obtener el valor
			});
			$("#ConceptoFacturado_iva").on('focusout', function (e) {
				CalcularIva(e);
			});
			$("#ConceptoFacturado_total").mask("000.000.000.000,00", { reverse: true });
			setTimeout(() => {
				$(".inputEditable").on("keypress", analizaEnterInput)
				document.getElementById("ConceptoFacturado_concepto").focus();
			}, 500);
			$("#ConceptoFacturado_concepto").trigger("focus");

			document.getElementById("btnAgregar").addEventListener("keyup", function (event) {
				if (event.keyCode === 13) {
					AgregarConceptoFacturado();
				}
			});
			CerrarWaiting();
			return true
		});
	}
}

function quitarConceptoFacturado(e) {
	var id = $(e).attr("data-interaction");
	var data = { id };
	PostGenHtml(data, quitarItemEnConceptoFacturadoURL, function (obj) {
		$("#divConceptosFacturados").html(obj);
		FormatearValores(tbGridConceptoFacturado, [4, 5, 6]);
		CargarGrillaTotales();
	});
}

function CargarGrillasAdicionales(reinicia = false) {
	//Grilla de Conceptos Facturados
	CargarGrillaConceptosFacturados(reinicia);
	//Grilla de Otros Tributos
	CargarGrillaOtrosTributos();
	//Grilla Totales
	CargarGrillaTotales();
}

function CargarGrillaConceptosFacturados(reinicia = false) {
	var data = { reinicia };
	PostGenHtml(data, cargarConceptosFacturadosUrl, function (obj) {
		$("#divConceptosFacturados").html(obj);
		FormatearValores(tbGridConceptoFacturado, [4, 5, 6]);
		return true
	});
}

function desactivarGrilla(gridId) {
	$(gridId).addClass("disable-table-rows");
	$(".table-wrapper").css("overflow", "hidden");
}

function activarGrilla(gridId) {
	$(gridId).removeClass("disable-table-rows");
	$(".table-wrapper").css("overflow", "auto");

}

function CargarGrillaConceptosFacturadosDesdeSeleccion(afip_id, cm_cuit, tco_id, cm_compte) {
	var data = { afip_id, cm_cuit, tco_id, cm_compte };
	PostGenHtml(data, cargarConceptosFacturadosDesdeSeleccionUrl, function (obj) {
		$("#divConceptosFacturados").html(obj);
		FormatearValores(tbGridConceptoFacturado, [4, 5, 6]);
		desactivarGrilla(tbGridConceptoFacturado);
		$("#btnAgregarConceptoFacturado").prop("disabled", true);
		return true
	});
}

function CargarGrillaTotales() {
	var data = {};
	PostGenHtml(data, cargarGrillaTotalesUrl, function (obj) {
		$("#divTotales").html(obj);
		FormatearValores(tbGridTotales, [1]);
		return true
	});
}

function ControlaSituacionSeleccionada() {
	var iva_sit_id = $("#listaIvaSit option:selected").val()
	if (iva_sit_id == IvaSituacion.GRAVADO) {
		$("#listaIvaAli").val("");
		$("#listaIvaAli").prop("disabled", false);
		$("#ConceptoFacturado_iva").prop("disabled", false);
		$("#listaIvaAli").trigger("focus");
	}
	else {
		$("#listaIvaAli").val("");
		$("#listaIvaAli").prop("disabled", true);
		$("#ConceptoFacturado_iva").prop("disabled", true);
		$("#ConceptoFacturado_subtotal").trigger("focus");
	}
}

function AgregarConceptoFacturado() {
	if (ValidarCamposModalIva()) {
		AbrirWaiting();
		var subt = $("#ConceptoFacturado_subtotal").inputmask('unmaskedvalue');
		var concepto = $("#ConceptoFacturado_concepto").val();
		var sit = $("#listaIvaSit").val();
		var ali = $("#listaIvaAli").val();
		var iva = $("#ConceptoFacturado_iva").inputmask('unmaskedvalue');
		var tot = $("#ConceptoFacturado_total").val();
		var data = { concepto, sit, ali, subt, iva, tot };
		PostGen(data, agregarConceptoFacturadoUrl, function (obj) {
			CerrarWaiting();
			if (obj.error === true) {
				ControlaMensajeError(obj.msg);
			}
			else {
				//Actualizar grilla Conceptos facturados
				CargarGrillaConceptosFacturados();
				CargarGrillaTotales();
				LimpiarCamposEnModalCargaIva();
			}

		});
	}
}

function ValidarCamposModalIva() {
	if ($("#ConceptoFacturado_concepto").val() == "") {
		ControlaMensajeWarning("El campo 'Concepto' es obligatorio.");
		return false;
	}
	if ($("#listaIvaSit").val() == "") {
		ControlaMensajeWarning("Debe seleccionar un valor de 'Situación.");
		return false;
	}
	if ($("#listaIvaSit").val() != "" && $("#listaIvaSit").val() == "G" && $("#listaIvaAli").val() == "") {
		ControlaMensajeWarning("Debe seleccionar un valor de 'Alicuota.");
		return false;
	}
	if ($("#ConceptoFacturado_subtotal").val() == "") {
		ControlaMensajeWarning("El campo 'Subtotal' es obligatorio.");
		return false;
	}
	return true;
}

function LimpiarCamposEnModalCargaIva() {
	$("#ConceptoFacturado_concepto").val("");
	$("#listaIvaSit").val("");
	$("#listaIvaAli").val("");
	$("#ConceptoFacturado_subtotal").val("");
	$("#ConceptoFacturado_iva").val("");
	$("#ConceptoFacturado_total").val("");
	$("#ConceptoFacturado_concepto").trigger("focus");
}

function LimpiarCamposEnModalCargaOT() {
	$("#listaOtroTrib").val("");
	$("#OtroTributo_base_imp").val("");
	$("#OtroTributo_alicuota").val("");
	$("#OtroTributo_importe").val("");
	$("#listaOtroTrib").trigger("focus");
}

/*
////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
FIN
MÉTODOS COMUNTES DE MODULOS:
*) comprobantedecompra.js
*) odepagodirecta.js
*/

function getMaskForDiscountType(selector) {
	$(selector).inputmask({
		alias: 'numeric',
		groupSeparator: '.',
		radixPoint: ',',
		digits: 1,
		digitsOptional: false,
		allowMinus: false,
		prefix: '',
		suffix: '',
		min: 0,
		max: 50,
		unmaskAsNumber: true
	});
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