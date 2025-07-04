$(function () {
	$(document).on("change", "#listaTipoOP", ControlalistaTipoOPSelected);
	$(document).on("keydown.autocomplete", "input#Rel03", function () {
		$(this).autocomplete({
			source: function (request, response) {

				data = { prefix: request.term };

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
				ctaIdSelected = ui.item.id;
				ctaDescSelected = ui.item.value;


				return true;
			}
		});
	});
	CargarListaTiposDeOrdenDePago();
	$("#btnBuscar").on("click", function () {
		if (tipoOPSelected == "") {
			AbrirMensaje("ATENCIÓN", "Debe seleccionar un tipo de Orden de Pago.", function () {
				$("#msjModal").modal("hide");
				$("listaTipoOP").trigger("focus");
				return true;
			}, false, ["Aceptar"], "error!", null);
		}
		else {
			AceptarDesdeSeleccionarTipoDeOP();
		}
	});

});

function AceptarDesdeSeleccionarTipoDeOP() {
	var data = { tipoOP: tipoOPSelected };
	PostGenHtml(data, aceptarDesdeSeleccionarTipoDeOPUrl, function (obj) {
		if (obj != "") {
			$("#divDetalle").html(obj);
			$("#divFiltro").collapse("hide");
			$("#divDetalle").collapse("show");
			$("#chkRel04").prop("disabled", true);
			$("#chkRel04").trigger("change");
			return true;
		}
		else {
			AbrirMensaje("ATENCIÓN", "No se encontraron resultados.", function () {
				$("#msjModal").modal("hide");
				return true;
			}, false, ["Aceptar"], "error!", null);
			return false;
		}
	});
}

function ControlalistaTipoOPSelected() {
	if ($("#listaTipoOP").val() != "")
		tipoOPSelected = $("#listaTipoOP").val();
	else
		tipoOPSelected = "";
}

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
	$("#chkRel04").prop("disabled", true);
	$("#chkRel04").prop('checked', true);
	$("#chkRel04").trigger("change");
	$("#divFiltro").collapse("show")
	$("#divDetalle").collapse("hide");
}

function onChangeCondAfip(x) {
	var condAfip = $("#listaCondAfip").val();
	if (condAfip != "") {
		var data = { condAfip };
		PostGenHtml(data, buscarTiposComptesUrl, function (obj) {
			$("#divTipoCompte").html(obj);
			return true
		});
	}
}

function selectReg(x, gridId) {
	$("#" + gridId + " tbody tr").each(function (index) {
		$(this).removeClass("selected-row");
		$(this).removeClass("selectedEdit-row");
	});
	$(x).addClass("selected-row");
}

function onChangeFechaCompte(x) {

}

function quitarConceptoFacturado(x) { }

function quitarOtroTributo() { }

function btnAbmCancelar_click() { }