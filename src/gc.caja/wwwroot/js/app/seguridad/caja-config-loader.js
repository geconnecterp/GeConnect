/**
 * ═══════════════════════════════════════════════════════════════════════════
 * SISTEMA DE CARGA DE CONFIGURACIÓN DE CAJA - ESTACIÓN LOCAL (v3 - Gesto de Usuario)
 * ═══════════════════════════════════════════════════════════════════════════
 * 
 * Versión: 2.0.0
 * Responsable: Sistema de Caja
 * 
 * PROPÓSITO:
 * - Intentar leer el archivo de configuración de forma automática y transparente.
 * - Si falla, recurrir a un método de selección manual como plan B.
 * 
 * FLUJO HÍBRIDO:
 * 1. Intento Automático: Usar File System Access API para leer la ruta proporcionada.
 * 2. Plan B (Fallo automático): Mostrar modal para que el usuario seleccione el archivo.
 * 3. Almacenamiento: Guardar en LocalStorage y enviar al servidor.
 * 
 * ═══════════════════════════════════════════════════════════════════════════
 */

const CajaConfigLoader = (function () {
    'use strict';

    // ═══════════════════════════════════════════════════════════════════════
    // CONFIGURACIÓN Y ESTADO
    // ═══════════════════════════════════════════════════════════════════════

    const CONFIG = {
        localStorageKey: 'GeConnect_CajaConfig',
        versionKey: 'GeConnect_CajaConfig_Version',
        currentVersion: '2.0.0',
        apiEndpoint: null,
        maxFileSizeBytes: 1048576,
        allowedFileExtension: '.json'
    };

    const SCHEMA = {
        required: ['CajaId', 'AdmId', 'IP'],
        optional: ['Caja', 'Descripcion', 'NombreEstacion', 'Facturacion', 'TipoCnnCF'],
        types: { CajaId: 'string', AdmId: 'string', IP: 'string', Caja: 'object', Descripcion: 'string', NombreEstacion: 'string' }
    };

    let _configActual = null;
    let _callbacks = { onSuccess: null, onError: null, onProgress: null, onAutoLoadFailed: null };

    // ═══════════════════════════════════════════════════════════════════════
    // MÉTODOS PRIVADOS - LÓGICA DE CARGA AUTOMÁTICA (File System Access API)
    // ═══════════════════════════════════════════════════════════════════════

    /**
     * ✅ NUEVO: Intenta leer el archivo de configuración de forma automática.
     * @param {string} rutaCompleta - Ruta local del archivo (ej. C:\Config\caja.json)
     * @returns {Promise<Object>} - { success: boolean, file: File|null, error: string|null }
     */
    async function _intentarCargaAutomatica(rutaCompleta) {
        console.log('═══════════════════════════════════════════════════════');
        console.log('🔍 INTENTO DE CARGA AUTOMÁTICA');
        console.log(`   Ruta objetivo: ${rutaCompleta}`);
        console.log('═══════════════════════════════════════════════════════');

        // 1. Verificar compatibilidad del navegador
        if (!('showOpenFilePicker' in window)) {
            console.warn('⚠️ Navegador no compatible con File System Access API.');
            return { success: false, file: null, error: 'Navegador no compatible para carga automática.' };
        }

        // 2. Parsear la ruta para obtener el nombre del archivo
        // Reemplaza las barras invertidas de Windows por barras normales
        const rutaNormalizada = rutaCompleta.replace(/\\/g, '/');
        const nombreArchivo = rutaNormalizada.split('/').pop();

        if (!nombreArchivo) {
            return { success: false, file: null, error: 'La ruta del archivo de configuración no es válida.' };
        }

        try {
            // 3. Solicitar acceso al archivo. El navegador puede pedir permiso.
            // startIn: 'desktop' es un truco para poder acceder a cualquier parte del sistema.
            const [fileHandle] = await window.showOpenFilePicker({
                startIn: 'desktop',
                suggestedName: nombreArchivo,
                types: [{
                    description: 'Archivos de Configuración',
                    accept: { 'application/json': ['.json'] }
                }],
            });

            // 4. Obtener el objeto File y retornarlo para procesamiento
            const file = await fileHandle.getFile();
            console.log('✅ Carga automática exitosa. Archivo obtenido.');
            return { success: true, file: file, error: null };

        } catch (error) {
            // El usuario canceló el selector de archivos o denegó el permiso.
            if (error.name === 'AbortError') {
                console.log('⚠️ El usuario canceló la selección o denegó el permiso.');
                return { success: false, file: null, error: 'El usuario canceló la operación.' };
            }
            console.error('❌ Error en carga automática:', error);
            return { success: false, file: null, error: `Error técnico: ${error.message}` };
        }
    }


    // ═══════════════════════════════════════════════════════════════════════
    // MÉTODOS PRIVADOS - LÓGICA COMÚN (Validación, Almacenamiento, etc.)
    // ═══════════════════════════════════════════════════════════════════════

    function _validarArchivo(file) {
        if (!file.name.toLowerCase().endsWith(CONFIG.allowedFileExtension)) return { valid: false, error: `El archivo debe tener extensión ${CONFIG.allowedFileExtension}` };
        if (file.size === 0) return { valid: false, error: 'El archivo está vacío' };
        if (file.size > CONFIG.maxFileSizeBytes) return { valid: false, error: `El archivo excede el tamaño máximo permitido (${CONFIG.maxFileSizeBytes / 1024} KB)` };
        return { valid: true, error: null };
    }

    function _validarEstructura(config) {
        if (!config || typeof config !== 'object') return { valid: false, error: 'El contenido del archivo no es un objeto JSON válido', warnings: [] };
        for (const field of SCHEMA.required) {
            if (!(field in config)) return { valid: false, error: `Falta el campo requerido: ${field}`, warnings: [] };
            const expectedType = SCHEMA.types[field];
            const actualType = typeof config[field];
            if (actualType !== expectedType) return { valid: false, error: `El campo '${field}' debe ser de tipo ${expectedType}, pero es ${actualType}`, warnings: [] };
            if (expectedType === 'string' && config[field].trim() === '') return { valid: false, error: `El campo '${field}' no puede estar vacío`, warnings: [] };
        }
        return { valid: true, error: null, warnings: [] };
    }

    function _guardarEnLocalStorage(config) {
        try {
            const dataToStore = { config: config, timestamp: new Date().toISOString(), version: CONFIG.currentVersion };
            localStorage.setItem(CONFIG.localStorageKey, JSON.stringify(dataToStore));
            localStorage.setItem(CONFIG.versionKey, CONFIG.currentVersion);
            return true;
        } catch (error) { return false; }
    }

    function _recuperarDeLocalStorage() {
        try {
            const stored = localStorage.getItem(CONFIG.localStorageKey);
            const storedVersion = localStorage.getItem(CONFIG.versionKey);
            if (!stored || storedVersion !== CONFIG.currentVersion) {
                if (stored) _limpiarLocalStorage();
                return null;
            }
            return JSON.parse(stored).config;
        } catch (error) {
            _limpiarLocalStorage();
            return null;
        }
    }

    function _limpiarLocalStorage() {
        localStorage.removeItem(CONFIG.localStorageKey);
        localStorage.removeItem(CONFIG.versionKey);
    }

    async function _enviarAlServidor(config) {
        if (!CONFIG.apiEndpoint) return { success: false, error: 'Error de configuración: Endpoint no inicializado' };
        try {
            const antiForgeryToken = document.querySelector('input[name="__RequestVerificationToken"]')?.value;
            const response = await fetch(CONFIG.apiEndpoint, {
                method: 'POST',
                headers: { 'Content-Type': 'application/json', 'X-Requested-With': 'XMLHttpRequest', 'RequestVerificationToken': antiForgeryToken || '' },
                body: JSON.stringify(config),
                credentials: 'same-origin'
            });
            const result = await response.json();
            if (!response.ok) throw new Error(result.mensaje || 'Error al enviar configuración al servidor');
            return { success: true, data: result };
        } catch (error) {
            return { success: false, error: error.message };
        }
    }

    async function _procesarArchivo(file) {
        const validacionArchivo = _validarArchivo(file);
        if (!validacionArchivo.valid) return { success: false, error: validacionArchivo.error };

        const contenido = await new Promise(resolve => {
            const reader = new FileReader();
            reader.onload = e => resolve({ success: true, data: e.target.result });
            reader.onerror = () => resolve({ success: false, error: 'No se pudo leer el archivo.' });
            reader.readAsText(file);
        });
        if (!contenido.success) return contenido;

        let config;
        try {
            config = JSON.parse(contenido.data);
        } catch (error) {
            return { success: false, error: 'El archivo no contiene un JSON válido: ' + error.message };
        }

        const validacionEstructura = _validarEstructura(config);
        if (!validacionEstructura.valid) return { success: false, error: validacionEstructura.error };

        _guardarEnLocalStorage(config);
        const resultadoServidor = await _enviarAlServidor(config);
        if (!resultadoServidor.success) return { success: false, error: 'Configuración válida pero falló el envío al servidor: ' + resultadoServidor.error };

        _configActual = config;
        return { success: true, config: config, warnings: validacionEstructura.warnings };
    }

    // ═══════════════════════════════════════════════════════════════════════
    // API PÚBLICA DEL MÓDULO
    // ═══════════════════════════════════════════════════════════════════════

    return {
        init: function (endpointUrl, callbacks) {
            CONFIG.apiEndpoint = endpointUrl;
            _callbacks = { ...callbacks };
            const configRecuperada = _recuperarDeLocalStorage();
            if (configRecuperada) {
                _configActual = configRecuperada;
                return { hasConfig: true, config: configRecuperada };
            }
            return { hasConfig: false, config: null };
        },

        /**
         * ✅ CORREGIDO: Orquesta el proceso de carga, priorizando el modo automático.
         * @param {string} rutaPreferida - Ruta local del archivo a intentar cargar.
         */
        iniciarCarga: async function (rutaPreferida) {
            if (_callbacks.onProgress) _callbacks.onProgress('Intentando carga automática...');

            const resultadoAuto = await _intentarCargaAutomatica(rutaPreferida);

            if (resultadoAuto.success && resultadoAuto.file) {
                // Éxito en la carga automática, procesar el archivo obtenido
                const resultadoProceso = await _procesarArchivo(resultadoAuto.file);
                if (resultadoProceso.success) {
                    if (_callbacks.onSuccess) _callbacks.onSuccess(resultadoProceso.config, resultadoProceso.warnings);
                } else {
                    if (_callbacks.onError) _callbacks.onError(resultadoProceso.error);
                }
                return;
            }

            // Si la carga automática falla, se notifica al controlador principal (login.js)
            // para que inicie el Plan B (mostrar el modal de selección manual).
            if (_callbacks.onAutoLoadFailed) {
                // ✅ CORREGIDO: Asegurarse de que el callback exista antes de llamarlo.
                _callbacks.onAutoLoadFailed(resultadoAuto.error);
            }
        },

        cargarDesdeArchivoManual: async function (file) {
            if (_callbacks.onProgress) _callbacks.onProgress('Procesando archivo manual...');
            const resultado = await _procesarArchivo(file);
            if (resultado.success) {
                if (_callbacks.onSuccess) _callbacks.onSuccess(resultado.config, resultado.warnings);
            } else {
                if (_callbacks.onError) _callbacks.onError(resultado.error);
            }
            return resultado;
        },

        cargarDesdeCache: async function () {
            const config = _recuperarDeLocalStorage();
            if (!config) return { success: false, error: 'No hay configuración en cache' };
            const resultadoServidor = await _enviarAlServidor(config);
            if (!resultadoServidor.success) return { success: false, error: 'No se pudo sincronizar la configuración con el servidor' };
            _configActual = config;
            return { success: true, config: config };
        },

        obtenerConfigActual: () => _configActual,
        limpiarConfig: () => { _limpiarLocalStorage(); _configActual = null; },
        tieneConfigValida: () => _configActual !== null,
        // ✅ NUEVO: Exponer los callbacks para que puedan ser llamados desde fuera si es necesario.
        get callbacks() {
            return _callbacks;
        }
    };
})();