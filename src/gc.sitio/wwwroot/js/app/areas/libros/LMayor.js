// Variables de control global
let filaClicDoble = null; // Fila seleccionada con doble clic (detalle)
let filasSeleccionadas = []; // Filas seleccionadas para operaciones adicionales
let saldoAnterior = 0; // Saldo anterior para mostrar en reportes
let saldoActual = 0; // Saldo actual para mostrar en reportes
let mayorDataBak = {}; // Respaldo de datos de búsqueda previa

// Constantes para identificar tipos de reportes
const TipoReporte = {
    MAYOR: 1,
    MAYOR_DIARIO: 2,
    DIARIO: 3
};

// Variable para controlar el tipo de reporte actual
let tipoReporteActual = TipoReporte.MAYOR;

// Al cargar el documento
$(function () {

    

    // Inicializar controles y eventos
    inicializarComponentes();

    configurarBotones();

    // Configurar eventos para controles de filtro
    configurarFiltros();

    // Configurar árbol de cuentas contables
    configurarArbolCuentas();    

    // Configurar eventos de tablas
    configurarEventosTablas();

    // Inicializar vista
    inicializarVista();

    // Aplicar estilos al inicio
    setTimeout(aplicarEstilosSelector, 500);
});

function analizaEstadoBtnDetalleLMayor() {
    if ($("#divDetalle").is(":visible")) {
        // Hay un asiento abierto, limpiarlo y cerrar el panel
        limpiarLibroMayor();
    }
}

function limpiarLibroMayor() {
    // Vaciar el panel que contiene el asiento
    $("#divResultados").empty();

    // Restablecer variables de control
    filaClicDoble = null;
    EntidadSelect = "";
    EntidadEstado = "";

    inicializarVista();
}

/**
 * Configura los botones de acción
 */
function configurarBotones() {

    // Botón de cancelar
    $("#btnCancel").on("click", function () {
        window.location.href = homeLMayorUrl;
    });

    // Usar mousedown en lugar de click para evitar conflictos con el collapse
    $("#btnDetalle").on("mousedown", analizaEstadoBtnDetalleLMayor);
    $("#btnFiltro").on("mousedown", function () {
        if ($("#divFiltro").is(":hidden")) {
            $("#divDetalle").collapse("hide");
        }
    });

    // Botón de imprimir
    $("#btnImprimir").on("click", function () {
        imprimirReporte();
    });

    // Botones de cambio de tab
    $('.nav-tabs .nav-link').on('click', function () {
        // Obtener el id de la pestaña seleccionada
        const tabId = $(this).attr('id');

        // Actualizar tipo de reporte según la tab seleccionada
        switch (tabId) {
            case 'tabMayor':
                tipoReporteActual = TipoReporte.MAYOR;
                break;
            case 'tabMayorDiario':
                tipoReporteActual = TipoReporte.MAYOR_DIARIO;
                // Cargar los datos de Mayor por Día si aún no se han cargado
                if ($("#divLMAcum").text().includes("SIN REGISTROS") && Object.keys(mayorDataBak).length > 0) {
                    buscarMayorDiario();
                }
                break;
            case 'tabDiario':
                tipoReporteActual = TipoReporte.DIARIO;
                // Cargar los datos de Libro Diario si aún no se han cargado
                if ($("#divAsientosCta").text().includes("SIN REGISTROS") && Object.keys(mayorDataBak).length > 0) {
                    buscarLibroDiario();
                }
                break;
        }
    });


    // Botón de búsqueda
    $("#btnBuscar").on("click", function () {
        // Validar campos obligatorios
        if (!validarCamposObligatorios()) {
            return;
        }

        // Es una nueva búsqueda, no resguardamos la búsqueda anterior
        mayorDataBak = {};

        // Limpiar todos los contenedores de resultados según la pestaña activa
        const tabActiva = $('.nav-tabs .active').attr('id');

        // Es una búsqueda por filtro, siempre será página 1
        pagina = 1;

        switch (tabActiva) {
            case 'tabMayor':
                tipoReporteActual = TipoReporte.MAYOR;
                buscarLibroMayor();
                break;
            case 'tabMayorDiario':
                tipoReporteActual = TipoReporte.MAYOR_DIARIO;
                buscarMayorDiario();
                break;
            case 'tabDiario':
                tipoReporteActual = TipoReporte.DIARIO;
                buscarLibroDiario();
                break;
            default:
                tipoReporteActual = TipoReporte.MAYOR;
                buscarLibroMayor();
        }
    });


    // Botón de cancelar
    $("#btnCancel").on("click", function () {
        window.location.href = homeLMayorUrl;
    });

    // Botón de imprimir
    $("#btnImprimir").on("click", function () {
        imprimirReporte();
    });

    // Botones de cambio de tab
    $('.nav-tabs .nav-link').on('click', function () {
       
        // Actualizar tipo de reporte según la tab seleccionada
        const tabId = $(this).attr('id');

        switch (tabId) {
            case 'tabMayor':
                tipoReporteActual = TipoReporte.MAYOR;
                break;
            case 'tabMayorDiario':
                tipoReporteActual = TipoReporte.MAYOR_DIARIO;
                break;
            case 'tabDiario':
                tipoReporteActual = TipoReporte.DIARIO;
                break;
        }
    });
}

/**
 * Busca y muestra el Mayor Diario (acumulado por día)
 */
