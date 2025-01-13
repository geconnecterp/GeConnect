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

//const Tabs = {
//	TabProveedor: 'btnTabProveedor',
//	TabFormasDePago: 'btnTabFormasDePago',
//	TabOtrosContactos: 'btnTabOtrosContactos',
//	TabNotas: 'btnTabNotas',
//	TabObservaciones: 'btnTabObservaciones',
//	TabFamilias: 'btnTabFliaProv'
//}

//const Grids = {
//	GridProveedor: 'tbGridProveedor',
//	GridFP: 'tbClienteFormaPagoEnTab',
//	GridOC: 'tbClienteOtroContacto',
//	GridNota: 'tbClienteNotas',
//	GridObs: 'tbClienteObservaciones',
//	GridFlias: 'tbProveedorFliaProv'
//}

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

function btnSubmitClick() { }

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