$(function () {
    console.log('✅ Módulo OR Validación de Producto');

    // Inicializar eventos
    inicializarEventosValidacion();

    inicializaPropiedadesOR();
});

function inicializaPropiedadesOR() {
    $("#btnBusquedaBase").prop("disabled", false);
    $("input#Busqueda").on("focus", function () {
        InicializaBusqueda();
    });
}

function inicializarEventosValidacion() {

    $("#txtBox").on("input", validaInputBox);

    // ✅ NUEVO: Evento click para validar BOX
    $("#btnValBox").on("click", validarBoxIngresado);

    // ✅ NUEVO: Evento Enter en txtBox
    $("#txtBox").on("keypress", manejarEnterTxtBox);

    //chequea los enter que se dan sobre los controles editables
    $(".inputEditable").on("keypress", analizaEnterInput);

    $("#btnBusquedaBase").on("click", function () {
        buscarProducto();
        return true;
    });

    $("#estadoFuncion").on("change", verificaEstado); //este control debe ser insertado el mismo o similar para cada modulo.

    //ESTE BOTON CARGARÍA LOS DATOS AL CARRITO
    $("#btnCargarProd").on("click", cargarCarritoOR);

}

function validaInputBox() {
    var inputLength = $(this).val().length; // Obtener la longitud del texto ingresado

    if (inputLength === 11) {
        // Si el texto tiene exactamente 11 caracteres, activar el botón
        $("#btnValBox")
            .prop("disabled", false) // Activar el botón
            .removeClass("btn-danger") // Quitar la clase de color rojo
            .addClass("btn-success"); // Agregar la clase de color verde
        // $("#chkDesarma").prop("disabled", false);
    } else {
        // Si el texto tiene menos o más de 11 caracteres, desactivar el botón
        $("#btnValBox")
            .prop("disabled", true) // Desactivar el botón
            .removeClass("btn-success") // Quitar la clase de color verde
            .addClass("btn-danger"); // Agregar la clase de color rojo
        //$("#chkDesarma").prop("checked",true).prop("disabled", true);
        //InicializaVista();
    }
}

/**
 * ✅ NUEVA FUNCIÓN: Maneja el evento Enter en el input txtBox
 * Valida estados del input y botón antes de ejecutar la validación
 * @param {Event} e - Evento keypress
 */
function manejarEnterTxtBox(e) {
    // Verificar si la tecla presionada es Enter (código 13)
    if (e.which === 13 || e.keyCode === 13) {
        e.preventDefault(); // Prevenir comportamiento por defecto del Enter
        
        var $txtBox = $("#txtBox");
        var $btnValBox = $("#btnValBox");
        
        console.log("🔍 Enter detectado en txtBox");
        
        // ✅ PASO 1: Verificar que el input NO esté readonly
        if ($txtBox.prop("readonly")) {
            console.log("⚠️ Input txtBox está en modo readonly - Acción cancelada");
            return;
        }
        
        // ✅ PASO 2: Verificar que el input NO esté disabled
        if ($txtBox.prop("disabled")) {
            console.log("⚠️ Input txtBox está deshabilitado - Acción cancelada");
            return;
        }
        
        // ✅ PASO 3: Verificar que el botón NO esté disabled
        if ($btnValBox.prop("disabled")) {
            console.log("⚠️ Botón btnValBox está deshabilitado - Acción cancelada");
            mostrarMensaje("Debe ingresar un código de BOX válido (11 caracteres)", "warning");
            return;
        }
        
        // ✅ PASO 4: Todas las validaciones pasadas - Ejecutar click
        console.log("✅ Validaciones pasadas - Ejecutando validación de BOX");
        $btnValBox.trigger("click");
    }
}

/**
 * ✅ NUEVA FUNCIÓN: Valida el BOX ingresado contra el almacenado en sesión
 */
