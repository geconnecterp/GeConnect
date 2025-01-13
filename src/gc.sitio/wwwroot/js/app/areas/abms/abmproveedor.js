$(function () {
	$("#pagEstado").on("change", function () {
		var div = $("#divPaginacion");
		presentaPaginacion(div);
	});

	$(document).on("change", "#listaAfip", controlaValorAfip);
	$(document).on("change", "#listaProvi", controlaValorProvi);
	$("#btnBuscar").on("click", function () { buscarProveedores(pagina); });

	//tabProveedor
	$("#tabProveedor").on("click", function () { SeteaIDProvSelected(); });
	$("#tabFormaDePago").on("click", function () { BuscarFormaDePago(); });
	$("#tabOtrosContactos").on("click", function () { BuscarOtrosContactos(); });
	$("#tabNotas").on("click", function () { BuscarNotas(); });
	$("#tabObservaciones").on("click", function () { BuscarObservaciones(); });
	$("#tabFliaProv").on("click", function () { BuscarFamilias(); });

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
		buscarProveedores(pagina);
	});

	$(".inputEditable").on("keypress", analizaEnterInput);
	$("#btnAbmAceptar").hide();
	$("#btnAbmCancelar").hide();

	InicializaPantallaAbmProveedor();
	funcCallBack = buscarProveedores;
	return true;
});

function btnNuevoClick() {
	tipoDeOperacion = AbmAction.ALTA;
	var tabActiva = $('.nav-tabs .active')[0].id;
	SetearDestinoDeOperacion(tabActiva);
	$("#btnAbmAceptar").show();
	$("#btnAbmCancelar").show();
	switch (tabActiva) {
		case Tabs.TabProveedor:
			NuevoProveedor();
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
		case Tabs.TabFamilias:
			NuevaFamilia();
			break;
		default:
			break;
	}
}

function NuevoProveedor() {
	var data = {};
	PostGenHtml(data, nuevoProveedorUrl, function (obj) {
		$("#divDatosProveedor").html(obj);
		$(".nav-link").prop("disabled", true);
		$(".activable").prop("disabled", false);
		$("#chkCtaActiva")[0].checked = true;
		$("#Proveedor_Cta_Id").prop("disabled", true);
		HabilitarBotonesPorAccion(AbmAction.ALTA);
		desactivarGrilla(Grids.GridProveedor);
		$("#Proveedor_Cta_Denominacion").focus();
		CerrarWaiting();
	}, function (obj) {
		ControlaMensajeError(obj.message);
		CerrarWaiting();
	});
}

