$(function () {
	$("#btnAnalizar").on("click", AnalizarTR);
	AddEventListenerToGrid("tbListaSucursales");
	setMaxValueTotxtMaxPallet();
	SeleccionarFila(1, "tbListaSucursales");
	$("#txtMaxPallet").hide();

	// Seleccionar / deseleccionar todos
	$(document).on("change", "#chkSelectAllDepositos", function () {
		const checked = $(this).is(":checked");
		$(".chkDeposito").prop("checked", checked);
	});

	// Actualizar el checkbox maestro según las filas
	$(document).on("change", ".chkDeposito", function () {
		const total = $(".chkDeposito").length;
		const marcados = $(".chkDeposito:checked").length;

		$("#chkSelectAllDepositos").prop("checked", total === marcados);
	});

	addEventListenersToDepositos();
});

function setMaxValueTotxtMaxPallet() {
	const cant = document.getElementById("txtMaxPallet");
	cant.addEventListener('input', function (e) {
		if (!isValid(this.value))
			cant.value = 100;
	});
}
function isValid(value) {
	var cantUL = document.getElementById("txtMaxPallet")
	if (parseInt(value) <= cantUL.getAttribute('max'))
		return true;
	return false;
}

function changeMaxPalletChk(x) {
	var value = $("#txtMaxPallet").val();
	if (x.checked) {
		$("#txtMaxPallet").show();
		if (value < 10)
			$("#txtMaxPallet").val(10);
	}
	else {
		$("#txtMaxPallet").hide();
	}
}

function AnalizarTR() {
	var depositos = "";
	if (ExistenDepositosSeleccionados() && ExistenPedidosIncluidos()) {
		AbrirWaiting();
		//depositos = ObtenerDepositosSeleccionadosJSON();
		var depositosJson = JSON.stringify(ObtenerDepositosSeleccionadosJSON());
		var chkMaxPallet = $("#chkMaxPallet")[0].checked;
		var maxPallet = 99999999;
		if (chkMaxPallet)
			maxPallet = $("#txtMaxPallet").val();
		//var datos = { depositos, stkExistente, sustituto, maxPallet };
		var datos = { depositosJson, maxPallet };
		PostGen(datos, TRAnalizarParametrosUrl, function (o) {
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
					window.location.href = TRAbrirVistaAutorizacionesUrl;
					return true;
				}, false, ["Aceptar"], "info!", null);
			} else {
				CerrarWaiting();
				window.location.href = TRAbrirVistaAutorizacionesUrl;
			}
		});
	}
}

function SeleccionarFila(fila, tabla) {
	var grilla = document.getElementById(tabla);
	if (grilla) {
		if (grilla.rows[fila]) {
			grilla.rows[fila].classList.add('selected-row');
			selectTRSucursalesRow(grilla.rows[1]);
		}
	}
}

function ExistenPedidosIncluidos() {
	var rowCount = $('#tbListaPedidosIncluidos tbody tr').length;
	if (rowCount && rowCount >= 1) {
		return true;
	}
	else {
		AbrirMensaje("Atención", "Debe al menos agregar un Pedido de Sucursal.", function () {
			$("#msjModal").modal("hide");
			return false;
		}, false, ["Aceptar"], "error!", null);
	}
}

function ExistenDepositosSeleccionados() {

	var rowCount = $('#tbDepositosDeEnvio tr').length;
	if (rowCount && rowCount > 1) {
		var listaDepo = ObtenerListaDepositoSeleccionado();
		if (listaDepo && listaDepo.length > 0) {
			return true;
		}
		else {
			AbrirMensaje("Atención", "Debe al menos seleccionar un depósito.", function () {
				$("#msjModal").modal("hide");
				return false;
			}, false, ["Aceptar"], "error!", null);
		}
	}
	else {
		AbrirMensaje("Atención", "No existen depósito para incluir en el análisis.", function () {
			$("#msjModal").modal("hide");
			return false;
		}, false, ["Aceptar"], "error!", null);
	}
}

