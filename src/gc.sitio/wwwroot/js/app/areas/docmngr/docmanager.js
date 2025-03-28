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

    $(document).on("click", "#btnEnviarEmail", enviarEmail);
    $(document).on("click", "#btnEnviarWhatsApp", enviarWhatsApp);

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
    var selectedNodes = $('#archivosDispuestos').jstree('get_selected', true);
    if (selectedNodes.length === 0) {
        AbrirMensaje("ATENCIÓN", "No hay archivos seleccionados para exportar.", function () {
            $("#msjModal").modal("hide");
            return true;
        }, false, ["Aceptar"], "error!", null);
        return;
    }

    selectedNodes.forEach(function (node) {
        if (node.data && node.data.archivoB64) {
            var archivoBase64 = node.data.archivoB64;
            var tipo = node.data.tipo;
            var data = {
                formato: formato,
                archivoBase64: archivoBase64,
                nodoId: node.id,
                moduloId: node.parent,
                tipo: tipo
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
                    if (formato === "P") {
                        var pdfWindow = window.open("");
                        pdfWindow.document.write(
                            "<iframe width='100%' height='100%' src='data:application/pdf;base64, " + encodeURI(obj.pdfBase64) + "'></iframe>"
                        );
                    } else if (formato === "X" || formato === "T") {
                        var link = document.createElement('a');
                        link.href = obj.fileUrl;
                        link.download = obj.fileName;
                        document.body.appendChild(link);
                        link.click();
                        document.body.removeChild(link);
                    }
                }
            });
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

function enviarEmail() {
    var selectedNodes = $('#archivosDispuestos').jstree('get_selected', true);
    if (selectedNodes.length === 0) {
        AbrirMensaje("ATENCIÓN", "No hay archivos seleccionados para enviar por email.", function () {
            $("#msjModal").modal("hide");
            return true;
        }, false, ["Aceptar"], "error!", null);
        return;
    }

    //inicializamos variables obteniendo los datos cargados para enviar el mail
    var emailTo = $("#emailTo").val();
    var emailSubject = $("#emailSubject").val();
    var emailBody = $("#emailBody").val();
    var totalSize = 0;
    var maxSize = 25 * 1024 * 1024; // 25MB
    var archivos = [];

    selectedNodes.forEach(function (node) {
        if (node.data && node.data.archivoB64) {
            var archivoBase64 = node.data.archivoB64;
            var archivoSize = (archivoBase64.length * (3 / 4)) - (archivoBase64.indexOf('=') > 0 ? (archivoBase64.length - archivoBase64.indexOf('=')) : 0);
            totalSize += archivoSize;

            if (totalSize > maxSize) {
                AbrirMensaje("ATENCIÓN", "El tamaño total de los archivos seleccionados excede el límite de 25MB para el envío por email.", function () {
                    $("#msjModal").modal("hide");
                    return true;
                }, false, ["Aceptar"], "error!", null);
                return;
            }

            archivos.push({
                archivoBase64: archivoBase64,
                nombre: node.text
            });
        }
    });

    var data = {
        archivos: archivos,
        emailTo: emailTo,
        emailSubject: emailSubject,
        emailBody: emailBody
    };

    PostGen(data, enviarEmailUrl, function (obj) {
        if (obj.error === true) {
            AbrirMensaje("Atención!", obj.msg, function () {
                $("#msjModal").modal("hide");
                return true;
            }, false, ["Aceptar"], "error!", null);
        } else {
            AbrirMensaje("Éxito", "El email ha sido enviado correctamente.", function () {
                $("#msjModal").modal("hide");
                return true;
            }, false, ["Aceptar"], "success", null);
        }
    });
}

function enviarWhatsApp() {
    var selectedNode = $('#archivosDispuestos').jstree('get_selected', true)[0];
    if (!selectedNode || !selectedNode.data || !selectedNode.data.archivoB64) {
        AbrirMensaje("ATENCIÓN", "No hay archivo seleccionado para enviar por WhatsApp.", function () {
            $("#msjModal").modal("hide");
            return true;
        }, false, ["Aceptar"], "error!", null);
        return;
    }

    var archivoBase64 = selectedNode.data.archivoB64;
    var whatsappTo = $("#whatsappTo").val();
    var whatsappMessage = $("#whatsappMessage").val();

    var data = {
        archivoBase64: archivoBase64,
        whatsappTo: whatsappTo,
        whatsappMessage: whatsappMessage
    };

    PostGen(data, enviarWhatsAppUrl, function (obj) {
        if (obj.error === true) {
            AbrirMensaje("Atención!", obj.msg, function () {
                $("#msjModal").modal("hide");
                return true;
            }, false, ["Aceptar"], "error!", null);
        } else {
            AbrirMensaje("Éxito", "El mensaje de WhatsApp ha sido enviado correctamente.", function () {
                $("#msjModal").modal("hide");
                return true;
            }, false, ["Aceptar"], "success", null);
        }
    });
}