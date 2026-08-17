$(function () {
    InicializaEventosImpEt();
});

// ✅ VARIABLES GLOBALES PARA CONTROL DE ESTADO
let productosEtiquetaCargados = [];
const MAX_PRODUCTOS_ETIQUETA = 100; // Límite razonable para impresión

// ✅ VARIABLES PARA CONTROL DE BÚSQUEDA (DECLARADAS UNA SOLA VEZ)
let $inputBusqueda, $btnBuscar, $spinner, $btnConfirmar;
let estadoConfirmacionImpresion = null;

/**
 * ✅ OPTIMIZADA: Inicializa eventos del módulo de impresión de etiquetas
 */
function InicializaEventosImpEt() {
    // ═══════════════════════════════════════════════════════════════════
    // INICIALIZAR REFERENCIAS A ELEMENTOS DOM (UNA SOLA VEZ)
    // ═══════════════════════════════════════════════════════════════════
    $inputBusqueda = $("#Busqueda");
    $btnBuscar = $("#btnBusquedaBase");
    $spinner = $("#spnBuscarProducto");
    $btnConfirmar = $("#btnConfirmar");

    // ═══════════════════════════════════════════════════════════════════
    // EVENTO: CLICK EN BOTÓN BUSCAR
    // ═══════════════════════════════════════════════════════════════════
    $btnBuscar.on("click", function () {
        buscarProducto();
        return true;
    });

    // ═══════════════════════════════════════════════════════════════════
    // EVENTO: ENTER EN CAMPO DE BÚSQUEDA (REGISTRADO UNA SOLA VEZ)
    // ═══════════════════════════════════════════════════════════════════
    $inputBusqueda.on("keydown", function (e) {
        if (e.key === "Enter") {
            e.preventDefault();
            
            const valor = $(this).val().trim();
            if (valor) {
                buscarProducto();
            } else {
                mostrarNotificacion('Por favor, ingrese un valor para buscar', 'warning');
                $(this).trigger('focus');
            }
        }
    });

    // ═══════════════════════════════════════════════════════════════════
    // EVENTO: CAMBIO EN ESTADO DE FUNCIÓN
    // ═══════════════════════════════════════════════════════════════════
    $("#estadoFuncion").on("change", verificaEstadoImpEt);

    // ═══════════════════════════════════════════════════════════════════
    // EVENTO: CLICK EN BOTÓN CONFIRMAR (✅ NUEVO)
    // ═══════════════════════════════════════════════════════════════════
    $btnConfirmar.on("click", function (e) {
        e.preventDefault();
        confirmarCargaPrevia();
    });

    // ═══════════════════════════════════════════════════════════════════
    // CONFIGURAR EVENTOS DE ELIMINACIÓN Y LIMPIEZA
    // ═══════════════════════════════════════════════════════════════════
    configurarEventosEliminacionProducto();
    $("#btnLimpiarProductos").on("click", limpiarTodosLosProductos);

    // ═══════════════════════════════════════════════════════════════════
    // INICIALIZAR ESTADO DE BOTONES
    // ═══════════════════════════════════════════════════════════════════
    actualizarEstadoBotonImprimir();
    actualizarEstadoBotonConfirmar();

    // ═══════════════════════════════════════════════════════════════════
    // ANIMACIÓN INICIAL (DESPUÉS DE QUE EL DOM ESTÉ LISTO)
    // ═══════════════════════════════════════════════════════════════════
    setTimeout(function() {
        if ($inputBusqueda.length > 0) {
            $inputBusqueda.addClass('fade-in');
        }
    }, 200);
}

/**
 * ✅ COMPLETAMENTE OPTIMIZADA: Verifica estado y carga producto al grid
 * Proceso paso a paso:
 * 1. Valida que exista productoBase
 * 2. Valida campos requeridos (P_id, P_desc)
 * 3. Verifica duplicados
 * 4. Elimina fila vacía si es el primer producto
 * 5. Agrega el producto con HTML sanitizado
 * 6. Actualiza contador y estado de botones
 */
