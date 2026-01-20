$(function () {
	$("#pagEstado").on("change", function () {
		var div = $("#divPaginacion");
		presentaPaginacion(div);
	});

	$(document).on("dblclick", "#" + Grids.GridMedioDePago + " tbody tr", function () {
		x = $(this);
		ejecutaDblClickGrid(x, Grids.GridMedioDePago);
	});

	$("#tabMedioDePago").on("click", function () { SeteaInsIdSelected(); });
	$("#tabOpcionesCuotas").on("click", function () { BuscarOpcionesCuotasTabClick(); });
	$("#tabCuentaFinContable").on("click", function () { BuscarCuentaFinContableTabClick(); });
	$("#tabPos").on("click", function () { BuscarPosTabClick(); });

	/*ABM Botones*/
	$("#btnAbmNuevo").on("click", function () { btnNuevoClick(); });
	$("#btnAbmModif").on("click", function () { btnModiClick(); });
	$("#btnAbmElimi").on("click", function () { btnBajaClick(); });
	$("#btnAbmAceptar").on("click", function () { btnSubmitClick(); });
	$("#btnAbmCancelar").on("click", function () { btnCancelClick(); });

	$("#btnDetalle").on("mousedown", analizaEstadoBtnDetalle);

	$("#btnDetalle").prop("disabled", true);

	$("#btnCancel").on("click", function () {
		//$("#btnFiltro").trigger("click");
		OcultarDivs(true);
		$("#divFiltro").collapse('show');
		$("#listaLs03").prop("disabled", false);
	});

	$("#btnBuscar").on("click", function () {
		let tipoSeleccionado = $("#listaLs03").val();
		if (tipoSeleccionado == "") {
			AbrirMensaje("ATENCIÓN", "Debe seleccionar un Tipo de Medio de Pago.", function () {
				$("#msjModal").modal("hide");
				$("#listaLs03").trigger('focus');
				return true;
			}, false, ["Aceptar"], "error!", null);
		}
		else {
			//es nueva la busqueda no resguardamos la busqueda anterior. es util para paginado
			dataBak = "";
			//es una busqueda por filtro. siempre sera pagina 1
			pagina = 1;
			buscarMediosDePago(pagina);
		}
	});

	$(".inputEditable").on("keypress", analizaEnterInput);
	$("#btnAbmAceptar").hide();
	$("#btnAbmCancelar").hide();

	InicializaPantallaAbmMedioDePago();
	funcCallBack = buscarMediosDePago;
	return true;
});

function OcultarDivs(valor) {
	$("#divDetalle").collapse('hide');
	$("#divTitulo").collapse('hide');
	$("#divGrilla").collapse('hide');
	$("#divPaginacion").collapse('hide');
}

function analizaEstadoBtnDetalle() {
	var res = $("#divDetalle").hasClass("show");
	if (res === true) {
		selectRegCli(regSelected, Grids.GridMedioDePago);
		activarGrilla(Grids.GridMedioDePago);
	}
	return true;

}

function SeteaInsIdSelected() {
	$("#IdSelected").val($("#MedioDePago_Ins_Id").val());
}

function BuscarOpcionesCuotasTabClick() {
	if ($(".nav-link").prop("disabled")) {
		return false;
	}
	
	if ($("#btnTabOpcionesCuotas").prop("disabled")) {
		return false;
	}
	BuscarOpcionesCuotas();
}

function BuscarOpcionesCuotas() {
	insId = $("#MedioDePago_Ins_Id").val();
	if (insId === "") {
		insId = $("#IdSelected").val();
	}
	if (insId != "") {
		var data = { insId };
		AbrirWaiting();
		PostGenHtml(data, buscarOpcionesCuotasUrl, function (obj) {
			$("#divOpcionesCuotas").html(obj);
			AgregarHandlerSelectedRow("tbOpcionesCuotas");
			$(".activable").prop("disabled", true);
			$("#IdSelected").val("");
			CerrarWaiting();
		}, function (obj) {
			ControlaMensajeError(obj.message);
			CerrarWaiting();
		});
	}
}

function BuscarCuentaFinContableTabClick() {
	if ($(".nav-link").prop("disabled")) {
		return false;
	}
	if ($("#btnTabCuentaFinContable").prop("disabled")) {
		return false;
	}
	BuscarCuentaFinContable();
	setTimeout(function () {
		// Inicializar el selector de cuentas
		inicializarSelectorCuentas();
	}, 1000);
}

function BuscarCuentaFinContable() {
	insId = $("#MedioDePago_Ins_Id").val();
	if (insId === "") {
		insId = $("#IdSelected").val();
	}
	if (insId != "") {
		var data = { insId };
		AbrirWaiting();
		PostGenHtml(data, buscarCuentasFinYContableUrl, function (obj) {
			$("#divCuentaFinContable").html(obj);
			AgregarHandlerSelectedRow("tbCuentaFinYContable");
			$(".activable").prop("disabled", true);
			$("#IdSelected").val("");
			CerrarWaiting();
		}, function (obj) {
			ControlaMensajeError(obj.message);
			CerrarWaiting();
		});
	}
}

