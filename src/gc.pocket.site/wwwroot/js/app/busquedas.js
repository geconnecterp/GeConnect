$(document).ready(function () {

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
                        var texto = item.cta_Denominacion;
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
            CerrarWaiting();
        }
        else {
            if (obj.producto.p_Id !== "0000-0000") {
                productoBase = obj.producto;
                $("#estadoFuncion").val(true);
                $("#estadoFuncion").trigger("change");
            }
            else {
                productoBase = null;
                $("#estadoFuncion").val(false);
            CerrarWaiting();
            }
        }
    });
}

