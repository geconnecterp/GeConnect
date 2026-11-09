(function (global) {
    'use strict';
    if (!global.PagoIntegradoModelo && typeof require === 'function') require('./cloverPago.js');
    const comun = global.PagoIntegradoModelo;
    function esInstrumento(instrumento, medio) { return String(medio?.tcf_id || instrumento?.tcf_id || '').trim().toUpperCase() === 'T2'; }
    class Sesion extends comun.Sesion {
        constructor(transporte, alCambiar, alAprobar) { super(transporte, alCambiar, alAprobar, { nombre: 'Mercado Pago', esInstrumento, point: true }); }
    }
    global.MercadoPagoModelo = Object.freeze({ ...comun, Sesion, esInstrumento });
    if (typeof module !== 'undefined' && module.exports) module.exports = global.MercadoPagoModelo;
})(typeof window !== 'undefined' ? window : globalThis);
