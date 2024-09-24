$(function () {
	if (document.getElementById("tbVerComptesDeRP") != null) {
		document.getElementById("tbVerComptesDeRP").addEventListener('click', function (e) {
			if (e.target.nodeName === 'TD') {
				var selectedRow = this.querySelector('.selected-row');
				if (selectedRow) {
					selectedRow.classList.remove('selected-row');
				}
				e.target.closest('tr').classList.add('selected-row');
			}
		});
	}

	$("#btnRegresarAAutorizacionesRP").on("click", RegresarASelAuto); //Regregar a la pantalla de seleccion de autorizaciones.
	$("#btnConfirmarRP").on("click", ConfirmarRP);
	CargarDetalleDeConteos();
	SeleccionarDeposito();
	SelecccionarPrimerRegistro("tbVerComptesDeRP");
	
});

function SelecccionarPrimerRegistro(grilla) {
	var grid = document.getElementById(grilla);
	if (grid) {
		var rowsBody = grid.getElementsByTagName('tbody')[0];
		if (rowsBody && rowsBody.firstElementChild) {
			rowsBody.firstElementChild.className = "selected-row"
			selectVerComptesDeRPRow(rowsBody.firstElementChild);
		}
	}
}

function SelecccionarPrimerRegistroConteos() {
	var grid = document.getElementById('tbDetalleVerConteos');
	if (grid) {
		var rowsBody = grid.getElementsByTagName('tbody')[0];
		if (rowsBody && rowsBody.firstElementChild) {
			rowsBody.firstElementChild.className = "selected-row"
			selectDetalleDeVerConteos(rowsBody.firstElementChild);
		}
	}
}

function SeleccionarDeposito() {
	var depoid = $("#DepoId").val();
	if (depoid !== "") {
		$("#listaDeposito").val(depoid);
	}
}

function CargarDetalleDeConteos() {
	var datos = {};
	PostGenHtml(datos, buscarDetalleVerConteosUrl, function (obj) {
		$("#divDetalleVerConteos").html(obj);
		AgregarHandlerDeSeleccion("tbDetalleVerConteos");
		CerrarWaiting();
		return true
	})
}

function ConfirmarRP() {
	AbrirMensaje("Confirmación", "Esta a punto de confirmar un comprobante RPR, esta acción no puede deshacerse. Desea continuar?", function (e) {
		$("#msjModal").modal("hide");
		switch (e) {
			case "SI": //Confirmar comprobante RPR
				Confirmar();
				break;
			case "NO":
				break;
			default: //NO
				break;
		}
		return true;
	}, true, ["Aceptar", "Cancelar"], "info!", null);
}

function Confirmar() {
	var rp = $("#Rp").val();
	var datos = { rp };
	PostGen(datos, confirmarRPRUrl, function (o) {
		if (o.error === true) {
			AbrirMensaje("Atención", o.msg, function () {
				$("#msjModal").modal("hide");
				window.location.href = volverAListaDeAutorizacionesUrl;
				return true;
			}, false, ["Aceptar"], "error!", null);
		} else if (o.warn === true) {
			AbrirMensaje("Atención", o.msg, function () {
				$("#msjModal").modal("hide");
				window.location.href = volverAListaDeAutorizacionesUrl;
				return true;
			}, false, ["Aceptar"], "warn!", null);
		} else if (o.codigo !== "") {
			AbrirMensaje("Atención", o.msg, function (e) {
				$("#msjModal").modal("hide");
				window.location.href = volverAListaDeAutorizacionesUrl;
				return true;
			}, false, ["Aceptar"], "info!", null);
		} else {
			window.location.href = volverAListaDeAutorizacionesUrl;
		}
	});
}

function RegresarASelAuto() {
	window.location.href = volverAListaDeAutorizacionesUrl;
}

function selectVerComptesDeRPRow(x) {
	var tco_id = x.cells[0].innerText.trim();
	var cc_compte = x.cells[1].innerText.trim();
	var rp = $("#Rp").val();
	var detalle = x.cells[2].innerText.trim();
	document.getElementById("tabOne").outerHTML = "<span id=\"tabOne\">Detalle de " + detalle + "</span>";
	if (rp != "" && tco_id != "" && cc_compte != "") {
		BuscarDetalleVerCompte(rp, tco_id, cc_compte);
	}
	else {
		AbrirMensaje("Atención", "Faltan datos.", function () {
			$("#msjModal").modal("hide");
			return true;
		}, false, ["Aceptar"], "error!", null);
	}
}

function BuscarDetalleVerCompte(rp, tco_id, cc_compte) {
	AbrirWaiting();
	var datos = { rp, tco_id, cc_compte };
	PostGenHtml(datos, buscarDetalleVerCompteUrl, function (obj) {
		$("#divDetalleVerCompte").html(obj);
		AgregarHandlerDeSeleccion("tbDetalleVerCompte");
		SelecccionarPrimerRegistroConteos();
		CerrarWaiting();
		return true
	})
}

function selectDetalleVerConteoSeleccionado(x) {
}

function selectDetalleDeVerCompte(x) {
}

function selectDetalleDeVerConteos(x) {
	var p_id = x.cells[1].innerText.trim();
	var datos = { p_id };
	PostGenHtml(datos, buscarDetalleVerConteoSeleccionadoUrl, function (obj) {
		$("#divDetalleVerConteosSeleccionado").html(obj);
		AgregarHandlerDeSeleccion("tbDetalleVerConteoSeleccionado");
		CerrarWaiting();
		return true
	})
}

function AgregarHandlerDeSeleccion(table) {
	if (document.getElementById(table) != null) {
		document.getElementById(table).addEventListener('click', function (e) {
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