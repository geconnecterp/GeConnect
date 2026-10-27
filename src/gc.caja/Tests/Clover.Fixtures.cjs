// Exclusivo de Tests: nunca se sirve ni referencia desde las páginas operativas.
const opciones = [
    { ins_id: 'G00', cuota: 1, pos_plan: 1, recargo_porcentaje: 0, pos_desc: '1 cuota' },
    { ins_id: 'G00', cuota: 5, pos_plan: 12, recargo_porcentaje: 12, pos_desc: '5 cuotas' },
    { ins_id: 'G00', cuota: 1, pos_plan: 8, recargo_porcentaje: -5, pos_desc: 'Descuento de prueba' }
];
const opcionesAnalista = [
    { ins_id: 'G00', cuota: 1, recargo_porcentaje: 0, pos_plan: 1, pos_desc: 'En 1 Pago' },
    { ins_id: 'G00', cuota: 2, recargo_porcentaje: 13.25, pos_plan: 2, pos_desc: 'En 2 Pago' },
    { ins_id: 'G00', cuota: 3, recargo_porcentaje: 19.17, pos_plan: 3, pos_desc: 'EN 3 PAGO' }
];
let secuencia = 0;
function respuesta(solicitud) {
    const id = solicitud.paymentid || 'SIMULADO' + (++secuencia);
    const opcion = [...opciones, ...opcionesAnalista].find(o => o.pos_plan === solicitud.pos_plan && o.cuota === solicitud.cuotas_reales);
    if (!opcion) throw new Error('Fixture sin opción configurada.');
    return { resultado: 0, resultado_msj: 'OK SIMULADO', simulacion: true,
        resultado_id: JSON.stringify([{ rb_nro_valor: '001', ins_id: 'V01', rb_dato1_valor: id,
            rb_dato2_valor: '000012', rb_dato3_valor: '001234', rb_opcion_cuota: String(solicitud.pos_plan),
            rb_fecha_valor: '2026-10-23T21:00:00', rb_importe: solicitud.monto_total, rb_rec: opcion.recargo_porcentaje,
            rb_cupon_manual: solicitud.paymentid ? 'S' : 'N', rb_ch_dif: 'N', rb_estado: 'N', rb_aux: solicitud.recargo, id_externo: id }]) };
}
function crearTransporte(escenario = () => 'aprobado') {
    const llamadas = [], estado = { resolver: null };
    return {
        llamadas, estado,
        opciones: async () => ({ ok: true, simulacion: true, opciones: opciones.map(o => ({ ...o })) }),
        procesar: async solicitud => {
            llamadas.push({ ...solicitud });
            switch (escenario()) {
                case 'rechazado': return { resultado: 2, resultado_msj: 'Tarjeta rechazada (simulado).' };
                case 'cancelado': return { resultado: 2, resultado_msj: 'Solicitud cancelada desde el POS simulado.' };
                case 'imputado': return { resultado: 2, resultado_msj: 'El pago ya se imputó (simulado).' };
                case 'inexistente': return { resultado: 2, resultado_msj: 'El pago no existe (simulado).' };
                case 'timeout': throw new Error('Se perdió la respuesta (simulado); no repetir el cobro.');
                case 'sin-datos': { const r = respuesta(solicitud); r.resultado_id = '[]'; return r; }
                case 'json-invalido': { const r = respuesta(solicitud); r.resultado_id = '<error>'; return r; }
                case 'espera': return new Promise(resolve => { estado.resolver = () => resolve(respuesta(solicitud)); });
                default: return respuesta(solicitud);
            }
        }
    };
}
module.exports = { opciones, opcionesAnalista, respuesta, crearTransporte };
