(function (window, document, $) {
    'use strict';
    const pendientes = new Set();
    let reinicio = null;
    let enviandoReinicio = false;
    let resultadoIncierto = false;
    let preparada = false;
    window.operacionFacturaPreparada = () => preparada;

    // Esperar las respuestas completas: abortar un XHR no cancela una escritura en el servidor.
    $.ajaxPrefilter(function (options, original, xhr) {
        const url = new URL(options.url, window.location.href);
        if (url.origin !== window.location.origin ||
            !/\/Facturacion\/(ProductoFact|Cliente|Checkout|ListaPrecio)\//i.test(url.pathname)) return;
        if (enviandoReinicio) { xhr.abort('reinicio'); return; }
        let terminar;
        const pendiente = new Promise(resolve => { terminar = resolve; });
        pendientes.add(pendiente);
        xhr.always(function () {
            if (xhr.status === 0 && /\/Facturacion\/(ProductoFact|Checkout)\//i.test(url.pathname)) resultadoIncierto = true;
            pendientes.delete(pendiente);
            terminar();
        });
    });

    function bloquear() {
        const modales = [];
        $('.modal').each(function () { modales.push([this, this.inert]); this.inert = true; });
        const overlay = $('<div id="reinicioOperacionFactura" class="position-fixed top-0 start-0 w-100 h-100 d-flex align-items-center justify-content-center" role="status" tabindex="-1" style="z-index:2147483647;background:rgba(0,0,0,.5)"><div class="card card-golden p-4 text-center shadow"><i class="bx bx-loader-alt bx-spin text-golden fs-1"></i><strong class="mt-3">Preparando nueva venta…</strong></div></div>');
        $('body').append(overlay);
        overlay.trigger('focus');
        return function () { overlay.remove(); modales.forEach(([modal, inert]) => { modal.inert = inert; }); };
    }

    async function ejecutarReinicio() {
        if (window.cambioListaPrecioEnCurso?.() || window.AutorizacionRemota?.estaEsperando())
            throw new Error('Finalizá la autorización en curso antes de iniciar otra venta.');
        const desbloquear = bloquear();
        try {
            let timeout;
            try {
                await Promise.race([
                    (async () => {
                        do {
                            await Promise.all([...pendientes]);
                            await new Promise(resolve => setTimeout(resolve, 0));
                        } while (pendientes.size);
                    })(),
                    new Promise((_, reject) => { timeout = setTimeout(() => reject(new Error('Todavía hay una petición en curso. Esperá que termine antes de reiniciar la venta.')), 45000); })
                ]);
            } finally { clearTimeout(timeout); }
            if (resultadoIncierto)
                throw new Error('Una petición anterior no tuvo una respuesta completa. Verificá la operación y recargá la pantalla antes de iniciar otra venta.');
            preparada = false;
            enviandoReinicio = true;
            await new Promise((resolve, reject) => {
                $.ajax({ url: window.NuevaOperacionFacturaUrl, type: 'POST', dataType: 'json', timeout: 30000 })
                    .done(response => response?.ok ? resolve() : reject(new Error(response?.mensaje || 'No se pudo reiniciar la venta.')))
                    .fail(xhr => reject(new Error(xhr.responseJSON?.mensaje || 'No se pudo preparar la nueva venta. Volvé a intentar.')));
            });
            window.limpiarVentaCompleta?.(false);
            window.descartarBorradorPago?.();
            preparada = true;
        } finally {
            enviandoReinicio = false;
            desbloquear();
        }
    }

    window.prepararNuevaOperacionFactura = function () {
        if (!reinicio) reinicio = ejecutarReinicio().finally(() => { reinicio = null; });
        return reinicio;
    };
})(window, document, window.jQuery);
