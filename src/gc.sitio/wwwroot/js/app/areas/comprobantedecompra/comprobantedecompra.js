$(function () {
	InicializaPantalla();
});

const IvaSituacion = {
	GRAVADO: 'G',
	NO_GRAVADO: 'N',
	EXENTO: 'E'
}

const formatter = new Intl.NumberFormat('de-DE', {
	//style: 'currency',
	//currency: 'USD',

	trailingZeroDisplay: 'stripIfInteger'
});

function InicializaPantalla() {
	$("#divFiltro").collapse("show");
	$("#lbRel01").text("Proveedor");
	$("#chkRel01").prop('checked', true);
	$("#chkRel01").trigger("change");
	$("#chkRel01").prop("disabled", true);
	$("#Rel01").prop("disabled", false);
	$("input#Rel01").on("click", function () {
		$("input#Rel01").val("");
		$("#Rel01Item").val("");
	});
	$("#Rel01List").collapse("hide")
	$("#btnBuscar").on("click", function () {
		if (ctaIdSelected == "") {
			AbrirMensaje("ATENCIÓN", "Debe seleccionar una cuenta.", function () {
				$("#msjModal").modal("hide");
				$("input#Rel01").focus();
				return true;
			}, false, ["Aceptar"], "error!", null);
		}
		else {
			InicializarComprobante(ctaIdSelected);
		}
	});
	$(".activable").prop("disabled", true);
	$("#btnAbmAceptar").hide();
	$("#btnAbmCancelar").hide();
	$("#btnDetalle").prop("disabled", true);

	activarBotones(false);
	ctaIdSelected = "";
	MostrarDatosDeCuenta(false);

	$(document).on("change", "#listaTCompte", ControlaListaOpcionesSeleccion);
	$(document).on("change", "#listaOpeIva", ControlaListaOpcionesSeleccion);
	$(document).on("change", "#listaOpciones", ObtenerGrillaDesdeOpcionSeleccionada);
	$(document).on("click", "#btnAgregarConceptoFacturado", AbrirModalConceptoFacturado); //Abrir modal
	$(document).on("change", "#listaIvaSit", ControlaSituacionSeleccionada);
	$(document).on("change", "#listaIvaAli", ControlaIvaAliSeleccionada);
	$(document).on("click", "#btnAgregarOtroTributo", AbrirModalAgregarOtroTributo); //Abrir modal
	$(document).on("change", "#listaOtroTrib", ControlaOtroTribSeleccionada);

	$(".inputEditable").on("keypress", analizaEnterInput);

	CerrarWaiting();
	return true;
}

function InicializarComprobante(id) {
	AbrirWaiting();
	var cta_id = ctaIdSelected;

	var data = { cta_id };
	PostGenHtml(data, inicializarComprobanteUrl, function (obj) {
		$("#divComprobante").html(obj);
		$("#divDetalle").collapse("show");
		$("#btnDetalle").prop("disabled", false);
		$("#divFiltro").collapse("hide")
		MostrarDatosDeCuenta(true);
		activarBotones(true);
		SetMascarasYValores();
		ActualizaEstadosVarios();
		CargarGrillasAdicionales();
		ObtenerGrillaDesdeOpcionSeleccionada();
		$(".inputEditable").on("keypress", analizaEnterInput);
		CerrarWaiting();
		return true
	});
}

function AbrirModalConceptoFacturado() {
	AbrirWaiting();
	var tco_id = $("#listaTCompte option:selected").val()
	var datos = { tco_id };
	PostGenHtml(datos, obtenerDatosModalConceptoFacturadoUrl, function (obj) {
		$("#divModalCargaIVA").html(obj);
		$('#modalCargaIVA').modal({
			backdrop: 'static',
			/*keyboard: false*/
		});
		$('#modalCargaIVA').modal('show');

		//$("#ConceptoFacturado_subtotal").mask("000.000.000.000,00", { reverse: true });
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
		//$("#ConceptoFacturado_iva").inputmask({ mask: '99,9', alias: 'numeric', digits: 1, digitsOptional: false, rightAlign: true, unmaskAsNumber: true, radixPoint: ',', placeholder: '' });
		//$("#ConceptoFacturado_iva").mask("000.000.000.000,00", { reverse: true });
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
		}, 500);
		$("#ConceptoFacturado_concepto").focus();
		CerrarWaiting();
		return true
	});
}

