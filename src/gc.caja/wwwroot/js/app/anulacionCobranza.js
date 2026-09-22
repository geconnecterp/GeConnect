// ========================================================
// GESTOR DEL MODULO DE ANULACION DE COBRANZA
// ========================================================

let clienteSeleccionadoAnulaCob = null;
let cobranzasAnulaCob = [];
let cobranzaSeleccionadaAnulaCob = null;
let anulandoAnulaCob = false;
let saliendoAnulaCob = false;

window.validarClienteAntesDeMostrar = function (cliente) {
    if (!cliente) {
        return true;
    }

    if (!validarClienteRegistradoAnulaCob(cliente)) {
        informarClienteNoValidoAnulaCob(cliente);
        return false;
    }

    return true;
};

$(function () {
    console.log('═══════════════════════════════════════════════════');
    console.log('MODULO DE ANULACION DE COBRANZA CARGADO');
    console.log('═══════════════════════════════════════════════════');

    registrarEventosAnulacionCobranza();

    $(document)
        .off('clienteConfirmado.anulacionCobranza')
        .on('clienteConfirmado.anulacionCobranza', function (event, cliente) {
            if (!cliente) {
                console.warn('[AnulacionCobranza] clienteConfirmado sin datos.');
                return;
            }

            if (!validarClienteRegistradoAnulaCob(cliente)) {
                informarClienteNoValidoAnulaCob(cliente);
                return;
            }

            clienteSeleccionadoAnulaCob = cliente;
            $('#modalIdentificarCliente').modal('hide');
            abrirModalAnulacionCobranza(cliente);
        });

    inicializarModuloAnulacionCobranza();
});

function inicializarModuloAnulacionCobranza() {
    if (typeof inicializaVistaFact !== 'function') {
        mostrarMensajeAnulaCob('Error', 'No se pudo inicializar el buscador de clientes.', 'error!');
        return;
    }

    setFechaActualAnulaCob();

    setTimeout(function () {
        inicializaVistaFact();
    }, 300);
}

function registrarEventosAnulacionCobranza() {
    $(document)
        .off('click.anulacionCobranza', '#btnBuscarAnulaCob')
        .on('click.anulacionCobranza', '#btnBuscarAnulaCob', function (event) {
            event.preventDefault();
            buscarCobranzasAnulaCob();
        });

    $(document)
        .off('change.anulacionCobranza', '#tbodyAnulacionCobranzas input[type="checkbox"]')
        .on('change.anulacionCobranza', '#tbodyAnulacionCobranzas input[type="checkbox"]', function () {
            seleccionarCobranzaAnulaCob($(this));
        });

    $(document)
        .off('click.anulacionCobranza', '#btnCancelarAnulaCob')
        .on('click.anulacionCobranza', '#btnCancelarAnulaCob', function (event) {
            event.preventDefault();
            if (anulandoAnulaCob) return;
            $('#modalAnulacionCobranza').modal('hide');
            setTimeout(function () {
                $('#modalIdentificarCliente').modal('show');
            }, 350);
        });

    $(document)
        .off('click.anulacionCobranza', '#btnAnularCobranza')
        .on('click.anulacionCobranza', '#btnAnularCobranza', function (event) {
            event.preventDefault();
            confirmarAnulacionCobranza();
        });
}

function validarClienteRegistradoAnulaCob(cliente) {
    const ctaId = obtenerPrimerValorAnulaCob(cliente?.cta_id, cliente?.id);
    const origen = obtenerPrimerValorAnulaCob(cliente?.origen).toUpperCase();

    return origen !== 'F' && Boolean(ctaId);
}

