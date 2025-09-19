/**
 * Script para manejo de activación de ofertas
 * Comparte funcionalidad con ofertas.js para selección y gestión de canales
 */

// Variables de estado para gestión de selección
var modoSeleccionCanal = "ninguno"; // "individual", "multiple", "ninguno"
var canalIndividualSeleccionado = null;

// ✅ Inicialización del módulo (modificada)
$(function () {
    console.log("🚀 Iniciando ofertaActivar.js");
    
    // Inicializaciones básicas
    inicializarShortcutsBasicos();
    
    // Cargar ofertas sin activar
    cargarOfertasSinActivar();
    
    // Cargar canales (misma funcionalidad que en ofertas.js)
    try { 
        cargarCanales(); 
    } catch (e) { 
        console.error("Error al cargar canales:", e); 
    }
    
    // Inicializar fecha desde/hasta si existen los elementos
    if ($("#txtFechaDesde").length && $("#txtFechaHasta").length) {
        inicializarCamposFecha();
    }

    // Configurar botones principales
    $(document).on("click", "#btnActivarOfertas", function() {
        activarOfertas();
    });
    
    // NUEVO: Configurar botón de activar vencimiento
    $(document).on("click", "#btnActivarVencimiento", function() {
        activarOfertasVencidas();
    });
    
    $("#btnCancelaActivacion").on("click", function() {
        confirmarCancelacion();
    });

    console.log("✅ ofertaActivar.js listo");
});

// ✅ MODIFICADA: Función para cargar ofertas sin activar
function cargarOfertasSinActivar(admId = "0000", lpId = "001", pagina = 1) {
    AbrirWaiting("Cargando ofertas sin activar...");

    // Crear objeto de datos para enviar en el cuerpo de la solicitud
    var datosPost = {
        admId: admId,
        lp_id: lpId,
        pag: pagina
    };

    // Realizar la llamada AJAX usando POST como especifica el controlador
    $.ajax({
        url: presentarOfertasSinActivarUrl,
        type: "POST",
        data: datosPost,
        success: function(response) {
            CerrarWaiting();
            $("#gridOfertaNoActivas").html(response);
            
            // Configurar eventos para la grilla de ofertas
            configurarEventosGridOfertasSinActivar();
            
            // NUEVO: Verificar ofertas vencidas para activar/desactivar botón
            verificarOfertasVencidas();
        },
        error: function(xhr, status, error) {
            CerrarWaiting();
            console.error("Error al cargar ofertas sin activar:", error);
            ControlaMensajeError("Error al cargar ofertas sin activar: " + (error || "Error desconocido"));
            
            // Mostrar mensaje de error en el contenedor
            $("#gridOfertaNoActivas").html(`
                <div class="alert alert-danger">
                    <i class="bx bx-error-circle me-2"></i>
                    No se pudieron cargar las ofertas sin activar. 
                    <button class="btn btn-outline-danger btn-sm ms-3" onclick="cargarOfertasSinActivar()">
                        <i class="bx bx-refresh"></i> Reintentar
                    </button>
                </div>
            `);
        }
    });
}

