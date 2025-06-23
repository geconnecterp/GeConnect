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

	$(document).on("dblclick", "#" + Grids.GridCliente + " tbody tr", function () {
		x = $(this);
		ejecutaDblClickGrid(x, Grids.GridCliente);
	});

	//tabCliente
	$("#tabCliente").on("click", function () { SeteaIDClienteSelected(); });
	$("#tabFormaDePago").on("click", function () { BuscarFormaDePagoTabClick(); });
	$("#tabOtrosContactos").on("click", function () { BuscarOtrosContactosTabClick(); });
	$("#tabNotas").on("click", function () { BuscarNotasTabClick(); });
	$("#tabObservaciones").on("click", function () { BuscarObservacionesTabClick(); });

	/*ABM Botones*/
	$("#btnAbmNuevo").on("click", function () { btnNuevoClick(); });
	$("#btnAbmModif").on("click", function () { btnModiClick(); });
	$("#btnAbmElimi").on("click", function () { btnBajaClick(); });
	$("#btnAbmAceptar").on("click", function () { btnSubmitClick(); });
	$("#btnAbmCancelar").on("click", function () { btnCancelClick(); });

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

function activarControles(band) { }
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
		$("#FormaDePago_Cta_Bco_Cuenta_Nro").prop("disabled", false);
		$("#FormaDePago_Cta_Bco_Cuenta_Nro").show();
		$("#Cta_Bco_Cuenta_Nro_Lbl").show();
		$("#FormaDePago_Cta_Bco_Cuenta_Cbu").prop("disabled", false);
		$("#FormaDePago_Cta_Bco_Cuenta_Cbu").show();
		$("#Cta_Bco_Cuenta_Cbu_Lbl").show();
	}
	else {
		$("#listaTipoCueBco").prop("disabled", true);
		$("#FormaDePago_Cta_Bco_Cuenta_Nro").val("");
		$("#FormaDePago_Cta_Bco_Cuenta_Nro").prop("disabled", true);
		$("#FormaDePago_Cta_Bco_Cuenta_Nro").hide();
		$("#Cta_Bco_Cuenta_Nro_Lbl").hide();
		$("#FormaDePago_Cta_Bco_Cuenta_Cbu").val("");
		$("#FormaDePago_Cta_Bco_Cuenta_Cbu").prop("disabled", true);
		$("#FormaDePago_Cta_Bco_Cuenta_Cbu").hide();
		$("#Cta_Bco_Cuenta_Cbu_Lbl").hide();
	}
	if ($("#listaFP option:selected").val() === "H") {
		$("#FormaDePago_Cta_Valores_A_Nombre").prop("disabled", false);
		$("#FormaDePago_Cta_Valores_A_Nombre").show();
		$("#Cta_Valores_A_Nombre_Lbl").show();
	}
	else {
		$("#FormaDePago_Cta_Valores_A_Nombre").val("");
		$("#FormaDePago_Cta_Valores_A_Nombre").prop("disabled", true);
		$("#FormaDePago_Cta_Valores_A_Nombre").hide();
		$("#Cta_Valores_A_Nombre_Lbl").hide();
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
	activarBotones(false);
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

function buscarClientes(pag, esBaja = false) {
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
	if (esBaja)
		buscaNew = true;

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
			var cta_id = x[0].cells[0].innerText.trim();
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
				$("#IdSelected").val(ctaId);
				posicionarRegOnTop(x);
			}
			break;
		case Grids.GridFP:
			var fpId = x.cells[2].innerText.trim();
			var data = { ctaId, fpId };
			AbrirWaiting();
			PostGenHtml(data, buscarDatosFormasDePagoUrl, function (obj) {
				$("#divDatosDeFPSelected").html(obj);
				$("#IdSelected").val(fpId);
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

function NuevoCliente() {
	var data = {};
	PostGenHtml(data, nuevoClienteUrl, function (obj) {
		$("#divDatosCliente").html(obj);
		$(".nav-link").prop("disabled", true);
		$(".activable").prop("disabled", false);
		$("#chkCtaActiva")[0].checked = true;
		$("#Cliente_Cta_Id").prop("disabled", true);
		desactivarGrilla(Grids.GridCliente);
		accionBotones(AbmAction.ALTA, Tabs.TabCliente);
		$("#divFiltro").collapse("hide");
		$("#divDetalle").collapse("show");
		$("#Cliente_Cta_Denominacion").focus();
		$("#GridsEnTabPrincipal").hide();
		CerrarWaiting();
	}, function (obj) {
		ControlaMensajeError(obj.message);
		CerrarWaiting();
	});
}

function ModificaCliente(tabAct) {
	accionBotones(AbmAction.MODIFICACION, tabAct);
	tipoDeOperacion = AbmAction.MODIFICACION;
	SetearDestinoDeOperacion(tabAct);
	$(".nav-link").prop("disabled", true);
	$(".activable").prop("disabled", false);
	desactivarGrilla(Grids.GridCliente);
	$("#Cliente_Cta_Denominacion").focus();
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
	var ctac_tope_credito =parseFloat($("#Cliente_Ctac_Tope_Credito").val())||0.00;
	var ctac_tope_credito_dia = parseFloat($("#Cliente_Ctac_Tope_Credito_Dia").val())||0.00;
	var ctac_dto_operacion = parseFloat($("#Cliente_Ctac_Dto_Operacion").val())||0;
	var ctac_dto_operacion_dia = parseFloat($("#Cliente_Ctac_Dto_Operacion_Dia").val())||0;
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
	var ctac_ptos_vtas = parseInt($("#Cliente_Ctac_Ptos_Vtas").val())||0;
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