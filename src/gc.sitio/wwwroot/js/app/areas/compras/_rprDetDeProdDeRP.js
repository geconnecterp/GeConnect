$(function () {
	$("#btnAddOCProdEnComprobanteRP").on("click", AgregarProdDesdeDetalleDeOC);
	$("#btnAddProdEnComprobanteRP").on("click", AgregarProdManual);
	CargarOCxCuenta();
	$("#btnRegresarDesdeComprobanteRP").on("click", RegresarDesdeComprobanteRP);
	$("#btnAceptarComprobanteRP").on("click", AceptarDesdeComprobanteRP);
	$("#btnDelProdEnComprobanteRP").on("click", DelProdEnComprobanteRP);
	$("#txtUPEnComprobanteRP").on("keyup", analizaInputUP);
	$("#txtBtoEnComprobanteRP").on("keyup", analizaInputBto);
	$("#txtUnidEnComprobanteRP").on("keyup", analizaInputUnid);
	CargarDetalleDeProducto();
	
});

function analizaInputUP(x) {
	if (x.which == "13") {
		$("#txtBtoEnComprobanteRP").focus();
	}
}

function analizaInputBto(x) {
	if (x.which == "13") {
		$("#txtUnidEnComprobanteRP").focus();
	}
}

function analizaInputUnid(x) {
	if (x.which == "13") {
		$("#btnAddProdEnComprobanteRP").focus();
	}
}

//Valores de parametro:
//0-> Agregar
//1-> Reemplazar
//2-> Acumular
function AgregarProdManual() {
	var idProd = $("#txtIdProdEnComprobanteRP").val();
	if (idProd === "") {
		AbrirMensaje("Atención", "Debe ingresar un producto para agregar.", function () {
			$("#msjModal").modal("hide");
			return true;
		}, false, ["Aceptar"], "warn!", null);
	} else {
		var accion = "";
		var oc_compte = "";
		var bulto = $("#txtBtoEnComprobanteRP").val();
		var up = $("#txtUPEnComprobanteRP").val();
		var unidad = $("#txtUnidEnComprobanteRP").val();
		var up_id = $("#txtUP_ID").val();
		var tco_id = $("#tco_id").val();
		var cm_compte = $("#cm_compte").val();
		var p_desc = $("#txtProDescripcion").val();
		var prov_id = $("#cta_id").val();
		var id_barrado = $("#txtBARRADO_ID").val();
		var id_prod = idProd;
		if (ExistenciaProdEnGrilla(idProd) === false) {
			accion = "0";//Agregar
			AbrirWaiting();
			var data = { oc_compte, id_prod, up, bulto, unidad, accion, tco_id, cm_compte, up_id, p_desc, prov_id, id_barrado };
			PostGenHtml(data, CargarDetalleDeProductosEnRPUrl, function (obj) {
				$("#divDetalleDeProductos").html(obj);
				AgregarHandlerSelectedRow("tbDetalleDeProd");
				CerrarWaiting();
				return true;
			}, function (obj) {
				ControlaMensajeError(obj.message);
				CerrarWaiting();
				return true;
			});
		} else {
			AbrirMensaje("Atención", "Existen productos en el detalle con igual ID, seleccione la opción deseada.", function (e) {
				$("#msjModal").modal("hide");
				switch (e) {
					case "SI": //Reemplazar
						accion = "1";
						break;
					case "SI2": //Acumular
						accion = "2";
						break;
					default: //NO
						accion = "1";
						break;
				}
				AbrirWaiting();
				var data = { oc_compte, id_prod, up, bulto, unidad, accion, tco_id, cm_compte, up_id, p_desc, prov_id, id_barrado };
				PostGenHtml(data, CargarDetalleDeProductosEnRPUrl, function (obj) {
					$("#divDetalleDeProductos").html(obj);
					AgregarHandlerSelectedRow("tbDetalleDeProd");
					CerrarWaiting();
					return true;
				}, function (obj) {
					ControlaMensajeError(obj.message);
					CerrarWaiting();
					return true;
				});
				return true;
			}, true, ["Reemplazar", "Acumular", "Cancelar"], "warn!", null);
		}
	}
}


