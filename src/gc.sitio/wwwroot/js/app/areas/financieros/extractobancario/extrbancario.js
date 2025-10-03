var itemSeleccionadoOrden = 0;
var fecha_extracto;
$(function () {
	InicializarCamposEnFiltros();
	$(document).on("change", "#listaCuentaBanco", ControlalistaCuentaBancoSelected);
	$(document).on("click", "#btnAgregarItem", abrirModalAgregarItemExtracto);
	$(document).on("click", "#btnModificarItem", abrirModalModificarItemExtracto);
	$(document).on("click", "#btnCancelarCarga", cancelarCargaDeExtracto);
	$(document).on("click", "#btnConfirmarAgregarExtracto", confirmarAgregarExtracto);
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
});

function cancelarCargaDeExtracto() {
	AbrirMensaje("ATENCIÓN", "¿Esta seguro que desea cancelar la carga del extracto?", function (e) {
		$("#msjModal").modal("hide");
		switch (e) {
			case "SI": 
				btnCancelarClick();
				break;
			case "NO":
				break;
			default: //NO
				break;
		}
		return true;

	}, true, ["Aceptar", "Cancelar"], "question!", null);
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

function eliminarItem(ctaf_id, extr_id, orden) {
	console.log("Ver detalle de cheque:", ctaf_id, extr_id, orden);
	var data = {
		orden
	};
	AbrirWaiting();
	PostGen(data, quitarItemExtractoUrl, function (obj) {
		CerrarWaiting();
		if (obj.error === true) {
			CerrarWaiting();
			AbrirMensaje("ATENCIÓN", obj.msg, function () {
				$("#msjModal").modal("hide");
				return true;
			}, false, ["Aceptar"], "error!", null);
		}
		else {
			setTimeout(() => {
				$('#modalAgregarItemExtracto').modal('hide');
				obtenerListaExtractoBancario();
			}, 200);
		}
	});
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
	//$("#btnCancel").on("click", function () {
	//	btnCancelarClick();
	//});
}

function btnCancelarClick() {
	$("#divFiltros").removeClass("collapse").addClass("show");
	$("#divDetalle").collapse("hide");
	$("#listaCuentaBanco").val("");
	EstableceValoresDeFechas(fecha_extracto);
	InicializarDatosEnSesion();
}

function abrirModalModificarItemExtracto() {
	if (abrirModalModificarItemExtracto != 0) {
		AbrirWaiting();
		var orden = itemSeleccionadoOrden;
		var abm = "M";
		var datos = { abm, orden };
		PostGenHtml(datos, abrirModalAgregarItemExtractoUrl, function (obj) {
			$("#divAgregarItemExtracto").html(obj);
			const $modal = $("#modalAgregarItemExtracto");

			$modal.modal({
				backdrop: 'static',
			});

			$("#Fecha").trigger("focus");

			// ✅ Corrección: usar el modal correcto y evitar document.ready redundante
			$modal.find("input, select, checkbox").on("keydown", function (e) {
				if (e.key === "Enter") {
					e.preventDefault();

					const $campos = $modal.find("input, select, checkbox")
						.filter(":visible:enabled");

					const index = $campos.index(this);

					if (index !== -1 && index < $campos.length - 1) {
						$campos.eq(index + 1).focus();
					}
				}
			});

			["#Debe", "#Haber"].forEach(selector => {
				const $campo = $modal.find(selector);
				let valor = $campo.val();

				// Si el valor tiene punto decimal, lo transformamos
				if (valor && valor.includes(".")) {
					valor = valor.replace(".", ",");
					$campo.val(valor);
				}
			});

			getMaskForMoneyType("#Debe");
			getMaskForMoneyType("#Haber");
			$("#listaMovimientos").trigger("focus");

			$modal.modal('show');
			CerrarWaiting();
			return true
		});
	}
	else {
		AbrirMensaje("ATENCIÓN", "Debe seleccionar un ítem extracto para modificar.", function () {
			$("#msjModal").modal("hide");
			return true;
		}, false, ["Aceptar"], "error!", null);
	}
}

function abrirModalAgregarItemExtracto() {
	AbrirWaiting();
	var datos = { abm: "A", orden: itemSeleccionadoOrden };
	PostGenHtml(datos, abrirModalAgregarItemExtractoUrl, function (obj) {
		$("#divAgregarItemExtracto").html(obj);
		const $modal = $("#modalAgregarItemExtracto");

		$modal.modal({
			backdrop: 'static',
		});
		//$modal.modal('show');
		$("#Fecha").trigger("focus");

		// ✅ Corrección: usar el modal correcto y evitar document.ready redundante
		$modal.find("input, select, checkbox").on("keydown", function (e) {
			if (e.key === "Enter") {
				e.preventDefault();

				const $campos = $modal.find("input, select, checkbox")
					.filter(":visible:enabled");

				const index = $campos.index(this);

				if (index !== -1 && index < $campos.length - 1) {
					$campos.eq(index + 1).focus();
				}
			}
		});

		// Interceptar y transformar valores numéricos antes de aplicar Inputmask
		["#Debe", "#Haber"].forEach(selector => {
			const $campo = $modal.find(selector);
			let valor = $campo.val();

			// Si el valor tiene punto decimal, lo transformamos
			if (valor && valor.includes(".")) {
				valor = valor.replace(".", ",");
				$campo.val(valor);
			}
		});

		getMaskForMoneyType("#Debe");
		getMaskForMoneyType("#Haber");
		$("#Fecha").trigger("focus");

		$modal.modal('show');
		CerrarWaiting();
		return true
	});
}

function confirmarAgregarExtracto() {
	var abm = $("#abm").val();
	if (abm == "A") {
		var fecha = $("#Fecha").val();
		var insertar = $("#chkInsertar")[0].checked;
		var movimiento = $("#listaMovimientos").val();
		var movimiento_desc = $("#listaMovimientos option:selected").text();
		var comprobante = $("#Comprobante").val();
		var debe = $("#Debe").inputmask('unmaskedvalue');
		var haber = $("#Haber").inputmask('unmaskedvalue');
		var orden = $("#orden").val();
		//Actualizar lista en backend
		var data = {
			ctaf_id: ctafIdSelected,
			ext_fecha: fecha,
			extr_id: movimiento,
			extr_desc: movimiento_desc,
			ext_concepto: comprobante,
			ext_debe: debe,
			ext_haber: haber,
			abm: "A",
			insertar,
			orden
		};
		AbrirWaiting();
		PostGen(data, agregarItemExtractoUrl, function (obj) {
			CerrarWaiting();
			if (obj.error === true) {
				CerrarWaiting();
				AbrirMensaje("ATENCIÓN", obj.msg, function () {
					$("#msjModal").modal("hide");
					return true;
				}, false, ["Aceptar"], "error!", null);
			}
			else {
				setTimeout(() => {
					$('#modalAgregarItemExtracto').modal('hide');
					obtenerListaExtractoBancario();
				}, 200);
			}
		});
	}
	else if (abm == "M") {
		var orden = $("#orden").val();
		var movimiento = $("#listaMovimientos").val();
		var movimiento_desc = $("#listaMovimientos option:selected").text();
		var comprobante = $("#Comprobante").val();
		var debe = $("#Debe").inputmask('unmaskedvalue');
		var haber = $("#Haber").inputmask('unmaskedvalue');
		
		var data = {
			orden,
			extr_id: movimiento,
			extr_desc: movimiento_desc,
			ext_concepto: comprobante,
			ext_debe: debe,
			ext_haber: haber,
			abm: "M"
		};
		AbrirWaiting();
		PostGen(data, modificarItemExtractoUrl, function (obj) {
			CerrarWaiting();
			if (obj.error === true) {
				CerrarWaiting();
				AbrirMensaje("ATENCIÓN", obj.msg, function () {
					$("#msjModal").modal("hide");
					return true;
				}, false, ["Aceptar"], "error!", null);
			}
			else {
				setTimeout(() => {
					$('#modalAgregarItemExtracto').modal('hide');
					obtenerListaExtractoBancario();
				}, 200);
			}
		});
	}
}

function obtenerListaExtractoBancario() {
	CerrarWaiting();
	var data = {};
	PostGenHtml(data, obtenerListaExtractoBancarioUrl, function (obj) {
		$("#divGridCrudExtracto").html(obj);
		itemSeleccionadoOrden = 0;
		CerrarWaiting();
		return true
	});
}

function selectItemExtracto(x) {
	console.log($(x).data("orden"))
	itemSeleccionadoOrden = $(x).data("orden");
	$("#tbListaCrudExtracto tbody tr").each(function (index) {
		$(this).removeClass("selected-row");
		$(this).removeClass("selectedEdit-row");
	});
	$(x).addClass("selected-row");
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