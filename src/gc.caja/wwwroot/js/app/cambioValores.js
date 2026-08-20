// ========================================================
// GESTOR DEL MODULO CAMBIOS E INGRESOS DE VALORES
// ========================================================

let clienteSeleccionadoCambioValores = null;
let mediosCambioValores = [];
let instrumentosCambioValores = [];
let bancosCambioValores = [];
let valoresCambioValores = [];
let finalizandoCambioValores = false;
let contextoDetalleCambioValores = null;

window.validarClienteAntesDeMostrar = function (cliente) {
    if (!cliente) return true;
    if (!validarClienteRegistradoCambioValores(cliente)) {
        informarClienteNoValidoCambioValores(cliente);
        return false;
    }
    return true;
};

$(function () {
    console.log('[CambioValores] Modulo cargado');
    registrarEventosCambioValores();

    $(document)
        .off('clienteConfirmado.cambioValores')
        .on('clienteConfirmado.cambioValores', function (event, cliente) {
            if (!cliente) {
                console.warn('[CambioValores] clienteConfirmado sin datos.');
                return;
            }

            if (!validarClienteRegistradoCambioValores(cliente)) {
                informarClienteNoValidoCambioValores(cliente);
                return;
            }

            clienteSeleccionadoCambioValores = cliente;
            $('#modalIdentificarCliente').modal('hide');
            abrirModalCambioValores(cliente);
        });

    inicializarModuloCambioValores();
});

function inicializarModuloCambioValores() {
    if (typeof inicializaVistaFact !== 'function') {
        mostrarMensajeCambioValores('Error', 'No se pudo inicializar el buscador de clientes.', 'error!');
        return;
    }

    setFechaValorActualCambioValores();
    actualizarTipoOperacionCambioValores();
    renderizarValoresCambioValores();
    setTimeout(function () { inicializaVistaFact(); }, 300);
}

function registrarEventosCambioValores() {
    $(document).off('change.cambioValores', '#chkIngresoValor').on('change.cambioValores', '#chkIngresoValor', actualizarTipoOperacionCambioValores);
    $(document).off('change.cambioValores', '#cmbMedioCambioValores').on('change.cambioValores', '#cmbMedioCambioValores', function () { cargarInstrumentosCambioValores($(this).val()); });
    $(document).off('change.cambioValores', '#cmbInstrumentoCambioValores').on('change.cambioValores', '#cmbInstrumentoCambioValores', actualizarFormularioInstrumentoCambioValores);

    $(document).off('focus.cambioValores', '#txtImporteCambioValores, #txtMontoCheque, #txtMontoTransferencia, #txtNroCheque, #txtNroTransferencia')
        .on('focus.cambioValores', '#txtImporteCambioValores, #txtMontoCheque, #txtMontoTransferencia, #txtNroCheque, #txtNroTransferencia', function () {
            seleccionarTextoInicialCambioValores(this);
            const selector = this.id ? '#' + this.id : null;
            setTimeout(function () {
                if (typeof posicionarTecladoVirtual === 'function') {
                    posicionarTecladoVirtual(selector, null, { preferredSide: 'left', verticalAlign: 'bottom' });
                }
            }, 80);
        });
    $(document).off('input.cambioValores', '#txtImporteCambioValores, #txtMontoCheque, #txtMontoTransferencia').on('input.cambioValores', '#txtImporteCambioValores, #txtMontoCheque, #txtMontoTransferencia', function () { this.value = this.value.replace(/[^0-9.,]/g, ''); });
    $(document).off('input.cambioValores', '#txtNroCheque').on('input.cambioValores', '#txtNroCheque', function () { this.value = this.value.replace(/\D/g, '').substring(0, 8); });
    $(document)
        .off('keydown.cambioValoresEnter keypress.cambioValoresEnter', '#txtNroCheque, #txtFechaCheque, #txtMontoCheque, #txtNroTransferencia, #txtFechaTransferencia, #txtMontoTransferencia')
        .on('keydown.cambioValoresEnter keypress.cambioValoresEnter', '#txtNroCheque, #txtFechaCheque, #txtMontoCheque, #txtNroTransferencia, #txtFechaTransferencia, #txtMontoTransferencia', manejarEnterDetalleCambioValores);

    $(document).off('click.cambioValores', '#btnAgregarValorCambioValores').on('click.cambioValores', '#btnAgregarValorCambioValores', function (event) { event.preventDefault(); agregarValorCambioValores(); });
    $(document).off('click.cambioValores', '.btn-eliminar-cambio-valores').on('click.cambioValores', '.btn-eliminar-cambio-valores', function (event) { event.preventDefault(); eliminarValorCambioValores(Number($(this).attr('data-index'))); });

    $(document).off('click.cambioValores', '#btnCancelarCambioValores').on('click.cambioValores', '#btnCancelarCambioValores', function (event) {
        event.preventDefault();
        $('#modalCambioValores').modal('hide');
        setTimeout(function () { $('#modalIdentificarCliente').modal('show'); }, 350);
    });

    $(document).off('click.cambioValores', '#btnFinalizarCambioValores').on('click.cambioValores', '#btnFinalizarCambioValores', function (event) {
        event.preventDefault();
        prepararConfirmacionCambioValores();
    });

    $(document).off('click.cambioValores', '#btnVolverCambioValoresConfirmacion').on('click.cambioValores', '#btnVolverCambioValoresConfirmacion', function (event) {
        event.preventDefault();
        $('#modalCambioValoresConfirmacion').modal('hide');
        setTimeout(function () { $('#modalCambioValores').modal('show'); }, 250);
    });

    $(document).off('click.cambioValores', '#btnConfirmarCambioValoresOperacion').on('click.cambioValores', '#btnConfirmarCambioValoresOperacion', function (event) {
        event.preventDefault();
        confirmarOperacionCambioValores();
    });

    $(document).off('click.cambioValores', '#btnGuardarDetalleCheque').on('click.cambioValores', '#btnGuardarDetalleCheque', function (event) {
        event.preventDefault();
        guardarDetalleChequeCambioValores();
    });

    $(document).off('click.cambioValores', '#btnGuardarDetalleTransferencia').on('click.cambioValores', '#btnGuardarDetalleTransferencia', function (event) {
        event.preventDefault();
        guardarDetalleTransferenciaCambioValores();
    });

    $('#modalDetalleCheque, #modalDetalleTransferencia').off('hidden.bs.modal.cambioValores').on('hidden.bs.modal.cambioValores', function () {
        limpiarValidacionesCambioValores($(this));
        contextoDetalleCambioValores = null;
        if (typeof ocultarTecladoVirtual === 'function') {
            ocultarTecladoVirtual();
        } else if (typeof cerrarTecladoVirtual === 'function') {
            cerrarTecladoVirtual();
        }
    });
}

