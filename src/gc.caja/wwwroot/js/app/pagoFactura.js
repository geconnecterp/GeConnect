// ════════════════════════════════════════════════════════════
// GESTOR DE PAGO DE FACTURA
// ════════════════════════════════════════════════════════════
// VERSIÓN v18.1 - Correcciones de limpieza y validación
// ════════════════════════════════════════════════════════════
// CAMBIOS v18.1:
// - Agregado evento de limpieza automática en modal de efectivo
// - Mejorada función cerrarModalDetalleEfectivo() con limpieza exhaustiva
// - Constante LIMITE_PORCENTAJE_DIFERENCIA para validación configurable
// - Mensaje de validación mejorado con tabla comparativa
// - Logs más descriptivos para debugging
//
// VERSIÓN ANTERIOR v18.0 - Integración de InputMask
// VERSIÓN ANTERIOR v16.1 - Mejora de rendimiento
// ════════════════════════════════════════════════════════════
// Autor: GeConnect ERP
// Última actualización: 2026-05-25
// ════════════════════════════════════════════════════════════

// ═══════════════════════════════════════════════════════════════════
// SECCIÓN 1: VARIABLES GLOBALES Y CONSTANTES
// ═══════════════════════════════════════════════════════════════════

let modalPagoInstance = null;
let modalTipoMedioPagoInstance = null;
let datosCliente = {};
let conceptosPago = {
    totalPagar: 0,
    recargos: 0,
    descuentos: 0,
    totalValores: 0,
    diferencia: 0
};
let valoresPago = [];
let valorIdCounter = 0;
let tipoMedioPagoSeleccionado = null;
let valoresMPCache = null;
let valoresMPCargados = false;

// ═══════════════════════════════════════════════════════════════════
// ✅ NUEVO v18.1: CONSTANTES DE CONFIGURACIÓN
// ═══════════════════════════════════════════════════════════════════

/**
 * Porcentaje máximo permitido sobre la diferencia pendiente
 * Un valor de 1.5 significa que se permite hasta 150% de la diferencia
 * 
 * Ejemplo:
 * - Diferencia pendiente: $ 1.000,00
 * - Monto máximo permitido: $ 1.500,00 (1.000 * 1.5)
 * 
 * Regla de negocio:
 * Permite flexibilidad para casos de vuelto o sobrepago intencional,
 * pero alerta al cajero si el monto es desproporcionado.
 */
const LIMITE_PORCENTAJE_DIFERENCIA = 1.5;

/**
 * Timeout para operaciones de animación de modal (milisegundos)
 */
const MODAL_ANIMATION_TIMEOUT = 300;

/**
 * Timeout para focus de inputs (milisegundos)
 */
const INPUT_FOCUS_TIMEOUT = 300;

// ═══════════════════════════════════════════════════════════════════
// SECCIÓN 2: INICIALIZACIÓN
// ═══════════════════════════════════════════════════════════════════

$(function () {
    console.log('🚀 Módulo de Pago de Factura inicializado v16.1');
    inicializarModales();
    inicializarEventosPago();
    console.log('✅ Módulo de Pago listo');
});

/**
 * Inicializa las instancias de los modales de Bootstrap
 */
function inicializarModales() {
    console.log('🔧 Inicializando modales...');

    // ❶ Modal principal de pago
    const modalPagoElement = document.querySelector('#modalPago');
    if (modalPagoElement) {
        modalPagoInstance = new bootstrap.Modal(modalPagoElement, {
            backdrop: 'static',
            keyboard: false
        });
        console.log('✅ Modal de pago inicializado');
    } else {
        console.error('❌ Modal #modalPago no encontrado');
    }

    // ❷ Modal de tipo medio de pago (lazy loading)
    setTimeout(() => {
        const modalTipoMPElement = document.querySelector('#modalTipoMedioPago');
        if (modalTipoMPElement) {
            modalTipoMedioPagoInstance = bootstrap.Modal.getInstance(modalTipoMPElement) ||
                new bootstrap.Modal(modalTipoMPElement, {
                    backdrop: 'static',
                    keyboard: false
                });
            console.log('✅ Modal de tipo medio de pago inicializado');
        } else {
            console.warn('⚠️ Modal #modalTipoMedioPago no encontrado (se inicializará dinámicamente)');
        }
    }, 500);
}

/**
 * ✅ ACTUALIZADO v18.1: Vincula todos los eventos del módulo de pago
 * NUEVO: Evento de limpieza automática del modal de efectivo
 */
function inicializarEventosPago() {
    console.log('🔧 Vinculando eventos de pago...');

    // ❶ Botones del modal principal
    $('#btnAgregarPago').off('click').on('click', agregarFormaPago);
    $('#btnVolverPago').off('click').on('click', volverACalculoFactura);
    $('#btnFinalizarPago').off('click').on('click', finalizarPago);

    // ❷ Eventos del modal principal
    $('#modalPago').off('hidden.bs.modal').on('hidden.bs.modal', limpiarModalPago);
    $('#modalPago').off('shown.bs.modal').on('shown.bs.modal', function () {
        setTimeout(() => $('#btnAgregarPago').trigger('focus'), 300);
    });

    // ❸ ✅ NUEVO v18.1: Evento de limpieza del modal de efectivo
    $('#modalDetalleEfectivo').off('hidden.bs.modal').on('hidden.bs.modal', function () {
        console.log('═══════════════════════════════════════════════════');
        console.log('🧹 LIMPIEZA AUTOMÁTICA - MODAL EFECTIVO');
        console.log('═══════════════════════════════════════════════════');

        // ❶ Limpiar valor del input
        const $input = $('#txtMontoEfectivo');
        const valorAnterior = $input.val();

        $input.val('');
        console.log(`   📝 Input limpiado (valor anterior: "${valorAnterior}")`);

        // ❷ Remover clases de validación
        $input.removeClass('is-invalid is-valid');
        console.log('   ✅ Clases de validación removidas');

        // ❸ Remover mensajes de error
        $input.siblings('.invalid-feedback').remove();
        $('.invalid-feedback').remove(); // Remover cualquier feedback suelto
        console.log('   ✅ Mensajes de error removidos');

        // ❹ Remover backdrop específico del modal de efectivo (si quedó huérfano)
        const $backdropEfectivo = $('.modal-backdrop[data-modal="efectivo"]');
        if ($backdropEfectivo.length > 0) {
            $backdropEfectivo.remove();
            console.log('   ✅ Backdrop huérfano removido');
        }

        // ❺ Resetear labels informativos
        $('#lblTipoMedioPagoEfectivo').text('-');
        $('#lblInstrumentoEfectivo').text('-');
        console.log('   ✅ Labels informativos reseteados');

        console.log('═══════════════════════════════════════════════════');
        console.log('✅ MODAL DE EFECTIVO LIMPIADO COMPLETAMENTE');
        console.log('═══════════════════════════════════════════════════');
    });

    console.log('✅ Eventos de pago configurados');
}

// ═══════════════════════════════════════════════════════════════════
// SECCIÓN 3: FUNCIONES PRINCIPALES DE VISTA
// ═══════════════════════════════════════════════════════════════════

/**
 * Abre el modal de pago con los datos de la factura
 * @param {Object} datosFactura - Objeto con totales y datos del cliente
 */
function abrirModalPago(datosFactura) {
    console.log('═══════════════════════════════════════════════════');
    console.log('🔓 ABRIR MODAL DE PAGO v16.1');
    console.log('═══════════════════════════════════════════════════');
    console.log('Datos recibidos:', datosFactura);

    // ❶ Validar que el modal esté inicializado
    if (!modalPagoInstance) {
        console.error('❌ Modal de pago no inicializado');
        mostrarMensajeError('El módulo de pago no está disponible. Por favor, recargue la página.');
        return false;
    }

    try {
        // ❷ Ocultar modal de cálculo (si existe)
        ocultarModalCalculoFactura();

        // ❸ Hidratar datos del cliente
        hidratarDatosClientePago();

        // ❹ Cargar conceptos de pago (totales)
        cargarConceptosPago(datosFactura?.totales || {});

        // ❺ Limpiar tabla de formas de pago
        limpiarTablaFormasPago();

        // ❻ Mostrar modal
        modalPagoInstance.show();

        // ❼ Ajustar z-index
        setTimeout(() => {
            $('#modalPago').css('z-index', '1060');
            $('.modal-backdrop').last().css('z-index', '1059');
        }, 100);

        console.log('✅ Modal de pago abierto correctamente');
        return true;

    } catch (error) {
        console.error('❌ Error al abrir modal de pago:', error);
        mostrarMensajeError(`No se pudo abrir el modal de pago.\n\n${error.message}`);
        return false;
    }
}

/**
 * ✅ NUEVO: Agrega una forma de pago
 * Abre el modal de tipo medio de pago
 */
function agregarFormaPago() {
    console.log('═══════════════════════════════════════════════════');
    console.log('➕ AGREGAR FORMA DE PAGO v16.1');
    console.log('═══════════════════════════════════════════════════');

    // ❶ Validar que no haya diferencia $0.00
    const diferencia = conceptosPago.diferencia || 0;

    if (Math.abs(diferencia) < 0.01) {
        console.warn('⚠️ La diferencia ya es $0.00');

        if (typeof toastr !== 'undefined') {
            toastr.info('La diferencia ya es $0.00. No es necesario agregar más valores.');
        } else {
            alert('La diferencia ya es $0.00');
        }

        return;
    }

    // ❷ Abrir modal de tipo medio de pago
    abrirModalTipoMedioPago();
}

/**
 * ✅ NUEVO: Abre el modal de selección de tipo de medio de pago
 * Carga los datos desde el servidor
 */
function abrirModalTipoMedioPago() {
    console.log('═══════════════════════════════════════════════════');
    console.log('🔓 ABRIR MODAL TIPO MEDIO DE PAGO v16.1');
    console.log('═══════════════════════════════════════════════════');

    // ❶ Validar/Inicializar modal si no existe
    if (!modalTipoMedioPagoInstance) {
        console.log('⚠️ Modal no inicializado - Intentando lazy loading...');

        const modalElement = document.querySelector('#modalTipoMedioPago');

        if (!modalElement) {
            console.error('❌ Elemento #modalTipoMedioPago no encontrado en el DOM');
            mostrarMensajeError('El modal de tipo de pago no está disponible.\n\nPor favor, recargue la página.');
            return;
        }

        try {
            modalTipoMedioPagoInstance = new bootstrap.Modal(modalElement, {
                backdrop: 'static',
                keyboard: false
            });
            console.log('✅ Modal inicializado dinámicamente');
        } catch (error) {
            console.error('❌ Error al crear modal:', error);
            mostrarMensajeError(`Error al inicializar el modal: ${error.message}`);
            return;
        }
    }

    // ❷ Resetear selección
    resetearSeleccionTipoMedioPago();

    // ❸ Mostrar modal
    modalTipoMedioPagoInstance.show();

    // ❹ Bloquear modal mientras carga
    bloquearModalTipoMedioPago('Cargando opciones de pago...');

    // ❺ Cargar datos desde el servidor
    cargarValoresMP()
        .then(function (valoresMP) {
            console.log('✅ Valores MP obtenidos:', valoresMP);

            // ❻ Renderizar opciones en el modal
            renderizarOpcionesMP(valoresMP);

            // ❼ Pre-seleccionar primera opción
            const $primerItem = $('.tipo-medio-pago-item').first();
            if ($primerItem.length > 0) {
                seleccionarItemTipoMedioPago($primerItem);
            }

            // ❽ Vincular eventos
            vincularEventosTipoMedioPago();

            console.log('✅ Modal de tipo medio de pago listo');
        })
        .catch(function (error) {
            console.error('❌ Error al cargar valores MP:', error);

            // Mostrar mensaje de error en el modal
            $('#listaTiposMedioPago').html(`
                <div class="text-center py-5 text-danger">
                    <i class="bx bx-error-circle bx-lg mb-3"></i>
                    <p class="mb-2">Error al cargar las opciones de pago</p>
                    <small class="text-muted">${error.message || 'Error desconocido'}</small>
                    <div class="mt-3">
                        <button class="btn btn-sm btn-outline-primary" onclick="location.reload()">
                            <i class="bx bx-refresh"></i> Recargar Página
                        </button>
                    </div>
                </div>
            `);
        })
        .always(function () {
            desbloquearModalTipoMedioPago();
        });
}

