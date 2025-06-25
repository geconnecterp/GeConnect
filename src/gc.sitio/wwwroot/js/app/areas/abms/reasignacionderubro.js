$(function () {
	$("#pagEstado").on("change", function () {
		var div = $("#divPaginacion");
		presentaPaginacion(div);
	});
	$(document).on("change", "#listaRubro", actualizarListaDeProductosPorRubro);
	$(document).on("dblclick", "#" + Grids.GridSector + " tbody tr", function () {
		x = $(this);
		ejecutaDblClickGrid(x, Grids.GridSector);
	});

	$("#btnDetalle").on("mousedown", analizaEstadoBtnDetalle);
	$("#btnDetalle").prop("disabled", true);
	
	$("#btnBuscar").on("click", function () {
		//es nueva la busqueda no resguardamos la busqueda anterior. es util para paginado
		dataBak = "";
		//es una busqueda por filtro. siempre sera pagina 1
		pagina = 1;
		buscarSectores(pagina);
	});

	$("#btnCancel").on("click", function () {
		InicializaPantalla();
	});

	InicializaPantalla();
	funcCallBack = buscarSectores;
	return true;
});

function btnSubmitClick() {
	var rubroOri = $("#listaRubro").val();
	var rubroDest = $("#listaRubroAReasignar").val();
	if (rubroDest === "") {
		AbrirMensaje("ATENCIÓN", "Debe seleccionar un rubro de producto destino válido.", function () {
			$("#msjModal").modal("hide");
			return true;
		}, false, ["Aceptar"], "error!", null);
		return;
	}
	if (rubroOri === rubroDest) {
		AbrirMensaje("ATENCIÓN", "El rubro de producto destino debe ser diferente al original.", function () {
			$("#msjModal").modal("hide");
			return true;
		}, false, ["Aceptar"], "error!", null);
		return
	}
	var ids = [];
	$("#tbProductosPorRubro").find('tr').each(function (i, el) {
		var td = $(this).find('td');
		if (td.eq(0)[0]) {
			if (td.eq(0)[0].children[0].checked)
				ids.push(td.eq(1).text());
		}
	});
	if (ids.length <= 0) {
		AbrirMensaje("ATENCIÓN", "Debe seleccionar al menos un producto a reasignar.", function () {
			$("#msjModal").modal("hide");
			return true;
		}, false, ["Aceptar"], "error!", null);
		return
	}
	var data = { secId, rubroDest, ids }
	PostGen(data, reasignarProductosURL, function (obj) {
		if (obj.error === true) {
			AbrirMensaje("ATENCIÓN", obj.msg, function () {
				$("#msjModal").modal("hide");
				return true;
			}, false, ["Aceptar"], "error!", null);
		}
		else {
			AbrirMensaje("ATENCIÓN", "La reasignación se ha realizado correctamente.", function () {
				$("#msjModal").modal("hide");
				return true;
			}, false, ["Aceptar"], "succ!", null);
			CargarProductosPorRubroSeleccionado(secId, rubroOri);
			return
		}

	});
}

function selectReg(x, gridId) {
	$("#" + gridId + " tbody tr").each(function (index) {
		$(this).removeClass("selected-row");
		$(this).removeClass("selectedEdit-row");
	});
	$(x).addClass("selected-row");
	LimpiarSeleccion();
}

function LimpiarSeleccion() {
	BuscarProductosPorRubro("");
	$("#btnDetalle").prop("disabled", true);
	$("#divDetalle").collapse("hide");
}

function CargarProductosPorRubroSeleccionado(secId, rubroSelected) {
	AbrirWaiting("Aguarde unos instantes mientras buscamos los productos del rubro seleccionado...");
	var data = { secId, rubroSelected };
	PostGenHtml(data, buscarProductosPorRubroUrl, function (obj) {
		$("#divGridProductosPorRubro").html(obj);
		AgregarHandlerAGrillaProdPorRubro();
		CerrarWaiting();
	}, function (obj) {
		CerrarWaiting();
		ControlaMensajeError(obj.message);
		return true;
	});
}

