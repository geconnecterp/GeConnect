$(document).ready(function () {

    $("input#Busqueda").keypress(verificaTeclaDeBusqueda);

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

});

function buscarProducto() {
    AbrirWaiting();
    var _post = busquedaProdBase;
    var valor = $("#Busqueda").val();
    PostGen({ busqueda: valor }, _post, function (obj) {
        if (obj.error === true) {
            ControlaMensajeError(obj.msg);
            productoBase = null;
            $("#estadoFuncion").val(false);
            $("#btnBusquedaBase").prop("disabled", false);
            CerrarWaiting();
            return true;
        }
        else {
            if (obj.producto.p_Id !== "0000-0000") {
                //debo verificar si el valor del warn es true.
                //esto permite peresentar un mensaje
                if (obj.warn === true) {
                    AbrirMensaje("ATENCIÓN!", obj.msg, function (resp) {
                        if (resp = "SI") {
                            productoBase = obj.producto;
                            $("#estadoFuncion").val(true);
                            $("#estadoFuncion").trigger("change");
                            return true;
                        }
                        else {
                            //se deniega
                            productoBase = null;
                            $("#estadoFuncion").val(false);
                            $("#btnBusquedaBase").prop("disabled", false);
                            CerrarWaiting();
                            return true;
                        }
                    },
                        true, ["Aceptar", "Denegar"], "Warning!", null);
                }
            }
            else {
                productoBase = null;
                $("#estadoFuncion").val(false);
                ControlaMensajeWarning(obj.msg);
                $("#btnBusquedaBase").prop("disabled", false);
                CerrarWaiting();
                return true;
            }
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