/**
 * ✅ ACTUALIZADO: Carga los tipos de medio de pago desde el servidor
 * CAMBIO v16.1: NO depende de variables globales de sesión
 * El servidor maneja automáticamente los datos de sesión
 * 
 * @returns {Promise<Array>} - Array de valores MP
 */
function cargarValoresMP() {
    console.log('═══════════════════════════════════════════════════');
    console.log('📡 CARGAR VALORES MP v16.1 (SIN DATOS DE SESIÓN EN FRONTEND)');
    console.log('═══════════════════════════════════════════════════');

    // ❶ Verificar cache
    if (valoresMPCargados && valoresMPCache !== null) {
        console.log('✅ Valores MP encontrados en cache');
        return $.Deferred().resolve(valoresMPCache).promise();
    }

    // ❷ Obtener datos del cliente
    const ctaId = $('#txtClienteIdPago').val() || '';
    const coTipo = ctaId && ctaId !== 'N/A' && ctaId.trim() !== '' ? 'CR' : 'CF';

    console.log('📋 Datos de la consulta:');
    console.log(`   cta_id: ${ctaId || 'N/A'}`);
    console.log(`   co_tipo: ${coTipo} (${coTipo === 'CF' ? 'Consumidor Final' : 'Cliente Registrado'})`);

    // ❸ ✅ CAMBIO CRÍTICO: NO enviar adm_id desde el frontend
    // El servidor lo obtendrá automáticamente desde la sesión
    const requestData = {
        co_tipo: coTipo,
        cta_id: ctaId
        // ❌ NO INCLUIR: adm_id (el servidor lo obtiene de la sesión)
    };

    console.log('   ✅ adm_id: Gestionado automáticamente por el servidor');

    // ❹ Llamada AJAX
    return $.ajax({
        url: typeof obtenerValoresMPUrl !== 'undefined' && obtenerValoresMPUrl
            ? obtenerValoresMPUrl
            : '/Facturacion/checkout/ObtenerValoresMP',
        type: 'POST',
        contentType: 'application/json',
        data: JSON.stringify(requestData),
        dataType: 'json',
        timeout: 10000
    })
        .then(function (response) {
            console.log('✅ Respuesta recibida:', response);

            if (!response || !response.ok) {
                console.warn('⚠️ Respuesta no exitosa');
                valoresMPCache = [];
                valoresMPCargados = true;
                return [];
            }

            const datos = response.datos || response.data || [];

            if (!Array.isArray(datos)) {
                console.warn('⚠️ Datos no son un array');
                valoresMPCache = [];
                valoresMPCargados = true;
                return [];
            }

            console.log(`✅ ${datos.length} tipos de medio de pago recipidos`);

            // Guardar en cache
            valoresMPCache = datos;
            valoresMPCargados = true;

            return datos;
        })
        .fail(function (jqXHR, textStatus, errorThrown) {
            console.error('❌ ERROR AL CARGAR VALORES MP');
            console.error('   Status:', textStatus);
            console.error('   Error:', errorThrown);

            valoresMPCache = [];
            valoresMPCargados = true;

            return $.Deferred().reject(new Error(`Error de comunicación: ${textStatus}`)).promise();
        });
}

/**
 * ✅ NUEVO: Renderiza las opciones de tipo de medio de pago en el modal
 * @param {Array} valoresMP - Array de objetos { tcf_id, tcf_desc }
 */
function renderizarOpcionesMP(valoresMP) {
    console.log('═══════════════════════════════════════════════════');
    console.log('🎨 RENDERIZAR OPCIONES MP v16.1');
    console.log(`   Total opciones: ${valoresMP?.length || 0}`);
    console.log('═══════════════════════════════════════════════════');

    const $lista = $('#listaTiposMedioPago');
    $lista.empty();

    // ❶ Validar que haya opciones
    if (!valoresMP || valoresMP.length === 0) {
        console.warn('⚠️ No hay opciones para renderizar');

        $lista.html(`
            <div class="text-center py-5 text-muted">
                <i class="bx bx-info-circle bx-lg mb-3"></i>
                <p class="mb-0">No hay medios de pago disponibles para este cliente.</p>
                <small class="text-muted">Contacte al administrador si esto es incorrecto.</small>
            </div>
        `);
        return;
    }

    // ❷ Renderizar cada opción
    valoresMP.forEach(function (valor, index) {
        const tcfId = valor.tcf_id || valor.id || '';
        const tcfDesc = valor.tcf_desc || valor.descripcion || 'Sin descripción';
        const icono = obtenerIconoMP(tcfId);

        const itemHtml = `
            <div class="list-group-item list-group-item-action tipo-medio-pago-item" 
                 data-tcf-id="${escapeHtml(tcfId)}" 
                 data-tcf-desc="${escapeHtml(tcfDesc)}"
                 data-index="${index}"
                 style="cursor: pointer; transition: all 0.2s ease;">
                <i class="${icono} me-2 text-muted"></i>
                <span class="tipo-medio-pago-desc">${escapeHtml(tcfDesc)}</span>
            </div>
        `;

        $lista.append(itemHtml);

        console.log(`   ✅ [${index + 1}] ${tcfDesc} (${tcfId})`);
    });

    console.log('✅ Opciones renderizadas correctamente');
}

/**
 * Vuelve al modal de cálculo de factura
 */
function volverACalculoFactura() {
    console.log('🔙 Volviendo al modal de cálculo...');

    if (modalPagoInstance) {
        modalPagoInstance.hide();
    }

    setTimeout(() => {
        $('#modalCalculoFactura').modal('show');
    }, 300);
}

/**
 * Finaliza el pago (por implementar)
 */
function finalizarPago() {
    console.log('✅ FINALIZAR PAGO (por implementar)');

    if (typeof toastr !== 'undefined') {
        toastr.info('Funcionalidad en desarrollo: Finalizar pago');
    } else {
        alert('Funcionalidad en desarrollo');
    }
}

// ═══════════════════════════════════════════════════════════════════
// SECCIÓN 4: FUNCIONES AUXILIARES
// ═══════════════════════════════════════════════════════════════════

/**
 * Hidratar datos del cliente en el modal de pago
 */
function hidratarDatosClientePago() {
    console.log('📝 Hidratando datos del cliente...');

    const mapeoIds = {
        'txtClienteNombreCalc': 'txtClienteNombrePago',
        'txtClienteIdCalc': 'txtClienteIdPago',
        'txtClienteDomicilioCalc': 'txtClienteDomicilioPago',
        'txtCondicionAfipCalc': 'txtCondicionAfipPago',
        'txtClienteCuitCalc': 'txtClienteCuitPago',
        'txtClienteEmailCalc': 'txtClienteEmailPago',
        'txtClienteMovilCalc': 'txtClienteMovilPago'
    };

    Object.keys(mapeoIds).forEach(function (idOrigen) {
        const idDestino = mapeoIds[idOrigen];
        const valor = $(`#${idOrigen}`).val() || '';
        $(`#${idDestino}`).val(valor);
    });

    const badgeHtml = $('#badgeTipoComprobanteCalc').html();
    $('#badgeTipoComprobantePago').html(badgeHtml);

    console.log('✅ Datos hidratados');
}

/**
 * Cargar conceptos de pago (totales)
 */
function cargarConceptosPago(totales) {
    console.log('💰 Cargando conceptos de pago...');

    const totalPagar = parseFloat(totales.totalPagar) || 0;
    const recargos = parseFloat(totales.recargos) || 0;
    const descuentos = parseFloat(totales.descuentos) || 0;

    conceptosPago.totalPagar = totalPagar;
    conceptosPago.recargos = recargos;
    conceptosPago.descuentos = descuentos;
    conceptosPago.totalValores = 0;
    conceptosPago.diferencia = totalPagar + recargos - descuentos;

    $('#totalPagar').text(`$ ${formatearNumero(totalPagar, 2)}`);
    $('#totalRecargos').text(`$ ${formatearNumero(recargos, 2)}`);
    $('#totalDescuentos').text(`$ ${formatearNumero(descuentos, 2)}`);
    $('#totalValores').text(`$ 0.00`);
    $('#diferencia').text(`$ ${formatearNumero(conceptosPago.diferencia, 2)}`);

    console.log('✅ Conceptos cargados');
}

/**
 * Limpiar tabla de formas de pago
 */
function limpiarTablaFormasPago() {
    const $tbody = $('#tbodyFormasPago');
    $tbody.html(`
        <tr id="rowSinFormasPago">
            <td colspan="5" class="text-center py-5">
                <i class='bx bx-info-circle bx-lg text-muted'></i>
                <p class="text-muted mb-0 mt-3 fs-5">
                    No hay formas de pago registradas.<br>
                    <small>Presione <strong>AGREGAR</strong> para comenzar</small>
                </p>
            </td>
        </tr>
    `);

    valoresPago = [];
    $('#badgeCantidadPagos').text('0 valores');

    console.log('✅ Tabla limpiada');
}

/**
 * Ocultar modal de cálculo de factura
 */
function ocultarModalCalculoFactura() {
    try {
        $('#modalCalculoFactura').modal('hide');
        console.log('✅ Modal de cálculo ocultado');
    } catch (error) {
        console.warn('⚠️ Error al ocultar modal de cálculo:', error);
    }
}

/**
 * Limpiar modal de pago al cerrarse
 */
function limpiarModalPago() {
    console.log('🧹 Limpiando modal de pago...');

    datosCliente = {};
    conceptosPago = {
        totalPagar: 0,
        recargos: 0,
        descuentos: 0,
        totalValores: 0,
        diferencia: 0
    };
    valoresPago = [];
    valoresMPCache = null;
    valoresMPCargados = false;

    limpiarTablaFormasPago();

    console.log('✅ Modal limpiado');
}

/**
 * Bloquear modal de tipo medio de pago
 */