function buscarMayorDiario() {
    AbrirWaiting("Consultando Mayor por día...");

    // Desactivamos los botones de acción
    $("#btnImprimir").prop("disabled", true);

    // Ocultamos filtro inmediatamente
    $("#divFiltro").collapse("hide");
    $("#divDetalle").collapse("show");

    // Realizar petición GET a la acción LMAcumuladoXDia
    $.ajax({
        url: obtenerMayorDiarioUrl,
        type: "GET",
        success: function (obj) {
            // Mostramos el resultado en el div correspondiente de Mayor Diario
            $("#divLMAcum").html(obj);

            // Agregar después de $("#divLMAcum").html(obj); en buscarMayorDiario
            setTimeout(function () {
                console.log("Comprobando tabla en #divLMAcum:");
                console.log("Tabla encontrada:", $("#divLMAcum table").length > 0);
                console.log("ID de la tabla:", $("#divLMAcum table").attr("id") || "No tiene ID");
                console.log("Filas en la tabla:", $("#divLMAcum table tbody tr").length);

                // Agregar ID si no lo tiene
                if ($("#divLMAcum table").length > 0 && !$("#divLMAcum table").attr("id")) {
                    $("#divLMAcum table").attr("id", "tbMayorDiario");
                    console.log("ID 'tbMayorDiario' añadido a la tabla");
                }

                // Verificar que las filas tengan el atributo data-fecha
                $("#divLMAcum table tbody tr").each(function (index) {
                    const tieneDataFecha = $(this).attr("data-fecha") !== undefined;
                    const contenidoPrimeraCelda = $(this).find("td:first").text().trim();
                    console.log(`Fila ${index}: data-fecha=${tieneDataFecha}, primera celda='${contenidoPrimeraCelda}'`);
                });
            }, 500);


            // Limpiamos el detalle hasta que se seleccione un día
            $("#divLMAcumDet").html(`
                <div class="alert alert-info">
                    <i class="bx bx-info-circle me-1"></i>
                    Seleccione una fecha para ver el detalle del día.
                </div>
            `);

            // Habilitamos botón de impresión
            $("#btnImprimir").prop("disabled", false);

            // Reinicializar eventos
            reinicializarEventosMayorDiario();

            CerrarWaiting();
        },
        error: function (xhr, status, error) {
            ControlaMensajeError("Error al consultar Mayor por día: " + error);
            $("#divFiltro").collapse("show");
            CerrarWaiting();
        }
    });
}


function reinicializarEventosMayorDiario() {
    // Remover eventos antiguos (opcional pero recomendable)
    $(document).off("dblclick", "#divLMAcum table tbody tr");

    // Volver a registrar el evento de doble clic
    $(document).on("dblclick", "#divLMAcum table tbody tr", function (e) {
        e.stopPropagation();

        // [resto del código del evento]
    });
}
/**
 * Carga el detalle de un día específico desde el servidor
 * @param {string} fecha - Fecha en formato DD/MM/YYYY
 */
function cargarDetalleDia(fecha) {
    if (!fecha) {
        $("#divLMAcumDet").html(`
            <div class="alert alert-warning">
                <i class="bx bx-error-circle me-1"></i>
                Fecha no especificada.
            </div>
        `);
        return;
    }

    // Mostrar indicador de carga
    $("#divLMAcumDet").html(`
        <div class="text-center p-3">
            <div class="spinner-border text-primary" role="status">
                <span class="visually-hidden">Cargando...</span>
            </div>
            <p class="mt-2">Cargando detalle del día ${fecha}...</p>
        </div>
    `);

    // Realizar petición GET
    $.ajax({
        url: obtenerDetalleDiarioUrl,
        type: "GET",
        data: { fecha: fecha },
        success: function (obj) {
            $("#divLMAcumDet").html(obj);
        },
        error: function (xhr, status, error) {
            $("#divLMAcumDet").html(`
                <div class="alert alert-danger">
                    <i class="bx bx-error-circle me-1"></i>
                    Error al cargar el detalle: ${error}
                </div>
            `);
        }
    });
}



// Actualizar o agregar los eventos necesarios en configurarEventosTablas()
// Añadir al método configurarEventosTablas()
function configurarEventosTablas() {
    // [Código existente]

    // Evento de clic simple para selección visual en la tabla Mayor Diario
    $(document).on("click", "#tbMayorDiario tbody tr", function (e) {
        e.stopPropagation();

        // Ignorar filas de saldo anterior y totales
        if ($(this).hasClass("saldo-anterior") ||
            $(this).hasClass("saldo-actual") ||
            $(this).hasClass("table-info")) {
            return;
        }

        // Remover selección simple previa
        $("#tbMayorDiario tbody tr").removeClass("selected-simple");

        // Aplicar selección simple a esta fila
        $(this).addClass("selected-simple");
    });

    // Evento para los botones de ver detalle
    $(document).on("click", "#divLMAcum .ver-detalle", function (e) {
        e.stopPropagation();

        // Obtener la fecha del botón
        const fecha = $(this).data("fecha");

        // Aplicar estilo a la fila
        $("#divLMAcum table tbody tr").removeClass("selectedEdit-row");
        $(this).closest("tr").addClass("selectedEdit-row");

        // Cargar detalle del día
        if (fecha) {
            cargarDetalleDia(fecha);
        }
    });

    // Modifica el evento de doble clic para #tbMayorDiario 
    $(document).on("dblclick", "#divLMAcum table tbody tr", function (e) {
        e.stopPropagation();

        console.log("Doble clic detectado en fila de tabla");

        // Ignorar filas de saldo anterior y totales
        if ($(this).hasClass("saldo-anterior") ||
            $(this).hasClass("saldo-actual") ||
            $(this).hasClass("table-info")) {
            console.log("Fila ignorada por ser saldo o total");
            return;
        }

        // Remover selección previa
        $("#divLMAcum table tbody tr").removeClass("selectedEdit-row");

        // Aplicar estilo "bordo" (selectedEdit-row) a esta fila
        $(this).addClass("selectedEdit-row");

        // Intenta obtener la fecha del atributo data-fecha
        let fecha = $(this).attr("data-fecha");

        // Si no existe, intenta obtenerla de la primera celda
        if (!fecha) {
            fecha = $(this).find("td:first").text().trim();
            console.log("Fecha obtenida de la celda:", fecha);
        }

        console.log("Fecha para cargar detalle:", fecha);

        // Cargar detalle del día
        if (fecha) {
            cargarDetalleDia(fecha);
        } else {
            console.error("No se pudo determinar la fecha de la fila");
        }
    });


    // Evento de doble clic en filas del detalle para ver asiento completo
    $(document).on("dblclick", "#tbDetalleDia tbody tr", function (e) {
        e.stopPropagation();

        // Obtener el ID del asiento
        const asientoId = $(this).attr("data-dia-movi");

        if (asientoId) {
            abrirDetalleAsiento(asientoId);
        }
    });
}



