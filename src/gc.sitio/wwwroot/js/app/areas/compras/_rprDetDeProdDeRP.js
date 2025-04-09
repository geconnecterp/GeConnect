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

	function moveColumn(table, sourceIndex, targetIndex) {
		console.log("Move Col " + sourceIndex + " to " + targetIndex);
		var body = $("tbody", table);
		$("tr", body).each(function (i, row) {
			$("td", row).eq(sourceIndex).insertAfter($("td", row).eq(targetIndex - 1));
		});
	}

	$(".mytable > thead > tr").sortable({
		items: "> th.sortme",
		start: function (event, ui) {
			ui.item.data("source", ui.item.index());
		},
		update: function (event, ui) {
			moveColumn($(this).closest("table"), ui.item.data("source"), ui.item.index());
			$(".mytable > tbody").sortable("refresh");
		}
	});

	$(".mytable > tbody").sortable({
		items: "> tr.sortme"
	});
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
				LimpiarCamposDeCarga(true);
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
					LimpiarCamposDeCarga(true);
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

function LimpiarCamposDeCarga(seteaFoco) {
	$("#txtIdProdEnComprobanteRP").val("");
	$("#txtProDescripcion").val("");
	$("#txtUPEnComprobanteRP").val("");
	$("#txtBtoEnComprobanteRP").val("");
	$("#txtUnidEnComprobanteRP").val("");
	if (seteaFoco) {
		$("#Busqueda").focus();
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
			var uri = VolverANuevaAutUrl + "?rp=" + $("#Rp").val();
			window.location.href = uri;
		}
	});
};

function GuardarDetalleDeProductos(guardado) {
	AbrirWaiting();
	var listaProd = "";
	$("#tbDetalleDeProd").find('tr').each(function (i, el) {
		var td = $(this).find('td');
		if (td.length > 0 && td[8].innerText !== undefined) {
			if (listaProd.length == 0)
				listaProd = td[0].innerText + "@" + td[8].innerText;
			else {
				listaProd = listaProd + "#" + td[0].innerText + "@" + td[8].innerText;
				//return false;
			}
		}
	});
	var generar = false;
	datos = { guardado, generar, listaProd };
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
			var uri = VolverANuevaAutUrl + "?rp=" + $("#Rp").val();
			window.location.href = uri;
		}
	});
}

