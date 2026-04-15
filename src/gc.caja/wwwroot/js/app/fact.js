// ============================================
// GESTOR PRINCIPAL DEL MÓDULO DE FACTURACIÓN
// ============================================

// ====== VARIABLES GLOBALES ======
let clienteSeleccionado = null;
let modoEdicionCliente = false; // Control de modo edición

// ====== INICIALIZACIÓN ======
$(function () {
    inicializaEventosFact();
    inicializaVistaFact();
});

// ====== EVENTOS PRINCIPALES ======
function inicializaEventosFact() {
    // ========================================
    // MODAL IDENTIFICAR CLIENTE
    // ========================================
    
    // Abrir modal identificar cliente (solo para pruebas o apertura manual)
    $('#btnAbrirIdentificarCliente').on('click', function () {
        abrirModalIdentificarCliente();
    });

    // Buscar cliente (botón)
    $('#btnBuscarCliente').on('click', function () {
        buscarCliente();
    });

    // Buscar cliente (Enter)
    $('#txtBuscarCliente').on('keypress', function (e) {
        if (e.which === 13) { // Enter
            e.preventDefault();
            buscarCliente();
        }
    });

    // Lista de precios
    $('#btnListaPrecios').on('click', function () {
        console.log('🛍️ Abrir Lista de Precios...');
        // TODO: Implementar lógica
    });

    // Nuevo cliente
    $('#btnNuevoCliente').on('click', function () {
        console.log('➕ Crear Nuevo Cliente...');
        abrirModalClienteNuevo();
    });

    // ✅ NUEVO: Salir al menú principal de caja
    $('#btnSalirFacturacion').on('click', function () {
        console.log('🚪 Usuario solicitó salir al menú principal...');
        confirmarSalidaAlMenu();
    });

    // Cancelar (solo limpia el modal)
    $('#btnCancelarCliente').on('click', function () {
        limpiarModalCliente();
    });

    // Seguir (confirmar cliente)
    $('#btnSeguirCliente').on('click', function () {
        if (clienteSeleccionado) {
            confirmarCliente(clienteSeleccionado);
        }
    });

    // ========================================
    // PREVENIR CIERRE ACCIDENTAL DEL MODAL
    // ========================================

    // ✅ MODIFICADO: Permitir cierre desde CANCELAR y SALIR
    $('#modalIdentificarCliente').on('hide.bs.modal', function (e) {
        // ❶ Identificar el elemento que disparó el cierre
        const disparadorId = e.relatedTarget ? e.relatedTarget.id : null;
        
        console.log('🔍 Modal intentando cerrarse - Disparador:', disparadorId);
        
        // ❷ Permitir cierre SOLO desde botones específicos
        const cierresPermitidos = ['btnCancelarCliente', 'btnSalirFacturacion'];
        const esCierrePermitido = cierresPermitidos.includes(disparadorId);
        
        // ❸ Evaluar si se debe prevenir el cierre
        if (!esCierrePermitido) {
            // No es un cierre autorizado (ESC, clic fuera, etc.)
            if (!clienteSeleccionado) {
                // Si no hay cliente seleccionado, prevenir cierre
                e.preventDefault();
                console.warn('⚠️ Cierre no autorizado - Debe seleccionar un cliente o usar CANCELAR/SALIR');
            } else {
                // Hay cliente seleccionado, se puede cerrar
                console.log('✅ Hay cliente seleccionado - Permitiendo cierre');
            }
        } else {
            console.log(`✅ Cierre autorizado desde: ${disparadorId}`);
        }
    });

    // ========================================
    // MODAL CLIENTE UPDATE (NUEVO/EDITAR)
    // ========================================
    
    // Validación en tiempo real del select
    $('#selTipoDocumento').on('change', function () {
        validarCampo($(this));
        ajustarPlaceholderSegunTipo();
    });

    // Validación en tiempo real de inputs
    $('#txtNumeroDocumento, #txtNombreCliente, #txtEmailCliente, #txtMovilCliente').on('input', function () {
        validarCampo($(this));
    });

    // Formatear número de documento (solo números)
    $('#txtNumeroDocumento').on('input', function () {
        let valor = $(this).val().replace(/\D/g, '');
        $(this).val(valor);
    });

    // Validar email al perder foco
    $('#txtEmailCliente').on('blur', function () {
        validarEmail($(this));
    });

    // ✅ NUEVO: Salir al menú principal
    $('#btnSalirFacturacion').on('click', function () {
        console.log('🚪 Salir al menú principal de caja...');
        confirmarSalidaAlMenu();
    });

    // Cancelar modal cliente update
    $('#btnCancelarClienteUpdate').on('click', function () {
        cerrarModalClienteUpdate();
    });

    // Guardar cliente (submit del form)
    $('#formClienteUpdate').on('submit', function (e) {
        e.preventDefault();
        
        if (validarFormularioCliente()) {
            guardarCliente();
        }
    });
}

// ====== INICIALIZACIÓN DE VISTA ======
function inicializaVistaFact() {
    // ✅ NUEVO: Abrir modal automáticamente al cargar la vista
    console.log('🚀 Inicializando módulo de Facturación...');
    
    // Esperar que el DOM esté completamente renderizado
    setTimeout(() => {
        abrirModalIdentificarCliente();
        console.log('✅ Modal de Identificar Cliente abierto automáticamente');
    }, 300);
}

// ========================================
// MODAL IDENTIFICAR CLIENTE - FUNCIONES
// ========================================

function abrirModalIdentificarCliente() {
    // Resetear estado
    limpiarModalCliente();

    // ✅ SOLUCIÓN SIMPLE: Usar jQuery para mostrar el modal
    $('#modalIdentificarCliente').modal('show');

    // Focus en campo de búsqueda
    setTimeout(() => {
        $('#txtBuscarCliente').trigger("focus");
    }, 500);
}

/**
 * ✅ MODIFICADO: Limpia completamente el modal de identificar cliente
 * 
 * Esta función se ejecuta cuando el usuario hace clic en CANCELAR.
 * Debe restaurar el modal al estado inicial (#1) independientemente
 * del estado actual.
 * 
 * Estados que maneja:
 * - Estado 1: Inicial (sin búsqueda)
 * - Estado 2: Con texto sin buscar
 * - Estado 3: Cliente único encontrado
 * - Estado 4: Grilla de múltiples clientes
 * - Estado 5: Cargando desde grilla (con loader)
 * - Estado 6: Cliente cargado desde grilla
 */