function validarBoxIngresado() {
    var boxIngresado = $("#txtBox").val().trim();

    if (boxIngresado.length !== 11) {
        mostrarMensaje("El código de BOX debe tener 11 caracteres", "warning");
        return;
    }

    // Deshabilitar botón durante validación
    $("#btnValBox").prop("disabled", true);
    
    console.log("📡 Validando BOX ingresado:", boxIngresado);

    $.ajax({
        url: validarBoxIngresadoUrl,
        type: "POST",
        data: {
            boxIngresado: boxIngresado
        },
        beforeSend: function () {
            mostrarCargando("Validando BOX...");
        },
        success: function (response) {
            ocultarCargando();

            if (response.success) {
                console.log("✅ BOX validado correctamente");
                mostrarMensaje(response.message, "success");
                
                // Deshabilitar input y botón de validación
                $("#txtBox").prop("readonly", true);
                $("#btnValBox").removeClass("btn-success").addClass("btn-secondary");
                
                // ✅ Mostrar sección de búsqueda de producto
                $("#divBusquedaProd").slideDown(75);
                
                // Enfocar en el input de búsqueda de producto
                setTimeout(function() {
                    $("#txtBuscar").trigger("focus");
                }, 120);
                
            } else {
                console.warn("⚠️ Validación fallida:", response.message);
                mostrarMensaje(response.message || "El BOX ingresado no coincide con el seleccionado", "error");
                
                // Limpiar input y mantener foco
                $("#txtBox").val("").trigger("focus");
                $("#btnValBox")
                    .prop("disabled", true)
                    .removeClass("btn-success")
                    .addClass("btn-danger");
            }
        },
        error: function (xhr, status, error) {
            ocultarCargando();
            console.error("❌ Error al validar BOX:", error);
            
            var mensaje = "Error al validar BOX";
            if (xhr.responseJSON && xhr.responseJSON.message) {
                mensaje = xhr.responseJSON.message;
            }
            
            mostrarMensaje(mensaje, "error");
            
            // Rehabilitar botón en caso de error
            $("#btnValBox").prop("disabled", false);
        }
    });
}

/**
 * Helper: Mostrar mensajes toast
 */
function mostrarMensaje(mensaje, tipo) {
    var icono = tipo === "success" ? "✅" : tipo === "warning" ? "⚠️" : "❌";
    var clase = tipo === "success" ? "alert-success" : tipo === "warning" ? "alert-warning" : "alert-danger";
    
    var alertHtml = `
        <div class="alert ${clase} alert-dismissible fade show" role="alert">
            ${icono} ${mensaje}
            <button type="button" class="btn-close" data-bs-dismiss="alert"></button>
        </div>
    `;
    
    // Insertar antes del primer row
    $(".row").first().before(alertHtml);
    
    // Auto-ocultar después de 5 segundos
    setTimeout(function() {
        $(".alert").fadeOut(function() {
            $(this).remove();
        });
    }, 5000);
}

/**
 * Helper: Mostrar indicador de carga
 */
function mostrarCargando(mensaje) {
    if (typeof Swal !== 'undefined') {
        Swal.fire({
            title: mensaje || 'Procesando...',
            allowOutsideClick: false,
            didOpen: () => {
                Swal.showLoading();
            }
        });
    }
}

/**
 * Helper: Ocultar indicador de carga
 */
function ocultarCargando() {
    if (typeof Swal !== 'undefined') {
        Swal.close();
    }
}

