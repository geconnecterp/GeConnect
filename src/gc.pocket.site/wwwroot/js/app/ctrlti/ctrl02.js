$(function () {
    CargarAutoActual();

    TraeDatosCtrlServer();
});

function InicializaPantallaCtrl02() {
    productosGridCtrl02();
    //$("#P_id").val("");
    //$("#Descipcion").val("");
    //$("#Rubro").val("");
    //$("#up").val(0).prop("disabled", true);
    //$("#fvto").val("").prop("disabled", true);

    //$("#box").val(0).prop("disabled", true);
    //$("#unid").val(0).prop("disabled", true);
    //$("#btnCargaProd").prop("disabled", true);
    //$("#divCtrlTIGrid").empty();


    return true;
}

function TraeDatosCtrlServer() {
    var data = {};
    PostGen(data, cargaProdCtrlDesdeServer, function (obj) {
        if (obj.error === true) {
            CerrarWaiting();
            AbrirMensaje("Datos Ctrl", obj.msg, function () {
                $('#msjModal').modal('hide');
                return true;
            }, false, ["Aceptar"], "warn!", null);
            InicializaPantallaCtrl02();
            return true;
        }
        else {
            CerrarWaiting();           
            InicializaPantallaCtrl02();
            return true;
        }

    });
}

function productosGridCtrl02() {
    var data = {};
    PostGenHtml(data, PresentarProductosSeleccionadosUrl, function (obj) {

        $("#divCtrlTIGrid").html(obj);
        var tb = $("#divCtrlTIGrid #tbProdRPR tbody td");
        if (tb.length <= 0) {
            $("#btnContinua02").hide("fast");
        } else {
            $("#btnContinua02").show("fast");
        }

        if (typeof ocultarTrash !== 'undefined') {
            if (ocultarTrash === true) {
                //ocultamos la 8° columna
                $(".ocultar2").toggle();
                //$("#divCtrlTIGrid #tbProdRPR tbody td:nth-child(7)").toggle();
                //$("#divCtrlTIGrid #tbProdRPR tbody td:nth-child(8)").toggle();
                $("#divCtrlTIGrid #tbProdRPR tbody td:nth-child(9)").toggle();
            }
        }

        $("#txtTiro").val(autorizacionActual.ti);
        $("#txtNNro").val(autorizacionActual.adm_nombre);
        $("#txtUsro").val(autorizacionActual.usu_id);
        $("#txtFero").val(autorizacionActual.fecha);

        return true;
    }, function (obj) {
        ControlaMensajeError(obj.message);
        return true;
    });
}