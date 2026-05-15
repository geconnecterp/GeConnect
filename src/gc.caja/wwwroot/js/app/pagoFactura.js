/*!
 * pagoFactura.js
 * Sistema de Gestión de Pago - GeConnect
 * Versión v4.0 - Inicialización mejorada
 * Autor: GeConnect ERP
 * Fecha: 2026
 */

// ════════════════════════════════════════════════════════════
// VALIDACIÓN DE DEPENDENCIAS INMEDIATA
// ════════════════════════════════════════════════════════════
(function validarDependenciasInmediata() {
    console.log('═══════════════════════════════════════════════════');
    console.log('🔍 VALIDANDO DEPENDENCIAS DE PAGO FACTURA v4.0');
    console.log('═══════════════════════════════════════════════════');

    const dependencias = {
        'jQuery': typeof jQuery !== 'undefined',
        '$': typeof $ !== 'undefined',
        'Bootstrap Modal': typeof bootstrap !== 'undefined' && typeof bootstrap.Modal !== 'undefined'
    };

    let todasDisponibles = true;

    Object.keys(dependencias).forEach(dep => {
        const disponible = dependencias[dep];
        console.log(`   ${disponible ? '✅' : '❌'} ${dep}: ${disponible ? 'Disponible' : 'NO disponible'}`);
        if (!disponible) todasDisponibles = false;
    });

    if (!todasDisponibles) {
        console.error('❌ CRÍTICO: Faltan dependencias requeridas');
        console.error('   El módulo PagoFactura NO se inicializará');
        console.log('═══════════════════════════════════════════════════');
        return;
    }

    console.log('✅ Todas las dependencias están disponibles');
    console.log('═══════════════════════════════════════════════════');
})();

