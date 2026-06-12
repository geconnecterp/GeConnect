let soSorteoSeleccionado = null;
let _pedidoLoading = false;
let tabsSorteosPendientes = 0;
let actualizarTabs = false;
let actualizarTabsComptes = false;
let actualizarTabsAnaProds = false;

const TabToTableMap = {
	"btnTabSorteoDatos": "#tbDatos",
	"btnTabSorteoComprobantes": "#tbGridSorteoComptes",
	"btnTabSorteoAnalisis": "#tbGridSorteoAnalisisProd"
};

$(function () {
	InicializaPantallaPedido();
	InicializaEventosSorteos();
});

function FinalizarCargaDetalle() {
	tabsSorteosPendientes--;
	console.log("tabsSorteosPendientes", tabsSorteosPendientes);
	if (tabsSorteosPendientes <= 0) {
		CerrarWaiting();
	}
}

function EvaluarBotonImprimir(tabId) {
	console.log("Evaluando botón imprimir para tab:", tabId);

	const tablaSelector = TabToTableMap[tabId];
	if (!tablaSelector) {
		$("#btnImprimir").hide();
		return;
	}

	const $tabla = $(tablaSelector);

	// Si la tabla no existe
	if ($tabla.length === 0) {
		$("#btnImprimir").hide();
		return;
	}

	// Contar solo filas que NO sean la fila vacía
	const filasValidas = $tabla.find("tbody tr").not(".fila-vacia").length;

	console.log("Filas válidas:", filasValidas);

	if (filasValidas === 0) {
		$("#btnImprimir").hide();
		return;
	}

	// Si tiene datos → mostrar botón
	$("#btnImprimir").show();

	// Asignar handler según tabla
	if (tablaSelector === "#tbGridSorteoComptes") {
		$(document).off("click", "#btnImprimir").on("click", "#btnImprimir", ControlaImprRepoSorteoComptes);
	} else {
		$(document).off("click", "#btnImprimir").on("click", "#btnImprimir", ControlaImprRepoSorteoAnaProds);
	}

	// Guardar tab activo
	$("#btnImprimir").data("tab-activo", tabId);
}


function ControlaImprRepoSorteoComptes() {
	var filas = $("#tbGridSorteoComptes tbody tr[data-so-sorteo]").length;
	if (filas == 0) {
		AbrirMensaje("ATENCIÓN", "No hay datos para imprimir.", function () {
			$("#msjModal").modal("hide");
			return true;
		}, false, ["Aceptar"], "error!", null);
	}
	else {
		AbrirWaiting("Imprimiendo listado...");
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
				CerrarWaiting();
				ImpimirRepoSorteoComptes();
			}
		});
	}
}

function ImpimirRepoSorteoComptes() {
	ReseteoDeReportes();
	setTimeout(() => {
		var so_sorteo = soSorteoSeleccionado;
		var data = { so_sorteo };
		cargarReporteEnArre(79, data, "RERPORTE SORTEO COMPROBANTES", "", "");
		invocacionGestorDoc({});
	}, 500);
}

function ControlaImprRepoSorteoAnaProds() {
	var filas = $("#tbGridSorteoAnalisisProd tbody tr[data-so-sorteo]").length;
	if (filas == 0) {
		AbrirMensaje("ATENCIÓN", "No hay datos para imprimir.", function () {
			$("#msjModal").modal("hide");
			return true;
		}, false, ["Aceptar"], "error!", null);
	}
	else {
		AbrirWaiting("Imprimiendo listado...");
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
				CerrarWaiting();
				ImpimirRepoSorteoAnaProds();
			}
		});
	}
}

function ReseteoDeReportes() {
	console.log("Reseto de reportes");
	ReporteResetArre();
}

function ImpimirRepoSorteoAnaProds() {
	ReseteoDeReportes();
	setTimeout(() => {
		const adm_id = $("#contenedorAnalisisProd").data("adm-id");
		console.log("adm_id:", adm_id);
		var so_sorteo = soSorteoSeleccionado;
		var data = { so_sorteo, adm_id };
		cargarReporteEnArre(80, data, "REPORTE SORTEO ANÁLISIS DE PRODUCTOS", "", "");
		invocacionGestorDoc({});
	}, 500);
}