function AbrirModalAgregarOtroTributo() {
	AbrirWaiting();
	var tco_id = $("#listaTCompte option:selected").val()
	var datos = { tco_id };
	PostGenHtml(datos, obtenerDatosModalOtrosTributosUrl, function (obj) {
		$("#divModalCargaOT").html(obj);
		$('#modalCargaOT').modal({
			backdrop: 'static',
			/*keyboard: false*/
		});
		$('#modalCargaOT').modal('show');

		/*$("#OtroTributo_base_imp").mask("000.000.000.000,00", { reverse: true });*/
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
		//$("#OtroTributo_alicuota").mask("000.000.000.000,00", { reverse: true });
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
		//$("#OtroTributo_importe").inputmask({
		//	alias: 'numeric',
		//	groupSeparator: '.',
		//	radixPoint: ',',
		//	digits: 2,
		//	digitsOptional: false,
		//	allowMinus: false,
		//	prefix: '$',
		//	suffix: '',
		//	rightAlign: true,
		//	unmaskAsNumber: true // Devuelve un número al obtener el valor
		//});
		$(".inputEditable").on("keypress", analizaEnterInput);
		$("#listaOtroTrib").focus();
		CerrarWaiting();
		return true
	});
}

function CalcularIva(e) {
	var sit_id = $("#listaIvaSit option:selected").val()
	var ali_id = $("#listaIvaAli option:selected").val()
	var subt = $("#ConceptoFacturado_subtotal").inputmask('unmaskedvalue');
	var iva = $("#ConceptoFacturado_iva").inputmask('unmaskedvalue');
	//var num_subt = parseFloat(subt.replace(".", "").replace(",", "."));
	//var num_ali_id = parseFloat(ali_id);
	if (subt > 0 && sit_id == "G") {
		if (ali_id != "") {
			if (e.target.id != "ConceptoFacturado_iva") {
				var calc = (subt * parseFloat(ali_id)) / 100;
				$("#ConceptoFacturado_iva").val(calc.toFixed(2).replace(".",","));
				//$("#ConceptoFacturado_iva").val(calc);
				$("#ConceptoFacturado_total").val((subt + calc).toFixed(2));
			}
			else {
				var calc = subt + iva;
				$("#ConceptoFacturado_total").val((calc).toFixed(2));
			}
		}
	}
	else if (subt > 0 && sit_id != "G") {
		if (e.target.id != "ConceptoFacturado_iva") {
			var calc = subt / 100;
			$("#ConceptoFacturado_iva").val(calc);
			$("#ConceptoFacturado_total").val(subt + calc);
		}
		else {
			var calc = subt / 100;
			$("#ConceptoFacturado_total").val(subt + calc);
		}
	}
}

function CalcularImporteOT() {
	var baseImp = $("#OtroTributo_base_imp").inputmask('unmaskedvalue');
	var aliCuota = $("#OtroTributo_alicuota").inputmask('unmaskedvalue');
	if ($("#OtroTributo_base_imp").val() != "" && $("#OtroTributo_alicuota").val() != "") {
		//var subt = $("#OtroTributo_base_imp").val();
		//var num_subt = parseFloat(subt.replace(".", "").replace(",", "."));
		//var ali = $("#OtroTributo_alicuota").val();
		//var num_ali = parseFloat(ali.replace(".", "").replace(",", "."));
		var calc = ((baseImp * aliCuota) / 100) + baseImp;
		$("#OtroTributo_importe").val(calc.toFixed(2));
	}
	else if ($("#OtroTributo_base_imp").val() != "" && $("#OtroTributo_alicuota").val() == "") {
		$("#OtroTributo_alicuota").val("0");
		$("#OtroTributo_importe").val(baseImp);
	}
}

