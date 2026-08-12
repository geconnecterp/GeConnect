$(function () {
	const dateControl = document.querySelector('input[type="date"]');
	const dateControl2 = $('input[type="date"]');
	var local = moment().format('yyyy-MM-DD');
	var twoMonthPast = moment().add(-2, 'months')
	for (var i = 0; i < dateControl2.length; i++) {
		dateControl2[i].value = local;
		if (dateControl2[i].id == "dtpFechCompte") {
			dateControl2[i].setAttribute('min', twoMonthPast.format('yyyy-MM-DD'));
			dateControl2[i].setAttribute('max', local);
		}
		if (dateControl2[i].id == "dtpFechaTurno") {
			dateControl2[i].setAttribute('min', local);
		}
	}
	if (dateControl !== undefined && dateControl !== null)
		dateControl.value = local;
	$("#dtpFechaTurno").val(local);
	$("#btnBuscarCC").on("click", buscarCuentasComercial);
	InicializaPantallaCC("");
	$("#VerDetalle").on("click", VerDetalleClick);

	const verDetalle = document.getElementById("VerDetalle");
	if (verDetalle !== undefined && verDetalle !== null) {
		document.getElementById("VerDetalle").addEventListener("click", function () {
			if (!linkVerDetalle || linkVerDetalle == "") {
				AbrirMensaje("Atención", "Debe seleccionar un comprobante.", function () {
					$("#msjModal").modal("hide");
					return true;
				}, false, ["Aceptar"], "error!", null);
				return;
			}

			window.location.href = linkVerDetalle;
		});
	}
	$("#btnNuevoCompteDeRP").on("click", NuevoCompteDeRP);
	$("#btnEliminarCompteDeRP").on("click", EliminarCompteDeRP);
	$("#btnRegresarASelAuto").on("click", RegresarASelAuto); //Regregar a la pantalla de seleccion de autorizaciones.
	$("#btnAceptarAutoRP").on("click", AceptarAutoRP); //Generar Json de comprobante RPR
	$("#btnEliminarAutoRP").on("click", EliminarAutoRP); //Eliminar RPR cargado

	$("#NroComprobantePtoVta").mask("00000", { reverse: true });
	$("#NroComprobanteNumero").mask("000000000000000", { reverse: true });

	$("#Cuenta").on("keyup", analizaInputEnCuenta);
	$("#Cuenta").on("change", analizaChangeEnCuenta);
	$('#Cuenta').on('input', function () {
		const valor = $(this).val();
		if (valor === '') {
			$("#razonsocial").val("");
			document.dispatchEvent(new CustomEvent('cc:cuentaChanged', {
				detail: {
					cta_denominacion: undefined,
					cta_id: undefined
				}
			}));
		}
	});

	$("#txtNota").on("keyup", analizaInputTxtNota);
	$("#tco_id").on("change", analizaInputlistaTco);
	$("#txtMonto").on("keyup", ControlaKeyUpInputMonto);
	$("#listaDeposito").on("change", analizaInputlistaDeposito);
	$("#dtpFechaTurno").on("change", analizaInputTurno);
	$("#chkPonerEnCurso").on("change", analizaInputChkPonerEnCurso);
	$("#txtCantidadUL").on("keyup", analizaInputCantidadUL);

	$(document).on("keyup", "#NroComprobantePtoVta", ControlaKeyUpComptePtoVta);
	$(document).on("focusout", "#NroComprobantePtoVta", ControlaFocusOutComptePtoVta);
	$(document).on("keyup", "#NroComprobanteNumero", ControlaKeyUpCompteNro);
	$(document).on("focusout", "#NroComprobanteNumero", ControlaFocusOutCompteNro);

	if ($("#CtaId").val() !== "") {
		$("#btnBuscarCC").trigger("click");
	}
	if ($("#DepoId").val() !== "") {
		$("#listaDeposito").val($("#DepoId").val());
	}
	if (modelObj !== undefined && modelObj !== null)
		CargarCompteEnGrilla(modelObj);

	if (modelObj !== undefined && modelObj !== null && modelObj.rp == null) {
		$("#btnEliminarAutoRP").attr('disabled', 'disabled');
	}

	const cantUL = document.getElementById("txtCantidadUL");
	if (cantUL !== undefined && cantUL !== null) {
		cantUL.addEventListener('input', function (e) {
			if (!isValid(this.value))
				cantUL.value = 999;
		});
	}

	var grilla = document.getElementById("tbComptesDeRP");
	if (grilla !== undefined && grilla !== null)
		AddEventListenerToComptesGrid();

	AplicarFormato();
});

function ControlaKeyUpInputMonto(e) {
	if (e.which == 13 || e.which == 109) {
		$("#btnNuevoCompteDeRP").trigger("focus");
	}
}

function analizaInputlistaTco() {
	$("#NroComprobantePtoVta").trigger('focus');
}

function ControlaFocusOutComptePtoVta() {
	var ptv = $("#NroComprobantePtoVta").inputmask('unmaskedvalue');
	if (ptv != "") {
		var aux = $("#NroComprobantePtoVta").inputmask('unmaskedvalue').padStart(4, '0');
		$("#NroComprobantePtoVta").val(aux);
		$("#NroComprobanteNumero").trigger("focus");
	}
}

