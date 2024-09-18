$(function () {
    //cargo el js para llamar desde esta funcion a la función de busqueda
    //verifico si se hace click en el boton buscar
    $("#btnBusquedaBase").on("click", function () {
        buscarProducto();
        return true;
    });

    $("#estadoFuncion").on("change", verificaEstado); //este control debe ser insertado el mismo o similar para cada modulo.

    $("#btnValBox").on("click", validaBox);

    $("#btnBusquedaBase").prop("disabled", false);

});

//esta funcion verifica si el box que ingresa corresponde al box del producto
function validaBox() {
    var dato = { boxId: $("#txtBox").val() }
    PostGen(dato, validarBoxIngresadoUrl, function (obj) {
        if (obj.error === true) {            
            AbrirMensaje("Importante", obj.msg, function () {
                $("#msjModal").modal("hide");
                return true;
            }, false, ["Aceptar"], "error!", null);
        }
        else if (obj.warn === true) {           
            AbrirMensaje("Importante", obj.msg, function () {
                $("#msjModal").modal("hide");
                return true;
            }, false, ["Aceptar"], "warn!", null);
        }
        else {
            ControlaMensajeSuccess(obj.msg);
            //solo pasa al otro campo.           
            $("#Busqueda").focus();
            return true;
        }

    });
}

function verificaEstado() {
    CerrarWaiting();
    var res = $("#estadoFuncion").val();
    if (res === "true") {
        
        //antes de mostrar los datos debo verificar si el producto es el que deseo presentar.
        var dato = { pId: productoBase.p_id }
        PostGen(dato, validarProductoIngresadoUrl, function (obj) {
            if (obj.error === true) {
                AbrirMensaje("Importante", obj.msg, function () {
                    $("#msjModal").modal("hide");
                    return true;
                }, false, ["Aceptar"], "error!", null);
            }
            else if (obj.warn === true) {
                AbrirMensaje("Importante", obj.msg, function () {
                    $("#msjModal").modal("hide");
                    return true;
                }, false, ["Aceptar"], "warn!", null);
            }
            else {
                ControlaMensajeSuccess(obj.msg);
                //traigo la variable productoBase e hidrato componentes
                var prod = productoBase;
                $("#P_id").val(prod.p_id);
                $("#Marca").val(prod.p_m_marca);
                $("#Descipcion").val(prod.p_desc);               
                $("#Rubro").val(prod.rub_desc);
                //$("#fvto").val(prod.)

                return true;
            }
        });



        
        $("#estadoFuncion").val(false);

        //PresentarStkD(prod.p_Id);
    
        $("#btnBusquedaBase").prop("disabled", false);

    }
    return true;
}