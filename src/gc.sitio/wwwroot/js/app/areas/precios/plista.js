let _filterLoading = false;

$(function () {

    inicializaVista();
    inicializaEventosDeVista();
});

function inicializaVista() {
    // INICIALIZAMOS PANELES
    if ($("#divDetalle").is(":visible")) {
        $("#divDetalle").collapse("hide");
    }
    $("#divFiltro").collapse("show");

    $("#lbChkDesdeHasta").text("Modificados");
    $("#lbChkInCosto").text("Incluir Costo");

    $("#lbRel01").text("Proveedor");
    $("#lbRel03").text("Familias");
    $("#lbRel02").text("Rubros");


    $("#chkDesdeHasta").prop("checked", true);
    // Inicializar campos de fecha con un período de 3 meses
    // Date2 se establece con la fecha actual
    const hoy = new Date();
    const tresMesesAtras = new Date();
    tresMesesAtras.setMonth(hoy.getMonth() - 3);

    // Formatear fechas a YYYY-MM-DD para input type="date"
    const formatearFecha = (fecha) => {
        const año = fecha.getFullYear();
        const mes = String(fecha.getMonth() + 1).padStart(2, '0');
        const dia = String(fecha.getDate()).padStart(2, '0');
        return `${año}-${mes}-${dia}`;
    };

    $("#Date2").val(formatearFecha(hoy));
    $("#Date1").val(formatearFecha(tresMesesAtras));
}

function inicializaEventosDeVista() {

    $("#Rel01").on("click", function () { $(this).val(""); });
    //a los campos fecha "date1" y "date2" asignarle un periodo de tiempo de 3 meses. 
    //la fecha de date2 corresponde a la fecha de hoy

    $("#Rel01List").off("dblclick.removeOption").on("dblclick.removeOption", "option", function (e) {
        e.stopPropagation();
        $(this).remove();
        const $list = $("#Rel01List");
        if ($.fn.selectpicker && $list.hasClass("selectpicker")) {
            $list.selectpicker("refresh");
        }
    });
    $("#chkDesdeHasta").on("change", function () {
        const isChecked = $(this).is(":checked");
        $("#Date1, #Date2").prop("disabled", !isChecked);
    });

    $(document).on("change", "select#Rel01List", function () {
        verificarYDesactivarControles();
    });

    $(document).off("change.addRel03Item").on("change.addRel03Item", "select#Rel03", function () {
        const $origen = $(this);
        const $destino = $("#Rel03List");
        const $seleccionadas = $origen.find("option:selected");
        if ($seleccionadas.length === 0) return;

        let huboCambios = false;

        $seleccionadas.each(function () {
            const val = this.value;
            const txt = this.text;
            if (!val) return;

            const existe = $destino.find("option[value='" + $.escapeSelector(val) + "']").length > 0;
            if (!existe) {
                $destino.append($("<option></option>").val(val).text(txt));
                huboCambios = true;
            }
        });

        if (huboCambios) {
            if ($.fn.selectpicker && $destino.hasClass("selectpicker")) {
                $destino.selectpicker("refresh");
            }
            $destino.trigger("change");
        }

        $origen.val("");
        if ($.fn.selectpicker && $origen.hasClass("selectpicker")) {
            $origen.selectpicker("refresh");
        }
    });

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

    //****** Rubros    * /
    $(document).off("change.addRel02Item").on("change.addRel02Item", "select#Rel02", function () {
        const $origen = $(this);
        const $destino = $("#Rel02List");
        const $seleccionadas = $origen.find("option:selected");
        if ($seleccionadas.length === 0) return;

        let huboCambios = false;

        $seleccionadas.each(function () {
            const val = this.value;
            const txt = this.text;
            if (!val) return;

            const existe = $destino.find("option[value='" + $.escapeSelector(val) + "']").length > 0;
            if (!existe) {
                $destino.append($("<option></option>").val(val).text(txt));
                huboCambios = true;
            }
        });

        if (huboCambios) {
            if ($.fn.selectpicker && $destino.hasClass("selectpicker")) {
                $destino.selectpicker("refresh");
            }
            $destino.trigger("change");
        }

        $origen.val("");
        if ($.fn.selectpicker && $origen.hasClass("selectpicker")) {
            $origen.selectpicker("refresh");
        }
    });

    $("#Rel02List").off("dblclick.removeRel02Option").on("dblclick.removeRel02Option", "option", function (e) {
        e.stopPropagation();
        const $opcion = $(this);
        const $lista = $opcion.parent();
        $opcion.remove();

        if ($.fn.selectpicker && $lista.hasClass("selectpicker")) {
            $lista.selectpicker("refresh");
        }
        $lista.trigger("change");
    });
    //****** Fin rubros*/
    //****** lista de precios    * /
    $(document).off("change.addRel04Item").on("change.addRel04Item", "select#Rel04", function () {
        const $origen = $(this);
        const $destino = $("#Rel04List");
        const $seleccionadas = $origen.find("option:selected");
        if ($seleccionadas.length === 0) return;

        let huboCambios = false;

        $seleccionadas.each(function () {
            const val = this.value;
            const txt = this.text;
            if (!val) return;

            const existe = $destino.find("option[value='" + $.escapeSelector(val) + "']").length > 0;
            if (!existe) {
                $destino.append($("<option></option>").val(val).text(txt));
                huboCambios = true;
            }
        });

        if (huboCambios) {
            if ($.fn.selectpicker && $destino.hasClass("selectpicker")) {
                $destino.selectpicker("refresh");
            }
            $destino.trigger("change");
        }

        $origen.val("");
        if ($.fn.selectpicker && $origen.hasClass("selectpicker")) {
            $origen.selectpicker("refresh");
        }
    });

    $("#Rel04List").off("dblclick.removeRel04Option").on("dblclick.removeRel04Option", "option", function (e) {
        e.stopPropagation();
        const $opcion = $(this);
        const $lista = $opcion.parent();
        $opcion.remove();

        if ($.fn.selectpicker && $lista.hasClass("selectpicker")) {
            $lista.selectpicker("refresh");
        }
        $lista.trigger("change");
    });
    //****** Fin lista de precios*/
    $("#btnBuscar").on("click", function () {
        buscarPrecios(this);
    });
}

