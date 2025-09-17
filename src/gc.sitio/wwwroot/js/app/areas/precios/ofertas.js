// ✅ NUEVA: Función para parsear fechas de forma segura
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

// ✅ NUEVA: Función para obtener fecha actual normalizada
function obtenerFechaActualNormalizada() {
    const hoy = new Date();
    return new Date(hoy.getFullYear(), hoy.getMonth(), hoy.getDate());
}

// ✅ NUEVA: Función para normalizar fecha (sin componente horaria)
function normalizarFecha(fecha) {
    if (!fecha || !(fecha instanceof Date) || isNaN(fecha.getTime())) return null;
    return new Date(fecha.getFullYear(), fecha.getMonth(), fecha.getDate());
}

// ✅ NUEVA: Función para formatear fecha en formato YYYY-MM-DD para inputs
function formatearFechaParaInput(fecha) {
    if (!fecha || !(fecha instanceof Date) || isNaN(fecha.getTime())) return '';
    
    var año = fecha.getFullYear();
    var mes = (fecha.getMonth() + 1).toString().padStart(2, '0');
    var dia = fecha.getDate().toString().padStart(2, '0');
    
    return `${año}-${mes}-${dia}`;
}

// ✅ CORREGIDA: Función para manejo de tooltip de error
function mostrarTooltipError(elemento, mensaje) {
    // Verificar disponibilidad de Bootstrap tooltips
    if (typeof elemento.tooltip !== 'function') {
        elemento.attr('title', mensaje);
        return;
    }

    try {
        // Destruir tooltip existente si existe
        elemento.tooltip('dispose');
        
        // ✅ CORREGIDO: Configuración mejorada del tooltip
        elemento.tooltip({
            title: mensaje,
            placement: 'bottom', // Cambiar a bottom para que aparezca debajo apuntando hacia arriba
            trigger: 'manual',
            container: 'body', // Ayuda a evitar problemas de posicionamiento
            customClass: 'tooltip-error',
            template: '<div class="tooltip tooltip-error" role="tooltip"><div class="arrow"></div><div class="tooltip-inner"></div></div>'
        });
        
        // Mostrar el tooltip
        elemento.tooltip('show');
    } catch (error) {
        // Fallback a title simple
        elemento.attr('title', mensaje);
    }
}

// ✅ CORREGIDA: Función para ocultar tooltip de error
function ocultarTooltipError(elemento) {
    try {
        if (typeof elemento.tooltip === 'function') {
            // Ocultar y destruir tooltip
            elemento.tooltip('hide');
            elemento.tooltip('dispose');
        }
        // Limpiar title también
        elemento.removeAttr('title');
    } catch (error) {
        // Fallback: solo remover title
        elemento.removeAttr('title');
    }
}

// ✅ OPTIMIZADO: Configuración InputMask para precio y tope de venta
function configurarInputMaskPrecios() {
    // ✅ VERIFICACIÓN: Disponibilidad de InputMask
    if (typeof Inputmask === 'undefined') {
        console.warn("InputMask no está disponible");
        return;
    }

    // ✅ CORREGIDO: Máscara decimal para precio con punto decimal fijo
    const maskConfigPrecio = {
        alias: "numeric",
        groupSeparator: ",",     // Separador de miles
        radixPoint: ".",         // Punto como separador decimal
        autoGroup: true,
        digits: 2,               // 2 decimales fijos
        digitsOptional: false,   // No opcional, siempre 2 decimales
        rightAlign: true,
        prefix: '',
        placeholder: "0.00",
        clearMaskOnLostFocus: false,
        showMaskOnHover: false,
        showMaskOnFocus: true,
        min: 0,
        allowMinus: false,
        unmaskAsNumber: true,    // ✅ IMPORTANTE: Obtener valor como número al usar .unmask()
        onBeforeMask: function (value) {
            // Normalización del valor antes de aplicar la máscara
            if (!value) return '0';
            // Limpiar cualquier formato y convertir a número
            return parseFloat(value.toString().replace(/[^\d.]/g, '')).toFixed(2);
        }
    };

    // ✅ SIMPLIFICADA: Máscara para tope de venta (entero)
    const maskConfigTope = {
        alias: "integer",
        min: 0,
        rightAlign: true,
        placeholder: "0",
        clearMaskOnLostFocus: false
    };

    // ✅ APLICAR: Máscaras a los campos
    try {
        Inputmask(maskConfigPrecio).mask('#txtPrecioOferta');
        Inputmask(maskConfigTope).mask('#txtTopeVenta');
        console.log("InputMask configurado para campos de oferta");
    } catch (error) {
        console.error("Error al aplicar InputMask:", error);
    }
}

