let cta_seleccionada = false;
let cta_id_seleccionada = "";
let cta_denominacion_seleccionada = "";
let modoEdicionCantidad = false;

$(function () {
	InicializarEventos();
});

function InicializarEventos() {
	$(document).on("click", "#Rel03", function () { $(this).val(""); cta_seleccionada = false; cta_id_seleccionada = ""; });
	$(document).on("click", "#btnCargar", btnCargarClick);
	$(document).on("click", "#btnConfirmar", btnConfirmarClick);
	$(document).on("click", "#btnCancelar", btnCancelarClick);
	$(document).on("click", "#btnAgregarProducto", btnAgregarProductoClick);
	$(document).on("change", "#listaDeposito", listaDepositoChange);
	$(document).on("keyup", "#PtoVta", ControlaKeyUpComptePtoVta);
	$(document).on("focusout", "#PtoVta", ControlaFocusOutComptePtoVta);
	$(document).on("keyup", "#NroComprobante", ControlaKeyUpCompteNro);
	$(document).on("focusout", "#NroComprobante", ControlaFocusOutCompteNro);
	$("#PtoVta").inputmask("9999");
	$("#NroComprobante").inputmask("99999999");
	$(document).ready(function () {
		// Selecciona el primer radio
		$("#DesdeFactura").prop("checked", true);

		// Dispara el evento para aplicar habilitación + limpieza
		$("input[name='TipoRelacion']:checked").trigger("change");
	});
	$(document).on("change", "input[name='TipoRelacion']", function () {

		const tipo = $(this).val();

		// Controles Factura
		const ddlTipo = $("#listaTipoComprobante");
		const txtPtoVta = $("#PtoVta");
		const txtNroComprobante = $("#NroComprobante");

		// Controles Cotización
		const txtNroCotizacion = $("#NroCotizacion");

		// Autocompletar Sin Relación
		const txtAutocompletar = $("#Rel03");
		const hiddenAutocompletar = $("#Rel03Item");

		// Función auxiliar para limpiar
		function limpiarFactura() {
			ddlTipo.val("");              // limpia selección
			txtPtoVta.val("");            // limpia texto
			txtNroComprobante.val("");    // limpia texto
		}

		function limpiarCotizacion() {
			txtNroCotizacion.val("");
		}

		function limpiarSinRelacion() {
			txtAutocompletar.val("");
			hiddenAutocompletar.val("");
		}

		// 🔹 Blanquear detalle del comprobante incrustado
		$("#infoComprobanteContainer").empty();

		// ============================
		// Estado según radio seleccionado
		// ============================
		if (tipo === "Factura") {

			// Habilitar Factura
			ddlTipo.prop("disabled", false);
			txtPtoVta.prop("disabled", false);
			txtNroComprobante.prop("disabled", false);

			// Deshabilitar Cotización + limpiar
			txtNroCotizacion.prop("disabled", true);
			limpiarCotizacion();

			// Deshabilitar Autocompletar + limpiar
			txtAutocompletar.prop("disabled", true);
			limpiarSinRelacion();
		}

		else if (tipo === "Cotizacion") {

			// Deshabilitar Factura + limpiar
			ddlTipo.prop("disabled", true);
			txtPtoVta.prop("disabled", true);
			txtNroComprobante.prop("disabled", true);
			limpiarFactura();

			// Habilitar Cotización
			txtNroCotizacion.prop("disabled", false);

			// Deshabilitar Autocompletar + limpiar
			txtAutocompletar.prop("disabled", true);
			limpiarSinRelacion();
		}

		else if (tipo === "SinRelacion") {

			// Deshabilitar Factura + limpiar
			ddlTipo.prop("disabled", true);
			txtPtoVta.prop("disabled", true);
			txtNroComprobante.prop("disabled", true);
			limpiarFactura();

			// Deshabilitar Cotización + limpiar
			txtNroCotizacion.prop("disabled", true);
			limpiarCotizacion();

			// Habilitar Autocompletar
			txtAutocompletar.prop("disabled", false);
		}
	});
	CancelarRemito();
}

