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
var invtIdSeleccionado = "";
$(function () {
	InicializarVista();
});

function InicializarVista() {
	if ($("#divDetalle").is(":visible")) {
		$("#divDetalle").collapse("hide");
	}
	$("#divFiltro").collapse("show");
	$("#lbChkDesdeHasta").text("Fecha de Inventario")

	$("#chkDesdeHasta").on("change", actualizarDesdeHasta);

	$("#chkDesdeHasta").prop("checked", true);
	actualizarDesdeHasta();

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

	$("#Date1, #Date2").on("blur keydown change", function (e) {
		if (e.type === "keydown" && e.key !== "Enter") return;

		const v = $(this).val();
		if (esFechaISOCompleta(v)) {
			finalizarEdicionFechas();
		}
	});

	InicializarFechasEnFiltros();
}

function actualizarDesdeHasta() {
	const habilitado = $("#chkDesdeHasta").is(":checked");

	$("#Date1").prop("disabled", !habilitado);
	$("#Date2").prop("disabled", !habilitado);

	if (habilitado) {
		$("#Date1").trigger("focus");
	}
}


function InicializarBusqueda() {
	var data = {};
	AbrirWaiting("Inicializando presentación de vista de inventario...");
	PostGenHtml(data, inicializarPantallPrincipalURL, function (obj) {
		$("#divDetalle").html(obj);
		$("#divFiltro").collapse("hide");
		$("#divDetalle").collapse("show");
		$("#btnTabValorizacion").addClass("tab-disabled");
		$("#btnTabCerrarInv").addClass("tab-disabled");
		CerrarWaiting();
		CargarInventarioLista();
		setTimeout(() => {
			CargarCamposDatosInventario();
			CargarDatosAdicionalesInicial();
			CargarEventosABotonesEnDivPrincipal();
			CerrarWaiting();
		}, 1000);
		return true
	});
}

function CargarEventosABotonesEnDivPrincipal() {
	$(document).on("click", "#btnAgregar", ControlaAgregarInventario);
	$(document).on("click", "#btnModificar", ControlaModificarInventario);
	$(document).on("click", "#btnEliminar", ControlaEliminarInventario);
	$(document).on("click", "#btnRegStkCtrl", ControlaRegStkCtrl);
	$(document).on("click", "#btnValorizacion", ControlaValorizacion);
	$(document).on("click", "#btnCerrarInv", ControlaCerrarInventario);
	$(document).on("click", "#btnConfirmar", ControlaConfirmarInventario);
	$(document).on("click", "#btnCancelar", ControlaCancelarInventario);
}

function ControlaAgregarInventario() {
	HabilitarDatosInventario();
	HabilitarDatosAdicionales();
	DeshabilitarGrillaInventarios();

	ActualizarEstadoDeBotonesPorEventos(EstadoBtnEnDivPrincipal.AGREGAR)
}

function ControlaModificarInventario() {
	HabilitarDatosInventario();
	HabilitarDatosAdicionales();
	DeshabilitarGrillaInventarios();

	ActualizarEstadoDeBotonesPorEventos(EstadoBtnEnDivPrincipal.MODIFICAR)
	$("#listaDepositos").trigger("focus");
}

function ControlaEliminarInventario() {
	DeshabilitarGrillaInventarios();
	DeshabilitarDatosInventario();
	DeshabilitarDatosAdicionales();
	ActualizarEstadoDeBotonesPorEventos(EstadoBtnEnDivPrincipal.ELIMINAR)
	$("#btnConfirmar").trigger("focus");
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
				//Limpiar datos en controles
				InicializarDatosDeInventario();
				//Limpiar grillas adicionales
				LimpiarGrillasEnDatosAdicionales();
				//limpiar seleccion en selectLista de grillas adicionales
				LimiparSelectListEnDatosAdicionales()
				//Actualizar estado de botones
				ActualizarEstadoDeBotonesPorEventos(EstadoBtnEnDivPrincipal.CONFIRMAR)
				//RecargarGrilla
				CargarInventarioLista();
				//Habilito lista principal
				HabilitarGrillaInventarios();
				//Deshabilitar Grillas adicionales y datos adicionales
				DeshabilitarDatosAdicionales();
			}, 500);
		}
	});

}