function BuscarPosTabClick() {
	if ($(".nav-link").prop("disabled")) {
		return false;
	}
	if ($("#btnTabPos").prop("disabled")) {
		return false;
	}
	BuscarPos();
	activarBotones(true);
}

function BuscarPos() {
	insId = $("#MedioDePago_Ins_Id").val();
	if (insId === "") {
		insId = $("#IdSelected").val();
	}
	if (insId != "") {
		var data = { insId };
		AbrirWaiting();
		PostGenHtml(data, buscarPosUrl, function (obj) {
			$("#divPos").html(obj);
			$(".activable").prop("disabled", true);
			$("#IdSelected").val(insId);
			//accionBotones(AbmAction.MODIFICACION, Tabs.TabPos);
			CerrarWaiting();
		}, function (obj) {
			ControlaMensajeError(obj.message);
			CerrarWaiting();
		});
	}
}

function NuevoMedioDePago() {
	var data = {};
	PostGenHtml(data, nuevoMedioDePagoUrl, function (obj) {
		$("#divDatosMedioDePago").html(obj);
		$(".nav-link").prop("disabled", true);
		$(".activable").prop("disabled", false);
		$("#chkInsActivo")[0].checked = true;
		//$("#MedioDePago_Ins_Id").prop("disabled", true);
		desactivarGrilla(Grids.GridMedioDePago);
		accionBotones(AbmAction.ALTA, Tabs.TabMedioDePago);
		$("#divFiltro").collapse("hide");
		$("#divDetalle").collapse("show");
		$("#MedioDePago_Ins_Id").focus();
		CerrarWaiting();
	}, function (obj) {
		ControlaMensajeError(obj.message);
		CerrarWaiting();
	});
}

function NuevaOpcionCuota() {
	var tabActiva = $('.nav-tabs .active')[0].id;
	var mensaje = PuedoAgregar(tabActiva);
	if (mensaje !== "") {
		AbrirMensaje("ATENCIÓN", mensaje, function () {
			$("#msjModal").modal("hide");
			return true;
		}, false, ["Aceptar"], "error!", null);
	}
	else {
		insId = $("#MedioDePago_Ins_Id").val();
		if (insId === "") {
			insId = $("#IdSelected").val();
		}
		var data = { insId };
		PostGenHtml(data, nuevaOpcionCuotaUrl, function (obj) {
			$("#divOpcionesCuotasSelected").html(obj);
			$(".nav-link").prop("disabled", true);
			$(".activable").prop("disabled", false);
			accionBotones(AbmAction.ALTA, tabActiva);
			desactivarGrilla(Grids.GridOpcionesCuotas);
			$("#OpcionCuota_Cuota").focus();
			CerrarWaiting();
		}, function (obj) {
			ControlaMensajeError(obj.message);
			CerrarWaiting();
		});
	}
}

function NuevaCuentaFinYContable() {
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
		PostGenHtml(data, nuevaCuentaFinYContableUrl, function (obj) {
			$("#divCuentaFinYContableSelected").html(obj);
			$(".nav-link").prop("disabled", true);
			$(".activable").prop("disabled", false);
			accionBotones(AbmAction.ALTA, tabActiva);
			desactivarGrilla(Grids.GridCuentaFinYConta);
			$("#CuentaFin_Ctaf_Id").prop("disabled", true);
			$("#CuentaFin_Ctaf_Denominacion").prop("disabled", true);
			setTimeout(function () {
				// Inicializar el selector de cuentas
				inicializarSelectorCuentas();
			}, 500);
			$("#listaTipo").focus();
			CerrarWaiting();
		}, function (obj) {
			ControlaMensajeError(obj.message);
			CerrarWaiting();
		});
	}
}

function NuevaPos() {
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
		PostGenHtml(data, nuevaPosUrl, function (obj) {
			$("#divPos").html(obj);
			$(".nav-link").prop("disabled", true);
			$(".activable").prop("disabled", false);
			accionBotones(AbmAction.ALTA, tabActiva);
			$("#Ins_Id_Pos").focus();
			CerrarWaiting();
		}, function (obj) {
			ControlaMensajeError(obj.message);
			CerrarWaiting();
		});
	}
}

function ModificaMedioDePago(tabAct) {
	accionBotones(AbmAction.MODIFICACION, tabAct);
	tipoDeOperacion = AbmAction.MODIFICACION;
	SetearDestinoDeOperacion(tabAct);
	$(".nav-link").prop("disabled", true);
	$(".activable").prop("disabled", false);
	desactivarGrilla(Grids.GridMedioDePago);
	$("#MedioDePago_Ins_Id").prop("disabled", true);
	$("#MedioDePago_Ins_Desc").focus();
	ControlaChangeChkLinkActivo();
}

