// El historial puede restaurar el DOM completo sin consultar MVC. Comparar la versión
// de operación al volver; no borrar IndexedDB, referencias de impresión ni pagos aprobados.
(function () {
    let consultando = false;
    window.addEventListener('pageshow', async function (event) {
        if (!event.persisted || consultando) return;
        const url = document.body.dataset.estadoOperacionUrl;
        if (!url) return;
        consultando = true;
        try {
            const response = await fetch(url, { credentials: 'same-origin', cache: 'no-store', redirect: 'error' });
            if (!response.ok) throw new Error('No se pudo consultar la vigencia de la operación.');
            const estado = await response.json();
            if (typeof estado.version !== 'string') throw new Error('Respuesta de vigencia inválida.');
            if (estado.version === (document.body.dataset.operacionVersion || '')) return;
            if ((typeof hayCloverPendiente === 'function' && hayCloverPendiente()) ||
                (typeof hayCobroIntegradoCambioValores === 'function' && hayCobroIntegradoCambioValores())) {
                console.warn('[CAJA] Se conservó un cobro integrado pendiente al restaurar la vista.');
                return;
            }
            window.location.reload();
        } catch (error) {
            console.warn('[CAJA] No se pudo verificar la vista restaurada:', error);
        } finally { consultando = false; }
    });
})();
