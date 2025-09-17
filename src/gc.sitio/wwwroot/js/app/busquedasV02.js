// Variables globales para búsqueda avanzada V02
var productosSeleccionadosBusqueda = [];
var dataBakV02 = {};

$(function () {
    // Eventos base del modal de búsqueda
    $("button[type='button'].close.buscAdv").on("click", function () {
        $("#busquedaModal").modal("toggle");
        limpiarSeleccionBusqueda();
    });

    // Eventos de inputs de relaciones
    $("input#Rel01").on("click", function () {
        $(this).val("");
        $("#Rel01Item").val("");
    });

    $("input#Rel02").on("click", function () {
        $(this).val("");
        $("#Rel02Item").val("");
    });

    // Eliminar items de listas
    $("#Rel01List").on("dblclick", 'option', function () { $(this).remove(); });
    $("#Rel02List").on("dblclick", 'option', function () { $(this).remove(); });

    // ✅ CORREGIDO: Usar trigger en lugar del método deprecado
    $("input").on("focus", function () {
        $(this).trigger("select");
    });

    // Botón de búsqueda
    $("#btnBuscarProd").on("click", function () { busquedaAvanzadaProductosV02(pagina); });

    // Paginación
    $("#pagEstado").on("change", function () {
        var div = $("#divPaginacionAdv");
        presentaPaginacion(div);
    });

    // Callback para paginación
    funcCallBack = busquedaAvanzadaProductosV02;

    return true;
});

// ✅ CORREGIDA: Función con validación de metadata
function busquedaAvanzadaProductosV02(pag) {
    let ri01 = $("#Rel01Item").val();
    let ri02 = $("#Rel02Item").val();
    let ri03 = $("#Rel03 option:selected").val() || "%";
    var act = $("#chkActivos").is(":checked");
    var dis = $("#chkDisc").is(":checked");
    var ina = $("#chkInact").is(":checked");
    var cstk = true;
    var sstk = true;

    if ($("#rdConStk").is(":checked") || $("#rdSinStk").is(":checked")) {
        if ($("#rdSinStk").is(":checked")) {
            sstk = true;
            cstk = false;
        } else {
            sstk = false;
            cstk = true;
        }
    }

    var buscar = $("#Search").val();
    var data1 = { ri01, ri02, ri03, act, dis, ina, cstk, sstk, buscar };

    var buscaNew = JSON.stringify(dataBakV02) != JSON.stringify(data1);
    if (buscaNew === false) {
        pagina = pag;
    } else {
        dataBakV02 = data1;
        pagina = 1;
        pag = 1;
        limpiarSeleccionBusqueda();
    }

    var sort = "p_desc";
    var sortDir = "asc";
    var data2 = { sort, sortDir, pag, buscaNew };
    var data = $.extend({}, data1, data2);

    var urlBusqueda = busquedaAvanzadaUrl;
    
    PostGen(data, urlBusqueda, function (response) {
        if (response.error) {
            ControlaMensajeError(response.msg || "Error en búsqueda");
            return;
        }

        // ✅ VALIDACIÓN: Metadata con valores por defecto
        var metadata = response.metadata || {
            totalCount: response.productos ? response.productos.length : 0,
            totalPages: 1,
            currentPage: pag,
            pageSize: response.productos ? response.productos.length : 0
        };

        // ✅ GENERAR HTML: Con metadata validada
        var htmlGrid = generarGridDesdeProductoListaDto(response.productos, metadata);
        $("#divBusquedaAvanzada").html(htmlGrid);
        
        configurarEventosGridBusquedaV02();

        // ✅ METADATA: Actualizar variables globales
        if (response.metadata) {
            totalRegs = response.metadata.totalCount;
            pags = response.metadata.totalPages;
            pagRegs = response.metadata.pageSize;
            $("#pagEstado").val(true).trigger("change");
        }
    }, function (error) {
        ControlaMensajeError("Error en búsqueda avanzada: " + (error.message || "Error desconocido"));
    });

    return true;
}

