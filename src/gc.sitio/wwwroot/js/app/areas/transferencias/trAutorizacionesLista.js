$(function () {
	$("#btnradioSucursales").on("click", buscarPorSucursal);
	$("#btnradioDepositos").on("click", buscarPorDeposito);
});

function selectTRRow(x) {

}

function buscarPorSucursal() {
	var titId = "S";
	buscarAuto(titId);
}

function buscarPorDeposito() {
	var titId = "D";
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