function AgregarConceptoFacturado() {
	if (ValidarCamposModalIva()) {
		AbrirWaiting();
		var subt = $("#ConceptoFacturado_subtotal").inputmask('unmaskedvalue');
		//var num_subt = parseFloat(subt.replace(".", "").replace(",", "."));
		var concepto = $("#ConceptoFacturado_concepto").val();
		var sit = $("#listaIvaSit").val();
		var ali = $("#listaIvaAli").val();
		//var subt = num_subt;
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
				//De deshabilita para que el usuario pueda cargar mas de un item sin cerrar el modal.
				//$('#modalCargaIVA').modal('hide');
			}

		});
	}
}

function AgregarOtroTributo() {
	//TODO MARCE:
	//Agregar funcion de validacion de campos antes de agrega del tributo DONE
	if (ValidarCamposModalOT()) {
		AbrirWaiting();
		var insId = $("#listaOtroTrib option:selected").val();
		var baseImp = $("#OtroTributo_base_imp").inputmask('unmaskedvalue');
		//var baseImp = parseFloat(baseImp_temp.replace(".", "").replace(",", "."));
		var alicuota = $("#OtroTributo_alicuota").inputmask('unmaskedvalue');
		//var alicuota = parseFloat(alicuota_temp.replace(".", "").replace(",", "."));
		//var importe_temp = $("#OtroTributo_importe").val();
		//var importe = parseFloat(importe_temp.replace(".", "").replace(",", "."));
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
				//$('#modalCargaIVA').modal('hide');
			}
		});
	};
	//Luego llamar a un metodo (aun no existe) de BE para agregarlo a la lista en sesion. DONE
	//Agregar el metodo de quitar tributo (icono de tacho de basura)
}

function LimpiarCamposEnModalCargaIva() {
	$("#ConceptoFacturado_concepto").val("");
	$("#listaIvaSit").val("");
	$("#listaIvaAli").val("");
	$("#ConceptoFacturado_subtotal").val("");
	$("#ConceptoFacturado_iva").val("");
	$("#ConceptoFacturado_total").val("");
}

function LimpiarCamposEnModalCargaOT() {
	$("#listaOtroTrib").val("");
	$("#OtroTributo_base_imp").val("");
	$("#OtroTributo_alicuota").val("");
	$("#OtroTributo_importe").val("");
	$("#listaOtroTrib").focus();
}

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

function ControlaIvaAliSeleccionada() {
	var iva_ali_id = $("#listaIvaAli option:selected").val()
	if (iva_ali_id != "") {
		$("#ConceptoFacturado_subtotal").focus();
	}
}

function ControlaSituacionSeleccionada() {
	var iva_sit_id = $("#listaIvaSit option:selected").val()
	if (iva_sit_id == IvaSituacion.GRAVADO) {
		$("#listaIvaAli").prop("disabled", false);
		$("#listaIvaAli").focus();
	}
	else {
		$("#listaIvaAli").prop("disabled", true);
		$("#ConceptoFacturado_subtotal").focus();
	}
}

function ControlaOtroTribSeleccionada() {
	var otro_tirb_id = $("#listaOtroTrib option:selected").val()
	if (otro_tirb_id != "") {
		$("#OtroTributo_base_imp").focus();
	}
}

