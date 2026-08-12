// Variable global para detectar cuando un campo va a ser editado
var campoEnPreparacionEdicion = null;
// Agregar variable global para controlar el modo de modificación
var modoModificacionCombo = false;
// me permite saber si se hace una busqueda 
var realizaAlgunaBusqueda = false;
// Variable global para almacenar el ID del combo guardado/modificado
var comboIdGuardado = null;
var comboSeleccionadoId = null;
var comboSeleccionadoEstado = null;
var detalleCargaVersion = 0;
var edicionEstructuraPermitida = false;

/**
 * Script para gestión de combos y promociones
 */
$(function () {
    // Inicialización
    console.log("🚀 Inicializando módulo de combos y promociones");

    //callback para que funcione la paginación
    var funcCallBack = buscarCombos;

    // Configurar eventos
    inicializarEventos();

    // Verificar variables necesarias
    if (typeof homeCombo === 'undefined') {
        console.error("La variable homeCombo no está definida");
    }

    if (typeof presentarPromosYCombosUrl === 'undefined') {
        console.error("La variable presentarPromosYCombosUrl no está definida");
    }

    if (typeof obtenerCanalesComboUrl === 'undefined') {
        console.error("La variable obtenerCanalesComboUrl no está definida");
    }

    if (typeof obtenerComboPorIdUrl === 'undefined') {
        console.error("La variable obtenerComboPorIdUrl no está definida");
    }

    // Añadir al inicio del documento, después de $(function() {...})
    // Limpiar la bandera de edición en eventos globales para prevenir estados inconsistentes
    $(document).on('mouseup', function () {
        // Limpiar si el mouseup ocurre pero no se llegó a hacer clic en el campo
        setTimeout(function () {
            if (campoEnPreparacionEdicion !== null) {
                campoEnPreparacionEdicion = null;
            }
        }, 100);
    });

    // Inicializar estados
    accionesIniciales();
});

// Variables globales para manejar relaciones de productos y sustitutos
var productosSustitutosMap = {};
var modoNuevoCombo = false;

//activar botones btnAbmAcepar y btnAbmCancelar
function ActivarBtnAC(band) {
    if (band) {
        $("#btnAbmCancelar").prop("disabled", false).show();
        $("#btnAbmAceptar").prop("disabled", false).show();
    }
    else {
        $("#btnAbmCancelar").prop("disabled", true).hide();
        $("#btnAbmAceptar").prop("disabled", true).hide();
    }
}

function analizaEstadoCombo() {
    // El propio componente collapse administra la apertura/cierre del listado.
    // No se debe limpiar el contexto del ABM al utilizar esta flecha.
}

/**
 * Inicializa los eventos para los elementos del formulario
 */
function inicializarEventos() {
    $("#btnCancel").on("click", function () {
        window.location.href = homeCombo;
    });

    // Configurar el evento click para el botón Cancelar/Inicializar
    $("#btnAbmCancelar").on("click", function (e) {
        cancelarOperacion(e);
    });

    // Configurar el evento click para el botón Buscar/Filtrar
    $("#btnBuscar").on("click", function () {
        buscarCombos();
    });
    funcCallBack = buscarCombos;

    // Eventos para los checkboxes del filtro
    $("#chkTipo").on("change", function () {
        $("#Tipo").prop("disabled", !$(this).prop("checked"));
    });

    $("#chkEstado").on("change", function () {
        $("#Estado").prop("disabled", !$(this).prop("checked"));
    });

    // ✅ NUEVO: Evento para controlar visibilidad del dropdown de preajustes
    $(document).on("change", "#cmb_tipo", function () {
        var tipoSeleccionado = $(this).val();
        var $contenedorPreajuste = $("#contenedorPreajuste");

        console.log("🔄 Tipo seleccionado:", tipoSeleccionado);

        // Controlar visibilidad del dropdown de preajustes
        actualizarVisibilidadSegunTipo(tipoSeleccionado);
    });

    $(document).on("click", "#btnCambiarEstadoCombo", function () {
        var estadoActual = $("#cmb_estado").val();
        var nuevoEstado = estadoActual === 'N' ? 'A' : (estadoActual === 'A' ? 'H' : null);
        if (!nuevoEstado) return;

        var tipo = $("#cmb_tipo").val() === 'C' || $("#cmb_tipo").val() === 'D' ? 'combo' : 'promoción';
        var accion = nuevoEstado === 'A' ? 'activar' : 'pasar a Histórico';
        AbrirMensaje(
            nuevoEstado === 'A' ? "ACTIVAR" : "PASAR A HISTÓRICO",
            `¿Está seguro que desea ${accion} ${tipo} "${$("#cmb_desc").val().trim()}"?`,
            function (resp) {
                if (resp === "SI") cambiarEstadoComboExistente(tipo, nuevoEstado);
                $("#msjModal").modal("hide");
                return true;
            },
            true,
            [nuevoEstado === 'A' ? "Activar" : "Pasar a Histórico", "Cancelar"],
            "info!",
            null
        );
    });

    // Evento para el botón de nuevo combo
    $("#btnAbmNuevo").on("click", function () {
        if ($("#Estado").val() !== 'N') {
            ControlaMensajeWarning("Agregar solo está disponible al consultar el estado Sin Activar");
            return;
        }
        modoNuevoCombo = true;
        edicionEstructuraPermitida = true;
        inicializarNuevoCombo();

        // Activar/desactivar botones
        ActivarBtnAC(true);

        $("#btnAbmNuevo").prop("disabled", true);
        $("#btnAbmModif").prop("disabled", true);

        // Verifico si el divFiltro esta SHOW. Si eso es así lo oculto.
        if ($("#divFiltro").is(":visible")) {
            $("#divFiltro").collapse("hide");
        }
        $("#divTools").show();
        // Cargar el modal de búsqueda avanzada
        cargarModalBusquedaAvanzada();
        // Inicializar los campos editables para cantidad y descuento en la grilla de productos
        inicializarCamposEditablesProductos();
        // Inicializar el mapa de sustitutos
        productosSustitutosMap = {};

        //// Actualizar los contenedores con los grids vacíos
        //$(".col-sm-4:has(#tbGridProductos)").show();
        //$(".col-sm-4:has(#tbGridSustitutos)").show();
    });

    // Evento para el botón de modificación
    $("#btnAbmModif").on("click", function () {
        // Verificar si hay algún combo seleccionado
        var comboId = $("#tbGridPromoCombo tbody tr.selected-row").data("combo-id");
        if (!comboId) {
            ControlaMensajeWarning("Debe seleccionar un combo/promoción para modificar");
            return;
        }

        var estadoActual = $("#cmb_estado").val();
        if (estadoActual === 'H') {
            ControlaMensajeWarning("Los registros históricos son de solo consulta");
            return;
        }

        // ✅ NUEVO: Verificar si hay productos cargados
        var $tbody = $("#tbGridProductos tbody");
        var hayProductos = $tbody.find("tr").length > 0 &&
            !$tbody.find("tr td[colspan]").length;

        edicionEstructuraPermitida = estadoActual === 'N';
        var tipoActual = $("#cmb_tipo").val();
        var admiteSustitutos = tipoActual !== 'Q' && tipoActual !== 'D';
        $("#btnAgregarCProducto").prop("disabled", !edicionEstructuraPermitida);
        $("#btnAgregarSustituto").prop("disabled", !edicionEstructuraPermitida || !admiteSustitutos);
        $(".btn-eliminar-producto, .btn-eliminar-sustituto").toggle(edicionEstructuraPermitida);

        // Activar modo modificación
        modoModificacionCombo = true;

        // Activar/desactivar botones apropiados
        ActivarBtnAC(true);
        $("#btnAbmNuevo").prop("disabled", true);
        $("#btnAbmModif").prop("disabled", true);
        desactivarGrilla("tbGridPromoCombo");

        $("#cmb_desc").prop("readonly", !edicionEstructuraPermitida);
        $("#cmb_desde, #cmb_hasta").prop("readonly", false);
        $("#btnCambiarEstadoCombo").hide();

        if (edicionEstructuraPermitida) {
            adaptarGrillaCanales();
        }

        // Inicializar los campos editables para cantidad y descuento
        inicializarCamposEditablesProductos();

        // Mostrar mensaje informativo
        //ControlaMensajeInfo("Ahora puede modificar cantidades y descuentos. Al terminar haga clic en 'Confirmar'.");
    });
   
    // Evento para el botón confirmar
    $("#btnAbmAceptar").on("click", function () {
        confirmarCombo();
    });

    // ✅ OPTIMIZACIÓN: Usar delegación de eventos específica para botones dinámicos
    // Remover solo el handler específico del botón antes de agregarlo
    $(document).off("click", "#btnAgregarCProducto").on("click", "#btnAgregarCProducto", function (e) {
        e.preventDefault();
        e.stopPropagation();

        console.log("🔘 Click en botón Agregar Producto");

        // Verificar que estemos en modo edición
        if (!modoNuevoCombo && !modoModificacionCombo) {
            console.warn("⚠️ No está en modo nuevo promo/combo o modificacion promo/combo");
            ControlaMensajeWarning("Debe estar Creando un nuevo combo o Modificando uno de ellos, para agregar productos");
            return;
        }
        if (!edicionEstructuraPermitida) {
            ControlaMensajeWarning("En estado Activo solo puede modificarse la vigencia");
            return;
        }

        // Cargar el modal si no existe y luego mostrarlo
        if ($("#busquedaModal").length === 0) {
            console.log("📦 Cargando modal de búsqueda avanzada...");
            cargarModalBusquedaAvanzada(function () {
                // Configurar el destino como "combos" y definir el callback
                if (typeof configurarDestinoBusquedaProductos === 'function') {
                    configurarDestinoBusquedaProductos("combos", "001", agregarProductosAlGrid, obtenerProductosExistentesIds);
                } else {
                    console.error("❌ Función configurarDestinoBusquedaProductos no está definida");
                }
                //limpia productos seleccionados
                limpiarSeleccionBusqueda();
                $("#busquedaModal").modal("show");
            });
        } else {
            console.log("✅ Modal ya existe, configurando y mostrando...");
            // Si ya existe, configurar destino y mostrar
            if (typeof configurarDestinoBusquedaProductos === 'function') {
                configurarDestinoBusquedaProductos("combos", "001", agregarProductosAlGrid, obtenerProductosExistentesIds);
            } else {
                console.error("❌ Función configurarDestinoBusquedaProductos no está definida");
            }
            limpiarSeleccionBusqueda();
            $("#busquedaModal").modal("show");
        }
    });

    // ✅ OPTIMIZACIÓN: Evento delegado específico para el botón de agregar sustituto
    $(document).off("click", "#btnAgregarSustituto").on("click", "#btnAgregarSustituto", function (e) {
        e.preventDefault();
        e.stopPropagation();

        console.log("🔘 Click en botón Agregar Sustituto");

        // Verificar que estemos en modo edición
        if (!modoNuevoCombo && !modoModificacionCombo) {
            console.warn("⚠️ No está en modo nuevo o modificacion de promo/combo");
            ControlaMensajeWarning("Debe estar creando un nuevo combo o modificando promo/combo, para agregar sustitutos");
            return;
        }
        if (!edicionEstructuraPermitida) {
            ControlaMensajeWarning("En estado Activo no pueden agregarse sustitutos");
            return;
        }

        // ✅ NUEVO: Bloquear sustitutos para tipos Q y D
        var tipoCombo = $("#cmb_tipo").val();
        if (tipoCombo === 'Q' || tipoCombo === 'D') {
            ControlaMensajeWarning("No se pueden agregar sustitutos para este tipo de combo/promoción");
            return;
        }

        // Verificar si hay un producto seleccionado
        var productoSeleccionado = $("#tbGridProductos tbody tr.selected-row");
        if (productoSeleccionado.length === 0) {
            ControlaMensajeWarning("Debe seleccionar un producto antes de agregar sustitutos");
            return;
        }

        var productoId = productoSeleccionado.find("td:first").text().trim();
        var productoDesc = productoSeleccionado.find("td:nth-child(2)").text().trim();

        console.log(`📍 Producto seleccionado: ${productoId} - ${productoDesc}`);

        // Cargar el modal de búsqueda avanzada
        if ($("#busquedaModal").length === 0) {
            console.log("📦 Cargando modal de búsqueda avanzada para sustitutos...");
            cargarModalBusquedaAvanzada(function () {
                // Configurar el destino como "sustitutos" y definir el callback
                if (typeof configurarDestinoBusquedaProductos === 'function') {
                    configurarDestinoBusquedaProductos("sustitutos", "001", function (productos) {
                        agregarSustitutosAlGrid(productos, productoId);
                    }, obtenerSustitutosExistentesIds);
                } else {
                    console.error("❌ Función configurarDestinoBusquedaProductos no está definida");
                }
                //limpia productos seleccionados
                limpiarSeleccionBusqueda();
                $("#busquedaModal").modal("show");
            });
        } else {
            console.log("✅ Modal ya existe, configurando para sustitutos...");
            // Si ya existe, configurar destino y mostrar
            if (typeof configurarDestinoBusquedaProductos === 'function') {
                configurarDestinoBusquedaProductos("sustitutos", "001", function (productos) {
                    agregarSustitutosAlGrid(productos, productoId);
                }, obtenerSustitutosExistentesIds);
            } else {
                console.error("❌ Función configurarDestinoBusquedaProductos no está definida");
            }
            limpiarSeleccionBusqueda();
            $("#busquedaModal").modal("show");
        }
    });
}

/**
 * Agrega sustitutos al grid y actualiza el mapa de relaciones
 * @param {Array} sustitutos - Productos que serán agregados como sustitutos
 * @param {string} productoId - ID del producto principal al que se asignarán los sustitutos
 */
