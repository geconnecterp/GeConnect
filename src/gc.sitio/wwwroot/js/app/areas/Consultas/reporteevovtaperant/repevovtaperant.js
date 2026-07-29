$(function () {

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
		// 1) Validación de rango de fechas
		if (!ValidarRangoFechas()) {
			return;
		}

		// 2) Validación de familias
		if ($("#chkFamilias").is(":checked")) {

			// Obtener elementos seleccionados en la ListBox
			const familias = $("#FamiliaList option");

			if (familias.length === 0) {
				AbrirMensaje("ATENCIÓN", "Debe seleccionar al menos una Familia.", function () {
					$("#msjModal").modal("hide");
					return true;
				}, false, ["Aceptar"], "error!", null);
				return;
			}
		}

		// 3) Ejecutar búsqueda
		BuscarEvoVtasPerAnt();
	});

});

function BuscarEvoVtasPerAnt() {
	AbrirWaiting();
	var lSuc = [];
	var lProv = [];
	var lFam = [];
	var lRub = [];
	var temp = ObtenerFiltroLista("#chkSucursales", "#SucursalesList");
	var lSuc = temp.ids;
	var lSucTextos = temp.textos;
	//$("#DepositosList").children().each(function (i, item) { lDep.push($(item).val()) });
	temp = ObtenerFiltroLista("#chkRel01", "#Rel01List");
	var lProv = temp.ids;
	var lProvTextos = temp.textos;
	temp = ObtenerFiltroLista("#chkFamilias", "#FamiliaList");
	lFam = temp.ids;
	var lFamTextos = temp.textos;
	temp = ObtenerFiltroLista("#chkRubro", "#RubrosList");
	var lRub = temp.ids;
	var lRubTextos = temp.textos;
	var desde = $("#Desde").val();
	var hasta = $("#Hasta").val();
	var agrupador = $("#listaAgrupador").val();

	var data = { lSuc, lProv, lFam, lRub, desde, hasta, agrupador, lSucTextos, lProvTextos, lFamTextos, lRubTextos };

	PostGenHtml(data, repEvoVtasPerAnterioresURL, function (obj) {
		$("#divGrillaRepEvoVtasPerAnteriores").html(obj);
		$("#divFiltros").collapse("hide");
		$("#divDetalle").collapse("show");
		$("#btnImprimir").show();
		CerrarWaiting();
		return true
	});
}

function ObtenerFiltroLista(idCheckbox, idListBox) {

	const estaChequeado = $(idCheckbox).is(":checked");
	const ids = [];
	const textos = [];

	// Tomar valores y textos del ListBox
	$(idListBox + " option").each(function () {
		ids.push($(this).val());
		textos.push($(this).text());
	});

	// Si está chequeado y no hay valores → devolver "%"
	if (estaChequeado && ids.length === 0) {
		return {
			ids: ["%"],
			textos: "Todos"
		};
	}

	return {
		ids: ids,
		textos: textos.join(", ")
	};
}


$("#Rel01").on("click", function () { $(this).val(""); });

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

function ControlalistaDepositosSelected() {
	var item = $("#listaDepositos").val();
	var desc = $("#listaDepositos option:selected").text();
	if ($("#DepositosList").has('option:contains("' + item + '")').length === 0 && $("#DepositosList").has('option:contains("' + desc + '")').length === 0) {
		var opc = "<option value=" + item + ">" + desc + "</option>"
		$("#DepositosList").append(opc);
	}
}

function ControlalistaSucursalesSelected() {
	var item = $("#listaSucursales").val();
	var desc = $("#listaSucursales option:selected").text();
	if ($("#SucursalesList").has('option:contains("' + item + '")').length === 0 && $("#SucursalesList").has('option:contains("' + desc + '")').length === 0) {
		var opc = "<option value=" + item + ">" + desc + "</option>"
		$("#SucursalesList").append(opc);
	}
}

