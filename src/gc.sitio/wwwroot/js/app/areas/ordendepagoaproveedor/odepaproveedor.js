$(function () {
	$(document).on("click", "#btnCancelDesdeValidPrev", CancelDesdeValidPrev);
	$(document).on("click", "#btnAceptarDesdeValidPrev", AceptarDesdeValidPrev);
	$(document).on("click", "#btnSiguiente1", btnSiguiente1Validar);
	$(document).on("click", "#btnAnterior2", btnAnterior2Validar);
	$(document).on("click", "#btnAgregarValor", btnAgregarValorValidar);
	$(document).on("click", "#btnConfirmar2", btnConfirmar2Validar);
	//
	InicializaPantalla();
	$("#btnBuscar").on("click", function () {
		if (ctaIdSelected == "") {
			AbrirMensaje("ATENCIÓN", "Debe seleccionar una cuenta.", function () {
				$("#msjModal").modal("hide");
				$("input#Rel01").focus();
				return true;
			}, false, ["Aceptar"], "error!", null);
		}
		else {
			ValidarProveedor();
		}
	});
	$("#UpdateValores").on("change", function () {
		if ($(this).val() == 'true') {
			CargarValoresDesdeObligYCredSeleccionados();
			$("#CuentaObs").prop("disabled", false);
		}
	});
	$("#btnCancel").on("click", function () {
		InicializarDatosEnSesion();
		InicializaPantalla();
		LimpiarDatosDelFiltroInicial();
		$("#btnFiltro").trigger("click");
		$("#btnDetalle").trigger("click");
		$("#divDetalle").collapse("hide");
	});
});

function btnConfirmar2Validar() {
	ValidarPrevioAConfirmar();
}

function ValidarPrevioAConfirmar() {
	var data = {};
	PostGen(data, validarAntesDeConfirmarUrl, function (obj) {
		if (obj.error === true) {
			AbrirMensaje("ATENCIÓN", obj.msg, function () {
				$("#msjModal").modal("hide");
				return false;
			}, false, ["Aceptar"], "error!", null);
		}
		else {
			AbrirMensaje("ATENCIÓN", "¿Confirma la generación de la Orden de Pago a Proveedor?", function (e) {
				$("#msjModal").modal("hide");
				switch (e) {
					case "SI": //Confirmar
						var cta_obs = $("#CuentaObs").val();
						var opt_id = "";
						if (esPagoAnticipado)
							opt_id = "PA";
						else
							opt_id = "PP";
						var data = { cta_obs, opt_id };
						PostGen(data, confirmarOPaProveedorUrl, function (obj) {
							if (obj.error === true) {
								AbrirMensaje("ATENCIÓN", obj.msg, function () {
									$("#msjModal").modal("hide");
									return true;
								}, false, ["Aceptar"], "error!", null);
							}
							else {
								console.log(obj.id);
								AbrirMensaje("ATENCIÓN", obj.msg, function () {
									$("#msjModal").modal("hide");
									console.log(obj.id); //Tomar este valor para imprimir.
									btnAbmCancelar_click();
									return true;
								}, false, ["Aceptar"], "succ!", null);
							}
						});
						break;
					case "NO":
						break;
					default:
						break;
				}
				return true;
			}, true, ["Aceptar", "Cancelar"], "warn!", null);
		}
	});
}

//Abro modal de seleccion de valores
function btnAgregarValorValidar() {
	//TODO MARCE: Ver de donde saco los datos "importe" y "valor_a_nombre_de"
	var app = "OPP";
	var saldo = $("#txtDiferencias").val();
	saldo = saldo.replaceAll(".", "");
	saldo = saldo.replace(",", ".");
	var saldoN = Number(saldo);
	var importe = 0
	if (saldoN != NaN && saldoN >0)
		importe = saldoN;
	var valor_a_nombre_de = valorANombreDe;
	var valores = [];
	var data = { app, importe, valor_a_nombre_de, valores };
	invocarModalDeSeleccionDeValores(data);
}

//Me muevo al paso1
function btnAnterior2Validar() {
	AbrirWaiting("Cargando....");
	var data = {};
	PostGenHtml(data, cargarVistaObligacionesYCreditosPaso1Url, function (obj) {
		$("#divDetalle").html(obj);
		//Setear los valores de la cuenta seleccionada en la vista de obligaciones y creditos
		$("#CtaID").val(ctaIdSelected);
		$("#CtaDesc").val(ctaDescSelected);
		MostrarDatosDeCuenta(true);
		ActualizarTotalesSuperiores();
		ActualizarGrillaObligacionesInferior();
		ActualizarGrillaCreditosInferior();
		EvaluarBotonesWizzard();
		CerrarWaiting();
	});
}

