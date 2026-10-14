let desdeProdElaborado = false;
let bloqueDestino = null;

$(function () {
	InicializarEventos();
	if ($("#divDetalle").is(":visible")) {
		ModificacionesLocales();
	}
});

function InicializarEventos() {
	// Delegación para ambos botones con ID duplicado
	$(document).on("click", "#btnBusquedaBase", function () {

		// Detectar el bloque padre
		const bloque = $(this).closest(".grid-golden");

		let origen = "";

		if (bloque.find("strong").text().includes("Elaborados")) {
			origen = "elaborados";
		} else if (bloque.find("strong").text().includes("Materia Prima")) {
			origen = "materiaPrima";
		}

		console.log("Botón presionado desde:", origen);

		// Obtener el input asociado
		const inputBusqueda = bloque.find("input[asp-for='Busqueda']");
		console.log("Valor ingresado:", inputBusqueda.val());

		// Lógica según origen
		if (origen === "elaborados") {
			desdeProdElaborado = true;
		} else {
			desdeProdElaborado = false;
		}
		// Guardamos el bloque destino
		bloqueDestino = bloque;
	});
	// Handler para el input de búsqueda del componente común
	$(document).on("keydown", "input[name='Busqueda']", function (e) {
		const bloque = $(this).closest(".grid-golden");

		let origen = "";

		if (bloque.find("strong").text().includes("Elaborados")) {
			origen = "elaborados";
			desdeProdElaborado = true;
		} else {
			origen = "materiaPrima";
			desdeProdElaborado = false;
		}

		console.log("ENTER en input desde:", origen);

		bloqueDestino = bloque;

		if (e.key === "Enter") {
			e.preventDefault();
			buscarProducto();
		}
	});


	$(document).off("change", "#listaDeposito");
	$(document).on("change", "#listaDeposito", listaDepositoChange);

	$(document).on("keyup", "#txtUP", function (e) {
		analizaInputUP(e, $(this));
	});
	$(document).on("keyup", "#txtBto", function (e) {
		analizaInputBto(e, $(this));
	});
	$(document).on("keyup", "#txtUnid", function (e) {
		analizaInputUnid(e, $(this));
	});

	$(document).on("click", "#btnAddProd", function () {

		// Detectar el bloque padre
		const bloque = $(this).closest(".grid-golden");

		// Guardar bloque destino
		bloqueDestino = bloque;

		// Ejecutar la lógica de agregar producto
		agregarProductoEnBloque(bloque);
	});
	$(document).on("click", ".btnQuitarPedido", function () {

		const btn = $(this);
		const fila = btn.closest("tr");
		const bloque = btn.closest(".grid-golden");
		const tbody = bloque.find("tbody");

		// Animación fade-out
		fila.addClass("fade-out-row");

		setTimeout(() => {
			fila.remove();

			// Si la tabla queda vacía → agregar fila "No hay items"
			if (tbody.find("tr").length === 0) {
				tbody.append(`
                <tr class="fila-vacia">
                    <td colspan="4" class="text-center text-muted py-2">
                        <i class="bx bx-info-circle me-1"></i>
                        No hay items para mostrar.
                    </td>
                </tr>
            `);
			}
			// Recalcular interlineado
			recalcularInterlineado(bloque);
		}, 200);
	});
	$(document).off("click", "#tbProdMp tbody tr");
	$(document).on("click", "#tbProdMp tbody tr", function (e) {

		if ($(e.target).is("button, a, .btn, i")) return;

		const $nuevaFila = $(this);
		ProcesarSeleccionFilaEnProdMatPrima($nuevaFila);
	});

	$(document).off("click", "#tbProdElab tbody tr");
	$(document).on("click", "#tbProdElab tbody tr", function (e) {

		if ($(e.target).is("button, a, .btn, i")) return;

		const $nuevaFila = $(this);
		ProcesarSeleccionFilaEnProdElab($nuevaFila);
	});
	$(document).off("click", "#btnConfirmar");
	$(document).on("click", "#btnConfirmar", function (e) {

	});
	$(document).off("click", "#btnCancelar");
	$(document).on("click", "#btnCancelar", function (e) {
		const tablaElab = $("#divTablaProductosElaborados").find("tbody tr:not(.fila-vacia)");
		const tablaMp = $("#divTablaProductosMateriaPrima").find("tbody tr:not(.fila-vacia)");

		const hayDatos = tablaElab.length > 0 || tablaMp.length > 0;

		if (hayDatos) {

			AbrirMensaje(
				'CANCELAR CARGA',
				"¿Cancelar la carga de la Planilla? Si continúa los datos se perderán.",
				function (resp) {
					if (resp === 'SI') {
						$('#msjModal').modal('hide');
						CancelarCarga();
					} else {
						$('#msjModal').modal('hide');
					}
				},
				true,
				['SI', 'NO'],
				'info!',
				null
			);

		} else {
			// No hay datos → cancelar directamente
			CancelarCarga();
		}
	});

	$(document).off("click", "#btnConfirmar");
	$(document).on("click", "#btnConfirmar", function (e) {
		const tablaElab = $("#divTablaProductosElaborados").find("tbody tr:not(.fila-vacia)");
		const tablaMp = $("#divTablaProductosMateriaPrima").find("tbody tr:not(.fila-vacia)");
		const depo = $("#listaDeposito").val();
		const box = $("#listaBox").val();
		const nota = $("#Nota").val();
		const hayDatos = tablaElab.length > 0 && tablaMp.length > 0;

		if (hayDatos && depo != "" && box != "" && nota != "") {

			AbrirMensaje(
				'CONFIRMAR CARGA',
				"¿Confirmar la carga de la Planilla?",
				function (resp) {
					if (resp === 'SI') {
						$('#msjModal').modal('hide');
						ConfirmarCarga();
					} else {
						$('#msjModal').modal('hide');
					}
				},
				true,
				['SI', 'NO'],
				'info!',
				null
			);

		} else {
			if (!hayDatos) {
				if (tablaElab.length <= 0) {
					AbrirMensaje("ATENCIÓN",
						"Se deben agregar productos en sección 'PRODUCTOS ELABORADOS'.", function () {
							$("#msjModal").modal("hide");
							return true;
						}, false, ["Aceptar"], "error!", null);
					return;
				}
				else {
					AbrirMensaje("ATENCIÓN",
						"Se deben agregar productos en sección 'MATERIA PRIMA'", function () {
							$("#msjModal").modal("hide");
							return true;
						}, false, ["Aceptar"], "error!", null);
					return;
				}
			}
			else if (depo == "") {
				AbrirMensaje("ATENCIÓN",
					"Se deben seleccionar un DEPÓSITO válido", function () {
						$("#msjModal").modal("hide");
						$("#listaDeposito").trigger('focus');
						return true;
					}, false, ["Aceptar"], "error!", null);
				return;
			}
			else if (box == "") {
				AbrirMensaje("ATENCIÓN",
					"Se deben seleccionar un BOX válido", function () {
						$("#msjModal").modal("hide");
						$("#listaBox").trigger('focus');
						return true;
					}, false, ["Aceptar"], "error!", null);
				return;
			}
			else if (nota == "") {
				AbrirMensaje("ATENCIÓN",
					"Se deben agregar un valor de NOTA válido", function () {
						$("#msjModal").modal("hide");
						$("#Nota").trigger('focus');
						return true;
					}, false, ["Aceptar"], "error!", null);
				return;
			}
			else {
				AbrirMensaje("ATENCIÓN",
					"Faltan datos.", function () {
						$("#msjModal").modal("hide");
						$("#listaDeposito").trigger('focus');
						return true;
					}, false, ["Aceptar"], "error!", null);
				return;
			}
		}
	});
}

