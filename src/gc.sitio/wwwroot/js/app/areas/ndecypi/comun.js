$(function () {
	$("#btnCollapseSection").on("click", btnCollapseSectionClicked);
	$("#btnCancel").on("click", function () {
		LimpiarDatosDelFiltroInicial();
		$("#btnFiltro").trigger("click");
	});
	$("input#Rel03").on("click", function () {
		$("input#Rel03").val("");
		$("#Rel03Item").val("");
	});
	AddEventListenerToGrid("tbListaProducto");
	$("#tbListaProducto").on('input', 'td[contenteditable]', function () {
	});
	$("#btnBuscar").on("click", function () {
		dataBak = "";
		pagina = 1;
		BuscarProductos(pagina);
	});
	$(document).on("change", "#listaSucursales", ControlaSucursalSeleccionada);
	InicializaPantallaNC();
	funcCallBack = BuscarProductos;
	return true;
});

function ControlaSucursalSeleccionada() {
	BuscarInfoAdicional();
}

function LimpiarDatosDelFiltroInicial() {
	$("input#Rel01").val("");
	$("#Rel01Item").val("");
	$("#Rel01List").empty();
	$("#chkRel01").prop('checked', false);
	$("#chkRel01").trigger("change");
	$("input#Rel01").prop('disabled', true);
	$("#Rel01List").prop('disabled', true);

	$("input#Rel03").val("");
	$("#Rel03Item").val("");
	$("#Rel03List").empty();
	$("#chkRel03").prop('checked', false);
	$("#chkRel03").trigger("change");
	$("input#Rel03").prop('disabled', true);
	$("#Rel03List").prop('disabled', true);

	$("input#Rel02").val("");
	$("#Rel02Item").val("");
	$("#Rel02List").empty();
	$("#chkRel02").prop('checked', false);
	$("#chkRel02").trigger("change");
	$("input#Rel02").prop('disabled', true);
	$("#Rel02List").prop('disabled', true);

	$("#chk01").prop('checked', false);
	$("#chk01").trigger("change");
	$("#chk02").prop('checked', false);
	$("#chk02").trigger("change");
	$("#chk03").prop('checked', false);
	$("#chk03").trigger("change");
	$("#chk04").prop('checked', false);
	$("#chk04").trigger("change");
	$("#chk05").prop('checked', false);
	$("#chk05").trigger("change");

	$("#chkDescr").prop('checked', false);
	$("#chkDescr").trigger("change");
	$("input#Buscar").val("");
	$("input#Buscar").prop('disabled', true);

	$("#chkDesdeHasta").prop('checked', false);
	$("#chkDesdeHasta").trigger("change");
	$("input#Id").val("");
	$("input#Id").prop('disabled', true);
	$("input#Id2").val("");
	$("input#Id2").prop('disabled', true);
}

function NoHayProdSeleccionado() {
	if (pIdSeleccionado == undefined || pIdSeleccionado == "") {
		return true;
	}
	return false;
}

function changeProductosDelMismoProveedor(x) {
	if (NoHayProdSeleccionado()) {
		AbrirMensaje("Atención", "Debe seleccionar un producto.", function () {
			$("#msjModal").modal("hide");
			return true;
		}, false, ["Aceptar"], "error!", null);
	}
	AbrirWaiting();
	var pId = pIdSeleccionado;
	var tipo = tipoDeOperacion;
	var soloProv = $("#chkProductosDelMismoProveedor")[0].checked;
	datos = { pId, tipo, soloProv }
	PostGenHtml(datos, BuscarInfoProdSustitutoURL, function (obj) {
		$("#divInfoProdSustituto").html(obj);
		AddEventListenerToGrid("tbListaProductoSust");
		CerrarWaiting();
		return true
	});
}

function addTxtMesesKeyUpHandler() {
	$("#txtMeses").on('keyup', function (e) {
		if (e.keyCode == 13) {
			BuscarInfoAdicional();
		}
	});
}

function addTxtSemanasKeyUpHandler() {
	$("#txtSemanas").on('keyup', function (e) {
		if (e.keyCode == 13) {
			BuscarInfoAdicional();
		}
	});
}

