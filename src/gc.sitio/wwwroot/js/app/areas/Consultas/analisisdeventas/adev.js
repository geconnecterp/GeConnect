$(function () {
	InicializarCamposEnFiltros(false);
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
		if (validarFechasAnalisis()) {
			var lSuc = [];
			$("#SucursalesList").children().each(function (i, item) { lSuc.push($(item).val()) });
			if (lSuc.length == 0) {
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
});

function InicializarPantallaPrincipal() {
	var lSucTExt = [];
	$("#SucursalesList").children().each(function (i, item) { lSucTExt.push($(item).text()) });
	var sucursalesText = lSucTExt.join(", ");
	var desde = $("#Desde").val();
	var hasta = $("#Hasta").val();
	AbrirWaiting("Cargando información..."); 
	PostGenHtml({ sucursalesText, desde, hasta }, inicializarPantallPrincipalURL, function (obj) {
		$("#divDetalle").html(obj);
		$("#divFiltros").collapse("hide");
		$("#divDetalle").collapse("show");
		CerrarWaiting();
		setTimeout(() => {
			CargarAnalisisDeVentaMensual();
		}, 200);
		return true
	});
}

function CargarAnalisisDeVentaMensual() {
	var lSuc = [];
	$("#SucursalesList").children().each(function (i, item) { lSuc.push($(item).val()) });
	var sucursalesIds = lSuc.join(",");
	var data = {
		Desde: $("#Desde").val(),
		Hasta: $("#Hasta").val(),
		Sucursales: sucursalesIds,
	}
	AbrirWaiting("Obteniendo Análisis Mensual...");
	PostGenHtml(data, buscarAnalisisDeVentasMensualURL, function (obj) {
		$("#divAnalisisMensual").html(obj);
		CerrarWaiting();
		return true
	});
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
		if ($("#HabilitarCambioDeSucursalSeleccionada").val() == "False")
			$("#divListaSucursales").find("input, select, textarea, button").prop("disabled", true);
		else
			$("#divListaSucursales").find("input, select, textarea, button").prop("disabled", false);
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