function verificaEstadoImpEt() {
    console.log("🔍 Verificando estado para agregar producto a etiquetas...");

    // ═══════════════════════════════════════════════════════════════════
    // PASO 1: VALIDAR EXISTENCIA DE productoBase
    // ═══════════════════════════════════════════════════════════════════
    if (!productoBase) {
        console.warn("⚠️ No hay producto en productoBase");
        return;
    }

    // ═══════════════════════════════════════════════════════════════════
    // PASO 2: VALIDAR CAMPOS REQUERIDOS
    // ═══════════════════════════════════════════════════════════════════
    const pId = (productoBase.P_id || productoBase.p_id || "").trim();
    const pDesc = (productoBase.P_desc || productoBase.p_desc || "Sin descripción").trim();

    if (!pId) {
        console.error("❌ El producto no tiene un ID válido");
        mostrarNotificacion("Error: Producto sin código identificador", "error");
        return;
    }

    console.log(`📦 Producto a agregar: [${pId}] ${pDesc}`);

    // ═══════════════════════════════════════════════════════════════════
    // PASO 3: VERIFICAR DUPLICADOS
    // ═══════════════════════════════════════════════════════════════════
    if (productoYaExisteEnGrid(pId)) {
        console.warn(`⚠️ Producto ${pId} ya existe en el grid`);
        mostrarNotificacion(`El producto ${pId} ya está agregado`, "warning");
        return;
    }

    // ═══════════════════════════════════════════════════════════════════
    // PASO 4: VERIFICAR LÍMITE MÁXIMO
    // ═══════════════════════════════════════════════════════════════════
    if (productosEtiquetaCargados.length >= MAX_PRODUCTOS_ETIQUETA) {
        console.warn(`⚠️ Se alcanzó el límite de ${MAX_PRODUCTOS_ETIQUETA} productos`);
        mostrarNotificacion(
            `Límite alcanzado: máximo ${MAX_PRODUCTOS_ETIQUETA} productos`,
            "warning"
        );
        return;
    }

    // ═══════════════════════════════════════════════════════════════════
    // PASO 5: ELIMINAR FILA VACÍA SI ES EL PRIMER PRODUCTO
    // ═══════════════════════════════════════════════════════════════════
    const esElPrimerProducto = productosEtiquetaCargados.length === 0;
    
    if (esElPrimerProducto) {
        console.log("🗑️ Eliminando fila vacía (primer producto)");
        $("#tbProductoEtiqueta tbody tr").filter(function () {
            // Buscar fila con colspan y mensaje de vacío
            return $(this).find("td[colspan]").length > 0;
        }).remove();
    }

    // ═══════════════════════════════════════════════════════════════════
    // PASO 6: CREAR Y AGREGAR FILA CON HTML SANITIZADO
    // ═══════════════════════════════════════════════════════════════════
    const filaHTML = crearFilaProductoEtiqueta(pId, pDesc);
    $("#tbProductoEtiqueta tbody").append(filaHTML);

    // ═══════════════════════════════════════════════════════════════════
    // PASO 7: ACTUALIZAR CONTROL INTERNO
    // ═══════════════════════════════════════════════════════════════════
    productosEtiquetaCargados.push({
        p_id: pId,
        p_desc: pDesc,
        timestamp: new Date().toISOString()
    });

    console.log(`✅ Producto agregado correctamente. Total: ${productosEtiquetaCargados.length}`);

    // ═══════════════════════════════════════════════════════════════════
    // PASO 8: ACTUALIZAR UI Y FEEDBACK VISUAL
    // ═══════════════════════════════════════════════════════════════════
    actualizarContadorProductos();
    actualizarEstadoBotonImprimir();
    actualizarEstadoBotonConfirmar();
    mostrarNotificacion(`Producto ${pId} agregado correctamente`, "success");

    // ✅ OPTIMIZACIÓN: Resaltar fila recién agregada
    resaltarUltimaFila();

    // ✅ LIMPIAR CAMPO DE BÚSQUEDA PARA SIGUIENTE PRODUCTO
    limpiarCampoBusqueda();
}

/**
 * ✅ NUEVA FUNCIÓN: Verifica si un producto ya existe en el grid
 * @param {string} pId - ID del producto
 * @returns {boolean} true si el producto ya existe
 */
function productoYaExisteEnGrid(pId) {
    return productosEtiquetaCargados.some(p => p.p_id === pId);
}

