$(function () {
	$("#btnCancel").on("click", function () {
		$("#btnFiltro").trigger("click");
	});
	$("#pagEstado").on("change", function () {
		var div = $("#divPaginacion");
		presentaPaginacion(div);
	});
	$("#btnBuscar").on("click", function () { buscarClientes(pagina); });

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


	InicializaPantallaAbmProd();
	funcCallBack = buscarClientes;
	return true;
});

const AbmObject = {
	CLIENTES: 'clientes', //ABM principal clientes
	CLIENTES_CONDICIONES_VTA: 'clientes_condiciones_vtas', //ABM relacionado clientes formas de pago
	CUENTAS_CONTACTOS: 'cuentas_contactos', //ABM relacionado contactos
	CUENTAS_NOTAS: 'cuentas_notas', //ABM relacionado notas de clientes
	CUENTAS_OBSERVACIONES: 'cuentas_observaciones' //ABM relacionado observaciones de clientes
}

const AbmAction = {
	ALTA: 'A',
	BAJA: 'B',
	MODIFICACION: 'M',
	SUBMIT: 'S',
	CANCEL: 'C'
}

const Tabs = {
	TabCliente: 'btnTabCliente',
	TabFormasDePago: 'tabFormasDePago',
	TabOtrosContactos: 'tabOtrosContactos',
	TabNotas: 'tabNotas',
	TabObservaciones: 'tabObservaciones'
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
			CerrarWaiting();
		}, function (obj) {
			ControlaMensajeError(obj.message);
			CerrarWaiting();
		});
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
			CerrarWaiting();
		}, function (obj) {
			ControlaMensajeError(obj.message);
			CerrarWaiting();
		});
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

function selectOCenTab(x) {
	var tcId = x.cells[5].innerText.trim();
	var data = { ctaId, tcId };
	AbrirWaiting();
	PostGenHtml(data, buscarDatosOtrosContactosUrl, function (obj) {
		$("#divDatosDeOCSelected").html(obj);
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
		CerrarWaiting();
	}, function (obj) {
		ControlaMensajeError(obj.message);
		CerrarWaiting();
	});
}

function InicializaPantallaAbmProd() {
	var tb = $("#tbGridCliente tbody tr");
	if (tb.length === 0) {
		$("#divFiltro").collapse("show")
	}

	$("#lbRel01").text("TIPO");
	$("#lbRel02").text("ZONA");

	$("#lbChkDescr").text("Denominación");
	$("#lbDescr").html("Desc");

	$("#lbChkDesdeHasta").text("ID Cuenta");

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
	}
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

function HabilitarBotones(btnAlta, btnBaja, btnModi, btnSubmit, btnCancel) {
	$("#btnAbmNuevo").prop("disabled", btnAlta);
	$("#btnAbmModif").prop("disabled", btnModi);
	$("#btnAbmElimi").prop("disabled", btnBaja);
	$("#btnAbmAceptar").prop("disabled", btnSubmit);
	$("#btnAbmCancelar").prop("disabled", btnCancel);
}

function btnNuevoClick() {
	HabilitarBotonesPorAccion(AbmAction.ALTA);
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
		$("#Cliente_Cta_Id").prop("disabled", true);
		$("#Cliente_Cta_Denominacion").focus();
		CerrarWaiting();
	}, function (obj) {
		ControlaMensajeError(obj.message);
		CerrarWaiting();
	});
}

function btnModiClick() {
	HabilitarBotonesPorAccion(AbmAction.MODIFICACION);
	tipoDeOperacion = AbmAction.MODIFICACION;
	var tabActiva = $('.nav-tabs .active')[0].id;
	SetearDestinoDeOperacion(tabActiva);
	$(".nav-link").prop("disabled", true);
	$("#Cliente_Cta_Denominacion").focus();
}

function btnBajaClick() {
	HabilitarBotonesPorAccion(AbmAction.BAJA);
	tipoDeOperacion = AbmAction.BAJA;
}

function btnSubmitClick() {
	HabilitarBotonesPorAccion(AbmAction.SUBMIT);
	Guardar();
}

function btnCancelClick() {
	HabilitarBotonesPorAccion(AbmAction.CANCEL);
	tipoDeOperacion = AbmAction.CANCEL;
	$(".nav-link").prop("disabled", false);
}

