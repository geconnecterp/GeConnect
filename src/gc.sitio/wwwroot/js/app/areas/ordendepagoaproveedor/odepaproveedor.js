$(function () {
	$(document).on("click", "#btnCancelDesdeValidPrev", CancelDesdeValidPrev);
	$(document).on("click", "#btnAceptarDesdeValidPrev", AceptarDesdeValidPrev);
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
				var msg = $("#MsgErrorEnCargarOSacarObligaciones").val();
			}
			else {
				$("#divCreditosParaAgregar").html(obj);
				ActualizarGrillaCreditosInferior();
				var msg = $("#MsgErrorEnCargarOSacarCreditos").val();
			}
			if (msg != "") {
				AbrirMensaje("ATENCIÓN", msg, function () {
					$("#msjModal").modal("hide");
					return true;
				}, false, ["Aceptar"], "error!", null);
			}
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
		PostGenHtml(data, cargarOSacarObligacionesOCreditosUrl, function (obj) {
			if (gridId == "tbListaObligacionesParaAgregar") {
				$("#divObligacionesParaAgregar").html(obj);
				ActualizarGrillaObligacionesInferior();
				var msg = $("#MsgErrorEnCargarOSacarObligaciones").val();
			}
			else {
				$("#divCreditosParaAgregar").html(obj);
				ActualizarGrillaCreditosInferior();
				var msg = $("#MsgErrorEnCargarOSacarCreditos").val();
			}
			if (msg != "") {
				AbrirMensaje("ATENCIÓN", msg, function () {
					$("#msjModal").modal("hide");
					return true;
				}, false, ["Aceptar"], "error!", null);
			}
		});
	}
	CerrarWaiting();
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
	ctaIdSelected = "";
	MostrarDatosDeCuenta(false);
	$("#divFiltro").collapse("show")
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