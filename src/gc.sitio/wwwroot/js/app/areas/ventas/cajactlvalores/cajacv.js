var caja_nro_proceso_selected = null;
var caja_nro_cierre_selected = null;
var caja_id_selected = null;
var cierre_pendientes_bool = null;
var caja_nro_rend_selected = null;
var tcf_id_selected = null;
var rend_pendiente_selected = null;
var existe_edicion = false;
var fila_seleccionada_actual = null;
var fila_cierre_seleccionada_actual = null;
var guardando_importe = false;
var sucDesc_selected = null;
var sucId_selected = null;
var diaId_selected = null;
var clienteIdSelected = null;
var clienteDescSelected = null;

$(function () {
	if ($("#divDetalle").is(":visible")) {
		$("#divDetalle").collapse("hide");
	}
	$("#divFiltro").collapse("show");

	$("#btnCancel").on("click", function () {
		window.location.href = homeCtlValoresUrl;
	});

	$("#lbSucursales").text("Sucursal");
	$("#lbDias").text("Día");

	$("#btnBuscar").on("click", function () {
		if (validarCamposSeleccionados()) {
			InicializarBusqueda();
		} else {
			AbrirMensaje("ATENCIÓN", "Debe seleccionar Sucursal y Día.", function () {
				$("#msjModal").modal("hide");
				return true;
			}, false, ["Aceptar"], "error!", null);
		}
	});

	$("#chkDias").on("click", function () {
		if ($("#chkDias").is(":checked")) {
			$("#listaDias").prop("disabled", false);
			$("#listaDias").trigger("focus");
		}
		else {
			$("#listaDias").prop("disabled", true).val("");
		}
	});
	$("#chkSucursales").prop("checked", true);
	$("#chkSucursales").prop("disabled", true);
	$("#chkSucursales").trigger('change');
	$("#listaSucursales").prop("disabled", false);

	$(document).on("change", "#listaSucursales", ControlalistaSucursalesSelected);
});

function InicializarBusqueda() {
	sucDesc_selected = $("#listaSucursales").find("option:selected").text();
	sucId_selected = $("#listaSucursales").find("option:selected").val();
	diaId_selected = $("#listaDias").find("option:selected").val();
	var data = { admDesc: sucDesc_selected, admId: sucId_selected, nroProceso: diaId_selected };
	AbrirWaiting("Cargando datos de cierres...");
	PostGenHtml(data, cargarDatosDeCierresUrl, function (html) {
		$("#divDetalle").html(html);
		$("#divFiltro").collapse("hide");
		$("#divDetalle").collapse("show");
		InicializaEventosGrillaVtasPVCtlCierres();
		CerrarWaiting();
	});
}

function CargarGrillaVtasPVCtlCierres() {
	var data = { admDesc: sucDesc_selected, admId: sucId_selected, nroProceso: diaId_selected };
	AbrirWaiting("Actualizando cierres...");
	PostGenHtml(data, cargarDatosDeCierresUrl, function (html) {
		CerrarWaiting();
		$("#divDetalle").html(html);
		InicializaEventosGrillaVtasPVCtlCierres();
	});
}

function validarCamposSeleccionados() {
	let sucSeleccionada = $("#listaSucursales").val();
	let diaSeleccionado = $("#listaDias").val();
	if (sucSeleccionada == null || sucSeleccionada == undefined || sucSeleccionada == "")
		return false;
	if (diaSeleccionado == null || diaSeleccionado == undefined || diaSeleccionado == "")
		return false;
	return true;
}

function ControlalistaSucursalesSelected() {
	var item = $("#listaSucursales").val();
	var data = { suc_id: item };
	AbrirWaiting("Cargando datos de días...");
	PostGenHtml(data, obtenerDiasPorSucursalUrl, function (html) {
		CerrarWaiting();
		$("#divListaDias").html(html);
		$("#divDetalle").empty();
		setTimeout(function () {
			$("#chkDias").prop("disabled", false);
			$("#chkDias").trigger('change');
			$("#chkDias").prop("checked", true);
			$("#listaDias").prop("disabled", false);
			$("#listaDias").trigger('focus');
		}, 0);
	});
}

function SeleccionarPrimeraFilaCierres() {
	const $filas = $("#tbVtasPVCtlCierres tbody tr").not(".fila-vacia");

	if ($filas.length === 0) return;

	const $primera = $filas.first();

	// Guardar referencia
	fila_cierre_seleccionada_actual = $primera;

	// Marcar visualmente
	$("#tbVtasPVCtlCierres tbody tr").removeClass("selected-row");
	$primera.addClass("selected-row");

	// Ejecutar la lógica normal de selección
	ProcesarSeleccionFilaCierres($primera);
}


function ProcesarSeleccionFilaCierres($fila) {

	// Quitar selección previa
	$("#tbVtasPVCtlCierres tbody tr").removeClass("selected-row");

	// Marcar fila seleccionada
	$fila.addClass("selected-row");

	// Guardar referencia
	fila_cierre_seleccionada_actual = $fila;

	// Guardar valores seleccionados
	caja_nro_proceso_selected = $fila.data("caja-nro-proceso");
	caja_nro_cierre_selected = $fila.data("caja-nro-cierre");
	caja_id_selected = $fila.data("caja-id");
	cierre_pendientes_bool = String($fila.data("pendientes-bool")).toLowerCase() === "true";

	// Habilitar / deshabilitar botón
	const habilitar = (cierre_pendientes_bool === true || cierre_pendientes_bool === "true" || cierre_pendientes_bool === "True");
	$("#btnConfirmacionContable").prop("disabled", habilitar);

	// Cargar grilla de rendiciones
	if (caja_nro_proceso_selected) {
		CargarGrillaVtasPVCtlRend();
	}
}


function InicializaEventosGrillaVtasPVCtlCierres() {
	$(document).off("click", "#btnConfirmacionContable");
	$(document).on("click", "#btnConfirmacionContable", function (e) {
		EvaluarConfirmacionContable();
	});

	$(document).off("click", "#tbVtasPVCtlCierres tbody tr");
	$(document).on("click", "#tbVtasPVCtlCierres tbody tr", function (e) {

		if ($(e.target).is("button, a, .btn, i")) return;

		const $nuevaFila = $(this);

		// Si ya había una fila seleccionada y se intenta cambiar
		if (fila_cierre_seleccionada_actual && fila_cierre_seleccionada_actual[0] !== $nuevaFila[0]) {

			if (existe_edicion === true) {
				AbrirMensaje("ATENCIÓN", "Tiene cambios sin guardar en la grilla de rendiciones. Si cambia de cierre perderá los cambios realizados. ¿Desea continuar?", function (e) {
					$("#msjModal").modal("hide");
					switch (e) {
						case "SI":
							existe_edicion = false; // Se descartan cambios
							ProcesarSeleccionFilaCierres($nuevaFila);
							break;
						case "NO":
							break;
						default: //NO
							break;
					}
					return true;

				}, true, ["Aceptar", "Cancelar"], "question!", null);

				return; // Detener el click original
			}
		}

		// Si no hay edición pendiente o es la misma fila → continuar normalmente
		ProcesarSeleccionFilaCierres($nuevaFila);
	});

	$("#btnConfirmacionContable").prop("disabled", true);
	$("#btnConfirmarArqueo").prop("disabled", true);
	$("#btnAnularArqueo").prop("disabled", true);
	$("#btnAgregarArqueo").prop("disabled", !cierre_pendientes_bool);
	$("#btnGuardarValores").prop("disabled", true);

	// 🔥 Seleccionar automáticamente la primera fila válida
	SeleccionarPrimeraFilaCierres();
}

