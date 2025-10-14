$(function () {
	InicializarCamposEnFiltros();
	$(document).on("click", "#btnConfirmar", ConfirmarConciliacionExtracto);
	$(document).on("click", "#btnCancelar", CancelarConciliacionExtracto);
	$(document).on("click", "#btnDesconciliar", DesconciliarRegistrosConciliados);
	//

	$("#FechaDesde, #FechaHasta").on("blur", validarFechas);

	$("#btnFiltro").on("click", function () {
		if ($("#divFiltros").hasClass("show")) {
			$("#divFiltros").collapse("hide");
		}
		else {
			$("#divFiltros").collapse("show");
		}
	});

	$("#btnCancel").on("click", function () {
		btnCancelarClick();
	});
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
			CargarDatosExtractoYSistema();
		}
	});
	$('[data-tabindex]').on('keydown', function (e) {
		if (e.key === 'Enter') {
			e.preventDefault();

			const currentTab = parseInt($(this).attr('data-tabindex'));
			const next = $('[data-tabindex]').filter(function () {
				return parseInt($(this).attr('data-tabindex')) === currentTab + 1;
			});

			if (next.length > 0) {
				next.focus();
			} else {
				if ($(this).is('#btnBuscar')) {
					$(this).trigger('click');
				}
			}

		}
	});
});

function ConfirmarConciliacionExtracto() { }

function DesconciliarRegistrosConciliados() {
	var nroConci = $("#ConciliadoNro").val();
	AbrirMensaje("ATENCIÓN", `¿Esta seguro de desconciliar los registros N° ${nroConci}`, function (e) {
		$("#msjModal").modal("hide");
		switch (e) {
			case "SI":
				handleDesconciliarRegistrosConciliados(nroConci);
				break;
			case "NO":
				break;
			default: //NO
				break;
		}
		return true;

	}, true, ["Aceptar", "Cancelar"], "question!", null);
}

function handleDesconciliarRegistrosConciliados(nroConci) {
	AbrirWaiting();
	var ctaf_id = ctafIdSelected;
	var conciliado_nro = nroConci;
	var data = { ctaf_id, conciliado_nro };
	PostGen(data, desconciliarRegistrosConciliadosUrl, function (obj) {
		CerrarWaiting();
		if (obj.error === true) {
			AbrirMensaje("ATENCIÓN", obj.msg, function () {
				$("#msjModal").modal("hide");
				return true;
			}, false, ["Aceptar"], "error!", null);
		}
		else {
			$("#modalRegistrosConciliados").modal("hide");
			ActualizarRegistrosLuegoDeDesconciliar(conciliado_nro);
		}
	});
}

function ActualizarRegistrosLuegoDeDesconciliar(conciliado_nro) {
	AbrirWaiting();
	var ctaf_id = ctafIdSelected;
	var data = { ctaf_id, conciliado_nro };
	PostGenHtml(data, actualizarRegistrosLuegoDeDesconciliarUrl, function (obj) {
		CerrarWaiting();
		$("#divConciliacionExtracto").html(obj);
		$("#divFiltros").removeClass("show").addClass("collapse");
		$("#divDetalle").collapse("show");
		return true
	}, function (obj) {
		console.log(obj);
		ControlaMensajeError(obj.responseText);
		CerrarWaiting();
	});
}

function CancelarConciliacionExtracto() {
	AbrirMensaje("ATENCIÓN", "¿Esta seguro de cancelar la Conciliación del Extracto?", function (e) {
		$("#msjModal").modal("hide");
		switch (e) {
			case "SI":
				handleCancelarConciliacion();
				break;
			case "NO":
				break;
			default: //NO
				break;
		}
		return true;

	}, true, ["Aceptar", "Cancelar"], "question!", null);
}

function handleCancelarConciliacion() {
	btnCancelarClick();
}

