/**
 * Script para manejo de activación de ofertas
 * Comparte funcionalidad con ofertas.js para selección y gestión de canales
 */

// Variables de estado para gestión de selección
var modoSeleccionCanal = "ninguno"; // "individual", "multiple", "ninguno"
var canalIndividualSeleccionado = null;

// ✅ Inicialización del módulo
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
    $("#btnActivarOfertas").on("click", function() {
        activarOfertas();
    });
    
    $("#btnCancelaActivacion").on("click", function() {
        confirmarCancelacion();
    });

    console.log("✅ ofertaActivar.js listo");
});

// ✅ NUEVA: Función para cargar ofertas sin activar
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
        type: "POST", // Cambiado de GET a POST para coincidir con [HttpPost]
        data: datosPost, // Enviar datos como form-data
        success: function(response) {
            CerrarWaiting();
            $("#gridOfertaNoActivas").html(response);
            
            // Configurar eventos para la grilla de ofertas
            configurarEventosGridOfertasSinActivar();
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
        var plId = $(this).data("pl-id");
        
        activarOfertaIndividual(pId, admId, plId);
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

// ✅ NUEVA: Función para activar oferta individual
function activarOfertaIndividual(pId, admId, plId) {
    if (!pId || !admId || !plId) {
        ControlaMensajeWarning("Faltan datos para activar la oferta");
        return;
    }
    
    // Verificar si hay canales seleccionados
    var canalesInfo = obtenerCanalesSeleccionados();
    if (canalesInfo.canales.length === 0) {
        ControlaMensajeWarning("Debe seleccionar al menos un canal antes de activar la oferta");
        return;
    }
    
    // Mostrar mensaje de confirmación
    var mensaje = `¿Desea activar la oferta del producto <strong>${pId}</strong> en los canales seleccionados?`;
    
    AbrirMensaje(
        "CONFIRMAR ACTIVACIÓN DE OFERTA",
        mensaje,
        function(resp) {
            if (resp === "SI") {
                // Aquí iría el código para activar la oferta individual
                // Esto dependerá de cómo esté implementado en el backend
                ControlaMensajeInfo("Funcionalidad en desarrollo");
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

// ✅ Función para cargar canales al inicializar
function cargarCanales() {
    AbrirWaiting("Cargando canales...");

    PostGenHtml({}, buscarCanalesUrl, function (obj) {
        CerrarWaiting();
        $("#gridCanales").html(obj);
        configurarEventosGridCanales();
    }, function (error) {
        CerrarWaiting();
        ControlaMensajeError("Error al cargar canales: " + (error.message || "Error desconocido"));
    });
}

// ✅ Configuración de eventos para el grid de canales
function configurarEventosGridCanales() {
    // Checkbox "Seleccionar todos" para canales
    $("#checkAllCanales").off("change").on("change", function () {
        var isChecked = $(this).is(":checked");
        $(".check-canal").prop("checked", isChecked);

        if (isChecked) {
            cambiarModoSeleccion("multiple");
        } else {
            // Si se deselecciona "todos", verificar si hay alguno seleccionado
            var checkedCount = $(".check-canal:checked").length;
            if (checkedCount === 0) {
                cambiarModoSeleccion("ninguno");
            }
        }

        actualizarContadorCanales();
    });

    // Checkboxes individuales para canales
    $(".check-canal").off("change").on("change", function () {
        var totalChecks = $(".check-canal").length;
        var checkedCount = $(".check-canal:checked").length;

        $("#checkAllCanales").prop("checked", totalChecks === checkedCount);

        // Determinar modo de selección
        if (checkedCount === 0) {
            cambiarModoSeleccion("ninguno");
        } else if (checkedCount === 1 && modoSeleccionCanal !== "individual") {
            cambiarModoSeleccion("multiple");
        } else if (checkedCount > 1) {
            cambiarModoSeleccion("multiple");
        }

        actualizarContadorCanales();
    });

    // Botones de seleccionar canal individual
    $(".btn-seleccionar-canal").off("click").on("click", function () {
        var admId = $(this).data("adm-id");
        var lpId = $(this).data("lp-id");
        var canal = $(this).data("canal");
        var admNombre = $(this).data("adm-nombre");
        var lpDesc = $(this).data("lp-desc");

        seleccionarCanalIndividual(admId, lpId, canal, admNombre, lpDesc);
    });

    // Botón limpiar selección
    $("#btnLimpiarSeleccion").off("click").on("click", function () {
        limpiarSeleccionCanales();
    });
}

// ✅ Función optimizada para actualizar contador
function actualizarContadorCanales() {
    var checkedCount = $(".check-canal:checked").length;
    $("#canalesSeleccionados").text(checkedCount);

    // Mostrar/ocultar el panel según la selección
    if (checkedCount === 0 && modoSeleccionCanal !== "ninguno") {
        cambiarModoSeleccion("ninguno");
    }
}

// ✅ Función para seleccionar canal individual
function seleccionarCanalIndividual(admId, lpId, canal, admNombre, lpDesc) {
    var mensaje = `Canal: ${canal}<br>Administración: ${admNombre}<br>Lista: ${lpDesc}`;

    AbrirMensaje(
        "CONFIRMAR SELECCIÓN DE CANAL",
        `¿Desea seleccionar este canal para las ofertas?<br><br>${mensaje}`,
        function (resp) {
            if (resp === "SI") {
                // Limpiar selecciones previas para modo individual
                $(".check-canal").prop("checked", false);
                $("#checkAllCanales").prop("checked", false);

                // Seleccionar solo este canal
                $(`.check-canal[data-adm-id="${admId}"][data-lp-id="${lpId}"]`).prop("checked", true);

                // Cambiar a modo individual y guardar datos
                canalIndividualSeleccionado = {
                    admId: admId,
                    lpId: lpId,
                    canal: canal,
                    admNombre: admNombre,
                    lpDesc: lpDesc
                };

                cambiarModoSeleccion("individual");
                actualizarContadorCanales();

                ControlaMensajeSuccess(`Canal "${canal}" seleccionado correctamente`);
            }
            $("#msjModal").modal("hide");
            return true;
        },
        true,
        ["Seleccionar", "Cancelar"],
        "info!",
        null
    );
}

// ✅ Función para cambiar modo de selección
function cambiarModoSeleccion(nuevoModo) {
    modoSeleccionCanal = nuevoModo;

    var infoPanel = $("#infoSeleccionCanales");
    var modoTexto = $("#modoSeleccion");
    var datosCanal = $("#datosCanal");

    switch (nuevoModo) {
        case "individual":
            infoPanel.show();
            modoTexto.text("Individual").removeClass().addClass("badge bg-info");

            if (canalIndividualSeleccionado) {
                $("#canalSeleccionado").text(canalIndividualSeleccionado.canal);
                $("#admSeleccionada").text(canalIndividualSeleccionado.admNombre);
                $("#lpSeleccionada").text(canalIndividualSeleccionado.lpDesc);
                datosCanal.show();
            }
            break;

        case "multiple":
            infoPanel.show();
            modoTexto.text("Múltiple").removeClass().addClass("badge bg-warning");
            datosCanal.hide();
            canalIndividualSeleccionado = null;
            break;

        case "ninguno":
        default:
            infoPanel.hide();
            datosCanal.hide();
            canalIndividualSeleccionado = null;
            break;
    }
}

// ✅ Función para limpiar selección de canales
function limpiarSeleccionCanales() {
    AbrirMensaje(
        "CONFIRMAR LIMPIEZA",
        "¿Está seguro de limpiar toda la selección de canales?",
        function (resp) {
            if (resp === "SI") {
                // Limpiar todos los checkboxes
                $(".check-canal").prop("checked", false);
                $("#checkAllCanales").prop("checked", false);

                // Resetear modo y datos
                canalIndividualSeleccionado = null;
                cambiarModoSeleccion("ninguno");
                actualizarContadorCanales();

                ControlaMensajeInfo("Selección de canales limpiada correctamente");
            }
            $("#msjModal").modal("hide");
            return true;
        },
        true,
        ["Limpiar", "Cancelar"],
        "warn!",
        null
    );
}

// ✅ Función para obtener canales seleccionados
function obtenerCanalesSeleccionados() {
    var canales = [];

    $(".check-canal:checked").each(function () {
        var canal = {
            admId: $(this).data("adm-id"),
            lpId: $(this).data("lp-id"),
            canal: $(this).data("canal"),
            admNombre: $(this).data("adm-nombre"),
            lpDesc: $(this).data("lp-desc")
        };
        canales.push(canal);
    });

    return {
        modo: modoSeleccionCanal,
        canales: canales,
        individual: canalIndividualSeleccionado
    };
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

// ✅ Función para activar ofertas
function activarOfertas() {
    // Verificar canales seleccionados
    var canalesInfo = obtenerCanalesSeleccionados();
    if (canalesInfo.canales.length === 0) {
        ControlaMensajeWarning("Debe seleccionar al menos un canal antes de activar las ofertas");
        return;
    }
    
    // Verificar ofertas seleccionadas
    var ofertasSeleccionadas = obtenerOfertasSeleccionadas();
    if (ofertasSeleccionadas.length === 0) {
        ControlaMensajeWarning("Debe seleccionar al menos una oferta para activar");
        return;
    }
    
    // Verificar fechas válidas si existen los campos
    if ($("#txtFechaDesde").length && $("#txtFechaHasta").length) {
        if (!validarRangoFechas()) {
            ControlaMensajeWarning("Las fechas seleccionadas no son válidas");
            return;
        }
    }
    
    // Mostrar mensaje de confirmación
    var mensaje = generarMensajeConfirmacionActivacion(canalesInfo, ofertasSeleccionadas);
    
    AbrirMensaje(
        "CONFIRMAR ACTIVACIÓN DE OFERTAS",
        mensaje,
        function (resp) {
            if (resp === "SI") {
                procesarActivacionOfertasMultiples(canalesInfo, ofertasSeleccionadas);
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

// ✅ NUEVA: Función para obtener ofertas seleccionadas
function obtenerOfertasSeleccionadas() {
    var ofertas = [];
    
    $(".check-oferta:checked").each(function() {
        var oferta = {
            pId: $(this).data("p-id"),
            admId: $(this).data("adm-id"),
            plId: $(this).data("pl-id")
        };
        ofertas.push(oferta);
    });
    
    return ofertas;
}

// ✅ MODIFICADA: Función para generar mensaje de confirmación (incluye ofertas)
function generarMensajeConfirmacionActivacion(canalesInfo, ofertasSeleccionadas) {
    var mensaje = `¿Desea activar ${ofertasSeleccionadas.length} oferta(s) para los canales seleccionados?<br><br>`;
    
    // Sección de ofertas
    mensaje += '<div class="text-start"><strong>📋 Ofertas Seleccionadas:</strong><br><small>';
    mensaje += `Total: ${ofertasSeleccionadas.length} oferta(s)`;
    
    // Mostrar primeras 3 ofertas si hay más de una
    if (ofertasSeleccionadas.length > 1) {
        mensaje += '<br>Ejemplos:';
        var maxOfertas = Math.min(3, ofertasSeleccionadas.length);
        for (var i = 0; i < maxOfertas; i++) {
            mensaje += `<br>- Producto ID: ${ofertasSeleccionadas[i].pId}`;
        }
        
        if (ofertasSeleccionadas.length > maxOfertas) {
            mensaje += `<br>... y ${ofertasSeleccionadas.length - maxOfertas} más`;
        }
    }
    mensaje += '</small></div><br>';
    
    // Sección de canales
    mensaje += generarSeccionCanales(canalesInfo);
    
    // Fechas si existen
    if ($("#txtFechaDesde").length && $("#txtFechaHasta").length) {
        var fechaDesde = $("#txtFechaDesde").val();
        var fechaHasta = $("#txtFechaHasta").val();
        
        mensaje += '<div class="text-start"><strong>📅 Período de activación:</strong><br><small>';
        mensaje += `Del ${formatearFechaVisual(fechaDesde)} al ${formatearFechaVisual(fechaHasta)}<br>`;
        mensaje += '</small></div><br>';
    }
    
    mensaje += '<div class="alert alert-info">Esta acción activará las ofertas seleccionadas en los canales especificados.</div>';
    
    return mensaje;
}

// ✅ NUEVA: Función para procesar activación de ofertas múltiples
function procesarActivacionOfertasMultiples(canalesInfo, ofertasSeleccionadas) {
    AbrirWaiting("Activando ofertas...");
    
    var datosActivacion = {
        canales: canalesInfo.canales,
        canalIndividual: canalesInfo.individual,
        modoSeleccion: canalesInfo.modo,
        ofertas: ofertasSeleccionadas,
        fechaDesde: $("#txtFechaDesde").val() || null,
        fechaHasta: $("#txtFechaHasta").val() || null
    };
    
    var jsonData = JSON.stringify(datosActivacion);
    
    $.ajax({
        url: activarOfertasUrl,
        type: "POST",
        contentType: "application/json",
        data: jsonData,
        success: function(response) {
            CerrarWaiting();
            
            if (response.error) {
                ControlaMensajeError(response.msg || "Error al activar ofertas");
                return;
            }
            
            if (response.warn) {
                ControlaMensajeWarning(response.msg || "Advertencia en la activación");
                return;
            }
            
            // Mensaje de éxito con redirección
            ControlaMensajeSuccess(response.msg || `${ofertasSeleccionadas.length} ofertas activadas correctamente`);
            
            // Redireccionar después de 2 segundos
            setTimeout(function() {
                window.location.href = homeOfertaUrl || "/Precios/Ofertas";
            }, 2000);
        },
        error: function(xhr, status, error) {
            CerrarWaiting();
            console.error("Error en solicitud: ", xhr.responseText);
            ControlaMensajeError("Error de comunicación: " + (xhr.responseText || error || "Error desconocido"));
        }
    });
}

// ✅ Función para confirmar cancelación
function confirmarCancelacion() {
    AbrirMensaje(
        "CONFIRMAR CANCELACIÓN",
        "¿Está seguro de cancelar la activación de ofertas?",
        function(resp) {
            if (resp === "SI") {
                window.location.href = homeOfertaUrl || "/Precios/Ofertas";
            }
            $("#msjModal").modal("hide");
            return true;
        },
        true,
        ["Cancelar Activación", "Continuar Editando"],
        "warn!",
        null
    );
}