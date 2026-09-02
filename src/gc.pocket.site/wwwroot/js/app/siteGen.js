var inventarioSeleccionado = null;
// Las mascaras muestran la coma como separador de miles (por ejemplo, 1,250),
// pero los controladores esperan recibir el valor numerico sin formato.
function NormalizarNumeroEntrada(valor, contexto) {
    if (valor === null || typeof valor === "undefined") {
        console.info("[Pocket][Numeros][" + (contexto || "general") + "] Valor sin normalizar", { recibido: valor });
        return valor;
    }

    var normalizado = String(valor).replace(/,/g, "");
    console.info("[Pocket][Numeros][" + (contexto || "general") + "] Normalizacion", {
        recibido: valor,
        normalizado: normalizado
    });
    return normalizado;
}

// Regla común de cantidades de producto para GECO Pocket:
// up_id 07 usa enteros; cualquier otro up_id admite punto decimal explícito
// y hasta tres posiciones. La coma queda reservada para separar miles.
function ConfigurarEntradaCantidadProducto(selector, upId, contexto) {
    var $control = $(selector);
    var permiteDecimales = String(upId || "").padStart(2, "0") !== "07";
    var espacioEventos = ".cantidadProductoPocket";

    if (typeof $control.unmask === "function") {
        $control.unmask();
    }

    $control.off(espacioEventos);
    $control.attr({
        inputmode: permiteDecimales ? "decimal" : "numeric",
        placeholder: permiteDecimales ? "0.000" : "0",
        maxlength: permiteDecimales ? 19 : 15
    });

    $control.on("input" + espacioEventos, function () {
        var valor = String(this.value || "").replace(/,/g, "");
        var parteEntera;
        var parteDecimal = "";

        if (permiteDecimales) {
            valor = valor.replace(/[^0-9.]/g, "");
            var posicionPunto = valor.indexOf(".");

            if (posicionPunto >= 0) {
                parteEntera = valor.substring(0, posicionPunto);
                parteDecimal = valor.substring(posicionPunto + 1).replace(/\./g, "").substring(0, 3);
            }
            else {
                parteEntera = valor;
            }

            parteEntera = parteEntera.replace(/\D/g, "").substring(0, 12);
            if (posicionPunto >= 0 && parteEntera.length === 0) {
                parteEntera = "0";
            }

            this.value = parteEntera + (posicionPunto >= 0 ? "." + parteDecimal : "");
            return;
        }

        this.value = valor.replace(/\D/g, "").substring(0, 12);
    });

    $control.on("blur" + espacioEventos, function () {
        var valor = String(this.value || "").replace(/,/g, "");
        if (valor === "") {
            return;
        }

        var partes = valor.split(".");
        var parteEntera = partes[0].replace(/\B(?=(\d{3})+(?!\d))/g, ",");
        this.value = parteEntera + (permiteDecimales && partes.length > 1 && partes[1] !== "" ? "." + partes[1] : "");
    });

    console.info("[Pocket][Cantidades][" + (contexto || "general") + "] Entrada configurada", {
        selector: selector,
        upId: upId,
        permiteDecimales: permiteDecimales,
        separadorDecimal: permiteDecimales ? "." : null,
        decimalesMaximos: permiteDecimales ? 3 : 0
    });
}

function FormatearCantidadProducto(valor, upId) {
    var permiteDecimales = String(upId || "").padStart(2, "0") !== "07";
    var numero = Number(NormalizarNumeroEntrada(valor, "FormatoCantidad"));

    if (!Number.isFinite(numero)) {
        return permiteDecimales ? "0.000" : "0";
    }

    return numero.toLocaleString("en-US", {
        minimumFractionDigits: permiteDecimales ? 3 : 0,
        maximumFractionDigits: permiteDecimales ? 3 : 0
    });
}

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

var confirmacionSeguraActiva = null;

function BloquearInteraccionDuranteConfirmacion(evento) {
    evento.preventDefault();
    evento.stopImmediatePropagation();
}

function AdvertirSalidaDuranteConfirmacion(evento) {
    evento.preventDefault();
    evento.returnValue = '';
    return '';
}

/**
 * Inicia una confirmación que no admite interacción ni navegación accidental.
 * Combina spinner en el botón, bloqueo visual, bloqueo de teclado y aria-busy.
 * @returns {object|null} Contexto que debe enviarse a FinalizarConfirmacionSegura.
 */
function IniciarConfirmacionSegura(btnSelector, mensajePantalla, textoBoton) {
    if (confirmacionSeguraActiva !== null) {
        console.warn('[Pocket][Confirmacion] Se ignoró una confirmación duplicada');
        return null;
    }

    var $btn = typeof btnSelector === 'string' ? $(btnSelector) : btnSelector;
    var estadoBoton = ActivarSpinnerBoton($btn, textoBoton || 'Procesando...');
    if (!estadoBoton) {
        return null;
    }

    var contexto = {
        boton: $btn,
        estadoBoton: estadoBoton,
        finalizada: false
    };

    confirmacionSeguraActiva = contexto;
    $btn.attr('aria-busy', 'true');
    $('#formulario').attr({ 'aria-busy': 'true', 'inert': '' });
    $('body').addClass('confirmacion-en-proceso');

    // Se usa la fase de captura para frenar primero el teclado físico y el
    // lector de códigos de los colectores, antes de que lleguen a la vista.
    document.addEventListener('keydown', BloquearInteraccionDuranteConfirmacion, true);
    window.addEventListener('beforeunload', AdvertirSalidaDuranteConfirmacion);

    AbrirWaiting(mensajePantalla || 'Procesando... Espere un momento por favor.');
    console.info('[Pocket][Confirmacion] Pantalla bloqueada y operación iniciada');
    return contexto;
}

