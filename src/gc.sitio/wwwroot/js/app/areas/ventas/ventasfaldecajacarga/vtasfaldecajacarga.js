let ultimaFechaFaltanteProcesado = null;
let msgValidacionParaBusqueda = null;
let controlFaltanteEnEdicion = false;
const selectorTablaControl = "#tbFaltantes";
const selectorInputControl = "#tbFaltantes .js-control-acumulado:not(:disabled)";

$(function () {
	InicializarCamposEnFiltros();
	InicializaEventos();
});

function InicializaEventos() {
	$(document).off("change", "#listaSucursales");
	$(document).on("change", "#listaSucursales", ControlalistaSucursalesSelected);

	$("#btnFiltro").on("click", function () {
		if ($("#divFiltros").hasClass("show")) {
			$("#divFiltros").collapse("hide");
			$("#divDetalle").collapse("show");
		}
		else {
			$("#divFiltros").collapse("show");
			$("#divDetalle").collapse("hide");
		}
	});
	$("#btnBuscar").on("click", function () {
		if (validarFechasAnalisis()) {
			MostrarFiltrosAplicados();
			InicializarPantallaPrincipal();
		} else {
			AbrirMensaje("ATENCIÓN", msgValidacionParaBusqueda, function () {
				$("#msjModal").modal("hide");
				return true;
			}, false, ["Aceptar"], "error!", null);
		}
	});
	$(document).off("keydown.controlFaltantes", "#tbFaltantes .js-control-acumulado")
				.on("keydown.controlFaltantes", "#tbFaltantes .js-control-acumulado",
			function (event) {

				if (event.key === "Enter") {

					event.preventDefault();

					moverFocoControlRecepcion(
						this,
						event.shiftKey ? -1 : 1
					);
				}

				if (event.key === "ArrowDown") {

					event.preventDefault();

					moverFocoVerticalControlRecepcion(
						this,
						1
					);
				}

				if (event.key === "ArrowUp") {

					event.preventDefault();

					moverFocoVerticalControlRecepcion(
						this,
						-1
					);
				}
			}
	);
	$(document).off("input.controlFaltantes", "#tbFaltantes .js-control-acumulado")
		.on("input.controlFaltantes", "#tbFaltantes .js-control-acumulado",
			function () {
				/*normalizarEntradaControlRecepcion(this);*/
				marcarCampoModificadoAcumulado(this);
			}
		);
	$(document).off("blur.controlFaltantes", "#tbFaltantes .js-control-acumulado")
		.on("blur.controlFaltantes", "#tbFaltantes .js-control-acumulado",
			function () {
				//formatearInputControlAcumulado(this);
				marcarCampoModificadoAcumulado(this);
				enviarCambioControlAcumulado(this);
			}
		);
	$(document).off("focus.controlFaltantes", "#tbFaltantes .js-control-acumulado")
		.on("focus.controlFaltantes", "#tbFaltantes .js-control-acumulado",
			function () {

				$("#tbFaltantes tbody tr")
					.removeClass("selected-row");

				$(this)
					.closest("tr")
					.addClass("selected-row");

				this.select();
			}
	);
	$(document).off("click.controlFaltantes", "#tbFaltantes tbody tr.fila-control")
		.on("click.controlFaltantes", "#tbFaltantes tbody tr.fila-control", function (e) {

			// Ignorar clicks sobre inputs y botones
			if ($(e.target).closest("input, button, .btn-opcion-tabla").length) {
				return;
			}

			$("#tbFaltantes tbody tr")
				.removeClass("selected-row");

			$(this)
				.addClass("selected-row");
		});

	$(document).off("click.controlFaltantes", "#tbFaltantes tbody tr")
				.on("click.controlFaltantes", "#tbFaltantes tbody tr",
			function () {

				$("#tbFaltantes tbody tr")
					.removeClass("selected-row");

				$(this)
					.addClass("selected-row");

				$(this)
					.find(".js-control-acumulado:first")
					.trigger("focus");
			}
		);
}

function moverFocoVerticalControlRecepcion(inputActual, desplazamiento) {

	const $filaActual = $(inputActual).closest("tr");

	const $filasVisibles = $(
		selectorTablaControl + " tbody tr:not(.fila-eliminada)"
	);

	const indiceActual =
		$filasVisibles.index($filaActual);

	const indiceDestino =
		indiceActual + desplazamiento;

	if (
		indiceDestino < 0 ||
		indiceDestino >= $filasVisibles.length
	) {
		return;
	}

	const $inputDestino =
		$filasVisibles
			.eq(indiceDestino)
			.find(".js-control-acumulado:not(:disabled)");

	if ($inputDestino.length) {
		$inputDestino.trigger("focus");
	}
}

