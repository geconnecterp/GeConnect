$(function () {  
    $("#boxId").on("click", function () {
        $("#boxId").val("");
        $("#btnContinua01").prop("disabled", true)
    });
    $("#btnVerifica").on("click", VerificaDevProdBox);
    $("#btnContinua01").on("click", continuaDevProv);    

    $(".inputEditable").on("keypress", analizaEnterInput);
})


function VerificaDevProdBox() {
    var datos = { boxId: $("#boxId").val() };
    AbrirWaiting();
    PostGen(datos, validaBoxUrl, function (obj) {
        if (obj.error === true) {
            CerrarWaiting();

            AbrirMensaje("Importante", obj.msg, function () {
                $("#msjModal").modal("hide");
                return true;
            }, false, ["Aceptar"], "error!", null);
        } else if (obj.warn === true) {
            CerrarWaiting();
            AbrirMensaje("Importante", obj.msg, function () {
                $("#msjModal").modal("hide");
                return true;
            }, false, ["Aceptar"], "warn!", null);
        }
        else {
            CerrarWaiting();
            ControlaMensajeSuccess(obj.msg);
            $("#btnContinua01").prop("disabled",false)
            return true;
        }
    });
}

function continuaDevProv() {
    //obtengo los datos de box y de tipo de ajuste
    var box = $("#boxId").val();
    
    window.location.href = cargaProducto + "?box=" + box;
}