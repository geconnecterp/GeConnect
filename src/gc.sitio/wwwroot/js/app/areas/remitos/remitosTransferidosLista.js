$(function () {
	AddEventListenerToGrid("tbListaRemitosTransferidos");
});

function selectRemitoRow(x) {
	var link = "";
	if (x) {
		remSeleccionado = x.cells[0].innerText.trim();
		remQuienEnvia = x.cells[2].innerText.trim();
		link = RVerDetalleDeConteosDeRemitoUrl + "?remCompte=" + remSeleccionado + "&quienEnvia=" + remQuienEnvia;
		$("#btnVer").prop("href", link);
	}
	else {
		remQuienEnvia = "";
		remSeleccionado = "";
		$("#btnVer").prop("href", link);
	}
}

function autorizarRemito() {
	if (!remSeleccionado) {
		AbrirMensaje("Atención", "Debe seleccionar un remito.", function () {
			$("#msjModal").modal("hide");
			return false;
		}, false, ["Aceptar"], "error!", null);
	}
	else if (remSeleccionado === "") {
		AbrirMensaje("Atención", "Debe seleccionar un remito.", function () {
			$("#msjModal").modal("hide");
			return false;
		}, false, ["Aceptar"], "error!", null);
	}
	else {
		AbrirMensaje("Confirmación", "Desea poner en curso el remito?", function (e) {
			$("#msjModal").modal("hide");
			switch (e) {
				case "SI": //
					AbrirWaiting();
					var remCompte = remSeleccionado;
					var estado = "C" // -> Estado: Autorizar
					var datos = { remCompte, estado }
					PostGen(datos, RSetearEstadoDeRemitoUrl, function (o) {
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
							window.location.href = RActualizarListadoDeRemitoUrl;
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
}

function AddEventListenerToGrid(tabla) {
	var grilla = document.getElementById(tabla);
	if (grilla) {
		document.getElementById(tabla).addEventListener('click', function (e) {
			if (e.target.nodeName === 'TD') {
				var selectedRow = this.querySelector('.selected-row');
				if (selectedRow) {
					selectedRow.classList.remove('selected-row');
				}
				e.target.closest('tr').classList.add('selected-row');
			}
		});
	}
}

function confirmarRemito() {
	var remCompte = $("#remCompte").val();
	if (!remCompte) {
		AbrirMensaje("Atención", "Debe seleccionar un remito.", function () {
			$("#msjModal").modal("hide");
			return false;
		}, false, ["Aceptar"], "error!", null);
	}
	AbrirMensaje("Confirmación", "Desea confirmar la recepción del remito?", function (e) {
		$("#msjModal").modal("hide");
		switch (e) {
			case "SI": //
				AbrirWaiting();
				var datos = { remCompte }
				PostGen(datos, RConfirmarRecepcionDeRemitoUrl, function (o) {
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
						window.location.href = RActualizarListadoDeRemitoUrl;
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

function regresarARemitosTransferidosLista() {
	window.location.href = RVolverARemitosTransferidosListaUrl;
}

function verDetalleConteosRemito() {
	AbrirWaiting();
	var remCompte = remSeleccionado;
	var quienEnvia = remQuienEnvia
	var datos = { remCompte, quienEnvia }
	PostGen(datos, RVerDetalleDeConteoDeRemitoUrl, function (o) {
		CerrarWaiting();
	});
}

function selectVerConteosRow(x) {

}