// Agregar esta función en inicializarComponentes() o similar
function aplicarEstilosSelector() {
    // Asegurar que el modal tenga los estilos correctos
    $("#modalCuentas .modal-header").addClass("bg-primary text-white");
    $("#modalCuentas .btn-close").addClass("btn-close-white");
    $("#modalCuentas .modal-content").addClass("border-0 shadow");
    $("#modalCuentas .modal-footer").addClass("border-top bg-light");

    // Mejorar aspecto de los botones
    $("#btnBuscarCuentaModal").addClass("btn-outline-primary");
    $("#btnBuscarCuentaModal i").addClass("me-1");

    // Añadir íconos a botones si no los tienen
    if (!$("#btnSeleccionarCuenta").find("i").length) {
        $("#btnSeleccionarCuenta").prepend('<i class="bx bx-check me-1"></i>');
    }

    if (!$("#modalCuentas button[data-bs-dismiss='modal']:not(#btnSeleccionarCuenta)").find("i").length) {
        $("#modalCuentas button[data-bs-dismiss='modal']:not(#btnSeleccionarCuenta)").prepend('<i class="bx bx-x me-1"></i>');
    }

    // Asegurar que los controles tengan el tamaño correcto
    $("#cuentasTree").addClass("border rounded p-2");
    $("#modalCuentas .input-group").addClass("input-group-sm");
    $("#modalCuentas .btn").addClass("btn-sm");
}

/**
 * Inicializa los componentes básicos de la interfaz
 */
function inicializarComponentes() {
    // Agregar estilos CSS para diferencias visuales
    $("<style>")
            .prop("type", "text/css")
            .html(`
            /* Estilo para selección simple en Mayor Diario */
            #tbMayorDiario tbody tr.selected-simple {
                background-color: #f0f0f0 !important;
            }
        
            /* Estilo para selección con doble clic (estilo bordo) */
            #tbMayorDiario tbody tr.selectedEdit-row {
                background-color: firebrick !important;
                color: white !important;
                font-weight: bold;
            }
        
            /* Estilos para saldo anterior y actual */
            .saldo-anterior, .saldo-actual {
                background-color: #f8f9fa !important;
                font-weight: bold;
            }
        
            .saldo-actual {
                background-color: #e9ecef !important;
            }
        
            /* Hacer que las tablas tengan filas con cursor pointer */
            #tbMayorDiario tbody tr, 
            #tbDetalleDia tbody tr {
                cursor: pointer;
            }
        `)
        .appendTo("head");


    // Asegurarse de que los estilos se apliquen después de abrir el modal
    $("#modalCuentas").on("shown.bs.modal", function () {
        aplicarEstilosSelector();
    });
    // Inicializar selectores con Select2 si está disponible
    if ($.fn.select2) {
        $("#Eje_nro").select2({
            placeholder: "Seleccione ejercicio",
            width: '100%'
        });
    }

    // Configurar datepickers para fechas
    $('.datepicker').datepicker({
        format: 'dd/mm/yyyy',
        autoclose: true,
        language: 'es',
        todayHighlight: true
    });
}

/**
 * Carga los ejercicios contables disponibles
 */
function cargarEjercicios() {
    AbrirWaiting("Cargando ejercicios contables...");

    $.ajax({
        url: obtenerEjerciciosUrl,
        type: "GET",
        contentType: "application/json",
        success: function (response) {
            // Limpiar selector de ejercicios
            $("#Eje_nro").empty();

            // Agregar opción por defecto
            //$("#Eje_nro").append('<option value="">Seleccione ejercicio</option>');

            // Agregar opciones con los ejercicios recibidos
            $.each(response, function (index, item) {
                $("#Eje_nro").append(`<option value="${item.eje_nro}">${item.eje_lista}</option>`);
            });

            CerrarWaiting();
        },
        error: function (xhr, status, error) {
            CerrarWaiting();
            ControlaMensajeError("Error al cargar ejercicios: " + error);
        }
    });
}

/**
 * Configura los filtros del formulario
 */
function configurarFiltros() {
    // Forzar selección y deshabilitación del checkbox de ejercicio
    $('#chkEjercicio').prop('checked', true).prop('disabled', true);
    $('#Eje_nro').prop('disabled', false);

    // Forzar selección y deshabilitación del checkbox de cuenta contable
    $('#chkCuenta').prop('checked', true).prop('disabled', true);

    // Prevenir clics en checkboxes que deben estar siempre habilitados
    $('#chkEjercicio, #chkCuenta').on('click', function (e) {
        e.preventDefault();
        return false;
    });

    // Evento de cambio para ejercicio contable
    $('#Eje_nro').on('change', function () {
        const ejercicioId = $(this).val();
        if (!ejercicioId) return;
    });

    // Evento para checkbox de incluir asientos temporales
    $('#chkIncluirTemp').on('change', function () {
        const isChecked = $(this).prop('checked');
        $('#incluirTemporales').val(isChecked ? "true" : "false");
    });

    // Evento para checkbox de rango de fechas
    $('#Rango').on('change', function () {
        const isChecked = $(this).prop('checked');
        $('input[name="Desde"], input[name="Hasta"]').prop('disabled', !isChecked);
    });

    // Inicializar estados de los campos según checkboxes
    toggleComponent('Rango', 'input[name="Desde"]');
    toggleComponent('Rango', 'input[name="Hasta"]');
}