function btnAgregarProductoClick() {

	// ============================
	// 1) Obtener valores del formulario
	// ============================
	const prodID = $("#ProdID").val().trim();
	const prodNombre = $("#ProdNombre").text().trim();
	const provID = $("#ProvID").val().trim();
	const upID = $("#UpID").val().trim();
	const depoID = $("#listaDeposito").val();
	const boxID = $("#listaBoxes").val();

	let prodUP = parseFloat($("#ProdUP").val().replace(/,/g, "")) || 0;
	let prodBto = parseFloat($("#ProdBto").val().replace(/,/g, "")) || 0;
	let prodUnid = parseFloat($("#ProdUnid").val().replace(/,/g, "")) || 0;

	let ctaID = "";
	let ctaDenominacion = "";
	let tipo = $("input[name='TipoRelacion']:checked").val();
	if (tipo === "Factura" || tipo === "Cotizacion") {
		cta_id_seleccionada = $("#cta_id").val();
		cta_denominacion_seleccionada = $("#cta_denominacion").val();
	}

    // ============================
    // 2) Validaciones básicas
    // =============================
    if (!prodID) {
		AbrirMensaje("ATENCIÓN", "Debe seleccionar un producto antes de agregarlo.", null, false, ["Aceptar"], "error!", null);
		return;
	}

	if (!depoID || depoID === "") {
		AbrirMensaje("ATENCIÓN", "Debe seleccionar un depósito.", null, false, ["Aceptar"], "error!", null);
		return;
	}

	if (!boxID || boxID === "") {
		AbrirMensaje("ATENCIÓN", "Debe seleccionar un BOX.", null, false, ["Aceptar"], "error!", null);
		return;
	}

	// ============================
	// 3) Cálculo de cantidad a remitir
	// ============================
	// Regla: si ProdBto = 0, se toma como 1
	const bultoReal = prodBto === 0 ? 1 : prodBto;

	const cantidadARemitir = (prodUP * bultoReal) + prodUnid;

	// ============================
	// 4) Crear fila HTML
	// ============================
	const filaHTML = `
        <tr class="row-ajuste"
            data-p-id="${prodID}"
            data-p-desc="${prodNombre}"
            data-pre-id=""
            data-pree-id=""
            data-pret-id=""
			data-up-id="${upID}"
            data-depo-id="${depoID}"
            data-box-id="${boxID}"
            data-unidad-pres="${prodUP}"
            data-bulto="${prodBto}"
            data-us="${prodUnid}"
			data-cta-id="${cta_id_seleccionada}"
			data-cta-denominacion="${cta_denominacion_seleccionada}"
        >
            <td class="text-center">${prodID}</td>
            <td class="text-start">${prodNombre}</td>
            <td class="text-center">${provID}</td>
            <td class="text-center">${boxID}</td>
            <td class="text-end">0</td>
            <td class="text-end">0</td>
            <td class="text-end">0</td>

            <td class="text-end celda-a-remitir">
                <input type="text"
                       class="form-control form-control-sm editor-celda input-cantidad"
                       value="${cantidadARemitir}"
                       data-original="${cantidadARemitir}"
                       data-permite-decimales="false" />
            </td>

            <td class="text-center">
                <div class="d-flex justify-content-center gap-1">
                    <button class="btn btn-sm btn-danger btn-icon-compact btnQuitarProducto"
                            data-p-id="${prodID}"
                            title="Quitar producto del remito externo">
                        <i class="bx bx-trash"></i>
                    </button>
                </div>
            </td>
        </tr>
    `;

	// ============================
	// 5) Insertar fila en la tabla
	// ============================
	const tbody = $("#tbGridProductos tbody");

	// Si estaba la fila vacía, eliminarla
	tbody.find(".fila-vacia").remove();

	// Agregar fila
	tbody.append(filaHTML);

	// ============================
	// 6) Recalcular alternancia
	// ============================
	RecalcularAlternanciaFilas();

	// ============================
	// 7) Reaplicar eventos y máscaras
	// ============================
	CargarEventosTablaProductos();

	// Aplicar máscara al input recién agregado
	const $nuevaFila = tbody.find("tr").last();
	const $input = $nuevaFila.find("input.editor-celda");

	$input.inputmask(maskConfigEnteros);

	// 🔥 Forzar alineación derecha dentro de la celda
	$nuevaFila.find("td.celda-a-remitir").css({
		display: "flex",
		justifyContent: "flex-end",
		alignItems: "center"
	});

	// 🔥 Forzar estilo compacto después de Inputmask
	$input.css({
		width: "60px",
		minWidth: "60px",
		maxWidth: "60px",
		padding: "0 4px",
		height: "22px",
		lineHeight: "22px",
		fontSize: "0.75rem",
		textAlign: "right",
		display: "block"
	});

	// ============================
	// 8) Limpiar campos del formulario
	// ============================
	$("#ProdID").val("");
	$("#UpID").val("");
	$("#BarradoID").val("");
	$("#ProvID").val("");
	$("#ProdNombre").text("");
	$("#ProdUP").val("");
	$("#ProdBto").val("");
	$("#ProdUnid").val("");
}


function btnCargarClick() {
	let v = ValidarFiltrosParaCarga();

	if (!v.ok) {
		AbrirMensaje("Atención", v.msg, function () {
			$("#msjModal").modal("hide");
			return true;
		}, false, ["Aceptar"], "warn!", null);
		return;
	}

	// Si todo está OK → habilitar carga
	HabilitarCargaDeProductos();
	let tipo = $("input[name='TipoRelacion']:checked").val();
	if (tipo === "Factura" || tipo === "Cotizacion") {
		ValidarExistenciaDeProductos();
	}
}


