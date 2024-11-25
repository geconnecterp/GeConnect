$(function () {
    //cargo el js para llamar desde esta funcion a la función de busqueda
    //verifico si se hace click en el boton buscar
    $("#btnBusquedaBase").on("click", function () {
        InicializaPantallaASTK();
        buscarProducto();
        return true;
    });
    //input del control. Sirve para permitir inicializar pantalla.
    $("input#Busqueda").on("focus", function () {
        InicializaPantallaASTK();
    });

    //$("#unid").on('keypress', function (e) {
    //    // Permitir el signo negativo solo al inicio
    //    if (e.which === 45 && $(this).val().length === 0) {
    //        return true; // Permite el signo negativo
    //    }
    //    // Permitir solo números y el punto decimal
    //    if (e.which >= 48 && e.which <= 57 || e.which === 46) {
    //        return true; // Permite números y el punto decimal
    //    }
    //    return false; // Bloquea cualquier otra tecla
    //});
    //$("#box").on('keypress', function (e) {
    //    // Permitir el signo negativo solo al inicio
    //    if (e.which === 45 && $(this).val().length === 0) {
    //        return true; // Permite el signo negativo
    //    }
    //    //Permitir solo números y el punto decimal
    //    if (e.which >= 48 && e.which <= 57 || e.which === 46) {
    //        return true; // Permite números y el punto decimal
    //    }
    //    return false; // Bloquea cualquier otra tecla
    //});



    //este control debe ser insertado el mismo o similar para cada modulo.
    $("#estadoFuncion").on("change", verificaEstadoASTK);

    $(".inputEditable").on("keypress", analizaEnterInput);
    $("#btnCargarProd").on("click", cargarProductosASTK);
    InicializaPantallaASTK();

    return true;
})

function InicializaPantallaASTK() {
    productosGridASTK();
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
    if (tajValor === "B") {
        $("#spanSoloNegativos").empty().html("Atención: <strong>Solo se admiten valores NEGATIVOS</strong>");
    } else {
        $("#spanSoloNegativos").empty().html("Atención: <strong>Se admiten valores NEGATIVOS y POSITIVOS, en valores absolutos. Verifique el signo de la carga.</strong>");

    }

    return true;
}



function verificaEstadoASTK(e) {
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

        $("#box").mask("000,000,000,000", {
            reverse: true,
            regex: "[-]d{1,}",
            placeholder: "",
            showMaskOnHover: false
        });

        let patron = "";
        if (prod.up_id === "07") {  //unidades enteras
            if (tajValor === "M") {
                //admite valores positivos y negativos ENTEROS
                patron ="[-]d{1,}";                               
            }
            else {
                //admite valores negativos ENTEROS
                patron = "-d{1,}";                               
            }
            //$("#unid").mask("000,000,000,000", { reverse: true });
            $("#unid").mask('#,###', {
                reverse: true,
                //regex: "[-]?\\d+(\\.\\d+)?", // Permite números negativos, enteros y decimales
                regex: patron,
                placeholder: "",
                showMaskOnHover: false
            });
            $("#box").val(0).prop("disabled", false);
        }
        else { //unidades decimales
            $("#up").prop("readonly", true);
            $("#up").val(1);
            $("#up").addClass("backReadOnly");

            //$("#unid").mask("000,000,000,000.000", { reverse: true });
            if (tajValor === "M") {
                //admite valores positivos y negativos DECIMALES
                patron = "[-]d{1,3}[.]{0,1}d{0,2}";                
            }
            else {
                //admite valores negativos DECIMALES
                patron = "-d{1,3}[.]{0,1}d{0,2}";               
            }
            $("#unid").mask('#,###.###', {
                reverse: true,
                //regex: "[-]?\\d+(\\.\\d+)?", // Permite números negativos, enteros y decimales
                regex: patron,
                placeholder: "",
                showMaskOnHover: false
            });
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

function cargarProductosASTK() {
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
                    productosGridASTK();
                    $("#msjModal").modal("hide");
                    return true;
                }
                else if (resp === "SI2") {
                    //boton remplazar
                    RemplazarProducto();
                    productosGridASTK();
                    $("#msjModal").modal("hide");

                    return true;
                }
                else {
                    productosGridASTK();
                    $("#msjModal").modal("hide");
                    return true;
                }
            }, true, ["Acumular", "Reemplazar", "Cancelar"])

        }
        else {
            CerrarWaiting();
            ControlaMensajeSuccess("¡¡Producto cargado!!")
            productosGridASTK();
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
            productosGridASTK();
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