function seleccionarTextoInicialCambioValores(input) {
    if (!input || input.dataset.cvSeleccionInicial === '1') return;
    input.dataset.cvSeleccionInicial = '1';

    // Solo se selecciona una vez por apertura del detalle. El teclado virtual puede
    // reenfocar el input al presionar teclas, y seleccionar siempre pisa lo tipeado.
    setTimeout(function () {
        if (document.activeElement === input && typeof input.select === 'function') {
            input.select();
        }
    }, 0);
}

function resetSeleccionInicialDetalleCambioValores(selectorModal) {
    const modal = document.querySelector(selectorModal);
    if (!modal) return;
    modal.querySelectorAll('input').forEach(function (input) {
        delete input.dataset.cvSeleccionInicial;
    });
}
let ultimoEnterDetalleCambioValores = 0;

function manejarEnterDetalleCambioValores(event) {
    const esEnter = event.key === 'Enter' || event.which === 13 || event.keyCode === 13;
    if (!esEnter) return;

    const ahora = Date.now();
    if (ahora - ultimoEnterDetalleCambioValores < 180) {
        event.preventDefault();
        return;
    }

    ultimoEnterDetalleCambioValores = ahora;
    event.preventDefault();
    event.stopPropagation();

    const id = event.currentTarget.id;
    const avanzar = function (selector) {
        setTimeout(function () {
            $(selector).trigger('focus').trigger('select');
        }, 40);
    };

    switch (id) {
        case 'txtNroCheque':
            avanzar('#txtFechaCheque');
            return;
        case 'txtFechaCheque':
            avanzar('#txtMontoCheque');
            return;
        case 'txtMontoCheque':
            $('#btnGuardarDetalleCheque').trigger('click');
            return;
        case 'txtNroTransferencia':
            avanzar('#txtFechaTransferencia');
            return;
        case 'txtFechaTransferencia':
            avanzar('#txtMontoTransferencia');
            return;
        case 'txtMontoTransferencia':
            $('#btnGuardarDetalleTransferencia').trigger('click');
            return;
        default:
            return;
    }
}
function validarClienteRegistradoCambioValores(cliente) {
    const ctaId = obtenerPrimerValorCambioValores(cliente?.cta_id, cliente?.id);
    const origen = obtenerPrimerValorCambioValores(cliente?.origen).toUpperCase();
    return origen !== 'F' && Boolean(ctaId);
}

function informarClienteNoValidoCambioValores(cliente) {
    clienteSeleccionadoCambioValores = null;
    mediosCambioValores = [];
    instrumentosCambioValores = [];
    valoresCambioValores = [];

    const nombre = escaparHtmlCambioValores(obtenerPrimerValorCambioValores(cliente?.denominacion, cliente?.cta_denominacion, cliente?.nombre, 'N/A'));
    const documento = escaparHtmlCambioValores(obtenerTipoNumeroClienteCambioValores(cliente) || 'N/A');
    const origen = escaparHtmlCambioValores(obtenerPrimerValorCambioValores(cliente?.origen_desc, cliente?.origen, 'Consumidor Final'));
    const ctaId = escaparHtmlCambioValores(obtenerPrimerValorCambioValores(cliente?.cta_id, cliente?.id, 'N/A'));

    if (typeof limpiarVista === 'function') limpiarVista();
    setTimeout(function () { $('#modalIdentificarCliente').modal('hide'); }, 50);

    const mensaje = `<div class="text-start">
        <p class="mb-2">El cliente seleccionado no puede utilizarse para <strong>Cambios e Ingresos de Valores</strong>.</p>
        <div class="border rounded p-2 bg-light mb-2">
            <div><strong>Cliente:</strong> ${nombre}</div>
            <div><strong>Documento:</strong> ${documento}</div>
            <div><strong>Origen:</strong> ${origen}</div>
            <div><strong>Cuenta:</strong> ${ctaId}</div>
        </div>
        <p class="mb-0 text-muted">Este proceso requiere un Cliente Registrado con identificador de cuenta valido.</p>
    </div>`;

    mostrarMensajeCambioValores('Cliente no habilitado', mensaje, 'warn!', function () { $('#modalIdentificarCliente').modal('show'); });
}

