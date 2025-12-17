$(function () {
	InicializarVista();
});

function InicializarVista() {
	if ($("#divDetalle").is(":visible")) {
		$("#divDetalle").collapse("hide");
	}
	$("#divFiltro").collapse("show");
	$("#lbChkDesdeHasta").text("Fecha de Inventario")

	$("#chkDesdeHasta").on("click", function () {
		if ($("#chkDesdeHasta").is(":checked")) {
			$("#Date1").prop("disabled", false);
			$("#Date2").prop("disabled", false);
			$("#Date1").trigger("focus");
		}
		else {
			$("#Date1").prop("disabled", true);
			$("#Date2").prop("disabled", true);
		}
	});

	$("#btnBuscar").on("click", function () {
		if (validarFechas()) {
			InicializarBusqueda();
		} else {
			AbrirMensaje("ATENCIÓN", "Problemas con las fechas, por favor verifique.", function () {
				$("#msjModal").modal("hide");
				return true;
			}, false, ["Aceptar"], "error!", null);
		}
	});

	// Validación al terminar edición
	$("#Date1, #Date2").on("blur keydown change", function (e) {
		if (e.type === "keydown" && e.key !== "Enter") return;

		const v = $(this).val();
		if (esFechaISOCompleta(v)) {
			finalizarEdicionFechas();
		}
	});

	InicializarFechasEnFiltros();
}

function InicializarBusqueda() {
	var data = {};
	AbrirWaiting("Inicializando presentación de vista de inventario...");
	PostGenHtml(data, inicializarPantallPrincipalURL, function (obj) {
		$("#divDetalle").html(obj);
		$("#divFiltro").collapse("hide");
		$("#divDetalle").collapse("show");
		CargarInventarioLista();
		CargarCamposDatosInventario();
		CargarDatosAdicionalesInicial();
		CerrarWaiting();
		return true
	});
}

function CargarDatosAdicionalesInicial() {
	var data = {};
	PostGenHtml(data, cargarDatosAdicionalesInicialURL, function (obj) {
		$("#divGrillasAdicionales").html(obj);
		$("#lbCargarPorSector").text("Cargar por Sector");
		//Cargar las grillas (Rubros y Usuarios) y listas (Sectores y Usuarios)
		CargarGrillaRubrosEnSeccionDatosAdicionales();
		CargarListaSectoresEnSeccionDatosAdicionales();
		return true
	});
}

function CargarListaSectoresEnSeccionDatosAdicionales() {
	var data = { };
	PostGenHtml(data, cargarListaSectoresEnSeccionDatosAdicionalesURL, function (obj) {
		$("#divListaSector").html(obj);
		return true
	});
}

function CargarGrillaRubrosEnSeccionDatosAdicionales(invId = 0) { 
	var inv_nro = invId;
	var data = { inv_nro };
	PostGenHtml(data, cargarGrillaRubrosEnSeccionDatosAdicionalesURL, function (obj) {
		$("#divGrillaRubros").html(obj);
		return true
	});
}

function CargarInventarioLista() {
	var desde = $("#Date1").val();
	var hasta = $("#Date2").val();
	var data = { desde, hasta };
	PostGenHtml(data, buscarInventarioListaURL, function (obj) {
		$("#divCargaInventario").html(obj);
		return true
	});
}

function CargarCamposDatosInventario() {
	var data = { };
	PostGenHtml(data, cargarCamposDatosInventarioURL, function (obj) {
		$("#divDatosDeInventario").html(obj);
		InicializarFechasEnDatos();
		return true
	});
}

function InicializarFechasEnFiltros() {
	// Fecha actual
	let today = new Date();

	// Fecha un mes atrás
	let pastMonth = new Date();
	pastMonth.setMonth(pastMonth.getMonth() - 1);

	// Formatear en yyyy-MM-dd
	let todayStr = today.toISOString().split('T')[0];
	let pastMonthStr = pastMonth.toISOString().split('T')[0];

	// Asignar valores a los inputs
	$("#Date2").val(todayStr);     // hasta = hoy
	$("#Date1").val(pastMonthStr); // desde = un mes atrás
}

function InicializarFechasEnDatos() {
	// Fecha actual (hoy)
	let today = new Date();

	// Fecha de ayer
	let yesterday = new Date();
	yesterday.setDate(today.getDate() - 1);

	// Formatear en yyyy-MM-dd
	let todayStr = today.toISOString().split('T')[0];
	let yesterdayStr = yesterday.toISOString().split('T')[0];

	// Asignar valores a los inputs
	$("#dtAperturaHasta").val(todayStr);     // hasta = hoy
	$("#dtAperturaDesde").val(yesterdayStr); // desde = ayer
}

// Debounce genérico para input continuo (opcional)
function debounce(fn, delay = 400) {
	let t;
	return function (...args) {
		clearTimeout(t);
		t = setTimeout(() => fn.apply(this, args), delay);
	};
}

function esFechaISOCompleta(v) {
	return typeof v === "string" && v.length === 10 && !isNaN(Date.parse(v));
}

let errorMostrado = false;

function finalizarEdicionFechas() {
	const d = $("#Date1").val();
	const h = $("#Date2").val();

	if (!esFechaISOCompleta(d) || !esFechaISOCompleta(h)) return;

	if (!validarFechas()) {
		if (!errorMostrado) {
			AbrirMensaje("ATENCIÓN", "La fecha 'Desde' no puede ser mayor que la fecha 'Hasta'.", function () {
				$("#msjModal").modal("hide");
				return true;
			}, false, ["Aceptar"], "error!", null);
			errorMostrado = true; // marcamos que ya se mostró
			InicializarFechasEnFiltros();
		}
		return;
	}

	// Si la validación pasa, reseteamos el flag
	errorMostrado = false;

	// Aquí dispará tu llamada Ajax/actualización
	// ejemplo: ejecutarConsulta();
}

function validarFechas() {
	let desde = $("#Date1").val();
	let hasta = $("#Date2").val();

	if (!desde || !hasta) return false;

	let fechaDesde = new Date(desde);
	let fechaHasta = new Date(hasta);

	return !(fechaDesde > fechaHasta);
}

function selectItemGrillaRubro(x) { }

function eliminarItemRubro(inv_nro, rub_id) { }