$(function () {
    AbrirWaiting();
    CargarAutoActual();
    $("#btnConfirmacionFinal").on("click", ConfirmaFinalizacionTI);
});

function ConfirmaFinalizacionTI() {
    AbrirWaiting();
    var data = {}
    PostGen(data, ControlSalidaTIUrl, function (obj) {

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