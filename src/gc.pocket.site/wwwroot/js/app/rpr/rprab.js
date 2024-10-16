$(function () {
    $("#txtUl").on("click", function () {
        $("#txtBox").prop("disabled", true);
    });
    $("#txtBox").on("click", function () {
        $("#txtBox").val("");
    });

    $("#btnConfirmar").on("click", ConfirmarBoxUl);
    $(".inputEditable").on("keypress", analizaInput);
});

function analizaInput(e) {
    if (e.which === 13) {
        //verificamos quien es
        var who = $(this).prop("id");
        switch (who) {
            case "txtUl":
                validaUL();
                break;
            case "txtBox":
                validaBox();
                break;
            default:
                break;
        }
    }
}

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

            $("#txtBox").prop("disabled", false).focus();
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
            $("#txtBox").focus();
            AbrirMensaje("Importante", obj.msg, function () {
                $("#msjModal").modal("hide");
                return true;
            }, false, ["Aceptar"], "error!", null);
        }
        else if (obj.warn === true) {
            $("#txtBox").focus();
            AbrirMensaje("Importante", obj.msg, function () {
                $("#msjModal").modal("hide");
                return true;
            }, false, ["Aceptar"], "warn!", null);
        }
        else {
            ControlaMensajeSuccess(obj.msg);
            $("#txtBox").val(obj.box);
            //solo pasa al otro campo.
            $("#btnConfirmar").focus();


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