function bloquearModalTipoMedioPago(mensaje) {
    if ($('#overlayTipoMedioPago').length === 0) {
        $('#modalTipoMedioPago .modal-content').append(`
            <div id="overlayTipoMedioPago" style="
                position: absolute;
                top: 0;
                left: 0;
                width: 100%;
                height: 100%;
                background: rgba(255, 255, 255, 0.9);
                display: flex;
                align-items: center;
                justify-content: center;
                z-index: 9999;
            ">
                <div class="text-center">
                    <div class="spinner-border text-golden mb-3" style="width: 2.5rem; height: 2.5rem;"></div>
                    <p class="fw-bold text-golden-dark" id="mensajeOverlayTipoMP">${mensaje}</p>
                </div>
            </div>
        `);
    } else {
        $('#mensajeOverlayTipoMP').text(mensaje);
        $('#overlayTipoMedioPago').fadeIn(200);
    }

    $('#btnConfirmarTipoMedioPago').prop('disabled', true);
}

/**
 * Desbloquear modal de tipo medio de pago
 */
function desbloquearModalTipoMedioPago() {
    $('#overlayTipoMedioPago').fadeOut(300, function () {
        $(this).remove();
    });
}

/**
 * Resetear selección de tipo medio de pago
 */
function resetearSeleccionTipoMedioPago() {
    $('.tipo-medio-pago-item').removeClass('selected');
    tipoMedioPagoSeleccionado = null;
    $('#btnConfirmarTipoMedioPago').prop('disabled', true);
}

/**
 * Seleccionar ítem de tipo medio de pago
 */
function seleccionarItemTipoMedioPago($item) {
    $('.tipo-medio-pago-item').removeClass('selected');
    $item.addClass('selected');

    tipoMedioPagoSeleccionado = {
        tcf_id: $item.data('tcf-id'),
        tcf_desc: $item.data('tcf-desc')
    };

    $('#btnConfirmarTipoMedioPago').prop('disabled', false);

    console.log('✅ Tipo seleccionado:', tipoMedioPagoSeleccionado);
}

/**
 * Vincular eventos del modal de tipo medio de pago
 */
function vincularEventosTipoMedioPago() {
    $('.tipo-medio-pago-item').off('click').on('click', function () {
        seleccionarItemTipoMedioPago($(this));
    });

    $('.tipo-medio-pago-item').off('dblclick').on('dblclick', function () {
        seleccionarItemTipoMedioPago($(this));
        setTimeout(() => confirmarSeleccionTipoMedioPago(), 300);
    });

    $('#btnConfirmarTipoMedioPago').off('click').on('click', confirmarSeleccionTipoMedioPago);
}

/**
 * ✅ ACTUALIZADO v17.1: Confirmar selección de tipo medio de pago
 * Cierra el modal y dispara la carga de instrumentos
 */
function confirmarSeleccionTipoMedioPago() {
    console.log('═══════════════════════════════════════════════════');
    console.log('✅ CONFIRMAR TIPO MEDIO DE PAGO v17.1');
    console.log('═══════════════════════════════════════════════════');

    // ❶ Validar que haya selección
    if (!tipoMedioPagoSeleccionado) {
        console.error('❌ No hay tipo de medio de pago seleccionado');

        if (typeof toastr !== 'undefined') {
            toastr.warning('Debe seleccionar un tipo de medio de pago');
        }

        return;
    }

    console.log('📋 Tipo seleccionado:', tipoMedioPagoSeleccionado);

    // ❷ Cerrar modal de tipo medio de pago
    if (modalTipoMedioPagoInstance) {
        modalTipoMedioPagoInstance.hide();
    }

    // ❸ Esperar a que se cierre completamente el modal
    setTimeout(() => {
        // ❹ Cargar instrumentos disponibles para este tipo de pago
        cargarInstrumentos(tipoMedioPagoSeleccionado);
    }, 300);
}

/**
 * ✅ NUEVO v17.1: Carga los instrumentos disponibles para un tipo de medio de pago
 * @param {Object} tipoMedioPago - Objeto con tcf_id y tcf_desc
 * @returns {Promise<Array>} - Array de instrumentos
 */
function cargarInstrumentos(tipoMedioPago) {
    console.log('═══════════════════════════════════════════════════');
    console.log('📡 CARGAR INSTRUMENTOS v17.1');
    console.log(`   Tipo MP: ${tipoMedioPago.tcf_desc} (${tipoMedioPago.tcf_id})`);
    console.log('═══════════════════════════════════════════════════');

    // ❶ Obtener datos del cliente
    const ctaId = $('#txtClienteIdPago').val() || '';
    const coTipo = ctaId && ctaId !== 'N/A' && ctaId.trim() !== '' ? 'CR' : 'CF';
    const admId = $('#hdnAdmId').val() || '';

    console.log('📋 Datos de la consulta:');
    console.log(`   cta_id: ${ctaId || 'N/A'}`);
    console.log(`   co_tipo: ${coTipo}`);
    console.log(`   adm_id: ${admId || 'N/A'}`);
    console.log(`   tcf_id: ${tipoMedioPago.tcf_id}`);

    // ❷ Preparar request
    const requestData = {
        co_tipo: coTipo,
        cta_id: ctaId,
        adm_id: admId,
        tcf_id: tipoMedioPago.tcf_id
    };

    // ❸ Mostrar loading global
    mostrarLoadingGlobal('Cargando instrumentos disponibles...');

    // ❹ Llamada AJAX
    return $.ajax({
        url: typeof obtenerValoresInsUrl !== 'undefined' && obtenerValoresInsUrl
            ? obtenerValoresInsUrl
            : '/Facturacion/Checkout/ObtenerValoresIns',
        type: 'POST',
        contentType: 'application/json',
        data: JSON.stringify(requestData),
        dataType: 'json',
        timeout: 10000
    })
        .then(function (response) {
            console.log('✅ Respuesta recibida:', response);

            ocultarLoadingGlobal();

            if (!response || !response.ok) {
                console.warn('⚠️ Respuesta no exitosa');

                if (typeof toastr !== 'undefined') {
                    toastr.error(response?.mensaje || 'Error al cargar instrumentos');
                }

                return [];
            }

            const instrumentos = response.datos || response.data || [];

            if (!Array.isArray(instrumentos)) {
                console.warn('⚠️ Datos no son un array');
                return [];
            }

            console.log(`✅ ${instrumentos.length} instrumentos recibidos`);

            // ❺ Procesar instrumentos según tengan detalle o no
            procesarInstrumentos(instrumentos, tipoMedioPago);

            return instrumentos;
        })
        .fail(function (jqXHR, textStatus, errorThrown) {
            console.error('❌ ERROR AL CARGAR INSTRUMENTOS');
            console.error('   Status:', textStatus);
            console.error('   Error:', errorThrown);

            ocultarLoadingGlobal();

            if (typeof toastr !== 'undefined') {
                toastr.error(`Error de comunicación: ${textStatus}`);
            }

            return $.Deferred().reject(new Error(`Error de comunicación: ${textStatus}`)).promise();
        });
}


// ═══════════════════════════════════════════════════════════════════
// SECCIÓN 4: FUNCIONES AUXILIARES (continuación)
// ═══════════════════════════════════════════════════════════════════

/**
 * ✅ CORREGIDO v17.2: Muestra un loading global que bloquea toda la pantalla
 * CORRECCIÓN: Eliminada verificación de cssRules (causa SecurityError con CDNs externos)
 * @param {string} mensaje - Mensaje a mostrar durante la carga (opcional)
 */
function mostrarLoadingGlobal(mensaje = 'Cargando...') {
    console.log(`⏳ Mostrando loading global: "${mensaje}"`);

    // ❶ Verificar si ya existe el overlay
    if ($('#overlayLoadingGlobal').length > 0) {
        console.log('⚠️ Loading global ya existe - Actualizando mensaje');
        $('#textoLoadingGlobal').text(mensaje);
        $('#overlayLoadingGlobal').fadeIn(200);
        return;
    }

    // ❷ ✅ NUEVO: Agregar estilos de animación si no existen (sin verificar cssRules)
    if (!document.getElementById('styleLoadingGlobalAnimation')) {
        const style = document.createElement('style');
        style.id = 'styleLoadingGlobalAnimation'; // ✅ ID único para evitar duplicados
        style.textContent = `
            @keyframes fadeInScaleLoading {
                from {
                    opacity: 0;
                    transform: scale(0.9);
                }
                to {
                    opacity: 1;
                    transform: scale(1);
                }
            }
        `;
        document.head.appendChild(style);
        console.log('✅ Estilos de animación agregados');
    }

    // ❸ Crear estructura HTML del loading
    const loadingHtml = `
        <div id="overlayLoadingGlobal" style="
            position: fixed;
            top: 0;
            left: 0;
            width: 100%;
            height: 100%;
            background-color: rgba(0, 0, 0, 0.75);
            display: flex;
            align-items: center;
            justify-content: center;
            z-index: 10000;
            backdrop-filter: blur(3px);
            -webkit-backdrop-filter: blur(3px);
        ">
            <div style="
                text-align: center;
                background: white;
                padding: 3rem 4rem;
                border-radius: 12px;
                box-shadow: 0 8px 32px rgba(0, 0, 0, 0.3);
                animation: fadeInScaleLoading 0.3s ease-out;
            ">
                <!-- Spinner -->
                <div class="spinner-border text-golden mb-3" 
                     style="width: 3.5rem; height: 3.5rem; border-width: 0.4rem;"
                     role="status">
                    <span class="visually-hidden">Cargando...</span>
                </div>
                
                <!-- Mensaje -->
                <p class="fw-bold text-golden-dark fs-5 mb-0" 
                   id="textoLoadingGlobal">${escapeHtml(mensaje)}</p>
            </div>
        </div>
    `;

    // ❹ Agregar al body
    $('body').append(loadingHtml);

    console.log('✅ Loading global mostrado');
}

/**
 * ✅ NUEVO v17.1: Oculta el loading global
 * @param {number} delay - Milisegundos de retraso antes de ocultar (opcional)
 */
function ocultarLoadingGlobal(delay = 0) {
    console.log(`⏳ Ocultando loading global (delay: ${delay}ms)`);

    // ❶ Verificar si existe el overlay
    if ($('#overlayLoadingGlobal').length === 0) {
        console.warn('⚠️ Loading global no existe - No hay nada que ocultar');
        return;
    }

    // ❷ Ocultar con animación
    setTimeout(() => {
        $('#overlayLoadingGlobal').fadeOut(300, function () {
            $(this).remove();
            console.log('✅ Loading global removido del DOM');
        });
    }, delay);
}

/**
 * ✅ NUEVO v17.1: Actualiza el mensaje del loading global sin ocultarlo
 * Útil para operaciones de varios pasos
 * @param {string} nuevoMensaje - Nuevo mensaje a mostrar
 */
function actualizarMensajeLoadingGlobal(nuevoMensaje) {
    console.log(`🔄 Actualizando mensaje: "${nuevoMensaje}"`);

    const $texto = $('#textoLoadingGlobal');

    if ($texto.length === 0) {
        console.warn('⚠️ Loading global no existe');
        return;
    }

    // Animación de cambio de texto
    $texto.fadeOut(150, function () {
        $(this).text(escapeHtml(nuevoMensaje)).fadeIn(150);
    });
}

/**
 * ✅ CORREGIDO v17.3: Abre el modal de selección de instrumento
 * CORRECCIÓN: Manejo mejorado de múltiples modales de Bootstrap
 */
