$(function () {
    inicializaComponentesAsientoAjuste();
    configurarBotonesAsientoAjuste();


    // Manejar el check/uncheck de todos los elementos - MODIFICADO
    $(document).on('click', '#checkAllAAJ', function () {
        var isChecked = $(this).prop('checked');
        $('.asiento-check').prop('checked', isChecked);

        // No llamamos a actualizarEstadoAjusta() aquí
        // Solo se encarga de marcar/desmarcar todos los checkboxes
    });

    // Manejar el check/uncheck individual - MODIFICADO
    $(document).on('click', '.asiento-check', function (e) {
        e.stopPropagation(); // Evitar que el click en el checkbox active la fila

        // Simplemente actualizamos el estado del checkbox general
        var totalChecks = $('.asiento-check').length;
        var totalChecked = $('.asiento-check:checked').length;
        $('#checkAllAAJ').prop('checked', totalChecks === totalChecked);

        // No llamamos a actualizarEstadoAjusta() aquí
        // Solo gestionamos el estado del checkbox
    });

    // Botón de confirmar ajustes
    $(document).on('click', '#btnConfirmarAjuste', function () {
        confirmarAjustes();
    });

    // Botón de cancelar ajustes
    $(document).on('click', '#btnCancelarAjuste', function () {
        cancelarAjustes();
    });

    // Comportamiento de click en filas - MODIFICADO
    $(document).on('click', '#tbAAJ tr', function (e) {
        // Si la tabla está desactivada o hay una carga en progreso, no hacer nada
        if ($("#divAaj").hasClass("tabla-desactivada")) {
            return;
        }

        // Evitar que se active si se hizo click directamente en el checkbox
        if (!$(e.target).is('.asiento-check') && !$(e.target).is('input[type="checkbox"]')) {
            // Encontrar el checkbox dentro de la fila
            var checkbox = $(this).find('.asiento-check');

            // Desmarcar todas las filas
            $("#tbAAJ tr").removeClass("selected-row");

            // Marcar esta fila como seleccionada
            $(this).addClass("selected-row");

            // Ahora sí, llamamos a actualizarEstadoAjusta
            actualizarEstadoAjusta();
        }
    });

   
});

/**
 * Función para bloquear o desbloquear la tabla de asientos
 * @param {boolean} bloquear - true para bloquear, false para desbloquear
 */
function bloquearTablaAsientos(bloquear) {
    if (bloquear) {
        // Añadir overlay y clase de desactivado
        $("#divAaj").addClass("tabla-desactivada");
        if ($("#overlay-tabla").length === 0) {
            $("#divAaj").append('<div id="overlay-tabla" class="overlay-tabla"></div>');
        }
        // Deshabilitar eventos de clic en las filas
        $("#tbAAJ tr").css("pointer-events", "none");
    } else {
        // Quitar overlay y clase de desactivado
        $("#divAaj").removeClass("tabla-desactivada");
        $("#overlay-tabla").remove();
        // Rehabilitar eventos de clic en las filas
        $("#tbAAJ tr").css("pointer-events", "auto");
    }
}

// Función para actualizar el estado Ajusta y cargar los detalles - SIN CAMBIOS
function actualizarEstadoAjusta() {
    // Obtener todos los checkboxes marcados
    var checkedBoxes = $('.asiento-check:checked');

    // Si hay al menos un checkbox marcado, buscar los detalles
    if (checkedBoxes.length > 0) {
        // Tomamos el primer checkbox marcado para mostrar sus detalles
        var selectedCcbId = $(checkedBoxes[0]).data('ccb-id');
        var ejercicio = $("#Eje_nro").val();

        // Bloquear la tabla mientras se carga el detalle
        bloquearTablaAsientos(true);

        // Limpiamos el div de detalle antes de cargar nuevos datos
        $("#divAajDet").empty();

        // Mostramos indicador de carga
        $("#divAajDet").html('<div class="text-center"><i class="bx bx-loader bx-spin text-primary" style="font-size: 2rem;"></i><p>Cargando detalles...</p></div>');

        // Invocamos al controlador para obtener los detalles
        var data = {
            eje_nro: ejercicio,
            ccb_id: selectedCcbId
        };

        // Realizar la petición AJAX
        PostGenHtml(data, buscarRegistrosAsAjCcbUrl, function (html) {
            // Actualizar el div con la respuesta
            $("#divAajDet").html(html);

            // Inicializar componentes que pudieran necesitar reinicialización
            $('.datepicker').datepicker({
                format: 'dd/mm/yyyy',
                autoclose: true,
                language: 'es',
                todayHighlight: true,
                orientation: 'bottom'
            });

            // Desbloquear la tabla después de cargar el detalle
            bloquearTablaAsientos(false);
        }, function (error) {
            // Manejo de errores
            $("#divAajDet").html('<div class="alert alert-danger">Error al cargar los detalles: ' + error.message + '</div>');

            // Desbloquear la tabla en caso de error
            bloquearTablaAsientos(false);
        });
    } else {
        // Si no hay checkboxes seleccionados, limpiar el área de detalle
        $("#divAajDet").empty();
        $("#divAajDet").html('<div class="alert alert-info">Seleccione una cuenta para ver sus detalles.</div>');
    }
}