function informarClienteNoValidoAnulaCob(cliente) {
    clienteSeleccionadoAnulaCob = null;
    cobranzasAnulaCob = [];
    cobranzaSeleccionadaAnulaCob = null;

    const nombre = escaparHtmlAnulaCob(obtenerPrimerValorAnulaCob(cliente?.denominacion, cliente?.cta_denominacion, cliente?.nombre, 'N/A'));
    const documento = escaparHtmlAnulaCob(obtenerTipoNumeroClienteAnulaCob(cliente) || 'N/A');
    const origen = escaparHtmlAnulaCob(obtenerPrimerValorAnulaCob(cliente?.origen_desc, cliente?.origen, 'Consumidor Final'));
    const ctaId = escaparHtmlAnulaCob(obtenerPrimerValorAnulaCob(cliente?.cta_id, cliente?.id, 'N/A'));

    console.warn('[AnulacionCobranza] Cliente rechazado para anulacion:', cliente);

    if (typeof limpiarVista === 'function') {
        limpiarVista();
    }

    setTimeout(function () {
        $('#modalIdentificarCliente').modal('hide');
    }, 50);

    const mensaje = `<div class="text-start">
        <p class="mb-2">El cliente seleccionado no puede utilizarse para <strong>Anulacion de Cobranza</strong>.</p>
        <div class="border rounded p-2 bg-light mb-2">
            <div><strong>Cliente:</strong> ${nombre}</div>
            <div><strong>Documento:</strong> ${documento}</div>
            <div><strong>Origen:</strong> ${origen}</div>
            <div><strong>Cuenta:</strong> ${ctaId}</div>
        </div>
        <p class="mb-0 text-muted">Este proceso requiere un Cliente Registrado con identificador de cuenta corriente valido.</p>
    </div>`;

    mostrarMensajeAnulaCob(
        'Cliente no habilitado',
        mensaje,
        'warn!',
        function () {
            $('#modalIdentificarCliente').modal('show');
        }
    );
}

function abrirModalAnulacionCobranza(cliente) {
    poblarDatosClienteAnulaCob(cliente);
    limpiarGrillaAnulaCob('Buscando cobranzas del cliente...');
    $('#modalAnulacionCobranza').modal('show');

    setTimeout(function () {
        buscarCobranzasAnulaCob();
    }, 250);
}

function buscarCobranzasAnulaCob() {
    if (anulandoAnulaCob) return;
    if (!clienteSeleccionadoAnulaCob) {
        mostrarMensajeAnulaCob('Atencion', 'Debe seleccionar un cliente registrado.', 'warn!');
        return;
    }

    const ctaId = obtenerPrimerValorAnulaCob(
        clienteSeleccionadoAnulaCob.cta_id,
        clienteSeleccionadoAnulaCob.id
    );

    if (!ctaId) {
        mostrarMensajeAnulaCob('Atencion', 'El cliente seleccionado no posee un identificador de cuenta valido.', 'warn!');
        return;
    }

    const origen = obtenerPrimerValorAnulaCob(clienteSeleccionadoAnulaCob.origen).toUpperCase();
    if (origen === 'F') {
        mostrarMensajeAnulaCob('Atencion', 'La anulacion de cobranza requiere un Cliente Registrado.', 'warn!');
        return;
    }

    const fecha = $('#txtFechaAnulaCob').val() || obtenerFechaActualIsoAnulaCob();

    mostrarLoaderAnulaCob('Buscando cobranzas para anulacion...');
    limpiarGrillaAnulaCob('Buscando cobranzas...');

    $.ajax({
        url: anulacionCobranzaBuscarUrl,
        type: 'POST',
        contentType: 'application/json',
        dataType: 'json',
        data: JSON.stringify({ cta_id: ctaId, fecha: fecha }),
        success: function (response) {
            ocultarLoaderAnulaCob();
            console.log('[AnulacionCobranza] Respuesta BuscarCobranzas:', response);

            if (!response || !response.ok) {
                cobranzasAnulaCob = [];
                renderizarCobranzasAnulaCob([]);
                mostrarMensajeAnulaCob(
                    'Informacion',
                    response?.mensaje || 'No se pudieron obtener las cobranzas del cliente.',
                    'info!'
                );
                return;
            }

            cobranzasAnulaCob = Array.isArray(response.lista) ? response.lista : [];
            renderizarCobranzasAnulaCob(cobranzasAnulaCob);
        },
        error: function (xhr, status, error) {
            ocultarLoaderAnulaCob();
            console.error('[AnulacionCobranza] Error AJAX BuscarCobranzas:', {
                status: xhr.status,
                statusText: xhr.statusText,
                responseText: xhr.responseText,
                error: error
            });

            renderizarCobranzasAnulaCob([]);
            mostrarMensajeAnulaCob('Error', 'No se pudieron consultar las cobranzas.', 'error!');
        }
    });
}

