$(function () {
    //cargo el js para llamar desde esta funcion a la función de busqueda
    //verifico si se hace click en el boton buscar
    $("#btnBusquedaBase").on("click", function () {
        buscarProducto();
        return true;
    });

    $("#estadoFuncion").on("change", verificaEstado); //este control debe ser insertado el mismo o similar para cada modulo.

    //$("#btnCleanProd").on("click", limpiarProductoCarrito);

    //este boton valida el BOX
    $("#btnValBox").on("click", validaBox);

    //ESTE BOTON CARGARÍA LOS DATOS AL CARRITO
    $("#btnCargarProd").on("click", cargarCarrito);
    //chequea los enter que se dan sobre los controles editables
    $(".inputEditable").on("keypress", analizaEnterInput);

    $("#btnBusquedaBase").prop("disabled", false);
    CargarAutoActual();
});



function cargarCarrito() {
    //aca se validará previamente si la cantidad ingresada corresponde a lo solicitado
        AbrirWaiting()
    var cantSolic = autorizacionActual.pPedido;
    var upId = productoBase.up_id;
    var cantidad = 0;
    var up = parseInt($("#up").val());
    var bulto = parseInt($("#box").val());
    var unid = parseFloat($("#unid").val())
    var fv = null;
    if (upId === "07") {
        cantidad = (up * bulto) + unid;
    } else {
        cantidad = bulto;
    }

    if (cantidad !== cantSolic) {
        CerrarWaiting();

        AbrirMensaje("Atención", "La cantidad ingresada no corresponde a la cantidad solicitada. Verifique.", function () {
            $("#msjModal").modal("hide");
            $("#up").focus();
            return true;
        }, false, ["Aceptar"], "warn!", null);
    }
    else {
        ControlaMensajeSuccess("Cantidad correcta");
        //se procede a enviar el producto a cargar
        var dato = { p_id: autorizacionActual.pId, up, bulto, unid, cantidad, fv }
        PostGen(dato, ResguardarProductoCarritoUrl, function (obj) {
            if (obj.error === true) {
                CerrarWaiting();

                AbrirMensaje("Importante", obj.msg, function () {
                    $("#msjModal").modal("hide");
                    return true;
                }, false, ["Aceptar"], "error!", null);
            }
            else {
                CerrarWaiting();

                ControlaMensajeSuccess(obj.msg);
                window.location.href = proximoProductoUrl + "?esrubro=false&esbox=false";
            }
        });

    }
}


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
                    $("#Busqueda").val("");
                    $("#Busqueda").focus();

                    return true;
                }, false, ["Aceptar"], "error!", null);
            }
            else if (obj.warn === true) {
                AbrirMensaje("Importante", obj.msg, function () {
                    $("#msjModal").modal("hide");
                    $("#Busqueda").val("");
                    $("#Busqueda").focus();

                    return true;
                }, false, ["Aceptar"], "warn!", null);
            }
            else {
                ControlaMensajeSuccess(obj.msg);
                //traigo la variable productoBase e hidrato componentes
                var prod = productoBase;
                var autoAct = autorizacionActual;

                $("#P_id").val(prod.p_id);
                $("#Marca").val(prod.p_m_marca);
                $("#Descipcion").val(prod.p_desc);
                $("#Rubro").val(prod.rub_desc);
                $("#up").mask("000.000.000.000", { reverse: true });
                $("#up").val(autoAct.pUnidPres).prop("disabled", false);
                $("#unid").mask("000.000.000.000", { reverse: true });
                //$("#fvto").val(prod.)

                if (prod.up_id === "07") {  //unidades enteras
                    $("#box").mask("000.000.000.000", { reverse: true });
                    $("#unid").val(0).prop("disabled", false);
                }
                else { //unidades decimales
                    $("#box").mask("000.000.000.000,00", { reverse: true });
                    $("#unid").val(0).prop("disabled", true);
                }

                $("#box").val(0).prop("disabled", false);

                //activamos el boton
                $("#btnCargarProd").prop("disabled", false);

                //inicializamos el campo de busqueda
                $("#Busqueda").val("");

                $("#up").focus();

                return true;
            }
        });




        $("#estadoFuncion").val(false);

        //PresentarStkD(prod.p_Id);

        $("#btnBusquedaBase").prop("disabled", false);

    }
    return true;
}