function reiniciarCambioValoresParaNuevoCliente() {
    clienteSeleccionadoCambioValores = null;
    mediosCambioValores = [];
    instrumentosCambioValores = [];
    valoresCambioValores = [];
    contextoDetalleCambioValores = null;

    $('#modalCambioValores, #modalCambioValoresConfirmacion, #modalDetalleCheque, #modalDetalleTransferencia').modal('hide');
    $('#chkIngresoValor').prop('checked', false);
    actualizarTipoOperacionCambioValores();

    $('#txtNombreCambioValores, #txtClienteIdCambioValores, #txtDomicilioCambioValores, #txtCondicionAfipCambioValores, #txtTipoNumeroCambioValores, #txtEmailCambioValores, #txtMovilCambioValores').val('');
    $('#cmbMedioCambioValores').html('<option value="">Seleccione...</option>');
    $('#cmbInstrumentoCambioValores').html('<option value="">Seleccione un medio</option>');
    limpiarCargaValorCambioValores();
    renderizarValoresCambioValores();

    if (typeof ocultarTecladoVirtual === 'function') {
        ocultarTecladoVirtual();
    } else if (typeof cerrarTecladoVirtual === 'function') {
        cerrarTecladoVirtual();
    }

    if (typeof limpiarVista === 'function') {
        limpiarVista();
    }

    if (typeof clienteCompleto !== 'undefined') {
        clienteCompleto = null;
    }
    if (typeof clienteActualFactura !== 'undefined') {
        clienteActualFactura = null;
    }

    setTimeout(function () {
        $('#modalIdentificarCliente').modal('show');
        $('#txtBuscarCliente, #txtBusquedaCliente, #BusquedaCliente, #criterioBusqueda').filter(':visible:first').trigger('focus');
    }, 250);
}
function abrirModalCambioValores(cliente) {
    poblarDatosClienteCambioValores(cliente);
    valoresCambioValores = [];
    renderizarValoresCambioValores();
    limpiarCargaValorCambioValores();
    $('#modalCambioValores').modal('show');
    cargarMediosPagoCambioValores();
}

function cargarMediosPagoCambioValores() {
    mostrarLoaderCambioValores('Consultando medios habilitados para el cliente...');
    $('#cmbMedioCambioValores').html('<option value="">Cargando...</option>');
    $('#cmbInstrumentoCambioValores').html('<option value="">Seleccione un medio</option>');

    $.ajax({
        url: cambioValoresMediosPagoUrl,
        type: 'POST',
        dataType: 'json',
        contentType: 'application/json; charset=utf-8',
        data: JSON.stringify({}),
        success: function (response) {
            ocultarLoaderCambioValores();
            console.log('[CambioValores] Response medios:', response);
            if (!response || response.ok !== true) {
                mediosCambioValores = [];
                renderizarMediosCambioValores();
                mostrarMensajeCambioValores('Atencion', response?.mensaje || 'No se pudieron obtener los medios habilitados.', 'warn!');
                return;
            }
            mediosCambioValores = Array.isArray(response.datos) ? response.datos : [];
            renderizarMediosCambioValores();
        },
        error: function (xhr, status, error) {
            ocultarLoaderCambioValores();
            console.error('[CambioValores] Error medios:', { status: xhr.status, responseText: xhr.responseText, error: error });
            mediosCambioValores = [];
            renderizarMediosCambioValores();
            mostrarMensajeCambioValores('Error', 'No se pudieron consultar los medios habilitados.', 'error!');
        }
    });
}

function renderizarMediosCambioValores() {
    const $combo = $('#cmbMedioCambioValores');
    $combo.empty();
    if (!mediosCambioValores.length) {
        $combo.append('<option value="">Sin medios habilitados</option>');
        return;
    }

    $combo.append('<option value="">Seleccione...</option>');
    mediosCambioValores.forEach(function (medio) {
        $combo.append(`<option value="${escaparAttrCambioValores(medio.tcf_id)}">${escaparHtmlCambioValores(medio.tcf_desc || medio.tcf_id)}</option>`);
    });
}

function cargarInstrumentosCambioValores(tcfId) {
    instrumentosCambioValores = [];
    $('#cmbInstrumentoCambioValores').html('<option value="">Seleccione...</option>');
    actualizarFormularioInstrumentoCambioValores();
    if (!tcfId) return;

    mostrarLoaderCambioValores('Consultando instrumentos habilitados...');
    $.ajax({
        url: cambioValoresInstrumentosUrl,
        type: 'POST',
        dataType: 'json',
        contentType: 'application/json; charset=utf-8',
        data: JSON.stringify({ tcf_id: tcfId }),
        success: function (response) {
            ocultarLoaderCambioValores();
            console.log('[CambioValores] Response instrumentos:', response);
            if (!response || response.ok !== true) {
                mostrarMensajeCambioValores('Atencion', response?.mensaje || 'No se pudieron obtener los instrumentos.', 'warn!');
                return;
            }
            instrumentosCambioValores = Array.isArray(response.datos) ? response.datos : [];
            renderizarInstrumentosCambioValores();
        },
        error: function (xhr, status, error) {
            ocultarLoaderCambioValores();
            console.error('[CambioValores] Error instrumentos:', { status: xhr.status, responseText: xhr.responseText, error: error });
            mostrarMensajeCambioValores('Error', 'No se pudieron consultar los instrumentos habilitados.', 'error!');
        }
    });
}

function renderizarInstrumentosCambioValores() {
    const $combo = $('#cmbInstrumentoCambioValores');
    $combo.empty();
    if (!instrumentosCambioValores.length) {
        $combo.append('<option value="">Sin instrumentos</option>');
        return;
    }

    $combo.append('<option value="">Seleccione...</option>');
    instrumentosCambioValores.forEach(function (instrumento) {
        $combo.append(`<option value="${escaparAttrCambioValores(instrumento.ins_id)}">${escaparHtmlCambioValores(instrumento.ins_desc || instrumento.ins_id)}</option>`);
    });
}

