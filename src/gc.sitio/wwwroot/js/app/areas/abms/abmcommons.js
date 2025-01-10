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

function NuevaFormaDePago() {
	var tabActiva = $('.nav-tabs .active')[0].id;
	var mensaje = PuedoAgregar(tabActiva);
	if (mensaje !== "") {
		AbrirMensaje("ATENCIÓN", mensaje, function () {
			$("#msjModal").modal("hide");
			return true;
		}, false, ["Aceptar"], "error!", null);
	}
	else {
		var data = {};
		PostGenHtml(data, nuevaFormaDePagoUrl, function (obj) {
			$("#divDatosDeFPSelected").html(obj);
			$(".nav-link").prop("disabled", true);
			$(".activable").prop("disabled", false);
			desactivarGrilla(Grids.GridFP);
			HabilitarBotonesPorAccion(AbmAction.ALTA);
			$("#FormaDePago_Fp_Id").focus();
			CerrarWaiting();
		}, function (obj) {
			ControlaMensajeError(obj.message);
			CerrarWaiting();
		});
	}
}

function NuevoContacto() {
	var tabActiva = $('.nav-tabs .active')[0].id;
	var mensaje = PuedoAgregar(tabActiva);
	if (mensaje !== "") {
		AbrirMensaje("ATENCIÓN", mensaje, function () {
			$("#msjModal").modal("hide");
			return true;
		}, false, ["Aceptar"], "error!", null);
	}
	else {
		var data = {};
		PostGenHtml(data, nuevoContactoUrl, function (obj) {
			$("#divDatosDeOCSelected").html(obj);
			$(".nav-link").prop("disabled", true);
			$(".activable").prop("disabled", false);
			desactivarGrilla(Grids.GridOC);
			HabilitarBotonesPorAccion(AbmAction.ALTA);
			$("#OtroContacto_cta_nombre").focus();
			CerrarWaiting();
		}, function (obj) {
			ControlaMensajeError(obj.message);
			CerrarWaiting();
		});
	}
}

function NuevaNota() {
	var tabActiva = $('.nav-tabs .active')[0].id;
	var mensaje = PuedoAgregar(tabActiva);
	if (mensaje !== "") {
		AbrirMensaje("ATENCIÓN", mensaje, function () {
			$("#msjModal").modal("hide");
			return true;
		}, false, ["Aceptar"], "error!", null);
	}
	else {
		var data = {};
		PostGenHtml(data, nuevaNotaUrl, function (obj) {
			$("#divDatosDeNotaSelected").html(obj);
			$(".nav-link").prop("disabled", true);
			$(".activable").prop("disabled", false);
			desactivarGrilla(Grids.GridNota);
			HabilitarBotonesPorAccion(AbmAction.ALTA);
			$("#Nota_usu_apellidoynombre").focus();
			CerrarWaiting();
		}, function (obj) {
			ControlaMensajeError(obj.message);
			CerrarWaiting();
		});
	}
}