function ModificaOpcionesCuota(tabAct, mainGrid) {
	var mensaje = PuedoModificar(tabAct);
	if (mensaje !== "") {
		AbrirMensaje("ATENCIÓN", mensaje, function () {
			$("#msjModal").modal("hide");
			return true;
		}, false, ["Aceptar"], "error!", null);
	}
	else {
		accionBotones(AbmAction.MODIFICACION, Tabs.TabOpcionesCuota);
		tipoDeOperacion = AbmAction.MODIFICACION;
		SetearDestinoDeOperacion(tabAct);
		$(".nav-link").prop("disabled", true);
		$(".activable").prop("disabled", false);
		$("#OpcionCuota_Cuota").prop("disabled", true);
		desactivarGrilla(Grids.GridOpcionesCuotas);
		desactivarGrilla(mainGrid);
		$("#OpcionCuota_Recargo").focus();
	}
}

function ModificaCuentaFinYContable(tabAct, mainGrid) {
	var mensaje = PuedoModificar(tabAct);
	if (mensaje !== "") {
		AbrirMensaje("ATENCIÓN", mensaje, function () {
			$("#msjModal").modal("hide");
			return true;
		}, false, ["Aceptar"], "error!", null);
	}
	else {
		accionBotones(AbmAction.MODIFICACION, Tabs.TabCuentaFinYContable);
		tipoDeOperacion = AbmAction.MODIFICACION;
		SetearDestinoDeOperacion(tabAct);
		$(".nav-link").prop("disabled", true);
		$(".activable").prop("disabled", false);
		$("#CuentaFin_Ctaf_Id").prop("disabled", true);
		$("#CuentaFin_Ctaf_Denominacion").prop("disabled", true);
		desactivarGrilla(Grids.GridCuentaFinYConta);
		desactivarGrilla(mainGrid);
		$("#listaTipo").focus();
	}
}
function ModificaPos(tabAct, mainGrid) {
	var mensaje = PuedoModificar(tabAct);
	if (mensaje !== "") {
		AbrirMensaje("ATENCIÓN", mensaje, function () {
			$("#msjModal").modal("hide");
			return true;
		}, false, ["Aceptar"], "error!", null);
	}
	else {
		accionBotones(AbmAction.MODIFICACION, Tabs.TabPos);
		tipoDeOperacion = AbmAction.MODIFICACION;
		SetearDestinoDeOperacion(tabAct);
		$(".nav-link").prop("disabled", true);
		$(".activable").prop("disabled", false);
		desactivarGrilla(mainGrid);
		$("#Ins_Id_Pos").focus();
	}
}

function InicializaPantallaAbmMedioDePago() {
	var tb = $("#tbGridMedioDePago tbody tr");
	if (tb.length === 0) {
		$("#divFiltro").collapse("show")
	}

	$("#lbRel03").text("Medios de Pago");
	$("#Rel03List").hide();

	$("#lbChkDescr").text("Denominación");
	$("#lbDescr").html("Desc");

	$("#lbChkDesdeHasta").text("ID Cuenta");
	$("#chkRel03").prop('checked', true);
	$("#chkRel03").trigger("change");
	$("#chkRel03").prop("disabled", true);
	$("#IdSelected").val("");
	$(".activable").prop("disabled", true);
	activarBotones(false);
	CargarTiposDeMedioDePago();
	$("#btnAbmNuevo").prop("disabled", true);
	CerrarWaiting();
	return true;
}

function CargarTiposDeMedioDePago() {
	var data = {};
	PostGenHtml(data, cargarTiposDeMedioDePagoUrl, function (obj) {
		$("#divLs03").html(obj);
	}, function (obj) {
		ControlaMensajeError(obj.message);
	});
}

