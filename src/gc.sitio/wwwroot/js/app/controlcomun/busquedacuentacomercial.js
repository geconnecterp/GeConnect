﻿$(function () {
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
	dateControl.value = local;
	$("#dtpFechaTurno").val(local);
	$("#btnBuscarCC").on("click", buscarCuentasComercial);
	InicializaPantallaCC();
	$("#VerDetalle").on("click", VerDetalleClick);
	$("#btnNuevoCompteDeRP").on("click", NuevoCompteDeRP);
	$("#btnEliminarCompteDeRP").on("click", EliminarCompteDeRP);
	$("#btnRegresarASelAuto").on("click", RegresarASelAuto); //Regregar a la pantalla de seleccion de autorizaciones.
	$("#btnAceptarAutoRP").on("click", AceptarAutoRP); //Generar Json de comprobante RPR
	$("#btnEliminarAutoRP").on("click", EliminarAutoRP); //Eliminar RPR cargado

	$("#txtNroCompte").mask("0000-00000000", { reverse: true });

	$("#Cuenta").on("keyup", analizaInput);
	$("#txtNota").on("keyup", analizaInputTxtNota);
	$("#listaDeposito").on("change", analizaInputlistaDeposito);
	$("#dtpFechaTurno").on("change", analizaInputTurno);
	$("#chkPonerEnCurso").on("change", analizaInputChkPonerEnCurso);
	$("#txtCantidadUL").on("keyup", analizaInputCantidadUL);

	if ($("#CtaId").val() !== "") {
		$("#btnBuscarCC").trigger("click");
	}
	if ($("#DepoId").val() !== "") {
		$("#listaDeposito").val($("#DepoId").val());
	}
	CargarCompteEnGrilla(modelObj);
	//if ($("#FechaTurno").val() !== "") {
	//	$("#dtpFechaTurno").val($("#FechaTurno").val())

	//}
	const cantUL = document.getElementById("txtCantidadUL");
	cantUL.addEventListener('input', function (e) {
		if (!isValid(this.value))
			cantUL.value = 999;
	});

	AddEventListenerToComptesGrid();
});

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

function analizaInput(e) {
	$("#CtaId").val("");
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
	var depoSelec = $("#listaDeposito").val();
	var notaAuto = $("#txtNota").val();
	var turno = moment($("#dtpFechaTurno").val()).format("X");
	var ponerEnCurso = $("#chkPonerEnCurso")[0].checked;
	var ul = $("#txtCantidadUL").val();
	var rp = $("#Rp").val();
	var tipoCompte = $("#idTipoCompteDeRPSelected").val();
	var nroCompte = $("#txtNroCompte").val();
	var rp = $("#Rp").val();
	var fechaCompte = moment($("#fechaCompteDeRPSelected").val()).format("X");
	var monto = $("#txtMonto").val();
	var descTipoCompte = $("#descTipoCompteDeRPSelected").val();
	var cta = $("#Cuenta").val();
	var link = VerDetalleDeCompteDeRPUrl + "?idTipoCompte=" + tipoCompte + "&nroCompte=" + nroCompte + "&depoSelec=" + depoSelec + "&notaAuto=" + notaAuto + "&turno=" + turno + "&ponerEnCurso=" + ponerEnCurso + "&ulCantidad=" + ul + "&rp=" + rp + "&ctaId=" + cta + "&tipoCuenta=" + tipoCuenta + "&fechaCompte=" + fechaCompte + "&monto=" + monto + "&descTipoCompte" + descTipoCompte;
	$("#VerDetalle").prop("href", link);
}

