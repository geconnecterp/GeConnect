// ════════════════════════════════════════════════════════════
// MÓDULO DE GESTIÓN DE REPORTES
// ════════════════════════════════════════════════════════════
// VERSIÓN v10.0 - Sistema de reportes para comprobantes
// ════════════════════════════════════════════════════════════

/**
 * ✅ NUEVO v10.0: Módulo para gestionar reportes de comprobantes
 * Responsabilidades:
 * - Cachear configuración de reportes
 * - Mapear tipos de comprobantes → IDs de reporte
 * - Invocar API de reportes
 * - Visualizar PDFs en nueva pestaña
 */
const ModuloReportes = (function () {
    'use strict';

    // ════════════════════════════════════════════════════════════
    // VARIABLES PRIVADAS
    // ════════════════════════════════════════════════════════════

    let _configReportes = null;
    let _cacheTimeout = 3600000; // 1 hora en milisegundos

    // ════════════════════════════════════════════════════════════
    // URLS DE API
    // ════════════════════════════════════════════════════════════

    const URL_CONFIG_REPORTES = '/ProductoFact/ObtenerConfigReportes';
    const URL_GENERAR_REPORTE = '/ProductoFact/GenerarReporteComprobante';

    // ════════════════════════════════════════════════════════════
    // INICIALIZACIÓN
    // ════════════════════════════════════════════════════════════

    /**
     * ✅ Inicializa el módulo de reportes
     * Carga la configuración desde el servidor
     */
    async function inicializar() {
        console.log('═══════════════════════════════════════════════════');
        console.log('📋 INICIALIZAR MÓDULO DE REPORTES v10.0');
        console.log('═══════════════════════════════════════════════════');

        try {
            await cargarConfiguracionReportes();
            console.log('✅ Módulo de reportes inicializado correctamente');
        } catch (error) {
            console.error('❌ Error al inicializar módulo de reportes:', error);
        }

        console.log('═══════════════════════════════════════════════════');
    }

    // ════════════════════════════════════════════════════════════
    // CONFIGURACIÓN DE REPORTES
    // ════════════════════════════════════════════════════════════

    /**
     * ✅ Carga la configuración de reportes desde el servidor
     * Implementa caché en memoria para evitar llamadas innecesarias
     */
    async function cargarConfiguracionReportes() {
        // ❶ Verificar si ya existe en caché
        if (_configReportes !== null) {
            console.log('ℹ️ Usando configuración de reportes en caché');
            return _configReportes;
        }

        console.log('📡 Cargando configuración de reportes desde servidor...');

        try {
            const response = await $.ajax({
                url: URL_CONFIG_REPORTES,
                type: 'GET',
                dataType: 'json',
                timeout: 10000
            });

            if (!response.ok) {
                throw new Error(response.mensaje || 'No se pudo cargar configuración de reportes');
            }

            _configReportes = response.reportes;

            console.log('═══════════════════════════════════════════════════');
            console.log('✅ CONFIGURACIÓN DE REPORTES CARGADA');
            console.log('═══════════════════════════════════════════════════');
            console.log(`   Total de reportes: ${_configReportes.length}`);

            _configReportes.forEach(function (reporte) {
                console.log(`   ✅ [${reporte.Key}] ${reporte.Nombre} → ID: ${reporte.Id}`);
            });

            console.log('═══════════════════════════════════════════════════');

            // ❷ Invalidar caché después de 1 hora
            setTimeout(function () {
                console.log('⏱️ Caché de configuración de reportes invalidada');
                _configReportes = null;
            }, _cacheTimeout);

            return _configReportes;

        } catch (error) {
            console.error('❌ Error al cargar configuración de reportes:', error);
            throw error;
        }
    }

    /**
     * ✅ Obtiene configuración de un reporte por su Key (A, B, C, etc.)
     * 
     * @param {string} key - Clave del reporte (Ej: "A", "B")
     * @returns {Object|null} - Configuración del reporte o null si no existe
     */
    async function obtenerReportePorKey(key) {
        // ❶ Asegurar que la configuración esté cargada
        if (_configReportes === null) {
            await cargarConfiguracionReportes();
        }

        // ❷ Buscar por key (case-insensitive)
        const keyNormalizada = (key || '').trim().toUpperCase();
        const reporte = _configReportes.find(function (r) {
            return r.Key.trim().toUpperCase() === keyNormalizada;
        });

        if (!reporte) {
            console.warn(`⚠️ No se encontró configuración para reporte con Key: "${key}"`);
            return null;
        }

        console.log(`✅ Reporte encontrado: [${reporte.Key}] ${reporte.Nombre} (ID: ${reporte.Id})`);
        return reporte;
    }

    // ════════════════════════════════════════════════════════════
    // GENERACIÓN DE REPORTES
    // ════════════════════════════════════════════════════════════

    /**
     * ✅ Genera un reporte de comprobante y lo visualiza en nueva pestaña
     * 
     * @param {Object} datosComprobante - Datos del comprobante
     * @param {string} datosComprobante.tco_letra - Letra del comprobante (A, B, C, etc.)
     * @param {string} datosComprobante.tco_id - ID del tipo de comprobante
     * @param {string} datosComprobante.cm_compte - Número de comprobante
     * @param {string} datosComprobante.cm_repetido - Indicador de repetido ("0" o "1")
     * @returns {Promise<boolean>} - true si fue exitoso, false en caso contrario
     */
    async function generarYVisualizarReporte(datosComprobante) {
        console.log('═══════════════════════════════════════════════════');
        console.log('📄 GENERAR Y VISUALIZAR REPORTE DE COMPROBANTE v10.0');
        console.log('═══════════════════════════════════════════════════');
        console.log('Datos del comprobante:', datosComprobante);

        try {
            // ❶ Validar entrada
            if (!datosComprobante || !datosComprobante.tco_letra) {
                throw new Error('Datos de comprobante inválidos: falta tco_letra');
            }

            // ❷ Verificar que el reporte esté configurado
            const reporteConfig = await obtenerReportePorKey(datosComprobante.tco_letra);

            if (!reporteConfig) {
                throw new Error(`No existe configuración de reporte para comprobante tipo "${datosComprobante.tco_letra}"`);
            }

            // ❸ Mostrar loader
            AbrirWaiting("Generando Comprobante...<br><small class='text-muted'>Por favor espere</small>");

            console.log('📡 Invocando API de reportes...');

            // ❹ Llamar al endpoint de generación
            const response = await $.ajax({
                url: URL_GENERAR_REPORTE,
                type: 'POST',
                contentType: 'application/json',
                data: JSON.stringify({
                    tco_letra: datosComprobante.tco_letra,
                    tco_id: datosComprobante.tco_id || '',
                    cm_compte: datosComprobante.cm_compte || '',
                    cm_repetido: datosComprobante.cm_repetido || '0'
                }),
                dataType: 'json',
                timeout: 30000
            });

            CerrarWaiting();

            console.log('═══════════════════════════════════════════════════');
            console.log('📥 RESPUESTA DE API DE REPORTES');
            console.log('═══════════════════════════════════════════════════');
            console.log('Response:', response);

            // ❺ Validar respuesta
            if (response.resultado !== 0) {
                console.error(`❌ Error en API de reportes: ${response.resultado_msj}`);

                AbrirMensaje(
                    "Error al Generar Reporte",
                    response.resultado_msj || 'No se pudo generar el comprobante',
                    function () {
                        $("#msjModal").modal("hide");
                    },
                    false,
                    ["Aceptar"],
                    "error!",
                    null
                );

                return false;
            }

            if (!response.Base64 || response.Base64.trim() === '') {
                console.error('❌ La API no devolvió contenido Base64');

                AbrirMensaje(
                    "Error al Generar Reporte",
                    'El servidor no devolvió el PDF del comprobante',
                    function () {
                        $("#msjModal").modal("hide");
                    },
                    false,
                    ["Aceptar"],
                    "error!",
                    null
                );

                return false;
            }

            console.log(`✅ PDF recibido: ${response.Base64.length} caracteres Base64`);
            console.log(`   Nombre archivo: ${response.resultado_msj}`);

            // ❻ Visualizar PDF en nueva pestaña
            visualizarPdfEnNuevaVentana(response.Base64, response.resultado_msj);

            console.log('═══════════════════════════════════════════════════');
            console.log('✅ REPORTE GENERADO Y VISUALIZADO EXITOSAMENTE');
            console.log('═══════════════════════════════════════════════════');

            return true;

        } catch (error) {
            CerrarWaiting();

            console.error('═══════════════════════════════════════════════════');
            console.error('❌ ERROR AL GENERAR REPORTE');
            console.error('═══════════════════════════════════════════════════');
            console.error('Error:', error);

            // Manejo de errores según tipo
            let mensajeError = 'Error al generar el comprobante';

            if (error.status === 401 || error.status === 403) {
                mensajeError = 'Su sesión ha expirado. Por favor, inicie sesión nuevamente.';
            } else if (error.status === 500) {
                mensajeError = 'Error interno del servidor. Contacte al administrador.';
            } else if (error.status === 0) {
                mensajeError = 'No se pudo conectar con el servidor. Verifique su conexión.';
            } else if (error.responseJSON && error.responseJSON.resultado_msj) {
                mensajeError = error.responseJSON.resultado_msj;
            } else if (error.message) {
                mensajeError = error.message;
            }

            AbrirMensaje(
                "Error al Generar Reporte",
                mensajeError,
                function () {
                    $("#msjModal").modal("hide");
                },
                false,
                ["Aceptar"],
                "error!",
                null
            );

            return false;
        }
    }

    // ════════════════════════════════════════════════════════════
    // VISUALIZACIÓN DE PDF
    // ════════════════════════════════════════════════════════════

    /**
     * ✅ Visualiza un PDF en una nueva pestaña del navegador
     * 
     * @param {string} base64 - Contenido del PDF en Base64
     * @param {string} nombreArchivo - Nombre del archivo (opcional)
     */
    function visualizarPdfEnNuevaVentana(base64, nombreArchivo) {
        console.log('═══════════════════════════════════════════════════');
        console.log('🖥️ VISUALIZAR PDF EN NUEVA VENTANA');
        console.log('═══════════════════════════════════════════════════');
        console.log(`   Tamaño Base64: ${base64.length} caracteres`);
        console.log(`   Nombre archivo: ${nombreArchivo || 'comprobante.pdf'}`);

        try {
            // ❶ Convertir Base64 a Blob
            const byteCharacters = atob(base64);
            const byteNumbers = new Array(byteCharacters.length);

            for (let i = 0; i < byteCharacters.length; i++) {
                byteNumbers[i] = byteCharacters.charCodeAt(i);
            }

            const byteArray = new Uint8Array(byteNumbers);
            const blob = new Blob([byteArray], { type: 'application/pdf' });

            console.log(`✅ Blob creado: ${blob.size} bytes`);

            // ❷ Crear URL temporal del Blob
            const blobUrl = URL.createObjectURL(blob);

            console.log(`✅ URL temporal creada: ${blobUrl}`);

            // ❸ Abrir en nueva pestaña
            const nuevaVentana = window.open(blobUrl, '_blank');

            if (!nuevaVentana) {
                throw new Error('El navegador bloqueó la apertura de la nueva ventana. Verifique la configuración de ventanas emergentes.');
            }

            console.log('✅ Nueva ventana abierta exitosamente');

            // ❹ Limpiar URL temporal después de 30 segundos
            setTimeout(function () {
                URL.revokeObjectURL(blobUrl);
                console.log('🧹 URL temporal liberada');
            }, 30000);

            console.log('═══════════════════════════════════════════════════');
            console.log('✅ PDF VISUALIZADO EXITOSAMENTE');
            console.log('═══════════════════════════════════════════════════');

        } catch (error) {
            console.error('═══════════════════════════════════════════════════');
            console.error('❌ ERROR AL VISUALIZAR PDF');
            console.error('═══════════════════════════════════════════════════');
            console.error('Error:', error);

            AbrirMensaje(
                "Error al Visualizar PDF",
                error.message || 'No se pudo abrir el PDF en una nueva ventana',
                function () {
                    $("#msjModal").modal("hide");
                },
                false,
                ["Aceptar"],
                "error!",
                null
            );
        }
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