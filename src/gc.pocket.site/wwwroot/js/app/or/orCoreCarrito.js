// ======================================================================
// OR CARRITO - MÓDULO DE CARGA DE PRODUCTOS EN CARRITO
// ======================================================================

$(function () {
    console.log('✅ Módulo OR Carrito inicializado');

    // Inicializar eventos
    inicializarEventosCarrito();

    // Cargar vista por defecto (ordenado por BOX)
    presentaListaProducto("B");
});

// ======================================================================
// INICIALIZACIÓN DE EVENTOS
// ======================================================================

function inicializarEventosCarrito() {
    console.log('🔧 Inicializando eventos de OR Carrito...');

    // Evento cambio de radiobutton BOX
    $("#radioBox").on("change", function () {
        if ($(this).is(":checked")) {
            console.log('📦 Cambiando ordenamiento a BOX');
            presentaListaProducto("B");
        }
    });

    // Evento cambio de radiobutton RUBRO
    $("#radioRub").on("change", function () {
        if ($(this).is(":checked")) {
            console.log('🏷️ Cambiando ordenamiento a RUBRO');
            presentaListaProducto("R");
        }
    });

    // Evento cambio de radiobutton PRODUCTO
    $("#radioProd").on("change", function () {
        if ($(this).is(":checked")) {
            console.log('📝 Cambiando ordenamiento a PRODUCTO');
            presentaListaProducto("P");
        }
    });   

    console.log('✅ Eventos de OR Carrito inicializados');
}

// ======================================================================
// FUNCIÓN PRINCIPAL: PRESENTAR LISTA DE PRODUCTOS
// ======================================================================

/**
 * Carga la lista de productos con el ordenamiento especificado
 * @param {string} orden - Criterio de ordenamiento: "B" (BOX), "R" (RUBRO), "P" (PRODUCTO)
 */
function presentaListaProducto(orden) {
    console.log(`📡 Cargando lista de productos - Orden: ${orden}`);
    
    // Mostrar indicador de carga
    AbrirWaiting('Cargando productos...');

    // Preparar datos
    const datos = { orden: orden };

    // Realizar petición
    PostGenHtml(datos, BuscarListaProductosORUrl, function (html) {
        console.log('✅ Vista de productos cargada correctamente');
        
        // Inyectar HTML en el contenedor
        $("#contenedorCarritoOR").html(html);
        
        // Verificar si hay productos y controlar visibilidad del botón continuar
        var tbody = $("#contenedorCarritoOR #tbORListaProd tbody td");
        if (tbody.length <= 0) {
            $("#btnContinuar").hide("fast");
            console.log('⚠️ No hay productos - Ocultando botón continuar');
        } else {
            $("#btnContinuar").show("fast");
            console.log('✅ Productos cargados - Mostrando botón continuar');
        }
        
        CerrarWaiting();
    }, function (xhr, status, error) {
        console.error('❌ Error al cargar lista de productos:', error);
        CerrarWaiting();
        
        AbrirMensaje(
            "ERROR",
            "Error al cargar la lista de productos. Por favor, intente nuevamente.",
            function() {
                $("#msjModal").modal("hide");
                return true;
            },
            false,
            ["Aceptar"],
            "error!",
            null
        );
    });
}

// ======================================================================
// FUNCIONES AUXILIARES
// ======================================================================

/**
 * Muestra un mensaje informativo
 * @param {string} nota - Texto del mensaje a mostrar
 */
function mostrarMensaje(nota) {
    // Parsear nota si viene como JSON
    let mensajeTexto = nota;
    try {
        mensajeTexto = JSON.parse(nota);
    } catch (e) {
        // Si no es JSON válido, usar el texto tal cual
    }
    
    AbrirMensaje(
        "INFORMACIÓN",
        mensajeTexto,
        function() {
            $("#msjModal").modal("hide");
            return true;
        },
        false,
        ["Aceptar"],
        "info!",
        null
    );
}

/**
 * Limpia un producto del carrito
 * @param {string} p_id - ID del producto
 * @param {string} boxId - ID del BOX
 */
function limpiaProductoCarritoOR(p_id, boxId) {
    console.log(`🧹 Limpiando producto del carrito - P_ID: ${p_id}, BOX: ${boxId}`);
    
    AbrirWaiting('Limpiando producto del carrito...');

    const datos = { p_id: p_id, boxId: boxId };

    PostGen(datos, LimpiaProductoCarritoORUrl, function (obj) {
        CerrarWaiting();
        
        if (obj.error === true) {
            AbrirMensaje(
                "ERROR",
                obj.msg,
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
        else if (obj.warn === true) {
            AbrirMensaje(
                "ADVERTENCIA",
                obj.msg,
                function() {
                    $("#msjModal").modal("hide");
                    return true;
                },
                false,
                ["Aceptar"],
                "warn!",
                null
            );
        }
        else {
            AbrirMensaje(
                "ÉXITO",
                obj.msg,
                function() {
                    $("#msjModal").modal("hide");
                    // Recargar lista con el mismo ordenamiento
                    window.location.href = proximoProductoUrl + `?or_compte=${orCompteActual}`
                    return true;
                },
                false,
                ["Aceptar"],
                "succ!",
                null
            );
        }
    }, function (xhr, status, error) {
        CerrarWaiting();
        console.error('❌ Error al limpiar producto:', error);
        
        AbrirMensaje(
            "ERROR",
            "Error al limpiar el producto del carrito",
            function() {
                $("#msjModal").modal("hide");
                return true;
            },
            false,
            ["Aceptar"],
            "error!",
            null
        );
    });
}

console.log('🎉 Módulo orCoreCarrito.js cargado - Versión 1.0.0');