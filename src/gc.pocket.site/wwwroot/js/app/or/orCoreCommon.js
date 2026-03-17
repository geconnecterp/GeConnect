// ======================================================================
// VARIABLES GLOBALES Y CONFIGURACIÓN
// ======================================================================
const OR = {
    // Configuración de endpoints
    endpoints: {
        obtenerOrdenes: ObtenerOrdenesRepartoUrl,
        validarUsuario: ValidarUsuarioUrl,
        abrirORLista: AbrirOrListaUrl,
        //obtenerORListByBox: PresentarListaORbyBoxUrl,
        //obtenerORListByRubro: PresentarListaORbyRubroUrl
        //abrirORCarrito: ORCargaCarritoUrl
    },

    // Cache de datos
    cache: {
        ordenesActuales: [],
        ordenSeleccionada: null,
        // ✅ NUEVO: Cache para BOX seleccionado
        boxSeleccionado: {
            box_id: null,
            depo_id: null,
            conteo: null,
            timestamp: null
        },
        // ✅ NUEVO: Cache para RUBRO seleccionado
        rubroSeleccionado: {
            rub_id: null,
            rubg_id: null,
            conteo: null,
            timestamp: null
        }
    },

    // Configuración de la aplicación
    config: {
        animacionDuracion: 300,
        intentosMaximos: 3,
        tiempoEsperaReintento: 2000
    },

    // Selectores DOM
    dom: {
        grid: '#tbGridOrdenesReparto',
        tbody: '#tbGridOrdenesReparto tbody',
        btnRefrescar: '#btnRefrescar',
        btnContinuar: '#btnContinuar',
        noDataRow: '#noDataRow',
        totalOrdenes: '#totalOrdenes',
        ultimaActualizacion: '#ultimaActualizacion',
        // ✅ NUEVO: Selectores para BOX y RUBRO
        tbOrBoxList: '#tbOrBoxList',
        tbOrRubList: '#tbOrRubList'
    }
};

// ======================================================================
// ✅ NUEVAS FUNCIONES HELPER PARA CACHE DE BOX Y RUBRO
// ======================================================================

/**
 * Guarda los datos del BOX seleccionado en cache
 * @param {string} boxId - ID del BOX
 * @param {string} depoId - ID del depósito
 * @param {number} conteo - Cantidad de ítems
 */
function guardarBoxEnCache(boxId, depoId, conteo) {
    OR.cache.boxSeleccionado = {
        box_id: boxId,
        depo_id: depoId,
        conteo: conteo,
        timestamp: new Date().toISOString()
    };
    
    console.log('📦 BOX guardado en cache:', OR.cache.boxSeleccionado);
}

/**
 * Guarda los datos del RUBRO seleccionado en cache
 * @param {string} rubId - ID del rubro
 * @param {string} rubgId - ID del grupo de rubro
 * @param {number} conteo - Cantidad de ítems
 */
function guardarRubroEnCache(rubId, rubgId, conteo) {
    OR.cache.rubroSeleccionado = {
        rub_id: rubId,
        rubg_id: rubgId,
        conteo: conteo,
        timestamp: new Date().toISOString()
    };
    
    console.log('🏷️ RUBRO guardado en cache:', OR.cache.rubroSeleccionado);
}

/**
 * Obtiene el BOX seleccionado del cache
 * @returns {object|null} Objeto con datos del BOX o null si no hay selección
 */
function obtenerBoxDeCache() {
    if (!OR.cache.boxSeleccionado.box_id) {
        console.warn('⚠️ No hay BOX seleccionado en cache');
        return null;
    }
    
    return OR.cache.boxSeleccionado;
}

/**
 * Obtiene el RUBRO seleccionado del cache
 * @returns {object|null} Objeto con datos del RUBRO o null si no hay selección
 */
function obtenerRubroDeCache() {
    if (!OR.cache.rubroSeleccionado.rub_id) {
        console.warn('⚠️ No hay RUBRO seleccionado en cache');
        return null;
    }
    
    return OR.cache.rubroSeleccionado;
}

/**
 * Limpia el cache de BOX seleccionado
 */
function limpiarCacheBox() {
    OR.cache.boxSeleccionado = {
        box_id: null,
        depo_id: null,
        conteo: null,
        timestamp: null
    };
    
    console.log('🧹 Cache de BOX limpiado');
}

/**
 * Limpia el cache de RUBRO seleccionado
 */
function limpiarCacheRubro() {
    OR.cache.rubroSeleccionado = {
        rub_id: null,
        rubg_id: null,
        conteo: null,
        timestamp: null
    };
    
    console.log('🧹 Cache de RUBRO limpiado');
}

/**
 * Valida si hay datos válidos en el cache de BOX
 * @returns {boolean} true si hay datos válidos
 */
function validarCacheBox() {
    const cache = OR.cache.boxSeleccionado;
    return !!(cache.box_id && cache.depo_id);
}

/**
 * Valida si hay datos válidos en el cache de RUBRO
 * @returns {boolean} true si hay datos válidos
 */
function validarCacheRubro() {
    const cache = OR.cache.rubroSeleccionado;
    return !!(cache.rub_id);
}

/**
 * ✅ API PÚBLICA: Exponer funciones globalmente
 */
window.OR_CACHE_API = {
    // Funciones de BOX
    guardarBox: guardarBoxEnCache,
    obtenerBox: obtenerBoxDeCache,
    limpiarBox: limpiarCacheBox,
    validarBox: validarCacheBox,
    
    // Funciones de RUBRO
    guardarRubro: guardarRubroEnCache,
    obtenerRubro: obtenerRubroDeCache,
    limpiarRubro: limpiarCacheRubro,
    validarRubro: validarCacheRubro
};

console.log('✅ API de Cache OR expuesta globalmente como window.OR_CACHE_API');