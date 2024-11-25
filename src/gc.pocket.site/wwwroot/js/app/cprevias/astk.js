$(function () {  

    $("#btnVerifica").on("click", VerificaInfoProdBox);
    $("#btnContinua01").on("click", continuaAjusteStock);
    $("#ddlTipoAjuste").on("change", function () { $("#btnContinua01").prop("disabled", false); })

    $(".inputEditable").on("keypress", analizaEnterInput);
})


function VerificaInfoProdBox() {
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
            //cargarControles(obj);
            //SE DEBE LANZAR EL TRIGGER DE CLICK PARA QUE EJECUTE LA BUSQUEDA DE STK DEL BOX
            // $("#btnStkBox").trigger("click");
            return true;
        }
    });
}

function continuaAjusteStock() {
    //obtengo los datos de box y de tipo de ajuste
    var box = $("#boxId").val();
    var taj = $("#ddlTipoAjuste").find(":selected").val()

    window.location.href = cargaProducto + "?box=" + box +"&taj=" + taj;
}