function LimpiarTablaDeProductos() {
	PostGen({}, limpiarProductosCargadosURL, function (obj) {
		CerrarWaiting();
		if (obj.esError === true) {
			AbrirMensaje("ATENCIÓN", obj.mensaje, function () {
				$("#msjModal").modal("hide");
				return true;
			}, false, ["Aceptar"], "error!", null);
		}
		else if (obj.esWarn === true) {
			AbrirMensaje("ATENCIÓN", obj.mensaje, function () {
				$("#msjModal").modal("hide");
				return true;
			}, false, ["Aceptar"], "warn!", null);
		}
		else {
			// ✔ Limpiar tabla
			const tbody = $("#tbGridProductos tbody");
			tbody.empty();
			tbody.append(`
				<tr class="fila-vacia">
					<td colspan="9" class="text-center text-muted py-4">
						<i class="bx bx-info-circle me-2"></i>
						No hay items para mostrar.
					</td>
				</tr>
			`);
		}
	});
}

function ValidarExistenciaDeProductos() {
	var data = ArmarRequestParaBuscarProductos();
	AbrirWaiting();
	PostGen(data, validarExistenciaDeProdsURL, function (obj) {
		CerrarWaiting();
		if (obj.esError === true) {
			AbrirMensaje("ATENCIÓN", obj.mensaje, function () {
				$("#msjModal").modal("hide");
				return true;
			}, false, ["Aceptar"], "error!", null);
		}
		else if (obj.esWarn === true) {
			AbrirMensaje("ATENCIÓN", obj.mensaje, function () {
				$("#msjModal").modal("hide");
				return true;
			}, false, ["Aceptar"], "error!", null);
		}
		///IMPORTANTE!!! Descomentar este codigo en la version final, esta solo para propositos de prueba con comprobantes que no cumplen las condiciones y sirven para pruebas
		// else if (obj.permite === false) {
		// 	AbrirMensaje("ATENCIÓN", obj.mensaje, function () {
		// 		$("#msjModal").modal("hide");
		// 		return true;
		// 	}, false, ["Aceptar"], "error!", null);
		// }
		else {
			AbrirMensaje("ATENCIÓN", "¿Desea cargar los productos asociados al comprobante?", function (e) {
				$("#msjModal").modal("hide");
				switch (e) {
					case "SI": //Confirmar
						BuscarProductosDelComprobante("SI");
						break;
					case "NO":
						BuscarProductosDelComprobante("NO");
						break;
					default: //NO
						break;
				}
				return true;

			}, true, ["Aceptar", "Cancelar"], "question!", null);
		}
	});
}

function BuscarProductosDelComprobante(CargarProductos) {

	AbrirWaiting();

	PostGenHtml({}, cargarProductosDesdeComprobanteURL, function (obj) {

		// Renderizar el HTML completo en divProductos (lo necesitamos para extraer el acordeón)
		$("#divProductos").html(obj);

		// ============================
		// 1) Mover el acordeón SIEMPRE
		// ============================
		var info = $("#divProductos").find("#infoComprobanteRendered");

		if (info.length) {
			$("#infoComprobanteContainer").html(info);
		}

		// ============================
		// 2) Renderizar tabla SOLO si CargarProductos === "SI"
		// ============================
		if (CargarProductos === "SI") {

			// La tabla ya está dentro de obj → se deja tal cual
			CargarEventosTablaProductos();

		} else {

			// NO cargar productos → limpiar tabla
			$("#tbGridProductos tbody").html(`
                <tr class="fila-vacia">
                    <td colspan="9" class="text-center text-muted py-4">
                        <i class="bx bx-info-circle me-2"></i>
                        No hay items para mostrar.
                    </td>
                </tr>
            `);
		}

		CerrarWaiting();
	},
		function (obj) {
			ControlaMensajeError(obj.message);
			CerrarWaiting();
		});
}

function RecalcularAlternanciaFilas() {
	let alt = true;

	$("#tbGridProductos tbody tr").each(function () {

		// Ignorar la fila vacía
		if ($(this).hasClass("fila-vacia")) return;

		if (alt) {
			$(this).removeClass().addClass("alt row-ajuste");
			alt = false;
		} else {
			$(this).removeClass().addClass("row-ajuste");
			alt = true;
		}
	});
}

