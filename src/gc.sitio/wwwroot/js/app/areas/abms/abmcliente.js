$(function () {
	$("#btnCancel").on("click", function () {
		$("#btnFiltro").trigger("click");
	});
	$("#pagEstado").on("change", function () {
		var div = $("#divPaginacion");
		presentaPaginacion(div);
	});
	$(document).on("change", "#listaAfip", controlaValorAfip);
	$(document).on("change", "#listaProvi", controlaValorProvi);
	$(document).on("change", "#listaTipoCanal", controlaValorCanal);
	$(document).on("click", "#chkPivaCertActiva", controlaCertIva);
	$(document).on("click", "#chkIbCertActiva", controlaCertIb);
	$(document).on("click", "#chkCtaEmpActiva", controlaCtaEmp);
	$(document).on("change", "#listaFP", controlaValorFP);
	$("#btnBuscar").on("click", function () { buscarClientes(pagina); });

	//tabCliente
	$("#tabCliente").on("click", function () { SeteaIDClienteSelected(); });
	$("#tabFormaDePago").on("click", function () { BuscarFormaDePago(); });
	$("#tabOtrosContactos").on("click", function () { BuscarOtrosContactos(); });
	$("#tabNotas").on("click", function () { BuscarNotas(); });
	$("#tabObservaciones").on("click", function () { BuscarObservaciones(); });

	/*ABM Botones*/
	$("#btnAbmNuevo").on("click", function () { btnNuevoClick(); });
	$("#btnAbmModif").on("click", function () { btnModiClick(); });
	$("#btnAbmElimi").on("click", function () { btnBajaClick(); });
	$("#btnAbmAceptar").on("click", function () { btnSubmitClick(); });
	$("#btnAbmCancelar").on("click", function () { btnCancelClick(); });


	InicializaPantallaAbmCliente();
	funcCallBack = buscarClientes;
	return true;
});

const AbmObject = {
	CLIENTES: 'clientes', //ABM principal clientes
	CLIENTES_CONDICIONES_VTA: 'cuentas_fp', //ABM relacionado clientes formas de pago
	CUENTAS_CONTACTOS: 'cuentas_contactos', //ABM relacionado contactos
	CUENTAS_NOTAS: 'cuentas_notas', //ABM relacionado notas de clientes
	CUENTAS_OBSERVACIONES: 'cuentas_obs' //ABM relacionado observaciones de clientes
}

const Tabs = {
	TabCliente: 'btnTabCliente',
	TabFormasDePago: 'btnTabFormasDePago',
	TabOtrosContactos: 'btnTabOtrosContactos',
	TabNotas: 'btnTabNotas',
	TabObservaciones: 'btnTabObservaciones'
}

function SeteaIDClienteSelected() {
	$("#IdSelected").val($("#Cliente_Cta_Id").val());
}

function controlaValorFP() {
	if ($("#listaFP option:selected").val() === "B" || $("#listaFP option:selected").val() === "I") {
		$("#listaTipoCueBco").prop("disabled", false);
		$("#FormaDePago_cta_bco_cuenta_nro").prop("disabled", false);
		$("#FormaDePago_cta_bco_cuenta_cbu").prop("disabled", false);
	}
	else {
		$("#listaTipoCueBco").prop("disabled", true);
		$("#FormaDePago_cta_bco_cuenta_nro").prop("disabled", true);
		$("#FormaDePago_cta_bco_cuenta_cbu").prop("disabled", true);
	}
	if ($("#listaFP option:selected").val() === "H") {
		$("#FormaDePago_cta_valores_a_nombre").prop("disabled", false);
	}
	else {
		$("#FormaDePago_cta_valores_a_nombre").prop("disabled", true);
	}
}

function controlaCtaEmp() {
	if ($(this).is(":checked")) {
		$("#Cliente_Cta_Emp_Legajo").prop("disabled", false);
		$("#listaFinancieros").prop("disabled", false);
	}
	else {
		$("#Cliente_Cta_Emp_Legajo").prop("disabled", true);
		$("#listaFinancieros").prop("disabled", true);
	}
}

