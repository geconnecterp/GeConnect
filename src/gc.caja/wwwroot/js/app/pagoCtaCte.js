// ========================================================
// GESTOR PRINCIPAL DEL MÓDULO DE COBRANZA CUENTA CORRIENTE
// ========================================================
// Regla de importes:
// - cv_importe / data-importe: saldo actual y valor editable a cobrar.
// - cv_importe_ori / data-importe-ori: importe histórico original. No se modifica.
// - data-importe-bak: saldo original disponible al cargar la grilla.
//                       Se utiliza para restaurar y como máximo permitido.
// ========================================================

let clienteSeleccionadoCC = null;
let nombreClienteCC = '';
let $filaImporteCCEnEdicion = null;
let cobranzaCCEnCurso = false;

$(function () {
    console.log('═══════════════════════════════════════════════════');
    console.log('🚀 MÓDULO DE COBRANZA CUENTA CORRIENTE CARGADO');
    console.log('═══════════════════════════════════════════════════');

    registrarEventosCuentaCorriente();

    $(document)
        .off('clienteConfirmado.ccPendientes')
        .on('clienteConfirmado.ccPendientes', function (event, cliente) {
            if (!cliente) {
                console.warn('⚠️ Se recibió clienteConfirmado sin datos de cliente.');
                return;
            }

            clienteSeleccionadoCC = cliente;
            nombreClienteCC = obtenerPrimerValorCC(
                cliente.denominacion,
                cliente.cta_denominacion,
                cliente.nombre
            );

            $('#modalIdentificarCliente').modal('hide');

            obtenerDatosCCDesdeMemoria(cliente);
        });

    inicializarModuloConModal();
});

// ================================================================
// INICIALIZACIÓN Y EVENTOS
// ================================================================

function inicializarModuloConModal() {
    if (typeof inicializaVistaFact !== 'function') {
        console.error('❌ No está disponible inicializaVistaFact() de fact.js.');
        mostrarMensajeCC(
            'Error',
            'No se pudo inicializar el buscador de clientes.',
            'error!'
        );
        return;
    }

    setTimeout(function () {
        inicializaVistaFact();
    }, 300);
}

function registrarEventosCuentaCorriente() {
    $(document)
        .off('change.ccPendientes', '#chkSeleccionarTodoCC')
        .on('change.ccPendientes', '#chkSeleccionarTodoCC', function () {
            seleccionarTodosMovimientosCC($(this).is(':checked'));
        });

    $(document)
        .off(
            'change.ccPendientes',
            '#tbodyCuentaCorriente input.form-check-input[type="checkbox"]'
        )
        .on(
            'change.ccPendientes',
            '#tbodyCuentaCorriente input.form-check-input[type="checkbox"]',
            function () {
                actualizarEstadoSeleccionCC();
            }
        );

    $(document)
        .off(
            'click.ccPendientes',
            '#tbodyCuentaCorriente .btn-modificar-importe-cc'
        )
        .on(
            'click.ccPendientes',
            '#tbodyCuentaCorriente .btn-modificar-importe-cc',
            function (event) {
                event.preventDefault();
                event.stopPropagation();

                abrirModalImporteCC($(this).closest('tr'));
            }
        );

    $(document)
        .off(
            'click.ccPendientes',
            '#tbodyCuentaCorriente .btn-restaurar-importe-cc'
        )
        .on(
            'click.ccPendientes',
            '#tbodyCuentaCorriente .btn-restaurar-importe-cc',
            function (event) {
                event.preventDefault();
                event.stopPropagation();

                restaurarImporteDesdeBackupCC($(this).closest('tr'));
            }
        );

    // Modal definitivo recomendado.
    $(document)
        .off('click.ccPendientes', '#btnGuardarImporteCC')
        .on('click.ccPendientes', '#btnGuardarImporteCC', function (event) {
            event.preventDefault();
            guardarImporteCC();
        });

    $(document)
        .off('submit.ccPendientes', '#formModificarImporteCC')
        .on('submit.ccPendientes', '#formModificarImporteCC', function (event) {
            event.preventDefault();
            guardarImporteCC();
        });

    // Compatibilidad temporal mientras siga cargado el modal antiguo
    // _detalleImputaCuentaCorriente.cshtml.
    $(document)
        .off('click.ccPendientes', '#btnGuardarImputaCC')
        .on('click.ccPendientes', '#btnGuardarImputaCC', function (event) {
            event.preventDefault();
            guardarImporteCC();
        });

    $(document)
        .off('submit.ccPendientes', '#formDetalleImputaCC')
        .on('submit.ccPendientes', '#formDetalleImputaCC', function (event) {
            event.preventDefault();
            guardarImporteCC();
        });

    $('#modalModificarImporteCC, #modalDetalleImputaCC')
        .off('hidden.bs.modal.ccPendientes')
        .on('hidden.bs.modal.ccPendientes', function () {
            $filaImporteCCEnEdicion = null;
        });

    $('#btnCancelarCC')
        .off('click.ccPendientes')
        .on('click.ccPendientes', function (event) {
            event.preventDefault();

            $('#modalCuentaCorriente').modal('hide');

            setTimeout(function () {
                $('#modalIdentificarCliente').modal('show');
            }, 350);
        });

    $(document)
        .off('click.ccPendientes', '#btnSeguirCC')
        .on('click.ccPendientes', '#btnSeguirCC', function (event) {
            event.preventDefault();
            event.stopPropagation();

            iniciarCobranzaCuentaCorriente();
        });

    $('#modalModificarImporteCC')
        .off('hidden.bs.modal.tecladoCC')
        .on('hidden.bs.modal.tecladoCC', function () {
            if (typeof ocultarTecladoVirtual === 'function') {
                ocultarTecladoVirtual();
            }

            $('#txtImportePagarCC').trigger('blur');

            $filaImporteCCEnEdicion = null;
        });
}

