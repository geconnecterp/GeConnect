$(function () {
	if ($("#listaIntervalo")) {
		$(document).on("change", "#listaIntervalo", ControlalistaIntervaloSelected);
	}
	$(document).on("click", "#btnAgregarValorOrigen", btnAgregarValorOrigenValidar);
	$(document).on("click", "#btnAgregarValorDestino", btnAgregarValorDestinoValidar);
	$(document).on("click", "#btnAbmCancelar", btnAbmCancelarControlar);
	$(document).on("click", "#btnAbmAceptar", btnAbmAceptarControlar);

	$("#UpdateValores").on("change", function () {
		if ($(this).val() == 'true') {
			CargarValoresDesdeObligYCredSeleccionados();
		}
	});

	// Botón de imprimir
	$(document).on("click", ".btnImprimir", function () {
		imprimirTRA();
	});

	$("#btnImprimirTemp").on("click", function () {
		ImprimirTRA_Generada("00-00118821");
	});

	InicializarCampos();
});

function imprimirTRA() {
	// Invocar gestor documental
	invocacionGestorDoc({});
}

function selectRegDblGrillaValores(x, grilla) {
	console.log(grilla);
	console.log(x);
	var orden = x.childNodes[7].innerText;
	var sourceSeleccionado = grilla;
	var data = { orden, sourceSeleccionado };
	PostGenHtml(data, actualizarGrillaValoresUrl, function (obj) {
		if (grilla == "tbListaOrigen") {
			$("#divOrigen").html(obj);
		}
		else {
			$("#divDestino").html(obj);
		}
		ActualizarTotales();
	});
}

const TypeIntervalo = {
	24: '1',
	48: '2',
	72: '3',
	Otros: '4'
}

function ImprimirTRA_Generada(traCompte) {
	let data = { tra_compte: traCompte };
	cargarReporteEnArre(25, data, "TRANSFERENCIA ENTRE CUENTAS", "", "");
	invocacionGestorDoc({});
}