function controlaCertIb() {
	if ($(this).is(":checked")) {
		$("#Cliente_Pib_Cert_Vto").prop("disabled", false);
	}
	else {
		$("#Cliente_Pib_Cert_Vto").prop("disabled", true);
	}
}

function controlaCertIva() {
	if ($(this).is(":checked")) {
		$("#Cliente_Piva_Cert_Vto").prop("disabled", false);
	}
	else {
		$("#Cliente_Piva_Cert_Vto").prop("disabled", true);
	}
}

function controlaValorCanal() {
	if ($("#listaTipoCanal option:selected").val() === "DI") {
		$("#listaVendedor").prop("disabled", false);
		$("#listaDias").prop("disabled", false);
		$("#listaRepartidor").prop("disabled", false);
	}
	else {
		$("#listaVendedor").prop("disabled", true);
		$("#listaDias").prop("disabled", true);
		$("#listaRepartidor").prop("disabled", true);
	}
}

//function controlaValorAfip() {
//	if ($("#listaAfip option:selected").val() !== "05" && $("#listaAfip option:selected").val() !== "02") {
//		$("#listaIB").prop("disabled", false);
//		$("#Cliente_Cta_Ib_Nro").prop("disabled", false);
//		$("#chkPivaCertActiva").prop("disabled", false);
//		$("#chkIbCertActiva").prop("disabled", false);
//	}
//	else {
//		$("#listaIB").prop("disabled", true);
//		$("#Cliente_Cta_Ib_Nro").prop("disabled", true);
//		$("#chkPivaCertActiva").prop("disabled", true);
//		$("#chkIbCertActiva").prop("disabled", true);
//	}
//}

//function controlaValorProvi() {
//	var provId = $("#listaProvi option:selected").val()
//	if (provId !== "") {
//		var data = { provId }
//		AbrirWaiting();
//		PostGenHtml(data, obtenerDepartamentosUrl, function (obj) {
//			$("#divLocalidad").html(obj);
//			CerrarWaiting();
//		}, function (obj) {
//			ControlaMensajeError(obj.message);
//			CerrarWaiting();
//		});
//	}
//}

//function BuscarObservaciones() {
//	if ($(".nav-link").prop("disabled")) {
//		return false;
//	}
//	if (ctaId != "") {
//		var data = { ctaId };
//		AbrirWaiting();
//		PostGenHtml(data, buscarObservacionesUrl, function (obj) {
//			$("#divObservaciones").html(obj);
//			AgregarHandlerSelectedRow("tbClienteObservaciones");
//			$(".activable").prop("disabled", true);
//			$("#IdSelected").val("");
//			CerrarWaiting();
//		}, function (obj) {
//			ControlaMensajeError(obj.message);
//			CerrarWaiting();
//		});
//	}
//}

//function BuscarNotas() {
//	if ($(".nav-link").prop("disabled")) {
//		return false;
//	}
//	if (ctaId != "") {
//		var data = { ctaId };
//		AbrirWaiting();
//		PostGenHtml(data, buscarNotasUrl, function (obj) {
//			$("#divNotas").html(obj);
//			AgregarHandlerSelectedRow("tbClienteNotas");
//			$(".activable").prop("disabled", true);
//			$("#IdSelected").val("");
//			CerrarWaiting();
//		}, function (obj) {
//			ControlaMensajeError(obj.message);
//			CerrarWaiting();
//		});
//	}
//}

//function BuscarOtrosContactos() {
//	if ($(".nav-link").prop("disabled")) {
//		return false;
//	}
//	if (ctaId != "") {
//		var data = { ctaId };
//		AbrirWaiting();
//		PostGenHtml(data, buscarOtrosContactosUrl, function (obj) {
//			$("#divOtrosContactos").html(obj);
//			AgregarHandlerSelectedRow("tbClienteOtroContacto");
//			$(".activable").prop("disabled", true);
//			$("#IdSelected").val("");
//			CerrarWaiting();
//		}, function (obj) {
//			ControlaMensajeError(obj.message);
//			CerrarWaiting();
//		});
//	}
//}