function ControlaListaOpcionesSeleccion() {
	var tco_id = $("#listaTCompte option:selected").val()
	var ope_iva = $("#listaOpeIva option:selected").val()
	if (tco_id !== "" && ope_iva != "") {
		var data = { tco_id, ope_iva }
		AbrirWaiting();
		PostGenHtml(data, actualizarListaOpcionesUrl, function (obj) {
			$("#divListaOpciones").html(obj);
			ObtenerGrillaDesdeOpcionSeleccionada();
			CerrarWaiting();
		}, function (obj) {
			ControlaMensajeError(obj.message);
			CerrarWaiting();
		});
	}
}

function ObtenerGrillaDesdeOpcionSeleccionada() {
	var id_selected = $("#listaOpciones option:selected").val()
	var cta_id = $("#CtaID").val();
	if (id_selected != "") {
		var data = { cta_id, id_selected };
		AbrirWaiting();
		PostGenHtml(data, obtenerGrillaDesdeOpcionSeleccionadaUrl, function (obj) {
			$("#divGrillaOpcional").html(obj);
			CerrarWaiting();
		}, function (obj) {
			ControlaMensajeError(obj.message);
			CerrarWaiting();
		});
	}
}

function CargarGrillasAdicionales() {
	//Grilla de Conceptos Facturados
	CargarGrillaConceptosFacturados();
	//Grilla de Otros Tributos
	CargarGrillaOtrosTributos();
	//Grilla Totales
	CargarGrillaTotales();
}

function CargarGrillaTotales() {
	var data = {};
	PostGenHtml(data, cargarGrillaTotalesUrl, function (obj) {
		$("#divTotales").html(obj);
		return true
	});
}

function CargarGrillaConceptosFacturados() {
	var data = {};
	PostGenHtml(data, cargarCargarConceptosFacturadosUrl, function (obj) {
		$("#divConceptosFacturados").html(obj);
		FormatearValores(tbGridConceptoFacturado, [4, 5, 6]);
		return true
	});
}

function CargarGrillaOtrosTributos() {
	var tco_id = $("#listaTCompte option:selected").val()
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

function selectReg(x, gridId) {
	$("#" + gridId + " tbody tr").each(function (index) {
		$(this).removeClass("selected-row");
		$(this).removeClass("selectedEdit-row");
	});
	$(x).addClass("selected-row");
	if (gridId == 'tbGridOtroTributo') {
		idOtroTributoSeleccionado = x.cells[0].innerText.trim();
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
		FormatearValores(tbGridOtroTributo, [4]);
		CargarGrillaTotales();
	});
}

function ActualizaEstadosVarios() {
	$("#chkCtlFis").on("click", function () {
		ActualizarEstadoCAE();
	});
	ActualizarEstadoCAE();
	$("#chkImpCtaDirecta").on("click", function () {
		ActualizarEstadoCtaDirecta();
	});
	ActualizarEstadoCtaDirecta();
}
function ActualizarEstadoCtaDirecta() {
	if ($("#chkImpCtaDirecta").prop("checked")) {
		$("#listaCtaDir").prop("disabled", false);
		$("#listaCtaDir").focus();
	}
	else {
		$("#listaCtaDir").val("");
		$("#listaCtaDir").prop("disabled", true);
	}
}

function ActualizarEstadoCAE() {
	if ($("#chkCtlFis").prop("checked")) {
		$("#Comprobante_cm_cae").prop("disabled", false);
		$("#Comprobante_cm_cae_vto").prop("disabled", false);
		$("#Comprobante_cm_cae_vto").focus();
	}
	else {
		$("#Comprobante_cm_cae").val("");
		$("#Comprobante_cm_cae").prop("disabled", true);
		$("#Comprobante_cm_cae_vto").prop("disabled", true);
	}
}

function SetMascarasYValores() {
	$("#Comprobante_cuit_parcial").mask("00-00000000-0", { reverse: false });
	$("#Comprobante_cm_compte").mask("0000-00000000", { reverse: false });
	var now = moment().format('yyyy-MM-DD');
	$("#Comprobante_fecha_pago").val(now);
	$("#Comprobante_cm_cae_vto").val(now);
	$("#Comprobante_fecha_compte").val(now);
	//$("#listaTCompte").on("change", function () {
	//	console.log(this);
	//});
	//$(document).on("change", "#listaTCompte", function () {
	//	console.log(this);
	//});
}

