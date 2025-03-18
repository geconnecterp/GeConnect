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

    $("#btnGenerarFile").on("click", invocaGenerarArchivo);
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

}