//function BuscarFormaDePago() {
//	if ($(".nav-link").prop("disabled")) {
//		return false;
//	}
//	if (ctaId != "") {
//		var data = { ctaId };
//		AbrirWaiting();
//		PostGenHtml(data, buscarFormaDePagoUrl, function (obj) {
//			$("#divFormasDePago").html(obj);
//			AgregarHandlerSelectedRow("tbClienteFormaPagoEnTab");
//			$(".activable").prop("disabled", true);
//			$("#IdSelected").val("");
//			CerrarWaiting();
//		}, function (obj) {
//			ControlaMensajeError(obj.message);
//			CerrarWaiting();
//		});
//	}
//}

//function AgregarHandlerSelectedRow(grilla) {
//	document.getElementById(grilla).addEventListener('click', function (e) {
//		if (e.target.nodeName === 'TD') {
//			var selectedRow = this.querySelector('.selected-row');
//			if (selectedRow) {
//				selectedRow.classList.remove('selected-row');
//			}
//			e.target.closest('tr').classList.add('selected-row');
//		}
//	});
//}

function selectOCenTab(x) {
	var tcId = x.cells[5].innerText.trim();
	var data = { ctaId, tcId };
	AbrirWaiting();
	PostGenHtml(data, buscarDatosOtrosContactosUrl, function (obj) {
		$("#divDatosDeOCSelected").html(obj);
		$("#IdSelected").val(tcId);
		$(".activable").prop("disabled", true);
		CerrarWaiting();
	}, function (obj) {
		ControlaMensajeError(obj.message);
		CerrarWaiting();
	});
}

function selectFPenTab(x) {
	var fpId = x.cells[2].innerText.trim();
	var data = { ctaId, fpId };
	AbrirWaiting();
	PostGenHtml(data, buscarDatosFormasDePagoUrl, function (obj) {
		$("#divDatosDeFPSelected").html(obj);
		$("#IdSelected").val($("#FormaDePago_fp_id").val());
		$(".activable").prop("disabled", true);
		CerrarWaiting();
	}, function (obj) {
		ControlaMensajeError(obj.message);
		CerrarWaiting();
	});
}

function selectNotaenTab(x) {
	var usuId = x.cells[3].innerText.trim();
	var data = { ctaId, usuId };
	AbrirWaiting();
	PostGenHtml(data, buscarDatosNotaUrl, function (obj) {
		$("#divDatosDeNotaSelected").html(obj);
		$("#IdSelected").val(usuId);
		$(".activable").prop("disabled", true);
		CerrarWaiting();
	}, function (obj) {
		ControlaMensajeError(obj.message);
		CerrarWaiting();
	});
}

function selectObsEnTab(x) {
	var toId = x.cells[2].innerText.trim();
	var data = { ctaId, toId };
	AbrirWaiting();
	PostGenHtml(data, buscarDatosObservacionesUrl, function (obj) {
		$("#divDatosDeObsSelected").html(obj);
		$("#IdSelected").val(toId);
		$(".activable").prop("disabled", true);
		CerrarWaiting();
	}, function (obj) {
		ControlaMensajeError(obj.message);
		CerrarWaiting();
	});
}

function InicializaPantallaAbmCliente() {
	var tb = $("#tbGridCliente tbody tr");
	if (tb.length === 0) {
		$("#divFiltro").collapse("show")
	}

	$("#lbRel01").text("TIPO");
	$("#lbRel02").text("ZONA");

	$("#lbChkDescr").text("Denominación");
	$("#lbDescr").html("Desc");

	$("#lbChkDesdeHasta").text("ID Cuenta");

	$("#IdSelected").val("");
	$(".activable").prop("disabled", true);
	CerrarWaiting();
	return true;
}

function cargaPaginacion() {
	$("#divPaginacion").pagination({
		items: totalRegs,
		itemsOnPage: pagRegs,
		cssStyle: "dark-theme",
		currentPage: pagina,
		onPageClick: function (num) {
			buscarClientes(num);
		}
	});
	$("#pagEstado").val(false);
	$("#divFiltro").collapse("hide")
	return true;
}

