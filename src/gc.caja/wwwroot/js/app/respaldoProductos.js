(function (global) {
    'use strict';
    // Una única transacción reemplaza todo el detalle, incluso al eliminar la última fila.
    // La revisión evita que otra pestaña sobrescriba silenciosamente un detalle más reciente.
    function crear(clave, nombreDB = 'geco-caja-respaldos') {
        let base, revision, cola = Promise.resolve(), pendientes = 0;
        function abrir() {
            if (!clave) return Promise.reject(new Error('No se identificó el puesto para guardar el respaldo.'));
            if (!base) base = new Promise((resolve, reject) => {
                if (!global.indexedDB) { reject(new Error('El navegador no permite guardar el respaldo local.')); return; }
                const req = global.indexedDB.open(nombreDB, 1);
                req.onupgradeneeded = () => req.result.createObjectStore('detalles', { keyPath: 'clave' });
                req.onerror = () => reject(req.error);
                req.onblocked = () => reject(new Error('Cierre las otras pestañas de Caja para actualizar el respaldo.'));
                req.onsuccess = () => {
                    const db = req.result;
                    db.onversionchange = () => { db.close(); base = null; };
                    resolve(db);
                };
            }).catch(error => { base = null; throw error; });
            return base;
        }
        async function leerDirecto() {
            const db = await abrir();
            return new Promise((resolve, reject) => {
                const tx = db.transaction('detalles', 'readonly');
                const req = tx.objectStore('detalles').get(clave);
                tx.oncomplete = () => { if (revision === undefined) revision = req.result?.revision || 0; resolve(req.result || null); };
                tx.onabort = tx.onerror = () => reject(tx.error || new Error('No se pudo leer el respaldo.'));
            });
        }
        function guardar(productos) {
            // Capturar ahora: la grilla puede cambiar antes de que termine la transacción anterior.
            const copia = productos.map(p => ({ codigo: String(p.codigo ?? p.p_id).trim(), cantidad: Number(p.cantidad ?? p.cantidad_tot) }));
            pendientes++;
            const tarea = cola.catch(() => {}).then(async () => {
                if (copia.some(p => !p.codigo || !Number.isFinite(p.cantidad) || p.cantidad <= 0)) throw new Error('El detalle contiene cantidades inválidas.');
                if (revision === undefined) await leerDirecto();
                const db = await abrir();
                return new Promise((resolve, reject) => {
                    let tx, conflicto;
                    try { tx = db.transaction('detalles', 'readwrite', { durability: 'strict' }); }
                    catch (_) { tx = db.transaction('detalles', 'readwrite'); }
                    const store = tx.objectStore('detalles');
                    const req = store.get(clave);
                    let siguiente;
                    req.onsuccess = () => {
                        if ((req.result?.revision || 0) !== revision) {
                            conflicto = new Error('Otra pestaña modificó el respaldo. Use una sola pestaña de facturación y recargue para continuar.');
                            tx.abort(); return;
                        }
                        siguiente = revision + 1;
                        store.put({ clave, revision: siguiente, version: 1, fecha: new Date().toISOString(), productos: copia });
                    };
                    tx.oncomplete = () => { revision = siguiente; resolve(); };
                    tx.onabort = tx.onerror = () => reject(conflicto || tx.error || new Error('No se pudo guardar el respaldo local.'));
                });
            });
            cola = tarea.finally(() => pendientes--);
            return cola;
        }
        return { guardar, leer: () => cola.catch(() => {}).then(leerDirecto), esperar: () => cola,
            pendiente: () => pendientes > 0, cerrar: async () => { if (base) (await base).close(); base = null; } };
    }
    global.RespaldoProductos = { crear };
})(globalThis);
