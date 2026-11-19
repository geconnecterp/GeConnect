/* Activar en un input con js-dictado. data-dictado-etiqueta personaliza el botón
   y data-dictado-idioma permite cambiar el idioma predeterminado es-AR. */
(function (global) {
    'use strict';
    const selector = 'input.js-dictado';
    const controles = new WeakMap();
    const Reconocimiento = global.SpeechRecognition || global.webkitSpeechRecognition;
    const compatible = Boolean(global.isSecureContext && Reconocimiento);
    let activo = null;
    let secuencia = 0;

    function editable(campo) {
        return campo.isConnected && campo.matches(selector) && !campo.matches(':disabled') && !campo.readOnly &&
            ['text', 'search'].includes(campo.type) && campo.getClientRects().length > 0;
    }

    function mensaje(control, texto, estado = 'info') {
        control.estado.textContent = texto;
        control.estado.hidden = !texto;
        control.estado.dataset.estado = estado;
    }

    function sincronizar(control) {
        control.boton.disabled = !compatible || control.campo.matches(':disabled') || control.campo.readOnly;
    }

    function terminar(sesion, texto, estado = 'info') {
        if (activo !== sesion) return;
        activo = null;
        clearTimeout(sesion.tiempo);
        if (sesion.reconocimiento) {
            sesion.reconocimiento.onresult = null;
            sesion.reconocimiento.onerror = null;
            sesion.reconocimiento.onend = null;
            try { sesion.reconocimiento.abort(); } catch (_) { /* Puede estar preparando el idioma. */ }
        }
        const control = sesion.control;
        control.boton.classList.remove('gc-dictado-activo');
        control.boton.setAttribute('aria-pressed', 'false');
        control.boton.setAttribute('aria-label', control.rotulo);
        control.boton.title = control.rotulo;
        control.icono.className = 'bx bx-microphone';
        mensaje(control, texto, estado);
        sincronizar(control);
    }

    function enfocar(control) {
        if (editable(control.campo) &&
            [control.boton, control.campo, document.body].includes(document.activeElement)) {
            control.campo.focus({ preventScroll: true });
        }
    }

    async function iniciar(control) {
        if (activo) {
            const mismo = activo.control === control;
            terminar(activo, 'Dictado cancelado.');
            if (mismo) { enfocar(control); return; }
        }
        if (!compatible || !editable(control.campo)) return;

        const sesion = { control, valorInicial: control.campo.value, reconocimiento: null, tiempo: null };
        activo = sesion;
        control.boton.classList.add('gc-dictado-activo');
        control.boton.setAttribute('aria-pressed', 'true');
        control.boton.setAttribute('aria-label', 'Cancelar dictado');
        control.boton.title = 'Cancelar dictado';
        control.icono.className = 'bx bx-stop-circle';
        mensaje(control, 'Preparando micrófono…');
        sesion.tiempo = setTimeout(() => terminar(sesion, 'Se agotó el tiempo para iniciar el dictado.'), 30000);

        try {
            const reconocimiento = new Reconocimiento();
            sesion.reconocimiento = reconocimiento;
            const idioma = control.campo.dataset.dictadoIdioma || 'es-AR';
            const idiomaLocal = idioma.startsWith('es') ? 'es-ES' : idioma;
            let local = false;
            if ('processLocally' in reconocimiento && typeof Reconocimiento.available === 'function') {
                try {
                    local = await Reconocimiento.available({ langs: [idiomaLocal], processLocally: true }) === 'available';
                } catch (_) { /* El servicio del navegador está autorizado como alternativa. */ }
            }
            if (activo !== sesion) return;
            if (!editable(control.campo) || control.campo.value !== sesion.valorInicial) {
                terminar(sesion, 'Dictado detenido.');
                return;
            }
            reconocimiento.lang = local ? idiomaLocal : idioma;
            if ('processLocally' in reconocimiento) reconocimiento.processLocally = local;
            reconocimiento.continuous = false;
            reconocimiento.interimResults = false;
            reconocimiento.maxAlternatives = 1;
            reconocimiento.onstart = () => {
                if (activo !== sesion) return;
                clearTimeout(sesion.tiempo);
                mensaje(control, 'Escuchando… Hablá ahora. Pulsá el micrófono para cancelar.');
                sesion.tiempo = setTimeout(() => {
                    terminar(sesion, 'Tiempo de dictado agotado. Volvé a pulsar el micrófono.');
                    enfocar(control);
                }, 15000);
            };
            reconocimiento.onresult = evento => {
                if (activo !== sesion) return;
                if (!editable(control.campo) || control.campo.value !== sesion.valorInicial) {
                    terminar(sesion, 'Dictado detenido.');
                    return;
                }
                const texto = Array.from(evento.results).filter(resultado => resultado.isFinal)
                    .map(resultado => resultado[0]?.transcript || '').join(' ').replace(/\s+/g, ' ').trim();
                if (!texto) return;
                if (control.campo.maxLength >= 0 && texto.length > control.campo.maxLength) {
                    terminar(sesion, `Usá una búsqueda de hasta ${control.campo.maxLength} caracteres.`, 'error');
                    enfocar(control);
                    return;
                }
                terminar(sesion, 'Texto listo. Revisalo antes de buscar.', 'ok');
                control.campo.value = texto;
                control.campo.dispatchEvent(new Event('input', { bubbles: true }));
                enfocar(control);
            };
            reconocimiento.onerror = evento => {
                if (activo !== sesion) return;
                const errores = {
                    'no-speech': 'No se detectó voz. Volvé a pulsar el micrófono.',
                    'audio-capture': 'No se encontró un micrófono disponible. Revisá su conexión.',
                    'not-allowed': 'Permití el micrófono para este sitio en Chrome.',
                    'service-not-allowed': 'El navegador no permite el servicio de dictado en este puesto.',
                    'network': 'No se pudo conectar con el servicio de dictado. Revisá la conexión.',
                    'language-not-supported': 'El idioma del dictado no está disponible en este navegador.',
                    'aborted': 'Dictado cancelado.'
                };
                terminar(sesion, errores[evento.error] || 'No se pudo completar el dictado. Volvé a intentar.', 'error');
                enfocar(control);
            };
            reconocimiento.onend = () => {
                if (activo !== sesion) return;
                terminar(sesion, 'No se recibió texto. Volvé a pulsar el micrófono.');
                enfocar(control);
            };
            reconocimiento.start();
        } catch (_) {
            terminar(sesion, 'No se pudo iniciar el dictado. Volvé a intentar.', 'error');
            enfocar(control);
        }
    }

    function inicializar(campo) {
        if (controles.has(campo) || !['text', 'search'].includes(campo.type)) return;
        const boton = document.createElement('button');
        boton.type = 'button';
        boton.className = 'btn gc-dictado-boton';
        const rotulo = 'Dictar ' + (campo.dataset.dictadoEtiqueta || 'búsqueda');
        boton.setAttribute('aria-label', rotulo);
        boton.setAttribute('aria-pressed', 'false');
        boton.title = compatible ? rotulo : 'Dictado no disponible en este navegador o conexión.';
        const icono = document.createElement('i');
        icono.className = 'bx bx-microphone';
        icono.setAttribute('aria-hidden', 'true');
        boton.append(icono);
        const estado = document.createElement('span');
        estado.className = 'gc-dictado-estado';
        estado.id = 'gcDictadoEstado' + (++secuencia);
        estado.setAttribute('role', 'status');
        estado.setAttribute('aria-live', 'polite');
        estado.setAttribute('aria-atomic', 'true');
        estado.hidden = true;
        if (!campo.id) campo.id = 'gcDictadoCampo' + secuencia;
        boton.setAttribute('aria-controls', campo.id);
        campo.setAttribute('aria-describedby', [campo.getAttribute('aria-describedby'), estado.id].filter(Boolean).join(' '));
        const control = { campo, boton, icono, estado, rotulo };
        controles.set(campo, control);
        let grupo = campo.parentElement;
        if (!grupo.classList.contains('input-group')) {
            grupo = document.createElement('div');
            grupo.className = 'input-group gc-dictado-campo';
            campo.before(grupo);
            grupo.append(campo);
        }
        campo.after(boton);
        grupo.after(estado);
        boton.addEventListener('click', () => iniciar(control));
        sincronizar(control);
    }

    function descubrir(raiz) {
        if (raiz.nodeType !== Node.ELEMENT_NODE && raiz !== document) return;
        if (raiz.matches?.(selector)) inicializar(raiz);
        raiz.querySelectorAll(selector).forEach(inicializar);
    }

    function cancelar() {
        if (activo) terminar(activo, 'Dictado cancelado.');
    }

    function activarComponente() {
        descubrir(document);
        new MutationObserver(cambios => {
            for (const cambio of cambios) {
                cambio.addedNodes.forEach(descubrir);
                if (cambio.type === 'attributes') {
                    if (cambio.target.matches(selector)) inicializar(cambio.target);
                    const control = controles.get(cambio.target);
                    if (control) sincronizar(control);
                }
            }
            if (activo && !editable(activo.control.campo)) cancelar();
        }).observe(document.body, { childList: true, subtree: true, attributes: true, attributeFilter: ['disabled', 'readonly', 'class'] });
        document.addEventListener('input', evento => {
            if (activo && evento.target === activo.control.campo) cancelar();
        });
        document.addEventListener('focusin', evento => {
            if (activo && evento.target !== activo.control.campo && evento.target !== activo.control.boton) cancelar();
        });
        document.addEventListener('click', evento => {
            if (activo && evento.target !== activo.control.campo && !activo.control.boton.contains(evento.target)) cancelar();
        }, true);
        document.addEventListener('keydown', evento => {
            if (activo && evento.key === 'Escape') {
                const control = activo.control;
                evento.preventDefault();
                evento.stopPropagation();
                cancelar();
                enfocar(control);
            }
        }, true);
        document.addEventListener('hide.bs.modal', evento => {
            if (activo && evento.target.contains(activo.control.campo)) cancelar();
        });
        document.addEventListener('show.bs.modal', evento => {
            if (activo && !evento.target.contains(activo.control.campo)) cancelar();
        });
        document.addEventListener('visibilitychange', () => { if (document.hidden) cancelar(); });
        global.addEventListener('pagehide', cancelar);
    }

    if (document.readyState === 'loading') document.addEventListener('DOMContentLoaded', activarComponente, { once: true });
    else activarComponente();
})(window);
