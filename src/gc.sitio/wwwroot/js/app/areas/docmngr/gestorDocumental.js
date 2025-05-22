/**
 * Gestor Documental
 * 
 * Módulo dedicado a gestionar documentos para:
 * - Impresión
 * - Exportación
 * - Envío por email
 * - Envío por WhatsApp
 */

// Namespace para evitar conflictos
var GestorDocumental = (function () {
    // Variables privadas
    let _treeInstance = null;
    let _documentosSeleccionados = [];
    let _solicitudesActivas = [];

    // Configuración
    const config = {
        contenedorArbol: '#docsTreeContainer',
        modalId: '#gestorDocumentalModal',
        urls: {
            obtenerArbol: '',
            imprimir: imprimirDocumentosUrl,
            exportar: exportarDocumentosUrl,
            email: enviarEmailUrl,
            whatsapp: enviarWhatsAppUrl
        }
    };

    // Inicialización del módulo
    function init(urls) {
        // Asignar URLs de las APIs
        if (urls) {
            config.urls = { ...config.urls, ...urls };
        }

        // Inicializar eventos
        _initEventos();

        console.log('Gestor Documental inicializado');
    }

    // Inicialización de eventos
    function _initEventos() {
        // Evento al mostrar el modal
        $(document).on('shown.bs.modal', config.modalId, function () {
            _cargarArbolDocumentos();
        });

        // Evento al ocultar el modal
        $(document).on('hidden.bs.modal', config.modalId, function () {
            _destruirArbol();
        });

        // Eventos de botones de acción
        $(document).on('click', '#btnPrintDocs', _imprimirDocumentos);
        $(document).on('click', '#btnExportDocs', _exportarDocumentos);
        $(document).on('click', '#btnSendEmail', _enviarEmail);
        $(document).on('click', '#btnSendWhatsApp', _enviarWhatsApp);
    }

    // Carga el árbol de documentos
    function _cargarArbolDocumentos() {
        console.log('Cargando árbol de documentos...');

        // Eliminar árbol anterior si existe
        _destruirArbol();

        // Mostrar indicador de carga
        $(config.contenedorArbol).html('<div class="text-center py-4"><div class="spinner-border text-primary" role="status"></div><p class="mt-2">Cargando documentos...</p></div>');

        // Solicitar datos del árbol usando PostGen
        PostGen({}, config.urls.obtenerArbol, function (obj) {
            if (obj.error) {
                AbrirMensaje("ATENCIÓN", obj.msg, function () {
                    $("#msjModal").modal("hide");
                    return true;
                }, false, ["Aceptar"], "error!", null);
                return;
            }

            if (obj.warn && obj.auth) {
                AbrirMensaje("ATENCIÓN", obj.msg, function () {
                    window.location.href = login;
                }, false, ["Aceptar"], "warn!", null);
                return;
            }

            // Parsear los datos del árbol
            let datosArbol;
            try {
                datosArbol = typeof obj.arbol === 'string' ? JSON.parse(obj.arbol) : obj.arbol;
            } catch (error) {
                console.error('Error al parsear datos del árbol:', error);
                AbrirMensaje("ERROR", "Error al procesar los datos del árbol", function () {
                    $("#msjModal").modal("hide");
                    return true;
                }, false, ["Aceptar"], "error!", null);
                return;
            }

            // Completar campos de contacto
            if (obj.cuenta) {
                $('#emailTo').val(obj.cuenta.cta_Email || '');
                $('#whatsappTo').val(obj.cuenta.cta_Celu || '');
            }

            // Inicializar jsTree con configuración específica
            _inicializarArbol(datosArbol);
        });
    }

    // Inicializa el árbol jsTree
    function _inicializarArbol(datos) {
        try {
            console.log('Inicializando árbol con datos:', datos);

            // Vaciar contenedor
            $(config.contenedorArbol).empty();

            // Crear árbol jsTree
            $(config.contenedorArbol).jstree({
                'core': {
                    'data': datos,
                    'themes': {
                        'name': 'default',
                        'responsive': true
                    },
                    'check_callback': true
                },
                'checkbox': {
                    'keep_selected_style': false,
                    'three_state': true,
                    'whole_node': false,
                    'tie_selection': true
                },
                'plugins': ['checkbox', 'types', 'wholerow'],  //
                'types': {
                    'default': {
                        'icon': 'bx bx-file'
                    }
                }
            });

            // Guardar referencia al árbol
            _treeInstance = $(config.contenedorArbol).jstree(true);

            // Evento cuando cambia la selección
            $(config.contenedorArbol).on('changed.jstree', function (e, data) {
                _actualizarSeleccion(data.selected);
            });

            // Evento cuando el árbol está listo
            $(config.contenedorArbol).on('ready.jstree', function () {
                console.log('Árbol listo');

                // Forzar redibujado del árbol para asegurar que se muestren los checkboxes
                setTimeout(function () {
                    if (_treeInstance) {
                        _treeInstance.redraw(true);
                    }
                }, 100);
            });
        } catch (error) {
            console.error('Error al inicializar árbol:', error);
            $(config.contenedorArbol).html('<div class="alert alert-danger">Error al inicializar árbol: ' + error.message + '</div>');
        }
    }

    // Actualiza la lista de documentos seleccionados
    function _actualizarSeleccion(idsSeleccionados) {
        _documentosSeleccionados = [];
        _solicitudesActivas = [];

        if (!_treeInstance || !idsSeleccionados || !idsSeleccionados.length) {
            return;
        }

        // Procesar cada nodo seleccionado
        idsSeleccionados.forEach(function (id) {
            const nodo = _treeInstance.get_node(id);

            // Solo procesar nodos que no son carpetas (que no tienen children o tienen children vacío)
            if (nodo && (!nodo.children || nodo.children.length === 0)) {
                _documentosSeleccionados.push({
                    id: nodo.id,
                    text: nodo.text,
                    data: nodo.data
                });

                // Crear objeto de solicitud para este documento
                _solicitudesActivas.push({
                    ReporteId: parseInt(nodo.id),
                    Filtros: [],  // Esto debe completarse con los filtros adecuados
                    Formato: "P"  // Por defecto PDF
                });
            }
        });

        // Actualizar UI con información de selección
        const cantidadSeleccionados = _documentosSeleccionados.length;

        // Actualizar botones según la selección
        $('.doc-action-btn').prop('disabled', cantidadSeleccionados === 0);

        // Actualizar mensajes de email y whatsapp
        const nombresDocumentos = _documentosSeleccionados.map(doc => doc.text).join(', ');
        const mensajeDocumentos = cantidadSeleccionados > 0
            ? 'Documentos seleccionados: ' + nombresDocumentos
            : 'No hay documentos seleccionados.';

        $('#whatsappMessage').val(mensajeDocumentos);
        $('#emailBody').val(mensajeDocumentos);

        console.log('Documentos seleccionados:', _documentosSeleccionados);
    }

    // Destruye el árbol jsTree
    function _destruirArbol() {
        try {
            if (_treeInstance) {
                _treeInstance.destroy(true);
                _treeInstance = null;
            }
            $(config.contenedorArbol).empty();
        } catch (error) {
            console.warn('Error al destruir árbol:', error);
        }
    }

    // Imprime los documentos seleccionados
    function _imprimirDocumentos() {
        if (_solicitudesActivas.length === 0) {
            AbrirMensaje("ATENCIÓN", "Debe seleccionar al menos un documento para imprimir.", function () {
                $("#msjModal").modal("hide");
                return true;
            }, false, ["Aceptar"], "warn!", null);
            return;
        }

        AbrirWaiting("Generando documentos para impresión...");

        // Si tienes un arreglo arrRepoParams, úsalo aquí para armar las solicitudes
        // Si no, usa los datos de _solicitudesActivas directamente
        let solicitudes = [];
        _documentosSeleccionados.forEach(function (doc) {
            // Si tienes arrRepoParams, descomenta la siguiente línea y ajusta el id
            let params = arrRepoParams[parseInt(doc.id) - 1];
            $.ajax({
                url: config.urls.imprimir,
                type: "POST",
                data: params,
                contentType: "application/json; charset=utf-8",
                success: function (obj) {
                    CerrarWaiting();

                    if (obj.error) {
                        AbrirMensaje("ATENCIÓN", obj.msg, function () {
                            $("#msjModal").modal("hide");
                            return true;
                        }, false, ["Aceptar"], "error!", null);
                        return;
                    }

                    if (obj.warn && obj.auth) {
                        AbrirMensaje("ATENCIÓN", obj.msg, function () {
                            window.location.href = login;
                        }, false, ["Aceptar"], "warn!", null);
                        return;
                    }

                    // Imprimir los documentos generados
                    if (obj.documentos && obj.documentos.length) {
                        obj.documentos.forEach(function (doc, index) {
                            if (doc) {
                                setTimeout(function () {
                                    _imprimirDocumento(doc);
                                }, index * 500);
                            }
                        });

                        if (obj.warn) {
                            AbrirMensaje("AVISO", obj.msg, function () {
                                $("#msjModal").modal("hide");
                                return true;
                            }, false, ["Aceptar"], "warn!", null);
                        } else {
                            AbrirMensaje("ÉXITO", "Documentos enviados a impresión correctamente.", function () {
                                $("#msjModal").modal("hide");
                                return true;
                            }, false, ["Aceptar"], "success", null);
                        }
                    } else {
                        AbrirMensaje("AVISO", "No se pudieron generar documentos para impresión.", function () {
                            $("#msjModal").modal("hide");
                            return true;
                        }, false, ["Aceptar"], "warn!", null);
                    }
                },
                error: function (xhr, status, error) {
                    CerrarWaiting();
                    AbrirMensaje("ERROR", "Error al imprimir documentos: " + error, function () {
                        $("#msjModal").modal("hide");
                        return true;
                    }, false, ["Aceptar"], "error!", null);
                }
            });
        });
       
   
    }

    // Imprime un documento a partir de su contenido base64
    function _imprimirDocumento(contenidoBase64) {
        try {
            const blob = _base64ToBlob(contenidoBase64, 'application/pdf');
            const url = URL.createObjectURL(blob);

            const printWindow = window.open(url);
            printWindow.onload = function () {
                printWindow.print();

                // Liberar recursos
                setTimeout(function () {
                    URL.revokeObjectURL(url);
                }, 1000);
            };
        } catch (error) {
            console.error('Error al imprimir documento:', error);
            _mostrarMensaje('error', 'Error al imprimir documento: ' + error.message);
        }
    }

    // Exporta los documentos seleccionados
    function _exportarDocumentos() {
        if (_solicitudesActivas.length === 0) {
            _mostrarMensaje('error', 'Debe seleccionar al menos un documento para exportar.');
            return;
        }

        // Obtener formato seleccionado
        const formato = $('input[name="exportFormat"]:checked').val() || 'P';

        // Actualizar formato en todas las solicitudes
        _solicitudesActivas.forEach(function (solicitud) {
            solicitud.Formato = formato;
        });

        _mostrarProgreso('Generando documentos para exportación...');

        $.ajax({
            url: config.urls.exportar,
            type: 'POST',
            contentType: 'application/json',
            data: JSON.stringify({
                solicitudes: _solicitudesActivas,
                formato: formato
            }),
            success: function (response) {
                _ocultarProgreso();

                if (response.error) {
                    _mostrarMensaje('error', response.msg);
                    return;
                }

                // Descargar los documentos generados
                if (response.documentos && response.documentos.length) {
                    response.documentos.forEach(function (doc, index) {
                        if (doc) {
                            setTimeout(function () {
                                _descargarDocumento(doc.Base64, doc.Nombre, formato);
                            }, index * 300);
                        }
                    });

                    if (response.warn) {
                        _mostrarMensaje('warning', response.msg);
                    } else {
                        _mostrarMensaje('success', 'Documentos exportados correctamente.');
                    }
                } else {
                    _mostrarMensaje('warning', 'No se pudieron generar documentos para exportación.');
                }
            },
            error: function (xhr, status, error) {
                _ocultarProgreso();
                _mostrarMensaje('error', 'Error al exportar documentos: ' + error);
            }
        });
    }

    // Descarga un documento
    function _descargarDocumento(contenidoBase64, nombre, formato) {
        try {
            // Determinar tipo MIME según formato
            let mimeType;
            let extension;

            switch (formato) {
                case 'P':
                    mimeType = 'application/pdf';
                    extension = 'pdf';
                    break;
                case 'X':
                    mimeType = 'application/vnd.ms-excel';
                    extension = 'xls';
                    break;
                case 'T':
                    mimeType = 'text/plain';
                    extension = 'txt';
                    break;
                default:
                    mimeType = 'application/octet-stream';
                    extension = 'bin';
            }

            const blob = _base64ToBlob(contenidoBase64, mimeType);

            // Nombre de archivo por defecto si no se proporciona
            const nombreArchivo = nombre || 'documento.' + extension;

            // Crear enlace de descarga
            const link = document.createElement('a');
            link.href = URL.createObjectURL(blob);
            link.download = nombreArchivo;

            // Simular clic para descargar
            document.body.appendChild(link);
            link.click();

            // Liberar recursos
            setTimeout(function () {
                URL.revokeObjectURL(link.href);
                document.body.removeChild(link);
            }, 100);
        } catch (error) {
            console.error('Error al descargar documento:', error);
            _mostrarMensaje('error', 'Error al descargar documento: ' + error.message);
        }
    }

    // Envía documentos por email
    function _enviarEmail() {
        if (_solicitudesActivas.length === 0) {
            _mostrarMensaje('error', 'Debe seleccionar al menos un documento para enviar por email.');
            return;
        }

        const emailTo = $('#emailTo').val();
        if (!emailTo) {
            _mostrarMensaje('error', 'Debe especificar un destinatario de email.');
            return;
        }

        const emailSubject = $('#emailSubject').val() || 'Documentos solicitados';
        const emailBody = $('#emailBody').val() || 'Adjunto los documentos solicitados.';

        _mostrarProgreso('Enviando email...');

        $.ajax({
            url: config.urls.email,
            type: 'POST',
            contentType: 'application/json',
            data: JSON.stringify({
                solicitudes: _solicitudesActivas,
                emailTo: emailTo,
                emailSubject: emailSubject,
                emailBody: emailBody
            }),
            success: function (response) {
                _ocultarProgreso();

                if (response.error) {
                    _mostrarMensaje('error', response.msg);
                    return;
                }

                if (response.warn) {
                    _mostrarMensaje('warning', response.msg);
                } else {
                    _mostrarMensaje('success', 'Email enviado correctamente.');
                }
            },
            error: function (xhr, status, error) {
                _ocultarProgreso();
                _mostrarMensaje('error', 'Error al enviar email: ' + error);
            }
        });
    }

    // Envía documentos por WhatsApp
    function _enviarWhatsApp() {
        const whatsappTo = $('#whatsappTo').val();
        if (!whatsappTo) {
            _mostrarMensaje('error', 'Debe especificar un número de teléfono.');
            return;
        }

        const whatsappMessage = $('#whatsappMessage').val() || 'Documentos solicitados';
        const adjuntarArchivos = $('#adjuntarArchivos').is(':checked');

        if (adjuntarArchivos && _solicitudesActivas.length === 0) {
            _mostrarMensaje('error', 'Debe seleccionar al menos un documento para adjuntar.');
            return;
        }

        _mostrarProgreso('Preparando mensaje de WhatsApp...');

        $.ajax({
            url: config.urls.whatsapp,
            type: 'POST',
            contentType: 'application/json',
            data: JSON.stringify({
                solicitudes: adjuntarArchivos ? _solicitudesActivas : null,
                whatsappTo: whatsappTo,
                whatsappMessage: whatsappMessage,
                adjuntarArchivos: adjuntarArchivos
            }),
            success: function (response) {
                _ocultarProgreso();

                if (response.error) {
                    _mostrarMensaje('error', response.msg);
                    return;
                }

                if (response.url) {
                    window.open(response.url, '_blank');
                }

                if (response.warn) {
                    _mostrarMensaje('warning', response.msg);
                } else {
                    _mostrarMensaje('success', 'Mensaje preparado correctamente.');
                }
            },
            error: function (xhr, status, error) {
                _ocultarProgreso();
                _mostrarMensaje('error', 'Error al preparar mensaje de WhatsApp: ' + error);
            }
        });
    }

    // Muestra mensaje de progreso
    function _mostrarProgreso(mensaje) {
        if ($('#procesandoModal').length === 0) {
            $('body').append(`
                <div class="modal fade" id="procesandoModal" tabindex="-1" aria-hidden="true" data-bs-backdrop="static">
                    <div class="modal-dialog modal-dialog-centered">
                        <div class="modal-content">
                            <div class="modal-body text-center py-4">
                                <div class="spinner-border text-primary mb-3" role="status"></div>
                                <p id="procesandoMensaje">${mensaje || 'Procesando...'}</p>
                            </div>
                        </div>
                    </div>
                </div>
            `);
        } else {
            $('#procesandoMensaje').text(mensaje || 'Procesando...');
        }

        $('#procesandoModal').modal('show');
    }

    // Oculta mensaje de progreso
    function _ocultarProgreso() {
        $('#procesandoModal').modal('hide');
    }

    // Muestra mensaje al usuario
    function _mostrarMensaje(tipo, mensaje) {
        const iconos = {
            success: 'bx bx-check-circle',
            warning: 'bx bx-error',
            error: 'bx bx-x-circle'
        };

        const clases = {
            success: 'alert-success',
            warning: 'alert-warning',
            error: 'alert-danger'
        };

        const alertHtml = `
            <div class="alert ${clases[tipo] || 'alert-info'} alert-dismissible fade show" role="alert">
                <i class="${iconos[tipo] || 'bx bx-info-circle'} me-2"></i>
                ${mensaje}
                <button type="button" class="btn-close" data-bs-dismiss="alert" aria-label="Close"></button>
            </div>
        `;

        // Buscar contenedor para mostrar alerta
        const contenedor = $(config.modalId).find('.modal-body').first();

        // Remover alertas anteriores
        contenedor.find('.alert').remove();

        // Insertar nueva alerta al inicio
        contenedor.prepend(alertHtml);

        // Auto-cerrar después de 5 segundos si es éxito
        if (tipo === 'success') {
            setTimeout(function () {
                contenedor.find('.alert').alert('close');
            }, 5000);
        }
    }

    // Convierte Base64 a Blob
    function _base64ToBlob(base64, mime) {
        const byteCharacters = atob(base64);
        const byteArrays = [];

        for (let offset = 0; offset < byteCharacters.length; offset += 512) {
            const slice = byteCharacters.slice(offset, offset + 512);

            const byteNumbers = new Array(slice.length);
            for (let i = 0; i < slice.length; i++) {
                byteNumbers[i] = slice.charCodeAt(i);
            }

            const byteArray = new Uint8Array(byteNumbers);
            byteArrays.push(byteArray);
        }

        return new Blob(byteArrays, { type: mime });
    }

    // API pública del módulo
    return {
        init: init,
        mostrarModal: function () {
            $(config.modalId).modal('show');
        },
        ocultarModal: function () {
            $(config.modalId).modal('hide');
        }
    };
})();

// Función de invocación del gestor documental
function invocarGestorDocumental(modulo, parametros) {
    console.log("Invocando gestor documental con:", modulo, parametros);

    var data = {
        modulo: modulo,
        parametros: parametros || []
    };

    PostGenHtml(data, obtenerModalGestorUrl, function (htmlRespuesta) {
        // Renderizar el modal en el DOM
        $('#gestorDocumentalContainer').html(htmlRespuesta);

        // Inicializar el gestor documental
        GestorDocumental.init({
            obtenerArbol: obtenerArbolDocumentosUrl,
            imprimir: imprimirDocumentosUrl,
            exportar: exportarDocumentosUrl,
            email: enviarEmailUrl,
            whatsapp: enviarWhatsAppUrl
        });

        // Mostrar el modal
        GestorDocumental.mostrarModal();
    });
}
