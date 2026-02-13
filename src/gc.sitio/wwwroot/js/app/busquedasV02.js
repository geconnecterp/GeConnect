// Variables globales para búsqueda avanzada V02
var productosSeleccionadosBusqueda = [];
var dataBakV02 = {};

// Variables globales para definir el contexto de destino
var busquedaDestinoTipo = "ofertas"; // valores: "ofertas", "combos", "sustitutos", "presupuestos", "etiquetas"
var busquedaDestinoCallback = null;

/**
 * Configura el destino de los productos seleccionados en la búsqueda avanzada
 * @param {string} tipo - Tipo de destino: "ofertas", "combos" o "sustitutos"
 * @param {Function} callback - Función callback para procesar los productos
 * @param {Function} [validadorCallback] - Función opcional que devuelve IDs de productos existentes en el grid destino
 */
function configurarDestinoBusquedaProductos(tipo, callback, validadorCallback) {
    busquedaDestinoTipo = tipo || "ofertas";
    busquedaDestinoCallback = callback;
    busquedaValidadorCallback = validadorCallback || function() { return []; };
    
    // ✅ CRÍTICO: Limpiar selección previa al configurar nuevo destino
    limpiarSeleccionBusqueda();
    
    // ✅ ACTUALIZADO: Mapeo de títulos según tipo de destino
    const titulosModulos = {
        "ofertas": "Búsqueda Avanzada de Productos",
        "combos": "Selección de Productos para Combo",
        "sustitutos": "Selección de Productos Sustitutos",
        "presupuestos": "Selección de Productos para Presupuesto",
        "etiquetas": "Selección de Productos para Etiquetas"
    };
    
    const titulo = titulosModulos[tipo] || "Búsqueda Avanzada de Productos";
    $("#buscTitulo").text(titulo);
    
    console.log(`🔄 Destino configurado: ${tipo} - Productos seleccionados limpiados`);
}

$(function () {
    // Eventos base del modal de búsqueda
    $("button[type='button'].close.buscAdv").on("click", function () {
        $("#busquedaModal").modal("toggle");
        limpiarSeleccionBusqueda();
    });

    // ✅ ACTUALIZADO: Eventos de inputs con IDs B2
    $("input#Rel01B2").on("click", function () {
        $(this).val("");
        $("#Rel01B2Item").val("");
    });

    $("input#Rel02B2").on("click", function () {
        $(this).val("");
        $("#Rel02B2Item").val("");
    });

    // ✅ ACTUALIZADO: Eliminar items de listas con IDs B2
    $("#Rel01B2List").on("dblclick", 'option', function () { $(this).remove(); });
    $("#Rel02B2List").on("dblclick", 'option', function () { $(this).remove(); });

    // Usar trigger en lugar del método deprecado
    $("input").on("focus", function () {
        $(this).trigger("select");
    });

    // Botón de búsqueda
    $("#btnBuscarProd").off("click").on("click", function () {
        buscarAvUIStart();
        busquedaAvanzadaProductosV02(pagina);
    });

    // Paginación
    $("#pagEstado").on("change", function () {
        var div = $("#divPaginacionAdv");
        presentaPaginacion(div);
    });
    
    // Evento Enter en campo Search
    $("#Search").off("keydown").on("keydown", function (e) {
        if (e.key === "Enter") {
            e.preventDefault();
            const valor = $(this).val().trim();
            if (valor) {
                $("#btnBuscarProd").trigger("click");
            }
        }
    });

    // ✅ NUEVO: Configurar autocompletado genérico para Rel01B2 (Proveedor)
    // Se usa delegación de eventos para que funcione incluso si el modal se carga dinámicamente
    $(document).on("focus", "#busquedaModal #Rel01B2", function() {
        var $input = $(this);
        
        // Verificar si ya está inicializado para evitar duplicaciones
        if ($input.data("autocomplete-initialized")) {
            return;
        }
        
        // Verificar que la URL esté definida globalmente
        if (typeof autoComRel01Url === 'undefined') {
            console.error("❌ autoComRel01Url no está definida. Debe definirse en la vista que invoca el modal.");
            return;
        }
        
        console.log("🔧 Inicializando autocompletado para Rel01B2 (Proveedor)");
        
        $input.autocomplete({
            source: function (request, response) {
                $.ajax({
                    url: autoComRel01Url,
                    type: "POST",
                    dataType: "json",
                    data: { prefix: request.term },
                    success: function (obj) {
                        if (!obj || obj.length === 0) {
                            response([{ label: "No se encontraron proveedores", value: "", disabled: true }]);
                            return;
                        }
                        response($.map(obj, function (item) {
                            var texto = item.descripcion;
                            return { label: texto, value: item.descripcion, id: item.id, prov: item.provId, tipo: "P" };
                        }));
                        //response($.map(data, function (item) {
                        //    return {
                        //        label: item.cta_id + " - " + item.cta_denominacion,
                        //        value: item.cta_denominacion,
                        //        id: item.cta_id
                        //    };
                        //}));
                    },
                    error: function (xhr, status, error) {
                        console.error("❌ Error en autocompletado de proveedores:", error);
                        response([]);
                    }
                });
            },
            minLength: 3,
            select: function (event, ui) {
                // Evitar selección de opciones deshabilitadas
                if (ui.item.disabled) {
                    return false;
                }
                
                $("#Rel01B2").val(ui.item.value);
                $("#Rel01B2Item").val(ui.item.id);
                
                // ✅ CRÍTICO: Disparar evento personalizado para que módulos externos reaccionen
                // Esto permite que combo.js cargue las familias sin duplicar código
                $("#busquedaModal #Rel01B2").trigger("autocompleteselect", [ui]);
                
                console.log(`✅ Proveedor seleccionado: ${ui.item.id} - ${ui.item.value}`);
                
                return false;
            },
            focus: function (event, ui) {
                if (ui.item.disabled) {
                    return false;
                }
                $("#Rel01B2").val(ui.item.value);
                return false;
            }
        });
        
        // Marcar como inicializado
        $input.data("autocomplete-initialized", true);
        
        console.log("✅ Autocompletado para Rel01B2 inicializado correctamente");
    });

    // Callback para paginación
    funcCallBack = busquedaAvanzadaProductosV02;

    return true;
});

