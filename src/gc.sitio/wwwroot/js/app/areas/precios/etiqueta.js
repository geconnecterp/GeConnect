let _presuLoading = false;

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
    $("#btnCancel").on("click", function () {
        window.location.href = homeEtiqueta;
    });

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
    $("#btnBuscar").on("click", function () {
        buscarEtiquetas(this);
    });

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

/* PSEUDOCODIGO (en comentarios):
- Dentro de `buscarEtiquetas`, armar 'proveedores' usando solo jQuery:
  - Tomar opciones con $("#Rel011List option")
  - Si hay opciones:
     - Recorrer con .each()
     - Tomar value, hacer trim y validar no vacío
     - Evitar duplicados con un diccionario 'visto'
     - Acumular en arreglo 'ids' y luego join(",")
  - Si no hay opciones:
     - Tomar #Rel011Item como fallback si tiene valor
*/
function buscarEtiquetas(btn) {
    if (_presuLoading) return;
    _presuLoading = true;

    const $btn = $(btn);
    const originalHtml = $btn.html();
    setBtnLoading($btn, true);

    let tipoVal = "";
    if ($("#chkTipoEtiq").is(":checked")) {
        tipoVal = $("#TipoEtiqueta").val();//*
    }

    let sinImp = $("#chkSinImprimir").is(":checked");//*
    let oferta = $("#chkOferta").is(":checked");//*


    let cargaPrevBit = false;
    let cargaPrevVal = "";
    if ($("#chkCargaPrevia").is(":checked")) {
        cargaPrevBit = true;
        cargaPrevVal = $("#CargaPrevia").val();
    }

    let fecD = "";
    let fecH = "";
    if ($("#chkDesdeHasta").is(":checked")) {
        fecD = $("#Date1").val();
        fecH = $("#Date2").val();
    }

    // Proveedores -> Array<string>
    let proveedores = [];
    if ($("#chkRel011").is(":checked")) {
        var $optsProv = $("#Rel011List").find("option");
        if ($optsProv.length > 0) {
            var vistoProv = {};
            $optsProv.each(function () {
                var v = $(this).val();
                if (v != null) {
                    v = String(v).trim();
                    if (v.length > 0 && !vistoProv[v]) {
                        vistoProv[v] = true;
                        proveedores.push(v);
                    }
                }
            });
        } else {
            var unicoProv = $("#Rel011Item").val();
            if (unicoProv != null) {
                unicoProv = String(unicoProv).trim();
                if (unicoProv.length > 0) {
                    proveedores.push(unicoProv);
                }
            }
        }
    }

    // Familias -> Array<string>
    let familias = [];
    if ($("#chkRel03").is(":checked")) {
        var $optsFam = $("#Rel03List").find("option");
        if ($optsFam.length > 0) {
            var vistoFam = {};
            $optsFam.each(function () {
                var v = $(this).val();
                if (v != null) {
                    v = String(v).trim();
                    if (v.length > 0 && !vistoFam[v]) {
                        vistoFam[v] = true;
                        familias.push(v);
                    }
                }
            });
        } else {
            var unicoFam = $("#Rel03List").val();
            if (unicoFam != null) {
                unicoFam = String(unicoFam).trim();
                if (unicoFam.length > 0) {
                    familias.push(unicoFam);
                }
            }
        }
    }

    let rubros = [];
    if ($("#chkRel02").is(":checked")) {
        var $optsRub = $("#Rel02List").find("option");
        if ($optsRub.length > 0) {
            var vistoRub = {};
            $optsRub.each(function () {
                var v = $(this).val();
                if (v != null) {
                    v = String(v).trim();
                    if (v.length > 0 && !vistoRub[v]) {
                        vistoRub[v] = true;
                        rubros.push(v);
                    }
                }
            });
        } else {
            var unicoRub = $("#Rel02List").val();
            if (unicoRub != null) {
                unicoRub = String(unicoRub).trim();
                if (unicoRub.length > 0) {
                    rubros.push(unicoRub);
                }
            }
        }
    }

    const data = {
        Tipo: tipoVal || null,
        Opt1: sinImp,
        Opt2: oferta,
        Opt3: cargaPrevBit,
        StrOpt03: cargaPrevVal || null,
        // ✅ FechaD y FechaH: null si vacío (compatible con DateTime?)
        FechaD: fecD && fecD.trim() !== "" ? fecD : null,
        FechaH: fecH && fecH.trim() !== "" ? fecH : null,
        // ✅ Rel01 y Rel02: Arrays de strings (OK)
        Rel01: proveedores.length > 0 ? proveedores : null,
        Rel02: rubros.length > 0 ? rubros : null,
        // ✅ Rel03: Convertir a List<ComboGenDto>
        Rel03: familias.length > 0
            ? familias.map(f => ({ Id: f, Descripcion: f }))
            : null,
        // ✅ Campos adicionales para compatibilidad completa
        Id: null,
        Id2: null,
        Buscar: null,
        Registros: null,
        Pagina: null,
        Estado: null,
        Adm_id: null,
        Usu_id: null
    };
   
    try {       
        const url = obtenerDetalleEtiquetasUrl;

        $.ajax({
            url: url,
            type: "POST",
            contentType: "application/json; charset=utf-8",
            dataType: "html",
            data: JSON.stringify(data),
            success: function (html) {
                $("#divDetalle").html(html).collapse("show");
                $("#divFiltro").collapse("hide");
            },
            error: function (xhr, status, error) {
                console.error("Error al obtener detalle de etiquetas:", error);
                $("#divDetalle").html('<div class="alert alert-danger py-2 mb-0"><i class="bx bx-error-circle me-1"></i>No se pudo obtener la información. Intente nuevamente.</div>').collapse("show");
            }
        });
    } finally {
        setBtnLoading($btn, false, originalHtml);
        _presuLoading = false;
    }
}

function configurarEventosSeleccionEtiqueta() {
    $(document).off("click", "#tbGridEtiquetaDetalle tbody tr");
    $(document).on("click", "#tbGridEtiquetaDetalle tbody tr", function (e) {
        if (!$(e.target).is("button, a, .btn, i")) {
            var $this = $(this);
            var fueSeleccionado = $this.hasClass("selected-row");

            $("#tbGridEtiquetaDetalle tbody tr").removeClass("selected-row");

            if (!fueSeleccionado) {
                $this.addClass("selected-row");
                let preId = $this.data("pre-id");

                if (preId) {
                    $("#btnImprimir").prop("disabled", false);
                    let data = { pre_id: preId };
                    cargarReporteEnArre(indexPrint, data, "Presupuesto/Cotización");
                }
            }

            //achico el tamaño del grid
            const $grid = $("#divPresupuesto");
            var gridAchicado = $grid.hasClass("table-wrapper-100");
            if (!gridAchicado) {
                $grid.removeClass("table-wrapper-300").addClass("table-wrapper-100")
            }
            setTimeout(() => {
                ///posiciona el select en la parte visual del grid al achicarlo
                posicionarRegOnTop($this, ".table-wrapper-100");
            }, 200);

        }
    });
    //configurando los eventos para el boton que elimina el registro.
    configurarEventosEliminacionProducto();
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

function setBtnLoading($btn, loading, originalHtml) {
    if (!$btn || !$btn.length) return;
    if (loading) {
        $btn.prop("disabled", true).html('<span class="spinner-border spinner-border-sm me-1"></span>Buscando...');
    } else {
        $btn.prop("disabled", false).html(originalHtml ?? "Buscar");
    }
}