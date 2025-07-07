$(function () {
    inicializaComponentesAsientoResultado();
    configurarBotonesAsientoResultado();

    // Botón de confirmar ajustes
    $(document).on('click', '#btnConfirmarAjuste', function () {
        confirmarAjustesRes();
    });

    // Botón de cancelar ajustes
    $(document).on('click', '#btnCancelarAjuste', function () {
        cancelarAjustesRes();
    });
});

function inicializaComponentesAsientoResultado() {
    // Inicializar tooltips
    $('[data-bs-toggle="tooltip"]').tooltip();

    // Inicializar el selector de cuentas
    inicializarSelectorCuentas();
}
function configurarBotonesAsientoResultado() {
    // Botón de cancelar
    $("#btnCancel").on("click", function () {
        window.location.href = homeAResUrl;
    });

    // Botón de búsqueda
    $("#btnBuscar").on("click", function () {

        // Es una nueva búsqueda, no resguardamos la búsqueda anterior
        mayorDataBak = {};

        // Es una búsqueda por filtro, siempre será página 1
        pagina = 1;

        // Realizar búsqueda
        buscarres();
    });

    // Usar mousedown en lugar de click para evitar conflictos con el collapse
    $("#btnDetalle").on("mousedown", analizaEstadoBtnDetalleAres);
    $("#btnFiltro").on("mousedown", function () {
        if ($("#divFiltro").is(":hidden")) {
            $("#divDetalle").collapse("hide");
        }
    });
}

function analizaEstadoBtnDetalleAres() {
    if ($("#divDetalle").is(":visible")) {        
        limpiarAsres();
    }
}

function buscarres() {
    AbrirWaiting("Consultando los asientos de resultados. Espere, puede tardar. Sea paciente...");

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
    cargarReporteEnArre(22, data, "Asientos de Resultado por PG", "", administracion);

    limpiarAsres();

    // Realizamos la petición al servidor
    PostGenHtml(data, buscarAsientosResUrl, function (obj) {
        // Mostramos el resultado en el div correspondiente
        $("#divAsres").html(obj);

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

function limpiarAsres() {
    $("#divAsres").empty();
    $("#btnFilter").collapse("show");
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
function confirmarAjustesRes() {
    // Obtener el ID de la cuenta seleccionada y la fecha
    const cuentaAjusteId = $('#cuentaAjusteId').val();
    const ejercicio = parseInt($("#Eje_nro").val());

    // Obtener todas las cuentas de resultados
    const cuentasSeleccionadas = [];

    $('[data-ccb-id]').each(function () {
        const ccbId = $(this).data('ccb-id');
        if (ccbId && cuentasSeleccionadas.indexOf(ccbId) === -1) {
            cuentasSeleccionadas.push(ccbId);
        }
    });

    // Validaciones
    if (!ejercicio || isNaN(ejercicio)) {
        AbrirMensaje(
            "Validación",
            "Debe seleccionar un ejercicio contable válido.",
            function () { $("#msjModal").modal("hide"); },
            false,
            ["Aceptar"],
            "warn!",
            null
        );
        return;
    }

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

    if (cuentasSeleccionadas.length === 0) {
        AbrirMensaje(
            "Validación",
            "Debe seleccionar al menos una cuenta para aplicar el ajuste.",
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
        "¿Está seguro de que desea generar el asiento de resultados PG?",
        function (resp) {
            if (resp === "SI") {
                // Mostrar indicador de carga
                AbrirWaiting("Generando asiento de resultados PG...");

                

                // CORRECCIÓN: Enviar los datos con los nombres correctos como parámetros normales (no JSON)
                $.ajax({
                    url: confirmarAsientoResUrl,
                    type: "POST",
                    data: {
                        eje_nro: ejercicio,
                        ccbid: cuentaAjusteId,
                        listCcb: cuentasSeleccionadas
                    },
                    traditional: true, // Importante para arrays
                    success: function (resultado) {
                        CerrarWaiting();

                        if (resultado.error) {
                            AbrirMensaje(
                                "Error",
                                resultado.msg || "Error al generar el asiento de resultados PG",
                                function () { $("#msjModal").modal("hide"); },
                                false,
                                ["Aceptar"],
                                "error!",
                                null
                            );
                            return;
                        }

                        if (resultado.warn) {
                            AbrirMensaje(
                                "Advertencia",
                                resultado.msg || "Se encontraron advertencias al generar el asiento",
                                function () {
                                    $("#msjModal").modal("hide");
                                    // Si es un problema de autenticación, redirigir al login
                                    if (resultado.auth) {
                                        window.location.href = loginUrl;
                                    }
                                },
                                false,
                                ["Aceptar"],
                                "warn!",
                                null
                            );
                            return;
                        }

                        // Mostrar mensaje de éxito
                        AbrirMensaje(
                            "Éxito",
                            resultado.msg || "El asiento de ajuste de resultados PG se ha generado correctamente",
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

                        // Intentar parsear el mensaje de error si es un JSON
                        let mensajeError = error;
                        try {
                            const respuestaError = JSON.parse(xhr.responseText);
                            if (respuestaError && respuestaError.msg) {
                                mensajeError = respuestaError.msg;
                            }
                        } catch (e) {
                            // Si no es un JSON válido, usar el mensaje de error original
                        }

                        AbrirMensaje(
                            "Error",
                            "Error al generar el asiento de resultados PG: " + mensajeError,
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
function cancelarAjustesRes() {
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