// ✅ COMPLETAMENTE OPTIMIZADA: Función con todos los IDs B2
function busquedaAvanzadaProductosV02(pag) {
    const ri01 = $("#Rel01B2Item").val() || "";
    const ri02 = $("#Rel02B2Item").val() || "";
    const ri03 = $("#Rel03B2 option:selected").val() || "%";
    const act = $("#chkActivos").is(":checked");
    const dis = $("#chkDisc").is(":checked");
    const ina = $("#chkInact").is(":checked");
    
    let cstk = true;
    let sstk = true;

    if ($("#rdConStk").is(":checked") || $("#rdSinStk").is(":checked")) {
        sstk = $("#rdSinStk").is(":checked");
        cstk = !sstk;
    }

    const buscar = $("#Search").val() || "";
    const data1 = { ri01, ri02, ri03, act, dis, ina, cstk, sstk, buscar, lp_id: admLp_id };

    const buscaNew = JSON.stringify(dataBakV02) !== JSON.stringify(data1);
    
    if (!buscaNew) {
        pagina = pag;
    } else {
        dataBakV02 = data1;
        pagina = 1;
        pag = 1;
        // ✅ CRÍTICO: Limpiar selección al iniciar nueva búsqueda
        limpiarSeleccionBusqueda();
    }

    const sort = "p_desc";
    const sortDir = "asc";
    const data2 = { sort, sortDir, pag, buscaNew };
    const data = $.extend({}, data1, data2);

    const urlBusqueda = busquedaAvanzadaUrl;
    
    PostGen(data, urlBusqueda, function (response) {
        try { buscarAvUIStop(); } catch (e) { }

        if (response.error) {
            ControlaMensajeError(response.msg || "Error en búsqueda");
            return;
        }

        // Validación de metadata con valores por defecto
        const metadata = response.metadata || {
            totalCount: response.productos ? response.productos.length : 0,
            totalPages: 1,
            currentPage: pag,
            pageSize: response.productos ? response.productos.length : 0
        };

        const htmlGrid = generarGridDesdeProductoListaDto(response.productos, metadata);
        $("#divBusquedaAvanzada").html(htmlGrid);
        
        configurarEventosGridBusquedaV02();
        
        // ✅ CRÍTICO: Restaurar estado de checkboxes basado en selección actual
        restaurarEstadoCheckboxes();

        if (response.metadata) {
            totalRegs = response.metadata.totalCount;
            pags = response.metadata.totalPages;
            pagRegs = response.metadata.pageSize;
            $("#pagEstado").val(true).trigger("change");
        }
    }, function (error) {
        try { buscarAvUIStop(); } catch (e) { }
        ControlaMensajeError("Error en búsqueda avanzada: " + (error.message || "Error desconocido"));
    });

    return true;
}

// Función para obtener productos seleccionados (API pública)
function obtenerProductosSeleccionados() {
    return {
        productos: productosSeleccionadosBusqueda,
        cantidad: productosSeleccionadosBusqueda.length,
        haySeleccion: productosSeleccionadosBusqueda.length > 0
    };
}

