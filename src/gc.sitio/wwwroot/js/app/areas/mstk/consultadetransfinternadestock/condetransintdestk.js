let sucEnvIdsList = [];
let sucRecIdsList = [];
let tiposIdsList = [];
let tabsDetallePendientes = 0;

const TabToTableMap = {
	"navs-top-trans": "#tbTransferencias",
	"navs-top-cont": "#tbDetalleConteos",
	"navs-top-rem": "#tbRemito"
};

$(function () {

	InicializarCamposEnFiltros();

	$(document).on("change", "#listaSucursalesEnvia", ControlalistaSucursalesEnviaSelected);
	$(document).on("change", "#listaSucursalesRecibe", ControlalistaSucursalesRecibeSelected);
	$(document).on("change", "#listaTipos", ControlalistaTiposSelected);

	$("#SucursalesEnviaList").on("dblclick", 'option', function () { $(this).remove(); })
	$("#SucursalesRecibeList").on("dblclick", 'option', function () { $(this).remove(); })
	$("#TiposList").on("dblclick", 'option', function () { $(this).remove(); })

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
		InicializarPantallaPrincipal();
	});
});


function EvaluarBotonImprimir(tabId) {
	console.log("Evaluando botón imprimir para tab:", tabId);
	const tablaSelector = TabToTableMap[tabId];
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

function InicializarPantallaPrincipal() {
	var tipos = $("#listaTipos").val();
	var sucEnv = ObtenerSucursalesSeleccionadasConTexto("SucursalesEnviaList", "listaSucursalesEnvia");
	var sucRec = ObtenerSucursalesSeleccionadasConTexto("SucursalesRecibeList", "listaSucursalesRecibe");

	if (!tipos || tipos == null || tipos == undefined || tipos == "") {
		AbrirMensaje("ATENCIÓN", "Debe seleccionar al menos un Tipo", function () {
			$("#msjModal").modal("hide");
			$("#listaTipos").trigger('focus');
			return true;
		}, false, ["Aceptar"], "error!", null);
		return;
	}

	const resultado = EvaluarSeleccionDeTipo(tipos, sucEnv.ids, sucEnv.todos_seleccionados, sucRec.ids, sucRec.todos_seleccionados);
	if (!resultado.ok) {
		AbrirMensaje("ATENCIÓN", resultado.mensaje, function () {
			$("#msjModal").modal("hide");
			return true;
		}, false, ["Aceptar"], "error!", null);
		return;
	}

	// ✔ Si tipo != 'S', igualamos sucEnv = sucRec
	if (resultado.sucEnvIgualado) {
		sucEnv.ids = resultado.sucEnvIgualado;
		sucEnv.textos = sucRec.textos; // mantenemos coherencia visual
	}

	var sucursalesEnvText = sucEnv.textos;
	var sucursalesRecText = sucRec.textos;
	var tiposText = $("#listaTipos option:selected").text();
	sucEnvIdsList = sucEnv.ids;
	sucRecIdsList = sucRec.ids;
	tiposIdsList = tipos;
	var desde = $("#Desde").val();
	var hasta = $("#Hasta").val();
	AbrirWaiting("Cargando información...");
	PostGenHtml({ sucursalesEnvText, sucursalesRecText, tiposText, desde, hasta }, inicializarPantallPrincipalURL, function (obj) {
		$("#divDetalle").html(obj);
		$(document).on('shown.bs.tab', 'button[data-bs-toggle="tab"]', function (e) {
			const tabId = $(e.target).attr("data-bs-target").replace("#", "");
			EvaluarBotonImprimir(tabId);
		});
		$("#divFiltros").collapse("hide");
		$("#divDetalle").collapse("show");
		CerrarWaiting();
		setTimeout(() => {
			CargarTablaTabTransferencias(tiposIdsList, sucEnvIdsList, sucRecIdsList, desde, hasta);
		}, 100);
		return true
	});
}

function FinalizarCargaDetalle() {
	tabsDetallePendientes--;

	if (tabsDetallePendientes <= 0) {
		CerrarWaiting();
	}
}

function HabilitarTabRemito(habilitar) {
	const $tab = $("#btnTabRemito");

	if (habilitar) {
		$tab.removeClass("tab-disabled");
		$tab.removeAttr("disabled");
	} else {
		$tab.addClass("tab-disabled");
		$tab.attr("disabled", "disabled");
	}
}

function HabilitarTabRemito(habilitar, mensajeTooltip) {
	const $tab = $("#btnTabRemito");

	if (habilitar) {
		// Quitar clases y disabled
		$tab.removeClass("tab-disabled tab-clicked");
		$tab.removeAttr("disabled");

		// Restaurar comportamiento de TAB
		$tab.attr("data-bs-toggle", "tab");

		// Eliminar tooltip si existía
		const instance = bootstrap.Tooltip.getInstance($tab[0]);
		if (instance) {
			instance.dispose();
		}

		// Limpiar atributos que Bootstrap deja pegados
		$tab.removeAttr("data-bs-original-title");
		$tab.removeAttr("aria-describedby");
		$tab.removeAttr("title");

		// Quitar handler de click especial
		$tab.off("click.tooltip");

		return;
	}

	// -------------------------
	// Caso DESHABILITADO
	// -------------------------

	$tab.addClass("tab-disabled");
	$tab.attr("disabled", "disabled");

	const msg = mensajeTooltip || "El remito no está disponible para esta transferencia";

	// Tooltip
	$tab.attr("title", msg);
	$tab.attr("data-bs-toggle", "tooltip");

	new bootstrap.Tooltip($tab[0]);

	// Mostrar tooltip al hacer clic
	$tab.off("click.tooltip").on("click.tooltip", function (e) {
		e.preventDefault();
		$tab.addClass("tab-clicked");

		const tooltip = bootstrap.Tooltip.getInstance($tab[0]);
		tooltip.show();

		setTimeout(() => $tab.removeClass("tab-clicked"), 200);
	});
}

function SeleccionarTransferencia(x, grid) {
	var $row = $(x);

	// Seleccionar visualmente la fila
	//selectReg(x, grid); Descomentar si no se visualiza la seleccion de la fila

	// Obtener valores desde los atributos data-*
	var ti = $row.data("ti");
	var pv_compte = $row.data("pv-compte");
	var re_compte = $row.data("re-compte");
	AbrirWaiting("Cargando datos..."); // ← abrir al inicio
	tabsDetallePendientes = 2; // ← cantidad de tabs a cargar
	consultarConteos(ti);
	// Validación para habilitar o no el tab Remito
	if (pv_compte && re_compte && pv_compte !== "" && re_compte !== "") {
		HabilitarTabRemito(true, "");
		consultarRemito(re_compte);
	} else {
		HabilitarTabRemito(false, "El remito no está disponible para esta transferencia");
	}
	FinalizarCargaDetalle(); // ← marcar como completado
}

function consultarConteos(ti) {
	PostGenHtml({ ti }, consultarConteosURL, function (obj) {
		$("#divConteos").html(obj);
		FinalizarCargaDetalle();
		CerrarWaiting();
		return true
	});
}

function consultarRemito(re_compte) {
	PostGenHtml({ re_compte }, consultarRemitoURL, function (obj) {
		$("#divRemito").html(obj);
		FinalizarCargaDetalle();
		CerrarWaiting();
		return true
	});
}

function EvaluarSeleccionDeTipo(tipo, sucEnv, sucEnv_Todos, sucRec, sucRec_Todos) {

	// Convertir strings "000,001,002" en arrays ["000","001","002"]
	const env = sucEnv ? sucEnv.split(',').map(x => x.trim()) : [];
	const rec = sucRec ? sucRec.split(',').map(x => x.trim()) : [];

	// Caso 1: tipo = 'S'
	if (tipo === 'S' && (!sucEnv_Todos && !sucRec_Todos)) {
		// Buscar intersección entre env y rec
		const repetidos = env.filter(x => rec.includes(x));

		if (repetidos.length > 0) {
			return {
				ok: false,
				mensaje: `Las siguientes sucursales no pueden estar en Envío y Recepción al mismo tiempo: ${repetidos.join(', ')}`
			};
		}

		return { ok: true, mensaje: "" };
	}

	// Caso 2: tipo distinto de 'S'
	// sucEnv debe igualarse a sucRec
	return {
		ok: true,
		mensaje: "",
		sucEnvIgualado: rec.join(',') // por si necesitás devolverlo
	};
}


function CargarTablaTabTransferencias(tipoIdsLista, sucursalEnvioIdsLista, sucursalRecibeIdsLista, desde, hasta) {
	AbrirWaiting("Cargando transferencias...");
	PostGenHtml({ tipoIdsLista, sucursalEnvioIdsLista, sucursalRecibeIdsLista, desde, hasta }, cargarTabTransferenciasURL, function (obj) {
		$("#divTransferencias").html(obj);
		InicializarEventosTabTransferencias();
		// Seleccionar automáticamente la primera transferencia válida
		const $primeraFila = $("#tbTransferencias tbody tr").not(".fila-vacia").first();

		if ($primeraFila.length > 0) {
			// Marcar visualmente
			$primeraFila.addClass("selected-row");

			// Ejecutar la misma lógica que el doble clic
			SeleccionarTransferencia($primeraFila[0], "tbTransferencias");
		}

		setTimeout(() => {
			EvaluarBotonImprimir("navs-top-trans")
		}, 1000);
		CerrarWaiting();
		return true
	});
}

function InicializarEventosTabTransferencias() {
	$(document).off("click", "#tbTransferencias tbody tr");
	$(document).on("click", "#tbTransferencias tbody tr", function (e) {

		if ($(e.target).is("button, a, .btn, i")) return;

		const $nuevaFila = $(this);
		ProcesarSeleccionFilaEnTabTransferencias($nuevaFila);
	});
}

function InicializarEventosTabConteos() {
	$(document).off("click", "#tbDetalleConteos tbody tr");
	$(document).on("click", "#tbDetalleConteos tbody tr", function (e) {

		if ($(e.target).is("button, a, .btn, i")) return;

		const $nuevaFila = $(this);
		ProcesarSeleccionFilaEnTabConteos($nuevaFila);
	});
}

function InicializarEventosTabRemito() {
	$(document).off("click", "#tbRemito tbody tr");
	$(document).on("click", "#tbRemito tbody tr", function (e) {

		if ($(e.target).is("button, a, .btn, i")) return;

		const $nuevaFila = $(this);
		ProcesarSeleccionFilaEnTabRemito($nuevaFila);
	});
}

function ProcesarSeleccionFilaEnTabRemito($nuevaFila) {
	$("#tbRemito tbody tr").removeClass("selected-row");
	$fila.addClass("selected-row");
}

function ProcesarSeleccionFilaEnTabConteos($nuevaFila) {
	$("#tbDetalleConteos tbody tr").removeClass("selected-row");
	$fila.addClass("selected-row");
}

function ProcesarSeleccionFilaEnTabTransferencias($fila) {
	$("#tbTransferencias tbody tr").removeClass("selected-row");
	$fila.addClass("selected-row");
}

function InicializarCamposEnFiltros() {
	$("#btnImprimir").hide();
	$("#lbChkDesdeHasta").text("Fechas");
	$("#lbSucursalesEnvia").text("Sucursal que envía");
	$("#lbSucursalesRecibe").text("Sucursal que recibe");
	$("#lbTipo").text("Tipo");

	$("#chkDesdeHasta").prop('checked', true);
	$("#chkDesdeHasta").trigger("change");
	$("#chkDesdeHasta").prop('disabled', true);
	$("#chkSucursalesEnvia").prop('checked', false);
	$("#chkSucursalesEnvia").trigger("change");
	$("#chkSucursalesRecibe").prop('checked', false);
	$("#chkSucursalesRecibe").trigger("change");
	$("#chkTipo").prop('checked', true);
	$("#chkTipo").trigger("change");
	$("#chkTipo").prop('disabled', true);
	$("#listaTipos").prop('disabled', false);

	$("#SucursalesEnviaList").empty();
	$("#SucursalesRecibeList").empty();
	$("#TiposList").empty();

	$("#listaSucursalesEnvia").val("");
	$("#listaSucursalesRecibe").val("");
	$("#listaTipos").val("");

	$("#divFiltros").collapse("show");
	$("#divDetalle").collapse("hide");


	$("#chkSucursalesEnvia").on("click", function () {
		if ($("#chkSucursalesEnvia").is(":checked")) {
			$("#listaSucursalesEnvia").prop("disabled", false);
			$("#SucursalesEnviaList").prop("disabled", false);
			$("#listaSucursalesEnvia").trigger("focus");
		}
		else {
			$("#listaSucursalesEnvia").prop("disabled", true);
			$("#SucursalesEnviaList").prop("disabled", true);
			$("#listaSucursalesEnvia").val("");
			$("#SucursalesEnviaList").empty();
		}
	});

	$("#chkSucursalesRecibe").on("click", function () {
		if ($("#chkSucursalesRecibe").is(":checked")) {
			$("#listaSucursalesRecibe").prop("disabled", false);
			$("#SucursalesRecibeList").prop("disabled", false);
			$("#listaSucursalesRecibe").trigger("focus");
		}
		else {
			$("#listaSucursalesRecibe").prop("disabled", true);
			$("#SucursalesRecibeList").prop("disabled", true);
			$("#listaSucursalesRecibe").val("");
			$("#SucursalesRecibeList").empty();
		}
	});

	$("#chkTipo").on("click", function () {
		if ($("#chkTipo").is(":checked")) {
			$("#listaTipos").prop("disabled", false);
			$("#TiposList").prop("disabled", false);
			$("#listaTipos").trigger("focus");
		}
		else {
			$("#listaTipos").prop("disabled", true);
			$("#TiposList").prop("disabled", true);
			$("#listaTipos").val("");
			$("#TiposList").empty();
		}
	});
}

function ObtenerSucursalesSeleccionadasConTexto(sucList, suc) {

	let ids = [];
	let textos = [];
	let todos = false;

	// 1) Obtener sucursales seleccionadas en el ListBox
	$("#" + sucList + " option").each(function () {
		ids.push($(this).val());
		textos.push($(this).text());
	});

	// 2) Si NO hay ninguna seleccionada → devolver TODAS las del DropDownList
	if (ids.length === 0) {
		todos = true;
		$("#" + suc + " option").each(function () {
			const val = $(this).val();
			const txt = $(this).text();

			if (val && val !== "") {
				ids.push(val);
				textos.push(txt);
			}
		});
	}

	return {
		ids: ids.join(","),
		textos: textos.join(", "),
		todos_seleccionados: todos
	};
}

function ControlalistaSucursalesEnviaSelected() {
	var item = $("#listaSucursalesEnvia").val();
	var desc = $("#listaSucursalesEnvia option:selected").text();
	if ($("#SucursalesEnviaList").has('option:contains("' + item + '")').length === 0 && $("#SucursalesEnviaList").has('option:contains("' + desc + '")').length === 0) {
		var opc = "<option value=" + item + ">" + desc + "</option>"
		$("#SucursalesEnviaList").append(opc);
	}
}

function ControlalistaSucursalesRecibeSelected() {
	var item = $("#listaSucursalesRecibe").val();
	var desc = $("#listaSucursalesRecibe option:selected").text();
	if ($("#SucursalesRecibeList").has('option:contains("' + item + '")').length === 0 && $("#SucursalesRecibeList").has('option:contains("' + desc + '")').length === 0) {
		var opc = "<option value=" + item + ">" + desc + "</option>"
		$("#SucursalesRecibeList").append(opc);
	}
}

function ControlalistaTiposSelected() {
	var item = $("#listaTipos").val();
	var desc = $("#listaTipos option:selected").text();
	$("#TiposList").empty();
	var opc = "<option value=" + item + ">" + desc + "</option>"
	$("#TiposList").append(opc);
	//if ($("#TiposList").has('option:contains("' + item + '")').length === 0 && $("#TiposList").has('option:contains("' + desc + '")').length === 0) {
	//	var opc = "<option value=" + item + ">" + desc + "</option>"
	//	$("#TiposList").append(opc);
	//}
}