function EvaluarConfirmacionContable() {
	AbrirMensaje("ATENCIÓN", `¿Esta seguro que desea realizar la confirmación contable?`, function (e) {
		$("#msjModal").modal("hide");
		switch (e) {
			case "SI":
				ConfirmacionContable();
				break;
			case "NO":
				break;
			default: //NO
				break;
		}
		return true;

	}, true, ["Aceptar", "Cancelar"], "question!", null);
}

function ConfirmacionContable() {
	var data = {
		caja_nro_proceso: caja_nro_proceso_selected,
		caja_nro_cierre: caja_nro_cierre_selected,
	};
	AbrirWaiting("Realizando confirmación contable...");
	PostGen(data, confirmacionContableUrl, function (obj) {
		CerrarWaiting();
		if (obj.error === true || obj.warn === true) {
			let resumen = obj.msg;
			if (obj.errores && obj.errores.length > 0) {
				resumen = GenerarResumenErroresSimples(obj.errores);
			}
			AbrirMensaje("ATENCIÓN", resumen, function () {
				$("#msjModal").modal("hide");
				return true;
			}, false, ["Aceptar"], "error!", null);
		}
		else {
			AbrirMensaje("ATENCIÓN", "Se ha realizado la confirmación contable de forma exitosa.", function () {
				$("#msjModal").modal("hide");
				// 🔥 Redirigir al Index del módulo
				window.location.href = homeCtlValoresUrl;
				return true;
			}, false, ["Aceptar"], "succ!", null);
		}
	});
}

function GenerarResumenErroresSimples(errores) {
	let resumen = "<strong>Errores en la confirmación contable:</strong><br><br>";

	errores.forEach(err => {
		resumen += `<div>• ${err}</div>`;
	});

	return resumen;
}

function GenerarResumenErroresConDetalles(fallidos) {

	const grupos = {};

	fallidos.forEach(f => {
		const mensaje = f.mensaje || "Error desconocido";
		const id = f.ent_compte;

		if (!grupos[mensaje]) {
			grupos[mensaje] = {
				cantidad: 0,
				ids: []
			};
		}

		grupos[mensaje].cantidad++;
		grupos[mensaje].ids.push(id);
	});

	// Construir texto final
	let resumen = "Algunas entregas no pudieron confirmarse:\n\n";

	Object.entries(grupos).forEach(([mensaje, info]) => {
		resumen += `• ${mensaje} (${info.cantidad})\n`;

		info.ids.forEach(id => {
			resumen += `   - Cierre ${id}\n`;
		});

		resumen += "\n";
	});

	return resumen;
}



function GenerarResumenErrores(respuestas) {
	const contador = {};

	// Recorrer todas las respuestas
	Object.values(respuestas).forEach(r => {
		const msg =
			r?.mensaje ||
			r?.entidad?.resultado_msj ||
			"Error desconocido";

		if (!contador[msg]) {
			contador[msg] = 1;
		} else {
			contador[msg]++;
		}
	});

	// Construir texto final
	let resumen = "Algunas confirmaciones no pudieron confirmarse:\n\n";

	Object.entries(contador).forEach(([mensaje, cantidad]) => {
		resumen += `• ${mensaje} (${cantidad})\n`;
	});

	return resumen;
}


function CargarGrillaVtasPVCtlRend() {
	if (!validarCierreSeleccionado()) {
		AbrirMensaje("ATENCIÓN", "Debe seleccionar un Cierre.", function () {
			$("#msjModal").modal("hide");
			return;
		}, false, ["Aceptar"], "error!", null);
	}
	else {
		var data = { nro_proceso: caja_nro_proceso_selected, nro_cierre: caja_nro_cierre_selected };
		AbrirWaiting("Cargando datos de rendición de Cierre seleccionado...");
		PostGenHtml(data, obtenerRendDeCierreSeleccionadoUrl, function (html) {
			CerrarWaiting();
			$("#divVtasPVCtlRend").html(html);
			InicializaEventosGrillaVtasPVCtlRend();
			// Seleccionar automáticamente la primera fila si existe
			SeleccionarPrimeraFilaRend();
		});
	}
}

function SeleccionarPrimeraFilaRend() {

	const $filas = $("#tbVtasPVCtlRend tbody tr").not(".fila-vacia");

	if ($filas.length === 0) return;

	const $primera = $filas.first();

	// Guardar como fila seleccionada actual
	fila_seleccionada_actual = $primera;

	// Marcar visualmente
	$("#tbVtasPVCtlRend tbody tr").removeClass("selected-row");
	$primera.addClass("selected-row");

	// Ejecutar la lógica normal de selección
	ProcesarSeleccionFila($primera);
}

function InicializaEventosGrillaVtasPVCtlRend() {
	$(document).off("click", "#btnConfirmarArqueo");
	$(document).on("click", "#btnConfirmarArqueo", function (e) {
		EvaluarConfirmarCtlArqueo();
	});

	$(document).off("click", "#btnAnularArqueo");
	$(document).on("click", "#btnAnularArqueo", function (e) {
		EvaluarAnularCtlArqueo();
	});

	$(document).off("click", "#btnAgregarArqueo");
	$(document).on("click", "#btnAgregarArqueo", function (e) {
		EvaluarAgregarCtlArqueo();
	});

	$(document).off("click", "#tbVtasPVCtlRend tbody tr");
	$(document).on("click", "#tbVtasPVCtlRend tbody tr", function (e) {

		if ($(e.target).is("button, a, .btn, i")) return;

		const $nuevaFila = $(this);

		// Si ya había una fila seleccionada y se intenta cambiar
		if (fila_seleccionada_actual && fila_seleccionada_actual[0] !== $nuevaFila[0]) {

			if (existe_edicion === true) {
				AbrirMensaje("ATENCIÓN", "Tiene cambios sin guardar. Si cambia de fila perderá los cambios realizados. ¿Desea continuar?", function (e) {
					$("#msjModal").modal("hide");
					switch (e) {
						case "SI":
							existe_edicion = false; // Se descartan cambios
							RestaurarValoresOriginalesEnPadre();
							ProcesarSeleccionFila($nuevaFila);
							break;
						case "NO":
							break;
						default: //NO
							break;
					}
					return true;

				}, true, ["Aceptar", "Cancelar"], "question!", null);

				return; // Detener el click original
			}
		}

		// Si no hay edición pendiente o es la misma fila → continuar normalmente
		ProcesarSeleccionFila($nuevaFila);
	});

	$("#btnConfirmarArqueo").prop("disabled", true);
	$("#btnAnularArqueo").prop("disabled", true);
	$("#btnAgregarArqueo").prop("disabled", !cierre_pendientes_bool);
}

function EvaluarConfirmarCtlArqueo() {
	AbrirMensaje("ATENCIÓN", `¿Esta seguro que desea Confirmar el Arqueo?`, function (e) {
		$("#msjModal").modal("hide");
		switch (e) {
			case "SI":
				ConfirmarCtlArqueo();
				break;
			case "NO":
				break;
			default: //NO
				break;
		}
		return true;

	}, true, ["Aceptar", "Cancelar"], "question!", null);

}

