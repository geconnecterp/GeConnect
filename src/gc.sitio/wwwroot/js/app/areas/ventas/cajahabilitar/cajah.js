$(function () {
	$(document).on("click", "#btnHabilitacionGeneralCaja", ControlaHabilitacionGeneralCaja);
	$(document).on("click", "#btnSalir", ControlaSalir);
});

function ControlaHabilitacionGeneralCaja() {
	AbrirMensaje("ATENCIÓN", `¿Esta seguro que desea habilitar las cajas?`, function (e) {
		$("#msjModal").modal("hide");
		switch (e) {
			case "SI":
				HandlerHabilitarCaja();
				break;
			case "NO":
				break;
			default: //NO
				break;
		}
		return true;

	}, true, ["Aceptar", "Cancelar"], "question!", null);
}

function HandlerHabilitarCaja() {
	AbrirWaiting("Confirmando habilitación de cajas...");
	PostGen({}, habilitarCajasUrl, function (obj) {
		CerrarWaiting();
		if (!obj.ok && obj.error && obj.msg === "No autenticado") {
			window.location.href = login;
			return false;
		}

		if (obj.error === true) {
			AbrirMensaje("ATENCIÓN", obj.msg, function () {
				$("#msjModal").modal("hide");
				return true;
			}, false, ["Aceptar"], "error!", null);
		}
		else if (obj.warn === true) {
			AbrirMensaje("ATENCIÓN", obj.msg, function () {
				$("#msjModal").modal("hide");
				return true;
			}, false, ["Aceptar"], "warn!", null);
		}
		else {
			setTimeout(() => {
				AbrirMensaje("ATENCIÓN", `Se realizado la habilitación de las cajas de forma correcta. ID: ${obj.id}`, function () {
					$("#msjModal").modal("hide");
					//ImprimirComprobante(obj.id);
					//btnAbmCancelarControlar();
					return true;
				}, false, ["Aceptar"], "succ!", null);
			}, 200);
		}
	});
}

function ControlaSalir() {
}