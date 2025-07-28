$(function () {
	InicializaPantalla();
	$(document).on("click", "#btnAbmAceptar", btnAbmAceptarClick); //Abrir modal
	//
});

function InicializaPantalla() {
	$("#divFiltro").collapse("show");
	$("#lbRel01").text("Proveedor");
	$("#chkRel01").prop('checked', true);
	$("#chkRel01").trigger("change");
	$("#chkRel01").prop("disabled", true);
	$("#Rel01").prop("disabled", false);
	$("input#Rel01").on("click", function () {
		$("input#Rel01").val("");
		$("#Rel01Item").val("");
	});
	$("#Rel01List").collapse("hide");
	$("#btnBuscar").on("click", function () {
		if (ctaIdSelected == "") {
			AbrirMensaje("ATENCIÓN", "Debe seleccionar una cuenta.", function () {
				$("#msjModal").modal("hide");
				$("input#Rel01").trigger("focus");
				return true;
			}, false, ["Aceptar"], "error!", null);
		}
		else {
			InicializarTabsComprobantes(ctaIdSelected);
		}
	});
	$("#btnCancel").on("click", function () {
		InicializarDatosEnSesion(true);
		InicializaPantalla();
		LimpiarDatosDelFiltroInicial();
		$("#btnFiltro").trigger("click");
		$("#btnDetalle").trigger("click");
		$("#divDetalle").collapse("hide");
	});
	$("#btnAbmCancelar").on("click", function () {
		InicializarDatosEnSesion(true);
		InicializaPantalla();
		LimpiarDatosDelFiltroInicial();
		$("#btnFiltro").trigger("click");
		$("#btnDetalle").trigger("click");
		$("#divDetalle").collapse("hide");
	});
	$(".activable").prop("disabled", true);
	$("#btnAbmAceptar").hide();
	$("#btnAbmCancelar").hide();
	$("#btnDetalle").prop("disabled", true);

	activarBotones(false);
	ctaIdSelected = "";
	MostrarDatosDeCuenta(false);

	document.getElementById("Rel01").focus();

	CerrarWaiting();
	return true;
}

function LimpiarDatosDelFiltroInicial() {
	$("input#Rel01").val("");
	$("#Rel01Item").val("");
	$("#Rel01List").empty();
}

