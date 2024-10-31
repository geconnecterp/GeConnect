$(function () {
	$("#listaSucursales").on("change", listaSucursalesChange);
	$("#txtMeses").on("change", BuscarInfoAdicional);
	$("#txtSemanas").on("change", BuscarInfoAdicional);
});

function listaSucursalesChange() {
	if (pIdSeleccionado == undefined) {
		AbrirMensaje("Atención", "Debe seleccionar un producto.", function () {
			$("#msjModal").modal("hide");
			return true;
		}, false, ["Aceptar"], "error!", null);
	}
	if (pIdSeleccionado == "") {
		AbrirMensaje("Atención", "Debe seleccionar un producto.", function () {
			$("#msjModal").modal("hide");
			return true;
		}, false, ["Aceptar"], "error!", null);
	}
	BuscarInfoAdicional();
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
	var admId = $("#listaSucursales").val();
	var meses = $("#txtMeses").val();
	var semanas = $("#txtSemanas").val();
	var pId = pIdSeleccionado;
	var datos = { pId, admId, meses };
	PostGenHtml(datos, BuscarInfoProdIExMesesURL, function (obj) {
		$("#divInfoProdIExMeses").html(obj);
		AddEventListenerToGrid("tbInfoProdIExMes");
		CerrarWaiting();
		return true
	});
	datos = { pId, admId, semanas };
	PostGenHtml(datos, BuscarInfoProdIExSemanasURL, function (obj) {
		$("#divInfoProdIExSemanas").html(obj);
		AddEventListenerToGrid("tbInfoProdIExSemana");
		CerrarWaiting();
		return true
	});
	datos = { pId, admId };
	PostGenHtml(datos, BuscarInfoProdStkDepositoURL, function (obj) {
		$("#divInfoProductoStkD").html(obj);
		AddEventListenerToGrid("tbInfoProdStkD");
		CerrarWaiting();
		return true
	});
	PostGenHtml(datos, BuscarInfoProdStkSucursalURL, function (obj) {
		$("#divInfoProductoStkA").html(obj);
		AddEventListenerToGrid("tbInfoProdStkA");
		CerrarWaiting();
		return true
	});
	var tipo = "OC";
	var soloProv = true; //TODO: agregar un check en alguna parte del form, a lo mejor dentro del mismo tab de infoProdSustituto
	datos = { pId, tipo, soloProv }
	PostGenHtml(datos, BuscarInfoProdSustitutoURL, function (obj) {
		$("#divInfoProdSustituto").html(obj);
		AddEventListenerToGrid("tbListaProductoSust");
		CerrarWaiting();
		return true
	});
}

function selectListaInfoProdIExMesRow(x) {
}

function selectListaInfoProdIExSemanaRow(x) {
}