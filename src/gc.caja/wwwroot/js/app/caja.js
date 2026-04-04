$(function () {
    // Referencias a instancias de Modal en BS5
    const modalValidacion = new bootstrap.Modal(document.getElementById('modalValidacionIngreso'));
    const modalMenu = new bootstrap.Modal(document.getElementById('modalMenuCaja'));

    // ---------------------------------------------------------
    // 1. ✅ LLAMADA REAL AL BACKEND PARA VALIDAR INTEGRIDAD
    // ---------------------------------------------------------
    validarIntegridadCaja();

    // ---------------------------------------------------------
    // 2. Controladores de Botones: Modal VISTA PREGUNTA
    // ---------------------------------------------------------

    /**
     * Botón: HACE APERTURA
     * Llama al SP SPGECO_CAJA_Apertura mediante AJAX
     */
    $("#btnHaceApertura").on("click", function () {
        realizarAperturaCaja();
    });

    /**
     * Botón: OPERA SIN CAJA
     * Permite continuar sin realizar apertura
     */
    $("#btnOperaSinCaja").on("click", function () {
        modalValidacion.hide();
        setTimeout(() => {
            modalMenu.show();
        }, 400);
    });

    /**
     * Botón: SALIR
     * Cierra sesión y redirige al login
     */
    $("#btnSale").on("click", function () {
        modalValidacion.hide();
        window.location.href = "/seguridad/token/logout";
    });

    // ---------------------------------------------------------
    // 3. Controladores de Botones: Modal MENÚ PRINCIPAL
    // ---------------------------------------------------------

    /**
     * Botón: CERRAR MENÚ
     * Cierra el menú y redirige al logout
     */
    $("#btnCerrarMenu").on("click", function () {
        modalMenu.hide();
        setTimeout(() => {
            window.location.href = "/seguridad/token/logout";
        }, 300);
    });

    // ---------------------------------------------------------
    // FUNCIONES DE VALIDACIÓN Y PROCESAMIENTO
    // ---------------------------------------------------------

    /**
     * Realiza la validación de integridad de caja mediante AJAX
     */
    function validarIntegridadCaja() {
        // Mostrar overlay de carga
        $('#loaderText').html("Validando Integridad de Sesión...<br><small class='text-muted'>Verificando configuración de caja</small>");
        $('#loaderOverlay').fadeIn(300);

        $.ajax({
            url: ValidacionIntegridadUrl,
            type: 'POST',
            dataType: 'json',
            timeout: 60000, // 60 segundos de timeout
            success: function (response) {
                procesarRespuestaValidacion(response);
            },
            error: function (xhr, status, error) {
                manejarErrorValidacion(xhr, status, error);
            }
        });
    }

    /**
     * Procesa la respuesta de la validación según el código de resultado
     * @param {object} response - Respuesta del servidor
     */
    function procesarRespuestaValidacion(response) {
        // Ocultar overlay de carga
        $('#loaderOverlay').fadeOut(400);

        // Hidratar datos en la vista
        $("#lblCajero").text(response.usuario || "Usuario");
        $("#lblCajaId").text("CAJA " + (response.caja_id || "N/A"));

        // Evaluar el código de resultado
        if (response.resultado < 0) {
            // ❌ ERROR CRÍTICO (resultado < 0)
            mostrarErrorCritico(response);
        } else if (response.resultado !== 0 && response.resultado !== 3) {
            // ℹ️ ADVERTENCIA/INFORMACIÓN 
            mostrarAdvertencia(response, true);
        } else if (response.resultado === 0 && response.respuesta_id === "") {
            // ⚠️ REQUIERE APERTURA (resultado = 0 y sin respuesta_id)
            mostrarModalApertura(response);
        } else if (response.resultado === 0) {
            // ✅ TODO OK - IR DIRECTO AL MENÚ
            mostrarMenuPrincipal(response);
        } else if (response.resultado > 0) {
            // ℹ️ ADVERTENCIA/INFORMACIÓN (resultado > 0)
            mostrarAdvertencia(response, true);
        }
    }

    /**
     * Maneja errores de la llamada AJAX
     */
    function manejarErrorValidacion(xhr, status, error) {
        $('#loaderOverlay').fadeOut(400);

        let mensajeError = "Error desconocido al validar integridad.";

        if (status === 'timeout') {
            mensajeError = "La validación tardó demasiado tiempo. Por favor, intente nuevamente.";
        } else if (xhr.status === 401) {
            mensajeError = "Su sesión ha expirado. Será redirigido al login.";
            setTimeout(() => {
                window.location.href = "/seguridad/token/login";
            }, 20000);
        } else if (xhr.status === 500) {
            mensajeError = "Error interno del servidor. Contacte al administrador.";
        } else if (xhr.responseJSON && xhr.responseJSON.mensaje) {
            mensajeError = xhr.responseJSON.mensaje;
        }

        AbrirMensaje(
            "ERROR DE VALIDACIÓN",
            mensajeError,
            function () {
                $("#msjModal").modal("hide");
                // Redirigir al login después de 20 segundos
                setTimeout(() => {
                    window.location.href = "/seguridad/token/login";
                }, 20000);
            },
            false,
            ["Aceptar"],
            "error!",
            null
        );
    }

    /**
     * Realiza la apertura de caja mediante AJAX
     */
    function realizarAperturaCaja() {
        let $btn = $("#btnHaceApertura");
        let originalText = $btn.html();

        // Deshabilitar botón y mostrar loading
        $btn.prop("disabled", true).html("<i class='bx bx-loader-alt bx-spin'></i> Abriendo Caja...");

        $.ajax({
            url: AperturaCajaUrl,
            type: 'POST',
            dataType: 'json',
            data: {
                caja_id: $("#lblCajaId").text().replace("CAJA ", ""),
                usuario: $("#lblCajero").text()
            },
            success: function (response) {
                $btn.prop("disabled", false).html(originalText);

                if (response.ok) {
                    // Apertura exitosa
                    modalValidacion.hide();
                    setTimeout(() => {
                        modalMenu.show();
                    }, 400);
                } else {
                    // Error en la apertura
                    AbrirMensaje(
                        "ERROR AL ABRIR CAJA",
                        response.mensaje || "No se pudo realizar la apertura de caja.",
                        function () {
                            $("#msjModal").modal("hide");
                        },
                        false,
                        ["Aceptar"],
                        "error!",
                        null
                    );
                }
            },
            error: function (xhr, status, error) {
                $btn.prop("disabled", false).html(originalText);

                AbrirMensaje(
                    "ERROR",
                    "Error al realizar apertura de caja. Intente nuevamente.",
                    function () {
                        $("#msjModal").modal("hide");
                    },
                    false,
                    ["Aceptar"],
                    "error!",
                    null
                );
            }
        });
    }

    /**
     * Muestra error crítico y redirige al login
     */
    function mostrarErrorCritico(response) {
        AbrirMensaje(
            "ERROR CRÍTICO",
            response.mensaje || "No se pudo validar la integridad de la caja.",
            function () {
                $("#msjModal").modal("hide");
                window.location.href = "/seguridad/token/login";
            },
            false,
            ["Salir"],
            "error!",
            null
        );
    }

    /**
     * Muestra el modal de apertura de caja
     */
    function mostrarModalApertura(response) {
        // Mostrar modal de validación (apertura de caja)
        setTimeout(() => {
            modalValidacion.show();
        }, 500);
    }

    /**
     * Muestra el menú principal directamente
     */
    function mostrarMenuPrincipal(response) {
        // Mostrar modal de menú principal
        setTimeout(() => {
            modalMenu.show();
        }, 500);
    }

    /**
     * Muestra mensaje de advertencia y continúa al menú
     */
    function mostrarAdvertencia(response, salir = false) {
        AbrirMensaje(
            "ATENCIÓN",
            response.mensaje || "Información sobre la validación de caja.",
            function () {
                $("#msjModal").modal("hide");
                if (salir) {
                    window.location.href = login
                }
            },
            false,
            ["Continuar"],
            "warn!",
            null
        );
    }
});