function inicializaComponentesAsientoAjuste() {

    // Inicializar tooltips
    $('[data-bs-toggle="tooltip"]').tooltip();


    // Inicializar selectores con Select2 si está disponible
    if ($.fn.select2) {
        $("#Eje_nro").select2({
            placeholder: "Seleccione ejercicio",
            width: '100%'
        });
    }

    // Configurar datepickers para fechas con localización explícita
    $.fn.datepicker.dates['es'] = {
        days: ["Domingo", "Lunes", "Martes", "Miércoles", "Jueves", "Viernes", "Sábado"],
        daysShort: ["Dom", "Lun", "Mar", "Mié", "Jue", "Vie", "Sáb"],
        daysMin: ["Do", "Lu", "Ma", "Mi", "Ju", "Vi", "Sa"],
        months: ["Enero", "Febrero", "Marzo", "Abril", "Mayo", "Junio", "Julio", "Agosto", "Septiembre", "Octubre", "Noviembre", "Diciembre"],
        monthsShort: ["Ene", "Feb", "Mar", "Abr", "May", "Jun", "Jul", "Ago", "Sep", "Oct", "Nov", "Dic"],
        today: "Hoy",
        clear: "Borrar",
        format: "dd/mm/yyyy",
        titleFormat: "MM yyyy",
        weekStart: 1
    };

    $('.datepicker').datepicker({
        format: 'dd/mm/yyyy',
        autoclose: true,
        language: 'es',
        todayHighlight: true,
        orientation: 'bottom'
    });

    // Forzar la localización regional para las fechas
    $.datepicker.setDefaults($.datepicker.regional["es"]);   


    // Inicializar el selector de cuentas
    inicializarSelectorCuentas();
}

function configurarBotonesAsientoAjuste() {
    // Botón de cancelar
    $("#btnCancel").on("click", function () {
        window.location.href = homeBSSUrl;
    });

    // Botón de búsqueda
    $("#btnBuscar").on("click", function () {
        
        // Es una nueva búsqueda, no resguardamos la búsqueda anterior
        mayorDataBak = {};

        // Es una búsqueda por filtro, siempre será página 1
        pagina = 1;

        // Realizar búsqueda
        buscaraaj();
    });

    // Usar mousedown en lugar de click para evitar conflictos con el collapse
    $("#btnDetalle").on("mousedown", analizaEstadoBtnDetalleaaj);
    $("#btnFiltro").on("mousedown", function () {
        if ($("#divFiltro").is(":hidden")) {
            $("#divDetalle").collapse("hide");
        }
    });

    // Botón de imprimir
    $(document).on("click", ".btnImprimir", function () {
        imprimiraaj();
    });
}

function analizaEstadoBtnDetalleaaj() {
    if ($("#divDetalle").is(":visible")) {
        // HAY QUE LIMPIAR 
        bssDataBak = {};
        limpiarAaj();
    }
}