function AgregarProdDesdeDetalleDeOC() {
	if ($("#ocCompteSelected").val() !== "") {
		if (ExistenciaProdEnGrilla("")) {
			AbrirMensaje("Atención", "Existen productos en el detalle con igual ID, seleccione la opción deseada.", function (e) {
				$("#msjModal").modal("hide");
				switch (e) {
					case "SI": //Reemplazar
						CargarDetalleDeProductosEnRP(1);
						DesmarcarItemsDeOC();
						break;
					case "SI2": //Acumular
						CargarDetalleDeProductosEnRP(2);
						DesmarcarItemsDeOC();
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
			if (td.eq(0)[0]) {
				if (td.eq(0)[0].children[0].checked)
					ids.push(td.eq(1).text());
			}
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
	//AbrirWaiting();
	var oc_compte = $("#ocCompteSelected").val();
	var id_prod = $("txtIdProdEnComprobanteRP").val();
	var up = $("#txtUPEnComprobanteRP").val();
	var bulto = $("#txtBtoEnComprobanteRP").val();
	var unidad = $("#txtUnidEnComprobanteRP").val();
	var tco_id = $("#tco_id").val();
	var cm_compte = $("#cm_compte").val();
	var listaProd = "";

	if (oc_compte !== undefined && oc_compte !== "") {
		$("#tbDetalleDeOC").find('tr').each(function (i, el) {
			var td = $(this).find('td');
			if (td.eq(0)[0]) {
				if (td.eq(0)[0].children[0].checked) {
					if (listaProd === "") {
						listaProd = td.eq(1).text();
					}
					else {
						listaProd = listaProd + "#" + td.eq(1).text(); 
					}
				}
			}
		});

		if (listaProd.length == 0) {
			AbrirMensaje("Atención", "Debe seleccionar al menos un producto de la OC.", function () {
				$("#msjModal").modal("hide");
				return true;
			}, false, ["Aceptar"], "warn!", null);
			return false;
		}
	}
	AbrirWaiting();
	var data = { oc_compte, id_prod, up, bulto, unidad, accion, tco_id, cm_compte, listaProd };
	PostGenHtml(data, CargarDetalleDeProductosEnRPUrl, function (obj) {
		$("#divDetalleDeProductos").html(obj);
		AgregarHandlerSelectedRow("tbDetalleDeProd");
		temporal();
		CerrarWaiting();
		return true;
	}, function (obj) {
		ControlaMensajeError(obj.message);
		CerrarWaiting();
		return true;
	});
}

function moveColumn(table, sourceIndex, targetIndex) {
	console.log("Move Col " + sourceIndex + " to " + targetIndex);
	var body = $("tbody", table);
	$("tr", body).each(function (i, row) {
		$("td", row).eq(sourceIndex).insertAfter($("td", row).eq(targetIndex - 1));
	});
}

function temporal() {
	$(".drageable-table > thead > tr").sortable({
		items: "> th.sortme",
		start: function (event, ui) {
			ui.item.data("source", ui.item.index());
		},
		update: function (event, ui) {
			moveColumn($(this).closest("table"), ui.item.data("source"), ui.item.index());
			$(".drageable-table > tbody").sortable("refresh");
		}
	});

	$(".drageable-table > tbody").sortable({
		items: "> tr.sortme"
	});

	$(document).on("mouseup", "#tbDetalleDeProd tbody tr", function (e) {
		setTimeout(() => {
			RecalcularItemValue()
		}, 500);
	});
}

function RecalcularItemValue() {
	AbrirWaiting();
	var index = 1;
	$("#tbDetalleDeProd").find('tr').each(function (i, el) {
		var td = $(this).find('td');
		if (td.length > 0 && td[8].innerText !== undefined) {
			td[8].innerText = index.toString();
			index++;
		}
	});
	CerrarWaiting();
}

//let items = document.querySelectorAll('.drageable-table > tbody > tr');
//items.forEach(function (item) {
//	item.addEventListener('dragstart', handleDragStart);
//	item.addEventListener('dragend', handleDragEnd);
//});

//function handleDragEnd(e) {
//	console.log("handleDragEnd");
//}

///Reacomodar los valores de la columna Item, luego de arrastra y soltar elementos de la grilla.

function selectOCRow(x) {
	$("#ocCompteSelected").val(x.cells[0].innerText.trim());
	var oc_compte = x.cells[0].innerText.trim();
	var data = { oc_compte };
	PostGenHtml(data, VerDetalleDeOCRPUrl, function (obj) {
		$("#divDetalleDeOrdenDeCompra").html(obj);
		document.getElementById("leyendDetalleOC").outerHTML = "<h5 id=\"leyendDetalleOC\" style=\"margin-bottom: 0; margin-top: 10px;\"> Detalle de OC " + oc_compte + "</h5>";
		AgregarHandlerAGrillaDetalleDeOC();
		return true;
	}, function (obj) {
		ControlaMensajeError(obj.message);
		return true;
	});
}

function AgregarHandlerAGrillaDetalleDeOC() {
	var dataTable = document.getElementById('tbDetalleDeOC');
	var checkItAll = dataTable.querySelector('input[name="select_all"]');
	var inputs = dataTable.querySelectorAll('tbody>tr>td>input');

	checkItAll.addEventListener('change', function () {
		if (checkItAll.checked) {
			inputs.forEach(function (input) {
				input.checked = true;
			});
		}
		else {
			inputs.forEach(function (input) {
				input.checked = false;
			});
		}
	});
}

function DesmarcarItemsDeOC() {
	var dataTable = document.getElementById('tbDetalleDeOC');
	var checkItAll = dataTable.querySelector('input[name="select_all"]');
	checkItAll.checked = false;
	let event = new Event('change');
	checkItAll.dispatchEvent(event);
}