/**
 * ✅ NUEVA FUNCIÓN: Crea el HTML de una fila de producto (con sanitización)
 * @param {string} pId - ID del producto
 * @param {string} pDesc - Descripción del producto
 * @returns {string} HTML de la fila
 */
function crearFilaProductoEtiqueta(pId, pDesc) {
    // ✅ SANITIZACIÓN: Escapar caracteres HTML para prevenir XSS
    const pIdSeguro = escaparHTML(pId);
    const pDescSegura = escaparHTML(pDesc);

    // ✅ USAR TEMPLATE LITERALS para mejor legibilidad
    return `
        <tr data-p-id="${pIdSeguro}" class="fila-producto-etiqueta">
            <td class="text-center">${pIdSeguro}</td>
            <td>${pDescSegura}</td>
            <td class="text-center" style="width: 50px;">
                <button type="button" 
                        class="btn btn-sm btn-outline-danger btn-eliminar-producto-etiqueta"
                        data-p-id="${pIdSeguro}"
                        title="Eliminar producto">
                    <i class="bx bx-trash"></i>
                </button>
            </td>
        </tr>
    `;
}

/**
 * ✅ NUEVA FUNCIÓN: Escapa caracteres HTML para prevenir XSS
 * @param {string} texto - Texto a escapar
 * @returns {string} Texto sanitizado
 */
function escaparHTML(texto) {
    const div = document.createElement('div');
    div.textContent = String(texto);
    return div.innerHTML;
}

/**
 * ✅ NUEVA FUNCIÓN: Configura eventos de eliminación de productos
 */
function configurarEventosEliminacionProducto() {
    // ✅ USAR DELEGACIÓN DE EVENTOS para elementos dinámicos
    $(document).off("click.eliminarProductoEtiqueta", ".btn-eliminar-producto-etiqueta");
    
    $(document).on("click.eliminarProductoEtiqueta", ".btn-eliminar-producto-etiqueta", function (e) {
        e.preventDefault();
        e.stopPropagation();

        const $btn = $(this);
        const pId = $btn.data("p-id");
        const $fila = $btn.closest("tr");

        if (!pId) {
            console.error("❌ No se pudo obtener el ID del producto");
            return;
        }

        console.log(`🗑️ Eliminando producto: ${pId}`);
        eliminarProductoDelGrid(pId, $fila);
    });
}

/**
 * ✅ NUEVA FUNCIÓN: Elimina un producto del grid
 * @param {string} pId - ID del producto
 * @param {jQuery} $fila - Elemento jQuery de la fila
 */
function eliminarProductoDelGrid(pId, $fila) {
    // ✅ ANIMACIÓN SUAVE antes de eliminar
    $fila.fadeOut(300, function () {
        $(this).remove();

        // ✅ ACTUALIZAR CONTROL INTERNO
        productosEtiquetaCargados = productosEtiquetaCargados.filter(p => p.p_id !== pId);

        console.log(`✅ Producto ${pId} eliminado. Total: ${productosEtiquetaCargados.length}`);

        // ✅ SI NO QUEDAN PRODUCTOS, MOSTRAR FILA VACÍA
        if (productosEtiquetaCargados.length === 0) {
            mostrarFilaVacia();
        }

        // ✅ ACTUALIZAR UI
        actualizarContadorProductos();
        actualizarEstadoBotonImprimir();
        actualizarEstadoBotonConfirmar();
        mostrarNotificacion(`Producto ${pId} eliminado`, "info");
    });
}

/**
 * ✅ NUEVA FUNCIÓN: Muestra la fila vacía cuando no hay productos
 */
function mostrarFilaVacia() {
    const filaVacia = `
        <tr class="fila-vacia">
            <td colspan="3" class="text-center text-muted py-3">
                <i class="bx bx-info-circle me-1"></i>
                No hay productos cargados.
            </td>
        </tr>
    `;
    $("#tbProductoEtiqueta tbody").html(filaVacia);
}

/**
 * ✅ NUEVA FUNCIÓN: Limpia todos los productos del grid
 */