// ================================================================
// CARGA DE DATOS Y GRILLA
// ================================================================

function obtenerDatosCCDesdeMemoria(cliente) {
    if (!cliente) {
        mostrarMensajeCC(
            'Atención',
            'No se pudo determinar el cliente seleccionado.',
            'warn!'
        );
        return;
    }

    const ctaId = obtenerPrimerValorCC(cliente.cta_id, cliente.id);

    if (!ctaId) {
        mostrarMensajeCC(
            'Atención',
            'El cliente seleccionado no posee un identificador de cuenta válido.',
            'warn!'
        );
        return;
    }

    mostrarLoaderCC('Buscando datos en Cuenta Corriente...');

    $.ajax({
        url: obtenerCtaCteUrl,
        type: 'POST',
        contentType: 'application/json',
        dataType: 'json',
        data: JSON.stringify({ cta_id: ctaId }),
        success: function (response) {
            ocultarLoaderCC();

            if (!response || !response.ok) {
                mostrarMensajeCC(
                    'Información',
                    response?.mensaje ||
                    'No se encontraron registros de Cuenta Corriente para este cliente.',
                    'info!'
                );
                return;
            }

            mostrarCtaCtePendientes(cliente, response.lista || []);
        },
        error: function (xhr, status, error) {
            ocultarLoaderCC();

            console.error('❌ Error obteniendo Cuenta Corriente:', {
                status: xhr.status,
                statusText: xhr.statusText,
                responseText: xhr.responseText,
                error: error
            });

            let mensaje = 'No se pudieron obtener los registros de Cuenta Corriente.';

            if (xhr.responseJSON?.mensaje) {
                mensaje = xhr.responseJSON.mensaje;
            } else if (xhr.status === 400) {
                mensaje = 'Los datos enviados para consultar la cuenta son inválidos.';
            } else if (xhr.status === 0) {
                mensaje = 'No se pudo establecer conexión con el servidor.';
            }

            mostrarMensajeCC('Error de Comunicación', mensaje, 'error!');
        }
    });
}