// ✅ NUEVA: Función para configurar eventos en la grilla de ofertas sin activar
function configurarEventosGridOfertasSinActivar() {
    // Checkbox "Seleccionar todos" para ofertas
    $("#checkAllOfertas").off("change").on("change", function() {
        var isChecked = $(this).is(":checked");
        $(".check-oferta").prop("checked", isChecked);
        actualizarContadorOfertasSeleccionadas();
    });
    
    // Checkboxes individuales para ofertas
    $(".check-oferta").off("change").on("change", function() {
        var totalChecks = $(".check-oferta").length;
        var checkedCount = $(".check-oferta:checked").length;
        
        // Actualizar el checkbox "Seleccionar todos"
        $("#checkAllOfertas").prop("checked", totalChecks === checkedCount);
        
        actualizarContadorOfertasSeleccionadas();
    });
    
    // Botones para activar ofertas individuales
    $(".btn-activar-oferta").off("click").on("click", function() {
        var pId = $(this).data("p-id");
        var admId = $(this).data("adm-id");
        var lpId = $(this).data("lp-id");
        
        activarOfertaIndividual(pId, admId, lpId);
    });
    
    // Permitir seleccionar al hacer clic en la fila
    $("#tbGridOfertasSinActivar tbody tr").off("click").on("click", function(e) {
        // Solo si no se hizo clic en el botón o en el checkbox directamente
        if (!$(e.target).is('button, input, i')) {
            var checkbox = $(this).find('.check-oferta');
            checkbox.prop('checked', !checkbox.prop('checked'));
            checkbox.trigger('change');
        }
    });
    
    // Botones de paginación si existen
    $(".pagination .page-link").off("click").on("click", function(e) {
        e.preventDefault();
        var pagina = $(this).data("page") || 1;
        cargarOfertasSinActivar(undefined, undefined, pagina);
    });
    
    // Inicializar contador
    actualizarContadorOfertasSeleccionadas();
}

// ✅ NUEVA: Función para actualizar contador de ofertas seleccionadas
function actualizarContadorOfertasSeleccionadas() {
    var checkedCount = $(".check-oferta:checked").length;
    $("#ofertasSeleccionadas").text(checkedCount);
}

// ✅ OPTIMIZADA: Función para activar oferta individual
function activarOfertaIndividual(pId, admId, plId) {
    if (!pId) {
        AbrirMensaje(
            "ADVERTENCIA",
            "Falta el ID del producto para activar la oferta",
            function() {
                $("#msjModal").modal("hide");
                return true;
            },
            false,
            ["Aceptar"],
            "warn!",
            null
        );
        return;
    }
    
    // Obtener descripción del producto de la fila actual de la grilla
    var descripcion = "";
    try {
        var fila = $(`tr[data-producto-id="${pId}"]`);
        descripcion = fila.find('td:eq(2)').text().trim();
    } catch (e) {
        console.warn("No se pudo obtener la descripción del producto:", e);
    }
    
    // Componer mensaje con código y descripción
    var mensaje = `¿Está seguro que desea activar la oferta del producto <strong>${pId}</strong>`;
    if (descripcion) {
        mensaje += ` - <strong>${descripcion}</strong>`;
    }
    mensaje += '?';
    
    // Mostrar mensaje de confirmación
    AbrirMensaje(
        "CONFIRMAR ACTIVACIÓN DE OFERTA",
        mensaje,
        function(resp) {
            if (resp === "SI") {
                // Guardar los parámetros actuales de grilla
                var currentAdmId = admId || "0000";
                var currentLpId = plId || "001";
                
                AbrirWaiting("Activando oferta...");
                
                $.ajax({
                    url: activarOfertaUrl,
                    type: "POST",
                    data: {
                        'ids[0]': pId,
                        'admId': currentAdmId,
                        'lp_id': currentLpId
                    },
                    success: function(response) {
                        CerrarWaiting();
                        
                        if (response.error) {
                            AbrirMensaje(
                                "ERROR",
                                response.msg || "Error al activar oferta",
                                function() {
                                    $("#msjModal").modal("hide");
                                    return true;
                                },
                                false,
                                ["Aceptar"],
                                "error!",
                                null
                            );
                            return;
                        }
                        
                        if (response.warn) {
                            AbrirMensaje(
                                "ADVERTENCIA",
                                response.msg || "Advertencia al activar oferta",
                                function() {
                                    $("#msjModal").modal("hide");
                                    return true;
                                },
                                false,
                                ["Aceptar"],
                                "warn!",
                                null
                            );
                            return;
                        }
                        
                        // Mensaje de éxito y recarga del grid
                        AbrirMensaje(
                            "OPERACIÓN EXITOSA",
                            response.msg || "Oferta activada correctamente",
                            function() {
                                // Recargar el grid con los mismos parámetros
                                cargarOfertasSinActivar(currentAdmId, currentLpId);
                                $("#msjModal").modal("hide");
                                return true;
                            },
                            false,
                            ["Aceptar"],
                            "success!",
                            null
                        );
                    },
                    error: function(xhr, status, error) {
                        CerrarWaiting();
                        console.error("Error en solicitud:", error);
                        
                        AbrirMensaje(
                            "ERROR DE COMUNICACIÓN",
                            "Error de comunicación: " + (xhr.responseText || error || "Error desconocido"),
                            function() {
                                $("#msjModal").modal("hide");
                                return true;
                            },
                            false,
                            ["Aceptar"],
                            "error!",
                            null
                        );
                    }
                });
            }
            $("#msjModal").modal("hide");
            return true;
        },
        true,
        ["Activar", "Cancelar"],
        "info!",
        null
    );
}

