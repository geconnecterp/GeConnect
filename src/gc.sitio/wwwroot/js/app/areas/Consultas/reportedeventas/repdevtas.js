let _pedidoLoading = false;

const TabToTableMap = {
	"navs-top-proc": "#btnTabProcesosDeCaja",
	"navs-top-rend": "#btnTabRendicionCierre",
	"navs-top-ana": "#btnTabAnaliticoOperacion"
};

$(function () {
	InicializarCamposEnFiltros(false);
	InicializaEventos();
	
});

function InicializaEventos() {
	$(document).on("change", "#listaSucursales", ControlalistaSucursalesSelected);
	funcCallBack = CargarSeccionProcesoDeCajas;
	$("#btnImprimir").prop("disabled", true);

	$("#pagEstado").on("change", function () {
		var div = $("#divPaginacion");
		presentaPaginacion(div);
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
		if (validarFechasAnalisis()) {
			var sucursalesIds = ObtenerSucursalesSeleccionadas();
			if (!sucursalesIds || sucursalesIds.length == 0) {
				AbrirMensaje("ATENCIÓN", "Debe al menos seleccionar una sucursal.", function () {
					$("#msjModal").modal("hide");
					return true;
				}, false, ["Aceptar"], "error!", null);
			} else {
				InicializarPantallaPrincipal();
			}
		} else {
			AbrirMensaje("ATENCIÓN", "Problemas con las fechas, por favor verifique.", function () {
				$("#msjModal").modal("hide");
				return true;
			}, false, ["Aceptar"], "error!", null);
		}
	});
}

function InicializarPantallaPrincipal() {
	var suc = ObtenerSucursalesSeleccionadasConTexto();
	var sucursalesText = suc.textos;
	var desde = $("#Desde").val();
	var hasta = $("#Hasta").val();
	AbrirWaiting("Cargando información...");
	PostGenHtml({ sucursalesText, desde, hasta }, inicializarPantallPrincipalURL, function (obj) {
		$("#divDetalle").html(obj);
		$(document).on('shown.bs.tab', 'button[data-bs-toggle="tab"]', function (e) {
			const tabId = $(e.target).attr("data-bs-target").replace("#", "");
			EvaluarBotonImprimir(tabId);
		});
		$("#divFiltros").collapse("hide");
		$("#divDetalle").collapse("show");
		CerrarWaiting();
		setTimeout(() => {
			CargarSeccionProcesoDeCajas(1);
		}, 200);
		return true
	});
}