function limpiarTodosLosProductos() {
    if (productosEtiquetaCargados.length === 0) {
        mostrarNotificacion("No hay productos para limpiar", "info");
        return;
    }

    // ✅ CONFIRMACIÓN ANTES DE LIMPIAR
    if (typeof AbrirMensaje === "function") {
        AbrirMensaje(
            "CONFIRMAR LIMPIEZA",
            `¿Está seguro de eliminar todos los ${productosEtiquetaCargados.length} producto(s)?`,
            function (resp) {
                if (resp === "SI") {
                    ejecutarLimpiezaCompleta();
                }
                $("#msjModal").modal("hide");
            },
            true,
            ["Limpiar Todo", "Cancelar"],
            "warning!",
            null
        );
    } else {
        if (confirm(`¿Está seguro de eliminar todos los ${productosEtiquetaCargados.length} productos?`)) {
            ejecutarLimpiezaCompleta();
        }
    }
}

/**
 * ✅ NUEVA FUNCIÓN: Ejecuta la limpieza completa del grid
 */
function ejecutarLimpiezaCompleta() {
    const cantidadEliminada = productosEtiquetaCargados.length;

    // ✅ LIMPIAR ARRAY
    productosEtiquetaCargados = [];

    // ✅ MOSTRAR FILA VACÍA
    mostrarFilaVacia();

    // ✅ ACTUALIZAR UI
    actualizarContadorProductos();
    actualizarEstadoBotonImprimir();
    actualizarEstadoBotonConfirmar();

    console.log(`✅ ${cantidadEliminada} producto(s) eliminado(s) del grid`);
    mostrarNotificacion(`${cantidadEliminada} producto(s) eliminado(s)`, "success");
}

/**
 * ✅ NUEVA FUNCIÓN: Actualiza el contador de productos
 */
function actualizarContadorProductos() {
    const cantidad = productosEtiquetaCargados.length;
    
    // ✅ ACTUALIZAR BADGE O CONTADOR SI EXISTE EN EL HTML
    const $contador = $("#contadorProductosEtiqueta");
    if ($contador.length > 0) {
        $contador.text(cantidad);
    }

    // ✅ ACTUALIZAR TÍTULO DEL GRID
    const $titulo = $("#tituloGridProductos");
    if ($titulo.length > 0) {
        const textoPlural = cantidad === 1 ? "producto" : "productos";
        $titulo.text(`Productos para Etiquetas (${cantidad} ${textoPlural})`);
    }
}

/**
 * ✅ NUEVA FUNCIÓN: Actualiza el estado del botón imprimir
 */
function actualizarEstadoBotonImprimir() {
    const $btnImprimir = $("#btnImprimirEtiquetas");
    
    if ($btnImprimir.length === 0) return;

    const hayProductos = productosEtiquetaCargados.length > 0;

    $btnImprimir.prop("disabled", !hayProductos);

    if (hayProductos) {
        $btnImprimir.removeClass("btn-secondary").addClass("btn-primary");
    } else {
        $btnImprimir.removeClass("btn-primary").addClass("btn-secondary");
    }
}

/**
 * ✅ NUEVA FUNCIÓN: Actualiza el estado del botón confirmar
 * Se activa cuando hay al menos un producto en el array
 */
function actualizarEstadoBotonConfirmar() {
    // Verificar que la variable global esté inicializada
    if (typeof $btnConfirmar === 'undefined' || !$btnConfirmar || $btnConfirmar.length === 0) {
        // Intentar obtener la referencia si no existe
        $btnConfirmar = $("#btnConfirmar");
    }
    
    if ($btnConfirmar.length === 0) {
        console.warn("⚠️ Botón #btnConfirmar no encontrado en el DOM");
        return;
    }

    const hayProductos = productosEtiquetaCargados.length > 0;

    console.log(`🔘 Actualizando estado botón confirmar: ${hayProductos ? 'HABILITADO' : 'DESHABILITADO'} (${productosEtiquetaCargados.length} productos)`);

    // ✅ HABILITAR/DESHABILITAR según cantidad de productos
    $btnConfirmar.prop("disabled", !hayProductos);

    // ✅ CAMBIAR CLASES VISUALES
    if (hayProductos) {
        $btnConfirmar
            .removeClass("btn-secondary")
            .addClass("btn-success")
            .attr("title", `Confirmar ${productosEtiquetaCargados.length} producto(s) para impresión`);
    } else {
        $btnConfirmar
            .removeClass("btn-success")
            .addClass("btn-secondary")
            .attr("title", "Agregue productos para habilitar");
    }
}

