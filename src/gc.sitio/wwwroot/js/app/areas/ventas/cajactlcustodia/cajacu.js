var fila_entrega_seleccionada_actual = null;
var ent_compte_selected = null;
var ent_estado_selected = null;
var ent_actu_selected = null;
var ent_actu_bool_selected = null;
var ent_tcf_id_selected = null;
var guardando_importe = false;
var existe_edicion = false;

$(function () {
	if ($("#divDetalle").is(":visible")) {
		$("#divDetalle").collapse("hide");
	}
	$("#divFiltro").collapse("show");

	$("#btnCancel").on("click", function () {
		window.location.href = homeCtlValoresUrl;
	});

	$("#lbSucursales").text("Sucursal");

	$("#btnBuscar").on("click", function () {
		if (validarCamposSeleccionados()) {
			InicializarBusqueda();
		} else {
			AbrirMensaje("ATENCIÓN", "Debe seleccionar Sucursal.", function () {
				$("#msjModal").modal("hide");
				return true;
			}, false, ["Aceptar"], "error!", null);
		}
	});

	$("#chkSucursales").prop("checked", true);
	$("#chkSucursales").prop("disabled", true);
	$("#chkSucursales").trigger('change');
	$("#listaSucursales").prop("disabled", false);
});

function validarCamposSeleccionados() {
	let sucSeleccionada = $("#listaSucursales").val();
	if (sucSeleccionada == null || sucSeleccionada == undefined || sucSeleccionada == "")
		return false;
	return true;
}

function InicializarBusqueda() {
	sucDesc_selected = $("#listaSucursales").find("option:selected").text();
	sucId_selected = $("#listaSucursales").find("option:selected").val();
	const tipoEntrega = $("input[name='TipoEntrega']:checked").val();
	var data = { admDesc: sucDesc_selected, admId: sucId_selected, tipo: tipoEntrega };
	AbrirWaiting("Cargando datos de valores en custodia...");
	PostGenHtml(data, cargarDatosDeValoresUrl, function (html) {
		$("#divDetalle").html(html);
		$("#divFiltro").collapse("hide");
		$("#divDetalle").collapse("show");
		InicializaEventosGrillaVtasPVCtlEntregas();
		CerrarWaiting();
	});
}

