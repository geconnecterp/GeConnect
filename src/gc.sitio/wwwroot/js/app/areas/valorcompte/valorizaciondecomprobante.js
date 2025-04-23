$(function () {
	$("input#Rel01").on("click", function () {
		$("input#Rel01").val("");
		$("#Rel01Item").val("");
		$("#chkRel04").prop('checked', false);
		$("#chkRel04").trigger("change");
		$("input#Rel03").prop("disabled", true);
		$("input#Rel04").prop("disabled", true);
	});
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

////codigo generico para autocomplete 03
//$("#Rel03").autocomplete({
//	source: function (request, response) {

//		data = { prefix: request.term }; Rel03

//		$.ajax({
//			url: autoComRel03Url,
//			type: "POST",
//			dataType: "json",
//			data: data,
//			success: function (obj) {
//				response($.map(obj, function (item) {
//					var texto = item.descripcion;
//					return { label: texto, value: item.descripcion, id: item.id, prov: item.provId };
//				}));
//			}
//		})
//	},
//	minLength: 3,
//	select: function (event, ui) {
//		if ($("#Rel03List").has('option:contains("' + ui.item.id + '")').length === 0) {
//			$("#Rel03Item").val(ui.item.id);
//			var opc = "<option value=" + ui.item.id + ">" + ui.item.value + "</option>"
//			$("#Rel03List").append(opc);
//		}
//		return true;
//	}
//});