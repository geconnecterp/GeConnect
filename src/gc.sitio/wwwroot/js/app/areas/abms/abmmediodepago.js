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
		$("#btnFiltro").trigger("click");
	});

	$("#btnBuscar").on("click", function () {
		//es nueva la busqueda no resguardamos la busqueda anterior. es util para paginado
		dataBak = "";
		//es una busqueda por filtro. siempre sera pagina 1
		pagina = 1;
		buscarMediosDePago(pagina);
	});

	$(".inputEditable").on("keypress", analizaEnterInput);
	$("#btnAbmAceptar").hide();
	$("#btnAbmCancelar").hide();

	InicializaPantallaAbmMedioDePago();
	funcCallBack = buscarMediosDePago;
	return true;
});

function analizaEstadoBtnDetalle() {
	var res = $("#divDetalle").hasClass("show");
	if (res === true) {
		selectRegCli(regSelected, Grids.GridMedioDePago);
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

	$("#lbRel01").text("Medios de Pago");

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
			}
			break;
		case Grids.GridOpcionesCuotas:
			var insId = x.cells[2].innerText.trim();
			var cuota = x.cells[0].innerText.trim();
			var data = { insId, cuota };
			AbrirWaiting();
			PostGenHtml(data, buscarOpcionCuotaUrl, function (obj) {
				$("#divOpcionesCuotasSelected").html(obj);
				$("#IdSelected").val(cuota);
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

function ActualizarTitulo() {
	PostGenHtml(data, actualizarTituloUrl, function (obj) {
		$("#divTitulo").html(obj);
		CerrarWaiting();
	}, function (obj) {
		ControlaMensajeError(obj.message);
		CerrarWaiting();
	});
}

function BuscarMedioDePago(insId) {
	var data = { insId };
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
		CerrarWaiting();
	}, function (obj) {
		ControlaMensajeError(obj.message);
		CerrarWaiting();
	});
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
	if ($("#chkLinkActivo")[0].checked) {
		$("#btnTabCuentaFinContable").prop("disabled", true);
	}
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
	var cuota = $("#OpcionCuota_Cuota").val();
	var recargo = $("#OpcionCuota_Recargo").val();

	var data = { ins_id, cuota, recargo, destinoDeOperacion, tipoDeOperacion };
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
	var ccb_id = $("#listaConta").val();
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
	var ins_id_pos_ctls = $("#ins_id_pos_ctls").val();

	var data = {
		ins_id, ins_desc, ins_lista, mon_codigo, ins_dato1_desc, ins_dato2_desc, ins_dato3_desc, ins_detalle, ins_comision, ins_comision_fija, ins_razon_social, ins_cuit, ins_ret_gan, ins_ret_ib, ins_ret_iva,
		ins_arqueo, ins_tiene_vto, ins_vigente, ctaf_id_link_check, ctaf_id_link, tcf_id, tcf_desc, ins_id_pos, ins_id_pos_ctls, destinoDeOperacion, tipoDeOperacion
	};
	return data;
}