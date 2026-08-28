let productoActualEnLista = null;

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

	$(document).on("click", "#btnAgregarProducto", AbrirModalAgregarProducto); //Abrir modal
	/*$("#estadoFuncion").on("change", verificaEstado); //este control debe ser insertado el mismo o similar para cada modulo.*/

	$(document).on("mouseup", "#tbListaDescFinanc tbody tr", function (e) {

		// Ignorar click derecho (botón 2) y botón central (botón 3)
		if (e.button !== 0) return;

		// Ejecutar solo si realmente se seleccionó la fila
		// (evita disparos por clicks en elementos internos)
		if (!$(e.target).closest("tr").is(this)) return;

		setTimeout(() => {
			RecalcularItemValue();
		}, 500);
	});


	InicializarPantallaDeFiltros();
});

function AgregarProducto() {
	//p_cantidad
	var cantidad = $("#p_cantidad").inputmask('unmaskedvalue');
	if (cantidad <= 0) {
		AbrirMensaje("ATENCIÓN", "La cantidad debe ser mayor a 0.", function () {
			$("#msjModal").modal("hide");
			$("#p_cantidad").trigger("focus");
			return true;
		}, false, ["Aceptar"], "error!", null);
	}
	else if ($("#chkIncluyeRp")[0].checked && $("#listaRP").val()=="") {
		AbrirMensaje("ATENCIÓN", "Debe seleccionar un valor para RP.", function () {
			$("#msjModal").modal("hide");
			$("#listaRP").trigger("focus");
			return true;
		}, false, ["Aceptar"], "error!", null);
	}
	else {
		AbrirWaiting("Agregando producto para valorizar...");
		var cta_id = $("#CtaID").val();
		var tco_id = $("#tco_id").val();
		var cm_compte = $("#cm_compte").val();
		var dia_movi = $("#dia_movi").val();
		var incluye_rp = $("#chkIncluyeRp")[0].checked;
		var p_id = $("#p_id").val();
		var cantidad = $("#p_cantidad").inputmask('unmaskedvalue');
		var rp_compte = $("#listaRP").val();
		var data = { cta_id, tco_id, cm_compte, dia_movi, rp_compte, p_id, cantidad, incluye_rp };
		PostGen(data, agregarProductoParaValorizarUrl, function (obj) {
			CerrarWaiting();
			if (obj.error === true) {
				AbrirMensaje("ATENCIÓN", obj.msg, function () {
					$("#msjModal").modal("hide");
					return true;
				}, false, ["Aceptar"], "error!", null);
			}
			else {
				$("#modalAgregarProducto").modal("hide");
				CargarDatosParaValorizar($("#cm_compte").val());
			}
		});
		
	}
}

function verificaTeclaDeBusqueda(e) {
	if (e.which == "13") {

		$("#btnBusquedaBase").trigger("click");
		$("#btnBusquedaBase").prop("disabled", true);
		return true;

	}
}

function AbrirModalAgregarProducto() {
	var cm_compte = $("#cm_compte").val();
	var cta_id = $("#CtaID").val();
	var dia_movi = $("#dia_movi").val();
	var tco_id = $("#tco_id").val();
	AbrirWaiting();
	var datos = { cm_compte, cta_id, dia_movi, tco_id };
	PostGenHtml(datos, obtenerDatosModalAgregarProductoUrl, function (obj) {
		$("#divAgregarProducto").html(obj);
		$('#modalAgregarProducto').modal({
			backdrop: 'static',
		});
		$('#modalAgregarProducto').modal('show');

		$("#btnBusquedaBase").on("click", function () {
			buscarProducto();
			return true;
		});
		$("#estadoFuncion").on("change", verificaEstado); //este control debe ser insertado el mismo o similar para cada modulo.
		
		$(document).on("keyup", "#Busqueda", verificaTeclaDeBusqueda);

		CerrarWaiting();
		return true
	});
}

function LimpiarCamposEnModalDeAgregarProductoParaValorizar() {
	$("#p_id").val("");
	$("#p_desc").val("");
	$("#p_cantidad").val("");
	$("#chkIncluyeRp").prop('checked', false);
	$("#chkIncluyeRp").trigger("change");
	$("#listaRP").val("");
}

