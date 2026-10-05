document.addEventListener('DOMContentLoaded', function () {
    'use strict';
    const form = document.getElementById('admCaja');
    if (!form) return;
    const abrir = document.getElementById('admAbrir'), cerrar = document.getElementById('admCerrar');
    const actualizar = document.getElementById('admActualizar'), volver = document.getElementById('admVolver');
    const consulta = document.getElementById('admConsulta'), filas = document.getElementById('admPuestos');
    const estado = document.getElementById('admEstado');
    let ocupado = false, consultando = false, puedeCerrar = false, terminado = false;
    const seguro = texto => { const span = document.createElement('span'); span.textContent = texto; return span.innerHTML; };
    function controles() {
        abrir.disabled = ocupado || terminado || consultando;
        cerrar.disabled = ocupado || terminado || consultando || !puedeCerrar;
        actualizar.disabled = ocupado || consultando;
        volver.setAttribute('aria-disabled', String(ocupado));
        form.setAttribute('aria-busy', String(ocupado || consultando));
    }
    async function consultar() {
        consultando = true; puedeCerrar = false; controles(); filas.replaceChildren();
        consulta.textContent = 'Consultando puestos de la sucursal…';
        try {
            const response = await fetch(form.dataset.puestos, { headers: { Accept: 'application/json' }, cache: 'no-store' });
            const r = await response.json();
            if (!response.ok || r.ok !== true || !Array.isArray(r.puestos)) throw new Error();
            consulta.textContent = r.mensaje;
            r.puestos.forEach(p => {
                const tr = document.createElement('tr');
                [p.caja_nro_cierre, p.caja_id + ' · ' + (p.caja_nombre || ''), p.usu_apellidoynombre || p.usu_id].forEach(valor => {
                    const td = document.createElement('td'); td.textContent = valor; tr.appendChild(td);
                });
                filas.appendChild(tr);
            });
            puedeCerrar = r.puestos.length === 0;
        } catch (_) { consulta.textContent = 'No se pudo verificar si hay puestos abiertos. Actualice la consulta; el cierre general permanece bloqueado.'; }
        finally { consultando = false; controles(); }
    }
    function ejecutar(apertura) {
        if (ocupado || terminado || (!apertura && (!puedeCerrar || consultando))) return;
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
            if (terminado) estado.textContent += ' Para iniciar otra gestión, vuelva al inicio.';
            ocupado = false; controles(); await consultar();
            AbrirMensaje(r.ok ? 'Operación completada' : 'Administrador de Caja', seguro(r.mensaje),
                () => $('#msjModal').modal('hide'), false, ['Aceptar'], r.ok ? 'succ!' : 'warn!', null);
        }, true, [apertura ? 'Habilitar cajas' : 'Cerrar cajas', 'Cancelar'], 'warn!', null);
    }
    abrir.addEventListener('click', () => ejecutar(true)); cerrar.addEventListener('click', () => ejecutar(false));
    actualizar.addEventListener('click', consultar);
    volver.addEventListener('click', e => { if (ocupado) e.preventDefault(); });
    window.addEventListener('beforeunload', e => { if (ocupado) { e.preventDefault(); e.returnValue = ''; } });
    consultar();
});
