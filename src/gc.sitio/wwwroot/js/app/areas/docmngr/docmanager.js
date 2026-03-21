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

    // Contador de caracteres para WhatsApp
    $(document).on('input', '#whatsappMessage', function () {
        const length = $(this).val().length;
        const maxLength = 5000;

        let color = 'text-muted';
        if (length > 4000) color = 'text-warning';
        if (length > 4800) color = 'text-danger';

        $('#whatsappCharCounter')
            .text(`${length}/${maxLength} caracteres`)
            .attr('class', `text-muted ${color}`);
    });

    // Actualizar información de archivos para WhatsApp cuando cambia la selección
    $(document).on('changed.jstree', '#archivosDispuestos', function () {
        updateWhatsAppFilesInfo();
    });

    inicializaArbolArchivos();
});


/**
 * ✅ FUNCIÓN PRINCIPAL: Enviar Email con archivos adjuntos o enlaces
 * CORREGIDO: Unifica generación de PDFs y URLs para TODOS los proveedores
 */
function enviarEmail() {
    console.log('📧 Iniciando proceso de envío de email...');

    // ✅ Detectar proveedor seleccionado
    const emailProvider = $('input[name="emailProvider"]:checked').val();
    console.log(`📧 Proveedor seleccionado: ${emailProvider}`);

    // Validaciones básicas
    const emailTo = $('#emailTo').val().trim();
    const emailSubject = $('#emailSubject').val().trim();
    const emailBody = $('#emailBody').val().trim();

    if (!emailTo) {
        AbrirMensaje("ATENCIÓN", "Por favor ingresa un correo electrónico", function () {
            $("#msjModal").modal("hide");
        }, false, ["Aceptar"], "warn!", null);
        return;
    }

    if (!emailSubject) {
        AbrirMensaje("ATENCIÓN", "Por favor ingresa un asunto", function () {
            $("#msjModal").modal("hide");
        }, false, ["Aceptar"], "warn!", null);
        return;
    }

    if (!emailBody) {
        AbrirMensaje("ATENCIÓN", "Por favor ingresa un mensaje", function () {
            $("#msjModal").modal("hide");
        }, false, ["Aceptar"], "warn!", null);
        return;
    }

    // ✅ Obtener archivos seleccionados
    const selectedNodes = $('#archivosDispuestos').jstree('get_selected', true);
    const archivosSeleccionados = selectedNodes.filter(function (node) {
        const esNodoRaiz = node.parent === "#" || node.parent === null;
        const esCarpeta = node.children && node.children.length > 0;
        return !esNodoRaiz && !esCarpeta;
    });

    if (archivosSeleccionados.length === 0) {
        AbrirMensaje("ATENCIÓN",
            "⚠️ Debe seleccionar al menos un documento para procesar.\n\n" +
            "Por favor, selecciona uno o más documentos del árbol de la izquierda.",
            function () {
                $("#msjModal").modal("hide");
            }, false, ["Aceptar"], "warn!", null);
        return; // ✅ Detener ejecución
    }

    console.log(`📊 Total archivos seleccionados: ${archivosSeleccionados.length}`);

    // ✅ NUEVO: Procesar archivos de manera unificada ANTES de llamar a los proveedores específicos
    if (archivosSeleccionados.length > 0) {
        procesarArchivosParaEmail(emailProvider, emailTo, emailSubject, emailBody, archivosSeleccionados);
    } else {
        // Sin archivos, llamar directamente según el proveedor
        switch (emailProvider) {
            case 'outlookweb':
                enviarEmailOutlookWeb(emailTo, emailSubject, emailBody, []);
                break;
            case 'outlookdesktop':
                enviarEmailOutlookLocal(emailTo, emailSubject, emailBody, []);
                break;
            default:
                enviarEmailGmail(emailTo, emailSubject, emailBody, []);
        }
    }
}

/**
 * ✅ FUNCIÓN MODIFICADA: Genera URLs directamente (sin PDFs)
 */
function procesarArchivosParaEmail(emailProvider, emailTo, emailSubject, emailBody, archivosSeleccionados) {
    AbrirWaiting("Generando enlaces de documentos...");

    console.log(`📎 Procesando ${archivosSeleccionados.length} archivo(s) para ${emailProvider}`);

    // ✅ Generar URLs usando LinkController
    generarURLsDocumentos(archivosSeleccionados)
        .then(enlaces => {
            CerrarWaiting();
            console.log(`✅ ${enlaces.length} URL(s) generada(s)`);

            // ✅ Construir mensaje con URLs clicables
            let cuerpoConEnlaces = emailBody;

            // ✅ CORREGIDO: Detectar si ya es HTML
            const esHtml = cuerpoConEnlaces.includes('<br') ||
                cuerpoConEnlaces.includes('<p>') ||
                cuerpoConEnlaces.includes('<div>');

            // ✅ Para Outlook Web y Gmail: Convertir a HTML si es necesario
            if (!esHtml && (emailProvider === 'outlookweb' || emailProvider === 'gmail')) {
                cuerpoConEnlaces = cuerpoConEnlaces
                    .replace(/\r\n/g, '\n')
                    .replace(/\n\n/g, '<br/><br/>')
                    .replace(/\n/g, '<br/>');
            }

            // ✅ Agregar enlaces según el proveedor
            if (enlaces.length > 0) {
                if (emailProvider === 'outlookweb' || emailProvider === 'gmail') {
                    // ✅ HTML para Outlook Web y Gmail
                    cuerpoConEnlaces += '<br/><br/>📎 <strong>Documentos disponibles:</strong><br/><br/>';
                    enlaces.forEach((enlace, index) => {
                        cuerpoConEnlaces += `${index + 1}. <a href="${enlace.url}" target="_blank" style="color: #0066cc; text-decoration: none;">${enlace.nombre}</a><br/><br/>`;
                    });
                } else {
                    // ✅ Texto plano para Outlook Desktop (mailto)
                    cuerpoConEnlaces += '\n\n📎 Documentos disponibles:\n\n';
                    enlaces.forEach((enlace, index) => {
                        cuerpoConEnlaces += `${index + 1}. ${enlace.nombre}\n   ${enlace.url}\n\n`;
                    });
                }
            }

            console.log(`📧 Delegando a proveedor: ${emailProvider}`);

            // ✅ Delegar al proveedor específico CON URLs
            switch (emailProvider) {
                case 'outlookweb':
                    enviarEmailOutlookWeb(emailTo, emailSubject, cuerpoConEnlaces, enlaces);
                    break;
                case 'outlookdesktop':
                    enviarEmailOutlookLocal(emailTo, emailSubject, cuerpoConEnlaces, enlaces);
                    break;
                default:
                    enviarEmailGmailConAdjuntos(emailTo, emailSubject, cuerpoConEnlaces, []);
            }
        })
        .catch(error => {
            CerrarWaiting();
            console.error('❌ Error al generar URLs:', error);

            AbrirMensaje("Error",
                `❌ Error al generar enlaces:\n\n${error.message}`,
                function () {
                    $("#msjModal").modal("hide");
                }, false, ["Aceptar"], "error!", null);
        });
}

///**
// * ✅ NUEVA FUNCIÓN: Procesa archivos de manera unificada
// * Genera PDFs, guarda en FileStore y obtiene URLs públicas
// * Luego delega al proveedor específico (Gmail/Outlook)
// */
//function procesarArchivosParaEmail(emailProvider, emailTo, emailSubject, emailBody, archivosSeleccionados) {
//    AbrirWaiting("Generando PDFs y guardando en servidor...");

//    console.log(`📎 Procesando ${archivosSeleccionados.length} archivo(s) para ${emailProvider}`);

