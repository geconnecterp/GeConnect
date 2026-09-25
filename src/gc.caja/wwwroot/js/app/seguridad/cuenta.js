// Las reglas orientan al operador; la API y el SP validan siempre la política vigente.
function reglasClaveCuenta(actual, nueva, politica) {
    return {
        length: nueva.length >= politica.min && nueva.length <= politica.max,
        upper: /[A-Z]/.test(nueva), lower: /[a-z]/.test(nueva),
        number: /[0-9]/.test(nueva), symbol: /[^A-Za-z0-9 ]/.test(nueva),
        different: nueva.length > 0 && nueva !== actual
    };
}
function validarClaveCuenta(actual, nueva, confirmacion, politica) {
    if (!politica.obligatoria && !actual.trim()) return { msg: 'Ingresá tu contraseña actual.', focus: 'ClaveActual' };
    if (!nueva.trim()) return { msg: 'Ingresá la nueva contraseña.', focus: 'ClaveNueva' };
    if (actual.length > 128 || nueva.length > 128 || confirmacion.length > 128) return { msg: 'La contraseña admite hasta 128 caracteres.', focus: 'ClaveNueva' };
    const rules = reglasClaveCuenta(actual, nueva, politica);
    if (politica.longitud && !rules.length) return { msg: `Usá entre ${politica.min} y ${politica.max} caracteres.`, focus: 'ClaveNueva' };
    if (!politica.obligatoria && politica.distinta && !rules.different) return { msg: 'La nueva contraseña debe ser diferente de la actual.', focus: 'ClaveNueva' };
    for (const [key, label] of [['upper','una mayúscula'],['lower','una minúscula'],['number','un número'],['symbol','un símbolo']]) {
        if (politica.complejidad && politica[key] && !rules[key]) return { msg: `Incluí al menos ${label}.`, focus: 'ClaveNueva' };
    }
    if (nueva !== confirmacion) return { msg: 'La confirmación no coincide con la nueva contraseña.', focus: 'ConfirmacionClave' };
    return null;
}
$(function () {
    const $form = $('#formCuentaClave');
    if (!$form.length) return;
    const flag = key => String($form.attr('data-' + key)) === 'true';
    const politica = {
        obligatoria: flag('obligatoria'), longitud: flag('validar-longitud'),
        min: Number($form.attr('data-longitud-minima')), max: Number($form.attr('data-longitud-maxima')),
        complejidad: flag('validar-complejidad'), upper: flag('mayuscula'), lower: flag('minuscula'),
        number: flag('numero'), symbol: flag('simbolo'), distinta: flag('distinta')
    };
    const $guardar = $('#cuentaGuardar'), $mensaje = $('#cuentaMensaje');
    const original = $guardar.html();
    let ocupado = false, terminado = false;
    const valores = () => [$('#ClaveActual').val() || '', $('#ClaveNueva').val() || '', $('#ConfirmacionClave').val() || ''];
    function mensaje(texto, tipo) {
        $mensaje.removeClass('d-none alert-warning alert-danger alert-success').addClass('alert-' + tipo).text(texto);
    }
    function bloquear(bloqueado) {
        $form.attr('aria-busy', String(bloqueado));
        $form.find('input, button').prop('disabled', bloqueado);
        $('.cuenta-actions a, .cuenta-heading a, .cuenta-operador button').attr('aria-disabled', String(bloqueado)).toggleClass('disabled', bloqueado);
        $guardar.html(bloqueado ? '<i class="bx bx-loader-alt bx-spin" aria-hidden="true"></i> Guardando…' : original);
    }
    function actualizar() {
        const [actual, nueva, confirmacion] = valores();
        const rules = reglasClaveCuenta(actual, nueva, politica);
        Object.entries(rules).forEach(([key, valid]) => $form.find('[data-rule="' + key + '"]').toggleClass('valid', valid));
        $('#cuentaCoincidencia').text(!confirmacion ? '' : nueva === confirmacion ? 'Las contraseñas coinciden.' : 'Las contraseñas no coinciden.')
            .toggleClass('match', !!confirmacion && nueva === confirmacion).toggleClass('no-match', !!confirmacion && nueva !== confirmacion);
    }
    $('.cuenta-toggle').on('click', function () {
        const $input = $('#' + $(this).attr('data-target'));
        const mostrar = $input.attr('type') === 'password';
        $input.attr('type', mostrar ? 'text' : 'password');
        $(this).attr('aria-pressed', String(mostrar)).attr('aria-label', (mostrar ? 'Ocultar ' : 'Mostrar ') + $('label[for="' + $input.attr('id') + '"]').text().toLowerCase());
        $(this).find('i').toggleClass('bx-show', !mostrar).toggleClass('bx-hide', mostrar);
    });
    $form.find('input').on('input', actualizar);
    // Enter recorre los campos; el último confirma una sola vez.
    $form.find('input[type="password"]').on('keydown', function (event) {
        if (event.key !== 'Enter' || event.isComposing) return;
        event.preventDefault();
        const inputs = $form.find('input:not([type="hidden"])');
        const siguiente = inputs.eq(inputs.index(this) + 1);
        if (siguiente.length) siguiente.trigger('focus'); else $form.trigger('submit');
    });
    $(document).on('click.cuenta', 'a[aria-disabled="true"], button[aria-disabled="true"]', event => event.preventDefault());
    $form.on('submit', function (event) {
        event.preventDefault();
        if (ocupado || terminado) return;
        const invalid = validarClaveCuenta(...valores(), politica);
        if (invalid) { mensaje(invalid.msg, 'warning'); $('#' + invalid.focus).trigger('focus'); return; }
        const data = $form.serialize(); // Incluye antiforgery y conserva exactamente espacios y mayúsculas.
        ocupado = true;
        bloquear(true);
        $mensaje.addClass('d-none');
        $.ajax({ url: $form.attr('action'), type: 'POST', data, dataType: 'json', global: false, timeout: 45000 })
            .done(function (response) {
                if (!response || typeof response.ok !== 'boolean') {
                    terminado = true;
                    $form.find('input:not([type="hidden"])').val('');
                    mensaje('No se pudo comprobar el resultado. Volvé a ingresar antes de reintentar.', 'warning');
                    $('#cuentaReingresar').removeClass('d-none');
                    $guardar.text('Resultado sin confirmar');
                    return;
                }
                if (response.ok) {
                    terminado = true;
                    $form.find('input:not([type="hidden"])').val('');
                    mensaje(response.msg, 'success');
                    $guardar.text('Contraseña actualizada');
                    setTimeout(() => window.location.assign(response.redirect || $form.attr('data-login')), 1300);
                } else {
                    mensaje(response.msg || 'No se pudo modificar la contraseña.', response.warn ? 'warning' : 'danger');
                    bloquear(false);
                    if (['ClaveActual', 'ClaveNueva', 'ConfirmacionClave'].includes(response.focus)) $('#' + response.focus).trigger('focus');
                }
            })
            .fail(function (xhr) {
                if ([401, 403, 440].includes(xhr.status)) {
                    terminado = true;
                    window.location.assign(xhr.responseJSON?.redirect || $form.attr('data-login'));
                } else if (xhr.status === 400) {
                    mensaje('La solicitud venció o es inválida. Recargá la página antes de reintentar.', 'warning');
                    terminado = true;
                    $('#cuentaReingresar').removeClass('d-none');
                } else {
                    terminado = true;
                    $form.find('input:not([type="hidden"])').val('');
                    mensaje('No se pudo comprobar el resultado. Volvé a ingresar para verificar tu acceso antes de reintentar.', 'warning');
                    $('#cuentaReingresar').removeClass('d-none');
                    $guardar.text('Resultado sin confirmar');
                }
            })
            .always(function () { ocupado = false; if (!terminado) bloquear(false); });
    });
    actualizar();
});