function InicializaEventosGrillaVtasPVCtlEntregas() {
	$(document).off("change", "#chkSelectAll");
	$(document).off("change", ".chkRow");

	// Seleccionar / deseleccionar todos
	$(document).on("change", "#chkSelectAll", function () {
		const checked = $(this).is(":checked");
		$(".chkRow").prop("checked", checked);
	});

	// Actualizar el checkbox del header según las filas
	$(document).on("change", ".chkRow", function () {
		const total = $(".chkRow").length;
		const marcados = $(".chkRow:checked").length;

		$("#chkSelectAll").prop("checked", total === marcados);
	});

	$(document).off("click", "#tbVtasPVCtlEntrega tbody tr");
	$(document).on("click", "#tbVtasPVCtlEntrega tbody tr", function (e) {

		if ($(e.target).is("button, a, .btn, i")) return;

		const $nuevaFila = $(this);
		// Si ya había una fila seleccionada y se intenta cambiar
		if (fila_entrega_seleccionada_actual && fila_entrega_seleccionada_actual[0] !== $nuevaFila[0]) {
			if (existe_edicion === true) {
				AbrirMensaje("ATENCIÓN", "Tiene cambios sin guardar en la grilla de entregas. Si cambia de entrega perderá los cambios realizados. ¿Desea continuar?", function (e) {
					$("#msjModal").modal("hide");
					switch (e) {
						case "SI":
							existe_edicion = false; // Se descartan cambios
							ProcesarSeleccionFilaEntrega($nuevaFila);
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

		ProcesarSeleccionFilaEntrega($nuevaFila);
	});

	const $btn = $("#btnConfirmacionContable");
	var tipoRend = $("#TipoEntrega").val();
	if (tipoRend === "P") {
		$btn.html('<i class="bx bx-check"></i> Confirmación Entregas Seleccionadas');
	}
	else {
		$btn.html('<i class="bx bx-check"></i> Volver a Pendiente Entregas Seleccionadas');
	}

	$(document).off("click", "#btnConfirmacionContable");
	$(document).on("click", "#btnConfirmacionContable", function (e) {
		ConfirmacionContable();
	});

	SeleccionarPrimeraFilaEntregas();
}

function ConfirmacionContable() {
	var listaEntregas = obtenerEntregasSeleccionadasString();
	if (listaEntregas.length === 0) {
		AbrirMensaje("ATENCIÓN", "Debe seleccionar al menos una Entrega para confirmar.", function () {
			$("#msjModal").modal("hide");
			return true;
		}, false, ["Aceptar"], "error!", null);
		return;
	}
	else {
		var mensaje = "";
		var mensajeEnCurso = "";
		var url = "";
		var tipoRend = $("#TipoEntrega").val();
		if (tipoRend === "P") {
			mensaje = "Esta a punto de confirmar las entregas seleccionadas. ¿Desea continuar?";
			mensajeEnCurso = "Confirmando Entregas seleccionadas...";
			url = confirmarCtlEntregaUrl;
		}
		else {
			mensaje = "Esta a punto de volver a pendiente las entregas seleccionadas. ¿Desea continuar?";
			mensajeEnCurso = "Volviendo a pendiente las Entregas seleccionadas...";
			url = anularCtlEntregaUrl;
		}
		AbrirMensaje("ATENCIÓN", mensaje, function (e) {
			$("#msjModal").modal("hide");
			switch (e) {
				case "SI":
					var data = { ent_comptes: listaEntregas.join(";") }
					AbrirWaiting(mensajeEnCurso);
					PostGen(data, url, function (obj) {
						CerrarWaiting();
						if (obj.error === true || obj.warn === true) {
							AbrirMensaje("ATENCIÓN", obj.msg, function () {
								$("#msjModal").modal("hide");
								return true;
							}, false, ["Aceptar"], "error!", null);
						}
						else {
							AbrirMensaje("ATENCIÓN", obj.msg, function () {
								$("#msjModal").modal("hide");
								// 🔥 Redirigir al Index del módulo
								window.location.href = homeCtlCustodiaUrl;
								return true;
							}, false, ["Aceptar"], "succ!", null);
							
						}
					});
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

function GuardarCtlDetalle() {
	AbrirMensaje("ATENCIÓN", "Esta a punto de confirmar los cambios. ¿Desea continuar?", function (e) {
		$("#msjModal").modal("hide");
		switch (e) {
			case "SI":
				var tcf_id = ent_tcf_id_selected;
				var data = { tcf_id }
				AbrirWaiting("Guardando datos de Entrega...")
				PostGen(data, guardarCtlDetalleUrl, function (obj) {
					CerrarWaiting();
					if (obj.error === true || obj.warn === true) {
						let resumen = obj.msg;
						if (obj.fallidos && obj.fallidos.length > 0) {
							resumen = GenerarResumenErroresConDetalles(obj.fallidos);
						}
						AbrirMensaje("ATENCIÓN", resumen, function () {
							$("#msjModal").modal("hide");
							existe_edicion = false;
							InicializarBusqueda();
							return true;
						}, false, ["Aceptar"], "error!", null);
					}
					else {
						existe_edicion = false;
						InicializarBusqueda();
					}
				});
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
	let resumen = "Algunas rendiciones no pudieron guardarse:\n\n";

	Object.entries(grupos).forEach(([mensaje, info]) => {
		resumen += `• ${mensaje} (${info.cantidad})\n`;

		info.ids.forEach(id => {
			resumen += `   - Cierre ${id}\n`;
		});

		resumen += "\n";
	});

	return resumen;
}

function MoverCtlDetalle() {
	var ent_compte = $("#listaEntregas").val();
	var $fila = $("#tbVtasPVCtlEntregaRend tbody tr.selected-row");
	if (!ent_compte || ent_compte === "") {
		AbrirMensaje("ATENCIÓN", "Debe seleccionar una Entrega para mover.", function () {
			$("#msjModal").modal("hide");
			$("#listaEntregas").trigger('focus');
			return true;
		}, false, ["Aceptar"], "error!", null);
		return;
	}
	else if ($fila.length === 0) {
		AbrirMensaje("ATENCIÓN", "Debe seleccionar un elemento de Rendiciones.", function () {
			$("#msjModal").modal("hide");
			return true;
		}, false, ["Aceptar"], "error!", null);
		return;
	}
	else {

		AbrirMensaje("ATENCIÓN", `Esta a punto de mover las rendiciones a la entrega ${ent_compte}. ¿Desea continuar?`, function (e) {
			$("#msjModal").modal("hide");
			switch (e) {
				case "SI":
					var caja_nro_proceso = $fila.data("caja-nro-proceso");
					var caja_nro_cierre = $fila.data("caja-nro-cierre");
					var caja_nro_rend = $fila.data("caja-nro-rend");
					var rend_item = $fila.data("rend-item");
					var tcf_id = ent_tcf_id_selected;
					var data = { ent_compte, tcf_id, caja_nro_proceso, caja_nro_cierre, caja_nro_rend, rend_item }
					AbrirWaiting("Moviendo Entrega seleccionada...")
					PostGen(data, moverCtlDetalleUrl, function (obj) {
						CerrarWaiting();
						if (obj.error === true || obj.warn === true) {
							AbrirMensaje("ATENCIÓN", obj.msg, function () {
								$("#msjModal").modal("hide");
								InicializarBusqueda();
								return true;
							}, false, ["Aceptar"], "error!", null);
						}
						else {
							AbrirMensaje("ATENCIÓN", "Se ha movido la Entrega seleccionada de forma exitosa.", function () {
								$("#msjModal").modal("hide");
								InicializarBusqueda();
								return true;
							}, false, ["Aceptar"], "succ!", null);
						}
					});
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

function SeleccionarPrimeraFilaEntregas() {
	const $filas = $("#tbVtasPVCtlEntrega tbody tr").not(".fila-vacia");

	if ($filas.length === 0) return;

	const $primera = $filas.first();

	// Guardar referencia
	fila_cierre_seleccionada_actual = $primera;

	// Marcar visualmente
	$("#tbVtasPVCtlEntrega tbody tr").removeClass("selected-row");
	$primera.addClass("selected-row");

	// Ejecutar la lógica normal de selección
	ProcesarSeleccionFilaEntrega($primera);
}

function ProcesarSeleccionFilaEntrega($fila) {

	// Quitar selección previa
	$("#tbVtasPVCtlEntrega tbody tr").removeClass("selected-row");

	// Marcar fila seleccionada
	$fila.addClass("selected-row");

	// Guardar referencia
	fila_entrega_seleccionada_actual = $fila;

	// Guardar valores seleccionados
	ent_compte_selected = $fila.data("ent-compte");
	ent_estado_selected = $fila.data("ent-estado");
	ent_actu_selected = $fila.data("ent-actu");
	ent_actu_bool_selected = $fila.data("ent-actu-bool");
	ent_tcf_id_selected = $fila.data("tcf-id");

	// Habilitar / deshabilitar botón
	////const habilitar = (ent_actu_bool_selected === true || ent_actu_bool_selected === "true" || ent_actu_bool_selected === "True");
	////$("#btnConfirmacionContable").prop("disabled", habilitar);

	// Cargar grilla de rendiciones
	if (ent_compte_selected) {
		CargarGrillaVtasPVCtlEntregaRend();
	}
}

function CargarGrillaVtasPVCtlEntregaRend() {
	if (!validarEntregaSeleccionada()) {
		AbrirMensaje("ATENCIÓN", "Debe seleccionar una Entrega.", function () {
			$("#msjModal").modal("hide");
			return;
		}, false, ["Aceptar"], "error!", null);
	}
	else {
		var data = { ent_compte: ent_compte_selected };
		AbrirWaiting("Cargando datos de rendición de Entrega seleccionada...");
		PostGenHtml(data, obtenerRendDeEntregaSeleccionadaUrl, function (html) {
			CerrarWaiting();
			$("#divVtasPVCtlRend").html(html);
			InicializaEventosGrillaVtasPVCtlRend();
		});
	}
}

function InicializaEventosGrillaVtasPVCtlRend() {
	var tipoRend = $("#TipoEntrega").val();
	if (tipoRend === "P") {
		$("#divOpcionesEntregaRend").collapse("show");
		CargarListaEntregasParaCambioDeRendicion();
	}
	else {
		$("#divOpcionesEntregaRend").collapse("hide");
		console.log("TipoEntrega:", $("#TipoEntrega").val());
	}
	getMaskForMoneyType('#tbVtasPVCtlEntregaRend .input-importe');
	// (Opcional) Si querés que al hacer click se seleccione todo
	$('#tbVtasPVCtlRendDetalle').on('focus', '.input-importe', function () {
		$(this).select();
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
		const $inputs = $("#tbVtasPVCtlEntregaRend .input-importe");
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

	$(document).off("click", "#tbVtasPVCtlEntregaRend tbody tr");
	$(document).on("click", "#tbVtasPVCtlEntregaRend tbody tr", function (e) {

		if ($(e.target).is("button, a, .btn, i")) return;

		const $nuevaFila = $(this);

		ProcesarSeleccionFilaRendDetalle($nuevaFila);
	});

	$(document).off("click", "#btnGuardarCambios");
	$(document).on("click", "#btnGuardarCambios", function (e) {
		GuardarCtlDetalle();
	});

	$(document).off("click", "#btnMoverEntrega");
	$(document).on("click", "#btnMoverEntrega", function (e) {
		MoverCtlDetalle();
	});
}

function CargarListaEntregasParaCambioDeRendicion() {
	var data = { ent_compte: ent_compte_selected };
	PostGenHtml(data, obtenerEntregasParaCambioDeRendicionUrl, function (html) {
		$("#divListaEntregas").html(html);
	});
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
	const ent_compte = $td.data("ent-compte");

	if (nuevoValor === "" || isNaN(nuevoValor)) {
		CancelarEdicion($input);
		return;
	}

	AbrirWaiting("Guardando importe...");
	var data = {
		caja_nro_proceso: $td.data("caja-nro-proceso"),
		caja_nro_cierre: $td.data("caja-nro-cierre"),
		caja_nro_rend: $td.data("caja-nro-rend"),
		caja_rend_item: $td.data("rend-item"),
		importe: nuevoValor
	};
	PostGen(data, actualizarImporteEnItemDeDetalleUrl, function (obj) {
		CerrarWaiting();
		if (obj.error === true) {
			AbrirMensaje("ATENCIÓN", obj.msg, function () {
				$("#msjModal").modal("hide");
				CancelarEdicion($input);
				return true;
			}, false, ["Aceptar"], "error!", null);
		}
		else {
			ActualizarTotalesEnPadre(ent_compte);
			existe_edicion = true;
			// 🔥 IMPORTANTE: actualizar el valor original
			$input.data("original", nuevoValor);
			guardando_importe = false;
		}
	});
}

function ActualizarTotalesEnPadre(ent_compte) {
	// 1) Buscar todas las filas del detalle que correspondan a esta entrega
	const filasDetalle = $(`#tbVtasPVCtlEntregaRend tr.row-entrega[data-ent-compte="${ent_compte}"]`);

	let total = 0;

	filasDetalle.each(function () {
		const $fila = $(this);
		const $input = $fila.find(".input-importe");

		let valor = 0;

		if ($input.length > 0) {
			// Obtener valor unmasked
			const unmasked = $input.inputmask("unmaskedvalue");
			valor = parseFloat(unmasked || "0");
		} else {
			// Si no es editable, tomar el texto del <span>
			const texto = $fila.find("td.editable-importe span").text().trim();
			valor = convertirImporteADecimal(texto);
		}

		total += valor;
	});

	// 2) Actualizar la tabla padre
	const filaPadre = $(`#tbVtasPVCtlEntrega tr.row-entrega[data-ent-compte="${ent_compte}"]`);
	const celdaImporte = filaPadre.find("td").eq(6); // columna Importe

	if (celdaImporte.length > 0) {
		celdaImporte.text(FormatearPrecio(total));
	}
}

function CancelarEdicion($input) {
	const original = $input.data("original");
	$input.val(original);
}

function ProcesarSeleccionFilaRendDetalle($fila) {
	// Quitar selección previa
	$("#tbVtasPVCtlEntregaRend tbody tr").removeClass("selected-row");

	// Marcar fila seleccionada
	$fila.addClass("selected-row");
}

function validarEntregaSeleccionada() {
	//Aca agregar lo que sea necesario para validar la entrega seleccionada antes de cargar las rendiciones
	return true;
}

function obtenerEntregasSeleccionadasString() {
	const seleccionadas = [];

	$(".row-entrega").each(function () {
		const $row = $(this);
		const chk = $row.find(".chkRow").is(":checked");

		if (chk) {
			seleccionadas.push($row.data("ent-compte"));
		}
	});

	return seleccionadas;
}

function obtenerEntregasSeleccionadas() {
	const seleccionadas = [];

	$(".row-entrega").each(function () {
		const $row = $(this);
		const chk = $row.find(".chkRow").is(":checked");

		if (chk) {
			seleccionadas.push({
				ent_compte: $row.data("ent-compte"),
				ent_estado: $row.data("ent-estado"),
				ent_actu: $row.data("ent-actu"),
				ent_actu_bool: $row.data("ent-actu-bool")
			});
		}
	});

	return seleccionadas;
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

function FormatearPrecio(valor) {
	return Number(valor).toLocaleString("en-US", {
		minimumFractionDigits: 2,
		maximumFractionDigits: 2
	});
}

function convertirImporteADecimal(selector) {
	const $campo = $(selector);

	if ($campo.length === 0) return 0;

	// Usamos el método propio de Inputmask
	let valor = $campo.inputmask("unmaskedvalue");

	if (!valor || valor.trim() === "") return 0;

	return parseFloat(valor);
}