function CargarEventosTablaProductos() {
	// Delegación de eventos para quitar producto
	$(document).on("click", ".btnQuitarProducto", function () {

		// 1. Obtener la fila
		const fila = $(this).closest("tr");

		// 2. Eliminar la fila
		fila.remove();

		// 3. Verificar si la tabla quedó vacía
		const tbody = $("#tbGridProductos tbody");
		const filasRestantes = tbody.find("tr").length;

		if (filasRestantes === 0) {
			tbody.append(`
				<tr class="fila-vacia">
					<td colspan="9" class="text-center text-muted py-4">
						<i class="bx bx-info-circle me-2"></i>
						No hay items para mostrar.
					</td>
				</tr>
			`);
		} else {
			RecalcularAlternanciaFilas();
		}
	});
	$("#tbGridProductos .editor-celda").off();
	$("#tbGridProductos .editor-celda").on("keypress", function (e) {
		let permiteDecimales = $(this).data("permite-decimales");

		// Solo números
		if (e.which < 48 || e.which > 57) {
			// Permitir punto decimal si corresponde
			if (permiteDecimales && e.which === 46)
				return;

			e.preventDefault();
		}
	});

	$("#tbGridProductos .editor-celda").on("blur", function () {
		let $input = $(this);
		let tipo = $("input[name='TipoRelacion']:checked").val();
		if (tipo === "Factura" || tipo === "Cotizacion") {
			if (!ValidarCantidad($input)) {
				return; // no guardar ni avanzar
			}
		}

		GuardarValorYRecalcular($input);
	});

	$("#tbGridProductos").on("keydown", ".editor-celda", function (e) {

		let $input = $(this);
		let $fila = $input.closest("tr");
		let $todasLasFilas = $("#tbGridProductos tbody tr").not(".fila-vacia");
		let index = $todasLasFilas.index($fila);

		// ENTER o TAB → guardar y pasar a la siguiente fila
		if (e.key === "Enter" || e.key === "Tab") {
			e.preventDefault();

			let tipo = $("input[name='TipoRelacion']:checked").val();
			if (tipo === "Factura" || tipo === "Cotizacion") {
				if (!ValidarCantidad($input)) {
					return; // no guardar ni avanzar
				}
			}

			GuardarValorYRecalcular($input);

			// Ir a la siguiente fila
			let nextIndex = (index + 1) % $todasLasFilas.length;
			ActivarEdicionEnFila($todasLasFilas.eq(nextIndex));
			return;
		}

		// FLECHA ARRIBA
		if (e.key === "ArrowUp") {
			e.preventDefault();
			GuardarValorYRecalcular($input);

			let prevIndex = index - 1;
			if (prevIndex < 0) prevIndex = $todasLasFilas.length - 1;

			ActivarEdicionEnFila($todasLasFilas.eq(prevIndex));
			return;
		}

		// FLECHA ABAJO
		if (e.key === "ArrowDown") {
			e.preventDefault();
			GuardarValorYRecalcular($input);

			let nextIndex = (index + 1) % $todasLasFilas.length;

			ActivarEdicionEnFila($todasLasFilas.eq(nextIndex));
			return;
		}
	});

	$("#tbConsolidarDetalleConteo").on("change", ".editor-celda", function () {
		let $input = $(this);
		GuardarValorYRecalcular($input);
	});
}

function ObtenerValorNumerico(str) {
	if (!str) return 0;
	return parseFloat(str.toString().replace(/,/g, ""));
}

function ObtenerValorAEntrar($fila) {
	const texto = $fila.find("td").eq(6).text().trim(); // columna A Entr.
	return ObtenerValorNumerico(texto);
}

function ValidarCantidad($input) {

	const $fila = $input.closest("tr");

	// Valor ingresado (sin máscara)
	let valorIngresado = ObtenerValorNumerico($input.val());

	// Valor permitido (A Entr.)
	let maximo = ObtenerValorAEntrar($fila);

	if (valorIngresado > maximo) {

		AbrirMensaje("ATENCIÓN",
			`El valor a Remitir (${valorIngresado}) no puede ser mayor que el valor a Entregar (${maximo}).`,
			function () {
				$("#msjModal").modal("hide");
			},
			false,
			["Aceptar"],
			"error!",
			null
		);

		// Restaurar valor original (sin formato)
		let original = $input.data("original");

		// Quitar máscara actual
		$input.inputmask("remove");

		// Asignar valor crudo
		$input.val(original);

		// Reaplicar máscara correcta
		let permiteDecimales =
			$input.data("permite-decimales") === true ||
			$input.data("permite-decimales") === "true";

		if (permiteDecimales) {
			$input.inputmask(maskConfigDecimales);
		} else {
			$input.inputmask(maskConfigEnteros);
		}

		return false;
	}

	return true;
}

function ActivarEdicionEnFila($fila) {

	let $input = $fila.find("td.celda-a-remitir input");

	let permiteDecimales = $input.data("permite-decimales") === true ||
		$input.data("permite-decimales") === "true";

	// Habilitar input
	$input.prop("disabled", false);

	// Aplicar máscara
	if (permiteDecimales) {
		$input.inputmask(maskConfigDecimales);
	} else {
		$input.inputmask(maskConfigEnteros);
	}

	// Foco automático
	setTimeout(() => $input.focus().select(), 10);
}