function buscarClientes(pag) {
	AbrirWaiting();
	var buscar = "";
	var id = "";
	var id2 = "";
	var r01 = [];
	var r02 = [];

	if ($("#chkDescr").is(":checked")) {
		buscar = $("#Buscar").val();
	}
	if ($("#chkDesdeHasta").is(":checked")) {
		id = $("#Id").val();
		id2 = $("#Id2").val();
	}
	if ($("#chkRel01").is(":checked")) {
		$("#Rel01List").children().each(function (i, item) { r01.push($(item).val()) });
	}	
	if ($("#chkRel02").is(":checked")) {
		$("#Rel02List").children().each(function (i, item) { r02.push($(item).val()) });
	}

	var data1 = {
		id, id2,
		rel01: r01,
		rel02: r02,
		rel03: [],
		"fechaD": null, //"0001-01-01T00:00:00",
		"fechaH": null, //"0001-01-01T00:00:00",
		buscar
	};

	var buscaNew = JSON.stringify(dataBak) != JSON.stringify(data1)

	if (buscaNew === false) {
		//son iguales las condiciones cambia de pagina
		pagina = pag;
	}
	else {
		dataBak = data1;
		pagina = 1;
		pag = 1;
	}

	var sort = null;
	var sortDir = null

	var data2 = { sort, sortDir, pag, buscaNew }

	var data = $.extend({}, data1, data2);

	PostGenHtml(data, buscarUrl, function (obj) {
		$("#divGrilla").html(obj);
		$("#divFiltro").collapse("hide")
		PostGen({}, buscarMetadataURL, function (obj) {
			if (obj.error === true) {
				AbrirMensaje("ATENCIÓN", obj.msg, function () {
					$("#msjModal").modal("hide");
					return true;
				}, false, ["Aceptar"], "error!", null);
			}
			else {
				totalRegs = obj.metadata.totalCount;
				pags = obj.metadata.totalPages;
				pagRegs = obj.metadata.pageSize;

				$("#pagEstado").val(true).trigger("change");
				$(".activable").prop("disabled", true);
			}

		});
		CerrarWaiting();
	}, function (obj) {
		ControlaMensajeError(obj.message);
		CerrarWaiting();
	});

}

function selectReg(e) {
	$("#tbGridCliente tbody tr").each(function (index) {
		$(this).removeClass("selected-row");
	});
	$(e).addClass("selected-row");
}

function BuscarCliente(ctaId) {
	var data = { ctaId };
	AbrirWaiting();
	PostGenHtml(data, buscarClienteUrl, function (obj) {
		$("#divDatosCliente").html(obj);
		$("#IdSelected").val($("#Cliente_Cta_Id").val());
		$(".activable").prop("disabled", true);
		CerrarWaiting();
	}, function (obj) {
		ControlaMensajeError(obj.message);
		CerrarWaiting();
	});
}

function selectRegDbl(x) {
	AbrirWaiting("Espere mientras se busca el cliente seleccionado...");
	$("#tbGridCliente tbody tr").each(function (index) {
		$(this).removeClass("selectedEdit-row");
	});
	$(x).addClass("selectedEdit-row");

	var cta_id = x.cells[0].innerText.trim();
	if (cta_id !== "") {
		ctaId = cta_id;
		BuscarCliente(cta_id);
		BuscarFormaDePago();
		BuscarOtrosContactos();
		BuscarNotas();
		BuscarObservaciones();
		HabilitarBotones(false, false, false, true, true);
		controlaCertIb();
		$(".activable").prop("disabled", true);
	}
}

//function HabilitarBotonesPorAccion(accion) {
//	switch (accion) {
//		case AbmAction.ALTA:
//			HabilitarBotones(true, true, true, false, false);
//			break;
//		case AbmAction.BAJA:
//			HabilitarBotones(true, true, true, false, false);
//			break;
//		case AbmAction.MODIFICACION:
//			HabilitarBotones(true, true, true, false, false);
//			break;
//		case AbmAction.SUBMIT:
//			HabilitarBotones(false, false, false, true, true);
//			break;
//		case AbmAction.CANCEL:
//			HabilitarBotones(false, false, false, true, true);
//			break;
//		default:
//			HabilitarBotones(false, false, false, true, true);
//			break;
//	}
//}