// ════════════════════════════════════════════════════════════
// NAMESPACE GLOBAL
// ════════════════════════════════════════════════════════════
window.PagoFactura = (function() {
    'use strict';

    console.log('🚀 Creando namespace PagoFactura...');

    // ════════════════════════════════════════════════════════
    // VARIABLES PRIVADAS
    // ════════════════════════════════════════════════════════
    let _modalPago = null;
    let _modalCalculoFactura = null;
    let _datosCliente = {};
    let _conceptosPago = {
        totalPagar: 0,
        recargos: 0,
        descuentos: 0,
        totalValores: 0,
        diferencia: 0
    };
    let _valoresPago = [];

    // ════════════════════════════════════════════════════════
    // SELECTORES DOM
    // ════════════════════════════════════════════════════════
    const DOM = {
        modal: '#modalPago',
        modalCalculo: '#modalCalculoFactura',
        totalPagar: '#totalPagar',
        totalRecargos: '#totalRecargos',
        totalDescuentos: '#totalDescuentos',
        totalValores: '#totalValores',
        diferencia: '#diferencia',
        btnAgregarPago: '#btnAgregarPago',
        btnVolverPago: '#btnVolverPago',
        btnFinalizarPago: '#btnFinalizarPago'
    };

    // ════════════════════════════════════════════════════════
    // INICIALIZACIÓN
    // ════════════════════════════════════════════════════════
    function init() {
        console.log('═══════════════════════════════════════════════════');
        console.log('✅ Inicializando módulo PagoFactura v4.0');
        console.log('═══════════════════════════════════════════════════');

        // ❶ Validar jQuery
        if (typeof $ === 'undefined') {
            console.error('❌ jQuery no disponible');
            return false;
        }

        // ❷ Validar Bootstrap
        if (typeof bootstrap === 'undefined' || typeof bootstrap.Modal === 'undefined') {
            console.error('❌ Bootstrap Modal no disponible');
            return false;
        }

        // ❸ Buscar modal en el DOM
        const modalElement = document.querySelector(DOM.modal);
        if (!modalElement) {
            console.error(`❌ Modal ${DOM.modal} no encontrado en el DOM`);
            return false;
        }

        console.log(`✅ Modal encontrado: ${DOM.modal}`);

        // ❹ Crear instancia de Bootstrap Modal
        try {
            _modalPago = new bootstrap.Modal(modalElement, {
                backdrop: 'static',
                keyboard: false
            });
            console.log('✅ Instancia de Bootstrap Modal creada');
        } catch (error) {
            console.error('❌ Error al crear modal:', error);
            return false;
        }

        // ❺ Obtener modal de cálculo
        const modalCalculoElement = document.querySelector(DOM.modalCalculo);
        if (modalCalculoElement) {
            _modalCalculoFactura = bootstrap.Modal.getInstance(modalCalculoElement) || 
                                   new bootstrap.Modal(modalCalculoElement);
            console.log(`✅ Modal de cálculo encontrado: ${DOM.modalCalculo}`);
        } else {
            console.warn(`⚠️ Modal de cálculo no encontrado: ${DOM.modalCalculo}`);
        }

        // ❻ Vincular eventos
        _vincularEventos();

        console.log('✅ Módulo PagoFactura inicializado correctamente');
        console.log('═══════════════════════════════════════════════════');

        return true;
    }

    // ════════════════════════════════════════════════════════
    // VINCULAR EVENTOS
    // ════════════════════════════════════════════════════════
    function _vincularEventos() {
        console.log('🔧 Vinculando eventos del modal de pago...');

        $(DOM.btnAgregarPago).off('click').on('click', _agregarFormaPago);
        $(DOM.btnVolverPago).off('click').on('click', _volverACalculoFactura);
        $(DOM.btnFinalizarPago).off('click').on('click', _finalizarPago);

        $(DOM.modal).off('hidden.bs.modal').on('hidden.bs.modal', _limpiarModal);
        $(DOM.modal).off('shown.bs.modal').on('shown.bs.modal', function() {
            console.log('📋 Modal de pago abierto');
            setTimeout(() => $(DOM.btnAgregarPago).trigger('focus'), 300);
        });

        console.log('✅ Eventos vinculados');
    }

    // ════════════════════════════════════════════════════════════
    // ABRIR MODAL
    // ════════════════════════════════════════════════════════════
    function abrirModal(datosFactura) {
        console.log('═══════════════════════════════════════════════════');
        console.log('🔓 ABRIR MODAL DE PAGO v4.1');
        console.log('═══════════════════════════════════════════════════');

        if (!_modalPago) {
            console.error('❌ Modal no inicializado');
            alert('Error: Modal de pago no disponible. Por favor, recargue la página.');
            return false;
        }

        console.log('Datos recibidos:', datosFactura);

        try {
            // ❶ Ocultar modal de cálculo
            _ocultarModalCalculoFactura();

            // ❷ Hidratar datos
            _hidratarDatosClientePago();
            _cargarConceptosPago(datosFactura?.totales || {});

            // ❸ Mostrar modal
            _modalPago.show();

            // ❹ ✅ NUEVO: Forzar z-index DESPUÉS de mostrar el modal
            setTimeout(() => {
                const $modal = $('#modalPago');
                const $backdrop = $('.modal-backdrop').last();

                $modal.css('z-index', '1060');
                $backdrop.css('z-index', '1059');

                console.log('✅ Z-index ajustado:');
                console.log(`   Modal: ${$modal.css('z-index')}`);
                console.log(`   Backdrop: ${$backdrop.css('z-index')}`);
            }, 100);

            console.log('✅ Modal de pago abierto correctamente');
            console.log('═══════════════════════════════════════════════════');

            return true;
        } catch (error) {
            console.error('═══════════════════════════════════════════════════');
            console.error('❌ ERROR AL ABRIR MODAL');
            console.error(error);
            console.error('═══════════════════════════════════════════════════');

            alert('Error al abrir el modal de pago. Revise la consola para más detalles.');
            return false;
        }
    }

    // ════════════════════════════════════════════════════════
    // OCULTAR MODAL DE CÁLCULO
    // ════════════════════════════════════════════════════════
    function _ocultarModalCalculoFactura() {
        console.log('🔒 Ocultando modal de cálculo...');

        if (_modalCalculoFactura) {
            try {
                _modalCalculoFactura.hide();
                console.log('✅ Modal de cálculo ocultado');
            } catch (error) {
                console.warn('⚠️ No se pudo ocultar el modal de cálculo:', error);
            }
        } else {
            const $modalCalculo = $(DOM.modalCalculo);
            if ($modalCalculo.length > 0 && $modalCalculo.hasClass('show')) {
                $modalCalculo.modal('hide');
                console.log('✅ Modal de cálculo ocultado (fallback)');
            }
        }
    }

    // ════════════════════════════════════════════════════════
    // VOLVER A MODAL DE CÁLCULO
    // ════════════════════════════════════════════════════════
    function _volverACalculoFactura() {
        console.log('🔙 Volver a modal de cálculo...');

        _cerrarModal();

        setTimeout(() => {
            if (_modalCalculoFactura) {
                _modalCalculoFactura.show();
            } else {
                $(DOM.modalCalculo).modal('show');
            }
            console.log('✅ Modal de cálculo reabierto');
        }, 350);
    }

    // ════════════════════════════════════════════════════════
    // HIDRATAR DATOS DEL CLIENTE
    // ════════════════════════════════════════════════════════
    function _hidratarDatosClientePago() {
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

        Object.keys(mapeoIds).forEach(idOrigen => {
            const idDestino = mapeoIds[idOrigen];
            const valorOrigen = $(`#${idOrigen}`).val() || '';
            $(`#${idDestino}`).val(valorOrigen);
        });

        // Badge
        const badgeHtml = $('#badgeTipoComprobanteCalc').html();
        $('#badgeTipoComprobantePago').html(badgeHtml);

        console.log('✅ Datos del cliente hidratados');
    }

    // ════════════════════════════════════════════════════════
    // CARGAR CONCEPTOS DE PAGO
    // ════════════════════════════════════════════════════════
    function _cargarConceptosPago(totales) {
        _conceptosPago = {
            totalPagar: parseFloat(totales?.totalPagar || 0),
            recargos: parseFloat(totales?.recargos || 0),
            descuentos: parseFloat(totales?.descuentos || 0),
            totalValores: parseFloat(totales?.totalValores || 0),
            diferencia: parseFloat(totales?.totalPagar || 0)
        };

        _actualizarConceptosPago();
    }

    // ════════════════════════════════════════════════════════
    // ACTUALIZAR CONCEPTOS EN UI
    // ════════════════════════════════════════════════════════
    function _actualizarConceptosPago() {
        $(DOM.totalPagar).text(_formatearMoneda(_conceptosPago.totalPagar));
        $(DOM.totalRecargos).text(_formatearMoneda(_conceptosPago.recargos));
        $(DOM.totalDescuentos).text(_formatearMoneda(_conceptosPago.descuentos));
        $(DOM.totalValores).text(_formatearMoneda(_conceptosPago.totalValores));
        $(DOM.diferencia).text(_formatearMoneda(_conceptosPago.diferencia));

        const esCero = Math.abs(_conceptosPago.diferencia) < 0.01;
        $(DOM.btnFinalizarPago).prop('disabled', !esCero);
    }

    // ════════════════════════════════════════════════════════
    // FUNCIONES DE ACCIÓN
    // ════════════════════════════════════════════════════════
    function _agregarFormaPago() {
        console.log('➕ Agregar forma de pago...');
        alert('Funcionalidad en desarrollo');
    }

    function _finalizarPago() {
        console.log('✔️ Finalizar pago...');

        if (Math.abs(_conceptosPago.diferencia) >= 0.01) {
            alert('La diferencia debe ser $0.00');
            return;
        }

        alert('Pago procesado correctamente');
        _cerrarModal();
    }

    function _cerrarModal() {
        if (_modalPago) {
            _modalPago.hide();
        }
    }

    function _limpiarModal() {
        _conceptosPago = {
            totalPagar: 0,
            recargos: 0,
            descuentos: 0,
            totalValores: 0,
            diferencia: 0
        };

        _actualizarConceptosPago();
        $(DOM.btnFinalizarPago).prop('disabled', true);
    }

    // ════════════════════════════════════════════════════════
    // HELPERS
    // ════════════════════════════════════════════════════════
    function _formatearMoneda(valor) {
        const numero = parseFloat(valor) || 0;
        return numero.toLocaleString('es-AR', {
            minimumFractionDigits: 2,
            maximumFractionDigits: 2
        });
    }

    // ════════════════════════════════════════════════════════
    // API PÚBLICA
    // ════════════════════════════════════════════════════════
    return {
        init: init,
        abrirModal: abrirModal,
        cerrarModal: _cerrarModal,
        volverACalculoFactura: _volverACalculoFactura
    };
})();

