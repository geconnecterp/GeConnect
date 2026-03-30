var inventarioSeleccionado = null;
estado = {
    inv_lista: '',
    inv_box: '',
    inv_planilla: '',
    inv_valida_conteo: '',
    inv_conteo: '',
    inv_obtener_conteo: '',
    inv_confirmar: '',
    esBox: 'false', //determina si va por Box o Planilla
    invId: '', //determina cual es el ID seleccionado
    boxId: '', //determina cual es el box seleccionado. Si es '' (vacio) significa que sera un box nuevo
    inv_nro: '', //numero de inventario
    invt_id: '',//se puede determinar si es 'B'ox o 'S' conteo simple o 'D' conteo doble
    tipo: '',
    tipo_id: '',

};

/* ========================================================================
   SISTEMA DE SPINNER PARA BOTONES - GOLDEN THEME
   ======================================================================== */

/**
 * Activa el spinner en un botón y lo deshabilita
 * @param {string|jQuery} btnSelector - Selector CSS o objeto jQuery del botón
 * @param {string} textoLoading - Texto opcional a mostrar durante la carga (default: "Cargando...")
 * @returns {object} Estado original del botón para restauración
 */
function ActivarSpinnerBoton(btnSelector, textoLoading) {
    var $btn = typeof btnSelector === 'string' ? $(btnSelector) : btnSelector;
    
    if ($btn.length === 0) {
        console.warn('ActivarSpinnerBoton: No se encontró el botón con selector', btnSelector);
        return null;
    }

    // Guardar estado original
    var estadoOriginal = {
        contenidoHTML: $btn.html(),
        deshabilitado: $btn.prop('disabled'),
        clases: $btn.attr('class')
    };

    // Texto de carga
    var texto = textoLoading || 'Cargando...';
    
    // Crear spinner con boxicons (ya disponible en el proyecto)
    var spinnerHTML = '<i class="bx bx-loader-alt bx-spin me-1"></i>' + texto;

    // Aplicar spinner y deshabilitar
    $btn.prop('disabled', true)
        .html(spinnerHTML)
        .addClass('btn-loading'); // Clase adicional para estilos custom si se necesitan

    return estadoOriginal;
}

/**
 * Desactiva el spinner y restaura el estado original del botón
 * @param {string|jQuery} btnSelector - Selector CSS o objeto jQuery del botón
 * @param {object} estadoOriginal - Estado original retornado por ActivarSpinnerBoton
 */
function DesactivarSpinnerBoton(btnSelector, estadoOriginal) {
    var $btn = typeof btnSelector === 'string' ? $(btnSelector) : btnSelector;
    
    if ($btn.length === 0 || !estadoOriginal) {
        console.warn('DesactivarSpinnerBoton: Botón no encontrado o estado original inválido');
        return;
    }

    // Restaurar estado original
    $btn.prop('disabled', estadoOriginal.deshabilitado)
        .html(estadoOriginal.contenidoHTML)
        .removeClass('btn-loading');
}

/**
 * Wrapper para ejecutar función con spinner en botón
 * Maneja automáticamente la activación y desactivación del spinner
 * @param {string|jQuery} btnSelector - Selector del botón
 * @param {function} asyncFunction - Función asíncrona a ejecutar (debe retornar Promise o usar callbacks)
 * @param {string} textoLoading - Texto opcional durante la carga
 * @param {function} onFinally - Callback opcional que siempre se ejecuta al finalizar
 */
