$(function () {
	$("#pagEstado").on("change", function () {
		var div = $("#divPaginacion");
		presentaPaginacion(div);
	});

	$(document).on("dblclick", "#" + Grids.GridSector + " tbody tr", function () {
		x = $(this);
		ejecutaDblClickGrid(x, Grids.GridSector);
	});

	$("#tabSector").on("click", function () { SeteaIDSectorSelected(); });
	$("#tabSubSector").on("click", function () { BuscarSubSectorTabClick(); });
	$("#tabRubro").on("click", function () { BuscarRubroTabClick(); });

	$("#btnAbmNuevo").on("click", function () { btnNuevoClick(); });
	$("#btnAbmModif").on("click", function () { btnModiClick(); });
	$("#btnAbmElimi").on("click", function () { btnBajaClick(); });
	$("#btnAbmAceptar").on("click", function () { btnSubmitClick(); });
	$("#btnAbmCancelar").on("click", function () { btnCancelClick(); });

	$("#btnDetalle").on("mousedown", analizaEstadoBtnDetalle);

	$("#btnDetalle").prop("disabled", true);
	$("#btnAbmAceptar").hide();
	$("#btnAbmCancelar").hide();
	$("#btnCancel").on("click", function () {
		$("#btnFiltro").trigger("click");
	});

	$("#btnBuscar").on("click", function () {
		dataBak = "";
		pagina = 1;
		buscarSectores(pagina);
	});

	InicializaPantallaAbmSector();
	funcCallBack = buscarSectores;
	return true;

});

function NuevoSector() {
	var data = {};
	PostGenHtml(data, nuevoSectorUrl, function (obj) {
		$("#divDatosSector").html(obj);
		$(".nav-link").prop("disabled", true);
		$(".activable").prop("disabled", false);
		$("#Sector_Sec_Id").prop("disabled", false);
		desactivarGrilla(Grids.GridSector);
		accionBotones(AbmAction.ALTA, Tabs.TabSector);
		$("#divFiltro").collapse("hide");
		$("#divDetalle").collapse("show");
		$("#Sector_Sec_Id").focus();
		CerrarWaiting();
	}, function (obj) {
		ControlaMensajeError(obj.message);
		CerrarWaiting();
	});
}

function NuevoSubSector() {
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
		PostGenHtml(data, nuevoSubSectorUrl, function (obj) {
			$("#divDatosDeSubSectorSelected").html(obj);
			$(".nav-link").prop("disabled", true);
			$(".activable").prop("disabled", false);
			accionBotones(AbmAction.ALTA, Tabs.TabSubSector);
			desactivarGrilla(Grids.GridSubSector);
			$("#SubSector_Rubg_Id").prop("disabled", false);
			$("#SubSector_Rubg_Id").focus();
			CerrarWaiting();
		}, function (obj) {
			ControlaMensajeError(obj.message);
			CerrarWaiting();
		});
	}
}

function NuevoRubro() {
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
		PostGenHtml(data, nuevoRubroUrl, function (obj) {
			$("#divDatosDeRubroSelected").html(obj);
			$(".nav-link").prop("disabled", true);
			$(".activable").prop("disabled", false);
			accionBotones(AbmAction.ALTA, Tabs.TabRubro);
			desactivarGrilla(Grids.GridRubro);
			$("#Rubro_Rub_Id").prop("disabled", false);
			$("#Rubro_Rub_Id").focus();
			CerrarWaiting();
		}, function (obj) {
			ControlaMensajeError(obj.message);
			CerrarWaiting();
		});
	}
}

function ModificaSector(tabAct) {
	accionBotones(AbmAction.MODIFICACION, tabAct);
	tipoDeOperacion = AbmAction.MODIFICACION;
	SetearDestinoDeOperacion(tabAct);
	$(".nav-link").prop("disabled", true);
	$(".activable").prop("disabled", false);
	desactivarGrilla(Grids.GridSector);
	$("#Sector_Sec_Id").prop("disabled", true);
	$("#Sector_Sec_Desc").focus();
}

function ModificaSubSector(tabAct, mainGrid) {
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
		$("#SubSector_Rubg_Id").prop("disabled", true);
		desactivarGrilla(Grids.GridSubSector);
		desactivarGrilla(mainGrid);
		$("#SubSector_Rubg_Desc").focus();
	}
}

function ModificaRubro(tabAct, mainGrid) {
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
		$("#Rubro_Rub_Id").prop("disabled", true);
		desactivarGrilla(Grids.GridRubro);
		desactivarGrilla(mainGrid);
		$("#Rubro_Rub_Desc").focus();
	}
}

function analizaEstadoBtnDetalle() {
	var res = $("#divDetalle").hasClass("show");
	if (res === true) {
		selectRegCli(regSelected, Grids.GridProveedor);
	}
	return true;

}

function SeteaIDSectorSelected() {
	$("#IdSelected").val($("#Sector_Sec_Id").val());
}

function BuscarSubSectorTabClick() {
	if ($(".nav-link").prop("disabled")) {
		return false;
	}
	BuscarSubSector();
}

function BuscarSubSector() {
	if (secId != "") {
		var data = { secId };
		AbrirWaiting();
		PostGenHtml(data, buscarSubSectorUrl, function (obj) {
			$("#divSubSector").html(obj);
			AgregarHandlerSelectedRow("tbSubSectorEnTab");
			$(".activable").prop("disabled", true);
			$("#IdSelected").val("");
			CerrarWaiting();
		}, function (obj) {
			ControlaMensajeError(obj.message);
			CerrarWaiting();
		});
	}
}

function BuscarRubroTabClick() {
	if ($(".nav-link").prop("disabled")) {
		return false;
	}
	BuscarRubro();
}