function renderizarCobranzasAnulaCob(lista) {
    const $tbody = $('#tbodyAnulacionCobranzas');
    $tbody.empty();
    cobranzaSeleccionadaAnulaCob = null;
    actualizarSeleccionAnulaCob();

    if (!Array.isArray(lista) || lista.length === 0) {
        $tbody.append(
            '<tr>' +
            '<td colspan="5" class="text-center text-muted py-4">' +
            '<i class="bx bx-info-circle"></i> No se encontraron cobranzas anulables para la fecha seleccionada.' +
            '</td>' +
            '</tr>'
        );
        return;
    }

    lista.forEach(function (item, index) {
        const importe = normalizarMontoAnulaCob(item.co_cobranza);
        const fila = `
            <tr data-index="${index}">
                <td class="text-center">
                    <input type="checkbox" class="form-check-input chk-anula-cob" data-index="${index}">
                </td>
                <td class="text-center">${escaparHtmlAnulaCob(formatearFechaAnulaCob(item.co_fecha))}</td>
                <td class="fw-bold">${escaparHtmlAnulaCob(item.rb_compte || 'N/A')}</td>
                <td>${escaparHtmlAnulaCob(item.comprobantes_cancelados || '')}</td>
                <td class="text-end fw-bold text-success">$ ${formatearMontoAnulaCob(importe)}</td>
            </tr>`;

        $tbody.append(fila);
    });
}

function seleccionarCobranzaAnulaCob($checkbox) {
    if (anulandoAnulaCob) return;
    const estaMarcado = $checkbox.is(':checked');

    $('#tbodyAnulacionCobranzas input[type="checkbox"]').not($checkbox).prop('checked', false);

    if (!estaMarcado) {
        cobranzaSeleccionadaAnulaCob = null;
        actualizarSeleccionAnulaCob();
        return;
    }

    const index = convertirEnteroAnulaCob($checkbox.attr('data-index'));
    cobranzaSeleccionadaAnulaCob = Number.isInteger(index) ? cobranzasAnulaCob[index] : null;
    actualizarSeleccionAnulaCob();
}

function actualizarSeleccionAnulaCob() {
    if (anulandoAnulaCob) return;
    if (!cobranzaSeleccionadaAnulaCob) {
        $('#btnAnularCobranza').prop('disabled', true);
        $('#lblAnulacionCobranzaEstado').text('Sin recibo seleccionado');
        $('#lblAnulacionCobranzaImporte').text('$ 0.00');
        return;
    }

    const recibo = cobranzaSeleccionadaAnulaCob.rb_compte || 'N/A';
    const operacion = cobranzaSeleccionadaAnulaCob.caja_nro_operacion || 'N/A';
    const importe = normalizarMontoAnulaCob(cobranzaSeleccionadaAnulaCob.co_cobranza);

    $('#btnAnularCobranza').prop('disabled', false);
    $('#lblAnulacionCobranzaEstado').text(`Recibo ${recibo} - Operacion ${operacion}`);
    $('#lblAnulacionCobranzaImporte').text(`$ ${formatearMontoAnulaCob(importe)}`);
}

