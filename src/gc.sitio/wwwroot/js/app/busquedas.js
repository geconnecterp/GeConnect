$(function () {
    
    $("input#Busqueda").keypress(verificaTeclaDeBusqueda);
    $("input").on("focus", function () { $(this).select(); })

    $("#btnBuscarProd").on("click",busquedaAvanzadaProductos)

    //declaramos el input de proveedor como autocomplete
    $("#ProveedorNombre").autocomplete({
        source: function (request, response) {
            data = { prefix: request.term }
            $.ajax({
                url: buscarProveedorUrl,
                type: "POST",
                dataType: "json",
                data: data,
                success: function (obj) {
                    response($.map(obj, function (item) {
                        var texto = item.cta_Id + "-" + item.cta_Denominacion;
                        return { label: texto, value: item.cta_Denominacion, id: item.cta_Id };
                    }));
                }
            })
        },
        minLength: 3,
        select: function (event, ui) {
            $("#CtaProveedorId").val(ui.item.id);
            return true;
        }
    });

    $("#RubroNombre").autocomplete({
        source: function (request, response) {
            data = { prefix: request.term }
            $.ajax({
                url: buscarRubroUrl,
                type: "POST",
                dataType: "json",
                data: data,
                success: function (obj) {
                    response($.map(obj, function (item) {
                        var texto = item.rub_Desc;
                        return { label: texto, value: item.rub_Desc, id: item.rub_Id };
                    }));
                }
            })
        },
        minLength: 3,
        select: function (event, ui) {
            $("#RubroId").val(ui.item.id);
            return true;
        }
    });

    return true;
});

function busquedaAvanzadaProductos() {
    var pr = $("#CtaProveedorId").val();
    var rb = $("#RubroId").val();
    //activos
    var act = $("#chkActivos").val();
    //discontinuos
    var dis = $("#chkDisc").val();
    //inactivos
    var ina = $("#chkInact").val();

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
            ControlaMensajeError(obj.msg);
            productoBase = null;
            $("#estadoFuncion").val(false);
            $("#btnBusquedaBase").prop("disabled", false);
            CerrarWaiting();
            $("#Busqueda").focus();
            return true;
        }
        else if (obj.warn === true) {
            if (obj.producto.p_id === "0000-0000") {
                //un enter sin ningun codigo
                productoBase = null;
                $("#estadoFuncion").val(false);
                ControlaMensajeWarning(obj.msg);
                $("#btnBusquedaBase").prop("disabled", false);
                CerrarWaiting();
                return true;
            }
            else if (obj.producto.p_id === "NO") {
                //se busco un codigo pero no se encontró
                if (typeof funcionBusquedaAvanzada !== 'undefined') {
                    if (funcionBusquedaAvanzada) {
                        ///se abre el modal de la busqueda avanzada
                        $("#busquedaModal").modal("toggle");
                    }
                    else {
                        //si no esta la variable funcionBusquedaAvanzada o la misma es false, no se realiza la busqueda avanzada
                        productoBase = null;
                        $("#estadoFuncion").val(false);
                        $("#btnBusquedaBase").prop("disabled", false);
                        $("#msjModal").modal("hide");
                        $("#Busqueda").focus();
                        return true;
                    }
                }
                else {
                    productoBase = null;
                    $("#estadoFuncion").val(false);
                    $("#btnBusquedaBase").prop("disabled", false);
                    $("#msjModal").modal("hide");
                    $("#Busqueda").focus();
                    return true;
                }
                CerrarWaiting();
            } else {
                //encontro producto pero hay warning
                CerrarWaiting();
                AbrirMensaje("ATENCIÓN!", obj.msg, function (resp) {
                    if (resp === "SI") {
                        productoBase = obj.producto;
                        $("#estadoFuncion").val(true);
                        $("#estadoFuncion").trigger("change");
                        $("#msjModal").modal("hide");
                        var up = $("#txtUPEnComprobanteRP");
                        if (up) {
                            up.focus();
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

function verificaTeclaDeBusqueda(e) {
    if (e.which == "13") {

        $("#btnBusquedaBase").trigger("click");
        $("#btnBusquedaBase").prop("disabled", true);
        return true;

    }
}