//function HabilitarBotones(btnAlta, btnBaja, btnModi, btnSubmit, btnCancel) {
//	$("#btnAbmNuevo").prop("disabled", btnAlta);
//	$("#btnAbmModif").prop("disabled", btnModi);
//	$("#btnAbmElimi").prop("disabled", btnBaja);
//	$("#btnAbmAceptar").prop("disabled", btnSubmit);
//	$("#btnAbmCancelar").prop("disabled", btnCancel);
//}

function btnNuevoClick() {

	tipoDeOperacion = AbmAction.ALTA;
	var tabActiva = $('.nav-tabs .active')[0].id;
	SetearDestinoDeOperacion(tabActiva);
	switch (tabActiva) {
		case Tabs.TabCliente:
			NuevoCliente();
			break;
		case Tabs.TabFormasDePago:
			NuevaFormaDePago();
			break;
		case Tabs.TabNotas:
			NuevaNota();
			break;
		case Tabs.TabObservaciones:
			NuevaObservacion();
			break;
		case Tabs.TabOtrosContactos:
			NuevoContacto();
			break;
		default:
			break;
	}
}

function NuevoCliente() {
	var data = {};
	PostGenHtml(data, nuevoClienteUrl, function (obj) {
		$("#divDatosCliente").html(obj);
		$(".nav-link").prop("disabled", true);
		$(".activable").prop("disabled", false);
		$("#chkCtaActiva")[0].checked = true;
		$("#Cliente_Cta_Id").prop("disabled", true);
		HabilitarBotonesPorAccion(AbmAction.ALTA);
		$("#Cliente_Cta_Denominacion").focus();
		CerrarWaiting();
	}, function (obj) {
		ControlaMensajeError(obj.message);
		CerrarWaiting();
	});
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
			HabilitarBotonesPorAccion(AbmAction.ALTA);
			$("#Observacion_cta_obs").focus();
			CerrarWaiting();
		}, function (obj) {
			ControlaMensajeError(obj.message);
			CerrarWaiting();
		});
	}
}

function btnModiClick() {
	var tabActiva = $('.nav-tabs .active')[0].id;
	switch (tabActiva) {
		case Tabs.TabCliente:
			ModificaCliente(tabActiva);
			break;
		case Tabs.TabFormasDePago:
			ModificaFormaDePago(tabActiva);
			break;
		case Tabs.TabNotas:
			ModificaNota(tabActiva);
			break;
		case Tabs.TabObservaciones:
			ModificaObservacion(tabActiva);
			break;
		case Tabs.TabOtrosContactos:
			ModificaContacto(tabActiva);
			break;
		default:
			break;
	}
}

function ModificaCliente(tabAct) {
	HabilitarBotonesPorAccion(AbmAction.MODIFICACION);
	tipoDeOperacion = AbmAction.MODIFICACION;
	SetearDestinoDeOperacion(tabAct);
	$(".nav-link").prop("disabled", true);
	$(".activable").prop("disabled", false);
	$("#tbGridCliente tbody tr td").addClass("disable-table-rows");
	$("#Cliente_Cta_Denominacion").focus();
}

function ModificaFormaDePago(tabAct) {
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
		$("#tbClienteFormaPagoEnTab tbody tr td").prop("disabled", true);
		//
		$("#tbClienteFormaPagoEnTab tbody tr td").addClass("disable-table-rows");
		$("#FormaDePago_fp_dias").focus();
	}
}

function ModificaContacto(tabAct) {
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
		$("#tbClienteOtroContacto tbody tr td").addClass("disable-table-rows");
		$("#OtroContacto_cta_nombre").prop("disabled", true);
		$("#listaTC").prop("disabled", true);
		$("#OtroContacto_cta_celu").focus();
	}
}

function ModificaNota(tabAct) {
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
		$("#tbClienteNotas tbody tr td").addClass("disable-table-rows");
		$("#Nota_nota").focus();
	}
}

function ModificaObservacion(tabAct) {
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
		$("#tbClienteObservaciones tbody tr td").addClass("disable-table-rows");
		$("#Observacion_cta_obs").focus();
	}
}

