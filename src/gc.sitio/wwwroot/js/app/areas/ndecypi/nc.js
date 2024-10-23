﻿$(function () {
	$("#btnradioPorProveedor").on("click", buscarPorProveedor);
	$("#btnradioSinStk").on("click", buscarSinStk);
	$("#btnradioConPI").on("click", buscarConPI);
	$("#btnradioPorProveedorFamilia").on("click", buscarPorProveedorFamilia);
	$("#btnradioConStkAVencer").on("click", buscarConStkAVencer);
	$("#btnradioConOC").on("click", buscarConOC);
	$("#btnradioPorRubro").on("click", buscarPorRubro);
	$("#btnradioAltaRotacion").on("click", buscarAltaRotacion);
	DisableComboProveedoresFamilia(true);
	DisableComboRubros(true);
	buscarPorProveedor();
	AddEventListenerToGrid("tbListaProducto");
	$("#tbListaProducto").on('input', 'td[contenteditable]', function () {
		console.log("its cool.");
	});
	//test2();
});

var tipoBusqueda = "";
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

const colCantidad = 11;
const colCosto = 12;
const colCostoTotal = 13;
const colPallet = 14;

function tdChange(x) {
	console.log(x);
}


function buscarPorProveedor() {
	if ($("#listaProveedor").val() == "") {
		$("#listaProveedor").prop("selectedIndex", 1).change();
	}
	DisableComboProveedoresFamilia(true);
	DisableComboRubros(true);
	DisableComboProveedores(false);
	tipoBusqueda = FuncionSobreBusquedaDeProductos.PROVEEDORES;
	BuscarProductos(tipoBusqueda);
}

function buscarSinStk() {
	DisableComboProveedoresFamilia(true);
	DisableComboRubros(true);
	DisableComboProveedores(true);
	tipoBusqueda = FuncionSobreBusquedaDeProductos.SINSTOCK;
	BuscarProductos(tipoBusqueda);
}

function buscarConPI() {
	DisableComboProveedoresFamilia(true);
	DisableComboRubros(true);
	DisableComboProveedores(true);
	tipoBusqueda = FuncionSobreBusquedaDeProductos.CONPI;
	BuscarProductos(tipoBusqueda);
}

function buscarPorProveedorFamilia() {
	if ($("#listaProveedor").val() == "") {
		$("#listaProveedor").prop("selectedIndex", 1).change();
	}
	if ($("#listaFamiliaProveedor").val() == "") {
		$("#listaFamiliaProveedor").prop("selectedIndex", 1).change();
	}
	DisableComboProveedoresFamilia(false);
	DisableComboProveedores(true);
	DisableComboRubros(true);
	tipoBusqueda = FuncionSobreBusquedaDeProductos.PROVEEDORESYFAMILIA;
	BuscarProductos(tipoBusqueda);
}

function buscarConStkAVencer() {
	DisableComboProveedoresFamilia(true);
	DisableComboRubros(true);
	DisableComboProveedores(true);
	tipoBusqueda = FuncionSobreBusquedaDeProductos.CONSTOCKAVENCER;
	BuscarProductos(tipoBusqueda);
}

function buscarConOC() {
	DisableComboProveedoresFamilia(true);
	DisableComboRubros(true);
	DisableComboProveedores(true);
	tipoBusqueda = FuncionSobreBusquedaDeProductos.CONOC;
	BuscarProductos(tipoBusqueda);
}

function buscarPorRubro() {
	if ($("#listaRubro").val() == "") {
		$("#listaRubro").prop("selectedIndex", 1).change();
	}
	DisableComboRubros(false);
	DisableComboProveedores(true);
	DisableComboProveedoresFamilia(true);
	tipoBusqueda = FuncionSobreBusquedaDeProductos.RUBROS;
	BuscarProductos(tipoBusqueda);
}

function buscarAltaRotacion() {
	DisableComboProveedoresFamilia(true);
	DisableComboRubros(true);
	DisableComboProveedores(true);
	tipoBusqueda = FuncionSobreBusquedaDeProductos.ALTAROTACION
	BuscarProductos(tipoBusqueda);
}

function BuscarProductos(tipoBusqueda) {
	AbrirWaiting();
	var filtro = "";
	var id = "";
	var tipo = "OC";

	switch (tipoBusqueda) {
		case FuncionSobreBusquedaDeProductos.PROVEEDORES:
			filtro = "C";
			id = $("#listaProveedor").val();
			break;
		case FuncionSobreBusquedaDeProductos.PROVEEDORESYFAMILIA:
			filtro = "F";
			id = $("#listaProveedor").val() + $("#listaFamiliaProveedor").val();
			break;
		case FuncionSobreBusquedaDeProductos.RUBROS:
			id = $("#listaRubro").val();
			filtro = "R";
			break;
		case FuncionSobreBusquedaDeProductos.SINSTOCK:
			id = "null";
			filtro = "S";
			break;
		case FuncionSobreBusquedaDeProductos.CONSTOCKAVENCER:
			id = "null";
			filtro = "V";
			break;
		case FuncionSobreBusquedaDeProductos.ALTAROTACION:
			id = "null";
			filtro = "A";
			break;
		case FuncionSobreBusquedaDeProductos.CONPI:
			id = "null";
			filtro = "I";
			break;
		case FuncionSobreBusquedaDeProductos.CONOC:
			id = "null";
			filtro = "O";
			break;
		default:
	}
	var datos = { filtro, id, tipo };
	PostGenHtml(datos, BuscarProductosOCPIURL, function (obj) {
		$("#divListaProducto").html(obj);
		addInCellEditHandler();
		addInCellKeyDownHandler();
		AddEventListenerToGrid("tbListaProducto");
		tableUpDownArrow();
		CerrarWaiting();
		return true
	});
}