// OPTIMIZADO: Eventos con delegación sobre el contenedor para evitar re-bind por recarga del grid
function configurarEventosGridBusquedaV02() {
    const $contenedor = $("#divBusquedaAvanzada");

    // Seleccionar/Deseleccionar todos
    $contenedor.off("change", "#checkAllBusqueda").on("change", "#checkAllBusqueda", function () {
        const isChecked = this.checked;
        const $checks = $contenedor.find(".check-producto-busqueda").prop("checked", isChecked);

        $checks.each(function () {
            const productoData = $(this).data("producto");
            if (!productoData) return;
            if (isChecked) {
                agregarProductoASeleccion(productoData);
            } else {
                removerProductoDeSeleccion(productoData.p_id);
            }
        });

        actualizarContadorSeleccion();
    });

    // Selección individual
    $contenedor.off("change", ".check-producto-busqueda").on("change", ".check-producto-busqueda", function () {
        const productoData = $(this).data("producto");
        if (!productoData) return;

        if (this.checked) {
            agregarProductoASeleccion(productoData);
        } else {
            removerProductoDeSeleccion(productoData.p_id);
        }

        const totalVisible = $contenedor.find(".check-producto-busqueda").length;
        const checkedVisible = $contenedor.find(".check-producto-busqueda:checked").length;
        $("#checkAllBusqueda").prop("checked", totalVisible > 0 && totalVisible === checkedVisible);

        actualizarContadorSeleccion();
    });

    // Botones del panel múltiple (están fuera del contenedor, se mantienen binds directos)
    $("#btnAgregarSeleccionados").off("click").on("click", function () {
        agregarProductosSeleccionadosAOfertas();
    });
    $("#btnLimpiarSeleccionBusqueda").off("click").on("click", function () {
        confirmarLimpiezaSeleccion();
    });
}

// ✅ MEJORADA: Auto-selección optimizada para ProductoListaDto
function autoSeleccionarProductosVisibles() {
    $(".check-producto-busqueda").each(function () {
        var checkbox = $(this);
        var productoData = checkbox.data("producto");
        
        if (productoData) {
            checkbox.prop("checked", true);
            
            // Solo verificar p_id
            if (!productosSeleccionadosBusqueda.some(p => p.p_id === productoData.p_id)) {
                agregarProductoASeleccion(productoData);
            }
        }
    });
    
    var totalVisible = $(".check-producto-busqueda").length;
    if (totalVisible > 0) {
        $("#checkAllBusqueda").prop("checked", true);
    }
    
    actualizarContadorSeleccion();
}

// Gestión de productos seleccionados
function agregarProductoASeleccion(producto) {
    var productoId = producto.p_id || producto.P_id;
    
    if (!productosSeleccionadosBusqueda.some(p => (p.p_id || p.P_id) === productoId)) {
        productosSeleccionadosBusqueda.push(producto);
        console.log(`✅ Producto agregado a selección: ${productoId}`);
    }
}

function removerProductoDeSeleccion(productoId) {
    const longitudAntes = productosSeleccionadosBusqueda.length;
    productosSeleccionadosBusqueda = productosSeleccionadosBusqueda.filter(p => 
        (p.p_id || p.P_id) !== productoId
    );
    const longitudDespues = productosSeleccionadosBusqueda.length;
    
    if (longitudAntes !== longitudDespues) {
        console.log(`🗑️ Producto removido de selección: ${productoId}`);
    }
}

// ✅ OPTIMIZADA: Contador que refleja selección automática
function actualizarContadorSeleccion() {
    const cantidad = productosSeleccionadosBusqueda.length;
    
    $("#contadorSeleccionados").text(cantidad);
    $("#badgeSeleccionados").text(cantidad + " seleccionados");
    $("#badgeSeleccionadosHeader").text(cantidad + " seleccionados");

    if (cantidad > 0) {
        $("#seccionSeleccionMultiple").show();
    } else {
        $("#seccionSeleccionMultiple").hide();
    }
}

// ✅ CRÍTICA: Función mejorada para limpiar selección
function limpiarSeleccionBusqueda() {
    console.log(`🧹 Limpiando selección. Productos antes: ${productosSeleccionadosBusqueda.length}`);
    
    // Limpiar array
    productosSeleccionadosBusqueda = [];
    
    // Limpiar checkboxes visibles
    $(".check-producto-busqueda").prop("checked", false);
    $("#checkAllBusqueda").prop("checked", false);
    
    // Actualizar contador
    actualizarContadorSeleccion();
    
    console.log(`✅ Selección limpiada. Productos después: ${productosSeleccionadosBusqueda.length}`);
}