function mostrarCtaCtePendientes(cliente, registrosCuentaCorriente) {
    const $tbody = $('#tbodyCuentaCorriente');

    poblarDatosClienteCC(cliente);

    $tbody.empty();
    $('#chkSeleccionarTodoCC').prop({
        checked: false,
        indeterminate: false
    });

    if (
        !Array.isArray(registrosCuentaCorriente) ||
        registrosCuentaCorriente.length === 0
    ) {
        $tbody.append(
            '<tr>' +
            '<td colspan="9" class="text-center text-muted py-4">' +
            'No hay registros pendientes de Cuenta Corriente.' +
            '</td>' +
            '</tr>'
        );

        $('#txtTotalCC').val('$ 0.00');
        $('#btnSeguirCC').prop('disabled', true);
        $('#modalCuentaCorriente').modal('show');
        return;
    }

    let registrosRenderizados = 0;

    registrosCuentaCorriente.forEach(function (ctacte, index) {
        try {
            const importeActual = normalizarMontoCC(ctacte.cv_importe);
            const importeOriginal = normalizarMontoCC(ctacte.cv_importe_ori);

            // El backup es el saldo disponible recibido al cargar esta grilla.
            // Nunca cambia durante la edición del usuario.
            const importeBackup = importeActual;

            if (
                !Number.isFinite(importeActual) ||
                importeActual <= 0 ||
                !Number.isFinite(importeOriginal) ||
                importeOriginal <= 0 ||
                importeOriginal + 0.01 < importeActual
            ) {
                console.warn(
                    `⚠️ Registro ${index + 1} omitido por importes inconsistentes.`,
                    ctacte
                );
                return;
            }

            const ctaId = obtenerPrimerValorCC(
                ctacte.cta_id,
                cliente?.cta_id,
                cliente?.id
            );

            const ctacteId = obtenerPrimerValorCC(
                ctacte.ctacte,
                ctacte.ctacte_id
            );

            const nombreCliente = obtenerPrimerValorCC(
                cliente?.denominacion,
                cliente?.cta_denominacion,
                cliente?.nombre,
                nombreClienteCC,
                'N/A'
            );

            const nombreMostrado = ctaId
                ? `${nombreCliente} (${ctaId})`
                : nombreCliente;

            const tipoNumero = obtenerTipoNumeroClienteCC(cliente);
            const fecha = formatearFechaCC(ctacte.cv_fecha_vto);

            const fila = `
                <tr data-importe="${importeActual.toFixed(2)}"
                    data-importe-ori="${importeOriginal.toFixed(2)}"
                    data-importe-bak="${importeBackup.toFixed(2)}">

                    <td>${escaparHtmlCC(ctacte.tco_id || 'N/A')}</td>
                    <td>${escaparHtmlCC(ctacte.cm_compte || 'N/A')}</td>
                    <td>${escaparHtmlCC(nombreMostrado)}</td>
                    <td>${escaparHtmlCC(tipoNumero)}</td>
                    <td class="text-center">${escaparHtmlCC(fecha)}</td>

                    <td class="text-end fw-bold celda-importe-ori-cc">
                        $ ${formatearMontoCC(importeOriginal)}
                    </td>

                    <td class="text-end fw-bold celda-importe-cc">
                        $ ${formatearMontoCC(importeActual)}
                    </td>

                    <td class="text-center py-1">
                        <button type="button"
                                class="btn btn-xs btn-success btn-modificar-importe-cc btn-accion-importe-cc"
                                title="Modificar importe a cobrar"
                                aria-label="Modificar importe a cobrar">
                            <i class="bx bx-edit-alt" style="font-size: 0.9rem;"></i>
                        </button>

                        <button type="button"
                                class="btn btn-xs btn-danger btn-restaurar-importe-cc btn-accion-importe-cc ms-1"
                                title="Restaurar saldo disponible"
                                aria-label="Restaurar saldo disponible">
                            <i class="bx bx-undo" style="font-size: 0.9rem;"></i>
                        </button>
                    </td>

                    <td class="text-center">
                        <input type="checkbox"
                               class="form-check-input"

                               data-cta-id="${escaparHtmlCC(ctaId)}"
                               data-dia-movi="${escaparHtmlCC(ctacte.dia_movi)}"
                               data-tco-id="${escaparHtmlCC(ctacte.tco_id)}"
                               data-cm-compte="${escaparHtmlCC(ctacte.cm_compte)}"
                               data-cm-compte-cuota="${escaparHtmlCC(ctacte.cm_compte_cuota ?? 0)}"
                               data-cv-fecha-vto="${escaparHtmlCC(ctacte.cv_fecha_vto)}"
                               data-cv-importe="${importeActual.toFixed(2)}"
                               data-cv-importe-ori="${importeOriginal.toFixed(2)}"
                               data-cv-concepto="${escaparHtmlCC(ctacte.cv_concepto)}"
                               data-ve-id="${escaparHtmlCC(ctacte.ve_id)}"
                               data-ccb-id="${escaparHtmlCC(ctacte.ccb_id)}"
                               data-ctacte="${escaparHtmlCC(ctacteId)}"
                               data-carga="${escaparHtmlCC(ctacte.carga)}"
                               data-carga-obligatoria="${escaparHtmlCC(ctacte.carga_obligatoria)}">
                    </td>
                </tr>
            `;

            $tbody.append(fila);
            registrosRenderizados++;
        } catch (error) {
            console.error(
                `❌ Error al renderizar registro de Cuenta Corriente ${index + 1}.`,
                error,
                ctacte
            );
        }
    });

    if (registrosRenderizados === 0) {
        $tbody.html(
            '<tr>' +
            '<td colspan="9" class="text-center text-muted py-4">' +
            'No se encontraron registros válidos para cobrar.' +
            '</td>' +
            '</tr>'
        );

        $('#txtTotalCC').val('$ 0.00');
        $('#btnSeguirCC').prop('disabled', true);
        $('#modalCuentaCorriente').modal('show');
        return;
    }

    seleccionarTodosMovimientosCC(true);

    if (typeof cerrarTecladoDigital === 'function') {
        cerrarTecladoDigital();
    }

    $('#modalCuentaCorriente').modal('show');
}

// ================================================================
// SELECCIÓN Y TOTALIZACIÓN
// ================================================================

function obtenerCheckboxesCuentaCorriente() {
    return $(
        '#tbodyCuentaCorriente input.form-check-input[type="checkbox"]'
    );
}

function seleccionarTodosMovimientosCC(seleccionar) {
    const $checkboxes = obtenerCheckboxesCuentaCorriente();

    $checkboxes.prop('checked', Boolean(seleccionar));

    actualizarEstadoSeleccionCC();
}

function actualizarEstadoSeleccionCC() {
    const $checkboxes = obtenerCheckboxesCuentaCorriente();
    const total = $checkboxes.length;
    const seleccionados = $checkboxes.filter(':checked').length;

    $('#chkSeleccionarTodoCC').prop({
        checked: total > 0 && seleccionados === total,
        indeterminate: seleccionados > 0 && seleccionados < total
    });

    sincronizarAccionesImporteCC();
    calcularTotalCC();
}

