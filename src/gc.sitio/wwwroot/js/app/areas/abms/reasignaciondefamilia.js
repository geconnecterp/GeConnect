$(function () {
	$("#pagEstado").on("change", function () {
		var div = $("#divPaginacion");
		presentaPaginacion(div);
	});
	$(document).on("change", "#listaFamilia", actualizarListaDeProductos);
	$(document).on("dblclick", "#" + Grids.GridProveedor + " tbody tr", function () {
		x = $(this);
		ejecutaDblClickGrid(x, Grids.GridProveedor);
	});

	$("#btnDetalle").on("mousedown", analizaEstadoBtnDetalle);
	$("#btnDetalle").prop("disabled", true);
	$("#btnBuscar").on("click", function () {
		//es nueva la busqueda no resguardamos la busqueda anterior. es util para paginado
		dataBak = "";
		//es una busqueda por filtro. siempre sera pagina 1
		pagina = 1;
		buscarProveedores(pagina);
	});

	$("#btnCancel").on("click", function () {
		InicializaPantalla();
	});

	InicializaPantalla();
	funcCallBack = buscarProveedores;
	return true;
});

function btnSubmitClick() {
	var familiaOri = $("#listaFamilia").val();
	var familiaDest = $("#listaFamiliaAReasignar").val();
	if (familiaDest === "") {
		AbrirMensaje("ATENCIÓN", "Debe seleccionar una familia de producto destino válida.", function () {
			$("#msjModal").modal("hide");
			return true;
		}, false, ["Aceptar"], "error!", null);
		return;
	}
	if (familiaOri === familiaDest) {
		AbrirMensaje("ATENCIÓN", "La familia de producto destino debe ser diferente a la original.", function () {
			$("#msjModal").modal("hide");
			return true;
		}, false, ["Aceptar"], "error!", null);
		return
	}
	var ids = [];
	$("#tbProductosPorFamilia").find('tr').each(function (i, el) {
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
	var data = { ctaId, familiaDest, ids }
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
			CargarProductosPorFamiliaSeleccionada(ctaId, familiaOri);
			return
		}

	});
}

function ejecutaDblClickGrid(x, grid) {
	AbrirWaiting("Espere mientras se busca el producto seleccionado...");
	selectRegDbl(x, grid);
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
	BuscarProductosPorFamilia("");
	$("#btnDetalle").prop("disabled", true);
	$("#divDetalle").collapse("hide");

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
				BuscarProductosPorFamilia(cta_id);
				activarBotones(true);
				$(".activable").prop("disabled", true);
				$("#btnDetalle").prop("disabled", false);
				$("#divFiltro").collapse("hide");
				$("#divDetalle").collapse("show");
				$("#IdSelected").val(ctaId);
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

function BuscarProductosPorFamilia(ctaId) {
	data = { ctaId };
	PostGenHtml(data, buscarFamiliaUrl, function (obj) {
		$("#divDatosReasignacion").html(obj);
		var fliaSelected = $("#listaFamilia").val();
		if (fliaSelected && fliaSelected !== "") {
			CargarProductosPorFamiliaSeleccionada(ctaId, fliaSelected);
		}
	}, function (obj) {
		ControlaMensajeError(obj.message);
		CerrarWaiting();
		return true;
	});
}

function CargarProductosPorFamiliaSeleccionada(ctaId, fliaSelected) {
	AbrirWaiting("Aguarde unos instantes mientras buscamos los productos de la familia seleccionada...");
	var data = { ctaId, fliaSelected };
	PostGenHtml(data, buscarProductosPorFamiliaUrl, function (obj) {
		$("#divGridProductosPorFamilia").html(obj);
		AgregarHandlerAGrillaProdPorFlia();
		CerrarWaiting();
	}, function (obj) {
		CerrarWaiting();
		ControlaMensajeError(obj.message);
		return true;
	});
}

function AgregarHandlerAGrillaProdPorFlia() {
	var dataTable = document.getElementById('tbProductosPorFamilia');
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

function actualizarListaDeProductos() {
	var fliaSelected = $("#listaFamilia").val();
	CargarProductosPorFamiliaSeleccionada(ctaId, fliaSelected);
}

function analizaEstadoBtnDetalle() {
	var res = $("#divDetalle").hasClass("show");
	if (res === true) {
		selectReg(regSelected, Grids.GridProveedor);
	}
	return true;

}

function InicializaPantalla() {
	var tb = $("#tbGridProveedor tbody tr");
	if (tb.length === 0) {
		$("#divFiltro").collapse("show")
	}
	$("#divDetalle").collapse("hide");
	$("#divGrilla").html(`<p style="background-color: gray"><h5> No se han especificado datos aún.</h5 ></p >`);
	//$("#divGrilla").collapse("hide");
	//$("#divGrilla").addClass("collapse");
	$("#divPaginacion").collapse("hide");
	$("#divPaginacion").addClass("collapse");

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
	$("#chkRel01").prop('checked', false);
	$("#chkRel01").trigger("change");
	$("#Rel01").val("");
	$("#Rel01").prop("disabled", true);
	//
	$(".activable").prop("disabled", true);
	$("#btnDetalle").prop("disabled", true);
	activarBotones(false);
	CerrarWaiting();
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