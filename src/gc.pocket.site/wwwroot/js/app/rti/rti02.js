﻿$(function () {
    //cargo el js para llamar desde esta funcion a la función de busqueda
    //verifico si se hace click en el boton buscar
    $("#btnBusquedaBase").on("click", function () {
        InicializaPantallaRTI02();
        buscarProducto();
        return true;
    });

    $("#btnConfirmarUL").click(confirmarRTI);

    $("#ul_Id").on("keypress", function (e) {
        if (e.which == "13") {
            var valor = $("#ul_Id").val();
            var res = parseInt(valor);
            if (isNaN(res)) {
                AbrirMensaje("ATENCIÓN", "El valor ingresado no es numérico. Verifique.", function () {
                    $("#msjModal").modal("hide");
                    return true;
                },
                    false, ["Aceptar"], "warn!", null);
            }
            switch (valor.length) {
                case 0:
                    AbrirMensaje("ATENCIÓN", "Debe ingresar el Nro de palet.", function () {
                        $("#msjModal").modal("hide");
                        return true;
                    },
                        false, ["Aceptar"], "warn!", null);
                    break;
                case 1:
                case 2:
                    valor = ('0' + valor).slice(-2); // Asegura que el numero siempre tenga dos dígitos
                    $("#ul_Id").val("RTR" + NroAuto +'-'+ valor);
                    break;
                default:

                    break;
            }
        }
    })

    $("#btnConfirmarUL").on("click", function () {


    });

    //input del control. Sirve para permitir inicializar pantalla.
    $("input#Busqueda").on("focus", function () {
        InicializaPantallaRTI02();
    });

    //este control debe ser insertado el mismo o similar para cada modulo.
    $("#estadoFuncion").on("change", verificaEstadoRTI02); 

    $("#btnVolverGen").on("click", function () {
        var tb = $("#divRtiGrid #tbProdRTI tbody td");
        if (tb.length > 0) {
            AbrirMensaje("¡¡Atención!!", "Esta volviendo al inicio de esta aplicación. Tenga en cuenta que si cambia de Remito, perderá toda la información cargada.",
                function (resp) {
                    if (resp === "SI") { window.location.href = homeRPRUrl; }
                    else {
                        $("#msjModal").modal("hide");
                        return true;
                    }
                }, true, ["Volver", "Quedarse"], "warn!", null);
        } else {
            window.location.href = VolverAnteriorUrl;
        }
    });

    $(".inputEditable").on("keypress", analizaEnterInput);
    $("#btnCargarProd").on("click", cargarProductosRTI02);

    InicializaPantallaRTI02();
    return true
})

//function analizaEnterInputRTI02(e) {
//    if (e.which == "13") {
//        tope = 99999;
//        index = -1;
//        //obtengo los inputs dentro del div
//        var inputss = $("main :input:not(:disabled)");
//        tope = inputss.length;
//        //le el id del input en el que he dado enter
//        var cual = $(this).prop("id");
//        inputss.each(function (i, item) {
//            if ($(item).prop("id") === cual) {
//                index = i;
//                return false;
//            }
//        });
//        if (index > -1 && tope > index + 1) {
//            inputss[index + 1].focus();
//        }    
//    }
//    return true;
//}

function InicializaPantallaRTI02() {
    productosGridRTI02();
    $("#P_id").val("");
    $("#Descipcion").val("");
    $("#Rubro").val("");
    $("#up").val(0).prop("disabled", true);
    $("#up").prop("readonly", false);
    $("#up").removeClass("backReadOnly");
    $("#fvto").val("").prop("disabled", true);

    $("#box").val(0).prop("disabled", true);
    $("#unid").val(0).prop("disabled", true);
    $("#btnCargaProd").prop("disabled", true);
    $("#divRprGrid").empty();


    return true;
}

function productosGridRTI02() {
    var data = {};
    PostGenHtml(data, PresentarProductosSeleccionadosUrl, function (obj) {
        $("#divRtiGrid").html(obj);
        var tb = $("#divRtiGrid #tbProdRTI tbody td");
        if (tb.length <= 0) {
            $("#btnContinuarRTI").hide("fast");
        } else {
            $("#btnContinuarRTI").show("fast");
        }

        //if (typeof ocultarTrash !== 'undefined') {
        //    if (ocultarTrash === true) {
        //        //ocultamos la 8° columna
        //        $(".ocultar").toggle();
        //        $("#divRprGrid #tbProdRPR tbody td:nth-child(8)").toggle();
        //    }
        //}

        return true;
    }, function (obj) {
        ControlaMensajeError(obj.message);
        return true;
    });
}


