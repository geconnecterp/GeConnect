// Sólo se ejecuta al usar el menú; no consulta la API ni inspecciona grillas en cada pulsación.
(function () {
    let consultaAbierta = false;
    document.addEventListener('click', function (event) {
        const enlace = event.target.closest('a');
        if (!enlace) return;
        if (window.PocketCuentaEnviando || (typeof confirmacionSeguraActiva !== 'undefined' && confirmacionSeguraActiva !== null)) {
            event.preventDefault(); event.stopImmediatePropagation(); return;
        }
        if (!enlace.hasAttribute('data-pocket-account-nav') ||
            !document.body.classList.contains('pocket-body') || document.getElementById('formCuentaClave')) return;
        // Advertencia conservadora: cada módulo conserva su propio mecanismo de carga.
        event.preventDefault(); event.stopImmediatePropagation();
        if (consultaAbierta) return;
        consultaAbierta = true;
        AbrirMensaje('Antes de salir',
            'Verifique que no queden cargas sin confirmar. Los datos pendientes no se guardarán automáticamente al abrir su cuenta o cerrar la sesión. ¿Desea continuar?',
            function (opcion) {
                consultaAbierta = false;
                $('#msjModal').modal('hide');
                if (opcion === 'SI') window.location.assign(enlace.href);
            }, true, ['Continuar', 'Cancelar'], 'warn!', null, 'cancelar');
    }, true);
    window.addEventListener('beforeunload', function (event) {
        if (window.PocketCuentaEnviando) { event.preventDefault(); event.returnValue = ''; }
    });
    $(document).ajaxError(function (_event, xhr) {
        const r = xhr.responseJSON;
        if (r && r.cambioClave && r.redirect) window.location.assign(r.redirect);
    });
})();
