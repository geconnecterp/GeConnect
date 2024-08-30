$(document).ready(function () {
	const dateControl = document.querySelector('input[type="date"]');
	const dateControl2 = $('input[type="date"]');
	var local = moment().format('yyyy-MM-DD');
	var twoMonthPast = moment().add(-2, 'months')
	for (var i = 0; i < dateControl2.length; i++) {
		dateControl2[i].value = local;
		if (dateControl2[i].id = "dtpFechCompte") {
			dateControl2[i].setAttribute('min', twoMonthPast.format('yyyy-MM-DD'));
		}
	}
	dateControl.value = local;
	$("#Cuenta").on("click", inicializaCuenta);
	$("#btnBuscarCC").on("click", buscarCuentasComercial);
	InicializaPantallaCC();
	comptesDeRPGrid();
	$("#btnNuevoCompteDeRP").click(NuevoCompteDeRP);
	$("#btnEliminarCompteDeRP").click(EliminarCompteDeRP);
	$("#btnVerCompteDeRP").click(VerCompteDeRP);

	$("#txtNroCompte").mask("0000-00000000", { reverse: true });


});

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

function inicializaCuenta() {
	$("#razonsocial").val("");
	$("#Cuenta").val("").focus();

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
}

function VerCompteDeRP() {
}

function comptesDeRPGrid(tipo, tipoDescripcion, nroComprobante, fecha, importe) {
	//document.getElementById("tbComptesDeRP").addEventListener('click', function (e) {
	//	if (e.target.nodeName === 'TD') {
	//		var selectedRow = this.querySelector('.selected-row');
	//		if (selectedRow) {
	//			selectedRow.classList.remove('selected-row');
	//		}
	//		e.target.closest('tr').classList.add('selected-row');
	//	}
	//});


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
