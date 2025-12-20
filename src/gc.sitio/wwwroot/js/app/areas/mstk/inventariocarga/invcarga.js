const EstadoBtnEnDivPrincipal = {
	AGREGAR: Symbol("AGREGAR"),
	MODIFICAR: Symbol("MODIFICAR"),
	ELIMINAR: Symbol("ELIMINAR"),
	CONFIRMAR: Symbol("CONFIRMAR"),
	CANCELAR: Symbol("CANCELAR"),
	REGISTRAR_STOCK_DE_CONTROL: Symbol("REGISTRAR_STOCK_DE_CONTROL"),
	VALORIZACION: Symbol("VALORIZACION"),
	CERRAR_INVENTARIO: Symbol("CERRAR_INVENTARIO")
};
var accion = "";
var invNroSeleccionado = "";
var inveIdSeleccionado = "";
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
		CargarEventosABotonesEnDivPrincipal();
		CerrarWaiting();
		return true
	});
}

function CargarEventosABotonesEnDivPrincipal() {
	$(document).on("click", "#btnAgregar", ControlaAgregarInventario);
	$(document).on("click", "#btnConfirmar", ControlaConfirmarInventario);
	$(document).on("click", "#btnCancelar", ControlaCancelarInventario);
}

function ControlaAgregarInventario() {
	HabilitarDatosInventario();
	HabilitarDatosAdicionales();
	ActualizarEstadoDeBotonesPorEventos(EstadoBtnEnDivPrincipal.AGREGAR)
}

function ControlaConfirmarInventario() {
	var resultado = ValidarCamposDeInventarioEnABM();
	if (resultado == "") { //TODO OK

		AbrirMensaje("ATENCIÓN", `¿Esta seguro que desea ${accion.description} el inventario?`, function (e) {
			$("#msjModal").modal("hide");
			switch (e) {
				case "SI":
					HandlerConfirmarCargaInventario();
					break;
				case "NO":
					break;
				default: //NO
					break;
			}
			return true;

		}, true, ["Aceptar", "Cancelar"], "question!", null);
	}
	else {
		AbrirMensaje("ATENCIÓN", resultado, function () {
			$("#msjModal").modal("hide");
			$("#listaDepositos").trigger("focus");
			return true;
		}, false, ["Aceptar"], "error!", null);
	}

	
}

function HandlerConfirmarCargaInventario() {
	// Aquí iría la lógica para guardar los datos del inventario
	let abm = ObtenerTipoDeOperacionEnABM();
	let inv_nro = $("#inv_nro").val();
	let invt_id = $("#listaConteos").val();
	let inv_descripcion = $("#txtDescripcion").val().trim();
	let inv_apertura = $("#dtAperturaDesde").val();
	let inv_cierre = $("#dtAperturaHasta").val();
	let depo_id = $("#listaDepositos").val();
	var data = {
		abm,
		inv_nro,
		invt_id,
		inv_descripcion,
		inv_apertura,
		inv_cierre,
		depo_id
	};
	AbrirWaiting("Confirmando Inventario...");
	PostGen(data, confirmarInventarioURL, function (obj) {
		CerrarWaiting();
		if (!obj.ok && obj.error && obj.msg === "No autenticado") {
			window.location.href = login;
			return false;
		}

		if (obj.error === true) {
			AbrirMensaje("ATENCIÓN", obj.msg, function () {
				$("#msjModal").modal("hide");
				return true;
			}, false, ["Aceptar"], "error!", null);
		}
		else {
			setTimeout(() => {
				//Hacer algo luego de actualizar
				//RecargarGrilla
				CargarInventarioLista();
				//Limpiar datos en controles
				InicializarDatosDeInventario();
				//Limpiar grillas adicionales
				LimpiarGrillasEnDatosAdicionales();
				//limpiar seleccion en selectLista de grillas adicionales
				LimiparSelectListEnDatosAdicionales()
				//Actualizar estado de botones
				ActualizarEstadoDeBotonesPorEventos(EstadoBtnEnDivPrincipal.CONFIRMAR)
			}, 500);
		}
	});

}

function LimpiarGrillasEnDatosAdicionales() {
	CargarGrillaRubrosEnSeccionDatosAdicionales();
	CargarGrillaUsuariosEnSeccionDatosAdicionales();
}

