var modoNuevoPedido = false;
var modoModificacionPedido = false;
var modoEliminacionPedido = false;  

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

    $("#btnAbmAceptar, #btnAbmCancelar, #btnImprimir").prop("disabled", true).hide();

    // Inicializa el período de fechas (hoy / hoy + 30 días)
    initPeriodoFechas();

    // Etiquetas de filtros
    $("#lbChkDesdeHasta").text("Periodo");
    $("#lbRel01").text("Cliente"); // Rel01
    $("#lbEstados").text("Estado"); // Estados
    $("#lbVendedores").text("Vendedores"); // Vendedores"
    $("#lbRepartidores").text("Repartidores"); // Repartidores

    $("#chkDesdeHasta")
        .prop("checked", true)
        .prop("disabled", true);

    $("#Desde").prop("disabled", false);
    $("#Hasta").prop("disabled", false);

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

        const desde = $("#Desde").val();
        const hasta = $("#Hasta").val();

        const clientes = listFrom("Rel01List");
        const estados = listFrom("EstadosList");
        const vendedores = listFrom("VendedoresList");
        const repartidores = listFrom("RepartidoresList");

        let html = '<div class="d-inline-flex align-items-center" style="gap:8px;white-space:nowrap;">';
        if (desde) html += `<span class="badge bg-secondary">Desde: ${desde}</span>`;
        if (hasta) html += `<span class="badge bg-secondary">Hasta: ${hasta}</span>`;

        html += renderGroup('CLIENTE', clientes);
        html += renderGroup('ESTADO', estados);
        html += renderGroup('VEND.', vendedores);
        html += renderGroup('REPAR.', repartidores);
        html += '</div>';

        cont.html(html);
    } catch (e) {
        console.error('MostrarFiltrosAplicados error', e);
    }
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
            url: autoComRel011Url,
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
    // Último lunes pasado
    const desde = obtenerUltimoLunes();

    // Hoy
    const hasta = new Date();

    // Formatear YYYY-MM-DD
    const fmt = d => d.toISOString().split("T")[0];

    $("#Desde").val(fmt(desde));
    $("#Hasta").val(fmt(hasta));

    // Siempre habilitadas
    $("#Desde").prop("disabled", false);
    $("#Hasta").prop("disabled", false);

    // Checkbox siempre marcado y deshabilitado
    $("#chkDesdeHasta")
        .prop("checked", true)
        .prop("disabled", true);
}

function obtenerUltimoLunes() {
    const hoy = new Date();
    const diaSemana = hoy.getDay(); // 0=Domingo ... 1=Lunes

    // Si hoy es lunes → retroceder 7 días
    const diferencia = diaSemana === 1 ? 7 : (diaSemana + 6) % 7;

    const ultimoLunes = new Date(hoy);
    ultimoLunes.setDate(hoy.getDate() - diferencia);

    return ultimoLunes;
}

function imprimirPedido(pcCompte) {
    ImprimirPedido_Generado(pcCompte);
}

function validarCliente() {
    // Caso 1: Pedido nuevo → se usa Rel01B
    const rel01 = $("#Rel01B");
    if (rel01.length && !rel01.prop("readonly")) {
        const valor = rel01.val()?.trim();
        const item = $("#Rel01BItem").val();

        if (!valor || !item) {
            //alert("Debe seleccionar un cliente válido.");
            return false;
        }

        return true;
    }

    // Caso 2: Pedido existente → se usa cta_denominacion
    const cta = $("#cta_id");
    if (cta.length) {
        const valor = cta.val()?.trim();

        if (!valor) {
            //alert("El cliente del pedido no es válido.");
            return false;
        }

        return true;
    }

    // Si no existe ninguno, es un error de estructura
    //alert("No se encontró un campo de cliente para validar.");
    return false;
}

function obtenerProductosDelGrid() {
    const productos = [];
    const $filas = $('#tbGridPedidoProds tbody tr');
    let cont = 0;

    let errorReemplazo = null; // 🔥 Para capturar el primer error

    $filas.each(function () {
        const $fila = $(this);
        cont++;

        if ($fila.find('td[colspan]').length > 0) return;

        const pId = $fila.data('p-id');
        if (!pId) return;

        const pDes = $fila.find('.input-p_desc').text().trim() || "";
        const pcdCantidad = parseFloat($fila.find('.input-pcd_pedida').val().replace(/,/g, '')) || 0;
        const pcdEnviada = parseFloat($fila.find('.input-pcd_enviada').text().replace(/,/g, '')) || 0;
        const pcdPVta = parseFloat($fila.find('.input-pcd_pvta').text().replace(/,/g, '')) || 0;

        const pcdOrigenBool = $fila.find('.input-pcd_origen_bool').prop('checked');
        const pcdOrigen = pcdOrigenBool ? 'S' : 'N';

        const $selectReemplazo = $fila.find('.input-pcd_reemplazo');
        const remplazoId = $selectReemplazo.val() || "";
        const remplazoDesc = $selectReemplazo.find("option:selected").text().trim();

        // 🔥 VALIDACIÓN: si es origen y no eligió reemplazo → ERROR
        if (pcdOrigenBool && remplazoId === "") {
            errorReemplazo = `Debe seleccionar un reemplazo para el producto ${pId} - ${pDes}`;
            return false; // cortar el each
        }

        productos.push({
            p_id: pId,
            p_desc: pDes,
            pcd_item: cont,
            pcd_pedida: pcdCantidad,
            pcd_enviada: pcdEnviada,
            lp_id: '003',
            pcd_pvta: pcdPVta,
            pcd_origen: pcdOrigen,
            pcd_oferta: 'N',
            p_id_remplazo: remplazoId,
            ve_comi_base: 0,
            ve_comi_porc: 0,
            rp_comi_base: 0,
            rp_comi_porc: 0,
        });
    });

    // 🔥 Si hubo error → mostrar aviso y devolver null
    if (errorReemplazo) {
        AbrirMensaje("VALIDACIÓN", errorReemplazo, function () {
            $('#msjModal').modal('hide');
        }, false, ["Aceptar"], "error!", null);

        return null;
    }

    return productos;
}

// ============================================================================
// FUNCIONES DE VALIDACIÓN Y CONFIRMACIÓN DE PEDIDO
// ============================================================================

