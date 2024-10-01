$(function () {
    AbrirWaiting();
    $("#txtBoxDestino").on("click", function () { $("#btnConfirmacionFinal").prop("disabled", true); });
    $("#btnConfirmacionFinal").on("click", ConfirmaFinalizacionTI);
    $("#btnVerifBoxDestino").on("click", VerificaBoxDestino);
    CargarAutoActual();
});

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
    AbrirWaiting();
    var ti = $("#btnConfirmacionFinal").attr("trint");
    
    var data = {ti,}
    PostGen(data, confirmacionFinalTIUrl, function (obj) {

        if (obj.error === true) {
            CerrarWaiting();
            AbrirMensaje("Importante", obj.msg, function () {
                $("#msjModal").modal("hide");
                return true;
            }, false, ["Aceptar"], "error!", null);
        }
        if (obj.warn === true) {
            CerrarWaiting();
            AbrirMensaje("Importante", obj.msg, function () {
                $("#msjModal").modal("hide");
                return true;
            }, false, ["Aceptar"], "warn!", null);
        }
        else {
            CerrarWaiting();
            ControlaMensajeSuccess(obj.msg);
            window.location.href = ConfirmarTIUrl;
        }
    });
}