function selectRegDbl(x, gridId) {
	AbrirWaiting("Espere mientras se busca el elemento seleccionado...");
	$("#" + gridId + " tbody tr").each(function (index) {
		$(this).removeClass("selectedEdit-row");
	});
	$(x).addClass("selectedEdit-row");
	regSelected = x;

	switch (gridId) {
		case Grids.GridMedioDePago:
			var ins_id = x[0].cells[0].innerText.trim();
			if (ins_id !== "") {
				insIdRow = x[0];
				insId = ins_id;
				BuscarMedioDePago(ins_id);
				/*ActualizarTitulo();*/
				activarBotones(true);
				$(".activable").prop("disabled", true);
				$("#btnDetalle").prop("disabled", false);
				$("#divFiltro").collapse("hide");
				$("#divDetalle").collapse("show");
				$("#IdSelected").val(insId);
				posicionarRegOnTop(x);
				//desactivarGrilla('tbGridMedioDePago');
			}
			break;
		case Grids.GridOpcionesCuotas:
			var insId = x.cells[4].innerText.trim();
			var opcion = x.cells[0].innerText.trim();
			var data = { insId, opcion };
			AbrirWaiting();
			PostGenHtml(data, buscarOpcionCuotaUrl, function (obj) {
				$("#divOpcionesCuotasSelected").html(obj);
				$("#IdSelected").val(opcion);
				$(".activable").prop("disabled", true);
				activarBotones(true);
				CerrarWaiting();
			}, function (obj) {
				ControlaMensajeError(obj.message);
				CerrarWaiting();
			});
			break;
		case Grids.GridCuentaFinYConta:
			var ctafId = x.cells[0].innerText.trim();
			var data = { ctafId };
			AbrirWaiting();
			PostGenHtml(data, buscarCuentaFinYContableUrl, function (obj) {
				$("#divCuentaFinYContableSelected").html(obj);
				$("#IdSelected").val(ctafId);
				let id = $("#CuentaFin_Ccb_Id").val();
				let nombre = $("#CuentaFin_Ccb_Desc").val();
				if (id != undefined && id != "") {
					let ccb_desc = `(${id}) ${nombre}`;
					$("#cuentaContable").val(ccb_desc);
					$("#cuentaContableId").val(id);
				}
				$(".activable").prop("disabled", true);
				activarBotones(true);
				setTimeout(function () {
					// Inicializar el selector de cuentas
					inicializarSelectorCuentas();
				}, 1000);
				CerrarWaiting();
			}, function (obj) {
				ControlaMensajeError(obj.message);
				CerrarWaiting();
			});
			break;
		default:
	}
}

function ActualizarTitulo() {
	var data = {};
	PostGenHtml(data, actualizarTituloUrl, function (obj) {
		$("#divTitulo").html(obj);
		CerrarWaiting();
	}, function (obj) {
		ControlaMensajeError(obj.message);
		CerrarWaiting();
	});
}

function BuscarMedioDePago(insId) {
	var tcfId = $("#listaLs03").val(); 
	var data = { insId, tcfId };
	AbrirWaiting();
	PostGenHtml(data, buscarMedioDePagoUrl, function (obj) {
		$("#divDatosMedioDePago").html(obj);
		$("#IdSelected").val($("#MedioDePago_Ins_Id").val());
		ValidarTabs();
		BuscarOpcionesCuotas();
		BuscarCuentaFinContable();
		BuscarPos();
		ActualizarTitulo();
		$(".activable").prop("disabled", true);
		$(document).off("change", "#chkLinkActivo").on("change", "#chkLinkActivo", ControlaChangeChkLinkActivo);
		ControlaChangeChkLinkActivo();
		ActualizarTabsSegunTipoSeleccionadoEnFiltro();
		ControlarVisibilidadSegunTipoSeleccionadoEnFiltro();
		CerrarWaiting();
	}, function (obj) {
		ControlaMensajeError(obj.message);
		CerrarWaiting();
	});
}

function ActualizarTabsSegunTipoSeleccionadoEnFiltro() {
	// Ocultar todos
	$("#tabOpcionesCuotas, #tabPos").hide();
	//$("#navs-top-profile, #navs-top-notes").hide();

	const valor = $("#listaLs03").val();

	if (valor === "TC") {
		$("#tabOpcionesCuotas").show();
		//$("#navs-top-profile").show();
		$("#tabPos").show();
		//$("#navs-top-notes").show();
	} else if (valor === "TD") {
		$("#tabPos").show();
		//$("#navs-top-notes").show();
	} 
}
function ControlarVisibilidadSegunTipoSeleccionadoEnFiltro() {
	const valor = $("#listaLs03").val();

	if (valor === "EF") {
		$("#divDatosDeLiquidacion").hide();
		$("#divChkLinkActivo").hide();
		$("#divListaFinanciero").hide();
	} else {
		$("#divDatosDeLiquidacion").show();
		$("#divChkLinkActivo").show();
		$("#divListaFinanciero").show();
	}
}

function ControlaChangeChkLinkActivo() {
	const isChecked = $("#chkLinkActivo").prop("checked");

	if (isChecked) {
		// Habilitar la lista
		$("#listaFinanciero").prop("disabled", false);
	} else {
		// Deshabilitar y limpiar selección
		$("#listaFinanciero").prop("disabled", true).val("");
	}

}

function ValidarTabs() {
	var tcfId = $("#MedioDePago_Tcf_Id").val();
	$("#btnTabOpcionesCuotas").prop("disabled", false);
	$("#btnTabPos").prop("disabled", false);
	$("#btnTabCuentaFinContable").prop("disabled", false);
	if (tcfId != "TC") {
		$("#btnTabOpcionesCuotas").prop("disabled", true);
	}
	if (tcfId != "TC" && tcfId!="TD") {
		$("#btnTabPos").prop("disabled", true);
	}
	//if ($("#chkLinkActivo")[0].checked) {
	//	$("#btnTabCuentaFinContable").prop("disabled", true);
	//}
}

