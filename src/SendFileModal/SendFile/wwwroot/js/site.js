// Please see documentation at https://learn.microsoft.com/aspnet/core/client-side/bundling-and-minification
// for details on configuring this project to bundle and minify static web assets.

// Write your JavaScript code.

$(function () {
    // Configuración de proveedores
    const providerConfig = {
        gmail: {
            name: 'Gmail (SMTP)',
            info: 'smtp.gmail.com:587 - TLS'
        },
        outlookweb: {
            name: 'Outlook Web',
            info: 'outlook.office.com - Deeplink'
        },
        outlookdesktop: {
            name: 'Outlook Desktop',
            info: 'Cliente local - mailto: protocol'
        }
    };

    // Actualizar información cuando cambia el proveedor
    $('input[name="emailProvider"]').on('change', function () {
        const selectedProvider = $(this).val();
        const config = providerConfig[selectedProvider];
        
        if (config) {
            $('#providerInfo').text(`Usando ${config.name} (${config.info})`);
        }

        // Ocultar todas las advertencias primero
        $('#outlookWebWarning, #outlookDesktopWarning').hide();
        $('#ccField, #bccField').hide();
        $('#emailCc, #emailBcc').val('');
        $('#emailFile').prop('disabled', false).removeClass('bg-light');
        $('#fileHelp').text('Tamaño máximo: 25 MB');

        // Mostrar/ocultar advertencias y controles según proveedor
        if (selectedProvider === 'outlookweb') {
            $('#outlookWebWarning').slideDown();
            $('#emailFile').prop('disabled', true).addClass('bg-light');
            $('#fileHelp').html('<em class="text-muted">Los adjuntos no se envían con Outlook Web (deben agregarse manualmente)</em>');
            $('#ccField, #bccField').slideDown();
        } 
        else if (selectedProvider === 'outlookdesktop') {
            $('#outlookDesktopWarning').slideDown();
            $('#emailFile').prop('disabled', true).addClass('bg-light');
            $('#fileHelp').html('<em class="text-muted">Los adjuntos NO se envían con mailto: (deben agregarse manualmente en Outlook)</em>');
            $('#ccField, #bccField').slideDown();
        }
    });

    // Abrir modal al hacer clic en el botón
    $('#openModalBtn').on('click', function () {
        $('#myModal').modal('show');
    });

    // Manejar el botón de guardar/enviar del modal
    $('#saveModalBtn').on('click', function () {
        // Detectar qué tab está activo
        const activeTab = $('.tab-pane.active').attr('id');
        
        if (activeTab === 'email-panel') {
            sendEmail();
        } else if (activeTab === 'whatsapp-panel') {
            sendWhatsApp();
        }
    });

    // Función para enviar email
    function sendEmail() {
        const selectedProvider = $('input[name="emailProvider"]:checked').val();
        
        // Validaciones básicas
        const emailTo = $('#emailTo').val();
        const emailSubject = $('#emailSubject').val();
        
        if (!emailTo || !emailSubject) {
            alert('Por favor completa los campos obligatorios (Para y Asunto)');
            return;
        }

        // ======= OUTLOOK DESKTOP: Abrir cliente local =======
        if (selectedProvider === 'outlookdesktop') {
            openOutlookDesktop(
                emailTo, 
                emailSubject, 
                $('#emailMessage').val() || '',
                $('#emailCc').val() || '',
                $('#emailBcc').val() || ''
            );
            return;
        }

        // ======= OUTLOOK WEB: Abrir cliente web =======
        if (selectedProvider === 'outlookweb') {
            openOutlookWeb(
                emailTo, 
                emailSubject, 
                $('#emailMessage').val() || '',
                $('#emailCc').val() || '',
                $('#emailBcc').val() || ''
            );
            return;
        }

        // ======= GMAIL SMTP: Envío tradicional =======
        const formData = new FormData();
        formData.append('Provider', 'gmail');
        formData.append('To', emailTo);
        formData.append('Subject', emailSubject);
        formData.append('Message', $('#emailMessage').val() || '');
        
        // Adjuntar archivo si existe
        const fileInput = $('#emailFile')[0];
        if (fileInput.files.length > 0) {
            formData.append('emailFile', fileInput.files[0]);
        }
        
        // Deshabilitar botón y mostrar feedback
        const $sendBtn = $('#saveModalBtn');
        const originalText = $sendBtn.text();
        $sendBtn.prop('disabled', true).html('<span class="spinner-border spinner-border-sm me-2"></span>Enviando...');
        
        console.log('=== Enviando Email vía Gmail ===');
        console.log('Para:', emailTo);
        console.log('Asunto:', emailSubject);
        
        // Enviar con AJAX
        $.ajax({
            url: '/Home/SendEmail',
            type: 'POST',
            data: formData,
            processData: false,
            contentType: false,
            success: function (response) {
                console.log('Respuesta del servidor:', response);
                
                if (response.success) {
                    alert(`✅ ${response.message}\n\nDestinatario: ${response.to}\nProveedor: ${response.provider}`);
                    
                    // Limpiar formulario
                    $('#emailForm')[0].reset();
                    
                    // Cerrar modal
                    $('#myModal').modal('hide');
                } else {
                    alert(`❌ Error: ${response.message}`);
                }
            },
            error: function (xhr, status, error) {
                console.error('Error en la solicitud:', error);
                console.error('Estado:', status);
                console.error('Respuesta:', xhr.responseText);
                
                alert(`❌ Error al enviar el email:\n${error}\n\nRevisa la consola para más detalles.`);
            },
            complete: function () {
                // Restaurar botón
                $sendBtn.prop('disabled', false).text(originalText);
            }
        });
    }

    // Función: Abrir Outlook Desktop (mailto:)
    function openOutlookDesktop(to, subject, body, cc, bcc) {
        const $sendBtn = $('#saveModalBtn');
        const originalText = $sendBtn.text();
        $sendBtn.prop('disabled', true).html('<span class="spinner-border spinner-border-sm me-2"></span>Abriendo...');

        console.log('=== Abriendo Outlook Desktop (mailto:) ===');
        console.log('Para:', to);
        console.log('CC:', cc || '(ninguno)');
        console.log('BCC:', bcc || '(ninguno)');
        console.log('Asunto:', subject);

        $.ajax({
            url: '/Home/GenerateMailtoLink',
            type: 'POST',
            contentType: 'application/json',
            data: JSON.stringify({
                Provider: 'outlookdesktop',
                To: to,
                Cc: cc,
                Bcc: bcc,
                Subject: subject,
                Message: body
            }),
            success: function (response) {
                console.log('Respuesta:', response);
                
                if (response.success && response.mailtoLink) {
                    // Redirigir al enlace mailto: (abre Outlook local)
                    window.location.href = response.mailtoLink;
                    
                    // Mostrar mensaje después de un breve delay
                    setTimeout(() => {
                        alert(`✅ ${response.message}\n\n${response.note}\n\n📝 Cuentas disponibles:\n• juanjobe@msn.com\n• tu@cafeamerica.com.ar`);
                        $('#emailForm')[0].reset();
                        $('#myModal').modal('hide');
                    }, 1000);
                } else {
                    alert(`❌ Error: ${response.message}`);
                }
            },
            error: function (xhr, status, error) {
                console.error('Error:', error);
                alert(`❌ Error al generar enlace mailto:\n${error}`);
            },
            complete: function () {
                $sendBtn.prop('disabled', false).text(originalText);
            }
        });
    }

    // Función: Abrir Outlook Web con CC/BCC
    function openOutlookWeb(to, subject, body, cc, bcc) {
        const $sendBtn = $('#saveModalBtn');
        const originalText = $sendBtn.text();
        $sendBtn.prop('disabled', true).html('<span class="spinner-border spinner-border-sm me-2"></span>Abriendo...');

        console.log('=== Abriendo Outlook Web ===');
        console.log('Para:', to);
        console.log('CC:', cc || '(ninguno)');
        console.log('BCC:', bcc || '(ninguno)');
        console.log('Asunto:', subject);

        $.ajax({
            url: '/Home/GenerateOutlookWebLink',
            type: 'POST',
            contentType: 'application/json',
            data: JSON.stringify({
                Provider: 'outlookweb',
                To: to,
                Cc: cc,
                Bcc: bcc,
                Subject: subject,
                Message: body
            }),
            success: function (response) {
                console.log('Respuesta:', response);
                
                if (response.success && response.outlookWebLink) {
                    // Abrir en nueva pestaña
                    const newWindow = window.open(response.outlookWebLink, '_blank');
                    
                    if (!newWindow || newWindow.closed || typeof newWindow.closed === 'undefined') {
                        alert('⚠️ El navegador bloqueó la ventana emergente.\n\nPermite ventanas emergentes para este sitio o copia el enlace:\n' + response.outlookWebLink);
                    } else {
                        // Mostrar mensaje de éxito
                        setTimeout(() => {
                            alert(`✅ ${response.message}\n\n${response.note}`);
                            $('#emailForm')[0].reset();
                            $('#myModal').modal('hide');
                        }, 500);
                    }
                } else {
                    alert(`❌ Error: ${response.message}`);
                }
            },
            error: function (xhr, status, error) {
                console.error('Error:', error);
                alert(`❌ Error al generar enlace de Outlook Web:\n${error}`);
            },
            complete: function () {
                $sendBtn.prop('disabled', false).text(originalText);
            }
        });
    }

    // ======= FUNCIÓN COMPLETA: Enviar WhatsApp Web (GRATIS) =======
    function sendWhatsApp() {
        // Validaciones básicas
        const whatsappPhone = $('#whatsappPhone').val().trim();
        const whatsappMessage = $('#whatsappMessage').val().trim();
        
        if (!whatsappPhone) {
            alert('❌ Por favor ingresa un número de teléfono');
            return;
        }

        if (!whatsappMessage) {
            alert('❌ Por favor escribe un mensaje');
            return;
        }

        // Validar formato de número
        const cleanNumber = whatsappPhone.replace(/[\s\-\(\)]/g, '');
        if (!cleanNumber.startsWith('+')) {
            alert('❌ El número debe incluir el código de país\n\nEjemplos:\n• Argentina: +5491123456789\n• México: +521234567890\n• España: +34612345678\n• Perú: +51999999999');
            return;
        }

        // Validar longitud del mensaje
        if (whatsappMessage.length > 5000) {
            alert('❌ El mensaje es muy largo. Recomendamos máximo 5000 caracteres\n\nCaracteres actuales: ' + whatsappMessage.length);
            return;
        }

        // Advertir sobre archivos
        const fileInput = $('#whatsappFiles')[0];
        if (fileInput.files.length > 0) {
            const confirmSend = confirm('⚠️ WhatsApp Web no permite adjuntar archivos automáticamente.\n\nEl mensaje se enviará sin adjuntos. Deberás agregarlos manualmente en WhatsApp.\n\n¿Continuar?');
            if (!confirmSend) {
                return;
            }
        }

        // Deshabilitar botón y mostrar feedback
        const $sendBtn = $('#saveModalBtn');
        const originalText = $sendBtn.text();
        $sendBtn.prop('disabled', true).html('<span class="spinner-border spinner-border-sm me-2"></span>Abriendo WhatsApp...');
        
        console.log('=== Abriendo WhatsApp Web ===');
        console.log('Para:', cleanNumber);
        console.log('Mensaje:', whatsappMessage);
        
        // Llamar al backend para generar el enlace
        $.ajax({
            url: '/Home/GenerateWhatsAppWebLink',
            type: 'POST',
            contentType: 'application/json',
            data: JSON.stringify({
                To: cleanNumber,
                Message: whatsappMessage
            }),
            success: function (response) {
                console.log('Respuesta del servidor:', response);
                
                if (response.success && response.whatsappWebLink) {
                    // Abrir en nueva pestaña
                    const newWindow = window.open(response.whatsappWebLink, '_blank');
                    
                    if (!newWindow || newWindow.closed || typeof newWindow.closed === 'undefined') {
                        alert('⚠️ El navegador bloqueó la ventana emergente.\n\nPermite ventanas emergentes para este sitio o copia el enlace:\n' + response.whatsappWebLink);
                    } else {
                        // Mostrar mensaje de éxito
                        setTimeout(() => {
                            alert(`✅ ${response.message}\n\n📱 Destinatario: ${response.to}\n\n${response.note}`);
                            $('#whatsappForm')[0].reset();
                            $('#charCounter').remove();
                            $('#myModal').modal('hide');
                        }, 500);
                    }
                } else {
                    alert(`❌ Error: ${response.message}`);
                }
            },
            error: function (xhr, status, error) {
                console.error('Error en la solicitud:', error);
                console.error('Estado:', status);
                console.error('Respuesta:', xhr.responseText);
                
                let errorMessage = '❌ Error al abrir WhatsApp Web:\n\n';
                
                try {
                    const errorResponse = JSON.parse(xhr.responseText);
                    errorMessage += errorResponse.message || error;
                } catch (e) {
                    errorMessage += error;
                }
                
                errorMessage += '\n\nRevisa la consola para más detalles.';
                alert(errorMessage);
            },
            complete: function () {
                // Restaurar botón
                $sendBtn.prop('disabled', false).text(originalText);
            }
        });
    }
    // ======= FIN FUNCIÓN =======

    // Contador de caracteres para WhatsApp
    $('#whatsappMessage').on('input', function() {
        const length = $(this).val().length;
        const maxLength = 5000;
        const remaining = maxLength - length;
        
        let color = 'text-muted';
        if (remaining < 1000) color = 'text-warning';
        if (remaining < 200) color = 'text-danger';
        
        // Crear o actualizar contador
        if ($('#charCounter').length === 0) {
            $(this).after(`<small id="charCounter" class="${color}">${length}/${maxLength} caracteres</small>`);
        } else {
            $('#charCounter').text(`${length}/${maxLength} caracteres`).attr('class', color);
        }
    });

    // Evento cuando el modal se cierra - Limpiar formularios
    $('#myModal').on('hidden.bs.modal', function () {
        console.log('Modal cerrado - Limpiando formularios');
        
        // Limpiar formulario de email
        $('#emailForm')[0].reset();
        
        // Limpiar formulario de WhatsApp
        $('#whatsappForm')[0].reset();

        // Resetear proveedor a Gmail
        $('#gmailProvider').prop('checked', true).trigger('change');
    });

    // Evento cuando el modal se abre
    $('#myModal').on('shown.bs.modal', function () {
        console.log('Modal abierto');
    });
});
