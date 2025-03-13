$(function () {
	$("#tituloLegend").text("Productos a cargar");
	$("#chkRel01").prop('checked', true);
	$("#chkRel01").trigger("change");
	$("#chkRel01").prop("disabled", true);
	$("#Rel01").prop("disabled", false);
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
		$("input#Rel03").prop("disabled", true);
		$("input#Rel04").prop("disabled", true);
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
		if (ctaIdSelected == "") {
			AbrirMensaje("ATENCIÓN", "Debe seleccionar una cuenta.", function () {
				$("#msjModal").modal("hide");
				$("input#Rel01").focus();
				return true;
			}, false, ["Aceptar"], "error!", null);
		}
		else {
			dataBak = "";
			pagina = 1;
			BuscarProductos(pagina);
		}
	});

	funcCallBack = BuscarProductos;
	InicializaPantalla();
	$("#Rel01").focus();
	$("#pagEstado").on("change", function () {
		var div = $("#divPaginacion");
		presentaPaginacionOC(div);
	});
	return true;
});

// Create our number formatter.
const formatter = new Intl.NumberFormat('en-US', {
	style: 'currency',
	currency: 'USD',

	// These options can be used to round to whole numbers.
	trailingZeroDisplay: 'stripIfInteger'   // This is probably what most people
	// want. It will only stop printing
	// the fraction when the input
	// amount is a round number (int)
	// already. If that's not what you
	// need, have a look at the options
	// below.
	//minimumFractionDigits: 0, // This suffices for whole numbers, but will
	// print 2500.10 as $2,500.1
	//maximumFractionDigits: 0, // Causes 2500.99 to be printed as $2,501
});

function focusOnTd(x) {
	var cell = x;
	var range, selection;
	if (document.body.createTextRange) {
		range = document.body.createTextRange();
		range.moveToElementText(cell);
		range.select();
	} else if (window.getSelection) {
		selection = window.getSelection();
		range = document.createRange();
		range.selectNodeContents(cell);
		selection.removeAllRanges();
		selection.addRange(range);
	}
}

function presentaPaginacionOC(div) {
	div.pagination({
		items: totalRegs,
		itemsOnPage: pagRegs,
		cssStyle: "dark-theme",
		currentPage: pagina,
		onPageClick: function (num) {
			BuscarProductos(num);
		}
	});
	$("#pagEstado").val(false);
	$("#divFiltro").collapse("hide")
	return true;
}

/// Funcion que restaura el estado del producto luego de quitarlo de la lista de OC
function ActualizarInfoDeProductoEnGrilla(pId) {
	$("#tbListaProducto").find('tr').each(function (i, el) {
		var td = $(this).find('td');
		if (td.length > 0 && td[0].innerText !== undefined) {
			var p_Id = td[0].innerText;
			if (p_Id === pId) {
				var id = "a" + pId;
				$("#" + id).addClass('btn-outline-success').removeClass('btn-outline-danger');
				$("#" + id).prop('title', '');
			}
		}
	});
}

/// Funcion que evalúa si el producto que existe en la grilla del primer Tab ya esta cargado en la grilla de OC, si es así cambiar el estilo del icono.
function ActualizarInfoDeProductosEnGrilla() {
	if ($("#tbListaProductoOC").length != 0) {
		var idArrayOC = [];
		$("#tbListaProductoOC").find('tr').each(function (i, el) {
			var td = $(this).find('td');
			if (td.length > 0 && td[1].innerText !== undefined) {
				idArrayOC.push(td[1].innerText);
			}
		});
		
		if (idArrayOC.length > 0 && $("#tbListaProducto").length != 0) {
			$("#tbListaProducto").find('tr').each(function (i, el) {
				var td = $(this).find('td');
				if (td.length > 0 && td[0].innerText !== undefined) {
					var pId = td[0].innerText;
					if (idArrayOC.find(x => x === pId)) {
						var id = "a" + pId;
						$("#" + id).addClass('btn-outline-danger').removeClass('btn-outline-success');
						$("#" + id).prop('title', 'Producto existente en OC.');
					}
				}
			});
		}
	}
}