function ConfirmarCtlArqueo() {
	var data = {
		caja_nro_proceso: caja_nro_proceso_selected,
		caja_nro_cierre: caja_nro_cierre_selected,
		caja_nro_rend: caja_nro_rend_selected,
		tcf_id: tcf_id_selected
	};
	AbrirWaiting("Confirmando arqueo...");
	PostGen(data, confirmarCtlArqueoUrl, function (obj) {
		CerrarWaiting();
		if (obj.error === true || obj.warn === true) {
			AbrirMensaje("ATENCIÓN", obj.msg, function () {
				$("#msjModal").modal("hide");
				return true;
			}, false, ["Aceptar"], "error!", null);
		}
		else {
			AbrirMensaje("ATENCIÓN", "Se ha confirmado el Arqueo de forma exitosa.", function () {
				$("#msjModal").modal("hide");
				CargarGrillaVtasPVCtlCierres();
				return true;
			}, false, ["Aceptar"], "succ!", null);
		}
	});
}

function EvaluarAnularCtlArqueo() {
	AbrirMensaje("ATENCIÓN", `¿Esta seguro que desea Anular el Arqueo?`, function (e) {
		$("#msjModal").modal("hide");
		switch (e) {
			case "SI":
				AnularCtlArqueo();
				break;
			case "NO":
				break;
			default: //NO
				break;
		}
		return true;
	}, true, ["Aceptar", "Cancelar"], "question!", null);

}

function AnularCtlArqueo() {
	var data = {
		caja_nro_proceso: caja_nro_proceso_selected,
		caja_nro_cierre: caja_nro_cierre_selected,
		caja_nro_rend: caja_nro_rend_selected,
		tcf_id: tcf_id_selected
	};
	AbrirWaiting("Anulando arqueo...");
	PostGen(data, anularCtlArqueoUrl, function (obj) {
		CerrarWaiting();
		if (obj.error === true || obj.warn === true) {
			AbrirMensaje("ATENCIÓN", obj.msg, function () {
				$("#msjModal").modal("hide");
				return true;
			}, false, ["Aceptar"], "error!", null);
		}
		else {
			AbrirMensaje("ATENCIÓN", "Se ha anulado el Arqueo de forma exitosa.", function () {
				$("#msjModal").modal("hide");
				CargarGrillaVtasPVCtlRend();
				return true;
			}, false, ["Aceptar"], "succ!", null);
		}
	});
}

function EvaluarAgregarCtlArqueo() {
	AbrirWaiting();
	var datos = {};
	PostGenHtml(datos, abrirModalAgregarMedioDePagoUrl, function (obj) {
		$("#divMedioDePagoAgregar").html(obj);
		const $modal = $("#modalMedioDePagoAgregar");

		$modal.modal({
			backdrop: 'static',
		});

		$modal.modal('show');

		// Cuando el modal termina de mostrarse
		$(document).on("shown.bs.modal", "#modalImportarArchivo", function () {
		});

		$(document).on("change", "#listaMedioDePago", function () {
		});

		$(document).off("click", "#btnAceptarAgregarTipoMedioDePago");
		$(document).on("click", "#btnAceptarAgregarTipoMedioDePago", function (e) {
			EvaluarAgregarTipoMedioDePago();
		});

		CerrarWaiting();
		return true
	});
}

function EvaluarAgregarTipoMedioDePago() {
	var tipoSelected = $("#listaMedioDePago").val();
	if (tipoSelected == null || tipoSelected == undefined || tipoSelected == "") {
		AbrirMensaje("ATENCIÓN", "Debe seleccionar un Tipo de Medio de Pago válido.", function () {
			$("#msjModal").modal("hide");
			return true;
		}, false, ["Aceptar"], "error!", null);
	}
	else {
		AgregarTipoMedioDePago(tipoSelected);
	}
}

function AgregarTipoMedioDePago(tipoSelected) {
	AbrirWaiting("Agregando medio de pago...");
	var data = {
		caja_nro_proceso: caja_nro_proceso_selected,
		caja_nro_cierre: caja_nro_cierre_selected,
		caja_nro_rend: caja_nro_rend_selected,
		tcf_id: tipoSelected
	};
	PostGen(data, agregarMedioDePagoUrl, function (obj) {
		CerrarWaiting();
		if (obj.error || obj.warn) {
			AbrirMensaje("ATENCIÓN", obj.msg, function () {
				$("#msjModal").modal("hide");
			}, false, ["Aceptar"], "error!", null);
		} else {
			// 🔥 Cerrar modal solo si todo salió bien
			$("#modalMedioDePagoAgregar").modal("hide");
			CargarGrillaVtasPVCtlRend();
		}
	});
}

function ProcesarSeleccionFila($fila) {

	// Quitar selección previa
	$("#tbVtasPVCtlRend tbody tr").removeClass("selected-row");

	// Marcar fila seleccionada
	$fila.addClass("selected-row");

	// Guardar referencia a la fila actual
	fila_seleccionada_actual = $fila;

	// 🔥 Guardar valores originales ANTES de recalcular nada
	const $tds = $fila.find("td");

	$fila.data("orig-rendido", $tds.eq(2).text());
	$fila.data("orig-arqueo", $tds.eq(3).text());
	$fila.data("orig-diferencia", $tds.eq(4).text());

	// Guardar valores seleccionados
	caja_nro_rend_selected = $fila.data("caja-nro-rend");
	tcf_id_selected = $fila.data("tcf-id");
	rend_pendiente_selected = String($fila.data("rend-pendiente")).toLowerCase() === "true";

	// Habilitar / deshabilitar botones
	const habilitar = rend_pendiente_selected === true;
	$("#btnConfirmarArqueo").prop("disabled", !habilitar);
	$("#btnAnularArqueo").prop("disabled", habilitar);
	$("#btnAgregarArqueo").prop("disabled", !cierre_pendientes_bool);

	// Cargar detalle
	if (caja_nro_rend_selected) {
		CargarGrillaVtasPVCtlRendDetalle();
	}
}

function RestaurarValoresOriginalesEnPadre() {
	if (!fila_seleccionada_actual) return;

	const $fila = fila_seleccionada_actual;
	const $tds = $fila.find("td");

	const origRendido = $fila.data("orig-rendido");
	const origArqueo = $fila.data("orig-arqueo");
	const origDiferencia = $fila.data("orig-diferencia");

	if (origRendido !== undefined) $tds.eq(2).text(origRendido);
	if (origArqueo !== undefined) $tds.eq(3).text(origArqueo);
	if (origDiferencia !== undefined) $tds.eq(4).text(origDiferencia);
}


function CargarGrillaVtasPVCtlRendDetalle() {
	if (!validarRendSeleccionado()) {
		AbrirMensaje("ATENCIÓN", "Debe seleccionar un Medio de Pago.", function () {
			$("#msjModal").modal("hide");
			return;
		}, false, ["Aceptar"], "error!", null);
	}
	else {
		var data = {
			nro_proceso: caja_nro_proceso_selected,
			nro_cierre: caja_nro_cierre_selected,
			caja_nro_rend: caja_nro_rend_selected,
			tcf_id: tcf_id_selected,
			pendiente: rend_pendiente_selected
		};
		AbrirWaiting("Cargando datos de detalle de rendición de Cierre seleccionado...");
		PostGenHtml(data, obtenerDetalleDeRendDeCierreSeleccionadoUrl, function (html) {
			CerrarWaiting();
			$("#divVtasPVCtlRendDetalle").html(html);
			InicializaEventosGrillaVtasPVCtlRendDetalle();
		});
	}
}