function InicializaEventosSorteos() {
	$(document).off("click", "#btnImprimir");
	$(document).on("click", "#btnImprimir", function () {
		//if (!soSorteoSeleccionado) {
		//	alert("Seleccione un sorteo primero.");
		//	return;
		//}
		//imprimirSorteo(soSorteoSeleccionado);
	});

	$("#btnImprimir").prop("disabled", true);

	$("#pagEstado").on("change", function () {
		var div = $("#divPaginacion");
		presentaPaginacion(div);
	});

	funcCallBack = buscarSorteos;

	// Buscar
	$("#btnBuscar").on("click", function () {
		buscarSorteos(1);
	});
}

function buscarSorteos(pag = 1) {
	if (_pedidoLoading) return;
	_pedidoLoading = true;
	pagina = pag;
	try {
		AbrirWaiting("Buscando Sorteos...")
		const filtros = buildQueryFilters(pag);
		const url = buscarSorteoListaUrl;
		PostGenHtml(filtros, url, function (html) {
			$("#divDetalle").html(html).collapse("show");
			$("#divFiltro").collapse("hide");

			configurarEventosSeleccionDeSorteo();

			CerrarWaiting();
			PostGen({}, buscarMetadataURL, function (obj) {
				if (obj.error === true) {
					AbrirMensaje("ATENCIÓN", obj.msg, function () {
						$("#msjModal").modal("hide");
						return true;
					}, false, ["Aceptar"], "error!", null);
				} else {
					totalRegs = obj.metadata.totalCount;
					pags = obj.metadata.totalPages;
					pagRegs = obj.metadata.pageSize;
					$("#pagEstado").val(true).trigger("change");
				}
			});
		});
	}
	catch (e) {
		console.error("Error al buscar sorteos:", e);
		$("#divDetalle").html('<div class="alert alert-danger py-2 mb-0">No se pudo obtener la información.</div>').collapse("show");
	} finally {
		_pedidoLoading = false;
	}
}

function configurarEventosSeleccionDeSorteo() {
	$(document).off("click", "#tbGridSorteo tbody tr");
	$(document).off("dblclick", "#tbGridSorteo tbody tr");

	$(document).on("click", "#tbGridSorteo tbody tr", function (e) {

		if (!$(e.target).is("button, a, .btn, i")) {

			const $this = $(this);

			// Quitar selección previa
			$("#tbGridSorteo tbody tr").removeClass("selected-row");

			// Marcar fila seleccionada
			$this.addClass("selected-row");

			// Guardar valor seleccionado
			soSorteoSeleccionado = $this.data("so-sorteo");

			// Habilitar botón imprimir
			if (soSorteoSeleccionado) {
				$("#btnImprimir").prop("disabled", false).show();
			}
		}
	});

	// ============================
	// DOBLE‑CLICK → Cargar datos + achicar grid
	// ============================
	$(document).on("dblclick", "#tbGridSorteo tbody tr", function (e) {

		if (!$(e.target).is("button, a, .btn, i")) {

			const $this = $(this);
			const soSorteo = $this.data("so-sorteo");

			if (!soSorteo) return;


			// 🔥 ACTUALIZAR DATA-SORTEO EN LOS TABS
			//actualizarDataSorteoEnTabs(soSorteo);
			// Ejecutar funciones de carga
			let data = { so_sorteo: soSorteo };
			//cargarReporteEnArre(62, data, "Pedido de Cliente", "", "");
			tabsSorteosPendientes = 4; // ← cantidad de tabs a cargar
			/*//InicializarEventosBotonesEnTabs();*/
			
			cargarTabs();

			// Achicar grid
			const $grid = $("#divSorteo");
			if (!$grid.hasClass("table-wrapper-100")) {
				$grid.removeClass("table-wrapper-full").addClass("table-wrapper-small");
			}

			// Reposicionar fila seleccionada
			setTimeout(() => {
				posicionarRegOnTop($this, ".table-wrapper-small");
			}, 200);
		}
	});
}

function tabNecesitaRecarga($tab, soSorteoActual) {
	const soCargado = $tab.data("sorteo-loaded");

	// Si nunca cargó nada → recargar
	if (!soCargado) return true;

	// Si cambió el sorteo → recargar
	return soCargado !== soSorteoActual;
}

function actualizarDataSorteoEnTabs(soSorteo) {
	const $btns = $("#tabsSorteoInfo button[data-bs-toggle='tab']");
	console.log("Tabs encontrados:", $btns.length, " | Sorteo:", soSorteo);

	$btns.each(function () {
		$(this).attr("data-sorteo", soSorteo); // si querés, usá attr para verlo en el DOM
	});
}

let sorteo_actual = null;

