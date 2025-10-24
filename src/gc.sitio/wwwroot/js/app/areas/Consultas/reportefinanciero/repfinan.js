$(function () {
	$(document).on("click", "#btnBuscarProyeccionFinanciera", ControlaBuscarProyeccionFinanciera);
	$(document).on("click", "#btnBuscarSaldoDeCuenta", ControlaBuscarSaldoDeCuenta);
	$(document).on("click", "#btnBuscarFlujoDeIngreso", ControlaBuscarFlujoDeIngreso);
	$(document).on("click", "#btnBuscarProyeccionDeEgreso", ControlaBuscarProyeccionDeEgreso);

	$(document).on("click", "#btnImprimirProyeccionFinanciera", ControlaImprimirProyeccionFinanciera);
	$(document).on("click", "#btnImprimirSaldoDeCuenta", ControlaImprimirSaldoDeCuenta);

	$("#fechaHastaFlujoDeIngreso").on("change", function () {
		SetearLimitesFechaDesdeFlujoDeIngreso();
	});

	$('#tabsReporteFinanciero button[data-bs-toggle="tab"]').on('shown.bs.tab', function (event) {
		const tabId = $(event.target).attr('id'); // ID del botón clickeado
		const targetPane = $(event.target).data('bsTarget'); // Ej: #navs-top-home

		console.log('Tab activado:', tabId);
		console.log('Contenido mostrado:', targetPane);

		// Ejemplo: lógica condicional
		switch (tabId) {
			case 'btnTabProyeccionFinanciera':
				console.log("btnTabProyeccionFinanciera");
				SetearCamposProyeccionFinanciera();
				break;
			case 'btnTabSaldoDeCuenta':
				console.log("btnTabSaldoDeCuenta");
				SetearCamposSaldoDeCuenta();
				break;
			case 'btnTabFlujoDeIngreso':
				console.log("btnTabFlujoDeIngreso");
				SetearCamposFlujoDeIngreso();
				break;
			case 'btnTabProyeccionDeEgreso':
				console.log("btnTabProyeccionDeEgreso");
				SetearCamposProyeccionDeEgreso();
				break;
			// etc...
		}
	});
	SetearCamposProyeccionFinanciera();
});

///----------------------Proyeccion Financiera---------------------///
function ControlaBuscarProyeccionFinanciera() {
	AbrirWaiting();
	var desde = $("#fechaDesdeProyeccionFinanciera").val();
	var hasta = $("#fechaHastaProyeccionFinanciera").val();
	var data = { desde, hasta };
	PostGenHtml(data, buscarProyeccionFinancieraURL, function (obj) {
		CerrarWaiting();
		$("#divProyeccionFinanciera").html(obj);
		var filas = $("#tbGridProyFinan tbody tr").length;
		if (filas == 0) {
			AbrirMensaje("ATENCIÓN", "No hay datos de Proyección para el criterio de búsqueda.", function () {
				$("#msjModal").modal("hide");
				return true;
			}, false, ["Aceptar"], "error!", null);
		}
		return true
	}, function (obj) {
		ControlaMensajeError(obj.message);
		CerrarWaiting();
	});
}

function SetearCamposProyeccionFinanciera() {
	var now = moment().format('YYYY-MM-DD');
	var now2 = moment().subtract(30, 'days');
	$("#fechaDesdeProyeccionFinanciera").val(now2.format('YYYY-MM-DD'));
	$("#fechaHastaProyeccionFinanciera").val(now);

	const $div = $("#navs-top-home");

	$div.find("input").on("keydown", function (e) {
		if (e.key === "Enter") {
			e.preventDefault();

			const $campos = $div.find("input")
				.filter(":visible:enabled");

			const index = $campos.index(this);

			if (index !== -1) {
				if (index < $campos.length - 1) {
					$campos.eq(index + 1).focus();
				} else {
					$div.find("#btnBuscarProyeccionFinanciera").focus();
				}
			}
		}
	});
}

