$(function () {
	$(document).on("click", "#btnCancelDesdeValidPrev", CancelDesdeValidPrev);
	$(document).on("click", "#btnAceptarDesdeValidPrev", AceptarDesdeValidPrev);
	InicializaPantalla();
	$("#btnBuscar").on("click", function () {
		if (ctaIdSelected == "") {
			AbrirMensaje("ATENCIÓN", "Debe seleccionar una cuenta.", function () {
				$("#msjModal").modal("hide");
				$("input#Rel01").focus();
				return true;
			}, false, ["Aceptar"], "error!", null);
		}
		else {
			ValidarProveedor();
		}
	});
});

function CancelDesdeValidPrev() {
	var data = {};
	PostGen(data, cancelarDesdeGrillaDeValidacionesPreviasURL, function (obj) {
		if (obj.error === true) {
			AbrirMensaje("ATENCIÓN", obj.msg, function () {
				$("#msjModal").modal("hide");
				return true;
			}, false, ["Aceptar"], "error!", null);
		}
		else {
			InicializaPantalla();
			$("#divDetalle").collapse("hide");
		}
	});
}

function selectReg(x, gridId) {
	$("#" + gridId + " tbody tr").each(function (index) {
		$(this).removeClass("selected-row");
		$(this).removeClass("selectedEdit-row");
	});
	$(x).addClass("selected-row");

}

function selectRegDbl(x, gridId) {
	ControlaMensajeWarning("Method not implemented.")
}

function AceptarDesdeValidPrev() {
	//Cargar con el backend la vista siguiente
	var cta_id = ctaIdSelected;
	var data = { cta_id };
	PostGenHtml(data, cargarVistaObligacionesYCreditosURL, function (obj) {
		$("#divDetalle").html(obj);
		//Setear los valores de la cuenta seleccionada en la vista de obligaciones y creditos
		$("#CtaID").val(ctaIdSelected);
		$("#CtaDesc").val(ctaDescSelected);
		MostrarDatosDeCuenta(true);
		CargarObligacionesOCreditos("D"); //Obligaciones
		CargarObligacionesOCreditos("H"); //Créditos
	});
}

function CargarObligacionesOCreditos(tipo) {
	var data = { tipo };
	PostGen(data, cargarObligacionesOCreditosURL, function (obj) {
		if (tipo == "D") {
			$("#divObligaciones").html(obj);
		}
		else {
			$("#divCreditos").html(obj);
		}
	});
}

function InicializaPantalla() {
	$("#chkRel01").prop('checked', true);
	$("#chkRel01").trigger("change");
	$("#chkRel01").prop("disabled", true);
	$("#Rel01").prop("disabled", false);
	$("input#Rel01").on("click", function () {
		$("input#Rel01").val("");
		$("#Rel01Item").val("");
	});
	$("input#Rel01").trigger("click");
	$("#Rel01List").empty();
	$("#Rel01").focus();
	$("#lbRel01").text("Proveedor");
	$("#btnDetalle").prop("disabled", true);
	ctaIdSelected = "";
	MostrarDatosDeCuenta(false);
	$("#divFiltro").collapse("show")
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


		return true;
	}
});

function ValidarProveedor() {
	var cta_id = ctaIdSelected;
	var data = { cta_id }
	PostGenHtml(data, validarProveedorURL, function (obj) {
		$("#divDetalle").html(obj);
		if ($("#tbListaValidacionesPrev tbody tr").length <= 0) {
			//Si la lista esta vacia, levanto la vista siguiente
			//Cargar con el backend la vista siguiente, y tirarla a divDetalle

			$("#divFiltro").collapse("hide");
			$("#divDetalle").collapse("show");
		}
		else {
			//Si hay al menos un registro con 'true' en la tercer columna, no permito continuar
			var puedoContinuar = true;
			$("#tbListaValidacionesPrev").find('tr').each(function (i, el) {
				var td = $(this).find('td');
				if (td.length > 0) {
					if (td[2].innerText == "True") {
						puedoContinuar = false;
					}
				}
			});
			if (!puedoContinuar) {
				$("#btnAceptarDesdeValidPrev").prop("disabled", true);
			}
			$("#divFiltro").collapse("hide");
			$("#divDetalle").collapse("show");
		}
	});
}