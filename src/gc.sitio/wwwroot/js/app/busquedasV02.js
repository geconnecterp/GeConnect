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

function busquedaAvanzadaProductosV02(pag) {
    var ri01 = $("#Rel01Item").val();
    var ri02 = $("#Rel02Item").val();
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
    var data1 = { ri01, ri02, act, dis, ina, cstk, sstk, buscar };

    // Verificar si es nueva búsqueda
    var buscaNew = JSON.stringify(dataBakV02) != JSON.stringify(data1);
    if (buscaNew === false) {
        pagina = pag;
    } else {
        dataBakV02 = data1;
        pagina = 1;
        pag = 1;
        // Limpiar selección en nueva búsqueda
        limpiarSeleccionBusqueda();
    }

    var sort = null;
    var sortDir = null;
    var data2 = { sort, sortDir, pag, buscaNew };
    var data = $.extend({}, data1, data2);

    PostGenHtml(data, busquedaAvanzadaUrl, function (obj) {
        $("#divBusquedaAvanzada").html(obj);
        configurarEventosGridBusquedaV02();

        PostGen({}, buscarMetadataURL, function (metaObj) {
            if (metaObj.error === true) {
                ControlaMensajeError(metaObj.msg);
            } else {
                totalRegs = metaObj.metadata.totalCount;
                pags = metaObj.metadata.totalPages;
                pagRegs = metaObj.metadata.pageSize;
                $("#pagEstado").val(true).trigger("change");
            }
        });
    });

    return true;
}

function buscarProducto() {
    AbrirWaiting();
    var _post = busquedaProdBaseUrl;
    var valor = $("#Busqueda").val();
    var validarEstado = true;

    var datos = {};
    if (typeof validarEstado !== 'undefined') {
        datos = { busqueda: valor, validarEstado };
    }
    else {
        datos = { busqueda: valor };
    }

    PostGen(datos, _post, function (obj) {
        if (obj.error === true) {
            CerrarWaiting();
            AbrirMensaje("ATENCIÓN", obj.msg, function () {
                productoBase = null;
                $("#estadoFuncion").val(false);
                $("#btnBusquedaBase").prop("disabled", false);
                $("#msjModal").modal("hide");
                $("#Busqueda").focus();
                return true;
            }, false, ["Aceptar"], "error!", null);
        }
        else if (obj.warn === true) {
            CerrarWaiting();
            if (obj.producto.p_id === "0000-0000") {
                AbrirMensaje("ATENCIÓN", obj.msg, function () {
                    productoBase = null;
                    $("#estadoFuncion").val(false);
                    $("#btnBusquedaBase").prop("disabled", false);
                    $("#msjModal").modal("hide");
                    $("#Busqueda").focus();
                    return true;
                }, false, ["Aceptar"], "error!", null);
            }
            else if (obj.producto.p_id === "NO") {
                if (funcionBusquedaAvanzada === true) {
                    AbrirMensaje("ATENCIÓN", "NO SE ENCONTRO EL PRODUCTO QUE INTENTO BUSCAR. SE ABRIRÁ LA BUSQUEDA AVANZADA.", function () {
                        $("#msjModal").modal("hide");
                        productoBase = null;
                        $("#estadoFuncion").val(false);
                        inicializaBusquedaAvanzadaV02();
                        $("#busquedaModal").modal("toggle");
                        return true;
                    }, false, ["Aceptar"], "error!", null);

                    return true;
                }
                else {
                    AbrirMensaje("ATENCIÓN", "NO SE ENCONTRO EL PRODUCTO QUE INTENTO BUSCAR.", function () {
                        $("#msjModal").modal("hide");
                        $("#Busqueda").focus();
                        return true;
                    }, false, ["Aceptar"], "error!", null);

                }
            } else {
                //encontro producto pero hay warning
                AbrirMensaje("ATENCIÓN!", obj.msg, function (resp) {
                    if (resp === "SI") {
                        productoBase = obj.producto;
                        $("#estadoFuncion").val(true);
                        $("#estadoFuncion").trigger("change");
                        $("#msjModal").modal("hide");
                        // ✅ Verificación antes de usar focus
                        var up = $("#txtUPEnComprobanteRP");
                        if (up.length > 0) {
                            up.trigger("focus");
                        }
                        return true;
                    }
                    else {
                        //se deniega
                        productoBase = null;
                        $("#estadoFuncion").val(false);
                        $("#btnBusquedaBase").prop("disabled", false);
                        $("#msjModal").modal("hide");
                        $("#Busqueda").focus();
                        return true;
                    }
                },
                    true, ["Aceptar", "Denegar"], "Warning!", null);
            }
        }
        else {
            //encontro y se presenta
            productoBase = obj.producto;
            $("#estadoFuncion").val(true);
            $("#estadoFuncion").trigger("change");
            return true;
        }
    });
    return true;
}