// Dentro de la función configurarArbolCuentas(), reemplazar por:
function configurarArbolCuentas() {
    // Evento de búsqueda dentro del modal
    $("#cuentaModalFilter").on("keyup", function () {
        const searchString = $(this).val();

        if ($.fn.jstree && $('#cuentasTree').jstree(true)) {
            $('#cuentasTree').jstree('search', searchString);
        }
    });

    // Botón para abrir el selector de cuentas (simplificar selector)
    $("[data-bs-target='#modalCuentas']").on("click", function () {
        // Verificar que haya un ejercicio seleccionado
        const ejercicioId = $("#Eje_nro").val();

        if (!ejercicioId) {
            AbrirMensaje("ATENCIÓN",
                "Debe seleccionar un ejercicio contable antes de elegir una cuenta.",
                function () { $("#msjModal").modal("hide"); },
                false, ["Aceptar"], "warn!", null);
            return;
        }

        // Abrir el modal
        $("#modalCuentas").modal("show");

        // Cargar el árbol de cuentas (sin parámetros)
        cargarArbolCuentasLMayor();
    });

    // Botón para seleccionar cuenta
    $("#btnSeleccionarCuenta").on("click", function () {
        // Obtener nodo seleccionado
        const selectedNode = $('#cuentasTree').jstree('get_selected', true);

        if (selectedNode.length > 0) {
            const node = selectedNode[0];

            // Verificar que sea una cuenta de movimiento (M)
            if (node.data && node.data.tipo === "M") {
                // Guardar ID y descripción de la cuenta seleccionada
                $("#cuentaId").val(node.id);
                $("#cuentaDesc").val(node.text);

                // Actualizar el campo visible para el usuario
                $("#cuentaFilter").val(node.id + " - " + node.text);

                // Actualizar texto en la vista
                $("#cuentaSeleccionadaTexto").text(node.text);
                $("#cuentaSeleccionada").removeClass("bg-light").addClass("bg-success bg-opacity-10");

                // Activar botón de búsqueda si hay ejercicio y cuenta seleccionada
                const ejercicioSeleccionado = $("#Eje_nro").val() !== "";
                $("#btnBuscar").prop("disabled", !(ejercicioSeleccionado && node.id));
            } else {
                // Mostrar mensaje si no es cuenta de movimiento
                AbrirMensaje("Aviso", "Solo se pueden seleccionar cuentas de movimiento.",
                    function () { $("#msjModal").modal("hide"); },
                    false, ["Aceptar"], "info!", null);
            }
        }
    });

}


// Actualizar la función cargarArbolCuentas() para manejar tanto la carga inicial como búsquedas específicas
function cargarArbolCuentasLMayor() {
    AbrirWaiting("Cargando plan de cuentas...");

    // Destruir árbol existente si hay uno
    if ($.fn.jstree && $('#cuentasTree').jstree(true)) {
        $('#cuentasTree').jstree('destroy');
    }

    const data = {
        buscar: "",
        buscaNew: true
    };

    // Usar PostGen para mantener consistencia
    PostGen(data, buscarPlanCuentasUrl, function (obj) {
        CerrarWaiting();

        if (obj.error === true) {
            ControlaMensajeError(obj.msg || "Error al cargar el plan de cuentas.");
            return;
        }

        try {
            // Parsear el árbol desde la respuesta JSON
            let jsonP = $.parseJSON(obj.arbol);

            // Procesamiento del árbol para asignar íconos y clases
            procesarNodosArbol(jsonP);

            // Inicializar jsTree
            $('#cuentasTree').jstree({
                'core': {
                    'data': jsonP,
                    'themes': { 'responsive': true }
                },
                'types': {
                    "activo": { "icon": "bx bx-wallet" },
                    "pasivo": { "icon": "bx bx-trending-down" },
                    "patrimonio": { "icon": "bx bx-building-house" },
                    "ingresos": { "icon": "bx bx-dollar-circle" },
                    "egresos": { "icon": "bx bx-print-dollar" },
                    "default": { "icon": "bx bx-folder" }
                },
                'search': {
                    'show_only_matches': true,
                    'show_only_matches_children': true
                },
                'plugins': ['search', 'types']
            });

            // Contraer todo el árbol al iniciar
            $('#cuentasTree').on('ready.jstree', function () {
                $('#cuentasTree').jstree('close_all');
            });

        } catch (error) {
            ControlaMensajeError("Error al procesar el árbol de cuentas: " + error);
        }
    });
}


/**
 * Procesa los nodos del árbol para asignarles clases e íconos
 * @param {Array} nodos - Lista de nodos del árbol
 */
function procesarNodosArbol(nodos) {
    nodos.forEach(nodo => {
        const tipo = nodo.data?.tipo;
        const cuenta = nodo.data?.cuenta?.toLowerCase();

        // Asignar tipo para íconos
        nodo.type = cuenta || "default";

        // Asignar clases CSS
        nodo.a_attr = nodo.a_attr || {};
        let clases = [];

        if (tipo === "M") clases.push("tipo-m");
        if (cuenta) clases.push("cuenta-" + cuenta);

        nodo.a_attr.class = clases.join(" ");

        // Procesar hijos recursivamente
        if (nodo.children && nodo.children.length > 0) {
            procesarNodosArbol(nodo.children);
        }
    });
}



/**
 * Configura eventos de las tablas de resultados
 */
