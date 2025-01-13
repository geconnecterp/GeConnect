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

function PuedoGuardar(tabAct) {
	var mensaje = "";
	switch (tabAct) {
		case Tabs.TabCliente:
			break;
		case Tabs.TabProveedor:
			break;
		case Tabs.TabFormasDePago:
			if ($("#FormaDePago_fp_dias").val() < 0)
				mensaje = "El valor de Plazo debe ser igual o mayor a 0.";
			break;
		case Tabs.TabNotas:
			break;
		case Tabs.TabObservaciones:
			break;
		case Tabs.TabOtrosContactos:
			break;
		case Tabs.TabFamilias:
			break;
		default:
			break;
	}
	return mensaje;
	return true;
}

function validarCampos() {
	var tabActiva = $('.nav-tabs .active')[0].id;
	switch (tabActiva) {
		case Tabs.TabCliente:
			if ($("#listaAfip option:selected").val() !== "05" && $("#listaAfip option:selected").val() !== "02") {
				if ($("#Cliente_Ctac_Ptos_Vtas").val() <= 0) {
					var textoSelected = $("#listaAfip option:selected").text();
					AbrirMensaje("ATENCIÓN", "La Cantidad de PV debe ser mayor a 0 cuando ha seleccionado " + textoSelected + " como Condición AFIP.", function () {
						$("#msjModal").modal("hide");
						return false;
					}, false, ["Aceptar"], "error!", null);
					return false;
				}
			}
			break;
		case Tabs.TabProveedor:
			break;
		case Tabs.TabFormasDePago:
			break;
		case Tabs.TabNotas:
			break;
		case Tabs.TabObservaciones:
			break;
		case Tabs.TabOtrosContactos:
			break;
		case Tabs.TabFamilias:
			break;
		default:
			break;
	}

	return true;
}

function Guardar() {
	if (validarCampos()) {
		var url = "";
		switch (destinoDeOperacion) {
			case AbmObject.CLIENTES:
				activarGrilla(Grids.GridCliente);
				url = dataOpsClienteUrl;
				break;
			case AbmObject.PROVEEDORES:
				activarGrilla(Grids.GridProveedor);
				url = dataOpsProveedorUrl;
				break;
			case AbmObject.CLIENTES_CONDICIONES_VTA:
				activarGrilla(Grids.GridFP);
				url = dataOpsFormaDePagoUrl;
				break;
			case AbmObject.CUENTAS_CONTACTOS:
				activarGrilla(Grids.GridOC);
				$("#OtroContacto_cta_nombre").prop("disabled", false);
				$("#listaTC").prop("disabled", false);
				url = dataOpsCuentaContactoUrl;
				break;
			case AbmObject.CUENTAS_NOTAS:
				$("#Nota_usu_apellidoynombre").prop("disabled", false);
				activarGrilla(Grids.GridNota);
				url = dataOpsCuentaNotaUrl;
				break;
			case AbmObject.CUENTAS_OBSERVACIONES:
				activarGrilla(Grids.GridObs);
				url = dataOpsObservacionesUrl;
				break;
			case AbmObject.PROVEEDORES_FAMILIA:
				activarGrilla(Grids.GridFlias);
				url = dataOpsFamiliaUrl;
				break;
			default:
		}
		var data = ObtenerDatosParaJson(destinoDeOperacion, tipoDeOperacion);
		PostGen(data, url, function (obj) {
			if (obj.error === true) {
				AbrirMensaje("ATENCIÓN", obj.msg, function () {
					$("#msjModal").modal("hide");
					$(".nav-link").prop("disabled", false);
					$(".activable").prop("disabled", false);
					return true;
				}, false, ["Aceptar"], "error!", null);
			}
			else {
				AbrirMensaje("ATENCIÓN", obj.msg, function () {
					$("#msjModal").modal("hide");
					$(".nav-link").prop("disabled", false);
					$(".activable").prop("disabled", false);
					return true;
				}, false, ["Aceptar"], "succ!", null);
				activarBotones(false);
				ActualizarDatosEnGrilla(destinoDeOperacion);
			}

		});
	}
}

function ActualizarDatosEnGrilla(destinoDeOperacion) {
	switch (destinoDeOperacion) {
		case AbmObject.CLIENTES:

			break;
		case AbmObject.PROVEEDORES:
			
			break;
		case AbmObject.CLIENTES_CONDICIONES_VTA:
			BuscarFormaDePago();
			break;
		case AbmObject.CUENTAS_CONTACTOS:
			BuscarOtrosContactos();
			break;
		case AbmObject.CUENTAS_NOTAS:
			BuscarNotas();
			break;
		case AbmObject.CUENTAS_OBSERVACIONES:
			BuscarObservaciones();
			break;
		case AbmObject.PROVEEDORES_FAMILIA:
			BuscarFamilias();
			break;
		default:
	}
}