function InicializaBusqueda() {
    $("input#Busqueda").val("");
    $("#P_id").val("");
    $("#Descipcion").val("");
    $("#Rubro").val("");
    $("#up").val(0).prop("disabled", true);
    $("#fvto").val("").prop("disabled", true);

    $("#box").val(0).prop("disabled", true);
    $("#unid").val(0).prop("disabled", true);
    $("#btnCargaProd").prop("disabled", true);

    //si el desarma esta activado
    if ($("#chkDesarma").is(":disabled") === false) {
        if ($("#chkDesarma").is(":checked") === true) {
            $("input#Busqueda").prop("disabled", false);
            $("btnBusquedaBase").prop("disabled", false);
        }
        else {
            $("input#Busqueda").prop("disabled", true);
            $("btnBusquedaBase").prop("disabled", true);
        }
    }
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
                    $("#Busqueda").trigger("focus");

                    return true;
                }, false, ["Aceptar"], "error!", null);
            }
            else if (obj.warn === true) {
                AbrirMensaje("Importante", obj.msg, function () {
                    $("#msjModal").modal("hide");
                    $("#Busqueda").val("");
                    $("#Busqueda").trigger("focus");

                    return true;
                }, false, ["Aceptar"], "warn!", null);
            }
            else {
                ControlaMensajeSuccess(obj.msg);
                //traigo la variable productoBase e hidrato componentes
                var prod = productoBase;
                var prodAct = productoActualOR;

                //se procedera a buscar la fecha de vencimiento del producto dependiendo del box en el que estamos trabajando.
                var bId = $("#txtBox").val();
                if (bId === "" || bId === undefined) {
                    InicializaBusqueda();
                    $("#msjModal").modal("hide");
                    $("#Busqueda").val("");
                    $("#Busqueda").trigger("focus");
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
                                $("#Busqueda").trigger("focus");

                                return true;
                            }, false, ["Aceptar"], "error!", null);
                        }
                        else if (obj.warn === true) {
                            AbrirMensaje("Importante", obj.msg, function () {
                                $("#msjModal").modal("hide");
                                $("#Busqueda").val("");
                                $("#Busqueda").trigger("focus");

                                return true;
                            }, false, ["Aceptar"], "warn!", null);
                        }
                        else {

                            $("#P_id").val(prod.p_id);
                            $("#Marca").val(prod.p_m_marca);
                            $("#Descipcion").val(prod.p_desc);
                            $("#Rubro").val(prod.rub_desc);
                            //$("#up").mask("000.000.000.000", { reverse: true });

                            //charly confirma 12/03/2026 unidad de presentacion que trae producto
                            $("#up").val(prod.p_unidad_pres).prop("disabled", false);
                            //if (prodAct.unidad_pres === 0) {
                            //    $("#up").val(prod.p_unidad_pres).prop("disabled", false);
                            //} else {
                            //    $("#up").val(prodAct.unidad_pres).prop("disabled", false);
                            //}
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
                            $("#btnCargarProd")
                                .prop("disabled", false) // Activar el botón
                                .removeClass("btn-danger") // Quitar la clase de color rojo
                                .addClass("btn-success"); // Agregar la clase de color verde

                            //inicializamos el campo de busqueda
                            $("#Busqueda").val("");

                            if (prod.p_con_vto !== "N" && prod.p_con_vto !== null && prod.p_con_vto !== " ") {
                                $("#fvto").prop("disabled", false);
                                $("#fvto").trigger("focus");

                            } else {
                                $("#up").trigger("focus");
                            }
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

function cargarCarritoOR() {
    //aca se validará previamente si la cantidad ingresada corresponde a lo solicitado
    AbrirWaiting()
    var cantSolic = productoActualOR.pedido;
    var desarma = $("#chkDesarma").is(":checked");
    //var upId = 0;
    //if (productoActualOR.sinAU === true) {
    //    productoActualOR.pId = $("#P_id").val();
    //}


    if (desarma === true) {
        var upId = productoBase.up_id;
        var cantidad = 0;
        console.info("[Pocket][OrdenReparto] Calculando cantidad para el carrito");
        var up = parseInt(NormalizarNumeroEntrada($("#up").val(), "OrdenReparto.unidadesPorBulto"));
        var bulto = parseInt(NormalizarNumeroEntrada($("#box").val(), "OrdenReparto.bultos"));
        var unid = parseFloat(NormalizarNumeroEntrada($("#unid").val(), "OrdenReparto.unidadesSueltas"))
        var fv = $("#fvto").val();
        if (upId === "07") {
            cantidad = (up * bulto) + unid;
        } else {
            cantidad = unid;
        }

        ////los que tienen que tener cantidad exacta seran tambien los que tengan upId!==07
        if (cantidad > cantSolic && upId === "07" && productoActualOR.sinAU === false) {
            CerrarWaiting();

            AbrirMensaje("Atención", "La cantidad ingresada" + cantidad + "no corresponde a la cantidad solicitada (" + cantSolic + "). Verifique.", function () {
                $("#msjModal").modal("hide");
                $("#up").trigger("focus");
                return true;
            }, false, ["Aceptar"], "warn!", null);
        }
        else {
            //ControlaMensajeSuccess("Cantidad correcta");
            //se procede a enviar el producto a cargar
            var dato = { p_id: productoActualOR.p_id, up, bulto, unid, cantidad, fv }
            PostGen(dato, ResguardarProductoCarritoORUrl, function (obj) {
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
                        window.location.href = proximoProductoUrl + `?or_compte=${orActual}`;
                    }, false, ["Aceptar"], "succ!", null);
                }
            });

        }
    } else {
        //ControlaMensajeSuccess("Cantidad correcta");
        //se procede a enviar el producto a cargar
        var dato = { p_id: productoActualOR.pId, up: 0, bulto: 0, unid: 0, cantidad: 0, fv: null, desarma }
        PostGen(dato, ResguardarProductoCarritoORUrl, function (obj) {
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
                window.location.href = proximoProductoUrl + `?or_compte=${orActual}`;
            }
        });
    }
}