function LimiparSelectListEnDatosAdicionales() {
	// Si está visible el de Sector
	if ($("#divListaSector").is(":visible")) {
		$("#listaSectores").val("");   // vuelve a "Seleccionar"
	}

	// Si está visible el de Rubro
	if ($("#divListaRubro").is(":visible")) {
		$("#listaRubros").val("");     // vuelve a "Seleccionar"
	}
	$("#listaUsuarios").val("");
}

function ObtenerTipoDeOperacionEnABM() {
	let tipo = "";
	switch (accion) {
		case EstadoBtnEnDivPrincipal.AGREGAR:
			tipo = "A";
			break;
		case EstadoBtnEnDivPrincipal.MODIFICAR:
			tipo = "M";
			break;
		case EstadoBtnEnDivPrincipal.ELIMINAR:
			tipo = "B";
			break;
		default:
			tipo = "";
			break;
	}
	return tipo;
}

function ValidarCamposDeInventarioEnABM() {
	let resultado = "";
	let depo = $("#listaDepositos").val();
	if (depo === "" || depo === null || depo === undefined) {
		resultado += "Debe seleccionar un Depósito.<br/>";
	}
	let conteo = $("#listaConteos").val();
	if (conteo === "" || conteo === null || conteo === undefined) {
		resultado += "Debe seleccionar un Tipo de Conteo.<br/>";
	}
	let desc = $("#txtDescripcion").val().trim();
	if (desc === "") {
		resultado += "Debe ingresar una Descripción para el Inventario.<br/>";
	}
	let fechas = validarFechasAperturaYCierre();
	if (fechas !== "") {
		resultado += fechas + "<br/>";
	}
	let grillas = validarTablasConDatos();
	if (grillas !== "") {
		resultado += grillas + "<br/>";
	}
	return resultado;
}

function validarTablasConDatos() {
	var resultado = "";
	// Obtener filas de cada tabla
	var filasRubros = $("#tbListaRubros tbody tr");
	var filasUsuarios = $("#tbListaUsuarios tbody tr");

	// Validar Rubros: al menos una fila y que no sea la de "No hay rubros cargados"
	var tieneRubros = filasRubros.length > 0 &&
		!filasRubros.first().text().includes("No hay rubros cargados");

	// Validar Usuarios: al menos una fila y que no sea la de "No hay rubros cargados"
	var tieneUsuarios = filasUsuarios.length > 0 &&
		!filasUsuarios.first().text().includes("No hay usuarios cargados");

	if (!tieneRubros) {
		resultado += "Debe agregar al menos un Rubro.<br/>";
	}

	if (!tieneUsuarios) {
		resultado += "Debe agregar al menos un Usuario.<br/>";
	}

	return resultado;
}


function ControlaCancelarInventario() {
	BlanquearControlesEnDatosDeInventario();
	DeshabilitarDatosInventario();
	DeshabilitarDatosAdicionales();
	InicializarFechasEnDatos();
	ActualizarEstadoDeBotonesPorEventos(EstadoBtnEnDivPrincipal.CANCELAR)
}

function BlanquearControlesEnDatosDeInventario() {
	// Limpiar todos los inputs de texto y fecha
	$("#divDatosDeInventario").find("input[type=text], input[type=date]").val("");

	// Resetear todos los selects a su primer opción
	$("#divDatosDeInventario").find("select").each(function () {
		$(this).prop("selectedIndex", 0);
	});
}

function CargarDatosAdicionalesInicial() {
	var data = {};
	PostGenHtml(data, cargarDatosAdicionalesInicialURL, function (obj) {
		$("#divGrillasAdicionales").html(obj);
		$("#chkCargarPorSector").on("change", function () {
			if ($(this).is(":checked")) {
				// 👉 Caso TRUE: el switch está activado
				console.log("Cargar SECTORES");
				// acá llamás a tu función, por ejemplo:
				CargarListaSectoresEnSeccionDatosAdicionales();
			} else {
				// 👉 Caso FALSE: el switch está desactivado
				console.log("Cargar RUBROS");
				// otra lógica, por ejemplo:
				CargarListaRubrosEnSeccionDatosAdicionales();
			}
		});
		

		// inicializar al cargar
		$("#listaConteos").trigger("change");

		$("#lbCargarPorSector").text("Cargar por Sector");
		CargarGrillaRubrosEnSeccionDatosAdicionales();
		CargarListaSectoresEnSeccionDatosAdicionales();
		CargarGrillaUsuariosEnSeccionDatosAdicionales();
		CargarListaUsuariosEnSeccionDatosAdicionales();
		setTimeout(() => {
			DeshabilitarDatosAdicionales();
		}, 500);
		ActualizarEstadoDeBotones();
		return true
	});
}

