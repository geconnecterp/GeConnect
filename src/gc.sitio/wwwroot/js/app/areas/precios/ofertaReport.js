$(function () {
    inicializaEventos();
    inicializarVista();
});

function inicializarVista() {
    // INICIALIZAMOS PANELES
    if ($("#divDetalle").is(":visible")) {
        $("#divDetalle").collapse("hide");
    }
    $("#divFiltro").collapse("show");

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

        const oferta = $("#rbOferta").is(":checked");
        const combo = $("#rbCombo").is(":checked");
        const tipoTextSele = $("#Tipo option:selected").text();
        var tipoText = "";
        if (tipoTextSele && tipoTextSele.toUpperCase() != "SELECCIONAR...")
            tipoText = $("#Tipo option:selected").text() || "Todos";

        let html = '<div class="d-inline-flex align-items-center" style="gap:8px;white-space:nowrap;">';
        if (tipoText && tipoText != "") {
            html += `<span class="badge bg-secondary">Tipo: ${tipoText}</span>`;
        }
        if (oferta) {
            html += `<span class="badge bg-secondary">Incluye Oferta</span>`;
        }
        if (combo) {
            html += `<span class="badge bg-secondary">Incluye Combos</span>`;
        }

        html += '</div>';

        cont.html(html);
    } catch (e) {
        console.error('MostrarFiltrosAplicados error', e);
    }
}

// intentar mostrar al cargar
try { MostrarFiltrosAplicados(); } catch (e) { }

function inicializaEventos() {
    //evento para el boton imprimir
    $(document).on("click", "#btnImprimir", function () {
        imprimirReporteOf();
    });

    $("#chkTipo").on("change", function () {
        $("#Tipo").prop("disabled", !$(this).is(":checked"));
    });

    // Configurar el evento click para el botón Buscar/Filtrar
    $("#btnBuscar").on("click", function (e) {
        // actualizar vista de filtros antes de buscar
        try { MostrarFiltrosAplicados(); } catch (e) { console.warn('MostrarFiltrosAplicados no disponible:', e); }
        SeleccionReporte(e, $(this));
    });

    $("#btnCancel").on("click", function () {
        window.location.href = homeOfertaRepoUrl;
    });

    // Eventos del grid de combos
    configurarEventosGridCombos();
}

// ====== FUNCIONES ESPECÍFICAS PARA GRID DE COMBOS ======
function configurarEventosGridCombos() {
    // Evento para volver a los filtros
    $(document).on("click", "#btnVolverFiltros", function () {
        $("#divDetalle").collapse("hide");
        $("#divFiltro").collapse("show");
    });

    // Evento para expandir/contraer columnas
    $(document).on("click", "#btnExpandirColumnas", function () {
        toggleColumnasExtendidas();
    });

    // Evento para exportar a Excel
    $(document).on("click", "#btnExportarExcel", function () {
        exportarGridCombosAExcel();
    });

    // Inicializar tooltips para celdas truncadas
    inicializarTooltipsCombo();

    // Actualizar timestamp cada minuto
    inicializarActualizacionTimestamp();
}

function toggleColumnasExtendidas() {
    const $tabla = $("#tbGridPrecios");
    const $columnas = $tabla.find("th, td").filter(":nth-child(n+6)"); // Columnas adicionales si las hubiera
    
    if ($columnas.length === 0) {
        console.log("No hay columnas adicionales para expandir/contraer");
        mostrarMensajeAlerta("No hay columnas adicionales para mostrar/ocultar", "info");
        return;
    }
    
    if ($columnas.is(":visible")) {
        $columnas.hide();
        $("#btnExpandirColumnas").html('<i class="bx bx-expand-alt"></i>');
        console.log("Columnas adicionales ocultas");
    } else {
        $columnas.show();
        $("#btnExpandirColumnas").html('<i class="bx bx-collapse-alt"></i>');
        console.log("Columnas adicionales mostradas");
    }
}

function exportarGridCombosAExcel() {
    if (typeof exportarGridAExcel === 'function') {
        exportarGridAExcel('#tbGridPrecios', 'DetalleCombos');
    } else {
        console.warn("La función exportarGridAExcel no está disponible");
        mostrarMensajeAlerta("Función de exportación no disponible", "warning");
    }
}

