$(function () {
    inicializaPantallaConteo();
    iniciaEventosConteo();
});

function inicializaPantallaConteo() {
    $("#nnProducto").hide();
}

function iniciaEventosConteo() {
    $("#btnBusquedaBase").off("click").on("click", function () {
        InicializaControlesConteo();
        buscarProducto("INV");
    });
    
    // Delegación de eventos para botones de eliminación (mejor rendimiento)
    $("#tbGridConteoProductos").off("click", ".btn-eliminar-producto")
        .on("click", ".btn-eliminar-producto", function (e) {
            e.preventDefault();
            eliminarProductoDelConteo($(this));
        });
    
    $("#estadoFuncion").on("change", verificaEstadoCont); 
    $("#btnCargaConteo").on("click", cargarConteoEnGrid);

    $(".inputEditable").on("keypress", analizaEnterInput);

    $("#btnConfirmarConteo").on("click", confirmarConteo);
   
}

function confirmarConteo() {
    // Obtener productos del grid
    const productos = obtenerProductosDelGrid();
    
    // Validar que haya productos
    if (productos.length === 0) {
        AbrirMensaje(
            "Sin productos",
            "Debe agregar al menos un producto antes de confirmar el conteo",
            function() { $("#msjModal").modal("hide"); },
            false,
            ["Aceptar"],
            "warn!",
            null
        );
        return;
    }
    
    // Confirmación del usuario
    AbrirMensaje(
        "¿Confirmar conteo?",
        `¿Está seguro de confirmar el conteo con <strong>${productos.length}</strong> producto(s)?<br><br>Esta acción no se puede deshacer.`,
        function(respuesta) {
            $("#msjModal").modal("hide");
            if (respuesta === "SI") {
                ejecutarConfirmacionConteo(productos);
            }
        },
        true,
        ["Sí, confirmar", "Cancelar"],
        "warn!",
        null
    );
}

/**
 * Obtiene los productos del grid y los convierte al formato InventarioConteoDto
 * @returns {Array} Array de productos en formato DTO
 */
function obtenerProductosDelGrid() {
    const productos = [];
    const $filas = $("#tbGridConteoProductos tbody tr[data-p-id]");
    
    $filas.each(function() {
        const $fila = $(this);
        const pId = $fila.data("p-id");
        const boxId = $fila.data("box-id") || "";
        const cargaNro = parseInt($fila.data("carga-nro"), 10) || 0;
        const pDesc = $fila.find("td:eq(1)").text();
        const bultos = parseInt($fila.find("td:eq(2)").text(), 10) || 0;
        const unidades = parseInt($fila.find("td:eq(3)").text(), 10) || 0;
        const cantidad = parseFloat($fila.find("td:eq(4)").text()) || 0;
        
        productos.push({
            p_id: pId,
            p_desc: pDesc,
            box_id: boxId,
            carga_nro: cargaNro,
            usu_id: "", // Se asigna en el servidor
            invd_unidad_pres: unidades,
            invd_bulto: bultos,
            invd_unidad_suelta: 0,
            invd_cantidad: cantidad
        });
    });
    
    return productos;
}

/**
 * Ejecuta la confirmación del conteo vía AJAX
 * @param {Array} productos - Array de productos a enviar
 */