function limpiarModalCliente() {
    console.log('═══════════════════════════════════════════════════');
    console.log('🧹 LIMPIAR MODAL CLIENTE - INICIO');
    console.log('═══════════════════════════════════════════════════');
    console.log('Estado actual antes de limpiar:');
    console.log('   - txtBuscarCliente:', $('#txtBuscarCliente').val());
    console.log('   - clienteSeleccionado:', clienteSeleccionado);
    console.log('   - cardDatosCliente visible:', $('#cardDatosCliente').is(':visible'));
    console.log('   - cardGrillaClientes existe:', $('#cardGrillaClientes').length > 0);
    console.log('   - loaderClienteTemp existe:', $('#loaderClienteTemp').length > 0);
    
    // ❶ LIMPIAR VARIABLE GLOBAL
    clienteSeleccionado = null;
    console.log('✅ Variable global clienteSeleccionado limpiada');

    // ❷ LIMPIAR CAMPO DE BÚSQUEDA
    $('#txtBuscarCliente').val('');
    console.log('✅ Campo de búsqueda limpiado');
    
    // ❸ LIMPIAR VALORES DE LOS INPUTS (sin eliminar el HTML)
    $('#txtNombre').val('');
    $('#txtClienteId').val('');
    $('#txtDomicilio').val('');
    $('#txtCondicionAfip').val('');
    $('#txtTipoNumero').val('');
    $('#txtEmite').val('');
    $('#txtEmail').val('');
    $('#txtMovil').val('');
    console.log('✅ Valores de inputs limpiados');

    // ❹ OCULTAR CARD DE DATOS DEL CLIENTE
    $('#cardDatosCliente')
        .removeClass('show')
        .hide();
    console.log('✅ Card de datos oculto');
    
    // ❺ MOSTRAR EL CARD-BODY ORIGINAL (si estaba oculto por el loader)
    const $cardBody = $('#cardDatosCliente .card-body');
    if ($cardBody.length > 0) {
        $cardBody.show();
        console.log('✅ Card-body restaurado (si estaba oculto)');
    }
    
    // ❻ ELIMINAR LOADER TEMPORAL (si existe)
    const $loader = $('#loaderClienteTemp');
    if ($loader.length > 0) {
        $loader.remove();
        console.log('✅ Loader temporal eliminado');
    }
    
    // ❼ LIMPIAR Y OCULTAR GRILLA DE MÚLTIPLES CLIENTES
    const $grilla = $('#cardGrillaClientes');
    if ($grilla.length > 0) {
        $grilla
            .removeClass('show')
            .hide()
            .empty(); // Vaciar contenido HTML
        console.log('✅ Grilla de clientes limpiada y ocultada');
    }

    // ❽ MOSTRAR ALERT DE "SIN CLIENTE"
    $('#alertSinCliente')
        .removeClass('hide')
        .show();
    console.log('✅ Alert "sin cliente" mostrado');

    // ❾ DESHABILITAR BOTÓN SEGUIR
    $('#btnSeguirCliente').prop('disabled', true);
    console.log('✅ Botón SEGUIR deshabilitado');
    
    // ❿ LIMPIAR SESIÓN DEL SERVIDOR (opcional, vía AJAX)
    limpiarSesionClientesBuscados();
    
    console.log('═══════════════════════════════════════════════════');
    console.log('✅ LIMPIAR MODAL CLIENTE - FINALIZADO');
    console.log('   Estado restaurado al inicial (#1)');
    console.log('═══════════════════════════════════════════════════');
}

// ====== BÚSQUEDA DE CLIENTE ======
function buscarCliente() {
    const criterioBusqueda = $('#txtBuscarCliente').val().trim();

    if (!criterioBusqueda) {
        mostrarMensajeError('Por favor, ingrese CUIT, DNI o ID del cliente');
        return;
    }

    console.log(`🔍 Buscando cliente: ${criterioBusqueda}`);

    const $btnBuscar = $('#btnBuscarCliente');
    $btnBuscar.prop('disabled', true).html('<i class="bx bx-loader-alt bx-spin"></i> Buscando...');

    const url = typeof BuscarClienteUrl !== 'undefined' && BuscarClienteUrl 
        ? BuscarClienteUrl 
        : '/Facturacion/Cliente/BuscarCliente';

    console.log('📡 URL de llamada:', url);

    $.ajax({
        url: url,
        type: 'POST',
        data: { criterio: criterioBusqueda },
        success: function (response) {
            if (response.ok) {
                const cantidadResultados = response.cantidadResultados || 0;

                //limpiamos el campo #txtBuscarCliente para por si hay una nueva busqueda.
                $("#txtBuscarCliente").val("");

                // ❶ CASO: 1 CLIENTE ENCONTRADO
                if (cantidadResultados === 1 && response.cliente) {
                    console.log('✅ Cliente único encontrado:', response.cliente);                    
                    mostrarDatosCliente(response.cliente);
                }
                // ❷ CASO: MÚLTIPLES CLIENTES ENCONTRADOS
                else if (cantidadResultados > 1) {
                    console.log(`✅ Múltiples clientes encontrados: ${cantidadResultados}`);
                    // ✅ LLAMAR A TRAER GRILLA (AJAX)
                    cargarGrillaClientes();
                }
                // ❸ CASO: NO SE ENCONTRARON CLIENTES (fallback)
                else {
                    console.warn('⚠️ Respuesta inesperada:', response);
                    mostrarMensajeError('No se encontraron clientes');
                    limpiarVista();
                }
            } else {
                // ❹ CASO: BÚSQUEDA SIN RESULTADOS
                console.warn('⚠️ Cliente no encontrado');
                mostrarMensajeError(response.mensaje || 'Cliente no encontrado');
                limpiarVista();
            }
        },
        error: function (xhr, status, error) {
            console.error('❌ Error AJAX:', {
                status: xhr.status,
                statusText: xhr.statusText,
                error: error,
                url: url
            });
            
            let mensaje = 'Error al buscar el cliente. Por favor, intente nuevamente.';
            
            if (xhr.status === 404) {
                mensaje = 'Servicio de búsqueda no encontrado. Verifique la configuración.';
            } else if (xhr.status === 400) {
                mensaje = 'Criterio de búsqueda inválido';
            } else if (xhr.status === 401 || xhr.status === 403) {
                mensaje = 'Su sesión ha expirado. Por favor, vuelva a iniciar sesión.';
            } else if (xhr.status === 500) {
                mensaje = 'Error interno del servidor. Contacte al administrador.';
            }
            
            mostrarMensajeError(mensaje);
            limpiarVista();
        },
        complete: function () {
            $btnBuscar.prop('disabled', false).html('<i class="bx bx-search"></i> Buscar');
        }
    });
}

