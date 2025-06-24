var usuarioAuth = "";
var administracion = "";
var repoApiUrl = "";

$(function () {
    AbrirWaiting("Espere, se esta inicializando la vista...");
    $("#formulario").slideUp(300).fadeIn(400);
    CerrarWaiting();

    desabilitarRetroceso();
    //var exeUnlock = true;
    //setTimeout($.unblockUI, 15000);

    InicializarPage();
    buildSubMenu();

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

function buildSubMenu() {
    document.addEventListener("DOMContentLoaded", function () {
        /////// Prevent closing from click inside dropdown
        document.querySelectorAll('.dropdown-menu').forEach(function (element) {
            element.addEventListener('click', function (e) {
                e.stopPropagation();
            });
        })

        // make it as accordion for smaller screens
        if (window.innerWidth < 992) {

            // close all inner dropdowns when parent is closed
            document.querySelectorAll('.navbar .dropdown').forEach(function (everydropdown) {
                everydropdown.addEventListener('hidden.bs.dropdown', function () {
                    // after dropdown is hidden, then find all submenus
                    this.querySelectorAll('.submenu').forEach(function (everysubmenu) {
                        // hide every submenu as well
                        everysubmenu.style.display = 'none';
                    });
                })
            });

            document.querySelectorAll('.dropdown-menu a').forEach(function (element) {
                element.addEventListener('click', function (e) {

                    let nextEl = this.nextElementSibling;
                    if (nextEl && nextEl.classList.contains('submenu')) {
                        // prevent opening link if link needs to open dropdown
                        e.preventDefault();
                        if (nextEl.style.display == 'block') {
                            nextEl.style.display = 'none';
                        } else {
                            nextEl.style.display = 'block';
                        }

                    }
                });
            })
        }
    });
}

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


// Agregar al final de site.js o en un archivo específico para manejo de AJAX

// Interceptor para peticiones AJAX que verifica si la sesión ha expirado
$(document).ajaxError(function (event, jqXHR, ajaxSettings, thrownError) {
    // Verificar si el error es por sesión expirada (código 440)
    if (jqXHR.status === 440) {
        // Mostrar mensaje al usuario
        AbrirMensaje(
            "Sesión expirada",
            "Su sesión ha expirado. Será redirigido a la página de inicio de sesión.",
            function () {
                // Redirigir al login
                window.location.href = "/seguridad/Token/Login";
            },
            false,
            ["Aceptar"],
            "warn!",
            null
        );

        // Evitar que se procesen otros manejadores de error
        return false;
    }
});

//// Para peticiones fetch modernas (opcional, si usas fetch en lugar de jQuery)
//const originalFetch = window.fetch;
//window.fetch = async function (input, init) {
//    const response = await originalFetch(input, init);

//    if (response.status === 440) {
//        const responseData = await response.json();

//        // Mostrar mensaje al usuario
//        AbrirMensaje(
//            "Sesión expirada",
//            responseData.msg || "Su sesión ha expirado. Será redirigido a la página de inicio de sesión.",
//            function () {
//                // Redirigir al login
//                window.location.href = "/seguridad/Token/Login";
//            },
//            false,
//            ["Aceptar"],
//            "warn!",
//            null
//        );

//        // Crear una promesa rechazada para que los manejadores catch puedan procesarla
//        return Promise.reject(new Error("Sesión expirada"));
//    }

//    return response;
//};
