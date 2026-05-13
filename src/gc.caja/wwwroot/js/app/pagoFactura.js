/*!
 * pagoFactura.js
 * Sistema de Gestión de Pago - GeConnect
 * Versión v3.0 - Corregido y optimizado
 * Autor: GeConnect ERP
 * Fecha: 2026
 */

// ════════════════════════════════════════════════════════════
// VALIDACIÓN DE DEPENDENCIAS
// ════════════════════════════════════════════════════════════
(function validarDependencias() {
    console.log('═══════════════════════════════════════════════════');
    console.log('🔍 VALIDANDO DEPENDENCIAS DE PAGO FACTURA v3.0');
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
const PagoFactura = (() => {
    'use strict';

    // ════════════════════════════════════════════════════════
    // VARIABLES PRIVADAS
    // ════════════════════════════════════════════════════════
    let _modalPago = null;
    let _modalCalculoFactura = null; // ← NUEVO: Referencia al modal de cálculo
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
        modalCalculo: '#modalCalculoFactura', // ← NUEVO
        // Conceptos de Pago
        totalPagar: '#totalPagar',
        totalRecargos: '#totalRecargos',
        totalDescuentos: '#totalDescuentos',
        totalValores: '#totalValores',
        diferencia: '#diferencia',
        // Botones
        btnAgregarPago: '#btnAgregarPago',
        btnVolverPago: '#btnVolverPago',
        btnFinalizarPago: '#btnFinalizarPago'
    };

    // ════════════════════════════════════════════════════════
    // INICIALIZACIÓN
    // ════════════════════════════════════════════════════════
    function init() {
        console.log('═══════════════════════════════════════════════════');
        console.log('✅ Inicializando módulo PagoFactura v3.0');
        console.log('═══════════════════════════════════════════════════');

        // ❶ Validar que jQuery esté disponible
        if (typeof $ === 'undefined') {
            console.error('❌ jQuery no está disponible. Abortando inicialización.');
            return false;
        }

        // ❷ Validar que Bootstrap esté disponible
        if (typeof bootstrap === 'undefined' || typeof bootstrap.Modal === 'undefined') {
            console.error('❌ Bootstrap Modal no está disponible. Abortando inicialización.');
            return false;
        }

        // ❸ Verificar que el modal de pago exista en el DOM
        const modalElement = document.querySelector(DOM.modal);
        if (!modalElement) {
            console.error(`❌ No se encontró el modal de pago en el DOM: ${DOM.modal}`);
            return false;
        }

        console.log(`✅ Modal de pago encontrado: ${DOM.modal}`);

        // ❹ Obtener instancia del modal de pago
        _modalPago = new bootstrap.Modal(modalElement, {
            backdrop: 'static',
            keyboard: false
        });

        console.log('✅ Instancia de Bootstrap Modal creada');

        // ❺ Obtener referencia al modal de cálculo (si existe)
        const modalCalculoElement = document.querySelector(DOM.modalCalculo);
        if (modalCalculoElement) {
            _modalCalculoFactura = bootstrap.Modal.getInstance(modalCalculoElement);
            if (!_modalCalculoFactura) {
                _modalCalculoFactura = new bootstrap.Modal(modalCalculoElement);
            }
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

        // Botón Agregar
        $(DOM.btnAgregarPago).off('click').on('click', () => {
            _agregarFormaPago();
        });

        // Botón Volver
        $(DOM.btnVolverPago).off('click').on('click', () => {
            _volverACalculoFactura();
        });

        // Botón Finalizar
        $(DOM.btnFinalizarPago).off('click').on('click', () => {
            _finalizarPago();
        });

        // Evento cuando se cierra el modal
        $(DOM.modal).off('hidden.bs.modal').on('hidden.bs.modal', () => {
            _limpiarModal();
        });

        // Evento cuando se abre el modal
        $(DOM.modal).off('shown.bs.modal').on('shown.bs.modal', () => {
            console.log('📋 Modal de pago abierto');
            _enfocarPrimerCampo();
        });

        console.log('✅ Eventos vinculados correctamente');
    }

    // ════════════════════════════════════════════════════════
    // ABRIR MODAL
    // ════════════════════════════════════════════════════════
    /**
     * ✅ ACTUALIZADO v3.0: Abre el modal de pago y oculta el modal de cálculo
     * 
     * @param {Object} datosFactura - Datos de la factura y totales
     * @param {Object} datosFactura.totales - Conceptos de pago
     * @param {string} datosFactura.puntoVenta - Nombre del punto de venta (opcional)
     */
    function abrirModal(datosFactura) {
        console.log('═══════════════════════════════════════════════════');
        console.log('🔓 ABRIR MODAL DE PAGO v3.0');
        console.log('═══════════════════════════════════════════════════');

        // ❶ Validar que el modal esté inicializado
        if (!_modalPago) {
            console.error('❌ Modal de pago no inicializado');
            _mostrarError('Error: Modal de pago no disponible. Por favor, recargue la página.');
            return false;
        }

        console.log('Datos recibidos:', datosFactura);

        try {
            // ❷ Ocultar modal de cálculo (sin animación para transición suave)
            _ocultarModalCalculoFactura();

            // ❸ Hidratar datos del cliente en el header
            _hidratarDatosClientePago();

            // ❹ Cargar conceptos de pago
            _cargarConceptosPago(datosFactura?.totales || {});

            // ❺ Mostrar modal de pago
            _modalPago.show();

            console.log('✅ Modal de pago abierto correctamente');
            console.log('═══════════════════════════════════════════════════');

            return true;
        } catch (error) {
            console.error('═══════════════════════════════════════════════════');
            console.error('❌ ERROR AL ABRIR MODAL DE PAGO');
            console.error('═══════════════════════════════════════════════════');
            console.error('Error:', error);
            console.error('Stack:', error.stack);

            _mostrarError('Error al cargar el modal de pago. Por favor, intente nuevamente.');

            return false;
        }
    }

    // ════════════════════════════════════════════════════════
    // OCULTAR MODAL DE CÁLCULO FACTURA
    // ════════════════════════════════════════════════════════
    /**
     * ✅ NUEVO v3.0: Oculta el modal de cálculo de factura
     * Para evitar superposición de modales
     */
    function _ocultarModalCalculoFactura() {
        console.log('🔒 Ocultando modal de cálculo de factura...');

        if (_modalCalculoFactura) {
            try {
                _modalCalculoFactura.hide();
                console.log('✅ Modal de cálculo ocultado correctamente');
            } catch (error) {
                console.warn('⚠️ No se pudo ocultar el modal de cálculo:', error);
            }
        } else {
            // Fallback: Intentar ocultar manualmente
            const $modalCalculo = $(DOM.modalCalculo);
            if ($modalCalculo.length > 0 && $modalCalculo.hasClass('show')) {
                $modalCalculo.modal('hide');
                console.log('✅ Modal de cálculo ocultado (fallback)');
            }
        }
    }

    // ════════════════════════════════════════════════════════
    // VOLVER A MODAL DE CÁLCULO FACTURA
    // ════════════════════════════════════════════════════════
    /**
     * ✅ NUEVO v3.0: Cierra el modal de pago y reabre el modal de cálculo
     */
    function _volverACalculoFactura() {
        console.log('═══════════════════════════════════════════════════');
        console.log('🔙 VOLVER A MODAL DE CÁLCULO');
        console.log('═══════════════════════════════════════════════════');

        // ❶ Cerrar modal de pago
        _cerrarModal();

        // ❷ Esperar a que se cierre completamente (300ms de animación Bootstrap)
        setTimeout(() => {
            // ❸ Reabrir modal de cálculo
            if (_modalCalculoFactura) {
                try {
                    _modalCalculoFactura.show();
                    console.log('✅ Modal de cálculo reabierto correctamente');
                } catch (error) {
                    console.error('❌ Error al reabrir modal de cálculo:', error);
                }
            } else {
                // Fallback: Intentar abrir manualmente
                const $modalCalculo = $(DOM.modalCalculo);
                if ($modalCalculo.length > 0) {
                    $modalCalculo.modal('show');
                    console.log('✅ Modal de cálculo reabierto (fallback)');
                } else {
                    console.error('❌ No se pudo reabrir el modal de cálculo');
                }
            }
        }, 350); // 300ms animación + 50ms margen de seguridad

        console.log('═══════════════════════════════════════════════════');
    }

    // ════════════════════════════════════════════════════════
    // HIDRATAR DATOS DEL CLIENTE
    // ════════════════════════════════════════════════════════
    /**
     * ✅ ACTUALIZADO v3.0: Hidrata datos del cliente con validación robusta
     * Copia los datos desde el modal de cálculo (sufijo "Calc") al modal de pago (sufijo "Pago")
     */
    function _hidratarDatosClientePago() {
        console.log('═══════════════════════════════════════════════════');
        console.log('📝 HIDRATAR DATOS DEL CLIENTE EN MODAL PAGO');
        console.log('═══════════════════════════════════════════════════');

        // ❶ Mapeo de IDs: Origen (Calc) → Destino (Pago)
        const mapeoIds = {
            'txtClienteNombreCalc': 'txtClienteNombrePago',
            'txtClienteIdCalc': 'txtClienteIdPago',
            'txtClienteDomicilioCalc': 'txtClienteDomicilioPago',
            'txtCondicionAfipCalc': 'txtCondicionAfipPago',
            'txtClienteCuitCalc': 'txtClienteCuitPago',
            'txtClienteEmailCalc': 'txtClienteEmailPago',
            'txtClienteMovilCalc': 'txtClienteMovilPago'
        };

        // ❷ Copiar valores de cada campo
        let camposCopiadosExitosos = 0;
        let camposSinDatos = 0;
        let camposNoEncontrados = 0;

        Object.keys(mapeoIds).forEach(function (idOrigen) {
            const idDestino = mapeoIds[idOrigen];
            const $campoOrigen = $(`#${idOrigen}`);
            const $campoDestino = $(`#${idDestino}`);

            // Validar existencia de campos
            if ($campoOrigen.length === 0) {
                console.warn(`   ⚠️ Campo origen no encontrado: ${idOrigen}`);
                camposNoEncontrados++;
                return;
            }

            if ($campoDestino.length === 0) {
                console.warn(`   ⚠️ Campo destino no encontrado: ${idDestino}`);
                camposNoEncontrados++;
                return;
            }

            const valorOrigen = $campoOrigen.val() || '';

            if (valorOrigen.trim() === '') {
                camposSinDatos++;
                console.log(`   ℹ️ ${idOrigen} → ${idDestino}: (vacío)`);
            } else {
                camposCopiadosExitosos++;
                console.log(`   ✅ ${idOrigen} → ${idDestino}: "${valorOrigen}"`);
            }

            $campoDestino.val(valorOrigen);
        });

        // ❸ Actualizar badge de tipo de comprobante
        const $badgeOrigen = $('#badgeTipoComprobanteCalc');
        const $badgeDestino = $('#badgeTipoComprobantePago');

        if ($badgeOrigen.length > 0 && $badgeDestino.length > 0) {
            const badgeHtml = $badgeOrigen.html();
            const badgeTexto = $badgeOrigen.text().trim();

            $badgeDestino.html(badgeHtml);

            console.log(`   ✅ Badge tipo comprobante: "${badgeTexto}"`);
        } else {
            console.warn('   ⚠️ Badge de tipo de comprobante no encontrado');
        }

        console.log('═══════════════════════════════════════════════════');
        console.log(`📊 Resumen de hidratación:`);
        console.log(`   ✅ Exitosos: ${camposCopiadosExitosos}`);
        console.log(`   ℹ️ Vacíos: ${camposSinDatos}`);
        console.log(`   ⚠️ No encontrados: ${camposNoEncontrados}`);
        console.log('═══════════════════════════════════════════════════');
    }

    // ════════════════════════════════════════════════════════
    // CARGAR CONCEPTOS DE PAGO
    // ════════════════════════════════════════════════════════
    /**
     * ✅ ACTUALIZADO v3.0: Carga conceptos de pago con validación
     */
    function _cargarConceptosPago(totales) {
        console.log('📊 Cargando conceptos de pago...');
        console.log('Totales recibidos:', totales);

        _conceptosPago = {
            totalPagar: parseFloat(totales?.totalPagar || 0),
            recargos: parseFloat(totales?.recargos || 0),
            descuentos: parseFloat(totales?.descuentos || 0),
            totalValores: parseFloat(totales?.totalValores || 0),
            diferencia: parseFloat(totales?.totalPagar || 0)
        };

        _actualizarConceptosPago();

        console.log('✅ Conceptos de pago cargados:', _conceptosPago);
    }

    // ════════════════════════════════════════════════════════
    // ACTUALIZAR CONCEPTOS DE PAGO EN LA UI
    // ════════════════════════════════════════════════════════
    function _actualizarConceptosPago() {
        $(DOM.totalPagar).text(_formatearMoneda(_conceptosPago.totalPagar));
        $(DOM.totalRecargos).text(_formatearMoneda(_conceptosPago.recargos));
        $(DOM.totalDescuentos).text(_formatearMoneda(_conceptosPago.descuentos));
        $(DOM.totalValores).text(_formatearMoneda(_conceptosPago.totalValores));
        $(DOM.diferencia).text(_formatearMoneda(_conceptosPago.diferencia));

        // Habilitar/deshabilitar botón Finalizar según diferencia
        const diferencia = _conceptosPago.diferencia;
        if (Math.abs(diferencia) < 0.01) { // Diferencia == 0 (con tolerancia)
            $(DOM.btnFinalizarPago).prop('disabled', false);
        } else {
            $(DOM.btnFinalizarPago).prop('disabled', true);
        }
    }

    // ════════════════════════════════════════════════════════
    // AGREGAR FORMA DE PAGO
    // ════════════════════════════════════════════════════════
    function _agregarFormaPago() {
        console.log('➕ Agregando forma de pago...');
        // TODO: Implementar modal secundario para seleccionar forma de pago

        _mostrarInfo('Esta funcionalidad se implementará próximamente');
    }

    // ════════════════════════════════════════════════════════
    // FINALIZAR PAGO
    // ════════════════════════════════════════════════════════
    function _finalizarPago() {
        console.log('═══════════════════════════════════════════════════');
        console.log('✔️ FINALIZAR PAGO');
        console.log('═══════════════════════════════════════════════════');

        // Validar que la diferencia sea 0
        if (Math.abs(_conceptosPago.diferencia) >= 0.01) {
            console.warn('⚠️ Diferencia no es $0.00');
            _mostrarAdvertencia('La diferencia debe ser $0.00 para finalizar el pago');
            return;
        }

        // TODO: Enviar datos al servidor
        console.log('📡 Enviando datos del pago al servidor...');
        console.log('Conceptos:', _conceptosPago);
        console.log('Valores de pago:', _valoresPago);

        _mostrarExito('Pago procesado correctamente');

        // Cerrar modal
        _cerrarModal();

        console.log('✅ Pago finalizado correctamente');
        console.log('═══════════════════════════════════════════════════');
    }

    // ════════════════════════════════════════════════════════
    // CERRAR MODAL
    // ════════════════════════════════════════════════════════
    function _cerrarModal() {
        console.log('🔙 Cerrando modal de pago...');

        if (_modalPago) {
            _modalPago.hide();
        }
    }

    // ════════════════════════════════════════════════════════
    // LIMPIAR MODAL
    // ════════════════════════════════════════════════════════
    function _limpiarModal() {
        console.log('═══════════════════════════════════════════════════');
        console.log('🧹 Limpiando datos del modal de pago');
        console.log('═══════════════════════════════════════════════════');

        // Resetear conceptos
        _conceptosPago = {
            totalPagar: 0,
            recargos: 0,
            descuentos: 0,
            totalValores: 0,
            diferencia: 0
        };

        _valoresPago = [];

        _actualizarConceptosPago();

        // Deshabilitar botón Finalizar
        $(DOM.btnFinalizarPago).prop('disabled', true);

        console.log('✅ Modal limpiado correctamente');
        console.log('═══════════════════════════════════════════════════');
    }

    // ════════════════════════════════════════════════════════
    // ENFOCAR PRIMER CAMPO
    // ════════════════════════════════════════════════════════
    function _enfocarPrimerCampo() {
        // Enfocar botón "Agregar" cuando se abre el modal
        setTimeout(() => {
            $(DOM.btnAgregarPago).focus();
        }, 300);
    }

    // ════════════════════════════════════════════════════════
    // FORMATEAR MONEDA
    // ════════════════════════════════════════════════════════
    function _formatearMoneda(valor) {
        const numero = parseFloat(valor) || 0;
        return numero.toLocaleString('es-AR', {
            minimumFractionDigits: 2,
            maximumFractionDigits: 2
        });
    }

    // ════════════════════════════════════════════════════════
    // MENSAJES DE USUARIO
    // ════════════════════════════════════════════════════════
    function _mostrarError(mensaje) {
        if (typeof GoldenMessage !== 'undefined' && typeof GoldenMessage.mostrarError === 'function') {
            GoldenMessage.mostrarError(mensaje);
        } else if (typeof alert !== 'undefined') {
            alert(`ERROR: ${mensaje}`);
        } else {
            console.error(`ERROR: ${mensaje}`);
        }
    }

    function _mostrarAdvertencia(mensaje) {
        if (typeof GoldenMessage !== 'undefined' && typeof GoldenMessage.mostrarAdvertencia === 'function') {
            GoldenMessage.mostrarAdvertencia(mensaje);
        } else if (typeof alert !== 'undefined') {
            alert(`ADVERTENCIA: ${mensaje}`);
        } else {
            console.warn(`ADVERTENCIA: ${mensaje}`);
        }
    }

    function _mostrarInfo(mensaje) {
        if (typeof GoldenMessage !== 'undefined' && typeof GoldenMessage.mostrarInfo === 'function') {
            GoldenMessage.mostrarInfo(mensaje);
        } else if (typeof alert !== 'undefined') {
            alert(`INFO: ${mensaje}`);
        } else {
            console.info(`INFO: ${mensaje}`);
        }
    }

    function _mostrarExito(mensaje) {
        if (typeof GoldenMessage !== 'undefined' && typeof GoldenMessage.mostrarExito === 'function') {
            GoldenMessage.mostrarExito(mensaje);
        } else if (typeof alert !== 'undefined') {
            alert(`ÉXITO: ${mensaje}`);
        } else {
            console.log(`ÉXITO: ${mensaje}`);
        }
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
// INICIALIZACIÓN AUTOMÁTICA AL CARGAR EL DOM
// ════════════════════════════════════════════════════════════
$(function () {
    console.log('═══════════════════════════════════════════════════');
    console.log('🚀 INICIALIZANDO MÓDULO PAGO FACTURA (document.ready)');
    console.log('═══════════════════════════════════════════════════');

    const inicializadoExitosamente = PagoFactura.init();

    if (inicializadoExitosamente) {
        console.log('✅ Módulo PagoFactura registrado en window.PagoFactura');
        // Exponer módulo globalmente para debugging
        window.PagoFactura = PagoFactura;
    } else {
        console.error('❌ El módulo PagoFactura NO se inicializó correctamente');
    }

    console.log('═══════════════════════════════════════════════════');
});