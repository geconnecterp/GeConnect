// Circuito compartido por RPR-ABOX y RTR-ABOX.
var validandoUl = false;
var validandoBox = false;
var confirmandoBoxUl = false;
var estadoConfirmacionBoxUl = null;

$(function () {
    console.info("[Pocket][BOX-UL] Inicializando circuito de almacenaje", {
        controlesConectados: $(".inputEditable").length,
        pasoInicial: "lectura-ul"
    });

    $("#txtUl").on("input", reiniciarDesdeUl);
    $("#txtBox").on("input", reiniciarDesdeBox);
    $("#btnConfirmar").on("click", ConfirmarBoxUl);
    $(".inputEditable").on("keydown", analizaInput);
});

function valorActual(selector) {
    return $.trim($(selector).val());
}
function reiniciarDesdeUl() {
    $("#txtBox").val("").prop("disabled", true);
    $("#btnConfirmar").prop("disabled", true);
    console.info("[Pocket][BOX-UL] UL modificada; se reinician BOX y confirmacion", {
        ul: valorActual("#txtUl")
    });
}

function reiniciarDesdeBox() {
    $("#btnConfirmar").prop("disabled", true);
    console.info("[Pocket][BOX-UL] BOX modificado; se requiere validarlo nuevamente", {
        box: valorActual("#txtBox")
    });
}

function registrarErrorComunicacion(etapa, jqXHR) {
    console.error("[Pocket][BOX-UL] Fallo de comunicacion con el servidor", {
        etapa: etapa,
        estadoHttp: jqXHR ? jqXHR.status : null,
        detalleHttp: jqXHR ? jqXHR.statusText : null
    });
    fnError(jqXHR);
}

function analizaInput(e) {
    if (e.which === 13 || e.key === "Enter") {
        e.preventDefault();
        var who = $(this).prop("id");
        console.info("[Pocket][BOX-UL] Enter capturado", {
            control: who,
            valor: $(this).val()
        });

        switch (who) {
            case "txtUl":
                validaUL();
                break;
            case "txtBox":
                validaBox();
                break;
            default:
                console.warn("[Pocket][BOX-UL] Enter recibido desde un control no reconocido", { control: who });
                break;
        }
    }
}

function validaUL() {
    if (validandoUl) {
        console.warn("[Pocket][BOX-UL] Se ignora una validacion de UL duplicada");
        return false;
    }

    var ul = valorActual("#txtUl");
    var datos = { ul };
    validandoUl = true;
    reiniciarDesdeUl();
    console.info("[Pocket][BOX-UL] Solicitando validacion de UL sin alterar su estructura", {
        ul: ul,
        segmentos: ul.split("-"),
        cantidadSegmentos: ul.split("-").length
    });

    PostGen(datos, validaUlUrl, function (obj) {
        validandoUl = false;
        console.info("[Pocket][BOX-UL] Respuesta de validacion de UL", {
            error: obj.error === true,
            advertencia: obj.warn === true,
            mensaje: obj.msg,
            ulAceptada: obj.ul || null,
            fueNormalizada: obj.fueNormalizada === true
        });

        if (valorActual("#txtUl") !== ul) {
            console.warn("[Pocket][BOX-UL] Se descarta una respuesta de UL porque el valor cambio durante la consulta", {
                ulConsultada: ul,
                ulActual: valorActual("#txtUl")
            });
            return;
        }

        if (obj.error === true) {
            $("#txtUl").focus();
            AbrirMensaje("Importante", obj.msg, function () {
                $("#msjModal").modal("hide");
                return true;
            }, false, ["Aceptar"], "error!", null);
        }
        else if (obj.warn === true) {
            $("#txtUl").focus();
            AbrirMensaje("Importante", obj.msg, function () {
                $("#msjModal").modal("hide");
                return true;
            }, false, ["Aceptar"], "warn!", null);
        }
        else {
            ControlaMensajeSuccess(obj.msg);

            if (obj.ul && obj.ul !== ul) {
                console.info("[Pocket][BOX-UL] UL normalizada sin modificar sus separadores", {
                    ingresada: ul,
                    validada: obj.ul
                });
                $("#txtUl").val(obj.ul);
            }

            $("#txtBox").prop("disabled", false).focus();
            console.info("[Pocket][BOX-UL] UL valida; campo BOX habilitado", {
                ulIngresada: ul,
                ulValidada: obj.ul || ul
            });
            return true;
        }
    }, function (jqXHR) {
        validandoUl = false;
        registrarErrorComunicacion("validacion-ul", jqXHR);
    });
    return true;
}

