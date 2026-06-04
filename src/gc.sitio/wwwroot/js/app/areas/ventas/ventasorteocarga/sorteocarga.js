let _pedidoLoading = false;
let soSorteoSeleccionado = null;

$(function () {
	InicializaPantallaPedido();
	InicializaEventosSorteos();
});

function InicializaEventosSorteos() {
	$(document).off("click", "#btnImprimir");
	$(document).on("click", "#btnImprimir", function () {
		if (!soSorteoSeleccionado) {
			alert("Seleccione un sorteo primero.");
			return;
		}
		imprimirSorteo(soSorteoSeleccionado);
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
		AbrirWaiting("Buscando Pedidos de Cliente...")
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

			// Ejecutar funciones de carga
			let data = { so_sorteo: soSorteo };
			//cargarReporteEnArre(62, data, "Pedido de Cliente", "", "");
			cargarSorteoDatos(soSorteo);
			//cargarProductosSorteo(soSorteo);

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

function cargarSorteoDatos(soSorteo) { }

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
	// ✅ Activar botón de nuevo pedido
	$("#btnAbmNuevo").prop("disabled", false);

	// Configurar el evento click para el botón Cancelar/Inicializar
	$("#btnAbmCancelar").on("click", function (e) {
		//cancelarOperacion(e);
	});

	$("#btnCancel").on("click", function () {
		window.location.href = homePedido;
	});

	$("#btnAbmAceptar, #btnAbmCancelar, #btnImprimir").prop("disabled", true).hide();

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
	const desde = obtenerUltimoLunes();

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

function obtenerUltimoLunes() {
	const hoy = new Date();
	const diaSemana = hoy.getDay(); // 0=Domingo ... 1=Lunes

	// Si hoy es lunes → retroceder 7 días
	const diferencia = diaSemana === 1 ? 7 : (diaSemana + 6) % 7;

	const ultimoLunes = new Date(hoy);
	ultimoLunes.setDate(hoy.getDate() - diferencia);

	return ultimoLunes;
}