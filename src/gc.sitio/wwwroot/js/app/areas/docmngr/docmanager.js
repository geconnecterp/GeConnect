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
    // Código para añadir efectos al abrir el modal
    $(document).on('shown.bs.modal', '#docmgrmodal', function () {
        // Animar la aparición del árbol de archivos
        $("#archivosDispuestos").css('opacity', 0).animate({
            opacity: 1
        }, 300);

        // Añadir clases para mejorar la interactividad
        $("#documentManagerTabs .nav-link").addClass('transition-all duration-200');
    });

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
    AbrirWaiting("Espere mientras se genera(n) el/los archivos...")
    var formato = $("input[name='rdgenera']:checked").val();
    var selectedNodes = $('#archivosDispuestos').jstree('get_selected', true);
    if (selectedNodes.length === 0) {
        CerrarWaiting();
        AbrirMensaje("ATENCIÓN", "No hay archivos seleccionados para exportar.", function () {
            $("#msjModal").modal("hide");
            return true;
        }, false, ["Aceptar"], "error!", null);
        return;
    }

    selectedNodes.forEach(function (node) {
        if (node.data) {
            var id = node.id;
            if (arrRepoParams[id - 1] !== undefined) {
                var data1 = arrRepoParams[id - 1];

                var data2 = {
                    formato: formato
                };
                //unimos ambos json
                var data = $.extend({}, data1, data2);

                PostGen(data, generadorArchivoUrl, function (obj) {
                    CerrarWaiting();
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
                        var archivoBase64 = obj.base64;

                        // Configura el tipo MIME según el formato
                        var mimeType;
                        var extension;
                        switch (formato) {
                            case "P":
                                mimeType = 'application/pdf';
                                extension = 'pdf';
                                break;
                            case "X":
                                mimeType = 'application/vnd.ms-excel';
                                extension = 'xls';
                                break;
                            case "T":
                                mimeType = 'text/plain';
                                extension = 'txt';
                                break;
                            default:
                                mimeType = 'application/octet-stream';
                                extension = 'bin';
                        }

                        var blob = base64ToBlob(archivoBase64, mimeType);

                        // Genera el nombre del archivo si no viene en la respuesta
                        var nombreArchivo = obj.name || "archivo." + extension;

                        // Crea el enlace para descargar el archivo
                        var link = document.createElement('a');
                        link.href = URL.createObjectURL(blob);
                        link.download = nombreArchivo;

                        // Agrega el enlace al DOM, lo activa y luego lo elimina
                        document.body.appendChild(link);
                        link.click();

                        // Buena práctica: liberar la URL del objeto
                        setTimeout(function () {
                            URL.revokeObjectURL(link.href);
                            document.body.removeChild(link);
                        }, 100);
                    }
                });
            }
            else {
                CerrarWaiting();
                AbrirMensaje("Atención!", "El Informe no esta disponible, aún. Ejecutelo visualmente y recien podrá realizar la descarga del mismo.",
                    function () {
                        $("#msjModal").modal("hide");
                        return true;
                    }, false, ["Aceptar"], "error!", null);
            }
        }
    });
}

function presentarArchivos() {
    // Mostrar estado de carga
    $("#archivosDispuestos").html(`
        <div class="d-flex justify-content-center align-items-center p-5">
            <div class="spinner-border text-primary" role="status">
                <span class="visually-hidden">Cargando...</span>
            </div>
            <span class="ms-2">Cargando documentos disponibles...</span>
        </div>
    `);

    PostGen({}, presentarArchivosUrl, function (obj) {
        if (obj.error === true) {
            AbrirMensaje("ATENCIÓN", obj.msg, function () {
                $("#msjModal").modal("hide");
                return true;
            }, false, ["Aceptar"], "error!", null);
        }
        else if (obj.warn === true) {
            AbrirMensaje("ATENCIÓN", obj.msg, function () {
                $("#msjModal").modal("hide");
            }, false, ["Aceptar"], "warn!", null);
        }
        else {
            let cuenta = obj.cuenta;
            $("#emailTo").val(cuenta.cta_Email);
            $("#whatsappTo").val(cuenta.cta_Celu);

            jsonP = JSON.parse(obj.arbol);

            // Modificar el árbol para deshabilitar los nodos sin parámetros
            procesarNodosArbol(jsonP);

            // Limpiar y destruir árbol existente
            $("#archivosDispuestos").jstree("destroy").empty();

            // Inicializamos el árbol jsTree con configuración mejorada
            $("#archivosDispuestos").jstree({
                "core": {
                    "data": jsonP,
                    "themes": {
                        "icons": true,
                        "dots": false,
                        "stripes": false,
                        "responsive": true,
                        "variant": "large"
                    },
                    "animation": 200
                },
                "checkbox": {
                    "keep_selected_style": false,
                    "three_state": false
                },
                "plugins": ['checkbox', 'types', 'wholerow'],
                "types": {
                    "disabled": {
                        "select_node": false,
                        "open_node": false,
                        "close_node": false,
                        "icon": "bx bx-lock-alt text-danger"
                    },
                    "locked": {
                        "select_node": false,
                        "open_node": false,
                        "close_node": false,
                        "icon": "bx bx-lock-alt text-danger"
                    },
                    "default": {
                        "icon": "bx bxs-file-pdf"
                    },
                    "folder": {
                        "icon": "bx bxs-folder"
                    }
                }
            }).on('ready.jstree', function () {
                $(this).find('.jstree-anchor').css({
                    'padding': '4px 7px',
                    'font-size': '1.05em'
                });

                $(this).find('.jstree-anchor').append('<span class="ms-1 badge bg-light text-dark count-badge"></span>');

                // Añadir conteos a las carpetas
                $(this).find('.jstree-anchor').each(function () {
                    const nodeId = $(this).parent().attr('id');
                    const node = $('#archivosDispuestos').jstree(true).get_node(nodeId);
                    if (node.children && node.children.length) {
                        const badge = $(this).find('.count-badge');
                        badge.text(node.children.length);
                    } else {
                        $(this).find('.count-badge').remove();
                    }
                });
            });
        }
    });
}

