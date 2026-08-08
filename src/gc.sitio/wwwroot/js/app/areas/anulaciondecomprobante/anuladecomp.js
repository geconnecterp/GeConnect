$(function () {
	InicializaPantalla();
	$(document).on("click", "#btnAbmAceptar", btnAbmAceptarClick); //Abrir modal
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
			InicializarComprobante(ctaIdSelected);
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
	LimpiarSeleccionEnRadioButtons();
	document.getElementById("Rel01").focus();

	CerrarWaiting();
	return true;
}

function LimpiarSeleccionEnRadioButtons() {
	$('input[name="opcion"][value="opcion1"]').prop('checked', false);
	$('input[name="opcion"][value="opcion2"]').prop('checked', false);
	$('input[name="opcion"][value="opcion3"]').prop('checked', false);
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
		AbrirMensaje("ATENCIÓN", "¿Confirma la anulación del comprobante seleccionado?", function (e) {
			$("#msjModal").modal("hide");
			switch (e) {
				case "SI": //Confirmar
					var ctaId = ctaIdSelected;
					var diaMovi = "";
					var tcoId = "";
					var cmCompte = "";
					var opcion = "";
					if (optSelected == "opcion1" || optSelected == "opcion2") {
						diaMovi = diaMoviGrid1;
						tcoId = tcoIdGrid1;
						cmCompte = cmCompteGrid1;
						if (optSelected == "opcion1") {
							opcion = "1";
						}
						else {
							opcion = "2";
						}
					}
					else { //opcion3 seleccionada
						diaMovi = diaMoviGrid2;
						tcoId = tcoIdGrid2;
						cmCompte = cmCompteGrid2;
						opcion = "3";
					}
					var data = { ctaId, diaMovi, tcoId, cmCompte, opcion };
					AbrirWaiting();
					PostGen(data, confirmarAnulacionUrl, function (obj) {
						if (obj.error === true) {
							CerrarWaiting();
							//ControlaMensajeError(obj.msg);
							AbrirMensaje("ATENCIÓN", obj.msg, function () {
								$("#msjModal").modal("hide");
								return true;
							}, false, ["Aceptar"], "error!", null);
						}
						else {
							AbrirMensaje("ATENCIÓN", obj.msg, function () {
								$("#msjModal").modal("hide");
								return true;
							}, false, ["Aceptar"], "error!", null);
							//ControlaMensajeSuccess(obj.msg);
							InicializarDatosEnSesion(false);
							InicializarComprobante(ctaIdSelected);
							LimpiarSeleccionEnRadioButtons();
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

function ValidarAntesDeConfirmar() {
	if (optSelected == undefined) {
		return { error: true, msg: "Debe seleccionar una opcion de tipo para anular." };
	}
	if (optSelected == "") {
		return { error: true, msg: "Debe seleccionar una opcion de tipo para anular." };
	}
	if (optSelected == "opcion1" || optSelected == "opcion2") {
		if (diaMoviGrid1 == "" || tcoIdGrid1 == "" || cmCompteGrid1 == "") {
			return { error: true, msg: "Debe seleccionar un Comprobante para anular." };
		}
	}
	else { //opcion3 seleccionada
		if ($("#MostrarGrilla").val() == "True") {
			if (diaMoviGrid2 == "" || tcoIdGrid2 == "" || cmCompteGrid2 == "") {
				return { error: true, msg: "Debe seleccionar una Nota a Cuenta para anular." };
			}
		}
		else {
			return { error: true, msg: "No existen Notas a Cuenta para Anular." };
		}
	}
	return { error: false, msg: "" };;
}

function InicializarComprobante(id) {
	AbrirWaiting();
	var cta_id = ctaIdSelected;

	var data = { cta_id };
	PostGenHtml(data, inicializarComprobanteUrl, function (obj) {
		$("#divComprobante").html(obj);
		$("#divDetalle").collapse("show");
		$("#btnDetalle").prop("disabled", false);
		$("#divFiltro").collapse("hide")
		MostrarDatosDeCuenta(true);
		activarBotones(true);
		$('#radioSection input').on('change', function () {
			optSelected = $('input[name=opcion]:checked', '#radioSection').val();
			if ($("#divLeyenda")) {
				if (optSelected === "opcion3") {
					$("#divLeyenda").show();
				}
				else {
					$("#divLeyenda").hide();
				}
			}
		});
		//$('input[name="opcion"][value="opcion1"]').prop('checked', true);
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
	optSelected = "";
	diaMoviGrid1 = "";
	tcoIdGrid1 = "";
	cmCompteGrid1 = "";
	diaMoviGrid2 = "";
	tcoIdGrid2 = "";
	cmCompteGrid2 = "";
	PostGen({}, inicializarDatosEnSesionURL, function (obj) {
		if (obj.error === true) {
			ControlaMensajeError(obj.msg);
		}
	});
}

function LimpiarDatosDelFiltroInicial() {
	$("input#Rel01").val("");
	$("#Rel01Item").val("");
	$("#Rel01List").empty();
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

function selectReg(x, gridId) {
	$("#" + gridId + " tbody tr").each(function (index) {
		$(this).removeClass("selected-row");
		$(this).removeClass("selectedEdit-row");
	});
	$(x).addClass("selected-row");
	if (gridId === "tbGrillaComprobantes") {
		CargarNotasACuenta(x);
		diaMoviGrid2 = "";
		tcoIdGrid2 = "";
		cmCompteGrid2 = "";
	}
	else {
		diaMoviGrid2 = $(x).find("td").eq(1).text().trim();
		tcoIdGrid2 = $(x).find("td").eq(9).text().trim();
		cmCompteGrid2 = $(x).find("td").eq(3).text().trim();
	}
}

function selectRegDbl(x, gridId) { }

function CargarNotasACuenta(x) {
	var ctaId = $("#CtaID").val();
	var diaMovi = $(x).find("td").eq(1).text().trim();
	diaMoviGrid1 = $(x).find("td").eq(1).text().trim();
	var tcoId = $(x).find("td").eq(9).text().trim();
	tcoIdGrid1 = $(x).find("td").eq(9).text().trim();
	var cmCompte = $(x).find("td").eq(3).text().trim();
	cmCompteGrid1 = $(x).find("td").eq(3).text().trim();
	var data = { ctaId, diaMovi, tcoId, cmCompte };
	PostGenHtml(data, inicializarVistaNotasACuentaUrl, function (obj) {
		$("#divGrillaNotas").html(obj);
		if ($("#MostrarGrilla").val() == "True") {
			//$("#divGrillaNotas").collapse("show");
			$("#divGrillaNotas").show();
		}
		else {
			//$("#divGrillaNotas").collapse("hide");
			$("#divGrillaNotas").hide();
		}
		MostrarLeyenda();
		CerrarWaiting();
		return true
	});
}

function MostrarLeyenda() {
	optSelected = $('input[name="opcion"]:checked').val();
	if (optSelected !== "") {
		if ($("#divLeyenda")) {
			if (optSelected === "opcion3") {
				$("#divLeyenda").show();
			}
			else {
				$("#divLeyenda").hide();
			}
		}
	}
	else {
		if ($("#divLeyenda")) {
			$("#divLeyenda").hide();
		}
	}
}