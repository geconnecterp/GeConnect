var modoNuevoPedido = false;
var modoModificacionPedido = false;

var campoEnEdicionPedido = null;
let procesandoCampo = false;

// Variable para guardar estado original del pedido
let _pedidoOriginal = null;

const fmtCurrency = (v) =>
    new Intl.NumberFormat('es-AR', { style: 'currency', currency: 'ARS', maximumFractionDigits: 2 }).format(v ?? 0);

const fmtPercent = (v) => {
    // v puede venir como 0.354 o 35.4 -> normalizamos a fracción
    const frac = (Math.abs(v) > 1) ? (v / 100) : v;
    return new Intl.NumberFormat('es-AR', { style: 'percent', minimumFractionDigits: 2, maximumFractionDigits: 2 }).format(frac ?? 0);
};

$(function () {
    InicializaPantallaPedido();
    InicializaEventosPedido();
});

function InicializaPantallaPedido() {
    // INICIALIZAMOS PANELES
    if ($("#divDetalle").is(":visible")) {
        $("#divDetalle").collapse("hide");
    }
    $("#divFiltro").collapse("show");
    // ✅ Activar botón de nuevo pedido
    $("#btnAbmNuevo").prop("disabled", false);

    // Configurar el evento click para el botón Cancelar/Inicializar
    $("#btnAbmCancelar").on("click", function (e) {
        cancelarOperacion(e);
    });

    $("#btnCancel").on("click", function () {
        window.location.href = homePedido;
    });

    $("#btnAbmAceptar, #btnAbmCancelar").prop("disabled", true).hide();

    // Inicializa el período de fechas (hoy / hoy + 30 días)
    initPeriodoFechas();

    // Etiquetas de filtros
    $("#lbChkDesdeHasta").text("Periodo");
    $("#lbRel01").text("Cliente"); // Rel01
    $("#lbEstados").text("Estado"); // Estados
    $("#lbVendedores").text("Vendedores"); // Vendedores"
    $("#lbRepartidores").text("Repartidores"); // Repartidores

    $("#chkDesdeHasta").on("click", function () {
        if ($("#chkDesdeHasta").is(":checked")) {
            $("#Desde").prop("disabled", false);
            $("#Hasta").prop("disabled", false);
        } else {
            $("#Desde").prop("disabled", true);
            $("#Hasta").prop("disabled", true);
        }
    });

    $("#chkEstados").on("click", function () {
        if ($("#chkEstados").is(":checked")) {
            $("#listaEstados").prop("disabled", false);
            $("#EstadosList").prop("disabled", false);
            $("#listaEstados").trigger("focus");
        }
        else {
            $("#listaEstados").prop("disabled", true).val("");
            $("#EstadosList").prop("disabled", true).empty();
        }
    });

    $("#chkVendedores").on("click", function () {
        if ($("#chkVendedores").is(":checked")) {
            $("#listaVendedores").prop("disabled", false);
            $("#VendedoresList").prop("disabled", false);
            $("#listaVendedores").trigger("focus");
        }
        else {
            $("#listaVendedores").prop("disabled", true).val("");
            $("#VendedoresList").prop("disabled", true).empty();
        }
    });

    $("#chkRepartidores").on("click", function () {
        if ($("#chkRepartidores").is(":checked")) {
            $("#listaRepartidores").prop("disabled", false);
            $("#RepartidoresList").prop("disabled", false);
            $("#listaRepartidores").trigger("focus");
        }
        else {
            $("#listaRepartidores").prop("disabled", true).val("");
            $("#RepartidoresList").prop("disabled", true).empty();
        }
    });

    $("#EstadosList").on("dblclick", 'option', function () { $(this).remove(); })
    $("#VendedoresList").on("dblclick", 'option', function () { $(this).remove(); })
    $("#RepartidoresList").on("dblclick", 'option', function () { $(this).remove(); })
    $("#Rel01List").on("dblclick", 'option', function () { $(this).remove(); })

    $("#Rel01").on("click", function () { $(this).val(""); });

    $(document).on("change", "#listaEstados", ControlalistaEstadosSelected);
    $(document).on("change", "#listaVendedores", ControlalistaVendedoresSelected);
    $(document).on("change", "#listaRepartidores", ControlalistaRepartidoresSelected);
}

