$(document).ready(function () {

    productosGrid();
    
    $("#btnConfirmarRPR").click(confirmarRPR);
    $("#ul_Id").on("keypress", function (e) {
        if (e.which == "13") {
            var valor = $("#ul_Id").val();
            var res = parseInt(valor);
            if (isNaN(res)) {
                AbrirMensaje("ATENCIÓN", "El valor ingresado no es numérico. Verifique.", function () {
                    $("#msjModal").modal("hide");
                    return true;
                },
                    false, ["Aceptar"], "warn!", null);
            }
            switch (valor.length) {
                case 0:
                    AbrirMensaje("ATENCIÓN", "Debe ingresar el Nro de palet.", function () {
                        $("#msjModal").modal("hide");
                        return true;
                    },
                        false, ["Aceptar"], "warn!", null);
                    break;
                case 1:
                case 2:
                    valor = ('0' + valor).slice(-2); // Asegura que el numero siempre tenga dos dígitos
                    $("#ul_Id").val("RPR"+NroAuto + valor);                    
                    break;
                default:
                   
                    break;
            }
                
        } 
    });

    return true;

});

var estadoConfirmacionRpr = null;

function confirmarRPR() {
    if (estadoConfirmacionRpr !== null) {
        console.warn("[Pocket][RPR] Se ignora una confirmación duplicada");
        return false;
    }

    // Obtener UL. Las validaciones funcionales continúan realizándose en el servidor.
    var ul = $("#ul_Id").val();
    var datos = { ul };

    estadoConfirmacionRpr = IniciarConfirmacionSegura(
        "#btnConfirmarRPR",
        "Espere... se están grabando los datos...",
        "Procesando..."
    );

    if (estadoConfirmacionRpr === null) {
        return false;
    }

    try {
        PostGen(datos, ConfirmarRPRUrl, function (obj) {
            FinalizarConfirmacionRpr();

            if (obj.error === true) {
                AbrirMensaje("ATENCIÓN", obj.msg, function () {
                    $("#msjModal").modal("hide");
                    return true;
                }, false, ["Aceptar"], "error!", null);
            }
            else if (obj.warn === true) {
                AbrirMensaje("ATENCIÓN", obj.msg, function () {
                    $("#msjModal").modal("hide");
                    return true;
                }, false, ["Aceptar"], "warn!", null);
            }
            else {
                AbrirMensaje("ATENCIÓN", obj.msg, function () {
                    $("#msjModal").modal("hide");
                    window.location.href = homeUrl;
                    return true;
                }, false, ["Aceptar"], "succ!", null);
            }
        }, function (jqXHR) {
            console.error("[Pocket][RPR] Error de comunicación durante la confirmación", {
                estadoHttp: jqXHR ? jqXHR.status : null,
                detalleHttp: jqXHR ? jqXHR.statusText : null
            });
            FinalizarConfirmacionRpr();
            fnError(jqXHR);
        });
    }
    catch (error) {
        console.error("[Pocket][RPR] Error inesperado al iniciar la confirmación", error);
        FinalizarConfirmacionRpr();
        ControlaMensajeError("No se pudo iniciar la confirmación. Intente nuevamente.");
    }

    return false;
}

function FinalizarConfirmacionRpr() {
    var contexto = estadoConfirmacionRpr;
    estadoConfirmacionRpr = null;
    FinalizarConfirmacionSegura(contexto);
}
