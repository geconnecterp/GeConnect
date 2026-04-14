$(function () {
	$(document).on("click", "#btnCierreGeneralCaja", ControlaCierreGeneralCaja);
	$(document).on("click", "#btnSalir", ControlaSalir);
});

function ControlaCierreGeneralCaja() {
	AbrirMensaje("ATENCIÓN", `¿Esta seguro que desea hacer el cierre de las cajas?`, function (e) {
		$("#msjModal").modal("hide");
		switch (e) {
			case "SI":
				HandlerCierreDeCajas();
				break;
			case "NO":
				break;
			default: //NO
				break;
		}
		return true;

	}, true, ["Aceptar", "Cancelar"], "question!", null);
}

function HandlerCierreDeCajas() {
	AbrirWaiting("Confirmando cierre de cajas...");
	PostGen({}, cerrarCajasUrl, function (obj) {
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
				AbrirMensaje("ATENCIÓN", `Se realizado el cierre de las cajas de forma correcta.`, function () {
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