/**
 * ✅ NUEVA FUNCIÓN: Resalta la última fila agregada
 */
function resaltarUltimaFila() {
    const $ultimaFila = $("#tbProductoEtiqueta tbody tr:last");
    
    if ($ultimaFila.length === 0) return;

    // ✅ ANIMACIÓN: Resaltar temporalmente
    $ultimaFila.addClass("fila-recien-agregada");
    
    setTimeout(() => {
        $ultimaFila.removeClass("fila-recien-agregada");
    }, 1500);
}

/**
 * ✅ NUEVA FUNCIÓN: Limpia el campo de búsqueda
 */
function limpiarCampoBusqueda() {
    if ($inputBusqueda && $inputBusqueda.length > 0) {
        $inputBusqueda.val("").trigger("focus");
    }
}

/**
 * ✅ NUEVA FUNCIÓN: Muestra notificaciones al usuario
 * @param {string} mensaje - Mensaje a mostrar
 * @param {string} tipo - Tipo de notificación: "success", "error", "warning", "info"
 */
function mostrarNotificacion(mensaje, tipo = "info") {
    // ✅ USAR SISTEMA DE NOTIFICACIONES DEL PROYECTO SI EXISTE
    if (typeof ControlaMensajeSuccess === "function") {
        switch (tipo) {
            case "success":
                ControlaMensajeSuccess(mensaje);
                break;
            case "error":
                ControlaMensajeError(mensaje);
                break;
            case "warning":
                ControlaMensajeWarning(mensaje);
                break;
            case "info":
            default:
                ControlaMensajeInfo(mensaje);
                break;
        }
    } else {
        // ✅ FALLBACK: Console log si no hay sistema de notificaciones
        console.log(`[${tipo.toUpperCase()}] ${mensaje}`);
    }
}

/**
 * ✅ NUEVA FUNCIÓN: Obtiene los IDs de productos cargados (útil para imprimir)
 * @returns {Array<string>} Array de IDs de productos
 */
function obtenerProductosParaImprimir() {
    return productosEtiquetaCargados.map(p => p.p_id);
}

/**
 * ✅ NUEVA FUNCIÓN: Obtiene el objeto completo de productos cargados
 * @returns {Array<Object>} Array de objetos producto
 */
function obtenerProductosCompletosParaImprimir() {
    return [...productosEtiquetaCargados]; // Retornar copia para evitar mutaciones
}

// ════════════════════════════════════════════════════════════════════════════
// ✅ NUEVA FUNCIÓN PRINCIPAL: CONFIRMAR CARGA PREVIA
// ════════════════════════════════════════════════════════════════════════════

/**
 * ✅ FUNCIÓN PRINCIPAL: Confirma la carga previa de productos para etiquetas
 * 
 * FLUJO DE PROCESO:
 * 1. Validar que existan productos
 * 2. Validar que la variable usuarioAuth esté disponible
 * 3. Construir JSON con formato: [{"usu_id": "user", "p_id": "id", "p_desc": "desc"}]
 * 4. Mostrar confirmación al usuario
 * 5. Enviar datos al servidor
 * 6. Procesar respuesta
 * 7. Limpiar grid y actualizar UI
 */