function ControlaCancelarInventario() {
	BlanquearControlesEnDatosDeInventario();
	DeshabilitarDatosInventario();
	DeshabilitarDatosAdicionales();
	InicializarFechasEnDatos();
	HabilitarGrillaInventarios();
	ActualizarEstadoDeBotonesPorEventos(EstadoBtnEnDivPrincipal.CANCELAR)
}

function ControlaRegStkCtrl() {
	AbrirMensaje("ATENCIÓN", `¿Esta seguro que desea Registrar el Stock de Control?`, function (e) {
		$("#msjModal").modal("hide");
		switch (e) {
			case "SI":
				HandlerRegStkCtrl();
				break;
			case "NO":
				break;
			default: //NO
				break;
		}
		return true;

	}, true, ["Aceptar", "Cancelar"], "question!", null);
}

function HandlerRegStkCtrl() {
	let inv_nro = $("#inv_nro").val();
	var data = { inv_nro };
	PostGen(data, registrarStockDeControlURL, function (obj) {
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
				//Limpiar datos en controles
				InicializarDatosDeInventario();
				//Limpiar grillas adicionales
				LimpiarGrillasEnDatosAdicionales();
				//limpiar seleccion en selectLista de grillas adicionales
				LimiparSelectListEnDatosAdicionales()
				//Actualizar estado de botones
				ActualizarEstadoDeBotonesPorEventos(EstadoBtnEnDivPrincipal.REGISTRAR_STOCK_DE_CONTROL)
				//RecargarGrilla
				CargarInventarioLista();
				//Habilito lista principal
				HabilitarGrillaInventarios();
				//Deshabilitar Grillas adicionales y datos adicionales
				DeshabilitarDatosAdicionales();
			}, 500);
		}
	});
}

function ControlaValorizacion() {
	TaskManager.start();
	let inv_nro = $("#inv_nro").val();
	var data = { inv_nro };
	PostGenHtml(data, inicializarTabValorizacionURL, function (obj) {
		$("#divValorizacion").html(obj);
		TaskManager.end();
		let tab = new bootstrap.Tab(document.querySelector("#btnTabValorizacion"));
		setTimeout(() => {
			$("#divEdicionConteos").find("input, select, textarea, button").prop("disabled", true);
			$(document).off("click", "#btnConfirmarValoracion").on("click", "#btnConfirmarValoracion", ControlaConfirmarValoracion);
		}, 300);
		tab.show();
		return true
	});
}

function ControlaConfirmarValoracion() {
	if (ValidarExistenciaDeInventario()) {
		AbrirMensaje("ATENCIÓN", `Esta acción bloqueará el inventario actual para realizar conteos y/o modificaciones. ¿Desea continuar?`, function (e) {
			$("#msjModal").modal("hide");
			switch (e) {
				case "SI":
					HandlerConfirmarValoracion();
					break;
				case "NO":
					break;
				default: //NO
					break;
			}
			return true;

		}, true, ["Aceptar", "Cancelar"], "question!", null);
	}
}

function HandlerConfirmarValoracion() {
	TaskManager.start();
	let inv_nro = $("#inv_nro").val();
	var data = { inv_nro };
	PostGen(data, registrarValorizacionURL, function (obj) {
		TaskManager.end();
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
				//Limpiar datos en controles
				InicializarDatosDeInventario();
				//Limpiar grillas adicionales
				LimpiarGrillasEnDatosAdicionales();
				//limpiar seleccion en selectLista de grillas adicionales
				LimiparSelectListEnDatosAdicionales()
				//Actualizar estado de botones
				ActualizarEstadoDeBotonesPorEventos(EstadoBtnEnDivPrincipal.VALORIZACION)
				//RecargarGrilla
				CargarInventarioLista();
				//Habilito lista principal
				HabilitarGrillaInventarios();
				//Deshabilitar Grillas adicionales y datos adicionales
				DeshabilitarDatosAdicionales();
				//Movemos al tab principal
				let tab = new bootstrap.Tab(document.querySelector("#btnTabCargaInventario"));
				tab.show();
			}, 500);
		}
	});
}

