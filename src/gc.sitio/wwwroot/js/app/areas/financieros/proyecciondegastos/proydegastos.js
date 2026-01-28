$(function () {
	InicializarControles();
	$(document).on("click", "#btnAgregar", AgregarProyeccionDeGasto);
	$(document).on("click", "#btnCancelar", CancelarProyeccionDeGasto);
	$(document).on("click", "#btnModificar", ModificarProyeccionDeGasto);
	$(document).on("click", "#btnConfirmar", ConfirmarProyeccionDeGasto);
	marcarFilasFuturas();
});

function ConfirmarProyeccionDeGasto() {
	AbrirMensaje("ATENCIÓN", "¿Esta seguro que desea confirmar la carga de la proyección de gastos? Los conceptos vencidos serán eliminados.", function (e) {
		$("#msjModal").modal("hide");
		switch (e) {
			case "SI":
				handlerConfirmarProyeccionDeGastos();
				break;
			case "NO":
				break;
			default: //NO
				break;
		}
		return true;

	}, true, ["Aceptar", "Cancelar"], "question!", null);
}

function handlerConfirmarProyeccionDeGastos() {
	AbrirWaiting();
	PostGen({}, confirmarProyeccionDeGastoUrl, function (obj) {
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
				$('#modalProyeccionDeGastoSeleccionada').modal('hide');
				ActualizarListaDeProyeccionDeGastos();
			}, 500);
		}
	});
}

function ModificarProyeccionDeGasto() {
	AbrirWaiting();
	var items = $("#itemsProyeccion").val();
	var fecha = $("#FechaProyeccion").val();
	var concepto = $("#ConceptoProyeccion").val();
	var importe = $("#ImporteProyeccion").inputmask('unmaskedvalue');
	var data = { items, fecha, concepto, importe };
	PostGen(data, modificarItemProyeccionDeGastoUrl, function (obj) {
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
				$('#modalProyeccionDeGastoSeleccionada').modal('hide');
				ActualizarListaDeProyeccionDeGastos();
			}, 500);
		}
	});
}

function CancelarProyeccionDeGasto() {
	AbrirMensaje("ATENCIÓN", "¿Esta seguro que desea cancelar la carga de la proyección de gastos?", function (e) {
		$("#msjModal").modal("hide");
		switch (e) {
			case "SI":
				handlerCancelarProyeccionDeGastos();
				break;
			case "NO":
				break;
			default: //NO
				break;
		}
		return true;

	}, true, ["Aceptar", "Cancelar"], "question!", null);
}

function handlerCancelarProyeccionDeGastos() {
	InicializarDatosEnSesion();
	setTimeout(() => {
		ActualizarListaDeProyeccionDeGastos();
		LimpiarCampos();
	}, 500);
}

function InicializarDatosEnSesion() {
	AbrirWaiting();
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
		}
	});
}

function AgregarProyeccionDeGasto() {
	AbrirWaiting();
	var fecha = $("#Fecha").val();
	var concepto = $("#Concepto").val();
	var importe = $("#Importe").inputmask('unmaskedvalue');
	var data = { fecha, concepto, importe };
	PostGen(data, agregarRegistroUrl, function (obj) {
		CerrarWaiting();
		if (obj.error === true) {
			AbrirMensaje("ATENCIÓN", obj.msg, function () {
				$("#msjModal").modal("hide");
				return true;
			}, false, ["Aceptar"], "error!", null);
		}
		else {
			ActualizarListaDeProyeccionDeGastos();
		}
	});
}

function ActualizarListaDeProyeccionDeGastos() {
	AbrirWaiting();
	var data = {};
	PostGenHtml(data, actualizarListaDeProyeccionDeGastosUrl, function (obj) {
		$("#divGrillaProyeccion").html(obj);
		LimpiarCampos();
		$("#Fecha").trigger("focus");
		marcarFilasFuturas();
		CerrarWaiting();
		return true
	});
}