function EjecutarConSpinner(btnSelector, asyncFunction, textoLoading, onFinally) {
    var $btn = typeof btnSelector === 'string' ? $(btnSelector) : btnSelector;
    var estadoOriginal = ActivarSpinnerBoton($btn, textoLoading);

    if (!estadoOriginal) {
        console.error('EjecutarConSpinner: No se pudo activar el spinner');
        return;
    }

    // Función de limpieza
    var cleanup = function() {
        DesactivarSpinnerBoton($btn, estadoOriginal);
        if (typeof onFinally === 'function') {
            onFinally();
        }
    };

    // Ejecutar función asíncrona
    try {
        var result = asyncFunction();
        
        // Si retorna una Promise
        if (result && typeof result.then === 'function') {
            result
                .then(cleanup)
                .catch(function(error) {
                    console.error('Error en EjecutarConSpinner:', error);
                    cleanup();
                });
        } else {
            // Si no es Promise, asumir que maneja sus propios callbacks
            // El usuario debe llamar a DesactivarSpinnerBoton manualmente en callbacks
            // o pasar el cleanup como parámetro
        }
    } catch (error) {
        console.error('Error ejecutando función con spinner:', error);
        cleanup();
    }
}

/* ========================================================================
   FIN SISTEMA DE SPINNER PARA BOTONES
   ======================================================================== */

function PostGenHtml(data, path, retorno) {
    PostGen(data, path, retorno, fnError, "HTML");
}
function PostGenHtml(data, path, retorno, fxError) {
    PostGen(data, path, retorno, fxError, "HTML");
}
function PostGen(data, path, retorno) {
    PostGen(data, path, retorno, fnError, "json");
}
function PostGen(data, path, retorno, fxError, datatype) {
    $.ajax({
        "dataType": datatype,
        "url": path,
        "type": "POST",
        "data": data,
        "success": retorno,
        //beforeSend: function () { Bloquear();},
        error: fxError
    });
}

function fnError(jqXHR) {
    //alert(jqXHR);
    if (jqXHR.error)
        ControlaMensajeError(jqXHR.error);
    else
        ControlaMensajeError(jqXHR);
}

function AbrirWaiting(Mensaje) {
    if (Mensaje != "") {
        $('#lblWaiting').text(Mensaje);
    } else {
        $('#lblWaiting').text("Cargando...");
    }
    $('#wWaiting').fadeIn(0);
}


///debo mandar true siempre y cuando
///haya definido una funcion de callback, 
///para ejecutar funcionalidad luego de cerrar modal waiting
function CerrarWaiting(ejecutar) {
    $('#wWaiting').fadeOut(0);
    if (ejecutar === true) {
        FunctionCallback();
        return true;
    }
    return true;
}

function CerrarMensaje(Value) {
    //$('#msjModal').fadeOut(0);
    FunctionCallback(Value);
}

function AceptarMensaje(Value) {
    FunctionCallback(Value);
}