/**
 * Libera todos los recursos de una confirmación. Es seguro llamarla una sola vez
 * desde cualquier salida: éxito, advertencia, error funcional o error HTTP.
 */
function FinalizarConfirmacionSegura(contexto) {
    if (!contexto || contexto.finalizada) {
        return false;
    }

    contexto.finalizada = true;
    CerrarWaiting();
    DesactivarSpinnerBoton(contexto.boton, contexto.estadoBoton);
    contexto.boton.removeAttr('aria-busy');
    $('#formulario').removeAttr('aria-busy inert');
    $('body').removeClass('confirmacion-en-proceso');
    document.removeEventListener('keydown', BloquearInteraccionDuranteConfirmacion, true);
    window.removeEventListener('beforeunload', AdvertirSalidaDuranteConfirmacion);

    if (confirmacionSeguraActiva === contexto) {
        confirmacionSeguraActiva = null;
    }

    console.info('[Pocket][Confirmacion] Pantalla y controles restaurados');
    return true;
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
    $('#wWaiting').attr('aria-hidden', 'false').fadeIn(0);
}


///debo mandar true siempre y cuando
///haya definido una funcion de callback, 
///para ejecutar funcionalidad luego de cerrar modal waiting
function CerrarWaiting(ejecutar) {
    $('#wWaiting').attr('aria-hidden', 'true').fadeOut(0);
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
function ObtenerBotonPredeterminadoMensaje(preferencia) {
    var selectores = {
        aceptar: "#btnMensajeAceptar",
        alternativa: "#btnMensajeAlternativa",
        cancelar: "#btnMensajeCancelar"
    };
    var selector = selectores[(preferencia || "").toLowerCase()] || preferencia;
    var boton = selector ? $(selector).filter(":visible:not(:disabled)").first() : $();

    if (boton.length === 0) {
        boton = $("#msjModal .modal-footer button:visible:not(:disabled)").first();
    }
    return boton;
}

function PrepararTecladoMensaje(botonPredeterminado) {
    var modal = $("#msjModal");
    modal.data("boton-predeterminado", botonPredeterminado || "aceptar");
    modal.off("shown.bs.modal.mensajeFoco")
        .one("shown.bs.modal.mensajeFoco", function () {
            setTimeout(function () {
                ObtenerBotonPredeterminadoMensaje(modal.data("boton-predeterminado")).trigger("focus");
            }, 0);
        });

    modal.off("keydown.mensajeEnter")
        .on("keydown.mensajeEnter", function (evento) {
            if (evento.key !== "Enter" || evento.altKey || evento.ctrlKey || evento.metaKey || evento.shiftKey) {
                return;
            }

            var boton = $(document.activeElement).filter("#msjModal button:visible:not(:disabled)");
            if (boton.length === 0) {
                boton = ObtenerBotonPredeterminadoMensaje(modal.data("boton-predeterminado"));
            }
            if (boton.length > 0) {
                evento.preventDefault();
                evento.stopPropagation();
                boton.trigger("click");
            }
        });
}

function AbrirMensaje(Titulo, Mensaje, CallBack, EsConfirmacion, Botones, Tipo, CallBackExportar, BotonPredeterminado) {
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

    PrepararTecladoMensaje(BotonPredeterminado);
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

function normalizarProveedorAutocomplete(item) {
    const descripcion = String(item.descripcion || "");
    const separador = descripcion.indexOf("#");
    const descripcionPrincipal = (separador >= 0 ? descripcion.substring(0, separador) : descripcion).trim();
    const tipoDescLegacy = separador >= 0 ? descripcion.substring(separador + 1).trim() : "";

    return {
        label: descripcionPrincipal,
        value: descripcionPrincipal,
        id: item.id,
        tipoDesc: String(item.tipoDesc || item.tipo_desc || tipoDescLegacy || "").trim()
    };
}

function aplicarRenderProveedorAutocomplete($input) {
    const autocomplete = $input.autocomplete("instance");
    if (!autocomplete) {
        return;
    }

    autocomplete._renderItem = function (ul, item) {
        const $contenido = $("<div>");
        $("<span>")
            .addClass("autocomplete-proveedor-principal")
            .text(item.label || "")
            .appendTo($contenido);

        if (item.tipoDesc) {
            $("<span>")
                .addClass("autocomplete-proveedor-tipo")
                .text(item.tipoDesc)
                .appendTo($contenido);
        }

        return $("<li>").append($contenido).appendTo(ul);
    };
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
                    return normalizarProveedorAutocomplete(item);
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

aplicarRenderProveedorAutocomplete($("#Rel01"));

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
