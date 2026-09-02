function cargarProductos() {
    //1 tomar datos
    //2 mandarlos al servidor
    //3 almacenarlo en sesion (lista de productos)
    //4 devolver una grilla con los productos hidratados en la misma

    var _post = reguardarProductoEnListaUrl;
    var datos = null;
    console.info("[Pocket][RPR] Preparando carga de producto");
    var up = NormalizarNumeroEntrada($("#up").val(), "RPR.unidadesPorBulto");
    var vto = null;
    var box = NormalizarNumeroEntrada($("#box").val(), "RPR.bultos");
    var un = NormalizarNumeroEntrada($("#unid").val(), "RPR.unidadesSueltas");
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
            CerrarWaiting();
            var selectorCorreccion = "#" + (obj.campo || "Busqueda");
            console.warn("[Pocket][RPR] La carga requiere corrección", { campo: obj.campo, mensaje: obj.msg });
            AbrirMensaje("ATENCIÓN", obj.msg, function () {
                enfocarControlRpr(selectorCorreccion);
                $("#msjModal").modal("hide");
                return true;
            }, false, ["Aceptar"], "warn!", null, "aceptar");
            return true;
        }
        else if (obj.warn === true) {
            CerrarWaiting();
            AbrirMensaje("Atención", obj.msg, function (resp) {
                if (resp === "SI") {
                    //boton acumular
                    AcumularProducto();
                    $("#msjModal").modal("hide");
                    return true;
                }
                else if (resp === "SI2") {
                    //boton remplazar
                    RemplazarProducto();
                    $("#msjModal").modal("hide");

                    return true;
                }
                else {
                    productosGrid();
                    enfocarControlRpr("#Busqueda");
                    $("#msjModal").modal("hide");
                    return true;
                }
            }, true, ["Acumular", "Reemplazar", "Cancelar"], "warn!", null, "aceptar");

        }
        else {
            CerrarWaiting();
            ControlaMensajeSuccess("¡¡Producto cargado!!")
            productosGrid();
            enfocarControlRpr("#Busqueda");
            return true;
        }
    });
    return true;
}

function enfocarControlRpr(selector) {
    var aplicarFoco = function () {
        var control = $(selector).filter(":visible:not(:disabled)").first();
        if (control.length > 0) {
            control.trigger("focus");
            if (control.is("input:not([type='date'])")) {
                control.trigger("select");
            }
        }
    };

    if ($("#msjModal").hasClass("show")) {
        $("#msjModal").one("hidden.bs.modal.rprFocus", function () {
            setTimeout(aplicarFoco, 0);
        });
    }
    else {
        setTimeout(aplicarFoco, 0);
    }
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
            productosGrid();
            enfocarControlRpr("#Busqueda");
        }
        return true;
    });
}

function EliminarProducto(id,item) {
    AbrirWaiting("Espere... estamos procesando la solicitud...");
    PostGen({ p_id: id, item }, EliminarProductoUrl, function (obj) {
        if (obj.error === true) {
            CerrarWaiting();
            AbrirMensaje("Eliminar Producto", obj.msg, function () {
                $('#msjModal').modal('hide');
                return true;

            }, false, ["Aceptar"], "warn!", null);
            InicializaPantalla();
            return true;
        }
        else {
            CerrarWaiting();
            AbrirMensaje("Eliminar Producto", obj.msg, function () {
                $('#msjModal').modal('hide');
                return true;
            }, false, ["Aceptar"], "succ!", null);
            InicializaPantalla();
            return true;
        }
    })
}

function AcumularProducto() {
    console.info("[Pocket][RPR] Solicitando acumulación del producto seleccionado");
    AbrirWaiting("Espere... estamos procesando la solicitud...");
    PostGen({}, AcumularProductoUrl, function (obj) {
        if (obj.error === true) {
            console.error("[Pocket][RPR] No se pudo acumular el producto", obj);
            CerrarWaiting();
            AbrirMensaje("Acumular Producto", obj.msg, function () {
                $('#msjModal').modal('hide');
                return true;
            }, false, ["Aceptar"], "warn!", null);
        }
        else {
            console.info("[Pocket][RPR] Producto acumulado correctamente", obj);
            CerrarWaiting();
            AbrirMensaje("Acumular Producto", obj.msg, function () {
                $('#msjModal').modal('hide');
                return true;
            }, false, ["Aceptar"], "succ!", null);
            InicializaPantalla();
            enfocarControlRpr("#Busqueda");
        }
        return true;
    })
}