function HabilitarBotonGuardarValores() {
	if (existe_edicion)
		return true;
	else
		return false;
}

function InicializaEventosGrillaVtasPVCtlRendDetalle() {
	if (rend_pendiente_selected === true) {
		$("#btnAgregarValor").prop("disabled", false);
	} else {
		$("#btnAgregarValor").prop("disabled", true);
	}
	$("#btnGuardarValores").prop("disabled", true);

	// Aplica máscara a todos los inputs de importe
	getMaskForMoneyType('#tbVtasPVCtlRendDetalle .input-importe');
	// (Opcional) Si querés que al hacer click se seleccione todo
	$('#tbVtasPVCtlRendDetalle').on('focus', '.input-importe', function () {
		$(this).select();
	});

	// Evitar eventos duplicados
	$(document).off("click", ".btnEditarValor");
	$(document).off("click", "#btnAgregarValor");
	$(document).off("click", "#btnGuardarValores");

	$(document).on("click", "#btnGuardarValores", function (e) {
		GuardarCtlDetalle();
	});

	$(document).on("click", "#btnAgregarValor", function (e) {
		CargaCtlNuevoItemDetalle();
	});

	// Delegación de eventos
	$(document).on("click", ".btnEditarValor", function (e) {
		e.stopPropagation(); // evita seleccionar la fila

		const $btn = $(this);

		const ins_id = $btn.data("ins-id");
		const tcf_id = $btn.data("tcf-id");
		const ins_detalle = $btn.data("ins-detalle");
		//const rend_item = $btn.data("rend-item");
		const rend_item = parseInt($btn.data("rend-item")) || 0;

		// Lógica de edición
		AbrirModalEditarValor(tcf_id, ins_id, ins_detalle, rend_item, caja_nro_proceso_selected, caja_nro_cierre_selected, caja_nro_rend_selected);
	});

	// Evitar duplicados
	$(document).off("blur", ".input-importe");
	$(document).off("keydown", ".input-importe");

	// Guardar al salir
	$(document).on("blur", ".input-importe", function () {
		const $input = $(this);
		const valorOriginal = Number($input.data("original") ?? 0);
		const nuevoValor = Number($input.inputmask("unmaskedvalue") || 0);

		// Si no cambió, no hacemos nada
		if (valorOriginal === nuevoValor) {
			return;
		}

		GuardarImporteEditado($(this));
	});

	// Seleccionar texto al recibir foco (solo si viene del mouse)
	$(document).off("focus", ".input-importe");
	$(document).on("focus", ".input-importe", function (e) {
		const $input = $(this);

		// Si el foco viene por teclado (Enter, Tab, Arrow), NO seleccionar
		if ($input.data("keyboard-nav")) {
			$input.data("keyboard-nav", false); // limpiar flag
			return;
		}

		// Si viene del mouse → seleccionar todo
		setTimeout(() => {
			$input.select();
		}, 10);
	});

	// Enter / Escape
	$(document).on("keydown", ".input-importe", function (e) {

		const $input = $(this);
		const $inputs = $("#tbVtasPVCtlRendDetalle .input-importe");
		const index = $inputs.index(this);

		const keysNext = ["Enter", "Tab", "ArrowDown"];
		const keysPrev = ["ArrowUp"];

		// ESC → cancelar edición
		if (e.key === "Escape") {
			e.preventDefault();
			CancelarEdicion($input);
			return;
		}

		// ENTER / TAB / FLECHA ABAJO → guardar + mover
		if (keysNext.includes(e.key)) {
			e.preventDefault();

			// Guardar antes de moverse
			console.log(".input-importe -> keydown -> keysNext", $input);
			GuardarImporteEditado($input);

			const nextIndex = (index + 1) % $inputs.length; // wrap-around
			const $next = $inputs.eq(nextIndex);

			$next.focus().select();
			return;
		}

		// FLECHA ARRIBA → guardar + mover hacia arriba
		if (keysPrev.includes(e.key)) {
			e.preventDefault();

			console.log(".input-importe -> keydown -> keysPrev", $input);
			GuardarImporteEditado($input);

			const prevIndex = (index - 1 + $inputs.length) % $inputs.length; // wrap-around
			const $prev = $inputs.eq(prevIndex);

			$prev.focus().select();
			return;
		}

		// Para cualquier otra tecla → no hacemos nada especial
	});

	$(document).off("click", "#tbVtasPVCtlRendDetalle tbody tr");
	$(document).on("click", "#tbVtasPVCtlRendDetalle tbody tr", function (e) {

		if ($(e.target).is("button, a, .btn, i")) return;

		const $nuevaFila = $(this);

		ProcesarSeleccionFilaRendDetalle($nuevaFila);
	});
}

function AbrirModalEditarValor(tcf_id, ins_id, ins_detalle, rend_item, nro_proceso, nro_cierre, nro_rend) {
	if (tcf_id == "") {
		AbrirMensaje("ATENCIÓN", "El Tipo Cuenta Financiero seleccionado no es válido", function () {
			$("#msjModal").modal("hide");
			return true;
		}, false, ["Aceptar"], "error!", null);
	}
	else {
		AbrirWaiting("Cargando datos...");
		var data = { tcf_id, ins_id, rend_item, nro_proceso, nro_cierre, nro_rend };
		PostGenHtml(data, obtenerPartialDeValoresUrl, function (obj) {
			$("#divEdicionDeValores").html(obj);

			getMaskForMoneyType("#Importe");
			$(document).off("click", "#btnAceptarDesdeModalValores");
			$(document).on("click", "#btnAceptarDesdeModalValores", function () {
				ConfirmarGuardarDetalleDeValor(tcf_id);
			});
			if (tcf_id == "CH") {
				InicializarCamposEnTcfId_CH();
				CargarCamposEnCH();
			}
			// Máscara de número de tarjeta SOLO si es TC
			if (tcf_id === "TC" || tcf_id === "TD") {
				getMaskForCardNumber("#NroTarjeta");
				inicializarCamposTC_TD();
				CargarCamposEnTC_TD();
			}
			if (tcf_id == "BA") {
				CargarCamposEnBA();
			}
			if (tcf_id == "MU") {
				CargarCamposEnMU();
			}
			const $modal = $("#modalEdicionDeValores");
			$modal.data("tcf-id", tcf_id);
			$modal.data("ins-id", ins_id);
			$modal.data("ins-detalle", ins_detalle);
			$modal.data("rend-item", rend_item);

			$modal.modal({
				backdrop: 'static',
			});
			// Activar navegación con ENTER
			habilitarNavegacionEnModal("#modalEdicionDeValores");
			$modal.modal('show');

			CerrarWaiting();
			return true

		});
	}
}

const camposPorModal = {
	"TC": ["MedioDePagoSeleccionado", "NroTarjeta", "Lote", "Cupon", "Importe"],
	"TD": ["MedioDePagoSeleccionado", "NroTarjeta", "Lote", "Cupon", "Importe"],

	"MU": ["MedioDePagoSeleccionado", "Titular", "NroOrden", "Cuit", "Importe"],
	"BA": ["MedioDePagoSeleccionado", "Banco", "NroCuenta", "NroDeposito", "Importe"],

	"CH": ["BcoCheqsSeleccionado", "NroCheque", "Plaza", "FechaVto", "Importe", "Rel01"]
};