// ✅ OPTIMIZADA: Función para validación en tiempo real sin validar al inicio
function configurarValidacionTiempoReal() {
    // Validación para precio solo en cambios, no al cargar
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

    // Validación para tope de venta solo en cambios, no al cargar
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

    // Validación para fechas
    $("#txtFechaDesde, #txtFechaHasta").on("change", function() {
        validarRangoFechas();
    });
}

// ✅ MEJORADA: Inicialización de campos de fecha, precio y tope de venta
function inicializarCamposFecha() {
    // Obtener fecha actual (hoy)
    var fechaActual = obtenerFechaActualNormalizada();
    
    // Calcular fecha 30 días después para fecha hasta a partir del mismo día
    var fechaHasta = new Date(fechaActual);
    fechaHasta.setDate(fechaHasta.getDate() + 30 - 1);
    
    // Formatear fechas para inputs HTML (YYYY-MM-DD)
    var fechaDesdeFormatted = formatearFechaParaInput(fechaActual);
    var fechaHastaFormatted = formatearFechaParaInput(fechaHasta);
    
    // Establecer valores en los campos de fecha
    $("#txtFechaDesde").val(fechaDesdeFormatted);
    $("#txtFechaHasta").val(fechaHastaFormatted);
    
    // ✅ CORREGIDO: Configurar InputMask y establecer valores sin validación inicial
    try {
        // 1. Desactivar temporalmente los eventos de change para evitar validación
        $("#txtPrecioOferta, #txtTopeVenta").off("change");
        
        // 2. Configurar InputMask
        configurarInputMaskPrecios();
        
        // 3. Asignar valores iniciales sin disparar validación
        $("#txtPrecioOferta").val("0");
        $("#txtTopeVenta").val("0");
        
        // 4. Asegurar que no hay clases de error
        $("#txtPrecioOferta, #txtTopeVenta").removeClass("is-invalid");
        
        // 5. Configurar los eventos de validación después de inicializar
        setTimeout(function() {
            configurarValidacionTiempoReal();
        }, 200);
    } catch (e) {
        console.warn("Error al inicializar valores:", e.message);
    }
    
    // Validar solo rango de fechas para mostrar información del período
    setTimeout(validarRangoFechas, 300);
}

// Variables de estado para gestión de selección
var modoSeleccionCanal = "ninguno"; // "individual", "multiple", "ninguno"
var canalIndividualSeleccionado = null;

// ✅ ACTUALIZADA: Función de inicialización única y simplificada
$(function () {
    console.log("🚀 Iniciando ofertas.js v2.0");
    
    // ✅ EVENTOS CORE
    $("#btnBusquedaBase").on("click", function () {
        buscarProducto();
        return true;
    });

    $("#estadoFuncion").on("change", verificaEstado);

    // ✅ ORDEN OPTIMIZADO: Primero inicializar sistema y campos, luego validación
    try { 
        inicializarSistemaBasico();
        inicializarCamposFecha(); // Ahora incluye la configuración de InputMask y maneja la validación
        // No llamamos a configurarValidacionTiempoReal() aquí, lo llamamos desde inicializarCamposFecha()
    } catch (e) { 
        console.warn("Inicialización:", e.message); 
    }
    
    try { cargarCanales(); } catch (e) { console.error("Canales:", e); }

    // ✅ SHORTCUTS
    $(document).on("keydown", function(e) {
        if (e.ctrlKey && e.key === "s") {
            e.preventDefault();
            guardarTodasLasOfertas();
        }
        if (e.key === "Escape") {
            limpiarFormularioOfertas();
        }
    });

    // Usar delegación de eventos para manejar autocompleteselect en el modal
    $(document).on("autocompleteselect", "#busquedaModal #Rel01", function (event, ui) {
        setTimeout(function () {
            cargarFamiliasParaBusquedaAvanzada(ui.item.id);
        }, 100);
    });

    // ✅ LIMPIEZA INICIAL
    $("#Busqueda").val("");
    $("#estadoFuncion").val(false);
    
    console.log("✅ Ofertas.js listo");
});

