// ========================================================
// GESTOR PRINCIPAL DEL MÓDULO DE COBRANZA DIFERIDA (CD)
// v1.0
// ========================================================

$(function () {
    console.log('🚀 Módulo de Cobranza Diferida Cargado');

    // 1. INICIA EL FLUJO ABRIENDO EL MODAL DE IDENTIFICACIÓN DE CLIENTE
    //    Se reutiliza la función de inicialización de fact.js, que ahora es segura de llamar.
    inicializaVistaFact();

    // 2. SE SUSCRIBE AL EVENTO 'clienteConfirmado'
    //    Este evento es disparado por fact.js cuando un cliente es seleccionado exitosamente.
    $(document).on('clienteConfirmado', function (event, cliente) {
        console.log('✅ Cliente confirmado en Cobranza Diferida:', cliente);

        // Ocultar el modal de identificación si aún está visible
        $('#modalIdentificarCliente').modal('hide');

        // Aquí comienza la lógica específica de la Cobranza Diferida
        gestionarClienteSeleccionado(cliente);
    });

    // 3. MANEJAR LA SALIDA DEL MÓDULO
    //    Sobrescribimos el comportamiento del botón de salida del modal para que
    //    redirija al menú principal de caja.
    $('#btnSalirFacturacion').off('click').on('click', function () {
        console.log('🚪 Usuario solicitó salir al menú principal desde Cobranza Diferida...');
        // Aquí se puede agregar una confirmación si es necesario
        window.location.href = MenuCajaUrl;
    });

    // 4. MANEJAR BOTONES DEL MODAL DE FACTURAS PENDIENTES
    $('#btnCancelarSeleccionFacturas').on('click', function () {
        $('#modalFacturasPendientes').modal('hide');
        // Vuelve a abrir el modal de identificación de cliente
        setTimeout(() => inicializaVistaFact(), 500);
    });

    $('#chkSeleccionarTodo').on('change', function () {
        const isChecked = $(this).is(':checked');
        $('#tbodyFacturasPendientes').find('input[type="checkbox"]').prop('checked', isChecked).trigger('change');
    });

    $(document).on('change', '#tbodyFacturasPendientes input[type="checkbox"]', function () {
        calcularTotalSeleccionado();
    });
});

/**
 * ✅ NUEVO v1.0: Se ejecuta una vez que el cliente ha sido identificado.
 * @param {object} cliente - El objeto con los datos del cliente seleccionado.
 */
function gestionarClienteSeleccionado(cliente) {
    // Mostrar un loader mientras se buscan las facturas pendientes
    if (typeof mostrarLoader === 'function') {
        mostrarLoader("Buscando facturas pendientes del cliente...<br><small class='text-muted'>" + (cliente.denominacion || 'N/A') + "</small>");
    }

    $.ajax({
        url: ObtenerFacturasPendientesUrl,
        type: 'POST',
        success: function (response) {
            if (typeof ocultarLoader === 'function') {
                ocultarLoader();
            }

            if (response.ok) {
                if (response.lista && response.lista.length > 0) {
                    mostrarModalFacturasPendientes(cliente, response.lista);
                } else {
                    AbrirMensaje("Información", "El cliente no tiene facturas pendientes de cobro.", () => {
                        $("#msjModal").modal("hide");
                        setTimeout(() => inicializaVistaFact(), 500);
                    }, false, ["Aceptar"], "info");
                }
            } else {
                AbrirMensaje("Error", response.mensaje || "Ocurrió un error al buscar las facturas.", () => {
                    $("#msjModal").modal("hide");
                    setTimeout(() => inicializaVistaFact(), 500);
                }, false, ["Aceptar"], "error");
            }
        },
        error: function (xhr, status, error) {
            if (typeof ocultarLoader === 'function') {
                ocultarLoader();
            }
            console.error("Error en AJAX:", error);
            AbrirMensaje("Error de Comunicación", "No se pudo conectar con el servidor para obtener las facturas.", () => {
                $("#msjModal").modal("hide");
                setTimeout(() => inicializaVistaFact(), 500);
            }, false, ["Aceptar"], "error");
        }
    });
}

/**
 * Muestra el modal con las facturas pendientes y puebla los datos.
 * @param {object} cliente - Datos del cliente.
 * @param {Array} facturas - Lista de facturas pendientes.
 */
function mostrarModalFacturasPendientes(cliente, facturas) {
    // Poblar datos del cliente en el modal
    $('#txtNombrePendiente').val(cliente.denominacion || '');
    $('#txtClienteIdPendiente').val(cliente.id || 'N/A');
    $('#txtDomicilioPendiente').val(cliente.domicilio || '');
    $('#txtCondicionAfipPendiente').val(cliente.condicionAfip || '');
    $('#txtTipoNumeroPendiente').val(cliente.tipoNumero || '');
    $('#txtEmailPendiente').val(cliente.email || '');
    $('#txtMovilPendiente').val(cliente.movil || '');

    // Poblar grilla de facturas
    const $tbody = $('#tbodyFacturasPendientes');
    $tbody.empty();
    facturas.forEach(factura => {
        const fecha = factura.cm_fecha ? new Date(factura.cm_fecha).toLocaleString('es-AR') : 'N/A';
        const importe = parseFloat(factura.cm_total_cpte || 0);
        const fila = `
            <tr data-importe="${importe}">
                <td class="text-center"><input type="checkbox" class="form-check-input" data-id="${factura.cm_compte}"></td>
                <td>${factura.tco_desc || 'N/A'}</td>
                <td>${factura.cm_compte || 'N/A'}</td>
                <td>${factura.cta_denominacion || 'N/A'}</td>
                <td class="text-center">${fecha}</td>
                <td class="text-end fw-bold">${formatearNumero(importe, 2)}</td>
            </tr>
        `;
        $tbody.append(fila);
    });

    // Resetear total y checkbox principal
    $('#chkSeleccionarTodo').prop('checked', false);
    calcularTotalSeleccionado();

    // Mostrar modal
    $('#modalFacturasPendientes').modal('show');
}

/**
 * Calcula y muestra el total de las facturas seleccionadas.
 */
function calcularTotalSeleccionado() {
    let total = 0;
    $('#tbodyFacturasPendientes tr').each(function () {
        const $checkbox = $(this).find('input[type="checkbox"]');
        if ($checkbox.is(':checked')) {
            total += parseFloat($(this).data('importe') || 0);
        }
    });

    $('#txtTotalSeleccionado').val(`$ ${formatearNumero(total, 2)}`);
    $('#btnSeguirConCobranza').prop('disabled', total === 0);
}

/**
 * Formatea un número a un string con separadores de miles y decimales.
 * @param {number} numero - El número a formatear.
 * @param {number} decimales - La cantidad de decimales.
 * @returns {string} - El número formateado.
 */
function formatearNumero(numero, decimales = 2) {
    if (isNaN(numero)) {
        return '0.00';
    }
    return new Intl.NumberFormat('es-AR', {
        minimumFractionDigits: decimales,
        maximumFractionDigits: decimales
    }).format(numero);
}