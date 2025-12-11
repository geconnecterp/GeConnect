var admId = "0000";
var lpId = "001";
var canal = "SANTA LUCIA - MAYORISTA";
var estado = {
    modoSeleccionCanal: "ninguno",
    canalSeleccionado: null,
    cacheDom: {}, // Cache de elementos DOM frecuentes
    canalActual: null,
    canalDestino: null,
    rbActivo: 1
}

$(function () {   
    inicializaEventos();
    cachearElementosDOM();
    cargarCanales();
    inicializarVista();
});

function inicializarVista() {
    // Ya no se selecciona aquí, se hace después de cargar los canales
    // La selección se realiza en cargarCanales() después de recibir el HTML
}

function inicializaEventos() {
    //evento para el boton imprimir
    $(document).on("click", "#btnImprimir", function () {
        //dependiendo del radiobutton activo es la impresion que se ejecutará
        imprimirOfertasActivas();
    });

    // Eventos para canales
    $(document).on("click", ".canal-seleccionable", function (e) {
        manejarSeleccionCanal(e, $(this));
    });
}

function manejarSeleccionCanal(e, fila) {
    admId = fila.data("adm-id");
    lpId = fila.data("lp-id");
    canal = fila.data("canal");
    if ($("#chkOferta").is(":checked")) {
        estado.rbActivo = 1;
    }
    else if ($("#chkCombo").is(":checked")) {        
        estado.rbActivo = 2;
    }
    else {
        estado.rbActivo = 1;
    }
    // Deseleccionar todas las filas
    $("#tbGridCanales tr").removeClass("selected-row");

    // Seleccionar solo la fila actual
    fila.addClass("selected-row");

    // Cargar ofertas activas para este canal
    cargarDatosParaReporte(admId, lpId, 1);

    // Mostrar información del canal seleccionado
    var adminDesc = fila.find("td:eq(1)").text().trim();
    var lpDesc = fila.find("td:eq(2)").text().trim();
    mostrarInformacionCanal(admId, lpId, adminDesc, lpDesc);

    // Mensaje informativo
    ControlaMensajeInfo("Mostrando ofertas del canal: " + canal);
}

function mostrarError($contenedor, titulo, mensaje) {
    $contenedor.html(`
            <div class="alert alert-danger">
                <h5 class="alert-heading"><i class="bx bx-error-circle me-2"></i>${titulo}</h5>
                <p>${mensaje}</p>
                <button class="btn btn-outline-danger btn-sm btn-reintentar">
                    <i class="bx bx-refresh"></i> Reintentar
                </button>
            </div>
        `);
}

function cargarCanales(){
    // Verificar que el contenedor de canales existe
    if ($("#gridCanales").length === 0) {
        console.warn("No se encontró el contenedor para los canales (#gridCanales)");
        return;
    }

    // Verificar que la URL está definida
    if (typeof buscarCanalesUrl === "undefined") {
        console.error("URL de búsqueda de canales no definida");
        mostrarError($("#gridCanales"), "Error de configuración",
            "URL para búsqueda de canales no definida");
        return;
    }

    AbrirWaiting("Cargando canales...");

    // Usar jQuery AJAX
    $.ajax({
        url: buscarCanalesUrl,
        type: "POST",
        data: {},
        success: function (html) {
            CerrarWaiting();
            $("#gridCanales").html(html);

            // Configurar eventos y UI después de cargar canales
            ocultarElementosSeleccionCanales();

            // Seleccionar primer canal después de que el DOM se haya actualizado
            setTimeout(function () {
                var primerCanal = $("#tbGridCanales .canal-seleccionable").first();
                if (primerCanal.length > 0) {
                    primerCanal.trigger('click');
                    console.log("Primer canal seleccionado automáticamente");
                } else {
                    console.warn("No se encontraron canales seleccionables");
                }
            }, 100);
        },
        error: function (xhr, status, error) {
            CerrarWaiting();
            console.error("Error al cargar canales:", error);
            ControlaMensajeError("Error al cargar canales: " + error);

            mostrarError($("#gridCanales"), "Error al cargar canales",
                "No se pudieron cargar los canales disponibles");
        }
    });
}

