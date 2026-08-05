const TabToTableMap = {
	"navs-top-devs": "#tbDevoluciones",
	"navs-top-det": "#tbDetalle"
};

$(function () {
	$("#pagEstado").on("change", function () {
		var div = $("#divPaginacion");
		cargaPaginacion();
	});

	InicializarCamposEnFiltros();

	$(document).on("change", "#listaSucursales", ControlalistaSucursalesSelected);

	$("#SucursalesList").on("dblclick", 'option', function () { $(this).remove(); })
	$("#Rel01List").on("dblclick", 'option', function () { $(this).remove(); })

	$("input#Rel01").on("click", function () {
		$("input#Rel01").val("");
		$("#Rel01Item").val("");
	});

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
			// actualizar vista de filtros antes de buscar
			try { MostrarFiltrosAplicados(); } catch (e) { console.warn('MostrarFiltrosAplicados no disponible:', e); }
			InicializarPantallaPrincipal();
		}
	});
});

function MostrarFiltrosAplicados() {
	try {
		// intentar usar el contenedor flotante; si no existe, no hacemos nada (el partial puede contener su propio container)
		const floatCont = $("#filtrosAplicadosFloating");
		const fallback = $("#filtrosAplicadosContainer");
		const cont = floatCont.length ? floatCont : (fallback.length ? fallback : null);
		if (!cont) return;

		const desde = $("#Desde").val();
		const hasta = $("#Hasta").val();

		const suc = listFrom("SucursalesList");
		const prov = listFrom("Rel01List");

		let html = '<div class="d-inline-flex align-items-center" style="gap:8px;white-space:nowrap;">';
		if (desde) html += `<span class="badge bg-secondary">Desde: ${desde}</span>`;
		if (hasta) html += `<span class="badge bg-secondary">Hasta: ${hasta}</span>`;

		html += renderGroup('SUC', suc);
		html += renderGroup('PROV', prov);
		html += '</div>';

		cont.html(html);
	} catch (e) {
		console.error('MostrarFiltrosAplicados error', e);
	}
}

// intentar mostrar al cargar
try { MostrarFiltrosAplicados(); } catch (e) { }

function cargaPaginacion() {
	$("#divPaginacion").pagination({
		items: totalRegs,
		itemsOnPage: pagRegs,
		cssStyle: "dark-theme",
		currentPage: pagina,
		onPageClick: function (num) {
			CargarTablaTabDevoluciones(num);
		}
	});
	$("#pagEstado").val(false);
	$("#divFiltros").collapse("hide")
	return true;
}

function InicializarPantallaPrincipal() {
	var suc = ObtenerSucursalesSeleccionadasConTexto("SucursalesList", "listaSucursales");
	var prov = ObtenerProveedoresSeleccionadasConTexto();

	var sucursalesText = suc.textos;
	var provText = prov.textos;
	sucIdsList = suc.ids;
	provIdsList = prov.ids;
	f_desde = $("#Desde").val();
	f_hasta = $("#Hasta").val();
	AbrirWaiting("Cargando información...");
	PostGenHtml({ sucursalesText, provText, f_desde, f_hasta }, inicializarPantallPrincipalURL, function (obj) {
		$("#divDetalle").html(obj);
		// actualizar filtros aplicados después de renderizar (fallback si partial reemplaza el DOM)
		try { MostrarFiltrosAplicados(); } catch (e) { console.warn('MostrarFiltrosAplicados no disponible:', e); }
		$(document).on('shown.bs.tab', 'button[data-bs-toggle="tab"]', function (e) {
			const tabId = $(e.target).attr("data-bs-target").replace("#", "");
			EvaluarBotonImprimir(tabId);
		});
		$("#divFiltros").collapse("hide");
		$("#divDetalle").collapse("show");
		CerrarWaiting();
		setTimeout(() => {
			CargarTablaTabDevoluciones(1);
		}, 100);
		return true
	});
}

let sucIdsList = null;
let provIdsList = null;
let f_desde = null;
let f_hasta = null;