const maskConfigDecimales = {
	alias: "numeric",
	groupSeparator: ",",
	radixPoint: ".",
	autoGroup: true,
	digits: 2,
	digitsOptional: false,
	rightAlign: true,
	prefix: '',
	placeholder: "0",
	clearMaskOnLostFocus: false,
	showMaskOnHover: false,
	showMaskOnFocus: false,
	onBeforeMask: function (value) {
		if (value) {
			let numValue = parseFloat(value.toString().replace(/,/g, ''));
			return isNaN(numValue) ? value : numValue.toFixed(2);
		}
		return value;
	}
};

const maskConfigEnteros = {
	alias: "numeric",
	groupSeparator: ",",
	radixPoint: ".",
	autoGroup: true,
	digits: 0,
	digitsOptional: true,
	rightAlign: true,
	prefix: '',
	placeholder: "0",
	clearMaskOnLostFocus: false,
	showMaskOnHover: false,
	showMaskOnFocus: false
};

function GuardarValorYRecalcular($input) {

	let valor = $input.val().trim();
	if (valor === "") valor = "0";

	valor = valor.replace(/,/g, "");

	// Guardar en el input (no reemplazar el td)
	$input.val(valor);
}

function btnConfirmarClick() {
	const filas = $("#tbGridProductos tbody tr").not(".fila-vacia");
	// Si hay productos cargados → pedir confirmación
	if (filas.length > 0) {

		AbrirMensaje(
			'CONFIRMAR CARGA',
			"¿Desea confirma la carga del remito?.",
			function (resp) {
				if (resp === 'SI') {
					$('#msjModal').modal('hide');
					ConfirmarRemito();
				} else {
					$('#msjModal').modal('hide');
				}
			},
			true,
			['Confirmar', 'Cancelar'],
			'info!',
			null
		);

		return; // detener flujo
	}
}

