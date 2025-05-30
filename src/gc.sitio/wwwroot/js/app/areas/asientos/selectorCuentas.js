/**
 * Selector de cuentas contables para asientos
 */
(function () {
    // Variables globales para el selector
    let filaEditando = null;  // Referencia a la fila actual que se está editando
    let cuentaCallbackFn = null; // Función de callback al seleccionar una cuenta
    let arbolInicializado = false; // Flag para controlar si el árbol ya fue inicializado

    // URLs para las llamadas AJAX - Se configurarán desde la vista
    let buscarPlanCuentaUrl = '';
    let buscarCuentaUrl = '';

    /**
     * Inicializa el selector de cuentas
     * @param {string} urlBuscarPlan - URL para buscar el plan de cuentas
     * @param {string} urlBuscarCuenta - URL para buscar datos de una cuenta específica
     */
    function inicializar(urlBuscarPlan, urlBuscarCuenta) {
        buscarPlanCuentaUrl = urlBuscarPlan;
        buscarCuentaUrl = urlBuscarCuenta;

        // Evento para mostrar el botón de búsqueda al editar
        $(document).on('click', '#tbAsientoDetalle tbody td:first-child', function () {
            if ($(this).closest('tr').find('.btn-buscar-cuenta').is(':visible')) {
                return; // Ya está visible, no hacer nada
            }

            // Ocultar todos los otros botones de búsqueda
            $('#tbAsientoDetalle tbody .btn-buscar-cuenta').hide();

            // Resetear todas las filas a su estado normal
            $('#tbAsientoDetalle tbody tr').removeClass('fila-editando');

            // Marcar esta fila como editando
            $(this).closest('tr').addClass('fila-editando');

            // Mostrar el botón de búsqueda
            $(this).find('.btn-buscar-cuenta').show();
        });

        // Evento para ocultar el botón al hacer clic fuera
        $(document).on('click', function (e) {
            if (!$(e.target).closest('#tbAsientoDetalle tbody td:first-child').length &&
                !$(e.target).closest('#modalSelectorCuentas').length) {
                $('#tbAsientoDetalle tbody .btn-buscar-cuenta').hide();
                $('#tbAsientoDetalle tbody tr').removeClass('fila-editando');
            }
        });

        // Evento para abrir el selector de cuentas
        $(document).on('click', '.btn-buscar-cuenta', function (e) {
            e.stopPropagation();
            filaEditando = $(this).closest('tr');
            mostrarSelectorCuentas();
        });

        // Evento para buscar cuentas
        $('#btnBuscarCuenta').on('click', function () {
            const termino = $('#buscarCuenta').val();
            buscarEnArbol(termino);
        });

        // Buscar al presionar Enter
        $('#buscarCuenta').on('keypress', function (e) {
            if (e.which === 13) {
                const termino = $(this).val();
                buscarEnArbol(termino);
            }
        });

        // Evento al cerrar el modal para limpiar el estado
        $('#modalSelectorCuentas').on('hidden.bs.modal', function () {
            $('#buscarCuenta').val('');
            if (filaEditando) {
                filaEditando.removeClass('fila-editando');
            }
        });
    }

    /**
     * Muestra el selector de cuentas y carga el árbol si es necesario
     */
    function mostrarSelectorCuentas() {
        // Mostrar el modal
        $('#modalSelectorCuentas').modal('show');

        // Si el árbol no está inicializado, cargarlo
        if (!arbolInicializado) {
            cargarArbolCuentas();
        }
    }

    /**
     * Carga el árbol de cuentas mediante una llamada AJAX
     */
    function cargarArbolCuentas() {
        AbrirWaiting("Cargando plan de cuentas...");

        // Datos para la búsqueda - se podrían añadir filtros si es necesario
        const data = {
            buscar: "",
            buscaNew: true
        };

        // Llamada AJAX para obtener el árbol
        PostGen(data, buscarPlanCuentaUrl, function (obj) {
            CerrarWaiting();

            if (obj.error === true) {
                console.error("Error al cargar el plan de cuentas:", obj.msg);
                return;
            }

            try {
                // Parsear el árbol desde la respuesta JSON
                let jsonP = $.parseJSON(obj.arbol);

                // Preprocesar para asignar clases e íconos
                procesarNodosArbol(jsonP);

                // Inicializar el árbol con jsTree
                inicializarJsTree(jsonP);

                arbolInicializado = true;
            } catch (error) {
                console.error("Error al procesar el árbol de cuentas:", error);
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
     * Inicializa el control jsTree con los datos procesados
     * @param {Array} datos - Datos del árbol procesados
     */
    function inicializarJsTree(datos) {
        // Destruir instancia previa si existe
        $("#arbolPlanCuentas").jstree("destroy").empty();

        // Inicializar nueva instancia
        $("#arbolPlanCuentas").jstree({
            "core": {
                "data": datos,
                "themes": {
                    "responsive": true
                }
            },
            "types": {
                "activo": {
                    "icon": "bx bx-wallet"
                },
                "pasivo": {
                    "icon": "bx bx-trending-down"
                },
                "patrimonio": {
                    "icon": "bx bx-building-house"
                },
                "ingresos": {
                    "icon": "bx bx-dollar-circle"
                },
                "egresos": {
                    "icon": "bx bx-print-dollar"
                },
                "default": {
                    "icon": "bx bx-folder"
                }
            },
            "plugins": ["types", "search"]
        });

        // Evento al seleccionar un nodo
        $("#arbolPlanCuentas").on("select_node.jstree", function (e, data) {
            const nodoId = data.node.id;
            const nodoTexto = data.node.text;
            const nodoTipo = data.node.data?.tipo;

            // Solo se pueden seleccionar cuentas de tipo Movimiento (M)
            if (nodoTipo === "M") {
                seleccionarCuenta(nodoId, nodoTexto);
            } else {
                // Mostrar mensaje si no es cuenta de movimiento
                AbrirMensaje("Aviso", "Solo se pueden seleccionar cuentas de movimiento.",
                    function () { $("#msjModal").modal("hide"); },
                    false, ["Aceptar"], "info!", null);
            }
        });
    }

    /**
     * Busca un término en el árbol y expande resultados
     * @param {string} termino - Término de búsqueda
     */
    function buscarEnArbol(termino) {
        if (!termino) return;

        // Obtener la instancia de jsTree
        const tree = $("#arbolPlanCuentas").jstree(true);
        if (!tree) return;

        // Cerrar todos los nodos primero
        tree.close_all();

        // Buscar en el árbol
        tree.search(termino, false, true);

        // Expandir los resultados encontrados
        const resultados = tree.search_result;
        resultados.forEach(nodeId => {
            // Encontrar padres y expandirlos
            tree.open_node(tree.get_parent(nodeId));
        });
    }

    /**
     * Procesa la selección de una cuenta
     * @param {string} id - ID de la cuenta
     * @param {string} texto - Texto descriptivo de la cuenta
     */
    function seleccionarCuenta(id, texto) {
        if (!filaEditando) return;

        // Obtener datos adicionales de la cuenta si es necesario
        obtenerDatosCuenta(id, function (datos) {
            // Actualizar el ID de la cuenta en la primera celda
            filaEditando.find('.cuenta-id').text(id);

            // Actualizar la descripción de la cuenta en la segunda celda
            // Si no encuentra .cuenta-desc, seleccionamos la segunda celda directamente
            const celdaDescripcion = filaEditando.find('.cuenta-desc').length > 0
                ? filaEditando.find('.cuenta-desc')
                : filaEditando.find('td:eq(1)');

            celdaDescripcion.text(datos ? datos.ccb_desc : texto)
                .attr('contenteditable', 'false')  // Hacerla readonly
                .addClass('cuenta-desc');          // Asegurar que tenga la clase

            // Eliminar la clase de cuenta faltante si existe
            filaEditando.find('td:first-child').removeClass('cuenta-faltante');

            // Ocultar el botón de búsqueda
            filaEditando.find('.btn-buscar-cuenta').hide();

            // Quitar la clase de fila editando
            filaEditando.removeClass('fila-editando');

            // Actualizar los totales
            if (typeof actualizarTotales === 'function') {
                actualizarTotales();
            }

            // Cerrar el modal
            $('#modalSelectorCuentas').modal('hide');

            // Limpiar referencia
            filaEditando = null;
        });
    }

    /**
     * Obtiene datos adicionales de una cuenta mediante AJAX
     * @param {string} id - ID de la cuenta
     * @param {Function} callback - Función de callback con los datos
     */
    function obtenerDatosCuenta(id, callback) {
        // Si no hay URL de búsqueda, devolver directamente
        if (!buscarCuentaUrl) {
            callback(null);
            return;
        }

        // Datos para la petición
        const data = { id: id };

        // Llamada AJAX para obtener detalles de la cuenta
        PostGen(data, buscarCuentaUrl, function (obj) {
            if (obj.error === true || obj.warn === true) {
                console.error("Error al obtener datos de la cuenta:", obj.msg);
                callback(null);
                return;
            }

            // Devolver la entidad de cuenta
            callback(obj.entidad);
        });
    }

    // Exponer funciones públicas
    window.selectorCuentas = {
        inicializar: inicializar
    };
})();