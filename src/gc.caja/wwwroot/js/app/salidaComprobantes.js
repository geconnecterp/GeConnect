/* Salida común de PDF. Registrar la operación e imprimir son acciones independientes. */
(function (global) {
    'use strict';
    const elemento = document.getElementById('gcSalidaConfiguracion');
    let config = {};
    try { config = elemento ? JSON.parse(elemento.textContent) : {}; } catch (_) { /* Sin configuración no se hacen solicitudes automáticas. */ }
    const tipos = new Set(['FE', 'CUSTODIA', 'RECIBO_CD', 'RECIBO_CC']);
    const activos = new Map(), blobs = new Map();
    const almacen = 'gc.caja.impresion.v1:' + (config.ambito || 'sin-sesion');
    let cola = Promise.resolve(), ultimo = null;
    const normalizarModo = valor => String(valor || '').trim().toUpperCase() === 'IMPRESORA' ? 'IMPRESORA' : 'PANTALLA';
    // Sin preferencia explícita, los comprobantes se envían a la impresora de Windows.
    const resolverModo = respaldo => normalizarModo(config.modo || respaldo || 'IMPRESORA');
    function documento(valor) {
        if (!valor || !tipos.has(valor.tipo)) throw new Error('Tipo de comprobante no admitido.');
        const d = { tipo: valor.tipo };
        for (const nombre of ['tco_letra', 'tco_id', 'cm_compte', 'cm_repetido', 'entrega_id', 'rb_compte']) {
            if (valor[nombre] !== undefined && valor[nombre] !== null) d[nombre] = String(valor[nombre]);
        }
        if (d.tipo === 'FE' && (!d.tco_id || !d.cm_compte)) throw new Error('Falta identificar el comprobante emitido.');
        if (d.tipo === 'CUSTODIA' && !d.entrega_id) throw new Error('Falta identificar la entrega registrada.');
        if (d.tipo.startsWith('RECIBO_') && !d.rb_compte) throw new Error('El servidor no informó el número de recibo.');
        if (Object.values(d).some(v => v.length > 80 || /[\r\n]/.test(v))) throw new Error('La identificación del comprobante no es válida.');
        return d;
    }
    const clave = d => JSON.stringify([d.tipo, d.tco_id || '', d.cm_compte || '', d.cm_repetido || '0', d.entrega_id || '', d.rb_compte || '']);
    const titulo = d => d.tipo === 'CUSTODIA' ? 'Acuse ' + d.entrega_id : d.tipo.startsWith('RECIBO_') ? 'Recibo ' + d.rb_compte : 'Comprobante ' + d.cm_compte;
    function leer() {
        try {
            const filas = JSON.parse(sessionStorage.getItem(almacen) || '[]');
            return Array.isArray(filas) ? filas.filter(x => x && x.documento && tipos.has(x.documento.tipo) && Date.now() - x.fecha < 86400000).slice(-10) : [];
        } catch (_) { return []; }
    }
    function guardar(trabajo) {
        try { sessionStorage.setItem(almacen, JSON.stringify([...leer().filter(x => x.clave !== trabajo.clave), trabajo].slice(-10))); }
        catch (_) { console.warn('[IMPRESION] No se pudo conservar la referencia del comprobante en esta pestaña.'); }
    }
    function registrar(trabajo) {
        console.info('[IMPRESION]', JSON.stringify(trabajo, null, 2));
        if (!config.registro || !config.token) return;
        fetch(config.registro, { method: 'POST', credentials: 'same-origin', keepalive: true,
            headers: { 'Content-Type': 'application/json', RequestVerificationToken: config.token },
            body: JSON.stringify({ solicitud: trabajo.solicitud, documento: trabajo.documento, modo: trabajo.modo,
                estado: trabajo.estado, mensaje: trabajo.mensaje || '' }) }).catch(() => console.warn('[IMPRESION] Diagnóstico no disponible.'));
    }
    function estado(trabajo, valor, mensaje) {
        trabajo.estado = valor; trabajo.mensaje = mensaje; trabajo.fecha = Date.now();
        guardar(trabajo); registrar(trabajo); pintar(trabajo);
    }
    function pintar(trabajo) {
        const panel = document.getElementById('gcSalidaEstado');
        if (!panel) return;
        ultimo = trabajo; panel.hidden = false;
        panel.dataset.estado = trabajo.estado;
        panel.querySelector('[data-salida-titulo]').textContent = titulo(trabajo.documento);
        panel.querySelector('[data-salida-mensaje]').textContent = trabajo.mensaje || 'La operación conserva su registro.';
        const ocupada = activos.has(trabajo.clave);
        for (const boton of panel.querySelectorAll('button[data-salida-accion]')) boton.disabled = ocupada;
    }
    async function obtenerPdf(d) {
        let response;
        if (d.tipo === 'CUSTODIA') {
            if (!config.acuse) throw new Error('No se configuró la consulta del acuse.');
            response = await fetch(config.acuse + '?entrega=' + encodeURIComponent(d.entrega_id), { credentials: 'same-origin', cache: 'no-store', signal: AbortSignal.timeout(60000), headers: { Accept: 'application/pdf' } });
        } else {
            const esRecibo = d.tipo.startsWith('RECIBO_');
            const url = esRecibo ? config.recibo : config.factura;
            if (!url) throw new Error('No se configuró la consulta del comprobante.');
            response = await fetch(url, { method: 'POST', credentials: 'same-origin', cache: 'no-store', signal: AbortSignal.timeout(60000),
                headers: { 'Content-Type': 'application/json', RequestVerificationToken: config.token || '', 'X-Requested-With': 'XMLHttpRequest' }, body: JSON.stringify(d) });
        }
        if (response.status === 401 || response.status === 403) throw new Error('La sesión expiró. Ingrese nuevamente para recuperar el comprobante.');
        const contenido = response.headers.get('content-type') || '';
        if (response.ok && contenido.includes('application/pdf')) return response.blob();
        const resultado = contenido.includes('application/json') ? await response.json() : null;
        if (!response.ok || !resultado || resultado.resultado !== 0 || !resultado.base64)
            throw new Error(resultado?.mensaje || resultado?.resultado_msj || 'No se pudo obtener el PDF. La operación conserva su registro.');
        return new Blob([Uint8Array.from(atob(resultado.base64), c => c.charCodeAt(0))], { type: 'application/pdf' });
    }
    async function validarPdf(blob) {
        if (!(blob instanceof Blob) || blob.size < 5 || !String(blob.type).includes('application/pdf') || await blob.slice(0, 5).text() !== '%PDF-')
            throw new Error('El servidor no devolvió un PDF válido. La operación conserva su registro.');
    }
    async function enviarPdf(blob, opciones) {
        await validarPdf(blob);
        const modo = normalizarModo(opciones.modo), url = URL.createObjectURL(blob);
        if (modo === 'PANTALLA') {
            if (opciones.contenedor) {
                opciones.contenedor.src = url; opciones.contenedor.hidden = false;
            } else {
                const ventana = window.open(url, '_blank');
                if (!ventana) { URL.revokeObjectURL(url); throw new Error('El navegador bloqueó la vista del PDF. Permita las ventanas de Caja y vuelva a abrir el comprobante.'); }
                try { ventana.opener = null; } catch (_) { /* Visor PDF aislado. */ }
            }
            // El visor conserva el recurso; se libera al abandonar esta página.
            return { ok: true, estado: 'EN_PANTALLA', mensaje: 'Comprobante disponible en pantalla.' };
        }
        const version = navigator.userAgent.match(/(?:Chrome|Edg)\/(\d+)/);
        if (!version || Number(version[1]) < 144) {
            URL.revokeObjectURL(url);
            throw new Error('La impresión directa requiere Chrome o Edge 144 o posterior y las políticas de impresión del puesto.');
        }
        const frame = document.createElement('iframe');
        frame.className = 'gc-salida-impresion'; frame.title = 'Impresión del comprobante'; frame.setAttribute('aria-hidden', 'true');
        return new Promise((resolve, reject) => {
            let solicitada = false, iniciando = false, liberada = false, limpieza;
            const foco = document.activeElement;
            const restaurarFoco = () => {
                if (document.activeElement === frame && foco?.isConnected && typeof foco.focus === 'function') foco.focus();
            };
            const limpiar = () => {
                if (liberada) return; liberada = true;
                clearTimeout(carga); clearTimeout(limpieza);
                restaurarFoco(); frame.remove(); URL.revokeObjectURL(url);
            };
            const fallo = error => { if (solicitada) return; limpiar(); reject(error); };
            // El visor PDF de Chrome no siempre notifica afterprint. No es un acuse de la impresora.
            const carga = setTimeout(() => fallo(new Error('No se pudo cargar el PDF para imprimir. Verifique el comprobante antes de reintentar.')), 30000);
            frame.onerror = () => fallo(new Error('No se pudo cargar el comprobante para imprimir.'));
            frame.onload = () => {
                if (solicitada || iniciando || liberada) return;
                iniciando = true;
                try {
                    frame.contentWindow.focus();
                    frame.contentWindow.print();
                    solicitada = true; clearTimeout(carga); restaurarFoco();
                    // Retener el visor permite terminar de preparar el trabajo sin una pestaña visible.
                    limpieza = setTimeout(limpiar, 60000);
                    resolve({ ok: true, estado: 'IMPRESION_SOLICITADA', mensaje: 'Impresión solicitada a la predeterminada de Windows. Verifique la salida en la impresora.' });
                } catch (error) { fallo(error); }
            };
            frame.src = url;
            document.body.append(frame);
        });
    }

    function solicitar(valor, opciones = {}) {
        let d;
        try { d = documento(valor); } catch (error) { return Promise.reject(error); }
        const id = clave(d);
        if (activos.has(id)) return activos.get(id);
        const anterior = leer().find(x => x.clave === id);
        if (!opciones.reimprimir && anterior) {
            if (anterior.estado === 'IMPRESION_SOLICITADA' || anterior.estado === 'EN_PANTALLA') { pintar(anterior); return Promise.resolve({ ok: true, estado: anterior.estado, repetida: true }); }
            anterior.mensaje = 'Hay una solicitud anterior. Verifique la cola de Windows y use Reimprimir si corresponde.';
            pintar(anterior); return Promise.resolve({ ok: false, estado: 'VERIFICAR' });
        }
        const trabajo = { clave: id, documento: d, solicitud: crypto.randomUUID(), fecha: Date.now(), modo: opciones.modoForzado ? normalizarModo(opciones.modoForzado) : resolverModo(opciones.modo), estado: 'PREPARANDO' };
        const ejecutar = async () => {
            estado(trabajo, 'PREPARANDO', 'Obteniendo comprobante. La operación ya está registrada.');
            try {
                const blob = blobs.get(id) || await obtenerPdf(d);
                await validarPdf(blob); blobs.set(id, blob);
                estado(trabajo, 'SOLICITANDO', trabajo.modo === 'IMPRESORA' ? 'Solicitando impresión…' : 'Abriendo comprobante…');
                const resultado = await enviarPdf(blob, { ...opciones, modo: trabajo.modo });
                estado(trabajo, resultado.estado, resultado.mensaje); return resultado;
            } catch (error) {
                estado(trabajo, 'VERIFICAR', error.message || 'No se pudo presentar o imprimir el comprobante. La operación conserva su registro.');
                return { ok: false, estado: trabajo.estado, mensaje: trabajo.mensaje };
            } finally { activos.delete(id); if (ultimo?.clave === id) pintar(trabajo); }
        };
        const promesa = cola.then(ejecutar, ejecutar);
        activos.set(id, promesa); cola = promesa.catch(() => {}); return promesa;
    }
    const panel = document.getElementById('gcSalidaEstado');
    if (panel) {
        panel.querySelector('[data-salida-accion="ver"]').addEventListener('click', () => ultimo && solicitar(ultimo.documento, { reimprimir: true, modoForzado: 'PANTALLA' }));
        panel.querySelector('[data-salida-accion="reimprimir"]').addEventListener('click', () => ultimo && solicitar(ultimo.documento, { reimprimir: true, modoForzado: 'IMPRESORA' }));
        panel.querySelector('[data-salida-cerrar]').addEventListener('click', () => { panel.hidden = true; });
        const pendiente = leer().reverse().find(x => !['IMPRESION_SOLICITADA', 'EN_PANTALLA'].includes(x.estado));
        if (pendiente) { pendiente.mensaje = 'El comprobante está registrado. Verifique la impresión anterior antes de reimprimir.'; pintar(pendiente); }
    }
    global.SalidaComprobantes = Object.freeze({ solicitar, resolverModo, enviarPdf, urlConfiguracionReportes: config.reportes,
        hayRecibo: tipo => tipo === 'RECIBO_CC' ? config.reciboCC === true : config.reciboCD === true });
})(window);