function BuscarRubro() {
	if (secId != "") {
		var data = { secId };
		AbrirWaiting();
		PostGenHtml(data, buscarRubroUrl, function (obj) {
			$("#divRubro").html(obj);
			AgregarHandlerSelectedRow("tbRubroEnTab");
			$(".activable").prop("disabled", true);
			$("#IdSelected").val("");
			CerrarWaiting();
		}, function (obj) {
			ControlaMensajeError(obj.message);
			CerrarWaiting();
		});
	}
}

function InicializaPantallaAbmSector() {
	var tb = $("#tbGridSector tbody tr");
	if (tb.length === 0) {
		$("#divFiltro").collapse("show")
	}


	$("#lbChkDescr").text("Denominación");
	$("#lbDescr").html("Desc");

	$("#lbChkDesdeHasta").text("ID");

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
			buscarSectores(num);
		}
	});
	$("#pagEstado").val(false);
	$("#divFiltro").collapse("hide")
	return true;
}

function buscarSectores(pag, esBaja = false) {
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

function selectRegDbl(x, gridId) {
	AbrirWaiting("Espere mientras se busca la información solicitada...");
	$("#" + gridId + " tbody tr").each(function (index) {
		$(this).removeClass("selectedEdit-row");
	});
	$(x).addClass("selectedEdit-row");
	regSelected = x;

	switch (gridId) {
		case Grids.GridSector:
			var sec_id = x[0].cells[0].innerText.trim();
			if (sec_id !== "") {
				secId = sec_id;
				BuscarSector(sec_id);
				activarBotones(true);
				$(".activable").prop("disabled", true);
				$("#btnDetalle").prop("disabled", false);
				$("#divFiltro").collapse("hide");
				$("#divDetalle").collapse("show");
				$("#IdSelected").val(secId);
				posicionarRegOnTop(x);
			}
			break;
		case Grids.GridSubSector:
			var ssId = x.cells[0].innerText.trim();
			var data = { ssId };
			AbrirWaiting();
			PostGenHtml(data, buscarDatosSubSectorUrl, function (obj) {
				$("#divDatosDeSubSectorSelected").html(obj);
				$("#IdSelected").val(ssId);
				$(".activable").prop("disabled", true);
				activarBotones(true);
				CerrarWaiting();
			}, function (obj) {
				ControlaMensajeError(obj.message);
				CerrarWaiting();
			});
			break;
		case Grids.GridRubro:
			var rubId = x.cells[0].innerText.trim();
			var data = { rubId };
			AbrirWaiting();
			PostGenHtml(data, buscarDatosRubroUrl, function (obj) {
				$("#divDatosDeRubroSelected").html(obj);
				$("#IdSelected").val(rubId);
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

function BuscarSector(secId) {
	var data = { secId };
	AbrirWaiting();
	PostGenHtml(data, buscarSectorUrl, function (obj) {
		$("#divDatosSector").html(obj);
		$("#IdSelected").val($("#Sector_Sec_Id").val());
		$(".activable").prop("disabled", true);
		CerrarWaiting();
	}, function (obj) {
		ControlaMensajeError(obj.message);
		CerrarWaiting();
	});
}

function ObtenerDatosDeSectorParaJson(destinoDeOperacion, tipoDeOperacion) {
	var sec_id = $("#Sector_Sec_Id").val();
	var sec_desc = $("#Sector_Sec_Desc").val();
	var sec_lista = $("#Sector_Sec_Desc").val() + "(" + $("#Sector_Sec_Id").val() + ")";
	var sec_prefactura = 'N';
	if ($("#chkPrefaActiva")[0].checked)
		sec_prefactura = "S";
	var data = { sec_id, sec_desc, sec_lista, sec_prefactura, destinoDeOperacion, tipoDeOperacion };
	return data;
}

function ObtenerDatosDeSubSectorParaJson(destinoDeOperacion, tipoDeOperacion) {
	var rubg_id = $("#SubSector_Rubg_Id").val();
	var rubg_desc = $("#SubSector_Rubg_Desc").val();
	var rubg_lista = $("#SubSector_Rubg_Desc").val() + "(" + $("#SubSector_Rubg_Id").val() + ")";
	var sec_id = $("#Sector_Sec_Id").val();
	var sec_desc = $("#Sector_Sec_Desc").val();
	var data = { rubg_id, rubg_desc, rubg_lista, sec_id, sec_desc, destinoDeOperacion, tipoDeOperacion };
	return data;
}

function ObtenerDatosDeRubroParaJson(destinoDeOperacion, tipoDeOperacion) {
	var rub_id = $("#Rubro_Rub_Id").val();
	var rub_desc = $("#Rubro_Rub_Desc").val();
	var rub_lista = $("#Rubro_Rub_Desc").val() + "(" + $("#Rubro_Rub_Id").val() + ")";
	var rub_feteado = 'N';
	if ($("#chkFeteados")[0].checked)
		rub_feteado = "S";
	var rub_ctlstk = 'N';
	if ($("#chkCtrlStkEnVtas")[0].checked)
		rub_ctlstk = "S";
	var rubg_id = $("#listaSubSector").val();
	var rubg_desc = $("#listaSubSector option:selected").text();
	var rubg_lista = $("#listaSubSector option:selected").text() + "(" + $("#listaSubSector").val() + ")";
	var sec_id = $("#Sector_Sec_Id").val();
	var sec_desc = $("#Sector_Sec_Desc").val();
	var data = { rub_id, rub_desc, rub_lista, rub_feteado, rub_ctlstk, rubg_id, rubg_desc, rubg_lista, sec_id, sec_desc, destinoDeOperacion, tipoDeOperacion };
	return data;
}