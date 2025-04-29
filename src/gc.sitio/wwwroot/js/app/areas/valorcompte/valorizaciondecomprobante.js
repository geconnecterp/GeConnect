$(function () {
	$("input#Rel01").on("click", function () {
		$("input#Rel01").val("");
		$("#Rel01Item").val("");
		$("#chkRel04").prop('checked', false);
		$("#chkRel04").trigger("change");
		$("input#Rel03").prop("disabled", true);
		$("input#Rel04").prop("disabled", true);
	});
	$(document).on("change", "#listaComptesPend", ControlaListaCompteSelected);
	$(document).on("click", "#btnAceptarDescFinanc", AceptarDescFinanc);
	$(document).on("mouseup", "#tbListaDescFinanc tbody tr", function (e) {
		setTimeout(() => {
			RecalcularItemValue()
		}, 500);
	});

	InicializarPantallaDeFiltros();
});

function moveColumn(table, sourceIndex, targetIndex) {
	console.log("Move Col " + sourceIndex + " to " + targetIndex);
	var body = $("tbody", table);
	$("tr", body).each(function (i, row) {
		$("td", row).eq(sourceIndex).insertAfter($("td", row).eq(targetIndex - 1));
	});
}

function InicializarPantallaDeFiltros() {
	$("#Rel01").prop("disabled", false);
	$("#lbRel01").text("Proveedor")
	$("#chkRel01").prop('checked', true);
	$("#chkRel01").trigger("change");
	$("#chkRel01").prop("disabled", true);
	$("#divFiltro").collapse("show");
	document.getElementById("Rel01").focus();
	MostrarDatosDeCuenta(false);

}

function AceptarDescFinanc() {
	var esValido = true;
	if ($("#listaConcDescFinanc").val() == "") {
		esValido = false;
		AbrirMensaje("ATENCIÓN", "Debe seleccionar un Concepto.", function () {
			$("#msjModal").modal("hide");
			document.getElementById("listaConcDescFinanc").focus();
			return true;
		}, false, ["Aceptar"], "error!", null);
	}
	if ($("#chkSobreTotal")[0].checked) {
		if ($("#DescFinanc_dto").inputmask('unmaskedvalue') <= 0) {
			esValido = false;
			AbrirMensaje("ATENCIÓN", "Debe prorcionar un valor mayor a 0.", function () {
				$("#msjModal").modal("hide");
				document.getElementById("DescFinanc_dto").focus();
				return true;
			}, false, ["Aceptar"], "error!", null);
		}
	}
	else {
		if ($("#DescFinanc_dto_importe").inputmask('unmaskedvalue') <= 0) {
			esValido = false;
			AbrirMensaje("ATENCIÓN", "Debe prorcionar un valor mayor a 0.", function () {
				$("#msjModal").modal("hide");
				document.getElementById("DescFinanc_dto_importe").focus();
				return true;
			}, false, ["Aceptar"], "error!", null);
		}
	}
	if (esValido) {
		AbrirWaiting("Actualizando Descuentos Financieros...");
		var cm_compte = $("#cm_compte").val();
		var dia_movi = $("#dia_movi").val();
		var dto_fijo = $("#chkNetoFijo")[0].checked;
		var dto_sobre_total = $("#chkSobreTotal")[0].checked;
		var tco_id = $("#tco_id").val();
		var dto = $("#DescFinanc_dto").inputmask('unmaskedvalue');
		var dto_importe = $("#DescFinanc_dto_importe").inputmask('unmaskedvalue');
		var dtoc_id = $("#listaConcDescFinanc").val();
		var dtoc_desc = $("#listaConcDescFinanc option:selected").text();
		var data = { cm_compte, dia_movi, dto_fijo, dto_sobre_total, tco_id, dto, dto_importe, dtoc_id, dtoc_desc }
		PostGenHtml(data, agregarDescFinancURL, function (obj) {
			CerrarWaiting();
			$("#divDescFinanc").html(obj);
			AddEventListenerToGrid("tbListaDescFinanc");
			LimpiarCamposEnDescFinanc();
			ActualizarListaValorizaciones();
			AgregarHandlerDragAndDrop();
		});
	}
}

