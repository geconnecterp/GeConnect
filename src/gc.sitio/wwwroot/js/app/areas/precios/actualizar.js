let ctaActual = "";
let denominacionActual = "";
let solicitudProductosActual = null;
let secuenciaConsultaProductos = 0;
let actualizacionEnProceso = false;

$(function () {
    initializeDocumentEvents();
    inicializarEstadoVisual();

    $("#pagEstado")
        .off("change.actualizarPP")
        .on("change.actualizarPP", function () {
            presentaPaginacion($("#divPaginacion"));
        });

    funcCallBack = cargarProductosProveedor;

    window.ActualizarPP = {
        obtenerProveedoresSeleccionados,
        cargarProductosProveedor,
        recargarProveedores: cargarProveedores
    };

    cargarProveedores();
});

function initializeDocumentEvents() {
    $(document)
        .off("change.actualizarPP", "#selectAllProveedores")
        .on("change.actualizarPP", "#selectAllProveedores", function () {
            $(".proveedor-check:not(:disabled)").prop("checked", $(this).prop("checked"));
            actualizarContadores();
        });

    $(document)
        .off("change.actualizarPP", ".proveedor-check")
        .on("change.actualizarPP", ".proveedor-check", actualizarContadores);

    $(document)
        .off("click.actualizarPP", ".proveedor-row")
        .on("click.actualizarPP", ".proveedor-row", function (e) {
            if ($(e.target).is("input[type='checkbox']")) return;

            const ctaId = String($(this).data("cta-id") || "").trim();
            const denominacion = String($(this).data("denominacion") || "").trim();
            if (!ctaId) return;

            $("#tbGridProveedores tbody tr").removeClass("selected");
            $(this).addClass("selected");

            ctaActual = ctaId;
            denominacionActual = denominacion;
            pagina = 1;

            mostrarProveedorConsultado(ctaActual, denominacionActual);
            bootstrap.Tab.getOrCreateInstance(document.getElementById("productos-tab")).show();
            cargarProductosProveedor(pagina);
        });
}

function inicializarEstadoVisual() {
    ctaActual = "";
    denominacionActual = "";
    actualizacionEnProceso = false;
    limpiarSeleccionProveedores();
    limpiarDetalleProductos();
    ocultarProveedorConsultado();
    restaurarBotonConfirmar();
}

async function cargarProveedores() {
    const $container = $("#proveedoresContainer");
    mostrarSpinnerCarga($container, "Cargando proveedores con productos para actualizar...");

    try {
        const response = await $.ajax({
            url: CargarProveedoresUrl,
            type: "POST",
            timeout: 30000,
            headers: obtenerHeadersAntiforgery()
        });

        $container.html(response);
        actualizarContadores();
        return true;
    } catch (error) {
        if (esErrorAutenticacion(error)) {
            manejarErrorAutenticacion();
            return false;
        }

        mostrarErrorConRecarga($container, obtenerMensajeErrorConexion(error, "Error al cargar los proveedores."));
        return false;
    }
}

async function cargarProductosProveedor(pag = 1) {
    if (!ctaActual) {
        mostrarMensajeSimple("Atención", "Debe seleccionar un proveedor para consultar sus productos.", "warn!");
        return;
    }

    cancelarSolicitudProductosPendiente();

    const $container = $("#productosContainer");
    const secuencia = ++secuenciaConsultaProductos;
    const cuentaConsultada = ctaActual;
    mostrarSpinnerCarga($container, `Obteniendo productos de ${denominacionActual || cuentaConsultada}...`);

    try {
        solicitudProductosActual = $.ajax({
            url: ObtenerProductosProveedorUrl,
            type: "POST",
            contentType: "application/x-www-form-urlencoded; charset=UTF-8",
            data: { ctaId: cuentaConsultada, pag },
            timeout: 30000,
            headers: obtenerHeadersAntiforgery()
        });

        const response = await solicitudProductosActual;
        if (secuencia !== secuenciaConsultaProductos || cuentaConsultada !== ctaActual) return;

        $container.html(response);

        if ($container.find("#tbGridProductos").length === 0) {
            limpiarPaginacion();
            return;
        }

        const metadataResponse = await $.ajax({
            url: buscarActuProductoMetadataURL,
            type: "POST",
            headers: obtenerHeadersAntiforgery()
        });

        if (secuencia !== secuenciaConsultaProductos || cuentaConsultada !== ctaActual) return;

        if (!metadataResponse || metadataResponse.error === true) {
            mostrarMensajeSimple(
                "Atención",
                metadataResponse?.msg || "No se pudo obtener la paginación de los productos.",
                "error!"
            );
            return;
        }

        const metadata = metadataResponse.metadata || {};
        totalRegs = metadata.totalCount || 0;
        pags = metadata.totalPages || 0;
        pagRegs = metadata.pageSize || 1;
        pagina = pag;
        $("#pagEstado").val(true).trigger("change");
    } catch (error) {
        if (error?.statusText === "abort") return;
        if (esErrorAutenticacion(error)) {
            manejarErrorAutenticacion();
            return;
        }

        mostrarErrorConRecarga(
            $container,
            obtenerMensajeErrorConexion(error, "Error al cargar los productos."),
            () => cargarProductosProveedor(pagina)
        );
    } finally {
        if (secuencia === secuenciaConsultaProductos) {
            solicitudProductosActual = null;
        }
    }
}

