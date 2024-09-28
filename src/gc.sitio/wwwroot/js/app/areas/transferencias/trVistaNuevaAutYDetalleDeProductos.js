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

function verNotaEnSucursal(x) {
	AbrirWaiting();
	var admId = x.dataset.interaction;
	if (admId) {
		var datos = { admId };
		PostGenHtml(datos, TREditarNotaEnSucursalUrl, function (obj) {
			$("#divNotaEnSucursal").html(obj);
			$('#modalNotaEnSucursal').modal('show')
			CerrarWaiting();
			return true
		});
	}
	CerrarWaiting();
}

function verNotaEnProducto(x) {
	AbrirWaiting();
	var pId = x.dataset.interaction;
	if (pId) {
		var datos = { pId };
		PostGenHtml(datos, TREditarNotaEnProductoUrl, function (obj) {
			$("#divNotaEnProducto").html(obj);
			$('#modalNotaEnProducto').modal('show')
			CerrarWaiting();
			return true
		});
	}
	CerrarWaiting();
}