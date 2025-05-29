$(function () {
	$(document).on("click", "#btnCancelDesdeValidPrev", CancelDesdeValidPrev);
	$(document).on("click", "#btnAceptarDesdeValidPrev", AceptarDesdeValidPrev);
	$(document).on("click", "#btnSiguiente1", btnSiguiente1Validar);
	$(document).on("click", "#btnAnterior2", btnAnterior2Validar);
	$(document).on("click", "#btnAgregarValor", btnAgregarValorValidar);
	//btnAgregarValor
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
});

//Abro modal de seleccion de valores
function btnAgregarValorValidar() {
	var app = "OPP";
	var importe = 0;
	var valor_a_nombre_de = "";
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
		RestaurarGrillasInferioresParaPaso1();
		EvaluarBotonesWizzard();
		CerrarWaiting();
	});
}

//Me muevo al paso2
function btnSiguiente1Validar() {
	if ($("#tbListaObligacionesParaAgregar tbody tr").length < $("#tbListaCreditosParaAgregar tbody tr").length) {
		AbrirMensaje("ATENCIÓN", "Deben existir mas elementos de Obligaciones/Débitos que créditos.", function () {
			$("#msjModal").modal("hide");
			return true;
		}, false, ["Aceptar"], "warn!", null);
	}
	//Validar que el total de obligaciones o débitos sea mayor o igual que el de los créditos
	else if ($("#tbListaObligacionesParaAgregar tbody tr").length >= $("#tbListaCreditosParaAgregar tbody tr").length) {
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
		});
	}
	else {
		//Si no existen obligaciones, debe suponer que es un “Pago Anticipado”, en ese caso debe advertirlo en el paso siguiente 
		//(tampoco deben existir créditos por defecto debería funcionar la validación anterior)
		if ($("#tbListaObligacionesParaAgregar tbody tr").length == 0 && $("#tbListaCreditosParaAgregar tbody tr").length == 0) {
			//TODO MARCE: seguir aca
		}
	}
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
	});
}

const formatter = new Intl.NumberFormat('en-US', {
	style: 'currency',
	currency: 'USD',
	trailingZeroDisplay: 'stripIfInteger'
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
				ActualizarTotalesSuperiores();
				var msg = $("#MsgErrorEnCargarOSacarObligaciones").val();
			}
			else {
				$("#divCreditosParaAgregar").html(obj);
				ActualizarGrillaCreditosInferior();
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
				ActualizarTotalesSuperiores();
				var msg = $("#MsgErrorEnCargarOSacarObligaciones").val();
			}
			else {
				$("#divCreditosParaAgregar").html(obj);
				ActualizarGrillaCreditosInferior();
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

function EvaluarBotonesWizzard() {
	if ($("#tbListaObligacionesParaAgregar tbody tr").length >= $("#tbListaCreditosParaAgregar tbody tr").length) {
		$("#btnSiguiente1").prop("disabled", false);
	}
	else if ($("#tbListaObligacionesParaAgregar tbody tr").length == 0 && $("#tbListaCreditosParaAgregar tbody tr").length == 0) {
		$("#btnSiguiente1").prop("disabled", false);
	}
	else {
		$("#btnSiguiente1").prop("disabled", true);
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

function ActualizarGrillaObligacionesInferior() {
	PostGenHtml({}, actualizarGrillaObligacionesInferiorUrl, function (obj) {
		$("#divObligaciones").html(obj);
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
		CerrarWaiting();
		$("#divDetalle").html(obj);
		//Setear los valores de la cuenta seleccionada en la vista de obligaciones y creditos
		$("#CtaID").val(ctaIdSelected);
		$("#CtaDesc").val(ctaDescSelected);
		MostrarDatosDeCuenta(true);
		setTimeout(() => {
			CargarObligacionesOCreditos("D"); //Obligaciones
		}, 500);
		setTimeout(() => {
			CargarObligacionesOCreditos("H"); //Créditos
		}, 500);
		$("#btnAbmCancelar").prop("disabled", false);
	});
}

function CargarObligacionesOCreditos(tipo) {
	AbrirWaiting("Cargando....");
	var data = { tipo };
	PostGen(data, cargarObligacionesOCreditosURL, function (obj) {
		CerrarWaiting();
		if (tipo == "D") {
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
}

function InicializarDatosEnSesion() {
	PostGen({}, inicializarDatosEnSesionURL, function (obj) {
		if (obj.error === true) {
			AbrirMensaje("ATENCIÓN", obj.msg, function () {
				$("#msjModal").modal("hide");
				return true;
			}, false, ["Aceptar"], "error!", null);
		}
		else {
			console.log(obj.msg);
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
		CargarObligacionesOCreditos("D"); //Obligaciones
	}, 500);
	setTimeout(() => {
		CargarObligacionesOCreditos("H"); //Créditos
	}, 500);
}