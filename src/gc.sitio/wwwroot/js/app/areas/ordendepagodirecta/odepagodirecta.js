$(function () {
	CargarListaTiposDeOrdenDePago();

});

function CargarListaTiposDeOrdenDePago() {
	var data = {};
	PostGenHtml(data, buscarTiposDeOrdenDePagoUrl, function (obj) {
		$("#divLstTipoOrdenDePago").html(obj);
		InicializarFiltros();
		return true
	});
}

function InicializarFiltros() {
	$("#lbRel04").text("Tipos de Orden de Pago");
	$("#chkRel04").prop("disabled", false);
	$("#chkRel04").trigger("change");
	$("#divFiltro").collapse("show")
	$("#divDetalle").collapse("hide");
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