// ✅ MANEJO DE ERRORES MÍNIMO
window.addEventListener('error', function(e) {
    console.error("Error:", e.message);
    return false;
});

// ✅ SIMPLIFICADA: Función de inicialización sin dependencias externas
function inicializarSistemaBasico() {
    try {
        // Solo inicializar lo esencial
        inicializarShortcutsBasicos();
        console.log("✅ Sistema básico inicializado correctamente");
    } catch (error) {
        console.error("❌ Error en inicialización básica:", error);
    }
}

// ✅ NUEVA: Shortcuts básicos sin dependencias
function inicializarShortcutsBasicos() {
    $(document).on("keydown", function (e) {
        // Ctrl + S para guardar ofertas
        if (e.ctrlKey && e.key === "s") {
            e.preventDefault();
            if (typeof guardarTodasLasOfertas === 'function') {
                guardarTodasLasOfertas();
            }
        }

        // Escape para limpiar formulario
        if (e.key === "Escape") {
            if (typeof limpiarFormularioOfertas === 'function') {
                limpiarFormularioOfertas();
            }
        }
    });
}

function verificaEstado(e) {
    FunctionCallback = null;
    var res = $("#estadoFuncion").val();
    CerrarWaiting();

    if (res === "true") {
        var prod = productoBase;

        if (prod && prod.p_id) {
            presentarProductoEnOferta(prod);
        }

        // Limpiar para siguiente búsqueda
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
        ControlaMensajeSuccess(`Producto "${producto.p_desc}" agregado a ofertas correctamente`);
    }, function (error) {
        CerrarWaiting();
        ControlaMensajeError("Error al agregar producto a ofertas: " + (error.message || "Error desconocido"));
    });
}

// ✅ OPTIMIZADA: Función para validación en tiempo real sin validar al inicio
function configurarValidacionTiempoReal() {
    // Validación para precio solo en cambios, no al cargar
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

    // Validación para tope de venta solo en cambios, no al cargar
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

    // Validación para fechas
    $("#txtFechaDesde, #txtFechaHasta").on("change", function() {
        validarRangoFechas();
    });
}

// ✅ MEJORADA: Inicialización de campos de fecha, precio y tope de venta
function inicializarCamposFecha() {
    // Obtener fecha actual (hoy)
    var fechaActual = obtenerFechaActualNormalizada();
    
    // Calcular fecha 30 días después para fecha hasta a partir del mismo día
    var fechaHasta = new Date(fechaActual);
    fechaHasta.setDate(fechaHasta.getDate() + 30 - 1);
    
    // Formatear fechas para inputs HTML (YYYY-MM-DD)
    var fechaDesdeFormatted = formatearFechaParaInput(fechaActual);
    var fechaHastaFormatted = formatearFechaParaInput(fechaHasta);
    
    // Establecer valores en los campos de fecha
    $("#txtFechaDesde").val(fechaDesdeFormatted);
    $("#txtFechaHasta").val(fechaHastaFormatted);
    
    // ✅ CORREGIDO: Configurar InputMask y establecer valores sin validación inicial
    try {
        // 1. Desactivar temporalmente los eventos de change para evitar validación
        $("#txtPrecioOferta, #txtTopeVenta").off("change");
        
        // 2. Configurar InputMask
        configurarInputMaskPrecios();
        
        // 3. Asignar valores iniciales sin disparar validación
        $("#txtPrecioOferta").val("0");
        $("#txtTopeVenta").val("0");
        
        // 4. Asegurar que no hay clases de error
        $("#txtPrecioOferta, #txtTopeVenta").removeClass("is-invalid");
        
        // 5. Configurar los eventos de validación después de inicializar
        setTimeout(function() {
            configurarValidacionTiempoReal();
        }, 200);
    } catch (e) {
        console.warn("Error al inicializar valores:", e.message);
    }
    
    // Validar solo rango de fechas para mostrar información del período
    setTimeout(validarRangoFechas, 300);
}

// Variables de estado para gestión de selección
var modoSeleccionCanal = "ninguno"; // "individual", "multiple", "ninguno"
var canalIndividualSeleccionado = null;

