// ============================================
// GESTOR DE PRODUCTOS DE FACTURACIÓN
// ============================================
// VERSIÓN CORREGIDA - NO MODIFICA fact.js
// Integración mediante eventos personalizados
// ============================================

// ====== VARIABLES GLOBALES ======
let productosFactura = [];
let totalFactura = 0;   
let clienteActualFactura = null;

// ====== INICIALIZACIÓN ======
$(function () {
    console.log('🚀 Módulo de Productos de Factura inicializado');
    inicializarEventosProductos();
    configurarListenersIntegracion();
});

// ====== CONFIGURACIÓN DE LISTENERS PARA INTEGRACIÓN ======
/**
 * ✅ NUEVO ENFOQUE: Escucha eventos personalizados desde fact.js
 * NO modifica fact.js, solo se suscribe a eventos
 */
function configurarListenersIntegracion() {
    console.log('🔧 Configurando listeners de integración...');
    
    // ✅ Escuchar evento personalizado cuando se confirma un cliente
    $(document).on('clienteConfirmado', function(event, clienteData) {
        console.log('═══════════════════════════════════════════════════');
        console.log('📡 EVENTO RECIBIDO: clienteConfirmado');
        console.log('═══════════════════════════════════════════════════');
        console.log('Datos del cliente:', clienteData);
        
        // Guardar cliente actual
        clienteActualFactura = clienteData;
        
        // Mostrar sección de productos
        mostrarSeccionProductos(clienteData);
    });
    
    // ✅ Escuchar evento quando se cancela/limpia el cliente
    $(document).on('clienteCancelado', function() {
        console.log('📡 EVENTO RECIBIDO: clienteCancelado');
        ocultarSeccionProductos();
    });
    
    console.log('✅ Listeners de integración configurados');
}

// ====== EVENTOS PRINCIPALES ======
function inicializarEventosProductos() {
    console.log('🔧 Configurando eventos de productos...');
    
    // Buscar producto (Enter)
    $('#txtCodigoProducto').on('keypress', function (e) {
        if (e.which === 13) {
            e.preventDefault();
            buscarProducto();
        }
    });
    
    // Buscar producto (botón)
    $('#btnBuscarProducto').on('click', function () {
        buscarProducto();
    });
    
    // ✅ CRÍTICO: Botón CANCELAR - Vuelve a identificar cliente
    $('#btnCancelarFactura').on('click', function () {
        console.log('🔙 Usuario solicitó cancelar factura...');
        confirmarCancelarFactura();
    });
    
    // Botón SEGUIR (Confirmar factura)
    $('#btnConfirmarFactura').on('click', function () {
        console.log('✅ Usuario solicitó confirmar factura...');
        confirmarFactura();
    });
    
    // Botones de acción rápida
    $('#btnPreFactura').on('click', function() {
        console.log('📄 Pre-Factura...');
        // TODO: Implementar
    });
    
    $('#btnFacturaEmitida').on('click', function() {
        console.log('🧾 Factura Emitida...');
        // TODO: Implementar
    });
    
    $('#btnCotizacion').on('click', function() {
        console.log('💰 Cotización...');
        // TODO: Implementar
    });
    
    $('#btnUltimoDetalle').on('click', function() {
        console.log('🕒 Último Detalle...');
        // TODO: Implementar
    });
    
    console.log('✅ Eventos configurados correctamente');
}

// ====== MOSTRAR SECCIÓN DE PRODUCTOS ====== (✅ ACTUALIZADO v3.0)
/**
 * ✅ ACTUALIZADO v3.0: Ahora abre el modal en lugar de mostrar div
 * 
 * CAMBIOS v3.0:
 * - Cambió de fadeIn() a modal('show')
 * - Actualizado ID de #seccionProductosFactura a #modalProductosFactura
 */
