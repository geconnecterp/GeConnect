// GESTOR DE PAGO DE FACTURA
// ════════════════════════════════════════════════════════════
// VERSIÓN v19.6 - CORRECCIÓN: Reemplazo de Swal por AbrirMensaje
// ════════════════════════════════════════════════════════════
// CAMBIOS v19.6:
// - ✅ CORRECCIÓN CRÍTICA: Eliminado uso de Swal (no disponible)
// - ✅ Implementado AbrirMensaje() sistema propio del proyecto
// - ✅ Afectados: guardarDetalleCuponEmpresa(), guardarDetalleTransferencia(), guardarDetalleValeCompra()
//
// CAMBIOS v19.5:
// - Implementación completa de Cupones de Empresa/Mutuales (MU)
//
// CAMBIOS v19.4:
// - ✅ CORRECCIÓN QUIRÚRGICA: Agregado case 'BA' en confirmarSeleccionInstrumento()
// - Sin cambios en lógica de VA (mantiene funcionamiento correcto)
// - Sin cambios en lógica de EF (mantiene funcionamiento correcto)
// - Log específico para BA: "Abriendo modal de Transferencia Bancaria..."
//
// CAMBIOS v19.3:
// - Implementación completa de Transferencias Bancarias (BA)
// - Funciones: abrirModalDetalleTransferencia(), guardarDetalleTransferencia()
// - Modal _detalleTransferencia.cshtml mejorado con InputMask
// - Validación de fecha no futura
// - Evento de limpieza automática
// ════════════════════════════════════════════════════════════
//
// VERSIÓN ANTERIOR v18.0 - Integración de InputMask
// VERSIÓN ANTERIOR v16.1 - Mejora de rendimiento
// ════════════════════════════════════════════════════════════
// Autor: GeConnect ERP
// Última actualización: 2026-05-26
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
// ✅ NUEVO v27.0: VARIABLES GLOBALES PARA CONTEXTO DINÁMICO
// ═══════════════════════════════════════════════════════════════════

/**
 * co_tipo actual del proceso de pago (CR/CF/CD)
 * Establecido por iniciarProcesoPago()
 * @type {string}
 */
window._coTipoActual = null;

/**
 * Contexto de operación actual (VENTA/COBRANZA)
 * Establecido por iniciarProcesoPago()
 * @type {string}
 */
window._contextoOperacionActual = null;

/**
 * co_tipo del cache actual de valores MP
 * Usado para invalidar cache cuando cambia el co_tipo
 * @type {string}
 */
window._coTipoCache = null;

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
 * ✅ ACTUALIZADO v19.0: Vincula todos los eventos del módulo de pago
 * NUEVO: Agregado evento de limpieza del modal de Vale de Compra
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

    // ❸ Evento de limpieza del modal de efectivo
    $('#modalDetalleEfectivo').off('hidden.bs.modal').on('hidden.bs.modal', function () {
        console.log('🧹 LIMPIEZA AUTOMÁTICA - MODAL EFECTIVO');
        const $input = $('#txtMontoEfectivo');
        $input.val('').removeClass('is-invalid is-valid');
        $input.siblings('.invalid-feedback').remove();
        $('.invalid-feedback').remove();
        $('#lblTipoMedioPagoEfectivo').text('-');
        $('#lblInstrumentoEfectivo').text('-');
        console.log('✅ MODAL DE EFECTIVO LIMPIADO');
    });

    /**
 * ✅ ACTUALIZADO v19.8: Evento de limpieza automática del modal de Vale de Compra
 * MEJORA: Limpieza más exhaustiva y logs de debugging
 */
    $('#modalDetalleValeCompra').off('hidden.bs.modal').on('hidden.bs.modal', function () {
        console.log('═══════════════════════════════════════════════════');
        console.log('🧹 LIMPIEZA AUTOMÁTICA - MODAL VALE COMPRA v19.8');
        console.log('═══════════════════════════════════════════════════');

        // ❶ Resetear formulario
        const $input = $('#txtMontoValeCompra');
        $input
            .val('')
            .removeClass('is-invalid is-valid')
            .prop('disabled', false);

        // ❷ Remover mensajes de error
        $input.siblings('.invalid-feedback').remove();
        $('.invalid-feedback').remove();

        console.log('   ✅ Formulario limpiado');

        // ❸ Resetear labels y hidden fields
        $('#lblValeCompraSeleccionado').text('-');
        $('#lblSaldoValeCompra')
            .text('$ 0,00')
            .removeClass('text-success text-warning text-danger text-info');
        $('#hdnIdValeCompra').val('');
        $('#hdnSaldoValeCompra').val('0');

        console.log('   ✅ Labels y campos ocultos reseteados');

        // ❹ ✅ NUEVO v19.8: Limpieza exhaustiva de backdrops con delay adicional
        setTimeout(() => {
            const modalesAbiertos = $('.modal.show').length;

            console.log(`   📊 Verificación final - Modales abiertos: ${modalesAbiertos}`);

            if (modalesAbiertos === 0) {
                // Solo si NO hay otros modales abiertos
                const backdropsRestantes = $('.modal-backdrop').length;

                if (backdropsRestantes > 0) {
                    console.warn(`   ⚠️ Se encontraron ${backdropsRestantes} backdrop(s) persistente(s)`);

                    $('.modal-backdrop').remove();
                    $('body')
                        .removeClass('modal-open')
                        .css({
                            'overflow': '',
                            'padding-right': ''
                        });

                    console.log('   ✅ Backdrops persistentes limpiados forzadamente');
                } else {
                    console.log('   ✅ No hay backdrops persistentes');
                }
            } else {
                console.log(`   ℹ️ ${modalesAbiertos} modal(es) aún abierto(s) - No tocar body`);
            }
        }, 400); // ← Timeout adicional para asegurar limpieza

        console.log('═══════════════════════════════════════════════════');
        console.log('✅ LIMPIEZA AUTOMÁTICA COMPLETADA');
        console.log('═══════════════════════════════════════════════════');
    });

    // ❺ ✅ NUEVO v19.3: Evento de limpieza del modal de Transferencia
    $('#modalDetalleTransferencia').off('hidden.bs.modal').on('hidden.bs.modal', function () {
        console.log('🧹 LIMPIEZA AUTOMÁTICA - MODAL TRANSFERENCIA');

        const $form = $('#formDetalleTransferencia');
        $form[0].reset();
        $form.find('.form-control').removeClass('is-invalid is-valid');
        $('.invalid-feedback').remove();

        $('#lblInstrumentoTransferencia').text('-');
        $('#hdnBancoIdTransferencia').val('');

        const $backdropTransf = $('.modal-backdrop[data-modal="transferencia"]');
        if ($backdropTransf.length > 0) {
            $backdropTransf.remove();
        }

        console.log('✅ MODAL DE TRANSFERENCIA LIMPIADO');
    });

    /**
 * ✅ ACTUALIZADO v19.7: Evento de limpieza automática del modal de Cupón Empresa
 * CAMBIO: Eliminada lógica de backdrops personalizados (Bootstrap lo gestiona automáticamente)
 */
    $('#modalDetalleCuponEmpresa').off('hidden.bs.modal').on('hidden.bs.modal', function () {
        console.log('🧹 LIMPIEZA AUTOMÁTICA - MODAL CUPÓN EMPRESA v19.7');

        // ❶ Resetear formulario
        const $form = $('#formDetalleCuponEmpresa');
        $form[0].reset();
        $form.find('.form-control').removeClass('is-invalid is-valid');
        $('.invalid-feedback').remove();

        console.log('   ✅ Formulario limpiado');

        // �② Resetear labels y hidden fields
        $('#lblEmpresaSeleccionada').text('-');
        $('#hdnEmpresaIdCupon').val('');

        console.log('   ✅ Labels reseteados');

        // ❸ ✅ ELIMINADO: Ya NO es necesario limpiar backdrops manualmente
        // Bootstrap gestiona sus propios backdrops automáticamente
        // const $backdropCupon = $('.modal-backdrop[data-modal="cuponempresa"]');
        // if ($backdropCupon.length > 0) {
        //     $backdropCupon.remove();
        // }

        // ❹ Verificar otros modales (por seguridad)
        setTimeout(() => {
            const modalesAbiertos = $('.modal.show').length;

            if (modalesAbiertos === 0) {
                // Solo si NO hay otros modales abiertos
                const backdropsHuerfanos = $('.modal-backdrop').length;

                if (backdropsHuerfanos > 0) {
                    console.warn(`   ⚠️ Se encontraron ${backdropsHuerfanos} backdrop(s) huérfano(s)`);
                    $('.modal-backdrop').remove();
                    $('body').removeClass('modal-open').css('overflow', '');
                    console.log('   ✅ Backdrops huérfanos limpiados');
                } else {
                    console.log('   ✅ No hay backdrops huérfanos');
                }
            } else {
                console.log(`   ℹ️ ${modalesAbiertos} modal(es) aún abierto(s) - No tocar body`);
            }
        }, 350); // Esperar animación de Bootstrap

        console.log('✅ MODAL DE CUPÓN EMPRESA LIMPIADO');
    });

    /**
     * ✅ ACTUALIZADO v20.3: Evento de limpieza automática del modal de cheque
     * ELIMINADO: Ya no limpia campo de plaza
     */
    $('#modalDetalleCheque').off('hidden.bs.modal').on('hidden.bs.modal', function () {
        console.log('🧹 LIMPIEZA AUTOMÁTICA - MODAL CHEQUE');

        const $form = $('#formDetalleCheque');
        $form[0].reset();
        $form.find('.form-control, .form-select').removeClass('is-invalid is-valid');
        $('.invalid-feedback').remove();

        $('#lblTipoMedioPagoCheque').text('-');
        $('#lblInstrumentoCheque').text('-');
        $('#selectBancoCheque').val('').prop('disabled', true);

        // ❌ ELIMINADO: Limpiar campo plaza (ya no existe)

        console.log('✅ MODAL DE CHEQUE LIMPIADO');
    });

    /**
 * ✅ NUEVO v21.0: Eventos de selección automática para modales de instrumentos secundarios
 * Se ejecutan cuando el modal termina de mostrarse (después de la animación)
 */

    // ═══════════════════════════════════════════════════════════════════
    // Modal: Transferencias Bancarias
    // ═══════════════════════════════════════════════════════════════════
    $('#modalInstrumentosTransferencia').on('shown.bs.modal', function () {
        console.log('🔓 Modal Transferencia mostrado - Seleccionando primer banco...');

        seleccionarPrimerItemAutomatico({
            contenedorId: '#listaInstrumentosTransferencia',
            itemClass: '.instrumento-transferencia-item',
            btnConfirmarId: '#btnConfirmarBancoTransferencia',
            tipoModal: 'transferencia'
        });
    });

    // ═══════════════════════════════════════════════════════════════════
    // Modal: Vales de Compra
    // ═══════════════════════════════════════════════════════════════════
    $('#modalInstrumentosValeCompra').on('shown.bs.modal', function () {
        console.log('🔓 Modal Vale Compra mostrado - Seleccionando primer vale...');

        seleccionarPrimerItemAutomatico({
            contenedorId: '#listaInstrumentosValeCompra',
            itemClass: '.instrumento-vale-item',
            btnConfirmarId: '#btnConfirmarValeCompra',
            tipoModal: 'vale'
        });
    });

    // ═══════════════════════════════════════════════════════════════════
    // Modal: Cupones/Órdenes de Empresa
    // ═══════════════════════════════════════════════════════════════════
    $('#modalInstrumentosCuponEmpresa').on('shown.bs.modal', function () {
        console.log('🔓 Modal Cupón Empresa mostrado - Seleccionando primera empresa...');

        seleccionarPrimerItemAutomatico({
            contenedorId: '#listaInstrumentosCupon',
            itemClass: '.instrumento-cupon-item',
            btnConfirmarId: '#btnConfirmarCuponEmpresa',
            tipoModal: 'cupon'
        });
    });

    // ═══════════════════════════════════════════════════════════
    // ✅ NUEVO v21.2: LIMPIEZA DE EVENTOS DE TECLADO
    // ═══════════════════════════════════════════════════════════

    /**
     * Evento que se dispara cuando el modal de tipo medio de pago se cierra completamente
     * Asegura limpieza de eventos de navegación con teclado
     */
    $('#modalTipoMedioPago').off('hidden.bs.modal.limpiezaTeclado').on('hidden.bs.modal.limpiezaTeclado', function () {
        console.log('🔒 Modal Tipo Medio de Pago cerrado - Limpiando eventos de teclado');
        limpiarEventosTipoMedioPago();
    });

    // ═══════════════════════════════════════════════════════════
    // ✅ NUEVO v21.3: NAVEGACIÓN CON TECLADO - MODAL DE INSTRUMENTOS GENÉRICO
    // ═══════════════════════════════════════════════════════════

    /**
     * Modal de Instrumentos Genérico (Monedas: Pesos, U$S, etc.)
     * 
     * Evento 'shown.bs.modal':
     * - Se dispara cuando el modal termina de mostrarse (después de la animación)
     * - Habilita navegación con teclado (↑↓ Enter Esc)
     * 
     * Evento 'hidden.bs.modal':
     * - Se dispara cuando el modal se cierra completamente
     * - Limpia eventos de teclado para evitar memory leaks
     */
    $('#modalInstrumentos')
        .off('shown.bs.modal.navTeclado')
        .on('shown.bs.modal.navTeclado', function () {
            console.log('🔓 Modal Instrumentos mostrado - Habilitando navegación con teclado...');

            habilitarNavegacionTecladoInstrumentos({
                modalId: '#modalInstrumentos',
                contenedorId: '#listaInstrumentos',
                itemClass: '.instrumento-item',
                btnConfirmarId: '#btnConfirmarInstrumento',
                onConfirmar: confirmarSeleccionInstrumento // ← Función existente (línea ~2340)
            });
        });

    $('#modalInstrumentos')
        .off('hidden.bs.modal.navTeclado')
        .on('hidden.bs.modal.navTeclado', function () {
            console.log('🔒 Modal Instrumentos cerrado - Limpiando eventos de teclado...');

            limpiarNavegacionTecladoInstrumentos('#modalInstrumentos');
        });

    console.log('✅ Navegación con teclado configurada para Modal Instrumentos Genérico');

    // ═══════════════════════════════════════════════════════════
    // ✅ NUEVO v21.3: NAVEGACIÓN CON TECLADO - MODAL DE TRANSFERENCIAS BANCARIAS
    // ═══════════════════════════════════════════════════════════

    /**
     * Modal de Instrumentos - Transferencias Bancarias
     * 
     * CONTEXTO:
     * - Se abre al seleccionar tipo medio de pago "BA" (Transferencias)
     * - Usuario selecciona el banco destino (Banco San Juan, Galicia, Macro)
     * - Después de seleccionar banco, se abre modal de detalle de transferencia
     * 
     * Evento 'shown.bs.modal':
     * - Se dispara cuando el modal termina de mostrarse
     * - Habilita navegación con teclado (↑↓ Enter Esc)
     * - Selecciona automáticamente el primer banco
     * 
     * Evento 'hidden.bs.modal':
     * - Se dispara cuando el modal se cierra completamente
     * - Limpia eventos de teclado para evitar memory leaks
     */
    $('#modalInstrumentosTransferencia')
        .off('shown.bs.modal.navTeclado')
        .on('shown.bs.modal.navTeclado', function () {
            console.log('🔓 Modal Transferencias mostrado - Habilitando navegación con teclado...');

            habilitarNavegacionTecladoInstrumentos({
                modalId: '#modalInstrumentosTransferencia',
                contenedorId: '#listaInstrumentosTransferencia',
                itemClass: '.instrumento-transferencia-item',
                btnConfirmarId: '#btnConfirmarBancoTransferencia',
                onConfirmar: function () {
                    // ✅ Callback: Simular click en botón confirmar
                    console.log('   ⏎ Enter presionado - Confirmando banco seleccionado...');
                    $('#btnConfirmarBancoTransferencia').trigger('click');
                }
            });
        });

    $('#modalInstrumentosTransferencia')
        .off('hidden.bs.modal.navTeclado')
        .on('hidden.bs.modal.navTeclado', function () {
            console.log('🔒 Modal Transferencias cerrado - Limpiando eventos de teclado...');

            limpiarNavegacionTecladoInstrumentos('#modalInstrumentosTransferencia');
        });

    console.log('✅ Navegación con teclado configurada para Modal Transferencias Bancarias');

    // ═══════════════════════════════════════════════════════════
    // ✅ NUEVO v21.3: NAVEGACIÓN CON TECLADO - MODAL DE VALES DE COMPRA
    // ═══════════════════════════════════════════════════════════

    /**
     * Modal de Instrumentos - Vales de Compra
     * 
     * CONTEXTO:
     * - Se abre al seleccionar tipo medio de pago "VA" (Vales de Compra)
     * - Usuario selecciona el vale a utilizar (12 vales disponibles)
     * - Vales: AMOEM, AMSESA, EL ZONDA, EMICAR, EMPL COMERCIO, ENERGIA SJ, 
     *          FABREGAS, GREMIO ATE, GREMIO ALIMEN, GREMIO ATSA, INDIVIDRIO, INTA
     * 
     * NOTA: Lista extensa (12 ítems) → Requiere scroll automático
     * 
     * Evento 'shown.bs.modal':
     * - Se dispara cuando el modal termina de mostrarse
     * - Habilita navegación con teclado (↑↓ Enter Esc)
     * - Selecciona automáticamente el primer vale
     * - Scroll automático al navegar si el vale está fuera de vista
     * 
     * Evento 'hidden.bs.modal':
     * - Se dispara cuando el modal se cierra completamente
     * - Limpia eventos de teclado para evitar memory leaks
     */
    $('#modalInstrumentosValeCompra')
        .off('shown.bs.modal.navTeclado')
        .on('shown.bs.modal.navTeclado', function () {
            console.log('🔓 Modal Vales de Compra mostrado - Habilitando navegación con teclado...');
            console.log('   📊 Total vales: 12 (scroll automático habilitado)');

            habilitarNavegacionTecladoInstrumentos({
                modalId: '#modalInstrumentosValeCompra',
                contenedorId: '#listaInstrumentosValeCompra',
                itemClass: '.instrumento-vale-item',
                btnConfirmarId: '#btnConfirmarValeCompra',
                onConfirmar: function () {
                    // ✅ Callback: Simular click en botón confirmar
                    console.log('   ⏎ Enter presionado - Confirmando vale seleccionado...');
                    $('#btnConfirmarValeCompra').trigger('click');
                }
            });

            console.log('✅ Navegación habilitada - Usar ↑↓ para navegar entre 12 vales');
        });

    $('#modalInstrumentosValeCompra')
        .off('hidden.bs.modal.navTeclado')
        .on('hidden.bs.modal.navTeclado', function () {
            console.log('🔒 Modal Vales de Compra cerrado - Limpiando eventos de teclado...');

            limpiarNavegacionTecladoInstrumentos('#modalInstrumentosValeCompra');
        });

    console.log('✅ Navegación con teclado configurada para Modal Vales de Compra (12 ítems)');

    // ═══════════════════════════════════════════════════════════
    // ✅ NUEVO v21.3: NAVEGACIÓN CON TECLADO - MODAL DE CUPONES/ÓRDENES DE EMPRESA
    // ═══════════════════════════════════════════════════════════

    /**
     * Modal de Instrumentos - Cupones/Órdenes de Empresa (Mutuales)
     * 
     * CONTEXTO:
     * - Se abre al seleccionar tipo medio de pago "MU" (Mutuales)
     * - Usuario selecciona la mutual/empresa emisora (12 empresas disponibles)
     * - Empresas: MUTUAL UPCN, CHICONI, ENAV, SEP, TCA 2014, ORDEN DE COMPRA,
     *             CARMAR, HUARPE, TRIELEC, VFG, ENTRETELAS, TERAPIA POLIVALENTE
     * 
     * NOTA TÉCNICA:
     * - Lista extensa (12 ítems) → Requiere scroll automático
     * - Modal con clase 'modal-dialog-elevated' → z-index puede requerir ajuste
     * - Campos auto-completados desde cliente actual (Titular, CUIT)
     * 
     * Evento 'shown.bs.modal':
     * - Se dispara cuando el modal termina de mostrarse
     * - Habilita navegación con teclado (↑↓ Enter Esc)
     * - Selecciona automáticamente la primera empresa/mutual
     * - Scroll automático al navegar si el ítem está fuera de vista
     * 
     * Evento 'hidden.bs.modal':
     * - Se dispara cuando el modal se cierra completamente
     * - Limpia eventos de teclado para evitar memory leaks
     */
    $('#modalInstrumentosCuponEmpresa')
        .off('shown.bs.modal.navTeclado')
        .on('shown.bs.modal.navTeclado', function () {
            console.log('🔓 Modal Cupones/Empresa mostrado - Habilitando navegación con teclado...');
            console.log('   📊 Total empresas/mutuales: 12 (scroll automático habilitado)');
            console.log('   ⚠️ Modal elevado (z-index alto) - Validando compatibilidad...');

            habilitarNavegacionTecladoInstrumentos({
                modalId: '#modalInstrumentosCuponEmpresa',
                contenedorId: '#listaInstrumentosCupon',
                itemClass: '.instrumento-cupon-item',
                btnConfirmarId: '#btnConfirmarCuponEmpresa',
                onConfirmar: function () {
                    // ✅ Callback: Simular click en botón confirmar
                    console.log('   ⏎ Enter presionado - Confirmando empresa/mutual seleccionada...');
                    $('#btnConfirmarCuponEmpresa').trigger('click');
                }
            });

            // ✅ Validar z-index del modal (modal-dialog-elevated)
            setTimeout(() => {
                const zIndexModal = parseInt($('#modalInstrumentosCuponEmpresa').css('z-index'));
                console.log(`   🔍 Z-index del modal: ${zIndexModal}`);

                if (zIndexModal < 5000) {
                    console.warn('   ⚠️ Z-index bajo detectado - Aplicando corrección...');
                    $('#modalInstrumentosCuponEmpresa').css('z-index', '5100');
                    $('.modal-backdrop').last().css('z-index', '5099');
                    console.log('   ✅ Z-index corregido: modal=5100, backdrop=5099');
                }
            }, 100);

            console.log('✅ Navegación habilitada - Usar ↑↓ para navegar entre 12 empresas/mutuales');
        });

    $('#modalInstrumentosCuponEmpresa')
        .off('hidden.bs.modal.navTeclado')
        .on('hidden.bs.modal.navTeclado', function () {
            console.log('🔒 Modal Cupones/Empresa cerrado - Limpiando eventos de teclado...');

            limpiarNavegacionTecladoInstrumentos('#modalInstrumentosCuponEmpresa');
        });

    console.log('✅ Navegación con teclado configurada para Modal Cupones/Órdenes de Empresa (12 ítems)');
}

// ═══════════════════════════════════════════════════════════════════
// SECCIÓN 3: FUNCIONES PRINCIPALES DE VISTA
// ═══════════════════════════════════════════════════════════════════

/**
 * ✅ NUEVO v27.0: Función genérica para iniciar el proceso de pago
 * Centraliza la lógica de apertura del modal de pago para MÚLTIPLES contextos
 * 
 * CONTEXTOS SOPORTADOS:
 * - VENTA: Facturación normal de productos (co_tipo: CR/CF)
 * - COBRANZA: Cobranza de facturas diferidas (co_tipo: CD)
 * 
 * PARÁMETROS:
 * @param {Object} config - Objeto de configuración
 * @param {number} config.totalPagar - Monto total a cobrar (requerido)
 * @param {string} config.co_tipo - Tipo de comprobante (CR/CF/CD) (requerido)
 * @param {string} config.puntoVenta - Descripción del punto de venta (opcional, default: 'GECO PV')
 * @param {string} config.tituloModal - Título del modal (opcional, default: 'Formas de Pago Ingresadas')
 * @param {string} config.contextoOperacion - Contexto: 'VENTA' o 'COBRANZA' (opcional, default: 'VENTA')
 * 
 * @returns {boolean} - true si se abrió correctamente, false en caso de error
 * 
 * EJEMPLO DE USO (VENTA):
 * ```javascript
 * iniciarProcesoPago({
 *     totalPagar: 1500.50,
 *     co_tipo: 'CF',
 *     puntoVenta: 'GECO PV',
 *     tituloModal: 'Pago de Factura',
 *     contextoOperacion: 'VENTA'
 * });
 * ```
 * 
 * EJEMPLO DE USO (COBRANZA):
 * ```javascript
 * iniciarProcesoPago({
 *     totalPagar: 2450.00,
 *     co_tipo: 'CD',
 *     puntoVenta: 'GECO PD',
 *     tituloModal: 'Cobranza Diferida',
 *     contextoOperacion: 'COBRANZA'
 * });
 * ```
 */

function iniciarProcesoPago(config) {
    console.log('═══════════════════════════════════════════════════');
    console.log('🎬 INICIAR PROCESO DE PAGO GENÉRICO v27.0');
    console.log('═══════════════════════════════════════════════════');
    console.log('   📋 Configuración recibida:', config);

    // ═══════════════════════════════════════════════════════════════════
    // ❶ VALIDACIONES DE PARÁMETROS
    // ═══════════════════════════════════════════════════════════════════

    if (!config || typeof config !== 'object') {
        console.error('❌ CRÍTICO: Configuración inválida o ausente');
        console.error('   Se esperaba un objeto con parámetros de configuración');
        mostrarMensajeError('Error en la configuración del módulo de pago.\n\nPor favor, recargue la página.');
        return false;
    }

    // ❷ Validar totalPagar (OBLIGATORIO)
    const totalPagar = parseFloat(config.totalPagar);

    if (isNaN(totalPagar) || totalPagar <= 0) {
        console.error('❌ ERROR: totalPagar inválido');
        console.error(`   Valor recibido: ${config.totalPagar}`);
        console.error(`   Valor parseado: ${totalPagar}`);
        mostrarMensajeError('El monto total a pagar debe ser mayor a cero.');
        return false;
    }

    console.log(`   ✅ Total a pagar validado: ${formatearMoneda(totalPagar)}`);

    // ❸ Validar co_tipo (OBLIGATORIO)
    const coTipo = (config.co_tipo || '').trim().toUpperCase();

    if (!coTipo || coTipo === '') {
        console.error('❌ ERROR: co_tipo no especificado');
        mostrarMensajeError('Tipo de comprobante no especificado.\n\nPor favor, recargue la página.');
        return false;
    }

    // ✅ Lista blanca de co_tipo permitidos
    const coTiposPermitidos = ['CR', 'CF', 'CD'];

    if (!coTiposPermitidos.includes(coTipo)) {
        console.warn(`⚠️ ADVERTENCIA: co_tipo "${coTipo}" no está en lista blanca`);
        console.warn(`   Tipos permitidos: ${coTiposPermitidos.join(', ')}`);
        console.warn('   ⚠️ Continuando de todos modos (para compatibilidad futura)');
    }

    console.log(`   ✅ Tipo de comprobante: ${coTipo}`);

    // ❹ Extraer parámetros opcionales con valores por defecto
    const puntoVenta = (config.puntoVenta || 'GECO PV').trim();
    const tituloModal = (config.tituloModal || 'Formas de Pago Ingresadas').trim();
    const contextoOperacion = (config.contextoOperacion || 'VENTA').trim().toUpperCase();

    console.log(`   📍 Punto de Venta: ${puntoVenta}`);
    console.log(`   📝 Título del Modal: "${tituloModal}"`);
    console.log(`   🔖 Contexto de Operación: ${contextoOperacion}`);

    // ═══════════════════════════════════════════════════════════════════
    // ❺ VALIDAR DISPONIBILIDAD DE FUNCIÓN abrirModalPago
    // ═══════════════════════════════════════════════════════════════════

    if (typeof abrirModalPago !== 'function') {
        console.error('═══════════════════════════════════════════════════');
        console.error('❌ CRÍTICO: Función abrirModalPago() NO está disponible');
        console.error('═══════════════════════════════════════════════════');
        console.error('   Diagnóstico:');
        console.error('   1. Verificar que el archivo pagoFactura.js esté cargado completamente');
        console.error('   2. Verificar que no haya errores de sintaxis en pagoFactura.js');
        console.error('   3. Revisar consola del navegador para errores de carga');
        console.error('═══════════════════════════════════════════════════');

        mostrarMensajeError('El módulo de pago no está disponible.\n\nPor favor, recargue la página e intente nuevamente.');
        return false;
    }

    console.log('   ✅ Función abrirModalPago() disponible');

    // ═══════════════════════════════════════════════════════════════════
    // ❻ PREPARAR DATOS PARA EL MODAL DE PAGO
    // ═══════════════════════════════════════════════════════════════════

    console.log('═══════════════════════════════════════════════════');
    console.log('📦 PREPARANDO DATOS PARA EL MODAL DE PAGO');

    const datosPago = {
        totales: {
            totalPagar: totalPagar,
            recargos: 0,        // ← Por ahora siempre 0 (puede extenderse en el futuro)
            descuentos: 0,      // ← Por ahora siempre 0 (puede extenderse en el futuro)
            totalValores: 0     // ← Se calculará dinámicamente en el modal
        },
        puntoVenta: puntoVenta,
        coTipo: coTipo,                     // ✅ NUEVO v27.0: Pasamos co_tipo al modal
        tituloModal: tituloModal,           // ✅ NUEVO v27.0: Título dinámico
        contextoOperacion: contextoOperacion // ✅ NUEVO v27.0: Contexto de operación
    };

    console.log('   ✅ Estructura de datos construida:');
    console.log('      totales.totalPagar:', formatearMoneda(datosPago.totales.totalPagar));
    console.log('      totales.recargos:', formatearMoneda(datosPago.totales.recargos));
    console.log('      totales.descuentos:', formatearMoneda(datosPago.totales.descuentos));
    console.log('      puntoVenta:', datosPago.puntoVenta);
    console.log('      coTipo:', datosPago.coTipo);
    console.log('      tituloModal:', datosPago.tituloModal);
    console.log('      contextoOperacion:', datosPago.contextoOperacion);
    console.log('═══════════════════════════════════════════════════');

    // ═══════════════════════════════════════════════════════════════════
    // ❼ INVOCAR abrirModalPago CON MANEJO DE ERRORES
    // ═══════════════════════════════════════════════════════════════════

    console.log('🔓 Invocando abrirModalPago()...');

    try {
        const resultado = abrirModalPago(datosPago);

        if (resultado === false) {
            console.error('❌ abrirModalPago() retornó false');
            console.error('   El modal no pudo abrirse correctamente');

            mostrarMensajeError('Error al abrir el modal de pago.\n\nRevise la consola para más detalles.');
            return false;
        }

        console.log('═══════════════════════════════════════════════════');
        console.log('✅ PROCESO DE PAGO INICIADO CORRECTAMENTE');
        console.log(`   Contexto: ${contextoOperacion}`);
        console.log(`   Monto: ${formatearMoneda(totalPagar)}`);
        console.log(`   Tipo: ${coTipo}`);
        console.log('═══════════════════════════════════════════════════');

        return true;

    } catch (error) {
        console.error('═══════════════════════════════════════════════════');
        console.error('❌ EXCEPCIÓN AL ABRIR MODAL DE PAGO');
        console.error('═══════════════════════════════════════════════════');
        console.error('   Error:', error);
        console.error('   Mensaje:', error.message);
        console.error('   Stack:', error.stack);
        console.error('═══════════════════════════════════════════════════');

        mostrarMensajeError(`Error al abrir el modal de pago:\n\n${error.message}\n\nPor favor, recargue la página e intente nuevamente.`);
        return false;
    }
}


