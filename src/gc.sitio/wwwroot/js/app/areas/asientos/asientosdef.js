// Al cargar, establecer variables para control de selección
let filaClicDoble = null; // Fila seleccionada con doble clic (detalle)
let filasSeleccionadas = []; // Filas seleccionadas para operaciones batch

// Función para actualizar el mensaje informativo
function actualizarMensajeSeleccion() {
    if ($("#seleccion-info").length === 0) {
        const mensaje = `
            <div id="seleccion-info" class="alert alert-info mb-2 py-1 small">
                <i class="bx bx-info-circle"></i> 
                Haga clic en un asiento para seleccionarlo. Use Ctrl+Clic para selección múltiple.
                <span class="badge bg-primary ms-1" id="selected-count">0</span> asiento(s) seleccionado(s).
            </div>
        `;
        $("#tbGridAsiento").after(mensaje);
    }
}

// Agregar manejo de tooltips para estados de asientos después de cargar la grilla
function inicializarTooltipsEstados() {
    // Seleccionar todas las celdas de estado que tengan el estado "revisar" != 0
    $("#tbGridAsiento tbody tr[data-revisable='true'] td:last-child").each(function () {
        const $celda = $(this);
        const mensaje = $celda.attr("data-revisar-desc") || "Este asiento requiere revisión";

        // Configurar tooltip de Bootstrap
        $celda.attr({
            "data-bs-toggle": "tooltip",
            "data-bs-placement": "left",
            "title": mensaje
        });

        // Inicializar tooltip
        new bootstrap.Tooltip($celda);

        // Cambiar estilo visual para llamar la atención
        $celda.addClass("text-danger fw-bold");
    });
}

// Llamar a esta función después de cargar la grilla
function inicializarSeleccionAsientos() {
    // Limpiar variables
    filaClicDoble = null;
    filasSeleccionadas = [];

    // Agregar mensaje de selección
    //actualizarMensajeSeleccion();

    // Deshabilitar botón inicialmente
    $("#btnPasarConta").prop("disabled", true);

    // Actualizar contador
    $("#selected-count").text("0");

    // Inicializar tooltips para estados de revisión
    setTimeout(inicializarTooltipsEstados, 50);
}

// Guardar la referencia original de la función
const buscarAsientoDefsOriginal = buscarAsientosDefs;

// Redefinir la función para incluir la inicialización de selección
function buscarAsientosDefs(pag) {
    // Validar campos obligatorios antes de proceder
    if (!validarCamposObligatoriosBusqueda()) {
        return; // Detener la ejecución si la validación falla
    }

    AbrirWaiting();
    //desactivamos los botones de acción
    activarBotones2(false);

    // Obtenemos los valores de los campos del filtro
    var data1 = {
        Eje_nro: $("#Eje_nro").val(), // Ejercicio
        Movi: $("#Movi").is(":checked").toString(), // bit Movimiento
        Movi_like: $("#Movi_like").val(), // Nro Movimiento
        Usu: $("#Usu").is(":checked").toString(), // bit Usuario
        Usu_like: $("#Usu_like").val(), // Usuario
        Tipo: $("#Tipo").is(":checked").toString(), // bit Tipo
        Tipo_like: $("#Tipo_like").val(), // Tipo de Asiento
        Rango: $("#Rango").is(":checked").toString(), // bit Rango
        Desde: $("input[name='Desde']").val(), // Fecha Desde
        Hasta: $("input[name='Hasta']").val() // Fecha Hasta
    };
    //let admId = administracion;
    //agregando los parametros de la busqueda realizada
    cargarReporteEnArre(11, data1, "Informe de Asientos Solicitados.", "Obseración:", "")

    var buscaNew = JSON.stringify(dataBak) != JSON.stringify(data1)

    if (buscaNew === false) {
        //son iguales las condiciones cambia de pagina
        pagina = pag;
    }
    else {
        dataBak = data1;
        pagina = 1;
        pag = 1;
    }

    var sort = null;
    var sortDir = null

    var data2 = { sort, sortDir, pag, buscaNew }

    var data = $.extend({}, data1, data2);

    PostGenHtml(data, buscarAsientoDefsUrl, function (obj) {
        $("#divGrilla").html(obj);
        $("#divFiltro").collapse("hide")

        // Agregar un pequeño retraso para asegurar que la grilla ya está cargada
        setTimeout(inicializarSeleccionAsientos, 100);

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
        CerrarWaiting();
    }, function (obj) {
        ControlaMensajeError(obj.message);
        CerrarWaiting();

    });
}

/**
 * Valida que los campos obligatorios para la búsqueda de asientos definitivos estén completos
 * @returns {boolean} True si todos los campos obligatorios están completos, false en caso contrario
 */