// ✅ OPTIMIZADA: Restaurar estado sin color de filas
function restaurarEstadoCheckboxes() {
    $(".check-producto-busqueda").each(function () {
        const productoData = $(this).data("producto");
        if (productoData) {
            const estaSeleccionado = productosSeleccionadosBusqueda.some(p => p.p_id === productoData.p_id);
            $(this).prop("checked", estaSeleccionado);
        }
    });

    const totalVisible = $(".check-producto-busqueda").length;
    const checkedVisible = $(".check-producto-busqueda:checked").length;
    $("#checkAllBusqueda").prop("checked", totalVisible > 0 && totalVisible === checkedVisible);
}

// Funciones compatibles con busquedas.js original
function selectRegDbl(x) {
    // Limpiar selección previa
    $("#tbGridBusquedaProductos tbody tr").removeClass("selected-row");

    // Seleccionar fila actual
    $(x).addClass("selected-row");

    // Obtener ID del producto (primera celda visible después del checkbox)
    const id = x.cells[1].innerText.trim();

    // Cerrar modal y buscar producto
    $("#busquedaModal").modal("toggle");
    $("input#Busqueda").val(id);
    $("#btnBusquedaBase").trigger("click");
}

// ✅ OPTIMIZADA: Función helper para focus que evita métodos deprecados
function enfocarElementoSeguro(selector) {
    try {
        const elemento = $(selector);
        if (elemento.length > 0) {
            elemento.trigger("focus");
            return true;
        }
    } catch (error) {
        console.warn("Error al enfocar elemento:", selector, error);
    }
    return false;
}

// ✅ COMPLETAMENTE ACTUALIZADA: Inicialización con todos los IDs B2
function inicializaBusquedaAvanzadaV02() {
    console.log("🔄 Inicializando búsqueda avanzada V02...");
    
    // ✅ CRÍTICO: Limpiar selección al inicializar
    limpiarSeleccionBusqueda();

    // Configurar proveedor (Rel01B2)
    if (typeof provUnico !== 'undefined' && provUnico === true) {
        $("#Rel01B2").val(provDesc).prop("disabled", true);
        $("#Rel01B2Item").val(provId);
    } else {
        $("#Rel01B2").val("").prop("disabled", false);
        $("#Rel01B2Item").val("");
    }

    // ✅ ACTUALIZADO: Configurar rubros (Rel02B2)
    if (typeof rubUnico !== 'undefined' && rubUnico === true) {
        $("#Rel02B2").val(rubDesc).prop("disabled", true);
        $("#Rel02B2Item").val(rubId);
    } else {
        $("#Rel02B2").val("").prop("disabled", false);
        $("#Rel02B2Item").val("");
    }

    // ✅ NUEVO: Configurar familia (Rel03B2)
    if (typeof famUnico !== 'undefined' && famUnico === true) {
        $("#Rel03B2").val(famId).prop("disabled", true);
        $("#Rel03B2Item").val(famId);
    } else {
        $("#Rel03B2").val("").prop("disabled", false);
        $("#Rel03B2Item").val("");
    }

    // Configurar estados
    if (typeof estadoUnico !== 'undefined' && estadoUnico === true) {
        $("#chkActivos").prop("checked", estActivo).prop("disabled", true);
        $("#chkDisc").prop("checked", estDiscon).prop("disabled", true);
        $("#chkInact").prop("checked", estInacti).prop("disabled", true);
    } else {
        $("#chkActivos").prop("checked", true).prop("disabled", false);
        $("#chkDisc").prop("checked", false).prop("disabled", false);
        $("#chkInact").prop("checked", false).prop("disabled", false);
    }

    console.log("✅ Búsqueda avanzada V02 inicializada");
    return true;
}

// NUEVA: Función auxiliar para generar metadata y controles
function generarSeccionMetadataYControles(cantidadProductos, metadata) {
    return `
        <div class="d-flex justify-content-between align-items-center mt-3 px-2">
            <div class="text-muted small">
                <i class="bx bx-package me-1"></i>
                Total: ${metadata.totalCount} productos encontrados
            </div>
            <div class="text-muted small text-center">
                <span class="badge bg-golden-light" id="badgeSeleccionados">
                    ${cantidadProductos} seleccionados
                </span>
            </div>
            <div class="text-muted small">
                Página ${metadata.currentPage} de ${metadata.totalPages}
            </div>
        </div>
        <div id="seccionSeleccionMultiple" class="row mt-3" style="display: none;">
            <div class="col-12">
                <div class="card border-primary">
                    <div class="card-header bg-primary text-white">
                        <h6 class="mb-0">
                            <i class="bx bx-check-square me-2"></i>
                            Productos Seleccionados
                            <span class="badge bg-light text-primary ms-2" id="badgeSeleccionadosHeader">0 seleccionados</span>
                        </h6>
                    </div>
                    <div class="card-body">
                        <div class="row align-items-center">
                            <div class="col-md-8">
                                <p class="mb-0">
                                    Has seleccionado <strong id="contadorSeleccionados">0</strong> productos para agregar.
                                </p>
                            </div>
                            <div class="col-md-4 text-end">
                                <button type="button" class="btn btn-success me-2" id="btnAgregarSeleccionados">
                                    <i class="bx bx-plus-circle me-1"></i>
                                    Agregar Seleccionados
                                </button>
                                <button type="button" class="btn btn-outline-secondary" id="btnLimpiarSeleccionBusqueda">
                                    <i class="bx bx-x me-1"></i>
                                    Limpiar
                                </button>
                            </div>
                        </div>
                    </div>
                </div>
            </div>
        </div>
    `;
}