function obtenerProveedoresSeleccionados() {
    return $(".proveedor-check:checked").map(function () {
        return String($(this).val() || "").trim();
    }).get().filter(Boolean);
}

function actualizarContadores() {
    const seleccionados = $(".proveedor-check:checked").length;
    const total = $(".proveedor-check").length;

    $("#selectedCount, #contadorSeleccionados").text(seleccionados);

    const $selectAll = $("#selectAllProveedores");
    if ($selectAll.length) {
        $selectAll
            .prop("indeterminate", seleccionados > 0 && seleccionados < total)
            .prop("checked", total > 0 && seleccionados === total);
    }

    if (!actualizacionEnProceso) {
        $("#btnConfirmarActualizacion").prop("disabled", seleccionados === 0);
    }
}

function confirmarActualizacion() {
    const proveedoresSeleccionados = obtenerProveedoresSeleccionados();

    if (proveedoresSeleccionados.length === 0) {
        mostrarMensajeSimple("Validación", "Debe seleccionar al menos un proveedor para confirmar la actualización.", "warn!");
        return;
    }

    prepararEstadoModal();
    AbrirMensaje(
        "Confirmar Actualización",
        `¿Confirma la actualización de <strong>${proveedoresSeleccionados.length}</strong> proveedor${proveedoresSeleccionados.length === 1 ? "" : "es"}?<br>` +
        '<small class="text-muted">Esta acción aplicará los cambios de precios definitivamente.</small>',
        function (respuesta) {
            if (respuesta !== "SI") {
                $("#msjModal").modal("hide");
                return;
            }

            cerrarMensajeYContinuar(() => ejecutarConfirmacionActualizacion(proveedoresSeleccionados));
        },
        true,
        ["Confirmar", "Cancelar"],
        "warn!",
        null
    );
}

async function ejecutarConfirmacionActualizacion(ctasId) {
    establecerEstadoProcesando(true);

    try {
        const response = await $.ajax({
            url: ConfirmarProveedoresUrl,
            type: "POST",
            traditional: true,
            data: { ctasId },
            timeout: 60000,
            headers: obtenerHeadersAntiforgery()
        });

        await procesarRespuestaConfirmacion(response);
    } catch (error) {
        establecerEstadoProcesando(false);

        if (esErrorAutenticacion(error)) {
            manejarErrorAutenticacion();
            return;
        }

        manejarErrorConfirmacion(obtenerMensajeErrorConexion(error, "Error de comunicación con el servidor."));
    }
}

async function procesarRespuestaConfirmacion(response) {
    if (!response || typeof response !== "object") {
        establecerEstadoProcesando(false);
        manejarErrorConfirmacion("Respuesta inválida del servidor.");
        return;
    }

    if (response.error === true) {
        establecerEstadoProcesando(false);
        manejarErrorConfirmacion(response.msg || "No se pudo completar la actualización.");
        return;
    }

    if (response.warn === true) {
        establecerEstadoProcesando(false);
        if (response.auth === true) {
            manejarErrorAutenticacion();
        } else {
            manejarAdvertenciaConfirmacion(response.msg || "No se pudo completar la actualización.");
        }
        return;
    }

    await manejarExitoConfirmacion(response.msg || "El procesamiento se realizó satisfactoriamente");
}

