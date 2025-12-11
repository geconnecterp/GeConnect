/**
 * Información adicional de productos
 * 
 * Este módulo se encarga de la visualización de información adicional relacionada con los productos.
 */
var p_id = "";
$(function () {
	var mostrarInfoProd = true;
	var mostrarInfoProdStkA = true;
	var mostrarInfoProdStkD = true;
	var mostrarInfoProdStkBox = true;
	var mostrarInfoProdStkMovM = true;
	var mostrarInfoProdStkMovS = true;
	var mostrarInfoProdStkMovD = true;
	var mostrarInfoProdSustituto = true;

	$(document).on("productoSeleccionadoParaInfoAdicional", function (e, data) {
		console.log("Evento captado en componente:", data);

		invocarComponenteDeInfoAdicionalDeProd({
			p_id: data.p_id,
			mostrarInfoProd,
			mostrarInfoProdStkA,
			mostrarInfoProdStkD,
			mostrarInfoProdStkBox,
			mostrarInfoProdStkMovM,
			mostrarInfoProdStkMovS,
			mostrarInfoProdStkMovD,
			mostrarInfoProdSustituto
		});
	});
	$(document).on("keydown", "#txtMeses", function (event) {
		if (event.key === "Enter") {
			event.preventDefault();
			let pId = p_id;
			let meses = $(this).val();
			let admId = $("#listaSucursales").val();
			var datos = { pId, admId, meses };
			PostGenHtml(datos, BuscarInfoProdMovMensURL, function (obj) {
				$("#divMovMen").html(obj);
				AddEventListenerToTable("tbInfoProdMovMes");
				CerrarWaiting();
				return true
			});
		}
	});
	$(document).on("change", "#listaSucursalesM", function () {
		let sucursalSeleccionada = $(this).val();
		console.log("Sucursal seleccionada:", sucursalSeleccionada);

		let pId = p_id;
		let meses = $("#txtMeses").val();
		let admId = sucursalSeleccionada;
		var datos = { pId, admId, meses };
		PostGenHtml(datos, BuscarInfoProdMovMensURL, function (obj) {
			$("#divMovMen").html(obj);
			AddEventListenerToTable("tbInfoProdMovMes");
			CerrarWaiting();
			return true
		});
	});
	$(document).on("keydown", "#txtSemanas", function (event) {
		if (event.key === "Enter") {
			event.preventDefault();
			let pId = p_id;
			let semanas = $(this).val();
			let admId = $("#listaSucursales").val();
			var datos = { pId, admId, semanas };
			PostGenHtml(datos, BuscarInfoProdMovSemURL, function (obj) {
				$("#divMovSem").html(obj);
				AddEventListenerToTable("tbInfoProdMovSem");
				CerrarWaiting();
				return true
			});
		}
	});
	$(document).on("change", "#listaSucursalesS", function () {
		let sucursalSeleccionada = $(this).val();
		console.log("Sucursal seleccionada:", sucursalSeleccionada);

		let pId = p_id;
		let semanas = $("#txtSemanas").val();
		let admId = sucursalSeleccionada;
		var datos = { pId, admId, semanas };
		PostGenHtml(datos, BuscarInfoProdMovSemURL, function (obj) {
			$("#divMovSem").html(obj);
			AddEventListenerToTable("tbInfoProdMovSem");
			CerrarWaiting();
			return true
		});
	});
	$(document).on("change", "#listaSucursalesX", function () {
		let sucursalSeleccionada = $(this).val();
		console.log("Sucursal seleccionada:", sucursalSeleccionada);

		let pId = p_id;
		let admId = sucursalSeleccionada;
		var datos = { pId, admId };
		PostGenHtml(datos, BuscarInfoProdStkBoxURL, function (obj) {
			$("#divStkBox").html(obj);
			AddEventListenerToTable("tbInfoProdStkBox");
			CerrarWaiting();
			return true
		});
	});
	$(document).on("change", "#listaSucursalesD", function () {
		let sucursalSeleccionada = $(this).val();
		console.log("Sucursal seleccionada:", sucursalSeleccionada);

		let pId = p_id;
		let admId = sucursalSeleccionada;
		var datos = { pId, admId };
		PostGenHtml(datos, BuscarInfoProdStkDURL, function (obj) {
			$("#divStkD").html(obj);
			AddEventListenerToTable("tbInfoProdStkD");
			CerrarWaiting();
			return true
		});
	});
});

