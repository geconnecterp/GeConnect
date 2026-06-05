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
			cargarSorteoTablas(soSorteo);

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

function cargarSorteoDatos(soSorteo) {
	var datos = { so_sorteo: soSorteo };
	AbrirWaiting("Cargando datos del sorteo...");
	PostGenHtml(datos, obtenerSorteoDatosUrl, function (html) {
		$("#divSorteoDatos").html(html).show();
		$("#btnAbmModif").prop("disabled", false);
		$("#btnAbmElimi").prop("disabled", false);

		// Debug - ayuda a identificar estados del sistema
		console.log("cargarSorteoDatos N°: ", soSorteo,
			"Permite edición:", true);

		CerrarWaiting();
	});
}

function cargarSorteoTablas(soSorteo) {
	var data = {};
	AbrirWaiting();
	PostGenHtml(data, obtenerSorteoTablasUrl, function (html) {
		$("#divSorteoTablas").html(html).show();
		cargarSorteoTablasSucursales(soSorteo);
		cargarSorteoTablasProductos(soSorteo);
		// Debug - ayuda a identificar estados del sistema
		console.log("cargarSorteoTablas N°: ", soSorteo);

		CerrarWaiting();
	});
}

function cargarSorteoTablasSucursales(soSorteo) {
	var data = { so_sorteo: soSorteo };
	AbrirWaiting("Cargando sucursales del sorteo...");
	PostGenHtml(data, obtenerSorteoTablasSucursalesUrl, function (html) {
		$("#divSorteoTablaSucursales").html(html).show();
		CerrarWaiting();
		inicializarEventosTablaSucursales();
	});
}

function cargarSorteoTablasProductos(soSorteo) {
	var data = { so_sorteo: soSorteo };
	AbrirWaiting("Cargando productos del sorteo...");
	PostGenHtml(data, obtenerSorteoTablasProductosUrl, function (html) {
		$("#divSorteoTablaProductos").html(html).show();
		CerrarWaiting();
		inicializarEventosTablaProductos();
	});
}

function inicializarEventosTablaSucursales() {
	// Seleccionar / deseleccionar todos
	$(document).off("change", "#chkAllIncluye");
	$(document).on("change", "#chkAllIncluye", function () {
		const checked = $(this).is(":checked");
		$("#tbSorteoAdm tbody .chkIncluye").prop("checked", checked);
	});

	$(document).off("change", ".chkIncluye");
	$(document).on("change", ".chkIncluye", function () {
		const total = $("#tbSorteoAdm tbody .chkIncluye").length;
		const marcados = $("#tbSorteoAdm tbody .chkIncluye:checked").length;

		$("#chkAllIncluye").prop("checked", total === marcados);
	});
	aplicarMascaraEnteros();
	$(document).off("blur", ".input-editable");
	$(document).on("blur", ".input-editable", function () {
		validarRangosSorteoAdm();
	});
	$(document).off("keydown", ".input-editable");
	$(document).on("keydown", ".input-editable", function (e) {

		const $inputs = $("#tbSorteoAdm .input-editable");
		const index = $inputs.index(this);

		let newIndex = index;

		switch (e.key) {

			case "Enter":
			case "Tab":
				e.preventDefault();
				if (e.shiftKey) {
					newIndex = (index - 1 + $inputs.length) % $inputs.length;
				} else {
					newIndex = (index + 1) % $inputs.length;
				}
				break;

			case "ArrowRight":
				e.preventDefault();
				newIndex = (index + 1) % $inputs.length;
				break;

			case "ArrowLeft":
				e.preventDefault();
				newIndex = (index - 1 + $inputs.length) % $inputs.length;
				break;

			case "ArrowDown":
				e.preventDefault();
				newIndex = buscarInputAbajo(index, $inputs);
				break;

			case "ArrowUp":
				e.preventDefault();
				newIndex = buscarInputArriba(index, $inputs);
				break;
		}

		const $next = $inputs.eq(newIndex);
		$next.focus().select();
	});

}

function buscarInputAbajo(index, $inputs) {
	const col = index % 2; // 0 = desde, 1 = hasta
	const fila = Math.floor(index / 2);

	const totalFilas = $("#tbSorteoAdm tbody tr").length;
	const nuevaFila = (fila + 1) % totalFilas;

	return nuevaFila * 2 + col;
}

function buscarInputArriba(index, $inputs) {
	const col = index % 2;
	const fila = Math.floor(index / 2);

	const totalFilas = $("#tbSorteoAdm tbody tr").length;
	const nuevaFila = (fila - 1 + totalFilas) % totalFilas;

	return nuevaFila * 2 + col;
}


function aplicarMascaraEnteros() {
	$(".input-numero").inputmask(maskConfigEnteros);
}

function validarRangosSorteoAdm() {

	let filas = [];

	$("#tbSorteoAdm tbody tr").each(function () {

		const desdeStr = $(this).find(".so-desde").val() || "0";
		const hastaStr = $(this).find(".so-hasta").val() || "0";

		const desde = parseInt(desdeStr.replace(/\./g, "")) || 0;
		const hasta = parseInt(hastaStr.replace(/\./g, "")) || 0;

		const admId = $(this).data("adm-id");

		if (desde > 0 && hasta > 0) {
			filas.push({ admId, desde, hasta, row: $(this) });
		}
	});

	// Ordenar por "desde"
	filas.sort((a, b) => a.desde - b.desde);

	let error = false;

	// Limpiar errores previos
	$("#tbSorteoAdm tbody tr").removeClass("error-range");

	for (let i = 0; i < filas.length - 1; i++) {

		const actual = filas[i];
		const siguiente = filas[i + 1];

		// Si se solapan
		if (actual.hasta >= siguiente.desde) {
			error = true;
			actual.row.addClass("error-range");
			siguiente.row.addClass("error-range");
		}
	}

	if (error) {
		AbrirMensaje("ATENCIÓN", "Los rangos de numeración se solapan entre sucursales.", null, false, ["Aceptar"], "error!", null);
	}

	return !error;
}

function inicializarEventosTablaProductos() {
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

const maskConfigEnteros = {
	alias: "numeric",
	groupSeparator: ".",
	autoGroup: true,
	digits: 0,
	digitsOptional: false,
	rightAlign: true,
	prefix: '',
	placeholder: "0",
	clearMaskOnLostFocus: false,
	showMaskOnHover: false,
	showMaskOnFocus: false,
	allowMinus: false,
	onBeforeMask: function (value) {
		if (value) {
			let numValue = parseInt(value.toString().replace(/\./g, ''));
			return isNaN(numValue) ? value : numValue.toString();
		}
		return value;
	}
};