/**
 * ✅ ACTUALIZADO v20.6: Abre el modal de pago con los datos de la factura
 * NUEVO: Apertura automática del modal de agregar formas de pago
 * 
 * MEJORA UX v20.6:
 * - Al abrir el modal de pago, automáticamente abre el modal de tipo medio de pago
 * - Elimina un click innecesario para el cajero
 * - Primera acción siempre es agregar una forma de pago
 * 
 * ✅ ACTUALIZADO v27.0: Soporte para contextos múltiples
 * - Acepta parámetros adicionales: coTipo, tituloModal, contextoOperacion
 * - Actualiza dinámicamente el título del modal según el contexto
 * 
 * @param {Object} datosFactura - Objeto con totales y datos del cliente
 */
function abrirModalPago(datosFactura) {
    console.log('═══════════════════════════════════════════════════');
    console.log('🔓 ABRIR MODAL DE PAGO v27.0');
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

        // ═══════════════════════════════════════════════════════════
        // ✅ NUEVO v27.0: ACTUALIZAR TÍTULO DEL MODAL DINÁMICAMENTE
        // ═══════════════════════════════════════════════════════════

        const tituloModal = datosFactura?.tituloModal || 'Formas de Pago Ingresadas';
        console.log(`   📝 Actualizando título del modal: "${tituloModal}"`);
        $('#tituloFormasPago').text(tituloModal);

        // ═══════════════════════════════════════════════════════════
        // ✅ NUEVO v27.0: GUARDAR co_tipo Y contextoOperacion EN VARIABLES GLOBALES
        // ═══════════════════════════════════════════════════════════

        window._coTipoActual = datosFactura?.coTipo || 'CF';
        window._contextoOperacionActual = datosFactura?.contextoOperacion || 'VENTA';

        console.log(`   🔖 co_tipo guardado: ${window._coTipoActual}`);
        console.log(`   🔖 contextoOperacion guardado: ${window._contextoOperacionActual}`);

        // ❻ Mostrar modal
        modalPagoInstance.show();

        // ❼ Ajustar z-index
        setTimeout(() => {
            $('#modalPago').css('z-index', '1060');
            $('.modal-backdrop').last().css('z-index', '1059');
        }, 100);

        // ═══════════════════════════════════════════════════════════
        // ✅ NUEVO v20.6: APERTURA AUTOMÁTICA DE MODAL AGREGAR
        // ═══════════════════════════════════════════════════════════

        console.log('═══════════════════════════════════════════════════');
        console.log('🚀 INICIANDO APERTURA AUTOMÁTICA DE AGREGAR v20.6');
        console.log('═══════════════════════════════════════════════════');

        // ❽ Esperar a que el modal de pago esté completamente visible
        setTimeout(() => {
            console.log('⏳ Modal de pago visible - Abriendo modal de agregar...');

            // ❾ Abrir modal de tipo medio de pago automáticamente
            abrirModalTipoMedioPago();

            console.log('✅ Modal de agregar formas de pago abierto automáticamente');
            console.log('   Beneficio UX: Cajero ahorra 1 click');
            console.log('   Primera acción necesaria: Agregar forma de pago');

        }, 400); // ← Timing crítico: Esperar a que modal de pago termine animación

        console.log('✅ Modal de pago abierto correctamente');
        return true;

    } catch (error) {
        console.error('❌ Error al abrir modal de pago:', error);
        mostrarMensajeError(`No se pudo abrir el modal de pago.\n\n${error.message}`);
        return false;
    }
}

/**
 * ✅ ACTUALIZADO v21.4: Agrega una forma de pago
 * CORRECCIÓN CRÍTICA: Valida diferencia <= 0 para prevenir agregado incorrecto
 * 
 * CAMBIOS v21.4:
 * - Agregada validación de diferencia <= 0 (exacto o vuelto)
 * - Mensaje informativo diferenciado según el caso
 * - Previene apertura del modal cuando no se deben agregar más valores
 * 
 * REGLA DE NEGOCIO:
 * - Solo permite agregar valores si diferencia > 0 (falta pagar)
 * - Bloquea si diferencia = 0 (pago exacto)
 * - Bloquea si diferencia < 0 (hay vuelto)
 */
