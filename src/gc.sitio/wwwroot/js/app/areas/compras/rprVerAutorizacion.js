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
	CargarDetalleDeConteos();
});

function CargarDetalleDeConteos() {
	var datos = {};
	PostGenHtml(datos, buscarDetalleVerConteosUrl, function (obj) {
		$("#divDetalleVerConteos").html(obj);
		AgregarHandlerDeSeleccion("tbDetalleVerConteos");
		CerrarWaiting();
		return true
	})
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