function buscarMediosDePago(pag, esBaja = false) {
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
	//TODO: Mandar el valor seleccionado del combo
	if ($("#chkRel03").is(":checked")) {
		r01.push($("#listaLs03").val());
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
		$("#btnAbmNuevo").prop("disabled", false);
		$("#divGrilla").collapse("show")
		$("#divPaginacion").collapse("show")
		CerrarWaiting();
	}, function (obj) {
		ControlaMensajeError(obj.message);
		CerrarWaiting();
	});

}

function ObtenerDatosDeMedioDePagoParaJson(destinoDeOperacion, tipoDeOperacion) {
	var ins_id = $("#MedioDePago_Ins_Id").val();
	var ins_desc = $("#MedioDePago_Ins_Desc").val();
	var ins_lista = $("#MedioDePago_Ins_Desc").val() + "(" + $("#MedioDePago_Ins_Id").val() + ")";
	var mon_codigo = $("#listaMoneda").val();
	var ins_dato1_desc = $("#MedioDePago_Ins_Dato1_Desc").val();
	var ins_dato2_desc = $("#MedioDePago_Ins_Dato2_Desc").val();
	var ins_dato3_desc = $("#MedioDePago_Ins_Dato3_Desc").val();
	var ins_detalle = $("#MedioDePago_Ins_Detalle").val();
	var ins_comision = $("#MedioDePago_Ins_Comision").val();
	var ins_comision_fija = $("#MedioDePago_Comision_Fija").val();
	var ins_razon_social = $("#MedioDePago_Ins_Razon_Social").val();
	var ins_cuit = $("#MedioDePago_Ins_Cuit").val();
	var ins_ret_gan = $("#MedioDePago_Ins_Ret_Gan").val();
	var ins_ret_ib = $("#MedioDePago_Ins_Ret_Ib").val();
	var ins_ret_iva = $("#MedioDePago_Ins_Ret_Iva").val();
	var ins_arqueo = $("#MedioDePago_Ins_Arqueo").val();
	var ins_tiene_vto = $("#MedioDePago_Ins_Tiene_Vto").val();
	var ins_vigente = "N";
	if ($("#chkInsActivo")[0].checked)
		ins_vigente = "S";
	var ctaf_id_link_check = false;
	if ($("#chkLinkActivo")[0].checked)
		ctaf_id_link_check = true;
	var ctaf_id_link = $("#listaFinanciero").val();
	var tcf_id = $("#MedioDePago_Tcf_Id").val();
	var tcf_desc = $("#MedioDePago_Tcf_Desc").val();
	var ins_id_pos = $("#Ins_Id_Pos").val();
	var ins_id_pos_ctls = $("#ins_id_pos_ctls").val();

	var data = {
		ins_id, ins_desc, ins_lista, mon_codigo, ins_dato1_desc, ins_dato2_desc, ins_dato3_desc, ins_detalle, ins_comision, ins_comision_fija, ins_razon_social, ins_cuit, ins_ret_gan, ins_ret_ib, ins_ret_iva,
		ins_arqueo, ins_tiene_vto, ins_vigente, ctaf_id_link_check, ctaf_id_link, tcf_id, tcf_desc, ins_id_pos, ins_id_pos_ctls, destinoDeOperacion, tipoDeOperacion
	};
	return data;
}

function ObtenerDatosDeOpcCuotaParaJson(destinoDeOperacion, tipoDeOperacion) {
	var ins_id = $("#MedioDePago_Ins_Id").val();
	var pos_plan = $("#OpcionCuota_Pos_Plan").val();
	var pos_desc = $("#OpcionCuota_Pos_Desc").val();
	var recargo = $("#OpcionCuota_Recargo").val();
	var opcion = $("#OpcionCuota_Opcion").val();
	var data = { ins_id, pos_plan, pos_desc, recargo, opcion, destinoDeOperacion, tipoDeOperacion };
	return data;
}

function ObtenerDatosDeCuentaFinContaParaJson(destinoDeOperacion, tipoDeOperacion) {
	var ctaf_id = $("#CuentaFin_Ctaf_Id").val();
	var ctaf_denominacion = $("#CuentaFin_Ctaf_Denominacion").val();
	var ctaf_lista = $("#CuentaFin_Ctaf_Denominacion").val() + "(" + $("#CuentaFin_Ctaf_Id").val() + ")";
	var ctaf_activo = "S";
	var ctaf_estado = $("#listaTipo").val();
	var ctaf_estado_des = $("#listaTipo option:selected").text();
	var ctaf_saldo = $("#CuentaFin_Ctaf_Saldo").val();
	var adm_id = $("#listaAdmin").val();
	var tcf_id = $("#MedioDePago_Tcf_Id").val();
	var tcf_desc = $("#MedioDePago_Tcf_Desc").val();
	var ins_id = $("#MedioDePago_Ins_Id").val();
	var ins_desc = $("#MedioDePago_Ins_Desc").val();
	var ccb_id = $("#cuentaContableId").val();
	var ccb_id_diferido = "";
	var ctag_id = $("#listaGasto").val();
	var mon_codigo = $("#listaMoneda").val();
	var cta_id = $("#CuentaFin_Cta_Id").val();

	var data = {
		ctaf_id, ctaf_denominacion, ctaf_lista, ctaf_activo, ctaf_estado, ctaf_estado_des, ctaf_saldo, adm_id, tcf_id, tcf_desc, ins_id, ins_desc, ccb_id, ccb_id_diferido,
		ctag_id, mon_codigo, cta_id, destinoDeOperacion, tipoDeOperacion
	};
	return data;
}

