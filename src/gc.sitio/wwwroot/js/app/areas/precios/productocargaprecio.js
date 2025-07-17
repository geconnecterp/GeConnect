$(function () {
    configurarBotonesProdCP();
    cargaEventosCP();
});

function cargaEventosCP() {
    // Observar la adición de elementos mediante MutationObserver
    const listObserver = new MutationObserver(function (mutations) {
        mutations.forEach(function (mutation) {
            if (mutation.type === 'childList' && mutation.addedNodes.length > 0) {
                verificarYDesactivarControles();
            }
        });
    });

    // Configurar y comenzar la observación si el elemento existe
    if (document.getElementById('Rel01List')) {
        listObserver.observe(document.getElementById('Rel01List'), {
            childList: true,
            subtree: true
        });
    }

    // Manejar los eventos específicos para el control de lista (solo change)
    $("#Rel01List").on("change", function () {
        verificarYDesactivarControles();
    });

    // Si el autocompletado de Rel01 selecciona un ítem
    $("#Rel01").on("autocompleteselect", function () {
        // Esperar brevemente para que el autocompletado actualice la lista
        setTimeout(verificarYDesactivarControles, 100);
    });

    // Verificar también después de que el documento está completamente cargado
    $(function () {
        verificarYDesactivarControles();
    });

    // Verificación periódica más controlada (con menos ruido en la consola)
    let checkCount = 0;
    const intervalCheck = setInterval(function () {
        // Verificar solo si aún no hay elementos y no estamos deshabilitados
        if ($("#Rel01List").find("option").length === 0 && !$("#Rel01").prop("disabled")) {
            checkCount++;
            verificarYDesactivarControles(false); // Pasar false para no loguear en cada intento
        } else {
            // Si ya encontramos elementos o ya están deshabilitados, parar el intervalo
            clearInterval(intervalCheck);
        }

        // Detener después de 10 intentos incluso si no encontramos nada
        if (checkCount >= 10) {
            clearInterval(intervalCheck);
        }
    }, 500);

    // Extender la funcionalidad del evento click en chkRel01
    // Este evento ya está definido en siteGen.js, pero necesitamos añadir más comportamiento
    $("#chkRel01").on("change", function () {
        // Si el checkbox se desmarca, desactivar y limpiar los controles de Rel03
        if (!$(this).is(":checked")) {
            // Desactivar y desmarcar chkRel03
            $("#chkRel03").prop("checked", false);
            $("#chkRel03").prop("disabled", true);

            // Limpiar y desactivar Rel03
            $("#Rel03").val("");
            $("#Rel03").prop("disabled", true);

            // Limpiar y desactivar Rel03List
            $("#Rel03List").empty();
            $("#Rel03List").prop("disabled", true);

            console.log("Se ha desactivado el filtro de proveedor y se ha limpiado el filtro de familia");
        }
        // Si se marca, no hacemos nada especial aquí, el código existente ya maneja ese caso
    });

    // Evento change para el combo Rel03
    $("#Rel03").on("change", function () {
        const selectedValue = $(this).val();
        const selectedText = $(this).find("option:selected").text();

        if (selectedValue && selectedValue !== "") {
            // Agregar la opción seleccionada a Rel03List si no existe ya
            if ($("#Rel03List option[value='" + selectedValue + "']").length === 0) {
                $("#Rel03List").append(
                    $("<option></option>")
                        .attr("value", selectedValue)
                        .text(selectedText)
                        .prop("selected", true)
                );

                console.log("Familia seleccionada agregada a la lista: " + selectedText);

                // También guardar el valor en el campo oculto Rel03Item si existe
                if ($("#Rel03Item").length > 0) {
                    $("#Rel03Item").val(selectedValue);
                }
            }

            // Limpiar la selección en el combo original después de agregarla a la lista
            $(this).val("");
        }
    });

    // Evento change para el checkbox chkFile
    $("#chkFile").on("change", function () {
        // Si el checkbox se activa, desactivar todos los controles excepto los relacionados con Rel01
        if ($(this).is(":checked")) {
            // Guardar el estado actual de los controles Rel01 antes de desactivar todo
            const rel01Checked = $("#chkRel01").is(":checked");
            const rel01Disabled = $("#Rel01").prop("disabled");
            const rel01ListDisabled = $("#Rel01List").prop("disabled");
            const rel01Value = $("#Rel01").val();
            const rel01ItemValue = $("#Rel01Item").val();
            const rel01ListOptions = $("#Rel01List").html();

            // Desactivar todos los checkboxes excepto chkFile y chkRel01
            $("input[type='checkbox']").not("#chkFile, #chkRel01").prop({
                "checked": false,
                "disabled": true
            });

            // Desactivar todos los inputs de texto excepto Rel01
            $("input[type='text']").not("#Rel01").prop("disabled", true);

            // Desactivar todos los select excepto Rel01List
            $("select").not("#Rel01List").prop("disabled", true).empty();


            // Limpiar específicamente los controles de Rel02
            $("#Rel02").val("");
            $("#Rel02Item").val("");
            $("#Rel02List").empty();
            $("#chkRel02").prop("checked", false);
            $("#chkRel02").prop("disabled", true);

            // Restaurar el estado de los controles Rel01
            $("#chkRel01").prop("checked", rel01Checked);

            // Solo si Rel01 no estaba desactivado previamente, lo dejamos activo
            if (!rel01Disabled) {
                $("#Rel01").prop("disabled", false);
            }

            // Solo si Rel01List no estaba desactivado previamente, lo dejamos activo
            if (!rel01ListDisabled) {
                $("#Rel01List").prop("disabled", false);
            }

            console.log("Modo archivo activado: Solo se permite filtrar por proveedor");
        } else {
            // Al desactivar el checkbox, restaurar el comportamiento normal
            // Primero habilitamos los controles básicos
            $("input[type='checkbox']").not("#chkFile").prop("disabled", false);

            // Luego verificamos la lógica de negocio específica
            if ($("#chkRel01").is(":checked") && $("#Rel01List").find("option").length > 0) {
                // Si hay un proveedor seleccionado, habilitar filtro de familia
                $("#chkRel03").prop("disabled", false);

                // Si el filtro de familia está activado, habilitar sus controles
                if ($("#chkRel03").is(":checked")) {
                    $("#Rel03").prop("disabled", false);
                    $("#Rel03List").prop("disabled", false);
                }
            } else {
                // Si no hay proveedor, deshabilitar familia
                $("#chkRel03").prop("disabled", true);
                $("#Rel03").prop("disabled", true);
                $("#Rel03List").prop("disabled", true);
            }


            // Habilitar controles de Rel02 (Rubro)
            $("#chkRel02").prop("disabled", false);
            if ($("#chkRel02").is(":checked")) {
                $("#Rel02").prop("disabled", false);
                $("#Rel02List").prop("disabled", false);
            }

            console.log("Modo archivo desactivado: Se restauran los filtros normales");
        }
    });
}