function CargarSeccionProcesoDeCajas(pag = 1) {
	if (_pedidoLoading) return;
	_pedidoLoading = true;
	pagina = pag;
	try {
		AbrirWaiting("Buscando Procesos...")
		const filtros = buildQueryFilters(pag);
		const url = buscarProcesosDeCaja2URL;
		PostGenHtml(filtros, url, function (html) {
			$("#divProcesosDeCaja").html(html);
			InicializarEventosProcesosDeCaja();
			setTimeout(function () {
				
			}, 1000);

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
		CerrarWaiting();
		console.error("Error al buscar sorteos:", e);
		$("#divDetalle").html('<div class="alert alert-danger py-2 mb-0">No se pudo obtener la información.</div>').collapse("show");
	} finally {
		_pedidoLoading = false;
	}
}

function InicializarEventosProcesosDeCaja() {
	console.log("Inicializando eventos de Procesos de Caja...");

	// Nos aseguramos que el contenedor exista
	const $contenedor = $("#divProcDeCajaProcesos");
	if ($contenedor.length === 0) {
		console.warn("No se encontró #divProcDeCajaProcesos en el DOM.");
		return;
	}

	// Quitamos cualquier handler previo
	$contenedor.off("click", "#tbGridProcesos tbody tr");

	// Delegamos el click desde el contenedor fijo
	$contenedor.on("click", "#tbGridProcesos tbody tr", function (e) {
		console.log("Click en fila de procesos de caja");

		if ($(e.target).is("button, a, .btn, i")) {
			console.log("Click ignorado por ser botón/link/icono");
			return;
		}

		const $fila = $(this);
		ProcesarSeleccionFilaEnProcesosDeCaja($fila);
	});
}

function ProcesarSeleccionFilaEnProcesosDeCaja($fila) {
	$("#tbGridProcesos tbody tr").removeClass("selected-row");
	$fila.addClass("selected-row");
	console.log("Fila seleccionada:", $fila.data("caja-nro-proceso"), $fila.data("adm-id"));
}


function buildQueryFilters(pag) {
	var suc = ObtenerSucursalesSeleccionadasConTexto();
	var sucursalesIds = suc.ids;
	sucursales_ids_desde_filtros = sucursalesIds;
	var data = {
		Registros: 200,
		Pagina: pag,
		Desde: $("#Desde").val(),
		Hasta: $("#Hasta").val(),
		Sucursales: sucursalesIds,
	}
	return data;
}

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

function ObtenerSucursalesSeleccionadas() {

	// 1) Obtener sucursales seleccionadas en el ListBox
	let seleccionadas = [];
	$("#SucursalesList option").each(function () {
		seleccionadas.push($(this).val());
	});

	// 2) Si NO hay ninguna seleccionada → devolver TODAS las del DropDownList
	if (seleccionadas.length === 0) {
		$("#listaSucursales option").each(function () {
			const val = $(this).val();
			if (val && val !== "") {
				seleccionadas.push(val);
			}
		});
	}

	// 3) Devolver como string separado por comas
	return seleccionadas.join(",");
}


function validarFechasAnalisis() {
	const desdeInput = document.getElementById("Desde");
	const hastaInput = document.getElementById("Hasta");

	const desde = new Date(desdeInput.value);
	const hasta = new Date(hastaInput.value);

	// Si alguna fecha no está cargada, no validamos todavía
	if (isNaN(desde) || isNaN(hasta)) {
		return true;
	}

	if (desde > hasta) {
		return false;
	}

	return true;
}

function ControlalistaSucursalesSelected() {
	var item = $("#listaSucursales").val();
	var desc = $("#listaSucursales option:selected").text();
	if ($("#SucursalesList").has('option:contains("' + item + '")').length === 0 && $("#SucursalesList").has('option:contains("' + desc + '")').length === 0) {
		var opc = "<option value=" + item + ">" + desc + "</option>"
		$("#SucursalesList").append(opc);
	}
}

function InicializarCamposEnFiltros(vieneDeCancelar) {
	if (!vieneDeCancelar) {
	}

	$("#chkDesdeHasta").prop('checked', true);
	$("#chkDesdeHasta").trigger("change");
	$("#chkDesdeHasta").prop("disabled", true);

	$("#btnImprimir").hide();
	$("#lbChkDesdeHasta").text("Fecha");
	$("#lbSucursales").text("Sucursal");

	$("#chkSucursales").prop('checked', true);
	$("#chkSucursales").trigger("change");
	$("#chkSucursales").prop("disabled", true);

	$("#listaSucursales").val("");
	$("#listaEstados").val("");

	$("#divFiltros").collapse("show");
	$("#divDetalle").collapse("hide");

	let sucSele = $("#SucursalSeleccionada").val();
	$("#listaSucursales").val(sucSele);

	setTimeout(() => {
		let habilitado = $("#HabilitarCambioDeSucursalSeleccionada").val();
		if (habilitado == "False") {
			$("#divListaSucursales").find("input, select, textarea, button").prop("disabled", true);
			ControlalistaSucursalesSelected();
		}
		else
			$("#divListaSucursales").find("input, select, textarea, button").prop("disabled", false);
	}, 500);
}

function ObtenerSucursalesSeleccionadasConTexto() {

	let ids = [];
	let textos = [];

	// 1) Obtener sucursales seleccionadas en el ListBox
	$("#SucursalesList option").each(function () {
		ids.push($(this).val());
		textos.push($(this).text());
	});

	// 2) Si NO hay ninguna seleccionada → devolver TODAS las del DropDownList
	if (ids.length === 0) {
		$("#listaSucursales option").each(function () {
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
		textos: textos.join(", ")
	};
}