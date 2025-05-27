//variables globales que son declaradas al inicio para que tengan alcance a la 
//mayor cantidad de codigo.
var nnControlCta01 = "";
var nnControlCta02 = "";
var nnControlCta03 = "";
var nnControlCta04 = "";

//arreglo destinado a contener los parametros del los reportes
//Inicialmente se genera con 300 posiciones
var arrRepoParams = new Array(300);

$(function () {

    // Reemplaza o modifica tu código actual de manejo de dropdown-submenu
    // Asegura que los clics en los elementos del menú no cierren el menú principal
    $('.dropdown-menu a.dropdown-toggle').on('click', function (e) {
        if ($(window).width() >= 992) {
            var $subMenu = $(this).next('.dropdown-menu');

            // Toggle la clase show para el submenu
            $subMenu.toggleClass('show');

            // Toggle la clase show para el item padre (para el giro del ícono)
            $(this).parent('.dropdown-submenu').toggleClass('show');

            // Oculta otros submenús al mismo nivel y quita su clase show
            var $siblings = $(this).parent().siblings();
            $siblings.removeClass('show');
            $siblings.find('.dropdown-menu').removeClass('show');

            // Previene cierre automático
            e.stopPropagation();
            e.preventDefault();
        }
    });

    // Para dispositivos móviles
    $('.dropdown-submenu > a').on('click', function (e) {
        if ($(window).width() < 992) {
            // Previene navegación si hay submenú
            if ($(this).next('.dropdown-menu').length > 0) {
                e.preventDefault();
                e.stopPropagation();

                // Toggle el submenú actual
                var submenu = $(this).next('.dropdown-menu');
                var parent = $(this).parent();

                if (submenu.is(':visible')) {
                    submenu.hide();
                    parent.removeClass('show');
                } else {
                    // Oculta otros submenús abiertos al mismo nivel y quita show
                    var siblings = parent.siblings();
                    siblings.removeClass('show');
                    siblings.find('.dropdown-menu').hide();

                    // Muestra este submenú y añade show
                    submenu.show();
                    parent.addClass('show');
                }
            }
        }
    });






    //const mainContent = $("main"); // Ajusta el selector según tu estructura HTML

    const modal = $("#msjModal");
    const btnAceptar = $("#btnMensajeAceptar");

    // Gestionar el foco y eliminar aria-hidden al mostrar el modal
    modal.on("show.bs.modal", function () {
        // Asegúrate de que aria-hidden no esté presente
        modal.removeAttr("aria-hidden");

        //mainContent.attr("inert", "true"); // Desactiva el contenido principal
    });

    // Mover el foco al botón "Aceptar" cuando el modal se abre
    modal.on("shown.bs.modal", function () {
        btnAceptar.focus();
    });

    // Restaurar aria-hidden y el foco al cerrar el modal
    modal.on("hide.bs.modal", function () {
        // Opcional: Si necesitas ocultar el modal de los lectores de pantalla
        modal.attr("aria-hidden", "true");

        //mainContent.removeAttr("inert"); // Reactiva el contenido principal
    });

    // Restaurar el foco al elemento que activó el modal cuando se cierra
    modal.on("hidden.bs.modal", function () {
        const triggerElement = $(document.activeElement);
        triggerElement.focus();
    });

    //check generico REL01 activando componentes disables
    $("#chkRel01").on("click", function () {
        if ($("#chkRel01").is(":checked")) {
            $("#Rel01").prop("disabled", false);
            $("#Rel01List").prop("disabled", false);
            $("#Rel01").trigger("focus");
        }
        else {


            $("#Rel01").prop("disabled", true).val("");
            $("#Rel01List").prop("disabled", true).empty();

        }
    });

    //check generico REL02 activando componentes disables
    $("#chkRel02").on("click", function () {
        if ($("#chkRel02").is(":checked")) {
            $("#Rel02").prop("disabled", false);
            $("#Rel02List").prop("disabled", false);
            $("#Rel02").trigger("focus");

        }
        else {
            $("#Rel02").prop("disabled", true).val("");
            $("#Rel02List").prop("disabled", true).empty();
        }
    });

    //check generico REL03 activando componentes disables
    $("#chkRel03").on("click", function () {
        if ($("#chkRel03").is(":checked")) {
            $("#Rel03").prop("disabled", false);
            $("#Rel03List").prop("disabled", false);
            $("#Rel03").trigger("focus");

        }
        else {
            $("#Rel03").prop("disabled", true).val("");
            $("#Rel03List").prop("disabled", true).empty();
        }
    });

    $("#chkRel04").on("click", function () {
        if ($("#chkRel04").is(":checked")) {
            $("#Rel04").prop("disabled", false);
            $("#Rel04List").prop("disabled", false);
            $("#Rel04").trigger("focus");

        }
        else {
            $("#Rel04").prop("disabled", true).val("");
            $("#Rel04List").prop("disabled", true).empty();
        }
    });

    $("#chkRel05").on("click", function () {
        if ($("#chkRel05").is(":checked")) {
            $("#Rel05").prop("disabled", false);
            $("#Rel05List").prop("disabled", false);
            $("#Rel05").trigger("focus");

        }
        else {
            $("#Rel05").prop("disabled", true).val("");
            $("#Rel05List").prop("disabled", true).empty();
        }
    });

    //check generico chkDescr activando componentes disables
    $(document).on("click", "input#chkDescr", function () {
        if ($(this).is(":checked")) {
            $("#Buscar").prop("disabled", false);
            $("#Buscar").trigger("focus");
        }
        else {
            $("#Buscar").val("").prop("disabled", true);
        }
    });

    //check generico chkDescr activando componentes disables
    $(document).on("click", "#chkDesdeHasta", function () {
        if ($(this).is(":checked")) {
            $("#Id").prop("disabled", false);
            $("#Id2").prop("disabled", false);
            $("#Id").trigger("focus");

        }
        else {
            $("#Id").val("").prop("disabled", true);
            $("#Id2").val("").prop("disabled", true);
        }
    });


    $("#UserPerfilId").on("change", cambiaMenuApp);
});

