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
});

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
		CerrarWaiting();
		return true
	})
}