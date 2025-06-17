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
	$(document).on("click", "#btnAplicarOC", ValidarOC);
	$(document).on("click", "#btnCostoActual", SetearCostoActual);
	$(document).on("click", "#btnAplicarSeteoMasivo", AplicarSeteoMasivo);
	$(document).on("click", "#btnCancelarDesdeDetalleRpr", CancelarDesdeDetalleRpr);
	$(document).on("click", "#btnCostoOC", SetearCostoDesdeOc);
	$(document).on("click", "#btnAceptarDesdeDetalleRpr", AceptarDesdeDetalleRpr);
	$(document).on("click", "#btnCancel", btnCancelClick);
	//

	$(document).on("click", "#btnGuardarValorizacion", GuardarValorizacion);
	$(document).on("click", "#btnConfirmarValorizacion", ConfirmarValorizacion);
	$(document).on("click", "#btnCancelarValorizacion", CancelarValorizacion);

	$(document).on("keyup", "#txtPLista", ControlaKeyUpTxtPLista);
	$(document).on("keyup", "#txtDto1", ControlaKeyUpTxtDto1);
	$(document).on("keyup", "#txtDto2", ControlaKeyUpTxtDto2);
	$(document).on("keyup", "#txtDto3", ControlaKeyUpTxtDto3);
	$(document).on("keyup", "#txtDto4", ControlaKeyUpTxtDto4);
	$(document).on("keyup", "#txtDpa", ControlaKeyUpTxtDpa);
	$(document).on("keyup", "#txtBoni", ControlaKeyUpTxtBoni);

	$(document).on("mouseup", "#tbListaDescFinanc tbody tr", function (e) {
		setTimeout(() => {
			RecalcularItemValue()
		}, 500);
	});

	InicializarPantallaDeFiltros();
});

function btnCancelClick() {
	CancelarValorizacion();
}

function ControlaKeyUpTxtPLista(e) {
	if (e.which == 13 || e.which == 109) {
		$("#txtDto1").trigger("focus");
	}
}

function ControlaKeyUpTxtDto1(e) {
	if (e.which == 13 || e.which == 109) {
		$("#txtDto2").trigger("focus");
	}
}

function ControlaKeyUpTxtDto2(e) {
	if (e.which == 13 || e.which == 109) {
		$("#txtDto3").trigger("focus");
	}
}

function ControlaKeyUpTxtDto3(e) {
	if (e.which == 13 || e.which == 109) {
		$("#txtDto4").trigger("focus");
	}
}

function ControlaKeyUpTxtDto4(e) {
	if (e.which == 13 || e.which == 109) {
		$("#txtDpa").trigger("focus");
	}
}

function ControlaKeyUpTxtDpa(e) {
	if (e.which == 13 || e.which == 109) {
		$("#txtBoni").trigger("focus");
	}
}

function ControlaKeyUpTxtBoni(e) {
	if (e.which == 13 || e.which == 109) {
		$("#btnAplicarSeteoMasivo").trigger("focus");
	}
}

function moveColumn(table, sourceIndex, targetIndex) {
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
	$("#btnDetalle").prop("disabled", true);
	$("#divFiltro").collapse("show");
	if ($("#divLstComptesPendiente") && $("#divLstComptesPendiente")[0]) {
		$("#divLstComptesPendiente")[0].innerHTML = "";
	}
	var obj = document.getElementById("Rel01");
	if (obj) {
		obj.focus();
	}
	MostrarDatosDeCuenta(false);

}

function LimpiarDatosDelFiltroInicial() {
	$("input#Rel01").val("");
	$("#Rel01Item").val("");
	$("#Rel01List").empty();
	$("#listaComptesPend").empty();
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
	else if ($("#chkSobreTotal")[0].checked) {
		if ($("#DescFinanc_dto").inputmask('unmaskedvalue') <= 0) {
			esValido = false;
			AbrirMensaje("ATENCIÓN", "Debe prorcionar un valor mayor a 0.", function () {
				$("#msjModal").modal("hide");
				document.getElem
				entById("DescFinanc_dto").focus();
				return true;
			}, false, ["Aceptar"], "error!", null);
		}
	}
	else if ($("#chkNetoFijo")[0].checked) {
		if ($("#DescFinanc_dto_importe").inputmask('unmaskedvalue') <= 0) {
			esValido = false;
			AbrirMensaje("ATENCIÓN", "Debe prorcionar un valor mayor a 0.", function () {
				$("#msjModal").modal("hide");
				document.getElementById("DescFinanc_dto").focus();
				return true;
			}, false, ["Aceptar"], "error!", null);
		}
	}
	else {
		if ($("#DescFinanc_dto").inputmask('unmaskedvalue') <= 0) {
			esValido = false;
			AbrirMensaje("ATENCIÓN", "Debe prorcionar un valor mayor a 0.", function () {
				$("#msjModal").modal("hide");
				document.getElementById("DescFinanc_dto").focus();
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
		var dto_obs = $("#DescFinanc_dto_obs").val();
		var data = { cm_compte, dia_movi, dto_fijo, dto_sobre_total, tco_id, dto, dto_importe, dtoc_id, dtoc_desc, dto_obs }
		PostGenHtml(data, agregarDescFinancURL, function (obj) {
			$("#divDescFinanc").html(obj);
			AddEventListenerToGrid("tbListaDescFinanc");
			LimpiarCamposEnDescFinanc();
			ActualizarListaValorizaciones();
			AgregarHandlerDragAndDrop();
			CerrarWaiting();
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
		CerrarWaiting();
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
			var dto_obs = td[11].innerText;
			var item_data = { cm_compte, dia_movi, dto_fijo, dto_sobre_total, tco_id, dto, dto_importe, dtoc_id, dtoc_desc, item, dto_obs };
			listaDesFinanc.push(item_data);
		}
	});
	if (listaDesFinanc.length > 0) {
		AbrirWaiting();
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
				CerrarWaiting();
			}
		});
	}
}

