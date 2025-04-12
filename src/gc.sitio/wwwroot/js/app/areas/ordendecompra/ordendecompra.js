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
	//elimina item de la lista
	$("#Rel02List").on("dblclick", 'option', function () { $(this).remove(); })
	$("#Rel03List").on("dblclick", 'option', function () { $(this).remove(); })
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
		ConfirmarOrdenDeCompra();
	});
	$("#btnCancel").on("click", function () {
		LimpiarDatosDelFiltroInicial();
		$("#btnFiltro").trigger("click");
	});
	$("#btnAbmCancelar").on("click", function () {
		InicializarDatosEnSesion();
		InicializaPantalla();
		LimpiarDatosDelFiltroInicial();
		$("#btnFiltro").trigger("click");
		$("#btnDetalle").trigger("click");
		$("#divDetalle").collapse("hide");
	});
	funcCallBack = BuscarProductos;
	InicializaPantalla();
	$("#Rel01").focus();
	$("#pagEstado").on("change", function () {
		var div = $("#divPaginacion");
		presentaPaginacionOC(div);
	});
	$(document).on("change", "#listaOCPend", ControlaListaOcSelected);
	$(document).on("change", "#listaSucursales", ControlaSucursalSeleccionada);
	$(document).on("keypress", ".inputEditable", analizaEnterInput);

	$("#btnCollapseSection").on("click", btnCollapseSectionClicked);
	$("#tabResumen").on("click", function () {
		$("#btnAbmAceptar").prop("disabled", false);
		CargarResumenDeOc();
	});
	$("#tabNuevaOC").on("click", function () {
		$("#btnAbmAceptar").prop("disabled", true);
	});
	$("#tabProductos").on("click", function () {
		$("#btnAbmAceptar").prop("disabled", true);
	});
	return true;
});

// Create our number formatter.
const formatter = new Intl.NumberFormat('en-US', {
	style: 'currency',
	currency: 'USD',
	trailingZeroDisplay: 'stripIfInteger'
});

const EstadoColor = {
	Activo: '#34dc22', //≈ Lima
	NoActivo: '#f74146', //≈ Sunset Orange
	Discontinuo: '#4180f7' //≈ Dodger Blue
}

function ControlaSucursalSeleccionada() {
	BuscarInfoAdicional();
}

function ControlaListaOcSelected() {
	if ($("#listaOCPend").val() != "")
		ocIdSelected = $("#listaOCPend").val();
	else
		ocIdSelected = "";
}

function addHandlerOnChkRel04_Click() {
	$("#chkRel04").on("click", function () {
		if ($("#chkRel04").is(":checked")) {
			$("#listaOCPend").prop("disabled", false);
			$("#listaOCPend").trigger("focus");

		}
		else {
			$("#listaOCPend").prop("disabled", true).val("");
			ocIdSelected = "";
		}
	});
}

function LimpiarDatosDelFiltroInicial() {
	$("input#Rel01").val("");
	$("#Rel01Item").val("");
	$("#Rel01List").empty();

	$("#chkRel04").prop('checked', false);
	$("#chkRel04").trigger("change");
	$("input#Rel04").val("");
	$("#Rel04Item").val("");
	$("input#Rel04").prop('disabled', true);

	$("input#Rel03").val("");
	$("#Rel03Item").val("");
	$("#Rel03List").empty();
	$("#chkRel03").prop('checked', false);
	$("#chkRel03").trigger("change");
	$("input#Rel03").prop('disabled', true);
	$("#Rel03List").prop('disabled', true);
	$("#chkRel03").prop('disabled', true);

	$("input#Rel02").val("");
	$("#Rel02Item").val("");
	$("#Rel02List").empty();
	$("#chkRel02").prop('checked', false);
	$("#chkRel02").trigger("change");
	$("input#Rel02").prop('disabled', true);
	$("#Rel02List").prop('disabled', true);

	$("#chk01").prop('checked', false);
	$("#chk01").trigger("change");
	$("#chk02").prop('checked', false);
	$("#chk02").trigger("change");
	$("#chk03").prop('checked', false);
	$("#chk03").trigger("change");
	$("#chk04").prop('checked', false);
	$("#chk04").trigger("change");
	$("#chk05").prop('checked', false);
	$("#chk05").trigger("change");

	$("#chkDescr").prop('checked', false);
	$("#chkDescr").trigger("change");
	$("input#Buscar").val("");
	$("input#Buscar").prop('disabled', true);

	$("#chkDesdeHasta").prop('checked', false);
	$("#chkDesdeHasta").trigger("change");
	$("input#Id").val("");
	$("input#Id").prop('disabled', true);
	$("input#Id2").val("");
	$("input#Id2").prop('disabled', true);
	$("#listaOCPend").empty();
	$("#divLstOcPendiente").html("");
}

