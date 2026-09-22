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
    var boxIngresado = $(this).val().replace(/\D/g, "").slice(0, 11);
    $(this).val(boxIngresado);
    var inputLength = boxIngresado.length;

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
                $("#txtBox").val(response.data?.boxId || boxIngresado).prop("readonly", true);
                $("#btnValBox").removeClass("btn-success").addClass("btn-secondary");
                
                // ✅ Mostrar sección de búsqueda de producto
                $("#divBusquedaProd").slideDown(75);
                
                // Enfocar en el input de búsqueda de producto
                setTimeout(function() {
                    $("#Busqueda").trigger("focus");
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
    $("#btnCargarProd").prop("disabled", true);

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
                                ConfigurarEntradaCantidadProducto("#unid", prod.up_id, "OrdenReparto");
                                $("#unid").val(0).prop("disabled", false);
                                $("#box").val(0).prop("disabled", false);
                            }
                            else { //unidades decimales
                                ConfigurarEntradaCantidadProducto("#unid", prod.up_id, "OrdenReparto");
                                $("#unid").val(0).prop("disabled", false);
                                $("#box").val(0).prop("disabled", true);
                                $("#up").val(1).prop("disabled", true);
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

var cargaOREnCurso = false;
function textoSeguroOR(valor) { return $("<div>").text(valor ?? "").html(); }
function formatoCantidadOR(valor) {
    return Number(valor || 0).toLocaleString("es-AR", { maximumFractionDigits: productoBase.up_id === "07" ? 0 : 3 });
}
function avisoCargaOR(mensaje, tipo, continuar) {
    AbrirMensaje("Importante", textoSeguroOR(mensaje), function () {
        $("#msjModal").modal("hide");
        if (continuar) continuar();
    }, false, ["Aceptar"], tipo || "warn!", null);
}
function enviarCargaOR(dato) {
    if (cargaOREnCurso) return;
    cargaOREnCurso = true;
    $("#btnCargarProd").prop("disabled", true);
    AbrirWaiting("Registrando producto...");
    PostGen(dato, ResguardarProductoCarritoORUrl, function (obj) {
        CerrarWaiting();
        cargaOREnCurso = false;
        if (!obj || obj.error !== false || obj.warn !== false) {
            $("#btnCargarProd").prop("disabled", false);
            if (obj?.reconsultar && obj.producto) productoActualOR = obj.producto;
            avisoCargaOR(obj?.msg || "No se recibió una respuesta válida. Actualice el listado antes de reintentar.", obj?.error ? "error!" : "warn!");
            return;
        }
        avisoCargaOR(obj.msg, "succ!", function () {
            window.location.href = proximoProductoUrl;
        });
    }, function () {
        CerrarWaiting();
        // Resultado incierto: no reintentar automáticamente ni permitir un doble envío.
        avisoCargaOR("No se pudo confirmar la respuesta. Volveremos al listado para verificar lo registrado antes de otra carga.", "warn!", function () {
            window.location.href = proximoProductoUrl;
        });
    }, "json");
}
function resolverCargaPreviaOR(dato) {
    var previa = Number(productoActualOR.colectado || 0);
    dato.cantidadPrevia = previa;
    dato.bultosPrevios = productoActualOR.bulto;
    dato.unidadesPrevias = productoActualOR.us;
    dato.upPrevia = productoActualOR.unidad_pres;
    if (esReemplazoOR || previa <= 0) {
        dato.modoCarga = "nueva";
        enviarCargaOR(dato);
        return;
    }
    var mensaje = "El producto <strong>" + textoSeguroOR(productoActualOR.p_id + " " + productoActualOR.p_desc) +
        "</strong> tiene <strong>" + formatoCantidadOR(previa) + "</strong> colectado por su usuario.<br><br>" +
        "Sobrescribir carga: quedará en <strong>" + formatoCantidadOR(dato.cantidad) + "</strong>.<br>" +
        "Acumular: quedará en <strong>" + formatoCantidadOR(previa + dato.cantidad) + "</strong>.<br><br>" +
        "Esta decisión modifica cantidades; no reemplaza el producto.";
    AbrirMensaje("Producto ya colectado", mensaje, function (respuesta) {
        $("#msjModal").modal("hide");
        if (respuesta !== "SI" && respuesta !== "SI2") return;
        dato.modoCarga = respuesta === "SI2" ? "acumular" : "sobrescribir";
        enviarCargaOR(dato);
    }, true, ["Sobrescribir carga", "Acumular", "Cancelar"], "warn!", null, "cancelar");
}
function cargarCarritoOR() {
    if (cargaOREnCurso) return;
    if (!productoBase || !$("#chkDesarma").is(":checked")) {
        avisoCargaOR("Busque el producto para realizar una colecta individual.");
        return;
    }
    var up = Number(NormalizarNumeroEntrada($("#up").val(), "OR.presentacion"));
    var bulto = productoBase.up_id === "07" ? Number(NormalizarNumeroEntrada($("#box").val(), "OR.bultos")) : 0;
    var unid = Number(NormalizarNumeroEntrada($("#unid").val(), "OR.unidades"));
    // Redondeo de la aritmética JS, no de entradas con precisión inválida.
    var cantidad = Number((productoBase.up_id === "07" ? up * bulto + unid : unid).toFixed(3));
    if (![up, bulto, unid, cantidad].every(Number.isFinite) || !Number.isInteger(up) || up < 1 || bulto < 0 || unid < 0 || cantidad <= 0) {
        avisoCargaOR("Ingrese cantidades válidas y positivas.");
        return;
    }
    if (Number(unid.toFixed(3)) !== unid || Number(bulto.toFixed(1)) !== bulto) {
        avisoCargaOR("Se permiten hasta 3 decimales en unidades y 1 en bultos.");
        return;
    }
    resolverCargaPreviaOR({ p_id: productoBase.p_id, item: productoActualOR.item,
        contextoCarga: contextoCargaOR,
        orCompte: productoActualOR.or_compte, productoOriginal: productoActualOR.p_id, boxOriginal: productoActualOR.box_id,
        up, bulto, unid, cantidad, fv: $("#fvto").val(), desarma: true });
}