//Me muevo al paso2
function btnSiguiente1Validar() {
	esPagoAnticipado = false;
	if ($("#tbListaObligacionesParaAgregar tbody tr").length == 0 && $("#tbListaCreditosParaAgregar tbody tr").length == 0) {
		esPagoAnticipado = true;
	}
	AbrirWaiting("Cargando....");
	var data = {};
	PostGenHtml(data, cargarVistaObligacionesYCreditosPaso2Url, function (obj) {
		$("#divDetalle").html(obj);
		//Setear los valores de la cuenta seleccionada en la vista de obligaciones y creditos
		$("#CtaID").val(ctaIdSelected);
		$("#CtaDesc").val(ctaDescSelected);
		MostrarDatosDeCuenta(true);
		CargarRetencionesDesdeObligYCredSeleccionados();
		CargarValoresDesdeObligYCredSeleccionados();
		setTimeout(() => {
			ActualizarTotalesSuperiores();
		}, 500);
		CerrarWaiting();
		if (esPagoAnticipado) {
			ControlaMensajeInfo("Es Pago Anticipado.");
		}
	});
}

function CargarRetencionesDesdeObligYCredSeleccionados() {
	var data = {};
	PostGenHtml(data, cargarRetencionesDesdeObligYCredSeleccionadosUrl, function (obj) {
		$("#divRetenciones").html(obj);
	});
}

function CargarValoresDesdeObligYCredSeleccionados() {
	var data = {};
	PostGenHtml(data, cargarValoresDesdeObligYCredSeleccionadosUrl, function (obj) {
		$("#divValores").html(obj);
		ActualizarTotalesSuperiores();
	});
}

const formatter = new Intl.NumberFormat('de-DE', {
	minimumFractionDigits: 2,
	maximumFractionDigits: 2
});

function CancelDesdeValidPrev() {
	var data = {};
	PostGen(data, cancelarDesdeGrillaDeValidacionesPreviasURL, function (obj) {
		if (obj.error === true) {
			AbrirMensaje("ATENCIÓN", obj.msg, function () {
				$("#msjModal").modal("hide");
				return true;
			}, false, ["Aceptar"], "error!", null);
		}
		else {
			InicializaPantalla();
			$("#divDetalle").collapse("hide");
		}
	});
}

function selectReg(x, gridId) {
	$("#" + gridId + " tbody tr").each(function (index) {
		$(this).removeClass("selected-row");
		$(this).removeClass("selectedEdit-row");
	});
	$(x).addClass("selected-row");
}

function selectRegDbl(x, gridId) {
	$("#MsgErrorEnCargarOSacarCreditos").val("");
	if (gridId == "tbListaObligaciones" || gridId == "tbListaCreditos") { //Agregar
		var cuota = x.childNodes[3].innerText;
		var dia_movi = x.childNodes[9].innerText;
		var cm_compte = x.childNodes[11].innerText;
		var tco_id = x.childNodes[13].innerText;
		var cta_id = x.childNodes[15].innerText;
		var accion = "C";
		var origen = gridId;
		var data = { cuota, dia_movi, cm_compte, tco_id, cta_id, accion, origen };
		AbrirWaiting("Cargando....");
		PostGenHtml(data, cargarOSacarObligacionesOCreditosUrl, function (obj) {
			if (gridId == "tbListaObligaciones") {
				$("#divObligacionesParaAgregar").html(obj);
				ActualizarGrillaObligacionesInferior();
				ActualizarGrillaCreditosSuperior();
				ActualizarGrillaCreditosInferior();
				ActualizarTotalesSuperiores();
				var msg = $("#MsgErrorEnCargarOSacarObligaciones").val();
			}
			else {
				$("#divCreditosParaAgregar").html(obj);
				ActualizarGrillaCreditosInferior();
				ActualizarGrillaObligacionesSuperior();
				ActualizarGrillaObligacionesInferior();
				ActualizarTotalesSuperiores();
				var msg = $("#MsgErrorEnCargarOSacarCreditos").val();
			}
			CerrarWaiting();
			if (msg != "") {
				AbrirMensaje("ATENCIÓN", msg, function () {
					$("#msjModal").modal("hide");
					return true;
				}, false, ["Aceptar"], "error!", null);
			}
			EvaluarBotonesWizzard();
		});
	}
	else { //Quitar
		var cuota = x.childNodes[3].innerText;
		var dia_movi = x.childNodes[11].innerText;
		var cm_compte = x.childNodes[13].innerText;
		var tco_id = x.childNodes[15].innerText;
		var cta_id = x.childNodes[17].innerText;
		var accion = "S";
		var origen = gridId;
		var data = { cuota, dia_movi, cm_compte, tco_id, cta_id, accion, origen };
		AbrirWaiting("Cargando....");
		PostGenHtml(data, cargarOSacarObligacionesOCreditosUrl, function (obj) {
			if (gridId == "tbListaObligacionesParaAgregar") {
				$("#divObligacionesParaAgregar").html(obj);
				ActualizarGrillaObligacionesInferior();
				ActualizarGrillaCreditosSuperior();
				ActualizarGrillaCreditosInferior();
				ActualizarTotalesSuperiores();
				var msg = $("#MsgErrorEnCargarOSacarObligaciones").val();
			}
			else {
				$("#divCreditosParaAgregar").html(obj);
				ActualizarGrillaCreditosInferior();
				ActualizarGrillaObligacionesSuperior();
				ActualizarGrillaObligacionesInferior();
				ActualizarTotalesSuperiores();
				var msg = $("#MsgErrorEnCargarOSacarCreditos").val();
			}
			CerrarWaiting();
			if (msg != "") {
				AbrirMensaje("ATENCIÓN", msg, function () {
					$("#msjModal").modal("hide");
					return true;
				}, false, ["Aceptar"], "error!", null);
			}
			EvaluarBotonesWizzard();
		});
	}
	CerrarWaiting();
}