var FunctionCallback = null;
var FunctionCallBackExportar = null;
function AbrirMensaje(Titulo, Mensaje, CallBack, EsConfirmacion, Botones, Tipo, CallBackExportar) {
    if (EsConfirmacion) {
        if (Botones.length > 2) {
            $("#btnMensajeAceptar").show();
            $("#btnMensajeAlternativa").show();
            $("#btnMensajeCancelar").show();
        }
        else {
            $("#btnMensajeAceptar").show();
            $("#btnMensajeAlternativa").hide();
            $("#btnMensajeCancelar").show();
        }

    } else {
        $("#btnMensajeAceptar").show();
        $("#btnMensajeAlternativa").hide();
        $("#btnMensajeCancelar").hide();
    }
    if (Mensaje != null) {
        $('#msjContenido').html(Mensaje);
    } else {
        $('#msjContenido').html('Error inesperado, intente de nuevo en unos minutos...');
    }
    if (Titulo != null) {
        $('#msjTitulo').text(Titulo);
    } else {
        $('#msjTitulo').text('¡Atención!');
    }
    FunctionCallback = CallBack;
    if (Botones != null) {
        if (Botones.length == 1) {
            $("#btnMensajeAceptar").text(Botones[0]);
        }
        if (Botones.length == 2) {
            $("#btnMensajeAceptar").text(Botones[0]);
            $("#btnMensajeCancelar").text(Botones[1]);
        }
        else {
            $("#btnMensajeAceptar").text(Botones[0]);
            $("#btnMensajeAlternativa").text(Botones[1]);
            $("#btnMensajeCancelar").text(Botones[2]);
        }
        if (Botones.length == 0) {
            $("#btnMensajeCancelar").text("Cancelar");
        }
    } else {
        $("#btnMensajeAceptar").text("Aceptar");
        $("#btnMensajeCancelar").text("Cancelar");
    }
    //$('#msjModal').fadeIn(0);
    $("#msjIcono").html("");
    // Al inicio del switch, antes de aplicar nuevos estilos:
    $("#msjHeader").removeClass("info warn error success");
    // Aplicar clases según el tipo de mensaje
    switch (Tipo) {
        case "info!":
            $("#msjTitulo").prop("class", "text-info");
            $("#msjIcono").html('<i class="bx bx-md bx-spin bx-info-circle text-info"></i>');
            $("#msjHeader").addClass("info"); // Agregar clase al encabezado
            break;
        case "warn!":
            $("#msjTitulo").prop("class", "text-warning");
            $("#msjIcono").html('<i class="bx bx-md bx-spin bx-error text-warning"></i>');
            $("#msjHeader").addClass("warn"); // Agregar clase al encabezado
            break;
        case "error!":
            $("#msjTitulo").prop("class", "text-danger");
            $("#msjIcono").html('<i class="bx bx-md bx-spin bx-hand text-danger"></i>');
            $("#msjHeader").addClass("error"); // Agregar clase al encabezado
            break;
        case "succ!":
            $("#msjTitulo").prop("class", "text-success");
            $("#msjIcono").html('<i class="bx bx-md bx-spin bx-check text-success"></i>');
            $("#msjHeader").addClass("success"); // Agregar clase al encabezado (nota: usamos "success", no "succ")
            break;
        default:
            $("#msjIcono").prop("class", "");
            $("#msjIcono").html('');
            break;
    }

    $("#btnMensajeExportar").hide();
    if (CallBackExportar != null) {
        FunctionCallBackExportar = CallBackExportar;
        $("#btnMensajeExportar").show();
        $("#btnMensajeAceptar").hide();
        $("#btnMensajeCancelar").show();
    }

    $('#msjModal').modal('show');
}

function formatoFechaYMD(pFecha) {
    var f = new Date(pFecha);
    var month = ('0' + (f.getMonth() + 1)).slice(-2); // Asegura que el mes siempre tenga dos dígitos
    var day = ('0' + f.getDate()).slice(-2); // Asegura que el día siempre tenga dos dígitos
    return f.getFullYear() + '-' + month + '-' + day;
}

function restarFecha(pFecha, diasRestar) {
    var fecha = new Date(pFecha);
    fecha.setDate(fecha.getDate() - diasRestar);
    return fecha;
}

function sumarFecha(pFecha, diasSumar) {
    var fecha = new Date(pFecha);
    fecha.setDate(fecha.getDate() + diasSumar);
    return fecha;
}

function analizaEnterInput(e) {
    if (e.which == "13") {
        tope = 99999;
        index = -1;
        //obtengo los inputs dentro del div
        var inputss = $("main :input:not(:disabled)");
        tope = inputss.length;
        //le el id del input en el que he dado enter
        var cual = $(this).prop("id");
        inputss.each(function (i, item) {
            if ($(item).prop("id") === cual) {
                index = i;
                return false;
            }
        });
        if (index > -1 && tope > index + 1) {
            inputss[index + 1].focus();
        }

        ////verifico cuantos input habilitados encuentro
        //var $nextInput = $(this).nextAll("input:not(:disabled)");
        //if ($nextInput.length>0) {
        //    $nextInput.first().focus();
        //    return true;
        //} else if ($(this).prop("id") === "unid") {
        //    e.preventDefault();
        //    $("#btnCargarProd").focus();
        //}
    }
    return true;
}