function btnAbmAceptarClick() {
	var valResult = ValidarAntesDeConfirmar();
	if (valResult.error === true) {
		AbrirMensaje("ATENCIÓN", valResult.msg, function () {
			$("#msjModal").modal("hide");
			return true;
		}, false, ["Aceptar"], "error!", null);
	}
	else {
		AbrirMensaje("ATENCIÓN", "¿Confirma la Justificación de los comprobantes seleccionados?", function (e) {
			$("#msjModal").modal("hide");
			switch (e) {
				case "SI": //Confirmar
					var comprobantes = ObtenerComprobantes();
					var rps = ObtenerComprobantesRpr();
					var data = { cta_id: $("#CtaID").val(), comprobantes, rps };
					AbrirWaiting();
					PostGen(data, confirmarJustificacionUrl, function (obj) {
						if (obj.error === true) {
							CerrarWaiting();
							ControlaMensajeError(obj.msg);
						}
						else {
							ControlaMensajeSuccess(obj.msg);
							$("#btnAbmCancelar").trigger("click");
						}
						return true;
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
}

function ObtenerComprobantes() {
	var comprobantes = [];
	var dataTable = document.getElementById('tbListaComprobantes');
	var inputs = dataTable.querySelectorAll('tbody>tr>td>input');
	inputs.forEach(function (input) {
		if (input.checked) {
			if (input.parentNode && input.parentNode.parentNode && input.parentNode.parentNode.childNodes && input.parentNode.parentNode.childNodes.length > 0) {
				comprobantes.push({
					cta_id: $("#CtaID").val(),
					tco_id: input.parentNode.parentNode.childNodes[13].innerText,
					cm_compte: input.parentNode.parentNode.childNodes[1].innerText,
					dia_movi: input.parentNode.parentNode.childNodes[15].innerText
				});
			}
		}
	});
	return comprobantes;
	//if (id_selected == 1) {
	//	//RPR
	//	var dataTable = document.getElementById('tbGridRprAsociado');
	//	var inputs = dataTable.querySelectorAll('tbody>tr>td>input');
	//	var pIds = [];
	//	inputs.forEach(function (input) {
	//		if (input.checked) {
	//			alMenosUno = true;
	//			pIds.push(input.id.substr(3, 11));
	//		}
	//	});
	//	if (pIds.length > 0) {
	//		for (var i = 0; i < pIds.length; i++) {
	//			asociaciones.push({ tco_id: "RPR", cm_compte_rp: pIds[i] });
	//		}
	//	}
	//}
	//else if (id_selected == 4) {
	//	//Notas a cuenta
	//	var dataTable = document.getElementById('tbGridNotasACuenta');
	//	var inputs = dataTable.querySelectorAll('tbody>tr>td>input');
	//	var pIds = [];
	//	inputs.forEach(function (input) {
	//		if (input.checked) {
	//			alMenosUno = true;
	//			if (input.parentNode && input.parentNode.parentNode && input.parentNode.parentNode.childNodes && input.parentNode.parentNode.childNodes.length > 0) {
	//				asociaciones.push({ tco_id: input.parentNode.parentNode.childNodes[13].innerText, cm_compte_rp: input.parentNode.parentNode.childNodes[11].innerText });
	//			}
	//		}
	//	});
	//}
	//return asociaciones;
}

function ObtenerComprobantesRpr() {
	var comprobantes = [];
	var dataTable = document.getElementById('tbListaRP');
	var inputs = dataTable.querySelectorAll('tbody>tr>td>input');
	inputs.forEach(function (input) {
		if (input.checked) {
			if (input.parentNode && input.parentNode.parentNode && input.parentNode.parentNode.childNodes && input.parentNode.parentNode.childNodes.length > 0) {
				comprobantes.push({
					cta_id: $("#CtaID").val(),
					rp_compte: input.parentNode.parentNode.childNodes[3].innerText
				});
			}
		}
	});
	return comprobantes;
}

function ValidarAntesDeConfirmar() {
	var existeItemSeleccionado = false;
	$("#tbListaComprobantes").find('tr').each(function (i, el) {
		var td = $(this).find('td');
		if (td.eq(0)[0]) {
			if (td.eq(5)[0].children[0].checked) {
				existeItemSeleccionado = true;
				return false; // Salir del each si se encuentra al menos un item seleccionado
			}
		}
	});
	if (!existeItemSeleccionado) {
		return { error: true, msg: "Debe seleccionar al menos un comprobante para Justificar y/o Relacionar." };
	}
	return { error: false, msg: "" };;
}

function onChangeChkJustifica(x) { }

function InicializarTabsComprobantes(ctaId) {
	var data = { ctaId };
	PostGenHtml(data, inicializarComprobantesUrl, function (obj) {
		$("#divComprobantesyRp").html(obj);
		$("#divDetalle").collapse("show");
		$("#btnDetalle").prop("disabled", false);
		$("#divFiltro").collapse("hide")
		MostrarDatosDeCuenta(true);
		activarBotones(true);
		AgregarHandlerAGrillaCheckAll('tbListaComprobantes');
		AgregarHandlerAGrillaCheckAll('tbListaRP');
		CerrarWaiting();
		return true
	});
}

function InicializarDetalleRp(compteId) {
	var data = { compteId };
	PostGenHtml(data, inicializarDetalleRpUrl, function (obj) {
		$("#divDetalles").html(obj);
		CerrarWaiting();
		return true
	});
}

function activarBotones(activar) {
	if (activar === true) {
		$("#btnAbmAceptar").prop("disabled", false);
		$("#btnAbmCancelar").prop("disabled", false);
		$("#btnAbmAceptar").show();
		$("#btnAbmCancelar").show();
	}
	else {
		$("#btnAbmAceptar").prop("disabled", true);
		$("#btnAbmCancelar").prop("disabled", true);
		$("#btnAbmAceptar").hide();
		$("#btnAbmCancelar").hide();
	}
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

function InicializarDatosEnSesion(limpiaCtaIdSelected) {
	if (limpiaCtaIdSelected) {
		ctaIdSelected = "";
		ctaDescSelected = "";
	}
	PostGen({}, inicializarDatosEnSesionURL, function (obj) {
		if (obj.error === true) {
			ControlaMensajeError(obj.msg);
		}
	});
}

function selectReg(x, gridId) {
	$("#" + gridId + " tbody tr").each(function (index) {
		$(this).removeClass("selected-row");
		$(this).removeClass("selectedEdit-row");
	});
	$(x).addClass("selected-row");
	if (gridId === "tbListaRP") {
		var compteId = $(x).find("td").eq(1).text().trim();
		InicializarDetalleRp(compteId);
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


function AgregarHandlerAGrillaCheckAll(grilla) {
	var dataTable = document.getElementById(grilla);
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