function ObtenerListaDepositoSeleccionado() {
	var lista = [];

	$("#tbDepositosDeEnvio tbody tr").each(function () {
		var chk = $(this).find(".chkDeposito");

		if (chk.length && chk.is(":checked")) {
			var depoId = $(this).find("td:eq(2)").text().trim();   // columna oculta depo_id
			lista.push(depoId);
		}
	});

	return lista;
}

function selectTRSucursalesRow(x) {
	AbrirWaiting();
	var admId = x.cells[3].innerText.trim();
	if (admId && admId !== "") {
		var datos = { admId };
		PostGenHtml(datos, TRCargarPedidosPorSucursalUrl, function (obj) {
			$("#divListaPedidosSucursal").html(obj);
			AddEventListenerToGrid("tbListaPedidosSucursal");
			ocultarPedidosYaIncluidosEnTablaSuperior();
			CerrarWaiting();
			return true
		});
	}
	else {
		AbrirMensaje("Atención", "Código de sucursal no válido.", function () {
			$("#msjModal").modal("hide");
			CerrarWaiting();
			return true;
		}, false, ["Aceptar"], "error!", null);
	}
}

function ocultarPedidosYaIncluidosEnTablaSuperior() {
	const incluidos = obtenerPedidosIncluidos();

	incluidos.forEach(pi => {
		ocultarFilaPedidoSucursal(pi);
	});
}

