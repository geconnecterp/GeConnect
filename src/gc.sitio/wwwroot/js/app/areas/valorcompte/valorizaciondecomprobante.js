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
	InicializarPantallaDeFiltros();
});

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
		//TODO MARCE : completar el metodo para agregar un elemento en la lista Desc Financ.
	}
}

function selectListaValorizacion(x) { }

function selectListaDescFinanc(x) { }

function quitarDescFinanc(x) {
	var cm_compte = $(e).attr("data-interaction");
	var data = { cm_compte };
	PostGenHtml(data, quitarDescFinancURL, function (obj) {
		$("#divDescFinanc").html(obj);
		AddEventListenerToGrid("tbListaDescFinanc");
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
			}
		});
	}
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