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


    $("#chkDesdeHasta").prop("checked", false);
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

    // intentar mostrar al cargar
    try { MostrarFiltrosAplicados(); } catch (e) { }

    $("#divFiltro").on("shown.bs.collapse hidden.bs.collapse", function () {
        const abierto = $(this).hasClass("show");

        if (abierto) {
            $("#divDetalle").collapse("hide");
        } else {
            $("#divDetalle").collapse("show");
        }
    });
}

function MostrarFiltrosAplicados() {
    try {
        // preferir un contenedor flotante si existe, si no usar el container dentro del collapse
        const floatCont = $("#filtrosAplicadosFloating");
        const fallback = $("#filtrosAplicadosContainer");
        const cont = floatCont.length ? floatCont : (fallback.length ? fallback : null);
        if (!cont) return;

        const desde = $("#Date1").val();
        const hasta = $("#Date2").val();

        const listaPrecio = listFrom("Rel04List");
        const proveedor = listFrom("Rel01List");
        const familia = listFrom("Rel03List");
        const rubro = listFrom("Rel02List");

        const desdeHasta = $("#chkDesdeHasta").is(":checked");
        const incluyeCosto = $("#chkInCosto").is(":checked");

        let html = '<div class="d-inline-flex align-items-center" style="gap:8px;white-space:nowrap;">';
        if (desdeHasta) {
            if (desde) html += `<span class="badge bg-secondary">Desde: ${desde}</span>`;
            if (hasta) html += `<span class="badge bg-secondary">Hasta: ${hasta}</span>`;
        }
        if (incluyeCosto) {
            html += `<span class="badge bg-secondary">Incluye Costo</span>`;
        }

        html += renderGroup('LISTA', listaPrecio);
        html += renderGroup('PROV.', proveedor);
        html += renderGroup('FAM.', familia);
        html += renderGroup('RUBRO', rubro);
        html += '</div>';

        cont.html(html);
    } catch (e) {
        console.error('MostrarFiltrosAplicados error', e);
    }
}

function inicializaEventosDeVista() {

    $("#btnDetalle").on("mousedown", VerEstadoBtnDetalle);

    $("#btnImprimir").on("click", function () {
        imprimirReporteLP();
    });  

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
        try { MostrarFiltrosAplicados(); } catch (e) { console.warn('MostrarFiltrosAplicados no disponible:', e); }
        buscarPrecios(this);
    });
}

function VerEstadoBtnDetalle() {
    // Verificar si hay un asiento abierto (panel de detalle visible)
    if ($("#divDetalle").is(":visible") && $("#divpanel01").children().length > 0) {
        // Hay un asiento abierto, limpiarlo y cerrar el panel
        LimpiarGrid();
    }

    // Permitir que el evento siga propagándose (para que funcione el collapse)
    return true;
}

function LimpiarGrid() {
    $("#divDetalle").empty();

    inicializaVista();
}

function imprimirReporteLP() {
    const datos = obtenerParametrosInvocacion();
    
    cargarReporteEnArre(indexPrint, datos, "Reporte de Precios");
    const data = { modulo: "", parametros: [] };
    invocacionGestorDoc(data);
}

function obtenerParametrosInvocacion() {
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
    let data = {
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
        StrOpt01: familias && familias.length > 0
            ? familias.join(',')
            : null,   // o '' según lo qu,
        StrOpt02: listas && listas.length > 0
            ? listas.join(',')
            : null,   // o '' según lo qu,
        StrOpt03: proveedores && proveedores.length > 0
            ? proveedores.join(',')
            : null, 
        StrOpt04: rubros && rubros.length > 0
            ? rubros.join(',')
            : null, 
        StrOpt05: "",

        // Lista adicional
        ListNN: [],

        // Título
        TituloLeyend: "",

        // Campos de administración (el servidor los asigna)
        Adm_id: administracion.split('#')[0],
        Usu_id: usuarioAuth
    };

    return data;
}

function buscarPrecios(btn) {
    if (_filterLoading) return;
    _filterLoading = true;

    const $btn = $(btn);
    const originalHtml = $btn.html();
    setBtnLoading($btn, true);

    const data = obtenerParametrosInvocacion();
 
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

            // actualizar filtros aplicados (si el partial reemplaza el DOM)
            try { MostrarFiltrosAplicados(); } catch (e) { console.warn('MostrarFiltrosAplicados no disponible:', e); }

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

$("#Rel01").autocomplete({
    source: function (request, response) {

        data = { prefix: request.term }; Rel01

        $.ajax({
            url: autoComRel01Url,
            type: "POST",
            dataType: "json",
            data: data,
            success: function (obj) {
                response($.map(obj, function (item) {
                    var texto = item.descripcion;
                    return {
                        label: texto,
                        value: item.descripcion,
                        id: item.id,
                        prov: item.provId, tipo: "P"
                    };
                }));
            }
        })
    },
    minLength: 3,

    focus: function (event, ui) {
        // evita que el # aparezca mientras navegas con flechas
        const partes = ui.item.value.split("#");
        $("#Rel01").val(partes.join(" "));
        return false;
    },

    select: function (event, ui) {
        const partes = ui.item.value.split("#");
        const textoSinSeparador = partes.join(" ");

        // Mostrar SIN el "#"
        $("#Rel01").val(textoSinSeparador);

        if ($("#Rel01List").has('option:contains("' + ui.item.id + '")').length === 0) {
            $("#Rel01Item").val(ui.item.id);
            var opc = "<option value=" + ui.item.id + ">" + textoSinSeparador + "</option>"
            $("#Rel01List").append(opc);
            $("#Rel01List").trigger("change");
            consCta = ui.item.id;
            consRrss = ui.item.label;
            consTipo = ui.item.tipo;
        }

        event.preventDefault();
        return true;
    }
}).autocomplete("instance")._renderItem = function (ul, item) {

    const partes = item.label.split("#");

    const ctaLista = partes[0];
    const tipoDesc = partes[1];

    return $("<li>")
        .append(
            `<div>
                <span style="font-weight:bold; font-size:14px;">
                    ${ctaLista}
                </span>
                <span style="font-size:13px; color:#555;">
                    ${tipoDesc}
                </span>
            </div>`
        )
        .appendTo(ul);
};