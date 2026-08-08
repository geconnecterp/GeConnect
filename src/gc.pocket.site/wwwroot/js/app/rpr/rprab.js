var validandoUl = false;
var validandoBox = false;
var confirmandoBoxUl = false;

$(function () {
    console.info("[Pocket][RPR-BOX] Inicializando circuito de almacenaje", {
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
    console.info("[Pocket][RPR-BOX] UL modificada; se reinician BOX y confirmacion", {
        ul: valorActual("#txtUl")
    });
}

function reiniciarDesdeBox() {
    $("#btnConfirmar").prop("disabled", true);
    console.info("[Pocket][RPR-BOX] BOX modificado; se requiere validarlo nuevamente", {
        box: valorActual("#txtBox")
    });
}

function registrarErrorComunicacion(etapa, jqXHR) {
    console.error("[Pocket][RPR-BOX] Fallo de comunicacion con el servidor", {
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
        console.info("[Pocket][RPR-BOX] Enter capturado", {
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
                console.warn("[Pocket][RPR-BOX] Enter recibido desde un control no reconocido", { control: who });
                break;
        }
    }
}

function validaUL() {
    if (validandoUl) {
        console.warn("[Pocket][RPR-BOX] Se ignora una validacion de UL duplicada");
        return false;
    }

    var ul = valorActual("#txtUl");
    var datos = { ul };
    validandoUl = true;
    reiniciarDesdeUl();
    console.info("[Pocket][RPR-BOX] Solicitando validacion de UL sin alterar su estructura", {
        ul: ul,
        segmentos: ul.split("-"),
        cantidadSegmentos: ul.split("-").length
    });

    PostGen(datos, validaUlUrl, function (obj) {
        validandoUl = false;
        console.info("[Pocket][RPR-BOX] Respuesta de validacion de UL", {
            error: obj.error === true,
            advertencia: obj.warn === true,
            mensaje: obj.msg,
            ulAceptada: obj.ul || null,
            fueNormalizada: obj.fueNormalizada === true
        });

        if (valorActual("#txtUl") !== ul) {
            console.warn("[Pocket][RPR-BOX] Se descarta una respuesta de UL porque el valor cambio durante la consulta", {
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
                console.info("[Pocket][RPR-BOX] UL normalizada sin modificar sus separadores", {
                    ingresada: ul,
                    validada: obj.ul
                });
                $("#txtUl").val(obj.ul);
            }

            $("#txtBox").prop("disabled", false).focus();
            console.info("[Pocket][RPR-BOX] UL valida; campo BOX habilitado", {
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
        console.warn("[Pocket][RPR-BOX] Se ignora una validacion de BOX duplicada");
        return false;
    }

    var box = valorActual("#txtBox");
    var datos = { box };
    validandoBox = true;
    $("#btnConfirmar").prop("disabled", true);
    console.info("[Pocket][RPR-BOX] Solicitando validacion de BOX", { box: box });

    PostGen(datos, validaBoxUrl, function (obj) {
        validandoBox = false;
        console.info("[Pocket][RPR-BOX] Respuesta de validacion de BOX", {
            error: obj.error === true,
            advertencia: obj.warn === true,
            mensaje: obj.msg,
            boxSugerido: obj.box || null
        });

        if (valorActual("#txtBox") !== box) {
            console.warn("[Pocket][RPR-BOX] Se descarta una respuesta de BOX porque el valor cambio durante la consulta", {
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
            console.info("[Pocket][RPR-BOX] BOX valido; confirmacion habilitada", {
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
        console.warn("[Pocket][RPR-BOX] Se ignora una confirmacion duplicada");
        return false;
    }

    var box = valorActual("#txtBox");
    var ul = valorActual("#txtUl");
    var datos = { box, ul };
    confirmandoBoxUl = true;
    $("#btnConfirmar").prop("disabled", true);
    console.info("[Pocket][RPR-BOX] Confirmando almacenaje de UL en BOX", {
        ul: ul,
        box: box
    });

    PostGen(datos, almacenajeBoxUrl, function (obj) {
        confirmandoBoxUl = false;
        console.info("[Pocket][RPR-BOX] Respuesta de confirmacion de almacenaje", {
            error: obj.error === true,
            advertencia: obj.warn === true,
            mensaje: obj.msg,
            ulAlmacenada: obj.ul || ul,
            boxUtilizado: obj.box || box
        });

        if (obj.error === true) {
            $("#btnConfirmar").prop("disabled", false);
            AbrirMensaje("Importante", obj.msg, function () {
                $("#msjModal").modal("hide");
                return true;
            }, false, ["Aceptar"], "error!", null);
        }
        else if (obj.warn === true) {
            $("#btnConfirmar").prop("disabled", false);
            AbrirMensaje("Importante", obj.msg, function () {
                $("#msjModal").modal("hide");
                return true;
            }, false, ["Aceptar"], "warn!", null);
        }
        else {
            console.info("[Pocket][RPR-BOX] Almacenaje completado correctamente", {
                ul: obj.ul || ul,
                box: obj.box || box
            });
            AbrirMensaje("Importante", obj.msg, function () {
                $("#msjModal").modal("hide");
                window.location.href = homeInicio;
            }, false, ["Aceptar"], "succ!", null);
        }
    }, function (jqXHR) {
        confirmandoBoxUl = false;
        $("#btnConfirmar").prop("disabled", false);
        registrarErrorComunicacion("confirmacion-almacenaje", jqXHR);
    });
    return true;
}