function ActualizarListaValorizaciones() {
	AbrirWaiting("");
	var cm_compte = $("#cm_compte").val();
	var dif_precio = $("#chkDifPrecio")[0].checked;
	var dif_cantidad = $("#chkDifCantidad")[0].checked;
	var idsProductos = ObtenerIdsProdSeleccionadosEnDetalleRpr();
	var checks = ObtenerCheckDeLosProductos();
	var data = { cm_compte, dif_precio, dif_cantidad, checks }
	PostGenHtml(data, actualizarValorizacionURL, function (obj) {
		$("#divListaValorizacion").html(obj);
		AddEventListenerToGrid("tbListaValorizacion");
		ValidarRespuestaDeObtencionDeValorizacion();

		$("#btnTabComprobantes").trigger("click");
		CerrarWaiting("");
	});
}

function ObtenerCheckDeLosProductos() {
	var lista = [];
	$("#tbListaDetalleRpr").find('tr').each(function (i, el) {
		var td = $(this).find('td');
		if (td.length > 0 && td[1].innerText !== undefined) {
			lista.push({ id: td[1].innerText.substring(0, 6), check: td[20].childNodes[0].checked });
		}
	});
	return lista;
}

function LimpiarCamposEnDescFinanc() {
	$("#listaConcDescFinanc").val("");
	$("#DescFinanc_dto_obs").val("");
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
			else if (e.target.nodeName === 'TR') {
				var selectedRow = this.querySelector('.selected-row');
				if (selectedRow) {
					selectedRow.classList.remove('selected-row');
				}
				e.target.classList.add('selected-row');
			}
		});
	}
}

function inCellInputEditable() {
	$("#tbListaDetalleRpr").on('focusout', 'input', function (e) {
		if (this.id) {
			var actualiza = true;
			if (cellValueTemp == $("#" + this.id).val())
				actualiza = false;
			else {
				if (this.id.includes("_plista") || this.id.includes("_rpd_cantidad_compte") || this.id.includes("_dto1") || this.id.includes("_dto2") || this.id.includes("_dto3") || this.id.includes("_dto4") || this.id.includes("_dto_pa")) {
					var valor = $("#" + this.id).inputmask('unmaskedvalue');
					if (valor == "") { //Backspace + Enter
						valor = "0";
						$("#" + this.id).val("0");
					}
				}
				else if (this.id.includes("_boni")) {
					var spl = $(this).val().split("/");
					if (spl.length === 2) {
						var num1 = Number(spl[0]);
						var num2 = Number(spl[1]);
						if (num1 > num2) {
							$("#" + this.id).val("");
							$("#" + this.id).text("");
							actualiza = false;
						}
						var valor = $(this).val();
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
		}
	});
}

function ControlaListaCompteSelected() {
	if ($("#listaComptesPend").val() != "")
		cmCompteSelected = $("#listaComptesPend").val();
	else
		cmCompteSelected = "";
	if (cmCompteSelected != "") {
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
				});
				$("#chkNetoFijo").on("click", function () {
					ActualizarEstadoChecks_NetoFijo();
				});
				ActualizarVisualizacionDeControlesABMDescFinanc();
				AplicarMascarasEnInput_Section_DescFinanc();
				ObtenerListaDetalleRpr();
				ValidarRespuestaDeObtencionDeValorizacion();
			}
		});
	}
}

