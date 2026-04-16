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

// ====== MOSTRAR SECCIÓN DE PRODUCTOS ====== (✅ CORREGIDO v3.2)
/**
 * ✅ CORREGIDO v3.2: Badge de tipo de comprobante corregido
 * 
 * CAMBIOS v3.2:
 * - Eliminada línea incorrecta que usaba .val() en <span>
 * - actualizarTipoComprobante() ahora maneja correctamente el badge
 * - Usa clienteData.emite si está disponible, sino calcula desde condicionAfip
 */
function mostrarSeccionProductos(clienteData) {
    console.log('═══════════════════════════════════════════════════');
    console.log('📦 MOSTRAR MODAL DE PRODUCTOS v3.2');
    console.log('═══════════════════════════════════════════════════');
    console.log('Cliente recibido:', clienteData);
    
    // ❶ Hidratar datos del cliente en el modal
    $('#txtClienteNombreProd').val(clienteData.nombre || '');
    $('#txtClienteIdProd').val(clienteData.id || 'N/A');
    $('#txtClienteDomicilioProd').val(clienteData.domicilio || '');
    $('#txtCondicionAfipProd').val(clienteData.condicionAfip || '');
    $('#txtClienteCuitProd').val(clienteData.tipoNumero || '');
    $('#txtClienteEmailProd').val(clienteData.email || '');
    $('#txtClienteMovilProd').val(clienteData.movil || '');
    
    console.log('✅ Datos hidratados correctamente:');
    console.log('   - Nombre:', clienteData.nombre);
    console.log('   - ID:', clienteData.id || 'N/A');
    console.log('   - Condición AFIP:', clienteData.condicionAfip);
    console.log('   - Tipo/Número:', clienteData.tipoNumero);
    console.log('   - Emite:', clienteData.emite);
    
    // ❷ Actualizar badge de tipo de comprobante
    // ✅ CORREGIDO: Ahora maneja clienteData.emite correctamente
    actualizarTipoComprobante(clienteData);
    
    // ❸ Abrir modal
    $('#modalProductosFactura').modal('show');
    console.log('✅ Modal de productos abierto');
    
    // ❹ Focus en campo de búsqueda (después de que el modal se muestre)
    $('#modalProductosFactura').on('shown.bs.modal', function () {
        setTimeout(() => {
            $('#txtCodigoProducto').trigger('focus');
        }, 200);
    });
    
    console.log('═══════════════════════════════════════════════════');
    console.log('✅ MODAL DE PRODUCTOS MOSTRADO CORRECTAMENTE v3.2');
    console.log('═══════════════════════════════════════════════════');
}

// ====== ACTUALIZAR TIPO DE COMPROBANTE ====== (✅ CORREGIDO v2.0)
/**
 * ✅ CORREGIDO v2.0: Ahora usa clienteData.emite como prioridad
 * 
 * CAMBIOS v2.0:
 * - Primero intenta usar clienteData.emite (dato directo del servidor)
 * - Si no existe, calcula desde clienteData.condicionAfip
 * - Mejora precisión del tipo de comprobante
 */
function actualizarTipoComprobante(clienteData) {
    const $badge = $('#badgeTipoComprobante');
    
    let tipoFactura = 'FACTURA B'; // Por defecto
    let iconoFactura = 'bx-file';
    
    // ❶ PRIORIDAD 1: Usar clienteData.emite si existe
    if (clienteData.emite && clienteData.emite.trim() !== '') {
        tipoFactura = clienteData.emite.toUpperCase();
        
        console.log(`✅ Tipo de comprobante obtenido desde clienteData.emite: "${tipoFactura}"`);
        
        // Determinar icono según el tipo
        if (tipoFactura.includes('FACTURA A')) {
            iconoFactura = 'bx-file-blank';
        } else if (tipoFactura.includes('FACTURA C')) {
            iconoFactura = 'bx-file';
        } else if (tipoFactura.includes('FACTURA B')) {
            iconoFactura = 'bx-file';
        } else if (tipoFactura.includes('NOTA')) {
            iconoFactura = 'bx-receipt';
        } else {
            iconoFactura = 'bx-file'; // Default
        }
        
    } 
    // ❷ PRIORIDAD 2: Calcular desde clienteData.condicionAfip
    else if (clienteData.condicionAfip) {
        const condicion = clienteData.condicionAfip.toUpperCase();
        
        console.log(`ℹ️ clienteData.emite no disponible, calculando desde condicionAfip: "${condicion}"`);
        
        if (condicion.includes('INSCRIPTO') || condicion.includes('MONOTRIBUTO')) {
            tipoFactura = 'FACTURA A';
            iconoFactura = 'bx-file-blank';
        } else if (condicion.includes('EXENTO')) {
            tipoFactura = 'FACTURA C';
            iconoFactura = 'bx-file';
        } else {
            tipoFactura = 'FACTURA B';
            iconoFactura = 'bx-file';
        }
    }
    // ❸ FALLBACK: Consumidor Final por defecto
    else {
        console.warn('⚠️ No se pudo determinar tipo de comprobante, usando FACTURA B por defecto');
        tipoFactura = 'FACTURA B';
        iconoFactura = 'bx-file';
    }
    
    // ❹ Actualizar el badge (✅ Usar .html() para <span>)
    $badge.html(`<i class='bx ${iconoFactura}'></i> ${tipoFactura}`);
    
    console.log('═══════════════════════════════════════════════════');
    console.log('📋 TIPO DE COMPROBANTE ACTUALIZADO');
    console.log(`   Texto: "${tipoFactura}"`);
    console.log(`   Icono: "${iconoFactura}"`);
    console.log('═══════════════════════════════════════════════════');
}

// ====== OCULTAR SECCIÓN DE PRODUCTOS ====== (✅ CORREGIDO v3.1)
/**
 * ✅ CORREGIDO v3.1: Actualizado para usar IDs únicos con sufijo "Prod"
 */
function ocultarSeccionProductos() {
    console.log('═══════════════════════════════════════════════════');
    console.log('🔙 OCULTAR MODAL DE PRODUCTOS v3.1');
    console.log('═══════════════════════════════════════════════════');
    
    // ❶ Limpiar campos de cliente (✅ CORREGIDO: IDs con sufijo Prod)
    $('#txtClienteNombreProd').val('');
    $('#txtClienteIdProd').val('');
    $('#txtClienteDomicilioProd').val('');
    $('#txtCondicionAfipProd').val('');
    $('#txtClienteCuitProd').val('');
    $('#txtClienteEmailProd').val('');
    $('#txtClienteMovilProd').val('');
    
    // ❷ Limpiar campo de búsqueda
    $('#txtCodigoProducto').val('');
    
    // ❸ Limpiar grilla de productos
    limpiarGrillaProductos();
    
    // ❹ Limpiar cliente actual
    clienteActualFactura = null;
    
    // ❺ Cerrar modal
    $('#modalProductosFactura').modal('hide');
    
    console.log('✅ Modal de productos ocultado correctamente');
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