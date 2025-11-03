var modoNuevoPresup = false;
var modoModificacionPresup = false;

// ✅ AGREGAR: Variable global para controlar edición
var campoEnEdicionPresup = null;

// ✅ NUEVA: Variable para guardar estado original del presupuesto
let _presupOriginal = null;

$(function () {
    InicializaPantallaPresupuesto();
    InicializaEventosPresupuesto();
});

function InicializaPantallaPresupuesto() {
    // INICIALIZAMOS PANELES
    if ($("#divDetalle").is(":visible")) {
        $("#divDetalle").collapse("hide");
    }
    $("#divFiltro").collapse("show");
    // Activar el botón de nuevo combo
    $("#btnAbmNuevo").prop("disabled", false);

    // Configurar el evento click para el botón Cancelar/Inicializar
    $("#btnAbmCancelar").on("click", function (e) {
        cancelarOperacion(e);
    });

    $("#btnCancel").on("click", function () {
        window.location.href = homePresup;
    });

    // Inicializa el período de fechas (hoy / hoy + 30 días)
    initPeriodoFechas();

    // Etiquetas de filtros
    $("#lbChkDesdeHasta").text("Periodo");
    $("#lbRel011").text("Cliente"); // Rel011
    $("#divLs01 div.input-group.input-group-sm span").text("Cliente"); // Rel01
    $("#divLs02 div.input-group.input-group-sm span").text("Usuario"); // Rel02 (opcional)
    $("#divLs03 div.input-group.input-group-sm span").text("Estado");  // Rel03
    $("#chkRel03").prop("disabled", false);
}

function initPeriodoFechas() {
    const hoy = new Date();
    const base = new Date(hoy.getFullYear(), hoy.getMonth(), hoy.getDate());
    const hasta = new Date(base);
    hasta.setDate(hasta.getDate() + 30);

    const format = (d) => {
        const y = d.getFullYear();
        const m = String(d.getMonth() + 1).padStart(2, '0');
        const day = String(d.getDate()).padStart(2, '0');
        return `${y}-${m}-${day}`;
    };

    $("#Date1").val(format(base));
    $("#Date2").val(format(hasta));

    const enabled = $("#chkDesdeHasta").is(":checked");
    $("#Date1").prop("disabled", !enabled);
    $("#Date2").prop("disabled", !enabled);
}

function cancelarOperacion(e) {
    console.log('🔄 Cancelando operación de presupuesto...');

    // ✅ PASO 1: Resetear modos de edición
    modoNuevoPresup = false;
    modoModificacionPresup = false;
    campoEnEdicionPresup = null;
    _presupOriginal = null;

    // ✅ PASO 2: Ocultar divs de datos y productos
    $("#divPresDatos, #divPresProds").hide().empty();
    $("#divDetalle").collapse("hide");

    // ✅ PASO 3: Mostrar panel de filtros
    $("#divFiltro").collapse("show");

    // ✅ PASO 4: Restaurar botones ABM a estado inicial
    $("#btnAbmNuevo").prop("disabled", false);
    $("#btnAbmModif, #btnAbmElimi").prop("disabled", true);
    $("#btnAbmAceptar, #btnAbmCancelar").prop("disabled", true).hide();

    // ✅ PASO 5: Deshabilitar botón de agregar productos
    $("#btnAgregarCProducto").prop("disabled", true);

    // ✅ PASO 6: Limpiar selección de filas en el grid de búsqueda
    $("#tbGridPresupuesto tbody tr").removeClass("selected-row selectedEdit-row");

    console.log('✅ Operación cancelada - Vista reinicializada');

    // ✅ PASO 7: Redirección si es necesario
    if (e && $(e.target).is("#btnAbmCancelar") && typeof homePresup !== 'undefined') {
        console.log('🔀 Redirigiendo a:', homePresup);
        window.location.href = homePresup;
    }
}

