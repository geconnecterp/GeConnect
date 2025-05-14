$(function () {
    // Función genérica para habilitar/deshabilitar componentes
    function toggleComponent(checkboxId, componentSelector) {
        try {
            const isChecked = $(`#${checkboxId}`).is(':checked');
            const $component = $(componentSelector);

            if (isChecked) {
                $component.prop('disabled', false).css({
                    'background-color': '',
                    'font-weight': 'normal'
                });
            } else {
                $component.prop('disabled', true).css({
                    'background-color': 'rgb(251,255,195)',
                    'font-weight': '900'
                });
            }
        } catch (error) {
            console.error(`Error al procesar el checkbox ${checkboxId}:`, error);
        }
    }

    // Eventos para activar/desactivar componentes
    $('#chkEjercicio').on('change', function () {
        toggleComponent('chkEjercicio', '#Eje_nro');
    });

    $('#Mov').on('change', function () {
        toggleComponent('Mov', '#Mov_like');
    });

    $('#Usu').on('change', function () {
        toggleComponent('Usu', '#Usu_like');
    });

    $('#Tipo').on('change', function () {
        toggleComponent('Tipo', '#Tipo_like');
    });

    $('#Rango').on('change', function () {
        toggleComponent('Rango', 'input[name="Desde"]');
        toggleComponent('Rango', 'input[name="Hasta"]');
    });

    // Inicialización: Deshabilitar componentes si los checkboxes no están marcados
    toggleComponent('chkEjercicio', '#Eje_nro');
    toggleComponent('Mov', '#Mov_like');
    toggleComponent('Usu', '#Usu_like');
    toggleComponent('Tipo', '#Tipo_like');
    toggleComponent('Rango', 'input[name="Desde"]');
    toggleComponent('Rango', 'input[name="Hasta"]');




    $("#divFiltro").collapse("show");
})