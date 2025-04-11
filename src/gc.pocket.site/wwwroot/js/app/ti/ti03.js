$(function () {

    $("#rbBox").on("click", function () { presentaListaProducto("B"); });
    $("#rbProd").on("click", function () { presentaListaProducto("R"); });
    $("#rbRubro").on("click", function () { presentaListaProducto("P"); });  

   

    AbrirWaiting();
    CargarAutoActual();
    $("#btnCtrlSalida").on("click", VerificaCtrlSalida);
    presentaListaProducto("B");//ordenado por box
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
        else if (obj.warn === true) {
            CerrarWaiting();
            AbrirMensaje("Importante", obj.msg, function () {
                $("#msjModal").modal("hide");
                return true;
            }, false, ["Aceptar"], "warn!", null);
        }
        else {
            CerrarWaiting();
            AbrirMensaje("Importante", obj.msg, function () {
                $("#msjModal").modal("hide");
                if (autorizacionActual.tipoTI === "S") {
                    window.location.href = homeModUrl;

                } else {
                    window.location.href = ConfirmarTIUrl;
                }
                return true;
            }, false, ["Aceptar"], "succ!", null);                     
        }
    });
}
function presentaListaProducto(orden) {
    datos = {orden};
    PostGenHtml(datos, buscarListaProductosUrl, function (obj) {

        $("#divti03").html(obj);
        var tb = $("#divti03 #tbListaProd tbody td");
        if (tb.length <= 0) {
            $("#btnCtrlSalida").hide("fast");
        } else {
            $("#btnCtrlSalida").show("fast");
        }
        CerrarWaiting();

        //verifico si tiene el producto actual
        //si lo tiene lo posiciona en el registro de ese mismo producto

    });
}

function mostrarMensaje(nota) {
    AbrirMensaje("Atención", nota, function () {
        $("#msjModal").modal("hide");
        return true;
    }, false, ["Aceptar"], "info!", null);
}
function limpiarProductoCarrito(id,boxId) {
    AbrirWaiting()
    //aca se validará previamente si la cantidad ingresada corresponde a lo solicitado

    //se procede a enviar el producto a cargar
    var dato = { p_id: id, boxId }
    PostGen(dato, LimpiaProductoCarritoUrl, function (obj) {
        if (obj.error === true) {
            CerrarWaiting();
            AbrirMensaje("Importante", obj.msg, function () {
                $("#msjModal").modal("hide");
                return true;
            }, false, ["Aceptar"], "error!", null);
        }
        else if (obj.warn === true) {
            CerrarWaiting();
            AbrirMensaje("Importante", obj.msg, function () {
                $("#msjModal").modal("hide");
                return true;
            }, false, ["Aceptar"], "error!", null);
        }
        else {
            CerrarWaiting();
            ControlaMensajeSuccess(obj.msg);
            window.location.href = proximoProductoUrl + "?esrubro=false&esbox=false&tiId=" + obj.tiId;
        }
    });


}
