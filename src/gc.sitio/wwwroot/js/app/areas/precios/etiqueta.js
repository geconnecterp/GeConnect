let _etiquetaLoading = false;

let _impresionPendiente = null;
let _reporteSolicitado = false;
let _reporteGenerado = false;
let _cierreGestorCancelado = false;
let _cierreGestorPorReporte = false;

$(function () {
    InicializaPantallaEtiqueta();
    InicializaEnventosEtiqueta();
});

function cancelarEtiqueta() {
    $("#btnAbmCancelar, #btnImprimir").prop("disabled", true);
    AbrirWaiting("Reiniciando impresión de etiquetas...");

    $.ajax({
        url: reiniciarEstadoEtiquetaUrl,
        type: "POST"
    }).always(function () {
        window.location.href = homeEtiqueta;
    });
}

function InicializaPantallaEtiqueta() {
    $("#btnAbmCancelar").hide();
    $("#btnImprimir").prop("disabled", true).hide();

    // INICIALIZAMOS PANELES
    if ($("#divDetalle").is(":visible")) {
        $("#divDetalle").collapse("hide");
    }
    $("#divFiltro").collapse("show");
    
    $("#chkTipoEtiq").prop("disabled", true);
    $("#OfertaTipoList").prop("disabled", true).val([]);
    $("#chkCargaPrevia").prop("checked", false);
    $("#CargaPrevia").prop("disabled", true);
    $("#lbCargaPrevia").text("Carga Previa");
    $("#lbChkDesdeHasta").text("Modificados");
    $("#divLs01 span").text("Proveedor");
    $("#lbRel01").text("Proveedor");
    $("#lbRel03").text("Familias");
    $("#lbRel02").text("Rubros");
    $("#lbNombreRel02").text("Rubro");
}