function AgregarHandlerAGrillaProdOC() {
	var dataTable = document.getElementById('tbListaProductoOC');
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

function quitarProductoEnOC(e) {
	var pId = $(e).attr("data-interaction");
	var data = { pId };
	PostGenHtml(data, QuitarProductoEnOcURL, function (obj) {
		if (obj.error === true) {
			AbrirMensaje("ATENCIÓN", obj.msg, function () {
				$("#msjModal").modal("hide");
				return true;
			}, false, ["Aceptar"], "error!", null);
		}
		else {
			$("#divListaProductoNuevaOC").html(obj);
			AgregarHandlerAGrillaProdOC();
			ActualizarInfoDeProductoEnGrilla(pId);
		}
	});
}

function actualizarProducto(e) {
	if ($(e).hasClass("btn-outline-success")) {
		AbrirWaiting("Actualizando información de Orden de Compra.");
		var pId = $(e).attr("data-interaction");
		var data = { pId };
		PostGenHtml(data, AgregarProductoEnOcURL, function (obj) {
			if (obj.error === true) {
				CerrarWaiting();
				AbrirMensaje("ATENCIÓN", obj.msg, function () {
					$("#msjModal").modal("hide");
					return true;
				}, false, ["Aceptar"], "error!", null);
			}
			else {
				$("#divListaProductoNuevaOC").html(obj);
				AgregarHandlerAGrillaProdOC();
				ActualizarInfoDeProductosEnGrilla();
				CerrarWaiting();
			}
		});
	}
	else if ($(e).hasClass("btn-outline-secondary")) {
		AbrirMensaje("ATENCIÓN", "El producto seleccionado esta discontínuo.", function () {
			$("#msjModal").modal("hide");
			return true;
		}, false, ["Aceptar"], "error!", null);
	}
	else if ($(e).hasClass("btn-outline-danger")) {
		AbrirMensaje("ATENCIÓN", "El producto seleccionado ya esta incluído en la OC.", function () {
			$("#msjModal").modal("hide");
			return true;
		}, false, ["Aceptar"], "error!", null);
	}
	else {
		console.log("chan!");
	}
}

function actualizarProductoEnOC(e) { }

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
	$("#btnAbmAceptar").hide();
	$("#btnAbmCancelar").hide();
	$("#btnDetalle").prop("disabled", true);
	//activarBotones(false);
	ocIdSelected = "";
	ctaIdSelected = "";
	MostrarDatosDeCuenta(false);
	CerrarWaiting();
	return true;
}

function selectListaProductoRow(x) {
	if (x) {
		pIdSeleccionado = x.cells[0].innerText.trim();
		//BuscarInfoAdicional();
	}
	else {
		pIdSeleccionado = "";
	}
}

function selectListaProductoRowOC(x) { }

function BuscarProductosTabOC() {

	if (ocIdSelected && ocIdSelected != "") {
		$("#btnTabNuevaOC").text(ocIdSelected);
	}
	else {
		$("#btnTabNuevaOC").text("Nueva OC");
	}
	var ocCompte = ocIdSelected;
	var ctaId = ctaIdSelected;
	data = { ctaId, ocCompte }
	PostGenHtml(data, BuscarProductosTabOCURL, function (obj) {
		if (obj.error === true) {
			AbrirMensaje("ATENCIÓN", obj.msg, function () {
				$("#msjModal").modal("hide");
				return true;
			}, false, ["Aceptar"], "error!", null);
		}
		else {
			$("#divListaProductoNuevaOC").html(obj);
			AgregarHandlerAGrillaProdOC();
			ActualizarInfoDeProductosEnGrilla();
		}
	});
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
	Rel01.push(ctaIdSelected);
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
	PostGenHtml(data, BuscarProductosURL, function (obj) {
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
		BuscarProductosTabOC();
		$("#btnAbmAceptar").show();
		$("#btnAbmCancelar").show();
		$("#btnDetalle").prop("disabled", false);
		MostrarDatosDeCuenta(true);
		CargarTopesDeOC();
		CerrarWaiting();
		viendeDesdeBusquedaDeProducto = false;
		return true
	});
}

function CargarTopesDeOC() {
	data = {};
	PostGen(data, ObtenerTopesDeOcURL, function (obj) {
		if (obj.error === true) {
			AbrirMensaje("ATENCIÓN", obj.msg, function () {
				$("#msjModal").modal("hide");
				return true;
			}, false, ["Aceptar"], "error!", null);
		}
		else if (obj.warn === true) {
			AbrirMensaje("ATENCIÓN", obj.msg, function () {
				$("#msjModal").modal("hide");
				return true;
			}, false, ["Aceptar"], "error!", null);
		}
		else {
			console.log(obj);
			//formatter.format(e.target.value);
			$("#Lim_Mensual").val(formatter.format(obj.data.oc_limite_semanal));
			$("#OC_Emitidas").val(formatter.format(obj.data.oc_emitidas));
			$("#Tope_Emision").val(formatter.format(obj.data.oc_tope));
		}
	});
}

function MostrarDatosDeCuenta(mostrar) {
	if (mostrar) {
		$("#CtaID").val(ctaIdSelected);
		$("#CtaDesc").val(ctaDescSelected);
		$("#divProveedorSeleccionado").collapse("show");
	}
	else {
		$("#CtaID").val("");
		$("#CtaDesc").val("");
		$("#divProveedorSeleccionado").collapse("hide");
	}
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
		ctaIdSelected = ui.item.id;
		ctaDescSelected = ui.item.value;
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
		ocIdSelected = ui.item.id;
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