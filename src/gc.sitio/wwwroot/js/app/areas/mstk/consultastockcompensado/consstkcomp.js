$(function () {
	$("#pagEstado").on("change", function () {
		var div = $("#divPaginacion");
		cargaPaginacion();
	});

	InicializarCamposEnFiltros(false);

	$(document).on("click", "#btnImprimir", ControlaImprimirSelected);
	$(document).on("click", "#btnCancel", ControlaCancelar);

	$(document).on("change", "#listaRubros", ControlalistaRubroSelected);

	$("#RubrosList").on("dblclick", 'option', function () { $(this).remove(); })

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

	$('#divDetalle').on('shown.bs.collapse', function () {
		toggleBtnCollapseSection();
	});

	$("#btnBuscar").on("click", function () {
		dataBak = "";
		pagina = 1;
		BuscarProductos(pagina);
	});

	funcCallBack = BuscarProductos;
});

function toggleBtnCollapseSection() {
	const contenido = $("#divGrillaProductos").html().trim();

	if (contenido === "" || contenido.length === 0) {
		// vacío → oculto el botón
		$("#btnCollapseSection").hide();
	} else {
		// con contenido → muestro el botón
		$("#btnCollapseSection").show();
	}
}


function ControlaImprimirSelected() {
	if (!validarTablaParaImpresion()) {
		AbrirMensaje("ATENCIÓN", "No hay datos generar el reporte.", function () {
			$("#msjModal").modal("hide");
			return true;
		}, false, ["Aceptar"], "error!", null);
	}
	else {
		ImprimirListaProductosStk_Generada();
	}
}

function validarTablaParaImpresion() {
	const tbody = document.querySelector("#tbGridProductos tbody");
	if (!tbody) return false;

	const filas = tbody.querySelectorAll("tr");
	if (filas.length === 0) return false;

	// Verifico si la única fila contiene el mensaje
	if (filas.length === 1) {
		const textoFila = filas[0].innerText.trim();
		if (textoFila.includes("No se encontraron productos con los criterios especificados")) {
			return false;
		}
	}

	return true;
}



function ReseteoDeReportes() {
	console.log("Reseto de reportes");
	ReporteResetArre();
}

function ImprimirListaProductosStk_Generada() {
	ReseteoDeReportes();
	setTimeout(() => {
		var lProvArr = [];
		var lRubArr = [];

		$("#Rel01List").children().each(function (i, item) { lProvArr.push($(item).val()) });
		$("#RubrosList").children().each(function (i, item) { lRubArr.push($(item).val()) });

		var lProv = lProvArr.join(",");
		var lRub = lRubArr.join(",");

		var chkEstAct = $("#chkEstadoActivo")[0].checked
		var chkEstDisc = $("#chkEstadoDiscontinuo")[0].checked

		var diferencia = $("#txtDiferencia").val();

		var data = { lProv, lRub, chkEstAct, chkEstDisc, diferencia };
		cargarReporteEnArre(54, data, "REPORTE DE STOCK COMPENSADO", "", "");
		invocacionGestorDoc({});
	}, 500);
}

function ControlaCancelar() {
	InicializarCamposEnFiltros(true);
}

function ControlalistaRubroSelected() {
}

function BuscarProductos(pag = 1) {
	AbrirWaiting();
	var lProv = [];
	var lRub = [];

	$("#Rel01List").children().each(function (i, item) { lProv.push($(item).val()) });
	$("#RubrosList").children().each(function (i, item) { lRub.push($(item).val()) });

	var chkEstAct = $("#chkEstadoActivo")[0].checked
	var chkEstDisc = $("#chkEstadoDiscontinuo")[0].checked

	var diferencia = $("#txtDiferencia").inputmask('unmaskedvalue');

	var buscaNew = true;
	pagina = pag;
	Pagina = pag;
	var sort = null;
	var sortDir = null
	var data2 = { sort, sortDir, Pagina, buscaNew }
	var data1 = { lProv, lRub, chkEstAct, chkEstDisc, diferencia };
	var data = $.extend({}, data1, data2);

	PostGenHtml(data, buscarStockProductosURL, function (obj) {
		$("#divGrillaProductos").html(obj);
		$("#divFiltros").collapse("hide");
		$("#divDetalle").collapse("show");
		$("#btnImprimir").show();
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
		AgregarHanlderColumnaDescripcion();
		CerrarWaiting();
		viendeDesdeBusquedaDeProducto = false;
		return true
	});
}

function selectListaProductoRow(x) {
	$("#tbGridProductos tbody tr").each(function (index) {
		$(this).removeClass("selected-row");
		$(this).removeClass("selectedEdit-row");
	});
	$(x).addClass("selected-row");
	const id = x.getAttribute("data-p-id");
	console.log("Producto ID:", id);
	if (id) {
		pIdSeleccionado = id;
		BuscarInfoAdicional();
	}
	else {
		pIdSeleccionado = "";
	}
}

function NoHayProdSeleccionado() {
	if (pIdSeleccionado == undefined || pIdSeleccionado == "") {
		return true;
	}
	return false;
}

function BuscarInfoAdicional() {
	if (NoHayProdSeleccionado()) {
		AbrirMensaje("Atención", "Debe seleccionar un producto.", function () {
			$("#msjModal").modal("hide");
			return true;
		}, false, ["Aceptar"], "error!", null);
	}
	AbrirWaiting();
	var pId = pIdSeleccionado;
	var admId = "";
	datos = { pId, admId };
	PostGenHtml(datos, BuscarInfoProdStkDepositoURL, function (obj) {
		$("#divInfoProductoStkD").html(obj);
		AddEventListenerToGrid("tbInfoProdStkD");
		CerrarWaiting();
		return true
	});
}

