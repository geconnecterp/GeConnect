// ============================================
// GESTOR DE PRODUCTOS DE FACTURACIÓN
// ============================================
// VERSIÓN COMPLETA v5.0 CORREGIDA
// Integración mediante eventos personalizados
// ✅ NUEVO: Soporte para selección de múltiples productos
// ✅ CORREGIDO: Validación correcta de respuesta del servidor
// ============================================

// ====== VARIABLES GLOBALES ======
let productosFactura = [];
let totalFactura = 0;
let clienteActualFactura = null;
let modoBloqueoGrilla = null; // 'cotizacion' cuando se carga una cotización
let origenCargaActual = 'directo'; // ✅ NUEVO: Guardar origen de carga actual
let ultimoCambioProducto = null; // ✅ NUEVO: Permite reflejar visualmente altas/fusiones
// ✅ NUEVO v12.0: Control de acumulación según tipo de controlador fiscal
let cajaAcumulaProductos = true; // Default: TRUE (acumula por defecto)
// ====== CONSTANTES ======
const TIPO_CARGA = {
    PRODUCTO: 'P',
    PREFACTURA: 'F',
    COTIZACION: 'C'
};

const REGEX_CANTIDAD_COMODIN = /^(\d+)\+(.+)$/; // Ej: 5+7790070036599
const REGEX_BARRAS_BALANZA = /^2(\d{5})(\d{5})(\d)$/; // Formato balanza: 2 + 5 dígitos código + 5 dígitos peso + dígito verificador

// ====== INICIALIZACIÓN ======
$(function () {
    console.log('🚀 Módulo de Productos de Factura inicializado v5.0 CORREGIDA');
    inicializarEventosProductos();
    configurarListenersIntegracion();
    obtenerConfiguracionCaja(); // ✅ NUEVO: Obtener configuración al inicio
});

/**
 * ✅ NUEVO v12.0: Obtiene la configuración de la caja actual
 * Incluye el flag de acumulación de productos
 */
