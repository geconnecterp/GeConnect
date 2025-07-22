const divs = {
    ProductoDetalle: "#divPCP",
    ProductoListas: "#divProdLista"
}

$(function () {
    // Estilos para campos readonly
    $('<style>')
        .prop('type', 'text/css')
        .html(`
            .campo-readonly {
                background-color: #f8f9fa;
                cursor: pointer;
                border-color: #ced4da;
            }
            .campo-readonly:hover {
                background-color: #e9ecef;
                border-color: #adb5bd;
            }
        `)
        .appendTo('head');
    
    // Estilos para campos readonly
    if (!$('style:contains(".campo-readonly")').length) {
        $('<style>')
            .prop('type', 'text/css')
            .html(`
                .campo-readonly {
                    background-color: #f8f9fa;
                    cursor: pointer;
                    border-color: #ced4da;
                }
                .campo-readonly:hover {
                    background-color: #e9ecef;
                    border-color: #adb5bd;
                }
                input[readonly]:not(.campo-readonly) {
                    background-color: #e9ecef;
                    cursor: not-allowed;
                }
            `)
            .appendTo('head');
    }

    // Añadir estilos para ajustar anchos de campos específicos
    $('<style>')
        .prop('type', 'text/css')
        .html(`
            /* Campos descuentos y flete reducidos al 50% (antes 70%) */
            .input-tp_dto1, .input-tp_dto2, .input-tp_dto3, .input-tp_dto4, .input-tp_dto_pa, .input-tp_porc_flete, .input-tp_boni {
                width: 50%;
                min-width: 45px;
                font-size: 0.75rem; /* Fuente más pequeña para estos campos */
            }
            
            /* Campos con 3 decimales (más anchos) */
            .input-tp_plista, .input-tp_pcosto, .input-tp_pneto {
                width: 100%;
                min-width: 85px;
                font-size: 0.8rem; /* Fuente ligeramente más pequeña para estos campos */
            }
            
            /* Campos con 2 decimales */
            .input-tp_margen, .input-tin_alicuota, .input-tp_pvta {
                font-size: 0.8rem;
                min-width: 70px;
            }
            
            /* Ajustes generales para todos los campos numéricos */
            .input-numeric {
                padding: 0.2rem 0.3rem; /* Reducir el padding interno */
                height: auto;           /* Altura automática */
            }
            
            /* Alineación del texto en los inputs */
            .input-numeric {
                text-align: right;
                letter-spacing: -0.2px; /* Reducir ligeramente el espacio entre caracteres */
            }
            
            /* Mejorar visualización en pantallas pequeñas */
            @media (max-width: 1200px) {
                .input-tp_plista, .input-tp_pcosto, .input-tp_pneto {
                    font-size: 0.75rem;
                }
            }
        `)
        .appendTo('head');

    configurarBotonesProdCP();
    cargaEventosCP();

});

function inicializaControlCuenta() {
    $("#controlConsultaCambio" + nnControlCta01).val(true);
    window["AsignaDatosCuenta" + nnControlCta01]();

    //muestro el control
    $("#controlCta" + nnControlCta01).show("fast");
}

function configurarEventosTabla() {
    // Evento para seleccionar filas de la tabla
    $("#tbProdDet tbody tr").on("click", function (e) {
        // Solo activar si el clic no fue en un input
        if (!$(e.target).is('input')) {
            $(this).toggleClass("selected");
        }
    });

    // Evento para el checkbox de seleccionar todos
    $("#checkAllProd").on("change", function () {
        const isChecked = $(this).prop("checked");
        $("#tbProdDet tbody tr").each(function () {
            if (isChecked) {
                $(this).addClass("selected");
            } else {
                $(this).removeClass("selected");
            }
        });
    });
}


// Configuración optimizada de elementos de tabla
// Configuración optimizada de elementos de tabla
function configuracionElementosTablaDetalle() {
    console.log("Configurando elementos de tabla detalle...");

    // Remover máscaras previas para evitar conflictos en todos los campos
    $('.input-tp_plista, .input-tp_dto1, .input-tp_dto2, .input-tp_dto3, .input-tp_dto4, .input-tp_dto_pa, .input-tp_porc_flete, .input-tp_boni, .input-tp_pcosto, .input-tp_margen, .input-tp_pneto, .input-tin_alicuota, .input-tp_pvta').inputmask('remove');

    // Establecer todos los campos como readonly inicialmente (excepto los que ya tienen readonly)
    $('.input-tp_plista, .input-tp_dto1, .input-tp_dto2, .input-tp_dto3, .input-tp_dto4, .input-tp_dto_pa, .input-tp_porc_flete, .input-tp_boni, .input-tp_pcosto, .input-tp_margen, .input-tp_pneto, .input-tin_alicuota, .input-tp_pvta')
        .prop('readonly', true)
        .addClass('campo-readonly');

    // Formatear los valores
    formatearValoresIniciales();

    // Configurar eventos para activar/desactivar edición
    configurarEventosEdicion();

    // Configuración para campos con 3 decimales (P.Lista, P.Costo y P.Neto)
    Inputmask({
        alias: "numeric",
        groupSeparator: ",",
        radixPoint: ".",
        autoGroup: true,
        digits: 3,
        digitsOptional: false,
        rightAlign: true,
        prefix: '',
        placeholder: "0",
        clearMaskOnLostFocus: false,
        showMaskOnHover: false,
        showMaskOnFocus: false,
        onBeforeMask: function (value) {
            if (value) {
                let numValue = parseFloat(value.toString().replace(/,/g, ''));
                return isNaN(numValue) ? value : numValue.toFixed(3);
            }
            return value;
        }
    }).mask('.input-tp_plista, .input-tp_pcosto, .input-tp_pneto');

    // Configuración para campos con 1 decimal (descuentos y flete)
    Inputmask({
        alias: "numeric",
        groupSeparator: ",",
        radixPoint: ".",
        autoGroup: true,
        digits: 1,
        digitsOptional: false,
        rightAlign: true,
        integerDigits: 2, // Máximo 2 dígitos enteros
        min: 0,
        max: 99.9, // Máximo valor permitido: 99.9
        prefix: '',
        placeholder: "0",
        clearMaskOnLostFocus: false,
        showMaskOnHover: false,
        showMaskOnFocus: false,
        onBeforeMask: function (value) {
            if (value) {
                let numValue = parseFloat(value.toString().replace(/,/g, ''));
                if (numValue > 99.9) numValue = 99.9; // Limitar al máximo permitido
                return isNaN(numValue) ? value : numValue.toFixed(1);
            }
            return value;
        }
    }).mask('.input-tp_dto1, .input-tp_dto2, .input-tp_dto3, .input-tp_dto4, .input-tp_dto_pa, .input-tp_porc_flete');

    // Configuración para campos con 2 decimales (los demás campos numéricos)
    Inputmask({
        alias: "numeric",
        groupSeparator: ",",
        radixPoint: ".",
        autoGroup: true,
        digits: 2,
        digitsOptional: false,
        rightAlign: true,
        prefix: '',
        placeholder: "0",
        clearMaskOnLostFocus: false,
        showMaskOnHover: false,
        showMaskOnFocus: false,
        onBeforeMask: function (value) {
            if (value) {
                let numValue = parseFloat(value.toString().replace(/,/g, ''));
                return isNaN(numValue) ? value : numValue.toFixed(2);
            }
            return value;
        }
    }).mask('.input-tp_margen, .input-tin_alicuota, .input-tp_pvta');

    // Configuración para campo de bonificación (formato 999/999)
    Inputmask({
        mask: "999/999",
        placeholder: "",
        showMaskOnHover: false,
        showMaskOnFocus: false
    }).mask('.input-tp_boni');

    console.log("Configuración de elementos de tabla detalle completada");
}