function buscarProducto() {
	AbrirWaiting();
	var _post = busquedaProdBaseUrl;
	var valor = $("#Busqueda").val();
	var validarEstado = true;

	var datos = {};
	if (typeof validarEstado !== 'undefined') {
		datos = { busqueda: valor, validarEstado };
	}
	else {
		datos = { busqueda: valor };
	}

	PostGen(datos, _post, function (obj) {
		if (obj.error === true) {
			CerrarWaiting();
			AbrirMensaje("ATENCIÓN", obj.msg, function () {
				productoBase = null;
				$("#estadoFuncion").val(false);
				$("#btnBusquedaBase").prop("disabled", false);
				$("#msjModal").modal("hide");
				LimpiarCamposEnModalDeAgregarProductoParaValorizar();
				$("#Busqueda").trigger("focus");
				return true;
			}, false, ["Aceptar"], "error!", null);
		}
		else if (obj.warn === true) {
			CerrarWaiting();
			if (obj.producto.p_id === "0000-0000") {
				AbrirMensaje("ATENCIÓN", obj.msg, function () {
					productoBase = null;
					$("#estadoFuncion").val(false);
					$("#btnBusquedaBase").prop("disabled", false);
					$("#msjModal").modal("hide");
					LimpiarCamposEnModalDeAgregarProductoParaValorizar();
					$("#Busqueda").trigger("focus");
					return true;
				}, false, ["Aceptar"], "error!", null);
			}
			else if (obj.producto.p_id === "NO") {
				if (funcionBusquedaAvanzada === true) {
					AbrirMensaje("ATENCIÓN", "NO SE ENCONTRO EL PRODUCTO QUE INTENTO BUSCAR. SE ABRIRÁ LA BUSQUEDA AVANZADA.", function () {
						$("#msjModal").modal("hide");
						productoBase = null;
						$("#estadoFuncion").val(false);
						inicializaBusquedaAvanzada();
						$("#busquedaModal").modal("toggle");
						return true;
					}, false, ["Aceptar"], "error!", null);

					return true;
				}
				else {
					AbrirMensaje("ATENCIÓN", "NO SE ENCONTRO EL PRODUCTO QUE INTENTO BUSCAR.", function () {
						$("#msjModal").modal("hide");
						LimpiarCamposEnModalDeAgregarProductoParaValorizar();
						$("#Busqueda").trigger("focus");
						return true;
					}, false, ["Aceptar"], "error!", null);

				}
			} else {
				//encontro producto pero hay warning
				AbrirMensaje("ATENCIÓN!", obj.msg, function (resp) {
					if (resp === "SI") {
						productoBase = obj.producto;
						$("#estadoFuncion").val(true);
						$("#estadoFuncion").trigger("change");
						$("#msjModal").modal("hide");
						$("#Busqueda").trigger("focus");
						return true;
					}
					else {
						//se deniega
						productoBase = null;
						$("#estadoFuncion").val(false);
						$("#btnBusquedaBase").prop("disabled", false);
						$("#msjModal").modal("hide");
						$("#Busqueda").trigger("focus");
						return true;
					}
				},
					true, ["Aceptar", "Denegar"], "Warning!", null);
			}
		}
		else {
			//encontro y se presenta
			productoBase = obj.producto;
			$("#estadoFuncion").val(true);
			$("#estadoFuncion").trigger("change");
			return true;
		}
	});
	return true;
}

function verificaEstado(e) {
	FunctionCallback = null; //inicializo funcion por si tiene alguna funcionalidad asignada.
	var res = $("#estadoFuncion").val();
	CerrarWaiting();
	if (res === "true") {
		//traigo la variable productoBase e hidrato componentes
		var prod = productoBase;
		console.log(prod);
		$("#p_id").val(prod.p_id);
		$("#p_desc").val(prod.p_desc);
		$("#estadoFuncion").val(false);
		if (prod.up_id == "07") {
			getMaskForMoneyType("#p_cantidad", 0);
		}
		else {
			getMaskForMoneyType("#p_cantidad", 3);
		}
		$("#p_cantidad").val("");
		$("#Busqueda").val("");
		$("#p_cantidad").trigger("focus");
	}
	return true;
}

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

	$("#tbListaDescFinanc tbody tr").each(function () {

		var $tr = $(this);
		var td = $tr.find("td");

		var cm_compte = $tr.data("cm_compte");
		var dia_movi = $tr.data("dia_movi");
		var tco_id = $tr.data("tco_id");
		var dtoc_id = $tr.data("dtoc_id");
		var dto_obs = $tr.data("dto_obs");

		var dto_fijo = $tr.find(".chkNetoFijo").is(":checked");
		var dto_sobre_total = $tr.find(".chkDtoTot").is(":checked");

		var dtoc_desc = td.eq(3).text().trim();
		var dto = Number(td.eq(4).text().replace(",", "."));
		var dto_importe = Number(td.eq(5).text().replace(",", "."));
		var item = td.eq(0).text().trim();

		listaDesFinanc.push({
			cm_compte,
			dia_movi,
			dto_fijo,
			dto_sobre_total,
			tco_id,
			dto,
			dto_importe,
			dtoc_id,
			dtoc_desc,
			item,
			dto_obs
		});
	});

	if (listaDesFinanc.length > 0) {
		AbrirWaiting();
		PostGen({ listaDesFinanc }, actualizarOrdenDescFinancURL, function (obj) {
			if (obj.error) {
				AbrirMensaje("ATENCIÓN", obj.msg, function () {
					$("#msjModal").modal("hide");
				}, false, ["Aceptar"], "error!", null);
			} else {
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
		colorearFilasValorizacion();
		$("#btnTabComprobantes").trigger("click");
		CerrarWaiting("");
	});
}

function colorearFilasValorizacion() {
	const filas = document.querySelectorAll("#tbListaValorizacion tbody tr");

	filas.forEach(fila => {
		const tipo = fila.getAttribute("data-tco-id");

		fila.classList.remove("fila-0DC", "fila-0DP", "fila-0DT");

		if (tipo === "0DC") fila.classList.add("fila-0DC");
		if (tipo === "0DP") fila.classList.add("fila-0DP");
		if (tipo === "0DT") fila.classList.add("fila-0DT");
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

function CargarDatosParaValorizar(cmCompteSelected) {
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
			colorearFilasValorizacion();
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

function ControlaListaCompteSelected() {
	if ($("#listaComptesPend").val() != "")
		cmCompteSelected = $("#listaComptesPend").val();
	else
		cmCompteSelected = "";
	if (cmCompteSelected != "") {
		CargarDatosParaValorizar(cmCompteSelected);
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
		finalizarInicializacion();
		SetMaskSeteoMasivoComponentes();
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
	getMaskForMoneyType("#DescFinanc_dto_importe", 2);
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
					return {
						label: texto,
						value: item.descripcion,
						id: item.id,
						prov: item.provId
					};
				}));
			}
		})
	},
	minLength: 3,

	focus: function (event, ui) {
		// evita que el # aparezca mientras navegas con flechas
		const partes = ui.item.value.split("#");
		$("#Rel01").val(partes.join(" "));
		return false;
	},

	select: function (event, ui) {
		const partes = ui.item.value.split("#");
		const textoSinSeparador = partes.join(" ");

		// Mostrar SIN el "#"
		$("#Rel01").val(textoSinSeparador);

		ctaIdSelected = ui.item.id;
		ctaDescSelected = partes[0];
		$("#Rel01List").empty();
		$("#Rel01Item").val(ui.item.id);
		var opc = "<option value=" + ui.item.id + ">" + textoSinSeparador + "</option>"
		$("#Rel01List").append(opc);
		$("#chkRel04").prop("disabled", false);
		CargarComprobantesDelProveedorSeleccionado(ui.item.id);

		event.preventDefault();
		return true;
	}
}).autocomplete("instance")._renderItem = function (ul, item) {

	const partes = item.label.split("#");

	const ctaLista = partes[0];
	const tipoDesc = partes[1];

	return $("<li>")
		.append(
			`<div>
                <span style="font-weight:bold; font-size:14px;">
                    ${ctaLista}
                </span>
                <span style="font-size:13px; color:#555;">
                    ${tipoDesc}
                </span>
            </div>`
		)
		.appendTo(ul);
};

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