function ControlaKeyUpComptePtoVta(e) {
	if (e.which == 13 || e.which == 109) {
		var aux = $("#NroComprobantePtoVta").inputmask('unmaskedvalue').padStart(4, '0');
		$("#NroComprobantePtoVta").val(aux);
		$("#NroComprobanteNumero").trigger("focus");
	}
}

function ControlaFocusOutCompteNro() {
	var nro = $("#NroComprobanteNumero").inputmask('unmaskedvalue');
	if (nro != "") {
		var aux = $("#NroComprobanteNumero").inputmask('unmaskedvalue').padStart(8, '0');
		$("#NroComprobanteNumero").val(aux);
		$("#dtpFechCompte").trigger("focus");
	}
}

function ControlaKeyUpCompteNro(e) {
	if (e.which == 13 || e.which == 109) {
		var aux = $("#NroComprobanteNumero").inputmask('unmaskedvalue').padStart(8, '0');
		$("#NroComprobanteNumero").val(aux);
		$("#dtpFechCompte").trigger("focus");
	}
}

const formatter = new Intl.NumberFormat('de-DE', {
	minimumFractionDigits: 2,
	maximumFractionDigits: 2
});

function AplicarFormato() {
	//txtMonto
	getMaskForMoneyType("#txtMonto");
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

function AddEventListenerToComptesGrid() {
	var grilla = document.getElementById("tbComptesDeRP");
	if (grilla) {
		document.getElementById("tbComptesDeRP").addEventListener('click', function (e) {
			if (e.target.nodeName === 'TD') {
				var selectedRow = this.querySelector('.selected-row');
				if (selectedRow) {
					selectedRow.classList.remove('selected-row');
				}
				e.target.closest('tr').classList.add('selected-row');
			}
			ActualizarLinkBotonVerDetalle();
		});
	}
}

function isValid(value) {
	var cantUL = document.getElementById("txtCantidadUL")
	if (parseInt(value) <= cantUL.getAttribute('max'))
		return true;
	return false;
}

function VerDetalleClick() {
	if ($("#Cuenta") == undefined) {
		AbrirMensaje("Atención", "Debe especificar una cuenta válida.", function () {
			$("#msjModal").modal("hide");
			return true;
		}, false, ["Aceptar"], "error!", null);
	}
	if ($("#Cuenta").val() == "") {
		AbrirMensaje("Atención", "Debe especificar una cuenta válida.", function () {
			$("#msjModal").modal("hide");
			return true;
		}, false, ["Aceptar"], "error!", null);
	}
	if ($("#tco_id") == undefined) {
		AbrirMensaje("Atención", "Debe especificar una cuenta válida.", function () {
			$("#msjModal").modal("hide");
			return true;
		}, false, ["Aceptar"], "error!", null);
	}
}

function analizaChangeEnCuenta(e) {
	console.log(e);
	//document.dispatchEvent(new CustomEvent('cc:cuentaChanged', {
	//	detail: {
	//		cta_denominacion: obj.cuenta.cta_Denominacion,
	//		cta_id: obj.cuenta.cta_Id
	//	}
	//}));
}

function analizaInputEnCuenta(e) {
	if (e.key == "Enter") {
		buscarCuentasComercial();
	}
}

function analizaInputTurno(e) {
	ActualizarLinkBotonVerDetalle();
}

function analizaInputTxtNota(e) {
	ActualizarLinkBotonVerDetalle();
}

function analizaInputlistaDeposito() {
	ActualizarLinkBotonVerDetalle();
}

function analizaInputChkPonerEnCurso() {
	ActualizarLinkBotonVerDetalle();
}

function analizaInputCantidadUL() {
	ActualizarLinkBotonVerDetalle();
}

function ActualizarLinkBotonVerDetalle() {
	if (VerDetalleDeCompteDeRPUrl === "") {
		return false;
	}
	let item = obtenerCompteDeRPSeleccionado();
	if (item) {
		var tipoCompte = item.tipoId;
		var nroCompte = item.comprobante.completo;
		var monto = item.importe;
		var descTipoCompte = item.tipoDescripcion;
	}
	else {
		var tipoCompte = "";
		var nroCompte = "";
		var monto = "";
		var descTipoCompte = "";
	}

	var depoSelec = $("#listaDeposito").val();
	var notaAuto = $("#txtNota").val();
	var turno = moment($("#dtpFechaTurno").val()).format("X");
	var ponerEnCurso = $("#chkPonerEnCurso")[0].checked;
	var ul = $("#txtCantidadUL").val();
	var rp = $("#Rp").val();
	//var tipoCompte = $("#tco_id").val();
	//var nroCompte = $("#txtNroCompte").val(); TODO: Reemplazar por el nuevo componente
	//var nroCompte = $("#NroComprobantePtoVta").val() + "-" + $("#NroComprobanteNumero").val();
	var rp = $("#Rp").val();
	var fechaCompte = moment($("#fechaCompteDeRPSelected").val()).format("X");
	//var monto = $("#txtMonto").val();
	//var descTipoCompte = $("#tco_id option:selected").text();//
	var cta = $("#Cuenta").val();
	var link = VerDetalleDeCompteDeRPUrl + "?idTipoCompte=" + tipoCompte + "&nroCompte=" + nroCompte + "&depoSelec=" + depoSelec + "&notaAuto=" + notaAuto + "&turno=" + turno + "&ponerEnCurso=" + ponerEnCurso + "&ulCantidad=" + ul + "&rp=" + rp + "&ctaId=" + cta + "&tipoCuenta=" + tipoCuenta + "&fechaCompte=" + fechaCompte + "&monto=" + monto + "&descTipoCompte=" + descTipoCompte;
	if (!tipoCompte || tipoCompte.trim() === "" || !nroCompte || nroCompte.trim() === "" || nroCompte === "-") {
		linkVerDetalle = "";
	}
	else {
		linkVerDetalle = link;
	}
	$("#VerDetalle").prop("href", link);
}

function CargarCompteEnGrilla(obj) {
	if (obj != null) {
		$("#txtCantidadUL").val(obj.cantidadUL);
		if (obj.rpe_id == "P") {
			$("#chkPonerEnCurso")[0].checked = false;
		}
		else if (obj.rpe_id == "C") {
			$("#chkPonerEnCurso")[0].checked = true;
		}
		else {
			$("#chkPonerEnCurso")[0].checked = false;
		}
		if (obj.fechaTurno !== "")
			$("#dtpFechaTurno").val(obj.fechaTurno.substring(0, 10));
		if (obj.compte != null && obj.compte.tipo != "") {
			comptesDeRPGrid(obj.compte.tipo, obj.compte.tipoDescripcion, obj.compte.nroComprobante, obj.compte.fecha, obj.compte.importe, obj.rp);
		}
		else {//El usuario esta trabajando con un nuevo RP, el cual al no tener valor es null, cargo los comptes que tiene null en RP
			comptesDeRPGrid(null, null, null, null, null, null);
		}
	}
};

function EliminarAutoRP() {
	var rp_id = $("#Rp").val();
	if ($("#Rp").val() != "") {
		AbrirMensaje("Confirmación", "Esta a punto de eliminar un comprobante RPR, esta acción no puede deshacerse. Desea continuar?", function (e) {
			$("#msjModal").modal("hide");
			switch (e) {
				case "SI": //Borrar comprobante RPR
					EliminarComprobanteRPR(rp_id);
					break;
				case "NO":
					break;
				default: //NO
					break;
			}
			return true;
		}, true, ["Aceptar", "Cancelar"], "info!", null);
	}
	else {
		AbrirMensaje("Atención", "La autorización no ha sido registrada, no se puede 'Eliminar'.", function () {
			$("#msjModal").modal("hide");
			return true;
		}, false, ["Aceptar"], "error!", null);
	}
}

function EliminarComprobanteRPR(rp) {
	AbrirWaiting("");
	datos = { rp };
	PostGen(datos, eliminarComprobanteRPRUrl, function (o) {
		CerrarWaiting("");
		if (o.error === true) {
			AbrirMensaje("Atención", o.msg, function () {
				$("#msjModal").modal("hide");
				window.location.href = volverAListaDeAutorizacionesUrl;
				return true;
			}, false, ["Aceptar"], "error!", null);
		} else if (o.warn === true) {
			AbrirMensaje("Atención", o.msg, function () {
				$("#msjModal").modal("hide");
				window.location.href = volverAListaDeAutorizacionesUrl;
				return true;
			}, false, ["Aceptar"], "warn!", null);
		} else if (o.codigo.toString() !== "0") {
			AbrirMensaje("Atención", o.msg, function (e) {
				$("#msjModal").modal("hide");
				window.location.href = volverAListaDeAutorizacionesUrl;
				return true;
			}, false, ["Aceptar"], "info!", null);
		} else {
			window.location.href = volverAListaDeAutorizacionesUrl;
		}
	});
}

function AceptarAutoRP() {
	AbrirMensaje("ATENCIÓN", "¿Esta seguro que desea guardar la autorización?", function (e) {
		$("#msjModal").modal("hide");
		switch (e) {
			case "SI": //Confirmar la entrega
				GuardarDetalleDeProductos(true);
				break;
			case "NO":
				break;
			default: //NO
				break;
		}
		return true;

	}, true, ["Aceptar", "Cancelar"], "question!", null);
}

function RegresarASelAuto() {
	AbrirWaiting("");
	//Antes de volver debo validar si hay productos cargados en el detalle, si es así consultar con el operador
	datos = {};
	PostGen(datos, verificarDetalleCargadoURL, function (o) {
		CerrarWaiting();
		if (o.error === true) {
			AbrirMensaje("Atención", o.msg, function () {
				$("#msjModal").modal("hide");
				return true;
			}, false, ["Aceptar"], "error!", null);
		} else if (o.warn === true) {
			AbrirMensaje("Atención", o.msg, function () {
				$("#msjModal").modal("hide");
				return true;
			}, false, ["Aceptar"], "warn!", null);
		} else if (o.vacio === false) {
			AbrirMensaje("Atención", o.msg, function (e) {
				$("#msjModal").modal("hide");
				switch (e) {
					case "SI": //Guardar los cambios
						GuardarDetalleDeProductos(true);
						break;
					case "NO": //Cancelar la pre carga de productos en el detalle del comprobante
						GuardarDetalleDeProductos(false);
						break;
					default: //NO
						break;
				}
				return true;
			}, true, ["Aceptar", "Cancelar"], "info!", null);
		} else {
			window.location.href = volverAListaDeAutorizacionesUrl;
		}
	});
}

function GuardarDetalleDeProductos(guardado) {
	AbrirWaiting("Registrando Autorización...");
	var ulCantidad = $("#txtCantidadUL").val();
	var fechaTurno = $("#dtpFechaTurno").val();
	var depoId = $("#listaDeposito").val();
	var nota = $("#txtNota").val();
	var ponerEnCurso = $("#chkPonerEnCurso")[0].checked;
	var generar = true;
	datos = { guardado, generar, ponerEnCurso, ulCantidad, fechaTurno, depoId, nota };
	PostGen(datos, GuardarDetalleDeComprobanteRPUrl, function (o) {
		CerrarWaiting();
		if (o.error === true) {
			AbrirMensaje("Atención", o.msg, function () {
				$("#msjModal").modal("hide");
				return true;
			}, false, ["Aceptar"], "error!", null);
		} else if (o.warn === true) {
			AbrirMensaje("Atención", o.msg, function () {
				$("#msjModal").modal("hide");
				return true;
			}, false, ["Aceptar"], "warn!", null);
		} else {
			if (guardado) {
				AbrirMensaje("Atención", "La Autorización se guardó crrectamente.", function () {
					$("#msjModal").modal("hide");
					window.location.href = volverAListaDeAutorizacionesUrl;
				}, false, ["Aceptar"], "warn!", null);
			}
			else {
				AbrirMensaje("Atención", "Los cambios se cancelaron correctamente.", function () {
					$("#msjModal").modal("hide");
					window.location.href = volverAListaDeAutorizacionesUrl;
				}, false, ["Aceptar"], "warn!", null);
			}
			
		}
	});
}

function InicializaPantallaCC(tipo) {
	var compareValue = "";
	if (tipo !== "")
		compareValue = tipo;
	else
		compareValue = tipoCuenta;
	switch (compareValue) {
		case 'C':
			//Celeste 
			$("#iconTipoCC").removeClass(["text-primary", "text-secondary", "text-info", "text-danger"]).addClass("text-success");
			break;
		case 'B':
			$("#iconTipoCC").removeClass(["text-primary", "text-secondary", "text-success", "text-danger"]).addClass("text-info");
			break;
		case 'P':
			$("#iconTipoCC").removeClass(["text-primary", "text-success", "text-secondary", "text-info"]).addClass("text-danger");
			break;
		default:
			$("#iconTipoCC").removeClass(["text-primary", "text-success", "text-info", "text-danger"]).addClass("text-secondary");
			break;
	}
}

function NuevoCompteDeRP() {
	var nroCompte = $("#NroComprobantePtoVta").val() + "-" + $("#NroComprobanteNumero").val();
	var resultValidation = ValidarCamposEnComprobantesDeRP($("#Cuenta").val(), $("#tco_id").val(), nroCompte, $("#txtMonto").val());
	if (!resultValidation) {
		return;
	}
	else {
		comptesDeRPGrid($("#tco_id").val(), $("#tco_id")[0].selectedOptions[0].text, nroCompte, $("#dtpFechCompte").val(), $("#txtMonto").val(), $("#Rp").val());
	}
}

function AgregarComprobante() {
	AbrirWaiting("");
	PostGen(datos, buscarCuentaUrl, function (obj) {
		CerrarWaiting();
		if (obj.error === true) {
			AbrirMensaje("Atención", obj.msg, function () {
				$("#msjModal").modal("hide");
				return true;
			}, false, ["Aceptar"], "error!", null);
		} else if (obj.warn === true) {
			AbrirMensaje("Atención", obj.msg, function () {
				$("#msjModal").modal("hide");
				return true;
			}, false, ["Aceptar"], "warn!", null);
		} else if (obj.unico === true) {
			$("#razonsocial").val(obj.cuenta.cta_Denominacion);
			$("#Cuenta").val(obj.cuenta.cta_Id)
			CargarComboTiposComptes(cuenta);
			return true;
		} else {
			AbrirMensaje("Atención", "Trae una banda.....habilitar el modal para mostar la lista", function () {
				$("#msjModal").modal("hide");
				return true;
			}, false, ["Aceptar"], "warn!", null);
		}
	});
	return true;
}

function ValidarCamposEnComprobantesDeRP(cuenta, tipo, nroCompte, monto) {
	if (cuenta == "") {
		AbrirMensaje("Atención", "Debe seleccionar una cuenta.", function () {
			$("#msjModal").modal("hide");
			$("#Rel01").trigger('focus');
			return true;
		}, false, ["Aceptar"], "error!", null);
		return false;
	}
	if (tipo == "") {
		AbrirMensaje("Atención", "Debe seleccionar un tipo de comprobante.", function () {
			$("#msjModal").modal("hide");
			$("#tco_id").trigger('focus');
			return true;
		}, false, ["Aceptar"], "error!", null);
		return false;
	}
	if (nroCompte == "" || nroCompte.length < 12) {
		AbrirMensaje("Atención", "Debe ingresar un número de comprobante válido.", function () {
			$("#msjModal").modal("hide");
			$("#NroComprobantePtoVta").trigger('focus');
			return true;
		}, false, ["Aceptar"], "error!", null);
		return false;
	}
	if (monto == 0) {
		AbrirMensaje("Atención", "Debe ingresar un monto mayor a 0.", function () {
			$("#msjModal").modal("hide");
			$("#txtMonto").trigger('focus');
			return true;
		}, false, ["Aceptar"], "error!", null);
		return false;
	}
	if (document.getElementById("tbComptesDeRP")) {
		var cantComptes = document.getElementById("tbComptesDeRP").rows.length;
		if (cantComptes >= 7) {
			AbrirMensaje("Atención", "Máximo de 6 comprobantes.", function () {
				$("#msjModal").modal("hide");
				$("#tbComptesDeRP").trigger('focus');
				return true;
			}, false, ["Aceptar"], "error!", null);
			return false;
		}
	}
	return true;
}

function EliminarCompteDeRP() {
	var id = $("#idTipoCompteDeRPSelected").val();
	var nro = $("#nroCompteDeRPSelected").val();
	if (id == "" && nro == "") {
		AbrirMensaje("Atención", "Debe seleccionar un comprobante.", function () {
			$("#msjModal").modal("hide");

			return true;
		}, false, ["Aceptar"], "error!", null);
		return false;
	}
	else {
		eliminarComptesDeRPGrid(id, nro);
		limparCompteDeRPSelected();
	}
}

function limparCompteDeRPSelected() {
	$("#idCompteDeRPSelected").val("");
	$("#nroCompteDeRPSelected").val("");
}

function VerCompteDeRP() {
	AbrirWaiting("");
	var tipo = $("#idTipoCompteDeRPSelected").val();
	var nroComprobante = $("#nroCompteDeRPSelected").val();
	var data = { tipo, nroComprobante };
	PostGen(data, VerDetalleDeCompteDeRPUrl, function (obj) {
		CerrarWaiting();
		return true;
	}, function (obj) {
		CerrarWaiting();
		ControlaMensajeError(obj.message);
		return true;
	});
}

function eliminarComptesDeRPGrid(tipo, nroComprobante) {
	var data = { tipo, nroComprobante };
	AbrirWaiting("");
	PostGenHtml(data, ActualizarComptesDeRPUrl, function (obj) {
		$("#divComptesDeRPGrid").html(obj);
		CerrarWaiting();
		return true;
	}, function (obj) {
		ControlaMensajeError(obj.message);
		CerrarWaiting();
		return true;
	});
}

function comptesDeRPGrid(tipo, tipoDescripcion, nroComprobante, fecha, importe, rp) {
	AbrirWaiting();
	var data = { tipo, tipoDescripcion, nroComprobante, fecha, importe, rp };
	PostGenHtml(data, CargarComptesDeRPUrl, function (obj) {
		$("#divComptesDeRPGrid").html(obj);
		AddEventListenerToComptesGrid();
		console.log("comptesDeRPGrid");
		setTimeout(() => {
			SelecccionarPrimerRegistro("tbComptesDeRP")
		}, 250);
		LimpiarDatosSeleccionadosEnCompte();
		CerrarWaiting();
		return true;
	}, function (obj) {
		ControlaMensajeError(obj.message);
		CerrarWaiting();
		return true;
	});
}

function LimpiarDatosSeleccionadosEnCompte() {
	$("#tco_id").val("");
	$("#NroComprobantePtoVta").val("");
	$("#NroComprobanteNumero").val("");
	$("#dtpFechCompte").val(new Date().toISOString().split("T")[0]);
	$("#txtMonto").val("0");
}

function SelecccionarPrimerRegistro(grilla) {
	var grid = document.getElementById(grilla);
	if (grid) {
		var rowsBody = grid.getElementsByTagName('tbody')[0];
		if (rowsBody && rowsBody.firstElementChild) {
			rowsBody.firstElementChild.className = "selected-row"
			selectCompteDeRPRow(rowsBody.firstElementChild);
		}
	}
}

$("#Rel01").autocomplete({
	source: function (request, response) {

		data = { prefix: request.term }; /*Rel01*/

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

		$("#razonsocial").val(partes[0]);
		$("#Cuenta").val(ui.item.id)
		$("#Rel01List").empty();
		$("#Rel01Item").val(ui.item.id);
		var opc = "<option value=" + ui.item.id + ">" + textoSinSeparador + "</option>"
		$("#Rel01List").append(opc);
		CargarComboTiposComptes(ui.item.id);

		event.preventDefault();
		return true;
	}
});

// 🔥 VALIDACIÓN CRÍTICA: solo asignar _renderItem si la instancia existe
const ac = $("#Rel01").autocomplete("instance");

if (ac) {
	ac._renderItem = function (ul, item) {

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
}

function buscarCuentasComercial() {
	if ($("#CtaId").val() !== "" && $("#CtaId").val() !== undefined) {
		$("#Cuenta").val($("#CtaId").val());
	}
	var cuenta = $("#Cuenta").val();
	if (cuenta === "")
		return false;
	var tipo = tipoCuenta;
	var seccion = seccionEnVista; //-> Aca inyectar el html con los datos 
	var vista = vistaParcial;
	var datos = { cuenta, tipo, vista };
	AbrirWaiting("");
	PostGen(datos, buscarCuentaUrl, function (obj) {
		CerrarWaiting();
		if (obj.error === true) {
			AbrirMensaje("Atención", obj.msg, function () {
				$("#msjModal").modal("hide");
				return true;
			}, false, ["Aceptar"], "error!", null);
		} else if (obj.warn === true) {
			AbrirMensaje("Atención", obj.msg, function () {
				$("#msjModal").modal("hide");
				return true;
			}, false, ["Aceptar"], "warn!", null);
		} else if (obj.unico === true) {
			$("#razonsocial").val(obj.cuenta.cta_Denominacion);
			$("#Cuenta").val(obj.cuenta.cta_Id)
			console.log('Evento disparado desde Busqueda de Cuenta Comercial');
			document.dispatchEvent(new CustomEvent('cc:cuentaChanged', {
				detail: {
					cta_denominacion: obj.cuenta.cta_Denominacion,
					cta_id: obj.cuenta.cta_Id
				}
			 }));
			CargarComboTiposComptes(obj.cuenta.cta_Id);
			return true;
		} else {
			MostrarModalCuentasComerciales(obj.cuenta);
		}
	});
	return true;
}

var ctas = [];

function MostrarModalCuentasComerciales(lista) {
	if (lista.length > 0) {
		AbrirWaiting();
		$("#tbCuentasComerciales tbody tr").remove();
		var tableBody = document.getElementById("tbCuentasComerciales").getElementsByTagName('tbody')[0];
		ctas = [];
		for (var i = 0; i < lista.length; i++) {
			var row = tableBody.insertRow();
			var celda0 = row.insertCell(0);
			var celda1 = row.insertCell(1);
			var celda2 = row.insertCell(2);
			var celda3 = row.insertCell(3);
			var celda4 = row.insertCell(4);
			celda0.innerText = lista[i].cta_Id;
			celda1.innerText = lista[i].cta_Denominacion;
			celda2.innerText = lista[i].tdoc_Desc + " " + lista[i].cta_Documento;
			if (lista[i].prov_Id === "C")
				celda3.innerHTML = '<i class="bx bx-user bx-md text-success" id="iconTipoCC"></i>';
			else if (lista[i].prov_Id === "B")
				celda3.innerHTML = '<i class="bx bx-user bx-md text-info" id="iconTipoCC"></i>';
			else if (lista[i].prov_Id === "P")
				celda3.innerHTML = '<i class="bx bx-user bx-md text-danger" id="iconTipoCC"></i>';
			else
				celda3.innerHTML = '<i class="bx bx-user bx-md text-secondary" id="iconTipoCC"></i>';
			celda4.innerText = lista[i].prov_Id;
			celda0.className = "align-center";
			celda1.className = "align-left";
			celda2.className = "align-center";
			celda3.className = "align-center";
			celda4.className = "align-center";
			celda4.hidden = true;
			ctas.push(lista[i].cta_Denominacion);
		}
		AddEventListenerToGrid("tbCuentasComerciales");
		addRowHandlers("tbCuentasComerciales");
		autocomplete(document.getElementById("razonSocialEnModalDeBusqueda"), ctas);
		$('#modalCC').modal('show')
		CerrarWaiting();
	}
}

function MostrarOcultarRowsEnModalDeBusquedaDeCC(texto) {
	var tableBody = document.getElementById("tbCuentasComerciales").getElementsByTagName('tbody')[0];
	for (var i = 0; i < tableBody.rows.length; i++) {
		if (texto.toUpperCase() == tableBody.rows[i].cells[1].innerText.toUpperCase()) {
			tableBody.rows[i].style.display = '';
		}
		else if (tableBody.rows[i].cells[1].innerText.includes(texto.toUpperCase())) {
			tableBody.rows[i].style.display = '';
		}
		else {
			tableBody.rows[i].style.display = 'none';
		}
	}
}

function addRowHandlers(table) {
	var table = document.getElementById(table);
	var rows = table.getElementsByTagName("tr");
	for (i = 0; i < rows.length; i++) {
		var currentRow = table.rows[i];
		var createClickHandler = function (row) {
			return function () {
				var cell = row.getElementsByTagName("td")[0];
				idCuentaSeleccionada = cell.innerHTML;
				cell = row.getElementsByTagName("td")[1];
				razonSocialSeleccionada = cell.innerHTML;
				cell = row.getElementsByTagName("td")[4];
				provIdSeleccionado = cell.innerHTML;
			};
		};
		currentRow.onclick = createClickHandler(currentRow);
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
		});
	}
}

