let remCompteSeleccionado = "";
let f_desde = null;
let f_hasta = null;

const TabToTableMapRemitos = {
	"navs-top-rem": "#tbRemitos",
	"navs-top-det": "#tbDetalle"
};

$(function () {
	$("#pagEstado").on("change", function () {
		var div = $("#divPaginacion");
		cargaPaginacion();
	});

	InicializarCamposEnFiltros();

	$("#btnImprimir").off("click");
	$("#btnImprimir").on("click", controlaImprimirRemito);

	$("#btnFiltro").on("click", function () {
		if ($("#divFiltros").hasClass("show")) {
			$("#divFiltros").collapse("hide");
			$("#divDetalle").collapse("show");
		}
		else {
			$("#divFiltros").collapse("show");
			$("#divDetalle").collapse("hide");
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
});

// Mostrar filtros al cargar la pantalla
try { MostrarFiltrosAplicados(); } catch (e) { }

function InicializarPantallaPrincipal() {
	f_desde = $("#Desde").val();
	f_hasta = $("#Hasta").val();
	AbrirWaiting("Cargando información...");
	PostGenHtml({ f_desde, f_hasta }, inicializarPantallPrincipalURL, function (obj) {
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
			CargarTablaTabRemitos(1);
		}, 100);
		return true
	});
}

function CargarTablaTabRemitos(pag = 1) {

	AbrirWaiting("Cargando Remitos externos...");

	var buscaNew = true;
	pagina = pag;
	Pagina = pag;
	var sort = null;
	var sortDir = null
	var data2 = { sort, sortDir, Pagina, buscaNew }
	var data1 = { f_desde, f_hasta };
	var data = $.extend({}, data1, data2);

	PostGenHtml(data, cargarRemitosExternosURL, function (obj) {
		$("#divRemitos").html(obj);
		PostGen({}, buscarMetadataURL, function (obj) {
			if (obj.error === true) {
				AbrirMensaje("ATENCIÓN", obj.msg, function () {
					$("#msjModal").modal("hide");
					return true;
				}, false, ["Aceptar"], "error!", null);
			}
			else {
				totalRegs = obj.metadata.totalCount;
				pags = obj.metadata.totalPages;
				pagRegs = obj.metadata.pageSize;

				$("#pagEstado").val(true).trigger("change");
			}

		});
		EvaluarBotonImprimir("navs-top-aju");
		InicializarEventosTabRemitos();
		// 🔥 Seleccionar automáticamente la primera fila y cargar el detalle
		setTimeout(() => {
			const $primera = $("#tbRemitos tbody tr.row-remito").first();
			if ($primera.length) {
				ProcesarSeleccionFilaEnTabRemitos($primera); // marca visualmente
				SeleccionarRemito($primera[0], "tbRemitos");  // carga el detalle
			}
		}, 50);
		CerrarWaiting();
		return true
	});
}

function SeleccionarRemito(x, grid) {
	var $row = $(x);

	// Obtener valores desde los atributos data-*
	var rem_compte = $row.data("rem-compte");
	AbrirWaiting("Cargando datos del remito...");
	consultarDetalle(rem_compte);
	remCompteSeleccionado = rem_compte;
}

function consultarDetalle(rem_compte) {
	PostGenHtml({ rem_compte }, obtenerDetalleRemitoURL, function (obj) {
		$("#divDetalleRemito").html(obj);
		InicializarEventosTabDetalleRemito();
		CerrarWaiting();
		return true
	});
}

function InicializarEventosTabDetalleRemito() {
	$(document).off("click", "#tbDetalle tbody tr");
	$(document).on("click", "#tbDetalle tbody tr", function (e) {

		if ($(e.target).is("button, a, .btn, i")) return;

		const $nuevaFila = $(this);
		ProcesarSeleccionFilaEnTabDetalleDeRemito($nuevaFila);
	});
}

function ProcesarSeleccionFilaEnTabDetalleDeRemito($fila) {
	$("#tbDetalle tbody tr").removeClass("selected-row");
	$fila.addClass("selected-row");
}

function InicializarEventosTabRemitos() {
	$(document).off("click", "#tbRemitos tbody tr");
	$(document).on("click", "#tbRemitos tbody tr", function (e) {

		if ($(e.target).is("button, a, .btn, i")) return;

		const $nuevaFila = $(this);
		ProcesarSeleccionFilaEnTabRemitos($nuevaFila);
	});
}

function ProcesarSeleccionFilaEnTabRemitos($fila) {
	$("#tbRemitos tbody tr").removeClass("selected-row");
	$fila.addClass("selected-row");
}

function EvaluarBotonImprimir(tabId) {
	console.log("Evaluando botón imprimir para tab:", tabId);
	const tablaSelector = TabToTableMapAjustes[tabId];
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
	$("#btnImprimir").show();

	// Guardamos el tab actual para imprimir
	$("#btnImprimir").data("tab-activo", tabId);
}

function MostrarFiltrosAplicados() {
	const $target = $("#filtrosAplicadosFloating").length
		? $("#filtrosAplicadosFloating")
		: $("#filtrosAplicadosContainer");

	if ($target.length === 0) return;

	const desde = $("#Desde").val();
	const hasta = $("#Hasta").val();

	let html = `
        <div class="d-inline-flex align-items-center"
             style="gap:8px; white-space:nowrap;">
    `;

	if (desde)
		html += `<span class="badge bg-secondary">DESDE: ${desde}</span>`;

	if (hasta)
		html += `<span class="badge bg-secondary">HASTA: ${hasta}</span>`;

	html += `</div>`;

	$target.html(html);
}

function ValidarFechasFiltro() {

	let fDesde = $("#Desde").val();
	let fHasta = $("#Hasta").val();

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

	return [true, ""];
}

function controlaImprimirRemito() {
	if (remCompteSeleccionado == "") {
		AbrirMensaje("ATENCIÓN", "Debe seleccionar un Remito Externo.", function () {
			$("#msjModal").modal("hide");
			return true;
		}, false, ["Aceptar"], "error!", null);
	}
	else {
		ImprimirRemitoExterno_Generado(remCompteSeleccionado);
	}
}

function ImprimirRemitoExterno_Generado(id) {
	ReseteoDeReportes();
	setTimeout(() => {
		let data = { id: id, sm_tipo: "RE" };
		cargarReporteEnArre(102, data, "REMITO NO FISCAL", "", "");
		invocacionGestorDoc({});
	}, 500);
}

function ReseteoDeReportes() {
	console.log("Reseto de reportes");
	ReporteResetArre();
}

function InicializarCamposEnFiltros() {
	$("#btnImprimir").hide();
	$("#lbChkDesdeHasta").text("Fecha")

	$("#chkDesdeHasta").prop('checked', true);
	$("#chkDesdeHasta").trigger("change");
	$("#chkDesdeHasta").prop("disabled", true);

	$("#divFiltros").collapse("show");
	$("#divDetalle").collapse("hide");
}

function cargaPaginacion() {
	$("#divPaginacion").pagination({
		items: totalRegs,
		itemsOnPage: pagRegs,
		cssStyle: "dark-theme",
		currentPage: pagina,
		onPageClick: function (num) {
			CargarTablaTabRemitosExternos(num);
		}
	});
	$("#pagEstado").val(false);
	$("#divFiltros").collapse("hide")
	return true;
}