//#### INICIO Region Carga de datos en tab 'Carga de Inventario' ####//
function CargarListaSectoresEnSeccionDatosAdicionales() {
	var data = {};
	PostGenHtml(data, cargarListaSectoresEnSeccionDatosAdicionalesURL, function (obj) {
		$("#divListaSector").html(obj);
		$("#divListaRubro").hide(); // ejemplo: ocultar usuarios
		$("#divListaSector").show();   // ejemplo: mostrar sectores
		$(document).off("click", "#btnAgregarSector").on("click", "#btnAgregarSector", ControlaAgregarRubrosPorSector);
		return true
	});
}

function ControlaAgregarRubrosPorSector() {
	var sec_id = $("#listaSectores").val();
	if (sec_id != "") {
		var data = { sec_id };
		PostGenHtml(data, agregarRubrosPorSectorURL, function (obj) {
			$("#divGrillaRubros").html(obj);
			return true
		});
	}
	else {
		AbrirMensaje("ATENCIÓN", "Debe seleccionar un Sector.", function () {
			$("#msjModal").modal("hide");
			$("#listaSectores").trigger("focus");
			return true;
		}, false, ["Aceptar"], "error!", null);
	}
}

function CargarListaRubrosEnSeccionDatosAdicionales() {
	var data = {};
	PostGenHtml(data, cargarListaRubrosEnSeccionDatosAdicionalesURL, function (obj) {
		$("#divListaRubro").html(obj);
		$("#divListaSector").hide();
		$("#divListaRubro").show();
		$(document).off("click", "#btnAgregarRubro").on("click", "#btnAgregarRubro", ControlaAgregarRubroIndividual);
		return true
	});
}

function ControlaAgregarRubroIndividual() {
	var rub_id = $("#listaRubros").val();
	if (rub_id != "") {
		var data = { rub_id };
		PostGenHtml(data, agregarRubroIndividualURL, function (obj) {
			$("#divGrillaRubros").html(obj);
			return true
		});
	}
	else {
		AbrirMensaje("ATENCIÓN", "Debe seleccionar un Rubro.", function () {
			$("#msjModal").modal("hide");
			$("#listaRubros").trigger("focus");
			return true;
		}, false, ["Aceptar"], "error!", null);
	}
}

function CargarGrillaRubrosEnSeccionDatosAdicionales(invId = 0) {
	var inv_nro = invId;
	var data = { inv_nro };
	PostGenHtml(data, cargarGrillaRubrosEnSeccionDatosAdicionalesURL, function (obj) {
		$("#divGrillaRubros").html(obj);
		return true
	});
}

function CargarGrillaUsuariosEnSeccionDatosAdicionales(invId = 0) {
	var inv_nro = invId;
	var data = { inv_nro };
	PostGenHtml(data, cargarGrillaUsuariosEnSeccionDatosAdicionalesURL, function (obj) {
		$("#divGrillaUsuarios").html(obj);
		return true
	});
}

function CargarListaUsuariosEnSeccionDatosAdicionales() {
	var data = {};
	PostGenHtml(data, cargarListaUsuariosEnSeccionDatosAdicionalesURL, function (obj) {
		$("#divListaUsuarios").html(obj);
		$(document).off("click", "#btnAgregarUsuario").on("click", "#btnAgregarUsuario", ControlaAgregarUsuarioIndividual);
		return true
	});
}

function ControlaAgregarUsuarioIndividual() {
	var usu_id = $("#listaUsuarios").val();
	var grupo = $("#listaOpcionesConteo").val();
	if (usu_id != "") {
		var data = { usu_id, grupo };
		PostGenHtml(data, agregarUsuarioIndividualURL, function (obj) {
			$("#divGrillaUsuarios").html(obj);
			return true
		});
	}
	else {
		AbrirMensaje("ATENCIÓN", "Debe seleccionar un Usuario.", function () {
			$("#msjModal").modal("hide");
			$("#listaUsuarios").trigger("focus");
			return true;
		}, false, ["Aceptar"], "error!", null);
	}
}

