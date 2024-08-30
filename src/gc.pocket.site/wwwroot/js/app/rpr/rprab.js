$(documnt).ready(function () {
    $("#txtUl").on("blur", validaUL);
    $("#txtBox").on("blur", validaBox);
    $("#btnConfirmar").on("click", ConfirmarBoxUl);
});

function validaUL() {
    var ul = $("#txtUl").val();
    var datos = { ul }
    PostGen(datos, validaUlUrl, function (obj) {
        if (obj.error === true) {
            $("#txtUl").focus();
            AbrirMensaje("Importante", obj.msg, function () {
                $("#msjModal").modal("hide");
                return true;
            }, false, ["Aceptar"], "error!", null);
        }
        else if (obj.warn === true) {
            $("#txtUl").focus();
            AbrirMensaje("Importante", obj.msg, function () {
                $("#msjModal").modal("hide");
                return true;
            }, false, ["Aceptar"], "warn!", null);
        }
        else {
            ControlaMensajeSuccess(obj.msg);
            //solo pasa al otro campo.
            $("#txtBox").focus();

            return true;
        }
    });
    return true;
}

function validaBox() {
    var box = $("#txtBox").val();
    var datos = { box };
    PostGen(datos, validaBoxUrl, function (obj) {
        if (obj.error === true) {
            $("#txtUl").focus();
            AbrirMensaje("Importante", obj.msg, function () {
                $("#msjModal").modal("hide");
                return true;
            }, false, ["Aceptar"], "error!", null);
        }
        else if (obj.warn === true) {
            $("#txtUl").focus();
            AbrirMensaje("Importante", obj.msg, function () {
                $("#msjModal").modal("hide");
                return true;
            }, false, ["Aceptar"], "warn!", null);
        }
        else {
            ControlaMensajeSuccess(obj.msg);
            //solo pasa al otro campo.
            $("#txtBox").focus();

            return true;
        }
    });
    return true;
}

function ConfirmarBoxUl() {
    var box = $("#txtBox").val();
    var ul = $("#txtUl").val();

    var datos = { box, ul };
    PostGen(datos, almacenajeBoxUrl, function (obj) {
        if (obj.error === true) {
            AbrirMensaje("Importante", obj.msg, function () {
                $("#msjModal").modal("hide");
                return true;
            }, false, ["Aceptar"], "error!", null);
        }
        else if (obj.warn === true) {
            AbrirMensaje("Importante", obj.msg, function () {
                $("#msjModal").modal("hide");
                return true;
            }, false, ["Aceptar"], "warn!", null);
        }
        else {
            AbrirMensaje("Importante", obj.msg, function () {
                $("#msjModal").modal("hide");
                return true;
            }, false, ["Aceptar"], "succ!", null);
        }

    });
    return true;
}