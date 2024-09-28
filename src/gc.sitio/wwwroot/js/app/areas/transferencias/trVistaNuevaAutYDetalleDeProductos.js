$(function () {
	AddEventListenerToGrid("tbNuevaAutListaSucursales");
	AddEventListenerToGrid("tbNuevaAutListaProductos");
});

function selectNuevaAutListaSucursalesRow(x) {
}

function selectNuevaAutListaProductosRow(x) {
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