function buscarPrecios(btn) {
    if (_filterLoading) return;
    _filterLoading = true;

    const $btn = $(btn);
    const originalHtml = $btn.html();
    setBtnLoading($btn, true);

    // ✅ PASO 1.1: Extraer y validar fechas correctamente
    let fecD = null;
    let fecH = null;
    if ($("#chkDesdeHasta").is(":checked")) {
        const date1Val = $("#Date1").val();
        const date2Val = $("#Date2").val();
        
        // Solo asignar si tienen valor, de lo contrario dejar null
        fecD = (date1Val && date1Val.trim() !== "") ? date1Val.trim() : null;
        fecH = (date2Val && date2Val.trim() !== "") ? date2Val.trim() : null;
    }

    // ✅ PASO 1.2: Extraer valores de los selectores
    const incluirCosto = $("#chkInCosto").is(":checked");
    const proveedores = extraerValoresDeSelect("#Rel01List", "#Rel01Item", "#chkRel01");
    const familias = extraerValoresDeSelect("#Rel03List", null, "#chkRel03");
    const rubros = extraerValoresDeSelect("#Rel02List", null, "#chkRel02");
    const listas = extraerValoresDeSelect("#Rel04List", null, "#chkRel04");

    // ✅ PASO 1.3: Construir objeto con TODOS los campos de QueryFilters
    // CRÍTICO: La estructura debe coincidir EXACTAMENTE con QueryFilters.cs
    const data = {
        // Campos principales
        Id: null,
        Id2: null,
        Buscar: null,
        
        // Relaciones - ENVIAR ARRAYS SIMPLES, no objetos ComboGenDto
        Rel01: proveedores && proveedores.length > 0 ? proveedores : null,
        Rel02: rubros && rubros.length > 0 ? rubros : null,
        
        // ⚠️ CORRECCIÓN CRÍTICA: Rel03 y Rel04 deben ser arrays de objetos con propiedades en PascalCase
        Rel03: familias && familias.length > 0 
            ? familias.map(f => ({
                id: f,           // Minúscula para compatibilidad con deserializador
                descripcion: f   // Minúscula para compatibilidad con deserializador
              }))
            : null,
        
        Rel04: listas && listas.length > 0 
            ? listas.map(l => ({
                id: l,
                descripcion: l
              }))
            : null,
        
        Rel05: null,
        
        // Fechas - CRÍTICO: Enviar como strings o null, NO como strings vacíos
        Date1: fecD,
        Date2: fecH,
        FechaD: fecD,  // Mantener por compatibilidad
        FechaH: fecH,  // Mantener por compatibilidad
        
        // Paginación
        Registros: null,
        Pagina: null,
        
        // Estados y tipos
        Tipo: "",      // String vacío según QueryFilters
        Estado: "",    // String vacío según QueryFilters
        
        // Opciones booleanas
        Opt1: incluirCosto,
        Opt2: null,
        Opt3: null,
        Opt4: null,
        Opt5: null,
        
        // Opciones string
        StrOpt01: "",
        StrOpt02: "",
        StrOpt03: "",
        StrOpt04: "",
        StrOpt05: "",
        
        // Lista adicional
        ListNN: [],
        
        // Título
        TituloLeyend: "",
        
        // Campos de administración (el servidor los asigna)
        Adm_id: "",
        Usu_id: ""
    };

    // ✅ PASO 1.4: Log para debugging (eliminar en producción)
    console.log("📤 Datos enviados al servidor:", JSON.stringify(data, null, 2));

    // ✅ PASO 1.5: AJAX con manejo de errores mejorado
    $.ajax({
        url: obtenerDetallePreciosUrl,
        type: "POST",
        contentType: "application/json; charset=utf-8",
        dataType: "html",
        data: JSON.stringify(data),
        success: function (html) {
            console.log("✅ Respuesta recibida del servidor");
            
            $("#divDetalle").html(html).collapse("show");
            $("#divFiltro").collapse("hide");

            $("#btnAbmCancelar").show();
            $("#btnImprimir").prop("disabled", false).show();
            $("#btnAgregarEIProducto").prop("disabled", false);

            // Verificar que estas funciones existan
            if (typeof configurarEventosEliminacionEtiqueta === 'function') {
                configurarEventosEliminacionEtiqueta();
            }
            if (typeof configurarEventosSeleccionMultiple === 'function') {
                configurarEventosSeleccionMultiple();
            }
            if (typeof actualizarContadorEtiquetas === 'function') {
                actualizarContadorEtiquetas();
            }
        },
        error: function (xhr, status, error) {
            console.error("❌ Error en la solicitud AJAX:");
            console.error("  - Status:", status);
            console.error("  - Error:", error);
            console.error("  - Response:", xhr.responseText);
            console.error("  - Status Code:", xhr.status);
            
            let mensajeError = '<div class="alert alert-danger py-2 mb-0">' +
                '<i class="bx bx-error-circle me-1"></i>' +
                'No se pudo obtener la información de precios. ';
            
            // Mensaje específico según el código de error
            if (xhr.status === 400) {
                mensajeError += 'Parámetros inválidos. Verifique los filtros seleccionados.';
            } else if (xhr.status === 500) {
                mensajeError += 'Error en el servidor. Contacte al administrador.';
            } else {
                mensajeError += 'Intente nuevamente.';
            }
            
            mensajeError += '</div>';
            
            $("#divDetalle").html(mensajeError).collapse("show");
        },
        complete: function () {
            console.log("🏁 Solicitud AJAX completada");
            setBtnLoading($btn, false, originalHtml);
            _filterLoading = false;
        }
    });
}
function verificarYDesactivarControles(mostrarLog = true) {
    if ($("#Rel01List").find("option").length > 0) {
        if (mostrarLog) {
            console.log("Se encontraron opciones en Rel011List, verificando controles...");
        }

        const opciones = $("#Rel01List option");
        const cantidad = opciones.length;

        if (cantidad === 1) {
            AbrirWaiting("Buscando familias de productos...");

            const primerValor = opciones.first().val();
            $("#Rel01List").val([primerValor]);

            const proveedorId = $("#Rel01Item").val() || primerValor;
            cargarFliaDelProveedor(proveedorId);
            $("#chkRel03").prop("disabled", false);

            if (mostrarLog) {
                console.log("Controles actualizados correctamente");
            }
            CerrarWaiting();
        } else {
            $("#chkRel03").prop("disabled", true);
            $("#Rel03, #Rel03List").prop("disabled", true).empty();
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
    const datos = { ctaId: proveedorId };

    PostGen(datos, buscarFamiliaUrl,
        function (obj) {
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
                const combo = $("#Rel03");
                combo.empty();
                combo.append("<option value=''>Seleccionar...</option>");

                $.each(obj.lista, function (i, item) {
                    combo.append(`<option value='${item.id}'>${item.descripcion}</option>`);
                });
                CerrarWaiting();
            }
        },
        function (error) {
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