function InicializarControles() {
	getMaskForMoneyType("#Importe");
	const $div = $("#divProyGastos");

	$div.find("input").on("keydown", function (e) {
		if (e.key === "Enter") {
			e.preventDefault();

			const $campos = $div.find("input")
				.filter(":visible:enabled");

			const index = $campos.index(this);

			if (index !== -1) {
				if (index < $campos.length - 1) {
					$campos.eq(index + 1).focus();
				} else {
					$div.find("#btnAgregar").focus();
				}
			}
		}
	});
}

function LimpiarCampos() {
	var now = moment().format('yyyy-MM-DD');
	$("#Fecha").val(now);
	$("#Concepto").val("");
	$("#Importe").val("0");
}

function modificarItem(orden, items) {
	AbrirWaiting();
	var datos = { orden, items };
	PostGenHtml(datos, cargarProyeccionSeleccionadaUrl, function (obj) {
		$("#divModalProyeccionDeGastoSeleccionada").html(obj);
		const $modal = $("#modalProyeccionDeGastoSeleccionada");

		$modal.modal({
			backdrop: 'static',
		});

		inicializarCamposEnModal();
		$("#Fecha").trigger("focus");

		$modal.modal('show');
		CerrarWaiting();
		return true
	});
}

function eliminarItem(orden, items, concepto) {
	AbrirMensaje("ATENCIÓN", `¿Esta seguro de eliminar el item Concepto: '${concepto}'?`, function (e) {
		$("#msjModal").modal("hide");
		switch (e) {
			case "SI":
				handlerEliminarItemProyeccionDeGastos(items);
				break;
			case "NO":
				break;
			default: //NO
				break;
		}
		return true;

	}, true, ["Aceptar", "Cancelar"], "question!", null);
}

function handlerEliminarItemProyeccionDeGastos(items) {
	var data = { items };
	PostGen(data, eliminarItemProyeccionDeGastosUrl, function (obj) {
		CerrarWaiting();
		if (obj.error === true) {
			AbrirMensaje("ATENCIÓN", obj.msg, function () {
				$("#msjModal").modal("hide");
				return true;
			}, false, ["Aceptar"], "error!", null);
		}
		else {
			ActualizarListaDeProyeccionDeGastos();
		}
	});
}

function selectItemGrillaProyeccion(x) {

}

function inicializarCamposEnModal() {
	const $modal = $("#modalProyeccionDeGastoSeleccionada");

	$modal.find("input").on("keydown", function (e) {
		if (e.key === "Enter") {
			e.preventDefault();

			const $campos = $modal.find("input")
				.filter(":visible:enabled");

			const index = $campos.index(this);

			if (index !== -1) {
				if (index < $campos.length - 1) {
					$campos.eq(index + 1).focus();
				} else {
					$modal.find("#btnModificar").focus();
				}
			}
		}
	});

	["#ImporteProyeccion"].forEach(selector => {
		const $campo = $modal.find(selector);
		let valor = $campo.val();

		if (valor && valor.includes(".")) {
			valor = valor.replace(".", ",");
			$campo.val(valor);
		}
	});

	$("#ImporteProyeccion").inputmask({
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

function marcarFilasFuturas() {
	const hoy = new Date();
	hoy.setHours(0, 0, 0, 0);

	$("#tbListaProyeccionDeGastos tbody tr").each(function () {

		const $tdFecha = $(this).find("td").eq(0);
		const textoFecha = $tdFecha.text().trim();

		// Esperamos dd/MM/yy o dd/MM/yyyy
		const partes = textoFecha.split("/");
		if (partes.length !== 3) return;

		let dia = parseInt(partes[0], 10);
		let mes = parseInt(partes[1], 10) - 1;
		let anio = partes[2].trim();

		// Normalizar año:
		// 00 → 2000
		// 01 → 2001
		// 25 → 2025
		// 2025 → 2025
		if (anio.length === 2) {
			anio = 2000 + parseInt(anio, 10);
		} else {
			anio = parseInt(anio, 10);
		}

		const fecha = new Date(anio, mes, dia);

		if (fecha > hoy) {
			// Aplicar clase a TODOS los td de la fila
			$(this).find("td").addClass("fila-futura");
		}
		if (textoFecha == "01/01/00") {
			// Aplicar clase a TODOS los td de la fila
			$(this).find("td").addClass("fila-futura");
		}
	});
}
