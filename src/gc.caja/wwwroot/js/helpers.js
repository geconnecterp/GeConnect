/*!
 * Helpers JS - Funciones auxiliares del sistema
 */

/**
 * Helper functions para el sistema
 */
(function () {
    'use strict';

    // Helper para detectar tema oscuro/claro
    window.Helpers = {
        /**
         * Inicializa el tema
         */
        initTheme: function () {
            const defaultTheme = 'light';
            const savedTheme = localStorage.getItem('theme') || defaultTheme;
            document.documentElement.setAttribute('data-bs-theme', savedTheme);
        },

        /**
         * Cambia el tema
         */
        setTheme: function (theme) {
            localStorage.setItem('theme', theme);
            document.documentElement.setAttribute('data-bs-theme', theme);
        },

        /**
         * Detecta si es dispositivo móvil
         */
        isMobile: function () {
            return /Android|webOS|iPhone|iPad|iPod|BlackBerry|IEMobile|Opera Mini/i.test(navigator.userAgent);
        },

        /**
         * Detecta si es touch device
         */
        isTouchDevice: function () {
            return 'ontouchstart' in window || navigator.maxTouchPoints > 0;
        },

        /**
         * Auto-inicialización
         */
        autoInit: function () {
            this.initTheme();
        }
    };

    // Auto-ejecutar al cargar
    if (document.readyState === 'loading') {
        document.addEventListener('DOMContentLoaded', function () {
            window.Helpers.autoInit();
        });
    } else {
        window.Helpers.autoInit();
    }
})();

/**
 * Utility para formatear fechas
 */
window.formatDate = function (date) {
    if (!(date instanceof Date)) {
        date = new Date(date);
    }
    const day = String(date.getDate()).padStart(2, '0');
    const month = String(date.getMonth() + 1).padStart(2, '0');
    const year = date.getFullYear();
    return `${day}/${month}/${year}`;
};

/**
 * Utility para validar email
 */
window.isValidEmail = function (email) {
    const re = /^[^\s@]+@[^\s@]+\.[^\s@]+$/;
    return re.test(String(email).toLowerCase());
};

/**
 * Utility para scroll suave
 */
window.smoothScrollTo = function (element) {
    if (typeof element === 'string') {
        element = document.querySelector(element);
    }
    if (element) {
        element.scrollIntoView({ behavior: 'smooth', block: 'start' });
    }
};

/**
 * Debounce function
 */
window.debounce = function (func, wait) {
    let timeout;
    return function executedFunction(...args) {
        const later = () => {
            clearTimeout(timeout);
            func(...args);
        };
        clearTimeout(timeout);
        timeout = setTimeout(later, wait);
    };
};

/**
 * Throttle function
 */
window.throttle = function (func, limit) {
    let inThrottle;
    return function () {
        const args = arguments;
        const context = this;
        if (!inThrottle) {
            func.apply(context, args);
            inThrottle = true;
            setTimeout(() => inThrottle = false, limit);
        }
    };
};

console.log('✅ Helpers.js cargado correctamente');