function InicializaEnventosEtiqueta() {
    $("#divFiltro")
        .off("show.bs.collapse.etiqueta")
        .on("show.bs.collapse.etiqueta", function () {
            // Volver a filtros inicia un nuevo contexto de consulta: la grilla
            // anterior no debe continuar habilitando la impresión.
            $("#btnImprimir").prop("disabled", true);
            $("#divDetalle").collapse("hide");

            _impresionPendiente = null;
            _reporteSolicitado = false;
            _reporteGenerado = false;
            _cierreGestorCancelado = false;
            _cierreGestorPorReporte = false;
        });

    $("#btnImprimir").on("click", function () {
        imprimirEtiquetas();
    });    

    $("#btnAbmCancelar").on("click", function () {
        cancelarEtiqueta();
    });

    $("#btnBuscar").on("click", function () {
        buscarEtiquetas(this);
    });

    $("#btnCancel").on("click", function () {
        cancelarEtiqueta();
    });

    $("#chkCargaPrevia").on("change", function () {
        const isChecked = $(this).is(":checked");
        $("#CargaPrevia").prop("disabled", !isChecked);
    });

    $("#chkOferta").on("change", function () {
        const filtrarOfertas = $(this).is(":checked");
        $("#OfertaTipoList").prop("disabled", !filtrarOfertas);

        if (!filtrarOfertas) {
            $("#OfertaTipoList").val([]);
        }
    });

    $("#chkDesdeHasta").on("change", function () {
        const isChecked = $(this).is(":checked");
        $("#Date1, #Date2").prop("disabled", !isChecked);

        if (isChecked) {
            const hoy = obtenerFechaActualInput();
            $("#Date1, #Date2").val(hoy);
        } else {
            $("#Date1, #Date2").val("");
        }
    });

    $("#chkRel011").on("change", function () {
        const isChecked = $(this).is(":checked");

        if (isChecked) {
            $("#Rel011, #Rel011List").prop("disabled", false);
            setTimeout(() => $("#Rel011").trigger("focus"), 50);
        } else {
            $("#Rel011").prop("disabled", true).val("");
            $("#Rel011List").prop("disabled", true).empty();
            $("#Rel011Item").val("");
            limpiarFiltroFamilias();
        }
    });

    $("#Rel011List").off("dblclick.removeOption").on("dblclick.removeOption", "option", function (e) {
        e.stopPropagation();
        $(this).remove();
        const $list = $("#Rel011List");
        if ($.fn.selectpicker && $list.hasClass("selectpicker")) {
            $list.selectpicker("refresh");
        }
        $list.trigger("change");
    });

    $("#Rel011").autocomplete({
            source: function (request, response) {
                $.ajax({
                    url: autoComRel01Url,
                    type: "POST",
                    dataType: "json",
                    data: { prefix: request.term },
                    success: function (obj) {
                        response($.map(obj, function (item) {
                            const proveedor = normalizarProveedorAutocomplete(item);
                            proveedor.nombre = item.nombre || proveedor.label;
                            proveedor.domicilio = item.domicilio || "";
                            return proveedor;
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

    aplicarRenderProveedorAutocomplete($("#Rel011"));

    $(document).on("change", "select#Rel011List", function () {
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
            $destino.trigger("change");
        }

        $origen.val("");
        if ($.fn.selectpicker && $origen.hasClass("selectpicker")) {
            $origen.selectpicker("refresh");
        }
    });

    $("#Rel02List").off("dblclick.removeRel02Option").on("dblclick.removeRel02Option", "option", function (e) {
        e.stopPropagation();
        const $lista = $(this).parent();
        $(this).remove();

        if ($.fn.selectpicker && $lista.hasClass("selectpicker")) {
            $lista.selectpicker("refresh");
        }
        $lista.trigger("change");
    });

    // ✅ NUEVO: Configurar eventos de búsqueda avanzada
    configurarEventosBusquedaAvanzadaEtiquetas();
    
    configurarEventosEliminacionEtiqueta();
    configurarEventosSeleccionMultiple();
}

function obtenerFechaActualInput() {
    const hoy = new Date();
    const anio = hoy.getFullYear();
    const mes = String(hoy.getMonth() + 1).padStart(2, "0");
    const dia = String(hoy.getDate()).padStart(2, "0");
    return `${anio}-${mes}-${dia}`;
}

// ============================================================================
// ✅ NUEVAS FUNCIONES: INTEGRACIÓN CON BÚSQUEDA AVANZADA V02
// ============================================================================

/**
 * ✅ NUEVA: Configura el evento del botón para abrir búsqueda avanzada
 */
function configurarEventosBusquedaAvanzadaEtiquetas() {
    $(document).off("click.agregarEtiqueta", "#btnAgregarEIProducto");
    
    $(document).on("click.agregarEtiqueta", "#btnAgregarEIProducto", function (e) {
        e.preventDefault();
        
        if ($(this).prop("disabled")) {
            console.warn("⚠️ Botón de agregar etiquetas está deshabilitado");
            return;
        }
        
        console.log("🔍 Abriendo búsqueda avanzada para etiquetas...");
        abrirBusquedaAvanzadaEtiquetas();
    });
}

/**
 * ✅ NUEVA: Abre el modal de búsqueda avanzada configurado para etiquetas
 */
function abrirBusquedaAvanzadaEtiquetas() {
    // Verificar si el modal ya existe en el DOM
    if ($("#busquedaModal").length === 0) {
        console.log("📦 Cargando modal de búsqueda avanzada...");
        cargarModalBusquedaAvanzadaEtiquetas(function () {
            configurarYMostrarModalEtiquetas();
        });
    } else {
        configurarYMostrarModalEtiquetas();
    }
}

/**
 * ✅ NUEVA: Carga el modal de búsqueda avanzada (similar a presup.js)
 */
function cargarModalBusquedaAvanzadaEtiquetas(callback) {
    const urlModal = typeof busquedaAvanzadaModalUrl !== 'undefined'
        ? busquedaAvanzadaModalUrl
        : '/ControlComun/Producto/BusquedaAvanzadaV02';

    $.ajax({
        url: urlModal,
        type: 'GET',
        success: function (html) {
            if ($("#busquedaModal").length === 0) {
                $('body').append(html);
                console.log("✅ Modal de búsqueda cargado correctamente");
            }
            
            if (typeof callback === 'function') {
                callback();
            }
        },
        error: function (xhr, status, error) {
            console.error("❌ Error al cargar modal de búsqueda:", error);
            ControlaMensajeError("No se pudo cargar el módulo de búsqueda de productos");
        }
    });
}

/**
 * ✅ NUEVA: Configura y muestra el modal para etiquetas
 */
function configurarYMostrarModalEtiquetas() {
    if (typeof configurarDestinoBusquedaProductos === 'function') {
        console.log("⚙️ Configurando búsqueda para destino 'etiquetas'");
        configurarDestinoBusquedaProductos(
            "etiquetas",
            "001",
            agregarProductosAlGridEtiquetas,
            obtenerProductosEtiquetasExistentes
        );
    } else {
        console.error("❌ Función configurarDestinoBusquedaProductos no está disponible");
        ControlaMensajeError("Error: Módulo de búsqueda no cargado correctamente");
        return;
    }
    
    $("#busquedaModal").modal("show");
    console.log("✅ Modal de búsqueda mostrado");
}

/**
 * ✅ NUEVA: Obtiene los IDs de productos que ya existen en el grid de etiquetas
 * @returns {Array<string>} Array de IDs de productos
 */
function obtenerProductosEtiquetasExistentes() {
    const productosIds = [];
    
    $("#tbGridEtiquetaDetalle tbody tr:not(.empty-message)").each(function () {
        const $fila = $(this);
        const pId = $fila.data("p-id");
        
        if (pId) {
            productosIds.push(String(pId));
        }
    });
    
    console.log(`📋 ${productosIds.length} producto(s) ya existente(s) en el grid de etiquetas`);
    return productosIds;
}

/**
 * ✅ NUEVA: Agrega productos al grid de etiquetas (callback de búsqueda)
 * @param {Array<Object>} productos - Array de productos seleccionados
 */
function agregarProductosAlGridEtiquetas(productos) {
    if (!Array.isArray(productos) || productos.length === 0) {
        console.warn("⚠️ No hay productos para agregar");
        return;
    }
    
    console.log(`➕ Agregando ${productos.length} producto(s) al grid de etiquetas`);
    
    const $tbody = $("#tbGridEtiquetaDetalle tbody");
    
    // Eliminar fila de mensaje vacío si existe
    const $filaVacia = $tbody.find("tr.empty-message");
    if ($filaVacia.length > 0) {
        $filaVacia.remove();
        console.log("🗑️ Fila de mensaje vacío eliminada");
    }
    
    // Determinar si la próxima fila debe tener clase "alt"
    let esAlternado = $tbody.find("tr:not(.empty-message)").length % 2 !== 0;
    
    // Agregar cada producto al grid
    productos.forEach(function (producto, index) {
        const fila = crearFilaEtiqueta(producto, esAlternado);
        $tbody.append(fila);
        esAlternado = !esAlternado;
        console.log(`✅ Producto ${producto.p_id} agregado al grid`);
    });
    
    // Actualizar contador
    actualizarContadorEtiquetas();
    
    // Reconfigurar eventos (importante para las nuevas filas)
    configurarEventosEliminacionEtiqueta();
    configurarEventosSeleccionMultiple();
    
    console.log(`✅ ${productos.length} producto(s) agregado(s) exitosamente`);
}

/**
 * ✅ NUEVA: Crea el HTML de una fila de etiqueta
 * @param {Object} producto - Objeto producto con p_id y p_desc
 * @param {boolean} esAlternado - Si debe aplicar clase "alt"
 * @returns {string} HTML de la fila
 */
function crearFilaEtiqueta(producto, esAlternado) {
    const claseAlt = esAlternado ? "alt" : "";
    const pId = escaparHTML(producto.p_id || "");
    const pDesc = escaparHTML(producto.p_desc || "Sin descripción");
    
    // Las etiquetas nuevas siempre están pendientes de impresión
    const estadoClase = "text-success fw-semibold";
    const estadoIcono = "bx bx-time-five";
    const estadoTexto = "Pendiente";
    
    return `
        <tr class="${claseAlt}" data-p-id="${pId}">
            <td class="text-center td-compact">
                <div class="form-check d-flex justify-content-center mb-0">
                    <input class="form-check-input chk-etiqueta-item" 
                           type="checkbox" 
                           value="${pId}" 
                           data-p-desc="${pDesc}"
                           id="chk_${pId}">
                </div>
            </td>
            <td class="text-center td-compact">${pId}</td>
            <td class="td-compact etiqueta-descripcion" title="${pDesc}">${pDesc}</td>
            <td class="text-center td-compact ${estadoClase}">
                <i class="${estadoIcono} me-1"></i>${estadoTexto}
            </td>
            <td class="text-center td-compact">
                <button type="button"
                        class="btn btn-sm btn-outline-danger btn-eliminar-etiqueta"
                        data-p-id="${pId}"
                        title="Eliminar etiqueta">
                    <i class="bx bx-trash"></i>
                </button>
            </td>
        </tr>
    `;
}

/**
 * ✅ NUEVA: Escapa caracteres HTML para prevenir XSS
 * @param {string} texto - Texto a escapar
 * @returns {string} Texto escapado
 */
function escaparHTML(texto) {
    const div = document.createElement('div');
    div.textContent = String(texto);
    return div.innerHTML;
}

// ============================================================================
// FUNCIONES EXISTENTES (MANTENER)
// ============================================================================

function determinarIndiceImpresion(tipoEt) {
    const mapaIndices = { "0": 45, "1": 46, "2": 47 };
    return mapaIndices[tipoEt];
}

function imprimirEtiquetas() {
    const tipoEt = $("#TipoEtiqueta").val();
    const tipoDesc = $("#TipoEtiqueta option:selected").text();
    
    if (!tipoEt) {
        mostrarNotificacion("Debe seleccionar un tipo de etiqueta", "warning");
        return;
    }

    const indexImp = determinarIndiceImpresion(tipoEt);
    
    if (!indexImp) {
        mostrarNotificacion("Tipo de etiqueta no válido", "error");
        return false;
    }

    const productosSeleccionados = [];
    $(".chk-etiqueta-item:checked").each(function () {
        const pId = $(this).val();
        if (pId) {
            productosSeleccionados.push({ p_id: pId });
        }
    });

    if (productosSeleccionados.length === 0) {
        AbrirMensaje(
            "A tener en cuenta", 
            "Debe seleccionar al menos un producto para imprimir etiquetas",
            function () {
                $("#msjModal").modal("hide");
            }, 
            false, 
            ["Continuar"], 
            "warn!", 
            null
        );        
        return;
    }

    const adm_id = administracion;
    const productos = JSON.stringify(productosSeleccionados);
    const info = {
        json_p: productos,
        etiqueta: Number(tipoEt),
        adm_id: adm_id,
        usu_id: usuarioAuth
    };

    _impresionPendiente = {
        tipo: tipoEt,
        reporte: indexImp,
        json: productos,
        cantidad: productosSeleccionados.length
    };

    cargarReporteEnArre(45, {}, "");
    cargarReporteEnArre(46, {}, "");
    cargarReporteEnArre(47, {}, "");
    cargarReporteEnArre(indexImp, info, tipoDesc);

    const data = { modulo: "", parametros: [] };
    invocacionGestorDoc(data);

    setTimeout(() => {
        configurarEventoCierreModal(tipoEt);
    }, 300);
}

function configurarEventoCierreModal(tipoEt) {
    const $modal = $('#docmgrmodal');
    
    if ($modal.length === 0) {
        console.warn("⚠️ Modal #docmgrmodal no encontrado en el DOM");
        return;
    }

    $modal.off('hidden.bs.modal.confirmarImpresion');
    $modal.off('hide.bs.modal.confirmarImpresion');
    _reporteSolicitado = false;
    _reporteGenerado = false;
    _cierreGestorCancelado = false;
    _cierreGestorPorReporte = false;

    $(document)
        .off('click.etiquetaReporte', '#btnArchImprimir')
        .on('click.etiquetaReporte', '#btnArchImprimir', function () {
            const seleccionados = $('#archivosDispuestos').jstree('get_selected', true)
                .filter(node => node.parent !== "#" && node.parent !== null);
            _reporteSolicitado = seleccionados.length > 0;
        })
        .off('click.etiquetaCancelarGestor', '#btnCancelarGD, #docmgrmodal .modal-header [data-bs-dismiss="modal"]')
        .on('click.etiquetaCancelarGestor', '#btnCancelarGD, #docmgrmodal .modal-header [data-bs-dismiss="modal"]', function () {
            _cierreGestorCancelado = true;
            _reporteSolicitado = false;
            _reporteGenerado = false;
            _impresionPendiente = null;
        })
        .off('gestorDocumental:reporteAbierto.etiquetaReporte')
        .on('gestorDocumental:reporteAbierto.etiquetaReporte', function () {
            if (_cierreGestorCancelado || !_reporteSolicitado || !_impresionPendiente) {
                return;
            }

            _reporteGenerado = true;
            _cierreGestorPorReporte = true;
            $modal.modal('hide');
        })
        .off('gestorDocumental:reporteBloqueado.etiquetaReporte')
        .on('gestorDocumental:reporteBloqueado.etiquetaReporte', function () {
            if (!_reporteSolicitado || !_impresionPendiente) {
                return;
            }

            _reporteSolicitado = false;
            mostrarNotificacion(
                "El navegador bloqueó la apertura del reporte. Habilite las ventanas emergentes e intente nuevamente.",
                "warning"
            );
        });

    $modal.on('hidden.bs.modal.confirmarImpresion', function () {
        $modal.off('hidden.bs.modal.confirmarImpresion');
        $(document)
            .off('click.etiquetaReporte', '#btnArchImprimir')
            .off('click.etiquetaCancelarGestor', '#btnCancelarGD, #docmgrmodal .modal-header [data-bs-dismiss="modal"]')
            .off('gestorDocumental:reporteAbierto.etiquetaReporte')
            .off('gestorDocumental:reporteBloqueado.etiquetaReporte');

        const reporteGenerado = _reporteGenerado &&
            _cierreGestorPorReporte &&
            !_cierreGestorCancelado;

        _reporteSolicitado = false;
        _reporteGenerado = false;
        _cierreGestorPorReporte = false;

        if (!reporteGenerado) {
            _impresionPendiente = null;
            return;
        }

        setTimeout(() => {
            if (tipoEt === "0") {
                mostrarNotificacion("El reporte se generó correctamente", "success");
                setTimeout(cancelarEtiqueta, 700);
                return;
            }

            mostrarConfirmacionDeImpresion();
        }, 200);
    });
}

function mostrarConfirmacionDeImpresion() {
    AbrirMensaje(
        "Confirmación",
        "¿Se generó correctamente el reporte?",
        function (resp) {
            if (resp === 'SI' || resp === 'Sí') {
                ConfirmarImpresionOK();
            } else {
                _impresionPendiente = null;
                mostrarNotificacion(
                    "La impresión no fue confirmada. Puede revisar el reporte e intentarlo nuevamente.",
                    "warning"
                );
            }
            $("#msjModal").modal("hide");
        },
        true,
        ["Sí", "No"],
        "info!"
    );
}

function ConfirmarImpresionOK() {
    if (!_impresionPendiente?.json) {
        mostrarNotificacion("No se encontró una impresión pendiente para confirmar", "error");
        return;
    }
   
    const request = {
        json: _impresionPendiente.json,
        adm: "",
        usu: ""        
    };

    AbrirWaiting("Confirmando impresión de etiquetas...");

    $.ajax({
        url: confirmarImpresionEtiquetaUrl,
        type: "POST",
        contentType: "application/json; charset=utf-8",
        dataType: "json",
        data: JSON.stringify(request),
        success: function (response) {
            CerrarWaiting();

            if (response && response.ok) {
                const mensaje = response.mensaje || 
                    `Impresión de ${_impresionPendiente.cantidad} etiqueta(s) confirmada correctamente`;
                
                mostrarNotificacion(mensaje, "success");

                limpiarSeleccionEtiquetas();

                setTimeout(() => {
                    cancelarEtiqueta();
                }, 1500);
            } else {
                const mensajeError = response?.mensaje || "No se pudo confirmar la impresión";
                mostrarNotificacion(mensajeError, "error");
            }
        },
        error: function (xhr, status, error) {
            CerrarWaiting();
            console.error("❌ Error al confirmar impresión:", error);

            let mensajeError = "Error al confirmar la impresión de etiquetas.";
            
            if (xhr.responseJSON?.mensaje) {
                mensajeError = xhr.responseJSON.mensaje;
            } else if (xhr.status === 401) {
                mensajeError = "Sesión expirada. Por favor, inicie sesión nuevamente.";
            } else if (xhr.status === 0) {
                mensajeError = "No se pudo conectar con el servidor. Verifique su conexión.";
            }

            mostrarNotificacion(mensajeError, "error");
        }
    });
}

function buscarEtiquetas(btn) {
    if (_etiquetaLoading) return;

    const tipoVal = $("#chkTipoEtiq").is(":checked") ? $("#TipoEtiqueta").val() : "";
    const sinImp = $("#chkSinImprimir").is(":checked");
    const oferta = $("#chkOferta").is(":checked");
    const tiposOferta = oferta ? ($("#OfertaTipoList").val() || []) : [];

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

        if (!fecD || !fecH) {
            mostrarNotificacion("Debe indicar las fechas desde y hasta", "warning");
            return;
        }

        if (fecD > fecH) {
            mostrarNotificacion("La fecha desde no puede ser posterior a la fecha hasta", "warning");
            return;
        }
    }

    if (cargaPrevBit && !cargaPrevVal) {
        mostrarNotificacion("Debe seleccionar una carga previa", "warning");
        return;
    }

    const proveedores = extraerValoresDeSelect("#Rel011List", "#Rel011Item", "#chkRel011");
    const familias = extraerValoresDeSelect("#Rel03List", null, "#chkRel03");
    const rubros = extraerValoresDeSelect("#Rel02List", null, "#chkRel02");

    const data = {
        Tipo: tipoVal || null,
        Opt1: sinImp,
        Opt2: oferta,
        OfertaList: tiposOferta,
        Opt3: cargaPrevBit,
        StrOpt03: cargaPrevVal || null,
        FechaD: fecD && fecD.trim() !== "" ? fecD : null,
        FechaH: fecH && fecH.trim() !== "" ? fecH : null,
        Rel01: proveedores.length > 0 ? proveedores : null,
        Rel02: rubros.length > 0 ? rubros : null,
        Rel03: familias.length > 0 
            ? familias.map(f => ({ Id: f, Descripcion: f }))
            : null,
        Id: null,
        Id2: null,
        Buscar: null,
        Registros: null,
        Pagina: null,
        Estado: null,
        Adm_id: null,
        Usu_id: null
    };

    // La impresión sólo vuelve a habilitarse cuando la nueva consulta
    // finaliza y contiene etiquetas.
    $("#btnImprimir").prop("disabled", true);

    _etiquetaLoading = true;
    const $btn = $(btn);
    const originalHtml = $btn.html();
    setBtnLoading($btn, true);
   
    $.ajax({
        url: obtenerDetalleEtiquetasUrl,
        type: "POST",
        contentType: "application/json; charset=utf-8",
        dataType: "html",
        data: JSON.stringify(data),
        success: function (html) {
            $("#divDetalle").html(html).collapse("show");
            $("#divFiltro").collapse("hide");

            $("#btnAbmCancelar").show();
            const hayEtiquetas = $(".chk-etiqueta-item").length > 0;
            $("#btnImprimir").prop("disabled", !hayEtiquetas).show();
            $("#btnAgregarEIProducto").prop("disabled", false);

            configurarEventosEliminacionEtiqueta();
            configurarEventosSeleccionMultiple();
            actualizarContadorEtiquetas();
        },
        error: function (xhr, status, error) {
            console.error("Error al obtener detalle de etiquetas:", error);
            const detalle = xhr.responseJSON?.mensaje ||
                "No se pudo obtener la información de etiquetas. Revise los filtros e intente nuevamente.";
            const mensajeError = '<div class="alert alert-danger py-2 mb-0">' +
                '<i class="bx bx-error-circle me-1"></i>' +
                escaparHTML(detalle) +
                '</div>';
            $("#divDetalle").html(mensajeError).collapse("show");
        },
        complete: function () {
            setBtnLoading($btn, false, originalHtml);
            _etiquetaLoading = false;
        }
    });
}


function configurarEventosSeleccionMultiple() {
    $(document).off("change.seleccionarTodas", "#chkSeleccionarTodas");
    $(document).off("change.itemSeleccionado", ".chk-etiqueta-item");
    
    $(document).on("change.seleccionarTodas", "#chkSeleccionarTodas", function () {
        const isChecked = $(this).is(":checked");
        $(".chk-etiqueta-item").prop("checked", isChecked);
        actualizarEstadoSeleccion();
    });
    
    $(document).on("change.itemSeleccionado", ".chk-etiqueta-item", function () {
        actualizarCheckboxPrincipal();
        actualizarEstadoSeleccion();
    });
}

function actualizarCheckboxPrincipal() {
    const $checkboxes = $(".chk-etiqueta-item");
    const totalCheckboxes = $checkboxes.length;
    const checkedCheckboxes = $checkboxes.filter(":checked").length;
    
    const $chkPrincipal = $("#chkSeleccionarTodas");
    
    if (checkedCheckboxes === 0) {
        $chkPrincipal.prop("checked", false);
        $chkPrincipal.prop("indeterminate", false);
    } else if (checkedCheckboxes === totalCheckboxes) {
        $chkPrincipal.prop("checked", true);
        $chkPrincipal.prop("indeterminate", false);
    } else {
        $chkPrincipal.prop("checked", false);
        $chkPrincipal.prop("indeterminate", true);
    }
}

function actualizarEstadoSeleccion() {
    const $seleccionadas = $(".chk-etiqueta-item:checked");
    const cantidad = $seleccionadas.length;

    if (cantidad > 0) {
        const textoSeleccion = cantidad === 1 
            ? "1 etiqueta seleccionada" 
            : `${cantidad} etiquetas seleccionadas`;
        $("#txtSeleccionadas").text(textoSeleccion);
    } else {
        $("#txtSeleccionadas").text("Ninguna seleccionada");
    }
}

function obtenerEtiquetasSeleccionadas() {
    const etiquetas = [];
    
    $(".chk-etiqueta-item:checked").each(function () {
        const $checkbox = $(this);
        etiquetas.push({
            id: $checkbox.val(),
            descripcion: $checkbox.data("p-desc")
        });
    });
    
    return etiquetas;
}

function eliminarEtiquetasSeleccionadas() {
    const etiquetasSeleccionadas = obtenerEtiquetasSeleccionadas();
    
    if (etiquetasSeleccionadas.length === 0) {
        mostrarNotificacion("No hay etiquetas seleccionadas", "warning");
        return;
    }
    
    const cantidad = etiquetasSeleccionadas.length;
    const listaEtiquetas = etiquetasSeleccionadas
        .map(e => `<li>${e.id} - ${e.descripcion}</li>`)
        .join("");
    
    const mensaje = `¿Está seguro de eliminar ${cantidad} etiqueta(s) de la vista?<br><br>
        <div style="max-height: 200px; overflow-y: auto;">
            <ul class="list-unstyled small text-start">${listaEtiquetas}</ul>
        </div>
        <small class="text-muted">Esta acción solo eliminará las etiquetas de la vista actual.</small>`;
    
    if (typeof AbrirMensaje === "function") {
        AbrirMensaje(
            "CONFIRMAR ELIMINACIÓN MÚLTIPLE",
            mensaje,
            function () {
                ejecutarEliminacionMultiple(etiquetasSeleccionadas);
                $("#msjModal").modal("hide");
            },
            true,
            ["Eliminar Todas", "Cancelar"],
            "warning!",
            null
        );
    } else {
        if (confirm(`¿Está seguro de eliminar ${cantidad} etiquetas?`)) {
            ejecutarEliminacionMultiple(etiquetasSeleccionadas);
        }
    }
}

function ejecutarEliminacionMultiple(etiquetas) {
    const $btnEliminar = $("#btnEliminarSeleccionadas");
    const originalHtml = $btnEliminar.html();
    
    $btnEliminar.prop("disabled", true)
        .html('<span class="spinner-border spinner-border-sm me-1"></span>Eliminando...');
    
    let eliminadas = 0;
    
    etiquetas.forEach((etiqueta, index) => {
        const $fila = $(`tr[data-p-id="${etiqueta.id}"]`);
        
        setTimeout(() => {
            $fila.fadeOut(300, function () {
                $(this).remove();
                eliminadas++;
                
                if (eliminadas === etiquetas.length) {
                    actualizarContadorEtiquetas();
                    renumerarFilasEtiquetas();
                    verificarEtiquetasVacias();
                    limpiarSeleccionEtiquetas();
                    
                    $btnEliminar.prop("disabled", false).html(originalHtml);
                    
                    const mensaje = eliminadas === 1 
                        ? "1 etiqueta eliminada" 
                        : `${eliminadas} etiquetas eliminadas`;
                    mostrarNotificacion(mensaje, "success");
                    
                    console.log(`${eliminadas} etiquetas eliminadas del grid`);
                }
            });
        }, index * 100);
    });
}

function limpiarSeleccionEtiquetas() {
    $(".chk-etiqueta-item").prop("checked", false);
    $("#chkSeleccionarTodas").prop("checked", false).prop("indeterminate", false);
    actualizarEstadoSeleccion();
}

function configurarEventosEliminacionEtiqueta() {
    $(document).off("click.eliminarEtiqueta", ".btn-eliminar-etiqueta");
    
    $(document).on("click.eliminarEtiqueta", ".btn-eliminar-etiqueta", function (e) {
        e.preventDefault();
        e.stopPropagation();
        
        const $btn = $(this);
        const $fila = $btn.closest("tr");
        const etiquetaId = $btn.data("p-id");
        const descripcion = $fila.find("td").eq(2).text().trim();
        
        if (!etiquetaId) {
            console.error("No se pudo obtener el ID de la etiqueta");
            mostrarNotificacion("Error al identificar la etiqueta", "error");
            return;
        }
        
        confirmarEliminacionEtiqueta(etiquetaId, descripcion, $fila, $btn);
    });
}

function confirmarEliminacionEtiqueta(etiquetaId, descripcion, $fila, $btn) {
    const mensaje = `¿Está seguro de eliminar la etiqueta de la vista?<br><br>
        <strong>Código:</strong> ${etiquetaId}<br>
        <strong>Descripción:</strong> ${descripcion}<br><br>
        <small class="text-muted">Esta acción solo eliminará la etiqueta de la vista actual.</small>`;
    
    if (typeof AbrirMensaje === "function") {
        AbrirMensaje(
            "CONFIRMAR ELIMINACIÓN DE ETIQUETA",
            mensaje,
            function () {
                ejecutarEliminacionEtiqueta(etiquetaId, $fila, $btn);
                $("#msjModal").modal("hide");
            },
            true,
            ["Eliminar", "Cancelar"],
            "warning!",
            null
        );
    } else {
        if (confirm(`¿Está seguro de eliminar la etiqueta ${etiquetaId} - ${descripcion}?`)) {
            ejecutarEliminacionEtiqueta(etiquetaId, $fila, $btn);
        }
    }
}

function ejecutarEliminacionEtiqueta(etiquetaId, $fila, $btn) {
    $btn.prop("disabled", true).html('<span class="spinner-border spinner-border-sm"></span>');
    
    $fila.fadeOut(300, function () {
        $fila.remove();
        actualizarContadorEtiquetas();
        renumerarFilasEtiquetas();
        verificarEtiquetasVacias();
        actualizarCheckboxPrincipal();
        actualizarEstadoSeleccion();
        mostrarNotificacion(`Etiqueta ${etiquetaId} eliminada de la vista`, "success");
        console.log(`Etiqueta ${etiquetaId} eliminada del grid`);
    });
}

function actualizarContadorEtiquetas() {
    const $tbody = $("#tbGridEtiquetaDetalle tbody");
    const $filas = $tbody.find("tr:not(.empty-message)");
    const totalRegistros = $filas.length;

    $("#totalEtiquetasMostradas").text(totalRegistros);
}

function renumerarFilasEtiquetas() {
    const $tbody = $("#tbGridEtiquetaDetalle tbody");
    const $filas = $tbody.find("tr:not(.empty-message)");
    
    $filas.each(function (index) {
        const $fila = $(this);
        
        if ((index + 1) % 2 === 0) {
            $fila.removeClass("alt");
        } else {
            $fila.addClass("alt");
        }
    });
}

function verificarEtiquetasVacias() {
    const $tbody = $("#tbGridEtiquetaDetalle tbody");
    const $filas = $tbody.find("tr:not(.empty-message)");
    
    if ($filas.length === 0) {
        const mensajeVacio = `
            <tr class="empty-message">
                <td colspan="5" class="text-center text-muted py-3">
                    <i class="bx bx-info-circle me-1"></i>
                    No hay etiquetas en la vista actual
                </td>
            </tr>`;
        
        $tbody.html(mensajeVacio);
        $("#txtSeleccionadas").text("Ninguna seleccionada");
    }
}

function mostrarNotificacion(mensaje, tipo = "info") {
    switch (tipo) {
        case "info":
            ControlaMensajeInfo(mensaje);
            break;
        case "error":
            ControlaMensajeError(mensaje);
            break;
        case "warning":
            ControlaMensajeWarning(mensaje);
            break;
        case "success":
            ControlaMensajeSuccess(mensaje);
            break;
        default:
            return false;
    }
}

function limpiarFiltroFamilias() {
    $("#chkRel03").prop("checked", false).prop("disabled", true);
    $("#Rel03").prop("disabled", true).empty()
        .append("<option value=''>Seleccionar...</option>");
    $("#Rel03List").prop("disabled", true).empty();
}

function verificarYDesactivarControles() {
    const opciones = $("#Rel011List option");
    const cantidad = opciones.length;

    limpiarFiltroFamilias();

    if (cantidad !== 1) {
        if (cantidad === 0) {
            $("#Rel011Item").val("");
        }
        return;
    }

    const proveedorId = opciones.first().val();
    $("#Rel011List").val([proveedorId]);
    $("#Rel011Item").val(proveedorId);
    $("#chkRel03").prop("disabled", false);

    AbrirWaiting("Buscando familias de productos...");
    cargarFliaDelProveedor(proveedorId);
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
            CerrarWaiting();
            console.error("Error al cargar las familias del proveedor:", error);
            mostrarNotificacion("No se pudieron obtener las familias del proveedor", "error");
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