function InicializaEventosPresupuesto() {
    // Activar/desactivar período
    $("#chkDesdeHasta").on("change", function () {
        const on = $(this).is(":checked");
        $("#Date1, #Date2").prop("disabled", !on);
    });

    //check generico REL01 activando componentes disables
    $("#chkRel011").on("change", function () {
        const isChecked = $(this).is(":checked");
        
        if (isChecked) {
            $("#Rel011").prop("disabled", false);
            $("#Rel011List").prop("disabled", false);
            
            // ✅ INICIALIZAR AUTOCOMPLETE SOLO UNA VEZ
            if (!$("#Rel011").hasClass("ui-autocomplete-input")) {
                inicializarAutocompleteRel011();
            }
            
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

    //check generico REL02 activando componentes disables
    $("#chkRel022").on("click", function () {
        const isChecked = $(this).is(":checked");

        if (isChecked) {
            $("#Rel022").prop("disabled", false);
            $("#Rel022List").prop("disabled", false);

            // ✅ INICIALIZAR AUTOCOMPLETE SOLO UNA VEZ
            if (!$("#Rel022").hasClass("ui-autocomplete-input")) {
                inicializarAutocompleteRel022();
            }

            // Poner foco después de un pequeño delay
            setTimeout(() => $("#Rel022").trigger("focus"), 50);
        } else {
            $("#Rel022").prop("disabled", true).val("");
            $("#Rel022List").prop("disabled", true).empty();
            $("#Rel022Item").val("");

            // ✅ DESTRUIR INSTANCIA DE AUTOCOMPLETE si existe
            if ($("#Rel022").hasClass("ui-autocomplete-input")) {
                $("#Rel022").autocomplete("destroy");
            }
        }
    });

    // Habilita/Deshabilita Administraciones
    $("#chkRel04").on("change", function () {
        $("#Rel04").prop("disabled", !$(this).is(":checked"));
    });

    $("#pagEstado").on("change", function () {
        var div = $("#divPaginacion");
        presentaPaginacion(div);
    });

    // Limpieza rápida
    $("#Rel011, #Rel022").on("click", function () { $(this).val(""); });

    // Buscar
    $("#btnBuscar").on("click", function () {
        buscarPresupuestos(this);
    });
    funcCallBack = buscarPresupuestos;

    // ✅ NUEVO: Eliminar opción de Rel01List con doble click
    $("#Rel011List").off("dblclick.removeOption").on("dblclick.removeOption", "option", function(e) {
        e.stopPropagation();
        $(this).remove();
        const $list = $("#Rel011List");
        if ($.fn.selectpicker && $list.hasClass("selectpicker")) {
            $list.selectpicker("refresh");
        }
    });

    // ✅ NUEVO: Eliminar opción de Rel02List con doble click
    $("#Rel022List").off("dblclick.removeOption").on("dblclick.removeOption", "option", function(e) {
        e.stopPropagation();
        $(this).remove();
        const $list = $("#Rel022List");
        if ($.fn.selectpicker && $list.hasClass("selectpicker")) {
            $list.selectpicker("refresh");
        }
    });

    // Rel03: mover selección a Rel03List y reinicializar combo
    const $rel03 = $("#Rel03");
    const $rel03List = $("#Rel03List");
    
    $rel03.off("change.rel03").on("change.rel03", function () {
        if (!$rel03List.length) return;

        let vals = $rel03.val();
        if (vals == null) return;
        if (!Array.isArray(vals)) vals = [vals];

        for (let i = 0; i < vals.length; i++) {
            const v = String(vals[i] ?? "").trim();
            if (!v) continue;

            const txt = ($rel03.find(`option[value="${CSSSafe(v)}"]`).first().text() || v).trim();
            appendIfMissingOption($rel03List, v, txt);
        }

        resetComboSilent($rel03);
    });

    // ✅ NUEVO: Eliminar opción de Rel03List con doble click
    $("#Rel03List").off("dblclick.removeOption").on("dblclick.removeOption", "option", function(e) {
        e.stopPropagation();
        $(this).remove();
        const $list = $("#Rel03List");
        if ($.fn.selectpicker && $list.hasClass("selectpicker")) {
            $list.selectpicker("refresh");
        }
    });

    // Evento delegado para el botón de agregar producto
    $(document).on("click", "#btnAgregarCProducto", function () {
        if ($("#busquedaModal").length === 0) {
            cargarModalBusquedaAvanzada(function () {
                if (typeof configurarDestinoBusquedaProductos === 'function') {
                    configurarDestinoBusquedaProductos("presupuestos", agregarProductosAlGrid, obtenerProductosExistentesIds);
                }
                $("#busquedaModal").modal("show");
            });
        } else {
            if (typeof configurarDestinoBusquedaProductos === 'function') {
                configurarDestinoBusquedaProductos("presupuestos", agregarProductosAlGrid, obtenerProductosExistentesIds);
            }
            $("#busquedaModal").modal("show");
        }
    });

    // Doble click para activar edición
    $(document).on('dblclick', '.input-pre_cantidad, .input-pre_margen, .input-pre_pvta', function (e) {
        e.stopPropagation();
        activarEdicionCampoPresup($(this));
    });

    // Enter/Tab para guardar y avanzar
    $(document).on('keydown', '.input-pre_cantidad, .input-pre_margen, .input-pre_pvta', function (e) {
        if (e.key === 'Enter' || e.key === 'Tab') {
            e.preventDefault();
            guardarYAvanzarCampoPresup($(this));
        }
    });

    // Blur para guardar cambios
    $(document).on('blur', '.input-pre_cantidad, .input-pre_margen, .input-pre_pvta', function () {
        const $campo = $(this);
        if ($campo.hasClass('input-pre_cantidad')) {
            recalcularTotalDesdeCantidad($campo);
        } else {
            guardarCampoPresup($campo);
        }
    });

    // Doble click en cta_denominacion (mantener sin cambios)
    $(document).off("dblclick").on("dblclick", "input#cta_denominacion", function () {
        $("input#cta_denominacion").val("");
        $("input#cta_id").val("");
        $("input#pre_nombre").val("");
        $("input#pre_domicilio").val("");
    });
   
    

    // Handler para Nuevo Presupuesto
    $(document).on('click', '#btnAbmNuevo', function (e) {
        e.preventDefault();

        if ($("#divFiltro").is(":visible")) {
            $("#divFiltro").collapse("hide");
        }

        modoNuevoPresup = true;
        modoModificacionPresup = false;

        if (typeof nuevoPresupuestoUrl === 'undefined') {
            console.error('nuevoPresupuestoUrl no está definido.');
            return;
        }

        PostGenHtml({}, nuevoPresupuestoUrl, function (html) {
            $('#divPresDatos').html(html).show();

            $('#divPresupuestoDatos').find('input:not([type=hidden]), textarea, select').each(function () {
                const $el = $(this);
                $el.prop('readonly', false).prop('disabled', false).removeClass('campo-readonly');
            });

            const $first = $('#divPresupuestoDatos').find('input:not([type=hidden]), textarea, select').filter(':visible').first();
            if ($first && $first.length) {
                setTimeout(() => $first.trigger("focus"), 50);
            }

            $('#divPresProds').html(crearGridPresupVacioHtml()).show();
            $('#btnAgregarCProducto').prop('disabled', false);
            $('#btnAbmAceptar').prop('disabled', false).show();
            $('#btnAbmCancelar').prop('disabled', false).show();
            $('#btnAbmModif, #btnAbmNuevo, #btnAbmElimi').prop('disabled', true);

            aplicarReadonlyCamposPresup();
            _presupOriginal = null;

            console.log('Modo Nuevo Presupuesto activado.');
        }, function (err) {
            console.error('Error al cargar NuevoPresupuesto:', err);
        });
    });

    // Handler para Modificar Presupuesto
    $(document).on('click', '#btnAbmModif', function (e) {
        e.preventDefault();

        if ($(this).prop('disabled')) return;

        modoNuevoPresup = false;
        modoModificacionPresup = true;

        _presupOriginal = capturarEstadoFormularioPresup();
        habilitarCamposFormularioPresup(true);
        $('#btnAgregarCProducto').prop('disabled', false);
        $('#btnAbmNuevo, #btnAbmModif, #btnAbmElimi').prop('disabled', true);
        $('#btnAbmAceptar, #btnAbmCancelar').prop('disabled', false).show();

        aplicarReadonlyCamposPresup();

        ///se llama a la funcion que actualiza el valor de costo de los productos
        ///que ya existen en el presupuesto.
        let preId = $("input#pre_id").val();
        if (preId) {            
            cargarProductosPresupuesto(preId,true);
        }


        const $primer = $('#divPresupuestoDatos').find('input:not([type=hidden]):not([readonly]), textarea:not([readonly]), select:not([disabled])').filter(':visible').first();
        if ($primer.length) {
            setTimeout(() => $primer.trigger("focus"), 50);
        }

        console.log('✅ Modo Modificación Presupuesto activado');
    });

    // Autocomplete especializado para Rel011
    $(document).on("keydown.autocomplete", "input#Rel011", function () {
        $(this).autocomplete({
            source: function (request, response) {
                $.ajax({
                    url: autoComRel04Url,
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
                    error: function() {
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
                }
                
                setTimeout(() => $("#Rel011").val(""), 10);
                return false;
            },
            focus: function() {
                return false;
            }
        });
    });

    // Autocomplete para cta_denominacion
    $(document).off("keydown.autocomplete").on("keydown.autocomplete", "input#cta_denominacion", function () {
        $(this).autocomplete({
            source: function (request, response) {
                data = { prefix: request.term }
                $.ajax({
                    url: autoComRel04Url,
                    type: "POST",
                    dataType: "json",
                    data: data,
                    success: function (obj) {
                        response($.map(obj, function (item) {
                            var texto = item.descripcion;
                            return { label: texto, value: item.descripcion, id: item.id, nombre: item.nombre, domicilio: item.domicilio };
                        }));
                    }
                })
            },
            minLength: 3,
            select: function (event, ui) {
                $("input#cta_id").val(ui.item.id);
                $("input#pre_nombre").val(ui.item.nombre);
                $("input#pre_domicilio").val(ui.item.domicilio);
                var data = { cta_id: ui.item.id };
                return true;
            }
        });
    });
} // ✅ CIERRE DE InicializaEventosPresupuesto

// ============================================================================
// FUNCIONES AUXILIARES PARA FORMULARIO
// ============================================================================

function capturarEstadoFormularioPresup() {
    const estado = {};
    $('#divPresupuestoDatos').find('input, textarea, select').each(function() {
        const $campo = $(this);
        const nombre = $campo.attr('name') || $campo.attr('id');
        if (nombre) {
            estado[nombre] = $campo.val();
        }
    });
    return estado;
}

function restaurarEstadoFormularioPresup(estado) {
    if (!estado) return;
    $.each(estado, function(nombre, valor) {
        const $campo = $(`[name="${nombre}"], #${nombre}`);
        if ($campo.length) {
            $campo.val(valor);
        }
    });
}

function habilitarCamposFormularioPresup(habilitar) {
    const camposNoEditables = [
        'pre_id', 'pret_id', 'pree_id', 'usu_id', 'usu_apellidoynombre',
        'adm_id', 'adm_nombre', 'tco_id', 'cm_compte'
    ];

    $('#divPresupuestoDatos').find('input:not([type=hidden]), textarea, select').each(function() {
        const $campo = $(this);
        const nombre = $campo.attr('name') || $campo.attr('id');
        
        const esNoEditable = camposNoEditables.some(campo => 
            nombre === campo || nombre?.includes(campo)
        );

        if (esNoEditable) {
            $campo.prop('readonly', true).prop('disabled', true).addClass('campo-readonly');
        } else if (habilitar) {
            if ($campo.is('select')) {
                $campo.prop('disabled', false).removeClass('campo-readonly');
            } else {
                $campo.prop('readonly', false).removeClass('campo-readonly');
            }
        } else {
            if ($campo.is('select')) {
                $campo.prop('disabled', true).addClass('campo-readonly');
            } else {
                $campo.prop('readonly', true).addClass('campo-readonly');
            }
        }
    });
}

// ============================================================================
// FUNCIONES DE EDICIÓN DE CAMPOS
// ============================================================================

function activarEdicionCampoPresup($campo) {
    if (!estaEnModoEdicionPresup()) return;
    if (campoEnEdicionPresup !== null) return;

    campoEnEdicionPresup = $campo[0];
    $campo.prop('readonly', false)
          .removeClass('campo-readonly')
          .focus()
          .select();
}

function guardarYAvanzarCampoPresup($campo) {
    guardarCampoPresup($campo);
    
    const $fila = $campo.closest('tr');
    const esMargen = $campo.hasClass('input-pre_margen');
    
    if (esMargen) {
        const $siguiente = $fila.find('.input-pre_pvta');
        if ($siguiente.length) {
            setTimeout(() => activarEdicionCampoPresup($siguiente), 50);
            return;
        }
    }
    
    const $siguienteFila = $fila.next('tr');
    if ($siguienteFila.length) {
        const $siguiente = $siguienteFila.find('.input-pre_margen');
        if ($siguiente.length) {
            setTimeout(() => activarEdicionCampoPresup($siguiente), 50);
        }
    }
}

function guardarCampoPresup($campo) {
    if ($campo.prop('readonly')) return;

    const $fila = $campo.closest('tr');
    const esMargen = $campo.hasClass('input-pre_margen');
    
    const valorOriginal = parseFloat($campo.data('original-value')) || 0;
    const valorNuevo = parseFloat($campo.val().replace(/,/g, '')) || 0;
    
    $campo.val(valorNuevo.toFixed(2));
    $campo.prop('readonly', true).addClass('campo-readonly');
    
    campoEnEdicionPresup = null;
    
    if (Math.abs(valorOriginal - valorNuevo) > 0.01) {
        if (esMargen) {
            recalcularPrecioDesdeMargen($fila, valorNuevo);
        } else {
            recalcularMargenDesdePrecio($fila, valorNuevo);
        }
        marcarCampoModificadoPresup($campo);
    }
}

function recalcularTotalDesdeCantidad($campo) {
    if ($campo.prop('readonly')) return;

    const $fila = $campo.closest('tr');
    const cantidadOriginal = parseFloat($campo.data('original-value')) || 0;
    const cantidadNueva = parseFloat($campo.val().replace(/,/g, '')) || 0;

    if (cantidadNueva <= 0) {
        $campo.val(cantidadOriginal.toFixed(2));
        AbrirMensaje("Advertencia",
            "La cantidad debe ser mayor a 0",
            () => $("#msjModal").modal("hide"),
            false, ["Aceptar"], "warn!", null);
        return;
    }

    $campo.val(cantidadNueva.toFixed(2));
    $campo.data('original-value', cantidadNueva);
    $campo.prop('readonly', true).addClass('campo-readonly');
    campoEnEdicionPresup = null;

    if (Math.abs(cantidadOriginal - cantidadNueva) > 0.01) {
        const precioVenta = parseFloat($fila.find('.input-pre_pvta').val().replace(/,/g, '')) || 0;
        const nuevoTotal = precioVenta * cantidadNueva;
        
        actualizarTotalFila($fila, nuevoTotal);
        actualizarTotalGeneralPresup();
        marcarCampoModificadoPresup($campo);
    }
}

function marcarCampoModificadoPresup($campo) {
    if (!$campo || !$campo.length) return;
    $campo.addClass('campo-modificado');
    setTimeout(() => $campo.removeClass('campo-modificado'), 1500);
}

function recalcularPrecioDesdeMargen($fila, nuevoMargen) {
    const preCosto = parseFloat($fila.data('pre-costo')) || 0;
    const preCantidad = parseFloat($fila.find('.input-pre_cantidad').val().replace(/,/g, '')) || 1;
    
    const preNeto = preCosto * (1 + nuevoMargen / 100);
    
    const ivaSituacion = $fila.data('iva-situacion') || 'E';
    const ivaAlicuota = parseFloat($fila.data('iva-alicuota')) || 0;
    const inAlicuota = parseFloat($fila.data('in-alicuota')) || 0;
    
    let precioVenta = preNeto;
    if (ivaSituacion === 'G') {
        precioVenta = preNeto * (1 + ivaAlicuota / 100);
    }
    if (inAlicuota > 0) {
        precioVenta = precioVenta * (1 + inAlicuota / 100);
    }
    
    const total = precioVenta * preCantidad;
    
    const $campoPVta = $fila.find('.input-pre_pvta');
    $campoPVta.val(precioVenta.toFixed(3));
    $campoPVta.data('original-value', precioVenta);
    marcarCampoModificadoPresup($campoPVta);
    
    actualizarTotalFila($fila, total);
    actualizarTotalGeneralPresup();
}

function recalcularMargenDesdePrecio($fila, nuevoPrecio) {
    const preCosto = parseFloat($fila.data('pre-costo')) || 0;
    const preCantidad = parseFloat($fila.find('.input-pre_cantidad').val().replace(/,/g, '')) || 1;
    
    if (preCosto === 0) {
        AbrirMensaje("Error", 
            "No se puede calcular el margen: el costo es cero",
            () => $("#msjModal").modal("hide"),
            false, ["Aceptar"], "error!", null);
        return;
    }
    
    const ivaSituacion = $fila.data('iva-situacion') || 'E';
    const ivaAlicuota = parseFloat($fila.data('iva-alicuota')) || 0;
    const inAlicuota = parseFloat($fila.data('in-alicuota')) || 0;
    
    let preNeto = nuevoPrecio;
    if (inAlicuota > 0) {
        preNeto = preNeto / (1 + inAlicuota / 100);
    }
    if (ivaSituacion === 'G') {
        preNeto = preNeto / (1 + ivaAlicuota / 100);
    }
    
    const nuevoMargen = ((preNeto - preCosto) / preCosto) * 100;
    
    if (nuevoMargen < 0) {
        const $campoPVta = $fila.find('.input-pre_pvta');
        const valorOriginal = parseFloat($campoPVta.data('original-value')) || 0;
        $campoPVta.val(valorOriginal.toFixed(3));
        
        AbrirMensaje("Advertencia", 
            `El precio de venta genera un margen negativo (${nuevoMargen.toFixed(2)}%).`,
            () => $("#msjModal").modal("hide"),
            false, ["Aceptar"], "warn!", null);
        return;
    }
    
    const $campoMargen = $fila.find('.input-pre_margen');
    $campoMargen.val(nuevoMargen.toFixed(2));
    $campoMargen.data('original-value', nuevoMargen);
    marcarCampoModificadoPresup($campoMargen);
    
    const total = nuevoPrecio * preCantidad;
    actualizarTotalFila($fila, total);
    actualizarTotalGeneralPresup();
}

function actualizarTotalFila($fila, total) {
    $fila.find('.td-total').text(total.toFixed(2));
}

function actualizarTotalGeneralPresup() {
    let totalGeneral = 0;
    
    $('#tbGridPresupuestoProds tbody tr').each(function() {
        const $fila = $(this);
        if ($fila.find('td[colspan]').length > 0) return;
        
        const total = parseFloat($fila.find('.td-total').text().replace(/,/g, '')) || 0;
        totalGeneral += total;
    });
    
    $('#tbGridPresupuestoProds tfoot .fw-bold:last').text(totalGeneral.toFixed(2));
}

// ============================================================================
// BÚSQUEDA Y RENDER DE GRID
// ============================================================================

let _presuLoading = false;

async function buscarPresupuestos(btn, pag = 1) {
    if (_presuLoading) return;
    _presuLoading = true;

    const $btn = $(btn);
    const originalHtml = $btn.html();
    setBtnLoading($btn, true);

    try {
        const filtros = buildQueryFilters(pag);
        const url = buscarPresupuestosUrl;

        PostGenHtml(filtros, url, function (html) {
            $("#divDetalle").html(html).collapse("show");
            $("#divFiltro").collapse("hide");

            configurarEventosSeleccionPres();

            PostGen({}, buscarMetadataURL, function (obj) {
                if (obj.error === true) {
                    AbrirMensaje("ATENCIÓN", obj.msg, function () {
                        $("#msjModal").modal("hide");
                        return true;
                    }, false, ["Aceptar"], "error!", null);
                } else {
                    totalRegs = obj.metadata.totalCount;
                    pags = obj.metadata.totalPages;
                    pagRegs = obj.metadata.pageSize;
                    $("#pagEstado").val(true).trigger("change");
                }
            });
        });
    } catch (e) {
        console.error("Error al buscar presupuestos:", e);
        $("#divDetalle").html('<div class="alert alert-danger py-2 mb-0">No se pudo obtener la información.</div>').collapse("show");
    } finally {
        setBtnLoading($btn, false, originalHtml);
        _presuLoading = false;
    }
}

function configurarEventosSeleccionPres() {
    $(document).off("click", "#tbGridPresupuesto tbody tr");
    $(document).on("click", "#tbGridPresupuesto tbody tr", function (e) {
        if (!$(e.target).is("button, a, .btn, i")) {
            var $this = $(this);
            var fueSeleccionado = $this.hasClass("selected-row");

            $("#tbGridPresupuesto tbody tr").removeClass("selected-row");

            if (!fueSeleccionado) {
                $this.addClass("selected-row");
                let preId = $this.data("pre-id");
                if (preId) {
                    cargarPresupuestoDatos(preId);
                    cargarProductosPresupuesto(preId);
                }
            }
        }
    });
}

function cargarPresupuestoDatos(preId) {
    const url = obtenerPresupuestoDatoUrl; 
    PostGenHtml({ pre_id: preId }, url, function (html) {
        $("#divPresDatos").html(html).show();
        
        // ✅ DETERMINAR PERMISOS DE EDICIÓN BASÁNDOSE EN EL ESTADO DEL PRESUPUESTO
        // ════════════════════════════════════════════════════════════════════
        // La función removida puedeEditarPresupuesto() ha sido reempFazada por Facturado lógica.
        // 
        // sistemas de presupuestos:
        // 'P' = Pendiente (editable)
        // 'F' = Facturado (no editable)
        // 'R' = Remitido (no editable)
        // 'A' = Anulado (no editable)
        //
        // ⚠️ IMPORTANTE: Ajustar el array 'estadosEditables' según los estados
        //    reales definidos en la base de datos (tabla PresupE o similar)
        // ═══════════════════════════════════════════════════════════════════════
        
        const preeId = $("#pree_id").val(); // Estado del presupuesto desde el formulario cargado
        
        //✅ Solo permitir edición si está en estado Pendiente ('P') o Borrador ('B')
        const estadosEditables = ['P']; // ⚠️ Ajustar estos valores según sea necesario

        //hay que tener en cuenta también que los presupuestos facturados no se pueden editar
        //por lo que si el estado es 'F' tampoco se podrá editar
        //Tampoco se podrá editar si la fecha actual esta fuera del periodo desde/hasta. 
        //esta ultima validación la dejaremos pendiente.

        const permite = estadosEditables.includes(preeId);
        
        $("#btnAbmModif").prop("disabled", !permite);

        // Debug - ayuda a identificar estados del sistema
        console.log("cargarPresupuestoDatos: Estado del presupuesto:", preeId, 
                    "Permite edición:", permite);
    });
}

function cargarProductosPresupuesto(preId,isUpdate=false) {
    let url = "";
    if (isUpdate) {
        //trae los productos con los costos actualizados
        url = obtenerPresupuestoProductoActualizadoUrl;
    }
    else {
        //trae los productos tal cual están en el presupuesto
        url = obtenerPresupuestoProductoUrl;
    }
        
    PostGenHtml({ pre_id: preId }, url, function(html) {
        $("#divPresProds").empty().html(html).show();
        // Forzar estado readonly acorde al modo
        aplicarReadonlyCamposPresup();
    });
}

// ============================================================================
// HELPERS
// ============================================================================

function buildQueryFilters(pag) {
    const usaPeriodo = $("#chkDesdeHasta").is(":checked");
    const fechaD = usaPeriodo ? $("#Date1").val() : null;
    const fechaH = usaPeriodo ? $("#Date2").val() : null;

    const rel01 = getValues("#Rel011List", false);
    const rel02 = getValues("#Rel022List", false);
    const rel03 = getValues("#Rel03List", true);
    
    let rel04Val = $("#Rel04").val();
    let rel04 = [];
    if (rel04Val) {
        rel04.push({
            Id: $("#Rel04").val(),
            Descripcion: $("#Rel04 option:selected").text().trim()
        });        
    }

    return {
        Registros: 200,
        Pagina: pag,
        FechaD: fechaD || null,
        FechaH: fechaH || null,
        Rel01: rel01.length ? rel01 : null,
        Rel02: rel02.length ? rel02 : null,
        Rel03: rel03.length ? rel03 : null,
        Rel04: rel04
    };
}

function getValues(src, asComboDto = false) {
    const $el = typeof src === "string" ? $(src) : src;
    if (!$el || !$el.length) return [];

    let items = [];

    if ($el.is("select")) {
        items = $el.find("option").map((_, o) => ({
            value: o.value,
            text: o.text || o.value
        })).get();
    } else {
        const dataVals = $el.attr("data-values") ?? $el.data("values");
        if (Array.isArray(dataVals)) {
            items = dataVals.map(v => ({ value: String(v), text: String(v) }));
        } else if (typeof dataVals === "string" && dataVals.trim()) {
            items = dataVals.split(",").map(v => ({ value: v.trim(), text: v.trim() }));
        } else {
            items = $el.find("[data-id],[value]").map((_, n) => {
                const $n = $(n);
                const id = $n.attr("data-id") ?? $n.attr("value");
                const txt = $n.attr("data-text") ?? $n.text() ?? id;
                return { value: id, text: txt };
            }).get();
        }
    }

    if (!items || items.length === 0) return [];

    const seen = new Set();
    const out = [];
    for (let i = 0; i < items.length; i++) {
        const v = String(items[i].value ?? "").trim();
        if (!v || seen.has(v)) continue;
        seen.add(v);
        
        if (asComboDto) {
            out.push({
                Id: v,
                Descripcion: String(items[i].text ?? v).trim()
            });
        } else {
            out.push(v);
        }
    }
    
    return out;
}

function appendIfMissingOption($select, value, text) {
    if (!$select || !$select.length) return;
    const exists = $select.find("option").filter(function () { return this.value === value; }).length > 0;
    if (!exists) {
        const opt = new Option(text || value, value, false, true);
        $select.append(opt);
    } else {
        $select.find("option").filter(function () { return this.value === value; }).prop("selected", true);
    }

    if ($.fn.selectpicker && $select.hasClass("selectpicker")) {
        $select.selectpicker("refresh");
    }
}

function resetComboSilent($el) {
    if (!$el || !$el.length) return;
    
    const handlers = $._data($el[0], "events");
    $el.off("change");
    
    if ($.fn.selectpicker && $el.hasClass("selectpicker")) {
        $el.selectpicker("val", []);
    } else if ($el.data("select2")) {
        $el.val(null);
    } else {
        $el.val("");
    }
    
    if (handlers && handlers.change) {
        handlers.change.forEach(h => $el.on("change", h.handler));
    }
}

function resetCombo($el) {
    if (!$el || !$el.length) return;
    if ($.fn.selectpicker && $el.hasClass("selectpicker")) {
        $el.selectpicker("val", []);
    } else if ($el.data("select2")) {
        $el.val(null).trigger("change");
    } else {
        $el.val("").trigger("change");
    }
}

function CSSSafe(v) {
    if (window.CSS && typeof window.CSS.escape === "function") {
        return CSS.escape(v);
    }
    return String(v).replace(/([!"#$%&'()*+,.\/:;<=>?@\[\\\]^`{|}~])/g, "\\$1");
}

function setBtnLoading($btn, loading, originalHtml) {
    if (!$btn || !$btn.length) return;
    if (loading) {
        $btn.prop("disabled", true).html('<span class="spinner-border spinner-border-sm me-1"></span>Buscando...');
    } else {
        $btn.prop("disabled", false).html(originalHtml ?? "Buscar");
    }
}

function estaEnModoEdicionPresup() {
    return !!(modoNuevoPresup || modoModificacionPresup);
}

function aplicarReadonlyCamposPresup() {
    const campos = $('.input-pre_margen, .input-pre_pvta, .input-pre_cantidad');
    const tooltipMsg = 'Active el modo edición (Editar) para modificar este campo';

    requestAnimationFrame(() => {
        if (!estaEnModoEdicionPresup()) {
            campos.each(function () {
                const $c = $(this);
                $c.prop('readonly', true).addClass('campo-readonly');
                if (!$c.attr('title')) $c.attr('title', tooltipMsg);
            });
        } else {
            campos.each(function () {
                const $c = $(this);
                $c.prop('readonly', true).addClass('campo-readonly');
                $c.attr('title', 'Doble click para editar');
            });
        }
    });
}

function crearGridPresupVacioHtml() {
    return `
    <div class="card h-100">
        <div class="card-header py-1 d-flex justify-content-between align-items-center">
            <h6 class="mb-0">Productos del Presupuesto</h6>
            <button type="button" class="btn btn-sm btn-outline-primary" id="btnAgregarCProducto" title="Agregar Producto" disabled>
                <i class="bx bx-plus"></i>
            </button>
        </div>
        <div class="card-body p-1">
            <div class="table-responsive" style="max-height: 400px;">
                <table class="table table-sm table-hover mb-0 table-golden" id="tbGridPresupuestoProds">
                    <thead class="table-golden-header">
                        <tr class="header">
                            <th class="text-center">Código</th>
                            <th class="text-left">Descripción</th>
                            <th class="text-end">Costo</th>
                            <th class="text-end">Cantidad</th>
                            <th class="text-end">Mg %</th>
                            <th class="text-end">Venta</th>
                            <th class="text-end">Total</th>
                        </tr>
                    </thead>
                    <tbody>
                        <tr>
                            <td colspan="7" class="text-center text-muted py-2">
                                <i class="bx bx-info-circle me-1"></i>No hay productos en este presupuesto
                            </td>
                        </tr>
                    </tbody>
                </table>
            </div>
        </div>
    </div>`;
}

// ============================================================================
// INTEGRACIÓN CON BÚSQUEDA AVANZADA V02
// ============================================================================

function cargarModalBusquedaAvanzada(callback) {
    if ($("#busquedaModal").length > 0) {
        if (typeof callback === 'function') callback();
        return;
    }

    const urlModal = typeof busquedaAvanzadaModalUrl !== 'undefined' 
        ? busquedaAvanzadaModalUrl 
        : '/ControlComun/Producto/BusquedaAdvanceV02';

    $.ajax({
        url: urlModal,
        type: 'GET',
        success: function(html) {
            if ($("#busquedaModal").length === 0) {
                $('body').append(html);
            }
            if (typeof callback === 'function') {
                callback();
            }
        },
        error: function(xhr, status, error) {
            console.error("Error al cargar modal de búsqueda:", error);
            ControlaMensajeError("No se pudo cargar el módulo de búsqueda de productos");
        }
    });
}

function obtenerProductosExistentesIds() {
    const productosIds = [];
    
    $('#tbGridPresupuestoProds tbody tr').each(function() {
        const $fila = $(this);
        if ($fila.find('td[colspan]').length > 0) return;
        
        const pId = $fila.data('p-id');
        if (pId) {
            productosIds.push(pId);
        }
    });
    
    return productosIds;
}

function agregarProductosAlGrid(productos) {
    if (!Array.isArray(productos) || productos.length === 0) return;

    const $tbody = $('#tbGridPresupuestoProds tbody');
    
    const $filaVacia = $tbody.find('tr td[colspan]');
    if ($filaVacia.length > 0) {
        $filaVacia.closest('tr').remove();
    }

    let $tfoot = $('#tbGridPresupuestoProds tfoot');
    if ($tfoot.length === 0) {
        $('#tbGridPresupuestoProds').append(`
            <tfoot class="table-golden-footer">
                <tr>
                    <td colspan="6" class="text-end fw-bold">Total General:</td>
                    <td class="text-end fw-bold">0.00</td>
                </tr>
            </tfoot>
        `);
        $tfoot = $('#tbGridPresupuestoProds tfoot');
    }

    let esAlternado = $tbody.find('tr').length % 2 !== 0;

    productos.forEach(function(producto) {
        const fila = crearFilaProductoPresupuesto(producto, esAlternado);
        $tbody.append(fila);
        esAlternado = !esAlternado;
    });

    aplicarInputMaskPresupuesto();
    aplicarReadonlyCamposPresup();
    actualizarTotalGeneralPresup();
}

function crearFilaProductoPresupuesto(producto, esAlternado) {
    const pId = producto.p_id || '';
    const pDesc = producto.p_desc || '';
    const pCosto = parseFloat(producto.p_pcosto || 0);
    const cantidad = 1;
    const margen = 30;



    const precioNeto = pCosto * (1 + margen / 100);
    
    const ivaSituacion = producto.iva_situacion || 'E';
    const ivaAlicuota = parseFloat(producto.iva_alicuota || 21);
    const inAlicuota = parseFloat(producto.in_alicuota || 0);
    
    let precioVenta = precioNeto;
    if (ivaSituacion === 'G') {
        precioVenta = precioVenta * (1 + ivaAlicuota / 100);
    }
    if (inAlicuota > 0) {
        precioVenta = precioVenta * (1 + inAlicuota / 100);
    }
    
    const total = cantidad * precioVenta;
    const claseAlt = esAlternado ? 'alt' : '';
    
    return `
        <tr class="${claseAlt}"
            data-p-id="${pId}"
            data-pre-pcosto="${pCosto.toFixed(3)}"
            data-pre-pneto="${precioNeto.toFixed(3)}"
            data-iva-situacion="${ivaSituacion}"
            data-iva-alicuota="${ivaAlicuota}"
            data-in-alicuota="${inAlicuota}">
            <td class="text-center">${pId}</td>
            <td>${pDesc}</td>
            <td class="text-end">${pCosto.toFixed(3)}</td>
            <td class="text-end">
                <div class="input-container">
                    <input type="text" 
                           class="form-control form-control-sm input-pre_cantidad input-numeric campo-readonly"
                           value="${cantidad.toFixed(2)}"
                           data-original-value="${cantidad}"
                           readonly 
                           title="Doble click para editar" />
                </div>
            </td>
            <td class="text-end">
                <div class="input-container">
                    <input type="text" 
                           class="form-control form-control-sm input-pre_margen input-numeric campo-readonly"
                           value="${margen.toFixed(2)}"
                           data-original-value="${margen}"
                           readonly 
                           title="Doble click para editar" />
                </div>
            </td>
            <td class="text-end">
                <div class="input-container">
                    <input type="text" 
                           class="form-control form-control-sm input-pre_pvta input-numeric campo-readonly"
                           value="${precioVenta.toFixed(3)}"
                           data-original-value="${precioVenta}"
                           readonly 
                           title="Doble click para editar" />
                </div>
            </td>
            <td class="text-end td-total">${total.toFixed(2)}</td>
        </tr>
    `;
}

function aplicarInputMaskPresupuesto() {
    if (typeof Inputmask === 'undefined') return;

    Inputmask({
        alias: "numeric",
        groupSeparator: ",",
        radixPoint: ".",
        autoGroup: true,
        digits: 2,
        digitsOptional: false,
        rightAlign: true,
        allowMinus: false,
        min: 0
    }).mask('.input-pre_cantidad:not(.inputmask-applied)');
    
    Inputmask({
        alias: "numeric",
        groupSeparator: ",",
        radixPoint: ".",
        autoGroup: true,
        digits: 2,
        digitsOptional: false,
        rightAlign: true,
        allowMinus: false,
        min: 0
    }).mask('.input-pre_margen:not(.inputmask-applied)');

    Inputmask({
        alias: "numeric",
        groupSeparator: ",",
        radixPoint: ".",
        autoGroup: true,
        digits: 3,
        digitsOptional: false,
        rightAlign: true,
        allowMinus: false,
        min: 0
    }).mask('.input-pre_pvta:not(.inputmask-applied)');

    $('.input-pre_cantidad, .input-pre_margen, .input-pre_pvta').addClass('inputmask-applied');
}

// ============================================================================
// AUTOCOMPLETE
// ============================================================================

function inicializarAutocompleteRel011() {
    if (typeof autoComRel04Url === 'undefined') {
        console.error("autoComRel04Url no está definida");
        return;
    }

    $("#Rel011").autocomplete({
        source: function (request, response) {
            $.ajax({
                url: autoComRel04Url,
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
                error: function() {
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
            }
            
            setTimeout(() => $("#Rel011").val(""), 10);
            return false;
        },
        focus: function() {
            return false;
        }
    });
}

function inicializarAutocompleteRel022() {
    if (typeof autoComRel05Url === 'undefined') {
        console.error("autoComRel05Url no está definida");
        return;
    }

    $("#Rel022").autocomplete({
        source: function (request, response) {
            $.ajax({
                url: autoComRel05Url,
                type: "POST",
                dataType: "json",
                data: { prefix: request.term },
                success: function (obj) {
                    response($.map(obj, function (item) {
                        return {
                            label: item.descripcion,
                            value: item.descripcion,
                            id: item.id,
                            nombre: item.nombre || item.descripcion
                        };
                    }));
                },
                error: function() {
                    response([]);
                }
            });
        },
        minLength: 3,
        select: function (event, ui) {
            const yaExiste = $("#Rel022List option[value='" + ui.item.id + "']").length > 0;
            
            if (!yaExiste) {
                $("#Rel022Item").val(ui.item.id);
                const opcion = $("<option></option>")
                    .attr("value", ui.item.id)
                    .text(ui.item.label);
                $("#Rel022List").append(opcion);
            }
            
            setTimeout(() => $("#Rel022").val(""), 10);
            return false;
        },
        focus: function() {
            return false;
        }
    });
}

function destruirAutocompleteRel011() {
    if ($("#Rel011").hasClass("ui-autocomplete-input")) {
        $("#Rel011").autocomplete("destroy");
    }
}

// ============================================================================
// PROCESAMIENTO DE PRODUCTOS MÚLTIPLES (PARA BUSQUEDASV02.JS)
// ============================================================================

function procesarAgregarProductosMultiples() {
    AbrirWaiting("Agregando productos al presupuesto...");
    
    try {
        const productosExistentesIds = obtenerProductosExistentesIds();
        const productosFiltrados = productosSeleccionadosBusqueda.filter(producto => 
            !productosExistentesIds.includes(producto.p_id));
        
        const cantidadDuplicados = productosSeleccionadosBusqueda.length - productosFiltrados.length;
        
        if (productosFiltrados.length === 0) {
            CerrarWaiting();
            if (cantidadDuplicados > 0) {
                ControlaMensajeWarning(`Los ${cantidadDuplicados} producto(s) seleccionado(s) ya existen en el presupuesto.`);
            } else {
                ControlaMensajeWarning("No hay productos para agregar.");
            }
            return;
        }
        
        agregarProductosAlGrid(productosFiltrados);
        $("#busquedaModal").modal("hide");
        
        const cantidadAgregada = productosFiltrados.length;
        limpiarSeleccionBusqueda();
        
        CerrarWaiting();                            
        let mensaje = '';
        
        if (cantidadDuplicados > 0) {
            mensaje = `Se agregaron ${cantidadAgregada} producto(s). Se omitieron ${cantidadDuplicados} producto(s) duplicado(s).`;
        } else {
            mensaje = `${cantidadAgregada} producto${cantidadAgregada > 1 ? 's' : ''} agregado${cantidadAgregada > 1 ? 's' : ''} correctamente al presupuesto`;
        }
        
        ControlaMensajeSuccess(mensaje);
    } catch (error) {
        CerrarWaiting();
        console.error("Error al procesar productos:", error);
        ControlaMensajeError("Error al agregar productos: " + error.message);
    }
}