function configurarEventosTablas() {
    // Evento de clic en filas de la tabla Mayor
    $(document).on("click", "#tbMayor tbody tr", function (e) {
        e.stopPropagation();

        // Remover selección de todas las filas
        $("#tbMayor tbody tr").removeClass("selected-simple");

        // Seleccionar esta fila
        $(this).addClass("selected-simple");

        // Guardar referencia a la fila seleccionada
        filaClicDoble = $(this);
    });

    // Evento de clic en filas de la tabla Mayor Diario (resumen por día)
    $(document).on("click", "#tbMayorDiario tbody tr", function (e) {
        e.stopPropagation();

        // Remover selección de todas las filas
        $("#tbMayorDiario tbody tr").removeClass("selected-simple");

        // Seleccionar esta fila
        $(this).addClass("selected-simple");

        // Obtener la fecha del día seleccionado
        const fecha = $(this).find("td:first-child").text().trim();

        // Cargar detalle del día seleccionado
        cargarDetalleDia(fecha);
    });

    // Evento de doble clic en filas del Libro Diario para ver el asiento completo
    $(document).on("dblclick", "#tbDiario tbody tr", function (e) {
        e.stopPropagation();

        // Obtener el ID del asiento (dia_movi)
        const asientoId = $(this).attr("data-dia-movi");

        // Si es un encabezado de asiento (tiene dia_movi)
        if (asientoId) {
            // Abrir detalle del asiento
            abrirDetalleAsiento(asientoId);
        }
    });
}

/**
 * Inicializa la vista del módulo
 */
function inicializarVista() {
    // Deshabilitar botón de impresión hasta tener resultados
    $("#btnImprimir").prop("disabled", true);

    // MODIFICAR: Simplificar el manejo de paneles - mostrar filtro y ocultar resultados directamente
    $("#divFiltro").collapse("show");
    $("#divDetalle").collapse("hide");

    // ELIMINAR: Todo el código que maneja el evento hide.bs.collapse
    // ELIMINAR: window.explicitFilterClose y todo lo relacionado

    // MANTENER: Configuración de funciones de callback para paginación
    funcCallBack = buscarLibroMayor;

    // MANTENER: Evento de cambio para paginación
    $("#pagEstado").on("change", function () {
        const div = $("#divPaginacion");
        presentaPaginacion(div);
    });
}

/**
 * Valida los campos obligatorios antes de realizar una búsqueda
 * @returns {boolean} True si todos los campos obligatorios están completos
 */
function validarCamposObligatorios() {
    let camposFaltantes = [];

    // Validar ejercicio (siempre obligatorio)
    if (!$("#Eje_nro").val()) {
        camposFaltantes.push("Ejercicio contable");
    }

    // Validar cuenta contable (siempre obligatoria)
    if (!$("#cuentaId").val()) {
        camposFaltantes.push("Cuenta contable");
    }

    // Validar rango de fechas si está habilitado
    if ($("#Rango").is(":checked")) {
        const fechaDesde = $("input[name='Desde']").val();
        const fechaHasta = $("input[name='Hasta']").val();

        if (!fechaDesde) {
            camposFaltantes.push("Fecha desde");
        }

        if (!fechaHasta) {
            camposFaltantes.push("Fecha hasta");
        }
    }

    // Si faltan campos, mostrar mensaje y devolver false
    if (camposFaltantes.length > 0) {
        AbrirMensaje(
            "ATENCIÓN",
            "Para realizar la búsqueda debe seleccionar valores para: " + camposFaltantes.join(", "),
            function () {
                $("#msjModal").modal("hide");
                return true;
            },
            false,
            ["Aceptar"],
            "warn!",
            null
        );
        return false;
    }

    return true;
}




/**
 * Busca y muestra el Libro Mayor
 * @param {number} pag - Número de página a buscar (default: 1)
 */
function buscarLibroMayor(pag = 1) {
    AbrirWaiting("Consultando Libro Mayor...");

    // Desactivamos los botones de acción
    $("#btnImprimir").prop("disabled", true);

    // SIMPLIFICADO: Ocultamos filtro inmediatamente
    $("#divFiltro").collapse("hide");

    // Obtenemos los valores de los campos del filtro
    var data1 = obtenerParametrosBusqueda();

    // Guardamos parámetros para el reporte
    cargarReporteEnArre(11, data1, "Libro Mayor", "Cuenta: " + data1.ccb_desc, "");

    // Verificar si cambió la búsqueda o solo la página
    var buscaNew = JSON.stringify(mayorDataBak) != JSON.stringify(data1);

    if (buscaNew === false) {
        // Son iguales las condiciones, solo cambia de página
        pagina = pag;
    } else {
        // Cambiaron las condiciones, vuelve a página 1
        mayorDataBak = JSON.parse(JSON.stringify(data1));
        pagina = 1;
        pag = 1;
    }

    // Agregamos parámetros adicionales
    var data2 = {
        pagina: pagina,
        buscaNew: buscaNew
    };

    // Combinamos todos los parámetros
    var data = $.extend({}, data1, data2);

    // Realizamos la petición al servidor
    PostGenHtml(data, obtenerLibroMayorUrl, function (obj) {
        // Mostramos el resultado en el div correspondiente
        $("#divResultados").html(obj);

        // SIMPLIFICADO: Mostramos resultados directamente
        $("#divDetalle").collapse("show");

        // Obtenemos metadata adicional (paginación, saldos)
        PostGen(data, buscarMetadataURL, function (obj) {
            if (obj.error === true) {
                ControlaMensajeError(obj.msg);
            } else {
                // Guardamos valores para reportes
                saldoAnterior = obj.metadata.saldoAnterior || 0;
                saldoActual = obj.metadata.saldoActual || 0;

                // Configuramos paginación
                totalRegs = obj.metadata.totalCount;
                pags = obj.metadata.totalPages;
                pagRegs = obj.metadata.pageSize;

                // Activamos paginación
                $("#pagEstado").val(true).trigger("change");

                // Habilitamos botón de impresión
                $("#btnImprimir").prop("disabled", false);
            }
        });

        CerrarWaiting();
    }, function (obj) {
        // En caso de error, mostrar mensaje y restablecer el filtro
        ControlaMensajeError(obj.message);
        $("#divFiltro").collapse("show");
        CerrarWaiting();
    });
}


