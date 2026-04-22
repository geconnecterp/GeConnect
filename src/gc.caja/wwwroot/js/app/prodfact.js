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
        cargarPreFactura();
    });
    
    $('#btnFacturaEmitida').on('click', function() {
        console.log('🧾 Cargar Factura Emitida...');
        cargarFacturaEmitida();
    });
    
    $('#btnCotizacion').on('click', function() {
        console.log('💰 Cargar Cotización...');
        cargarCotizacion();
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
            console.error('❌ Error AJAX:', {
                status: xhr.status,
                error: error,
                responseText: xhr.responseText
            });
            
            let mensaje = 'Error al buscar el producto. Por favor, intente nuevamente.';
            
            if (xhr.status === 401 || xhr.status === 403) {
                mensaje = 'Su sesión ha expirado. Por favor, vuelva a iniciar sesión.';
            } else if (xhr.status === 500) {
                mensaje = 'Error interno del servidor. Contacte al administrador.';
                
                // ✅ NUEVO: Mostrar detalles del error en consola
                try {
                    const errorDetail = JSON.parse(xhr.responseText);
                    console.error('📋 Detalles del error 500:', errorDetail);
                } catch (e) {
                    console.error('📋 Respuesta del servidor:', xhr.responseText);
                }
            }
            
            // ✅ CORREGIDO: Mostrar error de comunicación
            $('#mensajeEstadoProducto')
                .removeClass('text-info text-success text-muted')  // ✅ Incluir text-muted
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
 * ✅ ACTUALIZADO: Procesa la respuesta del servidor
 * CORREGIDO: Manejo correcto de estados del mensaje
 */
function procesarRespuestaProducto(response, origenCarga) {
    console.log('═══════════════════════════════════════════════════');
    console.log('📊 PROCESANDO RESPUESTA DE PRODUCTO v5.0 CORREGIDA');
    console.log('═══════════════════════════════════════════════════');
    console.log('   Origen de carga:', origenCarga);
    console.log('   Response:', response);
    
    const producto = response.producto;
    
    // ❶ VALIDAR QUE EXISTA PRODUCTO
    if (!producto) {
        console.warn('⚠️ No se recibió producto en la respuesta');
        
        // ✅ CORREGIDO
        $('#mensajeEstadoProducto')
            .removeClass('text-info text-success text-muted')  // ✅ Incluir text-muted
            .addClass('text-danger')
            .html(`<i class='bx bx-error-circle'></i> Producto no encontrado`);
        
        mostrarMensajeError('El producto no existe o no está disponible');
        return;
    }
    
    // ❷ CASO: Array vacío
    if (Array.isArray(producto) && producto.length === 0) {
        console.warn('⚠️ Array de productos vacío');
        
        // ✅ CORREGIDO
        $('#mensajeEstadoProducto')
            .removeClass('text-info text-success text-muted')  // ✅ Incluir text-muted
            .addClass('text-danger')
            .html(`<i class='bx bx-error-circle'></i> Producto no encontrado`);
        
        mostrarMensajeError('El producto no existe o no está disponible');
        return;
    }
    
    // ❸ CASO: Producto único (esUnico = true o no es array)
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
                <td class="text-center ${textClass}">
                    <span class="badge bg-info">${unidadPres}</span>
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
    console.log(`🔍 VALIDANDO PRODUCTO ÚNICO`);
    console.log('═══════════════════════════════════════════════════');
    console.log(`   Descripción: ${descripcion}`);
    console.log(`   Respuesta: ${respuesta}`);
    console.log(`   Origen carga: ${origenCarga}`);

    // ❶ Producto con error (respuesta != 0)
    if (respuesta !== 0) {
        const mensaje = producto.respuesta_msj || 'El producto no se puede cargar';

        console.error(`❌ Producto con error: ${mensaje}`);

        // Para último detalle, ignorar silenciosamente
        if (origenCarga === 'ultimo') {
            console.log('ℹ️ Error ignorado (último detalle)');
            return;
        }

        // ✅ CORREGIDO: Eliminar TODAS las clases de color
        $('#mensajeEstadoProducto')
            .removeClass('text-info text-success text-muted')  // ✅ Incluir text-muted
            .addClass('text-danger')
            .html(`<i class='bx bx-error-circle'></i> ${mensaje}`);

        mostrarMensajeError(mensaje);
        return;
    }

    // �②  Producto válido
    console.log('✅ Producto válido, agregando a grilla...');
    agregarProductoAGrilla(producto);

    // ✅ CORREGIDO: Mensaje de éxito
    $('#mensajeEstadoProducto')
        .removeClass('text-info text-danger text-muted')  // ✅ Incluir text-muted
        .addClass('text-success')
        .html(`<i class='bx bx-check-circle'></i> Producto agregado`);

    // ✅ CORREGIDO: Restaurar estado inicial después de 3 segundos
    setTimeout(() => {
        $('#mensajeEstadoProducto')
            .removeClass('text-success text-danger text-info')  // ✅ Eliminar colores
            .addClass('text-muted')  // ✅ Restaurar text-muted
            .html('Presione <kbd>Enter</kbd> o <strong>BUSCAR</strong> para agregar producto');
    }, 3000);
}

// ═══════════════════════════════════════════════════════════════════
// SECCIÓN 3: GESTIÓN DE GRILLA PRINCIPAL
// ═══════════════════════════════════════════════════════════════════

/**
 * ✅ ACTUALIZADO v5.0: Agrega un producto a la grilla principal
 * Adaptado a la estructura de ProductoDatosResponseDto
 */
function agregarProductoAGrilla(producto) {
    console.log('═══════════════════════════════════════════════════');
    console.log('➕ AGREGANDO PRODUCTO A LA GRILLA v5.0 REVERTIDO');
    console.log('═══════════════════════════════════════════════════');
    console.log('   Producto recibido:', producto);

    // ✅ Normalizar producto
    const productoNormalizado = {
        // IDs y códigos
        p_id: producto.p_id || '???',
        p_id_barrado: producto.p_id_barrado || '',

        // Descripción
        descripcion: producto.p_desc || 'Sin descripción',

        // Presentación
        unidadPresentacion: producto.p_unidad_pres || 1,
        peso: producto.p_peso || 0,

        // ✅ Cantidad (YA calculada por el SP)
        cantidadTotal: producto.cantidad_tot || 1,

        // Precios
        precioVenta: producto.p_pvta || 0,
        precioCosto: producto.p_pcosto || 0,

        // IVA
        ivaAlicuota: producto.iva_alicuota || 0,
        ivaImporte: producto.p_iva || 0,

        // Impuestos internos
        internAlicuota: producto.in_alicuota || 0,
        internImporte: producto.p_in || 0,

        // Otros
        rubro: producto.rub_desc || '',
        activo: producto.p_activo || 'S',

        // ✅ Calcular precio total
        precioTotal: calcularPrecioTotal(producto),

        // Datos de origen
        preNro: producto.pre_nro || null,
        cpfNro: producto.cpf_nro || null,

        // ✅ Guardar datos completos
        _original: producto
    };

    // Agregar al array
    productosFactura.push(productoNormalizado);

    // ✅ LOGS SIMPLIFICADOS
    console.log(`✅ Producto agregado a grilla:`);
    console.log(`   - Código: ${productoNormalizado.p_id}`);
    console.log(`   - Descripción: ${productoNormalizado.descripcion}`);
    console.log(`   - Unidades por Presentación: ${productoNormalizado.unidadPresentacion}`);
    console.log(`   - Cantidad Total (calculada por SP): ${productoNormalizado.cantidadTotal}`);
    console.log(`   - Precio Unitario: $ ${formatearNumero(productoNormalizado.precioVenta, 2)}`);
    console.log(`   - Precio Total: $ ${formatearNumero(productoNormalizado.precioTotal, 2)}`);
    console.log(`   📊 Total productos en grilla: ${productosFactura.length}`);
    console.log('═══════════════════════════════════════════════════');

    // Recalcular total
    recalcularTotalFactura();

    // Actualizar grilla visual
    actualizarGrillaProductos();

    // Actualizar contador de items
    $('#cantidadItems').text(productosFactura.length);
}

/**
 * ✅ NUEVO v5.0: Calcula el precio total de un producto
 * Fórmula: Precio Venta * Cantidad
 * (IVA e Imp. Internos ya están incluidos en p_pvta)
 */
function calcularPrecioTotal(producto) {
    const precioVenta = producto.p_pvta || 0;
    const cantidad = producto.cantidad_tot || 1;
    
    const precioTotal = precioVenta * cantidad;
    
    console.log(`💰 Cálculo precio total:`);
    console.log(`   Precio Venta (incluye impuestos): $ ${precioVenta}`);
    console.log(`   Cantidad: ${cantidad}`);
    console.log(`   PRECIO TOTAL: $ ${precioTotal}`);
    
    return precioTotal;
}

/**
 * ✅ ACTUALIZADO v5.0: Actualiza la visualización de la grilla principal
 */
function actualizarGrillaProductos() {
    const $tbody = $('#tbodyProductos');
    
    if (productosFactura.length === 0) {
        $tbody.html(`
            <tr id="rowSinProductos" class="compact-row">
                <td colspan="8" class="text-center text-muted py-4">
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
    
    // Eliminar mensaje de "sin productos"
    $('#rowSinProductos').remove();
    
    // Generar filas
    let html = '';
    productosFactura.forEach((producto, index) => {
        html += `
            <tr class="compact-row" data-index="${index}">
                <td class="text-center fw-bold">${escapeHtml(producto.p_id)}</td>
                <td class="text-center">${escapeHtml(producto.p_id_barrado)}</td>
                <td class="text-start" style="max-width: 250px; overflow: hidden; text-overflow: ellipsis; white-space: nowrap;" title="${escapeHtml(producto.descripcion)}">
                    ${escapeHtml(producto.descripcion)}
                </td>
                <td class="text-center">
                    <span class="badge badge-compact bg-info">${producto.unidadPresentacion}</span>
                </td>
                <td class="text-end fw-bold">${formatearNumero(producto.cantidadTotal, 2)}</td>
                <td class="text-end">$ ${formatearNumero(producto.precioVenta, 2)}</td>
                <td class="text-end fw-bold text-success">$ ${formatearNumero(producto.precioTotal, 2)}</td>
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
    console.log('✅ Grilla principal actualizada visualmente');
}

/**
 * ✅ NUEVO v4.0: Recalcula el total de la factura
 */
function recalcularTotalFactura() {
    totalFactura = productosFactura.reduce((sum, prod) => sum + (prod.precioTotal || 0), 0);
    
    $('#txtTotalFactura').val(`$ ${formatearNumero(totalFactura, 2)}`);
    
    console.log(`💰 Total factura recalculado: $ ${formatearNumero(totalFactura, 2)}`);
}

/**
 * ✅ NUEVO v4.0: Elimina un producto de la grilla
 */
function eliminarProducto(index) {
    if (index < 0 || index >= productosFactura.length) {
        console.error(`❌ Índice inválido: ${index}`);
        return;
    }
    
    const producto = productosFactura[index];
    
    console.log(`🗑️ Solicitando eliminación de producto: ${producto.descripcion}`);
    
    AbrirMensaje(
        "Confirmar Eliminación",
        `¿Está seguro que desea eliminar este producto?\n\n${producto.descripcion}`,
        function () {
            $("#msjModal").modal("hide");
            
            // Eliminar del array
            productosFactura.splice(index, 1);
            
            console.log(`✅ Producto eliminado. Total restante: ${productosFactura.length}`);
            
            // Si era una cotización y se eliminaron todos, desbloquear
            if (productosFactura.length === 0 && modoBloqueoGrilla === 'cotizacion') {
                modoBloqueoGrilla = null;
                console.log('✅ Modo bloqueo cotización desactivado');
            }
            
            // Actualizar grilla
            recalcularTotalFactura();
            actualizarGrillaProductos();
            $('#cantidadItems').text(productosFactura.length);
        },
        true,
        ["Sí, eliminar", "No"],
        "warning",
        null
    );
}

/**
 * ✅ NUEVO v4.0: Edita un producto de la grilla
 * TODO: Implementar modal de edición
 */
function editarProducto(index) {
    if (index < 0 || index >= productosFactura.length) {
        console.error(`❌ Índice inválido: ${index}`);
        return;
    }
    
    const producto = productosFactura[index];
    
    console.log(`✏️ Editar producto: ${producto.descripcion}`);
    console.log('⚠️ TODO: Implementar modal de edición de producto');
    
    // TODO: Abrir modal de edición con:
    // - Cantidad
    // - Precio unitario
    // - Descuento
    // - Recalcular precio total
}

// ═══════════════════════════════════════════════════════════════════
// SECCIÓN 4: CARGA ESPECIAL (Pre-Factura, Cotización, Último Detalle)
// ═══════════════════════════════════════════════════════════════════

/**
 * ✅ NUEVO v4.0: Carga una pre-factura
 */
function cargarPreFactura() {
    console.log('═══════════════════════════════════════════════════');
    console.log('📄 CARGAR PRE-FACTURA');
    console.log('═══════════════════════════════════════════════════');
    
    // TODO: Implementar búsqueda de pre-facturas
    // Debe mostrar un modal con lista de pre-facturas disponibles
    // Al seleccionar una, invocar buscarProductoPorCodigo con:
    // - tipoValor: 'F'
    // - valor: ID de la pre-factura
    // - cantidad: 1
    // - bulto: true
    // - origenCarga: 'prefactura'
    
    console.log('⚠️ TODO: Implementar modal de búsqueda de pre-facturas');
    mostrarMensajeAdvertencia('Funcionalidad en desarrollo');
}

/**
 * ✅ NUEVO v4.0: Carga una factura emitida
 */
function cargarFacturaEmitida() {
    console.log('═══════════════════════════════════════════════════');
    console.log('🧾 CARGAR FACTURA EMITIDA');
    console.log('═══════════════════════════════════════════════════');
    
    // TODO: Implementar búsqueda de facturas emitidas
    console.log('⚠️ TODO: Implementar modal de búsqueda de facturas emitidas');
    mostrarMensajeAdvertencia('Funcionalidad en desarrollo');
}

/**
 * ✅ NUEVO v4.0: Carga una cotización
 * RESTRICCIÓN: Solo si la grilla está vacía
 * EFECTO: Bloquea carga individual de productos
 */
function cargarCotizacion() {
    console.log('═══════════════════════════════════════════════════');
    console.log('💰 CARGAR COTIZACIÓN');
    console.log('═══════════════════════════════════════════════════');
    
    // ❶ Validar que la grilla esté vacía
    if (productosFactura.length > 0) {
        console.warn('⚠️ Grilla no vacía - No se puede cargar cotización');
        mostrarMensajeError(
            'No se puede cargar una cotización.\n\n' +
            'La grilla debe estar vacía. Por favor, elimine los productos actuales.'
        );
        return;
    }
    
    // TODO: Implementar búsqueda de cotizaciones
    // Al seleccionar una, invocar buscarProductoPorCodigo con:
    // - tipoValor: 'C'
    // - valor: ID de la cotización
    // - cantidad: 1
    // - bulto: true
    // - origenCarga: 'cotizacion'
    // 
    // Y establecer: modoBloqueoGrilla = 'cotizacion';
    
    console.log('⚠️ TODO: Implementar modal de búsqueda de cotizaciones');
    mostrarMensajeAdvertencia('Funcionalidad en desarrollo');
}

/**
 * ✅ ACTUALIZADO v5.0: Carga el último detalle registrado
 * RESTRICCIÓN: Solo si la grilla está vacía
 * COMPORTAMIENTO: Ignora silenciosamente productos con error
 */
function cargarUltimoDetalle() {
    console.log('═══════════════════════════════════════════════════');
    console.log('🕒 CARGAR ÚLTIMO DETALLE');
    console.log('═══════════════════════════════════════════════════');
    
    // ❶ Validar que la grilla esté vacía
    if (productosFactura.length > 0) {
        console.warn('⚠️ Grilla no vacía - No se puede cargar último detalle');
        mostrarMensajeError(
            'No se puede cargar el último detalle.\n\n' +
            'La grilla debe estar vacía. Por favor, elimine los productos actuales.'
        );
        return;
    }
    
    // TODO: Implementar carga de último detalle
    // Debe obtener el último detalle guardado en sesión o BD
    // y llamar a procesarRespuestaProducto con origenCarga = 'ultimo'
    
    console.log('⚠️ TODO: Implementar carga de último detalle');
    mostrarMensajeAdvertencia('Funcionalidad en desarrollo');
}

// ═══════════════════════════════════════════════════════════════════
// SECCIÓN 5: GESTIÓN DE MODAL Y CLIENTE
// ═══════════════════════════════════════════════════════════════════

/**
 * ✅ CORREGIDO v3.2: Muestra el modal de productos
 */
function mostrarSeccionProductos(clienteData) {
    console.log('═══════════════════════════════════════════════════');
    console.log('📦 MOSTRAR MODAL DE PRODUCTOS v3.2');
    console.log('═══════════════════════════════════════════════════');
    console.log('Cliente recibido:', clienteData);
    
    // ❶ Hidratar datos del cliente en el modal
    $('#txtClienteNombreProd').val(clienteData.denominacion || '');
    $('#txtClienteIdProd').val(clienteData.id || 'N/A');
    $('#txtClienteDomicilioProd').val(clienteData.domicilio || '');
    $('#txtCondicionAfipProd').val(clienteData.condicionAfip || '');
    $('#txtClienteCuitProd').val(clienteData.tipoNumero || '');
    $('#txtClienteEmailProd').val(clienteData.email || '');
    $('#txtClienteMovilProd').val(clienteData.movil || '');
    
    console.log('✅ Datos hidratados correctamente');
    
    // ❷ Actualizar badge de tipo de comprobante
    actualizarTipoComprobante(clienteData);
    
    // ❸ Abrir modal
    $('#modalProductosFactura').modal('show');
    console.log('✅ Modal de productos abierto');
    
    // ❹ Focus en campo de búsqueda
    $('#modalProductosFactura').on('shown.bs.modal', function () {
        setTimeout(() => {
            $('#txtCodigoProducto').trigger('focus');
        }, 200);
    });
    
    console.log('═══════════════════════════════════════════════════');
    console.log('✅ MODAL DE PRODUCTOS MOSTRADO CORRECTAMENTE v3.2');
    console.log('═══════════════════════════════════════════════════');
}

/**
 * ✅ CORREGIDO v2.0: Actualiza el badge de tipo de comprobante
 */
function actualizarTipoComprobante(clienteData) {
    const $badge = $('#badgeTipoComprobante');
    
    let tipoFactura = 'FACTURA B';
    let iconoFactura = 'bx-file';
    
    // ❶ Prioridad 1: Usar clienteData.emite
    if (clienteData.emite && clienteData.emite.trim() !== '') {
        tipoFactura = clienteData.emite.toUpperCase();
        
        if (tipoFactura.includes('FACTURA A')) {
            iconoFactura = 'bx-file-blank';
        } else if (tipoFactura.includes('FACTURA C')) {
            iconoFactura = 'bx-file';
        } else if (tipoFactura.includes('NOTA')) {
            iconoFactura = 'bx-receipt';
        }
    } 
    // ❷ Prioridad 2: Calcular desde condicionAfip
    else if (clienteData.condicionAfip) {
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

/**
 * ✅ CORREGIDO v3.1: Oculta el modal de productos
 */
function ocultarSeccionProductos() {
    console.log('═══════════════════════════════════════════════════');
    console.log('🔙 OCULTAR MODAL DE PRODUCTOS v3.1');
    console.log('═══════════════════════════════════════════════════');
    
    // ❶ Limpiar campos de cliente
    $('#txtClienteNombreProd').val('');
    $('#txtClienteIdProd').val('');
    $('#txtClienteDomicilioProd').val('');
    $('#txtCondicionAfipProd').val('');
    $('#txtClienteCuitProd').val('');
    $('#txtClienteEmailProd').val('');
    $('#txtClienteMovilProd').val('');
    
    // ❷ Limpiar campo de búsqueda
    $('#txtCodigoProducto').val('');
    
    // ❸ Limpiar grilla y estado
    limpiarGrillaProductos();
    clienteActualFactura = null;
    modoBloqueoGrilla = null;
    
    // ❹ Cerrar modal
    $('#modalProductosFactura').modal('hide');
    
    console.log('✅ Modal de productos ocultado correctamente');
}

/**
 * ✅ ACTUALIZADO: Limpia la grilla de productos
 * CORREGIDO: Restaurar estado inicial del mensaje
 */
function limpiarGrillaProductos() {
    console.log('🧹 Limpiando grilla de productos...');
    
    productosFactura = [];
    totalFactura = 0;
    modoBloqueoGrilla = null;
    
    $('#tbodyProductos').html(`
        <tr id="rowSinProductos" class="compact-row">
            <td colspan="8" class="text-center text-muted py-4">
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
    
    // ✅ CORREGIDO: Restaurar estado inicial del mensaje
    $('#mensajeEstadoProducto')
        .removeClass('text-danger text-success text-info')  // ✅ Eliminar todos los colores
        .addClass('text-muted')  // ✅ Restaurar text-muted
        .html('Presione <kbd>Enter</kbd> o <strong>BUSCAR</strong> para agregar producto');
    
    console.log('✅ Grilla limpiada');
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

/**
 * Confirma la factura
 */
function confirmarFactura() {
    console.log('✅ Confirmando factura...');
    
    if (productosFactura.length === 0) {
        mostrarMensajeError('Debe cargar al menos un producto para continuar');
        return;
    }
    
    console.log('⚠️ TODO: Implementar confirmación de factura');
    // TODO: Continuar con el flujo (pago, impresión, etc.)
}

// ═══════════════════════════════════════════════════════════════════
// SECCIÓN 7: FUNCIONES AUXILIARES
// ═══════════════════════════════════════════════════════════════════

/**
 * Formatea un número con separadores de miles
 */
function formatearNumero(numero, decimales = 0) {
    if (isNaN(numero)) return '0';
    
    return numero.toLocaleString('es-AR', {
        minimumFractionDigits: decimales,
        maximumFractionDigits: decimales
    });
}

/**
 * Escapa caracteres HTML para prevenir XSS
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
    
    return texto.replace(/[&<>"']/g, m => map[m]);
}

/**
 * Muestra mensaje de error
 */
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

/**
 * Muestra mensaje de éxito
 */
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

/**
 * ✅ NUEVO v4.0: Muestra mensaje de advertencia
 */
function mostrarMensajeAdvertencia(mensaje) {
    console.warn('💬 Advertencia:', mensaje);
    
    AbrirMensaje(
        "Advertencia",
        mensaje,
        function () {
            $("#msjModal").modal("hide");
        },
        false,
        ["Aceptar"],
        "warning",
        null
    );
}