function btnBajaClick() {
	var idSelected = $("#IdSelected").val();
	if (idSelected === "") {
		AbrirMensaje("ATENCIÓN", "Debe seleccionar un elemento.", function () {
			$("#msjModal").modal("hide");
			return false;
		}, false, ["Aceptar"], "error!", null);

	}
	else {
		HabilitarBotonesPorAccion(AbmAction.BAJA);
		tipoDeOperacion = AbmAction.BAJA;
		$(".activable").prop("disabled", true);
		var tabActiva = $('.nav-tabs .active')[0].id;
		SetearDestinoDeOperacion(tabActiva);
		$(".nav-link").prop("disabled", true);
	}
}

function btnSubmitClick() {
	var tabActiva = $('.nav-tabs .active')[0].id;
	var mensaje = PuedoGuardar(tabActiva);
	if (mensaje === "") {
		HabilitarBotonesPorAccion(AbmAction.SUBMIT);
		Guardar();
	}
}

function PuedoGuardar(tabAct) {
	var mensaje = "";
	switch (tabAct) {
		case Tabs.TabCliente:
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
		default:
			break;
	}
	return mensaje;
	return true;
}

function PuedoAgregar(tabAct) {
	var mensaje = "";
	switch (tabAct) {
		case Tabs.TabCliente:
			break;
		case Tabs.TabFormasDePago:
			if (!$("#chkCtaActiva").is(":checked"))
				mensaje = "Solo se pueden agregar formas de pago para cuentas activas.";
			break;
		case Tabs.TabNotas:
			if (!$("#chkCtaActiva").is(":checked"))
				mensaje = "Solo se pueden agregar formas de pago para cuentas activas.";
			break;
		case Tabs.TabObservaciones:
			if (!$("#chkCtaActiva").is(":checked"))
				mensaje = "Solo se pueden agregar formas de pago para cuentas activas.";
			break;
		case Tabs.TabOtrosContactos:
			if (!$("#chkCtaActiva").is(":checked"))
				mensaje = "Solo se pueden agregar formas de pago para cuentas activas.";
			break;
		default:
			break;
	}
	return mensaje;
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
				mensaje = "Solo se pueden modificar formas de pago para cuentas activas.";
			if ($("#IdSelected").val() == "")
				mensaje = "Debe seleccionar una forma de pago para modificar.";
			break;
		case Tabs.TabObservaciones:
			if (!$("#chkCtaActiva").is(":checked"))
				mensaje = "Solo se pueden modificar formas de pago para cuentas activas.";
			if ($("#IdSelected").val() == "")
				mensaje = "Debe seleccionar una forma de pago para modificar.";
			break;
		case Tabs.TabOtrosContactos:
			if (!$("#chkCtaActiva").is(":checked"))
				mensaje = "Solo se pueden modificar formas de pago para cuentas activas.";
			if ($("#IdSelected").val() == "")
				mensaje = "Debe seleccionar una forma de pago para modificar.";
			break;
		default:
			break;
	}
	return mensaje;
}

function PuedoBorrar(tabAct) {
	switch (tabAct) {
		case Tabs.TabCliente:
			break;
		case Tabs.TabFormasDePago:
			if ($("#chkCtaActiva").is(":checked"))
				return true;
			else
				return false;
			break;
		case Tabs.TabNotas:
			break;
		case Tabs.TabObservaciones:
			break;
		case Tabs.TabOtrosContactos:
			if ($("#chkCtaActiva").is(":checked"))
				return true;
			else
				return false;
			break;
			break;
		default:
			break;
	}
	return true;
}