//    // ✅ PASO 1: Generar PDFs en tiempo real
//    const promesasGeneracion = archivosSeleccionados.map(node => {
//        console.log(`🔄 Generando PDF: "${node.text}" (ID: ${node.id})`);
//        return generarPDFEnTiempoReal(node);
//    });

//    Promise.all(promesasGeneracion)
//        .then(archivosGenerados => {
//            console.log(`✅ ${archivosGenerados.length} PDF(s) generados exitosamente`);

//            // ✅ PASO 2: Guardar TODOS los archivos en FileStore (obtener URLs públicas)
//            console.log('💾 Guardando archivos en FileStore para obtener URLs públicas...');

//            // ✅ CORREGIDO: Retornar AMBOS archivosGenerados y enlaces juntos
//            return guardarArchivosGrandesEnServidor(archivosGenerados)
//                .then(enlaces => ({ archivosGenerados, enlaces }));
//        })
//        .then(({ archivosGenerados, enlaces }) => {  // ✅ CORREGIDO: Desestructurar ambos
//            CerrarWaiting();
//            console.log(`✅ ${enlaces.length} URL(s) pública(s) obtenida(s) del FileStore`);

//            // ✅ Construir mensaje con URLs
//            let cuerpoConEnlaces = emailBody;

//            // ✅ Detectar si el cuerpo ya es HTML (contiene <br/> o <p>)
//            const esHtml = cuerpoConEnlaces.includes('<br') ||
//                cuerpoConEnlaces.includes('<p>') ||
//                cuerpoConEnlaces.includes('<div>');

//            if (!esHtml && (emailProvider === 'outlookweb' || emailProvider === 'gmail')) {
//                // ✅ Convertir saltos de línea a <br/> para Outlook Web y Gmail
//                cuerpoConEnlaces = cuerpoConEnlaces
//                    .replace(/\r\n/g, '\n')      // Normalizar CRLF a LF
//                    .replace(/\n\n/g, '<br/><br/>') // Párrafos dobles
//                    .replace(/\n/g, '<br/>');    // Saltos de línea simples

//                console.log('✅ Cuerpo convertido a HTML (saltos de línea → <br/>)');
//            } else if (emailProvider === 'outlookdesktop') {
//                // ✅ Para Outlook Desktop (mailto), mantener saltos de línea normales
//                // porque NO soporta HTML
//                console.log('ℹ️ Manteniendo texto plano para Outlook Desktop (mailto)');
//            }

//            if (enlaces.length > 0) {
//                // ✅ Agregar enlaces como HTML clicables (para Gmail y Outlook Web)
//                if (emailProvider === 'outlookweb' || emailProvider === 'gmail') {
//                    cuerpoConEnlaces += '<br/><br/>📎 <strong>Documentos disponibles:</strong><br/><br/>';
//                    enlaces.forEach((enlace, index) => {
//                        cuerpoConEnlaces += `${index + 1}. <a href="${enlace.url}" target="_blank" style="color: #0066cc; text-decoration: none;">${enlace.nombre}</a><br/><br/>`;
//                    });
//                } else {
//                    // ✅ Para Outlook Desktop, usar URLs planas
//                    cuerpoConEnlaces += '\n\n📎 Documentos disponibles:\n\n';
//                    enlaces.forEach((enlace, index) => {
//                        cuerpoConEnlaces += `${index + 1}. ${enlace.nombre}\n   ${enlace.url}\n\n`;
//                    });
//                }
//            }

//            console.log(`📧 Delegando a proveedor: ${emailProvider}`);

//            // ✅ PASO 3: Delegar al proveedor específico CON URLs
//            switch (emailProvider) {
//                case 'outlookweb':
//                    enviarEmailOutlookWeb(emailTo, emailSubject, cuerpoConEnlaces, enlaces);
//                    break;
//                case 'outlookdesktop':
//                    enviarEmailOutlookLocal(emailTo, emailSubject, cuerpoConEnlaces, enlaces);
//                    break;
//                default:
//                    // ✅ CORREGIDO: Ahora archivosGenerados SÍ está disponible
//                    clasificarYEnviarPorGmail(emailTo, emailSubject, emailBody, archivosGenerados, enlaces);
//            }
//        })
//        .catch(error => {
//            CerrarWaiting();
//            console.error('❌ Error al procesar archivos:', error);

//            AbrirMensaje("Error",
//                `❌ Error al procesar archivos:\n\n${error.message}\n\nRevisa la consola del navegador (F12) para más detalles.`,
//                function () {
//                    $("#msjModal").modal("hide");
//                }, false, ["Aceptar"], "error!", null);
//        });
//}

///**
// * ✅ NUEVA FUNCIÓN: Procesa archivos de manera unificada
// * Genera PDFs, guarda en FileStore y obtiene URLs públicas
// * Luego delega al proveedor específico (Gmail/Outlook)
// */
//function procesarArchivosParaEmail(emailProvider, emailTo, emailSubject, emailBody, archivosSeleccionados) {
//    AbrirWaiting("Generando PDFs y guardando en servidor...");

//    console.log(`📎 Procesando ${archivosSeleccionados.length} archivo(s) para ${emailProvider}`);

//    // ✅ PASO 1: Generar PDFs en tiempo real
//    const promesasGeneracion = archivosSeleccionados.map(node => {
//        console.log(`🔄 Generando PDF: "${node.text}" (ID: ${node.id})`);
//        return generarPDFEnTiempoReal(node);
//    });

//    Promise.all(promesasGeneracion)
//        .then(archivosGenerados => {
//            console.log(`✅ ${archivosGenerados.length} PDF(s) generados exitosamente`);

//            // ✅ PASO 2: Guardar TODOS los archivos en FileStore (obtener URLs públicas)
//            console.log('💾 Guardando archivos en FileStore para obtener URLs públicas...');
//            return guardarArchivosGrandesEnServidor(archivosGenerados);
//        })
//        .then(enlaces => {
//            CerrarWaiting();
//            console.log(`✅ ${enlaces.length} URL(s) pública(s) obtenida(s) del FileStore`);

//            // ✅ Construir mensaje con URLs
//            let cuerpoConEnlaces = emailBody;

//            // ✅ Detectar si el cuerpo ya es HTML (contiene <br/> o <p>)
//            const esHtml = cuerpoConEnlaces.includes('<br') ||
//                cuerpoConEnlaces.includes('<p>') ||
//                cuerpoConEnlaces.includes('<div>');

//            if (!esHtml && (emailProvider === 'outlookweb' || emailProvider === 'gmail')) {
//                // ✅ Convertir saltos de línea a <br/> para Outlook Web y Gmail
//                cuerpoConEnlaces = cuerpoConEnlaces
//                    .replace(/\r\n/g, '\n')      // Normalizar CRLF a LF
//                    .replace(/\n\n/g, '<br/><br/>') // Párrafos dobles
//                    .replace(/\n/g, '<br/>');    // Saltos de línea simples

//                console.log('✅ Cuerpo convertido a HTML (saltos de línea → <br/>)');
//            } else if (emailProvider === 'outlookdesktop') {
//                // ✅ Para Outlook Desktop (mailto), mantener saltos de línea normales
//                // porque NO soporta HTML
//                console.log('ℹ️ Manteniendo texto plano para Outlook Desktop (mailto)');
//            }

//            //if (enlaces.length > 0) {
//            //    // ✅ NUEVO: Agregar enlaces como HTML clicables
//            //    cuerpoConEnlaces += '\n\n📎 <strong>Documentos disponibles:</strong><br/><br/>';
//            //    enlaces.forEach((enlace, index) => {
//            //        cuerpoConEnlaces += `${index + 1}. <a href="${enlace.url}" target="_blank" style="color: #0066cc; text-decoration: none;">${enlace.nombre}</a><br/><br/>`;
//            //    });
//            //}