const reglasValidacion = {
	"TC": ["#listaMediosDePago", "#NroTarjeta", "#Lote", "#Cupon", "#Importe"],
	"TD": ["#listaMediosDePago", "#NroTarjeta", "#Lote", "#Cupon", "#Importe"],

	"MU": ["#listaMediosDePago", "#Titular", "#NroOrden", "#Cuit", "#Importe"],
	"BA": ["#listaMediosDePago", "#NroDeposito", "#Importe"],

	"CH": ["#listaBcoCheqs", "#NroCheque", "#Plaza", "#FechaVto", "#Importe", "#Rel01"]
};

function obtenerDatosDelModal(tcf_id) {

	const campos = camposPorModal[tcf_id];
	if (!campos) return null;

	const data = {};

	campos.forEach(nombre => {
		const selector = `#${nombre}`;
		const $campo = $(selector);

		if ($campo.length > 0) {

			if (nombre === "Importe") {

				// Obtener valor unmasked desde Inputmask
				const unmasked = $campo.inputmask("unmaskedvalue");

				// Convertir a decimal real
				data[nombre] = parseFloat(unmasked || "0");

			} else {

				data[nombre] = $campo.val();
			}
		}
	});

	data["tcf_id"] = tcf_id;

	return data;
}

function convertirImporteADecimal(selector) {
	const $campo = $(selector);

	if ($campo.length === 0) return 0;

	// Usamos el método propio de Inputmask
	let valor = $campo.inputmask("unmaskedvalue");

	if (!valor || valor.trim() === "") return 0;

	return parseFloat(valor);
}

function ValidarGuardarDetalleDeValor(tcf_id) {
	const campos = reglasValidacion[tcf_id];
	if (!campos) return true; // si no hay reglas, no valida

	let valido = true;
	let primerError = null;

	campos.forEach(selector => {
		const $campo = $(selector);

		if ($campo.length === 0) return;

		const valor = ($campo.val() || "").trim();

		// Validación básica
		if (valor === "" || valor === "Seleccionar") {
			valido = false;

			$campo.addClass("is-invalid");

			if (!primerError) primerError = $campo;
		} else {
			$campo.removeClass("is-invalid");
		}
	});

	if (!valido && primerError) {
		primerError.focus();
	}

	return valido;
}

function ConfirmarGuardarDetalleDeValor(tcf_id) {
	if (!ValidarGuardarDetalleDeValor(tcf_id)) {
		AbrirMensaje("ATENCIÓN", "Existen datos incompletos, por favor revise.", function () {
			$("#msjModal").modal("hide");
			return true;
		}, false, ["Aceptar"], "error!", null);
	}
	else {
		AbrirMensaje("ATENCIÓN", `¿Esta seguro que desea guardar los datos ingresados?`, function (e) {
			$("#msjModal").modal("hide");
			switch (e) {
				case "SI":
					GuardarDetalleDeValor(tcf_id);
					break;
				case "NO":
					break;
				default: //NO
					break;
			}
			return true;
		}, true, ["Aceptar", "Cancelar"], "question!", null);
	}
}

function obtenerFechaActualISO() {
	return new Date().toISOString().split("T")[0];
}

function GuardarDetalleDeValor(tcf_id) {
	// Obtener datos del modal
	const data = obtenerDatosDelModal(tcf_id);

	if (!data) {
		AbrirMensaje("ATENCIÓN", "Se ha producido un error al intentar obtener los datos para guardar.", function () {
			$("#msjModal").modal("hide");
			return true;
		}, false, ["Aceptar"], "error!", null);
		return;
	}
	const resultadoValidacion = validarCamposObligatorios(tcf_id);
	if (!resultadoValidacion.ok) {
		AbrirMensaje("ATENCIÓN", resultadoValidacion.mensaje, function () {
			$("#msjModal").modal("hide");
			return true;
		}, false, ["Aceptar"], "error!", null);
		return;
	}
	// URL según tipo de medio de pago
	let request;
	let url = "";

	switch (tcf_id) {
		case "TC":
		case "TD":
			url = actualizarItemConceptoValorEnDetalleRendUrl;
			request = new ObjValorCorreccionVtaPV(tcf_id, data.NroTarjeta, "Tarjeta", data.Lote, "Lote", data.Cupon, "Cupon", data.Importe, obtenerFechaActualISO(), "");
			break;
		case "MU":
			url = actualizarItemConceptoValorEnDetalleRendUrl;
			request = new ObjValorCorreccionVtaPV(tcf_id, data.Titular, "Titular", data.NroOrden, "Nº Orden", data.Cuit, "CUIT", data.Importe, obtenerFechaActualISO(), "");
			break;
		case "BA":
			url = actualizarItemConceptoValorEnDetalleRendUrl;
			request = new ObjValorCorreccionVtaPV(tcf_id, data.Banco, "Banco", data.NroCuenta, "Nº Cuenta", data.NroDeposito, "Nº Depósito", data.Importe, obtenerFechaActualISO(), "");
			break;

		case "CH":
			url = actualizarItemConceptoValorEnDetalleRendUrl;
			request = new ObjValorCorreccionVtaPV(tcf_id, $("#listaBcoCheqs option:selected").text(), "Banco", data.NroCheque, "Nº Cheque", data.Plaza, "Plaza", data.Importe, data.FechaVto, $("#Rel01Item").val());
			break;

		default:
			AbrirMensaje("ATENCIÓN", "El tipo de instrumento no tiene asociado un marco de guardado.", function () {
				$("#msjModal").modal("hide");
				return true;
			}, false, ["Aceptar"], "error!", null);
			return;
	}
	const $modal = $("#modalEdicionDeValores");
	const ins_id = $modal.data("ins-id");
	const ins_detalle = $modal.data("ins-detalle");
	const rend_item = $modal.data("rend-item");

	var dataToSend = {
		caja_nro_proceso: caja_nro_proceso_selected,
		caja_nro_cierre: caja_nro_cierre_selected,
		caja_nro_rend: caja_nro_rend_selected,
		tcf_id: tcf_id,
		ins_id: ins_id,
		ins_detalle: ins_detalle,
		rend_item: rend_item,
		detalle: request
	};
	AbrirWaiting("Actualizando valor...");
	PostGen(dataToSend, url, function (obj) {
		CerrarWaiting();
		if (obj.error === true || obj.warn === true) {
			AbrirMensaje("ATENCIÓN", obj.msg, function () {
				$("#msjModal").modal("hide");
				existe_edicion = false;
				return true;
			}, false, ["Aceptar"], "error!", null);
		}
		else {
			// Actualizar concepto_valor en la grilla
			const selectorFila = `tr.row-pedido[data-ins-id="${ins_id}"][data-rend-item="${rend_item}"]`;
			const $fila = $(selectorFila);

			if ($fila.length > 0) {
				//1 Actualizar concepto_valor en la fila
				$fila.find("td.col-valores").text(obj.concepto);
			}
			// 2) Actualizar importe editable
			const $inputImporte = $fila.find("td.editable-importe .input-importe");

			if ($inputImporte.length > 0) {

				// Valor decimal real que viene del modal
				const nuevoImporte = data.Importe;

				// Formatear como lo muestra la grilla (ej: 1.234,56)
				const importeFormateado = FormatearPrecio(nuevoImporte);

				// Remover máscara ANTES de setear el valor
				$inputImporte.inputmask("remove");

				// Setear el valor formateado
				$inputImporte.val(importeFormateado);

				// Actualizar data-original
				$inputImporte.data("original", nuevoImporte.toString());

				// Volver a aplicar la máscara correctamente
				//getMaskForMoneyType($inputImporte.selector);
				getMaskForMoneyType($inputImporte);

				// 🔥 3) ACTUALIZAR DIFERENCIA
				const textoArqueo = $fila.find("td.col-num").eq(1).text().trim();
				const importeArqueo = parseFloat($fila.find("td.col-num").eq(1).data("arqueo")) || 0;
				//FormatearPrecio
				//const arqueo = convertirImporteADecimal(textoArqueo);
				
				const arqueo = toDecimalSafe(textoArqueo);
				const diferencia = nuevoImporte - importeArqueo;

				const $celdaDif = $fila.find("td.col-num").eq(2);
				$celdaDif.text(FormatearPrecio(diferencia));
			}

			existe_edicion = true;
			$("#modalEdicionDeValores").modal("hide");
			$("#btnGuardarValores").prop("disabled", !HabilitarBotonGuardarValores());
			ActualizarTotalesEnPadre();
		}
	});
}

