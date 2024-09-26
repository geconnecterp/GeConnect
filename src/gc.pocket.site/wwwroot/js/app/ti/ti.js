$(function () {

    $("#btnContinua01").on("click", validarUsuario);

    $("#btnContinua01").prop("disabled", true);
    AbrirWaiting();
    cargarAutorizacionesPendientes();
});

function cargarAutorizacionesPendientes() {

    var datos = {};
    PostGenHtml(datos, getAUPendientesUrl, function (obj) {

        $("#divGrAUTIPend").html(obj);
        var tb = $("#divGrAUTIPend #tbAuPend tbody td");
        if (tb.length === 0) {
            $("#btnCarrito").show();
        }
        else {
            $("#btnCarrito").hide();
        }
        CerrarWaiting();
    });

    return true;
}

function seleccionarRegistroTR(x) {
    //eliminamos de todos las filas el estilo 
    $("#tbAuPend tbody tr").each(function (index) {
        $(this).removeClass("selected-row");
    });
    $(x).addClass("selected-row");
    var reg = parseInt(x.cells[0].innerText.trim());
    $("#txtTiro").val($("#ti" + reg).val());
    $("#txtNNro").val(x.cells[1].innerText.trim());
    $("#txtUsro").val($("#usu" + reg).val());
    $("#txtFero").val($("#fec" + reg).val());

    $("#btnContinua01").prop("disabled", false);
}

function validarUsuario() {
    AbrirWaiting();
    PostGen({ auId: $("#txtTiro").val() }, validaUsuarioUrl, function (obj) {
        if (obj.error === true) {
            CerrarWaiting();
            AbrirMensaje("Algo pasó", obj.msg, function () {
                $("#msjModal").modal("hide");
                return true;
            }, false, ["Aceptar"], "error!", null);
        }
        else if (obj.warn === true) {
            CerrarWaiting();
            AbrirMensaje("Algo pasó", obj.msg, function () {
                $("#msjModal").modal("hide");
                return true;
            }, false, ["Aceptar"], "warn!", null);
        }
        else {
            CerrarWaiting();
            //no se da el ok al usuario validado. Directamente se direcciona
            window.location.href = trScr02;


            //AbrirMensaje("Usuario Validado", obj.msg, function () {
            //    $("#msjModal").modal("hide");
            //    window.location.href = trScr02;

            //    //return true;
            //}, false, ["Aceptar"], "succ!", null);
            //ControlaMensajeSuccess(obj.msg);
        }
    });
    return true;
}