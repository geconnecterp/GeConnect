$(function () {
    // Mostrar mensajes de TempData
    if (MensajeErrorTempData && MensajeErrorTempData !== '') {
        mostrarMensaje('error', MensajeErrorTempData);
    }
    if (MensajeInfoTempData && MensajeInfoTempData !== '') {
        mostrarMensaje('info', MensajeInfoTempData);
    }
    if (MensajeWarnTempData && MensajeWarnTempData !== '') {
        mostrarMensaje('warning', MensajeWarnTempData);
    }
    if (MensajeSuccessTempData && MensajeSuccessTempData !== '') {
        mostrarMensaje('success', MensajeSuccessTempData);
    }

    // Toggle password visibility
    $('#togglePassword').on('click', function () {
        const passwordInput = $('#Password');
        const toggleIcon = $('#toggleIcon');
        const type = passwordInput.attr('type') === 'password' ? 'text' : 'password';
        passwordInput.attr('type', type);
        toggleIcon.toggleClass('bx-hide bx-show');
    });

    // Validación del formulario
    $('#formAuthentication').on('submit', function (e) {
        e.preventDefault();

        // Validaciones
        /*const admid = $('#Admid').val();*/
        const username = $('#UserName').val().trim();
        const password = $('#Password').val().trim();

        //if (!admid || admid === '') {
        //    mostrarMensaje('warning', 'Por favor, seleccione una administración');
        //    return false;
        //}

        if (username === '') {
            mostrarMensaje('warning', 'Por favor, ingrese su usuario');
            $('#UserName').trigger("focus");
            return false;
        }

        if (password === '') {
            mostrarMensaje('warning', 'Por favor, ingrese su contraseña');
            $('#Password').trigger("focus");
            return false;
        }

        // Mostrar loading overlay
        $('#loadingOverlay').addClass('active');
        $('#btnLogin').prop('disabled', true);

        // Submit del formulario
        this.submit();
    });

    // Enter key en campos
    $('#UserName, #Password, #Admid').on('keypress', function (e) {
        if (e.which === 13) {
            $('#formAuthentication').submit();
        }
    });
});

function mostrarMensaje(tipo, mensaje) {
    const iconos = {
        'error': 'bx-error-circle',
        'warning': 'bx-error',
        'info': 'bx-info-circle',
        'success': 'bx-check-circle'
    };

    const colores = {
        'error': 'danger',
        'warning': 'warning',
        'info': 'info',
        'success': 'success'
    };

    const html = `
                <div class="alert alert-${colores[tipo]} alert-dismissible fade show" role="alert">
                    <i class='bx ${iconos[tipo]} me-2'></i>
                    <strong>${mensaje}</strong>
                    <button type="button" class="btn-close" data-bs-dismiss="alert" aria-label="Close"></button>
                </div>
            `;

    $('#messageContainer').html(html);

    // Auto-dismiss después de 5 segundos
    setTimeout(function () {
        $('.alert').fadeOut('slow', function () {
            $(this).remove();
        });
    }, 5000);
}