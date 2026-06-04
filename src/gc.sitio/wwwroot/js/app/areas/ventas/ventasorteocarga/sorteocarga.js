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

let _pedidoLoading = false;
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

			//configurarEventosSeleccionPedido();

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