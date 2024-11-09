$(function () {
	$("#btnCargaPrevia").on("click", AbrirCargaPrevia);
	$("#txtUPEnComprobanteRP").on("keyup", analizaInputUP);
	$("#txtBtoEnComprobanteRP").on("keyup", analizaInputBto);
	$("#txtUnidEnComprobanteRP").on("keyup", analizaInputUnid);
	$("#listaDeposito").on("change", listaDepositoChange);
	$("#listaBox").on("change", listaBoxesChange);
	$("#listaDepositoEnCargaPrevia").on("change", listaDepositoEnCargaPreviaChange);
	//
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
		$("#btnAddProd").focus();
	}
}

function seleccionarProductosDesdeCargaPrevia() {
}

function listaBoxesChange() {
}

function AbrirCargaPrevia() {
	AbrirWaiting();
	var datos = { };
	PostGenHtml(datos, ObtenerDatosModalCargaPreviaUrl, function (obj) {
		$("#divModalCargaPrevia").html(obj);
		AddEventListenerToGrid("tbListaProductosParaAgregar");
		$("#listaDepositoEnCargaPrevia").on("change", listaDepositoEnCargaPreviaChange);
		$('#modalCargaPrevia').modal('show')
		CerrarWaiting();
		return true
	});
	CerrarWaiting();
}

function listaDepositoChange() {
	BuscarBoxDesdeDeposito();
}

function listaBoxEnCargaPreviaChange() {
}

function BuscarBoxDesdeDeposito() {
	AbrirWaiting();
	var depoId = $("#listaDeposito").val();
	var datos = { depoId };
	PostGenHtml(datos, BuscarBoxesDesdeDepositoURL, function (obj) {
		$("#divComboBoxes").html(obj);
		$("#listaBox").on("change", listaBoxesChange);
		CerrarWaiting();
		return true
	});
}

function listaDepositoEnCargaPreviaChange() {
	AbrirWaiting();
	var depoId = $("#listaDepositoEnCargaPrevia").val();
	var datos = { depoId };
	PostGenHtml(datos, ObtenerBoxesDesdeDepositoDesdeCargaPreviaURL, function (obj) {
		$("#divComboBoxesEnCargaPrevia").html(obj);
		$("#listaBoxEnCargaPrevia").on("change", listaBoxEnCargaPreviaChange);
		CerrarWaiting();
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