// ✅ Función para inicializar shortcuts básicos
function inicializarShortcutsBasicos() {
    $(document).on("keydown", function (e) {
        // Ctrl + A para activar ofertas
        if (e.ctrlKey && e.key === "a") {
            e.preventDefault();
            activarOfertas();
        }

        // Escape para cancelar
        if (e.key === "Escape") {
            confirmarCancelacion();
        }
    });
}

// ✅ Función para cargar canales al inicializar (optimizada)
function cargarCanales() {
    AbrirWaiting("Cargando canales...");

    // Añadir estilo para filas seleccionadas si no existe
    if ($("style#canal-row-style").length === 0) {
        $("<style>")
            .attr("id", "canal-row-style")
            .prop("type", "text/css")
            .html(`
                .selected-row {
                    background-color: #e9f5ff !important;
                    border-left: 3px solid #0d6efd;
                }
            `)
            .appendTo("head");
    }

    PostGenHtml({}, buscarCanalesUrl, function (obj) {
        CerrarWaiting();
        $("#gridCanales").html(obj);
        
        // Ocultar checkboxes en canales
        ocultarElementosSeleccionCanales();
        
        // Configurar eventos de selección
        configurarEventosGridCanales();
    }, function (error) {
        CerrarWaiting();
        ControlaMensajeError("Error al cargar canales: " + (error.message || "Error desconocido"));
    });
}

// ✅ OPTIMIZADA: Función para ocultar elementos de selección múltiple de canales
function ocultarElementosSeleccionCanales() {
    // Ocultar checkbox principal "Seleccionar todos"
    $("#checkAllCanales").parent().css("display", "none");
    
    // Ocultar checkboxes individuales
    $(".check-canal").parent().css("display", "none");
    
    // Ocultar botón de limpiar selección si existe
    $("#btnLimpiarSeleccion").css("display", "none");
    
    // Ocultar información de selección múltiple
    $("#canalesSeleccionados").parent().css("display", "none");
    
    // Ocultar panel informativo de selección de canales si existe
    $("#infoSeleccionCanales").css("display", "none");
}

// ✅ Configuración de eventos para el grid de canales (optimizada)
function configurarEventosGridCanales() {
    // Solo configurar los botones de selección de canal individual
    $(".btn-seleccionar-canal").off("click").on("click", function () {
        var button = $(this);
        var fila = button.closest("tr");
        var admId = button.data("adm-id");
        var lpId = button.data("lp-id");
        var canal = button.data("canal");
        
        // Deseleccionar todas las filas y seleccionar solo la actual
        $("#tbGridCanales tr").removeClass("selected-row");
        fila.addClass("selected-row");
        
        // Recargar ofertas sin activar con estos parámetros
        cargarOfertasSinActivar(admId, lpId, 1);
        
        // Mostrar mensaje de selección de canal
        ControlaMensajeInfo(`Mostrando ofertas del canal: ${canal}`);
    });
}