// Función de depuración mejorada que verifica todos los campos
function depurarValoresIniciales() {
    console.log("=== DEPURACIÓN DE VALORES INICIALES ===");

    // Agrupar todos los selectores para campos con 3 decimales
    $('.input-tp_plista').each(function (index) {
        let value = $(this).val();
        let originalValue = $(this).data('original-value');
        console.log(`Campo tp_plista ${index + 1}: valor=${value}, original=${originalValue}`);
    });

    // Agrupar todos los selectores para campos con 2 decimales
    $('.input-tp_dto1, .input-tp_dto2, .input-tp_dto3, .input-tp_dto4, .input-tp_dto_pa, .input-tp_porc_flete').each(function (index) {
        let value = $(this).val();
        let originalValue = $(this).data('original-value');
        let fieldClass = $(this).attr('class').match(/input-tp_[^\s]+/)[0];
        console.log(`Campo ${fieldClass} ${index + 1}: valor=${value}, original=${originalValue}`);
    });

    // Revisar campos de bonificación
    $('.input-tp_boni').each(function (index) {
        let value = $(this).val();
        console.log(`Campo tp_boni ${index + 1}: valor=${value}`);
    });
}

//// Función optimizada para formatear valores iniciales
//function depurarValoresIniciales() {
//    console.log("=== DEPURACIÓN DE VALORES INICIALES ===");

//    // Agrupar todos los selectores para campos con 3 decimales
//    $('.input-tp_plista').each(function (index) {
//        let value = $(this).val();
//        let originalValue = $(this).data('original-value');
//        console.log(`Campo tp_plista ${index + 1}: valor=${value}, original=${originalValue}`);
//    });

//    // Agrupar todos los selectores para campos con 2 decimales (incluye nuevos campos)
//    $('.input-tp_dto1, .input-tp_dto2, .input-tp_dto3, .input-tp_dto4, .input-tp_dto_pa, .input-tp_porc_flete, .input-tp_margen, .input-tin_alicuota, .input-tp_pvta').each(function (index) {
//        let value = $(this).val();
//        let originalValue = $(this).data('original-value');
//        let fieldClass = $(this).attr('class').match(/input-tp_[^\s]+|input-tin_[^\s]+/)[0];
//        console.log(`Campo ${fieldClass} ${index + 1}: valor=${value}, original=${originalValue}`);
//    });

//    // Revisar campos de bonificación
//    $('.input-tp_boni').each(function (index) {
//        let value = $(this).val();
//        console.log(`Campo tp_boni ${index + 1}: valor=${value}`);
//    });

//    // Revisar campos readonly
//    $('input[readonly]').not('.campo-readonly').each(function (index) {
//        let value = $(this).val();
//        let originalValue = $(this).data('original-value');
//        let fieldId = $(this).attr('id') || 'readonly-field';
//        console.log(`Campo readonly ${fieldId} ${index + 1}: valor=${value}, original=${originalValue}`);
//    });
//}

// Nueva función para recalcular valores cuando cambia un campo
function recalcularValores(changedField) {
    // Obtener el ID del producto
    let productId = changedField.data('p-id');
    let row = changedField.closest('tr');

    // Si el campo cambiado es uno de los que afecta al costo
    if (changedField.hasClass('input-tp_plista') ||
        changedField.hasClass('input-tp_dto1') ||
        changedField.hasClass('input-tp_dto2') ||
        changedField.hasClass('input-tp_dto3') ||
        changedField.hasClass('input-tp_dto4') ||
        changedField.hasClass('input-tp_dto_pa') ||
        changedField.hasClass('input-tp_porc_flete') ||
        changedField.hasClass('input-tp_boni')) {

        // Recalcular costo (esto sería un ejemplo, el cálculo real dependería de la lógica de negocio)
        recalcularCosto(row);
    }

    // Si el campo cambiado es el margen
    if (changedField.hasClass('input-tp_margen')) {
        // Recalcular precio neto basado en el costo y el nuevo margen
        recalcularPrecioNeto(row);
    }

    // Si el campo cambiado es precio de venta o impuesto interno
    if (changedField.hasClass('input-tp_pvta') || changedField.hasClass('input-tin_alicuota')) {
        // Recalcular relación con precio venta
        recalcularRelacionPrecioVenta(row);
    }
}