function mostrarSeccionProductos(clienteData) {
    console.log('═══════════════════════════════════════════════════');
    console.log('📦 MOSTRAR MODAL DE PRODUCTOS v3.0');
    console.log('═══════════════════════════════════════════════════');
    console.log('Cliente recibido:', clienteData);
    
    // ❶ Hidratar datos del cliente en el modal
    $('#txtClienteNombre').val(clienteData.nombre || '');
    $('#txtClienteId').val(clienteData.id || 'N/A');
    $('#txtClienteDomicilio').val(clienteData.domicilio || '');
    $('#txtCondicionAfip').val(clienteData.condicionAfip || '');
    $('#txtClienteCuit').val(clienteData.tipoNumero || '');
    $('#txtClienteEmail').val(clienteData.email || '');
    $('#txtClienteMovil').val(clienteData.movil || '');
    
    // ❷ Actualizar badge de tipo de comprobante
    actualizarTipoComprobante(clienteData);
    
    // ❸ ✅ NUEVO v3.0: Abrir modal en lugar de fadeIn
    $('#modalProductosFactura').modal('show');
    console.log('✅ Modal de productos abierto');
    
    // ❹ Focus en campo de búsqueda (después de que el modal se muestre)
    $('#modalProductosFactura').on('shown.bs.modal', function () {
        setTimeout(() => {
            $('#txtCodigoProducto').trigger('focus');
        }, 200);
    });
    
    console.log('✅ Modal de productos mostrado correctamente');
}

// ====== ACTUALIZAR TIPO DE COMPROBANTE ======
function actualizarTipoComprobante(clienteData) {
    const $badge = $('#badgeTipoComprobante');
    
    // Determinar tipo de factura según condición AFIP
    let tipoFactura = 'FACTURA B'; // Por defecto
    let iconoFactura = 'bx-file';
    
    if (clienteData.condicionAfip) {
        const condicion = clienteData.condicionAfip.toUpperCase();
        
        if (condicion.includes('INSCRIPTO') || condicion.includes('MONOTRIBUTO')) {
            tipoFactura = 'FACTURA A';
            iconoFactura = 'bx-file-blank';
        } else if (condicion.includes('EXENTO')) {
            tipoFactura = 'FACTURA C';
            iconoFactura = 'bx-file';
        }
    }
    
    $badge.html(`<i class='bx ${iconoFactura}'></i> ${tipoFactura}`);
    console.log(`📋 Tipo de comprobante: ${tipoFactura}`);
}

// ====== OCULTAR SECCIÓN DE PRODUCTOS ====== (✅ ACTUALIZADO v3.0)
/**
 * ✅ ACTUALIZADO v3.0: Ahora cierra el modal en lugar de fadeOut
 */
function ocultarSeccionProductos() {
    console.log('═══════════════════════════════════════════════════');
    console.log('🔙 OCULTAR MODAL DE PRODUCTOS v3.0');
    console.log('═══════════════════════════════════════════════════');
    
    // ❶ Limpiar campos de cliente
    $('#txtClienteNombre').val('');
    $('#txtClienteId').val('');
    $('#txtClienteDomicilio').val('');
    $('#txtCondicionAfip').val('');
    $('#txtClienteCuit').val('');
    $('#txtClienteEmail').val('');
    $('#txtClienteMovil').val('');
    
    // ❷ Limpiar campo de búsqueda
    $('#txtCodigoProducto').val('');
    
    // ❸ Limpiar grilla de productos
    limpiarGrillaProductos();
    
    // ❹ Limpiar cliente actual
    clienteActualFactura = null;
    
    // ❺ ✅ NUEVO v3.0: Cerrar modal en lugar de fadeOut
    $('#modalProductosFactura').modal('hide');
    
    console.log('✅ Modal de productos ocultado');
}

