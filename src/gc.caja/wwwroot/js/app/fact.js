// ============================================
// GESTOR PRINCIPAL DEL MÓDULO DE FACTURACIÓN
// ============================================

// ====== VARIABLES GLOBALES ======
let clienteSeleccionado = null;
let modoEdicionCliente = false; // Control de modo edición

// ========================================
// FUNCIONES DE MENSAJES AL USUARIO
// ========================================

function mostrarMensajeError(mensaje) {
    console.error('💬 Mostrando mensaje de error al usuario');
    console.error(`   Mensaje: "${mensaje}"`);

    // ✅ INTEGRACIÓN CON SISTEMA DE MENSAJES DEL PROYECTO
    AbrirMensaje(
        "Error",
        mensaje,
        function () {
            $("#msjModal").modal("hide");
        },
        false, // No mostrar botón cancelar
        ["Aceptar"],
        "error!", // Tipo de icono
        null
    );
}

function mostrarMensajeExito(mensaje) {
    console.log('💬 Mostrando mensaje de éxito al usuario');
    console.log(`   Mensaje: "${mensaje}"`);

    // ✅ INTEGRACIÓN CON SISTEMA DE MENSAJES DEL PROYECTO
    AbrirMensaje(
        "Éxito",
        mensaje,
        function () {
            $("#msjModal").modal("hide");
        },
        false, // No mostrar botón cancelar
        ["Aceptar"],
        "ok!", // Tipo de icono
        null
    );
}

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

    // ✅ NUEVO: Editar cliente (solo para Consumidores Finales)
    $('#btnEditarCliente').on('click', function () {
        console.log('✏️ Editar Consumidor Final...');
        abrirModalClienteEditar();
    });

    // ════════════════════════════════════════════════════════════════
    // ✅ INTEGRACIÓN CON MÓDULO DE PRODUCTOS DE FACTURACIÓN
    // ════════════════════════════════════════════════════════════════
    // AGREGAR AL FINAL DEL ARCHIVO (después de todas las funciones existentes)
    // NO MODIFICA NINGUNA FUNCIÓN EXISTENTE
    // ════════════════════════════════════════════════════════════════

    /**
     * ✅ NUEVO: Listener para evento de vuelta a identificar cliente
     * Se dispara desde prodfact.js cuando el usuario cancela la factura
     */
    $(document).on('volverAIdentificarCliente', function () {
        console.log('═══════════════════════════════════════════════════');
        console.log('📡 EVENTO RECIBIDO: volverAIdentificarCliente');
        console.log('═══════════════════════════════════════════════════');

        // Abrir modal de identificar cliente con delay
        setTimeout(() => {
            abrirModalIdentificarCliente();
            console.log('✅ Modal de identificar cliente reabierto');
        }, 400);
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
    
    // ✅ NUEVO: Ocultar botón EDITAR
    $('#btnEditarCliente').hide();
    console.log('✅ Botón EDITAR ocultado');
    
    // ❿ LIMPIAR SESIÓN DEL SERVIDOR
    limpiarSesionClientesBuscados();
    
    console.log('═══════════════════════════════════════════════════');
    console.log('✅ LIMPIAR MODAL CLIENTE - FINALIZADO');
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
        console.warn('⚠️ ADVERTENCIA: Origen desconocido "${datosCliente.origen}"');
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

    // ❽ OCULTAR ALERT DE "SIN CLIENTE"
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

    // ⓪ DESHABILITAR BOTÓN SEGUIR TEMPORALMENTE
    $('#btnSeguirCliente').prop('disabled', true);

    // ⓻ LLAMAR A LA FUNCIÓN DE BÚSQUEDA CON EL CRITERIO CORRECTO
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

// ====== MOSTRAR DATOS DEL CLIENTE ====== (✅ ACTUALIZADO v3.0)
/**
 * ✅ ACTUALIZADO v3.1: Ahora maneja correctamente el ID según origen
 * 
 * CAMBIOS v3.1:
 * - Para Origen C: Muestra cta_id
 * - Para Origen F: Muestra "N/A" (no tiene ID de cliente)
 */
function mostrarDatosCliente(cliente) {
    console.log('═══════════════════════════════════════════════════');
    console.log('📋 MOSTRAR DATOS DEL CLIENTE - INICIO v3.1');
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
    
    // ✅ NUEVO v3.1: Determinar valor de ID según origen
    let idDisplay = '';
    const origenUpper = (cliente.origen || '').toUpperCase();
    
    if (origenUpper === 'C') {
        // Cliente Registrado → Mostrar ID
        idDisplay = cliente.id || '';
        console.log('   - ID (Cliente Registrado):', idDisplay);
    } else if (origenUpper === 'F') {
        // Consumidor Final → No tiene ID
        idDisplay = 'N/A';
        console.log('   - ID (Consumidor Final): N/A (no aplica)');
    } else {
        // Origen desconocido → Mostrar ID si existe
        idDisplay = cliente.id || 'N/A';
        console.log('   - ID (Origen desconocido):', idDisplay);
    }
    
    // Determinar valor de tipoNumero (con retrocompatibilidad)
    let tipoNumeroDisplay = '';
    
    if (cliente.tdocDesc && cliente.documento) {
        tipoNumeroDisplay = `${cliente.tdocDesc} ${cliente.documento}`;
        console.log('   - Tipo de documento (separado):', cliente.tdocDesc);
        console.log('   - Número de documento:', cliente.documento);
    } else if (cliente.tipoNumero) {
        tipoNumeroDisplay = cliente.tipoNumero;
        console.log('   - Tipo/Número (combinado):', cliente.tipoNumero);
    }
    
    $('#txtNombre').val(cliente.nombre || '');
    $('#txtClienteId').val(idDisplay); // ← USAR idDisplay (no cliente.id directamente)
    $('#txtDomicilio').val(cliente.domicilio || '');
    $('#txtCondicionAfip').val(cliente.condicionAfip || '');
    $('#txtTipoNumero').val(tipoNumeroDisplay);
    $('#txtEmite').val(cliente.emite || '');
    $('#txtEmail').val(cliente.email || '');
    $('#txtMovil').val(cliente.movil || '');
    
    console.log('   - Nombre:', cliente.nombre);
    console.log('   - ID Display:', idDisplay);
    console.log('   - Domicilio:', cliente.domicilio);
    console.log('   - Tipo/Número Display:', tipoNumeroDisplay);
    
    // ❼ ✅ NUEVO: Mostrar/Ocultar botón EDITAR según origen
    const esConsumidorFinal = cliente.origen && cliente.origen.toUpperCase() === 'F';
    
    if (esConsumidorFinal) {
        $('#btnEditarCliente').fadeIn(300);
        console.log('✅ Botón EDITAR mostrado (Consumidor Final)');
    } else {
        $('#btnEditarCliente').fadeOut(300);
        console.log('ℹ️ Botón EDITAR ocultado (no es Consumidor Final)');
    }
    
    // ❽ Mostrar el card con los datos
    $('#cardDatosCliente')
        .show()
        .removeClass('hide')
        .addClass('show');
    
    console.log('✅ Card de datos mostrado');
    
    // ❾ Habilitar botón SEGUIR
    $('#btnSeguirCliente').prop('disabled', false);
    console.log('✅ Botón SEGUIR habilitado');
    
    // ❿ Guardar cliente seleccionado en variable global
    clienteSeleccionado = cliente;
    console.log('✅ Cliente guardado en variable global');
    
    console.log('═══════════════════════════════════════════════════');
    console.log('✅ MOSTRAR DATOS DEL CLIENTE - FINALIZADO v3.0');
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
    
    // ✅ NUEVO: Ocultar botón EDITAR
    $('#btnEditarCliente').hide();
    
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
 * ✅ ACTUALIZADO: Abre el modal para editar un Consumidor Final
 * Ahora carga datos desde la sesión del servidor
 */
function abrirModalClienteEditar() {
    console.log('═══════════════════════════════════════════════════');
    console.log('✏️ ABRIR MODAL EDITAR CONSUMIDOR FINAL v2.0');
    console.log('═══════════════════════════════════════════════════');
    
    // ❶ Verificar si hay cliente seleccionado en memoria
    if (!clienteSeleccionado) {
        console.error('❌ No hay cliente seleccionado');
        mostrarMensajeError('No hay cliente seleccionado para editar');
        return;
    }
    
    // ❷ Verificar que sea Consumidor Final
    if (!clienteSeleccionado.origen || clienteSeleccionado.origen.toUpperCase() !== 'F') {
        console.error('❌ El cliente seleccionado no es Consumidor Final');
        console.error('   Origen:', clienteSeleccionado.origen);
        mostrarMensajeError('Solo se pueden editar Consumidores Finales');
        return;
    }
    
    console.log('✅ Cliente es Consumidor Final - Cargando datos desde sesión...');
    
    // ❸ Obtener datos completos desde el servidor (sesión)
    const urlObtenerCliente = typeof ObtenerClienteActualUrl !== 'undefined' && ObtenerClienteActualUrl
        ? ObtenerClienteActualUrl
        : '/Facturacion/Cliente/ObtenerClienteActual';
    
    $.ajax({
        url: urlObtenerCliente,
        type: 'POST',
        success: function(response) {
            if (response.ok && response.cliente) {
                console.log('✅ Datos del cliente obtenidos desde sesión:', response.cliente);
                
                // ✅ CRÍTICO: Establecer modo EDITAR PRIMERO
                modoEdicionCliente = true;
                console.log('✅ Modo edición establecido a: TRUE');
                
                // ✅ CORREGIDO v3.0: Limpiar formulario PRESERVANDO el modo
                limpiarFormularioCliente(true); // ← PASAR true PARA PRESERVAR
                
                // Configurar textos para EDITAR
                $('#lblTituloClienteUpdate').html('<i class="bx bx-edit"></i> Editar Consumidor Final');
                $('#lblBotonAccion').text('Actualizar');
                
                // ❹ Hidratar formulario con datos desde sesión
                $('#txtClienteIdUpdate').val(response.cliente.id);
                $('#selTipoDocumento').val(response.cliente.tipoDocumento || '96');
                $('#txtNumeroDocumento').val(response.cliente.numeroDocumento).prop('readonly', true);
                $('#txtNombreCliente').val(response.cliente.nombre);
                $('#txtDomicilioCliente').val(response.cliente.domicilio || '');
                $('#txtEmailCliente').val(response.cliente.email || '');
                $('#txtMovilCliente').val(response.cliente.movil || '');
                
                // ✅ CORREGIDO v2.0: Ajustar placeholder SIN limpiar valor
                ajustarPlaceholderSegunTipo(false); // ← PASAR false PARA NO LIMPIAR
                
                console.log('═══════════════════════════════════════════════════');
                console.log('📊 VERIFICACIÓN DE DATOS HIDRATADOS');
                console.log('═══════════════════════════════════════════════════');
                console.log('   ID:', $('#txtClienteIdUpdate').val());
                console.log('   Tipo Doc:', $('#selTipoDocumento').val());
                console.log('   Número Doc:', $('#txtNumeroDocumento').val());
                console.log('   Nombre:', $('#txtNombreCliente').val());
                console.log('   Domicilio:', $('#txtDomicilioCliente').val());
                console.log('   Email:', $('#txtEmailCliente').val());
                console.log('   Móvil:', $('#txtMovilCliente').val());
                console.log('═══════════════════════════════════════════════════');
                
                // Mostrar modal
                $('#modalClienteUpdate').modal('show');
                
                // Focus en nombre
                setTimeout(() => {
                    $('#txtNombreCliente').trigger("focus").trigger("select");
                }, 500);
                
                console.log('═══════════════════════════════════════════════════');
                console.log('✅ Modal de edición abierto exitosamente v2.0');
                console.log('═══════════════════════════════════════════════════');
            } else {
                console.error('❌ No se pudieron obtener datos del cliente desde sesión');
                mostrarMensajeError(response.mensaje || 'No se pudieron cargar los datos del cliente');
            }
        },
        error: function(xhr) {
            console.error('❌ Error AJAX al obtener cliente:', xhr);
            mostrarMensajeError('Error al cargar datos del cliente desde el servidor');
        }
    });
}

/**
 * ✅ ACTUALIZADO v3.0: Limpia el formulario de cliente
 * 
 * CAMBIOS v3.0:
 * - Agregado parámetro preservarModoEdicion
 * - Solo resetea modoEdicionCliente si preservarModoEdicion = false
 * 
 * @param {boolean} preservarModoEdicion - Si es true, NO resetea modoEdicionCliente (default: false)
 * @returns {void}
 */
function limpiarFormularioCliente(preservarModoEdicion = false) {
    console.log('🧹 Limpiando formulario de cliente v3.0...');
    console.log(`   Preservar modo edición: ${preservarModoEdicion ? 'SÍ' : 'NO'}`);
    console.log(`   Modo actual ANTES: ${modoEdicionCliente ? 'EDITAR' : 'NUEVO'}`);
    
    // ❶ Resetear formulario HTML
    $('#formClienteUpdate')[0].reset();
    
    // ❷ Limpiar campo oculto de ID
    $('#txtClienteIdUpdate').val('');
    
    // ❸ Quitar clases de validación
    $('#formClienteUpdate .form-control, #formClienteUpdate .form-select')
        .removeClass('is-valid is-invalid');
    
    // ❹ Habilitar botón de guardar
    $('#btnCargarCliente')
        .removeClass('processing')
        .prop('disabled', false);
    
    // ❺ Habilitar campo de número de documento
    $('#txtNumeroDocumento').prop('readonly', false);
    
    // ❶ Resetear placeholder según tipo seleccionado (CON limpieza de valor)
    ajustarPlaceholderSegunTipo(true); // ← LIMPIAR en modo NUEVO
    
    // ❼ ✅ NUEVO v3.0: Resetear modo SOLO si NO se debe preservar
    if (!preservarModoEdicion) {
        modoEdicionCliente = false;
        console.log('   ✅ Modo edición reseteado a FALSE');
    } else {
        console.log(`   ℹ️ Modo edición PRESERVADO como: ${modoEdicionCliente ? 'EDITAR' : 'NUEVO'}`);
    }
    
    console.log(`   Modo actual DESPUÉS: ${modoEdicionCliente ? 'EDITAR' : 'NUEVO'}`);
    console.log('✅ Formulario limpiado correctamente v3.0');
}

/**
 * ✅ ACTUALIZADO v2.0: Confirma el cliente seleccionado y abre productos
 * 
 * Se invoca cuando el usuario hace clic en "SEGUIR".
 * Valida que haya cliente seleccionado, cierra el modal y dispara evento para mostrar productos.
 * 
 * @param {Object} cliente - Objeto con datos del cliente seleccionado
 * @returns {void}
 */
function confirmarCliente(cliente) {
    console.log('═══════════════════════════════════════════════════');
    console.log('✅ CONFIRMAR CLIENTE SELECCIONADO v2.0');
    console.log('═══════════════════════════════════════════════════');

    // ❶ VALIDAR que hay cliente
    if (!cliente) {
        console.error('❌ No hay cliente para confirmar');
        mostrarMensajeError('No hay cliente seleccionado');
        return;
    }

    console.log(`   Cliente: ${cliente.nombre}`);
    console.log(`   ID: ${cliente.id || 'N/A'}`);
    console.log(`   Origen: ${cliente.origenDesc} (${cliente.origen})`);
    console.log(`   Documento: ${cliente.documento || 'N/A'}`);

    // ❷ Cerrar modal de identificar cliente
    $('#modalIdentificarCliente').modal('hide');
    console.log('✅ Modal de identificar cliente cerrado');

    // ❸ ✅ NUEVO v2.0: Disparar evento personalizado para prodfact.js
    setTimeout(() => {
        $(document).trigger('clienteConfirmado', [cliente]);
        console.log('✅ Evento "clienteConfirmado" disparado con datos del cliente');
        console.log('   prodfact.js debería mostrar la sección de productos automáticamente');
    }, 400);

    console.log('═══════════════════════════════════════════════════');
    console.log('✅ CONFIRMAR CLIENTE - FINALIZADO v2.0');
    console.log('═══════════════════════════════════════════════════');
}

// ════════════════════════════════════════════════════════════════
// ✅ FUNCIONES AUXILIARES Y DE VALIDACIÓN
// ════════════════════════════════════════════════════════════════

/**
 * ✅ ACTUALIZADO v3.1: Corrige manejo de ID en Consumidores Finales
 * 
 * CAMBIOS v3.1:
 * - NO sobrescribe el ID (los Consumidores Finales no tienen ID)
 * - Solo actualiza campos modificables
 */
function guardarCliente() {
    // Obtener datos del formulario
    const clienteData = {
        id: $('#txtClienteIdUpdate').val(),
        nombre: $('#txtNombreCliente').val().trim(),
        domicilio: $('#txtDomicilioCliente').val().trim(),
        email: $('#txtEmailCliente').val().trim(),
        movil: $('#txtMovilCliente').val().trim(),
        tipoDocumento: $('#selTipoDocumento').val(),
        numeroDocumento: $('#txtNumeroDocumento').val().trim()
    };
    
    // Validar datos requeridos
    if (!clienteData.nombre || !clienteData.tipoDocumento || !clienteData.numeroDocumento) {
        mostrarMensajeError('Por favor, complete todos los campos obligatorios');
        return;
    }
    
    // URL de actualización (fallback en caso de que no esté definida la variable global)
    const urlActualizar = typeof ActualizarConsumidorFinalUrl !== 'undefined' && ActualizarConsumidorFinalUrl 
        ? ActualizarConsumidorFinalUrl 
        : '/Facturacion/Cliente/ActualizarConsumidorFinal';
    
    console.log('🔄 Guardando cliente...', clienteData);
    
    if (modoEdicionCliente) {
        $('#btnCargarCliente').addClass('processing').prop('disabled', true);
        
        $.ajax({
            url: urlActualizar,
            type: 'POST',
            data: clienteData,
            success: function(response) {
                if (response.ok) {
                    console.log('✅ Cliente actualizado en el servidor');
                    
                    mostrarMensajeExito('Consumidor Final actualizado correctamente');
                    
                    setTimeout(() => {
                        cerrarModalClienteUpdate();
                        
                        // ✅ CORREGIDO v3.1: NO sobrescribir ID (Consumidores Finales no tienen)
                        const clienteActualizado = {
                            ...clienteSeleccionado, // ← Mantiene el ID original (null o vacío)
                            
                            // ✅ Actualizar SOLO campos modificables
                            nombre: clienteData.nombre,
                            domicilio: clienteData.domicilio,
                            email: clienteData.email,
                            movil: clienteData.movil,
                            
                            // ✅ Actualizar campos derivados del tipo de documento
                            tdocDesc: obtenerDescripcionTipoDoc(clienteData.tipoDocumento),
                            tdocId: clienteData.tipoDocumento,
                            documento: clienteData.numeroDocumento,
                            tipoNumero: `${obtenerDescripcionTipoDoc(clienteData.tipoDocumento)} ${clienteData.numeroDocumento}`
                            
                            // ❌ NO agregar: id: clienteData.id
                            // Razón: Los Consumidores Finales NO tienen ID
                        };
                        
                        console.log('═══════════════════════════════════════════════════');
                        console.log('📊 CLIENTE ACTUALIZADO - OBJETO CORREGIDO v3.1');
                        console.log('═══════════════════════════════════════════════════');
                        console.log('   ID:', clienteActualizado.id, '(debe ser null o vacío para CF)');
                        console.log('   Origen:', clienteActualizado.origen);
                        console.log('   Nombre:', clienteActualizado.nombre);
                        console.log('   Documento:', clienteActualizado.documento);
                        console.log('   Tipo Doc:', clienteActualizado.tdocDesc);
                        console.log('═══════════════════════════════════════════════════');
                        
                        mostrarDatosCliente(clienteActualizado);
                    }, 1500);
                } else {
                    console.error('❌ Error al actualizar:', response.mensaje);
                    mostrarMensajeError(response.mensaje || 'Error al actualizar el cliente');
                    $('#btnCargarCliente').removeClass('processing').prop('disabled', false);
                }
            },
            error: function(xhr) {
                console.error('❌ Error AJAX:', xhr);
                mostrarMensajeError('Error al comunicarse con el servidor');
                $('#btnCargarCliente').removeClass('processing').prop('disabled', false);
            }
        });
    } else {
        // ✅ MODO NUEVO: Crear Consumidor Final (TODO: Implementar)
        console.log('⚠️ TODO: Implementar creación de nuevo cliente');
        mostrarMensajeError('Funcionalidad de creación de cliente no implementada aún');
    }
}

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

/**
 * Valida un campo individual
 */
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

/**
 * Valida formato de email
 */
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

/**
 * ✅ NUEVA: Obtiene la descripción del tipo de documento según su ID
 */
function obtenerDescripcionTipoDoc(tdocId) {
    const tipos = {
        '80': 'CUIT',
        '86': 'CUIL',
        '87': 'CDI',
        '89': 'LE',
        '90': 'LC',
        '91': 'CI Extranjera',
        '94': 'Pasaporte',
        '95': 'CI Bs. As. RNP',
        '96': 'D.N.I.',
        '99': 'Sin Identificar'
    };

    return tipos[tdocId] || 'Desconocido';
}

// ════════════════════════════════════════════════════════════════
// ✅ FUNCIONES DE LIMPIEZA Y SESIÓN
// ════════════════════════════════════════════════════════════════

/**
 * ✅ NUEVO v1.0: Limpia la sesión de clientes buscados en el servidor
 * 
 * Esta función se invoca cuando el usuario limpia el modal de identificar cliente.
 * Realiza una llamada AJAX asíncrona al servidor para limpiar las variables de sesión:
 * - ClientesBuscados: Lista de clientes encontrados en búsqueda múltiple
 * 
 * Comportamiento:
 * - No bloquea el flujo de ejecución (fire and forget)
 * - Si falla, solo registra warning en consola (no crítico para UX)
 * - Libera memoria en el servidor
 * 
 * @returns {void}
 */
function limpiarSesionClientesBuscados() {
    // ❶ Verificar si existe la URL configurada
    const urlLimpiarSesion = typeof LimpiarSesionClientesUrl !== 'undefined' && LimpiarSesionClientesUrl
        ? LimpiarSesionClientesUrl
        : '/Facturacion/Cliente/LimpiarSesionClientes';

    console.log('🧹 Limpiando sesión de clientes buscados en el servidor...');
    console.log(`   URL: ${urlLimpiarSesion}`);

    // ❷ Llamada AJAX asíncrona (fire and forget)
    $.ajax({
        url: urlLimpiarSesion,
        type: 'POST',
        async: true, // No bloquear la ejecución
        timeout: 5000, // Timeout de 5 segundos
        success: function (response) {
            if (response && response.ok) {
                console.log('✅ Sesión de clientes limpiada en el servidor');
            } else {
                console.warn('⚠️ Respuesta inesperada al limpiar sesión:', response);
            }
        },
        error: function (xhr, status, error) {
            // No mostrar error al usuario, solo log de advertencia
            console.warn('═══════════════════════════════════════════════════');
            console.warn('⚠️ No se pudo limpiar la sesión en el servidor');
            console.warn(`   Status HTTP: ${xhr.status}`);
            console.warn(`   Error: ${error}`);
            console.warn(`   Nota: Esto no afecta la funcionalidad del cliente`);
            console.warn('═══════════════════════════════════════════════════');
        }
    });
}

/**
 * ✅ NUEVO v1.0: Cierra modal y retorna al menú principal de caja
 * 
 * Se invoca cuando el usuario hace clic en el botón "SALIR AL MENÚ".
 * Muestra confirmación antes de redirigir.
 * 
 * @returns {void}
 */
function confirmarSalidaAlMenu() {
    console.log('═══════════════════════════════════════════════════');
    console.log('🚪 CONFIRMACIÓN DE SALIDA AL MENÚ PRINCIPAL');
    console.log('═══════════════════════════════════════════════════');

    // ❶ Verificar si hay cliente seleccionado
    if (clienteSeleccionado) {
        console.warn('⚠️ Hay cliente seleccionado - Requiere confirmación');

        // Mostrar confirmación
        AbrirMensaje(
            "Confirmar Salida",
            "¿Está seguro que desea salir al menú principal?\n\n" +
            "Se perderá el cliente seleccionado.",
            function () {
                $("#msjModal").modal("hide");
                // Usuario confirmó - Redirigir
                redirigirAlMenu();
            },
            true, // Mostrar botón cancelar
            ["Sí, salir", "Cancelar"],
            "warning",
            null
        );
    } else {
        console.log('ℹ️ No hay cliente seleccionado - Salida directa');

        // No hay cliente seleccionado - Salir directamente
        redirigirAlMenu();
    }
}

/**
 * ✅ NUEVO v1.0: Redirige al menú principal de caja
 * 
 * Función auxiliar que realiza la redirección efectiva.
 * Limpia sesión antes de redirigir.
 * 
 * @returns {void}
 */
function redirigirAlMenu() {
    console.log('🚀 Redirigiendo al menú principal...');

    // ❶ Limpiar sesión del servidor
    limpiarSesionClientesBuscados();

    // ❷ Obtener URL del menú
    const urlMenu = typeof MenuCajaUrl !== 'undefined' && MenuCajaUrl
        ? MenuCajaUrl
        : '/Home/Index';

    console.log(`   URL destino: ${urlMenu}`);

    // ❸ Redirigir después de un pequeño delay (permite que el AJAX termine)
    setTimeout(() => {
        window.location.href = urlMenu;
    }, 300);
}

/**
 * ✅ NUEVO v1.0: Cierra el modal de actualización de cliente
 * 
 * Limpia el formulario y resetea el modo de edición.
 * 
 * @returns {void}
 */
function cerrarModalClienteUpdate() {
    console.log('🔒 Cerrando modal de actualización de cliente...');

    // ❶ Cerrar modal
    $('#modalClienteUpdate').modal('hide');

    // ❷ Limpiar formulario
    limpiarFormularioCliente();

    console.log('✅ Modal de actualización cerrado');
}

/**
 * ✅ ACTUALIZADO v2.0: Limpia el formulario de cliente
 * 
 * Cambios desde v1.0:
 * - Modificado: Ahora llama a ajustarPlaceholderSegunTipo con limpiarValor=true
 * 
 * Resetea todos los campos y quita clases de validación.
 * 
 * @returns {void}
 */
function limpiarFormularioCliente() {
    console.log('🧹 Limpiando formulario de cliente v2.0...');

    // ❶ Resetear formulario HTML
    $('#formClienteUpdate')[0].reset();

    // ❷ Limpiar campo oculto de ID
    $('#txtClienteIdUpdate').val('');

    // ❸ Quitar clases de validación
    $('#formClienteUpdate .form-control, #formClienteUpdate .form-select')
        .removeClass('is-valid is-invalid');

    // ❹ Habilitar botón de guardar
    $('#btnCargarCliente')
        .removeClass('processing')
        .prop('disabled', false);

    // ❺ Habilitar campo de número de documento
    $('#txtNumeroDocumento').prop('readonly', false);

    // ❻ Resetear placeholder según tipo seleccionado (CON limpieza de valor)
    ajustarPlaceholderSegunTipo(true); // ← LIMPIAR en modo NUEVO

    // ❼ Resetear modo de edición
    modoEdicionCliente = false;

    console.log('✅ Formulario limpiado correctamente v2.0');
}

/**
 * ✅ ACTUALIZADO v2.0: Ajusta placeholder, maxlength Y clases CSS del input según tipo de documento
 * 
 * Cambios desde v1.0:
 * - Agregado: Parámetro limpiarValor para control de limpieza
 * - Modificado: Solo limpia valor si limpiarValor = true
 * 
 * Reglas de clases CSS:
 * - Tipos numéricos (80, 86, 87, 89, 90, 95, 96): "jsteclado jsinteger"
 * - Tipos alfanuméricos (91, 94, 99): "jsteclado"
 * 
 * @param {boolean} limpiarValor - Si es true, limpia el valor del input (default: true)
 */
function ajustarPlaceholderSegunTipo(limpiarValor = true) {
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

    // ✅ MODIFICADO v2.0: Limpiar valor SOLO si se indica explícitamente
    if (limpiarValor) {
        $inputNumero.val('');
        console.log(`📝 Valor del documento limpiado (modo: ${limpiarValor ? 'LIMPIAR' : 'PRESERVAR'})`);
    } else {
        console.log(`📝 Valor del documento PRESERVADO`);
    }

    console.log(`📝 Tipo documento cambiado a: ${tipoSeleccionado}`);
    console.log(`   - Placeholder: "${placeholder}"`);
    console.log(`   - MaxLength: ${maxLength}`);
    console.log(`   - Clases CSS: "${clasesCss}"`);
    console.log(`   - Solo números: ${tiposNumericos.includes(tipoSeleccionado) ? 'SÍ' : 'NO'}`);
}