function verificaEstadoRTI02(e) {
    var res = $("#estadoFuncion").val();
    CerrarWaiting();
    if (res === "true") {
        var prod = productoBase;

        $("#P_id").val(prod.p_id);
        $("#Descipcion").val(prod.p_desc);
        $("#Rubro").val(prod.rub_desc);
        $("#estadoFuncion").val(false);
        $("#up").mask("000,000,000,000", { reverse: true });
        $("#up").val(prod.p_unidad_pres).prop("disabled", false);
        $("#box").mask("000,000,000,000", { reverse: true });

        if (prod.up_id === "07") {  //unidades enteras
            $("#unid").mask("000,000,000,000", { reverse: true });
            $("#box").val(0).prop("disabled", false);
        }
        else { //unidades decimales
            $("#up").prop("readonly", true);
            $("#up").val(1);
            $("#up").addClass("backReadOnly");

            $("#unid").mask("000,000,000,000.000", { reverse: true });
        }
        $("#unid").val(0).prop("disabled", false);

        //activamos el boton
        $("#btnCargarProd").prop("disabled", false);

        //inicializamos el campo de busqueda
        $("#Busqueda").val("");

        ////verificamos que el producto tenga vencimiento
        //if (prod.p_con_vto !== "N") {
        //    //var f = new Date();
        //    //var month = ('0' + (f.getMonth() + 1)).slice(-2); // Asegura que el mes siempre tenga dos dígitos
        //    //var day = ('0' + f.getDate()).slice(-2); // Asegura que el día siempre tenga dos dígitos
        //    //var newfecha = f.getFullYear() + '-' + month + '-' + day;
        //    $("#fvto").prop("disabled", false).val(cotaVto);
        //    //asigno callback para que se ejecute luego que cierre el waiting
        //    /* FunctionCallback = function () {*/
        //    $("#fvto").focus();
        //    //    //return true;
        //    //};
        //} else {
        //    //asigno callback para que se ejecute luego que cierre el waiting
        //    /*FunctionCallback = function () {*/
        //    $("#up").focus();
        //    //    //return true;
        //    //};
        //}
        if (prod.up_id === "07") {
            $("#up").focus();
        } else {
            $("#unid").focus();
        }
    }

    return true;
}

function cargarProductosRTI02() {
    //1 tomar datos
    //2 mandarlos al servidor
    //3 almacenarlo en sesion (lista de productos)
    //4 devolver una grilla con los productos hidratados en la misma

    var _post = reguardarProductoEnListaUrl;
    var datos = null;
    var up = $("#up").val();
    var vto = null;
    var box = $("#box").val();
    var un = $("#unid").val();
    if (productoBase.p_con_vto !== "N") {
        vto = $("#fvto").val();
        datos = { up, vto, bulto: box, unidad: un };
    }
    else {
        datos = { up, vto: " ", bulto: box, unidad: un };
    }


    AbrirWaiting();

    PostGen(datos, _post, function (obj) {
        if (obj.error === true) {
            ControlaMensajeError(obj.msg);
            CerrarWaiting();
            return true;
        }
        else if (obj.warn === true) {
            CerrarWaiting();
            AbrirMensaje("Atención", obj.msg, function (resp) {
                if (resp === "SI") {
                    //boton acumular
                    AcumularProducto();
                    productosGridRTI02();
                    $("#msjModal").modal("hide");
                    return true;
                }
                else if (resp === "SI2") {
                    //boton remplazar
                    RemplazarProducto();
                    productosGridRTI02();
                    $("#msjModal").modal("hide");

                    return true;
                }
                else {
                    productosGridRTI02();
                    $("#msjModal").modal("hide");
                    return true;
                }
            }, true, ["Acumular", "Reemplazar", "Cancelar"])

        }
        else {
            CerrarWaiting();
            ControlaMensajeSuccess("¡¡Producto cargado!!")
            productosGridRTI02();
            return true;
        }
    });
    $("input#Busqueda").focus();
    return true;
}