//            if (enlaces.length > 0) {
//                // ✅ Agregar enlaces como HTML clicables (para Gmail y Outlook Web)
//                if (emailProvider === 'outlookweb' || emailProvider === 'gmail') {
//                    cuerpoConEnlaces += '<br/><br/>📎 <strong>Documentos disponibles:</strong><br/><br/>';
//                    enlaces.forEach((enlace, index) => {
//                        cuerpoConEnlaces += `${index + 1}. <a href="${enlace.url}" target="_blank" style="color: #0066cc; text-decoration: none;">${enlace.nombre}</a><br/><br/>`;
//                    });
//                } else {
//                    // ✅ Para Outlook Desktop, usar URLs planas
//                    cuerpoConEnlaces += '\n\n📎 Documentos disponibles:\n\n';
//                    enlaces.forEach((enlace, index) => {
//                        cuerpoConEnlaces += `${index + 1}. ${enlace.nombre}\n   ${enlace.url}\n\n`;
//                    });
//                }
//            }

//            console.log(`📧 Delegando a proveedor: ${emailProvider}`);

//            // ✅ PASO 3: Delegar al proveedor específico CON URLs
//            switch (emailProvider) {
//                case 'outlookweb':
//                    enviarEmailOutlookWeb(emailTo, emailSubject, cuerpoConEnlaces, enlaces);
//                    break;
//                case 'outlookdesktop':
//                    enviarEmailOutlookLocal(emailTo, emailSubject, cuerpoConEnlaces, enlaces);
//                    break;
//                default:
//                    // Para Gmail, clasificar por tamaño (pequeños adjuntos + grandes por enlace)
//                    clasificarYEnviarPorGmail(emailTo, emailSubject, emailBody, archivosGenerados, enlaces);
//            }
//        })
//        .catch(error => {
//            CerrarWaiting();
//            console.error('❌ Error al procesar archivos:', error);

//            AbrirMensaje("Error",
//                `❌ Error al procesar archivos:\n\n${error.message}\n\nRevisa la consola del navegador (F12) para más detalles.`,
//                function () {
//                    $("#msjModal").modal("hide");
//                }, false, ["Aceptar"], "error!", null);
//        });
//}

///**
// * ✅ FUNCIÓN AUXILIAR: Clasifica archivos por tamaño para Gmail
// * (Gmail puede adjuntar archivos pequeños directamente)
// */
//function clasificarYEnviarPorGmail(emailTo, emailSubject, emailBody, archivosGenerados, enlaces) {
//    const LIMITE_MB = 5;
//    const LIMITE_BYTES = LIMITE_MB * 1024 * 1024;

//    const archivosPequeños = [];
//    const enlacesGrandes = [];

//    archivosGenerados.forEach((archivo, index) => {
//        const tamañoMB = (archivo.tamañoBytes / 1024 / 1024).toFixed(2);

//        if (archivo.tamañoBytes <= LIMITE_BYTES) {
//            archivosPequeños.push(archivo);
//            console.log(`  📎 Pequeño: ${archivo.nombre} (${tamañoMB} MB) - Adjuntar directamente`);
//        } else {
//            enlacesGrandes.push(enlaces[index]); // Usar enlace del FileStore
//            console.log(`  🔗 Grande: ${archivo.nombre} (${tamañoMB} MB) - Usar enlace del FileStore`);
//        }
//    });

//    // Construir cuerpo con información de archivos
//    let cuerpoFinal = emailBody;

//    if (enlacesGrandes.length > 0) {
//        // ✅ NUEVO: Agregar enlaces como HTML clicables
//        cuerpoFinal += '\n\n🔗 <strong>Archivos grandes disponibles para descarga:</strong><br/><br/>';
//        enlacesGrandes.forEach((enlace, index) => {
//            cuerpoFinal += `${index + 1}. <a href="${enlace.url}" target="_blank" style="color: #0066cc; text-decoration: none;">${enlace.nombre}</a><br/><br/>`;
//        });
//    }

//    // Enviar por Gmail con adjuntos pequeños
//    enviarEmailGmailConAdjuntos(emailTo, emailSubject, cuerpoFinal, archivosPequeños);
//}

/**
 * ✅ FUNCIÓN MODIFICADA: Envía email por SMTP usando Gmail
 * Ahora recibe archivos YA clasificados
 */
function enviarEmailGmailConAdjuntos(emailTo, emailSubject, emailBody, archivosPequeños) {
    AbrirWaiting("Enviando email por Gmail...");

    const archivosParaAdjuntar = archivosPequeños.map(a => ({
        archivoBase64: a.base64,
        nombre: a.nombre
    }));

    console.log(`📎 Archivos a adjuntar: ${archivosParaAdjuntar.length}`);

    $.ajax({
        url: '/ControlComun/GestorImpresion/EnviarEmail',
        type: 'POST',
        contentType: 'application/json',
        data: JSON.stringify({
            archivos: archivosParaAdjuntar,
            emailTo: emailTo,
            emailSubject: emailSubject,
            emailBody: emailBody
        }),
        success: function (response) {
            CerrarWaiting();

            if (response.error) {
                AbrirMensaje("Error", response.msg || "Error al enviar email", function () {
                    $("#msjModal").modal("hide");
                }, false, ["Aceptar"], "error!", null);
            } else {
                console.log('✅ Email enviado exitosamente por Gmail');

                AbrirMensaje("Éxito",
                    `✅ Email enviado exitosamente a ${emailTo}\n\n` +
                    `📎 Archivos adjuntos: ${archivosPequeños.length}`,
                    function () {
                        $("#msjModal").modal("hide");
                    }, false, ["Aceptar"], "success", null);
            }
        },
        error: function (xhr, status, error) {
            CerrarWaiting();
            console.error('❌ Error al enviar email por Gmail:', error);

            let errorMessage = '❌ Error al enviar email:\n\n';

            try {
                const errorResponse = JSON.parse(xhr.responseText);
                errorMessage += errorResponse.msg || error;
            } catch (e) {
                errorMessage += error;
            }

            AbrirMensaje("Error", errorMessage, function () {
                $("#msjModal").modal("hide");
            }, false, ["Aceptar"], "error!", null);
        }
    });
}

// ✅ MANTENER: La función enviarEmailGmail() original SOLO para compatibilidad
// Ahora internamente llama a las nuevas funciones unificadas
function enviarEmailGmail(emailTo, emailSubject, emailBody, archivosSeleccionados) {
    if (archivosSeleccionados.length === 0) {
        // Sin archivos, enviar directamente
        enviarEmailGmailConAdjuntos(emailTo, emailSubject, emailBody, []);
    } else {
        // Con archivos, usar el flujo unificado
        procesarArchivosParaEmail('gmail', emailTo, emailSubject, emailBody, archivosSeleccionados);
    }
}

/**
 * ✅ FUNCIÓN MODIFICADA: Envía email abriendo Outlook Web (deeplink)
 * Ahora recibe enlaces del FileStore
 */
