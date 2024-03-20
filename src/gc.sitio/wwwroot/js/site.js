$(document).ready(function () {
    desabilitarRetroceso();
    var exeUnlock = true;
    setTimeout($.unblockUI, 15000);

    //$(document).ajaxStart(function () {
    //    $.blockUI({
    //        overlayCSS: { backgroundColor: '#00f' },
    //        message: "<h1>Por favor, espere un instante... </h1>"
    //    });
    //}).ajaxStop($.unblockUI());

    $.datepicker.regional['es'] = {
        closeText: 'Cerrar',
        prevText: '< Ant',
        nextText: 'Sig >',
        currentText: 'Hoy',
        monthNames: ['Enero', 'Febrero', 'Marzo', 'Abril', 'Mayo', 'Junio', 'Julio', 'Agosto', 'Septiembre', 'Octubre', 'Noviembre', 'Diciembre'],
        monthNamesShort: ['Ene', 'Feb', 'Mar', 'Abr', 'May', 'Jun', 'Jul', 'Ago', 'Sep', 'Oct', 'Nov', 'Dic'],
        dayNames: ['Domingo', 'Lunes', 'Martes', 'Miércoles', 'Jueves', 'Viernes', 'Sábado'],
        dayNamesShort: ['Dom', 'Lun', 'Mar', 'Mié', 'Juv', 'Vie', 'Sáb'],
        dayNamesMin: ['Do', 'Lu', 'Ma', 'Mi', 'Ju', 'Vi', 'Sá'],
        weekHeader: 'Sm',
        dateFormat: 'dd/mm/yy',
        firstDay: 1,
        isRTL: false,
        showMonthAfterYear: false,
        yearSuffix: ''
    };
    $.datepicker.setDefaults($.datepicker.regional['es']);
    $(".datepicker").datepicker({
        dateFormat: 'dd/mm/yy',
        minDate: -30,
        maxDate: '+1M',
        showWeek: false,
        autohide: true,

    });


    $("input.form-control").on("focus", function () { $(this).select(); })

    //el control de este código es para obtener el texto de la transición elegida.
    $(".btnNext").click(function () {
        var btn = $(this);
        var formu = btn.attr("relacion");
        //busco el texto y lo inserto en el input hidden        
        $("#form" + formu + " input#descr" + formu).val($("textarea#descripcion").val());

        //verifico el campo pedidoParcial. si existe verifico el valor que tiene
        var parcial = $("input#pedido" + formu + "");
        if ($("input#pedido" + formu + "") !== undefined) {
            //verificamos el valor del componente
            if ($("input#pedido" + formu + "").val() === "True") {
                $("input#tipoValor" + formu + "").val($("input#vo_TipoValorId").val());
            }
        }

        Bloquear();
        $("#form" + formu).submit();

    });

    //$("#btnAddLodi").click(function () {

    //    if ($("input#valores_2__Adicion").is(":hidden")) {
    //        $("input#valores_2__Adicion").val(0.5);
    //        addFirst = true;
    //        $("input#valores_2__Adicion").show("fast");
    //    }
    //    else {
    //        $("input#valores_2__Adicion").hide("fast");
    //        $("input#valores_2__Adicion").val("");
    //        VerificaAdicion();
    //    }
    //})

    $("#btnGridHistoria").click(function () {
        $("#gridHistoriaEstados").modal("toggle");
    });

    $(".money").mask('#.##0,00', { reverse: true });

    InicializarPage();
});

function Bloquear() {
    $.blockUI({ overlayCSS: { backgroundColor: '#d3d3d3' }, message: MensajeBlock });
}

//desabiliatar el retroceso
function desabilitarRetroceso() {
    window.location.hash = "no-back-button";
    window.location.hash = "Again-No-back-button-" //chrome
    window.onhashchange = function () { window.location.hash = ""; }
}




/**
 * 
 * @param {any} data
 * @param {any} path
 * @param {any} retorno
 */