function ControlaCancelar() {
	InicializarCamposEnFiltros(true);
}

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
		var lSuc = [];
		var lProv = [];
		var lFam = [];
		var lRub = [];
		var temp = ObtenerFiltroLista("#chkSucursales", "#SucursalesList");
		var lSuc = temp.ids;
		temp = ObtenerFiltroLista("#chkRel01", "#Rel01List");
		var lProv = temp.ids;
		$("#FamiliaList").children().each(function (i, item) { lFam.push($(item).val()) });
		temp = ObtenerFiltroLista("#chkRubro", "#RubrosList");
		var lRub = temp.ids;
		var desde = $("#Desde").val();
		var hasta = $("#Hasta").val();
		var agrupador = $("#listaAgrupador").val();
		var tipoReporte = $("#listaAgrupador option:selected").text();

		// 🔥 Construcción del string de filtros
		var filtrosDesc = [];

		filtrosDesc.push(ConstruirDescripcionFiltro("Sucursales", "#chkSucursales", "#SucursalesList"));
		filtrosDesc.push(ConstruirDescripcionFiltro("Proveedores", "#chkRel01", "#Rel01List"));
		filtrosDesc.push(ConstruirDescripcionFiltro("Familias", "#chkFamilias", "#FamiliaList"));
		filtrosDesc.push(ConstruirDescripcionFiltro("Rubros", "#chkRubro", "#RubrosList"));

		// Limpieza: eliminar vacíos
		filtrosDesc = filtrosDesc.filter(x => x !== "");

		// String final
		var filtrosString = filtrosDesc.join(" | ");

		var data = { lSuc, lProv, lFam, lRub, desde, hasta, agrupador, tipoReporte, filtrosString };

		cargarReporteEnArre(93, data, "REPORTE DE EVOLUCION DE VENTAS CON PERIODO ANTERIORES", "", "");
		invocacionGestorDoc({});
	}, 500);
}

function ReseteoDeReportes() {
	console.log("Reseto de reportes");
	ReporteResetArre();
}

function ConstruirDescripcionFiltro(nombre, idCheckbox, idListBox) {

	const activo = $(idCheckbox).is(":checked");
	if (!activo) return ""; // No incluir si el filtro no está activo

	const valores = [];
	$(idListBox + " option").each(function () {
		valores.push($(this).text().trim());
	});

	if (valores.length === 0) {
		return `${nombre}: Todos`;
	}

	return `${nombre}: ${valores.join(", ")}`;
}

function ValidarRangoFechas() {

	const fDesde = $("#Desde").val();
	const fHasta = $("#Hasta").val();

	// Validar que existan
	if (!fDesde || !fHasta) {
		//AbrirMensaje("ATENCIÓN", "Debe seleccionar ambas fechas.");
		AbrirMensaje("ATENCIÓN", "Debe seleccionar ambas fechas.", function () {
			$("#msjModal").modal("hide");
			return false;
		}, false, ["Aceptar"], "error!", null);
		return false;
	}

	const dDesde = new Date(fDesde);
	const dHasta = new Date(fHasta);

	// Validar fechas inválidas
	if (isNaN(dDesde.getTime()) || isNaN(dHasta.getTime())) {
		//AbrirMensaje("ATENCIÓN", "Alguna de las fechas no es válida.");
		AbrirMensaje("ATENCIÓN", "Alguna de las fechas no es válida.", function () {
			$("#msjModal").modal("hide");
			return false;
		}, false, ["Aceptar"], "error!", null);
		return false;
	}

	// Validar Desde > Hasta
	if (dDesde > dHasta) {
		//AbrirMensaje("ATENCIÓN", "La fecha 'Desde' no puede ser mayor que la fecha 'Hasta'.");
		AbrirMensaje("ATENCIÓN", "La fecha 'Desde' no puede ser mayor que la fecha 'Hasta'.", function () {
			$("#msjModal").modal("hide");
			return false;
		}, false, ["Aceptar"], "error!", null);
		return false;
	}

	// NUEVA VALIDACIÓN: ambas fechas deben pertenecer al mismo año
	if (dDesde.getFullYear() !== dHasta.getFullYear()) {
		AbrirMensaje("ATENCIÓN", "Las fechas deben pertenecer al mismo año.", function () {
			$("#msjModal").modal("hide");
			return false;
		}, false, ["Aceptar"], "error!", null);
		return false;
	}

	return true;
}

function InicializarCamposEnFiltros(vieneDeCancelar) {
	if (!vieneDeCancelar) {
		CargarRubros();
	}
	$("#chkDesdeHasta")
		.prop("checked", true)
		.prop("disabled", true);

	$("#Desde").prop("disabled", false);
	$("#Hasta").prop("disabled", false);

	$("#btnImprimir").hide();
	$("#lbSucursales").text("Sucursal");
	$("#lbDepositos").text("Depósitos");
	$("#lbRel01").text("Proveedor");
	$("#lbFamilias").text("Familia");
	$("#lbRubro").text("Rubro");
	$("#lbChkDesdeHasta").text("Periodo");

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

function CargarFamiliaLista(id) {
	var ctaId = id;
	data = { ctaId };
	PostGenHtml(data, BuscarProveedoresFamiliaURL, function (obj) {
		$("#divListaFamilias").html(obj);
		CerrarWaiting();
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