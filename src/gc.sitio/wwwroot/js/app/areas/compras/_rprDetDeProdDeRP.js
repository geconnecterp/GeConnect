$(function () {
	$("#btnAddOCProdEnComprobanteRP").on("click", AgregarProdDesdeDetalleDeOC);
	CargarDetalleDeProductosEnRP();
	CargarOCxCuenta();
});

function AgregarProdDesdeDetalleDeOC() {
	if ($("#ocCompteSelected").val() !== "") {
		if (ExistenciaProdEnGrilla()) {
			AbrirMensaje("Atención", "Existen productos en el detalle con igual ID, seleccione la opción deseada.", function (e) {
				$("#msjModal").modal("hide");
				switch (e) {
					case "SI": //Reemplazar
						break;
					case "SI2": //Acumular
						break;
					default: //NO
						break;
				}

				return true;
			}, true, ["Reemplazar", "Acumular", "Cancelar"], "warn!", null);
		}
		if (!ExisteProdDeOCSele($("#ocCompteSelected").val())) {
			CargarDetalleDeProductosEnRP();
		}
		else {
			AbrirMensaje("Atención", "Debe seleccionar una OC.", function () {
				$("#msjModal").modal("hide");
				return true;
			}, false, ["Aceptar"], "warn!", null);
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

//ValidarExistenciaDeProductosDeOCSeleccionada
function ExisteProdDeOCSele(oc_compte) {
	$("#tbDetalleDeProd").find('tr').each(function (i, el) {
		var td = $(this).find('td');
		if (td.eq(3).text() === oc_compte) {
			return true;
		}
	});
	return false;
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

function CargarDetalleDeProductosEnRP() {
	var oc_compte = $("#ocCompteSelected").val();
	var id_prod = $("txtIdProdEnComprobanteRP").val();
	var up = $("#txtUPEnComprobanteRP").val();
	var bulto = $("#txtBtoEnComprobanteRP").val();
	var unidad = $("#txtUnidEnComprobanteRP").val();
	var data = { oc_compte, id_prod, up, bulto, unidad };
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