function PostGen(data, path, retorno) {

    $.ajax({
        "dataType": 'json',
        "url": path,
        "type": "POST",
        "data": data,
        "success": retorno,
        //beforeSend: function () { Bloquear();},
        error: fnError
    });
}

function fnError(jqXHR) {
    //alert(jqXHR);
    if (jqXHR.error) {
        ControlaMensajeError(jqXHR.error);
    }
    else {
        ControlaMensajeError(jqXHR);
    }
}

function InicializarPage() {
    if (MensajeErrorTempData)
        ControlaMensajeError(MensajeErrorTempData);
    if (MensajeInfoTempData)
        ControlaMensajeInfo(MensajeInfoTempData);
    if (MensajeWarnTempData)
        ControlaMensajeWarning(MensajeWarnTempData);
    if (MensajeSuccessTempData)
        ControlaMensajeSuccess(MensajeSuccessTempData)
}

function ControlaMensajeError(mensaje, unlock = false) {

    Lobibox.notify('error', {
        title: 'Error',
        pauseDelayOnHover: true,
        continueDelayOnInactiveTab: false,
        position: 'top right',
        icon: 'bx bx-x-circle',
        sound: false,
        msg: mensaje
    });

    if (unlock) {
        $.unblockUI();
    }
}

function ControlaMensajeInfo(mensaje, unlock = false) {

    Lobibox.notify('info', {
        title: 'Informaci&oacute;n',
        pauseDelayOnHover: true,
        continueDelayOnInactiveTab: false,
        position: 'top right',
        icon: 'bx bx-info-circle',
        sound: false,
        msg: mensaje
    });


    if (unlock) {
        $.unblockUI();
    }
}

function ControlaMensajeSuccess(mensaje, unlock = false) {

    Lobibox.notify('success', {
        title: 'Satisfactorio',
        pauseDelayOnHover: true,
        continueDelayOnInactiveTab: false,
        position: 'top right',
        icon: 'bx bx-check-circle',
        sound: false,
        msg: mensaje
    });


    if (unlock) {
        $.unblockUI();
    }
}

function ControlaMensajeWarning(mensaje, unlock = false) {

    Lobibox.notify('warning', {
        title: 'Atenci&oacute;n',
        pauseDelayOnHover: true,
        continueDelayOnInactiveTab: false,
        position: 'top right',
        icon: 'bx bx-error',
        sound: false,
        msg: mensaje
    });

    if (unlock) {
        $.unblockUI();
    }
}

function Reemplazar(texto, actual, proximo) {
    if (texto)
        return texto.replace(actual, proximo);
    return false;
}
//corrige a 4 decimales
function CorrigePuntuacion(valor) {
    return Reemplazar(ConvierteADecimal(valor).toFixed(4), ".", ",");
}
//corrige a 2 decimales
function CorrigePuntuacion2(valor) {
    if (isNaN(valor) || !$.isNumeric(valor)) {
        valor = 0;
    }
    return Reemplazar(ConvierteADecimal(valor).toFixed(2), ".", ",");
}

function CorrigePuntuacionMonto() {
    var monto = $(this).val();
    if (monto.trim() != "") {
        var m = CorrigePuntuacion(monto);
        $(this).val(m);
    }
}

function ConvierteADecimal(valor) {
    if (valor != undefined) {
        return parseFloat(Reemplazar(valor.toString(), ",", "."));
    }
    else {
        return 0;
    }
}

function CorrigeDecimal(i, item) {
    item.value = CorrigePuntuacion(ConvierteADecimal(item.value));

}

function AgregandoRequestVerificationToken(objs) {
    objs.__RequestVerificationToken = $($("input[name=__RequestVerificationToken]")[0]).val();
    return objs;
}

function AgregandoRequestVerificationTokenEnArray(arr) {
    arr.push({ "name": "__RequestVerificationToken", "value": $($("input[name=__RequestVerificationToken]")[0]).val() });
    return arr;
}

