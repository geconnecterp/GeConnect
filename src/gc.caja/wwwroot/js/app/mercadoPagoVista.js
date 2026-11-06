(function (global) {
    'use strict';
    const dinero = n => '$ ' + Number(n || 0).toLocaleString('en-US', { minimumFractionDigits: 2, maximumFractionDigits: 2 });
    function transporteHttp(root) {
        async function pedir(url, opciones, tiempo) {
            const abort = new AbortController(), timer = setTimeout(() => abort.abort(), tiempo);
            try {
                const response = await fetch(url, { credentials: 'same-origin', ...opciones, signal: abort.signal });
                const r = await response.json();
                if (!response.ok && r.ok === true) throw new Error('Respuesta de pago no válida. Verifique el resultado.');
                return r;
            } finally { clearTimeout(timer); }
        }
        return {
            opciones: solicitud => {
                const url = new URL(root.dataset.opcionesUrl, global.location.href);
                url.searchParams.set('insId', solicitud.ins_id); url.searchParams.set('coTipo', solicitud.co_tipo);
                return pedir(url, { headers: { Accept: 'application/json' } }, 15000);
            },
            procesar: solicitud => pedir(root.dataset.procesarUrl, {
                method: 'POST', headers: { Accept: 'application/json', 'Content-Type': 'application/json', RequestVerificationToken: root.querySelector('[name="__RequestVerificationToken"]')?.value || '' }, body: JSON.stringify(solicitud)
            }, 690000)
        };
    }
    function crearVista(root, transporte, alAprobar) {
        const q = id => root.querySelector('#' + id);
        const valor = (id, v) => { if (q(id).value !== String(v)) q(id).value = v; };
        let claves = '';
        const sesion = new global.MercadoPagoModelo.Sesion(transporte, s => {
            const bloqueado = !s.puedeEditar();
            ['txtMontoMercadoPago','cmbCuotaMercadoPago','txtPaymentIdMercadoPago'].forEach(id => q(id).disabled = bloqueado);
            ['btnVolverMercadoPago','btnCerrarMercadoPago'].forEach(id => q(id).disabled = !s.puedeCerrar());
            q('formDetalleMercadoPago').setAttribute('aria-busy', String(s.estado === 'ESPERANDO' || s.estado === 'CARGANDO'));
            q('lblInstrumentoMercadoPago').textContent = s.contexto?.instrumento?.ins_desc || 'MercadoPago';
            q('lblSaldoMercadoPago').textContent = dinero(s.contexto?.saldo);
            valor('txtMontoMercadoPago', s.base || ''); valor('txtPaymentIdMercadoPago', s.paymentid || '');
            const varias = s.opciones.find(o => o.clave === s.plan)?.cuota > 1;
            root.querySelectorAll('[name="tipoMercadoPago"]').forEach(radio => { radio.checked = Number(radio.value) === s.tipo; radio.disabled = bloqueado || varias && Number(radio.value) !== 2; });
            q('ayudaTipoMercadoPago').textContent = varias ? 'Con más de una cuota, el pago debe ser con tarjeta de Crédito.' : 'En una cuota puede elegir Débito, Crédito o QR en el Point.';
            const firma = JSON.stringify([s.estado === 'CARGANDO', s.opciones]);
            if (firma !== claves) {
                claves = firma; q('cmbCuotaMercadoPago').replaceChildren();
                if (!s.opciones.length) {
                    const o = root.ownerDocument.createElement('option'); o.value = ''; o.textContent = s.estado === 'CARGANDO' ? 'Cargando opciones…' : 'Sin cuotas disponibles'; q('cmbCuotaMercadoPago').appendChild(o);
                }
                s.opciones.forEach(o => { const elemento = root.ownerDocument.createElement('option'); elemento.value = o.clave; elemento.textContent = `${o.pos_desc} (${o.recargo_porcentaje} %)`; q('cmbCuotaMercadoPago').appendChild(elemento); });
            }
            valor('cmbCuotaMercadoPago', s.plan || '');
            let valida = false;
            try { const c = s.cotizacion(); valor('txtRecargoMercadoPago', dinero(c.recargo)); valor('txtTotalMercadoPago', dinero(c.total)); valida = true; }
            catch (_) { valor('txtRecargoMercadoPago', '$ 0.00'); valor('txtTotalMercadoPago', '$ 0.00'); }
            q('btnPagarMercadoPago').disabled = bloqueado || !valida;
            if (s.estado === 'INCIERTO') q('txtPaymentIdMercadoPago').disabled = false;
            let idValido = false; try { global.MercadoPagoModelo.normalizarId(s.paymentid); idValido = true; } catch (_) { }
            // Tres caracteres nunca pueden alcanzar la llamada al SP entregado.
            q('btnManualMercadoPago').disabled = (bloqueado && s.estado !== 'INCIERTO') || !valida || !idValido;
            q('estadoMercadoPago').textContent = s.mensaje || 'Seleccione monto y cuotas.';
            q('estadoMercadoPago').className = 'alert mt-3 mb-0 ' + (s.estado === 'INCIERTO' ? 'alert-warning' : s.estado === 'APROBADO' ? 'alert-success' : ['RECHAZADO','BLOQUEADO'].includes(s.estado) ? 'alert-danger' : 'alert-light border');
            if (s.paymentid?.length === 3 && s.puedeEditar()) q('estadoMercadoPago').textContent = 'Por ahora, la recuperación requiere un ID de al menos 4 caracteres. Verifique el cupón.';
        }, async resultado => {
            const aplicado = await alAprobar(resultado);
            if (aplicado === true) { setTimeout(() => global.jQuery(root).modal('hide'), 0); }
            return aplicado;
        });
        q('formDetalleMercadoPago').addEventListener('submit', e => e.preventDefault());
        q('txtMontoMercadoPago').addEventListener('input', e => sesion.actualizar({ base: e.target.value }));
        q('cmbCuotaMercadoPago').addEventListener('change', e => sesion.actualizar({ plan: e.target.value }));
        root.querySelectorAll('[name="tipoMercadoPago"]').forEach(radio => radio.addEventListener('change', e => sesion.actualizar({ tipo: Number(e.target.value) })));
        q('txtPaymentIdMercadoPago').addEventListener('input', e => sesion.actualizar({ paymentid: e.target.value }));
        q('btnPagarMercadoPago').addEventListener('click', () => sesion.enviar('POS'));
        q('btnManualMercadoPago').addEventListener('click', () => sesion.enviar('MANUAL'));
        const cerrar = () => { if (sesion.cerrar()) global.jQuery(root).modal('hide'); };
        q('btnCerrarMercadoPago').addEventListener('click', cerrar); q('btnVolverMercadoPago').addEventListener('click', cerrar);
        global.jQuery(root).on('hide.bs.modal', e => { if (!sesion.puedeCerrar()) e.preventDefault(); });
        const orden = ['txtMontoMercadoPago','cmbCuotaMercadoPago','tipoCreditoMercadoPago','txtPaymentIdMercadoPago','btnManualMercadoPago'];
        orden.slice(0,-1).forEach((id, i) => q(id).addEventListener('keydown', e => {
            if (e.key === 'Enter') { e.preventDefault(); const siguiente = q(orden[i+1]); if (!siguiente.disabled) { siguiente.focus(); siguiente.select?.(); } }
        }));
        return {
            sesion,
            abrir: contexto => { claves = ''; global.jQuery(root).modal('show'); const tarea = sesion.abrir(contexto); return tarea.then(r => { if (sesion.puedeEditar()) { q('txtMontoMercadoPago').focus(); q('txtMontoMercadoPago').select(); } return r; }); }
        };
    }
    let vista;
    function abrir(contexto) {
        const root = global.document.getElementById('modalDetalleMercadoPago');
        if (!root) throw new Error('La ventana MercadoPago no está disponible. Recargue la página.');
        if (!vista) vista = crearVista(root, transporteHttp(root), resultado => {
            if (typeof vista.sesion.contexto?.alAprobar !== 'function') throw new Error('No está disponible el registro del pago aprobado. Conserve el ID.');
            return vista.sesion.contexto.alAprobar(resultado);
        });
        return vista.abrir(contexto);
    }
    function bloqueado() { return !!vista && ['ESPERANDO','APROBADO','INCIERTO'].includes(vista.sesion.estado); }
    global.MercadoPagoPoint = Object.freeze({ abrir, bloqueado, esInstrumento: global.MercadoPagoModelo.esInstrumento, crearVista });
})(window);