function abrirModalInstrumentos(instrumentos, tipoMedioPago) {
    console.log('═══════════════════════════════════════════════════');
    console.log('🔓 ABRIR MODAL INSTRUMENTOS v17.3');
    console.log(`   Tipo MP: ${tipoMedioPago.tcf_desc}`);
    console.log(`   Total instrumentos: ${instrumentos.length}`);
    console.log('═══════════════════════════════════════════════════');

    // ❶ Obtener elemento del modal
    const modalElement = document.querySelector('#modalInstrumentos');

    if (!modalElement) {
        console.error('❌ CRÍTICO: Elemento #modalInstrumentos no encontrado en el DOM');
        console.error('   El modal no está cargado en la página');

        if (typeof toastr !== 'undefined') {
            toastr.error('El modal de instrumentos no está disponible. Recargue la página.');
        }

        return;
    }

    console.log('✅ Elemento #modalInstrumentos encontrado:', modalElement);

    // ❷ Obtener o crear instancia de Bootstrap
    let modalInstrumentosInstance = bootstrap.Modal.getInstance(modalElement);

    if (!modalInstrumentosInstance) {
        console.log('⚠️ Modal Instrumentos no tiene instancia - Creando nueva...');

        try {
            modalInstrumentosInstance = new bootstrap.Modal(modalElement, {
                backdrop: 'static',
                keyboard: false
            });
            console.log('✅ Instancia de Bootstrap Modal creada:', modalInstrumentosInstance);
        } catch (error) {
            console.error('❌ ERROR al crear instancia de Bootstrap Modal:', error);

            if (typeof toastr !== 'undefined') {
                toastr.error(`Error al inicializar el modal: ${error.message}`);
            }

            return;
        }
    } else {
        console.log('✅ Instancia de Bootstrap Modal ya existe:', modalInstrumentosInstance);
    }

    // ❸ Resetear selección previa
    resetearSeleccionInstrumento();

    // ❹ Renderizar instrumentos en la lista
    renderizarInstrumentos(instrumentos);

    // ❺ Guardar contexto del tipo de medio de pago
    window._tipoMedioPagoActual = tipoMedioPago;

    // ❻ ✅ NUEVO: Verificar estado ANTES de mostrar
    console.log('🔍 Estado del modal ANTES de show():');
    console.log('   isVisible:', $(modalElement).is(':visible'));
    console.log('   hasClass show:', $(modalElement).hasClass('show'));
    console.log('   display:', $(modalElement).css('display'));
    console.log('   z-index:', $(modalElement).css('z-index'));

    // ❼ ✅ CRÍTICO: Forzar display y z-index ANTES de show()
    $(modalElement).css({
        'display': 'block',  // Forzar visibilidad
        'z-index': '5090'    // Asegurar z-index
    });

    console.log('⚙️ Llamando a modalInstrumentosInstance.show()...');

    try {
        // ❽ Mostrar modal
        modalInstrumentosInstance.show();

        console.log('✅ Método show() ejecutado sin errores');
    } catch (error) {
        console.error('❌ ERROR en modalInstrumentosInstance.show():', error);

        if (typeof toastr !== 'undefined') {
            toastr.error(`Error al mostrar el modal: ${error.message}`);
        }

        return;
    }

    // ❾ Verificar estado DESPUÉS de show()
    setTimeout(() => {
        console.log('🔍 Estado del modal DESPUÉS de show():');
        console.log('   isVisible:', $(modalElement).is(':visible'));
        console.log('   hasClass show:', $(modalElement).hasClass('show'));
        console.log('   display:', $(modalElement).css('display'));
        console.log('   z-index:', $(modalElement).css('z-index'));

        const isVisible = $(modalElement).is(':visible');
        const hasShow = $(modalElement).hasClass('show');

        if (!isVisible || !hasShow) {
            console.error('❌ CRÍTICO: Modal NO se mostró correctamente');
            console.error('   Forzando visibilidad manualmente...');

            // ❿ Fallback: Forzar apertura manual
            $(modalElement).addClass('show').css({
                'display': 'block',
                'opacity': '1',
                'z-index': '5090'
            });

            // Crear backdrop si no existe
            if ($('.modal-backdrop.show').length === 0) {
                $('body').append('<div class="modal-backdrop fade show" style="z-index: 5089;"></div>');
            }

            console.log('✅ Visibilidad forzada manualmente');
        } else {
            console.log('✅ Modal mostrado correctamente');
        }

        // Verificar z-index jerárquico
        const zIndexModal = parseInt($(modalElement).css('z-index'));
        const zIndexPadre = parseInt($('#modalPago').css('z-index'));

        console.log('🔍 Verificación de z-index:');
        console.log(`   Modal Pago: ${zIndexPadre}`);
        console.log(`   Modal Instrumentos: ${zIndexModal}`);

        if (zIndexModal <= zIndexPadre) {
            console.error('❌ ADVERTENCIA: Z-index incorrecto');
            console.error(`   Esperado: ${zIndexModal} > ${zIndexPadre}`);

            // Forzar z-index correcto
            $(modalElement).css('z-index', '5090');
            console.log('✅ Z-index corregido a 5090');
        } else {
            console.log('✅ Jerarquía de z-index correcta');
        }
    }, 300);

    console.log('✅ Función abrirModalInstrumentos finalizada');
}

/**
 * ✅ NUEVO v17.2: Renderiza la lista de instrumentos en el modal
 * @param {Array} instrumentos - Array de objetos con datos de instrumentos
 */
function renderizarInstrumentos(instrumentos) {
    console.log('═══════════════════════════════════════════════════');
    console.log('🎨 RENDERIZAR INSTRUMENTOS v17.2');
    console.log(`   Total: ${instrumentos.length}`);
    console.log('═══════════════════════════════════════════════════');

    const $lista = $('#listaInstrumentos');
    $lista.empty();

    // ❶ Validar que haya instrumentos
    if (!instrumentos || instrumentos.length === 0) {
        console.warn('⚠️ No hay instrumentos para renderizar');

        $lista.html(`
            <div class="text-center py-5 text-muted">
                <i class="bx bx-info-circle bx-lg mb-3"></i>
                <p class="mb-0">No hay instrumentos disponibles.</p>
                <small class="text-muted">Contacte al administrador.</small>
            </div>
        `);
        return;
    }

    // ❷ Renderizar cada instrumento
    instrumentos.forEach(function (inst, index) {
        const insId = inst.ins_id || inst.id || '';
        const insDesc = inst.ins_desc || inst.descripcion || 'Sin descripción';
        const insSimbolo = inst.ins_simbolo || inst.simbolo || '$';
        const total = inst.total_actual || 0;
        const tieneDetalle = inst.tiene_detalle || false;

        // Determinar icono y color según el instrumento
        let icono = 'bx bx-dollar';
        let colorClase = 'text-success';

        if (insId.toUpperCase() === 'USD') {
            icono = 'bx bx-dollar-circle';
            colorClase = 'text-primary';
        } else if (insId.toUpperCase() === 'EUR') {
            icono = 'bx bx-euro';
            colorClase = 'text-purple';
        }

        const itemHtml = `
            <div class="list-group-item list-group-item-action instrumento-item d-flex align-items-center" 
                 data-instrumento-id="${escapeHtml(insId)}" 
                 data-instrumento-desc="${escapeHtml(insDesc)}"
                 data-instrumento-simbolo="${escapeHtml(insSimbolo)}"
                 data-instrumento-tiene-detalle="${tieneDetalle}"
                 data-instrumento-total="${total}"
                 data-index="${index}"
                 style="cursor: pointer; transition: all 0.2s ease;">
                <div class="flex-grow-1">
                    <i class="${icono} me-2 ${colorClase}"></i>
                    <span class="instrumento-desc">${escapeHtml(insDesc)}</span>
                </div>
                <div style="width: 150px; text-align: right;">
                    <span class="instrumento-total fw-bold ${colorClase}">
                        ${insSimbolo} ${formatearNumero(total, 2)}
                    </span>
                </div>
            </div>
        `;

        $lista.append(itemHtml);

        console.log(`   ✅ [${index + 1}] ${insDesc} (${insId}) - ${insSimbolo} ${formatearNumero(total, 2)}`);
    });

    // ❸ Vincular eventos
    vincularEventosInstrumentos();

    console.log('✅ Instrumentos renderizados correctamente');
}

/**
 * ✅ v17.2: Resetea la selección de instrumento
 */
function resetearSeleccionInstrumento() {
    $('.instrumento-item').removeClass('selected');
    $('#btnConfirmarInstrumento').prop('disabled', true);
    window._instrumentoSeleccionado = null;
}

/**
 * ✅ v17.2: Selecciona un instrumento
 * @param {jQuery} $item - Elemento jQuery del instrumento
 */
function seleccionarInstrumento($item) {
    console.log('🔘 Seleccionando instrumento...');

    $('.instrumento-item').removeClass('selected');
    $item.addClass('selected');
    $('#btnConfirmarInstrumento').prop('disabled', false);

    window._instrumentoSeleccionado = {
        ins_id: $item.data('instrumento-id'),
        ins_desc: $item.data('instrumento-desc'),
        ins_simbolo: $item.data('instrumento-simbolo'),
        tiene_detalle: $item.data('instrumento-tiene-detalle'),
        total_actual: $item.data('instrumento-total')
    };

    console.log('✅ Instrumento seleccionado:', window._instrumentoSeleccionado);
}

/**
 * ✅ v17.2: Vincula eventos de instrumentos
 */
function vincularEventosInstrumentos() {
    console.log('🔧 Vinculando eventos de instrumentos...');

    $('.instrumento-item').off('click').on('click', function () {
        seleccionarInstrumento($(this));
    });

    $('.instrumento-item').off('dblclick').on('dblclick', function () {
        seleccionarInstrumento($(this));
        setTimeout(() => confirmarSeleccionInstrumento(), 300);
    });

    $('#btnConfirmarInstrumento').off('click').on('click', confirmarSeleccionInstrumento);

    console.log('✅ Eventos vinculados');
}


/**
 * ✅ v17.5: Confirma la selección del instrumento
 */
function confirmarSeleccionInstrumento() {
    console.log('═══════════════════════════════════════════════════');
    console.log('✅ CONFIRMAR INSTRUMENTO SELECCIONADO v17.5');
    console.log('═══════════════════════════════════════════════════');

    if (!window._instrumentoSeleccionado) {
        console.error('❌ No hay instrumento seleccionado');

        if (typeof toastr !== 'undefined') {
            toastr.warning('Debe seleccionar un instrumento');
        }

        return;
    }

    console.log('📋 Instrumento:', window._instrumentoSeleccionado);
    console.log('📋 Tipo MP:', window._tipoMedioPagoActual);

    // ❶ Cerrar modal de instrumentos
    cerrarModalInstrumentos();

    // ❷ Esperar cierre y abrir modal de detalle
    setTimeout(() => {
        const tcfId = window._tipoMedioPagoActual.tcf_id.toUpperCase();

        switch (tcfId) {
            case 'EF': // Efectivo
                abrirModalDetalleEfectivo(
                    window._instrumentoSeleccionado,
                    window._tipoMedioPagoActual
                );
                break;

            case 'CH': // Cheque
                console.warn('⚠️ Modal de cheque por implementar');
                if (typeof toastr !== 'undefined') {
                    toastr.info('Funcionalidad de cheques en desarrollo');
                }
                break;

            case 'TC': // Tarjeta Crédito
            case 'TD': // Tarjeta Débito
                console.warn('⚠️ Modal de tarjeta por implementar');
                if (typeof toastr !== 'undefined') {
                    toastr.info('Funcionalidad de tarjetas en desarrollo');
                }
                break;

            default:
                console.warn(`⚠️ Tipo ${tcfId} sin modal específico`);
                agregarValorDirecto(
                    window._instrumentoSeleccionado,
                    window._tipoMedioPagoActual
                );
                break;
        }
    }, 300);
}

