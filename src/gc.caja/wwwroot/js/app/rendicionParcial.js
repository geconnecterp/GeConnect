(function () {
    'use strict';

    const LOG_PREFIX = '[Rendicion]';
    const SELECTORES = {
        modal: '#modalRendicionParcial',
        modalCantidad: '#modalRendicionCantidad',
        tbodyInstrumentos: '#tbodyRendicionInstrumentos',
        tbodyNominaciones: '#tbodyRendicionNominaciones',
        instrumentosCount: '#lblRendicionInstrumentosCount',
        instrumentoSeleccionado: '#lblRendicionInstrumentoSeleccionado',
        total: '#lblRendicionTotal',
        btnCancelar: '#btnRendicionCancelar',
        btnConfirmar: '#btnRendicionConfirmar',
        nomIndex: '#txtRendicionNomIndex',
        nomValor: '#txtRendicionNomValor',
        cantidad: '#txtRendicionCantidad',
        cantidadMensaje: '#rendicionCantidadMensaje',
        cantidadTecladoAncla: '#rendicionCantidadTecladoAncla',
        btnCantidadCancelar: '#btnRendicionCantidadCancelar',
        btnCantidadBorrar: '#btnRendicionCantidadBorrar',
        btnCantidadAceptar: '#btnRendicionCantidadAceptar'
    };

    let instrumentos = [];
    let nominacionesPorInstrumento = {};
    let instrumentoActivo = null;
    let confirmando = false;

    $(function () {
        logPaso('Inicializando modulo', {
            cargarInstrumentosUrl: window.rendicionCargarInstrumentosUrl,
            cargarNominacionesUrl: window.rendicionCargarNominacionesUrl,
            confirmarUrl: window.rendicionConfirmarUrl
        });

        registrarEventos();
        abrirModulo();
    });

    function registrarEventos() {
        $(document).on('click', '.rendicion-instrumento-row', function () {
            seleccionarInstrumento($(this).data('ins-id'));
        });

        $(document).on('click', '.btn-rendicion-editar-cantidad', function (event) {
            event.preventDefault();
            event.stopPropagation();
            abrirEditorCantidad($(this).data('index'));
        });

        $(document).on('click', SELECTORES.btnCantidadCancelar, function () {
            ocultarTecladoRendicion();
            $(SELECTORES.modalCantidad).modal('hide');
        });

        $(document).on('click', SELECTORES.btnCantidadBorrar, function () {
            ocultarTecladoRendicion();
            guardarCantidadNominal(0);
        });

        $(document).on('click', SELECTORES.btnCantidadAceptar, function () {
            const cantidad = parsearEntero($(SELECTORES.cantidad).val(), -1);
            if (cantidad < 0) {
                mostrarMensajeCantidad('La cantidad debe ser cero o mayor.');
                return;
            }

            ocultarTecladoRendicion();
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
                ocultarTecladoRendicion();
            });

        $(document).on('click', SELECTORES.btnCancelar, function () {
            window.location.href = window.rendicionMenuCajaUrl || '/';
        });

        $(document).on('click', SELECTORES.btnConfirmar, confirmarRendicion);
    }

    function abrirModulo() {
        $(SELECTORES.modal).modal('show');
        cargarInstrumentos();
    }

    function cargarInstrumentos() {
        const url = String(window.rendicionCargarInstrumentosUrl || '').trim();
        if (!url) {
            mostrarMensaje('Error', 'No se encontro la URL para cargar instrumentos.', 'error!');
            return;
        }

        logPaso('Cargando instrumentos');
        mostrarLoaderRendicion('Cargando instrumentos de rendicion...');

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
                ocultarLoaderRendicion();
            });
    }

    function seleccionarInstrumento(insId) {
        const instrumento = instrumentos.find(x => x.ins_id === String(insId || '').trim());
        if (!instrumento) {
            return;
        }

        instrumentoActivo = instrumento;
        $('.rendicion-instrumento-row').removeClass('table-success');
        $(`.rendicion-instrumento-row[data-ins-id="${escaparSelector(instrumento.ins_id)}"]`).addClass('table-success');
        $(SELECTORES.instrumentoSeleccionado).text(`${instrumento.ins_id} - ${instrumento.ins_desc}`);

        if (nominacionesPorInstrumento[instrumento.ins_id]) {
            renderNominaciones(nominacionesPorInstrumento[instrumento.ins_id]);
            return;
        }

        cargarNominaciones(instrumento.ins_id);
    }

    function cargarNominaciones(insId) {
        const url = String(window.rendicionCargarNominacionesUrl || '').trim();
        if (!url) {
            mostrarMensaje('Error', 'No se encontro la URL para cargar nominaciones.', 'error!');
            return;
        }

        logPaso('Cargando nominaciones', { ins_id: insId });
        mostrarLoaderRendicion('Cargando nominaciones...');
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
                ocultarLoaderRendicion();
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

    function ocultarTecladoRendicion() {
        if (typeof ocultarTecladoVirtual === 'function') {
            ocultarTecladoVirtual();
            return;
        }

        if (typeof cerrarTecladoDigital === 'function') {
            cerrarTecladoDigital();
        }
    }

    function confirmarRendicion() {
        if (confirmando) {
            return;
        }

        const total = calcularTotalGeneral();
        if (total <= 0) {
            mostrarMensaje('Atencion', 'Debe cargar al menos un importe para confirmar la rendicion.', 'warn!');
            return;
        }

        AbrirMensaje(
            'Confirmar Rendicion',
            `Esta por confirmar una rendicion parcial por <strong>${formatearMoneda(total)}</strong>.<br><br>¿Desea continuar?`,
            function (respuesta) {
                $('#msjModal').modal('hide');
                if (respuesta === 'SI') {
                    ejecutarConfirmacion();
                }
            },
            true,
            ['Confirmar', 'Cancelar'],
            'quest!',
            null
        );
    }

    function ejecutarConfirmacion() {
        const url = String(window.rendicionConfirmarUrl || '').trim();
        if (!url) {
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

        logPaso('Confirmando rendicion', payload);
        confirmando = true;
        mostrarLoaderRendicion('Confirmando rendicion parcial. Aguarde, no toque nada hasta que el proceso termine...');
        $(SELECTORES.btnConfirmar).prop('disabled', true).html('<i class="bx bx-loader-alt bx-spin"></i> CONFIRMANDO');

        $.ajax({
            url: url,
            type: 'POST',
            dataType: 'json',
            contentType: 'application/json; charset=utf-8',
            data: JSON.stringify(payload),
            timeout: 45000
        })
            .done(function (response) {
                logPaso('Response confirmar rendicion', response);
                if (!response || response.ok !== true) {
                    mostrarMensaje('Atencion', response?.mensaje || 'No se pudo confirmar la rendicion.', 'warn!');
                    return;
                }

                AbrirMensaje(
                    'Rendicion Confirmada',
                    construirMensajeExito(response),
                    function () {
                        $('#msjModal').modal('hide');
                        window.location.href = window.rendicionMenuCajaUrl || '/';
                    },
                    false,
                    ['Aceptar'],
                    'success!',
                    null
                );
            })
            .fail(function (xhr) {
                logError('Error AJAX confirmar rendicion', xhr?.responseJSON || xhr?.responseText || xhr?.status);
                mostrarMensaje('Error', xhr?.responseJSON?.mensaje || 'Ocurrio un error al confirmar la rendicion.', 'error!');
            })
            .always(function () {
                confirmando = false;
                ocultarLoaderRendicion();
                $(SELECTORES.btnConfirmar).html('<i class="bx bx-check-circle"></i> CONFIRMAR');
                actualizarTotalGeneral();
            });
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
                <tr class="rendicion-instrumento-row" data-ins-id="${escaparHtml(item.ins_id)}">
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
                        <button type="button" class="btn btn-sm btn-golden btn-rendicion-editar-cantidad" data-index="${index}" title="Editar cantidad">
                            <i class="bx bx-edit-alt"></i>
                        </button>
                    </td>
                </tr>
            `;
        }).join('');

        $(SELECTORES.tbodyNominaciones).html(html);
    }

    function actualizarTotalGeneral() {
        const total = calcularTotalGeneral();
        $(SELECTORES.total).text(formatearMoneda(total));
        $(SELECTORES.btnConfirmar).prop('disabled', total <= 0 || confirmando);
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
        const mensaje = response?.mensaje || 'La rendicion parcial fue confirmada correctamente.';
        return `<div class="text-center">
            <i class='bx bx-check-circle text-golden' style="font-size: 4rem;"></i>
            <h4 class="text-golden mt-3">${escaparHtml(mensaje)}</h4>
            ${response?.resultado_id ? `<div class="small text-muted mt-2">${escaparHtml(response.resultado_id)}</div>` : ''}
        </div>`;
    }

    function mostrarMensajeCantidad(mensaje) {
        $(SELECTORES.cantidadMensaje)
            .removeClass('d-none')
            .html(`<i class="bx bx-info-circle"></i> ${escaparHtml(mensaje)}`);
    }

    function mostrarLoaderRendicion(mensaje) {
        if (typeof mostrarLoader === 'function') {
            mostrarLoader(mensaje || 'Procesando...');
        }
    }

    function ocultarLoaderRendicion() {
        if (typeof ocultarLoader === 'function') {
            ocultarLoader();
        }
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
            .replace(/'/g, '&#039;');
    }

    function escaparSelector(valor) {
        if (window.CSS && typeof window.CSS.escape === 'function') {
            return window.CSS.escape(String(valor || ''));
        }
        return String(valor || '').replace(/([ #;?%&,.+*~\':"!^$[\]()=>|/@])/g, '\\$1');
    }
})();
