$(function () {
    const $form = $('#formCambioClave');
    const $actual = $('#ClaveActual');
    const $nueva = $('#ClaveNueva');
    const $confirmacion = $('#ConfirmacionClave');
    const flag = name => String($form.data(name)).toLowerCase() === 'true';
    const min = Number($form.data('longitud-minima')) || 1;
    const max = Number($form.data('longitud-maxima')) || 128;

    function setRule(rule, valid) {
        $(`.policy-list li[data-rule="${rule}"]`).toggleClass('valid', valid);
    }

    function updateRules() {
        const actual = $actual.val();
        const nueva = $nueva.val();
        const confirmacion = $confirmacion.val();
        setRule('length', nueva.length >= min && nueva.length <= max);
        setRule('upper', /[A-Z]/.test(nueva));
        setRule('lower', /[a-z]/.test(nueva));
        setRule('number', /[0-9]/.test(nueva));
        setRule('symbol', /[^A-Za-z0-9 ]/.test(nueva));
        setRule('different', nueva.length > 0 && nueva !== actual);

        const $match = $('#coincidenciaClave');
        if (!confirmacion) $match.text('').removeClass('match no-match');
        else if (nueva === confirmacion) $match.text('Las contraseñas coinciden.').removeClass('no-match').addClass('match');
        else $match.text('Las contraseñas no coinciden.').removeClass('match').addClass('no-match');
    }

    function clientMessage() {
        const actual = $actual.val();
        const nueva = $nueva.val();
        if (!actual || /^\s*$/.test(actual)) return { msg: 'Debe ingresar la contraseña actual.', field: $actual };
        if (!nueva || /^\s*$/.test(nueva)) return { msg: 'Debe ingresar la contraseña nueva.', field: $nueva };
        if (flag('validar-longitud') && (nueva.length < min || nueva.length > max)) return { msg: `La contraseña debe tener entre ${min} y ${max} caracteres.`, field: $nueva };
        if (flag('distinta') && nueva === actual) return { msg: 'La contraseña nueva debe ser diferente de la actual.', field: $nueva };
        if (flag('validar-complejidad') && flag('mayuscula') && !/[A-Z]/.test(nueva)) return { msg: 'La contraseña debe incluir al menos una letra mayúscula.', field: $nueva };
        if (flag('validar-complejidad') && flag('minuscula') && !/[a-z]/.test(nueva)) return { msg: 'La contraseña debe incluir al menos una letra minúscula.', field: $nueva };
        if (flag('validar-complejidad') && flag('numero') && !/[0-9]/.test(nueva)) return { msg: 'La contraseña debe incluir al menos un número.', field: $nueva };
        if (flag('validar-complejidad') && flag('simbolo') && !/[^A-Za-z0-9 ]/.test(nueva)) return { msg: 'La contraseña debe incluir al menos un símbolo.', field: $nueva };
        if (nueva !== $confirmacion.val()) return { msg: 'La confirmación no coincide con la contraseña nueva.', field: $confirmacion };
        return null;
    }

    $('.password-toggle').on('click', function () {
        const $input = $('#' + $(this).data('target'));
        const show = $input.attr('type') === 'password';
        $input.attr('type', show ? 'text' : 'password');
        $(this).find('i').toggleClass('bx-show', !show).toggleClass('bx-hide', show);
        $(this).attr('aria-label', show ? 'Ocultar contraseña' : 'Mostrar contraseña');
    });

    $actual.add($nueva).add($confirmacion).on('input', updateRules);

    $form.on('submit', function (event) {
        event.preventDefault();
        const invalid = clientMessage();
        if (invalid) {
            ControlaMensajeWarning(invalid.msg);
            invalid.field.trigger('focus');
            return;
        }

        const $button = $('#btnGuardarClave');
        const original = $button.html();
        $button.prop('disabled', true).html('<i class="bx bx-loader-alt bx-spin"></i><span>Procesando</span>');
        AbrirWaiting('Modificando contraseña...');

        $.ajax({ url: $form.attr('action'), type: 'POST', data: $form.serialize() })
            .done(function (response) {
                if (response.ok) {
                    $form.find('input, button').prop('disabled', true);
                    ControlaMensajeSuccess(response.msg);
                    setTimeout(function () { window.location.assign(response.redirect); }, 1300);
                    return;
                }
                if (response.warn) ControlaMensajeWarning(response.msg);
                else ControlaMensajeError(response.msg);
                if (response.focus) $('#' + response.focus).trigger('focus');
            })
            .fail(function () { ControlaMensajeError('No se pudo completar el cambio de contraseña.'); })
            .always(function () {
                CerrarWaiting();
                if (!$form.find('input').first().prop('disabled')) $button.prop('disabled', false).html(original);
            });
    });

    updateRules();
});