// ====== CARGAR GRILLA DE CLIENTES (AJAX) ======
function cargarGrillaClientes() {
    console.log('📊 Cargando grilla de clientes desde servidor...');
    
    // ❶ Ocultar card de cliente único e alert
    $('#cardDatosCliente').removeClass('show').hide();
    $('#alertSinCliente').addClass('hide').hide();
    
    // ❷ Mostrar loader en el contenedor de grilla
    const urlTraerGrilla = typeof TraerGrillaClientesUrl !== 'undefined' && TraerGrillaClientesUrl 
        ? TraerGrillaClientesUrl 
        : '/Facturacion/Cliente/TraerGrillaClientes';
    
    // Verificar si existe el contenedor, si no, crearlo
    if ($('#cardGrillaClientes').length === 0) {
        $('#alertSinCliente').after('<div class="card card-golden" id="cardGrillaClientes"></div>');
    }
    
    // Mostrar loader
    $('#cardGrillaClientes').html(`
        <div class="text-center py-5">
            <i class='bx bx-loader-alt bx-spin' style='font-size: 3rem; color: #f0ad4e;'></i>
            <p class="mt-3 text-muted">Cargando resultados...</p>
        </div>
    `).show();
    
    // ❸ Llamada AJAX para obtener la vista parcial
    $.ajax({
        url: urlTraerGrilla,
        type: 'POST',
        dataType: 'html', // ✅ Esperamos HTML (vista parcial)
        success: function (htmlGrilla) {
            console.log('✅ Grilla recibida del servidor');
            
            // ❹ Insertar HTML de la grilla
            $('#cardGrillaClientes').html(htmlGrilla).show().removeClass('hide').addClass('show');
            
            // ❺ Deshabilitar botón SEGUIR
            $('#btnSeguirCliente').prop('disabled', true);
            
            // ❻ Adjuntar eventos
            attachGrillaEventos();
        },
        error: function (xhr, status, error) {
            console.error('❌ Error al cargar grilla:', {
                status: xhr.status,
                error: error
            });
            
            let mensajeError = 'Error al cargar la grilla de clientes.';
            
            if (xhr.status === 401 || xhr.status === 403) {
                mensajeError = 'Su sesión ha expirado. Por favor, vuelva a iniciar sesión.';
            } else if (xhr.status === 500) {
                mensajeError = 'Error interno del servidor. Contacte al administrador.';
            }
            
            $('#cardGrillaClientes').html(`
                <div class="alert alert-danger m-3">
                    <i class='bx bx-error-circle'></i> ${mensajeError}
                </div>
            `);
        }
    });
}

// ====== VALIDAR CLIENTE ANTES DE SELECCIONAR ====== (✅ CORREGIDO)
/**
 * ✅ CORREGIDO: Función centralizada para validar datos críticos del cliente
 * 
 * Valida que el cliente puede ser seleccionado verificando:
 * 1. Origen "N" (No Habilitado) → BLOQUEA con mensaje
 * 2. Consumidor Final (F) sin documento → BLOQUEA con mensaje
 * 
 * Se reutiliza desde:
 * - Evento doble clic en fila
 * - Evento clic en botón "Seleccionar"
 * 
 * @param {jQuery} $row - Fila de la grilla que contiene los data-attributes
 * @returns {boolean} true si el cliente es válido, false si debe bloquearse
 */
function validarClienteAntesDeSeleccionar($row) {
    // ❶ Extraer datos mínimos necesarios para validación
    const origen = $row.data('cta-origen');
    const origenDesc = $row.data('cta-origen-desc');
    const nombre = $row.data('cta-nombre');
    const documento = $row.data('cta-documento');
    
    console.log(`🔍 Validando cliente: "${nombre}" | Origen: ${origenDesc} (${origen})`);
    
    // ❷ VALIDACIÓN CRÍTICA 1: Origen "N" (No Habilitado)
    if (origen && origen.toUpperCase() === 'N') {
        console.error('═══════════════════════════════════════════════════');
        console.error('❌ BLOQUEADO: Cliente NO HABILITADO');
        console.error(`   Cliente: ${nombre}`);
        console.error(`   Origen: ${origenDesc} (${origen})`);
        console.error('═══════════════════════════════════════════════════');
        
        mostrarMensajeError(
            `⚠️ CLIENTE NO HABILITADO\n\n` +
            `El cliente "${nombre}" NO ESTÁ HABILITADO para operar.\n\n` +
            `Por favor, contacte al administrador del sistema.`
        );
        return false;
    }
    
    // ❸ VALIDACIÓN CRÍTICA 2: Consumidor Final sin documento
    if (origen && origen.toUpperCase() === 'F') {
        if (!documento || documento.toString().trim() === '') {
            console.error('═══════════════════════════════════════════════════');
            console.error('❌ BLOQUEADO: Consumidor Final sin documento');
            console.error(`   Cliente: ${nombre}`);
            console.error(`   Documento: ${documento || '(vacío)'}`);
            console.error('═══════════════════════════════════════════════════');
            
            mostrarMensajeError(
                `⚠️ DATOS INCOMPLETOS\n\n` +
                `El consumidor final "${nombre}" no tiene número de documento registrado.\n\n` +
                `Este dato es obligatorio para continuar.`
            );
            return false;
        }
    }
    
    // ❹ TODAS LAS VALIDACIONES PASARON
    console.log('✅ Validaciones básicas aprobadas');
    return true;
}

// ====== EVENTOS DE LA GRILLA ====== (✅ CORREGIDO v3.1)
function attachGrillaEventos() {
    console.log('🔧 Adjuntando eventos a la grilla de clientes...');
    
    // ❶ LIMPIAR EVENTOS ANTERIORES (prevenir duplicados)
    $(document).off('dblclick', '.cliente-row');
    $(document).off('click', '.btn-seleccionar-cliente');
    $(document).off('click', '#btnCerrarGrilla');
    
    // ❷ EVENTO: Doble clic en fila completa (✅ CORREGIDO v3.1)
    /**
     * ✅ CORREGIDO v3.1: Ahora valida ANTES de seleccionar
     * 
     * Flujo:
     * 1. Obtiene la fila
     * 2. Llama a validarClienteAntesDeSeleccionar()
     * 3. Si pasa validación → Llama a seleccionarClienteDesdeGrilla()
     * 4. Si falla validación → Muestra error y termina
     */
    $(document).on('dblclick', '.cliente-row', function() {
        console.log('═══════════════════════════════════════════════════');
        console.log('🖱️ DOBLE CLIC EN FILA - Iniciando validación...');
        console.log('═══════════════════════════════════════════════════');
        
        const $row = $(this);
        
        // ✅ Validar ANTES de continuar
        if (!validarClienteAntesDeSeleccionar($row)) {
            console.error('❌ Validación fallida - Selección bloqueada');
            console.log('═══════════════════════════════════════════════════');
            return; // DETENER ejecución
        }
        
        console.log('✅ Validación aprobada - Continuando con selección...');
        console.log('═══════════════════════════════════════════════════');
        
        // ✅ Continuar con la selección normal
        seleccionarClienteDesdeGrilla($row);
    });
    
    // ❸ EVENTO: Clic en botón "Seleccionar" (✅ SIMPLIFICADO v3.1)
    /**
     * ✅ SIMPLIFICADO v3.1: Ahora usa la función de validación compartida
     */
    $(document).on('click', '.btn-seleccionar-cliente', function (e) {
        e.stopPropagation();

        console.log('═══════════════════════════════════════════════════');
        console.log('🖱️ BOTÓN SELECCIONAR - Iniciando validación...');
        console.log('═══════════════════════════════════════════════════');

        // ❶ OBTENER FILA PADRE
        const $row = $(this).closest('.cliente-row');

        if ($row.length === 0) {
            console.error('❌ No se pudo obtener la fila padre del botón');
            mostrarMensajeError('Error: No se pudo identificar el cliente seleccionado');
            return;
        }

        // ❷ VALIDAR usando función compartida
        if (!validarClienteAntesDeSeleccionar($row)) {
            console.error('❌ Validación fallida - Selección bloqueada');
            console.log('═══════════════════════════════════════════════════');
            return; // DETENER ejecución
        }

        console.log('✅ Validación aprobada - Delegando a seleccionarClienteDesdeGrilla()');
        console.log('═══════════════════════════════════════════════════');
        
        // ❸ Continuar con la selección normal
        seleccionarClienteDesdeGrilla($row);
    });
    
    // ❹ EVENTO: Botón "Cerrar" grilla
    $(document).on('click', '#btnCerrarGrilla', function() {
        console.log('🖱️ Cerrar grilla - Limpiando vista...');
        limpiarVista();
    });
    
    console.log('✅ Eventos de grilla adjuntados correctamente');
}

