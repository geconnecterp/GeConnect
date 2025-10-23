$(function () {
	$(document).on("click", "#btnBuscarProyeccionFinanciera", ControlaBuscarProyeccionFinanciera);

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
///----------------------FIN Proyeccion Financiera----------------///

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

function SetearCamposProyeccionFinanciera() {
	var now = moment().format('yyyy-MM-DD');
	var now2 = moment().subtract(30, 'days');
	$("#fechaDesdeProyeccionFinanciera").val(now2.format('yyyy-MM-DD'));
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

function SetearCamposSaldoDeCuenta() {
	var now = moment().format('yyyy-MM-DD');
	$("#fechaHastaSaldoDeCuenta").val(now);
}

function SetearCamposFlujoDeIngreso() {
	var now = moment().format('yyyy-MM-DD');
	var now2 = moment().subtract(30, 'days');
	$("#fechaDesdeFlujoDeIngreso").val(now2.format('yyyy-MM-DD'));
	$("#fechaHastaFlujoDeIngreso").val(now);
}

function SetearCamposProyeccionDeEgreso() { }