// Función centralizada para verificar y desactivar los controles
function verificarYDesactivarControles(mostrarLog = true) {
    // Verificar si hay opciones en la lista
    if ($("#Rel01List").find("option").length > 0) {
        if (mostrarLog) {
            console.log("Se encontraron opciones en Rel01List, desactivando controles...");
        }

        // Asegurar que solo hay un elemento seleccionado
        const opciones = $("#Rel01List option");
        if (opciones.length > 0) {
            // Seleccionar solo el primer elemento
            const primerValor = opciones.first().val();
            $("#Rel01List").val([primerValor]);

            // Aplicar la desactivación inmediatamente
            $("#Rel01List").prop("disabled", true);
            $("#Rel01").prop("disabled", true);

            // Habilitar el control de familia ya que ahora podemos seleccionar familia
            // Pero solo si chkFile no está marcado
            if (!$("#chkFile").is(":checked")) {
                $("#chkRel03").prop("disabled", false);
            }

            // Obtener el ID del proveedor seleccionado
            const proveedorId = $("#Rel01Item").val() || primerValor;

            // Cargar las familias relacionadas con este proveedor, solo si chkFile no está marcado
            if (!$("#chkFile").is(":checked")) {
                cargarFamiliasDelProveedor(proveedorId);
            }

            if (mostrarLog) {
                console.log("Controles desactivados correctamente");
            }
        }
    } else if (mostrarLog && $("#Rel01").val()) {
        console.log("No hay opciones en Rel01List todavía, pero hay texto en Rel01");
    }
}