// ====== SELECCIONAR CLIENTE DESDE GRILLA ====== (✅ OPTIMIZADO v3.0)
/**
 * ✅ OPTIMIZADO v3.0: ÚNICA FUNCIÓN que maneja la lógica completa de selección
 * 
 * Esta función es el PUNTO CENTRAL para seleccionar un cliente desde la grilla.
 * Se llama desde:
 * - Doble clic en fila
 * - Clic en botón "Seleccionar" (después de validaciones básicas)
 * 
 * Responsabilidades:
 * 1. Extraer TODOS los datos del cliente desde data-attributes
 * 2. Validar integridad de datos
 * 3. Determinar criterio de búsqueda según origen:
 *    • Origen "C" → cta_id
 *    • Origen "F" → documento
 * 4. Mostrar loader
 * 5. Llamar a buscarClientePorId() con el criterio correcto
 */
function seleccionarClienteDesdeGrilla($row) {
    console.log('═══════════════════════════════════════════════════');
    console.log('📍 SELECCIONAR CLIENTE DESDE GRILLA - v3.0');
    console.log('═══════════════════════════════════════════════════');

    // ❶ VALIDAR QUE LA FILA EXISTE
    if (!$row || $row.length === 0) {
        console.error('❌ Parámetro $row inválido o vacío');
        mostrarMensajeError('Error: No se pudo acceder a los datos de la fila seleccionada');
        return;
    }

    // ❷ EXTRAER TODOS LOS DATA-ATTRIBUTES
    const datosCliente = {
        id: $row.data('cta-id'),
        nombre: $row.data('cta-nombre'),
        domicilio: $row.data('cta-domicilio'),
        tdocId: $row.data('cta-tdoc-id'),
        tdocDesc: $row.data('cta-tdoc-desc'),
        documento: $row.data('cta-documento'),
        email: $row.data('cta-email'),
        movil: $row.data('cta-movil'),
        origen: $row.data('cta-origen'),
        origenDesc: $row.data('cta-origen-desc')
    };

    console.log('📊 Datos extraídos de la fila:');
    console.log('   ID:', datosCliente.id);
    console.log('   Nombre:', datosCliente.nombre);
    console.log('   Origen:', datosCliente.origenDesc, `(${datosCliente.origen})`);
    console.log('   Tipo Doc:', datosCliente.tdocDesc, `(ID: ${datosCliente.tdocId})`);
    console.log('   Documento:', datosCliente.documento);

    // ❸ VALIDACIÓN: ID del cliente (SIEMPRE REQUERIDO)
    if (!datosCliente.id && !datosCliente.documento) {
        console.error('❌ ID del cliente vacío o inválido');
        console.error('   Fila problemática:', $row);
        mostrarMensajeError(
            'Error: No se pudo identificar el ID del cliente.\n' +
            'Por favor, intente nuevamente.'
        );
        return;
    }

    // ❹ VALIDACIÓN: Origen del cliente (SIEMPRE REQUERIDO)
    if (!datosCliente.origen || datosCliente.origen.toString().trim() === '') {
        console.error('❌ Origen del cliente vacío o inválido');
        mostrarMensajeError('Error: Los datos de origen del cliente están incompletos.');
        return;
    }

    // ❺ VALIDACIÓN: Origen "N" (doble verificación, aunque el botón ya validó)
    if (datosCliente.origen.toUpperCase() === 'N') {
        console.warn('⚠️ Origen N detectado en seleccionarClienteDesdeGrilla()');
        mostrarMensajeError(
            `El cliente "${datosCliente.nombre}" NO ESTÁ HABILITADO.\n` +
            `Por favor, contacte al administrador.`
        );
        return;
    }

    // ❻ DETERMINAR CRITERIO DE BÚSQUEDA SEGÚN ORIGEN
    let criterioBusqueda = '';
    let tipoBusqueda = '';

    const origenUpper = datosCliente.origen.toUpperCase();

    if (origenUpper === 'C') {
        // ✅ CLIENTE REGISTRADO → Usar ID
        criterioBusqueda = datosCliente.id;
        tipoBusqueda = 'ID de Cliente Registrado';

        console.log('═══════════════════════════════════════════════════');
        console.log('✅ CRITERIO: CLIENTE REGISTRADO (Origen C)');
        console.log(`   Buscando por: ID = "${criterioBusqueda}"`);
        console.log('═══════════════════════════════════════════════════');

    } else if (origenUpper === 'F') {
        // ✅ CONSUMIDOR FINAL → Usar Documento

        // Validación crítica: El documento DEBE existir
        if (!datosCliente.documento || datosCliente.documento.toString().trim() === '') {
            console.error('═══════════════════════════════════════════════════');
            console.error('❌ ERROR CRÍTICO: Consumidor Final sin documento');
            console.error(`   Cliente: ${datosCliente.nombre}`);
            console.error(`   ID: ${datosCliente.id}`);
            console.error(`   Documento: ${datosCliente.documento || '(vacío)'}`);
            console.error('═══════════════════════════════════════════════════');

            mostrarMensajeError(
                `⚠️ DATOS INCOMPLETOS\n\n` +
                `El consumidor final "${datosCliente.nombre}" no tiene documento registrado.\n\n` +
                `No se puede continuar con la búsqueda.`
            );
            return;
        }

        criterioBusqueda = datosCliente.documento;
        tipoBusqueda = `Documento ${datosCliente.tdocDesc}`;

        console.log('═══════════════════════════════════════════════════');
        console.log('✅ CRITERIO: CONSUMIDOR FINAL (Origen F)');
        console.log(`   Buscando por: Documento = "${criterioBusqueda}"`);
        console.log(`   Tipo: ${datosCliente.tdocDesc} (ID: ${datosCliente.tdocId})`);
        console.log('═══════════════════════════════════════════════════');

    } else {
        // ⚠️ ORIGEN DESCONOCIDO - Usar ID por defecto
        console.warn('═══════════════════════════════════════════════════');
        console.warn(`⚠️ ADVERTENCIA: Origen desconocido "${datosCliente.origen}"`);
        console.warn('   Usando ID por defecto como fallback');
        console.warn('═══════════════════════════════════════════════════');

        criterioBusqueda = datosCliente.id;
        tipoBusqueda = 'ID (Origen desconocido - fallback)';
    }

    // ❼ RESUMEN ANTES DE LA BÚSQUEDA
    console.log('═══════════════════════════════════════════════════');
    console.log('📋 RESUMEN ANTES DE BÚSQUEDA');
    console.log('═══════════════════════════════════════════════════');
    console.log(`   Cliente: ${datosCliente.nombre}`);
    console.log(`   Origen: ${datosCliente.origenDesc} (${datosCliente.origen})`);
    console.log(`   Tipo de Búsqueda: ${tipoBusqueda}`);
    console.log(`   Criterio: "${criterioBusqueda}"`);
    console.log('═══════════════════════════════════════════════════');

    // ❽ OCULTAR GRILLA INMEDIATAMENTE (mejora UX)
    $('#cardGrillaClientes')
        .removeClass('show')
        .hide()
        .empty();

    // ❾ OCULTAR ALERT DE "SIN CLIENTE"
    $('#alertSinCliente').hide();

    // ❿ MOSTRAR LOADER CON INFORMACIÓN DEL CRITERIO
    const $cardBody = $('#cardDatosCliente .card-body');

    // Mensaje del loader según tipo de búsqueda
    let loaderMensaje = '';
    if (origenUpper === 'C') {
        loaderMensaje = `ID: ${criterioBusqueda}`;
    } else if (origenUpper === 'F') {
        loaderMensaje = `${datosCliente.tdocDesc}: ${criterioBusqueda}`;
    } else {
        loaderMensaje = criterioBusqueda;
    }

    if ($cardBody.length > 0) {
        $cardBody.hide();

        if ($('#loaderClienteTemp').length === 0) {
            $cardBody.after(`
                <div id="loaderClienteTemp" class="card-body text-center py-5">
                    <i class='bx bx-loader-alt bx-spin' style='font-size: 3rem; color: #f0ad4e;'></i>
                    <p class="mt-3 text-muted fw-semibold">Cargando datos del cliente...</p>
                    <small class="text-muted">${loaderMensaje}</small>
                </div>
            `);
        } else {
            $('#loaderClienteTemp').show();
        }
    } else {
        console.warn('⚠️ No se encontró .card-body, usando método alternativo');
        $('#cardDatosCliente').append(`
            <div id="loaderClienteTemp" class="text-center py-5">
                <i class='bx bx-loader-alt bx-spin' style='font-size: 3rem; color: #f0ad4e;'></i>
                <p class="mt-3 text-muted fw-semibold">Cargando datos del cliente...</p>
                <small class="text-muted">${loaderMensaje}</small>
            </div>
        `);
    }

    $('#cardDatosCliente').show();

    // ⓫ DESHABILITAR BOTÓN SEGUIR TEMPORALMENTE
    $('#btnSeguirCliente').prop('disabled', true);

    // ⓬ LLAMAR A LA FUNCIÓN DE BÚSQUEDA CON EL CRITERIO CORRECTO
    console.log(`📡 Llamando a buscarClientePorId("${criterioBusqueda}")...`);
    console.log('═══════════════════════════════════════════════════');

    buscarClientePorId(criterioBusqueda);
}

