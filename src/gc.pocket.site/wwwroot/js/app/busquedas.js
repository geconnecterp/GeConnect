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
            $("#proveedorId").val(ui.item.id);
            return true;
        }
    });


    $("#RubroNombre").autocomplete({
        source: function (request, response) {
            data = { prefix: request.term }
            $.ajax({
                url: buscarProveedorUrl,
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
            $("#rubroId").val(ui.item.id);
            return true;
        }
    });
});