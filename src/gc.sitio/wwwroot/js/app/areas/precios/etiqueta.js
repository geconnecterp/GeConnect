$(function () {
    InicializaPantallaEtiqueta();
    InicializaEnventosEtiqueta();
});

function cancelarEtiqueta() {
    $("#chkTipoEtiq").prop("checked", false);
    $("#chkSinImprimir").prop("checked", false);
    $("#chkOferta").prop("checked", false);
    
    if ($("#chkCargaPrevia").is(":checked")) {
        $("#chkCargaPrevia").trigger("click");       
    }
    if ($("#chkDesdeHasta").is(":checked")) {
        $("#chkDesdeHasta").trigger("click");
    }
    
    if ($("#chkRel011").is(":checked")) {
        $("#chkRel011").trigger("click");
    }
    if ($("#chkRel03").is(":not(:disabled)")) {
        if ($("#chkRel03").is(":checked")) {
            $("#chkRel03").trigger("click");
        }
        $("#chkRel03").prop("checked", false);
    }
    if ($("#chkRel02").is(":checked")) {
        $("#chkRel02").trigger("click");
    }
}

function InicializaPantallaEtiqueta() {
    // INICIALIZAMOS PANELES
    if ($("#divDetalle").is(":visible")) {
        $("#divDetalle").collapse("hide");
    }
    $("#divFiltro").collapse("show");

    //tipo etiqueta (siempre checked disabled)
    $("#chkTipoEtiq").prop("disabled", true);

    //carga previa. Desactivado desde el inicio
    $("#chkCargaPrevia").prop("checked", false);
    $("#CargaPrevia").prop("disabled", true);
    $("#lbCargaPrevia").text("Carga Previa")
    //Nombre del check que controla las fechas
    $("#lbChkDesdeHasta").text("Modificados");

    //especificando nombre del label de proveedor
    $("#divLs01 span").text("Proveedor")
    $("#lbRel01").text("Proveedor")

    $("#lbRel03").text("Familias")
    $("#lbRel02").text("Rubros")
    $("#lbNombreRel02").text("Rubro");
}