// ✅ Función para inicializar campos de fecha
function inicializarCamposFecha() {
    // Obtener fecha actual (hoy)
    var fechaActual = obtenerFechaActualNormalizada();
    
    // Calcular fecha 30 días después para fecha hasta
    var fechaHasta = new Date(fechaActual);
    fechaHasta.setDate(fechaHasta.getDate() + 30 - 1);
    
    // Formatear fechas para inputs HTML (YYYY-MM-DD)
    var fechaDesdeFormatted = formatearFechaParaInput(fechaActual);
    var fechaHastaFormatted = formatearFechaParaInput(fechaHasta);
    
    // Establecer valores en los campos de fecha
    $("#txtFechaDesde").val(fechaDesdeFormatted);
    $("#txtFechaHasta").val(fechaHastaFormatted);
    
    // Validar rango de fechas
    setTimeout(validarRangoFechas, 300);
}

// ✅ Función para obtener fecha actual normalizada (sin hora)
function obtenerFechaActualNormalizada() {
    const hoy = new Date();
    return new Date(hoy.getFullYear(), hoy.getMonth(), hoy.getDate());
}

// ✅ Función para formatear fecha en formato YYYY-MM-DD para inputs
function formatearFechaParaInput(fecha) {
    if (!fecha || !(fecha instanceof Date) || isNaN(fecha.getTime())) return '';
    
    var año = fecha.getFullYear();
    var mes = (fecha.getMonth() + 1).toString().padStart(2, '0');
    var dia = fecha.getDate().toString().padStart(2, '0');
    
    return `${año}-${mes}-${dia}`;
}

// ✅ Función para validar rango de fechas
function validarRangoFechas() {
    var fechaDesdeStr = $("#txtFechaDesde").val();
    var fechaHastaStr = $("#txtFechaHasta").val();
    
    if (!fechaDesdeStr || !fechaHastaStr) return;
    
    var fechaDesde = parsearFechaSegura(fechaDesdeStr);
    var fechaHasta = parsearFechaSegura(fechaHastaStr);
    var fechaActual = obtenerFechaActualNormalizada();
    
    var isValid = true;
    var mensajeError = "";
    
    // Validar fechas
    if (!fechaDesde || !fechaHasta || isNaN(fechaDesde.getTime()) || isNaN(fechaHasta.getTime())) {
        isValid = false;
        mensajeError = "Las fechas especificadas no son válidas";
    } 
    // Validar que Desde <= Hasta
    else if (fechaDesde > fechaHasta) {
        isValid = false;
        mensajeError = "La fecha de inicio debe ser menor o igual a la fecha de fin";
    }
    // Validar que Desde >= fecha actual
    else if (fechaDesde < fechaActual) {
        isValid = false;
        mensajeError = "La fecha de inicio no puede ser anterior a la fecha actual";
    }
    
    // Aplicar validación visual
    if (!isValid) {
        $("#txtFechaDesde, #txtFechaHasta").addClass("is-invalid");
        if ($("#fechasError").length === 0) {
            $("#txtFechaHasta").after(`<div id="fechasError" class="invalid-feedback">${mensajeError}</div>`);
        } else {
            $("#fechasError").text(mensajeError);
        }
        return false;
    } else {
        $("#txtFechaDesde, #txtFechaHasta").removeClass("is-invalid");
        $("#fechasError").remove();
        
        // Mostrar información sobre el período
        var diferenciaTiempo = fechaHasta.getTime() - fechaDesde.getTime();
        var dias = Math.floor(diferenciaTiempo / (1000 * 60 * 60 * 24)) + 1;
        
        if ($("#infoPeriodo").length === 0) {
            $("#txtFechaHasta").after(`<div id="infoPeriodo" class="text-muted small mt-2">
                <i class="bx bx-calendar"></i> Período: ${dias} día(s)
            </div>`);
        } else {
            $("#infoPeriodo").html(`<i class="bx bx-calendar"></i> Período: ${dias} día(s)`);
        }
        return true;
    }
}

