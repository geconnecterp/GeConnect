$(function () {
	if ($("#divDetalle").is(":visible")) {
		$("#divDetalle").collapse("hide");
	}
	$("#divFiltro").collapse("show");

	$("#btnCancel").on("click", function () {
		window.location.href = homeCtlFiservUrl;
	});

	$("#lbSucursales").text("Sucursal");
	$("#lbDias").text("Día");

	$("#btnBuscar").on("click", function () {
		if (validarCamposSeleccionados()) {
			InicializarBusqueda();
		} else {
			AbrirMensaje("ATENCIÓN", "Debe seleccionar Sucursal y Día.", function () {
				$("#msjModal").modal("hide");
				return true;
			}, false, ["Aceptar"], "error!", null);
		}
	});

	$("#chkDias").on("click", function () {
		if ($("#chkDias").is(":checked")) {
			$("#listaDias").prop("disabled", false);
			$("#listaDias").trigger("focus");
		}
		else {
			$("#listaDias").prop("disabled", true).val("");
		}
	});
	$("#chkSucursales").prop("checked", true);
	$("#chkSucursales").prop("disabled", true);
	$("#chkSucursales").trigger('change');
	$("#listaSucursales").prop("disabled", false);

	$(document).on("change", "#listaSucursales", ControlalistaSucursalesSelected);
});

function ControlalistaSucursalesSelected() {
	var item = $("#listaSucursales").val();
	var data = { suc_id: item };
	AbrirWaiting("Cargando datos de días...");
	PostGenHtml(data, obtenerDiasPorSucursalUrl, function (html) {
		CerrarWaiting();
		$("#divListaDias").html(html);
		$("#divDetalle").empty();
		setTimeout(function () {
			$("#chkDias").prop("disabled", false);
			$("#chkDias").trigger('change');
			$("#chkDias").prop("checked", true);
			$("#listaDias").prop("disabled", false);
			$("#listaDias").trigger('focus');
		}, 0);
	});
}

function InicializarBusqueda() {
	console.log("InicializarBusqueda()");
	sucDesc_selected = $("#listaSucursales").find("option:selected").text();
	sucId_selected = $("#listaSucursales").find("option:selected").val();
	diaId_selected = $("#listaDias").find("option:selected").val();
	var data = { admDesc: sucDesc_selected, admId: sucId_selected, nroProceso: diaId_selected };
	console.log("data", data);
	AbrirWaiting("Inicializando vista...");
	PostGenHtml(data, inicializarVistaUrl, function (html) {
		$("#divDetalle").html(html);
		$("#divFiltro").collapse("hide");
		$("#divDetalle").collapse("show");
		CerrarWaiting();
	});
}

function validarCamposSeleccionados() {
	let sucSeleccionada = $("#listaSucursales").val();
	let diaSeleccionado = $("#listaDias").val();
	if (sucSeleccionada == null || sucSeleccionada == undefined || sucSeleccionada == "")
		return false;
	if (diaSeleccionado == null || diaSeleccionado == undefined || diaSeleccionado == "")
		return false;
	return true;
}