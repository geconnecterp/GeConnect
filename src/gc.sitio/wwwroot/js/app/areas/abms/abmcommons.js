function activarBotones(activar) {
	if (activar === true) {
		//el activarlos es activar BM
		$("#btnAbmNuevo").prop("disabled", false);
		$("#btnAbmModif").prop("disabled", false);
		$("#btnAbmElimi").prop("disabled", false);


		$("#btnAbmAceptar").prop("disabled", true);
		$("#btnAbmCancelar").prop("disabled", true);
		$("#btnAbmAceptar").hide();
		$("#btnAbmCancelar").hide();
	}
	else {
		$("#btnAbmNuevo").prop("disabled", false);
		$("#btnAbmModif").prop("disabled", true);
		$("#btnAbmElimi").prop("disabled", true);

		$("#btnAbmAceptar").prop("disabled", true);
		$("#btnAbmCancelar").prop("disabled", true);
		$("#btnAbmAceptar").hide();
		$("#btnAbmCancelar").hide();
	}
}

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

function HabilitarBotonesPorAccion(accion) {
	switch (accion) {
		case AbmAction.ALTA:
			HabilitarBotones(true, true, true, false, false);
			break;
		case AbmAction.BAJA:
			HabilitarBotones(true, true, true, false, false);
			break;
		case AbmAction.MODIFICACION:
			HabilitarBotones(true, true, true, false, false);
			break;
		case AbmAction.SUBMIT:
			HabilitarBotones(false, false, false, true, true);
			break;
		case AbmAction.CANCEL:
			HabilitarBotones(false, false, false, true, true);
			break;
		default:
			HabilitarBotones(false, false, false, true, true);
			break;
	}
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

function controlaValorProvi() {
	var provId = $("#listaProvi option:selected").val()
	if (provId !== "") {
		var data = { provId }
		AbrirWaiting();
		PostGenHtml(data, obtenerDepartamentosUrl, function (obj) {
			$("#divLocalidad").html(obj);
			CerrarWaiting();
		}, function (obj) {
			ControlaMensajeError(obj.message);
			CerrarWaiting();
		});
	}
}

function controlaValorAfip() {
	if ($("#listaAfip option:selected").val() !== "05" && $("#listaAfip option:selected").val() !== "02") {
		$("#listaIB").prop("disabled", false);
		$("#Cliente_Cta_Ib_Nro").prop("disabled", false);
		$("#chkPivaCertActiva").prop("disabled", false);
		$("#chkIbCertActiva").prop("disabled", false);
	}
	else {
		$("#listaIB").prop("disabled", true);
		$("#Cliente_Cta_Ib_Nro").prop("disabled", true);
		$("#chkPivaCertActiva").prop("disabled", true);
		$("#chkIbCertActiva").prop("disabled", true);
	}
}