function ObtenerDatosParaJson(destinoDeOperacion, tipoDeOperacion) {
	var json = "";
	switch (destinoDeOperacion) {
		case AbmObject.CLIENTES:
			json = ObtenerDatosDeClienteParaJson(destinoDeOperacion, tipoDeOperacion);
			break;
		case AbmObject.PROVEEDORES:
			json = ObtenerDatosDeProveedorParaJson(destinoDeOperacion, tipoDeOperacion);
			break;
		case AbmObject.CLIENTES_CONDICIONES_VTA:
			json = ObtenerDatosDeFormaDePagoPParaJson(destinoDeOperacion, tipoDeOperacion);
			break;
		case AbmObject.CUENTAS_CONTACTOS:
			json = ObtenerDatosDeOtrosContactosParaJson(destinoDeOperacion, tipoDeOperacion);
			break;
		case AbmObject.CUENTAS_NOTAS:
			json = ObtenerDatosDeNotasParaJson(destinoDeOperacion, tipoDeOperacion);
			break;
		case AbmObject.CUENTAS_OBSERVACIONES:
			json = ObtenerDatosDeObsParaJson(destinoDeOperacion, tipoDeOperacion);
			break;
		case AbmObject.PROVEEDORES_FAMILIA:
			json = ObtenerDatosDeProveedorFamiliaParaJson(destinoDeOperacion, tipoDeOperacion);
			break;
		default:
	}
	return json;
}

function ObtenerDatosDeObsParaJson(destinoDeOperacion, tipoDeOperacion) {
	var cta_id = $("#Cliente_Cta_Id").val();
	if (cta_id == undefined || cta_id == null)
		cta_id = $("#Proveedor_Cta_Id").val();
	var to_id = $("#listaTipoObs").val();
	var to_desc = $("#listaTipoObs option:selected").text();
	var to_lista = $("#listaTipoObs option:selected").text() + "(" + $("#listaTipoObs").val() + ")";
	var cta_obs = $("#Observacion_cta_obs").val();
	var data = { cta_id, to_id, to_desc, to_lista, cta_obs, destinoDeOperacion, tipoDeOperacion };
	return data;
}

function ObtenerDatosDeNotasParaJson(destinoDeOperacion, tipoDeOperacion) {
	var cta_id = $("#Cliente_Cta_Id").val();
	if (cta_id == undefined || cta_id == null)
		cta_id = $("#Proveedor_Cta_Id").val();
	var usu_id = $("#Nota_usu_apellidoynombre").val();
	var usu_apellidoynombre = $("#Nota_usu_apellidoynombre").val();
	var usu_lista = $("#Nota_usu_apellidoynombre").val();
	var fecha = null;
	var nota = $("#Nota_nota").val();
	var data = { cta_id, usu_id, usu_apellidoynombre, usu_lista, fecha, nota, destinoDeOperacion, tipoDeOperacion };
	return data;
}

function ObtenerDatosDeOtrosContactosParaJson(destinoDeOperacion, tipoDeOperacion) {
	var cta_id = $("#Cliente_Cta_Id").val();
	if (cta_id == undefined || cta_id == null)
		cta_id = $("#Proveedor_Cta_Id").val();
	var tc_id = $("#listaTC").val();
	var tc_desc = $("#listaTC option:selected").text();
	var tc_lista = $("#listaTC option:selected").text() + "(" + $("#listaTC").val() + ")";
	var cta_celu = $("#OtroContacto_cta_celu").val();
	var cta_te = $("#OtroContacto_cta_te").val();
	var cta_email = $("#OtroContacto_cta_email").val();
	var cta_nombre = $("#OtroContacto_cta_nombre").val();
	var data = { cta_id, tc_id, tc_desc, tc_lista, cta_celu, cta_te, cta_email, cta_nombre, destinoDeOperacion, tipoDeOperacion };
	return data;
}

function ObtenerDatosDeFormaDePagoPParaJson(destinoDeOperacion, tipoDeOperacion) {
	var cta_id = $("#Cliente_Cta_Id").val();
	if (cta_id == undefined || cta_id == null)
		cta_id = $("#Proveedor_Cta_Id").val();
	var fp_id = $("#listaFP").val();
	var fp_desc = $("#listaFP option:selected").text();
	var fp_lista = $("#listaFP option:selected").text() + "(" + $("#listaFP").val() + ")";
	var fp_dias = $("#FormaDePago_fp_dias").val();
	var tcb_id = $("#listaTipoCueBco").val();
	var tcb_desc = $("#listaTipoCueBco option:selected").val();
	var tcb_lista = $("#listaTipoCueBco option:selected").text() + "(" + $("#listaTipoCueBco").val() + ")";
	var cta_bco_cuenta_nro = $("#FormaDePago_cta_bco_cuenta_nro").val();
	var cta_bco_cuenta_cbu = $("#FormaDePago_cta_bco_cuenta_cbu").val();
	var cta_valores_a_nombre = $("#FormaDePago_cta_valores_a_nombre").val();
	var cta_obs = $("#FormaDePago_cta_obs").val();
	var fp_default = "N";
	var data = { cta_id, fp_id, fp_desc, fp_lista, fp_dias, tcb_id, tcb_desc, tcb_lista, cta_bco_cuenta_nro, cta_bco_cuenta_cbu, cta_valores_a_nombre, cta_obs, fp_default, destinoDeOperacion, tipoDeOperacion };
	return data;
}