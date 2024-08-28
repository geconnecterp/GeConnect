$(document).ready(function () {
	$("#Cuenta").on("click", inicializaCuenta);
	$("#btnBuscarCC").on("click", buscarCuentasComercial);
	InicializaPantallaCC();
});

function InicializaPantallaCC() {
	switch (tipoCuenta) {
		case 'C':
			//Celeste 
			$("#iconTipoCC").removeClass(["text-primary", "text-secondary", "text-info", "text-danger"]).addClass("text-success");
			break;
		case 'B':
			$("#iconTipoCC").removeClass(["text-primary", "text-secondary", "text-success", "text-danger"]).addClass("text-info");
			break;
		case 'P':
			$("#iconTipoCC").removeClass(["text-primary", "text-success", "text-secondary", "text-info"]).addClass("text-danger");
			break;
		default:
			$("#iconTipoCC").removeClass(["text-primary", "text-success", "text-info", "text-danger"]).addClass("text-secondary");
			break;
	}
}

function inicializaCuenta() {
	$("#razonsocial").val("");
	$("#Cuenta").val("").focus();

}

function buscarCuentasComercial() {
	var cuenta = $("#Cuenta").val();
	var tipo = tipoCuenta;
	var seccion = seccionEnVista; //-> Aca inyectar el html con los datos 
	var vista = vistaParcial;
	var datos = { cuenta, tipo, vista };

	PostGen(datos, buscarCuentaUrl, function (obj) {
		if (obj.error === true) {
			AbrirMensaje("Atención", obj.msg, function () {
				$("#msjModal").modal("hide");
				return true;
			}, false, ["Aceptar"], "error!", null);
		} else if (obj.warn === true) {
			AbrirMensaje("Atención", obj.msg, function () {
				$("#msjModal").modal("hide");
				return true;
			}, false, ["Aceptar"], "warn!", null);
		} else if (obj.unico === true) {
			$("#razonsocial").val(obj.cuenta.cta_Denominacion);
			return true;
		} else {
			AbrirMensaje("Atención", "Trae una banda.....habilitar el modal para mostar la lista", function () {
				$("#msjModal").modal("hide");
				return true;
			}, false, ["Aceptar"], "warn!", null);
		}
	});
	return true;
}