function ValidarExistenciaDeInventario() {
	let invtId = $("#invt_id").val();

	if (invtId == "B") {
		let filasBox = $("#tbValorGridBox tbody tr");
		// Validar Box: al menos una fila y que no sea la de "No se encontraron box"
		let tieneBox = filasBox.length > 0 &&
			!filasBox.first().text().includes("No se encontraron box");

		return tieneBox;
	}
	else {
		let filasRubros = $("#tbValorGridRubros tbody tr");
		// Validar Usuarios: al menos una fila y que no sea la de "No hay rubros cargados"
		var tieneRubros = filasRubros.length > 0 &&
			!filasRubros.first().text().includes("No se encontraron rubros");

		return tieneRubros;
	}
}

function ValidarExistenciaDeInventarioParaCierre() {
	let invtId = $("#invt_id").val();

	if (invtId == "B") {
		let filasBox = $("#tbCerrarGridBox tbody tr");
		// Validar Box: al menos una fila y que no sea la de "No se encontraron box"
		let tieneBox = filasBox.length > 0 &&
			!filasBox.first().text().includes("No se encontraron box");

		return tieneBox;
	}
	else {
		let filasRubros = $("#tbCerrarGridRubros tbody tr");
		// Validar Usuarios: al menos una fila y que no sea la de "No hay rubros cargados"
		var tieneRubros = filasRubros.length > 0 &&
			!filasRubros.first().text().includes("No se encontraron rubros");

		return tieneRubros;
	}
}

function ControlaCerrarInventario() {
	TaskManager.start();
	let inv_nro = $("#inv_nro").val();
	var data = { inv_nro };
	PostGenHtml(data, inicializarTabCerrarInventarioURL, function (obj) {
		$("#divCerrarInv").html(obj);
		TaskManager.end();
		let tab = new bootstrap.Tab(document.querySelector("#btnTabCerrarInv"));
		setTimeout(() => {
			//$("#divEdicionConteos").find("input, select, textarea, button").prop("disabled", true);
			$(document).off("click", "#btnConfirmarCierre").on("click", "#btnConfirmarCierre", ControlaConfirmarCierreDeInventario);
		}, 300);
		actualizarCheckHeader();
		tab.show();
		return true
	});
}

function ControlaConfirmarCierreDeInventario() {
	if (ValidarExistenciaDeInventarioParaCierre()) {
		AbrirMensaje("ATENCIÓN", `Esta acción realizará el ajuste de los productos seleccionado y se cerrará el inventario. ¿Desea continuar?`, function (e) {
			$("#msjModal").modal("hide");
			switch (e) {
				case "SI":
					HandlerConfirmarCierre();
					break;
				case "NO":
					break;
				default: //NO
					break;
			}
			return true;

		}, true, ["Aceptar", "Cancelar"], "question!", null);
	}
}

function HandlerConfirmarCierre() {
	TaskManager.start();
	let inv_nro = $("#inv_nro").val();
	var data = { inv_nro };
	PostGen(data, registrarCierreURL, function (obj) {
		TaskManager.end();
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
				//Limpiar datos en controles
				InicializarDatosDeInventario();
				//Limpiar grillas adicionales
				LimpiarGrillasEnDatosAdicionales();
				//limpiar seleccion en selectLista de grillas adicionales
				LimiparSelectListEnDatosAdicionales()
				//Actualizar estado de botones
				ActualizarEstadoDeBotonesPorEventos(EstadoBtnEnDivPrincipal.CERRAR_INVENTARIO)
				//RecargarGrilla
				CargarInventarioLista();
				//Habilito lista principal
				HabilitarGrillaInventarios();
				//Deshabilitar Grillas adicionales y datos adicionales
				DeshabilitarDatosAdicionales();
				//Movemos al tab principal
				let tab = new bootstrap.Tab(document.querySelector("#btnTabCargaInventario"));
				tab.show();
			}, 500);
		}
	});
}

