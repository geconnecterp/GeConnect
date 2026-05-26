// ════════════════════════════════════════════════════════════
// MÓDULO DE MÁSCARAS DE ENTRADA PARA VALORES MONETARIOS
// ════════════════════════════════════════════════════════════
// VERSIÓN v1.0 - Implementación de InputMask argentino
// Formato: punto (.) separador de miles, coma (,) separador decimal
// Ejemplos: 1.234.567,89 | 599.994,16 | 100,50
// ════════════════════════════════════════════════════════════
// Autor: GeConnect ERP
// Fecha: 2026-05-25
// Dependencias: jQuery, Inputmask (jquery.inputmask.min.js)
// ════════════════════════════════════════════════════════════

/**
 * Namespace para máscaras monetarias
 */
const InputMaskMonetario = (function () {
    'use strict';

    // ═══════════════════════════════════════════════════════════════
    // CONFIGURACIONES PREDEFINIDAS
    // ═══════════════════════════════════════════════════════════════

    /**
     * ✅ Configuración base para valores monetarios en formato argentino
     * Características:
     * - Separador de miles: . (punto)
     * - Separador decimal: , (coma)
     * - Alineación a la derecha
     * - Sin prefijo/sufijo por defecto
     * - Números positivos únicamente
     */
    const configuracionBase = {
        alias: "numeric",
        groupSeparator: ".",       // ✅ Punto para miles (argentino)
        radixPoint: ",",           // ✅ Coma para decimales (argentino)
        autoGroup: true,           // Agrupa automáticamente por miles
        digits: 2,                 // Decimales por defecto
        digitsOptional: false,     // Los decimales son obligatorios
        rightAlign: true,          // Alinear a la derecha
        prefix: '',                // Sin prefijo
        suffix: '',                // Sin sufijo
        placeholder: "0",          // Placeholder cuando está vacío
        clearMaskOnLostFocus: false, // Mantener formato al perder foco
        showMaskOnHover: false,    // No mostrar máscara al hover
        showMaskOnFocus: false,    // No mostrar máscara al focus
        allowMinus: false,         // Solo valores positivos
        min: 0,                    // Valor mínimo
        max: 999999999.99,         // Valor máximo (ajustable)
        nullable: false,           // No permitir null

        /**
         * Función ejecutada antes de aplicar la máscara
         * Normaliza el valor de entrada
         */
        onBeforeMask: function (value) {
            if (!value) return '0,00';

            // Convertir a string
            let strValue = value.toString();

            // Si ya está en formato argentino, preservarlo
            if (strValue.includes(',')) {
                return strValue;
            }

            // Si viene como número JavaScript (punto decimal)
            const numValue = parseFloat(strValue.replace(/[^\d.-]/g, ''));

            if (isNaN(numValue)) {
                return '0,00';
            }

            // Convertir a formato argentino
            return numValue.toFixed(2).replace('.', ',');
        },

        /**
         * Función ejecutada cuando el campo pierde el foco
         * Asegura que siempre tenga formato correcto
         */
        onUnMask: function (maskedValue) {
            if (!maskedValue) return '0';

            // Remover separadores de miles (puntos)
            let cleanValue = maskedValue.replace(/\./g, '');

            // Reemplazar coma decimal por punto (para parseFloat)
            cleanValue = cleanValue.replace(',', '.');

            return cleanValue;
        }
    };

    /**
     * ✅ Configuración para moneda en PESOS ($)
     * Formato: $ 1.234.567,89
     */
    const configuracionPesos = {
        ...configuracionBase,
        prefix: '$ ',              // Prefijo con espacio
        digits: 2,
        max: 999999999.99
    };

    /**
     * ✅ Configuración para moneda en DÓLARES (USD)
     * Formato: USD 1.234.567,89
     */
    const configuracionDolares = {
        ...configuracionBase,
        prefix: 'USD ',            // Prefijo con espacio
        digits: 2,
        max: 999999999.99
    };

    /**
     * ✅ Configuración SIN prefijo (valores genéricos)
     * Formato: 1.234.567,89
     */
    const configuracionSinPrefijo = {
        ...configuracionBase,
        prefix: '',
        digits: 2
    };

    /**
     * ✅ Configuración para porcentajes
     * Formato: 21,50 %
     */
    const configuracionPorcentaje = {
        ...configuracionBase,
        prefix: '',
        suffix: ' %',
        digits: 2,
        min: 0,
        max: 100
    };

    /**
     * ✅ Configuración para cantidades (sin decimales)
     * Formato: 1.234
     */
    const configuracionCantidad = {
        ...configuracionBase,
        digits: 0,
        digitsOptional: true,
        radixPoint: "",            // Sin separador decimal
        max: 999999999
    };

    // ═══════════════════════════════════════════════════════════════
    // FUNCIONES PÚBLICAS
    // ═══════════════════════════════════════════════════════════════

    /**
     * ✅ Aplica máscara de pesos a un selector
     * @param {string|jQuery} selector - Selector CSS o elemento jQuery
     * @param {Object} opciones - Opciones adicionales para override
     * @returns {jQuery} - Elementos enmascarados
     */
    function aplicarMascaraPesos(selector, opciones = {}) {
        console.log(`🎭 Aplicando máscara PESOS a: ${selector}`);

        const config = { ...configuracionPesos, ...opciones };
        const $elementos = $(selector);

        if ($elementos.length === 0) {
            console.warn(`⚠️ No se encontraron elementos para: ${selector}`);
            return $elementos;
        }

        Inputmask(config).mask($elementos);

        console.log(`✅ Máscara aplicada a ${$elementos.length} elemento(s)`);
        return $elementos;
    }

    /**
     * ✅ Aplica máscara de dólares a un selector
     * @param {string|jQuery} selector - Selector CSS o elemento jQuery
     * @param {Object} opciones - Opciones adicionales para override
     * @returns {jQuery} - Elementos enmascarados
     */
    function aplicarMascaraDolares(selector, opciones = {}) {
        console.log(`🎭 Aplicando máscara DÓLARES a: ${selector}`);

        const config = { ...configuracionDolares, ...opciones };
        const $elementos = $(selector);

        if ($elementos.length === 0) {
            console.warn(`⚠️ No se encontraron elementos para: ${selector}`);
            return $elementos;
        }

        Inputmask(config).mask($elementos);

        console.log(`✅ Máscara aplicada a ${$elementos.length} elemento(s)`);
        return $elementos;
    }

    /**
     * ✅ Aplica máscara sin prefijo (genérica)
     * @param {string|jQuery} selector - Selector CSS o elemento jQuery
     * @param {Object} opciones - Opciones adicionales para override
     * @returns {jQuery} - Elementos enmascarados
     */
    function aplicarMascaraMonetaria(selector, opciones = {}) {
        console.log(`🎭 Aplicando máscara MONETARIA a: ${selector}`);

        const config = { ...configuracionSinPrefijo, ...opciones };
        const $elementos = $(selector);

        if ($elementos.length === 0) {
            console.warn(`⚠️ No se encontraron elementos para: ${selector}`);
            return $elementos;
        }

        Inputmask(config).mask($elementos);

        console.log(`✅ Máscara aplicada a ${$elementos.length} elemento(s)`);
        return $elementos;
    }

    /**
     * ✅ Aplica máscara de porcentaje
     * @param {string|jQuery} selector - Selector CSS o elemento jQuery
     * @param {Object} opciones - Opciones adicionales para override
     * @returns {jQuery} - Elementos enmascarados
     */
    function aplicarMascaraPorcentaje(selector, opciones = {}) {
        console.log(`🎭 Aplicando máscara PORCENTAJE a: ${selector}`);

        const config = { ...configuracionPorcentaje, ...opciones };
        const $elementos = $(selector);

        if ($elementos.length === 0) {
            console.warn(`⚠️ No se encontraron elementos para: ${selector}`);
            return $elementos;
        }

        Inputmask(config).mask($elementos);

        console.log(`✅ Máscara aplicada a ${$elementos.length} elemento(s)`);
        return $elementos;
    }

    /**
     * ✅ Aplica máscara de cantidad (sin decimales)
     * @param {string|jQuery} selector - Selector CSS o elemento jQuery
     * @param {Object} opciones - Opciones adicionales para override
     * @returns {jQuery} - Elementos enmascarados
     */
    function aplicarMascaraCantidad(selector, opciones = {}) {
        console.log(`🎭 Aplicando máscara CANTIDAD a: ${selector}`);

        const config = { ...configuracionCantidad, ...opciones };
        const $elementos = $(selector);

        if ($elementos.length === 0) {
            console.warn(`⚠️ No se encontraron elementos para: ${selector}`);
            return $elementos;
        }

        Inputmask(config).mask($elementos);

        console.log(`✅ Máscara aplicada a ${$elementos.length} elemento(s)`);
        return $elementos;
    }

    /**
     * ✅ Obtiene el valor numérico sin formato de un input enmascarado
     * @param {string|jQuery} selector - Selector CSS o elemento jQuery
     * @returns {number} - Valor numérico limpio
     */
    function obtenerValorNumerico(selector) {
        const $input = $(selector);

        if ($input.length === 0) {
            console.warn(`⚠️ Elemento no encontrado: ${selector}`);
            return 0;
        }

        const valorEnmascarado = $input.val();

        if (!valorEnmascarado || valorEnmascarado.trim() === '') {
            return 0;
        }

        // Remover prefijos/sufijos
        let valorLimpio = valorEnmascarado
            .replace(/\$/g, '')
            .replace(/USD/g, '')
            .replace(/%/g, '')
            .trim();

        // Remover separadores de miles (puntos)
        valorLimpio = valorLimpio.replace(/\./g, '');

        // Reemplazar coma decimal por punto
        valorLimpio = valorLimpio.replace(',', '.');

        const valorNumerico = parseFloat(valorLimpio);

        if (isNaN(valorNumerico)) {
            console.error(`❌ Error al parsear valor: "${valorEnmascarado}"`);
            return 0;
        }

        console.log(`✅ Valor parseado: "${valorEnmascarado}" → ${valorNumerico}`);
        return valorNumerico;
    }

    /**
     * ✅ Establece un valor numérico en un input enmascarado
     * @param {string|jQuery} selector - Selector CSS o elemento jQuery
     * @param {number} valor - Valor numérico a establecer
     */
    function establecerValor(selector, valor) {
        const $input = $(selector);

        if ($input.length === 0) {
            console.warn(`⚠️ Elemento no encontrado: ${selector}`);
            return;
        }

        if (isNaN(valor) || valor === null || valor === undefined) {
            valor = 0;
        }

        // Convertir a formato argentino (con coma decimal)
        const valorFormateado = parseFloat(valor).toFixed(2).replace('.', ',');

        $input.val(valorFormateado);
        $input.trigger('input'); // Disparar evento para actualizar máscara

        console.log(`✅ Valor establecido: ${valor} → "${valorFormateado}"`);
    }

    /**
     * ✅ Remueve la máscara de un input
     * @param {string|jQuery} selector - Selector CSS o elemento jQuery
     */
    function removerMascara(selector) {
        const $input = $(selector);

        if ($input.length === 0) {
            console.warn(`⚠️ Elemento no encontrado: ${selector}`);
            return;
        }

        Inputmask.remove($input[0]);
        console.log(`✅ Máscara removida de: ${selector}`);
    }

    // ═══════════════════════════════════════════════════════════════
    // API PÚBLICA
    // ═══════════════════════════════════════════════════════════════

    return {
        // Aplicar máscaras
        aplicarMascaraPesos,
        aplicarMascaraDolares,
        aplicarMascaraMonetaria,
        aplicarMascaraPorcentaje,
        aplicarMascaraCantidad,

        // Manipular valores
        obtenerValorNumerico,
        establecerValor,
        removerMascara,

        // Acceso a configuraciones (para casos avanzados)
        configuraciones: {
            pesos: configuracionPesos,
            dolares: configuracionDolares,
            monetaria: configuracionSinPrefijo,
            porcentaje: configuracionPorcentaje,
            cantidad: configuracionCantidad
        }
    };
})();

// ═══════════════════════════════════════════════════════════════
// EXPORTAR PARA USO GLOBAL
// ═══════════════════════════════════════════════════════════════

window.InputMaskMonetario = InputMaskMonetario;

console.log('✅ Módulo InputMaskMonetario cargado correctamente');