$(function () {
    $("#btnContinua01").on("click", ConfirmarAjustes);
    productosGridASTK();
});

function ConfirmarAjustes() {
    var datos = {};
    AbrirWaiting();
    PostGen(datos, confirmarAJURL, function (obj) {
        if (obj.error === true) {
            CerrarWaiting();
            AbrirMensaje("ATENCIÓN", obj.msg, function () {
                $("#msjModal").modal("hide");
                return true;
            }, false, ["Cerrar"], "error!", null);
            return true;
        }
        else {
            CerrarWaiting();
            AbrirMensaje("Carga Satisfactoria", obj.msg, function () {
                window.location.href = homeASTKUrl;
            }, false, ["Cerrar"], "succ!", null);
            
            return true;
        }
    });

}
