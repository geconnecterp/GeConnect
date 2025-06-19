$(function () {
	$("#pagEstado").on("change", function () {
		var div = $("#divPaginacion");
		presentaPaginacion(div);
	});

	$(document).on("change", "#listaAfip", controlaValorAfip);
	$(document).on("change", "#listaProvi", controlaValorProvi);
	$(document).on("change", "#listaFP", controlaValorFP);

	$(document).on("dblclick", "#" + Grids.GridProveedor + " tbody tr", function () {
		x = $(this);
		ejecutaDblClickGrid(x, Grids.GridProveedor);
	});

	$("#btnBuscar").on("click", function () { buscarProveedores(pagina); });

	//tabProveedor
	$("#tabProveedor").on("click", function () { SeteaIDProvSelected(); });
	$("#tabFormaDePago").on("click", function () { BuscarFormaDePagoTabClick(); });
	$("#tabOtrosContactos").on("click", function () { BuscarOtrosContactosTabClick(); });
	$("#tabNotas").on("click", function () { BuscarNotasTabClick(); });
	$("#tabObservaciones").on("click", function () { BuscarObservacionesTabClick(); });
	$("#tabFliaProv").on("click", function () { BuscarFamiliasTabClick(); });

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
		buscarProveedores(pagina);
	});

	$(".inputEditable").on("keypress", analizaEnterInput);
	$("#btnAbmAceptar").hide();
	$("#btnAbmCancelar").hide();

	InicializaPantallaAbmProveedor();
	funcCallBack = buscarProveedores;
	return true;
});

function activarControles(band) { }

function NuevoProveedor() {
	var data = {};
	PostGenHtml(data, nuevoProveedorUrl, function (obj) {
		$("#divDatosProveedor").html(obj);
		$(".nav-link").prop("disabled", true);
		$(".activable").prop("disabled", false);
		$("#chkCtaActiva")[0].checked = true;
		$("#Proveedor_Cta_Id").prop("disabled", true);
		desactivarGrilla(Grids.GridProveedor);
		accionBotones(AbmAction.ALTA, Tabs.TabProveedor);
		$("#divFiltro").collapse("hide");
		$("#divDetalle").collapse("show");
		$("#Proveedor_Cta_Denominacion").focus();
		$("#GridsEnTabPrincipal").hide();
		CerrarWaiting();
	}, function (obj) {
		ControlaMensajeError(obj.message);
		CerrarWaiting();
	});
}

function NuevaFamilia() {
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
		PostGenHtml(data, nuevaFamiliaUrl, function (obj) {
			$("#divDatosDeFamiliaSelected").html(obj);
			$(".nav-link").prop("disabled", true);
			$(".activable").prop("disabled", false);
			accionBotones(AbmAction.ALTA, tabActiva);
			desactivarGrilla(Grids.GridFlias);
			$("#ProveedorGrupo_Pg_Id").prop("disabled", true);
			$("#ProveedorGrupo_Pg_Desc").focus();
			CerrarWaiting();
		}, function (obj) {
			ControlaMensajeError(obj.message);
			CerrarWaiting();
		});
	}
}

function ModificaProveedor(tabAct) {
	accionBotones(AbmAction.MODIFICACION, tabAct);
	tipoDeOperacion = AbmAction.MODIFICACION;
	SetearDestinoDeOperacion(tabAct);
	$(".nav-link").prop("disabled", true);
	$(".activable").prop("disabled", false);
	desactivarGrilla(Grids.GridProveedor);
	$("#Proveedor_Cta_Denominacion").focus();
}

function ModificaFamilia(tabAct, mainGrid) {
	var mensaje = PuedoModificar(tabAct);
	if (mensaje !== "") {
		AbrirMensaje("ATENCIÓN", mensaje, function () {
			$("#msjModal").modal("hide");
			return true;
		}, false, ["Aceptar"], "error!", null);
	}
	else {
		accionBotones(AbmAction.MODIFICACION, Tabs.TabFamilias);
		tipoDeOperacion = AbmAction.MODIFICACION;
		SetearDestinoDeOperacion(tabAct);
		$(".nav-link").prop("disabled", true);
		$(".activable").prop("disabled", false);
		$("#ProveedorGrupo_Pg_Id").prop("disabled", true);
		desactivarGrilla(Grids.GridFlias);
		desactivarGrilla(mainGrid);
		$("#ProveedorGrupo_Pg_Desc").focus();
	}
}

