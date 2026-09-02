$(function () {
    inicializacionEventosOrCtl();

    cargarProductosOrCtl(orCompte);
});

function inicializacionEventosOrCtl() {
    $("#btnConfirmar").on("click", confirmarProductosOrCtl);
}

var estadoConfirmacionControlOr = null;

function confirmarProductosOrCtl() {
    if (estadoConfirmacionControlOr !== null) {
        console.warn("[Pocket][ControlOR] Se ignora una confirmación duplicada");
        return false;
    }

    var datos = {};
    estadoConfirmacionControlOr = IniciarConfirmacionSegura(
        "#btnConfirmar",
        "Espere... se están confirmando los productos controlados...",
        "Procesando..."
    );

    if (estadoConfirmacionControlOr === null) {
        return false;
    }

    console.info("[Pocket][ControlOR] Enviando productos controlados");

    try {
        $.ajax({
            url: ReguardaProductosEnServerOrCtlUrl,
            type: 'POST',
            contentType: 'application/json; charset=utf-8',
            dataType: 'json',
            data: JSON.stringify(datos),
            success: function (obj) {
                FinalizarConfirmacionControlOr();

                var mensaje = obj.msg || obj.message || "Productos cargados exitosamente";
                var esError = obj.error === true || obj.success === false;
                console.info("[Pocket][ControlOR] Respuesta de confirmación", {
                    error: esError,
                    advertencia: obj.warn === true,
                    exitoso: obj.success === true,
                    mensaje: mensaje
                });

                if (esError) {
                    AbrirMensaje("Error", mensaje, function () {
                        $("#msjModal").modal("hide");
                    }, false, ["Aceptar"], "error!", null);
                }
                else if (obj.warn === true) {
                    AbrirMensaje("Advertencia", mensaje, function () {
                        $("#msjModal").modal("hide");
                    }, false, ["Aceptar"], "warn!", null);
                }
                else {
                    console.info("[Pocket][ControlOR] Productos controlados confirmados correctamente");
                    AbrirMensaje("Éxito", mensaje, function () {
                        $("#msjModal").modal("hide");
                        $("#btnConfirmar").hide("fast");
                    }, false, ["Aceptar"], "succ!", null);
                }
            },
            error: function (xhr, status, error) {
                FinalizarConfirmacionControlOr();
                console.error("[Pocket][ControlOR] Error de comunicación durante la confirmación", {
                    estadoHttp: xhr ? xhr.status : null,
                    detalleHttp: status,
                    error: error
                });

                var mensajeError = "Error de conexión al cargar los productos";
                if (xhr.responseJSON) {
                    mensajeError = xhr.responseJSON.msg || xhr.responseJSON.message || mensajeError;
                }
                else if (xhr.responseText) {
                    try {
                        var response = JSON.parse(xhr.responseText);
                        mensajeError = response.msg || response.message || mensajeError;
                    }
                    catch (parseError) {
                        console.error("[Pocket][ControlOR] No se pudo interpretar la respuesta de error", parseError);
                    }
                }

                AbrirMensaje("Error", mensajeError, function () {
                    $("#msjModal").modal("hide");
                }, false, ["Aceptar"], "error!", null);
            }
        });
    }
    catch (error) {
        console.error("[Pocket][ControlOR] Error inesperado al iniciar la confirmación", error);
        FinalizarConfirmacionControlOr();
        ControlaMensajeError("No se pudo iniciar la confirmación. Intente nuevamente.");
    }

    return false;
}

function FinalizarConfirmacionControlOr() {
    var contexto = estadoConfirmacionControlOr;
    estadoConfirmacionControlOr = null;
    FinalizarConfirmacionSegura(contexto);
}

function cargarProductosOrCtl(orCompte) {
    const url = CargaProductosOrCtlUrl;
    const datos = { or_compte: orCompte };

    $.ajax({
        url: url,
        type: 'POST',
        data: datos,
        beforeSend: function () {
            mostrarCargando();
        },
        success: function (response) {
            ocultarCargando();

            if (!response.success) {
                mostrarError(response.message || "Error al cargar productos");
                return;
            }

            if (response.data) {
                renderizarProductos(response.data);
            }
        },
        error: function (xhr, status, error) {
            ocultarCargando();
            mostrarError("Error de conexión al cargar productos");
            console.error("Error:", error);
        }
    });
}

function renderizarProductos(productos) {
    const tbody = $("#tbGridListaControl tbody");
    tbody.empty();

    if (!productos || productos.length === 0) {
        tbody.append('<tr><td colspan="6" class="text-center">No hay productos disponibles</td></tr>');
        $("#btnConfirmar").hide("fast");
        return;
    }
    else {
        $("#btnConfirmar").show("fast");
    }

    productos.forEach(function (producto) {
        const fila = construirFila(producto);
        tbody.append(fila);
    });
}

/**
 * Construye una fila de producto con botón de eliminación
 * @param {object} producto - Objeto producto con sus propiedades
 * @returns {jQuery} Fila de tabla jQuery
 */
