document.addEventListener('DOMContentLoaded', function () {
    'use strict';
    const form = document.getElementById('admCaja');
    if (!form) return;
    const abrir = document.getElementById('admAbrir'), cerrar = document.getElementById('admCerrar');
    const actualizar = document.getElementById('admActualizar'), volver = document.getElementById('admVolver');
    const consulta = document.getElementById('admConsulta'), filas = document.getElementById('admPuestos');
    const estado = document.getElementById('admEstado');
    const seccionApertura = document.getElementById('admSeccionApertura');
    const seccionCierre = document.getElementById('admSeccionCierre');
    const seccionPuestos = document.getElementById('admSeccionPuestos');
    let ocupado = false, consultando = false, puedeCerrar = false, puedeAbrir = false, terminado = false;
    const seguro = texto => { const span = document.createElement('span'); span.textContent = texto; return span.innerHTML; };
    function controles() {
        abrir.disabled = ocupado || terminado || consultando || !puedeAbrir;
        cerrar.disabled = ocupado || terminado || consultando || !puedeCerrar;
        actualizar.disabled = ocupado || consultando;
        volver.setAttribute('aria-disabled', String(ocupado));
        form.setAttribute('aria-busy', String(ocupado || consultando));
    }
    async function consultar() {
        consultando = true; puedeCerrar = false; puedeAbrir = false; controles(); filas.replaceChildren();
        seccionApertura.hidden = seccionCierre.hidden = seccionPuestos.hidden = true;
        consulta.textContent = 'Consultando habilitación de la sucursal…';
        try {
            const response = await fetch(form.dataset.puestos, { headers: { Accept: 'application/json' }, cache: 'no-store' });
            const r = await response.json().catch(() => { throw new Error('No se recibió un estado válido de la sucursal. Actualice la consulta.'); });
            if (!response.ok || typeof r.ok !== 'boolean' || !Array.isArray(r.puestos)) throw new Error('No se pudo consultar el estado de la sucursal. Actualice la consulta.');
            if (r.habilitada !== true && r.habilitada !== false) throw new Error(r.mensaje || 'Estado de habilitación desconocido.');
            seccionApertura.hidden = r.habilitada !== false;
            seccionCierre.hidden = seccionPuestos.hidden = r.habilitada !== true;
            consulta.textContent = r.mensaje;
            r.puestos.forEach(p => {
                const tr = document.createElement('tr');
                [p.caja_nro_cierre, p.caja_id + ' · ' + (p.caja_nombre || ''), p.usu_apellidoynombre || p.usu_id].forEach(valor => {
                    const td = document.createElement('td'); td.textContent = valor; tr.appendChild(td);
                });
                filas.appendChild(tr);
            });
            puedeAbrir = r.ok === true && r.habilitada === false;
            puedeCerrar = r.ok === true && r.habilitada === true && r.puestos.length === 0;
        } catch (error) { consulta.textContent = error.message && error.message !== 'Failed to fetch' ? error.message : 'No se pudo determinar la habilitación de la sucursal. Actualice la consulta.'; }
        finally { consultando = false; controles(); }
    }
    function ejecutar(apertura) {
        if (ocupado || terminado || consultando || (apertura ? !puedeAbrir : !puedeCerrar)) return;
        ocupado = true; controles();
        let decidido = false;
        const nombre = apertura ? 'apertura general' : 'cierre general';
        AbrirMensaje('Confirmar ' + nombre, seguro('¿Desea realizar ' + (apertura ? 'la ' : 'el ') + nombre + ' de la sucursal ' + form.dataset.sucursal + '?'), async function (decision) {
            if (decidido) return;
            decidido = true; $('#msjModal').modal('hide');
            if (decision !== 'SI') { ocupado = false; controles(); return; }
            estado.className = 'alert alert-info'; estado.textContent = 'Procesando. Espere el resultado de la operación…';
            let r;
            try {
                const body = new URLSearchParams({ solicitud: form.dataset.solicitud, __RequestVerificationToken: form.querySelector('[name="__RequestVerificationToken"]').value });
                const response = await fetch(apertura ? form.dataset.apertura : form.dataset.cierre, {
                    method: 'POST', headers: { Accept: 'application/json' }, body
                });
                r = await response.json();
                if (typeof r.ok !== 'boolean' || (r.ok && !response.ok)) throw new Error();
                terminado = r.ok || r.incierto === true;
            } catch (_) {
                terminado = true;
                r = { ok: false, incierto: true, mensaje: 'No se pudo confirmar el resultado. Verifique el estado de la sucursal antes de repetir la operación.' };
            }
            estado.textContent = r.mensaje;
            estado.className = 'alert alert-' + (r.incierto ? 'warning' : r.ok ? 'success' : 'danger');
            if (terminado && !r.ok) estado.textContent += ' Para iniciar otra gestión, vuelva al inicio.';
            ocupado = false; controles(); await consultar();
            let resultadoCerrado = false;
            AbrirMensaje(r.ok ? 'Operación completada' : 'Administrador de Caja', seguro(r.mensaje), () => {
                if (resultadoCerrado) return;
                resultadoCerrado = true;
                $('#msjModal').modal('hide');
                if (r.ok) window.location.assign(volver.href);
            }, false, ['Aceptar'], r.ok ? 'succ!' : 'warn!', null);
        }, true, [apertura ? 'Habilitar cajas' : 'Cerrar cajas', 'Cancelar'], 'warn!', null);
    }
    abrir.addEventListener('click', () => ejecutar(true)); cerrar.addEventListener('click', () => ejecutar(false));
    actualizar.addEventListener('click', consultar);
    volver.addEventListener('click', e => { if (ocupado) e.preventDefault(); });
    window.addEventListener('beforeunload', e => { if (ocupado) { e.preventDefault(); e.returnValue = ''; } });
    consultar();
});