function confirmarAnulacionCobranza() {
    if (anulandoAnulaCob) {
        return;
    }

    if (!cobranzaSeleccionadaAnulaCob) {
        mostrarMensajeAnulaCob('Atencion', 'Debe seleccionar un recibo para anular.', 'warn!');
        return;
    }

    const recibo = escaparHtmlAnulaCob(cobranzaSeleccionadaAnulaCob.rb_compte || 'N/A');
    const operacion = escaparHtmlAnulaCob(cobranzaSeleccionadaAnulaCob.caja_nro_operacion || 'N/A');
    const importe = formatearMontoAnulaCob(normalizarMontoAnulaCob(cobranzaSeleccionadaAnulaCob.co_cobranza));

    AbrirMensaje(
        'Confirmar anulación de cobranza',
        `<div class="text-center p-2">
            <i class="bx bx-undo text-golden" style="font-size: 4rem;" aria-hidden="true"></i>
            <h4 class="text-golden mt-3 mb-3">¿Confirmás la anulación de esta cobranza?</h4>
            <div class="card card-golden text-start mb-3">
                <div class="card-header fw-bold">Cobranza seleccionada</div>
                <div class="card-body">
                    <div class="d-flex justify-content-between mb-2"><span>Recibo</span><strong>${recibo}</strong></div>
                    <div class="d-flex justify-content-between mb-2"><span>Operación</span><strong>${operacion}</strong></div>
                    <div class="d-flex justify-content-between border-top pt-2"><span>Importe</span><strong class="fs-4 text-golden">$ ${importe}</strong></div>
                </div>
            </div>
            <p class="text-muted mb-0">Se registrará la anulación en la caja actual.</p>
        </div>`,
        function (respuesta) {
            $('#msjModal').modal('hide');
            if (respuesta === 'SI') {
                ejecutarAnulacionCobranza();
            }
        },
        true,
        ['Confirmar anulación', 'Cancelar'],
        'quest!',
        null
    );
}

function ejecutarAnulacionCobranza() {
    if (anulandoAnulaCob) return;
    if (!cobranzaSeleccionadaAnulaCob) {
        mostrarMensajeAnulaCob('Atencion', 'Debe seleccionar un recibo para anular.', 'warn!');
        return;
    }

    const url = String(anulacionCobranzaAnularUrl || '').trim();
    if (!url) {
        mostrarMensajeAnulaCob('Error', 'No se encontro la URL de anulacion.', 'error!');
        return;
    }

    const payload = {
        cta_id: cobranzaSeleccionadaAnulaCob.cta_id,
        caja_nro_proceso: cobranzaSeleccionadaAnulaCob.caja_nro_proceso,
        caja_nro_cierre: cobranzaSeleccionadaAnulaCob.caja_nro_cierre,
        caja_nro_operacion: cobranzaSeleccionadaAnulaCob.caja_nro_operacion
    };

    console.log('[AnulacionCobranza] Request Anular:', payload);
    anulandoAnulaCob = true;
    bloquearVistaAnulaCob(true);
    $('#btnAnularCobranza').prop('disabled', true).html('<i class="bx bx-loader-alt bx-spin"></i> ANULANDO');

    $.ajax({
        url: url,
        type: 'POST',
        dataType: 'json',
        contentType: 'application/json; charset=utf-8',
        data: JSON.stringify(payload),
        timeout: 45000,
        success: function (response) {
            mostrarProgresoAnulaCob(false);
            console.log('[AnulacionCobranza] Response Anular:', response);

            if (!response || typeof response.ok !== 'boolean') {
                informarResultadoInciertoAnulaCob();
                return;
            }
            if (!response.ok) {
                anulandoAnulaCob = false;
                bloquearVistaAnulaCob(false);
                mostrarMensajeAnulaCob('Atención', response.mensaje || 'No se pudo anular la cobranza.', 'warn!');
                return;
            }

            // Mantener el bloqueo hasta salir, también si se cierra el aviso con la X.
            $('#msjModal').one('hidden.bs.modal.salidaAnulaCob', volverAlMenuAnulaCob);
            AbrirMensaje(
                'Cobranza Anulada',
                construirMensajeAnulacionExitosa(response),
                function () {
                    $('#msjModal').modal('hide');
                    volverAlMenuAnulaCob();
                },
                false,
                ['Aceptar'],
                'succ!',
                null
            );
        },
        error: function (xhr, status, error) {
            mostrarProgresoAnulaCob(false);
            console.error('[AnulacionCobranza] Error AJAX Anular:', {
                status: xhr.status,
                statusText: xhr.statusText,
                responseText: xhr.responseText,
                error: error
            });

            informarResultadoInciertoAnulaCob();
        },
        complete: function () {
            if (!anulandoAnulaCob) {
                $('#btnAnularCobranza').html('<i class="bx bx-check-circle"></i> ANULAR');
                actualizarSeleccionAnulaCob();
            }
        }
    });
}

function mostrarProgresoAnulaCob(visible) {
    $('#bloqueoAnulaCob').toggleClass('d-none', !visible).toggleClass('d-flex', visible);
    if (visible) $('#bloqueoAnulaCob').trigger('focus');
}

