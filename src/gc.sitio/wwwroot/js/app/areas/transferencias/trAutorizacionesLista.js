$(function () {
	titId = "S";
	AddEventListenerToGrid("tbListaAutorizaciones");
	$("#btnradioSucursales").on("click", buscarPorSucursal);
	$("#btnradioDepositos").on("click", buscarPorDeposito);
});

function selectTRRow(x) {
	if (x) {
		trSeleccionada = x.cells[0].innerText.trim();
		sucSeleccionada = x.cells[1].innerText.trim();
	}
	else {
		trSeleccionada = "";
		sucSeleccionada = "";
	}
}

function verTransferencia() {
	if (trSeleccionada === "") {
		AbrirMensaje("Atención", "Debe seleccionar una transferencia.", function () {
			$("#msjModal").modal("hide");
			return false;
		}, false, ["Aceptar"], "error!", null);
	}
	else {
		AbrirWaiting();
		link = TRVerTransferenciaUrl + "?ti=" + trSeleccionada + "&tipo=" + titId + "&destino=" + sucSeleccionada;
		window.location.href = link;
	}
}

function buscarPorSucursal() {
	titId = "S";
	buscarAuto(titId);
}

function buscarPorDeposito() {
	titId = "D";
	buscarAuto(titId);
}

function buscarAuto(titId) {
	AbrirWaiting();
	var datos = { titId };
	PostGenHtml(datos, TRAutorizacionesListaPorSucursalUrl, function (obj) {
		$("#divListaAutorizaciones").html(obj);
		AddEventListenerToGrid("tbListaAutorizaciones");
		CerrarWaiting();
		return true
	});
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