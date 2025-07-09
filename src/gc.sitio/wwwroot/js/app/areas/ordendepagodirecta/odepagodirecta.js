$(function () {
	$(document).on("change", "#listaTipoOP", ControlalistaTipoOPSelected);
	$(document).on("change", "#listaTCompte", ControlalistaTCompteSelected);
	$(document).on("click", "#btnAgregarConceptoFacturado", AbrirModalConceptoFacturado); //Abrir modal
	$(document).on("click", "#btnAgregarOtroTributo", AbrirModalAgregarOtroTributo); //Abrir modal
	$(document).on("change", "#listaIvaSit", ControlaSituacionSeleccionada);

	$(document).on("keyup", "#ConceptoFacturado_concepto", ControlaKeyUpConceptoFacturado_concepto);
	$(document).on("keyup", "#ConceptoFacturado_subtotal", ControlaKeyUpConceptoFacturado_subtotal);
	$(document).on("focusin", "#ConceptoFacturado_subtotal", ControlaGotFocusConceptoFacturado_subtotal);
	$(document).on("keyup", "#ConceptoFacturado_iva", ControlaKeyUpConceptoFacturado_iva);
	$(document).on("focusin", "#ConceptoFacturado_iva", ControlaGotFocusConceptoFacturado_iva);

	$(document).on("focusin", "#OtroTributo_base_imp", ControlaGotFocusOtroTributo_base_imp);
	$(document).on("focusin", "#OtroTributo_alicuota", ControlaGotFocusOtroTributo_alicuota);
	$(document).on("keyup", "#OtroTributo_base_imp", ControlaKeyUpOtroTributo_base_imp);

	$(document).on("keyup", "#itemOPD_cm_compte_pto_vta", ControlaKeyUpComptePtoVta);
	$(document).on("keyup", "#itemOPD_cm_compte_pto_nro", ControlaKeyUpCompteNro);

	$(document).on("click", "#btnAbmAgregarItem", AbmAgregarItem);
	//

	$(".inputEditable").on("keypress", analizaEnterInput);
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

const IvaSituacion = {
	GRAVADO: 'G',
	NO_GRAVADO: 'N',
	EXENTO: 'E'
}

const formatter = new Intl.NumberFormat('de-DE', {
	minimumFractionDigits: 2,
	maximumFractionDigits: 2
});

var keysAceptadas = [8, 37, 39, 46, 96, 97, 98, 99, 100, 101, 102, 103, 104, 105, 110, 190];

function ControlaKeyUpComptePtoVta(e) {
	if (e.which == 13 || e.which == 109) {
		var aux = $("#itemOPD_cm_compte_pto_vta").inputmask('unmaskedvalue').padStart(4, '0');
		$("#itemOPD_cm_compte_pto_vta").val(aux);
		$("#itemOPD_cm_compte_pto_nro").trigger("focus");
	}
}

function ControlaKeyUpCompteNro(e) {
	if (e.which == 13 || e.which == 109) {
		var aux = $("#itemOPD_cm_compte_pto_nro").inputmask('unmaskedvalue').padStart(8, '0');
		$("#itemOPD_cm_compte_pto_nro").val(aux);
		$("#itemOPD_cm_fecha").trigger("focus");
	}
}

var mensajeErrorAlAgregarAntesDeGuardar = "";
var focusObject = "";
function AbmAgregarItem() {
	if (ValidarAntesDeAgregar()) {
		AbrirMensaje("ATENCIÓN!!", "¿Agrega el Comprobante?", function (e) {
			$("#msjModal").modal("hide");
			switch (e) {
				case "SI":
					var request = ObtenerEncabezado();
					var data = { request };
					PostGen(data, agregarItemEnOpdPaso1Url, function (obj) {
						if (obj.error === true) {
							AbrirMensaje("ATENCIÓN", obj.msg, function () {
								$("#msjModal").modal("hide");
								return true;
							}, false, ["Aceptar"], "error!", null);
						}
						else {
							//Limpiar variables de sesión
							LimpiarCamposDeEdicion();
							CargarGrillasAdicionales();
							CargarListaObligaciones();
							ActualizarTotalesSuperiores();
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
	else {
		AbrirMensaje("ATENCIÓN", mensajeErrorAlAgregarAntesDeGuardar, function () {
			$("#msjModal").modal("hide");
			$(focusObject).trigger("focus");
			mensajeErrorAlAgregarAntesDeGuardar = "";
			focusObject = "";
			return true;
		}, false, ["Aceptar"], "error!", null);
	}
}

function ActualizarTotalesSuperiores() {
	PostGen(data, actualizarTotalesSuperioresUrl, function (obj) {
		if (obj.error === true) {
			ControlaMensajeError(obj.msg);
		}
		else {
			$("#txtObligACancelar").val(formatter.format(obj.data.obligacionesCancelar));
			$("#txtCredYValImputados").val(formatter.format(obj.data.credYValImputados));
			$("#txtDiferencias").val(formatter.format(obj.data.diferencia));
		}
	});
}

function CargarListaObligaciones() {
	var data = {};
	PostGenHtml(data, cargarGrillaObligacionesUrl, function (obj) {
		$("#divObligaciones").html(obj);
		return true
	});
}

function LimpiarCamposDeEdicion() {
	$("#itemOPD_cm_cuit").val("");
	$("#itemOPD_cm_nombre").val("");
	$("#itemOPD_cm_domicilio").val("");
	$("#listaCondAfip").val("");
	$("#listaTCompte").val("");
	$("#itemOPD_cm_compte_pto_vta").val("");
	$("#itemOPD_cm_compte_pto_nro").val("");
	$("#itemOPD_cm_fecha").val(moment().format('yyyy-MM-DD'));
	$("#listaCtaDir").val("");
	$("#Rel03").val("");
}

function LimpiarVariablesDeSesion() {
	var data = {};
	PostGen(data, limpiarVariablesDeSesionUrl, function (obj) {
		if (obj.error === true) {
			AbrirMensaje("ATENCIÓN", obj.msg, function () {
				$("#msjModal").modal("hide");
				return true;
			}, false, ["Aceptar"], "error!", null);
		}
		else {
			//Limpiar variables de sesión
			$("#divDetalle").html("");
			CargarListaTiposDeOrdenDePago();
			return true;
		}
	});
}

function ValidarAntesDeAgregar() {
	var res = true;
	let condAfip = $("#listaCondAfip").val();
	if (condAfip == "") {
		mensajeErrorAlAgregarAntesDeGuardar = "Debe seleccionar una Condición de Afip.";
		focusObject = "#listaCondAfip";
		return false;
	}

	let cuit = $("#itemOPD_cm_cuit").inputmask('unmaskedvalue');
	if (cuit == "") {
		mensajeErrorAlAgregarAntesDeGuardar = "Debe ingresar un valor de CUIT.";
		focusObject = "#itemOPD_cm_cuit";
		return false;
	}

	let tipoCompte = $("#listaTCompte").val();
	if (tipoCompte == "") {
		mensajeErrorAlAgregarAntesDeGuardar = "Debe seleccionar un Tipo de Comprobante.";
		focusObject = "#listaTCompte";
		return false;
	}

	let ptoVta = $("#itemOPD_cm_compte_pto_vta").val();
	if (ptoVta == "") {
		mensajeErrorAlAgregarAntesDeGuardar = "El valor de Nro de Comprobante no es válido.";
		focusObject = "#itemOPD_cm_compte_pto_vta";
		return false;
	}

	let nro = $("#itemOPD_cm_compte_pto_nro").val();
	if (nro == "") {
		mensajeErrorAlAgregarAntesDeGuardar = "El valor de Nro de Comprobante no es válido.";
		focusObject = "#itemOPD_cm_compte_pto_nro";
		return false;
	}

	var rowsConceptosFacturados = $("#tbGridConceptoFacturado > tbody > tr").length;
	if (rowsConceptosFacturados <= 0) {
		
		mensajeErrorAlAgregarAntesDeGuardar = "Debe al menos ingresar un Concepto Facturado.";
		focusObject = "#btnAgregarConceptoFacturado";
		return false;
	}
	return true;
}

function AceptarDesdeSeleccionarTipoDeOP() {
	var data = { tipoOP: tipoOPSelected };
	PostGenHtml(data, aceptarDesdeSeleccionarTipoDeOPUrl, function (obj) {
		if (obj != "") {
			$("#divDetalle").html(obj);
			$("#divFiltro").collapse("hide");
			$("#divDetalle").collapse("show");
			$("#chkRel04").prop("disabled", true);
			$("#chkRel04").trigger("change");
			CargarGrillasAdicionales();
			CargarMascaras();
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

function CargarMascaras() {
	$("#itemOPD_cm_cuit").inputmask("99-99999999-9");
	$("#itemOPD_cm_compte_pto_vta").inputmask("9999");
	$("#itemOPD_cm_compte_pto_nro").inputmask("99999999");

	var now = moment().format('yyyy-MM-DD');
	$("#itemOPD_cm_fecha").attr('min', now);
	$("#itemOPD_cm_fecha").val(now);
}

function ControlalistaTipoOPSelected() {
	if ($("#listaTipoOP").val() != "") {
		tipoOPSelected = $("#listaTipoOP").val();
	}
	else
		tipoOPSelected = "";
}

function ControlalistaTCompteSelected() {
	var tco_id = $("#listaTCompte option:selected").val()
	if (tco_id != "") {
		//Grilla de Otros Tributos
		CargarGrillaOtrosTributos();
	}
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

	if (gridId == "tbListaObligaciones_Paso1") {
		CargarItemsObligacionDesdeElementoSeleccionado(x);
	}
}

function CargarItemsObligacionDesdeElementoSeleccionado(x) {
	AbrirWaiting("Cargando datos...");
	var aux = x.cells[0].innerText.trim();
	var aux2 = x.childNodes[7].innerText;
	var afip_id = x.cells[5].innerText.trim();
	var cm_cuit = x.cells[8].innerText.trim();
	var tco_id = x.cells[7].innerText.trim();
	var cm_compte = x.cells[6].innerText.trim();
	var data = { afip_id, cm_cuit, tco_id, cm_compte };
	PostGenHtml(data, cargarDatosDeComprobanteSeleccionadoUrl, function (obj) {
		$("#divDatosComprobante").html(obj);
		$("#Rel03").val($("#itemOPD_ctag_motivo").val());
		CargarGrillaOtrosTributosDesdeSeleccion(afip_id, cm_cuit, tco_id, cm_compte);
		CargarGrillaConceptosFacturadosDesdeSeleccion(afip_id, cm_cuit, tco_id, cm_compte);
		setTimeout(() => {
			CargarGrillaTotalesDesdeSeleccion();
		}, 500);
		return true
	});
}

function CargarGrillaTotalesDesdeSeleccion() {
	var data = {};
	PostGenHtml(data, cargarGrillaTotalesDesdeSeleccionUrl, function (obj) {
		$("#divTotales").html(obj);
		FormatearValores(tbGridTotales, [1]);
		CerrarWaiting();
		return true
	});
}

function onChangeFechaCompte(x) {

}

function tableUpDownArrow() {
	var table = document.querySelector('#tbGridOtroTributo tbody');
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
			(evt.altKey && evt.shiftKey && movKey[evt.code])
			||
			(evt.ctrlKey && movKey[evt.code])
			//||
			//evt.code === 'Tab'
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

				//if (pos.c == 8 && cellIndexTemp < pos.c) //moviendome desde la columna 'pedido bultos' hacia la derecha, la cual no es editable, debo saltar a la siguiente
				//	pos.c = 9;
				//if (pos.c == 6 && cellIndexTemp > pos.c) //moviendome desde la columna 'pedido bultos' hacia la izquierda, la cual no es editable, debo saltar a la siguiente
				//	pos.c = 15;
				//if (pos.c == 8 && cellIndexTemp > pos.c) //moviendome desde la columna 'precio lista' hacia la izquierda, la cual no es editable, debo saltar a la siguiente
				//	pos.c = 7;
				//if (pos.c == 16 && cellIndexTemp < pos.c) //moviendome desde la columna 'boni' hacia la derecha, la cual no es editable, debo saltar a la siguiente
				//	pos.c = 7;
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


function btnAbmCancelar_click() { }

function CalcularIva(e) {
	var sit_id = $("#listaIvaSit option:selected").val()
	var ali_id = $("#listaIvaAli option:selected").val()
	var subt = $("#ConceptoFacturado_subtotal").inputmask('unmaskedvalue');
	var iva = $("#ConceptoFacturado_iva").inputmask('unmaskedvalue');

	if (subt > 0 && sit_id == "G") {
		if (ali_id != "") {
			if (e.target.id != "ConceptoFacturado_iva") {
				var calc = (subt * parseFloat(ali_id)) / 100;
				$("#ConceptoFacturado_iva").val(calc.toFixed(2).replace(".", ","));
				$("#ConceptoFacturado_total").val((subt + calc).toFixed(2));
			}
			else {
				var calc = subt + iva;
				$("#ConceptoFacturado_total").val((calc).toFixed(2));
			}
		}
	}
	else if (subt > 0 && sit_id != "G") {
		if (e.target.id != "ConceptoFacturado_iva") {
			$("#ConceptoFacturado_iva").val(0);
			$("#ConceptoFacturado_total").val(subt);
		}
		else {
			$("#ConceptoFacturado_total").val(subt);
		}
	}
}

function FormatearValores(grilla, idx) {
	$(grilla).find('tr').each(function (i, el) {
		var td = $(this).find('td');
		if (td.length > 0) {
			for (var i = 0; i < idx.length; i++) {
				if (td[idx[i]].innerText !== undefined) {
					td[idx[i]].innerText = formatter.format(td[idx[i]].innerText);
				}
			}
		}
	});
}

function ControlaIvaAliSeleccionada() {
	var iva_ali_id = $("#listaIvaAli option:selected").val()
	if (iva_ali_id != "") {
		$("#ConceptoFacturado_subtotal").trigger("focus");
	}
}

function ControlaSituacionSeleccionada() {
	var iva_sit_id = $("#listaIvaSit option:selected").val()
	if (iva_sit_id == IvaSituacion.GRAVADO) {
		$("#listaIvaAli").val("");
		$("#listaIvaAli").prop("disabled", false);
		$("#ConceptoFacturado_iva").prop("disabled", false);
		$("#listaIvaAli").trigger("focus");
	}
	else {
		$("#listaIvaAli").val("");
		$("#listaIvaAli").prop("disabled", true);
		$("#ConceptoFacturado_iva").prop("disabled", true);
		$("#ConceptoFacturado_subtotal").trigger("focus");
	}
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

function ObtenerEncabezado() {
	var afip_id = $("#listaCondAfip").val();
	var cm_cuit = $("#itemOPD_cm_cuit").inputmask('unmaskedvalue');
	var cm_nombre = $("#itemOPD_cm_nombre").val();
	var cm_domicilio = $("#itemOPD_cm_domicilio").val();
	var tco_id = $("#listaTCompte").val();
	var tco_desc = $("#listaTCompte option:selected").text();
	var cm_compte = $("#itemOPD_cm_compte_pto_vta").val() + '-' + $("#itemOPD_cm_compte_pto_nro").val();
	var cm_fecha = $("#itemOPD_cm_fecha").val();
	var ctag_id = $("#listaCtaDir").val();
	var ctag_desc = $("#listaCtaDir option:selected").text();
	var ctag_motivo = $("#Rel03").val();
	var encabezado = {
		afip_id, cm_cuit, cm_nombre, cm_domicilio, tco_id, tco_desc, cm_compte, cm_fecha, ctag_id, ctag_desc, ctag_motivo
	};
	return encabezado;
}