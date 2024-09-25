$(function () {

    $("#rbBox").on("click", function () { alert("Radio Button Box") });
    $("#rbProd").on("click", function () { alert("Radio Button Prod") });
    $("#rbRubro").on("click", function () { alert("Radio Button Rubro") });  

   

    AbrirWaiting();
    CargarAutoActual();
    $("#btnCtrlSalida").on("click", VerificaCtrlSalida);
    presentaListaProducto();
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

function VerificaCtrlSalida() {
    data = { ti:autorizacionActual.ti };
    AbrirWaiting
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
function presentaListaProducto() {
    datos = {};
    PostGenHtml(datos, buscarListaProductosUrl, function (obj) {

        $("#divti03").html(obj);
        var tb = $("#divti03 #tbListaProd tbody td");
        if (tb.length <= 0) {
            $("#btnCtrlSalida").hide("fast");
        } else {
            $("#btnCtrlSalida").show("fast");
        }
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