/**
 * Procesa recursivamente el árbol de nodos verificando si cada nodo tiene parámetros guardados en arrRepoParams
 * Si un nodo no tiene parámetros guardados, se marca como deshabilitado
 * @param {Array} nodos - Array de nodos del árbol
 */
function procesarNodosArbol(nodos) {
    if (!nodos || !Array.isArray(nodos)) return;

    nodos.forEach(function (nodo) {
        // Marcar carpetas con el tipo adecuado para el icono
        if (nodo.parent === "#") {
            nodo.type = "folder";
        } else if (nodo.children && nodo.children.length > 0) {
            nodo.type = "folder";
        }

        // Si el nodo tiene un ID numérico (es un reporte) y no es un nodo padre
        if (nodo.id && !isNaN(parseInt(nodo.id)) && nodo.parent !== "#") {
            const reporteId = parseInt(nodo.id);

            // Verificar si el reporte tiene parámetros guardados
            if (typeof arrRepoParams === 'undefined' ||
                reporteId <= 0 ||
                reporteId > arrRepoParams.length ||
                arrRepoParams[reporteId - 1] === null ||
                arrRepoParams[reporteId - 1] === undefined ||
                (arrRepoParams[reporteId - 1] && (
                    !arrRepoParams[reporteId - 1].parametros ||
                    Object.keys(arrRepoParams[reporteId - 1].parametros).length === 0
                ))
            ) {
                // No hay parámetros guardados o son insuficientes, deshabilitar el nodo
                nodo.state = nodo.state || {};
                nodo.state.disabled = true;

                nodo.type = "locked";

                nodo.a_attr = nodo.a_attr || {};
                nodo.a_attr.class = (nodo.a_attr.class || "") + " disabled-node";
                nodo.li_attr = nodo.li_attr || {};
                nodo.li_attr.title = "Este reporte no está disponible porque no se han ejecutado sus parámetros";

                nodo.text = nodo.text + " 🔒";
            }
        }

        // Procesamiento recursivo para hijos
        if (nodo.children && Array.isArray(nodo.children)) {
            procesarNodosArbol(nodo.children);
        }
    });
}

