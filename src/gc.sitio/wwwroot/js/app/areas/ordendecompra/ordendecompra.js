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
	$("#btnAbmAceptar").on("click", function () {
		console.log(this);
		ConfirmarOrdenDeCompra();
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

function ConfirmarOrdenDeCompra() {
	AbrirMensaje("ATENCIÓN", "¿Confirma la generación de la Orden de Compra?", function () {
		var data = { ocIdSelected };
		PostGen(data, ConfirmarOrdenDeCompraURL, function (obj) {
			if (obj.error === true) {
				AbrirMensaje("ATENCIÓN", obj.msg, function () {
					$("#msjModal").modal("hide");
					return true;
				}, false, ["Aceptar"], "error!", null);
			}
			else {
				AbrirMensaje("ATENCIÓN", obj.msg, function () {
					$("#msjModal").modal("hide");
					InicializaPantalla();
					return true;
				}, false, ["Aceptar"], "info!", null);
			}
		});
	}, false, ["Aceptar", "Cancelar"], "question!", null);
}

function ActualizarGrillaConceptos() {
	var Oc_Compte = ocIdSelected;
	var Entrega_Fecha = $("#FechaEntrega").val();
	var Entrega_Adm = $("#listaSucEntrega").val()
	var Pago_Anticipado = 'N';
	if ($("#chkPagoAnticipado")[0].checked)
		Pago_Anticipado = 'S';
	var Pago_Fecha = $("#PagoPlazo").val();
	var Observaciones = $("#Obs").val();
	var Oce_Id = 'P';
	if ($("#chkDejarOCActiva")[0].checked)
		Oce_Id = 'C';
	var data = { Oc_Compte, Entrega_Fecha, Entrega_Adm, Pago_Anticipado, Pago_Fecha, Observaciones, Oce_Id };
	console.log(data);
	PostGenHtml(data, ObtenerConceptoURL, function (obj) {
		if (obj.error === true) {
			AbrirMensaje("ATENCIÓN", obj.msg, function () {
				$("#msjModal").modal("hide");
				return true;
			}, false, ["Aceptar"], "error!", null);
		}
		else {
			$("#divGridConcepto").html(obj);
			FormatearValores("#tbGridConcepto", 1);
		}
	});
}

function onChangeFechaEntrega(e) {
	ActualizarGrillaConceptos();
}

function onChangePagoPlazo(e) {
	ActualizarGrillaConceptos();
}

function onChangeListaSucEntrega(e) {
	ActualizarGrillaConceptos();
}

function AplicarSeteoMasivo() {
	var alMenosUno = false;
	var dataTable = document.getElementById('tbListaProductoOC');
	var inputs = dataTable.querySelectorAll('tbody>tr>td>input');
	inputs.forEach(function (input) {
		if (input.checked) {
			alMenosUno = true;
		}
	});
	if (alMenosUno) {
		//Recorrer los items seleccionados y enviarlos al backend, junto con los valores de los campos de seteo masivo.
		var pIds = [];
		$("#tbListaProductoOC").find('tr').each(function (i, el) {
			var td = $(this).find('td');
			if (td.length > 0 && td[1].innerText !== undefined) {
				pIds.push(td[1].innerText);
			}
		});
		var dto1 = $("#Dto1").val();
		var dto2 = $("#Dto2").val();
		var dto3 = $("#Dto3").val();
		var dto4 = $("#Dto4").val();
		var dpa = $("#Dpa").val();
		var boolFlete = $("#chkFleteAPagar")[0].checked
		var flete = $("#Flete").val();
		var data = { pIds, dto1, dto2, dto3, dto4, dpa, boolFlete, flete };
		PostGenHtml(data, UpdateMasivoEnOcURL, function (obj) {
			if (obj.error === true) {
				AbrirMensaje("ATENCIÓN", obj.msg, function () {
					$("#msjModal").modal("hide");
					return true;
				}, false, ["Aceptar"], "error!", null);
			}
			else {
				$("#divListaProductoNuevaOC").html(obj);
				$("#Total_Costo").val(formatter.format($("#Total_Costo").val()));
				$("#Total_Pallet").val(formatter.format($("#Total_Pallet").val()));
				AgregarHandlerAGrillaProdOC();
				//ActualizarInfoDeProductosEnGrilla();
				addInCellLostFocusHandler();
				addInCellGotFocusHandler();
				addInCellKeyDownHandler();
				addInCellEditHandler();
				AddEventListenerToGrid("tbListaProductoOC");
			}
		});
	}
	else {
		AbrirMensaje("ATENCIÓN", "Debe seleccionar al menos un producto para la edición masiva.", function () {
			$("#msjModal").modal("hide");
			return true;
		}, false, ["Aceptar"], "error!", null);
	}
}

function addInCellEditHandler() {
	$("#tbListaProductoOC").on('input', 'td[contenteditable]', function (e) {
		//Do something
		var val = $("#" + this.id).text();
		if (this.id === "P_Dto1" || this.id === "P_Dto2" || this.id === "P_Dto3" || this.id === "P_Dto4" || this.id === "P_Dto_Pa") {
			var num = Number(val);
			if (num > 50) val = "50";
			$("#" + this.id).mask("00,0", { reverse: true });
			$("#" + this.id).val(val);
			$("#" + this.id).text(val);
		}
		else if (this.id === "P_Plista") {
			$("#" + this.id).mask("000.000.000.000,00", { reverse: false });
			$("#" + this.id).val(val);
		}
		else if (this.id === "P_Boni") {
			$("#" + this.id).mask("000/000", { reverse: false });
			$("#" + this.id).val(val);
		}
		else if (this.id === "Bultos") {
			$("#" + this.id).mask("000.000.000.000", { reverse: false });
			$("#" + this.id).val(val);
		}
		console.log($("#" + this.id).text());
	});
}

var keysAceptadas = [8, 37, 39, 46, 96, 97, 98, 99, 100, 101, 102, 103, 104, 105, 110, 190];
function addInCellKeyDownHandler() {
	$("#tbListaProductoOC").on('keydown', 'td[contenteditable]', function (e) {
		if (isNaN(String.fromCharCode(e.which)) && !(keysAceptadas.indexOf(e.which) != -1) && !(e.shiftKey && (e.which == 37 || e.which == 39))) e.preventDefault();
		//if (isNaN(String.fromCharCode(e.which)) && !(e.which == 8) && !(e.which == 110) && !(e.which == 37) && !(e.which == 39) && !(e.which == 46) && !(e.which == 190) && !(e.which >= 96 && e.which <= 105) && !(e.shiftKey && (e.which == 37 || e.which == 39))) e.preventDefault();
		cellValueTemp = $("#" + this.id).text();
	});
}

function addInCellGotFocusHandler() {
	$("#tbListaProductoOC").on('focusin', 'td[contenteditable]', function (e) {
		cellValueTemp = $("#" + this.id).text();
		console.log(cellValueTemp);
	});
}

function addInCellLostFocusHandler() {
	$("#tbListaProductoOC").on('focusout', 'td[contenteditable]', function (e) {
		var actualiza = true;
		if (cellValueTemp !== $("#" + this.id).text())
			actualiza = false;
		else {
			if (this.id === "P_Dto1" || this.id === "P_Dto2" || this.id === "P_Dto3" || this.id === "P_Dto4" || this.id === "P_Dto_Pa") {

			}
			else if (this.id === "P_Plista") {

			}
			else if (this.id === "P_Boni") {
				var spl = $("#" + this.id).val().split("/");
				if (spl.length === 2) {
					var num1 = Number(spl[0]);
					var num2 = Number(spl[1]);
					if (num1 > num2) {
						actualiza = false;
						$("#" + this.id).val("");
						$("#" + this.id).text("");
					}
				}
				else
					actualiza = false;
			}
		}
		if (actualiza) {
			ActualizarProductoEnOc(this.id, $("#" + this.id).val());
		}
	});
}

///Actualizar datos de producto, luego de la edicion de algunos de sus parámetros editables
function ActualizarProductoEnOc(field, val) {
	var pId = pIdEnOcSeleccionado;
	var data = { pId, field, val };
	PostGen(data, ActualizarProductoEnOcURL, function (obj) {
		if (obj.error === true) {
			AbrirMensaje("ATENCIÓN", obj.msg, function () {
				$("#msjModal").modal("hide");
				return true;
			}, false, ["Aceptar"], "error!", null);
		}
		else {
			//Actualizar valores en la grilla
			$("#tbListaProductoOC").find('tr').each(function (i, el) {
				var td = $(this).find('td');
				if (td.length > 0 && td[1].innerText !== undefined && td[1].innerText === pId) {
					console.log("innerText: " + td[1].innerText);
					console.log("obj: " + obj);
					//GRILLA
					td[16].innerText = obj.data.pedido_Mas_Boni;//PEDIDO +BONI -> obj.data.pedido_Mas_Boni
					td[17].innerText = obj.data.p_Pcosto;//PRECIO COSTO -> obj.data.p_Pcosto
					td[18].innerText = obj.data.p_Pcosto_Total;//TOTAL COSTO -> obj.data.p_Pcosto_Total
					td[19].innerText = obj.data.paletizado;//TOTAL PALLET -> obj.data.paletizado
					
					//TOTALES
					$("#Total_Costo").val(formatter.format(obj.data.total_Costo));//TOTAL_COSTO -> obj.data.total_Costo
					$("#Total_Pallet").val(formatter.format(obj.data.total_Pallet));//TOTAL_PALLET -> obj.data.total_Pallet
				}
			});
		}

	});
}

function CargarResumenDeOc() {
	if (ExitensItemsEnOC()) {
		var data = { ocIdSelected };
		PostGenHtml(data, CargarResumenDeOcURL, function (obj) {
			if (obj.error === true) {
				AbrirMensaje("ATENCIÓN", obj.msg, function () {
					$("#msjModal").modal("hide");
					return true;
				}, false, ["Aceptar"], "error!", null);
			}
			else {
				$("#divResumen").html(obj);
				FormatearValores("#tbGridConcepto", 1);
				$("#Obs").on('focusout', function (e) {
					ActualizarGrillaConceptos();
				});
				$("#chkPagoAnticipado").on("click", function () {
					ActualizarGrillaConceptos();
				});
				$("#chkDejarOCActiva").on("click", function () {
					ActualizarGrillaConceptos();
				});
			}
		});
	}
}

///Da formato monetario a los campos de tipo "money"
function FormatearValores(grilla, idx) {
	//grilla = "#tbListaProductoOC"
	$(grilla).find('tr').each(function (i, el) {
		var td = $(this).find('td');
		if (td.length > 0 && td[idx].innerText !== undefined) {
			td[idx].innerText = formatter.format(td[idx].innerText);
		}
	});
}

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

/// Funcion que restaura el estado del producto en la grilla del primer Tab, luego de quitarlo de la lista de OC (segundo Tab)
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

/// Funcion que evalúa si el producto de la grilla del primer Tab ya esta cargado en la grilla de OC (segundo Tab), si es así cambiar el estilo del icono.
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

function AddEventListenerToGrid(tabla) {
	var grilla = document.getElementById(tabla);
	if (grilla) {
		document.getElementById(tabla).addEventListener('click', function (e) {
			if (e.target.nodeName === 'TD') {
				var selectedRow = this.querySelector('.selected-row');
				if (selectedRow) {
					selectedRow.classList.remove('selected-row');
				}
				e.target.closest('tr').classList.add('selected-row');
			}
		});
	}
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
			AddEventListenerToGrid("tbListaProductoOC");
			ActualizarInfoDeProductoEnGrilla(pId);
			addInCellLostFocusHandler();
			addInCellGotFocusHandler();
			addInCellKeyDownHandler();
			addInCellEditHandler();
		}
	});
}