function agregarSustitutosAlGrid(sustitutos, productoId) {
    // Validar que estemos en modo nuevo combo y que haya sustitutos para agregar
    if (!modoNuevoCombo || !sustitutos || sustitutos.length === 0) {
        return;
    }

    // Inicializar el mapa para este producto si no existe
    if (!productosSustitutosMap[productoId]) {
        productosSustitutosMap[productoId] = [];
    }

    var $filaProductoPrincipal = $("#tbGridProductos tbody tr").filter(function () {
        return String($(this).data("producto-id")) === String(productoId);
    }).first();
    var costoProductoPrincipal = parseFloat(
        $filaProductoPrincipal.find("td").eq(2).text().replace(/,/g, "").trim()
    );

    if (!Number.isFinite(costoProductoPrincipal)) {
        ControlaMensajeWarning("No se pudo determinar el costo del producto principal.");
        return;
    }

    var sustitutosConCostoDistinto = [];

    // Filtrar productos inválidos (el mismo producto, duplicados o distinto costo)
    var sustitutosValidos = sustitutos.filter(function (sustituto) {
        // Verificar que no sea el mismo producto principal
        if (sustituto.p_id === productoId) {
            console.warn(`⚠️ Un producto no puede ser sustituto de sí mismo: ${productoId}`);
            return false;
        }

        // Verificar que no exista ya como sustituto
        if (productosSustitutosMap[productoId].some(s => s.p_id === sustituto.p_id)) {
            console.warn(`⚠️ El producto ${sustituto.p_id} ya está agregado como sustituto`);
            return false;
        }

        var costoSustituto = Number(sustituto.p_pcosto);
        if (!Number.isFinite(costoSustituto) || Math.abs(costoSustituto - costoProductoPrincipal) > 0.0005) {
            sustitutosConCostoDistinto.push(sustituto);
            return false;
        }

        return true;
    });

    // Si después del filtrado no quedan sustitutos válidos, salir
    if (sustitutosValidos.length === 0) {
        if (sustitutosConCostoDistinto.length > 0) {
            var costoPrincipalTexto = costoProductoPrincipal.toLocaleString('en-US', {
                minimumFractionDigits: 3,
                maximumFractionDigits: 3
            });
            ControlaMensajeWarning(
                `El sustituto debe tener el mismo costo que el producto principal (${costoPrincipalTexto}).`
            );
        }
        console.log("No hay sustitutos válidos para agregar después del filtrado");
        return;
    }

    // Añadir los sustitutos válidos al mapa
    productosSustitutosMap[productoId] = productosSustitutosMap[productoId].concat(sustitutosValidos);

    // Guardar en sesión
    guardarSustitutosEnSesion();

    // Guardar en el servidor si la URL está definida
    if (typeof resguardarRelacionProductoSustitutoUrl !== 'undefined') {
        guardarRelacionProductoSustitutoEnServidor(productoId);
    }

    // Actualizar el grid de sustitutos
    actualizarGridSustitutos(productoId);

    // Informar al usuario si se descartaron algunos sustitutos
    var descartados = sustitutos.length - sustitutosValidos.length;
    if (descartados > 0) {
        var motivoCosto = sustitutosConCostoDistinto.length > 0
            ? " por tener un costo diferente al producto principal"
            : "";
        ControlaMensajeWarning(`Se descartaron ${descartados} producto(s) no válido(s) como sustituto(s)${motivoCosto}`);
    }
}

/**
 * Configura eventos para los botones de eliminación de sustitutos
 */
