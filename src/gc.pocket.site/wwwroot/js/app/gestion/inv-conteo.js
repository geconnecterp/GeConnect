let estadoConfirmacionInventario = null;
let productoValidadoInventario = null;
let secuenciaBusquedaInventario = 0;
let decisionInventarioPendiente = false;

$(function () {
    InicializaControlesConteo();
    actualizarContadorProductos();
    $("#btnBusquedaBase").off("click").on("click", function () {
        if (estadoConfirmacionInventario !== null || decisionInventarioPendiente) return;
        InicializaControlesConteo();
        buscarProducto("INV");
    });
    $("#estadoFuncion").on("change", verificaEstadoCont);
    $("#btnCargaConteo").on("click", cargarConteoEnGrid);
    $("#btnConfirmarConteo").on("click", confirmarConteo);
    $(".inputEditable").on("keypress", analizaEnterInput);
    $("#tbGridConteoProductos").on("click", ".btn-eliminar-producto", function () { eliminarProductoDelConteo($(this)); });
    if (typeof preguntarRecuperacion !== "undefined" && preguntarRecuperacion) {
        decisionInventarioPendiente = true;
        AbrirMensaje("BOX con conteo previo",
            "¿Desea recuperar su conteo o comenzar desde cero?<br>Lo guardado solo se reemplaza al confirmar un conteo con productos.",
            function (opcion) {
                $("#msjModal").modal("hide");
                decisionInventarioPendiente = false;
                if (opcion === "SI2") {
                    const url = new URL(window.location.href);
                    url.searchParams.set("reiniciar", "true");
                    window.location.href = url.toString();
                } else if (opcion !== "SI") window.location.href = "/Gestion/Inventario/Index";
            }, true, ["Recuperar", "Desde cero", "Cancelar"], "warn!", null, "cancelar");
    }
});
function textoInventario(valor) { return $("<span>").text(valor == null ? "" : String(valor)).html(); }
function formatoInventario(valor) { return Number(valor).toLocaleString("es-AR", { useGrouping: false, maximumFractionDigits: 3 }); }
function avisoInventario(mensaje, error = false) {
    AbrirMensaje("Inventario", textoInventario(mensaje), () => $("#msjModal").modal("hide"),
        false, ["Aceptar"], error ? "error!" : "warn!", null);
}
function contextoInventario() {
    return { inv_nro: estado.inv_nro, tipo: estado.tipo, tipo_id: estado.tipo_id, usu_id: "" };
}
function InicializaControlesConteo() {
    secuenciaBusquedaInventario++;
    productoValidadoInventario = null;
    $("#pId, #btos, #up, #uns").val("");
    $("#btos, #up, #uns").off(".cantidadProductoPocket").prop("readonly", true).prop("disabled", false);
    $("#btnCargaConteo").prop("disabled", true);
    $("#nnProducto").hide();
}
function verificaEstadoCont() {
    if (!productoBase || !productoBase.p_id || estadoConfirmacionInventario !== null) return;
    const producto = { ...productoBase };
    const secuencia = ++secuenciaBusquedaInventario;
    productoValidadoInventario = null;
    $("#btnCargaConteo").prop("disabled", true);
    $.ajax({
        url: invValidarProductoUrl, type: "POST", contentType: "application/json",
        data: JSON.stringify({ ...contextoInventario(), p_id: producto.p_id }),
        success: function (r) {
            if (secuencia !== secuenciaBusquedaInventario) return;
            if (!r || r.error !== false || r.warn !== false) {
                InicializaControlesConteo(); avisoInventario(r?.msg || "No se pudo validar el producto."); return;
            }
            if (!producto.up_tipo) { avisoInventario("El producto no informa up_tipo. No se puede configurar la carga."); return; }
            productoValidadoInventario = producto;
            cargaProductoEnControl(producto);
        },
        error: function () {
            if (secuencia !== secuenciaBusquedaInventario) return;
            InicializaControlesConteo(); avisoInventario("No se pudo validar el producto. La grilla se conserva.", true);
        }
    });
}
function cargaProductoEnControl(producto) {
    const pesable = InventarioCantidades.pesable(producto.up_tipo);
    $("#pId").val(producto.p_id);
    $("#nnProducto").text(producto.p_desc).show();
    $("#btos, #up, #uns").prop("readonly", false);
    // Adaptador al helper de entrada; no modifica el up_id real del producto.
    ConfigurarEntradaCantidadProducto("#btos", "07", "InventarioBultos");
    ConfigurarEntradaCantidadProducto("#up", "07", "InventarioPresentacion");
    ConfigurarEntradaCantidadProducto("#uns", pesable ? "00" : "07", "InventarioUS");
    $("#btos").val(0).prop("disabled", pesable);
    $("#up").val(pesable ? 1 : producto.p_unidad_pres).prop("disabled", pesable);
    $("#uns").val(0);
    $("#btnCargaConteo").prop("disabled", false);
    $("#Busqueda").val("");
    $(pesable ? "#uns" : "#btos").trigger("focus").trigger("select");
}
function leerFilaInventario($fila) {
    return {
        p_id: $fila.attr("data-p-id"), p_desc: $fila.find("td:eq(1)").text(),
        up_id: $fila.attr("data-up-id"), up_tipo: $fila.attr("data-up-tipo"),
        box_id: $fila.attr("data-box-id") || "", carga_nro: Number($fila.attr("data-carga-nro") || 0),
        invd_bulto: Number($fila.attr("data-bultos")), invd_unidad_pres: Number($fila.attr("data-presentacion")),
        invd_unidad_suelta: Number($fila.attr("data-unidad-suelta")), invd_cantidad: Number($fila.attr("data-cantidad"))
    };
}
function obtenerProductosDelGrid() {
    const filas = [];
    $("#tbGridConteoProductos tbody tr[data-p-id]").each(function () { filas.push(leerFilaInventario($(this))); });
    return filas;
}
function cargarConteoEnGrid() {
    if (estadoConfirmacionInventario !== null || decisionInventarioPendiente) return;
    const producto = productoValidadoInventario;
    if (!producto || $("#pId").val() !== producto.p_id) { avisoInventario("Busque y valide el producto antes de cargarlo."); return; }
    let nueva;
    try {
        const numero = id => Number(NormalizarNumeroEntrada($(id).val(), "Inventario"));
        nueva = InventarioCantidades.calcular(producto.up_tipo, numero("#btos"), numero("#up"), numero("#uns"));
    } catch (e) { avisoInventario(e.message); return; }
    const $fila = $("#tbGridConteoProductos tbody tr[data-p-id]").filter(function () { return $(this).attr("data-p-id") === producto.p_id; });
    const aplicar = cantidades => {
        if ($fila.length) actualizarFilaExistente($fila, producto, cantidades);
        else agregarNuevaFilaAlGrid(producto, cantidades);
        InicializaControlesConteo(); actualizarContadorProductos(); $("#Busqueda").trigger("focus");
    };
    if (!$fila.length) { aplicar(nueva); return; }
    const anterior = leerFilaInventario($fila);
    decisionInventarioPendiente = true;
    const totalAcumulado = Number((anterior.invd_cantidad + nueva.invd_cantidad).toFixed(3));
    AbrirMensaje("Producto ya contado",
        textoInventario(producto.p_id + " " + producto.p_desc) + "<br>" +
        "Anterior: <strong>" + formatoInventario(anterior.invd_cantidad) + "</strong>. Ingresado: <strong>" + formatoInventario(nueva.invd_cantidad) + "</strong>.<br>" +
        "Acumular: <strong>" + formatoInventario(totalAcumulado) + "</strong>. Reemplazar: <strong>" + formatoInventario(nueva.invd_cantidad) + "</strong>." +
        (anterior.invd_unidad_pres !== nueva.invd_unidad_pres ? "<br>Al acumular se normalizará el total a la nueva presentación." : ""),
        function (opcion) {
            $("#msjModal").modal("hide"); decisionInventarioPendiente = false;
            try {
                if (opcion === "SI") aplicar(InventarioCantidades.acumular(producto.up_tipo, anterior, nueva));
                else if (opcion === "SI2") aplicar(nueva);
            } catch (e) { avisoInventario(e.message); }
        }, true, ["Acumular", "Reemplazar", "Cancelar carga"], "warn!", null, "cancelar");
}
function actualizarFilaExistente($fila, producto, cantidades) {
    $fila.attr({ "data-up-id": producto.up_id, "data-up-tipo": producto.up_tipo,
        "data-bultos": cantidades.invd_bulto, "data-presentacion": cantidades.invd_unidad_pres,
        "data-unidad-suelta": cantidades.invd_unidad_suelta, "data-cantidad": cantidades.invd_cantidad });
    $fila.find("td:eq(1)").text(producto.p_desc);
    [cantidades.invd_bulto, cantidades.invd_unidad_pres, cantidades.invd_unidad_suelta, cantidades.invd_cantidad]
        .forEach((valor, i) => $fila.find("td:eq(" + (i + 2) + ")").text(formatoInventario(valor)));
    actualizarContadorProductos();
}
function agregarNuevaFilaAlGrid(producto, cantidades) {
    const $tbody = $("#tbGridConteoProductos tbody");
    $tbody.find("td[colspan]").closest("tr").remove();
    const $fila = $("<tr>").attr({ "data-p-id": producto.p_id,
        "data-box-id": estado.tipo === "B" ? estado.tipo_id : "", "data-carga-nro": estado.tipo === "P" ? estado.tipo_id : 1 });
    $("<td>").text(producto.p_id).appendTo($fila);
    $("<td>").text(producto.p_desc).appendTo($fila);
    for (let i = 0; i < 4; i++) $("<td>").addClass("text-end").appendTo($fila);
    const $boton = $("<button>").attr({ type: "button", title: "Eliminar producto del conteo" })
        .addClass("btn btn-danger btn-sm btn-eliminar-producto").html('<i class="bx bx-trash"></i>');
    $("<td>").addClass("text-center").append($boton).appendTo($fila);
    $tbody.prepend($fila); actualizarFilaExistente($fila, producto, cantidades);
}
function eliminarProductoDelConteo($boton) {
    if (estadoConfirmacionInventario !== null || decisionInventarioPendiente) return;
    const $fila = $boton.closest("tr"), producto = leerFilaInventario($fila);
    decisionInventarioPendiente = true;
    AbrirMensaje("Eliminar del conteo",
        "¿Quitar " + textoInventario(producto.p_id + " " + producto.p_desc) + "?<br>La eliminación se guardará al confirmar el conteo. No se permite confirmar sin productos.",
        function (respuesta) {
            $("#msjModal").modal("hide"); decisionInventarioPendiente = false;
            if (respuesta === "SI") { $fila.remove(); actualizarContadorProductos(); }
        }, true, ["Quitar", "Cancelar"], "warn!", null, "cancelar");
}
function actualizarContadorProductos() {
    const cantidad = $("#tbGridConteoProductos tbody tr[data-p-id]").length;
    $("#btnConfirmarConteo").prop("disabled", cantidad === 0 || estadoConfirmacionInventario !== null);
    $(".grid-golden-footer strong").text(cantidad); $(".grid-golden-footer").toggle(cantidad > 0);
    if (!cantidad) $("#tbGridConteoProductos tbody").html('<tr><td colspan="7">No hay productos en el conteo.</td></tr>');
}
function confirmarConteo() {
    if (estadoConfirmacionInventario !== null || decisionInventarioPendiente) return;
    const productos = obtenerProductosDelGrid();
    if (!productos.length) { avisoInventario("Debe agregar al menos un producto para confirmar el conteo."); return; }
    decisionInventarioPendiente = true;
    AbrirMensaje("Confirmar conteo",
        "Se guardarán los <strong>" + productos.length + "</strong> productos de la grilla, reemplazando el conteo anterior de este BOX o planilla para su usuario.",
        function (respuesta) {
            $("#msjModal").modal("hide"); decisionInventarioPendiente = false;
            if (respuesta === "SI") ejecutarConfirmacionConteo(productos);
        }, true, ["Confirmar", "Cancelar"], "warn!", null, "cancelar");
}
function ejecutarConfirmacionConteo(productos) {
    if (estadoConfirmacionInventario !== null) return false;
    if (!productos?.length) { avisoInventario("Debe agregar al menos un producto para confirmar el conteo."); return false; }
    const contexto = IniciarConfirmacionSegura("#btnConfirmarConteo", "Confirmando conteo...", "Confirmando...");
    if (contexto === null) return false;
    estadoConfirmacionInventario = contexto;
    $("#btnCargaConteo, #btnBusquedaBase, .btn-eliminar-producto").prop("disabled", true);
    const liberar = () => {
        FinalizarConfirmacionSegura(contexto); estadoConfirmacionInventario = null;
        $("#btnBusquedaBase, .btn-eliminar-producto").prop("disabled", false);
        $("#btnCargaConteo").prop("disabled", !productoValidadoInventario); actualizarContadorProductos();
    };
    const resultadoIncierto = () => {
        FinalizarConfirmacionSegura(contexto); estadoConfirmacionInventario = "verificar"; actualizarContadorProductos();
        AbrirMensaje("Verificar conteo", "No se pudo confirmar el resultado. La grilla se conserva. Revise los conteos guardados antes de reintentar para no duplicar una planilla nueva.",
            function (opcion) { $("#msjModal").modal("hide"); if (opcion === "SI") window.location.href = "/Gestion/Inventario/Index"; },
            true, ["Ir a los conteos", "Permanecer"], "warn!", null, "cancelar");
    };
    try {
        $.ajax({ url: "/Gestion/Inventario/ConfirmarConteo", type: "POST", contentType: "application/json",
            data: JSON.stringify({ ...contextoInventario(), json: productos }),
            headers: { "RequestVerificationToken": $('input[name="__RequestVerificationToken"]').val() },
            success: function (r) {
                if (!r || r.error === true || typeof r.warn !== "boolean" || typeof r.error !== "boolean" || !r.msg) { resultadoIncierto(); return; }
                if (r.warn) { liberar(); avisoInventario(r.msg || "No se pudo confirmar el conteo."); return; }
                FinalizarConfirmacionSegura(contexto); estadoConfirmacionInventario = "confirmado"; actualizarContadorProductos();
                AbrirMensaje("Conteo confirmado", textoInventario(r.msg),
                    () => { window.location.href = "/Gestion/Inventario/Index"; }, false, ["Aceptar"], "succ!", null);
            }, error: resultadoIncierto });
    } catch (e) { resultadoIncierto(); }
    return false;
}