/**
 * Obtiene los parámetros para la búsqueda
 * @returns {Object} Objeto con los parámetros para la búsqueda
 */
function obtenerParametrosBusqueda() {
    return {
        eje_nro: $("#Eje_nro").val(),
        ccb_id: $("#cuentaId").val(),
        ccb_desc: $("#cuentaDesc").val(),
        incluirTemporales: $("#chkIncluirTemp").is(":checked"),
        rango: $("#Rango").is(":checked"),
        desde: $("#Rango").is(":checked") ? $("input[name='Desde']").val() : null,
        hasta: $("#Rango").is(":checked") ? $("input[name='Hasta']").val() : null
    };
}

/**
 * Obtiene los IDs de asientos del Libro Mayor
 * @returns {string} String con IDs separados por comas
 */
function obtenerMovimientosSeleccionados() {
    // Si tenemos filas seleccionadas, usar esos IDs
    if (filasSeleccionadas.length > 0) {
        return filasSeleccionadas.join(',');
    }

    // Si no hay selección específica, obtener todos los movimientos de la tabla Mayor
    const movimientos = [];

    $("#tbMayor tbody tr").each(function () {
        const dia_movi = $(this).attr("data-dia-movi");
        if (dia_movi && !movimientos.includes(dia_movi)) {
            movimientos.push(dia_movi);
        }
    });

    return movimientos.join(',');
}

/**
 * Renderiza los resultados del Libro Mayor
 * @param {Array} datos - Datos del Libro Mayor
 */
function renderizarLibroMayor(datos) {
    // Construir la tabla del Libro Mayor
    let html = `
        <div class="table-responsive">
            <table id="tbMayor" class="table table-striped table-hover">
                <thead>
                    <tr>
                        <th>Fecha</th>
                        <th>Nº Asiento</th>
                        <th>Concepto</th>
                        <th class="text-end">Debe</th>
                        <th class="text-end">Haber</th>
                        <th class="text-end">Saldo</th>
                    </tr>
                </thead>
                <tbody>
    `;

    // Fila para el saldo anterior
    html += `
        <tr class="saldo-anterior">
            <td colspan="3">Saldo Anterior</td>
            <td class="text-end"></td>
            <td class="text-end"></td>
            <td class="text-end">${formatearImporte(saldoAnterior)}</td>
        </tr>
    `;

    // Filas de datos
    if (datos.length === 0) {
        html += `<tr><td colspan="6" class="text-center">No se encontraron registros</td></tr>`;
    } else {
        datos.forEach(item => {
            html += `
                <tr data-dia-movi="${item.dia_movi || ''}">
                    <td>${formatearFecha(item.dia_fecha)}</td>
                    <td>${item.dia_movi || ''}</td>
                    <td>${item.dia_desc || ''}</td>
                    <td class="text-end">${formatearImporte(item.debe)}</td>
                    <td class="text-end">${formatearImporte(item.haber)}</td>
                    <td class="text-end">${formatearImporte(item.saldo)}</td>
                </tr>
            `;
        });
    }

    // Fila para el saldo actual
    html += `
        <tr class="saldo-actual">
            <td colspan="3">Saldo Actual</td>
            <td class="text-end"></td>
            <td class="text-end"></td>
            <td class="text-end">${formatearImporte(saldoActual)}</td>
        </tr>
    `;

    html += `
                </tbody>
            </table>
        </div>
    `;

    // Mostrar resultados
    $("#divResultados").html(html);

    // Actualizar estado de paginación si hay metadata disponible
    if (response && response.metadata) {
        totalRegs = response.metadata.totalCount;
        pags = response.metadata.totalPages;
        pagRegs = response.metadata.pageSize;

        // Activar la paginación
        $("#pagEstado").val(true).trigger("change");
    }
}

/**
 * Renderiza los resultados del Mayor Diario (resumen por día)
 * @param {Array} datos - Datos del Mayor Diario
 */
function renderizarMayorDiario(datos) {
    // Agrupar datos por fecha
    const datosPorFecha = agruparPorFecha(datos);

    // Construir la tabla de resumen por día
    let html = `
        <div class="row">
            <div class="col-md-6">
                <div class="table-responsive">
                    <table id="tbMayorDiario" class="table table-striped table-hover">
                        <thead>
                            <tr>
                                <th>Fecha</th>
                                <th class="text-end">Debe</th>
                                <th class="text-end">Haber</th>
                                <th class="text-end">Saldo</th>
                            </tr>
                        </thead>
                        <tbody>
                            <tr class="saldo-anterior">
                                <td>Saldo Anterior</td>
                                <td class="text-end"></td>
                                <td class="text-end"></td>
                                <td class="text-end">${formatearImporte(saldoAnterior)}</td>
                            </tr>
    `;

    // Filas de resumen por día
    if (Object.keys(datosPorFecha).length === 0) {
        html += `<tr><td colspan="4" class="text-center">No se encontraron registros</td></tr>`;
    } else {
        Object.keys(datosPorFecha).forEach(fecha => {
            const resumen = datosPorFecha[fecha].resumen;
            html += `
                <tr data-fecha="${fecha}">
                    <td>${fecha}</td>
                    <td class="text-end">${formatearImporte(resumen.debe)}</td>
                    <td class="text-end">${formatearImporte(resumen.haber)}</td>
                    <td class="text-end">${formatearImporte(resumen.saldo)}</td>
                </tr>
            `;
        });
    }

    html += `
                            <tr class="saldo-actual">
                                <td>Saldo Actual</td>
                                <td class="text-end"></td>
                                <td class="text-end"></td>
                                <td class="text-end">${formatearImporte(saldoActual)}</td>
                            </tr>
                        </tbody>
                    </table>
                </div>
            </div>
            <div class="col-md-6">
                <div id="detalleDia">
                    <div class="alert alert-info">
                        Seleccione una fecha para ver el detalle del día
                    </div>
                </div>
            </div>
        </div>
    `;

    // Mostrar resultados
    $("#divResultados").html(html);

    // Guardar datos agrupados para uso posterior
    $("#divResultados").data("datosPorFecha", datosPorFecha);
}