/**
 * ✅ NUEVO v17.1: Procesa los instrumentos según requieran detalle o no
 * @param {Array} instrumentos - Array de instrumentos disponibles
 * @param {Object} tipoMedioPago - Tipo de medio de pago seleccionado
 */
function procesarInstrumentos(instrumentos, tipoMedioPago) {
    console.log('═══════════════════════════════════════════════════');
    console.log('🔄 PROCESAR INSTRUMENTOS v17.1');
    console.log(`   Total: ${instrumentos.length}`);
    console.log('═══════════════════════════════════════════════════');

    // ❶ Verificar si hay instrumentos
    if (!instrumentos || instrumentos.length === 0) {
        console.warn('⚠️ No hay instrumentos disponibles');

        if (typeof toastr !== 'undefined') {
            toastr.info('No hay instrumentos disponibles para este medio de pago');
        }

        return;
    }

    // ❷ Clasificar instrumentos por si tienen detalle
    const instrumentosConDetalle = instrumentos.filter(i => i.tiene_detalle === true);
    const instrumentosSinDetalle = instrumentos.filter(i => i.tiene_detalle === false);

    console.log(`📋 Con detalle: ${instrumentosConDetalle.length}`);
    console.log(`📋 Sin detalle: ${instrumentosSinDetalle.length}`);

    // ❸ Decidir flujo según la clasificación
    if (instrumentos.length === 1 && !instrumentos[0].tiene_detalle) {
        // ✅ CASO 1: Un solo instrumento sin detalle (ej: Efectivo en Pesos)
        console.log('✅ CASO 1: Instrumento único sin detalle - Agregar directamente');

        agregarValorDirecto(instrumentos[0], tipoMedioPago);

    } else if (instrumentosConDetalle.length > 0 || instrumentos.length > 1) {
        // ✅ CASO 2: Múltiples instrumentos o con detalle (mostrar modal)
        console.log('✅ CASO 2: Múltiples instrumentos o con detalle - Mostrar modal');

        abrirModalInstrumentos(instrumentos, tipoMedioPago);

    } else {
        // ✅ CASO 3: Múltiples instrumentos sin detalle (poco común)
        console.log('✅ CASO 3: Múltiples instrumentos sin detalle');

        abrirModalInstrumentos(instrumentos, tipoMedioPago);
    }
}

/**
 * ✅ NUEVO v17.1: Agrega un valor directamente a la tabla sin modal intermedio
 * Se usa cuando el medio de pago tiene un solo instrumento sin detalle
 * @param {Object} instrumento - Objeto con datos del instrumento
 * @param {Object} tipoMedioPago - Tipo de medio de pago seleccionado
 */
function agregarValorDirecto(instrumento, tipoMedioPago) {
    console.log('═══════════════════════════════════════════════════');
    console.log('➕ AGREGAR VALOR DIRECTO v17.1');
    console.log(`   Instrumento: ${instrumento.ins_desc}`);
    console.log(`   Tipo MP: ${tipoMedioPago.tcf_desc}`);
    console.log('═══════════════════════════════════════════════════');

    // ❶ Determinar el importe sugerido (diferencia pendiente)
    const diferencia = conceptosPago.diferencia || 0;
    const importeSugerido = Math.abs(diferencia);

    console.log(`💰 Importe sugerido: $ ${formatearNumero(importeSugerido, 2)}`);

    // ❷ Abrir modal de ingreso simple (por implementar en siguiente lote)
    // Por ahora, mostrar confirmación con SweetAlert o similar

    if (typeof Swal !== 'undefined') {
        Swal.fire({
            title: 'Ingresar Importe',
            html: `
                <div class="text-start mb-3">
                    <p class="mb-2">
                        <strong>Tipo:</strong> ${tipoMedioPago.tcf_desc}<br>
                        <strong>Instrumento:</strong> ${instrumento.ins_desc}
                    </p>
                </div>
                <div class="form-group">
                    <label for="swalImporte" class="form-label">Importe:</label>
                    <input type="number" 
                           id="swalImporte" 
                           class="form-control form-control-lg text-end" 
                           value="${importeSugerido}" 
                           step="0.01" 
                           min="0.01"
                           autofocus>
                </div>
            `,
            icon: 'question',
            showCancelButton: true,
            confirmButtonText: '<i class="bx bx-check"></i> Agregar',
            cancelButtonText: '<i class="bx bx-x"></i> Cancelar',
            confirmButtonColor: '#28a745',
            cancelButtonColor: '#dc3545',
            focusConfirm: false,
            preConfirm: () => {
                const importe = parseFloat($('#swalImporte').val());

                if (isNaN(importe) || importe <= 0) {
                    Swal.showValidationMessage('Debe ingresar un importe válido mayor a 0');
                    return false;
                }

                return { importe: importe };
            }
        }).then((result) => {
            if (result.isConfirmed) {
                const importe = result.value.importe;

                // ❸ Crear objeto de valor
                const nuevoValor = {
                    id: ++valorIdCounter,
                    tcf_id: tipoMedioPago.tcf_id,
                    tcf_desc: tipoMedioPago.tcf_desc,
                    ins_id: instrumento.ins_id,
                    ins_desc: instrumento.ins_desc,
                    ins_simbolo: instrumento.ins_simbolo || '$',
                    importe: importe,
                    observacion: '',
                    detalle: null // Sin detalle adicional
                };

                // ❹ Agregar a la tabla
                agregarFilaValor(nuevoValor);

                // ❺ Actualizar totales
                actualizarTotalesPago();

                console.log('✅ Valor agregado correctamente');

                if (typeof toastr !== 'undefined') {
                    toastr.success('Valor agregado correctamente');
                }
            }
        });
    } else {
        // Fallback sin SweetAlert
        const importe = prompt(`Ingrese el importe para ${tipoMedioPago.tcf_desc} - ${instrumento.ins_desc}:`, importeSugerido);

        if (importe !== null && importe !== '') {
            const importeNum = parseFloat(importe);

            if (!isNaN(importeNum) && importeNum > 0) {
                const nuevoValor = {
                    id: ++valorIdCounter,
                    tcf_id: tipoMedioPago.tcf_id,
                    tcf_desc: tipoMedioPago.tcf_desc,
                    ins_id: instrumento.ins_id,
                    ins_desc: instrumento.ins_desc,
                    ins_simbolo: instrumento.ins_simbolo || '$',
                    importe: importeNum,
                    observacion: '',
                    detalle: null
                };

                agregarFilaValor(nuevoValor);
                actualizarTotalesPago();
            }
        }
    }
}

/**
 * Obtener ícono según tipo de medio de pago
 */
function obtenerIconoMP(tcfId) {
    if (!tcfId) return 'bx bx-circle';

    const iconos = {
        'EF': 'bx bxs-dollar-circle',
        'CH': 'bx bx-receipt',
        'TC': 'bx bxs-credit-card',
        'TD': 'bx bxs-credit-card-alt',
        'TB': 'bx bx-transfer-alt',
        'MP': 'bx bxl-paypal',
        'VA': 'bx bx-gift',
        'CP': 'bx bx-purchase-tag',
        'CR': 'bx bx-file-blank',
        'CC': 'bx bx-spreadsheet',
        'NC': 'bx bx-receipt',
        'RE': 'bx bx-wallet'
    };

    return iconos[tcfId.toUpperCase()] || 'bx bx-circle';
}

/**
 * Formatear número con separadores de miles
 */
function formatearNumero(numero, decimales = 2) {
    if (isNaN(numero)) return '0.00';

    return parseFloat(numero).toLocaleString('es-AR', {
        minimumFractionDigits: decimales,
        maximumFractionDigits: decimales
    });
}

/**
 * Escapar HTML para prevenir XSS
 */
function escapeHtml(texto) {
    if (!texto) return '';

    const map = {
        '&': '&amp;',
        '<': '&lt;',
        '>': '&gt;',
        '"': '&quot;',
        "'": '&#039;'
    };

    return texto.toString().replace(/[&<>"']/g, function (m) {
        return map[m];
    });
}

/**
 * Mostrar mensaje de error
 */
function mostrarMensajeError(mensaje) {
    if (typeof AbrirMensaje === 'function') {
        AbrirMensaje(
            "Error",
            mensaje,
            function () { $("#msjModal").modal("hide"); },
            false,
            ["Aceptar"],
            "error!",
            null
        );
    } else {
        alert(`ERROR: ${mensaje}`);
    }
}

/**
 * ✅ ACTUALIZADO v18.0: Abre el modal de detalle de efectivo con InputMask
 * NUEVO: Aplica máscara monetaria argentina al input de monto
 * 
 * @param {Object} instrumento - Objeto con datos del instrumento
 * @param {Object} tipoMedioPago - Tipo de medio de pago
 */