function CargarInventarioLista() {
	var desde = $("#Date1").val();
	var hasta = $("#Date2").val();
	var data = { desde, hasta };
	PostGenHtml(data, buscarInventarioListaURL, function (obj) {
		$("#divGrillaInventario").html(obj);
		ActualizarEstadoDeBotones(); // evaluar estado inicial
		return true
	});
}

function CargarCamposDatosInventario() {
	var data = {};
	PostGenHtml(data, cargarCamposDatosInventarioURL, function (obj) {
		$("#divDatosDeInventario").html(obj);
		$("#listaConteos").on("change", function () {
			let valor = $(this).find("option:selected").text(); // texto de la opción seleccionada
			let id = $(this).find("option:selected").val(); // texto de la opción seleccionada
			var $listaOpciones = $("#listaOpcionesConteo");

			// limpiar opciones
			$listaOpciones.empty();

			if (id === "D") {
				// agregar opciones 1 y 2
				$listaOpciones.append(new Option("1", "1"));
				$listaOpciones.append(new Option("2", "2"));
			} else {
				// agregar solo opción 1
				$listaOpciones.append(new Option("1", "1"));
			}
		});
		CargarOpcionesInicialesEnListaGrupos();
		InicializarFechasEnDatos();
		DeshabilitarDatosInventario();
		return true
	});
}

function CargarOpcionesInicialesEnListaGrupos() {
	var valor = $("#listaConteos").find("option:selected").text(); // texto de la opción seleccionada
	var id = $("#listaConteos").find("option:selected").val(); // texto de la opción seleccionada
	var $listaOpciones = $("#listaOpcionesConteo");

	// limpiar opciones
	$listaOpciones.empty();

	if (id === "D") {
		// agregar opciones 1 y 2
		$listaOpciones.append(new Option("1", "1"));
		$listaOpciones.append(new Option("2", "2"));
	} else {
		// agregar solo opción 1
		$listaOpciones.append(new Option("1", "1"));
	}
}

//#### FIN Region Carga de datos en tab 'Carga de Inventario' ####//

function DeshabilitarDatosInventario() {
	// Deshabilitar controles
	$("#divDatosDeInventario").find("input, select, textarea, button").prop("disabled", true);
	//$("#divGrillasAdicionales").find("input, select, textarea, button").prop("disabled", true);

	// Bloquear selección de filas en todas las grillas
	//$("#divDatosDeInventario, #divGrillasAdicionales").find("table tbody tr").addClass("disabled-row");
}

function HabilitarDatosInventario() {
	// Habilitar controles
	$("#divDatosDeInventario").find("input, select, textarea, button").prop("disabled", false);
	//$("#divGrillasAdicionales").find("input, select, textarea, button").prop("disabled", false);

	// Permitir selección de filas nuevamente
	//$("#divDatosDeInventario, #divGrillasAdicionales").find("table tbody tr").removeClass("disabled-row");
}

function DeshabilitarDatosAdicionales() {
	$("#divGrillasAdicionales").find("input, select, textarea, button").prop("disabled", true);

	$("#divGrillasAdicionales, #divGrillaRubros").find("table tbody tr").addClass("disabled-row");

}
function HabilitarDatosAdicionales() {
	$("#divGrillasAdicionales").find("input, select, textarea, button").prop("disabled", false);

	$("#divGrillasAdicionales, #divGrillaRubros").find("table tbody tr").removeClass("disabled-row");
}

function ActualizarEstadoDeBotonesPorEventos(estado) {
	accion = estado;
	if (estado === EstadoBtnEnDivPrincipal.AGREGAR || estado === EstadoBtnEnDivPrincipal.MODIFICAR || estado === EstadoBtnEnDivPrincipal.ELIMINAR) {
		$("#btnAgregar, #btnModificar, #btnEliminar, #btnRegStkCtrl, #btnValorizacion, #btnCerrarInv")
			.prop("disabled", true);
		$("#btnConfirmar, #btnCancelar")
			.prop("disabled", false);
	}
	else if (estado === EstadoBtnEnDivPrincipal.CONFIRMAR || estado === EstadoBtnEnDivPrincipal.CANCELAR) {
		$("#btnModificar, #btnEliminar, #btnRegStkCtrl, #btnValorizacion, #btnCerrarInv, #btnConfirmar, #btnCancelar")
			.prop("disabled", true);
		$("#btnAgregar")
			.prop("disabled", false);
	}
}