function InicializaEnventosEtiqueta() {
    //Evento de cambio en check de CargaPrevia
    $("#chkCargaPrevia").on("change", function () {
        if ($("#chkCargaPrevia").is(":checked")) {
            $("#CargaPrevia").prop("disabled", false);
        } else {
            $("#CargaPrevia").prop("disabled", true);
        }
    });

    //evento de cambio en check de Modificados
    $("#chkDesdeHasta").on("change", function () {
        if ($("#chkDesdeHasta").is(":checked")) {
            $("#Date1").prop("disabled", false);
            $("#Date2").prop("disabled", false);
        } else {
            $("#Date1").prop("disabled", true);
            $("#Date2").prop("disabled", true);
        }
    });

    //check generico REL01 activando componentes disables
    $("#chkRel011").on("change", function () {
        const isChecked = $(this).is(":checked");

        if (isChecked) {
            $("#Rel011").prop("disabled", false);
            $("#Rel011List").prop("disabled", false);

            //// ✅ INICIALIZAR AUTOCOMPLETE SOLO UNA VEZ
            //if (!$("#Rel011").hasClass("ui-autocomplete-input")) {
            //    inicializarAutocompleteRel011();
            //}

            setTimeout(() => $("#Rel011").trigger("focus"), 50);
        } else {
            $("#Rel011").prop("disabled", true).val("");
            $("#Rel011List").prop("disabled", true).empty();
            $("#Rel011Item").val("");

            if ($("#Rel011").hasClass("ui-autocomplete-input")) {
                $("#Rel011").autocomplete("destroy");
            }
        }
    });

    $("#Rel011").on("click", function () { $(this).val(""); });

    $("#Rel011List").off("dblclick.removeOption").on("dblclick.removeOption", "option", function (e) {
        e.stopPropagation();
        $(this).remove();
        const $list = $("#Rel011List");
        if ($.fn.selectpicker && $list.hasClass("selectpicker")) {
            $list.selectpicker("refresh");
        }
    });

   

    // Autocomplete especializado para Rel011
    $(document).on("keydown.autocomplete", "input#Rel011", function () {
        $(this).autocomplete({
            source: function (request, response) {
                $.ajax({
                    url: autoComRel01Url,
                    type: "POST",
                    dataType: "json",
                    data: { prefix: request.term },
                    success: function (obj) {
                        response($.map(obj, function (item) {
                            return {
                                label: item.descripcion,
                                value: item.descripcion,
                                id: item.id,
                                nombre: item.nombre || item.descripcion,
                                domicilio: item.domicilio || ""
                            };
                        }));
                    },
                    error: function () {
                        response([]);
                    }
                });
            },
            minLength: 3,
            select: function (event, ui) {
                const yaExiste = $("#Rel011List option[value='" + ui.item.id + "']").length > 0;

                if (!yaExiste) {
                    $("#Rel011Item").val(ui.item.id);
                    const opcion = $("<option></option>")
                        .attr("value", ui.item.id)
                        .text(ui.item.label);
                    $("#Rel011List").append(opcion);
                    $("#Rel011List").trigger("change");
                }

                setTimeout(() => $("#Rel011").val(""), 10);
                return false;
            },
            focus: function () {
                return false;
            }
        });
    });

    $(document).on("change", "select#Rel011List", function () {
        verificarYDesactivarControles();
    });

    // Evento: al seleccionar una opción en #Rel03, copiarla a #Rel03List sin duplicados
    $(document).off("change.addRel03Item").on("change.addRel03Item", "select#Rel03", function () {
        const $origen = $(this);
        const $destino = $("#Rel03List");
        const $seleccionadas = $origen.find("option:selected");
        if ($seleccionadas.length === 0) return;

        let huboCambios = false;

        $seleccionadas.each(function () {
            const val = this.value;
            const txt = this.text;
            if (!val) return; // ignora opción vacía "Seleccionar..."

            // evita duplicados por value
            const existe = $destino.find("option[value='" + $.escapeSelector(val) + "']").length > 0;
            if (!existe) {
                const $op = $("<option></option>").val(val).text(txt);
                $destino.append($op);
                huboCambios = true;
            }
        });

        if (huboCambios) {
            if ($.fn.selectpicker && $destino.hasClass("selectpicker")) {
                $destino.selectpicker("refresh");
            }
            $destino.trigger("change");
        }

        // limpia selección del origen
        $origen.val("");
        if ($.fn.selectpicker && $origen.hasClass("selectpicker")) {
            $origen.selectpicker("refresh");
        }
    });

    // Evento: doble clic en #Rel03List elimina la opción
    $("#Rel03List").off("dblclick.removeRel03Option").on("dblclick.removeRel03Option", "option", function (e) {
        e.stopPropagation();
        const $opcion = $(this);
        const $lista = $opcion.parent();
        $opcion.remove();

        if ($.fn.selectpicker && $lista.hasClass("selectpicker")) {
            $lista.selectpicker("refresh");
        }
        $lista.trigger("change");
    });
}

function verificarYDesactivarControles(mostrarLog = true) {

    // Verificar si hay opciones en la lista
    if ($("#Rel011List").find("option").length > 0) {
        if (mostrarLog) {
            console.log("Se encontraron opciones en Rel01List, desactivando controles...");
        }

        // Asegurar que solo hay un elemento seleccionado
        const opciones = $("#Rel011List option");
        // obtengo la cantidad
        const cantidad = opciones.length;
        if (cantidad > 0) {
            if (cantidad === 1) {
                AbrirWaiting("Buscando familia...");
                // Seleccionar solo el primer elemento
                const primerValor = opciones.first().val();
                $("#Rel011List").val([primerValor]);
               
                // Obtener el ID del proveedor seleccionado
                const proveedorId = $("#Rel011Item").val() || primerValor;

                cargarFliaDelProveedor(proveedorId);
              
                $("#chkRel03").prop("disabled", false);

                if (mostrarLog) {
                    console.log("Controles desactivados correctamente");
                }
                CerrarWaiting();
            }
            else {
                //hay más de 1 proveedor. Se desactiva la familia
                $("#chkRel03").prop("disabled", true);

                // Limpiar el dropdown actual
                $("#Rel03").prop("disabled", true).empty();
                $("Rel03List").prop("disabled", true).empty();                               
            }                       
        }
    } else if (mostrarLog && $("#Rel01").val()) {
        console.log("No hay opciones en Rel01List todavía, pero hay texto en Rel01");
    }
}

function cargarFliaDelProveedor(proveedorId) {
   
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