function RemplazarProducto() {
    AbrirWaiting("Espere... estamos procesando la solicitud...");

    PostGen({}, RemplazarProductoUrl, function (obj) {
        if (obj.error === true) {
            CerrarWaiting();
            AbrirMensaje("Remplazar Producto", obj.msg, function () {
                $('#msjModal').modal('hide');
                return true;
            }, false, ["Aceptar"], "warn!", null);
        }
        else {
            CerrarWaiting();
            AbrirMensaje("Remplazar Producto", obj.msg, function () {
                $('#msjModal').modal('hide');
                return true;
            }, false, ["Aceptar"], "succ!", null);
            productosGridRTI02();
        }
        return true;
    });
}

function EliminarProducto(id) {
    AbrirWaiting("Espere... estamos procesando la solicitud...");
    PostGen({ p_id: id }, EliminarProductoUrl, function (obj) {
        if (obj.error === true) {
            CerrarWaiting();
            AbrirMensaje("Eliminar Producto", obj.msg, function () {
                $('#msjModal').modal('hide');
                return true;

            }, false, ["Aceptar"], "warn!", null);
            InicializaPantallaRTI02();
            return true;
        }
        else {
            CerrarWaiting();
            AbrirMensaje("Eliminar Producto", obj.msg, function () {
                $('#msjModal').modal('hide');
                return true;
            }, false, ["Aceptar"], "succ!", null);
            InicializaPantallaRTI02();
            return true;
        }
    })
}

function AcumularProducto() {
    AbrirWaiting("Espere... estamos procesando la solicitud...");
    PostGen({}, AcumularProductoUrl, function (obj) {
        if (obj.error === true) {
            CerrarWaiting();
            AbrirMensaje("Acumular Producto", obj.msg, function () {
                $('#msjModal').modal('hide');
                return true;
            }, false, ["Aceptar"], "warn!", null);
        }
        else {
            CerrarWaiting();
            AbrirMensaje("Acumular Producto", obj.msg, function () {
                $('#msjModal').modal('hide');
                return true;
            }, false, ["Aceptar"], "succ!", null);
            InicializaPantallaRTI02();


        }
        return true;
    })
}

//function productosGrid() {
//    var data = {};
//    PostGenHtml(data, PresentarProductosSeleccionadosUrl, function (obj) {


//        $("#divRprGrid").html(obj);
//        var tb = $("#divRprGrid #tbProdRPR tbody td");
//        if (tb.length <= 0) {
//            $("#btnContinuarRpr").hide("fast");
//        } else {
//            $("#btnContinuarRpr").show("fast");
//        }

//        if (typeof ocultarTrash !== 'undefined') {
//            if (ocultarTrash === true) {
//                //ocultamos la 8° columna
//                $(".ocultar").toggle();
//                $("#divRprGrid #tbProdRPR tbody td:nth-child(8)").toggle();
//            }
//        }


//        return true;
//    }, function (obj) {
//        ControlaMensajeError(obj.message);
//        return true;
//    });
//}
function confirmarRTI() {
    //obtener deposito y UL
    var ul = $("#ul_Id").val();
   
    datos = { ul }
    AbrirWaiting("Espere... se estan grabando los datos...");
    PostGen(datos, ConfirmarRTIUrl, function (obj) {
        if (obj.error === true) {
            CerrarWaiting();
            AbrirMensaje("ATENCIÓN", obj.msg, function () {
                $("#msjModal").modal("hide");
                return true;
            },
                false, ["Aceptar"], "error!", null);
        }
        else if (obj.warn === true) {
            CerrarWaiting();
            AbrirMensaje("ATENCIÓN", obj.msg, function () {
                $("#msjModal").modal("hide");
                return true;
            },
                false, ["Aceptar"], "warn!", null);
        }
        else {
            CerrarWaiting();
            AbrirMensaje("ATENCIÓN", obj.msg, function () {

                $("#msjModal").modal("hide");
                window.location.href = homeUrl;
                return true;
            },
                false, ["Aceptar"], "succ!", null);
        }
    })
    return true;
}