function btnDetalleClick() {
}

function analizaEstadoBtnDetalle() {
	var res = $("#divDetalle").hasClass("show");
	if (res === true) {
		selectRegProv(regSelected, Grids.GridProveedor);
	}
	return true;

}

function SeteaIDProvSelected() {
	$("#IdSelected").val($("#Proveedor_Cta_Id").val());
}

function InicializaPantallaAbmProveedor() {
	var tb = $("#tbGridProveedor tbody tr");
	if (tb.length === 0) {
		$("#divFiltro").collapse("show")
	}

	$("#lbRel01").text("TIPO OPE");

	$("#chkRel02").hide();
	$("#lbRel02").hide();
	$("#lbNombreRel02").hide();
	$("#Rel02").hide();
	$("#Rel02List").hide();

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
			buscarProveedores(num);
		}
	});
	$("#pagEstado").val(false);
	$("#divFiltro").collapse("hide")
	return true;
}

function buscarProveedores(pag, esBaja = false) {
	AbrirWaiting();
	var buscar = "";
	var id = "";
	var id2 = "";
	var r01 = [];

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

	var data1 = {
		id, id2,
		rel01: r01,
		rel02: [],
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

function BuscarFamiliasTabClick() {
	if ($(".nav-link").prop("disabled")) {
		return false;
	}
	BuscarFamilias();
}

function BuscarFamilias() {
	if (ctaId != "") {
		var data = { ctaId };
		AbrirWaiting();
		PostGenHtml(data, buscarFamiliasUrl, function (obj) {
			$("#divFliaProv").html(obj);
			AgregarHandlerSelectedRow("tbProveedorFliaProv");
			$(".activable").prop("disabled", true);
			$("#IdSelected").val("");
			CerrarWaiting();
		}, function (obj) {
			ControlaMensajeError(obj.message);
			CerrarWaiting();
		});
	}
}

function selectRegProv(e, gridId) {

	selectReg(e, gridId);

	switch (gridId) {
		case Grids.GridProveedor:
			if ($("#divDetalle").is(":visible")) {
				$("#divDetalle").collapse("hide");
			}
			$("#btnDetalle").prop("disabled", true);
			activarGrilla(Grids.GridProveedor);
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

function selectRegDbl(x, gridId) {
	AbrirWaiting("Espere mientras se busca el proveedor seleccionado...");
	$("#" + gridId + " tbody tr").each(function (index) {
		$(this).removeClass("selectedEdit-row");
	});
	$(x).addClass("selectedEdit-row");
	regSelected = x;

	switch (gridId) {
		case Grids.GridProveedor:
			var cta_id = x[0].cells[0].innerText.trim();
			if (cta_id !== "") {
				ctaIdRow = x[0];
				ctaId = cta_id;
				BuscarProveedor(cta_id);
				BuscarFormaDePago();
				BuscarOtrosContactos();
				BuscarNotas();
				BuscarObservaciones();
				activarBotones(true);
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
		case Grids.GridFlias:
			var pgId = x.cells[0].innerText.trim();
			var data = { ctaId, pgId };
			AbrirWaiting();
			PostGenHtml(data, buscarDatosFamiliasUrl, function (obj) {
				$("#divDatosDeFamiliaSelected").html(obj);
				$("#IdSelected").val(pgId);
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

function BuscarProveedor(ctaId) {
	var data = { ctaId };
	AbrirWaiting();
	PostGenHtml(data, buscarProveedorUrl, function (obj) {
		$("#divDatosProveedor").html(obj);
		$("#IdSelected").val($("#ProveedorGrupo_Pg_Id").val());
		$(".activable").prop("disabled", true);
		CerrarWaiting();
	}, function (obj) {
		ControlaMensajeError(obj.message);
		CerrarWaiting();
	});
}

function controlaValorFP() {
	if ($("#listaFP option:selected").val() === "B" || $("#listaFP option:selected").val() === "I") {
		$("#listaTipoCueBco").prop("disabled", false);
		$("#FormaDePago_Cta_Bco_Cuenta_Nro").prop("disabled", false);
		$("#FormaDePago_Cta_Bco_Cuenta_Cbu").prop("disabled", false);
	}
	else {
		$("#listaTipoCueBco").prop("disabled", true);
		$("#FormaDePago_Cta_Bco_Cuenta_Nro").val("");
		$("#FormaDePago_Cta_Bco_Cuenta_Nro").prop("disabled", true);
		$("#FormaDePago_Cta_Bco_Cuenta_Cbu").val("");
		$("#FormaDePago_Cta_Bco_Cuenta_Cbu").prop("disabled", true);
	}
	if ($("#listaFP option:selected").val() === "H") {
		$("#FormaDePago_Cta_Valores_A_Nombre").prop("disabled", false);
	}
	else {
		$("#FormaDePago_Cta_Valores_A_Nombre").val("");
		$("#FormaDePago_Cta_Valores_A_Nombre").prop("disabled", true);
	}
}

function ObtenerDatosDeProveedorFamiliaParaJson(destinoDeOperacion, tipoDeOperacion) {
	var cta_id = $("#Proveedor_Cta_Id").val();
	var pg_id = $("#ProveedorGrupo_Pg_Id").val();
	var pg_desc = $("#ProveedorGrupo_Pg_Desc").val();
	var pg_lista = $("#ProveedorGrupo_Pg_Desc").val() + "(" + $("#ProveedorGrupo_Pg_Id").val() + ")";
	var data = { cta_id, pg_id, pg_desc, pg_lista, destinoDeOperacion, tipoDeOperacion };
	return data;
}

function ObtenerDatosDeProveedorParaJson(destinoDeOperacion, tipoDeOperacion) {
	var cta_id = $("#Proveedor_Cta_Id").val();
	var cta_denominacion = $("#Proveedor_Cta_Denominacion").val();
	var tdoc_id = $("#listaTipoDoc").val();
	var tdoc_desc = $("#listaTipoDoc option:selected").text();
	var cta_documento = $("#Proveedor_Cta_Documento").val();
	var cta_domicilio = $("#Proveedor_Cta_Domicilio").val();
	var cta_localidad = $("#Proveedor_Cta_Localidad").text();
	var cta_cpostal = $("#Proveedor_Cta_Cpostal").val();
	var prov_id = $("#listaProvi").val();
	var prov_nombre = $("#listaProvi option:selected").text();
	var dep_id = $("#listaDepto").val();
	var dep_nombre = $("#listaDepto option:selected").text();
	var cta_www = $("#Proveedor_Cta_Www").val();
	var afip_id = $("#listaAfip").val();
	var afip_desc = $("#listaAfip option:selected").text();
	var nj_id = $("#listaNJ").val();
	var nj_desc = $("#listaNJ option:selected").text();
	var cta_ib_nro = $("#Proveedor_Cta_Ib_Nro").val();
	var ib_id = $("#listaIB").val();
	var ib_desc = $("#listaIB option:selected").text();
	var cta_alta = null;
	var cta_cuit_vto = $("#Proveedor_Cta_Cuit_Vto").text();
	var cta_emp = "N";
	var cta_emp_legajo = null;
	var cta_emp_ctaf = null;
	var cta_actu_fecha = null;
	var cta_actu = null;
	var tp_id = $("#listaTipoOc").val();
	var ctap_ean = $("#Proveedor_Ctap_Ean").val();
	var ctap_id_externo = $("#Proveedor_Ctap_Id_Externo").val();
	var ctap_rgan = 'N';
	if ($("#chkCtapRganBool")[0].checked)
		ctap_rgan = "S";
	var rgan_id = $("#listaTipoRetGan").val();
	var rgan_cert = 'N';
	if ($("#chkCtapRganCertBool")[0].checked)
		rgan_cert = "S";
	var rgan_cert_vto = $("#Proveedor_Rgan_Cert_Vto").val();
	var rgan_porc = $("#Proveedor_Rgan_Porc").val();
	var ctap_rib = 'N';
	if ($("#chkCtapRibBool")[0].checked)
		ctap_rib = "S";
	var rib_id = $("#listaTipoRetIB").val();
	var rib_cert = 'N';
	if ($("#chkCtapRibCertBool")[0].checked)
		rib_cert = "S";
	var rib_cert_vto = $("#Proveedor_Rib_Cert_Vto").val();
	var rib_porc = $("#Proveedor_Rib_Porc").val();
	var ctap_ret_iva = 'N';
	if ($("#chkCtapRetIvaBool")[0].checked)
		ctap_ret_iva = "S";
	var ctap_ret_iva_porc = $("#Proveedor_Ctap_Ret_Iva_Porc").val();
	var ctap_per_iva = 'N';
	if ($("#chkCtapPerIvaBool")[0].checked)
		ctap_per_iva = "S";
	var ctap_per_iva_ali = $("#Proveedor_Ctap_Per_Iva_Ali").val();
	var ctap_per_ib = 'N';
	if ($("#chkCtapPerIbBool")[0].checked)
		ctap_per_ib = "S";
	var ctap_per_ib_ali = $("#Proveedor_Ctap_Per_Iva_Ali").val();
	var ctap_pago_susp = 'N';
	if ($("#chkCtapPagoSuspBool")[0].checked)
		ctap_pago_susp = "S";
	var ctap_devolucion = 'N';
	if ($("#chkCtapDevolucionBool")[0].checked)
		ctap_devolucion = "S";
	var ctap_devolucion_flete = 'N';
	if ($("#chkCtapDevolucionFleteBool")[0].checked)
		ctap_devolucion_flete = "S";
	var ctap_acuenta_dev = 'N';
	if ($("#chkCtapACuentaDevBool")[0].checked)
		ctap_acuenta_dev = "S";
	var ctap_d1 = $("#Proveedor_Ctap_D1").val();
	var ctap_d2 = $("#Proveedor_Ctap_D2").val();
	var ctap_d3 = $("#Proveedor_Ctap_D3").val();
	var ctap_d4 = $("#Proveedor_Ctap_D4").val();
	var ctap_d5 = $("#Proveedor_Ctap_D5").val();
	var ctap_d6 = $("#Proveedor_Ctap_D6").val();
	var ope_iva = $("#listaOpe").val();
	var ope_iva_descripcion = $("#listaOpe option:selected").text();
	var ctag_id = $("#listaTipoGasto").val();
	var ctag_denominacion = $("#listaTipoGasto option:selected").text();
	var ctap_habilitada = "N";
	if ($("#chkCtaActiva")[0].checked)
		ctap_habilitada = "S";
	
	var data = {
		cta_id, cta_denominacion, tdoc_id, tdoc_desc, cta_documento, cta_domicilio, cta_localidad, cta_cpostal, prov_id, prov_nombre, dep_id, dep_nombre, cta_www, afip_id, afip_desc, nj_id, nj_desc, cta_ib_nro,
		ib_id, ib_desc, cta_alta, cta_cuit_vto, cta_emp, cta_emp_legajo, cta_emp_ctaf, cta_actu_fecha, cta_actu, tp_id, ctap_ean, ctap_id_externo, ctap_rgan, rgan_id, rgan_cert, rgan_cert_vto, rgan_porc, 
		ctap_rib, rib_id, rib_cert, rib_cert_vto, rib_porc, ctap_ret_iva, ctap_ret_iva_porc, ctap_per_iva, ctap_per_iva_ali, ctap_per_ib, ctap_per_ib_ali, ctap_pago_susp, ctap_devolucion, ctap_devolucion_flete, 
		ctap_acuenta_dev, ctap_d1, ctap_d2, ctap_d3, ctap_d4, ctap_d5, ctap_d6, ope_iva, ope_iva_descripcion, ctag_id, ctag_denominacion, ctap_habilitada, destinoDeOperacion, tipoDeOperacion
	};
	return data;
}