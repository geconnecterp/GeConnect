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
        $("#docmgrmodal").modal("hide");
        $("#modalGestorDocumental").hide();
    });

    $(document).on("click", "#btnArchImprimir", imprimirArchivoSeleccionado);

    $(document).on("click", "input[name='rdgenera']", function () {
        if ($(this).is(":checked")) {
            $("#btnGenerarFile").prop("disabled", false);
        }
        else {
            $("#btnGenerarFile").prop("disabled", true);
        }
    });

    $(document).on("click", "#btnGenerarFile", invocaGenerarArchivo);

    inicializaArbolArchivos();
});

function inicializaArbolArchivos() {
    //borramos el contenido del arbol.
    $("#archivosDispuestos").jstree("destroy").empty();
}

function invocacionGestorDoc(data) {
    PostGenHtml(data, OrquestadorDeModulosUrl, function (obj) {
        $("#modalGestorDocumental").html(obj);
        //detectaremos primero si hubo error
        if ($("input#msgError").length > 0 || $("input#msgWarm").length > 0) {
            $("#modalGestorDocumental").show();
        }
        else {
            //si no hubo error, mostramos el modal
            //antes de abrir el modal, se cargará el arbol de archivos
            presentarArchivos();

            $("#modalGestorDocumental").show();
            

            $("#docmgrmodal").modal("show");
        }
    });
}

function inicializaGestorDocumental() {
    $("#modalGestorDocumental").hide();
    $("#docmgrmodal").modal("hide");
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

function presentarArchivos() {
    PostGen({}, presentarArchivosUrl, function (obj) {
        if (obj.error === true) {
            AbrirMensaje("ATENCIÓN", obj.msg, function () {
                $("#msjModal").modal("hide");
                return true;
            }, false, ["Aceptar"], "error!", null);
        }
        else if (obj.warn === true) {

        }
        else {
            jsonP = $.parseJSON(obj.arbol);
            $("#archivosDispuestos").jstree("destroy").empty();

            $("#archivosDispuestos").jstree({
                "core": { "data": jsonP },
                "checkbox": {
                    "keep_selected_style": false
                },
                "plugins": ['checkbox']
            });
        }
    });
}

function imprimirArchivoSeleccionado() {
    var selectedNodes = $('#archivosDispuestos').jstree('get_selected', true);
    if (selectedNodes.length === 0) {
        AbrirMensaje("ATENCIÓN", "No hay archivos seleccionados para imprimir.", function () {
            $("#msjModal").modal("hide");
            return true;
        }, false, ["Aceptar"], "error!", null);
        return;
    }

    selectedNodes.forEach(function (node) {
        if (node.data && node.data.archivoB64) {
            var archivoBase64 = node.data.archivoB64;
            var blob = base64ToBlob(archivoBase64, 'application/pdf');
            var url = URL.createObjectURL(blob);

            var printWindow = window.open(url);
            printWindow.onload = function () {
                printWindow.print();
            };
        }
    });
}

function base64ToBlob(base64, mime) {
    var byteCharacters = atob(base64);
    var byteArrays = [];

    for (var offset = 0; offset < byteCharacters.length; offset += 512) {
        var slice = byteCharacters.slice(offset, offset + 512);

        var byteNumbers = new Array(slice.length);
        for (var i = 0; i < slice.length; i++) {
            byteNumbers[i] = slice.charCodeAt(i);
        }

        var byteArray = new Uint8Array(byteNumbers);
        byteArrays.push(byteArray);
    }

    return new Blob(byteArrays, { type: mime });
}