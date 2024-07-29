$(document).ready(function () {

    //declaramos el input de proveedor como autocomplete
    $("#ProveedorNombre").autocomplete({
        source: function (request, response) {
            data = { prefix: request.term }
            $.ajax({
                url: buscarProveedorUrl,
                type: "GET",
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
            $("#proveedorId").val(ui.item.cta_Id);
        }
    });

});