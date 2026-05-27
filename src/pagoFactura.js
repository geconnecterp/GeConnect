// ════════════════════════════════════════════════════════════
// GESTOR DE PAGO DE FACTURA
// ════════════════════════════════════════════════════════════
// VERSIÓN v17.0 - Exportación de módulo público
// ════════════════════════════════════════════════════════════

// ═══════════════════════════════════════════════════════════════════
// ✅ NUEVO: OBJETO PÚBLICO EXPORTADO
// ═══════════════════════════════════════════════════════════════════

/**
 * ✅ NUEVO v17.0: Módulo público de gestión de pagos
 * Expone las funciones necesarias para ser invocadas desde otros módulos
 */
window.PagoFactura = {
    /**
     * Abre el modal de pago con los datos de la factura
     * @param {Object} datosFactura - Objeto con totales y datos del cliente
     * @returns {boolean} - true si se abrió correctamente, false si hubo error
     */
    abrirModal: function(datosFactura) {
        return abrirModalPago(datosFactura);
    },

    /**
     * Obtiene el estado actual del pago
     * @returns {Object} - Objeto con información del estado
     */
    obtenerEstado: function() {
        return {
            diferencia: conceptosPago.diferencia || 0,
            totalValores: conceptosPago.totalValores || 0,
            cantidadValores: valoresPago.length || 0
        };
    },

    /**
     * Cierra el modal de pago
     */
    cerrar: function() {
        if (modalPagoInstance) {
            modalPagoInstance.hide();
        }
    }
};

// ═══════════════════════════════════════════════════════════════════
// SECCIÓN 1: VARIABLES GLOBALES
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
// SECCIÓN 2: INICIALIZACIÓN
// ═══════════════════════════════════════════════════════════════════

$(function () {
    console.log('🚀 Módulo de Pago de Factura inicializado v17.0');
    inicializarModales();
    inicializarEventosPago();
    console.log('✅ Módulo de Pago listo y exportado como window.PagoFactura');
});

/**
 * Abre el modal de pago con los datos de la factura
 * @param {Object} datosFactura - Objeto con totales y datos del cliente
 */
function abrirModalPago(datosFactura) {
    console.log('═══════════════════════════════════════════════════');
    console.log('🔓 ABRIR MODAL DE PAGO v16.2');
    console.log('═══════════════════════════════════════════════════');

    if (!modalPagoInstance) {
        console.error('❌ Modal de pago no inicializado');
        mostrarMensajeError('El módulo de pago no está disponible. Por favor, recargue la página.');
        return false;
    }

    try {
        ocultarModalCalculoFactura();
        hidratarDatosClientePago();
        cargarConceptosPago(datosFactura?.totales || {});
        limpiarTablaFormasPago();
        
        modalPagoInstance.show();

        // ❌ ELIMINADAS ESTAS LÍNEAS:
        // setTimeout(() => {
        //     $('#modalPago').css('z-index', '1060');
        //     $('.modal-backdrop').last().css('z-index', '1059');
        // }, 100);

        console.log('✅ Modal de pago abierto correctamente');
        return true;

    } catch (error) {
        console.error('❌ Error al abrir modal de pago:', error);
        mostrarMensajeError(`No se pudo abrir el modal de pago.\n\n${error.message}`);
        return false;
    }
}

/**
 * ✅ ACTUALIZADO v16.2: Abre el modal de selección de tipo de medio de pago
 */
function abrirModalTipoMedioPago() {
    console.log('═══════════════════════════════════════════════════');
    console.log('🔓 ABRIR MODAL TIPO MEDIO DE PAGO v16.2');
    console.log('═══════════════════════════════════════════════════');

    if (!modalTipoMedioPagoInstance) {
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

    resetearSeleccionTipoMedioPago();
    modalTipoMedioPagoInstance.show();

    // ✅ VERIFICACIÓN DE Z-INDEX (Solo para debug)
    setTimeout(() => {
        const zIndexModalPago = parseInt($('#modalPago').css('z-index'));
        const zIndexModalTipo = parseInt($('#modalTipoMedioPago').css('z-index'));
        
        console.log('🔍 Verificación de z-index:');
        console.log(`   Modal Pago: ${zIndexModalPago}`);
        console.log(`   Modal Tipo MP: ${zIndexModalTipo}`);
        
        if (zIndexModalTipo <= zIndexModalPago) {
            console.error('❌ CRÍTICO: Modal hijo tiene z-index menor o igual al padre');
            console.error(`   Esperado: ${zIndexModalTipo} > ${zIndexModalPago}`);
            console.error('   El modal podría no ser visible');
        } else {
            console.log('✅ Jerarquía de z-index correcta');
        }
    }, 150);

    bloquearModalTipoMedioPago('Cargando opciones de pago...');

    cargarValoresMP()
        .then(function (valoresMP) {
            renderizarOpcionesMP(valoresMP);

            const $primerItem = $('.tipo-medio-pago-item').first();
            if ($primerItem.length > 0) {
                seleccionarItemTipoMedioPago($primerItem);
            }

            vincularEventosTipoMedioPago();
            console.log('✅ Modal de tipo medio de pago listo');
        })
        .catch(function (error) {
            console.error('❌ Error al cargar valores MP:', error);

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