function ControlalistaEstadosSelected() {
    var item = $("#listaEstados").val();
    var desc = $("#listaEstados option:selected").text();
    if ($("#EstadosList").has('option:contains("' + item + '")').length === 0 && $("#EstadosList").has('option:contains("' + desc + '")').length === 0) {
        var opc = "<option value=" + item + ">" + desc + "</option>"
        $("#EstadosList").append(opc);
    }
}

function ControlalistaVendedoresSelected() {
    var item = $("#listaVendedores").val();
    var desc = $("#listaVendedores option:selected").text();
    if ($("#VendedoresList").has('option:contains("' + item + '")').length === 0 && $("#VendedoresList").has('option:contains("' + desc + '")').length === 0) {
        var opc = "<option value=" + item + ">" + desc + "</option>"
        $("#VendedoresList").append(opc);
    }
}

function ControlalistaRepartidoresSelected() {
    var item = $("#listaRepartidores").val();
    var desc = $("#listaRepartidores option:selected").text();
    if ($("#RepartidoresList").has('option:contains("' + item + '")').length === 0 && $("#RepartidoresList").has('option:contains("' + desc + '")').length === 0) {
        var opc = "<option value=" + item + ">" + desc + "</option>"
        $("#RepartidoresList").append(opc);
    }
}

$("#Rel01").autocomplete({
    source: function (request, response) {

        data = { prefix: request.term }; /*Rel01*/

        $.ajax({
            url: autoComRel01Url,
            type: "POST",
            dataType: "json",
            data: data,
            success: function (obj) {
                response($.map(obj, function (item) {
                    var texto = item.descripcion;
                    return { label: texto, value: item.descripcion, id: item.id, prov: item.provId };
                }));
            }
        })
    },
    minLength: 3,
    select: function (event, ui) {
        //ctaIdSelected = ui.item.id;
        //ctaDescSelected = ui.item.value;
        if ($("#Rel01List").has('option:contains("' + ui.item.id + '")').length === 0) {
            $("#Rel01Item").val(ui.item.id);
            var opc = "<option value=" + ui.item.id + ">" + ui.item.value + "</option>"
            $("#Rel01List").append(opc);
        }
        return true;
    }
});

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

    $("#Desde").val(format(base));
    $("#Hasta").val(format(hasta));

    const enabled = $("#chkDesdeHasta").is(":checked");
    $("#Desde").prop("disabled", !enabled);
    $("#Hasta").prop("disabled", !enabled);
}

function imprimirPedido() {
    let data = { modulo: "", parametros: [] }
    invocacionGestorDoc(data);
}
function InicializaEventosPedido() {
    $(document).on("click", "#btnImprimir", imprimirPedido);
    cargarReporteEnArre(62, {}, "Pedido de Cliente");
    $("#btnImprimir").prop("disabled", true);


    $("#pagEstado").on("change", function () {
        var div = $("#divPaginacion");
        presentaPaginacion(div);
    });

    // Buscar
    $("#btnBuscar").on("click", function () {
        buscarPedidosDeCliente(this);
    });
    funcCallBack = buscarPedidosDeCliente;

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
    $(document).on('dblclick', '.input-pcd_pedida', function (e) {
        e.stopPropagation();
        activarEdicionCampoPresup($(this));
    });

    // Handler para Nuevo Presupuesto
    $(document).on('click', '#btnAbmNuevo', function (e) {
        e.preventDefault();

        if ($("#divFiltro").is(":visible")) {
            $("#divFiltro").collapse("hide");
        }

        modoNuevoPedido = true;
        modoModificacionPedido = false;

        if (typeof nuevoPedidoUrl === 'undefined') {
            console.error('nuevoPedidoUrl no está definido.');
            return;
        }

        PostGenHtml({}, nuevoPedidoUrl, function (html) {
            $('#divPedDatos').html(html).show();

            $('#divPedidoDatos').find('input:not([type=hidden]), textarea, select').each(function () {
                const $el = $(this);
                $el.prop('readonly', false).prop('disabled', false).removeClass('campo-readonly');
            });

            const $first = $('#divPedidoDatos').find('input:not([type=hidden]), textarea, select').filter(':visible').first();
            if ($first && $first.length) {
                setTimeout(() => $first.trigger("focus"), 50);
            }

            $('#divPedProds').html(crearGridPedidoVacioHtml()).show();
            $('#btnAgregarCProducto').prop('disabled', false);
            $('#btnAbmAceptar').prop('disabled', false).show();
            $('#btnAbmCancelar').prop('disabled', false).show();
            $('#btnAbmModif, #btnAbmNuevo, #btnAbmElimi').prop('disabled', true);

            setTimeout(() => {
                //aplicarReadonlyCamposPresup();
                finalizarInicializacion()
                // Agregar inicialización del drag & drop aquí
                inicializarDragAndDropProductos();
            }, 100);
            _presupOriginal = null;

            console.log('Modo Nuevo Presupuesto activado.');
        }, function (err) {
            console.error('Error al cargar NuevoPresupuesto:', err);
        });
    });
}

