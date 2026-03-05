var usuarioAuth = "";
var administracion = "";
var repoApiUrl = "";
var pathBase = ""; // Variable global para PathBase

$(function () {
    // Obtener PathBase desde el DOM (se debe agregar en el layout)
    pathBase = document.body.getAttribute('data-pathbase') || '';
    
    AbrirWaiting("Espere, se esta inicializando la vista...");
    $("#formulario").slideUp(300).fadeIn(400);
    CerrarWaiting();

    desabilitarRetroceso();

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

/**
 * Construye una ruta completa incluyendo el PathBase si existe
 * @param {string} path - Ruta relativa (debe empezar con /)
 * @returns {string} - Ruta completa con PathBase
 */
function buildPath(path) {
    if (!path.startsWith('/')) {
        path = '/' + path;
    }
    return pathBase + path;
}

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

// Interceptor para peticiones AJAX que verifica si la sesión ha expirado
$(document).ajaxError(function (event, jqXHR, ajaxSettings, thrownError) {
    // Verificar si el error es por sesión expirada (código 440)
    if (jqXHR.status === 440) {
        // Mostrar mensaje al usuario
        AbrirMensaje(
            "Sesión expirada",
            "Su sesión ha expirado. Será redirigido a la página de inicio de sesión.",
            function () {
                // Redirigir al login usando PathBase
                window.location.href = buildPath("/seguridad/Token/Login");
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