function selectRegDblGrillaValores(x) {
	var orden = x.childNodes[7].innerText;
	var data = { orden };
	PostGenHtml(data, actualizarGrillaValoresUrl, function (obj) {
		$("#divValores").html(obj);
		//Actualizar totales
		ActualizarTotalesSuperiores();
	});
}

function EvaluarBotonesWizzard() {
	if ($("#tbListaObligacionesParaAgregar tbody tr").length >= $("#tbListaCreditosParaAgregar tbody tr").length) {
		$("#btnSiguiente1").prop("disabled", false);
	}
	else if ($("#tbListaObligacionesParaAgregar tbody tr").length == 0 && $("#tbListaCreditosParaAgregar tbody tr").length == 0) {
		$("#btnSiguiente1").prop("disabled", false);
	}
	else {
		$("#btnSiguiente1").prop("disabled", false);
	}

}

function ActualizarTotalesSuperiores() {
	PostGen(data, actualizarTotalesSuperioresUrl, function (obj) {
		if (obj.error === true) {
			ControlaMensajeError(obj.msg);
		}
		else {
			$("#txtObligACancelar").val(formatter.format(obj.data.obligacionesCancelar));
			$("#txtCredYValImputados").val(formatter.format(obj.data.credYValImputados));
			$("#txtDiferencias").val(formatter.format(obj.data.diferencia));
		}
	});
}

function ActualizarGrillaObligacionesSuperior() {
	PostGenHtml({}, actualizarGrillaObligacionesSuperiorUrl, function (obj) {
		$("#divObligacionesParaAgregar").html(obj);
	});
}

function ActualizarGrillaObligacionesInferior() {
	PostGenHtml({}, actualizarGrillaObligacionesInferiorUrl, function (obj) {
		$("#divObligaciones").html(obj);
	});
}

function ActualizarGrillaCreditosSuperior() {
	PostGenHtml({}, actualizarGrillaCreditosSuperiorUrl, function (obj) {
		$("#divCreditosParaAgregar").html(obj);
	});
}

function ActualizarGrillaCreditosInferior() {
	PostGenHtml({}, actualizarGrillaCreditosInferiorUrl, function (obj) {
		$("#divCreditos").html(obj);
	});
}

function AceptarDesdeValidPrev() {
	AbrirWaiting("Cargando....");
	var cta_id = ctaIdSelected;
	var data = { cta_id };
	PostGenHtml(data, cargarVistaObligacionesYCreditosURL, function (obj) {
		$("#divDetalle").html(obj);
		//Setear los valores de la cuenta seleccionada en la vista de obligaciones y creditos
		$("#CtaID").val(ctaIdSelected);
		$("#CtaDesc").val(ctaDescSelected);
		MostrarDatosDeCuenta(true);
		setTimeout(() => {
			CargarObligacionesOCreditos("Creditos"); //Créditos
		}, 500);
		setTimeout(() => {
			CargarObligacionesOCreditos("Obligaciones"); //Obligaciones
		}, 1000);
		$("#btnAbmCancelar").prop("disabled", false);
		$("#btnAbmCancelar").on("click", function () {
			InicializarDatosEnSesion();
			InicializaPantalla();
			LimpiarDatosDelFiltroInicial();
			$("#btnFiltro").trigger("click");
			$("#btnDetalle").trigger("click");
			$("#divDetalle").collapse("hide");
		});
		valorANombreDe = $("#valoresANombreDe").val();
		CerrarWaiting();
	});
}