function inicializarTooltipsCombo() {
    // Esperar a que el DOM esté listo
    setTimeout(function() {
        // Destruir tooltips existentes para evitar duplicados
        $('[data-bs-toggle="tooltip"]').tooltip('dispose');
        
        // Inicializar nuevos tooltips
        $('[title]').tooltip({
            placement: 'top',
            trigger: 'hover',
            container: 'body',
            boundary: 'window'
        });
        
        console.log("Tooltips inicializados para grid de combos");
    }, 100);
}

function inicializarActualizacionTimestamp() {
    const $spanTimestamp = $("#spanUltimaActualizacion");
    
    if ($spanTimestamp.length > 0) {
        // Actualizar inmediatamente
        actualizarTimestamp($spanTimestamp);
        
        // Actualizar cada minuto
        setInterval(function () {
            actualizarTimestamp($spanTimestamp);
        }, 60000);
        
        console.log("Actualización de timestamp iniciada");
    }
}

function actualizarTimestamp($elemento) {
    const ahora = new Date();
    $elemento.text(ahora.toLocaleString('es-AR', {
        year: 'numeric',
        month: '2-digit',
        day: '2-digit',
        hour: '2-digit',
        minute: '2-digit'
    }));
}

// ====== FUNCIONES EXISTENTES MEJORADAS ======
function imprimirReporteOf() {
    let indice = 0;
    let repo = "";
    
    if (estado.rbRepo === 1) {
        indice = estado.report[0];
        repo = "Reporte de Oferta";
    } else if (estado.rbRepo === 2) {
        indice = estado.report[1];
        repo = "Reporte de Combo";
    } else {
        indice = estado.report[0];
        repo = "Reporte de Oferta";
    }

    //let data = { adm_id: admId, lp_id: lpId, canal };
    //cargarReporteEnArre(indice, data, repo);

    data = { modulo: "", parametros: [] };
    invocacionGestorDoc(data);
}

function SeleccionReporte(e, fila) {
    let lp = $("#Tipo").val();

    admId = administracion.split('#')[0];
    lpId = lp.trim() === "" ? admLp_id : lp;
    
    if ($("#rbOferta").is(":checked")) {
        estado.rbRepo = 1;
    } else if ($("#rbCombo").is(":checked")) {
        estado.rbRepo = 2;
    } else {
        estado.rbRepo = 1;
    }
    
    cargarDatosParaReporte(admId, lpId, 1);
}

function mostrarError($contenedor, titulo, mensaje) {
    const html = `
        <div class="alert alert-danger alert-dismissible fade show" role="alert">
            <h5 class="alert-heading">
                <i class="bx bx-error-circle me-2"></i>${titulo}
            </h5>
            <p class="mb-2">${mensaje}</p>
            <button type="button" class="btn btn-outline-danger btn-sm btn-reintentar">
                <i class="bx bx-refresh me-1"></i> Reintentar
            </button>
            <button type="button" class="btn-close" data-bs-dismiss="alert" aria-label="Close"></button>
        </div>
    `;
    
    $contenedor.html(html);
}

function mostrarMensajeAlerta(mensaje, tipo = "info") {
    const iconos = {
        info: "bx-info-circle",
        warning: "bx-error",
        success: "bx-check-circle",
        danger: "bx-x-circle"
    };
    
    const html = `
        <div class="alert alert-${tipo} alert-dismissible fade show" role="alert">
            <i class="bx ${iconos[tipo]} me-2"></i>${mensaje}
            <button type="button" class="btn-close" data-bs-dismiss="alert" aria-label="Close"></button>
        </div>
    `;
    
    // Agregar al contenedor de mensajes si existe, sino crear uno temporal
    let $contenedor = $("#mensajesContainer");
    if ($contenedor.length === 0) {
        $contenedor = $("<div>").attr("id", "mensajesContainer").prependTo(".grid-golden-body");
    }
    
    $contenedor.html(html);
    
    // Auto-ocultar después de 5 segundos
    setTimeout(function () {
        $contenedor.find(".alert").fadeOut(300, function () {
            $(this).remove();
        });
    }, 5000);
}

function cargarDatosParaReporte(admId, lpId, pagina) {
    const queRb = estado.rbRepo;
    
    if (queRb === 1) {
        cargarOfertasActivas(admId, lpId, pagina);
    } else if (queRb === 2) {
        cargarCombosActivos(admId, lpId);
    } else {
        cargarOfertasActivas(admId, lpId, pagina);
    }
}