function activarBotones(activar) {
	if (activar === true) {
		$("#btnAbmAceptar").prop("disabled", false);
		$("#btnAbmCancelar").prop("disabled", false);
		$("#btnAbmAceptar").show();
		$("#btnAbmCancelar").show();
	}
	else {
		$("#btnAbmAceptar").prop("disabled", true);
		$("#btnAbmCancelar").prop("disabled", true);
		$("#btnAbmAceptar").hide();
		$("#btnAbmCancelar").hide();
	}
}

function MostrarDatosDeCuenta(mostrar) {
	if (mostrar) {
		$("#CtaID").val(ctaIdSelected);
		$("#CtaDesc").val(ctaDescSelected);
		$("#divProveedorSeleccionado").collapse("show");
	}
	else {
		$("#CtaID").val("");
		$("#CtaDesc").val("");
		$("#divProveedorSeleccionado").collapse("hide");
	}
}

function onChangeFechaDePago(e) {
}

function onChangeCaeVto(e) {
}

function onChangeFechaCompte(e) { }

function selectListaRprAsociado(e) { }

$("#Rel01").autocomplete({
	source: function (request, response) {

		data = { prefix: request.term }; Rel01

		$.ajax({
			url: autoComRel01Url,
			type: "POST",
			dataType: "json",
			data: data,
			success: function (obj) {
				response($.map(obj, function (item) {
					var texto = item.descripcion;
					return { label: texto, value: item.descripcion, id: item.id, prov: item.provId };
				}));
			}
		})
	},
	minLength: 3,
	select: function (event, ui) {
		ctaIdSelected = ui.item.id;
		ctaDescSelected = ui.item.value;
		$("#Rel01List").empty();
		$("#Rel01Item").val(ui.item.id);
		var opc = "<option value=" + ui.item.id + ">" + ui.item.value + "</option>"
		$("#Rel01List").append(opc);
		return true;
	}
});

function analizaEnterInput(e) {
	if (e.which == "13") {
		tope = 99999;
		index = -1;
		//obtengo los inputs dentro del div
		var inputss = $("main :input:not(:disabled)");
		tope = inputss.length;
		//le el id del input en el que he dado enter
		var cual = $(this).prop("id");
		inputss.each(function (i, item) {
			if ($(item).prop("id") === cual) {
				index = i;
				return false;
			}
		});
		if (index > -1 && tope > index + 1) {
			inputss[index + 1].focus();
		}

		////verifico cuantos input habilitados encuentro
		//var $nextInput = $(this).nextAll("input:not(:disabled)");
		//if ($nextInput.length>0) {
		//    $nextInput.first().focus();
		//    return true;
		//} else if ($(this).prop("id") === "unid") {
		//    e.preventDefault();
		//    $("#btnCargarProd").focus();
		//}
	}
	return true;
}

function focusOnTd(x) {
	var cell = x;
	var range, selection;
	if (document.body.createTextRange) {
		range = document.body.createTextRange();
		range.moveToElementText(cell);
		range.select();
	} else if (window.getSelection) {
		selection = window.getSelection();
		range = document.createRange();
		range.selectNodeContents(cell);
		selection.removeAllRanges();
		selection.addRange(range);
	}
}

var keysAceptadas = [8, 37, 39, 46, 96, 97, 98, 99, 100, 101, 102, 103, 104, 105, 110, 190];

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

function addInCellKeyDownHandler() {
	$("#tbGridOtroTributo").on('keydown', 'td[contenteditable]', function (e) {
		if (isNaN(String.fromCharCode(e.which)) && !(keysAceptadas.indexOf(e.which) != -1) && !(e.shiftKey && (e.which == 37 || e.which == 39))) e.preventDefault();
	});
}