function ControlaImprimirProyeccionFinanciera() {
	if ($("#tbGridProyFinan > tbody > tr").length === 0) {
		AbrirMensaje("ATENCIÓN", "No hay datos generar el reporte.", function () {
			$("#msjModal").modal("hide");
			return true;
		}, false, ["Aceptar"], "error!", null);
	}
	else {
		AbrirWaiting();
		var tipoReporte = 1;
		var data = { tipoReporte };
		PostGen(data, setearTipoDeReporteUrl, function (obj) {
			CerrarWaiting();
			if (obj.error === true) {
				CerrarWaiting();
				AbrirMensaje("ATENCIÓN", obj.msg, function () {
					$("#msjModal").modal("hide");
					return true;
				}, false, ["Aceptar"], "error!", null);
			}
			else {
				HandlerImprimirListaProyeccionFinanciera();
			}
		});
	}
}

function HandlerImprimirListaProyeccionFinanciera() {
	ReseteoDeReportes();
	setTimeout(() => {
		var desde = $("#fechaDesdeProyeccionFinanciera").val();
		var hasta = $("#fechaHastaProyeccionFinanciera").val();
		var desde1Print = moment($("#fechaDesdeProyeccionFinanciera").val()).format('DD/MM/yyyy')
		var hasta2Print = moment($("#fechaHastaProyeccionFinanciera").val()).format('DD/MM/yyyy')

		let data = {
			desde, desde1Print,
			hasta, hasta2Print
		};
		cargarReporteEnArre(34, data, "PROYECCIÓN DE INGRESO", "", "");
		invocacionGestorDoc({});
	}, 500);
}
///----------------------FIN Proyeccion Financiera----------------///

///----------------------Saldo de Cuentas-------------------------///
function ControlaBuscarSaldoDeCuenta() {
	AbrirWaiting();
	var hasta = $("#fechaHastaSaldoDeCuenta").val();
	var data = { hasta };
	PostGenHtml(data, buscarSaldoDeCuentasURL, function (obj) {
		CerrarWaiting();
		$("#divSaldoDeCuenta").html(obj);
		var filas = $("#tbGridSaldoEnCuenta tbody tr").length;
		if (filas == 0) {
			AbrirMensaje("ATENCIÓN", "No hay datos de Proyección para el criterio de búsqueda.", function () {
				$("#msjModal").modal("hide");
				return true;
			}, false, ["Aceptar"], "error!", null);
		}
		return true
	}, function (obj) {
		ControlaMensajeError(obj.message);
		CerrarWaiting();
	});
}

function SetearCamposSaldoDeCuenta() {
	var now = moment().format('YYYY-MM-DD');
	$("#fechaHastaSaldoDeCuenta").val(now).attr("max", now);
}

function ControlaImprimirSaldoDeCuenta() {
	if ($("#tbGridSaldoEnCuenta > tbody > tr").length === 0) {
		AbrirMensaje("ATENCIÓN", "No hay datos generar el reporte.", function () {
			$("#msjModal").modal("hide");
			return true;
		}, false, ["Aceptar"], "error!", null);
	}
	else {
		AbrirWaiting();
		var tipoReporte = 2;
		var data = { tipoReporte };
		PostGen(data, setearTipoDeReporteUrl, function (obj) {
			CerrarWaiting();
			if (obj.error === true) {
				CerrarWaiting();
				AbrirMensaje("ATENCIÓN", obj.msg, function () {
					$("#msjModal").modal("hide");
					return true;
				}, false, ["Aceptar"], "error!", null);
			}
			else {
				HandlerImprimirListaSaldoDeCuenta();
			}
		});
	}
}

function HandlerImprimirListaSaldoDeCuenta() {
	ReseteoDeReportes();
	setTimeout(() => {
		var hasta = $("#fechaHastaSaldoDeCuenta").val();
		var hasta1Print = moment($("#fechaDesdeProyeccionFinanciera").val()).format('DD/MM/yyyy')

		let data = {
			hasta, hasta1Print
		};
		cargarReporteEnArre(35, data, "SALDO DE CUENTAS", "", "");
		invocacionGestorDoc({});
	}, 500);
}
///----------------------FIN Saldo de Cuentas---------------------///

///----------------------Flujo de Ingreso-------------------------///
function ControlaBuscarFlujoDeIngreso() {
	AbrirWaiting();
	var desde = $("#fechaDesdeFlujoDeIngreso").val();
	var hasta = $("#fechaHastaFlujoDeIngreso").val();
	var data = { desde, hasta };
	PostGenHtml(data, buscarFlujoDeIngresoURL, function (obj) {
		CerrarWaiting();
		$("#divFlujoDeIngreso").html(obj);
		var filas = $("#tbGridFlujoDeIngreso tbody tr").length;
		if (filas == 0) {
			AbrirMensaje("ATENCIÓN", "No hay datos de Proyección para el criterio de búsqueda.", function () {
				$("#msjModal").modal("hide");
				return true;
			}, false, ["Aceptar"], "error!", null);
		}
		return true
	}, function (obj) {
		ControlaMensajeError(obj.message);
		CerrarWaiting();
	});
}