function enviarEmailOutlookWeb(emailTo, emailSubject, emailBody, enlaces) {
    AbrirWaiting("Abriendo Outlook Web...");

    console.log('🌐 Abriendo Outlook Web con deeplink');
    console.log(`  Para: ${emailTo}`);
    console.log(`  Enlaces incluidos: ${enlaces.length}`);

    $.ajax({
        url: '/ControlComun/GestorImpresion/GenerateOutlookWebLink',
        type: 'POST',
        contentType: 'application/json',
        data: JSON.stringify({
            To: emailTo,
            Subject: emailSubject,
            Body: emailBody // Ya incluye los enlaces del FileStore
        }),
        success: function (response) {
            CerrarWaiting();

            if (response.success && response.outlookWebLink) {
                const newWindow = window.open(response.outlookWebLink, '_blank');

                if (!newWindow || newWindow.closed || typeof newWindow.closed === 'undefined') {
                    AbrirMensaje("Advertencia",
                        '⚠️ El navegador bloqueó la ventana emergente.\n\n' +
                        'Permite ventanas emergentes para este sitio o copia el enlace:\n' +
                        response.outlookWebLink,
                        function () {
                            $("#msjModal").modal("hide");
                        }, false, ["Aceptar"], "warn!", null);
                } else {
                    setTimeout(() => {
                        AbrirMensaje("Éxito",
                            `✅ Outlook Web abierto exitosamente\n\n` +
                            `📧 Destinatario: ${emailTo}\n` +
                            `🔗 Enlaces incluidos: ${enlaces.length}\n\n` +
                            `Los enlaces están disponibles en el mensaje.`,
                            function () {
                                $("#msjModal").modal("hide");
                            }, false, ["Aceptar"], "success", null);
                    }, 500);
                }
            } else {
                AbrirMensaje("Error", response.message || "Error al abrir Outlook Web", function () {
                    $("#msjModal").modal("hide");
                }, false, ["Aceptar"], "error!", null);
            }
        },
        error: function (xhr, status, error) {
            CerrarWaiting();
            console.error('❌ Error al abrir Outlook Web:', error);

            let errorMessage = '❌ Error al abrir Outlook Web:\n\n';

            try {
                const errorResponse = JSON.parse(xhr.responseText);
                errorMessage += errorResponse.message || error;
            } catch (e) {
                errorMessage += error;
            }

            AbrirMensaje("Error", errorMessage, function () {
                $("#msjModal").modal("hide");
            }, false, ["Aceptar"], "error!", null);
        }
    });
}

/**
 * ✅ FUNCIÓN MODIFICADA: Envía email abriendo Outlook Local (mailto)
 * Ahora recibe enlaces del FileStore
 */
function enviarEmailOutlookLocal(emailTo, emailSubject, emailBody, enlaces) {
    AbrirWaiting("Abriendo Outlook Local...");

    console.log('💻 Abriendo Outlook Local con mailto');
    console.log(`  Para: ${emailTo}`);
    console.log(`  Enlaces incluidos: ${enlaces.length}`);

    $.ajax({
        url: '/ControlComun/GestorImpresion/GenerateMailtoLink',
        type: 'POST',
        contentType: 'application/json',
        data: JSON.stringify({
            To: emailTo,
            Subject: emailSubject,
            Body: emailBody // Ya incluye los enlaces del FileStore
        }),
        success: function (response) {
            CerrarWaiting();

            if (response.success && response.mailtoLink) {
                window.location.href = response.mailtoLink;

                setTimeout(() => {
                    AbrirMensaje("Éxito",
                        `✅ Outlook Local abierto exitosamente\n\n` +
                        `📧 Destinatario: ${emailTo}\n` +
                        `🔗 Enlaces incluidos: ${enlaces.length}\n\n` +
                        `Los enlaces están disponibles en el mensaje.`,
                        function () {
                            $("#msjModal").modal("hide");
                        }, false, ["Aceptar"], "success", null);
                }, 500);
            } else {
                AbrirMensaje("Error", response.message || "Error al abrir Outlook Local", function () {
                    $("#msjModal").modal("hide");
                }, false, ["Aceptar"], "error!", null);
            }
        },
        error: function (xhr, status, error) {
            CerrarWaiting();
            console.error('❌ Error al abrir Outlook Local:', error);

            let errorMessage = '❌ Error al abrir Outlook Local:\n\n';

            try {
                const errorResponse = JSON.parse(xhr.responseText);
                errorMessage += errorResponse.message || error;
            } catch (e) {
                errorMessage += error;
            }

            AbrirMensaje("Error", errorMessage, function () {
                $("#msjModal").modal("hide");
            }, false, ["Aceptar"], "error!", null);
        }
    });
}

/**
 * Función: Enviar mensaje por WhatsApp Web con archivos
 * ✅ CORREGIDO: Filtro unificado con Email
 */
function enviarWhatsApp() {
    console.log('📱 Iniciando proceso de envío por WhatsApp...');

    const whatsappTo = $('#whatsappTo').val().trim();
    let whatsappMessage = $('#whatsappMessage').val().trim();

    if (!whatsappTo) {
        AbrirMensaje("ATENCIÓN", "Por favor ingresa un número de teléfono", function () {
            $("#msjModal").modal("hide");
        }, false, ["Aceptar"], "warn!", null);
        return;
    }

    if (!whatsappMessage) {
        whatsappMessage = generarContenidoWhatsAppDesdeConfig(
            window.datosCtaActual || {
                id: 'N/A',
                nombre: $('#whatsappTo').val() || 'Cliente/Proveedor',
                email: '',
                tipo: 'E'
            }
        );
    }

    const cleanNumber = whatsappTo.replace(/[\s\-\(\)]/g, '');
    if (!cleanNumber.startsWith('+')) {
        AbrirMensaje("ATENCIÓN",
            "El número debe incluir el código de país\n\nEjemplos:\n" +
            "• Argentina: +5491123456789\n" +
            "• México: +521234567890\n" +
            "• Perú: +51999999999",
            function () {
                $("#msjModal").modal("hide");
            }, false, ["Aceptar"], "warn!", null);
        return;
    }

    // ✅ CORREGIDO: Usar el MISMO filtro que Email
    const selectedNodes = $('#archivosDispuestos').jstree('get_selected', true);
    const archivosSeleccionados = selectedNodes.filter(function (node) {
        const esNodoRaiz = node.parent === "#" || node.parent === null;
        const esCarpeta = node.children && node.children.length > 0;  // ✅ IGUAL QUE EMAIL
        return !esNodoRaiz && !esCarpeta;
    });

    if (archivosSeleccionados.length === 0) {
        AbrirMensaje("ATENCIÓN",
            "⚠️ Debe seleccionar al menos un documento para procesar.\n\n" +
            "Por favor, selecciona uno o más documentos del árbol de la izquierda.",
            function () {
                $("#msjModal").modal("hide");
            }, false, ["Aceptar"], "warn!", null);
        return; // ✅ Detener ejecución
    }

    // ✅ DIAGNÓSTICO: Logging detallado
    console.log(`🔍 Total nodos seleccionados (antes de filtrar): ${selectedNodes.length}`);
    console.log(`📊 Total archivos válidos (después de filtrar): ${archivosSeleccionados.length}`);

    if (archivosSeleccionados.length > 0) {
        console.log('📎 Archivos seleccionados:');
        archivosSeleccionados.forEach((node, index) => {
            console.log(`  ${index + 1}. ${node.text} (ID: ${node.id})`);
        });
    }

    if (archivosSeleccionados.length === 0) {
        console.log('📱 Enviando WhatsApp sin archivos adjuntos');
        enviarWhatsAppSinArchivos(cleanNumber, whatsappMessage);
        return;
    }

    console.log(`📎 Procesando ${archivosSeleccionados.length} archivo(s) para WhatsApp`);

    AbrirWaiting("Generando enlaces de documentos...");

    // ✅ NUEVO FLUJO: Generar URLs directamente
    generarURLsDocumentos(archivosSeleccionados)
        .then(enlaces => {
            CerrarWaiting();

            if (!enlaces || !Array.isArray(enlaces) || enlaces.length === 0) {
                throw new Error("No se pudieron generar los enlaces");
            }

            console.log(`✅ ${enlaces.length} enlace(s) generado(s)`);

            // Construir mensaje con enlaces
            let mensajeFinal = whatsappMessage;

            mensajeFinal += '\n\n📎 Archivos disponibles para descarga:\n';
            enlaces.forEach((enlace, index) => {
                mensajeFinal += `${index + 1}. ${enlace.nombre}\n${enlace.url}\n\n`;
            });

            if (mensajeFinal.length > 5000) {
                throw new Error(`Mensaje muy largo (${mensajeFinal.length} caracteres). Máximo: 5000.`);
            }

            console.log('📱 Enviando WhatsApp con enlaces');
            enviarWhatsAppConEnlaces(cleanNumber, mensajeFinal, enlaces.length);
        })
        .catch(error => {
            CerrarWaiting();
            console.error('❌ Error al procesar archivos para WhatsApp:', error);

            AbrirMensaje("Error",
                `❌ Error al generar enlaces:\n\n${error.message}`,
                function () {
                    $("#msjModal").modal("hide");
                }, false, ["Aceptar"], "error!", null);
        });
}

