$(document).ready(function () {
	$("#btnNuevaAut").on("click", cargarVistaNuevaAut);
});

function cargarProductos() {
	var _post = nuevaAutorizacionUrl;
	var datos = null;
	var rp_id = $("#rp_id").val();
	datos = { rp: rp_id };
	AbrirWaiting();
	PostGen(datos, _post, function (obj) {
		if (obj.error === true) {
			ControlaMensajeError(obj.msg);
			CerrarWaiting();
			return true;
		}
		else {
			CerrarWaiting();

		}
	}
}
