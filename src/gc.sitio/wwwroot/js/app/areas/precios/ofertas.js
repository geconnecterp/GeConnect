/**
 * Script para manejo de ofertas
 * Versión optimizada con integración de búsqueda avanzada
 */

// Variables globales
var modoSeleccionCanal = "ninguno";
var canalIndividualSeleccionado = null;

// ✅ NUEVO: Variable para validación de productos existentes
var productosEnGridOfertas = [];

// ✅ Inicialización unificada del módulo
$(function () {
    console.log("🚀 Iniciando ofertas.js");
    
    // Eventos principales
    $("#btnBusquedaBase").on("click", function () {
        buscarProducto();
        return true;
    });

    $("#estadoFuncion").on("change", verificaEstado);
    
    // Inicializaciones
    try { 
        inicializarSistemaBasico();
        inicializarCamposFecha(); 
    } catch (e) { 
        console.warn("Error en inicialización:", e.message); 
    }
    
    try { cargarCanales(); } catch (e) { console.error("Error al cargar canales:", e); }

    // Delegación de eventos para autocomplete en el modal
    $(document).on("autocompleteselect", "#busquedaModal #Rel01", function (event, ui) {
        setTimeout(function () {
            cargarFamiliasParaBusquedaAvanzada(ui.item.id);
        }, 100);
    });

    // ✅ NUEVO: Configurar búsqueda avanzada para ofertas
    configurarBusquedaAvanzadaOfertas();

    // Limpieza inicial
    $("#Busqueda").val("");
    $("#estadoFuncion").val(false);
    
    console.log("✅ ofertas.js listo");
});

// ✅ NUEVA: Configuración de búsqueda avanzada para ofertas
function configurarBusquedaAvanzadaOfertas() {
    // Configurar cuando se abre el modal de búsqueda
    $("#busquedaModal").on("show.bs.modal", function () {
        if (typeof configurarDestinoBusquedaProductos === 'function') {
            configurarDestinoBusquedaProductos(
                "ofertas",
                agregarProductosAlGridOfertas,
                obtenerProductosExistentesIdsOfertas
            );
        }
    });
}

// ✅ NUEVA: Obtener IDs de productos ya existentes en el grid
function obtenerProductosExistentesIdsOfertas() {
    const productosIds = [];
    
    $('#tbGridProductosOferta tbody tr[data-producto-id]').each(function () {
        const pId = $(this).data('producto-id');
        if (pId) {
            productosIds.push(pId);
        }
    });
    
    console.log(`📦 Productos existentes en grid: ${productosIds.length}`);
    return productosIds;
}

// ✅ NUEVA: Agregar productos al grid de ofertas (callback principal)
function agregarProductosAlGridOfertas(productos) {
    if (!Array.isArray(productos) || productos.length === 0) {
        console.warn("⚠️ No hay productos para agregar");
        return;
    }

    console.log(`📥 Agregando ${productos.length} productos al grid de ofertas`);
    
    AbrirWaiting("Agregando productos a ofertas...");

    try {
        // Convertir productos al formato esperado por el servidor
        const productosParaEnvio = productos.map(producto => ({
            P_id: producto.p_id,
            P_desc: producto.p_desc || '',
            P_pcosto: parseFloat(producto.p_pcosto || 0),
            P_pvta: parseFloat(producto.p_vta || 0),
            P_pvta_oferta: parseFloat(producto.p_vta || 0),
            P_id_barrado: producto.p_id_barrado || '',
            P_id_prov: producto.cta_id || '',
            Pg_id: producto.pg_id || '',
            Pg_desc: producto.pg_desc || '',
            P_activo: producto.p_activo || 'S'
        }));

        // Llamada al servidor para renderizar el grid actualizado
        $.ajax({
            url: presentarProductosOfertaMultipleUrl || presentarProductoOfertaUrl,
            type: 'POST',
            contentType: 'application/json',
            data: JSON.stringify({ productos: productosParaEnvio }),
            success: function (response) {
                CerrarWaiting();
                
                if (response.error) {
                    ControlaMensajeError(response.msg || "Error al agregar productos");
                    return;
                }

                // Actualizar el grid con el HTML recibido
                $("#gridProductoOferta").html(response.html || response);
                configurarEventosGridOferta();
                
                // Actualizar lista de productos en memoria
                actualizarListaProductosEnGrid();
                
                const mensaje = productos.length === 1
                    ? `Producto "${productos[0].p_desc}" agregado correctamente`
                    : `${productos.length} productos agregados correctamente`;
                
                ControlaMensajeSuccess(mensaje);
            },
            error: function (xhr, status, error) {
                CerrarWaiting();
                console.error("❌ Error al agregar productos:", error);
                ControlaMensajeError("Error al agregar productos: " + (xhr.responseJSON?.msg || error));
            }
        });

    } catch (error) {
        CerrarWaiting();
        console.error("❌ Error al procesar productos:", error);
        ControlaMensajeError("Error al procesar productos: " + error.message);
    }
}

