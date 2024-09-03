$(function () {
	CargarDetalleDeProductosEnRP();
	CargarOCxCuenta();
});

function CargarOCxCuenta() {
	var cta_id = document.getElementById('cta_id').value;
	var data = { cta_id };
	PostGenHtml(data, CargarOCxCuentaEnRPUrl, function (obj) {
		$("#divOrdenDeCompraXCuenta").html(obj);
		return true;
	}, function (obj) {
		ControlaMensajeError(obj.message);
		return true;
	});
}

function CargarDetalleDeProductosEnRP() {
	var data = { };
	PostGenHtml(data, CargarDetalleDeProductosEnRPUrl, function (obj) {
		$("#divDetalleDeProductos").html(obj);
		return true;
	}, function (obj) {
		ControlaMensajeError(obj.message);
		return true;
	});
}

function selectOCRow(x) {

	$("#ocCompteSelected").val(x.cells[0].innerText.trim());
	var oc_compte = x.cells[0].innerText.trim();
	var data = { oc_compte };
	PostGenHtml(data, VerDetalleDeOCRPUrl, function (obj) {
		$("#divDetalleDeOrdenDeCompra").html(obj);
		return true;
	}, function (obj) {
		ControlaMensajeError(obj.message);
		return true;
	});
}