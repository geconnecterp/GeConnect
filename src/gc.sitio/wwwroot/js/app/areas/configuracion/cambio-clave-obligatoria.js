$(function () {
    const $form = $('#formClaveObligatoria');
    const $nueva = $('#ClaveNueva');
    const $confirmacion = $('#ConfirmacionClave');
    const $mensaje = $('#mensajeClaveObligatoria');
    const flag = name => String($form.data(name)).toLowerCase() === 'true';
    const min = Number($form.data('longitud-minima')) || 1;
    const max = Number($form.data('longitud-maxima')) || 128;

    function mostrar(mensaje, error) {
        $mensaje.removeClass('d-none alert-success alert-warning')
            .addClass(error ? 'alert-warning' : 'alert-success').text(mensaje);
    }
    function regla(nombre, valida) { $(`.policy-list li[data-rule="${nombre}"]`).toggleClass('valid', valida); }
    function actualizar() {
        const clave = $nueva.val();
        const confirmacion = $confirmacion.val();
        regla('length', clave.length >= min && clave.length <= max);
        regla('upper', /[A-Z]/.test(clave)); regla('lower', /[a-z]/.test(clave));
        regla('number', /[0-9]/.test(clave)); regla('symbol', /[^A-Za-z0-9 ]/.test(clave));
        regla('match', clave.length > 0 && clave === confirmacion);
        $('#coincidenciaClave').text(!confirmacion ? '' : (clave === confirmacion ? 'Las contraseñas coinciden.' : 'Las contraseñas no coinciden.'))
            .toggleClass('match', !!confirmacion && clave === confirmacion)
            .toggleClass('no-match', !!confirmacion && clave !== confirmacion);
    }
    function validar() {
        const clave = $nueva.val();
        if (!clave || /^\s*$/.test(clave)) return 'Debe ingresar la contraseña nueva.';
        if (flag('validar-longitud') && (clave.length < min || clave.length > max)) return `La contraseña debe tener entre ${min} y ${max} caracteres.`;
        if (flag('validar-complejidad') && flag('mayuscula') && !/[A-Z]/.test(clave)) return 'La contraseña debe incluir al menos una letra mayúscula.';
        if (flag('validar-complejidad') && flag('minuscula') && !/[a-z]/.test(clave)) return 'La contraseña debe incluir al menos una letra minúscula.';
        if (flag('validar-complejidad') && flag('numero') && !/[0-9]/.test(clave)) return 'La contraseña debe incluir al menos un número.';
        if (flag('validar-complejidad') && flag('simbolo') && !/[^A-Za-z0-9 ]/.test(clave)) return 'La contraseña debe incluir al menos un símbolo.';
        if (clave !== $confirmacion.val()) return 'La confirmación no coincide con la contraseña nueva.';
        return null;
    }

    $('.password-toggle').on('click', function () {
        const $input = $('#' + $(this).data('target'));
        const mostrarClave = $input.attr('type') === 'password';
        $input.attr('type', mostrarClave ? 'text' : 'password');
        $(this).find('i').toggleClass('bx-show', !mostrarClave).toggleClass('bx-hide', mostrarClave);
    });
    $nueva.add($confirmacion).on('input', actualizar);
    $form.on('submit', function (event) {
        event.preventDefault();
        const error = validar();
        if (error) { mostrar(error, true); return; }

        const $boton = $('#btnGuardarClave');
        $boton.prop('disabled', true).html('<i class="bx bx-loader-alt bx-spin"></i><span>Procesando</span>');
        $.ajax({ url: $form.attr('action'), type: 'POST', data: $form.serialize() })
            .done(function (respuesta) {
                mostrar(respuesta.msg, !respuesta.ok);
                if (respuesta.ok) {
                    $form.find('input, button').prop('disabled', true);
                    setTimeout(function () { window.location.assign(respuesta.redirect); }, 1200);
                } else {
                    $boton.prop('disabled', false).html('<i class="bx bx-check-circle"></i><span>Confirmar</span>');
                    if (respuesta.focus) $('#' + respuesta.focus).trigger('focus');
                }
            })
            .fail(function () {
                mostrar('No se pudo completar el cambio obligatorio de contraseña.', true);
                $boton.prop('disabled', false).html('<i class="bx bx-check-circle"></i><span>Confirmar</span>');
            });
    });
    actualizar();
});