// Funciones auxiliares para los recálculos
function recalcularCosto(row) {
    // Obtener valores de los campos
    let precioLista = parseFloat(row.find('.input-tp_plista').val().replace(/,/g, ''));
    let dto1 = parseFloat(row.find('.input-tp_dto1').val().replace(/,/g, ''));
    let dto2 = parseFloat(row.find('.input-tp_dto2').val().replace(/,/g, ''));
    let dto3 = parseFloat(row.find('.input-tp_dto3').val().replace(/,/g, ''));
    let dto4 = parseFloat(row.find('.input-tp_dto4').val().replace(/,/g, ''));
    let dtoPa = parseFloat(row.find('.input-tp_dto_pa').val().replace(/,/g, ''));
    let flete = parseFloat(row.find('.input-tp_porc_flete').val().replace(/,/g, ''));

    // Procesar bonificación
    let boniText = row.find('.input-tp_boni').val();
    let boniValue = 0;
    if (boniText && boniText.includes('/')) {
        let parts = boniText.split('/');
        if (parts.length === 2) {
            let num = parseInt(parts[0], 10);
            let den = parseInt(parts[1], 10);
            if (den > 0) {
                boniValue = num / den;
            }
        }
    }

    // Aquí iría la lógica de cálculo del costo (ejemplo simplificado)
    // Nota: Esta es una simplificación, el cálculo real dependería de la lógica de negocio específica
    let precioConDescuentos = precioLista;
    if (!isNaN(dto1) && dto1 > 0) precioConDescuentos = precioConDescuentos * (1 - dto1 / 100);
    if (!isNaN(dto2) && dto2 > 0) precioConDescuentos = precioConDescuentos * (1 - dto2 / 100);
    if (!isNaN(dto3) && dto3 > 0) precioConDescuentos = precioConDescuentos * (1 - dto3 / 100);
    if (!isNaN(dto4) && dto4 > 0) precioConDescuentos = precioConDescuentos * (1 - dto4 / 100);
    if (!isNaN(dtoPa) && dtoPa > 0) precioConDescuentos = precioConDescuentos * (1 - dtoPa / 100);

    let costoCalculado = precioConDescuentos;
    if (!isNaN(flete) && flete > 0) costoCalculado = costoCalculado * (1 + flete / 100);
    if (boniValue > 0) costoCalculado = costoCalculado * (1 - boniValue);

    // Actualizar el campo de costo (readonly)
    row.find('input[data-original-value]').filter(function () {
        return $(this).closest('td').hasClass(row.find('.input-tp_pcosto').closest('td').attr('class'));
    }).val(costoCalculado.toFixed(2));

    // Recalcular precio neto basado en el margen actual
    recalcularPrecioNeto(row);
}

function recalcularPrecioNeto(row) {
    // Obtener el costo
    let costo = parseFloat(row.find('input[data-original-value]').filter(function () {
        return $(this).closest('td').hasClass(row.find('.input-tp_pcosto').closest('td').attr('class'));
    }).val().replace(/,/g, ''));

    // Obtener el margen
    let margen = parseFloat(row.find('.input-tp_margen').val().replace(/,/g, ''));

    // Calcular precio neto
    let precioNeto = costo;
    if (!isNaN(margen) && margen > 0) {
        precioNeto = costo * (1 + margen / 100);
    }

    // Actualizar el campo de precio neto (readonly)
    row.find('input[data-original-value]').filter(function () {
        return $(this).closest('td').hasClass(row.find('input[data-original-value="' + row.find('.input-tp_pneto').data('original-value') + '"]').closest('td').attr('class'));
    }).val(precioNeto.toFixed(2));
}