// ════════════════════════════════════════════════════════════
// INICIALIZACIÓN AUTOMÁTICA CON MÚLTIPLES INTENTOS
// ════════════════════════════════════════════════════════════
(function intentarInicializar() {
    console.log('═══════════════════════════════════════════════════');
    console.log('🚀 INTENTANDO INICIALIZAR PAGO FACTURA');
    console.log('═══════════════════════════════════════════════════');

    let intentos = 0;
    const maxIntentos = 5;
    const intervalo = 200;

    function intentar() {
        intentos++;
        console.log(`   Intento ${intentos}/${maxIntentos}...`);

        if (typeof $ === 'undefined' || typeof bootstrap === 'undefined') {
            console.warn('   ⏳ Esperando dependencias...');

            if (intentos < maxIntentos) {
                setTimeout(intentar, intervalo);
            } else {
                console.error('❌ Máximo de intentos alcanzado. Dependencias no disponibles.');
            }
            return;
        }

        // Dependencias disponibles
        $(function() {
            console.log('✅ Dependencias disponibles, inicializando...');

            const exitoso = window.PagoFactura.init();

            if (exitoso) {
                console.log('✅ PagoFactura inicializado y expuesto globalmente');
                console.log('═══════════════════════════════════════════════════');
            } else {
                console.error('❌ PagoFactura NO se inicializó correctamente');
                console.log('═══════════════════════════════════════════════════');
            }
        });
    }

    intentar();
})();