function addInCellGotFocusHandler() {
	$("#tbGridOtroTributo").on('focusin', 'td[contenteditable]', function (e) {

		cellValueTemp = $("#" + this.id).text();
		if (e.target) {
			cellIndexTemp = e.target.cellIndex;
		}
	});
}

function addInCellEditHandler() {
	$("#tbGridOtroTributo").on('input', 'td[contenteditable]', function (e) {
		var val = $("#" + this.id).text();
		if (this.id.includes("alicuota")) {
			//var num = Number(val);
			//if (num > 50) val = "50";
			//$("#" + this.id).mask("00,0", { reverse: true });
			//$("#" + this.id).val(val);
			//$("#" + this.id).text(val);
		}
		else if (this.id.includes("base_imp") || this.id.includes("importe")) {
			//console.log($("#" + this.id));
			//console.log($("#" + this.id).text());
			//console.log($("#" + this.id).val());
			//$("#" + this.id).mask("000.000.000.000,00", { reverse: false });
			//$("#" + this.id).val(val);
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
			ActualizarOtroTributo(this.id, valor);
			//if ($("#" + this.id).val() == "")
			//	ActualizarOtroTributo(this.id, $("#" + this.id).text());
			//else
			//	ActualizarOtroTributo(this.id, $("#" + this.id).val());
		}
	});
}

function ActualizarOtroTributo(id, val) {
	//var temp = val.replace(".", "");

	//val = parseFloat(val.replace(".", "").replace(",", "."));
	console.log("idOtroTributoSeleccionado: " + idOtroTributoSeleccionado + " id: " + id + " val: " + val);
	var data = { id, val, idOtroTributoSeleccionado };
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
				if (td.length > 0 && td[0].innerText !== undefined && td[0].innerText === idOtroTributoSeleccionado) {
					console.log("td[0].innerText: " + td[0].innerText);
					$("#" + td[4].id).val(formatter.format(obj.data.importe));
				}
			});
			
		}
	});
	///TODO:
	//usar idOtroTributoSeleccionado que lo almaceno cuando selecciono un elemento de la grilla y mandar:
	// idOtroTributoSeleccionado -> id del "otro tributo" seleccionado
	// id -> campo que he editado
	// val -> valor
	// con esos datos calcular el valor de la columna Importe y pisarlo

	//var data = { pId, field, val };
	//PostGen(data, ActualizarProductoEnOcURL, function (obj) {
	//	if (obj.error === true) {
	//		AbrirMensaje("ATENCIÓN", obj.msg, function () {
	//			$("#msjModal").modal("hide");
	//			return true;
	//		}, false, ["Aceptar"], "error!", null);
	//	}
	//	else {
	//		//Actualizar valores en la grilla
	//		$("#tbListaProductoOC").find('tr').each(function (i, el) {
	//			var td = $(this).find('td');
	//			if (td.length > 0 && td[1].innerText !== undefined && td[1].innerText === pId) {
	//				//GRILLA
	//				td[16].innerText = obj.data.pedido_Mas_Boni;//PEDIDO +BONI -> obj.data.pedido_Mas_Boni
	//				td[17].innerText = obj.data.p_Pcosto;//PRECIO COSTO -> obj.data.p_Pcosto
	//				td[18].innerText = obj.data.p_Pcosto_Total;//TOTAL COSTO -> obj.data.p_Pcosto_Total
	//				td[19].innerText = obj.data.paletizado;//TOTAL PALLET -> obj.data.paletizado

	//				//TOTALES
	//				$("#Total_Costo").val(formatter.format(obj.data.total_Costo));//TOTAL_COSTO -> obj.data.total_Costo
	//				$("#Total_Pallet").val(formatter.format(obj.data.total_Pallet));//TOTAL_PALLET -> obj.data.total_Pallet
	//			}
	//		});
	//	}

	//});
}