// ✅ Función para parsear fechas de forma segura
function parsearFechaSegura(fechaString) {
    if (!fechaString) return null;
    
    try {
        // Manejar formatos YYYY-MM-DD y DD/MM/YYYY
        if (fechaString.includes('-')) {
            const partes = fechaString.split('-');
            if (partes.length === 3) {
                return new Date(parseInt(partes[0]), parseInt(partes[1]) - 1, parseInt(partes[2]));
            }
        } else if (fechaString.includes('/')) {
            const partes = fechaString.split('/');
            if (partes.length === 3) {
                return new Date(parseInt(partes[2]), parseInt(partes[1]) - 1, parseInt(partes[0]));
            }
        }
        return new Date(fechaString);
    } catch (error) {
        console.error("Error al parsear fecha:", error);
        return null;
    }
}

// ✅ Función para activar ofertas (optimizada - sin verificar canales)
function activarOfertas() {       
    // Verificar ofertas seleccionadas
    var ofertasSeleccionadas = obtenerOfertasSeleccionadas();
    if (ofertasSeleccionadas.length === 0) {
        AbrirMensaje(
            "ADVERTENCIA",
            "Debe seleccionar al menos una oferta para activar",
            function() {
                $("#msjModal").modal("hide");
                return true;
            },
            false,
            ["Aceptar"],
            "warn!",
            null
        );
        return;
    }
    
    // Mostrar mensaje de confirmación
    var mensaje = generarMensajeConfirmacionActivacion(ofertasSeleccionadas);
    
    AbrirMensaje(
        "CONFIRMAR ACTIVACIÓN DE OFERTAS",
        mensaje,
        function (resp) {
            if (resp === "SI") {
                procesarActivacionOfertas(ofertasSeleccionadas);
            }
            $("#msjModal").modal("hide");
            return true;
        },
        true,
        ["Activar Ofertas", "Cancelar"],
        "info!",
        null
    );
}

// ✅ OPTIMIZADA: Función para obtener ofertas seleccionadas con descripción
function obtenerOfertasSeleccionadas() {
    var ofertas = [];
    
    $(".check-oferta:checked").each(function() {
        var checkbox = $(this);
        var pId = checkbox.data("p-id");
        
        // Obtener descripción del producto desde la fila correspondiente
        var descripcion = "";
        try {
            var fila = checkbox.closest("tr");
            descripcion = fila.find('td:eq(2)').text().trim();
        } catch (e) {
            console.warn("No se pudo obtener la descripción del producto:", e);
        }
        
        var oferta = {
            pId: pId,
            admId: checkbox.data("adm-id"),
            plId: checkbox.data("pl-id"),
            descripcion: descripcion
        };
        
        ofertas.push(oferta);
    });
    
    return ofertas;
}

// ✅ OPTIMIZADA: Función para generar mensaje de confirmación con descripción
function generarMensajeConfirmacionActivacion(ofertasSeleccionadas) {
    var mensaje = `¿Desea activar ${ofertasSeleccionadas.length} oferta(s) seleccionada(s)?<br><br>`;
    
    // Sección de ofertas
    mensaje += '<div class="text-start"><strong>📋 Ofertas Seleccionadas:</strong><br><small>';
    mensaje += `Total: ${ofertasSeleccionadas.length} oferta(s)`;
    
    // Mostrar primeras 3 ofertas si hay más de una
    if (ofertasSeleccionadas.length > 1) {
        mensaje += '<br>Ejemplos:';
        var maxOfertas = Math.min(3, ofertasSeleccionadas.length);
        for (var i = 0; i < maxOfertas; i++) {
            mensaje += `<br>- <strong>${ofertasSeleccionadas[i].pId}</strong>`;
            if (ofertasSeleccionadas[i].descripcion) {
                mensaje += ` - ${ofertasSeleccionadas[i].descripcion}`;
            }
        }
        
        if (ofertasSeleccionadas.length > maxOfertas) {
            mensaje += `<br>... y ${ofertasSeleccionadas.length - maxOfertas} más`;
        }
    } else if (ofertasSeleccionadas.length === 1) {
        // Si solo hay una oferta, mostrarla con más detalle
        mensaje += `<br>Producto: <strong>${ofertasSeleccionadas[0].pId}</strong>`;
        if (ofertasSeleccionadas[0].descripcion) {
            mensaje += ` - ${ofertasSeleccionadas[0].descripcion}`;
        }
    }
    mensaje += '</small></div><br>';
    
    // Fechas si existen
    if ($("#txtFechaDesde").length && $("#txtFechaHasta").length) {
        var fechaDesde = $("#txtFechaDesde").val();
        var fechaHasta = $("#txtFechaHasta").val();
        
        mensaje += '<div class="text-start"><strong>📅 Período de activación:</strong><br><small>';
        mensaje += `Del ${formatearFechaVisual(fechaDesde)} al ${formatearFechaVisual(fechaHasta)}<br>`;
        mensaje += '</small></div><br>';
    }
    
    mensaje += '<div class="alert alert-info">Esta acción activará las ofertas seleccionadas.</div>';
    
    return mensaje;
}

