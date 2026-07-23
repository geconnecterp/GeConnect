// ============================================
// GESTOR PRINCIPAL DEL FLUJO DE CAJA
// ============================================

$(function () {
    // Variables para referencias a modales (inicialización lazy)
    let modalValidacion = null;
    let modalMenu = null;
    let modalCambiaPV = null;

    // Variable global para control de acceso al menú
    let nivelAccesoMenu = 'ninguno';

    // Variable global para control de cierre intencional del modal Cambio PV
    let cierreIntencional = false;

    // ---------------------------------------------------------
    // FUNCIONES HELPER PARA GESTIÓN DE MODALES
    // ---------------------------------------------------------

    /**
     * Obtiene o inicializa la instancia del modal de validación
     */
    function getModalValidacion() {
        if (!modalValidacion) {
            const elemento = document.getElementById('modalValidacionIngreso');
            if (elemento) {
                modalValidacion = new bootstrap.Modal(elemento);
            } else {
                console.error('❌ Elemento modalValidacionIngreso no encontrado en el DOM');
            }
        }
        return modalValidacion;
    }

    /**
     * Obtiene o inicializa la instancia del modal de menú
     */
    function getModalMenu() {
        if (!modalMenu) {
            const elemento = document.getElementById('modalMenuCaja');
            if (elemento) {
                modalMenu = new bootstrap.Modal(elemento);
            } else {
                console.error('❌ Elemento modalMenuCaja no encontrado en el DOM');
            }
        }
        return modalMenu;
    }

    /**
     * Obtiene o inicializa la instancia del modal de cambio PV
     */
    function getModalCambiaPV() {
        if (!modalCambiaPV) {
            const elemento = document.getElementById('modalCambiaPV');
            if (elemento) {
                modalCambiaPV = new bootstrap.Modal(elemento);
            } else {
                console.error('❌ Elemento modalCambiaPV no encontrado en el DOM');
            }
        }
        return modalCambiaPV;
    }

    // ---------------------------------------------------------
    // INICIO DEL FLUJO: VALIDACIÓN DE INTEGRIDAD
    // ---------------------------------------------------------
    iniciarFlujoValidacion();

    // ---------------------------------------------------------
    // MANEJADORES DE EVENTOS: MODAL VALIDACIÓN
    // ---------------------------------------------------------

    /**
     * Botón: HACE APERTURA
     */
    $("#btnHaceApertura").on("click", function () {
        const modal = getModalValidacion();
        if (modal) modal.hide();

        // Pequeña pausa para que el modal se cierre antes de procesar
        setTimeout(() => {
            procesarAperturaCaja();
        }, 300);
    });

    /**
     * Botón: OPERA SIN CAJA (solo disponible cuando resultado = 3)
     */
    $("#btnOperaSinCaja").on("click", function () {
        const modal = getModalValidacion();
        if (modal) modal.hide();

        nivelAccesoMenu = 'parcial';
        setTimeout(() => {
            configurarMenuSegunAcceso();
            const menuModal = getModalMenu();
            if (menuModal) menuModal.show();
        }, 400);
    });

    /**
     * Botón: SALIR
     */
    $("#btnSale").on("click", function () {
        const modal = getModalValidacion();
        if (modal) modal.hide();
        window.location.href = logout;
    });

    // ---------------------------------------------------------
    // MANEJADORES DE EVENTOS: MODAL CAMBIO PV
    // ---------------------------------------------------------

    /**
     * Botón: CONFIRMAR CAMBIO PV
     */
    $("#btnConfirmaCambioPV").on("click", function () {
        cierreIntencional = true;
        
        // Cerrar modal ANTES de procesar
        const modal = getModalCambiaPV();
        if (modal) modal.hide();
        
        // Esperar que el modal se cierre completamente
        setTimeout(() => {
            procesarCambioPV();
        }, 300);
    });

    /**
     * Botón: CANCELAR CAMBIO PV
     */
    $("#btnCancelaCambiaPV").on("click", function () {
        const modal = getModalCambiaPV();
        if (modal) modal.hide();
    });

    /**
     * Event listener para prevenir cierre accidental del modal
     */
    $('#modalCambiaPV').on('hide.bs.modal', function (e) {
        if (!cierreIntencional) {
            e.preventDefault();

            AbrirMensaje(
                "Confirmar Salida",
                "¿Está seguro de que desea cancelar el cambio de punto de venta?<br><br>" +
                "<small class='text-muted'><i class='bx bx-info-circle'></i> Si cancela, será redirigido al inicio de sesión.</small>",
                function (respuesta) {
                    $("#msjModal").modal("hide");

                    if (respuesta === "SI") {
                        cierreIntencional = true;
                        setTimeout(() => {
                            $('#modalCambiaPV').modal('hide');
                            setTimeout(() => {
                                window.location.href = logout;
                            }, 300);
                        }, 300);
                    }
                },
                true,
                ["Sí, Salir", "No, Continuar"],
                "warn!",
                null
            );
        } else {
            cierreIntencional = false;
        }
    });

    /**
     * Preparar modal al mostrarse
     */
    $('#modalCambiaPV').on('shown.bs.modal', function () {
        cierreIntencional = false;
        $("#lblUsuarioPV").text($("#lblCajero").text() || "---");
    });

    // ---------------------------------------------------------
    // MANEJADORES DE EVENTOS: MODAL MENÚ PRINCIPAL
    // ---------------------------------------------------------

    /**
     * Botón: CERRAR MENÚ
     */
    $("#btnCerrarMenu").on("click", function () {
        const modal = getModalMenu();
        if (modal) modal.hide();
        setTimeout(() => {
            window.location.href = logout;
        }, 300);
    });

    /**
     * Manejadores de botones del menú
     */
    $('.menu-btn-enhanced').on('click', function () {
        const accion = $(this).data('action');
        manejarAccionMenu(accion);
    });

    // ---------------------------------------------------------
    // FUNCIONES PRINCIPALES DEL FLUJO
    // ---------------------------------------------------------

    /**
     * PASO 1: Inicia el flujo de validación de integridad
     */
    function iniciarFlujoValidacion() {
        mostrarLoader("Validando Integridad de Sesión...<br><small class='text-muted'>Verificando configuración de caja</small>");

        $.ajax({
            url: ValidacionIntegridadUrl,
            type: 'POST',
            dataType: 'json',
            timeout: 60000,
            success: function (response) {
                ocultarLoader();
                procesarValidacionIntegridad(response);
            },
            error: function (xhr, status, error) {
                ocultarLoader();
                manejarErrorAjax(xhr, status, error, "validar integridad");
            }
        });
    }

    /**
     * PASO 2: Procesa el resultado de la validación de integridad
     * resultado = 0: Procede automáticamente con apertura (SIN modal)
     * resultado = 3: Muestra modal para que usuario decida
     * resultado = 4: Cambiar PV
     * otro: Salir
     */
    function procesarValidacionIntegridad(response) {
        // Hidratar datos en las vistas
        $("#lblCajero").text(response.usuario || "Usuario");
        $("#lblCajaId").text("CAJA " + (response.caja_id || "N/A"));

        const resultado = response.resultado;

        if (resultado === 0) {
            // ✅ CORRECTO: Procede automáticamente con apertura
            console.log("✅ Validación OK - Procediendo automáticamente con apertura");
            mostrarLoader("Procediendo a realizar apertura de caja...<br><small class='text-muted'>Inicializando punto de venta</small>");

            // Pequeña pausa visual para que el usuario vea el mensaje
            setTimeout(() => {
                procesarAperturaCaja();
            }, 800);
        }
        else if (resultado === 3) {
            // ✅ CORRECTO: Muestra modal para que usuario evalúe opciones
            console.log("⚠️ Validación resultado=3 - Mostrando opciones al usuario");
            mostrarModalValidacionConOpciones(response.mensaje);
        }
        else if (resultado === 4) {
            // Cambiar punto de venta
            console.log("🔄 Validación resultado=4 - Cambiar PV");
            mostrarModalCambioPV(response.mensaje);
        }
        else if (resultado < 0) {
            // Error crítico
            console.error("❌ Error crítico en validación");
            mostrarErrorCritico(response.mensaje || "Error crítico al validar integridad.");
        }
        else {
            // Cualquier otro resultado - Salir
            console.warn("⚠️ Resultado inesperado - Salir");
            mostrarAdvertenciaYSalir(response.mensaje || "No se puede continuar. Contacte al administrador.");
        }
    }

    /**
     * PASO 3: Procesa la apertura de caja
     * resultado = 0: Apertura exitosa - Obtener datos
     * resultado = 3: Caja ya abierta - Menú solo cierre
     * otro: Error - Salir
     */
    function procesarAperturaCaja() {
        let $btn = $("#btnBuenoApertura");
        let originalText = "";
        let botonExiste = $btn.length > 0;

        if (botonExiste) {
            originalText = $btn.html();
            $btn.prop("disabled", true).html("<i class='bx bx-loader-alt bx-spin'></i> Abriendo Caja...");
        }

        $.ajax({
            url: AperturaCajaUrl,
            type: 'POST',
            dataType: 'json',
            success: function (response) {
                ocultarLoader();

                if (botonExiste) {
                    $btn.prop("disabled", false).html(originalText);
                }

                // ✅ CORRECCIÓN: Validar response.ok y redirigir a login
                if (!response.ok) {
                    console.error("❌ No se pudo realizar apertura - Redirigiendo a login");
                    mostrarMensajeErrorYSalir(response.mensaje || "Error al realizar apertura de caja.");
                    return;
                }

                const resultado = response.resultado;

                if (resultado === 0) {
                    // Apertura exitosa - Obtener datos de caja
                    console.log("✅ Apertura exitosa - Obteniendo datos");
                    const modal = getModalValidacion();
                    if (modal) modal.hide();
                    obtenerDatosCaja();
                }
                else if (resultado === 3) {
                    // Caja ya abierta - Cerrar modal ANTES de mostrar mensaje
                    console.log("⚠️ Caja ya abierta - Menú solo cierre");
                    const modal = getModalValidacion();

                    if (modal) modal.hide();

                    nivelAccesoMenu = 'solo-cierre';

                    setTimeout(() => {
                        AbrirMensaje(
                            "Atención",
                            response.mensaje,
                            function () {
                                $("#msjModal").modal("hide");

                                setTimeout(() => {
                                    configurarMenuSegunAcceso();
                                    const menuModal = getModalMenu();
                                    if (menuModal) menuModal.show();
                                }, 400);
                            },
                            false,
                            ["Continuar"],
                            "info!",
                            null
                        );
                    }, 500);
                }
                else {
                    // ✅ CORRECCIÓN: Error en apertura - Redirigir a login
                    console.error("❌ Error en apertura de caja - Resultado:", resultado);
                    mostrarErrorYSalir(response.mensaje || "No se pudo realizar la apertura de caja.");
                }
            },
            error: function (xhr, status, error) {
                ocultarLoader();
                if (botonExiste) {
                    $btn.prop("disabled", false).html(originalText);
                }
                manejarErrorAjax(xhr, status, error, "apertura de caja");
            }
        });
    }

    /**
    * PASO 4: Obtiene los datos de la caja después de apertura exitosa
    * resultado = 0: Datos OK - Menú completo
    * otro: Error - Salir
    * 
    * NUEVO: Maneja advertencias del sistema (mostrar_mensaje = true)
    */
    function obtenerDatosCaja() {
        mostrarLoader("Cargando datos de caja...<br><small class='text-muted'>Configurando punto de venta</small>");

        $.ajax({
            url: ObtenerDatosCajaUrl,
            type: 'POST',
            dataType: 'json',
            success: function (response) {
                ocultarLoader();

                // 📋 LOGGING: Registrar respuesta completa para trazabilidad
                console.log("📦 Respuesta ObtenerDatosCaja:", {
                    ok: response.ok,
                    resultado: response.resultado,
                    mensaje: response.mensaje,
                    mostrar_mensaje: response.mostrar_mensaje,
                    mensaje_advertencia: response.mensaje_advertencia,
                    tiene_datos: !!response.datos
                });

                // ✅ VALIDACIÓN 1: Respuesta no exitosa o resultado erróneo
                if (!response.ok || response.resultado !== 0) {
                    console.error("❌ Error al obtener datos de caja - Resultado:", response.resultado);
                    mostrarErrorYSalir(response.mensaje || "Error al obtener datos de caja.");
                    return;
                }

                // ✅ NUEVO: Actualizar el footer del menú con los datos recibidos
                actualizarFooterMenu(response.datos);

                // ✅ VALIDACIÓN 2: Verificar si hay mensaje de advertencia del sistema
                const tieneAdvertencia = response.mostrar_mensaje === true && response.mensaje_advertencia;

                if (tieneAdvertencia) {
                    console.warn("⚠️ Advertencia del sistema detectada:", response.mensaje_advertencia);

                    // Construir información adicional según el tipo de controlador
                    let datosAdicionales = construirDatosAdvertencia(response.datos);

                    // Mostrar advertencia y continuar después
                    mostrarAdvertenciaConContinuacion(
                        response.mensaje_advertencia,
                        function () {
                            // Callback: Continuar con el flujo normal
                            continuarConMenuCompleto(response);
                        },
                        datosAdicionales
                    );
                } else {
                    // ✅ Sin advertencias: Continuar directamente
                    console.log("✅ Datos de caja obtenidos sin advertencias - Menú completo");
                    continuarConMenuCompleto(response);
                }
            },
            error: function (xhr, status, error) {
                ocultarLoader();
                manejarErrorAjax(xhr, status, error, "obtener datos de caja");
            }
        });
    }

    /**
     * Construye información adicional para mostrar en advertencias
     * según el contexto y datos disponibles
     */
    function construirDatosAdvertencia(datos) {
        if (!datos) return null;

        let info = '';

        if (datos.caja_id) {
            info += `<i class='bx bx-store'></i> Caja: <strong>${datos.caja_id}</strong><br>`;
        }

        if (datos.caja_nombre) {
            info += `<i class='bx bx-tag'></i> Nombre: <strong>${datos.caja_nombre}</strong><br>`;
        }

        if (datos.caja_nro_proceso) {
            info += `<i class='bx bx-hash'></i> Proceso: <strong>${datos.caja_nro_proceso}</strong><br>`;
        }

        if (datos.usuario) {
            info += `<i class='bx bx-user'></i> Usuario: <strong>${datos.usuario}</strong><br>`;
        }

        return info || null;
    }

    /**
     * Continúa con el flujo normal: Configura menú completo y lo muestra
     */
    function continuarConMenuCompleto(response) {
        console.log("📊 Datos de caja:", response.datos);

        nivelAccesoMenu = 'completo';

        setTimeout(() => {
            configurarMenuSegunAcceso();
            const menuModal = getModalMenu();
            if (menuModal) menuModal.show();
        }, 400);
    }

    /**
     * PASO ALTERNATIVO: Procesa el cambio de punto de venta
     * resultado = 0: Cambio exitoso - Continuar con apertura
     * resultado = -1: MOCK - Funcionalidad no implementada
     * otro: Error - Salir
     */
    function procesarCambioPV() {
        // Mostrar loader inmediatamente (modal ya está cerrado)
        mostrarLoader("Procesando cambio de punto de venta...<br><small class='text-muted'>Por favor espere</small>");

        $.ajax({
            url: CambioPVUrl,
            type: 'POST',
            dataType: 'json',
            data: { nuevo_pv_id: "" },
            timeout: 30000,
            success: function (response) {
                ocultarLoader();

                // Validar respuesta
                if (!response.ok) {
                    // Error controlado (incluye MOCK)
                    const esMock = response.resultado === -1 && response.mensaje && response.mensaje.includes("MOCK");
                    
                    if (esMock) {
                        // MOCK: Funcionalidad no implementada
                        console.warn("⚠️ Cambio de PV - MOCK: Funcionalidad no implementada");
                        
                        AbrirMensaje(
                            "Funcionalidad en Desarrollo",
                            `<div class="text-center">
                                <i class='bx bx-info-circle text-info' style='font-size: 3rem;'></i>
                                <p class="mt-3">${response.mensaje}</p>
                                <hr>
                                <small class="text-muted">
                                    <i class='bx bx-user'></i> Usuario: <strong>${response.usuario}</strong><br>
                                    <i class='bx bx-store'></i> Caja: <strong>${response.caja_id}</strong>
                                </small>
                            </div>`,
                            function () {
                                $("#msjModal").modal("hide");
                                setTimeout(() => {
                                    window.location.href = logout;
                                }, 300);
                            },
                            false,
                            ["Aceptar"],
                            "info!",
                            null
                        );
                    } else {
                        // Error real
                        mostrarErrorYSalir(response.mensaje || "Error al cambiar punto de venta.");
                    }
                    return;
                }

                // Cambio exitoso (resultado = 0)
                console.log("✅ Cambio de PV exitoso - Procediendo con apertura automática");
                
                mostrarLoader("Procediendo a realizar apertura de caja...<br><small class='text-muted'>Nuevo punto de venta configurado</small>");
                setTimeout(() => {
                    procesarAperturaCaja();
                }, 800);
            },
            error: function (xhr, status, error) {
                ocultarLoader();
                
                let mensajeError = "Error al procesar cambio de punto de venta.";
                
                if (status === 'timeout') {
                    mensajeError = "El proceso de cambio de PV tardó demasiado tiempo. Por favor, contacte al administrador.";
                }  else if (xhr.status === 500) {
                    mensajeError = "Error interno del servidor. Contacte al administrador.";
                } else if (xhr.responseJSON && xhr.responseJSON.mensaje) {
                    mensajeError = xhr.responseJSON.mensaje;
                }

                mostrarErrorYSalir(mensajeError);
            }
        });
    }

    // ---------------------------------------------------------
    // FUNCIONES DE INTERFAZ Y CONTROL DE ACCESO
    // ---------------------------------------------------------

    /**
     * Configura el menú según el nivel de acceso
     */
    function configurarMenuSegunAcceso() {
        const $botones = $('.menu-btn-enhanced');

        // Resetear todos los botones
        $botones.prop('disabled', false).removeClass('disabled-menu-item');

        switch (nivelAccesoMenu) {
            case 'solo-cierre':
                // Solo habilitar botón de cierre
                $botones.not('[data-action="cierre"]').prop('disabled', true).addClass('disabled-menu-item');
                console.log("🔒 Menú configurado: Solo CIERRE activo");
                break;

            case 'parcial':
                // Deshabilitar funciones críticas que requieren caja abierta
                $botones.filter('[data-action="facturacion"], [data-action="cobranza-diferida"], [data-action="cobranza"], [data-action="cierre"]')
                    .prop('disabled', true).addClass('disabled-menu-item');
                console.log("⚠️ Menú configurado: Acceso PARCIAL");
                break;

            case 'completo':
                // Todos los botones habilitados
                console.log("✅ Menú configurado: Acceso COMPLETO");
                break;

            default:
                // Deshabilitar todo por seguridad
                $botones.prop('disabled', true).addClass('disabled-menu-item');
                console.warn("🚫 Menú configurado: SIN ACCESO");
                break;
        }
    }

    /**
     * Muestra el modal de validación con opciones (resultado = 3)
     * Solo se usa cuando el usuario debe tomar una decisión
     */
    function mostrarModalValidacionConOpciones(mensaje) {
        if (mensaje) {
            $("#mensajeValidacion").text(mensaje);
        }
        setTimeout(() => {
            const modal = getModalValidacion();
            if (modal) modal.show();
        }, 500);
    }

    /**
     * Muestra el modal de cambio de PV (resultado = 4)
     */
    function mostrarModalCambioPV(mensaje, nuevoPvId) {
        if (mensaje) {
            $("#mensajeCambioPV").text(mensaje);
        }

        // Mostrar el PV destino si viene del backend
        if (nuevoPvId) {
            $("#lblNuevoPV").text(`Punto de Venta: ${nuevoPvId}`);
        }

        $("#lblUsuarioPV").text($("#lblCajero").text() || "---");

        setTimeout(() => {
            const modal = getModalCambiaPV();
            if (modal) modal.show();
        }, 500);
    }

 

    /**
     * Muestra un mensaje de advertencia del sistema y ejecuta callback al cerrar
     * @param {string} mensaje - Mensaje a mostrar
     * @param {function} callback - Función a ejecutar después de cerrar el mensaje
     * @param {string} datos - Datos adicionales opcionales para mostrar
     */
    function mostrarAdvertenciaConContinuacion(mensaje, callback, datos) {
        let contenidoHTML = `
            <div class="text-center">
                <i class='bx bx-info-circle text-warning' style='font-size: 3rem;'></i>
                <p class="mt-3">${mensaje}</p>
        `;

        // Si hay datos adicionales, mostrarlos
        if (datos) {
            contenidoHTML += `
                <hr>
                <small class="text-muted">
                    ${datos}
                </small>
            `;
        }

        contenidoHTML += `
                <hr>
                <small class="text-muted">
                    <i class='bx bx-check-circle'></i> Puede continuar operando con la caja.
                </small>
            </div>
        `;

        AbrirMensaje(
            "ADVERTENCIA DEL SISTEMA",
            contenidoHTML,
            function () {
                $("#msjModal").modal("hide");

                // Ejecutar callback después de cerrar el modal
                if (typeof callback === 'function') {
                    setTimeout(() => {
                        callback();
                    }, 300);
                }
            },
            false,
            ["Continuar"],
            "warn!",
            null
        );
    }

    function mostrarErrorCritico(mensaje) {
        AbrirMensaje(
            "ATENCIÓN",
            mensaje,
            function () {
                $("#msjModal").modal("hide");
                window.location.href = logout;
            },
            false,
            ["Salir"],
            "warn!",
            null
        );
    }

    function mostrarErrorYSalir(mensaje) {
        AbrirMensaje(
            "INFORMACION",
            mensaje,
            function () {
                $("#msjModal").modal("hide");
                window.location.href = logout;
            },
            false,
            ["Salir"],
            "info!",
            null
        );
    }

    function mostrarAdvertenciaYSalir(mensaje) {
        AbrirMensaje(
            "ATENCIÓN",
            mensaje,
            function () {
                $("#msjModal").modal("hide");
                window.location.href = logout;
            },
            false,
            ["Salir"],
            "warn!",
            null
        );
    }

    // ✅ NUEVA FUNCIÓN: Muestra error y redirige a login
    function mostrarMensajeErrorYSalir(mensaje) {
        AbrirMensaje(
            "informacion",
            `<div class="text-center">
                <i class='bx bx-info-circle golden-message-info' style='font-size: 3rem;'></i>
                <p class="mt-3">${mensaje}</p>
                <hr>
                <small class="text-muted">
                    <i class='bx bx-info-circle'></i> Será redirigido al inicio de sesión.
                </small>
            </div>`,
            function () {
                $("#msjModal").modal("hide");
                setTimeout(() => {
                    console.log("🚪 Redirigiendo al login...");
                    window.location.href = logout;
                }, 300);
            },
            false,
            ["Salir"],
            "error!",
            null
        );
    }

    function mostrarMensajeError(mensaje) {
        AbrirMensaje(
            "ERROR",
            mensaje,
            function () {
                $("#msjModal").modal("hide");
            },
            false,
            ["Aceptar"],
            "error!",
            null
        );
    }

    // ✅ ACTUALIZADO: Función de manejo de errores AJAX unificada
    function manejarErrorAjax(xhr, status, error, operacion) {
        let mensajeError = `Error desconocido al ${operacion}.`;

        // ✅ NUEVO: Usar función centralizada de siteGen.js
        if (esSesionExpirada(xhr.status)) {
            manejarSesionExpirada(`La operación de ${operacion} falló porque su sesión ha expirado.`);
            return;
        }

        if (status === 'timeout') {
            mensajeError = `La operación de ${operacion} tardó demasiado tiempo. Por favor, intente nuevamente.`;
        } else if (xhr.status === 500) {
            mensajeError = "Error interno del servidor. Contacte al administrador.";
        } else if (xhr.responseJSON && xhr.responseJSON.mensaje) {
            mensajeError = xhr.responseJSON.mensaje;
        }

        mostrarErrorYSalir(mensajeError);
    }

    // ---------------------------------------------------------
    // GESTOR DE ACCIONES DEL MENÚ PRINCIPAL
    // ---------------------------------------------------------

    function manejarAccionMenu(accion) {
        console.log(`🎯 Acción seleccionada: ${accion}`);

        switch (accion) {
            case 'facturacion':
                abrirModuloFacturacion();
                break;
            case 'cobranza-diferida':
                abrirModuloCobranzaDiferida();
                break;
            case 'devolucion-nc':
                abrirModuloDevolucion();
                break;
            case 'debito-credito':
                abrirModuloDebitoCredito();
                break;
            case 'cobranzacc':
                abrirModuloCobranzacc();
                break;
            case 'anula-cobranza':
                abrirModuloAnulaCobranza();
                break;
            case 'dist-facturacion':
                abrirModuloDistribucionFacturacion();
                break;
            case 'dist-cobranza':
                abrirModuloDistribucionCobranza();
                break;
            case 'cambio-valores':
                abrirModuloCambioValores();
                break;
            case 'rendiciones':
                abrirModuloRendiciones();
                break;
            case 'cierre':
                abrirModuloCierre();
                break;
            case 'administrador':
                abrirModuloAdministrador();
                break;
            case 'reportes-z':
                abrirModuloReportesZ();
                break;
            case 'demo-teclado':
                abrirModalTecladoDemo();
                break;
            default:
                console.warn(`⚠️ Acción no implementada: ${accion}`);
                mostrarMensajeNoImplementado(accion);
                break;
        }
    }

    // Funciones placeholder para módulos
    function abrirModuloFacturacion() {
        console.log('💵 Iniciando validación para Facturación...');

        // Mostrar loader
        mostrarLoader("Validando datos de caja...<br><small class='text-muted'>Preparando módulo de facturación</small>");

        $.ajax({
            url: FacturacionValidarUrl,
            type: 'post',
            dataType: 'json',
            timeout: 10000,
            success: function (response) {
                ocultarLoader();

                if (!response.success) {
                    // ❌ Validación fallida
                    console.error("❌ Validación de datos de caja fallida:", response.message);
                    
                    AbrirMensaje(
                        "Error de Validación",
                        `<div class="text-center">
                            <i class='bx bx-error-circle text-danger' style='font-size: 3rem;'></i>
                            <p class="mt-3">${response.message}</p>
                            <hr>
                            <small class="text-muted">
                                Por favor, contacte al administrador o verifique la configuración de la caja.
                            </small>
                        </div>`,
                        function () {
                            $("#msjModal").modal("hide");
                        },
                        false,
                        ["Aceptar"],
                        "error!",
                        null
                    );
                    return;
                }

                // ✅ Validación exitosa
                console.log("✅ Validación exitosa - Abriendo módulo de Facturación");
                
                // Cerrar el modal del menú principal
                const menuModal = getModalMenu();
                if (menuModal) menuModal.hide();
                
                // Mostrar loader de transición
                mostrarLoader("Abriendo módulo de Facturación...<br><small class='text-muted'>Por favor espere</small>");
                
                // Redirigir al área de Facturación después de una breve pausa
                setTimeout(() => {
                    window.location.href = facturacionInicializaUrl;
                }, 800);
            },
            error: function (xhr, status, error) {
                ocultarLoader();
                
                console.error("❌ Error al validar datos para Facturación:", error);
                
                let mensajeError = "Error al validar los datos de la caja para Facturación.";
                
                if (status === 'timeout') {
                    mensajeError = "La validación tardó demasiado tiempo. Por favor, intente nuevamente.";
                }  else if (xhr.status === 500) {
                    mensajeError = "Error interno del servidor. Contacte al administrador.";
                } else if (xhr.responseJSON && xhr.responseJSON.message) {
                    mensajeError = xhr.responseJSON.message;
                }

                AbrirMensaje(
                    "Error",
                    `<div class="text-center">
                        <i class='bx bx-error-circle text-danger' style='font-size: 3rem;'></i>
                        <p class="mt-3">${mensajeError}</p>
                    </div>`,
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

    function abrirModuloCobranzaDiferida() {
        console.log('💰 Iniciando validación para Cobranza Diferida...');
        mostrarLoader("Validando datos de caja...<br><small class='text-muted'>Preparando módulo de Cobranza Diferida</small>");

        $.ajax({
            url: cobranzaDiferidaValidarUrl, // ✅ NUEVA URL
            type: 'post',
            dataType: 'json',
            timeout: 10000,
            success: function (response) {
                ocultarLoader();

                if (!response.success) {
                    console.error("❌ Validación de datos de caja fallida para Cobranza Diferida:", response.message);
                    AbrirMensaje(
                        "Error de Validación",
                        `<div class="text-center">
                            <i class='bx bx-error-circle text-danger' style='font-size: 3rem;'></i>
                            <p class="mt-3">${response.message}</p>
                            <hr>
                            <small class="text-muted">
                                Por favor, contacte al administrador o verifique la configuración de la caja.
                            </small>
                        </div>`,
                        function () { $("#msjModal").modal("hide"); },
                        false, ["Aceptar"], "error!", null
                    );
                    return;
                }

                console.log("✅ Validación exitosa - Abriendo módulo de Cobranza Diferida");
                //busca el modal cargado en memoria y lo cierra
                const menuModal = getModalMenu();
                if (menuModal) menuModal.hide();

                mostrarLoader("Abriendo módulo de Cobranza Diferida...<br><small class='text-muted'>Por favor, espere...</small>");
                setTimeout(() => {
                    window.location.href = cobranzaDiferidaInicializaUrl; // ✅ NUEVA URL
                }, 800);
            },
            error: function (xhr, status, error) {
                ocultarLoader();
                let mensajeError = "Error al validar los datos de la caja para Cobranza Diferida.";
                if (status === 'timeout') {
                    mensajeError = "La validación tardó demasiado tiempo. Por favor, intente nuevamente.";
                } else if (xhr.status === 500) {
                    mensajeError = "Error interno del servidor. Contacte al administrador.";
                } else if (xhr.responseJSON && xhr.responseJSON.message) {
                    mensajeError = xhr.responseJSON.message;
                }
                AbrirMensaje("Error", `<div class="text-center"><i class='bx bx-error-circle text-danger' style='font-size: 3rem;'></i><p class="mt-3">${mensajeError}</p></div>`, function () { $("#msjModal").modal("hide"); }, false, ["Aceptar"], "error!", null);
            }
        });
    }
    function abrirModuloDevolucion() {
        console.log('↩️ Iniciando validación para Nota de Crédito por Devolución...');

        if (
            typeof notaCreditoDevolucionValidarUrl === 'undefined' ||
            !notaCreditoDevolucionValidarUrl ||
            typeof notaCreditoDevolucionInicializaUrl === 'undefined' ||
            !notaCreditoDevolucionInicializaUrl
        ) {
            console.error(
                'No están definidas las URLs del módulo Nota de Crédito por Devolución.'
            );

            AbrirMensaje(
                'Error de configuración',
                'No se pudo preparar el módulo de Nota de Crédito por Devolución.',
                function () {
                    $('#msjModal').modal('hide');
                },
                false,
                ['Aceptar'],
                'error!',
                null
            );

            return;
        }

        mostrarLoader(
            "Validando datos de caja...<br>" +
            "<small class='text-muted'>Preparando módulo de Nota de Crédito por Devolución</small>"
        );

        $.ajax({
            url: notaCreditoDevolucionValidarUrl,
            type: 'POST',
            dataType: 'json',
            timeout: 10000,

            success: function (response) {
                ocultarLoader();

                if (!response || response.success !== true) {
                    const mensaje =
                        response?.message ||
                        'No fue posible validar los datos de caja para iniciar la devolución.';

                    console.error(
                        'Validación fallida para NC por Devolución:',
                        mensaje
                    );

                    AbrirMensaje(
                        'Error de validación',
                        `<div class="text-center">
                        <i class='bx bx-error-circle text-danger'
                           style='font-size: 3rem;'></i>
                        <p class="mt-3">${mensaje}</p>
                        <hr>
                        <small class="text-muted">
                            Verifique la configuración de caja o contacte al administrador.
                        </small>
                    </div>`,
                        function () {
                            $('#msjModal').modal('hide');
                        },
                        false,
                        ['Aceptar'],
                        'error!',
                        null
                    );

                    return;
                }

                console.log(
                    'Validación exitosa. Abriendo módulo de Nota de Crédito por Devolución.'
                );

                const menuModal = getModalMenu();

                if (menuModal) {
                    menuModal.hide();
                }

                mostrarLoader(
                    "Abriendo módulo de Nota de Crédito por Devolución...<br>" +
                    "<small class='text-muted'>Por favor, espere...</small>"
                );

                setTimeout(function () {
                    window.location.href = notaCreditoDevolucionInicializaUrl;
                }, 800);
            },

            error: function (xhr, status, error) {
                ocultarLoader();

                console.error(
                    'Error al validar datos para NC por Devolución:',
                    {
                        status: xhr?.status,
                        textStatus: status,
                        error
                    }
                );

                let mensaje =
                    'Error al validar los datos de caja para Nota de Crédito por Devolución.';

                if (status === 'timeout') {
                    mensaje =
                        'La validación tardó demasiado tiempo. Intente nuevamente.';
                } else if (xhr?.status === 500) {
                    mensaje =
                        'Error interno del servidor. Contacte al administrador.';
                } else if (xhr?.responseJSON?.message) {
                    mensaje = xhr.responseJSON.message;
                }

                AbrirMensaje(
                    'Error',
                    `<div class="text-center">
                    <i class='bx bx-error-circle text-danger'
                       style='font-size: 3rem;'></i>
                    <p class="mt-3">${mensaje}</p>
                </div>`,
                    function () {
                        $('#msjModal').modal('hide');
                    },
                    false,
                    ['Aceptar'],
                    'error!',
                    null
                );
            }
        });
    }

    function abrirModuloDebitoCredito() {
        console.log('Iniciando validacion para ND, NC y Factura de Servicio...');

        if (
            typeof notaDebitoCreditoValidarUrl === 'undefined' ||
            !notaDebitoCreditoValidarUrl ||
            typeof notaDebitoCreditoInicializaUrl === 'undefined' ||
            !notaDebitoCreditoInicializaUrl
        ) {
            AbrirMensaje(
                'Error de configuracion',
                'No se pudo preparar el modulo de Nota de Debito, Credito y Factura de Servicio.',
                function () {
                    $('#msjModal').modal('hide');
                },
                false,
                ['Aceptar'],
                'error!',
                null
            );

            return;
        }

        mostrarLoader(
            "Validando datos de caja...<br>" +
            "<small class='text-muted'>Preparando modulo de ND, NC y Factura de Servicio</small>"
        );

        $.ajax({
            url: notaDebitoCreditoValidarUrl,
            type: 'POST',
            dataType: 'json',
            timeout: 10000,
            success: function (response) {
                ocultarLoader();

                if (!response || response.success !== true) {
                    AbrirMensaje(
                        'Error de validacion',
                        response?.message ||
                        'No fue posible validar los datos de caja para iniciar el modulo.',
                        function () {
                            $('#msjModal').modal('hide');
                        },
                        false,
                        ['Aceptar'],
                        'error!',
                        null
                    );

                    return;
                }

                const menuModal = getModalMenu();

                if (menuModal) {
                    menuModal.hide();
                }

                mostrarLoader(
                    "Abriendo modulo de ND, NC y Factura de Servicio...<br>" +
                    "<small class='text-muted'>Por favor, espere...</small>"
                );

                setTimeout(function () {
                    window.location.href = notaDebitoCreditoInicializaUrl;
                }, 800);
            },
            error: function (xhr, status) {
                ocultarLoader();

                let mensaje =
                    'Error al validar los datos de caja para ND, NC y Factura de Servicio.';

                if (status === 'timeout') {
                    mensaje = 'La validacion tardo demasiado tiempo. Intente nuevamente.';
                } else if (xhr?.responseJSON?.message) {
                    mensaje = xhr.responseJSON.message;
                }

                AbrirMensaje(
                    'Error',
                    mensaje,
                    function () {
                        $('#msjModal').modal('hide');
                    },
                    false,
                    ['Aceptar'],
                    'error!',
                    null
                );
            }
        });
    }
    function abrirModuloCobranzacc() {
        console.log('💰 Iniciando Módulo Cobranza en Cuenta Corriente');
        mostrarLoader("Validando datos de caja...<br><small class='text-muted'>Preparando módulo de Cobranza en Cuenta Corriente</small>");

        $.ajax({
            url: validarModuloCCUrl, // ✅ NUEVA URL
            type: 'post',
            dataType: 'json',
            timeout: 10000,
            success: function (response) {
                ocultarLoader();

                if (!response.success) {
                    console.error("❌ Validación de autenticación fallida para Cobranza en Cuenta Corriente:", response.message);
                    AbrirMensaje(
                        "Error de Validación",
                        `<div class="text-center">
                            <i class='bx bx-error-circle text-danger' style='font-size: 3rem;'></i>
                            <p class="mt-3">${response.message}</p>
                            <hr>
                            <small class="text-muted">
                                Por favor, contacte al administrador o verifique la configuración o autentiquese nuevamente..
                            </small>
                        </div>`,
                        function () {
                            setTimeout(() => {
                                window.location.href = MenuCajaUrl; // ✅ verifica que no esta autenticado y reenvia a Login
                            }, 100); },
                        false, ["Continuar"], "error!", null
                    );
                    return;
                }

                console.log("✅ Validación exitosa - Abriendo módulo de Cobranza en Cuenta Corriente");
                //busca el modal cargado en memoria y lo cierra
                const menuModal = getModalMenu();
                if (menuModal) menuModal.hide();

                mostrarLoader("Abriendo módulo de Cobranza en CUENTA CORRIENTE...<br><small class='text-muted'>Por favor, espere...</small>");
                setTimeout(() => {
                    window.location.href = accesoModuloCCUrl; // ✅ NUEVA URL
                }, 800);
            },
            error: function (xhr, status, error) {
                ocultarLoader();
                let mensajeError = "Error al validar los datos de la caja para Cobranza Diferida.";
                if (status === 'timeout') {
                    mensajeError = "La validación tardó demasiado tiempo. Por favor, intente nuevamente.";
                } else if (xhr.status === 500) {
                    mensajeError = "Error interno del servidor. Contacte al administrador.";
                } else if (xhr.responseJSON && xhr.responseJSON.message) {
                    mensajeError = xhr.responseJSON.message;
                }
                AbrirMensaje("Error", `<div class="text-center"><i class='bx bx-error-circle text-danger' style='font-size: 3rem;'></i><p class="mt-3">${mensajeError}</p></div>`, function () { $("#msjModal").modal("hide"); }, false, ["Aceptar"], "error!", null);
            }
        });
    }
    function abrirModuloAnulaCobranza() { console.log('❌ Anula Cobranza...'); }
    function abrirModuloDistribucionFacturacion() { console.log('📊 Distribución Facturación...'); }
    function abrirModuloDistribucionCobranza() { console.log('📈 Distribución Cobranza...'); }
    function abrirModuloCambioValores() { console.log('🔄 Cambio de Valores...'); }
    function abrirModuloRendiciones() { console.log('📄 Rendiciones...'); }

    function abrirModuloCierre() {
        console.log('🔒 Iniciando proceso de cierre de caja...');

        AbrirMensaje(
            "CONFIRMAR CIERRE DE CAJA",
            "¿Está seguro de que desea cerrar la caja?<br><br>" +
            "<strong class='text-danger'>⚠️ Esta acción finalizará su sesión y cerrará la caja.</strong>",
            function (confirmado) {
                $("#msjModal").modal("hide");

                if (confirmado === "SI") {
                    setTimeout(() => {
                        procesarCierreCaja();
                    }, 300);
                }
            },
            true,
            ["Sí, Cerrar Caja", "Cancelar"],
            "warn!",
            null
        );
    }

    function abrirModuloAdministrador() { console.log('🛡️ Administrador...'); }
    function abrirModuloReportesZ() { console.log('📊 Reportes Z...'); }

    function abrirModalTecladoDemo() {
        const menuModal = getModalMenu();
        if (menuModal) menuModal.hide();
        setTimeout(() => {
            $('#modalTecladoDemo').modal('show');
        }, 500);
    }

    function mostrarMensajeNoImplementado(nombreModulo) {
        console.info(`ℹ️ ${nombreModulo} - Módulo en desarrollo`);
    }

    // ---------------------------------------------------------
    // FUNCIONES: CIERRE DE CAJA
    // ---------------------------------------------------------

    function procesarCierreCaja() {
        mostrarLoader("Procesando cierre de caja...<br><small class='text-muted'>Por favor espere, esto puede tardar unos momentos</small>");

        $.ajax({
            url: CierreCajaUrl,
            type: 'POST',
            dataType: 'json',
            timeout: 120000,
            success: function (response) {
                ocultarLoader();

                if (!response.ok) {
                    mostrarErrorCierre(response.mensaje || "Error al procesar cierre de caja.");
                    return;
                }

                const resultado = response.resultado;

                if (resultado === 0) {
                    console.log("✅ Cierre exitoso");
                    mostrarResumenCierre(response);
                } else {
                    console.error("❌ Error en cierre - Resultado:", resultado);
                    mostrarErrorCierre(response.mensaje || "No se pudo completar el cierre de caja.");
                }
            },
            error: function (xhr, status, error) {
                ocultarLoader();

                let mensajeError = "Error al procesar cierre de caja.";

                if (status === 'timeout') {
                    mensajeError = "El proceso de cierre tardó demasiado tiempo. Por favor, contacte al administrador para verificar el estado de la caja.";
                }  else if (xhr.status === 500) {
                    mensajeError = "Error interno del servidor al procesar el cierre. Contacte al administrador.";
                } else if (xhr.responseJSON && xhr.responseJSON.mensaje) {
                    mensajeError = xhr.responseJSON.mensaje;
                }

                mostrarErrorCierre(mensajeError);
            }
        });
    }

    function mostrarResumenCierre(response) {
        const datos = response.datos || {};

        let tablaResumen = '<div class="table-responsive"><table class="table table-sm table-bordered">';

        for (let clave in datos) {
            if (datos.hasOwnProperty(clave)) {
                let valor = datos[clave];

                let nombreCampo = clave
                    .split('_')
                    .map(palabra => palabra.charAt(0).toUpperCase() + palabra.slice(1))
                    .join(' ');

                let valorFormateado = valor;

                if (typeof valor === 'number') {
                    if (clave.toLowerCase().includes('total') ||
                        clave.toLowerCase().includes('monto') ||
                        clave.toLowerCase().includes('importe')) {
                        valorFormateado = '$ ' + formatearMoneda(valor);
                    } else {
                        valorFormateado = valor;
                    }
                } else if (valor === null || valor === undefined) {
                    valorFormateado = '---';
                }

                let claseDestacada = '';
                if (clave.toLowerCase().includes('total_general') ||
                    clave.toLowerCase().includes('resultado')) {
                    claseDestacada = 'table-active fw-bold';
                }

                tablaResumen += `
                    <tr class="${claseDestacada}">
                        <td class="text-end" style="width: 50%;"><strong>${nombreCampo}:</strong></td>
                        <td class="text-start" style="width: 50%;">${valorFormateado}</td>
                    </tr>
                `;
            }
        }

        tablaResumen += '</table></div>';

        const mensajeResumen = `
            <div class="text-center">
                <i class='bx bx-check-circle text-success' style='font-size: 3.5rem;'></i>
                <h4 class="mt-3 mb-3 text-success">✅ Cierre de Caja Exitoso</h4>
                <p class="text-muted mb-3">${response.mensaje || 'El proceso se completó correctamente'}</p>
                <hr>
            </div>
            ${tablaResumen}
            <div class="text-center mt-3">
                <small class="text-muted">
                    <i class='bx bx-info-circle'></i>
                    Usuario: <strong>${response.usuario || '---'}</strong> | 
                    Caja: <strong>${response.caja_id || '---'}</strong>
                </small>
            </div>
        `;

        AbrirMensaje(
            "RESUMEN DE CIERRE",
            mensajeResumen,
            function () {
                $("#msjModal").modal("hide");

                const menuModal = getModalMenu();
                if (menuModal) menuModal.hide();

                setTimeout(() => {
                    console.log("🚪 Redirigiendo al login después del cierre...");
                    window.location.href = logout;
                }, 500);
            },
            false,
            ["Salir"],
            "succ!",
            null
        );
    }

    function mostrarErrorCierre(mensaje) {
        AbrirMensaje(
            "ERROR EN CIERRE",
            `<div class="text-center">
                <i class='bx bx-error-circle text-danger' style='font-size: 3rem;'></i>
                <p class="mt-3">${mensaje}</p>
            </div>`,
            function () {
                $("#msjModal").modal("hide");
            },
            false,
            ["Aceptar"],
            "error!",
            null
        );
    }

    function formatearMoneda(valor) {
        if (valor === null || valor === undefined || isNaN(valor)) {
            return '0,00';
        }

        return new Intl.NumberFormat('es-AR', {
            minimumFractionDigits: 2,
            maximumFractionDigits: 2
        }).format(valor);
    }

    // ---------------------------------------------------------
    // COMPORTAMIENTO MODAL CAMBIO PV (SIMPLIFICADO)
    // ---------------------------------------------------------

    //const $modalCambiaPV = $('#modalCambiaPV');
    //cierreIntencional = false;

    //$modalCambiaPV.on('shown.bs.modal', function () {
    //    cierreIntencional = false;
    //    $("#lblUsuarioPV").text($("#lblCajero").text() || "---");
    //});

    //$('#btnCancelaCambiaPV, #btnConfirmaCambioPV').on('click', function () {
    //    cierreIntencional = true;
    //});

    //$modalCambiaPV.on('hide.bs.modal', function (e) {
    //    if (!cierreIntencional) {
    //        e.preventDefault();

    //        AbrirMensaje(
    //            "Confirmar Salida",
    //            "¿Está seguro de que desea cancelar el cambio de punto de venta?<br><br>" +
    //            "<small class='text-muted'><i class='bx bx-info-circle'></i> Si cancela, será redirigido al inicio de sesión.</small>",
    //            function (respuesta) {
    //                $("#msjModal").modal("hide");

    //                if (respuesta === "SI") {
    //                    cierreIntencional = true;
    //                    window.location.href = logout;
    //                }
    //            },
    //            true,
    //            ["Sí, Salir", "No, Continuar"],
    //            "warn!",
    //            null
    //        );
    //    } else {
    //        cierreIntencional = false;
    //    }
    //});
});

/**
 * ✅ NUEVO: Actualiza la información del footer en el modal del menú.
 * @param {object} datos - El objeto 'datos' de la respuesta AJAX.
 */
function actualizarFooterMenu(datos) {
    if (!datos) {
        console.warn("⚠️ No se proporcionaron datos para actualizar el footer del menú.");
        return;
    }

    $("#lblPuntoVenta").text(datos.caja_nombre || '---');
    $("#lblNroProceso").text(datos.caja_nro_proceso || '---');
    $("#lblNroCierre").text(datos.caja.caja.caja_nro_cierre || '---');
    $("#lblFechaHora").text(datos.caja.caja.caja_apertura || '---');
    
}