//const AbmAction = {
//    ALTA: 'A',
//    BAJA: 'B',
//    MODIFICACION: 'M',
//    SUBMIT: 'S',
//    CANCEL: 'C'
//}

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
        /*contentType: "application/json",*/
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


//function AbrirMensaje(Titulo, Mensaje, CallBack, EsConfirmacion, Botones, Tipo, CallBackExportar) {
//    if (EsConfirmacion) {
//        if (Botones.length > 2) {
//            $("#btnMensajeAceptar").show();
//            $("#btnMensajeAlternativa").show();
//            $("#btnMensajeCancelar").show();
//        }
//        else {
//            $("#btnMensajeAceptar").show();
//            $("#btnMensajeAlternativa").hide();
//            $("#btnMensajeCancelar").show();
//        }

//    } else {
//        $("#btnMensajeAceptar").show();
//        $("#btnMensajeAlternativa").hide();
//        $("#btnMensajeCancelar").hide();
//    }
//    if (Mensaje != null) {
//        $('#msjContenido').html(Mensaje);
//    } else {
//        $('#msjContenido').html('Error inesperado, intente de nuevo en unos minutos...');
//    }
//    if (Titulo != null) {
//        $('#msjTitulo').text(Titulo);
//    } else {
//        $('#msjTitulo').text('¡Atención!');
//    }
//    FunctionCallback = CallBack;
//    if (Botones != null) {
//        if (Botones.length == 1) {
//            $("#btnMensajeAceptar").text(Botones[0]);
//        }
//        if (Botones.length == 2) {
//            $("#btnMensajeAceptar").text(Botones[0]);
//            $("#btnMensajeCancelar").text(Botones[1]);
//        }
//        else {
//            $("#btnMensajeAceptar").text(Botones[0]);
//            $("#btnMensajeAlternativa").text(Botones[1]);
//            $("#btnMensajeCancelar").text(Botones[2]);
//        }
//        if (Botones.length == 0) {
//            $("#btnMensajeCancelar").text("Cancelar");
//        }
//    } else {
//        $("#btnMensajeAceptar").text("Aceptar");
//        $("#btnMensajeCancelar").text("Cancelar");
//    }
//    //$('#msjModal').fadeIn(0);
//    $("#msjIcono").html("");
//    switch (Tipo) {
//        case "info!":
//            $("#msjTitulo").prop("class", "text-info");
//            $("#msjIcono").html('<i class="bx bx-md bx-spin bx-info-circle text-info"></i>');
//            break;
//        case "warn!":
//            $("#msjTitulo").prop("class", "text-warning");
//            $("#msjIcono").html('<i class="bx bx-md bx-spin bx-error text-warning"></i>');
//            break;
//        case "error!":
//            $("#msjTitulo").prop("class", "text-danger");
//            $("#msjIcono").html('<i class="bx bx-md bx-spin bx-hand text-danger"></i>');
//            break;
//        case "succ!":
//            $("#msjTitulo").prop("class", "text-success");
//            $("#msjIcono").html('<i class="bx bx-md bx-spin bx-check text-success"></i>');
//            break;
//        default:
//            $("#msjIcono").prop("class", "");
//            $("#msjIcono").html('');
//            break;
//    }
//    $("#btnMensajeExportar").hide();
//    if (CallBackExportar != null) {
//        FunctionCallBackExportar = CallBackExportar;
//        $("#btnMensajeExportar").show();
//        $("#btnMensajeAceptar").hide();
//        $("#btnMensajeCancelar").show();
//    }