function btnAbmAceptarControlar() {
	if ($("#concepto").val() == "") {
		AbrirMensaje("ATENCIÓN", "Debe especificar un valor válido para 'Concepto'.", function () {
			$("#msjModal").modal("hide");
			$("#concepto").trigger("focus");
			return true;
		}, false, ["Aceptar"], "error!", null);
	}
	else {
		var data = {};
		AbrirWaiting();
		PostGen(data, validarAntesDeGuardarURL, function (obj) {
			CerrarWaiting();
			if (obj.error === true) {
				AbrirMensaje("ATENCIÓN", obj.msg, function () {
					$("#msjModal").modal("hide");
					return true;
				}, false, ["Aceptar"], "error!", null);
			}
			else {
				AbrirMensaje("ATENCIÓN", "¿Confirma?", function (e) {
					$("#msjModal").modal("hide");
					switch (e) {
						case "SI": //Confirmar
							var ttra_id = $("#parametro_confirmacion").val();
							var tra_concepto = $("#concepto").val();
							if ($("#fechaAcreditacion").val() != undefined) {
								var tra_fecha = $("#fechaAcreditacion").val();
							}
							else {
								var tra_fecha = $("#fecha").val();
							}
							var data = { ttra_id, tra_concepto, tra_fecha };
							PostGen(data, confirmarTransferenciaUrl, function (obj) {
								if (obj.error === true) {
									AbrirMensaje("ATENCIÓN", obj.msg, function () {
										$("#msjModal").modal("hide");
										return true;
									}, false, ["Aceptar"], "error!", null);
								}
								else {
									AbrirMensaje("ATENCIÓN", obj.msg, function () {
										$("#msjModal").modal("hide");
										console.log(obj.id); //Tomar este valor para imprimir.
										ImprimirTRA_Generada(obj.id);
										btnAbmCancelarControlar();
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
}

function btnAbmCancelarControlar() {
	InicializarDatosEnSesion();
	InicializarCampos();
	setTimeout(() => {
		ActualizarGrillas();
	}, 250);
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

function ActualizarGrillas() {
	var data = {};
	PostGenHtml(data, recargarGrillaOrigenUrl, function (obj) {
		$("#divOrigen").html(obj);
	});
	PostGenHtml(data, recargarGrillaDestinoUrl, function (obj) {
		$("#divDestino").html(obj);
	});
}

function CargarValoresDesdeObligYCredSeleccionados() {
	var source = $("#parametro_confirmacion").val();
	var data = { source, sourceSeleccionado };
	PostGenHtml(data, cargarValoresUrl, function (obj) {
		if (sourceSeleccionado == "1") {
			$("#divOrigen").html(obj);
		}
		else {
			$("#divDestino").html(obj);
		}
		ActualizarTotales();
		CerrarWaiting();
	});
}

function ActualizarTotales() {
	var data = {};
	PostGen(data, actualizarTotalesUrl, function (obj) {
		if (obj.error === true) {
			AbrirMensaje("ATENCIÓN", obj.msg, function () {
				$("#msjModal").modal("hide");
				return true;
			}, false, ["Aceptar"], "error!", null);
		}
		else {
			$("#total_origen").val(obj.totalOrigen);
			$("#total_destino").val(obj.totalDestino);
		}
	});
}

function InicializarCampos() {
	const fechaInput = document.getElementById("fechaAcreditacion");
	if (fechaInput) {
		fechaInput.addEventListener("change", function () {
			const [year, month, day] = this.value.split("-");
			const fechaSeleccionada = new Date(year, month - 1, day); // Evita el desfase
			const dia = fechaSeleccionada.getDay(); // 0 = domingo, 6 = sábado
			var now = moment().format('yyyy-MM-DD');

			if (dia === 0 || dia === 6) {
				//alert("No se pueden seleccionar fines de semana.");
				ControlaMensajeWarning("No se pueden seleccionar fines de semana.");
				$("#fechaAcreditacion").val(now);
			}
		});
	}
	$("#concepto").val("");
	$("#total_origen").val(0);
	$("#total_destino").val(0);
	getMaskForMoneyType("#total_origen");
	getMaskForMoneyType("#total_destino");
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

function ControlalistaIntervaloSelected(e) {
	var fecha = $("#fechaAcreditacion").val();
	const [year, month, day] = fecha.split("-");
	const fechaSeleccionada = new Date(year, month - 1, day); // Evita el desfase
	if (e.currentTarget.value != TypeIntervalo.Otros) {
		var nuevaFecha = sumarDiasHabiles(fechaSeleccionada, e.currentTarget.value);
		$("#fechaAcreditacion").val(nuevaFecha);
	}
	else {
	}
}

function sumarDiasHabiles(fechaInicial, diasHabiles) {
	const resultado = new Date(fechaInicial);
	let diasSumados = 0;

	while (diasSumados < diasHabiles) {
		resultado.setDate(resultado.getDate() + 1);
		const diaSemana = resultado.getDay(); // 0 = domingo, 6 = sábado

		if (diaSemana !== 0 && diaSemana !== 6) {
			diasSumados++;
		}
	}

	return formatearFecha(resultado);
}

function formatearFecha(fecha) {
	const año = fecha.getFullYear();
	const mes = String(fecha.getMonth() + 1).padStart(2, '0'); // Meses van de 0 a 11
	const dia = String(fecha.getDate()).padStart(2, '0');

	return `${año}-${mes}-${dia}`;
}



function onChangeAcreditacion() {

}

//Abro modal de seleccion de valores
function btnAgregarValorOrigenValidar() {
	sourceSeleccionado = "1";
	var app = $("#parametro_valores_origen").val();
	var importe = 0;
	var valor_a_nombre_de = "";
	var valores = [];
	var data = { app, importe, valor_a_nombre_de, valores };
	invocarModalDeSeleccionDeValores(data);
}

function btnAgregarValorDestinoValidar() {
	var app = $("#parametro_valores_destino").val();
	var filas = $("#tbListaDestino tbody tr").length;
	if (app == "DPD" && filas >= 1) {
		AbrirMensaje("ATENCIÓN", "La cuanta destino solo puede cargar un solo valor.", function () {
			$("#msjModal").modal("hide");
			return true;
		}, false, ["Aceptar"], "error!", null);
	}
	else {
		sourceSeleccionado = "2";
		var importe = $("#total_origen").inputmask('unmaskedvalue');
		var valor_a_nombre_de = "";
		var valores = [];
		var data = { app, importe, valor_a_nombre_de, valores };
		invocarModalDeSeleccionDeValores(data);
	}
}