function validarCamposObligatorios(tcf_id) {

	const campos = reglasValidacion[tcf_id];
	if (!campos) return true;

	for (const selector of campos) {

		const $campo = $(selector);
		if ($campo.length === 0) continue;

		const valor = ($campo.val() || "").trim();

		// Validación básica
		if (valor === "" || valor === "Seleccionar") {
			marcarError($campo);
			return { ok: false, mensaje: `El campo ${obtenerNombreCampo(selector)} es obligatorio` };
		}

		// Validación CUIT
		if (selector === "#Cuit" && !validarCUIT(valor)) {
			marcarError($campo);
			return { ok: false, mensaje: "El CUIT ingresado no es válido" };
		}

		// Validación tarjeta (Luhn)
		if (selector === "#NroTarjeta" && valor === "") {
			marcarError($campo);
			return false;
		}
		//if (selector === "#NroTarjeta" && !validarTarjetaLuhn(valor)) {
		//	marcarError($campo);
		//	return { ok: false, mensaje: "El número de tarjeta no es válido" };
		//}

		// Validación fecha cheque
		if (selector === "#FechaVto" && !validarFechaCheque(valor)) {
			marcarError($campo);
			return { ok: false, mensaje: "La fecha de vencimiento no puede ser pasada" };
		}
	}

	return { ok: true, mensaje: "" };

	function marcarError($c) {
		$c.focus(); // 🔥 foco inmediato
	}
}

function obtenerNombreCampo(selector) {
	switch (selector) {
		case "#listaMediosDePago": return "Medio de Pago";
		case "#NroTarjeta": return "Número de Tarjeta";
		case "#Lote": return "Lote";
		case "#Cupon": return "Cupón";
		case "#Importe": return "Importe";
		case "#Titular": return "Titular";
		case "#NroOrden": return "Número de Orden";
		case "#Cuit": return "CUIT";
		case "#listaBcoCheqs": return "Banco";
		case "#NroCheque": return "Número de Cheque";
		case "#Plaza": return "Plaza";
		case "#FechaVto": return "Fecha de Vencimiento";
		case "#Rel01": return "Cliente";
		default: return selector;
	}
}

function validarCUIT(cuit) {
	cuit = cuit.replace(/\D/g, "");

	if (cuit.length !== 11) return false;

	const coef = [5, 4, 3, 2, 7, 6, 5, 4, 3, 2];
	let suma = 0;

	for (let i = 0; i < 10; i++) {
		suma += parseInt(cuit[i]) * coef[i];
	}

	let resto = suma % 11;
	let digito = resto === 0 ? 0 : resto === 1 ? 9 : 11 - resto;

	return digito === parseInt(cuit[10]);
}

function validarTarjetaLuhn(numero) {
	numero = numero.replace(/\D/g, "");

	let suma = 0;
	let alternar = false;

	for (let i = numero.length - 1; i >= 0; i--) {
		let n = parseInt(numero[i]);

		if (alternar) {
			n *= 2;
			if (n > 9) n -= 9;
		}

		suma += n;
		alternar = !alternar;
	}

	return (suma % 10) === 0;
}

function validarFechaCheque(fechaStr) {
	if (!fechaStr) return false;

	const hoy = new Date();
	hoy.setHours(0, 0, 0, 0);

	const fecha = new Date(fechaStr);
	fecha.setHours(0, 0, 0, 0);

	return fecha >= hoy;
}


function habilitarNavegacionEnModal(modalSelector) {

	const selectorFocusables = `
        ${modalSelector} input:visible:not([disabled]):not([readonly]),
        ${modalSelector} select:visible:not([disabled]):not([readonly]),
        ${modalSelector} textarea:visible:not([disabled]):not([readonly]),
        ${modalSelector} button:visible:not([disabled])
    `;

	$(document).on("keydown", selectorFocusables, function (e) {

		const $modal = $(modalSelector);

		const $elements = $modal
			.find("input:visible:not([disabled]):not([readonly]), select:visible:not([disabled]):not([readonly]), textarea:visible:not([disabled]):not([readonly]), button:visible:not([disabled])");

		const index = $elements.index(this);

		// ENTER o TAB → siguiente
		if (e.key === "Enter" || (e.key === "Tab" && !e.shiftKey)) {
			e.preventDefault();

			if (index < $elements.length - 1) {
				$elements.eq(index + 1).focus();
			} else {
				// Último → ejecutar Aceptar
				$modal.find("#btnAceptarDesdeModalValores").trigger("click");
			}
		}

		// SHIFT+ENTER o SHIFT+TAB → anterior
		if ((e.key === "Enter" && e.shiftKey) || (e.key === "Tab" && e.shiftKey)) {
			e.preventDefault();

			if (index > 0) {
				$elements.eq(index - 1).focus();
			}
		}
	});
}

function habilitarNavegacionConEnterEnModal(modalSelector) {

	const focusables = `${modalSelector} input, 
                        ${modalSelector} select, 
                        ${modalSelector} textarea, 
                        ${modalSelector} button`;

	$(document).on("keydown", focusables, function (e) {

		if (e.key === "Enter") {
			e.preventDefault();

			const $modal = $(modalSelector);

			// Todos los elementos focusables visibles y habilitados
			const $elements = $modal
				.find("input:visible:not([disabled]), select:visible:not([disabled]), textarea:visible:not([disabled]), button:visible:not([disabled])")
				.filter(":not([readonly])");

			const index = $elements.index(this);

			// Si existe siguiente → focus
			if (index >= 0 && index < $elements.length - 1) {
				$elements.eq(index + 1).focus();
			} else {
				// Si no hay más → presionar Aceptar
				$modal.find("#btnAceptarDesdeModalValores").trigger("click");
			}
		}
	});
}

function inicializarCamposTC_TD() {

	// Aplicar máscara numérica estricta
	$("#Lote, #Cupon").inputmask({
		mask: "999999",
		placeholder: "",
		showMaskOnHover: false,
		showMaskOnFocus: true,
		clearIncomplete: false,
		rightAlign: false
	});

	// Al salir del campo → completar con ceros
	$(document).off("blur", "#Lote, #Cupon");
	$(document).on("blur", "#Lote, #Cupon", function () {
		let val = $(this).val().replace(/\D/g, "");
		$(this).val(padLeftZeros(val, 6));
	});
}