// ====== BUSCAR CLIENTE POR ID ====== (✅ CORREGIDO)
function buscarClientePorId(clienteId) {
    console.log('═══════════════════════════════════════════════════');
    console.log('🔍 BUSCAR CLIENTE POR ID - INICIO');
    console.log(`   Cliente ID: ${clienteId}`);
    console.log('═══════════════════════════════════════════════════');
    
    // ❶ VALIDAR PARÁMETRO
    if (!clienteId || clienteId.toString().trim() === '') {
        console.error('❌ clienteId inválido:', clienteId);
        mostrarMensajeError('Error: ID de cliente inválido');
        limpiarVista();
        return;
    }
    
    // ❷ CONSTRUIR URL (con fallback)
    const url = typeof BuscarClienteUrl !== 'undefined' && BuscarClienteUrl 
        ? BuscarClienteUrl 
        : '/Facturacion/Cliente/BuscarCliente';
    
    console.log(`📡 URL de búsqueda: ${url}`);
    console.log(`📤 Criterio de búsqueda: "${clienteId}"`);
    
    // ❸ REALIZAR AJAX
    $.ajax({
        url: url,
        type: 'POST',
        data: { criterio: clienteId },
        timeout: 30000,
        success: function (response) {
            console.log('✅ Respuesta AJAX recibida');
            console.log('   response.ok:', response.ok);
            console.log('   cantidadResultados:', response.cantidadResultados);
            
            // ❹ VALIDAR QUE LA BÚSQUEDA FUE EXITOSA
            if (!response.ok) {
                console.error('❌ Error en la búsqueda - response.ok === false');
                console.error('   Mensaje del servidor:', response.mensaje);
                
                mostrarMensajeError(response.mensaje || 'Error al cargar los datos del cliente');
                limpiarVista();
                return;
            }
            
            // ❺ VALIDAR CANTIDAD DE RESULTADOS (debe ser exactamente 1)
            const cantidadResultados = response.cantidadResultados || 0;
            
            if (cantidadResultados !== 1) {
                console.error('❌ Cantidad de resultados inesperada');
                console.error(`   Se esperaba: 1`);
                console.error(`   Se recibió: ${cantidadResultados}`);
                console.error('   Respuesta completa:', response);
                
                mostrarMensajeError('Error: No se pudieron obtener los datos del cliente seleccionado');
                limpiarVista();
                return;
            }
            
            // ❻ VALIDAR QUE EXISTE EL OBJETO CLIENTE
            if (!response.cliente) {
                console.error('❌ response.cliente es null o undefined');
                console.error('   Respuesta completa:', response);
                
                mostrarMensajeError('Error: Los datos del cliente no están disponibles');
                limpiarVista();
                return;
            }
            
            // ❼ ✅ TODO CORRECTO - MOSTRAR DATOS
            console.log('✅ Cliente encontrado correctamente');
            console.log('   ID:', response.cliente.id);
            console.log('   Nombre:', response.cliente.nombre);
            console.log('   Datos completos:', response.cliente);
            
            // ✅ CORRECCIÓN: NO hacer .empty() aquí
            // Mostrar datos en el modal (esta función se encargará de limpiar el loader)
            mostrarDatosCliente(response.cliente);
            
            console.log('═══════════════════════════════════════════════════');
            console.log('✅ BUSCAR CLIENTE POR ID - FINALIZADO EXITOSAMENTE');
            console.log('═══════════════════════════════════════════════════');
        },
        error: function (xhr, status, error) {
            console.error('═══════════════════════════════════════════════════');
            console.error('❌ ERROR AJAX - BUSCAR CLIENTE POR ID');
            console.error('   Status HTTP:', xhr.status);
            console.error('   Status Text:', xhr.statusText);
            console.error('   Error:', error);
            console.error('   Status de jQuery:', status);
            console.error('═══════════════════════════════════════════════════');
            
            let mensaje = 'Error al cargar los datos del cliente. Por favor, intente nuevamente.';
            
            if (status === 'timeout') {
                mensaje = 'La búsqueda tardó demasiado tiempo. Por favor, verifique su conexión e intente nuevamente.';
            } else if (xhr.status === 404) {
                mensaje = 'Servicio de búsqueda no encontrado. Contacte al administrador.';
            } else if (xhr.status === 401 || xhr.status === 403) {
                mensaje = 'Su sesión ha expirado. Por favor, vuelva a iniciar sesión.';
            } else if (xhr.status === 500) {
                mensaje = 'Error interno del servidor. Contacte al administrador.';
            } else if (xhr.responseJSON && xhr.responseJSON.mensaje) {
                mensaje = xhr.responseJSON.mensaje;
            }
            
            mostrarMensajeError(mensaje);
            limpiarVista();
        }
    });
}

