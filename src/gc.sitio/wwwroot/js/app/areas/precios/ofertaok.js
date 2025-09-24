/**
 * Script para gestión de ofertas activas
 * Implementa funcionalidad para selección y gestión de canales
 */

var OfertaOk = {
    // Estado de la aplicación
    estado: {
        modoSeleccionCanal: "ninguno",
        canalSeleccionado: null,
        cacheDom: {}, // Cache de elementos DOM frecuentes
        canalActual: null,
        canalDestino: null
    },
    
    // Inicialización del módulo
    init: function() {
        console.log("🚀 Iniciando ofertaok.js");
        
        // Verificar URLs necesarias
        this.verificarConfiguracion();
        
        // Cachear elementos DOM de uso frecuente
        this.cachearElementosDOM();
        
        // Inicializar eventos básicos
        this.inicializarEventos();
        
        // Cargar canales
        this.cargarCanales();
        
        // Agregar estilos para canales
        this.agregarEstilosCanales();
        
        console.log("✅ ofertaok.js inicializado");
    },
    
    // Verificar configuración necesaria
    verificarConfiguracion: function() {
        // Verificar que las URLs necesarias estén definidas
        var urlsRequeridas = [
            { nombre: "buscarCanalesUrl", mensaje: "para buscar canales" },
            { nombre: "presentarOfertasActivasUrl", mensaje: "para presentar ofertas activas" },
            { nombre: "copiarACanalUrl", mensaje: "para copiar a canal" },
            { nombre: "eliminarOfertasActivasUrl", mensaje: "para eliminar ofertas activas" }
        ];
        
        urlsRequeridas.forEach(function(url) {
            if (typeof window[url.nombre] === "undefined") {
                console.error(`Error: URL ${url.mensaje} no definida (${url.nombre})`);
            }
        });
    },
    
    // Agregar estilos necesarios para canales
    agregarEstilosCanales: function() {
        if ($("#canal-row-style").length === 0) {
            $("<style>")
                .attr("id", "canal-row-style")
                .html(`
                    .selected-row {
                        background-color: #e9f5ff !important;
                        border-left: 3px solid #0d6efd;
                    }
                    .canal-destino-selected {
                        background-color: #d1e7dd !important;
                        border-left: 3px solid #198754;
                    }
                    .canal-destino-seleccionable {
                        cursor: pointer;
                    }
                    .canal-destino-seleccionable:hover {
                        background-color: #f8f9fa;
                    }
                `)
                .appendTo("head");
        }
    },
    
    // Ocultar elementos de selección múltiple de canales
    ocultarElementosSeleccionCanales: function() {
        // Ocultar elementos relacionados con selección múltiple
        $("#checkAllCanales, .check-canal").parent().css("display", "none");
        $("#btnLimpiarSeleccion, #canalesSeleccionados, #infoSeleccionCanales").css("display", "none");
    },
    
    // Cachear referencias a elementos DOM frecuentes para mejorar rendimiento
    cachearElementosDOM: function() {
        this.estado.cacheDom = {
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
    },
    
    // Inicializar eventos principales usando jQuery
    inicializarEventos: function() {
        var self = this;
        
        // Eventos para canales
        $(document).on("click", ".canal-seleccionable", function(e) {
            self.manejarSeleccionCanal(e, $(this));
        });
        
        // Eventos para el grid de ofertas
        $(document).on("change", "#checkAllOfertas", function() {
            self.toggleSeleccionarTodas();
        });
        
        $(document).on("change", ".check-oferta", function() {
            self.actualizarContadorSeleccionadas();
        });
        
        $(document).on("click", "#tbGridOfertasActivas tbody tr", function(e) {
            self.manejarClickFila(e, $(this));
        });
        
        // Botón para mostrar modal de selección de canal
        $(document).on("click", "#btnCopiarACanal", function() {
            self.mostrarSeleccionCanalDestino();
        });
        
        // Delegación de eventos para canales destino en el modal
        $(document).on("click", ".canal-destino-seleccionable", function() {
            self.seleccionarCanalDestino($(this));
        });
        
        // Confirmar copia a canal seleccionado
        $(document).on("click", "#btnConfirmarCopiaACanal", function() {
            self.confirmarCopiaCanalDestino();
        });
        
        // Botón Eliminar seleccionados
        $(document).on("click", "#btnEliminarSelec", function() {
            self.eliminarOfertasSeleccionadas();
        });
        
        // Evento cuando se cierra el modal para limpiar selección
        $(document).on("hidden.bs.modal", "#modalSeleccionCanalDestino", function() {
            self.limpiarSeleccionCanalDestino();
        });
        
        // Evento cuando se muestra el modal
        $(document).on("shown.bs.modal", "#modalSeleccionCanalDestino", function() {
            // Asegurar que el botón está deshabilitado al inicio
            $("#btnConfirmarCopiaACanal").prop("disabled", true);
        });
        
        // Botones de reintento en caso de error
        $(document).on("click", ".btn-reintentar", function() {
            var admId = $(this).data("adm-id") || "0000";
            var lpId = $(this).data("lp-id") || "001";
            var pagina = $(this).data("pagina") || 1;
            self.cargarOfertasActivas(admId, lpId, pagina);
        });
        
        $(document).on("click", "#btnReintentarCanalesModal", function() {
            self.cargarCanalesParaModal();
        });
        
        // Botón para cambiar selección en el modal
        $(document).on("click", "#btnCambiarSeleccion", function() {
            $("#destinoSeleccionInfo").addClass("d-none");
            self.estado.canalDestino = null;
            $("#btnConfirmarCopiaACanal").prop("disabled", true);
        });
    },
    
    // Cargar canales optimizada con jQuery
    cargarCanales: function() {
        var self = this;
        
        // Verificar que el contenedor de canales existe
        if ($("#gridCanales").length === 0) {
            console.warn("No se encontró el contenedor para los canales (#gridCanales)");
            return;
        }
        
        // Verificar que la URL está definida
        if (typeof buscarCanalesUrl === "undefined") {
            console.error("URL de búsqueda de canales no definida");
            this.mostrarError($("#gridCanales"), "Error de configuración", 
                "URL para búsqueda de canales no definida");
            return;
        }
        
        AbrirWaiting("Cargando canales...");
        
        // Usar jQuery AJAX
        $.ajax({
            url: buscarCanalesUrl,
            type: "POST",
            data: {},
            success: function(html) {
                CerrarWaiting();
                $("#gridCanales").html(html);
                
                // Configurar eventos y UI después de cargar canales
                self.ocultarElementosSeleccionCanales();
                
                // Seleccionar canal por defecto
                setTimeout(function() {
                    self.seleccionarCanalPredeterminado();
                }, 100);
            },
            error: function(xhr, status, error) {
                CerrarWaiting();
                console.error("Error al cargar canales:", error);
                ControlaMensajeError("Error al cargar canales: " + error);
                
                self.mostrarError($("#gridCanales"), "Error al cargar canales", 
                    "No se pudieron cargar los canales disponibles");
            }
        });
    },
    
    // Mostrar modal de selección de canal destino
    mostrarSeleccionCanalDestino: function() {
        // Obtener ofertas seleccionadas
        var ofertasSeleccionadas = this.obtenerOfertasSeleccionadas();
        if (ofertasSeleccionadas.length === 0) {
            ControlaMensajeInfo("Debe seleccionar al menos una oferta para copiar");
            return;
        }
        
        // Obtener canal actual seleccionado
        var canalActual = $("#tbGridCanales tr.selected-row");
        if (canalActual.length === 0) {
            ControlaMensajeInfo("Debe seleccionar un canal origen");
            return;
        }
        
        // Guardar información del canal actual en el estado
        this.estado.canalActual = {
            admId: canalActual.data("adm-id") || "0000",
            lpId: canalActual.data("lp-id") || "001",
            descripcion: canalActual.data("canal") || "Canal actual"
        };
        
        // Cargar canales para el modal
        this.cargarCanalesParaModal();
        
        // Mostrar modal usando jQuery
        $("#modalSeleccionCanalDestino").modal("show");
    },
    
    // Limpiar selección de canal destino al cerrar modal
    limpiarSeleccionCanalDestino: function() {
        this.estado.canalDestino = null;
        $("#destinoSeleccionInfo").addClass("d-none");
        $("#btnConfirmarCopiaACanal").prop("disabled", true);
        $("#canalDestinoSeleccionado").text("");
    },
    
    // Cargar canales para el modal
    cargarCanalesParaModal: function() {
        var self = this;
        
        $("#gridCanalesDestino").html('<p class="text-center py-3"><i class="bx bx-loader-alt bx-spin"></i> Cargando canales...</p>');
        $("#btnConfirmarCopiaACanal").prop("disabled", true);
        
        // Usar jQuery AJAX para cargar los canales
        $.ajax({
            url: buscarCanalesUrl,
            type: "POST",
            data: {},
            success: function(html) {
                // Procesar el HTML para modificar identificadores y clases
                var $html = $(html);
                
                // Cambiar el ID de la tabla
                $html.find("#tbGridCanales").attr("id", "tbGridCanalesDestino");
                
                // Cambiar el ID y comportamiento del checkbox "Seleccionar todos"
                $html.find("#checkAllCanales")
                    .attr("id", "checkAllCanalesDestino")
                    .attr("title", "Seleccionar todos los canales destino");
                
                // Modificar la clase de los checkboxes individuales y las filas
                $html.find(".check-canal")
                    .removeClass("check-canal")
                    .addClass("check-canal-destino");
                
                $html.find(".canal-seleccionable")
                    .removeClass("canal-seleccionable")
                    .addClass("canal-destino-seleccionable");
                
                // Resaltar y deshabilitar el canal Actual
                var canalActualRow = $html.find('tr[data-adm-id="' + self.estado.canalActual.admId + '"][data-lp-id="' + self.estado.canalActual.lpId + '"]');
                
                if (canalActualRow.length > 0) {
                    canalActualRow
                        .addClass("table-secondary")
                        .removeClass("canal-destino-seleccionable")
                        .attr("title", "Este es el canal actual (no puede seleccionarlo)")
                        .css("opacity", "0.7")
                        .find("input[type='checkbox']").prop("disabled", true).css("opacity", "0.5");
                    
                    canalActualRow.find("td").first().prepend('<i class="bx bx-check-circle text-muted me-1"></i> ');
                }
                
                // Cambiar el contador de seleccionados
                $html.find("#canalesSeleccionados").attr("id", "canalesDestinoSeleccionados");
                
                // Insertar el HTML modificado en el contenedor
                $("#gridCanalesDestino").html($html);
                
                // Inicializar eventos para los elementos del grid destino
                self.inicializarEventosGridDestino();
                
                // Mostrar un mensaje si no hay canales disponibles aparte del actual
                if ($html.find(".canal-destino-seleccionable").length === 0) {
                    $("#gridCanalesDestino").append(`
                        <div class="alert alert-warning mt-3">
                            <i class="bx bx-error-circle me-2"></i>
                            No hay canales adicionales disponibles para copiar las ofertas
                        </div>
                    `);
                }
            },
            error: function(xhr, status, error) {
                $("#gridCanalesDestino").html(`
                    <div class="alert alert-danger">
                        <i class="bx bx-error-circle me-2"></i>
                        Error al cargar canales: ${error || "Error desconocido"}
                        <button class="btn btn-outline-danger btn-sm ms-3" id="btnReintentarCanalesModal">
                            <i class="bx bx-refresh"></i> Reintentar
                        </button>
                    </div>
                `);
            }
        });
    },
    
    // Actualizar contador de canales destino seleccionados
    actualizarContadorCanalesDestino: function() {
        var contadorElement = $("#canalesDestinoSeleccionados");
        var cantidadSeleccionada = $(".check-canal-destino:checked").length;
        
        if (contadorElement.length > 0) {
            contadorElement.text(cantidadSeleccionada);
        }
        
        // Actualizar estado del checkbox "Seleccionar todos"
        var checkAll = $("#checkAllCanalesDestino");
        var totalCheckboxes = $(".check-canal-destino:not(:disabled)").length;
        
        if (checkAll.length > 0 && totalCheckboxes > 0) {
            if (cantidadSeleccionada === totalCheckboxes && cantidadSeleccionada > 0) {
                checkAll.prop("checked", true);
                checkAll.prop("indeterminate", false);
            } else if (cantidadSeleccionada === 0) {
                checkAll.prop("checked", false);
                checkAll.prop("indeterminate", false);
            } else {
                checkAll.prop("indeterminate", true);
            }
        }
    },
    
    // Seleccionar canal destino en el modal
    seleccionarCanalDestino: function($fila) {
        // Recopilamos la información del canal
        var admId = $fila.data("adm-id");
        var lpId = $fila.data("lp-id");
        var canal = $fila.data("canal") || "Canal seleccionado";
        var admNombre = $fila.data("adm-nombre");
        var lpDesc = $fila.data("lp-desc");
        
        // Guardar información del canal destino en el estado
        this.estado.canalDestino = {
            admId: admId,
            lpId: lpId,
            descripcion: canal
        };
        
        // Mostrar información del canal seleccionado
        $("#canalDestinoSeleccionado").text(canal);
        $("#destinoSeleccionInfo").removeClass("d-none");
        
        // Habilitar el botón de confirmar
        $("#btnConfirmarCopiaACanal").prop("disabled", false);
        
        // Actualizar la UI para mostrar la selección
        $(".canal-destino-seleccionable").removeClass("canal-destino-selected");
        $fila.addClass("canal-destino-selected");
    },
    
    // Confirmar copia a canal destino
    confirmarCopiaCanalDestino: function() {
        var self = this;
        
        // Verificar que se haya seleccionado un canal destino
        if (!this.estado.canalDestino) {
            ControlaMensajeInfo("Debe seleccionar un canal destino");
            return;
        }
        
        // Verificar que el canal destino sea diferente al origen
        if (this.estado.canalActual.admId === this.estado.canalDestino.admId && 
            this.estado.canalActual.lpId === this.estado.canalDestino.lpId) {
            ControlaMensajeInfo("El canal destino debe ser diferente al canal origen");
            return;
        }
        
        // Obtener ofertas seleccionadas
        var ofertasSeleccionadas = this.obtenerOfertasSeleccionadas();
        if (ofertasSeleccionadas.length === 0) {
            ControlaMensajeInfo("Debe seleccionar al menos una oferta para copiar");
            return;
        }
        
        // Extraer solo los IDs de las ofertas
        var ids = ofertasSeleccionadas.map(function(o) { 
            return o.pId;
        });
        
        // Cerrar el modal
        $("#modalSeleccionCanalDestino").modal("hide");
        
        // Mostrar confirmación final
        AbrirMensaje(
            "CONFIRMAR COPIAR A CANAL",
            `¿Está seguro que desea copiar ${ofertasSeleccionadas.length} oferta(s) seleccionada(s) al canal ${this.estado.canalDestino.descripcion}?`,
            function(resp) {
                if (resp === "SI") {
                    AbrirWaiting("Copiando ofertas al canal destino...");
                    
                    // Realizar la llamada AJAX para copiar
                    $.ajax({
                        url: copiarACanalUrl,
                        type: "POST",
                        data: {
                            ids: ids,
                            admId: self.estado.canalDestino.admId,
                            lp_id: self.estado.canalDestino.lpId
                        },
                        success: function(response) {
                            CerrarWaiting();
                            
                            if (response.error) {
                                AbrirMensaje(
                                    "ERROR",
                                    response.msg || "Error al copiar ofertas",
                                    function() {
                                        $("#msjModal").modal("hide");
                                        return true;
                                    },
                                    false,
                                    ["Aceptar"],
                                    "error!",
                                    null
                                );
                                return;
                            }
                            
                            if (response.warn) {
                                AbrirMensaje(
                                    "ADVERTENCIA",
                                    response.msg || "Advertencia al copiar ofertas",
                                    function() {
                                        $("#msjModal").modal("hide");
                                        return true;
                                    },
                                    false,
                                    ["Aceptar"],
                                    "warn!",
                                    null
                                );
                                return;
                            }
                            
                            // Mensaje de éxito
                            AbrirMensaje(
                                "OPERACIÓN EXITOSA",
                                response.msg || "Ofertas copiadas correctamente al canal destino",
                                function() {
                                    // Recargar el grid con los mismos parámetros del canal origen
                                    self.cargarOfertasActivas(self.estado.canalActual.admId, self.estado.canalActual.lpId, 1);
                                    $("#msjModal").modal("hide");
                                    return true;
                                },
                                false,
                                ["Aceptar"],
                                "success!",
                                null
                            );
                        },
                        error: function(xhr, status, error) {
                            CerrarWaiting();
                            console.error("Error al copiar ofertas:", error);
                            
                            AbrirMensaje(
                                "ERROR DE COMUNICACIÓN",
                                "Error al copiar ofertas: " + (error || "Error desconocido"),
                                function() {
                                    $("#msjModal").modal("hide");
                                    return true;
                                },
                                false,
                                ["Aceptar"],
                                "error!",
                                null
                            );
                        }
                    });
                }
                
                $("#msjModal").modal("hide");
                return true;
            },
            true,
            ["Copiar", "Cancelar"],
            "info!",
            null
        );
    },
    
    // Obtener ofertas seleccionadas
    obtenerOfertasSeleccionadas: function() {
        var ofertas = [];
        
        $(".check-oferta:checked").each(function() {
            var checkbox = $(this);
            ofertas.push({
                pId: checkbox.data("p-id"),
                admId: checkbox.data("adm-id"),
                lpId: checkbox.data("lp-id")
            });
        });
        
        return ofertas;
    },
    
    // Actualizar contador de ofertas seleccionadas
    actualizarContadorSeleccionadas: function() {
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
    },
    
    // Manejar click en fila para selección
    manejarClickFila: function(e, fila) {
        // No procesar si se hizo clic en un elemento interactivo
        if ($(e.target).is("button, input, i, a, .btn")) {
            return;
        }
        
        var checkbox = fila.find(".check-oferta");
        if (checkbox.length > 0) {
            checkbox.prop("checked", !checkbox.prop("checked"));
            this.actualizarContadorSeleccionadas();
        }
    },
    
    // Manejar checkbox "seleccionar todas"
    toggleSeleccionarTodas: function() {
        var isChecked = $("#checkAllOfertas").prop("checked") || false;
        $(".check-oferta").prop("checked", isChecked);
        this.actualizarContadorSeleccionadas();
    },
    
    // Manejar selección de canal
    manejarSeleccionCanal: function(e, fila) {
        var admId = fila.data("adm-id");
        var lpId = fila.data("lp-id");
        var canal = fila.data("canal");
        
        // Deseleccionar todas las filas
        $("#tbGridCanales tr").removeClass("selected-row");
        
        // Seleccionar solo la fila actual
        fila.addClass("selected-row");
        
        // Cargar ofertas activas para este canal
        this.cargarOfertasActivas(admId, lpId, 1);
        
        // Mostrar información del canal seleccionado
        var adminDesc = fila.find("td:eq(1)").text().trim();
        var lpDesc = fila.find("td:eq(2)").text().trim();
        this.mostrarInformacionCanal(admId, lpId, adminDesc, lpDesc);
        
        // Mensaje informativo
        ControlaMensajeInfo("Mostrando ofertas del canal: " + canal);
    },
    
    // Seleccionar canal predeterminado
    seleccionarCanalPredeterminado: function() {
        var primerCanal = $("#tbGridCanales tbody tr.canal-seleccionable:first");
        
        if (primerCanal.length > 0) {
            var admId = primerCanal.data("adm-id");
            var lpId = primerCanal.data("lp-id");
            var canal = primerCanal.data("canal");
            
            // Deseleccionar todas las filas y seleccionar la primera
            $("#tbGridCanales tr").removeClass("selected-row");
            primerCanal.addClass("selected-row");
            
            // Cargar ofertas activas para el canal predeterminado
            this.cargarOfertasActivas(admId, lpId, 1);
            
            console.log("Canal inicial seleccionado automáticamente:", canal);
        } else {
            console.warn("No se encontraron canales en la grilla");
            this.cargarOfertasActivas(); // Cargar con valores por defecto
        }
    },
    
    // Mostrar información del canal seleccionado
    mostrarInformacionCanal: function(admId, lpId, adminDesc, lpDesc) {
        if (!admId) admId = "0000";
        if (!lpId) lpId = "001";
        if (!adminDesc) adminDesc = admId;
        if (!lpDesc) lpDesc = lpId;
        
        // Crear elemento HTML para información del canal
        var infoCanal = `
            <div class="filter-golden mb-1 mt-1" id="infoCanal">
                <div class="filter-golden-header">
                    <h5><i class="bx bx-broadcast me-2"></i>Canal Seleccionado</h5>
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
    },
    
    // Cargar ofertas activas del canal seleccionado
    cargarOfertasActivas: function(admId, lpId, pagina) {
        var self = this;
        
        // Verificar que el contenedor de ofertas existe
        if ($("#gridOfertasActivas").length === 0) {
            console.warn("No se encontró el contenedor para las ofertas activas (#gridOfertasActivas)");
            return;
        }
        
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
        this.mostrarInformacionCanal(admId, lpId, adminDesc, lpDesc);
        
        // Cargar ofertas activas usando AJAX
        $.ajax({
            url: presentarOfertasActivasUrl,
            type: "POST",
            data: datosPost,
            success: function(html) {
                CerrarWaiting();
                
                // Actualizar grid de ofertas activas
                $("#gridOfertasActivas").html(html);
                
                // Configurar eventos para el grid
                self.configurarEventosGrid();
            },
            error: function(xhr, status, error) {
                CerrarWaiting();
                console.error("Error al cargar ofertas activas:", error);
                
                // Obtener mensaje de error detallado si está disponible
                var errorMensaje = "No se pudieron cargar las ofertas activas.";
                try {
                    if (xhr.responseJSON && xhr.responseJSON.message) {
                        errorMensaje += " Error: " + xhr.responseJSON.message;
                    }
                } catch (e) {}
                
                self.mostrarError($("#gridOfertasActivas"), "Error al cargar ofertas", errorMensaje);
                ControlaMensajeError("Error al cargar ofertas activas: " + error);
            }
        });
    },
    
    // Configurar eventos para el grid de ofertas
    configurarEventosGrid: function() {
        var self = this;
        
        // Configurar eventos de paginación
        $(".pagination .page-link").off("click").on("click", function(e) {
            e.preventDefault();
            var pagina = $(this).data("page") || 1;
            
            // Obtener canal seleccionado
            var canalSeleccionado = $("#tbGridCanales tr.selected-row");
            if (canalSeleccionado.length > 0) {
                var admId = canalSeleccionado.data("adm-id") || "0000";
                var lpId = canalSeleccionado.data("lp-id") || "001";
                self.cargarOfertasActivas(admId, lpId, pagina);
            } else {
                self.cargarOfertasActivas("0000", "001", pagina);
            }
        });
        
        // Actualizar contador de seleccionadas
        this.actualizarContadorSeleccionadas();
    },
    
    // Eliminar ofertas seleccionadas
    eliminarOfertasSeleccionadas: function() {
        var self = this;
        
        // Verificar que la URL está definida
        if (typeof eliminarOfertasActivasUrl === "undefined") {
            ControlaMensajeError("URL para eliminar ofertas no definida");
            return;
        }
        
        // Obtener ofertas seleccionadas
        var ofertasSeleccionadas = this.obtenerOfertasSeleccionadas();
        
        if (ofertasSeleccionadas.length === 0) {
            ControlaMensajeInfo("Debe seleccionar al menos una oferta para eliminar");
            return;
        }
        
        // Obtener canal seleccionado
        var canalSeleccionado = $("#tbGridCanales tr.selected-row");
        if (canalSeleccionado.length === 0) {
            ControlaMensajeInfo("Debe seleccionar un canal");
            return;
        }
        
        var admId = canalSeleccionado.data("adm-id") || "0000";
        var lpId = canalSeleccionado.data("lp-id") || "001";
        
        // Extraer solo los IDs de las ofertas
        var ids = ofertasSeleccionadas.map(function(o) { 
            return o.pId;
        });
        
        // Mostrar confirmación
        AbrirMensaje(
            "CONFIRMAR ELIMINACIÓN",
            `¿Está seguro que desea eliminar ${ofertasSeleccionadas.length} oferta(s) seleccionada(s)?`,
            function(resp) {
                if (resp === "SI") {
                    AbrirWaiting("Eliminando ofertas...");
                    
                    // Realizar la llamada AJAX para eliminar
                    $.ajax({
                        url: eliminarOfertasActivasUrl,
                        type: "POST",
                        data: {
                            ids: ids,
                            admId: admId,
                            lp_id: lpId
                        },
                        success: function(response) {
                            CerrarWaiting();
                            
                            if (response.error) {
                                AbrirMensaje(
                                    "ERROR",
                                    response.msg || "Error al eliminar ofertas",
                                    function() {
                                        $("#msjModal").modal("hide");
                                        return true;
                                    },
                                    false,
                                    ["Aceptar"],
                                    "error!",
                                    null
                                );
                                return;
                            }
                            
                            if (response.warn) {
                                AbrirMensaje(
                                    "ADVERTENCIA",
                                    response.msg || "Advertencia al eliminar ofertas",
                                    function() {
                                        $("#msjModal").modal("hide");
                                        return true;
                                    },
                                    false,
                                    ["Aceptar"],
                                    "warn!",
                                    null
                                );
                                return;
                            }
                            
                            // Mensaje de éxito y recarga del grid
                            AbrirMensaje(
                                "OPERACIÓN EXITOSA",
                                response.msg || "Ofertas eliminadas correctamente",
                                function() {
                                    // Recargar el grid con los mismos parámetros
                                    self.cargarOfertasActivas(admId, lpId, 1);
                                    $("#msjModal").modal("hide");
                                    return true;
                                },
                                false,
                                ["Aceptar"],
                                "success!",
                                null
                            );
                        },
                        error: function(xhr, status, error) {
                            CerrarWaiting();
                            console.error("Error al eliminar ofertas:", error);
                            
                            AbrirMensaje(
                                "ERROR DE COMUNICACIÓN",
                                "Error al eliminar ofertas: " + (error || "Error desconocido"),
                                function() {
                                    $("#msjModal").modal("hide");
                                    return true;
                                },
                                false,
                                ["Aceptar"],
                                "error!",
                                null
                            );
                        }
                    });
                }
                
                $("#msjModal").modal("hide");
                return true;
            },
            true,
            ["Eliminar", "Cancelar"],
            "warn!",
            null
        );
    },
    
    // Mostrar mensaje de error en un contenedor
    mostrarError: function($contenedor, titulo, mensaje) {
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
};

// Inicialización cuando el DOM esté cargado
$(function() {
    OfertaOk.init();
});