function actualizarFormularioInstrumentoCambioValores() {
    const medio = obtenerMedioSeleccionadoCambioValores();
    const tipo = obtenerTipoMedioCambioValores(medio);
    const usaModalDetalle = tipo === 'CH' || tipo === 'BA';
    const instrumento = obtenerInstrumentoSeleccionadoCambioValores();

    configurarDatoCambioValores(1, usaModalDetalle ? '' : instrumento?.ins_dato1_desc);
    configurarDatoCambioValores(2, usaModalDetalle ? '' : instrumento?.ins_dato2_desc);
    configurarDatoCambioValores(3, usaModalDetalle ? '' : instrumento?.ins_dato3_desc);

    const tieneVto = !usaModalDetalle && obtenerPrimerValorCambioValores(instrumento?.ins_tiene_vto).toUpperCase() === 'S';
    $('#grpFechaCambioValores').toggleClass('d-none', usaModalDetalle || !tieneVto);
    if (!tieneVto) setFechaValorActualCambioValores();

    $('#txtImporteCambioValores').prop('disabled', usaModalDetalle);
    if (usaModalDetalle) $('#txtImporteCambioValores').val('');
}

function configurarDatoCambioValores(numero, etiqueta) {
    const texto = obtenerPrimerValorCambioValores(etiqueta);
    $(`#grpDato${numero}CambioValores`).toggleClass('d-none', !texto);
    $(`#lblDato${numero}CambioValores`).text(texto || `Dato ${numero}`);
    if (!texto) $(`#txtDato${numero}CambioValores`).val('');
}

function agregarValorCambioValores() {
    const medio = obtenerMedioSeleccionadoCambioValores();
    const instrumento = obtenerInstrumentoSeleccionadoCambioValores();
    if (!medio) {
        mostrarMensajeCambioValores('Atencion', 'Debe seleccionar un medio habilitado.', 'warn!');
        return;
    }
    if (!instrumento) {
        mostrarMensajeCambioValores('Atencion', 'Debe seleccionar un instrumento.', 'warn!');
        return;
    }

    const tipoMedio = obtenerTipoMedioCambioValores(medio);
    if (tipoMedio === 'CH') {
        abrirModalChequeCambioValores(medio, instrumento);
        return;
    }
    if (tipoMedio === 'BA') {
        abrirModalTransferenciaCambioValores(medio, instrumento);
        return;
    }

    const importe = normalizarMontoCambioValores($('#txtImporteCambioValores').val());
    if (importe <= 0) {
        mostrarMensajeCambioValores('Atencion', 'Debe ingresar un importe mayor a cero.', 'warn!');
        $('#txtImporteCambioValores').focus();
        return;
    }

    const fechaValor = $('#txtFechaValorCambioValores').val() || obtenerFechaActualIsoCambioValores();
    agregarValorNormalizadoCambioValores({
        medio: medio,
        instrumento: instrumento,
        dato1: $('#txtDato1CambioValores').val() || '',
        dato2: $('#txtDato2CambioValores').val() || '',
        dato3: $('#txtDato3CambioValores').val() || '',
        fechaValor: fechaValor,
        importe: importe,
        observacion: [$('#txtDato1CambioValores').val(), $('#txtDato2CambioValores').val(), $('#txtDato3CambioValores').val()].filter(Boolean).join(' / ')
    });

    limpiarCargaValorCambioValores(true);
}

function abrirModalChequeCambioValores(medio, instrumento) {
    contextoDetalleCambioValores = { medio: medio, instrumento: instrumento };
    limpiarValidacionesCambioValores($('#modalDetalleCheque'));
    $('#lblTipoMedioPagoCheque').text(medio.tcf_desc || medio.tcf_id || 'Cheque');
    $('#lblInstrumentoCheque').text(instrumento.ins_desc || instrumento.ins_id || '-');
    $('#txtNroCheque').val('');
    $('#txtFechaCheque').val(obtenerFechaActualIsoCambioValores());
    $('#txtMontoCheque').val('');
    cargarBancosChequeCambioValores();
    resetSeleccionInicialDetalleCambioValores('#modalDetalleCheque');
    $('#modalDetalleCheque')
        .css('z-index', '5100')
        .off('shown.bs.modal.focusCambioValores')
        .one('shown.bs.modal.focusCambioValores', function () {
            $('.modal-backdrop').last().css('z-index', '5099');
            $('#modalDetalleCheque .modal-dialog').css('z-index', '5101');
            $('#txtNroCheque').trigger('focus');
        })
        .modal('show');
}

function cargarBancosChequeCambioValores() {
    const $combo = $('#selectBancoCheque');
    $combo.prop('disabled', true).html('<option value="">Cargando bancos...</option>');

    if (bancosCambioValores.length) {
        renderizarBancosChequeCambioValores();
        return;
    }

    $.ajax({
        url: cambioValoresBancosUrl,
        type: 'GET',
        dataType: 'json',
        success: function (response) {
            console.log('[CambioValores] Response bancos:', response);
            bancosCambioValores = response?.ok === true && Array.isArray(response.datos) ? response.datos : [];
            renderizarBancosChequeCambioValores();
            if (!bancosCambioValores.length) {
                mostrarMensajeCambioValores('Atencion', response?.mensaje || 'No se pudieron obtener los bancos.', 'warn!');
            }
        },
        error: function (xhr, status, error) {
            console.error('[CambioValores] Error bancos:', { status: xhr.status, responseText: xhr.responseText, error: error });
            bancosCambioValores = [];
            renderizarBancosChequeCambioValores();
            mostrarMensajeCambioValores('Error', 'No se pudieron consultar los bancos.', 'error!');
        }
    });
}