function abrirModalDetalleEfectivo(instrumento, tipoMedioPago) {
    console.log('═══════════════════════════════════════════════════');
    console.log('🔓 ABRIR MODAL DETALLE EFECTIVO v18.0');
    console.log(`   Instrumento: ${instrumento.ins_desc} (${instrumento.ins_id})`);
    console.log(`   Tipo MP: ${tipoMedioPago.tcf_desc}`);
    console.log('═══════════════════════════════════════════════════');

    // ❶ Obtener elemento del modal
    const $modal = $('#modalDetalleEfectivo');

    if ($modal.length === 0) {
        console.error('❌ Modal #modalDetalleEfectivo no encontrado');

        if (typeof toastr !== 'undefined') {
            toastr.error('El modal de efectivo no está disponible');
        }

        return;
    }

    // ❷ Hidratar información del tipo e instrumento
    $('#lblTipoMedioPagoEfectivo').text(tipoMedioPago.tcf_desc);
    $('#lblInstrumentoEfectivo').text(`${instrumento.ins_desc} (${instrumento.ins_simbolo})`);

    // ❸ Calcular importe sugerido (diferencia pendiente)
    const diferencia = conceptosPago.diferencia || 0;
    const importeSugerido = Math.abs(diferencia);

    console.log(`💰 Importe sugerido: ${importeSugerido}`);

    // ❹ ✅ NUEVO: Verificar y aplicar máscara según símbolo de moneda
    const $inputMonto = $('#txtMontoEfectivo');

    // Remover máscara previa (si existe)
    if (typeof InputMaskMonetario !== 'undefined') {
        InputMaskMonetario.removerMascara($inputMonto);

        // Aplicar máscara según el símbolo
        if (instrumento.ins_simbolo === '$' || instrumento.ins_simbolo === 'ARS') {
            InputMaskMonetario.aplicarMascaraPesos($inputMonto);
        } else if (instrumento.ins_simbolo === 'USD') {
            InputMaskMonetario.aplicarMascaraDolares($inputMonto);
        } else {
            // Máscara genérica para otras monedas
            InputMaskMonetario.aplicarMascaraMonetaria($inputMonto, {
                prefix: `${instrumento.ins_simbolo} `
            });
        }

        console.log(`✅ Máscara aplicada para ${instrumento.ins_simbolo}`);

        // ❺ Establecer valor inicial usando la función del módulo
        InputMaskMonetario.establecerValor($inputMonto, importeSugerido);
    } else {
        console.warn('⚠️ InputMaskMonetario no disponible - usando valor sin formato');
        $inputMonto.val(importeSugerido.toFixed(2));
    }

    // ❻ Limpiar validaciones previas
    $inputMonto.removeClass('is-invalid is-valid');
    $('.invalid-feedback').remove();

    // ❼ Mostrar modal con jQuery (evitar problemas de Bootstrap)
    $modal
        .addClass('show')
        .css({
            'display': 'block',
            'opacity': '1',
            'z-index': '5100' // Sobre modal instrumentos (5090)
        })
        .attr('aria-modal', 'true')
        .removeAttr('aria-hidden');

    // ❽ Crear backdrop
    if ($('.modal-backdrop[data-modal="efectivo"]').length === 0) {
        $('body').append(
            '<div class="modal-backdrop fade show" ' +
            'data-modal="efectivo" ' +
            'style="z-index: 5099;"></div>'
        );
    }

    // ❾ Focus en el input con delay
    setTimeout(() => {
        $inputMonto.trigger("focus").trigger("select");
    }, INPUT_FOCUS_TIMEOUT);

    // ❿ Vincular evento de guardar
    $('#btnGuardarDetalleEfectivo')
        .off('click.guardar')
        .on('click.guardar', function () {
            guardarDetalleEfectivo(instrumento, tipoMedioPago);
        });

    // ⓫ Vincular evento Enter en el input
    $inputMonto
        .off('keypress.enter')
        .on('keypress.enter', function (e) {
            if (e.which === 13) { // Enter
                e.preventDefault();
                guardarDetalleEfectivo(instrumento, tipoMedioPago);
            }
        });

    console.log('✅ Modal detalle efectivo abierto con máscara aplicada');
}

/**
 * ✅ ACTUALIZADO v18.0: Guarda el detalle de efectivo usando InputMask
 * NUEVO: Extrae valor numérico limpio desde el input enmascarado
 * 
 * @param {Object} instrumento - Objeto con datos del instrumento
 * @param {Object} tipoMedioPago - Tipo de medio de pago
 */
function guardarDetalleEfectivo(instrumento, tipoMedioPago) {
    console.log('═══════════════════════════════════════════════════');
    console.log('💾 GUARDAR DETALLE EFECTIVO v18.0');
    console.log('═══════════════════════════════════════════════════');

    // ❶ ✅ ACTUALIZADO: Obtener valor numérico usando el módulo InputMask
    let monto = 0;

    if (typeof InputMaskMonetario !== 'undefined') {
        monto = InputMaskMonetario.obtenerValorNumerico('#txtMontoEfectivo');
        console.log(`   💰 Monto extraído con InputMask: ${monto}`);
    } else {
        // Fallback: parseo manual
        const montoStr = $('#txtMontoEfectivo').val();
        monto = parsearNumeroArgentino(montoStr);
        console.warn(`   ⚠️ InputMask no disponible - usando parseo manual: ${monto}`);
    }

    console.log(`   📝 Monto final: ${monto}`);

    // ❷ Validaciones
    if (isNaN(monto) || monto <= 0) {
        console.warn('⚠️ Monto inválido o cero');
        mostrarErrorCampo('#txtMontoEfectivo', 'Debe ingresar un monto válido mayor a cero');
        return;
    }

    // ❸ Validación de límite máximo (opcional)
    const diferencia = Math.abs(conceptosPago.diferencia || 0);

    // ✅ ACTUALIZADO v18.1: Usar constante configurable
    if (monto > diferencia * LIMITE_PORCENTAJE_DIFERENCIA) {
        console.warn(`⚠️ Monto muy alto: ${monto} > ${diferencia * LIMITE_PORCENTAJE_DIFERENCIA}`);
        console.warn(`   Diferencia pendiente: $ ${formatearNumero(diferencia, 2)}`);
        console.warn(`   Límite permitido (${LIMITE_PORCENTAJE_DIFERENCIA * 100}%): $ ${formatearNumero(diferencia * LIMITE_PORCENTAJE_DIFERENCIA, 2)}`);

        Swal.fire({
            title: '¿Monto elevado?',
            html: `<div class="text-start">
                       <p class="mb-3">El monto ingresado es <strong>mayor</strong> a la diferencia pendiente:</p>
                       <table class="table table-sm table-borderless mb-0">
                           <tr>
                               <td class="text-end">Monto ingresado:</td>
                               <td class="text-start"><strong class="text-danger">${instrumento.ins_simbolo} ${formatearNumero(monto, 2)}</strong></td>
                           </tr>
                           <tr>
                               <td class="text-end">Diferencia pendiente:</td>
                               <td class="text-start"><strong class="text-warning">${instrumento.ins_simbolo} ${formatearNumero(diferencia, 2)}</strong></td>
                           </tr>
                           <tr>
                               <td class="text-end">Excedente:</td>
                               <td class="text-start"><strong class="text-info">${instrumento.ins_simbolo} ${formatearNumero(monto - diferencia, 2)}</strong></td>
                           </tr>
                       </table>
                       <p class="mt-3 mb-0"><i class="bx bx-info-circle"></i> ¿Desea continuar de todos modos?</p>
                   </div>`,
            icon: 'warning',
            showCancelButton: true,
            confirmButtonText: '<i class="bx bx-check"></i> Sí, continuar',
            cancelButtonText: '<i class="bx bx-x"></i> No, corregir',
            confirmButtonColor: '#28a745',
            cancelButtonColor: '#6c757d',
            customClass: {
                popup: 'swal-wide'
            }
        }).then((result) => {
            if (result.isConfirmed) {
                finalizarGuardadoEfectivo(monto, instrumento, tipoMedioPago);
            } else {
                // Hacer focus en el input para corrección
                $('#txtMontoEfectivo').trigger("focus").trigger("select");
            }
        });

        return;
    }

    // ❹ Si validaciones OK, finalizar guardado
    finalizarGuardadoEfectivo(monto, instrumento, tipoMedioPago);
}

/**
 * ✅ NUEVO v17.5: Finaliza el guardado del efectivo
 * @param {number} monto - Monto validado
 * @param {Object} instrumento - Datos del instrumento
 * @param {Object} tipoMedioPago - Datos del tipo de medio de pago
 */
function finalizarGuardadoEfectivo(monto, instrumento, tipoMedioPago) {
    console.log('✅ Finalizando guardado de efectivo...');
    console.log(`   Monto: ${monto}`);

    // ❶ Crear objeto de valor
    const nuevoValor = {
        id: ++valorIdCounter,
        tcf_id: tipoMedioPago.tcf_id,
        tcf_desc: tipoMedioPago.tcf_desc,
        ins_id: instrumento.ins_id,
        ins_desc: instrumento.ins_desc,
        ins_simbolo: instrumento.ins_simbolo,
        importe: monto,
        observacion: '',
        detalle: null,
        fecha_creacion: new Date().toISOString()
    };

    console.log('📦 Nuevo valor creado:', nuevoValor);

    // ❷ Agregar a array global
    valoresPago.push(nuevoValor);

    // ❸ Agregar fila a la tabla
    agregarFilaValor(nuevoValor);

    // ❹ Actualizar totales del modal de pago
    actualizarTotalesPago();

    // ❺ Actualizar total del instrumento en modal instrumentos
    actualizarTotalInstrumento(instrumento.ins_id, monto);

    // ❻ Cerrar modal de efectivo
    cerrarModalDetalleEfectivo();

    // ❼ Notificación de éxito
    if (typeof toastr !== 'undefined') {
        toastr.success(
            `Efectivo agregado: ${instrumento.ins_simbolo} ${formatearNumero(monto, 2)}`,
            'Valor guardado',
            { timeOut: 3000 }
        );
    }

    console.log('✅ Valor de efectivo guardado correctamente');
}

/**
* ✅ NUEVO v17.5: Agrega una fila a la tabla de formas de pago
* @param {Object} valor - Objeto con datos del valor
*/
function agregarFilaValor(valor) {
    console.log('═══════════════════════════════════════════════════');
    console.log('➕ AGREGAR FILA A TABLA v17.5');
    console.log(`   ID: ${valor.id}`);
    console.log(`   Tipo: ${valor.tcf_desc}`);
    console.log(`   Instrumento: ${valor.ins_desc}`);
    console.log(`   Importe: ${valor.ins_simbolo} ${formatearNumero(valor.importe, 2)}`);
    console.log('═══════════════════════════════════════════════════');

    const $tbody = $('#tbodyFormasPago');

    // ❶ Remover fila de "sin valores" si existe
    $('#rowSinFormasPago').remove();

    // ❷ Construir HTML de la fila
    const filaHtml = `
        <tr data-valor-id="${valor.id}" class="fila-valor">
            <!-- # -->
            <td class="text-center align-middle">
                <span class="badge bg-secondary">${valor.id}</span>
            </td>
            
            <!-- Forma de Pago -->
            <td class="align-middle">
                <div>
                    <i class="${obtenerIconoMP(valor.tcf_id)} me-2 text-primary"></i>
                    <strong>${escapeHtml(valor.tcf_desc)}</strong>
                </div>
                <small class="text-muted">${escapeHtml(valor.ins_desc)}</small>
            </td>
            
            <!-- Importe -->
            <td class="text-end align-middle">
                <span class="fw-bold fs-5 text-success">
                    ${escapeHtml(valor.ins_simbolo)} ${formatearNumero(valor.importe, 2)}
                </span>
            </td>
            
            <!-- Observación -->
            <td class="align-middle">
                <small class="text-muted fst-italic">
                    ${valor.observacion || 'Sin observaciones'}
                </small>
            </td>
            
            <!-- Acciones -->
            <td class="text-center align-middle">
                <button class="btn btn-sm btn-outline-danger" 
                        onclick="eliminarValor(${valor.id})"
                        title="Eliminar">
                    <i class="bx bx-trash"></i>
                </button>
            </td>
        </tr>
    `;

    // ❸ Agregar fila al tbody
    $tbody.append(filaHtml);

    // ❹ Actualizar badge de cantidad
    const cantidadValores = valoresPago.length;
    $('#badgeCantidadPagos').text(`${cantidadValores} ${cantidadValores === 1 ? 'valor' : 'valores'}`);

    console.log('✅ Fila agregada correctamente');
}