function ValidarRespuestaDeObtencionDeValorizacion() {
	$("#tbListaValorizacion").find('tr').each(function (i, el) {
		var td = $(this).find('td');
		if (td.length > 0 && td[9].innerText !== undefined) {
			var cod = td[9].innerText;
			var msj = td[10].innerText;
			if (cod != "0") {
				AbrirMensaje("ATENCIÓN", msj, function () {
					$("#msjModal").modal("hide");
					return true;
				}, false, ["Aceptar"], "error!", null);
				$("#btnGuardarValorizacion").prop("disabled", true);
				$("#btnConfirmarValorizacion").prop("disabled", true);
			}
			else {
				$("#btnGuardarValorizacion").prop("disabled", false);
				$("#btnConfirmarValorizacion").prop("disabled", false);
			}
		}
	});
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
		addInCellInputGotFocusHandler();
		addInCellLostFocusHandler();
		addMaskInEditableCells();
		tableUpDownArrow();
		AgregarHandlerAGrillaDetalleRprCheckAll();
		inCellInputEditable();
		$('#radioSection input').on('change', function () {
			optSelected = $('input[name=opcion]:checked', '#radioSection').val();
			if (optSelected == "opcion1") {
				$("#sectionDeOtraOC").collapse("show");
				$("#sectionCostosEspecificos").collapse("hide");
			}
			else if (optSelected == "opcion4") {
				$("#sectionCostosEspecificos").collapse("show");
				$("#sectionDeOtraOC").collapse("hide");
			}
			else {
				$("#sectionCostosEspecificos").collapse("hide");
				$("#sectionDeOtraOC").collapse("hide");
			}
		});
	});
}

function AplicarMascarasEnInput_Section_DescFinanc() {
	getMaskForMoneyType("#DescFinanc_dto_importe");
	getMaskForDiscountType("#DescFinanc_dto");
}

function ActualizarEstadoChecks_SobreTotal() {
	if ($("#chkSobreTotal")[0].checked) {
		$("#divDescFinancDto").collapse("show");
		$("#divDescFinancDtoImporte").collapse("hide");
	}
	else {
		if (!$("#chkNetoFijo").is(':checked')) {
			$("#divDescFinancDto").collapse("show");
			$("#divDescFinancDtoImporte").collapse("hide");
		}
		else {
			$("#divDescFinancDto").collapse("hide");
			$("#divDescFinancDtoImporte").collapse("show");
		}
	}
	$("#DescFinanc_dto_importe").val(0);
	$("#DescFinanc_dto").val(0);
}

function ActualizarEstadoChecks_NetoFijo() {
	if ($("#chkNetoFijo")[0].checked) {
		$("#chkSobreTotal").prop('checked', false);
		$("#chkSobreTotal").trigger("change");
		$("#chkSobreTotal").prop('disabled', true);
		$("#divDescFinancDto").collapse("hide");
		$("#divDescFinancDtoImporte").collapse("show");
	}
	else {
		$("#chkSobreTotal").prop('disabled', false);
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
		min: 0,
		rightAlign: true,
		unmaskAsNumber: true,
		positionCaretOnClick: "lvp",
		onBeforeWrite: function (event, buffer, caretPos, opts) {
			//console.log("event: " + event);
		}
	});
}

function getMaskForBonificationType(selector) {
	$(selector).inputmask({
		alias: 'bonification',
		mask: "999/999",
	});
}

function focusOnInput(x) {
	if (x) {
		$('#' + x.id.substring(0, 6)).trigger('click');
		$("#" + x.id).select().one('mouseup', function (e) {
			$(this).off('keyup');
			e.preventDefault();
		}).one('keyup', function () {
			$(this).select().off('mouseup');
		});
	}
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
			document.getElementById(next).focus();
			return true;
		}
		if (id.includes("ocd_dto1")) {
			var arr_p_id = id.split("_");
			var p_id = arr_p_id[0];
			var next = p_id + "_ocd_dto2";
			document.getElementById(next).focus();
			return true;
		}
		if (id.includes("ocd_dto2")) {
			var arr_p_id = id.split("_");
			var p_id = arr_p_id[0];
			var next = p_id + "_ocd_dto3";
			document.getElementById(next).focus();
			return true;
		}
		if (id.includes("ocd_dto3")) {
			var arr_p_id = id.split("_");
			var p_id = arr_p_id[0];
			var next = p_id + "_ocd_dto4";
			document.getElementById(next).focus();
			return true;
		}
		if (id.includes("ocd_dto4")) {
			var arr_p_id = id.split("_");
			var p_id = arr_p_id[0];
			var next = p_id + "_ocd_dto_pa";
			document.getElementById(next).focus();
			return true;
		}
		if (id.includes("ocd_dto_pa")) {
			var arr_p_id = id.split("_");
			var p_id = arr_p_id[0];
			var next = p_id + "_ocd_boni";
			document.getElementById(next).focus();
			return true;
		}
		if (id.includes("ocd_boni")) {
			var arr_p_id = id.split("_");
			var p_id = arr_p_id[0];
			var next = p_id + "_rpd_plista";
			document.getElementById(next).focus();
			return true;
		}
		if (id.includes("rpd_plista")) {
			var arr_p_id = id.split("_");
			var p_id = arr_p_id[0];
			var next = p_id + "_rpd_dto1";
			document.getElementById(next).focus();
			return true;
		}
		if (id.includes("rpd_dto1")) {
			var arr_p_id = id.split("_");
			var p_id = arr_p_id[0];
			var next = p_id + "_rpd_dto2";
			document.getElementById(next).focus();
			return true;
		}
		if (id.includes("rpd_dto2")) {
			var arr_p_id = id.split("_");
			var p_id = arr_p_id[0];
			var next = p_id + "_rpd_dto3";
			document.getElementById(next).focus();
			return true;
		}
		if (id.includes("rpd_dto3")) {
			var arr_p_id = id.split("_");
			var p_id = arr_p_id[0];
			var next = p_id + "_rpd_dto4";
			document.getElementById(next).focus();
			return true;
		}
		if (id.includes("rpd_dto4")) {
			var arr_p_id = id.split("_");
			var p_id = arr_p_id[0];
			var next = p_id + "_rpd_dto_pa";
			document.getElementById(next).focus();
			return true;
		}
		if (id.includes("rpd_dto_pa")) {
			var arr_p_id = id.split("_");
			var p_id = arr_p_id[0];
			var next = p_id + "_rpd_boni";
			document.getElementById(next).focus();
			return true;
		}
		if (id.includes("rpd_boni")) {
			var arr_p_id = id.split("_");
			var p_id = arr_p_id[0];
			var next = p_id + "_rpd_cantidad_compte";
			document.getElementById(next).focus();
			return true;
		}
		if (id.includes("_rpd_cantidad_compte")) {
			var arr_p_id = id.split("_");
			var p_id = arr_p_id[0];
			var next = p_id + "_ocd_plista";
			document.getElementById(next).focus();
			return true;
		}
	}
}