function obtenerConfiguracionCaja() {
    console.log('═══════════════════════════════════════════════════');
    console.log('⚙️ OBTENIENDO CONFIGURACIÓN DE CAJA v12.0');
    console.log('═══════════════════════════════════════════════════');

    const url = typeof ObtenerConfiguracionCajaUrl !== 'undefined' && ObtenerConfiguracionCajaUrl
        ? ObtenerConfiguracionCajaUrl
        : '/Facturacion/ProductoFact/ObtenerConfiguracionCaja';

    $.ajax({
        url: url,
        type: 'GET',
        success: function (response) {
            if (response.ok && response.configuracion) {
                cajaAcumulaProductos = response.configuracion.acumula ?? true;

                console.log('✅ CONFIGURACIÓN DE CAJA OBTENIDA:');
                console.log(`   Caja ID: ${response.configuracion.caja_id}`);
                console.log(`   Acumula Productos: ${cajaAcumulaProductos ? 'SÍ ✅' : 'NO ❌'}`);
                console.log(`   Modo: ${cajaAcumulaProductos ? 'Impresión al final' : 'Impresión en tiempo real'}`);
                console.log('═══════════════════════════════════════════════════');
            } else {
                console.warn('⚠️ No se pudo obtener configuración, usando default (acumula=true)');
            }
        },
        error: function (xhr, status, error) {
            console.error('❌ ERROR AL OBTENER CONFIGURACIÓN DE CAJA');
            console.error(`   Status: ${xhr.status}`);
            console.error(`   Error: ${error}`);
            console.warn('⚠️ Usando configuración por defecto (acumula=true)');
        }
    });
}

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
            procesarEntradaCodigo();
        }
    });
    
    // Buscar producto (botón)
    $('#btnBuscarProducto').on('click', function () {
        procesarEntradaCodigo();
    });

    // Buscar otros productos desde la base (botón)
    $('#btnBuscarProductos').on('click', function () {
        BuscarProductos();
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
    
    // ✅ Botones de acción especial
    $('#btnPreFactura').on('click', function() {
        console.log('📄 Cargar Pre-Factura...');
        abrirModalPreFacturas(); // ← Llamar función de factPreFactura.js
    });
    
    $('#btnFacturaEmitida').on('click', function() {
        console.log('🧾 Cargar Factura Emitida...');
        cargarFacturaEmitida();
    });
    
    $('#btnCotizacion').on('click', function() {
        console.log('💰 Cargar Cotización...');
        abrirModalCotizaciones(); // ← Llamar función de factCotizacion.js
    });
    
    $('#btnUltimoDetalle').on('click', function() {
        console.log('🕒 Cargar Último Detalle...');
        cargarUltimoDetalle();
    });   

    console.log('✅ Eventos configurados correctamente');
}

// ═══════════════════════════════════════════════════════════════════
// SECCIÓN 1: PROCESAMIENTO DE ENTRADA
// ═══════════════════════════════════════════════════════════════════

/**
 * Nuevo v1.0: Busca productos desde el modal.
 * 
 */
function BuscarProductos() {
    console.log('═══════════════════════════════════════════════════');
    console.log('🔍 ABRIR MODAL DE BÚSQUEDA DE PRODUCTOS v5.1');
    console.log('═══════════════════════════════════════════════════');

    // ❶ Validar que exista el modal
    const $modalBusqueda = $('#busquedaModal');

    if ($modalBusqueda.length === 0) {
        console.error('❌ Modal #busquedaModal no encontrado');
        mostrarMensajeError('Error: Modal de búsqueda no disponible');
        return;
    }

    // ❷ Limpiar campos de búsqueda previos (si existen)
    $modalBusqueda.find('input[type="text"], input[type="search"]').val('');

    // ❸ Abrir modal
    $modalBusqueda.modal('show');

    console.log('✅ Modal de búsqueda abierto');

    // ❹ Focus en campo de búsqueda al mostrarse
    $modalBusqueda.on('shown.bs.modal', function () {
        const $campoBusqueda = $modalBusqueda.find('input[type="text"], input[type="search"]').first();

        if ($campoBusqueda.length > 0) {
            setTimeout(() => {
                $campoBusqueda.trigger('focus');
            }, 200);
        }
    });

    console.log('═══════════════════════════════════════════════════');
}

/**
 * ✅ NUEVO v4.0: Procesa la entrada del campo de código
 * Detecta el tipo de entrada y ejecuta el flujo correspondiente
 */
function procesarEntradaCodigo() {
    const entrada = $('#txtCodigoProducto').val().trim();
    
    if (!entrada) {
        mostrarMensajeError('Por favor, ingrese un código de producto');
        return;
    }
    
    console.log('═══════════════════════════════════════════════════');
    console.log('🔍 PROCESANDO ENTRADA DE CÓDIGO');
    console.log(`   Entrada: "${entrada}"`);
    console.log('═══════════════════════════════════════════════════');
    
    // ❶ Detectar cantidad comodín (Ej: 5+7790070036599)
    const matchCantidad = entrada.match(REGEX_CANTIDAD_COMODIN);
    
    if (matchCantidad) {
        const cantidad = parseInt(matchCantidad[1], 10);
        const codigo = matchCantidad[2];
        
        console.log('✅ Detectado: CANTIDAD COMODÍN');
        console.log(`   Cantidad: ${cantidad}`);
        console.log(`   Código: ${codigo}`);
        
        procesarCodigoConCantidad(codigo, cantidad);
        return;
    }
    
    // ❷ Si no hay comodín, procesar como código simple
    console.log('✅ Detectado: CÓDIGO SIMPLE');
    procesarCodigoSimple(entrada);
}

/**
 * ✅ ACTUALIZADO v5.1: Procesa código simple (sin cantidad comodín)
 * NUEVO: Respeta el checkbox "Por Bulto"
 * Detecta si es barras de balanza, barras normal o ID de producto
 */
function procesarCodigoSimple(codigo) {
    console.log('═══════════════════════════════════════════════════');
    console.log('📦 PROCESANDO CÓDIGO SIMPLE v5.1');
    console.log(`   Código: "${codigo}"`);
    console.log('═══════════════════════════════════════════════════');

    // ❶ Detectar barras de balanza (formato: 2 + 5 dígitos + 5 dígitos + 1 dígito)
    const matchBalanza = codigo.match(REGEX_BARRAS_BALANZA);

    if (matchBalanza) {
        const codigoProducto = matchBalanza[1]; // 5 dígitos del código
        const pesoStr = matchBalanza[2]; // 5 dígitos del peso
        const peso = parseInt(pesoStr, 10) / 1000; // Convertir a kg

        console.log('✅ Detectado: BARRAS DE BALANZA');
        console.log(`   Código producto: ${codigoProducto}`);
        console.log(`   Peso (kg): ${peso}`);
        console.log('   ⚠️  Para balanza, el checkbox "Por Bulto" se IGNORA (siempre bulto=true)');

        // ⚠️ IMPORTANTE: Balanza siempre usa bulto=true (el peso es la cantidad exacta)
        buscarProductoPorCodigo(TIPO_CARGA.PRODUCTO, codigo, peso, true, 'directo');
        return;
    }

    // ❷ Si no es balanza, buscar como producto normal (cantidad = 1)
    console.log('✅ Detectado: BARRAS NORMAL o ID PRODUCTO');
    console.log('   → Enviando cantidad=1, bulto=true al servidor');
    console.log('   → El SP decide si multiplica por unidad_pres o no');

    // ⬇️ SIEMPRE bulto=true (el SP decide)
    buscarProductoPorCodigo(
        TIPO_CARGA.PRODUCTO,  // "P"
        codigo,                // EAN o ID
        1,                     // cantidad = 1
        true,                  // ✅ SIEMPRE true (el SP decide)
        'directo'              // origen = 'directo'
    );


    console.log('═══════════════════════════════════════════════════');
}

/**
 * ✅ ACTUALIZADO v5.1: Procesa código con cantidad comodín
 * NUEVO: Respeta el checkbox "Por Bulto"
 */
function procesarCodigoConCantidad(codigo, cantidad) {
    console.log('═══════════════════════════════════════════════════');
    console.log('📦 PROCESANDO CÓDIGO CON CANTIDAD COMODÍN v5.1');
    console.log(`   Código: "${codigo}"`);
    console.log(`   Cantidad ingresada: ${cantidad}`);
    console.log('═══════════════════════════════════════════════════');

    if (cantidad <= 0) {
        mostrarMensajeError('La cantidad debe ser mayor a cero');
        return;
    }

    console.log(`   → Enviando cantidad=${cantidad}, bulto=true al servidor`);
    console.log('   → El SP decide si multiplica por unidad_pres o no');

    // ⬇️ SIEMPRE bulto=true (el SP decide)
    buscarProductoPorCodigo(TIPO_CARGA.PRODUCTO, codigo, cantidad, true, 'directo');

    console.log('═══════════════════════════════════════════════════');
}

// ═══════════════════════════════════════════════════════════════════
// SECCIÓN 2: BÚSQUEDA Y VALIDACIÓN DE PRODUCTOS
// ═══════════════════════════════════════════════════════════════════

/**
 * ✅ ACTUALIZADO v5.1: Busca producto por código mediante AJAX
 * LOGS MEJORADOS: Muestra claramente el estado del parámetro "bulto"
 */
function buscarProductoPorCodigo(tipoValor, valor, cantidad = 1, bulto = true, origenCarga = 'directo') {
    console.log('═══════════════════════════════════════════════════');
    console.log('🔍 BUSCAR PRODUCTO POR CÓDIGO - v5.1');
    console.log('═══════════════════════════════════════════════════');
    console.log(`   Tipo Valor: ${tipoValor}`);
    console.log(`   Valor: ${valor}`);
    console.log(`   Cantidad: ${cantidad}`);
    console.log(`   Bulto: ${bulto} ${bulto ? '✅ (Por bulto)' : '❌ (Por unidad)'}`);  // ✅ LOG MEJORADO
    console.log(`   Origen Carga: ${origenCarga}`);
    console.log(`   🔧 Modo Actual: ${cajaAcumulaProductos ? 'ACUMULA ✅' : 'NO ACUMULA ❌'}`);
    console.log('═══════════════════════════════════════════════════');
    
    // ❶ Validar modo de bloqueo (cotización)
    if (modoBloqueoGrilla === 'cotizacion' && origenCarga === 'directo') {
        console.warn('⚠️ Grilla bloqueada por cotización');
        mostrarMensajeError(
            'No se pueden agregar productos individuales.\n\n' +
            'Ya se cargó una COTIZACIÓN. Debe cancelar la factura para cargar otros productos.'
        );
        return;
    }
    
    // ❷ Guardar origen de carga actual
    origenCargaActual = origenCarga;
    
    // ❸ Deshabilitar campo y botón
    const $txtCodigo = $('#txtCodigoProducto');
    const $btnBuscar = $('#btnBuscarProducto');
    
    $txtCodigo.prop('disabled', true);
    $btnBuscar.prop('disabled', true).html('<i class="bx bx-loader-alt bx-spin"></i> '); //Buscando...
    
    // ❹ CORREGIDO: Actualizar mensaje de estado a "Buscando..."
    $('#mensajeEstadoProducto')
        .removeClass('text-danger text-success text-muted')  // ✅ Incluir text-muted
        .addClass('text-info')
        .html(`<i class='bx bx-loader-alt bx-spin'></i> Buscando producto...`);
    
    // ❺ Construir URL
    const url = typeof ObtenerProductoDatosUrl !== 'undefined' && ObtenerProductoDatosUrl
        ? ObtenerProductoDatosUrl
        : '/Facturacion/ProductoFact/ObtenerProductoDatos';
    
    console.log(`📡 URL: ${url}`);
    
    // ❻ Realizar AJAX
    $.ajax({
        url: url,
        type: 'POST',
        data: {
            tipoValor: tipoValor,  // "P"
            valor: valor,          // "0001-0001" o "7790070036599"
            cantidad: cantidad,    // 1 o cantidad del comodín (ej: 7)
            bulto: bulto           // ✅ true o false según checkbox
        },
        success: function(response) {
            console.log('═══════════════════════════════════════════════════');
            console.log('✅ RESPUESTA RECIBIDA DEL SERVIDOR');
            console.log('═══════════════════════════════════════════════════');
            console.log('   Response completo:', response);
            
            // ✅ NUEVO: Mostrar cálculo de cantidad en el log
            if (response.ok && response.producto) {
                const prod = Array.isArray(response.producto) ? response.producto[0] : response.producto;
                console.log(`   📊 Cantidad calculada por SP: ${prod.cantidad_tot || 'N/A'}`);
                console.log(`   🔧 El SP ya aplicó la lógica de unidad_pres si corresponde`);
                console.log(`   💰 Precio total: $ ${formatearNumero((prod.p_pvta || 0) * (prod.cantidad_tot || 1), 2)}`);
            }

            // ✅ NUEVO v12.0: Actualizar flag de acumulación desde respuesta
            if (response.hasOwnProperty('acumula')) {
                const acumulaAnterior = cajaAcumulaProductos;
                cajaAcumulaProductos = response.acumula;

                if (acumulaAnterior !== cajaAcumulaProductos) {
                    console.log('═══════════════════════════════════════════════════');
                    console.log('🔄 CAMBIO DE MODO DE ACUMULACIÓN DETECTADO');
                    console.log(`   Anterior: ${acumulaAnterior ? 'ACUMULA ✅' : 'NO ACUMULA ❌'}`);
                    console.log(`   Nuevo: ${cajaAcumulaProductos ? 'ACUMULA ✅' : 'NO ACUMULA ❌'}`);
                    console.log('═══════════════════════════════════════════════════');
                }
            }
            
            if (response.ok) {
                procesarRespuestaProducto(response, origenCarga);
            } else {
                console.error('❌ Error en respuesta:', response.mensaje);
                
                // ✅ CORREGIDO: Mostrar error
                $('#mensajeEstadoProducto')
                    .removeClass('text-info text-success text-muted')  // ✅ Incluir text-muted
                    .addClass('text-danger')
                    .html(`<i class='bx bx-error-circle'></i> ${response.mensaje}`);
                
                mostrarMensajeError(response.mensaje);
            }
        },
        error: function(xhr, status, error) {
            console.error('❌ ERROR AJAX AL BUSCAR PRODUCTO');
            ocultarLoaderCalculando();

            // ✅ NUEVO: Usar función centralizada
            if (esSesionExpirada(xhr.status)) {
                manejarSesionExpirada();
                return;
            }

            let mensaje = 'Error al buscar el producto. Por favor, intente nuevamente.';
            if (xhr.status === 500) {
                mensaje = 'Error interno del servidor. Contacte al administrador.';
            }

            $('#mensajeEstadoProducto')
                .removeClass('text-info text-success text-muted')
                .addClass('text-danger')
                .html(`<i class='bx bx-error-circle'></i> Error de comunicación`);

            mostrarMensajeError(mensaje);
        },
        complete: function() {
            // Rehabilitar campo y botón
            $txtCodigo.prop('disabled', false).val('');
            $btnBuscar.prop('disabled', false).html('<i class="bx bx-search"></i>');
            
            // Focus en el campo
            $txtCodigo.trigger('focus');
        }
    });
}

/**
 * ✅ ACTUALIZADO v5.1 CORREGIDA: Procesa la respuesta del servidor
 * NUEVO: Soporte correcto para cotizaciones (múltiples productos)
 */
function procesarRespuestaProducto(response, origenCarga) {
    console.log('═══════════════════════════════════════════════════');
    console.log('📊 PROCESANDO RESPUESTA DE PRODUCTO v5.1 CORREGIDA');
    console.log('═══════════════════════════════════════════════════');
    console.log('   Origen de carga:', origenCarga);
    console.log('   Response:', response);
    
    const producto = response.producto;
    
    // ❶ VALIDAR QUE EXISTA PRODUCTO
    if (!producto) {
        console.warn('⚠️ No se recibió producto en la respuesta');
        
        $('#mensajeEstadoProducto')
            .removeClass('text-info text-success text-muted')
            .addClass('text-danger')
            .html(`<i class='bx bx-error-circle'></i> Producto no encontrado`);
        
        mostrarMensajeError('El producto no existe o no está disponible');
        return;
    }
    
    // ❷ CASO: Array vacío
    if (Array.isArray(producto) && producto.length === 0) {
        console.warn('⚠️ Array de productos vacío');
        
        $('#mensajeEstadoProducto')
            .removeClass('text-info text-success text-muted')
            .addClass('text-danger')
            .html(`<i class='bx bx-error-circle'></i> Producto no encontrado`);
        
        mostrarMensajeError('El producto no existe o no está disponible');
        return;
    }
    
    // ═══════════════════════════════════════════════════════════════════
    // ✅ NUEVO: CASO ESPECIAL PARA COTIZACIONES
    // ═══════════════════════════════════════════════════════════════════
    if (origenCarga === 'cotizacion') {
        console.log('═══════════════════════════════════════════════════');
        console.log('💰 PROCESAMIENTO ESPECIAL: COTIZACIÓN');
        console.log('═══════════════════════════════════════════════════');
        
        // ❶ Validar que sea un array
        if (!Array.isArray(producto)) {
            console.error('❌ Cotización no retornó un array de productos');
            mostrarMensajeError('Error en el formato de la cotización');
            return;
        }
        
        console.log(`   Total productos en cotización: ${producto.length}`);
        
        // ❷ Validar que no haya productos con error
        let productosValidos = 0;
        let productosInvalidos = 0;
        
        producto.forEach((prod, index) => {
            if (prod.respuesta === 0) {
                productosValidos++;
            } else {
                productosInvalidos++;
                console.warn(`⚠️ Producto ${index + 1} con error: ${prod.respuesta_msj}`);
            }
        });
        
        console.log(`   - Productos válidos: ${productosValidos}`);
        console.log(`   - Productos inválidos: ${productosInvalidos}`);
        
        if (productosValidos === 0) {
            console.error('❌ Ningún producto válido en la cotización');
            mostrarMensajeError('La cotización no contiene productos válidos');
            return;
        }
        
        // ❸ ESTABLECER MODO DE BLOQUEO
        modoBloqueoGrilla = 'cotizacion';
        console.log('⚠️ MODO BLOQUEO ACTIVADO: No se podrán agregar productos individuales');
        
        // ❹ PROCESAR CADA PRODUCTO VÁLIDO
        let productosAgregados = 0;
        
        producto.forEach((prod, index) => {
            if (prod.respuesta === 0) {
                console.log(`   [${index + 1}/${producto.length}] Agregando: ${prod.p_desc}`);
                agregarProductoAGrilla(prod);
                productosAgregados++;
            }
        });
        
        // ❺ MENSAJE DE ÉXITO
        console.log('═══════════════════════════════════════════════════');
        console.log(`✅ COTIZACIÓN CARGADA: ${productosAgregados} productos`);
        console.log('═══════════════════════════════════════════════════');
        
        $('#mensajeEstadoProducto')
            .removeClass('text-info text-danger text-muted')
            .addClass('text-success')
            .html(`<i class='bx bx-check-circle'></i> Cotización cargada: ${productosAgregados} productos`);
        
        // Restaurar después de 5 segundos
        setTimeout(() => {
            $('#mensajeEstadoProducto')
                .removeClass('text-success text-danger text-info')
                .addClass('text-muted')
                .html('Presione <kbd>Enter</kbd> o <strong>BUSCAR</strong> para agregar producto');
        }, 5000);
        
        return; // ← SALIR (ya procesamos la cotización)
    }
    
    // ═══════════════════════════════════════════════════════════════════
    // ❸ CASO: Producto único (esUnico = true o no es array)
    // ═══════════════════════════════════════════════════════════════════
    if (response.esUnico === true || !Array.isArray(producto)) {
        console.log('═══════════════════════════════════════════════════');
        console.log('✅ PRODUCTO ÚNICO DETECTADO');
        console.log('═══════════════════════════════════════════════════');
        
        // Si es array con un solo elemento, extraerlo
        const productoUnico = Array.isArray(producto) ? producto[0] : producto;
        
        validarYAgregarProducto(productoUnico, origenCarga);
        return;
    }
    
    // ❹ CASO: Múltiples productos (esMultiple = true o es array con más de 1 elemento)
    if (response.esMultiple === true || (Array.isArray(producto) && producto.length > 1)) {
        console.log('═══════════════════════════════════════════════════');
        console.log(`✅ MÚLTIPLES PRODUCTOS DETECTADOS: ${producto.length}`);
        console.log('═══════════════════════════════════════════════════');
        console.log(`   - Total productos: ${response.totalProductos || producto.length}`);
        console.log(`   - Productos válidos: ${response.productosValidos || 'N/A'}`);
        console.log(`   - Productos inválidos: ${response.productosInvalidos || 'N/A'}`);
        
        // Mostrar modal de selección
        mostrarModalSeleccionProducto(producto, origenCarga);
        return;
    }
    
    // ❺ FALLBACK: Si no se detectó ningún caso anterior
    console.warn('⚠️ Caso no contemplado, tratando como producto único');
    const productoFallback = Array.isArray(producto) ? producto[0] : producto;
    validarYAgregarProducto(productoFallback, origenCarga);
}

// ═══════════════════════════════════════════════════════════════════
// SECCIÓN 2.5: MODAL DE SELECCIÓN DE PRODUCTOS (✅ NUEVO v5.0)
// ═══════════════════════════════════════════════════════════════════

/**
 * ✅ NUEVO v5.0: Muestra modal con grilla de selección de productos
 * 
 * @param {Array} productos - Array de productos encontrados
 * @param {string} origenCarga - Origen de la carga
 */
function mostrarModalSeleccionProducto(productos, origenCarga) {
    console.log('═══════════════════════════════════════════════════');
    console.log('📋 MOSTRAR MODAL DE SELECCIÓN DE PRODUCTO v5.0');
    console.log(`   Total productos: ${productos.length}`);
    console.log(`   Origen: ${origenCarga}`);
    console.log('═══════════════════════════════════════════════════');
    
    const $tbody = $('#tbodySeleccionProducto');
    let html = '';
    let productosValidosCount = 0;
    
    productos.forEach((producto, index) => {
        const respuesta = producto.respuesta || 0;
        const esValido = respuesta === 0;
        
        if (esValido) {
            productosValidosCount++;
        }
        
        // Datos del producto
        const codigo = producto.p_id || '???';
        const barras = producto.p_id_barrado || '';
        const descripcion = producto.p_desc || 'Sin descripción';
        const unidadPres = producto.p_unidad_pres || 1;
        const precio = producto.p_pvta || 0;
        const cantidad = producto.cantidad_tot || 1;
        const mensajeError = producto.respuesta_msj || '';
        
        console.log(`   [${index}] ${descripcion}`);
        console.log(`      - Respuesta: ${respuesta} ${esValido ? '✅ VÁLIDO' : '❌ INVÁLIDO'}`);
        console.log(`      - Mensaje: ${mensajeError}`);
        
        // ❶ Fila con clase según validez
        const rowClass = esValido ? '' : 'table-danger';
        const textClass = esValido ? '' : 'text-muted';
        
        html += `
            <tr class="${rowClass}" data-index="${index}">
                <td class="${textClass} fw-bold">${escapeHtml(codigo)}</td>
                <td class="${textClass}">${escapeHtml(barras)}</td>
                <td class="${textClass}">
                    <div class="fw-semibold">${escapeHtml(descripcion)}</div>
                    ${!esValido ? `<small class="text-danger fw-bold"><i class='bx bx-error-circle'></i> ${escapeHtml(mensajeError)}</small>` : ''}
                </td>
                <td class="text-center ${textClass} fw-semibold">
                    ${unidadPres}
                </td>
                <td class="text-end ${textClass} fw-semibold">$ ${formatearNumero(precio, 2)}</td>
                <td class="text-center">
                    ${esValido 
                        ? '<span class="badge bg-success"><i class="bx bx-check"></i> Disponible</span>'
                        : '<span class="badge bg-danger"><i class="bx bx-x"></i> No disponible</span>'
                    }
                </td>
                <td class="text-center">
                    ${esValido 
                        ? `<button class="btn btn-success btn-sm" 
                                   type="button" 
                                   onclick="seleccionarProductoDeModal(${index})"
                                   title="Seleccionar este producto">
                               <i class='bx bx-check-circle'></i> Seleccionar
                           </button>`
                        : `<button class="btn btn-secondary btn-sm" 
                                   type="button" 
                                   disabled
                                   title="${escapeHtml(mensajeError)}">
                               <i class='bx bx-block'></i> No disponible
                           </button>`
                    }
                </td>
            </tr>
        `;
    });
    
    $tbody.html(html);
    
    console.log('═══════════════════════════════════════════════════');
    console.log(`📊 GRILLA GENERADA:`);
    console.log(`   - Total productos: ${productos.length}`);
    console.log(`   - Productos válidos (seleccionables): ${productosValidosCount}`);
    console.log(`   - Productos inválidos: ${productos.length - productosValidosCount}`);
    console.log('═══════════════════════════════════════════════════');
    
    // ❷ Guardar productos en memoria temporal del modal
    $('#modalSeleccionProducto').data('productos', productos);
    $('#modalSeleccionProducto').data('origenCarga', origenCarga);
    
    // ❸ Mostrar modal
    $('#modalSeleccionProducto').modal('show');
    
    console.log('✅ Modal de selección mostrado');
}

/**
 * ✅ NUEVO v5.0: Selecciona un producto del modal y lo agrega a la grilla
 * 
 * @param {number} index - Índice del producto seleccionado
 */
function seleccionarProductoDeModal(index) {
    console.log('═══════════════════════════════════════════════════');
    console.log(`✅ PRODUCTO SELECCIONADO DEL MODAL - Índice: ${index}`);
    console.log('═══════════════════════════════════════════════════');
    
    // ❶ Obtener datos del modal
    const $modal = $('#modalSeleccionProducto');
    const productos = $modal.data('productos');
    const origenCarga = $modal.data('origenCarga') || 'directo';
    
    if (!productos || index < 0 || index >= productos.length) {
        console.error(`❌ Índice inválido o productos no encontrados: ${index}`);
        return;
    }
    
    const productoSeleccionado = productos[index];
    
    console.log('   Producto seleccionado:', productoSeleccionado);
    console.log(`   - Código: ${productoSeleccionado.p_id}`);
    console.log(`   - Descripción: ${productoSeleccionado.p_desc}`);
    console.log(`   - Respuesta: ${productoSeleccionado.respuesta}`);
    
    // ❷ Cerrar modal
    $modal.modal('hide');
    
    // ❸ Agregar producto a la grilla principal
    validarYAgregarProducto(productoSeleccionado, origenCarga);
}

/**
 * ✅ ACTUALIZADO v5.1: Valida y agrega un producto único
 * CORREGIDO: Manejo correcto de clases de color
 */
function validarYAgregarProducto(producto, origenCarga) {
    const respuesta = producto.respuesta || 0;
    const descripcion = producto.p_desc || 'Sin descripción';

    console.log('═══════════════════════════════════════════════════');
    console.log('🔍 VALIDANDO PRODUCTO ÚNICO');
    console.log('═══════════════════════════════════════════════════');
    console.log(`   Descripción: ${descripcion}`);
    console.log(`   Respuesta: ${respuesta}`);
    console.log(`   Origen carga: ${origenCarga}`);

    if (respuesta !== 0) {
        const mensaje = producto.respuesta_msj || 'El producto no se puede cargar';

        console.error(`❌ Producto con error: ${mensaje}`);

        if (origenCarga === 'ultimo') {
            console.log('ℹ️ Error ignorado (último detalle)');
            return;
        }

        $('#mensajeEstadoProducto')
            .removeClass('text-info text-success text-muted')
            .addClass('text-danger')
            .html(`<i class='bx bx-error-circle'></i> ${mensaje}`);

        mostrarMensajeError(mensaje);
        return;
    }

    console.log('✅ Producto válido, agregando a grilla...');
    const resultado = agregarProductoAGrilla(producto);

    let mensajeExito = 'Producto agregado';

    if (resultado?.accion === 'fusionado' && resultado.producto) {
        mensajeExito = `Cantidad actualizada: ${formatearNumero(normalizarNumero(resultado.producto.cantidadTotal, 0), 2)}`;
    }

    $('#mensajeEstadoProducto')
        .removeClass('text-info text-danger text-muted')
        .addClass('text-success')
        .html(`<i class='bx bx-check-circle'></i> ${mensajeExito}`);

    setTimeout(() => {
        $('#mensajeEstadoProducto')
            .removeClass('text-success text-danger text-info')
            .addClass('text-muted')
            .html('Presione <kbd>Enter</kbd> o <strong>BUSCAR</strong> para agregar producto');
    }, 3000);
}
//function validarYAgregarProducto(producto, origenCarga) {
//    const respuesta = producto.respuesta || 0;
//    const descripcion = producto.p_desc || 'Sin descripción';

//    console.log('═══════════════════════════════════════════════════');
//    console.log(`🔍 VALIDANDO PRODUCTO ÚNICO`);
//    console.log('═══════════════════════════════════════════════════');
//    console.log(`   Descripción: ${descripcion}`);
//    console.log(`   Respuesta: ${respuesta}`);
//    console.log(`   Origen carga: ${origenCarga}`);

//    // ❶ Producto con error (respuesta != 0)
//    if (respuesta !== 0) {
//        const mensaje = producto.respuesta_msj || 'El producto no se puede cargar';

//        console.error(`❌ Producto con error: ${mensaje}`);

//        // Para último detalle, ignorar silenciosamente
//        if (origenCarga === 'ultimo') {
//            console.log('ℹ️ Error ignorado (último detalle)');
//            return;
//        }

//        // ✅ CORREGIDO: Eliminar TODAS las clases de color
//        $('#mensajeEstadoProducto')
//            .removeClass('text-info text-success text-muted')  // ✅ Incluir text-muted
//            .addClass('text-danger')
//            .html(`<i class='bx bx-error-circle'></i> ${mensaje}`);

//        mostrarMensajeError(mensaje);
//        return;
//    }

//    // ②  Producto válido
//    console.log('✅ Producto válido, agregando a grilla...');
//    agregarProductoAGrilla(producto);

//    // ✅ CORREGIDO: Mensaje de éxito
//    $('#mensajeEstadoProducto')
//        .removeClass('text-info text-danger text-muted')  // ✅ Incluir text-muted
//        .addClass('text-success')
//        .html(`<i class='bx bx-check-circle'></i> Producto agregado`);

//    // ✅ CORREGIDO: Restaurar estado inicial después de 3 segundos
//    setTimeout(() => {
//        $('#mensajeEstadoProducto')
//            .removeClass('text-success text-danger text-info')  // ✅ Eliminar colores
//            .addClass('text-muted')  // ✅ Restaurar text-muted
//            .html('Presione <kbd>Enter</kbd> o <strong>BUSCAR</strong> para agregar producto');
//    }, 3000);
//}

// ═══════════════════════════════════════════════════════════════════
// SECCIÓN 3: GESTIÓN DE GRILLA PRINCIPAL
// ═══════════════════════════════════════════════════════════════════

function normalizarClaveProducto(p_id) {
    return String(p_id || '').trim().toUpperCase();
}

function normalizarNumero(valor, valorPorDefecto = 0) {
    if (valor === null || valor === undefined || valor === '') {
        return valorPorDefecto;
    }

    if (typeof valor === 'number') {
        return Number.isFinite(valor) ? valor : valorPorDefecto;
    }

    let texto = String(valor).trim();

    if (texto.includes(',') && texto.includes('.')) {
        texto = texto.replace(/\./g, '').replace(',', '.');
    } else {
        texto = texto.replace(',', '.');
    }

    const numero = Number(texto);

    return Number.isFinite(numero) ? numero : valorPorDefecto;
}

function registrarUltimoCambioProducto(accion, producto, indice) {
    ultimoCambioProducto = {
        accion: accion,
        indice: indice,
        item: producto?.item || 0,
        p_id: normalizarClaveProducto(producto?.p_id),
        timestamp: Date.now()
    };
}

/**
 * ✅ NUEVO v8.1: Busca un producto en la grilla por su código normalizado
 *
 * @param {string} p_id - Código del producto a buscar
 * @returns {number} Índice del producto en el array (-1 si no existe)
 */
function buscarProductoExistente(p_id) {
    const claveBuscada = normalizarClaveProducto(p_id);

    if (!claveBuscada) {
        console.warn('⚠️ p_id inválido para búsqueda:', p_id);
        return -1;
    }

    const indice = productosFactura.findIndex(p => normalizarClaveProducto(p.p_id) === claveBuscada);

    if (indice !== -1) {
        console.log(`🔍 Producto encontrado en índice ${indice} (item=${productosFactura[indice].item}, clave=${claveBuscada})`);
    } else {
        console.log(`🔍 Producto NO encontrado en la grilla (clave=${claveBuscada})`);
    }

    return indice;
}

/**
 * ✅ ACTUALIZADO v10.0: Incrementa cantidad CON RECALCULO REDONDEADO
 */
function incrementarCantidadProducto(indice, cantidadAIncrementar) {
    console.log('═══════════════════════════════════════════════════');
    console.log('🔄 INCREMENTANDO CANTIDAD DE PRODUCTO v10.0 (CON REDONDEO)');
    console.log('═══════════════════════════════════════════════════');

    if (indice < 0 || indice >= productosFactura.length) {
        console.error(`❌ Índice inválido: ${indice}`);
        return null;
    }

    const incremento = normalizarNumero(cantidadAIncrementar, 0);

    if (incremento <= 0) {
        console.error(`❌ Cantidad inválida: ${cantidadAIncrementar}`);
        return null;
    }

    const producto = productosFactura[indice];
    const cantidadAnterior = normalizarNumero(producto.cantidad_tot, 0);
    const precioUnitario = normalizarNumero(producto.p_pvta, 0);
    const cantidadNueva = cantidadAnterior + incremento;

    console.log(`   Item: ${producto.item}`);
    console.log(`   Código: ${producto.p_id}`);
    console.log(`   Cantidad anterior: ${cantidadAnterior}`);
    console.log(`   Incremento: +${incremento}`);
    console.log(`   Cantidad nueva: ${cantidadNueva}`);

    // ❶ Actualizar cantidad
    producto.cantidad_tot = cantidadNueva;
    producto.cantidadTotal = cantidadNueva;

    // ❷ ✅ NUEVO: Recalcular precio total CON REDONDEO
    const precioTotalNuevo = calcularPrecioTotal({
        p_pvta: precioUnitario,
        cantidad_tot: cantidadNueva
    });

    producto.p_pvta_tot = precioTotalNuevo;
    producto.precioTotal = precioTotalNuevo;

    // ❸ Sincronizar objeto original
    if (producto._original) {
        producto._original.cantidad_tot = cantidadNueva;
        producto._original.p_pvta_tot = precioTotalNuevo;
    }

    console.log(`   💰 Precio unitario: $ ${formatearNumero(precioUnitario, 2)}`);
    console.log(`   💰 Precio total REDONDEADO: $ ${formatearNumero(precioTotalNuevo, 2)}`);
    console.log('═══════════════════════════════════════════════════');

    return producto;
}

/**
 * ✅ ACTUALIZADO v12.0: Agrega producto CON LÓGICA DIFERENCIAL según "acumula"
 * NUEVO: Si acumula=FALSE, siempre crea nuevo registro (no busca duplicados)
 */
function agregarProductoAGrilla(producto) {
    console.log('═══════════════════════════════════════════════════');
    console.log('➕ AGREGANDO/ACTUALIZANDO PRODUCTO v12.0');
    console.log('═══════════════════════════════════════════════════');
    console.log('   Producto recibido:', producto);
    console.log(`   🔧 Modo Acumulación: ${cajaAcumulaProductos ? 'ACUMULA ✅' : 'NO ACUMULA ❌'}`);

    const claveProducto = normalizarClaveProducto(producto.p_id);
    const cantidadNueva = normalizarNumero(producto.cantidad_tot, 1);

    console.log(`   🔍 Código producto: ${claveProducto}`);
    console.log(`   📦 Cantidad recibida: ${cantidadNueva}`);

    // ═══════════════════════════════════════════════════════════════════
    // ✅ NUEVO v12.0: LÓGICA DIFERENCIAL SEGÚN "acumula"
    // ═══════════════════════════════════════════════════════════════════

    if (cajaAcumulaProductos) {
        console.log('═══════════════════════════════════════════════════');
        console.log('✅ MODO: ACUMULA PRODUCTOS (Impresión al final)');
        console.log('═══════════════════════════════════════════════════');

        // ❶ Buscar si el producto ya existe
        const indiceExistente = buscarProductoExistente(claveProducto);

        if (indiceExistente !== -1) {
            console.log('🔄 PRODUCTO YA EXISTE - Incrementando cantidad...');

            const productoActualizado = incrementarCantidadProducto(indiceExistente, cantidadNueva);

            if (!productoActualizado) {
                return {
                    accion: 'error',
                    producto: null,
                    indice: indiceExistente
                };
            }

            registrarUltimoCambioProducto('fusionado', productoActualizado, indiceExistente);

            recalcularTotalFactura();
            actualizarGrillaProductos();
            $('#cantidadItems').text(productosFactura.length);

            console.log('═══════════════════════════════════════════════════');
            console.log('✅ CANTIDAD INCREMENTADA EXITOSAMENTE');
            console.log(`   Total productos únicos: ${productosFactura.length}`);
            console.log('═══════════════════════════════════════════════════');

            return {
                accion: 'fusionado',
                producto: productoActualizado,
                indice: indiceExistente
            };
        }

        console.log('✨ PRODUCTO NUEVO - Agregando a la grilla...');
    } else {
        console.log('═══════════════════════════════════════════════════');
        console.log('⚠️ MODO: NO ACUMULA (Impresión en tiempo real)');
        console.log('✨ SIEMPRE AGREGA COMO NUEVO REGISTRO');
        console.log('═══════════════════════════════════════════════════');
    }

    // ═══════════════════════════════════════════════════════════════════
    // ❷ AGREGAR COMO PRODUCTO NUEVO (común para ambos modos)
    // ═══════════════════════════════════════════════════════════════════

    const siguienteItem = productosFactura.length > 0
        ? Math.max(...productosFactura.map(p => p.item || 0)) + 1
        : 1;

    console.log(`📊 Item correlativo calculado: ${siguienteItem}`);

    const cantidadNormalizada = normalizarNumero(producto.cantidad_tot, 1);
    const precioVentaNormalizado = normalizarNumero(producto.p_pvta, 0);
    const precioCostoNormalizado = normalizarNumero(producto.p_pcosto, 0);
    const precioCostoRepoNormalizado = normalizarNumero(producto.p_pcosto_repo, 0);
    const precioNetoNormalizado = normalizarNumero(producto.p_pneto, 0);
    const ivaAlicuotaNormalizada = normalizarNumero(producto.iva_alicuota, 0);
    const ivaImporteNormalizado = normalizarNumero(producto.p_iva, 0);
    const inAlicuotaNormalizada = normalizarNumero(producto.in_alicuota, 0);
    const inImporteNormalizado = normalizarNumero(producto.p_in, 0);
    const previsionTotNormalizada = normalizarNumero(producto.lp_prevision_tot, 0);
    const previsionPinNormalizada = normalizarNumero(producto.lp_prevision_pin, 0);
    const poLimiteNormalizado = normalizarNumero(producto.po_limite, 0);

    const productoNormalizado = {
        item: siguienteItem,
        p_id: producto.p_id || '???',
        p_id_barrado: producto.p_id_barrado || '',
        p_desc: producto.p_desc || 'Sin descripción',
        descripcion: producto.p_desc || 'Sin descripción',
        unidadPresentacion: normalizarNumero(producto.p_unidad_pres, 1),
        peso: normalizarNumero(producto.p_peso, 0),
        cantidad_tot: cantidadNormalizada,
        cantidadTotal: cantidadNormalizada,
        p_pvta: precioVentaNormalizado,
        precioVenta: precioVentaNormalizado,
        p_pcosto: precioCostoNormalizado,
        precioCosto: precioCostoNormalizado,
        p_pcosto_repo: precioCostoRepoNormalizado,
        p_pneto: precioNetoNormalizado,
        iva_situacion: producto.iva_situacion || '',
        iva_alicuota: ivaAlicuotaNormalizada,
        ivaAlicuota: ivaAlicuotaNormalizada,
        p_iva: ivaImporteNormalizado,
        ivaImporte: ivaImporteNormalizado,
        in_alicuota: inAlicuotaNormalizada,
        internAlicuota: inAlicuotaNormalizada,
        p_in: inImporteNormalizado,
        internImporte: inImporteNormalizado,
        lp_prevision_tot: previsionTotNormalizada,
        lp_prevision_pin: previsionPinNormalizada,
        po: producto.po || false,
        po_limite: poLimiteNormalizado,
        p_pvta_tot: calcularPrecioTotal({
            p_pvta: precioVentaNormalizado,
            cantidad_tot: cantidadNormalizada
        }),
        precioTotal: calcularPrecioTotal({
            p_pvta: precioVentaNormalizado,
            cantidad_tot: cantidadNormalizada
        }),
        rubro: producto.rub_desc || '',
        activo: producto.p_activo || 'S',
        cta_id: clienteActualFactura?.id || '',
        pre_id: producto.pre_id || null,
        preId: producto.pre_id || null,
        cpf_nro: producto.cpf_nro || null,
        cpfNro: producto.cpf_nro || null,
        _original: { ...producto }
    };

    productosFactura.push(productoNormalizado);

    registrarUltimoCambioProducto('agregado', productoNormalizado, productosFactura.length - 1);

    console.log('✅ Producto agregado con TODOS los campos:');
    console.log(`   - Item: ${productoNormalizado.item}`);
    console.log(`   - p_id: ${productoNormalizado.p_id}`);
    console.log(`   - cantidad_tot: ${productoNormalizado.cantidad_tot}`);
    console.log(`   📊 Total productos en grilla: ${productosFactura.length}`);
    console.log('═══════════════════════════════════════════════════');

    recalcularTotalFactura();
    actualizarGrillaProductos();
    $('#cantidadItems').text(productosFactura.length);

    return {
        accion: 'agregado',
        producto: productoNormalizado,
        indice: productosFactura.length - 1
    };
}

/**
 * ✅ ACTUALIZADO v10.0: Recalcula el total de la factura CON REDONDEO
 * CORREGIDO: Aplica redondeo a cada precio total antes de sumar
 */
function recalcularTotalFactura() {
    console.log('═══════════════════════════════════════════════════');
    console.log('💰 RECALCULANDO TOTAL DE FACTURA v10.0 (CON REDONDEO)');
    console.log('═══════════════════════════════════════════════════');

    // ❶ Sumar precios totales CON REDONDEO
    totalFactura = productosFactura.reduce((sum, prod) => {
        const precioTotal = redondear(prod.precioTotal || 0, 2);
        return sum + precioTotal;
    }, 0);

    // ❷ CRÍTICO: Redondear el total final
    totalFactura = redondear(totalFactura, 2);

    console.log(`   📊 Total productos: ${productosFactura.length}`);
    console.log(`   💵 Total calculado (redondeado): $ ${formatearNumero(totalFactura, 2)}`);
    console.log(`   🔢 Total raw: ${totalFactura}`);
    console.log('═══════════════════════════════════════════════════');

    $('#txtTotalFactura').val(`$ ${formatearNumero(totalFactura, 2)}`);
}


/**
 * ✅ ACTUALIZADO v8.1: Actualiza la visualización de la grilla principal
 * NUEVO: Resalta visualmente altas y fusiones
 */
function actualizarGrillaProductos() {
    const $tbody = $('#tbodyProductos');

    if (productosFactura.length === 0) {
        $tbody.html(`
            <tr id="rowSinProductos" class="compact-row">
                <td colspan="9" class="text-center text-muted py-4">
                    <i class='bx bx-package bx-lg text-golden'></i>
                    <p class="mb-0 mt-2">
                        <strong>No hay productos cargados</strong><br>
                        <small>Busque un producto por código o código de barras</small>
                    </p>
                </td>
            </tr>
        `);
        return;
    }

    $('#rowSinProductos').remove();

    let html = '';

    productosFactura.forEach((producto, index) => {
        const claveActual = normalizarClaveProducto(producto.p_id);
        const esUltimoCambio = ultimoCambioProducto && ultimoCambioProducto.p_id === claveActual;
        const esFusionado = esUltimoCambio && ultimoCambioProducto.accion === 'fusionado';
        const esAgregado = esUltimoCambio && ultimoCambioProducto.accion === 'agregado';

        const claseFila = esFusionado
            ? 'table-warning'
            : esAgregado
                ? 'table-success'
                : '';

        const badgeCambio = esFusionado
            ? `<span class="badge bg-warning text-dark ms-2">Fusionado</span>`
            : esAgregado
                ? `<span class="badge bg-success ms-2">Nuevo</span>`
                : '';

        html += `
            <tr class="compact-row ${claseFila}" data-index="${index}" data-item="${producto.item}">
                <td class="text-center text-muted fw-bold">${producto.item}</td>
                <td class="text-center fw-bold">${escapeHtml(producto.p_id)}</td>
                <td class="text-center">${escapeHtml(producto.p_id_barrado)}</td>
                <td class="text-start" style="max-width: 250px; overflow: hidden; text-overflow: ellipsis; white-space: nowrap;" title="${escapeHtml(producto.descripcion)}">
                    ${escapeHtml(producto.descripcion)} ${badgeCambio}
                </td>
                <td class="text-center">
                    <span class="badge badge-compact bg-info">${formatearNumero(normalizarNumero(producto.unidadPresentacion, 1), 0)}</span>
                </td>
                <td class="text-end fw-bold">${formatearNumero(normalizarNumero(producto.cantidadTotal, 0), 2)}</td>
                <td class="text-end">$ ${formatearNumero(normalizarNumero(producto.precioVenta, 0), 2)}</td>
                <td class="text-end fw-bold text-success">$ ${formatearNumero(normalizarNumero(producto.precioTotal, 0), 2)}</td>
                <td class="text-center">
                    <button class="btn btn-primary btn-xs"
                            type="button"
                            title="Editar"
                            onclick="editarProducto(${index})">
                        <i class='bx bx-edit-alt'></i>
                    </button>
                    <button class="btn btn-danger btn-xs"
                            type="button"
                            title="Eliminar"
                            onclick="eliminarProducto(${index})">
                        <i class='bx bx-trash'></i>
                    </button>
                </td>
            </tr>
        `;
    });

    $tbody.html(html);
    console.log('✅ Grilla principal actualizada visualmente (con items y cambios visibles)');
}

/**
 * ✅ ACTUALIZADO v10.0: Calcula precio total CON REDONDEO
 * CORREGIDO: Redondea el resultado final
 */
function calcularPrecioTotal(producto) {
    const precioVenta = parseFloat(producto.p_pvta) || 0;
    const cantidad = parseFloat(producto.cantidad_tot) || 1;

    // ❶ Multiplicar
    const precioTotal = precioVenta * cantidad;

    // ❷ CRÍTICO: Redondear el resultado
    const precioTotalRedondeado = redondear(precioTotal, 2);

    console.log(`💰 Cálculo precio total (CON REDONDEO):`);
    console.log(`   Precio Venta: $ ${precioVenta}`);
    console.log(`   Cantidad: ${cantidad}`);
    console.log(`   Resultado sin redondear: $ ${precioTotal}`);
    console.log(`   Resultado REDONDEADO: $ ${precioTotalRedondeado}`);

    return precioTotalRedondeado;
}

/**
 * ✅ NUEVO v6.0: Abre el modal de cálculo de factura
 */
function abrirModalCalculoFactura(data) {
    console.log('═══════════════════════════════════════════════════');
    console.log('📊 ABRIENDO MODAL DE CÁLCULO DE FACTURA');
    console.log('═══════════════════════════════════════════════════');
    console.log('Datos de cálculo:', data);
    
    const $modal = $('#modalCalculoFactura');
    
    // ❶ Hidratar datos en el modal
    $modal.find('#txtTotalGravado').val(`$ ${formatearNumero(data.tot_gravado, 2)}`);
    $modal.find('#txtTotalExento').val(`$ ${formatearNumero(data.tot_exento, 2)}`);
    $modal.find('#txtTotalNoGravado').val(`$ ${formatearNumero(data.tot_no_gravado, 2)}`);
    $modal.find('#txtTotalImpuestoInterno').val(`$ ${formatearNumero(data.tot_ii, 2)}`);
    $modal.find('#txtTotalIva').val(`$ ${formatearNumero(data.tot_iva, 2)}`);
    $modal.find('#txtSubtotal').val(`$ ${formatearNumero(data.tot_subtotal, 2)}`);
    $modal.find('#txtDescuentos').val(`$ ${formatearNumero(data.tot_descuentos, 2)}`);
    $modal.find('#txtRecargo').val(`$ ${formatearNumero(data.tot_recargo, 2)}`);
    $modal.find('#txtTotalFactura').val(`$ ${formatearNumero(data.tot_factura, 2)}`);
    
    // ❷ Mostrar detalles de cada producto
    const $tbodyDetalles = $modal.find('#tbodyDetallesProductos');
    $tbodyDetalles.empty();
    
    data.productos.forEach((prod, index) => {
        const precioTotal = calcularPrecioTotal(prod);
        
        $tbodyDetalles.append(`
            <tr>
                <td class="text-center">${prod.item}</td>
                <td class="text-start">${escapeHtml(prod.p_desc)}</td>
                <td class="text-center">${prod.cantidad_tot}</td>
                <td class="text-end">$ ${formatearNumero(prod.p_pvta, 2)}</td>
                <td class="text-end">$ ${formatearNumero(precioTotal, 2)}</td>
            </tr>
        `);
    });
    
    console.log('✅ Detalles de productos cargados en el modal');
    
    // ❸ Abrir modal
    $modal.modal('show');
    
    console.log('✅ Modal de cálculo abierto');
}

/**
 * ✅ NUEVO v6.0: Calcula el total de la factura
 * - Sincroniza con el SP los campos de totals
 */
function calcularTotalFactura() {
    console.log('═══════════════════════════════════════════════════');
    console.log('📊 CALCULANDO TOTAL DE FACTURA v6.0');
    console.log('═══════════════════════════════════════════════════');

    const request = {
        productos: construirProductosDTO(),
        lp_id: clienteActualFactura.listaPrecio || '001'
    };

    console.log('📤 Request a enviar:', request);

    // Mostrar loader
    $('#btnConfirmarFactura').prop('disabled', true).html('<i class="bx bx-loader-alt bx-spin"></i> Calculando...');

    $.ajax({
        url: typeof CalcularTotalesUrl !== 'undefined' && CalcularTotalesUrl
            ? CalcularTotalesUrl
            : '/Facturacion/ProductoFact/CalcularTotales',
        type: 'POST',
        contentType: 'application/json',
        data: JSON.stringify(request),
        success: function (response) {
            console.log('✅ RESPUESTA RECEBIDA DE CALCULO DE TOTAL');
            console.log('   Response:', response);
            
            if (response.ok) {
                // Actualizar totales en la UI
                totalFactura = response.totales.tot_factura || 0;
                
                $('#txtTotalFactura').val(`$ ${formatearNumero(totalFactura, 2)}`);
                $('#cantidadItems').text(response.totales.tot_items || 0);
                
                console.log('💰 Total de factura actualizado: $', formatearNumero(totalFactura, 2));
                
                // Habilitar botón de confirmación
                $('#btnConfirmarFactura').prop('disabled', false).html('<i class="bx bx-check-circle"></i> SEGUIR');
            } else {
                mostrarMensajeError(response.mensaje || 'Error al calcular el total de la factura');
            }
        },
        error: function (xhr, status, error) {
            console.error('❌ ERROR AL CALCULAR TOTAL DE FACTURA');
            ocultarLoaderCalculando();

            // ✅ Usar función centralizada
            if (esSesionExpirada(xhr.status)) {
                manejarSesionExpirada('No se pudo calcular el total de la factura porque su sesión ha expirado.');
                return;
            }

            let mensaje = 'Error al calcular el total de la factura. Por favor, intente nuevamente.';
            if (xhr.status === 500) {
                mensaje = 'Error interno del servidor. Contacte al administrador.';
            }

            mostrarMensajeError(mensaje);
        },
        complete: function () {
            // Ocultar loader
            $('#btnConfirmarFactura').prop('disabled', false);
        }
    });
}

// ═══════════════════════════════════════════════════════════════════
// SECCIÓN 6: CONFIRMACIÓN Y CANCELACIÓN
// ═══════════════════════════════════════════════════════════════════

/**
 * Confirma la cancelación de la factura
 */
function confirmarCancelarFactura() {
    console.log('⚠️ Confirmando cancelación de factura...');

    if (productosFactura.length > 0) {
        AbrirMensaje(
            "Confirmar Cancelación",
            "¿Está seguro que desea cancelar la factura?\n\n" +
            "Se perderán todos los productos cargados.",
            function () {
                $("#msjModal").modal("hide");
                ejecutarCancelarFactura();
            },
            true,
            ["Sí, cancelar", "No"],
            "warning",
            null
        );
    } else {
        ejecutarCancelarFactura();
    }
}

/**
 * Ejecuta la cancelación de la factura
 */
function ejecutarCancelarFactura() {
    console.log('🔙 Ejecutando cancelación de factura...');

    ocultarSeccionProductos();
    $(document).trigger('volverAIdentificarCliente');

    console.log('✅ Evento "volverAIdentificarCliente" disparado');
}

// ════════════════════════════════════════════════════════════
// SECCIÓN 8: LIMPIEZA COMPLETA DE VENTA (✅ NUEVO v8.0)
// ════════════════════════════════════════════════════════════

/**
 * ✅ NUEVO v8.0: Limpia completamente el módulo de ventas
 * Se invoca después de:
 * - Diferir Factura
 * - Diferir Pago
 * - Cancelar Factura
 * 
 * ACCIONES:
 * 1. Limpia arrays de productos
 * 2. Resetea totales
 * 3. Limpia cliente actual
 * 4. Cierra modal de productos
 * 5. Resetea campos de búsqueda
 * 6. Restaura estado inicial de mensajes
 */
function limpiarVentaCompleta() {
    console.log('═══════════════════════════════════════════════════');
    console.log('🧹 LIMPIEZA COMPLETA DE VENTA v8.0');
    console.log('═══════════════════════════════════════════════════');

    // ❶ Limpiar arrays y variables globales
    productosFactura = [];
    totalFactura = 0;
    clienteActualFactura = null;
    modoBloqueoGrilla = null;
    origenCargaActual = 'directo';

    console.log('✅ Variables globales reseteadas');

    // ❷ Limpiar grilla visual
    $('#tbodyProductos').html(`
        <tr id="rowSinProductos" class="compact-row">
            <td colspan="9" class="text-center text-muted py-4">
                <i class='bx bx-package bx-lg text-golden'></i>
                <p class="mb-0 mt-2">
                    <strong>No hay productos cargados</strong><br>
                    <small>Busque un producto por código o código de barras</small>
                </p>
            </td>
        </tr>
    `);

    // ❸ Resetear totales
    $('#txtTotalFactura').val('$ 0.00');
    $('#cantidadItems').text('0');

    // ❹ Limpiar campos de búsqueda
    $('#txtCodigoProducto').val('').prop('disabled', false);
    $('#btnBuscarProducto').prop('disabled', false).html('<i class="bx bx-search"></i>');

    // ❺ Restaurar mensaje de estado inicial
    $('#mensajeEstadoProducto')
        .removeClass('text-danger text-success text-info')
        .addClass('text-muted')
        .html('Presione <kbd>Enter</kbd> o <strong>BUSCAR</strong> para agregar producto');

    console.log('✅ Campos y mensajes restaurados');

    // ❻ Limpiar datos del cliente en modal de productos
    $('#txtClienteNombreProd').val('');
    $('#txtClienteIdProd').val('');
    $('#txtClienteDomicilioProd').val('');
    $('#txtCondicionAfipProd').val('');
    $('#txtClienteCuitProd').val('');
    $('#txtClienteEmailProd').val('');
    $('#txtClienteMovilProd').val('');

    console.log('✅ Datos del cliente limpiados del modal');

    // ❼ CERRAR MODAL DE PRODUCTOS (si está abierto)
    if ($('#modalProductosFactura').hasClass('show')) {
        $('#modalProductosFactura').modal('hide');
        console.log('✅ Modal de productos cerrado');
    }

    // ❽ Remover overlays (si existen)
    $('#overlayCalculando').remove();

    console.log('═══════════════════════════════════════════════════');
    console.log('✅ LIMPIEZA COMPLETA FINALIZADA');
    console.log('═══════════════════════════════════════════════════');
}

/**
 * ✅ NUEVO v8.0: Limpia la grilla de productos y resetea totales
 */
function limpiarGrillaProductos() {
    console.log('🧹 Limpiando grilla de productos...');

    productosFactura = [];
    totalFactura = 0;
    modoBloqueoGrilla = null;
    ultimoCambioProducto = null;

    $('#tbodyProductos').html(`
        <tr id="rowSinProductos" class="compact-row">
            <td colspan="9" class="text-center text-muted py-4">
                <i class='bx bx-package bx-lg text-golden'></i>
                <p class="mb-0 mt-2">
                    <strong>No hay productos cargados</strong><br>
                    <small>Busque un producto por código o código de barras</small>
                </p>
            </td>
        </tr>
    `);

    $('#txtTotalFactura').val('$ 0.00');
    $('#cantidadItems').text('0');

    $('#mensajeEstadoProducto')
        .removeClass('text-danger text-success text-info')
        .addClass('text-muted')
        .html('Presione <kbd>Enter</kbd> o <strong>BUSCAR</strong> para agregar producto');

    console.log('✅ Grilla limpiada');
}

/**
 * NUEVO v5.0: Muestra la sección de productos y oculta otras secciones
 * @param {Object} clienteData - Los datos del cliente confirmado
 */
function mostrarSeccionProductos(clienteData) {
    console.log('═══════════════════════════════════════════════════');
    console.log('📦 MOSTRAR MODAL DE PRODUCTOS');
    console.log('═══════════════════════════════════════════════════');
    console.log('Cliente recibido:', clienteData);

    if (!clienteData) {
        console.warn('⚠️ No se recibieron datos del cliente');
        return;
    }

    clienteActualFactura = clienteData;

    $('#txtClienteNombreProd').val(clienteData.denominacion || '');
    $('#txtClienteIdProd').val(clienteData.id || 'N/A');
    $('#txtClienteDomicilioProd').val(clienteData.domicilio || '');
    $('#txtCondicionAfipProd').val(clienteData.condicionAfip || '');
    $('#txtClienteCuitProd').val(clienteData.tipoNumero || '');
    $('#txtClienteEmailProd').val(clienteData.email || '');
    $('#txtClienteMovilProd').val(clienteData.movil || '');

    actualizarTipoComprobante(clienteData);

    const modalElement = document.getElementById('modalProductosFactura');

    if (modalElement && window.bootstrap && window.bootstrap.Modal) {
        const modal = window.bootstrap.Modal.getOrCreateInstance(modalElement);
        modal.show();
    } else {
        $('#modalProductosFactura').modal('show');
    }

    $('#modalProductosFactura')
        .off('shown.bs.modal.prodfact')
        .on('shown.bs.modal.prodfact', function () {
            setTimeout(() => {
                $('#txtCodigoProducto').trigger('focus');
            }, 200);
        });

    console.log('✅ Modal de productos abierto correctamente');
}

/**
 * NUEVO v5.0: Actualiza el tipo de comprobante en función de los datos del cliente
 * @param {Object} clienteData - Los datos del cliente confirmado
 */
function actualizarTipoComprobante(clienteData) {
    const $badge = $('#badgeTipoComprobante');

    if ($badge.length === 0) {
        console.warn('⚠️ No se encontró el badge de tipo de comprobante');
        return;
    }

    let tipoFactura = 'FACTURA B';
    let iconoFactura = 'bx-file';

    if (clienteData?.emite && String(clienteData.emite).trim() !== '') {
        tipoFactura = String(clienteData.emite).toUpperCase();

        if (tipoFactura.includes('FACTURA A')) {
            iconoFactura = 'bx-file-blank';
        } else if (tipoFactura.includes('FACTURA C')) {
            iconoFactura = 'bx-file';
        } else if (tipoFactura.includes('NOTA')) {
            iconoFactura = 'bx-receipt';
        }
    } else if (clienteData?.condicionAfip) {
        const condicion = String(clienteData.condicionAfip).toUpperCase();

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

/**
 * OCULTAR sección de productos: Limpia los datos del cliente y cierra el modal de productos
 */
function ocultarSeccionProductos() {
    console.log('═══════════════════════════════════════════════════');
    console.log('🔙 OCULTAR MODAL DE PRODUCTOS');
    console.log('═══════════════════════════════════════════════════');

    $('#txtClienteNombreProd').val('');
    $('#txtClienteIdProd').val('');
    $('#txtClienteDomicilioProd').val('');
    $('#txtCondicionAfipProd').val('');
    $('#txtClienteCuitProd').val('');
    $('#txtClienteEmailProd').val('');
    $('#txtClienteMovilProd').val('');
    $('#txtCodigoProducto').val('');

    limpiarGrillaProductos();
    clienteActualFactura = null;
    modoBloqueoGrilla = null;
    origenCargaActual = 'directo';
    ultimoCambioProducto = null;

    const modalElement = document.getElementById('modalProductosFactura');

    if (modalElement && window.bootstrap && window.bootstrap.Modal) {
        const modal = window.bootstrap.Modal.getInstance(modalElement);
        if (modal) {
            modal.hide();
        } else {
            $('#modalProductosFactura').modal('hide');
        }
    } else {
        $('#modalProductosFactura').modal('hide');
    }

    console.log('✅ Modal de productos ocultado correctamente');
}

/**
 * ✅ ACTUALIZADO v10.0: Confirma factura CON REDONDEO EN TODOS LOS VALORES
 */
function confirmarFactura() {
    console.log('═══════════════════════════════════════════════════');
    console.log('✅ CONFIRMANDO FACTURA v10.0 (CON REDONDEO)');
    console.log('═══════════════════════════════════════════════════');

    // ❶ Validaciones...
    if (productosFactura.length === 0) {
        console.warn('⚠️ No hay productos cargados');
        mostrarMensajeError('Debe cargar al menos un producto para continuar');
        return;
    }

    if (!clienteActualFactura) {
        console.error('❌ No hay cliente seleccionado');
        mostrarMensajeError('Error: No hay cliente seleccionado');
        return;
    }

    // ❸ ✅ NUEVO: Construir JSON CON VALORES REDONDEADOS
    const productosArray = productosFactura.map((producto) => {
        return {
            // ═══════════════════════════════════════════════════
            // ✅ IDENTIFICACIÓN
            // ═══════════════════════════════════════════════════
            item: producto.item || 0,
            p_id: producto.p_id || '',
            p_id_barrado: producto.p_id_barrado || '',
            p_desc: producto.p_desc || '',

            // ═══════════════════════════════════════════════════
            // ✅ PRECIOS (REDONDEADOS)
            // ═══════════════════════════════════════════════════
            p_pcosto: redondear(producto.p_pcosto || 0, 2),
            p_pcosto_repo: redondear(producto.p_pcosto_repo || 0, 2),
            p_pneto: redondear(producto.p_pneto || 0, 2),
            p_pvta: redondear(producto.p_pvta || 0, 2),
            p_margen_imp: redondear(producto.p_margen_imp || 0, 2),
            p_margen_vig: redondear(producto.p_margen_vig || 0, 2),

            // ═══════════════════════════════════════════════════
            // ✅ CANTIDAD Y TOTAL (REDONDEADOS)
            // ═══════════════════════════════════════════════════
            cantidad_tot: redondear(producto.cantidad_tot || 0, 2),
            p_pvta_tot: redondear(producto.p_pvta_tot || 0, 2),
            bultos: redondear(producto.bultos || 0, 0),

            // ═══════════════════════════════════════════════════
            // ✅ IVA (REDONDEADO)
            // ═══════════════════════════════════════════════════
            iva_situacion: producto.iva_situacion || '',
            iva_alicuota: redondear(producto.iva_alicuota || 0, 2),
            p_iva: redondear(producto.p_iva || 0, 2),

            // ═══════════════════════════════════════════════════
            // ✅ IMPUESTOS INTERNOS (REDONDEADO)
            // ═══════════════════════════════════════════════════
            in_alicuota: redondear(producto.in_alicuota || 0, 2),
            p_in: redondear(producto.p_in || 0, 2),

            // ═══════════════════════════════════════════════════
            // ✅ PREVISIÓN DE LISTA (REDONDEADO)
            // ═══════════════════════════════════════════════════
            lp_prevision_tot: redondear(producto.lp_prevision_tot || 0, 2),
            lp_prevision_pin: redondear(producto.lp_prevision_pin || 0, 2),

            // ═══════════════════════════════════════════════════
            // ✅ PRECIO DE OFERTA (REDONDEADO)
            // ═══════════════════════════════════════════════════
            po: producto.po || false,
            po_limite: redondear(producto.po_limite || 0, 2),

            // ═══════════════════════════════════════════════════
            // ✅ TOTALES DE COMPROBANTE (REDONDEADO)
            // ═══════════════════════════════════════════════════
            cm_gravado: redondear(producto.cm_gravado || 0, 2),
            cm_no_gravado: redondear(producto.cm_no_gravado || 0, 2),
            cm_exento: redondear(producto.cm_exento || 0, 2),
            cm_iva: redondear(producto.cm_iva || 0, 2),
            cm_ii: redondear(producto.cm_ii || 0, 2),

            // ═══════════════════════════════════════════════════
            // ✅ DESCUENTOS (REDONDEADO)
            // ═══════════════════════════════════════════════════
            cm_dto: redondear(producto.cm_dto || 0, 2),
            cm_dto_porc: redondear(producto.cm_dto_porc || 2, 2),

            // ═══════════════════════════════════════════════════
            // ✅ ORIGEN (SIN CAMBIOS)
            // ═══════════════════════════════════════════════════
            cta_id: producto.cta_id || '',
            pre_id: producto.pre_id || null,
            cpf_nro: producto.cpf_nro || null,

            // ═══════════════════════════════════════════════════
            // ✅ COMBOS (SIN CAMBIOS)
            // ═══════════════════════════════════════════════════
            cmb_p_id: producto.cmb_p_id || '',
            cmd_cmb: producto.cmd_cmb || '',
            cmd_cmb_id: producto.cmd_cmb_id || '',
            cmd_cmb_dto: redondear(producto.cmd_cmb_dto || 0, 2),
            cmd_cmb_cant: redondear(producto.cmd_cmb_cant || 0, 2),
            cmd_cmb_desc: producto.cmd_cmb_desc || '',

            // ═══════════════════════════════════════════════════
            // ✅ CÓDIGO DE BARRAS (SIN CAMBIOS)
            // ═══════════════════════════════════════════════════
            barre: producto.barre || ''
        };
    });

    const jsonProductos = JSON.stringify(productosArray);

    console.log('═══════════════════════════════════════════════════');
    console.log('📋 JSON CON VALORES REDONDEADOS generado:');
    console.log(jsonProductos);
    console.log('═══════════════════════════════════════════════════');

    // ❹ ✅ NUEVO: Calcular totales CON REDONDEO
    const tot_rows = productosFactura.length;

    // ✅ Suma de cantidades REDONDEADA
    const tot_cantidad = redondear(
        productosFactura.reduce((sum, p) => sum + (parseFloat(p.cantidad_tot) || 0), 0),
        2
    );

    // ✅ Total de precios REDONDEADO
    const tot_pvta = redondear(totalFactura, 2);

    // ❺ Construir request DTO
    const request = {
        json_p: jsonProductos,
        tot_rows: tot_rows,
        tot_cantidad: tot_cantidad,
        tot_pvta: tot_pvta,  // ← ✅ AHORA ESTÁ REDONDEADO
        lp_id: clienteActualFactura.listaPrecio || '001'
    };

    console.log('═══════════════════════════════════════════════════');
    console.log('📤 REQUEST CON TOTALES REDONDEADOS:');
    console.log(`   tot_rows: ${tot_rows}`);
    console.log(`   tot_cantidad: ${tot_cantidad}`);
    console.log(`   tot_pvta: ${tot_pvta} ← ✅ REDONDEADO`);
    console.log('═══════════════════════════════════════════════════');

    // ❻ Mostrar loader
    mostrarLoaderCalculando();

    // ❼ Llamar a la API
    $.ajax({
        url: typeof CalcularFilasUrl !== 'undefined' && CalcularFilasUrl
            ? CalcularFilasUrl
            : '/Facturacion/ProductoFact/CalcularFilas',
        type: 'POST',
        contentType: 'application/json',
        data: JSON.stringify(request),
        success: function (response) {
            console.log('✅ RESPUESTA RECIBIDA');
            ocultarLoaderCalculando();

            if (!response || typeof response !== 'object') {
                mostrarMensajeError('Error: Respuesta inválida del servidor');
                return;
            }

            if (!response.ok) {
                mostrarMensajeError(response.mensaje || 'Error al calcular totales');
                return;
            }

            abrirModalCalculoFactura(response);
        },
        error: function (xhr, status, error) {
            console.error('❌ ERROR EN CALCULAR FILAS');
            ocultarLoaderCalculando();

            if (esSesionExpirada(xhr.status)) {
                manejarSesionExpirada('No se pudo calcular la factura porque su sesión ha expirado.');
                return;
            }

            let mensaje = 'Error al calcular totales. Por favor, intente nuevamente.';
            if (xhr.status === 500) {
                mensaje = 'Error interno del servidor. Contacte al administrador.';
            }

            mostrarMensajeError(mensaje);
        }
    });
}

/**
* ✅ NUEVO v6.0: Muestra loader mientras se calculan totales
*/
function mostrarLoaderCalculando() {
    console.log('⏳ Mostrando loader de cálculo...');

    // Deshabilitar botones
    $('#btnConfirmarFactura').prop('disabled', true).html('<i class="bx bx-loader-alt bx-spin"></i> Calculando...');
    $('#btnCancelarFactura').prop('disabled', true);

    // Mostrar overlay en la grilla
    if ($('#overlayCalculando').length === 0) {
        $('#modalProductosFactura .modal-body').append(`
            <div id="overlayCalculando" style="
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
                    <div class="spinner-border spinner-border-golden mb-3" role="status">
                        <span class="visually-hidden">Calculando...</span>
                    </div>
                    <h5 class="text-golden">Calculando totales...</h5>
                    <p class="text-muted">Por favor, espere un momento</p>
                </div>
            </div>
        `);
    }
}

/**
* ✅ NUEVO v6.0: Oculta loader de cálculo
*/
function ocultarLoaderCalculando() {
    console.log('✅ Ocultando loader de cálculo...');

    // Rehabilitar botones
    $('#btnConfirmarFactura').prop('disabled', false).html('<i class="bx bx-check-circle"></i> SEGUIR');
    $('#btnCancelarFactura').prop('disabled', false);

    // Remover overlay
    $('#overlayCalculando').remove();
}