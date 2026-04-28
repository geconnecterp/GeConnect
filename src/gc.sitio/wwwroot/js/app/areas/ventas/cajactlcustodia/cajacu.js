var fila_entrega_seleccionada_actual = null;
var ent_compte_selected = null;
var ent_estado_selected = null;
var ent_actu_selected = null;
var ent_actu_bool_selected = null;
var guardando_importe = false;

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

		ProcesarSeleccionFilaEntrega($nuevaFila);
	});
	setTimeout(() => {
		$("#divOpcionesEntregaRend").hide();
	}, 500);
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
		$("#divOpcionesEntregaRend").show();
		CargarListaEntregasParaCambioDeRendicion();
	}
	else {
		$("#divOpcionesEntregaRend").hide();
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
}

function CargarListaEntregasParaCambioDeRendicion() {
	var data = { ent_compte: ent_compte_selected };
	PostGenHtml(data, obtenerEntregasParaCambioDeRendicionUrl, function (html) {
		$("#divEntregasParaCambioRend").html(html);
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
			ActualizarTotalesEnPadre();
			existe_edicion = true;
			$("#btnGuardarValores").prop("disabled", !existe_edicion);

			// 🔥 IMPORTANTE: actualizar el valor original
			$input.data("original", nuevoValor);
			guardando_importe = false;
		}
	});
}

function ActualizarTotalesEnPadre() {
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