var keysAceptadas = [8, 37, 39, 46, 96, 97, 98, 99, 100, 101, 102, 103, 104, 105, 110, 190];
function addInCellKeyDownHandler() {
	$("#tbListaDetalleRpr").on('keydown', 'td[contenteditable]', function (e) {
		if (e) {
			if (isNaN(String.fromCharCode(e.which)) && !(keysAceptadas.indexOf(e.which) != -1) && !(e.shiftKey && (e.which == 37 || e.which == 39))) { e.preventDefault(); }
		}
	});
}

function addInCellGotFocusHandler() {
	$("#tbListaDetalleRpr").on('focusin', 'td[contenteditable]', function (e) {
		if (e) {
			cellValueTemp = $("#" + this.id).text();
			if (e.target) {
				pIdEnOcSeleccionado = e.target.parentNode.cells[1].innerText;
				cellIndexTemp = e.target.cellIndex;
			}
		}
	});
}

function addInCellInputGotFocusHandler() {
	$("#tbListaDetalleRpr").on('focusin', 'input', function (e) {
		if (e) {
			cellValueTemp = $("#" + this.id).val();
			$(this).select();
			if (e.target) {
				pIdEnOcSeleccionado = e.target.parentNode.parentNode.cells[1].innerText;
				cellIndexTemp = e.target.parentNode.cellIndex;
			}
		}
	});
}

function onChangeChkNcGenera(x) {
	if (x) {
		$(".nav-link").prop("disabled", true);
	}
	else
		event.preventDefault;
}

function addInCellLostFocusHandler() {
	$("#tbListaDetalleRpr").on('focusout', 'td[contenteditable]', function (e) {
		if (this.id) {
			var actualiza = true;
			if (cellValueTemp == $("#" + this.id).text())
				actualiza = false;
			else {
				if (this.id.includes("_dto1") || this.id.includes("_dto2") || this.id.includes("_dto3") || this.id.includes("_dto4") || this.id.includes("_dto_pa")) {
					var valor = this.innerText;
				}
				else if (this.id.includes("_plista") || this.id.includes("_rpd_cantidad_compte")) {
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
					td[10].innerText = obj.costo;
					//DC
					if (obj.valorizacion_mostrar_dc) {
						td[22].innerHTML = obj.td_dc;
						td[22].style.padding = "0";
						td[22].style.textAlignLast = "center";
						td[22].style.width = "10px";
					}
					else {
						td[22].innerHTML = "";
					}

					//DP
					if (obj.valorizacion_mostrar_dp) {
						td[23].innerHTML = obj.td_dp;
						td[23].style.padding = "0";
						td[23].style.textAlignLast = "center";
						td[23].style.width = "10px";
					}
					else {
						td[23].innerHTML = "";
					}
				}
			});
			$(".nav-link").prop("disabled", true);
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
					td[19].innerText = obj.costo;
					//DC
					if (obj.valorizacion_mostrar_dc) {
						td[22].innerHTML = obj.td_dc;
						td[22].style.padding = "0";
						td[22].style.textAlignLast = "center";
						td[22].style.width = "10px";
					}
					else {
						td[22].innerHTML = "";
					}

					//DP
					if (obj.valorizacion_mostrar_dp) {
						td[23].innerHTML = obj.td_dp;
						td[23].style.padding = "0";
						td[23].style.textAlignLast = "center";
						td[23].style.width = "10px";
					}
					else {
						td[23].innerHTML = "";
					}
				}
			});
			$(".nav-link").prop("disabled", true);
		}
	});
}

