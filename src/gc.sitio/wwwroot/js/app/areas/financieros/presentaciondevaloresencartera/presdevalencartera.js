$(function () {
	$(document).on("click", "#btnSiguiente1", btnSiguiente1Validar);
	$(document).on("click", "#btnSiguiente2", btnSiguiente2Validar);
	$(document).on("click", "#btnSiguiente3", btnSiguiente3Validar);
	$(document).on("click", "#btnAbmCancelar1", btnAbmCancelar1Validar);
	$(document).on("click", "#btnAnterior2", btnAnterior2Validar);
	$(document).on("click", "#btnAnterior3", btnAnterior3Validar);
	$(document).on("click", "#btnAnterior4", btnAnterior4Validar);
	$(document).on("click", "#btnConfirmar1", btnConfirmar1Validar);
	//
	$(document).on('change', 'tbValores input[type="checkbox"]', function () {
		ActualizarTotalSeleccionado();
	});

});

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

function btnAbmCancelar1Validar() {
	AbrirWaiting("");
	InicializarDatosEnSesion();
	setTimeout(() => {
		InicializaPantalla()
		CerrarWaiting();
	}, 500);
}

function btnConfirmar1Validar() {
	if ($("#concepto").val() == "") {
		AbrirMensaje("ATENCIÓN", "Ingresar un valor en Comcepto.", function () {
			$("#msjModal").modal("hide");
			$("#concepto").trigger("focus");
			return true;
		}, false, ["Aceptar"], "error!", null);
	}
	else {
		AbrirMensaje("ATENCIÓN", "¿Confirma la presentación de valores", function (e) {
			$("#msjModal").modal("hide");
			switch (e) {
				case "SI": //Confirmar
					//Armado de request
					var concepto = $("#concepto").val();
					var fecha_acreditacion = $("#fechaAcreditacion").val();
					var ctaf_id_al_cobro = $("#ctaf_id_al_cobro").val();
					var ctaf_desc_al_cobro = $("#ctaf_desc_al_cobro").val();

					var data = { concepto, fecha_acreditacion, ctaf_id_al_cobro, ctaf_desc_al_cobro };
					PostGen(data, confirmarPresentacionDeValoresUrl, function (obj) {
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
								ImprimirTRA_Generada(obj.id);
								///Aca hay que inicializar todo
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

function InicializarDatosEnSesion() {
	PostGen({}, inicializarDatosEnSesionUrl, function (obj) {
		if (obj.error === true) {
			AbrirMensaje("ATENCIÓN", obj.msg, function () {
				$("#msjModal").modal("hide");
				return true;
			}, false, ["Aceptar"], "error!", null);
		}
	});
}

//function InicializaPantalla() {
//	var data = {};
//	PostGenHtml(data, paso1Url, function (obj) {
//		$("#divPrincipal").html(obj);
//	});
//}

function InicializaPantalla() {
	var data = {};
	PostGenHtml(data, paso1Url, function (obj) {
		$("#divPrincipal").html(obj);
	});
}

function btnSiguiente1Validar() {
	var tipo = $("#listaTipoMedioPago").val();
	if (tipo && tipo != "") {
		var tcf_id = tipo;
		tipoMedioPagoSelected = tipo;
		var data = { tcf_id };
		PostGenHtml(data, seleccionCuentaFinUrl, function (obj) {
			$("#divPrincipal").html(obj);
		});
	}
	else {
		AbrirMensaje("ATENCIÓN", "Debe seleccionar un Tipo de Medio de Pago.", function () {
			$("#msjModal").modal("hide");
			$("#listaTipoMedioPago").trigger("focus");
			return true;
		}, false, ["Aceptar"], "error!", null);
	}
}

function selectReg(x, gridId) {
	$("#" + gridId + " tbody tr").each(function (index) {
		$(this).removeClass("selected-row");
		$(this).removeClass("selectedEdit-row");
	});
	$(x).addClass("selected-row");
	if (gridId == "tbCtaFin") {
		ctafIdSelected = x.childNodes[1].innerText;
		ctafDescSelected = x.childNodes[3].innerText;
		var saldo = Number(x.childNodes[5].innerText.replace(',', ''));
		if (saldo <= 0) {
			$("#btnSiguiente2").prop("disabled", true);
		}
		else {
			$("#btnSiguiente2").prop("disabled", false);
		}
		saldo_de_ctaf = saldo;
	}
}

function btnSiguiente2Validar() {
	if (ctafIdSelected == "") {
		AbrirMensaje("ATENCIÓN", "Debe seleccionar una cuenta financiera.", function () {
			$("#msjModal").modal("hide");
			return true;
		}, false, ["Aceptar"], "error!", null);
	}
	else {
		var ctaf_id = ctafIdSelected;
		var ctaf_desc = ctafDescSelected;
		var data = { ctaf_id, ctaf_desc };
		PostGenHtml(data, seleccionValoresAPresentarUrl, function (obj) {
			$("#divPrincipal").html(obj);
			AgregarHandlerAGrillaPresDeValores();
		});
	}
}

function btnSiguiente3Validar() {
	var ctafIdLista = "";
	var unoSele = false;
	$("#tbValores").find('tr').each(function (i, el) {
		var td = $(this).find('td');
		if (td.eq(0)[0]) {
			if (td.eq(6)[0].children[0].checked) {
				unoSele = true;
				ctafIdLista += td.eq(7)[0].innerText + "|" + td.eq(8)[0].innerText + "|" + td.eq(9)[0].innerText + ",";
			}
		}
	});
	if (!unoSele) {
		AbrirMensaje("ATENCIÓN", "Debe al menos seleccionar un Valor en Cartera.", function () {
			$("#msjModal").modal("hide");
			return true;
		}, false, ["Aceptar"], "error!", null);
	}
	else {
		var totalSeleccionadoEnCartera = total_seleccionado_en_cartera;
		var saldoDeCtaf = saldo_de_ctaf;
		var tcfIdSelected = tipoMedioPagoSelected;
		var data = { totalSeleccionadoEnCartera, saldoDeCtaf, ctafIdSelected, ctafDescSelected, ctafIdLista, tcfIdSelected };
		PostGenHtml(data, detalleDePresentacionUrl, function (obj) {
			$("#divPrincipal").html(obj);
			var now = moment().format('yyyy-MM-DD');
			var max = moment().add(2, 'months');
			$("#fechaAcreditacion").attr('min', now);
			$("#fechaAcreditacion").attr('max', max.format('yyyy-MM-DD'));
			$("#saldo_cuenta_en_cartera").val(formatter.format($("#saldo_cuenta_en_cartera").val()));
			$("#importe_a_presentar_en_cartera").val(formatter.format($("#importe_a_presentar_en_cartera").val()));
			$("#saldo_a_constituir_en_cartera").val(formatter.format($("#saldo_a_constituir_en_cartera").val()));

			$("#saldo_cuenta_al_cobro").val(formatter.format($("#saldo_cuenta_al_cobro").val()));
			$("#importe_a_presentar_al_cobro").val(formatter.format($("#importe_a_presentar_al_cobro").val()));
			$("#saldo_a_constituir_al_cobro").val(formatter.format($("#saldo_a_constituir_al_cobro").val()));
		});
	}
}

const formatter = new Intl.NumberFormat('de-DE', {
	minimumFractionDigits: 2,
	maximumFractionDigits: 2
});

function ActualizarTotalSeleccionado() {
	var count = 0;
	var dataTable = document.getElementById('tbValores');
	var checkItems = dataTable.querySelectorAll('input[type="checkbox"]');
	if (checkItems) {
		checkItems.forEach(function (input) {
			if (input.checked) {
				count++;
			}
		});
		console.log(count);
	}
	var listaProd = "";
	total_seleccionado_en_cartera = 0;
	$("#tbValores").find('tr').each(function (i, el) {
		var td = $(this).find('td');
		if (td.eq(0)[0]) {
			if (td.eq(6)[0].children[0].checked) {
				total_seleccionado_en_cartera = total_seleccionado_en_cartera + Number(td.eq(5).text().replace(',', ''));
			}
		}
	});
	$("#total").val(formatter.format(total_seleccionado_en_cartera));
}

function AgregarHandlerAGrillaPresDeValores() {
	var dataTable = document.getElementById('tbValores');
	if (dataTable != null) {
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
		var checkItems = dataTable.querySelectorAll('input[type="checkbox"]');
		if (checkItems) {
			checkItems.forEach(function (input) {
				input.addEventListener('change', function () {
					ActualizarTotalSeleccionado();
				});
			});
		}
	}
}

function btnAnterior2Validar() {
	var data = {};
	PostGenHtml(data, paso1Url, function (obj) {
		$("#divPrincipal").html(obj);
	});
}

function btnAnterior3Validar() {
	var tcf_id = tipoMedioPagoSelected;
	var data = { tcf_id };
	PostGenHtml(data, seleccionCuentaFinUrl, function (obj) {
		$("#divPrincipal").html(obj);
		ctafIdSelected = "";
		ctafDescSelected = "";
	});
}

function btnAnterior4Validar() {
	var ctaf_id = ctafIdSelected;
	var ctaf_desc = ctafDescSelected;
	var data = { ctaf_id, ctaf_desc };
	PostGenHtml(data, seleccionValoresAPresentarUrl, function (obj) {
		$("#divPrincipal").html(obj);
		AgregarHandlerAGrillaPresDeValores();
	});
}

function onChangeAcreditacion(x) {

}