function btnCancelClick() {
	HabilitarBotonesPorAccion(AbmAction.CANCEL);
	tipoDeOperacion = AbmAction.CANCEL;
	$(".nav-link").prop("disabled", false);
	$(".activable").prop("disabled", true);
	var tabActiva = $('.nav-tabs .active')[0].id;
	switch (tabActiva) {
		case Tabs.TabCliente:
			$("#tbGridCliente tbody tr td").removeClass("disable-table-rows");
			break;
		case Tabs.TabFormasDePago:
			$("#tbClienteFormaPagoEnTab").prop("disabled", false);
			$("#tbClienteFormaPagoEnTab tbody tr td").removeClass("disable-table-rows");
			break;
		case Tabs.TabNotas:
			$("#Nota_usu_apellidoynombre").prop("disabled", false);
			$("#tbClienteNotas tbody tr td").removeClass("disable-table-rows");
			break;
		case Tabs.TabObservaciones:
			$("#tbClienteObservaciones tbody tr td").removeClass("disable-table-rows");
			break;
		case Tabs.TabOtrosContactos:
			$("#tbClienteOtroContacto tbody tr td").removeClass("disable-table-rows");
			$("#OtroContacto_cta_nombre").prop("disabled", false);
			$("#listaTC").prop("disabled", false);
			break;
		default:
			break;
	}
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
		case Tabs.TabFormasDePago:
			break;
		case Tabs.TabNotas:
			break;
		case Tabs.TabObservaciones:
			break;
		case Tabs.TabOtrosContactos:
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
				$("#tbGridCliente tbody tr td").removeClass("disable-table-rows");
				url = dataOpsClienteUrl;
				break;
			case AbmObject.CLIENTES_CONDICIONES_VTA:
				$("#tbClienteFormaPagoEnTab tbody tr td").removeClass("disable-table-rows");
				url = dataOpsFormaDePagoUrl;
				break;
			case AbmObject.CUENTAS_CONTACTOS:
				$("#tbClienteOtroContacto tbody tr td").removeClass("disable-table-rows");
				$("#OtroContacto_cta_nombre").prop("disabled", false);
				$("#listaTC").prop("disabled", false);
				url = dataOpsCuentaContactoUrl;
				break;
			case AbmObject.CUENTAS_NOTAS:
				$("#Nota_usu_apellidoynombre").prop("disabled", false);
				$("#tbClienteNotas tbody tr td").removeClass("disable-table-rows");
				url = dataOpsCuentaNotaUrl;
				break;
			case AbmObject.CUENTAS_OBSERVACIONES:
				$("#tbClienteObservaciones tbody tr td").removeClass("disable-table-rows");
				url = dataOpsObservacionesUrl;
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
			}

		});
	}
}

function ObtenerDatosParaJson(destinoDeOperacion, tipoDeOperacion) {
	var json = "";
	switch (destinoDeOperacion) {
		case AbmObject.CLIENTES:
			json = ObtenerDatosDeClienteParaJson();
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
		default:
	}
	return json;
}

function ObtenerDatosDeObsParaJson(destinoDeOperacion, tipoDeOperacion) {
	var cta_id = $("#Cliente_Cta_Id").val();
	var to_id = $("#listaTipoObs").val();
	var to_desc = $("#listaTipoObs option:selected").text();
	var to_lista = $("#listaTipoObs option:selected").text() + "(" + $("#listaTipoObs").val() + ")";
	var cta_obs = $("#Observacion_cta_obs").val();
	var data = { cta_id, to_id, to_desc, to_lista, cta_obs, destinoDeOperacion, tipoDeOperacion };
	return data;
}

