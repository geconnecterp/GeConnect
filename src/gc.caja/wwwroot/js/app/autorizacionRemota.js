(function (window, document) {
    'use strict';

    const SELECTORES = Object.freeze({
        modal: '#modalAutorizacionRemota',
        titulo: '#autorizacionRemotaTitulo',
        subtitulo: '#autorizacionRemotaSubtitulo',
        indicador: '#autorizacionRemotaIndicador',
        mensaje: '#autorizacionRemotaMensaje',
        detalle: '#autorizacionRemotaDetalle',
        estado: '#autorizacionRemotaEstado',
        tiempo: '#autorizacionRemotaTiempo'
    });

    const ESTADOS_TERMINALES = Object.freeze({
        APROBADA: 'APROBADA',
        RECHAZADA: 'RECHAZADA',
        EXPIRADA: 'EXPIRADA',
        REEMPLAZADA: 'REEMPLAZADA',
        ERROR: 'ERROR'
    });

    let esperaActiva = null;

    function elemento(selector) {
        return document.querySelector(selector);
    }

    function texto(selector, valor) {
        const nodo = elemento(selector);
        if (nodo) {
            nodo.textContent = valor == null ? '' : String(valor);
        }
    }

    function normalizarConfiguracion(config) {
        if (!config || typeof config !== 'object') {
            throw new Error('La configuración de autorización es requerida.');
        }

        const idSolicitud = String(config.idSolicitud || '').trim();
        const claveOperacion = String(config.claveOperacion || '').trim().toUpperCase();
        const urlEstado = String(config.urlEstado || '').trim();

        if (!idSolicitud || !claveOperacion || !urlEstado) {
            throw new Error('idSolicitud, claveOperacion y urlEstado son requeridos.');
        }

        const modal = elemento(SELECTORES.modal);
        if (!modal) {
            throw new Error('No se encontró el modal reutilizable de autorización remota.');
        }

        const intervaloConfigurado = Number(modal.dataset.intervaloConsulta);
        const intervaloSolicitado = Number(config.intervaloMs);
        const intervaloMs = Number.isFinite(intervaloSolicitado) && intervaloSolicitado >= 500
            ? intervaloSolicitado
            : (Number.isFinite(intervaloConfigurado) && intervaloConfigurado >= 500
                ? intervaloConfigurado
                : 2000);

        const timeoutSegundos = Number(config.timeoutSegundos);
        const fechaLimiteMs = Number.isFinite(timeoutSegundos) && timeoutSegundos > 0
            ? Date.now() + (timeoutSegundos * 1000)
            : null;

        return {
            idSolicitud,
            claveOperacion,
            urlEstado,
            intervaloMs,
            fechaLimiteMs,
            margenErrorMs: Math.max(intervaloMs * 3, 10000),
            titulo: config.titulo || 'Esperando autorización',
            subtitulo: config.subtitulo || 'La operación requiere confirmación remota',
            mensaje: config.mensaje || 'Solicitud enviada. Esperando la respuesta de un administrador.',
            detalle: config.detalle || 'No cierre ni actualice esta pantalla.',
            retardoCierreMs: Number.isFinite(Number(config.retardoCierreMs))
                ? Math.max(0, Number(config.retardoCierreMs))
                : 650,
            callbacks: {
                aprobada: typeof config.onAprobada === 'function' ? config.onAprobada : null,
                rechazada: typeof config.onRechazada === 'function' ? config.onRechazada : null,
                expirada: typeof config.onExpirada === 'function' ? config.onExpirada : null,
                reemplazada: typeof config.onReemplazada === 'function' ? config.onReemplazada : null,
                error: typeof config.onError === 'function' ? config.onError : null,
                estado: typeof config.onEstado === 'function' ? config.onEstado : null
            }
        };
    }

    function prepararModal(config) {
        texto(SELECTORES.titulo, config.titulo);
        texto(SELECTORES.subtitulo, config.subtitulo);
        texto(SELECTORES.mensaje, config.mensaje);
        texto(SELECTORES.detalle, config.detalle);
        texto(SELECTORES.estado, 'PENDIENTE');
        texto(SELECTORES.tiempo, '');

        const estado = elemento(SELECTORES.estado);
        if (estado) {
            estado.className = 'badge rounded-pill text-bg-warning';
        }

        mostrarIndicadorEspera();
    }

    function mostrarIndicadorEspera() {
        const indicador = elemento(SELECTORES.indicador);
        if (!indicador) {
            return;
        }

        indicador.className = 'autorizacion-remota-indicador';
        indicador.replaceChildren();

        const spinner = document.createElement('div');
        spinner.className = 'spinner-border text-warning';
        spinner.setAttribute('role', 'status');

        const oculto = document.createElement('span');
        oculto.className = 'visually-hidden';
        oculto.textContent = 'Consultando autorización...';
        spinner.appendChild(oculto);
        indicador.appendChild(spinner);
    }

    function mostrarResultado(icono, claseColor) {
        const indicador = elemento(SELECTORES.indicador);
        if (!indicador) {
            return;
        }

        indicador.className = `autorizacion-remota-indicador autorizacion-remota-resultado ${claseColor}`;
        indicador.replaceChildren();
        const icon = document.createElement('i');
        icon.className = `bx ${icono}`;
        indicador.appendChild(icon);
    }

    function actualizarEstadoVisual(respuesta) {
        const estadoTexto = String(respuesta.estado || 'PENDIENTE').toUpperCase();
        texto(SELECTORES.estado, estadoTexto);

        const badge = elemento(SELECTORES.estado);
        if (badge) {
            badge.className = estadoTexto === 'EN_PROCESO'
                ? 'badge rounded-pill text-bg-info'
                : 'badge rounded-pill text-bg-warning';
        }

        if (estadoTexto === 'EN_PROCESO') {
            texto(SELECTORES.mensaje, 'Un administrador está revisando la solicitud.');
            texto(SELECTORES.detalle, 'Aguarde la confirmación para continuar.');
        }
    }

    function actualizarCuentaRegresiva() {
        if (!esperaActiva || !esperaActiva.config.fechaLimiteMs) {
            return;
        }

        const restanteMs = Math.max(0, esperaActiva.config.fechaLimiteMs - Date.now());
        const totalSegundos = Math.ceil(restanteMs / 1000);
        const minutos = Math.floor(totalSegundos / 60);
        const segundos = String(totalSegundos % 60).padStart(2, '0');
        texto(SELECTORES.tiempo, `Tiempo estimado: ${minutos}:${segundos}`);
    }

    function emitir(tipo, detalle) {
        document.dispatchEvent(new CustomEvent(`autorizacionremota:${tipo}`, { detail: detalle }));
    }

    function abrirModal(modal) {
        const instancia = bootstrap.Modal.getOrCreateInstance(modal, {
            backdrop: 'static',
            keyboard: false,
            focus: true
        });

        modal.addEventListener('shown.bs.modal', function marcarBackdrop() {
            const backdrops = document.querySelectorAll('.modal-backdrop');
            const backdrop = backdrops[backdrops.length - 1];
            if (backdrop) {
                backdrop.dataset.autorizacionRemota = 'true';
            }
        }, { once: true });

        instancia.show();
        return instancia;
    }

    function ocultarModal(espera) {
        return new Promise(function (resolve) {
            const modal = espera.modal;

            modal.addEventListener('hidden.bs.modal', function alOcultar() {
                if (document.querySelector('.modal.show')) {
                    document.body.classList.add('modal-open');
                }

                if (espera.focoAnterior && typeof espera.focoAnterior.focus === 'function') {
                    try {
                        espera.focoAnterior.focus({ preventScroll: true });
                    } catch (_) {
                        // El elemento original puede haber dejado de existir.
                    }
                }
                resolve();
            }, { once: true });

            espera.modalBootstrap.hide();
        });
    }

    function limpiarTemporizadores(espera) {
        if (espera.timerConsulta) {
            window.clearTimeout(espera.timerConsulta);
        }
        if (espera.timerCuentaRegresiva) {
            window.clearInterval(espera.timerCuentaRegresiva);
        }
        espera.abortController?.abort();
    }

    async function finalizar(tipo, respuesta) {
        const espera = esperaActiva;
        if (!espera || espera.finalizada) {
            return;
        }

        espera.finalizada = true;
        limpiarTemporizadores(espera);

        const presentaciones = {
            [ESTADOS_TERMINALES.APROBADA]: {
                estado: 'APROBADA', badge: 'text-bg-success', icono: 'bx-check-circle', color: 'text-success',
                mensaje: 'Autorización aprobada.', detalle: respuesta.mensaje || 'La operación puede continuar.', callback: 'aprobada'
            },
            [ESTADOS_TERMINALES.RECHAZADA]: {
                estado: 'RECHAZADA', badge: 'text-bg-danger', icono: 'bx-x-circle', color: 'text-danger',
                mensaje: 'Autorización rechazada.', detalle: respuesta.mensaje || 'La operación no fue autorizada.', callback: 'rechazada'
            },
            [ESTADOS_TERMINALES.EXPIRADA]: {
                estado: 'EXPIRADA', badge: 'text-bg-secondary', icono: 'bx-time-five', color: 'text-secondary',
                mensaje: 'La solicitud expiró.', detalle: respuesta.mensaje || 'No se recibió respuesta dentro del tiempo permitido.', callback: 'expirada'
            },
            [ESTADOS_TERMINALES.REEMPLAZADA]: {
                estado: 'REEMPLAZADA', badge: 'text-bg-secondary', icono: 'bx-refresh', color: 'text-secondary',
                mensaje: 'La solicitud ya no está vigente.', detalle: respuesta.mensaje || 'Existe una solicitud más reciente.', callback: 'reemplazada'
            },
            [ESTADOS_TERMINALES.ERROR]: {
                estado: 'ERROR', badge: 'text-bg-danger', icono: 'bx-error-circle', color: 'text-danger',
                mensaje: 'No se pudo completar la autorización.', detalle: respuesta.mensaje || 'Intente nuevamente.', callback: 'error'
            }
        };

        let tipoFinal = tipo;
        let vista = presentaciones[tipoFinal];
        let respuestaFinal = respuesta;

        const resultadoCallback = {
            tipo: tipoFinal,
            idSolicitud: espera.config.idSolicitud,
            claveOperacion: espera.config.claveOperacion,
            respuesta: respuestaFinal
        };

        if (tipoFinal === ESTADOS_TERMINALES.APROBADA && espera.config.callbacks.aprobada) {
            texto(SELECTORES.estado, 'APLICANDO');
            texto(SELECTORES.mensaje, 'Autorización aprobada. Aplicando la operación...');
            texto(SELECTORES.detalle, 'Espere mientras se confirma el cambio en el servidor.');
        }

        try {
            await Promise.resolve(espera.config.callbacks[vista.callback]?.(resultadoCallback));
        } catch (error) {
            tipoFinal = ESTADOS_TERMINALES.ERROR;
            vista = presentaciones[tipoFinal];
            respuestaFinal = {
                mensaje: error?.message || 'No se pudo aplicar la operación autorizada.'
            };
            try {
                await Promise.resolve(espera.config.callbacks.error?.({
                    tipo: tipoFinal,
                    idSolicitud: espera.config.idSolicitud,
                    claveOperacion: espera.config.claveOperacion,
                    respuesta: respuestaFinal
                }));
            } catch (callbackError) {
                console.error('Error en el callback de autorización remota.', callbackError);
            }
        }

        texto(SELECTORES.estado, vista.estado);
        texto(SELECTORES.mensaje, vista.mensaje);
        texto(
            SELECTORES.detalle,
            tipoFinal === ESTADOS_TERMINALES.ERROR
                ? respuestaFinal.mensaje
                : vista.detalle);
        texto(SELECTORES.tiempo, '');

        const badge = elemento(SELECTORES.estado);
        if (badge) {
            badge.className = `badge rounded-pill ${vista.badge}`;
        }
        mostrarResultado(vista.icono, vista.color);

        const resultado = {
            tipo: tipoFinal,
            idSolicitud: espera.config.idSolicitud,
            claveOperacion: espera.config.claveOperacion,
            respuesta: respuestaFinal
        };

        emitir(vista.callback, resultado);
        await new Promise(function (resolve) {
            window.setTimeout(resolve, espera.config.retardoCierreMs);
        });
        await ocultarModal(espera);
        if (esperaActiva === espera) {
            esperaActiva = null;
        }
        espera.resolve(resultado);
    }

    function programarConsulta(espera, demora) {
        if (!espera.finalizada) {
            espera.timerConsulta = window.setTimeout(function () {
                consultarEstado(espera);
            }, demora);
        }
    }

    async function consultarEstado(espera) {
        if (espera.finalizada || espera.consultando || esperaActiva !== espera) {
            return;
        }

        espera.consultando = true;
        espera.abortController = new AbortController();

        try {
            const response = await fetch(espera.config.urlEstado, {
                method: 'GET',
                credentials: 'same-origin',
                cache: 'no-store',
                headers: { 'Accept': 'application/json' },
                signal: espera.abortController.signal
            });

            let data = null;
            try {
                data = await response.json();
            } catch (_) {
                data = null;
            }

            if (response.status === 401 || response.status === 403) {
                await finalizar(ESTADOS_TERMINALES.ERROR, {
                    mensaje: data?.mensaje || 'La sesión ya no permite consultar la autorización.'
                });
                return;
            }

            if (response.status >= 400 && response.status < 500) {
                await finalizar(ESTADOS_TERMINALES.ERROR, {
                    mensaje: data?.mensaje || `La consulta respondió HTTP ${response.status}.`
                });
                return;
            }

            if (!response.ok || !data?.ok) {
                throw new Error(data?.mensaje || `La consulta respondió HTTP ${response.status}.`);
            }

            espera.erroresConsecutivos = 0;
            espera.config.callbacks.estado?.(data);
            emitir('estado', data);

            if (data.vigente === false || String(data.estado).toUpperCase() === 'REEMPLAZADA') {
                await finalizar(ESTADOS_TERMINALES.REEMPLAZADA, data);
                return;
            }

            const estado = String(data.estado || '').toUpperCase();
            if (estado === 'EXPIRADO') {
                await finalizar(ESTADOS_TERMINALES.EXPIRADA, data);
                return;
            }

            if (estado === 'RESUELTO' || data.terminal === true) {
                await finalizar(
                    data.aprobada === true ? ESTADOS_TERMINALES.APROBADA : ESTADOS_TERMINALES.RECHAZADA,
                    data);
                return;
            }

            actualizarEstadoVisual(data);
            programarConsulta(espera, espera.config.intervaloMs);
        } catch (error) {
            if (error?.name === 'AbortError' || espera.finalizada) {
                return;
            }

            espera.erroresConsecutivos += 1;
            texto(SELECTORES.estado, 'RECONECTANDO');
            texto(SELECTORES.mensaje, 'Se perdió momentáneamente la conexión.');
            texto(SELECTORES.detalle, 'La solicitud continúa activa. Intentando reconectar...');

            const badge = elemento(SELECTORES.estado);
            if (badge) {
                badge.className = 'badge rounded-pill text-bg-warning';
            }

            const limiteSuperado = espera.config.fechaLimiteMs &&
                Date.now() > espera.config.fechaLimiteMs + espera.config.margenErrorMs;

            if (limiteSuperado) {
                await finalizar(ESTADOS_TERMINALES.ERROR, { mensaje: error.message });
                return;
            }

            const demora = Math.min(
                espera.config.intervaloMs * Math.max(1, espera.erroresConsecutivos),
                10000);
            programarConsulta(espera, demora);
        } finally {
            espera.consultando = false;
            espera.abortController = null;
        }
    }

    /**
     * Inicia una espera bloqueante y devuelve una promesa con el resultado terminal.
     * El callback onAprobada puede ser asíncrono: el modal permanecerá abierto hasta
     * que la aplicación del cambio de negocio finalice correctamente.
     */
    function esperar(configuracion) {
        let config;
        try {
            config = normalizarConfiguracion(configuracion);
        } catch (error) {
            return Promise.reject(error);
        }

        if (esperaActiva) {
            const esMismaSolicitud = esperaActiva.config.idSolicitud === config.idSolicitud &&
                esperaActiva.config.claveOperacion === config.claveOperacion;

            return esMismaSolicitud
                ? esperaActiva.promise
                : Promise.reject(new Error('Ya existe una autorización remota en espera.'));
        }

        const modal = elemento(SELECTORES.modal);
        prepararModal(config);
        const focoAnterior = document.activeElement;

        let resolvePromise;
        const promise = new Promise(function (resolve) {
            resolvePromise = resolve;
        });

        esperaActiva = {
            config,
            modal,
            modalBootstrap: abrirModal(modal),
            focoAnterior,
            promise,
            resolve: resolvePromise,
            finalizada: false,
            consultando: false,
            erroresConsecutivos: 0,
            timerConsulta: null,
            timerCuentaRegresiva: null,
            abortController: null
        };

        actualizarCuentaRegresiva();
        esperaActiva.timerCuentaRegresiva = window.setInterval(actualizarCuentaRegresiva, 1000);
        programarConsulta(esperaActiva, 0);

        return promise;
    }

    window.AutorizacionRemota = Object.freeze({
        esperar,
        estaEsperando: function () { return esperaActiva !== null; },
        estados: ESTADOS_TERMINALES
    });
})(window, document);
