(function () {
    'use strict';

    const LOG_PREFIX = '[CierreCaja]';
    const SELECTORES = {
        modal: '#modalCierreCaja',
        modalCantidad: '#modalCierreCantidad',
        panelEstado: '#panelCierreEstado',
        panelPendientes: '#panelCierrePendientes',
        panelRendicion: '#panelCierreRendicion',
        tbodyPendientes: '#tbodyCierrePendientes',
        pendientesCount: '#lblCierrePendientesCount',
        tbodyInstrumentos: '#tbodyCierreInstrumentos',
        tbodyNominaciones: '#tbodyCierreNominaciones',
        instrumentosCount: '#lblCierreInstrumentosCount',
        instrumentoSeleccionado: '#lblCierreInstrumentoSeleccionado',
        total: '#lblCierreTotal',
        btnCancelar: '#btnCierreCancelar',
        btnConfirmar: '#btnCierreConfirmar',
        nomIndex: '#txtCierreNomIndex',
        nomValor: '#txtCierreNomValor',
        cantidad: '#txtCierreCantidad',
        cantidadMensaje: '#cierreCantidadMensaje',
        cantidadTecladoAncla: '#cierreCantidadTecladoAncla',
        btnCantidadCancelar: '#btnCierreCantidadCancelar',
        btnCantidadBorrar: '#btnCierreCantidadBorrar',
        btnCantidadAceptar: '#btnCierreCantidadAceptar'
    };

    let instrumentos = [];
    let nominacionesPorInstrumento = {};
    let instrumentoActivo = null;
    let confirmando = false;
    let pendienteBloqueante = false;
    let cierreFinalizado = false;

    $(function () {
        logPaso('Inicializando modulo', {
            verificarPendientesUrl: window.cierreVerificarPendientesUrl,
            cargarInstrumentosUrl: window.cierreCargarInstrumentosUrl,
            cargarNominacionesUrl: window.cierreCargarNominacionesUrl,
            confirmarUrl: window.cierreConfirmarUrl
        });

        registrarEventos();
        abrirModulo();
    });

    function registrarEventos() {
        $(document).on('click', '.cierre-instrumento-row', function () {
            if (confirmando) {
                return;
            }

            seleccionarInstrumento($(this).data('ins-id'));
        });

        $(document).on('click', '.btn-cierre-editar-cantidad', function (event) {
            event.preventDefault();
            event.stopPropagation();

            if (confirmando) {
                return;
            }

            abrirEditorCantidad($(this).data('index'));
        });

        $(document).on('click', SELECTORES.btnCantidadCancelar, function () {
            ocultarTecladoCierre();
            $(SELECTORES.modalCantidad).modal('hide');
        });

        $(document).on('click', SELECTORES.btnCantidadBorrar, function () {
            ocultarTecladoCierre();
            guardarCantidadNominal(0);
        });

        $(document).on('click', SELECTORES.btnCantidadAceptar, function () {
            const cantidad = parsearEntero($(SELECTORES.cantidad).val(), -1);
            if (cantidad < 0) {
                mostrarMensajeCantidad('La cantidad debe ser cero o mayor.');
                return;
            }

            ocultarTecladoCierre();
            guardarCantidadNominal(cantidad);
        });

        $(document).on('keydown', SELECTORES.cantidad, function (event) {
            if (event.key === 'Enter') {
                event.preventDefault();
                $(SELECTORES.btnCantidadAceptar).trigger('click');
                return;
            }

            permitirSoloEntero(event);
        });

        $(document).on('input', SELECTORES.cantidad, function () {
            limitarSoloDigitos($(this));
        });

        $(document).on('focus', SELECTORES.cantidad, function () {
            this.select();
            posicionarTecladoCantidad();
        });

        $(SELECTORES.modalCantidad)
            .on('shown.bs.modal', function () {
                activarTecladoCantidad();
            })
            .on('hide.bs.modal hidden.bs.modal', function () {
                ocultarTecladoCierre();
            });

        $(document).on('click', SELECTORES.btnCancelar, function () {
            $(SELECTORES.modal).modal('hide');
            setTimeout(function () {
                window.location.href = window.cierreMenuCajaUrl || '/';
            }, 150);
        });
        $(document).on('click', SELECTORES.btnConfirmar, confirmarCierre);
    }

    function abrirModulo() {
        $(SELECTORES.modal).modal('show');
        verificarPendientesIniciales();
    }

    function verificarPendientesIniciales() {
        const url = String(window.cierreVerificarPendientesUrl || '').trim();
        if (!url) {
            mostrarMensaje('Error', 'No se encontro la URL para verificar pendientes.', 'error!');
            bloquearPorError('No se encontro la URL para verificar pendientes.');
            return;
        }

        pendienteBloqueante = false;
        instrumentos = [];
        nominacionesPorInstrumento = {};
        instrumentoActivo = null;
        renderEstado('Verificando cobranzas diferidas pendientes...', 'info', true);
        $(SELECTORES.panelPendientes).addClass('d-none');
        $(SELECTORES.panelRendicion).addClass('d-none');
        $(SELECTORES.btnConfirmar).prop('disabled', true);

        logPaso('Verificando pendientes');
        mostrarLoaderCierre('Verificando cobranzas diferidas pendientes...');

        $.ajax({
            url: url,
            type: 'POST',
            dataType: 'json',
            timeout: 30000
        })
            .done(function (response) {
                logPaso('Response verificar pendientes', response);
                if (!response || response.ok !== true) {
                    bloquearPorError(response?.mensaje || 'No se pudieron verificar las cobranzas diferidas pendientes.');
                    return;
                }

                if (response.hayPendientes === true) {
                    pendienteBloqueante = true;
                    renderPendientes(response.lista || []);
                    renderEstado(response.mensaje || 'Existen cobranzas diferidas pendientes para el cierre actual.', 'warning', false);
                    $(SELECTORES.panelPendientes).removeClass('d-none');
                    $(SELECTORES.panelRendicion).addClass('d-none');
                    $(SELECTORES.btnConfirmar).prop('disabled', true);
                    return;
                }

                renderEstado(response.mensaje || 'No se encontraron facturas pendientes de clientes. Puede continuar con la rendicion final de caja.', 'success', false);
                $(SELECTORES.panelPendientes).addClass('d-none');
                $(SELECTORES.panelRendicion).removeClass('d-none');
                cargarInstrumentos();
            })
            .fail(function (xhr) {
                logError('Error AJAX verificar pendientes', xhr?.responseJSON || xhr?.responseText || xhr?.status);
                bloquearPorError(xhr?.responseJSON?.mensaje || 'Ocurrio un error al verificar las cobranzas diferidas pendientes.');
            })
            .always(function () {
                ocultarLoaderCierre();
            });
    }

    function bloquearPorError(mensaje) {
        renderEstado(mensaje, 'danger', false);
        $(SELECTORES.panelPendientes).addClass('d-none');
        $(SELECTORES.panelRendicion).addClass('d-none');
        $(SELECTORES.btnConfirmar).prop('disabled', true);
        mostrarMensaje('Error', mensaje, 'error!');
    }

    function cargarInstrumentos() {
        const url = String(window.cierreCargarInstrumentosUrl || '').trim();
        if (!url) {
            mostrarMensaje('Error', 'No se encontro la URL para cargar instrumentos.', 'error!');
            return;
        }

        logPaso('Cargando instrumentos finales');
        mostrarLoaderCierre('Cargando instrumentos de cierre...');

        $.ajax({
            url: url,
            type: 'POST',
            dataType: 'json',
            timeout: 30000
        })
            .done(function (response) {
                logPaso('Response cargar instrumentos', response);
                if (!response || response.ok !== true) {
                    mostrarMensaje('Atencion', response?.mensaje || 'No se pudieron cargar los instrumentos.', 'warn!');
                    renderInstrumentos([]);
                    return;
                }

                instrumentos = normalizarInstrumentos(response.lista || []);
                renderInstrumentos(instrumentos);

                if (instrumentos.length > 0) {
                    seleccionarInstrumento(instrumentos[0].ins_id);
                }
            })
            .fail(function (xhr) {
                logError('Error AJAX cargar instrumentos', xhr?.responseJSON || xhr?.responseText || xhr?.status);
                mostrarMensaje('Error', xhr?.responseJSON?.mensaje || 'Ocurrio un error al cargar los instrumentos.', 'error!');
                renderInstrumentos([]);
            })
            .always(function () {
                ocultarLoaderCierre();
            });
    }

    function seleccionarInstrumento(insId) {
        const instrumento = instrumentos.find(x => x.ins_id === String(insId || '').trim());
        if (!instrumento) {
            return;
        }

        instrumentoActivo = instrumento;
        $('.cierre-instrumento-row').removeClass('table-success');
        $(`.cierre-instrumento-row[data-ins-id="${escaparSelector(instrumento.ins_id)}"]`).addClass('table-success');
        $(SELECTORES.instrumentoSeleccionado).text(`${instrumento.ins_id} - ${instrumento.ins_desc}`);

        if (nominacionesPorInstrumento[instrumento.ins_id]) {
            renderNominaciones(nominacionesPorInstrumento[instrumento.ins_id]);
            return;
        }

        cargarNominaciones(instrumento.ins_id);
    }

    function cargarNominaciones(insId) {
        const url = String(window.cierreCargarNominacionesUrl || '').trim();
        if (!url) {
            mostrarMensaje('Error', 'No se encontro la URL para cargar nominaciones.', 'error!');
            return;
        }

        logPaso('Cargando nominaciones', { ins_id: insId });
        mostrarLoaderCierre('Cargando nominaciones...');
        renderNominacionesCargando();

        $.ajax({
            url: url,
            type: 'POST',
            dataType: 'json',
            contentType: 'application/json; charset=utf-8',
            data: JSON.stringify({ ins_id: insId }),
            timeout: 30000
        })
            .done(function (response) {
                logPaso('Response cargar nominaciones', response);
                if (!response || response.ok !== true) {
                    mostrarMensaje('Atencion', response?.mensaje || 'No se pudieron cargar las nominaciones.', 'warn!');
                    nominacionesPorInstrumento[insId] = [];
                    renderNominaciones([]);
                    return;
                }

                const nominaciones = normalizarNominaciones(response.lista || []);
                nominacionesPorInstrumento[insId] = nominaciones;
                renderNominaciones(nominaciones);
            })
            .fail(function (xhr) {
                logError('Error AJAX cargar nominaciones', xhr?.responseJSON || xhr?.responseText || xhr?.status);
                mostrarMensaje('Error', xhr?.responseJSON?.mensaje || 'Ocurrio un error al cargar las nominaciones.', 'error!');
                nominacionesPorInstrumento[insId] = [];
                renderNominaciones([]);
            })
            .always(function () {
                ocultarLoaderCierre();
            });
    }

    function abrirEditorCantidad(index) {
        if (!instrumentoActivo) {
            return;
        }

        const nominaciones = nominacionesPorInstrumento[instrumentoActivo.ins_id] || [];
        const nominacion = nominaciones[index];
        if (!nominacion) {
            mostrarMensaje('Atencion', 'No se pudo identificar la nominacion seleccionada.', 'warn!');
            return;
        }

        $(SELECTORES.nomIndex).val(index);
        $(SELECTORES.nomValor).val(formatearNumero(nominacion.nominacion, 2));
        $(SELECTORES.cantidad).val(String(nominacion.cantidad || 0));
        $(SELECTORES.cantidadMensaje).addClass('d-none').empty();
        $(SELECTORES.modalCantidad).modal('show');

        setTimeout(function () {
            activarTecladoCantidad();
        }, 300);
    }

    function guardarCantidadNominal(cantidad) {
        if (!instrumentoActivo) {
            return;
        }

        const index = parsearEntero($(SELECTORES.nomIndex).val(), -1);
        const nominaciones = nominacionesPorInstrumento[instrumentoActivo.ins_id] || [];
        const nominacion = nominaciones[index];
        if (!nominacion) {
            mostrarMensajeCantidad('No se pudo identificar la nominacion seleccionada.');
            return;
        }

        nominacion.cantidad = cantidad;
        nominacion.importe = redondear(nominacion.nominacion * cantidad);
        instrumentoActivo.ins_importe = redondear(nominaciones.reduce(function (total, item) {
            return total + Number(item.importe || 0);
        }, 0));

        logPaso('Cantidad nominal actualizada', {
            ins_id: instrumentoActivo.ins_id,
            nominacion: nominacion.nominacion,
            cantidad: cantidad,
            importe: nominacion.importe,
            totalInstrumento: instrumentoActivo.ins_importe
        });

        $(SELECTORES.modalCantidad).modal('hide');
        renderNominaciones(nominaciones);
        renderInstrumentos(instrumentos);
        seleccionarInstrumento(instrumentoActivo.ins_id);
        actualizarTotalGeneral();
    }

    function confirmarCierre() {
        if (confirmando || pendienteBloqueante || instrumentos.length === 0) {
            return;
        }

        const total = calcularTotalGeneral();
        AbrirMensaje(
            'Confirmar Cierre de Caja',
            `Esta por cerrar el PV con una rendicion final de <strong>${formatearMoneda(total)}</strong>.<br><br>El sistema volvera a verificar pendientes antes de cerrar. ¿Desea continuar?`,
            function (respuesta) {
                $('#msjModal').modal('hide');
                if (respuesta === 'SI') {
                    bloquearPantallaCierre('Confirmando cierre de caja. Aguarde, no toque nada hasta que el proceso termine...');
                    setTimeout(ejecutarConfirmacion, 150);
                }
            },
            true,
            ['Cerrar PV', 'Cancelar'],
            'quest!',
            null
        );
    }

    function ejecutarConfirmacion() {
        const url = String(window.cierreConfirmarUrl || '').trim();
        if (!url) {
            desbloquearPantallaCierre();
            mostrarMensaje('Error', 'No se encontro la URL de confirmacion.', 'error!');
            return;
        }

        const payload = {
            rendiciones: instrumentos.map(function (item) {
                return {
                    ins_id: item.ins_id,
                    ins_desc: item.ins_desc,
                    ins_importe: redondear(item.ins_importe || 0)
                };
            })
        };

        logPaso('Confirmando cierre', payload);
        confirmando = true;
        bloquearPantallaCierre('Confirmando cierre de caja. Aguarde, no toque nada hasta que el proceso termine...');
        $(SELECTORES.btnConfirmar).prop('disabled', true).html('<i class="bx bx-loader-alt bx-spin"></i> CERRANDO');

        $.ajax({
            url: url,
            type: 'POST',
            dataType: 'json',
            contentType: 'application/json; charset=utf-8',
            data: JSON.stringify(payload),
            timeout: 60000
        })
            .done(function (response) {
                logPaso('Response confirmar cierre', response);
                if (response && response.bloqueado === true) {
                    pendienteBloqueante = true;
                    renderPendientes(response.pendientes || []);
                    renderEstado(response.mensaje || 'No se puede cerrar la caja porque existen pendientes.', 'warning', false);
                    $(SELECTORES.panelPendientes).removeClass('d-none');
                    $(SELECTORES.panelRendicion).addClass('d-none');
                    mostrarMensaje('Cierre bloqueado', response.mensaje || 'Existen pendientes que impiden el cierre.', 'warn!');
                    return;
                }

                if (!response || response.ok !== true) {
                    mostrarMensaje('Atencion', response?.mensaje || 'No se pudo confirmar el cierre de caja.', 'warn!');
                    return;
                }

                cierreFinalizado = true;
                ocultarBloqueoPantallaCierre();
                $(SELECTORES.btnCancelar).prop('disabled', true);
                $(SELECTORES.btnConfirmar).prop('disabled', true).html('<i class="bx bx-check-circle"></i> CERRADO');

                AbrirMensaje(
                    'Cierre de Caja Confirmado',
                    construirMensajeExito(response),
                    function () {
                        $('#msjModal').modal('hide');
                        window.location.href = window.cierreLogoutUrl || window.logout || window.cierreMenuCajaUrl || '/';
                    },
                    false,
                    ['Aceptar'],
                    'success!',
                    null
                );
            })
            .fail(function (xhr) {
                logError('Error AJAX confirmar cierre', xhr?.responseJSON || xhr?.responseText || xhr?.status);
                mostrarMensaje('Error', xhr?.responseJSON?.mensaje || 'Ocurrio un error al confirmar el cierre de caja.', 'error!');
            })
            .always(function () {
                ocultarLoaderCierre();
                if (cierreFinalizado) {
                    return;
                }

                desbloquearPantallaCierre();
                confirmando = false;
                $(SELECTORES.btnConfirmar).html('<i class="bx bx-check-circle"></i> CERRAR PV');
                actualizarTotalGeneral();
            });
    }

    function renderPendientes(lista) {
        const pendientes = Array.isArray(lista) ? lista : [];
        $(SELECTORES.pendientesCount).text(pendientes.length);

        if (!pendientes.length) {
            $(SELECTORES.tbodyPendientes).html(`
                <tr>
                    <td colspan="6" class="text-center text-muted py-4">
                        <i class="bx bx-info-circle"></i>
                        No se recibieron detalles de pendientes.
                    </td>
                </tr>
            `);
            return;
        }

        const html = pendientes.map(function (item) {
            return `
                <tr>
                    <td class="fw-bold">${escaparHtml(item.tco_id || item.tipo || '')}</td>
                    <td>${escaparHtml(item.cm_compte || item.comprobante || '')}</td>
                    <td>${escaparHtml(item.co_pd_nombre || item.cliente || item.cta_id || '')}</td>
                    <td>${escaparHtml(item.co_pd_doc || item.documento || '')}</td>
                    <td>${escaparHtml(formatearFecha(item.dia_movi || item.cv_fecha_vto || item.fecha || ''))}</td>
                    <td class="text-end fw-bold text-danger">${formatearMoneda(parsearNumero(item.cv_importe, 0))}</td>
                </tr>
            `;
        }).join('');

        $(SELECTORES.tbodyPendientes).html(html);
    }

    function renderInstrumentos(lista) {
        $(SELECTORES.instrumentosCount).text(lista.length);

        if (!lista.length) {
            $(SELECTORES.tbodyInstrumentos).html(`
                <tr>
                    <td colspan="3" class="text-center text-muted py-4">
                        <i class="bx bx-info-circle"></i>
                        Sin instrumentos disponibles
                    </td>
                </tr>
            `);
            actualizarTotalGeneral();
            return;
        }

        const html = lista.map(function (item) {
            return `
                <tr class="cierre-instrumento-row" data-ins-id="${escaparHtml(item.ins_id)}">
                    <td class="fw-bold">${escaparHtml(item.ins_id)}</td>
                    <td>${escaparHtml(item.ins_desc)}</td>
                    <td class="text-end fw-bold text-success">${formatearMoneda(item.ins_importe)}</td>
                </tr>
            `;
        }).join('');

        $(SELECTORES.tbodyInstrumentos).html(html);
        actualizarTotalGeneral();
    }

    function renderNominacionesCargando() {
        $(SELECTORES.tbodyNominaciones).html(`
            <tr>
                <td colspan="4" class="text-center text-muted py-4">
                    <i class="bx bx-loader-alt bx-spin"></i>
                    Cargando nominaciones...
                </td>
            </tr>
        `);
    }

    function renderNominaciones(lista) {
        if (!lista.length) {
            $(SELECTORES.tbodyNominaciones).html(`
                <tr>
                    <td colspan="4" class="text-center text-muted py-4">
                        <i class="bx bx-info-circle"></i>
                        El instrumento no tiene nominaciones configuradas
                    </td>
                </tr>
            `);
            return;
        }

        const html = lista.map(function (item, index) {
            return `
                <tr>
                    <td class="text-end fw-bold">${formatearNumero(item.nominacion, 2)}</td>
                    <td class="text-end">${formatearNumero(item.cantidad, 0)}</td>
                    <td class="text-end fw-bold text-success">${formatearMoneda(item.importe)}</td>
                    <td class="text-center">
                        <button type="button" class="btn btn-sm btn-golden btn-cierre-editar-cantidad" data-index="${index}" title="Editar cantidad">
                            <i class="bx bx-edit-alt"></i>
                        </button>
                    </td>
                </tr>
            `;
        }).join('');

        $(SELECTORES.tbodyNominaciones).html(html);
    }

    function renderEstado(mensaje, tipo, cargando) {
        const clases = 'alert-info alert-success alert-warning alert-danger';
        const clase = tipo === 'success' ? 'alert-success' : tipo === 'warning' ? 'alert-warning' : tipo === 'danger' ? 'alert-danger' : 'alert-info';
        const icono = cargando ? 'bx-loader-alt bx-spin' : tipo === 'success' ? 'bx-check-circle' : tipo === 'warning' ? 'bx-error-circle' : tipo === 'danger' ? 'bx-x-circle' : 'bx-info-circle';
        $(SELECTORES.panelEstado)
            .removeClass(clases)
            .addClass(clase)
            .html(`<i class="bx ${icono} me-1"></i>${escaparHtml(mensaje)}`);
    }

    function actualizarTotalGeneral() {
        const total = calcularTotalGeneral();
        $(SELECTORES.total).text(formatearMoneda(total));
        $(SELECTORES.btnConfirmar).prop('disabled', cierreFinalizado || pendienteBloqueante || instrumentos.length === 0 || confirmando);
    }

    function calcularTotalGeneral() {
        return redondear(instrumentos.reduce(function (total, item) {
            return total + Number(item.ins_importe || 0);
        }, 0));
    }

    function normalizarInstrumentos(lista) {
        return lista.map(function (item) {
            return {
                ins_id: String(item.ins_id || '').trim(),
                ins_desc: String(item.ins_desc || '').trim(),
                ins_importe: parsearNumero(item.ins_importe, 0)
            };
        }).filter(function (item) {
            return item.ins_id;
        });
    }

    function normalizarNominaciones(lista) {
        return lista.map(function (item) {
            const nominacion = parsearNumero(item.nominacion, 0);
            const cantidad = parsearEntero(item.cantidad, 0);
            const importe = item.importe === undefined || item.importe === null
                ? nominacion * cantidad
                : parsearNumero(item.importe, 0);

            return {
                nominacion: nominacion,
                cantidad: cantidad,
                importe: redondear(importe)
            };
        }).filter(function (item) {
            return item.nominacion > 0;
        });
    }

    function construirMensajeExito(response) {
        const totalRendido = redondear(response?.total_rendido ?? calcularTotalGeneral());
        const mensajeServidor = String(response?.mensaje || '').trim();
        const mensajeVisible = !mensajeServidor || mensajeServidor.toUpperCase() === 'OK'
            ? `El cierre de caja fue confirmado correctamente con una rendicion final de ${formatearMoneda(totalRendido)}.`
            : mensajeServidor;

        return `<div class="text-center px-2">
            <i class='bx bx-check-circle text-golden' style="font-size: 4rem;"></i>
            <h4 class="text-golden mt-3 mb-2">PV cerrado</h4>
            <p class="fs-5 mb-2">${escaparHtml(mensajeVisible)}</p>
            <p class="text-muted mb-0">La caja quedo cerrada. Al aceptar, se cerrara la sesion del operador.</p>
            ${response?.resultado_id ? `<div class="small text-muted mt-2">Operacion: ${escaparHtml(response.resultado_id)}</div>` : ''}
        </div>`;
    }

    function activarTecladoCantidad() {
        if (typeof activarTecladoParaInput === 'function') {
            activarTecladoParaInput(SELECTORES.cantidad, {
                anchorSelector: SELECTORES.cantidadTecladoAncla,
                preferredSide: 'left'
            });
            return;
        }

        $(SELECTORES.cantidad).trigger('focus').trigger('select');
    }

    function posicionarTecladoCantidad() {
        if (typeof posicionarTecladoVirtual === 'function') {
            setTimeout(function () {
                posicionarTecladoVirtual(
                    SELECTORES.cantidad,
                    SELECTORES.cantidadTecladoAncla,
                    { preferredSide: 'left' }
                );
            }, 80);
        }
    }

    function ocultarTecladoCierre() {
        if (typeof ocultarTecladoVirtual === 'function') {
            ocultarTecladoVirtual();
            return;
        }

        if (typeof cerrarTecladoDigital === 'function') {
            cerrarTecladoDigital();
        }
    }

    function mostrarMensajeCantidad(mensaje) {
        $(SELECTORES.cantidadMensaje)
            .removeClass('d-none')
            .html(`<i class="bx bx-info-circle"></i> ${escaparHtml(mensaje)}`);
    }

    function mostrarLoaderCierre(mensaje) {
        if (typeof mostrarLoader === 'function') {
            mostrarLoader(mensaje || 'Procesando...');
        }
    }

    function ocultarLoaderCierre() {
        if (typeof ocultarLoader === 'function') {
            ocultarLoader();
        }
    }

    function bloquearPantallaCierre(mensaje) {
        const texto = mensaje || 'Procesando cierre de caja. Aguarde, no toque nada hasta que el proceso termine...';
        mostrarLoaderCierre(texto);

        let $overlay = $('#cierreBloqueoPantalla');
        if (!$overlay.length) {
            $overlay = $('<div id="cierreBloqueoPantalla" role="status" aria-live="polite"></div>');
            $overlay.css({
                position: 'fixed',
                inset: 0,
                zIndex: 999998,
                display: 'none',
                background: 'rgba(0, 0, 0, 0.48)',
                pointerEvents: 'auto'
            });
            $overlay.html('<div style="position:absolute;top:50%;left:50%;transform:translate(-50%,-50%);min-width:360px;max-width:560px;background:#fff;border:2px solid #d6a319;border-radius:8px;padding:26px;text-align:center;box-shadow:0 18px 50px rgba(0,0,0,.35);"><i class="bx bx-loader-alt bx-spin text-golden" style="font-size:3.5rem;"></i><div style="font-size:1.35rem;font-weight:700;margin-top:12px;color:#8a6410;">Cerrando PV</div><div class="cierre-bloqueo-mensaje" style="margin-top:8px;color:#333;"></div></div>');
            $('body').append($overlay);
        }

        $overlay.find('.cierre-bloqueo-mensaje').text(texto);
        $overlay.show();
        $('#modalCierreCaja button, #modalCierreCaja input, #modalCierreCaja select, #modalCierreCaja textarea').prop('disabled', true);
        $(SELECTORES.modal).addClass('cierre-confirmando');
    }

    function ocultarBloqueoPantallaCierre() {
        ocultarLoaderCierre();
        $('#cierreBloqueoPantalla').hide();
        $(SELECTORES.modal).removeClass('cierre-confirmando');
    }

    function desbloquearPantallaCierre() {
        ocultarBloqueoPantallaCierre();
        $('#modalCierreCaja button, #modalCierreCaja input, #modalCierreCaja select, #modalCierreCaja textarea').prop('disabled', false);
        actualizarTotalGeneral();
    }

    function mostrarMensaje(titulo, mensaje, tipo) {
        AbrirMensaje(
            titulo,
            mensaje,
            function () { $('#msjModal').modal('hide'); },
            false,
            ['Aceptar'],
            tipo || 'info!',
            null
        );
    }

    function permitirSoloEntero(event) {
        const teclasPermitidas = ['Backspace', 'Delete', 'Tab', 'Escape', 'Enter', 'ArrowLeft', 'ArrowRight', 'Home', 'End'];
        if (teclasPermitidas.includes(event.key) || event.ctrlKey || event.metaKey) {
            return;
        }

        if (!/^\d$/.test(event.key)) {
            event.preventDefault();
        }
    }

    function limitarSoloDigitos($input) {
        $input.val(String($input.val() || '').replace(/\D/g, ''));
    }

    function parsearEntero(valor, defecto) {
        const numero = parseInt(String(valor ?? '').replace(/\D/g, ''), 10);
        return Number.isFinite(numero) ? numero : defecto;
    }

    function parsearNumero(valor, defecto) {
        if (typeof valor === 'number') {
            return Number.isFinite(valor) ? valor : defecto;
        }

        let normalizado = String(valor ?? '').replace(/\$/g, '').replace(/\s/g, '').trim();
        if (!normalizado) {
            return defecto;
        }

        const ultimoPunto = normalizado.lastIndexOf('.');
        const ultimaComa = normalizado.lastIndexOf(',');
        if (ultimoPunto >= 0 && ultimaComa >= 0) {
            const separadorDecimal = ultimoPunto > ultimaComa ? '.' : ',';
            const separadorMiles = separadorDecimal === '.' ? ',' : '.';
            normalizado = normalizado.replaceAll(separadorMiles, '').replace(separadorDecimal, '.');
        } else if (ultimaComa >= 0) {
            normalizado = normalizado.replace(',', '.');
        }

        const numero = parseFloat(normalizado);
        return Number.isFinite(numero) ? numero : defecto;
    }

    function formatearMoneda(valor) {
        return new Intl.NumberFormat('en-US', {
            style: 'currency',
            currency: 'ARS',
            minimumFractionDigits: 2
        }).format(Number(valor || 0));
    }

    function formatearNumero(valor, decimales) {
        return new Intl.NumberFormat('en-US', {
            minimumFractionDigits: decimales,
            maximumFractionDigits: decimales
        }).format(Number(valor || 0));
    }

    function formatearFecha(valor) {
        if (!valor) {
            return '';
        }

        const fecha = new Date(valor);
        if (Number.isNaN(fecha.getTime())) {
            return String(valor);
        }

        return fecha.toLocaleDateString('es-AR');
    }

    function redondear(valor) {
        return Math.round(Number(valor || 0) * 100) / 100;
    }

    function logPaso(paso, datos) {
        if (datos === undefined) {
            console.log(LOG_PREFIX, paso);
            return;
        }
        console.log(LOG_PREFIX, paso, datos);
    }

    function logError(paso, datos) {
        if (datos === undefined) {
            console.error(LOG_PREFIX, paso);
            return;
        }
        console.error(LOG_PREFIX, paso, datos);
    }

    function escaparHtml(valor) {
        return String(valor ?? '')
            .replace(/&/g, '&amp;')
            .replace(/</g, '&lt;')
            .replace(/>/g, '&gt;')
            .replace(/"/g, '&quot;')
            .replace(/'/g, '&#39;');
    }

    function escaparSelector(valor) {
        if (window.CSS && typeof window.CSS.escape === 'function') {
            return window.CSS.escape(String(valor ?? ''));
        }

        return String(valor ?? '').replace(/([ #;&,.+*~':"!^$[\]()=>|/@])/g, '\\$1');
    }
})();