function obtenerPedidosIncluidos() {
	const filas = document.querySelectorAll("#tbListaPedidosIncluidos tr[data-pi-compte]");
	const lista = [];

	filas.forEach(f => {
		const pi = f.getAttribute("data-pi-compte");
		if (pi) lista.push(pi);
	});

	return lista;
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

function agregarAPedidosIncl(x) {
	AbrirWaiting();
	var picompte = x.dataset.interaction;
	if (picompte) {
		var datos = { picompte };
		PostGenHtml(datos, TRAgregarAPedidosIncluidosParaAutUrl, function (obj) {
			$("#divListaPedidosIncluidos").html(obj);
			AddEventListenerToGrid("tbListaPedidosIncluidos");
			ocultarFilaPedidoSucursal(picompte);
			ActualizarInfoSucursales();
			CerrarWaiting();
			return true
		});
	}
	CerrarWaiting();
}

function ocultarFilaPedidoSucursal(pi_compte) {
	const fila = document.querySelector(`#tbListaPedidosSucursal tr[data-pi-compte="${pi_compte}"]`);
	if (fila) fila.style.display = "none";
}

function imprimirDetallePI(pi_compte) {
	console.log(pi_compte);
	ReseteoDeReportes();
	setTimeout(() => {
		let data = { id: pi_compte };
		cargarReporteEnArre(65, data, "PEDIDO INTERNO", "", "");
		invocacionGestorDoc({});
	}, 500);
}
function ReseteoDeReportes() {
	console.log("Reseto de reportes");
	ReporteResetArre();
}

function verDetalleDePedido(x) {
	AbrirWaiting();
	var picompte = x.dataset.interaction;
	if (picompte) {
		var datos = { picompte };
		PostGenHtml(datos, TRVerDetallePedidoDeSucursalUrl, function (obj) {
			$("#divDetalleDePedido").html(obj);
			AddEventListenerToGrid("tbDetalleDePedido");
			document.getElementById("btnImprimirDetallePI").addEventListener("click", function () {
				$('#modalCenter').modal('hide')
				imprimirDetallePI(picompte);
			});

			$('#modalCenter').modal('show')
			CerrarWaiting();
			return true
		});
	}
	CerrarWaiting();
}

function mostrarFilaPedidoSucursal(pi_compte) {
	const fila = document.querySelector(`#tbListaPedidosSucursal tr[data-pi-compte="${pi_compte}"]`);
	if (fila) fila.style.display = "";
}

function quitarDePedidosIncl(x) {
	AbrirWaiting();
	var picompte = x.dataset.interaction;
	if (picompte) {
		var datos = { picompte };
		PostGenHtml(datos, TRQuitarDePedidosIncluidosParaAutUrl, function (obj) {
			$("#divListaPedidosIncluidos").html(obj);
			AddEventListenerToGrid("tbListaPedidosIncluidos");
			mostrarFilaPedidoSucursal(picompte);
			ActualizarInfoSucursales();
			CerrarWaiting();
			return true
		});
	}
	CerrarWaiting();
}

function ActualizarInfoSucursales() {
	AbrirWaiting();
	var datos = {};
	PostGenHtml(datos, TRActualizarInfoEnListaDeSucursalesUrl, function (obj) {
		$("#divListaSucursales").html(obj);
		AddEventListenerToGrid("tbListaSucursales");
		CerrarWaiting();
		return true
	});
}

function selectTRPedidoIncluidoRow(x) {

}

function selectTRDepositosDeEnvioRow(x) {
}

function selectTRPedidoSucursalRow(x) {
}

function selectTRDetalleDePedido(x) {
}

function getChildCheckbox(row) {
	return row.querySelector('.chkSoloSiNoHayStk');
}

function ObtenerDepositosSeleccionadosJSON() {
	var lista = [];

	$("#tbDepositosDeEnvio tbody tr").each(function () {
		var row = this;
		var chkParent = $(row).find(".chkDeposito");

		if (chkParent.length && chkParent.is(":checked")) {

			// depo_id está en la columna oculta (última)
			var depoId = $(row).find("td:eq(3)").text().trim();

			// obtener checkbox hijo
			var chkChild = getChildCheckbox(row);

			var soloSiNoHayStk = (chkChild && chkChild.checked) ? "S" : "N";

			lista.push({
				depo_id: depoId,
				solo_sino_hay_stk: soloSiNoHayStk
			});
		}
	});

	return lista;
}


function getChildCell(row) {
	return row.querySelector('.td-solo-si-no-hay-stk');
}

function createChildCheckbox(row, depoId) {
	var cell = getChildCell(row);
	if (!cell) return;
	if (cell.querySelector('input.chkSoloSiNoHayStk')) return;

	var name = 'soloSiNoHayStk_' + (depoId || '');
	cell.innerHTML = '<input type="checkbox" class="chkSoloSiNoHayStk" name="' + name + '" />';
}

function removeChildCheckbox(row) {
	var cell = getChildCell(row);
	if (!cell) return;
	cell.innerHTML = '';
}

function updateRowChild(row) {
	var parentChk = row.querySelector('.chkDeposito');
	if (!parentChk) return;
	var depoIdTd = row.querySelector('td[style*="display:none"]') || row.querySelector('td:last-child');
	var depoId = depoIdTd ? depoIdTd.textContent.trim() : '';
	if (parentChk.checked) {
		createChildCheckbox(row, depoId);
	} else {
		removeChildCheckbox(row);
	}
}

function addEventListenersToDepositos() {
	var table = document.getElementById('tbDepositosDeEnvio');
	if (!table) return;

	var headerChk = document.getElementById('chkSelectAllDepositos');
	var rowParentChecks = table.querySelectorAll('tbody .chkDeposito');

	rowParentChecks.forEach(function (chk) {
		var row = chk.closest('tr');
		updateRowChild(row);

		chk.addEventListener('change', function () {
			var row = chk.closest('tr');
			if (chk.checked) {
				updateRowChild(row);
			} else {
				removeChildCheckbox(row);
			}
		});
	});

	if (headerChk) {
		headerChk.addEventListener('change', function () {
			var checked = headerChk.checked;
			rowParentChecks.forEach(function (chk) {
				chk.checked = checked;
				chk.dispatchEvent(new Event('change'));
			});
		});
	}
}