function ObtenerDatosDeNotasParaJson(destinoDeOperacion, tipoDeOperacion) {
	var cta_id = $("#Cliente_Cta_Id").val();
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

function ObtenerDatosDeClienteParaJson() {
	var cta_id = $("#Cliente_Cta_Id").val();
	var cta_denominacion = $("#Cliente_Cta_Denominacion").val();
	var tdoc_id = $("#listaTipoDoc").val();
	var tdoc_desc = $("#listaTipoDoc option:selected").text();
	var cta_documento = $("#Cliente_Cta_Documento").val();
	var cta_domicilio = $("#Cliente_Cta_Domicilio").val();
	var cta_localidad = $("#listaDepto option:selected").text();
	var cta_cpostal = $("#Cliente_Cta_Cpostal").val();
	var prov_id = $("#listaProvi").val();
	var prov_nombre = $("#listaProvi option:selected").text();
	var dep_id = $("#listaDepto").val();
	var dep_nombre = $("#listaDepto option:selected").text();
	var afip_id = $("#listaAfip").val();
	var afip_desc = $("#listaAfip option:selected").text();
	var cta_ib_nro = $("#Cliente_Cta_Ib_Nro").val();
	var ib_id = $("#listaIB").val();
	var ib_desc = $("#listaIB option:selected").text();
	var nj_id = $("#listaNJ").val();
	var nj_desc = $("#listaNJ option:selected").text();
	var cta_alta = null;
	var cta_cuit_vto = null;
	var cta_emp = "N";
	if ($("#chkCtaEmpActiva")[0].checked)
		cta_emp = "S";
	var cta_emp_legajo = $("#Cliente_Cta_Emp_Legajo").val();
	var cta_emp_ctaf = $("#listaFinancieros option:selected").text();
	var cta_actu_fecha = null;
	var cta_actu = null;
	var ctac_tope_credito = $("#Cliente_Ctac_Tope_Credito").val();
	var ctac_tope_credito_dia = $("#Cliente_Ctac_Tope_Credito_Dia").val();
	var ctac_dto_operacion = $("#Cliente_Ctac_Dto_Operacion").val();
	var ctac_dto_operacion_dia = $("#Cliente_Ctac_Dto_Operacion_Dia").val();
	var piva_cert = "N";
	var pib_cert = "N";
	if ($("#listaAfip option:selected").val() !== "05" && $("#listaAfip option:selected").val() !== "02") {
		if ($("#chkPivaCertActiva")[0].checked)
			piva_cert = "S";

		if ($("#chkIbCertActiva")[0].checked)
			pib_cert = "S";
	}
	var piva_cert_vto = $("#Cliente_Piva_Cert_Vto").val();
	var pib_cert_vto = $("#Cliente_Pib_Cert_Vto").val();
	var ctn_id = $("#listaTipoNeg").val();
	var ctn_desc = $("#listaTipoNeg option:selected").text();
	var ctc_id = $("#listaTipoCanal").val();
	var ctc_desc = $("#listaTipoCanal option:selected").text();
	var ve_id = $("#listaVendedor").val();
	var ve_nombre = $("#listaVendedor option:selected").text();
	var ve_visita = $("#listaDias option:selected").text();
	var zn_id = $("#listaZonas").val();
	var zn_desc = $("#listaZonas option:selected").text();
	var rp_id = $("#listaRepartidor").val();
	var rp_nombre = $("#listaRepartidor option:selected").text();
	var ctac_tope_credito_dia_ult = null;
	var ctac_dto_operacion_dia_ult = null;
	var ctac_habilitada = "N";
	if ($("#chkCtaActiva")[0].checked)
		ctac_habilitada = "S";
	var ctac_ptos_vtas = $("#Cliente_Ctac_Ptos_Vtas").val();
	var ctac_negocio_inicio = null;
	var lp_id = $("#Cliente_Lp_Id").val();
	var lp_desc = $("#Cliente_Lp_Id option:selected").text();
	var data = {
		cta_id, cta_denominacion, tdoc_id, tdoc_desc, cta_documento, cta_domicilio, cta_localidad, cta_cpostal, prov_id, prov_nombre, dep_id, dep_nombre, afip_id, afip_desc, cta_ib_nro,
		ib_id, ib_desc, cta_alta, cta_cuit_vto, cta_emp, cta_emp_legajo, cta_emp_ctaf, cta_actu_fecha, cta_actu, ctac_tope_credito, ctac_ptos_vtas, ctac_negocio_inicio, ctac_tope_credito_dia,
		ctac_dto_operacion, ctac_dto_operacion_dia, piva_cert, piva_cert_vto, pib_cert, pib_cert_vto, ctn_id, ctn_desc, ctc_id, ctac_tope_credito_dia_ult, ctac_dto_operacion_dia_ult, ctc_desc,
		ve_id, ve_nombre, ve_visita, zn_id, zn_desc, rp_id, rp_nombre, ctac_habilitada, nj_id, nj_desc, lp_id, lp_desc, destinoDeOperacion, tipoDeOperacion
	};
	return data;
}

function SetearDestinoDeOperacion(tabActiva) {
	switch (tabActiva) {
		case Tabs.TabCliente:
			destinoDeOperacion = AbmObject.CLIENTES;
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
		default:
			destinoDeOperacion = "";
			break;
	}
}