// ✅ ACTUALIZADA: Función de inicialización única y simplificada
$(function () {
    console.log("🚀 Iniciando ofertas.js v2.0");
    
    // ✅ EVENTOS CORE
    $("#btnBusquedaBase").on("click", function () {
        buscarProducto();
        return true;
    });

    $("#estadoFuncion").on("change", verificaEstado);

    // ✅ ORDEN OPTIMIZADO: Primero inicializar sistema y campos, luego validación
    try { 
        inicializarSistemaBasico();
        inicializarCamposFecha(); // Ahora incluye la configuración de InputMask y maneja la validación
        // No llamamos a configurarValidacionTiempoReal() aquí, lo llamamos desde inicializarCamposFecha()
    } catch (e) { 
        console.warn("Inicialización:", e.message); 
    }
    
    try { cargarCanales(); } catch (e) { console.error("Canales:", e); }

    // ✅ SHORTCUTS
    $(document).on("keydown", function(e) {
        if (e.ctrlKey && e.key === "s") {
            e.preventDefault();
            guardarTodasLasOfertas();
        }
        if (e.key === "Escape") {
            limpiarFormularioOfertas();
        }
    });

    // Usar delegación de eventos para manejar autocompleteselect en el modal
    $(document).on("autocompleteselect", "#busquedaModal #Rel01", function (event, ui) {
        setTimeout(function () {
            cargarFamiliasParaBusquedaAvanzada(ui.item.id);
        }, 100);
    });

    // ✅ LIMPIEZA INICIAL
    $("#Busqueda").val("");
    $("#estadoFuncion").val(false);
    
    console.log("✅ Ofertas.js listo");
});

// ✅ MANEJO DE ERRORES MÍNIMO
window.addEventListener('error', function(e) {
    console.error("Error:", e.message);
    return false;
});

// ✅ SIMPLIFICADA: Función de inicialización sin dependencias externas
function inicializarSistemaBasico() {
    try {
        // Solo inicializar lo esencial
        inicializarShortcutsBasicos();
        console.log("✅ Sistema básico inicializado correctamente");
    } catch (error) {
        console.error("❌ Error en inicialización básica:", error);
    }
}

// ✅ NUEVA: Shortcuts básicos sin dependencias
function inicializarShortcutsBasicos() {
    $(document).on("keydown", function (e) {
        // Ctrl + S para guardar ofertas
        if (e.ctrlKey && e.key === "s") {
            e.preventDefault();
            if (typeof guardarTodasLasOfertas === 'function') {
                guardarTodasLasOfertas();
            }
        }

        // Escape para limpiar formulario
        if (e.key === "Escape") {
            if (typeof limpiarFormularioOfertas === 'function') {
                limpiarFormularioOfertas();
            }
        }
    });
}

function verificaEstado(e) {
    FunctionCallback = null;
    var res = $("#estadoFuncion").val();
    CerrarWaiting();

    if (res === "true") {
        var prod = productoBase;

        if (prod && prod.p_id) {
            presentarProductoEnOferta(prod);
        }

        // Limpiar para siguiente búsqueda
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
        ControlaMensajeSuccess(`Producto "${producto.p_desc}" agregado a ofertas correctamente`);
    }, function (error) {
        CerrarWaiting();
        ControlaMensajeError("Error al agregar producto a ofertas: " + (error.message || "Error desconocido"));
    });
}

// Función mejorada de eliminación
function eliminarProductoDelGrid(row, productDesc) {
    row.fadeOut(300, function () {
        $(this).remove();

        // Verificar si quedan productos (corrección del selector)
        if ($("#tbGridProductosOferta tbody tr[data-producto-id]").length === 0) {
            $("#gridProductoOferta").html(`
                <div class="text-center text-muted py-4">
                    <i class="bx bx-info-circle me-2"></i>
                    No hay productos seleccionados para ofertas
                </div>
            `);
        }

        ControlaMensajeInfo(`Producto "${productDesc}" eliminado de ofertas`);
    });
}

/** Funciones sobre los canales */
// Función para cargar canales al inicializar
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

// Configurar eventos específicos del grid de canales
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
            // Si hay solo uno seleccionado y no estamos en modo individual, cambiar a múltiple
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

// Función optimizada para actualizar contador
function actualizarContadorCanales() {
    var checkedCount = $(".check-canal:checked").length;
    $("#canalesSeleccionados").text(checkedCount);

    // Mostrar/ocultar el panel según la selección
    if (checkedCount === 0 && modoSeleccionCanal !== "ninguno") {
        cambiarModoSeleccion("ninguno");
    }
}