function InicializarEventosBotonesEnTabs() {

	$(document).off("shown.bs.tab", '#tabsSorteoInfo button[data-bs-toggle="tab"]');
	$(document).on("shown.bs.tab", '#tabsSorteoInfo button[data-bs-toggle="tab"]', function () {

		const tabId = $(this).attr("id");
		console.log("➡️ Tab visible:", tabId);

		cargarSegunTabVisible();
	});
}

function obtenerSorteoDelTabVisible() {
	let $btn = $('#tabsSorteoInfo button.nav-link.active');

	if ($btn.length === 0) {
		$btn = $('#tabsSorteoInfo li.active button.nav-link');
	}

	// Usar attr() en lugar de data() para evitar el cache de jQuery
	return $btn.attr("data-sorteo");
}


function obtenerTabVisible() {
	// Buscar primero el botón activo
	let $btn = $('#tabsSorteoInfo button.nav-link.active');

	// Si no lo encuentra, buscar el li activo y luego su botón
	if ($btn.length === 0) {
		$btn = $('#tabsSorteoInfo li.active button.nav-link');
	}

	return $btn.attr("id");
}

function cargarSegunTabVisible() {
	const tabId = obtenerTabVisible();
	const soSorteo = obtenerSorteoDelTabVisible();

	const $tab = $("#" + tabId);
	EvaluarBotonImprimir(tabId);
	console.log("➡️ Tab visible:", tabId, "| Sorteo actual:", soSorteo, "| Último cargado:", $tab.data("sorteo-loaded"));

	if (!soSorteo) {
		console.warn("⚠️ No hay sorteo asociado al tab visible.");
		return;
	}

	// Si no necesita recarga → salir
	if (!tabNecesitaRecarga($tab, soSorteo)) {
		console.log("⏩ Tab ya cargado para este sorteo. No se recarga.");
		return;
	}

	// Si necesita recarga → cargar y actualizar marca
	switch (tabId) {
		case "btnTabSorteoDatos":
			cargarTabsDatos();
			break;

		case "btnTabSorteoComprobantes":
			cargarTabsComptes();
			break;

		case "btnTabSorteoAnalisis":
			cargarTabsAnaProds();
			break;
	}

	// Marcar que este tab ya cargó este sorteo
	$tab.data("sorteo-loaded", soSorteo);
}



function cargarTabs() {
	AbrirWaiting("Cargando datos del sorteo...")
	var data = {};
	PostGenHtml(data, cargarTabsInicialUrl, function (html) {
		$("#divTabsSorteo").html(html).show();
		// 🔥 AHORA sí existen los tabs en el DOM
		actualizarDataSorteoEnTabs(soSorteoSeleccionado);

		InicializarEventosBotonesEnTabs();
		// Cargar datos de los tabs internos
		setTimeout(() => {
			CerrarWaiting();
			cargarSegunTabVisible();
		}, 200);
		// Debug - ayuda a identificar estados del sistema
		console.log("cargarSorteoDatos N°: ", soSorteoSeleccionado,
			"Permite edición:", true);
	});
	//FinalizarCargaDetalle(); // ← marcar como completado
}

function cargarTabsDatos() {
	AbrirWaiting("Cargando datos...");
	const soSorteo = obtenerSorteoDelTabVisible();
	var data = { so_sorteo: soSorteo };
	PostGenHtml(data, obtenerSorteoDatosUrl, function (html) {
		$("#divSorteoDatos").html(html);
		
		setTimeout(() => {
			EvaluarBotonImprimir("navs-top-dat");
		}, 500);
		// 🔥 Marcar que este tab ya cargó este sorteo
		$("#btnTabSorteoDatos").data("sorteo-loaded", soSorteo);
		console.log("cargarSorteoTablas N°: ", soSorteo,
			"Permite edición:", true);
		CerrarWaiting();
	});
}

function cargarTabsComptes() {
	AbrirWaiting("Cargando datos...");
	const soSorteo = obtenerSorteoDelTabVisible();
	var data = { so_sorteo: soSorteo };
	PostGenHtml(data, obtenerSorteoComptesUrl, function (html) {
		$("#divSorteoComprobantes").html(html);
		InicializarEventosTabComptes();
		FinalizarCargaDetalle(); // ← marcar como completado
		setTimeout(() => {
			EvaluarBotonImprimir("btnTabSorteoComprobantes");
		}, 500);
		// 🔥 Marcar que este tab ya cargó este sorteo
		$("#btnTabSorteoComprobantes").data("sorteo-loaded", soSorteo);
		console.log("cargarSorteoComptes N°: ", soSorteo,
			"Permite edición:", true);
		CerrarWaiting();
	});
}

