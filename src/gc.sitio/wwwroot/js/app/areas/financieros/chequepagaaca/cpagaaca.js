$(function () {
	$("input#txtCliente").on("click", function () {
		$("input#txtCliente").val("");
		$("#txtClienteItem").val("");
	});
	$("#chkCliente").on("click", function () {
		if ($("#chkCliente").is(":checked")) {
			$("#txtCliente").prop("disabled", false);
		}
		else {
			$("#txtCliente").prop("disabled", true);
			$("input#txtCliente").val("");
			$("#txtClienteItem").val("");
		}
	});
	$("#chkDocumentoEnCuenta").on("click", function () {
		if ($("#chkDocumentoEnCuenta").is(":checked")) {
			$("#chkCambioDeFechaDePresentacion").prop('checked', false);
			$("#chkCambioDeFechaDePresentacion").trigger("change");
		}
	});
	$("#chkCambioDeFechaDePresentacion").on("click", function () {
		if ($("#chkCambioDeFechaDePresentacion").is(":checked")) {
			$("#chkDocumentoEnCuenta").prop('checked', false);
			$("#chkDocumentoEnCuenta").trigger("change");
		}
	});
	$(document).on("click", "#btnBuscar", btnBuscarValidar);
	$(document).on("click", "#btnCancel", btnCancelValidar);
	$(document).on("click", "#btnAceptar", btnAceptarValidar);
});

function btnAceptarValidar() {
	if (dia_movi == "") {
		AbrirMensaje("ATENCIÓN", "Debe seleccionar un Cheque de Tercero en Cartera.", function () {
			$("#msjModal").modal("hide");
			return true;
		}, false, ["Aceptar"], "error!", null);
	}
	else {
		//TODO Marce: Mandar al BE: la fecha (en el caso de ser necesario), dia_movi, fc_compte, fc_item
		//El resto de los campos los completo en el BE
		AbrirMensaje("ATENCIÓN", "¿Confirma la carga del cheque de tercero en cartera seleccionado", function (e) {
			$("#msjModal").modal("hide");
			switch (e) {
				case "SI": //Confirmar
					AbrirWaiting("Confirmando la carga...");
					//Armado de request
					var fecha_valor = "";
					var data2 = {};
					if (cambioDeFecha) {
						fecha_valor = $("#fechaValor").val();
						data2 = { fecha_valor }
					}

					var data1 = { dia_movi, fc_compte, fc_item };
					var data = $.extend({}, data1, data2);
					PostGen(data, confirmarCargaDeChequeDeTerceroEnCarteraUrl, function (obj) {
						CerrarWaiting();
						if (obj.error === true) {
							AbrirMensaje("ATENCIÓN", obj.msg, function () {
								$("#msjModal").modal("hide");
								return true;
							}, false, ["Aceptar"], "error!", null);
						}
						else {
							if (obj.id == null || obj.id == undefined) {
								AbrirMensaje("ATENCIÓN", "Se ha producido un error al intentar obtener el identificador de la transferencia.", function () {
									$("#msjModal").modal("hide");
									return true;
								}, false, ["Aceptar"], "info!", null);
							}
							else {
								if (obj.id != "0") {
									AbrirMensaje("ATENCIÓN", obj.msg, function () {
										$("#msjModal").modal("hide");
										ImprimirTRA_Generada(obj.id);
										InicializarDatosEnSesion();
										btnCancelValidar();
										return true;
									}, false, ["Aceptar"], "info!", null);
								}
								else {
									AbrirMensaje("ATENCIÓN", obj.msg, function () {
										$("#msjModal").modal("hide");
										InicializarDatosEnSesion();
										btnCancelValidar();
										return true;
									}, false, ["Aceptar"], "info!", null);
								}
							}
						}
					});
					break;
				case "NO":
					break;
				default: //NO
					break;
			}
			return true;

		}, true, ["Aceptar", "Cancelar"], "question!", null);
	}
}

function ImprimirTRA_Generada(traCompte) {
	let data = { tra_compte: traCompte };
	cargarReporteEnArre(25, data, "TRANSFERENCIA ENTRE CUENTAS", "", "");
	invocacionGestorDoc({});
}