//funcion para obtener los datos de la oferta definida
// ✅ OPTIMIZADA: Función con parsing seguro de precios
function obtenerInformacionOfertaDefinida() {
    try {
        // ✅ CORREGIDO: Obtención del precio con manejo correcto del separador decimal
        var precioTexto = $("#txtPrecioOferta").val() || "0";
        
        // Extracción del valor numérico quitando todos los formatos
        var precioLimpio = precioTexto.replace(/[^\d.-]/g, '');
        
        // Si el último caracter es punto o coma, lo quitamos (evita "123.")
        if (precioLimpio.endsWith('.') || precioLimpio.endsWith(',')) {
            precioLimpio = precioLimpio.slice(0, -1);
        }
        
        var precioOferta = parseFloat(precioLimpio);
        
        // Resto de la función continúa igual...
        var fechaDesde = $("#txtFechaDesde").val();
        var fechaHasta = $("#txtFechaHasta").val();
        var topeVenta = parseInt($("#txtTopeVenta").val().replace(/[^\d]/g, '') || "0");
        
        // VALIDACIÓN 1: Precio mayor a cero
        if (isNaN(precioOferta) || precioOferta <= 0) {
            return {
                valido: false,
                error: "El precio de la oferta debe ser mayor a cero"
            };
        }

        // VALIDACIÓN 2: Tope de venta >= 0
        if (isNaN(topeVenta) || topeVenta < 0) {
            return {
                valido: false,
                error: "El tope de venta debe ser mayor o igual a cero"
            };
        }

        // VALIDACIÓN 3: Fechas requeridas
        if (!fechaDesde || !fechaHasta) {
            return {
                valido: false,
                error: "Debe especificar las fechas de inicio y fin de la oferta"
            };
        }

        // ✅ PARSING SEGURO: Usando función helper
        var fechaDesdeObj = parsearFechaSegura(fechaDesde);
        var fechaHastaObj = parsearFechaSegura(fechaHasta);
        var fechaActual = obtenerFechaActualNormalizada();

        // VALIDACIÓN 4: Fechas válidas
        if (!fechaDesdeObj || !fechaHastaObj || isNaN(fechaDesdeObj.getTime()) || isNaN(fechaHastaObj.getTime())) {
            return {
                valido: false,
                error: "Las fechas especificadas no son válidas"
            };
        }

        // VALIDACIÓN 5: Desde <= Hasta
        if (fechaDesdeObj > fechaHastaObj) {
            return {
                valido: false,
                error: "La fecha de inicio debe ser menor o igual a la fecha de fin"
            };
        }

        // VALIDACIÓN 6: Desde >= fecha actual
        if (fechaDesdeObj < fechaActual) {
            return {
                valido: false,
                error: "La fecha de inicio no puede ser anterior a la fecha actual"
            };
        }

        // VALIDACIÓN 7: Hasta <= Desde + 30 días
        var fechaMaxima = new Date(fechaDesdeObj.getFullYear(), fechaDesdeObj.getMonth(), fechaDesdeObj.getDate() + 30);

        if (fechaHastaObj > fechaMaxima) {
            return {
                valido: false,
                error: "El período de la oferta no puede exceder 30 días"
            };
        }

        // ✅ CÁLCULO OPTIMIZADO: Diferencia de días
        var diferenciaTiempo = fechaHastaObj.getTime() - fechaDesdeObj.getTime();
        var dias = Math.floor(diferenciaTiempo / (1000 * 60 * 60 * 24)) + 1;

        // ✅ RETORNO: Datos válidos con parsing seguro
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
        return {
            valido: false,
            error: "Error al procesar la información de la oferta"
        };
    }
}

// Función para obtener canales seleccionados (útil para otras funciones)
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

// Seleccionar un canal individual
// Función optimizada para selección individual
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

// Nueva función para cambiar modo de selección
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

// Nueva función para limpiar selección
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