function bloquearVistaAnulaCob(bloquear) {
    $('#modalAnulacionCobranza, #modalIdentificarCliente').prop('inert', bloquear).attr('aria-busy', String(bloquear));
    $('#modalAnulacionCobranza').find('button, input').prop('disabled', bloquear);
    mostrarProgresoAnulaCob(bloquear);
    if (!bloquear) actualizarSeleccionAnulaCob();
}

function volverAlMenuAnulaCob() {
    if (saliendoAnulaCob) return;
    saliendoAnulaCob = true;
    window.location.replace(anulacionCobranzaMenuUrl);
}

function informarResultadoInciertoAnulaCob() {
    // Una respuesta perdida no significa que el servidor haya rechazado la anulación.
    $('#msjModal').one('hidden.bs.modal.salidaAnulaCob', volverAlMenuAnulaCob);
    mostrarMensajeAnulaCob('Resultado pendiente de verificar',
        'No se pudo confirmar el resultado de la anulación. Volvé al menú y consultá las cobranzas antes de intentar nuevamente.',
        'warn!', volverAlMenuAnulaCob);
}

function construirMensajeAnulacionExitosa(response) {
    const recibo = escaparHtmlAnulaCob(response?.recibo || cobranzaSeleccionadaAnulaCob?.rb_compte || 'N/A');
    const importe = formatearMontoAnulaCob(normalizarMontoAnulaCob(response?.importe ?? cobranzaSeleccionadaAnulaCob?.co_cobranza));
    const mensajeServidor = obtenerPrimerValorAnulaCob(response?.mensaje);
    const mensajeVisible = !mensajeServidor || mensajeServidor.toUpperCase() === 'OK'
        ? `La cobranza del recibo <strong>${recibo}</strong> fue anulada correctamente. Se registró el contramovimiento por <strong>$ ${importe}</strong>.`
        : escaparHtmlAnulaCob(mensajeServidor);

    return `<div class="text-center px-2">
        <i class='bx bx-check-circle text-golden' style="font-size: 4rem;"></i>
        <h4 class="text-golden mt-3 mb-2">Anulación registrada</h4>
        <p class="fs-5 mb-2">${mensajeVisible}</p>
        <p class="text-muted mb-0">La operación quedó asentada en la caja actual.</p>
        ${response?.resultado_id ? `<div class="small text-muted mt-2">Operacion: ${escaparHtmlAnulaCob(response.resultado_id)}</div>` : ''}
    </div>`;
}

function poblarDatosClienteAnulaCob(cliente) {
    $('#txtNombreAnulaCob').val(obtenerPrimerValorAnulaCob(cliente?.denominacion, cliente?.cta_denominacion, cliente?.nombre));
    $('#txtClienteIdAnulaCob').val(obtenerPrimerValorAnulaCob(cliente?.cta_id, cliente?.id));
    $('#txtDomicilioAnulaCob').val(obtenerPrimerValorAnulaCob(cliente?.domicilio, cliente?.cta_domicilio));
    $('#txtCondicionAfipAnulaCob').val(obtenerPrimerValorAnulaCob(cliente?.condicionAfip, cliente?.afip_desc, cliente?.afip_id));
    $('#txtTipoNumeroAnulaCob').val(obtenerTipoNumeroClienteAnulaCob(cliente));
    $('#txtEmailAnulaCob').val(obtenerPrimerValorAnulaCob(cliente?.email, cliente?.cta_email));
    $('#txtMovilAnulaCob').val(obtenerPrimerValorAnulaCob(cliente?.movil, cliente?.cta_celu));
}

function limpiarGrillaAnulaCob(mensaje) {
    $('#tbodyAnulacionCobranzas').html(
        '<tr>' +
        '<td colspan="5" class="text-center text-muted py-4">' +
        `<i class="bx bx-loader-alt bx-spin"></i> ${escaparHtmlAnulaCob(mensaje || 'Cargando...')}` +
        '</td>' +
        '</tr>'
    );
    cobranzaSeleccionadaAnulaCob = null;
    actualizarSeleccionAnulaCob();
}

