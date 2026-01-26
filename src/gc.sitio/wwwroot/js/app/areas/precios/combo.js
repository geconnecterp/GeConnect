// Variable global para detectar cuando un campo va a ser editado
var campoEnPreparacionEdicion = null;
// Agregar variable global para controlar el modo de modificación
var modoModificacionCombo = false;
// me permite saber si se hace una busqueda 
var realizaAlgunaBusqueda = false;

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

    // Ocultar detalle y mostrar filtro
    $("#divDetalle").collapse("hide");
    $("#divPromoCombo").css("max-height", "500px");
    $("#divTools").hide();
    $("#divCanales").empty();
    $("#divComboProducto").empty();
    $("#divComboSustituto").empty();
    // Desactivar botón detalle
    $("#btnDetalle").prop("disabled", true);

    activarGrilla("tbGridPromoCombo");
    // Limpiar selección visual
    $("#tbGridPromoCombo tbody tr").removeClass("selectedEdit-row");

    accionesIniciales();
}

/**
 * Inicializa los eventos para los elementos del formulario
 */
function inicializarEventos() {
    $("#btnCancel").on("click", function () {
        window.location.href = homeCombo;
    });

    //boton para realicar la cancelación de toda operación que se esté realizando
    $("#btnDetalle").on("mousedown", analizaEstadoCombo);

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

    // Evento para el checkbox de estado del combo
    $(document).on("change", "#chkEstadoCombo", function () {
        var isChecked = $(this).prop("checked");
        var nuevoEstado = isChecked ? 'A' : 'N';
        var estadoAnterior = $("#cmb_estado").val();
        
        // Actualizar el valor oculto del estado
        $("#cmb_estado").val(nuevoEstado);
        
        // Actualizar el texto de la etiqueta
        $("#lblEstadoCombo").text(isChecked ? "Activo" : "No activo");
        
        // Si no estamos en modo nuevo/edición, y se está activando un combo inactivo
        if (!modoNuevoCombo && isChecked && estadoAnterior === 'N') {
            // Revertir temporalmente el cambio para que se reactive solo tras confirmación
            $(this).prop("checked", false);
            $("#cmb_estado").val(estadoAnterior);
            $("#lblEstadoCombo").text("No activo");
            
            // Obtener el tipo de promoción o combo
            var tipo = $("#cmb_tipo").val() === 'C' ? 'combo' : 'promoción';
            var descripcion = $("#cmb_desc").val().trim();
            
            // Mostrar mensaje de confirmación
            AbrirMensaje(
                "ACTIVAR " + tipo.toUpperCase(),
                `¿Está seguro que desea activar ${tipo} "${descripcion}"?`,
                function (resp) {
                    if (resp === "SI") {
                        // Aplicar el cambio de estado y activar
                        $("#chkEstadoCombo").prop("checked", true);
                        $("#cmb_estado").val('A');
                        $("#lblEstadoCombo").text("Activo");
                        activarComboExistente(tipo);
                    }
                    $("#msjModal").modal("hide");
                    return true;
                },
                true,
                ["Activar", "Cancelar"],
                "info!",
                null
            );
        }
    });

    // Evento para el botón de nuevo combo
    $("#btnAbmNuevo").on("click", function () {
        modoNuevoCombo = true;
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

        // Actualizar los contenedores con los grids vacíos
        $(".col-sm-4:has(#tbGridProductos)").show();
        $(".col-sm-4:has(#tbGridSustitutos)").show();
    });

    // Evento para el botón de modificación
    $("#btnAbmModif").on("click", function () {
        // Verificar si hay algún combo seleccionado
        var comboId = $("#tbGridPromoCombo tbody tr.selected-row").data("combo-id");
        if (!comboId) {
            ControlaMensajeWarning("Debe seleccionar un combo/promoción para modificar");
            return;
        }

        // Verificar si el combo está activo y no permitir modificación
        var estadoActivo = $("#chkEstadoCombo").prop("checked");
        if (estadoActivo) {
            ControlaMensajeWarning("No se pueden modificar combos/promociones activos");
            return;
        }

        // Activar modo modificación
        modoModificacionCombo = true;

        // Activar/desactivar botones apropiados
        ActivarBtnAC(true);
        $("#btnAbmNuevo").prop("disabled", true);
        $("#btnAbmModif").prop("disabled", true);

        // Inicializar los campos editables para cantidad y descuento
        inicializarCamposEditablesProductos();

        // Mostrar mensaje informativo
        ControlaMensajeInfo("Ahora puede modificar cantidades y descuentos. Al terminar haga clic en 'Confirmar'.");
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
        if (!modoNuevoCombo) {
            console.warn("⚠️ No está en modo nuevo combo");
            ControlaMensajeWarning("Debe estar creando un nuevo combo para agregar productos");
            return;
        }
        
        // Cargar el modal si no existe y luego mostrarlo
        if ($("#busquedaModal").length === 0) {
            console.log("📦 Cargando modal de búsqueda avanzada...");
            cargarModalBusquedaAvanzada(function () {
                // Configurar el destino como "combos" y definir el callback
                if (typeof configurarDestinoBusquedaProductos === 'function') {
                    configurarDestinoBusquedaProductos("combos", agregarProductosAlGrid, obtenerProductosExistentesIds);
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
                configurarDestinoBusquedaProductos("combos", agregarProductosAlGrid, obtenerProductosExistentesIds);
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
        if (!modoNuevoCombo) {
            console.warn("⚠️ No está en modo nuevo combo");
            ControlaMensajeWarning("Debe estar creando un nuevo combo para agregar sustitutos");
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
                    configurarDestinoBusquedaProductos("sustitutos", function (productos) {
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
                configurarDestinoBusquedaProductos("sustitutos", function (productos) {
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

    // Filtrar productos inválidos (el mismo producto o duplicados)
    var sustitutosValidos = sustitutos.filter(function(sustituto) {
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
        
        return true;
    });

    // Si después del filtrado no quedan sustitutos válidos, salir
    if (sustitutosValidos.length === 0) {
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
        ControlaMensajeWarning(`Se descartaron ${descartados} producto(s) no válido(s) como sustituto(s)`);
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

        // Actualizar en el servidor si la URL está definida
        if (typeof resguardarRelacionProductoSustitutoUrl !== 'undefined') {
            guardarRelacionProductoSustitutoEnServidor(productoId);
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
 * Configura eventos para los botones de eliminación de productos
 */
function configurarEventosEliminacionProductos() {
    // Remover eventos previos para evitar duplicación
    $(document).off("click", ".btn-eliminar-producto");

    // Configurar evento de click para eliminar productos
    $(document).on("click", ".btn-eliminar-producto", function (e) {
        e.stopPropagation(); // Evitar que se active la selección de fila

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
 * Elimina un producto del grid
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

    // Si es la única fila, mostrar mensaje "No hay productos"
    if ($("#tbGridProductos tbody tr").length === 1) {
        $("#tbGridProductos tbody").html(`
            <tr>
                <td colspan="${modoNuevoCombo ? 7 : 6}" class="text-center text-muted py-2">
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
function configurarEventosSeleccion() {
    // Aplicar estilo de cursor a todas las filas de la tabla
    $("#tbGridPromoCombo tbody tr").css("cursor", "pointer");

    // Remover eventos previos para evitar duplicación
    $(document).off("click", "#tbGridPromoCombo tbody tr");

    // Configurar evento de click para seleccionar filas (comportamiento de selecci贸n 煇nica)
    $(document).on("click", "#tbGridPromoCombo tbody tr", function (e) {
        if (!$(e.target).is("button, a, .btn, i")) {
            var $this = $(this);
            var wasSelected = $this.hasClass("selected-row");

            // Eliminar la selecci贸n de todas las filas
            $("#tbGridPromoCombo tbody tr").removeClass("selected-row");

            // Seleccionar esta fila solo si no estaba seleccionada anteriormente
            if (!wasSelected) {
                $this.addClass("selected-row");

                // Obtener el ID del combo seleccionado
                var comboId = $this.data("combo-id");

                if (comboId) {

                    //$("#divPromoCombo").removeClass("table-wrapper-500").addClass("table-wrapper-200");
                    $("#divPromoCombo").css("max-height", "200px");
                    $("#divTools").show();
                    // Cargar datos del combo y sus canales
                    cargarDatosCombo(comboId);
                    cargarCanalesCombo(comboId);

                    // Cargar productos del combo
                    cargarProductosCombo(comboId);
                    //se procede a activar el boton modificacion.
                    let est = $this.find("td:nth-child(4) span").data("estado-id");               
                    if (est === "H") {
                        $("#btnAbmModif").prop("disabled", true);
                    }
                    else {
                        $("#btnAbmModif").prop("disabled", false);
                    }
                } else {
                    console.error("No se encontr贸 el ID del combo en la fila seleccionada");
                }
            } else {
                // Si estaba seleccionado y se hace click de nuevo, ocultar datos
                $("#divComboDatos").hide();
            }

            actualizarContadorSeleccionados();
        }
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


    // Activar el botón de nuevo combo
    $("#btnAbmNuevo").prop("disabled", false);
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
    $(document).off("autocompleteselect", "#busquedaModal #Rel01").on("autocompleteselect", "#busquedaModal #Rel01", function (event, ui) {
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

function cargarFamiliasParaBusquedaAvanzadaCombos(proveedorId) {
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

///**
// * Limpia la selección de productos en la búsqueda
// */
//function limpiarSeleccionBusqueda() {
//    $("#divBusquedaAvanzada table tbody tr").removeClass("selected-row");
//    actualizarContadorProductosSeleccionados();
//}

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
    limpiarSeleccionBusqueda();
}

/**
 * Limpia los grids de productos y sustitutos
 * @param {boolean} modoEdicion - Indica si estamos en modo edición (para mostrar columnas de acción)
 */
function limpiarGridsProductos(modoEdicion = modoNuevoCombo) {
    // Crear HTML para un grid vacío de productos
    var htmlProductosVacio = `
    <div class="card h-100">
        <div class="card-header py-1 d-flex justify-content-between align-items-center">
            <h6 class="mb-0">Productos</h6>
            ${modoEdicion ? `
            <button type="button" class="btn btn-sm btn-outline-primary" id="btnAgregarCProducto" title="Agregar Producto">
                <i class="bx bx-plus"></i>
            </button>
            ` : ''}
        </div>
        <div class="card-body p-1">
            <div class="table-responsive" style="max-height: 250px;">
                <table class="table table-sm table-hover mb-0 table-golden" id="tbGridProductos">
                    <thead class="table-golden-header">
                        <tr class="header">
                            <th class="text-center">ID</th>
                            <th class="text-left">Descripción</th>
                            <th class="text-center">Costo</th>
                            <th class="text-center">Cantidad</th>
                            <th class="text-center">Descuento %</th>
                            <th class="text-center">Estado</th>
                            ${modoEdicion ? '<th class="text-center">Acción</th>' : ''}
                        </tr>
                    </thead>
                    <tbody>
                        <tr>
                            <td colspan="${modoEdicion ? 7 : 6}" class="text-center text-muted py-2">
                                <i class="bx bx-info-circle me-1"></i>No hay productos disponibles
                            </td>
                        </tr>
                    </tbody>
                </table>
            </div>
        </div>
    </div>`;

    // Crear HTML para un grid vacío de sustitutos
    var htmlSustitutosVacio = `
    <div class="card h-100">
        <div class="card-header py-1 d-flex justify-content-between align-items-center">
            <h6 class="mb-0">Sustitutos</h6>
            ${modoEdicion ? `
            <button type="button" class="btn btn-sm btn-outline-primary" id="btnAgregarSustituto" title="Agregar Sustituto">
                <i class="bx bx-plus"></i>
            </button>
            ` : ''}
        </div>
        <div class="card-body p-1">
            <div class="table-responsive" style="max-height: 250px;">
                <table class="table table-sm table-hover mb-0 table-golden" id="tbGridSustitutos">
                    <thead class="table-golden-header">
                        <tr class="header">
                            <th class="text-center">ID</th>
                            <th class="text-left">Descripción</th>
                            <th class="text-center">Costo</th>
                            <th class="text-center">Estado</th>
                            ${modoEdicion ? '<th class="text-center">Acción</th>' : ''}
                        </tr>
                    </thead>
                    <tbody>
                        <tr>
                            <td colspan="${modoEdicion ? 5 : 4}" class="text-center text-muted py-2">
                                <i class="bx bx-info-circle me-1"></i>No hay sustitutos disponibles
                            </td>
                        </tr>
                    </tbody>
                </table>
            </div>
        </div>
    </div>`;

    // Actualizar los contenedores con los grids vacíos
    $(".col-sm-4:has(#tbGridProductos)").html(htmlProductosVacio).hide();
    $(".col-sm-4:has(#tbGridSustitutos)").html(htmlSustitutosVacio).hide();

    // Habilitar los botones de agregar solo en modo edición
    if (modoEdicion) {
        $("#btnAgregarCProducto, #btnAgregarSustituto").prop("disabled", false)
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
function cancelarOperacion(e) {    
    // Ocultar formulario
    $("#divComboDatos").hide();
    $("#divPromoCombo").css("max-height", "500px");;
    // Desactivar modos de edición
    modoNuevoCombo = false;
    modoModificacionCombo = false;

    // Restaurar estado de los campos
    restaurarCamposFormulario();

    // Limpiar grid de canales
    $("#divCanales").empty();

    // Limpiar los grids de productos y sustitutos
    limpiarGridsProductos();

    // Deshabilitar explícitamente los botones de agregar después de limpiar grids
    $("#btnAgregarCProducto, #btnAgregarSustituto").prop("disabled", true);

    // Restaurar estado de los botones
    $("#btnAbmNuevo").prop("disabled", false);
    $("#btnAbmAceptar").prop("disabled", true);
    $("#btnAbmModif").prop("disabled", true); // Deshabilitar botón modificar también
    
    accionesIniciales(buscarCombos());
    
}

/**
 * Restaura el estado original de los campos del formulario
 */
function restaurarCamposFormulario() {
    // Restaurar campos a su estado original (readonly/disabled)
    $("#cmb_desc").prop("readonly", true).val("");
    $("#cmb_tipo").prop("disabled", true);
    $("#chkEstadoCombo").prop("disabled", true).prop("checked", false);
    $("#cmb_desde, #cmb_hasta").prop("readonly", true).val("");
    $("#cmb_id").val("");
    $("#cmb_estado").val("");

    // Restaurar badge de estado
    $("#divComboDatos .badge").removeClass("bg-success bg-danger");
}

/**
 * Carga los canales asociados a un combo
 */
function cargarCanalesCombo(comboId) {
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
function cargarDatosCombo(comboId) {
    if (typeof obtenerComboPorIdUrl === 'undefined') {
        console.error("URL para obtener datos del combo no definida");
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

            if (response && response.ok) {
                // Mostrar el panel de datos
                $("#divComboDatos").show();

                // Rellenar los campos del formulario con los datos recibidos
                var datos = response.entidad;
                if (datos) {
                    // Establecer valores en los campos
                    $("#cmb_id").val(datos.cmb_id); // Este campo sigue siendo readonly

                    // Habilitar y establecer valores en campos editables
                    $("#cmb_desc").val(datos.cmb_desc).prop("readonly", false);

                    // Actualizar campo tipo
                    $("#cmb_tipo").val(datos.cmb_tipo).prop("disabled", false);

                    // Determinar el estado activo y configurar checkbox
                    var esActivo = datos.cmb_estado === 'A';
                    $("#cmb_estado").val(datos.cmb_estado);
                    $("#chkEstadoCombo").prop("checked", esActivo);
                    $("#lblEstadoCombo").text(esActivo ? "Activo" : "No activo");

                    // OPTIMIZACIÓN: Deshabilitar el checkbox SOLO si está activo
                    // Si está inactivo, mantenerlo habilitado para permitir activación
                    $("#chkEstadoCombo").prop("disabled", esActivo);

                    // Actualizar fechas y habilitarlas
                    $("#cmb_desde").val(formatearFecha(datos.cmb_desde)).prop("readonly", false);
                    $("#cmb_hasta").val(formatearFecha(datos.cmb_hasta)).prop("readonly", false);

                    // Actualizar badge de estado
                    var estadoBadge = $("#divComboDatos .badge");
                    estadoBadge.removeClass("bg-success bg-danger")
                        .addClass(datos.pasa_activar ? "bg-success" : "bg-danger")
                        .text(datos.pasa_activar ? "ACTIVADO" : "SIN ACTIVAR");
                }
            } else {
                ControlaMensajeError("Error al obtener datos del combo: " + (response.mensaje || "Error desconocido"));
            }
        },
        error: function (xhr, status, error) {
            CerrarWaiting();
            console.error("Error al cargar datos del combo:", error);
            ControlaMensajeError("Error al cargar datos del combo: " + error);
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

    // Configurar estado como no activo y habilitarlo
    $("#cmb_estado").val("N");
    $("#chkEstadoCombo").prop("checked", false).prop("disabled", false);
    $("#lblEstadoCombo").text("No activo");

    // Configurar fechas
    const hoy = new Date();
    const tresMesesDespues = new Date(hoy);
    tresMesesDespues.setMonth(hoy.getMonth() + 3);

    $("#cmb_desde").val(formatearFecha(hoy)).prop("readonly", false);
    $("#cmb_hasta").val(formatearFecha(tresMesesDespues)).prop("readonly", false);

    // Actualizar badge de estado
    var estadoBadge = $("#divComboDatos .badge");
    estadoBadge.removeClass("bg-success bg-danger")
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
    // Ocultar la columna de estado (cuarta columna)
    //$("#tbGridCanales th:nth-child(1), #tbGridCanales td:nth-child(1)").hide();
    //$("#tbGridCanales th:nth-child(2), #tbGridCanales td:nth-child(2)").hide();
    $("#tbGridCanales th:nth-child(4), #tbGridCanales td:nth-child(4)").hide();

    // Añadir columna de selección en el encabezado
    $("#tbGridCanales thead tr").prepend("<th class='text-center'>Selección</th>");

    // Añadir checkbox a cada fila
    $("#tbGridCanales tbody tr").each(function () {
        var canalId = $(this).find("td:eq(0)").text().trim();
        var incluido = $(this).find("td:eq(3) .badge").text().includes("Incluido");

        $(this).prepend(
            `<td class='text-center'>
                <div class='form-check'>
                    <input class='form-check-input canal-checkbox' type='checkbox' value='${canalId}' ${incluido ? 'checked' : ''}>
                </div>
            </td>`
        );
    });

    // Añadir evento para manejar la selección de canales
    $(".canal-checkbox").on("change", function () {
        var checked = $(this).prop("checked");
        // Se podría implementar lógica adicional aquí
        console.log("Canal " + $(this).val() + " " + (checked ? "seleccionado" : "deseleccionado"));
    });
}

/**
 * Formatea una fecha en formato yyyy-MM-dd para inputs de tipo date
 */
function formatearFecha(fechaStr) {
    if (!fechaStr) return "";

    // Si es string ISO, convertir a objeto Date
    var fecha = typeof fechaStr === 'string' ? new Date(fechaStr) : new Date(fechaStr);

    // Verificar si es una fecha válida
    if (isNaN(fecha.getTime())) return "";

    // Formatear como yyyy-MM-dd para input type="date"
    return fecha.toISOString().split('T')[0];
}

/**
 * Agrega productos al grid
 */
function agregarProductosAlGrid(productos) {
    if (productos.length === 0) return;

    // Obtener el tbody de la tabla
    var $tbody = $("#tbGridProductos tbody");

    // Limpiar mensaje "No hay productos" si existe
    if ($tbody.find("tr td[colspan]").length > 0) {
        $tbody.empty();
    }

    // Obtener ID del combo actuales
    var comboId = $("#cmb_id").val();

    // Agregar cada producto como una nueva fila
    $.each(productos, function (i, producto) {
        // MODIFICADO: Manejo de estado histórico
        var estadoTexto, estadoClase;
        
        if (producto.activo === 'A') {
            estadoTexto = "Activo";
            estadoClase = "bg-success";
        } else if (producto.activo === 'H') {
            estadoTexto = "Histórico";
            estadoClase = "bg-secondary"; // Usar color gris para histórico
        } else {
            estadoTexto = "Pendiente";
            estadoClase = "bg-danger";
        }
        
        var fila = `
        <tr data-producto-id="${producto.p_id}" data-combo-id="${comboId}" data-producto-estado="${producto.activo}">
            <td class="text-center">
                ${producto.p_id}
            </td>
            <td>
                ${producto.p_desc}
            </td>
            <td class="text-end">
                ${producto.p_pcosto.toFixed(3)}
            </td>
            <td class="text-end">
                <div class="input-container">
                    <input type="text" class="form-control form-control-sm input-cantidad input-numeric"
                           value="${producto.cantidad.toFixed(2)}"
                           data-producto-id="${producto.p_id}"
                           data-original-value="${producto.cantidad}"
                           readonly />
                </div>
            </td>
            <td class="text-end">
                <div class="input-container">
                    <input type="text" class="form-control form-control-sm input-descuento input-numeric"
                           value="${producto.dto_porc.toFixed(2)}"
                           data-producto-id="${producto.p_id}"
                           data-original-value="${producto.dto_porc}"
                           readonly />
                </div>
            </td>
            <td class="text-center">
                <span class="badge ${estadoClase}">
                    ${estadoTexto}
                </span>
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
            .filter(function() { 
                return $(this).text().trim() === producto || $(this).closest('tr').data('producto-id') === producto;
            }).closest('tr') : 
        $(producto).closest('tr');
    
    if ($fila.length === 0) return false;
    
    // Verificar texto del badge en la columna de estado (6ta columna)
    var estadoTexto = $fila.find("td:nth-child(6) .badge").text().trim();
    return estadoTexto === "Histórico";
}

/**
 * Inicializa los campos editables para cantidad y descuento en la grilla de productos
 */
function inicializarCamposEditablesProductos() {
    console.log("🔄 Inicializando campos editables en grid de productos");

    // Si no estamos en modo edición, mantener los campos readonly y salir
    if (!modoNuevoCombo && !modoModificacionCombo) {
        $('.input-cantidad, .input-descuento').prop('readonly', true);
        console.log("✅ Campos configurados como readonly (modo visualización)");
        return;
    }

    // NUEVO: Capturar el evento mousedown que ocurre ANTES del click
    $(document).off('mousedown', '.input-cantidad, .input-descuento').on('mousedown', '.input-cantidad, .input-descuento', function (e) {
        // Si no estamos en modo edición, no permitir la edición
        if (!modoNuevoCombo && !modoModificacionCombo) {
            e.preventDefault();
            return false;
        }
        
        // NUEVA VERIFICACIÓN: Comprobar si el producto es histórico
        var productoId = $(this).data('producto-id');
        if (esProductoHistorico(productoId)) {
            e.preventDefault();
            e.stopPropagation();
            // No establecemos campoEnPreparacionEdicion para prevenir edición
            // Mostraremos el mensaje de advertencia en el click
            return false;
        }
        
        // Marcar este elemento como "en preparación para edición"
        campoEnPreparacionEdicion = this;
    });

    // 1. Configurar campos editables al hacer clic
    $(document).off('click', '.input-cantidad, .input-descuento')
        .on('click', '.input-cantidad, .input-descuento', function (e) {
            // Si no estamos en modo edición, no permitir la edición
            if (!modoNuevoCombo && !modoModificacionCombo) {
                e.preventDefault();
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
        // Configuración para cantidad (2 decimales)
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
        }).mask('.input-cantidad');

        // Configuración para descuento (2 decimales, máx 100%)
        Inputmask({
            alias: "numeric",
            groupSeparator: ",",
            radixPoint: ".",
            autoGroup: true,
            digits: 2,
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
    const decimales = $campo.hasClass('input-cantidad') ? 2 : 2;
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
                .focus()
                .select();
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
                .focus()
                .select();
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
                var comboId = $("#cmb_id").val();

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

                    // Mostrar mensaje si no hay sustitutos
                    if (!response.sustitutos || response.sustitutos.length === 0) {
                        ControlaMensajeWarning("El producto \"" + productoDesc + "\" (ID: " + productoId + ") no tiene sustitutos asociados.");
                    }
                } else {
                    ControlaMensajeWarning(response.mensaje || "No se encontraron sustitutos para este producto");
                }
            } else {
                // Para modo visualización, procesamos respuesta HTML de ObtenerProductosSustitutos
                // Actualizar el contenido del grid de sustitutos
                $(".col-sm-4:has(#tbGridSustitutos)").html(response).show();

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

                    if (!tieneFilasConDatos) {
                        ControlaMensajeWarning("El producto \"" + productoDesc + "\" (ID: " + productoId + ") no tiene sustitutos asociados.");
                    }
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
            <td class="text-center">
                <span class="badge ${sustituto.activo == 'A' ? "bg-success" : "bg-danger"}">
                    ${sustituto.activo == 'A' ? "Activo" : "Pendiente"}
                </span>
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
 * Carga los productos asociados a un combo
 * @param {string} comboId - ID del combo
 */
function cargarProductosCombo(comboId) {
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

            // Actualizar el contenido del grid de productos
            $(".col-sm-4:has(#tbGridProductos)").html(html).show();

            if ($("#tbGridProductos tbody tr").length === 0) {
                // Si no hay filas después de cargar, mostrar mensaje "No hay productos"
                $("#tbGridProductos tbody").html(`
                    <tr>
                        <td colspan="${modoNuevoCombo ? 7 : 6}" class="text-center text-muted py-2">
                            <i class="bx bx-info-circle me-1"></i>No hay productos disponibles
                        </td>
                    </tr>
                `);
            } else {
                // Si estamos viendo un combo existente (no en modo nuevo combo)
                if (!modoNuevoCombo) {
                    // Asegurar que los campos estén en modo readonly
                    $("#tbGridProductos .input-cantidad, #tbGridProductos .input-descuento").prop("readonly", true);
                    
                    // Si la columna de acción existe, ocultarla
                    if ($("#tbGridProductos th").length > 6) {
                        $("#tbGridProductos th:last-child, #tbGridProductos td:last-child").hide();
                    }
                } else {
                    // En modo edición, inicializar los campos editables
                    inicializarCamposEditablesProductos();
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

    // 2. Recopilar canales seleccionados (solo para nuevo combo)
    var canales = [];
    if (modoNuevoCombo) {
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
 * Recopila los productos desde el grid con sus cantidades y descuentos actualizados
 * @returns {Array} Array con los productos del combo
 */
function recopilarProductosCombo() {
    var productos = [];
    
    // Recorrer todas las filas del grid de productos
    $("#tbGridProductos tbody tr").each(function () {
        // Verificar que no sea la fila de "No hay productos"
        if (!$(this).find("td[colspan]").length) {
            var $fila = $(this);
            
            // Obtener valores de los inputs
            var cantidad = parseFloat($fila.find(".input-cantidad").val().replace(/,/g, '')) || 0;
            var descuento = parseFloat($fila.find(".input-descuento").val().replace(/,/g, '')) || 0;
            
            var producto = {
                cmb_id: $fila.data("combo-id") || '',
                p_id: $fila.data("producto-id") || $fila.find("td:eq(0)").text().trim(),
                p_desc: $fila.find("td:eq(1)").text().trim(),
                p_pcosto: parseFloat($fila.find("td:eq(2)").text().replace(/,/g, '')) || 0,
                cantidad: cantidad,
                dto_porc: descuento,
                activo: 'A'
            };
            
            productos.push(producto);
        }
    });

    return productos;
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

                // Cancelar operación y volver al estado inicial
                setTimeout(function () {
                    cancelarOperacion();
                    refrescarGridPromoCombo();
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
 * Activa un combo o promoción existente
 * @param {string} tipoDesc - Descripción del tipo de entidad (combo o promoción)
 */
function activarComboExistente(tipoDesc) {
    // Verificar URL de confirmación
    if (typeof confirmacionComboUrl === 'undefined') {
        console.error("URL de confirmación no definida");
        ControlaMensajeError("Error de configuración: URL de confirmación no definida");
        return;
    }
    
    // 1. Mostrar indicador de progreso
    AbrirWaiting(`Activando ${tipoDesc}...`);
    
    // 2. Recopilar datos existentes
    var datos = {
        cmb_id: $("#cmb_id").val(),
        cmb_desc: $("#cmb_desc").val().trim(),
        cmb_tipo: $("#cmb_tipo").val() || 'C',
        cmb_estado: 'A', // Forzar estado activo
        cmb_desde: $("#cmb_desde").val(),
        cmb_hasta: $("#cmb_hasta").val(),
        pasa_activar: true, // Indicar que es una activación
        pasa_historico: false
    };
    
    // 3. Recopilar canales (todos los visibles en la tabla, ya que estamos en visualización)
    var canales = [];
    $("#tbGridCanales tbody tr").each(function() {
        // Obtener datos de columnas relevantes (ajustar índices según estructura real)
        var fila = $(this);
        var canal = {
            adm_id: fila.find("td:eq(1)").text().trim(),
            //adm_id: $fila.find("td:eq(1)").text().trim(),
            //adm_nombre: $fila.find("td:eq(2)").text().trim(),
            lp_id: fila.find("td:eq(3)").text().trim(),
            //lp_desc: $fila.find("td:eq(4)").text().trim(),
            canal: fila.find("td:eq(0)").text().trim(),
            incluida: 'S'
        };
        canales.push(canal);
    });
    
    // 4. Recopilar productos con los valores mostrados actualmente
    var productos = [];
    $("#tbGridProductos tbody tr").each(function() {
        // Verificar que no sea la fila de "No hay productos"
        if (!$(this).find("td[colspan]").length) {
            var fila = $(this);
            var producto = {
                cmb_id: datos.cmb_id,
                p_id: fila.find("td:eq(0)").text().trim(),
                p_desc: fila.find("td:eq(1)").text().trim(),
                p_pcosto: parseFloat(fila.find("td:eq(2)").text().replace(/[^\d.-]/g, '')) || 0,
                cantidad: parseFloat(fila.find(".input-cantidad").val().replace(/,/g, '')) || 1,
                dto_porc: parseFloat(fila.find(".input-descuento").val().replace(/,/g, '')) || 0,
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
                ControlaMensajeSuccess(
                    response.msg || `${tipoDesc.charAt(0).toUpperCase() + tipoDesc.slice(1)} activado correctamente`
                );
                
                // Actualizar interfaz para reflejar el estado activo
                var estadoBadge = $("#divComboDatos .badge");
                estadoBadge.removeClass("bg-danger").addClass("bg-success")
                    .text("ACTIVADO");
                
                // Deshabilitar el checkbox para prevenir cambios (cumple con la lógica existente)
                $("#chkEstadoCombo").prop("disabled", true);

                // Refrescar el grid con los filtros vigentes
                refrescarGridPromoCombo();
            } else {
                // Error o advertencia
                var mensaje = response.msg || `Error al activar ${tipoDesc}`;
                if (response.warn) {
                    ControlaMensajeWarning(mensaje);
                    // Restaurar estado anterior ya que no se pudo activar
                    $("#chkEstadoCombo").prop("checked", false);
                    $("#cmb_estado").val('N');
                    $("#lblEstadoCombo").text("No activo");
                } else {
                    ControlaMensajeError(mensaje);
                    // Restaurar estado anterior ya que no se pudo activar
                    $("#chkEstadoCombo").prop("checked", false);
                    $("#cmb_estado").val('N');
                    $("#lblEstadoCombo").text("No activo");
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
            
            // Restaurar estado anterior ya que no se pudo activar
            $("#chkEstadoCombo").prop("checked", false);
            $("#cmb_estado").val('N');
            $("#lblEstadoCombo").text("No activo");
        }
    });
}