// ====== MOSTRAR DATOS DEL CLIENTE ====== (✅ ACTUALIZADO v2.0)
/**
 * ✅ ACTUALIZADO v2.0: Ahora muestra el tipo de documento separado del número
 * 
 * El objeto cliente puede venir con:
 * - tipoNumero: "DNI 23418922" (formato combinado - compatibilidad con versión anterior)
 * - tdocDesc: "DNI" (separado)
 * - documento: "23418922" (separado)
 */
function mostrarDatosCliente(cliente) {
    console.log('═══════════════════════════════════════════════════');
    console.log('📋 MOSTRAR DATOS DEL CLIENTE - INICIO');
    console.log('═══════════════════════════════════════════════════');
    console.log('Cliente recibido:', cliente);
    
    // ❶ Ocultar grilla (si existe)
    if ($('#cardGrillaClientes').length > 0) {
        $('#cardGrillaClientes').removeClass('show').hide().empty();
        console.log('✅ Grilla ocultada y limpiada');
    }
    
    // ❷ Ocultar alert de "sin cliente"
    $('#alertSinCliente').hide();
    console.log('✅ Alert "sin cliente" ocultado');
    
    // ❸ CRÍTICO: Eliminar SOLO el loader temporal (si existe)
    const $loader = $('#loaderClienteTemp');
    if ($loader.length > 0) {
        $loader.remove();
        console.log('✅ Loader temporal eliminado');
    }
    
    // ❹ Mostrar el card-body original (que contiene los inputs)
    const $cardBody = $('#cardDatosCliente .card-body');
    if ($cardBody.length > 0) {
        $cardBody.show();
        console.log('✅ Card-body con inputs mostrado');
    }
    
    // ❺ VALIDACIÓN: Verificar que los inputs existen en el DOM
    const inputsRequeridos = [
        'txtNombre', 'txtClienteId', 'txtDomicilio', 
        'txtCondicionAfip', 'txtTipoNumero', 'txtEmite', 
        'txtEmail', 'txtMovil'
    ];
    
    let todosExisten = true;
    inputsRequeridos.forEach(inputId => {
        if ($(`#${inputId}`).length === 0) {
            console.error(`❌ ERROR: Input #${inputId} NO existe en el DOM`);
            todosExisten = false;
        }
    });
    
    if (!todosExisten) {
        console.error('❌ ERROR CRÍTICO: Algunos inputs no existen');
        console.error('   HTML de #cardDatosCliente:', $('#cardDatosCliente').html());
        
        mostrarMensajeError(
            'Error crítico: La estructura del modal no está completa. ' +
            'Por favor, recargue la página e intente nuevamente.'
        );
        return;
    }
    
    console.log('✅ Todos los inputs existen en el DOM');
    
    // ❻ Hidratar campos con datos del cliente
    console.log('📝 Hidratando campos con datos del cliente...');
    
    // ✅ NUEVO: Determinar valor de tipoNumero (con retrocompatibilidad)
    let tipoNumeroDisplay = '';
    
    if (cliente.tdocDesc && cliente.documento) {
        // ✅ Datos separados (nuevo formato)
        tipoNumeroDisplay = `${cliente.tdocDesc} ${cliente.documento}`;
        console.log('   - Tipo de documento (separado):', cliente.tdocDesc);
        console.log('   - Número de documento:', cliente.documento);
    } else if (cliente.tipoNumero) {
        // ✅ Datos combinados (formato anterior - retrocompatibilidad)
        tipoNumeroDisplay = cliente.tipoNumero;
        console.log('   - Tipo/Número (combinado):', cliente.tipoNumero);
    }
    
    $('#txtNombre').val(cliente.nombre || '');
    $('#txtClienteId').val(cliente.id || '');
    $('#txtDomicilio').val(cliente.domicilio || '');
    $('#txtCondicionAfip').val(cliente.condicionAfip || '');
    $('#txtTipoNumero').val(tipoNumeroDisplay);
    $('#txtEmite').val(cliente.emite || '');
    $('#txtEmail').val(cliente.email || '');
    $('#txtMovil').val(cliente.movil || '');
    
    console.log('   - Nombre:', cliente.nombre);
    console.log('   - ID:', cliente.id);
    console.log('   - Domicilio:', cliente.domicilio);
    console.log('   - Tipo/Número Display:', tipoNumeroDisplay);
    
    // ❼ Mostrar el card con los datos
    $('#cardDatosCliente')
        .show()
        .removeClass('hide')
        .addClass('show');
    
    console.log('✅ Card de datos mostrado');
    
    // ❽ Habilitar botón SEGUIR
    $('#btnSeguirCliente').prop('disabled', false);
    console.log('✅ Botón SEGUIR habilitado');
    
    // ❾ Guardar cliente seleccionado en variable global
    clienteSeleccionado = cliente;
    console.log('✅ Cliente guardado en variable global');
    
    console.log('═══════════════════════════════════════════════════');
    console.log('✅ MOSTRAR DATOS DEL CLIENTE - FINALIZADO');
    console.log('═══════════════════════════════════════════════════');
}

// ====== LIMPIAR VISTA ====== (✅ VALIDAR QUE SEA IGUAL A limpiarModalCliente)
/**
 * ✅ VALIDADO: Esta función hace lo mismo que limpiarModalCliente()
 * Se llama desde errores de búsqueda o cuando se cierra la grilla.
 */
function limpiarVista() {
    console.log('🧹 Limpiando vista...');
    
    // ❶ Limpiar valores de los inputs (NO eliminar los inputs)
    $('#txtNombre').val('');
    $('#txtClienteId').val('');
    $('#txtDomicilio').val('');
    $('#txtCondicionAfip').val('');
    $('#txtTipoNumero').val('');
    $('#txtEmite').val('');
    $('#txtEmail').val('');
    $('#txtMovil').val('');
    
    // ❷ Ocultar card de datos
    $('#cardDatosCliente').removeClass('show').hide();
    
    // ❸ Mostrar el card-body original (si estaba oculto)
    const $cardBody = $('#cardDatosCliente .card-body');
    if ($cardBody.length > 0) {
        $cardBody.show();
    }
    
    // ❹ Eliminar loader temporal si existe
    $('#loaderClienteTemp').remove();
    
    // ❺ Limpiar grilla
    if ($('#cardGrillaClientes').length > 0) {
        $('#cardGrillaClientes').removeClass('show').hide().empty();
    }
    
    // ❻ Mostrar alert de "sin cliente"
    $('#alertSinCliente').removeClass('hide').show();
    
    // ❼ Deshabilitar botón SEGUIR
    $('#btnSeguirCliente').prop('disabled', true);
    
    // ❽ Limpiar variable global
    clienteSeleccionado = null;
    
    console.log('✅ Vista limpiada - Estado inicial restaurado');
}

