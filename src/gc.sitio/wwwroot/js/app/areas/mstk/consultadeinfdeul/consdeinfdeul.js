$(function () {
	$("#rbULPorFecha, #rbULSinAlmacen").on("change", actualizarFechas);

	actualizarFechas();

	$("#btnFiltro").on("click", function () {
		if ($("#divFiltros").hasClass("show")) {
			$("#divFiltros").collapse("hide");
			$("#divDetalle").collapse("show");
			autoEvalTabVisible();
		}
		else {
			$("#divFiltros").collapse("show");
			$("#divDetalle").collapse("hide");
			$("#btnImprimir").hide();
		}
	});

	$("#btnBuscar").on("click", function () {
		const [ok, msg] = ValidarFechasFiltro();
		if (!ok) {
			AbrirMensaje("ATENCIÓN", msg, function () {
				$("#msjModal").modal("hide");
				return true;
			}, false, ["Aceptar"], "error!", null);
		}
		else {
			try { MostrarFiltrosAplicados(); } catch (e) { }
			InicializarPantallaPrincipal();
		}
	});

	$("#divFiltros").collapse("show");
	$("#btnImprimir").hide();
});

try { MostrarFiltrosAplicados(); } catch (e) { }

function InicializarPantallaPrincipal() {
	var radioText = obtenerOpcionULSeleccionadaParaLeyenda();

	fechadesde = $("#Desde").val();
	fechahasta = $("#Hasta").val();
	AbrirWaiting("Cargando información...");
	PostGenHtml({ radioText, fechadesde, fechahasta }, inicializarPantallPrincipalURL, function (obj) {
		$("#divDetalle").html(obj);
		try { MostrarFiltrosAplicados(); } catch (e) { };
		$(document).on('shown.bs.tab', 'button[data-bs-toggle="tab"]', function (e) {
			const tabId = $(e.target).attr("data-bs-target").replace("#", "");
			EvaluarBotonImprimir(tabId);
		});
		$("#divFiltros").collapse("hide");
		$("#divDetalle").collapse("show");
		CerrarWaiting();
		setTimeout(() => {
			BuscarUnidadesDeLectura();
		}, 100);
		return true
	});
}

function autoEvalTabVisible() {
	const selector = 'button[data-bs-toggle="tab"]';

	$(document)
		.off('shown.bs.tab.autoEval', selector)
		.on('shown.bs.tab.autoEval', selector, function (e) {
			const tabId = $(e.target).attr("data-bs-target").replace("#", "");
			EvaluarBotonImprimir(tabId);
		});
}


const TabToTableMapUL = {
	"navs-top-ul": "#tbUL",
	"navs-top-det": "#tbULDetalle"
};

function EvaluarBotonImprimir(tabId) {
	console.log("Evaluando botón imprimir para tab:", tabId);
	const tablaSelector = TabToTableMapUL[tabId];
	if (!tablaSelector) {
		console.log("tablaSelector:", tablaSelector);
		$("#btnImprimir").hide();
		return;
	}

	const $tabla = $(tablaSelector);

	// Si la tabla no existe o no tiene filas de datos
	if ($tabla.length === 0 || $tabla.find("tbody tr").length === 0) {
		console.log("$tabla.length:", $tabla.length);
		console.log("$tabla.find(tbody tr).length:", $tabla.find("tbody tr").length);
		$("#btnImprimir").hide();
		return;
	}

	// Si tiene datos → mostrar botón
	//$("#btnImprimir").show(); => DESCOMENTAR ESTA LINEA SI HAY QUE IMPRIMIR REPORTE, LA LOGICA DE MOSTRAR O NO EL BOTON YA ESTA CONSTRUIDA
	$("#btnImprimir").hide();

	// Guardamos el tab actual para imprimir
	$("#btnImprimir").data("tab-activo", tabId);
}