function ConfirmarOrdenDeCompra() {
	AbrirMensaje("ATENCIÓN", "¿Confirma la generación de la Orden de Compra?", function (e) {
		$("#msjModal").modal("hide");
		switch (e) {
			case "SI": //Confirmar 
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
				PostGen(data, "/Compras/ordendecompra/ConfirmarOrdenDeCompra", function (obj) {
					if (obj.error === true) {
						AbrirMensaje("ATENCIÓN", obj.msg, function () {
							$("#msjModal").modal("hide");
							return true;
						}, false, ["Aceptar"], "error!", null);
					}
					else {
						AbrirMensaje("ATENCIÓN", obj.msg, function () {
							$("#msjModal").modal("hide");
							InicializarDatosEnSesion();
							InicializaPantalla();
							LimpiarDatosDelFiltroInicial();
							$("#btnFiltro").trigger("click");
							$("#btnDetalle").trigger("click");
							$("#divDetalle").collapse("hide");
							return true;
						}, false, ["Aceptar"], "info!", null);
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

function InicializarDatosEnSesion() {
	PostGen({}, inicializarDatosEnSesionURL, function (obj) {
		if (obj.error === true) {
			AbrirMensaje("ATENCIÓN", obj.msg, function () {
				$("#msjModal").modal("hide");
				return true;
			}, false, ["Aceptar"], "error!", null);
		}
		else {
			console.log(obj.msg);
		}
	});
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
	var validDate = moment($("#FechaEntrega").val()).isValid();
	if (!validDate) {
		var fecha = moment().format('yyyy-MM-DD');
		$("#FechaEntrega").val(fecha)
		fecha = moment($("#FechaEntrega").val()).add(1, 'day').format('yyyy-MM-DD');
		$("#PagoPlazo").val(fecha);
	}
	if ($("#FechaEntrega").val() > $("#PagoPlazo").val()) {
		var fecha = moment($("#FechaEntrega").val()).add(1, 'day').format('yyyy-MM-DD');
		$("#PagoPlazo").val(fecha);
	}
	ActualizarGrillaConceptos();
}

function onChangePagoPlazo(e) {
	var validDate = moment($("#PagoPlazo").val()).isValid();
	if (!validDate) {
		fecha = moment($("#FechaEntrega").val()).add(1, 'day').format('yyyy-MM-DD');
		$("#PagoPlazo").val(fecha);
	}
	if ($("#FechaEntrega").val() > $("#PagoPlazo").val()) {
		var fecha = moment($("#FechaEntrega").val()).add(1, 'day').format('yyyy-MM-DD');
		$("#PagoPlazo").val(fecha);

	}
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
		var val = $("#" + this.id).text();
		if (this.id.includes("p_dto1") || this.id.includes("p_dto2") || this.id.includes("p_dto3") || this.id.includes("p_dto4") || this.id.includes("p_dto_pa")) {
			//var num = Number(val);
			//if (num > 50) val = "50";
			////$("#" + this.id).mask("00,0", { reverse: true });
			//$("#" + this.id).val(val);
			//$("#" + this.id).text(val);
		}
		else if (this.id.includes("p_plista")) {
			/*$("#" + this.id).mask("000.000.000.000,00", { reverse: true });*/
			//$("#" + this.id).inputmask({
			//	alias: 'numeric',
			//	groupSeparator: '.',
			//	radixPoint: ',',
			//	digits: 2,
			//	digitsOptional: false,
			//	allowMinus: false,
			//	prefix: '',
			//	suffix: '',
			//	rightAlign: true,
			//	unmaskAsNumber: true // Devuelve un número al obtener el valor
			//});
			//$("#" + this.id).val(val);
		}
		else if (this.id.includes("p_boni")) {
			//$("#" + this.id).mask("000/000", { reverse: true });
			//$("#" + this.id).val(val);
		}
	});
}

var keysAceptadas = [8, 37, 39, 46, 96, 97, 98, 99, 100, 101, 102, 103, 104, 105, 110, 190];
function addInCellKeyDownHandler() {
	$("#tbListaProductoOC").on('keydown', 'td[contenteditable]', function (e) {
		if (isNaN(String.fromCharCode(e.which)) && !(keysAceptadas.indexOf(e.which) != -1) && !(e.shiftKey && (e.which == 37 || e.which == 39))) e.preventDefault();
	});
}

function addInCellGotFocusHandler() {
	$("#tbListaProductoOC").on('focusin', 'td[contenteditable]', function (e) {
		cellValueTemp = $("#" + this.id).text();
		if (e.target) {
			cellIndexTemp = e.target.cellIndex;
		}
	});
}

function addInCellLostFocusHandler() {
	$("#tbListaProductoOC").on('focusout', 'td[contenteditable]', function (e) {
		var actualiza = true;
		if (cellValueTemp == $("#" + this.id).text())
			actualiza = false;
		else {
			if (this.id.includes("p_dto1") || this.id.includes("p_dto2") || this.id.includes("p_dto3") || this.id.includes("p_dto4") || this.id.includes("p_dto_pa") || this.id.includes("bulto")) {
				var valor = this.innerText;
			}
			else if (this.id.includes("p_plista")) {
				var valor = $("#" + this.id).inputmask('unmaskedvalue');
			}
			else if (this.id.includes("p_boni")) {
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
			ActualizarProductoEnOc(this.id, valor);
			//if ($("#" + this.id).val() == "")
			//	ActualizarProductoEnOc(this.id, $("#" + this.id).text());
			//else
			//	ActualizarProductoEnOc(this.id, $("#" + this.id).val());
		}
	});
}

function tableUpDownArrow() {
	var table = document.querySelector('#tbListaProductoOC tbody');
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
				r: sPos ? sPos.rowIndex : -1
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

				if (pos.c == 8 && cellIndexTemp < pos.c) //moviendome desde la columna 'pedido bultos' hacia la derecha, la cual no es editable, debo saltar a la siguiente
					pos.c = 9;
				if (pos.c == 6 && cellIndexTemp > pos.c) //moviendome desde la columna 'pedido bultos' hacia la izquierda, la cual no es editable, debo saltar a la siguiente
					pos.c = 15;
				if (pos.c == 8 && cellIndexTemp > pos.c) //moviendome desde la columna 'precio lista' hacia la izquierda, la cual no es editable, debo saltar a la siguiente
					pos.c = 7;
				if (pos.c == 16 && cellIndexTemp < pos.c) //moviendome desde la columna 'boni' hacia la derecha, la cual no es editable, debo saltar a la siguiente
					pos.c = 7;
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
			//ActualizarProductoEnOc(nxFocus.id, sPos.val());
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
					//GRILLA
					td[8].innerText = obj.data.pedidoCantidad;//
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
				const dateControl2 = $('input[type="date"]');
				var now = moment().format('yyyy-MM-DD');
				var min = now;
				var max = moment().add(4, 'months');
				for (var i = 0; i < dateControl2.length; i++) {
					if (dateControl2[i].id == "FechaEntrega") {
						dateControl2[i].setAttribute('min', min);
						dateControl2[i].setAttribute('max', max.format('yyyy-MM-DD'));
					}
				}
			}
		});
	}
}

///Da formato monetario a los campos de tipo "money"
function FormatearValores(grilla, idx) {
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

function keyUpFromEditableCell(x) {
	if (event.key == "Enter") {
		var id = $(x).prop("id");
		if (id.includes("bultos_")) {
			var arr_p_id = id.split("_");
			var p_id = arr_p_id[arr_p_id.length - 1];
			var next = "p_plista_" + p_id;
			$("#" + next).focus();
			return true;
		}
		if (id.includes("p_plista")) {
			var arr_p_id = id.split("_");
			var p_id = arr_p_id[arr_p_id.length - 1];
			var next = "p_dto1_" + p_id;
			$("#" + next).focus();
			return true;
		}
		if (id.includes("p_dto1")) {
			var arr_p_id = id.split("_");
			var p_id = arr_p_id[arr_p_id.length - 1];
			var next = "p_dto2_" + p_id;
			$("#" + next).focus();
			return true;
		}
		if (id.includes("p_dto2")) {
			var arr_p_id = id.split("_");
			var p_id = arr_p_id[arr_p_id.length - 1];
			var next = "p_dto3_" + p_id;
			$("#" + next).focus();
			return true;
		}
		if (id.includes("p_dto3")) {
			var arr_p_id = id.split("_");
			var p_id = arr_p_id[arr_p_id.length - 1];
			var next = "p_dto4_" + p_id;
			$("#" + next).focus();
			return true;
		}
		if (id.includes("p_dto4")) {
			var arr_p_id = id.split("_");
			var p_id = arr_p_id[arr_p_id.length - 1];
			var next = "p_dto_pa_" + p_id;
			$("#" + next).focus();
			return true;
		}
		if (id.includes("p_dto_pa")) {
			var arr_p_id = id.split("_");
			var p_id = arr_p_id[arr_p_id.length - 1];
			var next = "p_boni_" + p_id;
			$("#" + next).focus();
			return true;
		}
		if (id.includes("p_boni")) {
			var arr_p_id = id.split("_");
			var p_id = arr_p_id[arr_p_id.length - 1];
			var next = "bultos_" + p_id;
			$("#" + next).focus();
			return true;
		}
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
				$("#" + id).addClass('btn-success').removeClass('btn-danger');
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
						$("#" + id).addClass('btn-danger').removeClass('btn-success');
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
	if ($(e).hasClass("btn-success")) {
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
	else if ($(e).hasClass("btn-secondary")) {
		AbrirMensaje("ATENCIÓN", "El producto seleccionado esta discontínuo.", function () {
			$("#msjModal").modal("hide");
			return true;
		}, false, ["Aceptar"], "error!", null);
	}
	else if ($(e).hasClass("btn-danger")) {
		AbrirMensaje("ATENCIÓN", "El producto seleccionado ya esta incluído en la OC.", function () {
			$("#msjModal").modal("hide");
			return true;
		}, false, ["Aceptar"], "error!", null);
	}
	else {
		console.log("chan!");
	}
}

function InicializaPantalla() {
	var tb = $("#tbListaProducto tbody tr");
	if (tb.length === 0) {
		$("#divFiltro").collapse("show")
	}

	$("#lbRel01").text("Proveedor");
	$("#lbRel02").text("Rubro");
	$("#lbRel03").text("Familia");
	$("#lbRel04").text("OC Pendiente");
	$("#lbChkDescr").text("Descripción Producto");
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
	$("#btnAbmAceptar").prop("disabled", true);
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
		$("#btnAbmAceptar").show();
		$("#btnAbmCancelar").show();
	}
	else {
		$("#btnAbmAceptar").hide();
		$("#btnAbmCancelar").hide();
	}
}

function addTxtMesesKeyUpHandler() {
	$("#txtMeses").on('keyup', function (e) {
		if (e.keyCode == 13) {
			BuscarInfoAdicional();
		}
	});
}

function addTxtSemanasKeyUpHandler() {
	$("#txtSemanas").on('keyup', function (e) {
		if (e.keyCode == 13) {
			BuscarInfoAdicional();
		}
	});
}

function selectListaProductoRow(x) {
	if (x) {
		pIdSeleccionado = x.cells[0].innerText.trim();
		BuscarInfoAdicional();
	}
	else {
		pIdSeleccionado = "";
	}
}

function NoHayProdSeleccionado() {
	if (pIdSeleccionado == undefined || pIdSeleccionado == "") {
		return true;
	}
	return false;
}

function BuscarInfoAdicional() {
	if (NoHayProdSeleccionado()) {
		AbrirMensaje("Atención", "Debe seleccionar un producto.", function () {
			$("#msjModal").modal("hide");
			return true;
		}, false, ["Aceptar"], "error!", null);
	}
	AbrirWaiting();
	var admId = $("#listaSucursales").val();
	var meses = $("#txtMeses").val();
	var semanas = $("#txtSemanas").val();
	var pId = pIdSeleccionado;
	var datos = { pId, admId, meses };
	PostGenHtml(datos, BuscarInfoProdIExMesesURL, function (obj) {
		$("#divInfoProdIExMeses").html(obj);
		AddEventListenerToGrid("tbInfoProdIExMes");
		CerrarWaiting();
		return true
	});
	datos = { pId, admId, semanas };
	PostGenHtml(datos, BuscarInfoProdIExSemanasURL, function (obj) {
		$("#divInfoProdIExSemanas").html(obj);
		AddEventListenerToGrid("tbInfoProdIExSemana");
		CerrarWaiting();
		return true
	});
	datos = { pId, admId };
	PostGenHtml(datos, BuscarInfoProdStkDepositoURL, function (obj) {
		$("#divInfoProductoStkD").html(obj);
		AddEventListenerToGrid("tbInfoProdStkD");
		CerrarWaiting();
		return true
	});
	PostGenHtml(datos, BuscarInfoProdStkSucursalURL, function (obj) {
		$("#divInfoProductoStkA").html(obj);
		AddEventListenerToGrid("tbInfoProdStkA");
		CerrarWaiting();
		return true
	});
	var tipo = tipoDeOperacion;
	var soloProv = true; //Valor por default
	datos = { pId, tipo, soloProv }
	PostGenHtml(datos, BuscarInfoProdSustitutoURL, function (obj) {
		$("#divInfoProdSustituto").html(obj);
		AddEventListenerToGrid("tbListaProductoSust");
		CerrarWaiting();
		return true
	});
	datos = { pId }
	PostGenHtml(datos, BuscarInfoProdURL, function (obj) {
		$("#divInfoProducto").html(obj);
		AddEventListenerToGrid("tbInfoProducto");
		CerrarWaiting();
		return true
	});
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
			addMaskInEditableCells();
			//$(".inputEditable").on("keypress", analizaEnterInput)
			activarBotones(true);
			tableUpDownArrow();
			CargarResumenDeOc();
		}
	});
}

function analizaEnterInput(e) {
	if (e.which == "13") {
		tope = 99999;
		index = -1;
		//obtengo los inputs dentro del div
		var inputss = $("main :input:not(:disabled)");
		tope = inputss.length;
		//le el id del input en el que he dado enter
		var cual = $(this).prop("id");
		inputss.each(function (i, item) {
			if ($(item).prop("id") === cual) {
				index = i;
				return false;
			}
		});
		if (index > -1 && tope > index + 1) {
			inputss[index + 1].focus();
		}

	}
	return true;
}

function addMaskInEditableCells() {
	if ($("#tbListaProductoOC").length != 0) {
		$("#tbListaProductoOC").find('tr').each(function (i, el) {
			var td = $(this).find('td');
			if (td.length > 0) {
				if (td[9].id.includes("p_plista")) {
					getMaskForMoneyType("#" + td[9].id);
				}
				if (td[10].id.includes("p_dto1")) {
					getMaskForDiscountType("#" + td[10].id);
				}
				if (td[11].id.includes("p_dto2")) {
					getMaskForDiscountType("#" + td[11].id);
				}
				if (td[12].id.includes("p_dto3")) {
					getMaskForDiscountType("#" + td[12].id);
				}
				if (td[13].id.includes("p_dto4")) {
					getMaskForDiscountType("#" + td[13].id);
				}
				if (td[14].id.includes("p_dto_pa")) {
					getMaskForDiscountType("#" + td[14].id);
				}
				if (td[15].id.includes("p_boni")) {
					$("#" + td[15].id).mask("000/000", { reverse: false });
				}
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
		AddEventListenerToGrid("tbListaProducto");
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
		$("#btnAbmCancelar").prop("disabled", false);
		MostrarDatosDeCuenta(true);
		CargarTopesDeOC();
		CargarSucursalesParInfoAdicional();
		LimpiarDatosDelFiltroInicial();
		addTxtMesesKeyUpHandler();
		addTxtSemanasKeyUpHandler()
		CerrarWaiting();
		viendeDesdeBusquedaDeProducto = false;
		return true
	});
}

function CargarSucursalesParInfoAdicional() {
	datos = {}
	PostGenHtml(datos, CargarSucursalesParInfoAdicionalURL, function (obj) {
		$("#divSucursales").html(obj);
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
	PostGen(data, buscarOCDesdeCtaIdSeleccionadoUrl, function (obj) {
		if (obj.error === true) {
			AbrirMensaje("ATENCIÓN", obj.msg, function () {
				$("#msjModal").modal("hide");
				return true;
			}, false, ["Aceptar"], "error!", null);
		}
		else {
			$("#divLstOcPendiente").html(obj);
			$("#lbRel04").text("OC Pendiente");
			addHandlerOnChkRel04_Click();
		}
	});
}

function btnCollapseSectionClicked() {
	if ($("#containerListaProducto").hasClass('table-wrapper-400-full-width')) {
		$("#containerListaProducto").removeClass('table-wrapper-400-full-width');
		$("#containerListaProducto").addClass('table-wrapper-300-full-width');
	} else {
		$("#containerListaProducto").removeClass('table-wrapper-300-full-width');
		$("#containerListaProducto").addClass('table-wrapper-400-full-width');
	}
}