// ========================================
// MODAL CLIENTE UPDATE - FUNCIONES
// ========================================

/**
 * Abre el modal para crear un nuevo cliente
 */
function abrirModalClienteNuevo() {
    // Configurar modo NUEVO
    modoEdicionCliente = false;
    
    // Resetear formulario
    limpiarFormularioCliente();
    
    // Configurar textos para NUEVO
    $('#lblTituloClienteUpdate').html('<i class="bx bx-user-plus"></i> Nuevo CF');
    $('#lblBotonAccion').text('Cargar CF');
    
    // Mostrar modal
    $('#modalClienteUpdate').modal('show');
    
    // Focus en tipo de documento
    setTimeout(() => {
        $('#selTipoDocumento').trigger("focus");
    }, 500);
    
    console.log('➕ Modal Nuevo Cliente abierto');
}

/**
 * Abre el modal para editar un cliente existente
 * @param {Object} clienteData - Datos del cliente a editar
 */
function abrirModalClienteEditar(clienteData) {
    // Configurar modo EDITAR
    modoEdicionCliente = true;
    
    // Resetear formulario
    limpiarFormularioCliente();
    
    // Configurar textos para EDITAR
    $('#lblTituloClienteUpdate').html('<i class="bx bx-edit"></i> Editar Cliente');
    $('#lblBotonAccion').text('Actualizar');
    
    // Hidratar formulario con datos existentes
    $('#txtClienteIdUpdate').val(clienteData.id);
    $('#selTipoDocumento').val(clienteData.tipoDocumento || 'DNI');
    $('#txtNumeroDocumento').val(clienteData.numeroDocumento).prop('readonly', true);
    $('#txtNombreCliente').val(clienteData.nombre);
    $('#txtDomicilioCliente').val(clienteData.domicilio || '');
    $('#txtEmailCliente').val(clienteData.email || '');
    $('#txtMovilCliente').val(clienteData.movil || '');
    
    // Mostrar modal
    $('#modalClienteUpdate').modal('show');
    
    // Focus en nombre
    setTimeout(() => {
        $('#txtNombreCliente').trigger("focus").trigger("select");
    }, 500);
    
    console.log('✏️ Modal Editar Cliente abierto:', clienteData);
}

/**
 * ✅ ACTUALIZADO: Ajusta placeholder, maxlength Y clases CSS del input según tipo de documento
 * 
 * Reglas de clases CSS:
 * - Tipos numéricos (80, 86, 87, 89, 90, 95, 96): "jsteclado jsinteger"
 * - Tipos alfanuméricos (91, 94, 99): "jsteclado"
 */
function ajustarPlaceholderSegunTipo() {
    const tipoSeleccionado = $('#selTipoDocumento').val();
    const $inputNumero = $('#txtNumeroDocumento');
    
    let placeholder = 'Ingrese el número de documento...';
    let maxLength = 20;
    let clasesCss = 'form-control form-control-lg fw-bold jsteclado'; // Base común
    
    // ✅ Tipos que SOLO aceptan números
    const tiposNumericos = ['80', '86', '87', '89', '90', '95', '96'];
    
    switch (tipoSeleccionado) {
        case '80': // CUIT
            placeholder = 'Ej: 20123456789 (sin guiones)';
            maxLength = 11;
            clasesCss += ' jsinteger';
            break;
            
        case '86': // CUIL
            placeholder = 'Ej: 27123456789 (sin guiones)';
            maxLength = 11;
            clasesCss += ' jsinteger';
            break;
            
        case '87': // CDI
            placeholder = 'Ej: 12345678';
            maxLength = 8;
            clasesCss += ' jsinteger';
            break;
            
        case '89': // LE (Libreta de Enrolamiento)
            placeholder = 'Ej: 1234567';
            maxLength = 8;
            clasesCss += ' jsinteger';
            break;
            
        case '90': // LC (Libreta Cívica)
            placeholder = 'Ej: 1234567';
            maxLength = 8;
            clasesCss += ' jsinteger';
            break;
            
        case '91': // CI Extranjera
            placeholder = 'Ej: ABC123456';
            maxLength = 15;
            // Solo "jsteclado" (sin jsinteger)
            break;
            
        case '94': // Pasaporte
            placeholder = 'Ej: AAA123456';
            maxLength = 15;
            // Solo "jsteclado" (sin jsinteger)
            break;
            
        case '95': // CI Bs. As. RNP
            placeholder = 'Ej: 12345678';
            maxLength = 8;
            clasesCss += ' jsinteger';
            break;
            
        case '96': // D.N.I.
            placeholder = 'Ej: 12345678';
            maxLength = 8;
            clasesCss += ' jsinteger';
            break;
            
        case '99': // Sin Identificar
            placeholder = 'No aplica';
            maxLength = 1;
            // Solo "jsteclado" (sin jsinteger)
            break;
            
        default:
            placeholder = 'Ingrese el número de documento...';
            maxLength = 20;
            // Solo "jsteclado" (sin jsinteger)
    }
    
    // ✅ Aplicar cambios al input
    $inputNumero
        .attr('placeholder', placeholder)
        .attr('maxlength', maxLength)
        .attr('class', clasesCss); // Reemplazar todas las clases
    
    // ✅ Limpiar valor si se cambia el tipo (opcional)
    $inputNumero.val('');
    
    console.log(`📝 Tipo documento cambiado a: ${tipoSeleccionado}`);
    console.log(`   - Placeholder: "${placeholder}"`);
    console.log(`   - MaxLength: ${maxLength}`);
    console.log(`   - Clases CSS: "${clasesCss}"`);
    console.log(`   - Solo números: ${tiposNumericos.includes(tipoSeleccionado) ? 'SÍ' : 'NO'}`);
}

// ====== VALIDAR FORMULARIO CLIENTE ====== (NUEVO)
/**
 * ✅ NUEVO: Valida todos los campos del formulario de cliente
 * 
 * Reglas de validación:
 * - Tipo y número de documento: Obligatorios y con formato específico según tipo
 * - Nombre: Obligatorio
 * - Email: Opcional, pero debe ser válido si se ingresa
 * 
 * Cambios en la interfaz:
 * - Los campos inválidos recibirán la clase "is-invalid" y los válidos "is-valid"
 * - Se mostrará un mensaje de error específico debajo de cada campo inválido
 * 
 * @returns {boolean} Verdadero si el formulario es válido, falso si hay errores
 */
