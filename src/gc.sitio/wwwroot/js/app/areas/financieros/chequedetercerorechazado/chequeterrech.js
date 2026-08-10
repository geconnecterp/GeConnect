$(function () {
	InicializarCampos();
	$(document).on("click", "#btnBuscar", btnBuscarValidar);
	$(document).on("change", "#ListaCuentaBancaria", ControlaListaCuentaBancariaSelected);
	$(document).on("click", "#btnCancel", btnCancelValidar);
	$(document).on("click", "#btnAceptar", btnAceptarValidar);
	//
});

function btnAceptarValidar() {
	// 🔥 VALIDACIÓN NUEVA: La tabla debe tener al menos un registro
	var cantRegistros = $("#tbListaChequesDepositados tbody tr").length
	if (cantRegistros === 0) {
		AbrirMensaje("ATENCIÓN", "No existen registros para confirmar.", function () {
			$("#msjModal").modal("hide");
			return true;
		}, false, ["Aceptar"], "error!", null);

		return; // 🔥 Detiene la ejecución
	}

	if (conciliado == "True" || rechazado == "True") {
		AbrirMensaje("ATENCIÓN", "El valor seleccionado no puede ser rechazado.", function () {
			$("#msjModal").modal("hide");
			$("#ListaCuentaBancaria").trigger("focus");
			return true;
		}, false, ["Aceptar"], "error!", null);
	}
	else {
		AbrirMensaje("ATENCIÓN", "¿Confirma el rechazo del valor seleccionado?", function (e) {
			$("#msjModal").modal("hide");
			switch (e) {
				case "SI": //Confirmar
					var fechaRechazo = $("#fechaRechazado").val();
					var data = { tra_compte_selected, fc_dia_movi_selected, fc_compte_selected, fc_item_selected, fechaRechazo };
					PostGen(data, confirmarRechazoDeValorUrl, function (obj) {
						if (obj.error === true) {
							AbrirMensaje("ATENCIÓN", obj.msg, function () {
								$("#msjModal").modal("hide");
								return true;
							}, false, ["Aceptar"], "error!", null);
						}
						else {
							// MOstrar mensaje
							AbrirMensaje("ATENCIÓN", obj.msg, function () {
								$("#msjModal").modal("hide");
								///Aca hay que inicializar todo
								ImprimirTRA_Generada(obj.id);
								InicializarDatosEnSesion();
								InicializaPantalla();
								return true;
							}, false, ["Aceptar"], "info!", null);
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

function ReseteoDeReportes() {
	console.log("Reseto de reportes");
	ReporteResetArre();
}

function ImprimirTRA_Generada(traCompte) {
	ReseteoDeReportes();
	setTimeout(() => {
		let data = { tra_compte: traCompte };
		cargarReporteEnArre(25, data, "TRANSFERENCIA ENTRE CUENTAS", "", "");
		invocacionGestorDoc({});
	}, 500);
}

function InicializaPantalla() {
	var data = {};
	PostGenHtml(data, paso1Url, function (obj) {
		$("#divPrincipal").html(obj);
		$("#divUno").removeClass("col-2").addClass("col-4");
		$("#divDos").removeClass("col-2").addClass("col-4");
		$("#divPrincipal").removeClass("col-8").addClass("col-4");
		InicializarCampos();
	});
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

function btnCancelValidar() {
	var data = {};
	PostGenHtml(data, volverPasoUnoUrl, function (obj) {
		$("#divPrincipal").html(obj);
		InicializarCampos();
		$(document).on("click", "#btnCancel", btnCancelValidar);
	});
}

function btnBuscarValidar() {
	let valFechas = ValidarFechasEnIndex();
	if (CuentaBancariaSelected == "") {
		AbrirMensaje("ATENCIÓN", "Debe seleccionar una cuenta bancaria.", function () {
			$("#msjModal").modal("hide");
			$("#ListaCuentaBancaria").trigger("focus");
			return true;
		}, false, ["Aceptar"], "error!", null);
	}
	else if (valFechas) {
		return false;
	}
	else {
		var fechaDesde = $("#FechaDesde").val();
		var fechaHasta = $("#FechaHasta").val();
		if (fechaDesde == "" || fechaHasta == "") {
			AbrirMensaje("ATENCIÓN", "Debe ingresar ambas fechas.", function () {
				$("#msjModal").modal("hide");
				if (fechaDesde == "") {
					$("#fechaDesde").trigger("focus");
				}
				else {
					$("#fechaHasta").trigger("focus");
				}
				return true;
			}, false, ["Aceptar"], "error!", null);
		}
		else {
			if (moment(fechaDesde).isAfter(moment(fechaHasta))) {
				AbrirMensaje("ATENCIÓN", "La fecha 'Desde' no puede ser mayor a la fecha 'Hasta'.", function () {
					$("#msjModal").modal("hide");
					$("#fechaDesde").trigger("focus");
					return true;
				}, false, ["Aceptar"], "error!", null);
			}
			else {
				AbrirWaiting("Buscando Cheques...");
				var data = { ctaf_id: CuentaBancariaSelected, fechaDesde, fechaHasta };
				PostGenHtml(data, buscarChequesDepositadosUrl, function (obj) {
					// $("#divUno").removeClass("col-4").addClass("col-2");
					// $("#divDos").removeClass("col-4").addClass("col-2");
					$("#divPrincipal").html(obj);
					// $("#divPrincipal").removeClass("col-4").addClass("col-8");
					// Centrar el div
					// $("#divPrincipal").parent().addClass("d-flex justify-content-center");
					EstablecerValoresLimites();
					$(document).on("click", "#btnCancel", btnCancelValidar);
					CerrarWaiting();
				});
			}
		}
	}
}

function ControlaListaCuentaBancariaSelected() {
	CuentaBancariaSelected = $("#ListaCuentaBancaria").val();
}

function EstablecerValoresLimites() {
	var now = moment().format('yyyy-MM-DD');
	var now2 = moment().subtract(30, 'days');
	$("#fechaRechazado").attr('min', now2.format('yyyy-MM-DD'));
	$("#fechaRechazado").attr('max', now);
	$("#fechaRechazado").val(now);
}

function InicializarCampos() {
	// var now = moment().format('yyyy-MM-DD');
	// $("#fechaHasta").val(now);
	// var now2 = moment().subtract(7, 'days');
	$("#chkCuentaBanco").prop('checked', true);
	$("#chkCuentaBanco").trigger("change");
	$("#chkDesdeHasta").prop('checked', true);
	$("#chkDesdeHasta").trigger("change");
	$("#FechaDesde").prop("disabled", false);
	$("#FechaHasta").prop("disabled", false);
	$("#chkCuentaBanco").prop("disabled", true);
	$("#chkDesdeHasta").prop("disabled", true);
	$("#chkDesdeHasta").on("click", function () {
		if ($("#chkDesdeHasta").is(":checked")) {
			$("#FechaDesde").prop("disabled", false);
			$("#FechaHasta").prop("disabled", false);
			
		}
		else {
			$("#FechaDesde").prop("disabled", true);
			$("#FechaHasta").prop("disabled", true);
		}
	});
	$("#chkCuentaBanco").on("click", function () {
		if ($("#chkCuentaBanco").is(":checked")) {
			$("#ListaCuentaBancaria").prop("disabled", false);

		}
		else {
			$("#ListaCuentaBancaria").prop("disabled", true);
		}
	});
}

function onChangeFechaRechazado(x) {

}

function onChangeFechaDesde(x) {
	//ValidarFechasEnIndex();
}

function onChangeFechaHasta(x) {
	//ValidarFechasEnIndex();
}

function ValidarFechasEnIndex() {
	var fechaDesde = $("#FechaDesde").val();
	var fechaHasta = $("#FechaHasta").val();
	if (fechaDesde == "" || fechaHasta == "") {
		AbrirMensaje("ATENCIÓN", "Debe ingresar ambas fechas.", function () {
			$("#msjModal").modal("hide");
			if (fechaDesde == "") {
				$("#fechaDesde").trigger("focus");
			}
			else {
				$("#fechaHasta").trigger("focus");
			}
			return true;
		}, false, ["Aceptar"], "error!", null);
	}
	else {
		if (moment(fechaDesde).isAfter(moment(fechaHasta))) {
			AbrirMensaje("ATENCIÓN", "La fecha 'Desde' no puede ser mayor a la fecha 'Hasta'.", function () {
				$("#msjModal").modal("hide");
				$("#fechaDesde").trigger("focus");
				return true;
			}, false, ["Aceptar"], "error!", null);
		}
	}
	return false;
}

function selectReg(x, gridId) {
	$("#" + gridId + " tbody tr").each(function (index) {
		$(this).removeClass("selected-row");
		$(this).removeClass("selectedEdit-row");
	});
	$(x).addClass("selected-row");
	if (gridId == "tbListaChequesDepositados") {
		tra_compte_selected = x.childNodes[1].innerText;
		fc_dia_movi_selected = x.childNodes[19].innerText;
		fc_compte_selected = x.childNodes[21].innerText;
		fc_item_selected = x.childNodes[23].innerText;
		conciliado = x.childNodes[25].innerText;
		rechazado = x.childNodes[27].innerText;
	}
}