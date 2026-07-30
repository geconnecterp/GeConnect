$(function () {
	$("#pagEstado").on("change", function () {
		var div = $("#divPaginacion");
		cargaPaginacion();
	});

	InicializarCamposEnFiltros(false);

	$(document).on("click", "#btnImprimir", ControlaImprimirSelected);
	$(document).on("click", "#btnCancel", ControlaCancelar);
	$(document).on("change", "#listaSucursales", ControlalistaSucursalesSelected);
	$(document).on("change", "#listaDepositos", ControlalistaDepositosSelected);
	$(document).on("change", "#listaRubros", ControlalistaRubroSelected);
	$(document).on("change", "#listaFamilia", ControlalistaFamiliaSelected);

	$("#SucursalesList").on("dblclick", 'option', function () { $(this).remove(); })
	$("#DepositosList").on("dblclick", 'option', function () { $(this).remove(); })
	$("#FamiliaList").on("dblclick", 'option', function () { $(this).remove(); })
	$("#RubrosList").on("dblclick", 'option', function () { $(this).remove(); })
	$("#Rel01List").on("dblclick", 'option', function () { $(this).remove(); })

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
		dataBak = "";
		pagina = 1;
		BuscarProductosValorizados(pagina);
	});

	funcCallBack = BuscarProductosValorizados;
});

function ControlaImprimirSelected() {
	if ($("#tbGridProductos > tbody > tr").length === 0) {
		AbrirMensaje("ATENCIÓN", "No hay datos generar el reporte.", function () {
			$("#msjModal").modal("hide");
			return true;
		}, false, ["Aceptar"], "error!", null);
	}
	else {
		ImprimirListaProductosStk_Generada();
	}
}
function ImprimirListaProductosStk_Generada() {
	ReseteoDeReportes();
	setTimeout(() => {
		var lSucArr = [];
		var lSucTextos = "";
		var lDepArr = [];
		var lDepTextos = "";
		var lProvArr = [];
		var lProvTextos = "";
		var lFamArr = [];
		var lFamTextos = "";
		var lRubArr = [];
		var lRubTextos = "";
		var temp = [];

		$("#SucursalesList").children().each(function (i, item) { lSucArr.push($(item).val()) });
		$("#SucursalesList").children().each(function (i, item) { temp.push($(item).text()) });
		lSucTextos = temp.join(", ");
		temp = [];
		$("#DepositosList").children().each(function (i, item) { lDepArr.push($(item).val()) });
		$("#DepositosList").children().each(function (i, item) { temp.push($(item).text()) });
		lDepTextos = temp.join(", ");
		temp = [];
		$("#Rel01List").children().each(function (i, item) { lProvArr.push($(item).val()) });
		$("#Rel01List").children().each(function (i, item) { temp.push($(item).text()) });
		lProvTextos = temp.join(", ");
		temp = [];
		$("#FamiliaList").children().each(function (i, item) { lFamArr.push($(item).val()) });
		$("#FamiliaList").children().each(function (i, item) { temp.push($(item).text()) });
		lFamTextos = temp.join(", ");
		temp = [];
		$("#RubrosList").children().each(function (i, item) { lRubArr.push($(item).val()) });
		$("#RubrosList").children().each(function (i, item) { temp.push($(item).text()) });
		lRubTextos = temp.join(", ");
		temp = [];

		var lSuc = lSucArr.join(",");
		var lDep = lDepArr.join(",");
		var lProv = lProvArr.join(",");
		var lFam = lFamArr.join(",");
		var lRub = lRubArr.join(",");

		var chkStkPos = $("#chkStockPositivo")[0].checked
		var chkStkCero = $("#chkStockCero")[0].checked
		var chkStkNeg = $("#chkStockNegativo")[0].checked
		var chkEstAct = $("#chkEstadoActivo")[0].checked
		var chkEstDisc = $("#chkEstadoDiscontinuo")[0].checked

		const chkStockTextos = obtenerTitulosSeleccionados([
			"chkStockPositivo",
			"chkStockCero",
			"chkStockNegativo"
		]);
		const chkEstadoTextos = obtenerTitulosSeleccionados([
			"chkEstadoActivo",
			"chkEstadoDiscontinuo"
		]);

		var chkCostoRepo = $("#chkCostoRepo")[0].checked

		const chkCostoRepoTextos = obtenerTitulosSeleccionados([
			"chkCostoRepo"
		]);

		var agrupador = $("#listaAgrupador").val();

		var data = {
			lSuc, lDep, lProv, lFam, lRub,
			chkStkPos, chkStkCero, chkStkNeg, chkEstAct, chkEstDisc, chkCostoRepo,
			agrupador,
			lSucTextos, lDepTextos, lProvTextos, lFamTextos, lRubTextos,
			chkStockTextos, chkEstadoTextos, chkCostoRepoTextos
		};
		cargarReporteEnArre(51, data, "REPORTE DE STOCK VALORIZADO", "", "");
		invocacionGestorDoc({});
	}, 500);
}