/**
 * Agrupa los datos del Mayor por fecha
 * @param {Array} datos - Datos del Mayor
 * @returns {Object} Datos agrupados por fecha
 */
function agruparPorFecha(datos) {
    const resultado = {};

    datos.forEach(item => {
        const fecha = formatearFecha(item.dia_fecha);

        if (!resultado[fecha]) {
            resultado[fecha] = {
                resumen: {
                    debe: 0,
                    haber: 0,
                    saldo: 0
                },
                detalle: []
            };
        }

        // Sumar al resumen del día
        resultado[fecha].resumen.debe += item.debe || 0;
        resultado[fecha].resumen.haber += item.haber || 0;
        resultado[fecha].resumen.saldo = item.saldo || 0; // El último saldo del día

        // Agregar al detalle
        resultado[fecha].detalle.push(item);
    });

    return resultado;
}

/**
 * Carga el detalle de un día específico
 * @param {string} fecha - Fecha en formato DD/MM/YYYY
 */
function cargarDetalleDia(fecha) {
    // Obtener datos agrupados por fecha
    const datosPorFecha = $("#divResultados").data("datosPorFecha");

    if (!datosPorFecha || !datosPorFecha[fecha]) {
        $("#detalleDia").html(`
            <div class="alert alert-warning">
                No hay datos disponibles para la fecha ${fecha}
            </div>
        `);
        return;
    }

    // Construir tabla de detalle
    let html = `
        <h5>Detalle del día ${fecha}</h5>
        <div class="table-responsive">
            <table id="tbDetalleDia" class="table table-striped table-hover">
                <thead>
                    <tr>
                        <th>Nº Asiento</th>
                        <th>Concepto</th>
                        <th class="text-end">Debe</th>
                        <th class="text-end">Haber</th>
                        <th class="text-end">Saldo</th>
                    </tr>
                </thead>
                <tbody>
    `;

    // Filas de detalle
    datosPorFecha[fecha].detalle.forEach(item => {
        html += `
            <tr data-dia-movi="${item.dia_movi || ''}">
                <td>${item.dia_movi || ''}</td>
                <td>${item.dia_desc || ''}</td>
                <td class="text-end">${formatearImporte(item.debe)}</td>
                <td class="text-end">${formatearImporte(item.haber)}</td>
                <td class="text-end">${formatearImporte(item.saldo)}</td>
            </tr>
        `;
    });

    html += `
                </tbody>
            </table>
        </div>
    `;

    // Mostrar detalle
    $("#detalleDia").html(html);
}

/**
 * Renderiza los resultados del Libro Diario
 * @param {Array} datos - Datos del Libro Diario
 */
function renderizarLibroDiario(datos) {
    // Agrupar datos por asiento (dia_movi)
    const datosPorAsiento = agruparPorAsiento(datos);

    // Construir la tabla del Libro Diario
    let html = `
        <div class="table-responsive">
            <table id="tbDiario" class="table table-striped table-hover">
                <thead>
                    <tr>
                        <th>Fecha</th>
                        <th>Nº Asiento</th>
                        <th>Cuenta/Concepto</th>
                        <th class="text-end">Debe</th>
                        <th class="text-end">Haber</th>
                    </tr>
                </thead>
                <tbody>
    `;

    // Filas de datos
    if (Object.keys(datosPorAsiento).length === 0) {
        html += `<tr><td colspan="5" class="text-center">No se encontraron registros</td></tr>`;
    } else {
        Object.keys(datosPorAsiento).forEach(asientoId => {
            const asiento = datosPorAsiento[asientoId];

            // Encabezado del asiento
            html += `
                <tr class="header-asiento" data-dia-movi="${asientoId}">
                    <td>${formatearFecha(asiento.fecha)}</td>
                    <td>${asientoId}</td>
                    <td>${asiento.descripcion}</td>
                    <td class="text-end"></td>
                    <td class="text-end"></td>
                </tr>
            `;

            // Detalle del asiento
            asiento.detalle.forEach(linea => {
                html += `
                    <tr class="detalle-asiento">
                        <td></td>
                        <td></td>
                        <td>${linea.ccb_desc || ''}</td>
                        <td class="text-end">${formatearImporte(linea.debe)}</td>
                        <td class="text-end">${formatearImporte(linea.haber)}</td>
                    </tr>
                `;
            });

            // Línea separadora entre asientos
            html += `<tr><td colspan="5" class="border-bottom"></td></tr>`;
        });
    }

    html += `
                </tbody>
            </table>
        </div>
    `;

    // Mostrar resultados
    $("#divResultados").html(html);
}

/**
 * Agrupa los datos del Diario por asiento
 * @param {Array} datos - Datos del Diario
 * @returns {Object} Datos agrupados por asiento
 */