function BuscarInfoAdicional() {
	if (NoHayProdSeleccionado()) {
		AbrirMensaje("Atención", "Debe seleccionar un producto.", function () {
			$("#msjModal").modal("hide");
			return true;
		}, false, ["Aceptar"], "error!", null);
	}
	AbrirWaiting();
	var admId = $("#listaSucursales").val();
	var meses = $("#txtMeses").val();
	var semanas = $("#txtSemanas").val();
	var pId = pIdSeleccionado;
	var datos = { pId, admId, meses };
	PostGenHtml(datos, BuscarInfoProdIExMesesURL, function (obj) {
		$("#divInfoProdIExMeses").html(obj);
		AddEventListenerToGrid("tbInfoProdIExMes");
		CerrarWaiting();
		return true
	});
	datos = { pId, admId, semanas };
	PostGenHtml(datos, BuscarInfoProdIExSemanasURL, function (obj) {
		$("#divInfoProdIExSemanas").html(obj);
		AddEventListenerToGrid("tbInfoProdIExSemana");
		CerrarWaiting();
		return true
	});
	datos = { pId, admId };
	PostGenHtml(datos, BuscarInfoProdStkDepositoURL, function (obj) {
		$("#divInfoProductoStkD").html(obj);
		AddEventListenerToGrid("tbInfoProdStkD");
		CerrarWaiting();
		return true
	});
	PostGenHtml(datos, BuscarInfoProdStkSucursalURL, function (obj) {
		$("#divInfoProductoStkA").html(obj);
		AddEventListenerToGrid("tbInfoProdStkA");
		CerrarWaiting();
		return true
	});
	var tipo = tipoDeOperacion;
	var soloProv = true; //Valor por default
	datos = { pId, tipo, soloProv }
	PostGenHtml(datos, BuscarInfoProdSustitutoURL, function (obj) {
		$("#divInfoProdSustituto").html(obj);
		AddEventListenerToGrid("tbListaProductoSust");
		CerrarWaiting();
		return true
	});
	datos = { pId }
	PostGenHtml(datos, BuscarInfoProdURL, function (obj) {
		$("#divInfoProducto").html(obj);
		AddEventListenerToGrid("tbInfoProducto");
		CerrarWaiting();
		return true
	});
}

function InicializaPantallaNC() {
	var tb = $("#tbListaProducto tbody tr");
	if (tb.length === 0) {
		$("#divFiltro").collapse("show")
	}

	$("#lbRel01").text("Proveedor");
	$("#lbRel02").text("Rubro");
	$("#lbRel03").text("Familia");
	$("#lbChkDescr").text("Descripción Producto");
	$("#lbDescr").html("Desc");

	$("#lbchk01").text("Alta Rotación");
	$("#lbchk02").text("Con PI");
	$("#lbchk03").text("Con OC");
	$("#lbchk04").text("Sin Stk");
	$("#lbchk05").text("Con Stk a Vencer");

	$("#lbChkDesdeHasta").text("ID Producto");

	$(".activable").prop("disabled", true);
	$("#chkRel03").prop("disabled", true);
	
	CerrarWaiting();
	return true;
}

$("#Rel01List").on("dblclick", 'option', function () {
	$(this).remove();
	if ($("#Rel01List")[0].length === 1) {
		$("#chkRel03").prop("disabled", false);
		CargarFamiliaLista($("#Rel01List")[0][0].value);
	}
	else {
		$("#chkRel03").prop("disabled", true);
	}
})

$("#Rel01").autocomplete({
	source: function (request, response) {

		data = { prefix: request.term }; Rel01

		$.ajax({
			url: autoComRel01Url,
			type: "POST",
			dataType: "json",
			data: data,
			success: function (obj) {
				response($.map(obj, function (item) {
					var texto = item.descripcion;
					return { label: texto, value: item.descripcion, id: item.id, prov: item.provId };
				}));
			}
		})
	},
	minLength: 3,
	select: function (event, ui) {
		if ($("#Rel01List").has('option:contains("' + ui.item.id + '")').length === 0) {
			$("#Rel01Item").val(ui.item.id);
			var opc = "<option value=" + ui.item.id + ">" + ui.item.value + "</option>"
			$("#Rel01List").append(opc);
		}
		if ($("#Rel01List")[0].length === 1) {
			$("#chkRel03").prop("disabled", false);
			CargarFamiliaLista(ui.item.id);
		}
		else {
			$("#chkRel03").prop("disabled", true);
			$("#Rel03").prop("disabled", true).val("");
			$("#Rel03List").prop("disabled", true).empty();
			$("#chkRel03")[0].checked = false;
		}

		return true;
	}
});

