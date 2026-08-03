let _pedidoLoading = false;
let tabsDetallePendientes = 0;
const TabToTableMap = {
	"navs-top-det": "#tbDetalleDeSaldos",
	"navs-top-res": "#tbResumenDeSaldos",
};

$(function () {
	InicializarCamposEnFiltros(false);
	InicializaEventos();

});

function MostrarFiltrosAplicados() {
	try {
		const cont = $("#filtrosAplicadosFloating");
		const target = cont.length ? cont : null;
		if (!target) return;

		const vendedores = listFrom("VendedoresList");

		let html = '<div class="d-inline-flex align-items-center" style="gap:8px;white-space:nowrap;">';

		html += renderGroup('VEND', vendedores);
		html += '</div>';

		target.html(html);
	} catch (e) {
		console.error('MostrarFiltrosAplicados error', e);
	}
}

// Ejecutar al cargar
$(function () { try { MostrarFiltrosAplicados(); } catch (e) { } });

function InicializaEventos() {
	$(document).off("dblclick", "VendedoresList");
	$("#VendedoresList").on("dblclick", 'option', function () { $(this).remove(); })
	$(document).off("change", "listaVendedores");
	$(document).on("change", "#listaVendedores", ControlalistaVendedoresSelected);
	//$("#btnImprimir").prop("disabled", true);

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
		try { MostrarFiltrosAplicados(); } catch (e) { console.warn('MostrarFiltrosAplicados no disponible:', e); }
		InicializarPantallaPrincipal();
	});
}

function InicializarPantallaPrincipal() {
	var vend = ObtenerVendedoresSeleccionadasConTexto();
	var vendedoresText = vend.textos;
	var vendedoresIds = vend.ids;
	AbrirWaiting("Cargando información...");
	PostGenHtml({ vendedoresText, vendedoresIds }, inicializarPantallPrincipalURL, function (obj) {
			$("#divDetalle").html(obj);
			// actualizar filtros aplicados despues de renderizar
			try { MostrarFiltrosAplicados(); } catch (e) { console.warn('MostrarFiltrosAplicados no disponible:', e); }
		$(document).on('shown.bs.tab', 'button[data-bs-toggle="tab"]', function (e) {
			const tabId = $(e.target).attr("data-bs-target").replace("#", "");
			EvaluarBotonImprimir(tabId);
		});
		tabsDetallePendientes = 2;
		$("#divFiltros").collapse("hide");
		$("#divDetalle").collapse("show");
		
		setTimeout(() => {
			CargarSeccionDetalleDeSaldos();
			CargarSeccionResumenDeSaldos();
		}, 200);
		return true
	});
}

function CargarSeccionDetalleDeSaldos() {
	var vend = ObtenerVendedoresSeleccionadasConTexto();
	var ve_list = vend.ids;
	AbrirWaiting("Cargando información...");
	PostGenHtml({ ve_list }, buscarDetalleDeSaldosURL, function (obj) {
		$("#divDetalleDeSaldos").html(obj);
		InicializarEventosDetalleDeSaldos();
		EvaluarBotonImprimir("navs-top-det");
		FinalizarCargaDetalle();
		CerrarWaiting();
		return true
	});
}

function CargarSeccionResumenDeSaldos() {
	var vend = ObtenerVendedoresSeleccionadasConTexto();
	var ve_list = vend.ids;
	AbrirWaiting("Cargando información...");
	PostGenHtml({ ve_list }, buscarResumenDeSaldosURL, function (obj) {
		$("#divResumenDeSaldos").html(obj);
		InicializarEventosResumenDeSaldos();
		FinalizarCargaDetalle();
		CerrarWaiting();
		return true
	});
}

function InicializarEventosDetalleDeSaldos() {
	$(document).off("click", "#tbDetalleDeSaldos tbody tr");
	$(document).on("click", "#tbDetalleDeSaldos tbody tr", function (e) {

		if ($(e.target).is("button, a, .btn, i")) return;

		const $nuevaFila = $(this);
		ProcesarSeleccionFilaEnDetalleDeSaldos($nuevaFila);
	});
}

function InicializarEventosResumenDeSaldos() {
	$(document).off("click", "#tbResumenDeSaldos tbody tr");
	$(document).on("click", "#tbResumenDeSaldos tbody tr", function (e) {

		if ($(e.target).is("button, a, .btn, i")) return;

		const $nuevaFila = $(this);
		ProcesarSeleccionFilaEnResumenDeSaldos($nuevaFila);
	});
}