function seleccionarCuentaComercial() {
	if (idCuentaSeleccionada !== "") {
		$("#razonsocial").val(razonSocialSeleccionada);
		$("#Cuenta").val(idCuentaSeleccionada);
		CargarComboTiposComptes(idCuentaSeleccionada);
		InicializaPantallaCC(provIdSeleccionado);
		ActualizarCuentaComercialSeleccionada(idCuentaSeleccionada);
	}
	$('#modalCC').modal('hide')
}

function ActualizarCuentaComercialSeleccionada(ctaId) {
	var tipo = tipoCuenta;
	var datos = { ctaId, tipo }
	AbrirWaiting("");
	PostGen(datos, ActualizarCuentaComercialSeleccionadaUrl, function (obj) {
		CerrarWaiting();
		if (obj.error === true) {
			AbrirMensaje("Atención", obj.msg, function () {
				$("#msjModal").modal("hide");
				return true;
			}, false, ["Aceptar"], "error!", null);
		} else if (obj.warn === true) {
			AbrirMensaje("Atención", obj.msg, function () {
				$("#msjModal").modal("hide");
				return true;
			}, false, ["Aceptar"], "warn!", null);
		} else if (obj.unico === true) {
			//$("#razonsocial").val(obj.cuenta.cta_Denominacion);
			//CargarComboTiposComptes(cuenta);
			return true;
		} else {
		}
	});
}

