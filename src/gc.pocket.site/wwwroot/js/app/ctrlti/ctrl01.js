﻿$(function () {
    AbrirWaiting();
    CargaInicialProductosCtrl();
    $("#btnBusquedaBase").on("click", function () {
        InicializaPantallaCtrl();
        buscarProducto();
        return true;
    });

    //input del control. Sirve para permitir inicializar pantalla.
    $("input#Busqueda").on("focus", function () {
        InicializaPantallaCtrl();
    });

    $("#estadoFuncion").on("change", verificaEstado); //este control debe ser insertado el mismo o similar para cada modulo.

    $(".inputEditable").on("keypress", analizaEnterInput);
    $("#btnCargarProd").on("click", cargarProductos);
    $("#btnContinua02").on("click", enviarProductos);

    InicializaPantallaCtrl();
});

function CargaInicialProductosCtrl() {
    var datos = {};
    PostGen(datos, cargaProdCtrlDesdeServer, function (obj) {
        if (obj.error === true) {
            ControlaMensajeError(obj.msg);
            CerrarWaiting();
            return true;
        }
        else {
            CerrarWaiting();
            return true;
        }
    });

}

function enviarProductos() {
    var data = {};
    PostGen(data, enviarProductosUrl, function (obj) {
        if (obj.error === true) {
            CerrarWaiting();
            AbrirMensaje("Atención", obj.msg, function () {
                $("#msjModal").modal("hide");
                return true;
            }, false, ["Aceptar"], "error!", null);
        }
        else {
            CerrarWaiting();
            window.location.href = listaAUPendientes;
        }

    });
}

function cargarProductos() {
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
                    productosGridCtrl();
                    $("#msjModal").modal("hide");
                    return true;
                }
                else if (resp === "SI2") {
                    //boton remplazar
                    RemplazarProducto();
                    productosGridCtrl();
                    $("#msjModal").modal("hide");

                    return true;
                }
                else {
                    productosGridCtrl();
                    $("#msjModal").modal("hide");
                    return true;
                }
            }, true, ["Acumular", "Reemplazar", "Cancelar"])

        }
        else {
            CerrarWaiting();
            ControlaMensajeSuccess("¡¡Producto cargado!!")
            productosGridCtrl();
            return true;
        }
    });
    $("input#Busqueda").focus();
    return true;
}



function productosGridCtrl() {
    var data = {};
    PostGenHtml(data, PresentarProductosSeleccionadosUrl, function (obj) {


        $("#divCtrlTIGrid").html(obj);
        var tb = $("#divCtrlTIGrid #tbProdRPR tbody td");
        if (tb.length <= 0) {
            $("#btnContinua02").hide("fast");
        } else {
            $("#btnContinua02").show("fast");
        }

        if (typeof ocultarTrash !== 'undefined') {
            if (ocultarTrash === true) {
                //ocultamos la 8° columna
                $(".ocultar").toggle();
                $("#divCtrlTIGrid #tbProdRPR tbody td:nth-child(7)").toggle();
                $("#divCtrlTIGrid #tbProdRPR tbody td:nth-child(8)").toggle();
                //$("#divCtrlTIGrid #tbProdRPR tbody td:nth-child(8)").toggle();
            }
        }


        return true;
    }, function (obj) {
        ControlaMensajeError(obj.message);
        return true;
    });
}


function verificaEstado(e) {
    FunctionCallback = null; //inicializo funcion por si tiene alguna funcionalidad asignada.
    var res = $("#estadoFuncion").val();
    CerrarWaiting();
    if (res === "true") {

        //traigo la variable productoBase e hidrato componentes
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

        //verificamos que el producto tenga vencimiento
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
        if (prod.up_id === "07") {
            $("#up").focus();
        } else {
            $("#unid").focus();
        }
        //    //    //return true;
        //    //};
        //}

        //pongo true para que ejecute el callback que se declararon previamente

    }
    return true;
}

function InicializaPantallaCtrl() {
    productosGridCtrl();
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
    $("#divCtrlTIGrid").empty();


    return true;
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
            InicializaPantallaCtrl();
            return true;
        }
        else {
            CerrarWaiting();
            AbrirMensaje("Eliminar Producto", obj.msg, function () {
                $('#msjModal').modal('hide');
                return true;
            }, false, ["Aceptar"], "succ!", null);
            InicializaPantallaCtrl();
            return true;
        }
    })
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
            productosGridCtrl();
        }
        return true;
    });
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
            InicializaPantallaCtrl();


        }
        return true;
    })
}