function addInCellEditHandler() {
	$("#tbListaProducto").on('input', 'td[contenteditable]', function (e) {
		pedido = e.currentTarget.innerText;
		pIdEditado = e.currentTarget.parentNode.cells[0].innerText;
		rowIndex = e.currentTarget.parentNode.rowIndex;
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
		if (isNaN(String.fromCharCode(e.which)) && !(e.shiftKey && (e.which == 37 || e.which == 39))) e.preventDefault();
		//if (e.altKey && e.shiftKey && e.keyCode == 13) {
		//	//event.altKey = true;
		//	//event.shiftKey = true;
		//	//event.code = 'ArrowDown';
		//	document.onkeydown();
		//	e.preventDefault();
		//	//return false;
		//}
	});
}

function DisableComboProveedores(value) {
	if (value === true) {
		$('#listaProveedor').prop('disabled', 'disabled');
	}
	else {
		$('#listaProveedor').prop('disabled', false);
	}
}

function DisableComboProveedoresFamilia(value) {
	if (value === true) {
		$('#listaFamiliaProveedor').prop('disabled', 'disabled');
	}
	else {
		$('#listaFamiliaProveedor').prop('disabled', false);
	}
}

function DisableComboRubros(value) {
	if (value === true) {
		$('#listaRubro').prop('disabled', 'disabled');
	}
	else {
		$('#listaRubro').prop('disabled', false);
	}
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
		idProvSeleccionado = x.cells[2].innerText.trim();
		idFamiliaProvSeleccionado = x.cells[15].innerText.trim();
		idRubroSeleccionado = x.cells[16].innerText.trim();
		SeleccionarDesdeFila(idProvSeleccionado, $("#listaProveedor")[0]);
		SeleccionarDesdeFila(idFamiliaProvSeleccionado, $("#listaFamiliaProveedor")[0]);
		SeleccionarDesdeFila(idRubroSeleccionado, $("#listaRubro")[0]);
	}
	else {
		pIdSeleccionado = "";
		idProvSeleccionado = "";
		idFamiliaProvSeleccionado = "";
		idRubroSeleccionado = "";
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

function tableUpDownArrow() {
	const myTable = document.querySelector('#tbListaProducto tbody')
		, nbRows = myTable.rows.length
		, nbCells = myTable.rows[0].cells.length
		, movKey = {
			ArrowUp: p => { p.r = (--p.r + nbRows) % nbRows }
			, ArrowLeft: p => { p.c = (--p.c + nbCells) % nbCells }
			, ArrowDown: p => {
				p.r = ++p.r % nbRows
			}
			, ArrowRight: p => { p.c = ++p.c % nbCells }
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
			(evt.altKey && evt.shiftKey && evt.enterKey)) {
			let loop = true
				, nxFocus = null
				, cell = null
			do {
				if (evt.code === 'ArrowDown' && pos.r == nbRows)
					pos.r = 0;
				movKey[evt.code](pos)
				cell = myTable.rows[pos.r].cells[pos.c]
				if (pos.r == 0)
					pos.r = nbRows;
				else if (pos.r == nbRows)
					pos.r = 2;
				nxFocus = myTable.rows[pos.r - 1].cells[pos.c]

				if (nxFocus
					&& cell.style.display !== 'none'
					&& cell.parentNode.style.display !== 'none') {
					nxFocus.focus()
					nxFocus.closest('tr').classList.add('selected-row');
					loop = false
				}
			}
			while (loop)
			actualizarPedidoBulto();
		}
	}
}

function actualizarPedidoBulto() {
	if (!pedido)
		return false;
	AbrirWaiting();
	console.log("pedido: " + pedido + " pIdEditado: " + pIdEditado + " rowIndex: " + rowIndex);
	var tipo = "OC";
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
			tabla.rows[rowIndex - 1].cells[colCantidad].innerText = o.cantidad;
			tabla.rows[rowIndex - 1].cells[colCosto].innerText = o.pCosto;
			tabla.rows[rowIndex - 1].cells[colCostoTotal].innerText = o.cantidad;
			tabla.rows[rowIndex - 1].cells[colPallet].innerText = o.pallet;
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
			console.log(prod);
		}
	}
	return true;
}