function ConfirmarCarga() {
	AbrirWaiting("Guardando Planilla de Elaboración y Fracción...");
	const depoId = $("#listaDeposito").val();
	const boxId = $("#listaBox").val();
	const nota = $("#Nota").val();
	const elaborados = obtenerProductosDeTabla("#tbProdElab");
	const materiaPrima = obtenerProductosDeTabla("#tbProdMp");
	var data = { depoId, boxId, nota, json_elab: elaborados, json_mat_prim: materiaPrima };
	PostGen(data, confirmarPlanillaUrl, function (obj) {
		CerrarWaiting();
		if (obj.error === true || obj.warn === true) {
			console.error('❌ Response:', obj.msg);
			AbrirMensaje("ATENCIÓN", 'Error al intentar confirmar la Planilla de Elaboración y Fracción: ' + (obj.msg || 'Error desconocido'), function () {
				$("#msjModal").modal("hide");
				return true;
			}, false, ["Aceptar"], "error!", null);
		}
		else {
			setTimeout(() => {
				AbrirMensaje(
					'CONFIRMACIÓN EXITOSA',
					'Se ha confirmado la Planilla de Elaboración y Fracción: ' + obj.id,
					function () {
						$('#msjModal').modal('hide');
						//Imprimir Remito
						//ImprimirPlanilla(obj.id);
						console.log("Planilla de Elaboración y Fracción Generada: ", obj.id);
						ResetearPantalla();
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

function ImprimirPlanilla(id) {
	ReseteoDeReportes();
	setTimeout(() => {
		let data = { id: id, sm_tipo: "RE" };
		cargarReporteEnArre(102, data, "REMITO NO FISCAL", "", "");
		invocacionGestorDoc({});
	}, 500);
}

function ReseteoDeReportes() {
	console.log("Reseto de reportes");
	ReporteResetArre();
}

function ResetearPantalla(){
	CancelarCarga();
}

function obtenerProductosDeTabla(selectorTabla) {

	const filas = $(selectorTabla).find("tbody tr[data-p-id]");

	const productos = [];

	filas.each(function () {

		const p_id = $(this).data("p-id");
		const p_desc = $(this).data("p-desc");
		const cantidad = parseFloat($(this).data("cantidad"));

		productos.push({
			p_id,
			p_desc,
			cantidad
		});
	});

	return JSON.stringify(productos);
}


function CancelarCarga() {

	// Limpiar tablas
	limpiarTabla("#divTablaProductosElaborados");
	limpiarTabla("#divTablaProductosMateriaPrima");

	// Limpiar campos superiores
	$("#listaDeposito").val("");   // vuelve a "Seleccionar..."
	$("#listaBox").val("");        // vuelve a "Seleccionar..."
	$("input[name='Nota']").val(""); // limpia nota

	// Opcional: limpiar inputs del módulo activo
	limpiarInputsModulo($("#divTablaProductosElaborados"));
	limpiarInputsModulo($("#divTablaProductosMateriaPrima"));

	// Foco en depósito
	$("#listaDeposito").trigger("focus");
}

function limpiarTabla(selectorBloque) {

	const bloque = $(selectorBloque);
	const tbody = bloque.find("tbody");

	tbody.empty();

	tbody.append(`
        <tr class="fila-vacia">
            <td colspan="4" class="text-center text-muted py-2">
                <i class="bx bx-info-circle me-1"></i>
                No hay items para mostrar.
            </td>
        </tr>
    `);
}


function ProcesarSeleccionFilaEnProdMatPrima($fila) {
	$("#tbProdMp tbody tr").removeClass("selected-row");
	$fila.addClass("selected-row");
}

function ProcesarSeleccionFilaEnProdElab($fila) {
	$("#tbProdElab tbody tr").removeClass("selected-row");
	$fila.addClass("selected-row");
}

function recalcularInterlineado(bloque) {

	const filas = bloque.find("tbody tr:not(.fila-vacia)");

	let alt = true;

	filas.each(function () {
		if (alt) {
			$(this).addClass("alt");
		} else {
			$(this).removeClass("alt");
		}
		alt = !alt;
	});
}

function agregarProductoEnBloque(bloque) {

	// Obtener valores del bloque
	const p_id = bloque.find("#txtIdProd").val();
	const up = bloque.find("#txtUP").val();
	const bto = bloque.find("#txtBto").val();
	const cantidad = bloque.find("#txtUnid").inputmask('unmaskedvalue');
	const descripcion = bloque.find("#txtProDescripcion").val();

	if (!p_id) {
		AbrirMensaje("ATENCIÓN",
			"Debe seleccionar un producto antes de agregarlo.", function () {
				$("#msjModal").modal("hide");
				return true;
			}, false, ["Aceptar"], "error!", null);
		return;
	}

	// Validación: cantidad ≠ 0
	const cantidadNum = parseFloat(cantidad);
	const cantidadUP = parseFloat(up);
	const cantidadBto = parseFloat(bto);
	if (isNaN(cantidadNum) || cantidadNum === 0) {
		AbrirMensaje("ATENCIÓN",
			"La cantidad no puede ser 0.", function () {
				$("#msjModal").modal("hide");
				return true;
			}, false, ["Aceptar"], "error!", null);
		return;
	}

	// Validar si ya existe en la tabla del bloque
	if (productoYaExisteEnTabla(bloque, p_id)) {
		AbrirMensaje("ATENCIÓN",
			"El producto ya existe en la tabla.", function () {
				$("#msjModal").modal("hide");
				return true;
			}, false, ["Aceptar"], "error!", null);
		return;
	}

	// Si no existe → agregarlo
	agregarFilaProducto(bloque, p_id, descripcion, (cantidadUP * cantidadBto) + cantidad);

	// Limpiar inputs del módulo
	limpiarInputsModulo(bloque);

	// Foco en el input de búsqueda del módulo
	bloque.find("input[name='Busqueda']").trigger("focus");
}

function formatearMonto(valor, decimales = 2) {
	if (isNaN(valor)) return "0.00";

	return Number(valor)
		.toFixed(decimales)
		.replace(/\B(?=(\d{3})+(?!\d))/g, ",");
}

function productoYaExisteEnTabla(bloque, p_id) {

	// Buscar filas dentro del bloque
	const filas = bloque.find("tr[data-p-id]");

	let existe = false;

	filas.each(function () {
		if ($(this).data("p-id") == p_id) {
			existe = true;
			return false; // cortar loop
		}
	});

	return existe;
}

function agregarFilaProducto(bloque, p_id, descripcion, cantidad) {

	const tbody = bloque.find("tbody");

	// Si la tabla está vacía, eliminar la fila "No hay items"
	const filaVacia = tbody.find(".fila-vacia");
	if (filaVacia.length) filaVacia.remove();

	// Detectar módulo
	const esElaborados = bloque.find("strong").text().includes("Elaborados");
	const claseFila = esElaborados ? "row-prod-elab" : "row-mat-prima";

	// Formateo de cantidad según decimales permitidos
	const permiteDecimales = bloque.find("#txtUnid").data("permite-decimales") === true;

	// Valor REAL para data-cantidad
	const cantidadReal = permiteDecimales
		? parseFloat(cantidad).toFixed(2)
		: parseInt(cantidad);

	// Valor FORMATEADO para mostrar
	const cantidadFormateada = permiteDecimales
		? formatearMonto(cantidadReal, 2)
		: formatearMonto(cantidadReal, 0);

	// Construcción de la fila
	const nuevaFila = `
        <tr class="${claseFila} fade-in-row" 
            data-p-id="${p_id}"
            data-p-desc="${descripcion}"
            data-cantidad="${cantidadReal}">
            <td class="text-center">${p_id}</td>
            <td class="text-start">${descripcion}</td>
            <td class="text-end">${cantidadFormateada}</td>
            <td class="text-center">
                <div class="d-flex justify-content-center gap-1">
                    <button class="btn btn-danger btn-table btn-sm btnQuitarPedido"
                            data-id="${p_id}"
                            title="Quitar Producto">
                        <i class="bx bx-minus"></i>
                    </button>
                </div>
            </td>
        </tr>
    `;

	tbody.append(nuevaFila);

	recalcularInterlineado(bloque);
}


function limpiarInputsModulo(bloque) {

	bloque.find("#txtIdProd").val("");
	bloque.find("#txtProDescripcion").val("");
	bloque.find("#txtUP").val("");
	bloque.find("#txtBto").val("");
	bloque.find("#txtUnid").val("");
	bloque.find("#txtUP_ID").val("");
	bloque.find("#txtBARRADO_ID").val("");
	bloque.find("#txtID_PROV").val("");
	bloque.find("#Busqueda").val("");

	// Reset de máscaras
	bloque.find("#txtUP").prop("disabled", false);
	bloque.find("#txtBto").prop("disabled", false);
	bloque.find("#txtUnid").prop("disabled", false);
}

function analizaInputUP(e, $input) {
	if (e.key === "Enter" || e.which === 13) {
		e.preventDefault();
		e.stopPropagation();

		const bloque = $input.closest(".grid-golden");
		bloque.find("#txtBto").trigger("focus");
	}
}

function analizaInputBto(e, $input) {
	if (e.key === "Enter" || e.which === 13) {
		e.preventDefault();
		e.stopPropagation();

		const bloque = $input.closest(".grid-golden");

		if (bloque.find("#txtUnid").prop("disabled")) {
			bloque.find("#btnAddProd").trigger("focus");
		} else {
			bloque.find("#txtUnid").trigger("focus");
		}
	}
}


function analizaInputUnid(e, $input) {
	if (e.key === "Enter" || e.which === 13) {
		e.preventDefault();
		e.stopPropagation();

		const bloque = $input.closest(".grid-golden");
		bloque.find("#btnAddProd").trigger("focus");
	}
}


function listaDepositoChange() {
	BuscarBoxDesdeDeposito();
}

function listaBoxesChange() {
}

function BuscarBoxDesdeDeposito() {
	AbrirWaiting();
	var depoId = $("#listaDeposito").val();
	var datos = { depoId };
	PostGenHtml(datos, BuscarBoxesDesdeDepositoURL, function (obj) {
		$("#divComboBox").html(obj);
		const $lista = $("#listaBox");
		const opciones = $lista.find("option");

		// Si solo hay 1 opción real (excluyendo "Seleccionar...")
		if (opciones.length === 2) {
			// opciones[0] = "Seleccionar..."
			// opciones[1] = único box
			const unicoValor = opciones.eq(1).val();
			$lista.val(unicoValor).trigger("change");
		}

		$lista.on("change", listaBoxesChange);
		CerrarWaiting();
		return true
	});
}

function ModificacionesLocales() {
	// Quitar texto del botón Buscar
	$("#divTablaProductosElaborados").find("#btnBusquedaBase").each(function () {
		$(this).contents().filter(function () {
			return this.nodeType === 3; // nodos de texto
		}).remove();
	});
	$("#divTablaProductosMateriaPrima").find("#btnBusquedaBase").each(function () {
		$(this).contents().filter(function () {
			return this.nodeType === 3; // nodos de texto
		}).remove();
	});
	$("#divDetalle .filter-golden .input-group-text").each(function () {
		if ($(this).text().trim() === "ID:") {
			$(this).hide(); // Oculta y NO ocupa espacio
		}
	});
}

function OcultarSpanID() {

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

	// Si no hay bloque destino, no podemos continuar
	if (!bloqueDestino) {
		console.warn("No hay bloque destino definido.");
		return;
	}

	// Detectar si el bloque es Elaborados o Materia Prima
	const esElaborados = bloqueDestino.find("strong").text().includes("Elaborados");
	const esMateriaPrima = bloqueDestino.find("strong").text().includes("Materia Prima");

	// VALIDACIÓN DE TIPO DE PRODUCTO SEGÚN MÓDULO
	if (esElaborados) {
		if (prod.p_elaboracion !== "S") {
			AbrirMensaje("ATENCIÓN",
				`El producto (${prod.p_id}) ${prod.p_desc} no es del tipo Elaboración Propia.`, function () {
					$("#msjModal").modal("hide");
					bloqueDestino.find("input[name='Busqueda']").val("").trigger("focus");
					return true;
				}, false, ["Aceptar"], "error!", null);
			return;
		}
	}

	if (esMateriaPrima) {
		if (prod.p_materia_prima !== "S") {
			AbrirMensaje("ATENCIÓN", `El producto (${prod.p_id}) ${prod.p_desc} no es del tipo Materia Prima.`, function () {
				$("#msjModal").modal("hide");
				bloqueDestino.find("input[name='Busqueda']").val("").trigger("focus");
				return true;
			}, false, ["Aceptar"], "error!", null);
			return;
		}
	}

	// Buscar los inputs dentro del bloque destino
	const $idProd = bloqueDestino.find("#txtIdProd");
	const $desc = bloqueDestino.find("#txtProDescripcion");
	const $up = bloqueDestino.find("#txtUP");
	const $bto = bloqueDestino.find("#txtBto");
	const $unid = bloqueDestino.find("#txtUnid");
	const $upId = bloqueDestino.find("#txtUP_ID");
	const $barradoId = bloqueDestino.find("#txtBARRADO_ID");
	const $provId = bloqueDestino.find("#txtID_PROV");

	// Hidratar valores
	$idProd.val(prod.p_id);
	$desc.val(prod.p_desc);
	$upId.val(prod.up_id);
	$barradoId.val(prod.p_id_barrado);
	$provId.val(prod.p_id_prov);

	// Máscaras
	$up.mask("000.000.000.000", { reverse: true });

	$bto.mask('#,##0', {
		reverse: true,
		translation: {
			'#': { pattern: /-|\d/, recursive: true }
		},
		onChange: function (value, e) {
			e.target.value = value.replace(/(?!^)-/g, '').replace(/^,/, '').replace(/^-,/, '-');
		}
	});

	// Cargar valores
	$up.val(prod.p_unidad_pres);
	$bto.val(prod.bulto);
	// Setear si permite decimales
	$unid.data("permite-decimales", prod.permiteDecimales);

	// Lógica según UP_ID
	if (prod.up_id === "07") {
		getMaskForInteger($unid);
		$unid.val(0).prop("disabled", false);
		$up.prop("disabled", false);
		$bto.prop("disabled", false);
		$up.trigger("focus");
	} else {
		$up.prop("disabled", true);
		$bto.prop("disabled", true);
		getMaskForTwoDecimals($unid);
		$unid.trigger("focus");
	}

	// Limpiar búsqueda del componente común
	bloqueDestino.find("input[asp-for='Busqueda']").val("");
}

function cargarProductos() {
}

function InicializaPantalla() {
}

function EliminarProducto(id) {
}

function getMaskForInteger(selector) {
	$(selector).inputmask({
		alias: 'numeric',
		groupSeparator: ',',       // separador de miles
		digits: 0,                 // sin decimales
		digitsOptional: false,
		allowMinus: false,
		prefix: '',
		suffix: '',
		rightAlign: true,
		unmaskAsNumber: true,
		min: 0
	});
}

function getMaskForTwoDecimals(selector) {
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

function buscarProducto() {

	if (!bloqueDestino) {
		console.warn("No hay bloque destino definido.");
		return;
	}

	AbrirWaiting();

	var _post = busquedaProdBaseUrl;

	// Obtener el input de búsqueda del bloque correcto
	var inputBusqueda = bloqueDestino.find("input[name='Busqueda']");
	var valor = inputBusqueda.val();

	var validarEstado = false;
	var datos = { busqueda: valor, validarEstado };

	// Limpiar tabla del bloque correspondiente
	bloqueDestino.find("#tbGridProd").empty();

	PostGen(datos, _post, function (obj) {

		// ERROR
		if (obj.error === true) {
			CerrarWaiting();
			AbrirMensaje("ATENCIÓN", obj.msg, function () {

				productoBase = null;

				bloqueDestino.find("#estadoFuncion").val(false);
				bloqueDestino.find("#btnBusquedaBase").prop("disabled", false);

				$("#msjModal").modal("hide");
				inputBusqueda.trigger('focus');

				return true;
			}, false, ["Aceptar"], "error!", null);

			return;
		}

		// WARNING
		if (obj.warn === true) {

			CerrarWaiting();

			if (obj.producto.p_id === "0000-0000") {
				AbrirMensaje("ATENCIÓN", obj.msg, function () {

					productoBase = null;

					bloqueDestino.find("#estadoFuncion").val(false);
					bloqueDestino.find("#btnBusquedaBase").prop("disabled", false);

					$("#msjModal").modal("hide");
					inputBusqueda.trigger('focus');

					return true;
				}, false, ["Aceptar"], "error!", null);

				return;
			}

			if (obj.producto.p_id === "NO" && valor != "") {

				if (funcionBusquedaAvanzada === true) {

					AbrirMensaje("ATENCIÓN",
						"NO SE ENCONTRO EL PRODUCTO QUE INTENTO BUSCAR. SE ABRIRÁ LA BUSQUEDA AVANZADA.",
						function () {

							$("#msjModal").modal("hide");

							productoBase = null;
							bloqueDestino.find("#estadoFuncion").val(false);

							inicializaBusquedaAvanzada();
							$("#busquedaModal").modal("toggle");

							return true;
						},
						false, ["Aceptar"], "error!", null
					);

					return;
				}

				AbrirMensaje("ATENCIÓN",
					"NO SE ENCONTRO EL PRODUCTO QUE INTENTO BUSCAR.",
					function () {
						$("#msjModal").modal("hide");
						inputBusqueda.trigger('focus');
						return true;
					},
					false, ["Aceptar"], "error!", null
				);

				return;
			}

			if (obj.producto.p_id === "NO" && valor == "") {

				if (funcionBusquedaAvanzada === true) {
					productoBase = null;
					bloqueDestino.find("#estadoFuncion").val(false);

					inicializaBusquedaAvanzada();
					$("#busquedaModal").modal("toggle");
				}

				return;
			}

			// Encontró producto pero con warning
			AbrirMensaje("ATENCIÓN!", obj.msg, function (resp) {

				if (resp === "SI") {

					productoBase = obj.producto;

					bloqueDestino.find("#estadoFuncion").val(true).trigger("change");

					$("#msjModal").modal("hide");

					ProcesarCargaProducto(prod, bloqueDestino);

					return true;
				}

				// Denegado
				productoBase = null;

				bloqueDestino.find("#estadoFuncion").val(false);
				bloqueDestino.find("#btnBusquedaBase").prop("disabled", false);

				$("#msjModal").modal("hide");
				inputBusqueda.trigger('focus');

				return true;

			}, true, ["Aceptar", "Denegar"], "Warning!", null);

			return;
		}

		CerrarWaiting();
		// OK — producto encontrado
		productoBase = obj.producto;

		bloqueDestino.find("#estadoFuncion").val(true).trigger("change");

		ProcesarCargaProducto(productoBase, bloqueDestino);

		return true;
	});

	return true;
}