// ✅ NUEVA: Procesar agregado de productos múltiples (función requerida por busquedasV02.js)
function procesarAgregarProductosMultiples() {
    console.log("🔄 Procesando agregado múltiple de productos a ofertas");
    
    AbrirWaiting("Agregando productos a ofertas...");

    try {
        // Obtener productos existentes para filtrar duplicados
        const productosExistentesIds = obtenerProductosExistentesIdsOfertas();
        
        // Filtrar productos ya existentes
        const productosFiltrados = productosSeleccionadosBusqueda.filter(producto =>
            !productosExistentesIds.includes(producto.p_id));

        const cantidadDuplicados = productosSeleccionadosBusqueda.length - productosFiltrados.length;

        if (productosFiltrados.length === 0) {
            CerrarWaiting();
            if (cantidadDuplicados > 0) {
                ControlaMensajeWarning(`Los ${cantidadDuplicados} producto(s) seleccionado(s) ya están en ofertas.`);
            } else {
                ControlaMensajeWarning("No hay productos para agregar.");
            }
            return;
        }

        // Convertir a formato para el servidor
        const productosParaEnvio = productosFiltrados.map(producto => ({
            P_id: producto.p_id,
            P_desc: producto.p_desc || '',
            P_pcosto: parseFloat(producto.p_pcosto || 0),
            P_mayorista: parseFloat(producto.p_pvta_001 || 0),
            P_minorista: parseFloat(producto.p_pvta_002 || 0),
            P_pvta: parseFloat(producto.p_pvta || 0),
            P_id_barrado: producto.p_id_barrado || '',
            P_id_prov: producto.cta_id || '',
            P_activo: producto.p_activo || 'S'
        }));

        // Enviar al servidor
        $.ajax({
            url: presentarProductosOfertaMultipleUrl || presentarProductoOfertaUrl,
            type: 'POST',
            contentType: 'application/json',
            data: JSON.stringify({ productos: productosParaEnvio }),
            success: function (response) {
                CerrarWaiting();
                
                // Cerrar modal de búsqueda
                $("#busquedaModal").modal("hide");
                
                if (response.error) {
                    ControlaMensajeError(response.msg || "Error al agregar productos");
                    return;
                }

                // Actualizar grid
                $("#gridProductoOferta").html(response.html || response);
                configurarEventosGridOferta();
                actualizarListaProductosEnGrid();

                // Limpiar selección
                if (typeof limpiarSeleccionBusqueda === 'function') {
                    limpiarSeleccionBusqueda();
                }

                // Mensaje de éxito
                let mensaje = `${productosFiltrados.length} producto(s) agregado(s) a ofertas correctamente`;
                
                if (cantidadDuplicados > 0) {
                    mensaje += `. Se omitieron ${cantidadDuplicados} duplicado(s).`;
                }
                
                ControlaMensajeSuccess(mensaje);
            },
            error: function (xhr, status, error) {
                CerrarWaiting();
                console.error("❌ Error al agregar productos:", error);
                ControlaMensajeError("Error al agregar productos: " + (xhr.responseJSON?.msg || error));
            }
        });

    } catch (error) {
        CerrarWaiting();
        console.error("❌ Error al procesar productos:", error);
        ControlaMensajeError("Error al procesar productos: " + error.message);
    }
}

// ✅ NUEVA: Actualizar lista en memoria de productos en grid
function actualizarListaProductosEnGrid() {
    productosEnGridOfertas = [];
    
    $('#tbGridProductosOferta tbody tr[data-producto-id]').each(function () {
        const pId = $(this).data('producto-id');
        if (pId) {
            productosEnGridOfertas.push(pId);
        }
    });
    
    console.log(`📊 Lista actualizada: ${productosEnGridOfertas.length} productos en grid`);
}

// ✅ Manejador global de errores
window.addEventListener('error', function(e) {
    console.error("Error en ofertas.js:", e.message);
    return false;
});

// ===== FUNCIONES DE INICIALIZACIÓN =====

function inicializarSistemaBasico() {
    inicializarShortcutsBasicos();
}

function inicializarShortcutsBasicos() {
    $(document).on("keydown", function (e) {
        if (e.ctrlKey && e.key === "s") {
            e.preventDefault();
            if (typeof guardarTodasLasOfertas === 'function') {
                guardarTodasLasOfertas();
            }
        }

        if (e.key === "Escape") {
            if (typeof limpiarFormularioOfertas === 'function') {
                limpiarFormularioOfertas();
            }
        }
    });
}

// ===== FUNCIONES DE GESTIÓN DE FECHAS =====