function CargarComboTiposComptes(cuenta) {
	if (buscarTiposComptesUrl === "") {
		return false;
	}
	AbrirWaiting("");
	var datos = { cuenta };
	PostGenHtml(datos, buscarTiposComptesUrl, function (obj) {
		CerrarWaiting();
		$("#divTiposComptes").html(obj);
		console.log("CargarComboTiposComptes");
		$("#tco_id").on("change", analizaInputlistaTco);
		CerrarWaiting();
		return true
	})
}


function selectCompteDeRPRow(x) {
	//$("#txtNroCompte").val(x.cells[2].innerText.trim());
	const valor = x.cells[2].innerText.trim(); // Ej: "0001-000123456"
	// Dividir por el guion
	const partes = valor.split('-');
	// Asignar a los inputs
	//$("#NroComprobantePtoVta").val(partes[0] || "");
	//$("#NroComprobanteNumero").val(partes[1] || "");

	var monto = x.cells[4].innerText.trim();
	// if (monto.includes(",")) {
	// 	$("#txtMonto").val(x.cells[4].innerText.trim().replace(".", ""));
	// } else {
	// 	$("#txtMonto").val(x.cells[4].innerText.trim().replace(".", ","));
	// }

	//$("#tco_id").val(x.cells[0].innerText.trim());
	//$("#tco_id").trigger("change");
	//$("#idTipoCompteDeRPSelected").val(x.cells[0].innerText.trim());
	//$("#nroCompteDeRPSelected").val(x.cells[2].innerText.trim());
	//$("#fechaCompteDeRPSelected").val(x.cells[3].innerText.trim());
	//$("#descTipoCompteDeRPSelected").val(x.cells[1].innerText.trim());
	//var tipoCompte = x.cells[0].innerText.trim();
	var obj = obtenerCompteSeleccionado(x);
	var tipoCompte = obj.tipoId;
	var nroCompte = obj.comprobante.completo;
	var monto = obj.importe;
	var descTipoCompte = obj.tipoDescripcion;

	//var descTipoCompte = x.cells[1].innerText.trim();
	//var nroCompte = x.cells[2].innerText.trim();
	//var monto = x.cells[4].innerText.trim();
	var fechaCompte = moment($("#fechaCompteDeRPSelected").val()).format("X");
	var depoSelec = $("#listaDeposito").val();
	var notaAuto = $("#txtNota").val();
	var turno = moment($("#dtpFechaTurno").val()).format("X");
	var ponerEnCurso = $("#chkPonerEnCurso")[0].checked;
	var ul = $("#txtCantidadUL").val();
	var rp = $("#Rp").val();
	var cta = $("#Cuenta").val();
	var link = VerDetalleDeCompteDeRPUrl + "?idTipoCompte=" + tipoCompte + "&nroCompte=" + nroCompte + "&depoSelec=" + depoSelec + "&notaAuto=" + notaAuto + "&turno=" + turno + "&ponerEnCurso=" + ponerEnCurso + "&ulCantidad=" + ul + "&rp=" + rp + "&ctaId=" + cta + "&tipoCuenta=" + tipoCuenta + "&fechaCompte=" + fechaCompte + "&monto=" + monto + "&descTipoCompte=" + descTipoCompte;
	$("#VerDetalle").prop("href", link);
}

