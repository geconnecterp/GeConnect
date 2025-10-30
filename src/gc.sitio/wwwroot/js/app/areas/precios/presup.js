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
    $("#btnCancel, #btnAbmCancelar").on("click", function (e) {
        cancelarOperacion(e);
    });
    // Inicializa el período de fechas (hoy / hoy + 30 días)
    initPeriodoFechas();

    // Etiquetas de filtros
    $("#lbChkDesdeHasta").text("Periodo");
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
    //// Ocultar formulario
    //$("#divPresDatos").hide();
    //$("#divPresProds").hide();

    //// Desactivar modos de edición
    //modoNuevoPresup = false;
    //modoModificacionCombo = false;


    //// Restaurar estado de los botones
    //$("#btnAbmNuevo").prop("disabled", false);
    //$("#btnAbmAceptar").prop("disabled", true);
    //$("#btnAbmModif").prop("disabled", true); // Deshabilitar botón modificar también

    // Si existe un homePresup y necesitamos redirigir
    if (e && $("#btnAbmCancelar").is(e.target) && typeof homePresup !== 'undefined') {
        window.location.href = homePresup;
    }
}

function InicializaEventosPresupuesto() {
    // Activar/desactivar período
    $("#chkDesdeHasta").on("change", function () {
        const on = $(this).is(":checked");
        $("#Date1, #Date2").prop("disabled", !on);
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
    $("#Rel01, #Rel02").on("click", function () { $(this).val(""); });

    // Buscar
    $("#btnBuscar").on("click", function () {
        buscarPresupuestos(this);
    });
    funcCallBack = buscarPresupuestos;

    // ✅ NUEVO: Eliminar opción de Rel01List con doble click
    $("#Rel01List").off("dblclick.removeOption").on("dblclick.removeOption", "option", function(e) {
        e.stopPropagation();
        $(this).remove();
        // Actualizar plugin si existe
        const $list = $("#Rel01List");
        if ($.fn.selectpicker && $list.hasClass("selectpicker")) {
            $list.selectpicker("refresh");
        }
    });

    // ✅ NUEVO: Eliminar opción de Rel02List con doble click (si es necesario)
    $("#Rel02List").off("dblclick.removeOption").on("dblclick.removeOption", "option", function(e) {
        e.stopPropagation();
        $(this).remove();
        // Actualizar plugin si existe
        const $list = $("#Rel02List");
        if ($.fn.selectpicker && $list.hasClass("selectpicker")) {
            $list.selectpicker("refresh");
        }
    });

    // Rel03: mover selección a Rel03List y reinicializar combo (SIN recursión)
    const $rel03 = $("#Rel03");
    const $rel03List = $("#Rel03List");
    
    $rel03.off("change.rel03").on("change.rel03", function () {
        if (!$rel03List.length) return;

        let vals = $rel03.val();
        if (vals == null) return;
        if (!Array.isArray(vals)) vals = [vals];

        // Añadir cada valor seleccionado al listado (evitar duplicados)
        for (let i = 0; i < vals.length; i++) {
            const v = String(vals[i] ?? "").trim();
            if (!v) continue;

            const txt = ($rel03.find(`option[value="${CSSSafe(v)}"]`).first().text() || v).trim();
            appendIfMissingOption($rel03List, v, txt);
        }

        // Reinicializar combo SIN disparar change (evita recursión)
        resetComboSilent($rel03);
    });

    // ✅ NUEVO: Eliminar opción de Rel03List con doble click
    $("#Rel03List").off("dblclick.removeOption").on("dblclick.removeOption", "option", function(e) {
        e.stopPropagation();
        $(this).remove();
        // Actualizar plugin si existe
        const $list = $("#Rel03List");
        if ($.fn.selectpicker && $list.hasClass("selectpicker")) {
            $list.selectpicker("refresh");
        }
    });

    // Inicializar eventos de edición de productos presupuesto
    $(document).on('dblclick', '.input-pre_margen, .input-pre_pvta', function(e) {
        e.stopPropagation();
        activarEdicionCampoPresup($(this));
    });

    $(document).off("dblclick").on("dblclick", "input#cta_denominacion", function () {
        $("input#cta_denominacion").val("");
        $("input#cta_id").val("");
        $("input#pre_nombre").val("");
        $("input#pre_domicilio").val("");
    });

    // Evento para Enter/Tab
    $(document).on('keydown', '.input-pre_margen, .input-pre_pvta', function(e) {
        if (e.key === 'Enter' || e.key === 'Tab') {
            e.preventDefault();
            guardarYAvanzarCampoPresup($(this));
        }
    });

    // Evento blur
    $(document).on('blur', '.input-pre_margen, .input-pre_pvta', function() {
        guardarCampoPresup($(this));
    });

    // ✅ NUEVO: Eventos para edición de cantidad
    $(document).on('dblclick', '.input-pre_cantidad', function(e) {
        e.stopPropagation();
        activarEdicionCampoPresup($(this));
    });

    // Evento para Enter/Tab en cantidad
    $(document).on('keydown', '.input-pre_cantidad', function(e) {
        if (e.key === 'Enter' || e.key === 'Tab') {
            e.preventDefault();
            recalcularTotalDesdeCantidad($(this));
        }
    });

    // Evento blur en cantidad
    $(document).on('blur', '.input-pre_cantidad', function() {
        recalcularTotalDesdeCantidad($(this));
    });

    // Handler para Nuevo Presupuesto
    $(document).on('click', '#btnAbmNuevo', function (e) {
        e.preventDefault();

        if ($("#divFiltro").is(":visible")) {
            $("#divFiltro").collapse("hide");
        }

        // Establecer modo nuevo
        modoNuevoPresup = true;
        modoModificacionPresup = false;

        // Limpiar/Inicializar datos del formulario y productos
        // 1) Cargar partial de datos mediante action NuevoPresupuesto
        // Se asume que existe la variable nuevoPresupuestoUrl definida en la vista
        if (typeof nuevoPresupuestoUrl === 'undefined') {
            console.error('nuevoPresupuestoUrl no está definido.');
            return;
        }

        PostGenHtml({}, nuevoPresupuestoUrl, function (html) {
            // Insertar partial de datos
            $('#divPresDatos').html(html).show();

            // Hacer que los campos del formulario sean editables (nuevo)
            $('#divPresupuestoDatos').find('input:not([type=hidden]), textarea, select').each(function () {
                const $el = $(this);
                $el.prop('readonly', false).prop('disabled', false).removeClass('campo-readonly');
            });

            // Poner foco en el primer campo editable (si existe)
            const $first = $('#divPresupuestoDatos').find('input:not([type=hidden]), textarea, select').filter(':visible').first();
            if ($first && $first.length) {
                setTimeout(() => $first.trigger("focus"), 50);
            }

            // 2) Inicializar grid de productos vacío
            $('#divPresProds').html(crearGridPresupVacioHtml()).show();

            // Habilitar botones para agregar productos
            $('#btnAgregarCProducto, #btnAgregarSustituto').prop('disabled', false);

            // Habilitar Aceptar / Cancelar y deshabilitar Nuevo / Modif / Elimi
            $('#btnAbmAceptar').prop('disabled', false).show();
            $('#btnAbmCancelar').prop('disabled', false).show();
            $('#btnAbmModif, #btnAbmNuevo, #btnAbmElimi').prop('disabled', true);

            // Aplicar estado readonly en campos del grid (no debe poderse editar hasta doble click en modo edición)
            aplicarReadonlyCamposPresup();

            // Guardar estado original nulo para nuevo
            _presupOriginal = null;

            console.log('Modo Nuevo Presupuesto activado. Partial de datos cargado y grid inicializado vacío.');
        }, function (err) {
            console.error('Error al cargar NuevoPresupuesto:', err);
        });
    });

    // ✅ NUEVO: Handler para Modificar Presupuesto
    $(document).on('click', '#btnAbmModif', function (e) {
        e.preventDefault();

        if ($(this).prop('disabled')) return;

        // Establecer modo modificación
        modoNuevoPresup = false;
        modoModificacionPresup = true;

        // Guardar estado original para restaurar en caso de cancelar
        _presupOriginal = capturarEstadoFormularioPresup();

        // Habilitar campos editables del formulario (excepto los especificados)
        habilitarCamposFormularioPresup(true);

        // Actualizar estado de botones ABM
        $('#btnAbmNuevo, #btnAbmModif, #btnAbmElimi').prop('disabled', true);
        $('#btnAbmAceptar, #btnAbmCancelar').prop('disabled', false).show();

        // Los campos del grid se mantienen en readonly hasta doble click
        aplicarReadonlyCamposPresup();

        // Poner foco en el primer campo editable
        const $primer = $('#divPresupuestoDatos').find('input:not([type=hidden]):not([readonly]), textarea:not([readonly]), select:not([disabled])').filter(':visible').first();
        if ($primer.length) {
            setTimeout(() => $primer.trigger("focus"), 50);
        }

        console.log('✅ Modo Modificación Presupuesto activado');
    });

    //busqueda no gen de proveedores
    $(document).off("keydown.autocomplete").on("keydown.autocomplete", "input#cta_denominacion", function () {
        $(this).autocomplete({
            source: function (request, response) {
                data = { prefix: request.term }
                $.ajax({
                    url: autoComRel01Url,
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
}

// ✅ NUEVA: Capturar estado del formulario para poder restaurarlo
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

// ✅ NUEVA: Restaurar estado del formulario
function restaurarEstadoFormularioPresup(estado) {
    if (!estado) return;
    $.each(estado, function(nombre, valor) {
        const $campo = $(`[name="${nombre}"], #${nombre}`);
        if ($campo.length) {
            $campo.val(valor);
        }
    });
}

// ✅ NUEVA: Habilitar/Deshabilitar campos del formulario según modo edición
function habilitarCamposFormularioPresup(habilitar) {
    // Campos que NUNCA se editan
    const camposNoEditables = [
        'pre_id',           // ID del presupuesto
        'pret_id',          // Tipo (combo)
        'pree_id',          // Estado (combo)
        'usu_id',           // Usuario ID
        'usu_apellidoynombre', // Usuario nombre
        'adm_id',           // Administración ID
        'adm_nombre',       // Administración nombre
        'tco_id',           // Tipo de comprobante
        'cm_compte'         // Número de comprobante
    ];

    $('#divPresupuestoDatos').find('input:not([type=hidden]), textarea, select').each(function() {
        const $campo = $(this);
        const nombre = $campo.attr('name') || $campo.attr('id');
        
        // Verificar si el campo está en la lista de no editables
        const esNoEditable = camposNoEditables.some(campo => 
            nombre === campo || nombre?.includes(campo)
        );

        if (esNoEditable) {
            // Estos campos siempre readonly/disabled
            $campo.prop('readonly', true).prop('disabled', true).addClass('campo-readonly');
        } else if (habilitar) {
            // Habilitar campo para edición
            if ($campo.is('select')) {
                $campo.prop('disabled', false).removeClass('campo-readonly');
            } else {
                $campo.prop('readonly', false).removeClass('campo-readonly');
            }
        } else {
            // Deshabilitar campo
            if ($campo.is('select')) {
                $campo.prop('disabled', true).addClass('campo-readonly');
            } else {
                $campo.prop('readonly', true).addClass('campo-readonly');
            }
        }
    });

    console.log(`✅ Campos del formulario ${habilitar ? 'habilitados' : 'deshabilitados'} para edición`);
}

// ✅ NUEVA: Activar edición de campo
function activarEdicionCampoPresup($campo) {
    // Solo permitir edición si estamos en modo edición (nuevo o modificación)
    if (!estaEnModoEdicionPresup()) return;
    if (campoEnEdicionPresup !== null) return; // Ya hay edición activa

    campoEnEdicionPresup = $campo[0];
    $campo.prop('readonly', false)
          .removeClass('campo-readonly')
          .focus()
          .select();
}

// ✅ NUEVA: Guardar campo y avanzar al siguiente
function guardarYAvanzarCampoPresup($campo) {
    guardarCampoPresup($campo);
    
    const $fila = $campo.closest('tr');
    const esMargen = $campo.hasClass('input-pre_margen');
    
    // Si es margen, ir a precio venta en misma fila
    if (esMargen) {
        const $siguiente = $fila.find('.input-pre_pvta');
        if ($siguiente.length) {
            setTimeout(() => activarEdicionCampoPresup($siguiente), 50);
            return;
        }
    }
    
    // Si es precio venta, ir a margen de siguiente fila
    const $siguienteFila = $fila.next('tr');
    if ($siguienteFila.length) {
        const $siguiente = $siguienteFila.find('.input-pre_margen');
        if ($siguiente.length) {
            setTimeout(() => activarEdicionCampoPresup($siguiente), 50);
        }
    }
}

// ✅ NUEVA: Guardar cambios en campo
function guardarCampoPresup($campo) {
    if ($campo.prop('readonly')) return;

    const $fila = $campo.closest('tr');
    const pId = $fila.data('p-id');
    const esMargen = $campo.hasClass('input-pre_margen');
    
    const valorOriginal = parseFloat($campo.data('original-value')) || 0;
    const valorNuevo = parseFloat($campo.val().replace(/,/g, '')) || 0;
    
    // Formatear valor
    $campo.val(valorNuevo.toFixed(2));
    $campo.prop('readonly', true).addClass('campo-readonly');
    
    campoEnEdicionPresup = null;
    
    // Si cambió el valor, recalcular
    if (Math.abs(valorOriginal - valorNuevo) > 0.01) {
        if (esMargen) {
            recalcularPrecioDesdeMargen($fila, valorNuevo);
        } else {
            recalcularMargenDesdePrecio($fila, valorNuevo);
        }
        marcarCampoModificadoPresup($campo);
    }
}

// ✅ NUEVA: Recalcular total desde cambio de cantidad
function recalcularTotalDesdeCantidad($campo) {
    if ($campo.prop('readonly')) return;

    const $fila = $campo.closest('tr');
    const cantidadOriginal = parseFloat($campo.data('original-value')) || 0;
    const cantidadNueva = parseFloat($campo.val().replace(/,/g, '')) || 0;

    // Validar cantidad positiva
    if (cantidadNueva <= 0) {
        $campo.val(cantidadOriginal.toFixed(2));
        AbrirMensaje("Advertencia",
            "La cantidad debe ser mayor a 0",
            () => $("#msjModal").modal("hide"),
            false, ["Aceptar"], "warn!", null);
        return;
    }

    // Formatear y guardar
    $campo.val(cantidadNueva.toFixed(2));
    $campo.data('original-value', cantidadNueva);
    $campo.prop('readonly', true).addClass('campo-readonly');
    campoEnEdicionPresup = null;

    // Recalcular total si cambió la cantidad
    if (Math.abs(cantidadOriginal - cantidadNueva) > 0.01) {
        const precioVenta = parseFloat($fila.find('.input-pre_pvta').val().replace(/,/g, '')) || 0;
        const nuevoTotal = precioVenta * cantidadNueva;
        
        actualizarTotalFila($fila, nuevoTotal);
        actualizarTotalGeneralPresup();
        marcarCampoModificadoPresup($campo);
        
        console.log(`✅ Cantidad ${cantidadNueva} → Total ${nuevoTotal.toFixed(2)}`);
    }
}

// ✅ NUEVA: Marcar campo como modificado visualmente
function marcarCampoModificadoPresup($campo) {
    if (!$campo || !$campo.length) return;
    
    // Añadir clase temporal para feedback visual
    $campo.addClass('campo-modificado');
    
    // Remover clase después de animación
    setTimeout(() => {
        $campo.removeClass('campo-modificado');
    }, 1500);
}

// ✅ NUEVA: Recalcular precio desde margen
function recalcularPrecioDesdeMargen($fila, nuevoMargen) {
    const preCosto = parseFloat($fila.data('pre-costo')) || 0;
    const preCantidad = parseFloat($fila.find('.input-pre_cantidad').val().replace(/,/g, '')) || 1;
    
    // Calcular precio neto: costo * (1 + margen/100)
    const preNeto = preCosto * (1 + nuevoMargen / 100);
    
    // Obtener impuestos
    const ivaSituacion = $fila.data('iva-situacion') || 'E';
    const ivaAlicuota = parseFloat($fila.data('iva-alicuota')) || 0;
    const inAlicuota = parseFloat($fila.data('in-alicuota')) || 0;
    
    // Calcular precio venta final
    let precioVenta = preNeto;
    
    if (ivaSituacion === 'G') {
        precioVenta = preNeto * (1 + ivaAlicuota / 100);
    }
    if (inAlicuota > 0) {
        precioVenta = precioVenta * (1 + inAlicuota / 100);
    }
    
    // ✅ OPTIMIZADO: Calcular total en un solo paso
    const total = precioVenta * preCantidad;
    
    // Actualizar campos
    const $campoPVta = $fila.find('.input-pre_pvta');
    $campoPVta.val(precioVenta.toFixed(3));
    $campoPVta.data('original-value', precioVenta);
    marcarCampoModificadoPresup($campoPVta);
    
    // ✅ OPTIMIZADO: Actualizar total en una sola operación
    actualizarTotalFila($fila, total);
    
    // Actualizar total general
    actualizarTotalGeneralPresup();
    
    console.log(`✅ Margen ${nuevoMargen}% → Precio venta ${precioVenta.toFixed(3)} → Total ${total.toFixed(2)}`);
}

// ✅ NUEVA: Recalcular margen desde precio
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
    
    // Obtener impuestos para calcular precio neto
    const ivaSituacion = $fila.data('iva-situacion') || 'E';
    const ivaAlicuota = parseFloat($fila.data('iva-alicuota')) || 0;
    const inAlicuota = parseFloat($fila.data('in-alicuota')) || 0;
    
    // Calcular precio neto desde precio venta
    let preNeto = nuevoPrecio;
    if (inAlicuota > 0) {
        preNeto = preNeto / (1 + inAlicuota / 100);
    }
    if (ivaSituacion === 'G') {
        preNeto = preNeto / (1 + ivaAlicuota / 100);
    }
    
    // Calcular margen: ((preNeto - costo) / costo) * 100
    const nuevoMargen = ((preNeto - preCosto) / preCosto) * 100;
    
    // ✅ VALIDACIÓN: Margen no puede ser negativo
    if (nuevoMargen < 0) {
        const $campoPVta = $fila.find('.input-pre_pvta');
        const valorOriginal = parseFloat($campoPVta.data('original-value')) || 0;
        $campoPVta.val(valorOriginal.toFixed(3));
        
        AbrirMensaje("Advertencia", 
            `El precio de venta ingresado (${nuevoPrecio.toFixed(3)}) genera un margen negativo (${nuevoMargen.toFixed(2)}%).<br><br>` +
            `<strong>Costo:</strong> ${preCosto.toFixed(3)}<br>` +
            `El margen debe ser mayor o igual a 0%.`,
            () => $("#msjModal").modal("hide"),
            false, ["Aceptar"], "warn!", null);
        return;
    }
    
    // Actualizar margen
    const $campoMargen = $fila.find('.input-pre_margen');
    $campoMargen.val(nuevoMargen.toFixed(2));
    $campoMargen.data('original-value', nuevoMargen);
    marcarCampoModificadoPresup($campoMargen);
    
    // ✅ OPTIMIZADO: Calcular y actualizar total
    const total = nuevoPrecio * preCantidad;
    actualizarTotalFila($fila, total);
    
    // Actualizar total general
    actualizarTotalGeneralPresup();
    
    console.log(`✅ Precio ${nuevoPrecio.toFixed(3)} → Margen ${nuevoMargen.toFixed(2)}% → Total ${total.toFixed(2)}`);
}

// ✅ NUEVA: Función unificada para actualizar total de fila
function actualizarTotalFila($fila, total) {
    $fila.find('.td-total').text(total.toFixed(2));
}

// ✅ MEJORADA: Actualizar total general con performance optimizado
function actualizarTotalGeneralPresup() {
    let totalGeneral = 0;
    
    // ✅ OPTIMIZADO: Usar selector específico y evitar bucles innecesarios
    $('#tbGridPresupuestoProds tbody tr').each(function() {
        const $fila = $(this);
        // Saltar filas de separador o sin datos
        if ($fila.find('td[colspan]').length > 0) return;
        
        const total = parseFloat($fila.find('.td-total').text().replace(/,/g, '')) || 0;
        totalGeneral += total;
    });
    
    // ✅ OPTIMIZADO: Actualizar en una sola operación
    $('#tbGridPresupuestoProds tfoot .fw-bold:last').text(totalGeneral.toFixed(2));
    
    console.log(`📊 Total general actualizado: ${totalGeneral.toFixed(2)}`);
}

// -------------------------
// Búsqueda y render de Grid
// -------------------------
let _presuLoading = false;

async function buscarPresupuestos(btn,pag=1) {
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

            //armado de la paginacion
            PostGen({}, buscarMetadataURL, function (obj) {
                if (obj.error === true) {
                    AbrirMensaje("ATENCIÓN", obj.msg, function () {
                        $("#msjModal").modal("hide");
                        return true;
                    }, false, ["Aceptar"], "error!", null);
                }
                else {
                    totalRegs = obj.metadata.totalCount;
                    pags = obj.metadata.totalPages;
                    pagRegs = obj.metadata.pageSize;

                    $("#pagEstado").val(true).trigger("change");
                }
            });
        });
    } catch (e) {
        console.error("Error al buscar presupuestos:", e);
        $("#divDetalle")
            .html('<div class="alert alert-danger py-2 mb-0">No se pudo obtener la información. Intente nuevamente.</div>')
            .collapse("show");
    } finally {
        setBtnLoading($btn, false, originalHtml);
        _presuLoading = false;
    }
}

function configurarEventosSeleccionPres() {
    $(document).off("click", "#tbGridPresupuesto tbody tr");
    $(document).on("click", "#tbGridPresupuesto tbody tr", function (e) {
        //al hacer click sobre elementos que sean distintos a los enumerados
        //se marcará el registro
        if (!$(e.target).is("button, a, .btn, i")) {
            var $this = $(this);
            var fueSeleccionado = $this.hasClass("selected-row");

            //se quita la clase de seleccion
            $("#tbGridPresupuesto tbody tr").removeClass("selected-row");

            //seleccionadmos el registro
            if (!fueSeleccionado) {
                $this.addClass("selected-row");
                let preId = $this.data("pre-id");
                if (preId) {
                    //se cargará el detalle del presupuesto en el _presupuestoDatos
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

function cargarProductosPresupuesto(preId) {
    const url = obtenerPresupuestoProductoUrl;
    PostGenHtml({ pre_id: preId }, url, function(html) {
        $("#divPresProds").html(html).show();
        // Forzar estado readonly acorde al modo
        aplicarReadonlyCamposPresup();
    });
}

// -------------------------
// Helpers
// -------------------------
function buildQueryFilters(pag) {
    // Periodo
    const usaPeriodo = $("#chkDesdeHasta").is(":checked");
    const fechaD = usaPeriodo ? $("#Date1").val() : null;
    const fechaH = usaPeriodo ? $("#Date2").val() : null;

    // Filtros relacionales con tipos correctos según QueryFilters
    const rel01 = getValues("#Rel01List", false);         // List<string> - Clientes
    const rel02 = getValues("#Rel02List", false);         // List<string> - Usuarios (opcional)
    const rel03 = getValues("#Rel03List", true);          // List<ComboGenDto> - Estados
    
    // Rel04: Administraciones (List<ComboGenDto>)
    let rel04Val = $("#Rel04").val();
    let rel04 = [];
    if (rel04Val) {
        rel04.push({
            Id: $("#Rel04").val(),
            Descripcion: $("#Rel04 option:selected").text().trim()
        });        
    }

    const filters = {
        Registros: 200,
        Pagina: pag,
        FechaD: fechaD || null,
        FechaH: fechaH || null,
        Rel01: rel01.length ? rel01 : null,               // List<string>
        Rel02: rel02.length ? rel02 : null,               // List<string>
        Rel03: rel03.length ? rel03 : null,               // List<ComboGenDto>
        Rel04: rel04                                      // List<ComboGenDto>
    };

    // Debug: verificar qué está llegando
    console.log("QueryFilters construido:", filters);
    console.log("Rel01 (Clientes):", rel01);
    console.log("Rel02 (Usuarios):", rel02);
    console.log("Rel03 (Estados):", rel03);
    console.log("Rel04 (Admins):", rel04);

    return filters;
}

/**
 * Devuelve valores del control según el tipo esperado por QueryFilters:
 * - Para Rel01/Rel02: string[] simple
 * - Para Rel03/Rel04: { Id, Descripcion }[] (ComboGenDto)
 * @param {string|jQuery} src - Selector o elemento jQuery
 * @param {boolean} asComboDto - Si true, devuelve objetos ComboGenDto; si false, devuelve string[]
 * @returns {Array} string[] o ComboGenDto[]
 */
function getValues(src, asComboDto = false) {
    const $el = typeof src === "string" ? $(src) : src;
    if (!$el || !$el.length) {
        console.warn(`getValues: No se encontró el elemento "${src}"`);
        return [];
    }

    let items = [];

    // 1) Si es un select, extraer TODOS los option
    if ($el.is("select")) {
        items = $el.find("option").map((_, o) => ({
            value: o.value,
            text: o.text || o.value
        })).get();
        
        console.log(`getValues(${src}): Encontrados ${items.length} options en select`);
    } else {
        // 2) Fallbacks para controles no-select
        const dataVals = $el.attr("data-values") ?? $el.data("values");
        if (Array.isArray(dataVals)) {
            items = dataVals.map(v => ({ value: String(v), text: String(v) }));
            console.log(`getValues(${src}): Usando data-values array, ${items.length} items`);
        } else if (typeof dataVals === "string" && dataVals.trim()) {
            items = dataVals.split(",").map(v => ({ value: v.trim(), text: v.trim() }));
            console.log(`getValues(${src}): Usando data-values CSV, ${items.length} items`);
        } else {
            // 3) Hijos con data-id/value
            items = $el.find("[data-id],[value]").map((_, n) => {
                const $n = $(n);
                const id = $n.attr("data-id") ?? $n.attr("value");
                const txt = $n.attr("data-text") ?? $n.text() ?? id;
                return { value: id, text: txt };
            }).get();
            console.log(`getValues(${src}): Usando hijos con data-id/value, ${items.length} items`);
        }
    }

    if (!items || items.length === 0) {
        console.warn(`getValues(${src}): No se encontraron valores`);
        return [];
    }

    // Normalizar: trim, filtrar vacíos, deduplicar por value
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
    
    console.log(`getValues(${src}, asComboDto=${asComboDto}): Devolviendo ${out.length} valores`, out);
    return out;
}

// Añade un option al select si no existe (sin disparar change para evitar recursión)
function appendIfMissingOption($select, value, text) {
    if (!$select || !$select.length) return;
    const exists = $select.find("option").filter(function () { return this.value === value; }).length > 0;
    if (!exists) {
        const opt = new Option(text || value, value, false, true);
        $select.append(opt);
    } else {
        $select.find("option").filter(function () { return this.value === value; }).prop("selected", true);
    }

    // Refrescar plugins sin disparar change
    if ($.fn.selectpicker && $select.hasClass("selectpicker")) {
        $select.selectpicker("refresh");
    }
    // No llamar a .trigger("change") aquí para evitar recursión
}

// Vacía el valor del combo SIN disparar eventos (evita recursión infinita)
function resetComboSilent($el) {
    if (!$el || !$el.length) return;
    
    // Desactivar eventos temporalmente
    const handlers = $._data($el[0], "events");
    $el.off("change");
    
    if ($.fn.selectpicker && $el.hasClass("selectpicker")) {
        $el.selectpicker("val", []);
    } else if ($el.data("select2")) {
        $el.val(null);
    } else {
        $el.val("");
    }
    
    // Restaurar eventos
    if (handlers && handlers.change) {
        handlers.change.forEach(h => $el.on("change", h.handler));
    }
}

// Vacía el valor visual del combo origen (soporta selectpicker/select2/plain) - CON eventos
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

// Escapa valores para selectores CSS cuando sea necesario
function CSSSafe(v) {
    if (window.CSS && typeof window.CSS.escape === "function") {
        return CSS.escape(v);
    }
    return String(v).replace(/([!"#$%&'()*+,.\/:;<=>?@\[\\\]^`{|}~])/g, "\\$1");
}

function setBtnLoading($btn, loading, originalHtml) {
    if (!$btn || !$btn.length) return;
    if (loading) {
        $btn.prop("disabled", true).html('<span class="spinner-border spinner-border-sm me-1" role="status" aria-hidden="true"></span>Buscando...');
    } else {
        $btn.prop("disabled", false).html(originalHtml ?? "Buscar");
    }
}

// En presup.js - Manejo de edición desde botonera principal

// Helper: determina si estamos en modo edición de presupuesto
function estaEnModoEdicionPresup() {
    return !!(modoNuevoPresup || modoModificacionPresup);
}

// Aplica o remueve readonly a los campos editables del grid según el modo de edición
function aplicarReadonlyCamposPresup() {
    const campos = $('.input-pre_margen, .input-pre_pvta, .input-pre_cantidad');
    const tooltipMsg = 'Active el modo edición (Editar) para modificar este campo';

    // Ejecutar en batch para minimizar reflows
    requestAnimationFrame(() => {
        if (!estaEnModoEdicionPresup()) {
            campos.each(function () {
                const $c = $(this);
                $c.prop('readonly', true).addClass('campo-readonly');
                // Añadir tooltip informativo sólo si no existe
                if (!$c.attr('title')) $c.attr('title', tooltipMsg);
            });
        } else {
            // En modo edición dejamos los campos en readonly por defecto;
            // la edición se activará mediante doble click que quita readonly.
            campos.each(function () {
                const $c = $(this);
                $c.prop('readonly', true).addClass('campo-readonly');
                // Indicar que ahora pueden editarse con doble click
                $c.attr('title', 'Doble click para editar (modo edición activo)');
            });
        }
    });
}

// Helper: crea HTML de grid de productos vacío (mismo formato que partial)
function crearGridPresupVacioHtml() {
    return `
    <div class="card h-100">
        <div class="card-header py-1 d-flex justify-content-between align-items-center">
            <h6 class="mb-0">Productos del Presupuesto</h6>
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