function ObtenerDatosDePosParaJson(destinoDeOperacion, tipoDeOperacion) {
	var ins_id = $("#MedioDePago_Ins_Id").val();
	var ins_desc = $("#MedioDePago_Ins_Desc").val();
	var ins_lista = $("#MedioDePago_Ins_Desc").val() + "(" + $("#MedioDePago_Ins_Id").val() + ")";
	var mon_codigo = $("#listaMoneda").val();
	var ins_dato1_desc = $("#MedioDePago_Ins_Dato1_Desc").val();
	var ins_dato2_desc = $("#MedioDePago_Ins_Dato2_Desc").val();
	var ins_dato3_desc = $("#MedioDePago_Ins_Dato3_Desc").val();
	var ins_detalle = $("#MedioDePago_Ins_Detalle").val();
	var ins_comision = $("#MedioDePago_Ins_Comision").val();
	var ins_comision_fija = $("#MedioDePago_Comision_Fija").val();
	var ins_razon_social = $("#MedioDePago_Ins_Razon_Social").val();
	var ins_cuit = $("#MedioDePago_Ins_Cuit").val();
	var ins_ret_gan = $("#MedioDePago_Ins_Ret_Gan").val();
	var ins_ret_ib = $("#MedioDePago_Ins_Ret_Ib").val();
	var ins_ret_iva = $("#MedioDePago_Ins_Ret_Iva").val();
	var ins_arqueo = $("#MedioDePago_Ins_Arqueo").val();
	var ins_tiene_vto = $("#MedioDePago_Ins_Tiene_Vto").val();
	var ins_vigente = "N";
	if ($("#chkInsActivo")[0].checked)
		ins_vigente = "S";
	var ctaf_id_link_check = false;
	if ($("#chkLinkActivo")[0].checked)
		ctaf_id_link_check = true;
	var ctaf_id_link = $("#listaFinanciero").val();
	var tcf_id = $("#MedioDePago_Tcf_Id").val();
	var tcf_desc = $("#MedioDePago_Tcf_Desc").val();
	var ins_id_pos = $("#Ins_Id_Pos").val();
	var ins_id_pos_ctls = $("#Ins_Id_Pos_Ctls").val();

	var data = {
		ins_id, ins_desc, ins_lista, mon_codigo, ins_dato1_desc, ins_dato2_desc, ins_dato3_desc, ins_detalle, ins_comision, ins_comision_fija, ins_razon_social, ins_cuit, ins_ret_gan, ins_ret_ib, ins_ret_iva,
		ins_arqueo, ins_tiene_vto, ins_vigente, ctaf_id_link_check, ctaf_id_link, tcf_id, tcf_desc, ins_id_pos, ins_id_pos_ctls, destinoDeOperacion, tipoDeOperacion
	};
	return data;
}

// Variables globales para el selector de cuentas
let cuentaSeleccionada = null;
let arbolCuentasInicializado = false;