function obtenerCompteSeleccionado(row) {
	const $row = $(row).find("td");

	// Campos según tu DOM
	const tipoId = $row.eq(0).text().trim();   // oculto
	const tipoDescripcion = $row.eq(1).text().trim();
	const comprobanteFull = $row.eq(2).text().trim();   // ej: "0001-00000001"
	const fecha = $row.eq(3).text().trim();
	const importe = $row.eq(4).text().trim();

	// Separar comprobante
	let ptoVta = "";
	let numero = "";

	if (comprobanteFull.includes("-")) {
		const partes = comprobanteFull.split("-");
		ptoVta = partes[0];
		numero = partes[1];
	}

	// Armar JSON final
	const data = {
		tipoId,
		tipoDescripcion,
		comprobante: {
			completo: comprobanteFull,
			puntoVenta: ptoVta,
			numero: numero
		},
		fecha,
		importe
	};

	console.log("Datos seleccionados:", data);

	return data;
}

function obtenerCompteDeRPSeleccionado() {

	// Buscar la fila seleccionada
	const $row = $("#tbComptesDeRP tbody tr.selected-row");

	// Si no hay fila seleccionada, devolver null
	if ($row.length === 0) {
		return null;
	}

	const tds = $row.find("td");

	// Extraer valores
	const tipoId = tds.eq(0).text().trim();   // oculto
	const tipoDescripcion = tds.eq(1).text().trim();
	const comprobanteFull = tds.eq(2).text().trim();   // ej: "0001-00000001"
	const fecha = tds.eq(3).text().trim();
	const importe = tds.eq(4).text().trim();

	// Separar comprobante
	let puntoVenta = "";
	let numero = "";

	if (comprobanteFull.includes("-")) {
		const partes = comprobanteFull.split("-");
		puntoVenta = partes[0];
		numero = partes[1];
	}

	// Armar JSON final
	return {
		tipoId,
		tipoDescripcion,
		comprobante: {
			completo: comprobanteFull,
			puntoVenta,
			numero
		},
		fecha,
		importe
	};
}