function imprimirArchivoSeleccionado() {

    var selectedNodes = $('#archivosDispuestos').jstree('get_selected', true);

    // Filtrar para eliminar el nodo raíz
    selectedNodes = selectedNodes.filter(function (node) {
        return node.parent !== "#" && node.parent !== null;
    });

    if (selectedNodes.length === 0) {
        AbrirMensaje("ATENCIÓN", "No hay archivos seleccionados para imprimir.", function () {
            $("#msjModal").modal("hide");
            return true;
        }, false, ["Aceptar"], "error!", null);
        return;
    }
    else {
        AbrirWaiting("Espere mientras se genera el archivo para imprimir...");
        selectedNodes.forEach(function (node) {
            if (node.data) {
                var id = node.id;
                if (arrRepoParams[id - 1] !== undefined) {
                    data = arrRepoParams[id - 1];

                    const solicitudReporte = {
                        Reporte: data.reporte,
                        Parametros: data.parametros,
                        Ids: data.parametros.Ids,
                        Titulo: node.text,
                        SubTitulo: data.subTitulo,
                        Observacion: data.observacion || "",
                        Formato: "P",
                        LogoPath: "",
                        Administracion: data.administracion || administracion
                    };

                    PostGen(solicitudReporte, repoApiUrl, function (obj) {
                        CerrarWaiting();
                        if (obj.error === true) {
                            AbrirMensaje("Atención", obj.resultado_msg, function () {
                                $("#msjModal").modal("hide");
                                return true;
                            }, false, ["Aceptar"], "warn!", null);
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
                        } else {
                            var archivoBase64 = obj.base64;
                            if (!archivoBase64.includes("|")) {
                                var blob = b64toBlob(archivoBase64, 'application/pdf');
                                var url = URL.createObjectURL(blob);
                                var printWindow = window.open(url);
                                printWindow.onload = function () {
                                    printWindow.print();
                                };
                            }
                            else {
                                var arrArchivoBase64 = archivoBase64.split("|");
                                arrArchivoBase64.forEach(function (elemento, indice, arrayOriginal) {
                                    if (elemento != "") {
                                        var blob = b64toBlob(elemento, 'application/pdf');
                                        var url = URL.createObjectURL(blob);
                                        var printWindow = window.open(url);
                                        printWindow.onload = function () {
                                            printWindow.print();
                                        };
                                    }
                                });
                            }
                        }
                    });
                }
                else {
                    CerrarWaiting();
                    AbrirMensaje("Atención!", "El Informe no esta disponible, aún. Ejecutelo visualmente y recien podrá realizar la impresión del mismo.",
                        function () {
                            $("#msjModal").modal("hide");
                            return true;
                        }, false, ["Aceptar"], "error!", null);
                }
            }
        });
    }
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
    AbrirWaiting("Espere mientras se envia el correo electrónico...");
    var selectedNodes = $('#archivosDispuestos').jstree('get_selected', true);
    if (selectedNodes.length === 0) {
        AbrirMensaje("ATENCIÓN", "No hay archivos seleccionados para enviar por email.", function () {
            $("#msjModal").modal("hide");
            return true;
        }, false, ["Aceptar"], "error!", null);
        return;
    }

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
        CerrarWaiting();
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
    var selectedNodes = $('#archivosDispuestos').jstree('get_selected', true);
    var whatsappTo = $("#whatsappTo").val();
    var whatsappMessage = $("#whatsappMessage").val();
    var adjuntarArchivos = $("#adjuntarArchivos").is(":checked");
    var totalSize = 0;
    var maxSize = 100 * 1024 * 1024; // 100MB
    var archivos = [];

    if (adjuntarArchivos) {
        if (selectedNodes.length === 0) {
            AbrirMensaje("ATENCIÓN", "No hay archivos seleccionados para enviar por WhatsApp. ¿Se CONTINUA sin archivos adjuntos?", function (resp) {
                if (resp === "SI") {
                    $("#adjuntarArchivos").prop("checked", false);
                    adjuntarArchivos = false;
                    $("#msjModal").modal("hide");
                    return true;
                }
                else {
                    $("#msjModal").modal("hide");
                    return false;
                }
            }, true, ["SI", "NO"], "warn!", null);
        }
    }

    if (adjuntarArchivos) {
        selectedNodes.forEach(function (node) {
            if (node.data && node.data.archivoB64) {
                var archivoBase64 = node.data.archivoB64;
                var archivoSize = (archivoBase64.length * (3 / 4)) - (archivoBase64.indexOf('=') > 0 ? (archivoBase64.length - archivoBase64.indexOf('=')) : 0);
                totalSize += archivoSize;

                if (totalSize > maxSize) {
                    AbrirMensaje("ATENCIÓN", "El tamaño total de los archivos seleccionados excede el límite de 100MB para el envío por WhatsApp.", function () {
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
                window.open(obj.url, "_blank");
                AbrirMensaje("Éxito", "El mensaje de WhatsApp ha sido enviado correctamente.", function () {
                    $("#msjModal").modal("hide");
                    $("#whatsappMessage").val("");
                    $("#whatsappTo").val("");
                    return true;
                }, false, ["Aceptar"], "success", null);
            }
        });
    }
    else {
        var data = {
            whatsappTo: whatsappTo,
            whatsappMessage: whatsappMessage
        };
        PostGen(data, enviarWhatsAppUrl, function (obj) {
            if (obj.error === false) {
                window.open(obj.url, "_blank");
                AbrirMensaje("Éxito", "El mensaje de WhatsApp ha sido enviado correctamente.", function () {
                    $("#msjModal").modal("hide");
                    $("#whatsappMessage").val("");
                    $("#whatsappTo").val("");
                    return true;
                }, false, ["Aceptar"], "error!", null);
            } else {
                AbrirMensaje("Atención", obj.msj, function () {
                    $("#msjModal").modal("hide");
                    return true;
                }, false, ["Aceptar"], "error!", null);
            }
        });
    }
}