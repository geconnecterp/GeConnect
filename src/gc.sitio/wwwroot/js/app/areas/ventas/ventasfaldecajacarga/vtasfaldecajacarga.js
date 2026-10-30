let ultimaFechaFaltanteProcesado = null;
let msgValidacionParaBusqueda = null;
let hayCambiosPendientes = false;
const selectorTablaControl = "#tbFaltantes";
const selectorInputControl = "#tbFaltantes .js-control-acumulado:not(:disabled)";
let ultimaFilaDetalle = null;

$(function () {
	InicializarCamposEnFiltros();
	InicializaEventos();
});

function InicializaEventos() {
	$(document).on("click", "#btnImprimir", ImprimirDetalle);
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
				const $fila = $(this).closest("tr");

				seleccionarFilaFaltante($fila);

				this.select();
			}
	);
	$(document).off("click.controlFaltantes", "#tbFaltantes tbody tr.fila-control")
		.on("click.controlFaltantes", "#tbFaltantes tbody tr.fila-control", function (e) {

			// Ignorar clicks sobre inputs y botones
			if ($(e.target).closest("input, button, .btn-opcion-tabla").length) {
				return;
			}

			$("#tbFaltantes tbody tr").removeClass("selected-row");
			$(this).addClass("selected-row");

			const usuId = $(this).data("usu-id");
			const admId = $(this).data("adm-id");
			const procesarHasta = $("#ProcesarHasta").val();

			BuscarDetalleDeFaltante(usuId, admId, procesarHasta);
		});

	$(document).off("click.controlFaltantes", "#tbFaltantes tbody tr")
				.on("click.controlFaltantes", "#tbFaltantes tbody tr",
			function () {

				seleccionarFilaFaltante($(this));
			}
	);

	$(document).off("click.controlFaltantes", "#tbFaltantesDetalle tbody tr")
		.on("click.controlFaltantes", "#tbFaltantesDetalle tbody tr",
			function () {

				$("#tbFaltantesDetalle tbody tr")
					.removeClass("selected-row");

				$(this)
					.addClass("selected-row");

			}
	);
	$(document).off("click.controlFaltantes", "#btnConfirmarFaltantes")
		.on("click.controlFaltantes", "#btnConfirmarFaltantes", function () {

			console.log("click.controlFaltantes #btnConfirmarFaltantes");
			confirmarFaltante();
		}
	);
	$(document).off("click.controlFaltantes", "#btnCancelarFaltantes")
		.on("click.controlFaltantes", "#btnCancelarFaltantes", function () {

			if (!hayCambiosPendientes) {
				deshabilitarConfirmarFaltantes();
				CargarFaltantesDeCajas();
			}
			else {
				AbrirMensaje("ATENCIÓN", "¿Cancelar la carga de faltante de caja?", function (e) {
					$("#msjModal").modal("hide");
					switch (e) {
						case "SI":
							deshabilitarConfirmarFaltantes();
							CargarFaltantesDeCajas();
							break;
						case "NO":
							break;
						default: //NO
							break;
					}
					return true;

				}, true, ["SI", "NO"], "question!", null);
			}
		});
}

function confirmarFaltante() {
	AbrirMensaje("ATENCIÓN", "Desea confirmar el faltante de caja?", function (e) {
		$("#msjModal").modal("hide");
		switch (e) {
			case "SI":
				handlerconfirmarFaltante();
				break;
			case "NO":
				break;
			default: //NO
				break;
		}
		return true;

	}, true, ["SI", "NO"], "question!", null);
}

function handlerconfirmarFaltante() {
	AbrirWaiting("Guardando cambios....")
	const fecha_hasta_fal = $("#ProcesarHasta").val();
	const adm_id_fal = $("#listaSucursales").val();

	PostGen({ fecha_hasta_fal, adm_id_fal }, confirmarFaltanteDeCajaUrl, function (obj) {
		CerrarWaiting();
		if (obj.error === true || obj.warn === true) {
			console.error('❌ Response:', obj.msg);
			AbrirMensaje("ATENCIÓN", 'Error al intentar confirmar el faltante de caja: ' + (obj.msg || 'Error desconocido'), function () {
				$("#msjModal").modal("hide");
				return true;
			}, false, ["Aceptar"], "error!", null);
		}
		else {
			setTimeout(() => {
				AbrirMensaje(
					'CONFIRMACIÓN EXITOSA',
					'Se ha confirmado el Faltante de Caja.',
					function () {
						$('#msjModal').modal('hide');
						CargarFaltantesDeCajas();
						//Imprimir Acuse de Confirmación
						HandlerImprimirAcuse(obj.id);
						//console.log("Planilla de Elaboración y Fracción Generada: ", obj.id);
						//ResetearPantalla();
					},
					false,
					['Aceptar'],
					'success!',
					null
				);
			}, 200);
		}
	});
}

function HandlerImprimirAcuse(id) {
	ReseteoDeReportes();
	setTimeout(() => {
		let data = { id };
		cargarReporteEnArre(40, data, "DETALLE DE ANTICIPO", "", "");
		invocacionGestorDoc({});
	}, 500);
}

function ReseteoDeReportes() {
	console.log("Reseto de reportes");
	ReporteResetArre();
}

function seleccionarFilaFaltante($fila) {

	const claveActual =
		$fila.data("usu-id") + "_" +
		$fila.data("adm-id");

	if (ultimaFilaDetalle === claveActual) {
		return;
	}

	ultimaFilaDetalle = claveActual;

	$("#tbFaltantes tbody tr").removeClass("selected-row");
	$fila.addClass("selected-row");

	const usuId = $fila.data("usu-id");
	const admId = $fila.data("adm-id");

	BuscarDetalleDeFaltante(usuId, admId, $("#ProcesarHasta").val());
}


function BuscarDetalleDeFaltante(usuId, admId, procesarHasta) {
	var data = { adm_id: admId, hasta: procesarHasta, usu_id: usuId };
	AbrirWaiting("Cargando Detalle de Faltantes...");
	PostGenHtml(data, cargarDetalleDeFaltanteDeCajaURL, function (obj) {
		$("#divFaltanteDetalle").html(obj);
		CerrarWaiting();
		setTimeout(() => {

		}, 200);
		return true
	});
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
			habilitarConfirmarFaltantes();
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
	hayCambiosPendientes = false;
	$("#btnConfirmarFaltantes")
		.prop("disabled", true)
		.addClass("disabled");
}

function habilitarConfirmarFaltantes() {
	hayCambiosPendientes = true;
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

		// Seleccionar automáticamente la primera fila y cargar el detalle
		const $primeraFila = $("#tbFaltantes tbody tr").not(".fila-vacia").first();

		if ($primeraFila.length) {
			seleccionarFilaFaltante($primeraFila);

			// Opcional: dejar enfocado el campo editable
			//$primeraFila.find(".js-control-acumulado:first").trigger("focus");
		}
		deshabilitarConfirmarFaltantes();
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
		const hastaValue = $("#ProcesarHasta").val();
		let hasta = "";
		if (hastaValue) {
			const [anio, mes, dia] = hastaValue.split("-");
			hasta = `${dia}/${mes}/${anio}`;
		}

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

function ImprimirDetalle() {
	ReseteoDeReportes();
	HandlerImprimirAcuse("00-00006857");
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