function ejecutarConfirmacionConteo(productos) {
    // Deshabilitar botón durante la operación
    const $btn = $("#btnConfirmarConteo");
    const textoOriginal = $btn.html();
    $btn.prop("disabled", true)
        .html('<span class="spinner-border spinner-border-sm me-2"></span>Confirmando...');
    
    // Construir el request DTO
    const request = {
        inv_nro: estado.inv_nro || "",
        tipo: estado.tipo || "",
        tipo_id: estado.tipo_id || "",
        usu_id: "", // Se asigna en el servidor
        p_id: null,
        json: productos
    };
    
    $.ajax({
        url: "/Gestion/Inventario/ConfirmarConteo",
        type: "POST",
        contentType: "application/json",
        data: JSON.stringify(request),
        headers: {
            "RequestVerificationToken": $('input[name="__RequestVerificationToken"]').val()
        },
        success: function(response) {
            if (response.error) {
                // Error del servidor
                AbrirMensaje(
                    "Error",
                    response.msg || "Error al confirmar el conteo",
                    function() { $("#msjModal").modal("hide"); },
                    false,
                    ["Aceptar"],
                    "error!",
                    null
                );
            } else if (response.warn) {
                // Advertencia del negocio
                if (response.auth) {
                    // Sesión expirada
                    AbrirMensaje(
                        "Sesión expirada",
                        response.msg,
                        function() {
                            window.location.href = "/Account/Login";
                        },
                        false,
                        ["Aceptar"],
                        "warn!",
                        null
                    );
                } else {
                    AbrirMensaje(
                        "Advertencia",
                        response.msg,
                        function() { $("#msjModal").modal("hide"); },
                        false,
                        ["Aceptar"],
                        "warn!",
                        null
                    );
                }
            } else {
                // Éxito
                AbrirMensaje(
                    "Conteo confirmado",
                    response.msg || "El conteo se confirmó correctamente",
                    function() {
                        window.location.href = "/Gestion/Inventario/Index";
                    },
                    false,
                    ["Aceptar"],
                    "succ!",
                    null
                );
            }
        },
        error: function(xhr) {
            console.error("Error al confirmar conteo:", xhr);
            
            let mensajeError = "Error de conexión al confirmar el conteo";
            
            if (xhr.status === 400) {
                mensajeError = "Datos inválidos. Verifique los productos agregados.";
            } else if (xhr.status === 401 || xhr.status === 403) {
                mensajeError = "No tiene permisos para confirmar el conteo.";
            } else if (xhr.status === 500) {
                mensajeError = "Error interno del servidor. Contacte al administrador.";
            }
            
            AbrirMensaje(
                "Error de conexión",
                mensajeError,
                function() { $("#msjModal").modal("hide"); },
                false,
                ["Aceptar"],
                "error!",
                null
            );
        },
        complete: function() {
            // Restaurar botón
            $btn.prop("disabled", false).html(textoOriginal);
        }
    });
}

function cargarConteoEnGrid() {
    // Obtener valores de los controles con validación
    const pId = $("#pId").val()?.trim();
    const pDesc = productoBase?.p_desc || "";
    const btos = parseInt($("#btos").val(), 10) || 0;
    const uns = parseInt($("#uns").val(), 10) || 0;
    const cantidad = btos * uns;
    
    // Validación de datos requeridos
    if (!pId || !pDesc) {
        AbrirMensaje(
            "Datos incompletos",
            "Debe seleccionar un producto antes de agregar al conteo",
            function() { $("#msjModal").modal("hide"); },
            false,
            ["Aceptar"],
            "warn!",
            null
        );
        return;
    }
    
    if (cantidad <= 0) {
        AbrirMensaje(
            "Cantidad inválida",
            "Debe ingresar bultos y unidades válidos para calcular la cantidad",
            function() { $("#msjModal").modal("hide"); },
            false,
            ["Aceptar"],
            "warn!",
            null
        );
        return;
    }
    
    // Verificar si el producto ya existe en el grid
    const $filaExistente = $(`#tbGridConteoProductos tbody tr[data-p-id="${pId}"]`);
    if ($filaExistente.length > 0) {
        AbrirMensaje(
            "Producto duplicado",
            `El producto <strong>${pDesc}</strong> ya está en el conteo. ¿Desea actualizar las cantidades?`,
            function(respuesta) {
                $("#msjModal").modal("hide");
                if (respuesta === "SI") {
                    actualizarFilaExistente($filaExistente, btos, uns, cantidad);
                }
            },
            true,
            ["Sí, actualizar", "Cancelar"],
            "warn!",
            null
        );
        return;
    }
    
    // Agregar nueva fila al grid
    agregarNuevaFilaAlGrid(pId, pDesc, btos, uns, cantidad);
    
    // Limpiar controles después de agregar
    InicializaControlesConteo();
    
    // Enfocar en el campo de búsqueda
    $("#Busqueda").trigger("focus");
}

/**
 * Actualiza una fila existente con nuevas cantidades
 */
