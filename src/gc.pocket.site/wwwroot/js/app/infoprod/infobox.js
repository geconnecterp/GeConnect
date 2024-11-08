$(function () {
    $("#btnVerifica").on("click", VerificaInfoProdBox);
    $("#btnStkBox").on("click", ObtenerInfoStk);
    $("#btnBuscarMov").on("click", ObtenerInfoMovStk);
    
    var f = new Date();
    //var month = ('0' + (f.getMonth() + 1)).slice(-2); // Asegura que el mes siempre tenga dos dígitos
    //var day = ('0' + f.getDate()).slice(-2); // Asegura que el día siempre tenga dos dígitos
    //var newfecha = f.getFullYear() + '-' + month + '-' + day;

    $("#fdesde").val(formatoFechaYMD(restarFecha(f, 7)));
    $("#fhasta").val(formatoFechaYMD(f));

    InicializarInfoBox();
})

function InicializarInfoBox()
{
    $("#boxId").val("");
    $("#depoId").val("");
    $("#gondId").val("");
    $("#nivId").val("");
    $("#rackId").val("");
    $("#zonaId").val("");

    //las grillas
    $("#gridStkBox").empty();
    $("#gridMov").empty();

    return true;
}

function ObtenerInfoMovStk() {
    var datos = { boxId: $("#boxId").val(), sm: $("#TmId").find(":selected").val(), desde: $("#fdesde").val(), hasta: $("#fhasta").val() };
    AbrirWaiting("Buscando Movimientos de productos en el BOX...")
    PostGenHtml(datos, ObtenerBoxInfoMovStkUrl, function (obj) {
        $("#gridMov").html(obj);
        CerrarWaiting();
        return true;
    });
}

function ObtenerInfoStk() {
    var datos = { boxId: $("#boxId").val() };
    AbrirWaiting("Buscando Stock almacenados en el BOX...")
    PostGenHtml(datos, ObtenerBoxInfoStkUrl, function (obj) {
        $("#gridStkBox").html(obj);
        CerrarWaiting();
        return true;
    });
}

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
            cargarControles(obj);
            //SE DEBE LANZAR EL TRIGGER DE CLICK PARA QUE EJECUTE LA BUSQUEDA DE STK DEL BOX
            $("#btnStkBox").trigger("click");
            return true;
        }
    });
}

function cargarControles(obj) {
    $("#depoId").val(obj.info.depo_id);
    $("#gondId").val(obj.info.box_gondola);
    $("#nivId").val(obj.info.box_nivel);
    $("#rackId").val(obj.info.box_rack);
    $("#zonaId").val(obj.info.box_zona);

    return true;
}