function crearGridPedidoVacioHtml() {
    return `
    <div class="grid-golden h-100">
        <div class="grid-golden-header py-1 d-flex align-items-center">
            <h6 class="mb-0">Productos del Pedido</h6>

            <button type="button" class="btn btn-sm btn-outline-primary" id="btnAgregarCProducto" title="Agregar Producto" disabled>
                <i class="bx bx-plus"></i>
            </button>
        </div>
        <div class="grid-golden-body p-1">
            <div class="table-responsive" style="max-height: 400px;">
                <table class="table table-sm table-hover mb-0 table-golden" id="tbGridPedidoProds">
                    <thead class="sticky-top table-golden-header-compact">
                        <tr class="header">
                            <th class="text-center th-compact">#</th>
                            <th class="text-center th-compact">Código</th>
                            <th class="text-left th-compact" style="width:35%;">Descripción</th>
                            <th class="text-end th-compact">Cantidad</th>
                            <th class="text-end th-compact">Venta</th>
                            <th class="text-end th-compact">Total</th>
                            <th class="text-end th-compact">Remp</th>
                            <th class="text-center th-compact" style="width: 50px;">Acciones</th>
                        </tr>
                    </thead>
                    <tbody>
                        <tr>
                            <td colspan="8" class="text-center text-muted py-2">
                                <i class="bx bx-info-circle me-1"></i>No hay productos en este pedido
                            </td>
                            <td></td>
                        </tr>
                    </tbody>
                </table>
            </div>
        </div>
    </div>`;
}

function activarEdicionCampoPresup($campo) {
    if (!estaEnModoEdicionPedido()) return;
    if (campoEnEdicionPedido !== null) return;

    campoEnEdicionPedido = $campo[0];
    $campo.prop('readonly', false)
        .removeClass('campo-readonly')
        .focus()
        .select();
}

function cancelarOperacion(e) {
    console.log('🔄 Cancelando operación de pedido...');

    // ✅ PASO 1: Resetear modos de edición
    modoNuevoPedido = false;
    modoModificacionPedido = false;
    campoEnEdicionPedido = null;
    _pedidoOriginal = null;

    if ($("#divDetalle").is(":visible") && $("#divFiltro").is(":not(:visible)")) {
        $("#divFiltro").collapse("show");
        $("#divDetalle").collapse("hide");
    }

    // ✅ PASO 2: Vaciar y ocultar divs de datos y productos
    $("#divPresDatos, #divPresProds").empty().hide();

    // ✅ PASO 3: Determinar si hay fila seleccionada en el grid de búsqueda
    const $filaSeleccionada = $("#tbGridPedido tbody tr.selected-row");
    const hayPedidoSeleccionado = $filaSeleccionada.length > 0;

    // ✅ PASO 4: Restaurar botones ABM según contexto
    if (hayPedidoSeleccionado) {
        // Si hay un pedido seleccionado, mantener habilitados Modificar y Eliminar
        const pceId = $filaSeleccionada.data('pce-id') || 'P';
        const estadosEditables = ['P'];
        const permite = estadosEditables.includes(pceId);

        $("#btnAbmModif").prop("disabled", !permite);
        $("#btnAbmElimi").prop("disabled", !permite);
        $("#btnAbmNuevo").prop("disabled", false);
        $("#btnImprimir").prop("disabled", false);

    } else {
        // Si no hay selección, solo habilitar Nuevo
        $("#btnAbmNuevo").prop("disabled", false);
        $("#btnAbmModif, #btnAbmElimi, #btnImprimir").prop("disabled", true);
        let data = {};
        cargarReporteEnArre(62, data, "Pedido de Cliente");
    }

    // ✅ PASO 5: Desactivar y ocultar botones de confirmación
    $("#btnAbmAceptar, #btnAbmCancelar").prop("disabled", true).hide();

    // ✅ PASO 6: Deshabilitar botón de agregar productos
    $("#btnAgregarCProducto").prop("disabled", true);

    // ✅ PASO 7: Limpiar clases de edición en el grid (mantener selección)
    $("#tbGridPedido tbody tr").removeClass("selectedEdit-row").removeClass("selected-row");

    console.log('✅ Operación cancelada - Vista reinicializada');

    $("#divPedido").removeClass("table-wrapper-100").addClass("table-wrapper-300");

    //// ✅ PASO 8: Redirección si es necesario
    //if (e && $(e.target).is("#btnAbmCancelar") && typeof homePedido !== 'undefined') {
    //    console.log('🔀 Redirigiendo a:', homePedido);
    //    window.location.href = homePedido;
    //}
}