function SetearCostoDesdeOc() {
	var idsProductos = ObtenerIdsProdSeleccionadosEnDetalleRpr();
	if (idsProductos.length == 0) {
		AbrirMensaje("ATENCIÓN", "Debe al menos seleccionar un producto", function () {
			$("#msjModal").modal("hide");
			document.getElementById("tbListaDetalleRpr").focus();
			return true;
		}, false, ["Aceptar"], "error!", null);
	}
	else {
		var oc_compte = $("#txtOC").val();
		AbrirMensaje("ATENCIÓN", "¿Obtener los costos desde la OC original? OC: " + oc_compte, function (e) {
			$("#msjModal").modal("hide");
			switch (e) {
				case "SI": //Confirmar la cancelacion
					AbrirWaiting();
					ActualizarProductosSeleccionadosDesdeOcOriginal(oc_compte, idsProductos);
					CerrarWaiting();
					break;
				case "NO":
					break;
				default: //NO
					break;
			}
			return true;

		}, true, ["Aceptar", "Cancelar"], "question!", null);
	}
}

function ActualizarProductosSeleccionadosDesdeOcOriginal(oc_compte, idsProds) {
	var data = { oc_compte, idsProds };
	PostGenHtml(data, actualizarProductosSeleccionadosDesdeOcOriginalUrl, function (obj) {
		$("#divListaDetalleRpr").html(obj);
		$(".nav-link").prop("disabled", true);
		AddEventListenerToGrid("tbListaDetalleRpr");
		limpiarValoresDeSeteoMasivo();
		addInCellKeyDownHandler();
		addInCellGotFocusHandler();
		addInCellInputGotFocusHandler();
		addInCellLostFocusHandler();
		addMaskInEditableCells();
		tableUpDownArrow();
		AgregarHandlerAGrillaDetalleRprCheckAll();
	});
}

function AplicarSeteoMasivo() {
	var sigue = true;
	var aplica_Precio_oc = $("#chkPrecio_oc")[0].checked;
	var aplica_Precio_fac = $("#chkPrecio_fac")[0].checked;
	var idsProductos = ObtenerIdsProdSeleccionadosEnDetalleRpr();
	if (idsProductos.length == 0) {
		sigue = false;
		AbrirMensaje("ATENCIÓN", "Debe al menos seleccionar un producto", function () {
			$("#msjModal").modal("hide");
			document.getElementById("tbListaDetalleRpr").focus();
			return true;
		}, false, ["Aceptar"], "error!", null);
	}
	else if (optSelected == "") {
		sigue = false;
		AbrirMensaje("ATENCIÓN", "Debe al menos seleccionar una opción de seteo masivo", function () {
			$("#msjModal").modal("hide");
			document.getElementById("tbListaDetalleRpr").focus();
			return true;
		}, false, ["Aceptar"], "error!", null);
	}
	else if (!aplica_Precio_oc && !aplica_Precio_fac) {
		sigue = false;
		AbrirMensaje("ATENCIÓN", "Debe al menos indicar si aplica a precio o factura", function () {
			$("#msjModal").modal("hide");
			document.getElementById("chkPrecio_oc").focus();
			return true;
		}, false, ["Aceptar"], "error!", null);
	}
	else if (optSelected == "opcion1") {//Costos de Otra OC
		if ($("#txtOC").val() == "") {
			sigue = false;
			AbrirMensaje("ATENCIÓN", "Debe indicar el número de OC para validar", function () {
				$("#msjModal").modal("hide");
				document.getElementById("txtOC").focus();
				return true;
			}, false, ["Aceptar"], "error!", null);
		}
	}
	else if (optSelected == "opcion4") {//Costos especificos
		var dto1 = $("#txtDto1").inputmask('unmaskedvalue');
		var dto2 = $("#txtDto2").inputmask('unmaskedvalue');
		var dto3 = $("#txtDto3").inputmask('unmaskedvalue');
		var dto4 = $("#txtDto4").inputmask('unmaskedvalue');
		var dtodpa = $("#txtDpa").inputmask('unmaskedvalue');
		var pLista = $("#txtPLista").inputmask('unmaskedvalue');
		var boni = $("#txtBoni").val();
		if (dto1 == 0 && dto2 == 0 && dto3 == 0 && dto4 == 0 && dtodpa == 0 && pLista == 0 && boni == "") {
			sigue = false;
			AbrirMensaje("ATENCIÓN", "Debe al menos indicar un valor distinto de 0, o indicar un valor válido para bonificación en el caso que se requiera.", function () {
				$("#msjModal").modal("hide");
				document.getElementById("txtPLista").focus();
				return true;
			}, false, ["Aceptar"], "error!", null);
		}
	}
	if (sigue) {

		//Pasaron las validaciones, depende de la opcion seleccionada (radioButton) y de los destinos a aplicar los cambios, es lo que voy a hacer
		switch (optSelected) {
			case "opcion1": //Costos de Otra OC
				ValidarOC(false);
				break;
			case "opcion2":
				ValidarOC(true);
				break;
			case "opcion3": //Costo sistema, envío "actual" como cm_compte
				SetearCostoActual();
				break;
			case "opcion4":
				SetearCostosEspecificos(idsProductos);
				break;
			default:
				break;
		}

	}
}
function SetearCostosEspecificos(idsProductos) {
	var aplica_oc = $("#chkPrecio_oc")[0].checked;
	var aplica_fac = $("#chkPrecio_fac")[0].checked;
	var plista = $("#txtPLista").inputmask('unmaskedvalue');
	var dto1 = $("#txtDto1").inputmask('unmaskedvalue');
	var dto2 = $("#txtDto2").inputmask('unmaskedvalue');
	var dto3 = $("#txtDto3").inputmask('unmaskedvalue');
	var dto4 = $("#txtDto4").inputmask('unmaskedvalue');
	var dtodpa = $("#txtDpa").inputmask('unmaskedvalue');
	var boni = $("#txtBoni").val();
	var plista_bool = $("#chkPLista")[0].checked;
	var dto1_bool = $("#chkDto1")[0].checked;
	var dto2_bool = $("#chkDto2")[0].checked;
	var dto3_bool = $("#chkDto3")[0].checked;
	var dto4_bool = $("#chkDto4")[0].checked;
	var dtoPa_bool = $("#chkDpa")[0].checked;
	var boni_bool = $("#chkBoni")[0].checked;
	AbrirWaiting("Aplicando cambios masivos ...");
	//Armar request
	var data = { plista, dto1, dto2, dto3, dto4, dtodpa, boni, idsProductos, aplica_oc, aplica_fac, plista_bool, dto1_bool, dto2_bool, dto3_bool, dto4_bool, dtoPa_bool, boni_bool };
	PostGenHtml(data, cargarActualizacionPorSeteoMasivoUrl, function (obj) {
		$("#divListaDetalleRpr").html(obj);
		$(".nav-link").prop("disabled", true);
		AddEventListenerToGrid("tbListaDetalleRpr");
		limpiarValoresDeSeteoMasivo();
		addInCellKeyDownHandler();
		addInCellGotFocusHandler();
		addInCellInputGotFocusHandler();
		addInCellLostFocusHandler();
		tableUpDownArrow();
		AgregarHandlerAGrillaDetalleRprCheckAll();
		addMaskInEditableCells();
		inCellInputEditable();
		limpiarValoresDeSeteoMasivo();
		CerrarWaiting();
	});
}