// Función para cargar las familias relacionadas con un proveedor
function cargarFamiliasDelProveedor(proveedorId) {
    // No cargar familias si estamos en modo archivo
    if ($("#chkFile").is(":checked")) {
        return;
    }

    if (!proveedorId) {
        console.error("No se pudo determinar el ID del proveedor");
        return;
    }

    console.log("Cargando familias para el proveedor con ID: " + proveedorId);
    let datos = { ctaId: proveedorId };
    // Usar PostGen para llamar al controlador
    PostGen(datos, buscarFamiliaUrl, // URL del action 
        function (obj) { // Función de éxito
            if (obj.error === true) {
                CerrarWaiting();
                AbrirMensaje("ATENCIÓN", obj.msg, function () {
                    $("#msjModal").modal("hide");
                }, false, ["Entendido"], "error!", null);
            }
            else if (obj.warn === true) {
                CerrarWaiting();
                AbrirMensaje("ATENCIÓN", obj.msg, function () {
                    $("#msjModal").modal("hide");
                }, false, ["Entendido"], "warn!", null);
            }
            else {
                //armado del ddl de Familia
                var combo = $("#Rel03");
                // Limpiar el dropdown actual
                combo.empty();
                var opc = "<option value=''>Seleccionar...</option>";
                combo.append(opc);
                $.each(obj.lista, function (i, item) {
                    opc = "<option value='" + item.id + "'>" + item.descripcion + "</option>";
                    combo.append(opc);
                });
                CerrarWaiting();
            }
        },
        function (error) { // Función de error
            console.error("Error al cargar las familias del proveedor:", error);
        }
    );
}

function configurarBotonesProdCP() {
    // Botón de cancelar
    $("#btnCancel").on("click", function () {
        window.location.href = homeCPUrl;
    });

    $("#btnFiltro").on("mousedown", function () {
        if ($("#divFiltro").is(":hidden")) {
            $("#divDetalle").collapse("hide");
        }
    });

    $("#btnDetalle").on("mousedown", function () {
        if ($("#divDetalle").is(":visible")) {
            $("divPCP").empty();
            $("#btnDetalle").collapse("hide");
            $("#btnFiltro").collapse("show");
        }
    });

    $("#lbRel01").text("PROVEEDOR");
    $("#lbRel02").text("RUBRO");
    $("#lbRel03").text("FAMILIA");

    //al inicializar el modulo, la familia debe estar desactivada hasta que se seleccione un proveedor
    $("#chkRel03").prop("disabled", true);

    // Verificar si los controles deben estar desactivados cuando se configuren los botones
    setTimeout(function () {
        verificarYDesactivarControles(true);

        // Verificar también si chkFile está marcado al inicio
        if ($("#chkFile").is(":checked")) {
            $("#chkFile").trigger("change");
        }
    }, 100);

    // Agregar un manejador específico para cuando siteGen.js haya completado su trabajo
    $(document).on("autocompleteready", function () {
        verificarYDesactivarControles(true);
    });

    // Agregar evento al checkbox de familia para habilitar/deshabilitar la selección
    $("#chkRel03").on("change", function () {
        if ($(this).is(":checked")) {
            $("#Rel03").prop("disabled", false);
            $("#Rel03List").prop("disabled", false);
        } else {
            $("#Rel03").prop("disabled", true);
            $("#Rel03List").prop("disabled", true);

            // Limpiar la selección cuando se desmarca el checkbox
            $("#Rel03").val("");
            $("#Rel03List").empty();
            if ($("#Rel03Item").length > 0) {
                $("#Rel03Item").val("");
            }
        }
    });

    // Asegurarse de que al iniciar, si chkRel01 no está marcado, Rel03 esté desactivado
    if (!$("#chkRel01").is(":checked")) {
        $("#chkRel03").prop("checked", false);
        $("#chkRel03").prop("disabled", true);
        $("#Rel03").val("");
        $("#Rel03").prop("disabled", true);
        $("#Rel03List").empty();
        $("#Rel03List").prop("disabled", true);
    }
}