async function manejarExitoConfirmacion(mensaje) {
    await reiniciarInterfazActualizacion({ limpiarSesionDetalle: false, volverAProveedores: true });
    await cargarProveedores();

    prepararEstadoModal();
    AbrirMensaje(
        "Actualización Completada",
        mensaje,
        () => $("#msjModal").modal("hide"),
        false,
        ["Aceptar"],
        "succ!",
        null
    );
}

function manejarErrorConfirmacion(mensaje) {
    restaurarBotonConfirmar();
    prepararEstadoModal();
    AbrirMensaje(
        "Error",
        mensaje,
        () => $("#msjModal").modal("hide"),
        false,
        ["Aceptar"],
        "error!",
        null
    );
}

function manejarAdvertenciaConfirmacion(mensaje) {
    restaurarBotonConfirmar();
    prepararEstadoModal();
    AbrirMensaje(
        "Advertencia",
        mensaje,
        () => $("#msjModal").modal("hide"),
        false,
        ["Aceptar"],
        "warn!",
        null
    );
}

function manejarErrorAutenticacion() {
    establecerEstadoProcesando(false);
    prepararEstadoModal();
    AbrirMensaje(
        "Sesión Expirada",
        "Su sesión ha terminado. Debe volver a autenticarse.",
        () => cerrarMensajeYContinuar(() => { window.location.href = home; }),
        false,
        ["Aceptar"],
        "warn!",
        null
    );
}

function cancelarActualizacion() {
    const hayEstadoParaLimpiar = obtenerProveedoresSeleccionados().length > 0 || !!ctaActual;
    if (!hayEstadoParaLimpiar) return;

    prepararEstadoModal();
    AbrirMensaje(
        "Cancelar Selección",
        "¿Desea limpiar la selección y volver al estado inicial?",
        function (respuesta) {
            if (respuesta !== "SI") {
                $("#msjModal").modal("hide");
                return;
            }

            cerrarMensajeYContinuar(() => {
                reiniciarInterfazActualizacion({ limpiarSesionDetalle: true, volverAProveedores: true });
            });
        },
        true,
        ["Continuar", "Cancelar"],
        "warn!",
        null
    );
}

async function reiniciarInterfazActualizacion({ limpiarSesionDetalle, volverAProveedores }) {
    cancelarSolicitudProductosPendiente();
    secuenciaConsultaProductos++;

    ctaActual = "";
    denominacionActual = "";
    establecerEstadoProcesando(false);
    limpiarSeleccionProveedores();
    limpiarDetalleProductos();
    ocultarProveedorConsultado();

    if (volverAProveedores) {
        bootstrap.Tab.getOrCreateInstance(document.getElementById("proveedores-tab")).show();
    }

    if (limpiarSesionDetalle) {
        try {
            await $.ajax({
                url: reiniciarConsultaActualizacionURL,
                type: "POST",
                headers: obtenerHeadersAntiforgery()
            });
        } catch (error) {
            if (esErrorAutenticacion(error)) manejarErrorAutenticacion();
        }
    }
}

function limpiarSeleccionProveedores() {
    $(".proveedor-check, #selectAllProveedores")
        .prop("checked", false)
        .prop("indeterminate", false)
        .prop("disabled", false);
    $("#tbGridProveedores tbody tr").removeClass("selected");
    actualizarContadores();
}

function limpiarDetalleProductos() {
    $("#productosContainer").html(`
        <div class="alert alert-info" role="alert">
            <i class="fas fa-info-circle me-2"></i>
            Seleccione un proveedor en la pestaña anterior para ver sus productos.
        </div>
    `);
    limpiarPaginacion();
}

function limpiarPaginacion() {
    const $paginacion = $("#divPaginacion");
    try {
        if ($paginacion.length && $paginacion.data("pagination")) {
            $paginacion.pagination("destroy");
        }
    } catch (error) {
        console.debug("No había una paginación activa para destruir.");
    }
    $paginacion.empty();

    pagina = 1;
    totalRegs = 0;
    pags = 0;
    pagRegs = 0;
    $("#pagEstado").val(false);
}

function mostrarProveedorConsultado(ctaId, denominacion) {
    consCta = ctaId;
    consRrss = denominacion;
    consTipo = "P";

    $("#controlConsultaCambio" + sufijoControlCuentaActualizarPP).val(true);
    const asignar = window["AsignaDatosCuenta" + sufijoControlCuentaActualizarPP];
    if (typeof asignar === "function") asignar();
}

