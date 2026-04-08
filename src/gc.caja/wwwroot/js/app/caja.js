// ============================================
// GESTOR PRINCIPAL DEL FLUJO DE CAJA
// ============================================

$(function () {
    // Variables para referencias a modales (inicialización lazy)
    let modalValidacion = null;
    let modalMenu = null;
    let modalCambiaPV = null;

    // Variable global para control de acceso al menú
    let nivelAccesoMenu = 'ninguno'; // 'ninguno', 'parcial', 'solo-cierre', 'completo'

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
    $("#btnBuenoApertura").on("click", function () {
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
        const nuevoPvId = $("#inputNuevoPV").val();
        if (!nuevoPvId) {
            mostrarMensajeError("Debe ingresar un punto de venta válido.");
            return;
        }
        procesarCambioPV(nuevoPvId);
    });

    /**
     * Botón: CANCELAR CAMBIO PV
     */
    $("#btnCancelaCambiaPV").on("click", function () {
        const modal = getModalCambiaPV();
        if (modal) modal.hide();
        window.location.href = logout;
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

    // Manejadores de botones del menú
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
        // Si hay un botón visible, mostrar loading en él
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
                // Ocultar loader si estaba visible
                ocultarLoader();

                // Restaurar botón si existe
                if (botonExiste) {
                    $btn.prop("disabled", false).html(originalText);
                }

                if (!response.ok) {
                    mostrarMensajeError(response.mensaje || "Error al realizar apertura de caja.");
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
                    // ✅ CORREGIDO: Caja ya abierta - Cerrar modal ANTES de mostrar mensaje
                    console.log("⚠️ Caja ya abierta - Menú solo cierre");
                    const modal = getModalValidacion();
                    
                    // ✅ PASO 1: Cerrar el modal de validación
                    if (modal) modal.hide();
                    
                    nivelAccesoMenu = 'solo-cierre';
                    
                    // ✅ PASO 2: Esperar que el modal se cierre COMPLETAMENTE
                    setTimeout(() => {
                        // ✅ PASO 3: Mostrar mensaje informativo
                        AbrirMensaje(
                            "Atención", 
                            response.mensaje, 
                            function () {
                                // ✅ PASO 4: Cerrar mensaje
                                $("#msjModal").modal("hide");
                                
                                // ✅ PASO 5: Esperar que el mensaje se cierre
                                setTimeout(() => {
                                    // ✅ PASO 6: Configurar y mostrar menú
                                    configurarMenuSegunAcceso();
                                    const menuModal = getModalMenu();
                                    if (menuModal) menuModal.show();
                                }, 400); // Esperar cierre completo del mensaje
                            },
                            false, 
                            ["Continuar"], 
                            "info!", 
                            null
                        );
                    }, 500); // ✅ Esperar cierre completo del modal de validación
                } 
                else {
                    // Error en apertura - Salir
                    console.error("❌ Error en apertura de caja");
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
     */
    function obtenerDatosCaja() {
        mostrarLoader("Cargando datos de caja...<br><small class='text-muted'>Configurando punto de venta</small>");

        $.ajax({
            url: ObtenerDatosCajaUrl,
            type: 'POST',
            dataType: 'json',
            success: function (response) {
                ocultarLoader();

                if (!response.ok || response.resultado !== 0) {
                    mostrarErrorYSalir(response.mensaje || "Error al obtener datos de caja.");
                    return;
                }

                // Datos obtenidos exitosamente - Menú completo
                console.log("✅ Datos de caja obtenidos - Menú completo");
                console.log("📊 Datos:", response.datos);
                nivelAccesoMenu = 'completo';
                
                setTimeout(() => {
                    configurarMenuSegunAcceso();
                    const menuModal = getModalMenu();
                    if (menuModal) menuModal.show();
                }, 400);
            },
            error: function (xhr, status, error) {
                ocultarLoader();
                manejarErrorAjax(xhr, status, error, "obtener datos de caja");
            }
        });
    }

    /**
     * PASO ALTERNATIVO: Procesa el cambio de punto de venta
     * resultado = 0: Cambio exitoso - Continuar con apertura
     * otro: Error - Salir
     */
    function procesarCambioPV(nuevoPvId) {
        let $btn = $("#btnConfirmaCambioPV");
        let originalText = $btn.html();

        $btn.prop("disabled", true).html("<i class='bx bx-loader-alt bx-spin'></i> Cambiando PV...");

        $.ajax({
            url: CambioPVUrl,
            type: 'POST',
            dataType: 'json',
            data: { nuevo_pv_id: nuevoPvId },
            success: function (response) {
                $btn.prop("disabled", false).html(originalText);

                if (!response.ok || response.resultado !== 0) {
                    mostrarErrorYSalir(response.mensaje || "Error al cambiar punto de venta.");
                    return;
                }

                // Cambio exitoso - Proceder automáticamente con apertura
                console.log("✅ Cambio de PV exitoso - Procediendo con apertura automática");
                const modal = getModalCambiaPV();
                if (modal) modal.hide();
                
                mostrarLoader("Procediendo a realizar apertura de caja...<br><small class='text-muted'>Nuevo punto de venta configurado</small>");
                setTimeout(() => {
                    procesarAperturaCaja();
                }, 800);
            },
            error: function (xhr, status, error) {
                $btn.prop("disabled", false).html(originalText);
                manejarErrorAjax(xhr, status, error, "cambio de punto de venta");
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
                $botones.filter('[data-action="facturacion"], [data-action="cobranza"], [data-action="cierre"]')
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
    function mostrarModalCambioPV(mensaje) {
        if (mensaje) {
            $("#mensajeCambioPV").text(mensaje);
        }
        setTimeout(() => {
            const modal = getModalCambiaPV();
            if (modal) modal.show();
        }, 500);
    }

    // ---------------------------------------------------------
    // FUNCIONES DE MENSAJES Y MANEJO DE ERRORES
    // ---------------------------------------------------------

    function mostrarLoader(texto) {
        $('#loaderText').html(texto);
        $('#loaderOverlay').fadeIn(500);
    }

    function ocultarLoader() {
        $('#loaderOverlay').fadeOut(300);
    }

    function mostrarErrorCritico(mensaje) {
        AbrirMensaje(
            "ERROR CRÍTICO",
            mensaje,
            function () {
                $("#msjModal").modal("hide");
                window.location.href = logout;
            },
            false,
            ["Salir"],
            "error!",
            null
        );
    }

    function mostrarErrorYSalir(mensaje) {
        AbrirMensaje(
            "ERROR",
            mensaje,
            function () {
                $("#msjModal").modal("hide");
                window.location.href = logout;
            },
            false,
            ["Salir"],
            "error!",
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

    function manejarErrorAjax(xhr, status, error, operacion) {
        let mensajeError = `Error desconocido al ${operacion}.`;

        if (status === 'timeout') {
            mensajeError = `La operación de ${operacion} tardó demasiado tiempo. Por favor, intente nuevamente.`;
        } else if (xhr.status === 401) {
            mensajeError = "Su sesión ha expirado. Será redirigido al login.";
            setTimeout(() => {
                window.location.href = logout;
            }, 2000);
            return;
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
            case 'devolucion-nc':
                abrirModuloDevolucion();
                break;
            case 'debito-credito':
                abrirModuloDebitoCredito();
                break;
            case 'cobranza':
                abrirModuloCobranza();
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
    function abrirModuloFacturacion() { console.log('💵 Facturación...'); }
    function abrirModuloDevolucion() { console.log('↩️ Devolución NC...'); }
    function abrirModuloDebitoCredito() { console.log('💳 Débito y Crédito...'); }
    function abrirModuloCobranza() { console.log('💰 Cobranza...'); }
    function abrirModuloAnulaCobranza() { console.log('❌ Anula Cobranza...'); }
    function abrirModuloDistribucionFacturacion() { console.log('📊 Distribución Facturación...'); }
    function abrirModuloDistribucionCobranza() { console.log('📈 Distribución Cobranza...'); }
    function abrirModuloCambioValores() { console.log('🔄 Cambio de Valores...'); }
    function abrirModuloRendiciones() { console.log('📄 Rendiciones...'); }
    function abrirModuloCierre() { console.log('🔒 Cierre...'); }
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
});