/**
 * ✅ Valida los datos del pedido antes de confirmar
 * @param {string} abm - Tipo de operación: 'A', 'M', 'B'
 * @returns {object} { esValido: boolean, mensaje: string }
 */
function validarPedido(abm) {
    console.log(`🔍 Validando pedido (Modo: ${abm})...`);

    // ✅ VALIDACIÓN 1: Cliente obligatorio
    const ctaValidar = validarCliente();
    if (!ctaValidar) {
        return {
            esValido: false,
            mensaje: 'Debe seleccionar un cliente para el pedido.'
        };
    }

    // ✅ VALIDACIÓN 6: Debe haber al menos un producto
    const productos = obtenerProductosDelGrid();
    if (productos == null || productos == undefined)
        return;
    if (productos.length === 0) {
        return {
            esValido: false,
            mensaje: 'Debe agregar al menos un producto al pedido'
        };
    }

    // ✅ VALIDACIÓN 7: Todos los productos deben tener cantidad > 0
    const productosConCantidadInvalida = productos.filter(p => p.pcd_pedida <= 0);
    if (productosConCantidadInvalida.length > 0) {
        return {
            esValido: false,
            mensaje: 'Todos los productos deben tener una cantidad mayor a 0'
        };
    }

    console.log('✅ Validación exitosa');
    return { esValido: true, mensaje: '' };
}

// Handler para Aceptar/Confirmar Pedido
$(document).on('click', '#btnAbmAceptar', function (e) {
    e.preventDefault();

    if ($(this).prop('disabled')) return;

    // Determinar modo ABM
    let abm = '';
    if (modoNuevoPedido) {
        abm = 'A'; // Alta
    } else if (modoModificacionPedido) {
        abm = 'M'; // Modificación
    } else if (modoEliminacionPedido) {
        abm = 'B'; // Baja
    } else {
        console.error('⚠️ Modo de operación no determinado');
        ControlaMensajeError('No se puede determinar la operación a realizar');
        return;
    }

    // Validar antes de confirmar
    const validacion = validarPedido(abm);
    if (validacion == null || validacion == undefined)
        return;
    if (!validacion.esValido) {
        ControlaMensajeWarning(validacion.mensaje);
        AbrirMensaje("ATENCIÓN", validacion.mensaje, function () {
            $("#msjModal").modal("hide");
            return true;
        }, false, ["Aceptar"], "error!", null);
        return;
    }
    else {
        // Mostrar confirmación
        const mensajeConfirmacion = abm === 'A'
            ? '¿Desea confirmar la creación del pedido?'
            : '¿Desea confirmar las modificaciones del pedido?';

        AbrirMensaje(
            'CONFIRMAR PEDIDO',
            mensajeConfirmacion,
            function (resp) {
                if (resp === 'SI') {
                    confirmarPedido(abm);
                }
                activarTablaPedidos();
                $("#divPedido")
                    .removeClass("table-wrapper-small")
                    .addClass("table-wrapper-full");
                $('#msjModal').modal('hide');
            },
            true,
            ['Confirmar', 'Cancelar'],
            'info!',
            null
        );
    }
});

/**
 * ✅ Confirma el pedido enviándolo al servidor
 * @param {string} abm - Tipo de operación: 'A', 'M', 'B'
 */
function confirmarPedido(abm) {
    console.log(`📤 Confirmando pedido (Modo: ${abm})...`);

    try {
        // Construir objeto de confirmación
        const confirmacionDto = construirPedidoConfirmaReqDto(abm);

        // 🔥 Si hubo error en la construcción del DTO, detener todo
        if (!confirmacionDto) {
            return;
        }

        AbrirWaiting('Confirmando pedido...');

        // Debug: Ver estructura completa
        console.log('📦 DTO de confirmación:', confirmacionDto);

        $.ajax({
            url: confirmarPedidoUrl,
            type: 'POST',
            contentType: 'application/json; charset=utf-8', // ⚠️ CRUCIAL
            data: JSON.stringify(confirmacionDto), // ⚠️ SERIALIZAR EXPLÍCITAMENTE
            dataType: 'json',
            success: function (response) {
                CerrarWaiting();
                if (response.error === true || response.warn === true) {
                    console.error('❌ Response:', response.msg);
                    AbrirMensaje("ATENCIÓN", 'Error al intentar confirmar el pedido: ' + (response.msg || 'Error desconocido'), function () {
                        $("#msjModal").modal("hide");
                        return true;
                    }, false, ["Aceptar"], "error!", null);
                }
                else {
                    procesarRespuestaConfirmacion(response, abm);
                    if (abm == 'A' || abm == 'M')
                        ImprimirPedido_Generado(response.id);
                }
            },
            error: function (xhr, status, error) {
                CerrarWaiting();
                console.error('❌ Error al confirmar pedido:', error);
                console.error('❌ Response:', xhr.responseText);
                ControlaMensajeError(
                    'Error al confirmar el pedido: ' +
                    (xhr.responseJSON?.mensaje || xhr.statusText || 'Error desconocido')
                );
            }
        });
        
    } catch (error) {
        CerrarWaiting();
        console.error('❌ Error al construir DTO:', error);
        ControlaMensajeError('Error al procesar los datos del pedido: ' + error.message);
    }
}

//Temporal
//$("#btnImprimir").on("click", function () {
//    ImprimirPedido_Generado("00-00089741");
//});

/**
 * ✅ Construye el DTO PedidoConfirmaReqDto
 * @param {string} abm - Tipo de operación
 * @returns {object} PedidoConfirmaReqDto
 */
function construirPedidoConfirmaReqDto(abm) {
    const productos = obtenerProductosDelGrid();

    if (!productos) return null; // 🔥 Evita continuar si hubo error
    return {
        Abm: abm,
        Datos: obtenerDatosFormularioPedido(),
        Productos: productos
    };
}

/**
 * ✅ Procesa la respuesta del servidor después de confirmar
 * @param {object} response - Respuesta del servidor
 * @param {string} abm - Tipo de operación
 */