//    $('#msjModal').modal('show');
//}

//codigo generico para autocomplete 01
$("#Rel01").autocomplete({
    source: function (request, response) {

        data = { prefix: request.term }; Rel01

        $.ajax({
            url: autoComRel01Url,
            type: "POST",
            dataType: "json",
            data: data,
            success: function (obj) {
                response($.map(obj, function (item) {
                    var texto = item.descripcion;
                    return { label: texto, value: item.descripcion, id: item.id, prov: item.provId };
                }));
            }
        })
    },
    minLength: 3,
    select: function (event, ui) {
        if ($("#Rel01List").has('option:contains("' + ui.item.id + '")').length === 0) {
            $("#Rel01Item").val(ui.item.id);
            var opc = "<option value=" + ui.item.id + ">" + ui.item.value + "</option>"
            $("#Rel01List").append(opc);

            //$("#Rel01").prop("disabled", true);
            //$("#Rel01List").prop("disabled", true);

        }
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
        if ($("#Rel02List").has('option:contains("' + ui.item.id + '")').length === 0) {
            $("#Rel02Item").val(ui.item.id);
            var opc = "<option value=" + ui.item.id + ">" + ui.item.value + "</option>"
            $("#Rel02List").append(opc);
        }
        return true;
    }
});

//exclusivo para Proveedor en EDIT PRODUCTO
$("input#Cta_Lista").autocomplete({
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
        $("#Cta_Id").val(ui.item.id);
        //var opc = "<option value=" + ui.item.id + ">" + ui.item.value + "</option>"
        //$("#Rel01List").append(opc);
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


function selectReg(x, gridId) {
    $("#" + gridId + " tbody tr").each(function (index) {
        $(this).removeClass("selected-row");
        $(this).removeClass("selectedEdit-row");
    });
    $(x).addClass("selected-row");

}

function desactivarGrilla(gridId) {
    $("#" + gridId + "").addClass("disable-table-rows");
    $(".table-wrapper").css("overflow", "hidden");
}

function activarGrilla(gridId) {
    $("#" + gridId + "").removeClass("disable-table-rows");
    $(".table-wrapper").css("overflow", "auto");

}


//mueve registro al top de la grilla
function posicionarRegOnTop(x) {

    var $registro = $(x); // El registro seleccionado
    var $contenedor = $(".table-wrapper"); // El contenedor desplazable
    var $header = $contenedor.find("thead"); // El header fijo de la tabla

    // Calcular la posición del registro relativo al contenedor
    var registroOffset = $registro.offset().top; // Posición del registro en el documento
    var contenedorOffset = $contenedor.offset().top; // Posición del contenedor en el documento
    var scrollActual = $contenedor.scrollTop(); // Posición actual del scroll del contenedor

    // Obtener la altura del header
    var headerHeight = $header.outerHeight() || 0; // Si no hay header, usar 0

    // Calcular el nuevo scroll para que el registro quede en la parte superior
    var nuevoScroll = scrollActual + (registroOffset - contenedorOffset) - headerHeight;

    // Animar el scroll del contenedor para posicionar el registro en la parte superior
    $contenedor.animate({
        scrollTop: nuevoScroll
    }, 500);

    //rowOffset = 0;
    //posActScrollTop = 0;
    //newPosScrollTop = 0

    //posTabla = $(".table-wrapper");
    ////calculamos la posicion del offset del registro seleccionado
    //rowOffset = x.position().top;
    ////posición actual del scroll
    //posActScrollTop = posTabla.scrollTop();
    ////calculamos la nueva posición del scroll
    //newPosScrollTop = rowOffset + posActScrollTop - posTabla.position().top;
    //posTabla.animate({
    //    scrollTop: newPosScrollTop
    //}, 500);
}

function cambiaMenuApp() {
    var perf = $("#UserPerfilId option:selected").val();
    var data = { perId: perf };

    PostGen(data, cambiaMenu, function (obj) {
        if (obj.error === true) {
            CerrarWaiting();
            AbrirMensaje("ALGO NO SALIO BIEN!", obj.msg, function () {
                $("#msjModal").modal("hide");
            }, false, ["CONTINUAR"], "error!", null);
        }
        else if (obj.warn === true) {
            CerrarWaiting();

            AbrirMensaje("ATENCIÓN", obj.msg, function () {
                if (obj.auth === true) {
                    window.location.href = login;
                }
                else {
                    $("#msjModal").modal("hide");
                }
            }, false, ["CONTINUAR"], "warn!", null);

        }
        else {
            CerrarWaiting();
            AbrirMensaje("ATENCIÓN", obj.msg, function () {
                window.location.href = home;

            }, false, ["CONTINUAR"], "succ!", null);
        }
    });

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

function hayRegistrosEnTabla(grid) {
    if ($(grid).length) {
        var tb = $(grid + " tbody tr");
        if (tb.length === 0) {
            return false;
        } else {
            return true;
        }
    }
    else {
        return false;
    }
}

/**
 * Carga los datos de un reporte en la posición correspondiente del arreglo.
 * @param {number} numeroReporte - El índice (número de reporte).
 * @param {Object} parametros - Objeto con los parámetros clave-valor del reporte.
 * @param {string} titulo - Título del reporte.
 * @param {string} observacion - Observación del reporte.
 * @param {number} admId - ID de la Sucursal.
 */
function cargarReporteEnArre(numeroReporte, parametros,titulo,observacion,admId) {
    if (numeroReporte - 1 < 0 || numeroReporte - 1 >= arrRepoParams.length) {
        let msg = "El número de reporte está fuera de rango (0-" + arrRepoParams.length+"). Verifique la identificación del Reporte. El mismo no se ha resguardado. ";
        ControlaMensajeWarning(msg);
        console.error("Número de reporte fuera de rango (0-299).");
        return;
    }

    arrRepoParams[numeroReporte -1] = {
        reporte: numeroReporte,
        parametros: parametros,
        titulo: titulo,
        observacion: observacion,
        administracion: admId,
        logoPath: "",
        formato: ""
    };
}

/**
 * Activa o desactiva un componente (input, select, etc.) según el estado de un checkbox.
 * Si el checkbox está marcado, habilita el componente y restaura su estilo visual.
 * Si el checkbox está desmarcado, deshabilita el componente y aplica un estilo de fondo y negrita.
 * @param {string} checkboxId - El ID del checkbox que controla el estado.
 * @param {string} componentSelector - Selector jQuery del componente a activar/desactivar.
 */
function toggleComponent(checkboxId, componentSelector) {
    try {
        const isChecked = $(`#${checkboxId}`).is(':checked');
        const $component = $(componentSelector);

        if (isChecked) {
            $component.prop('disabled', false).css({
                'background-color': '',
                'font-weight': 'normal'
            });
        } else {
            $component.prop('disabled', true).css({
                'background-color': 'rgb(251,255,195)',
                'font-weight': '900'
            });
        }
    } catch (error) {
        console.error(`Error al procesar el checkbox ${checkboxId}:`, error);
    }
}