function renderizarBancosChequeCambioValores() {
    const $combo = $('#selectBancoCheque');
    $combo.empty();
    if (!bancosCambioValores.length) {
        $combo.append('<option value="">Sin bancos disponibles</option>').prop('disabled', true);
        return;
    }

    $combo.append('<option value="">Seleccione banco...</option>');
    bancosCambioValores.forEach(function (banco) {
        const id = obtenerPrimerValorCambioValores(banco.bc_id, banco.bancoId, banco.id);
        const desc = obtenerPrimerValorCambioValores(banco.bc_denominacion, banco.bancoDesc, banco.descripcion, id);
        const plaza = obtenerPrimerValorCambioValores(banco.bc_plaza, banco.plaza);
        $combo.append(`<option value="${escaparAttrCambioValores(id)}" data-plaza="${escaparAttrCambioValores(plaza)}">${escaparHtmlCambioValores(desc)}</option>`);
    });
    $combo.prop('disabled', false);
}

function guardarDetalleChequeCambioValores() {
    if (!contextoDetalleCambioValores) return;
    limpiarValidacionesCambioValores($('#modalDetalleCheque'));

    const bancoId = $('#selectBancoCheque').val();
    const bancoTexto = $('#selectBancoCheque option:selected').text();
    const plaza = obtenerPrimerValorCambioValores($('#selectBancoCheque option:selected').data('plaza'));
    const nroCheque = $('#txtNroCheque').val().trim();
    const fechaCheque = $('#txtFechaCheque').val();
    const monto = normalizarMontoCambioValores($('#txtMontoCheque').val());

    if (!bancoId) { mostrarErrorCampoCambioValores('#selectBancoCheque', 'Debe seleccionar un banco'); return; }
    if (!nroCheque) { mostrarErrorCampoCambioValores('#txtNroCheque', 'Debe ingresar el numero de cheque'); return; }
    if (!/^\d+$/.test(nroCheque)) { mostrarErrorCampoCambioValores('#txtNroCheque', 'El numero de cheque debe ser numerico'); return; }
    if (nroCheque.length > 8) { mostrarErrorCampoCambioValores('#txtNroCheque', 'El numero de cheque no puede superar 8 digitos'); return; }
    if (!fechaCheque) { mostrarErrorCampoCambioValores('#txtFechaCheque', 'Debe seleccionar la fecha del cheque'); return; }
    if (!fechaEnRangoChequeCambioValores(fechaCheque)) { mostrarErrorCampoCambioValores('#txtFechaCheque', 'La fecha debe estar entre hoy y 365 dias'); return; }
    if (monto <= 0) { mostrarErrorCampoCambioValores('#txtMontoCheque', 'Debe ingresar un monto mayor a cero'); return; }

    agregarValorNormalizadoCambioValores({
        medio: contextoDetalleCambioValores.medio,
        instrumento: contextoDetalleCambioValores.instrumento,
        dato1: bancoId,
        dato2: nroCheque.padStart(8, '0'),
        dato3: plaza,
        fechaValor: fechaCheque,
        importe: monto,
        observacion: `CH ${nroCheque.padStart(8, '0')} - ${bancoTexto}${plaza ? ' - Plaza ' + plaza : ''}`
    });

    $('#modalDetalleCheque').modal('hide');
    limpiarCargaValorCambioValores(true);
}

function abrirModalTransferenciaCambioValores(medio, instrumento) {
    contextoDetalleCambioValores = { medio: medio, instrumento: instrumento };
    limpiarValidacionesCambioValores($('#modalDetalleTransferencia'));
    $('#lblInstrumentoTransferencia').text(instrumento.ins_desc || instrumento.ins_id || '-');
    $('#hdnBancoIdTransferencia').val(instrumento.ins_id || '');
    $('#txtNroTransferencia').val('');
    $('#txtFechaTransferencia').val(obtenerFechaActualIsoCambioValores());
    $('#txtMontoTransferencia').val('');
    resetSeleccionInicialDetalleCambioValores('#modalDetalleTransferencia');
    $('#modalDetalleTransferencia')
        .off('shown.bs.modal.focusCambioValores')
        .one('shown.bs.modal.focusCambioValores', function () { $('#txtNroTransferencia').trigger('focus'); })
        .modal('show');
}

function guardarDetalleTransferenciaCambioValores() {
    if (!contextoDetalleCambioValores) return;
    limpiarValidacionesCambioValores($('#modalDetalleTransferencia'));

    const nroTransferencia = $('#txtNroTransferencia').val().trim().toUpperCase();
    const fechaTransferencia = $('#txtFechaTransferencia').val();
    const monto = normalizarMontoCambioValores($('#txtMontoTransferencia').val());

    if (!nroTransferencia || nroTransferencia.length < 5) { mostrarErrorCampoCambioValores('#txtNroTransferencia', 'Debe ingresar al menos 5 caracteres'); return; }
    if (!fechaTransferencia) { mostrarErrorCampoCambioValores('#txtFechaTransferencia', 'Debe seleccionar la fecha de la transferencia'); return; }
    if (!fechaTransferenciaValidaCambioValores(fechaTransferencia)) { mostrarErrorCampoCambioValores('#txtFechaTransferencia', 'La fecha no puede ser futura ni anterior a ayer'); return; }
    if (monto <= 0) { mostrarErrorCampoCambioValores('#txtMontoTransferencia', 'Debe ingresar un monto mayor a cero'); return; }

    agregarValorNormalizadoCambioValores({
        medio: contextoDetalleCambioValores.medio,
        instrumento: contextoDetalleCambioValores.instrumento,
        dato1: '',
        dato2: '',
        dato3: nroTransferencia.padStart(15, '0'),
        fechaValor: fechaTransferencia,
        importe: monto,
        observacion: `Transf ${nroTransferencia}`
    });

    $('#modalDetalleTransferencia').modal('hide');
    limpiarCargaValorCambioValores(true);
}