function construirFila(producto) {
    const tr = $('<tr></tr>');
    tr.attr('data-pid', producto.p_id);
    tr.attr('data-item', producto.item);

    // Columnas de datos
    tr.append(`<td class="text-start">${producto.p_id || ''}</td>`);
    tr.append(`<td class="text-start">${producto.p_desc || ''}</td>`);
    tr.append(`<td class="text-start">${producto.bulto || 0}</td>`);
    tr.append(`<td class="text-start">${FormatearCantidadProducto(producto.us, producto.up_id)}</td>`);
    tr.append(`<td class="text-start">${FormatearCantidadProducto(producto.cantidad, producto.up_id)}</td>`);

    // Columna de acción (botón eliminar)
    const tdAccion = $('<td class="text-center"></td>');
    const btnEliminar = $('<button></button>')
        .addClass('btn btn-danger btn-sm')
        .attr('type', 'button')
        .attr('title', 'Eliminar producto')
        .attr('data-pid', producto.p_id)
        .attr('data-pdesc', producto.p_desc)
        .html('<i class="bx bx-trash bx-xs"></i>')
        .on('click', function (e) {
            e.preventDefault();
            e.stopPropagation();
            const pid = $(this).data('pid');
            const pdesc = $(this).data('pdesc');
            confirmarEliminacionProducto(pid, pdesc);
        });

    tdAccion.append(btnEliminar);
    tr.append(tdAccion);

    return tr;
}

function mostrarCargando() {
    const tbody = $("#tbGridListaControl tbody");
    tbody.html('<tr><td colspan="6" class="text-center"><i class="bx bx-loader-alt bx-spin"></i> Cargando...</td></tr>');
}

function ocultarCargando() {
    // Se limpia al renderizar productos
}

function mostrarError(mensaje) {
    const tbody = $("#tbGridListaControl tbody");
    tbody.html(`<tr><td colspan="6" class="text-center text-danger">${mensaje}</td></tr>`);
}

/**
* Muestra confirmación antes de eliminar un producto
* @param {string} pId - ID del producto a eliminar
* @param {string} pDesc - Descripción del producto
*/
function confirmarEliminacionProducto(pId, pDesc) {
    const mensajeConfirmacion = `¿Está seguro que desea eliminar el producto <strong>${pDesc || pId}</strong>?`;

    AbrirMensaje(
        "Confirmar Eliminación",
        mensajeConfirmacion,
        function () {
            $("#msjModal").modal("hide");
            eliminarProductoOrCtl(pId);
        },
        true, // Mostrar botón cancelar
        ["Sí, Eliminar", "Cancelar"],
        "warn!",
        null
    );
}

/**
 * Elimina un producto de la lista OR Control
 * @param {string} pId - ID del producto a eliminar
 */
function eliminarProductoOrCtl(pId) {
    if (!pId) {
        console.error("❌ No se proporcionó ID de producto para eliminar");
        return;
    }

    console.log(`🗑️ Eliminando producto: ${pId}`);

    AbrirWaiting();

    $.ajax({
        url: EliminarProductoOrCtlUrl,
        type: 'POST',
        data: { p_id: pId },
        dataType: 'json',
        success: function (response) {
            CerrarWaiting();

            if (response.error === true) {
                AbrirMensaje(
                    "Error",
                    response.msg || "Ocurrió un error al eliminar el producto",
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

            if (response.warn === true) {
                AbrirMensaje(
                    "Advertencia",
                    response.msg || "No se pudo eliminar el producto",
                    function () {
                        $("#msjModal").modal("hide");
                    },
                    false,
                    ["Aceptar"],
                    "warn!",
                    null
                );
                return;
            }

            // Éxito: actualizar grilla
            console.log(`✅ Producto eliminado: ${pId}`);

            // Eliminar fila visualmente con animación
            const $fila = $(`#tbGridListaControl tbody tr[data-pid="${pId}"]`);
            $fila.fadeOut(300, function () {
                $(this).remove();

                // Actualizar grilla con datos del servidor
                if (response.data && response.data.productos) {
                    renderizarProductos(response.data.productos);
                }

                // Verificar si quedan productos
                if (response.data.productosRestantes === 0) {
                    $("#btnConfirmar").hide("fast");
                    mostrarMensajeInfo("No hay productos cargados. Agregue productos para continuar.");
                }
            });

            // Mostrar mensaje de éxito
            mostrarToast(response.msg || "Producto eliminado correctamente", "success");
        },
        error: function (xhr, status, error) {
            CerrarWaiting();
            console.error("❌ Error AJAX al eliminar producto:", error, xhr);

            let mensajeError = "Error de conexión al eliminar el producto";
            if (xhr.responseJSON && xhr.responseJSON.msg) {
                mensajeError = xhr.responseJSON.msg;
            } else if (xhr.responseText) {
                try {
                    const response = JSON.parse(xhr.responseText);
                    mensajeError = response.msg || mensajeError;
                } catch (e) {
                    console.error("Error al parsear respuesta:", e);
                }
            }

            AbrirMensaje(
                "Error",
                mensajeError,
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
 * Muestra un mensaje informativo en la grilla
 * @param {string} mensaje - Mensaje a mostrar
 */
function mostrarMensajeInfo(mensaje) {
    const tbody = $("#tbGridListaControl tbody");
    tbody.html(`<tr><td colspan="6" class="text-center text-info py-3"><i class="bx bx-info-circle me-2"></i>${mensaje}</td></tr>`);
}

/**
 * Muestra un toast de notificación (opcional - si está disponible en el proyecto)
 * @param {string} mensaje - Mensaje del toast
 * @param {string} tipo - Tipo: success, error, warning, info
 */
function mostrarToast(mensaje, tipo = "info") {
    // Si el proyecto tiene sistema de toasts, implementar aquí
    // Por ahora solo log
    console.log(`📢 Toast [${tipo}]: ${mensaje}`);
}