function buscarProducto() {
    AbrirWaiting();
    var valor = $("#Busqueda").val();

    // ✅ VALIDACIÓN TEMPRANA: Sin abrir indicador de carga innecesariamente
    // ✅ VALIDACIÓN TEMPRANA: Sin abrir indicador de carga innecesariamente
    if (valor.trim() === "") {
        CerrarWaiting(); // ✅ IMPORTANTE: Cerrar indicador de carga
        inicializaBusquedaAvanzadaV02();
        $("#busquedaModal").modal("show");
        return; // ✅ CRÍTICO: Detener ejecución aquí
    }

    // ✅ CONTINÚA: Solo si hay valor para buscar
    var urlBusqueda = busquedaAvanzadaUrl;
    
    var datos = {
        ri01: "",
        ri02: "",
        act: true,
        dis: false,
        ina: false,
        cstk: true,
        sstk: false,
        buscar: valor,
        sort: "p_desc",
        sortDir: "asc",
        pag: 1,
        buscaNew: true
    };

    PostGen(datos, urlBusqueda, function (response) {
        CerrarWaiting();
        procesarRespuestaBusquedaJSON(response, valor);
    }, function (error) {
        CerrarWaiting();
        ControlaMensajeError("Error en la búsqueda: " + (error.message || "Error desconocido"));
    });
    
    return true;
}

// ✅ OPTIMIZADA: Configurar eventos con auto-selección mejorada
// ✅ SIMPLIFICADA: Solo usar p_id en eventos
function configurarEventosGridBusquedaV02() {
    autoSeleccionarProductosVisibles();
    
    $("#checkAllBusqueda").off("change").on("change", function () {
        var isChecked = $(this).is(":checked");
        $(".check-producto-busqueda").prop("checked", isChecked);

        $(".check-producto-busqueda").each(function () {
            var productoData = $(this).data("producto");
            if (productoData) {
                if (isChecked) {
                    agregarProductoASeleccion(productoData);
                } else {
                    // ✅ SOLO p_id
                    removerProductoDeSeleccion(productoData.p_id);
                }
            }
        });

        actualizarContadorSeleccion();
    });

    $(".check-producto-busqueda").off("change").on("change", function () {
        var productoData = $(this).data("producto");
        if (!productoData) return;

        if ($(this).is(":checked")) {
            agregarProductoASeleccion(productoData);
        } else {
            // ✅ SOLO p_id
            removerProductoDeSeleccion(productoData.p_id);
        }

        var totalVisible = $(".check-producto-busqueda").length;
        var checkedVisible = $(".check-producto-busqueda:checked").length;
        $("#checkAllBusqueda").prop("checked", totalVisible === checkedVisible);

        actualizarContadorSeleccion();
    });

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
    // ✅ USAR CAMPO p_id: Con fallback a P_id
    var productoId = producto.p_id || producto.P_id;
    
    if (!productosSeleccionadosBusqueda.some(p => (p.p_id || p.P_id) === productoId)) {
        productosSeleccionadosBusqueda.push(producto);
    }
}

function removerProductoDeSeleccion(productoId) {
    // ✅ SIMPLIFICADO: Comparar con ambos campos pero preferir p_id
    productosSeleccionadosBusqueda = productosSeleccionadosBusqueda.filter(p => 
        (p.p_id || p.P_id) !== productoId
    );
}

// ✅ OPTIMIZADA: Contador que refleja selección automática
function actualizarContadorSeleccion() {
    var cantidad = productosSeleccionadosBusqueda.length;
    
    // ✅ ACTUALIZAR: Todos los elementos de contador
    $("#contadorSeleccionados").text(cantidad);
    $("#badgeSeleccionados").text(cantidad + " seleccionados automáticamente");
    $("#badgeSeleccionadosHeader").text(cantidad + " seleccionados");

    // Mostrar sección de selección múltiple siempre que haya productos
    if (cantidad > 0) {
        $("#seccionSeleccionMultiple").show();
    } else {
        $("#seccionSeleccionMultiple").hide();
    }
}

function limpiarSeleccionBusqueda() {
    productosSeleccionadosBusqueda = [];
    $(".check-producto-busqueda").prop("checked", false);
    $("#checkAllBusqueda").prop("checked", false);
    actualizarContadorSeleccion();
}

