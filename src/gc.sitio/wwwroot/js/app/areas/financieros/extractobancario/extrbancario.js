var fecha_extracto;
$(function () {
	InicializarCamposEnFiltros();
	$(document).on("change", "#listaCuentaBanco", ControlalistaCuentaBancoSelected);
	$("#FechaDesde, #FechaHasta").on("change", validarFechas);

	$("#btnBuscar").on("click", function () {
		ctafIdSelected = $("#listaCuentaBanco").val();
		if (ctafIdSelected == "") {
			AbrirMensaje("ATENCIÓN", "Debe seleccionar una cuenta banco.", function () {
				$("#msjModal").modal("hide");
				$("#listaCuentaBanco").trigger("focus");
				return true;
			}, false, ["Aceptar"], "error!", null);
		}
		else {
			ctafDenominacionSelected = $("#listaCuentaBanco option:selected").text();
			ControlaCargarExtractoBancarioClick();
		}
	});
});

function ControlalistaCuentaBancoSelected() {
	var ctaf_id = $("#listaCuentaBanco").val();
	var data = { ctaf_id };
	PostGen(data, obtenerCuentaBancoUrl, function (obj) {
		CerrarWaiting();
		if (obj.error === true) {
			CerrarWaiting();
			AbrirMensaje("ATENCIÓN", obj.msg, function () {
				$("#msjModal").modal("hide");
				return true;
			}, false, ["Aceptar"], "error!", null);
		}
		else {
			CerrarWaiting();
			EstableceValoresDeFechas(obj.ext_fecha);
		}
	});
}

function ControlaCargarExtractoBancarioClick() {
	AbrirWaiting();
	var ctaf_id = $("#listaCuentaBanco").val();
	var FechaDesde = $("#FechaDesde").val();
	var FechaHasta = $("#FechaHasta").val();
	var data = { ctaf_id, FechaDesde, FechaHasta };
	PostGenHtml(data, cargarExtractoBancarioURL, function (obj) {
		CerrarWaiting();
		$("#divExtractoBco").html(obj);
		$("#divFiltros").removeClass("show").addClass("collapse");
		$("#divDetalle").collapse("show");
		return true
	}, function (obj) {
		ControlaMensajeError(obj.message);
		CerrarWaiting();
	});
}

function EstableceValoresDeFechas(fecha) {
	console.log(fecha);
	fecha_extracto = fecha;
	if (fecha && typeof fecha === "string" && moment(fecha, moment.ISO_8601, true).isValid()) {
		const fechaMoment = moment(fecha);
		console.log("Fecha válida:", fechaMoment.format("YYYY-MM-DD HH:mm:ss"));

		const fechaMinima = moment(fecha).format("YYYY-MM-DD");
		$("#FechaDesde").attr("min", fechaMinima);
		$("#FechaHasta").attr("min", fechaMinima)

	} else {
		console.warn("Fecha inválida o no definida");
	}
}

function validarFechas() {
	const $desde = $("#FechaDesde");
	const $hasta = $("#FechaHasta");

	const desde = $desde.val();
	const hasta = $hasta.val();

	if (!desde || !hasta) return;

	const mDesde = moment(desde, "YYYY-MM-DD");
	const mHasta = moment(hasta, "YYYY-MM-DD");
	const mMinima = moment(fecha_extracto, "YYYY-MM-DD");
	var now = moment().format('yyyy-MM-DD');

	// Si alguna fecha es menor a la mínima → setear ambas
	if (mDesde.isBefore(mMinima) || mHasta.isBefore(mMinima)) {
		$desde.val(now);
		$hasta.val(now);
		AbrirMensaje("ATENCIÓN", `Las fechas no pueden ser menor a la fecha del extracto (${mMinima})`, function () {
			$("#msjModal").modal("hide");
			return true;
		}, false, ["Aceptar"], "error!", null);
		return;
	}
}

function eliminarItem(ctaf_id, extr_id, ext_concepto) {
	console.log("Ver detalle de cheque:", ctaf_id);
	// Lógica para quitar elemento en el BE y actualizar la vista
	
}


function InicializarCamposEnFiltros() {
	var now = moment().format('yyyy-MM-DD');
	$("#FechaDesde").val(now);
	$("#FechaHasta").val(now);
	$("#divFiltros").removeClass("collapse").addClass("show");
	$("#lbCuentaBanco").text("Cuenta Banco");
	$("#lbFecha").text("Fecha");
	$("#lbCargar").text("Cargar");
	$("#chkFecha").on("click", function () {
		if ($("#chkFecha").is(":checked")) {
			$("#FechaDesde").prop("disabled", false);
			$("#FechaHasta").prop("disabled", false);
			$("#FechaDesde").trigger("focus");
		}
		else {
			$("#FechaDesde").prop("disabled", true);
			$("#FechaHasta").prop("disabled", true);
		}
	});
	$("#chkCuentaBanco").on("click", function () {
		if ($("#chkCuentaBanco").is(":checked")) {
			$("#listaCuentaBanco").prop("disabled", false);
			$("#listaCuentaBanco").trigger("focus");
		}
		else {
			$("#listaCuentaBanco").prop("disabled", true);
		}
	});
	$("#btnFiltro").on("click", function () {
		if ($("#divFiltros").hasClass("show")) {
			$("#divFiltros").collapse("hide");
		}
		else {
			$("#divFiltros").collapse("show");
		}
	});
	$("#FechaDesde, #FechaHasta, #listaCuentaBanco").prop("disabled", false);
	$("#chkCuentaBanco").prop('checked', true);
	$("#chkCuentaBanco").trigger("change");
	$("#chkCuentaBanco").prop("disabled", true);
	$("#chkFecha").prop('checked', true);
	$("#chkFecha").trigger("change");
	$("#chkFecha").prop("disabled", true);
	$("#btnCancel").on("click", function () {
		btnCancelarClick();
	});
}

function btnCancelarClick() {
	$("#listaCuentaBanco").val("");

	InicializarDatosEnSesion();
}

function InicializarDatosEnSesion() {
}