function ocultarElementosSeleccionCanales() {
    // Ocultar elementos relacionados con selección múltiple
    $("#checkAllCanales, .check-canal").parent().css("display", "none");
    $("#btnLimpiarSeleccion, #canalesSeleccionados, #infoSeleccionCanales").css("display", "none");
}

function seleccionarCanalPredeterminado() {
    var primerCanal = $("#tbGridCanales tbody tr.canal-seleccionable:first");

    if (primerCanal.length > 0) {
        admId = primerCanal.data("adm-id");
        lpId = primerCanal.data("lp-id");
        canal = primerCanal.data("canal");

        // Deseleccionar todas las filas y seleccionar la primera
        $("#tbGridCanales tr").removeClass("selected-row");
        primerCanal.addClass("selected-row");

        // Cargar ofertas activas para el canal predeterminado
        cargarDatosParaReporte(admId, lpId, 1);

        console.log("Canal inicial seleccionado automáticamente:", canal);
    } else {
        console.warn("No se encontraron canales en la grilla");
        cargarDatosParaReporte(); // Cargar con valores por defecto
    }
}

function cargarDatosParaReporte(admId, lpId, pagina) {
    //verifico que rb esta checkeado
    const queRb = estado.rbActivo;
    if (queRb === 1) {
        //se debe invocar las ofertas activas.

        // Verificar que la URL está definida
        if (typeof presentarOfertasActivasUrl === "undefined") {
            console.error("URL para presentar ofertas activas no definida");
            this.mostrarError($("#gridOfertasActivas"), "Error de configuración",
                "URL para presentar ofertas activas no definida");
            return;
        }

        // Valores por defecto
        admId = admId || "0000";
        lpId = lpId || "001";
        pagina = pagina || 1;

        AbrirWaiting("Cargando ofertas activas...");

        var datosPost = {
            admId: admId,
            lp_id: lpId,
            pag: pagina
        };

        // Obtener información del canal para mostrar
        var canalSeleccionado = $("#tbGridCanales tr.selected-row");
        var adminDesc = "", lpDesc = "";

        if (canalSeleccionado.length > 0) {
            try {
                adminDesc = canalSeleccionado.find("td:eq(1)").text().trim();
                lpDesc = canalSeleccionado.find("td:eq(2)").text().trim();
            } catch (e) {
                console.warn("No se pudo obtener descripción del canal");
            }
        }

        // Mostrar información del canal
        mostrarInformacionCanal(admId, lpId, adminDesc, lpDesc);

        // Cargar ofertas activas usando AJAX
        $.ajax({
            url: presentarOfertasActivasUrl,
            type: "POST",
            data: datosPost,
            success: function (html) {
                CerrarWaiting();

                // Actualizar grid de ofertas activas
                $("#gridProductoReporte").html(html);

                configurarVistaDelGrid();
                // Configurar eventos para el grid
                //Momentaneamente estan desactivados los eventos del grid
                //configurarEventosGrid();
            },
            error: function (xhr, status, error) {
                CerrarWaiting();
                console.error("Error al cargar ofertas activas:", error);

                // Obtener mensaje de error detallado si está disponible
                var errorMensaje = "No se pudieron cargar las ofertas activas.";
                try {
                    if (xhr.responseJSON && xhr.responseJSON.message) {
                        errorMensaje += " Error: " + xhr.responseJSON.message;
                    }
                } catch (e) { }

                self.mostrarError($("#gridOfertasActivas"), "Error al cargar ofertas", errorMensaje);
                ControlaMensajeError("Error al cargar ofertas activas: " + error);
            }
        });
    }
    else {
        //se debe invocar los combos
    }
    

}

function configurarVistaDelGrid() {
    // Ocultar elementos relacionados con selección múltiple
    $("#checkAllOfertas, .check-oferta").parent().css("display", "none");
}

function configurarEventosGrid() {
    // Configurar eventos de paginación
    $(".pagination .page-link").off("click").on("click", function (e) {
        e.preventDefault();
        var pagina = $(this).data("page") || 1;

        // Obtener canal seleccionado
        var canalSeleccionado = $("#tbGridCanales tr.selected-row");
        if (canalSeleccionado.length > 0) {
            admId = canalSeleccionado.data("adm-id") || "0000";
            lpId = canalSeleccionado.data("lp-id") || "001";
            cargarDatosParaReporte(admId, lpId, pagina);
        } else {
            cargarDatosParaReporte("0000", "001", pagina);
        }
    });

    // Actualizar contador de seleccionadas
    actualizarContadorSeleccionadas();

}