function ValidarOC(esRelacionada) {
	var aplica_oc = $("#chkPrecio_oc")[0].checked;
	var aplica_fac = $("#chkPrecio_fac")[0].checked;
	var idsProductos = ObtenerIdsProdSeleccionadosEnDetalleRpr();
	var cta_id = $("#CtaID").val()
	if (!esRelacionada) {
		var oc_compte = $("#txtOC").val();
		var data = { oc_compte, cta_id };
		PostGen(data, validarOcURL, function (obj) {
			if (obj.error === true) {
				AbrirMensaje("ATENCIÓN", obj.msg, function () {
					$("#msjModal").modal("hide");
					return true;
				}, false, ["Aceptar"], "error!", null);
			}
			else {
				//Si me devuelve ok, actualizo los valores de los productos
				CargarDetalleRprDesdeOcValidada(oc_compte, idsProductos, aplica_oc, aplica_fac);
			}
		});
	}
	else {
		var oc_compte = "relacionada";
		CargarDetalleRprDesdeOcValidada(oc_compte, idsProductos, aplica_oc, aplica_fac);
	}
}

function CargarDetalleRprDesdeOcValidada(ocCompte, idsProds, aplica_oc, aplica_fac) {
	AbrirWaiting("Cargando información desde OC: " + ocCompte);
	var data = {
		oc_compte: ocCompte,
		idsProds: idsProds,
		aplica_oc: aplica_oc,
		aplica_fac: aplica_fac
	};
	PostGenHtml(data, cargarDetalleRprDesdeOcValidadaUrl, function (obj) {
		$("#divListaDetalleRpr").html(obj);
		$(".nav-link").prop("disabled", true);
		AddEventListenerToGrid("tbListaDetalleRpr");
		addInCellKeyDownHandler();
		addInCellGotFocusHandler();
		addInCellInputGotFocusHandler();
		addInCellLostFocusHandler();
		tableUpDownArrow();
		AgregarHandlerAGrillaDetalleRprCheckAll();
		addMaskInEditableCells();
		limpiarValoresDeSeteoMasivo();
		inCellInputEditable();
		CerrarWaiting();
	});
}

function SetearCostoActual() {
	var idsProductos = ObtenerIdsProdSeleccionadosEnDetalleRpr();
	var aplica_oc = $("#chkPrecio_oc")[0].checked;
	var aplica_fac = $("#chkPrecio_fac")[0].checked;
	var ocCompte = "actual";
	AbrirWaiting("Cargando información ...");
	var data = { oc_compte: ocCompte, idsProds: idsProductos, aplica_oc, aplica_fac }
	PostGenHtml(data, cargarDetalleRprDesdeOcValidadaUrl, function (obj) {
		$("#divListaDetalleRpr").html(obj);
		$(".nav-link").prop("disabled", true);
		AddEventListenerToGrid("tbListaDetalleRpr");
		addInCellKeyDownHandler();
		addInCellGotFocusHandler();
		addInCellInputGotFocusHandler();
		addInCellLostFocusHandler();
		tableUpDownArrow();
		AgregarHandlerAGrillaDetalleRprCheckAll();
		addMaskInEditableCells();
		inCellInputEditable();
		limpiarValoresDeSeteoMasivo();
		CerrarWaiting();
	});
}