// ✅ NUEVA: Función simplificada para procesar activación de ofertas
function procesarActivacionOfertas(ofertasSeleccionadas) {
    AbrirWaiting("Activando ofertas...");
    
    // Obtener IDs para el formato que espera el servidor
    var ids = ofertasSeleccionadas.map(o => o.pId);
    
    // Usar el primer elemento para determinar admId y lp_id
    var admId = ofertasSeleccionadas[0].admId || "0000";
    var lp_id = ofertasSeleccionadas[0].plId || "001";
    
    $.ajax({
        url: activarOfertaUrl,
        type: "POST",
        data: {
            ids: ids,
            admId: admId,
            lp_id: lp_id
        },
        success: function(response) {
            CerrarWaiting();
            
            if (response.error) {
                AbrirMensaje(
                    "ERROR",
                    response.msg || "Error al activar ofertas",
                    function() {
                        $("#msjModal").modal("hide");
                        return true;
                    },
                    false,
                    ["Aceptar"],
                    "error!",
                    null
                );
                return;
            }
            
            if (response.warn) {
                AbrirMensaje(
                    "ADVERTENCIA",
                    response.msg || "Advertencia al activar ofertas",
                    function() {
                        $("#msjModal").modal("hide");
                        return true;
                    },
                    false,
                    ["Aceptar"],
                    "warn!",
                    null
                );
                return;
            }
            
            // Mensaje de éxito y recarga de la grilla
            AbrirMensaje(
                "OPERACIÓN EXITOSA",
                response.msg || `${ids.length} oferta(s) activada(s) correctamente`,
                function() {
                    // Recargar el grid
                    cargarOfertasSinActivar(admId, lp_id);
                    $("#msjModal").modal("hide");
                    return true;
                },
                false,
                ["Aceptar"],
                "success!",
                null
            );
        },
        error: function(xhr, status, error) {
            CerrarWaiting();
            console.error("Error en solicitud:", error);
            
            AbrirMensaje(
                "ERROR DE COMUNICACIÓN",
                "Error de comunicación: " + (xhr.responseText || error || "Error desconocido"),
                function() {
                    $("#msjModal").modal("hide");
                    return true;
                },
                false,
                ["Aceptar"],
                "error!",
                null
            );
        }
    });
}

// ✅ NUEVA: Función para verificar y manejar ofertas vencidas
function verificarOfertasVencidas() {
    // Buscar spans con badge de vigencia que tengan title="Oferta vencida"
    var ofertasVencidas = $("span.badge[title='Oferta vencida']");
    var hayOfertasVencidas = ofertasVencidas.length > 0;
    
    // Activar o desactivar el botón de activar vencimiento
    $("#btnActivarVencimiento").prop("disabled", !hayOfertasVencidas);
    
    // Si hay ofertas vencidas, añadir un contador al botón
    if (hayOfertasVencidas) {
        // Verificar si ya existe un badge en el botón
        if ($("#btnActivarVencimiento .badge-counter").length === 0) {
            // Añadir badge con contador
            $("#btnActivarVencimiento").append(
                `<span class="badge badge-counter bg-danger rounded-pill ms-2">${ofertasVencidas.length}</span>`
            );
        } else {
            // Actualizar contador existente
            $("#btnActivarVencimiento .badge-counter").text(ofertasVencidas.length);
        }
    } else {
        // Quitar badge si no hay ofertas vencidas
        $("#btnActivarVencimiento .badge-counter").remove();
    }
}