function agregarValorNormalizadoCambioValores(datos) {
    const valor = {
        idLocal: generarIdLocalCambioValores(),
        tcf_id: datos.medio.tcf_id || '',
        tcf_desc: datos.medio.tcf_desc || datos.medio.tcf_id || '',
        ins_desc: datos.instrumento.ins_desc || datos.instrumento.ins_id || '',
        observacion: datos.observacion || '',
        rb_nro_valor: String(valoresCambioValores.length + 1),
        ins_id: datos.instrumento.ins_id || '',
        rb_dato1_valor: datos.dato1 || '',
        rb_dato2_valor: datos.dato2 || '',
        rb_dato3_valor: datos.dato3 || '',
        rb_opcion_cuota: '0',
        rb_cupon_manual: 'N',
        rb_ch_dif: 'N',
        rb_fecha_valor: `${datos.fechaValor}T00:00:00`,
        rb_importe: Math.abs(normalizarMontoCambioValores(datos.importe)),
        rb_rec: 0,
        rb_aux: 0,
        rb_estado: 'N',
        id_externo: ''
    };

    valoresCambioValores.push(valor);
    console.log('[CambioValores] Valor agregado:', valor);
    renderizarValoresCambioValores();
}

function renderizarValoresCambioValores() {
    const $tbody = $('#tbodyCambioValores');
    $tbody.empty();
    $('#lblCambioValoresCount').text(valoresCambioValores.length);
    $('#btnFinalizarCambioValores').prop('disabled', valoresCambioValores.length === 0);

    if (!valoresCambioValores.length) {
        $tbody.append('<tr><td colspan="6" class="text-center text-muted py-4"><i class="bx bx-info-circle"></i> Sin valores cargados.</td></tr>');
        actualizarTotalCambioValores();
        return;
    }

    valoresCambioValores.forEach(function (valor, index) {
        const detalle = valor.observacion || [valor.rb_dato1_valor, valor.rb_dato2_valor, valor.rb_dato3_valor].filter(Boolean).join(' / ');
        $tbody.append(`
            <tr>
                <td class="fw-bold">${escaparHtmlCambioValores(valor.tcf_desc)}</td>
                <td>${escaparHtmlCambioValores(valor.ins_desc)}</td>
                <td>${escaparHtmlCambioValores(detalle || '-')}</td>
                <td class="text-center">${escaparHtmlCambioValores(formatearFechaCambioValores(valor.rb_fecha_valor))}</td>
                <td class="text-end fw-bold text-success">$ ${formatearMontoCambioValores(valor.rb_importe)}</td>
                <td class="text-center"><button type="button" class="btn btn-danger btn-sm btn-eliminar-cambio-valores" data-index="${index}" title="Eliminar valor"><i class="bx bx-trash"></i></button></td>
            </tr>`);
    });

    actualizarTotalCambioValores();
}

function eliminarValorCambioValores(index) {
    if (!Number.isInteger(index) || index < 0 || index >= valoresCambioValores.length) return;
    valoresCambioValores.splice(index, 1);
    renderizarValoresCambioValores();
}

function prepararConfirmacionCambioValores() {
    if (finalizandoCambioValores) return;
    if (!valoresCambioValores.length) {
        mostrarMensajeCambioValores('Atencion', 'Debe cargar al menos un valor antes de finalizar.', 'warn!');
        return;
    }

    renderizarConfirmacionCambioValores();
    $('#modalCambioValores').modal('hide');
    setTimeout(function () { $('#modalCambioValoresConfirmacion').modal('show'); }, 250);
}

function renderizarConfirmacionCambioValores() {
    const tipo = $('#chkIngresoValor').is(':checked') ? 'IV' : 'CV';
    const total = calcularTotalCambioValores();
    const $tbody = $('#tbodyCambioValoresConfirmacion').empty();

    valoresCambioValores.forEach(function (valor) {
        const detalle = valor.observacion || [valor.rb_dato1_valor, valor.rb_dato2_valor, valor.rb_dato3_valor].filter(Boolean).join(' / ');
        $tbody.append(`
            <tr>
                <td class="fw-bold">${escaparHtmlCambioValores(valor.tcf_desc)}</td>
                <td>${escaparHtmlCambioValores(valor.ins_desc)}</td>
                <td>${escaparHtmlCambioValores(detalle || '-')}</td>
                <td class="text-center">${escaparHtmlCambioValores(formatearFechaCambioValores(valor.rb_fecha_valor))}</td>
                <td class="text-end fw-bold text-success">$ ${formatearMontoCambioValores(valor.rb_importe)}</td>
            </tr>`);
    });

    $('#lblTotalCambioValoresConfirmacion').text(`$ ${formatearMontoCambioValores(total)}`);
    $('#lblOperacionCambioValoresConfirmacion').text(tipo === 'IV' ? 'Ingreso de Valor' : 'Cambio de Valor');
    $('#lblClienteCambioValoresConfirmacion').text(obtenerPrimerValorCambioValores(clienteSeleccionadoCambioValores?.denominacion, clienteSeleccionadoCambioValores?.cta_denominacion, clienteSeleccionadoCambioValores?.nombre, 'N/A'));
    $('#lblCuentaCambioValoresConfirmacion').text(obtenerPrimerValorCambioValores(clienteSeleccionadoCambioValores?.cta_id, clienteSeleccionadoCambioValores?.id, 'N/A'));
    $('#lblContrapartidaCambioValoresConfirmacion')
        .toggleClass('alert-warning', tipo === 'CV')
        .toggleClass('alert-success', tipo === 'IV')
        .html(tipo === 'CV'
            ? `Se entregara efectivo en pesos por <strong>$ ${formatearMontoCambioValores(total)}</strong>. El sistema enviara la contrapartida PES en negativo.`
            : 'Solo se registrara el ingreso de los valores a la caja.');
}