function buscaraaj() {
    AbrirWaiting("Consultando los asientos de ajuste por inflación. Espere, puede tardar. Sea paciente...");

    // Desactivamos los botones de acción
    $("#btnImprimir").prop("disabled", true);

    // SIMPLIFICADO: Ocultamos filtro inmediatamente
    $("#divFiltro").collapse("hide");

    // Obtenemos el ejercicio seleccionado
    var eje_nro = $("#Eje_nro").val();

    // Validar que se haya seleccionado un ejercicio
    if (!eje_nro) {
        CerrarWaiting();
        AbrirMensaje(
            "Validación",
            "Debe seleccionar un ejercicio contable.",
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

    let data = { eje_nro };

    // Guardamos parámetros para el reporte
    cargarReporteEnArre(21, data, "Asientos de Ajuste por Inflación", "", administracion);

    limpiarAaj();

    // Realizamos la petición al servidor
    PostGenHtml(data, buscarAsientosAjusteUrl, function (obj) {       
        // Mostramos el resultado en el div correspondiente
        $("#divAaj").html(obj);
      
        // Mostramos resultados directamente
        $("#divDetalle").collapse("show");

        // Habilitamos botón de impresión
        $("#btnImprimir").prop("disabled", false);

        CerrarWaiting();
    }, function (obj) {
        // En caso de error, mostrar mensaje y restablecer el filtro
        ControlaMensajeError(obj.message);
        $("#divFiltro").collapse("show");
        CerrarWaiting();
    });
}

// Función auxiliar para limpiar el área de resultados
function limpiarAaj() {
    $("#divAaj").empty();
    $("#divAajDet").empty(); // Limpiar también el área de detalle
    $("#btnFilter").collapse("show");
}

// Función para obtener los parámetros de búsqueda
function obtenerParametrosBusquedaaaj() {
    return {
        eje_nro: $("#Eje_nro").val(),
        // Otros parámetros de búsqueda que se requieran
    };
}

function imprimiraaj() {
    // Invocar gestor documental
    invocacionGestorDoc({});
}

// También modificamos la función selectReg para mantener consistencia
function selectReg(elem, tableId) {
    // Si se hizo click en una fila de la tabla principal de asientos
    if (tableId === 'tbAAJ') {
        // Desmarcar todas las filas
        $("#" + tableId + " tr").removeClass("selected-row");

        // Marcar esta fila como seleccionada
        $(elem).addClass("selected-row");
       
        // Actualizar el estado y cargar detalles
        actualizarEstadoAjusta();
    }
}

// Variables globales para el selector de cuentas
let cuentaSeleccionada = null;
let arbolCuentasInicializado = false;

/**
 * Modifica el selector de cuentas para implementar la búsqueda en tiempo real
 */
function inicializarSelectorCuentas() {
    // Configurar evento para abrir el selector al hacer clic en el botón
    $('#btnBuscarCuenta').off('click').on('click', function () {
        // Guardar referencias para los campos destino
        $('#selectorPlanCuentasModal').data('campo-destino', 'cuentaAjuste');
        $('#selectorPlanCuentasModal').data('campo-destino-id', 'cuentaAjusteId');

        // Abrir el modal
        $('#selectorPlanCuentasModal').modal('show');

        // Cargar el árbol si no está inicializado
        if (!arbolCuentasInicializado) {
            cargarArbolCuentas();
        }
    });

    // NUEVA IMPLEMENTACIÓN: Búsqueda en tiempo real al escribir
    $('#txtBuscarCuentaPlan').off('keyup').on('keyup', function () {
        const termino = $(this).val().trim();

        // Obtener instancia del árbol
        const tree = $("#cuentasTree").jstree(true);
        if (!tree) return;

        if (termino.length > 0) {
            // Si hay texto, realizar la búsqueda
            tree.search(termino, false, true);

            // Usar setTimeout para dar tiempo a jsTree a actualizar el DOM
            setTimeout(function () {
                // Contar los resultados usando jQuery
                const nodosEncontrados = $('.jstree-search');
                const totalResultados = nodosEncontrados.length;

                // Expandir los nodos padre de los resultados
                nodosEncontrados.each(function () {
                    const nodeId = $(this).closest('.jstree-node').attr('id');
                    if (nodeId) {
                        // Obtener y expandir todos los nodos padres
                        let parent = tree.get_parent(nodeId);
                        while (parent && parent !== "#") {
                            tree.open_node(parent);
                            parent = tree.get_parent(parent);
                        }
                    }
                });

                // Mostrar mensaje con cantidad de resultados
                if (totalResultados > 0) {
                    $("#resultadosBusqueda").html(`
                    <div class="alert alert-success py-1 small">
                        <i class="bx bx-check-circle me-1"></i>
                        Se encontraron <strong>${totalResultados}</strong> cuenta(s) que coinciden
                    </div>
                `).show();
                } else {
                    $("#resultadosBusqueda").html(`
                    <div class="alert alert-warning py-1 small">
                        <i class="bx bx-error-circle me-1"></i>
                        No se encontraron cuentas que coincidan
                    </div>
                `).show();
                }

                // Ocultar después de 3 segundos
                setTimeout(function () {
                    $("#resultadosBusqueda").fadeOut();
                }, 3000);
            }, 200); // Pequeño retraso para que jsTree termine de actualizar el DOM
        } else {
            // Si el campo está vacío, limpiar la búsqueda
            tree.clear_search();
            tree.close_all();
            $("#resultadosBusqueda").fadeOut();
        }
    });


    // Búsqueda al presionar Enter (para evitar envío de formulario)
    $('#txtBuscarCuentaPlan').off('keypress').on('keypress', function (e) {
        if (e.which === 13) {
            e.preventDefault(); // Evitar envío de formulario
            // La búsqueda ya se habrá hecho con el evento keyup
        }
    });

    // Evento para seleccionar cuenta
    $('#btnSeleccionarCuenta').off('click').on('click', function () {
        if (cuentaSeleccionada) {
            // Obtener los campos destino desde el modal
            const campoDestino = $('#selectorPlanCuentasModal').data('campo-destino');
            const campoDestinoId = $('#selectorPlanCuentasModal').data('campo-destino-id');

            // Actualizar los campos con la cuenta seleccionada
            $('#' + campoDestino).val(cuentaSeleccionada.text);
            $('#' + campoDestinoId).val(cuentaSeleccionada.id);

            // Cerrar el modal
            $('#selectorPlanCuentasModal').modal('hide');
        }
    });

    // Limpiar búsqueda y selección al abrir el modal
    $('#selectorPlanCuentasModal').off('shown.bs.modal').on('shown.bs.modal', function () {
        // Limpiar campo de búsqueda y darle el foco
        $('#txtBuscarCuentaPlan').val('').trigger("focus");

        // Limpiar búsqueda previa
        const tree = $("#cuentasTree").jstree(true);
        if (tree) {
            tree.clear_search();
            tree.close_all();
        }

        // Resetear selección
        cuentaSeleccionada = null;
        $('#btnSeleccionarCuenta').prop('disabled', true);
        $("#resultadosBusqueda").hide();
    });

    // Limpiar búsqueda y selección al cerrar el modal
    $('#selectorPlanCuentasModal').off('hidden.bs.modal').on('hidden.bs.modal', function () {
        $('#txtBuscarCuentaPlan').val('');
        cuentaSeleccionada = null;
        $('#btnSeleccionarCuenta').prop('disabled', true);

        // Devolver el foco al botón que abrió el modal (para accesibilidad)
        $('#btnBuscarCuenta').trigger("focus");
    });
}



/**
 * Carga el árbol de cuentas desde el servidor
 */
function cargarArbolCuentas() {
    // Mostrar indicador de carga en el árbol
    $("#cuentasTree").html(`
        <div class="text-center p-3">
            <div class="spinner-border spinner-border-sm text-warning" role="status">
                <span class="visually-hidden">Cargando...</span>
            </div>
            <p class="mt-2 small">Cargando plan de cuentas...</p>
        </div>
    `);

    AbrirWaiting("Cargando plan de cuentas...");

    const data = {
        buscar: "",
        buscaNew: true
    };

    // Verificar que la URL esté configurada
    if (!buscarPlanCuentasUrl) {
        console.error("La URL para buscar el plan de cuentas no está configurada");
        AbrirMensaje(
            "Error",
            "No se pudo cargar el plan de cuentas. La URL no está configurada.",
            function () { $("#msjModal").modal("hide"); },
            false,
            ["Aceptar"],
            "error!",
            null
        );
        CerrarWaiting();
        return;
    }

    // Realizar la petición AJAX
    $.ajax({
        url: buscarPlanCuentasUrl,
        type: "POST",
        contentType: "application/json",
        data: JSON.stringify(data),
        success: function (resultado) {
            CerrarWaiting();

            if (resultado.error) {
                console.error("Error al cargar el plan de cuentas:", resultado.msg);
                AbrirMensaje(
                    "Error",
                    "Error al cargar el plan de cuentas: " + resultado.msg,
                    function () { $("#msjModal").modal("hide"); },
                    false,
                    ["Aceptar"],
                    "error!",
                    null
                );
                return;
            }

            try {
                // Parsear el árbol
                const arbolCuentas = JSON.parse(resultado.arbol);

                // Procesar los nodos para añadir íconos y clases
                procesarNodosArbol(arbolCuentas);

                // Inicializar jsTree
                inicializarJsTree(arbolCuentas);

                arbolCuentasInicializado = true;
            } catch (error) {
                console.error("Error al procesar los datos del plan de cuentas:", error);
                AbrirMensaje(
                    "Error",
                    "Error al procesar los datos del plan de cuentas",
                    function () { $("#msjModal").modal("hide"); },
                    false,
                    ["Aceptar"],
                    "error!",
                    null
                );
            }
        },
        error: function (xhr, status, error) {
            CerrarWaiting();
            console.error("Error al cargar el plan de cuentas:", error);
            AbrirMensaje(
                "Error",
                "Error de comunicación al cargar el plan de cuentas",
                function () { $("#msjModal").modal("hide"); },
                false,
                ["Aceptar"],
                "error!",
                null
            );
        }
    });
}

/**
 * Procesa los nodos del árbol para añadir íconos y clases
 * @param {Array} nodos - Lista de nodos del árbol
 */
function procesarNodosArbol(nodos) {
    nodos.forEach(nodo => {
        // Determinar tipo de cuenta para el ícono
        const tipo = nodo.data?.tipo;
        const cuentaTipo = nodo.data?.cuenta?.toLowerCase();

        // Asignar tipo para íconos
        nodo.type = cuentaTipo || "default";

        // Asignar clases CSS
        nodo.a_attr = nodo.a_attr || {};
        let clases = [];

        if (tipo === "M") clases.push("tipo-movimiento");
        if (cuentaTipo) clases.push("cuenta-" + cuentaTipo);

        nodo.a_attr.class = clases.join(" ");

        // Procesar nodos hijos recursivamente
        if (nodo.children && nodo.children.length > 0) {
            procesarNodosArbol(nodo.children);
        }
    });
}

/**
 * Inicializa el árbol jsTree con los datos procesados y configura la búsqueda
 * @param {Array} datos - Datos del árbol
 */
function inicializarJsTree(datos) {
    // Destruir instancia previa si existe
    if ($.jstree.reference("#cuentasTree")) {
        $("#cuentasTree").jstree("destroy");
    }

    // Inicializar nueva instancia con soporte para búsqueda
    $("#cuentasTree").jstree({
        core: {
            data: datos,
            themes: {
                responsive: true
            }
        },
        types: {
            activo: {
                icon: "bx bx-wallet"
            },
            pasivo: {
                icon: "bx bx-trending-down"
            },
            patrimonio: {
                icon: "bx bx-building-house"
            },
            ingresos: {
                icon: "bx bx-dollar-circle"
            },
            egresos: {
                icon: "bx bx-money-withdraw"
            },
            default: {
                icon: "bx bx-folder"
            }
        },
        search: {
            show_only_matches: true,
            show_only_matches_children: true,
            close_opened_onclear: true,
            search_leaves_only: false
        },
        plugins: ["types", "search"]
    });

    // Evento al seleccionar un nodo
    $("#cuentasTree").off('select_node.jstree').on("select_node.jstree", function (e, data) {
        const nodo = data.node;
        const nodoId = nodo.id;
        const nodoTexto = nodo.text;
        const nodoTipo = nodo.data?.tipo;

        // Solo permitir seleccionar cuentas de movimiento
        if (nodoTipo === "M") {
            // Guardar la cuenta seleccionada
            cuentaSeleccionada = {
                id: nodoId,
                text: nodoTexto
            };

            // Habilitar el botón de seleccionar
            $('#btnSeleccionarCuenta').prop('disabled', false);
        } else {
            // No es una cuenta de movimiento, mostrar mensaje
            AbrirMensaje(
                "Aviso",
                "Solo puede seleccionar cuentas de movimiento.",
                function () { $("#msjModal").modal("hide"); },
                false,
                ["Aceptar"],
                "info!",
                null
            );

            // Desseleccionar el nodo
            $("#cuentasTree").jstree("deselect_node", nodoId);

            // Deshabilitar el botón de seleccionar
            $('#btnSeleccionarCuenta').prop('disabled', true);
            cuentaSeleccionada = null;
        }
    });

    // Cuando el árbol está listo, colapsarlo inicialmente
    $("#cuentasTree").on("ready.jstree", function () {
        $("#cuentasTree").jstree("close_all");
    });
}


// Función global para cargar cuentas (para compatibilidad con llamadas externas)
function cargarCuentas() {
    if (!arbolCuentasInicializado) {
        cargarArbolCuentas();
    }
}

// Función para confirmar ajustes
function confirmarAjustes() {
    // Obtener el ID de la cuenta seleccionada y la fecha
    const cuentaAjusteId = $('#cuentaAjusteId').val();
    const fechaAsiento = $('#fechaAsiento').val();
    const selectedCcbId = $('.asiento-check:checked').first().data('ccb-id');
    const ejercicio = $("#Eje_nro").val();

    // Validar que se haya seleccionado una cuenta
    if (!cuentaAjusteId) {
        AbrirMensaje(
            "Validación",
            "Debe seleccionar una cuenta para el ajuste.",
            function () { $("#msjModal").modal("hide"); },
            false,
            ["Aceptar"],
            "warn!",
            null
        );
        return;
    }

    // Validar que se haya seleccionado una fecha
    if (!fechaAsiento) {
        AbrirMensaje(
            "Validación",
            "Debe seleccionar una fecha para el asiento.",
            function () { $("#msjModal").modal("hide"); },
            false,
            ["Aceptar"],
            "warn!",
            null
        );
        return;
    }

    // Validar que se haya seleccionado una cuenta para ajustar
    if (!selectedCcbId) {
        AbrirMensaje(
            "Validación",
            "Debe seleccionar una cuenta para aplicar el ajuste.",
            function () { $("#msjModal").modal("hide"); },
            false,
            ["Aceptar"],
            "warn!",
            null
        );
        return;
    }

    // Confirmar la operación
    AbrirMensaje(
        "Confirmación",
        "¿Está seguro de que desea generar el asiento de ajuste por inflación?",
        function (resp) {
            if (resp === "SI") {
                // Mostrar indicador de carga
                AbrirWaiting("Generando asiento de ajuste por inflación...");

                // Preparar datos para la petición
                const data = {
                    eje_nro: ejercicio,
                    ccb_id: selectedCcbId,
                    cuenta_ajuste_id: cuentaAjusteId,
                    fecha_asiento: fechaAsiento
                };

                // Realizar la petición al servidor
                $.ajax({
                    url: confirmarAsientoAjusteUrl, // Esta URL debe definirse en la vista
                    type: "POST",
                    contentType: "application/json",
                    data: JSON.stringify(data),
                    success: function (resultado) {
                        CerrarWaiting();

                        if (resultado.error) {
                            AbrirMensaje(
                                "Error",
                                resultado.msg || "Error al generar el asiento de ajuste",
                                function () { $("#msjModal").modal("hide"); },
                                false,
                                ["Aceptar"],
                                "error!",
                                null
                            );
                            return;
                        }

                        // Mostrar mensaje de éxito
                        AbrirMensaje(
                            "Éxito",
                            resultado.msg || "El asiento de ajuste por inflación se ha generado correctamente",
                            function () {
                                $("#msjModal").modal("hide");
                                // Actualizar la vista
                                buscaraaj();
                            },
                            false,
                            ["Aceptar"],
                            "success!",
                            null
                        );
                    },
                    error: function (xhr, status, error) {
                        CerrarWaiting();
                        AbrirMensaje(
                            "Error",
                            "Error al generar el asiento de ajuste: " + error,
                            function () { $("#msjModal").modal("hide"); },
                            false,
                            ["Aceptar"],
                            "error!",
                            null
                        );
                    }
                });
            }

            $("#msjModal").modal("hide");
        },
        true,
        ["SI", "NO"],
        "question!",
        null
    );
}

// Función para cancelar ajustes
function cancelarAjustes() {
    // Limpiar campos
    $('#cuentaAjuste').val('');
    $('#cuentaAjusteId').val('');
    $('#fechaAsiento').datepicker('setDate', new Date());

    // Desmarcar todos los checkboxes
    $('.asiento-check').prop('checked', false);
    $('#checkAllAAJ').prop('checked', false);

    // Actualizar vista
    actualizarEstadoAjusta();
}