function procesarRespuestaConfirmacion(response, abm) {
    console.log('📥 Respuesta del servidor:', response);

    if (response.error || response.warn) {
        if (response.error) {
            AbrirMensaje("ATENCIÓN", response.mensaje || 'Error al confirmar el pedido', function () {
                $("#msjModal").modal("hide");
                return true;
            }, false, ["Aceptar"], "error!", null);
            return;
        }
        else //warn
        {
            AbrirMensaje("ATENCIÓN", response.mensaje || 'Atención al confirmar el pedido', function () {
                $("#msjModal").modal("hide");
                return true;
            }, false, ["Aceptar"], "error!", null);
            return;
        }
    }

    // Mensaje de éxito según el tipo de operación
    let mensajeExito = '';
    switch (abm) {
        case 'A':
            mensajeExito = 'Pedido creado exitosamente';
            break;
        case 'M':
            mensajeExito = 'Pedido modificado exitosamente';
            break;
        case 'B':
            mensajeExito = 'Pedido eliminado exitosamente';
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

            // Si hay ID de pedido en la respuesta, imprimir el pedido
            if (response.id) {
                // Opcional: Recargar el pedido recién creado/modificado
                console.log('✅ Pedido ID:', response.pc_compte);
            }
        },
        false,
        ['Aceptar'],
        'success!',
        null
    );
}

function obtenerDatosFormularioPedido() {
    const pcCompte = $('#pc_compte').val();
    var ctaId = "";
    var pcFc = "";
    if (pcCompte == "0" || pcCompte == "") {
        ctaId = $('#Rel01BItem').val() || '';
    }
    else {
		ctaId = $('#cta_id').val() || '';
    }
    if ($("#pc_cons_final").is(":checked")) {
        pcFc = "S";
    }
    else {
		pcFc = "N";
    }
    const datos = {
        pc_compte: $('#pc_compte').val() || '',
        pc_fecha: $('#pc_fecha').val() || '',
        pc_entrega: $('#pc_entrega').val() || '',
        cta_id: ctaId,
        pc_obs: $('#pc_obs').val() || '',
        pc_cf: pcFc
    };

    console.log('📋 Datos del formulario capturados:', datos);
    return datos;
}

function ReseteoDeReportes() {
    console.log("Reseto de reportes");
    ReporteResetArre();
}

function ImprimirPedido_Generado(pcCompte) {
    ReseteoDeReportes();
    setTimeout(() => {
        let data = { pc_compte: pcCompte };
        cargarReporteEnArre(62, data, "PEDIDO DE CLIENTE", "", "");
        invocacionGestorDoc({});
    }, 500);
}

function agregarProductosAlGrid(productos) {
    if (!Array.isArray(productos) || productos.length === 0) return;

    const $tbody = $('#tbGridPedidoProds tbody');

    const $filaVacia = $tbody.find('tr td[colspan]');
    if ($filaVacia.length > 0) {
        $filaVacia.closest('tr').remove();
    }

    let $tfoot = $('#tbGridPedidoProds tfoot');
    if ($tfoot.length === 0) {
        $('#tbGridPedidoProds').append(`
            <tfoot class="table-golden-footer">
                <tr>
                    <td colspan="7" class="text-end fw-bold">Total General:</td>
                    <td class="text-end fw-bold">0.00</td>
                </tr>
            </tfoot>
        `);
        $tfoot = $('#tbGridPedidoProds tfoot');
    }

    let esAlternado = $tbody.find('tr').length % 2 !== 0;

    productos.forEach(function (producto, index) {
        const fila = crearFilaProductoPedido(producto, esAlternado, index + 1);
        $tbody.append(fila);
        esAlternado = !esAlternado;
    });

    //aplicarInputMaskPresupuesto();
    aplicarReadonlyCamposPedido();
    actualizarTotalGeneralPedido();
    configurarEventosEliminacionProducto();
    setTimeout(() => {
        finalizarInicializacion();
        //calcularUtilidadMargen();
        // Reinicializar drag & drop con las nuevas filas
        inicializarDragAndDropProductos();
    }, 100);
}

/**
 * ✅ OPTIMIZADO: Crea HTML de fila de producto con TODOS los nuevos campos
 * Unifica lógica de cálculo y evita duplicación de código
 * @param {object} producto - ProductoListaDto
 * @param {boolean} esAlternado - Alternar clase CSS
 * @returns {string} HTML de la fila
 */
function crearFilaProductoPedido(producto, esAlternado, pcd_item) {
    // ✅ VALIDACIÓN Y NORMALIZACIÓN DE DATOS
    const datosProducto = normalizarDatosProducto(producto);

    // ✅ FORMATEO
    const claseAlt = esAlternado ? 'alt' : '';

    const selectReemplazoHTML = datosProducto.pcd_origen_bool
        ? crearSelectReemplazo(datosProducto.p_id, datosProducto.p_id_remplazo)
        : '<span class="text-muted">—</span>';

    // ✅ CONSTRUCCIÓN HTML CON TEMPLATE LITERALS (más legible y performante)
    return `
        <tr class="${claseAlt}"
            data-pcd-item="${pcd_item}"
            data-p-id="${datosProducto.p_id}">

            <td class="text-center">${pcd_item}</td>
            <td class="text-center">${datosProducto.p_id}</td>
            <td class="input-p_desc">${escaparHTML(datosProducto.p_desc)}</td>

            <td class="text-end">
                <div class="input-container">
                    <input type="text"
                           class="form-control form-control-sm input-pcd_pedida input-numeric"
                           value="${datosProducto.pcd_pedida}"
                           data-original-value="${datosProducto.pcd_pedida}"
                           title="Doble click para editar" />
                </div>
            </td>
            <td class="text-end input-pcd_enviada">${datosProducto.pcd_enviada.toFixed(0)}</td>
            <td class="text-end input-pcd_pvta">${datosProducto.p_pvta.toFixed(2)}</td>
            <td class="text-end input-pcd_pvta_total">${(datosProducto.p_pvta * datosProducto.pcd_pedida).toFixed(2)}</td>

            <td class="text-center align-middle">
                <input type="checkbox"
                       class="form-check-input m-0 p-0 input-pcd_origen_bool"
                       disabled
                       ${datosProducto.pcd_origen_bool ? "checked" : ""} />
            </td>

            <td class="text-center">
                ${selectReemplazoHTML}
            </td>

            <td class="text-center">
                <button type="button"
                        class="btn btn-sm btn-danger btn-eliminar-producto"
                        data-p-id="${datosProducto.p_id}"
                        title="Eliminar producto"
                        style="${estaEnModoEdicionPedido() ? '' : 'display: none;'}">
                    <i class="bx bx-trash"></i>
                </button>
            </td>
        </tr>
    `;
}

