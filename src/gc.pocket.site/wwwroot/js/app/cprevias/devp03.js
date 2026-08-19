$(function () {
    $("#btnContinua01").on("click", ConfirmarAjustes);
    productosGridASTK();
});

var estadoConfirmacionDevolucion = null;

function ConfirmarAjustes() {
    if (estadoConfirmacionDevolucion !== null) {
        console.warn("[Pocket][DevolucionProveedor] Se ignora una confirmación duplicada");
        return false;
    }

    var datos = {};
    estadoConfirmacionDevolucion = IniciarConfirmacionSegura(
        "#btnContinua01",
        "Espere... se está confirmando la devolución al proveedor...",
        "Procesando..."
    );

    if (estadoConfirmacionDevolucion === null) {
        return false;
    }

    console.info("[Pocket][DevolucionProveedor] Iniciando confirmación");

    try {
        PostGen(datos, confirmarDVURL, function (obj) {
            FinalizarConfirmacionDevolucion();
            console.info("[Pocket][DevolucionProveedor] Respuesta de confirmación", {
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
                console.info("[Pocket][DevolucionProveedor] Devolución confirmada correctamente");
                AbrirMensaje("Carga Satisfactoria", obj.msg, function () {
                    window.location.href = homeDevpUrl;
                }, false, ["Cerrar"], "succ!", null);
            }
        }, function (jqXHR) {
            console.error("[Pocket][DevolucionProveedor] Error de comunicación durante la confirmación", {
                estadoHttp: jqXHR ? jqXHR.status : null,
                detalleHttp: jqXHR ? jqXHR.statusText : null
            });
            FinalizarConfirmacionDevolucion();
            fnError(jqXHR);
        });
    }
    catch (error) {
        console.error("[Pocket][DevolucionProveedor] Error inesperado al iniciar la confirmación", error);
        FinalizarConfirmacionDevolucion();
        ControlaMensajeError("No se pudo iniciar la confirmación. Intente nuevamente.");
    }

    return false;
}

function FinalizarConfirmacionDevolucion() {
    var contexto = estadoConfirmacionDevolucion;
    estadoConfirmacionDevolucion = null;
    FinalizarConfirmacionSegura(contexto);
}