function SetearCamposFlujoDeIngreso() {
	var now = moment().format('YYYY-MM-DD');
	var now2 = moment().subtract(30, 'days').format('YYYY-MM-DD');
	$("#fechaDesdeFlujoDeIngreso").val(now2);
	$("#fechaHastaFlujoDeIngreso").val(now).attr("max", now);

	SetearLimitesFechaDesdeFlujoDeIngreso();
}

function SetearLimitesFechaDesdeFlujoDeIngreso() {
	var fechaHastaStr = $("#fechaHastaFlujoDeIngreso").val();
	if (!fechaHastaStr) return;

	var fechaHasta = moment(fechaHastaStr, 'YYYY-MM-DD');
	var fechaMinima = moment(fechaHasta).subtract(30, 'days');

	$("#fechaDesdeFlujoDeIngreso")
		.attr("min", fechaMinima.format('YYYY-MM-DD'))
		.attr("max", fechaHasta.format('YYYY-MM-DD'));
}
///----------------------FIN Flujo de Ingreso---------------------///

///----------------------Proyección de Egresos--------------------///
function ControlaBuscarProyeccionDeEgreso() {
	AbrirWaiting();
	var data = {};
	PostGenHtml(data, buscarPoyeccionEgresoGroupURL, function (obj) {
		CerrarWaiting();
		$("#divProyeccionDeEgresoGroup").html(obj);
		var filas = $("#tbGridProyEgrGroup tbody tr").length;
		if (filas == 0) {
			AbrirMensaje("ATENCIÓN", "No hay datos de Proyección para el criterio de búsqueda.", function () {
				$("#msjModal").modal("hide");
				return true;
			}, false, ["Aceptar"], "error!", null);
		}
		return true
	}, function (obj) {
		ControlaMensajeError(obj.message);
		CerrarWaiting();
	});
}

function CargarDetalleProyeccionDeEgreso(fecha) {
	AbrirWaiting();
	var data = { fecha };
	PostGenHtml(data, buscarPoyeccionEgresoDetailURL, function (obj) {
		CerrarWaiting();
		$("#divProyeccionDeEgresoDetail").html(obj);
		var filas = $("#tbGridProyEgrDetail tbody tr").length;
		if (filas == 0) {
			AbrirMensaje("ATENCIÓN", "No hay datos de Proyección para el criterio de búsqueda.", function () {
				$("#msjModal").modal("hide");
				return true;
			}, false, ["Aceptar"], "error!", null);
		}
		return true
	}, function (obj) {
		ControlaMensajeError(obj.message);
		CerrarWaiting();
	});
}
///----------------------FIN Proyección de Egresos----------------///

function ReseteoDeReportes() {
	console.log("Reseto de reportes");
	ReporteResetArre();
}

function LimpiarDivs() {
	$("#divProyeccionFinanciera").empty();
	$("#divSaldoDeCuenta").empty();
	$("#divFlujoDeIngreso").empty();
	$("#divProyeccionDeEgresoGroup").empty();
	$("#divProyeccionDeEgresoDetail").empty();
}

function selectReg(x, gridId) {
	$("#" + gridId + " tbody tr").each(function (index) {
		$(this).removeClass("selected-row");
		$(this).removeClass("selectedEdit-row");
	});
	$(x).addClass("selected-row");
	if (gridId == "tbGridProyEgrGroup") {
		fecha = $(x).data("fecha");
		CargarDetalleProyeccionDeEgreso(fecha);
	}
}

function ActivarTabPorId(idBotonTab) {
	if (!idBotonTab) return;

	const botonTab = document.getElementById(idBotonTab);
	if (!botonTab) {
		console.warn(`No se encontró el botón con ID: ${idBotonTab}`);
		return;
	}

	const instanciaTab = bootstrap.Tab.getOrCreateInstance(botonTab);
	instanciaTab.show();
}


function SetearCamposProyeccionDeEgreso() { }