$(document).ready(function () {
    
    //var exeUnlock = true;
    //setTimeout($.unblockUI, 15000);
    AbrirWaiting("Espere, se esta inicializando la vista...");
    $("#formulario").slideUp(300).fadeIn(400);
    CerrarWaiting();
    desabilitarRetroceso();
    $("input.form-control").on("focus", function () { $(this).select(); })

    //el control de este código es para obtener el texto de la transición elegida.
    //$(".btnNext").click(function () {
    //    var btn = $(this);
    //    var formu = btn.attr("relacion");
    //    //busco el texto y lo inserto en el input hidden        
    //    $("#form" + formu + " input#descr" + formu).val($("textarea#descripcion").val());

    //    //verifico el campo pedidoParcial. si existe verifico el valor que tiene
    //    var parcial = $("input#pedido" + formu + "");
    //    if ($("input#pedido" + formu + "") !== undefined) {
    //        //verificamos el valor del componente
    //        if ($("input#pedido" + formu + "").val() === "True") {
    //            $("input#tipoValor" + formu + "").val($("input#vo_TipoValorId").val());
    //        }
    //    }

    //    Bloquear();
    //    $("#form" + formu).submit();

    //});

    //$("#btnGridHistoria").click(function () {
    //    $("#gridHistoriaEstados").modal("toggle");
    //});

    InicializarPage();
});

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

function AbrirWaiting(mensaje) {
    if (mensaje !== "") {
        $("#lblWaiting").text(mensaje);
    } else {
        $("#lblWaiting").text("Cargando...");
    }
    $("#Waiting").fadeIn(0);
}

function CerrarWaiting() {
    $("#Waiting").fadeOut(1000);
}

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

//function PostGen(data, path, retorno) {

//    $.ajax({
//        "dataType": 'json',
//        "url": path,
//        "type": "POST",
//        "data": data,
//        "success": retorno,
//        //beforeSend: function () { Bloquear();},
//        error: fnError
//    });
//}

//function fnError(jqXHR) {
//    //alert(jqXHR);
//    if (jqXHR.error) {
//        ControlaMensajeError(jqXHR.error);
//    }
//    else {
//        ControlaMensajeError(jqXHR);
//    }
//}



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

//function Reemplazar(texto, actual, proximo) {
//    if (texto)
//        return texto.replace(actual, proximo);
//    return false;
//}
////corrige a 4 decimales
//function CorrigePuntuacion(valor) {
//    return Reemplazar(ConvierteADecimal(valor).toFixed(4), ".", ",");
//}
////corrige a 2 decimales
//function CorrigePuntuacion2(valor) {
//    if (isNaN(valor) || !$.isNumeric(valor)) {
//        valor = 0;
//    }
//    return Reemplazar(ConvierteADecimal(valor).toFixed(2), ".", ",");
//}

//function CorrigePuntuacionMonto() {
//    var monto = $(this).val();
//    if (monto.trim() != "") {
//        var m = CorrigePuntuacion(monto);
//        $(this).val(m);
//    }
//}

//function ConvierteADecimal(valor) {
//    if (valor != undefined) {
//        return parseFloat(Reemplazar(valor.toString(), ",", "."));
//    }
//    else {
//        return 0;
//    }
//}

//function CorrigeDecimal(i, item) {
//    item.value = CorrigePuntuacion(ConvierteADecimal(item.value));

//}

//function AgregandoRequestVerificationToken(objs) {
//    objs.__RequestVerificationToken = $($("input[name=__RequestVerificationToken]")[0]).val();
//    return objs;
//}

//function AgregandoRequestVerificationTokenEnArray(arr) {
//    arr.push({ "name": "__RequestVerificationToken", "value": $($("input[name=__RequestVerificationToken]")[0]).val() });
//    return arr;
//}

