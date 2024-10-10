$(function () {
	CerrarWaiting();
	ValidarTipoTR();
});

function regresarATRLista() {
	window.location.href = TRRegresarAAutorizacionesListaUrl;
}

function ValidarTipoTR() {
	var tipo = $("#tipo").val();
	if (tipo === "S") {
		$('#btnConfirmar').prop('disabled', true);
	}
}

function validarAutorizacion() {
	var ti = $("#ti").val();
	if (ti === "") {
		AbrirMensaje("Atención", "La Transferencia seleccionado no es válida.", function () {
			$("#msjModal").modal("hide");
			return false;
		}, false, ["Aceptar"], "error!", null);
	}
	else {
		AbrirWaiting();
		var datos = { ti }
		PostGen(datos, TRValidarTransferenciaUrl, function (o) {
			if (o.error === true) {
				CerrarWaiting();
				AbrirMensaje("Atención", o.msg, function () {
					$("#msjModal").modal("hide");
					return true;
				}, false, ["Aceptar"], "error!", null);
			} else if (o.warn === true) {
				CerrarWaiting();
				AbrirMensaje("Atención", o.msg, function () {
					$("#msjModal").modal("hide");
					return true;
				}, false, ["Aceptar"], "warn!", null);
			} else if (o.msg !== "") {
				CerrarWaiting();
				AbrirMensaje("Atención", o.msg, function (e) {
					$("#msjModal").modal("hide");
					return true;
				}, false, ["Aceptar"], "info!", null);
			} else {
				CerrarWaiting();
				confirmarAutorizacion(ti);
			}
		});
	}
}

function confirmarAutorizacion(ti) {
	AbrirMensaje("Confirmación", "Desea confirmar?", function (e) {
		$("#msjModal").modal("hide");
		switch (e) {
			case "SI": //
				AbrirWaiting();
				var datos = { ti }
				PostGen(datos, TRConfirmarAutorizacionUrl, function (o) {
					if (o.error === true) {
						CerrarWaiting();
						AbrirMensaje("Atención", o.msg, function () {
							$("#msjModal").modal("hide");
							return true;
						}, false, ["Aceptar"], "error!", null);
					} else if (o.warn === true) {
						CerrarWaiting();
						AbrirMensaje("Atención", o.msg, function () {
							$("#msjModal").modal("hide");
							return true;
						}, false, ["Aceptar"], "warn!", null);
					} else if (o.msg !== "") {
						CerrarWaiting();
						AbrirMensaje("Atención", o.msg, function (e) {
							$("#msjModal").modal("hide");
							return true;
						}, false, ["Aceptar"], "info!", null);
					} else {
						CerrarWaiting();
						//TODO: Imprimir Remito
						window.location.href = TRRegresarAAutorizacionesListaUrl;
					}
				});
				break;
			case "NO":
				break;
			default: //NO
				break;
		}
		return true;
	}, true, ["Aceptar", "Cancelar"], "info!", null);
}