function NuevaFamilia() {
	var data = {};
	PostGenHtml(data, nuevaFamiliaUrl, function (obj) {
		$("#divDatosDeFamiliaSelected").html(obj);
		$(".nav-link").prop("disabled", true);
		$(".activable").prop("disabled", false);
		HabilitarBotonesPorAccion(AbmAction.ALTA);
		desactivarGrilla(Grids.GridFlias);
		$("#ProveedorGrupo_pg_id").focus();
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
		case Tabs.TabProveedor:
			ModificaProveedor(tabActiva);
			break;
		case Tabs.TabFormasDePago:
			ModificaFormaDePago(tabActiva, Grids.GridProveedor);
			break;
		case Tabs.TabNotas:
			ModificaNota(tabActiva, Grids.GridProveedor);
			break;
		case Tabs.TabObservaciones:
			ModificaObservacion(tabActiva, Grids.GridProveedor);
			break;
		case Tabs.TabOtrosContactos:
			ModificaContacto(tabActiva, Grids.GridProveedor);
			break;
		case Tabs.TabFamilias:
			ModificaFamilia(tabActiva, Grids.GridProveedor);
			break;
		default:
			break;
	}
}

function ModificaProveedor(tabAct) {
	HabilitarBotonesPorAccion(AbmAction.MODIFICACION);
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
		HabilitarBotonesPorAccion(AbmAction.MODIFICACION);
		tipoDeOperacion = AbmAction.MODIFICACION;
		SetearDestinoDeOperacion(tabAct);
		$(".nav-link").prop("disabled", true);
		$(".activable").prop("disabled", false);
		$("#ProveedorGrupo_pg_id").prop("disabled", true);
		desactivarGrilla(Grids.GridFlias);
		desactivarGrilla(mainGrid);
		$("#ProveedorGrupo_pg_desc").focus();
	}
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
			case Tabs.TabProveedor:
				desactivarGrilla(Grids.GridProveedor);
				break;
			case Tabs.TabFormasDePago:
				desactivarGrilla(Grids.GridProveedor);
				desactivarGrilla(Grids.GridFP);
				break;
			case Tabs.TabNotas:
				desactivarGrilla(Grids.GridProveedor);
				desactivarGrilla(Grids.GridNota);
				break;
			case Tabs.TabObservaciones:
				desactivarGrilla(Grids.GridProveedor);
				desactivarGrilla(Grids.GridObs);
				break;
			case Tabs.TabOtrosContactos:
				desactivarGrilla(Grids.GridProveedor);
				desactivarGrilla(Grids.GridOC);
				break;
			case Tabs.TabFamilias:
				desactivarGrilla(Grids.GridProveedor);
				desactivarGrilla(Grids.GridFlias);
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

function btnCancelClick() {
	HabilitarBotonesPorAccion(AbmAction.CANCEL);
	tipoDeOperacion = AbmAction.CANCEL;
	$(".nav-link").prop("disabled", false);
	$(".activable").prop("disabled", true);
	$("#btnAbmAceptar").hide();
	$("#btnAbmCancelar").hide();
	var tabActiva = $('.nav-tabs .active')[0].id;
	switch (tabActiva) {
		case Tabs.TabProveedor:
			activarGrilla(Grids.GridProveedor);
			break;
		case Tabs.TabFormasDePago:
			$("#tbClienteFormaPagoEnTab").prop("disabled", false);
			activarGrilla(Grids.GridFP);
			activarGrilla(Grids.GridProveedor);
			break;
		case Tabs.TabNotas:
			$("#Nota_usu_apellidoynombre").prop("disabled", false);
			activarGrilla(Grids.GridNota);
			activarGrilla(Grids.GridProveedor);
			break;
		case Tabs.TabObservaciones:
			activarGrilla(Grids.GridObs);
			activarGrilla(Grids.GridProveedor);
			break;
		case Tabs.TabOtrosContactos:
			activarGrilla(Grids.GridOC);
			activarGrilla(Grids.GridProveedor);
			$("#OtroContacto_cta_nombre").prop("disabled", false);
			$("#listaTC").prop("disabled", false);
			break;
		case Tabs.TabFamilias:
			activarGrilla(Grids.GridFlias);
			activarGrilla(Grids.GridProveedor);
			break;
		default:
			break;
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

function buscarProveedores(pag) {
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

function BuscarFamilias() {
	if ($(".nav-link").prop("disabled")) {
		return false;
	}
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
	$("#tbGridProveedor tbody tr").each(function (index) {
		$(this).removeClass("selectedEdit-row");
	});
	$(x).addClass("selectedEdit-row");

	switch (gridId) {
		case Grids.GridProveedor:
			var cta_id = x.cells[0].innerText.trim();
			if (cta_id !== "") {
				ctaId = cta_id;
				BuscarProveedor(cta_id);
				BuscarFormaDePago();
				BuscarOtrosContactos();
				BuscarNotas();
				BuscarObservaciones();
				//HabilitarBotones(false, false, false, true, true);
				activarBotones(true);
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

function PuedoAgregar(tabAct) {
	var mensaje = "";
	switch (tabAct) {
		case Tabs.TabProveedor:
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
		case Tabs.TabFamilias:
			break;
		default:
			break;
	}
	return mensaje;
}

function ObtenerDatosDeProveedorFamiliaParaJson(destinoDeOperacion, tipoDeOperacion) {
	var cta_id = $("#Proveedor_Cta_Id").val();
	var pg_id = $("#ProveedorGrupo_pg_id").val();
	var pg_desc = $("#ProveedorGrupo_pg_desc").val();
	var pg_lista = $("#ProveedorGrupo_pg_desc").val() + "(" + $("#ProveedorGrupo_pg_id").val() + ")";
	var data = { cta_id, pg_id, pg_desc, pg_lista, destinoDeOperacion, tipoDeOperacion };
	return data;
}

function ObtenerDatosDeProveedorParaJson() {
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
	var ctac_habilitada = "N";
	if ($("#chkCtaActiva")[0].checked)
		ctac_habilitada = "S";
	
	var data = {
		cta_id, cta_denominacion, tdoc_id, tdoc_desc, cta_documento, cta_domicilio, cta_localidad, cta_cpostal, prov_id, prov_nombre, dep_id, dep_nombre, cta_www, afip_id, afip_desc, nj_id, nj_desc, cta_ib_nro,
		ib_id, ib_desc, cta_alta, cta_cuit_vto, cta_emp, cta_emp_legajo, cta_emp_ctaf, cta_actu_fecha, cta_actu, tp_id, ctap_ean, ctap_id_externo, ctap_rgan, rgan_id, rgan_cert, rgan_cert_vto, rgan_porc, 
		ctap_rib, rib_id, rib_cert, rib_cert_vto, rib_porc, ctap_ret_iva, ctap_ret_iva_porc, ctap_per_iva, ctap_per_iva_ali, ctap_per_ib, ctap_per_ib_ali, ctap_pago_susp, ctap_devolucion, ctap_devolucion_flete, 
		ctap_acuenta_dev, ctap_d1, ctap_d2, ctap_d3, ctap_d4, ctap_d5, ctap_d6, ope_iva, ope_iva_descripcion, ctag_id, ctag_denominacion, ctac_habilitada, destinoDeOperacion, tipoDeOperacion
	};
	return data;
}