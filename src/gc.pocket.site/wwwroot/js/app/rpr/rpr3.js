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

function confirmarRPR() {
    //obtener deposito y UL
    var ul = $("#ul_Id").val();
    var dp = $("#DepoId").val();
    datos = { dp, ul }
    AbrirWaiting("Espere... se estan grabando los datos...");
    PostGen(datos, ConfirmarRPRUrl, function (obj) {
        if (obj.error === true) {
            CerrarWaiting();
            AbrirMensaje("ATENCIÓN", obj.msg, function () {
                $("#msjModal").modal("hide");
                return true;
            },
                false, ["Aceptar"], "error!", null);
        }
        else if (obj.warn === true) {
            CerrarWaiting();
            AbrirMensaje("ATENCIÓN", obj.msg, function () {
                $("#msjModal").modal("hide");
                return true;
            },
                false, ["Aceptar"], "warn!", null);
        }
        else {
            CerrarWaiting();
            AbrirMensaje("ATENCIÓN", obj.msg, function () {

                $("#msjModal").modal("hide");
                window.location.href = homeUrl;
                return true;
            },
                false, ["Aceptar"], "succ!", null);
        }
    })
    return true;
}