function verDetalleConciliado(conciliado_nro) {
	AbrirWaiting();
	var ctaf_id = ctafIdSelected;
	var data = { ctaf_id, conciliado_nro };
	PostGenHtml(data, obtenerModalRegistrosConciliadosUrl, function (obj) {
		$("#divModalRegistrosConciliados").html(obj);
		const $modal = $("#modalRegistrosConciliados");

		$modal.modal({
			backdrop: 'static',
		});

		$modal.modal('show');
		CerrarWaiting();
		return true
	}, function (obj) {
		console.log(obj);
		CerrarWaiting();
		return true
	});
}

function VerDetalleAConciliar(ctaf_id) {
	PostGenHtml(data, obtenerModalRegistrosAConciliarUrl, function (obj) {
		$("#divModalRegistrosAConciliar").html(obj);
		const $modal = $("#modalRegistrosAConciliar");

		$modal.modal({
			backdrop: 'static',
		});

		$modal.modal('show');
		CerrarWaiting();
		return true
	}, function (obj) {
		console.log(obj);
		CerrarWaiting();
		return true
	});
}

function selectGrillaSistema(x) {
	$("#tbGrillaSistema tbody tr").each(function (index) {
		$(this).removeClass("selected-row");
		$(this).removeClass("selectedEdit-row");
	});
	$(x).addClass("selected-row");
}

function selectGrillaExtracto(x) {
	$("#tbGrillaExtracto tbody tr").each(function (index) {
		$(this).removeClass("selected-row");
		$(this).removeClass("selectedEdit-row");
	});
	$(x).addClass("selected-row");
}

function selectGrillaExtractoModalRegConci(x) {
	$("#tbGrillaExtractoRegConci tbody tr").each(function (index) {
		$(this).removeClass("selected-row");
		$(this).removeClass("selectedEdit-row");
	});
	$(x).addClass("selected-row");
}

function btnCancelarClick() {
	$("#divFiltros").removeClass("collapse").addClass("show");
	$("#divDetalle").collapse("hide");
	$("#listaCuentaBanco").val("");
	InicializarDatosEnSesion();
}

function InicializarDatosEnSesion() {
	var data = {};
	PostGen(data, inicializarDatosEnSesionUrl, function (obj) {
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
		}
	});
}

function validarFechas() {
	const $desde = $("#FechaDesde");
	const $hasta = $("#FechaHasta");

	const desde = $desde.val();
	const hasta = $hasta.val();

	if (!desde || !hasta) return;

	const mDesde = moment(desde, "YYYY-MM-DD");
	const mHasta = moment(hasta, "YYYY-MM-DD");
	var now = moment().format('yyyy-MM-DD');

	if (mHasta.isBefore(mDesde)) {
		$desde.val(now);
		$hasta.val(now);
		AbrirMensaje("ATENCIÓN", `Fecha Desde no puede ser mayor a Fecha Hasta`, function () {
			$("#msjModal").modal("hide");
			return true;
		}, false, ["Aceptar"], "error!", null);
		return;
	}
}

function CargarDatosExtractoYSistema() {
	AbrirWaiting();
	var ctaf_id = $("#listaCuentaBanco").val();
	var desde = $("#FechaDesde").val();
	var hasta = $("#FechaHasta").val();
	var concilia = $("#chkConciAuto")[0].checked;
	var select_conciliados = $("#chkRegNoConci")[0].checked;
	var data = { ctaf_id, desde, hasta, concilia, select_conciliados };
	PostGenHtml(data, cargarDatosExtractoYSistema2Url, function (obj) {
		CerrarWaiting();
		$("#divConciliacionExtracto").html(obj);
		$("#divFiltros").removeClass("show").addClass("collapse");
		$("#divDetalle").collapse("show");
		return true
	}, function (obj) {
		console.log(obj);
		ControlaMensajeError(obj.responseText);
		CerrarWaiting();
	});
}

function InicializarCamposEnFiltros() {
	var now = moment().format('yyyy-MM-DD');
	$("#FechaDesde").val(now);
	$("#FechaHasta").val(now);
	$("#divFiltros").removeClass("collapse").addClass("show");
	$("#lbRegNoConci").text("Registros No Conciliados");
	$("#lbConciAuto").text("Conciliación Automática");
	$("#lbCuentaBanco").text("Cuenta Banco");
	$("#lbFecha").text("Fecha");
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

}