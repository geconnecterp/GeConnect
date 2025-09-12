// Variables de estado para gestión de selección
var modoSeleccionCanal = "ninguno"; // "individual", "multiple", "ninguno"
var canalIndividualSeleccionado = null;

$(function () {
    // Verifico si se hace click en el botón buscar
    $("#btnBusquedaBase").on("click", function () {
        buscarProducto();
        return true;
    });

    $("#estadoFuncion").on("change", verificaEstado);

    cargarCanales();
});

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
        P_pvta: producto.p_pvta || "0",
        P_pvta_oferta: producto.p_pvta_oferta || "0",
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

function configurarEventosGridOferta() {
    // Checkbox "Seleccionar todos"
    $("#checkAllOfertas").off("change").on("change", function () {
        var isChecked = $(this).is(":checked");
        $(".check-oferta").prop("checked", isChecked);
    });

    // Checkboxes individuales
    $(".check-oferta").off("change").on("change", function () {
        var totalChecks = $(".check-oferta").length;
        var checkedCount = $(".check-oferta:checked").length;
        $("#checkAllOfertas").prop("checked", totalChecks === checkedCount);
    });

    // Botones de eliminar
    $(".btn-remover-oferta").off("click").on("click", function () {
        var productId = $(this).data("p-id");
        var row = $(this).closest("tr");
        var productDesc = row.find("td:nth-child(3)").text().trim();

        AbrirMensaje(
            "CONFIRMAR ELIMINACIÓN",
            `¿Está seguro de eliminar "${productDesc}" de las ofertas?`,
            function (resp) {
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

    // Botón guardar ofertas
    $("#btnGuardarOfertas").off("click").on("click", function () {
        guardarOfertasSeleccionadas();
    });

    // Validación en tiempo real
    $(".input-precio-oferta, .input-tope-venta").off("input").on("input", function () {
        validarCampoNumerico($(this));
    });

    $(".input-fecha-desde, .input-fecha-hasta").off("change").on("change", function () {
        validarRangoFechasGrid($(this));
    });
}

function eliminarProductoDelGrid(row, productDesc) {
    row.fadeOut(300, function () {
        $(this).remove();

        if ($("#tbGridProductosOferta tbody tr").length === 0) {
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

function validarCampoNumerico(campo) {
    var valor = parseFloat(campo.val());
    if (isNaN(valor) || valor < 0) {
        campo.addClass("is-invalid");
        return false;
    } else {
        campo.removeClass("is-invalid");
        return true;
    }
}

function validarRangoFechasGrid(campo) {
    var row = campo.closest("tr");
    var fechaDesde = row.find(".input-fecha-desde").val();
    var fechaHasta = row.find(".input-fecha-hasta").val();

    if (fechaDesde && fechaHasta && new Date(fechaDesde) > new Date(fechaHasta)) {
        campo.addClass("is-invalid");
        ControlaMensajeWarning("La fecha 'Hasta' debe ser posterior a la fecha 'Desde'");
        return false;
    } else {
        row.find(".input-fecha-desde, .input-fecha-hasta").removeClass("is-invalid");
        return true;
    }
}

function guardarOfertasSeleccionadas() {
    var productosSeleccionados = [];
    var erroresValidacion = false;

    $(".check-oferta:checked").each(function () {
        var row = $(this).closest("tr");
        var productId = $(this).data("p-id");
        var precioOferta = row.find(".input-precio-oferta");
        var fechaDesde = row.find(".input-fecha-desde");
        var fechaHasta = row.find(".input-fecha-hasta");
        var topeVenta = row.find(".input-tope-venta");

        if (!validarCampoNumerico(precioOferta) ||
            !validarCampoNumerico(topeVenta) ||
            !validarRangoFechasGrid(fechaDesde)) {
            erroresValidacion = true;
            return;
        }

        var oferta = {
            productoId: productId,
            precioOferta: precioOferta.val(),
            fechaDesde: fechaDesde.val(),
            fechaHasta: fechaHasta.val(),
            topeVenta: topeVenta.val() || "0"
        };

        productosSeleccionados.push(oferta);
    });

    if (erroresValidacion) {
        ControlaMensajeWarning("Corrija los errores de validación antes de guardar");
        return;
    }

    if (productosSeleccionados.length === 0) {
        ControlaMensajeWarning("Debe seleccionar al menos un producto para guardar");
        return;
    }

    AbrirMensaje(
        "CONFIRMAR GUARDADO",
        `¿Desea guardar ${productosSeleccionados.length} ofertas?`,
        function (resp) {
            if (resp === "SI") {
                procesarGuardadoOfertas(productosSeleccionados);
            }
            $("#msjModal").modal("hide");
            return true;
        },
        true,
        ["Guardar", "Cancelar"],
        "info!",
        null
    );
}

function procesarGuardadoOfertas(productos) {
    console.log("Productos a guardar:", productos);
    ControlaMensajeSuccess(`Se guardaron ${productos.length} ofertas correctamente`);
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
/** FINAL*/