//metodo se utliliza en inforpr y ti
function CargarAutoActual() {
    PostGen({}, ObtenerAutorizacionActualUrl, function (obj) {
        if (obj.error === true) {
            CerrarWaiting();
            AbrirMensaje("Importante", obj.msg, function () {
                $("#msjModal").modal("hide");
                return true;
            }, false, ["Aceptar"], "error!", null);
        }
        else {
            CerrarWaiting();
            autorizacionActual = obj.auto;
            ////permite activar o no el check que permite o no desarmar el paquete.
            //if ((obj.auto.tipoTI === "B" || obj.auto.tipoTI === "D") && obj.auto.sinAU === true) {
            //    //activamos el desarma
            //    $("#chkDesarma").prop("disabled", false);
            //}
            //else {
            //    $("#chkDesarma").prop("disabled", true);
            //}
            //permite activar o no el boton de carrito
            if (obj.auto.tipoTI === "S") {
                $("#btnCargaCarritoNuevo").removeClass("btn-success").addClass("btn-secundary link-noactivo");
            }
            else {
                $("#btnCargaCarritoNuevo").removeClass("btn-secundary link-noactivo").addClass("btn-success");
            }
        }
    });
}

//codigo generico para autocomplete 01
$("#Rel01").autocomplete({
    source: function (request, response) {
        data = { prefix: request.term }
        $.ajax({
            url: autoComRel01Url,
            type: "POST",
            dataType: "json",
            data: data,
            success: function (obj) {
                response($.map(obj, function (item) {
                    var texto = item.descripcion;
                    return { label: texto, value: item.descripcion, id: item.id };
                }));
            }
        })
    },
    minLength: 3,
    select: function (event, ui) {
        $("#Rel01Item").val(ui.item.id);
        var opc = "<option value=" + ui.item.id + ">" + ui.item.value + "</option>"
        $("#Rel01List").append(opc);
        return true;
    }
});

//codigo generico para autocomplete 02
$("#Rel02").autocomplete({
    source: function (request, response) {
        data = { prefix: request.term }
        $.ajax({
            url: autoComRel02Url,
            type: "POST",
            dataType: "json",
            data: data,
            success: function (obj) {
                response($.map(obj, function (item) {
                    var texto = item.descripcion;
                    return { label: texto, value: item.descripcion, id: item.id };
                }));
            }
        })
    },
    minLength: 3,
    select: function (event, ui) {
        $("#Rel02Item").val(ui.item.id);
        var opc = "<option value=" + ui.item.id + ">" + ui.item.value + "</option>"
        $("#Rel02List").append(opc);
        return true;
    }
});


function presentaPaginacion(div) {
    div.pagination({
        items: totalRegs,
        itemsOnPage: pagRegs,
        cssStyle: "dark-theme",
        currentPage: pagina,
        onPageClick: function (num) {
            //buscarProductos(num);
            if (funcCallBack !== null) {
                funcCallBack(num);
            }
        }
    });
    $("#pagEstado").val(false);
}
function analizaEnterInput(e) {
    if (e.which == "13") {
        tope = 99999;
        index = -1;
        //obtengo los inputs dentro del div
        var inputss = $("main :input:not(:disabled)");
        tope = inputss.length;
        //le el id del input en el que he dado enter
        var cual = $(this).prop("id");
        inputss.each(function (i, item) {
            if ($(item).prop("id") === cual) {
                index = i;
                return false;
            }
        });
        if (index > -1 && tope > index + 1) {
            inputss[index + 1].focus();
        }

        ////verifico cuantos input habilitados encuentro
        //var $nextInput = $(this).nextAll("input:not(:disabled)");
        //if ($nextInput.length>0) {
        //    $nextInput.first().focus();
        //    return true;
        //} else if ($(this).prop("id") === "unid") {
        //    e.preventDefault();
        //    $("#btnCargarProd").focus();
        //}
    }
    return true;
}