// ✅ OPTIMIZADA: Función con mensaje de confirmación unificado y eficiente
function guardarTodasLasOfertas() {
    // ✅ VALIDACIÓN 1: Verificar productos en el grid
    var totalProductos = $("#tbGridProductosOferta tbody tr[data-producto-id]").length;
    if (totalProductos === 0) {
        ControlaMensajeWarning("No hay productos para guardar en ofertas");
        return;
    }

    // ✅ VALIDACIÓN 2: Verificar canales seleccionados
    var canalesInfo = obtenerCanalesSeleccionados();
    if (!canalesInfo.haySeleccion && canalesInfo.canales.length === 0) {
        ControlaMensajeWarning("Debe seleccionar al menos un canal antes de guardar las ofertas");
        return;
    }

    // ✅ VALIDACIÓN 3: Verificar información de oferta
    var ofertaInfo = obtenerInformacionOfertaDefinida();
    if (!ofertaInfo.valido) {
        ControlaMensajeError(ofertaInfo.error);
        return;
    }

    // ✅ OPTIMIZADO: Generar mensaje unificado con función helper
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

// ✅ NUEVA: Función para generar mensaje de confirmación unificado
function generarMensajeConfirmacionOferta(totalProductos, canalesInfo, ofertaInfo) {
    // ✅ ENCABEZADO: Información principal
    var mensaje = `¿Desea guardar ${totalProductos} producto${totalProductos > 1 ? 's' : ''} en ofertas?<br><br>`;

    // ✅ SECCIÓN: Detalles de la oferta
    mensaje += '<div class="text-start"><strong>📋 Detalles de la Oferta:</strong><br>';
    mensaje += `<small>`;


    // ✅ PRECIO: Con formato argentino
    mensaje += `💰 <strong>Precio oferta:</strong> $${formatearPrecioArgentino(ofertaInfo.precio)}<br>`;

    // ✅ PERÍODO: Con duración calculada
    mensaje += `📅 <strong>Período:</strong> ${formatearFecha(ofertaInfo.fechaDesde)} al ${formatearFecha(ofertaInfo.fechaHasta)} <em>(${ofertaInfo.dias} día${ofertaInfo.dias > 1 ? 's' : ''})</em><br>`;

    // ✅ TOPE: Solo si está definido
    if (ofertaInfo.topeVenta > 0) {
        mensaje += `📦 <strong>Tope de venta:</strong> ${ofertaInfo.topeVenta.toLocaleString('es-AR')} unidad${ofertaInfo.topeVenta > 1 ? 'es' : ''}<br>`;
    }

    mensaje += `</small></div><br>`;

    // ✅ SECCIÓN: Información de canales
    mensaje += generarSeccionCanales(canalesInfo);

    // ✅ RESUMEN: Información adicional si hay múltiples elementos
    if (totalProductos > 1 || canalesInfo.canales.length > 1) {
        mensaje += generarResumenOperacion(totalProductos, canalesInfo, ofertaInfo);
    }

    return mensaje;
}

// ✅ NUEVA: Función para formatear precio con formato argentino
function formatearPrecioArgentino(precio) {
    return precio.toLocaleString('es-AR', {
        style: 'currency',
        currency: 'ARS',
        minimumFractionDigits: 2,
        maximumFractionDigits: 2
    }).replace('ARS', '').trim();
}

// ✅ OPTIMIZADA: Función para formatear fecha con parsing seguro
function formatearFecha(fechaString) {
    if (!fechaString) return '';

    try {
        // ✅ INTENTAR: Usar fecha ya parseada si viene del objeto de validación
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

        // ✅ FORMATO ARGENTINO: DD/MM/YYYY
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

// Función para generar sección de canales en el mensaje de confirmación
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

            // ✅ MOSTRAR: Primeros 3 canales y resumen si hay más
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

// Función para generar resumen de la operación en el mensaje de confirmación
function generarResumenOperacion(totalProductos, canalesInfo, ofertaInfo) {
    var resumen = '<div class="text-start bg-light p-2 rounded"><strong>📊 Resumen de la Operación:</strong><br><small>';

    // ✅ CÁLCULO: Total de ofertas que se crearán
    var totalCanales = canalesInfo.modo === "individual" ? 1 : canalesInfo.canales.length;
    var totalOfertas = totalProductos * totalCanales;

    resumen += `🔢 <strong>Total de ofertas a crear:</strong> ${totalOfertas.toLocaleString('es-AR')}<br>`;
    resumen += `   (${totalProductos} producto${totalProductos > 1 ? 's' : ''} × ${totalCanales} canal${totalCanales > 1 ? 'es' : ''})<br>`;

    // ✅ VALOR: Total estimado si hay tope de venta
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

// ✅ NUEVA: Procesar guardado de todas las ofertas
function procesarGuardadoTodasLasOfertas(totalProductos, canalesInfo, ofertaInfo) {
    AbrirWaiting("Guardando ofertas...");

    // ✅ GARANTIZAR FORMATO NUMÉRICO: Convertir explícitamente a número con 2 decimales
    var precioNumerico = parseFloat(ofertaInfo.precio.toFixed(2));
    
    var datosOferta = {
        canales: canalesInfo.canales,
        canalIndividual: canalesInfo.individual,
        modoSeleccion: canalesInfo.modo,
        precio: precioNumerico, // ✅ CORREGIDO: Usar valor numérico puro sin formato
        fechaDesde: ofertaInfo.fechaDesde,
        fechaHasta: ofertaInfo.fechaHasta,
        topeVenta: ofertaInfo.topeVenta
    };

    // ✅ SIMPLIFICADO: JSON.stringify sin replacer - ya preparamos el número correctamente
    var jsonData = JSON.stringify(datosOferta);
    
    // Verificación de depuración (opcional)
    console.log("Precio a enviar:", precioNumerico, "Tipo:", typeof precioNumerico);
    console.log("Datos JSON a enviar:", jsonData);

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

            // ✅ OPTIMIZADO: Generar mensaje y redirigir después de mostrar
            var mensajeExito = generarMensajeExitoGuardado(totalProductos, canalesInfo, ofertaInfo);
            
            // Mostrar mensaje con callback para redirección
            ControlaMensajeSuccessConCallback(mensajeExito, function() {
                // Redireccionar a la página inicial de ofertas
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

// ✅ NUEVA: Función para mostrar mensaje de éxito con redirección
function ControlaMensajeSuccessConCallback(mensaje, callback) {
    // Mostrar el mensaje de éxito
    ControlaMensajeSuccess(mensaje);
    
    // Ejecutar callback después de 2 segundos para dar tiempo a ver el mensaje
    setTimeout(function() {
        if (typeof callback === 'function') {
            callback();
        }
    }, 2000);
}

// ✅ NUEVA: Función para generar mensaje de éxito en el guardado
function generarMensajeExitoGuardado(totalProductos, canalesInfo, ofertaInfo) {
    var mensaje = `<strong>✅ Ofertas guardadas correctamente</strong><br><br>`;

    // Información principal
    mensaje += `<div class="text-start"><small>`;
    mensaje += `📦 <strong>Productos:</strong> ${totalProductos} producto${totalProductos > 1 ? 's' : ''}<br>`;

    // Información de canales
    var totalCanales = canalesInfo.modo === "individual" ? 1 : canalesInfo.canales.length;
    mensaje += `📺 <strong>Canales:</strong> ${totalCanales} canal${totalCanales > 1 ? 'es' : ''}<br>`;
    
    // Información de período
    mensaje += `📅 <strong>Período:</strong> ${formatearFecha(ofertaInfo.fechaDesde)} al ${formatearFecha(ofertaInfo.fechaHasta)}<br>`;
    
    // Información de precio
    mensaje += `💰 <strong>Precio oferta:</strong> $${formatearPrecioArgentino(ofertaInfo.precio)}<br>`;

    // Información de tope (si existe)
    if (ofertaInfo.topeVenta > 0) {
        mensaje += `📈 <strong>Tope por oferta:</strong> ${ofertaInfo.topeVenta.toLocaleString('es-AR')} unidad${ofertaInfo.topeVenta > 1 ? 'es' : ''}<br>`;
    }

    // Total de ofertas creadas
    var totalOfertas = totalProductos * totalCanales;
    mensaje += `🎯 <strong>Total de ofertas creadas:</strong> ${totalOfertas.toLocaleString('es-AR')}`;

    mensaje += `</small></div>`;

    return mensaje;
}

// Función para cargar familias en dropdown de búsqueda avanzada
function cargarFamiliasParaBusquedaAvanzada(proveedorId) {
    if (!proveedorId) return;

    // Habilitar dropdown y mostrar indicador de carga
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

// ✅ IMPLEMENTADA: Función para mostrar el estado de ofertas de un producto
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