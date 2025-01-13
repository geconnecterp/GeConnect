$(function () {
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
	//$("#btnDetalle").on("click", function () { btnDetalleClick(); });
	$("#btnDetalle").on("mousedown", analizaEstadoBtnDetalle); 

	$("#btnDetalle").prop("disabled", true);
	$("#btnCancel").on("click", function () {
		$("#btnFiltro").trigger("click");
	});
	$("#btnBuscar").on("click", function () {
		//es nueva la busqueda no resguardamos la busqueda anterior. es util para paginado
		dataBak = "";
		//es una busqueda por filtro. siempre sera pagina 1
		pagina = 1;
		buscarClientes(pagina);
	});

	$(".inputEditable").on("keypress", analizaEnterInput);
	$("#btnAbmAceptar").hide();
	$("#btnAbmCancelar").hide();

	InicializaPantallaAbmCliente();
	funcCallBack = buscarClientes;
	return true;
});

function SeteaIDClienteSelected() {
	$("#IdSelected").val($("#Cliente_Cta_Id").val());
}

function analizaEstadoBtnDetalle() {
	var res = $("#divDetalle").hasClass("show");
	if (res === true) {
		selectRegCli(regSelected, Grids.GridCliente);
	}
	return true;

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
		HabilitarBotones(false, true, true, true, true);
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
function selectRegDbl(x, gridId) {
	AbrirWaiting("Espere mientras se busca el cliente seleccionado...");
	$("#" + gridId + " tbody tr").each(function (index) {
		$(this).removeClass("selectedEdit-row");
	});
	$(x).addClass("selectedEdit-row");
	regSelected = x;

	switch (gridId) {
		case Grids.GridCliente:
			var cta_id = x.cells[0].innerText.trim();
			if (cta_id !== "") {
				ctaId = cta_id;
				BuscarCliente(cta_id);
				BuscarFormaDePago();
				BuscarOtrosContactos();
				BuscarNotas();
				BuscarObservaciones();
				activarBotones(true);
				controlaCertIb();
				$(".activable").prop("disabled", true);
				$("#btnDetalle").prop("disabled", false);
				$("#divFiltro").collapse("hide");
				$("#divDetalle").collapse("show");
			}
			break;
		case Grids.GridFP:
			var fpId = x.cells[2].innerText.trim();
			var data = { ctaId, fpId };
			AbrirWaiting();
			PostGenHtml(data, buscarDatosFormasDePagoUrl, function (obj) {
				$("#divDatosDeFPSelected").html(obj);
				$("#IdSelected").val($("#FormaDePago_fp_id").val());
				$(".activable").prop("disabled", true);
				activarBotones(true);
				CerrarWaiting();
			}, function (obj) {
				ControlaMensajeError(obj.message);
				CerrarWaiting();
			});
			break;
		case Grids.GridOC:
			var tcId = x.cells[5].innerText.trim();
			var data = { ctaId, tcId };
			AbrirWaiting();
			PostGenHtml(data, buscarDatosOtrosContactosUrl, function (obj) {
				$("#divDatosDeOCSelected").html(obj);
				$("#IdSelected").val(tcId);
				$(".activable").prop("disabled", true);
				activarBotones(true);
				CerrarWaiting();
			}, function (obj) {
				ControlaMensajeError(obj.message);
				CerrarWaiting();
			});
			break;
		case Grids.GridNota:
			var usuId = x.cells[3].innerText.trim();
			var data = { ctaId, usuId };
			AbrirWaiting();
			PostGenHtml(data, buscarDatosNotaUrl, function (obj) {
				$("#divDatosDeNotaSelected").html(obj);
				$("#IdSelected").val(usuId);
				$(".activable").prop("disabled", true);
				activarBotones(true);
				CerrarWaiting();
			}, function (obj) {
				ControlaMensajeError(obj.message);
				CerrarWaiting();
			});
			break;
		case Grids.GridObs:
			var toId = x.cells[2].innerText.trim();
			var data = { ctaId, toId };
			AbrirWaiting();
			PostGenHtml(data, buscarDatosObservacionesUrl, function (obj) {
				$("#divDatosDeObsSelected").html(obj);
				$("#IdSelected").val(toId);
				$(".activable").prop("disabled", true);
				activarBotones(true);
				CerrarWaiting();
			}, function (obj) {
				ControlaMensajeError(obj.message);
				CerrarWaiting();
			});
			break;
		default:
	}
}

function btnNuevoClick() {

	tipoDeOperacion = AbmAction.ALTA;
	var tabActiva = $('.nav-tabs .active')[0].id;
	SetearDestinoDeOperacion(tabActiva);
	$("#btnAbmAceptar").show();
	$("#btnAbmCancelar").show();
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
		desactivarGrilla(Grids.GridCliente);
		$("#Cliente_Cta_Denominacion").focus();
		CerrarWaiting();
	}, function (obj) {
		ControlaMensajeError(obj.message);
		CerrarWaiting();
	});
}