function configurarEventosEliminacionSustitutos() {
    // Remover eventos previos para evitar duplicación
    $(document).off("click", ".btn-eliminar-sustituto");

    // Configurar evento de click para eliminar sustitutos
    $(document).on("click", ".btn-eliminar-sustituto", function (e) {
        e.stopPropagation(); // Evitar que se active la selección de fila
        if (!edicionEstructuraPermitida) {
            ControlaMensajeWarning("La estructura solo puede modificarse en estado Sin Activar");
            return;
        }

        var $fila = $(this).closest("tr");
        var sustitutoId = $(this).data("producto-id");
        var sustitutoDesc = $fila.find("td:nth-child(2)").text().trim();

        // Obtener el ID del producto seleccionado actualmente
        var productoId = $("#tbGridProductos tbody tr.selected-row").data("producto-id") ||
            $("#tbGridProductos tbody tr.selected-row td:first").text().trim();

        // Confirmar eliminación
        AbrirMensaje(
            "ELIMINAR SUSTITUTO",
            `¿Está seguro que desea eliminar el producto sustituto "${sustitutoDesc}"?`,
            function (resp) {
                if (resp === "SI") {
                    eliminarSustitutoDeGrid($fila, sustitutoId, productoId);
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
}

/**
 * Elimina un sustituto del grid y del mapa de sustitutos
 */
function eliminarSustitutoDeGrid($fila, sustitutoId, productoId) {
    // Eliminar del mapa de sustitutos
    if (productoId && productosSustitutosMap[productoId]) {
        productosSustitutosMap[productoId] = productosSustitutosMap[productoId].filter(s => s.p_id !== sustitutoId);

        // Guardar cambios en sesión
        guardarSustitutosEnSesion();

        // ✅ NUEVO: Llamar a la acción específica de eliminación en el servidor
        if (typeof eliminarSustitutoUrl !== 'undefined') {
            eliminarSustitutoEnServidor(productoId, sustitutoId);
        }
    }

    // Si es la única fila, mostrar mensaje "No hay sustitutos"
    if ($("#tbGridSustitutos tbody tr").length === 1) {
        $("#tbGridSustitutos tbody").html(`

            <tr>
                <td colspan="${modoNuevoCombo ? 5 : 4}" class="text-center text-muted py-2">
                    <i class="bx bx-info-circle me-1"></i>No hay sustitutos disponibles
                </td>
            </tr>
        `);
    } else {
        // Eliminar la fila
        $fila.remove();
    }

    // Mostrar mensaje de éxito
    ControlaMensajeSuccess("Sustituto eliminado correctamente");
}

/**
 * Elimina un sustituto específico en el servidor
 * @param {string} productoId - ID del producto principal
 * @param {string} sustitutoId - ID del sustituto a eliminar
 */
function eliminarSustitutoEnServidor(productoId, sustitutoId) {
    $.ajax({
        url: eliminarSustitutoUrl,
        type: "POST",
        data: {
            productoId: productoId,
            sustitutoId: sustitutoId
        },
        success: function (response) {
            if (response && response.ok) {
                console.log("✅ Sustituto eliminado del servidor:", response.mensaje);
                console.log("Sustitutos restantes:", response.cantidadRestante);
            } else {
                console.warn("⚠️ Advertencia al eliminar sustituto:", response.mensaje);
            }
        },
        error: function (xhr, status, error) {
            console.error("❌ Error al eliminar sustituto del servidor:", error);
            // No mostramos mensaje al usuario porque ya eliminamos del cliente
            // y no queremos bloquear la experiencia
        }
    });
}

/**
 * Configura eventos para los botones de eliminación de productos
 */
function configurarEventosEliminacionProductos() {
    // Remover eventos previos para evitar duplicación
    $(document).off("click", ".btn-eliminar-producto");

    // Configurar evento de click para eliminar productos
    $(document).on("click", ".btn-eliminar-producto", function (e) {
        e.stopPropagation(); // Evitar que se active la selección de fila
        if (!edicionEstructuraPermitida) {
            ControlaMensajeWarning("La estructura solo puede modificarse en estado Sin Activar");
            return;
        }

        var $fila = $(this).closest("tr");
        var productoId = $(this).data("producto-id");
        var productoDesc = $fila.find("td:nth-child(2)").text().trim();

        // Confirmar eliminación
        AbrirMensaje(
            "ELIMINAR PRODUCTO",
            `¿Está seguro que desea eliminar el producto "${productoDesc}" de este combo?`,
            function (resp) {
                if (resp === "SI") {
                    eliminarProductoDeGrid($fila, productoId);
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
}

/**
 * ✅ SIMPLIFICADO: Elimina un producto con colspan FIJO (siempre incluye columna descuento)
 */
function eliminarProductoDeGrid($fila, productoId) {
    // Eliminar del mapa de productos (por si acaso está en modo edición)
    if (modoNuevoCombo) {
        productosSustitutosMap = Object.fromEntries(
            Object.entries(productosSustitutosMap).map(([key, value]) => [
                key,
                value.filter(p => p.p_id !== productoId)
            ])
        );

        // Guardar cambios en sesión
        guardarSustitutosEnSesion();
    }

    // ✅ SIMPLIFICADO: Colspan SIEMPRE es 6 (ID, Desc, Costo, Cantidad, Descuento, Acción)
    const colspan = modoNuevoCombo ? 6 : 5;

    // Si es la única fila, mostrar mensaje "No hay productos"
    if ($("#tbGridProductos tbody tr").length === 1) {
        $("#tbGridProductos tbody").html(`
            <tr>
                <td colspan="${colspan}" class="text-center text-muted py-2">
                    <i class="bx bx-info-circle me-1"></i>No hay productos disponibles
                </td>
            </tr>
        `);
    } else {
        // Eliminar la fila
        $fila.remove();
    }

    // Mostrar mensaje de éxito
    ControlaMensajeSuccess("Producto eliminado correctamente");
}

/**
 * Realiza la búsqueda de combos según los filtros
 */
function buscarCombos(pag = 1) {
    // Verificar que la URL está definida
    if (typeof presentarPromosYCombosUrl === 'undefined') {
        console.error("URL para presentar promos y combos no definida");
        ControlaMensajeError("Error de configuración: URL de búsqueda no definida");
        return;
    }

    if (!$("#chkEstado").prop("checked") || !$("#Estado").val()) {
        ControlaMensajeWarning("Debe seleccionar un Estado antes de consultar");
        $("#Estado").trigger("focus");
        return;
    }

    // Mostrar mensaje de espera
    AbrirWaiting("Buscando promos y combos...");

    // Construir objeto de filtros
    var filtros = {
        Tipo: $("#chkTipo").prop("checked") ? $("#Tipo").val() : null,
        Estado: $("#chkEstado").prop("checked") ? $("#Estado").val() : null,
        Pagina: pag
    };

    //pagina es la variable que define en el plugin que pagina se esta mostrando
    pagina = pag;

    // Realizar la búsqueda
    $.ajax({
        url: presentarPromosYCombosUrl,
        type: "POST",
        data: filtros,
        success: function (html) {
            CerrarWaiting();
            realizaAlgunaBusqueda = true;
            // Ocultar el panel de filtros y mostrar el de resultados
            $("#divFiltro").collapse("hide");
            $("#divDetalle").collapse("show");

            // Mostrar resultados en el contenedor
            $("#divDetalle").html(html);

            // Configurar eventos para la paginación y selección de filas
            configurarEventosPaginacion();
            configurarEventosSeleccion();
            comboSeleccionadoId = null;
            comboSeleccionadoEstado = null;
            $("#divTools").hide();
            $("#btnDetalle").prop("disabled", false);
            $("#btnAbmNuevo").prop("disabled", $("#Estado").val() !== 'N');
            $("#btnAbmModif").prop("disabled", true);

            //armado de la paginacion
            PostGen({}, buscarMetadataURL, function (obj) {
                if (obj.error === true) {
                    AbrirMensaje("ATENCIÓN", obj.msg, function () {
                        $("#msjModal").modal("hide");
                        return true;
                    }, false, ["Aceptar"], "error!", null);
                }
                else {
                    totalRegs = obj.metadata.totalCount;
                    pags = obj.metadata.totalPages;
                    pagRegs = obj.metadata.pageSize;

                    $("#pagEstado").val(true).trigger("change");
                }
            });
        },
        error: function (xhr, status, error) {
            CerrarWaiting();
            console.error("Error en la búsqueda: ", error);
            ControlaMensajeError("Error al buscar promos y combos: " + error);
        }
    });
}

/**
 * Configura los eventos de paginación en la tabla de resultados
 */
function configurarEventosPaginacion() {
    // Configurar eventos de paginación
    $(".pagination .page-link").off("click").on("click", function (e) {
        e.preventDefault();
        var pagina = $(this).data("page") || 1;

        // Reconstruir los filtros
        var filtros = {
            Tipo: $("#chkTipo").prop("checked") ? $("#Tipo").val() : null,
            Estado: $("#chkEstado").prop("checked") ? $("#Estado").val() : null,
            Pagina: pagina,
            Registros: 10
        };

        cargarPagina(filtros, pagina);
    });
}

/**
 * Configura los eventos para la selecci贸n de filas en la tabla
 */
/**
 * Configura los eventos para la selección de filas en la tabla
 */
function configurarEventosSeleccion(e) {
    // Aplicar estilo de cursor a todas las filas de la tabla
    $("#tbGridPromoCombo tbody tr").css("cursor", "pointer");

    // Remover eventos previos para evitar duplicación
    $(document).off("click dblclick", "#tbGridPromoCombo tbody tr");

    // Un clic selecciona; el doble clic ingresa al detalle, como el resto de los ABM.
    $(document).on("click", "#tbGridPromoCombo tbody tr", function (e) {
        if (!$(e.target).is("button, a, .btn, i")) {
            var $this = $(this);
            $("#tbGridPromoCombo tbody tr").removeClass("selected-row selectedEdit-row");
            $this.addClass("selected-row");
            comboSeleccionadoId = String($this.data("combo-id") || $this.find("td:first").text().trim());
            comboSeleccionadoEstado = String($this.find("[data-estado-id]").data("estado-id") || '');
            $("#btnAbmModif").prop("disabled", true);
            actualizarContadorSeleccionados();
        }
    });

    $(document).on("dblclick", "#tbGridPromoCombo tbody tr", function (e) {
        if ($(e.target).is("button, a, .btn, i")) return;
        $(this).trigger("click").addClass("selectedEdit-row");
        abrirDetalleCombo(comboSeleccionadoId, comboSeleccionadoEstado);
    });

    // Inicializar contador
    actualizarContadorSeleccionados();
}

/**
 * Actualiza el contador de elementos seleccionados
 */
function actualizarContadorSeleccionados() {
    var selectedCount = $("#tbGridPromoCombo tbody tr.selected-row").length;
    $("#combosSeleccionados").text(selectedCount);
}

// Helper: refresca el grid de combos usando los filtros y página actuales
function refrescarGridPromoCombo() {
    var pagActual = parseInt(window.pagina, 10);
    if (!Number.isFinite(pagActual) || pagActual < 1) pagActual = 1;
    buscarCombos(pagActual);
}


function accionesIniciales(callback) {
    if ($("#divDetalle").is(":visible")) {
        $("#divDetalle").collapse("hide");
    }

    $("#btnDetalle").prop("disabled", true);

    // Habilitar los campos de filtro por defecto
    $("#Tipo").prop("disabled", false);
    $("#Estado").prop("disabled", false);


    // Las acciones ABM se habilitan recién después de una consulta válida.
    $("#btnAbmNuevo").prop("disabled", true);
    $("#btnAbmModif").prop("disabled", true);
    //ocultamos el boton de eliminar
    $("#btnAbmElimi").hide();

    //inician ocultos los botones cancelar y confirmar
    ActivarBtnAC(false);

    $("#pagEstado").off("change").on("change", function () {
        var div = $("#divPaginacion");
        presentaPaginacion(div);
    });


    // Delegación de eventos para autocomplete en el modal
    $(document).off("autocompleteselect", "#busquedaModal #Rel01B2")
        .on("autocompleteselect", "#busquedaModal #Rel01B2", function (event, ui) {
            setTimeout(function () {
                cargarFamiliasParaBusquedaAvanzadaCombos(ui.item.id);
            }, 100);
        });

    if (realizaAlgunaBusqueda) {
        // Ejecutar callback si existe
        if (typeof callback === "function") {
            callback();
        }
    }
    else {
        $("#divFiltro").collapse("show");
    }
}

function abrirDetalleCombo(comboId, estado) {
    if (!comboId) return;
    detalleCargaVersion += 1;
    var version = detalleCargaVersion;
    comboSeleccionadoId = String(comboId);
    comboSeleccionadoEstado = String(estado || '');

    $("#divPromoCombo").css("max-height", "210px");
    $("#divTools").show();
    $("#btnAbmNuevo").prop("disabled", true);
    $("#btnAbmModif").prop("disabled", comboSeleccionadoEstado === 'H');
    cargarDatosCombo(comboId, version);
    cargarCanalesCombo(comboId, version);
    cargarProductosCombo(comboId, version);
}

function esCargaComboVigente(comboId, version) {
    return String(comboSeleccionadoId) === String(comboId) && version === detalleCargaVersion;
}

function configurarPresentacionEstado(estado, permitirTransicion = true) {
    var configuracion = {
        N: { texto: 'Sin Activar', clase: 'bg-warning text-dark', accion: 'Activar' },
        A: { texto: 'Activo', clase: 'bg-success', accion: 'Pasar a Histórico' },
        H: { texto: 'Histórico', clase: 'bg-secondary', accion: null }
    }[estado] || { texto: 'Sin selección', clase: 'bg-secondary', accion: null };

    $("#lblEstadoCombo").val(configuracion.texto);
    $("#estadoComboBadge")
        .removeClass("bg-warning bg-success bg-danger bg-secondary text-dark")
        .addClass(configuracion.clase)
        .text(configuracion.texto.toUpperCase());

    var $boton = $("#btnCambiarEstadoCombo");
    if (permitirTransicion && configuracion.accion && !modoNuevoCombo && !modoModificacionCombo) {
        $boton.text(configuracion.accion).show();
    } else {
        $boton.hide();
    }
}

function cargarFamiliasParaBusquedaAvanzadaCombos(proveedorId) {
    if (!proveedorId) return;

    // Habilitar dropdown y mostrar indicador de carga
    var combo = $("#busquedaModal #Rel03B2");
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

/**
 * Carga el modal de búsqueda avanzada si no existe en el DOM
 */
function cargarModalBusquedaAvanzada(callback) {
    // Verificar si el modal ya existe
    if ($("#busquedaModal").length === 0) {
        // Si no existe, cargarlo mediante AJAX
        $.ajax({
            url: busquedaAvanzadaUrl,
            type: "GET",
            success: function (html) {
                // Agregar el HTML al final del body
                $("body").append(html);

                // Configurar eventos del modal de búsqueda avanzada
                configurarEventosBusquedaAvanzada();

                // Ejecutar callback si existe
                if (typeof callback === "function") {
                    callback();
                }
            },
            error: function (xhr, status, error) {
                console.error("Error al cargar el modal de búsqueda avanzada:", error);
                ControlaMensajeError("Error al cargar la búsqueda avanzada: " + error);
            }
        });
    } else if (typeof callback === "function") {
        // Si ya existe el modal, ejecutar callback directamente
        callback();
    }
}

/**
 * Configura los eventos para el modal de búsqueda avanzada
 */
function configurarEventosBusquedaAvanzada() {
    // Cerrar modal al hacer clic en el botón de cierre
    $(".buscAdv").on("click", function () {
        $("#busquedaModal").modal("hide");
    });

    // Evento para el botón de búsqueda
    $("#btnBuscarProd").on("click", function () {
        buscarProductos();
    });

    // Evento para agregar productos seleccionados
    $("#btnAgregarSeleccionados").on("click", function () {
        agregarProductosSeleccionados();
    });

    // Evento para limpiar selección
    $("#btnLimpiarSeleccionBusqueda").on("click", function () {
        limpiarSeleccionBusqueda();
    });

    // Configurar el comportamiento del Enter en el campo de búsqueda
    $("#Search").on("keypress", function (e) {
        if (e.which === 13) {
            e.preventDefault();
            buscarProductos();
        }
    });
}

/**
 * Busca productos según los filtros del modal
 */
function buscarProductos() {
    var filtros = {
        Rel01: $("#Rel01").val(),
        Rel01Item: $("#Rel01Item").val(),
        Rel02: $("#Rel02").val(),
        Rel02Item: $("#Rel02Item").val(),
        Rel03: $("#Rel03").val(),
        EstadoActivo: $("#chkActivos").prop("checked"),
        EstadoDiscont: $("#chkDisc").prop("checked"),
        EstadoInactivo: $("#chkInact").prop("checked"),
        ConStock: $("input[name=ConStock]:checked").val(),
        Search: $("#Search").val()
    };

    AbrirWaiting("Buscando productos...");

    $.ajax({
        url: busquedaProdBaseUrl,
        type: "POST",
        data: filtros,
        success: function (html) {
            CerrarWaiting();
            $("#divBusquedaAvanzada").html(html);

            // Mostrar sección de selección múltiple
            $("#seccionSeleccionMultiple").show();

            // Configurar eventos para seleccionar productos
            configurarSeleccionProductosBusqueda();
        },
        error: function (xhr, status, error) {
            CerrarWaiting();
            console.error("Error en la búsqueda de productos:", error);
            ControlaMensajeError("Error al buscar productos: " + error);
        }
    });
}

/**
 * Configura eventos para seleccionar productos en la búsqueda
 */
function configurarSeleccionProductosBusqueda() {
    // Agregar clase 'selectable' a todas las filas
    $("#divBusquedaAvanzada table tbody tr").addClass("selectable");

    // Remover eventos previos
    $("#divBusquedaAvanzada table tbody tr.selectable").off("click");

    // Configurar evento de clic para seleccionar/deseleccionar filas
    $("#divBusquedaAvanzada table tbody tr.selectable").on("click", function () {
        $(this).toggleClass("selected-row");

        // Actualizar contador
        actualizarContadorProductosSeleccionados();
    });
}

/**
 * Actualiza el contador de productos seleccionados
 */
function actualizarContadorProductosSeleccionados() {
    var count = $("#divBusquedaAvanzada table tbody tr.selected-row").length;
    $("#contadorSeleccionados").text(count);
}

/**
 * Limpia la selección de productos en la búsqueda
 */
function limpiarSeleccionFilasLegacyCombo() {
    $("#divBusquedaAvanzada table tbody tr").removeClass("selected-row");
    actualizarContadorProductosSeleccionados();
}

/**
 * Agrega los productos seleccionados al grid de productos
 */
function agregarProductosSeleccionados() {
    var productosSeleccionados = [];

    // Obtener información de productos seleccionados
    $("#divBusquedaAvanzada table tbody tr.selected-row").each(function () {
        var $row = $(this);
        var producto = {
            p_id: $row.find("td:eq(0)").text().trim(),
            p_desc: $row.find("td:eq(1)").text().trim(),
            p_pcosto: parseFloat($row.find("td:eq(2)").text().replace(/[^\d.-]/g, '')) || 0,
            cantidad: 1, // Valor por defecto
            dto_porc: 0, // Valor por defecto
            activo: 'A' // Activo por defecto
        };
        productosSeleccionados.push(producto);
    });

    // Agregar productos al grid
    agregarProductosAlGrid(productosSeleccionados);

    // Ocultar el modal
    $("#busquedaModal").modal("hide");

    // Limpiar selección
    limpiarSeleccionFilasLegacyCombo();
}

/**
 * ✅ MODIFICADO: Limpia los grids considerando tipo de combo
 */
function limpiarGridsProductos(modoEdicion = modoNuevoCombo) {
    // ✅ NUEVO: Determinar tipo de combo
    const tipoCombo = $("#cmb_tipo").val() || 'C';
    const usaDescuento = (tipoCombo === 'P' || tipoCombo === 'C');

    // ✅ CRÍTICO: Crear SIEMPRE ambos contenedores en modo edición (inicialmente ocultos)
    var htmlPreajusteDropdown = '';
    var htmlImporteUnico = '';

    if (modoEdicion && modoNuevoCombo) {
        // Contenedor de preajustes (inicialmente oculto, se mostrará solo para tipo P)
        htmlPreajusteDropdown = `
        <div id="contenedorPreajuste" class="d-inline-block me-2 preajuste-hidden">
            <!-- El dropdown se cargará aquí dinámicamente -->
        </div>`;

        // Contenedor de importe único (inicialmente oculto, se mostrará para Q/D)
        htmlImporteUnico = `
        <div id="contenedorImporteUnico" class="d-inline-block me-2 importe-hidden">
            <label class="form-label mb-0 me-1" style="font-size: 0.875rem;">Importe:</label>
            <input type="text" 
                   id="importeUnico" 
                   class="form-control form-control-sm d-inline-block" 
                   style="width: 100px;" 
                   placeholder="0.00" 
                   title="Importe único para todos los productos" />
        </div>`;
    }

    // ✅ CRÍTICO: SIEMPRE crear columna descuento en el header, controlar visibilidad con clase
    const claseHeaderDescuento = usaDescuento ? '' : 'd-none';

    // Crear HTML para un grid vacío de productos
    var htmlProductosVacio = `
    <div class="card h-100">
        <div class="card-header py-1 d-flex justify-content-between align-items-center">
            <h6 class="mb-0">Productos</h6>
            ${modoEdicion ? `
            <div class="d-flex align-items-center">
                ${htmlPreajusteDropdown}
                ${htmlImporteUnico}
                <button type="button" class="btn btn-sm btn-outline-primary" id="btnAgregarCProducto" title="Agregar Producto">
                    <i class="bx bx-plus" style="font-size: 24px;"></i>
                </button>
            </div>
            ` : ''}
        </div>
        <div class="card-body p-1">
            <div class="table-responsive" style="max-height: 250px;">
                <table class="table table-sm table-hover mb-0 table-golden" id="tbGridProductos">
                    <thead class="sticky-top table-golden-header-compact">
                        <tr class="header">
                            <th class="text-center th-compact">ID</th>
                            <th class="text-left th-compact">Descripción</th>
                            <th class="text-center th-compact">Costo</th>
                            <th class="text-center th-compact">Cantidad</th>
                            <th class="text-center th-compact ${claseHeaderDescuento}">Descuento %</th>
                            ${modoEdicion ? '<th class="text-center th-compact">Acción</th>' : ''}
                        </tr>
                    </thead>
                    <tbody>
                        <tr>
                            <td colspan="${modoEdicion ? 6 : 5}" class="text-center text-muted py-2">
                                <i class="bx bx-info-circle me-1"></i>No hay productos disponibles
                            </td>
                        </tr>
                    </tbody>
                </table>
            </div>
        </div>
    </div>`;

    // Crear HTML para un grid vacío de sustitutos (sin cambios)
    var htmlSustitutosVacio = `
    <div class="card h-100">
        <div class="card-header py-1 d-flex justify-content-between align-items-center">
            <h6 class="mb-0">Sustitutos</h6>
            ${modoEdicion ? `
            <button type="button" class="btn btn-sm btn-outline-primary" id="btnAgregarSustituto" title="Agregar Sustituto">
                <i class="bx bx-plus" style="font-size: 24px;"></i>
            </button>
            ` : ''}
        </div>
        <div class="card-body p-1">
            <div class="table-responsive" style="max-height: 250px;">
                <table class="table table-sm table-hover mb-0 table-golden" id="tbGridSustitutos">
                    <thead class="sticky-top table-golden-header-compact">
                        <tr class="header">
                            <th class="text-center th-compact">ID</th>
                            <th class="text-left th-compact">Descripción</th>
                            <th class="text-center th-compact">Costo</th>
                            ${modoEdicion ? '<th class="text-center th-compact">Acción</th>' : ''}
                        </tr>
                    </thead>
                    <tbody>
                        <tr>
                            <td colspan="${modoEdicion ? 4 : 3}" class="text-center text-muted py-2">
                                <i class="bx bx-info-circle me-1"></i>No hay sustitutos disponibles
                            </td>
                        </tr>
                    </tbody>
                </table>
            </div>
        </div>
    </div>`;

    // Actualizar los contenedores con los grids vacíos
    $("#colComboProducto").html(`<div id="divComboProducto">${htmlProductosVacio}</div>`).show();
    $("#colComboSustituto").html(`<div id="divComboSustituto">${htmlSustitutosVacio}</div>`).show();

    // ✅ MEJORADO: Cargar dropdown de preajustes solo para tipo P
    if (modoEdicion && modoNuevoCombo && tipoCombo === 'P') {
        setTimeout(function () {
            var $contenedorPreajuste = $("#contenedorPreajuste");
            if ($contenedorPreajuste.length > 0) {
                cargarDropdownPreajuste($contenedorPreajuste);
            }
        }, 100);
    }

    // ✅ NUEVO: Configurar InputMask para importe único (Q/D)
    if (modoEdicion && modoNuevoCombo && (tipoCombo === 'Q' || tipoCombo === 'D')) {
        setTimeout(function () {
            var $importeUnico = $("#importeUnico");
            if ($importeUnico.length > 0 && typeof Inputmask !== 'undefined') {
                Inputmask({
                    alias: "numeric",
                    groupSeparator: ",",
                    radixPoint: ".",
                    autoGroup: true,
                    digits: 2,
                    digitsOptional: false,
                    rightAlign: true,
                    allowMinus: false,
                    min: 0
                }).mask('#importeUnico');
            }
        }, 100);
    }

    // ✅ CRÍTICO: Aplicar visibilidad según el tipo actual DESPUÉS de crear los contenedores
    setTimeout(function () {
        actualizarVisibilidadSegunTipo(tipoCombo);
    }, 150);

    // Habilitar los botones de agregar solo en modo edición
    if (modoEdicion) {
        var $btnAgregar = $("#btnAgregarCProducto, #btnAgregarSustituto");
        if ($btnAgregar.length > 0) {
            $btnAgregar.prop("disabled", false);
        }
    }
}

/**
 * Guarda la relación producto-sustituto en el servidor
 */
function guardarRelacionProductoSustitutoEnServidor(productoId) {
    // Verificar que existan sustitutos para este producto
    if (!productosSustitutosMap[productoId] || productosSustitutosMap[productoId].length === 0) {
        console.log("No hay sustitutos para guardar para el producto:", productoId);
        return;
    }

    // Filtrar una vez más para asegurar que no hay sustitutos inválidos
    var sustitutosValidos = productosSustitutosMap[productoId].filter(sustituto =>
        sustituto.p_id !== productoId
    );

    // Transformar los objetos de sustitutos al formato que espera el controlador
    var sustitutos = sustitutosValidos.map(function (sustituto) {
        return {
            cmb_id: "",                     // Vacío para nuevos registros
            p_id: productoId,               // ID del producto principal
            p_id_sustituto: sustituto.p_id, // ID del producto sustituto
            p_desc: sustituto.p_desc,       // Descripción del sustituto
            p_pcosto: sustituto.p_pcosto,   // Precio de costo del sustituto
            activo: "A"                     // Por defecto activo
        };
    });

    // Enviar al servidor
    $.ajax({
        url: resguardarRelacionProductoSustitutoUrl,
        type: "POST",
        contentType: "application/json",
        data: JSON.stringify({
            p_id: productoId,
            sus: sustitutos
        }),
        success: function (response) {
            if (response && response.ok) {
                console.log("Relación producto-sustituto guardada correctamente");
                console.log("Sustitutos guardados:", sustitutos.length);
            } else {
                ControlaMensajeWarning(response.mensaje || "No se pudo guardar la relación producto-sustituto");
                console.warn("Error al guardar relación:", response);
            }
        },
        error: function (xhr, status, error) {
            console.error("Error al guardar relación producto-sustituto:", error);
            ControlaMensajeError("Error al guardar relación producto-sustituto: " + error);
        }
    });
}

/**
 * Cancela la operación currente y restaura el estado inicial
 */
function cancelarOperacion(e, traerCombos = true) {
    var eraNuevo = modoNuevoCombo;
    var eraModificacion = modoModificacionCombo;
    var idSeleccionado = comboSeleccionadoId;
    var estadoSeleccionado = comboSeleccionadoEstado;

    activarGrilla("tbGridPromoCombo");
    $("#divPromoCombo").css("max-height", "500px");

    // Desactivar modos de edición
    modoNuevoCombo = false;
    modoModificacionCombo = false;
    edicionEstructuraPermitida = false;

    // Restaurar estado de los campos
    restaurarCamposFormulario();

    // Limpiar grid de canales
    $("#divCanales").empty();

    // Limpiar los grids de productos y sustitutos
    limpiarGridsProductos();

    // Deshabilitar explícitamente los botones de agregar después de limpiar grids
    $("#btnAgregarCProducto, #btnAgregarSustituto").prop("disabled", true);

    // Restaurar estado de los botones según el filtro vigente
    $("#btnAbmNuevo").prop("disabled", $("#Estado").val() !== 'N');
    $("#btnAbmAceptar").prop("disabled", true);
    $("#btnAbmModif").prop("disabled", true); // Deshabilitar botón modificar también

    //inician ocultos los botones cancelar y confirmar
    ActivarBtnAC(false);

    if (typeof limpiarSustitutosTemporalesUrl !== 'undefined') {
        $.post(limpiarSustitutosTemporalesUrl);
    }

    if (traerCombos && eraModificacion && idSeleccionado) {
        $("#tbGridPromoCombo tbody tr").removeClass("selectedEdit-row");
        var $fila = $(`#tbGridPromoCombo tbody tr[data-combo-id="${idSeleccionado}"]`);
        $fila.addClass("selected-row");
        abrirDetalleCombo(idSeleccionado, estadoSeleccionado);
    } else {
        $("#divComboDatos").hide();
        $("#divTools").hide();
        $("#colComboProducto, #colComboSustituto").show();
        if (eraNuevo) $("#divDetalle").collapse("show");
    }

}

/**
 * Restaura el estado original de los campos del formulario
 */
function restaurarCamposFormulario() {
    // Restaurar campos a su estado original (readonly/disabled)
    $("#cmb_desc").prop("readonly", true).val("");
    $("#cmb_tipo").prop("disabled", true);
    $("#lblEstadoCombo").val("Sin selección");
    $("#cmb_desde, #cmb_hasta").prop("readonly", true).val("");
    $("#cmb_id").val("");
    $("#cmb_estado").val("");

    // Restaurar badge de estado
    $("#estadoComboBadge").removeClass("bg-success bg-danger bg-warning text-dark").addClass("bg-secondary").text("SIN SELECCIÓN");
    $("#btnCambiarEstadoCombo").hide();
}

/**
 * Carga los canales asociados a un combo
 */
function cargarCanalesCombo(comboId, version) {
    if (typeof obtenerCanalesComboUrl === 'undefined') {
        console.error("URL para obtener canales no definida");
        return;
    }

    AbrirWaiting("Cargando canales...");

    $.ajax({
        url: obtenerCanalesComboUrl,
        type: "POST",
        data: { id: comboId },
        success: function (html) {
            CerrarWaiting();
            if (!esCargaComboVigente(comboId, version)) return;
            $("#divCanales").html(html);
        },
        error: function (xhr, status, error) {
            CerrarWaiting();
            console.error("Error al cargar canales: ", error);
            ControlaMensajeError("Error al cargar canales: " + error);
        }
    });
}

/**
 * Carga los datos del combo seleccionado y los muestra en el formulario
 */
function cargarDatosCombo(comboId, version) {
    // 🔧 OPTIMIZACIÓN: Validación temprana
    if (!comboId || comboId === '') {
        console.error("ComboId no válido:", comboId);
        ControlaMensajeWarning("El identificador del combo no es válido");
        return;
    }

    if (typeof obtenerComboPorIdUrl === 'undefined') {
        console.error("URL para obtener datos del combo no definida");
        ControlaMensajeError("Error de configuración: URL no definida");
        return;
    }

    AbrirWaiting("Cargando datos del combo...");

    $.ajax({
        url: obtenerComboPorIdUrl,
        type: "POST",
        data: { id: comboId },
        dataType: "json",
        success: function (response) {
            CerrarWaiting();
            if (!esCargaComboVigente(comboId, version)) return;

            if (response && response.ok) {
                // Mostrar el panel de datos
                $("#divComboDatos").show();

                // Rellenar los campos del formulario con los datos recibidos
                var datos = response.entidad;
                if (datos) {
                    // Establecer valores en los campos
                    $("#cmb_id").val(datos.cmb_id);
                    $("#cmb_desc").val(datos.cmb_desc).prop("readonly", true);
                    
                    // ✅ CRÍTICO: cmb_tipo SIEMPRE bloqueado para combos existentes
                    $("#cmb_tipo").val(datos.cmb_tipo).prop("disabled", true);
                    console.log("🔒 Campo cmb_tipo bloqueado para combo existente");

                    $("#cmb_estado").val(datos.cmb_estado);
                    configurarPresentacionEstado(datos.cmb_estado);

                    // Actualizar fechas
                    $("#cmb_desde").val(formatearFecha(datos.cmb_desde)).prop("readonly", true);
                    $("#cmb_hasta").val(formatearFecha(datos.cmb_hasta)).prop("readonly", true);
                }
            } else {
                var mensaje = response?.mensaje || "Error desconocido al obtener datos del combo";
                console.error("Error en respuesta:", mensaje);
                ControlaMensajeError(mensaje);
            }
        },
        error: function (xhr, status, error) {
            CerrarWaiting();

            // 🔧 OPTIMIZACIÓN: Mejor manejo de errores
            var mensajeError = "Error al cargar datos del combo";

            if (xhr.responseJSON && xhr.responseJSON.mensaje) {
                mensajeError = xhr.responseJSON.mensaje;
            } else if (xhr.status === 404) {
                mensajeError = "No se encontró el combo especificado";
            } else if (xhr.status === 401) {
                mensajeError = "No autorizado. Por favor, inicie sesión nuevamente";
            } else if (xhr.status === 500) {
                mensajeError = "Error interno del servidor al obtener datos del combo";
            }

            console.error("Error AJAX:", {
                status: xhr.status,
                statusText: xhr.statusText,
                error: error,
                response: xhr.responseText
            });

            ControlaMensajeError(mensajeError);
        }
    });
}

/**
 * Inicializa los campos para un nuevo combo
 */
function inicializarNuevoCombo() {
    $("#divDetalle").collapse("hide")
    // Mostrar el panel de datos
    $("#divComboDatos").show();

    // Limpiar y configurar los campos
    $("#cmb_id").val(""); // Mantener readonly

    // Habilitar y limpiar campos editables
    $("#cmb_desc").val("").prop("readonly", false);

    // Configurar campo tipo
    $("#cmb_tipo").prop("disabled", false);
    if ($("#cmb_tipo option").length > 1) {
        $("#cmb_tipo option:eq(1)").prop("selected", true); // Seleccionar primera opción válida
    }

    // Los nuevos registros siempre nacen Sin Activar.
    $("#cmb_estado").val("N");
    configurarPresentacionEstado('N', false);

    // Configurar fechas
    const hoy = new Date();
    const tresMesesDespues = new Date(hoy);
    tresMesesDespues.setMonth(hoy.getMonth() + 3);

    $("#cmb_desde").val(formatearFecha(hoy)).prop("readonly", false);
    $("#cmb_hasta").val(formatearFecha(tresMesesDespues)).prop("readonly", false);

    // Actualizar badge de estado
    var estadoBadge = $("#estadoComboBadge");
    estadoBadge.removeClass("bg-success bg-danger bg-secondary bg-warning")
        .addClass("bg-danger")
        .text("SIN ACTIVAR");

    // Limpiar grids de productos y sustitutos
    limpiarGridsProductos();

    // Cargar canales disponibles
    cargarCanalesParaNuevoCombo();
}

/**
 * Carga los canales disponibles para un nuevo combo
 */
function cargarCanalesParaNuevoCombo() {
    if (typeof obtenerCanalesComboUrl === 'undefined') {
        console.error("URL para obtener canales no definida");
        return;
    }

    AbrirWaiting("Cargando canales disponibles...");

    $.ajax({
        url: obtenerCanalesComboUrl,
        type: "POST",
        data: { id: "nuevo" }, // Indicamos que es para un nuevo combo
        success: function (html) {
            CerrarWaiting();
            $("#divCanales").html(html);

            // Modificar la tabla para mostrar checkboxes y ocultar columna de estado
            adaptarGrillaCanales();
        },
        error: function (xhr, status, error) {
            CerrarWaiting();
            console.error("Error al cargar canales: ", error);
            ControlaMensajeError("Error al cargar canales: " + error);
        }
    });
}

/**
 * Adapta la grilla de canales para mostrar checkboxes y ocultar la columna de estado
 */
function adaptarGrillaCanales() {
    if ($("#tbGridCanales .canal-checkbox").length > 0) {
        actualizarCheckboxMaestroCanales();
        return;
    }

    // Añadir columna de selección en el encabezado
    //$("#tbGridCanales thead tr").prepend("<th class='text-center'>Selección</th>");
    $("#tbGridCanales thead tr").prepend(`
    <th class='text-center'>
        <div class='form-check'>
            <input class='form-check-input' type='checkbox' id='chkSeleccionarTodosCanales' 
                   title='Seleccionar/Deseleccionar todos'>
            <label class='form-check-label' for='chkSeleccionarTodosCanales'>Selección</label>
        </div>
    </th>`);

    // Añadir checkbox a cada fila
    $("#tbGridCanales tbody tr").each(function () {
        var canalId = $(this).find("td:eq(0)").text().trim();
        var incluido = canalEstaIncluido($(this));

        $(this).prepend(
            `<td class='text-center'>
                <div class='form-check'>
                    <input class='form-check-input canal-checkbox' type='checkbox' value='${canalId}' ${incluido ? 'checked' : ''}>
                </div>
            </td>`
        );
    });

    // ✅ NUEVO: Evento para el checkbox maestro
    $("#chkSeleccionarTodosCanales").on("change", function () {
        var isChecked = $(this).prop("checked");

        // Seleccionar/deseleccionar todos los canales
        $(".canal-checkbox").prop("checked", isChecked).trigger("change");

        console.log("📋 Canales " + (isChecked ? "seleccionados" : "deseleccionados") + " en masa");
    });

    // ✅ MEJORADO: Evento para checkboxes individuales con sincronización
    $(".canal-checkbox").on("change", function () {
        var checked = $(this).prop("checked");
        var $fila = $(this).closest("tr");
        var $badge = $fila.find("td:last .badge");
        $fila.data("incluida", checked ? 'S' : 'N').attr("data-incluida", checked ? 'S' : 'N');
        $badge.toggleClass("bg-success", checked)
            .toggleClass("bg-secondary", !checked)
            .text(checked ? "Incluido" : "No incluido");
        console.log("Canal " + $(this).val() + " " + (checked ? "seleccionado" : "deseleccionado"));

        // Actualizar estado del checkbox maestro
        actualizarCheckboxMaestroCanales();
    });

    // ✅ NUEVO: Inicializar estado del checkbox maestro
    actualizarCheckboxMaestroCanales();
}

/**
 * ✅ NUEVA FUNCIÓN: Actualiza el estado del checkbox maestro de canales
 * según la selección de checkboxes individuales
 */
function actualizarCheckboxMaestroCanales() {
    var totalCheckboxes = $(".canal-checkbox").length;
    var checkboxesMarcados = $(".canal-checkbox:checked").length;

    var $checkboxMaestro = $("#chkSeleccionarTodosCanales");

    if (checkboxesMarcados === 0) {
        // Ninguno seleccionado
        $checkboxMaestro.prop("checked", false).prop("indeterminate", false);
    } else if (checkboxesMarcados === totalCheckboxes) {
        // Todos seleccionados
        $checkboxMaestro.prop("checked", true).prop("indeterminate", false);
    } else {
        // Algunos seleccionados (estado indeterminado)
        $checkboxMaestro.prop("checked", false).prop("indeterminate", true);
    }
}

/**
 * Formatea una fecha en formato yyyy-MM-dd para inputs de tipo date
 */
function formatearFecha(fechaStr) {
    if (!fechaStr) return "";

    // Las fechas del negocio no deben convertirse a UTC: se conserva el día
    // recibido por el servidor para evitar desplazamientos por zona horaria.
    if (typeof fechaStr === 'string') {
        var iso = fechaStr.match(/^(\d{4})-(\d{2})-(\d{2})/);
        if (iso) return `${iso[1]}-${iso[2]}-${iso[3]}`;
    }

    var fecha = new Date(fechaStr);

    // Verificar si es una fecha válida
    if (isNaN(fecha.getTime())) return "";

    // Formatear como yyyy-MM-dd para input type="date"
    var anio = fecha.getFullYear();
    var mes = String(fecha.getMonth() + 1).padStart(2, '0');
    var dia = String(fecha.getDate()).padStart(2, '0');
    return `${anio}-${mes}-${dia}`;
}

function canalEstaIncluido($fila) {
    var valor = String($fila.data("incluida") || $fila.attr("data-incluida") || '').toUpperCase();
    if (valor === 'S' || valor === 'N') return valor === 'S';

    return $.trim($fila.find("td:last .badge").text()).toLowerCase() === 'incluido';
}

/**
 * ✅ MODIFICADO: Agrega productos SIEMPRE con celda descuento (visible u oculta según tipo)
 */
function agregarProductosAlGrid(productos) {
    if (productos.length === 0) return;

    // Obtener el tbody de la tabla
    var $tbody = $("#tbGridProductos tbody");

    // Limpiar mensaje "No hay productos" si existe
    if ($tbody.find("tr td[colspan]").length > 0) {
        $tbody.empty();
    }

    // Obtener ID del combo actual
    var comboId = $("#cmb_id").val();

    // ✅ NUEVO: Determinar tipo de combo y si usa descuento
    const tipoCombo = $("#cmb_tipo").val() || 'C';
    const usaDescuento = (tipoCombo === 'P' || tipoCombo === 'C');

    // ✅ NUEVO: Obtener valores según preset/tipo
    const valoresPreset = obtenerValoresPreset();

    // Agregar cada producto como una nueva fila
    $.each(productos, function (i, producto) {
        // ✅ NUEVO: Aplicar valores del preset/tipo
        producto.cantidad = valoresPreset.cantidad;
        producto.dto_porc = valoresPreset.descuento;
        producto.dto_imp = 0;

        // ✅ CRÍTICO: SIEMPRE generar la celda, pero con clase .d-none si no se usa
        var claseCeldaDescuento = usaDescuento ? '' : 'd-none';

        var fila = `
        <tr data-producto-id="${producto.p_id}" data-combo-id="${comboId}" data-producto-estado="${producto.activo}" data-up-id="${producto.up_id || ''}">
            <td class="text-center">
                ${producto.p_id}
            </td>
            <td class="promo-descripcion" title="${producto.p_desc}">
                ${producto.p_desc}
            </td>
            <td class="text-end">
                ${Number(producto.p_pcosto || 0).toLocaleString('en-US', { minimumFractionDigits: 3, maximumFractionDigits: 3 })}
            </td>
            <td class="text-end">
                <div class="input-container">
                    <input type="text" class="form-control form-control-sm input-cantidad input-numeric"
                           value="${Math.trunc(producto.cantidad)}"
                           data-producto-id="${producto.p_id}"
                           data-original-value="${producto.cantidad}"
                           readonly />
                </div>
            </td>
            <td class="text-end ${claseCeldaDescuento}">
                <div class="input-container">
                    <input type="text" class="form-control form-control-sm input-descuento input-numeric"
                           value="${producto.dto_porc.toFixed(5)}"
                           data-producto-id="${producto.p_id}"
                           data-original-value="${producto.dto_porc}"
                           readonly />
                </div>
            </td>            
            ${modoNuevoCombo ? `
            <td class="text-center">
                <button type="button" class="btn btn-sm btn-outline-danger btn-eliminar-producto" 
                        title="Eliminar producto" data-producto-id="${producto.p_id}">
                    <i class="bx bx-trash"></i>
                </button>
            </td>
            ` : ''}
        </tr>`;

        $tbody.append(fila);
    });

    // Configurar eventos de selección para los nuevos productos
    configurarSeleccionProductos();

    // Configurar eventos para los botones de eliminar si estamos en modo de edición
    if (modoNuevoCombo) {
        configurarEventosEliminacionProductos();
    }

    // Seleccionar el primer producto agregado
    var $primerProducto = $("#tbGridProductos tbody tr:first");
    $primerProducto.trigger("click");

    console.log(`✅ ${productos.length} productos agregados con Cantidad: ${valoresPreset.cantidad}, Descuento: ${valoresPreset.descuento}%`);
}

/**
 * Verifica si un producto tiene estado histórico
 * @param {string|jQuery} producto - ID del producto o elemento jQuery de la fila
 * @returns {boolean} true si es histórico, false en caso contrario
 */
function esProductoHistorico(producto) {
    // Si recibimos un ID, encontrar la fila
    var $fila = typeof producto === 'string' ?
        $(`#tbGridProductos tbody tr[data-producto-id="${producto}"], #tbGridProductos tbody tr td:first-child:contains("${producto}")`)
            .filter(function () {
                return $(this).text().trim() === producto || $(this).closest('tr').data('producto-id') === producto;
            }).closest('tr') :
        $(producto).closest('tr');

    if ($fila.length === 0) return false;

    // Verificar texto del badge en la columna de estado (6ta columna)
    var estadoTexto = $fila.find("td:nth-child(6) .badge").text().trim();
    return estadoTexto === "Histórico";
}

/**
 * ✅ MODIFICADO: Inicializa los campos editables controlando descuento según tipo
 */
function inicializarCamposEditablesProductos() {
    console.log("🔄 Inicializando campos editables en grid de productos");

    // Si no estamos en modo edición, mantener los campos readonly y salir
    if ((!modoNuevoCombo && !modoModificacionCombo) || !edicionEstructuraPermitida) {
        $('.input-cantidad, .input-descuento').prop('readonly', true);
        console.log("✅ Campos configurados como readonly (modo visualización)");
        return;
    }

    // ✅ NUEVO: Determinar si el descuento es editable según tipo
    const tipoCombo = $("#cmb_tipo").val();
    const descuentoNoEditable = (tipoCombo === 'Q' || tipoCombo === 'D');

    if (descuentoNoEditable) {
        $('.input-descuento').prop('readonly', true).addClass('campo-readonly');
        console.log("🚫 Descuento bloqueado para tipo Q/D");
    }

    // NUEVO: Capturar el evento mousedown que ocurre ANTES del click
    $(document).off('mousedown', '.input-cantidad, .input-descuento')
        .on('mousedown', '.input-cantidad, .input-descuento', function (e) {
            // Si no estamos en modo edición, no permitir la edición
            if ((!modoNuevoCombo && !modoModificacionCombo) || !edicionEstructuraPermitida) {
                e.preventDefault();
                return false;
            }

            // ✅ NUEVO: Bloquear edición de descuento si es tipo Q/D
            if ($(this).hasClass('input-descuento') && descuentoNoEditable) {
                e.preventDefault();
                return false;
            }

            // NUEVA VERIFICACIÓN: Comprobar si el producto es histórico
            var productoId = $(this).data('producto-id');
            if (esProductoHistorico(productoId)) {
                e.preventDefault();
                e.stopPropagation();
                return false;
            }

            // Marcar este elemento como "en preparación para edición"
            campoEnPreparacionEdicion = this;
        });

    // 1. Configurar campos editables al hacer clic
    $(document).off('click', '.input-cantidad, .input-descuento')
        .on('click', '.input-cantidad, .input-descuento', function (e) {
            // Si no estamos en modo edición, no permitir la edición
            if ((!modoNuevoCombo && !modoModificacionCombo) || !edicionEstructuraPermitida) {
                e.preventDefault();
                return false;
            }

            // ✅ NUEVO: Bloquear edición de descuento si es tipo Q/D
            if ($(this).hasClass('input-descuento') && descuentoNoEditable) {
                e.preventDefault();
                ControlaMensajeWarning("El descuento no es editable para este tipo de combo");
                return false;
            }

            // NUEVA VERIFICACIÓN: Comprobar si el producto es histórico
            var productoId = $(this).data('producto-id');
            if (esProductoHistorico(productoId)) {
                e.preventDefault();
                e.stopPropagation();
                ControlaMensajeWarning("No se puede modificar un producto con estado histórico");
                return false;
            }

            // Detener propagación
            e.stopPropagation();
            e.preventDefault();

            // Seleccionar la fila manualmente para mantener el contexto visual
            var $fila = $(this).closest('tr');
            $("#tbGridProductos tbody tr").removeClass("selected-row");
            $fila.addClass("selected-row");

            // Hacer editable el campo
            $(this)
                .prop('readonly', false)
                .data('editando', true)
                .removeClass('campo-readonly')
                .trigger("focus")
                .trigger("select");

            // Limpiar bandera de preparación
            campoEnPreparacionEdicion = null;

            return false;
        });

    // 2. Aplicar InputMask a los campos numéricos
    if (typeof Inputmask !== 'undefined') {
        // El SP recibe cantidad como INT.
        Inputmask({
            alias: "numeric",
            groupSeparator: ",",
            radixPoint: ".",
            autoGroup: true,
            digits: 0,
            digitsOptional: true,
            rightAlign: true,
            allowMinus: false,
            min: 0
        }).mask('.input-cantidad');

        // Configuración para descuento (5 decimales, máx 100%)
        Inputmask({
            alias: "numeric",
            groupSeparator: ",",
            radixPoint: ".",
            autoGroup: true,
            digits: 5,
            digitsOptional: false,
            rightAlign: true,
            allowMinus: false,
            min: 0,
            max: 100
        }).mask('.input-descuento');
    }

    // 3. Manejar evento Enter y Tab
    $(document).off('keydown', '.input-cantidad, .input-descuento').on('keydown', '.input-cantidad, .input-descuento', function (e) {
        if (e.key === 'Enter' || e.key === 'Tab') {
            e.preventDefault();

            // Guardar cambios
            guardarCambiosCampoProducto(this);

            // Activar el siguiente campo editable
            activarSiguienteCampoProducto(this);
        }
    });

    // 4. Manejar evento blur (perder foco)
    $(document).off('blur', '.input-cantidad, .input-descuento').on('blur', '.input-cantidad, .input-descuento', function () {
        // No hacer nada si ya está en modo readonly
        if ($(this).prop('readonly')) {
            return;
        }

        // Guardar cambios y marcar como no editando
        guardarCambiosCampoProducto(this);
        $(this).data('editando', false);
    });

    // 5. Manejar click fuera de campos para cancelar edición
    $(document).off('click.editablesProductos').on('click.editablesProductos', function (e) {
        if (!$(e.target).is('.input-cantidad, .input-descuento')) {
            $('.input-cantidad:not([readonly]), .input-descuento:not([readonly])').each(function () {
                guardarCambiosCampoProducto(this);
                $(this).data('editando', false);
            });
        }
    });

    console.log("✅ Campos editables inicializados");
}

/**
 * Verifica si hay campos en edición activa o a punto de editarse
 * @returns {boolean} true si hay algún campo en edición, false en caso contrario
 */
function hayEdicionActiva() {
    // NUEVO: Verificar si hay un campo en preparación para edición
    if (campoEnPreparacionEdicion !== null) {
        return true;
    }

    // Verificar campos en edición activa (existentes)
    var camposEditando = $('.input-cantidad[data-editando=true], .input-descuento[data-editando=true]').length > 0;
    var camposNoReadonly = $('.input-cantidad:not([readonly]), .input-descuento:not([readonly])').length > 0;
    var camposEnFoco = $('.input-cantidad:focus, .input-descuento:focus').length > 0;

    return camposEditando || camposNoReadonly || camposEnFoco;
}

/**
 * Guarda los cambios en un campo editable de producto (cantidad o descuento)
 * @param {HTMLElement} campo - El campo que se está editando
 */
function guardarCambiosCampoProducto(campo) {
    const $campo = $(campo);
    const productoId = $campo.data('producto-id');

    // NUEVA VERIFICACIÓN: Doble comprobación de seguridad
    if (esProductoHistorico(productoId)) {
        // Restaurar valor original sin guardar cambios
        const valorOriginal = parseFloat($campo.data('original-value')) || 0;
        $campo.val(valorOriginal.toFixed($campo.hasClass('input-cantidad') ? 2 : 2));
        $campo.prop('readonly', true);
        ControlaMensajeWarning("No se puede modificar un producto con estado histórico");
        return;
    }

    const valorOriginal = parseFloat($campo.data('original-value')) || 0;
    const valorActual = parseFloat($campo.val().replace(/,/g, '')) || 0;

    // Formatear el valor según el tipo de campo
    const decimales = $campo.hasClass('input-cantidad') ? 0 : 5;
    $campo.val(valorActual.toFixed(decimales));

    // Volver a readonly
    $campo.prop('readonly', true);

    // Verificar si cambió el valor
    if (Math.abs(valorOriginal - valorActual) > 0.001) {
        marcarCampoModificadoProducto($campo);
        actualizarDatosProductoCombo($campo);
    }
}

/**
 * Marca un campo como modificado con indicador visual
 * @param {jQuery} $campo - El campo jQuery que se marcará
 */
function marcarCampoModificadoProducto($campo) {
    $campo.addClass('campo-modificado');

    // Agregar indicador visual si no existe
    const $container = $campo.closest('.input-container');
    if ($container.find('.indicador-cambio').length === 0) {
        $container.append('<div class="indicador-cambio"></div>');
    }
}

/**
 * Actualiza los datos internos del producto cuando se modifica cantidad o descuento
 * @param {jQuery} $campo - El campo jQuery modificado
 */
function actualizarDatosProductoCombo($campo) {
    const productoId = $campo.data('producto-id');
    const esCantidad = $campo.hasClass('input-cantidad');
    const nuevoValor = parseFloat($campo.val().replace(/,/g, '')) || 0;

    // Actualizar data-original-value para futuras comparaciones
    $campo.data('original-value', nuevoValor);

    // Actualizar datos internos o en el servidor según sea necesario
    console.log(`Producto ${productoId}: ${esCantidad ? 'cantidad' : 'descuento'} actualizado a ${nuevoValor}`);

    // Aquí se podría implementar el envío al servidor si fuera necesario
    // Por ejemplo, mediante una llamada AJAX a una función que guarde los cambios
}

/**
 * Activa el siguiente campo editable para continuar la edición
 * @param {HTMLElement} campoActual - El campo actual que pierde el foco
 */
function activarSiguienteCampoProducto(campoActual) {
    const $campoActual = $(campoActual);
    const $fila = $campoActual.closest('tr');
    const esCantidad = $campoActual.hasClass('input-cantidad');

    // Si es cantidad, activar descuento en la misma fila
    if (esCantidad) {
        const $siguiente = $fila.find('.input-descuento');
        if ($siguiente.length) {
            // NUEVA VERIFICACIÓN: No activar si es producto histórico
            const productoId = $siguiente.data('producto-id');
            if (esProductoHistorico(productoId)) {
                $campoActual.prop('readonly', true).addClass('campo-readonly');
                return;
            }

            $siguiente
                .prop('readonly', false)
                .removeClass('campo-readonly')
                .trigger("focus")
                .trigger("select");
            return;
        }
    }

    // Si es descuento o no hay siguiente en esta fila, ir a la siguiente fila
    const $siguienteFila = $fila.next('tr');
    if ($siguienteFila.length) {
        const $siguienteCampo = $siguienteFila.find('.input-cantidad');
        if ($siguienteCampo.length) {
            // NUEVA VERIFICACIÓN: No activar si es producto histórico
            const productoId = $siguienteCampo.data('producto-id');
            if (esProductoHistorico(productoId)) {
                $campoActual.prop('readonly', true).addClass('campo-readonly');
                return;
            }

            $siguienteCampo
                .prop('readonly', false)
                .removeClass('campo-readonly')
                .trigger("focus")
                .trigger("select");
            return;
        }
    }

    // Si no hay siguiente campo, solo cerrar la edición actual
    $campoActual.prop('readonly', true).addClass('campo-readonly');
}

/**
 * Configura los eventos para la selección de filas en la tabla de productos
 */
function configurarSeleccionProductos() {
    // Aplicar estilo de cursor a todas las filas de la tabla
    $("#tbGridProductos tbody tr").css("cursor", "pointer");

    // Remover eventos previos
    $(document).off("click", "#tbGridProductos tbody tr");

    // Configurar evento de click para seleccionar filas
    $(document).on("click", "#tbGridProductos tbody tr", function (e) {
        // NUEVO: Verificación directa y verificación de campo en preparación
        if (hayEdicionActiva() || $(e.target).is('.input-cantidad, .input-descuento') ||
            $(e.target).closest('.input-container').length > 0) {
            return false; // Evitar selección si hay edición activa o el clic fue en un campo editable
        }

        if (!$(e.target).is("button, a, .btn, i")) {
            var $this = $(this);
            var wasSelected = $this.hasClass("selected-row");

            // Eliminar la selecci贸n de todas las filas
            $("#tbGridProductos tbody tr").removeClass("selected-row");

            // Seleccionar esta fila solo si no estaba seleccionada anteriormente
            if (!wasSelected) {
                $this.addClass("selected-row");

                // Obtener el ID del producto seleccionado
                var productoId = $this.find("td:first").text().trim();
                var comboId = $this.data("combo-id");//$("#cmb_id").val();

                if ((productoId && comboId) || (productoId && modoNuevoCombo)) {
                    cargarProductosSustitutos(comboId, productoId);
                }
            }
        }
    });
}

/**
 * Obtiene los IDs de productos que ya están en el grid
 * @returns {Array} Array con IDs de productos existentes
 */
function obtenerProductosExistentesIds() {
    var productosIds = [];

    // Recorrer todas las filas del grid de productos que no sean la fila "No hay productos"
    $("#tbGridProductos tbody tr").each(function () {
        // Verificar que no sea la fila de "No hay productos"
        if (!$(this).find("td[colspan]").length) {
            var productoId = $(this).data("producto-id") || $(this).find("td:first").text().trim();
            if (productoId) {
                productosIds.push(productoId.toString());
            }
        }
    });

    console.log("Productos existentes:", productosIds);
    return productosIds;
}

/**
 * Obtiene los IDs de productos sustitutos que ya están en el grid
 * @returns {Array} Array con IDs de productos sustitutos existentes
 */
function obtenerSustitutosExistentesIds() {
    var sustitutosIds = [];

    // Obtener ID del producto seleccionado
    var productoId = $("#tbGridProductos tbody tr.selected-row").data("producto-id") ||
        $("#tbGridProductos tbody tr.selected-row td:first").text().trim();

    // Si tenemos un productoId y hay sustitutos en el mapa, usar esos
    if (productoId && productosSustitutosMap && productosSustitutosMap[productoId]) {
        sustitutosIds = productosSustitutosMap[productoId].map(s => s.p_id.toString());
    } else {
        // Caso alternativo: buscar directamente en la tabla (por si no está actualizado el mapa)
        $("#tbGridSustitutos tbody tr").each(function () {
            if (!$(this).find("td[colspan]").length) {
                var sustitutoId = $(this).data("producto-id") || $(this).find("td:first").text().trim();
                if (sustitutoId) {
                    sustitutosIds.push(sustitutoId.toString());
                }
            }
        });
    }

    console.log("Sustitutos existentes para producto " + productoId + ":", sustitutosIds);
    return sustitutosIds;
}

/**
 * Guarda el mapa de sustitutos en sesión
 */
function guardarSustitutosEnSesion() {
    try {
        sessionStorage.setItem('productosSustitutosMap', JSON.stringify(productosSustitutosMap));
        console.log("Mapa de sustitutos guardado en sesión:", Object.keys(productosSustitutosMap).length, "productos");
    } catch (e) {
        console.error("Error al guardar sustitutos en sesión:", e);
    }
}

/**
 * Carga los productos sustitutos asociados a un producto dentro de un combo
 * @param {string} comboId - ID del combo
 * @param {string} productoId - ID del producto
 * @returns {boolean} - false si se cancela la operación por edición activa
 */
function cargarProductosSustitutos(comboId, productoId) {
    // Verificación inmediata y estricta de edición activa
    if (hayEdicionActiva()) {
        console.log("⚠️ Edición activa detectada, no se cargarán sustitutos");
        return false;
    }

    // Determinar qué URL usar según el modo
    var url = modoNuevoCombo && typeof retornarProductosSustitutosUrl !== 'undefined'
        ? retornarProductosSustitutosUrl
        : obtenerProductosSustitutosUrl;

    if (typeof url === 'undefined') {
        console.error("URL para obtener productos sustitutos no definida");
        return false;
    }

    AbrirWaiting("Cargando productos sustitutos...");

    // Obtener descripción del producto seleccionado para el mensaje
    var productoDesc = $("#tbGridProductos tbody tr.selected-row td:nth-child(2)").text().trim();

    // Configurar datos según la URL que estamos usando
    var datos = modoNuevoCombo ? { p_id: productoId } : { comboId: comboId, productoId: productoId };

    $.ajax({
        url: url,
        type: "POST",
        data: datos,
        success: function (response) {
            CerrarWaiting();

            if (modoNuevoCombo) {
                // Para modo nuevo combo, procesamos respuesta JSON de RetornarProductosSustitutos
                if (response && response.ok) {
                    // Actualizar el mapa de sustitutos con los datos recibidos
                    if (!productosSustitutosMap[productoId] && response.sustitutos && response.sustitutos.length > 0) {
                        productosSustitutosMap[productoId] = response.sustitutos;
                        // Guardar en sesión si se agregaron sustitutos
                        guardarSustitutosEnSesion();
                    }

                    // Actualizar grid de sustitutos con los datos recibidos
                    actualizarGridSustitutos(productoId);

                    //// Mostrar mensaje si no hay sustitutos
                    //if (!response.sustitutos || response.sustitutos.length === 0) {
                    //    ControlaMensajeWarning("El producto \"" + productoDesc + "\" (ID: " + productoId + ") no tiene sustitutos asociados.");
                    //}
                } else {
                    ControlaMensajeWarning(response.mensaje || "No se encontraron sustitutos para este producto");
                }
            } else {
                // Para modo visualización, procesamos respuesta HTML de ObtenerProductosSustitutos
                // Actualizar el contenido del grid de sustitutos
                $("#colComboSustituto").html(`<div id="divComboSustituto">${response}</div>`).show();

                if ($("#tbGridSustitutos tbody tr").length === 0) {
                    // Si no hay filas después de cargar, mostrar mensaje "No hay sustitutos"
                    $("#tbGridSustitutos tbody").html(`
                        <tr>
                            <td colspan="${modoNuevoCombo ? 5 : 4}" class="text-center text-muted py-2">
                                <i class="bx bx-info-circle me-1"></i>No hay sustitutos disponibles
                            </td>
                        </tr>
                    `);
                } else {
                    // Ocultar columna de acción si no estamos en modo nuevo combo
                    if (!modoNuevoCombo) {
                        $("#tbGridSustitutos th:last-child, #tbGridSustitutos td:last-child").hide();
                    }
                }

                // Verificar si hay sustitutos después de cargar el HTML
                setTimeout(function () {
                    var tieneFilasConDatos = $("#tbGridSustitutos tbody tr").length > 0 &&
                        !$("#tbGridSustitutos tbody tr td").text().includes("No hay sustitutos disponibles");

                    //if (!tieneFilasConDatos) {
                    //    ControlaMensajeWarning("El producto \"" + productoDesc + "\" (ID: " + productoId + ") no tiene sustitutos asociados.");
                    //}
                }, 100);
            }
        },
        error: function (xhr, status, error) {
            CerrarWaiting();
            console.error("Error al cargar productos sustitutos:", error);
            ControlaMensajeError("Error al cargar productos sustitutos: " + error);
        }
    });

    return true;
}

/**
 * Actualiza el grid de sustitutos para un producto específico
 * @param {string} productoId - ID del producto para el que se mostrarán los sustitutos
 */
function actualizarGridSustitutos(productoId) {
    // Verificar que tengamos un productoId válido y que existan sustitutos para ese producto
    if (!productoId || !productosSustitutosMap[productoId] || productosSustitutosMap[productoId].length === 0) {
        console.log("⚠️ No hay sustitutos para mostrar para el producto:", productoId);

        // Mostrar mensaje de "No hay sustitutos disponibles"
        var htmlEmpty = `
        <tr>
            <td colspan="${modoNuevoCombo ? 5 : 4}" class="text-center text-muted py-2">
                <i class="bx bx-info-circle me-1"></i>No hay sustitutos disponibles
            </td>
        </tr>`;

        $("#tbGridSustitutos tbody").html(htmlEmpty);
        return;
    }

    // Obtener los sustitutos del mapa
    var sustitutos = productosSustitutosMap[productoId];

    // Limpiar el tbody actual
    var $tbody = $("#tbGridSustitutos tbody").empty();

    // Agregar cada sustituto como fila en la tabla
    $.each(sustitutos, function (i, sustituto) {
        var fila = `
        <tr data-producto-id="${sustituto.p_id}" data-combo-id="${sustituto.cmb_id || ''}">
            <td class="text-center">
                ${sustituto.p_id}
            </td>
            <td>
                ${sustituto.p_desc}
            </td>
            <td class="text-end">
                ${parseFloat(sustituto.p_pcosto).toFixed(3)}
            </td>          
            ${modoNuevoCombo ? `
            <td class="text-center">
                <button type="button" class="btn btn-sm btn-outline-danger btn-eliminar-sustituto" 
                        title="Eliminar sustituto" data-producto-id="${sustituto.p_id}">
                    <i class="bx bx-trash"></i>
                </button>
            </td>
            ` : ''}
        </tr>`;

        $tbody.append(fila);
    });

    // Configurar eventos para eliminar sustitutos si estamos en modo edición
    if (modoNuevoCombo) {
        configurarEventosEliminacionSustitutos();
    }

    console.log("✅ Grid de sustitutos actualizado para producto " + productoId + ":", sustitutos.length, "sustitutos");
}

/**
 * ✅ MEJORADA: Carga productos con aplicación de visibilidad según tipo
 */
function cargarProductosCombo(comboId, version) {
    if (typeof obtenerProductosDeComboUrl === 'undefined') {
        console.error("URL para obtener productos no definida");
        return;
    }

    AbrirWaiting("Cargando productos...");

    $.ajax({
        url: obtenerProductosDeComboUrl,
        type: "POST",
        data: { id: comboId },
        success: function (html) {
            CerrarWaiting();
            if (!esCargaComboVigente(comboId, version)) return;

            // Actualizar el contenido del grid de productos
            $("#colComboProducto").html(`<div id="divComboProducto">${html}</div>`).show();

            const colspan = modoNuevoCombo ? 6 : 5;

            if ($("#tbGridProductos tbody tr").length === 0) {
                // Si no hay filas después de cargar, mostrar mensaje "No hay productos"
                $("#tbGridProductos tbody").html(`
                    <tr>
                        <td colspan="${colspan}" class="text-center text-muted py-2">
                            <i class="bx bx-info-circle me-1"></i>No hay productos disponibles
                        </td>
                    </tr>
                `);
            } else {
                // Si estamos viendo un combo existente (no en modo nuevo combo)
                if (!modoNuevoCombo) {
                    // Asegurar que los campos estén en modo readonly
                    $("#tbGridProductos .input-cantidad, #tbGridProductos .input-descuento").prop("readonly", true);

                    // ✅ CRÍTICO: Aplicar visibilidad según tipo
                    const tipoCombo = $("#cmb_tipo").val() || 'C';
                    actualizarVisibilidadSegunTipo(tipoCombo);

                    // Si la columna de acción existe, ocultarla
                    if ($("#tbGridProductos th").length > 5) {
                        $("#tbGridProductos th:last-child, #tbGridProductos td:last-child").hide();
                    }
                } else {
                    // En modo edición, inicializar los campos editables
                    inicializarCamposEditablesProductos();

                    // ✅ NUEVO: Aplicar visibilidad también en modo edición
                    const tipoCombo = $("#cmb_tipo").val() || 'C';
                    actualizarVisibilidadSegunTipo(tipoCombo);
                }
            }

            // Configurar eventos de selección para los productos
            configurarSeleccionProductos();

            // Si hay productos, seleccionar el primero para mostrar sus sustitutos
            var $primerProducto = $("#tbGridProductos tbody tr:first");
            if ($primerProducto.length && !$primerProducto.find("td[colspan]").length) {
                $primerProducto.trigger("click");
            }
        },
        error: function (xhr, status, error) {
            CerrarWaiting();
            console.error("Error al cargar productos:", error);
            ControlaMensajeError("Error al cargar productos: " + error);
        }
    });
}

/**
 * Confirma la creación o modificación de un combo/promoción
 */
function confirmarCombo() {
    // Verificar que estamos en modo edición (nuevo o modificación)
    if (!modoNuevoCombo && !modoModificacionCombo) {
        ControlaMensajeWarning("No hay operación activa para confirmar");
        return;
    }

    // Validar URL de confirmación
    if (typeof confirmacionComboUrl === 'undefined') {
        console.error("URL de confirmación no definida");
        ControlaMensajeError("Error de configuración: URL de confirmación no definida");
        return;
    }

    // 1. Recopilar datos del combo
    var datos = recopilarDatosCombo();
    if (!datos) return;

    // ✅ NUEVO: Validar importe único para tipos Q y D
    if (datos.cmb_tipo === 'Q' || datos.cmb_tipo === 'D') {
        var importeUnico = obtenerYValidarImporteUnico();
        if (importeUnico === null) {
            // La función ya muestra el mensaje de error
            return;
        }
    }

    // 2. Recopilar canales seleccionados para altas y modificaciones inactivas.
    var canales = [];
    if (modoNuevoCombo || edicionEstructuraPermitida) {
        canales = recopilarCanalesSeleccionados();
        if (!canales || canales.length === 0) {
            ControlaMensajeWarning("Debe seleccionar al menos un canal para el combo/promoción");
            return;
        }
    } else {
        // Para modificación, obtener los canales existentes
        $("#tbGridCanales tbody tr").each(function () {
            var $fila = $(this);
            canales.push({
                adm_id: $fila.data("adm-id"),
                lp_id: $fila.data("lp-id"),
                canal: $fila.find("td:eq(0)").text().trim(),
                incluida: 'S'
            });
        });
    }

    // 3. Recopilar productos del grid
    var productos = recopilarProductosCombo();
    if (!productos || productos.length === 0) {
        ControlaMensajeWarning("Debe agregar al menos un producto al combo/promoción");
        return;
    }

    // 4. Preparar request usando ConfirmacionRequestDto
    var request = {
        Datos: datos,
        Canales: canales,
        Productos: productos
    };

    // 5. Confirmar con el usuario
    var tipoDesc = datos.cmb_tipo === 'C' ? 'combo' : 'promoción';
    var accionDesc = modoNuevoCombo ? "guardar" : "modificar";

    AbrirMensaje(
        (modoNuevoCombo ? "CONFIRMAR " : "MODIFICAR ") + tipoDesc.toUpperCase(),
        `¿Está seguro que desea ${accionDesc} este ${tipoDesc}?<br><br>` +
        `<strong>Descripción:</strong> ${datos.cmb_desc}<br>` +
        `<strong>Productos:</strong> ${productos.length}<br>` +
        `<strong>Canales:</strong> ${canales.length}`,
        function (resp) {
            if (resp === "SI") {
                enviarConfirmacionCombo(request, tipoDesc);
            }
            $("#msjModal").modal("hide");
            return true;
        },
        true,
        ["Confirmar", "Cancelar"],
        "info!",
        null
    );
}

/**
 * Recopila los datos principales del combo desde el formulario
 * @returns {Object|null} Objeto con los datos del combo o null si hay error
 */
function recopilarDatosCombo() {
    // Validar descripción
    var descripcion = $("#cmb_desc").val().trim();
    if (!descripcion) {
        ControlaMensajeWarning("La descripción del combo/promoción es obligatoria");
        $("#cmb_desc").trigger("focus");
        return null;
    }

    // Validar fechas
    var fechaDesde = $("#cmb_desde").val();
    var fechaHasta = $("#cmb_hasta").val();

    if (!fechaDesde || !fechaHasta) {
        ControlaMensajeWarning("Las fechas de vigencia son obligatorias");
        return null;
    }

    // Validar que fecha desde sea menor o igual a fecha hasta
    if (new Date(fechaDesde) > new Date(fechaHasta)) {
        ControlaMensajeWarning("La fecha desde no puede ser mayor a la fecha hasta");
        $("#cmb_desde").trigger("focus");
        return null;
    }

    // Construir objeto de datos
    return {
        cmb_id: $("#cmb_id").val() || '',
        cmb_desc: descripcion,
        cmb_tipo: $("#cmb_tipo").val() || 'C',
        cmb_estado: $("#cmb_estado").val() || 'N',
        cmb_desde: fechaDesde,
        cmb_hasta: fechaHasta,
        pasa_activar: false,
        pasa_historico: false
    };
}



/**
 * ✅ MODIFICADO: Recopila los productos desde el grid con manejo de importe único para Q/D
 * @returns {Array} Array con los productos del combo
 */
function recopilarProductosCombo() {
    var productos = [];

    // ✅ NUEVO: Determinar tipo de combo y obtener importe único si aplica
    var tipoCombo = $("#cmb_tipo").val();
    var esImporteUnico = (tipoCombo === 'Q' || tipoCombo === 'D');
    var importeUnico = 0;

    if (esImporteUnico) {
        // Obtener el importe único (ya validado previamente en confirmarCombo)
        importeUnico = obtenerYValidarImporteUnico();
        if (importeUnico === null) {
            importeUnico = 0; // Fallback por seguridad
        }
    }

    // Variable para controlar si es el primer producto
    var esPrimerProducto = true;

    // Recorrer todas las filas del grid de productos
    $("#tbGridProductos tbody tr").each(function () {
        // Verificar que no sea la fila de "No hay productos"
        if (!$(this).find("td[colspan]").length) {
            var $fila = $(this);

            // Obtener valores de los inputs
            var cantidadIngresada = parseFloat($fila.find(".input-cantidad").val().replace(/,/g, '')) || 0;
            var descuento = 0;
            var upId = String($fila.data("up-id") || '');

            if (!Number.isInteger(cantidadIngresada)) {
                ControlaMensajeWarning(`El producto ${$fila.data("producto-id")} requiere una cantidad entera`);
                productos = null;
                return false;
            }
            var cantidad = Math.trunc(cantidadIngresada);

            // ✅ CRÍTICO: Lógica diferenciada según tipo de combo
            if (esImporteUnico) {
                // Para tipos Q/D: solo el primer producto lleva el importe único
                if (esPrimerProducto) {
                    descuento = importeUnico;
                    esPrimerProducto = false;
                    console.log(`✅ Importe único asignado al primer producto: ${descuento}`);
                } else {
                    descuento = 0;
                }
            } else {
                // Para tipos P/C: usar el descuento individual de cada producto
                descuento = parseFloat($fila.find(".input-descuento").val().replace(/,/g, '')) || 0;
            }

            var producto = {
                cmb_id: $fila.data("combo-id") || '',
                p_id: $fila.data("producto-id") || $fila.find("td:eq(0)").text().trim(),
                p_desc: $fila.find("td:eq(1)").text().trim(),
                p_pcosto: parseFloat($fila.find("td:eq(2)").text().replace(/[^\d.-]/g, '')) || 0,
                cantidad: cantidad,
                dto_porc: esImporteUnico ? 0 : descuento,
                dto_imp: esImporteUnico ? descuento : 0,
                up_id: upId,
                activo: 'A'
            };

            productos.push(producto);
        }
    });

    // ✅ NUEVO: Log informativo para debugging
    if (productos && esImporteUnico) {
        console.log(`📦 Productos recopilados para tipo ${tipoCombo}:`);
        console.log(`   - Total productos: ${productos.length}`);
        console.log(`   - Importe único en primer producto: ${importeUnico}`);
    }

    return productos;
}

/**
 * Recopila los canales seleccionados desde la grilla
 * @returns {Array} Array con los canales seleccionados
 */
function recopilarCanalesSeleccionados() {
    var canales = [];

    // Recorrer todos los checkboxes de canales marcados
    $(".canal-checkbox:checked").each(function () {
        var $fila = $(this).closest("tr");
        var canal = {
            adm_id: $fila.data("adm-id"),
            //adm_id: $fila.find("td:eq(1)").text().trim(),
            //adm_nombre: $fila.find("td:eq(2)").text().trim(),
            lp_id: $fila.data("lp-id"),
            //lp_desc: $fila.find("td:eq(4)").text().trim(),
            canal: $(this).val(),
            incluida: 'S'
        };
        canales.push(canal);
    });

    return canales;
}

/**
 * Envía la confirmación del combo al servidor
 * @param {Object} request - Request con todos los datos del combo
 * @param {string} tipoDesc - Descripción del tipo (combo o promoción)
 */
function enviarConfirmacionCombo(request, tipoDesc) {
    var accionDesc = modoNuevoCombo ? "Guardando" : "Modificando";
    AbrirWaiting(`${accionDesc} ${tipoDesc}...`);

    $.ajax({
        url: confirmacionComboUrl,
        type: "POST",
        contentType: "application/json",
        data: JSON.stringify(request),
        success: function (response) {
            CerrarWaiting();

            if (response && response.ok && !response.error) {
                // Éxito
                var mensajeExito = modoNuevoCombo
                    ? `${tipoDesc.charAt(0).toUpperCase() + tipoDesc.slice(1)} guardado correctamente`
                    : `${tipoDesc.charAt(0).toUpperCase() + tipoDesc.slice(1)} modificado correctamente`;

                ControlaMensajeSuccess(response.msg || mensajeExito);

                // Limpiar datos temporales
                productosSustitutosMap = {};
                sessionStorage.removeItem('productosSustitutosMap');

                // Resguardar el ID del combo si existe en la respuesta
                if (response.id) {
                    comboIdGuardado = response.id;
                    console.log("✅ ID del combo guardado:", comboIdGuardado);
                }

                var idResultado = response.id || request.Datos.cmb_id || comboSeleccionadoId;


                // Cancelar operación y volver al estado inicial
                // ✅ SOLUCIÓN: Diferenciar flujo según el modo
                setTimeout(function () {
                    activarGrilla("tbGridPromoCombo");
                    if (modoNuevoCombo) {
                        // Modo NUEVO: cancelar y recargar grid
                        cancelarOperacion(null, false);

                        // Esperar a que se complete la recarga y luego seleccionar
                        refrescarYSeleccionarCombo(comboIdGuardado);
                    } else {
                        // Modo MODIFICACIÓN: actualizar la fila confirmada sin
                        // volver a consultar toda la página.
                        actualizarFilaComboEnGrid(idResultado, request.Datos);
                        cancelarOperacion(null, false);

                        // Seleccionar inmediatamente (el registro ya está en el grid)
                        seleccionarYPosicionarCombo(idResultado);
                    }
                }, 1500);
            } else {
                // Error o advertencia
                var mensaje = response.msg || `Error al ${modoNuevoCombo ? 'guardar' : 'modificar'} ${tipoDesc}`;
                if (response.warn) {
                    ControlaMensajeWarning(mensaje);
                } else {
                    ControlaMensajeError(mensaje);
                }
            }
        },
        error: function (xhr, status, error) {
            CerrarWaiting();
            console.error(`Error al ${modoNuevoCombo ? 'guardar' : 'modificar'} ${tipoDesc}:`, error);

            // Intentar extraer mensaje detallado del error
            var mensajeError = `Error al ${modoNuevoCombo ? 'guardar' : 'modificar'} el ${tipoDesc}`;
            if (xhr.responseJSON && xhr.responseJSON.mensaje) {
                mensajeError = xhr.responseJSON.mensaje;
            } else if (xhr.responseText) {
                try {
                    var errorObj = JSON.parse(xhr.responseText);
                    mensajeError = errorObj.mensaje || errorObj.msg || mensajeError;
                } catch (e) {
                    // Si no se puede parsear, usar mensaje por defecto
                }
            }

            ControlaMensajeError(mensajeError);
        }
    });
}

/**
 * Actualiza los datos visibles de una fila después de una modificación exitosa.
 * La fila se localiza por ID, nunca por posición, para tolerar orden y paginación.
 */
function actualizarFilaComboEnGrid(comboId, datos) {
    if (!comboId || !datos) return false;

    var $fila = $("#tbGridPromoCombo tbody tr").filter(function () {
        return String($(this).data("combo-id") || $(this).attr("data-combo-id") || '') === String(comboId);
    }).first();

    if ($fila.length === 0) {
        console.warn("No se encontró la fila modificada en la página actual:", comboId);
        return false;
    }

    var formatearFechaGrilla = function (fecha) {
        var fechaIso = formatearFecha(fecha);
        if (!fechaIso) return '';
        var partes = fechaIso.split('-');
        return partes.length === 3 ? `${partes[2]}/${partes[1]}/${partes[0]}` : fechaIso;
    };

    $fila.attr("data-descripcion", datos.cmb_desc).data("descripcion", datos.cmb_desc);
    $fila.find("td:eq(1)").text(datos.cmb_desc).attr("title", datos.cmb_desc);
    $fila.find("td:eq(2)").text(formatearFechaGrilla(datos.cmb_desde));
    $fila.find("td:eq(5)").text(formatearFechaGrilla(datos.cmb_hasta));

    return true;
}

/**
 * Refresca el grid y selecciona el combo recién guardado
 * @param {string} comboId - ID del combo a seleccionar
 */
function refrescarYSeleccionarCombo(comboId) {
    if (!comboId) {
        console.warn("⚠️ No hay ID de combo para seleccionar");
        refrescarGridPromoCombo();
        return;
    }

    // Los nuevos registros nacen Sin Activar; se fuerza ese filtro para que
    // el alta recién creada siempre pueda localizarse y posicionarse.
    $("#chkEstado").prop("checked", true);
    $("#Estado").prop("disabled", false).val('N');

    // Usar el success callback de buscarCombos para seleccionar después de cargar
    var pagActual = parseInt(window.pagina, 10);
    if (!Number.isFinite(pagActual) || pagActual < 1) pagActual = 1;

    // Mostrar mensaje de espera
    AbrirWaiting("Actualizando listado...");

    var filtros = {
        Tipo: $("#chkTipo").prop("checked") ? $("#Tipo").val() : null,
        Estado: $("#chkEstado").prop("checked") ? $("#Estado").val() : null,
        Pagina: pagActual
    };

    $.ajax({
        url: presentarPromosYCombosUrl,
        type: "POST",
        data: filtros,
        success: function (html) {
            CerrarWaiting();
            realizaAlgunaBusqueda = true;

            // Actualizar el contenedor
            $("#divFiltro").collapse("hide");
            $("#divDetalle").collapse("show");
            $("#divDetalle").html(html);

            // Configurar eventos
            configurarEventosPaginacion();
            configurarEventosSeleccion();

            // Actualizar paginación
            PostGen({}, buscarMetadataURL, function (obj) {
                if (!obj.error) {
                    totalRegs = obj.metadata.totalCount;
                    pags = obj.metadata.totalPages;
                    pagRegs = obj.metadata.pageSize;
                    $("#pagEstado").val(true).trigger("change");
                }

                // ✅ CLAVE: Seleccionar después de que todo esté cargado
                setTimeout(function () {
                    seleccionarYPosicionarCombo(comboId);
                }, 200);
            });
        },
        error: function (xhr, status, error) {
            CerrarWaiting();
            console.error("Error al refrescar grid:", error);
            ControlaMensajeError("Error al actualizar el listado: " + error);
        }
    });
}

/**
 * Selecciona y posiciona un combo en el grid por su ID
 * @param {string} comboId - ID del combo a seleccionar
 */
function seleccionarYPosicionarCombo(comboId) {
    if (!comboId) {
        console.warn("⚠️ No hay ID de combo para seleccionar");
        return;
    }

    console.log("🔍 Buscando combo con ID:", comboId);

    // Buscar la fila que contiene el combo guardado
    const $fila = $("#tbGridPromoCombo tbody tr").filter(function () {
        // Buscar por data-combo-id o por el texto de la primera columna
        var filaId = $(this).data("combo-id") ||
            $(this).attr("data-combo-id") ||
            $(this).find("td:first").text().trim();
        return filaId === comboId;
    }).first();

    if ($fila.length > 0) {
        console.log("✅ Combo encontrado, seleccionando...");

        // Remover selección previa
        $("#tbGridPromoCombo tbody tr").removeClass("selectedEdit-row selected-row");

        // Marcar la fila como seleccionada
        $fila.addClass("selected-row");

        // Actualizar contador
        actualizarContadorSeleccionados();

        // Posicionar la fila en la parte superior visible del contenedor
        requestAnimationFrame(function () {
            if (typeof posicionarRegOnTopMejorado === 'function') {
                posicionarRegOnTopMejorado($fila, ".table-wrapper");
            } else {
                // Fallback: scroll básico si la función no existe
                $fila[0]?.scrollIntoView({ behavior: 'smooth', block: 'center' });
            }

            console.log("✅ Combo seleccionado y posicionado correctamente");
        });
    } else {
        console.warn("⚠️ No se encontró el combo con ID:", comboId);
        console.log("IDs disponibles en el grid:",
            $("#tbGridPromoCombo tbody tr").map(function () {
                return $(this).data("combo-id") || $(this).find("td:first").text().trim();
            }).get()
        );
    }
}

/**
 * Activa un combo o promoción existente
 * @param {string} tipoDesc - Descripción del tipo de entidad (combo o promoción)
 */
function cambiarEstadoComboExistente(tipoDesc, nuevoEstado) {
    // Verificar URL de confirmación
    if (typeof confirmacionComboUrl === 'undefined') {
        console.error("URL de confirmación no definida");
        ControlaMensajeError("Error de configuración: URL de confirmación no definida");
        return;
    }

    // 1. Mostrar indicador de progreso
    var estadoAnterior = $("#cmb_estado").val();
    var accionGerundio = nuevoEstado === 'A' ? 'Activando' : 'Pasando a Histórico';
    AbrirWaiting(`${accionGerundio} ${tipoDesc}...`);

    // 2. Recopilar datos existentes
    var datos = {
        cmb_id: $("#cmb_id").val(),
        cmb_desc: $("#cmb_desc").val().trim(),
        cmb_tipo: $("#cmb_tipo").val() || 'C',
        cmb_estado: nuevoEstado,
        cmb_desde: $("#cmb_desde").val(),
        cmb_hasta: $("#cmb_hasta").val(),
        pasa_activar: nuevoEstado === 'A',
        pasa_historico: nuevoEstado === 'H'
    };

    // 3. Recopilar canales (todos los visibles en la tabla, ya que estamos en visualización)
    var canales = [];
    $("#tbGridCanales tbody tr").each(function () {
        // Obtener datos de columnas relevantes (ajustar índices según estructura real)
        var fila = $(this);
        if (!canalEstaIncluido(fila)) return;
        var canal = {
            adm_id: fila.data("adm-id"),
            //adm_id: $fila.find("td:eq(1)").text().trim(),
            //adm_nombre: $fila.find("td:eq(2)").text().trim(),
            lp_id: fila.data("lp-id"),
            //lp_desc: $fila.find("td:eq(4)").text().trim(),
            canal: fila.find("td:eq(0)").text().trim(),
            incluida: 'S'
        };
        canales.push(canal);
    });

    // 4. Recopilar productos con los valores mostrados actualmente
    var productos = [];
    $("#tbGridProductos tbody tr").each(function () {
        // Verificar que no sea la fila de "No hay productos"
        if (!$(this).find("td[colspan]").length) {
            var fila = $(this);
            var producto = {
                cmb_id: datos.cmb_id,
                p_id: fila.find("td:eq(0)").text().trim(),
                p_desc: fila.find("td:eq(1)").text().trim(),
                p_pcosto: parseFloat(fila.find("td:eq(2)").text().replace(/[^\d.-]/g, '')) || 0,
                cantidad: Math.trunc(parseFloat(fila.find(".input-cantidad").val().replace(/,/g, '')) || 1),
                dto_porc: parseFloat(fila.find(".input-descuento").val().replace(/,/g, '')) || 0,
                dto_imp: parseFloat(fila.data("dto-imp")) || 0,
                up_id: String(fila.data("up-id") || ''),
                activo: 'A'
            };
            productos.push(producto);
        }
    });

    // 5. Preparar request completo
    var request = {
        Datos: datos,
        Canales: canales,
        Productos: productos
    };

    // 6. Enviar al servidor para activar
    $.ajax({
        url: confirmacionComboUrl,
        type: "POST",
        contentType: "application/json",
        data: JSON.stringify(request),
        success: function (response) {
            CerrarWaiting();

            if (response && response.ok && !response.error) {
                // Éxito
                var accionCompletada = nuevoEstado === 'A' ? 'activado' : 'pasado a Histórico';
                ControlaMensajeSuccess(response.msg || `${tipoDesc.charAt(0).toUpperCase() + tipoDesc.slice(1)} ${accionCompletada} correctamente`);
                $("#cmb_estado").val(nuevoEstado);
                configurarPresentacionEstado(nuevoEstado);

                // Refrescar el grid con los filtros vigentes
                refrescarGridPromoCombo();
            } else {
                // Error o advertencia
                var mensaje = response.msg || `Error al activar ${tipoDesc}`;
                if (response.warn) {
                    ControlaMensajeWarning(mensaje);
                    $("#cmb_estado").val(estadoAnterior);
                    configurarPresentacionEstado(estadoAnterior);
                } else {
                    ControlaMensajeError(mensaje);
                    $("#cmb_estado").val(estadoAnterior);
                    configurarPresentacionEstado(estadoAnterior);
                }
            }
        },
        error: function (xhr, status, error) {
            CerrarWaiting();
            console.error(`Error al activar ${tipoDesc}:`, error);

            // Intentar extraer mensaje detallado del error
            var mensajeError = "Error al activar el " + tipoDesc;
            if (xhr.responseJSON && xhr.responseJSON.mensaje) {
                mensajeError = xhr.responseJSON.mensaje;
            } else if (xhr.responseText) {
                try {
                    var errorObj = JSON.parse(xhr.responseText);
                    mensajeError = errorObj.mensaje || errorObj.msg || mensajeError;
                } catch (e) {
                    // Si no se puede parsear, usar mensaje por defecto
                }
            }

            ControlaMensajeError(mensajeError);

            $("#cmb_estado").val(estadoAnterior);
            configurarPresentacionEstado(estadoAnterior);
        }
    });
}

/**
 * ✅ MEJORADA: Carga el dropdown de preajustes de promoción
 * @param {jQuery} $contenedor - Contenedor donde se insertará el dropdown
 * @param {Function} callback - Función a ejecutar después de cargar (opcional)
 */
function cargarDropdownPreajuste($contenedor, callback) {
    // Verificar que la URL esté definida
    if (typeof obtenerPreajustePromoUrl === 'undefined') {
        console.error("❌ URL obtenerPreajustePromoUrl no definida");
        return;
    }

    console.log("📦 Cargando dropdown de preajustes...");

    // Limpiar contenedor antes de cargar
    $contenedor.empty();

    $.ajax({
        url: obtenerPreajustePromoUrl,
        type: "POST",
        success: function (html) {
            // Insertar el HTML del dropdown en el contenedor
            $contenedor.html(html);

            console.log("✅ Dropdown de preajustes cargado correctamente");

            // ✅ NUEVO: Aplicar visibilidad según el tipo actual
            aplicarVisibilidadPreajuste();

            // Configurar evento change del dropdown
            configurarEventoPreajuste();

            // Ejecutar callback si existe
            if (typeof callback === 'function') {
                callback();
            }
        },
        error: function (xhr, status, error) {
            console.error("❌ Error al cargar dropdown de preajustes:", error);
            // No mostrar mensaje al usuario para no interrumpir el flujo
            // El usuario simplemente no verá el dropdown
        }
    });
}

/**
 * ✅ MEJORADA: Aplica la visibilidad del dropdown de preajustes según el tipo seleccionado
 */
function aplicarVisibilidadPreajuste() {
    var tipoSeleccionado = $("#cmb_tipo").val();
    actualizarVisibilidadSegunTipo(tipoSeleccionado);
}

/**
 * ✅ MEJORADO: Configura el evento change del dropdown de preajustes
 * Actualiza cantidad y descuento de TODOS los productos al cambiar preset
 */
function configurarEventoPreajuste() {
    // Usar delegación de eventos para asegurar que funcione con elementos dinámicos
    $(document).off("change", "#preset_id").on("change", "#preset_id", function () {
        var presetValue = $(this).val();
        var tipoCombo = $("#cmb_tipo").val();

        if (!presetValue || tipoCombo !== 'P') {
            console.log("ℹ️ No se seleccionó preset o tipo no es Promoción");
            return;
        }

        console.log("🔄 Preset seleccionado:", presetValue);

        // ✅ NUEVO: Parsear preset y actualizar productos existentes
        const preset = parsearPreset(presetValue);

        if (!preset.esValido) {
            console.warn("⚠️ Preset inválido, no se actualizan productos");
            return;
        }

        // ✅ NUEVO: Actualizar cantidad y descuento de TODOS los productos
        actualizarProductosConPreset(preset.cantidad, preset.descuento);
    });
}

/**
 * ✅ NUEVA FUNCIÓN: Actualiza cantidad y descuento de todos los productos en el grid
 * @param {number} cantidad - Nueva cantidad a aplicar
 * @param {number} descuento - Nuevo descuento a aplicar
 */
function actualizarProductosConPreset(cantidad, descuento) {
    var productosActualizados = 0;

    $("#tbGridProductos tbody tr").each(function () {
        // Verificar que no sea la fila de "No hay productos"
        if (!$(this).find("td[colspan]").length) {
            var $fila = $(this);

            // Actualizar campo cantidad
            var $inputCantidad = $fila.find(".input-cantidad");
            if ($inputCantidad.length > 0) {
                $inputCantidad.val(Math.trunc(cantidad));
                $inputCantidad.data('original-value', cantidad);
                productosActualizados++;
            }

            // Actualizar campo descuento
            var $inputDescuento = $fila.find(".input-descuento");
            if ($inputDescuento.length > 0) {
                $inputDescuento.val(descuento.toFixed(5));
                $inputDescuento.data('original-value', descuento);
            }
        }
    });

    if (productosActualizados > 0) {
        console.log(`✅ ${productosActualizados} productos actualizados - Cantidad: ${cantidad}, Descuento: ${descuento}%`);
        ControlaMensajeInfo(`Preset aplicado: ${productosActualizados} producto(s) actualizados`);
    } else {
        console.log("ℹ️ No hay productos para actualizar");
    }
}

/**
 * ✅ MEJORADO: Actualiza visibilidad con sincronización de header y celdas
 * @param {string} tipo - Tipo seleccionado ('P', 'C', 'Q', 'D')
 */
function actualizarVisibilidadSegunTipo(tipo) {
    var $contenedorPreajuste = $("#contenedorPreajuste");
    var $contenedorImporte = $("#contenedorImporteUnico");

    // Tipos que usan preajustes: P (Promoción)
    var usaPreajuste = tipo === 'P';

    // Tipos que usan importe único: Q (Promo x Importe), D (Combo x Importe)
    var usaImporteUnico = tipo === 'Q' || tipo === 'D';

    // Tipos que usan descuento: P (Promoción), C (Combo)
    var usaDescuento = tipo === 'P' || tipo === 'C';

    // Los tipos por importe no admiten productos sustitutos.
    $("#colComboSustituto").toggle(!usaImporteUnico);
    $("#btnAgregarSustituto").prop("disabled", usaImporteUnico || !edicionEstructuraPermitida);
    if (usaImporteUnico && typeof limpiarSustitutosTemporalesUrl !== 'undefined') {
        productosSustitutosMap = {};
        $.post(limpiarSustitutosTemporalesUrl);
    }

    // ✅ NUEVO: Controlar visibilidad del dropdown de preajustes SOLO si existe
    if ($contenedorPreajuste.length > 0) {
        if (usaPreajuste) {
            $contenedorPreajuste.removeClass('preajuste-hidden').addClass('preajuste-visible');
            console.log("✅ Dropdown de preajustes mostrado");
        } else {
            $contenedorPreajuste.removeClass('preajuste-visible').addClass('preajuste-hidden');
            console.log("🚫 Dropdown de preajustes ocultado");
        }
    }

    // ✅ NUEVO: Controlar visibilidad del input de importe único SOLO si existe
    if ($contenedorImporte.length > 0) {
        if (usaImporteUnico) {
            $contenedorImporte.removeClass('importe-hidden').addClass('importe-visible');
            console.log("✅ Input de importe único mostrado");
        } else {
            $contenedorImporte.removeClass('importe-visible').addClass('importe-hidden');
            console.log("🚫 Input de importe único ocultado");
        }
    }

    // ✅ CRÍTICO: Sincronizar visibilidad de columna Descuento (header + celdas)
    var $headerDescuento = $("#tbGridProductos thead th:nth-child(5)");
    var $celdasDescuento = $("#tbGridProductos tbody td:nth-child(5)");

    console.log(`🔍 Encontrados: ${$headerDescuento.length} headers y ${$celdasDescuento.length} celdas de descuento`);

    if ($headerDescuento.length > 0) {
        if (usaDescuento) {
            // Mostrar columna descuento
            $headerDescuento.removeClass('d-none').show();
            console.log("✅ Header Descuento mostrado");
        } else {
            // Ocultar columna descuento
            $headerDescuento.addClass('d-none').hide();
            console.log("🚫 Header Descuento ocultado");
        }
    }

    if ($celdasDescuento.length > 0) {
        if (usaDescuento) {
            // Mostrar celdas descuento
            $celdasDescuento.removeClass('d-none').show();
            console.log(`✅ ${$celdasDescuento.length} celdas de Descuento mostradas`);
        } else {
            // Ocultar celdas descuento
            $celdasDescuento.addClass('d-none').hide();
            console.log(`🚫 ${$celdasDescuento.length} celdas de Descuento ocultadas`);
        }
    }
}

/**
 * ✅ NUEVO: Obtiene cantidad y descuento del preset seleccionado o valores por defecto
 * @returns {Object} { cantidad: number, descuento: number }
 */
function obtenerValoresPreset() {
    const tipoCombo = $("#cmb_tipo").val();

    // Tipo C: valores por defecto (sin preset)
    if (tipoCombo === 'C') {
        return { cantidad: 1, descuento: 0 };
    }

    // Tipo Q/D: cantidad 1, descuento 0 fijo
    if (tipoCombo === 'Q' || tipoCombo === 'D') {
        return { cantidad: 1, descuento: 0 };
    }

    // Tipo P: parsear preset si existe
    if (tipoCombo === 'P') {
        const presetValue = $("#preset_id").val();

        if (presetValue && presetValue !== '') {
            const preset = parsearPreset(presetValue);
            return {
                cantidad: preset.cantidad,
                descuento: preset.descuento
            };
        }

        // Preset no seleccionado para promoción
        return { cantidad: 1, descuento: 0 };
    }

    // Fallback por defecto
    return { cantidad: 1, descuento: 0 };
}

/**
* ✅ NUEVO: Parsea el valor del preset seleccionado
* @param {string} presetValue - Valor en formato "P#cantidad#descuento"
* @returns {Object} { cantidad: number, descuento: number, esValido: boolean }
*/
function parsearPreset(presetValue) {
    try {
        if (!presetValue || presetValue === '') {
            return { cantidad: 1, descuento: 0, esValido: false };
        }

        const partes = presetValue.split('#');

        if (partes.length !== 3 || partes[0] !== 'P') {
            console.warn("⚠️ Formato de preset inválido:", presetValue);
            return { cantidad: 1, descuento: 0, esValido: false };
        }

        const cantidad = Math.trunc(parseFloat(partes[1]) || 1);
        const descuento = parseFloat(partes[2]) || 0;

        console.log(`✅ Preset parseado - Cantidad: ${cantidad}, Descuento: ${descuento}%`);

        return {
            cantidad: cantidad,
            descuento: descuento,
            esValido: true
        };
    } catch (error) {
        console.error("❌ Error al parsear preset:", error);
        return { cantidad: 1, descuento: 0, esValido: false };
    }
}

/**
 * ✅ MEJORADA: Obtiene y valida el importe único con feedback visual
 * @returns {number|null} Valor del importe único o null si es inválido
 */
function obtenerYValidarImporteUnico() {
    var $importeUnico = $("#importeUnico");

    // Limpiar estados previos
    $importeUnico.removeClass('is-invalid is-valid animate-error');

    // Verificar que el campo exista
    if ($importeUnico.length === 0) {
        console.warn("⚠️ Campo #importeUnico no encontrado en el DOM");
        ControlaMensajeWarning("No se encontró el campo de importe único");
        return null;
    }

    // Obtener el valor y limpiar formato
    var valorTexto = $importeUnico.val().trim();

    // Validar que no esté vacío
    if (valorTexto === '' || valorTexto === '0' || valorTexto === '0.00') {
        $importeUnico.addClass('is-invalid animate-error');
        ControlaMensajeWarning("Debe ingresar un importe único válido mayor a cero");
        $importeUnico.trigger("focus");
        return null;
    }

    // Convertir a número (eliminar comas si existen)
    var valor = parseFloat(valorTexto.replace(/,/g, ''));

    // Validar que sea un número válido
    if (isNaN(valor) || !isFinite(valor)) {
        $importeUnico.addClass('is-invalid animate-error');
        ControlaMensajeWarning("El importe único ingresado no es un número válido");
        $importeUnico.trigger("focus");
        return null;
    }

    // Validar que sea mayor a cero
    if (valor <= 0) {
        $importeUnico.addClass('is-invalid animate-error');
        ControlaMensajeWarning("El importe único debe ser mayor a cero");
        $importeUnico.trigger("focus");
        return null;
    }

    // Validar rango razonable (opcional, ajustar según necesidad)
    if (valor > 999999.99) {
        $importeUnico.addClass('is-invalid animate-error');
        ControlaMensajeWarning("El importe único no puede ser mayor a $999,999.99");
        $importeUnico.trigger("focus");
        return null;
    }

    // ✅ Todo validado correctamente
    $importeUnico.addClass('is-valid');
    console.log(`✅ Importe único validado: ${valor}`);
    return valor;
}