// Función para validar que los dropdowns obligatorios tengan valores seleccionados
function validarCamposObligatoriosBusqueda() {
    let camposFaltantes = [];

    // Validar ejercicio
    if (!$("#Eje_nro").val()) {
        camposFaltantes.push("Ejercicio");
    }

    // Validar usuario
    if (!$("#Usu_like").val()) {
        camposFaltantes.push("Usuario");
    }

    // Validar tipo de asiento
    if (!$("#Tipo_like").val()) {
        camposFaltantes.push("Tipo de Asiento");
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

// Script para configurar los controles de filtro en asientos definitivos
function configurarFiltrosAsientoDefinitivo() {
    // 1. Forzar selección y deshabilitación de checkboxes obligatorios
    $('#chkEjercicio, #Usu, #Tipo').prop('checked', true).prop('disabled', true);

    // 2. Habilitar los dropdowns asociados
    $('#Eje_nro, #Usu_like, #Tipo_like').prop('disabled', false);

    // 3. Prevenir clics en los checkboxes
    $('#chkEjercicio, #Usu, #Tipo').on('click', function (e) {
        e.preventDefault();
        return false;
    });
    // Actualizar el nombre del usuario seleccionado en la botonera
    $('#Usu_like').on('change', function () {
        var usuarioSeleccionado = $(this).find('option:selected').text();
        $('#nombreUsuarioSeleccionado').text('Usuario: ' + usuarioSeleccionado);
        $('#usuarioSeleccionadoLabel').removeClass('bg-secondary').addClass('bg-primary');
    });

    // Inicializar con el valor actual (si hay uno seleccionado)
    if ($('#Usu_like').val()) {
        var usuarioActual = $('#Usu_like').find('option:selected').text();
        $('#nombreUsuarioSeleccionado').text('Usuario: ' + usuarioActual);
        $('#usuarioSeleccionadoLabel').removeClass('bg-secondary').addClass('bg-primary');
    }

    // Detectar cambio en el ejercicio y actualizar la lista de usuarios
    $('#Eje_nro').on('change', function () {
        const ejercicioId = $(this).val();
        if (!ejercicioId) return;

        // Desactivar los botones de búsqueda y cancelar
        const $btnBuscar = $("#btnBuscar");
        const $btnCancel = $("#btnCancel");
        $btnBuscar.prop('disabled', true);
        $btnCancel.prop('disabled', true);

        // Mostrar indicador de carga
        const $usuarioSelect = $('#Usu_like');
        const valorActual = $usuarioSelect.val();
        $usuarioSelect.prop('disabled', true).html('<option value="">Cargando usuarios...</option>');

        // Actualizar etiqueta en la botonera
        $('#nombreUsuarioSeleccionado').text('Usuario: Cargando...');
        $('#usuarioSeleccionadoLabel').removeClass('bg-primary').addClass('bg-secondary');

        // Agregar un spinner junto al dropdown para indicar carga
        if ($('#spinner-usuarios').length === 0) {
            $usuarioSelect.after(`
                <div id="spinner-usuarios" class="dots-spinner ms-2">
                    <div></div>
                    <div></div>
                    <div></div>
                    <div></div>
                </div>
            `);
        } else {
            $('#spinner-usuarios').show();
        }



        // Realizar petición AJAX para obtener los usuarios del ejercicio seleccionado
        $.ajax({
            url: obtenerUsuariosEjercicioUrl, // URL definida en la vista Index
            type: 'GET',
            data: { ejercicioId: ejercicioId },
            success: function (data) {
                // Limpiar el select
                $usuarioSelect.empty();

                // Agregar opción por defecto
                $usuarioSelect.append('<option value="">Seleccione usuario</option>');

                // Agregar opciones con los usuarios recibidos
                $.each(data, function (index, item) {
                    $usuarioSelect.append(`<option value="${item.id}">${item.descripcion}</option>`);
                });

                // Restaurar valor seleccionado si existe en la nueva lista
                if (valorActual && $usuarioSelect.find(`option[value="${valorActual}"]`).length > 0) {
                    $usuarioSelect.val(valorActual);
                    const usuarioTexto = $usuarioSelect.find('option:selected').text();
                    $('#nombreUsuarioSeleccionado').text('Usuario: ' + usuarioTexto);
                    $('#usuarioSeleccionadoLabel').removeClass('bg-secondary').addClass('bg-primary');
                } else {
                    // Si el valor anterior no existe en la nueva lista, limpiar la selección
                    $usuarioSelect.val('');
                    $('#nombreUsuarioSeleccionado').text('Usuario: Sin seleccionar');
                    $('#usuarioSeleccionadoLabel').removeClass('bg-primary').addClass('bg-secondary');
                }

                // Habilitar el select y los botones
                $usuarioSelect.prop('disabled', false);
                $btnBuscar.prop('disabled', false);
                $btnCancel.prop('disabled', false);

                // Ocultar el spinner
                $('#spinner-usuarios').hide();
            },
            error: function (xhr, status, error) {
                console.error('Error al obtener usuarios:', error);

                // Restaurar el select con mensaje de error
                $usuarioSelect.html('<option value="">Error al cargar usuarios</option>');
                $usuarioSelect.prop('disabled', false);

                // Habilitar los botones
                $btnBuscar.prop('disabled', false);
                $btnCancel.prop('disabled', false);

                // Ocultar el spinner
                $('#spinner-usuarios').hide();

                // Actualizar etiqueta en la botonera
                $('#nombreUsuarioSeleccionado').text('Usuario: Error al cargar');
                $('#usuarioSeleccionadoLabel').removeClass('bg-primary').addClass('bg-danger');

                // Mostrar mensaje de error
                AbrirMensaje(
                    "ERROR",
                    "No se pudieron cargar los usuarios para el ejercicio seleccionado: " + error,
                    function () {
                        $("#msjModal").modal("hide");
                    },
                    false,
                    ["Aceptar"],
                    "error!",
                    null
                );
            },
            complete: function () {
                // Asegurar que siempre se habiliten los botones y se oculte el spinner
                // incluso si hay errores no capturados
                $btnBuscar.prop('disabled', false);
                $btnCancel.prop('disabled', false);
                $('#spinner-usuarios').hide();
            }
        });
    });


    console.log('Configuración de filtros para asientos definitivos aplicada');
}


$(function () {

    // Agregar estilos CSS para diferencias visuales claras
    $("<style>")
        .prop("type", "text/css")
        .html(`
        /* Estilo para selección simple (un clic) */
        #tbGridAsiento tbody tr.selected-simple {
            background-color: lightgray !important;
            color: white !important;
        }
        
        /* Estilo para selección múltiple (Ctrl+clic) */
        #tbGridAsiento tbody tr.selected-multiple {
            background-color: #e7f3ff !important;
            box-shadow: inset 0 0 0 1px #0d6efd !important;
        }
        
        /* Estilo para fila con detalle abierto (doble clic) */
        #tbGridAsiento tbody tr.selectedEdit-row {
            background-color: firebrick !important;
            color: white !important;
        }
        
        /* Asegurar que todas las filas tengan cursor de puntero */
        #tbGridAsiento tbody tr {
            cursor: pointer;
        }

         /* Estilos para los checkboxes */
        .checkbox-column {
            width: 40px;
            text-align: center;
        }
        
        .form-check-input {
            cursor: pointer;
        }
        
        /* Estilo para checkboxes deshabilitados */
        .form-check-input:disabled {
            opacity: 0.6;
            cursor: not-allowed;
        }
        
        /* Cambiar el color del fondo de las filas con checkboxes marcados */
        #tbGridAsiento tbody tr:has(.asiento-checkbox:checked) {
            background-color: #e7f3ff !important;
        }

        /* AGREGAR AQUÍ LOS NUEVOS ESTILOS PARA EL SPINNER */
        /* Spinner de puntos giratorio */
        .dots-spinner {
            display: inline-block;
            position: relative;
            width: 20px;
            height: 20px;
        }
    
        .dots-spinner div {
            position: absolute;
            width: 6px;
            height: 6px;
            background-color: #0d6efd;
            border-radius: 50%;
            animation: dots-spinner 1.2s linear infinite;
        }
    
        .dots-spinner div:nth-child(1) {
            top: 0;
            left: 7px;
            animation-delay: 0s;
        }
    
        .dots-spinner div:nth-child(2) {
            top: 5px;
            left: 14px;
            animation-delay: -0.1s;
        }
    
        .dots-spinner div:nth-child(3) {
            top: 14px;
            left: 7px;
            animation-delay: -0.2s;
        }
    
        .dots-spinner div:nth-child(4) {
            top: 5px;
            left: 0;
            animation-delay: -0.3s;
        }
    
        @keyframes dots-spinner {
            0%, 100% {
                opacity: 1;
            }
            50% {
                opacity: 0.2;
            }
        }
    `)
        .appendTo("head");


    // Eliminar todos los manejadores previos para evitar duplicación
    $(document).off("click", "#tbGridAsiento tbody tr");
    $(document).off("dblclick", "#tbGridAsiento tbody tr");


    // Llamar a la función de configuración
    configurarFiltrosAsientoDefinitivo();

    // Modificar la función buscarAsientoDefs para inicializar selección
    buscarAsientosDefs = function (pag) {
        buscarAsientoDefsOriginal(pag);
        // Agregar un pequeño retraso para asegurar que la grilla ya está cargada
        setTimeout(inicializarSeleccionAsientos, 100);
    };

    // Modificar evento del botón buscar para incluir validación
    const originalBtnBuscarClick = $("#btnBuscar").attr('onclick');
    $("#btnBuscar").off('click').on('click', function () {
        // Validar que los dropdowns obligatorios tengan valores seleccionados
        if (!validarCamposObligatoriosBusqueda()) {
            return false;
        }

        // Si hay un manejador previo, ejecutarlo
        if (typeof originalBtnBuscarClick === 'function') {
            return originalBtnBuscarClick.apply(this, arguments);
        } else {
            // Comportamiento predeterminado
            $("#divpanel01").empty();
            dataBak = "";
            pagina = 1;
            buscarAsientosDefs(pagina);
        }
    });
    
    //callback para que funcione la paginación
    funcCallBack = buscarAsientosDefs;
    $("#pagEstado").on("change", function () {
        var div = $("#divPaginacion");
        presentaPaginacion(div);
    });
    $("#btnCancel").on("click", function () {
        window.location.href = homeAsientosUrl;
    });

    //**** EVENTOS PARA CONTROLAR LOS BOTONES DE OPERACION
    // $("#btnAbmNuevo").on("click", ejecutarAltaAsientoTemp);
    // Modificar eventos para botones según las restricciones de asientos definitivos
    $("#btnAbmNuevo").hide(); // Ocultar el botón de nuevo asiento

    $("#btnAbmModif").on("click", ejecutarModificacionAsientoDef);
    $("#btnAbmElimi").on("click", ejecutarAnulacionAsientoDef);

    $("#btnAbmCancelar").on("click", InicializaVistaAsientos);
    $("#btnAbmAceptar").on("click", confirmarOperacionAsientoDef);
    //****FIN BOTONES DE OPERACION*/

    // Evento para el botón Imprimir
    $(document).on("click", "#btnImprimir", imprimirAsiento);
    // Usar mousedown en lugar de click para evitar conflictos con el collapse
    $("#btnDetalle").on("mousedown", analizaEstadoBtnDetalle);

    // Forzar que el ejercicio siempre esté seleccionado en asientos definitivos
    $('#chkEjercicio').prop('checked', true).prop('disabled', true);
    $('#Eje_nro').prop('disabled', false);

    $('#Usu').prop('checked', true).prop('disabled', true);
    $('#Usu_like').prop('disabled', false);

    $('#Tipo').prop('checked', true).prop('disabled', true);
    $('#Tipo_like').prop('disabled', false);

    // Modificar el comportamiento de toggleComponent para los campos obligatorios
    const originalToggleComponent = window.toggleComponent;
    window.toggleComponent = function (checkboxId, componentSelector) {
        // Si es alguno de los campos obligatorios, siempre debe estar seleccionado y habilitado
        if (checkboxId === 'chkEjercicio' || checkboxId === 'Usu' || checkboxId === 'Tipo') {
            $(`#${checkboxId}`).prop("checked", true);
            $(componentSelector).prop("disabled", false);
            return;
        }

        // Para otros componentes, utilizar la función original
        if (typeof originalToggleComponent === 'function') {
            originalToggleComponent(checkboxId, componentSelector);
        } else {
            // Implementación por defecto
            const isChecked = $(`#${checkboxId}`).is(':checked');
            $(componentSelector).prop('disabled', !isChecked);
        }
    };

    // Deshabilitar eventos click en los checkboxes obligatorios
    $('#chkEjercicio, #Usu, #Tipo').on('click', function (e) {
        e.preventDefault();
        return false;
    });

    //// Eventos para activar/desactivar componentes
    //$('#chkEjercicio').on('change', function () {
    //    if (typeof asientoTemporal !== 'undefined' && asientoTemporal === true) {
    //        // Si está en modo temporal, no ejecutar y desactivar el checkbox
    //        //$('#chkEjercicio').prop('checked', false).prop('disabled', true);
    //        return;
    //    }
    //    toggleComponent('chkEjercicio', '#Eje_nro');
    //});

    $('#Movi').on('change', function () {
        toggleComponent('Movi', '#Movi_like');
    });

    $('#Usu').on('change', function () {
        toggleComponent('Usu', '#Usu_like');
    });

    $('#Tipo').on('change', function () {
        toggleComponent('Tipo', '#Tipo_like');
    });

    $('#Rango').on('change', function () {
        toggleComponent('Rango', 'input[name="Desde"]');
        toggleComponent('Rango', 'input[name="Hasta"]');
    });

    // Agregar manejo de clic simple en filas para selección múltiple
    // Modificar el manejador del clic para mostrar checkboxes cuando se usa Ctrl+click
    // Agregar manejo de clic simple en filas para selección múltiple - SOLUCIÓN CORREGIDA
    $(document).on("click", "#tbGridAsiento tbody tr", function (e) {
        // Si se hace clic en un checkbox o en su contenedor, no hacer nada aquí
        if ($(e.target).is('.asiento-checkbox') || $(e.target).closest('.form-check').length > 0) {
            return;
        }

        const $this = $(this);
        const id = $this.find("td:nth-child(2)").text().trim(); // Ajustado por la nueva columna
        const esRevisable = $this.attr("data-revisable") === "true";

        // Si es doble clic, no hacer nada (el evento dblclick lo manejará)
        if (e.detail === 2) return;

        // Prevenir la propagación del evento
        e.stopPropagation();

        // Limpiar estilo de selección de doble clic (firebrick) de todas las filas
        $("#tbGridAsiento tbody tr").removeClass("selectedEdit-row");

        // Selección simple vs. múltiple
        if (e.ctrlKey || e.metaKey) {
            // Selección múltiple (Ctrl+clic)
            if ($this.hasClass("selected-multiple")) {
                // Si ya estaba en selección múltiple, quitar
                $this.removeClass("selected-multiple");

                // Desmarcar el checkbox correspondiente
                $(`#check_${id}`).prop("checked", false);

                // Eliminar de la lista de seleccionados
                filasSeleccionadas = filasSeleccionadas.filter(item => item !== id);
            } else {
                // Si no estaba seleccionada y no es revisable, seleccionar como múltiple
                if (!esRevisable) {
                    $this.addClass("selected-multiple");

                    // Marcar el checkbox correspondiente
                    $(`#check_${id}`).prop("checked", true);

                    // Agregar a la lista si no existe
                    if (!filasSeleccionadas.includes(id)) {
                        filasSeleccionadas.push(id);
                    }
                }
            }
        } else {
            // Selección simple (clic)
            // Quitar selección múltiple de todas las filas
            $("#tbGridAsiento tbody tr").removeClass("selected-multiple");

            // Desmarcar todos los checkboxes
            $(".asiento-checkbox").prop("checked", false);

            filasSeleccionadas = [];

            // Si no es revisable, seleccionar
            if (!esRevisable) {
                // Aplicar selección simple solo a esta fila
                $this.addClass("selected-simple");
                $("#tbGridAsiento tbody tr").not($this).removeClass("selected-simple");

                // Marcar solo el checkbox correspondiente
                $(`#check_${id}`).prop("checked", true);

                // Agregar a la lista de seleccionados
                filasSeleccionadas = [id];
            }
        }

        // Actualizar estado del botón
        const haySeleccionados = filasSeleccionadas.length > 0;
        $("#btnPasarConta").prop("disabled", !haySeleccionados);

        // Actualizar mensaje informativo
        $("#selected-count").text(filasSeleccionadas.length);
    });


    // Evento para el checkbox "selectAll"
    $(document).on("change", "#selectAllAsientos", function () {
        const isChecked = $(this).prop("checked");

        // Limpiar todas las selecciones existentes
        $("#tbGridAsiento tbody tr").removeClass("selected-multiple selected-simple");
        filasSeleccionadas = [];

        // Marcar/desmarcar todos los checkboxes (excepto los deshabilitados)
        $(".asiento-checkbox:not(:disabled)").prop("checked", isChecked);

        if (isChecked) {
            // Recopilar todos los IDs seleccionados de los checkboxes marcados
            $(".asiento-checkbox:checked").each(function () {
                const id = $(this).val();
                if (id && !filasSeleccionadas.includes(id)) {
                    filasSeleccionadas.push(id);
                }
            });

            // Seleccionar todas las filas correspondientes
            $("#tbGridAsiento tbody tr").each(function () {
                const $row = $(this);
                const id = $row.find("td:nth-child(2)").text().trim();

                if (filasSeleccionadas.includes(id)) {
                    $row.addClass("selected-multiple");
                }
            });
        }

        // Actualizar estado del botón
        const haySeleccionados = filasSeleccionadas.length > 0;
        $("#btnPasarConta").prop("disabled", !haySeleccionados);

        // Actualizar contador
        $("#selected-count").text(filasSeleccionadas.length);

        console.log("IDs seleccionados:", filasSeleccionadas); // Debug para verificar
    });

    // Evento para los checkboxes individuales - SOLUCIÓN
    $(document).on("click", ".asiento-checkbox", function (e) {
        e.stopPropagation(); // Evitar que se propague al tr
        //e.preventDefault(); // Evitar el comportamiento predeterminado del checkbox

        const $checkbox = $(this);
        const id = $checkbox.val();
        const isChecked = $checkbox.prop("checked"); // Obtenemos el estado DESPUÉS del clic
        const $row = $checkbox.closest("tr");

        if (isChecked) {
            // Seleccionar la fila
            $row.addClass("selected-multiple").removeClass("selected-simple");

            // Agregar a la lista de seleccionados
            if (!filasSeleccionadas.includes(id)) {
                filasSeleccionadas.push(id);
            }
        } else {
            // Deseleccionar la fila
            $row.removeClass("selected-multiple selected-simple");

            // Quitar de la lista de seleccionados
            filasSeleccionadas = filasSeleccionadas.filter(item => item !== id);

            // Desmarcar el checkbox "selectAll" si alguno se desmarca
            $("#selectAllAsientos").prop("checked", false);
        }

        // Verificar si todos los checkboxes no deshabilitados están marcados
        const todosSeleccionados = $(".asiento-checkbox:not(:disabled)").length ===
            $(".asiento-checkbox:not(:disabled):checked").length;
        $("#selectAllAsientos").prop("checked", todosSeleccionados);

        // Actualizar estado del botón
        const haySeleccionados = filasSeleccionadas.length > 0;
        $("#btnPasarConta").prop("disabled", !haySeleccionados);

        // Actualizar contador
        $("#selected-count").text(filasSeleccionadas.length);
    });

    // Manejo de doble clic para ver detalle
    $(document).on("dblclick", "#tbGridAsiento tbody tr", function (e) {
        e.stopPropagation();
        const $this = $(this);

        // Limpiar selección simple/múltiple (visual) para evitar confusión
        $("#tbGridAsiento tbody tr").removeClass("selected-simple selected-multiple");

        // Aplicar estilo de fila seleccionada para detalle
        $("#tbGridAsiento tbody tr").removeClass("selectedEdit-row");
        $this.addClass("selectedEdit-row");
        filaClicDoble = $this;

        // Llamar a la función existente de detalle
        ejecutaDblClickGridAsiento($this);
    });

    InicializaVistaAsientos();
})

//***FUNCIONES DE OPERACION***/
function ejecutarAltaAsientoTemp() {
    AbrirWaiting("Espere, se blanquea el ASIENTO...");
    accion = AbmAction.ALTA;
    var data = {};
    PostGenHtml(data, nuevoAsientoUrl, function (obj) {
        $("#divpanel01").html(obj);

        $("#btnDetalle").prop("disabled", false);
        $("#divFiltro").collapse("hide");
        $("#divDetalle").collapse("show");

        accionBotones(accion);
        activarControles(true);

        CerrarWaiting();
    });
}

/**
 * Ejecuta la modificación de un asiento definitivo, verificando primero los permisos
 */
function ejecutarModificacionAsientoDef() {
    // Verificar que haya un asiento seleccionado
    if (!EntidadSelect) {
        AbrirMensaje("ATENCIÓN", "Debe seleccionar un asiento definitivo para modificar.", function () {
            $("#msjModal").modal("hide");
        }, false, ["Aceptar"], "warn!", null);
        return;
    }

    // Mostrar indicador de espera mientras verificamos permisos
    AbrirWaiting("Verificando permisos de modificación...");

    // Verificar si el asiento es modificable (fecha dentro del período permitido)
    try {
        // Obtener la fecha del asiento del DOM
        const fechaStr = $("#Dia_fecha").val();
        if (!fechaStr) {
            CerrarWaiting();
            ControlaMensajeError("No se pudo obtener la fecha del asiento para verificar permisos");
            return;
        }

        // Intentar parsear la fecha en varios formatos
        let fechaAsiento;

        // Verificar si la fecha viene en formato DD/MM/YYYY
        if (fechaStr.includes('/')) {
            const partes = fechaStr.split('/');
            if (partes.length === 3) {
                fechaAsiento = new Date(
                    parseInt(partes[2]), // año
                    parseInt(partes[1]) - 1, // mes (0-11)
                    parseInt(partes[0]) // día
                );
            }
        }
        // Si no tiene formato DD/MM/YYYY o el parsing falló, intentar con el constructor normal
        if (!fechaAsiento || isNaN(fechaAsiento.getTime())) {
            fechaAsiento = new Date(fechaStr);
        }

        // Verificar si la fecha es válida antes de continuar
        if (isNaN(fechaAsiento.getTime())) {
            CerrarWaiting();
            ControlaMensajeError("Fecha de asiento inválida: " + fechaStr);
            return;
        }

        // Ejercicio seleccionado
        const ejercicioId = $("#Eje_nro").val() || "0";

        // Crear objeto para enviar al servidor
        const data = {
            eje_nro: ejercicioId,
            dia_fecha: fechaAsiento.toISOString()
        };

        // Verificar permisos en el servidor
        $.ajax({
            url: verificarFechaModificacionUrl,
            type: "POST",
            contentType: "application/json",
            data: JSON.stringify(data),
            success: function (response) {
                CerrarWaiting();

                if (response.permitido === false) {
                    // No se permite modificar por fecha
                    AbrirMensaje(
                        "ATENCIÓN",
                        response.mensaje || "Este asiento no puede ser modificado porque su fecha ha superado la fecha de control del ejercicio.",
                        function () {
                            $("#msjModal").modal("hide");
                        },
                        false,
                        ["Aceptar"],
                        "warn!",
                        null
                    );
                } else {
                    // Se permite modificar - proceder con la modificación
                    accion = AbmAction.MODIFICACION;
                    $("#divFiltro").collapse("hide");
                    accionBotones(accion);
                    activarControles(true);
                }
            },
            error: function (xhr, status, error) {
                CerrarWaiting();
                // Por defecto, mostrar un mensaje de error pero permitir continuar
                AbrirMensaje(
                    "ADVERTENCIA",
                    "No se pudo verificar si el asiento es modificable. ¿Desea continuar de todos modos?",
                    function (resp) {
                        if (resp === "SI") {
                            accion = AbmAction.MODIFICACION;
                            $("#divFiltro").collapse("hide");
                            accionBotones(accion);
                            activarControles(true);
                        }
                        $("#msjModal").modal("hide");
                    },
                    true, // Mostrar botones SI/NO
                    ["SI", "NO"],
                    "warn!",
                    null
                );
            }
        });
    } catch (error) {
        CerrarWaiting();
        ControlaMensajeError("Error al verificar permisos: " + error);
    }
}

/**
 * Ejecuta la anulación de un asiento definitivo, verificando primero los permisos
 */
function ejecutarAnulacionAsientoDef() {
    // Verificar que haya un asiento seleccionado
    if (!EntidadSelect) {
        AbrirMensaje("ATENCIÓN", "Debe seleccionar un asiento definitivo para anular.", function () {
            $("#msjModal").modal("hide");
        }, false, ["Aceptar"], "warn!", null);
        return;
    }

    // Mostrar indicador de espera mientras verificamos permisos
    AbrirWaiting("Verificando permisos de anulación...");

    // Verificar si el asiento es anulable (misma lógica que para modificación)
    try {
        // Obtener la fecha del asiento del DOM
        const fechaStr = $("#Dia_fecha").val();
        if (!fechaStr) {
            CerrarWaiting();
            ControlaMensajeError("No se pudo obtener la fecha del asiento para verificar permisos");
            return;
        }

        // Intentar parsear la fecha en varios formatos
        let fechaAsiento;

        // Verificar si la fecha viene en formato DD/MM/YYYY
        if (fechaStr.includes('/')) {
            const partes = fechaStr.split('/');
            if (partes.length === 3) {
                fechaAsiento = new Date(
                    parseInt(partes[2]), // año
                    parseInt(partes[1]) - 1, // mes (0-11)
                    parseInt(partes[0]) // día
                );
            }
        }
        // Si no tiene formato DD/MM/YYYY o el parsing falló, intentar con el constructor normal
        if (!fechaAsiento || isNaN(fechaAsiento.getTime())) {
            fechaAsiento = new Date(fechaStr);
        }

        // Verificar si la fecha es válida antes de continuar
        if (isNaN(fechaAsiento.getTime())) {
            CerrarWaiting();
            ControlaMensajeError("Fecha de asiento inválida: " + fechaStr);
            return;
        }

        // Ejercicio seleccionado
        const ejercicioId = $("#Eje_nro").val() || "0";

        // Crear objeto para enviar al servidor
        const data = {
            eje_nro: ejercicioId,
            dia_fecha: fechaAsiento.toISOString()
        };

        // Verificar permisos en el servidor
        $.ajax({
            url: verificarFechaModificacionUrl, // Misma URL que para modificación
            type: "POST",
            contentType: "application/json",
            data: JSON.stringify(data),
            success: function (response) {
                CerrarWaiting();

                if (response.permitido === false) {
                    // No se permite anular por fecha
                    AbrirMensaje(
                        "ATENCIÓN",
                        response.mensaje || "Este asiento no puede ser anulado porque su fecha ha superado la fecha de control del ejercicio.",
                        function () {
                            $("#msjModal").modal("hide");
                        },
                        false,
                        ["Aceptar"],
                        "warn!",
                        null
                    );
                } else {
                    // Se permite anular - confirmar la anulación
                    AbrirMensaje(
                        "CONFIRMACIÓN",
                        "¿Está seguro que desea anular el asiento definitivo seleccionado?<br>Esta acción no se puede deshacer.",
                        function (resp) {
                            if (resp === "SI") {
                                accion = AbmAction.BAJA;
                                $("#divFiltro").collapse("hide");
                                accionBotones(accion);

                                // No activar controles para edición en caso de anulación
                                activarControles(false);
                            }
                            $("#msjModal").modal("hide");
                        },
                        true, // Mostrar botones SI/NO
                        ["SI", "NO"],
                        "warn!",
                        null
                    );
                }
            },
            error: function (xhr, status, error) {
                CerrarWaiting();
                // Por defecto, mostrar un mensaje de error pero permitir continuar
                AbrirMensaje(
                    "ADVERTENCIA",
                    "No se pudo verificar si el asiento es anulable. ¿Desea continuar de todos modos?",
                    function (resp) {
                        if (resp === "SI") {
                            // Confirmar la anulación
                            AbrirMensaje(
                                "CONFIRMACIÓN",
                                "¿Está seguro que desea anular el asiento definitivo seleccionado?<br>Esta acción no se puede deshacer.",
                                function (resp2) {
                                    if (resp2 === "SI") {
                                        accion = AbmAction.BAJA;
                                        $("#divFiltro").collapse("hide");
                                        accionBotones(accion);
                                        activarControles(false);
                                    }
                                    $("#msjModal").modal("hide");
                                },
                                true, // Mostrar botones SI/NO
                                ["SI", "NO"],
                                "warn!",
                                null
                            );
                        }
                        $("#msjModal").modal("hide");
                    },
                    true, // Mostrar botones SI/NO
                    ["SI", "NO"],
                    "warn!",
                    null
                );
            }
        });
    } catch (error) {
        CerrarWaiting();
        ControlaMensajeError("Error al verificar permisos: " + error);
    }
}

/**
 * Función para presentar un reporte con los errores en html
 * Esta función es un placeholder - necesitarás implementarla según tus necesidades
 */
function presentarReporteErrores(detalles) {
    if (!detalles || detalles.length === 0) {
        AbrirMensaje("ATENCIÓN", "No hay errores para mostrar.", function () {
            $("#msjModal").modal("hide");
        }, false, ["Aceptar"], "warn!", null);
        return;
    }

    // Para una implementación inicial, podemos abrir una ventana con los detalles
    let contenido = "<h3>Detalle de Errores</h3><table class='table table-striped'>";
    contenido += "<thead><tr><th>Asiento</th><th>Mensaje de Error</th></tr></thead><tbody>";

    detalles.forEach(function (detalle) {
        contenido += `<tr><td>${detalle.asientoId}</td><td>${detalle.mensaje}</td></tr>`;
    });

    contenido += "</tbody></table>";

    // Crear una ventana emergente con el contenido HTML
    const ventana = window.open("", "_blank", "width=800,height=600");
    ventana.document.write(`
        <!DOCTYPE html>
        <html>
        <head>
            <title>Reporte de Errores</title>
            <link rel="stylesheet" href="https://cdn.jsdelivr.net/npm/bootstrap@5.1.3/dist/css/bootstrap.min.css">
            <style>
                body { padding: 20px; }
                h3 { margin-bottom: 20px; }
                .table { width: 100%; }
            </style>
        </head>
        <body>
            <div class="container">
                ${contenido}
                <div class="mt-4">
                    <button class="btn btn-primary" onclick="window.print()">Imprimir</button>
                    <button class="btn btn-secondary" onclick="window.close()">Cerrar</button>
                </div>
            </div>
        </body>
        </html>
    `);

    // En una implementación más avanzada, podrías llamar a un endpoint del controlador
    // que genere un PDF y lo descargue o abra en una nueva pestaña
    // window.open('/Asientos/AsientoTemporal/GenerarReporteErrores?ids=' + JSON.stringify(detalles), '_blank');
}

/**
 * Genera un informe PDF con los detalles de errores al enviar asientos a contabilidad
 * @param {Array} detalles - Lista de objetos con los detalles de los errores
 */
function generarReporteErrores(detalles) {
    if (!detalles || detalles.length === 0) {
        AbrirMensaje("ATENCIÓN", "No hay errores para generar el informe.", function () {
            $("#msjModal").modal("hide");
        }, false, ["Aceptar"], "warn!", null);
        return;
    }

    // Mostrar indicador de espera
    AbrirWaiting("Generando informe de errores...");

    // Convertir los detalles a JSON para pasarlo como parámetro
    const erroresJson = JSON.stringify(detalles);

    // Preparar el objeto de solicitud para el informe
    const reporteSolicitud = {
        Reporte: 9, // InfoReporte.R009_InfoErrAsTemp
        Parametros: {
            "errores": erroresJson,
            "fecha": new Date().toISOString()
        },
        Titulo: "Informe de Errores en Asientos Temporales",
        Observacion: "Detalle de errores al pasar asientos temporales a contabilidad",
        Formato: "P" // P = PDF
    };

    // Realizar la solicitud al servidor
    $.ajax({
        url: gestorImpresionUrl,
        type: "POST",
        contentType: "application/json",
        data: JSON.stringify(reporteSolicitud),
        success: function (response) {
            CerrarWaiting();

            if (response.error) {
                AbrirMensaje("ERROR", response.msg || "Error al generar el informe", function () {
                    $("#msjModal").modal("hide");
                }, false, ["Aceptar"], "error!", null);
                return;
            }

            if (response.warn) {
                AbrirMensaje("ADVERTENCIA", response.msg, function () {
                    if (response.auth) {
                        window.location.href = loginUrl; // Redirigir al login si es necesario
                    }
                    $("#msjModal").modal("hide");
                }, false, ["Aceptar"], "warn!", null);
                return;
            }

            // Si todo salió bien, mostrar el PDF
            const pdfData = response.base64;
            const fileName = response.name || "errores_asientos.pdf";

            // Crear un objeto Blob con los datos del PDF
            const blob = b64toBlob(pdfData, "application/pdf");

            // Crear una URL para el Blob
            const url = URL.createObjectURL(blob);

            // Abrir el PDF en una nueva ventana/pestaña
            window.open(url, "_blank");
        },
        error: function (xhr, status, error) {
            CerrarWaiting();
            AbrirMensaje("ERROR", "Error al generar el informe: " + error, function () {
                $("#msjModal").modal("hide");
            }, false, ["Aceptar"], "error!", null);
        }
    });
}

/**
 * Activa o desactiva los controles del asiento según el modo (alta/modificación/baja)
 * @param {boolean} act - Si es true activa los controles, si es false los desactiva
 */
function activarControles(act) {
    // Inicializar selector de cuentas si se están activando los controles
    if (act && typeof selectorCuentas !== 'undefined') {
        try {
            // Asegurarse de que las URLs están definidas
            if (typeof buscarPlanCuentasUrl === 'undefined' || typeof buscarCuentaUrl === 'undefined') {
                console.error("Las URLs para el selector de cuentas no están definidas");
            } else {
                selectorCuentas.inicializar(buscarPlanCuentasUrl, buscarCuentaUrl);
            }
        } catch (e) {
            console.error("Error al inicializar el selector de cuentas:", e);
        }
    }

    // Se invierte el valor porque 'disabled=true' significa desactivar
    // y queremos que act=true signifique habilitar controles
    const disabled = !act;

    // Header del asiento
    $("#Dia_tipo").prop("disabled", disabled);
    // El campo dia_movi (Nº Mov) siempre debe estar en modo solo lectura
    // ya que es la PK generada por la base de datos
    $(".card-header input.form-control:not(:first)").prop("readonly", disabled);

    // Añadir/eliminar líneas y botones operativos
    if (act) {
        // Eliminar manejadores de eventos existentes
        $(document).off("click", "#btnAddLinea");
        $(document).off("click", ".btn-eliminar-linea");

        // Si estamos activando los controles, añadimos botones de edición
        if ($("#btnAddLinea").length === 0) {
            // Si es nuevo o modificación, permitir editar líneas del asiento
            $("#tbAsientoDetalle thead tr").append(`<th style="width: 5%">Acciones</th>`);

            // Añadir un botón + en la primera celda de la última columna del thead
            $("#tbAsientoDetalle thead th:last-child").html(`
                <button id="btnAddLinea" class="btn btn-sm btn-success float-end" title="Nueva Linea">
                    <i class="bx bx-plus"></i>
                </button>
            `);

            $("#tbAsientoDetalle tbody tr").append(`
                <td class="text-center">
                    <button class="btn btn-sm btn-danger btn-eliminar-linea" title="Eliminar Linea">
                        <i class="bx bx-trash"></i>
                    </button>
                </td>
            `);

            // Mostrar botones de búsqueda de cuenta
            $("#tbAsientoDetalle tbody .btn-buscar-cuenta").show();
        }

        // Adjuntar eventos con delegación, pero asegurándose de que no haya duplicados
        $(document).on("click", "#btnAddLinea", function (e) {
            const nuevaLinea = `
                <tr class="">
                    <td contenteditable="true">
                        <div class="d-flex align-items-center">
                            <span class="cuenta-id"></span>
                            <button type="button" class="btn btn-sm btn-link text-primary btn-buscar-cuenta ms-1">
                                <i class="bx bx-search"></i>
                            </button>
                        </div>
                    </td>
                    <td class="cuenta-desc" contenteditable="false"></td>
                    <td class="linea-concepto" contenteditable="true"></td>
                    <td contenteditable="true" class="text-end">0,00</td>
                    <td contenteditable="true" class="text-end">0,00</td>
                    <td class="text-center">
                        <button class="btn btn-sm btn-danger btn-eliminar-linea">
                            <i class="bx bx-trash"></i>
                        </button>
                    </td>
                </tr>
            `;
            $("#tbAsientoDetalle tbody").append(nuevaLinea);
            actualizarTotales();
        });

        $(document).on("click", ".btn-eliminar-linea", function () {
            $(this).closest("tr").remove();
            actualizarTotales();
        });

        // Habilitar edición de datos de líneas
        $("#tbAsientoDetalle tbody").addClass("editable-grid");
        $("#tbAsientoDetalle tbody td:not(:last-child)").attr("contenteditable", true);

        // Configurar máscaras y validaciones para campos editables
        configurarCamposEditables();
    } else {
        // Si desactivamos, quitamos los controles de edición
        $("#tbAsientoDetalle thead th:last-child").remove();
        $("#tbAsientoDetalle tbody tr td:last-child").remove();
        $("#btnAddLinea").remove();

        // También eliminar los manejadores de eventos
        $(document).off("click", "#btnAddLinea");
        $(document).off("click", ".btn-eliminar-linea");

        // Deshabilitar edición
        $("#tbAsientoDetalle tbody").removeClass("editable-grid");
        $("#tbAsientoDetalle tbody td").removeAttr("contenteditable");

        // Ocultar botones de búsqueda de cuenta
        $("#tbAsientoDetalle tbody .btn-buscar-cuenta").hide();
    }

    // Control de botones de acción del footer
    $("#btnPasarContabilidad, #btnImprimir").prop("disabled", act); // Se deshabilitan al editar
}

/**
 * Configura los eventos para los botones de edición
 */
//function configurarEventosEdicion() {
//    // Primero, eliminar los manejadores de eventos existentes para evitar duplicación
//    $(document).off("click", "#btnAddLinea");
//    $(document).off("click", ".btn-eliminar-linea");

//    // Evento para agregar nueva línea
//    $(document).on("click", "#btnAddLinea", function () {
//        // Prevenir comportamiento predeterminado y propagación
//        e.preventDefault();
//        e.stopPropagation();

//        const nuevaLinea = `
//                <tr class="">
//                    <td contenteditable="true">
//                        <div class="d-flex align-items-center">
//                            <span class="cuenta-id"></span>
//                            <button type="button" class="btn btn-sm btn-link text-primary btn-buscar-cuenta ms-1">
//                                <i class="bx bx-search"></i>
//                            </button>
//                        </div>
//                    </td>
//                    <td class="cuenta-desc" contenteditable="false"></td>
//                    <td class="linea-concepto" contenteditable="true"></td>
//                    <td contenteditable="true" class="text-end">0,00</td>
//                    <td contenteditable="true" class="text-end">0,00</td>
//                    <td class="text-center">
//                        <button class="btn btn-sm btn-danger btn-eliminar-linea">
//                            <i class="bx bx-trash"></i>
//                        </button>
//                    </td>
//                </tr>
//            `;
//        $("#tbAsientoDetalle tbody").append(nuevaLinea);
//        actualizarTotales();
//    });

//    // Evento para eliminar línea
//    $(document).on("click", ".btn-eliminar-linea", function () {
//        // Prevenir comportamiento predeterminado y propagación
//        e.preventDefault();
//        e.stopPropagation();

//        $(this).closest("tr").remove();
//        actualizarTotales();
//    });
//}

/**
 * Configura los campos editables con máscaras y validaciones
 */
function configurarCamposEditables() {
    // Campos de importe (Debe y Haber)
    $("#tbAsientoDetalle tbody").on("focus", "td.text-end", function () {
        // Al entrar al campo, quitar formato pero mantener el punto decimal
        const valorOriginal = $(this).text().trim();

        try {
            // Quitar símbolo de moneda, espacios y comas (separadores de miles)
            let valorLimpio = valorOriginal.replace(/[$\s,]/g, "");

            // Si está vacío, poner 0
            if (!valorLimpio || valorLimpio === "") {
                valorLimpio = "0";
            }

            // Aplicar el valor limpio
            $(this).text(valorLimpio);

            // Seleccionar todo el contenido
            const range = document.createRange();
            range.selectNodeContents(this);
            const selection = window.getSelection();
            selection.removeAllRanges();
            selection.addRange(range);

            // Agregar clase para identificar que está siendo editado
            $(this).addClass("editando-importe");
        } catch (e) {
            console.error("Error al limpiar el formato:", e);
            $(this).text("0");

            // Seleccionar todo el contenido
            const range = document.createRange();
            range.selectNodeContents(this);
            const selection = window.getSelection();
            selection.removeAllRanges();
            selection.addRange(range);

            $(this).addClass("editando-importe");
        }
    });

    // Control de teclas permitidas: sólo números y un punto decimal
    $("#tbAsientoDetalle tbody").on("keypress", "td.text-end", function (e) {
        // Permitir teclas de control (flechas, backspace, etc.)
        if (e.ctrlKey || e.altKey || e.metaKey) {
            return true;
        }

        // Permitir números (0-9)
        if (/^[0-9]$/.test(e.key)) {
            return true;
        }

        // Permitir punto decimal (solo uno)
        if (e.key === '.' && $(this).text().indexOf('.') === -1) {
            return true;
        }

        // Si es coma, ignorarla (no queremos comas en la edición)
        if (e.key === ',') {
            e.preventDefault();
            return false;
        }

        // Bloquear cualquier otra entrada
        e.preventDefault();
        return false;
    });

    // Al perder el foco, formatear el número
    $("#tbAsientoDetalle tbody").on("blur", "td.text-end", function () {
        // Remover la clase de edición
        $(this).removeClass("editando-importe");

        try {
            // Obtener el valor actual
            let valor = $(this).text().trim();

            // Si está vacío, usar cero
            if (valor === "") {
                valor = "0";
            }

            // Asegurar que sea un número válido
            const numeroDecimal = !isNaN(parseFloat(valor)) ? parseFloat(valor) : 0;

            // Formatear con NumberFormat para consistencia (usando InvariantCulture - punto decimal)
            const formateado = numeroDecimal.toLocaleString('en-US', {
                style: 'currency',
                currency: 'USD', // No importa la moneda, solo el formato
                minimumFractionDigits: 2,
                maximumFractionDigits: 2
            }).replace('$', '$ '); // Ajustar el símbolo de moneda

            // Aplicar el formato
            $(this).text(formateado);
        } catch (e) {
            console.error("Error al formatear importe:", e);
            $(this).text("$ 0.00"); // Valor por defecto en caso de error
        }

        // Actualizar totales
        actualizarTotales();
    });
}


/**
 * Actualiza los totales del asiento
 */
function actualizarTotales() {
    let totalDebe = 0;
    let totalHaber = 0;

    // Sumar todos los valores de Debe y Haber
    $("#tbAsientoDetalle tbody tr").each(function () {
        // Extraer los valores de las celdas Debe y Haber
        const celdaDebe = $(this).find("td:nth-child(4)").text().trim();
        const celdaHaber = $(this).find("td:nth-child(5)").text().trim();

        // Limpiar el formato ($, espacios, comas de miles) y mantener el punto decimal
        const debeLimpio = celdaDebe.replace(/[$\s,]/g, "");
        const haberLimpio = celdaHaber.replace(/[$\s,]/g, "");

        // Convertir a número (decimal) - usar 0 si no es válido
        const debe = !isNaN(parseFloat(debeLimpio)) ? parseFloat(debeLimpio) : 0;
        const haber = !isNaN(parseFloat(haberLimpio)) ? parseFloat(haberLimpio) : 0;

        // Sumar a los totales
        totalDebe += debe;
        totalHaber += haber;
    });

    // Formatear los totales usando toLocaleString con formato en-US (punto decimal)
    const formateadoDebe = totalDebe.toLocaleString('en-US', {
        style: 'currency',
        currency: 'USD',
        minimumFractionDigits: 2,
        maximumFractionDigits: 2
    }).replace('$', '$ ');

    const formateadoHaber = totalHaber.toLocaleString('en-US', {
        style: 'currency',
        currency: 'USD',
        minimumFractionDigits: 2,
        maximumFractionDigits: 2
    }).replace('$', '$ ');

    // Actualizar los totales en el pie de tabla
    $("#tbAsientoDetalle tfoot tr:first-child td:nth-child(2)").text(formateadoDebe);
    $("#tbAsientoDetalle tfoot tr:first-child td:nth-child(3)").text(formateadoHaber);

    // Verificar si hay diferencia y actualizar la fila de diferencia
    const diferencia = Math.abs(totalDebe - totalHaber);
    const hayDiferencia = diferencia > 0.01; // Usar un umbral pequeño para evitar problemas de precisión

    // Si existe una fila de diferencia, actualizarla; si no, crearla si es necesario
    let filaDiferencia = $("#tbAsientoDetalle tfoot tr:nth-child(2)");

    const formateadoDiferencia = diferencia.toLocaleString('en-US', {
        style: 'currency',
        currency: 'USD',
        minimumFractionDigits: 2,
        maximumFractionDigits: 2
    }).replace('$', '$ ');

    if (hayDiferencia) {
        if (filaDiferencia.length === 0) {
            // Crear la fila si no existe
            const etiquetaDif = totalDebe > totalHaber ? "Diferencia en el DEBE" : "Diferencia en el HABER";
            filaDiferencia = $(`
                <tr>
                    <td colspan="3" class="text-end fw-bold">${etiquetaDif}</td>
                    <td colspan="2" class="text-end bg-black text-warning fw-bold">${formateadoDiferencia}</td>
                </tr>
            `);
            $("#tbAsientoDetalle tfoot").append(filaDiferencia);
        } else {
            // Actualizar la fila existente
            const etiquetaDif = totalDebe > totalHaber ? "Diferencia en el DEBE" : "Diferencia en el HABER";
            filaDiferencia.find("td:first-child").text(etiquetaDif);
            filaDiferencia.find("td:last-child").text(formateadoDiferencia);
        }

        // Añadir clase de estilo a los totales para resaltar el desbalance
        $("#tbAsientoDetalle tfoot tr:first-child td:nth-child(4), #tbAsientoDetalle tfoot tr:first-child td:nth-child(5)")
            .addClass("total-desbalanceado");
    } else {
        // Si no hay diferencia, eliminar la fila si existe
        filaDiferencia.remove();

        // Quitar clases de desbalance
        $("#tbAsientoDetalle tfoot tr:first-child td:nth-child(4), #tbAsientoDetalle tfoot tr:first-child td:nth-child(5)")
            .removeClass("total-desbalanceado");
    }
}


/**
 * Función para formatear decimales al cargar el asiento
 */
function formatearDecimalesAsiento() {
    // Aplica formato a las celdas con valores decimales que NO han sido formateadas aún
    $("#tbAsientoDetalle td.text-end:not([data-formatting-done='true'])").each(function () {
        const valor = $(this).text().trim();
        if (valor !== "") {
            try {
                // Limpiar el valor eliminando formato actual (pero manteniendo el punto decimal)
                const valorLimpio = valor
                    .replace(/[$\s,]/g, ""); // Quitar $ y espacios y comas

                // Convertir a número - si no es válido, usar 0
                const numeroDecimal = !isNaN(parseFloat(valorLimpio)) ? parseFloat(valorLimpio) : 0;

                // Usar toLocaleString para formato consistente (punto decimal)
                const formateado = numeroDecimal.toLocaleString('en-US', {
                    style: 'currency',
                    currency: 'USD',
                    minimumFractionDigits: 2,
                    maximumFractionDigits: 2
                }).replace('$', '$ ');

                // Aplicar el formato
                $(this).text(formateado);

                // Marcar como formateado
                $(this).attr("data-formatting-done", "true");
            } catch (e) {
                console.error("Error al formatear:", valor, e);
                $(this).text("$ 0.00");
            }
        }
    });

    // Similar para los totales en el pie de tabla 
    $("tfoot td.text-end:not([data-formatting-done='true'])").each(function () {
        const valor = $(this).contents().filter(function () {
            return this.nodeType === 3; // Nodo de texto
        }).text().trim();

        if (valor !== "") {
            try {
                // Limpiar el valor eliminando formato actual (pero manteniendo el punto decimal)
                const valorLimpio = valor
                    .replace(/[$\s,]/g, ""); // Quitar $ y espacios y comas

                // Convertir a número - si no es válido, usar 0
                const numeroDecimal = !isNaN(parseFloat(valorLimpio)) ? parseFloat(valorLimpio) : 0;

                // Usar toLocaleString para formato consistente (punto decimal)
                const formateado = numeroDecimal.toLocaleString('en-US', {
                    style: 'currency',
                    currency: 'USD',
                    minimumFractionDigits: 2,
                    maximumFractionDigits: 2
                }).replace('$', '$ ');

                // Aplicar el formato - reemplazando solo el nodo de texto
                $(this).contents().filter(function () {
                    return this.nodeType === 3;
                }).replaceWith(formateado);

                // Marcar como formateado
                $(this).attr("data-formatting-done", "true");
            } catch (e) {
                console.error("Error al formatear:", valor, e);
                $(this).contents().filter(function () {
                    return this.nodeType === 3;
                }).replaceWith("$ 0.00");
            }
        }
    });
}


/**
 * Confirma la operación de ABM del asiento (Alta, Baja, Modificación)
 */
function confirmarOperacionAsientoDef() {
    AbrirWaiting("Procesando operación de asiento definitivo...");
    try {
        // Recopilamos los datos del asiento según la estructura requerida
        const asientoData = recopilarDatosAsientoDef();

        // Validar los datos básicos del asiento
        if (!validarAsientoDef(asientoData.Detalles)) {
            CerrarWaiting();
            return;
        }

        // Construir el objeto AsientoAccionDto
        const datos = {
            asiento: asientoData,
            accion: accion.toString().charAt(0) // Asegurar que sea un solo carácter
        };

        console.log("Datos a enviar:", JSON.stringify(datos));

        // Hacemos la llamada al servicio
        $.ajax({
            url: confirmarAsientoDefinitivoUrl,
            type: "POST",
            contentType: "application/json",
            data: JSON.stringify(datos),
            success: function (obj) {
                CerrarWaiting();

                if (obj.error === true) {
                    // Mostrar mensaje de error
                    AbrirMensaje("ERROR", obj.msg || "Ocurrió un error al procesar el asiento Definitivo.", function () {
                        $("#msjModal").modal("hide");
                    }, false, ["Aceptar"], "error!", null);
                } else if (obj.warn === true) {
                    // Mostrar advertencia
                    AbrirMensaje("ATENCIÓN", obj.msg, function () {
                        if (obj.auth === true) {
                            window.location.href = loginUrl; // Redirigir al login si es necesario
                        } else {
                            $("#msjModal").modal("hide");
                            if (obj.focus) {
                                $(`#${obj.focus}`).focus();
                            }
                        }
                    }, false, ["Continuar"], "warn!", null);
                } else {
                    // Operación exitosa
                    AbrirMensaje("ÉXITO", obj.msg || "Operación realizada con éxito.", function () {
                        $("#msjModal").modal("hide");
                        InicializaVistaAsientos(); // Cierra el panel de edición y limpia la vista
                        // Refrescar la grilla
                        buscarAsientosDefs(1); // Llama a la función que recarga la grilla
                    }, false, ["Aceptar"], "succ!", null);
                }
            },
            error: function (xhr, status, error) {
                CerrarWaiting();
                console.error("Error al confirmar asiento:", error);
                console.error("Respuesta del servidor:", xhr.responseText);
                AbrirMensaje("ERROR", "Error al procesar la operación: " + error, function () {
                    $("#msjModal").modal("hide");
                }, false, ["Aceptar"], "error!", null);
            }
        });
    } catch (error) {
        CerrarWaiting();
        console.error("Error al preparar datos del asiento:", error);
        AbrirMensaje("ERROR", "Ocurrió un error al procesar los datos del asiento: " + error.message, function () {
            $("#msjModal").modal("hide");
        }, false, ["Aceptar"], "error!", null);
    }

}


/**
 * Recopila los datos del asiento definitivo desde el formulario
 * @returns {Object} Objeto AsientoDetalleDto con los datos del asiento
 */
function recopilarDatosAsientoDef() {
    // Obtener valores comunes a todas las líneas
    const ejercicioId = parseInt($("#Eje_nro").val()) || 0;
    const asientoId = EntidadSelect || "";
    const descripcion = $("#Dia_desc_asiento").val() || "";

    // Procesar la fecha correctamente
    const fechaStr = $("#Dia_fecha").val() || "";
    let fecha;

    try {
        // Intentar detectar el formato de la fecha
        if (fechaStr.includes('/')) {
            // Formato DD/MM/YYYY
            const partes = fechaStr.split('/');
            if (partes.length === 3) {
                fecha = new Date(
                    parseInt(partes[2]), // año
                    parseInt(partes[1]) - 1, // mes (0-11)
                    parseInt(partes[0]) // día
                );
            }
        } else {
            // Intentar con el constructor normal
            fecha = new Date(fechaStr);
        }

        // Verificar que la fecha sea válida
        if (isNaN(fecha.getTime())) {
            console.warn("Fecha inválida, usando fecha actual:", fechaStr);
            fecha = new Date(); // Usar fecha actual como fallback
        }
    } catch (e) {
        console.error("Error al procesar la fecha:", e);
        fecha = new Date(); // Usar fecha actual como fallback
    }

    const tipoAsiento = $("#Dia_tipo").val() || "";
    const listaAsiento = $("#Dia_lista").val() || "";

    // Array para almacenar las líneas del asiento
    const lineasDetalle = [];

    // Variables para calcular totales
    let totalDebe = 0;
    let totalHaber = 0;

    // Recopilar datos de cada fila
    $("#tbAsientoDetalle tbody tr").each(function (index) {
        const $fila = $(this);

        // Obtener los valores de cada celda
        const cuentaId = $fila.find(".cuenta-id").text().trim();
        const cuentaDesc = $fila.find(".cuenta-desc").text().trim();
        const descripcionLinea = $fila.find("td:nth-child(3)").text().trim();

        // Calcular el importe
        const debeStr = $fila.find("td:nth-child(4)").text().trim();
        const haberStr = $fila.find("td:nth-child(5)").text().trim();
        const debe = convertirMonedaANumero(debeStr);
        const haber = convertirMonedaANumero(haberStr);

        // Acumular totales
        totalDebe += debe;
        totalHaber += haber;

        // Crear objeto de línea según AsientoLineaDto
        const linea = {
            "Dia_movi": asientoId,
            "Dia_nro": index + 1,
            "Ccb_id": cuentaId,
            "Ccb_desc": cuentaDesc,
            "Dia_desc": descripcionLinea,
            "Debe": debe,
            "Haber": haber
        };

        // Agregar la línea al array de detalles
        lineasDetalle.push(linea);
    });

    // Construir el objeto AsientoDetalleDto completo
    const asientoDetalle = {
        "eje_nro": ejercicioId,
        "Dia_movi": asientoId,
        "Dia_fecha": fecha.toISOString(), // Usar formato ISO para la fecha
        "Dia_tipo": tipoAsiento,
        "Dia_lista": listaAsiento,
        "Dia_desc_asiento": descripcion,
        "TotalDebe": totalDebe,
        "TotalHaber": totalHaber,
        "Detalles": lineasDetalle
    };

    return asientoDetalle;
}


/**
 * Valida los datos del asiento definitivo antes de enviarlos al servidor
 * @param {Array} lineasAsiento - Array de objetos que representan las líneas del asiento
 * @returns {boolean} True si los datos son válidos, false en caso contrario
 */
function validarAsientoDef(lineasAsiento) {
    // Validar que haya al menos dos líneas
    if (!lineasAsiento || lineasAsiento.length < 2) {
        AbrirMensaje("VALIDACIÓN", "El asiento debe tener al menos dos líneas (débito y crédito).", function () {
            $("#msjModal").modal("hide");
        }, false, ["Aceptar"], "warn!", null);
        return false;
    }

    // Validar que cada línea tenga una cuenta asignada
    for (let i = 0; i < lineasAsiento.length; i++) {
        if (!lineasAsiento[i].Ccb_id) {
            AbrirMensaje("VALIDACIÓN", `La línea ${i + 1} debe tener una cuenta contable asignada.`, function () {
                $("#msjModal").modal("hide");
            }, false, ["Aceptar"], "warn!", null);
            return false;
        }
    }

    // Calcular saldo total
    let saldoTotal = 0;
    for (let i = 0; i < lineasAsiento.length; i++) {
        saldoTotal += (lineasAsiento[i].Debe - lineasAsiento[i].Haber);
    }

    // Validar que el saldo sea cero (con margen de error para decimales)
    if (Math.abs(saldoTotal) > 0.01) {
        AbrirMensaje("VALIDACIÓN", "El saldo del asiento debe ser cero. Revise los importes de débito y crédito.", function () {
            $("#msjModal").modal("hide");
        }, false, ["Aceptar"], "warn!", null);
        return false;
    }

    return true;
}


/**
 * Convierte un string con formato de moneda a un número decimal
 * @param {string} moneda - String con formato de moneda (ej: "$ 1.234,56" o "1.234.56")
 * @returns {number} Valor numérico (ej: 1234.56)
 */
function convertirMonedaANumero(moneda) {
    if (!moneda || moneda === "0.00" || moneda === "0") {
        return 0;
    }

    try {
        // Eliminar símbolo de moneda, espacios y comas (separadores de miles)
        let valor = moneda.replace(/[$\s,]/g, "");

        // Convertir a número
        const numero = parseFloat(valor);

        // Devolver 0 si no es un número válido
        return isNaN(numero) ? 0 : numero;
    } catch (e) {
        console.error("Error al convertir moneda a número:", e, moneda);
        return 0;
    }
}

/**
 * Convierte una fecha al formato esperado por el servidor (ISO string)
 * @param {string|Date} fecha - Fecha a formatear
 * @returns {string} Fecha en formato ISO (YYYY-MM-DDTHH:mm:ss.sssZ)
 */
function obtenerFechaFormateada(fecha) {
    if (!fecha) {
        return new Date().toISOString();
    }

    try {
        // Si es un string en formato dd/mm/yyyy, convertirlo a Date
        if (typeof fecha === "string" && fecha.includes("/")) {
            const partes = fecha.split("/");
            if (partes.length === 3) {
                const fechaObj = new Date(
                    parseInt(partes[2]), // año
                    parseInt(partes[1]) - 1, // mes (0-11)
                    parseInt(partes[0]) // día
                );

                // Verificar que la fecha sea válida
                if (!isNaN(fechaObj.getTime())) {
                    return fechaObj.toISOString();
                }
            }
        }

        // Si ya es un objeto Date, verificar que sea válido antes de convertirlo
        if (fecha instanceof Date) {
            if (!isNaN(fecha.getTime())) {
                return fecha.toISOString();
            } else {
                console.warn("Fecha inválida, usando fecha actual");
                return new Date().toISOString();
            }
        }

        // Si es otro formato, intentar crear un Date y convertirlo
        const nuevaFecha = new Date(fecha);
        if (!isNaN(nuevaFecha.getTime())) {
            return nuevaFecha.toISOString();
        } else {
            console.warn("No se pudo parsear la fecha, usando fecha actual:", fecha);
            return new Date().toISOString();
        }
    } catch (e) {
        console.error("Error al formatear fecha:", e, fecha);
        return new Date().toISOString(); // Devolver fecha actual en caso de error
    }
}




/**
 * Inicializa la vista de asientos y configura los componentes
 * @param {any} e - Evento opcional
 */
function InicializaVistaAsientos(e) {
    // Si NO es la primera ejecución y no hay acción pendiente, limpiar y regresar
    if (!primerArranque && accion === "") {
        // Oculta el panel del asiento
        $("#divpanel01").empty();

        // Limpiar selecciones y ocultar checkboxes
        limpiarSeleccionAsientos();

        // Configuración de componentes
        configurarComponentesIniciales();

        // Mostrar el filtro si no hay datos en la grilla
        mostrarFiltroSiNecesario();

        // Configurar botones y grilla
        accionBotones(AbmAction.CANCEL); // Usar tercer parámetro para mantener el botón cancelar
        removerSeleccion();
        activarGrilla(Grids.GridAsiento);

        CerrarWaiting();
        return; // Importante: salir de la función para evitar recursión
    }

    // Primera ejecución o acción pendiente
    primerArranque = false; // Marcar que ya no es la primera ejecución

    // Limpiar selecciones y ocultar checkboxes
    limpiarSeleccionAsientos();

    // Configuración de componentes
    configurarComponentesIniciales();

    // Mostrar el filtro si no hay datos en la grilla
    mostrarFiltroSiNecesario();

    // Configurar botones y grilla
    accionBotones(AbmAction.CANCEL); // Usar tercer parámetro para mantener el botón cancelar
    removerSeleccion();
    activarGrilla(Grids.GridAsiento);

    //resguardo los parametros para el reporte #9 = i-1
    // Obtenemos los valores de los campos del filtro
    let i = 9;
    let admId = administracion;
    if (arrRepoParams[i - 1] === undefined) {
        var data1 = {
            Eje_nro: $("#Eje_nro").val(), // Ejercicio
            Movi: $("#Movi").is(":checked").toString(), // bit Movimiento
            Movi_like: $("#Movi_like").val(), // Nro Movimiento
            Usu: $("#Usu").is(":checked").toString(), // bit Usuario
            Usu_like: $("#Usu_like").val(), // Usuario
            Tipo: $("#Tipo").is(":checked").toString(), // bit Tipo
            Tipo_like: $("#Tipo_like").val(), // Tipo de Asiento
            Rango: $("#Rango").is(":checked").toString(), // bit Rango
            Desde: $("input[name='Desde']").val(), // Fecha Desde
            Hasta: $("input[name='Hasta']").val() // Fecha Hasta
        };
        //let admId = administracion;
        //agregando los parametros de la busqueda realizada
        cargarReporteEnArre(9, data1, "Informe de Asientos Solicitados.", "Obseración:", administracion)
    }
    i++;
    //reseteo los parametros del detalle de asiento (reporte #10)
    if (arrRepoParams[i - 1] !== undefined) {
        ReporteResetCeldaEnArre(i);
    }

    CerrarWaiting();
}

/**
 * Limpia todas las selecciones de asientos y oculta los checkboxes
 */
function limpiarSeleccionAsientos() {
    // Limpiar las variables de selección
    filaClicDoble = null;
    filasSeleccionadas = [];

    // Quitar todas las clases de selección
    $("#tbGridAsiento tbody tr").removeClass("selected-simple selected-multiple selectedEdit-row");

    // Desmarcar todos los checkboxes
    $(".asiento-checkbox").prop("checked", false);
    $("#selectAllAsientos").prop("checked", false);

    // Deshabilitar botón de pasar a contabilidad
    $("#btnPasarConta").prop("disabled", true);

    // Actualizar contador de seleccionados
    $("#selected-count").text("0");
}

/**
 * Configura los componentes iniciales de la vista
 */
function configurarComponentesIniciales() {
    // Configurar checkbox de ejercicio según el modo
    if (typeof asientoTemporal !== 'undefined' && asientoTemporal === true) {
        $('#chkEjercicio').prop('checked', true).prop('disabled', true);
        $("#Eje_nro").prop("disabled", false);
    } else {
        toggleComponent('chkEjercicio', '#Eje_nro');
    }

    // Configurar otros componentes
    toggleComponent('Movi', '#Movi_like');
    toggleComponent('Usu', '#Usu_like');
    toggleComponent('Tipo', '#Tipo_like');
    toggleComponent('Rango', 'input[name="Desde"]');
    toggleComponent('Rango', 'input[name="Hasta"]');
}

/**
 * Muestra el filtro si la grilla no tiene datos
 */
function mostrarFiltroSiNecesario() {
    if ($("#divDetalle").is(":visible")) {
        $("#divDetalle").collapse("hide");
    }

    var nng = "#" + Grids.GridAsiento;
    var tb = $(nng + " tbody tr");
    if (tb.length === 0) {
        $("#divFiltro").collapse("show");
    }
}

/**
 * Remueve la selección de todos los registros de la grilla
 */
function removerSeleccion() {
    $("#" + Grids.GridAsiento + " tbody tr").each(function (index) {
        $(this).removeClass("selectedEdit-row");
    });
}

function ejecutaDblClickGridAsiento(x) {
    AbrirWaiting("Espere mientras se busca el Asiento solicitado...");
    selectAsientoDbl(x, Grids.GridAsiento);
}

// Modificar la función selectAsientoDbl para que no elimine otras selecciones
function selectAsientoDbl(x, gridId) {
    // Ya no eliminamos otras selecciones aquí
    // $("#" + gridId + " tbody tr").each(function (index) {
    //     $(this).removeClass("selectedEdit-row");
    // });

    // Solo seleccionamos la actual si no está seleccionada
    if (!$(x).hasClass("selectedEdit-row")) {
        $(x).addClass("selectedEdit-row");
    }

    var id = x.find("td:nth-child(2)").text().trim();

    //reporte #10
    //impresion del asiento
    let i = 10;
    if (arrRepoParams[i - 1] === undefined) {
        let data = { "dia_movi": id };
        cargarReporteEnArre(10, data, "Detalle de Asiento Movimiento N°: " + id, "", administracion);
    }

    //se agrega por inyection el tab con los datos del producto
    EntidadEstado = x.find("td:nth-child(4)").text().trim();
    var data = { id: id };
    EntidadSelect = id;
    desactivarGrilla(gridId);
    //se busca el perfil
    buscarAsientoDef(data);
    //se posiciona el registro seleccionado
    posicionarRegOnTop(x);
}

//function selectAsientoDbl(x, gridId) {
//    $("#" + gridId + " tbody tr").each(function (index) {
//        $(this).removeClass("selectedEdit-row");
//    });
//    $(x).addClass("selectedEdit-row");
//    var id = x.find("td:nth-child(1)").text();

//    //se agrega por inyection el tab con los datos del producto
//    EntidadEstado = x.find("td:nth-child(3)").text();
//    var data = { id: id };
//    EntidadSelect = id;
//    desactivarGrilla(gridId);
//    //se busca el perfil
//    buscarAsientoDef(data);
//    //se posiciona el registro seleccionado
//    posicionarRegOnTop(x);

//}

function buscarAsientoDef(data) {
    //se busca el asiento definitivo
    PostGenHtml(data, buscarAsientoDefUrl, function (obj) {
        $("#divpanel01").html(obj);

        // Marcar los elementos para evitar doble formateo
        $("#tbAsientoDetalle td.text-end").attr("data-formatting-done", "true");

        // Aplica formato a los decimales
        formatearDecimalesAsiento();

        $("#btnDetalle").prop("disabled", false);
        $("#divFiltro").collapse("hide");
        $("#divDetalle").collapse("show");

        //activar botones de acción para asientos definitivos (solo modificar y anular)
        $("#btnAbmNuevo").hide(); // Ocultar botón nuevo
        $("#btnAbmModif").show().prop("disabled", false);
        $("#btnAbmElimi").show().prop("disabled", false);        

        CerrarWaiting();
    });
}

/**
 * Verifica si el asiento definitivo puede ser modificado basado en su fecha
 * y la fecha de control del ejercicio
 */
function verificarPermisosModificacionAsiento() {
    try {
        // Obtener la fecha del asiento y convertirla a formato adecuado
        const fechaStr = $("#Dia_fecha").val();
        if (!fechaStr) return;

        // Convertir formato DD/MM/YYYY a objeto Date
        let fechaAsiento;
        if (fechaStr.includes('/')) {
            const partes = fechaStr.split('/');
            if (partes.length === 3) {
                fechaAsiento = new Date(
                    parseInt(partes[2]), // año
                    parseInt(partes[1]) - 1, // mes (0-11)
                    parseInt(partes[0]) // día
                );
            }
        } else {
            fechaAsiento = new Date(fechaStr);
        }

        // Verificar que la fecha sea válida
        if (isNaN(fechaAsiento.getTime())) {
            console.warn("Fecha inválida:", fechaStr);
            return;
        }

        // Continuar con la verificación usando la fecha correcta
        const ejercicioId = $("#Eje_nro").val() || "0";

        const data = {
            eje_nro: ejercicioId,
            dia_fecha: fechaAsiento.toISOString()
        };

        $.ajax({
            url: verificarFechaModificacionUrl, // Esta URL debe definirse en la vista
            type: "POST",
            contentType: "application/json",
            data: JSON.stringify(data),
            success: function (response) {
                if (response.permitido === false) {
                    // No se permite modificar por fecha
                    $("#btnAbmModif").prop("disabled", true);

                    // Agregar mensaje informativo
                    if ($("#fecha-advertencia").length === 0) {
                        const mensaje = `
                        <div id="fecha-advertencia" class="alert alert-warning mb-2 py-1 small">
                            <i class="bx bx-info-circle"></i> 
                            ${response.mensaje || "Este asiento no puede ser modificado porque su fecha ha superado la fecha de control del ejercicio."}
                        </div>
                    `;
                        $("#divpanel01").prepend(mensaje);
                    }
                }
            },
            error: function () {
                // Por defecto, permitir modificación si no podemos verificar
                console.warn("No se pudo verificar la validez de la fecha para modificación");
            }
        });
    } catch (error) {
        ControlaMensajeError("Hubo un problema en la verificación de permisos:" + error);
    }
}

// Agregar un mensaje informativo al inicio de la grilla
function agregarMensajeSeleccion() {
    if ($("#seleccion-info").length === 0) {
        const mensaje = `
            <div id="seleccion-info" class="alert alert-info mb-2 py-1 small">
                <i class="bx bx-info-circle"></i> 
                Para seleccionar varios asientos: haga clic en cada fila o use Ctrl+clic para selección múltiple.
                <span class="badge bg-primary ms-1">${$("#tbGridAsiento tbody tr.selectedEdit-row").length}</span> asiento(s) seleccionado(s).
            </div>
        `;
        $("#tbGridAsiento").before(mensaje);
    } else {
        // Actualizar contador
        $("#seleccion-info .badge").text($("#tbGridAsiento tbody tr.selectedEdit-row").length);
    }
}




//// Función genérica para habilitar/deshabilitar componentes
//function toggleComponent(checkboxId, componentSelector) {
//    try {
//        const isChecked = $(`#${checkboxId}`).is(':checked');
//        const $component = $(componentSelector);

//        if (isChecked) {
//            $component.prop('disabled', false).css({
//                'background-color': '',
//                'font-weight': 'normal'
//            });
//        } else {
//            $component.prop('disabled', true).css({
//                'background-color': 'rgb(251,255,195)',
//                'font-weight': '900'
//            });
//        }
//    } catch (error) {
//        console.error(`Error al procesar el checkbox ${checkboxId}:`, error);
//    }
//}

/**
 * Maneja la impresión de asientos
 */
function imprimirAsiento() {
    let data = { modulo: "", parametros: [] }
    invocacionGestorDoc(data);
}
//function imprimirAsiento() {
//    // Caso 1: Si hay un asiento abierto (seleccionado con doble clic)
//    if (filaClicDoble !== null) {
//        // Obtener el ID del asiento abierto
//        const asientoId = EntidadSelect;
//        if (asientoId) {
//            generarReporteAsiento(asientoId);
//            return;
//        }
//    }

//    // Caso 2: Si hay asientos seleccionados en la grilla
//    if (filasSeleccionadas.length === 1) {
//        // Hay un solo asiento seleccionado, obtener su ID
//        const asientoId = filasSeleccionadas[0];
//        generarReporteAsiento(asientoId);
//    } else if (filasSeleccionadas.length > 1) {
//        // Hay múltiples asientos seleccionados
//        AbrirMensaje(
//            "ATENCIÓN",
//            "Hay múltiples asientos seleccionados. Por favor, deseleccione todos y seleccione solo uno para imprimir.",
//            function () {
//                $("#msjModal").modal("hide");
//            },
//            false,
//            ["Aceptar"],
//            "warn!",
//            null
//        );
//    } else {
//        // No hay ningún asiento seleccionado
//        AbrirMensaje(
//            "ATENCIÓN",
//            "No hay ningún asiento seleccionado para imprimir. Por favor, seleccione uno.",
//            function () {
//                $("#msjModal").modal("hide");
//            },
//            false,
//            ["Aceptar"],
//            "warn!",
//            null
//        );
//    }
//}

/**
 * Genera el reporte PDF de un asiento específico
 * @param {string} asientoId - ID del asiento a imprimir (dia_movi)
 */
function generarReporteAsiento(asientoId) {
    // Mostrar indicador de espera
    AbrirWaiting("Generando informe del asiento...");

    // Preparar el objeto de solicitud para el informe
    const reporteSolicitud = {
        Reporte: 9,
        Parametros: {
            "dia_movi": asientoId
        },
        Titulo: "Informe de Asiento Temporal",
        Observacion: "Detalle del asiento temporal",
        Formato: "P" // P = PDF
    };

    // Realizar la solicitud al servidor
    $.ajax({
        url: gestorImpresionUrl,
        type: "POST",
        contentType: "application/json",
        data: JSON.stringify(reporteSolicitud),
        success: function (response) {
            CerrarWaiting();

            if (response.error) {
                AbrirMensaje("ERROR", response.msg || "Error al generar el informe", function () {
                    $("#msjModal").modal("hide");
                }, false, ["Aceptar"], "error!", null);
                return;
            }

            if (response.warn) {
                AbrirMensaje("ADVERTENCIA", response.msg, function () {
                    if (response.auth) {
                        window.location.href = loginUrl; // Redirigir al login si es necesario
                    }
                    $("#msjModal").modal("hide");
                }, false, ["Aceptar"], "warn!", null);
                return;
            }

            // Si todo salió bien, mostrar el PDF
            const pdfData = response.base64;

            // Crear un objeto Blob con los datos del PDF
            const blob = b64toBlob(pdfData, "application/pdf");

            // Crear una URL para el Blob
            const url = URL.createObjectURL(blob);

            // Abrir el PDF en una nueva ventana/pestaña
            window.open(url, "_blank");
        },
        error: function (xhr, status, error) {
            CerrarWaiting();
            AbrirMensaje("ERROR", "Error al generar el informe: " + error, function () {
                $("#msjModal").modal("hide");
            }, false, ["Aceptar"], "error!", null);
        }
    });
}

/**
 * Analiza el estado del botón Detalle y realiza las acciones correspondientes
 * Si hay un asiento abierto, lo cierra y limpia el panel
 * @returns {boolean} - Siempre devuelve true para permitir que el evento continúe
 */
function analizaEstadoBtnDetalle() {
    // Verificar si hay un asiento abierto (panel de detalle visible)
    if ($("#divDetalle").is(":visible") && $("#divpanel01").children().length > 0) {
        // Hay un asiento abierto, limpiarlo y cerrar el panel
        limpiarAsientoAbierto();
    }

    // Permitir que el evento siga propagándose (para que funcione el collapse)
    return true;
}

/**
 * Limpia el asiento abierto actualmente y restablece la interfaz
 */
function limpiarAsientoAbierto() {
    // Vaciar el panel que contiene el asiento
    $("#divpanel01").empty();

    // Restablecer variables de control
    filaClicDoble = null;
    EntidadSelect = "";
    EntidadEstado = "";

    InicializaVistaAsientos();


    //// Cerrar el panel de detalle y mostrar el de filtro
    //$("#divDetalle").collapse("hide");
    //$("#divFiltro").collapse("show");

    //// Reactivar la grilla si estaba desactivada
    //activarGrilla(Grids.GridAsiento);

    //// Quitar selección visual en la grilla
    //removerSeleccion();

    //// Actualizar estado de los botones
    //$("#btnDetalle").prop("disabled", true);
    //activarBotones2(false);
}