function tableUpDownArrow() {
	var table = document.querySelector('#tbGridOtroTributo tbody');
	if (table == undefined)
		return;
	if (table.rows[0] == undefined)
		return;
	const myTable = table
		, nbRows = myTable.rows.length
		, nbCells = myTable.rows[0].cells.length
		, movKey = {
			ArrowUp: p => { p.r = (--p.r + nbRows) % nbRows }
			, ArrowLeft: p => { p.c = (--p.c + nbCells) % nbCells }
			, ArrowDown: p => {
				p.r = ++p.r % nbRows
			}
			, ArrowRight: p => { p.c = ++p.c % nbCells }
			, Tab: p => {
				p.r = ++p.r % nbRows
			}
		}

	myTable
		.querySelectorAll('input, [contenteditable=true]')
		.forEach(elm => {
			elm.onfocus = e => {
				let sPos = myTable.querySelector('.selected-row')
					, tdPos = elm.parentNode

				if (sPos) sPos.classList.remove('selected-row')

				tdPos.classList.add('selected-row')
			}
		})


	document.onkeydown = e => {
		let sPos = myTable.querySelector('.selected-row')
			, evt = (e == null ? event : e)
			, pos = {
				r: sPos ? sPos.rowIndex : -1
				, c: sPos ? (sPos.cellIndex ? sPos.cellIndex : cellIndexTemp) : -1
			}

		if (sPos &&
			(evt.altKey && evt.shiftKey && movKey[evt.code])
			||
			(evt.ctrlKey && movKey[evt.code])
			//||
			//evt.code === 'Tab'
		) {
			let loop = true
				, nxFocus = null
				, cell = null

			do {
				if (evt.code === 'ArrowDown' && pos.r == nbRows)
					pos.r = 0;
				if (evt.code === 'Tab' && evt.shiftKey && pos.r == 0)
					pos.r = nbRows - 1;
				if (evt.code === 'Tab' && evt.shiftKey) {
					movKey['ArrowUp'](pos)
				}
				else
					movKey[evt.code](pos);

				if (pos.r == nbRows)
					cell = myTable.rows[pos.r - 1].cells[pos.c];
				else
					cell = myTable.rows[pos.r].cells[pos.c];
				if (pos.r == 0)
					pos.r = nbRows;
				else if (pos.r == nbRows)
					pos.r = nbRows;

				//if (pos.c == 8 && cellIndexTemp < pos.c) //moviendome desde la columna 'pedido bultos' hacia la derecha, la cual no es editable, debo saltar a la siguiente
				//	pos.c = 9;
				//if (pos.c == 6 && cellIndexTemp > pos.c) //moviendome desde la columna 'pedido bultos' hacia la izquierda, la cual no es editable, debo saltar a la siguiente
				//	pos.c = 15;
				//if (pos.c == 8 && cellIndexTemp > pos.c) //moviendome desde la columna 'precio lista' hacia la izquierda, la cual no es editable, debo saltar a la siguiente
				//	pos.c = 7;
				//if (pos.c == 16 && cellIndexTemp < pos.c) //moviendome desde la columna 'boni' hacia la derecha, la cual no es editable, debo saltar a la siguiente
				//	pos.c = 7;
				nxFocus = myTable.rows[pos.r - 1].cells[pos.c]

				if (nxFocus
					&& cell.style.display !== 'none'
					&& cell.parentNode.style.display !== 'none') {
					nxFocus.focus();
					nxFocus.closest('tr').classList.add('selected-row');
					nxFocus.focus();
					loop = false
				}
			}
			while (loop)
			if (evt.code === 'Tab') {
				event.preventDefault();
			}
		}
		else if (evt.code === 'Enter')
			event.preventDefault();
		else if (evt.code === 'NumpadEnter')
			event.preventDefault();
	}
}