function crearSelectReemplazo(p_id_actual, p_id_remplazo) {
    let html = `<select class="form-select form-select-sm input-pcd_reemplazo">
                    <option value="">-- Seleccionar --</option>`;

    window.productosReemplazables.forEach(prod => {
        if (prod.p_id !== p_id_actual) {
            const selected = (prod.p_id === p_id_remplazo) ? "selected" : "";
            html += `<option value="${prod.p_id}" ${selected}>${prod.p_id} - ${prod.p_desc}</option>`;
        }
    });

    html += `</select>`;
    return html;
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
        pcd_pedida: 1,
        pcd_enviada: 0,
        pcd_origen_bool: true,
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

function InicializaEventosPedido() {
    $(document).off("click", "#btnImprimir");
    $(document).on("click", "#btnImprimir", function () {
        if (!pcCompteSeleccionado) {
            alert("Seleccione un pedido primero.");
            return;
        }
        imprimirPedido(pcCompteSeleccionado);
    });

    //cargarReporteEnArre(62, {}, "Pedido de Cliente");
    $("#btnImprimir").prop("disabled", true);


    $("#pagEstado").on("change", function () {
        var div = $("#divPaginacion");
        presentaPaginacion(div);
    });

    // Buscar
    $("#btnBuscar").on("click", function () {
        try { MostrarFiltrosAplicados(); } catch (e) { console.warn('MostrarFiltrosAplicados no disponible:', e); }
        buscarPedidosDeCliente(1);
    });
    funcCallBack = buscarPedidosDeCliente;

    // Evento delegado para el botón de agregar producto
    $(document).on("click", "#btnAgregarCProducto", function () {
        if ($("#busquedaModal").length === 0) {
            cargarModalBusquedaAvanzada(function () {
                if (typeof configurarDestinoBusquedaProductos === 'function') {
                    configurarDestinoBusquedaProductos("pedidos", "003", agregarProductosAlGrid, obtenerProductosExistentesIds);
                }
                $("#busquedaModal").modal("show");
            });
        } else {
            if (typeof configurarDestinoBusquedaProductos === 'function') {
                configurarDestinoBusquedaProductos("pedidos", "003", agregarProductosAlGrid, obtenerProductosExistentesIds);
            }
            $("#busquedaModal").modal("show");
        }
    });

    // Doble click para activar edición
    $(document).on('dblclick', '.input-pcd_pedida', function (e) {
        e.stopPropagation();
        activarEdicionCampoPedido($(this));
    });

    // Handler para Nuevo Pedido
    $(document).on('click', '#btnAbmNuevo', function (e) {
        e.preventDefault();

        if ($("#divFiltro").is(":visible")) {
            $("#divFiltro").collapse("hide");
        }

        modoNuevoPedido = true;
        desactivarTablaPedidos();
        modoModificacionPedido = false;
        modoEliminacionPedido = false;

        if (typeof nuevoPedidoUrl === 'undefined') {
            console.error('nuevoPedidoUrl no está definido.');
            return;
        }

        PostGenHtml({}, nuevoPedidoUrl, function (html) {
            $('#divPedDatos').html(html).show();

            // Primero bloqueo todo
            $('#divPedidoDatos')
                .find('input:not([type=hidden]), textarea, select')
                .each(function () {
                    const $el = $(this);
                    $el.prop('readonly', true)
                        .prop('disabled', true)
                        .addClass('campo-readonly');
                });

            // Luego habilito solo los permitidos
            $('#divPedidoDatos')
                .find('#pc_fecha, #pc_cons_final, #cta_denominacion, #pc_obs, #Rel01B')
                .each(function () {
                    const $el = $(this);
                    $el.prop('readonly', false)
                        .prop('disabled', false)
                        .removeClass('campo-readonly');
                });

            // Finalmente, seteo pc_fecha a hoy
            const hoy = new Date().toISOString().split('T')[0];
            $('#pc_fecha').val(hoy);
            $('#pc_entrega').val(hoy);

            const $first = $('#divPedidoDatos').find('input:not([type=hidden]), textarea, select').filter(':visible').first();
            if ($first && $first.length) {
                setTimeout(() => $first.trigger("focus"), 50);
            }

            $('#divPedProds').html(crearGridPedidoVacioHtml()).show();
            $('#btnAgregarCProducto').prop('disabled', false);
            $('#btnAbmAceptar').prop('disabled', false).show();
            $('#btnAbmCancelar').prop('disabled', false).show();
            $('#btnAbmModif, #btnAbmNuevo, #btnAbmElimi').prop('disabled', true);

            $("#Rel01B").autocomplete({
                source: function (request, response) {

                    data = { prefix: request.term }; /*Rel01*/

                    $.ajax({
                        url: autoComRel011Url,
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
                    $("#Rel01BItem").val(ui.item.id);
                }
            });

            setTimeout(() => {
                //aplicarReadonlyCamposPresup();
                //finalizarInicializacion()
                // Agregar inicialización del drag & drop aquí
                inicializarDragAndDropProductos();
            }, 100);
            _pedidoOriginal = null;

            console.log('Modo Nuevo Pedido activado.');
        }, function (err) {
            console.error('Error al cargar Nuevo Pedido:', err);
        });
    });

    // Handler para Modificar Pedido
    $(document).on('click', '#btnAbmModif', function (e) {
        e.preventDefault();

        if ($(this).prop('disabled')) return;

        // 🔍 Obtener la fila seleccionada
        const $filaSeleccionada = $('#tbGridPedido tbody tr.selected-row');

        if ($filaSeleccionada.length === 0) {
            alert("Debe seleccionar un pedido.");
            return;
        }

        // 🔥 Capturar el data-pce-id
        const pceId = $filaSeleccionada.data('pce-id');
        console.log("Estado del pedido (pce_id):", pceId);



        modoNuevoPedido = false;
        modoModificacionPedido = true;
        desactivarTablaPedidos();
        modoEliminacionPedido = false;

        _pedidoOriginal = capturarEstadoFormularioPedido();
        habilitarCamposFormularioPedido(true, pceId);
        $('#btnAgregarCProducto').prop('disabled', false);
        $('#btnAbmNuevo, #btnAbmModif, #btnAbmElimi').prop('disabled', true);
        $('#btnAbmAceptar, #btnAbmCancelar').prop('disabled', false).show();

        aplicarReadonlyCamposPedido();

        setTimeout(() => {
            actualizarTotalGeneralPedido();
            // Agregar inicialización del drag & drop aquí
            inicializarDragAndDropProductos();
        }, 100);


        const $primer = $('#divPedidoDatos').find('input:not([type=hidden]):not([readonly]), textarea:not([readonly]), select:not([disabled])').filter(':visible').first();
        if ($primer.length) {
            setTimeout(() => $primer.trigger("focus"), 50);
        }

        console.log('✅ Modo Modificación Pedido activado');
    });


    // ============================================================================
    // ELIMINACIÓN DE PEDIDO
    // ============================================================================

    $(document).on('click', '#btnAbmElimi', function (e) {
        e.preventDefault();
        if ($(this).prop('disabled')) return;

        const pcCompte = $('#pc_compte').val();
        if (!pcCompte || pcCompte.trim() === '') {
            AbrirMensaje("ATENCIÓN", "Debe seleccionar un pedido para anular.", function () {
                $("#msjModal").modal("hide");
                return true;
            }, false, ["Aceptar"], "error!", null);
        }
        else {
            const pceId = $('#pce_id').val();
            const estadosEliminables = ['P'];

            if (!estadosEliminables.includes(pceId)) {
                const nombreEstado = pceId === 'F' ? 'facturado'
                    : pceId === 'C' ? 'consolidado'
                        : pceId === 'E' ? 'entregado'
                            : pceId === 'O' ? 'en curso'
                                : pceId === 'A' ? 'anulado'
                                    : pceId === 'T' ? 'a facturar'
                                        : 'en este estado';
                let mensaje = `No se puede anluar un pedido ${nombreEstado}. ` +
                    `Solo los pedidos en estado Pendiente pueden ser anulados.`;
                AbrirMensaje("ATENCIÓN", mensaje, function () {
                    $("#msjModal").modal("hide");
                    return true;
                }, false, ["Aceptar"], "error!", null);
            }
            else {
                desactivarTablaPedidos();
                const ctaDenominacion = $('#cta_denominacion').val() || 'Sin cliente';
                const vigenciaDesde = $('#pre_vigencia_desde').val() || '';
                const vigenciaHasta = $('#pre_vigencia_hasta').val() || '';

                const mensajeConfirmacion = `
                    <div class="text-start">
                        <p class="mb-2"><strong>¿Está seguro que desea anular este pedido?</strong></p>
                        <hr class="my-2">
                        <p class="mb-1"><strong>ID:</strong> ${pcCompte}</p>
                        <p class="mb-1"><strong>Cliente:</strong> ${ctaDenominacion}</p>
                        <hr class="my-2">
                        <p class="text-danger mb-0">
                            <i class="bx bx-error-circle me-1"></i>
                            <strong>Esta acción no se puede deshacer.</strong>
                        </p>
                    </div>
                    `;

                AbrirMensaje(
                    'ANULAR PEDIDO',
                    mensajeConfirmacion,
                    function (resp) {
                        if (resp === 'SI') {
                            eliminarPedido();
                        }
                        activarTablaPedidos();
                        $('#msjModal').modal('hide');
                    },
                    true,
                    ['Eliminar', 'Cancelar'],
                    'warn!',
                    null
                );
            }
        }
    });
}

// ============================================================================
// FUNCIONES DE ELIMINACIÓN DE PEDIDO
// ============================================================================

function eliminarPedido() {
    console.log('🗑️ Eliminando pedido...');

    const pcCompte = $('#pc_compte').val();
    if (!pcCompte || pcCompte.trim() === '') {
        ControlaMensajeError('Error: No se encontró el ID del pedido para anular');
        return;
    }

    AbrirWaiting('Anulado pedido...');

    try {
        const confirmacionDto = {
            Abm: 'B',
            Datos: obtenerDatosFormularioPedido(),
            Productos: obtenerProductosDelGrid()
        };

        console.log('📦 DTO de eliminación:', confirmacionDto);

        $.ajax({
            url: confirmarPedidoUrl,
            type: 'POST',
            contentType: 'application/json; charset=utf-8', // ⚠️ CRUCIAL
            data: JSON.stringify(confirmacionDto), // ⚠️ SERIALIZAR EXPLÍCITAMENTE
            dataType: 'json',
            success: function (response) {
                CerrarWaiting();
                procesarRespuestaEliminacion(response);
            },
            error: function (xhr, status, error) {
                CerrarWaiting();
                console.error('❌ Error al anular pedido:', error);
                console.error('❌ Response:', xhr.responseText);
                ControlaMensajeError(
                    'Error al anular pedido: ' +
                    (xhr.responseJSON?.mensaje || xhr.statusText || 'Error desconocido')
                );
            }
        });

        //PostGen(
        //    confirmacionDto,
        //    confirmarPedidoUrl,
        //    function (response) {
        //        CerrarWaiting();
        //        procesarRespuestaEliminacion(response);
        //    },
        //    function (error) {
        //        CerrarWaiting();
        //        console.error('❌ Error al anular pedido:', error);

        //        const mensajeError = error.responseJSON?.mensaje
        //            || error.responseJSON?.msg
        //            || error.statusText
        //            || 'Error desconocido';

        //        ControlaMensajeError(`Error al anular el pedido: ${mensajeError}`);
        //    }
        //);
    } catch (error) {
        CerrarWaiting();
        console.error('❌ Error al construir DTO:', error);
        ControlaMensajeError('Error al procesar la anulación: ' + error.message);
    }
}

function procesarRespuestaEliminacion(response) {
    console.log('📥 Respuesta de eliminación:', response);

    if (response.error || response.warn) {
        if (response.error) {
            ControlaMensajeError(response.mensaje || 'Error al anular el pedido');
            return;
        }
        else //warn
        {
            ControlaMensajeWarning(response.mensaje || 'Atención al intentar anular el pedido');
            return;
        }

    }

    AbrirMensaje(
        'ANULACIÖN EXITOSA',
        'El pedido ha sido anulado correctamente',
        function () {
            $('#msjModal').modal('hide');
            cancelarOperacion();

            if ($('#tbGridPedido tbody tr').length > 0) {
                console.log('🔄 Actualizando lista de pedidos...');
                buscarPedidosDeCliente(1);
            }
        },
        false,
        ['Aceptar'],
        'success!',
        null
    );
}

function capturarEstadoFormularioPedido() {
    const estado = {};
    $('#divPedidoDatos').find('input, textarea, select').each(function () {
        const $campo = $(this);
        const nombre = $campo.attr('name') || $campo.attr('id');
        if (nombre) {
            estado[nombre] = $campo.val();
        }
    });
    return estado;
}

function habilitarCamposFormularioPedido(habilitar, pceId) {
    // Normalizamos por si viene en minúscula
    pceId = (pceId || "").toUpperCase();

    // Grupos de estados
    const estadosModificables = ["P", "O"];   // Pendiente, En Curso
    const estadosParciales = ["C", "T"];      // Consolidado, A Facturar
    const estadosBloqueados = ["A", "E", "F"]; // Anulado, Entregado, Facturado

    if (estadosModificables.includes(pceId)) {
        habilitarObservacion(true);
        habilitarCF(true);
        habilitarDetalleProductos(true);
        console.log("Modo edición completa (P/O)");
        return;
    }
    if (estadosParciales.includes(pceId)) {
        habilitarObservacion(true);
        habilitarCF(true);
        habilitarDetalleProductos(false);
        console.log("Modo edición completa (C/T)");
        return;
    }
}

function habilitarObservacion(habilitar) {
    $("#pc_obs").prop("readonly", !habilitar);
}

function habilitarCF(habilitar) {
    $("#pc_cons_final").prop("disabled", !habilitar);
}

function habilitarDetalleProductos(habilitar) {
    // Inputs de cantidad
    $(".input-pcd_pedida").prop("readonly", !habilitar);

    // Botón eliminar producto
    $(".btn-eliminar-producto").toggle(habilitar);

    // Select de reemplazo
    $(".input-pcd_reemplazo").prop("disabled", !habilitar);
}

function obtenerProductosExistentesIds() {
    const productosIds = [];

    $('#tbGridPedidoProds tbody tr').each(function () {
        const $fila = $(this);
        if ($fila.find('td[colspan]').length > 0) return;

        const pId = $fila.data('p-id');
        if (pId) {
            productosIds.push(pId);
        }
    });

    return productosIds;
}

function crearGridPedidoVacioHtml() {
    return `
    <div class="card h-100">
        <div class="card-header py-1 d-flex justify-content-between align-items-center">
            <h6 class="mb-0">Productos del Pedido</h6>

            <button type="button" class="btn btn-sm btn-outline-primary" id="btnAgregarCProducto" title="Agregar Producto" disabled>
                <i class="bx bx-plus"></i>
            </button>
        </div>
        <div class="card-body p-1">
            <div class="table-responsive" style="max-height: 400px;">
                <table class="table table-sm table-hover mb-0 table-golden" id="tbGridPedidoProds">
                    <thead class="sticky-top table-golden-header-compact">
                        <tr class="header">
                            <th class="text-center th-compact">#</th>
                            <th class="text-center th-compact">Código</th>
                            <th class="text-left th-compact" style="width:35%;">Descripción</th>
                            <th class="text-end th-compact">Cantidad</th>
                            <th class="text-end th-compact">Enviada</th>
                            <th class="text-end th-compact">Venta</th>
                            <th class="text-end th-compact">Total</th>
                            <th class="text-end th-compact">Remp</th>
                            <th class="text-end th-compact">Código</th>
                            <th class="text-center th-compact" style="width: 50px;">Acción</th>
                        </tr>
                    </thead>
                    <tbody>
                        <tr>
                            <td colspan="9" class="text-center text-muted py-2">
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

function activarEdicionCampoPedido($campo) {
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
    modoNuevoPedido = false;
    modoModificacionPedido = false;
    campoEnEdicionPedido = null;
    _pedidoOriginal = null;

    $("#divPedDatos, #divPedProds").empty().hide();

    const $filaSeleccionada = $("#tbGridPedido tbody tr.selected-row");
    const hayPedidoSeleccionado = $filaSeleccionada.length > 0;

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
    }

    $("#btnAbmAceptar, #btnAbmCancelar, #btnImprimir").prop("disabled", true).hide();
    $("#btnAgregarCProducto").prop("disabled", true);
    $("#tbGridPedido tbody tr").removeClass("selectedEdit-row").removeClass("selected-row");

    console.log('✅ Operación cancelada - Vista reinicializada');

    $("#divPedido")
        .removeClass("table-wrapper-small")
        .addClass("table-wrapper-full");
    activarTablaPedidos();
}

function desactivarTablaPedidos() {
    $("#tbGridPedido").addClass("tabla-desactivada");
    $("#tbGridPedido tbody tr").addClass("disabled-row");
}

function activarTablaPedidos() {
    $("#tbGridPedido").removeClass("tabla-desactivada");
    $("#tbGridPedido tbody tr").removeClass("disabled-row");
}


let _pedidoLoading = false;

async function buscarPedidosDeCliente(pag = 1) {
    if (_pedidoLoading) return;
    _pedidoLoading = true;
    pagina = pag;
    //const $btn = $(btn);
    //const originalHtml = $btn.html();
    //setBtnLoading($btn, true);

    try {
        AbrirWaiting("Buscando Pedidos de Cliente...")
        const filtros = buildQueryFilters(pag);
        const url = buscarPedidosUrl;

        PostGenHtml(filtros, url, function (html) {
            $("#divDetalle").html(html).collapse("show");
            $("#divFiltro").collapse("hide");
            // actualizar filtros aplicados (si el partial reemplaza el DOM)
            try { MostrarFiltrosAplicados(); } catch (e) { console.warn('MostrarFiltrosAplicados no disponible:', e); }
            configurarEventosSeleccionPedido();
            
            CerrarWaiting();
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
        //setBtnLoading($btn, false, originalHtml);
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

        const total = parseFloat($fila.find('.input-pcd_pvta_total').text().replace(/,/g, '')) || 0;
        totalGeneral += total;
    });

    $('#tbGridPedidoProds tfoot .fw-bold:last').text(totalGeneral.toFixed(2));
}

function configurarEventosSeleccionPedido() {

    // Limpio handlers previos
    $(document).off("click", "#tbGridPedido tbody tr");
    $(document).off("dblclick", "#tbGridPedido tbody tr");

    // ============================
    // CLICK SIMPLE → Seleccionar fila
    // ============================
    $(document).on("click", "#tbGridPedido tbody tr", function (e) {

        if (!$(e.target).is("button, a, .btn, i")) {

            const $this = $(this);

            // Quitar selección previa
            $("#tbGridPedido tbody tr").removeClass("selected-row");

            // Marcar fila seleccionada
            $this.addClass("selected-row");

            // Guardar valor seleccionado
            pcCompteSeleccionado = $this.data("pc-compte");

            // Habilitar botón imprimir
            if (pcCompteSeleccionado) {
                $("#btnImprimir").prop("disabled", false).show();
            }
        }
    });

    // ============================
    // DOBLE‑CLICK → Cargar datos + achicar grid
    // ============================
    $(document).on("dblclick", "#tbGridPedido tbody tr", function (e) {

        if (!$(e.target).is("button, a, .btn, i")) {

            const $this = $(this);
            const pcCompte = $this.data("pc-compte");

            if (!pcCompte) return;

            // Ejecutar funciones de carga
            let data = { pc_compte: pcCompte };
            cargarReporteEnArre(62, data, "Pedido de Cliente", "", "");
            cargarPedidoDatos(pcCompte);
            cargarProductosPedido(pcCompte);

            // Achicar grid
            const $grid = $("#divPedido");
            if (!$grid.hasClass("table-wrapper-100")) {
                $grid.removeClass("table-wrapper-full").addClass("table-wrapper-small");
            }

            // Reposicionar fila seleccionada
            setTimeout(() => {
                posicionarRegOnTop($this, ".table-wrapper-small");
            }, 200);
        }
    });

    // Eventos de eliminación
    configurarEventosEliminacionProducto();
}

let pcCompteSeleccionado = null;

function cargarPedidoDatos(pcCompte) {
    const url = obtenerPedidoDatosUrl;
    PostGenHtml({ pcCompte: pcCompte }, url, function (html) {
        $("#divPedDatos").html(html).show();

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
        $("#divPedProds").empty().html(html).show();
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
    $('.input-pcd_pedida')
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
    //Inputmask(maskConfig1Decimal).mask('.input-tp_dto1, .input-tp_dto2, .input-tp_dto3, .input-tp_dto4, .input-tp_dto_pa, .input-tp_porc_flete');
    Inputmask(maskConfig2Decimales).mask('.input-pcd_pedida');
    //Inputmask(maskConfigBoni).mask('.input-tp_boni');

    // Configurar eventos de edición
    configurarEventosEdicionOptimizado();

    console.log("Configuración InputMask aplicada");
}

/**
 * Actualiza el atributo data-carga de una fila según las reglas:
 * - Si hay cambios y carga=0, establecer carga=1
 * - Si no hay cambios y carga=1, establecer carga=0
 * - En otros casos, mantener valor actual
 * @param {jQuery} row - La fila (tr) a verificar
 * @returns {boolean} - Indica si la fila tiene algún campo modificado
 */
function actualizarEstadoCarga(row) {
    // Obtener el estado actual de carga
    const estadoCargaActual = row.data('carga') === 1;

    // Verificación rápida: si ya hay campos con la clase 'campo-modificado', hay cambios
    const camposModificados = row.find('.campo-modificado').length;

    if (camposModificados > 0) {
        // Hay campos modificados, asegurar que carga=1
        if (!estadoCargaActual) {
            row.data('carga', 1);
            row.attr('data-carga', '1');
            console.log(`Fila ${row.data('p-id')}: Cambiando data-carga a 1 (detectados ${camposModificados} campos modificados)`);
        }
        return true; // Hay campos modificados
    } else {
        // No hay campos con la clase, verificar si realmente hay diferencias
        // (esta es una verificación más profunda y costosa)
        let hayAlgunCampoModificado = false;

        row.find('input[data-original-value]').each(function () {
            const $input = $(this);
            const valorOriginal = $input.data('original-value');
            const valorActual = $input.val().replace(/,/g, '');

            // Verificar si está modificado según el tipo de campo
            if ($input.hasClass('input-tp_boni')) {
                // Lógica para bonificación
                const originalTrim = (valorOriginal || '').toString().trim();
                const actualTrim = (valorActual || '').toString().trim();

                if (!((originalTrim === actualTrim) ||
                    (originalTrim === "0" && actualTrim === "") ||
                    (originalTrim === "" && actualTrim === "0"))) {
                    hayAlgunCampoModificado = true;
                    return false; // Salir del bucle
                }
            } else {
                // Lógica para campos numéricos (simplificada para rendimiento)
                try {
                    const numOriginal = parseFloat(valorOriginal);
                    const numActual = parseFloat(valorActual);

                    if (!isNaN(numOriginal) && !isNaN(numActual) &&
                        Math.abs(numOriginal - numActual) > 0.0001) {
                        hayAlgunCampoModificado = true;
                        return false; // Salir del bucle
                    }
                } catch (e) { }
            }
        });

        // Actualizar según resultado
        if (hayAlgunCampoModificado && !estadoCargaActual) {
            row.data('carga', 1);
            row.attr('data-carga', '1');
            console.log(`Fila ${row.data('p-id')}: Cambiando data-carga a 1 (hay campos modificados no marcados)`);
        } else if (!hayAlgunCampoModificado && estadoCargaActual) {
            row.data('carga', 0);
            row.attr('data-carga', '0');
            console.log(`Fila ${row.data('p-id')}: Cambiando data-carga a 0 (no hay campos modificados)`);
        }

        return hayAlgunCampoModificado;
    }
}

// ✅ SIMPLIFICADO: Eventos de edición más eficientes
function configurarEventosEdicionOptimizado() {
    const camposEditables = '.input-pcd_pedida';
    const camposSecuencia01 = '.input-pcd_pedida';

    // Limpiar eventos previos
    $(document).off('click.camposEditables keydown.camposEditables blur.camposSecuencia01');

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
            //const esMargen = $(this).hasClass('input-tp_margen');
            //const esPrecioVenta = $(this).hasClass('input-tp_pvta');

            marcarCampoModificadoPedido(this);
            actualizarEstadoCarga(row);
            activarSiguienteCampo(this);

            // Aplicar cálculos según tipo
            if (esSecuencia01) calcularTotalAPIDebounced(row);
            //else if (esMargen) calcularPrecioVentaAPIDebounced(row);
            //else if (esPrecioVenta) calcularPrecioVentaMargenAPIDebounced(row);
        }
    });

    // Eventos blur simplificados con delegación
    const eventosBlur = {
        [camposSecuencia01]: () => calcularTotalAPIDebounced
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

function marcarCampoModificadoPedido($campo) {
    if (!$campo || !$campo.length) return;
    $campo.addClass('campo-modificado');
    setTimeout(() => $campo.removeClass('campo-modificado'), 1500);
}

// Función de debounce para evitar llamadas repetidas
function debounce(func, wait) {
    let timeout;
    return function () {
        const context = this, args = arguments;
        clearTimeout(timeout);
        timeout = setTimeout(function () {
            func.apply(context, args);
        }, wait);
    };
}

// Aplicar debounce a funciones de cálculo intensivas
const calcularTotalAPIDebounced = debounce(function (row) {
    calcularProductoCompleto(row);
}, 300);

// ✅ UNIFICADA: Función principal que detecta contexto y aplica la lógica correcta
function calcularProductoCompleto(row, callback = null) {
    const productId = row.data('p-id');

    console.log(`🔄 Cálculo MASIVO para producto ${productId}`);
    calcularProductoCompletoSincrono(row);

}

// Helper: formatea número igual que GridHelper.FormatearPrecio (separador decimal ".", miles con ",")
function formatPrecio(valor, tipoPrecio = 'Venta') {
    if (valor == null || isNaN(Number(valor))) return '';

    const decimales = (tipoPrecio === 'Lista' || tipoPrecio === 'Costo' || tipoPrecio === 'Neto') ? 3 : 2;
    // Usamos 'en-US' para obtener separador decimal "." y miles con ","
    const nf = new Intl.NumberFormat('en-US', { minimumFractionDigits: decimales, maximumFractionDigits: decimales });
    return nf.format(Number(valor));
}


// ✅ CORREGIDA: Versión síncrona con resguardo completo de producto y listas
function calcularProductoCompletoSincrono(row) {
    const productId = row.data('p-id');

    // ✅ EVITAR: Cálculos duplicados
    if (row.data('processing') === true) {
        console.log(`⏭️ Producto ${productId} ya en procesamiento`);
        return { success: false, skip: true };
    }

    row.data('processing', true);

    try {
        console.log(`🔄 Calculando COMPLETO SÍNCRONO para producto ${productId}`);

        // ✅ PASO 1: Calcular precio de Venta Total
        const resultadoPrecioVenta = calcularPrecioDeVentaSincronoRapido(row);
        if (!resultadoPrecioVenta) {
            console.error(`❌ Error en cálculo de precio de venta para producto ${productId}`);
            row.data('processing', false);
            return { success: false, error: "Error en cálculo de precio de venta" };
        }

        console.log(`✅ Secuencia completa finalizada para producto ${productId} `);

        return {
            success: true,
            precio: resultadoPrecioVenta
        };

    } catch (error) {
        console.error(`💥 Error general en cálculo síncrono ${productId}:`, error);
        return { success: false, error: error.message };
    } finally {
        row.data('processing', false);
    }
}

 // ✅ MEJORADA: Función cálculo de costo con mejor retorno de información
function calcularPrecioDeVentaSincronoRapido(row) {
    const productId = row.data('p-id');

    console.log(`💰 Calculando precio de venta total para producto ${productId}`);

    const $pvta = row.find('.input-pcd_pvta').text().trim();
    const pcdPVta = parseFloat($pvta.replace(/,/g, '')) || 0;
    // Recopilar datos
    const pcdPedidaRaw = row.find('.input-pcd_pedida').val();
    const pcd_pedida = parseFloat((pcdPedidaRaw || '').toString().replace(/,/g, '')) || 0;

    const datos = {
        p_id: productId,
        pcd_pedida: pcd_pedida,
        pcd_pvta: pcdPVta || 0
    };

    try {
        // ✅ ACTUALIZAR: Campo de costo sin efectos visuales
        const nuevoPrecioNum = pcd_pedida * pcdPVta;
        const nuevoPrecioDeVenta = nuevoPrecioNum; // número sin formato

        const campoTotal = row.find('.input-pcd_pvta_total');
        if (!campoTotal || campoTotal.length === 0) {
            console.warn('No se encontró elemento .input-pcd_pvta_total en la fila', productId);
            return false;
        }

        // Determinar tipo de precio si está disponible en la fila (data-tipo-precio) o usar 'Venta'
        const tipoPrecio = (row.data('tipo-precio') || 'Venta').toString();

        // Formatear con la misma lógica que GridHelper.FormatearPrecio
        const formatted = formatPrecio(nuevoPrecioDeVenta, tipoPrecio);

        // Si es un input usamos val(), si es un td usamos text()
        if (campoTotal.is('input, textarea, :input')) {
            campoTotal.val(formatted);
        } else {
            campoTotal.text(formatted);
        }

        // Actualizar total general de la tabla
        if (typeof actualizarTotalGeneralPedido === 'function') {
            actualizarTotalGeneralPedido();
        }

        console.log(`✅ Precio calculado rápidamente: ${nuevoPrecioDeVenta}`);

        // ✅ RETORNAR: Información del cálculo
        return {
            success: true,
            precio: nuevoPrecioDeVenta,
            datos: datos
        };

    } catch (error) {
        console.error(`💥 Error calculando precio rápido para ${productId}:`, error.message);
        return false;
    }
}

function activarSiguienteCampo(campoActual) {
    const $campoActual = $(campoActual);
    const $fila = $campoActual.closest('tr');
    const camposEditables = '.input-pcd_pedida';
    const $camposEnFila = $fila.find(camposEditables);
    const indiceActual = $camposEnFila.index($campoActual);

    let $siguienteCampo = null;
    if (indiceActual < $camposEnFila.length - 1) {
        $siguienteCampo = $camposEnFila.eq(indiceActual + 1);
    } else if ($fila.next('tr').length) {
        $siguienteCampo = $fila.next('tr').find(camposEditables).first();
    }

    $campoActual.prop('readonly', true).addClass('campo-readonly');

    if ($siguienteCampo && $siguienteCampo.length) {
        $siguienteCampo.prop('readonly', false).removeClass('campo-readonly');
        setTimeout(() => { $siguienteCampo[0].focus(); $siguienteCampo[0].select(); }, 0);
    }
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
                    <td colspan="9" class="text-center text-muted py-2">
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