function limpiarValoresDeSeteoMasivo() {
	$("#txtDto1").val("");
	$("#txtDto2").val("");
	$("#txtDto3").val("");
	$("#txtDto4").val("");
	$("#txtDpa").val("");
	$("#txtBoni").val("");
	$("#txtPLista").val("");
	$("#chkPLista").prop('checked', false);
	$("#chkPLista").trigger("change");
	$("#chkDto1").prop('checked', false);
	$("#chkDto1").trigger("change");
	$("#chkDto2").prop('checked', false);
	$("#chkDto2").trigger("change");
	$("#chkDto3").prop('checked', false);
	$("#chkDto3").trigger("change");
	$("#chkDto4").prop('checked', false);
	$("#chkDto4").trigger("change");
	$("#chkDpa").prop('checked', false);
	$("#chkDpa").trigger("change");
	$("#chkBoni").prop('checked', false);
	$("#chkBoni").trigger("change");
}

function CancelarDesdeDetalleRpr() {
	AbrirMensaje("ATENCIÓN", "¿Va a perder cualquier modificación realizada, desea continuar?", function (e) {
		$("#msjModal").modal("hide");
		switch (e) {
			case "SI": //Confirmar la cancelacion
				AbrirWaiting();
				$(".nav-link").prop("disabled", false);
				//Restaurar valores originales
				CargarDesdeCopiaDeRespaldoListaRpr();
				$("#btnTabComprobantes").trigger("click");
				CerrarWaiting();
				break;
			case "NO":
				break;
			default: //NO
				break;
		}
		return true;

	}, true, ["Aceptar", "Cancelar"], "question!", null);
}

function AceptarDesdeDetalleRpr() {
	if ($(".nav-link").is(':disabled')) {
		AbrirMensaje("ATENCIÓN", "¿Aceptar las modificaciones realizadas y revalorizar? ", function (e) {
			$("#msjModal").modal("hide");
			switch (e) {
				case "SI": //Confirmar la cancelacion
					AbrirWaiting();
					$(".nav-link").prop("disabled", false);
					setTimeout(() => {
						ActualizarListaValorizaciones();
					}, 500);

					CerrarWaiting();
					break;
				case "NO":
					break;
				default: //NO
					break;
			}
			return true;

		}, true, ["Aceptar", "Cancelar"], "question!", null);
	} else {
		// The nav-item is not disabled, validamos si existe error en el calculo de costos
		PostGen(data, VerificarErrorEnCalculoDeCostosUrl, function (obj) {
			CerrarWaiting();
			if (obj.error === true) {
				AbrirMensaje("ATENCIÓN", obj.msg, function () {
					$("#msjModal").modal("hide");
					return true;
				}, false, ["Aceptar"], "error!", null);
			}
		});
	}
}

function GuardarValorizacion() {
	AbrirMensaje("ATENCIÓN", "¿Desea guardar la valorización? ", function (e) {
		$("#msjModal").modal("hide");
		switch (e) {
			case "SI": //Confirmar la cancelacion
				AbrirWaiting("Guardando valorización...");
				var esConfirmacion = false;
				var cmCompte = cmCompteSelected;
				var dif_precio = $("#chkDifPrecio")[0].checked;
				var dif_cantidad = $("#chkDifCantidad")[0].checked;
				var data = { cmCompte, esConfirmacion, dif_precio, dif_cantidad };
				PostGen(data, guardarValorizacionUrl, function (obj) {
					CerrarWaiting();
					if (obj.error === true) {
						AbrirMensaje("ATENCIÓN", obj.msg, function () {
							$("#msjModal").modal("hide");
							return true;
						}, false, ["Aceptar"], "error!", null);
					}
					else {
						ControlaMensajeSuccess("Se ha guardado la valorización de forma exitosa.");
					}
				});
				break;
			case "NO":
				break;
			default: //NO
				break;
		}
		return true;

	}, true, ["Aceptar", "Cancelar"], "question!", null);

}

function ConfirmarValorizacion() {
	AbrirMensaje("ATENCIÓN", "¿Desea confirmar la valorización? ", function (e) {
		$("#msjModal").modal("hide");
		switch (e) {
			case "SI": //Confirmar la cancelacion
				AbrirWaiting();
				var esConfirmacion = true;
				var cmCompte = cmCompteSelected;
				var dif_precio = $("#chkDifPrecio")[0].checked;
				var dif_cantidad = $("#chkDifCantidad")[0].checked;
				var data = { cmCompte, esConfirmacion, dif_precio, dif_cantidad };
				PostGen(data, guardarValorizacionUrl, function (obj) {
					CerrarWaiting();
					if (obj.error === true) {
						AbrirMensaje("ATENCIÓN", obj.msg, function () {
							$("#msjModal").modal("hide");
							return true;
						}, false, ["Aceptar"], "error!", null);
					}
					else {
						ControlaMensajeSuccess("Se ha confirmado la valorización de forma exitosa.");
						CancelarValorizacion();
					}
				});
				break;
			case "NO":
				break;
			default: //NO
				break;
		}
		return true;

	}, true, ["Aceptar", "Cancelar"], "question!", null);

}

