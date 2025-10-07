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

/**
 * Inicializa los eventos para los elementos del formulario
 */
function inicializarEventos() {
    // Configurar el evento click para el botón Cancelar/Inicializar
    $("#btnCancel, #btnAbmCancelar").on("click", function() {
        cancelarOperacion();
    });
    
    // Configurar el evento click para el botón Buscar/Filtrar
    $("#btnBuscar").on("click", function() {
        buscarCombos();
    });
    
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
        inicializarNuevoCombo();
        
        // Activar/desactivar botones
        $("#btnAbmAceptar").prop("disabled", false);
        $("#btnAbmNuevo").prop("disabled", true);
    });
    
    // Evento para el botón confirmar
    $("#btnAbmAceptar").on("click", function() {
        // Aquí iría el código para guardar el nuevo combo
        // Por ahora solo logueamos la acción
        console.log("Confirmar nuevo combo/promo");
    });
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
    
    // Realizar la búsqueda
    $.ajax({
        url: presentarPromosYCombosUrl,
        type: "POST",
        data: filtros,
        success: function(html) {
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
        error: function(xhr, status, error) {
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
    $(".pagination .page-link").off("click").on("click", function(e) {
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
 * Configura los eventos para la selección de filas en la tabla
 */
function configurarEventosSeleccion() {
    // Aplicar estilo de cursor a todas las filas de la tabla
    $("#tbGridPromoCombo tbody tr").css("cursor", "pointer");
    
    // Remover eventos previos para evitar duplicación
    $(document).off("click", "#tbGridPromoCombo tbody tr");
    
    // Configurar evento de click para seleccionar filas (comportamiento de selección única)
    $(document).on("click", "#tbGridPromoCombo tbody tr", function(e) {
        if (!$(e.target).is("button, a, .btn, i")) {
            var $this = $(this);
            var wasSelected = $this.hasClass("selected-row");
            
            // Eliminar la selección de todas las filas
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
                } else {
                    console.error("No se encontró el ID del combo en la fila seleccionada");
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

/**
 * Carga una página específica with los filtros dados
 */
function cargarPagina(filtros, pagina) {
    if (typeof presentarPromosYCombosUrl === 'undefined') {
        console.error("URL para presentar promos y combos no definida");
        return;
    }
    
    AbrirWaiting("Cargando página " + pagina + "...");
    
    $.ajax({
        url: presentarPromosYCombosUrl,
        type: "POST",
        data: filtros,
        success: function(html) {
            CerrarWaiting();
            $("#divDetalle").html(html);
            configurarEventosPaginacion();
            configurarEventosSeleccion();
        },
        error: function(xhr, status, error) {
            CerrarWaiting();
            console.error("Error al cargar página: ", error);
            ControlaMensajeError("Error al cargar página: " + error);
        }
    });
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
    
    // Cargar canales disponibles
    cargarCanalesParaNuevoCombo();
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
 * Cancela la operación actual y restaura el estado inicial
 */
function cancelarOperacion() {
    // Ocultar formulario
    $("#divComboDatos").hide();
    
    // Restaurar estado de los campos
    restaurarCamposFormulario();
    
    // Limpiar grid de canales
    $("#divCanales").empty();
    
    // Restaurar estado de los botones
    $("#btnAbmNuevo").prop("disabled", false);
    $("#btnAbmAceptar").prop("disabled", true);
    
    // Si existe un homeCombo y necesitamos redirigir
    if ($("#btnCancel").is(e.target) && typeof homeCombo !== 'undefined') {
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