let _pedidoLoading = false;

async function buscarPedidosDeCliente(btn, pag = 1) {
    if (_pedidoLoading) return;
    _pedidoLoading = true;

    const $btn = $(btn);
    const originalHtml = $btn.html();
    setBtnLoading($btn, true);

    try {
        const filtros = buildQueryFilters(pag);
        const url = buscarPedidosUrl;

        PostGenHtml(filtros, url, function (html) {
            $("#divDetalle").html(html).collapse("show");
            $("#divFiltro").collapse("hide");

            configurarEventosSeleccionPedido();

            $("#btnAbmAceptar").prop("disabled", true).show();
            $("#btnAbmCancelar").prop("disabled", false).show();

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
        console.error("Error al buscar pedidos de clientes:", e);
        $("#divDetalle").html('<div class="alert alert-danger py-2 mb-0">No se pudo obtener la información.</div>').collapse("show");
    } finally {
        setBtnLoading($btn, false, originalHtml);
        _pedidoLoading = false;
    }
}

function setBtnLoading($btn, loading, originalHtml) {
    if (!$btn || !$btn.length) return;
    if (loading) {
        $btn.prop("disabled", true).html('<span class="spinner-border spinner-border-sm me-1"></span>Buscando...');
    } else {
        $btn.prop("disabled", false).html(originalHtml ?? "Buscar");
    }
}

function buildQueryFilters(pag) {
    const usaPeriodo = $("#chkDesdeHasta").is(":checked");
    const fechaD = usaPeriodo ? $("#Desde").val() : null;
    const fechaH = usaPeriodo ? $("#Hasta").val() : null;

    var rel01 = [];
    $("#Rel01List").children().each(function (i, item) { rel01.push($(item).val()) });
    
    var rel02 = [];
    $("#EstadosList").children().each(function (i, item) { rel02.push($(item).val()) });

    var rel03 = [];
    $("#VendedoresList").children().each(function (i, item) { rel03.push($(item).val()) });

    var rel04 = [];
    $("#RepartidoresList").children().each(function (i, item) { rel04.push($(item).val()) });

    return {
        Registros: 200,
        Pagina: pag,
        FechaD: fechaD || null,
        FechaH: fechaH || null,
        Rel01: rel01.length ? rel01 : null,
        Rel02: rel02.length ? rel02 : null,
        Rel03: rel03.length ? rel03 : null,
        Rel04: rel04.length ? rel03 : null,
    };
}

function actualizarTotalGeneralPedido() {
    let totalGeneral = 0;

    $('#tbGridPedidoProds tbody tr').each(function () {
        const $fila = $(this);
        if ($fila.find('td[colspan]').length > 0) return;

        const total = parseFloat($fila.find('.td-total').text().replace(/,/g, '')) || 0;
        totalGeneral += total;
    });

    $('#tbGridPedidoProds tfoot .fw-bold:last').text(totalGeneral.toFixed(2));
}


function configurarEventosSeleccionPedido() {
    $(document).off("click", "#tbGridPedido tbody tr");
    $(document).on("click", "#tbGridPedido tbody tr", function (e) {
        if (!$(e.target).is("button, a, .btn, i")) {
            var $this = $(this);
            var fueSeleccionado = $this.hasClass("selected-row");

            $("#tbGridPedido tbody tr").removeClass("selected-row");

            if (!fueSeleccionado) {
                $this.addClass("selected-row");
                let pcCompte = $this.data("pc-compte");

                if (pcCompte) {
                    $("#btnImprimir").prop("disabled", false);
                    let data = { pc_compte: pcCompte };
                    cargarReporteEnArre(62, data, "Pedido de Cliente");
                    cargarPedidoDatos(pcCompte);
                    cargarProductosPedido(pcCompte);
                }
            }

            //achico el tamaño del grid
            const $grid = $("#divPedido");
            var gridAchicado = $grid.hasClass("table-wrapper-100");
            if (!gridAchicado) {
                $grid.removeClass("table-wrapper-300").addClass("table-wrapper-100")
            }
            setTimeout(() => {
                ///posiciona el select en la parte visual del grid al achicarlo
                posicionarRegOnTop($this, ".table-wrapper-100");
            }, 200);

        }
    });
    //configurando los eventos para el boton que elimina el registro.
    configurarEventosEliminacionProducto();
}

function cargarPedidoDatos(pcCompte) {
    const url = obtenerPedidoDatosUrl;
    PostGenHtml({ pcCompte: pcCompte }, url, function (html) {
        $("#divPedidoDatos").html(html).show();

        // ✅ DETERMINAR PERMISOS DE EDICIÓN BASÁNDOSE EN EL ESTADO DEL PEDIDO
        // ════════════════════════════════════════════════════════════════════
        //
        // sistemas de pedidos [REF A1]:
        // 'P' = Pendiente (editable Clase 'A')
		// 'O' = En Preparación (editable Clase 'A')
        // 'C' = A Consolidar (editable Clase 'B')
        // 'T' = A Facturar (editable Clase 'B')
        // 'A' = Anulado (no editable)
        // 'E' = Entregado (no editable)
		// 'F' = Facturado (no editable)
        //
        // ⚠️ IMPORTANTE: Ajustar el array 'estadosEditables' según los estados
        //    reales definidos en la base de datos (tabla [dbo].[pedidos_clientes_e])
        // ═══════════════════════════════════════════════════════════════════════

        const pceId = $("#pce_id").val(); // Estado del pedido desde el formulario cargado

        //✅ Solo permitir edición si REF A1
        const estadosEditables = ['P', 'O', 'C', 'T']; // ⚠️ Ajustar estos valores según sea necesario
        const permite = estadosEditables.includes(pceId);

        $("#btnAbmModif").prop("disabled", !permite);
        $("#btnAbmElimi").prop("disabled", !permite);

        // Debug - ayuda a identificar estados del sistema
        console.log("cargarPedidoDatos: Estado del pedido:", pceId,
            "Permite edición:", permite);
    });
}

function cargarProductosPedido(pcCompte, isUpdate = false) {
    let url = obtenerPedidoProductoUrl;

    PostGenHtml({ pcCompte: pcCompte }, url, function (html) {
        $("#divPedidoProds").empty().html(html).show();
        // Forzar estado readonly acorde al modo
        aplicarReadonlyCamposPedido();

        setTimeout(() => {
            finalizarInicializacion();

            // Inicializar drag & drop si corresponde
            inicializarDragAndDropProductos();
        }, 100);
    });
}

function configuracionInputMaskOptimizadaPedido() {
    console.log("Aplicando configuración InputMask optimizada...");

    // Establecer todos los campos como readonly de una sola vez
    $('.input-pcd_pedida, .input-tp_dto1, .input-tp_dto2, .input-tp_dto3, .input-tp_dto4, .input-tp_dto_pa, .input-tp_porc_flete, .input-tp_boni, .input-tp_pcosto, .input-tp_margen, .input-tp_pneto, .input-tin_alicuota, .input-tp_pvta')
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

function finalizarInicializacion() {
    setTimeout(function () {
        configuracionInputMaskOptimizadaPedido();
    }, 10);
}

function aplicarReadonlyCamposPedido() {
    const campos = $('.input-pcd_pedida');
    const tooltipMsg = 'Active el modo edición (Editar) para modificar este campo';

    requestAnimationFrame(() => {
        if (!estaEnModoEdicionPedido()) {
            // Modo NO edición - Deshabilitar todos los campos
            campos.each(function () {
                const $c = $(this);
                $c.prop('readonly', true)
                    .addClass('campo-readonly');
                if (!$c.attr('title')) {
                    $c.attr('title', tooltipMsg);
                }
            });

            // Ocultar botones de eliminación
            $('.btn-eliminar-producto').hide();

        } else {
            const $filas = $('#tbGridPedidoProds tbody tr');
            if (modoNuevoPedido) {
                return;
            }
        }
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
        `¿Está seguro que desea eliminar el producto "${pDesc}" del pedido?`,
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
        const $tbody = $('#tbGridPedidoProds tbody');
        if ($tbody.find('tr[data-p-id]').length === 0) {
            $tbody.html(`
                <tr>
                    <td colspan="8" class="text-center text-muted py-2">
                        <i class="bx bx-info-circle me-1"></i>No hay productos en este pedido
                    </td>
                </tr>
            `);

            // ✅ REMOVER FOOTER si no hay productos
            $('#tbGridPedidoProds tfoot').remove();
        } else {
            // ✅ REAJUSTAR CLASES ALTERNADAS
            reajustarClasesAlternadas();
        }

        // ✅ ACTUALIZAR TOTAL
        actualizarTotalGeneralPedido();

        ControlaMensajeSuccess(`Producto "${pDesc}" eliminado correctamente`);
    });
}

/**
* ✅ NUEVO: Reajusta clases 'alt' después de eliminar filas
* Mantiene consistencia visual
*/
function reajustarClasesAlternadas() {
    $('#tbGridPedidoProds tbody tr[data-p-id]').each(function (index) {
        const $fila = $(this);

        if (index % 2 === 0) {
            $fila.removeClass('alt');
        } else {
            $fila.addClass('alt');
        }
    });
}

function estaEnModoEdicionPedido() {
    return !!(modoNuevoPedido || modoModificacionPedido);
}

function inicializarDragAndDropProductos() {
    // Solo inicializar si hay filas y estamos en modo edición
    if (!estaEnModoEdicionPedido()) {
        console.log('❌ Drag & Drop no inicializado - No está en modo edición');
        return;
    }

    console.log('🔄 Inicializando Drag & Drop...');

    const $tbody = $('#tbGridPedidoProds tbody');

    // Destruir instancia previa si existe
    if ($tbody.hasClass('ui-sortable')) {
        $tbody.sortable('destroy');
    }

    // Usar Sortable de jQuery UI que ya está incluido en el proyecto
    $tbody.sortable({
        handle: 'td:first', // Usar primera columna como handle
        helper: function (e, ui) {
            // Mantener ancho de columnas durante el drag
            ui.children().each(function () {
                $(this).width($(this).width());
            });
            return ui;
        },
        axis: 'y',
        cursor: 'move',
        opacity: 0.7,
        stop: function (event, ui) {
            console.log('🔄 Reordenando filas...');
            // Reordenar items y actualizar numeración
            reordenarFilasPedidoProds();

            // Recalcular totales por si acaso
            setTimeout(() => {
                actualizarTotalGeneralPedido();
                //calcularUtilidadMargen();
            }, 50);
        }
    }).disableSelection();

    // Agregar indicador visual mejorado
    $tbody.find('tr').each(function () {
        const $firstCell = $(this).find('td:first');
        if ($firstCell.length && !$firstCell.hasClass('drag-handle')) {
            $firstCell
                .addClass('drag-handle')
                .css({
                    'cursor': 'move',
                    'position': 'relative'
                })
                .append('<i class="bx bx-move-vertical position-absolute" style="right: 5px; top: 50%; transform: translateY(-50%);"></i>');
        }
    });

    console.log('✅ Drag & Drop inicializado');
}

function reordenarFilasPedidoProds() {
    console.log('🔄 Iniciando reordenamiento de filas');

    const $tbody = $('#tbGridPedidoProds tbody');
    let contador = 1;

    $tbody.find('tr').each(function () {
        const $fila = $(this);

        // Ignorar filas de mensaje
        if ($fila.find('td[colspan]').length > 0) {
            console.log('⏭️ Saltando fila de mensaje');
            return;
        }

        // Actualizar número de ítem
        $fila.attr('data-pre-item', contador);
        $fila.find('td:first').text(contador);

        // Actualizar clases alternadas
        $fila.removeClass('alt');
        if (contador % 2 === 0) {
            $fila.addClass('alt');
        }

        contador++;
    });

    console.log(`✅ Reordenamiento completado - ${contador - 1} filas procesadas`);
}

/**
* ✅ OPTIMIZADO: Actualiza visibilidad de botones de eliminación
* Llamar al cambiar modo edición
*/
function aplicarVisibilidadBotonesEliminar() {
    const enEdicion = estaEnModoEdicionPedido();

    $('.btn-eliminar-producto').each(function () {
        $(this).toggle(enEdicion);
    });
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