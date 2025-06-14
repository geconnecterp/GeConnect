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
    $("<style>")
        .prop("type", "text/css")
        .html(`
        /* Estilos para el modal del gestor documental */
            #docmgrmodal .modal-content {
                border: none;
                border-radius: 8px;
                box-shadow: 0 5px 15px rgba(0,0,0,0.2);
            }

            /* CAMBIO 1: Gradiente naranja para el encabezado (similar al título de la vista) */
            #docmgrmodal .modal-header {
                background: linear-gradient(135deg, #b8860b 0%, #daa520 100%);
                color: #333;
                border-bottom: none;
                border-radius: 8px 8px 0 0;
                padding: 15px 20px;
            }

            /* CAMBIO 1: Adaptar color de texto del título */
            #docmgrmodal .modal-title {
                font-weight: 600;
                display: flex;
                align-items: center;
                color: #333;
                text-shadow: 0 1px 1px rgba(255,255,255,0.3);
            }
            
            #docmgrmodal .modal-title:before {
                content: '\\eb25';
                font-family: 'boxicons';
                margin-right: 10px;
                font-size: 1.2em;
            }

            #docmgrmodal .close {
                color: #333;
                opacity: 0.8;
                transition: opacity 0.2s;
            }

            #docmgrmodal .close:hover {
                opacity: 1;
            }

            #docmgrmodal .modal-body {
                padding: 20px;
                background-color: #f8f9fa;
            }

            /* CAMBIO 3: Árbol de documentos más grande para mejor visualización */
            #archivosDispuestos {
                max-height: 500px; /* Aumentar altura máxima */
                overflow-y: auto;
                border: 1px solid #dee2e6;
                border-radius: 6px;
                padding: 10px;
                background-color: white;
                box-shadow: inset 0 1px 3px rgba(0,0,0,0.1);
                font-size: 1.05em; /* Aumentar tamaño de texto */
            }

            /* CAMBIO 3: Más espacio entre elementos del árbol */
            .jstree-default .jstree-node {
                margin-left: 20px;
                margin-top: 3px;
                margin-bottom: 3px;
            }

            /* CAMBIO 2: Asegurar que el icono de candado sea visible */
            .jstree-default .jstree-icon.bx-lock-alt {
                color: #dc3545 !important;
                font-size: 1.1em !important;
                opacity: 1 !important;
                visibility: visible !important;
            }
            
            /* Más espacio para los textos en los nodos */
            .jstree-default .jstree-anchor {
                padding-right: 10px !important;
                line-height: 28px !important;
                height: 28px !important;
            }

            /* Estilizar pestañas con los colores del tema principal */
            #documentManagerTabs .nav-link {
                border-radius: 4px 4px 0 0;
                padding: 8px 15px;
                font-weight: 500;
                color: #495057;
                transition: all 0.2s;
            }
            
            #documentManagerTabs .nav-link:hover {
                background-color: rgba(184, 134, 11, 0.1);
            }

            #documentManagerTabs .nav-link.active {
                color: #b8860b;
                font-weight: 600;
                border-color: #dee2e6 #dee2e6 #fff;
                border-bottom: 3px solid #b8860b;
            }

            #documentManagerTabs .nav-link i {
                margin-right: 6px;
                vertical-align: middle;
            }

            /* CAMBIO 2: Mejorar estilo de nodos deshabilitados */
            .disabled-node {
                color: #9da9b0 !important;
                cursor: not-allowed !important;
                opacity: 0.7 !important;
                text-decoration: line-through;
            }
            
            .jstree-default .jstree-disabled {
                color: #9da9b0;
                cursor: not-allowed;
            }

            /* CAMBIO 2: Aumentar visibilidad de los iconos en nodos deshabilitados */
            .jstree-default .jstree-disabled > i.jstree-icon {
                opacity: 1 !important;
                color: #dc3545 !important;
            }

            /* Mejorar botones de acción manteniendo consistencia con el tema */
            .doc-action-btn, 
            #btnArchImprimir,
            #btnEnviarEmail, 
            #btnEnviarWhatsApp {
                border-radius: 4px;
                padding: 6px 16px;
                font-weight: 500;
                transition: all 0.2s;
                box-shadow: 0 2px 4px rgba(0,0,0,0.1);
                background-color: #b8860b;
                border-color: #daa520;
            }

            #btnCancelarGD {
                border-radius: 4px;
                padding: 6px 16px;
                font-weight: 500;
                transition: all 0.2s;
            }

            #btnGenerarFile {
                border-radius: 4px;
                padding: 6px 16px;
                font-weight: 500;
                transition: all 0.2s;
                box-shadow: 0 2px 4px rgba(0,0,0,0.1);
                background-color: #b8860b;
                border-color: #daa520;
            }

            .doc-action-btn:hover, 
            #btnArchImprimir:hover,
            #btnEnviarEmail:hover, 
            #btnEnviarWhatsApp:hover,
            #btnGenerarFile:hover {
                background-color: #daa520;
                border-color: #b8860b;
            }

            /* Estilizar los campos de formulario */
            #docmgrmodal .form-control {
                border-radius: 4px;
                border: 1px solid #ced4da;
                padding: 8px 12px;
                transition: border-color 0.2s;
            }
            
            #docmgrmodal .form-control:focus {
                border-color: #b8860b;
                box-shadow: 0 0 0 0.2rem rgba(184, 134, 11, 0.25);
            }

            /* Panel de contenido de pestañas */
            #documentManagerTabs .tab-content {
                background-color: white;
                border: 1px solid #dee2e6;
                border-top: none;
                border-radius: 0 0 4px 4px;
                padding: 20px;
                box-shadow: 0 1px 3px rgba(0,0,0,0.05);
            }

            /* Estilo especial para nodos seleccionados */
            .jstree-clicked {
                background-color: #faf5e6 !important;
                box-shadow: inset 0 0 1px #b8860b !important;
                border-radius: 3px !important;
            }

            /* Estilo para mostrar el ícono de candado al final del texto */
            .jstree-default .disabled-node-icon {
                margin-left: 5px;
                font-size: 0.9em;
                opacity: 0.8;
                color: #999;
                display: inline-block;
            }

            /* Estilo para los radio buttons de formato */
            #docmgrmodal .form-check-input:checked {
                background-color: #b8860b;
                border-color: #b8860b;
            }

            /* Estilos para encabezados dentro del modal */
            #docmgrmodal h5, #docmgrmodal .h5 {
                color: #b8860b;
                font-weight: 600;
            }

            /* Estilos consistentes para las etiquetas de formulario */
            #docmgrmodal label {
                color: #495057;
                font-weight: 500;
            }
    `)
        .appendTo("head");

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
        if (node.data ) {
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
            // Inicializamos el árbol jsTree con configuración mejorada
            $("#archivosDispuestos").jstree({
                "core": {
                    "data": jsonP,
                    "themes": {
                        "icons": true,
                        "dots": false,
                        "stripes": false,
                        "responsive": true,
                        "variant": "large" // CAMBIO: Usar variante grande para los nodos
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
                    "locked": { // CAMBIO: Tipo personalizado para nodos bloqueados
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
                // CAMBIO: Aumentar el tamaño del árbol y sus elementos
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

            // Código para cargar mensajes en WhatsApp y email...
            // (código existente sin cambios)
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

            // Verificar si el reporte tiene parámetros guardados - MODIFICACIÓN: verificación más genérica
            if (typeof arrRepoParams === 'undefined' ||
                reporteId <= 0 ||
                reporteId > arrRepoParams.length ||
                arrRepoParams[reporteId - 1] === null ||
                arrRepoParams[reporteId - 1] === undefined ||
                // Verificar que tiene los campos mínimos necesarios
                (arrRepoParams[reporteId - 1] && (
                    !arrRepoParams[reporteId - 1].parametros ||
                    Object.keys(arrRepoParams[reporteId - 1].parametros).length === 0
                ))
            ) {
                // No hay parámetros guardados o son insuficientes, deshabilitar el nodo
                nodo.state = nodo.state || {};
                nodo.state.disabled = true;

                // CAMBIO: Usar un tipo personalizado para nodos deshabilitados
                nodo.type = "locked";

                nodo.a_attr = nodo.a_attr || {};
                nodo.a_attr.class = (nodo.a_attr.class || "") + " disabled-node";
                nodo.li_attr = nodo.li_attr || {};
                nodo.li_attr.title = "Este reporte no está disponible porque no se han ejecutado sus parámetros";

                // CAMBIO: Agregar emoji de candado al texto para asegurar visibilidad
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

    // Filtrar para eliminar el nodo raíz (que típicamente tiene parent = "#" o parent = null)
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
            if (node.data) { //&& node.data.archivoB64
                var id = node.id;
                if (arrRepoParams[id - 1] !== undefined) {
                    data = arrRepoParams[id - 1];

                    // Crear objeto de solicitud con nombres de propiedades que coincidan con el modelo C#
                    const solicitudReporte = {
                        Reporte: data.reporte,  // Con mayúscula para coincidir con C#
                        Parametros: data.parametros,  // Con mayúscula
                        Titulo: data.titulo,  // Con mayúscula
                        Observacion: data.observacion || "",  // Con mayúscula
                        Formato: "P",  // Con mayúscula (PDF)
                        LogoPath: "",  // Con mayúscula
                        Administracion : data.administracion || administracion
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
                            var blob = b64toBlob(archivoBase64, 'application/pdf');
                            var url = URL.createObjectURL(blob);
                            var printWindow = window.open(url);
                            printWindow.onload = function () {
                                printWindow.print();
                            };
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

    var whatsappTo = $("#whatsappTo").val();
    var whatsappMessage = $("#whatsappMessage").val();
    var adjuntarArchivos = $("#adjuntarArchivos").is(":checked");
    var totalSize = 0;
    var maxSize = 100 * 1024 * 1024; // 100MB
    var archivos = [];

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