function confirmarCargaPrevia() {
    console.log("🚀 Iniciando proceso de confirmación de carga previa...");

    // ═══════════════════════════════════════════════════════════════════
    // PASO 1: VALIDAR QUE EXISTAN PRODUCTOS
    // ═══════════════════════════════════════════════════════════════════
    if (!productosEtiquetaCargados || productosEtiquetaCargados.length === 0) {
        console.warn("⚠️ No hay productos para confirmar");
        mostrarNotificacion("Debe agregar al menos un producto antes de confirmar", "warning");
        return;
    }

    // ═══════════════════════════════════════════════════════════════════
    // PASO 2: VALIDAR VARIABLE GLOBAL usuarioAuth
    // ═══════════════════════════════════════════════════════════════════
    if (typeof usuarioAuth === 'undefined' || !usuarioAuth) {
        console.error("❌ Variable usuarioAuth no está definida");
        mostrarNotificacion("Error: Usuario no identificado. Recargue la página", "error");
        return;
    }

    console.log(`👤 Usuario autenticado: ${usuarioAuth}`);

    // ═══════════════════════════════════════════════════════════════════
    // PASO 3: CONSTRUIR JSON CON FORMATO REQUERIDO
    // ═══════════════════════════════════════════════════════════════════
    const productosParaEnviar = productosEtiquetaCargados.map(producto => ({
        usu_id: usuarioAuth,
        p_id: producto.p_id,
        p_desc: producto.p_desc
    }));

    const jsonProductos = JSON.stringify(productosParaEnviar);

    console.log(`📦 JSON generado para ${productosParaEnviar.length} producto(s):`);
    console.log(jsonProductos);

    // ═══════════════════════════════════════════════════════════════════
    // PASO 4: MOSTRAR CONFIRMACIÓN AL USUARIO
    // ═══════════════════════════════════════════════════════════════════
    const mensajeConfirmacion = `
        ¿Está seguro de confirmar la carga previa de ${productosParaEnviar.length} producto(s)?
        <br><br>
        <small class="text-muted">
            Los productos serán procesados para impresión de etiquetas.
        </small>
    `;

    if (typeof AbrirMensaje === "function") {
        AbrirMensaje(
            "CONFIRMAR CARGA PREVIA",
            mensajeConfirmacion,
            function (resp) {
                if (resp === "SI") {
                    enviarCargaPreviaAlServidor(jsonProductos);
                }
                $("#msjModal").modal("hide");
            },
            true,
            ["Confirmar", "Cancelar"],
            "info!",
            null
        );
    } else {
        // Fallback si AbrirMensaje no está disponible
        if (confirm(`¿Está seguro de confirmar la carga previa de ${productosParaEnviar.length} productos?`)) {
            enviarCargaPreviaAlServidor(jsonProductos);
        }
    }
}

/**
 * ✅ NUEVA FUNCIÓN: Envía la carga previa al servidor
 * @param {string} jsonProductos - JSON string con los productos
 */
function enviarCargaPreviaAlServidor(jsonProductos) {
    if (estadoConfirmacionImpresion !== null) {
        console.warn("[Pocket][ImpresionEtiquetas] Se ignora una confirmación duplicada");
        return false;
    }

    console.info("[Pocket][ImpresionEtiquetas] Preparando confirmación");

    // ═══════════════════════════════════════════════════════════════════
    // VALIDAR URL DEL ENDPOINT
    // ═══════════════════════════════════════════════════════════════════
    if (typeof confirmarCargaPreviaUrl === 'undefined' || !confirmarCargaPreviaUrl) {
        console.error("❌ URL confirmarCargaPreviaUrl no está definida");
        mostrarNotificacion("Error de configuración: URL no definida", "error");
        return false;
    }

    // ═══════════════════════════════════════════════════════════════════
    // PREPARAR DATOS PARA ENVÍO
    // ═══════════════════════════════════════════════════════════════════
    const datosEnvio = {
        json: jsonProductos
    };

    estadoConfirmacionImpresion = IniciarConfirmacionSegura(
        $btnConfirmar,
        "Espere... se está confirmando la impresión de etiquetas...",
        "Procesando..."
    );

    if (estadoConfirmacionImpresion === null) {
        return false;
    }

    console.info("[Pocket][ImpresionEtiquetas] Enviando productos", {
        cantidad: productosEtiquetaCargados.length
    });

    // ═══════════════════════════════════════════════════════════════════
    // REALIZAR PETICIÓN AJAX
    // ═══════════════════════════════════════════════════════════════════
    try {
        $.ajax({
            url: confirmarCargaPreviaUrl,
            type: "POST",
            contentType: "application/x-www-form-urlencoded; charset=UTF-8",
            data: datosEnvio,
            dataType: "json",
            success: function (response) {
                procesarRespuestaConfirmacion(response);
            },
            error: function (xhr, status, error) {
                console.error("[Pocket][ImpresionEtiquetas] Error de comunicación durante la confirmación", {
                    estadoHttp: xhr ? xhr.status : null,
                    detalleHttp: status,
                    error: error
                });
                FinalizarConfirmacionImpresion();

                let mensajeError = "Error al confirmar la carga previa.";
                if (xhr.status === 401) {
                    mensajeError = "Sesión expirada. Por favor, inicie sesión nuevamente.";
                }
                else if (xhr.status === 0) {
                    mensajeError = "No se pudo conectar con el servidor. Verifique su conexión.";
                }
                else if (xhr.responseJSON) {
                    mensajeError = xhr.responseJSON.msg || xhr.responseJSON.message || mensajeError;
                }

                mostrarNotificacion(mensajeError, "error");
            }
        });
    }
    catch (error) {
        console.error("[Pocket][ImpresionEtiquetas] Error inesperado al iniciar la confirmación", error);
        FinalizarConfirmacionImpresion();
        mostrarNotificacion("No se pudo iniciar la confirmación. Intente nuevamente.", "error");
    }

    return false;
}