function ProcesarSeleccionFilaEnDetalleDeSaldos($fila) {
	// Quitar selección previa
	$("#tbDetalleDeSaldos tbody tr").removeClass("selected-row");
	// Marcar fila seleccionada
	$fila.addClass("selected-row");
}

function ProcesarSeleccionFilaEnResumenDeSaldos($fila) {
	// Quitar selección previa
	$("#tbResumenDeSaldos tbody tr").removeClass("selected-row");
	// Marcar fila seleccionada
	$fila.addClass("selected-row");
}

function FinalizarCargaDetalle() {
	tabsDetallePendientes--;

	if (tabsDetallePendientes <= 0) {
		CerrarWaiting();
	}
}

function ReseteoDeReportes() {
	console.log("Reseto de reportes");
	ReporteResetArre();
}

function ImprimirSegunTab(tabId) {

	switch (tabId) {
		case "navs-top-det":
			ImprimirDetalle();
			break;

		case "navs-top-res":
			ImprimirResumen();
			break;

	}
}

function ImprimirDetalle() {
	AbrirWaiting();
	var tipoReporte = 1;
	var data = { tipoReporte };
	PostGen(data, setearTipoDeReporteUrl, function (obj) {
		CerrarWaiting();
		if (obj.error === true) {
			CerrarWaiting();
			AbrirMensaje("ATENCIÓN", obj.msg, function () {
				$("#msjModal").modal("hide");
				return true;
			}, false, ["Aceptar"], "error!", null);
		}
		else {
			HandlerImprimirDetalle();
		}
	});
}

function ImprimirResumen() {
	AbrirWaiting();
	var tipoReporte = 2;
	var data = { tipoReporte };
	PostGen(data, setearTipoDeReporteUrl, function (obj) {
		CerrarWaiting();
		if (obj.error === true) {
			CerrarWaiting();
			AbrirMensaje("ATENCIÓN", obj.msg, function () {
				$("#msjModal").modal("hide");
				return true;
			}, false, ["Aceptar"], "error!", null);
		}
		else {
			HandlerImprimirResumen();
		}
	});
}

function HandlerImprimirDetalle() {
	ReseteoDeReportes();
	setTimeout(() => {
		var vend = ObtenerVendedoresSeleccionadasConTexto();
		var vendedoresIds = vend.ids;
		var vendedoresTextos = vend.textos;
		var data = {
			Vendedores: vendedoresIds,
			VendedoresTextos: vendedoresTextos
		}
		cargarReporteEnArre(86, data, "Detalle de Saldo de Cuenta de Distribuidora", "", "");
		invocacionGestorDoc({});
	}, 500);
}

function HandlerImprimirResumen() {
	ReseteoDeReportes();
	setTimeout(() => {
		var vend = ObtenerVendedoresSeleccionadasConTexto();
		var vendedoresIds = vend.ids;
		var vendedoresTextos = vend.textos;
		var data = {
			Vendedores: vendedoresIds,
			VendedoresTextos: vendedoresTextos
		}
		cargarReporteEnArre(87, data, "Resumen de Saldo de Cuenta de Distribuidora", "", "");
		invocacionGestorDoc({});
	}, 500);
}

$("#btnImprimir").on("click", function () {
	const tabId = $(this).data("tab-activo");
	ImprimirSegunTab(tabId);
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

function InicializarCamposEnFiltros(vieneDeCancelar) {
	if (!vieneDeCancelar) {
	}

	$("#chkVendedores").prop('checked', true);
	$("#chkVendedores").trigger("change");
	$("#chkVendedores").prop("disabled", true);

	$("#btnImprimir").hide();
	$("#lbVendedores").text("Vendedor");
	$("#listaVendedores").val("");

	$("#divFiltros").collapse("show");
	$("#divDetalle").collapse("hide");

}

function ControlalistaVendedoresSelected() {
	var item = $("#listaVendedores").val();
	var desc = $("#listaVendedores option:selected").text();
	if ($("#VendedoresList").has('option:contains("' + item + '")').length === 0 && $("#VendedoresList").has('option:contains("' + desc + '")').length === 0) {
		var opc = "<option value=" + item + ">" + desc + "</option>"
		$("#VendedoresList").append(opc);
	}
}

function ObtenerVendedoresSeleccionadasConTexto() {

	let ids = [];
	let textos = [];

	// 1) Obtener vendedores seleccionados en el ListBox
	$("#VendedoresList option").each(function () {
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