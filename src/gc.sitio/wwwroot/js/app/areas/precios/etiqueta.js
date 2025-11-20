let _etiquetaLoading = false;

$(function () {
    InicializaPantallaEtiqueta();
    InicializaEnventosEtiqueta();
});

function cancelarEtiqueta() {
    /*$("#btnAbmAceptar").prop("disabled", true).hide();*/
    $("#btnAbmCancelar").hide();

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

    $("#divDetalle").collapse("hide");
    $("#divFiltro").collapse("show");

}

function InicializaPantallaEtiqueta() {

    /*$("#btnAbmAceptar").hide();*/
    $("#btnAbmCancelar").hide();
    $("#btnImprimir").prop("disabled", true).hide();

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
        window.location.href = homeEtiqueta;
    });

    //Evento de cambio en check de CargaPrevia
    $("#chkCargaPrevia").on("change", function () {
        const isChecked = $(this).is(":checked");
        $("#CargaPrevia").prop("disabled", !isChecked);
    });

    //evento de cambio en check de Modificados
    $("#chkDesdeHasta").on("change", function () {
        const isChecked = $(this).is(":checked");
        $("#Date1, #Date2").prop("disabled", !isChecked);
    });

    //check generico REL01 activando componentes disables
    $("#chkRel011").on("change", function () {
        const isChecked = $(this).is(":checked");

        if (isChecked) {
            $("#Rel011, #Rel011List").prop("disabled", false);
            setTimeout(() => $("#Rel011").trigger("focus"), 50);
        } else {
            $("#Rel011").prop("disabled", true).val("");
            $("#Rel011List").prop("disabled", true).empty();
            $("#Rel011Item").val("");
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

    // ✅ Configurar eventos de eliminación de etiquetas
    configurarEventosEliminacionEtiqueta();
    
    // ✅ NUEVO: Configurar eventos de selección múltiple
    configurarEventosSeleccionMultiple();
}

function determinarIndiceImpresion(tipoEt) {
    // Determinar índice de impresión
    const mapaIndices = { "0": 45, "1": 46, "2": 47 };
    return mapaIndices[tipoEt];
}
function imprimirEtiquetas() {
    // Validar tipo de etiqueta
    const tipoEt = $("#TipoEtiqueta").val();
    const tipoDesc = $("#TipoEtiqueta option:selected").text();
    
    if (!tipoEt) {
        mostrarNotificacion("Debe seleccionar un tipo de etiqueta", "warning");
        return;
    }

    // Determinar índice de impresión
    const indexImp = determinarIndiceImpresion(tipoEt)
    
    if (!indexImp) {
        mostrarNotificacion("Tipo de etiqueta no válido", "error");
        return false;
    }

    // Obtener productos seleccionados
    const productosSeleccionados = [];
    $(".chk-etiqueta-item:checked").each(function () {
        const pId = $(this).val();
        if (pId) {
            productosSeleccionados.push({ p_id: pId });
        }
    });

    // Validar selección
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

    // Preparar datos para impresión
    const adm_id = administracion;
    const productos = JSON.stringify(productosSeleccionados);
    const info = {
        json_p: productos,
        etiqueta: indexImp,
        adm_id: adm_id,
        usu_id: usuarioAuth
    };

    // Cargar reportes
    cargarReporteEnArre(45, {}, "");
    cargarReporteEnArre(46, {}, "");
    cargarReporteEnArre(47, {}, "");
    cargarReporteEnArre(indexImp, info, tipoDesc);

    // Invocar gestor de documentos
    const data = { modulo: "", parametros: [] };
    invocacionGestorDoc(data);

    // ✅ SOLUCIÓN: Configurar el evento DESPUÉS de invocar el gestor
    // y usar un pequeño delay para asegurar que el modal esté completamente inicializado
    setTimeout(() => {
        configurarEventoCierreModal();
    }, 300);
}

/**
 * ✅ NUEVA FUNCIÓN: Configura el evento de cierre del modal de forma robusta
 */
function configurarEventoCierreModal() {
    const $modal = $('#docmgrmodal');
    
    // Verificar que el modal existe
    if ($modal.length === 0) {
        console.warn("⚠️ Modal #docmgrmodal no encontrado en el DOM");
        return;
    }

    // Limpiar eventos previos para evitar duplicados
    $modal.off('hidden.bs.modal.confirmarImpresion');
    $modal.off('hide.bs.modal.confirmarImpresion');

    // ✅ Usar namespace para eventos específicos de esta funcionalidad
    $modal.on('hidden.bs.modal.confirmarImpresion', function () {
        console.log("✅ Modal cerrado - Mostrando confirmación de impresión");
        
        // Remover el evento después de ejecutarlo (one-time)
        $modal.off('hidden.bs.modal.confirmarImpresion');
        
        // Mostrar confirmación con un pequeño delay
        setTimeout(() => {
            mostrarConfirmacionDeImpresion();
        }, 200);
    });

    console.log("✅ Evento de cierre configurado correctamente");
}

/**
 * ✅ NUEVA FUNCIÓN: Muestra el diálogo de confirmación de impresión
 */
function mostrarConfirmacionDeImpresion() {
    AbrirMensaje(
        "Confirmación",
        "¿Se imprimió correctamente el/las etiquetas?",
        function (resp) {
            if (resp === 'SI' || resp === 'Sí') {
                console.log("✔ Usuario confirmó impresión exitosa");
                ConfirmarImpresionOK();
            } else {
                console.log("✖ Usuario reportó problema en impresión");
                // Opcional: Manejar el caso de impresión fallida
                mostrarNotificacion(
                    "Por favor, verifique la impresora e intente nuevamente", 
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

/**
 * ✅ FUNCIÓN OPTIMIZADA: Confirma la impresión exitosa
 */
function ConfirmarImpresionOK() {
    // Recolectar productos seleccionados arrRepoParams
    const tipoEt = $("#TipoEtiqueta").val();
    // Determinar índice de impresión
    const indexImp = determinarIndiceImpresion(tipoEt)

    const productosSeleccionados = arrRepoParams[indexImp - 1].parametros.json_p;
   
    // Preparar request
    const request = {
        json: productosSeleccionados,
        adm: "",
        usu : ""        
    };

    // Mostrar loading
    AbrirWaiting("Confirmando impresión de etiquetas...");

    // Realizar llamada AJAX
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
                    `Impresión de ${productosSeleccionados.length} etiqueta(s) confirmada correctamente`;
                
                mostrarNotificacion(mensaje, "success");

                // Limpiar selección
                limpiarSeleccionEtiquetas();

                // Volver al formulario de búsqueda
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
    _etiquetaLoading = true;

    const $btn = $(btn);
    const originalHtml = $btn.html();
    setBtnLoading($btn, true);

    const tipoVal = $("#chkTipoEtiq").is(":checked") ? $("#TipoEtiqueta").val() : "";
    const sinImp = $("#chkSinImprimir").is(":checked");
    const oferta = $("#chkOferta").is(":checked");

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

    // Extraer y validar arrays de proveedores, familias y rubros
    const proveedores = extraerValoresDeSelect("#Rel011List", "#Rel011Item", "#chkRel011");
    const familias = extraerValoresDeSelect("#Rel03List", null, "#chkRel03");
    const rubros = extraerValoresDeSelect("#Rel02List", null, "#chkRel02");

    const data = {
        Tipo: tipoVal || null,
        Opt1: sinImp,
        Opt2: oferta,
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
   
    $.ajax({
        url: obtenerDetalleEtiquetasUrl,
        type: "POST",
        contentType: "application/json; charset=utf-8",
        dataType: "html",
        data: JSON.stringify(data),
        success: function (html) {
            $("#divDetalle").html(html).collapse("show");
            $("#divFiltro").collapse("hide");

            //se presenta los botones aceptar y cancelar
/*            $("#btnAbmAceptar").show();*/
            $("#btnAbmCancelar").show();
            $("#btnImprimir").prop("disabled",false).show();


            // ✅ Reconfigurar eventos después de cargar el HTML dinámico
            configurarEventosEliminacionEtiqueta();
            configurarEventosSeleccionMultiple();
            actualizarContadorEtiquetas();
        },
        error: function (xhr, status, error) {
            console.error("Error al obtener detalle de etiquetas:", error);
            const mensajeError = '<div class="alert alert-danger py-2 mb-0">' +
                '<i class="bx bx-error-circle me-1"></i>' +
                'No se pudo obtener la información de etiquetas. Intente nuevamente.' +
                '</div>';
            $("#divDetalle").html(mensajeError).collapse("show");
        },
        complete: function () {
            setBtnLoading($btn, false, originalHtml);
            _etiquetaLoading = false;
        }
    });
}

/**
 * ✅ NUEVA FUNCIÓN: Extrae valores de un select de forma optimizada
 */
function extraerValoresDeSelect(selectId, fallbackId, checkId) {
    const valores = [];
    
    if (!$(checkId).is(":checked")) {
        return valores;
    }

    const $opts = $(selectId).find("option");
    if ($opts.length > 0) {
        const visto = {};
        $opts.each(function () {
            let v = $(this).val();
            if (v != null) {
                v = String(v).trim();
                if (v.length > 0 && !visto[v]) {
                    visto[v] = true;
                    valores.push(v);
                }
            }
        });
    } else if (fallbackId) {
        let unicoVal = $(fallbackId).val();
        if (unicoVal != null) {
            unicoVal = String(unicoVal).trim();
            if (unicoVal.length > 0) {
                valores.push(unicoVal);
            }
        }
    }

    return valores;
}

/**
 * ✅ NUEVA FUNCIÓN: Configura los eventos de selección múltiple
 */
function configurarEventosSeleccionMultiple() {
    // Eliminar eventos previos para evitar duplicados
    $(document).off("change.seleccionarTodas", "#chkSeleccionarTodas");
    $(document).off("change.itemSeleccionado", ".chk-etiqueta-item");
    $(document).off("click.eliminarSeleccionadas", "#btnEliminarSeleccionadas");
    $(document).off("click.limpiarSeleccion", "#btnLimpiarSeleccion");
    
    // Evento: Checkbox "Seleccionar Todas"
    $(document).on("change.seleccionarTodas", "#chkSeleccionarTodas", function () {
        const isChecked = $(this).is(":checked");
        $(".chk-etiqueta-item").prop("checked", isChecked);
        actualizarEstadoSeleccion();
    });
    
    // Evento: Checkboxes individuales
    $(document).on("change.itemSeleccionado", ".chk-etiqueta-item", function () {
        actualizarCheckboxPrincipal();
        actualizarEstadoSeleccion();
    });
    
    // Evento: Botón "Eliminar Seleccionadas"
    $(document).on("click.eliminarSeleccionadas", "#btnEliminarSeleccionadas", function () {
        eliminarEtiquetasSeleccionadas();
    });
    
    // Evento: Botón "Limpiar Selección"
    $(document).on("click.limpiarSeleccion", "#btnLimpiarSeleccion", function () {
        limpiarSeleccionEtiquetas();
    });
}

/**
 * ✅ NUEVA FUNCIÓN: Actualiza el estado del checkbox principal
 */
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

/**
 * ✅ NUEVA FUNCIÓN: Actualiza el estado visual de la selección
 */
function actualizarEstadoSeleccion() {
    const $seleccionadas = $(".chk-etiqueta-item:checked");
    const cantidad = $seleccionadas.length;
    
    // Actualizar contador en botón
    $("#cantidadSeleccionadas").text(cantidad);
    
    // Mostrar/ocultar botones de acciones múltiples
    if (cantidad > 0) {
        $("#divAccionesMultiples").fadeIn(200);
        
        // Actualizar texto informativo
        const textoSeleccion = cantidad === 1 
            ? "1 etiqueta seleccionada" 
            : `${cantidad} etiquetas seleccionadas`;
        $("#txtSeleccionadas").text(textoSeleccion);
    } else {
        $("#divAccionesMultiples").fadeOut(200);
        $("#txtSeleccionadas").text("Ninguna seleccionada");
    }
}

/**
 * ✅ NUEVA FUNCIÓN: Obtiene las etiquetas seleccionadas
 * @returns {Array<Object>} Array de objetos con id y descripción
 */
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

/**
 * ✅ NUEVA FUNCIÓN: Elimina múltiples etiquetas seleccionadas
 */
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

/**
 * ✅ NUEVA FUNCIÓN: Ejecuta la eliminación de múltiples etiquetas
 * @param {Array<Object>} etiquetas - Array de etiquetas a eliminar
 */
function ejecutarEliminacionMultiple(etiquetas) {
    const $btnEliminar = $("#btnEliminarSeleccionadas");
    const originalHtml = $btnEliminar.html();
    
    $btnEliminar.prop("disabled", true)
        .html('<span class="spinner-border spinner-border-sm me-1"></span>Eliminando...');
    
    let eliminadas = 0;
    
    // Eliminar cada fila con animación escalonada
    etiquetas.forEach((etiqueta, index) => {
        const $fila = $(`tr[data-p-id="${etiqueta.id}"]`);
        
        setTimeout(() => {
            $fila.fadeOut(300, function () {
                $(this).remove();
                eliminadas++;
                
                // Cuando se eliminaron todas, actualizar la vista
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
        }, index * 100); // Desfase de 100ms entre cada eliminación
    });
}

/**
 * ✅ NUEVA FUNCIÓN: Limpia la selección de etiquetas
 */
function limpiarSeleccionEtiquetas() {
    $(".chk-etiqueta-item").prop("checked", false);
    $("#chkSeleccionarTodas").prop("checked", false).prop("indeterminate", false);
    actualizarEstadoSeleccion();
}

/**
 * ✅ FUNCIÓN: Configura los eventos de eliminación de etiquetas del grid
 */
function configurarEventosEliminacionEtiqueta() {
    $(document).off("click.eliminarEtiqueta", ".btn-eliminar-etiqueta");
    
    $(document).on("click.eliminarEtiqueta", ".btn-eliminar-etiqueta", function (e) {
        e.preventDefault();
        e.stopPropagation();
        
        const $btn = $(this);
        const $fila = $btn.closest("tr");
        const etiquetaId = $btn.data("p-id");
        const descripcion = $fila.find("td").eq(2).text().trim(); // ✅ Ajustado al índice correcto (columna 3)
        
        if (!etiquetaId) {
            console.error("No se pudo obtener el ID de la etiqueta");
            mostrarNotificacion("Error al identificar la etiqueta", "error");
            return;
        }
        
        confirmarEliminacionEtiqueta(etiquetaId, descripcion, $fila, $btn);
    });
}

/**
 * ✅ FUNCIÓN: Muestra diálogo de confirmación antes de eliminar
 */
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

/**
 * ✅ FUNCIÓN: Ejecuta la eliminación de la etiqueta del grid
 */
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

/**
 * ✅ FUNCIÓN: Actualiza el contador total de etiquetas en el footer
 */
function actualizarContadorEtiquetas() {
    const $tbody = $("#tbGridEtiquetaDetalle tbody");
    const $filas = $tbody.find("tr:not(.empty-message)");
    const totalRegistros = $filas.length;

    $("#totalEtiquetasMostradas").text(totalRegistros);
}

/**
 * ✅ FUNCIÓN: Renumera las filas después de eliminar una etiqueta
 */
function renumerarFilasEtiquetas() {
    const $tbody = $("#tbGridEtiquetaDetalle tbody");
    const $filas = $tbody.find("tr:not(.empty-message)");
    
    $filas.each(function (index) {
        const $fila = $(this);
        
        // Actualizar clase alt para filas alternas
        if ((index + 1) % 2 === 0) {
            $fila.removeClass("alt");
        } else {
            $fila.addClass("alt");
        }
    });
}

/**
 * ✅ FUNCIÓN: Verifica si el grid quedó vacío y muestra mensaje
 */
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
        $("#divAccionesMultiples").hide();
        $("#txtSeleccionadas").text("Ninguna seleccionada");
    }
}

/**
 * ✅ FUNCIÓN: Muestra notificaciones toast al usuario
 */
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

function verificarYDesactivarControles(mostrarLog = true) {
    if ($("#Rel011List").find("option").length > 0) {
        if (mostrarLog) {
            console.log("Se encontraron opciones en Rel011List, verificando controles...");
        }

        const opciones = $("#Rel011List option");
        const cantidad = opciones.length;
        
        if (cantidad === 1) {
            AbrirWaiting("Buscando familias de productos...");
            
            const primerValor = opciones.first().val();
            $("#Rel011List").val([primerValor]);
            
            const proveedorId = $("#Rel011Item").val() || primerValor;
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
    } else if (mostrarLog && $("#Rel011").val()) {
        console.log("No hay opciones en Rel011List todavía, pero hay texto en Rel011");
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