//Funcion que agrega el producto seleccionado en la grilla del primer, en la grilla de OC (Segundo Tab)
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
				AddEventListenerToGrid("tbListaProductoOC");
				ActualizarInfoDeProductosEnGrilla();
				addInCellLostFocusHandler();
				addInCellGotFocusHandler();
				addInCellKeyDownHandler();
				addInCellEditHandler();
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

//Funcion que actualiza los valores del elemento seleccionado y editado de la grilla de OC (segundo TAB)
//function actualizarProductoEnOC(e) {
//	var pId = $(e).attr("data-interaction");
//	var data = { pId };
//	PostGenHtml(data, ActualizarProductoEnOcURL, function (obj) {
//		if (obj.error === true) {
//			AbrirMensaje("ATENCIÓN", obj.msg, function () {
//				$("#msjModal").modal("hide");
//				return true;
//			}, false, ["Aceptar"], "error!", null);
//		}
//		else {
//			$("#divListaProductoNuevaOC").html(obj);
//			AgregarHandlerAGrillaProdOC();
//			AddEventListenerToGrid("tbListaProductoOC");
//			ActualizarInfoDeProductoEnGrilla(pId);
//			addInCellLostFocusHandler();
//			addInCellGotFocusHandler();
//			addInCellKeyDownHandler();
//			addInCellEditHandler();
//		}
//	});
//}

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
	activarBotones(false);
	ocIdSelected = "";
	ctaIdSelected = "";
	MostrarDatosDeCuenta(false);
	CerrarWaiting();
	return true;
}

