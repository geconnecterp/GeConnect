

$(function () {
	$("#pagEstado").on("change", function () {
		var div = $("#divPaginacion");
		cargaPaginacion();
	});

	InicializarCamposEnFiltros();

	$(document).on("change", "#listaSucursales", ControlalistaSucursalesSelected);

	$("#SucursalesList").on("dblclick", 'option', function () { $(this).remove(); })

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

function MostrarFiltrosAplicados() {
	const $target = $("#filtrosAplicadosFloating").length
		? $("#filtrosAplicadosFloating")
		: $("#filtrosAplicadosContainer");

	if ($target.length === 0) return;

	const desde = $("#Desde").val();
	const hasta = $("#Hasta").val();

	const suc = listFrom("SucursalesList");

	let html = `
        <div class="d-inline-flex align-items-center"
             style="gap:8px; white-space:nowrap;">
    `;

	if (desde)
		html += `<span class="badge bg-secondary">DESDE: ${desde}</span>`;

	if (hasta)
		html += `<span class="badge bg-secondary">HASTA: ${hasta}</span>`;

	html += renderGroup("SUC.", suc);
	html += `</div>`;

	$target.html(html);
}



// Mostrar filtros al cargar la pantalla
try { MostrarFiltrosAplicados(); } catch (e) { }

function InicializarPantallaPrincipal() {
	var suc = ObtenerSucursalesSeleccionadasConTexto("SucursalesList", "listaSucursales");

	var sucursalesText = suc.textos;
	sucIdsList = suc.ids;
	f_desde = $("#Desde").val();
	f_hasta = $("#Hasta").val();
	AbrirWaiting("Cargando información...");
	PostGenHtml({ sucursalesText, f_desde, f_hasta }, inicializarPantallPrincipalURL, function (obj) {
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
			CargarTablaTabAjustes(1);
		}, 100);
		return true
	});
}

let sucIdsList = null;
let provIdsList = null;
let f_desde = null;
let f_hasta = null;

function CargarTablaTabAjustes(pag = 1) {

	AbrirWaiting("Cargando ajustes de stock...");

	var buscaNew = true;
	pagina = pag;
	Pagina = pag;
	var sort = null;
	var sortDir = null
	var data2 = { sort, sortDir, Pagina, buscaNew }
	var data1 = { sucIdsList, f_desde, f_hasta };
	var data = $.extend({}, data1, data2);

	PostGenHtml(data, cargarAjustesDeStockURL, function (obj) {
		$("#divAjustes").html(obj);
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
		InicializarEventosTabAjustes();
		// 🔥 Seleccionar automáticamente la primera fila y cargar el detalle
		setTimeout(() => {
			const $primera = $("#tbAjustes tbody tr.row-ajuste").first();
			if ($primera.length) {
				ProcesarSeleccionFilaEnTabAjustes($primera); // marca visualmente
				SeleccionarAjuste($primera[0], "tbAjustes");  // carga el detalle
			}
		}, 50);
		CerrarWaiting();
		return true
	});
}

function InicializarEventosTabAjustes() {
	$(document).off("click", "#tbAjustes tbody tr");
	$(document).on("click", "#tbAjustes tbody tr", function (e) {

		if ($(e.target).is("button, a, .btn, i")) return;

		const $nuevaFila = $(this);
		ProcesarSeleccionFilaEnTabAjustes($nuevaFila);
	});
}

function ProcesarSeleccionFilaEnTabAjustes($fila) {
	$("#tbAjustes tbody tr").removeClass("selected-row");
	$fila.addClass("selected-row");
}

function ObtenerSucursalesSeleccionadasConTexto(sucList, suc) {

	let ids = [];
	let textos = [];

	// 1) Obtener sucursales seleccionadas en el ListBox
	$("#" + sucList + " option").each(function () {
		ids.push($(this).val());
		textos.push($(this).text());
	});

	// 2) Si NO hay ninguna seleccionada → devolver TODAS las del DropDownList
	if (ids.length === 0) {
		ids.push("%");
		textos.push("Todos");
	}

	return {
		ids: ids.join(","),
		textos: textos.join(", ")
	};
}

