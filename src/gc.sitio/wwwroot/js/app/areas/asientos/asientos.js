$(function () {
    
    $("#btnBuscar").on("click", function () {

        //es nueva la busqueda no resguardamos la busqueda anterior. es util para paginado
        $("#divpanel01").empty();
        dataBak = "";
        //es una busqueda por filtro. siempre sera pagina 1
        pagina = 1;
        buscarAsientos(pagina);
    });
    //callback para que funcione la paginación
    funcCallBack = buscarAsientos;

    // Eventos para activar/desactivar componentes
    $('#chkEjercicio').on('change', function () {
        if (typeof asientoTemporal !== 'undefined' && asientoTemporal === true) {
            // Si está en modo temporal, no ejecutar y desactivar el checkbox
            //$('#chkEjercicio').prop('checked', false).prop('disabled', true);
            return;
        }
        toggleComponent('chkEjercicio', '#Eje_nro');
    });

    $('#Movi').on('change', function () {
        toggleComponent('Movi', '#Movi_like');
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

    InicializaVista();
})

function InicializaVista() {

    // Inicialización: Deshabilitar componentes si los checkboxes no están marcados
    if (typeof asientoTemporal !== 'undefined' && asientoTemporal === true) {
        // Si está en modo temporal, no ejecutary desactivar el checkbox
        $('#chkEjercicio').prop('checked', true).prop('disabled', true);
        $("#Eje_nro").prop("disabled", true);
    }
    else {
        toggleComponent('chkEjercicio', '#Eje_nro');
    }

    toggleComponent('Movi', '#Movi_like');
    toggleComponent('Usu', '#Usu_like');
    toggleComponent('Tipo', '#Tipo_like');
    toggleComponent('Rango', 'input[name="Desde"]');
    toggleComponent('Rango', 'input[name="Hasta"]');

    $("#divFiltro").collapse("show");
    $("#divDetalle").collapse("hide");
}

function buscarAsientos(pagina) {
    AbrirWaiting();
    //desactivamos los botones de acción
    activarBotones2(false);

    // Obtenemos los valores de los campos del filtro
    var data1 = {
        Eje_nro: $("#Eje_nro").val(), // Ejercicio
        Movi: $("#Movi").is(":checked"), // bit Movimiento
        Movi_like: $("#Movi_like").val(), // Nro Movimiento
        Usu: $("#Usu").is(":checked"), // bit Usuario
        Usu_like: $("#Usu_like").val(), // Usuario
        Tipo: $("#Tipo").is(":checked"), // bit Tipo
        Tipo_like: $("#Tipo_like").val(), // Tipo de Asiento
        Rango: $("#Rango").is(":checked"), // bit Rango
        Desde: $("input[name='Desde']").val(), // Fecha Desde
        Hasta: $("input[name='Hasta']").val() // Fecha Hasta
    };

    var buscaNew = JSON.stringify(dataBak) != JSON.stringify(data1)

    if (buscaNew === false) {
        //son iguales las condiciones cambia de pagina
        pagina = pag;
    }
    else {
        dataBak = data1;
        pagina = 1;
        pag = 1;
    }

    var sort = null;
    var sortDir = null

    var data2 = { sort, sortDir, pag, buscaNew }

    var data = $.extend({}, data1, data2);

    PostGenHtml(data, buscarAsientoUrl, function (obj) {
        $("#divGrilla").html(obj);
        $("#divFiltro").collapse("hide")
        PostGen({}, buscarMetadataURL, function (obj) {
            if (obj.error === true) {
                AbrirMensaje("ATENCIÓN", obj.msg, function () {
                    $("#msjModal").modal("hide");
                    return true;
                }, false, ["Aceptar"], "error!", null);
            }
            else {
                totalRegs = obj.metadata.totalCount;
                pags = obj.metadata.totalPages;
                pagRegs = obj.metadata.pageSize;

                $("#pagEstado").val(true).trigger("change");
            }

        });
        CerrarWaiting();
    }, function (obj) {
        ControlaMensajeError(obj.message);
        CerrarWaiting();
    });
};



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