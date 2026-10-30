(function () {
    'use strict';
    function normalizar(valor) {
        const texto = String(valor ?? '').trim();
        if (!/^\d{1,15}(?:[.,]\d{0,2})?$/.test(texto)) return null;
        const partes = texto.replace(',', '.').split('.');
        return partes[0].replace(/^0+(?=\d)/, '') + '.' + (partes[1] || '').padEnd(2, '0');
    }
    function centavos(valor) {
        const texto = String(valor);
        const negativo = texto.startsWith('-');
        const limpio = normalizar(negativo ? texto.slice(1) : texto);
        if (limpio === null) throw new Error('Importe inválido.');
        return BigInt(limpio.replace('.', '')) * (negativo ? -1n : 1n);
    }
    function dinero(valor, instrumento) {
        let n = typeof valor === 'bigint' ? valor : centavos(valor);
        const signo = n < 0n ? '-' : ''; if (n < 0n) n = -n;
        const entero = (n / 100n).toString().replace(/\B(?=(\d{3})+(?!\d))/g, '.');
        return (instrumento === 'DOL' ? 'U$S ' : '$ ') + signo + entero + ',' + (n % 100n).toString().padStart(2, '0');
    }
    class SeleccionCustodia {
        constructor() { this.filas = new Map(); this.seleccion = new Map(); }
        cargar(filas, conservar) {
            const claves = conservar ? [...this.seleccion.keys()] : [];
            this.filas = new Map(filas.map(f => [f.clave, Object.freeze({ ...f })])); this.seleccion.clear();
            for (const clave of claves) if (this.filas.has(clave)) this.marcar(clave, true);
        }
        marcar(clave, marcada) {
            const fila = this.filas.get(clave); if (!fila) return;
            if (marcada) { if (!this.seleccion.has(clave)) this.seleccion.set(clave, String(fila.importe_corregido).replace('.', ',')); }
            else this.seleccion.delete(clave);
        }
        editar(clave, texto) { if (this.seleccion.has(clave)) this.seleccion.set(clave, texto); }
        get valida() { return this.seleccion.size > 0 && [...this.seleccion.values()].every(v => normalizar(v) !== null); }
        get modificada() { return [...this.seleccion].some(([k, v]) => normalizar(v) !== normalizar(this.filas.get(k).importe_corregido)); }
        totales() {
            const resultado = { PES: { cantidad: 0, rendido: 0n, corregido: 0n, invalidas: 0 }, DOL: { cantidad: 0, rendido: 0n, corregido: 0n, invalidas: 0 } };
            for (const [clave, importe] of this.seleccion) {
                const fila = this.filas.get(clave), total = resultado[fila.ins_id]; if (!total) continue;
                total.cantidad++; total.rendido += centavos(fila.importe_rendido);
                const normal = normalizar(importe); if (normal !== null) total.corregido += centavos(normal); else total.invalidas++;
            }
            return resultado;
        }
        payload() {
            if (!this.valida) throw new Error('Seleccione rendiciones y revise sus importes.');
            return [...this.seleccion].sort(([a], [b]) => a.localeCompare(b)).map(([clave, importe]) => {
                const fila = this.filas.get(clave);
                return { clave, version: fila.version, ins_id: fila.ins_id, importe: normalizar(importe) };
            });
        }
    }
    const reglas = { normalizar, centavos, dinero, SeleccionCustodia };
    if (typeof module !== 'undefined' && module.exports) module.exports = reglas;
    if (typeof document === 'undefined') return;
    const form = document.getElementById('cuForm'); if (!form) return;
    const $ = id => document.getElementById(id);
    const modelo = new SeleccionCustodia();
    let instrumentos = [], instrumento = '', ocupada = false, consultaValida = false, registrada = false, incierta = false, entregaHabilitada = false;
    let entregaId = '', pdfUrl = '', solicitud = form.dataset.solicitud, dialogoActivo = false, descartando = false, registrando = false;
    function nuevoId() { return globalThis.crypto?.randomUUID?.() || 'xxxxxxxx-xxxx-4xxx-yxxx-xxxxxxxxxxxx'.replace(/[xy]/g, c => { const r = Math.floor(Math.random() * 16); return (c === 'x' ? r : (r & 3 | 8)).toString(16); }); }
    function estado(texto, tipo) { $('cuEstado').textContent = texto; $('cuEstado').dataset.tipo = tipo || ''; }
    function celda(fila, texto, clase) { const td = document.createElement('td'); td.textContent = texto; if (clase) td.className = clase; fila.append(td); return td; }
    function pendientes() { return [...modelo.filas.values()].filter(f => f.ins_id === instrumento && (!registrada || modelo.seleccion.has(f.clave))); }
    function bloquear() {
        const bloqueo = ocupada || registrada || incierta || !consultaValida;
        form.setAttribute('aria-busy', String(ocupada));
        for (const input of form.querySelectorAll('input:not([type=hidden])')) input.disabled = bloqueo;
        for (const button of $('cuInstrumentos').querySelectorAll('button')) button.disabled = ocupada;
        for (const input of $('cuFilas').querySelectorAll('input[data-importe]')) input.disabled = bloqueo || !modelo.seleccion.has(input.dataset.importe);
        $('cuTodas').disabled = bloqueo || pendientes().length === 0;
        $('cuActualizar').disabled = ocupada || registrada;
        $('cuCorregir').disabled = bloqueo || !modelo.valida || !modelo.modificada;
        $('cuEntregar').disabled = bloqueo || !modelo.valida || !entregaHabilitada;
        $('cuEntregar').title = entregaHabilitada ? '' : 'La confirmación de entregas todavía no está disponible.';
        $('cuVolver').setAttribute('aria-disabled', String(registrando));
    }
    function resumen() {
        const totales = modelo.totales();
        for (const card of $('cuInstrumentos').querySelectorAll('button')) {
            const ins = card.dataset.ins, total = totales[ins];
            card.querySelector('.cu-monto').textContent = total.invalidas ? 'Revise el importe' : dinero(total.corregido, ins);
            card.querySelector('small').textContent = total.cantidad + (total.cantidad === 1 ? ' seleccionada' : ' seleccionadas') + ' · Rendido ' + dinero(total.rendido, ins) + (total.cantidad && !total.invalidas ? ' · Diferencia ' + dinero(total.corregido - total.rendido, ins) : '');
            card.setAttribute('aria-pressed', String(ins === instrumento));
        }
        const visibles = pendientes(), marcadas = visibles.filter(f => modelo.seleccion.has(f.clave)).length;
        $('cuRendicionesTitulo').textContent = registrada ? 'Rendiciones de esta entrega' : 'Rendiciones pendientes';
        $('cuResumen').textContent = registrada ? visibles.length + (visibles.length === 1 ? ' rendición' : ' rendiciones') + ' en este instrumento · ' + modelo.seleccion.size + ' en la entrega' : visibles.length + (visibles.length === 1 ? ' pendiente' : ' pendientes') + ' en este instrumento · ' + modelo.seleccion.size + (modelo.seleccion.size === 1 ? ' seleccionada' : ' seleccionadas') + ' en total';
        $('cuTodas').checked = visibles.length > 0 && marcadas === visibles.length;
        $('cuTodas').indeterminate = marcadas > 0 && marcadas < visibles.length;
        bloquear();
    }
    function pintarInstrumentos() {
        $('cuInstrumentos').replaceChildren();
        for (const ins of instrumentos) {
            const button = document.createElement('button'); button.type = 'button'; button.className = 'cu-moneda'; button.dataset.ins = ins.ins_id;
            const nombre = document.createElement('strong'); nombre.textContent = ins.ins_desc || ins.ins_id;
            const monto = document.createElement('span'); monto.className = 'cu-monto';
            const detalle = document.createElement('small'); button.append(nombre, monto, detalle);
            button.addEventListener('click', () => { instrumento = ins.ins_id; pintarFilas(); resumen(); button.focus(); });
            $('cuInstrumentos').append(button);
        }
    }
    function pintarFilas() {
        const body = $('cuFilas'); body.replaceChildren();
        for (const f of pendientes()) {
            const tr = document.createElement('tr'); tr.classList.toggle('cu-seleccionada', modelo.seleccion.has(f.clave));
            const fecha = f.rend_fecha?.slice(0, 10).split('-').reverse().join('/') || '';
            celda(tr, fecha); celda(tr, f.caja_nro_proceso); celda(tr, f.caja_nro_cierre + ' / ' + f.caja_nro_rend);
            celda(tr, f.rend_tipo_desc || (f.rend_tipo === 'P' ? 'Parcial' : 'Final')); celda(tr, f.caja_id); celda(tr, f.usu_apellidoynombre || f.usu_id);
            celda(tr, dinero(f.importe_rendido, f.ins_id), 'text-end');
            const tdImporte = celda(tr, '', 'text-end'), input = document.createElement('input');
            input.type = 'text'; input.inputMode = 'decimal'; input.className = 'form-control'; input.dataset.importe = f.clave;
            input.value = modelo.seleccion.get(f.clave) ?? String(f.importe_corregido).replace('.', ',');
            input.setAttribute('aria-label', 'Importe OK de ' + f.caja_nro_proceso + ', cierre ' + f.caja_nro_cierre + ', rendición ' + f.caja_nro_rend + ', ítem ' + f.rend_item);
            input.title = 'Cero o mayor, hasta dos decimales. Ingrese sin separador de miles.';
            input.addEventListener('focus', () => input.select());
            input.addEventListener('input', () => { modelo.editar(f.clave, input.value); const invalido = normalizar(input.value) === null; input.classList.toggle('is-invalid', invalido); input.setAttribute('aria-invalid', String(invalido)); resumen(); });
            input.addEventListener('blur', () => { const valor = normalizar(input.value); if (valor !== null) input.value = valor.replace('.', ','); });
            const diferencia = document.createElement('small');
            function actualizarDiferencia() {
                const normal = normalizar(input.value);
                if (normal === null) { diferencia.textContent = 'Revise el importe'; diferencia.className = 'text-danger'; return; }
                const valor = centavos(normal) - centavos(f.importe_rendido);
                diferencia.textContent = valor === 0n ? '' : 'Diferencia ' + dinero(valor, f.ins_id);
                diferencia.className = valor < 0n ? 'text-danger' : 'text-success';
            }
            input.addEventListener('input', actualizarDiferencia); actualizarDiferencia();
            tdImporte.append(input, diferencia);
            const tdCheck = celda(tr, '', 'text-center'), check = document.createElement('input'); check.type = 'checkbox'; check.className = 'form-check-input'; check.checked = modelo.seleccion.has(f.clave);
            check.setAttribute('aria-label', 'Entregar ' + f.ins_id + ', proceso ' + f.caja_nro_proceso + ', cierre ' + f.caja_nro_cierre + ', rendición ' + f.caja_nro_rend + ', ítem ' + f.rend_item);
            check.addEventListener('change', () => { modelo.marcar(f.clave, check.checked); tr.classList.toggle('cu-seleccionada', check.checked); input.value = modelo.seleccion.get(f.clave) ?? String(f.importe_corregido).replace('.', ','); input.classList.remove('is-invalid'); input.setAttribute('aria-invalid', 'false'); actualizarDiferencia(); resumen(); });
            tdCheck.append(check); body.append(tr);
        }
        if (!body.children.length) { const tr = document.createElement('tr'), td = celda(tr, consultaValida ? 'No hay rendiciones pendientes para este instrumento.' : 'Las rendiciones todavía no están disponibles.', 'cu-vacio'); td.colSpan = 9; body.append(tr); }
        resumen();
    }
    async function cargar(conservar) {
        ocupada = true; bloquear();
        try {
            const response = await fetch(form.dataset.consulta, { credentials: 'same-origin', cache: 'no-store', headers: { Accept: 'application/json', 'X-Requested-With': 'XMLHttpRequest' } });
            if (response.status === 401 || response.status === 403) throw new Error('La sesión expiró. Ingrese nuevamente para consultar las rendiciones.');
            const result = await response.json();
            if (!response.ok || !result.ok || !Array.isArray(result.data?.instrumentos) || !Array.isArray(result.data?.rendiciones)) throw new Error(result.mensaje || 'No se pudieron consultar las rendiciones.');
            entregaHabilitada = result.data.entrega_habilitada === true;
            instrumentos = result.data.instrumentos; modelo.cargar(result.data.rendiciones, conservar);
            if (!instrumentos.some(i => i.ins_id === instrumento)) instrumento = instrumentos[0]?.ins_id || '';
            consultaValida = true; pintarInstrumentos(); pintarFilas();
            if (!incierta) estado(modelo.filas.size ? 'Seleccione las rendiciones que se entregarán. La selección se conserva al cambiar de moneda.' : 'No hay rendiciones pendientes de entrega en esta sucursal.');
            return true;
        } catch (ex) { consultaValida = false; estado(ex.message || 'No se pudo consultar. Puede actualizar para reintentar.', 'error'); return false; }
        finally { ocupada = false; pintarFilas(); bloquear(); }
    }
    function confirmar(titulo, parrafos) {
        if (dialogoActivo) return Promise.resolve(false);
        dialogoActivo = true;
        return new Promise(resolve => {
            const elemento = $('cuConfirmacion'), modal = bootstrap.Modal.getOrCreateInstance(elemento), boton = $('cuContinuar');
            $('cuConfirmacionTitulo').textContent = titulo; $('cuConfirmacionTexto').replaceChildren();
            for (const texto of parrafos) { const p = document.createElement('p'); p.textContent = texto; if (texto.startsWith('Pesos:') || texto.startsWith('Dólares:')) p.className = 'cu-total-confirmacion'; $('cuConfirmacionTexto').append(p); }
            let aceptada = false;
            const aceptar = () => { aceptada = true; boton.disabled = true; modal.hide(); };
            const cerrado = () => { dialogoActivo = false; boton.removeEventListener('click', aceptar); elemento.removeEventListener('hidden.bs.modal', cerrado); boton.disabled = false; resolve(aceptada); };
            boton.disabled = false; boton.addEventListener('click', aceptar); elemento.addEventListener('hidden.bs.modal', cerrado); modal.show();
        });
    }
    async function acuse() {
        if (!entregaId) return;
        $('cuImprimir').disabled = true; $('cuEstadoAcuse').textContent = 'Obteniendo acuse…';
        try {
            const response = await fetch(form.dataset.acuse + '?entrega=' + encodeURIComponent(entregaId), { credentials: 'same-origin', cache: 'no-store', headers: { Accept: 'application/pdf', 'X-Requested-With': 'XMLHttpRequest' } });
            if (!response.ok || !response.headers.get('content-type')?.includes('application/pdf')) throw new Error('No se pudo obtener el acuse. La entrega está registrada; puede volver a solicitar el PDF sin confirmar otra entrega.');
            const blob = await response.blob(); if (pdfUrl) URL.revokeObjectURL(pdfUrl); pdfUrl = URL.createObjectURL(blob);
            $('cuPdf').src = pdfUrl; $('cuPdf').hidden = false; $('cuAbrirPdf').href = pdfUrl; $('cuAbrirPdf').hidden = false;
            $('cuEstadoAcuse').textContent = 'Acuse disponible. Puede imprimirlo o guardarlo desde el visor PDF.';
        } catch (ex) { $('cuEstadoAcuse').textContent = ex.message; }
        finally { $('cuImprimir').disabled = false; }
    }
    async function ejecutar(entrega) {
        if (ocupada || registrada || incierta || !consultaValida || !modelo.valida || (entrega && !entregaHabilitada)) return;
        const operacion = { solicitud, seleccion: modelo.payload(), responsable_entrega: $('cuResponsable').value.trim(), responsable_custodia: $('cuCustodio').value.trim(), comprobante_entrega: $('cuComprobante').value.trim() };
        if (entrega && (!operacion.responsable_entrega || !operacion.responsable_custodia || !operacion.comprobante_entrega)) { estado('Complete el responsable de la entrega, quién recibe en custodia y el número de comprobante.', 'error'); ['cuCustodio', 'cuResponsable', 'cuComprobante'].map($).find(i => !i.value.trim())?.focus(); return; }
        const textos = [entrega ? '¿Confirma la entrega en custodia de las rendiciones seleccionadas?' : '¿Desea guardar los importes corregidos de las rendiciones seleccionadas?'];
        for (const [ins, total] of Object.entries(modelo.totales())) if (total.cantidad) textos.push((ins === 'PES' ? 'Pesos: ' : 'Dólares: ') + dinero(total.corregido, ins) + ' · ' + total.cantidad + (total.cantidad === 1 ? ' rendición' : ' rendiciones'));
        if (entrega) textos.push('Entrega: ' + operacion.responsable_entrega, 'Recibe: ' + operacion.responsable_custodia, 'Comprobante: ' + operacion.comprobante_entrega);
        estado('Revise el resumen antes de confirmar.');
        ocupada = true; bloquear();
        let aceptada;
        try { aceptada = await confirmar(entrega ? 'Confirmar entrega en custodia' : 'Aceptar correcciones', textos); }
        catch (ex) { dialogoActivo = false; ocupada = false; estado('No se pudo abrir la confirmación. Recargue la página para volver a intentarlo.', 'error'); bloquear(); return; }
        if (!aceptada) { ocupada = false; bloquear(); return; }
        registrando = true; bloquear();
        estado(entrega ? 'Registrando entrega. Espere el resultado…' : 'Guardando correcciones. Espere el resultado…');
        try {
            const response = await fetch(entrega ? form.dataset.entregar : form.dataset.corregir, { method: 'POST', credentials: 'same-origin', headers: { 'Content-Type': 'application/json', 'X-Requested-With': 'XMLHttpRequest', RequestVerificationToken: form.querySelector('[name=__RequestVerificationToken]').value }, body: JSON.stringify(operacion) });
            if (response.status === 401 || response.status === 403) { consultaValida = false; estado('La sesión expiró. Ingrese nuevamente para realizar la operación.', 'error'); return; }
            const result = await response.json();
            if (typeof result.ok !== 'boolean' || !result.mensaje) throw new Error('El servidor no devolvió un resultado explícito.');
            incierta = !!result.incierto;
            if (!result.ok) { estado(result.mensaje, 'error'); return; }
            registrando = false;
            if (entrega) {
                if (!result.entrega_id) throw new Error('No se recibió el número de entrega. Verifique el resultado antes de repetir la operación.');
                registrada = true; entregaId = result.entrega_id; $('cuEntregaId').textContent = 'N.º de entrega: ' + entregaId;
                $('cuAcuse').hidden = false; estado(result.mensaje, 'ok'); pintarFilas(); bloquear(); await acuse();
            } else {
                solicitud = nuevoId();
                if (await cargar(true)) estado(result.mensaje + ' La selección se conserva para preparar la entrega.', 'ok');
            }
        } catch (ex) { incierta = true; estado((ex.message || 'No se pudo confirmar el resultado.') + ' Conserve esta referencia y verifique el estado antes de repetir: ' + solicitud, 'error'); }
        finally { registrando = false; ocupada = false; bloquear(); }
    }
    $('cuTodas').addEventListener('change', () => { for (const fila of pendientes()) modelo.marcar(fila.clave, $('cuTodas').checked); pintarFilas(); });
    $('cuActualizar').addEventListener('click', async () => {
        if (modelo.seleccion.size && !await confirmar('Actualizar rendiciones', ['Se consultará nuevamente la sucursal y se descartará la selección que está preparando. ¿Desea continuar?'])) return;
        await cargar(false);
    });
    $('cuCorregir').addEventListener('click', () => ejecutar(false)); $('cuEntregar').addEventListener('click', () => ejecutar(true)); $('cuImprimir').addEventListener('click', acuse);
    $('cuVolver').addEventListener('click', async event => {
        if (registrando) { event.preventDefault(); return; }
        if (!registrada && modelo.seleccion.size) { event.preventDefault(); if (await confirmar('Volver al menú', incierta ? ['No se recibió un resultado definitivo. Salir no revierte la operación.', 'Conserve la referencia ' + solicitud + ' y verifique el resultado antes de repetirla. ¿Desea volver al menú?'] : ['La entrega no fue confirmada. ¿Desea descartar esta selección y volver al menú?', 'Las correcciones que ya haya guardado se conservan.'])) { descartando = true; window.location.assign($('cuVolver').href); } }
    });
    form.addEventListener('submit', event => event.preventDefault());
    window.addEventListener('beforeunload', event => { if (!descartando && (registrando || (!registrada && modelo.modificada))) { event.preventDefault(); event.returnValue = ''; } });
    cargar(false);
})();
