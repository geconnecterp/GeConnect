// ═══════════════════════════════════════════════════════════════════
// MÓDULO: NOTA DE CRÉDITO POR DEVOLUCIÓN
// PASO 1: IDENTIFICACIÓN Y VALIDACIÓN DEL COMPROBANTE ORIGEN
// ═══════════════════════════════════════════════════════════════════

window.NCDevolucion = window.NCDevolucion || {};

(function (namespace) {
    'use strict';
    // ═══════════════════════════════════════════════════════════════════
    // TRAZABILIDAD Y LOGS DEL MÓDULO
    // ═══════════════════════════════════════════════════════════════════

    let trazabilidadHabilitada = true;
    let trazabilidadDetallada = true;

    const TRAZA = {
        operacionId: crearIdOperacion(),
        inicioMs: Date.now(),
        secuenciaAjax: 0
    };

    function crearIdOperacion() {
        const fecha = new Date()
            .toISOString()
            .replace(/[-:.TZ]/g, '')
            .substring(0, 14);

        const aleatorio = Math.random()
            .toString(36)
            .substring(2, 7)
            .toUpperCase();

        return `NCDEV-${fecha}-${aleatorio}`;
    }

    function tiempoTranscurrido() {
        return `${Date.now() - TRAZA.inicioMs}ms`;
    }

    function prefijoLog() {
        return `[NCDEV][${TRAZA.operacionId}][+${tiempoTranscurrido()}]`;
    }

    function logInfo(etapa, detalle) {
        if (!trazabilidadHabilitada) {
            return;
        }

        if (typeof detalle === 'undefined') {
            console.log(`${prefijoLog()} ${etapa}`);
            return;
        }

        console.log(`${prefijoLog()} ${etapa}`, detalle);
    }

    function logAdvertencia(etapa, detalle) {
        if (!trazabilidadHabilitada) {
            return;
        }

        console.warn(`${prefijoLog()} ${etapa}`, detalle ?? '');
    }

    function logError(etapa, detalle, error) {
        if (!trazabilidadHabilitada) {
            return;
        }

        console.error(
            `${prefijoLog()} ${etapa}`,
            detalle ?? '',
            error ?? ''
        );
    }

    function logDebug(etapa, detalle) {
        if (!trazabilidadHabilitada || !trazabilidadDetallada) {
            return;
        }

        console.debug(`${prefijoLog()} ${etapa}`, detalle ?? '');
    }

    function normalizarTextoLog(valor, maximo = 160) {
        const texto = String(valor ?? '').trim();

        if (texto.length <= maximo) {
            return texto;
        }

        return `${texto.substring(0, maximo)}...`;
    }

    function enmascararDocumento(valor) {
        const texto = String(valor ?? '').trim();

        if (!texto) {
            return '';
        }

        if (texto.length <= 4) {
            return '****';
        }

        return `****${texto.substring(texto.length - 4)}`;
    }

    function resumirComprobante(comprobante) {
        if (!comprobante) {
            return null;
        }

        return {
            tco_id: comprobante.tco_id ?? '',
            tco_desc: comprobante.tco_desc ?? '',
            cm_compte: comprobante.cm_compte ?? '',
            cm_repetido: comprobante.cm_repetido ?? 0,
            cm_fecha: comprobante.cm_fecha ?? '',
            cm_total: comprobante.cm_total ?? 0,
            cm_cuit: enmascararDocumento(comprobante.cm_cuit),
            nc_tco_id: comprobante.nc_tco_id ?? '',
            nc_tco_letra: comprobante.nc_tco_letra ?? '',
            nc_tco_desc: comprobante.nc_tco_desc ?? '',
            nc_ctacte: comprobante.nc_ctacte ?? 0,
            nc_dv_dist: comprobante.nc_dv_dist ?? 0,
            nc_dv_pago_diferido: comprobante.nc_dv_pago_diferido ?? 0
        };
    }

    function resumirRespuestaAjax(response) {
        if (!response || typeof response !== 'object') {
            return {
                tipo: typeof response,
                valor: normalizarTextoLog(response)
            };
        }

        const resumen = {
            ok: response.ok,
            codigo: response.codigo ?? '',
            mensaje: response.mensaje ?? '',
            requiereSeleccion: response.requiereSeleccion,
            modalidad: response.modalidad ?? '',
            cargarTodoDetalle: response.cargarTodoDetalle
        };

        if (Array.isArray(response.datos)) {
            resumen.cantidadDatos = response.datos.length;
        }

        if (Array.isArray(response.candidatos)) {
            resumen.cantidadCandidatos = response.candidatos.length;

            resumen.candidatos = response.candidatos.map(function (item) {
                return {
                    indice: item.indice,
                    tco_id: item.tco_id,
                    cm_compte: item.cm_compte,
                    cm_repetido: item.cm_repetido,
                    cm_fecha: item.cm_fecha,
                    cm_total: item.cm_total,
                    bloqueado: item.bloqueado,
                    motivo_bloqueo: item.motivo_bloqueo
                };
            });
        }

        if (response.comprobante) {
            resumen.comprobante = resumirComprobante(response.comprobante);
        }

        return resumen;
    }

    function resumirRequestAjax(data) {
        if (!data) {
            return {};
        }

        let request = data;

        if (typeof data === 'string') {
            try {
                request = JSON.parse(data);
            } catch {
                return {
                    body: normalizarTextoLog(data)
                };
            }
        }

        if (typeof request !== 'object') {
            return {
                body: normalizarTextoLog(request)
            };
        }

        return {
            tcoId: request.tcoId ?? '',
            puntoVenta: request.puntoVenta ?? '',
            numero: request.numero ?? '',
            indice: request.indice,
            cargarTodoDetalle: request.cargarTodoDetalle
        };
    }

    function resumirErrorAjax(xhr, textStatus, errorThrown) {
        const contentType = xhr?.getResponseHeader?.('content-type') ?? '';

        return {
            httpStatus: xhr?.status ?? 0,
            textStatus: textStatus ?? '',
            errorThrown: normalizarTextoLog(errorThrown),
            contentType: contentType,
            mensajeApi: xhr?.responseJSON?.mensaje ??
                xhr?.responseJSON?.message ??
                '',
            respuestaTexto: normalizarTextoLog(xhr?.responseText, 300)
        };
    }

    function esPeticionNcDevolucion(url) {
        const urlNormalizada = String(url ?? '').toLowerCase();

        if (!urlNormalizada) {
            return false;
        }

        return urlNormalizada.includes('/facturacion/notacredito/');
    }

    function instalarTrazabilidadAjax() {
        if (window.__ncDevolucionTrazabilidadAjaxInstalada === true) {
            logDebug(
                'TRAZA AJAX',
                'La trazabilidad AJAX ya estaba instalada.'
            );

            return;
        }

        window.__ncDevolucionTrazabilidadAjaxInstalada = true;

        $.ajaxPrefilter(function (options, originalOptions, jqXHR) {
            if (!esPeticionNcDevolucion(options.url)) {
                return;
            }

            const correlacionAjax =
                `${TRAZA.operacionId}-AJAX-${++TRAZA.secuenciaAjax}`;

            const inicioAjax = performance.now();

            logInfo('AJAX → Solicitud iniciada', {
                correlacionAjax: correlacionAjax,
                metodo: String(options.type ?? 'GET').toUpperCase(),
                url: options.url,
                contentType: options.contentType ?? '',
                dataType: options.dataType ?? '',
                request: resumirRequestAjax(options.data)
            });

            jqXHR.done(function (data, textStatus, xhr) {
                const duracionMs = Math.round(
                    performance.now() - inicioAjax
                );

                const contentType =
                    xhr?.getResponseHeader?.('content-type') ?? '';

                logInfo('AJAX ← Respuesta recibida', {
                    correlacionAjax: correlacionAjax,
                    httpStatus: xhr?.status ?? 0,
                    textStatus: textStatus,
                    contentType: contentType,
                    duracionMs: duracionMs,
                    response: resumirRespuestaAjax(data)
                });

                if (contentType.toLowerCase().includes('text/html')) {
                    logAdvertencia(
                        'AJAX ← Se recibió HTML en lugar de JSON',
                        {
                            correlacionAjax: correlacionAjax,
                            posibleMotivo:
                                'La sesión puede haber expirado o [Authorize] pudo redirigir al login.'
                        }
                    );
                }
            });

            jqXHR.fail(function (xhr, textStatus, errorThrown) {
                const duracionMs = Math.round(
                    performance.now() - inicioAjax
                );

                logError(
                    'AJAX ✖ Error de solicitud',
                    {
                        correlacionAjax: correlacionAjax,
                        duracionMs: duracionMs,
                        ...resumirErrorAjax(
                            xhr,
                            textStatus,
                            errorThrown
                        )
                    }
                );
            });
        });

        logInfo(
            'TRAZA AJAX',
            'Trazabilidad AJAX instalada correctamente.'
        );
    }

    function instalarTrazabilidadInterfaz() {
        const selectorBotones = [
            '#btnValidarComprobanteOrigen',
            '#btnCancelarNcDevolucion',
            '#btnVolverIdentificacionComprobante',
            '.btn-seleccionar-repetido'
        ].join(', ');

        $(document).on(
            'click.ncDevolucionTraza',
            selectorBotones,
            function () {
                logInfo('UI → Click detectado', {
                    id: this.id ?? '',
                    clases: this.className ?? '',
                    indice: $(this).data('indice')
                });
            }
        );

        $(document).on(
            'change.ncDevolucionTraza',
            '#ddlTipoComprobanteOrigen',
            function () {
                logInfo('UI → Tipo de comprobante seleccionado', {
                    tcoId: String($(this).val() ?? '').trim(),
                    texto: String(
                        $(this)
                            .find('option:selected')
                            .text() ?? ''
                    ).trim()
                });
            }
        );

        $(document).on(
            'blur.ncDevolucionTraza',
            '#txtPuntoVentaOrigen, #txtNumeroComprobanteOrigen',
            function () {
                logDebug('UI → Campo completado', {
                    campo: this.id,
                    valor: String($(this).val() ?? '').trim()
                });
            }
        );

        $(document).on(
            'keydown.ncDevolucionTraza',
            '#txtPuntoVentaOrigen, #txtNumeroComprobanteOrigen',
            function (event) {
                if (event.key === 'Enter') {
                    logInfo('UI → Enter presionado', {
                        campo: this.id,
                        valor: String($(this).val() ?? '').trim()
                    });
                }
            }
        );

        const modales = [
            '#modalIdentificarComprobanteOrigen',
            '#modalSeleccionComprobanteRepetido'
        ];

        modales.forEach(function (selector) {
            const elemento = document.querySelector(selector);

            if (!elemento) {
                logAdvertencia(
                    'UI → Modal no encontrado al instalar trazas',
                    { selector: selector }
                );

                return;
            }

            elemento.addEventListener('show.bs.modal', function () {
                logInfo('UI → Modal abriéndose', {
                    modal: selector
                });
            });

            elemento.addEventListener('shown.bs.modal', function () {
                logInfo('UI → Modal visible', {
                    modal: selector
                });
            });

            elemento.addEventListener('hide.bs.modal', function () {
                logInfo('UI → Modal cerrándose', {
                    modal: selector
                });
            });

            elemento.addEventListener('hidden.bs.modal', function () {
                logInfo('UI → Modal cerrado', {
                    modal: selector
                });
            });
        });

        logInfo(
            'TRAZA UI',
            'Trazabilidad de interfaz instalada correctamente.'
        );
    }

    namespace.traza = {
        activar: function (activo) {
            trazabilidadHabilitada = activo === true;

            console.log(
                `[NCDEV] Trazabilidad ${trazabilidadHabilitada
                    ? 'ACTIVADA'
                    : 'DESACTIVADA'}`
            );
        },

        detallado: function (activo) {
            trazabilidadDetallada = activo === true;

            console.log(
                `[NCDEV] Log detallado ${trazabilidadDetallada
                    ? 'ACTIVADO'
                    : 'DESACTIVADO'}`
            );
        },

        estado: function () {
            return {
                operacionId: TRAZA.operacionId,
                trazabilidadHabilitada: trazabilidadHabilitada,
                trazabilidadDetallada: trazabilidadDetallada,
                milisegundosDesdeInicio: Date.now() - TRAZA.inicioMs,
                solicitudesAjax: TRAZA.secuenciaAjax
            };
        }
    };

    let modalIdentificacion = null;
    let modalRepetidos = null;
    let guardandoModalidadCarga = false;

    const SELECTORES = {
        modalIdentificacion: '#modalIdentificarComprobanteOrigen',
        modalRepetidos: '#modalSeleccionComprobanteRepetido',

        tipoComprobante: '#ddlTipoComprobanteOrigen',
        puntoVenta: '#txtPuntoVentaOrigen',
        numeroComprobante: '#txtNumeroComprobanteOrigen',

        btnValidar: '#btnValidarComprobanteOrigen',
        btnCancelar: '#btnCancelarNcDevolucion',
        btnVolverIdentificacion: '#btnVolverIdentificacionComprobante',

        spinnerValidar: '#spnValidarComprobanteOrigen',
        iconoValidar: '#icoValidarComprobanteOrigen',

        alertaInline: '#alertValidacionComprobanteOrigen',
        tablaRepetidos: '#tbodyComprobantesRepetidos'
    };

    const DESTINOS_CANCELACION = Object.freeze({
        MENU_CAJA: 'MENU_CAJA',
        REINICIAR_NOTA_CREDITO: 'REINICIAR_NOTA_CREDITO'
    });

    const SELECTORES_CANCELACION = Object.freeze({
        IDENTIFICACION: '#btnCancelarNcDevolucion',
        PRODUCTOS: '#btnCancelarNcDevolucionProductos'
    });

    $(function () {
        logInfo('INICIO DEL MÓDULO', {
            ruta: window.location.pathname,
            urlCompleta: window.location.href,
            usuarioNavegador: navigator.userAgent
        });

        instalarTrazabilidadAjax();
        instalarTrazabilidadInterfaz();

        inicializarModales();
        inicializarEventos();
        iniciarIdentificacionComprobante();

        logInfo(
            'INICIO DEL MÓDULO',
            'Inicialización finalizada. Esperando interacción del cajero.'
        );
    });

    function inicializarModales() {
        logInfo('INICIALIZACIÓN', 'Inicializando modales de NC por Devolución.');

        const elementoIdentificacion = document.querySelector(
            SELECTORES.modalIdentificacion
        );

        const elementoRepetidos = document.querySelector(
            SELECTORES.modalRepetidos
        );

        if (!elementoIdentificacion || !elementoRepetidos) {
            console.error(
                '[NC Devolución] No se encontraron los modales requeridos.'
            );

            mostrarError(
                'No se pudieron inicializar las ventanas de identificación de comprobante.'
            );

            logError('INICIALIZACIÓN', {
                mensaje: 'No se encontraron uno o más modales requeridos.',
                existeModalIdentificacion: Boolean(elementoIdentificacion),
                existeModalRepetidos: Boolean(elementoRepetidos)
            });

            return;
        }

        modalIdentificacion = bootstrap.Modal.getOrCreateInstance(
            elementoIdentificacion
        );

        modalRepetidos = bootstrap.Modal.getOrCreateInstance(
            elementoRepetidos
        );

        $(elementoIdentificacion).on('shown.bs.modal', function () {
            logInfo('UI → Modal de identificación visible', {
                focoEsperado: 'ddlTipoComprobanteOrigen'
            });

            setTimeout(function () {
                $(SELECTORES.tipoComprobante).trigger('focus');
            }, 150);
        });

        $(elementoRepetidos).on('shown.bs.modal', function () {
            logInfo('UI → Modal de identificación visible', {
                focoEsperado: 'ddlTipoComprobanteOrigen'
            });

            setTimeout(function () {
                $('#tbodyComprobantesRepetidos .btn-seleccionar-repetido:not(:disabled)')
                    .first()
                    .trigger('focus');
            }, 150);
        });
    }

    function inicializarEventos() {
        $(SELECTORES.puntoVenta)
            .on('input', function () {
                normalizarSoloDigitos($(this), 4);
            })
            .on('keydown', function (event) {
                if (event.key === 'Enter') {
                    event.preventDefault();
                    $(SELECTORES.numeroComprobante).trigger('focus');
                }
            });

        $(SELECTORES.numeroComprobante)
            .on('input', function () {
                normalizarSoloDigitos($(this), 8);
            })
            .on('keydown', function (event) {
                if (event.key === 'Enter') {
                    event.preventDefault();
                    validarComprobanteOrigen();
                }
            });

        $(SELECTORES.tipoComprobante).on('change', function () {
            limpiarAlertaInline();
        });

        $(SELECTORES.btnValidar).on('click', function () {
            validarComprobanteOrigen();
        });

        $(SELECTORES.btnCancelar).on('click', function () {
            confirmarCancelacion({
                origen: 'IDENTIFICACION',
                destino: DESTINOS_CANCELACION.MENU_CAJA,
                selectorBoton: SELECTORES_CANCELACION.IDENTIFICACION
            });
        });

        $(SELECTORES.btnVolverIdentificacion).on('click', function () {
            modalRepetidos.hide();

            setTimeout(function () {
                limpiarAlertaInline();
                modalIdentificacion.show();
            }, 250);
        });

        $(document).on(
            'click',
            '.btn-seleccionar-repetido',
            function () {
                const indice = Number($(this).data('indice'));

                if (!Number.isInteger(indice) || indice < 0) {
                    mostrarError(
                        'La selección del comprobante no es válida.'
                    );

                    return;
                }

                seleccionarComprobanteRepetido(indice);
            }
        );

        document.addEventListener(
            'ncdev:solicitar-cancelacion',
            function () {
                logInfo('CANCELACIÓN', {
                    accion: 'Solicitud de cancelación recibida desde modal de productos.',
                    destino: DESTINOS_CANCELACION.REINICIAR_NOTA_CREDITO
                });

                confirmarCancelacion({
                    origen: 'PRODUCTOS',
                    destino: DESTINOS_CANCELACION.REINICIAR_NOTA_CREDITO,
                    selectorBoton: SELECTORES_CANCELACION.PRODUCTOS
                });
            }
        );
    }

    function iniciarIdentificacionComprobante() {
        if (
            !validarUrlDisponible(
                'ncDevolucionObtenerTiposUrl',
                window.ncDevolucionObtenerTiposUrl
            )
        ) {
            return;
        }

        cargarTiposComprobante();
    }

    function cargarTiposComprobante() {
        logInfo('ETAPA 1', {
            accion: 'Solicitando tipos de comprobante habilitados.',
            url: window.ncDevolucionObtenerTiposUrl
        });

        bloquearFormulario(true);
        limpiarAlertaInline();

        $.ajax({
            url: window.ncDevolucionObtenerTiposUrl,
            type: 'GET',
            dataType: 'json',
            timeout: 15000
        })
            .done(function (response) {
                if (!response || response.ok !== true) {
                    mostrarError(
                        response?.mensaje ||
                        'No fue posible obtener los tipos de comprobante.'
                    );

                    return;
                }

                const tipos = Array.isArray(response.datos)
                    ? response.datos
                    : [];

                logInfo('ETAPA 1', {
                    accion: 'Tipos de comprobante procesados.',
                    cantidad: tipos.length,
                    tipos: tipos.map(function (tipo) {
                        return {
                            tco_id: tipo.tco_id,
                            tco_desc: tipo.tco_desc,
                            tco_letra: tipo.tco_letra,
                            tco_tipo: tipo.tco_tipo
                        };
                    })
                });

                if (tipos.length === 0) {
                    mostrarError(
                        'No se encontraron tipos de comprobante habilitados para devolución.'
                    );

                    return;
                }

                const $select = $(SELECTORES.tipoComprobante);

                $select.empty();
                $select.append(
                    '<option value="">Seleccione un tipo de comprobante</option>'
                );

                tipos.forEach(function (tipo) {
                    const id = String(tipo.tco_id || '').trim();
                    const descripcion = String(
                        tipo.tco_desc || ''
                    ).trim();

                    const letra = String(
                        tipo.tco_letra || ''
                    ).trim();

                    if (!id) {
                        return;
                    }

                    const texto = letra
                        ? `${id} - ${descripcion} (${letra})`
                        : `${id} - ${descripcion}`;

                    $select.append(
                        $('<option>', {
                            value: id,
                            text: texto
                        })
                    );
                });

                bloquearFormulario(false);

                logInfo('ETAPA 1', {
                    accion: 'Abriendo modal de identificación.',
                    cantidadTiposDisponibles: tipos.length
                });

                modalIdentificacion.show();
            })
            .fail(function (xhr, status, error) {
                console.error(
                    '[NC Devolución] Error al cargar tipos de comprobante:',
                    {
                        status: xhr?.status,
                        textStatus: status,
                        error: error
                    }
                );

                logError(
                    'ETAPA 1',
                    {
                        accion: 'Falló la carga de tipos de comprobante.',
                        ...resumirErrorAjax(xhr, status, error)
                    }
                );

                if (
                    typeof esSesionExpirada === 'function' &&
                    esSesionExpirada(xhr?.status)
                ) {
                    return;
                }

                mostrarError(
                    'No fue posible cargar los tipos de comprobante. Intente nuevamente.'
                );
            });
    }

    function validarComprobanteOrigen() {
        limpiarAlertaInline();

        const tcoId = String(
            $(SELECTORES.tipoComprobante).val() || ''
        ).trim();

        const puntoVenta = String(
            $(SELECTORES.puntoVenta).val() || ''
        ).trim();

        const numero = String(
            $(SELECTORES.numeroComprobante).val() || ''
        ).trim();

        logInfo('ETAPA 2', {
            accion: 'Inicio de validación de comprobante origen.',
            tcoId: tcoId,
            puntoVenta: puntoVenta,
            numero: numero
        });

        const mensajeValidacion = validarDatosIngresados(
            tcoId,
            puntoVenta,
            numero
        );

        logDebug('ETAPA 2', {
            accion: 'Validación local de campos finalizada.',
            valido: !mensajeValidacion,
            mensaje: mensajeValidacion
        });

        if (mensajeValidacion) {
            mostrarAlertaInline(mensajeValidacion, 'warning');
            enfocarPrimerCampoInvalido(tcoId, puntoVenta, numero);
            return;
        }

        if (
            !validarUrlDisponible(
                'ncDevolucionValidarComprobanteUrl',
                window.ncDevolucionValidarComprobanteUrl
            )
        ) {
            return;
        }

        cambiarEstadoValidacion(true);

        $.ajax({
            url: window.ncDevolucionValidarComprobanteUrl,
            type: 'POST',
            contentType: 'application/json; charset=utf-8',
            dataType: 'json',
            timeout: 20000,
            data: JSON.stringify({
                tcoId: tcoId,
                puntoVenta: puntoVenta,
                numero: numero
            })
        })
            .done(function (response) {

                logInfo('ETAPA 2', {
                    accion: 'Respuesta de validación de comprobante recibida.',
                    response: resumirRespuestaAjax(response)
                });

                if (!response || response.ok !== true) {
                    const mensaje = response?.mensaje ||
                        'No fue posible validar el comprobante original.';

                    mostrarAlertaInline(mensaje, 'danger');
                    return;
                }

                logInfo('ETAPA 2', {
                    accion: 'Evaluando si el comprobante requiere selección por repetición.',
                    requiereSeleccion: response.requiereSeleccion === true,
                    cantidadCandidatos: Array.isArray(response.candidatos)
                        ? response.candidatos.length
                        : 0
                });

                if (response.requiereSeleccion === true) {
                    const candidatos = Array.isArray(response.candidatos)
                        ? response.candidatos
                        : [];

                    if (candidatos.length === 0) {
                        mostrarAlertaInline(
                            'No se recibieron comprobantes para seleccionar.',
                            'danger'
                        );

                        return;
                    }

                    renderizarCandidatosRepetidos(candidatos);

                    modalIdentificacion.hide();
                    logInfo('UI → Modal de repetidos visible', {
                        focoEsperado: 'primer botón Seleccionar disponible'
                    });
                    setTimeout(function () {
                        modalRepetidos.show();
                    }, 250);

                    return;
                }

                if (!response.comprobante) {
                    mostrarAlertaInline(
                        'La validación finalizó sin datos del comprobante seleccionado.',
                        'danger'
                    );

                    return;
                }

                logInfo('ETAPA 2', {
                    accion: 'Comprobante único validado. Se procesará como seleccionado.',
                    comprobante: resumirComprobante(response.comprobante)
                });

                procesarComprobanteSeleccionado(
                    response.comprobante
                );
            })
            .fail(function (xhr, status, error) {
                logError(
                    'ETAPA 2',
                    {
                        accion: 'Error HTTP o de red al validar comprobante.',
                        ...resumirErrorAjax(xhr, status, error)
                    }
                );
                console.error(
                    '[NC Devolución] Error al validar comprobante:',
                    {
                        status: xhr?.status,
                        textStatus: status,
                        error: error,
                        response: xhr?.responseJSON
                    }
                );

                if (
                    typeof esSesionExpirada === 'function' &&
                    esSesionExpirada(xhr?.status)
                ) {
                    return;
                }

                const mensaje = xhr?.responseJSON?.mensaje ||
                    'Ocurrió un error al validar el comprobante original.';

                mostrarAlertaInline(mensaje, 'danger');
            })
            .always(function () {
                cambiarEstadoValidacion(false);
            });
    }

    function seleccionarComprobanteRepetido(indice) {

        logInfo('ETAPA 3', {
            accion: 'Selección de comprobante repetido solicitada.',
            indice: indice
        });

        if (
            !validarUrlDisponible(
                'ncDevolucionSeleccionarRepetidoUrl',
                window.ncDevolucionSeleccionarRepetidoUrl
            )
        ) {
            return;
        }

        const $botones = $(
            '#tbodyComprobantesRepetidos .btn-seleccionar-repetido'
        );

        $botones.prop('disabled', true);

        $.ajax({
            url: window.ncDevolucionSeleccionarRepetidoUrl,
            type: 'POST',
            contentType: 'application/json; charset=utf-8',
            dataType: 'json',
            timeout: 20000,
            data: JSON.stringify({
                indice: indice
            })
        })
            .done(function (response) {

                logInfo('ETAPA 3', {
                    accion: 'Respuesta de selección de repetido recibida.',
                    response: resumirRespuestaAjax(response)
                });

                if (!response || response.ok !== true) {
                    mostrarError(
                        response?.mensaje ||
                        'No fue posible seleccionar el comprobante.'
                    );

                    return;
                }

                if (!response.comprobante) {
                    mostrarError(
                        'La selección finalizó sin datos del comprobante.'
                    );

                    return;
                }

                logInfo('ETAPA 3', {
                    accion: 'Comprobante repetido seleccionado correctamente.',
                    comprobante: resumirComprobante(response.comprobante)
                });

                procesarComprobanteSeleccionado(
                    response.comprobante
                );
            })
            .fail(function (xhr, status, error) {

                logError(
                    'ETAPA 3',
                    {
                        accion: 'Error al seleccionar comprobante repetido.',
                        ...resumirErrorAjax(xhr, status, error)
                    }
                );

                console.error(
                    '[NC Devolución] Error al seleccionar repetido:',
                    {
                        status: xhr?.status,
                        textStatus: status,
                        error: error
                    }
                );

                if (
                    typeof esSesionExpirada === 'function' &&
                    esSesionExpirada(xhr?.status)
                ) {
                    return;
                }

                mostrarError(
                    xhr?.responseJSON?.mensaje ||
                    'Ocurrió un error al seleccionar el comprobante.'
                );
            })
            .always(function () {
                $botones.prop('disabled', false);
            });
    }

    function notificarComprobanteOrigenValidado(comprobante) {
        const detalle = {
            comprobante: comprobante,
            fecha: new Date().toISOString()
        };

        logInfo('ETAPA 4', {
            accion: 'Notificando comprobante validado al módulo de productos.',
            comprobante: resumirComprobante(comprobante)
        });

        document.dispatchEvent(
            new CustomEvent(
                'ncdev:comprobante-origen-validado',
                {
                    detail: detalle
                }
            )
        );
    }

    function preguntarCargaInicialDetalle() {
        logInfo('ETAPA 5', {
            accion: 'Mostrando pregunta de modalidad de carga.',
            pregunta: '¿Desea cargar todo el detalle del comprobante original?'
        });

        AbrirMensaje(
            'Carga del detalle',
            `<div class="text-center">
            <i class="bx bx-package text-primary"
               style="font-size: 3rem;"></i>

            <p class="mt-3 mb-2 fw-semibold">
                ¿Desea cargar todo el detalle del comprobante original?
            </p>

            <small class="text-muted">
                Si selecciona <strong>Sí</strong>, el sistema cargará todos los productos
                facturados. Si selecciona <strong>No</strong>, podrá seleccionar los
                productos a devolver manualmente.
            </small>
        </div>`,
            function (respuesta) {
                logInfo('ETAPA 5', {
                    accion: 'Respuesta recibida desde AbrirMensaje.',
                    respuestaOriginal: respuesta,
                    respuestaNormalizada: normalizarRespuestaMensaje(respuesta)
                });

                if (esRespuestaPositiva(respuesta)) {
                    definirModalidadCargaInicial(true);
                    return;
                }

                if (esRespuestaNegativa(respuesta)) {
                    definirModalidadCargaInicial(false);
                    return;
                }

                logAdvertencia('ETAPA 5', {
                    accion: 'Respuesta no reconocida. Se volverá a preguntar.',
                    respuesta: respuesta
                });

                $('#msjModal').modal('hide');

                setTimeout(function () {
                    preguntarCargaInicialDetalle();
                }, 250);
            },
            true,
            ['Sí, cargar todo', 'No, carga manual'],
            'info!',
            null
        );
    }

    function definirModalidadCargaInicial(cargarTodoDetalle) {

        logInfo('ETAPA 6', {
            accion: 'Guardando modalidad de carga en sesión.',
            cargarTodoDetalle: cargarTodoDetalle,
            modalidad: cargarTodoDetalle ? 'TODOS' : 'MANUAL'
        });

        if (guardandoModalidadCarga) {
            return;
        }

        if (
            !validarUrlDisponible(
                'ncDevolucionDefinirModalidadCargaUrl',
                window.ncDevolucionDefinirModalidadCargaUrl
            )
        ) {
            return;
        }

        guardandoModalidadCarga = true;

        $.ajax({
            url: window.ncDevolucionDefinirModalidadCargaUrl,
            type: 'POST',
            contentType: 'application/json; charset=utf-8',
            dataType: 'json',
            timeout: 15000,
            data: JSON.stringify({
                cargarTodoDetalle: cargarTodoDetalle
            })
        })
            .done(function (response) {

                logInfo('ETAPA 6', {
                    accion: 'Respuesta al guardar modalidad recibida.',
                    response: resumirRespuestaAjax(response)
                });

                if (!response || response.ok !== true) {
                    mostrarErrorModalidadCarga(
                        response?.mensaje ||
                        'No fue posible guardar la modalidad de carga.'
                    );

                    return;
                }

                const modalidadTodos = response.cargarTodoDetalle === true;

                cerrarMensajeYNotificarModalidad(
                    modalidadTodos,
                    response
                );
            })
            .fail(function (xhr, status, error) {

                logError(
                    'ETAPA 6',
                    {
                        accion: 'Error al guardar modalidad de carga.',
                        ...resumirErrorAjax(xhr, status, error)
                    }
                );

                console.error(
                    '[NC Devolución] Error al guardar modalidad de carga:',
                    {
                        status: xhr?.status,
                        textStatus: status,
                        error: error,
                        response: xhr?.responseJSON
                    }
                );

                if (
                    typeof esSesionExpirada === 'function' &&
                    esSesionExpirada(xhr?.status)
                ) {
                    return;
                }

                mostrarErrorModalidadCarga(
                    xhr?.responseJSON?.mensaje ||
                    'Ocurrió un error al guardar la modalidad de carga.'
                );
            })
            .always(function () {
                guardandoModalidadCarga = false;
            });
    }

    function cerrarMensajeYNotificarModalidad(
        cargarTodoDetalle,
        response
    ) {
        const $modalMensaje = $('#msjModal');

        const notificar = function () {
            notificarModalidadCargaDefinida(
                cargarTodoDetalle,
                response
            );
        };

        if ($modalMensaje.length === 0) {
            logAdvertencia('ETAPA 6', {
                accion: 'No se encontró msjModal. Se notificará la modalidad directamente.'
            });

            notificar();
            return;
        }

        if (!$modalMensaje.hasClass('show')) {
            logInfo('ETAPA 6', {
                accion: 'msjModal ya estaba cerrado. Se notificará la modalidad.'
            });

            notificar();
            return;
        }

        logInfo('ETAPA 6', {
            accion: 'Cerrando msjModal antes de abrir el modal de productos.'
        });

        $modalMensaje
            .one('hidden.bs.modal.ncDevolucion', function () {
                logInfo('ETAPA 6', {
                    accion: 'msjModal cerrado. Notificando modalidad al módulo de productos.'
                });

                notificar();
            })
            .modal('hide');
    }

    function notificarModalidadCargaDefinida(
        cargarTodoDetalle,
        response
    ) {
        const modalidad = cargarTodoDetalle
            ? 'TODOS'
            : 'MANUAL';

        const detalle = {
            cargarTodoDetalle: cargarTodoDetalle,
            modalidad: response?.modalidad || modalidad,
            mensaje: response?.mensaje || '',
            fecha: new Date().toISOString()
        };

        logInfo('ETAPA 6', {
            accion: 'Notificando modalidad de carga al módulo de productos.',
            detalle: detalle
        });

        document.dispatchEvent(
            new CustomEvent(
                'ncdev:modalidad-carga-definida',
                {
                    detail: detalle
                }
            )
        );
    }

    function mostrarErrorModalidadCarga(mensaje) {
        AbrirMensaje(
            'Error al definir la carga',
            `<div class="text-center">
            <i class="bx bx-error-circle text-danger"
               style="font-size: 3rem;"></i>

            <p class="mt-3 mb-0">
                ${escaparHtml(mensaje)}
            </p>
        </div>`,
            function () {
                $('#msjModal').modal('hide');

                setTimeout(function () {
                    preguntarCargaInicialDetalle();
                }, 250);
            },
            false,
            ['Aceptar'],
            'error!',
            null
        );
    }

    function esRespuestaNegativa(respuesta) {
        const valor = String(respuesta || '')
            .trim()
            .toUpperCase();

        return valor === 'NO' ||
            valor === 'CANCELAR' ||
            valor === 'NEGAR';
    }

    function procesarComprobanteSeleccionado(comprobante) {
        logInfo('ETAPA 4', {
            accion: 'Comprobante seleccionado correctamente.',
            comprobante: resumirComprobante(comprobante)
        });

        notificarComprobanteOrigenValidado(comprobante);

        const modalIdentificacionVisible =
            modalIdentificacion &&
            document.querySelector(
                SELECTORES.modalIdentificacion
            )?.classList.contains('show');

        const modalRepetidosVisible =
            modalRepetidos &&
            document.querySelector(
                SELECTORES.modalRepetidos
            )?.classList.contains('show');

        if (modalIdentificacionVisible) {
            logInfo(
                'ETAPA 4',
                'Cerrando modal de identificación antes de definir modalidad.'
            );

            modalIdentificacion.hide();
        }

        if (modalRepetidosVisible) {
            logInfo(
                'ETAPA 4',
                'Cerrando modal de repetidos antes de definir modalidad.'
            );

            modalRepetidos.hide();
        }

        setTimeout(function () {
            preguntarCargaInicialDetalle();
        }, 300);
    }

    function renderizarCandidatosRepetidos(candidatos) {

        logInfo('UI → Renderizando candidatos repetidos', {
            cantidad: Array.isArray(candidatos) ? candidatos.length : 0,
            candidatos: Array.isArray(candidatos)
                ? candidatos.map(function (item) {
                    return {
                        indice: item.indice,
                        cm_compte: item.cm_compte,
                        cm_repetido: item.cm_repetido,
                        cm_fecha: item.cm_fecha,
                        cm_total: item.cm_total,
                        bloqueado: item.bloqueado,
                        motivo_bloqueo: item.motivo_bloqueo
                    };
                })
                : []
        });

        const $tbody = $(SELECTORES.tablaRepetidos);

        const filas = candidatos.map(function (item) {
            const bloqueado = item.bloqueado === true;
            const estadoHtml = bloqueado
                ? `<span class="badge bg-danger"
                         title="${escaparHtml(item.motivo_bloqueo || '')}">
                       Bloqueado
                   </span>`
                : `<span class="badge bg-success">Disponible</span>`;

            const accionHtml = bloqueado
                ? `<button type="button"
                           class="btn btn-sm btn-outline-secondary"
                           disabled
                           title="${escaparHtml(item.motivo_bloqueo || '')}">
                       <i class="bx bx-lock-alt me-1"></i>
                       No disponible
                   </button>`
                : `<button type="button"
                        class="btn btn-sm btn-golden btn-seleccionar-repetido"
                        data-indice="${Number(item.indice)}">
                       <i class="bx bx-check-circle me-1"></i>
                       Seleccionar
                   </button>`;

            return `
                <tr>
                    <td class="text-center">
                        <strong>${escaparHtml(item.tco_id || '')}</strong>
                        <br />
                        <small class="text-muted">
                            ${escaparHtml(item.cm_compte || '')}
                        </small>
                    </td>

                    <td class="text-center">
                        ${escaparHtml(item.cm_repetido ?? 0)}
                    </td>

                    <td class="text-center">
                        ${escaparHtml(item.cm_fecha || '')}
                    </td>

                    <td>
                        <div class="fw-semibold">
                            ${escaparHtml(item.cm_nombre || 'Sin denominación')}
                        </div>
                        <small class="text-muted">
                            ${escaparHtml(item.cm_cuit || '')}
                        </small>
                    </td>

                    <td class="text-end">
                        $ ${formatearImporte(item.cm_total)}
                    </td>

                    <td class="text-center">
                        ${estadoHtml}
                    </td>

                    <td class="text-center">
                        ${accionHtml}
                    </td>
                </tr>
            `;
        }).join('');

        $tbody.html(filas);
    }

    function volverAInicioNotaCredito() {
        const urlInicio = String(
            window.ncDevolucionIndexUrl || window.location.pathname
        ).trim();

        if (!urlInicio) {
            mostrarError(
                'No se encontró la ruta para reiniciar la Nota de Crédito.'
            );

            return;
        }

        logInfo('CANCELACIÓN', {
            accion: 'Operación cancelada. Volviendo al inicio de Nota de Crédito.',
            destino: 'INDEX_NOTA_CREDITO',
            url: urlInicio
        });

        /*
         * El contexto ya fue eliminado en servidor.
         * replace evita volver con Atrás a una operación cancelada.
         */
        window.location.replace(urlInicio);
    }

    function volverAMenuCaja() {
        const urlMenu = String(
            window.ncDevolucionMenuCajaUrl || ''
        ).trim();

        if (!urlMenu) {
            logError('CANCELACIÓN', {
                accion: 'No se encontró ncDevolucionMenuCajaUrl.'
            });

            mostrarError(
                'No se encontró la ruta para volver al menú principal de Caja.'
            );

            return;
        }

        logInfo('CANCELACIÓN', {
            accion: 'Operación cancelada. Volviendo al menú principal de Caja.',
            destino: 'MENU_CAJA',
            url: urlMenu
        });

        window.location.replace(urlMenu);
    }

    function navegarDespuesDeCancelar(destino) {
        if (destino === DESTINOS_CANCELACION.MENU_CAJA) {
            volverAMenuCaja();
            return;
        }

        if (destino === DESTINOS_CANCELACION.REINICIAR_NOTA_CREDITO) {
            volverAInicioNotaCredito();
            return;
        }

        logError('CANCELACIÓN', {
            accion: 'Destino de cancelación no reconocido.',
            destino: destino
        });

        mostrarError(
            'No se pudo determinar el destino posterior a la cancelación.'
        );
    }

    function confirmarCancelacion(opciones) {
        const configuracion = opciones || {
            origen: 'DESCONOCIDO',
            destino: DESTINOS_CANCELACION.MENU_CAJA,
            selectorBoton: SELECTORES_CANCELACION.IDENTIFICACION
        };

        logInfo('CANCELACIÓN', {
            accion: 'Solicitando confirmación para cancelar la operación.',
            origen: configuracion.origen,
            destino: configuracion.destino
        });

        AbrirMensaje(
            'Cancelar Nota de Crédito',
            `<div class="text-center">
            <i class="bx bx-error-circle text-warning"
               style="font-size: 3rem;"></i>

            <p class="mt-3 mb-1">
                ¿Desea cancelar la operación actual?
            </p>

            <small class="text-muted">
                Se eliminarán los datos temporales de esta devolución.
            </small>
        </div>`,
            function (respuesta) {
                logInfo('CANCELACIÓN', {
                    accion: 'Respuesta recibida para cancelar operación.',
                    origen: configuracion.origen,
                    destino: configuracion.destino,
                    respuestaOriginal: respuesta,
                    respuestaNormalizada: String(respuesta ?? '')
                        .trim()
                        .toUpperCase()
                });

                $('#msjModal').modal('hide');

                if (!esRespuestaPositiva(respuesta)) {
                    logInfo('CANCELACIÓN', {
                        accion: 'El operador decidió continuar la operación.',
                        origen: configuracion.origen
                    });

                    return;
                }

                cancelarOperacion(configuracion);
            },
            true,
            ['Sí, cancelar', 'No, continuar'],
            'warn!',
            null
        );
    }

    function cancelarOperacion(configuracion) {
        const selectorBoton = configuracion.selectorBoton ||
            SELECTORES_CANCELACION.IDENTIFICACION;

        logInfo('CANCELACIÓN', {
            accion: 'Solicitando limpieza de contexto de NC por Devolución.',
            origen: configuracion.origen,
            destino: configuracion.destino,
            url: window.ncDevolucionCancelarOperacionUrl
        });

        if (
            !validarUrlDisponible(
                'ncDevolucionCancelarOperacionUrl',
                window.ncDevolucionCancelarOperacionUrl
            )
        ) {
            return;
        }

        $(selectorBoton).prop('disabled', true);

        $.ajax({
            url: window.ncDevolucionCancelarOperacionUrl,
            type: 'POST',
            contentType: 'application/json; charset=utf-8',
            dataType: 'json',
            timeout: 10000,
            data: '{}'
        })
            .done(function (response) {
                logInfo('CANCELACIÓN', {
                    accion: 'Respuesta de cancelación recibida.',
                    origen: configuracion.origen,
                    destino: configuracion.destino,
                    response: resumirRespuestaAjax(response)
                });

                if (!response || response.ok !== true) {
                    mostrarError(
                        response?.mensaje ||
                        'No fue posible cancelar la operación.'
                    );

                    return;
                }

                navegarDespuesDeCancelar(configuracion.destino);
            })
            .fail(function (xhr, status, error) {
                logError('CANCELACIÓN', {
                    accion: 'Error al cancelar la operación.',
                    origen: configuracion.origen,
                    destino: configuracion.destino,
                    ...resumirErrorAjax(xhr, status, error)
                });

                console.error(
                    '[NC Devolución] Error al cancelar operación:',
                    {
                        status: xhr?.status,
                        textStatus: status,
                        error: error
                    }
                );

                if (
                    typeof esSesionExpirada === 'function' &&
                    esSesionExpirada(xhr?.status)
                ) {
                    return;
                }

                mostrarError(
                    xhr?.responseJSON?.mensaje ||
                    'Ocurrió un error al cancelar la operación.'
                );
            })
            .always(function () {
                $(selectorBoton).prop('disabled', false);
            });
    }

    function validarDatosIngresados(tcoId, puntoVenta, numero) {
        if (!tcoId) {
            return 'Debe seleccionar un tipo de comprobante.';
        }

        if (!puntoVenta) {
            return 'Debe ingresar el punto de venta.';
        }

        if (!numero) {
            return 'Debe ingresar el número de comprobante.';
        }

        if (!/^\d{1,4}$/.test(puntoVenta)) {
            return 'El punto de venta debe contener entre 1 y 4 dígitos numéricos.';
        }

        if (!/^\d{1,8}$/.test(numero)) {
            return 'El número de comprobante debe contener entre 1 y 8 dígitos numéricos.';
        }

        return '';
    }

    function enfocarPrimerCampoInvalido(tcoId, puntoVenta, numero) {
        if (!tcoId) {
            $(SELECTORES.tipoComprobante).trigger('focus');
            return;
        }

        if (!puntoVenta) {
            $(SELECTORES.puntoVenta).trigger('focus');
            return;
        }

        $(SELECTORES.numeroComprobante).trigger('focus');
    }

    function normalizarSoloDigitos($input, maximo) {
        const valor = String($input.val() || '')
            .replace(/\D/g, '')
            .substring(0, maximo);

        $input.val(valor);
    }

    function bloquearFormulario(bloquear) {
        $(SELECTORES.tipoComprobante).prop('disabled', bloquear);
        $(SELECTORES.puntoVenta).prop('disabled', bloquear);
        $(SELECTORES.numeroComprobante).prop('disabled', bloquear);
        $(SELECTORES.btnValidar).prop('disabled', bloquear);
    }

    function cambiarEstadoValidacion(estaValidando) {
        $(SELECTORES.tipoComprobante).prop('disabled', estaValidando);
        $(SELECTORES.puntoVenta).prop('disabled', estaValidando);
        $(SELECTORES.numeroComprobante).prop('disabled', estaValidando);
        $(SELECTORES.btnValidar).prop('disabled', estaValidando);

        $(SELECTORES.spinnerValidar).toggleClass(
            'd-none',
            !estaValidando
        );

        $(SELECTORES.iconoValidar).toggleClass(
            'd-none',
            estaValidando
        );
    }

    function mostrarAlertaInline(mensaje, tipo) {

        logAdvertencia('UI → Alerta mostrada en formulario', {
            tipo: tipo,
            mensaje: mensaje
        });

        const clases = {
            success: 'alert-success',
            warning: 'alert-warning',
            danger: 'alert-danger',
            info: 'alert-info'
        };

        const iconos = {
            success: 'bx-check-circle',
            warning: 'bx-error-circle',
            danger: 'bx-x-circle',
            info: 'bx-info-circle'
        };

        const clase = clases[tipo] || clases.info;
        const icono = iconos[tipo] || iconos.info;

        $(SELECTORES.alertaInline)
            .removeClass(
                'd-none alert-success alert-warning alert-danger alert-info'
            )
            .addClass(clase)
            .html(
                `<i class="bx ${icono} me-1"></i>${escaparHtml(mensaje)}`
            );
    }

    function limpiarAlertaInline() {
        $(SELECTORES.alertaInline)
            .addClass('d-none')
            .removeClass(
                'alert-success alert-warning alert-danger alert-info'
            )
            .empty();
    }

    function mostrarError(mensaje) {

        logError('UI → Mensaje de error modal', {
            mensaje: mensaje
        });

        AbrirMensaje(
            'Error',
            `<div class="text-center">
                <i class="bx bx-error-circle text-danger"
                   style="font-size: 3rem;"></i>
                <p class="mt-3 mb-0">
                    ${escaparHtml(mensaje)}
                </p>
            </div>`,
            function () {
                $('#msjModal').modal('hide');
            },
            false,
            ['Aceptar'],
            'error!',
            null
        );
    }

    function validarUrlDisponible(nombre, valor) {
        if (typeof valor !== 'string' || !valor.trim()) {
            console.error(
                `[NC Devolución] URL no disponible: ${nombre}`
            );

            mostrarError(
                'No se encontró la configuración necesaria para ejecutar esta operación.'
            );

            return false;
        }

        return true;
    }

    // function esRespuestaNegativa(respuesta) {
    //     const valor = String(respuesta ?? '')
    //         .trim()
    //         .toUpperCase()
    //         .normalize('NFD')
    //         .replace(/[\u0300-\u036f]/g, '');

    //     return valor === 'NO' ||
    //         valor === 'CANCELAR' ||
    //         valor === 'NEGAR' ||
    //         valor.startsWith('NO,') ||
    //         valor.includes('CARGA MANUAL');
    // }

    function normalizarRespuestaMensaje(respuesta) {
        return String(respuesta ?? '')
            .trim()
            .toUpperCase()
            .normalize('NFD')
            .replace(/[\u0300-\u036f]/g, '');
    }

    function esRespuestaPositiva(respuesta) {
        const valor = normalizarRespuestaMensaje(respuesta);

        return valor === 'SI' ||
            valor === 'YES' ||
            valor === 'ACEPTAR' ||
            valor.startsWith('SI,') ||
            valor.includes('CARGAR TODO');
    }

    function esRespuestaNegativa(respuesta) {
        const valor = normalizarRespuestaMensaje(respuesta);

        return valor === 'NO' ||
            valor === 'CANCELAR' ||
            valor === 'NEGAR' ||
            valor.startsWith('NO,') ||
            valor.includes('CARGA MANUAL');
    }

    function formatearImporte(valor) {
        const numero = Number(valor);

        if (!Number.isFinite(numero)) {
            return '0.00';
        }

        return numero.toLocaleString('en-US', {
            minimumFractionDigits: 2,
            maximumFractionDigits: 2
        });
    }

    function escaparHtml(valor) {
        if (
            typeof window.escapeHtml === 'function'
        ) {
            return window.escapeHtml(valor);
        }

        return String(valor ?? '')
            .replace(/&/g, '&amp;')
            .replace(/</g, '&lt;')
            .replace(/>/g, '&gt;')
            .replace(/"/g, '&quot;')
            .replace(/'/g, '&#039;');
    }

    namespace.validarComprobanteOrigen = validarComprobanteOrigen;

})(window.NCDevolucion);