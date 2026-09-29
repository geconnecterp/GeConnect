(function (root) {
    'use strict';
    function validar(porFecha, desde, hasta, hoy, minHasta) {
        if (porFecha) {
            function fecha(s) {
                if (!/^\d{4}-\d{2}-\d{2}$/.test(s)) return NaN;
                const n = Date.parse(s + 'T00:00:00Z');
                return Number.isFinite(n) && new Date(n).toISOString().slice(0, 10) === s ? n : NaN;
            }
            const d = fecha(desde), h = fecha(hasta);
            if (!Number.isFinite(d) || !Number.isFinite(h)) return 'Ingrese fechas válidas en Desde y Hasta.';
            if (d > h) return 'Desde no puede ser posterior a Hasta.';
            if ((h - d) / 86400000 > 7) return 'El intervalo no puede superar 7 días.';
            if (hasta > hoy) return 'Hasta no puede ser posterior a hoy.';
            if (hasta < minHasta) return 'Hasta no puede ser anterior a cinco años atrás.';
        } else {
            if (!/^[0-9]{1,5}$/.test(desde) || !/^[0-9]{1,5}$/.test(hasta) || +desde < 1 || +hasta < 1) return 'Ingrese números enteros entre 1 y 99999.';
            if (+desde > +hasta) return 'Desde no puede ser mayor que Hasta.';
            if (+hasta - +desde > 5) return 'La diferencia entre Hasta y Desde no puede superar 5.';
        }
        return '';
    }
    if (typeof module !== 'undefined' && module.exports) module.exports = { validar };
    if (!root.document) return;
    root.document.addEventListener('DOMContentLoaded', function () {
        const form = document.getElementById('rzForm');
        if (!form) return;
        const campos = document.getElementById('rzCampos'), desde = document.getElementById('rzDesde'), hasta = document.getElementById('rzHasta');
        const enviar = document.getElementById('rzImprimir'), nueva = document.getElementById('rzNueva'), volver = document.getElementById('rzVolver');
        const estado = document.getElementById('rzEstado'), ayuda = document.getElementById('rzAyuda');
        let ocupado = false, terminado = false, incierto = false;
        const disponible = form.dataset.habilitado === 'true';
        const esFecha = () => form.querySelector('input[name="rzModo"]:checked').value === 'fecha';
        const aviso = (texto, tipo) => { estado.textContent = texto; estado.className = 'rz-status alert alert-' + tipo; };
        const htmlSeguro = texto => { const e = document.createElement('div'); e.textContent = texto; return e.innerHTML; };
        function bloquear(valor) {
            ocupado = valor;
            form.setAttribute('aria-busy', String(valor));
            campos.disabled = valor || terminado || !disponible;
            enviar.disabled = valor || terminado || !disponible;
            volver.setAttribute('aria-disabled', String(valor));
            volver.tabIndex = valor ? -1 : 0;
            enviar.innerHTML = valor ? '<i class="bx bx-loader-alt bx-spin"></i> Solicitando…' : '<i class="bx bx-printer"></i> Imprimir Z';
        }
        function preparar() {
            terminado = false; incierto = false; nueva.hidden = true; estado.textContent = '';
            bloquear(false);
            const fecha = esFecha();
            [desde, hasta].forEach(input => {
                input.type = fecha ? 'date' : 'text';
                input.classList.toggle('jsteclado', !fecha);
                input.setAttribute('inputmode', fecha ? 'none' : 'numeric');
                input.value = fecha ? form.dataset.hoy : '';
                input.removeAttribute('min'); input.removeAttribute('max');
                if (fecha) { input.max = form.dataset.hoy; input.removeAttribute('maxlength'); }
                else input.maxLength = 5;
            });
            if (fecha) hasta.min = form.dataset.minHasta;
            ayuda.textContent = fecha ? 'Intervalo máximo: 7 días. Hasta debe estar entre hoy y cinco años atrás.' :
                'La diferencia entre Hasta y Desde puede ser de hasta 5. Ejemplo: 5000 a 5005.';
            desde.focus();
        }
        form.querySelectorAll('input[name="rzModo"]').forEach(radio => radio.addEventListener('change', preparar));
        desde.addEventListener('keydown', e => { if (e.key === 'Enter') { e.preventDefault(); hasta.focus(); if (!esFecha()) hasta.select(); } });
        [desde, hasta].forEach(input => input.addEventListener('focus', () => { if (!esFecha()) input.select(); }));
        volver.addEventListener('click', e => { if (ocupado) e.preventDefault(); });
        root.addEventListener('beforeunload', e => { if (ocupado) { e.preventDefault(); e.returnValue = ''; } });
        nueva.addEventListener('click', function () {
            if (ocupado) return;
            if (!incierto) { preparar(); return; }
            AbrirMensaje('Verificar controlador', 'El resultado anterior es incierto. ¿Ya verificó el controlador antes de preparar otra solicitud?',
                r => { $('#msjModal').modal('hide'); if (r === 'SI') preparar(); }, true, ['Sí, ya verifiqué', 'Volver'], 'warn!', null);
        });
        form.addEventListener('submit', function (e) {
            e.preventDefault();
            if (ocupado || terminado || !disponible) return;
            const error = validar(esFecha(), desde.value, hasta.value, form.dataset.hoy, form.dataset.minHasta);
            if (error) { aviso(error, 'warning'); return; }
            const datos = { PorFecha: esFecha(), Desde: desde.value, Hasta: hasta.value };
            bloquear(true); // Incluye el tiempo que está abierta la confirmación.
            const detalle = '¿Desea reimprimir los reportes Z ' + (datos.PorFecha ? 'por fecha' : 'por número de cierre') +
                ', desde ' + datos.Desde + ' hasta ' + datos.Hasta + '?';
            let decisionTomada = false;
            AbrirMensaje('Confirmar reimpresión Z', htmlSeguro(detalle), function (r) {
                if (decisionTomada) return;
                decisionTomada = true;
                $('#msjModal').modal('hide');
                if (r !== 'SI') { bloquear(false); return; }
                aviso('Enviando solicitud al controlador fiscal. Espere el resultado.', 'info');
                $.ajax({ url: form.dataset.url, method: 'POST', contentType: 'application/json', dataType: 'json',
                    headers: { RequestVerificationToken: form.querySelector('input[name="__RequestVerificationToken"]').value },
                    data: JSON.stringify(datos), timeout: 120000
                }).done(function (r) {
                    terminado = r.ok === true || r.incierto === true; incierto = r.incierto === true;
                    const mensaje = r.mensaje || 'No se recibió un resultado válido. Verifique el controlador.';
                    if (typeof r.ok !== 'boolean') { terminado = true; incierto = true; }
                    aviso(mensaje, incierto ? 'warning' : r.ok ? 'success' : 'danger');
                    AbrirMensaje(r.ok ? 'Solicitud de Reimpresión Z' : 'Reimpresión Z', htmlSeguro(mensaje),
                        () => $('#msjModal').modal('hide'), false, ['Aceptar'], incierto ? 'warn!' : r.ok ? 'succ!' : 'error!', null);
                }).fail(function (xhr) {
                    terminado = true; incierto = true;
                    aviso(xhr.status === 401 ? 'La sesión expiró. Ingrese nuevamente antes de continuar.' :
                        'No se pudo determinar el resultado. Revise el controlador antes de volver a solicitar la impresión.', 'warning');
                }).always(function () { nueva.hidden = !terminado; bloquear(false); });
            }, true, ['Imprimir Z', 'Cancelar'], 'warn!', null);
        });
    });
})(typeof window !== 'undefined' ? window : globalThis);