function setFechaActualAnulaCob() {
    $('#txtFechaAnulaCob').val(obtenerFechaActualIsoAnulaCob());
}

function obtenerFechaActualIsoAnulaCob() {
    const fecha = new Date();
    const yyyy = fecha.getFullYear();
    const mm = String(fecha.getMonth() + 1).padStart(2, '0');
    const dd = String(fecha.getDate()).padStart(2, '0');
    return `${yyyy}-${mm}-${dd}`;
}

function obtenerTipoNumeroClienteAnulaCob(cliente) {
    const tipoNumero = obtenerPrimerValorAnulaCob(cliente?.tipoNumero);
    if (tipoNumero) return tipoNumero;

    return [
        obtenerPrimerValorAnulaCob(cliente?.tdoc_desc, cliente?.tipoDocumento),
        obtenerPrimerValorAnulaCob(cliente?.documento, cliente?.nro_documento)
    ].filter(Boolean).join(' ');
}

function obtenerPrimerValorAnulaCob() {
    const valores = Array.from(arguments);

    for (let i = 0; i < valores.length; i++) {
        const valor = valores[i];
        if (valor !== null && valor !== undefined) {
            const texto = String(valor).trim();
            if (texto && texto.toLowerCase() !== 'null' && texto.toLowerCase() !== 'undefined') {
                return texto;
            }
        }
    }

    return '';
}

function convertirEnteroAnulaCob(valor) {
    const numero = Number.parseInt(String(valor ?? '').trim(), 10);
    return Number.isInteger(numero) ? numero : NaN;
}

function normalizarMontoAnulaCob(valor) {
    const numero = Number(valor);
    return Number.isFinite(numero) ? Math.round((numero + Number.EPSILON) * 100) / 100 : 0;
}

function formatearMontoAnulaCob(monto) {
    if (typeof formatearNumero === 'function') {
        return formatearNumero(normalizarMontoAnulaCob(monto), 2);
    }

    return new Intl.NumberFormat('en-US', {
        minimumFractionDigits: 2,
        maximumFractionDigits: 2
    }).format(normalizarMontoAnulaCob(monto));
}

function formatearFechaAnulaCob(valor) {
    if (!valor) return 'N/A';

    const texto = String(valor);
    const coincidencia = texto.match(/^(\d{4})-(\d{2})-(\d{2})/);
    if (coincidencia) {
        return `${coincidencia[3]}/${coincidencia[2]}/${coincidencia[1]}`;
    }

    const fecha = new Date(valor);
    if (Number.isNaN(fecha.getTime())) return 'N/A';

    return `${String(fecha.getDate()).padStart(2, '0')}/${String(fecha.getMonth() + 1).padStart(2, '0')}/${fecha.getFullYear()}`;
}

function escaparHtmlAnulaCob(valor) {
    return String(valor ?? '')
        .replace(/&/g, '&amp;')
        .replace(/</g, '&lt;')
        .replace(/>/g, '&gt;')
        .replace(/"/g, '&quot;')
        .replace(/'/g, '&#039;');
}

function mostrarLoaderAnulaCob(mensaje) {
    if (typeof mostrarLoader === 'function') {
        mostrarLoader(mensaje);
        return;
    }

    if (typeof AbrirWaiting === 'function') {
        AbrirWaiting(mensaje);
    }
}

function ocultarLoaderAnulaCob() {
    if (typeof ocultarLoader === 'function') {
        ocultarLoader();
        return;
    }

    if (typeof CerrarWaiting === 'function') {
        CerrarWaiting(false);
    }
}

function mostrarMensajeAnulaCob(titulo, mensaje, tipo, callback) {
    if (typeof AbrirMensaje === 'function') {
        AbrirMensaje(
            titulo,
            mensaje,
            function () {
                $('#msjModal').modal('hide');
                if (typeof callback === 'function') {
                    setTimeout(callback, 250);
                }
            },
            false,
            ['Aceptar'],
            tipo || 'warn!',
            null
        );
        return;
    }

    window.alert(`${titulo}: ${String(mensaje).replace(/<br>/g, '\n')}`);
    if (typeof callback === 'function') {
        callback();
    }
}
