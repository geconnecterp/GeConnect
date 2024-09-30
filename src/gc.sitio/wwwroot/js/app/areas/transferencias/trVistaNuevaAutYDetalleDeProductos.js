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

function guardarNotaDeSucursal() {
	AbrirWaiting();
	var nota = $("#txtNotaEnSucursal").val();
	var admId = $("#adm_id_en_modal_sucursal").val();
	var datos = { nota, admId };
	if (admId && nota) {
		PostGen(datos, TRAgregarNotaASucursalNuevaAutUrl, function (o) {
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
			} else if (o.codigo !== "" && o.msg !== "") {
				CerrarWaiting();
				AbrirMensaje("Atención", o.msg, function (e) {
					$("#msjModal").modal("hide");
					return true;
				}, false, ["Aceptar"], "info!", null);
			} else {
				CerrarWaiting();
				$("#modalNotaEnSucursal").modal("hide");
			}
		});
	}
	else {
		CerrarWaiting();
		AbrirMensaje("Atención", "Algunos de los valores de la nota que desea anexar no son válidos.", function () {
			$("#msjModal").modal("hide");
			return true;
		}, false, ["Aceptar"], "error!", null);
	}
}

function guardarNotaDeProducto() {
	AbrirWaiting();
	var nota = $("#txtNotaEnProducto").val();
	var pId = $("#p_id_en_modal_producto").val();
	var datos = { nota, pId };
	if (pId && nota) {
		PostGen(datos, TRAgregarNotaAProductoNuevaAutTRUrl, function (o) {
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
			} else if (o.codigo !== "" && o.msg !== "") {
				CerrarWaiting();
				AbrirMensaje("Atención", o.msg, function (e) {
					$("#msjModal").modal("hide");
					return true;
				}, false, ["Aceptar"], "info!", null);
			} else {
				CerrarWaiting();
				$("#modalNotaEnProducto").modal("hide");
			}
		});
	}
	else {
		CerrarWaiting();
		AbrirMensaje("Atención", "Algunos de los valores de la nota que desea anexar no son válidos.", function () {
			$("#msjModal").modal("hide");
			return true;
		}, false, ["Aceptar"], "error!", null);
	}
}