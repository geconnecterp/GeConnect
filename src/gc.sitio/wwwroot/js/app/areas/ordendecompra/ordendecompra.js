$(function () {
	$("#tituloLegend").text("Productos a cargar");
	$("#chkRel01").prop('checked', true);
	$("#chkRel01").trigger("change");
	$("#chkRel01").prop("disabled", true);
	$("input#Rel01").on("click", function () {
		$("input#Rel01").val("");
		$("#Rel01Item").val("");
		$("input#Rel03").val("");
		$("#Rel03Item").val("");
		$("#Rel03List").empty();
		$("input#Rel04").val("");
		$("#Rel04Item").val("");
		$("#chkRel03").prop('checked', false);
		$("#chkRel03").trigger("change");
		$("#chkRel04").prop('checked', false);
		$("#chkRel04").trigger("change");
	});
	$("input#Rel03").on("click", function () {
		$("input#Rel03").val("");
		$("#Rel03Item").val("");
	});
	$("input#Rel04").on("click", function () {
		$("input#Rel04").val("");
		$("#Rel04Item").val("");
	});
	$("#btnBuscar").on("click", function () {
		dataBak = "";
		pagina = 1;
		BuscarProductos(pagina);
	});
	funcCallBack = BuscarProductos;
	InicializaPantalla();
	return true;
});

function InicializaPantalla() {
	var tb = $("#tbListaProducto tbody tr");
	if (tb.length === 0) {
		$("#divFiltro").collapse("show")
	}

	$("#lbRel01").text("Proveedor");
	$("#lbRel02").text("Rubro");
	$("#lbRel03").text("Familia");
	$("#lbRel04").text("OC Pendiente");
	$("#lbChkDescr").text("Descripción");
	$("#lbDescr").html("Desc");

	$("#lbchk01").text("Alta Rotación");
	$("#lbchk02").text("Con PI");
	$("#lbchk03").text("Con OC");
	$("#lbchk04").text("Sin Stk");
	$("#lbchk05").text("Con Stk a Vencer");

	$("#lbChkDesdeHasta").text("ID Producto");

	//$("#IdSelected").val("");
	$(".activable").prop("disabled", true);
	$("#chkRel03").prop("disabled", true);
	//activarBotones(false);
	CerrarWaiting();
	return true;
}

function BuscarProductos(pag = 1) {
	viendeDesdeBusquedaDeProducto = true;
	AbrirWaiting();
	var Tipo = tipoDeOperacion;
	var Buscar = $("#Buscar").val();
	var Id = $("#Id").val();
	var Id2 = $("#Id2").val();
	var Rel01 = [];
	var Rel02 = [];
	var Rel03 = [];
	$("#Rel01List").children().each(function (i, item) { Rel01.push($(item).val()) });
	$("#Rel02List").children().each(function (i, item) { Rel02.push($(item).val()) });
	$("#Rel03List").children().each(function (i, item) {
		var aux = { Id: $(item).val(), Descripcion: $(item).text() };
		Rel03.push(aux);
	});

	var Opt1 = $("#chk01")[0].checked
	var Opt2 = $("#chk02")[0].checked
	var Opt3 = $("#chk03")[0].checked
	var Opt4 = $("#chk04")[0].checked
	var Opt5 = $("#chk05")[0].checked

	var buscaNew = true;
	pagina = pag;
	Pagina = pag;
	var sort = null;
	var sortDir = null
	var data2 = { sort, sortDir, Pagina, buscaNew }
	var data1 = { Tipo, Buscar, Id, Id2, Rel01, Rel02, Rel03, Opt1, Opt2, Opt3, Opt4, Opt5 };
	var data = $.extend({}, data1, data2);
	PostGenHtml(data, BuscarProductos, function (obj) {
		$("#divListaProducto").html(obj);
		$("#divDetalle").collapse("show");
		//addInCellEditHandler();
		//addInCellLostFocusHandler();
		//addInCellKeyDownHandler();
		//AddEventListenerToGrid("tbListaProducto");
		//tableUpDownArrow();
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
		viendeDesdeBusquedaDeProducto = false;
		return true
	});
}