function moverFocoControlRecepcion(inputActual, desplazamiento) {
	const $campos = $(
		selectorInputControl + ":visible"
	);

	const indiceActual = $campos.index(inputActual);

	if (indiceActual < 0 || !$campos.length) {
		return;
	}

	let nuevoIndice =
		(indiceActual + desplazamiento) % $campos.length;

	if (nuevoIndice < 0) {
		nuevoIndice = $campos.length - 1;
	}

	$campos.eq(nuevoIndice).trigger("focus");
}

//function formatearInputControlAcumulado(input) {
//	const $input = $(input);
//	const tipo = $input.data("tipo");
//	const decimales = parseInt($input.data("decimales"), 10) || 0;

//	const valor = obtenerNumeroControlAcumulado($input.val());

//	$input.removeClass("input-invalido");

//	if (!Number.isFinite(valor)) {

//		$input
//			.val(
//				tipo === "entero"
//					? "0"
//					: (0).toFixed(decimales)
//			)
//			.addClass("input-invalido");

//		return;
//	}

//	if (tipo === "entero") {
//		$input.val(Math.trunc(valor).toString());
//		return;
//	}

//	$input.val(valor.toFixed(decimales));
//}

function obtenerValorNumerico($input) {

	const valor =
		$input.inputmask("unmaskedvalue");

	return Number(valor || 0);
}

function enviarCambioControlAcumulado(input) {

	const $input = $(input);

	const valorActual =
		obtenerValorNumerico($input);

	const valorOriginal =
		Number($input.attr("data-original-value"));

	if (
		Math.abs(valorActual - valorOriginal)
		< 0.000001
	) {
		return;
	}

	const $fila =
		$input.closest("tr");

	const dto = {
		usu_id: String($fila.data("usu-id")),
		adm_id: String($fila.data("adm-id")),
		valor: Number($input.inputmask("unmaskedvalue"))
	};

	actualizarAcumulado(dto);
}

function actualizarAcumulado(dto) {
	console.log("dto", dto);
	var data = JSON.stringify(dto);
	$.ajax({
		url: "/Ventas/VentasFalDeCajaCarga/ActualizarAcumulado",
		type: "POST",
		contentType: "application/json",
		data: JSON.stringify(dto),

		success: function (response) {

			console.log("Actualizado correctamente");
		},

		error: function (xhr) {

			console.error("Error al actualizar", xhr);
		}
	});
}

function marcarCampoModificadoAcumulado(input) {

	const $input = $(input);

	const valorActual = Number(
		$input.inputmask("unmaskedvalue")
	);

	const valorOriginal = Number(
		obtenerNumeroControlAcumulado(
			$input.attr("data-original-value")
		)
	);

	const fueModificado =
		Math.abs(valorActual - valorOriginal) > 0.000001;

	$input.toggleClass(
		"campo-modificado",
		fueModificado
	);

	const $contenedor =
		$input.closest(".input-container");

	if (fueModificado) {

		if (
			$contenedor.find(".indicador-cambio").length === 0
		) {
			$contenedor.append(
				'<div class="indicador-cambio"></div>'
			);
		}

		habilitarConfirmarFaltantes();
	}
	else {

		$contenedor.find(".indicador-cambio").remove();

		if (
			$("#tbFaltantes .campo-modificado").length === 0
		) {
			deshabilitarConfirmarFaltantes();
		}
	}
}

function deshabilitarConfirmarFaltantes() {
	controlFaltanteEnEdicion = true;

	$("#btnConfirmarFaltantes")
		.prop("disabled", true)
		.addClass("disabled");
}

function habilitarConfirmarFaltantes() {
	controlFaltanteEnEdicion = false;

	$("#btnConfirmarFaltantes")
		.prop("disabled", false)
		.removeClass("disabled");
}

function obtenerNumeroControlAcumulado(valor) {

	if (
		valor === null ||
		valor === undefined ||
		valor === ""
	) {
		return 0;
	}

	const textoNormalizado = String(valor)
		.replace(/,/g, '')
		.trim();

	const numero = parseFloat(textoNormalizado);

	return Number.isFinite(numero)
		? numero
		: 0;
}

function InicializarPantallaPrincipal() {
	AbrirWaiting("Cargando información...");
	PostGenHtml({}, inicializarPantallaPrincipalURL, function (obj) {
		$("#divDetalle").html(obj);
		// Actualizar filtros aplicados después de renderizar la pantalla principal
		try { MostrarFiltrosAplicados(); } catch (e) { console.warn('MostrarFiltrosAplicados no disponible:', e); }
		
		$("#divFiltros").collapse("hide");
		$("#divDetalle").collapse("show");
		CerrarWaiting();
		setTimeout(() => {
			CargarFaltantesDeCajas();
		}, 200);
		return true
	});
}