/**
 * Envía mensaje de WhatsApp SIN archivos adjuntos
 * @param {string} numero - Número de teléfono con código de país
 * @param {string} mensaje - Mensaje a enviar
 */
function enviarWhatsAppSinArchivos(numero, mensaje) {
    AbrirWaiting("Abriendo WhatsApp Web...");

    console.log('=== Abriendo WhatsApp Web (sin archivos) ===');
    console.log('Para:', numero);
    console.log('Mensaje:', mensaje);

    $.ajax({
        url: '/ControlComun/GestorImpresion/GenerateWhatsAppWebLink',
        type: 'POST',
        contentType: 'application/json',
        data: JSON.stringify({
            To: numero,
            Message: mensaje
        }),
        success: function (response) {
            CerrarWaiting();
            
            if (response.success && response.whatsappWebLink) {
                const newWindow = window.open(response.whatsappWebLink, '_blank');
                
                if (!newWindow || newWindow.closed || typeof newWindow.closed === 'undefined') {
                    AbrirMensaje("Advertencia", 
                        '⚠️ El navegador bloqueó la ventana emergente.\n\n' +
                        'Permite ventanas emergentes para este sitio o copia el enlace:\n' + 
                        response.whatsappWebLink,
                        function () {
                            $("#msjModal").modal("hide");
                        }, false, ["Aceptar"], "warn!", null);
                } else {
                    setTimeout(() => {
                        AbrirMensaje("Éxito", 
                            `✅ ${response.message}\n\n` +
                            `📱 Destinatario: ${response.to}`,
                            function () {
                                $("#msjModal").modal("hide");
                                $('#whatsappForm')[0].reset();
                                $('#whatsappCharCounter').text('0/5000 caracteres');
                            }, false, ["Aceptar"], "success", null);
                    }, 500);
                }
            } else {
                AbrirMensaje("Error", response.message, function () {
                    $("#msjModal").modal("hide");
                }, false, ["Aceptar"], "error!", null);
            }
        },
        error: function (xhr, status, error) {
            CerrarWaiting();
            console.error('Error:', error);
            
            let errorMessage = '❌ Error al abrir WhatsApp Web:\n\n';
            
            try {
                const errorResponse = JSON.parse(xhr.responseText);
                errorMessage += errorResponse.message || error;
            } catch (e) {
                errorMessage += error;
            }
            
            errorMessage += '\n\nRevisa la consola del navegador para más detalles.';
            
            AbrirMensaje("Error", errorMessage, function () {
                $("#msjModal").modal("hide");
            }, false, ["Aceptar"], "error!", null);
        }
    });
}

/**
 * Envía mensaje de WhatsApp CON enlaces de archivos
 * @param {string} numero - Número de teléfono con código de país
 * @param {string} mensaje - Mensaje con enlaces incluidos
 * @param {number} cantidadArchivos - Cantidad de archivos incluidos
 */
function enviarWhatsAppConEnlaces(numero, mensaje, cantidadArchivos) {
    AbrirWaiting("Abriendo WhatsApp Web...");

    console.log('=== Abriendo WhatsApp Web (con enlaces) ===');
    console.log('Para:', numero);
    console.log('Archivos:', cantidadArchivos);
    console.log('Mensaje:', mensaje);

    $.ajax({
        url: '/ControlComun/GestorImpresion/GenerateWhatsAppWebLink',
        type: 'POST',
        contentType: 'application/json',
        data: JSON.stringify({
            To: numero,
            Message: mensaje
        }),
        success: function (response) {
            CerrarWaiting();
            
            if (response.success && response.whatsappWebLink) {
                const newWindow = window.open(response.whatsappWebLink, '_blank');
                
                if (!newWindow || newWindow.closed || typeof newWindow.closed === 'undefined') {
                    AbrirMensaje("Advertencia", 
                        '⚠️ El navegador bloqueó la ventana emergente.\n\n' +
                        'Permite ventanas emergentes para este sitio o copia el enlace:\n' + 
                        response.whatsappWebLink,
                        function () {
                            $("#msjModal").modal("hide");
                        }, false, ["Aceptar"], "warn!", null);
                } else {
                    setTimeout(() => {
                        AbrirMensaje("Éxito", 
                            `✅ WhatsApp Web abierto exitosamente\n\n` +
                            `📱 Destinatario: ${response.to}\n` +
                            `🔗 Enlaces incluidos: ${cantidadArchivos}\n\n` +
                            `Los archivos están disponibles en los enlaces del mensaje.`,
                            function () {
                                $("#msjModal").modal("hide");
                                $('#whatsappForm')[0].reset();
                                $('#whatsappCharCounter').text('0/5000 caracteres');
                            }, false, ["Aceptar"], "success", null);
                    }, 500);
                }
            } else {
                AbrirMensaje("Error", response.message, function () {
                    $("#msjModal").modal("hide");
                }, false, ["Aceptar"], "error!", null);
            }
        },
        error: function (xhr, status, error) {
            CerrarWaiting();
            console.error('Error:', error);
            
            let errorMessage = '❌ Error al abrir WhatsApp Web:\n\n';
            
            try {
                const errorResponse = JSON.parse(xhr.responseText);
                errorMessage += errorResponse.message || error;
            } catch (e) {
                errorMessage += error;
            }
            
            errorMessage += '\n\nRevisa la consola del navegador para más detalles.';
            
            AbrirMensaje("Error", errorMessage, function () {
                $("#msjModal").modal("hide");
            }, false, ["Aceptar"], "error!", null);
        }
    });
}

 /**
 * Actualiza la información de archivos para WhatsApp
 * ✅ FUNCIONALIDAD ORIGINAL RESTAURADA
 */
function updateWhatsAppFilesInfo() {
    const selectedNodes = $('#archivosDispuestos').jstree('get_selected', true);
    const archivosSeleccionados = selectedNodes.filter(function (node) {
        return node.parent !== "#" && node.parent !== null && !node.children;
    });
    
    if (archivosSeleccionados.length > 0) {
        let info = `⚠️ ${archivosSeleccionados.length} archivo(s) seleccionado(s).\n`;
        info += 'Recuerda que deberás adjuntarlos manualmente en WhatsApp.';
        $('#whatsappFilesInfo').text(info).show();
    } else {
        $('#whatsappFilesInfo').hide();
    }
}

function inicializaArbolArchivos() {
    //borramos el contenido del arbol.
    $("#archivosDispuestos").jstree("destroy").empty();
}

function invocacionGestorDoc(data) {
    PostGenHtml(data, OrquestadorDeModulosUrl, function (obj) {
        $("#modalGestorDocumental").html(obj);
        
        // ✅ NUEVO: Inicializar configuración antes de presentar archivos
        // Esperar un momento para que el script de configuración del modal se ejecute
        setTimeout(function() {
            inicializarConfiguracionModulo();
            
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
        }, 100); // ✅ Delay de 100ms para permitir que se ejecute el script del modal
    });
}