function agregarFormaPago() {
    console.log('═══════════════════════════════════════════════════');
    console.log('➕ AGREGAR FORMA DE PAGO v21.4');
    console.log('═══════════════════════════════════════════════════');

    // ❶ Obtener diferencia actual
    const diferencia = conceptosPago.diferencia || 0;

    console.log(`   Diferencia actual: ${formatearMoneda(diferencia)}`);
    console.log(`   Total valores actuales: ${valoresPago.length}`);

    // ❷ ✅ NUEVO v21.4: Validar diferencia <= 0 (exacto o vuelto)
    if (diferencia <= 0) {
        console.warn('⚠️ VALIDACIÓN FALLÓ: La diferencia es <= 0');
        console.warn('   → No se puede agregar más valores');

        let mensajeUsuario = '';
        let tipoMensaje = 'info';

        if (Math.abs(diferencia) < 0.01) {
            // ═══════════════════════════════════════════════════
            // CASO 1: Diferencia exacta ($0.00)
            // ═══════════════════════════════════════════════════
            console.log('   📊 Caso: Diferencia exacta ($0.00)');
            mensajeUsuario = 'El pago ya está completo. No es necesario agregar más valores.';
            tipoMensaje = 'info';
        } else {
            // ═══════════════════════════════════════════════════
            // CASO 2: Diferencia negativa (vuelto)
            // ═══════════════════════════════════════════════════
            const vuelto = Math.abs(diferencia);
            console.log(`   📊 Caso: Vuelto de ${formatearMoneda(vuelto)}`);

            mensajeUsuario = `El pago tiene un vuelto de ${formatearMoneda(vuelto)}. No se pueden agregar más valores.`;
            tipoMensaje = 'warning';
        }

        // ❸ Mostrar notificación al usuario
        if (typeof toastr !== 'undefined') {
            toastr[tipoMensaje](mensajeUsuario, 'Información');
        } else {
            alert(mensajeUsuario);
        }

        console.log('❌ Agregado de valores BLOQUEADO');
        console.log('═══════════════════════════════════════════════════');

        return; // ← Salir de la función
    }

    // ❹ Si llegó aquí, diferencia > 0 → Se puede agregar
    console.log('✅ Diferencia > 0 - Se puede agregar valores');
    console.log(`   Monto faltante: ${formatearMoneda(diferencia)}`);

    // ❺ Abrir modal de tipo medio de pago
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
 * ✅ ACTUALIZADO v27.0: Carga los tipos de medio de pago desde el servidor
 * CAMBIO CRÍTICO v27.0: Usa co_tipo dinámico desde variable global
 * 
 * CAMBIOS v27.0:
 * - ✅ Usa window._coTipoActual (establecido por iniciarProcesoPago)
 * - ✅ Fallback a lógica CR/CF si la variable no existe (compatibilidad)
 * - ✅ Soporte completo para co_tipo: CR, CF, CD (y futuros)
 * - ✅ Cache se invalida si co_tipo cambia entre llamadas
 * 
 * CAMBIOS v16.1:
 * - NO depende de variables globales de sesión
 * - El servidor maneja automáticamente los datos de sesión
 * 
 * @returns {Promise<Array>} - Array de valores MP
 */
function cargarValoresMP() {
    console.log('═══════════════════════════════════════════════════');
    console.log('📡 CARGAR VALORES MP v27.0 (CO_TIPO DINÁMICO)');
    console.log('═══════════════════════════════════════════════════');

    // ═══════════════════════════════════════════════════════════════════
    // ❶ OBTENER co_tipo DINÁMICAMENTE
    // ═══════════════════════════════════════════════════════════════════

    let coTipo = null;

    // ✅ NUEVO v27.0: Intentar obtener de variable global (establecida por iniciarProcesoPago)
    if (typeof window._coTipoActual !== 'undefined' && window._coTipoActual) {
        coTipo = window._coTipoActual.trim().toUpperCase();
        console.log('✅ co_tipo obtenido de variable global (iniciarProcesoPago)');
        console.log(`   Valor: ${coTipo}`);
    } else {
        // ⚠️ FALLBACK: Calcular co_tipo con lógica anterior (compatibilidad con código legacy)
        console.warn('⚠️ Variable global window._coTipoActual NO definida');
        console.warn('   Aplicando fallback: Cálculo basado en cta_id');

        const ctaId = $('#txtClienteIdPago').val() || '';
        coTipo = ctaId && ctaId !== 'N/A' && ctaId.trim() !== '' ? 'CR' : 'CF';

        console.log(`   cta_id: ${ctaId || 'N/A'}`);
        console.log(`   co_tipo calculado (fallback): ${coTipo}`);
    }

    console.log('═══════════════════════════════════════════════════');

    // ═══════════════════════════════════════════════════════════════════
    // ❷ VERIFICAR CACHE (INVALIDAR SI co_tipo CAMBIÓ)
    // ═══════════════════════════════════════════════════════════════════

    // ✅ NUEVO v27.0: Cache por co_tipo (evita problemas al cambiar contexto)
    if (valoresMPCargados && valoresMPCache !== null && window._coTipoCache === coTipo) {
        console.log('✅ Valores MP encontrados en cache');
        console.log(`   co_tipo del cache: ${window._coTipoCache}`);
        console.log(`   co_tipo actual: ${coTipo}`);
        console.log('   ℹ️ Cache válido - Reutilizando datos');
        return $.Deferred().resolve(valoresMPCache).promise();
    }

    // ⚠️ Si co_tipo cambió, invalidar cache
    if (valoresMPCargados && window._coTipoCache && window._coTipoCache !== coTipo) {
        console.warn('⚠️ Cache invalidado - co_tipo cambió');
        console.warn(`   co_tipo anterior: ${window._coTipoCache}`);
        console.warn(`   co_tipo actual: ${coTipo}`);
        valoresMPCargados = false;
        valoresMPCache = null;
    }

    // ═══════════════════════════════════════════════════════════════════
    // ❸ OBTENER DATOS DEL CLIENTE
    // ═══════════════════════════════════════════════════════════════════

    const ctaId = $('#txtClienteIdPago').val() || '';

    console.log('📋 Datos de la consulta:');
    console.log(`   cta_id: ${ctaId || 'N/A'}`);
    console.log(`   co_tipo: ${coTipo}`);

    // ✅ Descripción del co_tipo para logs
    const descripcionCoTipo = {
        'CR': 'Cliente Registrado',
        'CF': 'Consumidor Final',
        'CD': 'Cobranza Diferida',
        'CC': 'Cuenta Corriente' // ← Soporte futuro
    };

    const descripcion = descripcionCoTipo[coTipo] || `Tipo personalizado (${coTipo})`;
    console.log(`   Descripción: ${descripcion}`);

    // ═══════════════════════════════════════════════════════════════════
    // ❹ PREPARAR REQUEST DATA
    // ═══════════════════════════════════════════════════════════════════

    // ✅ CAMBIO CRÍTICO: NO enviar adm_id desde el frontend
    // El servidor lo obtendrá automáticamente desde la sesión
    const requestData = {
        co_tipo: coTipo,
        cta_id: ctaId
        // ❌ NO INCLUIR: adm_id (el servidor lo obtiene de la sesión)
    };

    console.log('   ✅ adm_id: Gestionado automáticamente por el servidor');
    console.log('═══════════════════════════════════════════════════');

    // ═══════════════════════════════════════════════════════════════════
    // ❺ LLAMADA AJAX
    // ═══════════════════════════════════════════════════════════════════

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
                window._coTipoCache = coTipo; // ✅ NUEVO v27.0: Guardar co_tipo del cache
                return [];
            }

            const datos = response.datos || response.data || [];

            if (!Array.isArray(datos)) {
                console.warn('⚠️ Datos no son un array');
                valoresMPCache = [];
                valoresMPCargados = true;
                window._coTipoCache = coTipo; // ✅ NUEVO v27.0
                return [];
            }

            console.log(`✅ ${datos.length} tipos de medio de pago recibidos`);

            // ✅ NUEVO v27.0: Guardar co_tipo junto con el cache
            valoresMPCache = datos;
            valoresMPCargados = true;
            window._coTipoCache = coTipo;

            console.log(`   📦 Cache actualizado para co_tipo: ${coTipo}`);

            return datos;
        })
        .fail(function (jqXHR, textStatus, errorThrown) {
            console.error('❌ ERROR AL CARGAR VALORES MP');
            console.error('   Status:', textStatus);
            console.error('   Error:', errorThrown);

            valoresMPCache = [];
            valoresMPCargados = true;
            window._coTipoCache = coTipo; // ✅ NUEVO v27.0

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

// ════════════════════════════════════════════════════════════
// ✅ NUEVO v20.2: VALIDACIÓN DE DIFERENCIA
// ════════════════════════════════════════════════════════════

/**
 * ✅ NUEVO v20.2: Valida si se puede finalizar el pago según la diferencia
 * 
 * REGLAS DE NEGOCIO:
 * 
 * 1. DIFERENCIA > 0 (Falta pagar):
 *    ❌ BLOQUEAR - Los valores no son suficientes
 * 
 * 2. DIFERENCIA = 0 (Exacto):
 *    ✅ PERMITIR - Sin validación adicional
 * 
 * 3. DIFERENCIA < 0 (Sobrepago/Vuelto):
 *    ⚠️ VALIDAR según tipo de valores:
 *    - Si todos los valores son EF (Efectivo) → ✅ PERMITIR (se entregará vuelto)
 *    - Si hay CH (Cheque) Y co_tipo='CR' (Cliente Registrado) → ✅ PERMITIR (cobranza)
 *    - Cualquier otro caso → ❌ BLOQUEAR
 * 
 * @returns {Object} - { permitir: boolean, mensaje: string, advertencia?: string }
 */
function validarDiferenciaParaFinalizar() {
    console.log('═══════════════════════════════════════════════════');
    console.log('🔍 VALIDAR DIFERENCIA PARA FINALIZAR v20.2');
    console.log('═══════════════════════════════════════════════════');

    const diferencia = conceptosPago.diferencia || 0;
    const totalValores = conceptosPago.totalValores || 0;
    const totalPagar = conceptosPago.totalPagar || 0;

    console.log(`   Total a pagar: ${formatearMoneda(totalPagar)}`);
    console.log(`   Total valores: ${formatearMoneda(totalValores)}`);
    console.log(`   Diferencia: ${formatearMoneda(diferencia)}`);
    console.log(`   Tipo diferencia: ${diferencia > 0 ? 'POSITIVA (falta)' : diferencia < 0 ? 'NEGATIVA (sobra)' : 'CERO (exacto)'}`);

    // ═══════════════════════════════════════════════════════════
    // CASO 1: DIFERENCIA > 0 (FALTA PAGAR)
    // ═══════════════════════════════════════════════════════════
    if (diferencia > 0.01) {
        console.error('❌ DIFERENCIA POSITIVA: Falta pagar');
        console.error(`   Monto faltante: ${formatearMoneda(diferencia)}`);

        return {
            permitir: false,
            mensaje: `
                <div class="text-start">
                    <p class="mb-3">
                        <i class='bx bx-error-circle text-danger fs-3'></i>
                        <strong class="text-danger">Los valores ingresados no cubren el total de la factura</strong>
                    </p>
                    <table class="table table-sm table-bordered mb-3">
                        <tr>
                            <td class="text-end fw-bold">Total a pagar:</td>
                            <td class="text-end"><strong>${formatearMoneda(totalPagar)}</strong></td>
                        </tr>
                        <tr>
                            <td class="text-end fw-bold">Total valores:</td>
                            <td class="text-end text-warning"><strong>${formatearMoneda(totalValores)}</strong></td>
                        </tr>
                        <tr class="table-danger">
                            <td class="text-end fw-bold">Falta pagar:</td>
                            <td class="text-end"><strong class="text-danger">${formatearMoneda(diferencia)}</strong></td>
                        </tr>
                    </table>
                    <p class="mb-0">
                        <i class='bx bx-info-circle'></i> 
                        Debe agregar más valores de pago o ajustar los montos.
                    </p>
                </div>
            `
        };
    }

    // ═══════════════════════════════════════════════════════════
    // CASO 2: DIFERENCIA = 0 (EXACTO)
    // ═══════════════════════════════════════════════════════════
    if (Math.abs(diferencia) <= 0.01) {
        console.log('✅ DIFERENCIA CERO: Monto exacto');
        console.log('   No se requiere validación adicional');

        return {
            permitir: true,
            mensaje: ''
        };
    }

    // ═══════════════════════════════════════════════════════════
    // CASO 3: DIFERENCIA < 0 (SOBREPAGO/VUELTO)
    // ═══════════════════════════════════════════════════════════
    console.warn('⚠️ DIFERENCIA NEGATIVA: Sobrepago detectado');
    console.warn(`   Sobrepago: ${formatearMoneda(Math.abs(diferencia))}`);

    const sobrepago = Math.abs(diferencia);

    // ❶ Analizar tipos de valores ingresados
    const tiposPago = valoresPago.map(v => v.tcf_id.toUpperCase());
    const cantidadValores = valoresPago.length;

    console.log('═══════════════════════════════════════════════════');
    console.log('📊 ANÁLISIS DE VALORES DE PAGO');
    console.log(`   Total valores: ${cantidadValores}`);
    console.log(`   Tipos: ${tiposPago.join(', ')}`);

    // ❷ Verificar si todos son efectivo
    const tieneSoloEfectivo = tiposPago.every(tipo => tipo === 'EF');
    console.log(`   Solo efectivo: ${tieneSoloEfectivo ? 'SÍ ✅' : 'NO ❌'}`);

    // ❸ Verificar si hay cheques
    const tieneCheque = tiposPago.includes('CH');
    console.log(`   Tiene cheque: ${tieneCheque ? 'SÍ ✅' : 'NO ❌'}`);

    console.log('═══════════════════════════════════════════════════');

    // ═══════════════════════════════════════════════════════════
    // REGLA 1: Si todos son efectivo → PERMITIR (vuelto)
    // ═══════════════════════════════════════════════════════════
    if (tieneSoloEfectivo) {
        console.log('✅ REGLA 1 APLICADA: Todos los valores son efectivo');
        console.log('   → PERMITIR (se dará vuelto al cliente)');

        return {
            permitir: true,
            mensaje: '',
            advertencia: `
                <div class="alert alert-info mb-0">
                    <div class="d-flex align-items-center">
                        <i class='bx bx-info-circle fs-3 me-2'></i>
                        <div>
                            <strong>Vuelto a entregar:</strong><br>
                            <span class="fs-5 text-primary fw-bold">${formatearMoneda(sobrepago)}</span>
                        </div>
                    </div>
                </div>
            `,
            vuelto: sobrepago
        };
    }

    // ═══════════════════════════════════════════════════════════
    // REGLA 2: Si hay cheque y es cliente registrado → PERMITIR
    // ═══════════════════════════════════════════════════════════
    if (tieneCheque) {
        console.log('🔍 REGLA 2: Verificando si es cliente registrado...');

        // Obtener ID del cliente desde el modal de pago
        const ctaId = $('#txtClienteIdPago').val() || '';
        const esClienteRegistrado = ctaId && ctaId !== 'N/A' && ctaId.trim() !== '';

        console.log(`   cta_id: "${ctaId}"`);
        console.log(`   Es cliente registrado: ${esClienteRegistrado ? 'SÍ ✅' : 'NO ❌'}`);

        if (esClienteRegistrado) {
            console.log('✅ REGLA 2 APLICADA: Hay cheque y es cliente registrado');
            console.log('   → PERMITIR (operación de cobranza - sobrepago queda a favor del cliente)');

            return {
                permitir: true,
                mensaje: '',
                advertencia: `
                    <div class="alert alert-warning mb-0">
                        <div class="d-flex align-items-center">
                            <i class='bx bx-info-circle fs-3 me-2'></i>
                            <div>
                                <strong>Operación de Cobranza</strong><br>
                                <span class="text-muted">Sobrepago:</span> 
                                <span class="fs-5 text-warning fw-bold">${formatearMoneda(sobrepago)}</span><br>
                                <small class="text-muted">
                                    El sobrepago quedará registrado a favor del cliente
                                </small>
                            </div>
                        </div>
                    </div>
                `,
                sobrepago: sobrepago,
                esCobranza: true
            };
        } else {
            console.warn('⚠️ REGLA 2 NO APLICA: Hay cheque pero es consumidor final');
        }
    }

    // ═══════════════════════════════════════════════════════════
    // REGLA 3: Cualquier otro caso → BLOQUEAR
    // ═══════════════════════════════════════════════════════════
    console.error('❌ REGLA 3 APLICADA: Sobrepago no permitido con estos medios de pago');
    console.error(`   Tipos de pago: ${tiposPago.join(', ')}`);
    console.error('   → BLOQUEAR');

    return {
        permitir: false,
        mensaje: `
            <div class="text-start">
                <p class="mb-3">
                    <i class='bx bx-error-circle text-danger fs-3'></i>
                    <strong class="text-danger">Sobrepago no permitido con estos medios de pago</strong>
                </p>
                <table class="table table-sm table-bordered mb-3">
                    <tr>
                        <td class="text-end fw-bold">Total a pagar:</td>
                        <td class="text-end"><strong>${formatearMoneda(totalPagar)}</strong></td>
                    </tr>
                    <tr>
                        <td class="text-end fw-bold">Total valores:</td>
                        <td class="text-end text-warning"><strong>${formatearMoneda(totalValores)}</strong></td>
                    </tr>
                    <tr class="table-warning">
                        <td class="text-end fw-bold">Sobrepago:</td>
                        <td class="text-end"><strong class="text-warning">${formatearMoneda(sobrepago)}</strong></td>
                    </tr>
                </table>
                <div class="alert alert-info mb-3">
                    <strong>Reglas de negocio:</strong>
                    <ul class="mb-0 mt-2">
                        <li>Sobrepago solo se permite con <strong>efectivo</strong></li>
                        <li>O con <strong>cheques</strong> en operaciones de <strong>cobranza (clientes registrados)</strong></li>
                    </ul>
                </div>
                <p class="mb-0">
                    <i class='bx bx-info-circle'></i> 
                    Ajuste los montos o use solo efectivo para poder continuar.
                </p>
            </div>
        `
    };
}

// ════════════════════════════════════════════════════════════
// ✅ NUEVO v20.2: CONSTRUCCIÓN DE JSON_VALORES
// ════════════════════════════════════════════════════════════

/**
 * ✅ NUEVO v20.2: Construye el array de valores de pago en formato backend
 * Convierte valoresPago[] (frontend) → Json_Valores[] (backend)
 * 
 * REGLAS DE MAPEO POR TIPO DE PAGO:
 * 
 * EF (Efectivo):
 *   - rb_importe: Monto pagado
 *   - rb_dato1/2/3: Vacíos
 *   - rb_fecha_valor: Fecha actual
 * 
 * CH (Cheque):
 *   - rb_dato1_valor: Descripción del banco
 *   - rb_dato2_valor: Número de cheque (8 dígitos, relleno ceros izq)
 *   - rb_dato3_valor: Plaza (6 dígitos, relleno ceros izq, opcional)
 *   - rb_fecha_valor: Fecha de vencimiento del cheque
 *   - rb_importe: Monto del cheque
 * 
 * MU (Cupón Empresa/Mutual):
 *   - rb_dato1_valor: Titular (mínimo 5 caracteres)
 *   - rb_dato2_valor: Número de orden (10 dígitos, relleno ceros izq)
 *   - rb_dato3_valor: CUIT (formato XX-XXXXXXXX-X)
 *   - rb_fecha_valor: Fecha actual
 *   - rb_importe: Monto del cupón
 * 
 * VA (Vale de Compra):
 *   - rb_importe: Monto consumido
 *   - rb_dato1/2/3: Vacíos (idéntico a efectivo)
 *   - rb_fecha_valor: Fecha actual
 * 
 * BA (Transferencia Bancaria):
 *   - rb_dato1_valor: Vacío (no se usa)
 *   - rb_dato2_valor: Vacío (no se usa)
 *   - rb_dato3_valor: Número de transferencia (mínimo 15 dígitos, relleno ceros izq)
 *   - rb_fecha_valor: Fecha de transferencia
 *   - rb_importe: Monto de la transferencia
 * 
 * @returns {Array<Object>} - Array de Json_Valores en formato backend
 */
function construirJsonValores() {
    console.log('═══════════════════════════════════════════════════');
    console.log('🔨 CONSTRUIR JSON_VALORES v20.2');
    console.log(`   Total valores a procesar: ${valoresPago.length}`);
    console.log('═══════════════════════════════════════════════════');

    const jsonValores = [];

    valoresPago.forEach((valor, index) => {
        const tcfIdUpper = valor.tcf_id.toUpperCase();

        console.log(`   [${index + 1}/${valoresPago.length}] Procesando: ${valor.tcf_desc} - ${valor.ins_desc}`);
        console.log(`      Tipo: ${tcfIdUpper}`);
        console.log(`      Monto: ${valor.ins_simbolo} ${formatearNumero(valor.importe, 2)}`);

        // ❶ OBJETO BASE (común para todos los tipos)
        const valorBackend = {
            rb_nro_valor: (index + 1).toString().padStart(3, '0'), // "001", "002", "003"...
            ins_id: valor.ins_id || '',
            rb_dato1_valor: '',
            rb_dato2_valor: '',
            rb_dato3_valor: '',
            rb_opcion_cuota: '0',
            rb_cupon_manual: 'N',
            rb_ch_dif: 'N',
            rb_fecha_valor: new Date().toISOString().split('T')[0], // Fecha actual por defecto (YYYY-MM-DD)
            rb_importe: parseFloat(valor.importe) || 0,
            rb_rec: 0,
            rb_aux: 0,
            rb_estado: 'A', // "A" = Aplicado
            id_externo: ''
        };

        console.log(`      rb_nro_valor: ${valorBackend.rb_nro_valor}`);
        console.log(`      ins_id: ${valorBackend.ins_id}`);
        console.log(`      rb_importe: ${valorBackend.rb_importe}`);

        // ❷ MAPEO ESPECÍFICO SEGÚN TIPO DE PAGO
        switch (tcfIdUpper) {
            case 'EF': // ✅ EFECTIVO
                console.log('      → Tipo: EFECTIVO');
                console.log('         rb_importe: ✅ Ya cargado');
                console.log('         rb_dato1/2/3: Vacíos');
                console.log('         rb_fecha_valor: Fecha actual');
                // Sin datos extras necesarios
                break;

            case 'CH': // ✅ CHEQUE
                console.log('      → Tipo: CHEQUE');

                if (valor.detalle) {
                    // rb_dato1_valor = Descripción del Banco
                    valorBackend.rb_dato1_valor = valor.detalle.banco_nombre || '';
                    console.log(`         rb_dato1_valor (Banco): "${valorBackend.rb_dato1_valor}"`);

                    // rb_dato2_valor = Número de Cheque (8 dígitos, relleno ceros izq)
                    const nroCheque = (valor.detalle.nro_cheque || '').toString().padStart(8, '0');
                    valorBackend.rb_dato2_valor = nroCheque;
                    console.log(`         rb_dato2_valor (Nro Cheque): "${nroCheque}"`);

                    // rb_dato3_valor = Plaza (6 dígitos, relleno ceros izq, opcional)
                    const plaza = valor.detalle.plaza || '';
                    valorBackend.rb_dato3_valor = plaza ? plaza.toString().padStart(6, '0') : '';
                    console.log(`         rb_dato3_valor (Plaza): "${valorBackend.rb_dato3_valor || 'N/A'}"`);

                    // rb_fecha_valor = Fecha de vencimiento del cheque
                    valorBackend.rb_fecha_valor = valor.detalle.fecha_cheque || valorBackend.rb_fecha_valor;
                    console.log(`         rb_fecha_valor (Vencimiento): ${valorBackend.rb_fecha_valor}`);

                    console.log('         ✅ Cheque mapeado correctamente');
                } else {
                    console.warn('         ⚠️ Cheque sin detalle - Usando valores por defecto');
                }
                break;

            case 'MU': // ✅ CUPÓN EMPRESA/MUTUAL
                console.log('      → Tipo: CUPÓN EMPRESA/MUTUAL');

                if (valor.detalle) {
                    // rb_dato1_valor = Titular (mínimo 5 caracteres)
                    valorBackend.rb_dato1_valor = valor.detalle.titular || '';
                    console.log(`         rb_dato1_valor (Titular): "${valorBackend.rb_dato1_valor}"`);

                    // rb_dato2_valor = Número de Orden (10 dígitos, relleno ceros izq)
                    const nroOrden = (valor.detalle.nro_orden || '').toString().padStart(10, '0');
                    valorBackend.rb_dato2_valor = nroOrden;
                    console.log(`         rb_dato2_valor (Nro Orden): "${nroOrden}"`);

                    // rb_dato3_valor = CUIT (formato XX-XXXXXXXX-X)
                    valorBackend.rb_dato3_valor = valor.detalle.cuit || '';
                    console.log(`         rb_dato3_valor (CUIT): "${valorBackend.rb_dato3_valor}"`);

                    // rb_fecha_valor = Fecha actual (ya está por defecto)
                    console.log(`         rb_fecha_valor: ${valorBackend.rb_fecha_valor}`);

                    console.log('         ✅ Cupón mapeado correctamente');
                } else {
                    console.warn('         ⚠️ Cupón sin detalle - Usando valores por defecto');
                }
                break;

            case 'VA': // ✅ VALE DE COMPRA (idéntico a efectivo)
                console.log('      → Tipo: VALE DE COMPRA (idéntico a efectivo)');
                console.log('         rb_importe: ✅ Ya cargado');
                console.log('         rb_dato1/2/3: Vacíos');
                console.log('         rb_fecha_valor: Fecha actual');
                // Sin datos extras necesarios
                break;

            case 'BA': // ✅ TRANSFERENCIA BANCARIA
                console.log('      → Tipo: TRANSFERENCIA BANCARIA');

                if (valor.detalle) {
                    // rb_dato1_valor = VACÍO (no se usa)
                    valorBackend.rb_dato1_valor = '';
                    console.log('         rb_dato1_valor: Vacío (no se usa)');

                    // rb_dato2_valor = VACÍO (no se usa)
                    valorBackend.rb_dato2_valor = '';
                    console.log('         rb_dato2_valor: Vacío (no se usa)');

                    // rb_dato3_valor = Número de Transferencia (mínimo 15 dígitos, relleno ceros izq)
                    const nroTransf = (valor.detalle.nro_transferencia || '').toString().padStart(15, '0');
                    valorBackend.rb_dato3_valor = nroTransf;
                    console.log(`         rb_dato3_valor (Nro Transferencia): "${nroTransf}"`);

                    // rb_fecha_valor = Fecha de transferencia
                    valorBackend.rb_fecha_valor = valor.detalle.fecha_transferencia || valorBackend.rb_fecha_valor;
                    console.log(`         rb_fecha_valor (Fecha Transf): ${valorBackend.rb_fecha_valor}`);

                    console.log('         ✅ Transferencia mapeada correctamente');
                } else {
                    console.warn('         ⚠️ Transferencia sin detalle - Usando valores por defecto');
                }
                break;

            case 'TC': // ⏳ TARJETA CRÉDITO (futuro)
                console.warn('      → Tipo: TARJETA CRÉDITO');
                console.warn('         ⚠️ Tipo pendiente de implementación');
                console.warn('         Usando valores por defecto');
                break;

            case 'TD': // ⏳ TARJETA DÉBITO (futuro)
                console.warn('      → Tipo: TARJETA DÉBITO');
                console.warn('         ⚠️ Tipo pendiente de implementación');
                console.warn('         Usando valores por defecto');
                break;

            default:
                console.warn(`      → Tipo: ${tcfIdUpper} (DESCONOCIDO)`);
                console.warn('         ⚠️ Tipo sin mapeo específico');
                console.warn('         Usando valores por defecto');
                break;
        }

        // ❸ Agregar al array
        jsonValores.push(valorBackend);

        console.log(`      ✅ Valor agregado al array: ${valor.ins_simbolo} ${formatearNumero(valorBackend.rb_importe, 2)}`);
        console.log('');
    });

    console.log('═══════════════════════════════════════════════════');
    console.log(`✅ ${jsonValores.length} VALORES MAPEADOS CORRECTAMENTE`);
    console.log('═══════════════════════════════════════════════════');
    console.log('📦 JSON_VALORES RESULTANTE:');
    console.log(JSON.stringify(jsonValores, null, 2));
    console.log('═══════════════════════════════════════════════════');

    return jsonValores;
}

/**
 * ✅ ACTUALIZADO v20.2: Finaliza el pago y envía datos al servidor
 * INTEGRACIÓN COMPLETA: Validación + Construcción JSON + Envío
 * 
 * FLUJO:
 * 1. Valida que haya formas de pago
 * 2. Valida diferencia según reglas de negocio (Lote 2)
 * 3. Construye payload JSON usando construirJsonValores() (Lote 3)
 * 4. Confirma con el usuario
 * 5. Envía datos al servidor (Lote 4)
 */
function finalizarPago() {
    console.log('═══════════════════════════════════════════════════');
    console.log('✅ FINALIZAR PAGO v20.2 - FLUJO COMPLETO');
    console.log('═══════════════════════════════════════════════════');

    // ❶ Validar que haya formas de pago
    if (valoresPago.length === 0) {
        console.error('❌ No hay formas de pago ingresadas');

        if (typeof toastr !== 'undefined') {
            toastr.error('Debe agregar al menos una forma de pago', 'Error');
        }

        return;
    }

    console.log(`   Total formas de pago: ${valoresPago.length}`);

    // ❷ Validar diferencia según reglas de negocio
    console.log('🔍 Validando diferencia...');
    const validacion = validarDiferenciaParaFinalizar();

    console.log('═══════════════════════════════════════════════════');
    console.log('📋 RESULTADO DE VALIDACIÓN DE DIFERENCIA');
    console.log(`   Permitir: ${validacion.permitir ? 'SÍ ✅' : 'NO ❌'}`);
    if (validacion.advertencia) {
        console.log('   ⚠️ Advertencia presente (vuelto o sobrepago permitido)');
    }
    console.log('═══════════════════════════════════════════════════');

    if (!validacion.permitir) {
        console.error('❌ Validación de diferencia FALLÓ - Operación BLOQUEADA');

        AbrirMensaje(
            "No se puede finalizar",
            validacion.mensaje,
            function () {
                $("#msjModal").modal("hide");
            },
            false,
            ["Aceptar"],
            "error!",
            null
        );

        return;
    }

    console.log('✅ Validación de diferencia EXITOSA - Puede continuar');

    // ❸ Construir valores de pago en formato backend
    console.log('🔨 Construyendo JSON de valores...');
    const jsonValores = construirJsonValores(); // ← INTEGRACIÓN LOTE 3

    console.log('═══════════════════════════════════════════════════');
    console.log('📦 JSON VALORES CONSTRUIDO');
    console.log(`   Total valores: ${jsonValores.length}`);
    console.log('═══════════════════════════════════════════════════');

    // ❹ Construir mensaje adicional si hay advertencia (vuelto/sobrepago)
    let mensajeAdicional = '';

    if (validacion.advertencia) {
        mensajeAdicional = validacion.advertencia;
        console.log('⚠️ Mensaje de advertencia agregado al modal de confirmación');
    }

    // ❺ Construir mensaje de confirmación
    const mensajeConfirmacion = `
        <div class="text-start">
            <p class="mb-3">
                <strong class="fs-5">¿Confirmar el pago de la factura?</strong>
            </p>
            <table class="table table-sm table-bordered mb-3">
                <tr>
                    <td class="text-end fw-bold">Total a pagar:</td>
                    <td class="text-end"><strong>${formatearMoneda(conceptosPago.totalPagar)}</strong></td>
                </tr>
                <tr>
                    <td class="text-end fw-bold">Total valores:</td>
                    <td class="text-end text-success"><strong>${formatearMoneda(conceptosPago.totalValores)}</strong></td>
                </tr>
                <tr>
                    <td class="text-end fw-bold">Formas de pago:</td>
                    <td class="text-end"><strong class="text-primary">${valoresPago.length}</strong></td>
                </tr>
            </table>
            ${mensajeAdicional}
            <div class="alert alert-warning mb-0 mt-3">
                <i class="bx bx-info-circle"></i> 
                Esta acción emitirá la factura fiscal y <strong>no se puede deshacer</strong>.
            </div>
        </div>
    `;

    // ❻ Confirmar con el usuario
    console.log('💬 Mostrando modal de confirmación al usuario...');

    AbrirMensaje(
        "Confirmar Pago",
        mensajeConfirmacion,
        function (respuesta) {
            $("#msjModal").modal("hide");

            if (respuesta === "SI") {
                console.log('✅ Usuario confirmó - Procediendo a enviar al servidor...');

                // ❼ Esperar cierre del modal y enviar
                setTimeout(() => {
                    enviarPagoAlServidor(jsonValores); // ← INTEGRACIÓN LOTE 4
                }, 300);
            } else {
                console.log('❌ Usuario canceló la operación');
            }
        },
        true, // Es confirmación
        ["Sí, Finalizar Pago", "Cancelar"],
        "quest!",
        null
    );
}

// ════════════════════════════════════════════════════════════
// ✅ NUEVO v20.2: ENVÍO DE PAGO AL SERVIDOR
// ════════════════════════════════════════════════════════════

/**
 * ✅ ACTUALIZADO v28.0: Envía el pago al servidor con soporte para Cobranza Diferida
 * NUEVO: Si contexto es COBRANZA, incluye facturas a cancelar en el payload
 * 
 * CAMBIOS v28.0:
 * - Detecta contexto de operación (VENTA/COBRANZA)
 * - Si es COBRANZA, obtiene facturas desde sesión servidor
 * - Construye array Cancelar con estructura correcta
 * - Incluye Cancelar en payload solo si es Cobranza
 * 
 * @param {Array<Object>} jsonValores - Array de Json_Valores construido
 */
function enviarPagoAlServidor(jsonValores) {
    console.log('═══════════════════════════════════════════════════');
    console.log('📤 ENVIANDO PAGO AL SERVIDOR v28.0');
    console.log('═══════════════════════════════════════════════════');
    console.log(`   Total valores: ${jsonValores.length}`);
    console.log(`   Total monto: ${formatearMoneda(conceptosPago.totalValores)}`);

    // ═══════════════════════════════════════════════════════════
    // ✅ NUEVO v28.0: DETECTAR CONTEXTO DE OPERACIÓN
    // ═══════════════════════════════════════════════════════════

    console.log('═══════════════════════════════════════════════════');
    console.log('🔍 DETECTANDO CONTEXTO DE OPERACIÓN v28.0');

    // Obtener contexto desde variable global (establecida por iniciarProcesoPago)
    let contextoOperacion = window._contextoOperacionActual || 'VENTA';
    contextoOperacion = contextoOperacion.toUpperCase();

    console.log(`   Contexto detectado: ${contextoOperacion}`);

    // Determinar ModuloOrigen según contexto
    let moduloOrigen = 'Facturacion'; // ← Default para VENTA

    if (contextoOperacion === 'COBRANZA') {
        moduloOrigen = 'CobranzaDiferida';
        console.log('   ✅ Contexto COBRANZA detectado');
        console.log('   → ModuloOrigen: CobranzaDiferida');
        console.log('   → Se incluirán facturas a cancelar en el payload');
    } else {
        console.log('   ✅ Contexto VENTA (default)');
        console.log('   → ModuloOrigen: Facturacion');
    }

    // Obtener co_tipo
    let coTipo = window._coTipoActual || null;

    if (!coTipo) {
        console.warn('⚠️ Variable global _coTipoActual NO definida');
        const ctaId = $('#txtClienteIdPago').val() || '';
        coTipo = ctaId && ctaId !== 'N/A' && ctaId.trim() !== '' ? 'CR' : 'CF';
        console.log(`   co_tipo calculado (fallback): ${coTipo}`);
    } else {
        console.log(`   co_tipo obtenido: ${coTipo}`);
    }

    console.log('═══════════════════════════════════════════════════');

    // Bloquear pantalla
    mostrarLoadingGlobal('Procesando pago y emitiendo comprobante...');

    // ═══════════════════════════════════════════════════════════
    // ✅ NUEVO v28.0: SI ES COBRANZA, OBTENER FACTURAS A CANCELAR
    // ═══════════════════════════════════════════════════════════

    if (contextoOperacion === 'COBRANZA') {
        console.log('═══════════════════════════════════════════════════');
        console.log('📋 OBTENIENDO FACTURAS A CANCELAR DESDE SESIÓN v28.0');
        console.log('═══════════════════════════════════════════════════');

        // Actualizar mensaje de loading
        actualizarMensajeLoadingGlobal('Obteniendo facturas a cancelar...');

        // ❶ URL del endpoint para obtener facturas desde sesión
        const urlFacturas = typeof ObtenerFacturasPendientesSesionUrl !== 'undefined' && ObtenerFacturasPendientesSesionUrl
            ? ObtenerFacturasPendientesSesionUrl
            : '/Facturacion/PDiferido/ObtenerFacturasPendientesSesion';

        console.log(`   URL: ${urlFacturas}`);

        // ❷ Llamada AJAX para obtener facturas
        $.ajax({
            url: urlFacturas,
            type: 'POST',
            dataType: 'json',
            timeout: 10000,
            success: function (responseFacturas) {
                console.log('   📥 Respuesta de facturas:', responseFacturas);
                //para lo unico que me sirve recuperar las facturas es para confirmar que las mismas estan. pero las tengo resguardadas en session las que se van a cobrar.
                if (!responseFacturas || !responseFacturas.ok) {
                    console.error('❌ No se pudieron obtener las facturas');
                    ocultarLoadingGlobal();

                    AbrirMensaje(
                        "Error",
                        "No se pudieron recuperar las facturas a cancelar.",
                        function () { $('#msjModal').modal('hide'); },
                        false,
                        ["Aceptar"],
                        "error!",
                        null
                    );
                    return;
                }

                const facturasACancelar = responseFacturas.lista || [];
                console.log(`   ✅ Facturas obtenidas: ${facturasACancelar.length}`);

                // ❸ Construir array Cancelar con estructura correcta
                const arrayCancelar = construirArrayCancelar(facturasACancelar);

                console.log(`   ✅ Array Cancelar construido: ${arrayCancelar.length} items`);

                // ❹ Proceder a enviar el pago con Cancelar incluido
                enviarPayloadAlServidor(jsonValores, moduloOrigen, arrayCancelar);
            },
            error: function (jqXHR, textStatus, errorThrown) {
                console.error('❌ ERROR AJAX al obtener facturas:', {
                    status: jqXHR.status,
                    error: errorThrown
                });

                ocultarLoadingGlobal();

                AbrirMensaje(
                    "Error de Comunicación",
                    "No se pudo conectar con el servidor para obtener las facturas.",
                    function () { $('#msjModal').modal('hide'); },
                    false,
                    ["Aceptar"],
                    "error!",
                    null
                );
            }
        });

    } else {
        // ═══════════════════════════════════════════════════════════
        // FLUJO NORMAL DE VENTA (sin Cancelar)
        // ═══════════════════════════════════════════════════════════
        console.log('   ℹ️ Contexto VENTA - Sin facturas a cancelar');
        enviarPayloadAlServidor(jsonValores, moduloOrigen, null);
    }
}

/**
 * ✅ NUEVA v28.0: Construye el array Cancelar con la estructura requerida por el backend
 * 
 * ESTRUCTURA DE SALIDA (Json_Cancela):
 * {
 *     tco_id: string,        // Tipo de comprobante (ej: "001", "006")
 *     cm_compte: string,     // Número de comprobante (ej: "00001-00000123")
 *     cm_compte_cuota: int   // Número de cuota (default: 0)
 * }
 * 
 * @param {Array<Object>} facturas - Array de facturas desde sesión servidor
 * @returns {Array<Object>} - Array de objetos Json_Cancela
 */
function construirArrayCancelar(facturas) {
    console.log('═══════════════════════════════════════════════════');
    console.log('🔨 CONSTRUIR ARRAY CANCELAR v28.0');
    console.log(`   Total facturas recibidas: ${facturas.length}`);
    console.log('═══════════════════════════════════════════════════');

    const arrayCancelar = [];

    if (!facturas || !Array.isArray(facturas) || facturas.length === 0) {
        console.warn('⚠️ No hay facturas para procesar');
        return arrayCancelar;
    }

    facturas.forEach((factura, index) => {
        try {
            // ❶ Validar datos críticos
            if (!factura.tco_id || !factura.cm_compte) {
                console.warn(`⚠️ Factura ${index} sin datos críticos, omitiendo:`, factura);
                return;
            }

            // ❷ Construir objeto Json_Cancela
            const jsonCancela = {
                tco_id: String(factura.tco_id).trim(),
                cm_compte: String(factura.cm_compte).trim(),
                cm_compte_cuota: parseInt(factura.cm_compte_cuota) || 0
            };

            arrayCancelar.push(jsonCancela);

            console.log(`   ✅ [${index + 1}] ${jsonCancela.tco_id} ${jsonCancela.cm_compte} (Cuota: ${jsonCancela.cm_compte_cuota})`);

        } catch (error) {
            console.error(`❌ Error al procesar factura ${index}:`, error, factura);
        }
    });

    console.log('═══════════════════════════════════════════════════');
    console.log(`✅ ARRAY CANCELAR CONSTRUIDO: ${arrayCancelar.length} ITEMS`);
    console.log('═══════════════════════════════════════════════════');

    return arrayCancelar;
}

/**
 * ✅ ACTUALIZADO v28.1: Envía el payload completo al servidor
 * CAMBIO CRÍTICO v28.1: Prioriza facturas desde variable global si arrayCancelar está vacío
 * 
 * FLUJO DE PRIORIDADES:
 * 1. arrayCancelar (parámetro explícito) - Primera prioridad
 * 2. window._facturasSeleccionadasParaCobro (variable global) - Segunda prioridad
 * 3. Cancelar: [] (vacío) - Si no hay facturas disponibles (modo VENTA)
 * 
 * CONTEXTOS SOPORTADOS:
 * - VENTA: Cancelar = [] (sin facturas)
 * - COBRANZA: Cancelar = facturas desde variable global o parámetro
 * 
 * @param {Array<Object>} jsonValores - Array de medios de pago (Json_Valor)
 * @param {string} moduloOrigen - "Facturacion" o "CobranzaDiferida"
 * @param {Array<Object>|null} arrayCancelar - Array de facturas a cancelar (Json_Cancela) [OPCIONAL]
 */
function enviarPayloadAlServidor(jsonValores, moduloOrigen, arrayCancelar) {
    console.log('═══════════════════════════════════════════════════');
    console.log('📦 ENVIAR PAYLOAD AL SERVIDOR v28.1');
    console.log(`   ModuloOrigen: ${moduloOrigen}`);
    console.log(`   Valores: ${jsonValores.length}`);
    console.log(`   Cancelar (parámetro): ${arrayCancelar ? arrayCancelar.length : 'NULL'}`);
    console.log('═══════════════════════════════════════════════════');

    // ❶ Actualizar mensaje de loading
    actualizarMensajeLoadingGlobal('Procesando pago y emitiendo comprobante...');

    // ❷ URL del endpoint
    const url = typeof finalizarCompraUrl !== 'undefined' && finalizarCompraUrl
        ? finalizarCompraUrl
        : '/Facturacion/Checkout/FinalizarCompra';

    console.log(`   URL: ${url}`);

    // ❸ Construir payload base
    const payload = {
        Valores: jsonValores,
        Uniones: [],
        ModuloOrigen: moduloOrigen
    };

    // ❹ ✅ CRÍTICO v28.1: Determinar facturas a cancelar (con prioridades)
    let cancelarFinal = arrayCancelar;

    console.log('═══════════════════════════════════════════════════');
    console.log('🔍 DETERMINANDO FACTURAS A CANCELAR v28.1');
    console.log('═══════════════════════════════════════════════════');

    // ═══════════════════════════════════════════════════════════
    // PRIORIDAD 1: Usar arrayCancelar si viene como parámetro
    // ═══════════════════════════════════════════════════════════
    if (cancelarFinal && Array.isArray(cancelarFinal) && cancelarFinal.length > 0) {
        console.log('✅ PRIORIDAD 1: arrayCancelar desde parámetro');
        console.log(`   Total facturas: ${cancelarFinal.length}`);

        // Loguear primeras 3 facturas (muestra)
        for (let i = 0; i < Math.min(3, cancelarFinal.length); i++) {
            const f = cancelarFinal[i];
            console.log(`   [${i + 1}] ${f.tco_id} ${f.cm_compte} (Cuota: ${f.cm_compte_cuota || 0})`);
        }

        if (cancelarFinal.length > 3) {
            console.log(`   ... y ${cancelarFinal.length - 3} más`);
        }
    }
    // ═══════════════════════════════════════════════════════════
    // PRIORIDAD 2: Buscar en variable global
    // ═══════════════════════════════════════════════════════════
    else if (window._facturasSeleccionadasParaCobro &&
        Array.isArray(window._facturasSeleccionadasParaCobro) &&
        window._facturasSeleccionadasParaCobro.length > 0) {

        console.log('✅ PRIORIDAD 2: Facturas desde variable global');
        console.log('   Origen: window._facturasSeleccionadasParaCobro');

        cancelarFinal = window._facturasSeleccionadasParaCobro;

        console.log(`   Total facturas: ${cancelarFinal.length}`);

        // Loguear primeras 3 facturas (muestra)
        for (let i = 0; i < Math.min(3, cancelarFinal.length); i++) {
            const f = cancelarFinal[i];
            console.log(`   [${i + 1}] ${f.tco_id} ${f.cm_compte} (Cuota: ${f.cm_compte_cuota || 0})`);
        }

        if (cancelarFinal.length > 3) {
            console.log(`   ... y ${cancelarFinal.length - 3} más`);
        }
    }
    // ═══════════════════════════════════════════════════════════
    // SIN FACTURAS: Modo VENTA
    // ═══════════════════════════════════════════════════════════
    else {
        console.log('ℹ️ Sin facturas a cancelar');
        console.log('   Contexto: VENTA (sin Cobranza Diferida)');
        cancelarFinal = null;
    }

    console.log('═══════════════════════════════════════════════════');

    // ❺ Incluir facturas en el payload
    if (cancelarFinal && Array.isArray(cancelarFinal) && cancelarFinal.length > 0) {
        payload.Cancelar = cancelarFinal;
        console.log(`✅ Campo Cancelar incluido: ${cancelarFinal.length} factura(s)`);
    } else {
        payload.Cancelar = [];
        console.log('ℹ️ Campo Cancelar vacío (modo VENTA)');
    }

    console.log('═══════════════════════════════════════════════════');
    console.log('📦 PAYLOAD COMPLETO v28.1:');
    console.log('═══════════════════════════════════════════════════');
    console.log(`   ModuloOrigen: ${payload.ModuloOrigen}`);
    console.log(`   Valores.length: ${payload.Valores.length}`);
    console.log(`   Uniones.length: ${payload.Uniones.length}`);
    console.log(`   Cancelar.length: ${payload.Cancelar.length}`);
    console.log('═══════════════════════════════════════════════════');
    console.log('Payload JSON completo:');
    console.log(JSON.stringify(payload, null, 2));
    console.log('═══════════════════════════════════════════════════');

    // ❻ Llamada AJAX
    $.ajax({
        url: url,
        type: 'POST',
        contentType: 'application/json',
        data: JSON.stringify(payload),
        dataType: 'json',
        timeout: 120000
    })
        .done(function (response) {
            console.log('═══════════════════════════════════════════════════');
            console.log('✅ RESPUESTA RECIBIDA DEL SERVIDOR v28.1');
            console.log('═══════════════════════════════════════════════════');
            console.log('Response completo:', response);

            // ❼ Validar respuesta básica
            if (!response || response.ok === false) {
                console.error('❌ Error en respuesta del servidor');
                ocultarLoadingGlobal();
                procesarErrorPago(response);
                return;
            }

            console.log('✅ Respuesta OK del servidor');

            // ❽ Detectar advertencia de PV (Punto de Venta)
            const tieneAdvertenciaPV = response.mostrar_mensaje_pv === true;
            const mensajeAdvertenciaPV = response.mensaje_advertencia || '';

            if (tieneAdvertenciaPV) {
                console.log('⚠️ ADVERTENCIA DE PV DETECTADA');
                console.log(`   mensaje_advertencia: "${mensajeAdvertenciaPV}"`);
            }

            // ❾ Validar datos del comprobante
            if (!response.data || !Array.isArray(response.data) || response.data.length === 0) {
                console.error('❌ No se recibieron datos del comprobante');
                ocultarLoadingGlobal();
                mostrarMensajeError('Error: No se recibió información del comprobante');
                return;
            }

            console.log(`✅ Datos del comprobante recibidos: ${response.data.length} comprobante(s)`);

            const comprobante = response.data[0];
            const esCobranzaDiferida = comprobante.es_cobranza_diferida === true;

            console.log('═══════════════════════════════════════════════════');
            console.log('📄 COMPROBANTE EMITIDO v28.1');
            console.log(`   tco_letra: ${comprobante.tco_letra}`);
            console.log(`   tco_id: ${comprobante.tco_id}`);
            console.log(`   cm_compte: ${comprobante.cm_compte}`);
            console.log(`   es_cobranza_diferida: ${esCobranzaDiferida ? 'SÍ' : 'NO'}`);
            console.log('═══════════════════════════════════════════════════');

            // ❿ Generar reporte PDF
            console.log('📄 Iniciando generación de reporte...');

            if (typeof ModuloReportes !== 'undefined') {
                console.log('✅ ModuloReportes disponible - Generando PDF...');

                ModuloReportes.generarYVisualizarReporte({
                    tco_letra: comprobante.tco_letra,
                    tco_id: comprobante.tco_id,
                    cm_compte: comprobante.cm_compte,
                    cm_repetido: comprobante.cm_repetido
                }).then(function (exitoso) {
                    console.log(`📄 Generación de reporte: ${exitoso ? '✅ EXITOSA' : '❌ FALLIDA'}`);

                    setTimeout(function () {
                        console.log('⏳ PDF abierto - Desbloqueando interfaz...');
                        ocultarLoadingGlobal();

                        if (tieneAdvertenciaPV && mensajeAdvertenciaPV) {
                            mostrarAdvertenciaPV(mensajeAdvertenciaPV, function () {
                                procesarPagoExitoso(comprobante, esCobranzaDiferida);
                            });
                        } else {
                            procesarPagoExitoso(comprobante, esCobranzaDiferida);
                        }
                    }, 500);

                }).catch(function (error) {
                    console.error('❌ ERROR al generar reporte:', error);
                    ocultarLoadingGlobal();

                    if (tieneAdvertenciaPV && mensajeAdvertenciaPV) {
                        mostrarAdvertenciaPV(mensajeAdvertenciaPV, function () {
                            procesarPagoExitoso(comprobante, esCobranzaDiferida);
                        });
                    } else {
                        procesarPagoExitoso(comprobante, esCobranzaDiferida);
                    }
                });
            } else {
                console.warn('⚠️ ModuloReportes NO disponible');
                ocultarLoadingGlobal();

                if (tieneAdvertenciaPV && mensajeAdvertenciaPV) {
                    mostrarAdvertenciaPV(mensajeAdvertenciaPV, function () {
                        procesarPagoExitoso(comprobante, esCobranzaDiferida);
                    });
                } else {
                    procesarPagoExitoso(comprobante, esCobranzaDiferida);
                }
            }
        })
        .fail(function (jqXHR, textStatus, errorThrown) {
            console.log('═══════════════════════════════════════════════════');
            console.error('❌ ERROR EN AJAX - ENVÍO DE PAGO');
            console.log('═══════════════════════════════════════════════════');
            console.error('   textStatus:', textStatus);
            console.error('   errorThrown:', errorThrown);
            console.error('   HTTP Status:', jqXHR.status);
            console.log('═══════════════════════════════════════════════════');

            ocultarLoadingGlobal();

            // ⓫ Validar sesión expirada
            if (jqXHR.status === 401 || jqXHR.status === 403) {
                console.error('❌ SESIÓN EXPIRADA');
                manejarSesionExpirada('Su sesión ha expirado. Por favor, inicie sesión nuevamente.');
                return;
            }

            // ⓬ Mensaje de error genérico
            let mensajeError = 'Error de comunicación con el servidor';

            if (jqXHR.status === 500) {
                mensajeError = 'Error interno del servidor. Contacte al administrador.';
            } else if (jqXHR.status === 0) {
                mensajeError = 'No se pudo conectar con el servidor. Verifique su conexión.';
            } else if (jqXHR.responseJSON && jqXHR.responseJSON.mensaje) {
                mensajeError = jqXHR.responseJSON.mensaje;
            }

            AbrirMensaje(
                "Error de Comunicación",
                mensajeError,
                function () { $("#msjModal").modal("hide"); },
                false,
                ["Aceptar"],
                "error!",
                null
            );
        });

    // ⓭ Timeout de seguridad (30 segundos)
    setTimeout(function () {
        if ($('#overlayLoadingGlobal').length > 0 && $('#overlayLoadingGlobal').is(':visible')) {
            console.warn('⚠️ TIMEOUT DE SEGURIDAD ALCANZADO (30s)');
            ocultarLoadingGlobal();

            AbrirMensaje(
                "Tiempo de Espera Agotado",
                "La operación está tomando más tiempo del esperado.<br><br>Verifique el resultado en el sistema.",
                function () { $("#msjModal").modal("hide"); },
                false,
                ["Aceptar"],
                "warning",
                null
            );
        }
    }, 30000);
}

/**
 * ✅ NUEVO v20.4: Muestra advertencia de validación de Punto de Venta
 * 
 * CASOS DE USO:
 * - Facturación Electrónica con CAEA vigente (resultado 1)
 * - Controlador Fiscal Hasar con comprobante cancelado (resultado 1)
 * 
 * @param {string} mensajeAdvertencia - Mensaje de advertencia del PV
 * @param {Function} callback - Función a ejecutar después de cerrar la advertencia
 */
function mostrarAdvertenciaPV(mensajeAdvertencia, callback) {
    console.log('═══════════════════════════════════════════════════');
    console.log('⚠️ MOSTRANDO ADVERTENCIA DE PV v20.4');
    console.log('═══════════════════════════════════════════════════');
    console.log(`   Mensaje: "${mensajeAdvertencia}"`);
    console.log('═══════════════════════════════════════════════════');

    AbrirMensaje(
        "Advertencia del Punto de Venta",
        `<div class="text-start">
            <p class="mb-3">
                <i class='bx bx-info-circle text-warning fs-3'></i>
                <strong class="text-warning">El comprobante se emitió correctamente, pero con advertencia:</strong>
            </p>
            <div class="alert alert-warning mb-3">
                <i class='bx bx-error-circle'></i> ${escapeHtml(mensajeAdvertencia)}
            </div>
            <p class="mb-0">
                <i class='bx bx-check-circle'></i> 
                La factura fue registrada exitosamente en el sistema.
            </p>
        </div>`,
        function () {
            $("#msjModal").modal("hide");

            // Ejecutar callback después de cerrar modal
            if (callback && typeof callback === 'function') {
                setTimeout(() => {
                    callback();
                }, 300);
            }
        },
        false,
        ["Aceptar"],
        "warning",
        null
    );
}

/**
 * ✅ ACTUALIZADO v27.0: Procesa una respuesta exitosa del servidor
 * NUEVO: Mensajes diferenciados según contexto (VENTA/COBRANZA)
 * 
 * CAMBIOS v27.0:
 * - ✅ Parámetro esCobranzaDiferida para diferenciar mensajes
 * - ✅ Mensaje específico para CobranzaDiferida: "Recibo" en lugar de "Factura"
 * - ✅ Mantiene lógica de preservación de backup (v20.3)
 * 
 * @param {Object} comprobante - Datos del comprobante emitido
 * @param {boolean} esCobranzaDiferida - true si es CobranzaDiferida, false si es Venta normal
 */
function procesarPagoExitoso(comprobante, esCobranzaDiferida = false) {
    console.log('═══════════════════════════════════════════════════');
    console.log('✅ PROCESANDO PAGO EXITOSO v27.0');
    console.log('═══════════════════════════════════════════════════');
    console.log('   Comprobante:', comprobante.cm_compte);
    console.log(`   Es Cobranza Diferida: ${esCobranzaDiferida ? 'SÍ' : 'NO'}`);

    const tipoComprobante = obtenerTipoComprobante(comprobante.tco_letra, comprobante.tco_id);
    const numeroComprobante = comprobante.cm_compte || 'Sin número';
    const esRepetido = comprobante.cm_repetido === "1" || comprobante.cm_repetido === 1;

    console.log(`   Tipo: ${tipoComprobante}`);
    console.log(`   Número: ${numeroComprobante}`);
    console.log(`   Repetido: ${esRepetido ? 'SÍ' : 'NO'}`);

    // ═══════════════════════════════════════════════════════════
    // ✅ NUEVO v27.0: MENSAJES DIFERENCIADOS POR CONTEXTO
    // ═══════════════════════════════════════════════════════════

    let tituloMensaje = '¡Factura Emitida y Pagada!';
    let mensajePrincipal = 'Factura emitida y pagada exitosamente';
    let iconoColor = 'text-golden'; // Color del título (opcional)

    if (esCobranzaDiferida) {
        console.log('   📋 Contexto: COBRANZA DIFERIDA');
        tituloMensaje = '¡Cobro Procesado Exitosamente!';
        mensajePrincipal = 'Recibo de cobranza emitido y registrado correctamente';
        iconoColor = 'text-success'; // Verde para cobros
    } else {
        console.log('   📋 Contexto: VENTA NORMAL');
    }

    // Mostrar mensaje de éxito
    AbrirMensaje(
        tituloMensaje,
        `<div class="text-center">
            <div class="mb-3">
                <i class='bx bx-check-circle ${iconoColor}' style="font-size: 4rem;"></i>
            </div>
            <h4 class="text-golden mb-3">${mensajePrincipal}</h4>
            
            <div class="alert alert-success mb-3">
                <div class="mb-2">
                    <strong class="d-block text-uppercase">${tipoComprobante}</strong>
                    <span class="badge bg-primary fs-6">${comprobante.tco_letra}</span>
                </div>
                <div class="mt-2">
                    <small class="text-muted">Número:</small><br>
                    <strong class="fs-5">${numeroComprobante}</strong>
                </div>
                ${esRepetido ? '<div class="mt-2"><span class="badge bg-warning">Comprobante Repetido</span></div>' : ''}
            </div>
            
            <p class="text-muted mb-0">
                <i class='bx bx-check-circle'></i> El comprobante fue visualizado exitosamente
            </p>
        </div>`,
        function () {
            $("#msjModal").modal("hide");

            // ═══════════════════════════════════════════════════
            // ✅ FLUJO DE LIMPIEZA Y REINICIO (PRESERVANDO BACKUP)
            // ═══════════════════════════════════════════════════

            setTimeout(() => {
                console.log('═══════════════════════════════════════════════════');
                console.log('🔄 INICIANDO REINICIO DEL MÓDULO DE VENTAS v20.3');
                console.log('═══════════════════════════════════════════════════');

                // ❶ PASO 1: Cerrar modal de pago
                if (modalPagoInstance) {
                    modalPagoInstance.hide();
                    console.log('✅ Paso 1: Modal de pago cerrado');
                }

                setTimeout(() => {
                    // ❷ PASO 2: Cerrar modal de cálculo
                    if (typeof cerrarModalCalculoFactura === 'function') {
                        cerrarModalCalculoFactura();
                        console.log('✅ Paso 2: Modal de cálculo cerrado');
                    }

                    setTimeout(() => {
                        // ═══════════════════════════════════════════════════
                        // ✅ CAMBIO CRÍTICO v20.3: PRESERVAR BACKUP
                        // ═══════════════════════════════════════════════════

                        // ❸ PASO 3: Limpiar venta SIN eliminar backup
                        if (typeof limpiarVentaCompleta === 'function') {
                            limpiarVentaCompleta(false); // ← 🚨 PARÁMETRO false = PRESERVAR BACKUP
                            console.log('✅ Paso 3: Módulo de ventas limpiado (backup preservado)');
                        } else {
                            console.error('❌ Función limpiarVentaCompleta no existe');
                        }

                        setTimeout(() => {
                            // ❹ PASO 4: Abrir modal de identificar cliente
                            if (typeof abrirModalIdentificarCliente === 'function') {
                                abrirModalIdentificarCliente();
                                console.log('✅ Paso 4: Modal de identificar cliente abierto');
                            } else {
                                console.error('❌ Función abrirModalIdentificarCliente no existe');
                            }

                            console.log('═══════════════════════════════════════════════════');
                            console.log('✅ REINICIO COMPLETADO - Backup disponible');
                            console.log('═══════════════════════════════════════════════════');

                        }, 200); // Esperar limpieza
                    }, 300); // Esperar cierre de modal cálculo
                }, 300); // Esperar cierre de modal pago
            }, 300); // Esperar cierre de mensaje de éxito
        },
        false,
        ["Aceptar"],
        "succ!",
        null
    );
}

/**
 * ✅ NUEVO v20.2: Procesa un error del servidor
 * Muestra mensaje de error al usuario
 * 
 * @param {Object} response - Respuesta del servidor con error
 */
function procesarErrorPago(response) {
    console.error('═══════════════════════════════════════════════════');
    console.error('❌ PROCESANDO ERROR DE PAGO');
    console.error('═══════════════════════════════════════════════════');
    console.error('Response:', response);

    const mensajeError = response?.mensaje || 'Ocurrió un error al procesar el pago';

    console.error(`   Mensaje: ${mensajeError}`);

    AbrirMensaje(
        "Error al Procesar Pago",
        `<div class="text-start">
            <p class="mb-3">
                <i class='bx bx-error-circle text-danger fs-3'></i>
                <strong class="text-danger">No se pudo procesar el pago</strong>
            </p>
            <div class="alert alert-danger mb-0">
                ${escapeHtml(mensajeError)}
            </div>
        </div>`,
        function () {
            $('#msjModal').modal('hide');
        },
        false,
        ["Aceptar"],
        "error!",
        null
    );
}

/**
 * ✅ REUTILIZADO: Obtiene el tipo de comprobante según letra e ID
 * (Esta función ya existe en prodfactcalc.js, pero la agregamos aquí por si acaso)
 * 
 * @param {string} letra - Letra del comprobante (A, B, C, etc.)
 * @param {string} id - ID del tipo de comprobante
 * @returns {string} - Descripción del tipo de comprobante
 */
function obtenerTipoComprobante(letra, id) {
    const letraNorm = (letra || '').toUpperCase().trim();
    const idNorm = (id || '').trim().replace(/^0+/, '');

    if (letraNorm === 'A' && (idNorm === '7' || id === '007')) return 'Factura A';
    if (letraNorm === 'B' && (idNorm === '6' || id === '006')) return 'Factura B';
    if (letraNorm === 'C' && (idNorm === '11' || id === '011')) return 'Factura C';
    if (letraNorm === 'M' && (idNorm === '51' || id === '051')) return 'Factura M';
    if (letraNorm === 'A' && (idNorm === '8' || id === '008')) return 'Nota de Crédito A';
    if (letraNorm === 'B' && (idNorm === '9' || id === '009')) return 'Nota de Crédito B';
    if (letraNorm === 'A' && (idNorm === '10' || id === '010')) return 'Nota de Débito A';

    return `Comprobante ${letraNorm || 'Desconocido'}`;
}

/**
 * ✅ NUEVO v20.2: Maneja sesión expirada
 * Redirige al login o muestra mensaje
 * 
 * @param {string} mensaje - Mensaje a mostrar
 */
function manejarSesionExpirada(mensaje) {
    console.error('═══════════════════════════════════════════════════');
    console.error('🔒 SESIÓN EXPIRADA DETECTADA');
    console.error('═══════════════════════════════════════════════════');

    AbrirMensaje(
        "Sesión Expirada",
        `<div class="text-center">
            <i class='bx bx-error-circle text-warning' style="font-size: 3rem;"></i>
            <p class="mt-3 mb-0">${escapeHtml(mensaje)}</p>
        </div>`,
        function () {
            $('#msjModal').modal('hide');

            // Redirigir al login después de 1 segundo
            setTimeout(function () {
                window.location.href = '/Account/Login';
            }, 1000);
        },
        false,
        ["Aceptar"],
        "warning",
        null
    );
}

// ═══════════════════════════════════════════════════════════════════
// ✅ NUEVO v25.0: FUNCIONES DE CONTROL DEL TECLADO VIRTUAL
// ═══════════════════════════════════════════════════════════════════

/**
 * ✅ NUEVO v25.0: Posiciona el teclado virtual junto al ancla.
 * Se asegura de que el teclado esté visible y alineado a la izquierda.
 */
function posicionarTecladoVirtual() {
    console.log('📍 Posicionando teclado virtual...');
    const ancla = document.getElementById('teclado-ancla');
    const teclado = document.getElementById('virtual-keyboard');

    if (!teclado) {
        console.error('❌ Teclado virtual no encontrado en el DOM.');
        return;
    }
    if (!ancla) {
        console.error('❌ Ancla #teclado-ancla no encontrada.');
        return;
    }

    // Forzar visibilidad si está oculto
    if (teclado.style.display !== 'flex') {
        teclado.style.display = 'flex';
        teclado.style.opacity = '1';
        console.log('   ✅ Teclado forzado a ser visible.');
    }

    // Calcular posición
    const rectAncla = ancla.getBoundingClientRect();
    const rectTeclado = teclado.getBoundingClientRect();

    // Posicionar el teclado
    // Usamos 'transform' para no interferir con otras propiedades de posicionamiento
    const top = rectAncla.top;
    const left = rectAncla.left;

    teclado.style.position = 'fixed';
    teclado.style.top = `${top}px`;
    teclado.style.left = `${left}px`;
    teclado.style.transform = 'none'; // Resetear transform de arrastre

    console.log(`   ✅ Teclado posicionado en: top=${top.toFixed(0)}px, left=${left.toFixed(0)}px`);
}

/**
 * ✅ NUEVO v25.0: Activa el teclado para un input específico.
 * @param {string} inputSelector - El selector del campo de entrada.
 */
function activarTecladoParaInput(inputSelector) {
    console.log(`⌨️ Activando teclado para: ${inputSelector}`);
    const input = document.querySelector(inputSelector);
    if (!input) {
        console.error(`❌ Input ${inputSelector} no encontrado.`);
        return;
    }

    // 1. Simular foco en el input para que virtual-keyboard.js lo detecte y renderice.
    input.focus();

    // 2. Usar un pequeño delay para asegurar que el teclado se haya renderizado en el DOM.
    setTimeout(() => {
        // 3. Mover el teclado a la posición deseada.
        posicionarTecladoVirtual();

        // 4. Volver a enfocar y seleccionar el contenido del input.
        input.focus();
        input.select();
    }, 150); // 150ms es un delay seguro para la renderización.
}

/**
 * ✅ NUEVO v25.0: Oculta el teclado virtual.
 */
function ocultarTecladoVirtual() {
    const teclado = document.getElementById('virtual-keyboard');
    if (teclado) {
        teclado.style.display = 'none';
        console.log('⌨️ Teclado virtual ocultado.');
    }
}

///**
// * ✅ NUEVO v20.2: Muestra mensaje de error genérico
// * Función auxiliar reutilizable
// * 
// * @param {string} mensaje - Mensaje de error
// */
//function mostrarMensajeError(mensaje) {
//    AbrirMensaje(
//        "Error",
//        mensaje,
//        function () {
//            $("#msjModal").modal("hide");
//        },
//        false,
//        ["Aceptar"],
//        "error!",
//        null
//    );
//}

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
 * ✅ ACTUALIZADO v24.0: Limpiar modal de pago al cerrarse
 * NUEVO: Destruir todos los tooltips activos
 */
function limpiarModalPago() {
    console.log('🧹 Limpiando modal de pago v24.0...');

    // ✅ NUEVO v25.0: Ocultar teclado si está visible
    ocultarTecladoVirtual();

    // ✅ NUEVO: Destruir tooltips activos
    $('#tbodyFormasPago [data-bs-toggle="tooltip"]').each(function () {
        const tooltipInstance = bootstrap.Tooltip.getInstance(this);
        if (tooltipInstance) {
            tooltipInstance.dispose();
        }
    });

    console.log('   ✅ Tooltips destruidos');

    // Limpieza normal (sin cambios)
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
 * ✅ ACTUALIZADO v20.7: Resetear selección de tipo medio de pago
 * CAMBIO: Ya NO manipula botón confirmar (está oculto)
 */
function resetearSeleccionTipoMedioPago() {
    $('.tipo-medio-pago-item').removeClass('selected');
    tipoMedioPagoSeleccionado = null;

    // ❌ ELIMINADO v20.7: No se deshabilita botón confirmar (está oculto)
    // $('#btnConfirmarTipoMedioPago').prop('disabled', true);
}

/**
 * ✅ ACTUALIZADO v20.7: Seleccionar ítem de tipo medio de pago
 * CAMBIO: Ya NO habilita botón confirmar (está oculto)
 */
function seleccionarItemTipoMedioPago($item) {
    $('.tipo-medio-pago-item').removeClass('selected');
    $item.addClass('selected');

    tipoMedioPagoSeleccionado = {
        tcf_id: $item.data('tcf-id'),
        tcf_desc: $item.data('tcf-desc')
    };

    // ❌ ELIMINADO v20.7: No se habilita botón confirmar (está oculto)
    // $('#btnConfirmarTipoMedioPago').prop('disabled', false);

    console.log('✅ Tipo seleccionado:', tipoMedioPagoSeleccionado);
}

/**
 * ✅ CORREGIDO v21.2: Vincular eventos del modal de tipo medio de pago
 * CORRECCIÓN CRÍTICA: Delegación de eventos para navegación con teclado
 * 
 * PROBLEMA RESUELTO:
 * - Primera apertura: Navegación NO funcionaba (evento vinculado antes de modal visible)
 * - Segunda apertura: Funcionaba correctamente (modal ya existía en DOM)
 * 
 * SOLUCIÓN:
 * - Delegación de eventos en document (siempre disponible)
 * - Validación de modal visible antes de procesar teclas
 * 
 * CAMBIOS v21.2:
 * - Movido evento keydown de modal a document (delegación)
 * - Agregada validación `hasClass('show')` antes de procesar
 * - Previene bug de primera apertura sin navegación
 * 
 * CAMBIOS v21.1 (anteriores):
 * - Agregada navegación con teclado (↑↓ Enter Esc)
 * 
 * CAMBIOS v20.7 (anteriores):
 * - Selección con UN SOLO CLICK (sin necesidad de confirmar)
 */
function vincularEventosTipoMedioPago() {
    console.log('🔧 Vinculando eventos tipo medio de pago v21.2...');

    // ═══════════════════════════════════════════════════════════
    // ✅ EVENTO DE CLICK (sin cambios)
    // ═══════════════════════════════════════════════════════════

    $('.tipo-medio-pago-item').off('click').on('click', function () {
        console.log('═══════════════════════════════════════════════════');
        console.log('🖱️ CLICK EN TIPO MEDIO DE PAGO v20.7');
        console.log('═══════════════════════════════════════════════════');

        const $item = $(this);

        // ❶ Seleccionar visualmente el ítem
        seleccionarItemTipoMedioPago($item);

        console.log('   ✅ Ítem seleccionado visualmente');

        // ❷ Confirmar automáticamente después de breve delay (feedback visual)
        setTimeout(() => {
            console.log('   ⏩ Confirmando selección automáticamente...');
            confirmarSeleccionTipoMedioPago();
        }, 200); // ← Delay corto para que usuario vea el resaltado azul
    });

    // ═══════════════════════════════════════════════════════════
    // ✅ CORREGIDO v21.2: NAVEGACIÓN CON TECLADO
    // ═══════════════════════════════════════════════════════════

    console.log('   🔧 Configurando delegación de eventos de teclado...');

    // ❌ ELIMINADO v21.2: Evento directo en modal (causaba bug de primera apertura)
    // $('#modalTipoMedioPago').off('keydown.navegacion').on('keydown.navegacion', function (e) {
    //     manejarNavegacionTeclado(e);
    // });

    // ✅ NUEVO v21.2: Delegación de eventos en document
    $(document)
        .off('keydown.navegacionTipoMP') // Limpiar eventos previos (evita duplicados)
        .on('keydown.navegacionTipoMP', function (e) {
            // ❶ Verificar que el modal esté visible y activo
            const $modal = $('#modalTipoMedioPago');

            if (!$modal.hasClass('show')) {
                // Modal no visible, ignorar evento
                return;
            }

            console.log(`🎹 Tecla presionada en modal visible: ${e.key}`);

            // ❷ Procesar navegación con teclado
            manejarNavegacionTeclado(e);
        });

    console.log('   ✅ Delegación de eventos configurada correctamente');
    console.log('✅ Eventos configurados (selección con 1 click + navegación teclado delegada)');
}

/**
 * ✅ NUEVO v21.2: Limpia eventos de navegación con teclado al cerrar modal
 * 
 * PROPÓSITO:
 * - Prevenir memory leaks por eventos huérfanos
 * - Garantizar que no queden listeners activos después de cerrar modal
 * - Evitar conflictos con otros modales
 * 
 * CUÁNDO SE LLAMA:
 * - Automáticamente al cerrar modal (evento 'hidden.bs.modal')
 * - Ver configuración en inicializarEventosPago()
 */
function limpiarEventosTipoMedioPago() {
    console.log('🧹 Limpiando eventos de navegación con teclado...');

    // ❶ Remover evento delegado de document
    $(document).off('keydown.navegacionTipoMP');

    console.log('   ✅ Evento keydown.navegacionTipoMP removido de document');
    console.log('✅ Eventos de teclado limpiados correctamente');
}

/**
 * ✅ NUEVO v21.1: Maneja la navegación con teclado en el modal de tipo medio de pago
 * 
 * TECLAS SOPORTADAS:
 * - ArrowDown (↓): Mover a siguiente item
 * - ArrowUp (↑): Mover a item anterior
 * - Enter: Confirmar selección actual
 * - Escape: Cerrar modal sin confirmar
 * 
 * COMPORTAMIENTO:
 * - Navegación cíclica (al llegar al final, vuelve al inicio)
 * - Scroll automático si el item está fuera de vista
 * - Integración con funciones existentes (sin duplicar código)
 * 
 * @param {KeyboardEvent} e - Evento de teclado
 */
function manejarNavegacionTeclado(e) {
    // ❶ Validar que el modal esté visible
    if (!$('#modalTipoMedioPago').hasClass('show')) {
        return; // Modal no visible, no procesar
    }

    // ❷ Obtener todos los items visibles
    const $items = $('.tipo-medio-pago-item:visible');

    if ($items.length === 0) {
        console.warn('⚠️ No hay items disponibles para navegar');
        return;
    }

    // ❸ Obtener item actualmente seleccionado
    const $itemActual = $('.tipo-medio-pago-item.selected');

    if ($itemActual.length === 0) {
        console.warn('⚠️ No hay item seleccionado');
        return;
    }

    const indiceActual = $items.index($itemActual);
    const totalItems = $items.length;

    // ❹ Procesar tecla presionada
    switch (e.key) {
        case 'ArrowDown': // ↓ Siguiente
            e.preventDefault(); // Evitar scroll del modal

            console.log('⬇️ FLECHA ABAJO - Siguiente item');

            // Calcular índice siguiente (cíclico)
            const indiceSiguiente = (indiceActual + 1) % totalItems;
            const $itemSiguiente = $items.eq(indiceSiguiente);

            console.log(`   Moviendo de índice ${indiceActual} → ${indiceSiguiente}`);

            // Seleccionar siguiente item
            seleccionarItemTipoMedioPago($itemSiguiente);

            // Hacer scroll si es necesario
            scrollToItem($itemSiguiente, '#listaTiposMedioPago');

            break;

        case 'ArrowUp': // ↑ Anterior
            e.preventDefault();

            console.log('⬆️ FLECHA ARRIBA - Item anterior');

            // Calcular índice anterior (cíclico)
            const indiceAnterior = (indiceActual - 1 + totalItems) % totalItems;
            const $itemAnterior = $items.eq(indiceAnterior);

            console.log(`   Moviendo de índice ${indiceActual} → ${indiceAnterior}`);

            // Seleccionar anterior item
            seleccionarItemTipoMedioPago($itemAnterior);

            // Hacer scroll si es necesario
            scrollToItem($itemAnterior, '#listaTiposMedioPago');

            break;

        case 'Enter': // ⏎ Confirmar
            e.preventDefault();

            console.log('⏎ ENTER - Confirmando selección');

            // Confirmar selección actual (sin delay)
            confirmarSeleccionTipoMedioPago();

            break;

        case 'Escape': // Esc Cancelar
            e.preventDefault();

            console.log('🚫 ESCAPE - Cerrando modal');

            // Cerrar modal con Bootstrap
            if (modalTipoMedioPagoInstance) {
                modalTipoMedioPagoInstance.hide();
            } else {
                $('#modalTipoMedioPago').modal('hide');
            }

            break;

        default:
            // Otras teclas no manejadas
            break;
    }
}

/**
 * ✅ NUEVO v21.1: Hace scroll a un item si está fuera de la vista del contenedor
 * 
 * FUNCIONALIDAD:
 * - Detecta si el item está visible dentro del contenedor scrolleable
 * - Si está fuera de vista, hace scroll suave hasta centrarlo
 * 
 * @param {jQuery} $item - Item al que hacer scroll
 * @param {string} contenedorSelector - Selector del contenedor scrolleable
 */
function scrollToItem($item, contenedorSelector) {
    if (!$item || $item.length === 0) {
        console.warn('⚠️ scrollToItem: Item inválido');
        return;
    }

    const $contenedor = $(contenedorSelector);

    if ($contenedor.length === 0) {
        console.warn(`⚠️ scrollToItem: Contenedor ${contenedorSelector} no encontrado`);
        return;
    }

    // ❶ Obtener posiciones
    const itemTop = $item.position().top;
    const itemBottom = itemTop + $item.outerHeight();
    const contenedorHeight = $contenedor.height();
    const scrollActual = $contenedor.scrollTop();

    // ❷ Verificar si el item está fuera de vista
    const fueraArribа = itemTop < 0;
    const fueraAbajo = itemBottom > contenedorHeight;

    // ❸ Calcular nueva posición de scroll si es necesario
    if (fueraArribа || fueraAbajo) {
        // Centrar el item en el contenedor
        const nuevaPosicion = scrollActual + itemTop - (contenedorHeight / 2) + ($item.outerHeight() / 2);

        console.log(`   📜 Haciendo scroll a posición ${nuevaPosicion.toFixed(0)}px`);

        // Hacer scroll suave
        $contenedor.animate({
            scrollTop: nuevaPosicion
        }, 200); // Animación rápida (200ms)
    } else {
        console.log('   ✅ Item ya está visible, no requiere scroll');
    }
}

/**
 * ✅ SIN CAMBIOS: Confirmar selección de tipo medio de pago
 * Esta función ahora se llama automáticamente desde el evento click
 * (Línea 1820 - Sin modificaciones)
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
 * ✅ ACTUALIZADO v21.0: Renderiza la lista de instrumentos en el modal
 * NUEVO: Selecciona automáticamente el primer item después de renderizar
 */
function renderizarInstrumentos(instrumentos) {
    console.log('═══════════════════════════════════════════════════');
    console.log('🎨 RENDERIZAR INSTRUMENTOS v21.0');
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

    // ❹ ✅ NUEVO v21.0: Seleccionar primer item automáticamente
    setTimeout(() => {
        seleccionarPrimerItemAutomatico({
            contenedorId: '#listaInstrumentos',
            itemClass: '.instrumento-item',
            btnConfirmarId: '#btnConfirmarInstrumento',
            tipoModal: 'instrumentos'
        });
    }, 100); // ← Delay para asegurar que eventos estén vinculados

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

    // ═══════════════════════════════════════════════════════════
    // ✅ ACTUALIZADO v21.3: SELECCIÓN CON UN SOLO CLICK + CONFIRMACIÓN AUTOMÁTICA
    // ═══════════════════════════════════════════════════════════

    /**
     * Evento 'click' en items de instrumentos
     * 
     * CAMBIOS v21.3:
     * - Agregado delay de 200ms antes de confirmar automáticamente
     * - Usuario ve feedback visual (resaltado azul) antes de continuar
     * - Mejora UX: Elimina necesidad de doble click o botón confirmar
     * 
     * INSPIRADO EN: Selección con 1 click del modal tipo medio de pago (v20.7)
     */
    $('.instrumento-item').off('click').on('click', function () {
        console.log('═══════════════════════════════════════════════════');
        console.log('🖱️ CLICK EN INSTRUMENTO v21.3');
        console.log('═══════════════════════════════════════════════════');

        const $item = $(this);

        // ❶ Seleccionar visualmente el ítem
        seleccionarInstrumento($item);

        console.log('   ✅ Ítem seleccionado visualmente');

        // ❷ ✅ NUEVO: Confirmar automáticamente después de breve delay
        setTimeout(() => {
            console.log('   ⏩ Confirmando selección automáticamente...');
            confirmarSeleccionInstrumento(); // ← Función existente (línea ~2340)
        }, 200); // ← Delay de 200ms para feedback visual
    });   

    $('#btnConfirmarInstrumento').off('click').on('click', confirmarSeleccionInstrumento);

    console.log('✅ Eventos vinculados');
}


/**
 * ✅ ACTUALIZADO v19.4: Confirma la selección del instrumento
 * CORRECCIÓN: Agregado case 'BA' para Transferencias Bancarias
 */
function confirmarSeleccionInstrumento() {
    console.log('═══════════════════════════════════════════════════');
    console.log('✅ CONFIRMAR INSTRUMENTO SELECCIONADO v19.4');
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
            case 'EF': // Efectivo ✅ IMPLEMENTADO
                abrirModalDetalleEfectivo(
                    window._instrumentoSeleccionado,
                    window._tipoMedioPagoActual
                );
                break;

            // ✅ LOTE 2 - VALES DE COMPRA (FUNCIONA CORRECTAMENTE)
            case 'VA': // Vales de Compra CA
                abrirModalDetalleValeCompra(
                    window._instrumentoSeleccionado,
                    window._tipoMedioPagoActual
                );
                break;

            // ✅ NUEVO v19.4: LOTE 3 - TRANSFERENCIAS BANCARIAS
            case 'BA': // Transferencias Bancarias
                console.log('✅ Abriendo modal de Transferencia Bancaria...');
                abrirModalDetalleTransferencia(
                    window._instrumentoSeleccionado,
                    window._tipoMedioPagoActual
                );
                break;

            // ✅ NUEVO v19.5: LOTE 4 - ÓRDENES/CUPONES DE MUTUALES
            case 'MU': // Mutuales / Órdenes de Empresa
                console.log('✅ Abriendo modal de Cupón/Orden de Empresa...');
                abrirModalDetalleCuponEmpresa(
                    window._instrumentoSeleccionado,
                    window._tipoMedioPagoActual
                );
                break;
            case 'CH': // ✅ ACTUALIZADO v20.0: Cheques
                console.log('✅ Abriendo modal de Cheque...');
                abrirModalDetalleCheque(
                    window._instrumentoSeleccionado,
                    window._tipoMedioPagoActual
                );
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
 * ✅ ACTUALIZADO v19.1: Procesa los instrumentos según requieran detalle o no
 * NUEVO: Maneja casos especiales de MP que requieren modal obligatorio (VA)
 * 
 * CASOS:
 * 1. Un solo instrumento + MP sin modal obligatorio → agregarValorDirecto()
 * 2. Un solo instrumento + MP con modal obligatorio (VA) → Abrir modal detalle
 * 3. Múltiples instrumentos → abrirModalInstrumentos()
 * 
 * @param {Array} instrumentos - Array de instrumentos disponibles
 * @param {Object} tipoMedioPago - Tipo de medio de pago seleccionado
 */
function procesarInstrumentos(instrumentos, tipoMedioPago) {
    console.log('═══════════════════════════════════════════════════');
    console.log('🔄 PROCESAR INSTRUMENTOS v19.1');
    console.log(`   Tipo MP: ${tipoMedioPago.tcf_desc} (${tipoMedioPago.tcf_id})`);
    console.log(`   Total instrumentos: ${instrumentos.length}`);
    console.log('═══════════════════════════════════════════════════');

    // ❶ Validar que haya instrumentos
    if (!instrumentos || instrumentos.length === 0) {
        console.warn('⚠️ No hay instrumentos disponibles');

        if (typeof toastr !== 'undefined') {
            toastr.info('No hay instrumentos disponibles para este medio de pago');
        }

        return;
    }

    // ❷ Obtener ID del tipo de medio de pago
    const tcfId = tipoMedioPago.tcf_id.toUpperCase();

    // ❸ Clasificar instrumentos por si tienen detalle
    const instrumentosConDetalle = instrumentos.filter(i => i.tiene_detalle === true);
    const instrumentosSinDetalle = instrumentos.filter(i => i.tiene_detalle === false);

    console.log(`📋 Instrumentos con detalle: ${instrumentosConDetalle.length}`);
    console.log(`📋 Instrumentos sin detalle: ${instrumentosSinDetalle.length}`);

    // ❹ ✅ NUEVO v19.1: CASO ESPECIAL - Un solo instrumento
    if (instrumentos.length === 1) {
        const instrumentoUnico = instrumentos[0];

        console.log('═══════════════════════════════════════════════════');
        console.log('🔍 CASO ESPECIAL: UN SOLO INSTRUMENTO DETECTADO');
        console.log(`   Instrumento: ${instrumentoUnico.ins_desc} (${instrumentoUnico.ins_id})`);
        console.log(`   Tipo MP: ${tcfId}`);
        console.log('═══════════════════════════════════════════════════');

        // ❺ Verificar si el MP requiere modal de detalle obligatorio
        if (requiereModalDetalle(tcfId)) {
            console.log('✅ FLUJO ESPECIAL: MP con modal obligatorio');
            console.log(`   Abriendo modal de detalle para ${tcfId}...`);

            // ❻ Abrir modal de detalle según el tipo
            abrirModalDetalleSegunTipo(instrumentoUnico, tipoMedioPago);

        } else {
            console.log('✅ FLUJO NORMAL: Agregar valor directamente');

            // ❼ Si NO tiene detalle → Agregar directo (SweetAlert)
            if (!instrumentoUnico.tiene_detalle) {
                console.log('   → Abriendo SweetAlert simple');
                agregarValorDirecto(instrumentoUnico, tipoMedioPago);
            } else {
                // ❽ Si TIENE detalle → Abrir modal de instrumentos (por precaución)
                console.log('   → Instrumento con detalle - Abriendo modal');
                abrirModalInstrumentos(instrumentos, tipoMedioPago);
            }
        }

        return; // ✅ Salir de la función
    }

    // ❾ CASO NORMAL: Múltiples instrumentos
    console.log('═══════════════════════════════════════════════════');
    console.log('✅ FLUJO NORMAL: MÚLTIPLES INSTRUMENTOS');
    console.log('   → Abriendo modal de selección de instrumentos');
    console.log('═══════════════════════════════════════════════════');

    abrirModalInstrumentos(instrumentos, tipoMedioPago);
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
 * ✅ NUEVO v22.0: Parsea números en formato GeConnect (en-US)
 * 
 * Convierte texto con formato GeConnect a número JavaScript
 * 
 * Ejemplos:
 *   "$ 1,234.56"       → 1234.56
 *   "1,234,567.89"     → 1234567.89
 *   "599.99"           → 599.99
 *   "$ 100,000"        → 100000
 *   "$ -500.50"        → -500.50
 * 
 * Formato GeConnect:
 *   - Separador de miles: , (coma)
 *   - Separador decimal: . (punto)
 *   - Símbolo de moneda: $ (opcional)
 * 
 * @param {string} texto - Texto con formato GeConnect (ej: "$ 1,234.56")
 * @returns {number} - Número parseado en formato estándar (ej: 1234.56)
 */
function parsearNumero(texto) {
    console.log('═══════════════════════════════════════════════════');
    console.log('🔢 PARSEAR NÚMERO GECONNECT v22.0');
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

    // ❹ ✅ CAMBIO CRÍTICO: Eliminar comas (separador de miles en formato GeConnect)
    limpio = limpio.replace(/,/g, '');
    console.log(`   📝 Paso 3 - Sin comas de miles: "${limpio}"`);

    // ❺ ✅ Punto ya es decimal (no requiere transformación)
    console.log(`   📝 Paso 4 - Punto decimal ya correcto: "${limpio}"`);

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
            "Atención",
            mensaje,
            function () { $("#msjModal").modal("hide"); },
            false,
            ["Aceptar"],
            "warn!",
            null
        );
    } else {
        alert(`ERROR: ${mensaje}`);
    }
}

/**
 * ✅ ACTUALIZADO v23.1: Abre el modal de detalle de efectivo SIN InputMask
 * CORRECCIÓN CRÍTICA: Vinculación explícita de botones cancelar/cerrar
 * 
 * CAMBIOS v23.1:
 * - Agregada vinculación de botones cancelar y cerrar (X)
 * - Garantiza cierre correcto incluso sin Bootstrap.Modal
 * 
 * @param {Object} instrumento - Objeto con datos del instrumento
 * @param {Object} tipoMedioPago - Tipo de medio de pago
 */
function abrirModalDetalleEfectivo(instrumento, tipoMedioPago) {
    console.log('═══════════════════════════════════════════════════');
    console.log('🔓 ABRIR MODAL DETALLE EFECTIVO v23.1 (SIN InputMask)');
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

    console.log(`💰 Importe sugerido: ${importeSugerido.toFixed(2)}`);

    // ❹ Obtener input
    const $inputMonto = $('#txtMontoEfectivo');

    // ✅ CRÍTICO v23.0: REMOVER INPUTMASK SI EXISTE
    if (typeof InputMaskMonetario !== 'undefined') {
        console.log('🗑️ Removiendo InputMask existente...');
        InputMaskMonetario.removerMascara($inputMonto);
        console.log('   ✅ InputMask removido correctamente');
    }

    // ❺ ✅ NUEVO v23.0: Establecer valor inicial SIN FORMATO
    $inputMonto.val(importeSugerido.toFixed(2));

    // Activar el teclado para este input
    activarTecladoParaInput('#txtMontoEfectivo');

    console.log(`   ✅ Valor inicial: ${importeSugerido.toFixed(2)}`);
    console.log('   ✅ Teclado digital listo para escribir');

    // ❻ Limpiar validaciones previas
    $inputMonto.removeClass('is-invalid is-valid');
    $('.invalid-feedback').remove();

    // ❼ Mostrar modal con jQuery
    $modal
        .addClass('show')
        .css({
            'display': 'block',
            'opacity': '1',
            'z-index': '5100'
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

        console.log('═══════════════════════════════════════════════════');
        console.log('✅ INPUT LISTO PARA TECLADO DIGITAL:');
        console.log('   - InputMask: DESACTIVADO ❌');
        console.log('   - Teclado digital: ACTIVO ✅');
        console.log('   - Formato: SOLO NÚMEROS (sin $, sin comas)');
        console.log('   - Validación: NATIVA HTML5');
        console.log('═══════════════════════════════════════════════════');
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

    // ═══════════════════════════════════════════════════════════
    // ✅ NUEVO v23.1: VINCULACIÓN EXPLÍCITA DE BOTONES DE CIERRE
    // ═══════════════════════════════════════════════════════════

    /**
     * Maneja el click en los botones de cancelar/cerrar
     * Garantiza cierre correcto del modal abierto con jQuery
     */
    $modal
        .find('[data-bs-dismiss="modal"], .btn-close')
        .off('click.cerrarEfectivo')
        .on('click.cerrarEfectivo', function (e) {
            console.log('═══════════════════════════════════════════════════');
            console.log('🚫 BOTÓN CANCELAR/CERRAR PRESIONADO v23.1');
            console.log('═══════════════════════════════════════════════════');

            e.preventDefault(); // ← Prevenir comportamiento por defecto
            e.stopPropagation(); // ← Detener propagación

            // Cerrar con función dedicada
            cerrarModalDetalleEfectivo();

            console.log('✅ Modal cerrado desde botón cancelar/cerrar');
        });

    console.log('✅ Botones cancelar y cerrar (X) vinculados correctamente');
    console.log('✅ Modal detalle efectivo abierto SIN InputMask');
}

/**
 * ✅ ACTUALIZADO v23.0: Guarda el detalle de efectivo SIN InputMask
 * CAMBIO: Parseo directo del valor sin InputMaskMonetario
 * 
 * LÓGICA:
 * 1. Obtener valor del input como string
 * 2. Limpiar caracteres no numéricos (excepto punto/coma decimal)
 * 3. Parsear con parseFloat()
 * 4. Validar y guardar
 * 
 * @param {Object} instrumento - Objeto con datos del instrumento
 * @param {Object} tipoMedioPago - Tipo de medio de pago
 */
function guardarDetalleEfectivo(instrumento, tipoMedioPago) {
    console.log('═══════════════════════════════════════════════════');
    console.log('💾 GUARDAR DETALLE EFECTIVO v23.0 (SIN InputMask)');
    console.log('═══════════════════════════════════════════════════');

    // ❶ ✅ ACTUALIZADO v23.0: Obtener y limpiar valor del input
    const montoStr = $('#txtMontoEfectivo').val().trim();

    // Limpiar: remover todo excepto dígitos, punto y coma
    // Convertir coma a punto (por si el usuario usó coma decimal)
    const montoLimpio = montoStr.replace(/[^\d.,]/g, '').replace(',', '.');

    // Parsear a número flotante
    const monto = parseFloat(montoLimpio) || 0;

    console.log(`   📝 Valor del input: "${montoStr}"`);
    console.log(`   🔧 Valor limpio: "${montoLimpio}"`);
    console.log(`   💰 Monto parseado: ${monto}`);

    // ❷ Validaciones
    if (isNaN(monto) || monto <= 0) {
        console.warn('⚠️ Monto inválido o cero');
        mostrarErrorCampo('#txtMontoEfectivo', 'Debe ingresar un monto válido mayor a cero');
        return;
    }

    // ❸ Validación de límite máximo (sin cambios)
    const diferencia = Math.abs(conceptosPago.diferencia || 0);

    if (monto > diferencia * LIMITE_PORCENTAJE_DIFERENCIA) {
        console.warn(`⚠️ Monto muy alto: ${monto} > ${diferencia * LIMITE_PORCENTAJE_DIFERENCIA}`);

        const mensajeHtml = `
        <div class="text-start">
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
        </div>
    `;

        AbrirMensaje(
            "¿Monto elevado?",
            mensajeHtml,
            function () {
                $('#msjModal').modal('hide');
                finalizarGuardadoEfectivo(monto, instrumento, tipoMedioPago);
            },
            false,
            ["Continuar", "Corregir"],
            "warn!",
            function () {
                $('#msjModal').modal('hide');
                setTimeout(() => {
                    $('#txtMontoEfectivo').trigger("focus").trigger("select");
                }, 300);
            }
        );

        return;
    }

    // ❹ Si validaciones OK, finalizar guardado (sin cambios)
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

    // ❻ ✅ NUEVO v25.0: Ocultar teclado virtual
    ocultarTecladoVirtual();

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
 * ✅ ACTUALIZADO v24.0: Agrega una fila a la tabla de formas de pago
 * CORRECCIÓN: Observación como tooltip en columna Importe
 * ELIMINADO: Columna <td> de observación
 * 
 * @param {Object} valor - Objeto con datos del valor
 */
function agregarFilaValor(valor) {
    console.log('═══════════════════════════════════════════════════');
    console.log('➕ AGREGAR FILA A TABLA v24.0');
    console.log(`   ID: ${valor.id}`);
    console.log(`   Tipo: ${valor.tcf_desc}`);
    console.log(`   Instrumento: ${valor.ins_desc}`);
    console.log(`   Importe: ${valor.ins_simbolo} ${formatearNumero(valor.importe, 2)}`);
    console.log(`   Observación: "${valor.observacion || 'Sin observaciones'}"`);
    console.log('═══════════════════════════════════════════════════');

    const $tbody = $('#tbodyFormasPago');

    // ❶ Remover fila de "sin valores" si existe
    $('#rowSinFormasPago').remove();

    // ❷ ✅ NUEVO v24.0: Preparar tooltip de observación
    const observacion = valor.observacion || 'Sin observaciones';
    const htmlTooltip = escapeHtml(observacion);

    // Construir atributos de tooltip (Bootstrap 5)
    const tooltipAttrs = observacion && observacion !== 'Sin observaciones'
        ? `data-bs-toggle="tooltip" 
           data-bs-placement="top" 
           data-bs-html="true" 
           data-bs-title="${htmlTooltip}"
           style="cursor: help; border-bottom: 1px dotted #28a745;"`
        : '';

    console.log(`   📊 Tooltip: ${tooltipAttrs ? 'SÍ' : 'NO'}`);

    // ❸ ✅ ACTUALIZADO: Construir HTML de la fila SIN columna observación
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
            
            <!-- ✅ Importe CON TOOLTIP de observación -->
            <td class="text-end align-middle">
                <span class="fw-bold fs-5 text-success" ${tooltipAttrs}>
                    ${escapeHtml(valor.ins_simbolo)} ${formatearNumero(valor.importe, 2)}
                    ${tooltipAttrs ? '<i class="bx bx-info-circle ms-1 text-muted" style="font-size: 0.9rem;"></i>' : ''}
                </span>
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

    // ❹ Agregar fila al tbody
    $tbody.append(filaHtml);

    // ❺ ✅ NUEVO v24.0: Inicializar tooltips de Bootstrap 5
    setTimeout(() => {
        const tooltipTriggerList = $tbody.find('[data-bs-toggle="tooltip"]').toArray();

        tooltipTriggerList.forEach(function (tooltipTriggerEl) {
            // Destruir tooltip previo si existe (evita duplicados)
            const existingTooltip = bootstrap.Tooltip.getInstance(tooltipTriggerEl);
            if (existingTooltip) {
                existingTooltip.dispose();
            }

            // Crear nuevo tooltip
            new bootstrap.Tooltip(tooltipTriggerEl, {
                container: 'body', // ← CRÍTICO: Evita problemas con overflow
                trigger: 'hover focus' // ← Activar con hover y focus (accesibilidad)
            });
        });

        console.log(`   ✅ ${tooltipTriggerList.length} tooltip(s) inicializado(s)`);
    }, 100); // ← Delay para asegurar que el elemento está en el DOM

    // ❻ Actualizar badge de cantidad
    const cantidadValores = valoresPago.length;
    $('#badgeCantidadPagos').text(`${cantidadValores} ${cantidadValores === 1 ? 'valor' : 'valores'}`);

    console.log('✅ Fila agregada correctamente');
}

/**
 * ✅ ACTUALIZADO v21.4: Actualiza los totales del modal de pago
 * CORRECCIÓN CRÍTICA: Control dual de botones Agregar y Finalizar
 * 
 * CAMBIOS v21.4:
 * - Agregada lógica de habilitación/deshabilitación del botón "Agregar"
 * - Lógica mejorada para botón "Finalizar" (valida vuelto con efectivo)
 * - Logs detallados de decisión de estados
 * 
 * REGLAS DE NEGOCIO:
 * 
 * BOTÓN AGREGAR:
 *   - ✅ Habilitado: diferencia > 0 (falta pagar)
 *   - ❌ Deshabilitado: diferencia <= 0 (exacto o vuelto)
 * 
 * BOTÓN FINALIZAR:
 *   - ✅ Habilitado si:
 *     • diferencia === 0 (pago exacto) O
 *     • diferencia < 0 (vuelto) Y solo efectivo
 *   - ❌ Deshabilitado en cualquier otro caso
 */
function actualizarTotalesPago() {
    console.log('═══════════════════════════════════════════════════');
    console.log('🔄 ACTUALIZAR TOTALES PAGO v21.4');
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

    // ═══════════════════════════════════════════════════════════
    // ✅ NUEVO v21.4: LÓGICA MEJORADA DE HABILITACIÓN DE BOTONES
    // ═══════════════════════════════════════════════════════════

    console.log('═══════════════════════════════════════════════════');
    console.log('🔧 EVALUANDO ESTADO DE BOTONES v21.4');

    // ❻ Validar si se puede finalizar
    let puedeFinalizar = false;

    if (valoresPago.length === 0) {
        // ═══════════════════════════════════════════════════════
        // CASO 1: Sin valores ingresados
        // ═══════════════════════════════════════════════════════
        puedeFinalizar = false;
        console.log('   ❌ Sin valores ingresados → Finalizar DESHABILITADO');
    } else if (Math.abs(diferencia) < 0.01) {
        // ═══════════════════════════════════════════════════════
        // CASO 2: Diferencia exacta ($0.00)
        // ═══════════════════════════════════════════════════════
        puedeFinalizar = true;
        console.log('   ✅ Diferencia exacta → Finalizar HABILITADO');
    } else if (diferencia < 0) {
        // ═══════════════════════════════════════════════════════
        // CASO 3: Diferencia negativa (vuelto)
        // ═══════════════════════════════════════════════════════
        console.log('   🔍 Diferencia negativa (vuelto) → Validando tipos de pago...');

        const tiposPago = valoresPago.map(v => v.tcf_id.toUpperCase());
        const tieneSoloEfectivo = tiposPago.every(tipo => tipo === 'EF');

        console.log(`      Tipos de pago: ${tiposPago.join(', ')}`);
        console.log(`      Solo efectivo: ${tieneSoloEfectivo ? 'SÍ' : 'NO'}`);

        if (tieneSoloEfectivo) {
            puedeFinalizar = true;
            console.log('   ✅ Solo efectivo con vuelto → Finalizar HABILITADO');
        } else {
            puedeFinalizar = false;
            console.log('   ❌ Vuelto con medios no permitidos → Finalizar DESHABILITADO');
        }
    } else {
        // ═══════════════════════════════════════════════════════
        // CASO 4: Diferencia positiva (falta pagar)
        // ═══════════════════════════════════════════════════════
        puedeFinalizar = false;
        console.log('   ❌ Diferencia positiva (falta pagar) → Finalizar DESHABILITADO');
        console.log(`      Falta pagar: ${formatearMoneda(diferencia)}`);
    }

    // ❼ ✅ NUEVO v21.4: Validar si se puede agregar
    const puedeAgregar = diferencia > 0;

    if (puedeAgregar) {
        console.log(`   ✅ Diferencia > 0 → Agregar HABILITADO`);
        console.log(`      Monto faltante: ${formatearMoneda(diferencia)}`);
    } else if (Math.abs(diferencia) < 0.01) {
        console.log('   ❌ Diferencia exacta → Agregar DESHABILITADO');
    } else {
        console.log('   ❌ Diferencia negativa (vuelto) → Agregar DESHABILITADO');
        console.log(`      Vuelto: ${formatearMoneda(Math.abs(diferencia))}`);
    }

    console.log('─────────────────────────────────────────────────');
    console.log(`   🎚️ ESTADO FINAL:`);
    console.log(`      Botón Agregar: ${puedeAgregar ? '✅ HABILITADO' : '❌ DESHABILITADO'}`);
    console.log(`      Botón Finalizar: ${puedeFinalizar ? '✅ HABILITADO' : '❌ DESHABILITADO'}`);
    console.log('═══════════════════════════════════════════════════');

    // ❽ Aplicar estados a los botones
    $('#btnAgregarPago').prop('disabled', !puedeAgregar);
    $('#btnFinalizarPago').prop('disabled', !puedeFinalizar);

    // ═══════════════════════════════════════════════════════════
    // ✅ MEJORA UX: FINALIZACIÓN AUTOMÁTICA
    // ═══════════════════════════════════════════════════════════
    // Si el pago está completo (diferencia es cero) y hay al menos un valor,
    // se dispara la finalización automáticamente para ahorrar un clic.
    if (puedeFinalizar && Math.abs(diferencia) < 0.01 && valoresPago.length > 0) {
        console.log('═══════════════════════════════════════════════════');
        console.log('🚀 DISPARANDO FINALIZACIÓN AUTOMÁTICA');
        console.log('   Razón: El pago está completo (diferencia cero)');
        console.log('═══════════════════════════════════════════════════');

        // Usamos un pequeño timeout para que el usuario perciba la actualización
        // de la UI (diferencia en $0.00) antes de que comience el proceso final.
        setTimeout(() => {
            finalizarPago();
        }, 500); // 500ms de delay para una mejor UX
    }

    console.log('✅ Totales y botones actualizados correctamente');
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
    $campo.trigger('focus').trigger('select');
}

/**
 * ✅ ACTUALIZADO v23.1: Cierra el modal y dispara evento de limpieza
 */
function cerrarModalDetalleEfectivo() {
    console.log('═══════════════════════════════════════════════════');
    console.log('🔒 CERRAR MODAL DETALLE EFECTIVO v23.1');
    console.log('═══════════════════════════════════════════════════');

    const $modal = $('#modalDetalleEfectivo');

    if ($modal.length === 0) {
        console.warn('⚠️ Modal #modalDetalleEfectivo no encontrado');
        return;
    }

    // ❶ Verificar si el modal está visible
    const estaVisible = $modal.hasClass('show');
    console.log(`   Modal visible: ${estaVisible ? 'SÍ' : 'NO'}`);

    if (!estaVisible) {
        console.log('   ℹ️ Modal ya está cerrado');
        return;
    }

    // ❷ Ocultar modal
    $modal
        .removeClass('show')
        .css('display', 'none')
        .attr('aria-hidden', 'true')
        .removeAttr('aria-modal');

    console.log('   ✅ Modal ocultado');

    // ❸ Remover backdrop específico
    const $backdropEfectivo = $('.modal-backdrop[data-modal="efectivo"]');
    if ($backdropEfectivo.length > 0) {
        $backdropEfectivo.fadeOut(200, function () {
            $(this).remove();
        });
        console.log('   ✅ Backdrop removido');
    }

    // ❹ ✅ NUEVO v23.1: DISPARAR EVENTO HIDDEN.BS.MODAL MANUALMENTE
    // Como el modal se abre con jQuery puro, debemos disparar el evento manualmente
    console.log('   🔔 Disparando evento hidden.bs.modal...');
    $modal.trigger('hidden.bs.modal');
    console.log('   ✅ Evento disparado - Limpieza automática ejecutada');

    // ❺ Verificar si hay otros modales abiertos
    setTimeout(() => {
        const modalesAbiertos = $('.modal.show').length;
        console.log(`   📊 Modales abiertos restantes: ${modalesAbiertos}`);

        if (modalesAbiertos === 0) {
            $('body').removeClass('modal-open').css('overflow', '');
            console.log('   ✅ Body desbloqueado');
        } else {
            console.log('   ℹ️ Otros modales abiertos');
        }
    }, 100);

    console.log('✅ MODAL CERRADO COMPLETAMENTE');
}

/**
 * ✅ ACTUALIZADO v24.0: Elimina un valor de la tabla
 * NUEVO: Destruye tooltips antes de eliminar fila
 * 
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
    const mensajeHtml = `
        <strong>${escapeHtml(valor.tcf_desc)} - ${escapeHtml(valor.ins_desc)}</strong><br>
        Importe: ${escapeHtml(valor.ins_simbolo)} ${formatearNumero(valor.importe, 2)}
    `;

    AbrirMensaje(
        "¿Eliminar valor?",
        mensajeHtml,
        function () {
            // Botón "Eliminar"
            $('#msjModal').modal('hide');

            // ✅ NUEVO v24.0: Destruir tooltips antes de eliminar
            const $fila = $(`.fila-valor[data-valor-id="${valorId}"]`);

            $fila.find('[data-bs-toggle="tooltip"]').each(function () {
                const tooltipInstance = bootstrap.Tooltip.getInstance(this);
                if (tooltipInstance) {
                    tooltipInstance.dispose();
                    console.log('   ✅ Tooltip destruido antes de eliminar fila');
                }
            });

            // Remover del array
            valoresPago.splice(index, 1);

            // Remover fila del DOM con animación
            $fila.fadeOut(300, function () {
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

            if (typeof toastr !== 'undefined') {
                toastr.success('Valor eliminado correctamente');
            }
        },
        false,
        ["Eliminar", "Cancelar"],
        "warn!",
        function () {
            // Botón "Cancelar"
            $('#msjModal').modal('hide');
        }
    );
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
 * ✅ ACTUALIZADO v22.0: Alias de compatibilidad (DEPRECADO)
 * 
 * MIGRACIÓN:
 * - Esta función ahora es un wrapper de parsearNumero()
 * - Se mantiene por compatibilidad con código legacy
 * - RECOMENDACIÓN: Migrar a parsearNumero() en nuevas implementaciones
 * 
 * @deprecated Usar parsearNumero() en su lugar
 * @param {string} texto - Texto con formato monetario
 * @returns {number} - Número parseado
 */
function parsearNumeroArgentino(texto) {
    console.warn('⚠️ DEPRECADO: parsearNumeroArgentino() → Usar parsearNumero()');
    return parsearNumero(texto);
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

    // ✅ ACTUALIZADO v22.0: Usar parseo GeConnect
    const totalFinal = parsearNumero(totalFinalTexto);

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

// ═══════════════════════════════════════════════════════════════════
// ✅ NUEVO v19.0: LOTE 2 - FUNCIONES PARA VALES DE COMPRA
// ═══════════════════════════════════════════════════════════════════

/**
 * ✅ ACTUALIZADO v24.2: Abre el modal de detalle de Vale de Compra SIN InputMask
 * CORRECCIÓN CRÍTICA: Eliminado InputMask para compatibilidad con teclado digital
 * 
 * CAMBIOS v24.2:
 * - Removido InputMaskMonetario del input monto
 * - Establecer valor inicial SIN FORMATO (solo números)
 * - Teclado digital escribe correctamente
 * 
 * @param {Object} instrumento - Objeto con datos del instrumento (vale seleccionado)
 * @param {Object} tipoMedioPago - Tipo de medio de pago (tcf_id='VA')
 */
function abrirModalDetalleValeCompra(instrumento, tipoMedioPago) {
    console.log('═══════════════════════════════════════════════════');
    console.log('🔓 ABRIR MODAL DETALLE VALE DE COMPRA v24.2 (SIN InputMask)');
    console.log(`   Instrumento: ${instrumento.ins_desc} (${instrumento.ins_id})`);
    console.log(`   Tipo MP: ${tipoMedioPago.tcf_desc}`);
    console.log('═══════════════════════════════════════════════════');

    // ❶ Validar objeto instrumento
    if (!instrumento) {
        console.error('❌ CRÍTICO: Objeto instrumento es null/undefined');
        if (typeof toastr !== 'undefined') {
            toastr.error('Error: No se pudo cargar la información del vale');
        }
        return;
    }

    // ❷ Obtener elemento del modal
    const $modal = $('#modalDetalleValeCompra');

    if ($modal.length === 0) {
        console.error('❌ Modal #modalDetalleValeCompra no encontrado');

        if (typeof toastr !== 'undefined') {
            toastr.error('El modal de vales de compra no está disponible');
        }

        return;
    }

    // ❸ Hidratar información del vale seleccionado
    $('#lblValeCompraSeleccionado').text(instrumento.ins_desc || 'Vale sin nombre');

    // ❹ Obtener saldo con validación defensiva
    let saldoDisponible = parseFloat(instrumento.total_actual) || 0;

    // ❺ Obtener diferencia de factura
    const diferenciaFactura = Math.abs(conceptosPago.diferencia || 0);

    console.log('═══════════════════════════════════════════════════');
    console.log('💰 ANÁLISIS DE SALDOS:');
    console.log(`   Saldo del vale (instrumento.total_actual): ${saldoDisponible}`);
    console.log(`   Diferencia de factura (conceptosPago.diferencia): ${diferenciaFactura}`);
    console.log('═══════════════════════════════════════════════════');

    // ❻ Fallback - Si saldo del vale es 0 → Usar diferencia de factura
    let usandoFallback = false;

    if (saldoDisponible <= 0 && diferenciaFactura > 0) {
        console.warn('⚠️ FALLBACK ACTIVADO:');
        console.warn(`   Saldo del vale es 0, pero diferencia de factura es ${formatearMoneda(diferenciaFactura)}`);
        console.warn('   → Asignando diferencia de factura como saldo disponible');

        saldoDisponible = diferenciaFactura;
        usandoFallback = true;

        if (typeof toastr !== 'undefined') {
            toastr.warning(
                `El vale no tiene saldo registrado. Se usará el saldo de la factura (${formatearMoneda(diferenciaFactura)}) como límite máximo.`,
                'Saldo no disponible',
                { timeOut: 5000, extendedTimeOut: 2000 }
            );
        }
    }

    // ❼ Mostrar saldo disponible en el modal
    $('#lblSaldoValeCompra').text(formatearMoneda(saldoDisponible));
    $('#hdnSaldoValeCompra').val(saldoDisponible);
    $('#hdnIdValeCompra').val(instrumento.ins_id);

    console.log(`   ✅ Saldo final a mostrar: ${formatearMoneda(saldoDisponible)} ${usandoFallback ? '(FALLBACK)' : '(REAL)'}`);

    // ❽ Cambiar color del saldo según el monto
    const $lblSaldo = $('#lblSaldoValeCompra');
    $lblSaldo.removeClass('text-success text-warning text-danger text-info');

    if (usandoFallback) {
        $lblSaldo.addClass('text-info');
    } else if (saldoDisponible > 1000) {
        $lblSaldo.addClass('text-success');
    } else if (saldoDisponible > 0) {
        $lblSaldo.addClass('text-warning');
    } else {
        $lblSaldo.addClass('text-danger');
    }

    // ❾ Calcular importe sugerido (usar el menor entre saldo y diferencia)
    let importeSugerido = Math.min(saldoDisponible, diferenciaFactura);

    console.log('═══════════════════════════════════════════════════');
    console.log('💵 CÁLCULO DE IMPORTE SUGERIDO:');
    console.log(`   Saldo disponible: ${formatearMoneda(saldoDisponible)}`);
    console.log(`   Diferencia factura: ${formatearMoneda(diferenciaFactura)}`);
    console.log(`   → Importe sugerido (menor de ambos): ${formatearMoneda(importeSugerido)}`);
    console.log('═══════════════════════════════════════════════════');

    // ❿ Validar que el importe sugerido sea válido
    if (importeSugerido <= 0) {
        console.error('❌ CRÍTICO: Importe sugerido es 0 o negativo');

        if (typeof toastr !== 'undefined') {
            toastr.error('No hay saldo disponible para aplicar. Verifique los datos del vale.');
        }

        setTimeout(() => {
            cerrarModalDetalleValeCompra();
        }, 2000);

        return;
    }

    // ⓫ Obtener input
    const $inputMonto = $('#txtMontoValeCompra');

    // ✅ CRÍTICO v24.2: REMOVER INPUTMASK SI EXISTE
    if (typeof InputMaskMonetario !== 'undefined') {
        console.log('🗑️ Removiendo InputMask existente...');
        InputMaskMonetario.removerMascara($inputMonto);
        console.log('   ✅ InputMask removido correctamente');
    }

    // ⓬ ✅ NUEVO v24.2: Establecer valor inicial SIN FORMATO
    $inputMonto.val(importeSugerido.toFixed(2));

    console.log(`   ✅ Valor inicial: ${importeSugerido.toFixed(2)}`);
    console.log('   ✅ Teclado digital listo para escribir');

    // ⓭ Limpiar validaciones previas
    $inputMonto.removeClass('is-invalid is-valid');
    $('.invalid-feedback').remove();

    // ⓮ Mostrar modal con jQuery
    $modal
        .addClass('show')
        .css({
            'display': 'block',
            'opacity': '1',
            'z-index': '5100'
        })
        .attr('aria-modal', 'true')
        .removeAttr('aria-hidden');

    // ⓯ Crear backdrop
    if ($('.modal-backdrop[data-modal="valecompra"]').length === 0) {
        $('body').append(
            '<div class="modal-backdrop fade show" ' +
            'data-modal="valecompra" ' +
            'style="z-index: 5099;"></div>'
        );
    }

    // ⓰ Focus en el input
    setTimeout(() => {
        $inputMonto.trigger("focus").trigger("select");
    }, INPUT_FOCUS_TIMEOUT);

    // ⓱ Vincular eventos de guardar
    $('#btnGuardarDetalleValeCompra')
        .off('click.guardarVale')
        .on('click.guardarVale', function () {
            guardarDetalleValeCompra(instrumento, tipoMedioPago);
        });

    // ⓲ Vincular evento Enter
    $inputMonto
        .off('keypress.enterVale')
        .on('keypress.enterVale', function (e) {
            if (e.which === 13) {
                e.preventDefault();
                guardarDetalleValeCompra(instrumento, tipoMedioPago);
            }
        });

    console.log('═══════════════════════════════════════════════════');
    console.log('✅ INPUT LISTO PARA TECLADO DIGITAL:');
    console.log('   - InputMask: DESACTIVADO ❌');
    console.log('   - Teclado digital: ACTIVO ✅');
    console.log('   - Formato: SOLO NÚMEROS (sin $, sin comas)');
    console.log('   - Validación: NATIVA HTML5');
    console.log('═══════════════════════════════════════════════════');

    console.log('✅ Modal detalle vale de compra abierto SIN InputMask');
}

/**
 * ✅ ACTUALIZADO v24.2: Guarda el detalle del vale de compra SIN InputMask
 * CAMBIO CRÍTICO: Parseo directo del valor sin InputMaskMonetario
 * 
 * LÓGICA:
 * 1. Obtener valor del input como string
 * 2. Limpiar caracteres no numéricos (excepto punto/coma decimal)
 * 3. Parsear con parseFloat()
 * 4. Validar y guardar
 * 
 * @param {Object} instrumento - Datos del vale
 * @param {Object} tipoMedioPago - Tipo de medio de pago
 */
function guardarDetalleValeCompra(instrumento, tipoMedioPago) {
    console.log('═══════════════════════════════════════════════════');
    console.log('💾 GUARDAR DETALLE VALE DE COMPRA v24.2 (SIN InputMask)');
    console.log('═══════════════════════════════════════════════════');

    // ❶ ✅ ACTUALIZADO v24.2: Obtener y limpiar valor del input
    const montoStr = $('#txtMontoValeCompra').val().trim();

    // Limpiar: remover todo excepto dígitos, punto y coma
    // Convertir coma a punto (por si el usuario usó coma decimal)
    const montoLimpio = montoStr.replace(/[^\d.,]/g, '').replace(',', '.');

    // Parsear a número flotante
    const monto = parseFloat(montoLimpio) || 0;

    console.log(`   📝 Valor del input: "${montoStr}"`);
    console.log(`   🔧 Valor limpio: "${montoLimpio}"`);
    console.log(`   💰 Monto parseado: ${monto}`);

    // ❂ Validaciones (sin cambios)
    if (isNaN(monto) || monto <= 0) {
        console.warn('⚠️ Monto inválido o cero');
        mostrarErrorCampo('#txtMontoValeCompra', 'Debe ingresar un monto válido mayor a cero');
        return;
    }

    // ❸ Obtener saldo del vale desde el hidden field
    const saldoValeStr = $('#hdnSaldoValeCompra').val();
    const saldoVale = parseFloat(saldoValeStr) || 0;

    console.log(`   💰 Saldo del vale (desde hidden): ${saldoVale}`);

    // ❹ Validación mejorada del saldo
    if (saldoVale <= 0) {
        console.error('❌ CRÍTICO: Saldo del vale es 0 o negativo');
        console.error(`   Valor recibido: ${saldoValeStr}`);
        console.error(`   Valor parseado: ${saldoVale}`);

        mostrarErrorCampo(
            '#txtMontoValeCompra',
            'El saldo del vale no está disponible. Recargue la página e intente nuevamente.'
        );

        if (typeof toastr !== 'undefined') {
            toastr.error('Saldo del vale no disponible', 'Error crítico');
        }

        return;
    }

    // ❺ Validar monto <= saldo disponible del vale
    if (monto > saldoVale) {
        console.warn(`⚠️ Monto supera saldo del vale: ${monto} > ${saldoVale}`);

        mostrarErrorCampo(
            '#txtMontoValeCompra',
            `El monto supera el saldo disponible (${formatearMoneda(saldoVale)})`
        );

        return;
    }

    // ❻ Validar monto <= saldo factura (con tolerancia)
    const diferenciaFactura = Math.abs(conceptosPago.diferencia || 0);

    console.log(`   📊 Diferencia de factura: ${formatearMoneda(diferenciaFactura)}`);

    if (monto > diferenciaFactura * LIMITE_PORCENTAJE_DIFERENCIA) {
        console.warn(`⚠️ Monto muy alto: ${monto} > ${diferenciaFactura * LIMITE_PORCENTAJE_DIFERENCIA}`);

        const mensajeHtml = `
        <div class="text-start">
            <p class="mb-3">El monto ingresado es <strong>mayor</strong> a la diferencia pendiente:</p>
            <table class="table table-sm table-borderless mb-0">
                <tr>
                    <td class="text-end">Monto ingresado:</td>
                    <td class="text-start"><strong class="text-danger">${formatearMoneda(monto)}</strong></td>
                </tr>
                <tr>
                    <td class="text-end">Diferencia pendiente:</td>
                    <td class="text-start"><strong class="text-warning">${formatearMoneda(diferenciaFactura)}</strong></td>
                </tr>
                <tr>
                    <td class="text-end">Excedente:</td>
                    <td class="text-start"><strong class="text-info">${formatearMoneda(monto - diferenciaFactura)}</strong></td>
                </tr>
            </table>
            <p class="mt-3 mb-0"><i class="bx bx-info-circle"></i> ¿Desea continuar?</p>
        </div>
    `;

        AbrirMensaje(
            "¿Monto elevado?",
            mensajeHtml,
            function () {
                $('#msjModal').modal('hide');
                finalizarGuardadoValeCompra(monto, instrumento, tipoMedioPago);
            },
            false,
            ["Continuar", "Corregir"],
            "warn!",
            function () {
                $('#msjModal').modal('hide');
                setTimeout(() => {
                    $('#txtMontoValeCompra').trigger("focus").trigger("select");
                }, 300);
            }
        );

        return;
    }

    // ❼ Si validaciones OK, finalizar guardado
    finalizarGuardadoValeCompra(monto, instrumento, tipoMedioPago);
}

/**
 * ✅ NUEVO v19.0: Finaliza el guardado del vale de compra
 * @param {number} monto - Monto validado
 * @param {Object} instrumento - Datos del vale
 * @param {Object} tipoMedioPago - Tipo de medio de pago
 */
function finalizarGuardadoValeCompra(monto, instrumento, tipoMedioPago) {
    console.log('✅ Finalizando guardado de vale de compra...');
    console.log(`   Monto: ${monto}`);
    console.log(`   Vale: ${instrumento.ins_desc}`);

    // ❶ Crear objeto de valor
    const nuevoValor = {
        id: ++valorIdCounter,
        tcf_id: tipoMedioPago.tcf_id,
        tcf_desc: tipoMedioPago.tcf_desc,
        ins_id: instrumento.ins_id,
        ins_desc: instrumento.ins_desc,
        ins_simbolo: instrumento.ins_simbolo || '$',
        importe: monto,
        observacion: '',
        detalle: {
            // Datos específicos del vale
            id_vale: instrumento.ins_id,
            saldo_anterior: instrumento.total_actual,
            saldo_nuevo: instrumento.total_actual - monto
        },
        fecha_creacion: new Date().toISOString()
    };

    console.log('📦 Nuevo valor creado:', nuevoValor);

    // ❷ Agregar a array global
    valoresPago.push(nuevoValor);

    // ❸ Agregar fila a la tabla
    agregarFilaValor(nuevoValor);

    // ❹ Actualizar totales
    actualizarTotalesPago();

    // ❺ Actualizar total del instrumento
    actualizarTotalInstrumento(instrumento.ins_id, -monto); // NOTA: Restar porque es consumo del vale

    // ❻ Cerrar modal
    cerrarModalDetalleValeCompra();

    // ❼ Notificación
    if (typeof toastr !== 'undefined') {
        toastr.success(
            `Vale agregado: ${formatearMoneda(monto)}`,
            'Valor guardado',
            { timeOut: 3000 }
        );
    }

    console.log('✅ Valor de vale de compra guardado correctamente');
}

/**
 * ✅ ACTUALIZADO v19.8: Cierra el modal de detalle de vale de compra
 * CORRECCIÓN CRÍTICA: Limpieza robusta de backdrops y estado del body
 * 
 * CAMBIOS v19.8:
 * - Agregada detección y limpieza de backdrops huérfanos
 * - Forzado de removeClass('modal-open') en body
 * - Logs exhaustivos para debugging
 * - Timeout de seguridad para backdrops persistentes
 */
function cerrarModalDetalleValeCompra() {
    console.log('═══════════════════════════════════════════════════');
    console.log('🔒 CERRAR MODAL DETALLE VALE DE COMPRA v19.8');
    console.log('═══════════════════════════════════════════════════');

    const $modal = $('#modalDetalleValeCompra');

    if ($modal.length === 0) {
        console.warn('⚠️ Modal #modalDetalleValeCompra no encontrado');
        return;
    }

    // ❶ Verificar si el modal está visible
    const estaVisible = $modal.hasClass('show');
    console.log(`   Modal visible: ${estaVisible ? 'SÍ' : 'NO'}`);

    if (!estaVisible) {
        console.log('   ℹ️ Modal ya está cerrado - No es necesario cerrar');
        return;
    }

    // ❷ Ocultar modal
    $modal
        .removeClass('show')
        .css('display', 'none')
        .attr('aria-hidden', 'true')
        .removeAttr('aria-modal');

    console.log('   ✅ Modal ocultado');

    // ❸ ✅ NUEVO v19.8: Limpieza exhaustiva de backdrops
    console.log('   🧹 Limpiando backdrops...');

    // Buscar backdrop específico del modal
    const $backdropVale = $('.modal-backdrop[data-modal="valecompra"]');
    console.log(`      Backdrops específicos encontrados: ${$backdropVale.length}`);

    if ($backdropVale.length > 0) {
        $backdropVale.fadeOut(200, function () {
            $(this).remove();
            console.log('      ✅ Backdrop específico removido');
        });
    }

    // ❹ ✅ NUEVO v19.8: Verificar backdrops huérfanos con delay
    setTimeout(() => {
        const modalesAbiertos = $('.modal.show').length;
        console.log(`   📊 Modales abiertos restantes: ${modalesAbiertos}`);

        if (modalesAbiertos === 0) {
            // ✅ NO hay otros modales abiertos → Limpiar TODO
            const backdropsHuerfanos = $('.modal-backdrop').length;
            console.log(`   📊 Backdrops huérfanos detectados: ${backdropsHuerfanos}`);

            if (backdropsHuerfanos > 0) {
                console.warn('   ⚠️ Backdrops huérfanos encontrados - Limpiando...');

                $('.modal-backdrop').fadeOut(150, function () {
                    $(this).remove();
                });

                console.log('      ✅ Backdrops huérfanos removidos');
            }

            // ✅ Desbloquear body
            $('body')
                .removeClass('modal-open')
                .css({
                    'overflow': '',
                    'padding-right': ''
                });

            console.log('   ✅ Body desbloqueado completamente');

        } else {
            console.log('   ℹ️ Otros modales abiertos - Manteniendo body bloqueado');
        }
    }, 350); // ← Esperar animación de Bootstrap (300ms) + margen

    // ❺ Limpiar formulario
    const $input = $('#txtMontoValeCompra');
    $input
        .val('')
        .removeClass('is-invalid is-valid')
        .prop('disabled', false);

    $('.invalid-feedback').remove();

    console.log('   ✅ Formulario limpiado');

    // ❻ Resetear labels y hidden fields
    $('#lblValeCompraSeleccionado').text('-');
    $('#lblSaldoValeCompra')
        .text('$ 0,00')
        .removeClass('text-success text-warning text-danger text-info');
    $('#hdnIdValeCompra').val('');
    $('#hdnSaldoValeCompra').val('0');

    console.log('   ✅ Labels y campos ocultos reseteados');

    console.log('═══════════════════════════════════════════════════');
    console.log('✅ MODAL CERRADO COMPLETAMENTE');
    console.log('═══════════════════════════════════════════════════');
}

/**
 * ✅ ACTUALIZADO v22.0: Formatea valor a moneda GeConnect (en-US)
 * CAMBIO CRÍTICO: Reemplazado 'es-AR' por 'en-US'
 * 
 * Formato GeConnect:
 * - Separador de miles: , (coma)
 * - Separador decimal: . (punto)
 * - Ejemplo: $ 1,234.56
 * 
 * @param {number} valor - Valor numérico
 * @returns {string} - Valor formateado (ej: "$ 1,234.56")
 */
function formatearMoneda(valor) {
    if (isNaN(valor)) {
        console.warn(`⚠️ formatearMoneda: entrada inválida (${valor})`);
        return '$ 0.00';
    }

    // ✅ Usar formato en-US con símbolo $ genérico
    return new Intl.NumberFormat('en-US', {
        style: 'currency',
        currency: 'USD',
        minimumFractionDigits: 2,
        maximumFractionDigits: 2
    }).format(valor || 0).replace('$', '$ '); // Espacio después del $
}

/**
 * ✅ ACTUALIZADO v19.3: Determina si un tipo de medio de pago requiere SIEMPRE modal de detalle
 * NUEVO: Agregado 'BA' (Transferencias Bancarias)
 * 
 * @param {string} tcfId - ID del tipo de medio de pago
 * @returns {boolean} - true si requiere modal de detalle obligatorio
 */
function requiereModalDetalle(tcfId) {
    console.log(`🔍 Verificando si ${tcfId} requiere modal de detalle...`);

    const tiposConModalObligatorio = [
        'VA',  // Vales de Compra
        'BA',  // ✅ NUEVO v19.3: Transferencias Bancarias
        'MU',  // ✅ NUEVO v19.5: Órdenes/Cupones de Mutuales
        'CH',  // ✅ NUEVO v20.0: Cheques
    ];

    const requiereModal = tiposConModalObligatorio.includes(tcfId.toUpperCase());

    console.log(`   ${requiereModal ? '✅' : '❌'} ${tcfId} ${requiereModal ? 'REQUIERE' : 'NO requiere'} modal obligatorio`);

    return requiereModal;
}

/**
 * ✅ ACTUALIZADO v19.3: Abre el modal de detalle correcto según el tipo
 * NUEVO: Agregado case 'BA' para Transferencias Bancarias
 */
function abrirModalDetalleSegunTipo(instrumento, tipoMedioPago) {
    console.log('═══════════════════════════════════════════════════');
    console.log('🔓 ABRIR MODAL DETALLE SEGÚN TIPO v19.3');
    console.log(`   Tipo MP: ${tipoMedioPago.tcf_id} - ${tipoMedioPago.tcf_desc}`);
    console.log(`   Instrumento: ${instrumento.ins_desc}`);
    console.log('═══════════════════════════════════════════════════');

    const tcfId = tipoMedioPago.tcf_id.toUpperCase();

    switch (tcfId) {
        case 'VA': // Vales de Compra
            console.log('✅ Abriendo modal de Vale de Compra...');
            abrirModalDetalleValeCompra(instrumento, tipoMedioPago);
            break;

        case 'EF': // Efectivo
            console.log('✅ Abriendo modal de Efectivo...');
            abrirModalDetalleEfectivo(instrumento, tipoMedioPago);
            break;

        case 'BA': // ✅ NUEVO v19.3: Transferencias Bancarias
            console.log('✅ Abriendo modal de Transferencia Bancaria...');
            abrirModalDetalleTransferencia(instrumento, tipoMedioPago);
            break;

        case 'MU': // ✅ NUEVO v19.5: Órdenes/Cupones de Mutuales
            console.log('✅ Abriendo modal de Cupón/Orden de Empresa...');
            abrirModalDetalleCuponEmpresa(instrumento, tipoMedioPago);
            break;

        case 'CH': // ✅ ACTUALIZADO v20.0: Cheques
            console.log('✅ Abriendo modal de Cheque...');
            abrirModalDetalleCheque(instrumento, tipoMedioPago);
            break;

        default:
            console.warn(`⚠️ Tipo ${tcfId} sin modal específico`);
            agregarValorDirecto(instrumento, tipoMedioPago);
            break;
    }

    console.log('✅ Función finalizada');
}

/**
* ✅ NUEVO v19.2: Valida y sanitiza el objeto instrumento
* Asegura que todas las propiedades críticas existan y sean del tipo correcto
* 
* @param {Object} instrumento - Objeto instrumento a validar
* @returns {Object} - Objeto validado con propiedades garantizadas
*/
function validarInstrumento(instrumento) {
    console.log('🔍 Validando objeto instrumento...');

    if (!instrumento) {
        console.error('❌ Instrumento es null/undefined');
        return null;
    }

    // Crear objeto sanitizado con valores por defecto
    const instrumentoValido = {
        ins_id: instrumento.ins_id || instrumento.id || '',
        ins_desc: instrumento.ins_desc || instrumento.descripcion || 'Sin descripción',
        ins_simbolo: instrumento.ins_simbolo || instrumento.simbolo || '$',
        total_actual: parseFloat(instrumento.total_actual) || parseFloat(instrumento.saldo) || 0,
        tiene_detalle: Boolean(instrumento.tiene_detalle)
    };

    console.log('✅ Instrumento validado:', instrumentoValido);

    return instrumentoValido;
}

// ═══════════════════════════════════════════════════════════════════
// ✅ NUEVO v19.3: FUNCIONES PARA TRANSFERENCIAS BANCARIAS
// ═══════════════════════════════════════════════════════════════════

/**
 * ✅ ACTUALIZADO v24.1: Abre el modal de detalle de Transferencia Bancaria SIN InputMask
 * CORRECCIÓN CRÍTICA: Eliminado InputMask para compatibilidad con teclado digital
 * 
 * CAMBIOS v24.1:
 * - Removido InputMaskMonetario del input monto
 * - Establecer valor inicial SIN FORMATO (solo números)
 * - Teclado digital escribe correctamente
 * 
 * @param {Object} instrumento - Banco seleccionado
 * @param {Object} tipoMedioPago - Tipo de MP (tcf_id='BA')
 */
function abrirModalDetalleTransferencia(instrumento, tipoMedioPago) {
    console.log('═══════════════════════════════════════════════════');
    console.log('🔓 ABRIR MODAL DETALLE TRANSFERENCIA v24.1 (SIN InputMask)');
    console.log(`   Banco: ${instrumento?.ins_desc || 'N/A'} (${instrumento?.ins_id || 'N/A'})`);
    console.log(`   Tipo MP: ${tipoMedioPago?.tcf_desc || 'N/A'}`);
    console.log('═══════════════════════════════════════════════════');

    // ❶ Validar objeto instrumento
    if (!instrumento) {
        console.error('❌ CRÍTICO: Objeto instrumento es null/undefined');

        if (typeof toastr !== 'undefined') {
            toastr.error('Error: No se pudo cargar la información del banco');
        }

        return;
    }

    // ❷ Obtener elemento del modal
    const $modal = $('#modalDetalleTransferencia');

    if ($modal.length === 0) {
        console.error('❌ Modal #modalDetalleTransferencia no encontrado en el DOM');

        if (typeof toastr !== 'undefined') {
            toastr.error('El modal de transferencias no está disponible');
        }

        return;
    }

    // ❸ Hidratar información del banco seleccionado
    $('#lblInstrumentoTransferencia').text(instrumento.ins_desc || 'Banco sin nombre');
    $('#hdnBancoIdTransferencia').val(instrumento.ins_id);

    console.log(`   ✅ Banco cargado: ${instrumento.ins_desc}`);

    // ❹ Establecer fecha actual por defecto
    const fechaHoy = new Date().toISOString().split('T')[0];
    $('#txtFechaTransferencia').val(fechaHoy);

    // ❺ Calcular importe sugerido (diferencia pendiente)
    const diferencia = Math.abs(conceptosPago.diferencia || 0);
    const importeSugerido = diferencia;

    console.log(`   💰 Importe sugerido: ${importeSugerido.toFixed(2)}`);

    // ❻ Obtener input
    const $inputMonto = $('#txtMontoTransferencia');

    // ✅ CRÍTICO v24.1: REMOVER INPUTMASK SI EXISTE
    if (typeof InputMaskMonetario !== 'undefined') {
        console.log('🗑️ Removiendo InputMask existente...');
        InputMaskMonetario.removerMascara($inputMonto);
        console.log('   ✅ InputMask removido correctamente');
    }

    // ❼ ✅ NUEVO v24.1: Establecer valor inicial SIN FORMATO
    $inputMonto.val(importeSugerido.toFixed(2));

    console.log(`   ✅ Valor inicial: ${importeSugerido.toFixed(2)}`);
    console.log('   ✅ Teclado digital listo para escribir');

    // ❽ Limpiar campos
    $('#txtNroTransferencia').val('');

    // ❾ Limpiar validaciones previas
    $('#formDetalleTransferencia .form-control')
        .removeClass('is-invalid is-valid');
    $('.invalid-feedback').remove();

    // ❿ Mostrar modal con jQuery
    $modal
        .addClass('show')
        .css({
            'display': 'block',
            'opacity': '1',
            'z-index': '5100'
        })
        .attr('aria-modal', 'true')
        .removeAttr('aria-hidden');

    // ⓫ Crear backdrop
    if ($('.modal-backdrop[data-modal="transferencia"]').length === 0) {
        $('body').append(
            '<div class="modal-backdrop fade show" ' +
            'data-modal="transferencia" ' +
            'style="z-index: 5099;"></div>'
        );
    }

    // ⓬ Focus en el primer campo
    setTimeout(() => {
        $('#txtNroTransferencia').trigger('focus');
    }, INPUT_FOCUS_TIMEOUT);

    // ⓭ Vincular eventos de guardar
    $('#btnGuardarDetalleTransferencia')
        .off('click.guardarTransf')
        .on('click.guardarTransf', function () {
            guardarDetalleTransferencia(instrumento, tipoMedioPago);
        });

    // ⓮ Vincular evento Enter
    $inputMonto
        .off('keypress.enterTransf')
        .on('keypress.enterTransf', function (e) {
            if (e.which === 13) {
                e.preventDefault();
                guardarDetalleTransferencia(instrumento, tipoMedioPago);
            }
        });

    // ⓯ Navegación con Enter entre campos
    $('#txtNroTransferencia')
        .off('keypress.enterNavTransf')
        .on('keypress.enterNavTransf', function (e) {
            if (e.which === 13) {
                e.preventDefault();
                console.log('⏎ Enter en Nro Transferencia - Saltando a Fecha...');
                $('#txtFechaTransferencia').trigger('focus');
            }
        });

    $('#txtFechaTransferencia')
        .off('keypress.enterNavTransf')
        .on('keypress.enterNavTransf', function (e) {
            if (e.which === 13) {
                e.preventDefault();
                console.log('⏎ Enter en Fecha - Saltando a Monto...');
                $('#txtMontoTransferencia').trigger('focus').trigger('select');
            }
        });

    console.log('═══════════════════════════════════════════════════');
    console.log('✅ INPUT LISTO PARA TECLADO DIGITAL:');
    console.log('   - InputMask: DESACTIVADO ❌');
    console.log('   - Teclado digital: ACTIVO ✅');
    console.log('   - Formato: SOLO NÚMEROS (sin $, sin comas)');
    console.log('   - Validación: NATIVA HTML5');
    console.log('═══════════════════════════════════════════════════');

    console.log('✅ Modal detalle transferencia abierto SIN InputMask');
}

/**
 * ✅ ACTUALIZADO v24.1: Guarda el detalle de la transferencia bancaria SIN InputMask
 * CAMBIO CRÍTICO: Parseo directo del valor sin InputMaskMonetario
 * 
 * LÓGICA:
 * 1. Obtener valor del input como string
 * 2. Limpiar caracteres no numéricos (excepto punto/coma decimal)
 * 3. Parsear con parseFloat()
 * 4. Validar y guardar
 * 
 * @param {Object} instrumento - Datos del banco
 * @param {Object} tipoMedioPago - Tipo de medio de pago
 */
function guardarDetalleTransferencia(instrumento, tipoMedioPago) {
    console.log('═══════════════════════════════════════════════════');
    console.log('💾 GUARDAR DETALLE TRANSFERENCIA v24.1 (SIN InputMask)');
    console.log('═══════════════════════════════════════════════════');

    // ❶ Obtener valores del formulario
    const nroTransferencia = $('#txtNroTransferencia').val().trim().toUpperCase();
    const fechaTransferencia = $('#txtFechaTransferencia').val();

    console.log('📋 Datos del formulario:');
    console.log(`   Nro Transferencia: "${nroTransferencia}"`);
    console.log(`   Fecha: "${fechaTransferencia}"`);

    // ❷ Validar Nro Transferencia (mínimo 15 caracteres)
    if (!nroTransferencia || nroTransferencia.length < 15) {
        console.warn('⚠️ Número de transferencia inválido');
        mostrarErrorCampo(
            '#txtNroTransferencia',
            'Debe ingresar un número de transferencia válido (mínimo 15 caracteres)'
        );
        return;
    }

    // ❸ Validar Fecha
    if (!fechaTransferencia) {
        console.warn('⚠️ Fecha no ingresada');
        mostrarErrorCampo('#txtFechaTransferencia', 'Debe seleccionar la fecha de la transferencia');
        return;
    }

    // ❹ Validar fecha (entre ayer y hoy)
    const fechaTransf = new Date(fechaTransferencia);
    const fechaHoy = new Date();
    fechaHoy.setHours(0, 0, 0, 0);

    const fechaAyer = new Date(fechaHoy);
    fechaAyer.setDate(fechaAyer.getDate() - 1);

    if (fechaTransf < fechaAyer) {
        console.warn('⚠️ Fecha de transferencia es muy antigua');
        mostrarErrorCampo(
            '#txtFechaTransferencia',
            'La fecha no puede ser anterior a ayer'
        );
        return;
    }

    if (fechaTransf > fechaHoy) {
        console.warn('⚠️ Fecha de transferencia es futura');
        mostrarErrorCampo('#txtFechaTransferencia', 'La fecha no puede ser futura');
        return;
    }

    // ❺ ✅ ACTUALIZADO v24.1: Obtener y limpiar valor del input
    const montoStr = $('#txtMontoTransferencia').val().trim();

    // Limpiar: remover todo excepto dígitos, punto y coma
    // Convertir coma a punto (por si el usuario usó coma decimal)
    const montoLimpio = montoStr.replace(/[^\d.,]/g, '').replace(',', '.');

    // Parsear a número flotante
    const monto = parseFloat(montoLimpio) || 0;

    console.log(`   📝 Valor del input: "${montoStr}"`);
    console.log(`   🔧 Valor limpio: "${montoLimpio}"`);
    console.log(`   💰 Monto parseado: ${monto}`);

    // ❻ Validar monto > 0
    if (isNaN(monto) || monto <= 0) {
        console.warn('⚠️ Monto inválido o cero');
        mostrarErrorCampo('#txtMontoTransferencia', 'Debe ingresar un monto válido mayor a cero');
        return;
    }

    // ❼ Validar monto <= saldo factura (con tolerancia)
    const diferenciaFactura = Math.abs(conceptosPago.diferencia || 0);

    if (monto > diferenciaFactura * LIMITE_PORCENTAJE_DIFERENCIA) {
        console.warn(`⚠️ Monto muy alto: ${monto} > ${diferenciaFactura * LIMITE_PORCENTAJE_DIFERENCIA}`);

        const mensajeHtml = `
        <div class="text-start">
            <p class="mb-3">El monto ingresado es <strong>mayor</strong> a la diferencia pendiente:</p>
            <table class="table table-sm table-borderless mb-0">
                <tr>
                    <td class="text-end">Monto ingresado:</td>
                    <td class="text-start"><strong class="text-danger">${formatearMoneda(monto)}</strong></td>
                </tr>
                <tr>
                    <td class="text-end">Diferencia pendiente:</td>
                    <td class="text-start"><strong class="text-warning">${formatearMoneda(diferenciaFactura)}</strong></td>
                </tr>
            </table>
            <p class="mt-3 mb-0"><i class="bx bx-info-circle"></i> ¿Desea continuar?</p>
        </div>
    `;

        AbrirMensaje(
            "¿Monto elevado?",
            mensajeHtml,
            function () {
                $('#msjModal').modal('hide');
                finalizarGuardadoTransferencia(monto, nroTransferencia, fechaTransferencia, instrumento, tipoMedioPago);
            },
            false,
            ["Continuar", "Corregir"],
            "warn!",
            function () {
                $('#msjModal').modal('hide');
                setTimeout(() => {
                    $('#txtMontoTransferencia').trigger("focus").trigger("select");
                }, 300);
            }
        );

        return;
    }

    // ❽ Si validaciones OK, finalizar guardado
    finalizarGuardadoTransferencia(monto, nroTransferencia, fechaTransferencia, instrumento, tipoMedioPago);
}

/**
* ✅ NUEVO v19.3: Finaliza el guardado de la transferencia bancaria
*/
function finalizarGuardadoTransferencia(monto, nroTransferencia, fechaTransferencia, instrumento, tipoMedioPago) {
    console.log('✅ Finalizando guardado de transferencia bancaria...');
    console.log(`   Monto: ${monto}`);
    console.log(`   Banco: ${instrumento.ins_desc}`);
    console.log(`   Nro Trasn: ${nroTransferencia}`);

    // ❶ Crear objeto de detalle
    const detalleTransferencia = {
        banco_id: instrumento.ins_id,
        banco_desc: instrumento.ins_desc,
        nro_transferencia: nroTransferencia,
        fecha_transferencia: fechaTransferencia
    };

    // ❷ Crear objeto de valor
    const nuevoValor = {
        id: ++valorIdCounter,
        tcf_id: tipoMedioPago.tcf_id,
        tcf_desc: tipoMedioPago.tcf_desc,
        ins_id: instrumento.ins_id,
        ins_desc: instrumento.ins_desc,
        ins_simbolo: instrumento.ins_simbolo || '$',
        importe: monto,
        observacion: `Transf ${nroTransferencia} - ${fechaTransferencia}`,
        detalle: detalleTransferencia,
        fecha_creacion: new Date().toISOString()
    };

    console.log('📦 Nuevo valor creado:', nuevoValor);

    // ❸ Agregar a array global
    valoresPago.push(nuevoValor);

    // ❹ Agregar fila a la tabla
    agregarFilaValor(nuevoValor);

    // ❺ Actualizar totales
    actualizarTotalesPago();

    // ❻ Cerrar modal
    cerrarModalDetalleTransferencia();

    // ❼ Notificación
    if (typeof toastr !== 'undefined') {
        toastr.success(
            `Transferencia agregada: ${formatearMoneda(monto)} - ${instrumento.ins_desc}`,
            'Valor guardado',
            { timeOut: 3000 }
        );
    }

    console.log('✅ Valor de transferencia bancaria guardado correctamente');
}


/**
* ✅ NUEVO v19.3: Cierra el modal de detalle de transferencia
*/
function cerrarModalDetalleTransferencia() {
    console.log('═══════════════════════════════════════════════════');
    console.log('🔒 CERRAR MODAL DETALLE TRANSFERENCIA v19.3');
    console.log('═══════════════════════════════════════════════════');

    const $modal = $('#modalDetalleTransferencia');

    if ($modal.length === 0) {
        console.warn('⚠️ Modal #modalDetalleTransferencia no encontrado');
        return;
    }

    // ❶ Ocultar modal
    $modal
        .removeClass('show')
        .css('display', 'none')
        .attr('aria-hidden', 'true')
        .removeAttr('aria-modal');

    console.log('   ✅ Modal ocultado');

    // ❷ Remover backdrop
    const $backdropTransf = $('.modal-backdrop[data-modal="transferencia"]');
    if ($backdropTransf.length > 0) {
        $backdropTransf.fadeOut(200, function () {
            $(this).remove();
        });
        console.log('   ✅ Backdrop removido');
    }

    // ❸ Limpiar formulario
    $('#formDetalleTransferencia')[0].reset();
    $('#formDetalleTransferencia .form-control')
        .removeClass('is-invalid is-valid');
    $('.invalid-feedback').remove();

    console.log('   ✅ Formulario limpiado');

    // ❹ Resetear labels
    $('#lblInstrumentoTransferencia').text('-');
    $('#hdnBancoIdTransferencia').val('');

    console.log('   ✅ Labels reseteados');

    // ❺ Verificar otros modales
    setTimeout(() => {
        if ($('.modal.show').length === 0) {
            $('body').removeClass('modal-open').css('overflow', '');
            console.log('   ✅ Body desbloqueado');
        }
    }, 100);

    console.log('✅ MODAL CERRADO COMPLETAMENTE');
}

// ❺ ✅ NUEVO v19.3: Evento de limpieza del modal de Transferencia
$('#modalDetalleTransferencia').off('hidden.bs.modal').on('hidden.bs.modal', function () {
    console.log('🧹 LIMPIEZA AUTOMÁTICA - MODAL TRANSFERENCIA');

    const $form = $('#formDetalleTransferencia');
    $form[0].reset();
    $form.find('.form-control').removeClass('is-invalid is-valid');
    $('.invalid-feedback').remove();

    $('#lblInstrumentoTransferencia').text('-');
    $('#hdnBancoIdTransferencia').val('');

    const $backdropTransf = $('.modal-backdrop[data-modal="transferencia"]');
    if ($backdropTransf.length > 0) {
        $backdropTransf.remove();
    }

    console.log('✅ MODAL DE TRANSFERENCIA LIMPIADO');
});

/**
 * ✅ ACTUALIZADO v24.3: Abre el modal de detalle de Cupón/Orden de Empresa SIN InputMask
 * CORRECCIÓN CRÍTICA: Eliminado InputMask para compatibilidad con teclado digital
 * 
 * CAMBIOS v24.3:
 * - Removido InputMaskMonetario del input monto
 * - Establecer valor inicial SIN FORMATO (solo números)
 * - Teclado digital escribe correctamente
 * 
 * @param {Object} instrumento - Mutual/Empresa seleccionada
 * @param {Object} tipoMedioPago - Tipo de MP (tcf_id='MU')
 */
function abrirModalDetalleCuponEmpresa(instrumento, tipoMedioPago) {
    console.log('═══════════════════════════════════════════════════');
    console.log('🔓 ABRIR MODAL DETALLE CUPÓN EMPRESA v24.3 (SIN InputMask)');
    console.log(`   Empresa: ${instrumento?.ins_desc || 'N/A'} (${instrumento?.ins_id || 'N/A'})`);
    console.log(`   Tipo MP: ${tipoMedioPago?.tcf_desc || 'N/A'}`);
    console.log('═══════════════════════════════════════════════════');

    // ❶ Validar objeto instrumento
    if (!instrumento) {
        console.error('❌ CRÍTICO: Objeto instrumento es null/undefined');

        if (typeof toastr !== 'undefined') {
            toastr.error('Error: No se pudo cargar la información de la empresa/mutual');
        }

        return;
    }

    // ❷ Obtener elemento del modal
    const modalElement = document.querySelector('#modalDetalleCuponEmpresa');

    if (!modalElement) {
        console.error('❌ Modal #modalDetalleCuponEmpresa no encontrado en el DOM');

        if (typeof toastr !== 'undefined') {
            toastr.error('El modal de órdenes/cupones no está disponible');
        }

        return;
    }

    // ❸ Obtener o crear instancia de Bootstrap Modal
    let modalInstance = bootstrap.Modal.getInstance(modalElement);

    if (!modalInstance) {
        console.log('⚠️ Creando instancia de Bootstrap Modal...');

        try {
            modalInstance = new bootstrap.Modal(modalElement, {
                backdrop: 'static',
                keyboard: false
            });
            console.log('✅ Instancia creada correctamente');
        } catch (error) {
            console.error('❌ Error al crear instancia:', error);

            if (typeof toastr !== 'undefined') {
                toastr.error(`Error al inicializar el modal: ${error.message}`);
            }

            return;
        }
    }

    // ❹ Hidratar información de la empresa/mutual seleccionada
    $('#lblEmpresaSeleccionada').text(instrumento.ins_desc || 'Empresa sin nombre');
    $('#hdnEmpresaIdCupon').val(instrumento.ins_id);

    console.log(`   ✅ Empresa cargada: ${instrumento.ins_desc}`);

    // ❵ Obtener datos del cliente actual
    console.log('═══════════════════════════════════════════════════');
    console.log('📋 OBTENIENDO DATOS DEL CLIENTE ACTUAL');

    const titular = $('#txtClienteNombrePago').val() || '';
    console.log(`   Titular: "${titular}"`);

    const cuitCliente = $('#txtClienteCuitPago').val() || '';
    console.log(`   CUIT/DNI: "${cuitCliente}"`);

    const ctaId = $('#txtClienteIdPago').val() || '';
    const esClienteRegistrado = ctaId && ctaId !== 'N/A' && ctaId.trim() !== '';

    console.log(`   Tipo Cliente: ${esClienteRegistrado ? 'CR (Cliente Registrado)' : 'CF (Consumidor Final)'}`);
    console.log(`   cta_id: "${ctaId}"`);
    console.log('═══════════════════════════════════════════════════');

    // ❻ Validar que tengamos datos mínimos
    if (!titular || titular.trim() === '') {
        console.error('❌ CRÍTICO: No se pudo obtener el nombre del cliente');

        if (typeof toastr !== 'undefined') {
            toastr.error('Error: No se pudo obtener los datos del cliente actual');
        }

        return;
    }

    // ❼ Asignar Titular al campo (readonly)
    $('#txtTitularCupon').val(titular);
    console.log(`   ✅ Titular asignado al campo: "${titular}"`);

    // ❽ Validar y asignar CUIT según tipo de cliente
    const resultadoCuit = validarYAsignarCuitCuponEmpresa(cuitCliente, esClienteRegistrado);

    if (!resultadoCuit.valido) {
        console.error('❌ Validación de CUIT falló');

        // Cerrar modal automáticamente
        setTimeout(() => {
            cerrarModalDetalleCuponEmpresa();
        }, 100);

        return;
    }

    console.log('✅ CUIT validado y asignado correctamente');

    // ❾ Calcular monto sugerido (diferencia pendiente)
    const diferencia = Math.abs(conceptosPago.diferencia || 0);
    const montoSugerido = diferencia;

    console.log(`   💰 Monto sugerido: ${formatearMoneda(montoSugerido)}`);

    // ❿ Obtener input
    const $inputMonto = $('#txtMontoCupon');

    // ✅ CRÍTICO v24.3: REMOVER INPUTMASK SI EXISTE
    if (typeof InputMaskMonetario !== 'undefined') {
        console.log('🗑️ Removiendo InputMask existente...');
        InputMaskMonetario.removerMascara($inputMonto);
        console.log('   ✅ InputMask removido correctamente');
    }

    // ⓫ ✅ NUEVO v24.3: Establecer valor inicial SIN FORMATO
    $inputMonto.val(montoSugerido.toFixed(2));

    console.log(`   ✅ Valor inicial: ${montoSugerido.toFixed(2)}`);
    console.log('   ✅ Teclado digital listo para escribir');

    // ⓬ Limpiar solo campo Nro de Orden
    $('#txtNroOrdenCupon').val('');

    // ⓭ Limpiar validaciones previas
    $('#formDetalleCuponEmpresa .form-control')
        .removeClass('is-invalid is-valid');
    $('.invalid-feedback').remove();

    // ⓮ Mostrar modal con Bootstrap
    try {
        modalInstance.show();
        console.log('✅ Modal mostrado con Bootstrap.show()');

        setTimeout(() => {
            $(modalElement).css('z-index', '5100');

            const $backdrops = $('.modal-backdrop');
            if ($backdrops.length > 0) {
                $backdrops.last().css('z-index', '5099');
            }

            console.log('   ✅ Z-index ajustado: modal=5100, backdrop=5099');
        }, 200);

    } catch (error) {
        console.error('❌ ERROR al mostrar modal:', error);

        if (typeof toastr !== 'undefined') {
            toastr.error(`Error al abrir el modal: ${error.message}`);
        }

        return;
    }

    // ⓯ Focus en Nro de Orden (ya que Titular es readonly)
    setTimeout(() => {
        $('#txtNroOrdenCupon').trigger('focus');
    }, INPUT_FOCUS_TIMEOUT);

    // ⓰ Vincular eventos de guardar
    $('#btnGuardarDetalleCupon')
        .off('click.guardarCupon')
        .on('click.guardarCupon', function () {
            guardarDetalleCuponEmpresa(instrumento, tipoMedioPago);
        });

    // ⓱ Vincular evento Enter en el último campo (monto)
    $inputMonto
        .off('keypress.enterCupon')
        .on('keypress.enterCupon', function (e) {
            if (e.which === 13) {
                e.preventDefault();
                guardarDetalleCuponEmpresa(instrumento, tipoMedioPago);
            }
        });

    console.log('═══════════════════════════════════════════════════');
    console.log('✅ INPUT LISTO PARA TECLADO DIGITAL:');
    console.log('   - InputMask: DESACTIVADO ❌');
    console.log('   - Teclado digital: ACTIVO ✅');
    console.log('   - Formato: SOLO NÚMEROS (sin $, sin comas)');
    console.log('   - Validación: NATIVA HTML5');
    console.log('═══════════════════════════════════════════════════');

    console.log('✅ Modal detalle cupón empresa configurado correctamente');
}

/**
 * ✅ CORREGIDO v20.5: Valida y asigna CUIT/DNI al campo según tipo de cliente
 * CORRECCIÓN CRÍTICA: Extrae solo los dígitos del CUIT/CUIL (elimina prefijos como "CUIL", "CUIT", espacios, etc.)
 * 
 * REGLAS DE NEGOCIO:
 * 
 * CR (Cliente Registrado):
 *   - Si tiene CUIT válido (11 dígitos) → ✅ Asignar con formato XX-XXXXXXXX-X
 *   - Si NO tiene CUIT o formato inválido → ❌ Bloquear + mensaje de error
 * 
 * CF (Consumidor Final):
 *   - Si tiene CUIT válido (11 dígitos) → ✅ Asignar con formato XX-XXXXXXXX-X
 *   - Si tiene solo DNI (8 dígitos) → ⚠️ Asignar DNI + mensaje informativo
 *   - Si está vacío → ⚠️ Mensaje informativo (cajero debe solicitar)
 * 
 * @param {string} cuitCliente - CUIT/DNI del cliente actual (puede incluir prefijos como "CUIL", "CUIT")
 * @param {boolean} esClienteRegistrado - true si es CR, false si es CF
 * @returns {Object} - { valido: boolean, mensaje: string }
 */
function validarYAsignarCuitCuponEmpresa(cuitCliente, esClienteRegistrado) {
    console.log('═══════════════════════════════════════════════════');
    console.log('🔍 VALIDAR Y ASIGNAR CUIT v20.5');
    console.log(`   CUIT/DNI recibido (original): "${cuitCliente}"`);
    console.log(`   Es Cliente Registrado: ${esClienteRegistrado ? 'SÍ' : 'NO'}`);
    console.log('═══════════════════════════════════════════════════');

    const $inputCuit = $('#txtCuitCupon');
    const $msgCuit = $('#msgCuitCupon');

    // ❶ ✅ NUEVO v20.5: Extraer SOLO los dígitos (eliminar "CUIL", "CUIT", espacios, guiones)
    const cuitOriginal = (cuitCliente || '').trim();

    // Remover palabras comunes y caracteres no numéricos excepto guiones
    let cuitLimpio = cuitOriginal
        .toUpperCase()
        .replace(/CUIL\s*/g, '')    // Quitar "CUIL "
        .replace(/CUIT\s*/g, '')    // Quitar "CUIT "
        .replace(/DNI\s*/g, '')     // Quitar "DNI "
        .trim();

    // Extraer solo dígitos y guiones
    const soloDigitosYGuiones = cuitLimpio.match(/[\d-]/g);
    cuitLimpio = soloDigitosYGuiones ? soloDigitosYGuiones.join('') : '';

    console.log(`   ✅ CUIT/DNI limpio (solo dígitos/guiones): "${cuitLimpio}"`);
    console.log(`   📊 Longitud: ${cuitLimpio.length} caracteres`);

    // ❷ Expresiones regulares
    const regexCuit = /^\d{2}-\d{8}-\d{1}$/;  // XX-XXXXXXXX-X (con guiones)
    const regexDni = /^\d{8}$/;                // 8 dígitos (solo DNI)
    const regexCuitSinGuiones = /^\d{11}$/;    // 11 dígitos sin guiones (CUIT sin formatear)

    // ❃ ✅ NUEVO: Extraer solo dígitos numéricos (sin guiones)
    const soloDigitos = cuitLimpio.replace(/-/g, '');
    console.log(`   🔢 Solo dígitos (sin guiones): "${soloDigitos}" (${soloDigitos.length} dígitos)`);

    // ═══════════════════════════════════════════════════════════
    // CASO 1: CLIENTE REGISTRADO (CR) - Validación estricta
    // ═══════════════════════════════════════════════════════════

    if (esClienteRegistrado) {
        console.log('📋 VALIDANDO CLIENTE REGISTRADO (CR)...');

        // ❶ Verificar si tiene formato de CUIT válido con guiones
        if (regexCuit.test(cuitLimpio)) {
            console.log('✅ CUIT válido con formato correcto (XX-XXXXXXXX-X)');

            $inputCuit.val(cuitLimpio);
            $msgCuit.hide();

            return { valido: true };
        }

        // ❷ ✅ CORREGIDO: Verificar si tiene 11 dígitos (CUIT sin guiones o con formato inconsistente)
        if (regexCuitSinGuiones.test(soloDigitos)) {
            console.log('⚠️ CUIT sin guiones detectado (11 dígitos) - Formateando...');

            // Formatear: "20123456789" → "20-12345678-9"
            const cuitFormateado = soloDigitos.substring(0, 2) + '-' +
                soloDigitos.substring(2, 10) + '-' +
                soloDigitos.substring(10);

            console.log(`   ✅ CUIT formateado: "${cuitFormateado}"`);

            $inputCuit.val(cuitFormateado);
            $msgCuit.hide();

            return { valido: true };
        }

        // ❌ CR sin CUIT válido → BLOQUEAR
        console.error('❌ CR sin CUIT válido - OPERACIÓN BLOQUEADA');
        console.error(`   Valor original: "${cuitOriginal}"`);
        console.error(`   Valor limpio: "${cuitLimpio}"`);
        console.error(`   Solo dígitos: "${soloDigitos}" (${soloDigitos.length} dígitos)`);

        const mensajeError = `
            <div class="text-start">
                <p class="mb-3">
                    <i class='bx bx-error-circle text-danger fs-3'></i>
                    <strong class="text-danger">Cliente sin CUIT válido</strong>
                </p>
                <div class="alert alert-danger mb-3">
                    <strong>Cliente Registrado:</strong> ${$('#txtClienteNombrePago').val()}<br>
                    <strong>CUIT actual:</strong> ${cuitOriginal || 'Sin CUIT'}<br>
                    <strong>Dígitos detectados:</strong> ${soloDigitos} (${soloDigitos.length} dígitos)<br><br>
                    <i class='bx bx-info-circle'></i> 
                    El CUIT debe tener <strong>11 dígitos</strong> (formato XX-XXXXXXXX-X)
                </div>
                <p class="mb-0">
                    Por favor, solicite al cliente que corrija su CUIT en 
                    <strong>Atención al Cliente</strong> antes de continuar.
                </p>
            </div>
        `;

        AbrirMensaje(
            "CUIT Inválido",
            mensajeError,
            function () {
                $('#msjModal').modal('hide');
            },
            false,
            ["Aceptar"],
            "error!",
            null
        );

        return { valido: false, mensaje: 'CR sin CUIT válido' };
    }

    // ═══════════════════════════════════════════════════════════
    // CASO 2: CONSUMIDOR FINAL (CF) - Validación flexible
    // ═══════════════════════════════════════════════════════════

    console.log('📋 VALIDANDO CONSUMIDOR FINAL (CF)...');

    // ❶ Verificar si tiene CUIT válido con guiones
    if (regexCuit.test(cuitLimpio)) {
        console.log('✅ CF con CUIT válido (formato con guiones)');

        $inputCuit.val(cuitLimpio);
        $msgCuit
            .removeClass('text-warning text-danger')
            .addClass('text-success')
            .html('<i class="bx bx-check-circle"></i> CUIT válido')
            .show();

        return { valido: true };
    }

    // ❷ ✅ CORREGIDO: Verificar si tiene 11 dígitos (CUIT sin guiones)
    if (regexCuitSinGuiones.test(soloDigitos)) {
        console.log('⚠️ CF con CUIT sin guiones (11 dígitos) - Formateando...');

        const cuitFormateado = soloDigitos.substring(0, 2) + '-' +
            soloDigitos.substring(2, 10) + '-' +
            soloDigitos.substring(10);

        console.log(`   ✅ CUIT formateado: "${cuitFormateado}"`);

        $inputCuit.val(cuitFormateado);
        $msgCuit
            .removeClass('text-warning text-danger')
            .addClass('text-success')
            .html('<i class="bx bx-check-circle"></i> CUIT válido (formateado automáticamente)')
            .show();

        return { valido: true };
    }

    // ❸ Verificar si tiene solo DNI (8 dígitos)
    if (regexDni.test(soloDigitos)) {
        console.log('⚠️ CF con DNI (sin CUIT) - 8 dígitos detectados');

        $inputCuit.val(soloDigitos);
        $msgCuit
            .removeClass('text-success text-danger')
            .addClass('text-warning')
            .html(`
                <i class="bx bx-info-circle"></i> 
                <strong>DNI detectado (sin CUIT)</strong><br>
                <small>Se usará el DNI para el registro. Si el cliente tiene CUIT, actualícelo en Atención al Cliente.</small>
            `)
            .show();

        // Mostrar toastr informativo
        if (typeof toastr !== 'undefined') {
            toastr.warning(
                'Se usará DNI en lugar de CUIT. Si el cliente tiene CUIT, actualícelo en Atención al Cliente.',
                'Documento sin CUIT',
                { timeOut: 5000 }
            );
        }

        return { valido: true };
    }

    // ❹ CF sin documento válido → Solicitar al cajero
    console.warn('⚠️ CF sin documento válido');
    console.warn(`   Valor original: "${cuitOriginal}"`);
    console.warn(`   Solo dígitos: "${soloDigitos}" (${soloDigitos.length} dígitos)`);

    $inputCuit.val('');
    $msgCuit
        .removeClass('text-success text-warning')
        .addClass('text-danger')
        .html(`
            <i class="bx bx-error-circle"></i> 
            <strong>Sin CUIT/DNI registrado</strong><br>
            <small>Solicite al cliente su CUIT o DNI y actualícelo en Atención al Cliente.</small>
        `)
        .show();

    const mensajeAdvertencia = `
        <div class="text-start">
            <p class="mb-3">
                <i class='bx bx-info-circle text-warning fs-3'></i>
                <strong>Cliente sin documento registrado</strong>
            </p>
            <div class="alert alert-warning mb-3">
                <strong>Consumidor Final:</strong> ${$('#txtClienteNombrePago').val()}<br>
                <strong>Documento actual:</strong> ${cuitOriginal || 'Sin registro'}<br>
                <strong>Dígitos detectados:</strong> ${soloDigitos} (${soloDigitos.length} dígitos)<br><br>
                <i class='bx bx-info-circle'></i> 
                Por favor, solicite al cliente su <strong>CUIT</strong> (11 dígitos) o <strong>DNI</strong> (8 dígitos).
            </div>
            <p class="mb-0">
                Actualice el documento en <strong>Atención al Cliente</strong> antes de continuar.
            </p>
        </div>
    `;

    AbrirMensaje(
        "Documento Faltante",
        mensajeAdvertencia,
        function () {
            $('#msjModal').modal('hide');
        },
        false,
        ["Aceptar"],
        "warning",
        null
    );

    return { valido: false, mensaje: 'CF sin documento' };
}

/**
 * ✅ ACTUALIZADO v24.3: Guarda el detalle del cupón/orden de empresa SIN InputMask
 * CAMBIO CRÍTICO: Parseo directo del valor sin InputMaskMonetario
 * 
 * LÓGICA:
 * 1. Obtener valor del input como string
 * 2. Limpiar caracteres no numéricos (excepto punto/coma decimal)
 * 3. Parsear con parseFloat()
 * 4. Validar y guardar
 * 
 * NOTA: Titular y CUIT ya vienen validados (readonly desde cliente actual)
 * 
 * @param {Object} instrumento - Datos de la empresa/mutual
 * @param {Object} tipoMedioPago - Tipo de medio de pago
 */
function guardarDetalleCuponEmpresa(instrumento, tipoMedioPago) {
    console.log('═══════════════════════════════════════════════════');
    console.log('💾 GUARDAR DETALLE CUPÓN EMPRESA v24.3 (SIN InputMask)');
    console.log('═══════════════════════════════════════════════════');

    // ❶ Obtener valores del formulario
    const titular = $('#txtTitularCupon').val().trim(); // ← readonly (ya validado)
    const nroOrden = $('#txtNroOrdenCupon').val().trim().toUpperCase();
    const cuit = $('#txtCuitCupon').val().trim(); // ← readonly (ya validado)

    console.log('📋 Datos del formulario:');
    console.log(`   Titular (auto): "${titular}"`);
    console.log(`   Nro Orden: "${nroOrden}"`);
    console.log(`   CUIT (auto): "${cuit}"`);

    // ❷ Validar solo Nro Orden (Titular y CUIT ya vienen validados)

    if (!nroOrden || nroOrden === '') {
        console.warn('⚠️ Número de orden vacío');
        mostrarErrorCampo('#txtNroOrdenCupon', 'Debe ingresar el número de orden');
        return;
    }

    if (!/^\d+$/.test(nroOrden)) {
        console.warn('⚠️ Número de orden no es numérico');
        mostrarErrorCampo('#txtNroOrdenCupon', 'El número de orden debe ser numérico');
        return;
    }

    if (nroOrden.length > 10) {
        console.warn('⚠️ Número de orden demasiado largo');
        mostrarErrorCampo('#txtNroOrdenCupon', 'El número de orden no puede tener más de 10 dígitos');
        return;
    }

    // ❸ ✅ ACTUALIZADO v24.3: Obtener y limpiar valor del input
    const montoStr = $('#txtMontoCupon').val().trim();

    // Limpiar: remover todo excepto dígitos, punto y coma
    // Convertir coma a punto (por si el usuario usó coma decimal)
    const montoLimpio = montoStr.replace(/[^\d.,]/g, '').replace(',', '.');

    // Parsear a número flotante
    const monto = parseFloat(montoLimpio) || 0;

    console.log(`   📝 Valor del input: "${montoStr}"`);
    console.log(`   🔧 Valor limpio: "${montoLimpio}"`);
    console.log(`   💰 Monto parseado: ${monto}`);

    // ❹ Validar monto > 0
    if (isNaN(monto) || monto <= 0) {
        console.warn('⚠️ Monto inválido o cero');
        mostrarErrorCampo('#txtMontoCupon', 'Debe ingresar un monto válido mayor a cero');
        return;
    }

    // ❺ Validar monto <= saldo factura
    const diferenciaFactura = Math.abs(conceptosPago.diferencia || 0);

    if (monto > diferenciaFactura * LIMITE_PORCENTAJE_DIFERENCIA) {
        console.warn(`⚠️ Monto muy alto: ${monto} > ${diferenciaFactura * LIMITE_PORCENTAJE_DIFERENCIA}`);

        const mensajeHtml = `
        <div class="text-start">
            <p class="mb-3">El monto ingresado es <strong>mayor</strong> a la diferencia pendiente:</p>
            <table class="table table-sm table-borderless mb-0">
                <tr>
                    <td class="text-end">Monto ingresado:</td>
                    <td class="text-start"><strong class="text-danger">${formatearMoneda(monto)}</strong></td>
                </tr>
                <tr>
                    <td class="text-end">Diferencia pendiente:</td>
                    <td class="text-start"><strong class="text-warning">${formatearMoneda(diferenciaFactura)}</strong></td>
                </tr>
            </table>
            <p class="mt-3 mb-0"><i class="bx bx-info-circle"></i> ¿Desea continuar?</p>
        </div>
    `;

        AbrirMensaje(
            "¿Monto elevado?",
            mensajeHtml,
            function () {
                $('#msjModal').modal('hide');
                finalizarGuardadoCuponEmpresa(monto, titular, nroOrden, cuit, instrumento, tipoMedioPago);
            },
            false,
            ["Continuar", "Corregir"],
            "warn!",
            function () {
                $('#msjModal').modal('hide');
                setTimeout(() => {
                    $('#txtMontoCupon').trigger("focus").trigger("select");
                }, 300);
            }
        );

        return;
    }

    // ❻ Si validaciones OK, finalizar guardado
    finalizarGuardadoCuponEmpresa(monto, titular, nroOrden, cuit, instrumento, tipoMedioPago);
}

/**
 * ✅ NUEVO v19.5: Finaliza el guardado del cupón/orden de empresa
 */
function finalizarGuardadoCuponEmpresa(monto, titular, nroOrden, cuit, instrumento, tipoMedioPago) {
    console.log('✅ Finalizando guardado de cupón empresa...');
    console.log(`   Monto: ${monto}`);
    console.log(`   Empresa: ${instrumento.ins_desc}`);
    console.log(`   Titular: ${titular}`);
    console.log(`   Nro Orden: ${nroOrden}`);
    console.log(`   CUIT: ${cuit}`);

    // ❶ Crear objeto de detalle
    const detalleCupon = {
        empresa_id: instrumento.ins_id,
        empresa_desc: instrumento.ins_desc,
        titular: titular,
        nro_orden: nroOrden,
        cuit: cuit
    };

    // ❷ Crear objeto de valor
    const nuevoValor = {
        id: ++valorIdCounter,
        tcf_id: tipoMedioPago.tcf_id,
        tcf_desc: tipoMedioPago.tcf_desc,
        ins_id: instrumento.ins_id,
        ins_desc: instrumento.ins_desc,
        ins_simbolo: instrumento.ins_simbolo || '$',
        importe: monto,
        observacion: `Orden ${nroOrden} - ${titular} (CUIT: ${cuit})`,
        detalle: detalleCupon,
        fecha_creacion: new Date().toISOString()
    };

    console.log('📦 Nuevo valor creado:', nuevoValor);

    // ❸ Agregar a array global
    valoresPago.push(nuevoValor);

    // ❹ Agregar fila a la tabla
    agregarFilaValor(nuevoValor);

    // ❺ Actualizar totales
    actualizarTotalesPago();

    // ❻ Cerrar modal
    cerrarModalDetalleCuponEmpresa();

    // ❼ Notificación
    if (typeof toastr !== 'undefined') {
        toastr.success(
            `Cupón agregado: ${formatearMoneda(monto)} - ${instrumento.ins_desc}`,
            'Valor guardado',
            { timeOut: 3000 }
        );
    }

    console.log('✅ Valor de cupón empresa guardado correctamente');
}

/**
 * ✅ SIMPLIFICADO v19.7: Cierra el modal de detalle de cupón empresa
 * CAMBIO: Usa Bootstrap.Modal.hide() en lugar de manipulación manual
 */
function cerrarModalDetalleCuponEmpresa() {
    console.log('═══════════════════════════════════════════════════');
    console.log('🔒 CERRAR MODAL DETALLE CUPÓN EMPRESA v19.7');
    console.log('═══════════════════════════════════════════════════');

    const modalElement = document.querySelector('#modalDetalleCuponEmpresa');

    if (!modalElement) {
        console.warn('⚠️ Modal #modalDetalleCuponEmpresa no encontrado');
        return;
    }

    // ❶ Obtener instancia de Bootstrap
    const modalInstance = bootstrap.Modal.getInstance(modalElement);

    if (!modalInstance) {
        console.warn('⚠️ No hay instancia de Bootstrap Modal');
        return;
    }

    // ❷ ✅ Usar método nativo de Bootstrap para cerrar
    try {
        modalInstance.hide();
        console.log('✅ Modal cerrado con Bootstrap.hide()');
    } catch (error) {
        console.error('❌ Error al cerrar modal:', error);
    }
}

// ═══════════════════════════════════════════════════════════════════
// ✅ NUEVO v19.5: EVENTO DE LIMPIEZA AUTOMÁTICA DEL MODAL
// ═══════════════════════════════════════════════════════════════════

/**
 * Evento que se dispara cuando el modal se cierra completamente
 * Asegura limpieza exhaustiva del formulario
 */
$('#modalDetalleCuponEmpresa').off('hidden.bs.modal').on('hidden.bs.modal', function () {
    console.log('🧹 LIMPIEZA AUTOMÁTICA - MODAL CUPÓN EMPRESA');

    const $form = $('#formDetalleCuponEmpresa');
    $form[0].reset();
    $form.find('.form-control').removeClass('is-invalid is-valid');
    $('.invalid-feedback').remove();

    $('#lblEmpresaSeleccionada').text('-');
    $('#hdnEmpresaIdCupon').val('');

    const $backdropCupon = $('.modal-backdrop[data-modal="cuponempresa"]');
    if ($backdropCupon.length > 0) {
        $backdropCupon.remove();
    }

    console.log('✅ MODAL DE CUPÓN EMPRESA LIMPIADO');
});

// ═══════════════════════════════════════════════════════════════════
// ✅ NUEVO v20.0: LOTE 4 - FUNCIONES PARA CHEQUES
// ═══════════════════════════════════════════════════════════════════

/**
 * ✅ NUEVO v20.0: Carga la lista de bancos desde el servidor
 * Obtiene los bancos con los que se opera (bc_id, bc_lista)
 * 
 * @returns {Promise<Array>} - Array de objetos {bc_id, bc_lista}
 */
function cargarBancos() {
    console.log('═══════════════════════════════════════════════════');
    console.log('📡 CARGAR BANCOS v20.0');
    console.log('═══════════════════════════════════════════════════');

    // ❶ URL del endpoint
    const url = typeof obtenerBancosUrl !== 'undefined' && obtenerBancosUrl
        ? obtenerBancosUrl
        : '/Facturacion/Checkout/ObtenerBancos';

    console.log(`   URL: ${url}`);

    // ❷ Llamada AJAX
    return $.ajax({
        url: url,
        type: 'GET',
        dataType: 'json',
        timeout: 10000
    })
        .then(function (response) {
            console.log('✅ Respuesta recibida:', response);

            if (!response || !response.ok) {
                console.warn('⚠️ Respuesta no exitosa');
                return [];
            }

            // ✅ CORRECCIÓN: Soportar ambas propiedades (datos o bancos)
            const bancos = response.datos || response.bancos || [];

            if (!Array.isArray(bancos)) {
                console.warn('⚠️ Datos no son un array');
                return [];
            }

            console.log(`✅ ${bancos.length} bancos recibidos`);
            return bancos;
        })
        .fail(function (jqXHR, textStatus, errorThrown) {
            console.error('❌ ERROR AL CARGAR BANCOS');
            console.error('   Status:', textStatus);
            console.error('   Error:', errorThrown);

            if (typeof toastr !== 'undefined') {
                toastr.error(`Error al cargar bancos: ${textStatus}`);
            }

            return $.Deferred().reject(new Error(`Error: ${textStatus}`)).promise();
        });
}

/**
 * ✅ ACTUALIZADO v24.4: Abre el modal de detalle de Cheque SIN InputMask
 * CORRECCIÓN CRÍTICA: Eliminado InputMask para compatibilidad con teclado digital
 * 
 * CAMBIOS v24.4:
 * - Removido InputMaskMonetario del input monto
 * - Establecer valor inicial SIN FORMATO (solo números)
 * - Teclado digital escribe correctamente
 * 
 * @param {Object} instrumento - Instrumento seleccionado
 * @param {Object} tipoMedioPago - Tipo de MP (tcf_id='CH')
 */
function abrirModalDetalleCheque(instrumento, tipoMedioPago) {
    console.log('═══════════════════════════════════════════════════');
    console.log('🔓 ABRIR MODAL DETALLE CHEQUE v24.4 (SIN InputMask)');
    console.log(`   Instrumento: ${instrumento?.ins_desc || 'N/A'}`);
    console.log(`   Tipo MP: ${tipoMedioPago?.tcf_desc || 'N/A'}`);
    console.log('═══════════════════════════════════════════════════');

    // ❶ Validar instrumento
    if (!instrumento) {
        console.error('❌ CRÍTICO: Objeto instrumento es null/undefined');

        if (typeof toastr !== 'undefined') {
            toastr.error('Error: No se pudo cargar la información del cheque');
        }

        return;
    }

    // ❷ Obtener elemento del modal
    const modalElement = document.querySelector('#modalDetalleCheque');

    if (!modalElement) {
        console.error('❌ Modal #modalDetalleCheque no encontrado en el DOM');

        if (typeof toastr !== 'undefined') {
            toastr.error('El modal de cheques no está disponible');
        }

        return;
    }

    // ❸ Obtener o crear instancia de Bootstrap Modal
    let modalInstance = bootstrap.Modal.getInstance(modalElement);

    if (!modalInstance) {
        console.log('⚠️ Creando instancia de Bootstrap Modal...');

        try {
            modalInstance = new bootstrap.Modal(modalElement, {
                backdrop: 'static',
                keyboard: false
            });
            console.log('✅ Instancia creada correctamente');
        } catch (error) {
            console.error('❌ Error al crear instancia:', error);

            if (typeof toastr !== 'undefined') {
                toastr.error(`Error al inicializar el modal: ${error.message}`);
            }

            return;
        }
    }

    // ❹ Hidratar información del tipo e instrumento
    $('#lblTipoMedioPagoCheque').text(tipoMedioPago.tcf_desc);
    $('#lblInstrumentoCheque').text(`${instrumento.ins_desc} (${instrumento.ins_simbolo})`);

    // ❺ Establecer fecha actual por defecto
    const fechaHoy = new Date().toISOString().split('T')[0];
    $('#txtFechaCheque').val(fechaHoy);

    // ❻ Calcular monto sugerido
    const diferencia = Math.abs(conceptosPago.diferencia || 0);
    const montoSugerido = diferencia;

    console.log(`   💰 Monto sugerido: ${formatearMoneda(montoSugerido)}`);

    // ❼ Obtener input
    const $inputMonto = $('#txtMontoCheque');

    // ✅ CRÍTICO v24.4: REMOVER INPUTMASK SI EXISTE
    if (typeof InputMaskMonetario !== 'undefined') {
        console.log('🗑️ Removiendo InputMask existente...');
        InputMaskMonetario.removerMascara($inputMonto);
        console.log('   ✅ InputMask removido correctamente');
    }

    // ❽ ✅ NUEVO v24.4: Establecer valor inicial SIN FORMATO
    $inputMonto.val(montoSugerido.toFixed(2));

    console.log(`   ✅ Valor inicial: ${montoSugerido.toFixed(2)}`);
    console.log('   ✅ Teclado digital listo para escribir');

    // ❾ Limpiar campos
    $('#txtNroCheque').val('');
    $('#selectBancoCheque').val('').prop('disabled', true);

    // ❿ Limpiar validaciones previas
    $('#formDetalleCheque .form-control, #formDetalleCheque .form-select')
        .removeClass('is-invalid is-valid');
    $('.invalid-feedback').remove();

    // ⓫ Bloquear modal mientras carga bancos
    bloquearModalCheque('Cargando bancos...');

    // ⓬ Cargar bancos y renderizar combobox
    cargarBancos()
        .then(function (bancos) {
            console.log('✅ Bancos obtenidos:', bancos);
            renderizarComboBancos(bancos);
            desbloquearModalCheque();
        })
        .catch(function (error) {
            console.error('❌ Error al cargar bancos:', error);

            $('#selectBancoCheque').html(`
                <option value="" disabled selected>Error al cargar bancos</option>
            `);

            desbloquearModalCheque();

            if (typeof toastr !== 'undefined') {
                toastr.error('No se pudieron cargar los bancos. Intente nuevamente.');
            }
        });

    // ⓭ Mostrar modal con Bootstrap
    try {
        modalInstance.show();
        console.log('✅ Modal mostrado con Bootstrap.show()');

        setTimeout(() => {
            $(modalElement).css('z-index', '5100');

            const $backdrops = $('.modal-backdrop');
            if ($backdrops.length > 0) {
                $backdrops.last().css('z-index', '5099');
            }

            console.log('   ✅ Z-index ajustado');
        }, 200);

    } catch (error) {
        console.error('❌ ERROR al mostrar modal:', error);

        if (typeof toastr !== 'undefined') {
            toastr.error(`Error al abrir el modal: ${error.message}`);
        }

        return;
    }

    // ⓮ Vincular eventos de guardar
    $('#btnGuardarDetalleCheque')
        .off('click.guardarCheque')
        .on('click.guardarCheque', function () {
            guardarDetalleCheque(instrumento, tipoMedioPago);
        });

    // ⓯ Vincular evento Enter en el último campo
    $inputMonto
        .off('keypress.enterCheque')
        .on('keypress.enterCheque', function (e) {
            if (e.which === 13) {
                e.preventDefault();
                guardarDetalleCheque(instrumento, tipoMedioPago);
            }
        });

    console.log('═══════════════════════════════════════════════════');
    console.log('✅ INPUT LISTO PARA TECLADO DIGITAL:');
    console.log('   - InputMask: DESACTIVADO ❌');
    console.log('   - Teclado digital: ACTIVO ✅');
    console.log('   - Formato: SOLO NÚMEROS (sin $, sin comas)');
    console.log('   - Validación: NATIVA HTML5');
    console.log('═══════════════════════════════════════════════════');

    console.log('✅ Modal detalle cheque configurado correctamente');
}

/**
 * ✅ ACTUALIZADO v20.3: Renderiza el combobox de bancos
 * NUEVO: Incluye bc_plaza en data-attribute de cada opción
 * 
 * @param {Array} bancos - Array de objetos {bc_id, bc_lista, bc_plaza}
 */
function renderizarComboBancos(bancos) {
    console.log('═══════════════════════════════════════════════════');
    console.log('🎨 RENDERIZAR COMBO BANCOS v20.3');
    console.log(`   Total bancos: ${bancos?.length || 0}`);
    console.log('═══════════════════════════════════════════════════');

    const $select = $('#selectBancoCheque');
    $select.empty();

    // ❶ Opción por defecto
    $select.append('<option value="" disabled selected>Seleccione un banco</option>');

    // �② Validar que haya bancos
    if (!bancos || bancos.length === 0) {
        console.warn('⚠️ No hay bancos para renderizar');

        $select.append('<option value="" disabled>No hay bancos disponibles</option>');
        $select.prop('disabled', true);

        return;
    }

    // ❸ Agregar opciones de bancos con plaza en data-attribute
    bancos.forEach(function (banco) {
        const bcId = banco.bc_id || banco.id || '';
        const bcLista = banco.bc_lista || banco.nombre || 'Banco sin nombre';
        const bcPlaza = banco.bc_plaza || ''; // ✅ NUEVO: Obtener plaza del objeto

        // ✅ NUEVO: Agregar data-plaza al option
        $select.append(
            `<option value="${escapeHtml(bcId)}" data-plaza="${escapeHtml(bcPlaza)}">${escapeHtml(bcLista)}</option>`
        );

        console.log(`   ✅ Banco: ${bcLista} (${bcId}) - Plaza: ${bcPlaza || 'N/A'}`);
    });

    // ❹ Habilitar combobox
    $select.prop('disabled', false);

    console.log('✅ Combo de bancos renderizado correctamente');
}

/**
 * ✅ ACTUALIZADO v24.4: Guarda el detalle del cheque SIN InputMask
 * CAMBIO CRÍTICO: Parseo directo del valor sin InputMaskMonetario
 * 
 * LÓGICA:
 * 1. Obtener valor del input como string
 * 2. Limpiar caracteres no numéricos (excepto punto/coma decimal)
 * 3. Parsear con parseFloat()
 * 4. Validar y guardar
 * 
 * VALIDACIONES:
 * - Banco: Obligatorio
 * - Nro Cheque: Obligatorio, numérico, máximo 8 caracteres
 * - Fecha: Obligatoria, >= hoy, <= hoy + 365 días
 * - Monto: > 0, <= Saldo factura (con tolerancia)
 * 
 * @param {Object} instrumento - Datos del instrumento
 * @param {Object} tipoMedioPago - Tipo de medio de pago
 */
function guardarDetalleCheque(instrumento, tipoMedioPago) {
    console.log('═══════════════════════════════════════════════════');
    console.log('💾 GUARDAR DETALLE CHEQUE v24.4 (SIN InputMask)');
    console.log('═══════════════════════════════════════════════════');

    // ❶ Obtener valores del formulario
    const $selectBanco = $('#selectBancoCheque');
    const bancoId = $selectBanco.val();
    const bancoTexto = $selectBanco.find('option:selected').text();

    const plaza = $selectBanco.find('option:selected').data('plaza') || '';

    const nroCheque = $('#txtNroCheque').val().trim();
    const fechaCheque = $('#txtFechaCheque').val();

    console.log('📋 Datos del formulario:');
    console.log(`   Banco ID: "${bancoId}"`);
    console.log(`   Banco: "${bancoTexto}"`);
    console.log(`   Plaza (desde BD): "${plaza || 'N/A'}"`);
    console.log(`   Nro Cheque: "${nroCheque}"`);
    console.log(`   Fecha: "${fechaCheque}"`);

    // ❷ Validar Banco
    if (!bancoId || bancoId === '') {
        console.warn('⚠️ Banco no seleccionado');
        mostrarErrorCampo('#selectBancoCheque', 'Debe seleccionar un banco');
        return;
    }

    // ❸ Validar Nro Cheque (numérico, máximo 8 dígitos)
    if (!nroCheque || nroCheque === '') {
        console.warn('⚠️ Número de cheque vacío');
        mostrarErrorCampo('#txtNroCheque', 'Debe ingresar el número de cheque');
        return;
    }

    if (!/^\d+$/.test(nroCheque)) {
        console.warn('⚠️ Número de cheque no es numérico');
        mostrarErrorCampo('#txtNroCheque', 'El número de cheque debe ser numérico');
        return;
    }

    if (nroCheque.length > 8) {
        console.warn('⚠️ Número de cheque demasiado largo');
        mostrarErrorCampo('#txtNroCheque', 'El número de cheque no puede tener más de 8 dígitos');
        return;
    }

    // ❹ Validar Fecha
    if (!fechaCheque) {
        console.warn('⚠️ Fecha no ingresada');
        mostrarErrorCampo('#txtFechaCheque', 'Debe seleccionar la fecha del cheque');
        return;
    }

    // ❺ Validar fecha >= hoy
    const fechaChq = new Date(fechaCheque);
    const fechaHoy = new Date();
    fechaHoy.setHours(0, 0, 0, 0);

    if (fechaChq < fechaHoy) {
        console.warn('⚠️ Fecha de cheque es pasada');
        mostrarErrorCampo('#txtFechaCheque', 'La fecha del cheque no puede ser anterior a hoy');
        return;
    }

    // ❻ Validar fecha <= hoy + 365 días
    const diasMaximos = 365;
    const fechaMaxima = new Date(fechaHoy);
    fechaMaxima.setDate(fechaMaxima.getDate() + diasMaximos);

    if (fechaChq > fechaMaxima) {
        console.warn(`⚠️ Fecha de cheque supera el límite de ${diasMaximos} días`);
        mostrarErrorCampo(
            '#txtFechaCheque',
            `La fecha del cheque no puede ser mayor a ${diasMaximos} días desde hoy`
        );
        return;
    }

    // ❼ ✅ ACTUALIZADO v24.4: Obtener y limpiar valor del input
    const montoStr = $('#txtMontoCheque').val().trim();

    // Limpiar: remover todo excepto dígitos, punto y coma
    // Convertir coma a punto (por si el usuario usó coma decimal)
    const montoLimpio = montoStr.replace(/[^\d.,]/g, '').replace(',', '.');

    // Parsear a número flotante
    const monto = parseFloat(montoLimpio) || 0;

    console.log(`   📝 Valor del input: "${montoStr}"`);
    console.log(`   🔧 Valor limpio: "${montoLimpio}"`);
    console.log(`   💰 Monto parseado: ${monto}`);

    // ❽ Validar monto > 0
    if (isNaN(monto) || monto <= 0) {
        console.warn('⚠️ Monto inválido o cero');
        mostrarErrorCampo('#txtMontoCheque', 'Debe ingresar un monto válido mayor a cero');
        return;
    }

    // ❾ Validar monto <= saldo factura (con tolerancia)
    const diferenciaFactura = Math.abs(conceptosPago.diferencia || 0);

    if (monto > diferenciaFactura * LIMITE_PORCENTAJE_DIFERENCIA) {
        console.warn(`⚠️ Monto muy alto: ${monto} > ${diferenciaFactura * LIMITE_PORCENTAJE_DIFERENCIA}`);

        const mensajeHtml = `
        <div class="text-start">
            <p class="mb-3">El monto ingresado es <strong>mayor</strong> a la diferencia pendiente:</p>
            <table class="table table-sm table-borderless mb-0">
                <tr>
                    <td class="text-end">Monto ingresado:</td>
                    <td class="text-start"><strong class="text-danger">${formatearMoneda(monto)}</strong></td>
                </tr>
                <tr>
                    <td class="text-end">Diferencia pendiente:</td>
                    <td class="text-start"><strong class="text-warning">${formatearMoneda(diferenciaFactura)}</strong></td>
                </tr>
            </table>
            <p class="mt-3 mb-0"><i class="bx bx-info-circle"></i> ¿Desea continuar?</p>
        </div>
    `;

        AbrirMensaje(
            "¿Monto elevado?",
            mensajeHtml,
            function () {
                $('#msjModal').modal('hide');
                finalizarGuardadoCheque(monto, bancoId, bancoTexto, nroCheque, plaza, fechaCheque, instrumento, tipoMedioPago);
            },
            false,
            ["Continuar", "Corregir"],
            "warn!",
            function () {
                $('#msjModal').modal('hide');
                setTimeout(() => {
                    $('#txtMontoCheque').trigger("focus").trigger("select");
                }, 300);
            }
        );

        return;
    }

    // ❿ Si validaciones OK, finalizar guardado
    finalizarGuardadoCheque(monto, bancoId, bancoTexto, nroCheque, plaza, fechaCheque, instrumento, tipoMedioPago);
}

/**
 * ✅ ACTUALIZADO v20.2: Finaliza el guardado del cheque
 * NUEVO: Incluye campo plaza en el detalle
 */
function finalizarGuardadoCheque(monto, bancoId, bancoTexto, nroCheque, plaza, fechaCheque, instrumento, tipoMedioPago) {
    console.log('✅ Finalizando guardado de cheque...');
    console.log(`   Monto: ${monto}`);
    console.log(`   Banco: ${bancoTexto} (${bancoId})`);
    console.log(`   Nro Cheque: ${nroCheque}`);
    console.log(`   Plaza: ${plaza || 'N/A'}`);
    console.log(`   Fecha: ${fechaCheque}`);

    // ❶ Crear objeto de detalle
    const detalleCheque = {
        banco_id: bancoId,
        banco_nombre: bancoTexto,
        nro_cheque: nroCheque,
        plaza: plaza || '', // ✅ NUEVO: Incluir plaza
        fecha_cheque: fechaCheque
    };

    // ❷ Crear objeto de valor
    const nuevoValor = {
        id: ++valorIdCounter,
        tcf_id: tipoMedioPago.tcf_id,
        tcf_desc: tipoMedioPago.tcf_desc,
        ins_id: instrumento.ins_id,
        ins_desc: instrumento.ins_desc,
        ins_simbolo: instrumento.ins_simbolo || '$',
        importe: monto,
        observacion: `CH ${nroCheque} - ${bancoTexto} - ${fechaCheque}${plaza ? ' - Plaza: ' + plaza : ''}`,
        detalle: detalleCheque,
        fecha_creacion: new Date().toISOString()
    };

    console.log('📦 Nuevo valor creado:', nuevoValor);

    // ❸ Agregar a array global
    valoresPago.push(nuevoValor);

    // ❹ Agregar fila a la tabla
    agregarFilaValor(nuevoValor);

    // ❺ Actualizar totales
    actualizarTotalesPago();

    // ❻ Cerrar modal
    cerrarModalDetalleCheque();

    // ❼ Notificación
    if (typeof toastr !== 'undefined') {
        toastr.success(
            `Cheque agregado: ${formatearMoneda(monto)} - ${bancoTexto}`,
            'Valor guardado',
            { timeOut: 3000 }
        );
    }

    console.log('✅ Valor de cheque guardado correctamente');
}

/**
 * ✅ NUEVO v20.0: Cierra el modal de detalle de cheque
 */
function cerrarModalDetalleCheque() {
    console.log('═══════════════════════════════════════════════════');
    console.log('🔒 CERRAR MODAL DETALLE CHEQUE v20.0');
    console.log('═══════════════════════════════════════════════════');

    const modalElement = document.querySelector('#modalDetalleCheque');

    if (!modalElement) {
        console.warn('⚠️ Modal #modalDetalleCheque no encontrado');
        return;
    }

    // ❶ Obtener instancia de Bootstrap
    const modalInstance = bootstrap.Modal.getInstance(modalElement);

    if (!modalInstance) {
        console.warn('⚠️ No hay instancia de Bootstrap Modal');
        return;
    }

    // ❷ Usar método nativo de Bootstrap para cerrar
    try {
        modalInstance.hide();
        console.log('✅ Modal cerrado con Bootstrap.hide()');
    } catch (error) {
        console.error('❌ Error al cerrar modal:', error);
    }
}

/**
 * ✅ NUEVO v20.0: Bloquea el modal de cheque mientras carga datos
 * @param {string} mensaje - Mensaje a mostrar
 */
function bloquearModalCheque(mensaje) {
    if ($('#overlayDetalleCheque').length === 0) {
        $('#modalDetalleCheque .modal-content').append(`
            <div id="overlayDetalleCheque" style="
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
                    <p class="fw-bold text-golden-dark" id="mensajeOverlayCheque">${mensaje}</p>
                </div>
            </div>
        `);
    } else {
        $('#mensajeOverlayCheque').text(mensaje);
        $('#overlayDetalleCheque').fadeIn(200);
    }

    $('#btnGuardarDetalleCheque').prop('disabled', true);
}

/**
 * ✅ NUEVO v20.0: Desbloquea el modal de cheque
 */
function desbloquearModalCheque() {
    $('#overlayDetalleCheque').fadeOut(300, function () {
        $(this).remove();
    });

    $('#btnGuardarDetalleCheque').prop('disabled', false);
}

/**
 * ✅ NUEVO v20.0: Evento de limpieza automática del modal de cheque
 */
$('#modalDetalleCheque').off('hidden.bs.modal').on('hidden.bs.modal', function () {
    console.log('🧹 LIMPIEZA AUTOMÁTICA - MODAL CHEQUE');

    const $form = $('#formDetalleCheque');
    $form[0].reset();
    $form.find('.form-control, .form-select').removeClass('is-invalid is-valid');
    $('.invalid-feedback').remove();

    $('#lblTipoMedioPagoCheque').text('-');
    $('#lblInstrumentoCheque').text('-');
    $('#selectBancoCheque').val('').prop('disabled', true);

    console.log('✅ MODAL DE CHEQUE LIMPIADO');
});

// ═══════════════════════════════════════════════════════════════════
// ✅ NUEVO v21.0: SELECCIÓN AUTOMÁTICA DEL PRIMER ITEM EN MODALES
// ═══════════════════════════════════════════════════════════════════

/**
 * ✅ NUEVO v21.0: Selecciona automáticamente el primer item visible de una lista
 * Función genérica reutilizable para todos los modales de instrumentos
 * 
 * FUNCIONALIDAD:
 * - Busca el primer item visible que no esté deshabilitado
 * - Aplica clase 'selected' (resaltado azul)
 * - Habilita el botón de confirmar correspondiente
 * - Guarda datos del item según el tipo de modal
 * 
 * @param {Object} config - Configuración del modal
 * @param {string} config.contenedorId - ID del contenedor de la lista (ej: '#listaInstrumentos')
 * @param {string} config.itemClass - Clase CSS de los items (ej: '.instrumento-item')
 * @param {string} config.btnConfirmarId - ID del botón confirmar (ej: '#btnConfirmarInstrumento')
 * @param {string} config.tipoModal - Tipo de modal ('instrumentos'|'transferencia'|'vale'|'cupon')
 * @returns {boolean} - true si se seleccionó un item, false si no hay items
 */
function seleccionarPrimerItemAutomatico(config) {
    console.log('═══════════════════════════════════════════════════');
    console.log('🎯 SELECCIONAR PRIMER ITEM AUTOMÁTICO v21.0');
    console.log(`   Modal: ${config.tipoModal}`);
    console.log(`   Contenedor: ${config.contenedorId}`);
    console.log('═══════════════════════════════════════════════════');

    // ❶ Obtener todos los items visibles y no deshabilitados
    const $items = $(`${config.contenedorId} ${config.itemClass}:visible:not(.disabled)`);

    console.log(`   📊 Items encontrados: ${$items.length}`);

    // ❷ Validar que existan items
    if ($items.length === 0) {
        console.warn('   ⚠️ No hay items disponibles para seleccionar');

        // Deshabilitar botón confirmar
        $(config.btnConfirmarId).prop('disabled', true);

        return false;
    }

    // ❸ Obtener el primer item
    const $primerItem = $items.first();

    console.log('   ✅ Primer item encontrado:');
    console.log(`      ID: ${$primerItem.data('instrumento-id') || $primerItem.data('banco-id') || $primerItem.data('vale-id') || $primerItem.data('cupon-id')}`);

    // ❹ Limpiar selecciones previas
    $items.removeClass('selected active');

    // ❺ Seleccionar el primer item
    $primerItem.addClass('selected');

    // ❻ Habilitar botón confirmar
    $(config.btnConfirmarId).prop('disabled', false);

    // ❼ Guardar datos según el tipo de modal
    switch (config.tipoModal) {
        case 'instrumentos':
            // Para modal de instrumentos (monedas)
            window._instrumentoSeleccionado = {
                ins_id: $primerItem.data('instrumento-id'),
                ins_desc: $primerItem.data('instrumento-desc'),
                ins_simbolo: $primerItem.data('instrumento-simbolo'),
                tiene_detalle: $primerItem.data('instrumento-tiene-detalle'),
                total_actual: $primerItem.data('instrumento-total')
            };
            console.log('   💾 _instrumentoSeleccionado guardado');
            break;

        case 'transferencia':
        case 'vale':
        case 'cupon':
            // Para otros modales (no requieren guardar en variable global)
            console.log('   ℹ️ Modal tipo secundario - No requiere variable global');
            break;

        default:
            console.warn(`   ⚠️ Tipo de modal desconocido: ${config.tipoModal}`);
            break;
    }

    // ❽ Scroll al item (opcional)
    if ($primerItem[0]) {
        $primerItem[0].scrollIntoView({
            behavior: 'smooth',
            block: 'nearest'
        });
        console.log('   📜 Scroll realizado al primer item');
    }

    console.log('═══════════════════════════════════════════════════');
    console.log('✅ PRIMER ITEM SELECCIONADO CORRECTAMENTE');
    console.log('═══════════════════════════════════════════════════');

    return true;
}

// ═══════════════════════════════════════════════════════════════════
// ✅ NUEVO v21.3: NAVEGACIÓN CON TECLADO EN MODALES DE INSTRUMENTOS
// ═══════════════════════════════════════════════════════════════════

/**
 * ✅ NUEVO v21.3: Habilita navegación con teclado en un modal de instrumentos
 * Función genérica reutilizable para todos los modales de selección de instrumentos
 * 
 * FUNCIONALIDAD:
 * - Navegación cíclica con flechas ↑↓
 * - Confirmación con Enter
 * - Cancelación con Escape
 * - Scroll automático si el item está fuera de vista
 * - Delegación de eventos en document (evita bug de primera apertura)
 * 
 * INSPIRADO EN: manejarNavegacionTeclado() del modal tipo medio de pago (v21.1)
 * CORRECCIÓN v21.2: Usa delegación en document en lugar de modal directo
 * 
 * @param {Object} config - Configuración del modal
 * @param {string} config.modalId - ID del modal (ej: '#modalInstrumentos')
 * @param {string} config.contenedorId - ID del contenedor de la lista (ej: '#listaInstrumentos')
 * @param {string} config.itemClass - Clase CSS de los items (ej: '.instrumento-item')
 * @param {string} config.btnConfirmarId - ID del botón confirmar (ej: '#btnConfirmarInstrumento')
 * @param {Function} config.onConfirmar - Callback al confirmar con Enter
 */
function habilitarNavegacionTecladoInstrumentos(config) {
    console.log('═══════════════════════════════════════════════════');
    console.log('🔧 HABILITAR NAVEGACIÓN CON TECLADO v21.3');
    console.log(`   Modal: ${config.modalId}`);
    console.log(`   Contenedor: ${config.contenedorId}`);
    console.log('═══════════════════════════════════════════════════');

    // ❶ Crear namespace único para evitar conflictos entre modales
    const namespace = `keydown.nav${config.modalId.replace(/[^a-zA-Z0-9]/g, '')}`;
    console.log(`   📛 Namespace: ${namespace}`);

    // ❷ ✅ CRÍTICO: Delegación de eventos en document (FIX v21.2)
    // Previene bug de primera apertura donde el modal aún no está en el DOM
    $(document)
        .off(namespace) // Limpiar eventos previos del mismo namespace
        .on(namespace, function (e) {
            // ❸ Validar que el modal esté visible y activo
            const $modal = $(config.modalId);

            if (!$modal.hasClass('show')) {
                // Modal no visible, ignorar evento
                return;
            }

            // ❹ Obtener todos los items visibles
            const $items = $(`${config.contenedorId} ${config.itemClass}:visible`);

            if ($items.length === 0) {
                console.warn(`⚠️ No hay items disponibles en ${config.contenedorId}`);
                return;
            }

            // ❺ Obtener item actualmente seleccionado
            const $itemActual = $(`${config.itemClass}.selected`);

            if ($itemActual.length === 0) {
                console.warn(`⚠️ No hay item seleccionado en ${config.contenedorId}`);
                return;
            }

            const indiceActual = $items.index($itemActual);
            const totalItems = $items.length;

            console.log(`🎹 Tecla: ${e.key} | Índice actual: ${indiceActual}/${totalItems - 1}`);

            // ❻ Procesar tecla presionada
            switch (e.key) {
                case 'ArrowDown': // ↓ Siguiente
                    e.preventDefault();

                    console.log('⬇️ FLECHA ABAJO - Siguiente item');

                    // Calcular índice siguiente (cíclico)
                    const indiceSiguiente = (indiceActual + 1) % totalItems;
                    const $itemSiguiente = $items.eq(indiceSiguiente);

                    console.log(`   Moviendo de índice ${indiceActual} → ${indiceSiguiente}`);

                    // Seleccionar siguiente item
                    seleccionarItemInstrumento($itemSiguiente, config);

                    // Hacer scroll si es necesario
                    scrollToItem($itemSiguiente, config.contenedorId);

                    break;

                case 'ArrowUp': // ↑ Anterior
                    e.preventDefault();

                    console.log('⬆️ FLECHA ARRIBA - Item anterior');

                    // Calcular índice anterior (cíclico)
                    const indiceAnterior = (indiceActual - 1 + totalItems) % totalItems;
                    const $itemAnterior = $items.eq(indiceAnterior);

                    console.log(`   Moviendo de índice ${indiceActual} → ${indiceAnterior}`);

                    // Seleccionar anterior item
                    seleccionarItemInstrumento($itemAnterior, config);

                    // Hacer scroll si es necesario
                    scrollToItem($itemAnterior, config.contenedorId);

                    break;

                case 'Enter': // ⏎ Confirmar
                    e.preventDefault();

                    console.log('⏎ ENTER - Confirmando selección');

                    // Ejecutar callback de confirmación
                    if (config.onConfirmar && typeof config.onConfirmar === 'function') {
                        console.log('   ✅ Ejecutando callback onConfirmar');
                        config.onConfirmar();
                    } else {
                        console.warn('   ⚠️ No hay callback onConfirmar definido');
                    }

                    break;

                case 'Escape': // Esc Cancelar
                    e.preventDefault();

                    console.log('🚫 ESCAPE - Cerrando modal');

                    // Cerrar modal con jQuery (compatible con Bootstrap 5)
                    $(config.modalId).modal('hide');

                    break;

                default:
                    // Otras teclas no manejadas
                    break;
            }
        });

    console.log(`✅ Navegación con teclado habilitada: ${namespace}`);
}

/**
 * ✅ NUEVO v21.3: Selecciona un item específico en un modal de instrumentos
 * Función auxiliar para unificar la lógica de selección
 * 
 * @param {jQuery} $item - Item jQuery a seleccionar
 * @param {Object} config - Configuración del modal
 */
function seleccionarItemInstrumento($item, config) {
    console.log(`   🔘 Seleccionando item: ${$item.data('instrumento-id') || $item.data('banco-id') || $item.data('vale-id') || $item.data('cupon-id')}`);

    // ❶ Limpiar selecciones previas
    $(`${config.itemClass}`).removeClass('selected active');

    // ❷ Seleccionar el item actual
    $item.addClass('selected');

    // ❸ Habilitar botón confirmar
    $(config.btnConfirmarId).prop('disabled', false);

    console.log('   ✅ Item seleccionado correctamente');
}

/**
 * ✅ NUEVO v21.3: Limpia eventos de navegación con teclado de un modal específico
 * Previene memory leaks por eventos huérfanos
 * 
 * CUÁNDO SE LLAMA:
 * - Automáticamente al cerrar modal (evento 'hidden.bs.modal')
 * 
 * @param {string} modalId - ID del modal (ej: '#modalInstrumentos')
 */
function limpiarNavegacionTecladoInstrumentos(modalId) {
    console.log('═══════════════════════════════════════════════════');
    console.log('🧹 LIMPIAR NAVEGACIÓN CON TECLADO v21.3');
    console.log(`   Modal: ${modalId}`);
    console.log('═══════════════════════════════════════════════════');

    // ❶ Calcular namespace del modal
    const namespace = `keydown.nav${modalId.replace(/[^a-zA-Z0-9]/g, '')}`;
    console.log(`   📛 Namespace a limpiar: ${namespace}`);

    // ❷ Remover evento delegado de document
    $(document).off(namespace);

    console.log('   ✅ Eventos de teclado limpiados correctamente');
    console.log('═══════════════════════════════════════════════════');
}