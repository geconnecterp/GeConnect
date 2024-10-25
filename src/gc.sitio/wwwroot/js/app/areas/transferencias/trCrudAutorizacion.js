$(function () {
	$("#btnAnalizar").on("click", AnalizarTR);
	AddEventListenerToGrid("tbListaSucursales");
	setMaxValueTotxtMaxPallet();
	SeleccionarFila(1, "tbListaSucursales");
	$("#txtMaxPallet").hide();
});

function setMaxValueTotxtMaxPallet() {
	const cant = document.getElementById("txtMaxPallet");
	cant.addEventListener('input', function (e) {
		if (!isValid(this.value))
			cant.value = 80;
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
		depositos = ObtenerListaDepositoSeleccionado();
		var stkExistente = $("#chkConsiderarStock")[0].checked;
		var sustituto = $("#chkModificarYSustituto")[0].checked;
		var chkMaxPallet = $("#chkMaxPallet")[0].checked;
		var maxPallet = 99999999;
		if (chkMaxPallet)
			maxPallet = $("#txtMaxPallet").val();
		var datos = { depositos, stkExistente, sustituto, maxPallet };
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
	var lista = "";
	$('#tbDepositosDeEnvio tbody tr').each(function (index, tr) {
		if (tr.cells[1] && tr.cells[1].firstChild) {
			if (tr.cells[1].firstChild.checked) {
				lista += tr.cells[2].innerText.trim() + "@";
			}
		}
	});
	return lista.substring(0, lista.length - 1);
}

function selectTRSucursalesRow(x) {
	AbrirWaiting();
	var admId = x.cells[3].innerText.trim();
	if (admId && admId !== "") {
		var datos = { admId };
		PostGenHtml(datos, TRCargarPedidosPorSucursalUrl, function (obj) {
			$("#divListaPedidosSucursal").html(obj);
			AddEventListenerToGrid("tbListaPedidosSucursal");
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
			ActualizarInfoSucursales();
			CerrarWaiting();
			return true
		});
	}
	CerrarWaiting();
}

function verDetalleDePedido(x) {
	AbrirWaiting();
	var picompte = x.dataset.interaction;
	if (picompte) {
		var datos = { picompte };
		PostGenHtml(datos, TRVerDetallePedidoDeSucursalUrl, function (obj) {
			$("#divDetalleDePedido").html(obj);
			AddEventListenerToGrid("tbDetalleDePedido");
			$('#modalCenter').modal('show')
			CerrarWaiting();
			return true
		});
	}
	CerrarWaiting();
}

function quitarDePedidosIncl(x) {
	AbrirWaiting();
	var picompte = x.dataset.interaction;
	if (picompte) {
		var datos = { picompte };
		PostGenHtml(datos, TRQuitarDePedidosIncluidosParaAutUrl, function (obj) {
			$("#divListaPedidosIncluidos").html(obj);
			AddEventListenerToGrid("tbListaPedidosIncluidos");
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