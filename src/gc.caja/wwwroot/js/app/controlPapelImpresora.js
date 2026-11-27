(function (global) {
    'use strict';
    global.ControlPapelImpresoraVista = Object.freeze({
        actualizar: function (aviso) {
            const texto = typeof aviso === 'string' ? aviso.trim() : '';
            document.querySelectorAll('[data-aviso-papel-impresora]').forEach(function (panel) {
                panel.querySelector('[data-aviso-papel-texto]').textContent = texto;
                panel.hidden = !texto;
            });
        }
    });
})(window);