function InicializarCamposEnFiltros(vieneDeCancelar) {
	if (!vieneDeCancelar) {
		CargarRubros();
	}

	$("#btnImprimir").hide();
	$("#lbRel01").text("Proveedor");
	$("#lbRubro").text("Rubro");

	$("#chkRel01").prop('checked', false);
	$("#chkRel01").trigger("change");
	$("#chkRubro").prop('checked', false);
	$("#chkRubro").trigger("change");

	$("#Rel01List").empty();
	$("#Rel01List").prop("disabled", true);
	$("#Rel01").prop("disabled", true);

	$("#RubrosList").empty();
	$("#RubrosList").prop("disabled", true);
	$("#listaRubros").prop("disabled", true);

	$("#Rel01Item").val("");
	$("#listaRubros").val("");

	$("#divFiltros").collapse("show");
	$("#divDetalle").collapse("hide");
	if (!vieneDeCancelar) {
		HandlerCheckBox();
	}
	getMaskForIntegerMax99999("#txtDiferencia");
	$("#txtDiferencia").val("0");
	$("#divGrillaProductos").empty();
}

function HandlerCheckBox() {
	$("#chkRubro").on("click", function () {
		if ($("#chkRubro").is(":checked")) {
			$("#listaRubros").prop("disabled", false);
			$("#RubrosList").prop("disabled", false);
			$("#listaRubros").trigger("focus");
		}
		else {
			$("#listaRubros").prop("disabled", true);
			$("#RubrosList").prop("disabled", true);
			$("#listaRubros").val("");
			$("#RubrosList").empty();
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
			BuscarProductos(num);
		}
	});
	$("#pagEstado").val(false);
	$("#divFiltro").collapse("hide")
	return true;
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


function CargarRubros() {
	data = {};
	PostGenHtml(data, BuscarRubrosURL, function (obj) {
		$("#divRubros").html(obj);
		/*$("#divLs02").attr("class", "col-md-6 col-sm-6");*/
		$("#listaRubros").prop("disabled", true);
		CerrarWaiting();
		return true
	});
}

$("#Rel01List").on("dblclick", 'option', function () {
	$(this).remove();
})

function ControlalistaRubroSelected() {
	var item = $("#listaRubros").val();
	var desc = $("#listaRubros option:selected").text();
	if ($("#RubrosList").has('option:contains("' + item + '")').length === 0 && $("#RubrosList").has('option:contains("' + desc + '")').length === 0) {
		var opc = "<option value=" + item + ">" + desc + "</option>"
		$("#RubrosList").append(opc);
	}
}

$("#Rel01").on("click", function () { $(this).val(""); });

function getMaskForIntegerMax99999(selector) {
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
		max: 99999
	});
}

/* ######	INICIO Componente de info adicional de producto ###### */
const mostrarInfoProd = true;
const mostrarInfoProdStkA = true;
const mostrarInfoProdStkD = true;
const mostrarInfoProdStkBox = true;
const mostrarInfoProdStkMovM = true;
const mostrarInfoProdStkMovS = true;
const mostrarInfoProdStkMovD = true;
const mostrarInfoProdSustituto = true;
const pasarAdmLogueo = false;

function btnCollapseSectionValidar() {
	if (pIdSeleccionado != "") {
		var p_id = pIdSeleccionado;
		var data = {
			p_id,
			mostrarInfoProd,
			mostrarInfoProdStkA,
			mostrarInfoProdStkD,
			mostrarInfoProdStkBox,
			mostrarInfoProdStkMovM,
			mostrarInfoProdStkMovS,
			mostrarInfoProdStkMovD,
			mostrarInfoProdSustituto,
			pasarAdmLogueo
		};
		invocarComponenteDeInfoAdicionalDeProd(data);
	}
	else {
		$("#divInfoAdicionaDeProducto").html("").collapse("hide");
		AbrirMensaje("ATENCIÓN", "Debe seleccionar un producto.", function () {
			$("#msjModal").modal("hide");
			return true;
		}, false, ["Aceptar"], "error!", null);
	}
}

/* ######	FIN Componente de info adicional de producto ###### */

/* *************************************************************************************** */
///Hanlder para manejar la apertura de Info de Producto desde la columna Descripción
function AgregarHanlderColumnaDescripcion() {
	$(document)
		.off("click", "[data-action='info-producto']")
		.on("click", "[data-action='info-producto']", function (e) {

			e.stopPropagation();
			e.preventDefault();
			AbrirInfoProducto();
		});
}

function AbrirInfoProducto() {
	//e.preventDefault();

	if (pIdSeleccionado && pIdSeleccionado !== "") {
		$("#divInfoAdicionaDeProducto").collapse("toggle");

		setTimeout(() => {
			invocarComponenteDeInfoAdicionalDeProd({
				p_id: pIdSeleccionado,
				mostrarInfoProd,
				mostrarInfoProdStkA,
				mostrarInfoProdStkD,
				mostrarInfoProdStkBox,
				mostrarInfoProdStkMovM,
				mostrarInfoProdStkMovD,
				mostrarInfoProdStkMovS,
				mostrarInfoProdSustituto,
				pasarAdmLogueo,
			});
		}, 500);
	} else {
		AbrirMensaje("ATENCIÓN", "Debe seleccionar un producto.", function () {
			$("#msjModal").modal("hide");
			return true;
		}, false, ["Aceptar"], "error!", null);
	}
}
///////////////////////////////////////////////////////////////////////////////////////////////////