// ====== CONFIRMAR CANCELAR FACTURA ======
function confirmarCancelarFactura() {
    console.log('⚠️ Confirmando cancelación de factura...');
    
    // ❶ Verificar si hay productos cargados
    if (productosFactura.length > 0) {
        // Mostrar confirmación
        AbrirMensaje(
            "Confirmar Cancelación",
            "¿Está seguro que desea cancelar la factura?\n\n" +
            "Se perderán todos los productos cargados.",
            function () {
                $("#msjModal").modal("hide");
                ejecutarCancelarFactura();
            },
            true, // Mostrar botón cancelar
            ["Sí, cancelar", "No"],
            "warning",
            null
        );
    } else {
        // No hay productos, cancelar directamente
        ejecutarCancelarFactura();
    }
}

// ====== EJECUTAR CANCELAR FACTURA ======
function ejecutarCancelarFactura() {
    console.log('🔙 Ejecutando cancelación de factura...');
    
    // ❶ Ocultar sección de productos
    ocultarSeccionProductos();
    
    // ❷ ✅ NUEVO ENFOQUE: Disparar evento personalizado para que fact.js lo escuche
    // NO llamamos directamente a funciones de fact.js
    $(document).trigger('volverAIdentificarCliente');
    
    console.log('✅ Evento "volverAIdentificarCliente" disparado');
    console.log('   fact.js debería abrir el modal automáticamente');
}

// ====== BUSCAR PRODUCTO ======
function buscarProducto() {
    const codigo = $('#txtCodigoProducto').val().trim();
    
    if (!codigo) {
        mostrarMensajeError('Por favor, ingrese un código de producto');
        return;
    }
    
    console.log(`🔍 Buscando producto: ${codigo}`);
    
    // TODO: Implementar búsqueda de producto (AJAX)
    // Por ahora, solo log
    console.log('⚠️ TODO: Implementar búsqueda de producto');
    
    // Ejemplo de cómo se agregaría un producto
    // agregarProducto(productoData);
}

// ====== AGREGAR PRODUCTO A LA GRILLA (EJEMPLO) ======
function agregarProducto(producto) {
    console.log('➕ Agregando producto:', producto);
    
    // TODO: Implementar lógica de agregado
    // - Validar stock
    // - Calcular precio
    // - Agregar a array productosFactura
    // - Actualizar grilla
    // - Actualizar total
}

// ====== CONFIRMAR FACTURA ======
function confirmarFactura() {
    console.log('✅ Confirmando factura...');
    
    // ❶ Validar que haya productos
    if (productosFactura.length === 0) {
        mostrarMensajeError('Debe cargar al menos un producto para continuar');
        return;
    }
    
    // TODO: Implementar confirmación de factura
    // - Validar datos
    // - Generar factura
    // - Enviar a AFIP (si corresponde)
    // - Imprimir
    console.log('⚠️ TODO: Implementar confirmación de factura');
}

// ====== LIMPIAR GRILLA DE PRODUCTOS ======
function limpiarGrillaProductos() {
    console.log('🧹 Limpiando grilla de productos...');
    
    productosFactura = [];
    totalFactura = 0;
    
    $('#tbodyProductos').html(`
        <tr class="compact-row alt">
            <td colspan="8" class="text-center text-muted py-3">
                <i class='bx bx-info-circle bx-lg'></i>
                <p class="mb-0">No hay productos cargados</p>
            </td>
        </tr>
    `);
    
    $('#txtTotalFactura').val('$ 0.00');
    
    console.log('✅ Grilla limpiada');
}

// ====== FUNCIONES AUXILIARES ======
function mostrarMensajeError(mensaje) {
    console.error('💬 Error:', mensaje);
    
    AbrirMensaje(
        "Error",
        mensaje,
        function () {
            $("#msjModal").modal("hide");
        },
        false,
        ["Aceptar"],
        "error!",
        null
    );
}

function mostrarMensajeExito(mensaje) {
    console.log('💬 Éxito:', mensaje);
    
    AbrirMensaje(
        "Éxito",
        mensaje,
        function () {
            $("#msjModal").modal("hide");
        },
        false,
        ["Aceptar"],
        "ok!",
        null
    );
}