function productosGrid() {
    var data = {};
    PostGenHtml(data, PresentarProductosSeleccionadosUrl, function (obj) {


        $("#divRprGrid").html(obj);
        var tb = $("#divRprGrid #tbProdRPR tbody td");
        if (tb.length <= 0) {
            $("#btnContinuarRpr").hide("fast");
        } else {
            $("#btnContinuarRpr").show("fast");
        }

        if (typeof ocultarTrash !== 'undefined') {
            if (ocultarTrash === true) {
                //ocultamos la 8° columna
                $(".ocultar").toggle();
                $("#divRprGrid #tbProdRPR tbody td:nth-child(8)").toggle();
            }
        }


        return true;
    }, function (obj) {
        ControlaMensajeError(obj.message);
        return true;
    });
}

//function analizaEnterInput(e) {
//    if (e.which == "13") {
//        tope = 99999;
//        index = -1;
//        //obtengo los inputs dentro del div
//        var inputss = $("#divInputs :input:not(:disabled)");
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

//        ////verifico cuantos input habilitados encuentro
//        //var $nextInput = $(this).nextAll("input:not(:disabled)");
//        //if ($nextInput.length>0) {
//        //    $nextInput.first().focus();
//        //    return true;
//        //} else if ($(this).prop("id") === "unid") {
//        //    e.preventDefault();
//        //    $("#btnCargarProd").focus();
//        //}
//    }
//    return true;
//}

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
            ConfigurarEntradaCantidadProducto("#unid", prod.up_id, "RPR");
            $("#box").val(0).prop("disabled", false);
        }
        else { //unidades decimales
            $("#up").prop("readonly", true);
            $("#up").val(1);
            $("#up").addClass("backReadOnly");

            ConfigurarEntradaCantidadProducto("#unid", prod.up_id, "RPR");
        }
        $("#unid").val(0).prop("disabled", false);

        //activamos el boton
        $("#btnCargarProd").prop("disabled", false);

        //inicializamos el campo de busqueda
        $("#Busqueda").val("");

        //verificamos que el producto tenga vencimiento
        if (prod.p_con_vto !== "N") {
            //var f = new Date();
            //var month = ('0' + (f.getMonth() + 1)).slice(-2); // Asegura que el mes siempre tenga dos dígitos
            //var day = ('0' + f.getDate()).slice(-2); // Asegura que el día siempre tenga dos dígitos
            //var newfecha = f.getFullYear() + '-' + month + '-' + day;
            $("#fvto").prop("disabled", false).val(productoBase.p_con_vto_ctl);
            //asigno callback para que se ejecute luego que cierre el waiting
            /* FunctionCallback = function () {*/
            enfocarControlRpr("#fvto");
            //    //return true;
            //};
        } else {
            //asigno callback para que se ejecute luego que cierre el waiting
            /*FunctionCallback = function () {*/
            if (prod.up_id === "07") {
                enfocarControlRpr("#up");
            } else {
                enfocarControlRpr("#unid");
            }
            //    //return true;
            //};
        }

        //pongo true para que ejecute el callback que se declararon previamente

    }
    return true;
}

function InicializaPantalla() {
    productosGrid();
    $("#P_id").val("");
    $("#Descipcion").val("");
    $("#Rubro").val("");
    $("#up").val(0).prop("disabled", true);
    $("#up").prop("readonly", false);
    $("#up").removeClass("backReadOnly");
    $("#fvto").val("").prop("disabled", true);

    $("#box").val(0).prop("disabled", true);
    $("#unid").unmask().off(".cantidadProductoPocket").val(0).prop("disabled", true);
    $("#btnCargarProd").prop("disabled", true);
    $("#divRprGrid").empty();


    return true;
}