/**
* ✅ NUEVO v17.5: Actualiza los totales del modal de pago
*/
function actualizarTotalesPago() {
    console.log('═══════════════════════════════════════════════════');
    console.log('🔄 ACTUALIZAR TOTALES PAGO v17.5');
    console.log('═══════════════════════════════════════════════════');

    // ❶ Calcular total de valores ingresados
    const totalValores = valoresPago.reduce((sum, v) => sum + v.importe, 0);

    // ❷ Calcular diferencia
    const totalAPagar = conceptosPago.totalPagar || 0;
    const recargos = conceptosPago.recargos || 0;
    const descuentos = conceptosPago.descuentos || 0;
    const diferencia = (totalAPagar + recargos - descuentos) - totalValores;

    // ❸ Actualizar objeto global
    conceptosPago.totalValores = totalValores;
    conceptosPago.diferencia = diferencia;

    console.log(`   Total a pagar: $ ${formatearNumero(totalAPagar, 2)}`);
    console.log(`   Recargos: $ ${formatearNumero(recargos, 2)}`);
    console.log(`   Descuentos: $ ${formatearNumero(descuentos, 2)}`);
    console.log(`   Total valores: $ ${formatearNumero(totalValores, 2)}`);
    console.log(`   Diferencia: $ ${formatearNumero(diferencia, 2)}`);

    // ❹ Actualizar UI
    $('#totalValores').text(`$ ${formatearNumero(totalValores, 2)}`);
    $('#diferencia').text(`$ ${formatearNumero(diferencia, 2)}`);

    // ❺ Cambiar color del badge según diferencia
    const $badgeDiferencia = $('#diferencia');
    $badgeDiferencia.removeClass('bg-success bg-warning bg-danger');

    if (Math.abs(diferencia) < 0.01) {
        $badgeDiferencia.addClass('bg-success'); // Verde = EXACTO
    } else if (diferencia > 0) {
        $badgeDiferencia.addClass('bg-warning'); // Amarillo = FALTA
    } else {
        $badgeDiferencia.addClass('bg-danger'); // Rojo = SOBRA
    }

    // ❻ Habilitar/deshabilitar botón finalizar
    const puedeFinelizar = Math.abs(diferencia) < 0.01 && valoresPago.length > 0;
    $('#btnFinalizarPago').prop('disabled', !puedeFinelizar);

    console.log('✅ Totales actualizados');
}

/**
 * ✅ NUEVO v17.5: Actualiza el total acumulado de un instrumento
 * @param {string} insId - ID del instrumento (ej: "ARS", "USD")
 * @param {number} monto - Monto a sumar
 */
function actualizarTotalInstrumento(insId, monto) {
    console.log(`🔄 Actualizando total instrumento: ${insId} (+${monto})`);

    // ❶ Buscar el elemento del instrumento en el modal
    const $instrumento = $(`.instrumento-item[data-instrumento-id="${insId}"]`);

    if ($instrumento.length === 0) {
        console.warn(`⚠️ Instrumento ${insId} no encontrado en modal`);
        return;
    }

    // ❷ Obtener total actual
    const $totalSpan = $instrumento.find('.instrumento-total');
    const totalActualStr = $totalSpan.text().replace(/[^0-9.-]/g, ''); // Quitar símbolos
    const totalActual = parseFloat(totalActualStr) || 0;

    // ❸ Calcular nuevo total
    const nuevoTotal = totalActual + monto;

    // ❹ Obtener símbolo
    const simbolo = $instrumento.data('instrumento-simbolo') || '$';

    // ❺ Actualizar UI con animación
    $totalSpan
        .addClass('updating')
        .text(`${simbolo} ${formatearNumero(nuevoTotal, 2)}`);

    setTimeout(() => {
        $totalSpan.removeClass('updating');
    }, 500);

    console.log(`✅ Total actualizado: ${simbolo} ${formatearNumero(nuevoTotal, 2)}`);
}

/**
* ✅ NUEVO v17.5: Muestra error en un campo
* @param {string} selector - Selector jQuery del campo
* @param {string} mensaje - Mensaje de error
*/
function mostrarErrorCampo(selector, mensaje) {
    const $campo = $(selector);

    $campo
        .addClass('is-invalid')
        .removeClass('is-valid');

    // Remover feedback anterior
    $campo.siblings('.invalid-feedback').remove();

    // Agregar nuevo feedback
    $campo.after(`<div class="invalid-feedback d-block">${escapeHtml(mensaje)}</div>`);

    // Focus y seleccionar
    $campo.focus().select();
}

/**
 * ✅ ACTUALIZADO v18.1: Cierra el modal de detalle efectivo
 * MEJORA: Limpieza más exhaustiva del formulario
 */
function cerrarModalDetalleEfectivo() {
    console.log('═══════════════════════════════════════════════════');
    console.log('🔒 CERRAR MODAL DETALLE EFECTIVO v18.1');
    console.log('═══════════════════════════════════════════════════');

    const $modal = $('#modalDetalleEfectivo');

    if ($modal.length === 0) {
        console.warn('⚠️ Modal #modalDetalleEfectivo no encontrado');
        return;
    }

    // ❶ Ocultar modal con jQuery
    $modal
        .removeClass('show')
        .css('display', 'none')
        .attr('aria-hidden', 'true')
        .removeAttr('aria-modal');

    console.log('   ✅ Modal ocultado');

    // ❷ Remover backdrop específico
    const $backdropEfectivo = $('.modal-backdrop[data-modal="efectivo"]');
    if ($backdropEfectivo.length > 0) {
        $backdropEfectivo.fadeOut(200, function () {
            $(this).remove();
        });
        console.log('   ✅ Backdrop removido');
    }

    // ❸ Limpiar formulario (el evento hidden.bs.modal también lo hará, pero por seguridad)
    const $input = $('#txtMontoEfectivo');
    $input
        .val('')
        .removeClass('is-invalid is-valid')
        .prop('disabled', false); // Asegurar que no quedó deshabilitado

    // ❹ Remover todos los mensajes de validación
    $input.siblings('.invalid-feedback').remove();
    $('.invalid-feedback').remove();

    console.log('   ✅ Formulario limpiado');

    // ❺ Resetear labels
    $('#lblTipoMedioPagoEfectivo').text('-');
    $('#lblInstrumentoEfectivo').text('-');

    console.log('   ✅ Labels reseteados');

    // ❻ Verificar si hay otros modales abiertos
    setTimeout(() => {
        if ($('.modal.show').length === 0) {
            $('body').removeClass('modal-open').css('overflow', '');
            console.log('   ✅ Body desbloqueado (no hay más modales)');
        } else {
            console.log('   ℹ️ Otros modales aún abiertos');
        }
    }, 100);

    console.log('═══════════════════════════════════════════════════');
    console.log('✅ MODAL CERRADO COMPLETAMENTE');
    console.log('═══════════════════════════════════════════════════');
}

/**
 * ✅ NUEVO v17.5: Elimina un valor de la tabla
 * @param {number} valorId - ID del valor a eliminar
 */
function eliminarValor(valorId) {
    console.log(`🗑️ Eliminando valor ID: ${valorId}`);

    // Buscar índice en array
    const index = valoresPago.findIndex(v => v.id === valorId);

    if (index === -1) {
        console.warn(`⚠️ Valor ${valorId} no encontrado`);
        return;
    }

    const valor = valoresPago[index];

    // Confirmar eliminación
    Swal.fire({
        title: '¿Eliminar valor?',
        html: `<strong>${valor.tcf_desc} - ${valor.ins_desc}</strong><br>` +
            `Importe: ${valor.ins_simbolo} ${formatearNumero(valor.importe, 2)}`,
        icon: 'question',
        showCancelButton: true,
        confirmButtonText: 'Sí, eliminar',
        cancelButtonText: 'Cancelar',
        confirmButtonColor: '#dc3545'
    }).then((result) => {
        if (result.isConfirmed) {
            // Remover del array
            valoresPago.splice(index, 1);

            // Remover fila del DOM
            $(`.fila-valor[data-valor-id="${valorId}"]`).fadeOut(300, function () {
                $(this).remove();

                // Si no quedan valores, mostrar mensaje
                if (valoresPago.length === 0) {
                    limpiarTablaFormasPago();
                }
            });

            // Actualizar totales
            actualizarTotalesPago();

            // Actualizar total del instrumento (restar)
            actualizarTotalInstrumento(valor.ins_id, -valor.importe);

            toastr.success('Valor eliminado correctamente');
        }
    });
}

/**
 * ✅ v17.4: Abre el modal de instrumentos usando jQuery
 * @param {Array} instrumentos - Array de instrumentos disponibles
 * @param {Object} tipoMedioPago - Tipo de medio de pago seleccionado
 */
function abrirModalInstrumentosJQuery(instrumentos, tipoMedioPago) {
    console.log('═══════════════════════════════════════════════════');
    console.log('🔓 ABRIR MODAL INSTRUMENTOS (jQuery) v17.4');
    console.log(`   Tipo MP: ${tipoMedioPago.tcf_desc}`);
    console.log(`   Total instrumentos: ${instrumentos.length}`);
    console.log('═══════════════════════════════════════════════════');

    const $modal = $('#modalInstrumentos');

    if ($modal.length === 0) {
        console.error('❌ Modal #modalInstrumentos no encontrado');

        if (typeof toastr !== 'undefined') {
            toastr.error('El modal de instrumentos no está disponible');
        }

        return;
    }

    // ❶ Resetear selección
    resetearSeleccionInstrumento();

    // ❷ Renderizar instrumentos
    renderizarInstrumentos(instrumentos);

    // ❸ Guardar contexto
    window._tipoMedioPagoActual = tipoMedioPago;

    // ❹ Forzar apertura con jQuery
    $modal
        .addClass('show')
        .css({
            'display': 'block',
            'opacity': '1',
            'z-index': '5090'
        })
        .attr('aria-modal', 'true')
        .attr('role', 'dialog')
        .removeAttr('aria-hidden');

    // ❺ Crear backdrop
    if ($('.modal-backdrop.show[data-modal="instrumentos"]').length === 0) {
        $('body').append(
            '<div class="modal-backdrop fade show" ' +
            'data-modal="instrumentos" ' +
            'style="z-index: 5089;"></div>'
        );
    }

    // ❻ Agregar clase al body
    $('body').addClass('modal-open').css('overflow', 'hidden');

    console.log('✅ Modal instrumentos abierto con jQuery');

    // ❼ Event listener para cerrar
    $modal.off('click.closeModal').on('click.closeModal', '.btn-close, .btn-secondary[data-bs-dismiss]', function () {
        cerrarModalInstrumentos();
    });
}


/**
* ✅ NUEVO v17.5: Cierra el modal de instrumentos
*/
function cerrarModalInstrumentos() {
    console.log('🔒 Cerrando modal instrumentos...');

    const $modal = $('#modalInstrumentos');

    if ($modal.length === 0) {
        console.warn('⚠️ Modal #modalInstrumentos no encontrado');
        return;
    }

    // ❶ Intentar cerrar con Bootstrap primero
    const modalInstance = bootstrap.Modal.getInstance($modal[0]);

    if (modalInstance) {
        try {
            modalInstance.hide();
            console.log('✅ Modal cerrado con Bootstrap');
        } catch (error) {
            console.warn('⚠️ Error al cerrar con Bootstrap:', error);
            // Fallback a jQuery
            cerrarModalInstrumentosJQuery();
        }
    } else {
        // ❷ Si no hay instancia Bootstrap, usar jQuery
        cerrarModalInstrumentosJQuery();
    }
}

/**
 * ✅ NUEVO v17.5: Cierra el modal de instrumentos usando jQuery directamente
 * Método alternativo para evitar problemas con Bootstrap
 */
