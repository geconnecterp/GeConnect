/**
 * Document Manager Module
 * 
 * Este módulo se encarga de gestionar las operaciones de documentos:
 * - Impresión
 * - Exportación
 * - Envío de correo electrónico
 * - Envío de WhatsApp
 */

$(function () {
    $(document).on("click", "#btnCancelarGD", function () {
        $("#documentManagerModal").modal("hide");
        $("#modalGestorDocumental").hide();
    });

    $(document).on("click", "input[name='rdgenera']", function () {
        if ($(this).is(":checked")) {
            $("#btnGenerarFile").prop("disabled", false);
        }
        else {
            $("#btnGenerarFile").prop("disabled", true);
        }
    });

    $(document).on("click", "#btnGenerarFile", invocaGenerarArchivo);
});

function invocacionGestorDoc(data) {
    PostGenHtml(data, OrquestadorDeModulosUrl, function (obj) {
        $("#modalGestorDocumental").html(obj);
        //detectaremos primero si hubo error
        if ($("input#msgError").length > 0 || $("input#msgWarm").length > 0) {
            $("#modalGestorDocumental").show();
        }
        else {
            //si no hubo error, mostramos el modal 
            $("#modalGestorDocumental").show();
            $("#documentManagerModal").modal("show");
        }
    });
}

function inicializaGestorDocumental() {
    $("#modalGestorDocumental").hide();
    $("#documentManagerModal").modal("hide");
}

function invocaGenerarArchivo() {
    var formato = $("input[name='rdgenera']:checked").val();
    var modulo = fkey; //resgardo el modulo que llamo la funcionalidad
    var data = {
        modulo: modulo,
        formato: formato
    };
    PostGen(data, generadorArchivoUrl, function (obj) {
        if (obj.error === true) {
            AbrirMensaje("Atención!", obj.msg, function () {
                $("#msjModal").modal("hide");
                return true;
            }, false, ["Aceptar"], "error!", null);
        }
        else if (obj.warn === true) {
            AbrirMensaje("Atención!", obj.msg, function () {
                if (obj.auth === true) {
                    window.location.href = login;
                } else {
                    $("#msjModal").modal("hide");
                    return true;
                }
            }, false, ["Aceptar"], "error!", null);
        }
        else {
            var pdfWindow = window.open("");
            pdfWindow.document.write(
                "<iframe width='100%' height='100%' src='data:application/pdf;base64, " + encodeURI(obj.pdfBase64) + "'></iframe>"
            );
        }
    });
}