function validaBox() {
    if (validandoBox) {
        console.warn("[Pocket][BOX-UL] Se ignora una validacion de BOX duplicada");
        return false;
    }

    var box = valorActual("#txtBox");
    var datos = { box };
    validandoBox = true;
    $("#btnConfirmar").prop("disabled", true);
    console.info("[Pocket][BOX-UL] Solicitando validacion de BOX", { box: box });

    PostGen(datos, validaBoxUrl, function (obj) {
        validandoBox = false;
        console.info("[Pocket][BOX-UL] Respuesta de validacion de BOX", {
            error: obj.error === true,
            advertencia: obj.warn === true,
            mensaje: obj.msg,
            boxSugerido: obj.box || null
        });

        if (valorActual("#txtBox") !== box) {
            console.warn("[Pocket][BOX-UL] Se descarta una respuesta de BOX porque el valor cambio durante la consulta", {
                boxConsultado: box,
                boxActual: valorActual("#txtBox")
            });
            return;
        }

        if (obj.error === true) {
            $("#txtBox").focus();
            AbrirMensaje("Importante", obj.msg, function () {
                $("#msjModal").modal("hide");
                return true;
            }, false, ["Aceptar"], "error!", null);
        }
        else if (obj.warn === true) {
            $("#txtBox").focus();
            AbrirMensaje("Importante", obj.msg, function () {
                $("#msjModal").modal("hide");
                return true;
            }, false, ["Aceptar"], "warn!", null);
        }
        else {
            ControlaMensajeSuccess(obj.msg);
            $("#txtBox").val(obj.box);
            $("#btnConfirmar").prop("disabled", false).focus();
            console.info("[Pocket][BOX-UL] BOX valido; confirmacion habilitada", {
                boxIngresado: box,
                boxValidado: obj.box
            });
            return true;
        }
    }, function (jqXHR) {
        validandoBox = false;
        registrarErrorComunicacion("validacion-box", jqXHR);
    });
    return true;
}

function ConfirmarBoxUl() {
    if (confirmandoBoxUl) {
        console.warn("[Pocket][BOX-UL] Se ignora una confirmacion duplicada");
        return false;
    }

    var box = valorActual("#txtBox");
    var ul = valorActual("#txtUl");
    var datos = { box, ul };
    confirmandoBoxUl = true;
    estadoConfirmacionBoxUl = IniciarConfirmacionSegura(
        "#btnConfirmar",
        "Espere... se está almacenando la UL en el BOX...",
        "Procesando..."
    );

    if (estadoConfirmacionBoxUl === null) {
        confirmandoBoxUl = false;
        return false;
    }

    console.info("[Pocket][BOX-UL] Confirmando almacenaje de UL en BOX", {
        ul: ul,
        box: box
    });

    try {
        PostGen(datos, almacenajeBoxUrl, function (obj) {
            FinalizarConfirmacionBoxUl();
            console.info("[Pocket][BOX-UL] Respuesta de confirmacion de almacenaje", {
                error: obj.error === true,
                advertencia: obj.warn === true,
                mensaje: obj.msg,
                ulAlmacenada: obj.ul || ul,
                boxUtilizado: obj.box || box
            });

            if (obj.error === true) {
                AbrirMensaje("Importante", obj.msg, function () {
                    $("#msjModal").modal("hide");
                    return true;
                }, false, ["Aceptar"], "error!", null);
            }
            else if (obj.warn === true) {
                AbrirMensaje("Importante", obj.msg, function () {
                    $("#msjModal").modal("hide");
                    return true;
                }, false, ["Aceptar"], "warn!", null);
            }
            else {
                console.info("[Pocket][BOX-UL] Almacenaje completado correctamente", {
                    ul: obj.ul || ul,
                    box: obj.box || box
                });
                AbrirMensaje("Importante", obj.msg, function () {
                    $("#msjModal").modal("hide");
                    window.location.href = homeInicio;
                }, false, ["Aceptar"], "succ!", null);
            }
        }, function (jqXHR) {
            FinalizarConfirmacionBoxUl();
            registrarErrorComunicacion("confirmacion-almacenaje", jqXHR);
        });
    }
    catch (error) {
        console.error("[Pocket][BOX-UL] Error inesperado al iniciar la confirmación", error);
        FinalizarConfirmacionBoxUl();
        ControlaMensajeError("No se pudo iniciar la confirmación. Intente nuevamente.");
    }

    return false;
}

function FinalizarConfirmacionBoxUl() {
    var contexto = estadoConfirmacionBoxUl;
    estadoConfirmacionBoxUl = null;
    confirmandoBoxUl = false;
    FinalizarConfirmacionSegura(contexto);
}
