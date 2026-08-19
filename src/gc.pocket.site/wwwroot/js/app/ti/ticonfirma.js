$(function () {
    AbrirWaiting();
    $("#txtBoxDestino").on("click", function () { $("#btnConfirmacionFinal").prop("disabled", true); });
    $("#btnConfirmacionFinal").on("click", ConfirmaFinalizacionTI);
    $("#btnVerifBoxDestino").on("click", VerificaBoxDestino);
    CargarAutoActual();
});

var estadoConfirmacionFinalTi = null;

function VerificaBoxDestino() {
    var dato = { boxId: $("#txtBoxDestino").val(),esBoxDest:true }
    PostGen(dato, validarBoxIngresadoUrl, function (obj) {
        if (obj.error === true) {
            AbrirMensaje("Importante", obj.msg, function () {
                $("#msjModal").modal("hide");
                $("#btnConfirmacionFinal").prop("disabled", true);
                return true;
            }, false, ["Aceptar"], "error!", null);
        }
        else if (obj.warn === true) {
            AbrirMensaje("Importante", obj.msg, function () {
                $("#msjModal").modal("hide");
                $("#btnConfirmacionFinal").prop("disabled", true);

                return true;
            }, false, ["Aceptar"], "warn!", null);
        }
        else {
            ControlaMensajeSuccess(obj.msg);
            $("#btnConfirmacionFinal").prop("disabled", false);
            //solo pasa al otro campo.           
            $("#btnConfirmacionFinal").focus();
            return true;
        }

    });
}

function ConfirmaFinalizacionTI() {
    if (estadoConfirmacionFinalTi !== null) {
        console.warn("[Pocket][TI] Se ignora una confirmación final duplicada");
        return false;
    }

    var ti = $("#btnConfirmacionFinal").attr("trint");
    var data = { ti: ti };
    estadoConfirmacionFinalTi = IniciarConfirmacionSegura(
        "#btnConfirmacionFinal",
        "Espere... se está confirmando la transferencia interna...",
        "Procesando..."
    );

    if (estadoConfirmacionFinalTi === null) {
        return false;
    }

    console.info("[Pocket][TI] Iniciando confirmación final", { ti: ti });

    try {
        PostGen(data, confirmacionFinalTIUrl, function (obj) {
            FinalizarConfirmacionFinalTi();
            console.info("[Pocket][TI] Respuesta de confirmación final", {
                error: obj.error === true,
                advertencia: obj.warn === true,
                mensaje: obj.msg,
                ti: ti
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
                console.info("[Pocket][TI] Transferencia confirmada correctamente", { ti: ti });
                ControlaMensajeSuccess(obj.msg);
                window.location.href = ConfirmarTIUrl;
            }
        }, function (jqXHR) {
            console.error("[Pocket][TI] Error de comunicación durante la confirmación final", {
                estadoHttp: jqXHR ? jqXHR.status : null,
                detalleHttp: jqXHR ? jqXHR.statusText : null,
                ti: ti
            });
            FinalizarConfirmacionFinalTi();
            fnError(jqXHR);
        });
    }
    catch (error) {
        console.error("[Pocket][TI] Error inesperado al iniciar la confirmación final", error);
        FinalizarConfirmacionFinalTi();
        ControlaMensajeError("No se pudo iniciar la confirmación. Intente nuevamente.");
    }

    return false;
}

function FinalizarConfirmacionFinalTi() {
    var contexto = estadoConfirmacionFinalTi;
    estadoConfirmacionFinalTi = null;
    FinalizarConfirmacionSegura(contexto);
}