//codigo generico para autocomplete 03
$("#Rel03").autocomplete({
	source: function (request, response) {

		data = { prefix: request.term }; Rel03

		$.ajax({
			url: autoComRel03Url,
			type: "POST",
			dataType: "json",
			data: data,
			success: function (obj) {
				response($.map(obj, function (item) {
					var texto = item.descripcion;
					return { label: texto, value: item.descripcion, id: item.id, prov: item.provId };
				}));
			}
		})
	},
	minLength: 3,
	select: function (event, ui) {
		if ($("#Rel03List").has('option:contains("' + ui.item.id + '")').length === 0) {
			$("#Rel03Item").val(ui.item.id);
			var opc = "<option value=" + ui.item.id + ">" + ui.item.value + "</option>"
			$("#Rel03List").append(opc);
		}
		return true;
	}
});

function CargarFamiliaLista(id) {
	var ctaId = id;
	data = { ctaId };
	PostGen(data, buscarFamiliaDesdeProveedorSeleccionadoUrl, function (obj) {
		if (obj.error === true) {
			AbrirMensaje("ATENCIÓN", obj.msg, function () {
				$("#msjModal").modal("hide");
				return true;
			}, false, ["Aceptar"], "error!", null);
		}
		else {
			
		}
	});
}

var tipoBusqueda = "";
var viendeDesdeBusquedaDeProducto = false;
const FuncionSobreBusquedaDeProductos = {
	PROVEEDORES: 'PROVEEDORES',
	PROVEEDORESYFAMILIA: 'PROVEEDORESYFAMILIA',
	RUBROS: 'RUBROS',
	SINSTOCK: 'SINSTOCK',
	CONSTOCKAVENCER: 'CONSTOCKAVENCER',
	ALTAROTACION: 'ALTAROTACION',
	CONPI: 'CONPI',
	CONOC: 'CONOC'
}