function CargarDetalleDeProducto() {
	CargarDetalleDeProductosEnRP(0);
}

function selectDetalleDeProdRow(x) {
	if (x) {
		p_id_selected = x.cells[0].innerText.trim();
	}
	else {
		p_id_selected = "";
	}
}

function DelProdEnComprobanteRP() {
	if (p_id_selected != null && p_id_selected !== "") {
		AbrirMensaje("Atención", "Desea quitar el producto seleccionado? Esta acción no se puede revertir.", function (e) {
			$("#msjModal").modal("hide");
			switch (e) {
				case "SI": //Quitar elemento de la grilla, actualizar las variables de sesión y recargar el componente
					QuitarProductoEnDetalle();
					break;
				case "NO":
					break;
				default: //NO
					break;
			}
			return true;
		}, true, ["Aceptar", "Cancelar"], "info!", null);
	}
	else {
		AbrirMensaje("Atención", "Debe seleccionar un producto para quitar de la lista.", function () {
			$("#msjModal").modal("hide");
			return true;
		}, false, ["Aceptar"], "warn!", null);
	}
}

function QuitarProductoEnDetalle() {
	var p_id = p_id_selected;
	datos = { p_id };
	PostGenHtml(datos, quitarItemDeDetalleDeProdURL, function (obj) {
		$("#divDetalleDeProductos").html(obj);
		AgregarHandlerSelectedRow("tbDetalleDeProd");
		p_id_selected = "";
		return true;
	}, function (obj) {
		ControlaMensajeError(obj.message);
		return true;
	});
}

function AceptarDesdeComprobanteRP() {
	GuardarDetalleDeProductos(true);
}

function RegresarDesdeComprobanteRP() {
	//Antes de volver debo validar si hay productos cargados en el detalle, si es así consultar con el operador
	var desdeDetalle = true;
	datos = { desdeDetalle };
	PostGen(datos, verificarDetalleCargadoURL, function (o) {
		if (o.error === true) {
			AbrirMensaje("Atención", o.msg, function () {
				$("#msjModal").modal("hide");
				return true;
			}, false, ["Aceptar"], "error!", null);
		} else if (o.warn === true) {
			AbrirMensaje("Atención", o.msg, function () {
				$("#msjModal").modal("hide");
				return true;
			}, false, ["Aceptar"], "warn!", null);
		} else if (o.vacio === false) {
			AbrirMensaje("Atención", o.msg, function (e) {
				$("#msjModal").modal("hide");
				switch (e) {
					case "SI": //Guardar los cambios
						GuardarDetalleDeProductos(true);
						break;
					case "NO": //Cancelar la pre carga de productos en el detalle del comprobante
						GuardarDetalleDeProductos(false);
						break;
					default: //NO
						break;
				}
				return true;
			}, true, ["Aceptar", "Cancelar"], "info!", null);
		} else {
			debugger;
			var uri = VolverANuevaAutUrl + "?rp=" + $("#Rp").val();
			window.location.href = uri;
		}
	});
};

function GuardarDetalleDeProductos(guardado) {
	AbrirWaiting();
	var generar = false;
	datos = { guardado, generar };
	PostGen(datos, GuardarDetalleDeComprobanteRPUrl, function (o) {
		if (o.error === true) {
			AbrirMensaje("Atención", o.msg, function () {
				$("#msjModal").modal("hide");
				CerrarWaiting();
				return true;
			}, false, ["Aceptar"], "error!", null);
		} else if (o.warn === true) {
			AbrirMensaje("Atención", o.msg, function () {
				$("#msjModal").modal("hide");
				CerrarWaiting();
				return true;
			}, false, ["Aceptar"], "warn!", null);
		} else {
			CerrarWaiting();
			debugger;
			var uri = VolverANuevaAutUrl + "?rp=" + $("#Rp").val();
			window.location.href = uri;
		}
	});
}