/**
 * ✅ NUEVA FUNCIÓN: Procesa la respuesta del servidor
 * @param {Object} response - Respuesta del servidor
 */
function procesarRespuestaConfirmacion(response) {
    FinalizarConfirmacionImpresion();
    console.info("[Pocket][ImpresionEtiquetas] Procesando respuesta", response);

    // ═══════════════════════════════════════════════════════════════════
    // VALIDAR ESTRUCTURA DE RESPUESTA
    // ═══════════════════════════════════════════════════════════════════
    if (!response) {
        console.error("❌ Respuesta vacía del servidor");
        mostrarNotificacion("Error: Respuesta inválida del servidor", "error");
        return;
    }

    // ═══════════════════════════════════════════════════════════════════
    // PROCESAR SEGÚN TIPO DE RESPUESTA
    // ═══════════════════════════════════════════════════════════════════
    if (response.ok === true && response.error === false) {
        // ✅ RESPUESTA EXITOSA
        console.log("✅ Carga previa confirmada exitosamente");
        
        const mensaje = response.msg || "La carga previa se realizó exitosamente";
        
        mostrarNotificacion(mensaje, "success");
        
        // Limpiar inmediatamente evita que la misma carga pueda confirmarse otra vez.
        ejecutarLimpiezaCompleta();
        
    } else if (response.error === true) {
        // ❌ ERROR DEL SERVIDOR
        console.error("❌ Error reportado por el servidor");
        
        const mensajeError = response.msg || "Error al procesar la carga previa";
        
        mostrarNotificacion(mensajeError, "error");
    } else if (response.warn === true) {
        // ⚠️ ADVERTENCIA DEL SERVIDOR
        console.warn("⚠️ Advertencia reportada por el servidor");
        
        const mensajeWarn = response.msg || "Advertencia al procesar la carga previa";
        
        mostrarNotificacion(mensajeWarn, "warning");
    } else {
        // ❓ RESPUESTA NO RECONOCIDA
        console.warn("⚠️ Respuesta no reconocida del servidor");
        
        mostrarNotificacion("Respuesta inesperada del servidor", "warning");
    }
}

function FinalizarConfirmacionImpresion() {
    const contexto = estadoConfirmacionImpresion;
    estadoConfirmacionImpresion = null;
    FinalizarConfirmacionSegura(contexto);
}

// ════════════════════════════════════════════════════════════════
// FUNCIONES GLOBALES PARA CONTROL DE SPINNER Y BÚSQUEDA
// ════════════════════════════════════════════════════════════════

/**
 * ✅ FUNCIÓN GLOBAL: Muestra spinner de búsqueda
 */
window.mostrarSpinnerBusqueda = function() {
    if ($btnBuscar && $btnBuscar.length > 0) {
        $btnBuscar.prop('disabled', true);
    }
    if ($spinner && $spinner.length > 0) {
        $spinner.removeClass('d-none');
    }
};

/**
 * ✅ FUNCIÓN GLOBAL: Oculta spinner de búsqueda
 */
window.ocultarSpinnerBusqueda = function() {
    if ($spinner && $spinner.length > 0) {
        $spinner.addClass('d-none');
    }
    if ($btnBuscar && $btnBuscar.length > 0) {
        $btnBuscar.prop('disabled', false);
    }
};

/**
 * ✅ FUNCIÓN GLOBAL: Limpia campo de búsqueda
 */
window.limpiarBusqueda = function() {
    if ($inputBusqueda && $inputBusqueda.length > 0) {
        $inputBusqueda.val('').trigger('focus');
    }
};
