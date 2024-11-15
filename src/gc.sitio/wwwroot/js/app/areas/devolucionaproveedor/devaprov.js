$(function () {
	$("#btnradioManual").on("click", BtnRadioManual);
	$("#btnradioRevertirDevolucion").on("click", BtnRadioRevertirDevolucion);
	$("#btnradioCargaPrevia").on("click", BtnRadioCargaPrevia);
	$("#btnRevertirDevolucion").on("click", ValidarDevolucion);
	$("#btnCargaPrevia").on("click", AbrirCargaPrevia);
	$("#listaDeposito").on("change", listaDepositoChange);
	$("#listaBox").on("change", listaBoxesChange);
});

function BtnRadioManual() {
	$("#divRevertirAjuste").find('input').each(function () {
		$(this).attr('disabled', 'disabled');
	});
	$("#divCargaPrevia").find('button').each(function () {
		$(this).attr('disabled', 'disabled');
	});
}

function BtnRadioRevertirDevolucion() {
	$("#divRevertirAjuste").find('input').each(function () {
		$(this).removeAttr('disabled');
	});
	$("#divCargaPrevia").find('button').each(function () {
		$(this).attr('disabled', 'disabled');
	});
}

function BtnRadioCargaPrevia() {
	$("#divRevertirAjuste").find('input').each(function () {
		$(this).attr('disabled', 'disabled');
	});
	$("#divCargaPrevia").find('button').each(function () {
		$(this).removeAttr('disabled');
	});
}

function listaDepositoChange() {
	BuscarBoxDesdeDeposito();
}

function listaBoxesChange() {
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

function AbrirCargaPrevia() {
	AbrirWaiting();
	var datos = {};
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

function ValidarDevolucion() {
	var ajId = $("#txtNroAjuste").val();
	if (ajId === "") {
		AbrirMensaje("Atención", "Debe ingresar un ID de Ajuste.", function () {
			$("#msjModal").modal("hide");
			$("#txtNroAjuste").focus();
			return true;
		}, false, ["Aceptar"], "warn!", null);
	}
	AbrirWaiting();
	var datos = { ajId }
	PostGen(datos, ValidarNroDeAjusteARevertirURL, function (o) {
		CerrarWaiting();
		if (o.error === true) {
			AbrirMensaje("Atención", o.msg, function () {
				$("#msjModal").modal("hide");
				return true;
			}, false, ["Aceptar"], "error!", null);
		} else if (o.warn === true) {
			AbrirMensaje("Atención", o.msg, function () {
				$("#msjModal").modal("hide");
				return true;
			}, false, ["Aceptar"], "warn!", null);
		} else {
			RevertirAjuste(ajId);
		}
	});
}