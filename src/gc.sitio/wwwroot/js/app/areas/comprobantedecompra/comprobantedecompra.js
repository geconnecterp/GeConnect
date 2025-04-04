$(function () {
	InicializaPantalla();
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
		CerrarWaiting();
		return true
	});
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
	var data = {};
	PostGenHtml(data, cargarCargarConceptosFacturadosUrl, function (obj) {
		$("#divConceptosFacturados").html(obj);
		return true
	});
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