// Configurar eventos específicos del grid de búsqueda V02
function configurarEventosGridBusquedaV02() {
    // Checkbox "Seleccionar todos"
    $("#checkAllBusqueda").off("change").on("change", function () {
        var isChecked = $(this).is(":checked");
        $(".check-producto-busqueda").prop("checked", isChecked);

        if (isChecked) {
            // Agregar todos los productos visibles
            $(".check-producto-busqueda").each(function () {
                var productoData = $(this).data("producto");
                if (productoData) {
                    agregarProductoASeleccion(productoData);
                }
            });
        } else {
            // Remover todos los productos visibles
            $(".check-producto-busqueda").each(function () {
                var productoData = $(this).data("producto");
                if (productoData) {
                    removerProductoDeSeleccion(productoData.P_id);
                }
            });
        }

        actualizarContadorSeleccion();
    });

    // Checkboxes individuales
    $(".check-producto-busqueda").off("change").on("change", function () {
        var productoData = $(this).data("producto");

        if (!productoData) return;

        if ($(this).is(":checked")) {
            agregarProductoASeleccion(productoData);
        } else {
            removerProductoDeSeleccion(productoData.P_id);
        }

        // Actualizar estado del checkbox "todos"
        var totalVisible = $(".check-producto-busqueda").length;
        var checkedVisible = $(".check-producto-busqueda:checked").length;
        $("#checkAllBusqueda").prop("checked", totalVisible === checkedVisible);

        actualizarContadorSeleccion();
    });

    // Botón agregar productos seleccionados
    $("#btnAgregarSeleccionados").off("click").on("click", function () {
        agregarProductosSeleccionadosAOfertas();
    });

    // Botón limpiar selección
    $("#btnLimpiarSeleccionBusqueda").off("click").on("click", function () {
        confirmarLimpiezaSeleccion();
    });
}

// Gestión de productos seleccionados
function agregarProductoASeleccion(producto) {
    if (!productosSeleccionadosBusqueda.some(p => p.P_id === producto.P_id)) {
        productosSeleccionadosBusqueda.push(producto);
    }
}

function removerProductoDeSeleccion(productoId) {
    productosSeleccionadosBusqueda = productosSeleccionadosBusqueda.filter(p => p.P_id !== productoId);
}

// ✅ OPTIMIZADA: Contador que refleja selección automática
function actualizarContadorSeleccion() {
    var cantidad = productosSeleccionadosBusqueda.length;
    $("#contadorSeleccionados").text(cantidad);
    $("#badgeSeleccionados").text(cantidad + " seleccionados");

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
            var estaSeleccionado = productosSeleccionadosBusqueda.some(p => p.P_id === productoData.P_id);
            $(this).prop("checked", estaSeleccionado);
        }
    });

    // Actualizar checkbox "todos"
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

// Función para agregar producto programáticamente
function agregarProductoProgramatico(producto) {
    if (producto && producto.P_id) {
        agregarProductoASeleccion(producto);
        actualizarContadorSeleccion();
        return true;
    }
    return false;
}

//✅ Helper para focus con manejo de errores
function enfocarElemento(selector) {
    try {
        var elemento = $(selector);
        if (elemento.length > 0) {
            elemento.trigger("focus");
        }
    } catch (error) {
        console.warn("Error al enfocar elemento:", selector, error);
    }
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
    // Obtener ID del producto (segunda celda, después del checkbox)
    var id = x.cells[1].innerText.trim();
    
    // Confirmar uso individual del producto
    var descripcion = x.cells[2].innerText.trim();
    
    AbrirMensaje(
        "USAR PRODUCTO INDIVIDUAL",
        `¿Desea usar únicamente el producto "${descripcion}"?<br><small>Se limpiará la selección múltiple actual.</small>`,
        function (resp) {
            if (resp === "SI") {
                // Cerrar modal de búsqueda
                $("#busquedaModal").modal("toggle");
                
                // Usar búsqueda simple
                $("input#Busqueda").val(id);
                $("#btnBusquedaBase").trigger("click");
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