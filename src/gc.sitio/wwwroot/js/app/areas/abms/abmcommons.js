function BuscarFormaDePago() {
	if ($(".nav-link").prop("disabled")) {
		return false;
	}
	if (ctaId != "") {
		var data = { ctaId };
		AbrirWaiting();
		PostGenHtml(data, buscarFormaDePagoUrl, function (obj) {
			$("#divFormasDePago").html(obj);
			AgregarHandlerSelectedRow("tbClienteFormaPagoEnTab");
			$(".activable").prop("disabled", true);
			$("#IdSelected").val("");
			CerrarWaiting();
		}, function (obj) {
			ControlaMensajeError(obj.message);
			CerrarWaiting();
		});
	}
}

function BuscarObservaciones() {
	if ($(".nav-link").prop("disabled")) {
		return false;
	}
	if (ctaId != "") {
		var data = { ctaId };
		AbrirWaiting();
		PostGenHtml(data, buscarObservacionesUrl, function (obj) {
			$("#divObservaciones").html(obj);
			AgregarHandlerSelectedRow("tbClienteObservaciones");
			$(".activable").prop("disabled", true);
			$("#IdSelected").val("");
			CerrarWaiting();
		}, function (obj) {
			ControlaMensajeError(obj.message);
			CerrarWaiting();
		});
	}
}

function BuscarNotas() {
	if ($(".nav-link").prop("disabled")) {
		return false;
	}
	if (ctaId != "") {
		var data = { ctaId };
		AbrirWaiting();
		PostGenHtml(data, buscarNotasUrl, function (obj) {
			$("#divNotas").html(obj);
			AgregarHandlerSelectedRow("tbClienteNotas");
			$(".activable").prop("disabled", true);
			$("#IdSelected").val("");
			CerrarWaiting();
		}, function (obj) {
			ControlaMensajeError(obj.message);
			CerrarWaiting();
		});
	}
}

function BuscarOtrosContactos() {
	if ($(".nav-link").prop("disabled")) {
		return false;
	}
	if (ctaId != "") {
		var data = { ctaId };
		AbrirWaiting();
		PostGenHtml(data, buscarOtrosContactosUrl, function (obj) {
			$("#divOtrosContactos").html(obj);
			AgregarHandlerSelectedRow("tbClienteOtroContacto");
			$(".activable").prop("disabled", true);
			$("#IdSelected").val("");
			CerrarWaiting();
		}, function (obj) {
			ControlaMensajeError(obj.message);
			CerrarWaiting();
		});
	}
}

function HabilitarBotones(btnAlta, btnBaja, btnModi, btnSubmit, btnCancel) {
	$("#btnAbmNuevo").prop("disabled", btnAlta);
	$("#btnAbmModif").prop("disabled", btnModi);
	$("#btnAbmElimi").prop("disabled", btnBaja);
	$("#btnAbmAceptar").prop("disabled", btnSubmit);
	$("#btnAbmCancelar").prop("disabled", btnCancel);
}

function AgregarHandlerSelectedRow(grilla) {
	document.getElementById(grilla).addEventListener('click', function (e) {
		if (e.target.nodeName === 'TD') {
			var selectedRow = this.querySelector('.selected-row');
			if (selectedRow) {
				selectedRow.classList.remove('selected-row');
			}
			e.target.closest('tr').classList.add('selected-row');
		}
	});
}
