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
	$("#btnBuscarCC").on("click", buscarCuentasComercial);
	InicializaPantallaCC();
	comptesDeRPGrid();
	$("#btnNuevoCompteDeRP").on("click", NuevoCompteDeRP);
	$("#btnEliminarCompteDeRP").on("click", EliminarCompteDeRP);
	$("#btnRegresarASelAuto").on("click", RegresarASelAuto); //Regregar a la pantalla de seleccion de autorizaciones.

	$("#txtNroCompte").mask("0000-00000000", { reverse: true });


});

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
			AbrirMensaje("Atención", o.msg, function () {
				$("#msjModal").modal("hide");
				switch (e) {
					//TODO
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
			}, false, ["Aceptar", "Cancelar"], "info!", null);
		} else {
			//Redireccionar a la pagina anterior
		}
	});
}

function GuardarDetalleDeProductos(guardado) {
	datos = { guardado };
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
			//Volver a la pantalla anterior
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
		comptesDeRPGrid($("#tco_id").val(), $("#tco_id")[0].selectedOptions[0].text, $("#txtNroCompte").val(), $("#dtpFechCompte").val(), $("#txtMonto").val());
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
	var cantComptes = document.getElementById("tbComptesDeRP").rows.length;
	if (cantComptes >= 7) {
		AbrirMensaje("Atención", "Máximo de 6 comprobantes.", function () {
			$("#msjModal").modal("hide");

			return true;
		}, false, ["Aceptar"], "error!", null);
		return false;
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

function comptesDeRPGrid(tipo, tipoDescripcion, nroComprobante, fecha, importe) {
	var data = { tipo, tipoDescripcion, nroComprobante, fecha, importe };
	PostGenHtml(data, CargarComptesDeRPUrl, function (obj) {
		$("#divComptesDeRPGrid").html(obj);
		return true;
	}, function (obj) {
		ControlaMensajeError(obj.message);
		return true;
	});
}

function buscarCuentasComercial() {
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
