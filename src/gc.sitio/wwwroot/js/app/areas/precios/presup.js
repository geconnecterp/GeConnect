var modoNuevoPresup = false;
var modoModificacionPresup = false;

// ✅ AGREGAR: Variable global para controlar edición
var campoEnEdicionPresup = null;
let procesandoCampo = false;

// ✅ NUEVA: Variable para guardar estado original del presupuesto
let _presupOriginal = null;

// Helpers de formato (ajusta la moneda si no es ARS)
const fmtCurrency = (v) =>
    new Intl.NumberFormat('es-AR', { style: 'currency', currency: 'ARS', maximumFractionDigits: 2 }).format(v ?? 0);

const fmtPercent = (v) => {
    // v puede venir como 0.354 o 35.4 -> normalizamos a fracción
    const frac = (Math.abs(v) > 1) ? (v / 100) : v;
    return new Intl.NumberFormat('es-AR', { style: 'percent', minimumFractionDigits: 2, maximumFractionDigits: 2 }).format(frac ?? 0);
};


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
    // ✅ Activar botón de nuevo presupuesto
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

    if ($("#divDetalle").is(":not(:visible)") && $("#divFiltro").is(":not(:visible)")) {
        $("#divFiltro").collapse("show");
    }

    // ✅ PASO 2: Vaciar y ocultar divs de datos y productos
    $("#divPresDatos, #divPresProds").empty().hide();

    // ✅ PASO 3: Determinar si hay fila seleccionada en el grid de búsqueda
    const $filaSeleccionada = $("#tbGridPresupuesto tbody tr.selected-row");
    const hayPresupuestoSeleccionado = $filaSeleccionada.length > 0;

    // ✅ PASO 4: Restaurar botones ABM según contexto
    if (hayPresupuestoSeleccionado) {
        // Si hay un presupuesto seleccionado, mantener habilitados Modificar y Eliminar
        const preeId = $filaSeleccionada.data('pree-id') || 'P';
        const estadosEditables = ['P'];
        const permite = estadosEditables.includes(preeId);

        $("#btnAbmModif").prop("disabled", !permite);
        $("#btnAbmElimi").prop("disabled", !permite);
        $("#btnAbmNuevo").prop("disabled", false);
    } else {
        // Si no hay selección, solo habilitar Nuevo
        $("#btnAbmNuevo").prop("disabled", false);
        $("#btnAbmModif, #btnAbmElimi").prop("disabled", true);
    }

    // ✅ PASO 5: Desactivar y ocultar botones de confirmación
    $("#btnAbmAceptar, #btnAbmCancelar").prop("disabled", true).hide();

    // ✅ PASO 6: Deshabilitar botón de agregar productos
    $("#btnAgregarCProducto").prop("disabled", true);

    // ✅ PASO 7: Limpiar clases de edición en el grid (mantener selección)
    $("#tbGridPresupuesto tbody tr").removeClass("selectedEdit-row");

    console.log('✅ Operación cancelada - Vista reinicializada');

    //// ✅ PASO 8: Redirección si es necesario
    //if (e && $(e.target).is("#btnAbmCancelar") && typeof homePresup !== 'undefined') {
    //    console.log('🔀 Redirigiendo a:', homePresup);
    //    window.location.href = homePresup;
    //}
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
    $("#Rel011List").off("dblclick.removeOption").on("dblclick.removeOption", "option", function (e) {
        e.stopPropagation();
        $(this).remove();
        const $list = $("#Rel011List");
        if ($.fn.selectpicker && $list.hasClass("selectpicker")) {
            $list.selectpicker("refresh");
        }
    });

    // ✅ NUEVO: Eliminar opción de Rel02List con doble click
    $("#Rel022List").off("dblclick.removeOption").on("dblclick.removeOption", "option", function (e) {
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
    $("#Rel03List").off("dblclick.removeOption").on("dblclick.removeOption", "option", function (e) {
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
            e.stopPropagation(); // Importante: evitar propagación

            if (procesandoCampo) {
                console.log('⚠️ Procesando campo anterior, espere...');
                return;
            }
            const $campo = $(this);
            const $fila = $campo.closest('tr');
            let tipo = '';
            if ($campo.hasClass('input-pre_cantidad')) {
                tipo = 'C';
                recalcularTotalDesdeCantidad($campo);
            } else {
                guardarYAvanzarCampoPresup($campo);
                //guardarCampoPresup($campo);
            }

            if ($campo.hasClass('input-pre_margen')) {
                tipo = 'M';
            }
            else if ($campo.hasClass('input-pre_pvta')) {
                tipo = 'V';
            }

            setTimeout(() => {
                calcularUtilidadMargen();
                seleccionaProximoCampo(tipo, $fila)
            }, 100);

        }
    });

    // Blur para guardar cambios
    $(document).on('blur', '.input-pre_cantidad, .input-pre_margen, .input-pre_pvta', function () {
        //const $campo = $(this);
        //if ($campo.hasClass('input-pre_cantidad')) {
        //    recalcularTotalDesdeCantidad($campo);
        //} else {
        //    guardarYAvanzarCampoPresup($campo);
        //    //guardarCampoPresup($campo);
        //}
        setTimeout(() => {
            calcularUtilidadMargen();
        }, 100);
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

            $("#pret_id").prop("disabled", false);
            $("#pree_id").prop("disabled", false);

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

        setTimeout(() => { actualizarTotalGeneralPresup(); }, 100);


        const $primer = $('#divPresupuestoDatos').find('input:not([type=hidden]):not([readonly]), textarea:not([readonly]), select:not([disabled])').filter(':visible').first();
        if ($primer.length) {
            setTimeout(() => $primer.trigger("focus"), 50);
        }

        console.log('✅ Modo Modificación Presupuesto activado');
    });

    // ============================================================================
    // ELIMINACIÓN DE PRESUPUESTO
    // ============================================================================

    $(document).on('click', '#btnAbmElimi', function (e) {
        e.preventDefault();
        if ($(this).prop('disabled')) return;

        const preId = $('#pre_id').val();
        if (!preId || preId.trim() === '') {
            ControlaMensajeWarning('Debe seleccionar un presupuesto para eliminar');
            return;
        }

        const preeId = $('#pree_id').val();
        const estadosEliminables = ['P'];

        if (!estadosEliminables.includes(preeId)) {
            const nombreEstado = preeId === 'F' ? 'facturado'
                : preeId === 'R' ? 'remitido'
                    : preeId === 'A' ? 'anulado'
                        : 'en este estado';

            ControlaMensajeError(
                `No se puede eliminar un presupuesto ${nombreEstado}. ` +
                `Solo los presupuestos en estado Pendiente pueden ser eliminados.`
            );
            return;
        }

        const ctaDenominacion = $('#cta_denominacion').val() || 'Sin cliente';
        const vigenciaDesde = $('#pre_vigencia_desde').val() || '';
        const vigenciaHasta = $('#pre_vigencia_hasta').val() || '';

        const mensajeConfirmacion = `
            <div class="text-start">
                <p class="mb-2"><strong>¿Está seguro que desea eliminar este presupuesto?</strong></p>
                <hr class="my-2">
                <p class="mb-1"><strong>ID:</strong> ${preId}</p>
                <p class="mb-1"><strong>Cliente:</strong> ${ctaDenominacion}</p>
                <p class="mb-1"><strong>Vigencia:</strong> ${vigenciaDesde} al ${vigenciaHasta}</p>
                <hr class="my-2">
                <p class="text-danger mb-0">
                    <i class="bx bx-error-circle me-1"></i>
                    <strong>Esta acción no se puede deshacer.</strong>
                </p>
            </div>
        `;

        AbrirMensaje(
            'ELIMINAR PRESUPUESTO',
            mensajeConfirmacion,
            function (resp) {
                if (resp === 'SI') {
                    eliminarPresupuesto();
                }
                $('#msjModal').modal('hide');
            },
            true,
            ['Eliminar', 'Cancelar'],
            'warn!',
            null
        );
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
                }

                setTimeout(() => $("#Rel011").val(""), 10);
                return false;
            },
            focus: function () {
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
    $('#divPresupuestoDatos').find('input, textarea, select').each(function () {
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
    $.each(estado, function (nombre, valor) {
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

    $('#divPresupuestoDatos').find('input:not([type=hidden]), textarea, select').each(function () {
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

// Helpers
//function parseDecimal(str) {
//    if (typeof str !== 'string') return Number(str);
//    str = str.trim();
//    // Soporta "7604.84" y también "7.604,84"
//    str = str.replace(/\./g, '').replace(',', '.');
//    return parseFloat(str);
//}

function calcularElTotaldelaFila(cantidad, precio, $fila) {
    // Calcular y escribir en la celda
    if (!isNaN(cantidad) && !isNaN(precio)) {
        const total = cantidad * precio;
        const $tdTotal = $fila.find('td.td-total');

        // Guardás el valor crudo y mostrás 2 decimales con punto
        $tdTotal.attr('data-value', total);
        $tdTotal.text(total.toFixed(2));
    }
    else {
        $tdTotal.text(parseFloat("0.00"));
    }
}
// Variable global para controlar el estado de procesamiento
function guardarYAvanzarCampoPresup($campo) {
    // Evitar procesamiento múltiple
    if (procesandoCampo) {
        console.log('⚠️ Ya hay un campo en proceso...');
        return;
    }

    procesandoCampo = true;
    console.log('🔄 Inicio proceso campo:', $campo.attr('class'));
    try {
        guardarCampoPresup($campo);

        const $fila = $campo.closest('tr');
        const esCantid = $campo.hasClass('input-pre_cantidad');
        const esMargen = $campo.hasClass('input-pre_margen');
        const esPVta = $campo.hasClass('input-pre_pvta');

        //obtengo los parametros comunes para invocar el recalculo de alguno de los
        //valores ya sea Margen o PVenta

        let val_pcosto = parseFloat($fila.data("pre-pcosto"));
        let val_prev_tot = parseFloat($fila.data("lp-prevision-tot"));
        let val_prev_pin = parseFloat($fila.data("lp-prevision-pin"));
        let val_iva_sit = $fila.data("iva-situacion");
        let val_iva_ali = parseFloat($fila.data("iva-alicuota"));
        let val_in_alic = parseFloat($fila.data("in-alicuota"));


        let cantidad = parseFloat($fila.find('input.input-pre_cantidad').val());
        //tenemos el control cantidad más global para luego de operar recalcule los totales.
        const $inCant = $fila.find('input.input-pre_cantidad');

        if (esCantid) {
            calcularElTotaldelaFila(cantidad, pvta, $fila);
            finalizarProceso($inCant);
            return;
        }
        if (esMargen) {
            //para calcular el precio de venta, primero debo verificar que el valor
            //cargado y el valor original sean distintos.
            const $c_mg = $fila.find('input.input-pre_margen');
            let margen = parseFloat($c_mg.val());
            let margen_or = parseFloat($c_mg.data("original-value"));
            if (margen !== margen_or) {

                //se modificó el precio de venta.
                //analizo si el precio de venta es menor al costo. 
                if (margen < 0) {
                    //de marcará el campo de venta
                    $c_mg.addClass('input-alerta-costo');
                }
                else {
                    $c_mg.removeClass('input-alerta-costo');
                }

                //invocar a la función para que me calcule el precio de venta y
                //lo tengo que resguardar en el input y en el data.
                let dataMg = {
                    tp_pcosto: val_pcosto,
                    lp_prevision_tot: val_prev_tot,
                    lp_prevision_pin: val_prev_pin,
                    tp_margen: margen,
                    iva_situacion: val_iva_sit,
                    iva_alicuota: val_iva_ali,
                    in_alicuota: val_in_alic
                };
                AbrirWaiting('Espere el calculo...');
                PostGen(dataMg, calcularPrecioVentaBaseUrl, function (resp) {

                    if (resp.error === true) {
                        AbrirMensaje("Algo no fue bien", resp.msg, function () {
                            $("#msjModal").modal("hide");
                        }, false, ["Aceptar"], "error!", null);
                        CerrarWaiting();
                        return;
                    }
                    else if (resp.warn === true) {
                        AbrirMensaje("Algo no fue bien", resp.msg, function () {
                            if (resp.auth === true) {
                                window.location.href = login;
                            } else {
                                $("#msjModal").modal("hide");
                            }
                        }, false, ["Aceptar"], "error!", null);
                        CerrarWaiting();

                        return;
                    }
                    CerrarWaiting();
                    //tenemos el valor calculado
                    let vta = resp.pvta.p_pvta;
                    //le asignamos el nuevo valor a PVTA
                    const $inPvta = $fila.find('input.input-pre_pvta');
                    $inPvta.val(vta.toFixed(2));
                    $inPvta.data("originalValue", vta)
                        .attr("data-original-value", vta)
                        .trigger("change");
                    //calculamos el total de la fila.
                    calcularElTotaldelaFila(cantidad, vta, $fila);
                    procesandoCampo = false;

                    finalizarProceso($inCant);
                });
            } else {
                finalizarProceso($inCant);
            }
            return;
        }

        if (esPVta) {
            const $c_pvta = $fila.find('input.input-pre_pvta');
            let pvta = parseFloat($c_pvta.val());
            let pvta_or = parseFloat($c_pvta.data("original-value"));

            if (pvta !== pvta_or) {
                //se modificó el precio de venta.
                //analizo si el precio de venta es menor al costo. 
                // 1. Primera validación: precio vs costo
                const precioEsValido = pvta >= val_pcosto;

                // 2. Actualizar UI según validación
                if (!precioEsValido) {
                    $c_pvta.addClass('input-alerta-costo');
                    //continua pues puede, por estrategias de negocio
                    //vender a un precio de venta menor al costo.
                } else {
                    $c_pvta.removeClass('input-alerta-costo');
                }

                //debo armar el data para enviar via post
                let dataVT = {
                    tp_pcosto: val_pcosto,
                    lp_prevision_tot: val_prev_tot,
                    lp_prevision_pin: val_prev_pin,
                    tp_pvta: pvta,
                    iva_situacion: val_iva_sit,
                    iva_alicuota: val_iva_ali,
                    in_alicuota: val_in_alic
                };

                AbrirWaiting('Espere el calculo...');
                PostGen(dataVT, calcularPrecioVentaMargenUrl, function (resp) {
                    if (resp.error === true) {
                        AbrirMensaje("Algo no fue bien", resp.msg, function () {
                            $("#msjModal").modal("hide");
                        }, false, ["Aceptar"], "error!", null);
                        CerrarWaiting();
                        return; // Importante: salir si hay error
                    }
                    else if (resp.warn === true) {
                        AbrirMensaje("Algo no fue bien", resp.msg, function () {
                            if (resp.auth === true) {
                                window.location.href = login;
                            } else {
                                $("#msjModal").modal("hide");
                                CerrarWaiting();
                            }
                        }, false, ["Aceptar"], "error!", null);
                        return; // Importante: salir si hay warning
                    }
                    CerrarWaiting();

                    // 4. Actualizar margen solo si el cálculo fue exitoso
                    if (resp.pvta && typeof resp.pvta.p_margen !== 'undefined') {
                        const mg = resp.pvta.p_margen;
                        const $inMg = $fila.find('input.input-pre_margen');

                        // 5. Actualizar UI del margen
                        $inMg.val(mg.toFixed(2));
                        $inMg.data("original-value", mg.toFixed(2))
                            .attr("data-original-value", mg.toFixed(2))
                            .trigger("change");

                        // 6. Actualizar estado visual del margen
                        if (mg < 0) {
                            $inMg.addClass("input-alerta-costo");
                        } else {
                            $inMg.removeClass("input-alerta-costo");
                        }

                        // 7. Actualizar precio original
                        $c_pvta.data("original-value", pvta.toFixed(2))
                            .attr("data-original-value", pvta.toFixed(2));

                       
                        finalizarProceso($inCant);
                    }

                });
            } else {
                finalizarProceso($inCant);
            }
            return;
        }

        finalizarProceso($inCant);

    } catch (error) {
        console.error('Error en guardarYAvanzarCampoPresup:', error);
        procesandoCampo = false;
        CerrarWaiting();
    }
}

// Funciones auxiliares para mantener el código organizado
function finalizarProceso($inCant) {
    recalcularTotalDesdeCantidad($inCant);
    procesandoCampo = false;
    CerrarWaiting();
    console.log('✅ Fin proceso campo');
}

function actualizarUIValidacionPrecio($campo, esValido) {
    if (!esValido) {
        $campo.addClass('input-alerta-costo');
    } else {
        $campo.removeClass('input-alerta-costo');
    }
}

function seleccionaProximoCampo(campo, $fila) {
    switch (campo) {
        case 'C':
            //el proximo es margen
            const $c2 = $fila.find('.input-pre_margen');
            $c2.trigger("focus");
            //seleccionamos 
            setTimeout(() => $c2.select(), 0);
            break;
        case 'M':
            //el proximo es venta
            const $c3 = $fila.find('.input-pre_pvta');
            $c3.trigger("focus");
            //seleccionamos 
            setTimeout(() => $c3.select(), 0);
            break;
        case 'V':
            //el proximo es cantidad en la fila siguiente
            const $siguienteFila = $fila.next('tr');
            if ($siguienteFila.length) {
                const $c1 = $siguienteFila.find('.input-pre_cantidad');
                if ($c1.length) {
                    $c1.trigger("focus");
                    //seleccionamos 
                    setTimeout(() => $c1.select(), 0);
                }
            }
            break;
        default:
            return false;
    }
}

function guardarCampoPresup($campo) {
    if ($campo.prop('readonly')) return;

    const $fila = $campo.closest('tr');
    const esMargen = $campo.hasClass('input-pre_margen');

    const valorOriginal = parseFloat($campo.data('original-value')) || 0;
    const valorNuevo = parseFloat($campo.val().replace(/,/g, '')) || 0;

    $campo.val(valorNuevo.toFixed(2));
    //$campo.prop('readonly', true).addClass('campo-readonly');

    //campoEnEdicionPresup = null;

    //if (Math.abs(valorOriginal - valorNuevo) > 0.01) {
    //    if (esMargen) {
    //        recalcularPrecioDesdeMargen($fila, valorNuevo);
    //    } else {
    //        recalcularMargenDesdePrecio($fila, valorNuevo);
    //    }
    //    marcarCampoModificadoPresup($campo);
    //}
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
    //$campo.prop('readonly', true).addClass('campo-readonly');
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

    $('#tbGridPresupuestoProds tbody tr').each(function () {
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
    //configurando los eventos para el boton que elimina el registro.
    configurarEventosEliminacionProducto();
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
        $("#btnAbmElimi").prop("disabled", !permite);

        // Debug - ayuda a identificar estados del sistema
        console.log("cargarPresupuestoDatos: Estado del presupuesto:", preeId,
            "Permite edición:", permite);
    });
}

function cargarProductosPresupuesto(preId, isUpdate = false) {
    let url = "";
    if (isUpdate) {
        //trae los productos con los costos actualizados
        url = obtenerPresupuestoProductoActualizadoUrl;
    }
    else {
        //trae los productos tal cual están en el presupuesto
        url = obtenerPresupuestoProductoUrl;
    }

    PostGenHtml({ pre_id: preId }, url, function (html) {
        $("#divPresProds").empty().html(html).show();
        // Forzar estado readonly acorde al modo
        aplicarReadonlyCamposPresup();

        setTimeout(() => {
            finalizarInicializacion();
            calcularUtilidadMargen();
        }, 100);
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
                //rescatamos la fila
                const $fila = $c.closest('tr');
                $c.prop('readonly', false).removeClass('campo-readonly');
                if ($c.hasClass('input-pre_margen')) {
                    //rescato el valor actual
                    const valor = parseFloat($c.data("margen-actual")).toFixed(2);
                    $c.val(valor);
                }
                else if ($c.hasClass('input-pre_pvta')) {
                    const valor = parseFloat($c.data("pvta-actual"));
                    $c.val(valor.toFixed(2));

                    //rescato la cantidad para recalcular el total
                    const cant = parseFloat($fila.find('input.input-pre_cantidad').val());
                    const vCosto = parseFloat($fila.data('p-pcosto-actual'));
                    // ✅ Asignar el costo actualizado a la cuarta celda (índice 3)
                    $fila.find('td:eq(3)').text(vCosto.toFixed(3));

                    //presentamos el boton de elimnación
                    $fila.find('td:last-child button.btn-eliminar-producto')
                        .show().prop('disabled', false)
                        .removeClass('d-none')
                        .removeAttr('style');
                    //multiplicamos la cantidad por el precio de venta.        
                    calcularElTotaldelaFila(cant, valor, $fila);
                }
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
             <!-- Centro (métricas globales) -->
            <div class="flex-grow-1 text-center">
                <span class="fw-bold me-2">Valor de Utilidad Total:</span>
                <span id="spUtilidadTotal" class="text-danger me-4">-</span>

                <span class="fw-bold me-2">Margen Total:</span>
                <span id="spMargenTotal" class="text-danger">-</span>
            </div>

            <button type="button" class="btn btn-sm btn-outline-primary" id="btnAgregarCProducto" title="Agregar Producto" disabled>
                <i class="bx bx-plus"></i>
            </button>
        </div>
        <div class="card-body p-1">
            <div class="table-responsive" style="max-height: 400px;">
                <table class="table table-sm table-hover mb-0 table-golden" id="tbGridPresupuestoProds">
                    <thead class="table-golden-header">
                        <tr class="header">
                            <th class="text-center">#</th>
                            <th class="text-center">Código</th>
                            <th class="text-left">Descripción</th>
                            <th class="text-end">Costo</th>
                            <th class="text-end">Cantidad</th>
                            <th class="text-end">Mg %</th>
                            <th class="text-end">Venta</th>
                            <th class="text-end">Total</th>
                            <th class="text-end">Accion</th>
                        </tr>
                    </thead>
                    <tbody>
                        <tr>
                            <td colspan="8" class="text-center text-muted py-2">
                                <i class="bx bx-info-circle me-1"></i>No hay productos en este presupuesto
                            </td>
                            <td></td>
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
        success: function (html) {
            if ($("#busquedaModal").length === 0) {
                $('body').append(html);
            }
            if (typeof callback === 'function') {
                callback();
            }
        },
        error: function (xhr, status, error) {
            console.error("Error al cargar modal de búsqueda:", error);
            ControlaMensajeError("No se pudo cargar el módulo de búsqueda de productos");
        }
    });
}

function obtenerProductosExistentesIds() {
    const productosIds = [];

    $('#tbGridPresupuestoProds tbody tr').each(function () {
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
                    <td colspan="7" class="text-end fw-bold">Total General:</td>
                    <td class="text-end fw-bold">0.00</td>
                </tr>
            </tfoot>
        `);
        $tfoot = $('#tbGridPresupuestoProds tfoot');
    }

    let esAlternado = $tbody.find('tr').length % 2 !== 0;

    productos.forEach(function (producto, index) {
        const fila = crearFilaProductoPresupuesto(producto, esAlternado, index + 1);
        $tbody.append(fila);
        esAlternado = !esAlternado;
    });

    //aplicarInputMaskPresupuesto();
    aplicarReadonlyCamposPresup();
    actualizarTotalGeneralPresup();
    configurarEventosEliminacionProducto();
    setTimeout(() => {
        finalizarInicializacion();
        calcularUtilidadMargen();
    }, 100);
}

/**
 * ✅ OPTIMIZADO: Crea HTML de fila de producto con TODOS los nuevos campos
 * Unifica lógica de cálculo y evita duplicación de código
 * @param {object} producto - ProductoListaDto
 * @param {boolean} esAlternado - Alternar clase CSS
 * @returns {string} HTML de la fila
 */
function crearFilaProductoPresupuesto(producto, esAlternado, pre_item) {
    // ✅ VALIDACIÓN Y NORMALIZACIÓN DE DATOS
    const datosProducto = normalizarDatosProducto(producto);

    // ✅ FORMATEO
    const claseAlt = esAlternado ? 'alt' : '';

    // ✅ CONSTRUCCIÓN HTML CON TEMPLATE LITERALS (más legible y performante)
    return `
        <tr class="${claseAlt}"
            data-p-id="${datosProducto.p_id}"
            data-pre-pcosto="${datosProducto.p_pcosto.toFixed(3)}"
            data-pre-pneto="${datosProducto.p_pneto.toFixed(2)}"
            data-p-pcosto-actual="${datosProducto.p_pcosto.toFixed(3)}"
            data-iva-situacion="${datosProducto.iva_situacion}"
            data-iva-alicuota="${datosProducto.iva_alicuota}"
            data-in-alicuota="${datosProducto.in_alicuota}"
            data-lp-prevision-tot="${datosProducto.lp_prevision_tot.toFixed(3)}"
            data-lp-prevision-pin="${datosProducto.lp_prevision_pin.toFixed(3)}">
            <td class="text-center" data-pre_item="${pre_item}">${pre_item}</td>
            <td class="text-center">${datosProducto.p_id}</td>
            <td>${escaparHTML(datosProducto.p_desc)}</td>
            <td class="text-end">${datosProducto.p_pcosto.toFixed(3)}</td>
            <td class="text-end">
                <div class="input-container">
                    <input type="text" 
                           class="form-control form-control-sm input-pre_cantidad input-numeric "
                           value="${datosProducto.cantidad.toFixed(2)}"
                           data-original-value="${datosProducto.cantidad}"
                           title="Doble click para editar" />
                </div>
            </td>
            <td class="text-end">
                <div class="input-container">
                    <input type="text" 
                           class="form-control form-control-sm input-pre_margen input-numeric"
                           value="${datosProducto.p_margen.toFixed(2)}"
                           data-original-value="${datosProducto.p_margen}"
                           data-margen-actual="${datosProducto.p_margen.toFixed(2)}"                            
                           title="Doble click para editar" />
                </div>
            </td>
            <td class="text-end">
                <div class="input-container">
                    <input type="text" 
                           class="form-control form-control-sm input-pre_pvta input-numeric"
                           value="${datosProducto.p_pvta.toFixed(2)}"
                           data-original-value="${datosProducto.p_pvta}"
                           data-pvta-actual="${datosProducto.p_pvta.toFixed(2)}"                           
                           title="Doble click para editar" />
                </div>
            </td>
            <td class="text-end td-total">${datosProducto.p_pvta.toFixed(2)}</td>
            <td class="text-center">
                <button type="button" 
                        class="btn btn-sm btn-danger btn-eliminar-producto" 
                        data-p-id="${datosProducto.p_id}"
                        title="Eliminar producto"
                        style="${estaEnModoEdicionPresup() ? '' : 'display: none;'}">
                    <i class="bx bx-trash"></i>
                </button>
            </td>
        </tr>
    `;
}

/**
 * ✅ NUEVO: Normaliza y valida datos del ProductoListaDto
 * Centraliza validación y conversión de tipos
 * @param {object} producto - ProductoListaDto
 * @returns {object} Datos normalizados y validados
 */
function normalizarDatosProducto(producto) {
    // ✅ HELPER: Parsear decimal con fallback seguro
    const parseDecimalSeguro = (valor, defecto = 0) => {
        const num = parseFloat(valor);
        return isNaN(num) ? defecto : num;
    };

    return {
        // Identificadores
        p_id: String(producto.p_id || producto.P_id || '').trim(),
        p_desc: String(producto.p_desc || producto.P_desc || 'Sin descripción').trim(),
        // Precios y costos
        p_pcosto: parseDecimalSeguro(producto.p_pcosto || producto.P_pcosto, 0),
        p_pvta: parseDecimalSeguro(producto.p_pvta || producto.P_pvta, 0),
        p_pneto: parseDecimalSeguro(producto.p_pneto, 0), // ✅ NUEVO CAMPO

        // Márgenes
        p_margen: parseDecimalSeguro(producto.p_margen, 0), // ✅ USA p_margen DEL DTO
        //margenActual: parseDecimalSeguro(producto.p_margen, 0), // ✅ NUEVO CAMPO

        // Cantidad (siempre 1 para nuevos productos)
        cantidad: 1,

        // Impuestos
        //ivaSituacion: String(producto.iva_situacion || 'E').trim(),
        iva_situacion: producto.iva_situacion,
        iva_alicuota: parseDecimalSeguro(producto.iva_alicuota, 21),
        in_alicuota: parseDecimalSeguro(producto.in_alicuota, 0),

        // ✅ NUEVOS CAMPOS: Previsiones
        lp_prevision_tot: parseDecimalSeguro(producto.lp_prevision_tot, 0),
        lp_prevision_pin: parseDecimalSeguro(producto.lp_prevision_pin, 0),
    };
}

/**
 * ✅ NUEVO: Escapa HTML para prevenir XSS
 * @param {string} texto - Texto a escapar
 * @returns {string} Texto escapado
 */
function escaparHTML(texto) {
    const div = document.createElement('div');
    div.textContent = texto;
    return div.innerHTML;
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
            }

            setTimeout(() => $("#Rel011").val(""), 10);
            return false;
        },
        focus: function () {
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
                error: function () {
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
        focus: function () {
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

// Handler para Aceptar/Confirmar Presupuesto
$(document).on('click', '#btnAbmAceptar', function (e) {
    e.preventDefault();

    if ($(this).prop('disabled')) return;

    // Determinar modo ABM
    let abm = '';
    if (modoNuevoPresup) {
        abm = 'A'; // Alta
    } else if (modoModificacionPresup) {
        abm = 'M'; // Modificación
    } else {
        console.error('⚠️ Modo de operación no determinado');
        ControlaMensajeError('No se puede determinar la operación a realizar');
        return;
    }

    // Validar antes de confirmar
    const validacion = validarPresupuesto(abm);
    if (!validacion.esValido) {
        ControlaMensajeWarning(validacion.mensaje);
        return;
    }

    // Mostrar confirmación
    const mensajeConfirmacion = abm === 'A'
        ? '¿Desea confirmar la creación del presupuesto?'
        : '¿Desea confirmar las modificaciones del presupuesto?';

    AbrirMensaje(
        'CONFIRMAR PRESUPUESTO',
        mensajeConfirmacion,
        function (resp) {
            if (resp === 'SI') {
                confirmarPresupuesto(abm);
            }
            $('#msjModal').modal('hide');
        },
        true,
        ['Confirmar', 'Cancelar'],
        'info!',
        null
    );
});

// ============================================================================
// FUNCIONES DE VALIDACIÓN Y CONFIRMACIÓN DE PRESUPUESTO
// ============================================================================

/**
 * ✅ Valida los datos del presupuesto antes de confirmar
 * @param {string} abm - Tipo de operación: 'A', 'M', 'B'
 * @returns {object} { esValido: boolean, mensaje: string }
 */
function validarPresupuesto(abm) {
    console.log(`🔍 Validando presupuesto (Modo: ${abm})...`);

    // ✅ VALIDACIÓN 1: Cliente obligatorio
    const ctaId = $('#cta_id').val();
    if (!ctaId || ctaId.trim() === '') {
        let nombre = $("#pre_nombre").val();
        let domicilio = $("#pre_domicilio").val();

        if (!nombre || nombre.trim() === '') {
            return {
                esValido: false,
                mensaje: 'Debe seleccionar un cliente para el presupuesto o por lo menos carar el nombre y domicilio en el formulario.'
            };
        } else if (!domicilio || domicilio.trim() === '') {
            return {
                esValido: false,
                mensaje: 'No se especificó cliente, se indicó un nombre pero falta especificar domicilio de la persona solicitante del presupuesto.'
            }
        }


    }

    // ✅ VALIDACIÓN 2: Tipo obligatorio
    const pretId = $('#pret_id').val();
    if (!pretId || pretId.trim() === '') {
        return {
            esValido: false,
            mensaje: 'Debe seleccionar el tipo de presupuesto'
        };
    }

    // ✅ VALIDACIÓN 3: Vigencia desde obligatorio
    const vigenciaDesde = $('#pre_vigencia_desde').val();
    if (!vigenciaDesde || vigenciaDesde.trim() === '') {
        return {
            esValido: false,
            mensaje: 'Debe ingresar la fecha de vigencia desde'
        };
    }

    // ✅ VALIDACIÓN 4: Vigencia hasta obligatorio
    const vigenciaHasta = $('#pre_vigencia_hasta').val();
    if (!vigenciaHasta || vigenciaHasta.trim() === '') {
        return {
            esValido: false,
            mensaje: 'Debe ingresar la fecha de vigencia hasta'
        };
    }

    // ✅ VALIDACIÓN 5: Vigencia desde <= hasta
    if (new Date(vigenciaDesde) > new Date(vigenciaHasta)) {
        return {
            esValido: false,
            mensaje: 'La fecha "Vigencia Desde" no puede ser posterior a "Vigencia Hasta"'
        };
    }

    // ✅ VALIDACIÓN 6: Debe haber al menos un producto
    const productos = obtenerProductosDelGrid();
    if (productos.length === 0) {
        return {
            esValido: false,
            mensaje: 'Debe agregar al menos un producto al presupuesto'
        };
    }

    // ✅ VALIDACIÓN 7: Todos los productos deben tener cantidad > 0
    const productosConCantidadInvalida = productos.filter(p => p.pre_cantidad <= 0);
    if (productosConCantidadInvalida.length > 0) {
        return {
            esValido: false,
            mensaje: 'Todos los productos deben tener una cantidad mayor a 0'
        };
    }

    console.log('✅ Validación exitosa');
    return { esValido: true, mensaje: '' };
}

/**
 * ✅ Confirma el presupuesto enviándolo al servidor
 * @param {string} abm - Tipo de operación: 'A', 'M', 'B'
 */
function confirmarPresupuesto(abm) {
    console.log(`📤 Confirmando presupuesto (Modo: ${abm})...`);

    AbrirWaiting('Confirmando presupuesto...');

    try {
        // Construir objeto de confirmación
        const confirmacionDto = construirPresupuestoConfirmaReqDto(abm);

        // Debug: Ver estructura completa
        console.log('📦 DTO de confirmación:', confirmacionDto);

        // Enviar al servidor
        PostGen(
            confirmacionDto,
            confirmarPresupuestoUrl,
            function (response) {
                CerrarWaiting();
                procesarRespuestaConfirmacion(response, abm);
            },
            function (error) {
                CerrarWaiting();
                console.error('❌ Error al confirmar presupuesto:', error);
                ControlaMensajeError(
                    'Error al confirmar el presupuesto: ' +
                    (error.responseJSON?.mensaje || error.statusText || 'Error desconocido')
                );
            }
        );
    } catch (error) {
        CerrarWaiting();
        console.error('❌ Error al construir DTO:', error);
        ControlaMensajeError('Error al procesar los datos del presupuesto: ' + error.message);
    }
}

/**
 * ✅ Construye el DTO PresupuestoConfirmaReqDto
 * @param {string} abm - Tipo de operación
 * @returns {object} PresupuestoConfirmaReqDto
 */
function construirPresupuestoConfirmaReqDto(abm) {
    return {
        Abm: abm,
        Datos: obtenerDatosFormularioPresupuesto(),
        Productos: obtenerProductosDelGrid()
    };
}

/**
 * ✅ Obtiene los datos del formulario de presupuesto
 * @returns {object} PresupuestoDto
 */
function obtenerDatosFormularioPresupuesto() {
    const datos = {
        pre_id: $('#pre_id').val() || '',
        pret_id: $('#pret_id').val() || '',
        pree_id: $('#pree_id').val() || 'P', // Pendiente por defecto
        cta_id: $('#cta_id').val() || '',
        cta_denominacion: $('#cta_denominacion').val() || '',
        pre_nombre: $('#pre_nombre').val() || '',
        pre_domicilio: $('#pre_domicilio').val() || '',
        pre_vigencia_desde: $('#pre_vigencia_desde').val() || '',
        pre_vigencia_hasta: $('#pre_vigencia_hasta').val() || '',
        usu_id: $('#usu_id').val() || '',
        usu_apellidoynombre: $('#usu_apellidoynombre').val() || '',
        adm_id: $('#adm_id').val() || '',
        adm_nombre: $('#adm_nombre').val() || '',
        tco_id: $('#tco_id').val() || '',
        cm_compte: $('#cm_compte').val() || '',
        pre_obs_pago: $('#pre_obs_pago').val() || '',
        pre_obs_entrega: $('#pre_obs_entrega').val() || ''
    };

    console.log('📋 Datos del formulario capturados:', datos);
    return datos;
}

function calcularUtilidadMargen() {
    //busco la tabla y presento la variable con las filas
    const $filas = $('#tbGridPresupuestoProds tbody tr');
    let costoTotal = 0;
    let utilidadTotal = 0;
    let margenTotal = 0;
    $filas.each(function () {
        const $fila = $(this);
        if ($fila.find('td[colspan]').length > 0) return;

        //costo total = (p_pcosto * pre_cantidad)
        const preCosto = parseFloat($fila.data('pre-pcosto')) || 0;
        const cantidad = parseFloat($fila.find('.input-pre_cantidad').val().replace(/,/g, '')) || 0;
        costoTotal += preCosto * cantidad;
        //utilidad total = p_pcosto * pre_cantidad * (pre_margen / 100)
        const margen = parseFloat($fila.find('.input-pre_margen').val().replace(/,/g, '')) || 0;
        utilidadTotal += preCosto * cantidad * (margen / 100);
    });

    //margen total = (utilidad total / costo total) * 100
    if (costoTotal > 0) {
        margenTotal = (utilidadTotal / costoTotal) * 100;
    }

    $("#spUtilidadTotal").text(fmtCurrency(utilidadTotal));
    $("#spMargenTotal").text(fmtPercent(margenTotal));
}

/**
 * ✅ Obtiene los productos del grid
 * @returns {Array} Lista de PresupuestoProductoDto
 */
function obtenerProductosDelGrid() {
    const productos = [];
    const $filas = $('#tbGridPresupuestoProds tbody tr');

    $filas.each(function () {
        const $fila = $(this);

        // ✅ OPTIMIZACIÓN: Saltar filas vacías o de mensaje en una sola verificación
        if ($fila.find('td[colspan]').length > 0) return;

        // ✅ OPTIMIZACIÓN: Extraer datos del DOM usando data attributes (más eficiente)
        const pId = $fila.data('p-id');
        if (!pId) return; // Si no hay ID, saltar esta fila

        // ✅ OPTIMIZACIÓN: Parsear valores numéricos una sola vez
        const preCosto = parseFloat($fila.data('pre-pcosto')) || 0;
        const preNeto = parseFloat($fila.data('pre-pneto')) || 0;
        const ivaSituacion = $fila.data('iva-situacion') || 'E';
        const ivaAlicuota = parseFloat($fila.data('iva-alicuota')) || 0;
        const inAlicuota = parseFloat($fila.data('in-alicuota')) || 0;

        // ✅ OPTIMIZACIÓN: Buscar inputs una sola vez y cachear resultados
        const $inputCantidad = $fila.find('.input-pre_cantidad');
        const $inputMargen = $fila.find('.input-pre_margen');
        const $inputPVta = $fila.find('.input-pre_pvta');

        // ✅ OPTIMIZACIÓN: Extraer y parsear valores en una sola línea
        const cantidad = parseFloat($inputCantidad.val().replace(/,/g, '')) || 0;
        const margen = parseFloat($inputMargen.val().replace(/,/g, '')) || 0;
        const precioVenta = parseFloat($inputPVta.val().replace(/,/g, '')) || 0;

        // ✅ OPTIMIZACIÓN: Calcular total directamente
        const total = cantidad * precioVenta;

        // ✅ Construir objeto PresupuestoProductoDto (coincide exactamente con el DTO de C#)
        productos.push({
            // Propiedades de productos
            pre_item: parseInt($fila.find('td:nth-child(1)').data("pre_item")),
            p_id: pId,
            p_des: $fila.find('td:nth-child(3)').text().trim(),
            iva_situacion: ivaSituacion,
            iva_alicuota: ivaAlicuota,
            in_alicuota: inAlicuota,
            pre_cantidad: cantidad,
            pre_pcosto: preCosto,
            pre_pneto: preNeto,
            pre_pmargen: margen,
            pre_pvta: precioVenta,
            pre_cantidad_ent: 0, // ✅ Campo requerido por PresupuestoProductoDto
            pre_total: total,
            // ✅ Heredadas de PresupuestoDto (vacías para productos individuales)
            pre_id: '',
            pre_descripcion: '',
            pre_fecha: new Date().toISOString(),
            pre_nombre: '',
            pre_domicilio: '',
            pre_vigencia_desde: new Date().toISOString(),
            pre_vigencia_hasta: new Date().toISOString(),
            pre_obs_pago: '',
            pre_obs_entrega: '',
            pree_id: 'P',
            pree_desc: '',
            pret_id: 'P',
            pret_desc: '',
            cta_id: '',
            cta_denominacion: '',
            usu_id: '',
            usu_apellidoynombre: '',
            adm_id: '',
            adm_nombre: '',
            tco_id: '',
            cm_compte: ''
        });
    });

    console.log(`📦 ${productos.length} productos capturados del grid`);
    return productos;
}

/**
 * ✅ Procesa la respuesta del servidor después de confirmar
 * @param {object} response - Respuesta del servidor
 * @param {string} abm - Tipo de operación
 */
function procesarRespuestaConfirmacion(response, abm) {
    console.log('📥 Respuesta del servidor:', response);

    if (!response.ok) {
        if (response.error) {
            ControlaMensajeError(response.mensaje || 'Error al confirmar el presupuesto');
            return;
        }
        else //warn
        {
            ControlaMensajeWarning(response.mensaje || 'Atención al confirmar el presupuesto');
            return;
        }
    }

    // Mensaje de éxito según el tipo de operación
    let mensajeExito = '';
    switch (abm) {
        case 'A':
            mensajeExito = 'Presupuesto creado exitosamente';
            break;
        case 'M':
            mensajeExito = 'Presupuesto modificado exitosamente';
            break;
        case 'B':
            mensajeExito = 'Presupuesto eliminado exitosamente';
            break;
        default:
            mensajeExito = 'Operación completada exitosamente';
    }

    // Mostrar mensaje y redirigir
    AbrirMensaje(
        'CONFIRMACIÓN EXITOSA',
        mensajeExito,
        function () {
            $('#msjModal').modal('hide');

            // Resetear formulario y volver al inicio
            cancelarOperacion();

            // Si hay ID de presupuesto en la respuesta, refrescar el grid
            if (response.pre_id) {
                // Opcional: Recargar el presupuesto recién creado/modificado
                console.log('✅ Presupuesto ID:', response.pre_id);
            }
        },
        false,
        ['Aceptar'],
        'success!',
        null
    );
}

// ============================================================================
// FUNCIONES DE ELIMINACIÓN DE PRESUPUESTO
// ============================================================================

function eliminarPresupuesto() {
    console.log('🗑️ Eliminando presupuesto...');

    const preId = $('#pre_id').val();
    if (!preId || preId.trim() === '') {
        ControlaMensajeError('Error: No se encontró el ID del presupuesto a eliminar');
        return;
    }

    AbrirWaiting('Eliminando presupuesto...');

    try {
        const confirmacionDto = {
            Abm: 'B',
            Datos: obtenerDatosFormularioPresupuesto(),
            Productos: obtenerProductosDelGrid()
        };

        console.log('📦 DTO de eliminación:', confirmacionDto);

        PostGen(
            confirmacionDto,
            confirmarPresupuestoUrl,
            function (response) {
                CerrarWaiting();
                procesarRespuestaEliminacion(response);
            },
            function (error) {
                CerrarWaiting();
                console.error('❌ Error al eliminar presupuesto:', error);

                const mensajeError = error.responseJSON?.mensaje
                    || error.responseJSON?.msg
                    || error.statusText
                    || 'Error desconocido';

                ControlaMensajeError(`Error al eliminar el presupuesto: ${mensajeError}`);
            }
        );
    } catch (error) {
        CerrarWaiting();
        console.error('❌ Error al construir DTO:', error);
        ControlaMensajeError('Error al procesar la eliminación: ' + error.message);
    }
}

function procesarRespuestaEliminacion(response) {
    console.log('📥 Respuesta de eliminación:', response);

    if (!response.ok) {
        if (response.error) {
            ControlaMensajeError(response.mensaje || 'Error al eliminar el presupuesto');
            return;
        }
        else //warn
        {
            ControlaMensajeWarning(response.mensaje || 'Atención al intentar eliminar el presupuesto');
            return;
        }

    }

    AbrirMensaje(
        'ELIMINACIÓN EXITOSA',
        'El presupuesto ha sido eliminado correctamente',
        function () {
            $('#msjModal').modal('hide');
            cancelarOperacion();

            if ($('#tbGridPresupuesto tbody tr').length > 0) {
                console.log('🔄 Actualizando lista de presupuestos...');
                buscarPresupuestos($('#btnBuscar'));
            }
        },
        false,
        ['Aceptar'],
        'success!',
        null
    );
}

function finalizarInicializacion() {
    setTimeout(function () {
        configuracionInputMaskOptimizadaPresup();
        optimizarVisualizacionTablaPresup();
    }, 10);
}

function optimizarVisualizacionTablaPresup() {
    // Asegurarnos de que la tabla existe
    if ($("#tbProdDet").length === 0) {
        return;
    }

    // Ajustar columnas con texto para que no sean demasiado anchas
    $("#tbProdDet th:nth-child(2)").css('max-width', '180px'); // Descripción
    $("#tbProdDet td:nth-child(2)").css({
        'max-width': '180px',
        'white-space': 'nowrap',
        'overflow': 'hidden',
        'text-overflow': 'ellipsis'
    });

    // Asegurarnos que la tabla tenga scroll horizontal si es necesario
    $("#tbProdDet").closest('.table-responsive').css('overflow-x', 'auto');

    console.log("Tabla optimizada para mejor visualización");
}

function configuracionInputMaskOptimizadaPresup() {
    console.log("Aplicando configuración InputMask optimizada...");

    // Establecer todos los campos como readonly de una sola vez
    $('.input-tp_plista, .input-tp_dto1, .input-tp_dto2, .input-tp_dto3, .input-tp_dto4, .input-tp_dto_pa, .input-tp_porc_flete, .input-tp_boni, .input-tp_pcosto, .input-tp_margen, .input-tp_pneto, .input-tin_alicuota, .input-tp_pvta')
        .prop('readonly', true)
        .addClass('campo-readonly');

    // Definir configuraciones de máscara fuera de los bucles
    const maskConfig3Decimales = {
        alias: "numeric",
        groupSeparator: ",",
        radixPoint: ".",
        autoGroup: true,
        digits: 3,
        digitsOptional: false,
        rightAlign: true,
        prefix: '',
        placeholder: "0",
        clearMaskOnLostFocus: false,
        showMaskOnHover: false,
        showMaskOnFocus: false,
        min: 0, // Explícitamente permitir 0 como valor mínimo
        allowMinus: false, // No permitir valores negativos
        onBeforeMask: function (value) {
            // Si es null, undefined o cadena vacía, retornar '0'
            if (value === null || value === undefined || value === '') {
                return '0';
            }

            // Para otros valores, formatear correctamente
            try {
                let numValue = parseFloat(value.toString().replace(/,/g, ''));
                return isNaN(numValue) ? '0' : numValue.toFixed(3);
            } catch (e) {
                console.error('Error al formatear valor:', e);
                return '0';
            }
        }
    };

    //const maskConfig1Decimal = {
    //    alias: "numeric",
    //    groupSeparator: ",",
    //    radixPoint: ".",
    //    autoGroup: true,
    //    digits: 1,
    //    digitsOptional: false,
    //    rightAlign: true,
    //    integerDigits: 2,
    //    min: 0,
    //    max: 99.9,
    //    prefix: '',
    //    placeholder: "0",
    //    clearMaskOnLostFocus: false,
    //    showMaskOnHover: false,
    //    showMaskOnFocus: false,
    //    onBeforeMask: function (value) {
    //        if (value) {
    //            let numValue = parseFloat(value.toString().replace(/,/g, ''));
    //            if (numValue > 99.9) numValue = 99.9;
    //            return isNaN(numValue) ? value : numValue.toFixed(1);
    //        }
    //        return value;
    //    }
    //};

    const maskConfig2Decimales = {
        alias: "numeric",
        groupSeparator: ",",
        radixPoint: ".",
        autoGroup: true,
        digits: 2,
        digitsOptional: false,
        rightAlign: true,
        prefix: '',
        placeholder: "0",
        clearMaskOnLostFocus: false,
        showMaskOnHover: false,
        showMaskOnFocus: false,
        onBeforeMask: function (value) {
            if (value) {
                let numValue = parseFloat(value.toString().replace(/,/g, ''));
                return isNaN(numValue) ? value : numValue.toFixed(2);
            }
            return value;
        }
    };

    //const maskConfigBoni = {
    //    mask: "999/999",
    //    placeholder: "",
    //    showMaskOnHover: false,
    //    showMaskOnFocus: false
    //};

    // Aplicar máscaras de forma eficiente con selección optimizada
    Inputmask(maskConfig3Decimales).mask('.input-tp_pcosto');
    //Inputmask(maskConfig1Decimal).mask('.input-tp_dto1, .input-tp_dto2, .input-tp_dto3, .input-tp_dto4, .input-tp_dto_pa, .input-tp_porc_flete');
    Inputmask(maskConfig2Decimales).mask('.input-tp_margen, .input-tp_pvta');
    //Inputmask(maskConfigBoni).mask('.input-tp_boni');

    // Configurar eventos de edición
    configurarEventosEdicionOptimizado();

    console.log("Configuración InputMask aplicada");
}

// ✅ SIMPLIFICADO: Eventos de edición más eficientes
function configurarEventosEdicionOptimizado() {
    const camposEditables = '.input-tp_plista, .input-tp_dto1, .input-tp_dto2, .input-tp_dto3, .input-tp_dto4, .input-tp_dto_pa, .input-tp_porc_flete, .input-tp_boni, .input-tp_margen, .input-tin_alicuota, .input-tp_pvta';
    const camposSecuencia01 = '.input-tp_plista, .input-tp_dto1, .input-tp_dto2, .input-tp_dto3, .input-tp_dto4, .input-tp_dto_pa, .input-tp_porc_flete, .input-tp_boni';

    // Limpiar eventos previos
    $(document).off('click.camposEditables keydown.camposEditables blur.camposSecuencia01 blur.campoMargen blur.campoPVta blur.campoImpuesto');

    // Evento click unificado
    $(document).on('click.camposEditables', camposEditables, function (e) {
        e.stopPropagation();

        const $this = $(this);
        const pIdDetalle = $this.closest('tr').data('p-id');

        //// Cambio de producto si es necesario
        //if (pIdDetalle !== productoActualEnLista) {
        //    productoActualEnLista = pIdDetalle;
        //    $("#divProdLista").attr('data-producto-actual', pIdDetalle);
        //    destacarFilaSeleccionada(pIdDetalle);
        //    buscarProductoListaOptimizado(pIdDetalle);
        //}

        // Habilitar campo
        $this.prop('readonly', false).removeClass('campo-readonly');
        setTimeout(() => { $this[0].focus(); $this[0].select(); }, 0);
    });

    // Evento keydown unificado
    $(document).on('keydown.camposEditables', camposEditables, function (e) {
        if (e.key === 'Enter' || e.key === 'Tab') {
            e.preventDefault();

            const row = $(this).closest('tr');
            const esSecuencia01 = $(this).is(camposSecuencia01);
            const esMargen = $(this).hasClass('input-tp_margen');
            const esPrecioVenta = $(this).hasClass('input-tp_pvta');

            marcarCampoModificado(this);
            actualizarEstadoCarga(row);
            activarSiguienteCampo(this);

            // Aplicar cálculos según tipo
            if (esSecuencia01) calcularCostoAPIDebounced(row);
            else if (esMargen) calcularPrecioVentaAPIDebounced(row);
            else if (esPrecioVenta) calcularPrecioVentaMargenAPIDebounced(row);
        }
    });

    // Eventos blur simplificados con delegación
    const eventosBlur = {
        [camposSecuencia01]: () => calcularCostoAPIDebounced,
        '.input-tp_margen': () => calcularPrecioVentaAPIDebounced,
        '.input-tp_pvta': () => calcularPrecioVentaMargenAPIDebounced,
        '.input-tin_alicuota': () => recalcularRelacionPrecioVenta
    };

    Object.entries(eventosBlur).forEach(([selector, getCallback]) => {
        $(document).on(`blur.${selector.replace(/[^a-zA-Z]/g, '')}`, selector, function () {
            if ($(this).prop('readonly')) return;

            const row = $(this).closest('tr');
            const value = $(this).val().replace(/,/g, '');
            const numValue = parseFloat(value);

            if (!isNaN(numValue)) {
                const decimals = $(this).hasClass('input-tp_plista') || $(this).hasClass('input-tp_pcosto') || $(this).hasClass('input-tp_pneto') ? 3 :
                    $(this).hasClass('input-tp_dto1') || $(this).hasClass('input-tp_dto2') || $(this).hasClass('input-tp_dto3') || $(this).hasClass('input-tp_dto4') || $(this).hasClass('input-tp_dto_pa') || $(this).hasClass('input-tp_porc_flete') ? 1 : 2;
                $(this).val(numValue.toFixed(decimals));
            }

            $(this).prop('readonly', true).addClass('campo-readonly');
            getCallback()(row);
        });
    });
}

/**
* ✅ NUEVO: Configura eventos de eliminación de productos
* Usa delegación de eventos para botones dinámicos
*/
function configurarEventosEliminacionProducto() {
    // ✅ REMOVER LISTENER PREVIO para evitar duplicados
    $(document).off('click', '.btn-eliminar-producto');

    // ✅ DELEGACIÓN DE EVENTOS (más performante para elementos dinámicos)
    $(document).on('click', '.btn-eliminar-producto', function (e) {
        e.preventDefault();
        e.stopPropagation();

        const $btn = $(this);
        const pId = $btn.data('p-id');
        const $fila = $btn.closest('tr');
        const pDesc = $fila.find('td:nth-child(2)').text().trim();

        confirmarEliminacionProducto(pId, pDesc, $fila);
    });
}

/**
* ✅ NUEVO: Confirma y ejecuta eliminación de producto del grid
* @param {string} pId - ID del producto
* @param {string} pDesc - Descripción del producto
* @param {jQuery} $fila - Fila a eliminar
*/
function confirmarEliminacionProducto(pId, pDesc, $fila) {
    AbrirMensaje(
        'ELIMINAR PRODUCTO',
        `¿Está seguro que desea eliminar el producto "${pDesc}" del presupuesto?`,
        function (resp) {
            if (resp === 'SI') {
                eliminarProductoDelGrid($fila);
            }
            $('#msjModal').modal('hide');
        },
        true,
        ['Eliminar', 'Cancelar'],
        'warn!',
        null
    );
}

/**
 * ✅ NUEVO: Elimina producto del grid y actualiza totales
 * @param {jQuery} $fila - Fila a eliminar
 */
function eliminarProductoDelGrid($fila) {
    const pDesc = $fila.find('td:nth-child(2)').text().trim();

    // ✅ ANIMACIÓN SUAVE (mejor UX)
    $fila.fadeOut(300, function () {
        $(this).remove();

        // ✅ VERIFICAR SI QUEDARON PRODUCTOS
        const $tbody = $('#tbGridPresupuestoProds tbody');
        if ($tbody.find('tr[data-p-id]').length === 0) {
            $tbody.html(`
                <tr>
                    <td colspan="8" class="text-center text-muted py-2">
                        <i class="bx bx-info-circle me-1"></i>No hay productos en este presupuesto
                    </td>
                </tr>
            `);

            // ✅ REMOVER FOOTER si no hay productos
            $('#tbGridPresupuestoProds tfoot').remove();
        } else {
            // ✅ REAJUSTAR CLASES ALTERNADAS
            reajustarClasesAlternadas();
        }

        // ✅ ACTUALIZAR TOTAL
        actualizarTotalGeneralPresup();

        ControlaMensajeSuccess(`Producto "${pDesc}" eliminado correctamente`);
    });
}

/**
 * ✅ NUEVO: Reajusta clases 'alt' después de eliminar filas
 * Mantiene consistencia visual
 */
function reajustarClasesAlternadas() {
    $('#tbGridPresupuestoProds tbody tr[data-p-id]').each(function (index) {
        const $fila = $(this);

        if (index % 2 === 0) {
            $fila.removeClass('alt');
        } else {
            $fila.addClass('alt');
        }
    });
}

/**
 * ✅ OPTIMIZADO: Actualiza visibilidad de botones de eliminación
 * Llamar al cambiar modo edición
 */
function aplicarVisibilidadBotonesEliminar() {
    const enEdicion = estaEnModoEdicionPresup();

    $('.btn-eliminar-producto').each(function () {
        $(this).toggle(enEdicion);
    });
}