function InicializarEventosTabComptes() {
	$(document).off("click", "#tbGridSorteoComptes tbody tr");
	$(document).on("click", "#tbGridSorteoComptes tbody tr", function (e) {

		if ($(e.target).is("button, a, .btn, i")) return;

		const $nuevaFila = $(this);
		ProcesarSeleccionFilaEnTabComptes($nuevaFila);
	});
}

function ProcesarSeleccionFilaEnTabComptes($nuevaFila) {
	$("#tbGridSorteoComptes tbody tr").removeClass("selected-row");
	$nuevaFila.addClass("selected-row");
}

function cargarTabsAnaProds() {
	AbrirWaiting("Cargando datos...");
	const soSorteo = obtenerSorteoDelTabVisible();
	var data = { so_sorteo: soSorteo };
	PostGenHtml(data, obtenerSorteoAnaProdsUrl, function (html) {
		$("#divSorteoAnalisis").html(html);
		InicializarEventosTabAnaProds();
		FinalizarCargaDetalle(); // ← marcar como completado
		setTimeout(() => {
			EvaluarBotonImprimir("btnTabSorteoAnalisis");
		}, 500);
		// 🔥 Marcar que este tab ya cargó este sorteo
		$("#btnTabSorteoAnalisis").data("sorteo-loaded", soSorteo);
		console.log("cargarSorteoAnaProds N°: ", soSorteo,
			"Permite edición:", true);
		CerrarWaiting();
	});
}

function InicializarEventosTabAnaProds() {
	$(document).off("click", "#tbGridSorteoAnalisisProd tbody tr");
	$(document).on("click", "#tbGridSorteoAnalisisProd tbody tr", function (e) {

		if ($(e.target).is("button, a, .btn, i")) return;

		const $nuevaFila = $(this);
		ProcesarSeleccionFilaEnTabAnaProds($nuevaFila);
	});
}

function ProcesarSeleccionFilaEnTabAnaProds($nuevaFila) {
	$("#tbGridSorteoAnalisisProd tbody tr").removeClass("selected-row");
	$nuevaFila.addClass("selected-row");
}

function buildQueryFilters(pag) {
	const usaPeriodo = $("#chkDesdeHasta").is(":checked");
	const fechaD = usaPeriodo ? $("#Desde").val() : null;
	const fechaH = usaPeriodo ? $("#Hasta").val() : null;

	return {
		Registros: 200,
		Pagina: pag,
		FechaD: fechaD || null,
		FechaH: fechaH || null,
	};
}

function InicializaPantallaPedido() {
	// INICIALIZAMOS PANELES
	if ($("#divDetalle").is(":visible")) {
		$("#divDetalle").collapse("hide");
	}
	$("#divFiltro").collapse("show");

	$("#btnCancel").on("click", function () {
		window.location.href = homePedido;
	});

	$("#btnImprimir").prop("disabled", true).hide();

	// Delegación: captura Enter en cualquiera de los inputs date del filtro
	$(document).on("keydown", "#divFiltro input[type='date']", function (e) {
		if (e.key === "Enter") {
			e.preventDefault(); // evita submit o comportamientos raros
			$("#btnBuscar").trigger("click");
		}
	});

	// Inicializa el período de fechas (hoy / hoy + 30 días)
	initPeriodoFechas();

	// Etiquetas de filtros
	$("#lbChkDesdeHasta").text("Periodo");
	$("#chkDesdeHasta")
		.prop("checked", true)
		.prop("disabled", true);

	$("#Desde").prop("disabled", false);
	$("#Hasta").prop("disabled", false);
}

function initPeriodoFechas() {
	// Último lunes pasado
	const desde = obtenerPrimerDiaMesAnterior();

	// Hoy
	const hasta = new Date();

	// Formatear YYYY-MM-DD
	const fmt = d => d.toISOString().split("T")[0];

	$("#Desde").val(fmt(desde));
	$("#Hasta").val(fmt(hasta));

	// Siempre habilitadas
	$("#Desde").prop("disabled", false);
	$("#Hasta").prop("disabled", false);

	// Checkbox siempre marcado y deshabilitado
	$("#chkDesdeHasta")
		.prop("checked", true)
		.prop("disabled", true);
}

function obtenerPrimerDiaMesAnterior() {
	const hoy = new Date();
	const year = hoy.getFullYear();
	const month = hoy.getMonth(); // 0=enero ... 11=diciembre

	// Primer día del mes anterior
	return new Date(year, month - 1, 1);
}