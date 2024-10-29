$(function () {
	$("#listaSucursales").on("change", listaSucursalesChange);
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
	AbrirWaiting();
	var admId = $("#listaSucursales").val();
	var meses = $("#txtMeses").val();
	var semanas = $("#txtSemanas").val();
	var pId = pIdSeleccionado;
	var datos = { pId, admId,  meses };
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
}

function selectListaInfoProdIExMesRow(x) {
}

function selectListaInfoProdIExSemanaRow(x) {
}