function agruparPorAsiento(datos) {
    const resultado = {};

    datos.forEach(item => {
        const asientoId = item.dia_movi;

        if (!resultado[asientoId]) {
            resultado[asientoId] = {
                fecha: item.dia_fecha,
                descripcion: item.dia_desc_asiento || '',
                detalle: []
            };
        }

        // Agregar línea al detalle del asiento
        resultado[asientoId].detalle.push({
            ccb_id: item.ccb_id,
            ccb_desc: item.ccb_desc,
            dia_desc: item.dia_desc,
            debe: item.debe,
            haber: item.haber
        });
    });

    return resultado;
}

/**
 * Abre el detalle de un asiento específico
 * @param {string} asientoId - ID del asiento (dia_movi)
 */
function abrirDetalleAsiento(asientoId) {
    AbrirWaiting("Cargando detalle del asiento...");

    $.ajax({
        url: buscarAsientoDefUrl,
        type: "GET",
        data: { id: asientoId },
        success: function (html) {
            // Mostrar modal con el detalle del asiento
            $("#modalDetalleAsiento .modal-body").html(html);
            $("#modalDetalleAsiento").modal("show");

            CerrarWaiting();
        },
        error: function (xhr, status, error) {
            CerrarWaiting();
            ControlaMensajeError("Error al cargar detalle del asiento: " + error);
        }
    });
}

/**
 * Imprime el reporte actual
 */
function imprimirReporte() {
    // Determinar parámetros según el tipo de reporte actual
    let modulo, parametros, titulo, observacion;

    // Obtener los parámetros base del formulario
    const params = obtenerParametrosBusqueda();

    switch (tipoReporteActual) {
        case TipoReporte.MAYOR:
            modulo = "LibroMayor";
            parametros = {
                eje_nro: params.eje_nro,
                ccb_id: params.ccb_id,
                ccb_desc: params.ccb_desc,
                desde: params.desde,
                hasta: params.hasta,
                incluirTemporales: params.incluirTemporales,
                saldoAnterior: saldoAnterior,
                saldoActual: saldoActual
            };
            titulo = "Libro Mayor";
            observacion = `Cuenta: ${params.ccb_desc}`;
            break;

        case TipoReporte.MAYOR_DIARIO:
            modulo = "MayorDiario";
            parametros = {
                eje_nro: params.eje_nro,
                ccb_id: params.ccb_id,
                ccb_desc: params.ccb_desc,
                desde: params.desde,
                hasta: params.hasta,
                incluirTemporales: params.incluirTemporales,
                saldoAnterior: saldoAnterior,
                saldoActual: saldoActual
            };
            titulo = "Mayor Diario";
            observacion = `Cuenta: ${params.ccb_desc}`;
            break;

        case TipoReporte.DIARIO:
            modulo = "LibroDiario";
            parametros = {
                eje_nro: params.eje_nro,
                desde: params.desde,
                hasta: params.hasta,
                incluirTemporales: params.incluirTemporales,
                movimientos: obtenerMovimientosSeleccionados()
            };
            titulo = "Libro Diario";
            observacion = "Movimientos seleccionados";
            break;
    }

    // Preparar datos para el gestor de impresión
    const data = {
        modulo: modulo,
        parametros: parametros,
        titulo: titulo,
        observacion: observacion
    };

    // Invocar gestor documental
    invocacionGestorDoc(data);
}

/**
 * Formatea un importe para mostrar en la interfaz
 * @param {number} importe - Importe a formatear
 * @returns {string} Importe formateado
 */
function formatearImporte(importe) {
    if (importe === null || importe === undefined) return '';

    // Convertir a número si es string
    const valor = typeof importe === 'string' ? parseFloat(importe) : importe;

    // Formatear con separador de miles y decimales
    return valor.toLocaleString('es-ES', {
        minimumFractionDigits: 2,
        maximumFractionDigits: 2
    });
}

/**
 * Formatea una fecha para mostrar en la interfaz
 * @param {string} fecha - Fecha en formato ISO o similar
 * @returns {string} Fecha formateada como DD/MM/YYYY
 */
function formatearFecha(fecha) {
    if (!fecha) return '';

    try {
        const date = new Date(fecha);
        return date.toLocaleDateString('es-ES', {
            day: '2-digit',
            month: '2-digit',
            year: 'numeric'
        });
    } catch (e) {
        console.error('Error al formatear fecha:', e);
        return fecha;
    }
}

/**
 * Función genérica para habilitar/deshabilitar componentes
 * @param {string} checkboxId - ID del checkbox
 * @param {string} componentSelector - Selector del componente a controlar
 */
function toggleComponent(checkboxId, componentSelector) {
    try {
        const isChecked = $(`#${checkboxId}`).is(':checked');
        const $component = $(componentSelector);

        if (isChecked) {
            $component.prop('disabled', false).css({
                'background-color': '',
                'font-weight': 'normal'
            });
        } else {
            $component.prop('disabled', true).css({
                'background-color': 'rgb(251,255,195)',
                'font-weight': '900'
            });
        }
    } catch (error) {
        console.error(`Error al procesar el checkbox ${checkboxId}:`, error);
    }
}

// Función para pruebas de depuración
function probarDetalleManual(fecha) {
    console.log("Probando carga de detalle para fecha:", fecha);
    cargarDetalleDia(fecha);
}

// Agregar un botón de prueba al contenedor principal
$(function () {
    setTimeout(function () {
        if ($("#testButton").length === 0) {
            $("#divLMAcumDet").append(`
                <div class="mt-3">
                    <button id="testButton" class="btn btn-sm btn-info">
                        Probar Detalle (última fecha)
                    </button>
                </div>
            `);

            // Evento para probar manualmente
            $("#testButton").on("click", function () {
                const ultimaFila = $("#tbMayorDiario tbody tr").not(".saldo-anterior, .saldo-actual, .table-info").last();
                const fecha = ultimaFila.attr("data-fecha");
                probarDetalleManual(fecha);
            });
        }
    }, 2000);
});