function sincronizarAccionesImporteCC() {
    $('#tbodyCuentaCorriente tr').each(function () {
        const $fila = $(this);
        const estaSeleccionada = $fila
            .find('input.form-check-input[type="checkbox"]')
            .is(':checked');

        $fila
            .find('.btn-accion-importe-cc')
            .prop('disabled', !estaSeleccionada)
            .attr('aria-disabled', String(!estaSeleccionada));
    });
}

function calcularTotalCC() {
    let total = 0;

    $('#tbodyCuentaCorriente tr').each(function () {
        const $fila = $(this);
        const $checkbox = $fila.find(
            'input.form-check-input[type="checkbox"]'
        );

        if (!$checkbox.is(':checked')) {
            return;
        }

        const importeActual = obtenerImporteActualCC($fila);

        if (Number.isFinite(importeActual) && importeActual > 0) {
            total += importeActual;
        }
    });

    total = redondearMontoCC(total);

    $('#txtTotalCC').val(`$ ${formatearMontoCC(total)}`);
    $('#btnSeguirCC').prop('disabled', total <= 0);

    return total;
}

// ================================================================
// EDICIÓN DEL IMPORTE A COBRAR
// ================================================================

function obtenerControlesModalImporteCC() {
    // Modal recomendado: _modificarImporteCuentaCorriente.cshtml.
    if ($('#modalModificarImporteCC').length > 0) {
        return {
            modalSelector: '#modalModificarImporteCC',
            inputSelector: '#txtImportePagarCC',
            labelOriginalSelector: '#lblImporteOriginalCC',
            labelSaldoSelector: '#lblSaldoDisponibleCC'
        };
    }

    // Compatibilidad temporal con el modal anterior.
    if ($('#modalDetalleImputaCC').length > 0) {
        return {
            modalSelector: '#modalDetalleImputaCC',
            inputSelector: '#txtImputaCC',
            labelOriginalSelector: '#lblImporteFacturaCC',
            labelSaldoSelector: ''
        };
    }

    return null;
}

function abrirModalImporteCC($fila) {
    if (!$fila || $fila.length === 0) {
        return;
    }

    // Validaciones y carga de datos del modal...

    $filaImporteCCEnEdicion = $fila;

    $('#modalModificarImporteCC')
        .off('shown.bs.modal.tecladoCC')
        .one('shown.bs.modal.tecladoCC', function () {
            const $modal = $(this);

            $modal.css('z-index', '5010');

            $('.modal-backdrop')
                .last()
                .css('z-index', '5009');

            if (typeof activarTecladoParaInput === 'function') {
                activarTecladoParaInput(
                    '#txtImportePagarCC',
                    {
                        anchorSelector: '#teclado-ancla-cc'
                    }
                );
            } else {
                $('#txtImportePagarCC')
                    .trigger('focus')
                    .select();
            }
        })
        .modal('show');
}

function guardarImporteCC() {
    if (
        !$filaImporteCCEnEdicion ||
        $filaImporteCCEnEdicion.length === 0
    ) {
        mostrarMensajeCC(
            'Atención',
            'No se pudo determinar el comprobante que desea modificar.',
            'warn!'
        );
        return;
    }

    const controles = obtenerControlesModalImporteCC();

    if (!controles) {
        mostrarMensajeCC(
            'Error',
            'No se encontró el modal para modificar el importe.',
            'error!'
        );
        return;
    }

    const importeIngresado = normalizarMontoCC(
        $(controles.inputSelector).val()
    );

    const importeMaximo = obtenerImporteBackupCC(
        $filaImporteCCEnEdicion
    );

    if (!Number.isFinite(importeIngresado)) {
        mostrarMensajeCC(
            'Atención',
            'Debe ingresar un importe válido.',
            'warn!'
        );
        return;
    }

    // Para no cobrar un comprobante debe desmarcarse su checkbox.
    if (importeIngresado <= 0) {
        mostrarMensajeCC(
            'Atención',
            'El importe a cobrar debe ser mayor a cero.',
            'warn!'
        );
        return;
    }

    if (!Number.isFinite(importeMaximo) || importeMaximo <= 0) {
        mostrarMensajeCC(
            'Error',
            'El saldo disponible del comprobante es inválido.',
            'error!'
        );
        return;
    }

    if (importeIngresado > importeMaximo + 0.01) {
        mostrarMensajeCC(
            'Atención',
            `El importe a cobrar no puede superar el saldo disponible: $ ${formatearMontoCC(importeMaximo)}.`,
            'warn!'
        );
        return;
    }

    actualizarImporteFilaCC(
        $filaImporteCCEnEdicion,
        importeIngresado
    );

    $(controles.modalSelector).modal('hide');

    calcularTotalCC();
}