function CargarCamposEnCH() {
	//N° Cheque
	const rend_dato1_valor = $("#rend_dato1_valor").val() || "";
	//Plaza
	const rend_dato2_valor = $("#rend_dato2_valor").val() || "";
	//Fecha Vto.
	const rend_dato3_valor = $("#rend_dato3_valor").val() || "";
	//Importe
	const rend_importe_ok = $("#rend_importe_ok").val() || 0;
	//Instrumento
	const ins_id = $("#ins_id").val() || "";

	if (rend_dato1_valor != "") {
		$("#NroCheque").val(rend_dato1_valor);
	}
	if (rend_dato2_valor != "") {
		$("#Plaza").val(padLeftZeros(rend_dato2_valor, 6));
	}
	if (rend_dato3_valor != "") {
		$("#FechaVto").val(padLeftZeros(rend_dato3_valor, 6));
	}
	if (rend_importe_ok > 0) {
		$("#Importe").val(FormatearPrecio(rend_importe_ok));
		getMaskForMoneyType($("#Importe"));
	}
	if (ins_id != "") {
		$("#listaMediosDePago").val(ins_id);
	}
}

function CargarCamposEnMU() {
	//Titular
	const rend_dato1_valor = $("#rend_dato1_valor").val() || "";
	//Nro. Orden
	const rend_dato2_valor = $("#rend_dato2_valor").val() || "";
	//Cuit
	const rend_dato3_valor = $("#rend_dato3_valor").val() || "";
	//Importe
	const rend_importe_ok = $("#rend_importe_ok").val() || 0;
	//Instrumento
	const ins_id = $("#ins_id").val() || "";

	if (rend_dato1_valor != "") {
		$("#Titular").val(rend_dato1_valor);
	}
	if (rend_dato2_valor != "") {
		$("#NroOrden").val(padLeftZeros(rend_dato2_valor, 6));
	}
	if (rend_dato3_valor != "") {
		$("#Cuit").val(padLeftZeros(rend_dato3_valor, 6));
	}
	if (rend_importe_ok > 0) {
		$("#Importe").val(FormatearPrecio(rend_importe_ok));
		getMaskForMoneyType($("#Importe"));
	}
	if (ins_id != "") {
		$("#listaMediosDePago").val(ins_id);
	}
}

function CargarCamposEnBA() {
	//Banco
	const rend_dato1_valor = $("#rend_dato1_valor").val() || "";
	//N°. Cuenta
	const rend_dato2_valor = $("#rend_dato2_valor").val() || "";
	//N°. Depósito
	const rend_dato3_valor = $("#rend_dato3_valor").val() || "";
	//Importe
	const rend_importe_ok = $("#rend_importe_ok").val() || 0;
	//Instrumento
	const ins_id = $("#ins_id").val() || "";

	if (rend_dato1_valor != "") {
		$("#Banco").val(rend_dato1_valor);
	}
	if (rend_dato2_valor != "") {
		$("#NroCuenta").val(padLeftZeros(rend_dato2_valor, 6));
	}
	if (rend_dato3_valor != "") {
		$("#NroDeposito").val(padLeftZeros(rend_dato3_valor, 6));
	}
	if (rend_importe_ok > 0) {
		$("#Importe").val(FormatearPrecio(rend_importe_ok));
		getMaskForMoneyType($("#Importe"));
	}
	if (ins_id != "") {
		$("#listaMediosDePago").val(ins_id);
	}
}

function CargarCamposEnTC_TD() { 
	//Tarjeta
	const rend_dato1_valor = $("#rend_dato1_valor").val() || "";
	//Lote
	const rend_dato2_valor = $("#rend_dato2_valor").val() || "";
	//Cupón
	const rend_dato3_valor = $("#rend_dato3_valor").val() || "";
	//Importe
	const rend_importe_ok = $("#rend_importe_ok").val() || 0;
	//Instrumento
	const ins_id = $("#ins_id").val() || "";

	if (rend_dato1_valor != "") {
		$("#NroTarjeta").val(rend_dato1_valor);
		getMaskForCardNumber($("#NroTarjeta")); 
	}
	if (rend_dato2_valor != "") {
		$("#Lote").val(padLeftZeros(rend_dato2_valor, 6));
	}
	if (rend_dato3_valor != "") {
		$("#Cupon").val(padLeftZeros(rend_dato3_valor, 6));
	}
	if (rend_importe_ok > 0) {
		$("#Importe").val(FormatearPrecio(rend_importe_ok)); 
		getMaskForMoneyType($("#Importe"));
	}
	if (ins_id != "") {
		$("#listaMediosDePago").val(ins_id);
	}
}

function padLeftZeros(value, length) {
	return value.toString().padStart(length, "0");
}

function InicializarCamposEnTcfId_CH() {
	$(document).on("keydown.autocomplete", "input#Rel01", function () {
		$(this).autocomplete({
			source: function (request, response) {

				data = { prefix: request.term };

				$.ajax({
					url: autoComRel01Url,
					type: "POST",
					dataType: "json",
					data: data,
					success: function (obj) {
						response($.map(obj, function (item) {
							var texto = item.descripcion;
							return { label: texto, value: item.descripcion, id: item.id, prov: item.provId };
						}));
					}
				})
			},
			minLength: 3,
			select: function (event, ui) {
				clienteIdSelected = ui.item.id;
				clienteDescSelected = ui.item.value;

				return true;
			}
		});
	});

	$(document).off("change", "#listaBcoCheqs");
	$(document).on("change", "#listaBcoCheqs", function () {
		const bc_id = $(this).val();
		ActualizarValorCampoPlaza(bc_id);
	});

	// 🔥 Corregir fecha si viene vacía o inválida
	const $fecha = $("#FechaVto");
	if (!$fecha.val() || $fecha.val() === "0001-01-01") {
		const hoy = new Date().toISOString().split("T")[0];
		$fecha.val(hoy);
	}

	// Aplicar máscara numérica estricta
	$("#NroCheque").inputmask({
		mask: "99999999",
		placeholder: "",
		showMaskOnHover: false,
		showMaskOnFocus: true,
		clearIncomplete: false,
		rightAlign: false
	});

	// Al salir del campo → completar con ceros
	$(document).on("blur", "#NroCheque", function () {
		let val = $(this).val().replace(/\D/g, "");
		$(this).val(padLeftZeros(val, 8));
	});
}

function ActualizarValorCampoPlaza(bc_id) {

	if (!bc_id || bc_id === "Seleccionar") {
		$("#Plaza").val("");
		return;
	}

	const data = { bc_id: bc_id };

	PostGen(data, urlObtenerPlazaPorBanco, function (resp) {

		if (resp && resp.ok && resp.data) {
			$("#Plaza").val(resp.data.bc_plaza);
		} else {
			$("#Plaza").val("");
			AbrirMensaje("Atención", "No se pudo obtener la plaza del banco seleccionado.");
		}
	});
}

function GuardarCtlDetalle() {
	var caja_nro_proceso = caja_nro_proceso_selected;
	var caja_nro_cierre = caja_nro_cierre_selected;
	var caja_nro_rend = caja_nro_rend_selected;
	var tcf_id = tcf_id_selected;
	var data = { caja_nro_proceso, caja_nro_cierre, caja_nro_rend, tcf_id }
	AbrirWaiting("Guardando datos de Detalle de Arqueo...")
	PostGen(data, guardarCtlDetalleUrl, function (obj) {
		CerrarWaiting();
		if (obj.error === true || obj.warn === true) {
			AbrirMensaje("ATENCIÓN", obj.msg, function () {
				$("#msjModal").modal("hide");
				existe_edicion = false;
				CargarGrillaVtasPVCtlRend();
				CargarGrillaVtasPVCtlRendDetalle();
				return true;
			}, false, ["Aceptar"], "error!", null);
		}
		else {
			AbrirMensaje("ATENCIÓN", "Se han actualizado los datos del Detalle de Arqueo de forma exitosa.", function () {
				$("#msjModal").modal("hide");
				existe_edicion = false;
				CargarGrillaVtasPVCtlRendDetalle();
				return true;

			}, false, ["Aceptar"], "succ!", null);
		}
	});
}

