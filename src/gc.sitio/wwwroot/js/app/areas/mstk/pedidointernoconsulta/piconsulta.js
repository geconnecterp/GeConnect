$(function () {
	$("#pagEstado").on("change", function () {
		var div = $("#divPaginacion");
		cargaPaginacion();
	});

	InicializarCamposEnFiltros(false);

	$("#FechaDesde, #FechaHasta").on("blur", ValidarFechasClick);
	$(document).on("change", "#listaSucursales", ControlalistaSucursalesSelected);
	$(document).on("change", "#listaEstados", ControlalistaEstadosSelected);

	$("#SucursalesList").on("dblclick", 'option', function () { $(this).remove(); })
	$("#EstadosList").on("dblclick", 'option', function () { $(this).remove(); })

	$("#btnBuscar").on("click", function () {
		if (validarFechas()) {
			dataBak = "";
			pagina = 1;
			BuscarPedidosInternos(pagina);
		} else {
			AbrirMensaje("ATENCIÓN", "Problemas con las fechas, por favor verifique.", function () {
				$("#msjModal").modal("hide");
				return true;
			}, false, ["Aceptar"], "error!", null);
		}
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

	funcCallBack = BuscarPedidosInternos;
});

function ControlalistaSucursalesSelected() {
	var item = $("#listaSucursales").val();
	var desc = $("#listaSucursales option:selected").text();
	if ($("#SucursalesList").has('option:contains("' + item + '")').length === 0 && $("#SucursalesList").has('option:contains("' + desc + '")').length === 0) {
		var opc = "<option value=" + item + ">" + desc + "</option>"
		$("#SucursalesList").append(opc);
	}
}

function ControlalistaEstadosSelected() {
	var item = $("#listaEstados").val();
	var desc = $("#listaEstados option:selected").text();
	if ($("#EstadosList").has('option:contains("' + item + '")').length === 0 && $("#EstadosList").has('option:contains("' + desc + '")').length === 0) {
		var opc = "<option value=" + item + ">" + desc + "</option>"
		$("#EstadosList").append(opc);
	}
}

function BuscarPedidosInternos(num) {
}

function cargaPaginacion() {
	$("#divPaginacion").pagination({
		items: totalRegs,
		itemsOnPage: pagRegs,
		cssStyle: "dark-theme",
		currentPage: pagina,
		onPageClick: function (num) {
			BuscarPedidosInternos(num);
		}
	});
	$("#pagEstado").val(false);
	$("#divFiltro").collapse("hide")
	return true;
}

function InicializarCamposEnFiltros(vieneDeCancelar) {
	if (!vieneDeCancelar) {
	}

	$("#chkDesdeHasta").prop('checked', true);
	$("#chkDesdeHasta").trigger("change");
	$("#chkDesdeHasta").prop("disabled", true);

	$("#btnImprimir").hide();
	$("#lbChkDesdeHasta").text("Fechas");
	$("#lbSucursales").text("Sucursal que genera");
	$("#lbEstados").text("Estado");

	$("#chkSucursales").prop('checked', false);
	$("#chkSucursales").trigger("change");
	$("#chkEstados").prop('checked', false);
	$("#chkEstados").trigger("change");

	$("#listaSucursales").val("");
	$("#listaEstados").val("");

	$("#divFiltros").collapse("show");
	$("#divDetalle").collapse("hide");

	$("#SucursalesList").empty();
	$("#EstadosList").empty();

	$("#listaSucursales").val("");
	$("#listaEstados").val("");

	if (!vieneDeCancelar) {
		HandlerCheckBox();
	}
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
	$("#chkEstados").on("click", function () {
		if ($("#chkEstados").is(":checked")) {
			$("#listaEstados").prop("disabled", false);
			$("#EstadosList").prop("disabled", false);
			$("#listaEstados").trigger("focus");
		}
		else {
			$("#listaEstados").prop("disabled", true);
			$("#EstadosList").prop("disabled", true);
			$("#listaEstados").val("");
			$("#EstadosList").empty();
		}
	});
}

function validarFechas() {
	let desde = $("#FechaDesde").val();
	let hasta = $("#FechaHasta").val();

	if (!desde || !hasta) return false;

	let fechaDesde = new Date(desde);
	let fechaHasta = new Date(hasta);

	const diffMs = hasta - desde;
	const diffDias = diffMs / (1000 * 60 * 60 * 24);

	if (diffDias > 370) {
		return false;
	}

	return !(fechaDesde > fechaHasta);
}

function ValidarFechasClick() {
	const desdeStr = $("#FechaDesde").val();
	const hastaStr = $("#FechaHasta").val();

	if (!desdeStr || !hastaStr)
		return;

	const desde = new Date(desdeStr);
	const hasta = new Date(hastaStr);

	if (desde > hasta) {
		AbrirMensaje("ATENCIÓN", "El valor de Fecha Desde no puede ser mayor a Fecha Hasta, revise.", function () {
			$("#msjModal").modal("hide");
			$("#FechaDesde").val($("#FechaHasta").val());
			return true;
		}, false, ["Aceptar"], "error!", null);
		return;
	}

	const diffMs = hasta - desde;
	const diffDias = diffMs / (1000 * 60 * 60 * 24);

	if (diffDias > 370) {

		// Calcular fechas por defecto
		const hoy = new Date();
		const hace30 = new Date();
		hace30.setDate(hoy.getDate() - 30);

		// Formatear a yyyy-MM-dd para los inputs type="date"
		const fmt = d => d.toISOString().split("T")[0];

		AbrirMensaje("ATENCIÓN", "El rango entre fechas no puede superar los 370 días.", function () {
			$("#msjModal").modal("hide");

			$("#FechaDesde").val(fmt(hace30));
			$("#FechaHasta").val(fmt(hoy));

			$("#FechaDesde").trigger('focus');
			return true;
		}, false, ["Aceptar"], "error!", null);

		return;
	}

}