function restaurarImporteDesdeBackupCC($fila) {
    if (!$fila || $fila.length === 0) {
        return;
    }

    const $checkbox = $fila.find(
        'input.form-check-input[type="checkbox"]'
    );

    if (!$checkbox.is(':checked')) {
        mostrarMensajeCC(
            'Atención',
            'Debe seleccionar el comprobante antes de restaurar el importe.',
            'warn!'
        );
        return;
    }

    const importeBackup = obtenerImporteBackupCC($fila);

    if (!Number.isFinite(importeBackup) || importeBackup <= 0) {
        mostrarMensajeCC(
            'Error',
            'El saldo original disponible es inválido.',
            'error!'
        );
        return;
    }

    actualizarImporteFilaCC($fila, importeBackup);
    calcularTotalCC();
}

function actualizarImporteFilaCC($fila, importe) {
    const importeNormalizado = redondearMontoCC(importe);

    $fila
        .attr('data-importe', importeNormalizado.toFixed(2))
        .data('importe', importeNormalizado);

    $fila.find('.celda-importe-cc').text(
        `$ ${formatearMontoCC(importeNormalizado)}`
    );
}

function obtenerImporteActualCC($fila) {
    return normalizarMontoCC($fila.attr('data-importe'));
}

function obtenerImporteOriginalCC($fila) {
    return normalizarMontoCC($fila.attr('data-importe-ori'));
}

function obtenerImporteBackupCC($fila) {
    return normalizarMontoCC($fila.attr('data-importe-bak'));
}

// Alias de compatibilidad para referencias anteriores.
function abrirModalModificarImporteCC($fila) {
    abrirModalImporteCC($fila);
}

function guardarImporteModificadoCC() {
    guardarImporteCC();
}

function restaurarImporteOriginalCC($fila) {
    restaurarImporteDesdeBackupCC($fila);
}

function recalcularTotalCuentaCorriente() {
    return calcularTotalCC();
}

// ================================================================
// RESGUARDO DE LA SELECCIÓN E INICIO DEL PAGO
// ================================================================

