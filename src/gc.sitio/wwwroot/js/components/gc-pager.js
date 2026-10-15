// Opt-in, sin estado global de consulta ni peticiones HTTP.
// Cada módulo atiende gc:page en su contenedor y conserva sus filtros/bloqueos.
(() => {
    'use strict';
    function navigate(pager, page) {
        if (!pager || pager.closest('[aria-busy="true"]')) return;
        const total = Number(pager.dataset.pages);
        if (!Number.isSafeInteger(page) || page < 1 || page > total || page === Number(pager.dataset.current)) return;
        pager.dispatchEvent(new CustomEvent('gc:page', { bubbles: true, detail: { page } }));
    }
    function jump(pager) {
        const input = pager?.querySelector('[data-gc-page-input]');
        if (!input || input.disabled || pager.closest('[aria-busy="true"]')) return;
        const page = Number(input.value);
        const total = Number(pager.dataset.pages);
        const valid = /^\d+$/.test(input.value) && Number.isSafeInteger(page) && page >= 1 && page <= total;
        input.setCustomValidity(valid ? '' : `Ingrese una página entera entre 1 y ${total}.`);
        input.setAttribute('aria-invalid', String(!valid));
        if (!input.reportValidity()) return;
        navigate(pager, page);
    }
    document.addEventListener('click', event => {
        const button = event.target.closest('.gc-pager button');
        if (!button || button.disabled) return;
        const pager = button.closest('[data-gc-pager]');
        if (button.hasAttribute('data-gc-page-go')) jump(pager);
        else if (button.hasAttribute('data-page')) navigate(pager, Number(button.dataset.page));
    });
    document.addEventListener('keydown', event => {
        if (event.key !== 'Enter' || !event.target.matches('.gc-pager [data-gc-page-input]')) return;
        event.preventDefault();
        jump(event.target.closest('[data-gc-pager]'));
    });
    document.addEventListener('input', event => {
        if (!event.target.matches('.gc-pager [data-gc-page-input]')) return;
        event.target.setCustomValidity('');
        event.target.removeAttribute('aria-invalid');
    });
})();
