$(function () {
    //cargo el js para llamar desde esta funcion a la función de busqueda
    //verifico si se hace click en el boton buscar
    $("#btnBusquedaBase").on("click", function () {
        buscarProducto();
        return true;
    });

    $("#estadoFuncion").on("change", verificaEstado); //este control debe ser insertado el mismo o similar para cada modulo.

    $("#txtBox").on("input", function () {
        var inputLength = $(this).val().length; // Obtener la longitud del texto ingresado

        if (inputLength === 11) {
            // Si el texto tiene exactamente 11 caracteres, activar el botón
            $("#btnValBox")
                .prop("disabled", false) // Activar el botón
                .removeClass("btn-danger") // Quitar la clase de color rojo
                .addClass("btn-success"); // Agregar la clase de color verde
            //$("#chkDesarma").prop("disabled", false);
        } else {
            // Si el texto tiene menos o más de 11 caracteres, desactivar el botón
            $("#btnValBox")
                .prop("disabled", true) // Desactivar el botón
                .removeClass("btn-success") // Quitar la clase de color verde
                .addClass("btn-danger"); // Agregar la clase de color rojo
            $("#chkDesarma").prop("checked",true).prop("disabled", true);
            //InicializaVista();
        }
    });

    //$("#btnCleanProd").on("click", limpiarProductoCarrito);

    //este boton valida el BOX
    $("#btnValBox").on("click", validaBoxCarrito);

    //ESTE BOTON CARGARÍA LOS DATOS AL CARRITO
    $("#btnCargarProd").on("click", cargarCarrito);
    //chequea los enter que se dan sobre los controles editables
    $(".inputEditable").on("keypress", analizaEnterInput);

    $("#btnBusquedaBase").prop("disabled", false);
    $("input#Busqueda").on("focus", function () {
        InicializaBusqueda();
    });
    $("#chkDesarma").on("click", function () {
        var tipoti = autorizacionActual.tipoTI;
        var sinau = autorizacionActual.sinAU;
        if ($("#chkDesarma").is(":checked")) {
            //habilito los controles
            $("#btnBusquedaBase").prop("disabled", true);
            $("input#Busqueda").prop("disabled", true);
            $("#btnCargarProd")
                .prop("disabled", true) // Desactivar el botón
                .removeClass("btn-success") // Quitar la clase de color verde
                .addClass("btn-danger"); // Agregar la clase de color rojo
            
        }
        else {
            $("#btnBusquedaBase").prop("disabled", true);
            $("input#Busqueda").prop("disabled", true);
           
            $("#btnCargarProd")
                .prop("disabled", false) // Activar el botón
                .removeClass("btn-danger") // Quitar la clase de color rojo
                .addClass("btn-success"); // Agregar la clase de color verde
        }
    });

    CargarAutoActual();

    //activa el desarma en funcion del valor inferido al inicio de la vista
    InicializaVista();
});

function InicializaVista() {
    if (activarCheckDesarma === true) {
        $("#chkDesarma").prop("checked", true);
    }
    else {
        $("#chkDesarma").prop("checked", false);
    }

    $("#btnCargarProd")
        .prop("disabled", true) // Desactivar el botón
        .removeClass("btn-success") // Quitar la clase de color verde
        .addClass("btn-danger"); // Agregar la clase de color rojo


    $("#btnBusquedaBase").prop("disabled", true);
    $("input#Busqueda").prop("disabled", true);
}

function InicializaBusqueda() {
    $("#P_id").val("");
    $("#Descipcion").val("");
    $("#Rubro").val("");
    $("#up").val(0).prop("disabled", true);
    $("#fvto").val("").prop("disabled", true);

    $("#box").val(0).prop("disabled", true);
    $("#unid").val(0).prop("disabled", true);
    $("#btnCargaProd").prop("disabled", true);
}