function SeleccionarAjuste(x, grid) {
	var $row = $(x);

	// Obtener valores desde los atributos data-*
	var as_compte = $row.data("as-compte");
	AbrirWaiting("Cargando datos del ajuste...");
	consultarDetalle(as_compte);
}

function consultarDetalle(as_compte) {
	PostGenHtml({ as_compte }, obtenerDetalleAjusteURL, function (obj) {
		$("#divDetalleAjuste").html(obj);
		InicializarEventosTabDetalleAjustes();
		CerrarWaiting();
		return true
	});
}

function InicializarEventosTabDetalleAjustes() {
	$(document).off("click", "#tbDetalle tbody tr");
	$(document).on("click", "#tbDetalle tbody tr", function (e) {

		if ($(e.target).is("button, a, .btn, i")) return;

		const $nuevaFila = $(this);
		ProcesarSeleccionFilaEnTabDetalleAjustes($nuevaFila);
	});
}

function ProcesarSeleccionFilaEnTabDetalleAjustes($fila) {
	$("#tbDetalle tbody tr").removeClass("selected-row");
	$fila.addClass("selected-row");
}

const TabToTableMapAjustes = {
	"navs-top-aju": "#tbAjustes",
	"navs-top-det": "#tbDetalle"
};

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

function ControlalistaSucursalesSelected() {
	var item = $("#listaSucursales").val();
	var desc = $("#listaSucursales option:selected").text();
	if ($("#SucursalesList").has('option:contains("' + item + '")').length === 0 && $("#SucursalesList").has('option:contains("' + desc + '")').length === 0) {
		var opc = "<option value=" + item + ">" + desc + "</option>"
		$("#SucursalesList").append(opc);
	}
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

	// 4) Validar diferencia máxima de 60 días
	let diffMs = dHasta - dDesde;
	let diffDias = diffMs / (1000 * 60 * 60 * 24);

	if (diffDias > 60) {
		return [false, "El rango de fechas no puede superar los 60 días."];
	}

	return [true, ""];
}

function InicializarCamposEnFiltros() {
	$("#btnImprimir").hide();
	$("#lbChkDesdeHasta").text("Fecha")
	$("#lbSucursales").text("Sucursales");

	$("#SucursalesList").empty();
	$("#listaSucursales").val("");

	$("#SucursalesList").prop("disabled", true);
	$("#listaSucursales").prop("disabled", true);

	$("#chkDesdeHasta").prop('checked', true);
	$("#chkDesdeHasta").trigger("change");
	$("#chkDesdeHasta").prop("disabled", true);

	$("#chkSucursales").prop('checked', false);
	$("#chkSucursales").trigger("change");

	$("#divFiltros").collapse("show");
	$("#divDetalle").collapse("hide");

	HandlerCheckBox();
}

function HandlerCheckBox() {
	$("#chkSucursales").on("click", function () {
		if ($("#chkSucursales").is(":checked")) {
			$("#listaSucursales").prop("disabled", false);
			$("#SucursalesList").prop("disabled", false);
			$("#listaSucursales").trigger("focus");
		}
		else {
			$("#listaSucursales").prop("disabled", true);
			$("#SucursalesList").prop("disabled", true);
			$("#listaSucursales").val("");
			$("#SucursalesList").empty();
		}
	});
}

function cargaPaginacion() {
	$("#divPaginacion").pagination({
		items: totalRegs,
		itemsOnPage: pagRegs,
		cssStyle: "dark-theme",
		currentPage: pagina,
		onPageClick: function (num) {
			CargarTablaTabAjustes(num);
		}
	});
	$("#pagEstado").val(false);
	$("#divFiltros").collapse("hide")
	return true;
}