function CargarCompteEnGrilla(obj) {
	if (obj != null) {
		console.log("CargarCompteEnGrilla (135): " + obj.fechaTurno.substring(0, 10));
		$("#txtCantidadUL").val(obj.cantidadUL);
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
}

function EliminarComprobanteRPR(rp) {
	datos = { rp };
	PostGen(datos, eliminarComprobanteRPRUrl, function (o) {
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
		} else if (o.codigo !== "") {
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
	GuardarDetalleDeProductos(true);
}

function RegresarASelAuto() {

	//Antes de volver debo validar si hay productos cargados en el detalle, si es así consultar con el operador
	datos = {};
	PostGen(datos, verificarDetalleCargadoURL, function (o) {
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
	var ulCantidad = $("#txtCantidadUL").val();
	var fechaTurno = $("#dtpFechaTurno").val();
	var depoId = $("#listaDeposito").val();
	var nota = $("#txtNota").val();
	var generar = true;
	datos = { guardado, generar, ulCantidad, fechaTurno, depoId, nota };
	PostGen(datos, GuardarDetalleDeComprobanteRPUrl, function (o) {
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
			window.location.href = volverAListaDeAutorizacionesUrl;
		}
	});
}

function InicializaPantallaCC() {
	switch (tipoCuenta) {
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
	var resultValidation = ValidarCamposEnComprobantesDeRP($("#Cuenta").val(), $("#tco_id").val(), $("#txtNroCompte").val(), $("#txtMonto").val());
	if (!resultValidation) {
		return;
	}
	else {
		console.log("NuevoCompteDeRP (269): " + $("#Rp").val());
		comptesDeRPGrid($("#tco_id").val(), $("#tco_id")[0].selectedOptions[0].text, $("#txtNroCompte").val(), $("#dtpFechCompte").val(), $("#txtMonto").val(), $("#Rp").val());
	}
}

function AgregarComprobante() {
	PostGen(datos, buscarCuentaUrl, function (obj) {
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
			return true;
		}, false, ["Aceptar"], "error!", null);
		return false;
	}
	if (tipo == "") {
		AbrirMensaje("Atención", "Debe seleccionar un tipo de comprobante.", function () {
			$("#msjModal").modal("hide");
			return true;
		}, false, ["Aceptar"], "error!", null);
		return false;
	}
	if (nroCompte == "") {
		AbrirMensaje("Atención", "Debe ingresar un número de comprobante válido.", function () {
			$("#msjModal").modal("hide");
			return true;
		}, false, ["Aceptar"], "error!", null);
		return false;
	}
	if (nroCompte.length < 12) {
		AbrirMensaje("Atención", "Debe ingresar un número de comprobante válido.", function () {
			$("#msjModal").modal("hide");
			return true;
		}, false, ["Aceptar"], "error!", null);
		return false;
	}
	if (monto == 0) {
		AbrirMensaje("Atención", "Debe ingresar un monto mayor a 0.", function () {
			$("#msjModal").modal("hide");

			return true;
		}, false, ["Aceptar"], "error!", null);
		return false;
	}
	if (document.getElementById("tbComptesDeRP")) {
		var cantComptes = document.getElementById("tbComptesDeRP").rows.length;
		if (cantComptes >= 7) {
			AbrirMensaje("Atención", "Máximo de 6 comprobantes.", function () {
				$("#msjModal").modal("hide");

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
	var tipo = $("#idTipoCompteDeRPSelected").val();
	var nroComprobante = $("#nroCompteDeRPSelected").val();
	var data = { tipo, nroComprobante };
	PostGen(data, VerDetalleDeCompteDeRPUrl, function (obj) {
		return true;
	}, function (obj) {
		ControlaMensajeError(obj.message);
		return true;
	});
}

function eliminarComptesDeRPGrid(tipo, nroComprobante) {
	var data = { tipo, nroComprobante };
	PostGenHtml(data, ActualizarComptesDeRPUrl, function (obj) {
		$("#divComptesDeRPGrid").html(obj);
		return true;
	}, function (obj) {
		ControlaMensajeError(obj.message);
		return true;
	});
}

function comptesDeRPGrid(tipo, tipoDescripcion, nroComprobante, fecha, importe, rp) {
	AbrirWaiting();
	var data = { tipo, tipoDescripcion, nroComprobante, fecha, importe, rp };
	PostGenHtml(data, CargarComptesDeRPUrl, function (obj) {
		$("#divComptesDeRPGrid").html(obj);
		AddEventListenerToComptesGrid();
		CerrarWaiting();
		return true;
	}, function (obj) {
		ControlaMensajeError(obj.message);
		CerrarWaiting();
		return true;
	});
}

function buscarCuentasComercial() {
	if ($("#CtaId").val() !== "") {
		$("#Cuenta").val($("#CtaId").val());
	}
	var cuenta = $("#Cuenta").val();
	var tipo = tipoCuenta;
	var seccion = seccionEnVista; //-> Aca inyectar el html con los datos 
	var vista = vistaParcial;
	var datos = { cuenta, tipo, vista };

	PostGen(datos, buscarCuentaUrl, function (obj) {
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

function CargarComboTiposComptes(cuenta) {
	var datos = { cuenta };
	PostGenHtml(datos, buscarTiposComptesUrl, function (obj) {
		$("#divTiposComptes").html(obj);
		CerrarWaiting();
		return true
	})
}


function selectCompteDeRPRow(x) {
	$("#txtNroCompte").val(x.cells[2].innerText.trim());
	$("#txtMonto").val(x.cells[4].innerText.trim());
	$("#tco_id").val(x.cells[0].innerText.trim());
	$("#idTipoCompteDeRPSelected").val(x.cells[0].innerText.trim());
	$("#nroCompteDeRPSelected").val(x.cells[2].innerText.trim());
	$("#fechaCompteDeRPSelected").val(x.cells[3].innerText.trim());
	$("#descTipoCompteDeRPSelected").val(x.cells[1].innerText.trim());
	var tipoCompte = x.cells[0].innerText.trim();
	var descTipoCompte = x.cells[1].innerText.trim();
	var nroCompte = x.cells[2].innerText.trim();
	var monto = x.cells[4].innerText.trim();
	var fechaCompte = moment($("#fechaCompteDeRPSelected").val()).format("X");
	var depoSelec = $("#listaDeposito").val();
	var notaAuto = $("#txtNota").val();
	var turno = moment($("#dtpFechaTurno").val()).format("X");
	var ponerEnCurso = $("#chkPonerEnCurso")[0].checked;
	var ul = $("#txtCantidadUL").val();
	var rp = $("#Rp").val();
	var cta = $("#Cuenta").val();
	var link = VerDetalleDeCompteDeRPUrl + "?idTipoCompte=" + tipoCompte + "&nroCompte=" + nroCompte + "&depoSelec=" + depoSelec + "&notaAuto=" + notaAuto + "&turno=" + turno + "&ponerEnCurso=" + ponerEnCurso + "&ulCantidad=" + ul + "&rp=" + rp + "&ctaId=" + cta + "&tipoCuenta=" + tipoCuenta + "&fechaCompte=" + fechaCompte + "&monto=" + monto + "&descTipoCompte" + descTipoCompte;
	$("#VerDetalle").prop("href", link);
}