function cargarCarrito() {
    //aca se validará previamente si la cantidad ingresada corresponde a lo solicitado
    AbrirWaiting()
    var cantSolic = autorizacionActual.pPedido;
    var desarma = $("#chkDesarma").is(":checked");
    //var upId = 0;
    if (autorizacionActual.sinAU === true) {
        autorizacionActual.pId = $("#P_id").val();
    }
    

    if (desarma === true) {
        var upId = productoBase.up_id;
        var cantidad = 0;
        var up = parseInt($("#up").val());
        var bulto = parseInt($("#box").val());
        var unid = parseFloat($("#unid").val())
        var fv = $("#fvto").val();
        if (upId === "07") {
            cantidad = (up * bulto) + unid;
        } else {
            cantidad = unid;
        }

        ////los que tienen que tener cantidad exacta seran tambien los que tengan upId!==07
        if (cantidad > cantSolic && upId === "07" && autorizacionActual.sinAU === false) {
            CerrarWaiting();

            AbrirMensaje("Atención", "La cantidad ingresada" + cantidad + "no corresponde a la cantidad solicitada (" + cantSolic + "). Verifique.", function () {
                $("#msjModal").modal("hide");
                $("#up").focus();
                return true;
            }, false, ["Aceptar"], "warn!", null);
        }
        else {
            //ControlaMensajeSuccess("Cantidad correcta");
            //se procede a enviar el producto a cargar
            var dato = { p_id: autorizacionActual.pId, up, bulto, unid, cantidad, fv}
            PostGen(dato, ResguardarProductoCarritoUrl, function (obj) {
                if (obj.error === true) {
                    CerrarWaiting();

                    AbrirMensaje("Importante", obj.msg, function () {
                        $("#msjModal").modal("hide");
                        return true;
                    }, false, ["Aceptar"], "error!", null);
                } else if (obj.warn === true) {
                    CerrarWaiting();
                    AbrirMensaje("Importante", obj.msg, function () {
                        $("#msjModal").modal("hide");
                        return true;
                    }, false, ["Aceptar"], "warn!", null);
                }
                else {
                    CerrarWaiting();
                    AbrirMensaje("Importante", obj.msg, function () {
                        $("#msjModal").modal("hide");
                        window.location.href = proximoProductoUrl + "?esrubro=false&esbox=false&tiId=" + autorizacionActual.tipoTI;                        
                    }, false, ["Aceptar"], "succ!", null);                                     
                }
            });

        }
    } else {
        //ControlaMensajeSuccess("Cantidad correcta");
        //se procede a enviar el producto a cargar
        var dato = { p_id: autorizacionActual.pId, up: 0, bulto: 0, unid: 0, cantidad: 0, fv: null, desarma }
        PostGen(dato, ResguardarProductoCarritoUrl, function (obj) {
            if (obj.error === true) {
                CerrarWaiting();

                AbrirMensaje("Importante", obj.msg, function () {
                    $("#msjModal").modal("hide");
                    return true;
                }, false, ["Aceptar"], "error!", null);
            } else if (obj.warn === true) {
                CerrarWaiting();
                AbrirMensaje("Importante", obj.msg, function () {
                    $("#msjModal").modal("hide");
                    return true;
                }, false, ["Aceptar"], "warn!", null);
            }
            else {
                CerrarWaiting();

                ControlaMensajeSuccess(obj.msg);
                window.location.href = proximoProductoUrl + "?esrubro=false&esbox=false&tiId=" + autorizacionActual.tipoTI;
            }
        });
    }
}


//esta funcion verifica si el box que ingresa corresponde al box del producto
function validaBoxCarrito() {
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
            if ($("#chkDesarma").prop("disabled") === false && !$("#chkDesarma").is(":checked")) {
                $("#Busqueda").prop("disabled", true);
                $("#btnCargarProd").prop("disabled", false);
            } else {
                //solo pasa al otro campo.           
                $("#btnCargarProd").prop("disabled", true);
                $("#Busqueda").prop("disabled",false);
                $("#Busqueda").focus();
            }
            
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

                //se procedera a buscar la fecha de vencimiento del producto dependiendo del box en el que estamos trabajando.
                var bId = $("#txtBox").val();
                if (bId === "" || bId === undefined) {
                    InicializaBusqueda();
                    $("#msjModal").modal("hide");
                    $("#Busqueda").val("");
                    $("#Busqueda").focus();
                    AbrirMensaje("Atención", "No se ha seleccionado Box aún. Seleccionelo y vuelva a buscar el producto.", function () {
                        $("#msjModal").modal("hide");
                        return true;
                    }, false, ["Aceptar"], "warn!", null)

                }
                else {
                    //buscamos el vencimiento
                    dato = { pId: productoBase.p_id, bId };

                    PostGen(dato, buscarFechaVtoUrl, function (obj) {
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

                            $("#P_id").val(prod.p_id);
                            $("#Marca").val(prod.p_m_marca);
                            $("#Descipcion").val(prod.p_desc);
                            $("#Rubro").val(prod.rub_desc);
                            //$("#up").mask("000.000.000.000", { reverse: true });
                            if (autoAct.pUnidPres === 0) {
                                $("#up").val(prod.p_unidad_pres).prop("disabled", false);
                            } else {
                                $("#up").val(autoAct.pUnidPres).prop("disabled", false);
                            }
                            //$("#unid").mask("000,000,000,000", { reverse: true });

                            if (obj.vto !== "") {
                                var f = new Date(obj.vto);
                                $("#fvto").val(formatoFechaYMD(f));
                            }

                            if (prod.up_id === "07") {  //unidades enteras
                                $("#unid").mask("000,000,000,000", { reverse: true });
                                $("#unid").val(0).prop("disabled", false);
                                $("#box").val(0).prop("disabled", false);
                            }
                            else { //unidades decimales
                                $("#unid").mask("000,000,000,000.000", { reverse: true });
                                $("#unid").val(0).prop("disabled", false);
                               // $("#box").val(0).prop("disabled", true);
                            }

                            //if (prod.sinAU === true) {
                            //    $("#chkDesarma").prop("disabled", false);
                            //}


                            

                            //activamos el boton
                            $("#btnCargarProd").prop("disabled", false);

                            //inicializamos el campo de busqueda
                            $("#Busqueda").val("");

                            if (prod.p_con_vto !== "N" && prod.p_con_vto !== null && prod.p_con_vto !== " ") {                               
                                $("#fvto").prop("disabled", false);                             
                                $("#fvto").focus();
                               
                            } else {                               
                                $("#up").focus();                               
                            }                           

                            return true;

                        }
                    });
                    
                }
            }
        });

        $("#estadoFuncion").val(false);

        //PresentarStkD(prod.p_Id);

        $("#btnBusquedaBase").prop("disabled", false);

    }
    return true;
}