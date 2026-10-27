(function (global) {
    'use strict';
    const LIMITE = 999999999999999n; // decimal(15,2)
    const editables = ['PREPARANDO', 'RECHAZADO', 'CANCELADO'];
    function nuevoIntento() {
        if (global.crypto?.randomUUID) return global.crypto.randomUUID();
        if (!global.crypto?.getRandomValues) throw new Error('El navegador no puede identificar de forma segura el intento. Actualice el navegador.');
        const bytes = global.crypto.getRandomValues(new Uint8Array(16)); bytes[6] = (bytes[6] & 15) | 64; bytes[8] = (bytes[8] & 63) | 128;
        const h = Array.from(bytes, b => b.toString(16).padStart(2, '0')).join('');
        return `${h.slice(0,8)}-${h.slice(8,12)}-${h.slice(12,16)}-${h.slice(16,20)}-${h.slice(20)}`;
    }
    function esInstrumento(instrumento, medio) {
        const tipo = String(medio?.tcf_id || instrumento?.tcf_id || '').trim().toUpperCase();
        return tipo === 'T1' || !tipo && ['G0', 'G00'].includes(String(instrumento?.ins_id || '').trim().toUpperCase());
    }
    function centavos(valor) {
        const texto = String(valor ?? '').trim();
        if (!/^\d{1,13}([.,]\d{1,2})?$/.test(texto)) throw new Error('Ingrese un monto válido con hasta dos decimales.');
        const partes = texto.replace(',', '.').split('.');
        const n = BigInt(partes[0]) * 100n + BigInt((partes[1] || '').padEnd(2, '0'));
        if (n > LIMITE) throw new Error('El monto supera el máximo permitido.');
        return n;
    }
    function monto(n) { return Number(n) / 100; }
    function porcentaje(valor) {
        const t = String(valor ?? '').trim();
        if (!/^-?\d{1,6}(\.\d{1,6})?$/.test(t)) throw new Error('El porcentaje de la opción no es válido.');
        const [a,b = ''] = t.replace('-', '').split('.');
        return (BigInt(a) * 1000000n + BigInt(b.padEnd(6, '0'))) * (t.startsWith('-') ? -1n : 1n);
    }
    function redondear(n, d) { const signo = n < 0n ? -1n : 1n; n *= signo; return ((n + d / 2n) / d) * signo; }
    function calcular(base, opcion, saldo) {
        const b = centavos(base), limite = centavos(saldo);
        if (b <= 0n || b > limite) throw new Error('El monto debe ser mayor a cero y no superar el saldo pendiente.');
        const rec = redondear(b * porcentaje(opcion.recargo_porcentaje), 100000000n);
        const total = b + rec;
        if (total <= 0n || total > LIMITE) throw new Error('La opción produce un total fuera del rango permitido.');
        return { base: monto(b), recargo: monto(rec), total: monto(total), saldoRestante: monto(limite - b) };
    }
    function normalizarId(valor) {
        const id = String(valor ?? '').trim().toUpperCase();
        if (!/^[A-Z0-9]{3,30}$/.test(id)) throw new Error('El ID debe tener entre 3 y 30 caracteres alfanuméricos.');
        // SP entregado usa LEN > 3; con tres entraría a SP_Payment y NO a consulta.
        if (id.length === 3) throw new Error('La recuperación de un ID de 3 caracteres todavía no está disponible. Verifique el cupón.');
        return id;
    }
    function opcionesValidas(lista, instrumento, coTipo) {
        if (!Array.isArray(lista)) throw new Error('No se recibió una lista válida de cuotas.');
        return lista.map((o, i) => {
            if (!o || String(o.ins_id || '').trim() !== instrumento.ins_id || !Number.isInteger(o.cuota) || o.cuota < 1 || o.cuota > 32767 || !Number.isInteger(o.pos_plan) || o.pos_plan < 0 || o.pos_plan > 32767 || !String(o.pos_desc || '').trim()) throw new Error('La lista de cuotas está incompleta o pertenece a otro instrumento.');
            const p = porcentaje(o.recargo_porcentaje);
            return { ...o, clave: String(i), permitida: coTipo !== 'CD' || p === 0n };
        }).filter(o => o.permitida);
    }
    function validarAprobacion(respuesta, solicitud) {
        // Resultado tradicional de la API. El SP determina la aprobación del cobro;
        // sólo se interpreta el valor devuelto para incorporarlo a la grilla.
        let valores;
        try { valores = JSON.parse(respuesta.resultado_id); } catch (_) { throw new Error('No se pudieron interpretar los datos del pago. Conserve el ID y verifique el resultado.'); }
        if (!Array.isArray(valores) || valores.length !== 1 || !valores[0]?.ins_id || !(Number(valores[0].rb_importe) > 0)) throw new Error('El resultado no contiene el valor del pago. Verifique antes de repetir.');
        const valor = Object.freeze({ ...valores[0] });
        return Object.freeze({ valorCanonico: valor, intentoId: solicitud.intento_id, paymentId: valor.id_externo || valor.rb_dato1_valor, datosSP: valor, recargo: Number(valor.rb_aux), recargoPorcentaje: Number(valor.rb_rec), base: (Math.round(Number(valor.rb_importe) * 100) - Math.round(Number(valor.rb_aux) * 100)) / 100, total: Number(valor.rb_importe), simulacion: respuesta.simulacion === true });
    }
    class Sesion {
        constructor(transporte, alCambiar = function () {}, alAprobar = function () {}) {
            this.transporte = transporte; this.alCambiar = alCambiar; this.alAprobar = alAprobar; this.version = 0; this.estado = 'CERRADO'; this.opciones = []; this.mensaje = ''; this.resultado = null;
        }
        notificar() { this.alCambiar(this); }
        puedeEditar() { return editables.includes(this.estado); }
        puedeCerrar() { return ['CERRADO','CARGANDO','BLOQUEADO',...editables].includes(this.estado); }
        async abrir(contexto) {
            if (!this.puedeCerrar()) return false;
            const version = ++this.version;
            this.contexto = { ...contexto, instrumento: { ...contexto.instrumento } }; this.base = String(contexto.saldo ?? 0); this.plan = ''; this.paymentid = ''; this.qr = false; this.resultado = null; this.opciones = []; this.mensaje = ''; this.estado = 'CARGANDO'; this.notificar();
            try {
                if (!esInstrumento(contexto.instrumento, contexto.medio)) throw new Error('El instrumento seleccionado no corresponde a Clover.');
                const instrumento = this.contexto.instrumento;
                instrumento.tcf_id = contexto.medio?.tcf_id || instrumento.tcf_id;
                instrumento.ins_id = instrumento.ins_id === 'DEF' ? '' : String(instrumento.ins_id || '').trim();
                if (!['CR','CF','CD','CC','CV','IV'].includes(contexto.coTipo)) throw new Error('La operación no está preparada para este medio de pago.');
                centavos(contexto.saldo);
                const r = await this.transporte.opciones({ ins_id: instrumento.ins_id, co_tipo: contexto.coTipo });
                if (version !== this.version) return false;
                if (r.ok !== true) throw new Error(r.mensaje || 'No se pudieron cargar las cuotas.');
                // El catálogo del servidor define el código; T1 no supone G0 ni G00.
                if (!instrumento.ins_id && Array.isArray(r.opciones) && r.opciones.length) {
                    const codigos = new Set(r.opciones.map(o => String(o.ins_id || '').trim()));
                    if (codigos.size !== 1 || !Array.from(codigos)[0]) throw new Error('No se pudo identificar un único instrumento Clover en las cuotas.');
                    instrumento.ins_id = Array.from(codigos)[0];
                }
                this.opciones = opcionesValidas(r.opciones, instrumento, contexto.coTipo);
                if (!this.opciones.length) throw new Error(contexto.coTipo === 'CD' ? 'Cobranza Diferida requiere una opción sin recargo financiero.' : 'No hay un instrumento Clover con cuotas configuradas para esta operación.');
                this.plan = this.opciones[0].clave; this.estado = 'PREPARANDO'; this.mensaje = r.simulacion ? 'SIMULACIÓN: no se ejecutan cobros ni registros de Caja.' : 'Revise monto y cuotas antes de solicitar el pago.';
            } catch (error) { if (version !== this.version) return false; this.estado = 'BLOQUEADO'; this.mensaje = error.message || 'No se pudieron cargar las cuotas.'; }
            this.notificar(); return true;
        }
        actualizar(datos) {
            if (this.estado === 'INCIERTO' && datos.paymentid !== undefined) { this.paymentid = String(datos.paymentid).toUpperCase(); this.notificar(); return true; }
            if (!this.puedeEditar()) return false;
            if (datos.base !== undefined) this.base = datos.base;
            if (datos.plan !== undefined) this.plan = datos.plan;
            if (datos.paymentid !== undefined) this.paymentid = String(datos.paymentid).toUpperCase();
            if (datos.qr !== undefined) this.qr = !!datos.qr;
            this.notificar(); return true;
        }
        cotizacion() {
            const opcion = this.opciones.find(o => o.clave === this.plan);
            if (!opcion) throw new Error('Seleccione una opción de cuotas válida.');
            return { ...calcular(this.base, opcion, this.contexto.saldo), opcion };
        }
        cerrar() { if (!this.puedeCerrar()) return false; this.version++; this.estado = 'CERRADO'; this.notificar(); return true; }
        async enviar(modo) {
            if (!this.puedeEditar() && !(modo === 'MANUAL' && this.estado === 'INCIERTO')) return false;
            let solicitud, version = this.version;
            try {
                if (!['POS','MANUAL'].includes(modo)) throw new Error('Seleccione una acción de pago válida.');
                const c = this.cotizacion();
                solicitud = { ins_id: this.contexto.instrumento.ins_id, co_tipo: this.contexto.coTipo, monto_base: c.base, recargo: c.recargo, monto_total: c.total, cuotas_reales: c.opcion.cuota, pos_plan: c.opcion.pos_plan, qr: this.qr, paymentid: modo === 'MANUAL' ? normalizarId(this.paymentid) : '' };
                solicitud.intento_id = nuevoIntento();
                solicitud.valores = this.contexto.obtenerValores?.() || [];
                solicitud.uniones = this.contexto.obtenerUniones?.() || [];
            } catch (error) { this.mensaje = error.message; this.notificar(); return false; }
            this.solicitud = solicitud;
            this.estado = 'ESPERANDO'; this.mensaje = modo === 'MANUAL' ? 'Consultando el pago pendiente…' : 'Esperando respuesta del dispositivo. Cancele desde el POS si corresponde.'; this.notificar();
            try {
                const r = await this.transporte.procesar(solicitud);
                if (version !== this.version) return false;
                if (r.resultado !== 0) {
                    this.estado = r.sin_respuesta ? 'INCIERTO' : 'RECHAZADO'; this.mensaje = r.resultado_msj || 'No se aprobó la solicitud.'; this.notificar(); return false;
                }
                this.resultado = validarAprobacion(r, solicitud);
                const c = this.cotizacion();
                this.estado = 'APROBADO'; this.mensaje = this.resultado.simulacion ? 'Pago SIMULADO aprobado; no se registró ninguna operación real.' : 'Pago aprobado pendiente de registración.'; this.notificar();
                // El motor incorpora el valor canónico y finaliza si el saldo queda cubierto.
                try { const aplicado = await this.alAprobar({ ...this.resultado, accion: c.saldoRestante === 0 ? 'FINALIZAR' : 'COMPLETAR', saldoRestante: c.saldoRestante }); if (aplicado === true) { this.estado = 'CERRADO'; this.notificar(); } }
                catch (_) { this.mensaje = 'El pago está aprobado. La registración no se completó; conserve el ID y verifique antes de continuar.'; this.notificar(); }
                return true;
            } catch (error) { if (version !== this.version) return false; this.estado = 'INCIERTO'; this.mensaje = error.message || 'No se pudo conocer el resultado. Verifique el pago antes de repetir.'; this.notificar(); return false; }
        }
    }
    global.CloverModelo = Object.freeze({ Sesion, esInstrumento, calcular, centavos, normalizarId, opcionesValidas, validarAprobacion });
    if (typeof module !== 'undefined' && module.exports) module.exports = global.CloverModelo;
})(typeof window !== 'undefined' ? window : globalThis);