const colBulto = 10;
const colCantidad = 11;
const colCosto = 12;
const colCostoTotal = 13;
const colPallet = 14;
function BuscarProductos(pag = 1) {
	viendeDesdeBusquedaDeProducto = true;
	AbrirWaiting();
	var Tipo = tipoDeOperacion;
	var Buscar = $("#Buscar").val();
	var Id = $("#Id").val();
	var Id2 = $("#Id2").val();
	var Rel01 = [];
	var Rel02 = [];
	var Rel03 = [];
	$("#Rel01List").children().each(function (i, item) { Rel01.push($(item).val()) });
	$("#Rel02List").children().each(function (i, item) { Rel02.push($(item).val()) });
	$("#Rel03List").children().each(function (i, item) {
		var aux = { Id: $(item).val(), Descripcion: $(item).text() };
		Rel03.push(aux);
	});

	var Opt1 = $("#chk01")[0].checked
	var Opt2 = $("#chk02")[0].checked
	var Opt3 = $("#chk03")[0].checked
	var Opt4 = $("#chk04")[0].checked
	var Opt5 = $("#chk05")[0].checked

	var buscaNew = true;
	pagina = pag;
	Pagina = pag;
	var sort = null;
	var sortDir = null
	var data2 = { sort, sortDir, Pagina, buscaNew }
	var data1 = { Tipo, Buscar, Id, Id2, Rel01, Rel02, Rel03, Opt1, Opt2, Opt3, Opt4, Opt5 };
	var data = $.extend({}, data1, data2);
	PostGenHtml(data, BuscarProductosOCPI2URL, function (obj) {
		$("#divListaProducto").html(obj);
		$("#divDetalle").collapse("show");
		addInCellEditHandler();
		addInCellLostFocusHandler();
		addInCellKeyDownHandler();
		AddEventListenerToGrid("tbListaProducto");
		tableUpDownArrow();
		LimpiarDatosDelFiltroInicial();
		PostGen({}, buscarMetadataURL, function (obj) {
			if (obj.error === true) {
				AbrirMensaje("ATENCIÓN", obj.msg, function () {
					$("#msjModal").modal("hide");
					return true;
				}, false, ["Aceptar"], "error!", null);
			}
			else {
				totalRegs = obj.metadata.totalCount;
				pags = obj.metadata.totalPages;
				pagRegs = obj.metadata.pageSize;

				$("#pagEstado").val(true).trigger("change");
			}

		});
		CerrarWaiting();
		viendeDesdeBusquedaDeProducto = false;
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

function selectListaProductoRow(x) {
	if (x) {
		pIdSeleccionado = x.cells[0].innerText.trim();
		BuscarInfoAdicional();
	}
	else {
		pIdSeleccionado = "";
	}
}

function SeleccionarDesdeFila(index, ctrol) {
	if (index === "")
		return;
	if (ctrol === undefined)
		return;
	for (var i = 0; i < ctrol.options.length; i++) {
		if (ctrol.options[i].value == index) {
			ctrol.options[i].selected = true;
			return;
		}
	}
}

function addInCellEditHandler() {
	$("#tbListaProducto").on('input', 'td[contenteditable]', function (e) {
		pedido = e.currentTarget.innerText;
		pIdEditado = e.currentTarget.parentNode.cells[0].innerText;
		rowIndex = e.currentTarget.parentNode.rowIndex;
	});
}

function addInCellLostFocusHandler() {
	$("#tbListaProducto").on('focusout', 'td[contenteditable]', function (e) {
		pedido = e.currentTarget.innerText;
		pIdEditado = e.currentTarget.parentNode.cells[0].innerText;
		rowIndex = e.currentTarget.parentNode.rowIndex;
		if (pedido !== "0")
			actualizarPedidoBulto();
	});
}

function addInCellKeyUpHandler() {
	$("#tbListaProducto").on('keyup', 'td[contenteditable]', function (e) {
		if (e.keyCode == 13) {
			return false;
		}
	});
}

function addInCellKeyDownHandler() {
	$("#tbListaProducto").on('keydown', 'td[contenteditable]', function (e) {
		if (isNaN(String.fromCharCode(e.which)) && !(e.which >= 96 && e.which <= 105) && !(e.shiftKey && (e.which == 37 || e.which == 39))) e.preventDefault();
	});
}

//MARCE TODO: Llevar este metodo a OC
function tableUpDownArrow() {
	var table = document.querySelector('#tbListaProducto tbody');
	if (table == undefined)
		return;
	if (table.rows[0] == undefined)
		return;
	const myTable = table
		, nbRows = myTable.rows.length
		, nbCells = myTable.rows[0].cells.length
		, movKey = {
			ArrowUp: p => { p.r = (--p.r + nbRows) % nbRows }
			, ArrowLeft: p => { p.c = (--p.c + nbCells) % nbCells }
			, ArrowDown: p => {
				p.r = ++p.r % nbRows
			}
			, ArrowRight: p => { p.c = ++p.c % nbCells }
			, Tab: p => {
				p.r = ++p.r % nbRows
			}
		}

	myTable
		.querySelectorAll('input, [contenteditable=true]')
		.forEach(elm => {
			elm.onfocus = e => {
				let sPos = myTable.querySelector('.selected-row')
					, tdPos = elm.parentNode

				if (sPos) sPos.classList.remove('selected-row')

				tdPos.classList.add('selected-row')
			}
		})


	document.onkeydown = e => {
		let sPos = myTable.querySelector('.selected-row')
			, evt = (e == null ? event : e)
			, pos = {
				r: sPos ? sPos.rowIndex : -1
				, c: sPos ? (sPos.cellIndex ? sPos.cellIndex : 10) : -1
			}

		if (sPos &&
			(evt.altKey && evt.shiftKey && movKey[evt.code])
			||
			(evt.ctrlKey && movKey[evt.code])
			||
			evt.code === 'Tab') {
			let loop = true
				, nxFocus = null
				, cell = null
			do {
				if (evt.code === 'ArrowDown' && pos.r == nbRows)
					pos.r = 0;
				if (evt.code === 'Tab' && evt.shiftKey && pos.r == 1)
					pos.r = nbRows;
				if (evt.code === 'Tab' && evt.shiftKey) {
					movKey['ArrowUp'](pos)
				}
				else
					movKey[evt.code](pos);
				cell = myTable.rows[pos.r].cells[pos.c]
				if (pos.r == 0)
					pos.r = nbRows;
				else if (pos.r == nbRows)
					pos.r = 2;
				nxFocus = myTable.rows[pos.r - 1].cells[pos.c]

				if (nxFocus
					&& cell.style.display !== 'none'
					&& cell.parentNode.style.display !== 'none') {
					nxFocus.focus();
					nxFocus.closest('tr').classList.add('selected-row');
					nxFocus.focus();
					loop = false
				}
			}
			while (loop)
			actualizarPedidoBulto();
			if (evt.code === 'Tab') {
				event.preventDefault();
			}
		}
		else if (evt.code === 'Enter')
			event.preventDefault();
		else if (evt.code === 'NumpadEnter')
			event.preventDefault();
		else
			console.log(evt.code);
	}
}

function focusOnTdPedido(x) {
	var cell = x;
	var range, selection;
	if (document.body.createTextRange) {
		range = document.body.createTextRange();
		range.moveToElementText(cell);
		range.select();
	} else if (window.getSelection) {
		selection = window.getSelection();
		range = document.createRange();
		range.selectNodeContents(cell);
		selection.removeAllRanges();
		selection.addRange(range);
	}
}

function actualizarPedidoBulto() {
	if (!pedido)
		return false;
	AbrirWaiting();
	var tipo = tipoDeOperacion;
	var pId = pIdEditado;
	var tipoCarga = "M";
	var bultos = pedido;
	var datos = { tipo, pId, tipoCarga, bultos }
	PostGen(datos, CargaPedidoOCPIURL, function (o) {
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
			CerrarWaiting();
			tabla = myTable = document.querySelector('#tbListaProducto tbody');
			if (o.cantidad != 0) {
				tabla.rows[rowIndex - 1].cells[colCantidad].innerText = o.cantidad;
				tabla.rows[rowIndex - 1].cells[colCantidad].style.backgroundColor = "lightgreen";
				tabla.rows[rowIndex - 1].cells[colCosto].innerText = (Math.round(o.pCosto * 100) / 100).toFixed(3);
				tabla.rows[rowIndex - 1].cells[colCosto].style.backgroundColor = "lightgreen";
				tabla.rows[rowIndex - 1].cells[colCostoTotal].innerText = (Math.round(o.pCostoTotal * 100) / 100).toFixed(3);
				tabla.rows[rowIndex - 1].cells[colCostoTotal].style.backgroundColor = "lightgreen";
				tabla.rows[rowIndex - 1].cells[colPallet].innerText = (Math.round(o.pallet * 100) / 100).toFixed(2) ;
				tabla.rows[rowIndex - 1].cells[colBulto].style.backgroundColor = "lightgreen";
			}
			else {
				tabla.rows[rowIndex - 1].cells[colCantidad].innerText = o.cantidad;
				tabla.rows[rowIndex - 1].cells[colCantidad].style.backgroundColor = "";
				tabla.rows[rowIndex - 1].cells[colCosto].innerText = (Math.round(o.pCosto * 100) / 100).toFixed(3);
				tabla.rows[rowIndex - 1].cells[colCosto].style.backgroundColor = "";
				tabla.rows[rowIndex - 1].cells[colCostoTotal].innerText = (Math.round(o.pCostoTotal * 100) / 100).toFixed(3);
				tabla.rows[rowIndex - 1].cells[colCostoTotal].style.backgroundColor = "";
				tabla.rows[rowIndex - 1].cells[colPallet].innerText = (Math.round(o.pallet * 100) / 100).toFixed(2);
				tabla.rows[rowIndex - 1].cells[colBulto].style.backgroundColor = "";
			}
			return false;
		}
	});
}

function verificaEstado(e) {
	FunctionCallback = null; //inicializo funcion por si tiene alguna funcionalidad asignada.
	var res = $("#estadoFuncion").val();
	CerrarWaiting();
	if (res === "true") {
		var prod = productoBase;
		if (prod) { //Producto existe
		}
	}
	return true;
}


function SelecccionarSucursal() {
	$("#listaSucursales").val()
	if ($("#listaSucursales").val() == "") {
		$("#listaSucursales").prop("selectedIndex", 1).change();
	}
}

function btnCollapseSectionClicked() {
	if ($("#containerListaProducto").hasClass('table-wrapper-full-width')) {
		$("#containerListaProducto").removeClass('table-wrapper-full-width');
		$("#containerListaProducto").addClass('table-wrapper-300-full-width');
	} else {
		$("#containerListaProducto").removeClass('table-wrapper-300-full-width');
		$("#containerListaProducto").addClass('table-wrapper-full-width');
	}
}