function parsearFechaSegura(fechaString) {
    if (!fechaString) return null;
    
    try {
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

function obtenerFechaActualNormalizada() {
    const hoy = new Date();
    return new Date(hoy.getFullYear(), hoy.getMonth(), hoy.getDate());
}

function normalizarFecha(fecha) {
    if (!fecha || !(fecha instanceof Date) || isNaN(fecha.getTime())) return null;
    return new Date(fecha.getFullYear(), fecha.getMonth(), fecha.getDate());
}

function formatearFechaParaInput(fecha) {
    if (!fecha || !(fecha instanceof Date) || isNaN(fecha.getTime())) return '';
    
    var año = fecha.getFullYear();
    var mes = (fecha.getMonth() + 1).toString().padStart(2, '0');
    var dia = fecha.getDate().toString().padStart(2, '0');
    
    return `${año}-${mes}-${dia}`;
}

function formatearFecha(fechaString) {
    if (!fechaString) return '';

    try {
        var fecha;
        if (typeof fechaString === 'string') {
            fecha = parsearFechaSegura(fechaString);
        } else if (fechaString instanceof Date) {
            fecha = fechaString;
        } else {
            return fechaString.toString();
        }

        if (!fecha || isNaN(fecha.getTime())) {
            return fechaString.toString();
        }

        return fecha.toLocaleDateString('es-AR', {
            day: '2-digit',
            month: '2-digit',
            year: 'numeric'
        });
    } catch (error) {
        console.error("Error al formatear fecha:", error);
        return fechaString.toString();
    }
}

function formatearPrecioArgentino(precio) {
    return precio.toLocaleString('es-AR', {
        style: 'currency',
        currency: 'ARS',
        minimumFractionDigits: 2,
        maximumFractionDigits: 2
    }).replace('ARS', '').trim();
}

// ===== FUNCIONES DE GESTIÓN DE UI =====

function mostrarTooltipError(elemento, mensaje) {
    if (typeof elemento.tooltip !== 'function') {
        elemento.attr('title', mensaje);
        return;
    }

    try {
        elemento.tooltip('dispose');
        elemento.tooltip({
            title: mensaje,
            placement: 'bottom',
            trigger: 'manual',
            container: 'body',
            customClass: 'tooltip-error',
            template: '<div class="tooltip tooltip-error" role="tooltip"><div class="arrow"></div><div class="tooltip-inner"></div></div>'
        });
        
        elemento.tooltip('show');
    } catch (error) {
        elemento.attr('title', mensaje);
    }
}

function ocultarTooltipError(elemento) {
    try {
        if (typeof elemento.tooltip === 'function') {
            elemento.tooltip('hide');
            elemento.tooltip('dispose');
        }
        elemento.removeAttr('title');
    } catch (error) {
        elemento.removeAttr('title');
    }
}

function ControlaMensajeSuccessConCallback(mensaje, callback) {
    ControlaMensajeSuccess(mensaje);
    
    setTimeout(function() {
        if (typeof callback === 'function') {
            callback();
        }
    }, 2000);
}

// ===== FUNCIONES DE INPUTS Y VALIDACIONES =====

function configurarInputMaskPrecios() {
    if (typeof Inputmask === 'undefined') {
        console.warn("InputMask no está disponible");
        return;
    }

    const maskConfigPrecio = {
        alias: "numeric",
        groupSeparator: ",",
        radixPoint: ".",
        autoGroup: true,
        digits: 2,
        digitsOptional: false,
        rightAlign: true,
        prefix: '',
        placeholder: "0.00",
        clearMaskOnLostFocus: false,
        showMaskOnHover: false,
        showMaskOnFocus: true,
        min: 0,
        allowMinus: false,
        unmaskAsNumber: true,
        onBeforeMask: function (value) {
            if (!value) return '0';
            return parseFloat(value.toString().replace(/[^\d.]/g, '')).toFixed(2);
        }
    };

    const maskConfigTope = {
        alias: "integer",
        min: 0,
        rightAlign: true,
        placeholder: "0",
        clearMaskOnLostFocus: false
    };

    try {
        Inputmask(maskConfigPrecio).mask('#txtPrecioOferta');
        Inputmask(maskConfigTope).mask('#txtTopeVenta');
    } catch (error) {
        console.error("Error al aplicar InputMask:", error);
    }
}

function configurarValidacionTiempoReal() {
    $("#txtPrecioOferta").on("change", function() {
        var precio = parseFloat($(this).val().replace(/\./g, '').replace(/,/g, '.') || "0");
        if (precio <= 0) {
            $(this).addClass("is-invalid");
            mostrarTooltipError($(this), "El precio debe ser mayor a cero");
        } else {
            $(this).removeClass("is-invalid");
            ocultarTooltipError($(this));
        }
    });

    $("#txtTopeVenta").on("change", function() {
        var tope = parseInt($(this).val() || "0");
        if (isNaN(tope) || tope < 0) {
            $(this).addClass("is-invalid");
            mostrarTooltipError($(this), "El tope de venta debe ser mayor o igual a cero");
        } else {
            $(this).removeClass("is-invalid");
            ocultarTooltipError($(this));
        }
    });

    $("#txtFechaDesde, #txtFechaHasta").on("change", function() {
        validarRangoFechas();
    });
}

function inicializarCamposFecha() {
    var fechaActual = obtenerFechaActualNormalizada();
    
    var fechaHasta = new Date(fechaActual);
    fechaHasta.setDate(fechaHasta.getDate() + 30 - 1);
    
    var fechaDesdeFormatted = formatearFechaParaInput(fechaActual);
    var fechaHastaFormatted = formatearFechaParaInput(fechaHasta);
    
    $("#txtFechaDesde").val(fechaDesdeFormatted);
    $("#txtFechaHasta").val(fechaHastaFormatted);
    
    try {
        $("#txtPrecioOferta, #txtTopeVenta").off("change");
        
        configurarInputMaskPrecios();
        
        $("#txtPrecioOferta").val("0");
        $("#txtTopeVenta").val("0");
        
        $("#txtPrecioOferta, #txtTopeVenta").removeClass("is-invalid");
        
        setTimeout(function() {
            configurarValidacionTiempoReal();
        }, 200);
    } catch (e) {
        console.warn("Error al inicializar valores:", e.message);
    }
    
    setTimeout(validarRangoFechas, 300);
}

function validarRangoFechas() {
    var fechaDesdeStr = $("#txtFechaDesde").val();
    var fechaHastaStr = $("#txtFechaHasta").val();
    
    if (!fechaDesdeStr || !fechaHastaStr) return;
    
    var fechaDesde = parsearFechaSegura(fechaDesdeStr);
    var fechaHasta = parsearFechaSegura(fechaHastaStr);
    var fechaActual = obtenerFechaActualNormalizada();
    
    var isValid = true;
    var mensajeError = "";
    
    if (!fechaDesde || !fechaHasta || isNaN(fechaDesde.getTime()) || isNaN(fechaHasta.getTime())) {
        isValid = false;
        mensajeError = "Las fechas especificadas no son válidas";
    } 
    else if (fechaDesde > fechaHasta) {
        isValid = false;
        mensajeError = "La fecha de inicio debe ser menor o igual a la fecha de fin";
    }
    else if (fechaDesde < fechaActual) {
        isValid = false;
        mensajeError = "La fecha de inicio no puede ser anterior a la fecha actual";
    }
    
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

// ===== FUNCIONES DE CANALES =====

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

function configurarEventosGridCanales() {
    $("#checkAllCanales").off("change").on("change", function () {
        var isChecked = $(this).is(":checked");
        $(".check-canal").prop("checked", isChecked);

        if (isChecked) {
            cambiarModoSeleccion("multiple");
        } else {
            var checkedCount = $(".check-canal:checked").length;
            if (checkedCount === 0) {
                cambiarModoSeleccion("ninguno");
            }
        }

        actualizarContadorCanales();
    });

    $(".check-canal").off("change").on("change", function () {
        var totalChecks = $(".check-canal").length;
        var checkedCount = $(".check-canal:checked").length;

        $("#checkAllCanales").prop("checked", totalChecks === checkedCount);

        if (checkedCount === 0) {
            cambiarModoSeleccion("ninguno");
        } else if (checkedCount === 1 && modoSeleccionCanal !== "individual") {
            cambiarModoSeleccion("multiple");
        } else if (checkedCount > 1) {
            cambiarModoSeleccion("multiple");
        }

        actualizarContadorCanales();
    });

    $(".btn-seleccionar-canal").off("click").on("click", function () {
        var admId = $(this).data("adm-id");
        var lpId = $(this).data("lp-id");
        var canal = $(this).data("canal");
        var admNombre = $(this).data("adm-nombre");
        var lpDesc = $(this).data("lp-desc");

        seleccionarCanalIndividual(admId, lpId, canal, admNombre, lpDesc);
    });

    $("#btnLimpiarSeleccion").off("click").on("click", function () {
        limpiarSeleccionCanales();
    });
}

function actualizarContadorCanales() {
    var checkedCount = $(".check-canal:checked").length;
    $("#canalesSeleccionados").text(checkedCount);

    if (checkedCount === 0 && modoSeleccionCanal !== "ninguno") {
        cambiarModoSeleccion("ninguno");
    }
}

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

function seleccionarCanalIndividual(admId, lpId, canal, admNombre, lpDesc) {
    var mensaje = `Canal: ${canal}<br>Administración: ${admNombre}<br>Lista: ${lpDesc}`;

    AbrirMensaje(
        "CONFIRMAR SELECCIÓN DE CANAL",
        `¿Desea seleccionar este canal para las ofertas?<br><br>${mensaje}`,
        function (resp) {
            if (resp === "SI") {
                $(".check-canal").prop("checked", false);
                $("#checkAllCanales").prop("checked", false);

                $(`.check-canal[data-adm-id="${admId}"][data-lp-id="${lpId}"]`).prop("checked", true);

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

function limpiarSeleccionCanales() {
    AbrirMensaje(
        "CONFIRMAR LIMPIEZA",
        "¿Está seguro de limpiar toda la selección de canales?",
        function (resp) {
            if (resp === "SI") {
                $(".check-canal").prop("checked", false);
                $("#checkAllCanales").prop("checked", false);

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

// ===== FUNCIONES DE OFERTAS =====

function verificaEstado(e) {
    FunctionCallback = null;
    var res = $("#estadoFuncion").val();
    CerrarWaiting();

    if (res === "true") {
        var prod = productoBase;

        if (prod && prod.p_id) {
            presentarProductoEnOferta(prod);
        }

        $("#Busqueda").val("");
        $("#estadoFuncion").val(false);
    }
    return true;
}

function presentarProductoEnOferta(producto) {
    AbrirWaiting("Agregando producto a ofertas...");

    var datos = {
        P_id: producto.p_id,
        P_desc: producto.p_desc,
        P_pcosto: producto.p_pcosto || "0",
        P_pvta: producto.p_vta || "0",
        P_pvta_oferta: producto.p_vta_oferta || "0",
        P_id_barrado: producto.p_id_barrado || "",
        P_id_prov: producto.p_id_prov || "",
        Pg_id: producto.pg_id || "",
        Pg_desc: producto.pg_desc || "",
        P_activo: producto.p_activo || "N"
    };

    PostGenHtml(datos, presentarProductoOfertaUrl, function (obj) {
        CerrarWaiting();
        $("#gridProductoOferta").html(obj);
        configurarEventosGridOferta();
        actualizarListaProductosEnGrid();
        ControlaMensajeSuccess(`Producto "${producto.p_desc}" agregado a ofertas correctamente`);
    }, function (error) {
        CerrarWaiting();
        ControlaMensajeError("Error al agregar producto a ofertas: " + (error.message || "Error desconocido"));
    });
}

function eliminarProductoDelGrid(row, productDesc) {
    row.fadeOut(300, function () {
        $(this).remove();

        if ($("#tbGridProductosOferta tbody tr[data-producto-id]").length === 0) {
            $("#gridProductoOferta").html(`
                <div class="text-center text-muted py-4">
                    <i class="bx bx-info-circle me-2"></i>
                    No hay productos seleccionados para ofertas
                </div>
            `);
        }

        actualizarListaProductosEnGrid();
        ControlaMensajeInfo(`Producto "${productDesc}" eliminado de ofertas`);
    });
}

function configurarEventosGridOferta() {
    $(".btn-remover-oferta").off("click").on("click", function() {
        var productId = $(this).data("p-id");
        var row = $(this).closest("tr");
        var productDesc = row.find("td:nth-child(2)").text().trim();

        AbrirMensaje(
            "CONFIRMAR ELIMINACIÓN",
            `¿Está seguro de eliminar "${productDesc}" de las ofertas?`,
            function(resp) {
                if (resp === "SI") {
                    eliminarProductoDelGrid(row, productDesc);
                }
                $("#msjModal").modal("hide");
                return true;
            },
            true,
            ["Eliminar", "Cancelar"],
            "warn!",
            null
        );
    });

    $(".btn-estado-oferta").off("click").on("click", function() {
        var productId = $(this).data("p-id");
        var productDesc = $(this).closest("tr").find("td:nth-child(2)").text().trim();
        mostrarEstadoOferta(productId, productDesc);
    });

    $("#btnGuardarOfertas").off("click").on("click", function() {
        guardarTodasLasOfertas();
    });

    $("#btnCancelaOferta").off("click").on("click", function() {
        AbrirMensaje(
            "CONFIRMAR CANCELACIÓN DE LA OFERTA",
            `¿Está seguro de desea CANCELAR la(s) oferta(s)?`,
            function(resp) {
                if (resp === "SI") {
                    window.location.href = homeOfertaUrl;
                }
                $("#msjModal").modal("hide");
                return true;
            },
            true,
            ["Continuar", "Cancelar"],
            "warn!",
            null
        );
    });
}

function obtenerInformacionOfertaDefinida() {
    try {
        var precioTexto = $("#txtPrecioOferta").val() || "0";
        var precioLimpio = precioTexto.replace(/[^\d.-]/g, '');
        
        if (precioLimpio.endsWith('.') || precioLimpio.endsWith(',')) {
            precioLimpio = precioLimpio.slice(0, -1);
        }
        
        var precioOferta = parseFloat(precioLimpio);
        var fechaDesde = $("#txtFechaDesde").val();
        var fechaHasta = $("#txtFechaHasta").val();
        var topeVenta = parseInt($("#txtTopeVenta").val().replace(/[^\d]/g, '') || "0");
        
        if (isNaN(precioOferta) || precioOferta <= 0) {
            return { valido: false, error: "El precio de la oferta debe ser mayor a cero" };
        }

        if (isNaN(topeVenta) || topeVenta < 0) {
            return { valido: false, error: "El tope de venta debe ser mayor o igual a cero" };
        }

        if (!fechaDesde || !fechaHasta) {
            return { valido: false, error: "Debe especificar las fechas de inicio y fin de la oferta" };
        }

        var fechaDesdeObj = parsearFechaSegura(fechaDesde);
        var fechaHastaObj = parsearFechaSegura(fechaHasta);
        var fechaActual = obtenerFechaActualNormalizada();

        if (!fechaDesdeObj || !fechaHastaObj || isNaN(fechaDesdeObj.getTime()) || isNaN(fechaHastaObj.getTime())) {
            return { valido: false, error: "Las fechas especificadas no son válidas" };
        }

        if (fechaDesdeObj > fechaHastaObj) {
            return { valido: false, error: "La fecha de inicio debe ser menor o igual a la fecha de fin" };
        }

        if (fechaDesdeObj < fechaActual) {
            return { valido: false, error: "La fecha de inicio no puede ser anterior a la fecha actual" };
        }

        var fechaMaxima = new Date(fechaDesdeObj.getFullYear(), fechaDesdeObj.getMonth(), fechaDesdeObj.getDate() + 30);

        if (fechaHastaObj > fechaMaxima) {
            return { valido: false, error: "El período de la oferta no puede exceder 30 días" };
        }

        var diferenciaTiempo = fechaHastaObj.getTime() - fechaDesdeObj.getTime();
        var dias = Math.floor(diferenciaTiempo / (1000 * 60 * 60 * 24)) + 1;

        return {
            valido: true,
            precio: precioOferta,
            topeVenta: topeVenta,
            fechaDesde: fechaDesde,
            fechaHasta: fechaHasta,
            fechaDesdeObj: fechaDesdeObj,
            fechaHastaObj: fechaHastaObj,
            fechaActual: fechaActual,
            dias: dias
        };

    } catch (error) {
        console.error("Error al obtener información de oferta:", error);
        return { valido: false, error: "Error al procesar la información de la oferta" };
    }
}

function guardarTodasLasOfertas() {
    var totalProductos = $("#tbGridProductosOferta tbody tr[data-producto-id]").length;
    if (totalProductos === 0) {
        ControlaMensajeWarning("No hay productos para guardar en ofertas");
        return;
    }

    var canalesInfo = obtenerCanalesSeleccionados();
    if (canalesInfo.canales.length === 0) {
        ControlaMensajeWarning("Debe seleccionar al menos un canal antes de guardar las ofertas");
        return;
    }

    var ofertaInfo = obtenerInformacionOfertaDefinida();
    if (!ofertaInfo.valido) {
        ControlaMensajeError(ofertaInfo.error);
        return;
    }

    var mensaje = generarMensajeConfirmacionOferta(totalProductos, canalesInfo, ofertaInfo);

    AbrirMensaje(
        "CONFIRMAR GUARDADO DE OFERTAS",
        mensaje,
        function (resp) {
            if (resp === "SI") {
                procesarGuardadoTodasLasOfertas(totalProductos, canalesInfo, ofertaInfo);
            }
            $("#msjModal").modal("hide");
            return true;
        },
        true,
        ["Guardar Ofertas", "Cancelar"],
        "info!",
        null
    );
}

function generarMensajeConfirmacionOferta(totalProductos, canalesInfo, ofertaInfo) {
    var mensaje = `¿Desea guardar ${totalProductos} producto${totalProductos > 1 ? 's' : ''} en ofertas?<br><br>`;

    mensaje += '<div class="text-start"><strong>📋 Detalles de la Oferta:</strong><br>';
    mensaje += `<small>`;

    mensaje += `💰 <strong>Precio oferta:</strong> $${formatearPrecioArgentino(ofertaInfo.precio)}<br>`;
    mensaje += `📅 <strong>Período:</strong> ${formatearFecha(ofertaInfo.fechaDesde)} al ${formatearFecha(ofertaInfo.fechaHasta)} <em>(${ofertaInfo.dias} día${ofertaInfo.dias > 1 ? 's' : ''})</em><br>`;

    if (ofertaInfo.topeVenta > 0) {
        mensaje += `📦 <strong>Tope de venta:</strong> ${ofertaInfo.topeVenta.toLocaleString('es-AR')} unidad${ofertaInfo.topeVenta > 1 ? 'es' : ''}<br>`;
    }

    mensaje += `</small></div><br>`;
    mensaje += generarSeccionCanales(canalesInfo);

    if (totalProductos > 1 || canalesInfo.canales.length > 1) {
        mensaje += generarResumenOperacion(totalProductos, canalesInfo, ofertaInfo);
    }

    return mensaje;
}

function generarSeccionCanales(canalesInfo) {
    var seccion = '<div class="text-start"><strong>🎯 Canales Seleccionados:</strong><br><small>';

    switch (canalesInfo.modo) {
        case "individual":
            if (canalesInfo.individual) {
                seccion += `📺 <strong>Canal Individual:</strong> ${canalesInfo.individual.canal}<br>`;
                seccion += `🏢 <strong>Administración:</strong> ${canalesInfo.individual.admNombre}<br>`;
                seccion += `📋 <strong>Lista:</strong> ${canalesInfo.individual.lpDesc}`;
            }
            break;

        case "multiple":
            var cantidad = canalesInfo.canales.length;
            seccion += `📺 <strong>Selección Múltiple:</strong> ${cantidad} canal${cantidad > 1 ? 'es' : ''}<br>`;

            if (cantidad <= 3) {
                canalesInfo.canales.forEach(function (canal, index) {
                    seccion += `   ${index + 1}. ${canal.canal} (${canal.admNombre})<br>`;
                });
            } else {
                canalesInfo.canales.slice(0, 3).forEach(function (canal, index) {
                    seccion += `   ${index + 1}. ${canal.canal} (${canal.admNombre})<br>`;
                });
                seccion += `   ... y ${cantidad - 3} canal${cantidad - 3 > 1 ? 'es' : ''} más`;
            }
            break;

        default:
            seccion += `⚠️ <em>Sin canales seleccionados</em>`;
    }

    seccion += '</small></div><br>';
    return seccion;
}

function generarResumenOperacion(totalProductos, canalesInfo, ofertaInfo) {
    var resumen = '<div class="text-start bg-light p-2 rounded"><strong>📊 Resumen de la Operación:</strong><br><small>';

    var totalCanales = canalesInfo.modo === "individual" ? 1 : canalesInfo.canales.length;
    var totalOfertas = totalProductos * totalCanales;

    resumen += `🔢 <strong>Total de ofertas a crear:</strong> ${totalOfertas.toLocaleString('es-AR')}<br>`;
    resumen += `   (${totalProductos} producto${totalProductos > 1 ? 's' : ''} × ${totalCanales} canal${totalCanales > 1 ? 'es' : ''})<br>`;

    if (ofertaInfo.topeVenta > 0) {
        var ventaMaximaTotal = totalOfertas * ofertaInfo.topeVenta;
        var valorMaximoTotal = ventaMaximaTotal * ofertaInfo.precio;

        resumen += `📈 <strong>Venta máxima total:</strong> ${ventaMaximaTotal.toLocaleString('es-AR')} unidades<br>`;
        resumen += `💵 <strong>Valor máximo total:</strong> $${formatearPrecioArgentino(valorMaximoTotal)}`;
    } else {
        resumen += `♾️ <strong>Sin límite de venta</strong> (tope no definido)`;
    }

    resumen += '</small></div>';
    return resumen;
}

function procesarGuardadoTodasLasOfertas(totalProductos, canalesInfo, ofertaInfo) {
    AbrirWaiting("Guardando ofertas...");

    var precioNumerico = parseFloat(ofertaInfo.precio.toFixed(2));
    
    var datosOferta = {
        canales: canalesInfo.canales,
        canalIndividual: canalesInfo.individual,
        modoSeleccion: canalesInfo.modo,
        precio: precioNumerico,
        fechaDesde: ofertaInfo.fechaDesde,
        fechaHasta: ofertaInfo.fechaHasta,
        topeVenta: ofertaInfo.topeVenta
    };

    var jsonData = JSON.stringify(datosOferta);

    $.ajax({
        url: confirmarAltaOfertaUrl,
        type: "POST",
        contentType: "application/json",
        data: jsonData,
        success: function(response) {
            CerrarWaiting();

            if (response.error) {
                ControlaMensajeError(response.msg || "Error al guardar ofertas");
                return;
            }

            if (response.warn) {
                ControlaMensajeWarning(response.msg || "Advertencia en el guardado");
                return;
            }

            var mensajeExito = generarMensajeExitoGuardado(totalProductos, canalesInfo, ofertaInfo);
            
            ControlaMensajeSuccessConCallback(mensajeExito, function() {
                window.location.href = homeOfertaUrl;
            });
        },
        error: function(xhr, status, error) {
            CerrarWaiting();
            console.error("Error en solicitud: ", xhr.responseText);
            ControlaMensajeError("Error de comunicación: " + (xhr.responseText || error || "Error desconocido"));
        }
    });
}

function generarMensajeExitoGuardado(totalProductos, canalesInfo, ofertaInfo) {
    var mensaje = `<strong>✅ Ofertas guardadas correctamente</strong><br><br>`;

    mensaje += `<div class="text-start"><small>`;
    mensaje += `📦 <strong>Productos:</strong> ${totalProductos} producto${totalProductos > 1 ? 's' : ''}<br>`;

    var totalCanales = canalesInfo.modo === "individual" ? 1 : canalesInfo.canales.length;
    mensaje += `📺 <strong>Canales:</strong> ${totalCanales} canal${totalCanales > 1 ? 'es' : ''}<br>`;
    
    mensaje += `📅 <strong>Período:</strong> ${formatearFecha(ofertaInfo.fechaDesde)} al ${formatearFecha(ofertaInfo.fechaHasta)}<br>`;
    
    mensaje += `💰 <strong>Precio oferta:</strong> $${formatearPrecioArgentino(ofertaInfo.precio)}<br>`;

    if (ofertaInfo.topeVenta > 0) {
        mensaje += `📈 <strong>Tope por oferta:</strong> ${ofertaInfo.topeVenta.toLocaleString('es-AR')} unidad${ofertaInfo.topeVenta > 1 ? 'es' : ''}<br>`;
    }

    var totalOfertas = totalProductos * totalCanales;
    mensaje += `🎯 <strong>Total de ofertas creadas:</strong> ${totalOfertas.toLocaleString('es-AR')}`;

    mensaje += `</small></div>`;

    return mensaje;
}

function mostrarEstadoOferta(productoId, productoDesc) {
    // Verificar parámetros requeridos
    if (!productoId) {
        console.error("Error: ID de producto requerido para mostrar estado");
        return;
    }
    
    // Actualizar título del modal con la descripción del producto
    $('#tituloModalEstado').html(`
        <i class="bx bx-info-circle text-info me-2"></i>
        Estado de Ofertas - ${productoDesc || productoId}
    `);
    
    // Mostrar spinner de carga
    $('#contenidoEstadoOferta').html(`
        <div class="d-flex justify-content-center">
            <div class="spinner-border text-warning" role="status">
                <span class="visually-hidden">Cargando...</span>
            </div>
        </div>
    `);
    
    // Mostrar el modal
    $('#modalEstadoOferta').modal('show');
    
    // Realizar la llamada AJAX
    $.ajax({
        url: obtenerEstadoOfertaProductoUrl || $('#obtenerEstadoOfertaProductoUrl').val(),
        type: 'POST',
        data: { p_id: productoId },
        success: function(response) {
            let contenido = '';
            
            if (response.error) {
                // Caso de error
                contenido = `
                    <div class="alert alert-danger">
                        <i class="bx bx-error-circle me-2"></i>
                        ${response.msg || "Error al obtener el estado de la oferta"}
                    </div>
                `;
            } else if (response.warn || !response.estados || response.estados.length === 0) {
                // Caso sin datos
                contenido = `
                    <div class="alert alert-warning">
                        <i class="bx bx-info-circle me-2"></i>
                        <strong>Sin ofertas activas</strong>
                        <hr>
                        <p class="mb-0">Aún no se le ha definido ninguna Oferta, Promo o Combo para ninguna Administración y Lista de Precios.</p>
                    </div>
                `;
            } else {
                // Caso con datos
                contenido = `
                    <div class="alert alert-success mb-3">
                        <i class="bx bx-check-circle me-2"></i>
                        <strong>Información disponible</strong>
                        <p class="mb-0">Se encontraron ${response.totalEstados || response.estados.length} registros de ofertas para este producto.</p>
                    </div>
                    <div class="table-responsive">
                        <table class="table table-sm table-striped table-bordered">
                            <thead class="table-dark">
                                <tr>
                                    <th scope="col">#</th>
                                    <th scope="col">Canal</th>
                                    <th scope="col">Estado</th>
                                </tr>
                            </thead>
                            <tbody>
                `;
                
                // Generar filas de la tabla
                response.estados.forEach((estado, index) => {
                    contenido += `
                        <tr>
                            <td>${index + 1}</td>
                            <td>${estado.canal || ''}</td>
                            <td>${estado.estado || ''}</td>
                        </tr>
                    `;
                });
                
                contenido += `
                            </tbody>
                        </table>
                    </div>
                `;
            }
            
            // Actualizar el contenido del modal
            $('#contenidoEstadoOferta').html(contenido);
        },
        error: function(xhr, status, error) {
            // Manejar errores de comunicación
            $('#contenidoEstadoOferta').html(`
                <div class="alert alert-danger">
                    <i class="bx bx-error-circle me-2"></i>
                    <strong>Error de comunicación</strong>
                    <p>No se pudo obtener el estado de la oferta. Por favor, inténtelo nuevamente.</p>
                    <p class="small text-muted">${error || status}</p>
                </div>
            `);
        }
    });
}

function cargarFamiliasParaBusquedaAvanzada(proveedorId) {
    if (!proveedorId) return;

    var combo = $("#busquedaModal #Rel03");
    combo.prop("disabled", false).html('<option>Cargando...</option>');

    $.ajax({
        url: autoComRel03Url,
        type: "POST",
        data: { ctaId: proveedorId },
        dataType: "json",
        success: function (obj) {
            combo.empty().append("<option value=''>Seleccionar...</option>");

            if (!obj.error && !obj.warn && obj.lista && obj.lista.length) {
                $.each(obj.lista, function (i, item) {
                    combo.append("<option value='" + item.id + "'>" + item.descripcion + "</option>");
                });
            } else if (obj.error || obj.warn) {
                console.warn("Advertencia al cargar familias:", obj.msg);
                combo.append("<option value=''>No hay familias disponibles</option>");
            }
        },
        error: function () {
            combo.html('<option>Error al cargar familias</option>');
        }
    });
}

// ✅ MEJORADO: Configuración de eventos para el grid de ofertas
function configurarEventosGridOferta() {
    // Botones para eliminar productos
    $(".btn-remover-oferta").off("click").on("click", function() {
        var productId = $(this).data("p-id");
        var row = $(this).closest("tr");
        var productDesc = row.find("td:nth-child(2)").text().trim();

        AbrirMensaje(
            "CONFIRMAR ELIMINACIÓN",
            `¿Está seguro de eliminar "${productDesc}" de las ofertas?`,
            function(resp) {
                if (resp === "SI") {
                    eliminarProductoDelGrid(row, productDesc);
                }
                $("#msjModal").modal("hide");
                return true;
            },
            true,
            ["Eliminar", "Cancelar"],
            "warn!",
            null
        );
    });

    // ✅ IMPLEMENTADO: Botones para ver estado de ofertas
    $(".btn-estado-oferta").off("click").on("click", function() {
        var productId = $(this).data("p-id");
        var productDesc = $(this).closest("tr").find("td:nth-child(2)").text().trim();
        mostrarEstadoOferta(productId, productDesc);
    });

    // Botón guardar ofertas
    $("#btnGuardarOfertas").off("click").on("click", function() {
        guardarTodasLasOfertas();
    });

    // Botón cancelar oferta
    $("#btnCancelaOferta").off("click").on("click", function() {
        AbrirMensaje(
            "CONFIRMAR CANCELACIÓN DE LA OFERTA",
            `¿Está seguro de desea CANCELAR la(s) oferta(s)?`,
            function(resp) {
                if (resp === "SI") {
                    window.location.href = homeOfertaUrl;
                }
                $("#msjModal").modal("hide");
                return true;
            },
            true,
            ["Continuar", "Cancelar"],
            "warn!",
            null
        );
    });
}