function Guardar() {
	var jsonString = ObtenerDatosParaJson(destinoDeOperacion);
	var data = { destinoDeOperacion, tipoDeOperacion, jsonString }
	PostGen(data, dataOpsClienteUrl, function (obj) {
		if (obj.error === true) {
			AbrirMensaje("ATENCIÓN", obj.msg, function () {
				$("#msjModal").modal("hide");
				return true;
			}, false, ["Aceptar"], "error!", null);
		}
		else {
			//Do something
		}

	});
}

function ObtenerDatosParaJson(destinoDeOperacion) {
	var json = "";
	switch (destinoDeOperacion) {
		case AbmObject.CLIENTES:
			json = ObtenerDatosDeClienteParaJson();
			break;
		case AbmObject.CLIENTES_CONDICIONES_VTA:
			break;
		case AbmObject.CUENTAS_CONTACTOS:
			break;
		case AbmObject.CUENTAS_NOTAS:
			break;
		case AbmObject.CUENTAS_OBSERVACIONES:
			break;
		default:
	}
	return json;
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
	var cta_alta = $("#listaAfip option:selected").text(); //?
	var cta_cuit_vto = $("#listaAfip option:selected").text(); //?
	var cta_emp = $("#listaAfip option:selected").text();//?
	var cta_actu_fecha = $("#listaAfip option:selected").text();//?
	var cta_actu = $("#listaAfip option:selected").text();//?
	var ctac_tope_credito = $("#Cliente_Ctac_Tope_Credito").val();
	var ctac_tope_credito_dia = $("#Cliente_Ctac_Tope_Credito_Dia").val();
	var ctac_dto_operacion = $("#Cliente_Ctac_Dto_Operacion").val();
	var ctac_dto_operacion_dia = $("#Cliente_Ctac_Dto_Operacion_Dia").val();
	var piva_cert = "N";
	if ($("#chkPivaCertActiva")[0].checked)
		piva_cert = "S";
	var pib_cert = "N";
	if ($("#chkIbCertActiva")[0].checked)
		pib_cert = "S";
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
	var ctac_habilitada = "N";
	if ($("#chkCtaActiva")[0].checked)
		ctac_habilitada = "S";
	var data = `{ "cta_id": "` + cta_id + `", "cta_denominacion": "` + cta_denominacion + `", "tdoc_id": "` + tdoc_id + `", "tdoc_desc": "` + tdoc_desc + `"
				, "cta_documento": "`+ cta_documento + `", "cta_domicilio": "` + cta_domicilio + `", "cta_localidad": "` + cta_localidad + `"
				, "cta_cpostal": "`+ cta_cpostal + `", "prov_id": "` + prov_id + `", "prov_nombre": "` + prov_nombre + `", "dep_id": "` + dep_id + `"
				, "dep_nombre": "`+ dep_nombre + `", "afip_id": "` + afip_id + `", "afip_desc": "` + afip_desc + `", "cta_ib_nro": "` + cta_ib_nro + `"
				, "ib_id": "`+ ib_id + `", "ib_desc": "` + ib_desc + `", "cta_alta": "` + cta_alta + `", "cta_cuit_vto": "` + cta_cuit_vto + `"
				, "cta_emp": "`+ cta_emp + `", "cta_actu_fecha": "` + cta_actu_fecha + `", "cta_actu": "` + cta_actu + `", "ctac_tope_credito": "` + ctac_tope_credito + `"
				, "ctac_tope_credito_dia": "`+ ctac_tope_credito_dia + `", "ctac_dto_operacion": "` + ctac_dto_operacion + `", "ctac_dto_operacion_dia": "` + ctac_dto_operacion_dia + `"
				, "piva_cert": "`+ piva_cert + `", "pib_cert": "` + pib_cert + `", "ctn_id": "` + ctn_id + `", "ctn_desc": "` + ctn_desc + `", "ctc_id": "` + ctc_id + `"
				, "ctc_desc": "`+ ctc_desc + `", "ve_id": "` + ve_id + `", "ve_nombre": "` + ve_nombre + `", "ve_visita": "` + ve_visita + `", "zn_id": "` + zn_id + `",
				, "zn_desc": "`+ zn_desc + `", "rp_id": "` + rp_id + `", "rp_nombre": "` + rp_nombre + `", "ctac_habilitada": "` + ctac_habilitada + `"}`;
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