// NUEVA: Función para validar y normalizar datos antes de envío
function validarYNormalizarProducto(producto) {
    if (!producto.p_id) {
        console.warn("Producto sin ID válido:", producto);
        return null;
    }
    
    return producto;
}

// NUEVA: Aplicar validación en agregado de productos
function validarProductosAntesDeEnvio(productos) {
    const productosValidos = [];
    let productosInvalidos = 0;
    
    productos.forEach(function(producto) {
        const productoValidado = validarYNormalizarProducto(producto);
        if (productoValidado) {
            productosValidos.push(productoValidado);
        } else {
            productosInvalidos++;
        }
    });
    
    if (productosInvalidos > 0) {
        console.warn(`${productosInvalidos} productos no pudieron ser validados y fueron omitidos`);
    }
    
    return productosValidos;
}

// REINTRODUCIDO + OPTIMIZADO: Genera el grid HTML desde un array de ProductoListaDto
function generarGridDesdeProductoListaDto(productos, metadata) {
    const lista = Array.isArray(productos) ? productos : [];
    const meta = metadata || { totalCount: lista.length, totalPages: 1, currentPage: 1, pageSize: lista.length };

    if (lista.length === 0) {
        return `
            <div class="text-center text-muted py-4">
                <i class="bx bx-info-circle me-2"></i>
                No se encontraron productos con los criterios especificados
            </div>
        `;
    }

    // Construcción de filas performante
    const filas = lista.map((item, index) => {
        const claseAlternada = index % 2 === 0 ? "table-row-alt" : "";
        const estadoActivo = item.p_activo === "S";
        const estadoBadge = estadoActivo ? "bg-success" : "bg-warning";
        const estadoTexto = estadoActivo ? "Activo" : "Inactivo";

        const pcostoFormateado = formatearNumeroConCultura(item.p_pcosto || 0, 3);
        const pmayoristaFormateado = formatearNumeroConCultura(item.p_pvta_001 || 0, 2);
        const pminoristaFormateado = formatearNumeroConCultura(item.p_pvta_002 || 0, 2);

        const dataProductoJson = JSON.stringify(item).replace(/'/g, "&apos;");

        return `
            <tr class="${claseAlternada} fila-producto" data-producto-id="${item.p_id}">
                <td class="text-center">
                    <input type="checkbox" 
                           class="form-check-input check-producto-busqueda"
                           data-p-id="${item.p_id}"
                           data-producto='${dataProductoJson}'
                           title="Producto seleccionable">
                </td>
                <td class="text-center">${item.p_id}</td>
                <td class="text-left" title="${item.p_desc || ''}">${item.p_desc || ''}</td>
                <td class="text-center">${item.p_id_barrado || ''}</td>
                <td class="text-right">$${pcostoFormateado}</td>
                <td class="text-right">$${pmayoristaFormateado}</td>
                <td class="text-right">$${pminoristaFormateado}</td>
                <td class="text-center" title="${item.cta_denominacion || ''}">${item.cta_id || ''}</td>
                <td class="text-center">
                    <span class="badge ${estadoBadge}">${estadoTexto}</span>
                </td>
                <td class="text-center">
                    <button type="button" class="btn btn-outline-primary btn-sm btn-seleccionar-individual"
                            onclick="selectRegDbl(this.closest('tr'))" title="Usar solo este producto">
                        <i class="bx bx-check-circle"></i>
                    </button>
                </td>
            </tr>
        `;
    }).join("");

    return `
        <div class="table-responsive text-nowrap table-wrapper-400">
            <table class="table table-sm mb-0 table-hover table-golden" id="tbGridBusquedaProductos">
                <thead class="table-golden-header">
                    <tr class="header">
                        <th class="text-center"><input type="checkbox" class="form-check-input" id="checkAllBusqueda"></th>
                        <th class="text-center">ID</th>
                        <th class="text-left">DESCRIPCIÓN</th>
                        <th class="text-center">CÓDIGO EAN</th>
                        <th class="text-right">P.COSTO</th>
                        <th class="text-right">P.MAYORISTA</th>
                        <th class="text-right">P.MINORISTA</th>
                        <th class="text-center">PROVEEDOR</th>
                        <th class="text-center">ESTADO</th>
                        <th class="text-center">ACCIÓN</th>
                    </tr>
                </thead>
                <tbody>${filas}</tbody>
            </table>
        </div>
        ${generarSeccionMetadataYControles(lista.length, meta)}
    `;
}

// MANTENER: Funciones de formateo
function formatearNumeroConCultura(numero, decimales = 2) {
    if (numero === null || numero === undefined || isNaN(numero)) {
        return "0" + ",".repeat(decimales > 0 ? 1 : 0) + "0".repeat(decimales);
    }
    
    const num = parseFloat(numero);
    return num.toLocaleString('es-AR', {
        minimumFractionDigits: decimales,
        maximumFractionDigits: decimales,
        useGrouping: true
    });
}

function parsearNumeroConCultura(valorTexto) {
    if (!valorTexto || valorTexto.trim() === '') return 0;
    
    let valor = valorTexto.toString().replace(/[$\s]/g, '');
    valor = valor.replace(/\./g, '').replace(/,/g, '.' );
    
    const numero = parseFloat(valor);
    return isNaN(numero) ? 0 : numero;
}

// ACTUALIZADA: Función para agregar productos con ProductoListaDto
function agregarProductosSeleccionadosAOfertas() {
    if (productosSeleccionadosBusqueda.length === 0) {
        ControlaMensajeWarning("Debe seleccionar al menos un producto");
        return;
    }

    let mensaje;
    let titulo;
    
    switch (busquedaDestinoTipo) {
        case "etiquetas":
            titulo = "CONFIRMAR AGREGADO A ETIQUETAS";
            mensaje = `¿Desea agregar ${productosSeleccionadosBusqueda.length} productos a las etiquetas?`;
            if (productosSeleccionadosBusqueda.length === 1) {
                const descripcion = productosSeleccionadosBusqueda[0].p_desc;
                mensaje = `¿Desea agregar el producto "${descripcion}" a las etiquetas?`;
            }
            break;
        case "presupuestos":
            titulo = "CONFIRMAR AGREGADO A PRESUPUESTO";
            mensaje = `¿Desea agregar ${productosSeleccionadosBusqueda.length} productos al presupuesto?`;
            if (productosSeleccionadosBusqueda.length === 1) {
                const descripcion = productosSeleccionadosBusqueda[0].p_desc;
                mensaje = `¿Desea agregar el producto "${descripcion}" al presupuesto?`;
            }
            break;
        case "sustitutos":
            titulo = "CONFIRMAR SUSTITUTOS";
            mensaje = `¿Desea agregar ${productosSeleccionadosBusqueda.length} productos como sustitutos?`;
            if (productosSeleccionadosBusqueda.length === 1) {
                const descripcion = productosSeleccionadosBusqueda[0].p_desc;
                mensaje = `¿Desea agregar el producto "${descripcion}" como sustituto?`;
            }
            break;
        case "combos":
            titulo = "CONFIRMAR AGREGADO A COMBO";
            mensaje = `¿Desea agregar ${productosSeleccionadosBusqueda.length} productos al combo?`;
            if (productosSeleccionadosBusqueda.length === 1) {
                const descripcion = productosSeleccionadosBusqueda[0].p_desc;
                mensaje = `¿Desea agregar el producto "${descripcion}" al combo?`;
            }
            break;
        default:
            titulo = "CONFIRMAR AGREGADO";
            mensaje = `¿Desea agregar ${productosSeleccionadosBusqueda.length} productos a las ofertas?`;
            if (productosSeleccionadosBusqueda.length === 1) {
                const descripcion = productosSeleccionadosBusqueda[0].p_desc;
                mensaje = `¿Desea agregar el producto "${descripcion}" a las ofertas?`;
            }
    }

    AbrirMensaje(
        titulo,
        mensaje,
        function (resp) {
            if (resp === "SI") {
                if ((busquedaDestinoTipo === "combos" ||
                    busquedaDestinoTipo === "sustitutos" ||
                    busquedaDestinoTipo === "presupuestos" ||
                    busquedaDestinoTipo === "etiquetas") &&
                    typeof busquedaDestinoCallback === 'function') {
                    procesarAgregarProductosCustom();
                } else {
                    procesarAgregarProductosMultiples();
                }
            }
            $("#msjModal").modal("hide");
        },
        true,
        ["Agregar", "Cancelar"],
        "info!",
        null
    );
}

/**
 * ✅ CRÍTICA: Procesa el agregado de productos mediante callback personalizado
 */
function procesarAgregarProductosCustom() {
    AbrirWaiting("Agregando productos...");
    
    try {
        const productosExistentesIds = typeof busquedaValidadorCallback === 'function' 
            ? busquedaValidadorCallback() 
            : [];
        
        let estadoProducto = 'A';
        if (busquedaDestinoTipo === "combos" || busquedaDestinoTipo === "sustitutos") {
            estadoProducto = 'P';
        }
        
        const productosSeleccionadosOriginales = [...productosSeleccionadosBusqueda];
        const productosFiltrados = productosSeleccionadosBusqueda.filter(producto => 
            !productosExistentesIds.includes(producto.p_id));
        
        const cantidadDuplicados = productosSeleccionadosOriginales.length - productosFiltrados.length;
        
        let productos;
        if (busquedaDestinoTipo === "etiquetas") {
            productos = productosFiltrados.map(function(producto) {
                return {
                    p_id: producto.p_id,
                    p_desc: producto.p_desc,
                    activo: estadoProducto
                };
            });
        } else {
            productos = productosFiltrados.map(function(producto) {
                return {
                    p_id: producto.p_id,
                    p_desc: producto.p_desc,
                    p_pcosto: parseFloat(producto.p_pcosto || 0),
                    lp_prevision_tot: parseFloat(producto.lp_prevision_tot),
                    lp_prevision_pin: parseFloat(producto.lp_prevision_pin),
                    p_margen: parseFloat(producto.p_margen),
                    p_pneto: parseFloat(producto.p_pneto),
                    p_pvta: parseFloat(producto.p_pvta),
                    in_alicuota: parseFloat(producto.in_alicuota),
                    iva_alicuota: parseFloat(producto.iva_alicuota),
                    iva_situacion: producto.iva_situacion,
                    cantidad: 1,
                    dto_porc: 0,
                    activo: estadoProducto
                };
            });
        }
        
        if (productos.length === 0) {
            CerrarWaiting();
            if (cantidadDuplicados > 0) {
                ControlaMensajeWarning(`Los ${cantidadDuplicados} producto(s) seleccionado(s) ya existen en el destino.`);
            } else {
                ControlaMensajeWarning("No hay productos para agregar.");
            }
            // ✅ CRÍTICO: Limpiar selección después de procesar
            limpiarSeleccionBusqueda();
            return;
        }
        
        if (typeof busquedaDestinoCallback === 'function') {
            busquedaDestinoCallback(productos);
        }
        
        $("#busquedaModal").modal("hide");
        
        const cantidadAgregada = productos.length;
        
        // ✅ CRÍTICO: Limpiar selección DESPUÉS de procesar exitosamente
        limpiarSeleccionBusqueda();
        
        CerrarWaiting();
        
        let mensaje = '';
        if (cantidadDuplicados > 0) {
            mensaje = `Se agregaron ${cantidadAgregada} producto(s). Se omitieron ${cantidadDuplicados} producto(s) duplicado(s).`;
        } else {
            const mensajesDestino = {
                "etiquetas": "a las etiquetas",
                "presupuestos": "al presupuesto",
                "sustitutos": "como sustituto",
                "combos": "al combo"
            };
            
            const destinoTexto = mensajesDestino[busquedaDestinoTipo] || "";
            
            if (busquedaDestinoTipo === "sustitutos") {
                mensaje = `${cantidadAgregada} producto${cantidadAgregada > 1 ? 's' : ''} agregado${cantidadAgregada > 1 ? 's' : ''} como sustituto${cantidadAgregada > 1 ? 's' : ''} correctamente`;
            } else {
                mensaje = `${cantidadAgregada} producto${cantidadAgregada > 1 ? 's' : ''} agregado${cantidadAgregada > 1 ? 's' : ''} ${destinoTexto} correctamente`;
            }
        }
        
        ControlaMensajeSuccess(mensaje);
    } catch (error) {
        CerrarWaiting();
        console.error("Error al procesar productos:", error);
        ControlaMensajeError("Error al agregar productos: " + error.message);
        // ✅ CRÍTICO: Limpiar selección en caso de error
        limpiarSeleccionBusqueda();
    }
}

// NUEVA: Función para confirmar limpieza de selección
function confirmarLimpiezaSeleccion() {
    if (productosSeleccionadosBusqueda.length === 0) {
        ControlaMensajeWarning("No hay productos seleccionados para limpiar");
        return;
    }

    AbrirMensaje(
        "CONFIRMAR LIMPIEZA",
        `¿Está seguro que desea limpiar la selección de ${productosSeleccionadosBusqueda.length} productos?`,
        function (resp) {
            if (resp === "SI") {
                limpiarSeleccionBusqueda();
                ControlaMensajeSuccess("Selección limpiada correctamente");
            }
            $("#msjModal").modal("hide");
            return true;
        },
        true,
        ["Limpiar", "Cancelar"],
        "warn!",
        null
    );
}

/**
 * ✅ COMPLETAMENTE ACTUALIZADA: Inicializa todos los controles con IDs B2
 */
function inicializarControlesBusquedaAvanzada() {
    console.log("🔄 Inicializando controles del modal de búsqueda avanzada...");

    // ✅ CRÍTICO: Limpiar selección al inicializar controles
    limpiarSeleccionBusqueda();

    $("#Rel01B2, #Rel02B2, #Search").val("").prop("disabled", false);
    $("#Rel01B2Item, #Rel02B2Item, #Rel03B2Item").val("");
    $("#Rel03B2").val("").prop("disabled", true);

    $("#chkActivos").prop("checked", true).prop("disabled", false);
    $("#chkDisc, #chkInact").prop("checked", false).prop("disabled", false);

    $("#rdConStk").prop("checked", true);
    $("#rdSinStk").prop("checked", false);

    $("#divBusquedaAvanzada").html(`
        <div class="text-center text-muted py-4">
            <i class="bx bx-info-circle me-2"></i>
            No se encontraron productos con los criterios especificados
        </div>
    `);

    $("#seccionSeleccionMultiple").hide();
    $("#pagEstado").val(false).trigger("change");

    try { buscarAvUIStop(); } catch (e) { }

    console.log("✅ Controles inicializados correctamente.");
}

/**
 * ✅ CRÍTICA: Configura el evento para abrir el modal de búsqueda avanzada
 */
function configurarAperturaModalBusquedaAvanzada() {
    console.log("🔧 Configurando apertura del modal de búsqueda avanzada...");

    // ✅ CRÍTICO: Limpiar selección al mostrar el modal
    $("#busquedaModal").on("show.bs.modal", function () {
        console.log("📂 Modal de búsqueda abierto - limpiando selección previa");
        inicializarControlesBusquedaAvanzada();
    });

    // ✅ CRÍTICO: Limpiar selección al ocultar el modal
    $("#busquedaModal").on("hidden.bs.modal", function () {
        console.log("📕 Modal de búsqueda cerrado - limpiando selección");
        limpiarSeleccionBusqueda();
    });

    console.log("✅ Apertura del modal configurada correctamente.");
}

// Configurar la apertura del modal al cargar el script
$(function () {
    configurarAperturaModalBusquedaAvanzada();
});

// Helpers UI de búsqueda avanzada (spinner y botón)
function buscarAvUIStart() {
    $("#btnBuscarProd").prop("disabled", true);
    $("#spnBuscarProd").removeClass("d-none");
}

function buscarAvUIStop() {
    $("#spnBuscarProd").addClass("d-none");
    $("#btnBuscarProd").prop("disabled", false);
}

// NUEVA: Procesar respuesta JSON de búsqueda individual
function procesarRespuestaBusquedaJSON(response, valorBuscado) {
    if (response.error) {
        ControlaMensajeError(response.msg || "Error en la búsqueda");
        return;
    }

    const productos = response.productos || [];

    if (productos.length === 0) {
        AbrirMensaje("ATENCIÓN", "NO SE ENCONTRÓ EL PRODUCTO QUE INTENTA BUSCAR.", function () {
            if (funcionBusquedaAvanzada === true) {
                inicializaBusquedaAvanzadaV02();
                $("#busquedaModal").modal("show");
            }
            $("#msjModal").modal("hide");
            enfocarElementoSeguro("#Busqueda");
            return true;
        }, false, ["Aceptar"], "warn!", null);

        return;
    }

    if (productos.length === 1) {
        const producto = productos[0];
        agregarProductoIndividualAOfertas(producto);
    } else {
        AbrirMensaje("ATENCIÓN",
            `Se encontraron ${productos.length} productos. Se abrirá la búsqueda avanzada para seleccionar.`,
            function () {
                $("#msjModal").modal("hide");
                $("#Search").val(valorBuscado);
                inicializaBusquedaAvanzadaV02();
                $("#busquedaModal").modal("show");
                setTimeout(function () {
                    busquedaAvanzadaProductosV02(1);
                }, 300);
                return true;
            }, false, ["Aceptar"], "info!", null);
    }
}