function MostrarFiltrosAplicados() {
	const $target = $("#filtrosAplicadosFloating").length
		? $("#filtrosAplicadosFloating")
		: $("#filtrosAplicadosContainer");

	if ($target.length === 0) return;

	const desde = $("#FechaDesde").val();
	const hasta = $("#FechaHasta").val();

	let html = `
        <div class="d-inline-flex align-items-center"
             style="gap:8px; white-space:nowrap;">
    `;

	var mostrarTipo = obtenerOpcionULSeleccionada();
	if (mostrarTipo == "F") {
		if (desde)
			html += `<span class="badge bg-secondary">DESDE: ${desde}</span>`;

		if (hasta)
			html += `<span class="badge bg-secondary">HASTA: ${hasta}</span>`;
	}
	
	var tipo = obtenerOpcionULSeleccionadaParaLeyenda();
	if (tipo) html += `<span class="badge bg-secondary">Tipo: ${tipo}</span>`;
	html += `</div>`;

	$target.html(html);
}

function BuscarUnidadesDeLectura() {
	AbrirWaiting("Buscando unidades de lectura...");
	var desde = $("#FechaDesde").val();
	var hasta = $("#FechaHasta").val();
	var tipo = obtenerOpcionULSeleccionada();
	var data = { desde, hasta, tipo };
	PostGenHtml(data, buscarUnidadesDeLecturaURL, function (obj) {
		$("#divUL").html(obj);
		//$("#btnImprimir").show();
		autoEvalTabVisible();
		CerrarWaiting();
		return true
	});
}

function SeleccionarUL(row, gridId) {
	let ulId = $(row).data("ul-id");

	if (!ulId) {
		AbrirMensaje("ATENCIÓN", "No se ha seleccionado una unidad de lectura.", function () {
			$("#msjModal").modal("hide");
			return;
		}, false, ["Aceptar"], "error!", null);
		return;
	}
	AbrirWaiting("Buscando detalle de la UL...");
	let data = { ul_id: ulId };

	PostGenHtml(data, buscarDetalleDeUnidadesDeLecturaURL, function (obj) {
		$("#divDetalleUL").html(obj);
		CerrarWaiting();
		return true;
	});
}


function obtenerOpcionULSeleccionada() {
	if ($("#rbULPorFecha").is(":checked")) {
		return "F";
	}
	if ($("#rbULSinAlmacen").is(":checked")) {
		return "A";
	}
	return null; // por si no hubiera ninguno marcado
}

function obtenerOpcionULSeleccionadaParaLeyenda() {
	if ($("#rbULPorFecha").is(":checked")) {
		return "Por Fecha";
	}
	if ($("#rbULSinAlmacen").is(":checked")) {
		return "Sin Almacenar";
	}
	return null; // por si no hubiera ninguno marcado
}

function ValidarFechasFiltro() {

	let fDesde = $("#FechaDesde").val();
	let fHasta = $("#FechaHasta").val();

	// 1) Validar que existan
	if (!fDesde || !fHasta) {
		return [false, "Debe seleccionar ambas fechas."];
	}

	// Convertir a Date
	let dDesde = new Date(fDesde);
	let dHasta = new Date(fHasta);

	// 2) Validar fechas inválidas
	if (isNaN(dDesde.getTime()) || isNaN(dHasta.getTime())) {
		return [false, "Alguna de las fechas no es válida."];
	}

	// 3) Validar Desde < Hasta
	if (dDesde > dHasta) {
		return [false, "La fecha 'Desde' no puede ser mayor que la fecha 'Hasta'."];
	}

	// 4) Validar diferencia máxima de 60 días
	//let diffMs = dHasta - dDesde;
	//let diffDias = diffMs / (1000 * 60 * 60 * 24);

	//if (diffDias > 60) {
	//	return [false, "El rango de fechas no puede superar los 60 días."];
	//}

	return [true, ""];
}

function actualizarFechas() {
	const activar = $("#rbULPorFecha").is(":checked");

	$("#FechaDesde").prop("disabled", !activar);
	$("#FechaHasta").prop("disabled", !activar);
}