function cerrarModalInstrumentosJQuery() {
    console.log('🔒 Cerrando modal instrumentos (jQuery)...');

    const $modal = $('#modalInstrumentos');

    // ❶ Ocultar modal
    $modal
        .removeClass('show')
        .css('display', 'none')
        .attr('aria-hidden', 'true')
        .removeAttr('aria-modal')
        .removeAttr('role');

    // ❷ Remover backdrop específico del modal instrumentos
    $('.modal-backdrop[data-modal="instrumentos"]').fadeOut(300, function () {
        $(this).remove();
    });

    // ❸ Solo remover clase modal-open si NO hay otros modales abiertos
    // (importante: puede haber modal de pago y modal de tipo MP abiertos)
    setTimeout(() => {
        if ($('.modal.show').length === 0) {
            $('body').removeClass('modal-open').css('overflow', '');
            console.log('✅ Body desbloqueado (no hay más modales)');
        } else {
            console.log('ℹ️ Body mantiene modal-open (hay otros modales abiertos)');
        }
    }, 100);

    // ❹ Limpiar selección
    resetearSeleccionInstrumento();

    console.log('✅ Modal instrumentos cerrado');
}

/**
* ✅ v17.5: Cierra el modal usando jQuery directamente
*/
function cerrarModalInstrumentosJQuery() {
    console.log('🔒 Cerrando modal instrumentos (jQuery)...');

    const $modal = $('#modalInstrumentos');

    $modal
        .removeClass('show')
        .css('display', 'none')
        .attr('aria-hidden', 'true')
        .removeAttr('aria-modal')
        .removeAttr('role');

    $('.modal-backdrop[data-modal="instrumentos"]').fadeOut(300, function () {
        $(this).remove();
    });

    setTimeout(() => {
        if ($('.modal.show').length === 0) {
            $('body').removeClass('modal-open').css('overflow', '');
            console.log('✅ Body desbloqueado');
        } else {
            console.log('ℹ️ Otros modales aún abiertos');
        }
    }, 100);

    resetearSeleccionInstrumento();

    console.log('✅ Modal instrumentos cerrado');
}

// ════════════════════════════════════════════════════════════
// FUNCIONES AUXILIARES DE FORMATO
// ════════════════════════════════════════════════════════════

/**
 * ✅ NUEVO v13.2: Parsea números en formato argentino a formato numérico estándar
 * Convierte texto con formato regional argentino a número JavaScript
 * 
 * Ejemplos:
 *   "$ 599.994,16"     → 599994.16
 *   "$ 1.234.567,89"   → 1234567.89
 *   "$ 599,99"         → 599.99
 *   "$ 100.000"        → 100000
 *   "$ -500,50"        → -500.50
 * 
 * Formato Argentino:
 *   - Separador de miles: . (punto)
 *   - Separador decimal: , (coma)
 *   - Símbolo de moneda: $ (opcional)
 * 
 * @param {string} texto - Texto con formato argentino (ej: "$ 599.994,16")
 * @returns {number} - Número parseado en formato estándar (ej: 599994.16)
 */
function parsearNumeroArgentino(texto) {
    console.log('═══════════════════════════════════════════════════');
    console.log('🔢 PARSEAR NÚMERO ARGENTINO v13.2');
    console.log(`   Entrada: "${texto}"`);
    console.log('═══════════════════════════════════════════════════');

    // ❶ Validar entrada
    if (!texto || typeof texto !== 'string') {
        console.warn('⚠️ Entrada inválida - No es una cadena de texto');
        console.warn(`   Tipo recibido: ${typeof texto}`);
        console.warn(`   Valor: ${texto}`);
        return 0;
    }

    // ❷ Limpiar espacios iniciales/finales
    let limpio = texto.trim();
    console.log(`   📝 Paso 1 - Después de trim(): "${limpio}"`);

    // ❸ Eliminar símbolo de moneda ($) y espacios internos
    limpio = limpio.replace(/[$\s]/g, '');
    console.log(`   📝 Paso 2 - Sin $ ni espacios: "${limpio}"`);

    // ❹ Eliminar puntos (separador de miles en formato argentino)
    limpio = limpio.replace(/\./g, '');
    console.log(`   📝 Paso 3 - Sin puntos de miles: "${limpio}"`);

    // ❺ Reemplazar coma decimal (formato argentino) por punto (formato estándar)
    limpio = limpio.replace(/,/g, '.');
    console.log(`   📝 Paso 4 - Coma → punto decimal: "${limpio}"`);

    // ❻ Parsear a número flotante
    const resultado = parseFloat(limpio);

    // ❼ Validar resultado
    if (isNaN(resultado)) {
        console.error('═══════════════════════════════════════════════════');
        console.error('❌ ERROR AL PARSEAR NÚMERO');
        console.error(`   Texto original: "${texto}"`);
        console.error(`   Texto limpio: "${limpio}"`);
        console.error(`   Resultado: NaN`);
        console.error('═══════════════════════════════════════════════════');
        return 0;
    }

    console.log('═══════════════════════════════════════════════════');
    console.log('✅ NÚMERO PARSEADO EXITOSAMENTE');
    console.log(`   Texto original: "${texto}"`);
    console.log(`   Número obtenido: ${resultado}`);
    console.log(`   Formato display: ${resultado.toFixed(2)}`);
    console.log('═══════════════════════════════════════════════════');

    return resultado;
}

/**
 * ✅ NUEVO v13.2: Formatea número al estilo argentino
 * Complemento de parsearNumeroArgentino() para operación inversa
 * 
 * @param {number} numero - Número a formatear
 * @param {number} decimales - Cantidad de decimales (default: 2)
 * @returns {string} - Número formateado (ej: "599.994,16")
 */
function formatearNumero(numero, decimales = 2) {
    if (isNaN(numero)) {
        console.warn(`⚠️ formatearNumero: entrada inválida (${numero})`);
        return '0,00';
    }

    return parseFloat(numero).toLocaleString('es-AR', {
        minimumFractionDigits: decimales,
        maximumFractionDigits: decimales
    });
}

/**
 * ✅ NUEVO v13.2: Escapa caracteres HTML para prevenir XSS
 * Reutilizable en todo el módulo
 * 
 * @param {string} texto - Texto a escapar
 * @returns {string} - Texto con caracteres especiales escapados
 */
function escapeHtml(texto) {
    if (!texto) return '';

    const mapa = {
        '&': '&amp;',
        '<': '&lt;',
        '>': '&gt;',
        '"': '&quot;',
        "'": '&#039;'
    };

    return texto.toString().replace(/[&<>"']/g, function (caracter) {
        return mapa[caracter];
    });
}

/**
* ✅ ACTUALIZADO v13.2: Abre el modal de pago con validación simplificada
* 
* CAMBIOS v13.2:
* - Usa parsearNumeroArgentino() para conversión correcta de formato
* - Logs mejorados para debugging de formato numérico
* - Validación de precisión decimal
* 
* CAMBIO ANTERIOR v13.1:
* - Llama directamente a abrirModalPago() en lugar de PagoFactura.abrirModal()
*/
function procesarPagoFactura() {
    console.log('═══════════════════════════════════════════════════');
    console.log('💰 PROCESAR PAGO DE FACTURA v13.2');
    console.log('═══════════════════════════════════════════════════');

    // ❶ VALIDACIÓN: Verificar que la función abrirModalPago esté disponible
    console.log('🔍 Verificando disponibilidad de la función abrirModalPago...');

    if (typeof abrirModalPago !== 'function') {
        console.error('═══════════════════════════════════════════════════');
        console.error('❌ CRÍTICO: Función abrirModalPago NO está disponible');
        console.error('═══════════════════════════════════════════════════');
        console.error('Diagnóstico:');
        console.error('   1. Verificar que el archivo pagoFactura.js esté cargado');
        console.error('   2. Ruta esperada: ~/js/app/pagoFactura.js');
        console.error('   3. Revisar consola del navegador para errores de carga');
        console.error('═══════════════════════════════════════════════════');

        mostrarMensajeError('El módulo de pago no está disponible.\nPor favor, recargue la página e intente nuevamente.');
        return;
    }

    console.log('✅ Función abrirModalPago disponible');

    // ❷ Extraer el total final de la tabla
    const $tdTotalFinal = $('#tdTotalFinal');

    if ($tdTotalFinal.length === 0) {
        console.error('❌ No se encontró el elemento #tdTotalFinal en el DOM');
        mostrarMensajeError('Error: No se pudo obtener el total de la factura');
        return;
    }

    const totalFinalTexto = $tdTotalFinal.text().trim();

    // ✅ CORREGIDO v13.2: Usar parseo específico para formato argentino
    const totalFinal = parsearNumeroArgentino(totalFinalTexto);

    console.log('═══════════════════════════════════════════════════');
    console.log('💵 EXTRACCIÓN DEL TOTAL FINAL');
    console.log(`   📝 Texto del DOM: "${totalFinalTexto}"`);
    console.log(`   🔢 Número parseado: ${totalFinal}`);
    console.log(`   💰 Formato display: $ ${totalFinal.toFixed(2)}`);
    console.log(`   ✅ Decimales preservados: ${(totalFinal % 1).toFixed(2)}`);
    console.log('═══════════════════════════════════════════════════');

    // ❸ Validar que el total sea mayor a 0
    if (totalFinal <= 0) {
        console.warn('⚠️ Total final es $0.00 o negativo');
        mostrarMensajeAdvertencia('El total de la factura debe ser mayor a $0.00');
        return;
    }

    // ❹ Validar precisión del número (seguridad adicional)
    if (totalFinal > 999999999.99) {
        console.error('❌ Total fuera de rango permitido');
        mostrarMensajeError('El total de la factura excede el valor máximo permitido.');
        return;
    }

    // ❺ Preparar datos para el modal de pago
    const datosPago = {
        totales: {
            totalPagar: totalFinal,
            recargos: 0,
            descuentos: 0,
            totalValores: 0
        },
        puntoVenta: $('#lblPuntoVentaCalculo').text().trim() || 'GECO PV'
    };

    console.log('═══════════════════════════════════════════════════');
    console.log('📋 DATOS PREPARADOS PARA MODAL DE PAGO');
    console.log('   Estructura completa:', datosPago);
    console.log(`   Total a pagar: $ ${datosPago.totales.totalPagar.toFixed(2)}`);
    console.log(`   Punto de venta: ${datosPago.puntoVenta}`);
    console.log('═══════════════════════════════════════════════════');

    // ❻ ✅ Llamar directamente a la función
    try {
        console.log('🔓 Invocando abrirModalPago()...');

        const resultado = abrirModalPago(datosPago);

        if (resultado === false) {
            console.error('❌ abrirModalPago() retornó false');
            mostrarMensajeError('Error al abrir el modal de pago. Revise la consola para más detalles.');
        } else {
            console.log('✅ Modal de pago abierto correctamente');
        }
    } catch (error) {
        console.error('═══════════════════════════════════════════════════');
        console.error('❌ EXCEPCIÓN AL ABRIR MODAL DE PAGO');
        console.error('═══════════════════════════════════════════════════');
        console.error('Error:', error);
        console.error('Stack:', error.stack);
        console.error('═══════════════════════════════════════════════════');

        mostrarMensajeError(`Error al abrir el modal de pago: ${error.message}`);
    }

    console.log('═══════════════════════════════════════════════════');
}