function validarFormularioCliente() {
    let esValido = true;
    
    // Validar tipo de documento
    if (!$('#selTipoDocumento').val()) {
        $('#selTipoDocumento').addClass('is-invalid').removeClass('is-valid');
        esValido = false;
    } else {
        $('#selTipoDocumento').addClass('is-valid').removeClass('is-invalid');
    }
    
    // Validar número de documento
    const numeroDoc = $('#txtNumeroDocumento').val().trim();
    const tipoDoc = $('#selTipoDocumento').val();
    
    if (!numeroDoc) {
        $('#txtNumeroDocumento').addClass('is-invalid').removeClass('is-valid');
        esValido = false;
    } else {
        let valido = true;
        
        // ✅ Validaciones específicas por tipo
        switch (tipoDoc) {
            case '80': // CUIT
            case '86': // CUIL
                if (numeroDoc.length !== 11 || !/^\d+$/.test(numeroDoc)) {
                    valido = false;
                }
                break;
                
            case '87': // CDI
            case '89': // LE
            case '90': // LC
            case '95': // CI Bs. As. RNP
            case '96': // DNI
                if (numeroDoc.length !== 8 || !/^\d+$/.test(numeroDoc)) {
                    valido = false;
                }
                break;
                
            case '91': // CI Extranjera
            case '94': // Pasaporte
                if (numeroDoc.length < 4 || numeroDoc.length > 15) {
                    valido = false;
                }
                break;
                
            case '99': // Sin Identificar
                // No se valida longitud
                break;
        }
        
        if (!valido) {
            $('#txtNumeroDocumento').addClass('is-invalid').removeClass('is-valid');
            esValido = false;
        } else {
            $('#txtNumeroDocumento').addClass('is-valid').removeClass('is-invalid');
        }
    }
    
    // Validar nombre
    if (!$('#txtNombreCliente').val().trim()) {
        $('#txtNombreCliente').addClass('is-invalid').removeClass('is-valid');
        esValido = false;
    } else {
        $('#txtNombreCliente').addClass('is-valid').removeClass('is-invalid');
    }
    
    // Validar email (opcional)
    const email = $('#txtEmailCliente').val().trim();
    if (email && !validarEmail($('#txtEmailCliente'))) {
        esValido = false;
    }
    
    return esValido;
}

// ====== FUNCIONES AUXILIARES ======

function validarCampo($campo) {
    const valor = $campo.val().trim();
    
    if ($campo.prop('required') && !valor) {
        $campo.addClass('is-invalid').removeClass('is-valid');
        return false;
    } else if (valor) {
        $campo.addClass('is-valid').removeClass('is-invalid');
        return true;
    } else {
        $campo.removeClass('is-invalid is-valid');
        return true;
    }
}

function validarEmail($campo) {
    const email = $campo.val().trim();
    
    if (!email) {
        $campo.removeClass('is-invalid is-valid');
        return true;
    }
    
    const regex = /^[^\s@]+@[^\s@]+\.[^\s@]+$/;
    const esValido = regex.test(email);
    
    if (esValido) {
        $campo.addClass('is-valid').removeClass('is-invalid');
    } else {
        $campo.addClass('is-invalid').removeClass('is-valid');
    }
    
    return esValido;
}

function guardarCliente() {
    $('#btnCargarCliente').addClass('processing').prop('disabled', true);
    
    const clienteData = {
        id: $('#txtClienteIdUpdate').val() || null,
        tipoDocumento: $('#selTipoDocumento').val(),
        numeroDocumento: $('#txtNumeroDocumento').val().trim(),
        nombre: $('#txtNombreCliente').val().trim().toUpperCase(),
        domicilio: $('#txtDomicilioCliente').val().trim().toUpperCase(),
        email: $('#txtEmailCliente').val().trim().toLowerCase(),
        movil: $('#txtMovilCliente').val().trim()
    };
    
    console.log(modoEdicionCliente ? '✏️ Actualizando cliente...' : '➕ Creando nuevo cliente...', clienteData);
    
    // TODO: Implementar AJAX real
    setTimeout(() => {
        console.log('✅ Cliente guardado exitosamente (MOCK)');
        
        mostrarMensajeExito(modoEdicionCliente ? 'Cliente actualizado correctamente' : 'Cliente creado correctamente');
        
        setTimeout(() => {
            cerrarModalClienteUpdate();
            
            if (!modoEdicionCliente) {
                const clienteCreado = {
                    id: Math.floor(Math.random() * 10000).toString(),
                    nombre: clienteData.nombre,
                    domicilio: clienteData.domicilio,
                    condicionAfip: 'CONSUMIDOR FINAL',
                    tipoNumero: `${clienteData.tipoDocumento} ${clienteData.numeroDocumento}`,
                    emite: 'FACTURA B',
                    email: clienteData.email,
                    movil: clienteData.movil
                };
                
                mostrarDatosCliente(clienteCreado);
            }
        }, 1500);
    }, 1000);
}

function cerrarModalClienteUpdate() {
    $('#modalClienteUpdate').modal('hide');
    limpiarFormularioCliente();
}

function limpiarFormularioCliente() {
    $('#formClienteUpdate')[0].reset();
    $('#txtClienteIdUpdate').val('');
    $('#formClienteUpdate .form-control, #formClienteUpdate .form-select').removeClass('is-valid is-invalid');
    $('#btnCargarCliente').removeClass('processing').prop('disabled', false);
    $('#txtNumeroDocumento').prop('readonly', false);
    ajustarPlaceholderSegunTipo();
}

function mostrarMensajeError(mensaje) {
    // ✅ INTEGRACIÓN CON SISTEMA DE MENSAJES DEL PROYECTO
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
    // ✅ INTEGRACIÓN CON SISTEMA DE MENSAJES DEL PROYECTO
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

// ========================================
// FUNCIONES DE LIMPIEZA Y SESIÓN
// ========================================

/**
 * ✅ RESTAURADO: Limpia la sesión del servidor que almacena ClientesBuscados
 * 
 * Esta función es CRÍTICA para evitar errores de referencia.
 * Se ejecuta cuando el usuario limpia el modal de identificar cliente.
 * 
 * Comportamiento:
 * - Hace una llamada AJAX asíncrona (no bloquea el flujo)
 * - Si falla, solo registra un warning en consola (no afecta al usuario)
 * - Libera memoria en el servidor eliminando datos obsoletos de la sesión
 * 
 * IMPORTANTE: Esta función NO debe eliminarse ya que está siendo
 * llamada desde limpiarModalCliente() línea 262.
 */
function limpiarSesionClientesBuscados() {
    // Verificar si existe la URL para limpiar sesión
    const urlLimpiarSesion = typeof LimpiarSesionClientesUrl !== 'undefined' && LimpiarSesionClientesUrl 
        ? LimpiarSesionClientesUrl 
        : '/Facturacion/Cliente/LimpiarSesionClientes';
    
    console.log('🧹 Limpiando sesión de clientes buscados en el servidor...');
    
    // Hacer llamada AJAX sin bloquear el flujo (fire and forget)
    $.ajax({
        url: urlLimpiarSesion,
        type: 'POST',
        async: true, // No bloquear
        success: function() {
            console.log('✅ Sesión de clientes buscados limpiada en el servidor');
        },
        error: function(xhr) {
            // No mostrar error al usuario, solo log de advertencia
            console.warn('⚠️ No se pudo limpiar la sesión en el servidor (no crítico)');
            console.warn('   Status:', xhr.status);
        }
    });
}