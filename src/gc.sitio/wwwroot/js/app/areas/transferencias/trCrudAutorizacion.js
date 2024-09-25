$(function () {
	AddEventListenerToGrid("tbListaSucursales");
});

function selectTRSucursalesRow(x) {
	AbrirWaiting();
	var admId = x.cells[2].innerText.trim()
	if (admId && admId !== "") {
		var datos = { admId };
		PostGenHtml(datos, TRCargarPedidosPorSucursalUrl, function (obj) {
			$("#divListaPedidosSucursal").html(obj);
			//SelecccionarPrimerRegistroConteos();
			AddEventListenerToGrid("tbListaPedidosSucursal");
			CerrarWaiting();
			return true
		});
		PostGenHtml(datos, TRCargarDepositosInclPorSucursalUrl, function (obj) {
			$("#divDepositosDeEnvio").html(obj);
			AddEventListenerToGrid("tbListaPedidosIncluidos");
			//SelecccionarPrimerRegistroConteos();
			CerrarWaiting();
			return true
		});
	}
	else {
		AbrirMensaje("Atención", "Código de sucursal no válido.", function () {
			$("#msjModal").modal("hide");
			CerrarWaiting();
			return true;
		}, false, ["Aceptar"], "error!", null);
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

function selectPI(x) {
	console.log(x.dataset.interaction); //TODO -> Agregar el PI en la grilla de abajo via PostGenHtml, x.dataset.interaction tiene el pi_compte
}

function selectTRPedidoIncluidoRow(x) {

}

function selectTRDepositosDeEnvioRow(x) { 
}

function selectTRPedidoSucursalRow(x) {
	console.log(x);
}