function ExitensItemsEnOC() {
	if ($("#tbListaProductoOC").length != 0) {
		return true;
	}
	else {
		return false;
	}
}

function activarBotones(activar) {
	if (activar === true && ExitensItemsEnOC()) {
		$("#btnAbmAceptar").prop("disabled", false);
		$("#btnAbmCancelar").prop("disabled", false);
		$("#btnAbmAceptar").show();
		$("#btnAbmCancelar").show();
	}
	else {
		$("#btnAbmAceptar").prop("disabled", true);
		$("#btnAbmCancelar").prop("disabled", true);
		$("#btnAbmAceptar").hide();
		$("#btnAbmCancelar").hide();
	}
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

function selectListaProductoRowOC(x) {
	if (x) {
		pIdEnOcSeleccionado = x.cells[1].innerText.trim();
	}
	else {
		pIdSeleccionado = "";
	}
}

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
			$("#Total_Costo").val(formatter.format($("#Total_Costo").val()));
			$("#Total_Pallet").val(formatter.format($("#Total_Pallet").val()));
			AgregarHandlerAGrillaProdOC();
			ActualizarInfoDeProductosEnGrilla();
			addInCellLostFocusHandler();
			addInCellGotFocusHandler();
			addInCellKeyDownHandler();
			addInCellEditHandler();
			AddEventListenerToGrid("tbListaProductoOC");
			activarBotones(true);
			CargarResumenDeOc(); ///TODO MARCE: Descomentar y ver porque poronga no funciona el SP
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