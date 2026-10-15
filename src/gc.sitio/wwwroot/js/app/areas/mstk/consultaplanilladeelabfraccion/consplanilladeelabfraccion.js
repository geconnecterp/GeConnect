let epCompteSeleccionado = "";

$(function () {
	$("#pagEstado").on("change", function () {
		var div = $("#divPaginacion");
		cargaPaginacion();
	});

	InicializarCamposEnFiltros();

	$(document).on("change", "#listaSucursales", ControlalistaSucursalesSelected);

	$("#btnImprimir").off("click");
	$("#btnImprimir").on("click", controlaImprimirPlanilla);

	$("#SucursalesList").on("dblclick", 'option', function () { $(this).remove(); })

	$("#btnFiltro").on("click", function () {
		if ($("#divFiltros").hasClass("show")) {
			$("#divFiltros").collapse("hide");
			$("#divDetalle").collapse("show");
			$("#btnImprimir").show();
		}
		else {
			$("#divFiltros").collapse("show");
			$("#divDetalle").collapse("hide");
			$("#btnImprimir").hide();
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
			CargarTablaTabPlanilla(1);
		}, 100);
		return true
	});
}

function CargarTablaTabPlanilla(pag = 1) {

	AbrirWaiting("Cargando planilla...");

	var buscaNew = true;
	pagina = pag;
	Pagina = pag;
	var sort = null;
	var sortDir = null
	var data2 = { sort, sortDir, Pagina, buscaNew }
	var data1 = { sucIdsList, f_desde, f_hasta };
	var data = $.extend({}, data1, data2);

	PostGenHtml(data, cargarPlanillaElabURL, function (obj) {
		$("#divPlanilla").html(obj);
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
		EvaluarBotonImprimir("navs-top-pla");
		InicializarEventosTabPlanilla();
		// 🔥 Seleccionar automáticamente la primera fila y cargar el detalle
		setTimeout(() => {
			const $primera = $("#tbPlanilla tbody tr.row-planilla").first();
			if ($primera.length) {
				ProcesarSeleccionFilaEnTabPlanilla($primera); // marca visualmente
				SeleccionarPlanilla($primera[0], "tbPlanilla");  // carga el detalle
			}
		}, 500);
		CerrarWaiting();
		return true
	});
}

function SeleccionarPlanilla(x, grid) {
	var $row = $(x);

	// Obtener valores desde los atributos data-*
	var ep_compte = $row.data("ep-compte");
	AbrirWaiting("Cargando datos de la planilla...");
	consultarDetalle(ep_compte);
	epCompteSeleccionado = ep_compte;
}

function consultarDetalle(ep_compte) {
	PostGenHtml({ ep_compte }, obtenerDetallePlanillaURL, function (obj) {
		$("#divDetallePlanilla").html(obj);
		InicializarEventosTabDetallePlanilla();
		CerrarWaiting();
		return true
	});
}

function InicializarEventosTabDetallePlanilla() {
	$(document).off("click", "#tbDetalleElab tbody tr");
	$(document).on("click", "#tbDetalleElab tbody tr", function (e) {

		if ($(e.target).is("button, a, .btn, i")) return;

		const $nuevaFila = $(this);
		ProcesarSeleccionFilaEnTabDetalleElabPlanilla($nuevaFila);
	});

	$(document).off("click", "#tbDetalleMP tbody tr");
	$(document).on("click", "#tbDetalleMP tbody tr", function (e) {

		if ($(e.target).is("button, a, .btn, i")) return;

		const $nuevaFila = $(this);
		ProcesarSeleccionFilaEnTabDetalleMPPlanilla($nuevaFila);
	});
}

function ProcesarSeleccionFilaEnTabDetalleElabPlanilla($fila) {
	$("#tbDetalleElab tbody tr").removeClass("selected-row");
	$fila.addClass("selected-row");
}

function ProcesarSeleccionFilaEnTabDetalleMPPlanilla($fila) {
	$("#tbDetalleMP tbody tr").removeClass("selected-row");
	$fila.addClass("selected-row");
}

function InicializarEventosTabPlanilla() {
	$(document).off("click", "#tbPlanilla tbody tr");
	$(document).on("click", "#tbPlanilla tbody tr", function (e) {

		if ($(e.target).is("button, a, .btn, i")) return;

		const $nuevaFila = $(this);
		ProcesarSeleccionFilaEnTabPlanilla($nuevaFila);
	});
}

function ProcesarSeleccionFilaEnTabPlanilla($fila) {
	$("#tbPlanilla tbody tr").removeClass("selected-row");
	$fila.addClass("selected-row");
}

let sucIdsList = null;
let provIdsList = null;
let f_desde = null;
let f_hasta = null;

const TabToTableMapPlanillas = {
	"navs-top-pla": "#tbPlanilla",
	"navs-top-det": "#tbDetalle"
};

function EvaluarBotonImprimir(tabId) {
	console.log("Evaluando botón imprimir para tab:", tabId);
	const tablaSelector = TabToTableMapPlanillas[tabId];
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

function renderGroup(label, items) {
	if (!items || items.length === 0) return `<span class="badge bg-secondary">${label}: TODOS </span>`;
	if (items.length === 1) return `<span class="badge bg-secondary">${label}: ${items[0]}</span>`;
	const listItems = items.map(i => `<li class="dropdown-item text-wrap">${i}</li>`).join('');
	return `
			<div class="btn-group">
				<button type="button"
						class="badge-dropdown-filtros dropdown-toggle"
						data-bs-toggle="dropdown"
						aria-expanded="false"
						data-bs-boundary="viewport">
					${label}: ${items.length} seleccionados
				</button>
				<ul class="dropdown-menu p-0" style="min-width:200px;">
					${listItems}</ul>
				</div>`;
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

function controlaImprimirPlanilla() {
	if (epCompteSeleccionado == "") {
		AbrirMensaje("ATENCIÓN", "Debe seleccionar una Planilla.", function () {
			$("#msjModal").modal("hide");
			return true;
		}, false, ["Aceptar"], "error!", null);
	}
	else {
		ImprimirPlanilla_Generada(epCompteSeleccionado);
	}
}

function ReseteoDeReportes() {
	console.log("Reseto de reportes");
	ReporteResetArre();
}

function ImprimirPlanilla_Generada(epCompteSeleccionado) {
	ReseteoDeReportes();
	setTimeout(() => {
		let data = { pe_compte: epCompteSeleccionado };
		cargarReporteEnArre(103, data, "PLANILLA DE ELABORACIÓN Y FRACCIÓN", "", "");
		invocacionGestorDoc({});
	}, 500);
}

function ControlalistaSucursalesSelected() {
	var item = $("#listaSucursales").val();
	var desc = $("#listaSucursales option:selected").text();
	if ($("#SucursalesList").has('option:contains("' + item + '")').length === 0 && $("#SucursalesList").has('option:contains("' + desc + '")').length === 0) {
		var opc = "<option value=" + item + ">" + desc + "</option>"
		$("#SucursalesList").append(opc);
	}
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
			CargarTablaTabPlanilla(num);
		}
	});
	$("#pagEstado").val(false);
	$("#divFiltros").collapse("hide")
	return true;
}