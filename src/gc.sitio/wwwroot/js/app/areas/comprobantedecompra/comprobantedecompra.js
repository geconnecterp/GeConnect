$(function () {
	InicializaPantalla();
});

const IvaSituacion = {
	GRAVADO: 'G',
	NO_GRAVADO: 'N',
	EXENTO: 'E'
}

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
			keyboard: false
		});
		$('#modalCargaIVA').modal('show');

		$("#ConceptoFacturado_subtotal").mask("000.000.000.000,00", { reverse: true });
		$("#ConceptoFacturado_subtotal").on('focusout', function (e) {
			CalcularIva();
		});
		$("#ConceptoFacturado_iva").on('focusout', function (e) {
			CalcularIva();
		});
		$(".inputEditable").on("keypress", analizaEnterInput);
		$("#ConceptoFacturado_concepto").focus();
		CerrarWaiting();
		return true
	});
}

function CalcularIva() {
	var sit_id = $("#listaIvaSit option:selected").val()
	var ali_id = $("#listaIvaAli option:selected").val()
	var subt = $("#ConceptoFacturado_subtotal").val();
	var num_subt = parseFloat(subt.replace(".", "").replace(",", "."));
	//var num_ali_id = parseFloat(ali_id);
	if (num_subt > 0 && sit_id == "G") {
		if (ali_id != "") {
			var calc = (num_subt * parseFloat(ali_id)) / 100;
			$("#ConceptoFacturado_iva").val(calc.toFixed(2));
			$("#ConceptoFacturado_total").val((num_subt + calc).toFixed(2));
		}
	}
	else if (num_subt > 0 && sit_id != "G") {
		var calc = num_subt / 100;
		$("#ConceptoFacturado_iva").val(calc);
		$("#ConceptoFacturado_total").val(num_subt + calc);
	}
}

function AgregarConceptoFacturado() {
	if (ValidarCamposModalIva()) {
		AbrirWaiting();
		var concepto = $("#ConceptoFacturado_concepto").val();
		var sit = $("#listaIvaSit").val();
		var ali = $("#listaIvaAli").val();
		var subt = $("#ConceptoFacturado_subtotal").val();
		var iva = $("#ConceptoFacturado_iva").val();
		var tot = $("#ConceptoFacturado_total").val();
		var data = { concepto, sit, ali, subt, iva, tot };
		PostGen(data, agregarConceptoFacturadoUrl, function (obj) {
			CerrarWaiting();
			if (obj.error === true) {
				ControlaMensajeError(obj.msg);
				//AbrirMensaje("ATENCIÓN", obj.msg, function () {
				//	$("#msjModal").modal("hide");
				//	return true;
				//}, false, ["Aceptar"], "error!", null);
			}
			else {
				//Actualizar grilla Conceptos facturados
				CargarGrillaConceptosFacturados();
				$('#modalCargaIVA').modal('hide');
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
	var data = {};
	PostGenHtml(data, cargarOtrosTributosUrl, function (obj) {
		$("#divOtrosTributos").html(obj);
		return true
	});
	//Grilla Totales
	var data = {};
	PostGenHtml(data, cargarGrillaTotalesUrl, function (obj) {
		$("#divTotales").html(obj);
		return true
	});
	//Depende si corresponde, grilla de Rpr Asociados o grilla A Cuentas Asociadas
}

function CargarGrillaConceptosFacturados() {
	var data = {};
	PostGenHtml(data, cargarCargarConceptosFacturadosUrl, function (obj) {
		$("#divConceptosFacturados").html(obj);
		return true
	});
}

function selectReg(x, gridId) {
	$("#" + gridId + " tbody tr").each(function (index) {
		$(this).removeClass("selected-row");
		$(this).removeClass("selectedEdit-row");
	});
	$(x).addClass("selected-row");

}

function quitarConceptoFacturado(e) {
}

function quitarOtroTributo(e) {
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