function autocomplete(inp, arr) {
	/*the autocomplete function takes two arguments,
	the text field element and an array of possible autocompleted values:*/
	var currentFocus;
	/*execute a function when someone writes in the text field:*/
	inp.addEventListener("input", function (e) {
		var a, b, i, val = this.value;
		/*close any already open lists of autocompleted values*/
		closeAllLists();
		if (!val) {
			MostrarOcultarRowsEnModalDeBusquedaDeCC(val);
			return false;
		}
		currentFocus = -1;
		/*create a DIV element that will contain the items (values):*/
		a = document.createElement("DIV");
		a.setAttribute("id", this.id + "autocomplete-list");
		a.setAttribute("class", "autocomplete-items");
		/*append the DIV element as a child of the autocomplete container:*/
		this.parentNode.appendChild(a);
		/*for each item in the array...*/
		for (i = 0; i < arr.length; i++) {
			/*check if the item starts with the same letters as the text field value:*/
			if (arr[i].substr(0, val.length).toUpperCase() == val.toUpperCase()) {
				/*create a DIV element for each matching element:*/
				b = document.createElement("DIV");
				/*make the matching letters bold:*/
				b.innerHTML = "<strong>" + arr[i].substr(0, val.length) + "</strong>";
				b.innerHTML += arr[i].substr(val.length);
				/*insert a input field that will hold the current array item's value:*/
				b.innerHTML += "<input type='hidden' value='" + arr[i] + "'>";
				/*execute a function when someone clicks on the item value (DIV element):*/
				b.addEventListener("click", function (e) {
					/*insert the value for the autocomplete text field:*/
					inp.value = this.getElementsByTagName("input")[0].value;
					/*close the list of autocompleted values,
					(or any other open lists of autocompleted values:*/
					closeAllLists();
					MostrarOcultarRowsEnModalDeBusquedaDeCC(inp.value);
				});
				a.appendChild(b);
			}
		}
		MostrarOcultarRowsEnModalDeBusquedaDeCC(val);
	});
	/*execute a function presses a key on the keyboard:*/
	inp.addEventListener("keydown", function (e) {
		var x = document.getElementById(this.id + "autocomplete-list");
		if (x) x = x.getElementsByTagName("div");
		if (e.keyCode == 40) {
			/*If the arrow DOWN key is pressed,
			increase the currentFocus variable:*/
			currentFocus++;
			/*and and make the current item more visible:*/
			addActive(x);
		} else if (e.keyCode == 38) { //up
			/*If the arrow UP key is pressed,
			decrease the currentFocus variable:*/
			currentFocus--;
			/*and and make the current item more visible:*/
			addActive(x);
		} else if (e.keyCode == 13) {
			/*If the ENTER key is pressed, prevent the form from being submitted,*/
			e.preventDefault();
			if (currentFocus > -1) {
				/*and simulate a click on the "active" item:*/
				if (x) x[currentFocus].click();
			}
		}
	});
	function addActive(x) {
		/*a function to classify an item as "active":*/
		if (!x) return false;
		/*start by removing the "active" class on all items:*/
		removeActive(x);
		if (currentFocus >= x.length) currentFocus = 0;
		if (currentFocus < 0) currentFocus = (x.length - 1);
		/*add class "autocomplete-active":*/
		x[currentFocus].classList.add("autocomplete-active");
	}
	function removeActive(x) {
		/*a function to remove the "active" class from all autocomplete items:*/
		for (var i = 0; i < x.length; i++) {
			x[i].classList.remove("autocomplete-active");
		}
	}
	function closeAllLists(elmnt) {
		/*close all autocomplete lists in the document,
		except the one passed as an argument:*/
		var x = document.getElementsByClassName("autocomplete-items");
		for (var i = 0; i < x.length; i++) {
			if (elmnt != x[i] && elmnt != inp) {
				x[i].parentNode.removeChild(x[i]);
			}
		}
	}
	/*execute a function when someone clicks in the document:*/
	document.addEventListener("click", function (e) {
		closeAllLists(e.target);
	});
}