function actualizarContadorSeleccionadas() {
    var checkedCount = $(".check-oferta:checked").length;
    var ofertasSeleccionadas = $("#ofertasSeleccionadas");

    if (ofertasSeleccionadas.length > 0) {
        ofertasSeleccionadas.text(checkedCount);
    }

    // Actualizar checkbox principal
    var checkAll = $("#checkAllOfertas");
    var totalChecks = $(".check-oferta").length;

    if (checkAll.length > 0 && totalChecks > 0) {
        checkAll.prop("checked", checkedCount === totalChecks);
        checkAll.prop("indeterminate", checkedCount > 0 && checkedCount < totalChecks);
    }

    // Habilitar o deshabilitar botones según si hay ofertas seleccionadas
    $("#btnCopiarACanal").prop("disabled", checkedCount === 0);
    $("#btnEliminarSelec").prop("disabled", checkedCount === 0);
}

function mostrarInformacionCanal(admId, lpId, adminDesc, lpDesc) {
    if (!admId) admId = "0000";
    if (!lpId) lpId = "001";
    if (!adminDesc) adminDesc = admId;
    if (!lpDesc) lpDesc = lpId;
    let queRb = estado.rbActivo;

    // Crear elemento HTML para información del canal
    var infoCanal = `
            <div class="filter-golden mb-1 mt-1" id="infoCanal">
                <div class="filter-golden-header">
                    <div class="card-header-golden py-1">
                        <div class="d-flex align-items-center">
                            <!-- Izquierda: título -->
                            <div class="flex-grow-1">
                                <h5 class="mb-0">
                                    <i class="bx bx-broadcast me-2"></i>Canal Seleccionado
                                </h5>
                            </div>

                            <!-- Centro: botón imprimir -->
                            <div class="flex-grow-1 text-center">
                                <button type="button" class="btn btn-light btn-sm mt-1 me-1" id="btnImprimir" title="Imprimir">
                                    <i class="bx bx-printer"></i> Imprimir
                                </button>
                            </div>

                            <!-- Derecha: switches -->
                            <div class="flex-grow-1 d-flex justify-content-end">
                                <div class="input-group input-group-sm">
                                    <div class="form-check form-check-inline form-switch mb-2">
                                        <input class="form-check-input" type="radio" name="rbSelect" id="chkOferta" ${queRb == 1 ? `checked`:``} } />
                                        <label class="form-check-label" for="chkOferta" id="lbOferta">Oferta</label>
                                    </div>
                                    <div class="form-check form-check-inline form-switch mb-2">
                                        <input class="form-check-input" type="radio" name="rbSelect" id="chkCombo" ${queRb == 2 ? `checked` : ``}/>
                                        <label class="form-check-label" for="chkCombo" id="lbCombo">Combos</label>
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

    // Actualizar la información del canal
    $("#infoSeleccionContainer").html(infoCanal);
}

function cachearElementosDOM() {
    estado.cacheDom = {
        gridCanales: $("#gridCanales"),
        gridOfertas: $("#gridOfertasActivas"),
        infoSeleccionContainer: $("#infoSeleccionContainer"),
        btnCopiarACanal: $("#btnCopiarACanal"),
        btnEliminarSelec: $("#btnEliminarSelec"),
        modalSeleccionCanal: $("#modalSeleccionCanalDestino"),
        btnConfirmarCopia: $("#btnConfirmarCopiaACanal")
    };

    // Si no existe el contenedor de información, crearlo cuando sea necesario
    if (this.estado.cacheDom.infoSeleccionContainer.length === 0 && $(".grid-golden-body .row").length > 0) {
        var contenedor = $("<div>")
            .attr("id", "infoSeleccionContainer")
            .addClass("mb-3");

        $(".grid-golden-body .row").first().before(contenedor);
        this.estado.cacheDom.infoSeleccionContainer = $("#infoSeleccionContainer");
    }
}