/**
* Modifica el selector de cuentas para implementar la búsqueda en tiempo real
*/
function inicializarSelectorCuentas() {
	$("input#cuentaContable").off('click').on("click", function () {
		$("input#cuentaContable").val("");
		$("input#cuentaContableId").val("");
	});
	// Configurar evento para abrir el selector al hacer clic en el botón
	$('.btnBuscarCuenta').off('click').on('click', function () {
		// Tomar los destinos desde los data-attributes
		const campo = $(this).data("target");
		const campoId = $(this).data("target-id");

		// Guardar referencias para los campos destino
		$('#selectorPlanCuentasModal').data('campo-destino', campo);
		$('#selectorPlanCuentasModal').data('campo-destino-id', campoId);

		// Abrir el modal
		$('#selectorPlanCuentasModal').modal('show');

		let tree = $('#cuentasTree').jstree(true);
		let tieneNodos = false;
		if (tree && tree.get_json('#', { flat: true }).length > 0) {
			tieneNodos = true;
		}

		// Cargar el árbol si no está inicializado
		if (!arbolCuentasInicializado || !tieneNodos) {
			cargarArbolCuentas();
		}
	});

	// NUEVA IMPLEMENTACIÓN: Búsqueda en tiempo real al escribir
	$('#txtBuscarCuentaPlan').off('keyup').on('keyup', function () {
		const termino = $(this).val().trim();

		// Obtener instancia del árbol
		const tree = $("#cuentasTree").jstree(true);
		if (!tree) return;

		if (termino.length > 0) {
			// Si hay texto, realizar la búsqueda
			tree.search(termino, false, true);

			// Usar setTimeout para dar tiempo a jsTree a actualizar el DOM
			setTimeout(function () {
				// Contar los resultados usando jQuery
				const nodosEncontrados = $('.jstree-search');
				const totalResultados = nodosEncontrados.length;

				// Expandir los nodos padre de los resultados
				nodosEncontrados.each(function () {
					const nodeId = $(this).closest('.jstree-node').attr('id');
					if (nodeId) {
						// Obtener y expandir todos los nodos padres
						let parent = tree.get_parent(nodeId);
						while (parent && parent !== "#") {
							tree.open_node(parent);
							parent = tree.get_parent(parent);
						}
					}
				});

				// Mostrar mensaje con cantidad de resultados
				if (totalResultados > 0) {
					$("#resultadosBusqueda").html(`
                    <div class="alert alert-success py-1 small">
                        <i class="bx bx-check-circle me-1"></i>
                        Se encontraron <strong>${totalResultados}</strong> cuenta(s) que coinciden
                    </div>
                `).show();
				} else {
					$("#resultadosBusqueda").html(`
                    <div class="alert alert-warning py-1 small">
                        <i class="bx bx-error-circle me-1"></i>
                        No se encontraron cuentas que coincidan
                    </div>
                `).show();
				}

				// Ocultar después de 3 segundos
				setTimeout(function () {
					$("#resultadosBusqueda").fadeOut();
				}, 3000);
			}, 200); // Pequeño retraso para que jsTree termine de actualizar el DOM
		} else {
			// Si el campo está vacío, limpiar la búsqueda
			tree.clear_search();
			tree.close_all();
			$("#resultadosBusqueda").fadeOut();
		}
	});


	// Búsqueda al presionar Enter (para evitar envío de formulario)
	$('#txtBuscarCuentaPlan').off('keypress').on('keypress', function (e) {
		if (e.which === 13) {
			e.preventDefault(); // Evitar envío de formulario
			// La búsqueda ya se habrá hecho con el evento keyup
		}
	});

	// Evento para seleccionar cuenta
	$('#btnSeleccionarCuenta').off('click').on('click', function () {
		if (cuentaSeleccionada) {
			// Obtener los campos destino desde el modal
			const campoDestino = $('#selectorPlanCuentasModal').data('campo-destino');
			const campoDestinoId = $('#selectorPlanCuentasModal').data('campo-destino-id');

			// Actualizar los campos con la cuenta seleccionada
			$('#' + campoDestino).val(cuentaSeleccionada.text);
			$('#' + campoDestinoId).val(cuentaSeleccionada.id);

			// Cerrar el modal
			$('#selectorPlanCuentasModal').modal('hide');
		}
	});

	// Limpiar búsqueda y selección al abrir el modal
	$('#selectorPlanCuentasModal').off('shown.bs.modal').on('shown.bs.modal', function () {
		// Limpiar campo de búsqueda y darle el foco
		$('#txtBuscarCuentaPlan').val('').trigger("focus");

		// Limpiar búsqueda previa
		const tree = $("#cuentasTree").jstree(true);
		if (tree) {
			tree.clear_search();
			tree.close_all();
		}

		// Resetear selección
		cuentaSeleccionada = null;
		$('#btnSeleccionarCuenta').prop('disabled', true);
		$("#resultadosBusqueda").hide();
	});

	// Limpiar búsqueda y selección al cerrar el modal
	$('#selectorPlanCuentasModal').off('hidden.bs.modal').on('hidden.bs.modal', function () {
		$('#txtBuscarCuentaPlan').val('');
		cuentaSeleccionada = null;
		$('#btnSeleccionarCuenta').prop('disabled', true);

		// Devolver el foco al botón que abrió el modal (para accesibilidad)
		$('#btnBuscarCuenta').trigger("focus");
	});
}