// ✅ NUEVA: Función para activar ofertas vencidas
function activarOfertasVencidas() {
    // Buscar todas las ofertas con badge de vencimiento
    var ofertasVencidas = $("span.badge[title='Oferta vencida']");
    
    if (ofertasVencidas.length === 0) {
        ControlaMensajeInfo("No hay ofertas vencidas para activar");
        return;
    }
    
    // Obtener el canal actualmente seleccionado (fila con clase selected-row)
    var filaSeleccionada = $("#tbGridCanales tr.selected-row");
    var admId = "0000";
    var lpId = "001";
    
    // Si hay un canal seleccionado, usar sus datos
    if (filaSeleccionada.length) {
        var boton = filaSeleccionada.find(".btn-seleccionar-canal");
        if (boton.length) {
            admId = boton.data("adm-id");
            lpId = boton.data("lp-id");
        }
    }
    
    // Mostrar mensaje de confirmación
    AbrirMensaje(
        "CONFIRMAR ACTUALIZACIÓN DE OFERTAS VENCIDAS",
        `¿Está seguro que desea actualizar ${ofertasVencidas.length} oferta(s) vencida(s)?<br><br>
         <div class="alert alert-warning">
            Esta acción actualizará las fechas de las ofertas vencidas para que puedan ser activadas.
         </div>`,
        function(resp) {
            if (resp === "SI") {
                procesarActualizacionOfertasVencidas(admId, lpId);
            }
            $("#msjModal").modal("hide");
            return true;
        },
        true,
        ["Actualizar", "Cancelar"],
        "warn!",
        null
    );
}

// ✅ NUEVA: Función para procesar actualización de ofertas vencidas
function procesarActualizacionOfertasVencidas(admId, lpId) {
    AbrirWaiting("Actualizando ofertas vencidas...");
    
    $.ajax({
        url: actualizarOfertaVencidaSinActivarUrl,
        type: "POST",
        data: {
            admId: admId,
            lp_id: lpId
        },
        success: function(response) {
            CerrarWaiting();
            
            if (response.error) {
                AbrirMensaje(
                    "ERROR",
                    response.msg || "Error al actualizar ofertas vencidas",
                    function() {
                        $("#msjModal").modal("hide");
                        return true;
                    },
                    false,
                    ["Aceptar"],
                    "error!",
                    null
                );
                return;
            }
            
            if (response.warn) {
                AbrirMensaje(
                    "ADVERTENCIA",
                    response.msg || "Advertencia al actualizar ofertas vencidas",
                    function() {
                        $("#msjModal").modal("hide");
                        return true;
                    },
                    false,
                    ["Aceptar"],
                    "warn!",
                    null
                );
                return;
            }
            
            // Mensaje de éxito y recarga de la grilla
            AbrirMensaje(
                "OPERACIÓN EXITOSA",
                response.msg || "Ofertas vencidas actualizadas correctamente",
                function() {
                    // Recargar el grid con los mismos parámetros
                    cargarOfertasSinActivar(admId, lpId);
                    $("#msjModal").modal("hide");
                    return true;
                },
                false,
                ["Aceptar"],
                "success!",
                null
            );
        },
        error: function(xhr, status, error) {
            CerrarWaiting();
            console.error("Error en solicitud:", error);
            
            AbrirMensaje(
                "ERROR DE COMUNICACIÓN",
                "Error de comunicación: " + (xhr.responseText || error || "Error desconocido"),
                function() {
                    $("#msjModal").modal("hide");
                    return true;
                },
                false,
                ["Aceptar"],
                "error!",
                null
            );
        }
    });
}