function iniciarCobranzaCuentaCorriente() { 
    if (cobranzaCCEnCurso) {
        console.warn('⚠️ Ya existe una solicitud de cobranza en curso.');
        return;
    }

    if (typeof iniciarProcesoPago !== 'function') {
        mostrarMensajeCC(
            'Error',
            'El módulo de pago no está disponible. Recargue la página e intente nuevamente.',
            'error!'
        );
        return;
    }

    if (
        typeof resguardarCuentaCorrienteSeleccionadaUrl === 'undefined' ||
        !resguardarCuentaCorrienteSeleccionadaUrl
    ) {
        mostrarMensajeCC(
            'Error',
            'No está configurada la URL para resguardar la selección de Cuenta Corriente.',
            'error!'
        );
        return;
    }

    const $checkboxesSeleccionados = $(
        '#tbodyCuentaCorriente input.form-check-input[type="checkbox"]:checked'
    );

    if ($checkboxesSeleccionados.length === 0) {
        mostrarMensajeCC(
            'Atención',
            'Debe seleccionar al menos un comprobante para cobrar.',
            'warn!'
        );
        return;
    }

    const registrosSeleccionados = [];
    const errores = [];

    $checkboxesSeleccionados.each(function (index) {
        const $checkbox = $(this);
        const $fila = $checkbox.closest('tr');

        const importeActual = obtenerImporteActualCC($fila);
        const importeOriginal = obtenerImporteOriginalCC($fila);
        const importeMaximo = obtenerImporteBackupCC($fila);

        const registro = {
            cta_id: obtenerDataCC($checkbox, 'cta-id'),
            dia_movi: obtenerDataCC($checkbox, 'dia-movi'),
            tco_id: obtenerDataCC($checkbox, 'tco-id'),
            cm_compte: obtenerDataCC($checkbox, 'cm-compte'),
            cm_compte_cuota: convertirAEnteroCC(
                obtenerDataCC($checkbox, 'cm-compte-cuota')
            ),
            cv_fecha_vto: normalizarFechaParaServidorCC(
                obtenerDataCC($checkbox, 'cv-fecha-vto')
            ),

            // Valor final elegido por el usuario.
            cv_importe: importeActual,

            // Importe original histórico. No se modifica.
            cv_importe_ori: importeOriginal,

            cv_concepto: obtenerDataCC($checkbox, 'cv-concepto'),
            ve_id: obtenerDataCC($checkbox, 've-id'),
            ccb_id: obtenerDataCC($checkbox, 'ccb-id'),
            ctacte: obtenerDataCC($checkbox, 'ctacte'),
            carga: obtenerDataCC($checkbox, 'carga'),
            carga_obligatoria: obtenerDataCC(
                $checkbox,
                'carga-obligatoria'
            )
        };

        const identificador = `${registro.tco_id || 'N/A'} ${registro.cm_compte || 'N/A'}`;
        const camposFaltantes = [];

        if (!registro.cta_id) camposFaltantes.push('cta_id');
        if (!registro.tco_id) camposFaltantes.push('tco_id');
        if (!registro.cm_compte) camposFaltantes.push('cm_compte');
        if (!registro.ctacte) camposFaltantes.push('ctacte');

        if (camposFaltantes.length > 0) {
            errores.push(
                `Registro ${index + 1} (${identificador}): faltan ${camposFaltantes.join(', ')}.`
            );
            return;
        }

        if (
            !Number.isInteger(registro.cm_compte_cuota) ||
            registro.cm_compte_cuota < 0
        ) {
            errores.push(
                `Registro ${index + 1} (${identificador}): la cuota es inválida.`
            );
            return;
        }

        if (!registro.cv_fecha_vto) {
            errores.push(
                `Registro ${index + 1} (${identificador}): la fecha de vencimiento es inválida.`
            );
            return;
        }

        if (!Number.isFinite(importeOriginal) || importeOriginal <= 0) {
            errores.push(
                `Registro ${index + 1} (${identificador}): el importe original es inválido.`
            );
            return;
        }

        if (!Number.isFinite(importeMaximo) || importeMaximo <= 0) {
            errores.push(
                `Registro ${index + 1} (${identificador}): el saldo disponible es inválido.`
            );
            return;
        }

        if (!Number.isFinite(importeActual) || importeActual <= 0) {
            errores.push(
                `Registro ${index + 1} (${identificador}): el importe a cobrar debe ser mayor a cero.`
            );
            return;
        }

        if (importeActual > importeMaximo + 0.01) {
            errores.push(
                `Registro ${index + 1} (${identificador}): el importe a cobrar no puede superar $ ${formatearMontoCC(importeMaximo)}.`
            );
            return;
        }

        if (importeOriginal + 0.01 < importeMaximo) {
            errores.push(
                `Registro ${index + 1} (${identificador}): el importe original es inconsistente con el saldo disponible.`
            );
            return;
        }

        registrosSeleccionados.push(registro);
    });

    if (errores.length > 0) {
        mostrarMensajeCC(
            'Datos incompletos',
            `No se puede iniciar la cobranza.<br><br>${errores.map(escaparHtmlCC).join('<br>')}`,
            'error!'
        );
        return;
    }

    if (registrosSeleccionados.length === 0) {
        mostrarMensajeCC(
            'Atención',
            'No se pudo construir ningún comprobante válido para la cobranza.',
            'error!'
        );
        return;
    }

    const totalPagar = redondearMontoCC(
        registrosSeleccionados.reduce(function (acumulado, registro) {
            return acumulado + registro.cv_importe;
        }, 0)
    );

    if (totalPagar <= 0) {
        mostrarMensajeCC(
            'Error',
            'El total a cobrar debe ser mayor a cero.',
            'error!'
        );
        return;
    }

    cobranzaCCEnCurso = true;
    $('#btnSeguirCC').prop('disabled', true);

    mostrarLoaderCC('Preparando cobranza de Cuenta Corriente...');

    $.ajax({
        url: resguardarCuentaCorrienteSeleccionadaUrl,
        type: 'POST',
        contentType: 'application/json',
        dataType: 'json',
        data: JSON.stringify({ Registros: registrosSeleccionados }),

        success: function (response) {
            ocultarLoaderCC();

            if (!response || !response.ok) {
                cobranzaCCEnCurso = false;
                calcularTotalCC();

                mostrarMensajeCC(
                    'Error',
                    response?.mensaje ||
                    'No se pudieron resguardar los comprobantes seleccionados.',
                    'error!'
                );
                return;
            }

            // pagoFactura.js utiliza esta variable para armar Cancelar.
            window._cuentaCorrienteDelClienteSeleccionadaParaElCobro =
                registrosSeleccionados;

            // Evita que un flujo previo de cobranza diferida se use por error.
            window._facturasSeleccionadasParaCobro = null;

            const iniciarPago = function () {
                try {
                    const iniciado = iniciarProcesoPago({
                        totalPagar: totalPagar,
                        co_tipo: 'CC',
                        puntoVenta: 'GECO PD',
                        tituloModal: 'Cobranza Cuenta Corriente',
                        contextoOperacion: 'COBRANZA',
                        fuenteCliente: 'CUENTA_CORRIENTE'
                    });

                    if (iniciado === false) {
                        calcularTotalCC();
                    } else {
                        // El pago central todavía usa el texto fijo
                        // "COBRANZA DIFERIDA" para cualquier COBRANZA.
                        // Se corrige visualmente para Cuenta Corriente.
                        $('#headerTituloPago').html(
                            "<i class='bx bx-receipt'></i> COBRANZA CUENTA CORRIENTE"
                        );
                    }
                } catch (error) {
                    console.error(
                        '❌ Error al iniciar el proceso de pago de Cuenta Corriente.',
                        error
                    );

                    calcularTotalCC();

                    mostrarMensajeCC(
                        'Error',
                        'No se pudo iniciar el proceso de pago.',
                        'error!'
                    );
                } finally {
                    cobranzaCCEnCurso = false;
                }
            };

            const $modalCuentaCorriente = $('#modalCuentaCorriente');

            if ($modalCuentaCorriente.hasClass('show')) {
                $modalCuentaCorriente
                    .off('hidden.bs.modal.iniciarCobranzaCC')
                    .one('hidden.bs.modal.iniciarCobranzaCC', iniciarPago)
                    .modal('hide');
            } else {
                iniciarPago();
            }
        },

        error: function (xhr, status, error) {
            ocultarLoaderCC();

            cobranzaCCEnCurso = false;
            calcularTotalCC();

            console.error(
                '❌ Error AJAX al resguardar Cuenta Corriente:',
                {
                    status: xhr.status,
                    statusText: xhr.statusText,
                    responseText: xhr.responseText,
                    error: error
                }
            );

            let mensaje =
                'No se pudieron guardar los comprobantes seleccionados en el servidor.';

            if (xhr.responseJSON?.mensaje) {
                mensaje = xhr.responseJSON.mensaje;
            } else if (xhr.status === 400) {
                mensaje = 'Los datos enviados son inválidos.';
            } else if (xhr.status === 0) {
                mensaje = 'No se pudo establecer conexión con el servidor.';
            }

            mostrarMensajeCC('Error de Comunicación', mensaje, 'error!');
        }
    });
}