function cargarOfertasActivas(admId, lpId, pagina) {
    if (typeof presentarOfertasActivasUrl === "undefined") {
        console.error("URL para presentar ofertas activas no definida");
        mostrarError($("#divDetalle"), "Error de configuración",
            "URL para presentar ofertas activas no definida");
        return;
    }

    admId = admId || "0000";
    lpId = lpId || "001";
    pagina = pagina || 1;

    AbrirWaiting("Cargando ofertas activas...");

    var datosPost = {
        adm_id: admId,
        lp_id: lpId,
        pag: pagina
    };

    $.ajax({
        url: presentarOfertasActivasUrl,
        type: "POST",
        data: datosPost,
        success: function (html) {
            CerrarWaiting();
            $("#divFiltro").collapse("hide");
            $("#divDetalle").html(html).collapse("show");
            // actualizar filtros aplicados después de renderizar (fallback si partial reemplaza el DOM)
            try { MostrarFiltrosAplicados(); } catch (e) { console.warn('MostrarFiltrosAplicados no disponible:', e); }
            cargarReporteEnArre(estado.report[0], datosPost, "Reporte de Ofertas Activas");
            configurarVistaDelGrid();
        },
        error: function (xhr, status, error) {
            CerrarWaiting();
            console.error("Error al cargar ofertas activas:", error);
            
            var errorMensaje = "No se pudieron cargar las ofertas activas.";
            try {
                if (xhr.responseJSON && xhr.responseJSON.message) {
                    errorMensaje += " Error: " + xhr.responseJSON.message;
                }
            } catch (e) { }

            mostrarError($("#divDetalle"), "Error al cargar ofertas", errorMensaje);
            ControlaMensajeError("Error al cargar ofertas activas: " + error);
        }
    });
}

function cargarCombosActivos(admId, lpId) {
    if (typeof presentarCombosActivosUrl === "undefined") {
        console.error("URL para presentar combos activos no definida");
        mostrarError($("#divDetalle"), "Error de configuración",
            "URL para presentar combos activos no definida");
        return;
    }

    admId = admId || "0000";
    lpId = lpId || "001";

    AbrirWaiting("Cargando combos activos...");

    var datosPost = {
        adm_id: admId,
        lp_id: lpId,
        cmb_estado: estado.cmbEstado,
        cmb_id: estado.cmbId,
        cmb_carga: estado.cmbFecha
    };

    $.ajax({
        url: presentarCombosActivosUrl,
        type: "POST",
        contentType: "application/json; charset=utf-8",  // ✅ AGREGADO
        dataType: "html",                                 // ✅ AGREGADO
        data: JSON.stringify(datosPost),
        success: function (html) {
            CerrarWaiting();
            $("#divFiltro").collapse("hide");            // ✅ AGREGADO para consistencia
            $("#divDetalle").html(html).collapse("show"); // ✅ MODIFICADO
            cargarReporteEnArre(estado.report[1], datosPost, "Reporte de Combos Activos");
            configurarVistaDelGrid();
            inicializarTooltipsCombo(); // Re-inicializar tooltips después de cargar el grid
        },
        error: function (xhr, status, error) {
            CerrarWaiting();
            console.error("Error al cargar combos activos:", error);
            console.error("Status HTTP:", xhr.status);
            console.error("Response Text:", xhr.responseText);

            var errorMensaje = "No se pudieron cargar los combos activos.";

            // Manejo específico de errores comunes
            switch (xhr.status) {
                case 415:
                    errorMensaje += " Error: Formato de contenido no soportado. Verifique la configuración del servidor.";
                    break;
                case 400:
                    errorMensaje += " Error: Datos inválidos en la solicitud.";
                    break;
                case 401:
                    errorMensaje += " Error: No autorizado. Por favor, inicie sesión nuevamente.";
                    break;
                case 500:
                    errorMensaje += " Error: Error interno del servidor.";
                    break;
                default:
                    if (xhr.responseJSON && xhr.responseJSON.message) {
                        errorMensaje += " Error: " + xhr.responseJSON.message;
                    }
            }

            mostrarError($("#divDetalle"), "Error al cargar combos", errorMensaje);
            ControlaMensajeError("Error al cargar combos activos: " + error);
        }
    });
}

function configurarVistaDelGrid() {
    $("#checkAllOfertas, .check-oferta, #checkAllCombos, .check-combo").parent().css("display", "none");
}

