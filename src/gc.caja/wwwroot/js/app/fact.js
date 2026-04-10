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
        abrirModalClienteNuevo(); // ✅ INTEGRADO
    });

    // Cancelar (solo cierra y redirige)
    $('#btnCancelarCliente').on('click', function () {
        cerrarYSalirFacturacion();
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
    
    // ✅ NUEVO: Prevenir cierre con ESC o clic fuera (doble seguridad)
    $('#modalIdentificarCliente').on('hide.bs.modal', function (e) {
        // Permitir cierre SOLO si se hace clic en el botón CANCELAR
        if (!e.relatedTarget || e.relatedTarget.id !== 'btnCancelarCliente') {
            // Prevenir cierre accidental
            if (!clienteSeleccionado) {
                // Si no hay cliente seleccionado, evitar cierre
                e.preventDefault();
                console.warn('⚠️ Debe seleccionar un cliente o presionar CANCELAR para salir');
            }
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

    //// Mostrar modal
    //const modal = new bootstrap.Modal(document.getElementById('modalIdentificarCliente'), {
    //    backdrop: 'static', // ✅ Evitar cierre con clic fuera
    //    keyboard: false     // ✅ Evitar cierre con ESC
    //});
    //modal.show();

    // ✅ SOLUCIÓN SIMPLE: Usar jQuery para mostrar el modal
    $('#modalIdentificarCliente').modal('show');

    // Focus en campo de búsqueda
    setTimeout(() => {
        $('#txtBuscarCliente').trigger("focus");
    }, 500);
}

function buscarCliente() {
    const criterioBusqueda = $('#txtBuscarCliente').val().trim();

    if (!criterioBusqueda) {
        mostrarMensajeError('Por favor, ingrese CUIT, DNI o ID del cliente');
        return;
    }

    console.log(`🔍 Buscando cliente: ${criterioBusqueda}`);

    // TODO: Implementar llamada AJAX al backend
    // Por ahora, simular datos de ejemplo
    setTimeout(() => {
        const clienteMock = {
            id: '12345',
            nombre: 'JUAN PÉREZ',
            domicilio: 'AV. SIEMPREVIVA 742',
            condicionAfip: 'RESPONSABLE INSCRIPTO',
            tipoNumero: 'CUIT 20-12345678-9',
            emite: 'FACTURA A',
            email: 'juan.perez@ejemplo.com',
            movil: '1234-567890'
        };

        mostrarDatosCliente(clienteMock);
    }, 500);
}

function mostrarDatosCliente(cliente) {
    // Ocultar alert de sin cliente
    $('#alertSinCliente').addClass('hide');

    // Hidratar campos
    $('#txtNombre').val(cliente.nombre);
    $('#txtClienteId').val(cliente.id);
    $('#txtDomicilio').val(cliente.domicilio);
    $('#txtCondicionAfip').val(cliente.condicionAfip);
    $('#txtTipoNumero').val(cliente.tipoNumero);
    $('#txtEmite').val(cliente.emite);
    $('#txtEmail').val(cliente.email);
    $('#txtMovil').val(cliente.movil);

    // Mostrar card con animación
    $('#cardDatosCliente').addClass('show');

    // Habilitar botón SEGUIR
    $('#btnSeguirCliente').prop('disabled', false);

    // Guardar cliente seleccionado
    clienteSeleccionado = cliente;

    console.log('✅ Cliente encontrado:', cliente);
}

function confirmarCliente(cliente) {
    console.log('✅ Cliente confirmado:', cliente);

    // TODO: Pasar datos al módulo de facturación

    // Cerrar modal
    $('#modalIdentificarCliente').modal('hide');

    // Continuar con el flujo de facturación
    // TODO: Cargar vista de productos, etc.
}

/**
 * ✅ NUEVO: Cierra el modal y redirige al menú principal o logout
 */
function cerrarYSalirFacturacion() {
    console.log('🚪 Saliendo del módulo de Facturación...');
    
    // Confirmar salida si no hay cliente seleccionado
    if (!clienteSeleccionado) {
        AbrirMensaje(
            "Confirmar Salida",
            "¿Está seguro de que desea salir del módulo de Facturación?<br><br>" +
            "<small class='text-muted'><i class='bx bx-info-circle'></i> No se ha seleccionado ningún cliente.</small>",
            function (respuesta) {
                $("#msjModal").modal("hide");
                
                if (respuesta === "SI") {
                    // Cerrar modal
                    $('#modalIdentificarCliente').modal('hide');
                    
                    // Redirigir al menú principal o logout
                    setTimeout(() => {
                        // ✅ OPCIÓN 1: Volver al menú de caja
                        // window.location.href = '/Home/Index';
                        
                        // ✅ OPCIÓN 2: Salir completamente (según tu flujo)
                        window.location.href = logout; // Variable global definida en tu layout
                    }, 300);
                }
            },
            true,
            ["Sí, Salir", "No, Continuar"],
            "warn!",
            null
        );
    } else {
        // Si hay cliente seleccionado, salir directamente
        $('#modalIdentificarCliente').modal('hide');
        
        setTimeout(() => {
            window.location.href = logout;
        }, 300);
    }
}

function limpiarModalCliente() {
    // Reset de cliente seleccionado
    clienteSeleccionado = null;

    // Limpiar campos
    $('#txtBuscarCliente').val('');
    $('#txtNombre').val('');
    $('#txtClienteId').val('');
    $('#txtDomicilio').val('');
    $('#txtCondicionAfip').val('');
    $('#txtTipoNumero').val('');
    $('#txtEmite').val('');
    $('#txtEmail').val('');
    $('#txtMovil').val('');

    // Ocultar card
    $('#cardDatosCliente').removeClass('show');

    // Mostrar alert sin cliente
    $('#alertSinCliente').removeClass('hide');

    // Deshabilitar botón SEGUIR
    $('#btnSeguirCliente').prop('disabled', true);
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
    const modal = new bootstrap.Modal(document.getElementById('modalClienteUpdate'));
    modal.show();
    
    // Focus en tipo de documento
    setTimeout(() => {
        $('#selTipoDocumento').focus();
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
    const modal = new bootstrap.Modal(document.getElementById('modalClienteUpdate'));
    modal.show();
    
    // Focus en nombre
    setTimeout(() => {
        $('#txtNombreCliente').focus().select();
    }, 500);
    
    console.log('✏️ Modal Editar Cliente abierto:', clienteData);
}

function ajustarPlaceholderSegunTipo() {
    const tipoSeleccionado = $('#selTipoDocumento').val();
    let placeholder = 'Ingrese el número de documento...';
    let maxLength = 20;
    
    switch (tipoSeleccionado) {
        case 'DNI':
            placeholder = 'Ej: 12345678';
            maxLength = 8;
            break;
        case 'CUIT':
        case 'CUIL':
            placeholder = 'Ej: 20123456789 (sin guiones)';
            maxLength = 11;
            break;
        case 'CDI':
            placeholder = 'Ej: 12345678';
            maxLength = 8;
            break;
        case 'PASAPORTE':
            placeholder = 'Ej: AAA123456';
            maxLength = 20;
            break;
    }
    
    $('#txtNumeroDocumento').attr('placeholder', placeholder).attr('maxlength', maxLength);
}

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
    if (!numeroDoc) {
        $('#txtNumeroDocumento').addClass('is-invalid').removeClass('is-valid');
        esValido = false;
    } else {
        const tipoDoc = $('#selTipoDocumento').val();
        let valido = true;
        
        if (tipoDoc === 'DNI' && numeroDoc.length !== 8) {
            valido = false;
        } else if ((tipoDoc === 'CUIT' || tipoDoc === 'CUIL') && numeroDoc.length !== 11) {
            valido = false;
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

// ========================================
// FUNCIONES AUXILIARES
// ========================================

function mostrarMensajeError(mensaje) {
    // TODO: Integrar con sistema de mensajes del proyecto
    console.error('❌ ERROR:', mensaje);
    alert(mensaje);
}

function mostrarMensajeExito(mensaje) {
    // TODO: Integrar con sistema de mensajes del proyecto
    console.log('✅ SUCCESS:', mensaje);
    alert(mensaje);
}

// ========================================
// EXPOSICIÓN DE FUNCIONES GLOBALES
// ========================================
window.abrirModalClienteEditar = abrirModalClienteEditar;
window.abrirModalClienteNuevo = abrirModalClienteNuevo;