function obtenerTitulosSeleccionados(idsCheckbox) {
	const textos = [];

	idsCheckbox.forEach(id => {
		const chk = $("#" + id)[0];
		if (chk && chk.checked) {
			const label = $("label[for='" + id + "']").text().trim();
			textos.push(label);
		}
	});

	return textos.join(", ");
}

function ReseteoDeReportes() {
	console.log("Reseto de reportes");
	ReporteResetArre();
}

function ControlaCancelar() {
	InicializarCamposEnFiltros(true);
}

function cargaPaginacion() {
	$("#divPaginacion").pagination({
		items: totalRegs,
		itemsOnPage: pagRegs,
		cssStyle: "dark-theme",
		currentPage: pagina,
		onPageClick: function (num) {
			BuscarProductosValorizados(num);
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
		if ($("#Rel01List")[0].length === 1) {
			$("#chkFamilias").prop("disabled", false);
			CargarFamiliaLista(ui.item.id);
		}
		else {
			$("#chkFamilias").prop("disabled", true);
			$("#listaFamilia").prop("disabled", true).val("");
			$("#FamiliaList").prop("disabled", true).empty();
			$("#chkFamilias")[0].checked = false;
		}

		return true;
	}
});

function InicializarCamposEnFiltros(vieneDeCancelar) {
	if (!vieneDeCancelar) {
		CargarRubros();
	}
	$("#btnImprimir").hide();
	$("#lbSucursales").text("Sucursal");
	$("#lbDepositos").text("Depósitos");
	$("#lbRel01").text("Proveedor");
	$("#lbFamilias").text("Familia");
	$("#lbRubro").text("Rubro");
	$("#lbCostoRepo").text("Costo Reposición");

	$("#chkSucursales").prop('checked', false);
	$("#chkSucursales").trigger("change");
	$("#chkDepositos").prop('checked', false);
	$("#chkDepositos").trigger("change");
	$("#chkRel01").prop('checked', false);
	$("#chkRel01").trigger("change");
	$("#chkFamilias").prop('checked', false);
	$("#chkFamilias").trigger("change");
	$("#chkRubro").prop('checked', false);
	$("#chkRubro").trigger("change");
	$("#chkCostoRepo").prop('checked', false);
	$("#chkCostoRepo").trigger("change");

	$("#SucursalesList").empty();
	$("#DepositosList").empty();
	$("#Rel01List").empty();
	$("#FamiliaList").empty();
	$("#RubrosList").empty();

	$("#listaSucursales").val("");
	$("#listaDepositos").val("");
	$("#Rel01Item").val("");
	$("#listaFamilia").val("");
	$("#listaRubros").val("");

	$("#divFiltros").collapse("show");
	$("#divDetalle").collapse("hide");
	if (!vieneDeCancelar) {
		HandlerCheckBox();
	}
}

function BuscarProductosValorizados(pag = 1) {
	AbrirWaiting();
	var lSuc = [];
	var lSucTextos = "";
	var lDep = [];
	var lDepTextos = "";
	var lProv = [];
	var lProvTextos = "";
	var lFam = [];
	var lFamTextos = "";
	var lRub = [];
	var lRubTextos = "";
	var temp = [];

	$("#SucursalesList").children().each(function (i, item) { lSuc.push($(item).val()) });
	$("#SucursalesList").children().each(function (i, item) { temp.push($(item).text()) });
	lSucTextos = temp.join(", ");
	temp = [];
	$("#DepositosList").children().each(function (i, item) { lDep.push($(item).val()) });
	$("#DepositosList").children().each(function (i, item) { temp.push($(item).text()) });
	lDepTextos = temp.join(", ");
	temp = [];
	$("#Rel01List").children().each(function (i, item) { lProv.push($(item).val()) });
	$("#Rel01List").children().each(function (i, item) { temp.push($(item).text()) });
	lProvTextos = temp.join(", ");
	temp = [];
	$("#FamiliaList").children().each(function (i, item) { lFam.push($(item).val()) });
	$("#FamiliaList").children().each(function (i, item) { temp.push($(item).text()) });
	lFamTextos = temp.join(", ");
	temp = [];
	$("#RubrosList").children().each(function (i, item) { lRub.push($(item).val()) });
	$("#RubrosList").children().each(function (i, item) { temp.push($(item).text()) });
	lRubTextos = temp.join(", ");
	temp = [];

	var chkStkPos = $("#chkStockPositivo")[0].checked
	var chkStkCero = $("#chkStockCero")[0].checked
	var chkStkNeg = $("#chkStockNegativo")[0].checked
	var chkEstAct = $("#chkEstadoActivo")[0].checked
	var chkEstDisc = $("#chkEstadoDiscontinuo")[0].checked

	var chkCostoRepo = $("#chkCostoRepo")[0].checked

	var agrupador = $("#listaAgrupador").val();

	const chkStockTextos = obtenerTitulosSeleccionados([
		"chkStockPositivo",
		"chkStockCero",
		"chkStockNegativo"
	]);
	const chkEstadoTextos = obtenerTitulosSeleccionados([
		"chkEstadoActivo",
		"chkEstadoDiscontinuo"
	]);
	const chkCostoRepoTextos = obtenerTitulosSeleccionados([
		"chkCostoRepo"
	]);

	var buscaNew = true;
	pagina = pag;
	Pagina = pag;
	var sort = null;
	var sortDir = null
	var data2 = { sort, sortDir, Pagina, buscaNew }
	var data1 = {
		lSuc, lDep, lProv, lFam, lRub,
		chkStkPos, chkStkCero, chkStkNeg, chkEstAct, chkEstDisc, chkCostoRepo,
		agrupador,
		lSucTextos, lDepTextos, lProvTextos, lFamTextos, lRubTextos,
		chkStockTextos, chkEstadoTextos, chkCostoRepoTextos
	};
	var data = $.extend({}, data1, data2);

	PostGenHtml(data, buscarStockProductosValorizadosURL, function (obj) {
		$("#divGrillaProductosValorizados").html(obj);
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
		CerrarWaiting();
		viendeDesdeBusquedaDeProducto = false;
		return true
	});
}

function HandlerCheckBox() {
	$("#chkSucursales").on("click", function () {
		if ($("#chkSucursales").is(":checked")) {
			$("#listaSucursales").prop("disabled", false);
			$("#SucursalesList").prop("disabled", false);
			$("#listaSucursales").trigger("focus");

			$("#listaDepositos").prop("disabled", true);
			$("#DepositosList").prop("disabled", true);
			$("#chkDepositos").prop('checked', false);
			$("#chkDepositos").trigger("change");
			$("#listaDepositos").val("");
			$("#DepositosList").empty();
		}
		else {
			$("#listaSucursales").prop("disabled", true);
			$("#SucursalesList").prop("disabled", true);
			$("#listaSucursales").val("");
			$("#SucursalesList").empty();
		}
	});
	$("#chkDepositos").on("click", function () {
		if ($("#chkDepositos").is(":checked")) {
			$("#listaDepositos").prop("disabled", false);
			$("#DepositosList").prop("disabled", false);
			$("#listaDepositos").trigger("focus");

			$("#listaSucursales").prop("disabled", true);
			$("#SucursalesList").prop("disabled", true);
			$("#chkSucursales").prop('checked', false);
			$("#chkSucursales").trigger("change");
			$("#listaSucursales").val("");
			$("#SucursalesList").empty();
		}
		else {
			$("#listaDepositos").prop("disabled", true);
			$("#DepositosList").prop("disabled", true);
			$("#listaDepositos").val("");
			$("#DepositosList").empty();
		}
	});
	$("#chkFamilias").on("click", function () {
		if ($("#chkFamilias").is(":checked")) {
			$("#listaFamilia").prop("disabled", false);
			$("#FamiliaList").prop("disabled", false);
			$("#listaFamilia").trigger("focus");
		}
		else {
			$("#listaFamilia").prop("disabled", true);
			$("#FamiliaList").prop("disabled", true);
			$("#listaFamilia").val("");
			$("#FamiliaList").empty();
		}
	});
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

function CargarFamiliaLista(id) {
	var ctaId = id;
	data = { ctaId };
	PostGenHtml(data, BuscarProveedoresFamiliaURL, function (obj) {
		$("#divListaFamilias").html(obj);
		CerrarWaiting();
		return true
	});
}

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

function ControlalistaSucursalesSelected() {
	var item = $("#listaSucursales").val();
	var desc = $("#listaSucursales option:selected").text();
	if ($("#SucursalesList").has('option:contains("' + item + '")').length === 0 && $("#SucursalesList").has('option:contains("' + desc + '")').length === 0) {
		var opc = "<option value=" + item + ">" + desc + "</option>"
		$("#SucursalesList").append(opc);
	}
}

function ControlalistaDepositosSelected() {
	var item = $("#listaDepositos").val();
	var desc = $("#listaDepositos option:selected").text();
	if ($("#DepositosList").has('option:contains("' + item + '")').length === 0 && $("#DepositosList").has('option:contains("' + desc + '")').length === 0) {
		var opc = "<option value=" + item + ">" + desc + "</option>"
		$("#DepositosList").append(opc);
	}
}

function ControlalistaFamiliaSelected() {
	var item = $("#listaFamilia").val();
	var desc = $("#listaFamilia option:selected").text();
	if ($("#FamiliaList").has('option:contains("' + item + '")').length === 0 && $("#FamiliaList").has('option:contains("' + desc + '")').length === 0) {
		var opc = "<option value=" + item + ">" + desc + "</option>"
		$("#FamiliaList").append(opc);
	}
}

function ControlalistaRubroSelected() {
	var item = $("#listaRubros").val();
	var desc = $("#listaRubros option:selected").text();
	if ($("#RubrosList").has('option:contains("' + item + '")').length === 0 && $("#RubrosList").has('option:contains("' + desc + '")').length === 0) {
		var opc = "<option value=" + item + ">" + desc + "</option>"
		$("#RubrosList").append(opc);
	}
}

$("#Rel01").on("click", function () { $(this).val(""); });