function btnModiClick() {
	var tabActiva = $('.nav-tabs .active')[0].id;
	$("#btnAbmAceptar").show();
	$("#btnAbmCancelar").show();
	switch (tabActiva) {
		case Tabs.TabCliente:
			ModificaCliente(tabActiva);
			break;
		case Tabs.TabFormasDePago:
			ModificaFormaDePago(tabActiva, Grids.GridCliente);
			break;
		case Tabs.TabNotas:
			ModificaNota(tabActiva, Grids.GridCliente);
			break;
		case Tabs.TabObservaciones:
			ModificaObservacion(tabActiva, Grids.GridCliente);
			break;
		case Tabs.TabOtrosContactos:
			ModificaContacto(tabActiva, Grids.GridCliente);
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
	desactivarGrilla(Grids.GridCliente);
	$("#Cliente_Cta_Denominacion").focus();
}
function btnBajaClick() {
	var tabActiva = $('.nav-tabs .active')[0].id;
	$("#btnAbmAceptar").show();
	$("#btnAbmCancelar").show();
	var idSelected = $("#IdSelected").val();
	if (idSelected === "") {
		AbrirMensaje("ATENCIÓN", GetMensajeParaBaja(tabActiva), function () {
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
		switch (tabActiva) {
			case Tabs.TabCliente:
				desactivarGrilla(Grids.GridCliente);
				break;
			case Tabs.TabFormasDePago:
				desactivarGrilla(Grids.GridFP);
				break;
			case Tabs.TabNotas:
				desactivarGrilla(Grids.GridNota);
				break;
			case Tabs.TabObservaciones:
				desactivarGrilla(Grids.GridObs);
				break;
			case Tabs.TabOtrosContactos:
				desactivarGrilla(Grids.GridOC);
				break;
			default:
				break;
		}
	}
}

function btnSubmitClick() {
	var tabActiva = $('.nav-tabs .active')[0].id;
	var mensaje = PuedoGuardar(tabActiva);
	if (mensaje === "") {
		//HabilitarBotonesPorAccion(AbmAction.SUBMIT);
		//$("#btnAbmAceptar").hide();
		//$("#btnAbmCancelar").hide();
		Guardar();
	}
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
				mensaje = "Solo se pueden agregar notas para cuentas activas.";
			break;
		case Tabs.TabObservaciones:
			if (!$("#chkCtaActiva").is(":checked"))
				mensaje = "Solo se pueden agregar observaciones para cuentas activas.";
			break;
		case Tabs.TabOtrosContactos:
			if (!$("#chkCtaActiva").is(":checked"))
				mensaje = "Solo se pueden agregar contactos para cuentas activas.";
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
	$("#btnAbmAceptar").hide();
	$("#btnAbmCancelar").hide();
	var tabActiva = $('.nav-tabs .active')[0].id;
	switch (tabActiva) {
		case Tabs.TabCliente:
			activarGrilla(Grids.GridCliente);
			break;
		case Tabs.TabFormasDePago:
			$("#tbClienteFormaPagoEnTab").prop("disabled", false);
			activarGrilla(Grids.GridFP);
			activarGrilla(Grids.GridCliente);
			break;
		case Tabs.TabNotas:
			$("#Nota_usu_apellidoynombre").prop("disabled", false);
			activarGrilla(Grids.GridNota);
			activarGrilla(Grids.GridCliente);
			break;
		case Tabs.TabObservaciones:
			activarGrilla(Grids.GridObs);
			activarGrilla(Grids.GridCliente);
			break;
		case Tabs.TabOtrosContactos:
			activarGrilla(Grids.GridOC);
			activarGrilla(Grids.GridCliente);
			$("#OtroContacto_cta_nombre").prop("disabled", false);
			$("#listaTC").prop("disabled", false);
			break;
		default:
			break;
	}
}

function ObtenerDatosDeClienteParaJson(destinoDeOperacion, tipoDeOperacion) {
	var cta_id = $("#Cliente_Cta_Id").val();
	var cta_denominacion = $("#Cliente_Cta_Denominacion").val();
	var tdoc_id = $("#listaTipoDoc").val();
	var tdoc_desc = $("#listaTipoDoc option:selected").text();
	var cta_documento = $("#Cliente_Cta_Documento").val();
	var cta_domicilio = $("#Cliente_Cta_Domicilio").val();
	var cta_localidad = $("#Cliente_Cta_Localidad").text();
	var cta_cpostal = $("#Cliente_Cta_Cpostal").val();
	var prov_id = $("#listaProvi").val();
	var prov_nombre = $("#listaProvi option:selected").text();
	var dep_id = $("#listaDepto").val();
	var dep_nombre = $("#listaDepto option:selected").text();
	var cta_www = $("#Cliente_Cta_Www").val();
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
		cta_id, cta_denominacion, tdoc_id, tdoc_desc, cta_documento, cta_domicilio, cta_localidad, cta_cpostal, prov_id, prov_nombre, dep_id, dep_nombre, cta_www, afip_id, afip_desc, cta_ib_nro,
		ib_id, ib_desc, cta_alta, cta_cuit_vto, cta_emp, cta_emp_legajo, cta_emp_ctaf, cta_actu_fecha, cta_actu, ctac_tope_credito, ctac_ptos_vtas, ctac_negocio_inicio, ctac_tope_credito_dia,
		ctac_dto_operacion, ctac_dto_operacion_dia, piva_cert, piva_cert_vto, pib_cert, pib_cert_vto, ctn_id, ctn_desc, ctc_id, ctac_tope_credito_dia_ult, ctac_dto_operacion_dia_ult, ctc_desc,
		ve_id, ve_nombre, ve_visita, zn_id, zn_desc, rp_id, rp_nombre, ctac_habilitada, nj_id, nj_desc, lp_id, lp_desc, destinoDeOperacion, tipoDeOperacion
	};
	return data;
}