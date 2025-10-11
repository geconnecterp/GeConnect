/**
 * Script para gestión de combos y promociones
 */
$(function () {
    // Inicialización
    console.log("🚀 Inicializando módulo de combos y promociones");
    
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
    
    // Inicializar estados
    accionesIniciales();
});

// Variables globales para manejar relaciones de productos y sustitutos
var productosSustitutosMap = {};
var modoNuevoCombo = false;

/**
 * Inicializa los eventos para los elementos del formulario
 */
function inicializarEventos() {
    // Configurar el evento click para el botón Cancelar/Inicializar
    $("#btnCancel, #btnAbmCancelar").on("click", function(e) {
        cancelarOperacion(e);
    });
    
    // Configurar el evento click para el botón Buscar/Filtrar
    $("#btnBuscar").on("click", function() {
        buscarCombos();
    });
    funcCallBack = buscarCombos;

    // Eventos para los checkboxes del filtro
    $("#chkTipo").on("change", function() {
        $("#Tipo").prop("disabled", !$(this).prop("checked"));
    });
    
    $("#chkEstado").on("change", function() {
        $("#Estado").prop("disabled", !$(this).prop("checked"));
    });
    
    // Evento para el checkbox de estado del combo
    $(document).on("change", "#chkEstadoCombo", function() {
        // Actualizar el valor oculto del estado
        var nuevoEstado = $(this).prop("checked") ? 'A' : 'N';
        $("#cmb_estado").val(nuevoEstado);
        
        // Actualizar el texto de la etiqueta
        $("#lblEstadoCombo").text($(this).prop("checked") ? "Activo" : "No activo");
    });
    
    // Evento para el botón de nuevo combo
    $("#btnAbmNuevo").on("click", function() {
        modoNuevoCombo = true;
        inicializarNuevoCombo();
        
        // Activar/desactivar botones
        $("#btnAbmAceptar").prop("disabled", false);
        $("#btnAbmNuevo").prop("disabled", true);

        // Cargar el modal de búsqueda avanzada
        cargarModalBusquedaAvanzada();
        
        // Inicializar el mapa de sustitutos
        productosSustitutosMap = {};
    });
    
    // Evento para el botón confirmar
    $("#btnAbmAceptar").on("click", function() {
        // Aquí iría el código para guardar el nuevo combo
        // Por ahora solo logueamos la acción
        console.log("Confirmar nuevo combo/promo");
    });

    // Evento delegado para el botón de agregar producto
    $(document).on("click", "#btnAgregarCProducto", function () {
        // Cargar el modal si no existe y luego mostrarlo
        if ($("#busquedaModal").length === 0) {
            cargarModalBusquedaAvanzada(function () {
                // Configurar el destino como "combos" y definir el callback
                if (typeof configurarDestinoBusquedaProductos === 'function') {
                    configurarDestinoBusquedaProductos("combos", agregarProductosAlGrid, obtenerProductosExistentesIds);
                }
                $("#busquedaModal").modal("show");
            });
        } else {
            // Si ya existe, configurar destino y mostrar
            if (typeof configurarDestinoBusquedaProductos === 'function') {
                configurarDestinoBusquedaProductos("combos", agregarProductosAlGrid, obtenerProductosExistentesIds);
            }
            $("#busquedaModal").modal("show");
        }
    });

    // Evento delegado para el botón de agregar sustituto
    $(document).on("click", "#btnAgregarSustituto", function () {
        // Verificar si hay un producto seleccionado
        var productoSeleccionado = $("#tbGridProductos tbody tr.selected-row");
        if (productoSeleccionado.length === 0) {
            ControlaMensajeWarning("Debe seleccionar un producto antes de agregar sustitutos");
            return;
        }

        var productoId = productoSeleccionado.find("td:first").text().trim();
        var productoDesc = productoSeleccionado.find("td:nth-child(2)").text().trim();
        
        // Cargar el modal de búsqueda avanzada
        if ($("#busquedaModal").length === 0) {
            cargarModalBusquedaAvanzada(function () {
                // Configurar el destino como "sustitutos" y definir el callback
                if (typeof configurarDestinoBusquedaProductos === 'function') {
                    configurarDestinoBusquedaProductos("sustitutos", function(productos) {
                        agregarSustitutosAlGrid(productos, productoId);
                    }, obtenerSustitutosExistentesIds);
                }
                $("#busquedaModal").modal("show");
            });
        } else {
            // Si ya existe, configurar destino y mostrar
            if (typeof configurarDestinoBusquedaProductos === 'function') {
                configurarDestinoBusquedaProductos("sustitutos", function(productos) {
                    agregarSustitutosAlGrid(productos, productoId);
                }, obtenerSustitutosExistentesIds);
            }
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

    // Añadir los nuevos sustitutos al mapa, evitando duplicados
    sustitutos.forEach(function (sustituto) {
        // Verificar si ya existe para evitar duplicados
        if (!productosSustitutosMap[productoId].some(s => s.p_id === sustituto.p_id)) {
            productosSustitutosMap[productoId].push(sustituto);
        }
    });

    // Guardar en sesión
    guardarSustitutosEnSesion();

    // Guardar en el servidor si la URL está definida
    if (typeof resguardarRelacionProductoSustitutoUrl !== 'undefined') {
        guardarRelacionProductoSustitutoEnServidor(productoId);
    }

    // Actualizar el grid de sustitutos
    actualizarGridSustitutos(productoId);
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

    // Remover eventos previos para evitar duplicaci贸n
    $(document).off("click", "#tbGridPromoCombo tbody tr");

    // Configurar evento de click para seleccionar filas (comportamiento de selecci贸n 煤nica)
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
                    // Cargar datos del combo y sus canales
                    cargarDatosCombo(comboId);
                    cargarCanalesCombo(comboId);

                    // Cargar productos del combo
                    cargarProductosCombo(comboId);
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


function accionesIniciales() {
    if ($("#divDetalle").is(":visible")) {
        $("#divDetalle").collapse("hide");
    }
    $("#divFiltro").collapse("show");   
    
    // Habilitar los campos de filtro por defecto
    $("#Tipo").prop("disabled", false);
    $("#Estado").prop("disabled", false);
    
    // Activar el botón de nuevo combo
    $("#btnAbmNuevo").prop("disabled", false);

    $("#pagEstado").on("change", function () {
        var div = $("#divPaginacion");
        presentaPaginacion(div);
    });
    //callback para que funcione la paginación
    var funcCallBack = buscarCombos;

    // Delegación de eventos para autocomplete en el modal
    $(document).on("autocompleteselect", "#busquedaModal #Rel01", function (event, ui) {
        setTimeout(function () {
            cargarFamiliasParaBusquedaAvanzadaCombos(ui.item.id);
        }, 100);
    });
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

/**
 * Limpia la selección de productos en la búsqueda
 */
function limpiarSeleccionBusqueda() {
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
    limpiarSeleccionBusqueda();
}

/**
 * Formatea una fecha en formato yyyy-MM-dd para inputs de tipo date
 */
function formatearFecha(fechaStr) {
    if (!fechaStr) return "";

    // Si es string ISO, convertir a objeto Date
    var fecha = typeof fechaStr === 'string' ? new Date(fechaStr) : new Date(fechaStr);

    // Verificar si es una fecha v谩lida
    if (isNaN(fecha.getTime())) return "";

    // Formatear como yyyy-MM-dd para input type="date"
    return fecha.toISOString().split('T')[0];
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
    $(".col-sm-6:has(#tbGridProductos)").html(htmlProductosVacio);
    $(".col-sm-6:has(#tbGridSustitutos)").html(htmlSustitutosVacio);
    
    // Habilitar los botones de agregar solo en modo edición
    if (modoEdicion) {
        $("#btnAgregarCProducto, #btnAgregarSustituto").prop("disabled", false);
    }
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

    // Obtener ID del combo actual
    var comboId = $("#cmb_id").val();

    // Agregar cada producto como una nueva fila
    $.each(productos, function (i, producto) {
        var fila = `
        <tr data-producto-id="${producto.p_id}" data-combo-id="${comboId}">
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
                ${producto.cantidad.toFixed(2)}
            </td>
            <td class="text-end">
                ${producto.dto_porc.toFixed(2)}
            </td>
            <td class="text-center">
                <span class="badge ${producto.activo == 'A' ? "bg-success" : "bg-danger"}">
                    ${producto.activo == 'A' ? "Activo" : "Pendiente"}
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
 * Elimina un producto del grid y actualiza los datos relacionados
 */
function eliminarProductoDeGrid($fila, productoId) {
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
        // Si es la fila seleccionada, seleccionar otra
        if ($fila.hasClass("selected-row")) {
            var $siguienteFila = $fila.next("tr");
            if (!$siguienteFila.length) {
                $siguienteFila = $fila.prev("tr");
            }

            $fila.remove();

            // Seleccionar la siguiente fila si existe
            if ($siguienteFila.length) {
                $siguienteFila.trigger("click");
            }
        } else {
            // Si no es la seleccionada, simplemente eliminarla
            $fila.remove();
        }
    }

    // Mostrar mensaje de éxito
    ControlaMensajeSuccess("Producto eliminado correctamente");
}

/**
 * Configura los eventos para la selecci贸n de filas en la tabla de productos
 */
function configurarSeleccionProductos() {
    // Aplicar estilo de cursor a todas las filas de la tabla de productos
    $("#tbGridProductos tbody tr").css("cursor", "pointer");

    // Remover eventos previos para evitar duplicaci贸n
    $(document).off("click", "#tbGridProductos tbody tr");

    // Configurar evento de click para seleccionar filas (comportamiento de selecci贸n 煤nica)
    $(document).on("click", "#tbGridProductos tbody tr", function (e) {
        if (!$(e.target).is("button, a, .btn, i")) {
            var $this = $(this);

            // Eliminar la selecci贸n de todas las filas
            $("#tbGridProductos tbody tr").removeClass("selected-row");

            // Seleccionar esta fila
            $this.addClass("selected-row");

            // Obtener el ID del producto seleccionado
            var productoId = $this.find("td:first").text().trim();

            // Obtener el ID del combo actual
            var comboId = $("#cmb_id").val();

            if (productoId && comboId) {
                // Cargar los sustitutos del producto seleccionado
                cargarProductosSustitutos(comboId, productoId);

                // Guardar el ID del producto seleccionado para uso futuro
                p_id_selected = productoId;
            }
        }
    });
}

/**
 * Actualiza el grid de sustitutos con los datos del producto seleccionado
 * @param {string} productoId - ID del producto para el que se mostrarán los sustitutos
 */
function actualizarGridSustitutos(productoId) {
    // Obtener sustitutos para el producto desde el mapa
    var sustitutos = productosSustitutosMap[productoId] || [];
    
    // Obtener el tbody de la tabla
    var $tbody = $("#tbGridSustitutos tbody");
    
    // Limpiar tabla actual
    $tbody.empty();
    
    if (sustitutos.length === 0) {
        // Mostrar mensaje de "No hay sustitutos"
        $tbody.html(`
            <tr>
                <td colspan="${modoNuevoCombo ? 5 : 4}" class="text-center text-muted py-2">
                    <i class="bx bx-info-circle me-1"></i>No hay sustitutos disponibles
                </td>
            </tr>
        `);
        return;
    }
    
    // Agregar cada sustituto como una nueva fila
    $.each(sustitutos, function(i, sustituto) {
        var fila = `
        <tr data-producto-id="${sustituto.p_id}" data-combo-id="">
            <td class="text-center">
                ${sustituto.p_id}
            </td>
            <td>
                ${sustituto.p_desc}
            </td>
            <td class="text-end">
                ${typeof sustituto.p_pcosto === 'number' ? sustituto.p_pcosto.toFixed(2) : '0.00'}
            </td>
            <td class="text-center">
                <span class="badge bg-success">
                    Activo
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
    
    // Configurar eventos para los botones de eliminar si estamos en modo de edición
    if (modoNuevoCombo) {
        configurarEventosEliminacionSustitutos();
    }
}

/**
 * Carga los productos asociados a un combo
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
        success: function(html) {
            CerrarWaiting();
            
            // Actualizar el contenido del grid de productos
            $(".col-sm-6:has(#tbGridProductos)").html(html);
            
            // Si estamos viendo un combo existente, ocultar la columna de acción
            if (!modoNuevoCombo) {
                $("#tbGridProductos th:last-child, #tbGridProductos td:last-child").hide();
            }
            
            // Configurar eventos de selección en la tabla de productos
            configurarSeleccionProductos();
            
            // Después de cargar productos, cargar los sustitutos del primer producto (si existe)
            setTimeout(function() {
                // Buscar el primer producto en la tabla
                var primerProducto = $("#tbGridProductos tbody tr:first");
                if (primerProducto.length > 0) {
                    var productoId = primerProducto.find("td:first").text().trim();
                    if (productoId) {
                        // Marcar el primer producto como seleccionado
                        primerProducto.addClass("selected-row");
                        
                        // Cargar los sustitutos del primer producto
                        cargarProductosSustitutos(comboId, productoId);
                    }
                }
            }, 100); // Pequeño retraso para asegurar que el DOM se actualice
        },
        error: function(xhr, status, error) {
            CerrarWaiting();
            console.error("Error al cargar productos:", error);
            ControlaMensajeError("Error al cargar productos: " + error);
        }
    });
}

/**
 * Carga los productos sustitutos asociados a un producto dentro de un combo
 */
function cargarProductosSustitutos(comboId, productoId) {
    if (typeof obtenerProductosSustitutosUrl === 'undefined') {
        console.error("URL para obtener productos sustitutos no definida");
        return;
    }
    
    AbrirWaiting("Cargando productos sustitutos...");
    
    // Obtener descripción del producto seleccionado para el mensaje
    var productoDesc = $("#tbGridProductos tbody tr.selected-row td:nth-child(2)").text().trim();
    
    $.ajax({
        url: obtenerProductosSustitutosUrl,
        type: "POST",
        data: { comboId: comboId, productoId: productoId },
        success: function(html) {
            CerrarWaiting();
            
            // Actualizar el contenido del grid de sustitutos
            $(".col-sm-6:has(#tbGridSustitutos)").html(html);
            
            // Si estamos viendo un combo existente, ocultar la columna de acción
            if (!modoNuevoCombo) {
                $("#tbGridSustitutos th:last-child, #tbGridSustitutos td:last-child").hide();
            }
            
            // Verificar si hay sustitutos después de cargar el HTML
            setTimeout(function() {
                var tieneFilasConDatos = $("#tbGridSustitutos tbody tr").length > 0 && 
                                         !$("#tbGridSustitutos tbody tr td").text().includes("No hay sustitutos disponibles");
                
                if (!tieneFilasConDatos) {
                    // Usar ControlaMensajeWarning en lugar de AbrirMensaje
                    ControlaMensajeWarning("El producto \"" + productoDesc + "\" (ID: " + productoId + ") no tiene sustitutos asociados.");
                }
            }, 100); // Pequeño retraso para asegurar que el DOM se actualice
        },
        error: function(xhr, status, error) {
            CerrarWaiting();
            console.error("Error al cargar productos sustitutos:", error);
            ControlaMensajeError("Error al cargar productos sustitutos: " + error);
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
function limpiarSeleccionBusqueda() {
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
    $(".col-sm-6:has(#tbGridProductos)").html(htmlProductosVacio);
    $(".col-sm-6:has(#tbGridSustitutos)").html(htmlSustitutosVacio);
    
    // Habilitar los botones de agregar solo en modo edición
    if (modoEdicion) {
        $("#btnAgregarCProducto, #btnAgregarSustituto").prop("disabled", false);
    }
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

    // Obtener ID del combo actual
    var comboId = $("#cmb_id").val();

    // Agregar cada producto como una nueva fila
    $.each(productos, function (i, producto) {
        var fila = `
        <tr data-producto-id="${producto.p_id}" data-combo-id="${comboId}">
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
                ${producto.cantidad.toFixed(2)}
            </td>
            <td class="text-end">
                ${producto.dto_porc.toFixed(2)}
            </td>
            <td class="text-center">
                <span class="badge ${producto.activo == 'A' ? "bg-success" : "bg-danger"}">
                    ${producto.activo == 'A' ? "Activo" : "Pendiente"}
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
 * Actualiza el grid de sustitutos con los datos del producto seleccionado
 * @param {string} productoId - ID del producto para el que se mostrarán los sustitutos
 */
function actualizarGridSustitutos(productoId) {
    // Obtener sustitutos para el producto desde el mapa
    var sustitutos = productosSustitutosMap[productoId] || [];
    
    // Obtener el tbody de la tabla
    var $tbody = $("#tbGridSustitutos tbody");
    
    // Limpiar tabla actual
    $tbody.empty();
    
    if (sustitutos.length === 0) {
        // Mostrar mensaje de "No hay sustitutos"
        $tbody.html(`
            <tr>
                <td colspan="${modoNuevoCombo ? 5 : 4}" class="text-center text-muted py-2">
                    <i class="bx bx-info-circle me-1"></i>No hay sustitutos disponibles
                </td>
            </tr>
        `);
        return;
    }
    
    // Agregar cada sustituto como una nueva fila
    $.each(sustitutos, function(i, sustituto) {
        var fila = `
        <tr data-producto-id="${sustituto.p_id}" data-combo-id="">
            <td class="text-center">
                ${sustituto.p_id}
            </td>
            <td>
                ${sustituto.p_desc}
            </td>
            <td class="text-end">
                ${typeof sustituto.p_pcosto === 'number' ? sustituto.p_pcosto.toFixed(2) : '0.00'}
            </td>
            <td class="text-center">
                <span class="badge bg-success">
                    Activo
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
    
    // Configurar eventos para los botones de eliminar si estamos en modo de edición
    if (modoNuevoCombo) {
        configurarEventosEliminacionSustitutos();
    }
}

/**
 * Carga los productos asociados a un combo
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
        success: function(html) {
            CerrarWaiting();
            
            // Actualizar el contenido del grid de productos
            $(".col-sm-6:has(#tbGridProductos)").html(html);
            
            // Si estamos viendo un combo existente, ocultar la columna de acción
            if (!modoNuevoCombo) {
                $("#tbGridProductos th:last-child, #tbGridProductos td:last-child").hide();
            }
            
            // Configurar eventos de selección en la tabla de productos
            configurarSeleccionProductos();
            
            // Después de cargar productos, cargar los sustitutos del primer producto (si existe)
            setTimeout(function() {
                // Buscar el primer producto en la tabla
                var primerProducto = $("#tbGridProductos tbody tr:first");
                if (primerProducto.length > 0) {
                    var productoId = primerProducto.find("td:first").text().trim();
                    if (productoId) {
                        // Marcar el primer producto como seleccionado
                        primerProducto.addClass("selected-row");
                        
                        // Cargar los sustitutos del primer producto
                        cargarProductosSustitutos(comboId, productoId);
                    }
                }
            }, 100); // Pequeño retraso para asegurar que el DOM se actualice
        },
        error: function(xhr, status, error) {
            CerrarWaiting();
            console.error("Error al cargar productos:", error);
            ControlaMensajeError("Error al cargar productos: " + error);
        }
    });
}

/**
 * Carga los productos sustitutos asociados a un producto dentro de un combo
 */
function cargarProductosSustitutos(comboId, productoId) {
    if (typeof obtenerProductosSustitutosUrl === 'undefined') {
        console.error("URL para obtener productos sustitutos no definida");
        return;
    }
    
    AbrirWaiting("Cargando productos sustitutos...");
    
    // Obtener descripción del producto seleccionado para el mensaje
    var productoDesc = $("#tbGridProductos tbody tr.selected-row td:nth-child(2)").text().trim();
    
    $.ajax({
        url: obtenerProductosSustitutosUrl,
        type: "POST",
        data: { comboId: comboId, productoId: productoId },
        success: function(html) {
            CerrarWaiting();
            
            // Actualizar el contenido del grid de sustitutos
            $(".col-sm-6:has(#tbGridSustitutos)").html(html);
            
            // Si estamos viendo un combo existente, ocultar la columna de acción
            if (!modoNuevoCombo) {
                $("#tbGridSustitutos th:last-child, #tbGridSustitutos td:last-child").hide();
            }
            
            // Verificar si hay sustitutos después de cargar el HTML
            setTimeout(function() {
                var tieneFilasConDatos = $("#tbGridSustitutos tbody tr").length > 0 && 
                                         !$("#tbGridSustitutos tbody tr td").text().includes("No hay sustitutos disponibles");
                
                if (!tieneFilasConDatos) {
                    // Usar ControlaMensajeWarning en lugar de AbrirMensaje
                    ControlaMensajeWarning("El producto \"" + productoDesc + "\" (ID: " + productoId + ") no tiene sustitutos asociados.");
                }
            }, 100); // Pequeño retraso para asegurar que el DOM se actualice
        },
        error: function(xhr, status, error) {
            CerrarWaiting();
            console.error("Error al cargar productos sustitutos:", error);
            ControlaMensajeError("Error al cargar productos sustitutos: " + error);
        }
    });
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
function limpiarSeleccionBusqueda() {
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
    $(".col-sm-6:has(#tbGridProductos)").html(htmlProductosVacio);
    $(".col-sm-6:has(#tbGridSustitutos)").html(htmlSustitutosVacio);
    
    // Habilitar los botones de agregar solo en modo edición
    if (modoEdicion) {
        $("#btnAgregarCProducto, #btnAgregarSustituto").prop("disabled", false);
    }
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

    // Obtener ID del combo actual
    var comboId = $("#cmb_id").val();

    // Agregar cada producto como una nueva fila
    $.each(productos, function (i, producto) {
        var fila = `
        <tr data-producto-id="${producto.p_id}" data-combo-id="${comboId}">
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
                ${producto.cantidad.toFixed(2)}
            </td>
            <td class="text-end">
                ${producto.dto_porc.toFixed(2)}
            </td>
            <td class="text-center">
                <span class="badge ${producto.activo == 'A' ? "bg-success" : "bg-danger"}">
                    ${producto.activo == 'A' ? "Activo" : "Pendiente"}
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
 * Actualiza el grid de sustitutos con los datos del producto seleccionado
 * @param {string} productoId - ID del producto para el que se mostrarán los sustitutos
 */
function actualizarGridSustitutos(productoId) {
    // Obtener sustitutos para el producto desde el mapa
    var sustitutos = productosSustitutosMap[productoId] || [];
    
    // Obtener el tbody de la tabla
    var $tbody = $("#tbGridSustitutos tbody");
    
    // Limpiar tabla actual
    $tbody.empty();
    
    if (sustitutos.length === 0) {
        // Mostrar mensaje de "No hay sustitutos"
        $tbody.html(`
            <tr>
                <td colspan="${modoNuevoCombo ? 5 : 4}" class="text-center text-muted py-2">
                    <i class="bx bx-info-circle me-1"></i>No hay sustitutos disponibles
                </td>
            </tr>
        `);
        return;
    }
    
    // Agregar cada sustituto como una nueva fila
    $.each(sustitutos, function(i, sustituto) {
        var fila = `
        <tr data-producto-id="${sustituto.p_id}" data-combo-id="">
            <td class="text-center">
                ${sustituto.p_id}
            </td>
            <td>
                ${sustituto.p_desc}
            </td>
            <td class="text-end">
                ${typeof sustituto.p_pcosto === 'number' ? sustituto.p_pcosto.toFixed(2) : '0.00'}
            </td>
            <td class="text-center">
                <span class="badge bg-success">
                    Activo
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
    
    // Configurar eventos para los botones de eliminar si estamos en modo de edición
    if (modoNuevoCombo) {
        configurarEventosEliminacionSustitutos();
    }
}

/**
 * Carga los productos asociados a un combo
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
        success: function(html) {
            CerrarWaiting();
            
            // Actualizar el contenido del grid de productos
            $(".col-sm-6:has(#tbGridProductos)").html(html);
            
            // Si estamos viendo un combo existente, ocultar la columna de acción
            if (!modoNuevoCombo) {
                $("#tbGridProductos th:last-child, #tbGridProductos td:last-child").hide();
            }
            
            // Configurar eventos de selección en la tabla de productos
            configurarSeleccionProductos();
            
            // Después de cargar productos, cargar los sustitutos del primer producto (si existe)
            setTimeout(function() {
                // Buscar el primer producto en la tabla
                var primerProducto = $("#tbGridProductos tbody tr:first");
                if (primerProducto.length > 0) {
                    var productoId = primerProducto.find("td:first").text().trim();
                    if (productoId) {
                        // Marcar el primer producto como seleccionado
                        primerProducto.addClass("selected-row");
                        
                        // Cargar los sustitutos del primer producto
                        cargarProductosSustitutos(comboId, productoId);
                    }
                }
            }, 100); // Pequeño retraso para asegurar que el DOM se actualice
        },
        error: function(xhr, status, error) {
            CerrarWaiting();
            console.error("Error al cargar productos:", error);
            ControlaMensajeError("Error al cargar productos: " + error);
        }
    });
}

/**
 * Carga los productos sustitutos asociados a un producto dentro de un combo
 */
function cargarProductosSustitutos(comboId, productoId) {
    if (typeof obtenerProductosSustitutosUrl === 'undefined') {
        console.error("URL para obtener productos sustitutos no definida");
        return;
    }
    
    AbrirWaiting("Cargando productos sustitutos...");
    
    // Obtener descripción del producto seleccionado para el mensaje
    var productoDesc = $("#tbGridProductos tbody tr.selected-row td:nth-child(2)").text().trim();
    
    $.ajax({
        url: obtenerProductosSustitutosUrl,
        type: "POST",
        data: { comboId: comboId, productoId: productoId },
        success: function(html) {
            CerrarWaiting();
            
            // Actualizar el contenido del grid de sustitutos
            $(".col-sm-6:has(#tbGridSustitutos)").html(html);
            
            // Si estamos viendo un combo existente, ocultar la columna de acción
            if (!modoNuevoCombo) {
                $("#tbGridSustitutos th:last-child, #tbGridSustitutos td:last-child").hide();
            }
            
            // Verificar si hay sustitutos después de cargar el HTML
            setTimeout(function() {
                var tieneFilasConDatos = $("#tbGridSustitutos tbody tr").length > 0 && 
                                         !$("#tbGridSustitutos tbody tr td").text().includes("No hay sustitutos disponibles");
                
                if (!tieneFilasConDatos) {
                    // Usar ControlaMensajeWarning en lugar de AbrirMensaje
                    ControlaMensajeWarning("El producto \"" + productoDesc + "\" (ID: " + productoId + ") no tiene sustitutos asociados.");
                }
            }, 100); // Pequeño retraso para asegurar que el DOM se actualice
        },
        error: function(xhr, status, error) {
            CerrarWaiting();
            console.error("Error al cargar productos sustitutos:", error);
            ControlaMensajeError("Error al cargar productos sustitutos: " + error);
        }
    });
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
function limpiarSeleccionBusqueda() {
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
    $(".col-sm-6:has(#tbGridProductos)").html(htmlProductosVacio);
    $(".col-sm-6:has(#tbGridSustitutos)").html(htmlSustitutosVacio);
    
    // Habilitar los botones de agregar solo en modo edición
    if (modoEdicion) {
        $("#btnAgregarCProducto, #btnAgregarSustituto").prop("disabled", false);
    }
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

    // Obtener ID del combo actual
    var comboId = $("#cmb_id").val();

    // Agregar cada producto como una nueva fila
    $.each(productos, function (i, producto) {
        var fila = `
        <tr data-producto-id="${producto.p_id}" data-combo-id="${comboId}">
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
                ${producto.cantidad.toFixed(2)}
            </td>
            <td class="text-end">
                ${producto.dto_porc.toFixed(2)}
            </td>
            <td class="text-center">
                <span class="badge ${producto.activo == 'A' ? "bg-success" : "bg-danger"}">
                    ${producto.activo == 'A' ? "Activo" : "Pendiente"}
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
 * Actualiza el grid de sustitutos con los datos del producto seleccionado
 * @param {string} productoId - ID del producto para el que se mostrarán los sustitutos
 */
function actualizarGridSustitutos(productoId) {
    // Obtener sustitutos para el producto desde el mapa
    var sustitutos = productosSustitutosMap[productoId] || [];
    
    // Obtener el tbody de la tabla
    var $tbody = $("#tbGridSustitutos tbody");
    
    // Limpiar tabla actual
    $tbody.empty();
    
    if (sustitutos.length === 0) {
        // Mostrar mensaje de "No hay sustitutos"
        $tbody.html(`
            <tr>
                <td colspan="${modoNuevoCombo ? 5 : 4}" class="text-center text-muted py-2">
                    <i class="bx bx-info-circle me-1"></i>No hay sustitutos disponibles
                </td>
            </tr>
        `);
        return;
    }
    
    // Agregar cada sustituto como una nueva fila
    $.each(sustitutos, function(i, sustituto) {
        var fila = `
        <tr data-producto-id="${sustituto.p_id}" data-combo-id="">
            <td class="text-center">
                ${sustituto.p_id}
            </td>
            <td>
                ${sustituto.p_desc}
            </td>
            <td class="text-end">
                ${typeof sustituto.p_pcosto === 'number' ? sustituto.p_pcosto.toFixed(2) : '0.00'}
            </td>
            <td class="text-center">
                <span class="badge bg-success">
                    Activo
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
    
    // Configurar eventos para los botones de eliminar si estamos en modo de edición
    if (modoNuevoCombo) {
        configurarEventosEliminacionSustitutos();
    }
}

/**
 * Carga los productos asociados a un combo
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
        success: function(html) {
            CerrarWaiting();
            
            // Actualizar el contenido del grid de productos
            $(".col-sm-6:has(#tbGridProductos)").html(html);
            
            // Si estamos viendo un combo existente, ocultar la columna de acción
            if (!modoNuevoCombo) {
                $("#tbGridProductos th:last-child, #tbGridProductos td:last-child").hide();
            }
            
            // Configurar eventos de selección en la tabla de productos
            configurarSeleccionProductos();
            
            // Después de cargar productos, cargar los sustitutos del primer producto (si existe)
            setTimeout(function() {
                // Buscar el primer producto en la tabla
                var primerProducto = $("#tbGridProductos tbody tr:first");
                if (primerProducto.length > 0) {
                    var productoId = primerProducto.find("td:first").text().trim();
                    if (productoId) {
                        // Marcar el primer producto como seleccionado
                        primerProducto.addClass("selected-row");
                        
                        // Cargar los sustitutos del primer producto
                        cargarProductosSustitutos(comboId, productoId);
                    }
                }
            }, 100); // Pequeño retraso para asegurar que el DOM se actualice
        },
        error: function(xhr, status, error) {
            CerrarWaiting();
            console.error("Error al cargar productos:", error);
            ControlaMensajeError("Error al cargar productos: " + error);
        }
    });
}

/**
 * Carga los productos sustitutos asociados a un producto dentro de un combo
 */
function cargarProductosSustitutos(comboId, productoId) {
    if (typeof obtenerProductosSustitutosUrl === 'undefined') {
        console.error("URL para obtener productos sustitutos no definida");
        return;
    }
    
    AbrirWaiting("Cargando productos sustitutos...");
    
    // Obtener descripción del producto seleccionado para el mensaje
    var productoDesc = $("#tbGridProductos tbody tr.selected-row td:nth-child(2)").text().trim();
    
    $.ajax({
        url: obtenerProductosSustitutosUrl,
        type: "POST",
        data: { comboId: comboId, productoId: productoId },
        success: function(html) {
            CerrarWaiting();
            
            // Actualizar el contenido del grid de sustitutos
            $(".col-sm-6:has(#tbGridSustitutos)").html(html);
            
            // Si estamos viendo un combo existente, ocultar la columna de acción
            if (!modoNuevoCombo) {
                $("#tbGridSustitutos th:last-child, #tbGridSustitutos td:last-child").hide();
            }
            
            // Verificar si hay sustitutos después de cargar el HTML
            setTimeout(function() {
                var tieneFilasConDatos = $("#tbGridSustitutos tbody tr").length > 0 && 
                                         !$("#tbGridSustitutos tbody tr td").text().includes("No hay sustitutos disponibles");
                
                if (!tieneFilasConDatos) {
                    // Usar ControlaMensajeWarning en lugar de AbrirMensaje
                    ControlaMensajeWarning("El producto \"" + productoDesc + "\" (ID: " + productoId + ") no tiene sustitutos asociados.");
                }
            }, 100); // Pequeño retraso para asegurar que el DOM se actualice
        },
        error: function(xhr, status, error) {
            CerrarWaiting();
            console.error("Error al cargar productos sustitutos:", error);
            ControlaMensajeError("Error al cargar productos sustitutos: " + error);
        }
    });
}

/**
 * Guarda la relación producto-sustituto en el servidor
 */
function guardarRelacionProductoSustitutoEnServidor(productoId) {
    // Extraer solo los IDs de los sustitutos
    var sustitutosIds = productosSustitutosMap[productoId].map(s => s.p_id);
    
    // Enviar al servidor
    $.ajax({
        url: resguardarRelacionProductoSustitutoUrl,
        type: "POST",
        data: {
            p_id: productoId,
            p_id_sus: sustitutosIds
        },
        success: function(response) {
            if (response && response.ok) {
                console.log("Relación producto-sustituto guardada en servidor correctamente");
            } else {
                ControlaMensajeWarning("No se pudo guardar la relación producto-sustituto en el servidor");
                console.warn("Error al guardar relación:", response);
            }
        },
        error: function(xhr, status, error) {
            console.error("Error al guardar relación producto-sustituto:", error);
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
    $("#tbGridProductos tbody tr").each(function() {
        // Verificar que no sea la fila de "No hay productos"
        if (!$(this).find("td[colspan]").length) {
            var productoId = $(this).data("producto-id") || $(this).find("td:first").text().trim();
            if (productoId) {
                productosIds.push(productoId.toString());
            }
        }
    });
    
    return productosIds;
}

/**
 * Obtiene los IDs de productos sustitutos que ya están en el grid
 * @returns {Array} Array con IDs de productos sustitutos existentes
 */
function obtenerSustitutosExistentesIds() {
    var sustitutosIds = [];
    
    // Recorrer todas las filas del grid de sustitutos que no sean la fila "No hay sustitutos"
    $("#tbGridSustitutos tbody tr").each(function() {
        // Verificar que no sea la fila de "No hay sustitutos"
        if (!$(this).find("td[colspan]").length) {
            var sustitutoId = $(this).data("producto-id") || $(this).find("td:first").text().trim();
            if (sustitutoId) {
                sustitutosIds.push(sustitutoId.toString());
            }
        }
    });
    
    return sustitutosIds;
}

/**
 * Guarda el mapa de sustitutos en sesión
 */
function guardarSustitutosEnSesion() {
    try {
        sessionStorage.setItem('productosSustitutosMap', JSON.stringify(productosSustitutosMap));
    } catch (e) {
        console.error("Error al guardar sustitutos en sesión:", e);
    }
}

/**
 * Cancela la operación currente y restaura el estado inicial
 */
function cancelarOperacion(e) {
    // Ocultar formulario
    $("#divComboDatos").hide();

    //Desactivamos el modo edición de la alta
    modoNuevoCombo = false;

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

    

    // Si existe un homeCombo y necesitamos redirigir
    if (e && $("#btnCancel").is(e.target) && typeof homeCombo !== 'undefined') {
        window.location.href = homeCombo;
    }
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
        success: function(html) {
            CerrarWaiting();
            $("#divCanales").html(html);
        },
        error: function(xhr, status, error) {
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
        success: function(response) {
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
                    
                    // Deshabilitar el checkbox si está activo o si pasa_activar es true
                    $("#chkEstadoCombo").prop("disabled", esActivo || datos.pasa_activar);
                    
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
        error: function(xhr, status, error) {
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
        success: function(html) {
            CerrarWaiting();
            $("#divCanales").html(html);
            
            // Modificar la tabla para mostrar checkboxes y ocultar columna de estado
            adaptarGrillaCanales();
        },
        error: function(xhr, status, error) {
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
    $("#tbGridCanales th:nth-child(4), #tbGridCanales td:nth-child(4)").hide();
    
    // Añadir columna de selección en el encabezado
    $("#tbGridCanales thead tr").prepend("<th class='text-center'>Selección</th>");
    
    // Añadir checkbox a cada fila
    $("#tbGridCanales tbody tr").each(function() {
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
    $(".canal-checkbox").on("change", function() {
        var checked = $(this).prop("checked");
        // Se podría implementar lógica adicional aquí
        console.log("Canal " + $(this).val() + " " + (checked ? "seleccionado" : "deseleccionado"));
    });
}

/**
 * Cancela la operación currente y restaura el estado inicial
 */
function cancelarOperacion(e) {
    // Ocultar formulario
    $("#divComboDatos").hide();
    
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
    
    // Si existe un homeCombo y necesitamos redirigir
    if (e && $("#btnCancel").is(e.target) && typeof homeCombo !== 'undefined') {
        window.location.href = homeCombo;
    }
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
        success: function(html) {
            CerrarWaiting();
            $("#divCanales").html(html);
        },
        error: function(xhr, status, error) {
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
        success: function(response) {
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
                    
                    // Deshabilitar el checkbox si está activo o si pasa_activar es true
                    $("#chkEstadoCombo").prop("disabled", esActivo || datos.pasa_activar);
                    
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
        error: function(xhr, status, error) {
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
        success: function(html) {
            CerrarWaiting();
            $("#divCanales").html(html);
            
            // Modificar la tabla para mostrar checkboxes y ocultar columna de estado
            adaptarGrillaCanales();
        },
        error: function(xhr, status, error) {
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
    $("#tbGridCanales th:nth-child(4), #tbGridCanales td:nth-child(4)").hide();
    
    // Añadir columna de selección en el encabezado
    $("#tbGridCanales thead tr").prepend("<th class='text-center'>Selección</th>");
    
    // Añadir checkbox a cada fila
    $("#tbGridCanales tbody tr").each(function() {
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
    $(".canal-checkbox").on("change", function() {
        var checked = $(this).prop("checked");
        // Se podría implementar lógica adicional aquí
        console.log("Canal " + $(this).val() + " " + (checked ? "seleccionado" : "deseleccionado"));
    });
}