function onProductoSeleccionado(pId, isChecked) {
	console.log("Producto seleccionado:", pId, "Estado:", isChecked);
	let p_id = pId;
	let ps_ajuste = isChecked;
	var data = { p_id, ps_ajuste, tipo_id };
	PostGen(data, marcarProductoEnCierreParaAjustarURL, function (obj) {
		TaskManager.end();
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
			actualizarCheckHeader();
		}
	});
}

function onProductosSeleccionados(isChecked) {
	const seleccionados = window.obtenerProdSeleccionados();
	let ps_ajuste = isChecked;
	var data = { seleccionados, ps_ajuste, tipo_id };
	PostGen(data, marcarProductosEnCierreParaAjustarURL, function (obj) {
		TaskManager.end();
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
			//Podria hacer algo luego de actualizar
		}
	});
}

function actualizarCheckHeader() {
	const total = $(".check-prod").length;
	const marcados = $(".check-prod:checked").length;

	$("#checkAllProdEnInve").prop("checked", total > 0 && total === marcados);
}

function LimpiarGrillasEnDatosAdicionales() {
	CargarGrillaRubrosEnSeccionDatosAdicionales();
	CargarGrillaUsuariosEnSeccionDatosAdicionales();
}

function LimiparSelectListEnDatosAdicionales() {
	TaskManager.start();
	// Si está visible el de Sector
	if ($("#divListaSector").is(":visible")) {
		$("#listaSectores").val("");   // vuelve a "Seleccionar"
	}

	// Si está visible el de Rubro
	if ($("#divListaRubro").is(":visible")) {
		$("#listaRubros").val("");     // vuelve a "Seleccionar"
	}
	$("#listaUsuarios").val("");
	TaskManager.end();
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




function BlanquearControlesEnDatosDeInventario() {
	// Limpiar todos los inputs de texto y fecha
	$("#divDatosDeInventario").find("input[type=text], input[type=date]").val("");

	// Resetear todos los selects a su primer opción
	$("#divDatosDeInventario").find("select").each(function () {
		$(this).prop("selectedIndex", 0);
	});
}

function CargarDatosAdicionalesInicial() {
	TaskManager.start();
	var data = {};
	PostGenHtml(data, cargarDatosAdicionalesInicialURL, function (obj) {
		$("#divGrillasAdicionales").html(obj);
		TaskManager.end();
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
	TaskManager.start();

	var inv_nro = invId;
	var data = { inv_nro };
	PostGenHtml(data, cargarGrillaRubrosEnSeccionDatosAdicionalesURL, function (obj) {
		$("#divGrillaRubros").html(obj);
		TaskManager.end();
		return true
	});
}

function CargarGrillaUsuariosEnSeccionDatosAdicionales(invId = 0) {
	TaskManager.start();

	var inv_nro = invId;
	var data = { inv_nro };
	PostGenHtml(data, cargarGrillaUsuariosEnSeccionDatosAdicionalesURL, function (obj) {
		$("#divGrillaUsuarios").html(obj);
		TaskManager.end();
		return true
	});
}

function CargarDatosDeInvEnSeccionDatosAdicionales(invId) {
	var inv_nro = invId;
	var data = { inv_nro };
	PostGenHtml(data, cargarDatosDeInvEnSeccionDatosAdicionalesURL, function (obj) {
		$("#divDatosDeInventario").html(obj);
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
	TaskManager.start();
	var desde = $("#Date1").val();
	var hasta = $("#Date2").val();
	var data = { desde, hasta };
	PostGenHtml(data, buscarInventarioListaURL, function (obj) {
		$("#divGrillaInventario").html(obj);
		TaskManager.end();
		return true
	});
}

function CargarCamposDatosInventario(invId = 0) {
	TaskManager.start();

	var inv_nro = invId;
	var data = { inv_nro };
	PostGenHtml(data, cargarCamposDatosInventarioURL, function (obj) {
		$("#divDatosDeInventario").html(obj);
		TaskManager.end();
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
		if (invId == 0)
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

function HabilitarGrillaInventarios() {
	$("#divGrillaInventario").find("table tbody tr").removeClass("disabled-row");
}

function DeshabilitarGrillaInventarios() {
	$("#divGrillaInventario").find("table tbody tr").addClass("disabled-row");
}

function DeshabilitarDatosAdicionales() {
	$("#divGrillasAdicionales").find("input, select, textarea, button").prop("disabled", true);

	$("#divGrillaUsuarios, #divGrillaRubros").find("table tbody tr").addClass("disabled-row");
	$("#divGrillaRubros").find("table tbody tr").addClass("disabled-row");

}
function HabilitarDatosAdicionales() {
	$("#divGrillasAdicionales").find("input, select, textarea, button").prop("disabled", false);

	$("#divGrillaUsuarios, #divGrillaRubros").find("table tbody tr").removeClass("disabled-row");
}

function ActualizarEstadoDeBotonesPorEventos(estado) {
	accion = estado;
	if (estado === EstadoBtnEnDivPrincipal.AGREGAR || estado === EstadoBtnEnDivPrincipal.MODIFICAR || estado === EstadoBtnEnDivPrincipal.ELIMINAR) {
		$("#btnAgregar, #btnModificar, #btnEliminar, #btnRegStkCtrl, #btnValorizacion, #btnCerrarInv")
			.prop("disabled", true);
		$("#btnConfirmar, #btnCancelar")
			.prop("disabled", false);
	}
	else if (estado === EstadoBtnEnDivPrincipal.CONFIRMAR || estado === EstadoBtnEnDivPrincipal.CANCELAR || estado === EstadoBtnEnDivPrincipal.REGISTRAR_STOCK_DE_CONTROL || estado === EstadoBtnEnDivPrincipal.VALORIZACION || estado === EstadoBtnEnDivPrincipal.CERRAR_INVENTARIO) {
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
		$("#btnModificar, #btnEliminar, #btnRegStkCtrl, #btnValorizacion, #btnCerrarInv").prop("disabled", true);
	}
	else {
		if (inveIdSeleccionado === "P") {
			$("#btnModificar, #btnEliminar, #btnRegStkCtrl").prop("disabled", false);
			$("#btnValorizacion, #btnCerrarInv").prop("disabled", true);
		}
		else if (inveIdSeleccionado === "S") {
			$("#btnValorizacion, #btnModificar").prop("disabled", false);
			$("#btnRegStkCtrl, #btnCerrarInv").prop("disabled", true);
		}
		else if (inveIdSeleccionado === "V") {
			$("#btnCerrarInv").prop("disabled", false);
			$("#btnRegStkCtrl, #btnValorizacion, #btnModificar").prop("disabled", true);
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
	if (gridId == 'tbGridInventario') {
		invNroSeleccionado = x.getAttribute("data-inv-nro");
		inveIdSeleccionado = x.getAttribute("data-inve-id");
		invtIdSeleccionado = x.getAttribute("data-invt-id");

		setTimeout(() => {
			ActualizarEstadoDeBotonesEnSeleccion();
		}, 100);
		setTimeout(() => {
			CargarGrillaUsuariosEnSeccionDatosAdicionales(invNroSeleccionado);
		}, 100);
		setTimeout(() => {
			CargarGrillaRubrosEnSeccionDatosAdicionales(invNroSeleccionado);
		}, 300);
		setTimeout(() => {
			CargarCamposDatosInventario(invNroSeleccionado);
		}, 300);
		setTimeout(() => {
			DeshabilitarDatosAdicionales();
		}, 1000);
	}
	if (gridId == 'tbValorGridBox') {
		TaskManager.start();
		let inv_nro = x.getAttribute("data-inv-nro");
		tipo = "B";
		tipo_id = x.getAttribute("data-inve-id");
		var data = { inv_nro, tipo, tipo_id };
		PostGenHtml(data, obtenerProductosEnValorizacionURL, function (obj) {
			$("#divProductosValorizacion").html(obj);
			TaskManager.end();
			return true
		});
	}
	if (gridId == 'tbValorGridRubros') {
		TaskManager.start();
		let inv_nro = x.getAttribute("data-inv-nro");
		tipo = "R";
		tipo_id = x.getAttribute("data-inve-id");
		var data = { inv_nro, tipo, tipo_id };
		PostGenHtml(data, obtenerProductosEnValorizacionURL, function (obj) {
			$("#divProductosValorizacion").html(obj);
			TaskManager.end();
			return true
		});
	}
	if (gridId == 'tbValorGridProductos') {
		TaskManager.start();
		let inv_nro = $("#inv_nro").val();
		let p_id = x.getAttribute("data-p-id");
		var data = { inv_nro, tipo, tipo_id, p_id };
		PostGenHtml(data, obtenerConteosEnValorizacionURL, function (obj) {
			$("#divConteosValorizacion").html(obj);
			TaskManager.end();
			getMaskForIntegerMax1000("#conteo");
			HabilitarSeccionEdicionDeConteo();
			return true
		});
	}
	if (gridId == 'tbCerrarGridBox') {
		TaskManager.start();
		let inv_nro = x.getAttribute("data-inv-nro");
		tipo = "B";
		tipo_id = x.getAttribute("data-inve-id");
		var data = { inv_nro, tipo, tipo_id };
		PostGenHtml(data, obtenerProductosEnCierreURL, function (obj) {
			$("#divProductosCierre").html(obj);
			actualizarCheckHeader();
			TaskManager.end();
			return true
		});
	}
	if (gridId == 'tbCerrarGridRubros') {
		TaskManager.start();
		let inv_nro = x.getAttribute("data-inv-nro");
		tipo = "R";
		tipo_id = x.getAttribute("data-inve-id");
		var data = { inv_nro, tipo, tipo_id };
		PostGenHtml(data, obtenerProductosEnCierreURL, function (obj) {
			$("#divProductosCierre").html(obj);
			actualizarCheckHeader();
			TaskManager.end();
			return true
		});
	}
}

function HabilitarSeccionEdicionDeConteo() {

	var filasConteos = $("#tbValorGridConteos tbody tr");

	// Validar Conteos: al menos una fila y que no sea la de "No hay conteos cargados"
	var tieneConteos = filasConteos.length > 0 &&
		!filasConteos.first().text().includes("No se encontraron conteos");

	if (!tieneConteos) {
		$("#divEdicionConteos").find("input, select, textarea, button").prop("disabled", true);
	}
	else {
		$("#divEdicionConteos").find("input, select, textarea, button").prop("disabled", false);
	}
}

let tipo = "";
let tipo_id = "";
let pendingTasks = 0;
function startTask() {
	if (pendingTasks === 0) {
		AbrirWaiting();
	}
	pendingTasks++;
}
function endTask() {
	pendingTasks--;
	if (pendingTasks <= 0) {
		pendingTasks = 0;
		CerrarWaiting();
	}
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
	TaskManager.start();
	$("#inv_nro").val("");
	$("#listaDepositos").val("");
	$("#listaConteos").val("");
	$("#txtDescripcion").val("");
	$("#txtEstado").val("");
	$("#txtAS_N").val("");
	InicializarFechasEnDatos();
	TaskManager.end();
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
	TaskManager.start();
	var data = { inv_nro, rub_id };
	PostGenHtml(data, quitarItemEnGrillaRubroURL, function (obj) {
		$("#divGrillaRubros").html(obj);
		TaskManager.end();
		return true
	});
}

function eliminarItemUsuario(inv_nro, usr_id) {
	TaskManager.start();
	console.log(inv_nro, usr_id);
	data = { inv_nro, usr_id };
	PostGenHtml(data, quitarItemEnGrillaUsuariosURL, function (obj) {
		$("#divGrillaUsuarios").html(obj);
		TaskManager.end();
		return true
	});
}

function getMaskForIntegerMax1000(selector) {
	$(selector).inputmask({
		alias: 'numeric',
		groupSeparator: '.',       // separador de miles
		digits: 0,                 // sin decimales
		digitsOptional: false,
		allowMinus: false,
		prefix: '',
		suffix: '',
		rightAlign: true,
		unmaskAsNumber: true,
		min: 0,
		max: 1000
	});
}