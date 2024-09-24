$(function () {
});

function selectTRSucursalesRow(x) {
	AbrirWaiting();
	var admId = x.cells[2].innerText.trim()
	if (admId && admId !== "") {
		var datos = { admId };
		PostGenHtml(datos, TRCargarPedidosPorSucursalUrl, function (obj) {
			$("#divListaPedidosSucursal").html(obj);
			//AgregarHandlerDeSeleccion("tbDetalleVerCompte");
			//SelecccionarPrimerRegistroConteos();
			CerrarWaiting();
			return true
		});
		PostGenHtml(datos, TRCargarDepositosInclPorSucursalUrl, function (obj) {
			$("#divDepositosDeEnvio").html(obj);
			//AgregarHandlerDeSeleccion("tbDetalleVerCompte");
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

function selectTRPedidoIncluidoRow(x) {

}

function selectTRDepositosDeEnvioRow(x) { 
}