function NuevaObservacion() {
	var tabActiva = $('.nav-tabs .active')[0].id;
	var mensaje = PuedoAgregar(tabActiva);
	if (mensaje !== "") {
		AbrirMensaje("ATENCIÓN", mensaje, function () {
			$("#msjModal").modal("hide");
			return true;
		}, false, ["Aceptar"], "error!", null);
	}
	else {
		var data = {};
		PostGenHtml(data, nuevaObservacionUrl, function (obj) {
			$("#divDatosDeObsSelected").html(obj);
			$(".nav-link").prop("disabled", true);
			$(".activable").prop("disabled", false);
			desactivarGrilla(Grids.GridObs);
			HabilitarBotonesPorAccion(AbmAction.ALTA);
			$("#Observacion_cta_obs").focus();
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

function ModificaFormaDePago(tabAct, mainGrid) {
	var mensaje = PuedoModificar(tabAct);
	if (mensaje !== "") {
		AbrirMensaje("ATENCIÓN", mensaje, function () {
			$("#msjModal").modal("hide");
			return true;
		}, false, ["Aceptar"], "error!", null);
	}
	else {
		HabilitarBotonesPorAccion(AbmAction.MODIFICACION);
		tipoDeOperacion = AbmAction.MODIFICACION;
		SetearDestinoDeOperacion(tabAct);
		$(".nav-link").prop("disabled", true);
		$(".activable").prop("disabled", false);
		$("#listaFP").prop("disabled", true);
		desactivarGrilla(Grids.GridFP);
		desactivarGrilla(mainGrid);
		$("#FormaDePago_fp_dias").focus();
	}
}

function ModificaContacto(tabAct, mainGrid) {
	var mensaje = PuedoModificar(tabAct);
	if (mensaje !== "") {
		activarBotones(false);
		AbrirMensaje("ATENCIÓN", mensaje, function () {
			$("#msjModal").modal("hide");
			return true;
		}, false, ["Aceptar"], "error!", null);
	}
	else {
		HabilitarBotonesPorAccion(AbmAction.MODIFICACION);
		tipoDeOperacion = AbmAction.MODIFICACION;
		SetearDestinoDeOperacion(tabAct);
		$(".nav-link").prop("disabled", true);
		$(".activable").prop("disabled", false);
		desactivarGrilla(Grids.GridOC);
		desactivarGrilla(mainGrid);
		$("#OtroContacto_cta_nombre").prop("disabled", true);
		$("#OtroContacto_cta_celu").focus();
	}
}

function ModificaNota(tabAct, mainGrid) {
	var mensaje = PuedoModificar(tabAct);
	if (mensaje !== "") {
		AbrirMensaje("ATENCIÓN", mensaje, function () {
			$("#msjModal").modal("hide");
			return true;
		}, false, ["Aceptar"], "error!", null);
	}
	else {
		HabilitarBotonesPorAccion(AbmAction.MODIFICACION);
		tipoDeOperacion = AbmAction.MODIFICACION;
		SetearDestinoDeOperacion(tabAct);
		$(".nav-link").prop("disabled", true);
		$(".activable").prop("disabled", false);
		$("#Nota_usu_apellidoynombre").prop("disabled", true);
		desactivarGrilla(Grids.GridNota);
		desactivarGrilla(mainGrid);
		$("#Nota_nota").focus();
	}
}

function ModificaObservacion(tabAct, mainGrid) {
	var mensaje = PuedoModificar(tabAct);
	if (mensaje !== "") {
		AbrirMensaje("ATENCIÓN", mensaje, function () {
			$("#msjModal").modal("hide");
			return true;
		}, false, ["Aceptar"], "error!", null);
	}
	else {
		HabilitarBotonesPorAccion(AbmAction.MODIFICACION);
		tipoDeOperacion = AbmAction.MODIFICACION;
		SetearDestinoDeOperacion(tabAct);
		$(".nav-link").prop("disabled", true);
		$(".activable").prop("disabled", false);
		$("#listaTipoObs").prop("disabled", true);
		desactivarGrilla(Grids.GridObs);
		desactivarGrilla(mainGrid);
		$("#Observacion_cta_obs").focus();
	}
}

function PuedoModificar(tabAct) {
	var mensaje = "";
	switch (tabAct) {
		case Tabs.TabCliente:
			break;
		case Tabs.TabFormasDePago:
			if (!$("#chkCtaActiva").is(":checked"))
				mensaje = "Solo se pueden modificar formas de pago para cuentas activas.";
			if ($("#IdSelected").val() == "")
				mensaje = "Debe seleccionar una forma de pago para modificar.";
			break;
		case Tabs.TabNotas:
			if (!$("#chkCtaActiva").is(":checked"))
				mensaje = "Solo se pueden modificar notas para cuentas activas.";
			if ($("#IdSelected").val() == "")
				mensaje = "Debe seleccionar una nota para modificar.";
			break;
		case Tabs.TabObservaciones:
			if (!$("#chkCtaActiva").is(":checked"))
				mensaje = "Solo se pueden modificar observaciones para cuentas activas.";
			if ($("#IdSelected").val() == "")
				mensaje = "Debe seleccionar una observación para modificar.";
			break;
		case Tabs.TabOtrosContactos:
			if (!$("#chkCtaActiva").is(":checked"))
				mensaje = "Solo se pueden modificar contactos para cuentas activas.";
			if ($("#IdSelected").val() == "")
				mensaje = "Debe seleccionar un contacto para modificar.";
			break;
		case Tabs.TabFamilias:
			if (!$("#chkCtaActiva").is(":checked"))
				mensaje = "Solo se pueden modificar familias para cuentas activas.";
			if ($("#IdSelected").val() == "")
				mensaje = "Debe seleccionar una familia para modificar.";
			break;
		default:
			break;
	}
	return mensaje;
}

function GetMensajeParaBaja(tabActiva) {
	var mensaje = "";
	switch (tabActiva) {
		case Tabs.TabCliente:
			mensaje = "Para eliminar primero debe seleccionar un Cliente."
			break;
		case Tabs.TabProveedor:
			mensaje = "Para eliminar primero debe seleccionar un Proveedor."
			break;
		case Tabs.TabFormasDePago:
			mensaje = "Para eliminar primero debe seleccionar una Forma de Pago."
			break;
		case Tabs.TabNotas:
			mensaje = "Para eliminar primero debe seleccionar una Nota."
			break;
		case Tabs.TabObservaciones:
			mensaje = "Para eliminar primero debe seleccionar una Observación."
			break;
		case Tabs.TabOtrosContactos:
			mensaje = "Para eliminar primero debe seleccionar un Contacto."
			break;
		case Tabs.TabFamilias:
			mensaje = "Para eliminar primero debe seleccionar una Familia."
			break;
		default:
			mensaje = "";
			break;
	}
	return mensaje;
}

function SetearDestinoDeOperacion(tabActiva) {
	switch (tabActiva) {
		case Tabs.TabCliente:
			destinoDeOperacion = AbmObject.CLIENTES;
			break;
		case Tabs.TabProveedor:
			destinoDeOperacion = AbmObject.PROVEEDORES;
			break;
		case Tabs.TabFormasDePago:
			destinoDeOperacion = AbmObject.CLIENTES_CONDICIONES_VTA;
			break;
		case Tabs.TabNotas:
			destinoDeOperacion = AbmObject.CUENTAS_NOTAS;
			break;
		case Tabs.TabObservaciones:
			destinoDeOperacion = AbmObject.CUENTAS_OBSERVACIONES;
			break;
		case Tabs.TabOtrosContactos:
			destinoDeOperacion = AbmObject.CUENTAS_CONTACTOS;
			break;
		case Tabs.TabFamilias:
			destinoDeOperacion = AbmObject.PROVEEDORES_FAMILIA;
			break;
		default:
			destinoDeOperacion = "";
			break;
	}
}

function selectRegCli(e, gridId) {
	selectReg(e, gridId);
	$("#IdSelected").val("");
	switch (gridId) {
		case Grids.GridProveedor:
			if ($("#divDetalle").is(":visible")) {
				$("#divDetalle").collapse("hide");
			}
			$("#btnDetalle").prop("disabled", true);
			activarGrilla(Grids.GridCliente);
			activarBotones(false);
			break;
		case Grids.GridCliente:
			if ($("#divDetalle").is(":visible")) {
				$("#divDetalle").collapse("hide");
			}
			$("#btnDetalle").prop("disabled", true);
			activarGrilla(Grids.GridCliente);
			activarBotones(false);
			break;
		case Grids.GridFP:
			break;
		case Grids.GridOC:
			break;
		case Grids.GridNota:
			break;
		case Grids.GridObs:
			break;
		case Grids.GridFlias:
			break;
		default:
	}
}