// ═══════════════════════════════════════════════════════════════════
// MÓDULO: NOTA DE CRÉDITO POR DEVOLUCIÓN
// PASO 2: PRODUCTOS DE DEVOLUCIÓN - ORQUESTACIÓN MODAL
// ═══════════════════════════════════════════════════════════════════

window.NCDevolucion = window.NCDevolucion || {};

(function (namespace) {
    'use strict';

    const EVENTO_COMPROBANTE_VALIDADO =
        'ncdev:comprobante-origen-validado';

    const EVENTO_MODALIDAD_CARGA =
        'ncdev:modalidad-carga-definida';

    const REGEX_CANTIDAD_COMODIN =
        /^(\d+(?:\.\d{1,3})?)\+(.+)$/;

    let modalProductos = null;
    let modalCalculo = null;
    let comprobanteOrigen = null;
    let modalidadActual = null;

    let cargaDetalleEnCurso = false;
    let cargaDetalleEjecutada = false;
    let cargaManualEnCurso = false;
    let calculoEnCurso = false;
    let finalizacionEnCurso = false;
    let productosActuales = [];
    let calculoActual = null;

    const SELECTORES = {
        modal: '#modalProductoDevolucion',
        modalCalculo: '#modalNcDevolucionCalculo',

        modalidadBadge: '#ncDevolucionModalidadBadge',
        cantidadProductos: '#ncDevolucionCantidadProductos',

        resumenOrigen: '#ncDevolucionProductoResumenOrigen',
        resumenNc: '#ncDevolucionProductoResumenNc',
        tipoComprobanteBadge: '#ncDevolucionTipoComprobanteBadge',
        clienteNombreProd: '#txtNcClienteNombreProd',
        clienteIdProd: '#txtNcClienteIdProd',
        clienteDomicilioProd: '#txtNcClienteDomicilioProd',
        condicionAfipProd: '#txtNcCondicionAfipProd',
        clienteCuitProd: '#txtNcClienteCuitProd',
        clienteEmailProd: '#txtNcClienteEmailProd',
        clienteMovilProd: '#txtNcClienteMovilProd',

        estado: '#ncDevolucionProductosEstado',
        cargando: '#ncDevolucionProductosCargando',
        error: '#ncDevolucionProductosError',
        manual: '#ncDevolucionProductosManual',

        inputManual: '#txtNcDevolucionCodigoProducto',
        tecladoAnclaProductos: '#ncDevolucionTecladoAnclaProductos',
        btnAgregarManual: '#btnNcDevolucionAgregarProducto',
        spinnerAgregarManual: '#spnNcDevolucionAgregarProducto',
        iconoAgregarManual: '#icoNcDevolucionAgregarProducto',
        estadoEntradaManual: '#ncDevolucionEstadoEntradaManual',

        resultado: '#ncDevolucionProductosResultado',

        tablaProductos: '#ncDevolucionTablaProductos',

        advertencias: '#ncDevolucionAdvertenciasProductos',
        listaAdvertencias: '#ncDevolucionListaAdvertenciasProductos',

        rechazos: '#ncDevolucionRechazosProductos',
        listaRechazos: '#ncDevolucionListaRechazosProductos',

        destinoOperacion: '#ncDevolucionDestinoOperacion',
        conceptosCalculo: '#tbodyNcDevolucionConceptosCalculo',
        totalFinalCalculo: '#tdNcDevolucionTotalFinal',
        calculoResumenOrigen: '#ncDevolucionCalculoResumenOrigen',
        calculoResumenNc: '#ncDevolucionCalculoResumenNc',
        calcClienteNombre: '#txtNcCalcClienteNombre',
        calcClienteId: '#txtNcCalcClienteId',
        calcClienteDomicilio: '#txtNcCalcClienteDomicilio',
        calcComprobanteOrigen: '#txtNcCalcComprobanteOrigen',
        calcTipoNc: '#txtNcCalcTipoNc',
        calcCondicionAfip: '#txtNcCalcCondicionAfip',
        calcDocumento: '#txtNcCalcDocumento',
        calcEmail: '#txtNcCalcEmail',
        calcMovil: '#txtNcCalcMovil',

        btnCancelar: '#btnCancelarNcDevolucionProductos',
        btnSeguir: '#btnNcDevolucionSeguir',
        spinnerSeguir: '#spnNcDevolucionSeguir',
        iconoSeguir: '#icoNcDevolucionSeguir',
        btnVolverCalculo: '#btnVolverNcDevolucionCalculo',
        btnFinalizar: '#btnNcDevolucionFinalizar',
        spinnerFinalizar: '#spnNcDevolucionFinalizar',
        iconoFinalizar: '#icoNcDevolucionFinalizar'
    };

    $(function () {
        logInfo('PRODUCTOS - INICIO', {
            accion: 'Inicializando módulo modal de productos.',
            urlCargaDetalle:
                window.ncDevolucionCargarDetalleCompletoUrl || ''
        });

        inicializarModal();
        registrarEventos();

        logInfo('PRODUCTOS - INICIO', {
            accion: 'Módulo modal de productos listo.'
        });
    });

    function inicializarModal() {
        const elementoModal = document.querySelector(
            SELECTORES.modal
        );

        if (!elementoModal) {
            logError('PRODUCTOS - INICIALIZACIÓN', {
                mensaje: 'No se encontró el modal de productos.',
                selector: SELECTORES.modal
            });

            return;
        }

        modalProductos = bootstrap.Modal.getOrCreateInstance(
            elementoModal
        );

        const elementoModalCalculo = document.querySelector(
            SELECTORES.modalCalculo
        );

        if (elementoModalCalculo) {
            modalCalculo = bootstrap.Modal.getOrCreateInstance(
                elementoModalCalculo
            );
        } else {
            logAdvertencia('PRODUCTOS - INICIALIZACIÓN', {
                mensaje: 'No se encontró el modal de cálculo de NC.',
                selector: SELECTORES.modalCalculo
            });
        }

        $(elementoModal).on(
            'shown.bs.modal',
            function () {
                logInfo('PRODUCTOS - MODAL', {
                    accion: 'Modal de productos visible.',
                    modalidad: modalidadActual?.modalidad || ''
                });

                ejecutarFlujoDespuesDeAbrirModal();
            }
        );

        $(elementoModal).on(
            'hidden.bs.modal',
            function () {
                logInfo('PRODUCTOS - MODAL', {
                    accion: 'Modal de productos cerrado.'
                });

                ocultarTecladoNc();
            }
        );

        $(elementoModalCalculo).on(
            'hidden.bs.modal',
            function () {
                ocultarTecladoNc();
            }
        );
    }

    function registrarEventos() {
        document.addEventListener(
            EVENTO_COMPROBANTE_VALIDADO,
            function (event) {
                const detalle = event?.detail || {};

                comprobanteOrigen = detalle.comprobante || null;

                logInfo('PRODUCTOS - EVENTO', {
                    accion: 'Comprobante origen recibido.',
                    comprobante: resumirComprobante(comprobanteOrigen)
                });
            }
        );

        document.addEventListener(
            EVENTO_MODALIDAD_CARGA,
            function (event) {
                const detalle = event?.detail || {};

                modalidadActual = {
                    cargarTodoDetalle:
                        detalle.cargarTodoDetalle === true,
                    modalidad: detalle.modalidad || (
                        detalle.cargarTodoDetalle === true
                            ? 'TODOS'
                            : 'MANUAL'
                    ),
                    mensaje: detalle.mensaje || '',
                    fecha: detalle.fecha || new Date().toISOString()
                };

                logInfo('PRODUCTOS - EVENTO', {
                    accion: 'Modalidad de carga recibida.',
                    modalidad: modalidadActual
                });

                abrirModalProductos();
            }
        );

        $(document).on(
            'click',
            SELECTORES.btnCancelar,
            function () {
                logInfo('PRODUCTOS - CANCELACIÓN', {
                    accion: 'El operador solicitó cancelar desde modal de productos.'
                });

                document.dispatchEvent(
                    new CustomEvent(
                        'ncdev:solicitar-cancelacion'
                    )
                );
            }
        );

        $(document).on(
            'click',
            SELECTORES.btnAgregarManual,
            function () {
                procesarEntradaManual();
            }
        );

        $(document).on(
            'click',
            SELECTORES.btnSeguir,
            function () {
                iniciarDefinicionDestino();
            }
        );

        $(document).on(
            'click',
            SELECTORES.btnFinalizar,
            function () {
                confirmarFinalizacion();
            }
        );

        $(document).on(
            'click',
            SELECTORES.btnVolverCalculo,
            function () {
                volverACargaProductos();
            }
        );

        $(document).on(
            'focus',
            SELECTORES.inputManual,
            function () {
                posicionarTecladoNc(
                    SELECTORES.inputManual,
                    null,
                    'right'
                );
            }
        );

        $(document).on(
            'keydown',
            SELECTORES.inputManual,
            function (event) {
                if (event.key !== 'Enter') {
                    return;
                }

                event.preventDefault();

                procesarEntradaManual();
            }
        );

        $(document).on(
            'input',
            SELECTORES.inputManual,
            function () {
                limpiarEstadoEntradaManual();
            }
        );
    }

    function abrirModalProductos() {
        if (!modalProductos) {
            mostrarErrorConfiguracion(
                'No se pudo abrir la pantalla de productos de devolución.'
            );

            return;
        }

        if (!comprobanteOrigen) {
            mostrarErrorConfiguracion(
                'No se encontró el comprobante original validado para iniciar la devolución.'
            );

            return;
        }

        cargaDetalleEnCurso = false;
        cargaDetalleEjecutada = false;
        calculoEnCurso = false;
        finalizacionEnCurso = false;
        productosActuales = [];
        calculoActual = null;

        limpiarEstadoVisual();
        renderizarResumenOperacion();

        logInfo('PRODUCTOS - MODAL', {
            accion: 'Abriendo modal de productos.',
            comprobante: resumirComprobante(comprobanteOrigen),
            modalidad: modalidadActual
        });

        modalProductos.show();
    }

    function ejecutarFlujoDespuesDeAbrirModal() {
        if (!modalidadActual) {
            mostrarErrorConfiguracion(
                'No se pudo determinar la modalidad de carga de productos.'
            );

            return;
        }

        if (modalidadActual.cargarTodoDetalle === true) {
            cargarDetalleCompleto();
            return;
        }

        mostrarModalidadManual();
    }

    function cargarDetalleCompleto() {
        if (cargaDetalleEnCurso || cargaDetalleEjecutada) {
            logAdvertencia('PRODUCTOS - CARGA TOTAL', {
                accion: 'Se evitó una carga duplicada.',
                cargaDetalleEnCurso: cargaDetalleEnCurso,
                cargaDetalleEjecutada: cargaDetalleEjecutada
            });

            return;
        }

        const url = String(
            window.ncDevolucionCargarDetalleCompletoUrl || ''
        ).trim();

        if (!url) {
            mostrarErrorConfiguracion(
                'No se encontró la URL para cargar el detalle del comprobante.'
            );

            return;
        }

        cargaDetalleEnCurso = true;
        cargaDetalleEjecutada = true;

        mostrarCargando();

        logInfo('PRODUCTOS - CARGA TOTAL', {
            accion: 'Invocando CargarDetalleCompleto.',
            url: url
        });

        $.ajax({
            url: url,
            type: 'POST',
            dataType: 'json',
            timeout: 30000
        })
            .done(function (response) {
                logInfo('PRODUCTOS - CARGA TOTAL', {
                    accion: 'Respuesta recibida desde CargarDetalleCompleto.',
                    respuesta: resumirRespuesta(response)
                });

                if (!response || response.ok !== true) {
                    mostrarErrorCarga(
                        response?.mensaje ||
                        'No fue posible cargar el detalle del comprobante original.'
                    );

                    return;
                }

                renderizarProductos(
                    Array.isArray(response.productos)
                        ? response.productos
                        : [],
                    Array.isArray(response.advertencias)
                        ? response.advertencias
                        : [],
                    Array.isArray(response.rechazos)
                        ? response.rechazos
                        : [],
                    'TODOS'
                );
            })
            .fail(function (xhr, status, error) {
                logError('PRODUCTOS - CARGA TOTAL', {
                    accion: 'Error HTTP o de red al cargar detalle completo.',
                    httpStatus: xhr?.status || 0,
                    textStatus: status || '',
                    error: String(error || ''),
                    mensajeApi: xhr?.responseJSON?.mensaje || '',
                    respuestaTexto: String(
                        xhr?.responseText || ''
                    ).substring(0, 500)
                });

                mostrarErrorCarga(
                    xhr?.responseJSON?.mensaje ||
                    'Ocurrió un error al cargar el detalle del comprobante original.'
                );
            })
            .always(function () {
                cargaDetalleEnCurso = false;
                $(SELECTORES.cargando).addClass('d-none');

                logDebug('PRODUCTOS - CARGA TOTAL', {
                    accion: 'Proceso de carga total finalizado.',
                    cargaDetalleEjecutada: cargaDetalleEjecutada
                });
            });
    }

    function mostrarModalidadManual() {
        $(SELECTORES.manual).removeClass('d-none');

        $(SELECTORES.estado)
            .removeClass(
                'd-none alert-success alert-warning alert-danger'
            )
            .addClass('alert-info')
            .html(
                '<i class="bx bx-edit-alt me-1"></i>' +
                '<strong>Modalidad manual seleccionada.</strong> ' +
                'Ingrese un código de producto para incorporarlo a la devolución.'
            );

        limpiarEstadoEntradaManual();

        logInfo('PRODUCTOS - MANUAL', {
            accion: 'Modal preparado para carga manual.',
            llamadoApiAutomatico: false
        });

        enfocarEntradaManual();
    }

    function procesarEntradaManual() {
        if (calculoActual) {
            mostrarEstadoEntradaManual(
                'La Nota de Crédito ya fue calculada. Cancele la operación para modificar productos.',
                'warning'
            );

            return;
        }

        if (cargaManualEnCurso) {
            logAdvertencia('PRODUCTOS - MANUAL', {
                accion: 'Se ignoró la entrada porque existe una carga manual en curso.'
            });

            return;
        }

        const entrada = String(
            $(SELECTORES.inputManual).val() || ''
        ).trim();

        logInfo('PRODUCTOS - MANUAL', {
            accion: 'Procesando entrada manual.',
            entrada: entrada
        });

        if (!entrada) {
            mostrarEstadoEntradaManual(
                'Debe ingresar un código de producto.',
                'warning'
            );

            enfocarEntradaManual();
            return;
        }

        const resultadoParseo = parsearEntradaManual(entrada);

        if (!resultadoParseo.ok) {
            mostrarEstadoEntradaManual(
                resultadoParseo.mensaje,
                'danger'
            );

            enfocarEntradaManual();
            return;
        }

        enviarProductoManual(
            resultadoParseo.valor,
            resultadoParseo.cantidad,
            entrada
        );
    }

    function parsearEntradaManual(entrada) {
        const texto = String(entrada || '').trim();

        if (!texto) {
            return {
                ok: false,
                mensaje: 'Debe ingresar un código de producto.'
            };
        }

        const matchCantidad = texto.match(
            REGEX_CANTIDAD_COMODIN
        );

        if (matchCantidad) {
            const cantidadTexto = matchCantidad[1];
            const valor = String(matchCantidad[2] || '').trim();
            const cantidad = Number(cantidadTexto);

            if (!Number.isFinite(cantidad) || cantidad <= 0) {
                return {
                    ok: false,
                    mensaje: 'La cantidad ingresada no es válida.'
                };
            }

            if (!valor) {
                return {
                    ok: false,
                    mensaje: 'Debe indicar un código después del signo +.'
                };
            }

            if (valor.length > 30) {
                return {
                    ok: false,
                    mensaje: 'El código ingresado supera la longitud máxima permitida.'
                };
            }

            if (valor.toUpperCase() === 'T') {
                return {
                    ok: false,
                    mensaje:
                        'El valor ingresado está reservado para la carga total del comprobante.'
                };
            }

            return {
                ok: true,
                valor: valor,
                cantidad: cantidad
            };
        }

        if (texto.includes('+')) {
            return {
                ok: false,
                mensaje:
                    'El formato cantidad+código es inválido. Ejemplo válido: 2+7790070036599.'
            };
        }

        if (texto.length > 30) {
            return {
                ok: false,
                mensaje: 'El código ingresado supera la longitud máxima permitida.'
            };
        }

        if (texto.toUpperCase() === 'T') {
            return {
                ok: false,
                mensaje:
                    'El valor ingresado está reservado para la carga total del comprobante.'
            };
        }

        return {
            ok: true,
            valor: texto,
            cantidad: 1
        };
    }

    function enviarProductoManual(valor, cantidad, entradaOriginal) {
        const url = String(
            window.ncDevolucionAgregarProductoManualUrl || ''
        ).trim();

        if (!url) {
            mostrarEstadoEntradaManual(
                'No se encontró la URL para agregar productos manualmente.',
                'danger'
            );

            logError('PRODUCTOS - MANUAL', {
                accion: 'URL de carga manual no disponible.'
            });

            return;
        }

        cargaManualEnCurso = true;

        bloquearEntradaManual(true);

        mostrarEstadoEntradaManual(
            `Consultando producto: ${entradaOriginal}`,
            'info'
        );

        logInfo('PRODUCTOS - MANUAL', {
            accion: 'Invocando AgregarProductoManual.',
            url: url,
            valor: valor,
            cantidad: cantidad
        });

        $.ajax({
            url: url,
            type: 'POST',
            contentType: 'application/json; charset=utf-8',
            dataType: 'json',
            timeout: 30000,
            data: JSON.stringify({
                valor: valor,
                cantidad: cantidad
            })
        })
            .done(function (response) {
                logInfo('PRODUCTOS - MANUAL', {
                    accion: 'Respuesta recibida desde AgregarProductoManual.',
                    respuesta: resumirRespuestaManual(response)
                });

                /*
                 * Mientras se resuelve el comportamiento de respuesta {},
                 * cualquier payload que no contenga explícitamente ok=true/false
                 * se trata como respuesta incompleta y dispara rehidratación.
                 */
                if (!esRespuestaManualCompleta(response)) {
                    logAdvertencia('PRODUCTOS - MANUAL', {
                        accion:
                            'La respuesta manual no contiene la estructura esperada. Se intentará rehidratar la grilla desde sesión.',
                        respuesta: response
                    });

                    mostrarEstadoEntradaManual(
                        'La operación fue procesada. Actualizando la grilla desde la sesión...',
                        'info'
                    );

                    hidratarProductosDesdeSesion(
                        function (exito) {
                            if (exito) {
                                $(SELECTORES.inputManual).val('');

                                mostrarEstadoEntradaManual(
                                    'Grilla actualizada desde la sesión.',
                                    'success'
                                );
                            }

                            enfocarEntradaManual();
                        }
                    );

                    return;
                }

                if (response.ok !== true) {
                    mostrarEstadoEntradaManual(
                        response.mensaje ||
                        'No fue posible agregar el producto.',
                        'danger'
                    );

                    enfocarEntradaManual();
                    return;
                }

                const productos = Array.isArray(response.productos)
                    ? response.productos
                    : [];

                const advertencias = Array.isArray(response.advertencias)
                    ? response.advertencias
                    : [];

                const rechazos = Array.isArray(response.rechazos)
                    ? response.rechazos
                    : [];

                renderizarProductos(
                    productos,
                    advertencias,
                    rechazos,
                    'MANUAL'
                );

                $(SELECTORES.inputManual).val('');

                if (rechazos.length > 0) {
                    mostrarEstadoEntradaManual(
                        obtenerMensajeOperacion(
                            rechazos,
                            'El producto no pudo incorporarse a la devolución.'
                        ),
                        'danger'
                    );
                } else if (advertencias.length > 0) {
                    mostrarEstadoEntradaManual(
                        obtenerMensajeOperacion(
                            advertencias,
                            'El producto fue agregado con advertencia.'
                        ),
                        'warning'
                    );
                } else {
                    mostrarEstadoEntradaManual(
                        response.mensaje ||
                        'Producto agregado correctamente.',
                        'success'
                    );
                }

                enfocarEntradaManual();
            })
            .fail(function (xhr, status, error) {
                logError('PRODUCTOS - MANUAL', {
                    accion:
                        'Error HTTP o de red al agregar producto manual.',
                    httpStatus: xhr?.status || 0,
                    textStatus: status || '',
                    error: String(error || ''),
                    mensajeApi: xhr?.responseJSON?.mensaje || '',
                    respuestaTexto: String(
                        xhr?.responseText || ''
                    ).substring(0, 500)
                });

                mostrarEstadoEntradaManual(
                    xhr?.responseJSON?.mensaje ||
                    'Ocurrió un error al agregar el producto.',
                    'danger'
                );

                enfocarEntradaManual();
            })
            .always(function () {
                cargaManualEnCurso = false;

                bloquearEntradaManual(false);

                logDebug('PRODUCTOS - MANUAL', {
                    accion: 'Proceso de carga manual finalizado.'
                });
            });
    }

    function esRespuestaManualCompleta(response) {
        return response &&
            typeof response === 'object' &&
            Object.keys(response).length > 0 &&
            typeof response.ok === 'boolean';
    }

    function hidratarProductosDesdeSesion(callback) {
        const url = String(
            window.ncDevolucionObtenerProductosUrl || ''
        ).trim();

        if (!url) {
            mostrarEstadoEntradaManual(
                'No se encontró la URL para actualizar la grilla de productos.',
                'danger'
            );

            logError('PRODUCTOS - HIDRATACIÓN', {
                accion: 'URL ObtenerProductosDevolucion no disponible.'
            });

            callback?.(false);
            return;
        }

        logInfo('PRODUCTOS - HIDRATACIÓN', {
            accion: 'Solicitando productos actuales desde sesión.',
            url: url
        });

        $.ajax({
            url: url,
            type: 'GET',
            dataType: 'json',
            timeout: 20000
        })
            .done(function (response) {
                logInfo('PRODUCTOS - HIDRATACIÓN', {
                    accion: 'Respuesta recibida desde ObtenerProductosDevolucion.',
                    respuesta: resumirRespuestaManual(response)
                });

                if (!esRespuestaManualCompleta(response) ||
                    response.ok !== true) {
                    mostrarEstadoEntradaManual(
                        response?.mensaje ||
                        'No se pudo obtener la grilla actual de productos.',
                        'danger'
                    );

                    callback?.(false);
                    return;
                }

                const productos = Array.isArray(response.productos)
                    ? response.productos
                    : [];

                renderizarProductos(
                    productos,
                    [],
                    [],
                    'MANUAL'
                );

                callback?.(true);
            })
            .fail(function (xhr, status, error) {
                logError('PRODUCTOS - HIDRATACIÓN', {
                    accion:
                        'Error HTTP o de red al obtener productos desde sesión.',
                    httpStatus: xhr?.status || 0,
                    textStatus: status || '',
                    error: String(error || ''),
                    mensajeApi: xhr?.responseJSON?.mensaje || '',
                    respuestaTexto: String(
                        xhr?.responseText || ''
                    ).substring(0, 500)
                });

                mostrarEstadoEntradaManual(
                    'No se pudo actualizar la grilla de productos desde la sesión.',
                    'danger'
                );

                callback?.(false);
            });
    }

    function bloquearEntradaManual(bloquear) {
        const bloquearPorCalculo = calculoActual !== null;

        $(SELECTORES.inputManual).prop(
            'disabled',
            bloquear || bloquearPorCalculo
        );

        $(SELECTORES.btnAgregarManual).prop(
            'disabled',
            bloquear || bloquearPorCalculo
        );

        $(SELECTORES.spinnerAgregarManual).toggleClass(
            'd-none',
            !(bloquear && cargaManualEnCurso)
        );

        $(SELECTORES.iconoAgregarManual).toggleClass(
            'd-none',
            bloquear && cargaManualEnCurso
        );
    }

    function mostrarEstadoEntradaManual(mensaje, tipo) {
        const clases = {
            info: 'text-info',
            success: 'text-success',
            warning: 'text-warning',
            danger: 'text-danger'
        };

        const iconos = {
            info: 'bx-info-circle',
            success: 'bx-check-circle',
            warning: 'bx-error-circle',
            danger: 'bx-x-circle'
        };

        const clase = clases[tipo] || clases.info;
        const icono = iconos[tipo] || iconos.info;

        $(SELECTORES.estadoEntradaManual)
            .removeClass(
                'text-muted text-info text-success text-warning text-danger'
            )
            .addClass(clase)
            .html(
                `<i class="bx ${icono} me-1"></i>` +
                escaparHtml(mensaje)
            );
    }

    function limpiarEstadoEntradaManual() {
        $(SELECTORES.estadoEntradaManual)
            .removeClass(
                'text-info text-success text-warning text-danger'
            )
            .addClass('text-muted')
            .html(
                '<i class="bx bx-info-circle me-1"></i>' +
                'Presione <kbd>Enter</kbd> o <strong>Agregar</strong> para incorporar un producto.'
            );
    }

    function enfocarEntradaManual() {
        setTimeout(function () {
            const modalVisible = document.querySelector(
                SELECTORES.modal
            )?.classList.contains('show');

            if (!modalVisible) {
                return;
            }

            if ($(SELECTORES.inputManual).prop('disabled')) {
                return;
            }

            enfocarEntradaManualConTeclado();
        }, 120);
    }

    function enfocarEntradaManualConTeclado() {
        if (typeof activarTecladoParaInput === 'function') {
            activarTecladoParaInput(SELECTORES.inputManual, {
                anchorSelector: SELECTORES.inputManual,
                preferredSide: 'right'
            });

            return;
        }

        $(SELECTORES.inputManual).trigger('focus').trigger('select');
    }

    function posicionarTecladoNc(selectorInput, selectorAncla, ladoPreferido) {
        setTimeout(function () {
            if (typeof posicionarTecladoVirtual === 'function') {
                posicionarTecladoVirtual(
                    selectorInput,
                    selectorAncla || selectorInput,
                    {
                        preferredSide: ladoPreferido || 'right'
                    }
                );
            }
        }, 170);
    }

    function ocultarTecladoNc() {
        if (typeof ocultarTecladoVirtual === 'function') {
            ocultarTecladoVirtual();
        }
    }

    function obtenerMensajeOperacion(items, mensajePredeterminado) {
        if (!Array.isArray(items) || items.length === 0) {
            return mensajePredeterminado;
        }

        const item = items[0] || {};

        return item.mensaje ||
            item.respuesta_msj ||
            mensajePredeterminado;
    }

    function resumirRespuestaManual(response) {
        if (!response || typeof response !== 'object') {
            return {
                tipo: typeof response,
                valor: String(response || '')
            };
        }

        return {
            ok: response.ok,
            codigo: response.codigo || '',
            mensaje: response.mensaje || '',
            productosAgregados: response.productosAgregados || 0,
            productos: Array.isArray(response.productos)
                ? response.productos.length
                : 0,
            advertencias: Array.isArray(response.advertencias)
                ? response.advertencias.length
                : 0,
            rechazos: Array.isArray(response.rechazos)
                ? response.rechazos.length
                : 0,
            keys: Object.keys(response)
        };
    }

    function crearHtmlResumenOrigen(origen) {
        return `
            <dl class="row mb-0">
                <dt class="col-sm-5">Tipo</dt>
                <dd class="col-sm-7">
                    ${escaparHtml(origen.tco_id || '')}
                    -
                    ${escaparHtml(origen.tco_desc || '')}
                </dd>

                <dt class="col-sm-5">Número</dt>
                <dd class="col-sm-7">
                    ${escaparHtml(origen.cm_compte || '')}
                </dd>

                <dt class="col-sm-5">Repetición</dt>
                <dd class="col-sm-7">
                    ${escaparHtml(origen.cm_repetido ?? 0)}
                </dd>

                <dt class="col-sm-5">Cliente</dt>
                <dd class="col-sm-7">
                    ${escaparHtml(origen.cm_nombre || 'Consumidor Final')}
                </dd>

                <dt class="col-sm-5">Documento</dt>
                <dd class="col-sm-7">
                    ${escaparHtml(origen.cm_cuit || '-')}
                </dd>

                <dt class="col-sm-5">Total Original</dt>
                <dd class="col-sm-7">
                    <strong>
                        $ ${formatearImporte(origen.cm_total)}
                    </strong>
                </dd>
            </dl>
        `;
    }

    function crearHtmlResumenNc(origen) {
        const modalidad = modalidadActual?.cargarTodoDetalle === true
            ? 'Carga total'
            : 'Carga manual';

        const claseModalidad = modalidadActual?.cargarTodoDetalle === true
            ? 'bg-success'
            : 'bg-info';

        return `
            <dl class="row mb-0">
                <dt class="col-sm-5">Tipo NC</dt>
                <dd class="col-sm-7">
                    ${escaparHtml(origen.nc_tco_id || '')}
                    -
                    ${escaparHtml(origen.nc_tco_desc || '')}
                </dd>

                <dt class="col-sm-5">Letra</dt>
                <dd class="col-sm-7">
                    ${escaparHtml(origen.nc_tco_letra || '-')}
                </dd>

                <dt class="col-sm-5">Modalidad</dt>
                <dd class="col-sm-7">
                    <span class="badge ${claseModalidad}">
                        ${escaparHtml(modalidad)}
                    </span>
                </dd>

                <dt class="col-sm-5">Cuenta Corriente</dt>
                <dd class="col-sm-7">
                    ${Number(origen.nc_ctacte) === 1
                ? '<span class="badge bg-info">Habilitada</span>'
                : '<span class="badge bg-secondary">No habilitada</span>'}
                </dd>
            </dl>
        `;
    }

    function renderizarResumenOperacion() {
        const origen = comprobanteOrigen || {};

        poblarDatosClienteProductos(origen);

        $(SELECTORES.resumenOrigen).html(crearHtmlResumenOrigen(origen));
        $(SELECTORES.resumenNc).html(crearHtmlResumenNc(origen));

        $(SELECTORES.modalidadBadge)
            .removeClass(
                'bg-light text-dark bg-success bg-info'
            )
            .addClass(
                modalidadActual?.cargarTodoDetalle === true
                    ? 'bg-success'
                    : 'bg-info'
            )
            .text(
                modalidadActual?.cargarTodoDetalle === true
                    ? 'Carga total'
                    : 'Carga manual'
            );
    }

    function poblarDatosClienteProductos(origen) {
        const tipoNc = `${origen.nc_tco_id || ''} ${origen.nc_tco_desc || ''}`.trim();

        $(SELECTORES.clienteNombreProd).val(
            origen.cm_nombre || 'Consumidor Final'
        );
        $(SELECTORES.clienteIdProd).val(origen.cta_id || 'N/A');
        $(SELECTORES.clienteDomicilioProd).val(origen.cm_domicilio || '');
        $(SELECTORES.condicionAfipProd).val(origen.afip_desc || '');
        $(SELECTORES.clienteCuitProd).val(origen.cm_cuit || '');
        $(SELECTORES.clienteEmailProd).val(origen.cm_email || '');
        $(SELECTORES.clienteMovilProd).val(origen.cm_movil || '');

        $(SELECTORES.tipoComprobanteBadge)
            .html(
                '<i class="bx bx-file"></i> ' +
                escaparHtml(tipoNc || 'NC')
            );
    }

    function renderizarProductos(
        productos,
        advertencias,
        rechazos,
        origenCarga = 'TODOS'
    ) {
        productosActuales = Array.isArray(productos)
            ? productos
            : [];

        limpiarCalculoActual();

        actualizarCantidadProductos(productos.length);
        renderizarTablaProductos(productos);
        renderizarMensajesProductos(advertencias, rechazos);

        const tieneProductos = productos.length > 0;
        const esManual = origenCarga === 'MANUAL';

        let claseEstado = 'alert-info';
        let mensajeEstado = '';

        if (esManual) {
            claseEstado = tieneProductos
                ? 'alert-success'
                : 'alert-info';

            mensajeEstado = tieneProductos
                ? `La devolución contiene ${productos.length} producto(s) cargado(s).`
                : 'Aún no hay productos cargados. Ingrese un código para comenzar.';
        } else {
            claseEstado = tieneProductos
                ? 'alert-success'
                : 'alert-warning';

            mensajeEstado = tieneProductos
                ? `Se cargaron ${productos.length} producto(s) desde el comprobante original.`
                : 'La carga finalizó sin productos disponibles para devolver.';
        }

        $(SELECTORES.estado)
            .removeClass(
                'd-none alert-info alert-success alert-warning alert-danger'
            )
            .addClass(claseEstado)
            .html(
                `<i class="bx ${tieneProductos
                    ? 'bx-check-circle'
                    : 'bx-info-circle'} me-1"></i>` +
                escaparHtml(mensajeEstado)
            );

        $(SELECTORES.resultado).removeClass('d-none');
        actualizarAccionesProductos(tieneProductos);

        logInfo('PRODUCTOS - RENDER', {
            accion: 'Productos renderizados.',
            productosCargados: productos.length,
            advertencias: advertencias.length,
            rechazos: rechazos.length
        });
    }

    function renderizarTablaProductos(productos) {
        const $tbody = $(SELECTORES.tablaProductos);

        if (!productos.length) {
            $tbody.html(`
                <tr>
                    <td colspan="6"
                         class="text-center text-muted py-4 td-compact">
                        No hay productos cargados para esta devolución.
                    </td>
                </tr>
            `);

            return;
        }

        $tbody.html(
            productos.map(function (producto) {
                const codigo = producto.p_id_barrado ||
                    producto.p_id ||
                    '-';

                const combo = String(
                    producto.cmd_cmb_desc || ''
                ).trim();

                const descripcion = combo
                    ? `${escaparHtml(producto.p_desc || '')}
                       <div class="small text-muted mt-1">
                           <i class="bx bx-package me-1"></i>
                           ${escaparHtml(combo)}
                       </div>`
                    : escaparHtml(producto.p_desc || '');

                return `
                    <tr class="ncdev-producto-row">
                        <td class="text-center td-compact">
                            <strong>${escaparHtml(codigo)}</strong>
                        </td>

                        <td class="text-start td-compact">${descripcion}</td>

                        <td class="text-end td-compact">
                            ${formatearCantidad(producto.cantidad_tot)}
                        </td>

                        <td class="text-end td-compact">
                            $ ${formatearImporte(producto.p_pvta)}
                        </td>

                        <td class="text-end td-compact">
                            $ ${formatearImporte(producto.p_pneto)}
                        </td>

                        <td class="text-end td-compact">
                            $ ${formatearImporte(producto.p_iva)}
                        </td>
                    </tr>
                `;
            }).join('')
        );
    }

    function renderizarMensajesProductos(
        advertencias,
        rechazos
    ) {
        renderizarListaMensajes(
            SELECTORES.advertencias,
            SELECTORES.listaAdvertencias,
            advertencias
        );

        renderizarListaMensajes(
            SELECTORES.rechazos,
            SELECTORES.listaRechazos,
            rechazos
        );
    }

    function renderizarListaMensajes(
        selectorContenedor,
        selectorLista,
        mensajes
    ) {
        if (!mensajes.length) {
            $(selectorContenedor).addClass('d-none');
            $(selectorLista).empty();
            return;
        }

        $(selectorLista).html(
            mensajes.map(function (item) {
                const codigo = item?.p_id_barrado ||
                    item?.p_id ||
                    'Sin código';

                const descripcion = item?.p_desc ||
                    'Producto sin descripción';

                const mensaje = item?.mensaje ||
                    'Sin detalle adicional.';

                return `
                    <li>
                        <strong>${escaparHtml(codigo)}</strong>
                        -
                        ${escaparHtml(descripcion)}

                        <div class="small">
                            ${escaparHtml(mensaje)}
                        </div>
                    </li>
                `;
            }).join('')
        );

        $(selectorContenedor).removeClass('d-none');
    }

    function mostrarCargando() {
        $(SELECTORES.estado)
            .removeClass(
                'd-none alert-success alert-warning alert-danger'
            )
            .addClass('alert-info')
            .html(
                '<i class="bx bx-loader-alt bx-spin me-1"></i>' +
                'Obteniendo productos del comprobante original...'
            );

        $(SELECTORES.cargando).removeClass('d-none');
    }

    function mostrarErrorCarga(mensaje) {
        const texto = String(
            mensaje ||
            'No fue posible cargar el detalle del comprobante original.'
        );

        $(SELECTORES.error)
            .removeClass('d-none')
            .html(
                `<i class="bx bx-error-circle me-1"></i>
                 <strong>Error al cargar productos.</strong>
                 ${escaparHtml(texto)}`
            );

        $(SELECTORES.estado)
            .removeClass(
                'd-none alert-info alert-success alert-warning'
            )
            .addClass('alert-danger')
            .html(
                `<i class="bx bx-x-circle me-1"></i>
                 ${escaparHtml(texto)}`
            );
    }

    function mostrarErrorConfiguracion(mensaje) {
        logError('PRODUCTOS - CONFIGURACIÓN', {
            mensaje: mensaje
        });

        mostrarErrorCarga(mensaje);
    }

    function limpiarEstadoVisual() {
        productosActuales = [];
        calculoActual = null;

        $(SELECTORES.cargando).addClass('d-none');
        $(SELECTORES.error).addClass('d-none').empty();
        $(SELECTORES.manual).addClass('d-none');
        $(SELECTORES.resultado).addClass('d-none');
        $(SELECTORES.destinoOperacion).empty();
        $(SELECTORES.conceptosCalculo).empty();
        $(SELECTORES.totalFinalCalculo).text('$ 0,00');
        $(SELECTORES.btnFinalizar).addClass('d-none').prop('disabled', true);
        $(SELECTORES.btnSeguir).prop('disabled', true);

        $(SELECTORES.advertencias).addClass('d-none');
        $(SELECTORES.listaAdvertencias).empty();

        $(SELECTORES.rechazos).addClass('d-none');
        $(SELECTORES.listaRechazos).empty();

        $(SELECTORES.tablaProductos).html(`
            <tr>
                <td colspan="6"
                    class="text-center text-muted py-4">
                    No hay productos cargados.
                </td>
            </tr>
        `);

        actualizarCantidadProductos(0);

        $(SELECTORES.inputManual).val('');
        bloquearEntradaManual(false);
        limpiarEstadoEntradaManual();
    }

    function limpiarCalculoActual() {
        calculoActual = null;

        $(SELECTORES.destinoOperacion).empty();
        $(SELECTORES.conceptosCalculo).empty();
        $(SELECTORES.totalFinalCalculo).text('$ 0,00');
        $(SELECTORES.btnFinalizar).addClass('d-none').prop('disabled', true);
        $(SELECTORES.btnSeguir).prop('disabled', productosActuales.length === 0);

        bloquearEntradaManual(false);
    }

    function actualizarAccionesProductos(tieneProductos) {
        const puedeSeguir = tieneProductos === true &&
            !calculoEnCurso &&
            !finalizacionEnCurso &&
            calculoActual === null;

        $(SELECTORES.btnSeguir).prop('disabled', !puedeSeguir);
    }

    function iniciarDefinicionDestino() {
        if (calculoEnCurso || finalizacionEnCurso) {
            return;
        }

        if (!productosActuales.length) {
            mostrarMensaje(
                'Atención',
                'Debe cargar al menos un producto para continuar.',
                'warn!'
            );

            return;
        }

        const origen = comprobanteOrigen || {};
        const esForzadoCuentaCorriente =
            Number(origen.nc_dv_dist) === 1 ||
            Number(origen.nc_dv_pago_diferido) === 1;

        if (esForzadoCuentaCorriente) {
            ejecutarSeguir({});
            return;
        }

        const requierePregunta =
            Number(origen.nc_ctacte) === 1 &&
            Number(origen.nc_dv_dist) === 0 &&
            Number(origen.nc_dv_pago_diferido) === 0;

        if (!requierePregunta) {
            ejecutarSeguir({
                dejarEnCuentaCorriente: false,
                confirmacionCuentaCorriente: false
            });

            return;
        }

        preguntarCuentaCorriente();
    }

    function preguntarCuentaCorriente() {
        AbrirMensaje(
            'Cuenta Corriente',
            '¿Desea dejar el saldo de la Nota de Crédito en Cuenta Corriente?',
            function (respuesta) {
                $('#msjModal').modal('hide');

                if (respuesta !== 'SI') {
                    ejecutarSeguir({
                        dejarEnCuentaCorriente: false,
                        confirmacionCuentaCorriente: false
                    });

                    return;
                }

                preguntarConfirmacionCuentaCorriente();
            },
            true,
            ['Sí', 'No'],
            'info!'
        );
    }

    function preguntarConfirmacionCuentaCorriente() {
        AbrirMensaje(
            'Confirmar Cuenta Corriente',
            'Confirme que el saldo quedará a favor del cliente en Cuenta Corriente.',
            function (respuesta) {
                $('#msjModal').modal('hide');

                if (respuesta !== 'SI') {
                    ejecutarSeguir({
                        dejarEnCuentaCorriente: false,
                        confirmacionCuentaCorriente: false
                    });

                    return;
                }

                ejecutarSeguir({
                    dejarEnCuentaCorriente: true,
                    confirmacionCuentaCorriente: true
                });
            },
            true,
            ['Confirmar', 'Volver'],
            'warn!'
        );
    }

    function ejecutarSeguir(payload) {
        const url = String(window.ncDevolucionSeguirUrl || '').trim();

        if (!url) {
            mostrarMensaje(
                'Error',
                'No se encontró la URL para calcular la Nota de Crédito.',
                'error!'
            );

            return;
        }

        calculoEnCurso = true;
        bloquearAccionSeguir(true);
        bloquearEntradaManual(true);

        logInfo('PRODUCTOS - SEGUIR', {
            accion: 'Invocando cálculo de filas para NC.',
            payload: payload
        });

        $.ajax({
            url: url,
            type: 'POST',
            contentType: 'application/json; charset=utf-8',
            dataType: 'json',
            timeout: 30000,
            data: JSON.stringify(payload || {})
        })
            .done(function (response) {
                logInfo('PRODUCTOS - SEGUIR', {
                    accion: 'Respuesta recibida desde Seguir.',
                    respuesta: response
                });

                if (!response || response.ok !== true) {
                    mostrarMensaje(
                        'Atención',
                        response?.mensaje ||
                        'No fue posible calcular la Nota de Crédito.',
                        'warn!'
                    );

                    return;
                }

                const subtotales = obtenerSubtotalesDesdeRespuesta(response);

                if (!Array.isArray(subtotales) ||
                    subtotales.length === 0) {
                    logAdvertencia('PRODUCTOS - SEGUIR', {
                        accion: 'La respuesta no contiene subtotales normalizables.',
                        subtotales: response?.subtotales || null,
                        jsonSubtotal: response?.calculo?.json_subtotal || ''
                    });

                    mostrarMensaje(
                        'Atención',
                        'El cálculo no devolvió subtotales. No se puede continuar con la Nota de Crédito.',
                        'warn!'
                    );

                    return;
                }

                response.subtotales = subtotales;
                calculoActual = response;
                renderizarCalculo(response);
            })
            .fail(function (xhr, status, error) {
                logError('PRODUCTOS - SEGUIR', {
                    accion: 'Error HTTP o de red al calcular NC.',
                    httpStatus: xhr?.status || 0,
                    textStatus: status || '',
                    error: String(error || ''),
                    mensajeApi: xhr?.responseJSON?.mensaje || ''
                });

                mostrarMensaje(
                    'Error de Comunicación',
                    xhr?.responseJSON?.mensaje ||
                    'Ocurrió un error al calcular la Nota de Crédito.',
                    'error!'
                );
            })
            .always(function () {
                calculoEnCurso = false;
                bloquearAccionSeguir(false);
                if (!calculoActual) {
                    bloquearEntradaManual(false);
                }
                actualizarAccionesProductos(productosActuales.length > 0);
            });
    }

    function obtenerSubtotalesDesdeRespuesta(response) {
        const subtotalesDirectos = normalizarSubtotales(
            response?.subtotales
        );

        if (subtotalesDirectos.length > 0) {
            return subtotalesDirectos;
        }

        const jsonSubtotal =
            response?.calculo?.json_subtotal ||
            response?.calculo?.jsonSubtotal ||
            response?.json_subtotal ||
            response?.jsonSubtotal ||
            '';

        return normalizarSubtotales(parsearJsonSeguro(jsonSubtotal));
    }

    function parsearJsonSeguro(valor) {
        if (!valor) {
            return [];
        }

        if (Array.isArray(valor) || typeof valor === 'object') {
            return valor;
        }

        if (typeof valor !== 'string') {
            return [];
        }

        try {
            const parseado = JSON.parse(valor);

            return typeof parseado === 'string'
                ? parsearJsonSeguro(parseado)
                : parseado;
        } catch (error) {
            logAdvertencia('PRODUCTOS - SEGUIR', {
                accion: 'No se pudo parsear json_subtotal.',
                error: String(error || ''),
                jsonSubtotal: valor
            });

            return [];
        }
    }

    function normalizarSubtotales(origen) {
        const filas = extraerFilasSubtotales(origen);

        return filas
            .map(normalizarFilaSubtotal)
            .filter(function (item) {
                return item.concepto || item.tipo || item.id_aux || item.importe !== 0;
            });
    }

    function extraerFilasSubtotales(origen) {
        if (Array.isArray(origen)) {
            return origen;
        }

        if (!origen || typeof origen !== 'object') {
            return [];
        }

        const posiblesColecciones = [
            origen.subtotales,
            origen.Subtotales,
            origen.data,
            origen.Data,
            origen.rows,
            origen.Rows,
            origen.items,
            origen.Items
        ];

        for (const coleccion of posiblesColecciones) {
            if (Array.isArray(coleccion)) {
                return coleccion;
            }
        }

        return [origen];
    }

    function normalizarFilaSubtotal(item) {
        const fila = item || {};

        return {
            orden: obtenerValor(fila, ['orden', 'Orden']) || 0,
            tipo: String(obtenerValor(fila, ['tipo', 'Tipo', 'id', 'ID']) || ''),
            concepto: String(
                obtenerValor(
                    fila,
                    [
                        'concepto',
                        'Concepto',
                        'descripcion',
                        'Descripcion',
                        'detalle',
                        'Detalle'
                    ]
                ) || ''
            ),
            base: normalizarNumero(
                obtenerValor(fila, ['base', 'Base', 'BaseImponible'])
            ),
            alicuota: normalizarNumero(
                obtenerValor(fila, ['alicuota', 'Alicuota'])
            ),
            importe: normalizarNumero(
                obtenerValor(
                    fila,
                    [
                        'importe',
                        'Importe',
                        'total',
                        'Total',
                        'monto',
                        'Monto',
                        'valor',
                        'Valor'
                    ]
                )
            ),
            id_aux: obtenerValor(fila, ['id_aux', 'IdAux', 'idAux']) || ''
        };
    }

    function obtenerValor(origen, nombres) {
        for (const nombre of nombres) {
            if (Object.prototype.hasOwnProperty.call(origen, nombre)) {
                return origen[nombre];
            }
        }

        return null;
    }

    function normalizarNumero(valor) {
        if (typeof valor === 'number') {
            return Number.isFinite(valor) ? valor : 0;
        }

        const texto = String(valor ?? '').trim();

        if (!texto) {
            return 0;
        }

        const sinSeparadoresMiles = texto
            .replace(/\s/g, '')
            .replace(/\.(?=\d{3}(?:\D|$))/g, '')
            .replace(',', '.');

        const numero = Number(sinSeparadoresMiles);

        return Number.isFinite(numero) ? numero : 0;
    }

    function renderizarCalculo(response) {
        const destino = response?.destino || '';
        const coTipo = response?.co_tipo || '';
        const subtotales = Array.isArray(response?.subtotales)
            ? response.subtotales
            : [];
        const total = obtenerTotalCalculo(subtotales);

        $(SELECTORES.destinoOperacion)
            .removeClass('bg-light text-dark bg-success bg-info')
            .addClass(coTipo === 'DV' ? 'bg-info' : 'bg-success')
            .text(destino || coTipo);

        renderizarDatosCalculo();
        renderizarConceptosCalculo(subtotales);
        $(SELECTORES.totalFinalCalculo).text(`$ ${formatearImporte(total)}`);
        $(SELECTORES.btnSeguir).prop('disabled', true);
        $(SELECTORES.btnFinalizar)
            .removeClass('d-none')
            .prop('disabled', false);

        bloquearEntradaManual(true);

        $(SELECTORES.estado)
            .removeClass('d-none alert-info alert-warning alert-danger')
            .addClass('alert-success')
            .html(
                '<i class="bx bx-check-circle me-1"></i>' +
                'Totales calculados. Verifique la información y finalice la Nota de Crédito.'
            );

        abrirModalCalculo();
    }

    function renderizarDatosCalculo() {
        const origen = comprobanteOrigen || {};

        $(SELECTORES.calcClienteNombre).val(
            origen.cm_nombre || 'Consumidor Final'
        );
        $(SELECTORES.calcClienteId).val(origen.cta_id || '');
        $(SELECTORES.calcClienteDomicilio).val(origen.cm_domicilio || '');
        $(SELECTORES.calcComprobanteOrigen).val(
            `${origen.tco_id || ''} ${origen.cm_compte || ''}`.trim()
        );
        $(SELECTORES.calcTipoNc).val(
            `${origen.nc_tco_id || ''} ${origen.nc_tco_desc || ''}`.trim()
        );
        $(SELECTORES.calcCondicionAfip).val(origen.afip_desc || '');
        $(SELECTORES.calcDocumento).val(origen.cm_cuit || '');
        $(SELECTORES.calcEmail).val(origen.cm_email || '');
        $(SELECTORES.calcMovil).val(origen.cm_movil || '');

        const resumenOrigenHtml = crearHtmlResumenOrigen(origen);
        const resumenNcHtml = crearHtmlResumenNc(origen);

        $(SELECTORES.calculoResumenOrigen).html(resumenOrigenHtml);
        $(SELECTORES.calculoResumenNc).html(resumenNcHtml);
    }

    function renderizarConceptosCalculo(subtotales) {
        $(SELECTORES.conceptosCalculo).html(
            subtotales.map(function (item) {
                const concepto = item.concepto || item.tipo || '-';
                const claseTotal = esFilaTotal(item)
                    ? 'fw-bold text-success'
                    : '';

                return `
                    <tr>
                        <td class="text-start ${claseTotal}">
                            ${escaparHtml(concepto)}
                        </td>
                        <td class="text-end ${claseTotal}">
                            $ ${formatearImporte(item.importe)}
                        </td>
                    </tr>
                `;
            }).join('')
        );
    }

    function obtenerTotalCalculo(subtotales) {
        const filaTotal = subtotales.find(esFilaTotal);

        if (filaTotal) {
            return Number(filaTotal.importe) || 0;
        }

        const ultimaFila = subtotales[subtotales.length - 1] || {};

        return Number(ultimaFila.importe) || 0;
    }

    function esFilaTotal(item) {
        const concepto = String(item?.concepto || '').trim().toUpperCase();
        const tipo = String(item?.tipo || '').trim().toUpperCase();

        return concepto === 'TOTAL' || tipo === 'TOTAL';
    }

    function volverACargaProductos() {
        if (!modalCalculo || !modalProductos) {
            return;
        }

        $(SELECTORES.modalCalculo).one('hidden.bs.modal', function () {
            modalProductos.show();
        });

        modalCalculo.hide();
    }

    function abrirModalCalculo() {
        if (!modalCalculo) {
            mostrarMensaje(
                'Error',
                'No se encontró la pantalla de cálculo de la Nota de Crédito.',
                'error!'
            );

            return;
        }

        if (!modalProductos) {
            modalCalculo.show();
            return;
        }

        const $modalProductos = $(SELECTORES.modal);

        if (!$modalProductos.hasClass('show')) {
            modalCalculo.show();
            return;
        }

        $(SELECTORES.modal).one('hidden.bs.modal', function () {
            modalCalculo.show();
        });

        modalProductos.hide();
    }

    function confirmarFinalizacion() {
        if (finalizacionEnCurso) {
            return;
        }

        if (!calculoActual) {
            mostrarMensaje(
                'Atención',
                'Debe presionar Seguir y calcular los totales antes de finalizar.',
                'warn!'
            );

            return;
        }

        AbrirMensaje(
            'Finalizar Nota de Crédito',
            '¿Confirma la emisión de la Nota de Crédito por Devolución?',
            function (respuesta) {
                $('#msjModal').modal('hide');

                if (respuesta !== 'SI') {
                    return;
                }

                ejecutarFinalizar();
            },
            true,
            ['Confirmar', 'Cancelar'],
            'warn!'
        );
    }

    function ejecutarFinalizar() {
        const url = String(window.ncDevolucionFinalizarUrl || '').trim();

        if (!url) {
            mostrarMensaje(
                'Error',
                'No se encontró la URL para finalizar la Nota de Crédito.',
                'error!'
            );

            return;
        }

        finalizacionEnCurso = true;
        bloquearAccionFinalizar(true);
        bloquearAccionSeguir(true);
        bloquearEntradaManual(true);

        logInfo('PRODUCTOS - FINALIZAR', {
            accion: 'Invocando confirmación de NC.'
        });

        $.ajax({
            url: url,
            type: 'POST',
            dataType: 'json',
            timeout: 45000
        })
            .done(function (response) {
                logInfo('PRODUCTOS - FINALIZAR', {
                    accion: 'Respuesta recibida desde Finalizar.',
                    respuesta: response
                });

                if (!response || response.ok !== true) {
                    mostrarMensaje(
                        'Atención',
                        response?.mensaje ||
                        'No fue posible emitir la Nota de Crédito.',
                        'warn!'
                    );

                    return;
                }

                mostrarMensaje(
                    'Nota de Crédito emitida',
                    response.mensaje ||
                    'La Nota de Crédito fue emitida correctamente.',
                    'succ!',
                    function () {
                        window.location.href =
                            window.ncDevolucionIndexUrl ||
                            window.ncDevolucionMenuCajaUrl ||
                            window.location.href;
                    }
                );
            })
            .fail(function (xhr, status, error) {
                logError('PRODUCTOS - FINALIZAR', {
                    accion: 'Error HTTP o de red al finalizar NC.',
                    httpStatus: xhr?.status || 0,
                    textStatus: status || '',
                    error: String(error || ''),
                    mensajeApi: xhr?.responseJSON?.mensaje || ''
                });

                mostrarMensaje(
                    'Error de Comunicación',
                    xhr?.responseJSON?.mensaje ||
                    'Ocurrió un error al emitir la Nota de Crédito.',
                    'error!'
                );
            })
            .always(function () {
                finalizacionEnCurso = false;
                bloquearAccionFinalizar(false);
            });
    }

    function bloquearAccionSeguir(bloquear) {
        $(SELECTORES.btnSeguir).prop('disabled', bloquear);
        $(SELECTORES.spinnerSeguir).toggleClass('d-none', !bloquear);
        $(SELECTORES.iconoSeguir).toggleClass('d-none', bloquear);
    }

    function bloquearAccionFinalizar(bloquear) {
        $(SELECTORES.btnFinalizar).prop('disabled', bloquear);
        $(SELECTORES.spinnerFinalizar).toggleClass('d-none', !bloquear);
        $(SELECTORES.iconoFinalizar).toggleClass('d-none', bloquear);
    }

    function mostrarMensaje(titulo, mensaje, tipo, callback) {
        AbrirMensaje(
            titulo,
            mensaje,
            function () {
                $('#msjModal').modal('hide');

                if (typeof callback === 'function') {
                    callback();
                }
            },
            false,
            ['Aceptar'],
            tipo || 'info!'
        );
    }

    function actualizarCantidadProductos(cantidad) {
        const total = Number(cantidad) || 0;

        $(SELECTORES.cantidadProductos).text(
            `${total} ${total === 1 ? 'producto' : 'productos'}`
        );
    }

    function resumirComprobante(comprobante) {
        if (!comprobante) {
            return null;
        }

        return {
            tco_id: comprobante.tco_id || '',
            cm_compte: comprobante.cm_compte || '',
            cm_repetido: comprobante.cm_repetido ?? 0,
            nc_tco_id: comprobante.nc_tco_id || '',
            nc_tco_letra: comprobante.nc_tco_letra || ''
        };
    }

    function resumirRespuesta(response) {
        return {
            ok: response?.ok,
            codigo: response?.codigo || '',
            mensaje: response?.mensaje || '',
            productosCargados: response?.productosCargados || 0,
            advertencias: Array.isArray(response?.advertencias)
                ? response.advertencias.length
                : 0,
            rechazos: Array.isArray(response?.rechazos)
                ? response.rechazos.length
                : 0
        };
    }

    function formatearImporte(valor) {
        const numero = Number(valor);

        if (!Number.isFinite(numero)) {
            return '0,00';
        }

        return numero.toLocaleString('es-AR', {
            minimumFractionDigits: 2,
            maximumFractionDigits: 2
        });
    }

    function formatearCantidad(valor) {
        const numero = Number(valor);

        if (!Number.isFinite(numero)) {
            return '0';
        }

        return numero.toLocaleString('es-AR', {
            minimumFractionDigits: 0,
            maximumFractionDigits: 3
        });
    }

    function escaparHtml(valor) {
        return String(valor ?? '')
            .replace(/&/g, '&amp;')
            .replace(/</g, '&lt;')
            .replace(/>/g, '&gt;')
            .replace(/"/g, '&quot;')
            .replace(/'/g, '&#039;');
    }

    function logInfo(etapa, detalle) {
        console.log(
            `[NCDEV][PRODUCTOS][${new Date().toISOString()}] ${etapa}`,
            detalle || ''
        );
    }

    function logAdvertencia(etapa, detalle) {
        console.warn(
            `[NCDEV][PRODUCTOS][${new Date().toISOString()}] ${etapa}`,
            detalle || ''
        );
    }

    function logError(etapa, detalle) {
        console.error(
            `[NCDEV][PRODUCTOS][${new Date().toISOString()}] ${etapa}`,
            detalle || ''
        );
    }

    function logDebug(etapa, detalle) {
        console.debug(
            `[NCDEV][PRODUCTOS][${new Date().toISOString()}] ${etapa}`,
            detalle || ''
        );
    }

    namespace.productos = {
        cargarDetalleCompleto: cargarDetalleCompleto,
        procesarEntradaManual: procesarEntradaManual,
        hidratarProductosDesdeSesion: hidratarProductosDesdeSesion
    };

})(window.NCDevolucion);