$("#Rel01").autocomplete({
	source: function (request, response) {

		data = { prefix: request.term }; Rel01

		$.ajax({
			url: autoComRel01Url,
			type: "POST",
			dataType: "json",
			data: data,
			success: function (obj) {
				response($.map(obj, function (item) {
					var texto = item.descripcion;
					return { label: texto, value: item.descripcion, id: item.id, prov: item.provId };
				}));
			}
		})
	},
	minLength: 3,
	select: function (event, ui) {
		$("#Rel01List").empty();
		$("#Rel01Item").val(ui.item.id);
		var opc = "<option value=" + ui.item.id + ">" + ui.item.value + "</option>"
		$("#Rel01List").append(opc);
		$("#chkRel03").prop("disabled", false);
		CargarFamiliaLista(ui.item.id);
		CargarOCLista(ui.item.id);
		//if ($("#Rel01List")[0].length === 1) {
		//	$("#chkRel03").prop("disabled", false);
		//	CargarFamiliaLista(ui.item.id);
		//	CargarOCLista(ui.item.id);
		//}
		//else {
		//	$("#chkRel03").prop("disabled", true);
		//	$("#Rel03").prop("disabled", true).val("");
		//	$("#Rel03List").prop("disabled", true).empty();
		//	$("#chkRel03")[0].checked = false;
		//}

		return true;
	}
});

//codigo generico para autocomplete 03
$("#Rel03").autocomplete({
	source: function (request, response) {

		data = { prefix: request.term }; Rel03

		$.ajax({
			url: autoComRel03Url,
			type: "POST",
			dataType: "json",
			data: data,
			success: function (obj) {
				response($.map(obj, function (item) {
					var texto = item.descripcion;
					return { label: texto, value: item.descripcion, id: item.id, prov: item.provId };
				}));
			}
		})
	},
	minLength: 3,
	select: function (event, ui) {
		if ($("#Rel03List").has('option:contains("' + ui.item.id + '")').length === 0) {
			$("#Rel03Item").val(ui.item.id);
			var opc = "<option value=" + ui.item.id + ">" + ui.item.value + "</option>"
			$("#Rel03List").append(opc);
		}
		return true;
	}
});

//codigo generico para autocomplete 03
$("#Rel04").autocomplete({
	source: function (request, response) {

		data = { prefix: request.term }; Rel03

		$.ajax({
			url: autoComRel04Url,
			type: "POST",
			dataType: "json",
			data: data,
			success: function (obj) {
				response($.map(obj, function (item) {
					var texto = item.descripcion;
					return { label: texto, value: item.descripcion, id: item.id, prov: item.provId };
				}));
			}
		})
	},
	minLength: 3,
	select: function (event, ui) {
		if ($("#Rel04List").has('option:contains("' + ui.item.id + '")').length === 0) {
			$("#Rel04Item").val(ui.item.id);
			var opc = "<option value=" + ui.item.id + ">" + ui.item.value + "</option>"
			$("#Rel04List").append(opc);
		}
		return true;
	}
});

function CargarFamiliaLista(id) {
	var ctaId = id;
	data = { ctaId };
	PostGen(data, buscarFamiliaDesdeProveedorSeleccionadoUrl, function (obj) {
		if (obj.error === true) {
			AbrirMensaje("ATENCIÓN", obj.msg, function () {
				$("#msjModal").modal("hide");
				return true;
			}, false, ["Aceptar"], "error!", null);
		}
		else {

		}
	});
}

function CargarOCLista(id) {
	var ctaId = id;
	data = { ctaId };
	PostGen(data, buscarOCDesdeProveedorSeleccionadoUrl, function (obj) {
		if (obj.error === true) {
			AbrirMensaje("ATENCIÓN", obj.msg, function () {
				$("#msjModal").modal("hide");
				return true;
			}, false, ["Aceptar"], "error!", null);
		}
		else {

		}
	});
}