function CancelarValorizacion() {
	InicializarDatosEnSesion();
	LimpiarDatosDelFiltroInicial();
	InicializarPantallaDeFiltros();
	$("#btnFiltro").trigger("click");
	$("#btnDetalle").trigger("click");
	$("#divDetalle").collapse("hide");
}

function InicializarDatosEnSesion() {
	PostGen({}, inicializarDatosEnSesion2Url, function (obj) {
		if (obj.error === true) {
			AbrirMensaje("ATENCIÓN", obj.msg, function () {
				$("#msjModal").modal("hide");
				return true;
			}, false, ["Aceptar"], "error!", null);
		}
	});
}

function CargarDesdeCopiaDeRespaldoListaRpr() {
	AbrirWaiting("Cargando información desde copia de respaldo...");
	var data = {};
	PostGenHtml(data, cargarDesdeCopiaDeRespaldoListaRprUrl, function (obj) {
		$("#divListaDetalleRpr").html(obj);
		AddEventListenerToGrid("tbListaDetalleRpr");
		addInCellKeyDownHandler();
		addInCellGotFocusHandler();
		addInCellInputGotFocusHandler();
		addInCellLostFocusHandler();
		tableUpDownArrow();
		AgregarHandlerAGrillaDetalleRprCheckAll();
		addMaskInEditableCells();
		inCellInputEditable();
		CerrarWaiting();
	});
}

function ObtenerIdsProdSeleccionadosEnDetalleRpr() {
	//RPR
	var dataTable = document.getElementById('tbListaDetalleRpr');
	var inputs = dataTable.querySelectorAll('tbody>tr>td>input');
	var pIds = [];
	inputs.forEach(function (input) {
		if (input.checked) {
			alMenosUno = true;
			pIds.push(input.id.substring(0, 6));
		}
	});
	return pIds;
}

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
		.querySelectorAll('[contenteditable=true]')
		.forEach(elm => {
			elm.onfocus = e => {
				let sPos = myTable.querySelector('.selected-row')
					, tdPos = elm.parentNode

				if (sPos) {
					sPos.classList.remove('selected-row');
				}

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
			//||
			//(evt.ctrlKey && movKey[evt.code])
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

					var tabla = document.getElementById("tbListaDetalleRpr");
					var selectedRow = tabla.querySelector('.selected-row');
					if (selectedRow) {
						selectedRow.classList.remove('selected-row');
					}
					nxFocus.closest('tr').classList.add('selected-row');
					nxFocus.focus();
					var obj = nxFocus.childNodes[0];

					obj.select();
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
		else if (evt.ctrlKey && movKey[evt.code])
			event.preventDefault();
	}
}

function addMaskInEditableCells() {
	if ($("#tbListaDetalleRpr tbody tr").length != 0) {
		$("#tbListaDetalleRpr").find('tr').each(function (i, el) {
			var td = $(this).find('td');
			if (td.length == 24) {
				getMaskForMoneyType("#" + td[3].childNodes[0].id); //_plista
				getMaskForDiscountType("#" + td[4].childNodes[0].id);//_dto1
				getMaskForDiscountType("#" + td[5].childNodes[0].id);//_dto2
				getMaskForDiscountType("#" + td[6].childNodes[0].id);//p_dto3
				getMaskForDiscountType("#" + td[7].childNodes[0].id);//p_dto4
				getMaskForDiscountType("#" + td[8].childNodes[0].id);//p_dto_pa
				$("#" + td[9].childNodes[0].id).mask("000/000", { reverse: false });//p_boni

				getMaskForMoneyType("#" + td[12].childNodes[0].id); //_plista
				getMaskForDiscountType("#" + td[13].childNodes[0].id);//_dto1
				getMaskForDiscountType("#" + td[14].childNodes[0].id);//_dto2
				getMaskForDiscountType("#" + td[15].childNodes[0].id);//p_dto3
				getMaskForDiscountType("#" + td[16].childNodes[0].id);//p_dto4
				getMaskForDiscountType("#" + td[17].childNodes[0].id);//p_dto_pa
				$("#" + td[18].childNodes[0].id).mask("000/000", { reverse: false });//p_boni
				getMaskForMoneyType("#" + td[21].childNodes[0].id);//p_dto_pa
			}
		});
	}

	//Seccion cambios masivos
	getMaskForMoneyType("#txtPLista"); //_plista
	getMaskForDiscountType("#txtDto1");//_dto1
	getMaskForDiscountType("#txtDto2");//_dto1
	getMaskForDiscountType("#txtDto3");//_dto1
	getMaskForDiscountType("#txtDto4");//_dto1
	getMaskForDiscountType("#txtDpa");//_dto1
	$("#txtBoni").mask("000/000", { reverse: false });//p_boni
}
