$(function () {
	CargarDetalleDeProductosEnRP();

});

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