// ✅ OPTIMIZADA: Restaurar estado sin color de filas
function restaurarEstadoCheckboxes() {
    $(".check-producto-busqueda").each(function () {
        var productoData = $(this).data("producto");
        if (productoData) {
            // ✅ SOLO p_id
            var estaSeleccionado = productosSeleccionadosBusqueda.some(p => p.p_id === productoData.p_id);
            $(this).prop("checked", estaSeleccionado);
        }
    });

    var totalVisible = $(".check-producto-busqueda").length;
    var checkedVisible = $(".check-producto-busqueda:checked").length;
    $("#checkAllBusqueda").prop("checked", totalVisible > 0 && totalVisible === checkedVisible);
}

// Funciones compatibles con busquedas.js original
function selectRegDbl(x) {
    // Limpiar selección previa
    $("#tbGridBusquedaProductos tbody tr").each(function (index) {
        $(this).removeClass("selected-row");
    });

    // Seleccionar fila actual
    $(x).addClass("selected-row");

    // Obtener ID del producto (primera celda visible después del checkbox)
    var id = x.cells[1].innerText.trim();

    // Cerrar modal y buscar producto
    $("#busquedaModal").modal("toggle");
    $("input#Busqueda").val(id);
    $("#btnBusquedaBase").trigger("click");
}

// ✅ OPTIMIZADA: Función helper para focus que evita métodos deprecados
function enfocarElementoSeguro(selector) {
    try {
        var elemento = $(selector);
        if (elemento.length > 0) {
            elemento.trigger("focus");
            return true;
        }
    } catch (error) {
        console.warn("Error al enfocar elemento:", selector, error);
    }
    return false;
}