function InicializaPantalla() {
	var tb = $("#tbGridSector tbody tr");
	if (tb.length === 0) {
		$("#divFiltro").collapse("show")
	}

	$("#divDetalle").collapse("hide");
	$("#divGrilla").html(`<p style="background-color: gray"><h5> No se han especificado datos aún.</h5 ></p >`);
	$("#divPaginacion").collapse("hide");
	$("#divPaginacion").addClass("collapse");

	$("#chkDescr").prop('checked', false);
	$("#chkDescr").trigger("change");
	$("#Buscar").val("");
	$("#Buscar").prop("disabled", true);
	$("#chkDesdeHasta").prop('checked', false);
	$("#chkDesdeHasta").trigger("change");
	$("#Id").val("");
	$("#Id").prop("disabled", true);
	$("#Id2").val("");
	$("#Id2").prop("disabled", true);

	$("#lbChkDescr").text("Denominación");
	$("#lbDescr").html("Desc");

	$("#lbChkDesdeHasta").text("ID");

	$("#IdSelected").val("");
	$(".activable").prop("disabled", true);
	$("#btnDetalle").prop("disabled", true);
	activarBotones(false);
	CerrarWaiting();
	return true;
}

function selectRegDbl(x, gridId) {
	AbrirWaiting("Espere mientras se busca el sector seleccionado...");
	$("#" + gridId + " tbody tr").each(function (index) {
		$(this).removeClass("selectedEdit-row");
	});
	$(x).addClass("selectedEdit-row");
	regSelected = x;

	switch (gridId) {
		case Grids.GridSector:
			var sec_id = x[0].cells[0].innerText.trim();
			if (sec_id !== "") {
				secIdRow = x[0];
				secId = sec_id;
				BuscarProductosPorRubro(sec_id);
				activarBotones(true);
				$(".activable").prop("disabled", true);
				$("#btnDetalle").prop("disabled", false);
				$("#divFiltro").collapse("hide");
				$("#divDetalle").collapse("show");
				$("#IdSelected").val(secId);
				posicionarRegOnTop(x);
				CerrarWaiting();
			}
			else
				CerrarWaiting();
			break;
		default:
			CerrarWaiting();
			break;
	}
}

function BuscarProductosPorRubro(secId) {
	data = { secId };
	PostGenHtml(data, buscarRubroUrl, function (obj) {
		$("#divDatosReasignacion").html(obj);
		var rubroSelected = $("#listaRubro").val();
		if (rubroSelected && rubroSelected !== "") {
			CargarProductosPorRubroSeleccionads(secId, rubroSelected);
		}
	}, function (obj) {
		ControlaMensajeError(obj.message);
		CerrarWaiting();
		return true;
	});
}

function CargarProductosPorRubroSeleccionads(secId, rubroSelected) {
	AbrirWaiting("Aguarde unos instantes mientras buscamos los productos del rubro seleccionado...");
	var data = { secId, rubroSelected };
	PostGenHtml(data, buscarProductosPorRubroUrl, function (obj) {
		$("#divGridProductosPorRubro").html(obj);
		AgregarHandlerAGrillaProdPorRubro();
		CerrarWaiting();
	}, function (obj) {
		CerrarWaiting();
		ControlaMensajeError(obj.message);
		return true;
	});
}

function actualizarListaDeProductosPorRubro() {
	var rubroSelected = $("#listaRubro").val();
	CargarProductosPorRubroSeleccionada(secId, rubroSelected);
}

function CargarProductosPorRubroSeleccionada(secId, rubroSelected) {
	AbrirWaiting("Aguarde unos instantes mientras buscamos los productos del rubro seleccionado...");
	var data = { secId, rubroSelected };
	PostGenHtml(data, buscarProductosPorRubroUrl, function (obj) {
		$("#divGridProductosPorRubro").html(obj);
		AgregarHandlerAGrillaProdPorRubro();
		CerrarWaiting();
	}, function (obj) {
		CerrarWaiting();
		ControlaMensajeError(obj.message);
		return true;
	});
}

function AgregarHandlerAGrillaProdPorRubro() {
	var dataTable = document.getElementById('tbProductosPorRubro');
	var checkItAll = dataTable.querySelector('input[name="select_all"]');
	var inputs = dataTable.querySelectorAll('tbody>tr>td>input');
	checkItAll.addEventListener('change', function () {
		if (checkItAll.checked) {
			inputs.forEach(function (input) {
				input.checked = true;
			});
		}
		else {
			inputs.forEach(function (input) {
				input.checked = false;
			});
		}
	});
}

function analizaEstadoBtnDetalle() {
	var res = $("#divDetalle").hasClass("show");
	if (res === true) {
		selectReg(regSelected, Grids.GridSector);
	}
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
				$("#divGrilla").removeClass("collapse");
				$("#divPaginacion").removeClass("collapse");
			}

		});
		CerrarWaiting();
	}, function (obj) {
		ControlaMensajeError(obj.message);
		CerrarWaiting();
	});

}