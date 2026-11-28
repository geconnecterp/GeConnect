/* Consulta temporal de sólo lectura. No utiliza el componente de impresión de Caja. */
(function () {
    'use strict';
    const root = document.getElementById('consultaReportes');
    if (!root) return;
    const byId = id => document.getElementById(id);
    const form = byId('consultaReportesForm'), tipo = byId('consultaTipo'), pv = byId('consultaPv'), numero = byId('consultaNumero');
    const repeticion = byId('consultaRepeticion'), seleccion = byId('consultaSeleccion'), resumen = byId('consultaResumen');
    const estado = byId('consultaEstado'), buscar = byId('consultaBuscar'), visores = byId('consultaVisores');
    const botones = Array.from(root.querySelectorAll('[data-consulta-formato]'));
    const token = form.querySelector('input[name="__RequestVerificationToken"]').value;
    const urls = new Map();
    let candidatos = [], secuencia = 0, peticion = null, ocupado = false, tiposListos = false;
    function mensaje(texto, nivel = 'info') { estado.className = 'alert alert-' + nivel + ' mt-3 mb-0'; estado.textContent = texto; }
    function elegido() { return candidatos.find(c => String(c.repeticion) === repeticion.value); }
    function controles(valor) {
        ocupado = valor; buscar.disabled = valor || !tiposListos;
        botones.forEach(b => { b.disabled = valor || !elegido(); });
        repeticion.disabled = valor;
    }
    function cerrarVisores() {
        root.querySelectorAll('[data-consulta-pdf]').forEach(frame => { frame.removeAttribute('src'); frame.hidden = true; });
        urls.forEach(url => URL.revokeObjectURL(url)); urls.clear();
        root.querySelectorAll('[data-consulta-panel]').forEach(panel => { panel.hidden = true; const texto = panel.querySelector('[data-consulta-pdf-estado]'); texto.textContent = ''; texto.classList.remove('text-danger'); });
        visores.hidden = true;
    }
    function invalidar(limpiarSeleccion = true) {
        secuencia++; if (peticion) peticion.abort(); peticion = null; cerrarVisores();
        if (limpiarSeleccion) { candidatos = []; repeticion.replaceChildren(); seleccion.hidden = true; resumen.textContent = ''; }
        controles(false);
    }
    function identificacion() { return { tcoId: tipo.value, puntoVenta: pv.value.trim(), numero: numero.value.trim() }; }
    async function solicitar(url, dato, signal) {
        const opciones = { credentials: 'same-origin', cache: 'no-store', redirect: 'error', signal };
        if (dato) Object.assign(opciones, { method: 'POST', headers: { 'Content-Type': 'application/json', RequestVerificationToken: token }, body: JSON.stringify(dato) });
        const response = await fetch(url, opciones);
        if (response.status === 401 || response.status === 403) throw new Error('La sesión expiró o no tiene acceso. Ingrese nuevamente.');
        if ((response.headers.get('content-type') || '').includes('application/pdf') && response.ok) return response.blob();
        let result;
        try { result = await response.json(); } catch (_) { throw new Error('El servidor no devolvió una respuesta válida. Revise la conexión y la sesión.'); }
        if (!response.ok || result.ok !== true) throw new Error(result.mensaje || 'No fue posible completar la consulta.');
        return result;
    }
    function describir(c) {
        const fecha = c.fecha ? new Date(c.fecha).toLocaleDateString('es-AR') : 'Sin fecha';
        const total = c.total == null ? 'Sin total informado' : Number(c.total).toLocaleString('es-AR', { minimumFractionDigits: 2, maximumFractionDigits: 2 });
        return [c.descripcion, c.numero, 'Rep. ' + c.repeticion, fecha, c.cliente, (c.moneda || '') + ' ' + total].filter(Boolean).join(' · ');
    }
    repeticion.addEventListener('change', () => {
        invalidar(false); const c = elegido(); resumen.textContent = c ? describir(c) : '';
        mensaje(c ? 'Seleccione el formato que desea visualizar.' : 'Seleccione una repetición para continuar.');
    });
    [tipo, pv, numero].forEach(input => input.addEventListener('input', () => { invalidar(); mensaje('Busque el comprobante con la nueva identificación.'); }));
    tipo.addEventListener('change', () => { invalidar(); mensaje('Ingrese punto de venta y número de comprobante.'); });
    byId('consultaLimpiar').addEventListener('click', () => { invalidar(); form.reset(); mensaje('Ingrese los datos de otro comprobante.'); tipo.focus(); });
    form.addEventListener('submit', async event => {
        event.preventDefault(); if (ocupado || !form.reportValidity()) return;
        invalidar(); const actual = secuencia, control = new AbortController(); peticion = control; controles(true);
        mensaje('Buscando el comprobante…');
        try {
            const resultado = await solicitar(root.dataset.buscarUrl, identificacion(), control.signal);
            if (actual !== secuencia) return;
            candidatos = resultado.candidatos || [];
            if (!candidatos.length) throw new Error('No se encontró el comprobante.');
            const partes = resultado.comprobante.split('-'); pv.value = partes[0]; numero.value = partes[1];
            if (candidatos.length > 1) repeticion.append(new Option('Seleccione la repetición correcta…', ''));
            candidatos.forEach(c => repeticion.append(new Option(describir(c), String(c.repeticion))));
            seleccion.hidden = false; resumen.textContent = elegido() ? describir(elegido()) : '';
            mensaje(elegido() ? 'Comprobante encontrado. Elija A4, ticket o compare ambos.' : 'Se encontraron varias repeticiones. Seleccione la correcta.', 'success');
        } catch (error) { if (actual === secuencia && error.name !== 'AbortError') mensaje(error.message, 'danger'); }
        finally { if (actual === secuencia) { peticion = null; controles(false); } }
    });
    botones.forEach(button => button.addEventListener('click', async () => {
        const c = elegido(); if (ocupado || !c) return;
        invalidar(false); const actual = secuencia, control = new AbortController(); peticion = control; controles(true);
        const formatos = button.dataset.consultaFormato === 'AMBOS' ? ['A4', 'TK'] : [button.dataset.consultaFormato];
        const dato = Object.assign(identificacion(), { repeticion: c.repeticion });
        visores.hidden = false; mensaje('Generando la visualización del comprobante…');
        const resultados = await Promise.allSettled(formatos.map(async formato => {
            const panel = root.querySelector('[data-consulta-panel="' + formato + '"]');
            const texto = panel.querySelector('[data-consulta-pdf-estado]'), frame = panel.querySelector('iframe');
            panel.hidden = false; texto.textContent = 'Generando PDF…';
            try {
                const blob = await solicitar(root.dataset.generarUrl, Object.assign({}, dato, { formato }), control.signal);
                if (actual !== secuencia) return false;
                if (!(blob instanceof Blob) || blob.type !== 'application/pdf') throw new Error('El archivo recibido no es un PDF.');
                const url = URL.createObjectURL(blob); urls.set(formato, url); frame.src = url; frame.hidden = false;
                texto.textContent = describir(c); return true;
            } catch (error) {
                if (actual === secuencia && error.name !== 'AbortError') { texto.textContent = error.message; texto.classList.add('text-danger'); }
                return false;
            }
        }));
        if (actual !== secuencia) return;
        const correctos = resultados.filter(r => r.status === 'fulfilled' && r.value === true).length;
        mensaje(correctos === formatos.length ? 'Visualización lista. Puede cambiar de formato o buscar otro comprobante.' : 'Algún reporte no pudo generarse. Revise el mensaje de cada visor.', correctos === formatos.length ? 'success' : 'warning');
        peticion = null; controles(false);
    }));
    window.addEventListener('pagehide', () => invalidar());
    solicitar(root.dataset.tiposUrl).then(resultado => {
        tipo.replaceChildren(new Option('Seleccione un tipo de comprobante…', ''));
        (resultado.tipos || []).forEach(t => tipo.append(new Option(t.id + ' - ' + t.descripcion, t.id)));
        tiposListos = tipo.options.length > 1; tipo.disabled = !tiposListos; controles(false);
        mensaje(tiposListos ? 'Ingrese tipo, punto de venta y número. Se completan los ceros a la izquierda.' : 'No hay tipos A o B disponibles.', tiposListos ? 'info' : 'warning');
    }).catch(error => mensaje(error.message, 'danger'));
})();