function confirmarOperacionCambioValores() {
    if (finalizandoCambioValores) return;
    finalizandoCambioValores = true;

    const tipo = $('#chkIngresoValor').is(':checked') ? 'IV' : 'CV';
    const payload = { tipo: tipo, valores: construirValoresBackendCambioValores() };
    console.log('[CambioValores] Confirmando operacion:', payload);

    $('#btnConfirmarCambioValoresOperacion, #btnVolverCambioValoresConfirmacion').prop('disabled', true);
    mostrarLoaderCambioValores('Confirmando operacion. Espere un momento...');

    $.ajax({
        url: cambioValoresConfirmarOperacionUrl,
        type: 'POST',
        dataType: 'json',
        contentType: 'application/json; charset=utf-8',
        data: JSON.stringify(payload),
        success: function (response) {
            ocultarLoaderCambioValores();
            console.log('[CambioValores] Response confirmar operacion:', response);
            if (!response || response.ok !== true) {
                mostrarMensajeCambioValores('Atencion', response?.mensaje || 'No se pudo confirmar la operacion.', 'warn!');
                return;
            }

            $('#modalCambioValoresConfirmacion').modal('hide');
            mostrarMensajeCambioValores('Operacion Confirmada', response.mensaje || 'La operacion fue confirmada correctamente.', 'succ!', function () {
                reiniciarCambioValoresParaNuevoCliente();
            });
        },
        error: function (xhr, status, error) {
            ocultarLoaderCambioValores();
            console.error('[CambioValores] Error confirmar operacion:', { status: xhr.status, responseText: xhr.responseText, error: error });
            mostrarMensajeCambioValores('Error', 'No se pudo confirmar la operacion de valores.', 'error!');
        },
        complete: function () {
            finalizandoCambioValores = false;
            $('#btnConfirmarCambioValoresOperacion, #btnVolverCambioValoresConfirmacion').prop('disabled', false);
        }
    });
}

function construirValoresBackendCambioValores() {
    return valoresCambioValores.map(function (valor, index) {
        return {
            rb_nro_valor: String(index + 1).padStart(3, '0'),
            ins_id: valor.ins_id,
            rb_dato1_valor: valor.rb_dato1_valor,
            rb_dato2_valor: valor.rb_dato2_valor,
            rb_dato3_valor: valor.rb_dato3_valor,
            rb_opcion_cuota: valor.rb_opcion_cuota,
            rb_cupon_manual: valor.rb_cupon_manual,
            rb_ch_dif: valor.rb_ch_dif,
            rb_fecha_valor: valor.rb_fecha_valor,
            rb_importe: Math.abs(normalizarMontoCambioValores(valor.rb_importe)),
            rb_rec: valor.rb_rec,
            rb_aux: valor.rb_aux,
            rb_estado: valor.rb_estado,
            id_externo: valor.id_externo
        };
    });
}

function poblarDatosClienteCambioValores(cliente) {
    $('#txtNombreCambioValores').val(obtenerPrimerValorCambioValores(cliente?.denominacion, cliente?.cta_denominacion, cliente?.nombre));
    $('#txtClienteIdCambioValores').val(obtenerPrimerValorCambioValores(cliente?.cta_id, cliente?.id));
    $('#txtDomicilioCambioValores').val(obtenerPrimerValorCambioValores(cliente?.domicilio, cliente?.cta_domicilio));
    $('#txtCondicionAfipCambioValores').val(obtenerPrimerValorCambioValores(cliente?.condicionAfip, cliente?.afip_desc, cliente?.afip_id));
    $('#txtTipoNumeroCambioValores').val(obtenerTipoNumeroClienteCambioValores(cliente));
    $('#txtEmailCambioValores').val(obtenerPrimerValorCambioValores(cliente?.email, cliente?.cta_email));
    $('#txtMovilCambioValores').val(obtenerPrimerValorCambioValores(cliente?.movil, cliente?.cta_celu));
}

function actualizarTipoOperacionCambioValores() {
    const esIngreso = $('#chkIngresoValor').is(':checked');
    $('#lblTipoOperacionCambioValores')
        .toggleClass('alert-warning', !esIngreso)
        .toggleClass('alert-success', esIngreso)
        .text(esIngreso ? 'Ingreso de Valor: solo se registra la entrada del valor a la caja.' : 'Cambio de Valor: se entregara efectivo en pesos contra el valor recibido.');
}

function limpiarCargaValorCambioValores(mantenerMedio) {
    if (!mantenerMedio) {
        $('#cmbMedioCambioValores').val('');
        $('#cmbInstrumentoCambioValores').html('<option value="">Seleccione un medio</option>');
    }
    $('#cmbInstrumentoCambioValores').val('');
    $('#txtDato1CambioValores, #txtDato2CambioValores, #txtDato3CambioValores, #txtImporteCambioValores').val('').prop('disabled', false);
    setFechaValorActualCambioValores();
    actualizarFormularioInstrumentoCambioValores();
}

function obtenerMedioSeleccionadoCambioValores() {
    const tcfId = $('#cmbMedioCambioValores').val();
    return mediosCambioValores.find(x => String(x.tcf_id || '') === String(tcfId || '')) || null;
}

function obtenerInstrumentoSeleccionadoCambioValores() {
    const insId = $('#cmbInstrumentoCambioValores').val();
    return instrumentosCambioValores.find(x => String(x.ins_id || '') === String(insId || '')) || null;
}

function obtenerTipoMedioCambioValores(medio) {
    return obtenerPrimerValorCambioValores(medio?.tcf_id, medio?.id).toUpperCase();
}

function actualizarTotalCambioValores() {
    $('#lblTotalCambioValores').text(`$ ${formatearMontoCambioValores(calcularTotalCambioValores())}`);
}