function recalcularRelacionPrecioVenta(row) {
    // Esta función calcularía la relación entre precio de venta y otros valores
    // La implementación dependería de la lógica de negocio específica
}


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

    $(document).on('blur', 'input.form-control-sm', function () {
        marcarCampoModificado(this);
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

    // Evento para el botón buscar
    $("#btnBuscar").on("click", function (e) {
        e.preventDefault();

        // Verificar si se ha seleccionado un proveedor mediante la variable consCta
        // Esta variable debería contener el ID de la cuenta del proveedor si se ha seleccionado
        if (typeof consCta === 'undefined' || !consCta) {
            // Si no hay proveedor seleccionado, mostrar mensaje de advertencia
            AbrirMensaje(
                "ATENCIÓN",
                "Debe seleccionar un proveedor antes de realizar la búsqueda.",
                function () {
                    $("#msjModal").modal("hide");
                },
                false,
                ["Entendido"],
                "warn!",
                null
            );
            return false; // Detener la ejecución
        }

        AbrirWaiting("Cargando los productos del proveedor según el filtro especificado. Por favor espere...");
        // Si hay un proveedor seleccionado, continuar con la búsqueda
        buscarProductosDetalle();
        //Presenta el control comun de cuenta
        inicializaControlCuenta();
    });

    //inicializo botones aceptar y confirmar desactivados y ocultos
    $("#btnAbmAceptar").prop("disabled", true).hide();
    $("#btnAbmCancelar").prop("disabled", true).hide();

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

    // Asegurarse de que los campos vuelvan a estado readonly si el usuario hace clic en otra parte
    $(document).off('click').on('click', function (e) {
        if (!$(e.target).is('.input-tp_plista, .input-tp_dto1, .input-tp_dto2, .input-tp_dto3, .input-tp_dto4, .input-tp_dto_pa, .input-tp_porc_flete, .input-tp_boni, .input-tp_margen, .input-tin_alicuota, .input-tp_pvta')) {
            // Si se hizo clic fuera de los inputs y hay alguno activo, desactivarlo
            $('.input-tp_plista, .input-tp_dto1, .input-tp_dto2, .input-tp_dto3, .input-tp_dto4, .input-tp_dto_pa, .input-tp_porc_flete, .input-tp_boni, .input-tp_margen, .input-tin_alicuota, .input-tp_pvta').each(function () {
                if (!$(this).prop('readonly')) {
                    // En lugar de usar .blur() directamente, que está deprecated
                    // Disparamos el evento blur de forma manual en el elemento DOM nativo
                    const event = new Event('blur', { bubbles: true });
                    this.dispatchEvent(event);
                }
            });
        }
    });

}

function obtenerParametros(div) {
    // Obtener valores de los filtros
    const proveedor = $("#Rel01Item").val() || $("#Rel01List").val();

    // Validar que se haya seleccionado un proveedor
    if (!proveedor || proveedor === "") {
        mostrarMensajeError("Debe seleccionar un proveedor para realizar la búsqueda.");
        return false;
    }

    // Obtener el resto de parámetros
    const buscar = $("#Buscar").val() || "";
    const id = $("#Id").val() || "";
    const id2 = $("#Id2").val() || "";

    // Obtener rubros seleccionados
    const rubros = [];
    $("#Rel02List option").each(function () {
        // Agregar todos los elementos de la lista, estén seleccionados o no
        rubros.push($(this).val());
    });


    // Obtener familias seleccionadas
    const familias = [];
    $("#Rel03List option").each(function () {
        // Agregar todos los elementos de la lista, estén seleccionados o no
        familias.push({
            id: $(this).val(),
            descripcion: $(this).text()
        });
    });

    // Verificar opciones adicionales
    const incluirDiscontinuos = $("#Opt1").prop("checked");
    const generarArchivo = $("#Opt2").prop("checked");

    // Mostrar indicador de carga
    $(div).html('<div class="text-center p-3"><i class="bx bx-loader bx-spin font-size-24"></i><p class="mt-2">Cargando datos...</p></div>');
    return {
        buscar: buscar,
        id: id,
        id2: id2,
        ctaId: proveedor,
        familias: familias,
        rubros: rubros,
        disc: incluirDiscontinuos,
        file: generarArchivo
    };
}

function buscarProductosDetalle() {
    let datos = obtenerParametros(divs.ProductoDetalle);

    // Realizar petición AJAX
    $.ajax({
        url: buscarProdDetUrl,
        type: "POST",
        data: datos,
        success: function (response) {
            // Mostrar resultados
            $("#divPCP").html(response);

            // Si hay resultados, mostrar el panel de detalles
            if ($(response).find("tbody tr").length > 0) {
                $("#divFiltro").removeClass("show");
                $("#divDetalle").addClass("show");

                // Configurar eventos para la tabla de resultados
                configurarEventosTabla();

                // Depurar valores iniciales antes de aplicar configuración
                console.log("Datos cargados, verificando valores iniciales...");
                depurarValoresIniciales();

                // Aplicar configuración a los inputs numéricos
                configuracionElementosTablaDetalle();

                // Añadir estilos para campos modificados si no existen
                if (!$('style:contains(".campo-modificado")').length) {
                    $('<style>')
                        .prop('type', 'text/css')
                        .html(`
                            /* Estilo para campos modificados */
                            .campo-modificado {
                                background-color: #d4f1f9 !important; /* Celeste pastel claro */
                                border-color: #a8e1f5 !important;
                            }
                            
                            /* Indicador visual de cambio */
                            .indicador-cambio {
                                position: absolute;
                                top: 0;
                                right: 0;
                                width: 0;
                                height: 0;
                                border-style: solid;
                                border-width: 0 8px 8px 0;
                                border-color: transparent #4bacc6 transparent transparent;
                            }
                            
                            /* Hacer que las celdas sean relativas para posicionar el indicador */
                            #tbProdDet td {
                                position: relative;
                            }
                        `)
                        .appendTo('head');
                }

                // Optimizar la visualización de la tabla
                optimizarVisualizacionTabla();

                // Verificar valores después de aplicar la configuración
                setTimeout(function () {
                    console.log("Después de aplicar configuración:");
                    depurarValoresIniciales();

                    // Inicializar la detección de campos modificados
                    actualizarCamposModificados();
                }, 500);

                // Obtener el ID del primer producto para consultar sus listas de precios
                const primerProductoId = $(response).find("tbody tr:not(.table-secondary):first").data("p-id");
                console.log("ID del primer producto:", primerProductoId);

                buscarProductoLista(primerProductoId);
            }
        },
        error: function (error) {
            console.error("Error al obtener productos:", error);
            mostrarMensajeError("Se produjo un error al buscar los productos. Por favor, inténtelo de nuevo más tarde.");
        }
    });

    return false;
}

// Función para optimizar la visualización de la tabla
function optimizarVisualizacionTabla() {
    // Asegurarnos de que la tabla existe
    if ($("#tbProdDet").length === 0) {
        return;
    }

    // Ajustar columnas con texto para que no sean demasiado anchas
    $("#tbProdDet th:nth-child(2)").css('max-width', '180px'); // Descripción
    $("#tbProdDet td:nth-child(2)").css({
        'max-width': '180px',
        'white-space': 'nowrap',
        'overflow': 'hidden',
        'text-overflow': 'ellipsis'
    });

    // Asegurarnos que la tabla tenga scroll horizontal si es necesario
    $("#tbProdDet").closest('.table-responsive').css('overflow-x', 'auto');

    console.log("Tabla optimizada para mejor visualización");
}

function buscarProductoLista(primerProductoId) {
    // Si encontramos un producto, cargar sus listas de precios
    if (primerProductoId) {
        console.log("Cargando listas de precios para el producto ID:", primerProductoId);

        let datos = obtenerParametros(divs.ProductoListas);
        // Añadir el ID del producto a los parámetros
        datos.id = primerProductoId;
        /*datos.id2 = primerProductoId;*/

        // Mostrar indicador de carga en el div de listas de precios
        $("#divProdLista").html('<div class="text-center p-3"><i class="bx bx-loader bx-spin font-size-24"></i><p class="mt-2">Cargando listas de precios...</p></div>');

        // Realizar la segunda petición AJAX para obtener las listas de precios
        $.ajax({
            url: buscarProdListaUrl,
            type: "POST",
            data: datos,
            success: function (responseLista) {
                CerrarWaiting();
                // Mostrar resultados de listas de precios
                $("#divProdLista").html(responseLista);
                console.log("Listas de precios cargadas correctamente");
            },
            error: function (error) {
                CerrarWaiting();
                console.error("Error al obtener las listas de precios:", error);
                $("#divProdLista").html('<div class="alert alert-danger">Error al cargar las listas de precios.</div>');
            }
        });
    } else {
        CerrarWaiting();
        console.warn("No se pudo obtener el ID del primer producto");
        $("#divProdLista").html('<div class="alert alert-warning">No se pudo obtener información de listas de precios.</div>');
    }
}

// Función para configurar eventos de activación/desactivación de edición
function configurarEventosEdicion() {
    // Eliminar cualquier evento click previo para evitar duplicados
    $('.input-tp_plista, .input-tp_dto1, .input-tp_dto2, .input-tp_dto3, .input-tp_dto4, .input-tp_dto_pa, .input-tp_porc_flete, .input-tp_boni, .input-tp_margen, .input-tin_alicuota, .input-tp_pvta')
        .off('click')
        .off('focus')
        .off('blur');

    // Agregar evento click para habilitar edición
    $('.input-tp_plista, .input-tp_dto1, .input-tp_dto2, .input-tp_dto3, .input-tp_dto4, .input-tp_dto_pa, .input-tp_porc_flete, .input-tp_boni, .input-tp_margen, .input-tin_alicuota, .input-tp_pvta').on('click', function (e) {
        // Evitar propagación para no activar el evento de selección de fila
        e.stopPropagation();

        // Obtener referencia al elemento DOM nativo
        const inputElement = this;

        // Activar edición solo para este campo
        $(inputElement).prop('readonly', false).removeClass('campo-readonly');

        // Usar setTimeout para evitar la advertencia de deprecated
        setTimeout(function () {
            // Enfocar el elemento
            inputElement.focus();

            // Seleccionar todo el contenido
            inputElement.select();
        }, 0);

        console.log(`Campo ${$(inputElement).attr('class').match(/input-tp_[^\s]+|input-tin_[^\s]+/)[0]} activado para edición`);
    });

    // Definir los campos de la secuencia01
    const camposSecuencia01 = '.input-tp_plista, .input-tp_dto1, .input-tp_dto2, .input-tp_dto3, .input-tp_dto4, .input-tp_dto_pa, .input-tp_porc_flete, .input-tp_boni';

    // Agregar evento para manejar la pérdida de foco en campos de la secuencia01
    $(camposSecuencia01).on('blur', function () {
        const $this = $(this);
        const campo = $this.attr('class').match(/input-tp_[^\s]+/)[0];
        const row = $this.closest('tr');

        // Formatear el valor según el tipo de campo
        let value = $this.val().replace(/,/g, '');
        let numValue = parseFloat(value);

        if (!isNaN(numValue)) {
            // Aplicar formato según el tipo de campo
            if ($this.hasClass('input-tp_plista')) {
                $this.val(numValue.toFixed(3));
            } else if ($this.hasClass('input-tp_dto1') ||
                $this.hasClass('input-tp_dto2') ||
                $this.hasClass('input-tp_dto3') ||
                $this.hasClass('input-tp_dto4') ||
                $this.hasClass('input-tp_dto_pa') ||
                $this.hasClass('input-tp_porc_flete')) {
                // Limitar a 99.9 como máximo para los campos de descuento y flete
                numValue = Math.min(numValue, 99.9);
                $this.val(numValue.toFixed(1));
            }
        }

        // Procesar bonificación si es el campo correspondiente
        if (campo === 'input-tp_boni') {
            let val = $this.val();
            let partes = val.split('/');
            if (partes.length === 2) {
                let num = parseInt(partes[0], 10);
                let den = parseInt(partes[1], 10);
                if (num > den && den > 0) {
                    alert('El denominador debe ser mayor al numerador. Se corregirá automáticamente.');
                    $this.val(den + '/' + num);
                }
            }
        }

        // Volver a readonly
        $this.prop('readonly', true).addClass('campo-readonly');

        // Llamar a la API para recalcular el costo
        calcularCostoAPI(row);

        console.log(`Campo ${campo} vuelve a readonly, recalculando costo...`);
    });

    // Agregar evento para manejar la pérdida de foco en otros campos
    $('.input-tp_margen, .input-tin_alicuota, .input-tp_pvta').on('blur', function () {
        const $this = $(this);
        const campo = $this.attr('class').match(/input-tp_[^\s]+|input-tin_[^\s]+/)[0];

        // Formatear el valor
        let value = $this.val().replace(/,/g, '');
        let numValue = parseFloat(value);

        if (!isNaN(numValue)) {
            $this.val(numValue.toFixed(2));
        }

        // Volver a readonly
        $this.prop('readonly', true).addClass('campo-readonly');

        // Ejecutar cálculos adicionales según el campo
        if (campo === 'input-tp_margen') {
            recalcularPrecioNetoDesdeMargen($this.closest('tr'));
        } else if (campo === 'input-tin_alicuota' || campo === 'input-tp_pvta') {
            recalcularRelacionPrecioVenta($this.closest('tr'));
        }

        console.log(`Campo ${campo} vuelve a readonly`);
    });

    // Asegurar que los campos vuelvan a estado readonly si el usuario hace clic en otra parte
    $(document).off('click.desactivarCampos').on('click.desactivarCampos', function (e) {
        if (!$(e.target).is('.input-tp_plista, .input-tp_dto1, .input-tp_dto2, .input-tp_dto3, .input-tp_dto4, .input-tp_dto_pa, .input-tp_porc_flete, .input-tp_boni, .input-tp_margen, .input-tin_alicuota, .input-tp_pvta')) {
            // Si se hizo clic fuera de los inputs y hay alguno activo, desactivarlo
            $('.input-tp_plista, .input-tp_dto1, .input-tp_dto2, .input-tp_dto3, .input-tp_dto4, .input-tp_dto_pa, .input-tp_porc_flete, .input-tp_boni, .input-tp_margen, .input-tin_alicuota, .input-tp_pvta').filter(function () {
                return !$(this).prop('readonly');
            }).each(function () {
                // Disparamos el evento blur de forma manual en el elemento DOM nativo
                const event = new Event('blur', { bubbles: true });
                this.dispatchEvent(event);
            });
        }
    });
}

// Función mejorada para formatear valores iniciales
function formatearValoresIniciales() {
    console.log("Formateando valores iniciales...");

    // Formatear campos con 3 decimales (plista, pcosto y pneto)
    $('.input-tp_plista, .input-tp_pcosto, .input-tp_pneto').each(function () {
        let originalValue = $(this).data('original-value');
        if (originalValue !== undefined) {
            let numValue = parseFloat(originalValue);
            if (!isNaN(numValue)) {
                // Redondear a 3 decimales para coincidir exactamente con el formato mostrado
                const valorRedondeado = parseFloat(numValue.toFixed(3));
                $(this).val(valorRedondeado.toFixed(3));

                // Actualizar data-original-value para que coincida exactamente con el valor mostrado
                $(this).data('original-value', valorRedondeado);

                let fieldClass = $(this).attr('class').match(/input-tp_[^\s]+/)[0];
                console.log(`Valor ${fieldClass}: ${originalValue} → ${valorRedondeado.toFixed(3)}`);
            }
        }
    });

    // Formatear campos con 1 decimal (descuentos y flete)
    $('.input-tp_dto1, .input-tp_dto2, .input-tp_dto3, .input-tp_dto4, .input-tp_dto_pa, .input-tp_porc_flete').each(function () {
        let originalValue = $(this).data('original-value');
        let fieldClass = $(this).attr('class').match(/input-tp_[^\s]+/)[0];

        if (originalValue !== undefined) {
            let numValue = parseFloat(originalValue);
            if (!isNaN(numValue)) {
                // Limitar a 99.9 como máximo y redondear a 1 decimal
                numValue = Math.min(numValue, 99.9);
                const valorRedondeado = parseFloat(numValue.toFixed(1));
                $(this).val(valorRedondeado.toFixed(1));

                // Actualizar data-original-value para que coincida exactamente con el valor mostrado
                $(this).data('original-value', valorRedondeado);

                console.log(`Valor ${fieldClass}: ${originalValue} → ${valorRedondeado.toFixed(1)}`);
            }
        }
    });

    // Formatear campos con 2 decimales (otros campos numéricos)
    $('.input-tp_margen, .input-tin_alicuota, .input-tp_pvta').each(function () {
        let originalValue = $(this).data('original-value');
        let fieldClass = $(this).attr('class').match(/input-tp_[^\s]+|input-tin_[^\s]+/)[0];

        if (originalValue !== undefined) {
            let numValue = parseFloat(originalValue);
            if (!isNaN(numValue)) {
                const valorRedondeado = parseFloat(numValue.toFixed(2));
                $(this).val(valorRedondeado.toFixed(2));

                // Actualizar data-original-value para que coincida exactamente con el valor mostrado
                $(this).data('original-value', valorRedondeado);

                console.log(`Valor ${fieldClass}: ${originalValue} → ${valorRedondeado.toFixed(2)}`);
            }
        }
    });

    // Normalizar bonificaciones con valor "0" a cadena vacía
    $('.input-tp_boni').each(function () {
        let originalValue = $(this).data('original-value');
        if (originalValue !== undefined && originalValue.toString().trim() === '0') {
            $(this).val('');
            $(this).data('original-value', ''); // Actualizar para coincidencia exacta
        }
    });
}




function configurarEventosTabla() {
    // Evento para seleccionar filas de la tabla
    $("#tbProdDet tbody tr").on("click", function () {
        $(this).toggleClass("selected");
    });

    // Evento para el checkbox de seleccionar todos
    $("#checkAllProd").on("change", function () {
        const isChecked = $(this).prop("checked");
        $("#tbProdDet tbody tr").each(function () {
            if (isChecked) {
                $(this).addClass("selected");
            } else {
                $(this).removeClass("selected");
            }
        });
    });
}

// Función para volver al filtro
function volverAFiltro() {
    $("#divDetalle").removeClass("show");
    $("#divFiltro").addClass("show");
}

// Opcionalmente, agregar una función helper para seleccionar contenido
function seleccionarContenido(element) {
    setTimeout(function () {
        element.focus();
        element.select();
    }, 0);
}

// Función para llamar a la API de cálculo de costo
function calcularCostoAPI(row) {
    const productId = row.data('p-id');

    // Recopilar los valores de los campos del Segmento01
    const datos = {
        p_id: productId,
        tp_plista: parseFloat(row.find('.input-tp_plista').val().replace(/,/g, '')),
        tp_dto1: parseFloat(row.find('.input-tp_dto1').val().replace(/,/g, '')),
        tp_dto2: parseFloat(row.find('.input-tp_dto2').val().replace(/,/g, '')),
        tp_dto3: parseFloat(row.find('.input-tp_dto3').val().replace(/,/g, '')),
        tp_dto4: parseFloat(row.find('.input-tp_dto4').val().replace(/,/g, '')),
        tp_dto_pa: parseFloat(row.find('.input-tp_dto_pa').val().replace(/,/g, '')),
        tp_porc_flete: parseFloat(row.find('.input-tp_porc_flete').val().replace(/,/g, '')),
        tp_boni: row.find('.input-tp_boni').val()
    };

    // Mostrar indicador de carga en el campo de costo
    const campoCoste = row.find('td:nth-child(13) input');
    const valorOriginal = campoCoste.val();
    campoCoste.val('Calculando...').addClass('calculating');

    // Llamar a la API usando PostGen según el patrón proporcionado
    AbrirWaiting("Calculando costo...");
    PostGen(datos, calcularCostoUrl, function (obj) {
        CerrarWaiting();

        if (obj.error === true) {
            // Manejo del error
            campoCoste.val(valorOriginal).removeClass('calculating');
            AbrirMensaje("¡¡Algo no fué bien!!", obj.msg, function () {
                $("#msjModal").modal("hide");
                return true;
            }, false, ["Aceptar"], "error!", null);
        } else if (obj.warn === true) {
            // Manejo de advertencia
            campoCoste.val(valorOriginal).removeClass('calculating');
            AbrirMensaje("ATENCIÓN", obj.msg, function () {
                if (obj.auth === true) {
                    window.location.href = login;
                } else {
                    $("#msjModal").modal("hide");
                }
                return true;
            }, false, ["Aceptar"], "warn!", null);
        } else {
            // Éxito: actualizar el valor del costo con el resultado de la API
            // Cambiado a 3 decimales
            campoCoste.val(parseFloat(obj.costo).toFixed(3)).removeClass('calculating');

            // NO actualizar data-original-value para mantener referencia al valor original
            // Esto permite identificar que el campo ha sido modificado
            // campoCoste.data('original-value', obj.costo);

            // Marcar el campo como modificado para destacarlo visualmente
            marcarCampoModificado(campoCoste);

            // Recalcular el precio neto basado en el nuevo costo
            // Pasamos false como segundo parámetro para indicar que no resguarde cambios aún
            recalcularPrecioNetoDesdeMargen(row, false);

            // Resguardar los cambios después de completar todos los cálculos
            resguardarCambiosProducto(row);

            console.log('Costo actualizado para producto ID:', productId, 'Nuevo valor:', obj.costo);
        }
    }, function (error) {
        // Función de error (callback de error de PostGen)
        CerrarWaiting();
        console.error('Error en la llamada al servidor:', error);
        campoCoste.val(valorOriginal).removeClass('calculating');
        AbrirMensaje("ERROR", "Se produjo un error al comunicarse con el servidor. Por favor, inténtelo nuevamente.", function () {
            $("#msjModal").modal("hide");
        }, false, ["Aceptar"], "error!", null);
    });
}

// Nueva función para resguardar los cambios del producto
function resguardarCambiosProducto(row) {
    // Recopilar todos los valores del producto
    const datos = {
        p_id: row.data('p-id'),
        tp_plista: parseFloat(row.find('.input-tp_plista').val().replace(/,/g, '')),
        tp_dto1: parseFloat(row.find('.input-tp_dto1').val().replace(/,/g, '')),
        tp_dto2: parseFloat(row.find('.input-tp_dto2').val().replace(/,/g, '')),
        tp_dto3: parseFloat(row.find('.input-tp_dto3').val().replace(/,/g, '')),
        tp_dto4: parseFloat(row.find('.input-tp_dto4').val().replace(/,/g, '')),
        tp_dto_pa: parseFloat(row.find('.input-tp_dto_pa').val().replace(/,/g, '')),
        tp_porc_flete: parseFloat(row.find('.input-tp_porc_flete').val().replace(/,/g, '')),
        tp_boni: row.find('.input-tp_boni').val(),
        tp_pcosto: parseFloat(row.find('td:nth-child(13) input').val().replace(/,/g, '')),
        tp_margen: parseFloat(row.find('.input-tp_margen').val().replace(/,/g, '')),
        tp_pneto: parseFloat(row.find('td:nth-child(15) input').val().replace(/,/g, '')),
        tin_alicuota: parseFloat(row.find('.input-tin_alicuota').val().replace(/,/g, '')),
        tp_pvta: parseFloat(row.find('.input-tp_pvta').val().replace(/,/g, ''))
    };

    // Llamar al servidor para resguardar los cambios
    $.ajax({
        url: resguardarCambiosProductoUrl, // Esta URL debe definirse en el controlador
        type: 'POST',
        data: datos,
        dataType: 'json',
        success: function (response) {
            if (response.error) {
                console.error('Error al resguardar cambios:', response.msg);
            } else if (response.warn) {
                console.warn('Advertencia al resguardar cambios:', response.msg);
            } else {
                console.log('Cambios resguardados correctamente:', response.msg);
            }
        },
        error: function (xhr, status, error) {
            console.error('Error en la llamada AJAX al resguardar cambios:', error);
        }
    });
}

// Función para recalcular el precio neto basado en el margen
// Añadimos un segundo parámetro para controlar si se resguardan los cambios
// Función para recalcular el precio neto basado en el margen
// Añadimos un segundo parámetro para controlar si se resguardan los cambios
function recalcularPrecioNetoDesdeMargen(row, resguardar = true) {
    // Obtener el costo
    let costo = parseFloat(row.find('td:nth-child(13) input').val().replace(/,/g, ''));

    // Obtener el margen
    let margen = parseFloat(row.find('.input-tp_margen').val().replace(/,/g, ''));

    // Calcular precio neto
    let precioNeto = costo;
    if (!isNaN(margen) && margen > 0) {
        precioNeto = costo * (1 + margen / 100);
    }

    // Actualizar el campo de precio neto (readonly) con 3 decimales
    row.find('td:nth-child(15) input').val(precioNeto.toFixed(3));
    marcarCampoModificado(row.find('td:nth-child(15) input'));

    // Resguardar los cambios solo si el parámetro resguardar es true
    if (resguardar) {
        resguardarCambiosProducto(row);
    }

    console.log('Precio neto recalculado basado en el margen:', precioNeto.toFixed(3));
}

// Función para marcar un campo como modificado
function marcarCampoModificado(input) {
    const $input = $(input);
    const valorOriginal = $input.data('original-value');
    const valorActual = $input.val().replace(/,/g, '');

    // Comparar valores numéricos con una pequeña tolerancia para decimales
    let esModificado = false;

    if ($input.hasClass('input-tp_boni')) {
        // Para el campo de bonificación, comparamos los strings directamente
        esModificado = valorOriginal !== valorActual;
    } else {
        // Para campos numéricos, convertimos a números y comparamos con tolerancia
        const numOriginal = parseFloat(valorOriginal);
        const numActual = parseFloat(valorActual);

        // Consideramos diferente si hay una diferencia mayor a 0.001
        esModificado = Math.abs(numOriginal - numActual) > 0.001;
    }

    // Aplicar o quitar la clase según corresponda
    if (esModificado) {
        $input.addClass('campo-modificado');

        // Si no existe el indicador de cambio, agregarlo
        if ($input.parent().find('.indicador-cambio').length === 0) {
            $input.parent().append('<div class="indicador-cambio"></div>');
        }
    } else {
        $input.removeClass('campo-modificado');
        $input.parent().find('.indicador-cambio').remove();
    }
}

// Función mejorada para marcar campos modificados
// Función mejorada para marcar campos modificados
function actualizarCamposModificados() {
    // Procesar todos los inputs de la tabla
    $('#tbProdDet input').each(function () {
        const $input = $(this);
        const valorOriginal = $input.data('original-value');

        // Ignorar si no tiene valor original o es readonly sin clase campo-readonly
        if (valorOriginal === undefined || ($input.prop('readonly') && !$input.hasClass('campo-readonly'))) {
            return;
        }

        let valorActual = $input.val().replace(/,/g, '');
        let esModificado = false;

        if ($input.hasClass('input-tp_boni')) {
            // Para el campo de bonificación
            // 1. Normalizar ambos valores quitando espacios
            const originalTrimmed = (valorOriginal || '').toString().trim();
            const actualTrimmed = valorActual.toString().trim();

            // 2. Verificar si el valor original es "0" y el actual está vacío
            if (originalTrimmed === "0" && actualTrimmed === "") {
                esModificado = false; // Consideramos "0" igual a vacío para bonificación
            } else {
                esModificado = originalTrimmed !== actualTrimmed;
            }
        } else {
            // Para campos numéricos, usando parsing más seguro
            try {
                const numOriginal = parseFloat(valorOriginal);
                const numActual = parseFloat(valorActual);

                if (!isNaN(numOriginal) && !isNaN(numActual)) {
                    // Usar una tolerancia basada en el tipo de campo y su precisión
                    let tolerancia = 0.009; // Tolerancia base para valores con 2 decimales

                    if ($input.hasClass('input-tp_dto1') ||
                        $input.hasClass('input-tp_dto2') ||
                        $input.hasClass('input-tp_dto3') ||
                        $input.hasClass('input-tp_dto4') ||
                        $input.hasClass('input-tp_dto_pa') ||
                        $input.hasClass('input-tp_porc_flete')) {
                        tolerancia = 0.09; // Mayor tolerancia para descuentos (1 decimal)
                    } else if ($input.hasClass('input-tp_plista') ||
                        $input.hasClass('input-tp_pcosto') ||
                        $input.hasClass('input-tp_pneto')) {
                        tolerancia = 0.0009; // Menor tolerancia para valores con 3 decimales
                    }

                    // Determinar modificación en base a la diferencia relativa o absoluta
                    if (Math.abs(numOriginal) < 0.001) {
                        // Para valores cercanos a cero, usar diferencia absoluta
                        esModificado = Math.abs(numActual) > tolerancia;
                    } else {
                        // Para otros valores, usar diferencia relativa o absoluta, la que sea menor
                        const difAbsoluta = Math.abs(numOriginal - numActual);
                        const difRelativa = difAbsoluta / Math.abs(numOriginal);
                        esModificado = difAbsoluta > tolerancia && difRelativa > 0.001;
                    }
                } else {
                    // Si alguno no es número pero no ambos están vacíos
                    const ambosVacios = (valorOriginal === null || valorOriginal === undefined || valorOriginal.toString().trim() === '') &&
                        (valorActual === null || valorActual === undefined || valorActual.trim() === '');
                    esModificado = !ambosVacios;
                }
            } catch (e) {
                console.error("Error al comparar valores:", e, { original: valorOriginal, actual: valorActual });
                esModificado = false; // En caso de error, asumimos que no hay cambios
            }
        }

        // Aplicar o quitar la clase según corresponda
        $input.toggleClass('campo-modificado', esModificado);

        // Manejar el indicador visual de cambio
        if (esModificado) {
            if ($input.parent().find('.indicador-cambio').length === 0) {
                $input.parent().append('<div class="indicador-cambio"></div>');
            }
        } else {
            $input.parent().find('.indicador-cambio').remove();
        }
    });
}

