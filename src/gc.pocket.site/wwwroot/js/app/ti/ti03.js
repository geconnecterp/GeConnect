$(function () {

    $("#rbBox").on("click", function () { alert("Radio Button Box") });
    $("#rbProd").on("click", function () { alert("Radio Button Prod") });
    $("#rbRubro").on("click", function () { alert("Radio Button Rubro") });  

    AbrirWaiting();
    CargarAutoActual();

    presentaListaProducto();
});

function presentaListaProducto() {
    datos = {};
    PostGenHtml(datos, buscarListaProductosUrl, function (obj) {

        $("#divti03").html(obj);

        CerrarWaiting();
    });
}

function mostrarMensaje(nota) {
    AbrirMensaje("Atención", nota, function () {
        $("#msjModal").modal("hide");
        return true;
    }, false, ["Aceptar"], "info!", null);
}
function limpiarProductoCarrito(id) {
    AbrirWaiting()
    //aca se validará previamente si la cantidad ingresada corresponde a lo solicitado

    //se procede a enviar el producto a cargar
    var dato = { p_id:id }
    PostGen(dato, LimpiaProductoCarritoUrl, function (obj) {
        if (obj.error === true) {
            CerrarWaiting();
            AbrirMensaje("Importante", obj.msg, function () {
                $("#msjModal").modal("hide");
                return true;
            }, false, ["Aceptar"], "error!", null);
        }
        else {
            CerrarWaiting();
            ControlaMensajeSuccess(obj.msg);
            window.location.href = proximoProductoUrl + "?esrubro=false&esbox=false";
        }
    });


}