// ================================================================
// HELPERS
// ================================================================

function obtenerPrimerValorCC() {
    const valores = Array.from(arguments);

    for (let i = 0; i < valores.length; i++) {
        const valor = valores[i];

        if (
            valor !== null &&
            valor !== undefined &&
            String(valor).trim() !== '' &&
            String(valor).trim().toLowerCase() !== 'null' &&
            String(valor).trim().toLowerCase() !== 'undefined'
        ) {
            return String(valor).trim();
        }
    }

    return '';
}

function obtenerTipoNumeroClienteCC(cliente) {
    if (!cliente) {
        return '';
    }

    const tipoNumero = obtenerPrimerValorCC(cliente.tipoNumero);

    if (tipoNumero) {
        return tipoNumero;
    }

    const tipoDocumento = obtenerPrimerValorCC(
        cliente.tdoc_desc,
        cliente.tipoDocumento
    );

    const documento = obtenerPrimerValorCC(
        cliente.documento,
        cliente.nro_documento
    );

    return [tipoDocumento, documento]
        .filter(Boolean)
        .join(' ');
}

function poblarDatosClienteCC(cliente) {
    $('#txtNombreCC').val(
        obtenerPrimerValorCC(
            cliente?.denominacion,
            cliente?.cta_denominacion,
            cliente?.nombre
        )
    );

    $('#txtClienteIdCC').val(
        obtenerPrimerValorCC(cliente?.cta_id, cliente?.id)
    );

    $('#txtDomicilioCC').val(
        obtenerPrimerValorCC(cliente?.domicilio, cliente?.cta_domicilio)
    );

    $('#txtCondicionAfipCC').val(
        obtenerPrimerValorCC(
            cliente?.condicionAfip,
            cliente?.afip_desc,
            cliente?.afip_id
        )
    );

    $('#txtTipoNumeroCC').val(obtenerTipoNumeroClienteCC(cliente));

    $('#txtEmailCC').val(
        obtenerPrimerValorCC(cliente?.email, cliente?.cta_email)
    );

    $('#txtMovilCC').val(
        obtenerPrimerValorCC(cliente?.movil, cliente?.cta_celu)
    );
}

function sincronizarDatosClienteParaPagoCC() {
    const mapeo = [
        ['#txtNombreCC', 'txtNombrePendiente'],
        ['#txtClienteIdCC', 'txtClienteIdPendiente'],
        ['#txtDomicilioCC', 'txtDomicilioPendiente'],
        ['#txtCondicionAfipCC', 'txtCondicionAfipPendiente'],
        ['#txtTipoNumeroCC', 'txtTipoNumeroPendiente'],
        ['#txtEmailCC', 'txtEmailPendiente'],
        ['#txtMovilCC', 'txtMovilPendiente']
    ];

    mapeo.forEach(function (item) {
        const selectorOrigen = item[0];
        const idDestino = item[1];
        let $destino = $(`#${idDestino}`);

        // Solo se crean campos puente cuando la vista de cobranza diferida
        // no está presente en esta pantalla.
        if ($destino.length === 0) {
            $destino = $(
                `<input type="hidden" id="${idDestino}" aria-hidden="true">`
            );

            $('body').append($destino);
        }

        $destino.val($(selectorOrigen).val() || '');
    });
}

function obtenerDataCC($elemento, nombre) {
    const valor = $elemento.attr(`data-${nombre}`);

    return valor === undefined || valor === null
        ? ''
        : String(valor).trim();
}

function convertirAEnteroCC(valor) {
    const texto = String(valor ?? '').trim();

    if (!/^\d+$/.test(texto)) {
        return NaN;
    }

    const numero = Number.parseInt(texto, 10);

    return Number.isInteger(numero) ? numero : NaN;
}