function quitarDescFinanc(x) {
	AbrirWaiting("Eliminando Descuentos Financieros...");
	var item = $(x).attr("data-interaction");
	var data = { item };
	PostGenHtml(data, quitarDescFinancURL, function (obj) {
		CerrarWaiting();
		$("#divDescFinanc").html(obj);
		AddEventListenerToGrid("tbListaDescFinanc");
		AgregarHandlerDragAndDrop();
		ActualizarListaValorizaciones();
	});
}

function AgregarHandlerDragAndDrop() {
	$(".drageable-table > thead > tr").sortable({
		items: "> th.sortme",
		start: function (event, ui) {
			ui.item.data("source", ui.item.index());
		},
		update: function (event, ui) {
			moveColumn($(this).closest("table"), ui.item.data("source"), ui.item.index());
			$(".drageable-table > tbody").sortable("refresh");
		}
	});

	$(".drageable-table > tbody").sortable({
		items: "> tr.sortme"
	});
}

function AgregarHandlerAGrillaDetalleRprCheckAll() {
	var dataTable = document.getElementById('tbListaDetalleRpr');
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

function RecalcularItemValue() {
	AbrirWaiting();
	var index = 1;
	$("#tbListaDescFinanc").find('tr').each(function (i, el) {
		var td = $(this).find('td');
		if (td.length > 0 && td[0].innerText !== undefined) {
			td[0].innerText = index.toString();
			index++;
		}
	});
	ActualizarOrdenDeDescFinancEnBackEnd();
	CerrarWaiting();
}

function ActualizarOrdenDeDescFinancEnBackEnd() {
	var listaDesFinanc = [];
	$("#tbListaDescFinanc").find('tr').each(function (i, el) {
		var td = $(this).find('td');
		var $tr = $(this);
		if (td.length > 0 && td[0].innerText !== undefined) {
			var cm_compte = td[7].innerText;
			var dia_movi = td[8].innerText;
			var dto_fijo_bool = $tr.find(".chkNetoFijo").is(":checked");
			var dto_fijo = dto_fijo_bool; //tomar el valor bool
			var dto_sobre_total_bool = $tr.find(".chkDtoTot").is(":checked");
			var dto_sobre_total = dto_sobre_total_bool; //tomar el valor bool
			var tco_id = td[9].innerText;
			var dto = Number(td[4].innerText); //tomar valor decimal
			var dto_importe = Number(td[5].innerText.replace(',', '')); //tomar valor decimal
			var dtoc_id = td[10].innerText;
			var dtoc_desc = td[3].innerText;
			var item = td[0].innerText;
			var item_data = { cm_compte, dia_movi, dto_fijo, dto_sobre_total, tco_id, dto, dto_importe, dtoc_id, dtoc_desc, item };
			listaDesFinanc.push(item_data);
		}
	});
	if (listaDesFinanc.length > 0) {
		var data = { listaDesFinanc };
		PostGen(data, actualizarOrdenDescFinancURL, function (obj) {
			if (obj.error === true) {
				AbrirMensaje("ATENCIÓN", obj.msg, function () {
					$("#msjModal").modal("hide");
					return true;
				}, false, ["Aceptar"], "error!", null);
			}
			else {
				ActualizarListaValorizaciones();
			}
		});
	}
}

function ActualizarListaValorizaciones() {
	AbrirWaiting("Actualizando Valorizaciones...");
	var cm_compte = $("#cm_compte").val();
	var data = { cm_compte }
	PostGenHtml(data, actualizarValorizacionURL, function (obj) {
		$("#divListaValorizacion").html(obj);
		AddEventListenerToGrid("tbListaValorizacion");
		CerrarWaiting();
	});
}

function LimpiarCamposEnDescFinanc() {
	$("#listaConcDescFinanc").val("");
	$("#DescFinanc_dto").val(0);
	$("#DescFinanc_dto_importe").val(0);
	ActualizarEstadoChecks_SobreTotal();
}

function selectListaValorizacion(x) { }

function selectListaDescFinanc(x) { }

function selectListaDetalleRpr(x) {
	if (x) {
		pIdEnOcSeleccionado = x.cells[1].innerText.trim();
	}
	else {
		pIdSeleccionado = "";
	}
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

function ControlaListaCompteSelected() {
	if ($("#listaComptesPend").val() != "")
		cmCompteSelected = $("#listaComptesPend").val();
	else
		cmCompteSelected = "";
	if (cmCompteSelected != "") {
		//Cargar Detalles de Rpr y Dtos en el Backend, y devolver la grilla de Valorizacion
		AbrirWaiting("Obteniendo datos de Valorización...");
		var cm_compte = cmCompteSelected;
		data = { cm_compte };
		PostGenHtml(data, cargarDatosParaValorizarURL, function (obj) {
			CerrarWaiting();
			if (obj.error === true) {
				AbrirMensaje("ATENCIÓN", obj.msg, function () {
					$("#msjModal").modal("hide");
					return true;
				}, false, ["Aceptar"], "error!", null);
			}
			else {
				$("#divComprobantes").html(obj);
				$("#divDetalle").collapse("show");
				$("#btnDetalle").prop("disabled", false);
				$("#divFiltro").collapse("hide")
				AddEventListenerToGrid("tbListaValorizacion");
				AddEventListenerToGrid("tbListaDescFinanc");
				MostrarDatosDeCuenta(true);
				$("#chkSobreTotal").on("click", function () {
					ActualizarEstadoChecks_SobreTotal();
					//ActualizarVisualizacionDeControlesABMDescFinanc();
				});
				$("#chkNetoFijo").on("click", function () {
					ActualizarEstadoChecks_NetoFijo();
					//ActualizarVisualizacionDeControlesABMDescFinanc();
				});
				ActualizarVisualizacionDeControlesABMDescFinanc();
				AplicarMascarasEnInput_Section_DescFinanc();
				ObtenerListaDetalleRpr();
				//AgregarHandlerAGrillaDetalleRprCheckAll()
			}
		});
	}
}

function ObtenerListaDetalleRpr() {
	AbrirWaiting("Obteniendo Detalles de Rpr...");
	var data = {};
	PostGenHtml(data, cargarListaDetalleRprURL, function (obj) {
		CerrarWaiting();
		$("#divDetalles").html(obj);
		AddEventListenerToGrid("tbListaDetalleRpr");
		addInCellKeyDownHandler();
		addInCellGotFocusHandler();
		addInCellLostFocusHandler();
		addMaskInEditableCells();
		tableUpDownArrow();
		AgregarHandlerAGrillaDetalleRprCheckAll();
	});
}

function AplicarMascarasEnInput_Section_DescFinanc() {
	getMaskForMoneyType("#DescFinanc_dto_importe");
	getMaskForDiscountType("#DescFinanc_dto");
}

function ActualizarEstadoChecks_SobreTotal() {
	if ($("#chkSobreTotal")[0].checked) {
		$("#chkNetoFijo").prop('checked', false);
		$("#chkNetoFijo").trigger("change");
		$("#divDescFinancDto").collapse("show");
		$("#divDescFinancDtoImporte").collapse("hide");
	}
	else {
		$("#chkNetoFijo").prop('checked', true);
		$("#chkNetoFijo").trigger("change");
		$("#divDescFinancDto").collapse("hide");
		$("#divDescFinancDtoImporte").collapse("show");
	}
	$("#DescFinanc_dto_importe").val(0);
	$("#DescFinanc_dto").val(0);
}

function ActualizarEstadoChecks_NetoFijo() {
	if ($("#chkNetoFijo")[0].checked) {
		$("#chkSobreTotal").prop('checked', false);
		$("#chkSobreTotal").trigger("change");
		$("#divDescFinancDto").collapse("hide");
		$("#divDescFinancDtoImporte").collapse("show");
	}
	else {
		$("#chkSobreTotal").prop('checked', true);
		$("#chkSobreTotal").trigger("change");
		$("#divDescFinancDto").collapse("show");
		$("#divDescFinancDtoImporte").collapse("hide");
	}
	$("#DescFinanc_dto_importe").val(0);
	$("#DescFinanc_dto").val(0);
}

function ActualizarVisualizacionDtoSobreTotal() {
	var auxSobreTotal = $("#chkSobreTotal")[0].checked;
	if (auxSobreTotal) {
		$("#divDescFinancDto").collapse("show");
		$("#divDescFinancDtoImporte").collapse("hide");
	}
	else {
		$("#divDescFinancDto").collapse("hide");
		$("#divDescFinancDtoImporte").collapse("show");
	}
}

function ActualizarVisualizacionDeControlesABMDescFinanc() {

	var auxNetoFijo = $("#chkNetoFijo")[0].checked;

	if (auxNetoFijo) {
		$("#divDescFinancDto").collapse("hide");
		$("#divDescFinancDtoImporte").collapse("show");
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
		$("#chkRel04").prop("disabled", false);
		CargarComprobantesDelProveedorSeleccionado(ui.item.id);

		return true;
	}
});

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

function CargarComprobantesDelProveedorSeleccionado(id) {
	var ctaId = id;
	data = { ctaId };
	PostGen(data, cargarComprobantesDelProveedorSeleccionadoUrl, function (obj) {
		if (obj.error === true) {
			AbrirMensaje("ATENCIÓN", obj.msg, function () {
				$("#msjModal").modal("hide");
				return true;
			}, false, ["Aceptar"], "error!", null);
		}
		else {
			$("#divLstComptesPendiente").html(obj);
			$("#lbRel04").text("Comprobantes");
			addHandlerOnChkRel04_Click();
		}
	});
}

function addHandlerOnChkRel04_Click() {
	$("#chkRel04").on("click", function () {
		if ($("#chkRel04").is(":checked")) {
			$("#listaComptesPend").prop("disabled", false);
			$("#listaComptesPend").trigger("focus");

		}
		else {
			$("#listaComptesPend").prop("disabled", true).val("");
			cmCompteSelected = "";
		}
	});
}

function getMaskForDiscountType(selector) {
	$(selector).inputmask({
		alias: 'numeric',
		groupSeparator: '.',
		radixPoint: ',',
		digits: 1,
		digitsOptional: false,
		allowMinus: false,
		prefix: '',
		suffix: '',
		min: 0,
		max: 50,
		unmaskAsNumber: true
	});
}

function getMaskForMoneyType(selector) {
	$(selector).inputmask({
		alias: 'numeric',
		groupSeparator: '.',
		radixPoint: ',',
		digits: 2,
		digitsOptional: false,
		allowMinus: false,
		prefix: '',
		suffix: '',
		rightAlign: true,
		unmaskAsNumber: true
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

function keyUpFromEditableCell(x) {
	if (event.key == "Enter") {
		var id = $(x).prop("id");
		if (id.includes("ocd_plista")) {
			var arr_p_id = id.split("_");
			var p_id = arr_p_id[0];
			var next = p_id + "_ocd_dto1";
			$("#" + next).focus();
			return true;
		}
		if (id.includes("ocd_dto1")) {
			var arr_p_id = id.split("_");
			var p_id = arr_p_id[0];
			var next = p_id + "_ocd_dto2";
			$("#" + next).focus();
			return true;
		}
		if (id.includes("ocd_dto2")) {
			var arr_p_id = id.split("_");
			var p_id = arr_p_id[0];
			var next = p_id + "_ocd_dto3";
			$("#" + next).focus();
			return true;
		}
		if (id.includes("ocd_dto3")) {
			var arr_p_id = id.split("_");
			var p_id = arr_p_id[0];
			var next = p_id + "_ocd_dto4";
			$("#" + next).focus();
			return true;
		}
		if (id.includes("ocd_dto4")) {
			var arr_p_id = id.split("_");
			var p_id = arr_p_id[0];
			var next = p_id + "_ocd_dto_pa";
			$("#" + next).focus();
			return true;
		}
		if (id.includes("ocd_dto_pa")) {
			var arr_p_id = id.split("_");
			var p_id = arr_p_id[0];
			var next = p_id + "_ocd_boni";
			$("#" + next).focus();
			return true;
		}
		if (id.includes("ocd_boni")) {
			var arr_p_id = id.split("_");
			var p_id = arr_p_id[0];
			var next = p_id + "_rpd_plista";
			$("#" + next).focus();
			return true;
		}
		if (id.includes("rpd_plista")) {
			var arr_p_id = id.split("_");
			var p_id = arr_p_id[0];
			var next = p_id + "_rpd_dto1";
			$("#" + next).focus();
			return true;
		}
		if (id.includes("rpd_dto1")) {
			var arr_p_id = id.split("_");
			var p_id = arr_p_id[0];
			var next = p_id + "_rpd_dto2";
			$("#" + next).focus();
			return true;
		}
		if (id.includes("rpd_dto2")) {
			var arr_p_id = id.split("_");
			var p_id = arr_p_id[0];
			var next = p_id + "_rpd_dto3";
			$("#" + next).focus();
			return true;
		}
		if (id.includes("rpd_dto3")) {
			var arr_p_id = id.split("_");
			var p_id = arr_p_id[0];
			var next = p_id + "_rpd_dto4";
			$("#" + next).focus();
			return true;
		}
		if (id.includes("rpd_dto4")) {
			var arr_p_id = id.split("_");
			var p_id = arr_p_id[0];
			var next = p_id + "_rpd_dto_pa";
			$("#" + next).focus();
			return true;
		}
		if (id.includes("rpd_dto_pa")) {
			var arr_p_id = id.split("_");
			var p_id = arr_p_id[0];
			var next = p_id + "_rpd_boni";
			$("#" + next).focus();
			return true;
		}
		if (id.includes("rpd_boni")) {
			var arr_p_id = id.split("_");
			var p_id = arr_p_id[0];
			var next = p_id + "_rpd_cantidad_compte";
			$("#" + next).focus();
			return true;
		}
		if (id.includes("_rpd_cantidad_compte")) {
			var arr_p_id = id.split("_");
			var p_id = arr_p_id[0];
			var next = p_id + "_ocd_plista";
			$("#" + next).focus();
			return true;
		}
	}
}

var keysAceptadas = [8, 37, 39, 46, 96, 97, 98, 99, 100, 101, 102, 103, 104, 105, 110, 190];
function addInCellKeyDownHandler() {
	$("#tbListaDetalleRpr").on('keydown', 'td[contenteditable]', function (e) {
		if (isNaN(String.fromCharCode(e.which)) && !(keysAceptadas.indexOf(e.which) != -1) && !(e.shiftKey && (e.which == 37 || e.which == 39))) e.preventDefault();
	});
}

function addInCellGotFocusHandler() {
	$("#tbListaDetalleRpr").on('focusin', 'td[contenteditable]', function (e) {
		cellValueTemp = $("#" + this.id).text();
		if (e.target) {
			cellIndexTemp = e.target.cellIndex;
			console.log("cellIndexTemp " + cellIndexTemp);
		}
	});
}

function addInCellLostFocusHandler() {
	$("#tbListaDetalleRpr").on('focusout', 'td[contenteditable]', function (e) {
		var actualiza = true;
		if (cellValueTemp == $("#" + this.id).text())
			actualiza = false;
		else {
			if (this.id.includes("_dto1") || this.id.includes("_dto2") || this.id.includes("_dto3") || this.id.includes("_dto4") || this.id.includes("_dto_pa") || this.id.includes("_cantidad_compte")) {
				var valor = this.innerText;
			}
			else if (this.id.includes("_plista")) {
				var valor = $("#" + this.id).inputmask('unmaskedvalue');
			}
			else if (this.id.includes("_boni")) {
				var spl = this.innerText.split("/");
				if (spl.length === 2) {
					var num1 = Number(spl[0]);
					var num2 = Number(spl[1]);
					if (num1 > num2) {
						$("#" + this.id).val("");
						$("#" + this.id).text("");
						actualiza = false;
					}
					var valor = this.innerText;
				}
				else
					actualiza = false;
			}
		}
		if (actualiza) {
			if (this.id.includes("_ocd_"))
				ActualizarProductoEnDetalleRprSeccionPrecio(this.id, valor);
			else
				ActualizarProductoEnDetalleRprSeccionFactura(this.id, valor);
		}
	});
}

function ActualizarProductoEnDetalleRprSeccionPrecio(field, val) {
	var pId = pIdEnOcSeleccionado;
	var data = { pId, field, val };
	PostGen(data, actualizarProdEnRprSeccionPrecioURL, function (obj) {
		if (obj.error === true) {
			AbrirMensaje("ATENCIÓN", obj.msg, function () {
				$("#msjModal").modal("hide");
				return true;
			}, false, ["Aceptar"], "error!", null);
		}
		else {
			//Actualizar valores en la grilla
			$("#tbListaDetalleRpr").find('tr').each(function (i, el) {
				var td = $(this).find('td');
				if (td.length > 0 && td[1].innerText !== undefined && td[1].innerText === pId) {
					console.log(obj.costo);
					td[10].innerText = obj.costo;
				}
			});
		}
	});
}

function ActualizarProductoEnDetalleRprSeccionFactura(field, val) {
	var pId = pIdEnOcSeleccionado;
	var data = { pId, field, val };
	PostGen(data, actualizarProdEnRprSeccionFacturaURL, function (obj) {
		if (obj.error === true) {
			AbrirMensaje("ATENCIÓN", obj.msg, function () {
				$("#msjModal").modal("hide");
				return true;
			}, false, ["Aceptar"], "error!", null);
		}
		else {
			//Actualizar valores en la grilla
			$("#tbListaDetalleRpr").find('tr').each(function (i, el) {
				var td = $(this).find('td');
				if (td.length > 0 && td[1].innerText !== undefined && td[1].innerText === pId) {
					console.log(obj.costo);
					td[19].innerText = obj.costo;
				}
			});
		}
	});
}

///TODO MARCE: Revisar y actualizar este metodo
function tableUpDownArrow() {
	var table = document.querySelector('#tbListaDetalleRpr tbody');
	if (table == undefined)
		return;
	if (table.rows[0] == undefined)
		return;
	const myTable = table
		, nbRows = myTable.rows.length
		, nbCells = myTable.rows[0].cells.length
		, movKey = {
			ArrowUp: p => { p.r = (--p.r + nbRows) % nbRows }
			, ArrowLeft: p => { p.c = (--p.c + nbCells) % nbCells }
			, ArrowDown: p => {
				p.r = ++p.r % nbRows
			}
			, ArrowRight: p => { p.c = ++p.c % nbCells }
			, Tab: p => {
				p.r = ++p.r % nbRows
			}
		}

	myTable
		.querySelectorAll('input, [contenteditable=true]')
		.forEach(elm => {
			elm.onfocus = e => {
				let sPos = myTable.querySelector('.selected-row')
					, tdPos = elm.parentNode

				if (sPos) sPos.classList.remove('selected-row')

				tdPos.classList.add('selected-row')
			}
		})


	document.onkeydown = e => {
		let sPos = myTable.querySelector('.selected-row')
			, evt = (e == null ? event : e)
			, pos = {
				r: sPos ? sPos.rowIndex - 1 : -1 //sPos.rowIndex -1 => porque tiene doble fila en la cabecera
				, c: sPos ? (sPos.cellIndex ? sPos.cellIndex : cellIndexTemp) : -1
			}

		if (sPos &&
			//(evt.altKey && evt.shiftKey && movKey[evt.code])
			(evt.shiftKey && movKey[evt.code])
			||
			(evt.ctrlKey && movKey[evt.code])
		) {
			let loop = true
				, nxFocus = null
				, cell = null

			do {
				if (evt.code === 'ArrowDown' && pos.r == nbRows)
					pos.r = 0;
				if (evt.code === 'Tab' && evt.shiftKey && pos.r == 0)
					pos.r = nbRows - 1;
				if (evt.code === 'Tab' && evt.shiftKey) {
					movKey['ArrowUp'](pos)
				}
				else
					movKey[evt.code](pos);

				if (pos.r == nbRows)
					cell = myTable.rows[pos.r - 1].cells[pos.c];
				else
					cell = myTable.rows[pos.r].cells[pos.c];
				if (pos.r == 0)
					pos.r = nbRows;
				else if (pos.r == nbRows)
					pos.r = nbRows;

				///TODO MARCE: Revisar esto, actualizarlo para la lista actual
				if (pos.c == 10 && cellIndexTemp < pos.c) //moviendome desde la columna 'ocd_boni' hacia la derecha, la cual no es editable, debo saltar a la siguiente editable 'rpd_plista'
					pos.c = 12;
				if (pos.c == 19 && cellIndexTemp < pos.c) //moviendome desde la columna 'rpd_boni' hacia la derecha, la cual no es editable, debo saltar a la siguiente editable 'rpd_cantidad_compte'
					pos.c = 20;

				if (pos.c == 19 && cellIndexTemp > pos.c) //moviendome desde la columna 'rpd_cantidad_compte' hacia la izquierda, la cual no es editable, debo saltar a la siguiente editable 'rpd_boni'
					pos.c = 18;
				if (pos.c == 11 && cellIndexTemp > pos.c) //moviendome desde la columna 'rpd_plista' hacia la izquierda, la cual no es editable, debo saltar a la siguiente editable 'ocd_boni'
					pos.c = 9;
				
				nxFocus = myTable.rows[pos.r - 1].cells[pos.c]

				if (nxFocus
					&& cell.style.display !== 'none'
					&& cell.parentNode.style.display !== 'none') {
					nxFocus.focus();
					nxFocus.closest('tr').classList.add('selected-row');
					nxFocus.focus();
					loop = false
				}
			}
			while (loop)
			if (evt.code === 'Tab') {
				event.preventDefault();
			}
		}
		else if (evt.code === 'Enter')
			event.preventDefault();
		else if (evt.code === 'NumpadEnter')
			event.preventDefault();
	}
}

function addMaskInEditableCells() {
	if ($("#tbListaDetalleRpr").length != 0) {
		$("#tbListaDetalleRpr").find('tr').each(function (i, el) {
			var td = $(this).find('td');
			if (td.length == 23) {
				getMaskForMoneyType("#" + td[3].id); //_plista
				getMaskForDiscountType("#" + td[4].id);//_dto1
				getMaskForDiscountType("#" + td[5].id);//_dto2
				getMaskForDiscountType("#" + td[6].id);//p_dto3
				getMaskForDiscountType("#" + td[7].id);//p_dto4
				getMaskForDiscountType("#" + td[8].id);//p_dto_pa
				$("#" + td[9].id).mask("000/000", { reverse: false });//p_boni

				getMaskForMoneyType("#" + td[12].id); //_plista
				getMaskForDiscountType("#" + td[13].id);//_dto1
				getMaskForDiscountType("#" + td[14].id);//_dto2
				getMaskForDiscountType("#" + td[15].id);//p_dto3
				getMaskForDiscountType("#" + td[16].id);//p_dto4
				getMaskForDiscountType("#" + td[17].id);//p_dto_pa
				$("#" + td[18].id).mask("000/000", { reverse: false });//p_boni
				getMaskForDiscountType("#" + td[20].id);//p_dto_pa
			}
		});
	}
}

function getMaskForDiscountType(selector) {
	$(selector).inputmask({
		alias: 'numeric',
		groupSeparator: '.',
		radixPoint: ',',
		digits: 1,
		digitsOptional: false,
		allowMinus: false,
		prefix: '',
		suffix: '',
		min: 0,
		max: 50,
		unmaskAsNumber: true
	});
}

function getMaskForMoneyType(selector) {
	$(selector).inputmask({
		alias: 'numeric',
		groupSeparator: '.',
		radixPoint: ',',
		digits: 2,
		digitsOptional: false,
		allowMinus: false,
		prefix: '',
		suffix: '',
		rightAlign: true,
		unmaskAsNumber: true
	});
}