function calcularTotalCambioValores() {
    return valoresCambioValores.reduce(function (total, valor) { return total + normalizarMontoCambioValores(valor.rb_importe); }, 0);
}

function setFechaValorActualCambioValores() {
    $('#txtFechaValorCambioValores').val(obtenerFechaActualIsoCambioValores());
}

function obtenerFechaActualIsoCambioValores() {
    const fecha = new Date();
    return `${fecha.getFullYear()}-${String(fecha.getMonth() + 1).padStart(2, '0')}-${String(fecha.getDate()).padStart(2, '0')}`;
}

function generarIdLocalCambioValores() {
    return `CV-${Date.now()}-${Math.floor(Math.random() * 100000)}`;
}

function obtenerTipoNumeroClienteCambioValores(cliente) {
    const tipoNumero = obtenerPrimerValorCambioValores(cliente?.tipoNumero);
    if (tipoNumero) return tipoNumero;
    return [obtenerPrimerValorCambioValores(cliente?.tdoc_desc, cliente?.tipoDocumento), obtenerPrimerValorCambioValores(cliente?.documento, cliente?.nro_documento)].filter(Boolean).join(' ');
}

function obtenerPrimerValorCambioValores() {
    const valores = Array.from(arguments);
    for (let i = 0; i < valores.length; i++) {
        const valor = valores[i];
        if (valor !== null && valor !== undefined) {
            const texto = String(valor).trim();
            if (texto && texto.toLowerCase() !== 'null' && texto.toLowerCase() !== 'undefined') return texto;
        }
    }
    return '';
}

function normalizarMontoCambioValores(valor) {
    if (typeof valor === 'number') return Number.isFinite(valor) ? Math.round((valor + Number.EPSILON) * 100) / 100 : 0;
    let texto = String(valor ?? '').trim();
    if (!texto) return 0;
    if (texto.includes(',') && texto.includes('.')) texto = texto.replace(/,/g, '');
    else if (texto.includes(',') && !texto.includes('.')) texto = texto.replace(',', '.');
    const numero = Number(texto);
    return Number.isFinite(numero) ? Math.round((numero + Number.EPSILON) * 100) / 100 : 0;
}

function formatearMontoCambioValores(monto) {
    if (typeof formatearNumero === 'function') return formatearNumero(normalizarMontoCambioValores(monto), 2);
    return new Intl.NumberFormat('en-US', { minimumFractionDigits: 2, maximumFractionDigits: 2 }).format(normalizarMontoCambioValores(monto));
}

function formatearFechaCambioValores(valor) {
    if (!valor) return 'N/A';
    const texto = String(valor);
    const coincidencia = texto.match(/^(\d{4})-(\d{2})-(\d{2})/);
    if (coincidencia) return `${coincidencia[3]}/${coincidencia[2]}/${coincidencia[1]}`;
    return texto;
}

function fechaEnRangoChequeCambioValores(fechaIso) {
    const fecha = new Date(`${fechaIso}T00:00:00`);
    const hoy = new Date(`${obtenerFechaActualIsoCambioValores()}T00:00:00`);
    const max = new Date(hoy);
    max.setDate(max.getDate() + 365);
    return fecha >= hoy && fecha <= max;
}

function fechaTransferenciaValidaCambioValores(fechaIso) {
    const fecha = new Date(`${fechaIso}T00:00:00`);
    const hoy = new Date(`${obtenerFechaActualIsoCambioValores()}T00:00:00`);
    const ayer = new Date(hoy);
    ayer.setDate(ayer.getDate() - 1);
    return fecha >= ayer && fecha <= hoy;
}

function limpiarValidacionesCambioValores($scope) {
    const $root = $scope && $scope.length ? $scope : $(document);
    $root.find('.is-invalid, .is-valid').removeClass('is-invalid is-valid');
    $root.find('.invalid-feedback').remove();
}

function mostrarErrorCampoCambioValores(selector, mensaje) {
    const $campo = $(selector);
    $campo.addClass('is-invalid').removeClass('is-valid');
    $campo.siblings('.invalid-feedback').remove();
    $campo.closest('.input-group').siblings('.invalid-feedback').remove();
    const feedback = `<div class="invalid-feedback d-block">${escaparHtmlCambioValores(mensaje)}</div>`;
    const $grupo = $campo.closest('.input-group');
    if ($grupo.length) $grupo.after(feedback);
    else $campo.after(feedback);
    $campo.trigger('focus');
}

function escaparHtmlCambioValores(valor) {
    return String(valor ?? '').replace(/&/g, '&amp;').replace(/</g, '&lt;').replace(/>/g, '&gt;').replace(/"/g, '&quot;').replace(/'/g, '&#039;');
}

function escaparAttrCambioValores(valor) {
    return escaparHtmlCambioValores(valor).replace(/`/g, '&#096;');
}

function mostrarLoaderCambioValores(mensaje) {
    if (typeof mostrarLoader === 'function') { mostrarLoader(mensaje); return; }
    if (typeof AbrirWaiting === 'function') AbrirWaiting(mensaje);
}

function ocultarLoaderCambioValores() {
    if (typeof ocultarLoader === 'function') { ocultarLoader(); return; }
    if (typeof CerrarWaiting === 'function') CerrarWaiting(false);
}

function mostrarMensajeCambioValores(titulo, mensaje, tipo, callback) {
    if (typeof AbrirMensaje === 'function') {
        AbrirMensaje(titulo, mensaje, function () {
            $('#msjModal').modal('hide');
            if (typeof callback === 'function') setTimeout(callback, 250);
        }, false, ['Aceptar'], tipo || 'warn!', null);
        return;
    }
    window.alert(`${titulo}: ${String(mensaje).replace(/<br>/g, '\n')}`);
    if (typeof callback === 'function') callback();
}







