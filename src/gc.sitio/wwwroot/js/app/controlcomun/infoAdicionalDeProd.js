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
			AbrirWaiting();
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
		AbrirWaiting();
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
			AbrirWaiting();
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
		AbrirWaiting();
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
		AbrirWaiting();
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
		AbrirWaiting();
		PostGenHtml(datos, BuscarInfoProdStkDURL, function (obj) {
			$("#divStkD").html(obj);
			AddEventListenerToTable("tbInfoProdStkD");
			CerrarWaiting();
			return true
		});
	});
	$(document).on("change", "#listaDeposMovD", function () {
		let depositoSeleccionado = $(this).val();
		console.log("Deposito seleccionado:", depositoSeleccionado);

		let pId = p_id;
		let admId = "";
		let depId = depositoSeleccionado;
		let tmId = $("#listaTMMovD").val();
		let desde = $("#txtDesde").val();
		let hasta = $("#txtHasta").val();
		AbrirWaiting();
		var datos = { pId, admId, depId, tmId, desde, hasta };
		PostGenHtml(datos, BuscarInfoProdMovDURL, function (obj) {
			$("#divMovDet").html(obj);
			AddEventListenerToTable("tbInfoProdMovD");
			CerrarWaiting();
			return true
		});
	});
	$(document).on("change", "#listaTMMovD", function () {
		let tipoMovSeleccionado = $(this).val();
		console.log("Tipo movimiento seleccionado:", tipoMovSeleccionado);

		let pId = p_id;
		let admId = "";
		let depId = $("#listaDeposMovD").val();
		let tmId = tipoMovSeleccionado;
		let desde = $("#txtDesde").val();
		let hasta = $("#txtHasta").val();
		var datos = { pId, admId, depId, tmId, desde, hasta };
		AbrirWaiting();
		PostGenHtml(datos, BuscarInfoProdMovDURL, function (obj) {
			$("#divMovDet").html(obj);
			AddEventListenerToTable("tbInfoProdMovD");
			CerrarWaiting();
			return true
		});
	});
	$(document).on("blur", "#txtDesde", function () {
		let desdeSeleccionado = $(this).val();
		console.log("Desde seleccionado:", desdeSeleccionado);

		let pId = p_id;
		let admId = "";
		let depId = $("#listaDeposMovD").val();
		let tmId = $("#listaTMMovD").val();
		let desde = desdeSeleccionado;
		let hasta = $("#txtHasta").val();
		var datos = { pId, admId, depId, tmId, desde, hasta };
		AbrirWaiting();
		PostGenHtml(datos, BuscarInfoProdMovDURL, function (obj) {
			$("#divMovDet").html(obj);
			AddEventListenerToTable("tbInfoProdMovD");
			CerrarWaiting();
			return true
		});
	});
	$(document).on("blur", "#txtHasta", function () {
		let hastaSeleccionado = $(this).val();
		console.log("Hasta seleccionado:", hastaSeleccionado);

		let pId = p_id;
		let admId = "";
		let depId = $("#listaDeposMovD").val();
		let tmId = $("#listaTMMovD").val();
		let desde = $("#txtDesde").val();
		let hasta = hastaSeleccionado;
		var datos = { pId, admId, depId, tmId, desde, hasta };
		AbrirWaiting();
		PostGenHtml(datos, BuscarInfoProdMovDURL, function (obj) {
			$("#divMovDet").html(obj);
			AddEventListenerToTable("tbInfoProdMovD");
			CerrarWaiting();
			return true
		});
	});

	$(document).on("change", "#ajusteAltura", function () {
		let valor = $(this).val();
		let base = 40; // altura base en %

		let nuevoAlto;
		if (valor === "-75") {
			nuevoAlto = base * 0.75;
		} else if (valor === "-100") {
			nuevoAlto = base;
		} else if (valor === "+25") {
			nuevoAlto = base * 1.25;
		}

		$("#divInfo").css("height", nuevoAlto + "%");
	});

});

function invocarComponenteDeInfoAdicionalDeProd(p) {
	// 1. Guardar el id del tab activo actual
	var tabActivo = $("#divInfo .nav-link.active").data("bs-target");

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

		if (tabActivo) {
			let boton = document.querySelector(`#divInfo .nav-link[data-bs-target='${tabActivo}']`);
			if (boton) {
				let tab = new bootstrap.Tab(boton);
				tab.show();
			}
		}

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
		if (mostrarInfoProdStkMovD) {
			var depId = "%";
			var tmId = "%";
			// fecha de hoy
			var hoy = new Date();

			// fecha 30 días atrás
			var hace30dias = new Date();
			hace30dias.setDate(hoy.getDate() - 30);

			// formatear a string (ejemplo: yyyy-MM-dd)
			function formatDate(d) {
				let yyyy = d.getFullYear();
				let mm = String(d.getMonth() + 1).padStart(2, '0');
				let dd = String(d.getDate()).padStart(2, '0');
				return `${yyyy}-${mm}-${dd}`;
			}

			var desde = formatDate(hace30dias);
			var hasta = formatDate(hoy);

			var datos = { pId, admId, depId, tmId, desde, hasta };
			PostGenHtml(datos, BuscarInfoProdMovDURL, function (obj) {
				$("#divMovDet").html(obj);
				AddEventListenerToTable("tbInfoProdMovD");
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
function selectListaInfoProdMovD(x) {
}