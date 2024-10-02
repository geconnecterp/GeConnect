﻿$(function () {
	AddEventListenerToGrid("tbNuevaAutListaSucursales");
	AddEventListenerToGrid("tbNuevaAutListaProductos");
});

function selectNuevaAutListaSucursalesRow(x) {
	admSeleccionado = x.cells[4].innerText.trim();
	admSeleccionadoNombre = x.cells[1].innerText.trim();

	filtrarListaDeProductosPorSucursal();
}

function filtrarListaDeProductosPorSucursal() {
	AbrirWaiting();
	var admId = admSeleccionado;
	if (admId) {
		var datos = { admId };
		PostGenHtml(datos, TRFiltrarListaDeProductosPorSucursalUrl, function (obj) {
			$("#divNuevaAutListaProductos").html(obj);
			AddEventListenerToGrid("tbNuevaAutListaProductos");
			CerrarWaiting();
			return true
		});
	}
	CerrarWaiting();
}

function selectNuevaAutListaProductosRow(x) {
	prodSeleccionado = x.cells[0].innerText.trim();
	sustituto = x.cells[11].children[0].checked;
	$('#btnSustituto').prop('disabled', !sustituto);
	
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

function abrirlModalAgregaProductoATR() {
	AbrirWaiting();
	var admId = admSeleccionado;
	var datos = { admId };
	PostGenHtml(datos, TRInicializarModalAgregarProductoATRUrl, function (obj) {
		$("#divListaProductosParaAgregar").html(obj);
		document.getElementById("modalCenterTitle").outerHTML = "<h5 class=\"modal-title\" id=\"modalCenterTitle\"> Detalle de TR (" + admSeleccionado + ") " + admSeleccionadoNombre + "</h5>";
		$('#modalCargarNuevoProducto').modal('show')
		CerrarWaiting();
		return true
	});
	CerrarWaiting();
}

function verificaEstado(e) {
	FunctionCallback = null; //inicializo funcion por si tiene alguna funcionalidad asignada.
	var res = $("#estadoFuncion").val();
	CerrarWaiting();
	if (res === "true") {
		var prod = productoBase;
		if (prod) { //Producto existe
			if (ExisteProductoEnTR(prod.p_id)) {
				return false;
			}
			BuscarSustituto(prod.p_id);
		}
	}
	return true;
}

//Llamar al SP SPGECO_TR_sustituto (el cual no existe, voy a probar con el SP SPGECO_TR_Aut_Sustituto)
function BuscarSustituto(p_id){
	AbrirWaiting();
	var pId = p_id;
	var admId = admSeleccionado;
	var listaDepo = "";
	var admIdDesc = admSeleccionado;
	var tipo = "N";
	var datos = { pId, listaDepo, admIdDesc, tipo };
	PostGenHtml(datos, TRCargarListaProductoSustitutoUrl, function (obj) {
		$("#divListaProductosParaAgregar").html(obj);
		CerrarWaiting();
		return true
	});
	CerrarWaiting();
}

function ExisteProductoEnTR(p_id) {
	AbrirWaiting();
	var pId = p_id;
	var admId = admSeleccionado;
	var datos = { pId, admId };
	PostGen(datos, TRExisteProductoEnTRUrl, function (o) {
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
			return false;
		}
	});
}


function analizaEnterInput(e) {
	console.log("analizaEnterInput" + e);
}

function cargarProductos() {
}

function EliminarProducto(id) {
	console.log(id);
}

function InicializaPantalla() {
}