function configurarEventosGrid() {
    $(".pagination .page-link").off("click").on("click", function (e) {
        e.preventDefault();
        var pagina = $(this).data("page") || 1;

        var canalSeleccionado = $("#tbGridCanales tr.selected-row");
        if (canalSeleccionado.length > 0) {
            admId = canalSeleccionado.data("adm-id") || "0000";
            lpId = canalSeleccionado.data("lp-id") || "001";
            cargarDatosParaReporte(admId, lpId, pagina);
        } else {
            cargarDatosParaReporte("0000", "001", pagina);
        }
    });

    actualizarContadorSeleccionadas();
}

function actualizarContadorSeleccionadas() {
    var checkedCount = $(".check-oferta:checked, .check-combo:checked").length;
    var ofertasSeleccionadas = $("#ofertasSeleccionadas, #combosSeleccionados");

    if (ofertasSeleccionadas.length > 0) {
        ofertasSeleccionadas.text(checkedCount);
    }

    var checkAll = $("#checkAllOfertas, #checkAllCombos");
    var totalChecks = $(".check-oferta, .check-combo").length;

    if (checkAll.length > 0 && totalChecks > 0) {
        checkAll.prop("checked", checkedCount === totalChecks);
        checkAll.prop("indeterminate", checkedCount > 0 && checkedCount < totalChecks);
    }

    $("#btnCopiarACanal, #btnEliminarSelec").prop("disabled", checkedCount === 0);
}

function mostrarInformacionCanal(admId, lpId, adminDesc, lpDesc) {
    if (!admId) admId = "0000";
    if (!lpId) lpId = "001";
    if (!adminDesc) adminDesc = admId;
    if (!lpDesc) lpDesc = lpId;
    let queRb = estado.rbRepo;

    var infoCanal = `
        <div class="filter-golden mb-1 mt-1" id="infoCanal">
            <div class="filter-golden-header">
                <div class="card-header-golden py-1">
                    <div class="d-flex align-items-center">
                        <div class="flex-grow-1">
                            <h5 class="mb-0">
                                <i class="bx bx-broadcast me-2"></i>Canal Seleccionado
                            </h5>
                        </div>
                        <div class="flex-grow-1 text-center">
                            <button type="button" class="btn btn-golden btn-sm mt-1 me-1" id="btnImprimir" title="Imprimir">
                                <i class="bx bx-printer me-1"></i> Imprimir
                            </button>
                        </div>
                        <div class="flex-grow-1 d-flex justify-content-end">
                            <div class="input-group input-group-sm">
                                <div class="form-check form-check-inline form-switch mb-2">
                                    <input class="form-check-input" type="radio" name="rbSelect" id="chkOferta" ${queRb == 1 ? 'checked' : ''} />
                                    <label class="form-check-label" for="chkOferta">Oferta</label>
                                </div>
                                <div class="form-check form-check-inline form-switch mb-2">
                                    <input class="form-check-input" type="radio" name="rbSelect" id="chkCombo" ${queRb == 2 ? 'checked' : ''} />
                                    <label class="form-check-label" for="chkCombo">Combos</label>
                                </div>
                            </div>
                        </div>
                    </div>
                </div>
            </div>
            <div class="filter-golden-body py-2">
                <div class="d-flex align-items-center">
                    <div class="me-3">
                        <span class="text-golden-dark">Administración:</span>
                        <span class="badge bg-golden ms-1">${admId}</span>
                    </div>
                    <div class="border-start ps-3">
                        <span class="text-golden-dark">Lista de Precios:</span>
                        <span class="badge bg-golden ms-1">${lpId}</span>
                        <span class="ms-1"><strong>${lpDesc}</strong></span>
                    </div>
                </div>
            </div>
        </div>
    `;

    $("#infoSeleccionContainer").html(infoCanal);
}

function cachearElementosDOM() {
    estado.cacheDom = {
        gridCanales: $("#gridCanales"),
        gridOfertas: $("#gridOfertasActivas"),
        gridCombos: $("#gridCombosActivos"),
        infoSeleccionContainer: $("#infoSeleccionContainer"),
        btnCopiarACanal: $("#btnCopiarACanal"),
        btnEliminarSelec: $("#btnEliminarSelec"),
        modalSeleccionCanal: $("#modalSeleccionCanalDestino"),
        btnConfirmarCopia: $("#btnConfirmarCopiaACanal")
    };

    if (estado.cacheDom.infoSeleccionContainer.length === 0 && $(".grid-golden-body .row").length > 0) {
        var contenedor = $("<div>")
            .attr("id", "infoSeleccionContainer")
            .addClass("mb-3");

        $(".grid-golden-body .row").first().before(contenedor);
        estado.cacheDom.infoSeleccionContainer = $("#infoSeleccionContainer");
    }
}