function CargarFaltantesDeCajas() {
	const admId = $("#listaSucursales").val();
	const hasta = $("#ProcesarHasta").val();
	var data = { adm_id: admId, hasta };
	AbrirWaiting("Cargando Faltantes...");
	PostGenHtml(data, cargarFaltanteDeCajaURL, function (obj) {
		$("#divFaltante").html(obj);
		getMaskForMoneyType(
			"#tbFaltantes .js-control-acumulado",
			2
		);
		$("#tbFaltantes .js-control-acumulado").each(function () {

			$(this).attr(
				"data-original-value",
				Number($(this).inputmask("unmaskedvalue"))
			);

		});
		CerrarWaiting();
		setTimeout(() => {
			
		}, 200);
		return true
	});
}

function getMaskForMoneyType(selector, decimales) {
	$(selector).inputmask({
		alias: 'numeric',
		groupSeparator: '.',
		radixPoint: ',',
		digits: decimales,
		digitsOptional: false,
		allowMinus: true,
		prefix: '',
		suffix: '',
		rightAlign: true,
		unmaskAsNumber: true,
		positionCaretOnClick: "lvp",
		onBeforeWrite: function (event, buffer, caretPos, opts) {
			//console.log("event: " + event);
		}
	});
}

function MostrarFiltrosAplicados() {
	try {
		const cont = $("#filtrosAplicadosFloating");
		const fallback = $(".p-1.border.rounded.bg-light").first();
		const target = cont.length ? cont : fallback;
		if (!target.length) return;

		const desde = ultimaFechaFaltanteProcesado;
		const hasta = $("#ProcesarHasta").val();

		const sucursal = $("#listaSucursales option:selected").text();;

		let html = '<div class="d-inline-flex align-items-center" style="gap:8px;white-space:nowrap;">';
		if (desde) html += `<span class="badge bg-secondary">Último Faltante Procesado: ${desde}</span>`;
		if (hasta) html += `<span class="badge bg-secondary">Procesado Hasta: ${hasta}</span>`;
		if (sucursal) html += `<span class="badge bg-secondary">Sucursal: ${sucursal}</span>`;
		
		html += '</div>';

		// Render
		target.html(html);
	} catch (e) {
		console.error('MostrarFiltrosAplicados error', e);
	}
}

function InicializarCamposEnFiltros() {
	$("#chkSucursales").prop('checked', true);
	$("#chkSucursales").trigger("change");
	$("#chkSucursales").prop("disabled", true);

	$("#btnImprimir").hide();
	$("#lbSucursales").text("Sucursal");

	$("#divUltFaltanteProcesado").hide();
	$("#spnUltFaltanteProcesado").text("");

	$("#divFiltros").collapse("show");
	$("#divDetalle").collapse("hide");
}

function ControlalistaSucursalesSelected() {
	var data = { admId: $("#listaSucursales").val() };
	PostGen(data, inicializarFechaProcesoFaltanteURL, function (obj) {
		CerrarWaiting();
		if (obj.error === true || obj.warn === true) {
			CerrarWaiting();
			AbrirMensaje("ATENCIÓN", obj.msg, function () {
				$("#msjModal").modal("hide");
				return true;
			}, false, ["Aceptar"], "error!", null);
		}
		else {
			$("#spnUltFaltanteProcesado").text(obj.fecha).removeClass("d-none");
			//$("#divUltFaltanteProcesado").show();

			ultimaFechaFaltanteProcesado = obj.fecha;
		}
	});
}

function validarFechasAnalisis() {
	msgValidacionParaBusqueda = null;
	const hastaInput = document.getElementById("ProcesarHasta");

	const hasta = new Date(hastaInput.value);

	// Si no hay fecha seleccionada aún
	if (isNaN(hasta)) {
		msgValidacionParaBusqueda = "Debe seleccionado un valor válido para 'Procesado Hasta'.";
		return false;
	}

	// Si todavía no se obtuvo la fecha desde backend
	if (!ultimaFechaFaltanteProcesado) {
		msgValidacionParaBusqueda = "Debe seleccionar una 'Sucursal'.";
		return false;
	}

	// Convertir dd/MM/yyyy a Date
	const partes = ultimaFechaFaltanteProcesado.split('/');

	const desde = new Date(
		parseInt(partes[2]), // año
		parseInt(partes[1]) - 1, // mes
		parseInt(partes[0]) // día
	);

	if (hasta < desde) {
		msgValidacionParaBusqueda = "El valor de 'Procesado Hasta' no puede ser meno a 'Último Faltante Procesado'.";
		return false;
	}

	return true;
}