/**
* Carga el árbol de cuentas desde el servidor
*/
function cargarArbolCuentas() {
	// Mostrar indicador de carga en el árbol
	$("#cuentasTree").html(`
        <div class="text-center p-3">
            <div class="spinner-border spinner-border-sm text-warning" role="status">
                <span class="visually-hidden">Cargando...</span>
            </div>
            <p class="mt-2 small">Cargando plan de cuentas...</p>
        </div>
    `);

	AbrirWaiting("Cargando plan de cuentas...");

	const data = {
		buscar: "",
		buscaNew: true
	};

	// Verificar que la URL esté configurada
	if (!buscarPlanCuentasUrl) {
		console.error("La URL para buscar el plan de cuentas no está configurada");
		AbrirMensaje(
			"Error",
			"No se pudo cargar el plan de cuentas. La URL no está configurada.",
			function () { $("#msjModal").modal("hide"); },
			false,
			["Aceptar"],
			"error!",
			null
		);
		CerrarWaiting();
		return;
	}

	// Realizar la petición AJAX
	$.ajax({
		url: buscarPlanCuentasUrl,
		type: "POST",
		contentType: "application/json",
		data: JSON.stringify(data),
		success: function (resultado) {
			CerrarWaiting();

			if (resultado.error) {
				console.error("Error al cargar el plan de cuentas:", resultado.msg);
				AbrirMensaje(
					"Error",
					"Error al cargar el plan de cuentas: " + resultado.msg,
					function () { $("#msjModal").modal("hide"); },
					false,
					["Aceptar"],
					"error!",
					null
				);
				return;
			}

			try {
				// Parsear el árbol
				const arbolCuentas = JSON.parse(resultado.arbol);

				// Procesar los nodos para añadir íconos y clases
				procesarNodosArbol(arbolCuentas);

				// Inicializar jsTree
				inicializarJsTree(arbolCuentas);

				arbolCuentasInicializado = true;
			} catch (error) {
				console.error("Error al procesar los datos del plan de cuentas:", error);
				AbrirMensaje(
					"Error",
					"Error al procesar los datos del plan de cuentas",
					function () { $("#msjModal").modal("hide"); },
					false,
					["Aceptar"],
					"error!",
					null
				);
			}
		},
		error: function (xhr, status, error) {
			CerrarWaiting();
			console.error("Error al cargar el plan de cuentas:", error);
			AbrirMensaje(
				"Error",
				"Error de comunicación al cargar el plan de cuentas",
				function () { $("#msjModal").modal("hide"); },
				false,
				["Aceptar"],
				"error!",
				null
			);
		}
	});
}

/**
* Procesa los nodos del árbol para añadir íconos y clases
* @param {Array} nodos - Lista de nodos del árbol
*/
function procesarNodosArbol(nodos) {
	nodos.forEach(nodo => {
		// Determinar tipo de cuenta para el ícono
		const tipo = nodo.data?.tipo;
		const cuentaTipo = nodo.data?.cuenta?.toLowerCase();

		// Asignar tipo para íconos
		nodo.type = cuentaTipo || "default";

		// Asignar clases CSS
		nodo.a_attr = nodo.a_attr || {};
		let clases = [];

		if (tipo === "M") clases.push("tipo-movimiento");
		if (cuentaTipo) clases.push("cuenta-" + cuentaTipo);

		nodo.a_attr.class = clases.join(" ");

		// Procesar nodos hijos recursivamente
		if (nodo.children && nodo.children.length > 0) {
			procesarNodosArbol(nodo.children);
		}
	});
}

/**
 * Inicializa el árbol jsTree con los datos procesados y configura la búsqueda
 * @param {Array} datos - Datos del árbol
 */
function inicializarJsTree(datos) {
	// Destruir instancia previa si existe
	if ($.jstree.reference("#cuentasTree")) {
		$("#cuentasTree").jstree("destroy");
	}

	// Inicializar nueva instancia con soporte para búsqueda
	$("#cuentasTree").jstree({
		core: {
			data: datos,
			themes: {
				responsive: true
			}
		},
		types: {
			activo: {
				icon: "bx bx-wallet"
			},
			pasivo: {
				icon: "bx bx-trending-down"
			},
			patrimonio: {
				icon: "bx bx-building-house"
			},
			ingresos: {
				icon: "bx bx-dollar-circle"
			},
			egresos: {
				icon: "bx bx-money-withdraw"
			},
			default: {
				icon: "bx bx-folder"
			}
		},
		search: {
			show_only_matches: true,
			show_only_matches_children: true,
			close_opened_onclear: true,
			search_leaves_only: false
		},
		plugins: ["types", "search"]
	});

	// Evento al seleccionar un nodo
	$("#cuentasTree").off('select_node.jstree').on("select_node.jstree", function (e, data) {
		const nodo = data.node;
		const nodoId = nodo.id;
		const nodoTexto = nodo.text;
		const nodoTipo = nodo.data?.tipo;

		// Solo permitir seleccionar cuentas de movimiento
		if (nodoTipo === "M") {
			// Guardar la cuenta seleccionada
			cuentaSeleccionada = {
				id: nodoId,
				text: nodoTexto
			};

			// Habilitar el botón de seleccionar
			$('#btnSeleccionarCuenta').prop('disabled', false);
		} else {
			// No es una cuenta de movimiento, mostrar mensaje
			AbrirMensaje(
				"Aviso",
				"Solo puede seleccionar cuentas de movimiento.",
				function () { $("#msjModal").modal("hide"); },
				false,
				["Aceptar"],
				"info!",
				null
			);

			// Desseleccionar el nodo
			$("#cuentasTree").jstree("deselect_node", nodoId);

			// Deshabilitar el botón de seleccionar
			$('#btnSeleccionarCuenta').prop('disabled', true);
			cuentaSeleccionada = null;
		}
	});

	// Cuando el árbol está listo, colapsarlo inicialmente
	$("#cuentasTree").on("ready.jstree", function () {
		$("#cuentasTree").jstree("close_all");
	});
}