function normalizarFechaParaServidorCC(valor) {
    const texto = String(valor ?? '').trim();

    if (!texto) {
        return null;
    }

    // ISO enviado normalmente por ASP.NET Core.
    if (/^\d{4}-\d{2}-\d{2}(T.*)?$/.test(texto)) {
        return texto.includes('T')
            ? texto.replace(' ', 'T')
            : `${texto}T00:00:00`;
    }

    // dd/MM/yyyy o dd-MM-yyyy.
    const coincidenciaFechaLatina = texto.match(
        /^(\d{1,2})[/-](\d{1,2})[/-](\d{4})$/
    );

    if (coincidenciaFechaLatina) {
        const dia = coincidenciaFechaLatina[1].padStart(2, '0');
        const mes = coincidenciaFechaLatina[2].padStart(2, '0');
        const anio = coincidenciaFechaLatina[3];

        return `${anio}-${mes}-${dia}T00:00:00`;
    }

    const fecha = new Date(texto);

    return Number.isNaN(fecha.getTime())
        ? null
        : fecha.toISOString();
}

function formatearFechaCC(valor) {
    const fechaNormalizada = normalizarFechaParaServidorCC(valor);

    if (!fechaNormalizada) {
        return 'N/A';
    }

    const coincidencia = fechaNormalizada.match(/^(\d{4})-(\d{2})-(\d{2})/);

    if (!coincidencia) {
        return 'N/A';
    }

    return `${coincidencia[3]}/${coincidencia[2]}/${coincidencia[1]}`;
}

function escaparHtmlCC(valor) {
    return String(valor ?? '')
        .replace(/&/g, '&amp;')
        .replace(/</g, '&lt;')
        .replace(/>/g, '&gt;')
        .replace(/"/g, '&quot;')
        .replace(/'/g, '&#039;');
}

function redondearMontoCC(valor) {
    const numero = Number(valor);

    return Number.isFinite(numero)
        ? Math.round((numero + Number.EPSILON) * 100) / 100
        : NaN;
}

function normalizarMontoCC(valor) {
    if (valor === null || valor === undefined || valor === '') {
        return NaN;
    }

    if (typeof valor === 'number') {
        return Number.isFinite(valor)
            ? redondearMontoCC(valor)
            : NaN;
    }

    let texto = String(valor)
        .trim()
        .replace(/\$/g, '')
        .replace(/\s/g, '');

    if (!texto) {
        return NaN;
    }

    // Conserva únicamente números, separadores decimales/miles y signo.
    texto = texto.replace(/[^\d,.\-]/g, '');

    if (!/^-?[\d,.]+$/.test(texto)) {
        return NaN;
    }

    const tieneComa = texto.includes(',');
    const tienePunto = texto.includes('.');

    if (tieneComa && tienePunto) {
        // El último separador se interpreta como decimal:
        // 1.234,56 -> 1234.56
        // 1,234.56 -> 1234.56
        if (texto.lastIndexOf(',') > texto.lastIndexOf('.')) {
            texto = texto.replace(/\./g, '').replace(',', '.');
        } else {
            texto = texto.replace(/,/g, '');
        }
    } else if (tieneComa) {
        const partes = texto.split(',');

        // 1,50 = decimal. 1,000 = separador de miles.
        if (partes.length === 2 && partes[1].length <= 2) {
            texto = partes[0].replace(/\./g, '') + '.' + partes[1];
        } else {
            texto = texto.replace(/,/g, '');
        }
    } else if (tienePunto) {
        const partes = texto.split('.');

        // 1.50 = decimal. 1.000 = separador de miles.
        if (partes.length === 2 && partes[1].length <= 2) {
            texto = partes[0].replace(/,/g, '') + '.' + partes[1];
        } else if (
            partes.length > 2 &&
            partes[partes.length - 1].length <= 2
        ) {
            const decimal = partes.pop();
            texto = partes.join('') + '.' + decimal;
        } else {
            texto = texto.replace(/\./g, '');
        }
    }

    const monto = Number(texto);

    return Number.isFinite(monto)
        ? redondearMontoCC(monto)
        : NaN;
}

function formatearMontoCC(monto) {
    const valor = Number.isFinite(monto) ? monto : 0;

    if (typeof formatearNumero === 'function') {
        return formatearNumero(valor, 2);
    }

    return new Intl.NumberFormat('en-US', {
        minimumFractionDigits: 2,
        maximumFractionDigits: 2
    }).format(valor);
}

function mostrarLoaderCC(mensaje) {
    if (typeof mostrarLoader === 'function') {
        mostrarLoader(mensaje);
        return;
    }

    if (typeof AbrirWaiting === 'function') {
        AbrirWaiting(mensaje);
    }
}

function ocultarLoaderCC() {
    if (typeof ocultarLoader === 'function') {
        ocultarLoader();
        return;
    }

    if (typeof CerrarWaiting === 'function') {
        CerrarWaiting(false);
    }
}

function mostrarMensajeCC(titulo, mensaje, tipo) {
    const tipoMensaje = tipo || 'warn!';

    if (typeof AbrirMensaje === 'function') {
        AbrirMensaje(
            titulo,
            mensaje,
            function () {
                $('#msjModal').modal('hide');
            },
            false,
            ['Aceptar'],
            tipoMensaje
        );
        return;
    }

    window.alert(`${titulo}: ${String(mensaje).replace(/<br>/g, '\n')}`);
}
