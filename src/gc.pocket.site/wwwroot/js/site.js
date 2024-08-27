﻿$(document).ready(function () {
    AbrirWaiting("Espere, se esta inicializando la vista...");
    $("#formulario").slideUp(300).fadeIn(400);
    CerrarWaiting();

    desabilitarRetroceso();
    //var exeUnlock = true;
    //setTimeout($.unblockUI, 15000);

    InicializarPage();

    //Esta declaración de evento es aca, pues se tiene que generarse el evento para que se ejecute.
    $("#btnMensajeAceptar").on("click", function (evento) {
        AceptarMensaje("SI");
        evento.preventDefault();
        evento.stopPropagation();
        return false;
    });

    $("#btnMensajeAlternativa").on("click", function (evento) {
        AceptarMensaje("SI2");
        evento.preventDefault();
        evento.stopPropagation();
        return false;
    });
});

function Bloquear() {
    $.blockUI({ overlayCSS: { backgroundColor: '#d3d3d3' }, message: MensajeBlock });
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

//desabiliatar el retroceso
function desabilitarRetroceso() {
    window.location.hash = "no-back-button";
    window.location.hash = "Again-No-back-button-" //chrome
    window.onhashchange = function () { window.location.hash = ""; }
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