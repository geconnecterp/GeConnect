$(function () {
	AddEventListenerToGrid("tbNuevaAutListaSucursales");
	AddEventListenerToGrid("tbNuevaAutListaProductos");
	$("#btnEditarCantidad").on("click", EditarCantidad);
	$("#btnRegresarANuevaAut").on("click", RegresarANuevaAut);
	$("#btnConfirmarAuto").on("click", ConfirmarAuto);
	SeleccionarFila(1, "tbNuevaAutListaSucursales");
});

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

var tipoFuncion = "";
const FuncionSobreProductosAAgregar = {
	NUEVO: 'NUEVO',
	SUSTITUTO: 'SUSTITUTO',
	EDICION: 'EDICION',
	NOSELECTED: 'NOSELECTED',
}

function RegresarANuevaAut() {
	//Validamos si existe un analisis 
	datos = {};
	PostGen(datos, TRValidarSiExisteAnalisisUrl, function (o) {
		if (o.error === true) {
			CerrarWaiting();
			AbrirMensaje("Atención", o.msg, function () {
				$("#msjModal").modal("hide");
				window.location.href = TRAbrirVistaTRCrudAutorizacionUrl;
				return true;
			}, false, ["Aceptar"], "error!", null);
		} else if (o.warn === true) {
			CerrarWaiting();
			ControlarRetorno(o.warn, o.msg, o.cant);
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

function ControlarRetorno(value, msg, cant) {
	if (value === true) {
		AbrirMensaje("Confirmación", msg + " Si decide continuar los datos se perderán.", function (e) {
			$("#msjModal").modal("hide");
			switch (e) {
				case "SI": //Confirmar comprobante RPR
					datos = {};
					PostGen(datos, TRLimpiarAnalisisUrl, function (o) {
						if (o.error === true) {
							CerrarWaiting();
							AbrirMensaje("Atención", o.msg, function () {
								$("#msjModal").modal("hide");
								window.location.href = TRAbrirVistaTRCrudAutorizacionUrl;
								return true;
							}, false, ["Aceptar"], "error!", null);
						} else {
							CerrarWaiting();
							window.location.href = TRAbrirVistaTRCrudAutorizacionUrl;
							return true;
						}
						CerrarWaiting();
						window.location.href = TRAbrirVistaTRCrudAutorizacionUrl;
						return true
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

function ConfirmarAuto() {
	AbrirMensaje("Confirmación", "Se generarán las Autorizaciones para realizar las Transferencias. ¿Está seguro de continuar?", function (e) {
		$("#msjModal").modal("hide");
		switch (e) {
			case "SI": //Confirmar transferencia
				AbrirWaiting();
				datos = {};
				PostGen(datos, TRConfirmaAutorizacionesDeTRUrl, function (o) {
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
						window.location.href = TRAbrirVistaTRAutorizacionesListaUrl;
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

function selectNuevaAutListaSucursalesRow(x) {
	admSeleccionado = x.cells[4].innerText.trim();
	admSeleccionadoNombre = x.cells[1].innerText.trim();
	autAGenerarSeleccionado = x.cells[0].innerText.trim();

	//filtrarListaDeProductosPorSucursal();
	filtrarListaDeProductosPorSucursalYAutAGenerar();
}

function filtrarListaDeProductosPorSucursalYAutAGenerar() {
	AbrirWaiting();
	var admId = admSeleccionado;
	var aut = autAGenerarSeleccionado;
	if (admId) {
		var datos = { admId, aut };
		PostGenHtml(datos, TRFiltrarListaDeProductosPorSucursalUrl, function (obj) {
			$("#divNuevaAutListaProductos").html(obj);
			AddEventListenerToGrid("tbNuevaAutListaProductos");
			CerrarWaiting();
			return true
		});
	}
	CerrarWaiting();
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
	prodSeleccionadoNombre = x.cells[1].innerText.trim();
	sustituto = x.cells[11].children[0].checked;
	$('#btnSustituto').prop('disabled', !sustituto);
	$('#btnEliminar').prop('disabled', false);
	$('#btnModCantidad').prop('disabled', false);
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
	var autorizacion = x.dataset.interaction2;
	if (admId) {
		var datos = { admId, autorizacion };
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
	var autorizacion = $("#autorizacion_en_modal_sucursal").val();
	var datos = { nota, admId, autorizacion };
	if (admId && autorizacion) {
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
				$("#tbNuevaAutListaSucursales tbody tr").each(function (index) {
					var row = $(this);
					if (row[0].cells[0]) {
						if (row[0].cells[0].innerText == autorizacion && row[0].cells[4].innerText == admId && nota !== "") {
							row[0].cells[3].childNodes[0].classList.remove("btn-outline-success")
							row[0].cells[3].childNodes[0].classList.add("btn-outline-danger")
						}
						else if (row[0].cells[0].innerText == autorizacion && row[0].cells[4].innerText == admId && nota == "") {
							row[0].cells[3].childNodes[0].classList.remove("btn-outline-danger")
							row[0].cells[3].childNodes[0].classList.add("btn-outline-success")
						}
					}
				});
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
		esProductoSustituto = false;
		tipoFuncion = FuncionSobreProductosAAgregar.NUEVO;
		CerrarWaiting();
		return true
	});
	CerrarWaiting();
}

function abrirlModalSustitutoDeProductoATR() {
	AbrirWaiting();
	var admId = admSeleccionado;
	var pId = prodSeleccionado;
	var listaDepo = "";
	var tipo = "S";
	var datos = { admId, prodSeleccionado, listaDepo, tipo };
	PostGenHtml(datos, TRInicializarModalAgregarProductoSustitutoATRUrl, function (obj) {
		$("#divListaProductosParaAgregar").html(obj);
		document.getElementById("modalCenterTitle").outerHTML = "<h5 class=\"modal-title\" id=\"modalCenterTitle\"> Detalle de TR (" + admSeleccionado + ") " + admSeleccionadoNombre + "</h5>";
		document.getElementById("leyendaNuevoProducto").outerHTML = "<h5 id=\"leyendaNuevoProducto\"> Producto Sustituto de (" + prodSeleccionado + ") " + prodSeleccionadoNombre + "</h5>";
		document.getElementById("divBusquedaProducto").style.display = 'none'
		$('#modalCargarNuevoProducto').modal('show')
		esProductoSustituto = true;
		tipoFuncion = FuncionSobreProductosAAgregar.SUSTITUTO;
		AddEventListenerToGrid("tbListaProductosParaAgregar");
		CerrarWaiting();
		return true
	});
	CerrarWaiting();
}

function abrirlModalModCantDeProductoATR() {
	AbrirWaiting();
	var admId = admSeleccionado;
	var pId = prodSeleccionado;
	var datos = { admId, pId };
	PostGenHtml(datos, TRInicializarModalModificarCantidadATRUrl, function (obj) {
		$("#divListaProductosParaAgregar").html(obj);
		document.getElementById("modalCenterTitle").outerHTML = "<h5 class=\"modal-title\" id=\"modalCenterTitle\"> Detalle de TR (" + admSeleccionado + ") " + admSeleccionadoNombre + "</h5>";
		document.getElementById("leyendaNuevoProducto").outerHTML = "<h5 id=\"leyendaNuevoProducto\"> Producto Sustituto de (" + prodSeleccionado + ") " + prodSeleccionadoNombre + "</h5>";
		document.getElementById("divBusquedaProducto").style.display = 'none'
		$('#modalCargarNuevoProducto').modal('show')
		esProductoSustituto = false;
		tipoFuncion = FuncionSobreProductosAAgregar.EDICION;
		AddEventListenerToGrid("tbListaProductosParaAgregar");
		SeleccionarFila(1, "tbListaProductosParaAgregar");
		CerrarWaiting();
		return true
	});
	CerrarWaiting();
}

function SeleccionarFila(fila, tabla) {
	var grilla = document.getElementById(tabla);
	if (grilla) {
		if (grilla.rows.length > 1) {
			grilla.rows[fila].classList.add('selected-row');
			if (tabla === "tbListaProductosParaAgregar") {
				idProdDeProdSeleccionado = grilla.rows[fila].cells[0].innerText.trim();
				prodSeleccionadoNombre = grilla.rows[fila].cells[1].innerText.trim();
			}
			if (tabla === "tbNuevaAutListaSucursales") {
				admSeleccionado = grilla.rows[fila].cells[4].innerText.trim();
				admSeleccionadoNombre = grilla.rows[fila].cells[1].innerText.trim();
				autAGenerarSeleccionado = grilla.rows[fila].cells[0].innerText.trim();
				filtrarListaDeProductosPorSucursalYAutAGenerar();
			}
		}
	}
}

function BuscarSustituto(p_id) {
	AbrirWaiting();
	var pId = p_id;
	var admId = admSeleccionado;
	var listaDepo = "";
	var admIdDesc = admSeleccionado;
	var tipo = "N";
	var datos = { pId, listaDepo, admIdDesc, tipo };
	PostGenHtml(datos, TRCargarListaProductoSustitutoUrl, function (obj) {
		$("#divListaProductosParaAgregar").html(obj);
		AddEventListenerToGrid("tbListaProductosParaAgregar");
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


//function analizaEnterInput(e) {
//}

function cargarProductos() {
}

function EliminarProducto(id) {
}

function InicializaPantalla() {
}

function selectProductosParaAgregarRow(x) {
	stkDeProdSeleccionado = x.cells[6].innerText.trim();
	boxDeProdSeleccionado = x.cells[5].innerText.trim();
	pedidoDeProdSeleccionado = x.cells[3].innerText.trim();
	idProvDeProdSeleccionado = x.cells[2].innerText.trim();
	idProdDeProdSeleccionado = x.cells[0].innerText.trim();
}

function EditarCantidad() {
	var cantidad = $("#txtAtransferir").val();
	if (cantidad === "") {
		AbrirMensaje("Atención", "La cantidad ingresada no posee un formato válido.", function () {
			$("#msjModal").modal("hide");
			return true;
		}, false, ["Aceptar"], "warn!", null);
	}
	else if (stkDeProdSeleccionado === "") {
		AbrirMensaje("Atención", "El stock del producto seleccionado no posee un formato válido.", function () {
			$("#msjModal").modal("hide");
			return true;
		}, false, ["Aceptar"], "warn!", null);
	}
	else {
		switch (tipoFuncion) {
			case FuncionSobreProductosAAgregar.NUEVO:
				var datos = { idProdDeProdSeleccionado, idProvDeProdSeleccionado, pedidoDeProdSeleccionado, boxDeProdSeleccionado, stkDeProdSeleccionado, cantidad, admSeleccionado, admSeleccionadoNombre };
				PostGen(datos, TRAgregarNuevoProductoUrl, function (o) {
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
						//Cerrar modal y actualizar lista
						CerrarWaiting();
						$("#modalCargarNuevoProducto").modal("hide")
						filtrarListaDeProductosPorSucursal();
					}
					tipoFuncion = FuncionSobreProductosAAgregar.NOSELECTED;
				});
				break;
			case FuncionSobreProductosAAgregar.SUSTITUTO:
				var idProductoSustituto = prodSeleccionado;
				var datos = { idProdDeProdSeleccionado, idProductoSustituto, idProvDeProdSeleccionado, pedidoDeProdSeleccionado, boxDeProdSeleccionado, stkDeProdSeleccionado, cantidad, admSeleccionado, admSeleccionadoNombre };
				PostGen(datos, TRAgregarProductoSustitutoUrl, function (o) {
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
						//Cerrar modal y actualizar lista
						CerrarWaiting();
						$("#modalCargarNuevoProducto").modal("hide")
						filtrarListaDeProductosPorSucursal();
					}
					tipoFuncion = FuncionSobreProductosAAgregar.NOSELECTED;
				});
				break;
			case FuncionSobreProductosAAgregar.EDICION:
				var datos = { idProdDeProdSeleccionado, admSeleccionado, cantidad };
				PostGen(datos, TREditarCantidadEnProductoUrl, function (o) {
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
						//Cerrar modal y actualizar lista
						CerrarWaiting();
						$("#modalCargarNuevoProducto").modal("hide")
						filtrarListaDeProductosPorSucursal();
					}
					tipoFuncion = FuncionSobreProductosAAgregar.NOSELECTED;
				});
				break;
			default:
		}
	}
}

function eliminarProductoATR() {
	if (prodSeleccionado == "") {
		AbrirMensaje("Atención", "Debe seleccionar un producto para eliminar.", function (e) {
			$("#msjModal").modal("hide");
			return true;
		}, false, ["Aceptar"], "info!", null);
	}
	if (admSeleccionado == "") {
		AbrirMensaje("Atención", "Debe una sucursal antes de intentar eliminar un producto.", function (e) {
			$("#msjModal").modal("hide");
			return true;
		}, false, ["Aceptar"], "info!", null);
	}

	AbrirMensaje("Confirmación", "Esta a punto eliminar el producto (" + prodSeleccionado + ") " + prodSeleccionadoNombre + ", desea continuar? Esta acción no se puede revertir.", function (e) {
		$("#msjModal").modal("hide");
		switch (e) {
			case "SI": //Confirmar comprobante RPR
				var datos = { pId: prodSeleccionado, admId: admSeleccionado, autorizacion: autAGenerarSeleccionado };
				PostGenHtml(datos, TREliminarProductoUrl, function (obj) {
					$("#tbNuevaAutListaProductos").html(obj);
					AddEventListenerToGrid("tbListaProductosParaAgregar");
					CerrarWaiting();
					return true
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