function ConfirmarRemito() {
	AbrirWaiting("Registrando Remito Externo...")
	var data = ObtenerRequest();
	PostGen(data, confirmarRemitoExternoUrl, function (obj) {
		CerrarWaiting();
		if (obj.error === true || obj.warn === true) {
			console.error('❌ Response:', obj.msg);
			AbrirMensaje("ATENCIÓN", 'Error al intentar confirmar el remito externo: ' + (obj.msg || 'Error desconocido'), function () {
				$("#msjModal").modal("hide");
				return true;
			}, false, ["Aceptar"], "error!", null);
		}
		else {
			setTimeout(() => {
				AbrirMensaje(
					'CONFIRMACIÓN EXITOSA',
					'Se ha confirmado el Remito Externo con el ID: ' + obj.id,
					function () {
						$('#msjModal').modal('hide');
						//Imprimir Remito
						//ImprimirRemitoExterno(obj.id);
						console.log("Remito Externo Generado: ", obj.id);
						ResetearPantallaRemito();
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

function ResetearPantallaRemito() {

	// ============================
	// 1) Resetear radios
	// ============================
	$("#DesdeFactura").prop("checked", true);
	$("#DesdeCotizacion").prop("checked", false);
	$("#SinRelacion").prop("checked", false);

	// ============================
	// 2) Limpiar selects e inputs
	// ============================
	$("#listaTipoComprobante").val("");
	$("#PtoVta").val("");
	$("#NroComprobante").val("");

	$("#NroCotizacion").val("");
	$("#Rel03").val("");
	$("#Rel03Item").val("");

	$("#listaDeposito").val("");
	$("#listaBoxes").val("");
	$("#Obs").val("");

	$("#ProdID").val("");
	$("#UpID").val("");
	$("#BarradoID").val("");
	$("#ProvID").val("");
	$("#BoxID").val("");
	$("#DepoID").val("");

	$("#ProdNombre").text("");
	$("#ProdUP").val("");
	$("#ProdBto").val("");
	$("#ProdUnid").val("");

	// ============================
	// 3) Limpiar acordeón
	// ============================
	$("#infoComprobanteContainer").empty();

	// ============================
	// 4) Limpiar tabla
	// ============================
	$("#tbGridProductos tbody").html(`
        <tr class="fila-vacia">
            <td colspan="9" class="text-center text-muted py-4">
                <i class="bx bx-info-circle me-2"></i>
                No hay items para mostrar.
            </td>
        </tr>
    `);

	// ============================
	// 5) Resetear botones
	// ============================
	$("#btnCargar").prop("disabled", false);
	$("#btnConfirmar").prop("disabled", true);
	$("#btnCancelar").prop("disabled", true);

	// ============================
	// 6) Habilitar solo los radios
	// ============================
	$("input[name='TipoRelacion']").prop("disabled", false);

	// ============================
	// 7) Aplicar estado de "DesdeFactura"
	// ============================
	// Habilitar controles de Factura
	$("#listaTipoComprobante").prop("disabled", false);
	$("#PtoVta").prop("disabled", false);
	$("#NroComprobante").prop("disabled", false);

	// Deshabilitar controles de Cotización
	$("#NroCotizacion").prop("disabled", true);

	// Deshabilitar controles de Sin Relación
	$("#Rel03").prop("disabled", true);

	// ============================
	// 8) Habilitar selects de la derecha
	// ============================
	$("#listaDeposito").prop("disabled", false);
	$("#listaBoxes").prop("disabled", false);
	$("#Obs").prop("disabled", false);

	// ============================
	// 9) Deshabilitar sección inferior
	// ============================
	$("#ProdID, #ProdUP, #ProdBto, #ProdUnid, #Busqueda").prop("disabled", true);
	$("#btnAgregarProducto, #btnQuitarProducto, #btnBusquedaBase").prop("disabled", true);

	console.log("Pantalla de remito reseteada correctamente.");
}




function ObtenerRequest() {
	let productos = ObtenerColeccionProductosJson();
	let tipo = ObtenerTipoRelacionValor();
	let tco_id = "";
	let cm_compte = "";
	let pre_id = "";
	if (tipo === "1") {
		tco_id = $("#listaTipoComprobante").val();
		cm_compte = $("#PtoVta").inputmask('unmaskedvalue').padStart(4, '0') + "-" + $("#NroComprobante").inputmask('unmaskedvalue').padStart(8, '0');
	}
	else if (tipo === "2") {
		pre_id = $("#NroCotizacion").val();
		let compte = ObtenerDatosDelComprobante();
		tco_id = compte.tco_id;
		cm_compte = compte.cm_compte;
	}
	let obs = $("#Obs").val();
	return {
		opcion: tipo,
		cta_id: cta_id_seleccionada,
		tco_id,
		cm_compte,
		pre_id,
		re_obs: obs,
		json: productos
	};
}

function ObtenerColeccionProductosJson() {
	const coleccion = ObtenerColeccionProductos();
	return JSON.stringify(coleccion);
}

function ObtenerTipoRelacionValor() {
	const valor = $("input[name='TipoRelacion']:checked").val();

	switch (valor) {
		case "Factura": return "1";
		case "Cotizacion": return "2";
		case "SinRelacion": return "3";
		default: return "0"; // por si acaso
	}
}

function ObtenerDatosDelComprobante() {

	// Obtener la primera fila que no sea la fila vacía
	const $fila = $("#tbGridProductos tbody tr").not(".fila-vacia").first();

	// Si no hay filas reales, devolver null
	if ($fila.length === 0) {
		return null;
	}

	// Construir y devolver el objeto
	return {
		tco_id: $fila.data("tco-id"),
		cm_compte: $fila.data("cm-compte")
	};
}


function ObtenerColeccionProductos() {

	let coleccion = [];
	let first = true;
	$("#tbGridProductos tbody tr").not(".fila-vacia").each(function () {

		const $fila = $(this);

		// Obtener cantidad desde el input con máscara
		let cantidad = $fila.find("input.editor-celda").val() || "0";
		cantidad = cantidad.replace(/,/g, ""); // quitar separadores
		cantidad = parseFloat(cantidad) || 0;

		if (first) {
			first = false;

		}
		// Armar objeto
		let item = {
			p_id: $fila.data("p-id"),
			p_desc: $fila.data("p-desc"),
			depo_id: $fila.data("depo-id"),
			box_id: $fila.data("box-id"),
			up_id: $fila.data("up-id"),
			unidad_pres: parseFloat($fila.data("unidad-pres")) || 0,
			bulto: parseFloat($fila.data("bulto")) || 0,
			us: parseFloat($fila.data("us")) || 0,
			cantidad: cantidad
		};

		coleccion.push(item);
	});

	return coleccion;
}

function btnCancelarClick() {

	const filas = $("#tbGridProductos tbody tr").not(".fila-vacia");

	// Si hay productos cargados → pedir confirmación
	if (filas.length > 0) {

		AbrirMensaje(
			'CONFIRMAR CANCELACIÓN',
			"¿Desea cancelar la carga del remito? Se perderán los productos agregados.",
			function (resp) {
				if (resp === 'SI') {
					$('#msjModal').modal('hide');
					CancelarRemito();
				} else {
					$('#msjModal').modal('hide');
				}
			},
			true,
			['Confirmar', 'Cancelar'],
			'info!',
			null
		);

		return; // detener flujo
	}

	// Si no hay productos → cancelar directamente
	CancelarRemito();
}


function CancelarRemito() {

	// Habilitar sección superior
	$(".row-block input, .row-block select").prop("disabled", false);
	$("input[name='TipoRelacion']").prop("disabled", false);

	// Deshabilitar sección inferior
	$("#ProdID, #ProdUP, #ProdBto, #ProdUnid, #Busqueda").prop("disabled", true);
	$("#btnAgregarProducto, #btnQuitarProducto, #btnBusquedaBase").prop("disabled", true);

	// Botones
	$("#btnCargar").prop("disabled", false);
	$("#btnConfirmar").prop("disabled", true);
	$("#btnCancelar").prop("disabled", true);

	LimpiarTablaDeProductos();

	console.log("CancelarRemito");
}


function HabilitarCargaDeProductos() {

	// Bloquear sección superior
	$(".row-block input, .row-block select").prop("disabled", true);
	$("input[name='TipoRelacion']").prop("disabled", true);

	// Habilitar sección inferior
	$("#ProdID, #ProdUP, #ProdBto, #ProdUnid, #Busqueda").prop("disabled", false);
	$("#btnAgregarProducto, #btnQuitarProducto, #btnBusquedaBase").prop("disabled", false);

	// Botones
	$("#btnCargar").prop("disabled", true);
	$("#btnConfirmar").prop("disabled", false);
	$("#btnCancelar").prop("disabled", false);

	console.log("HabilitarCargaDeProductos");
}

function ArmarRequestParaBuscarProductos() {
	let tipo = $("input[name='TipoRelacion']:checked").val();
	if (tipo === "Factura") {
		return {
			tipo: tipo,
			tco_id: $("#listaTipoComprobante").val(),
			cm_compte: $("#PtoVta").inputmask('unmaskedvalue').padStart(4, '0') + "-" + $("#NroComprobante").inputmask('unmaskedvalue').padStart(8, '0'),
			pre_id: "",
			box_id: $("#listaBoxes").val(),
			depo_id: $("#listaDeposito").val()
		};
	}

	if (tipo === "Cotizacion") {
		return {
			tipo: tipo,
			tco_id: "",
			cm_compte: "",
			pre_id: $("#NroCotizacion").val(),
			box_id: $("#listaBoxes").val(),
			depo_id: $("#listaDeposito").val()
		};
	}

	if (tipo === "SinRelacion") {
		return {
			tipo: tipo,
			tco_id: "",
			cm_compte: "",
			pre_id: "",
			box_id: $("#listaBoxes").val(),
			depo_id: $("#listaDeposito").val()
		};
	}
}


function ValidarFiltrosParaCarga() {

	let tipo = $("input[name='TipoRelacion']:checked").val();

	// Validaciones según radio
	if (tipo === "Factura") {
		if ($("#PtoVta").val().trim() === "" || $("#NroComprobante").val().trim() === "") {
			return { ok: false, msg: "Debe completar Punto de Venta y Número de Comprobante." };
		}
	}

	if (tipo === "Cotizacion") {
		if ($("#NroCotizacion").val().trim() === "") {
			return { ok: false, msg: "Debe completar el Número de Cotización." };
		}
	}

	if (tipo === "SinRelacion") {
		if ($("#Rel03").val().trim() === "") {
			return { ok: false, msg: "Debe completar el campo de búsqueda de relación." };
		}
	}

	// Validaciones comunes
	if ($("#listaDeposito").val().trim() === "") {
		return { ok: false, msg: "Debe seleccionar un Depósito." };
	}

	if ($("#listaBoxes").val().trim() === "") {
		return { ok: false, msg: "Debe seleccionar un BOX." };
	}

	if ($("#Obs").val().trim() === "") {
		return { ok: false, msg: "Debe completar la Observación." };
	}

	return { ok: true };
}


function ControlaFocusOutComptePtoVta() {
	var ptv = $("#PtoVta").inputmask('unmaskedvalue');
	if (ptv != "") {
		var aux = $("#PtoVta").inputmask('unmaskedvalue').padStart(4, '0');
		$("#PtoVta").val(aux);
		$("#NroComprobante").trigger("focus");
	}
}

function ControlaKeyUpComptePtoVta(e) {
	if (e.which == 13 || e.which == 109) {
		var aux = $("#PtoVta").inputmask('unmaskedvalue').padStart(4, '0');
		$("#PtoVta").val(aux);
		$("#NroComprobante").trigger("focus");
	}
}

function ControlaFocusOutCompteNro() {
	var nro = $("#NroComprobante").inputmask('unmaskedvalue');
	if (nro != "") {
		var aux = $("#NroComprobante").inputmask('unmaskedvalue').padStart(8, '0');
		$("#NroComprobante").val(aux);
		$("#listaDeposito").trigger("focus");
	}
}

function ControlaKeyUpCompteNro(e) {
	if (e.which == 13 || e.which == 109) {
		var aux = $("#NroComprobante").inputmask('unmaskedvalue').padStart(8, '0');
		$("#NroComprobante").val(aux);
		$("#listaDeposito").trigger("focus");
	}
}

function listaDepositoChange() {
	if ($("#listaDeposito").val() == "") {
		BlanquearComboBoxes();
		return false;
	}
	if ($("#listaDeposito").val() == "0") {
		BlanquearComboBoxes();
		return false;
	}
	BuscarBoxDesdeDeposito();
}

function BlanquearComboBoxes() {
	var depoId = "0";
	var datos = { depoId };
	PostGenHtml(datos, BuscarBoxesDesdeDepositoURL, function (obj) {
		$("#divComboBoxes").html(obj);
		$("#listaBoxes").on("change", listaBoxesChange);
		CerrarWaiting();
		return true
	});
}

function listaBoxesChange() {
}

function BuscarBoxDesdeDeposito() {
	AbrirWaiting();
	var depoId = $("#listaDeposito").val();
	var datos = { depoId };
	PostGenHtml(datos, BuscarBoxesDesdeDepositoURL, function (obj) {
		$("#divComboBoxes").html(obj);
		$("#listaBoxes").on("change", listaBoxesChange);
		CerrarWaiting();
		return true
	});
}

$("#Rel03").autocomplete({
	source: function (request, response) {

		data = { prefix: request.term }; Rel03

		$.ajax({
			url: autoComRel03Url,
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
		cta_seleccionada = true;
		cta_id_seleccionada = ui.item.id;
		cta_denominacion_seleccionada = QuitarParentesis(ui.item.label);
		return true;
	}
});

function QuitarParentesis(str) {
	return str
		.replace(/\([^)]*\)/g, "")   // quita paréntesis
		.replace(/\s+/g, " ")        // normaliza espacios
		.trim();
}

function VerificarExistenciaDeProductosDesdeComprobantes(datos) {


}

function cargarProductos() {
}

function EliminarProducto(id) {
}

function InicializaPantalla() {
}

async function verificaEstado(e) {
	FunctionCallback = null; //inicializo funcion por si tiene alguna funcionalidad asignada.
	var res = $("#estadoFuncion").val();
	CerrarWaiting();
	if (res === "true") {
		//traigo la variable productoBase e hidrato componentes
		var prod = productoBase;

		let tipo = $("input[name='TipoRelacion']:checked").val();
		if (tipo === "Factura" || tipo === "Cotizacion") {
			const ok = await ValidarExistenciaDeProducto(prod.p_id);
			if (!ok) return;   // corta el flujo
			// seguir con la carga
			ProcesarCargaProducto(prod);
		}
		ProcesarCargaProducto(prod);
	}
	return true;
}

function ProcesarCargaProducto(prod) {
	$("#ProdID").val(prod.p_id);
	$("#ProdNombre").text(prod.p_desc);
	$("#estadoFuncion").val(false);
	$("#UpID").val(prod.up_id);
	$("#BarradoID").val(prod.p_id_barrado);
	$("#ProvID").val(prod.p_id_prov);
	$("#txtUP").mask("000.000.000.000", { reverse: true });
	$("txtBto").mask('#,##0', {
		reverse: true,
		translation: {
			'#': {
				pattern: /-|\d/,
				recursive: true
			}
		},
		onChange: function (value, e) {
			e.target.value = value.replace(/(?!^)-/g, '').replace(/^,/, '').replace(/^-,/, '-');
		}
	});

	$("#ProdUP").val(prod.p_unidad_pres).prop("disabled", false);
	$("#ProdBto").val(prod.bulto).prop("disabled", false);
	$("#ProdUnid").mask("000.000.000.000", { reverse: true });

	if (prod.up_id !== "07") {  //unidades enteras
		// $("#box").mask("000.000.000.000,00", { reverse: true });
		$("#ProdUnid").mask("000.000.000.000,00", { reverse: true });
		$("#ProdUnid").val(0).prop("disabled", false);
	}
	else { //unidades decimales
		//$("#txtUnid").val(0).prop("disabled", true);
	}
	$("#Busqueda").val("");
	if (prod.p_con_vto !== "N") {
	} else {
	}
	$("#ProdUP").focus();
}

///Funcion para validar la existencia del prudcto agregado en el comprobante 
function ValidarExistenciaDeProducto(p_id) {
	return new Promise(function (resolve) {
		AbrirWaiting("Validando existencia de producto en comprobante...");
		PostGen({ pId: p_id }, validarExistenciaDeProductoUrl, function (o) {
			CerrarWaiting();
			if (o.esError === true) {
				AbrirMensaje("Atención", o.mensaje, function () {
					$("#msjModal").modal("hide");
					resolve(false);
				}, false, ["Aceptar"], "error!", null);
				return;
			}
			if (o.esWarn === true) {
				AbrirMensaje("Atención", o.mensaje, function () {
					$("#msjModal").modal("hide");
					resolve(false);
				}, false, ["Aceptar"], "warn!", null);
				return;
			}
			if (o.permite === false) {
				AbrirMensaje("ATENCIÓN", o.mensaje, function () {
					$("#msjModal").modal("hide");
					resolve(false);
				}, false, ["Aceptar"], "error!", null);
				return;
			}
			resolve(true);
		});
	});
}
