let _pedidoLoading = false;
let soSorteoSeleccionado = null;

var modoNuevoSorteo = false;
var modoModificacionSorteo = false;
var modoEliminacionSorteo = false;

let _sorteoOriginal = null;

$(function () {
	InicializaPantallaPedido();
	InicializaEventosSorteos();
});

function InicializaEventosSorteos() {
	$(document).off("click", "#btnImprimir");
	$(document).on("click", "#btnImprimir", function () {
		if (!soSorteoSeleccionado) {
			alert("Seleccione un sorteo primero.");
			return;
		}
		imprimirSorteo(soSorteoSeleccionado);
	});

	$("#btnImprimir").prop("disabled", true);

	$("#pagEstado").on("change", function () {
		var div = $("#divPaginacion");
		presentaPaginacion(div);
	});

	funcCallBack = buscarSorteos;

	// Evento delegado para el botón de agregar producto
	$(document).on("click", "#btnAgregarCProducto", function () {
		if ($("#busquedaModal").length === 0) {
			cargarModalBusquedaAvanzada(function () {
				if (typeof configurarDestinoBusquedaProductos === 'function') {
					configurarDestinoBusquedaProductos("pedidos", "003", agregarProductosAlGrid, obtenerProductosExistentesIds);
				}
				$("#busquedaModal").modal("show");
			});
		} else {
			if (typeof configurarDestinoBusquedaProductos === 'function') {
				configurarDestinoBusquedaProductos("pedidos", "003", agregarProductosAlGrid, obtenerProductosExistentesIds);
			}
			$("#busquedaModal").modal("show");
		}
	});

	// Buscar
	$("#btnBuscar").on("click", function () {
		buscarSorteos(1);
	});

	// Handler para Nuevo Sorteo
	$(document).on('click', '#btnAbmNuevo', function (e) {
		e.preventDefault();

		if ($("#divFiltro").is(":visible")) {
			$("#divFiltro").collapse("hide");
		}

		modoNuevoSorteo = true;
		desactivarTablaSorteos();
		modoModificacionSorteo = false;
		modoEliminacionSorteo = false;
		soSorteoSeleccionado = null;

		if (typeof nuevoSorteoUrl === 'undefined') {
			console.error('nuevoSorteoUrl no está definido.');
			return;
		}

		PostGenHtml({}, nuevoSorteoUrl, function (html) {
			$('#divSorteoDatos').html(html).show();
			//$('#divSorteoTablas').html(html).show();

			// Primero bloqueo todo
			$('#divSorteoDatos')
				.find('input:not([type=hidden]), textarea, select')
				.each(function () {
					const $el = $(this);
					$el.prop('readonly', true)
						.prop('disabled', true)
						.addClass('campo-readonly');
				});

			// Luego habilito solo los permitidos
			$('#divSorteoDatos')
				.find('#so_desc, #so_desde, #so_hasta, #tipo_valor, #acumula_valor, #so_inclusion_valor, #Rel01B, input[name="todos_los_prod_del_prov"]')
				.each(function () {
					const $el = $(this);
					$el.prop('readonly', false)
						.prop('disabled', false)
						.removeClass('campo-readonly');
				});

			// Finalmente, seteo so_hasta a hoy
			const hoy = new Date().toISOString().split('T')[0];
			$('#so_hasta').val(hoy);
			$('#so_desde').val(hoy);

			const $first = $('#divSorteoDatos').find('input:not([type=hidden]), textarea, select').filter(':visible').first();
			if ($first && $first.length) {
				setTimeout(() => $first.trigger("focus"), 50);
			}

			//$('#divSorteoTablaProductos').html(crearGridProdVacioHtml()).show();
			cargarSorteoTablas("")
			//cargarSorteoTablasSucursales("");
			$('#btnAgregarCProducto').prop('disabled', false);
			$('#btnAbmAceptar').prop('disabled', false).show();
			$('#btnAbmCancelar').prop('disabled', false).show();
			$('#btnAbmModif, #btnAbmNuevo, #btnAbmElimi').prop('disabled', true);

			$("#Rel01B").autocomplete({
				source: function (request, response) {

					data = { prefix: request.term }; /*Rel01*/

					$.ajax({
						url: autoComRel011Url,
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
					$("#Rel01BItem").val(ui.item.id);
				}
			});

			setTimeout(() => {
				agregarHandlerCheckTodosLosProveedores();
			}, 100);
			_pedidoOriginal = null;

			console.log('Modo Nuevo Sorteo activado.');
		}, function (err) {
			console.error('Error al cargar Nuevo Sorteo:', err);
		});
	});

	// Handler para Modificar Sorteo
	$(document).on('click', '#btnAbmModif', function (e) {
		e.preventDefault();

		if ($(this).prop('disabled')) return;

		// 🔍 Obtener la fila seleccionada
		const $filaSeleccionada = $('#tbGridSorteo tbody tr.selected-row');

		if ($filaSeleccionada.length === 0) {
			alert("Debe seleccionar un sorteo.");
			return;
		}

		modoNuevoSorteo = false;
		modoModificacionSorteo = true;
		desactivarTablaSorteos();
		modoEliminacionSorteo = false;

		_sorteoOriginal = capturarEstadoFormularioSorteo();
		habilitarCamposFormularioSorteo(true);
		$('#btnAgregarCProducto').prop('disabled', false);
		$('#btnAbmNuevo, #btnAbmModif, #btnAbmElimi').prop('disabled', true);
		$('#btnAbmAceptar, #btnAbmCancelar').prop('disabled', false).show();

		//aplicarReadonlyCamposPedido();

		setTimeout(() => {
			agregarHandlerCheckTodosLosProveedores();
			var chequeado = $("#todos_los_prod_del_prov").is(":checked");
			if (!chequeado) {
				$('#btnAgregarCProducto').prop('disabled', false);
			}
			else {
				$('#btnAgregarCProducto').prop('disabled', true);
			}
		}, 100);


		const $primer = $('#divSorteoDatos').find('input:not([type=hidden]):not([readonly]), textarea:not([readonly]), select:not([disabled])').filter(':visible').first();
		if ($primer.length) {
			setTimeout(() => $primer.trigger("focus"), 50);
		}

		console.log('✅ Modo Modificación Sorteo activado');
	});

	$(document).on('click', '#btnAbmElimi', function (e) {
		e.preventDefault();
		if ($(this).prop('disabled')) return;

		//const pcCompte = $('#pc_compte').val();
		if (!soSorteoSeleccionado || soSorteoSeleccionado.trim() === '') {
			AbrirMensaje("ATENCIÓN", "Debe seleccionar un sorteo para anular.", function () {
				$("#msjModal").modal("hide");
				return true;
			}, false, ["Aceptar"], "error!", null);
		}
		else {
			desactivarTablaSorteos();
			const ctaDenominacion = $('#cta_denominacion').val() || 'Sin cliente';
			const vigenciaDesde = $('#so_desde').val() || '';
			const vigenciaHasta = $('#so_hasta').val() || '';

			const mensajeConfirmacion = `
                    <div class="text-start">
                        <p class="mb-2"><strong>¿Está seguro que desea anular este sorteo?</strong></p>
                        <hr class="my-2">
                        <p class="mb-1"><strong>ID:</strong> ${soSorteoSeleccionado}</p>
                        <p class="mb-1"><strong>Cliente:</strong> ${ctaDenominacion}</p>
                        <hr class="my-2">
                        <p class="text-danger mb-0">
                            <i class="bx bx-error-circle me-1"></i>
                            <strong>Esta acción no se puede deshacer.</strong>
                        </p>
                    </div>
                    `;

			AbrirMensaje(
				'ANULAR SORTEO',
				mensajeConfirmacion,
				function (resp) {
					if (resp === 'SI') {
						eliminarSorteo();
					}
					activarTablaSorteos();
					$('#msjModal').modal('hide');
				},
				true,
				['Eliminar', 'Cancelar'],
				'warn!',
				null
			);
		}
	});
}

function obtenerProductosExistentesIds() {
	const productosIds = [];

	$('#tbSorteoProd tbody tr').each(function () {
		const $fila = $(this);
		if ($fila.find('td[colspan]').length > 0) return;

		const pId = $fila.data('p-id');
		if (pId) {
			productosIds.push(pId);
		}
	});

	return productosIds;
}

/**
* ✅ NUEVO: Configura eventos de eliminación de productos
* Usa delegación de eventos para botones dinámicos
*/
function configurarEventosEliminacionProducto() {
	// ✅ REMOVER LISTENER PREVIO para evitar duplicados
	$(document).off('click', '.btn-eliminar-producto');

	// ✅ DELEGACIÓN DE EVENTOS (más performante para elementos dinámicos)
	$(document).on('click', '.btn-eliminar-producto', function (e) {
		e.preventDefault();
		e.stopPropagation();

		const $btn = $(this);
		const pId = $btn.data('p-id');
		const $fila = $btn.closest('tr');
		const pDesc = $fila.find('td:nth-child(2)').text().trim();

		confirmarEliminacionProducto(pId, pDesc, $fila);
	});
}

/**
* ✅ NUEVO: Confirma y ejecuta eliminación de producto del grid
* @param {string} pId - ID del producto
* @param {string} pDesc - Descripción del producto
* @param {jQuery} $fila - Fila a eliminar
*/
function confirmarEliminacionProducto(pId, pDesc, $fila) {
	AbrirMensaje(
		'ELIMINAR PRODUCTO',
		`¿Está seguro que desea eliminar el producto "${pDesc}" del sorteo?`,
		function (resp) {
			if (resp === 'SI') {
				eliminarProductoDelGrid($fila);
			}
			$('#msjModal').modal('hide');
		},
		true,
		['Eliminar', 'Cancelar'],
		'warn!',
		null
	);
}

/**
 * ✅ NUEVO: Elimina producto del grid y actualiza totales
 * @param {jQuery} $fila - Fila a eliminar
 */
function eliminarProductoDelGrid($fila) {
	const pDesc = $fila.find('td:nth-child(2)').text().trim();

	// ✅ ANIMACIÓN SUAVE (mejor UX)
	$fila.fadeOut(300, function () {
		$(this).remove();

		// ✅ VERIFICAR SI QUEDARON PRODUCTOS
		const $tbody = $('#tbSorteoProd tbody');
		if ($tbody.find('tr[data-p-id]').length === 0) {
			$tbody.html(`
                <tr>
                    <td colspan="3" class="text-center text-muted py-2">
                        <i class="bx bx-info-circle me-1"></i>No hay productos en este sorteo
                    </td>
                </tr>
            `);

			// ✅ REMOVER FOOTER si no hay productos
			$('#tbSorteoProd tfoot').remove();
		} else {
			// ✅ REAJUSTAR CLASES ALTERNADAS
			reajustarClasesAlternadas();
		}

		// ✅ ACTUALIZAR TOTAL
		//actualizarTotalGeneralPedido();

		ControlaMensajeSuccess(`Producto "${pDesc}" eliminado correctamente`);
	});
}

/**
* ✅ NUEVO: Reajusta clases 'alt' después de eliminar filas
* Mantiene consistencia visual
*/
function reajustarClasesAlternadas() {
	$('#tbSorteoProd tbody tr[data-p-id]').each(function (index) {
		const $fila = $(this);

		if (index % 2 === 0) {
			$fila.removeClass('alt');
		} else {
			$fila.addClass('alt');
		}
	});
}

function validarCliente() {
	var chequeado = $("#todos_los_prod_del_prov").is(":checked");
	if (!chequeado) {
		return true;
	}
	// Caso 1: Sorteo nuevo → se usa Rel01B
	const rel01 = $("#Rel01B");
	if (rel01.length && !rel01.prop("readonly")) {
		const valor = rel01.val()?.trim();
		const item = $("#Rel01BItem").val();

		if (!valor || !item) {
			//alert("Debe seleccionar un cliente válido.");
			return false;
		}

		return true;
	}

	// Caso 2: Sorteo existente → se usa cta_denominacion
	const cta = $("#cta_id");
	if (cta.length) {
		const valor = cta.val()?.trim();

		if (!valor) {
			//alert("El cliente del sorteo no es válido.");
			return false;
		}

		return true;
	}

	// Si no existe ninguno, es un error de estructura
	//alert("No se encontró un campo de cliente para validar.");
	return false;
}

function actualizarHabilitacionFilaSucursal($row) {
	const chk = $row.find(".chkIncluye").is(":checked");

	const $inputs = $row.find(".so-desde, .so-hasta");

	if (chk) {
		// Habilitar edición
		$inputs.prop("disabled", false)
			.removeClass("disabled-cell");
	} else {
		// Deshabilitar y blanquear valores
		$inputs.prop("disabled", true)
			.val("")                     // ← BLANQUEAR
			.removeClass("error-range")  // ← limpiar errores previos
			.addClass("disabled-cell");
	}
}


// ============================================================================
// FUNCIONES DE VALIDACIÓN Y CONFIRMACIÓN DE PEDIDO
// ============================================================================

/**
 * ✅ Valida los datos del sorteo antes de confirmar
 * @param {string} abm - Tipo de operación: 'A', 'M', 'B'
 * @returns {object} { esValido: boolean, mensaje: string }
 */
function validarSorteo(abm) {
	console.log(`🔍 Validando sorteo (Modo: ${abm})...`);

	// VALIDACIÓN 1: Cliente obligatorio
	const ctaValidar = validarCliente();
	if (!ctaValidar) {
		return { esValido: false, mensaje: 'Debe seleccionar un cliente para el sorteo.' };
	}

	// VALIDACIÓN 2: Productos (si NO está chequeado "todos los productos")
	var chequeado = $("#todos_los_prod_del_prov").is(":checked");
	if (!chequeado) {
		const productos = obtenerProductosDelGrid();
		if (!productos || productos.length === undefined || productos.length === 0) {
			return { esValido: false, mensaje: 'Debe agregar al menos un producto al sorteo.' };
		}
	}

	// VALIDACIÓN 3: Sucursales
	const sucursales = obtenerSucursalesDelGrid();
	if (!sucursales || sucursales.length === 0) {
		return { esValido: false, mensaje: 'Debe agregar al menos una sucursal al sorteo.' };
	}

	// Filtrar solo las seleccionadas (incluye = 1)
	const seleccionadas = sucursales.filter(s => s.incluido === true);

	if (seleccionadas.length === 0) {
		return { esValido: false, mensaje: 'Debe seleccionar al menos una sucursal.' };
	}

	// VALIDACIÓN 4: Validar rangos individuales
	for (let s of seleccionadas) {

		if (s.so_nro_desde <= 0 || s.so_nro_hasta <= 0) {
			return {
				esValido: false,
				mensaje: `Sucursal ${s.adm_nombre}: Los valores deben ser mayores a 0.`
			};
		}

		if (s.so_nro_desde >= s.so_nro_hasta) {
			return {
				esValido: false,
				mensaje: `Sucursal ${s.adm_nombre}: El valor "Desde" debe ser menor que "Hasta".`
			};
		}
	}

	// VALIDACIÓN 5: Solapamiento entre sucursales
	for (let i = 0; i < seleccionadas.length; i++) {
		for (let j = i + 1; j < seleccionadas.length; j++) {

			const A = seleccionadas[i];
			const B = seleccionadas[j];

			const solapan =
				A.so_nro_desde <= B.so_nro_hasta &&
				B.so_nro_desde <= A.so_nro_hasta;

			if (solapan) {
				return {
					esValido: false,
					mensaje: `Los rangos de las sucursales "${A.adm_nombre}" y "${B.adm_nombre}" se solapan.`
				};
			}
		}
	}

	// VALIDACIÓN 6: Nombre del sorteo
	var nombre = $("#so_desc").val();
	if (!nombre || nombre.trim() === "") {
		return { esValido: false, mensaje: 'Debe indicar un nombre válido para el sorteo.' };
	}

	// VALIDACIÓN X: so_inclusion_valor > 0
	let valorStr = $("#so_inclusion_valor").val() || "0";
	let valor = parseInt(valorStr.replace(/\./g, "")) || 0;

	if (valor <= 0) {
		return {
			esValido: false,
			mensaje: 'Debe indicar un valor mayor a 0 en "Valor".'
		};
	}

	console.log('✅ Validación exitosa');
	return { esValido: true, mensaje: '' };
}

//function validarSorteo(abm) {
//	console.log(`🔍 Validando sorteo (Modo: ${abm})...`);

//	// ✅ VALIDACIÓN 1: Cliente obligatorio
//	const ctaValidar = validarCliente();
//	if (!ctaValidar) {
//		return {
//			esValido: false,
//			mensaje: 'Debe seleccionar un cliente para el sorteo.'
//		};
//	}

//	// ✅ VALIDACIÓN 6: Debe haber al menos un producto
//	var chequeado = $("#todos_los_prod_del_prov").is(":checked");
//	if (!chequeado) {
//		const productos = obtenerProductosDelGrid();
//		if (productos == null || productos == undefined)
//			return;
//		if (productos.length === 0) {
//			return {
//				esValido: false,
//				mensaje: 'Debe agregar al menos un producto al sorteo'
//			};
//		}
//	}

//	const sucursales = obtenerSucursalesDelGrid();
//	if (sucursales == null || sucursales == undefined)
//		return;
//	if (sucursales.length === 0) {
//		return {
//			esValido: false,
//			mensaje: 'Debe agregar al menos una sucursal al sorteo'
//		};
//	}


//	var nombre = $("#so_desc").val();
//	if (!nombre || nombre == "") {
//		return {
//			esValido: false,
//			mensaje: 'Debe indicar un nombre válido para el sorteo.'
//		};
//	}
//	console.log('✅ Validación exitosa');
//	return { esValido: true, mensaje: '' };
//}

// Handler para Aceptar/Confirmar Sorteo
$(document).on('click', '#btnAbmAceptar', function (e) {
	e.preventDefault();

	if ($(this).prop('disabled')) return;

	// Determinar modo ABM
	let abm = '';
	if (modoNuevoSorteo) {
		abm = 'A'; // Alta
	} else if (modoModificacionSorteo) {
		abm = 'M'; // Modificación
	} else if (modoEliminacionSorteo) {
		abm = 'B'; // Baja
	} else {
		console.error('⚠️ Modo de operación no determinado');
		ControlaMensajeError('No se puede determinar la operación a realizar');
		return;
	}

	// Validar antes de confirmar
	const validacion = validarSorteo(abm);
	if (validacion == null || validacion == undefined)
		return;
	if (!validacion.esValido) {
		ControlaMensajeWarning(validacion.mensaje);
		AbrirMensaje("ATENCIÓN", validacion.mensaje, function () {
			$("#msjModal").modal("hide");
			return true;
		}, false, ["Aceptar"], "error!", null);
		return;
	}
	else {
		// Mostrar confirmación
		const mensajeConfirmacion = abm === 'A'
			? '¿Desea confirmar la creación del sorteo?'
			: '¿Desea confirmar las modificaciones del sorteo?';

		AbrirMensaje(
			'CONFIRMAR SORTEO',
			mensajeConfirmacion,
			function (resp) {
				if (resp === 'SI') {
					confirmarSorteo(abm);
				}
				//activarTablaSorteos();
				//$("#divSorteo")
				//	.removeClass("table-wrapper-small")
				//	.addClass("table-wrapper-full");
				//$('#msjModal').modal('hide');
			},
			true,
			['Confirmar', 'Cancelar'],
			'info!',
			null
		);
	}
});

/**
 * ✅ Confirma el sorteo enviándolo al servidor
 * @param {string} abm - Tipo de operación: 'A', 'M', 'B'
 */
function confirmarSorteo(abm) {
	console.log(`📤 Confirmando sorteo (Modo: ${abm})...`);

	try {
		// Construir objeto de confirmación
		const confirmacionDto = construirSorteoConfirmaDto(abm);

		// 🔥 Si hubo error en la construcción del DTO, detener todo
		if (!confirmacionDto) {
			return;
		}

		AbrirWaiting('Confirmando sorteo...');

		// Debug: Ver estructura completa
		console.log('📦 DTO de confirmación:', confirmacionDto);

		PostGen(confirmacionDto, confirmarSorteoUrl, function (obj) {
			CerrarWaiting();
			if (obj.error === true || obj.warn === true) {
				console.error('❌ Response:', obj.msg);
				AbrirMensaje("ATENCIÓN", 'Error al intentar confirmar el sorteo: ' + (obj.msg || 'Error desconocido'), function () {
					$("#msjModal").modal("hide");
					return true;
				}, false, ["Aceptar"], "error!", null);
			}
			else {
				procesarRespuestaConfirmacion(obj, abm);
				//if (abm == 'A' || abm == 'M')
					//ImprimirPedido_Generado(response.id);
				//activarTablaSorteos();
				//$("#divSorteo")
				//	.removeClass("table-wrapper-small")
				//	.addClass("table-wrapper-full");
				//$('#msjModal').modal('hide');
			}
		}, function (error, xhr) { 
			CerrarWaiting();
			console.error('❌ Error al confirmar pedido:', error);
			console.error('❌ Response:', xhr.responseText);
			ControlaMensajeError(
				'Error al confirmar el pedido: ' +
				(xhr.responseJSON?.mensaje || xhr.statusText || 'Error desconocido')
			);
		});

		//$.ajax({
		//	url: confirmarSorteoUrl,
		//	type: 'POST',
		//	contentType: 'application/json; charset=utf-8', // ⚠️ CRUCIAL
		//	data: JSON.stringify(confirmacionDto), // ⚠️ SERIALIZAR EXPLÍCITAMENTE
		//	dataType: 'json',
		//	success: function (response) {
		//		CerrarWaiting();
		//		if (response.error === true || response.warn === true) {
		//			console.error('❌ Response:', response.msg);
		//			AbrirMensaje("ATENCIÓN", 'Error al intentar confirmar el sorteo: ' + (response.msg || 'Error desconocido'), function () {
		//				$("#msjModal").modal("hide");
		//				return true;
		//			}, false, ["Aceptar"], "error!", null);
		//		}
		//		else {
		//			procesarRespuestaConfirmacion(response, abm);
		//			if (abm == 'A' || abm == 'M')
		//				ImprimirPedido_Generado(response.id);
		//		}
		//	},
		//	error: function (xhr, status, error) {
		//		CerrarWaiting();
		//		console.error('❌ Error al confirmar pedido:', error);
		//		console.error('❌ Response:', xhr.responseText);
		//		ControlaMensajeError(
		//			'Error al confirmar el pedido: ' +
		//			(xhr.responseJSON?.mensaje || xhr.statusText || 'Error desconocido')
		//		);
		//	}
		//});

	} catch (error) {
		CerrarWaiting();
		console.error('❌ Error al construir DTO:', error);
		ControlaMensajeError('Error al procesar los datos del pedido: ' + error.message);
	}
}

/**
 * ✅ Procesa la respuesta del servidor después de confirmar
 * @param {object} response - Respuesta del servidor
 * @param {string} abm - Tipo de operación
 */
function procesarRespuestaConfirmacion(response, abm) {
	console.log('📥 Respuesta del servidor:', response);

	if (response.error || response.warn) {
		if (response.error) {
			AbrirMensaje("ATENCIÓN", response.mensaje || 'Error al confirmar el sorteo', function () {
				$("#msjModal").modal("hide");
				return true;
			}, false, ["Aceptar"], "error!", null);
			return;
		}
		else //warn
		{
			AbrirMensaje("ATENCIÓN", response.mensaje || 'Atención al confirmar el sorteo', function () {
				$("#msjModal").modal("hide");
				return true;
			}, false, ["Aceptar"], "error!", null);
			return;
		}
	}

	// Mensaje de éxito según el tipo de operación
	let mensajeExito = '';
	switch (abm) {
		case 'A':
			mensajeExito = 'Sorteo creado exitosamente';
			break;
		case 'M':
			mensajeExito = 'Sorteo modificado exitosamente';
			break;
		case 'B':
			mensajeExito = 'Sorteo eliminado exitosamente';
			break;
		default:
			mensajeExito = 'Operación completada exitosamente';
	}

	// Mostrar mensaje y redirigir
	AbrirMensaje(
		'CONFIRMACIÓN EXITOSA',
		mensajeExito,
		function () {
			$('#msjModal').modal('hide');

			// Resetear formulario y volver al inicio
			cancelarOperacion();

			// Si hay ID de sorteo en la respuesta, imprimir el sorteo
			if (response.id) {
				// Opcional: Recargar el sorteo recién creado/modificado
				console.log('✅ Sorteo ID:', response.pc_compte);
			}
		},
		false,
		['Aceptar'],
		'success!',
		null
	);
}

function cancelarOperacion(e) {
	console.log('🔄 Cancelando operación de sorteo...');
	modoNuevoSorteo = false;
	modoModificacionSorteo = false;
	campoEnEdicionSorteo = null;
	_sorteoOriginal = null;

	$("#divSorteoDatos, #divSorteoTablas").empty().hide();

	const $filaSeleccionada = $("#tbGridSorteo tbody tr.selected-row");
	const haySorteoSeleccionado = $filaSeleccionada.length > 0;

	if (haySorteoSeleccionado) {
		$("#btnAbmModif").prop("disabled", !haySorteoSeleccionado);
		$("#btnAbmElimi").prop("disabled", !haySorteoSeleccionado);
		$("#btnAbmNuevo").prop("disabled", false);
		$("#btnImprimir").prop("disabled", false);

	} else {
		// Si no hay selección, solo habilitar Nuevo
		$("#btnAbmNuevo").prop("disabled", false);
		$("#btnAbmModif, #btnAbmElimi, #btnImprimir").prop("disabled", true);
	}

	$("#btnAbmAceptar, #btnAbmCancelar, #btnImprimir").prop("disabled", true).hide();
	$("#btnAgregarCProducto").prop("disabled", true);
	$("#tbGridSorteo tbody tr").removeClass("selectedEdit-row").removeClass("selected-row");

	console.log('✅ Operación cancelada - Vista reinicializada');

	$("#divSorteo")
		.removeClass("table-wrapper-small")
		.addClass("table-wrapper-full");
	activarTablaSorteos();

	if ($("#divDetalle").is(":visible")) {
		$("#divDetalle").collapse("hide");
	}
	$("#divFiltro").collapse("show");
}

/**
 * ✅ Construye el DTO ConfirmarSorteoDto
 * @param {string} abm - Tipo de operación
 * @returns {object} ConfirmarSorteoDto
 */
function construirSorteoConfirmaDto(abm) {
	const productos = obtenerProductosDelGrid();
	const sucursales = obtenerSucursalesDelGrid();

	if (!productos || !sucursales)  return null; // 🔥 Evita continuar si hubo error
	return {
		Abm: abm,
		Datos: obtenerDatosFormularioSorteo(abm),
		Productos: productos,
		Sucursales: sucursales
	};
}

function eliminarSorteo() {
	console.log('🗑️ Eliminando sorteo...');

	if (!soSorteoSeleccionado || soSorteoSeleccionado.trim() === '') {
		ControlaMensajeError('Error: No se encontró el ID del sorteo para anular');
		return;
	}

	AbrirWaiting('Anulando sorteo...');

	try {
		const confirmacionDto = {
			Abm: 'B',
			Datos: obtenerDatosFormularioSorteo("B"),
			Productos: obtenerProductosDelGrid(),
			Sucursales: obtenerSucursalesDelGrid(),
		};

		console.log('📦 DTO de eliminación:', confirmacionDto);

		PostGen(confirmacionDto, confirmarSorteoUrl, function (obj) {
			CerrarWaiting();
			if (obj.error === true || obj.warn === true) {
				console.error('❌ Response:', obj.msg);
				AbrirMensaje("ATENCIÓN", 'Error al intentar anular el sorteo: ' + (obj.msg || 'Error desconocido'), function () {
					$("#msjModal").modal("hide");
					return true;
				}, false, ["Aceptar"], "error!", null);
			}
			else {
				procesarRespuestaEliminacion(obj);
			}
		}, function (error, xhr) {
			CerrarWaiting();
			console.error('❌ Error al anular sorteo:', error);
			console.error('❌ Response:', xhr.responseText);
			ControlaMensajeError(
				'Error al anular el sorteo: ' +
				(xhr.responseJSON?.mensaje || xhr.statusText || 'Error desconocido')
			);
		});

		//$.ajax({
		//	url: confirmarSorteoUrl,
		//	type: 'POST',
		//	contentType: 'application/json; charset=utf-8', // ⚠️ CRUCIAL
		//	data: JSON.stringify(confirmacionDto), // ⚠️ SERIALIZAR EXPLÍCITAMENTE
		//	dataType: 'json',
		//	success: function (response) {
		//		CerrarWaiting();
		//		procesarRespuestaEliminacion(response);
		//	},
		//	error: function (xhr, status, error) {
		//		CerrarWaiting();
		//		console.error('❌ Error al anular pedido:', error);
		//		console.error('❌ Response:', xhr.responseText);
		//		ControlaMensajeError(
		//			'Error al anular pedido: ' +
		//			(xhr.responseJSON?.mensaje || xhr.statusText || 'Error desconocido')
		//		);
		//	}
		//});
	} catch (error) {
		CerrarWaiting();
		console.error('❌ Error al construir DTO:', error);
		ControlaMensajeError('Error al procesar la anulación: ' + error.message);
	}
}

function procesarRespuestaEliminacion(response) {
	console.log('📥 Respuesta de eliminación:', response);

	if (response.error || response.warn) {
		if (response.error) {
			ControlaMensajeError(response.mensaje || 'Error al anular el sorteo');
			return;
		}
		else //warn
		{
			ControlaMensajeWarning(response.mensaje || 'Atención al intentar anular el sorteo');
			return;
		}

	}

	AbrirMensaje(
		'ANULACIÖN EXITOSA',
		'El sorteo ha sido anulado correctamente',
		function () {
			$('#msjModal').modal('hide');
			cancelarOperacion();

			if ($('#tbGridSorteo tbody tr').length > 0) {
				console.log('🔄 Actualizando lista de sorteos...');
				buscarSorteos(1);
			}
		},
		false,
		['Aceptar'],
		'success!',
		null
	);
}

function obtenerDatosFormularioSorteo(abm) {
	let so_sorteo = "";
	if (abm == "A") {
		so_sorteo = null;
	}
	else {
		so_sorteo = soSorteoSeleccionado;
	}
	
	let ctaId = "";

	// Si está visible el input editable del proveedor
	if ($("#Rel01B").is(":visible")) {
		ctaId = $("#Rel01BItem").val() || "";
	}
	// Si está visible el proveedor readonly
	else if ($("#cta_denominacion").is(":visible")) {
		ctaId = $("#cta_id").val() || "";
	}

	let so_desc = $('#so_desc').val() || '';
	let so_desde = $('#so_desde').val() || '';
	let so_hasta = $('#so_hasta').val() || '';
	let so_participan = '';
	if ($("#todos_los_prod_del_prov").is(":checked")) {
		so_participan = "A";
	}
	else {
		so_participan = "T";
	}
	if (so_participan == "A") {
		cta_id = null;
	}
	let so_inclusion_tipo = $('#tipo_valor').val() || '';
	let so_inclusion_acumula = $('#acumula_valor').val() || '';
	let so_inclusion_valor = $('#so_inclusion_valor').val() || '';
	const datos = {
		so_sorteo,
		cta_id: ctaId,
		so_desc,
		so_desde,
		so_hasta,
		so_participan,
		so_inclusion_tipo,
		so_inclusion_acumula,
		so_inclusion_valor
	};

	console.log('📋 Datos del formulario capturados:', datos);
	return datos;
}

function obtenerProductosDelGrid() {
	const productos = [];
	const $filas = $('#tbSorteoProd tbody tr');

	let tieneProductos = false;

	$filas.each(function () {
		const $fila = $(this);

		// Si es la fila "No hay items para mostrar." → ignorar
		if ($fila.find('td[colspan]').length > 0) return;

		const pId = $fila.data('p-id');
		if (!pId) return;

		tieneProductos = true;

		const pDes = $fila.data('p-desc') || "";

		productos.push({
			p_id: pId,
			p_desc: pDes
		});
	});

	// ❗ Si no hay productos → devolver objeto vacío {}
	if (!tieneProductos) {
		return {};
	}

	return productos;
}


function obtenerSucursalesDelGrid() {

	let sucursales = [];

	$("#tbSorteoAdm tbody tr").each(function () {

		const $row = $(this);

		// ❗ Solo incluir filas seleccionadas
		const incluye = $row.find(".chkIncluye").is(":checked");
		if (!incluye) return;

		const admId = $row.data("adm-id");
		const admNombre = $row.data("adm-nombre");

		const desdeStr = $row.find(".so-desde").val() || "0";
		const hastaStr = $row.find(".so-hasta").val() || "0";

		const desde = parseInt(desdeStr.replace(/\./g, "")) || 0;
		const hasta = parseInt(hastaStr.replace(/\./g, "")) || 0;

		sucursales.push({
			adm_id: admId,
			adm_nombre: admNombre,
			so_nro_desde: desde,
			so_nro_hasta: hasta,
			incluido: true
		});
	});

	return sucursales;
}



function buscarSorteos(pag = 1) {
	if (_pedidoLoading) return;
	_pedidoLoading = true;
	pagina = pag;
	try {
		AbrirWaiting("Buscando Pedidos de Cliente...")
		const filtros = buildQueryFilters(pag);
		const url = buscarSorteoListaUrl;
		PostGenHtml(filtros, url, function (html) {
			$("#divDetalle").html(html).collapse("show");
			$("#divFiltro").collapse("hide");

			configurarEventosSeleccionDeSorteo();

			CerrarWaiting();
			PostGen({}, buscarMetadataURL, function (obj) {
				if (obj.error === true) {
					AbrirMensaje("ATENCIÓN", obj.msg, function () {
						$("#msjModal").modal("hide");
						return true;
					}, false, ["Aceptar"], "error!", null);
				} else {
					totalRegs = obj.metadata.totalCount;
					pags = obj.metadata.totalPages;
					pagRegs = obj.metadata.pageSize;
					$("#pagEstado").val(true).trigger("change");
				}
			});
		});
	}
	catch (e) {
		console.error("Error al buscar sorteos:", e);
		$("#divDetalle").html('<div class="alert alert-danger py-2 mb-0">No se pudo obtener la información.</div>').collapse("show");
	} finally {
		_pedidoLoading = false;
	}
}

function configurarEventosSeleccionDeSorteo() {
	$(document).off("click", "#tbGridSorteo tbody tr");
	$(document).off("dblclick", "#tbGridSorteo tbody tr");

	$(document).on("click", "#tbGridSorteo tbody tr", function (e) {

		if (!$(e.target).is("button, a, .btn, i")) {

			const $this = $(this);

			// Quitar selección previa
			$("#tbGridSorteo tbody tr").removeClass("selected-row");

			// Marcar fila seleccionada
			$this.addClass("selected-row");

			// Guardar valor seleccionado
			soSorteoSeleccionado = $this.data("so-sorteo");

			// Habilitar botón imprimir
			if (soSorteoSeleccionado) {
				$("#btnImprimir").prop("disabled", false).show();
			}
		}
	});

	// ============================
	// DOBLE‑CLICK → Cargar datos + achicar grid
	// ============================
	$(document).on("dblclick", "#tbGridSorteo tbody tr", function (e) {

		if (!$(e.target).is("button, a, .btn, i")) {

			const $this = $(this);
			const soSorteo = $this.data("so-sorteo");

			if (!soSorteo) return;

			// Ejecutar funciones de carga
			let data = { so_sorteo: soSorteo };
			//cargarReporteEnArre(62, data, "Pedido de Cliente", "", "");
			cargarSorteoDatos(soSorteo);
			cargarSorteoTablas(soSorteo);

			// Achicar grid
			const $grid = $("#divSorteo");
			if (!$grid.hasClass("table-wrapper-100")) {
				$grid.removeClass("table-wrapper-full").addClass("table-wrapper-small");
			}

			// Reposicionar fila seleccionada
			setTimeout(() => {
				posicionarRegOnTop($this, ".table-wrapper-small");
			}, 200);
		}
	});

	// Eventos de eliminación
	configurarEventosEliminacionProducto();
}

function cargarSorteoDatos(soSorteo) {
	var datos = { so_sorteo: soSorteo };
	AbrirWaiting("Cargando datos del sorteo...");
	PostGenHtml(datos, obtenerSorteoDatosUrl, function (html) {
		$("#divSorteoDatos").html(html).show();
		$("#btnAbmModif").prop("disabled", false);
		$("#btnAbmElimi").prop("disabled", false);

		// Debug - ayuda a identificar estados del sistema
		console.log("cargarSorteoDatos N°: ", soSorteo,
			"Permite edición:", true);
		aplicarMascaraDecimales();
		CerrarWaiting();
	});
}

function cargarSorteoTablas(soSorteo) {
	var data = {};
	AbrirWaiting();
	PostGenHtml(data, obtenerSorteoTablasUrl, function (html) {
		$("#divSorteoTablas").html(html).show();
		cargarSorteoTablasSucursales(soSorteo);
		cargarSorteoTablasProductos(soSorteo);
		// Debug - ayuda a identificar estados del sistema
		console.log("cargarSorteoTablas N°: ", soSorteo);

		CerrarWaiting();
	});
}

function cargarSorteoTablasSucursales(soSorteo) {
	var data = { so_sorteo: soSorteo };
	AbrirWaiting("Cargando sucursales del sorteo...");
	PostGenHtml(data, obtenerSorteoTablasSucursalesUrl, function (html) {
		$("#divSorteoTablaSucursales").html(html).show();
		CerrarWaiting();
		inicializarEventosTablaSucursales();
	});
}

function cargarSorteoTablasProductos(soSorteo) {
	var data = { so_sorteo: soSorteo };
	AbrirWaiting("Cargando productos del sorteo...");
	PostGenHtml(data, obtenerSorteoTablasProductosUrl, function (html) {
		$("#divSorteoTablaProductos").html(html).show();
		CerrarWaiting();
		inicializarEventosTablaProductos();
	});
}

function inicializarEventosTablaSucursales() {
	// Seleccionar / deseleccionar todos
	$(document).off("change", "#chkAllIncluye");
	$(document).on("change", "#chkAllIncluye", function () {
		const checked = $(this).is(":checked");

		$("#tbSorteoAdm tbody .chkIncluye").prop("checked", checked);

		$("#tbSorteoAdm tbody tr").each(function () {
			actualizarHabilitacionFilaSucursal($(this));
		});
	});

	$(document).off("change", ".chkIncluye");
	$(document).on("change", ".chkIncluye", function () {
		const $row = $(this).closest("tr");
		actualizarHabilitacionFilaSucursal($row);

		const total = $("#tbSorteoAdm tbody .chkIncluye").length;
		const marcados = $("#tbSorteoAdm tbody .chkIncluye:checked").length;

		$("#chkAllIncluye").prop("checked", total === marcados);
	});
	aplicarMascaraEnteros();
	$(document).off("blur", ".input-editable");
	//$(document).on("blur", ".input-editable", function () {
	//	validarRangosSorteoAdm();
	//});
	$(document).off("keydown", ".input-editable");
	$(document).on("keydown", ".input-editable", function (e) {

		const navegacion = ["Enter", "Tab", "ArrowRight", "ArrowLeft", "ArrowDown", "ArrowUp"];

		// Si NO es una tecla de navegación → dejar escribir
		if (!navegacion.includes(e.key)) {
			return; // permitir escritura normal
		}

		e.preventDefault();

		const $inputs = $("#tbSorteoAdm .input-editable");
		const index = $inputs.index(this);
		let newIndex = index;

		switch (e.key) {
			case "Enter":
			case "Tab":
				newIndex = e.shiftKey
					? (index - 1 + $inputs.length) % $inputs.length
					: (index + 1) % $inputs.length;
				break;

			case "ArrowRight":
				newIndex = (index + 1) % $inputs.length;
				break;

			case "ArrowLeft":
				newIndex = (index - 1 + $inputs.length) % $inputs.length;
				break;

			case "ArrowDown":
				newIndex = buscarInputAbajo(index, $inputs);
				break;

			case "ArrowUp":
				newIndex = buscarInputArriba(index, $inputs);
				break;
		}

		const $next = $inputs.eq(newIndex);
		$next.focus().select();
	});

	// Al cargar la tabla, ajustar todas las filas
	$("#tbSorteoAdm tbody tr").each(function () {
		actualizarHabilitacionFilaSucursal($(this));
	});
}

function buscarInputAbajo(index, $inputs) {
	const col = index % 2; // 0 = desde, 1 = hasta
	const fila = Math.floor(index / 2);

	const totalFilas = $("#tbSorteoAdm tbody tr").length;
	const nuevaFila = (fila + 1) % totalFilas;

	return nuevaFila * 2 + col;
}

function buscarInputArriba(index, $inputs) {
	const col = index % 2;
	const fila = Math.floor(index / 2);

	const totalFilas = $("#tbSorteoAdm tbody tr").length;
	const nuevaFila = (fila - 1 + totalFilas) % totalFilas;

	return nuevaFila * 2 + col;
}


function aplicarMascaraEnteros() {
	$(".input-numero").inputmask(maskConfigEnterosTablas);
}

function aplicarMascaraDecimales() {
	$(".input-numero-valor").inputmask(maskConfig2Decimales);
}

function validarRangosSorteoAdm() {

	let filas = [];

	$("#tbSorteoAdm tbody tr").each(function () {

		const chk = $(this).find(".chkIncluye").is(":checked");
		if (!chk) return; // ❗ Solo validar seleccionadas

		const desdeStr = $(this).find(".so-desde").val() || "0";
		const hastaStr = $(this).find(".so-hasta").val() || "0";

		const desde = parseInt(desdeStr.replace(/\./g, "")) || 0;
		const hasta = parseInt(hastaStr.replace(/\./g, "")) || 0;

		const admId = $(this).data("adm-id");

		filas.push({ admId, desde, hasta, row: $(this) });
	});

	// Ordenar por "desde"
	filas.sort((a, b) => a.desde - b.desde);

	let error = false;

	$("#tbSorteoAdm tbody tr").removeClass("error-range");

	for (let i = 0; i < filas.length - 1; i++) {

		const actual = filas[i];
		const siguiente = filas[i + 1];

		if (actual.hasta >= siguiente.desde) {
			error = true;
			actual.row.addClass("error-range");
			siguiente.row.addClass("error-range");
		}
	}

	if (error) {
		AbrirMensaje("ATENCIÓN", "Los rangos de numeración se solapan entre sucursales.", function () {
			$("#msjModal").modal("hide");
			return true;
		}, false, ["Aceptar"], "error!", null);
	}

	return !error;
}

function inicializarEventosTablaProductos() {
}

function buildQueryFilters(pag) {
	const usaPeriodo = $("#chkDesdeHasta").is(":checked");
	const fechaD = usaPeriodo ? $("#Desde").val() : null;
	const fechaH = usaPeriodo ? $("#Hasta").val() : null;

	return {
		Registros: 200,
		Pagina: pag,
		FechaD: fechaD || null,
		FechaH: fechaH || null,
	};
}

function InicializaPantallaPedido() {
	// INICIALIZAMOS PANELES
	if ($("#divDetalle").is(":visible")) {
		$("#divDetalle").collapse("hide");
	}
	$("#divFiltro").collapse("show");
	// ✅ Activar botón de nuevo pedido
	$("#btnAbmNuevo").prop("disabled", false);

	// Configurar el evento click para el botón Cancelar/Inicializar
	$("#btnAbmCancelar").on("click", function (e) {
		cancelarOperacion(e);
	});

	$("#btnCancel").on("click", function () {
		window.location.href = homePedido;
	});

	$("#btnAbmAceptar, #btnAbmCancelar, #btnImprimir").prop("disabled", true).hide();

	// Delegación: captura Enter en cualquiera de los inputs date del filtro
	$(document).on("keydown", "#divFiltro input[type='date']", function (e) {
		if (e.key === "Enter") {
			e.preventDefault(); // evita submit o comportamientos raros
			$("#btnBuscar").trigger("click");
		}
	});

	agregarHandlerCheckTodosLosProveedores();

	// Inicializa el período de fechas (hoy / hoy + 30 días)
	initPeriodoFechas();

	// Etiquetas de filtros
	$("#lbChkDesdeHasta").text("Periodo");
	$("#chkDesdeHasta")
		.prop("checked", true)
		.prop("disabled", true);

	$("#Desde").prop("disabled", false);
	$("#Hasta").prop("disabled", false);
}

function agregarHandlerCheckTodosLosProveedores() {
	$(document).on("change", "input[name='todos_los_prod_del_prov']", function () {

		const chk = $(this).is(":checked");

		if (chk) {
			$("#Rel01B").prop("disabled", false);
			$("#cta_denominacion").prop("disabled", true);
			$("#btnAgregarCProducto").prop("disabled", true);

		} else {
			$("#cta_denominacion").prop("disabled", false);
			$("#Rel01B").prop("disabled", true);
			$("#btnAgregarCProducto").prop("disabled", false);
		}
	});
}



function initPeriodoFechas() {
	// Último lunes pasado
	const desde = obtenerPrimerDiaMesAnterior();

	// Hoy
	const hasta = new Date();

	// Formatear YYYY-MM-DD
	const fmt = d => d.toISOString().split("T")[0];

	$("#Desde").val(fmt(desde));
	$("#Hasta").val(fmt(hasta));

	// Siempre habilitadas
	$("#Desde").prop("disabled", false);
	$("#Hasta").prop("disabled", false);

	// Checkbox siempre marcado y deshabilitado
	$("#chkDesdeHasta")
		.prop("checked", true)
		.prop("disabled", true);
}

function obtenerPrimerDiaMesAnterior() {
	const hoy = new Date();
	const year = hoy.getFullYear();
	const month = hoy.getMonth(); // 0=enero ... 11=diciembre

	// Primer día del mes anterior
	return new Date(year, month - 1, 1);
}

function obtenerUltimoLunes() {
	const hoy = new Date();
	const diaSemana = hoy.getDay(); // 0=Domingo ... 1=Lunes

	// Si hoy es lunes → retroceder 7 días
	const diferencia = diaSemana === 1 ? 7 : (diaSemana + 6) % 7;

	const ultimoLunes = new Date(hoy);
	ultimoLunes.setDate(hoy.getDate() - diferencia);

	return ultimoLunes;
}

const maskConfigEnterosTablas = {
	alias: "numeric",
	groupSeparator: ".",
	autoGroup: true,
	digits: 0,
	digitsOptional: false,
	rightAlign: true,
	prefix: '',
	placeholder: "",
	clearMaskOnLostFocus: false,
	showMaskOnHover: false,
	showMaskOnFocus: true,
	allowMinus: false
};

const maskConfigEnteros = {
	alias: "numeric",
	groupSeparator: ".",
	autoGroup: true,
	digits: 0,
	digitsOptional: false,
	rightAlign: true,
	prefix: '',
	placeholder: "0",
	clearMaskOnLostFocus: false,
	showMaskOnHover: false,
	showMaskOnFocus: false,
	allowMinus: false,
	onBeforeMask: function (value) {
		if (value) {
			let numValue = parseInt(value.toString().replace(/\./g, ''));
			return isNaN(numValue) ? value : numValue.toString();
		}
		return value;
	}
};

const maskConfig2Decimales = {
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

function desactivarTablaSorteos() {
	$("#tbGridSorteo").addClass("tabla-desactivada");
	$("#tbGridSorteo tbody tr").addClass("disabled-row");
}

function activarTablaSorteos() {
	$("#tbGridSorteo").removeClass("tabla-desactivada");
	$("#tbGridSorteo tbody tr").removeClass("disabled-row");
}

function crearGridProdVacioHtml() {
	return `
    <style>
		.fixed-width-card {
			padding-left: 0px;
			padding-right: 0px;
		}
	</style>
	<div id="divCardSorteoProd" class="card mb-2 fixed-width-card">
		<div class="card-header py-1">
			<h6 class="mb-0">Productos</h6>
			<button type="button" class="btn btn-sm btn-outline-primary" id="btnAgregarCProducto" title="Agregar Producto" disabled>
				<i class="bx bx-plus"></i>
			</button>
		</div>
		<div class="card-body p-1">
			<div class="row g-2">
				<div class="table-responsive table-wrapper-300">
					<table class="table table-sm table-hover mb-0 table-golden" id="tbSorteoProd">
						<thead class="sticky-top table-golden-header-compact">
							<tr class="header">
								<th class="text-center">Código</th>
								<th class="text-center">Descripción</th>
							</tr>
						</thead>
						<tbody>
							@if (Model == null || Model.ListaDatos == null || !Model.ListaDatos.Any())
							{
								<tr class="fila-vacia">
									<td colspan="9" class="text-center text-muted py-2">
										<i class="bx bx-info-circle me-1"></i>
										No hay items para mostrar.
									</td>
								</tr>
							}
							else
							{
								foreach (var item in Model.ListaDatos)
								{
									if (alt) { clase = "alt"; alt = false; } else { clase = string.Empty; alt = true; }
									<tr class="@clase row-sorteo-prod"
										data-so-sorteo="@item.so_sorteo"
										data-p-id="@item.p_id">
										<td class="text-center">@item.p_id</td>
										<td class="text-start">@item.p_desc</td>
									</tr>
								}
							}
						</tbody>
					</table>
				</div>
			</div>
		</div>
	</div>`;
}

function capturarEstadoFormularioSorteo() {
	const estado = {};
	$('#divSorteoDatos').find('input, textarea, select').each(function () {
		const $campo = $(this);
		const nombre = $campo.attr('name') || $campo.attr('id');
		if (nombre) {
			estado[nombre] = $campo.val();
		}
	});
	return estado;
}

function habilitarCamposFormularioSorteo(habilitar) {
	habilitarDescripcion(habilitar);
	habilitarFechas(habilitar);
	habilitarTodosProd(habilitar);
	habilitarValor(habilitar);
	habilitarListas(habilitar);
	habilitarDetalleProductos(habilitar);
}

function habilitarDescripcion(habilitar) {
	$("#so_desc").prop("readonly", !habilitar);
}

function habilitarFechas(habilitar) {
	$("#so_desde").prop("readonly", !habilitar);
	$("#so_hasta").prop("readonly", !habilitar);
}

function habilitarTodosProd(habilitar) {
	$("#todos_los_prod_del_prov").prop("disabled", !habilitar);
}

function habilitarValor(habilitar) {
	$("#so_inclusion_valor").prop("readonly", !habilitar);
}

function habilitarListas(habilitar) {
	$("#tipo_valor").prop("disabled", !habilitar);
	$("#acumula_valor").prop("disabled", !habilitar);
}

function habilitarDetalleProductos(habilitar) {
	// Botón eliminar producto
	$(".btn-eliminar-producto").toggle(habilitar);
}

function estaEnModoEdicionSorteo() {
	return !!(modoNuevoSorteo || modoModificacionSorteo);
}

/**
* ✅ OPTIMIZADO: Actualiza visibilidad de botones de eliminación
* Llamar al cambiar modo edición
*/
function aplicarVisibilidadBotonesEliminar() {
	const enEdicion = estaEnModoEdicionSorteo();

	$('.btn-eliminar-producto').each(function () {
		$(this).toggle(enEdicion);
	});
}

// ============================================================================
// INTEGRACIÓN CON BÚSQUEDA AVANZADA V02
// ============================================================================

function cargarModalBusquedaAvanzada(callback) {
	if ($("#busquedaModal").length > 0) {
		if (typeof callback === 'function') callback();
		return;
	}

	const urlModal = typeof busquedaAvanzadaModalUrl !== 'undefined'
		? busquedaAvanzadaModalUrl
		: '/ControlComun/Producto/BusquedaAdvanceV02';

	$.ajax({
		url: urlModal,
		type: 'GET',
		success: function (html) {
			if ($("#busquedaModal").length === 0) {
				$('body').append(html);
			}
			if (typeof callback === 'function') {
				callback();
			}
		},
		error: function (xhr, status, error) {
			console.error("Error al cargar modal de búsqueda:", error);
			ControlaMensajeError("No se pudo cargar el módulo de búsqueda de productos");
		}
	});
}

function agregarProductosAlGrid(productos) {
	if (!Array.isArray(productos) || productos.length === 0) return;

	const $tbody = $('#tbSorteoProd tbody');

	const $filaVacia = $tbody.find('tr td[colspan]');
	if ($filaVacia.length > 0) {
		$filaVacia.closest('tr').remove();
	}

	//let $tfoot = $('#tbSorteoProd tfoot');
	//if ($tfoot.length === 0) {
	//	$('#tbSorteoProd').append(`
 //           <tfoot class="table-golden-footer">
 //               <tr>
 //                   <td colspan="7" class="text-end fw-bold">Total General:</td>
 //                   <td class="text-end fw-bold">0.00</td>
 //               </tr>
 //           </tfoot>
 //       `);
	//	$tfoot = $('#tbGridPedidoProds tfoot');
	//}

	let esAlternado = $tbody.find('tr').length % 2 !== 0;

	productos.forEach(function (producto, index) {
		const fila = crearFilaProductoSorteo(producto, esAlternado);
		$tbody.append(fila);
		esAlternado = !esAlternado;
	});

	//aplicarInputMaskPresupuesto();
	//aplicarReadonlyCamposSorteo();
	//actualizarTotalGeneralPedido();
	configurarEventosEliminacionProducto();
	setTimeout(() => {
		//finalizarInicializacion();
		//calcularUtilidadMargen();
		// Reinicializar drag & drop con las nuevas filas
		//inicializarDragAndDropProductos();
	}, 100);
}

/**
 * ✅ NUEVO: Escapa HTML para prevenir XSS
 * @param {string} texto - Texto a escapar
 * @returns {string} Texto escapado
 */
function escaparHTML(texto) {
	const div = document.createElement('div');
	div.textContent = texto;
	return div.innerHTML;
}


/**
* ✅ OPTIMIZADO: Crea HTML de fila de producto con TODOS los nuevos campos
* Unifica lógica de cálculo y evita duplicación de código
* @param {object} producto - ProductoListaDto
* @param {boolean} esAlternado - Alternar clase CSS
* @returns {string} HTML de la fila
*/
function crearFilaProductoSorteo(producto, esAlternado) {
	// ✅ VALIDACIÓN Y NORMALIZACIÓN DE DATOS
	const datosProducto = normalizarDatosProducto(producto);

	// ✅ FORMATEO
	const claseAlt = esAlternado ? 'alt' : '';

	// ✅ CONSTRUCCIÓN HTML CON TEMPLATE LITERALS (más legible y performante)
	return `
        <tr class="${claseAlt} row-sorteo-prod"
            data-so-sorteo="0"
            data-p-id="${datosProducto.p_id}"
			data-p-desc="${datosProducto.p_desc}">

			<td class="text-center">${datosProducto.p_id}</td>
            <td class="text-start">${escaparHTML(datosProducto.p_desc) }</td>

            <td class="text-center">
                <button type="button"
                        class="btn btn-sm btn-danger btn-eliminar-producto"
                        data-p-id="${datosProducto.p_id}"
                        title="Eliminar producto"
                        style="${estaEnModoEdicionSorteo() ? '' : 'display: none;'}">
                    <i class="bx bx-trash"></i>
                </button>
            </td>
        </tr>
    `;
}

/**
* ✅ NUEVO: Normaliza y valida datos del ProductoListaDto
* Centraliza validación y conversión de tipos
* @param {object} producto - ProductoListaDto
* @returns {object} Datos normalizados y validados
*/
function normalizarDatosProducto(producto) {
	return {
		// Identificadores
		p_id: String(producto.p_id || producto.P_id || '').trim(),
		p_desc: String(producto.p_desc || producto.P_desc || 'Sin descripción').trim(),
	};
}