function CargaCtlNuevoItemDetalle() {
	var data = {
		caja_nro_proceso: caja_nro_proceso_selected,
		caja_nro_cierre: caja_nro_cierre_selected,
		caja_nro_rend: caja_nro_rend_selected,
	};
	AbrirWaiting("Agregando nuevo registro...");
	PostGen(data, cargaCtlNuevoItemDetalleUrl, function (obj) {
		CerrarWaiting();
		if (obj.error === true || obj.warn === true) {
			AbrirMensaje("ATENCIÓN", obj.msg, function () {
				$("#msjModal").modal("hide");
				return true;
			}, false, ["Aceptar"], "error!", null);
		}
		else {
			//Actualizamos la tabla de detale de rend.
			CargarGrillaVtasPVCtlRendDetalle();
		}
	});
}

function ProcesarSeleccionFilaRendDetalle($fila) {
	// Quitar selección previa
	$("#tbVtasPVCtlRendDetalle tbody tr").removeClass("selected-row");

	// Marcar fila seleccionada
	$fila.addClass("selected-row");
}

function RecalcularDiferenciaEnFila($input) {

	const $td = $input.closest("td");
	const $tr = $td.closest("tr");

	// Obtener importe OK (editado)
	const importeOk = parseFloat($input.inputmask("unmaskedvalue")) || 0;

	// Importe Arqueo REAL desde data-arqueo
	const importeArqueo = parseFloat($tr.find("td").eq(2).data("arqueo")) || 0;

	// Calcular diferencia
	const dif = importeOk - importeArqueo;

	// Formatear diferencia
	const difFormateado = dif.toLocaleString("en-US", {
		minimumFractionDigits: 2,
		maximumFractionDigits: 2
	});

	// Actualizar celda Dif (columna 3)
	const $tdDif = $tr.find("td").eq(3);
	$tdDif.text(difFormateado);
	$tdDif.data("diferencia", dif); // 🔥 valor real
}

function GuardarImporteEditado($input) {
	// Si ya estamos guardando, NO volver a entrar
	if (guardando_importe) {
		return;
	}
	guardando_importe = true;

	const $td = $input.closest("td");
	const valorOriginal = $input.data("original");
	const nuevoValor = $input.inputmask("unmaskedvalue");

	if (nuevoValor === "" || isNaN(nuevoValor)) {
		CancelarEdicion($input);
		return;
	}

	AbrirWaiting("Guardando importe...");
	var data = { ins_id: $td.data("ins-id"), importe: nuevoValor };
	PostGen(data, actualizarImporteEnItemDeDetalleDeArqueoUrl, function (obj) {
		CerrarWaiting();
		if (obj.error === true) {
			AbrirMensaje("ATENCIÓN", obj.msg, function () {
				$("#msjModal").modal("hide");
				CancelarEdicion($input);
				return true;
			}, false, ["Aceptar"], "error!", null);
		}
		else {
			// Recalcular diferencia en la fila
			RecalcularDiferenciaEnFila($input);
			ActualizarTotalesEnPadre();
			existe_edicion = true;
			$("#btnGuardarValores").prop("disabled", !existe_edicion);

			// 🔥 IMPORTANTE: actualizar el valor original
			$input.data("original", nuevoValor);
			guardando_importe = false;
		}
	});
}

function toDecimalSafe(valor) {
	if (!valor) return 0;

	return Number(
		valor
			.toString()
			.trim()
			.replace(/\./g, "")
			.replace(",", ".")
	) || 0;
}

function ActualizarTotalesEnPadre() {
	const $filas = $("#tbVtasPVCtlRendDetalle tbody tr").not(".fila-vacia");

	let totalRendido = 0;
	let totalArqueo = 0;
	let totalDiferencia = 0;

	$filas.each(function () {
		const $tr = $(this);

		// --- RENDIDO ---
		let rendido = 0;
		const $tdRendido = $tr.find("td.editable-importe");

		if ($tdRendido.find("input").length) {
			rendido = parseFloat($tdRendido.find("input").inputmask("unmaskedvalue")) || 0;
		} else {
			rendido = parseFloat($tdRendido.data("rendido")) || 0;
		}

		// --- ARQUEO ---
		const arqueo = parseFloat($tr.find("td").eq(2).data("arqueo")) || 0;

		// --- DIFERENCIA ---
		const diferencia = parseFloat($tr.find("td").eq(3).data("diferencia")) || 0;

		totalRendido += rendido;
		totalArqueo += arqueo;
		totalDiferencia += diferencia;
	});

	if (fila_seleccionada_actual) {
		const $tds = fila_seleccionada_actual.find("td");
		$tds.eq(2).text(FormatearPrecio(totalRendido));
		$tds.eq(3).text(FormatearPrecio(totalArqueo));
		$tds.eq(4).text(FormatearPrecio(totalRendido - totalArqueo));
	}
}


function FormatearPrecio(valor) {
	return Number(valor).toLocaleString("en-US", {
		minimumFractionDigits: 2,
		maximumFractionDigits: 2
	});
}

function CancelarEdicion($input) {
	const original = $input.data("original");
	$input.val(original);
}

function validarRendSeleccionado() {
	if (caja_nro_rend_selected == null || caja_nro_rend_selected == undefined || caja_nro_rend_selected == "")
		return false;
	if (tcf_id_selected == null || tcf_id_selected == undefined || tcf_id_selected == "")
		return false;
	return true;
}

function validarCierreSeleccionado() {
	if (caja_nro_proceso_selected == null || caja_nro_proceso_selected == undefined || caja_nro_proceso_selected == "")
		return false;
	if (caja_nro_cierre_selected == null || caja_nro_cierre_selected == undefined || caja_nro_cierre_selected == "")
		return false;
	return true;
}

function getMaskForMoneyType(selector) {
	$(selector).inputmask({
		alias: 'numeric',
		groupSeparator: ',',
		radixPoint: '.',
		digits: 2,
		digitsOptional: false,
		allowMinus: false,
		prefix: '',
		suffix: '',
		rightAlign: true,
		unmaskAsNumber: true
	});
}

function getMaskForCardNumber(selector) {
	$(selector).inputmask({
		mask: "99999999",
		placeholder: " ",
		showMaskOnHover: false,
		showMaskOnFocus: true,
		clearIncomplete: true
	});
}


class ObjValorCorreccionVtaPV {
	constructor(tcf_id, op_dato1_valor, op_dato1_desc, op_dato2_valor, op_dato2_desc, op_dato3_valor, op_dato3_desc, op_importe, op_fecha_valor, cta_id) {
		this.tcf_id = tcf_id;
		this.op_dato1_valor = op_dato1_valor;
		this.op_dato1_desc = op_dato1_desc;
		this.op_dato2_valor = op_dato2_valor;
		this.op_dato2_desc = op_dato2_desc;
		this.op_dato3_valor = op_dato3_valor;
		this.op_dato3_desc = op_dato3_desc;
		this.op_fecha_valor = op_fecha_valor;
		this.op_importe = op_importe;
		this.cta_id = cta_id;
	}
};