/**
 * Inicializa window.currentModuleConfig con valores por defecto seguros
 * Se ejecuta cuando se abre el modal, antes de presentar archivos
 */
function inicializarConfiguracionModulo() {
    if (!window.currentModuleConfig) {
        console.warn('⚠️ Inicializando configuración de módulo por defecto (no hay configuración del servidor)');
        
        window.currentModuleConfig = {
            moduloId: 'DEFAULT',
            moduloTitulo: 'Documentación',
            mensajeriaTemplate: null,
            emailTemplate: null,
            whatsappTemplate: null,
            empresa: {
                nombre: 'GeCoNet',
                telefono: '',
                email: '',
                direccion: '',  
                localidad: '',
                provincia: ''
            }
        };
    } else {
        console.log('✅ Configuración de módulo ya existe:', window.currentModuleConfig.moduloId);
    }
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

            // ✅ DIAGNÓSTICO EXHAUSTIVO DE CONFIGURACIÓN
            console.log('═══════════════════════════════════════════════════════');
            console.log('🔍 DIAGNÓSTICO EXHAUSTIVO DE CONFIGURACIÓN');
            console.log('═══════════════════════════════════════════════════════');

            if (!window.currentModuleConfig) {
                console.error('❌ CRÍTICO: window.currentModuleConfig NO EXISTE');
                console.error('  → Causa: Script del modal no se ejecutó');
                console.error('  → Solución: Inicializar con fallback');
                inicializarConfiguracionModulo();
            } else {
                console.log('✅ window.currentModuleConfig EXISTE');
                console.log('  📋 Módulo ID:', window.currentModuleConfig.moduloId);
                console.log('  📋 Módulo Título:', window.currentModuleConfig.moduloTitulo);

                // Verificar mensajeriaTemplate
                if (!window.currentModuleConfig.mensajeriaTemplate) {
                    console.error('❌ mensajeriaTemplate es NULL');
                    console.error('  → Causa: El módulo no tiene configuración de mensajería en appsettings.json');
                } else {
                    console.log('✅ mensajeriaTemplate EXISTE');

                    // Verificar mensajeTemplate (el contenido del mensaje)
                    if (!window.currentModuleConfig.mensajeriaTemplate.mensajeTemplate) {
                        console.error('❌ mensajeTemplate es NULL/UNDEFINED');
                        console.error('  → Objeto completo:', window.currentModuleConfig.mensajeriaTemplate);
                    } else {
                        console.log('✅ mensajeTemplate EXISTE (primeros 80 chars):');
                        console.log('  "' + window.currentModuleConfig.mensajeriaTemplate.mensajeTemplate.substring(0, 80) + '..."');
                        console.log('  📊 Longitud:', window.currentModuleConfig.mensajeriaTemplate.mensajeTemplate.length, 'caracteres');
                        console.log('  📊 Es Personalizado:', window.currentModuleConfig.mensajeriaTemplate.esPersonalizado);
                        console.log('  📊 Tipo Destinatario:', window.currentModuleConfig.mensajeriaTemplate.tipoDestinatario);
                    }
                }

                // Verificar emailTemplate
                if (!window.currentModuleConfig.emailTemplate) {
                    console.warn('⚠️ emailTemplate es NULL');
                } else {
                    console.log('✅ emailTemplate EXISTE');
                    console.log('  📧 Asunto:', window.currentModuleConfig.emailTemplate.asuntoTemplate);
                }

                // Verificar empresa
                console.log('✅ Empresa:', window.currentModuleConfig.empresa);
            }

            console.log('═══════════════════════════════════════════════════════');

            // Datos de cuenta
            console.log('📋 Objeto cuenta recibido:', cuenta);

            window.datosCtaActual = {
                id: cuenta.cta_Id || '',
                nombre: cuenta.cta_Denominacion || '',
                email: cuenta.cta_Email || '',
                celular: cuenta.cta_Celu || '',
                tipo: cuenta.cta_Tipo || 'E'
            };

            console.log('📋 Datos de cuenta guardados:', window.datosCtaActual);

            // Cargar campos
            $("#emailTo").val(cuenta.cta_Email || '');
            $("#whatsappTo").val(cuenta.cta_Celu || '');

            setTimeout(function () {
                const emailAsignado = $("#emailTo").val();
                const whatsappAsignado = $("#whatsappTo").val();

                if (!emailAsignado || emailAsignado.trim() === '') {
                    $("#emailTo").attr('placeholder', '⚠️ Email no disponible - Ingresar manualmente');
                    $("#emailTo").addClass('border-warning');
                }

                if (!whatsappAsignado || whatsappAsignado.trim() === '') {
                    $("#whatsappTo").attr('placeholder', '⚠️ Celular no disponible - Ingresar manualmente');
                    $("#whatsappTo").addClass('border-warning');
                }
            }, 100);

            // ✅ Pre-cargar mensajes
            console.log('📝 Pre-cargando mensajes de Email y WhatsApp...');

            // Pre-cargar EMAIL
            if (window.currentModuleConfig &&
                window.currentModuleConfig.emailTemplate &&
                window.currentModuleConfig.emailTemplate.asuntoTemplate) {

                const emailResult = generarContenidoEmailDesdeConfig(
                    window.datosCtaActual,
                    [],
                    []
                );

                $("#emailSubject").val(emailResult.asunto);
                $("#emailBody").val(emailResult.cuerpo);

                console.log('📧 Email pre-cargado:');
                console.log('  Asunto:', emailResult.asunto);
                console.log('  Cuerpo (primeros 100 chars):', emailResult.cuerpo.substring(0, 100) + '...');
            } else {
                console.warn('⚠️ No hay configuración de email - usando fallback');
                $("#emailSubject").val(`Documentación${window.datosCtaActual.id ? ' - ' + window.datosCtaActual.id : ''}`);
                $("#emailBody").val(`Estimado/a,\n\nAdjuntamos la documentación solicitada.\n\nSaludos cordiales.`);
            }

            // Pre-cargar WHATSAPP
            const mensajeWhatsApp = generarContenidoWhatsAppDesdeConfig(window.datosCtaActual);
            $("#whatsappMessage").val(mensajeWhatsApp);

            const length = mensajeWhatsApp.length;
            $('#whatsappCharCounter').text(`${length}/5000 caracteres`);

            console.log('📱 WhatsApp pre-cargado (primeros 100 chars):', mensajeWhatsApp.substring(0, 100) + '...');

            jsonP = JSON.parse(obj.arbol);
            procesarNodosArbol(jsonP);
            $("#archivosDispuestos").jstree("destroy").empty();

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

/**
 * ============================================
 * FUNCIONES AUXILIARES PARA ENVÍO DE EMAIL
 * ============================================
 */

/**
 * Wrapper de PostGen que retorna una Promise
 * @param {Object} data - Datos a enviar
 * @param {string} url - URL del endpoint
 * @returns {Promise}
 */
function PostGenPromise(data, url) {
    return new Promise((resolve, reject) => {
        $.ajax({
            dataType: "json",
            url: url,
            type: "POST",
            data: data,
            xhrFields: {
                withCredentials: true
            },
            success: resolve,
            error: function(xhr, status, error) {
                reject({
                    status: xhr.status,
                    statusText: xhr.statusText,
                    message: error || "Error en la solicitud",
                    responseText: xhr.responseText
                });
            }
        });
    });
}

///**
// * Genera un PDF en tiempo real desde el servidor
// * @param {Object} node - Nodo del árbol jsTree seleccionado
// * @returns {Promise<Object>} - Promise con { base64, nombre, tamañoBytes }
// */
//function generarPDFEnTiempoReal(node) {
//    return new Promise((resolve, reject) => {
//        const id = node.id;
        
//        // Validar que existan parámetros guardados
//        if (!arrRepoParams[id - 1]) {
//            reject(new Error(`No hay parámetros guardados para "${node.text}". Debe ejecutar el reporte primero."`));
//            return;
//        }
        
//        const data = arrRepoParams[id - 1];
        
//        const solicitudReporte = {
//            Reporte: data.reporte,
//            Parametros: data.parametros,
//            Ids: data.parametros.Ids,
//            Titulo: node.text,
//            SubTitulo: data.subTitulo,
//            Observacion: data.observacion || "",
//            Formato: "P", // PDF
//            LogoPath: "",
//            Administracion: data.administracion || administracion
//        };
        
//        console.log(`🔄 Generando PDF para: ${node.text}`);
        
//        // Llamada AJAX para generar el PDF
//        PostGenPromise(solicitudReporte, repoApiUrl)
//            .then(obj => {
//                if (obj.error === true) {
//                    reject(new Error(obj.resultado_msg || "Error al generar PDF"));
//                } else if (obj.warn === true) {
//                    reject(new Error(obj.msg || "Advertencia al generar PDF"));
//                } else {
//                    // Calcular tamaño del PDF en bytes
//                    const base64 = obj.base64;
//                    const tamañoBytes = (base64.length * 3) / 4 - (base64.indexOf('=') > 0 ? (base64.length - base64.indexOf('=')) : 0);

//                    // ✅ MODIFICADO: Reemplazar espacios por underscore en el nombre
//                    const nombreSanitizado = node.text.replace(/\s+/g, '_');

//                    console.log(`✅ PDF generado: ${node.text} (${(tamañoBytes / 1024 / 1024).toFixed(2)} MB)`);
                    
//                    resolve({
//                        base64: base64,
//                        nombre: nombreSanitizado + ".pdf",  // ✅ Usar nombre sanitizado
//                        tamañoBytes: tamañoBytes
//                    });
//                }
//            })
//            .catch(error => {
//                reject(error);
//            });
//    });
//}

///**
// * Guarda archivos grandes en el servidor y retorna enlaces
// * @param {Array} archivos - Array de objetos { base64, nombre, tamañoBytes }
// * @returns {Promise<Array>} - Array de { nombre, url }
// */
//function guardarArchivosGrandesEnServidor(archivos) {
//    return new Promise((resolve, reject) => {
//        console.log(`💾 Guardando ${archivos.length} archivo(s) grande(s) en el servidor...`);
        
//        const data = {
//            archivos: archivos.map(a => ({
//                archivoBase64: a.base64,
//                nombre: a.nombre
//            }))
//        };
        
//        $.ajax({
//            url: '/ControlComun/GestorImpresion/GuardarArchivosGrandes',
//            type: 'POST',
//            contentType: 'application/json',
//            data: JSON.stringify(data),
//            success: function (response) {
//                if (response.error) {
//                    reject(new Error(response.msg || "Error al guardar archivos"));
//                } else {
//                    console.log(`✅ Archivos guardados exitosamente`);
//                    resolve(response.enlaces || []);
//                }
//            },
//            error: function (xhr, status, error) {
//                reject(new Error(`Error de red al guardar archivos: ${error}`));
//            }
//        });
//    });
//}

/**
 * Construye el cuerpo del email/WhatsApp agregando información de archivos
 * @param {string} cuerpoBase - Cuerpo base del mensaje
 * @param {Array} archivosAdjuntos - Archivos pequeños adjuntos
 * @param {Array} enlacesArchivos - Enlaces de archivos grandes
 * @returns {string} - Cuerpo completo
 */
function construirCuerpoEmail(cuerpoBase, archivosAdjuntos, enlacesArchivos) {
    let cuerpo = cuerpoBase;
    
    // Agregar información de archivos adjuntos pequeños
    if (archivosAdjuntos && archivosAdjuntos.length > 0) {
        cuerpo += '\n\n📎 Archivos adjuntos:\n';
        archivosAdjuntos.forEach((archivo, index) => {
            const tamañoMB = (archivo.tamañoBytes / 1024 / 1024).toFixed(2);
            cuerpo += `${index + 1}. ${archivo.nombre} (${tamañoMB} MB)\n`;
        });
    }
    
    // Agregar enlaces de archivos grandes
    if (enlacesArchivos && enlacesArchivos.length > 0) {
        cuerpo += '\n\n🔗 Archivos disponibles para descarga:\n';
        enlacesArchivos.forEach((enlace, index) => {
            cuerpo += `${index + 1}. ${enlace.nombre}\n   ${enlace.url}\n\n`;
        });
    }
    
    return cuerpo;
}

/**
 * Genera el contenido personalizado UNIFICADO para Email y WhatsApp
 * ✅ MEJORADO: Validación robusta de configuración
 */
function generarContenidoMensaje(cuentaInfo, archivosAdjuntos, enlacesArchivos, canal) {
    // ✅ VALIDACIÓN MEJORADA: Verificar paso a paso
    console.log(`🔍 [${canal.toUpperCase()}] Generando contenido de mensaje...`);

    // PASO 1: Verificar window.currentModuleConfig
    if (!window.currentModuleConfig) {
        console.error(`❌ [${canal.toUpperCase()}] window.currentModuleConfig NO existe`);
        return generarMensajeFallback(cuentaInfo, archivosAdjuntos, enlacesArchivos, canal);
    }

    console.log(`✅ [${canal.toUpperCase()}] window.currentModuleConfig existe:`, window.currentModuleConfig.moduloId);

    // PASO 2: Verificar mensajeriaTemplate
    if (!window.currentModuleConfig.mensajeriaTemplate) {
        console.warn(`⚠️ [${canal.toUpperCase()}] mensajeriaTemplate es NULL para módulo: ${window.currentModuleConfig.moduloId}`);
        return generarMensajeFallback(cuentaInfo, archivosAdjuntos, enlacesArchivos, canal);
    }

    console.log(`✅ [${canal.toUpperCase()}] mensajeriaTemplate existe`);

    // PASO 3: Verificar mensajeTemplate
    if (!window.currentModuleConfig.mensajeriaTemplate.mensajeTemplate) {
        console.error(`❌ [${canal.toUpperCase()}] mensajeTemplate es UNDEFINED/NULL`);
        console.error('  → Objeto mensajeriaTemplate:', window.currentModuleConfig.mensajeriaTemplate);
        return generarMensajeFallback(cuentaInfo, archivosAdjuntos, enlacesArchivos, canal);
    }

    console.log(`✅ [${canal.toUpperCase()}] mensajeTemplate existe (primeros 50 chars):`,
        window.currentModuleConfig.mensajeriaTemplate.mensajeTemplate.substring(0, 50) + '...');

    // ✅ AHORA SÍ: Generar mensaje desde la plantilla
    const mensajeria = window.currentModuleConfig.mensajeriaTemplate;
    const empresa = window.currentModuleConfig.empresa || {};

    const esPersonalizado = mensajeria.esPersonalizado === true;
    const saludoGenerico = mensajeria.saludoGenerico || 'Estimado/a';

    let nombreLimpio = (cuentaInfo.nombre || '').split('(')[0].trim();
    let tieneNombre = nombreLimpio.length > 0;

    const fechaActual = new Date().toLocaleDateString('es-ES');

    let tipoDestinatario = '';
    if (cuentaInfo.tipo === 'P') {
        tipoDestinatario = 'Proveedor';
    } else if (cuentaInfo.tipo === 'E') {
        tipoDestinatario = 'Cliente';
    } else {
        tipoDestinatario = mensajeria.tipoDestinatario || 'Cliente/Proveedor';
    }

    let asuntoBase = '';
    let cuerpoBase = '';

    // GENERAR ASUNTO (solo para Email)
    if (canal === 'email' && window.currentModuleConfig.emailTemplate) {
        asuntoBase = window.currentModuleConfig.emailTemplate.asuntoTemplate
            .replace(/\{cuenta\}/g, cuentaInfo.id || '')
            .replace(/\{fecha\}/g, fechaActual)
            .replace(/\{nombre\}/g, tieneNombre ? nombreLimpio : '');

        console.log(`📧 Asunto generado: "${asuntoBase}"`);
    }

    // GENERAR MENSAJE (común para Email y WhatsApp)
    if (esPersonalizado && tieneNombre) {
        // CASO 1: Mensaje personalizado CON nombre
        console.log(`📝 [${canal.toUpperCase()}] Generando mensaje personalizado con nombre`);

        cuerpoBase = mensajeria.mensajeTemplate
            .replace(/\{tipoDestinatario\}/g, tipoDestinatario)
            .replace(/\{nombre\}/g, nombreLimpio)
            .replace(/\{cuenta\}/g, cuentaInfo.id || '')
            .replace(/\{fecha\}/g, fechaActual);

    } else if (esPersonalizado && !tieneNombre) {
        // CASO 2: Mensaje personalizado SIN nombre (usar saludo genérico)
        console.log(`📝 [${canal.toUpperCase()}] Generando mensaje personalizado sin nombre`);

        let mensajeGenerico = mensajeria.mensajeTemplate;
        const patronSaludo = /Estimado\/a\s+\{tipoDestinatario\}\s+\{nombre\},?/gi;
        mensajeGenerico = mensajeGenerico.replace(patronSaludo, saludoGenerico + ',');

        cuerpoBase = mensajeGenerico
            .replace(/\{tipoDestinatario\}/g, '')
            .replace(/\{nombre\}/g, '')
            .replace(/\{cuenta\}/g, cuentaInfo.id || '')
            .replace(/\{fecha\}/g, fechaActual)
            .replace(/\s{2,}/g, ' ')
            .trim();

    } else {
        // ✅ CASO 3: Mensaje NO personalizado (genérico del módulo)
        console.log(`📝 [${canal.toUpperCase()}] Generando mensaje NO personalizado (genérico del módulo)`);

        // ✅ CORREGIDO: Simplemente usar la plantilla directamente
        cuerpoBase = mensajeria.mensajeTemplate
            .replace(/\{fecha\}/g, fechaActual)
            .replace(/\{cuenta\}/g, cuentaInfo.id || '');
    }

    // Agregar prefijo de WhatsApp si existe
    if (canal === 'whatsapp' && window.currentModuleConfig.whatsappTemplate) {
        const whatsapp = window.currentModuleConfig.whatsappTemplate;
        if (whatsapp.prefijoMensaje) {
            cuerpoBase = whatsapp.prefijoMensaje + '\n\n' + cuerpoBase;
        }
    }

    // Agregar pie con datos de la empresa
    cuerpoBase += `\n\nAtentamente,\n`;
    cuerpoBase += `${empresa.nombre || 'GeCoNet'}\n`;
    if (empresa.telefono) cuerpoBase += `Tel: ${empresa.telefono}\n`;
    if (empresa.email) cuerpoBase += `Email: ${empresa.email}\n`;

    // Agregar información de archivos
    cuerpoBase = construirCuerpoEmail(cuerpoBase, archivosAdjuntos, enlacesArchivos);

    console.log(`✅ [${canal.toUpperCase()}] Mensaje generado exitosamente`);

    return { asunto: asuntoBase, cuerpo: cuerpoBase };
}

/**
 * ✅ NUEVA FUNCIÓN: Genera mensaje fallback cuando NO hay configuración
 * @param {Object} cuentaInfo - Información de la cuenta
 * @param {Array} archivosAdjuntos - Archivos adjuntos
 * @param {Array} enlacesArchivos - Enlaces de archivos
 * @param {string} canal - 'email' o 'whatsapp'
 * @returns {Object} { asunto, cuerpo }
 */
function generarMensajeFallback(cuentaInfo, archivosAdjuntos, enlacesArchivos, canal) {
    console.warn(`⚠️ [${canal.toUpperCase()}] Usando mensaje FALLBACK genérico`);

    let asuntoBase = '';
    let cuerpoBase = '';

    if (canal === 'email') {
        asuntoBase = `Documentación${cuentaInfo.id ? ' - ' + cuentaInfo.id : ''}`;
    }

    const nombreLimpio = (cuentaInfo.nombre || '').split('(')[0].trim();

    if (nombreLimpio) {
        cuerpoBase = `Estimado/a ${nombreLimpio},\n\n`;
    } else {
        cuerpoBase = `Estimado/a,\n\n`;
    }

    cuerpoBase += `Adjuntamos la documentación solicitada.\n\n`;
    cuerpoBase += `Saludos cordiales.`;

    // Agregar información de archivos
    cuerpoBase = construirCuerpoEmail(cuerpoBase, archivosAdjuntos, enlacesArchivos);

    return { asunto: asuntoBase, cuerpo: cuerpoBase };
}

// ✅ FUNCIÓN PARA EMAIL (usa la función unificada)
function generarContenidoEmailDesdeConfig(cuentaInfo, archivosAdjuntos, enlacesArchivos) {
    return generarContenidoMensaje(cuentaInfo, archivosAdjuntos, enlacesArchivos, 'email');
}

// ✅ FUNCIÓN PARA WHATSAPP (usa la misma función unificada)
function generarContenidoWhatsAppDesdeConfig(cuentaInfo) {
    const resultado = generarContenidoMensaje(cuentaInfo, [], [], 'whatsapp');
    return resultado.cuerpo; // WhatsApp solo usa el cuerpo, no el asunto
}

/**
 * ✅ NUEVA FUNCIÓN: Genera URLs de documentos sin pre-generarlos
 * @param {Array} archivosSeleccionados - Nodos del árbol jsTree
 * @returns {Promise<Array>} - Array de { Nombre, Url }
 */
function generarURLsDocumentos(archivosSeleccionados) {
    return new Promise((resolve, reject) => {
        console.log(`🔗 Generando URLs para ${archivosSeleccionados.length} documento(s)`);

        // ✅ PASO 1: Construir solicitudes ReporteSolicitudDto
        const solicitudes = archivosSeleccionados.map(node => {
            const id = node.id;

            if (!arrRepoParams[id - 1]) {
                throw new Error(`No hay parámetros guardados para "${node.text}"`);
            }

            const data = arrRepoParams[id - 1];

            // ✅ Construir ReporteSolicitudDto
            return {
                Reporte: data.reporte,
                Parametros: data.parametros,
                Ids: data.parametros.Ids || [],
                Titulo: node.text,
                SubTitulo: data.subTitulo || "",
                Observacion: data.observacion || "",
                LogoPath: data.logoPath || "",
                Administracion: data.administracion || administracion,
                Formato: "P" // PDF
            };
        });

        console.log('📦 Solicitudes construidas:', solicitudes);

        // ✅ PASO 2: Enviar al backend para generar URLs
        $.ajax({
            url: '/ControlComun/GestorImpresion/GenerarURLsDocumentos',
            type: 'POST',
            contentType: 'application/json',
            data: JSON.stringify(solicitudes),
            success: function (response) {
                if (response.error) {
                    reject(new Error(response.msg || "Error al generar URLs"));
                } else {
                    console.log(`✅ ${response.enlaces.length} URL(s) generada(s)`);
                    resolve(response.enlaces || []);
                }
            },
            error: function (xhr, status, error) {
                reject(new Error(`Error de red: ${error}`));
            }
        });
    });
}