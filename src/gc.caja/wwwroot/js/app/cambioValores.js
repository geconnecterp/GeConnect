// ========================================================
// GESTOR DEL MODULO CAMBIOS E INGRESOS DE VALORES
// ========================================================

let clienteSeleccionadoCambioValores = null;
let mediosCambioValores = [];
let instrumentosCambioValores = [];
let valoresCambioValores = [];
let finalizandoCambioValores = false;

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
    $(document).off('focus.cambioValores', '#txtImporteCambioValores').on('focus.cambioValores', '#txtImporteCambioValores', function () { this.select(); });
    $(document).off('input.cambioValores', '#txtImporteCambioValores').on('input.cambioValores', '#txtImporteCambioValores', function () { this.value = this.value.replace(/[^0-9.,]/g, ''); });
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
    const instrumento = obtenerInstrumentoSeleccionadoCambioValores();
    configurarDatoCambioValores(1, instrumento?.ins_dato1_desc);
    configurarDatoCambioValores(2, instrumento?.ins_dato2_desc);
    configurarDatoCambioValores(3, instrumento?.ins_dato3_desc);
    const tieneVto = obtenerPrimerValorCambioValores(instrumento?.ins_tiene_vto).toUpperCase() === 'S';
    $('#grpFechaCambioValores').toggleClass('d-none', !tieneVto);
    if (!tieneVto) setFechaValorActualCambioValores();
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

    const importe = normalizarMontoCambioValores($('#txtImporteCambioValores').val());
    if (importe <= 0) {
        mostrarMensajeCambioValores('Atencion', 'Debe ingresar un importe mayor a cero.', 'warn!');
        $('#txtImporteCambioValores').focus();
        return;
    }

    const fechaValor = $('#txtFechaValorCambioValores').val() || obtenerFechaActualIsoCambioValores();
    const valor = {
        idLocal: generarIdLocalCambioValores(),
        tcf_id: medio.tcf_id || '',
        tcf_desc: medio.tcf_desc || medio.tcf_id || '',
        ins_desc: instrumento.ins_desc || instrumento.ins_id || '',
        rb_nro_valor: String(valoresCambioValores.length + 1),
        ins_id: instrumento.ins_id || '',
        rb_dato1_valor: $('#txtDato1CambioValores').val() || '',
        rb_dato2_valor: $('#txtDato2CambioValores').val() || '',
        rb_dato3_valor: $('#txtDato3CambioValores').val() || '',
        rb_opcion_cuota: '0',
        rb_cupon_manual: 'N',
        rb_ch_dif: 'N',
        rb_fecha_valor: `${fechaValor}T00:00:00`,
        rb_importe: importe,
        rb_rec: 0,
        rb_aux: 0,
        rb_estado: 'N',
        id_externo: ''
    };

    valoresCambioValores.push(valor);
    console.log('[CambioValores] Valor agregado:', valor);
    limpiarCargaValorCambioValores(true);
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
        const detalle = [valor.rb_dato1_valor, valor.rb_dato2_valor, valor.rb_dato3_valor].filter(Boolean).join(' / ');
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

    const tipo = $('#chkIngresoValor').is(':checked') ? 'IV' : 'CV';
    const valoresBackend = valoresCambioValores.map(function (valor) {
        return {
            rb_nro_valor: valor.rb_nro_valor,
            ins_id: valor.ins_id,
            rb_dato1_valor: valor.rb_dato1_valor,
            rb_dato2_valor: valor.rb_dato2_valor,
            rb_dato3_valor: valor.rb_dato3_valor,
            rb_opcion_cuota: valor.rb_opcion_cuota,
            rb_cupon_manual: valor.rb_cupon_manual,
            rb_ch_dif: valor.rb_ch_dif,
            rb_fecha_valor: valor.rb_fecha_valor,
            rb_importe: valor.rb_importe,
            rb_rec: valor.rb_rec,
            rb_aux: valor.rb_aux,
            rb_estado: valor.rb_estado,
            id_externo: valor.id_externo
        };
    });

    const payload = { tipo: tipo, valores: valoresBackend };
    console.log('[CambioValores] Payload preparado:', payload);
    finalizandoCambioValores = true;
    mostrarLoaderCambioValores('Preparando datos de valores...');

    $.ajax({
        url: cambioValoresPrepararConfirmacionUrl,
        type: 'POST',
        dataType: 'json',
        contentType: 'application/json; charset=utf-8',
        data: JSON.stringify(payload),
        success: function (response) {
            ocultarLoaderCambioValores();
            console.log('[CambioValores] Response preparar confirmacion:', response);
            const total = formatearMontoCambioValores(response?.total || calcularTotalCambioValores());
            mostrarMensajeCambioValores(response?.pendiente ? 'Confirmacion pendiente' : 'Atencion', `${escaparHtmlCambioValores(response?.mensaje || 'La confirmacion todavia no esta disponible.')}<br><br><strong>Tipo:</strong> ${escaparHtmlCambioValores(tipo)}<br><strong>Total:</strong> $ ${total}`, response?.pendiente ? 'info!' : 'warn!');
        },
        error: function (xhr, status, error) {
            ocultarLoaderCambioValores();
            console.error('[CambioValores] Error preparar confirmacion:', { status: xhr.status, responseText: xhr.responseText, error: error });
            mostrarMensajeCambioValores('Error', 'No se pudo preparar la confirmacion de valores.', 'error!');
        },
        complete: function () { finalizandoCambioValores = false; }
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
    $('#txtDato1CambioValores, #txtDato2CambioValores, #txtDato3CambioValores, #txtImporteCambioValores').val('');
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
