// ════════════════════════════════════════════════════════════
// MÓDULO DE GESTIÓN DE REPORTES
// ════════════════════════════════════════════════════════════
// VERSIÓN v11.2 - Normalización de Case Sensitivity
// ════════════════════════════════════════════════════════════

/**
 * ✅ ACTUALIZADO v11.2: Módulo para gestionar reportes de comprobantes
 * CORREGIDO: Normalización de propiedades (key/Key, nombre/Nombre, id/Id)
 */
const ModuloReportes = (function () {
    'use strict';

    // ════════════════════════════════════════════════════════════
    // VARIABLES PRIVADAS
    // ════════════════════════════════════════════════════════════

    let _configReportes = null;
    let _cacheTimeout = 3600000; // 1 hora en milisegundos

    // ════════════════════════════════════════════════════════════
    // URLs DE API PARA AREAS
    // ════════════════════════════════════════════════════════════

    const URL_CONFIG_REPORTES = window.SalidaComprobantes?.urlConfiguracionReportes || '/Facturacion/ProductoFact/ObtenerConfigReportes';

    // ════════════════════════════════════════════════════════════
    // ✅ NUEVA FUNCIÓN v11.2: NORMALIZAR PROPIEDADES
    // ════════════════════════════════════════════════════════════

    /**
     * Normaliza un objeto de reporte para garantizar propiedades en PascalCase
     * Acepta tanto minúsculas (key, nombre, id) como PascalCase (Key, Nombre, Id)
     * 
     * @param {Object} reporte - Objeto de reporte con propiedades en cualquier case
     * @returns {Object} - Objeto normalizado con propiedades en PascalCase
     */
    function normalizarReporte(reporte) {
        if (!reporte || typeof reporte !== 'object') {
            return null;
        }

        return {
            // ✅ Priorizar PascalCase, fallback a minúsculas
            Key: reporte.Key || reporte.key || '',
            Nombre: reporte.Nombre || reporte.nombre || '',
            Id: reporte.Id || reporte.id || ''
        };
    }

    // ════════════════════════════════════════════════════════════
    // INICIALIZACIÓN
    // ════════════════════════════════════════════════════════════

    /**
     * ✅ Inicializa el módulo de reportes
     * Carga la configuración desde el servidor
     */
    async function inicializar() {
        // La generación valida la configuración en el servidor. La consulta pública
        // obtenerReportePorKey mantiene su carga diferida para consumidores anteriores.
        return typeof SalidaComprobantes !== 'undefined';
    }

    // ════════════════════════════════════════════════════════════
    // CONFIGURACIÓN DE REPORTES
    // ════════════════════════════════════════════════════════════

    /**
     * ✅ ACTUALIZADO v11.2: Carga la configuración de reportes desde el servidor
     * CORREGIDO: Normaliza propiedades antes de validar
     */
    async function cargarConfiguracionReportes() {
        // ❶ Verificar si ya existe en caché
        if (_configReportes !== null) {
            console.log('ℹ️ Usando configuración de reportes en caché');
            return _configReportes;
        }

        console.log('📡 Cargando configuración de reportes desde servidor...');
        console.log(`   URL: ${URL_CONFIG_REPORTES}`);

        try {
            const response = await $.ajax({
                url: URL_CONFIG_REPORTES,
                type: 'GET',
                dataType: 'json',
                timeout: 10000,
                headers: {
                    'X-Requested-With': 'XMLHttpRequest'
                }
            });

            console.log('═══════════════════════════════════════════════════');
            console.log('📥 RESPUESTA RECIBIDA DEL SERVIDOR');
            console.log('═══════════════════════════════════════════════════');
            console.log('Response completo:', response);
            console.log('   response.ok:', response.ok);
            console.log('   response.reportes:', response.reportes);

            // ❷ ✅ VALIDAR ESTRUCTURA DE RESPUESTA
            if (!response) {
                throw new Error('La respuesta del servidor es null o undefined');
            }

            if (!response.ok) {
                throw new Error(response.mensaje || 'No se pudo cargar configuración de reportes');
            }

            if (!response.reportes) {
                throw new Error('La respuesta no contiene la propiedad "reportes"');
            }

            if (!Array.isArray(response.reportes)) {
                console.error('❌ response.reportes NO es un array:', typeof response.reportes);
                throw new Error('La propiedad "reportes" debe ser un array');
            }

            if (response.reportes.length === 0) {
                console.warn('⚠️ El array de reportes está vacío');
                throw new Error('No hay reportes configurados en el servidor');
            }

            // ❸ ✅ NORMALIZAR Y VALIDAR CADA REPORTE
            const reportesValidos = [];
            const reportesInvalidos = [];

            response.reportes.forEach(function (reporteOriginal, index) {
                console.log(`   Validando reporte [${index}] ORIGINAL:`, reporteOriginal);

                // ✅ NORMALIZAR PROPIEDADES PRIMERO
                const reporte = normalizarReporte(reporteOriginal);

                console.log(`   Validando reporte [${index}] NORMALIZADO:`, reporte);

                // Validar estructura mínima
                if (!reporte) {
                    console.warn(`   ⚠️ Reporte [${index}] es null después de normalizar`);
                    reportesInvalidos.push({ index, razon: 'null después de normalizar' });
                    return;
                }

                // ✅ VALIDAR CON PROPIEDADES NORMALIZADAS
                if (!reporte.Key || typeof reporte.Key !== 'string' || reporte.Key.trim() === '') {
                    console.warn(`   ⚠️ Reporte [${index}] no tiene Key válida:`, reporte);
                    reportesInvalidos.push({ index, razon: 'Key inválida', reporte });
                    return;
                }

                if (!reporte.Nombre || typeof reporte.Nombre !== 'string' || reporte.Nombre.trim() === '') {
                    console.warn(`   ⚠️ Reporte [${index}] no tiene Nombre válido:`, reporte);
                    reportesInvalidos.push({ index, razon: 'Nombre inválido', reporte });
                    return;
                }

                if (!reporte.Id || typeof reporte.Id !== 'string' || reporte.Id.trim() === '') {
                    console.warn(`   ⚠️ Reporte [${index}] no tiene Id válido:`, reporte);
                    reportesInvalidos.push({ index, razon: 'Id inválido', reporte });
                    return;
                }

                // ✅ Reporte válido - guardar NORMALIZADO
                reportesValidos.push(reporte);
                console.log(`   ✅ Reporte [${index}] válido y normalizado`);
            });

            // ❹ MOSTRAR RESUMEN DE VALIDACIÓN
            console.log('═══════════════════════════════════════════════════');
            console.log('📋 RESUMEN DE VALIDACIÓN');
            console.log('═══════════════════════════════════════════════════');
            console.log(`   Reportes totales: ${response.reportes.length}`);
            console.log(`   Reportes válidos: ${reportesValidos.length}`);
            console.log(`   Reportes inválidos: ${reportesInvalidos.length}`);

            if (reportesInvalidos.length > 0) {
                console.warn('⚠️ Reportes inválidos detectados:');
                reportesInvalidos.forEach(function (invalido) {
                    console.warn(`   - [${invalido.index}] ${invalido.razon}`, invalido.reporte);
                });
            }

            if (reportesValidos.length === 0) {
                throw new Error('No hay reportes válidos en la configuración');
            }

            // ❺ GUARDAR SOLO REPORTES VÁLIDOS Y NORMALIZADOS
            _configReportes = reportesValidos;

            console.log('═══════════════════════════════════════════════════');
            console.log('✅ CONFIGURACIÓN DE REPORTES CARGADA');
            console.log('═══════════════════════════════════════════════════');
            console.log(`   Total de reportes válidos: ${_configReportes.length}`);

            _configReportes.forEach(function (reporte) {
                console.log(`   ✅ [${reporte.Key}] ${reporte.Nombre} → ID: ${reporte.Id}`);
            });

            console.log('═══════════════════════════════════════════════════');

            // ❻ Invalidar caché después de 1 hora
            setTimeout(function () {
                console.log('⏱️ Caché de configuración de reportes invalidada');
                _configReportes = null;
            }, _cacheTimeout);

            return _configReportes;

        } catch (error) {
            console.error('═══════════════════════════════════════════════════');
            console.error('❌ ERROR AL CARGAR CONFIGURACIÓN DE REPORTES');
            console.error('═══════════════════════════════════════════════════');
            console.error('Error:', error);
            console.error('   URL intentada:', URL_CONFIG_REPORTES);
            console.error('   Tipo de error:', error.constructor.name);

            if (error.status) {
                console.error(`   HTTP Status: ${error.status} ${error.statusText}`);
            }

            if (error.responseText) {
                console.error('   Response Text:', error.responseText);
            }

            console.error('═══════════════════════════════════════════════════');

            throw error;
        }
    }

    /**
     * ✅ ACTUALIZADO v11.2: Obtiene configuración de un reporte por su Key
     * Ya no necesita normalización adicional porque los reportes en caché están normalizados
     */
    async function obtenerReportePorKey(key) {
        console.log('═══════════════════════════════════════════════════');
        console.log('🔍 BUSCAR REPORTE POR KEY');
        console.log(`   Key solicitada: "${key}"`);
        console.log('═══════════════════════════════════════════════════');

        // ❶ Asegurar que la configuración esté cargada
        if (_configReportes === null) {
            console.log('⚠️ Configuración no cargada, cargando...');
            await cargarConfiguracionReportes();
        }

        // ❷ Validar que haya configuración
        if (!_configReportes || _configReportes.length === 0) {
            console.error('❌ No hay reportes configurados');
            return null;
        }

        console.log(`   Total de reportes configurados: ${_configReportes.length}`);

        // ❸ Normalizar key de búsqueda
        const keyNormalizada = (key || '').trim().toUpperCase();

        if (keyNormalizada === '') {
            console.error('❌ La key proporcionada está vacía');
            return null;
        }

        console.log(`   Key normalizada: "${keyNormalizada}"`);

        // ❹ Logs de reportes disponibles
        console.log('   Reportes disponibles:');
        _configReportes.forEach(function (r, index) {
            console.log(`   [${index}] Key: "${r.Key}", Nombre: "${r.Nombre}", Id: "${r.Id}"`);
        });

        // ❺ Buscar por key (los reportes ya están normalizados)
        const reporte = _configReportes.find(function (r) {
            const reporteKeyNormalizada = r.Key.trim().toUpperCase();
            return reporteKeyNormalizada === keyNormalizada;
        });

        if (!reporte) {
            console.warn(`⚠️ No se encontró configuración para reporte con Key: "${key}"`);
            console.warn('   Keys disponibles:', _configReportes.map(r => r.Key));
            return null;
        }

        console.log('═══════════════════════════════════════════════════');
        console.log('✅ REPORTE ENCONTRADO');
        console.log(`   Key: ${reporte.Key}`);
        console.log(`   Nombre: ${reporte.Nombre}`);
        console.log(`   ID: ${reporte.Id}`);
        console.log('═══════════════════════════════════════════════════');

        return reporte;
    }

    // ════════════════════════════════════════════════════════════
    // GENERACIÓN DE REPORTES
    // ════════════════════════════════════════════════════════════

    /**
     * ✅ ACTUALIZADO v11.2: Genera un reporte de comprobante y lo visualiza
     * Sin cambios en esta función
     */
    async function generarYVisualizarReporte(datosComprobante, opciones) {
        if (typeof SalidaComprobantes === 'undefined') {
            console.error('[IMPRESION] No se encontró el componente común de salida.');
            return false;
        }
        try {
            const resultado = await SalidaComprobantes.solicitar({ tipo: 'FE', ...datosComprobante }, opciones || {});
            return resultado.ok;
        } catch (error) {
            console.error('[IMPRESION] No se pudo presentar el comprobante:', error);
            return false;
        }
    }

    // ════════════════════════════════════════════════════════════
    // VISUALIZACIÓN DE PDF
    // ════════════════════════════════════════════════════════════

    /**
     * ✅ Visualiza un PDF en una nueva pestaña del navegador
     * Sin cambios
     */
    async function visualizarPdfEnNuevaVentana(base64, nombreArchivo, opciones) {
        if (typeof SalidaComprobantes === 'undefined') throw new Error('No se encontró el componente común de salida.');
        const blob = new Blob([Uint8Array.from(atob(base64), c => c.charCodeAt(0))], { type: 'application/pdf' });
        return SalidaComprobantes.enviarPdf(blob, { ...opciones, modo: SalidaComprobantes.resolverModo(opciones?.modo), nombre: nombreArchivo });
    }

    // ════════════════════════════════════════════════════════════
    // API PÚBLICA
    // ════════════════════════════════════════════════════════════

    return {
        inicializar: inicializar,
        generarYVisualizarReporte: generarYVisualizarReporte,
        obtenerReportePorKey: obtenerReportePorKey,
        visualizarPdfEnNuevaVentana: visualizarPdfEnNuevaVentana
    };

})();

// ════════════════════════════════════════════════════════════
// AUTO-INICIALIZACIÓN
// ════════════════════════════════════════════════════════════
$(function () {
    ModuloReportes.inicializar();
});