function btnAbmCancelar_click() {
	InicializarDatosEnSesion();
	InicializaPantalla();
	LimpiarDatosDelFiltroInicial();
	$("#btnFiltro").trigger("click");
	$("#btnDetalle").trigger("click");
	$("#divDetalle").collapse("hide");

	/*
			InicializarDatosEnSesion();
		InicializaPantalla();
		LimpiarDatosDelFiltroInicial();
		$("#btnFiltro").trigger("click");
		$("#btnDetalle").trigger("click");
		$("#divDetalle").collapse("hide");
	*/
}

function CargarObligacionesOCreditos(tipo_carga) {
	AbrirWaiting("Cargando....");
	var data = { tipo_carga };
	PostGenHtml(data, cargarObligacionesOCreditosURL, function (obj) {
		CerrarWaiting();
		if (tipo_carga == "Obligaciones") {
			$("#divObligaciones").html(obj);
		}
		else {
			$("#divCreditos").html(obj);
		}
	});
}

function InicializaPantalla() {
	$("#chkRel01").prop('checked', true);
	$("#chkRel01").trigger("change");
	$("#chkRel01").prop("disabled", true);
	$("#Rel01").prop("disabled", false);
	$("input#Rel01").on("click", function () {
		$("input#Rel01").val("");
		$("#Rel01Item").val("");
	});
	$("input#Rel01").trigger("click");
	$("#Rel01List").empty();
	$("#Rel01").focus();
	$("#lbRel01").text("Proveedor");
	$("#btnDetalle").prop("disabled", true);
	$("#btnAbmCancelar").on("click", function () {
		InicializarDatosEnSesion();
		InicializaPantalla();
		LimpiarDatosDelFiltroInicial();
		$("#btnFiltro").trigger("click");
		$("#btnDetalle").trigger("click");
		$("#divDetalle").collapse("hide");
	});
	$("#btnAbmCancelar").prop("disabled", true);
	ctaIdSelected = "";
	MostrarDatosDeCuenta(false);
	$("#divFiltro").collapse("show")
	$("#divDetalle").collapse("hide");
}

function InicializarDatosEnSesion() {
	PostGen({}, inicializarDatosEnSesionURL, function (obj) {
		if (obj.error === true) {
			AbrirMensaje("ATENCIÓN", obj.msg, function () {
				$("#msjModal").modal("hide");
				return true;
			}, false, ["Aceptar"], "error!", null);
		}
	});
}

function LimpiarDatosDelFiltroInicial() {
	$("input#Rel01").val("");
	$("#Rel01Item").val("");
	$("#Rel01List").empty();
	$("#listaComptesPend").empty();
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

function ValidarProveedor() {
	var cta_id = ctaIdSelected;
	var data = { cta_id }
	PostGenHtml(data, validarProveedorURL, function (obj) {
		$("#divDetalle").html(obj);
		if ($("#tbListaValidacionesPrev tbody tr").length <= 0) {
			//Si la lista esta vacia, levanto la vista siguiente
			//Cargar con el backend la vista siguiente, y tirarla a divDetalle

			$("#divFiltro").collapse("hide");
			$("#divDetalle").collapse("show");
			$("#btnAceptarDesdeValidPrev").trigger("click");
		}
		else {
			//Si hay al menos un registro con 'true' en la tercer columna, no permito continuar
			var puedoContinuar = true;
			$("#tbListaValidacionesPrev").find('tr').each(function (i, el) {
				var td = $(this).find('td');
				if (td.length > 0) {
					if (td[2].innerText == "True") {
						puedoContinuar = false;
					}
				}
			});
			if (!puedoContinuar) {
				$("#btnAceptarDesdeValidPrev").prop("disabled", true);
			}
			$("#divFiltro").collapse("hide");
			$("#divDetalle").collapse("show");
		}
	});
}

function RestaurarGrillasInferioresParaPaso1() {
	setTimeout(() => {
		CargarObligacionesOCreditos("Obligaciones"); //Obligaciones
	}, 500);
	setTimeout(() => {
		CargarObligacionesOCreditos("Creditos"); //Créditos
	}, 500);
}