// ✅ ACTUALIZADA: Función de inicialización corregida
function inicializaBusquedaAvanzadaV02() {
    // Limpiar selección al inicializar
    limpiarSeleccionBusqueda();

    // Configurar proveedor
    if (typeof provUnico !== 'undefined' && provUnico === true) {
        $("input#Rel01").val(provDesc).prop("disabled", true);
        $("input#Rel01Item").val(provId);
    } else {
        $("input#Rel01").val("").prop("disabled", false);
        $("input#Rel01Item").val("");
    }

    // Configurar rubros
    if (typeof rubUnico !== 'undefined' && rubUnico === true) {
        $("input#Rel02").val(rubDesc).prop("disabled", true);
        $("input#Rel02Item").val(rubId);
    } else {
        $("input#Rel02").val("").prop("disabled", false);
        $("input#Rel02Item").val("");
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

// ✅ NUEVA: Función auxiliar para generar metadata y controles
function generarSeccionMetadataYControles(cantidadProductos, metadata) {
    return `
        <div class="d-flex justify-content-between align-items-center mt-3 px-2">
            <div class="text-muted small">
                <i class="bx bx-package me-1"></i>
                Total: ${metadata.totalCount} productos encontrados
            </div>
            <div class="text-muted small text-center">
                <span class="badge bg-golden-light" id="badgeSeleccionados">
                    ${cantidadProductos} seleccionados automáticamente
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
                                    Has seleccionado <strong id="contadorSeleccionados">0</strong> productos para agregar a las ofertas.
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

// ✅ NUEVA: Función para validar y normalizar datos antes de envío
function validarYNormalizarProducto(producto) {
    if (!producto.p_id) {
        console.warn("Producto sin ID válido:", producto);
        return null;
    }
    
    // ✅ RETORNO DIRECTO: Sin normalización de campos numéricos
    return producto;
}

// ✅ NUEVA: Aplicar validación en agregado de productos
function validarProductosAntesDeEnvio(productos) {
    var productosValidos = [];
    var productosInvalidos = 0;
    
    productos.forEach(function(producto) {
        var productoValidado = validarYNormalizarProducto(producto);
        if (productoValidado) {
            productosValidos.push(productoValidado);
        } else {
            productosInvalidos++;
        }
    });
    
    if (productosInvalidos > 0) {
        console.warn(`${productosInvalidos} productos no pudieron ser validados y fueronomitidos`);
    }
    
    return productosValidos;
}

// ✅ NUEVA: Función para auto-seleccionar productos al cargar grid
function autoSeleccionarProductosVisibles() {
    // Seleccionar todos los productos que aparecen en el grid
    $(".check-producto-busqueda").each(function () {
        var checkbox = $(this);
        var productoData = checkbox.data("producto");
        
        if (productoData) {
            // Marcar checkbox como seleccionado (ya viene checked del HTML)
            checkbox.prop("checked", true);
            
            // Agregar al array de seleccionados si no existe
            agregarProductoASeleccion(productoData);
        }
    });
    
    // Marcar "Seleccionar todos" si hay productos
    var totalVisible = $(".check-producto-busqueda").length;
    if (totalVisible > 0) {
        $("#checkAllBusqueda").prop("checked", true);
    }
    
    // Actualizar contador
    actualizarContadorSeleccion();
}

// ✅ OPTIMIZADA: Función para selección individual sin color
function selectRegDbl(x) {
    var id = x.cells[1].innerText.trim();
    var descripcion = x.cells[2].innerText.trim();
    
    AbrirMensaje(
        "USAR PRODUCTO INDIVIDUAL",
        `¿Desea usar únicamente el producto "${descripcion}"?<br><small>Se limpiará la selección múltiple actual.</small>`,
        function (resp) {
            if (resp === "SI") {
                // ✅ EXTRAER Y ENVIAR: Producto desde la fila actual
                var productoData = extraerProductoListaDtoDeFilaHTML($(x));
                if (productoData) {
                    $("#busquedaModal").modal("hide");
                    agregarProductoIndividualAOfertas(productoData);
                } else {
                    // Fallback al método original si no se puede extraer
                    $("#busquedaModal").modal("toggle");
                    $("input#Busqueda").val(id);
                    $("#btnBusquedaBase").trigger("click");
                }
            }
            $("#msjModal").modal("hide");
            return true;
        },
        true,
        ["Usar Individual", "Cancelar"],
        "info!",
        null
    );
}

// ✅ NUEVA: Función para procesar resultado de búsqueda individual usando ProductoListaDto
function procesarRespuestaBusquedaIndividual(htmlResponse, valorBuscado) {
    var $tempContainer = $('<div>').html(htmlResponse);
    var $filas = $tempContainer.find('#tbGridProd tbody tr[data-producto-id]');
    
    if ($filas.length === 0) {
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
    
    if ($filas.length === 1) {
        var $fila = $filas.first();
        var productoData = extraerProductoListaDtoDeFilaHTML($fila);
        
        if (productoData) {
            // ✅ ENVÍO DIRECTO: Sin conversión, usar ProductoListaDto tal como viene
            agregarProductoIndividualAOfertas(productoData);
        }
    } else {
        AbrirMensaje("ATENCIÓN", 
            `Se encontraron ${$filas.length} productos. Se abrirá la búsqueda avanzada para seleccionar.`, 
            function () {
                $("#msjModal").modal("hide");
                $("#Search").val(valorBuscado);
                inicializaBusquedaAvanzadaV02();
                $("#busquedaModal").modal("show");
                setTimeout(function() {
                    busquedaAvanzadaProductosV02(1);
                }, 300);
                return true;
            }, false, ["Aceptar"], "info!", null);
    }
}

// ✅ NUEVA: Extraer ProductoListaDto desde fila HTML del grid
function extraerProductoListaDtoDeFilaHTML($fila) {
    try {
        var $checkbox = $fila.find('.check-producto-busqueda');
        var productoData = $checkbox.data('producto');
        
        if (productoData) {
            return productoData;
        }
        
        // Fallback: extraer desde celdas usando solo notación p_
        var celdas = $fila.find('td');
        if (celdas.length >= 7) {
            return {
                p_id: $(celdas[1]).text().trim(),
                p_desc: $(celdas[2]).text().trim(),
                p_id_barrado: $(celdas[3]).text().trim(),
                p_pcosto: parsearNumeroConCultura($(celdas[4]).text()),
                p_pvta_001: parsearNumeroConCultura($(celdas[5]).text()),
                p_pvta_002: parsearNumeroConCultura($(celdas[6]).text()),
                p_activo: $(celdas[8]).find('.badge').hasClass('bg-success') ? "S" : "N"
            };
        }
        
        return null;
    } catch (error) {
        console.error("Error al extraer ProductoListaDto desde fila HTML:", error);
        return null;
    }
}

// ✅ MANTENER: Solo las funciones de formateo que se usan en generarGridDesdeProductoListaDto
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

// ✅ MANTENER: Para extraer datos desde HTML cuando sea necesario
function parsearNumeroConCultura(valorTexto) {
    if (!valorTexto || valorTexto.trim() === '') return 0;
    
    let valor = valorTexto.toString().replace(/[$\s]/g, '');
    valor = valor.replace(/\./g, '').replace(/,/g, '.');
    
    const numero = parseFloat(valor);
    return isNaN(numero) ? 0 : numero;
}

// ✅ ACTUALIZADA: Función para agregar productos con ProductoListaDto
function agregarProductosSeleccionadosAOfertas() {
    if (productosSeleccionadosBusqueda.length === 0) {
        ControlaMensajeWarning("Debe seleccionar al menos un producto");
        return;
    }

    var mensaje = `¿Desea agregar ${productosSeleccionadosBusqueda.length} productos a las ofertas?`;
    if (productosSeleccionadosBusqueda.length === 1) {
        // ✅ SOLO p_desc
        var descripcion = productosSeleccionadosBusqueda[0].p_desc;
        mensaje = `¿Desea agregar el producto "${descripcion}" a las ofertas?`;
    }

    AbrirMensaje(
        "CONFIRMAR AGREGADO",
        mensaje,
        function (resp) {
            if (resp === "SI") {
                procesarAgregarProductosMultiples();
            }
            $("#msjModal").modal("hide");
            return true;
        },
        true,
        ["Agregar", "Cancelar"],
        "info!",
        null
    );
}

// ✅ ACTUALIZADA: Procesar agregado usando ProductoListaDto
function procesarAgregarProductosMultiples() {
    AbrirWaiting("Agregando productos a ofertas...");
    
    // ✅ ENVÍO DIRECTO: Lista de ProductoListaDto sin conversión
    PostGen2(
        productosSeleccionadosBusqueda, 
        presentarProductosOfertaMultipleUrl, 
        function (response) {
            CerrarWaiting();
            
            // ✅ ACTUALIZAR: Grid desde respuesta del controlador
            $("#gridProductoOferta").html(response);
            
            if (typeof configurarEventosGridOferta === 'function') {
                configurarEventosGridOferta();
            }
            
            // Cerrar modal de búsqueda
            $("#busquedaModal").modal("hide");
            
            // Guardar cantidad antes de limpiar
            var cantidadAgregada = productosSeleccionadosBusqueda.length;
            limpiarSeleccionBusqueda();
            
            // Mensaje de éxito
            ControlaMensajeSuccess(`${cantidadAgregada} producto${cantidadAgregada > 1 ? 's' : ''} agregado${cantidadAgregada > 1 ? 's' : ''} a ofertas correctamente`);
        }, 
        function (error) {
            CerrarWaiting();
            ControlaMensajeError("Error al agregar productos: " + (error.message || "Error desconocido"));
        }
    );
}

// ✅ NUEVA: Procesar respuesta JSON de búsqueda individual
function procesarRespuestaBusquedaJSON(response, valorBuscado) {
    if (response.error) {
        ControlaMensajeError(response.msg || "Error en la búsqueda");
        return;
    }
    
    var productos = response.productos || [];
    
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
        // ✅ ENVÍO DIRECTO: Sin conversión, directo al controlador
        var producto = productos[0];
        agregarProductoIndividualAOfertas(producto);
    } else {
        // Múltiples productos - mostrar búsqueda avanzada
        AbrirMensaje("ATENCIÓN", 
            `Se encontraron ${productos.length} productos. Se abrirá la búsqueda avanzada para seleccionar.`, 
            function () {
                $("#msjModal").modal("hide");
                $("#Search").val(valorBuscado);
                inicializaBusquedaAvanzadaV02();
                $("#busquedaModal").modal("show");
                setTimeout(function() {
                    busquedaAvanzadaProductosV02(1);
                }, 300);
                return true;
            }, false, ["Aceptar"], "info!", null);
    }
}

// ✅ OPTIMIZADA: Envío directo a PresentarProductoOferta del controlador
function agregarProductoIndividualAOfertas(productoListaDto) {
    AbrirWaiting("Agregando producto a ofertas...");
    
    // ✅ VALIDACIÓN: Verificar datos mínimos
    if (!productoListaDto.p_id) {
        CerrarWaiting();
        ControlaMensajeError("Producto sin ID válido");
        return;
    }
    
    // ✅ ENVÍO DIRECTO: Al controlador sin conversión previa
    PostGenHtml(productoListaDto, presentarProductoOfertaUrl, function (htmlResponse) {
        CerrarWaiting();
        
        // ✅ ACTUALIZAR: Grid desde respuesta del controlador
        $("#gridProductoOferta").html(htmlResponse);
        
        // Configurar eventos del grid si existe la función
        if (typeof configurarEventosGridOferta === 'function') {
            configurarEventosGridOferta();
        }
        
        // ✅ LIMPIAR: Campo de búsqueda
        $("#Busqueda").val("");
        $("#estadoFuncion").val(false);
        
        // Mensaje de éxito
        var descripcion = productoListaDto.p_desc || "Producto";
        ControlaMensajeSuccess(`Producto "${descripcion}" agregado a ofertas correctamente`);
        
    }, function (error) {
        CerrarWaiting();
        ControlaMensajeError("Error al agregar producto a ofertas: " + (error.message || "Error desconocido"));
    });
}

// Variables y funciones que ya no son necesarias

// ELIMINAR: Variable productoBase (no se usa más)
// var productoBase; // ELIMINADA

// ELIMINAR: Función formatearParaCompatibilidad (incluida en convertirProductoListaABusqueda eliminada)
// function formatearParaCompatibilidad() { ... } // ELIMINADA

// CONSERVAR: Solo funciones esenciales
function obtenerProductosSeleccionados() {
    return {
        productos: productosSeleccionadosBusqueda,
        cantidad: productosSeleccionadosBusqueda.length,
        haySeleccion: productosSeleccionadosBusqueda.length > 0
    };
}

// ✅ NUEVA: Función faltante para generar grid desde ProductoListaDto
function generarGridDesdeProductoListaDto(productos, metadata) {
    if (!productos || productos.length === 0) {
        return `
            <div class="text-center text-muted py-4">
                <i class="bx bx-info-circle me-2"></i>
                No se encontraron productos con los criterios especificados
            </div>
        `;
    }

    var html = `
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
                <tbody>
    `;

    productos.forEach(function(item, index) {
        var claseAlternada = index % 2 === 0 ? "table-row-alt" : "";
        var estadoBadge = item.p_activo === "S" ? "bg-success" : "bg-warning";
        var estadoTexto = item.p_activo === "S" ? "Activo" : "Inactivo";
        
        var pcostoFormateado = formatearNumeroConCultura(item.p_pcosto || 0, 3);
        var pmayoristaFormateado = formatearNumeroConCultura(item.p_pvta_001 || 0, 2);
        var pminoristaFormateado = formatearNumeroConCultura(item.p_pvta_002 || 0, 2);
        
        html += `
            <tr class="${claseAlternada} fila-producto" data-producto-id="${item.p_id}">
                <td class="text-center">
                    <input type="checkbox" 
                           class="form-check-input check-producto-busqueda"
                           data-p-id="${item.p_id}"
                           data-producto='${JSON.stringify(item).replace(/'/g, "&apos;")}'
                           checked
                           title="Producto seleccionado automáticamente">
                </td>
                <td class="text-center">${item.p_id}</td>
                <td class="text-left" title="${item.p_desc}">${item.p_desc}</td>
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
    });

    html += `
                </tbody>
            </table>
        </div>
    `;

    // ✅ AGREGAR: Sección de metadata y controles de selección
    html += generarSeccionMetadataYControles(productos.length, metadata);

    return html;
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