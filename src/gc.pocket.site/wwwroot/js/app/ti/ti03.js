$(function () {

    $("#rbBox").on("click", function () { presentaListaProducto("B"); });
    $("#rbProd").on("click", function () { presentaListaProducto("R"); });
    $("#rbRubro").on("click", function () { presentaListaProducto("P"); });  

   

    AbrirWaiting();
    CargarAutoActual();
    $("#btnCtrlSalida").on("click", VerificaCtrlSalida);
    presentaListaProducto("B");//ordenado por box
    //InicializaTiCarga();
});

//function InicializaTiCarga() {
//    if (autorizacionActual.tipoTI === "S") {
//        $("#btnCargaCarritoNuevo").removeClass("btn-success").addClass("btn-secundary");
//        $("#btnCargaCarritoNuevo").prop("disabled", true);
//    }
//    else {
//        $("#btnCargaCarritoNuevo").removeClass("btn-secundary").addClass("btn-success");
//        $("#btnCargaCarritoNuevo").prop("disabled", flase);
//    }
//}

var estadoControlSalidaTi = null;

function VerificaCtrlSalida() {
    if (estadoControlSalidaTi !== null) {
        console.warn("[Pocket][TI] Se ignora un control de salida duplicado");
        return false;
    }

    if (!autorizacionActual || !autorizacionActual.ti) {
        console.error("[Pocket][TI] No se puede controlar la salida sin una transferencia activa");
        ControlaMensajeError("No se pudo identificar la transferencia activa. Recargue la pantalla.");
        return false;
    }

    var ti = autorizacionActual.ti;
    var data = { ti: ti };
    estadoControlSalidaTi = IniciarConfirmacionSegura(
        "#btnCtrlSalida",
        "Espere... se está realizando el control de salida...",
        "Procesando..."
    );

    if (estadoControlSalidaTi === null) {
        return false;
    }

    console.info("[Pocket][TI] Iniciando control de salida", { ti: ti });

    try {
        PostGen(data, ControlSalidaTIUrl, function (obj) {
            FinalizarControlSalidaTi();
            console.info("[Pocket][TI] Respuesta del control de salida", {
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
                console.info("[Pocket][TI] Control de salida completado", { ti: ti });
                AbrirMensaje("Importante", obj.msg, function () {
                    $("#msjModal").modal("hide");
                    if (autorizacionActual.tipoTI === "S") {
                        window.location.href = homeModUrl;
                    }
                    else {
                        window.location.href = ConfirmarTIUrl;
                    }
                    return true;
                }, false, ["Aceptar"], "succ!", null);
            }
        }, function (jqXHR) {
            console.error("[Pocket][TI] Error de comunicación durante el control de salida", {
                estadoHttp: jqXHR ? jqXHR.status : null,
                detalleHttp: jqXHR ? jqXHR.statusText : null,
                ti: ti
            });
            FinalizarControlSalidaTi();
            fnError(jqXHR);
        });
    }
    catch (error) {
        console.error("[Pocket][TI] Error inesperado al iniciar el control de salida", error);
        FinalizarControlSalidaTi();
        ControlaMensajeError("No se pudo iniciar el control de salida. Intente nuevamente.");
    }

    return false;
}

function FinalizarControlSalidaTi() {
    var contexto = estadoControlSalidaTi;
    estadoControlSalidaTi = null;
    FinalizarConfirmacionSegura(contexto);
}
function presentaListaProducto(orden) {
    datos = {orden};
    PostGenHtml(datos, buscarListaProductosUrl, function (obj) {

        $("#divti03").html(obj);
        var tb = $("#divti03 #tbListaProd tbody td");
        if (tb.length <= 0) {
            $("#btnCtrlSalida").hide("fast");
        } else {
            $("#btnCtrlSalida").show("fast");
        }
        CerrarWaiting();

        //verifico si tiene el producto actual
        //si lo tiene lo posiciona en el registro de ese mismo producto

    });
}

function mostrarMensaje(nota) {
    AbrirMensaje("Atención", nota, function () {
        $("#msjModal").modal("hide");
        return true;
    }, false, ["Aceptar"], "info!", null);
}
function limpiarProductoCarrito(id,boxId) {
    AbrirWaiting()
    //aca se validará previamente si la cantidad ingresada corresponde a lo solicitado

    //se procede a enviar el producto a cargar
    var dato = { p_id: id, boxId }
    PostGen(dato, LimpiaProductoCarritoUrl, function (obj) {
        if (obj.error === true) {
            CerrarWaiting();
            AbrirMensaje("Importante", obj.msg, function () {
                $("#msjModal").modal("hide");
                return true;
            }, false, ["Aceptar"], "error!", null);
        }
        else if (obj.warn === true) {
            CerrarWaiting();
            AbrirMensaje("Importante", obj.msg, function () {
                $("#msjModal").modal("hide");
                return true;
            }, false, ["Aceptar"], "error!", null);
        }
        else {
            CerrarWaiting();
            ControlaMensajeSuccess(obj.msg);
            window.location.href = proximoProductoUrl + "?esrubro=false&esbox=false&tiId=" + obj.tiId;
        }
    });


}