function CargarTablaTabDevoluciones(pag=1) {

	AbrirWaiting("Cargando devoluciones...");

	var buscaNew = true;
	pagina = pag;
	Pagina = pag;
	var sort = null;
	var sortDir = null
	var data2 = { sort, sortDir, Pagina, buscaNew }
	var data1 = { sucIdsList, provIdsList, f_desde, f_hasta };
	var data = $.extend({}, data1, data2);

	PostGenHtml(data, cargarDevolucionesURL, function (obj) {
		$("#divDevoluciones").html(obj);
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
		InicializarEventosTabDevoluciones();
		// 🔥 Seleccionar automáticamente la primera fila y cargar el detalle
		setTimeout(() => {
			const $primera = $("#tbDevoluciones tbody tr.row-devolucion").first();
			if ($primera.length) {
				ProcesarSeleccionFilaEnTabDevoluciones($primera); // marca visualmente
				SeleccionarDevolucion($primera[0], "tbDevoluciones"); // carga el detalle
			}
		}, 50);
		CerrarWaiting();
		return true
	});
}

function InicializarEventosTabDevoluciones() {
	$(document).off("click", "#tbDevoluciones tbody tr");
	$(document).on("click", "#tbDevoluciones tbody tr", function (e) {

		if ($(e.target).is("button, a, .btn, i")) return;

		const $nuevaFila = $(this);
		ProcesarSeleccionFilaEnTabDevoluciones($nuevaFila);
	});
}

function ProcesarSeleccionFilaEnTabDevoluciones($fila) {
	$("#tbDevoluciones tbody tr").removeClass("selected-row");
	$fila.addClass("selected-row");
}

function SeleccionarDevolucion(x, grid) {
	var $row = $(x);

	// Obtener valores desde los atributos data-*
	var ti = $row.data("ti");
	var pv_compte = $row.data("pv-compte");
	var dv_compte = $row.data("dv-compte");
	var cm_compte = $row.data("cm-compte");
	AbrirWaiting("Cargando datos...");
	consultarDetalle(dv_compte);
}

function consultarDetalle(dv_compte) {
	PostGenHtml({ dv_compte }, obtenerDetalleDevolucionURL, function (obj) {
		$("#divDetalleDevolucion").html(obj);
		InicializarEventosTabDetalleDevolucion();
		CerrarWaiting();
		return true
	});
}

function InicializarEventosTabDetalleDevolucion() {
	$(document).off("click", "#tbDetalle tbody tr");
	$(document).on("click", "#tbDetalle tbody tr", function (e) {

		if ($(e.target).is("button, a, .btn, i")) return;

		const $nuevaFila = $(this);
		ProcesarSeleccionFilaEnTabDetalleDevolucion($nuevaFila);
	});
}

function ProcesarSeleccionFilaEnTabDetalleDevolucion($fila) {
	$("#tbDetalle tbody tr").removeClass("selected-row");
	$fila.addClass("selected-row");
}

function ObtenerProveedoresSeleccionadasConTexto() {

	let ids = [];
	let textos = [];

	// 1) Obtener sucursales seleccionadas en el ListBox
	$("#Rel01List option").each(function () {
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
		//$("#" + suc + " option").each(function () {
		//	const val = $(this).val();
		//	const txt = $(this).text();

		//	if (val && val !== "") {
		//		ids.push(val);
		//		textos.push(txt);
		//	}
		//});
	}

	return {
		ids: ids.join(","),
		textos: textos.join(", ")
	};
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
	$("#lbRel01").text("Proveedores");

	$("#SucursalesList").empty();
	$("#Rel01List").empty();

	$("#listaSucursales").val("");
	$("#Rel01Item").val("");

	$("#SucursalesList").prop("disabled", true);
	$("#listaSucursales").prop("disabled", true);

	$("#chkDesdeHasta").prop('checked', true);
	$("#chkDesdeHasta").trigger("change");
	$("#chkDesdeHasta").prop("disabled", true);

	$("#chkSucursales").prop('checked', false);
	$("#chkSucursales").trigger("change");
	$("#chkRel01").prop('checked', false);
	$("#chkRel01").trigger("change");

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


$("#Rel01").autocomplete({
	source: function (request, response) {

		data = { prefix: request.term }; Rel01

		$.ajax({
			url: autoComRel01Url,
			type: "POST",
			dataType: "json",
			data: data,
			success: function (obj) {
				response($.map(obj, function (item) {
					var texto = item.descripcion;
					return { label: texto, value: item.descripcion, id: item.id, prov: item.provId };
				}));
			}
		})
	},
	minLength: 3,
	select: function (event, ui) {
		if ($("#Rel01List").has('option:contains("' + ui.item.id + '")').length === 0) {
			$("#Rel01Item").val(ui.item.id);
			var opc = "<option value=" + ui.item.id + ">" + ui.item.value + "</option>"
			$("#Rel01List").append(opc);
		}
		return true;
	}
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