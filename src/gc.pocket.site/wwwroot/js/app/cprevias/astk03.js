$(function () {
    $("#btnContinua01").on("click", ConfirmarAjustes);
    productosGridASTK();
});

var estadoConfirmacionAjusteStock = null;

function ConfirmarAjustes() {
    if (estadoConfirmacionAjusteStock !== null) {
        console.warn("[Pocket][AjusteStock] Se ignora una confirmación duplicada");
        return false;
    }

    var datos = {};
    estadoConfirmacionAjusteStock = IniciarConfirmacionSegura(
        "#btnContinua01",
        "Espere... se están confirmando los ajustes de stock...",
        "Procesando..."
    );

    if (estadoConfirmacionAjusteStock === null) {
        return false;
    }

    console.info("[Pocket][AjusteStock] Iniciando confirmación");

    try {
        PostGen(datos, confirmarAJURL, function (obj) {
            FinalizarConfirmacionAjusteStock();
            console.info("[Pocket][AjusteStock] Respuesta de confirmación", {
                error: obj.error === true,
                advertencia: obj.warn === true,
                mensaje: obj.msg
            });

            if (obj.error === true) {
                AbrirMensaje("ATENCIÓN", obj.msg, function () {
                    $("#msjModal").modal("hide");
                    return true;
                }, false, ["Cerrar"], "error!", null);
            }
            else if (obj.warn === true) {
                AbrirMensaje("ATENCIÓN", obj.msg, function () {
                    $("#msjModal").modal("hide");
                    return true;
                }, false, ["Cerrar"], "warn!", null);
            }
            else {
                console.info("[Pocket][AjusteStock] Ajustes confirmados correctamente");
                AbrirMensaje("Carga Satisfactoria", obj.msg, function () {
                    window.location.href = homeASTKUrl;
                }, false, ["Cerrar"], "succ!", null);
            }
        }, function (jqXHR) {
            console.error("[Pocket][AjusteStock] Error de comunicación durante la confirmación", {
                estadoHttp: jqXHR ? jqXHR.status : null,
                detalleHttp: jqXHR ? jqXHR.statusText : null
            });
            FinalizarConfirmacionAjusteStock();
            fnError(jqXHR);
        });
    }
    catch (error) {
        console.error("[Pocket][AjusteStock] Error inesperado al iniciar la confirmación", error);
        FinalizarConfirmacionAjusteStock();
        ControlaMensajeError("No se pudo iniciar la confirmación. Intente nuevamente.");
    }

    return false;
}

function FinalizarConfirmacionAjusteStock() {
    var contexto = estadoConfirmacionAjusteStock;
    estadoConfirmacionAjusteStock = null;
    FinalizarConfirmacionSegura(contexto);
}