function ActualizarEstadoDeBotones() {
	// Cantidad de filas de datos (excluyendo el mensaje vacío)
	let filas = $("#tbGridInventario tbody tr").not(":has(td[colspan])").length;

	// Verificar si hay alguna fila seleccionada (con tu clase)
	let filaSeleccionada = $("#tbGridInventario tbody tr.selected-row").length > 0;

	// Primero deshabilitamos todos
	$("#btnAgregar, #btnModificar, #btnEliminar, #btnConfirmar, #btnCancelar, #btnRegStkCtrl, #btnValorizacion, #btnCerrarInv")
		.prop("disabled", true);

	if (filas === 0) {
		// 1) No hay datos → solo habilitar Agregar
		$("#btnAgregar").prop("disabled", false);
	} else {
		if (!filaSeleccionada) {
			// 2) Hay datos pero ninguna fila seleccionada → solo habilitar Agregar
			$("#btnAgregar").prop("disabled", false);
		} else {
			// 3) Hay datos y una fila seleccionada → habilitar todos
			$("#btnAgregar, #btnModificar, #btnEliminar, #btnConfirmar, #btnCancelar, #btnRegStkCtrl, #btnValorizacion, #btnCerrarInv")
				.prop("disabled", false);
		}
	}
}

function ActualizarEstadoDeBotonesEnSeleccion() {
	if (inveIdSeleccionado === "" || inveIdSeleccionado === null || inveIdSeleccionado === undefined) {
		$("#btnModificar, #btnEliminar").prop("disabled", true);
	}
	else {
		if (inveIdSeleccionado === "P") {
			$("#btnModificar, #btnEliminar").prop("disabled", false);
		}
		else if (inveIdSeleccionado === "S") {
			$("#btnModificar").prop("disabled", false);
		}
		else {
			$("#btnModificar, #btnEliminar").prop("disabled", true);
		}
	}
}

function selectReg(x, gridId) {
	$("#" + gridId + " tbody tr").each(function (index) {
		$(this).removeClass("selected-row");
		$(this).removeClass("selectedEdit-row");
	});
	$(x).addClass("selected-row");
	invNroSeleccionado = x.getAttribute("data-inv-nro");
	inveIdSeleccionado = x.getAttribute("data-inve-id");
	
	ActualizarEstadoDeBotonesEnSeleccion();

	CargarGrillaRubrosEnSeccionDatosAdicionales(invNroSeleccionado);
	CargarGrillaUsuariosEnSeccionDatosAdicionales(invNroSeleccionado);
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

function InicializarDatosDeInventario() {
	$("#inv_nro").val("");
	$("#listaDepositos").val("");
	$("#listaConteos").val("");
	$("#txtDescripcion").val("");
	$("#txtEstado").val("");
	$("#txtAS_N").val("");
	InicializarFechasEnDatos();
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

function validarFechasAperturaYCierre() {
	var desdeVal = $("#dtAperturaDesde").val();
	var hastaVal = $("#dtAperturaHasta").val();

	// Validar que no estén vacíos
	if (!desdeVal || !hastaVal) {
		return "Debe seleccionar ambas fechas de apertura.";
	}

	// Convertir a objetos Date
	var fechaDesde = new Date(desdeVal);
	var fechaHasta = new Date(hastaVal);

	// Validar que sean fechas válidas
	if (isNaN(fechaDesde.getTime()) || isNaN(fechaHasta.getTime())) {
		return "Las fechas seleccionadas no son válidas.";
	}

	// Validar que desde <= hasta
	if (fechaDesde > fechaHasta) {
		return "La fecha 'Desde' no puede ser mayor que la fecha 'Hasta'.";
	}

	return ""; // todo correcto
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

function selectItemGrillaUsuarios(x) { }

function eliminarItemRubro(inv_nro, rub_id) {
}

function eliminarItemUsuario(inv_nro, usr_id) {
}