function actualizarFilaExistente($fila, btos, uns, cantidad) {
    $fila.find("td:eq(2)").text(btos);
    $fila.find("td:eq(3)").text(uns);
    $fila.find("td:eq(4)").text(cantidad.toFixed(2));
    
    // Efecto visual de actualización
    $fila.addClass("table-warning");
    setTimeout(() => $fila.removeClass("table-warning"), 1000);
}

/**
 * Agrega una nueva fila al grid de conteo
 */
function agregarNuevaFilaAlGrid(pId, pDesc, btos, uns, cantidad) {
    const $tbody = $("#tbGridConteoProductos tbody");
    
    // Obtener datos adicionales del contexto si están disponibles
    const boxId = $("#boxId").val() || "";
    const cargaNro = $("#cargaNro").val() || 0;
    
    // Remover mensaje de "sin datos" si existe
    $tbody.find("tr td[colspan]").closest("tr").remove();
    
    // Crear nueva fila con template literal para mejor rendimiento
    const nuevaFila = `
        <tr data-p-id="${pId}" data-box-id="${boxId}" data-carga-nro="${cargaNro}">
            <td>${pId}</td>
            <td>${pDesc}</td>
            <td class="text-end">${btos}</td>
            <td class="text-end">${uns}</td>
            <td class="text-end">${cantidad.toFixed(2)}</td>
            <td class="text-center">
                <button type="button" 
                        class="btn btn-danger btn-sm btn-eliminar-producto" 
                        data-p-id="${pId}"
                        data-p-desc="${pDesc}"
                        title="Eliminar producto del conteo">
                    <i class="bx bx-trash"></i>
                </button>
            </td>
        </tr>
    `;
    
    // Insertar al inicio de la tabla para mejor UX
    $tbody.prepend(nuevaFila);
    
    // Efecto visual de nuevo elemento
    const $nuevaFila = $tbody.find("tr:first");
    $nuevaFila.hide().fadeIn(400);
    
    // Actualizar contador de productos
    actualizarContadorProductos();
    
    // Mostrar footer si estaba oculto
    const $footer = $(".grid-golden-footer");
    if ($footer.is(":hidden")) {
        $footer.fadeIn();
    }
}

function InicializaControlesConteo() {
    $("#pId").val("");
    $("#btos").val("");
    $("#uns").val("");
    $("#nnProducto").hide();
}

function cargaProductoEnControl() {
    $("#pId").val(productoBase.p_id);
    $("#nnProducto").text(productoBase.p_desc).show();
    $("#btos, #uns").prop("readonly", false);
    $("#btnCargaConteo").prop("disabled", false);

    setTimeout(() => {
        $("#Busqueda").val("");
        $("#btos").trigger("focus");
    }, 200);
}

function verificaEstadoCont() {
    cargaProductoEnControl();
}

/**
 * Elimina un producto del conteo de inventario con confirmación
 * @param {jQuery} $btn - Botón de eliminación clickeado
 */
function eliminarProductoDelConteo($btn) {
    const pId = $btn.data("p-id");
    const pDesc = $btn.data("p-desc");
    const $fila = $btn.closest("tr");
    
    // Validación de datos
    if (!pId) {
        AbrirMensaje(
            "Error",
            "No se pudo identificar el producto a eliminar",
            function() {
                $("#msjModal").modal("hide");
            },
            false,
            ["Aceptar"],
            "error!",
            null
        );
        return;
    }
    
    // Confirmación usando AbrirMensaje
    AbrirMensaje(
        "¿Eliminar producto?",
        `¿Está seguro de eliminar el producto <strong>${pDesc}</strong> (${pId}) del conteo?<br><br>Esta acción no se puede deshacer.`,
        function(respuesta) {
            $("#msjModal").modal("hide");
            if (respuesta === "SI") {
                ejecutarEliminacionProducto(pId, pDesc, $fila);
            }
        },
        true, // Es confirmación
        ["Sí, eliminar", "Cancelar"],
        "warn!",
        null
    );
}

