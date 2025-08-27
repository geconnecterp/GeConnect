$(function () {
	InicializarCampos();
	$(document).on("click", "#btnBuscar", btnBuscarValidar);
	$(document).on("change", "#ListaCuentaBancaria", ControlaListaCuentaBancariaSelected);
	$(document).on("click", "#btnCancel", btnCancelValidar);
});

function btnCancelValidar() {
	var data = {};
	PostGenHtml(data, volverPasoUnoUrl, function (obj) {
		$("#divPrincipal").html(obj);
		InicializarCampos();
		$(document).on("click", "#btnCancel", btnCancelValidar);
	});
}

function btnBuscarValidar() {
	if (CuentaBancariaSelected == "") {
		AbrirMensaje("ATENCIÓN", "Debe seleccionar una cuenta bancaria.", function () {
			$("#msjModal").modal("hide");
			$("#ListaCuentaBancaria").trigger("focus");
			return true;
		}, false, ["Aceptar"], "error!", null);
	}
	else {
		var fechaDesde = $("#fechaDesde").val();
		var fechaHasta = $("#fechaHasta").val();
		if (fechaDesde == "" || fechaHasta == "") {
			AbrirMensaje("ATENCIÓN", "Debe ingresar ambas fechas.", function () {
				$("#msjModal").modal("hide");
				if (fechaDesde == "") {
					$("#fechaDesde").trigger("focus");
				}
				else {
					$("#fechaHasta").trigger("focus");
				}
				return true;
			}, false, ["Aceptar"], "error!", null);
		}
		else {
			if (moment(fechaDesde).isAfter(moment(fechaHasta))) {
				AbrirMensaje("ATENCIÓN", "La fecha 'Desde' no puede ser mayor a la fecha 'Hasta'.", function () {
					$("#msjModal").modal("hide");
					$("#fechaDesde").trigger("focus");
					return true;
				}, false, ["Aceptar"], "error!", null);
			}
			else {
				var data = { ctaf_id: CuentaBancariaSelected, fechaDesde, fechaHasta };
				PostGenHtml(data, buscarChequesDepositadosUrl, function (obj) {
					$("#divPrincipal").html(obj);
					EstablecerValoresLimites();
					$(document).on("click", "#btnCancel", btnCancelValidar);
				});
			}
		}
	}
}

function ControlaListaCuentaBancariaSelected() {
	CuentaBancariaSelected = $("#ListaCuentaBancaria").val();
}

function EstablecerValoresLimites() {
	var now = moment().format('yyyy-MM-DD');
	var now2 = moment().subtract(30, 'days');
	$("#fechaRechazado").attr('min', now2.format('yyyy-MM-DD'));
	$("#fechaRechazado").attr('max', now);
	$("#fechaRechazado").val(now);
}

function InicializarCampos() {
	var now = moment().format('yyyy-MM-DD');
	$("#fechaHasta").val(now);
	var now2 = moment().subtract(7, 'days');
	$("#fechaDesde").val(now2.format('yyyy-MM-DD'));
}

function onChangeFechaRechazado(x) {

}

function onChangeFechaDesde(x) {
	ValidarFechasEnIndex();
}

function onChangeFechaHasta(x) {
	ValidarFechasEnIndex();
}

function ValidarFechasEnIndex() {
	var fechaDesde = $("#fechaDesde").val();
	var fechaHasta = $("#fechaHasta").val();
	if (fechaDesde == "" || fechaHasta == "") {
		AbrirMensaje("ATENCIÓN", "Debe ingresar ambas fechas.", function () {
			$("#msjModal").modal("hide");
			if (fechaDesde == "") {
				$("#fechaDesde").trigger("focus");
			}
			else {
				$("#fechaHasta").trigger("focus");
			}
			return true;
		}, false, ["Aceptar"], "error!", null);
	}
	else {
		if (moment(fechaDesde).isAfter(moment(fechaHasta))) {
			AbrirMensaje("ATENCIÓN", "La fecha 'Desde' no puede ser mayor a la fecha 'Hasta'.", function () {
				$("#msjModal").modal("hide");
				$("#fechaDesde").trigger("focus");
				return true;
			}, false, ["Aceptar"], "error!", null);
		}
	}
}