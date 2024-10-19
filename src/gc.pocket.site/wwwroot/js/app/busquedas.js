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
    var mod = "RPR";
    var valEst = false;

    if (typeof modulo !== 'undefined') {
        mod = modulo;
    }
   
    if (typeof validarEstado !== 'undefined') {
        valEst = validarEstado;
    }
    
    var datos = { busqueda: valor, validarEstado:valEst,modulo:mod };

    PostGen(datos, _post, function (obj) {
        CerrarWaiting();

        if (obj.error === true) {
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
            if (obj.producto.p_Id === "0000-0000") {

                AbrirMensaje("ATENCIÓN", obj.msg, function () {
                    productoBase = null;
                    $("#estadoFuncion").val(false);
                    $("#btnBusquedaBase").prop("disabled", false);
                    $("#msjModal").modal("hide");
                    $("#Busqueda").focus();
                    return true;
                }, false, ["Aceptar"], "error!", null);                   
            }
            else if (obj.producto.p_Id === "NO") {
                //se busco un codigo pero no se encontró
                if (typeof funcionBusquedaAvanzada !== 'undefined' || funcionBusquedaAvanzada === false) {
                    //si no esta la variable funcionBusquedaAvanzada o la misma es false, no se realiza la busqueda avanzada
                    productoBase = null;
                    $("#estadoFuncion").val(false);
                    $("#btnBusquedaBase").prop("disabled", false);
                    $("#msjModal").modal("hide");
                    $("#Busqueda").focus();
                    return true;
                }
                else {
                    ///se abre el modal de la busqueda avanzada
                    $("#busquedaModal").modal("toggle");
                }
            } else {
                //encontro producto pero hay warning
                CerrarWaiting();
                AbrirMensaje("ATENCIÓN!", obj.msg, function (resp) {
                    if (resp === "SI") {
                        productoBase = obj.producto;
                        $("#estadoFuncion").val(true);
                        $("#estadoFuncion").trigger("change");
                        $("#msjModal").modal("hide");
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