$(function () {
	$("#btnAddOCProdEnComprobanteRP").on("click", AgregarProdDesdeDetalleDeOC);
	CargarOCxCuenta();
	$("#btnRegresarDesdeComprobanteRP").on("click", RegresarDesdeComprobanteRP);
	CargarDetalleDeProducto();
});

function CargarDetalleDeProducto() {
	console.log("cta_id: " + $("#cta_id").val());
	console.log("tco_id: " + $("#tco_id").val());
	console.log("cm_compte: " + $("#cm_compte").val());
	CargarDetalleDeProductosEnRP(0);
}

function RegresarDesdeComprobanteRP() {
	//Antes de volver debo validar si hay productos cargados en el detalle, si es así consultar con el operador
	datos = {};
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
					//TODO
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
			window.location.href = VolverANuevaAutUrl;
		}
	});
};

function GuardarDetalleDeProductos(guardado) {
	datos = { guardado };
	PostGen(datos, GuardarDetalleDeComprobanteRPUrl, function (o) {
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
		} else {
			window.location.href = VolverANuevaAutUrl;
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
function ExistenciaProdEnGrilla() {
	var ids = [];
	var existe = false;
	//Levanto los id de producto de la OC seleccionada
	$("#tbDetalleDeOC").find('tr').each(function (i, el) {
		var td = $(this).find('td');
		ids.push(td.eq(0).text());
	});
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

function selectDetalleDeProdRow(x) { }

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
		return true;
	}, function (obj) {
		ControlaMensajeError(obj.message);
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