function AgregarProdDesdeDetalleDeOC() {
	if ($("#ocCompteSelected").val() !== "") {
		if (ExistenciaProdEnGrilla()) {
			AbrirMensaje("Atención", "Existen productos en el detalle con igual ID, seleccione la opción deseada.", function (e) {
				$("#msjModal").modal("hide");
				switch (e) {
					case "SI": //Reemplazar
						CargarDetalleDeProductosEnRP(1);
						break;
					case "SI2": //Acumular
						CargarDetalleDeProductosEnRP(2);
						break;
					default: //NO
						break;
				}

				return true;
			}, true, ["Reemplazar", "Acumular", "Cancelar"], "warn!", null);
		}
		else {
			CargarDetalleDeProductosEnRP(0);
		}
	}
	else {
		AbrirMensaje("Atención", "Debe seleccionar una OC.", function () {
			$("#msjModal").modal("hide");
			return true;
		}, false, ["Aceptar"], "warn!", null);
		return true;
	}
}

//ValidarExistenciaDeProductoSinOCYaExistente
function ExistenciaProdEnGrilla(id) {
	var ids = [];
	var existe = false;
	if (id === "") {
		//Levanto los id de producto de la OC seleccionada
		$("#tbDetalleDeOC").find('tr').each(function (i, el) {
			var td = $(this).find('td');
			ids.push(td.eq(0).text());
		});
	} else {
		ids.push(id);
	}
	
	//Recorro la tabla de detalle de producto buscando la ocurrencia de los items
	$("#tbDetalleDeProd").find('tr').each(function (i, el) {
		var td = $(this).find('td');
		if (td.length > 0 && ids.find(x => x === td.eq(0).text()) !== undefined) {
			//Existe el producto
			existe = true;
			return false;
		}
	});
	return existe;
}

function CargarOCxCuenta() {
	var cta_id = document.getElementById('cta_id').value;
	var data = { cta_id };
	PostGenHtml(data, CargarOCxCuentaEnRPUrl, function (obj) {
		$("#divOrdenDeCompraXCuenta").html(obj);
		AgregarHandlerSelectedRow("tbOCxCuenta");
		return true;
	}, function (obj) {
		ControlaMensajeError(obj.message);
		return true;
	});
}

function selectOCDetalleRow(x) { }



function AgregarHandlerSelectedRow(grilla) {
	document.getElementById(grilla).addEventListener('click', function (e) {
		if (e.target.nodeName === 'TD') {
			var selectedRow = this.querySelector('.selected-row');
			if (selectedRow) {
				selectedRow.classList.remove('selected-row');
			}
			e.target.closest('tr').classList.add('selected-row');
		}
	});
}

//Valores de parametro:
//0-> Agregar
//1-> Reemplazar
//2-> Acumular
function CargarDetalleDeProductosEnRP(accion) {
	AbrirWaiting();
	var oc_compte = $("#ocCompteSelected").val();
	var id_prod = $("txtIdProdEnComprobanteRP").val();
	var up = $("#txtUPEnComprobanteRP").val();
	var bulto = $("#txtBtoEnComprobanteRP").val();
	var unidad = $("#txtUnidEnComprobanteRP").val();
	var tco_id = $("#tco_id").val();
	var cm_compte = $("#cm_compte").val();
	var data = { oc_compte, id_prod, up, bulto, unidad, accion, tco_id, cm_compte };
	PostGenHtml(data, CargarDetalleDeProductosEnRPUrl, function (obj) {
		$("#divDetalleDeProductos").html(obj);
		AgregarHandlerSelectedRow("tbDetalleDeProd");
		CerrarWaiting();
		return true;
	}, function (obj) {
		ControlaMensajeError(obj.message);
		CerrarWaiting();
		return true;
	});
}

function selectOCRow(x) {
	$("#ocCompteSelected").val(x.cells[0].innerText.trim());
	var oc_compte = x.cells[0].innerText.trim();
	var data = { oc_compte };
	PostGenHtml(data, VerDetalleDeOCRPUrl, function (obj) {
		$("#divDetalleDeOrdenDeCompra").html(obj);
		document.getElementById("leyendDetalleOC").outerHTML = "<h5 id=\"leyendDetalleOC\"> Detalle de OC " + oc_compte + "</h5>";
		return true;
	}, function (obj) {
		ControlaMensajeError(obj.message);
		return true;
	});
}