function btnCancelValidar() {
	AbrirWaiting();
	var data = {};
	PostGenHtml(data, cargarPasoUnoUrl, function (obj) {
		CerrarWaiting();
		dia_movi = "";
		fc_compte = "";
		fc_item = "";
		$("#divPrincipal").html(obj);
		$("#chkCliente").on("click", function () {
			if ($("#chkCliente").is(":checked")) {
				$("#txtCliente").prop("disabled", false);
			}
			else {
				$("#txtCliente").prop("disabled", true);
				$("input#txtCliente").val("");
				$("#txtClienteItem").val("");
			}
		})
		$("#txtCliente").autocomplete({
			source: function (request, response) {

				data = { prefix: request.term }; /*Rel01*/

				$.ajax({
					url: autoComClienteUrl,
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
				$("#txtClienteItem").val(ui.item.id);

				return true;
			}
		});
	});
}

function selectReg(x, gridId) {
	$("#" + gridId + " tbody tr").each(function (index) {
		$(this).removeClass("selected-row");
		$(this).removeClass("selectedEdit-row");
	});
	$(x).addClass("selected-row");
	dia_movi = x.childNodes[13].innerText;
	fc_compte = x.childNodes[15].innerText;
	fc_item = x.childNodes[17].innerText;
}

function onChangeFechaValor(x) {

}

function btnBuscarValidar() {
	var esValido = true;
	var ctaf_id = $("#ListaCuentaValoresEnCartera").val();
	var cta_id = $("#txtClienteItem").val();
	var ctaSeleccionada = $("#chkCliente")[0].checked;
	docEnCuenta = $("#chkDocumentoEnCuenta")[0].checked;
	cambioDeFecha = $("#chkCambioDeFechaDePresentacion")[0].checked;
	if (ctaf_id == "") {
		AbrirMensaje("ATENCIÓN", "Debe seleccionar una Cuenta de Valores en Cartera.", function () {
			$("#msjModal").modal("hide");
			$("#ListaCuentaValoresEnCartera").trigger("focus");
			esValido = false;
			return true;
		}, false, ["Aceptar"], "error!", null);
	}
	else if (ctaSeleccionada && cta_id == "") {
		AbrirMensaje("ATENCIÓN", "Debe seleccionar un Cliente.", function () {
			$("#msjModal").modal("hide");
			$("#txtCliente").trigger("focus");
			esValido = false;
			return true;
		}, false, ["Aceptar"], "error!", null);
	}
	else if (!docEnCuenta && !cambioDeFecha) {
		AbrirMensaje("ATENCIÓN", "Debe seleccionar entre 'Documento en Cuenta' y 'Cambio de Fecha de Presentación'.", function () {
			$("#msjModal").modal("hide");
			$("#chkDocumentoEnCuenta").trigger("focus");
			esValido = false;
			return true;
		}, false, ["Aceptar"], "error!", null);
	}
	else {
		CargarChequesDeTercerosEnCartera(ctaf_id, cta_id, docEnCuenta, cambioDeFecha);
	}
}

function InicializarDatosEnSesion() {
	var data = {};
	PostGen(data, inicializarDatosEnSesionURL, function (obj) {
		if (obj.error === true) {
			AbrirMensaje("ATENCIÓN", obj.msg, function () {
				$("#msjModal").modal("hide");
				return true;
			}, false, ["Aceptar"], "error!", null);
		}
		else {
			console.log("Se han limpiado las variables de sesión.")
		}
	});
}

function CargarChequesDeTercerosEnCartera(ctaf_id, cta_id, docEnCuenta, cambioDeFecha) {
	AbrirWaiting();
	var mostrarFecha = cambioDeFecha;
	if (cta_id == "") {
		cta_id = "%";
	}
	var data = { ctaf_id, cta_id, mostrarFecha, docEnCuenta };
	PostGenHtml(data, cargarChequesDeTercerosEnCarteraUrl, function (obj) {
		CerrarWaiting();
		$("#divPrincipal").html(obj);
	});
}

$("#txtCliente").autocomplete({
	source: function (request, response) {

		data = { prefix: request.term }; /*Rel01*/

		$.ajax({
			url: autoComClienteUrl,
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
		$("#txtClienteItem").val(ui.item.id);

		return true;
	}
});