function invocarComponenteDeInfoAdicionalDeProd(p) {
	pId = p.p_id;
	p_id = p.p_id;
	var mostrarInfoProd = p.mostrarInfoProd;
	var mostrarInfoProdStkA = p.mostrarInfoProdStkA;
	var mostrarInfoProdStkD = p.mostrarInfoProdStkD;
	var mostrarInfoProdStkBox = p.mostrarInfoProdStkBox;
	var mostrarInfoProdStkMovM = p.mostrarInfoProdStkMovM;
	var mostrarInfoProdStkMovS = p.mostrarInfoProdStkMovS;
	var mostrarInfoProdStkMovD = p.mostrarInfoProdStkMovD;
	var mostrarInfoProdSustituto = p.mostrarInfoProdSustituto;
	var data = { pId };
	PostGenHtml(data, abrirComponenteDeInfoAdicionalDeProdUrl, function (obj) {
		$("#divInfoAdicionaDeProducto").html(obj);
		//$("#divInfoAdicionaDeProducto").collapse("show");
		var meses = $("#txtMeses").val();
		var semanas = $("#txtSemanas").val();
		var admId = $("#listaSucursales").val();
		if (mostrarInfoProd) {
			datos = { pId }
			PostGenHtml(datos, BuscarInfoProdURL, function (obj) {
				$("#divInfoProducto").html(obj);
				AddEventListenerToTable("tbInfoProducto");
				CerrarWaiting();
				return true
			});
		}
		if (mostrarInfoProdStkA) {
			PostGenHtml(datos, BuscarInfoProdStkAURL, function (obj) {
				$("#divStkA").html(obj);
				AddEventListenerToTable("tbInfoProdStkA");
				CerrarWaiting();
				return true
			});
		}
		if (mostrarInfoProdStkD) {
			var datos = { pId, admId };
			PostGenHtml(datos, BuscarInfoProdStkDURL, function (obj) {
				$("#divStkD").html(obj);
				AddEventListenerToTable("tbInfoProdStkD");
				CerrarWaiting();
				return true
			});
		}
		if (mostrarInfoProdStkBox) {
			var datos = { pId, admId };
			PostGenHtml(datos, BuscarInfoProdStkBoxURL, function (obj) {
				$("#divStkBox").html(obj);
				AddEventListenerToTable("tbInfoProdStkBox");
				CerrarWaiting();
				return true
			});
		}
		if (mostrarInfoProdStkMovM) {
			var datos = { pId, admId, meses };
			PostGenHtml(datos, BuscarInfoProdMovMensURL, function (obj) {
				$("#divMovMen").html(obj);
				AddEventListenerToTable("tbInfoProdMovMes");
				CerrarWaiting();
				return true
			});
		}
		if (mostrarInfoProdStkMovS) {
			var datos = { pId, admId, semanas };
			PostGenHtml(datos, BuscarInfoProdMovSemURL, function (obj) {
				$("#divMovSem").html(obj);
				AddEventListenerToTable("tbInfoProdMovSem");
				CerrarWaiting();
				return true
			});
		}
		if (mostrarInfoProdSustituto) {
			var tipo = tipoDeOperacion;
			var soloProv = true; //Valor por default
			var datos = { pId, tipo, soloProv }
			PostGenHtml(datos, BuscarInfoProdSustitutoURL, function (obj) {
				$("#divSus").html(obj);
				AddEventListenerToTable("tbListaProductoSust");
				CerrarWaiting();
				return true
			});
		}

	});
}

function AddEventListenerToTable(tabla) {
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

function selectListaInfoProdMovMens(x) {
}

function selectListaInfoProdMovSem(x) {
}
function selectListaInfoProdStkBox(x) {
}
function selectListaInfoProdStkD(x) {
}
function selectListaInfoProdStkA(x) {
}
function selectListaInfoProdSustituto(x) {
}