function getMaskForMoneyType(selector,decimales) {
	$(selector).inputmask({
		alias: 'numeric',
		groupSeparator: '.',
		radixPoint: ',',
		digits: decimales,
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

function onChangeChkNcGenera(x) {
	if (x) {
		$(".nav-link").prop("disabled", true);
	}
	else
		event.preventDefault;
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
		finalizarInicializacion();
		SetMaskSeteoMasivoComponentes();
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
		finalizarInicializacion();
		SetMaskSeteoMasivoComponentes();
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
		finalizarInicializacion();
		SetMaskSeteoMasivoComponentes();
		limpiarValoresDeSeteoMasivo();
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
		finalizarInicializacion();
		SetMaskSeteoMasivoComponentes();
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

function esValido(valor) {
	return valor !== null &&
		valor !== undefined &&
		valor !== "" &&
		!(typeof valor === "string" && valor.trim() === "");
}

function dblClickListaValorizacion(row) {
	const $row = $(row);

	const tcoId = $row.data("tco-id");
	const cmCompte = $row.data("cm-compte");
	const diaMovi = $row.data("dia-movi");

	// Validación estricta
	if (!esValido(tcoId) || !esValido(cmCompte) || !esValido(diaMovi)) {
		console.error("Error: datos inválidos en la fila seleccionada.", {
			tcoId, cmCompte, diaMovi
		});
		return;
	}

	// Si todo está OK, procesamos
	procesarDobleClickValorizacion(tcoId, cmCompte, diaMovi);
}

function procesarDobleClickValorizacion(tcoId, cmCompte, diaMovi) {
	const el = document.getElementById("divComponenteDetalleComprobante");

	$(document).trigger("comprobanteSeleccionadoParaVisualizar", {
		tco_id: tcoId,
		cm_compte: cmCompte,
		dia_movi: diaMovi
	});

	//if (!el || el.style.display === "none") {
	//	return;
	//}
	//else {
	//	/* ######	INICIO Componente de info detalle de comprobante ###### */
	//	// disparar evento custom con datos del compte
	//	$(document).trigger("comprobanteSeleccionadoParaVisualizar", {
	//		tco_id: tcoId,
	//		cm_compte: cmCompte,
	//		dia_movi: diaMovi
	//	});
	//	/* ######	FIN Componente de info adicional de producto ###### */
	//}
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
		finalizarInicializacion();
		SetMaskSeteoMasivoComponentes();
		CerrarWaiting();
	});
}

function ObtenerIdsProdSeleccionadosEnDetalleRpr() {
	//RPR
	var pIds = [];
	
	$('#tbListaDetalleRpr tbody tr').each(function () {
		const $checkbox = $(this).find('.check-producto');
		if ($checkbox.length && $checkbox.is(':checked')) {
			//alMenosUno = true;
			// Esta fila tiene el checkbox marcado
			const pId = $checkbox.data('p-id');
			pIds.push(pId);
		}
	});

	return pIds;
}

//function tableUpDownArrow() {
//	var table = document.querySelector('#tbListaDetalleRpr tbody');
//	if (table == undefined)
//		return;
//	if (table.rows[0] == undefined)
//		return;
//	const myTable = table
//		, nbRows = myTable.rows.length
//		, nbCells = myTable.rows[0].cells.length
//		, movKey = {
//			ArrowUp: p => { p.r = (--p.r + nbRows) % nbRows }
//			, ArrowLeft: p => { p.c = (--p.c + nbCells) % nbCells }
//			, ArrowDown: p => {
//				p.r = ++p.r % nbRows
//			}
//			, ArrowRight: p => { p.c = ++p.c % nbCells }
//			, Tab: p => {
//				p.r = ++p.r % nbRows
//			}
//		}

//	myTable
//		.querySelectorAll('[contenteditable=true]')
//		.forEach(elm => {
//			elm.onfocus = e => {
//				let sPos = myTable.querySelector('.selected-row')
//					, tdPos = elm.parentNode

//				if (sPos) {
//					sPos.classList.remove('selected-row');
//				}

//				tdPos.classList.add('selected-row')
//			}
//		})


//	document.onkeydown = e => {
//		let sPos = myTable.querySelector('.selected-row')
//			, evt = (e == null ? event : e)
//			, pos = {
//				r: sPos ? sPos.rowIndex - 1 : -1 //sPos.rowIndex -1 => porque tiene doble fila en la cabecera
//				, c: sPos ? (sPos.cellIndex ? sPos.cellIndex : cellIndexTemp) : -1
//			}

//		if (sPos &&
//			//(evt.altKey && evt.shiftKey && movKey[evt.code])
//			(evt.shiftKey && movKey[evt.code])
//			//||
//			//(evt.ctrlKey && movKey[evt.code])
//		) {
//			let loop = true
//				, nxFocus = null
//				, cell = null

//			do {
//				if (evt.code === 'ArrowDown' && pos.r == nbRows)
//					pos.r = 0;
//				if (evt.code === 'Tab' && evt.shiftKey && pos.r == 0)
//					pos.r = nbRows - 1;
//				if (evt.code === 'Tab' && evt.shiftKey) {
//					movKey['ArrowUp'](pos)
//				}
//				else
//					movKey[evt.code](pos);

//				if (pos.r == nbRows)
//					cell = myTable.rows[pos.r - 1].cells[pos.c];
//				else
//					cell = myTable.rows[pos.r].cells[pos.c];
//				if (pos.r == 0)
//					pos.r = nbRows;
//				else if (pos.r == nbRows)
//					pos.r = nbRows;

//				if (pos.c == 10 && cellIndexTemp < pos.c) //moviendome desde la columna 'ocd_boni' hacia la derecha, la cual no es editable, debo saltar a la siguiente editable 'rpd_plista'
//					pos.c = 12;
//				if (pos.c == 19 && cellIndexTemp < pos.c) //moviendome desde la columna 'rpd_boni' hacia la derecha, la cual no es editable, debo saltar a la siguiente editable 'rpd_cantidad_compte'
//					pos.c = 20;

//				if (pos.c == 19 && cellIndexTemp > pos.c) //moviendome desde la columna 'rpd_cantidad_compte' hacia la izquierda, la cual no es editable, debo saltar a la siguiente editable 'rpd_boni'
//					pos.c = 18;
//				if (pos.c == 11 && cellIndexTemp > pos.c) //moviendome desde la columna 'rpd_plista' hacia la izquierda, la cual no es editable, debo saltar a la siguiente editable 'ocd_boni'
//					pos.c = 9;

//				nxFocus = myTable.rows[pos.r - 1].cells[pos.c]

//				if (nxFocus
//					&& cell.style.display !== 'none'
//					&& cell.parentNode.style.display !== 'none') {
//					nxFocus.focus();

//					var tabla = document.getElementById("tbListaDetalleRpr");
//					var selectedRow = tabla.querySelector('.selected-row');
//					if (selectedRow) {
//						selectedRow.classList.remove('selected-row');
//					}
//					nxFocus.closest('tr').classList.add('selected-row');
//					nxFocus.focus();
//					var obj = nxFocus.childNodes[0];

//					obj.select();
//					loop = false
//				}
//			}
//			while (loop)
//			if (evt.code === 'Tab') {
//				event.preventDefault();
//			}
//		}
//		else if (evt.code === 'Enter')
//			event.preventDefault();
//		else if (evt.code === 'NumpadEnter')
//			event.preventDefault();
//		else if (evt.ctrlKey && movKey[evt.code])
//			event.preventDefault();
//	}
//}

//function addMaskInEditableCells() {
//	if ($("#tbListaDetalleRpr tbody tr").length != 0) {
//		$("#tbListaDetalleRpr").find('tr').each(function (i, el) {
//			var td = $(this).find('td');
//			if (td.length == 24) {
//				getMaskForMoneyType("#" + td[3].childNodes[0].id, 2); //_plista
//				getMaskForDiscountType("#" + td[4].childNodes[0].id);//_dto1
//				getMaskForDiscountType("#" + td[5].childNodes[0].id);//_dto2
//				getMaskForDiscountType("#" + td[6].childNodes[0].id);//p_dto3
//				getMaskForDiscountType("#" + td[7].childNodes[0].id);//p_dto4
//				getMaskForDiscountType("#" + td[8].childNodes[0].id);//p_dto_pa
//				$("#" + td[9].childNodes[0].id).mask("000/000", { reverse: false });//p_boni

//				getMaskForMoneyType("#" + td[12].childNodes[0].id, 2); //_plista
//				getMaskForDiscountType("#" + td[13].childNodes[0].id);//_dto1
//				getMaskForDiscountType("#" + td[14].childNodes[0].id);//_dto2
//				getMaskForDiscountType("#" + td[15].childNodes[0].id);//p_dto3
//				getMaskForDiscountType("#" + td[16].childNodes[0].id);//p_dto4
//				getMaskForDiscountType("#" + td[17].childNodes[0].id);//p_dto_pa
//				$("#" + td[18].childNodes[0].id).mask("000/000", { reverse: false });//p_boni
//				getMaskForMoneyType("#" + td[21].childNodes[0].id, 2);//p_dto_pa
//			}
//		});
//	}

//	//Seccion cambios masivos
//	SetMaskSeteoMasivoComponentes();
//}

function SetMaskSeteoMasivoComponentes() {
	//Seccion cambios masivos
	getMaskForMoneyType("#txtPLista", 2); //_plista
	getMaskForDiscountType("#txtDto1");//_dto1
	getMaskForDiscountType("#txtDto2");//_dto1
	getMaskForDiscountType("#txtDto3");//_dto1
	getMaskForDiscountType("#txtDto4");//_dto1
	getMaskForDiscountType("#txtDpa");//_dto1
	$("#txtBoni").mask("000/000", { reverse: false });//p_boni
}

/****************************************************************************************
################################ ADD-ON --  tbListaDetalleRpr  ##########################
*****************************************************************************************/

// Función de utilidad para destacar la fila seleccionada
// ✅ MEJORADA: Función destacar fila con verificación adicional
function destacarFilaSeleccionada(productoId) {
	console.log(`🎯 Destacando fila para producto ID: ${productoId}`);

	// Remover el destacado de todas las filas
	$("#tbListaDetalleRpr tbody tr").removeClass("selected");

	// Verificar que existe una fila con ese ID
	const $fila = $("#tbListaDetalleRpr tbody tr[data-p-id='" + productoId + "']");

	if ($fila.length === 0) {
		console.warn(`⚠️ No se encontró ninguna fila con data-p-id="${productoId}"`);
		return false;
	}

	// Añadir el destacado solo a la fila del producto seleccionado
	$fila.addClass("selected");
	console.log(`✅ Fila destacada correctamente para producto ${productoId}`);

	// Hacer scroll a la fila si está fuera de vista
	scrollAFilaSeleccionada($fila);

	return true;
}

// ✅ NUEVA: Función separada para scroll optimizado
function scrollAFilaSeleccionada($fila) {
	const $tableContainer = $("#tbListaDetalleRpr").closest('.table-responsive');

	if ($tableContainer.length > 0) {
		const containerTop = $tableContainer.offset().top;
		const containerHeight = $tableContainer.height();
		const rowTop = $fila.offset().top;

		// Solo hacer scroll si la fila está fuera del área visible
		if (rowTop < containerTop || rowTop > containerTop + containerHeight) {
			$tableContainer.animate({
				scrollTop: $tableContainer.scrollTop() + (rowTop - containerTop - containerHeight / 2)
			}, 300);
			console.log(`📜 Realizando scroll a la fila seleccionada`);
		}
	}
}

function finalizarInicializacion() {
	setTimeout(function () {
		configuracionInputMaskOptimizada();
		optimizarVisualizacionTabla();
	}, 10);
}

function optimizarVisualizacionTabla() {
	// Asegurarnos de que la tabla existe
	if ($("#tbListaDetalleRpr").length === 0) {
		return;
	}

	// Ajustar columnas con texto para que no sean demasiado anchas
	$("#tbListaDetalleRpr th:nth-child(2)").css('max-width', '180px'); // Descripción
	$("#tbListaDetalleRpr td:nth-child(2)").css({
		'max-width': '180px',
		'white-space': 'nowrap',
		'overflow': 'hidden',
		'text-overflow': 'ellipsis'
	});

	// Asegurarnos que la tabla tenga scroll horizontal si es necesario
	$("#tbListaDetalleRpr").closest('.table-responsive').css('overflow-x', 'auto');

	console.log("Tabla optimizada para mejor visualización");
}

function configuracionInputMaskOptimizada() {
	console.log("Aplicando configuración InputMask optimizada...");

	// Establecer todos los campos como readonly de una sola vez
	$('.input-ocd_plista, .input-ocd_dto1, .input-ocd_dto2, .input-ocd_dto3, .input-ocd_dto4, .input-ocd_dto_pa, .input-ocd_boni, .input-rpd_plista, .input-rpd_dto1, .input-rpd_dto2, .input-rpd_dto3, .input-rpd_dto4, .input-rpd_dto_pa, .input-rpd_boni, .input-rpd_cantidad_compte')
		.prop('readonly', true)
		.addClass('campo-readonly');

	// Definir configuraciones de máscara fuera de los bucles
	const maskConfig3Decimales = {
		alias: "numeric",
		groupSeparator: ",",
		radixPoint: ".",
		autoGroup: true,
		digits: 3,
		digitsOptional: false,
		rightAlign: true,
		prefix: '',
		placeholder: "0",
		clearMaskOnLostFocus: false,
		showMaskOnHover: false,
		showMaskOnFocus: false,
		min: 0, // Explícitamente permitir 0 como valor mínimo
		allowMinus: false, // No permitir valores negativos
		onBeforeMask: function (value) {
			// Si es null, undefined o cadena vacía, retornar '0'
			if (value === null || value === undefined || value === '') {
				return '0';
			}

			// Para otros valores, formatear correctamente
			try {
				let numValue = parseFloat(value.toString().replace(/,/g, ''));
				return isNaN(numValue) ? '0' : numValue.toFixed(3);
			} catch (e) {
				console.error('Error al formatear valor:', e);
				return '0';
			}
		}
	};

	const maskConfig1Decimal = {
		alias: "numeric",
		groupSeparator: ",",
		radixPoint: ".",
		autoGroup: true,
		digits: 1,
		digitsOptional: false,
		rightAlign: true,
		integerDigits: 2,
		min: 0,
		max: 99.9,
		prefix: '',
		placeholder: "0",
		clearMaskOnLostFocus: false,
		showMaskOnHover: false,
		showMaskOnFocus: false,
		onBeforeMask: function (value) {
			if (value) {
				let numValue = parseFloat(value.toString().replace(/,/g, ''));
				if (numValue > 99.9) numValue = 99.9;
				return isNaN(numValue) ? value : numValue.toFixed(1);
			}
			return value;
		}
	};

	const maskConfigEntero = {
		alias: "numeric",
		groupSeparator: ",",
		radixPoint: ".", // no se usa en enteros, pero puede quedar por consistencia
		autoGroup: true,
		digits: 0,
		digitsOptional: false,
		rightAlign: true,
		integerDigits: 2,
		min: 0,
		max: 99999,
		prefix: '',
		placeholder: "0",
		clearMaskOnLostFocus: false,
		showMaskOnHover: false,
		showMaskOnFocus: false,
		onBeforeMask: function (value) {
			if (value) {
				let numValue = parseInt(value.toString().replace(/,/g, ''));
				if (numValue > 99999) numValue = 99999;
				return isNaN(numValue) ? value : numValue.toString();
			}
			return value;
		}
	};


	const maskConfig2Decimales = {
		alias: "numeric",
		groupSeparator: ",",
		radixPoint: ".",
		autoGroup: true,
		digits: 2,
		digitsOptional: false,
		rightAlign: true,
		prefix: '',
		placeholder: "0",
		clearMaskOnLostFocus: false,
		showMaskOnHover: false,
		showMaskOnFocus: false,
		onBeforeMask: function (value) {
			if (value) {
				let numValue = parseFloat(value.toString().replace(/,/g, ''));
				return isNaN(numValue) ? value : numValue.toFixed(2);
			}
			return value;
		}
	};

	const maskConfigBoni = {
		mask: "999/999",
		placeholder: "",
		showMaskOnHover: false,
		showMaskOnFocus: false
	};

	// Aplicar máscaras de forma eficiente con selección optimizada
	Inputmask(maskConfig3Decimales).mask('.input-ocd_plista, .input-rpd_plista');
	Inputmask(maskConfig1Decimal).mask('.input-ocd_dto1, .input-ocd_dto2, .input-ocd_dto3, .input-ocd_dto4, .input-ocd_dto_pa');
	Inputmask(maskConfig1Decimal).mask('.input-rpd_dto1, .input-rpd_dto2, .input-rpd_dto3, .input-rpd_dto4, .input-rpd_dto_pa');
	Inputmask(maskConfigBoni).mask('.input-ocd_boni, .input-rpd_boni');
	Inputmask(maskConfigEntero).mask('.input-rpd_cantidad_compte');

	// Configurar eventos de edición
	configurarEventosEdicionOptimizado();

	console.log("Configuración InputMask aplicada");
}

function configurarEventosEdicionOptimizado() {
	const camposEditables = '.input-ocd_plista, .input-ocd_dto1, .input-ocd_dto2, .input-ocd_dto3, .input-ocd_dto4, .input-ocd_dto_pa, .input-ocd_boni, .input-rpd_plista, .input-rpd_dto1, .input-rpd_dto2, .input-rpd_dto3, .input-rpd_dto4, .input-rpd_dto_pa, .input-rpd_boni, .input-rpd_cantidad_compte';
	const camposSecuencia01 = '.input-ocd_plista, .input-ocd_dto1, .input-ocd_dto2, .input-ocd_dto3, .input-ocd_dto4, .input-ocd_dto_pa, .input-ocd_boni';
	const camposSecuencia02 = '.input-rpd_plista, .input-rpd_dto1, .input-rpd_dto2, .input-rpd_dto3, .input-rpd_dto4, .input-rpd_dto_pa, .input-rpd_boni, .input-rpd_cantidad_compte';

	// Limpiar eventos previos
	$(document).off('click.camposEditables keydown.camposEditables blur.camposSecuencia01 blur.camposSecuencia02');

	// Evento click unificado
	$(document).on('click.camposEditables', camposEditables, function (e) {
		e.stopPropagation();

		const $this = $(this);
		const pIdDetalle = $this.closest('tr').data('p-id');

		// Cambio de producto si es necesario
		if (pIdDetalle !== productoActualEnLista) {
			productoActualEnLista = pIdDetalle;
			//$("#divProdLista").attr('data-producto-actual', pIdDetalle);
			destacarFilaSeleccionada(pIdDetalle);
			//buscarProductoListaOptimizado(pIdDetalle);
		}

		// Habilitar campo
		$this.prop('readonly', false).removeClass('campo-readonly');
		setTimeout(() => { $this[0].focus(); $this[0].select(); }, 0);
	});

	// Evento keydown unificado
	$(document).on('keydown.camposEditables', camposEditables, function (e) {
		if (e.key === 'Enter' || e.key === 'Tab') {
			e.preventDefault();

			const row = $(this).closest('tr');
			const esSecuencia01 = $(this).is(camposSecuencia01);
			const esSecuencia02 = $(this).is(camposSecuencia02);
			//const esPrecioVenta = $(this).hasClass('input-tp_pvta');

			var fueModificado = marcarCampoModificado(this);
			//actualizarEstadoCarga(row);
			activarSiguienteCampo(this);

			// Aplicar cálculos según tipo
			if (esSecuencia01 && fueModificado) ActualizarProductoEnDetalleRprSeccionPrecioDebounced(row, this);
			else if (esSecuencia02 && fueModificado) ActualizarProductoEnDetalleRprSeccionFacturaDebounced(row, this);
			//else if (esPrecioVenta) calcularPrecioVentaMargenAPIDebounced(row);
		}
	});

	// Eventos blur simplificados con delegación
	const eventosBlur = {
		[camposSecuencia01]: () => ActualizarProductoEnDetalleRprSeccionPrecioDebounced,
		[camposSecuencia02]: () => ActualizarProductoEnDetalleRprSeccionFacturaDebounced
	};

	Object.entries(eventosBlur).forEach(([selector, getCallback]) => {
		$(document).on(`blur.${selector.replace(/[^a-zA-Z]/g, '')}`, selector, function () {
			if ($(this).prop('readonly')) return;

			const row = $(this).closest('tr');
			const value = $(this).val().replace(/,/g, '');
			const numValue = parseFloat(value);

			if (!isNaN(numValue)) {
				const decimals = $(this).hasClass('input-tp_plista') || $(this).hasClass('input-tp_pcosto') || $(this).hasClass('input-tp_pneto') ? 3 :
					$(this).hasClass('input-tp_dto1') || $(this).hasClass('input-tp_dto2') || $(this).hasClass('input-tp_dto3') || $(this).hasClass('input-tp_dto4') || $(this).hasClass('input-tp_dto_pa') || $(this).hasClass('input-tp_porc_flete') ? 1 : 2;
				$(this).val(numValue.toFixed(decimals));
			}

			$(this).prop('readonly', true).addClass('campo-readonly');
			getCallback()(row);
		});
	});
}

// Función de utilidad para destacar la fila seleccionada
// ✅ MEJORADA: Función destacar fila con verificación adicional
function destacarFilaSeleccionada(productoId) {
	console.log(`🎯 Destacando fila para producto ID: ${productoId}`);

	// Remover el destacado de todas las filas
	$("#tbListaDetalleRpr tbody tr").removeClass("selected");

	// Verificar que existe una fila con ese ID
	const $fila = $("#tbListaDetalleRpr tbody tr[data-p-id='" + productoId + "']");

	if ($fila.length === 0) {
		console.warn(`⚠️ No se encontró ninguna fila con data-p-id="${productoId}"`);
		return false;
	}

	// Añadir el destacado solo a la fila del producto seleccionado
	$fila.addClass("selected");
	console.log(`✅ Fila destacada correctamente para producto ${productoId}`);

	// Hacer scroll a la fila si está fuera de vista
	scrollAFilaSeleccionada($fila);

	return true;
}

// ✅ NUEVA: Función separada para scroll optimizado
function scrollAFilaSeleccionada($fila) {
	const $tableContainer = $("#tbListaDetalleRpr").closest('.table-responsive');

	if ($tableContainer.length > 0) {
		const containerTop = $tableContainer.offset().top;
		const containerHeight = $tableContainer.height();
		const rowTop = $fila.offset().top;

		// Solo hacer scroll si la fila está fuera del área visible
		if (rowTop < containerTop || rowTop > containerTop + containerHeight) {
			$tableContainer.animate({
				scrollTop: $tableContainer.scrollTop() + (rowTop - containerTop - containerHeight / 2)
			}, 300);
			console.log(`📜 Realizando scroll a la fila seleccionada`);
		}
	}
}

function marcarCampoModificado(input) {
	// Usar el parámetro input en lugar de this
	const $input = $(input);

	// Validar que el input existe
	if (!$input.length) {
		console.warn('marcarCampoModificado: Input no válido', input);
		return false;
	}

	const valorOriginal = $input.data('original-value');

	// Obtener valor actual con manejo de errores
	let valorActual = '';
	try {
		valorActual = $input.val() ? $input.val().replace(/,/g, '') : '';
	} catch (e) {
		console.error('Error al obtener valor del campo:', e);
		return false;
	}

	// Si no hay valor original definido, no podemos comparar
	if (valorOriginal === undefined) {
		return false;
	}

	// Determinar si el campo está modificado
	let esModificado = false;

	
	// Para el campo de bonificación (caso especial)
	if ($input.hasClass('input-ocd_boni') || $input.hasClass('input-rpd_boni')) {
		const originalTrim = (valorOriginal || '').toString().trim();
		const actualTrim = (valorActual || '').toString().trim();

		// Casos especiales: "0" y "" se consideran iguales
		if ((originalTrim === "0" && actualTrim === "") ||
			(originalTrim === "" && actualTrim === "0")) {
			esModificado = false;
		} else {
			esModificado = originalTrim !== actualTrim;
		}
	} else {
		// Para campos numéricos - manejar correctamente el caso del valor 0
		try {
			// Convertir valores a números, manejando cadenas vacías como 0
			let numOriginal = valorOriginal === '' || valorOriginal === null ? 0 : parseFloat(valorOriginal);
			let numActual = valorActual === '' ? 0 : parseFloat(valorActual);

			// Si ambos valores son realmente cero (o equivalentes a cero), no están modificados
			if ((numOriginal === 0 || isNaN(numOriginal)) &&
				(numActual === 0 || isNaN(numActual))) {
				esModificado = false;
			} else if (!isNaN(numOriginal) && !isNaN(numActual)) {
				// Ambos son números válidos, usar tolerancias específicas según el campo
				let tolerancia = 0.009; // Base para campos con 2 decimales

				if ($input.hasClass('input-ocd_dto1') || $input.hasClass('input-rpd_dto1') || 
					$input.hasClass('input-ocd_dto2') || $input.hasClass('input-rpd_dto2') ||
					$input.hasClass('input-ocd_dto3') || $input.hasClass('input-rpd_dto3') ||
					$input.hasClass('input-ocd_dto4') || $input.hasClass('input-rpd_dto4') ||
					$input.hasClass('input-ocd_dto_pa') || $input.hasClass('input-rpd_dto_pa')) {
					tolerancia = 0.09; // Para campos con 1 decimal
				} else if ($input.hasClass('input-ocd_plista') || $input.hasClass('input-rpd_plista')) {
					tolerancia = 0.0009; // Para campos con 3 decimales
				}

				// Si la diferencia supera la tolerancia, está modificado
				esModificado = Math.abs(numOriginal - numActual) > tolerancia;
			} else if (isNaN(numOriginal) !== isNaN(numActual)) {
				// Si uno es NaN y el otro no, están diferentes
				esModificado = true;
			}
		} catch (e) {
			console.error("Error al comparar valores:", e);
			esModificado = false; // En caso de error, no marcar como modificado
		}
	}
	
	// Aplicar o quitar la clase según corresponda
	if (esModificado) {
		$input.addClass('campo-modificado');
	} else {
		$input.removeClass('campo-modificado');
	}

	// Manejar el indicador visual
	const container = $input.closest('.input-container');
	if (esModificado) {
		if (container.find('.indicador-cambio').length === 0) {
			container.append('<div class="indicador-cambio"></div>');
		}
	} else {
		container.find('.indicador-cambio').remove();
	}

	return esModificado;
}
//'.input-ocd_plista, .input-ocd_dto1, .input-ocd_dto2, .input-ocd_dto3, .input-ocd_dto4, .input-ocd_dto_pa, .input-ocd_boni,
// .input-rpd_plista, .input-rpd_dto1, .input-rpd_dto2, .input-rpd_dto3, .input-rpd_dto4, .input-rpd_dto_pa, .input-rpd_boni, .input-rpd_cantidad_compte';

function activarSiguienteCampo(campoActual) {
	const $campoActual = $(campoActual);
	const $fila = $campoActual.closest('tr');
	const camposEditables = '.input-ocd_plista, .input-ocd_dto1, .input-ocd_dto2, .input-ocd_dto3, .input-ocd_dto4, .input-p_dto4, .input-ocd_dto_pa, .input-ocd_boni, .input-rpd_plista, .input-rpd_dto1, .input-rpd_dto2, .input-rpd_dto3, .input-rpd_dto4, .input-rpd_dto_pa, .input-rpd_boni, .input-rpd_cantidad_compte';
	const $camposEnFila = $fila.find(camposEditables);
	const indiceActual = $camposEnFila.index($campoActual);

	let $siguienteCampo = null;
	if (indiceActual < $camposEnFila.length - 1) {
		$siguienteCampo = $camposEnFila.eq(indiceActual + 1);
	} else if ($fila.next('tr').length) {
		$siguienteCampo = $fila.next('tr').find(camposEditables).first();
	}

	$campoActual.prop('readonly', true).addClass('campo-readonly');

	if ($siguienteCampo && $siguienteCampo.length) {
		$siguienteCampo.prop('readonly', false).removeClass('campo-readonly');
		setTimeout(() => { $siguienteCampo[0].focus(); $siguienteCampo[0].select(); }, 0);
	}
}

// Función de debounce para evitar llamadas repetidas
function debounce(func, wait) {
	let timeout;
	return function () {
		const context = this, args = arguments;
		clearTimeout(timeout);
		timeout = setTimeout(function () {
			func.apply(context, args);
		}, wait);
	};
}

// Aplicar debounce a funciones de cálculo intensivas
const ActualizarProductoEnDetalleRprSeccionPrecioDebounced = debounce(function (row, campoActual) {
	ActualizarProductoEnDetalleRprSeccionPrecio(row, campoActual);
}, 300);

// Aplicar debounce a funciones de cálculo intensivas
const ActualizarProductoEnDetalleRprSeccionFacturaDebounced = debounce(function (row, campoActual) {
	ActualizarProductoEnDetalleRprSeccionFactura(row, campoActual);
}, 300);

function ActualizarProductoEnDetalleRprSeccionPrecio(row, campoActual) {
	if (campoActual == undefined) {
		return false;
	}
	else {
		var pId = row.data('p-id');
		//var pId = pIdEnOcSeleccionado; 
		var field = $(campoActual).data('field');
		var val = normalizarNumero($(campoActual).val());
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
							//td[22].style.width = "10px";
						}
						else {
							td[22].innerHTML = "";
						}

						//DP
						if (obj.valorizacion_mostrar_dp) {
							td[23].innerHTML = obj.td_dp;
							td[23].style.padding = "0";
							td[23].style.textAlignLast = "center";
							//td[23].style.width = "10px";
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
}

function normalizarNumero(valor) {
	if (!valor) return "";

	// Quitar separadores de miles (coma)
	valor = valor.replace(/,/g, "");

	// Trim por seguridad
	valor = valor.trim();

	return valor;
}

function ActualizarProductoEnDetalleRprSeccionFactura(row, campoActual) {
	if (campoActual == undefined) {
		return false;
	}
	else {
		var pId = row.data('p-id');
		var field = $(campoActual).data('field');
		var val = normalizarNumero($(campoActual).val());
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
							//td[22].style.width = "10px";
						}
						else {
							td[22].innerHTML = "";
						}

						//DP
						if (obj.valorizacion_mostrar_dp) {
							td[23].innerHTML = obj.td_dp;
							td[23].style.padding = "0";
							td[23].style.textAlignLast = "center";
							//td[23].style.width = "10px";
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
}

/****************************************************************************************
################################ ADD-ON -FIN-  tbListaDetalleRpr  ##########################
*****************************************************************************************/