/**
 * Ejecuta la eliminación del producto vía AJAX
 * @param {string} pId - ID del producto
 * @param {string} pDesc - Descripción del producto
 * @param {jQuery} $fila - Fila de la tabla a eliminar
 */
function ejecutarEliminacionProducto(pId, pDesc, $fila) {
    // Deshabilitar botón para evitar clicks múltiples
    const $btn = $fila.find(".btn-eliminar-producto");
    $btn.prop("disabled", true).html('<span class="spinner-border spinner-border-sm"></span>');
    
    // Obtener datos adicionales del conteo
    const boxId = $fila.data("box-id") || "";
    const cargaNro = $fila.data("carga-nro") || 0;

    // Animación de eliminación
    $fila.fadeOut(400, function () {
        $(this).remove();
        actualizarContadorProductos();
    });

    //$.ajax({
    //    url: "/Gestion/Inventario/EliminarProductoConteo",
    //    type: "POST",
    //    data: {
    //        p_id: pId,
    //        box_id: boxId,
    //        carga_nro: cargaNro
    //    },
    //    headers: {
    //        "RequestVerificationToken": $('input[name="__RequestVerificationToken"]').val()
    //    },
    //    success: function (response) {
    //        if (response.success) {
    //            // Animación de eliminación
    //            $fila.fadeOut(400, function () {
    //                $(this).remove();
    //                actualizarContadorProductos();
    //            });
                
    //            // Mensaje de éxito
    //            AbrirMensaje(
    //                "Eliminado",
    //                response.message || `Producto <strong>${pDesc}</strong> eliminado correctamente del conteo`,
    //                function() {
    //                    $("#msjModal").modal("hide");
    //                },
    //                false,
    //                ["Aceptar"],
    //                "succ!",
    //                null
    //            );
    //        } else {
    //            // Restaurar botón en caso de error
    //            $btn.prop("disabled", false).html('<i class="bx bx-trash"></i>');
                
    //            AbrirMensaje(
    //                "Error al eliminar",
    //                response.message || "No se pudo eliminar el producto del conteo",
    //                function() {
    //                    $("#msjModal").modal("hide");
    //                },
    //                false,
    //                ["Aceptar"],
    //                "error!",
    //                null
    //            );
    //        }
    //    },
    //    error: function (xhr) {
    //        console.error("Error al eliminar producto:", xhr);
            
    //        // Restaurar botón en caso de error
    //        $btn.prop("disabled", false).html('<i class="bx bx-trash"></i>');
            
    //        // Determinar mensaje de error
    //        let mensajeError = "Error de conexión al eliminar el producto. Por favor, intente nuevamente.";
            
    //        if (xhr.status === 404) {
    //            mensajeError = "El producto no fue encontrado en el sistema.";
    //        } else if (xhr.status === 403) {
    //            mensajeError = "No tiene permisos para eliminar este producto.";
    //        } else if (xhr.status === 500) {
    //            mensajeError = "Error interno del servidor. Contacte al administrador.";
    //        }
            
    //        AbrirMensaje(
    //            "Error de conexión",
    //            mensajeError,
    //            function() {
    //                $("#msjModal").modal("hide");
    //            },
    //            false,
    //            ["Aceptar"],
    //            "error!",
    //            null
    //        );
    //    }
    //});
}

/**
 * Actualiza el contador de productos en el footer
 */
function actualizarContadorProductos() {
    const $tbody = $("#tbGridConteoProductos tbody");
    const cantidadProductos = $tbody.find("tr[data-p-id]").length;
    
    if (cantidadProductos === 0) {
        // Mostrar mensaje de sin datos
        $tbody.html(`
            <tr>
                <td colspan="6" class="text-center">
                    <div class="alert alert-info mb-0" role="alert">
                        <i class="bx bx-info-circle"></i> No hay productos para mostrar en el conteo.
                    </div>
                </td>
            </tr>
        `);
        $("#btnConfirmarConteo").prop("disabled", true);
        // Ocultar footer
        $(".grid-golden-footer").fadeOut();
    } else {
        $("#btnConfirmarConteo").prop("disabled", false);

        // Actualizar contador
        $(".grid-golden-footer strong").text(cantidadProductos);
    }
}