function ocultarProveedorConsultado() {
    const inicializar = window["inicializaCtrl" + sufijoControlCuentaActualizarPP];
    if (typeof inicializar === "function") inicializar();
}

function establecerEstadoProcesando(enProceso) {
    actualizacionEnProceso = enProceso;
    $(".proveedor-check, #selectAllProveedores, #btnCancelarActualizacion").prop("disabled", enProceso);

    if (enProceso) {
        $("#btnConfirmarActualizacion")
            .prop("disabled", true)
            .removeClass("btn-outline-success")
            .addClass("btn-success")
            .html('<i class="bx bx-loader-alt bx-spin me-2"></i><span>Procesando actualización...</span>');
    } else {
        restaurarBotonConfirmar();
    }
}

function restaurarBotonConfirmar() {
    const seleccionados = $(".proveedor-check:checked").length;
    $("#btnConfirmarActualizacion")
        .removeClass("btn-outline-success")
        .addClass("btn-success")
        .html('<i class="bx bx-check-circle me-2"></i><div class="d-flex flex-column"><span class="fw-bold">CONFIRMAR</span><small class="opacity-75">Aplicar cambios</small></div>')
        .prop("disabled", actualizacionEnProceso || seleccionados === 0);
    $("#btnCancelarActualizacion").prop("disabled", actualizacionEnProceso);
}

function cancelarSolicitudProductosPendiente() {
    if (solicitudProductosActual && solicitudProductosActual.readyState !== 4) {
        solicitudProductosActual.abort();
    }
    solicitudProductosActual = null;
}

function cerrarMensajeYContinuar(callback) {
    const $modal = $("#msjModal");

    if (!$modal.hasClass("show")) {
        callback();
        return;
    }

    $modal
        .off("hidden.bs.modal.actualizarPP")
        .one("hidden.bs.modal.actualizarPP", callback)
        .modal("hide");
}

function prepararEstadoModal() {
    const $modal = $("#msjModal");
    $modal.removeClass("modal-error modal-warning modal-success modal-info modal-danger");
    $modal.find(".modal-header").removeClass("bg-danger bg-warning bg-success bg-info text-white text-dark");
    $modal.find(".modal-body").removeClass("text-danger text-warning text-success text-info");
}

function mostrarMensajeSimple(titulo, mensaje, tipo) {
    prepararEstadoModal();
    AbrirMensaje(
        titulo,
        mensaje,
        () => $("#msjModal").modal("hide"),
        false,
        ["Aceptar"],
        tipo,
        null
    );
}

function mostrarSpinnerCarga($container, message) {
    $container.html(`
        <div class="loading-container">
            <div class="spinner-container">
                <div class="spinner-border spinner-border-golden" role="status">
                    <span class="visually-hidden">Cargando...</span>
                </div>
                <p class="mt-3 text-muted mb-0">${message}</p>
            </div>
        </div>
    `);
}

function mostrarErrorConRecarga($container, message, retryFunction = null) {
    const retryFunctionName = retryFunction ? "retryFunction()" : "ActualizarPP.recargarProveedores()";
    if (retryFunction) window.retryFunction = retryFunction;

    $container.html(`
        <div class="alert alert-danger" role="alert">
            <div class="d-flex align-items-center">
                <i class="fas fa-exclamation-triangle me-2"></i>
                <div class="flex-grow-1"><strong>Error:</strong> ${message}</div>
                <button class="btn btn-sm btn-outline-danger" onclick="${retryFunctionName}">
                    <i class="fas fa-redo me-1"></i>Reintentar
                </button>
            </div>
        </div>
    `);
}

function obtenerHeadersAntiforgery() {
    return {
        RequestVerificationToken: $("input[name='__RequestVerificationToken']").val() || ""
    };
}

function esErrorAutenticacion(error) {
    return error?.status === 401 || error?.status === 403;
}

function obtenerMensajeErrorConexion(error, mensajePredeterminado) {
    if (error?.status === 0) return "Error de conexión. Verifique su conexión.";
    if (error?.status === 404) return "No se encontró el servicio solicitado.";
    if (error?.status >= 500) return "Error interno del servidor. Intente nuevamente.";
    if (error?.statusText === "timeout") return "La operación superó el tiempo de espera.";
    return mensajePredeterminado;
}
