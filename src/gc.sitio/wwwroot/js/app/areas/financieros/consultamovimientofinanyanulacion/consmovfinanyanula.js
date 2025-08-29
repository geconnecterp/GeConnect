$(function () {
	$("#Date1, #Date2").on("blur", ValidarFechasClick);

	$(document).on("change", "#listaCFO", ControlalistaCFOSelected);
	$(document).on("change", "#listaCFD", ControlalistaCFDSelected);
	$(document).on("change", "#listaTT", ControlalistaTTSelected);
	$(document).on("change", "#listaUsu", ControlalistaUsuSelected);

	$("#CFOList").on("dblclick", 'option', function () { $(this).remove(); })
	$("#CFDList").on("dblclick", 'option', function () { $(this).remove(); })
	$("#TTList").on("dblclick", 'option', function () { $(this).remove(); })
	$("#UsuList").on("dblclick", 'option', function () { $(this).remove(); })

	InicializarCamposEnFiltros();
});

function ValidarFechasClick() {
	const desde = $("#Date1").val();
	const hasta = $("#Date2").val();

	if (desde && hasta && desde > hasta) {
		AbrirMensaje("ATENCIÓN", "El valor de Fecha Desde no puede ser mayor a Fecha Hasta, revise.", function () {
			$("#msjModal").modal("hide");
			$("#Date1").val($("#Date2").val());
			return true;
		}, false, ["Aceptar"], "error!", null);
	} else {
		ActualizarListaDeUsuarios();
	}
}

function ActualizarListaDeUsuarios() {
	var data = { desde: $("#Date1").val(), hasta: $("#Date2").val() };
	PostGenHtml(data, actualizarListaDeUsuariosURL, function (obj) {
		$("#divUsuarios").html(obj);
		$("#chkUsu").on("click", function () {
			if ($("#chkUsu").is(":checked")) {
				$("#listaUsu").prop("disabled", false);
				$("#UsuList").prop("disabled", false);
				$("#listaUsu").trigger("focus");
			}
			else {
				$("#listaUsu").prop("disabled", true);
				$("#UsuList").prop("disabled", true);
			}
		});
		CerrarWaiting();
		return true
	});
}

function ControlalistaUsuSelected() {
	var item = $("#listaUsu").val();
	var desc = $("#listaUsu option:selected").text();
	if ($("#UsuList").has('option:contains("' + item + '")').length === 0 && $("#UsuList").has('option:contains("' + desc + '")').length === 0) {
		var opc = "<option value=" + item + ">" + desc + "</option>"
		$("#UsuList").append(opc);
	}
}

function ControlalistaCFOSelected() {
	var item = $("#listaCFO").val();
	var desc = $("#listaCFO option:selected").text();
	if ($("#CFOList").has('option:contains("' + item + '")').length === 0 && $("#CFOList").has('option:contains("' + desc + '")').length === 0) {
		var opc = "<option value=" + item + ">" + desc + "</option>"
		$("#CFOList").append(opc);
	}
}

function ControlalistaCFDSelected() {
	var item = $("#listaCFD").val();
	var desc = $("#listaCFD option:selected").text();
	if ($("#CFDList").has('option:contains("' + item + '")').length === 0 && $("#CFDList").has('option:contains("' + desc + '")').length === 0) {
		var opc = "<option value=" + item + ">" + desc + "</option>"
		$("#CFDList").append(opc);
	}
}

function ControlalistaTTSelected() {
	var item = $("#listaTT").val();
	var desc = $("#listaTT option:selected").text();
	if ($("#TTList").has('option:contains("' + item + '")').length === 0 && $("#TTList").has('option:contains("' + desc + '")').length === 0) {
		var opc = "<option value=" + item + ">" + desc + "</option>"
		$("#TTList").append(opc);
	}
}

function InicializarCamposEnFiltros() {
	$("#chkDesdeHasta").on("click", function () {
		if ($("#chkDesdeHasta").is(":checked")) {
			$("#Date1").prop("disabled", false);
			$("#Date2").prop("disabled", false);
			$("#Date1").trigger("focus");
		}
		else {
			$("#Date1").prop("disabled", true);
			$("#Date2").prop("disabled", true);
		}
	});
	$("#chkCFO").on("click", function () {
		if ($("#chkCFO").is(":checked")) {
			$("#listaCFO").prop("disabled", false);
			$("#CFOList").prop("disabled", false);
			$("#listaCFO").trigger("focus");
		}
		else {
			$("#listaCFO").prop("disabled", true);
			$("#CFOList").prop("disabled", true);
		}
	});
	$("#chkCFD").on("click", function () {
		if ($("#chkCFD").is(":checked")) {
			$("#listaCFD").prop("disabled", false);
			$("#CFDList").prop("disabled", false);
			$("#listaCFD").trigger("focus");
		}
		else {
			$("#listaCFD").prop("disabled", true);
			$("#CFDList").prop("disabled", true);
		}
	});
	$("#chkTT").on("click", function () {
		if ($("#chkTT").is(":checked")) {
			$("#listaTT").prop("disabled", false);
			$("#TTList").prop("disabled", false);
			$("#listaTT").trigger("focus");
		}
		else {
			$("#listaTT").prop("disabled", true);
			$("#TTList").prop("disabled", true);
		}
	});
	$("#chkUsu").on("click", function () {
		if ($("#chkUsu").is(":checked")) {
			$("#listaUsu").prop("disabled", false);
			$("#UsuList").prop("disabled", false);
			$("#listaUsu").trigger("focus");
		}
		else {
			$("#listaUsu").prop("disabled", true);
			$("#UsuList").prop("disabled", true);
		}
	});
}