//variables globales que son declaradas al inicio para que tengan alcance a la 
//mayor cantidad de codigo.
var nnControlCta01 = "";
var nnControlCta02 = "";
var nnControlCta03 = "";
var nnControlCta04 = "";
var consCta = "";
var consRrss = "";
var consTipo = "";

consCta2 = "";
consRrss2 = "";
consTipo2 = "";

//arreglo destinado a contener los parametros del los reportes
//Inicialmente se genera con 300 posiciones
var arrRepoParams = new Array(300);

$(function () {

    // Remover tooltips anteriores para evitar duplicados
    $("#golden-tooltip").remove();

    // Crear el elemento tooltip con estilo golden
    $("body").append('<div id="golden-tooltip" class="tooltip-golden"></div>');

    // Añadir estilos específicos si no están ya definidos en el CSS
    if (!$("style#golden-tooltip-styles").length) {
        $("head").append(`
                                                    <style id="golden-tooltip-styles">
                                                        .tooltip-golden {
                                                            position: absolute;
                                                            display: none;
                                                            background: linear-gradient(135deg, #b8860b 0%, #daa520 100%);
                                                            color: #333;
                                                            text-shadow: 0 1px 1px rgba(255, 255, 255, 0.3);
                                                            padding: 0.5rem 1rem;
                                                            border-radius: 0.25rem;
                                                            font-size: 0.875rem;
                                                            font-weight: 600;
                                                            white-space: nowrap;
                                                            max-width: 80vw;
                                                            overflow: hidden;
                                                            text-overflow: ellipsis;
                                                            z-index: 9999;
                                                            pointer-events: none;
                                                            box-shadow: 0 2px 8px rgba(0, 0, 0, 0.15);
                                                            border: 1px solid #f5e7c1;
                                                        }
                
                                                        .tooltip-golden::after {
                                                            content: '';
                                                            position: absolute;
                                                            top: 100%;
                                                            left: 50%;
                                                            margin-left: -8px;
                                                            width: 0;
                                                            height: 0;
                                                            border-left: 8px solid transparent;
                                                            border-right: 8px solid transparent;
                                                            border-top: 8px solid #daa520;
                                                        }
                                                    </style>
                                                `);
    }

    // Variable para almacenar la instancia de Popper
    let popperInstance = null;

    // Manejadores de eventos para filas con atributo title
    $(document).on({
        mouseenter: function () {
            const title = $(this).attr('title');
            if (!title) return;

            // Guardar y eliminar el atributo title para evitar el tooltip nativo
            $(this).data('original-title', title);
            $(this).attr('title', '');

            // Mostrar nuestro tooltip personalizado con el contenido
            const tooltip = $("#golden-tooltip");
            tooltip.text(title).show();

            // Crear instancia de Popper para posicionar el tooltip
            popperInstance = Popper.createPopper(this, tooltip[0], {
                placement: 'top',
                modifiers: [
                    {
                        name: 'offset',
                        options: {
                            offset: [0, 8],
                        },
                    },
                    {
                        name: 'preventOverflow',
                        options: {
                            boundary: document.body,
                            padding: 10
                        }
                    },
                    {
                        name: 'flip',
                        options: {
                            fallbackPlacements: ['bottom', 'right', 'left'],
                            padding: 10
                        }
                    }
                ]
            });
        },
        mouseleave: function () {
            // Restaurar el atributo title original
            const originalTitle = $(this).data('original-title');
            if (originalTitle) {
                $(this).attr('title', originalTitle);
            }

            // Ocultar el tooltip
            $("#golden-tooltip").hide();

            // Destruir instancia de Popper para liberar recursos
            if (popperInstance) {
                popperInstance.destroy();
                popperInstance = null;
            }
        }
    }, 'tr[title]');

    // Desactivar el handler anterior
    $(document).off('mousemove.tooltip');

    /** FIN CODIGO TOOLTIP */

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

    // ========================================
    // GESTIÓN DEL MODAL DE MENSAJES
    // ========================================
    const modal = $("#msjModal");
    const btnAceptar = $("#btnMensajeAceptar");
    let elementoAnteriorConFoco = null;

    // Gestionar el foco y eliminar aria-hidden al mostrar el modal
    modal.on("show.bs.modal", function () {
        // Guardar elemento que tenía el foco antes de abrir el modal
        elementoAnteriorConFoco = document.activeElement;
    });

    // Cuando el modal YA está visible, mover foco al botón Aceptar
    modal.on("shown.bs.modal", function () {
        // Mover foco al botón visible más importante
        const $botonVisible = $("#btnMensajeAceptar:visible, #btnMensajeCancelar:visible").first();
        if ($botonVisible.length > 0) {
            $botonVisible.trigger("focus");
        }
    });

    // Al INICIAR el cierre del modal
    modal.on("hide.bs.modal", function () {
        // ✅ NO manipular aria-hidden aquí (causa el warning)
    });

    // Cuando el modal YA está oculto, restaurar el foco
    modal.on("hidden.bs.modal", function () {
        // Esperar a que Bootstrap limpie completamente el modal
        setTimeout(() => {
            if (elementoAnteriorConFoco && typeof elementoAnteriorConFoco.focus === 'function') {
                try {
                    elementoAnteriorConFoco.focus();
                } catch (e) {
                    console.warn("No se pudo restaurar el foco:", e);
                }
            }
            elementoAnteriorConFoco = null;
        }, 150); // ✅ Delay crucial para evitar conflictos
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

function PostGenHtml(data, path, retorno) {
    PostGen(data, path, retorno, fnError, "HTML");
}
function PostGenHtml(data, path, retorno, fxError) {
    PostGen(data, path, retorno, fxError, "HTML");
}
//function PostGen(data, path, retorno) {
//    PostGen(data, path, retorno, fnError, "json");
//}
function PostGen(data, path, retorno, fxError, datatype) {
    $.ajax({
        "dataType": datatype,
        "url": path,
        "type": "POST",
        "data": data,
        xhrFields: {
            withCredentials: true
        },
        /*contentType: "application/json",*/
        "success": retorno,
        //beforeSend: function () { Bloquear();},
        error: fxError
    });
}

/**
 * Realiza una solicitud POST al servidor con datos JSON o FormData.
 * @param {Object|string|FormData} data - Datos a enviar (objeto JS, string JSON o FormData).
 * @param {string} url - URL del endpoint.
 * @param {Function} success - Callback para respuesta exitosa.
 * @param {Function} error - Callback para error (opcional).
 */
function PostGen2(data, url, success, error) {
    let dataToSend;
    let contentType;
    let processData = true;

    // Si es FormData (para archivos)
    if (typeof FormData !== "undefined" && data instanceof FormData) {
        dataToSend = data;
        contentType = false; // Deja que el navegador lo maneje
        processData = false;
    }
    // Si es string y parece JSON
    else if (typeof data === "string" &&
        ((data.trim().startsWith('{') && data.trim().endsWith('}')) ||
            (data.trim().startsWith('[') && data.trim().endsWith(']')))) {
        dataToSend = data;
        contentType = "application/json";
    }
    // Si es objeto JS normal
    else if (typeof data === "object" && data !== null) {
        dataToSend = JSON.stringify(data);
        contentType = "application/json";
    }
    // Cualquier otro tipo (fallback)
    else {
        dataToSend = data;
        contentType = "application/x-www-form-urlencoded; charset=UTF-8";
    }

    $.ajax({
        url: url,
        type: "POST",
        data: dataToSend,
        contentType: contentType,
        processData: processData,
        success: function (response) {
            if (typeof success === 'function') {
                success(response);
            }
        },
        error: function (xhr, status, errorThrown) {
            if (window.console) {
                console.error("Error en solicitud AJAX:", {
                    url: url,
                    status: status,
                    error: errorThrown,
                    response: xhr.responseText
                });
            }
            if (typeof error === 'function') {
                error({
                    status: xhr.status,
                    statusText: xhr.statusText,
                    message: errorThrown || "Error en la solicitud",
                    responseText: xhr.responseText
                });
            }
        }
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


//debo mandar true siempre y cuando
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
        if (Botones.length === 1) {
            $("#btnMensajeAceptar").text(Botones[0]);
        } else {
            if (Botones.length === 2) {
                $("#btnMensajeAceptar").text(Botones[0]);
                $("#btnMensajeCancelar").text(Botones[1]);
            }
            else {
                $("#btnMensajeAceptar").text(Botones[0]);
                $("#btnMensajeAlternativa").text(Botones[1]);
                $("#btnMensajeCancelar").text(Botones[2]);
            }
        }
        if (Botones.length === 0) {
            $("#btnMensajeCancelar").text("Cancelar");
        }
    } else {
        $("#btnMensajeAceptar").text("Aceptar");
        $("#btnMensajeCancelar").text("Cancelar");
    }

    $("#msjIcono").html("");
    $("#msjHeader").removeClass("info warn error success");

    switch (Tipo) {
        case "info!":
            $("#msjTitulo").prop("class", "text-info");
            $("#msjIcono").html('<i class="bx bx-md bx-spin bx-info-circle text-info"></i>');
            $("#msjHeader").addClass("info");
            break;
        case "warn!":
            $("#msjTitulo").prop("class", "text-warning");
            $("#msjIcono").html('<i class="bx bx-md bx-spin bx-error text-warning"></i>');
            $("#msjHeader").addClass("warn");
            break;
        case "error!":
            $("#msjTitulo").prop("class", "text-danger");
            $("#msjIcono").html('<i class="bx bx-md bx-spin bx-hand text-danger"></i>');
            $("#msjHeader").addClass("error");
            break;
        case "succ!":
            $("#msjTitulo").prop("class", "text-success");
            $("#msjIcono").html('<i class="bx bx-md bx-spin bx-check text-success"></i>');
            $("#msjHeader").addClass("success");
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

    // ✅ MOSTRAR EL MODAL
    $('#msjModal').modal('show');
}

//codigo generico para autocomplete 01
$("#Rel01").autocomplete({
    source: function (request, response) {
        data = { prefix: request.term };

        $.ajax({
            url: autoComRel01Url,
            type: "POST",
            dataType: "json",
            data: data,
            success: function (obj) {
                response($.map(obj, function (item) {
                    var texto = item.descripcion;
                    return { label: texto, value: item.descripcion, id: item.id, prov: item.provId, tipo: "P" };
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
            $("#Rel01List").trigger("change");
            consCta = ui.item.id;
            consRrss = ui.item.label;
            consTipo = ui.item.tipo;
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
        var inputss = $("main :input:not(:disabled)");
        tope = inputss.length;
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
    const $grid = $("#" + gridId);
    $grid.addClass("disable-table-rows");
    $grid.closest(".table-wrapper").css("overflow", "hidden");
}

function activarGrilla(gridId) {
    const $grid = $("#" + gridId);
    $grid.removeClass("disable-table-rows");
    $grid.closest(".table-wrapper").css("overflow", "auto");
}

function desactivaGrillav2(gridId, esPadre = true) {
    const $grid = $(gridId);
    $grid.addClass("disable-table-rows");
    if (esPadre) {
        $grid.closest(".table-wrapper").css("overflow", "hidden");
    }
    else {
        $grid.find(".table-wrapper").css("overflow", "hidden");
    }
}

function activaGrillav2(gridId, esPadre = true) {
    const $grid = $(gridId);
    $grid.removeClass("disable-table-rows");
    if (esPadre) {
        $grid.closest(".table-wrapper").css("overflow", "auto");
    }
    else {
        $grid.find(".table-wrapper").css("overflow", "auto");
    }
}

function desactivarGrillav2(gridIdentifier, wrapperClass = 'table-wrapper') {
    const $grid = resolverGrilla(gridIdentifier);

    if ($grid.length === 0) {
        console.warn(`⚠️ No se encontró la grilla: ${gridIdentifier}`);
        return false;
    }

    $grid.addClass("grid-disabled");

    const $wrapper = $grid.closest(`.${wrapperClass}`);
    if ($wrapper.length > 0) {
        $wrapper.css("overflow", "hidden");
    }

    return true;
}

function activarGrillav2(gridIdentifier, wrapperClass = 'table-wrapper') {
    const $grid = resolverGrilla(gridIdentifier);

    if ($grid.length === 0) {
        console.warn(`⚠️ No se encontró la grilla: ${gridIdentifier}`);
        return false;
    }

    $grid.removeClass("grid-disabled");

    const $wrapper = $grid.closest(`.${wrapperClass}`);
    if ($wrapper.length > 0) {
        $wrapper.css("overflow", "auto");
    }

    return true;
}

function resolverGrilla(identifier) {
    if (identifier instanceof jQuery) {
        return identifier;
    }

    if (identifier instanceof HTMLElement) {
        return $(identifier);
    }

    if (typeof identifier === 'string') {
        identifier = identifier.trim();

        if (identifier.length === 0) {
            return $();
        }

        if (/^[#.\[:]/.test(identifier)) {
            return $(identifier);
        }

        const $byId = $(`#${identifier}`);
        if ($byId.length > 0) {
            return $byId;
        }

        const $byClass = $(`.${identifier}`);
        if ($byClass.length > 0) {
            return $byClass;
        }

        const $byDataAttr = $(`[data-grid="${identifier}"]`);
        if ($byDataAttr.length > 0) {
            return $byDataAttr;
        }

        return $(identifier);
    }

    return $();
}

function desactivarGrillav1(gridId) {
    const $grid = resolverGrilla(gridId);

    if ($grid.length === 0) {
        console.warn(`⚠️ No se encontró la grilla: ${gridId}`);
        return false;
    }

    $grid.addClass("disable-table-rows");
    $grid.closest(".table-wrapper").css("overflow", "hidden");
    return true;
}

function activarGrillav1(gridId) {
    const $grid = resolverGrilla(gridId);

    if ($grid.length === 0) {
        console.warn(`⚠️ No se encontró la grilla: ${gridId}`);
        return false;
    }

    $grid.removeClass("disable-table-rows");
    $grid.closest(".table-wrapper").css("overflow", "auto");
    return true;
}

function posicionarRegOnTop(x, classWrapper = "") {
    if (classWrapper.trim() == "") {
        classWrapper = ".table-wrapper";
    }

    var $registro = $(x);
    var $contenedor = $(classWrapper);
    var $header = $contenedor.find("thead");

    var registroOffset = $registro.offset().top;
    var contenedorOffset = $contenedor.offset().top;
    var scrollActual = $contenedor.scrollTop();

    var headerHeight = $header.outerHeight() || 0;

    var nuevoScroll = scrollActual + (registroOffset - contenedorOffset) - headerHeight;

    $contenedor.animate({
        scrollTop: nuevoScroll
    }, 500);
}

function posicionarRegOnTopMejorado($registro, classWrapper = ".table-wrapper") {
    if (!$registro || $registro.length === 0) {
        console.warn("⚠️ posicionarRegOnTopMejorado: registro inválido");
        return;
    }

    const $contenedor = $($registro).closest(classWrapper);

    if ($contenedor.length === 0) {
        console.warn("⚠️ posicionarRegOnTopMejorado: contenedor no encontrado");
        return;
    }

    $contenedor.css('scroll-behavior', 'auto');

    const $header = $contenedor.find("thead");
    const headerHeight = $header.outerHeight() || 0;

    const registroOffset = $registro.position().top;
    const scrollActual = $contenedor.scrollTop();

    const offsetAdicional = 5;
    const nuevoScroll = scrollActual + registroOffset - headerHeight - offsetAdicional;

    $contenedor.scrollTop(nuevoScroll);

    setTimeout(function () {
        $contenedor.css('scroll-behavior', '');
    }, 100);
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
    var month = ('0' + (f.getMonth() + 1)).slice(-2);
    var day = ('0' + f.getDate()).slice(-2);
    return f.getFullYear() + '-' + month + '-' + day;
}

function formatoFecha_ddMMyyyy(pFecha) {
    var f = new Date(pFecha);
    var month = ('0' + (f.getMonth() + 1)).slice(-2);
    var day = ('0' + f.getDate()).slice(-2);
    return day + "/" + month + "/" + f.getFullYear();
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

function cargarReporteEnArre(numeroReporte, parametros, titulo, observacion, admId) {
    if (numeroReporte - 1 < 0 || numeroReporte - 1 >= arrRepoParams.length) {
        let msg = "El número de reporte está fuera de rango (0-" + arrRepoParams.length + "). Verifique la identificación del Reporte. El mismo no se ha resguardado. ";
        ControlaMensajeWarning(msg);
        console.error("Número de reporte fuera de rango (0-299).");
        return;
    }

    arrRepoParams[numeroReporte - 1] = {
        reporte: numeroReporte,
        parametros: parametros,
        titulo: titulo,
        observacion: observacion,
        administracion: admId,
        logoPath: "",
        formato: ""
    };
}

function ReporteResetCeldaEnArre(numeroReporte) {
    if (numeroReporte - 1 < 0 || numeroReporte - 1 >= arrRepoParams.length) {
        let msg = "El número de reporte está fuera de rango (0-" + arrRepoParams.length + "). Verifique la identificación del Reporte. El mismo no se ha reseteado. ";
        ControlaMensajeWarning(msg);
        console.error("Número de reporte fuera de rango (0-299).");
        return;
    }

    arrRepoParams[numeroReporte - 1] = undefined;
}

function ReporteResetArre() {
    arrRepoParams = new Array(300);
}

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

function b64toBlob(b64Data, contentType, sliceSize) {
    contentType = contentType || "";
    sliceSize = sliceSize || 512;

    const byteCharacters = atob(b64Data);
    const byteArrays = [];

    for (let offset = 0; offset < byteCharacters.length; offset += sliceSize) {
        const slice = byteCharacters.slice(offset, offset + sliceSize);

        const byteNumbers = new Array(slice.length);
        for (let i = 0; i < slice.length; i++) {
            byteNumbers[i] = slice.charCodeAt(i);
        }

        const byteArray = new Uint8Array(byteNumbers);
        byteArrays.push(byteArray);
    }

    return new Blob(byteArrays, { type: contentType });
}

function validarRangoFechas() {
    $("#fechaError").remove();

    const fechaDesde = $("input[name='Desde']").val();
    const fechaHasta = $("input[name='Hasta']").val();

    if (fechaDesde && fechaHasta) {
        const desde = parseFechaES(fechaDesde);
        const hasta = parseFechaES(fechaHasta);

        if (desde && hasta && desde > hasta) {
            $("input[name='Hasta']").parent().after(
                `<div id="fechaError" class="text-danger small mt-1">
                                                            <i class="bx bx-error-circle"></i> 
                                                            La fecha Desde no puede ser mayor a la fecha Hasta
                                                        </div>`
            );

            $("input[name='Desde'], input[name='Hasta']").addClass("is-invalid");
        } else {
            $("input[name='Desde'], input[name='Hasta']").removeClass("is-invalid");
        }
    }
}

function validarRangoFechasC() {
    $("#fechaError").remove();

    const fechaDesde = $("input[name='DesdeFC']").val();
    const fechaHasta = $("input[name='HastaFC']").val();

    if (fechaDesde && fechaHasta) {
        const desde = parseFechaES(fechaDesde);
        const hasta = parseFechaES(fechaHasta);

        if (desde && hasta && desde > hasta) {
            $("input[name='HastaFC']").parent().after(
                `<div id="fechaError" class="text-danger small mt-1">
                                                            <i class="bx bx-error-circle"></i> 
                                                            La fecha Desde no puede ser mayor a la fecha Hasta
                                                        </div>`
            );

            $("input[name='DesdeFC'], input[name='HastaFC']").addClass("is-invalid");
        } else {
            $("input[name='DesdeFC'], input[name='HastaFC']").removeClass("is-invalid");
        }
    }
}

function parseFechaES(fechaStr) {
    if (!fechaStr) return null;

    let fecha;

    if (fechaStr.includes('/')) {
        const partes = fechaStr.split('/');
        if (partes.length !== 3) return null;

        fecha = new Date(parseInt(partes[2]), parseInt(partes[1]) - 1, parseInt(partes[0]));
    } else if (fechaStr.includes('-')) {
        fecha = new Date(fechaStr);
    } else {
        return null;
    }

    return isNaN(fecha.getTime()) ? null : fecha;
}

function extraerValoresDeSelect(selectId, fallbackId, checkId) {
    const valores = [];

    if (!$(checkId).is(":checked")) {
        return valores;
    }

    const $opts = $(selectId).find("option");
    if ($opts.length > 0) {
        const visto = {};
        $opts.each(function () {
            let v = $(this).val();
            if (v != null) {
                v = String(v).trim();
                if (v.length > 0 && !visto[v]) {
                    visto[v] = true;
                    valores.push(v);
                }
            }
        });
    } else if (fallbackId) {
        let unicoVal = $(fallbackId).val();
        if (unicoVal != null) {
            unicoVal = String(unicoVal).trim();
            if (unicoVal.length > 0) {
                valores.push(unicoVal);
            }
        }
    }

    return valores;
}

// ===============================
//  Task Manager Reutilizable
// ===============================

window.TaskManager = window.TaskManager || (function () {

    let pending = 0;

    function start() {
        if (pending === 0) AbrirWaiting();
        pending++;
    }

    function end() {
        pending--;
        if (pending <= 0) {
            pending = 0;
            CerrarWaiting();
        }
    }

    function getPending() {
        return pending;
    }

    return { start, end, getPending };

})();

// ═══════════════════════════════════════════════════════════════════
// SISTEMA DE GESTIÓN DE SESIONES (✅ NUEVO)
// ═══════════════════════════════════════════════════════════════════

/**
 * ✅ NUEVO v1.0: Valida si la respuesta indica sesión expirada
 * 
 * @param {number} statusCode - Código HTTP de respuesta
 * @returns {boolean} - true si es sesión expirada
 */
function esSesionExpirada(statusCode) {
    // 401 = No autorizado
    // 403 = Prohibido
    // 440 = Login Timeout (código personalizado del middleware)
    return statusCode === 401 || statusCode === 403 || statusCode === 440;
}

/**
 * ✅ NUEVO v1.0: Muestra mensaje de sesión expirada y redirige al login
 * 
 * @param {string} mensajePersonalizado - Mensaje opcional
 */
function manejarSesionExpirada(mensajePersonalizado = null) {
    console.error('🚪 Sesión expirada detectada - Redirigiendo al login...');
    
    const mensaje = mensajePersonalizado || 
        'Su sesión ha expirado.<br><br>' +
        '<small class="text-muted"><i class="bx bx-info-circle"></i> Por favor, vuelva a iniciar sesión.</small>';
    
    AbrirMensaje(
        "Sesión Expirada",
        mensaje,
        function () {
            $("#msjModal").modal("hide");
            
            setTimeout(() => {
                // ✅ Usar variable global 'logout' definida en _Layout.cshtml
                if (typeof logout !== 'undefined' && logout) {
                    window.location.href = logout;
                } else {
                    // Fallback si 'logout' no está definida
                    console.error('⚠️ Variable logout no definida, usando URL por defecto');
                    window.location.href = '/Token/Login?area=seguridad';
                }
            }, 500);
        },
        false,
        ["Aceptar"],
        "warn!",
        null
    );
}

/**
 * ✅ NUEVO v1.0: Interceptor global de errores AJAX
 * 
 * Configura jQuery para detectar automáticamente sesiones expiradas
 * en TODAS las llamadas AJAX del sitio
 */
function configurarInterceptorSesiones() {
    // ✅ Interceptor global de errores AJAX
    $(document).ajaxError(function (event, jqXHR, ajaxSettings, thrownError) {
        // Detectar sesiones expiradas (incluye código 440)
        if (esSesionExpirada(jqXHR.status)) {
            console.error('═══════════════════════════════════════════════════');
            console.error('🚨 INTERCEPTOR GLOBAL: Sesión expirada detectada');
            console.error('═══════════════════════════════════════════════════');
            console.error(`   URL: ${ajaxSettings.url}`);
            console.error(`   Status: ${jqXHR.status}`);
            console.error(`   Error: ${thrownError}`);

            // ✅ NUEVO: Mostrar respuesta JSON si existe
            if (jqXHR.responseJSON) {
                console.error('   Respuesta JSON:', jqXHR.responseJSON);
            }

            console.error('═══════════════════════════════════════════════════');

            // Prevenir múltiples redirecciones
            if (!window.sesionExpiradaEnProceso) {
                window.sesionExpiradaEnProceso = true;

                // ✅ NUEVO: Usar mensaje del servidor si está disponible
                let mensajePersonalizado = null;
                if (jqXHR.responseJSON && jqXHR.responseJSON.msg) {
                    mensajePersonalizado = jqXHR.responseJSON.msg;
                }

                manejarSesionExpirada(mensajePersonalizado);
            }
        }
    });

    console.log('✅ Interceptor global de sesiones configurado');
}

/**
 * ✅ NUEVO v1.0: Valida respuesta JSON de endpoints
 * 
 * @param {Object} response - Respuesta del servidor
 * @param {Function} callbackError - Callback para manejar error
 * @returns {boolean} - true si la sesión está activa, false si expiró
 */
function validarRespuestaSesion(response, callbackError = null) {
    // Detectar mensaje de sesión expirada en respuesta JSON
    if (response && !response.ok && response.mensaje) {
        const mensajeLower = response.mensaje.toLowerCase();
        
        if (mensajeLower.includes('sesión expirada') || 
            mensajeLower.includes('sesion expirada') ||
            mensajeLower.includes('session expired') ||
            response.resultado === -1) {
            
            console.warn('⚠️ Sesión expirada detectada en respuesta JSON');
            
            if (callbackError) {
                callbackError(response.mensaje);
            } else {
                manejarSesionExpirada(response.mensaje);
            }
            
            return false;
        }
    }
    
    return true;
}

// ════════════════════════════════════════════════════════════
// HELPERS (Reutilización de funciones)
// ════════════════════════════════════════════════════════════

/**
 * Formatea un número con separadores de miles
 */
function formatearNumero(numero, decimales = 2) {
    if (isNaN(numero)) return '0.00';
    return numero.toLocaleString('es-AR', {
        minimumFractionDigits: decimales,
        maximumFractionDigits: decimales
    });
}

/**
 * Muestra mensaje de error
 */
function mostrarMensajeError(mensaje) {
    console.error('💬 Error:', mensaje);
    AbrirMensaje("Error", mensaje, function () {
        $("#msjModal").modal("hide");
    }, false, ["Aceptar"], "error!", null);
}

/**
 * Muestra mensaje de advertencia
 */
function mostrarMensajeAdvertencia(mensaje) {
    console.warn('💬 Advertencia:', mensaje);
    AbrirMensaje("Advertencia", mensaje, function () {
        $("#msjModal").modal("hide");
    }, false, ["Aceptar"], "warning", null);
}


/**
 * Muestra mensaje de éxito
 */
function mostrarMensajeExito(mensaje) {
    if (typeof window.mostrarMensajeExito === 'function') {
        window.mostrarMensajeExito(mensaje);
    } else {
        console.log('💬 Éxito:', mensaje);
        alert(mensaje);
    }
}

// ════════════════════════════════════════════════════════════
// ✅ NUEVO v10.0: UTILIDADES DE REDONDEO Y FORMATEO
// ════════════════════════════════════════════════════════════

/**
 * ✅ NUEVO v10.0: Redondea un número a una cantidad específica de decimales
 * Soluciona problemas de precisión de punto flotante en JavaScript
 * 
 * @param {number|string} valor - Valor a redondear
 * @param {number} decimales - Cantidad de decimales (default: 2)
 * @returns {number} - Número redondeado
 * 
 * @example
 * redondear(109911.35999999999, 2) → 109911.36
 * redondear(0.1 + 0.2, 2) → 0.30
 * redondear("123.456", 2) → 123.46
 */
function redondear(valor, decimales = 2) {
    // ❶ Validar entrada
    if (valor === null || valor === undefined || valor === '') {
        return 0;
    }

    // ❷ Convertir a número si es string
    let numero = typeof valor === 'number' ? valor : parseFloat(valor);

    // ❸ Validar que sea un número válido
    if (!Number.isFinite(numero)) {
        console.warn(`⚠️ Valor no numérico recibido en redondear(): "${valor}"`);
        return 0;
    }

    // ❹ Validar decimales
    if (!Number.isInteger(decimales) || decimales < 0) {
        console.warn(`⚠️ Decimales inválidos: ${decimales}, usando 2 por defecto`);
        decimales = 2;
    }

    // ❺ REDONDEO ROBUSTO: Usar multiplicación/división para evitar errores de precisión
    const factor = Math.pow(10, decimales);
    const resultado = Math.round(numero * factor) / factor;

    // ❻ Validar resultado
    if (!Number.isFinite(resultado)) {
        console.error(`❌ Error al redondear: ${valor} → ${resultado}`);
        return 0;
    }

    return resultado;
}

/**
 * ✅ ACTUALIZADO v22.0: Formatea número al estilo GeConnect (en-US)
 * CAMBIO CRÍTICO: Reemplazado 'es-AR' por 'en-US'
 * 
 * Formato GeConnect:
 * - Separador de miles: , (coma)
 * - Separador decimal: . (punto)
 * - Ejemplo: 1,234.56
 * 
 * @param {number} numero - Número a formatear
 * @param {number} decimales - Cantidad de decimales (default: 2)
 * @returns {string} - Número formateado (ej: "1,234.56")
 */
function formatearNumero(numero, decimales = 2) {
    if (isNaN(numero)) {
        console.warn(`⚠️ formatearNumero: entrada inválida (${numero})`);
        return '0.00';
    }

    return parseFloat(numero).toLocaleString('en-US', {
        minimumFractionDigits: decimales,
        maximumFractionDigits: decimales
    });
}

/**
 * ✅ NUEVO v10.0: Escapa caracteres HTML para prevenir XSS
 * (Mantener esta función existente si ya estaba)
 */
function escapeHtml(texto) {
    if (!texto) return '';

    const map = {
        '&': '&amp;',
        '<': '&lt;',
        '>': '&gt;',
        '"': '&quot;',
        "'": '&#039;'
    };

    return String(texto).replace(/[&<>"']/g, function (m) { return map[m]; });
}

/**
 * ✅ NUEVO v10.0: Suma array de números con redondeo al final
 * Minimiza acumulación de errores de precisión
 * 
 * @param {Array<number>} valores - Array de números
 * @param {number} decimales - Decimales del resultado final
 * @returns {number} - Suma redondeada
 * 
 * @example
 * sumarConRedondeo([10.1, 20.2, 30.3], 2) → 60.60
 */
function sumarConRedondeo(valores, decimales = 2) {
    if (!Array.isArray(valores) || valores.length === 0) {
        return 0;
    }

    const suma = valores.reduce((acc, val) => acc + (parseFloat(val) || 0), 0);
    return redondear(suma, decimales);
}

// ═══════════════════════════════════════════════════════════════════
// SECCIÓN 0: FUNCIÓN CENTRALIZADA DE MENSAJES (✅ NUEVO v15.1)
// ═══════════════════════════════════════════════════════════════════

/**
 * ✅ ACTUALIZADO v15.1: Muestra mensaje informativo en el área de estado
 * CENTRALIZA todos los mensajes de información/error del módulo
 * NUEVO v15.1: Tamaño de fuente aumentado a 1.5rem para mejor visibilidad
 * 
 * @param {string} mensaje - Mensaje a mostrar
 * @param {string} tipo - Tipo: 'info', 'success', 'warning', 'danger'
 * @param {number} duracion - Duración en ms (0 = permanente, null = 5000ms default)
 */
function mostrarMensajeEstado(mensaje, tipo = 'danger', duracion = 5000) {
    console.log('═══════════════════════════════════════════════════');
    console.log(`📢 MENSAJE DE ESTADO v15.1: ${tipo.toUpperCase()}`);
    console.log(`   Mensaje: ${mensaje}`);
    console.log(`   Duración: ${duracion === 0 ? 'PERMANENTE' : duracion + 'ms'}`);
    console.log('═══════════════════════════════════════════════════');

    // ❶ Mapear iconos según tipo
    const iconos = {
        'info': 'bx-info-circle',
        'success': 'bx-check-circle',
        'warning': 'bx-error-circle',
        'danger': 'bx-error-circle'
    };

    const icono = iconos[tipo] || 'bx-info-circle';

    // ❷ Remover todas las clases de color y aplicar la nueva
    const $mensaje = $('#mensajeEstadoProducto');

    $mensaje
        .removeClass('text-info text-success text-warning text-danger text-muted')
        .addClass(`text-${tipo}`)
        .css('font-size', '1.5rem')  // ✅ NUEVO v15.1: Tamaño de fuente aumentado
        .html(`<i class='bx ${icono}'></i> ${mensaje}`);

    // ❸ Restaurar al estado inicial después del tiempo especificado
    if (duracion > 0) {
        setTimeout(() => {
            $mensaje
                .removeClass('text-info text-success text-warning text-danger')
                .addClass('text-muted')
                .css('font-size', '')  // ✅ NUEVO v15.1: Restaurar tamaño original
                .html('Presione <kbd>Enter</kbd> o <strong>BUSCAR</strong> para agregar producto');
        }, duracion);
    }

    // ❹ CRÍTICO: Devolver foco al input de código
    setTimeout(() => {
        $('#txtCodigoProducto').trigger('focus');
    }, 100);
}

// ---------------------------------------------------------
// FUNCIONES DE MENSAJES Y MANEJO DE ERRORES
// ---------------------------------------------------------

function mostrarLoader(texto) {
    $('#loaderText').html(texto);
    $('#loaderOverlay').fadeIn(500);
}

function ocultarLoader() {
    $('#loaderOverlay').fadeOut(300);
}

// ═══════════════════════════════════════════════════════════════════
// SECCIÓN 0.5: GESTIÓN DEL TECLADO DIGITAL (✅ OPTIMIZADO v18.0)
// ═══════════════════════════════════════════════════════════════════

/**
 * ✅ OPTIMIZADO v18.0: Cierra el teclado virtual específico del proyecto
 * 
 * COMPATIBLE CON:
 * - virtual-keyboard.js (plugin actual de gc.caja)
 * 
 * @returns {boolean} - true si se cerró el teclado, false si no estaba abierto
 */
function cerrarTecladoDigital() {
    console.log('═══════════════════════════════════════════════════');
    console.log('⌨️ CERRANDO TECLADO DIGITAL v18.0 (OPTIMIZADO)');
    console.log('═══════════════════════════════════════════════════');

    let tecladoCerrado = false;

    // ❶ MÉTODO PRINCIPAL: Buscar contenedor por ID
    const $teclado = $('#virtual-keyboard');

    if ($teclado.length > 0) {
        console.log('🔍 Teclado virtual detectado (ID: virtual-keyboard)');

        // ❷ VERIFICAR SI ESTÁ VISIBLE
        const estaVisible = $teclado.css('display') !== 'none';

        console.log(`   Estado actual: ${estaVisible ? 'VISIBLE ✅' : 'OCULTO ❌'}`);

        if (estaVisible) {
            // ❸ OCULTAR USANDO EL MISMO MÉTODO QUE EL PLUGIN
            $teclado.css('display', 'none');
            console.log('✅ Teclado ocultado correctamente');
            tecladoCerrado = true;
        } else {
            console.log('ℹ️ El teclado ya estaba oculto');
        }
    } else {
        console.log('ℹ️ No se detectó el contenedor #virtual-keyboard en el DOM');
    }

    // ❹ MÉTODO SECUNDARIO: Blur del input si está enfocado
    const $input = $('#txtCodigoProducto');

    if ($input.length > 0 && $input.is(':focus')) {
        console.log('🔍 Input enfocado detectado, aplicando blur...');
        $input.trigger('blur');
        tecladoCerrado = true;
    }

    // ❺ RESULTADO FINAL
    if (tecladoCerrado) {
        console.log('═══════════════════════════════════════════════════');
        console.log('✅ TECLADO DIGITAL CERRADO EXITOSAMENTE');
        console.log('═══════════════════════════════════════════════════');
    } else {
        console.log('═══════════════════════════════════════════════════');
        console.log('ℹ️ NO SE DETECTÓ TECLADO DIGITAL ABIERTO');
        console.log('═══════════════════════════════════════════════════');
    }

    return tecladoCerrado;
}

/**
 * ✅ NUEVO v18.1: Cierra el teclado con retraso (debounce)
 * Útil para evitar conflictos con animaciones de cierre
 * 
 * @param {number} delay - Milisegundos de retraso (default: 100)
 * @returns {Promise<boolean>} - Promesa que resuelve cuando se cierra el teclado
 */
function cerrarTecladoDigitalConRetraso(delay = 100) {
    return new Promise((resolve) => {
        setTimeout(() => {
            const resultado = cerrarTecladoDigital();
            resolve(resultado);
        }, delay);
    });
}

// ═══════════════════════════════════════════════════════════════════
// ✅ NUEVO v25.0: FUNCIONES DE CONTROL DEL TECLADO VIRTUAL
// ═══════════════════════════════════════════════════════════════════

/**
 * ✅ NUEVO v25.0: Posiciona el teclado virtual junto al ancla.
 * Se asegura de que el teclado esté visible y alineado a la izquierda.
 */
function posicionarTecladoVirtual(inputSelector, anchorSelector) {
    console.log('📍 Posicionando teclado virtual...');

    const teclado = document.getElementById('virtual-keyboard');

    if (!teclado) {
        console.error('❌ Teclado virtual no encontrado en el DOM.');
        return false;
    }

    const input = inputSelector
        ? document.querySelector(inputSelector)
        : null;

    const modalActual = input
        ? input.closest('.modal.show')
        : null;

    let ancla = null;

    // ❶ Ancla indicada explícitamente por el flujo.
    if (anchorSelector) {
        ancla = document.querySelector(anchorSelector);
    }

    // ❷ Ancla declarada dentro del modal actual.
    if (!ancla && modalActual) {
        ancla = modalActual.querySelector('[data-teclado-ancla]');
    }

    // ❸ Compatibilidad con el flujo genérico de pago.
    if (!ancla) {
        ancla = document.getElementById('teclado-ancla');
    }

    // ❹ Último fallback: usar el propio input como referencia.
    const elementoReferencia = ancla || input;

    if (!elementoReferencia) {
        console.error(
            '❌ No se encontró ancla ni input para posicionar el teclado.'
        );
        return false;
    }

    // Hacer visible el teclado antes de medirlo.
    teclado.style.display = 'flex';
    teclado.style.opacity = '1';
    teclado.style.position = 'fixed';
    teclado.style.transform = 'none';

    const rectReferencia =
        elementoReferencia.getBoundingClientRect();

    const rectTeclado =
        teclado.getBoundingClientRect();

    const anchoTeclado = rectTeclado.width || 360;
    const altoTeclado = rectTeclado.height || 280;

    const margen = 12;

    let top = rectReferencia.bottom + margen;
    let left = rectReferencia.left;

    // Si no entra debajo del campo, se muestra arriba.
    if (top + altoTeclado > window.innerHeight - margen) {
        top = rectReferencia.top - altoTeclado - margen;
    }

    // Evita salir por arriba.
    if (top < margen) {
        top = margen;
    }

    // Evita salir horizontalmente del viewport.
    if (left + anchoTeclado > window.innerWidth - margen) {
        left = window.innerWidth - anchoTeclado - margen;
    }

    if (left < margen) {
        left = margen;
    }

    teclado.style.top = `${Math.round(top)}px`;
    teclado.style.left = `${Math.round(left)}px`;

    // El teclado debe estar por encima del modal activo.
    const zIndexModal = modalActual
        ? parseInt(window.getComputedStyle(modalActual).zIndex, 10)
        : 0;

    teclado.style.zIndex = String(
        Math.max(Number.isFinite(zIndexModal) ? zIndexModal + 10 : 0, 5020)
    );

    console.log(
        `✅ Teclado posicionado: top=${Math.round(top)}px, left=${Math.round(left)}px`
    );

    return true;
}
// function posicionarTecladoVirtual() {
//     console.log('📍 Posicionando teclado virtual...');
//     const ancla = document.getElementById('teclado-ancla');
//     const teclado = document.getElementById('virtual-keyboard');

//     if (!teclado) {
//         console.error('❌ Teclado virtual no encontrado en el DOM.');
//         return;
//     }
//     if (!ancla) {
//         console.error('❌ Ancla #teclado-ancla no encontrada.');
//         return;
//     }

//     // Forzar visibilidad si está oculto
//     if (teclado.style.display !== 'flex') {
//         teclado.style.display = 'flex';
//         teclado.style.opacity = '1';
//         console.log('   ✅ Teclado forzado a ser visible.');
//     }

//     // Calcular posición
//     const rectAncla = ancla.getBoundingClientRect();
//     const rectTeclado = teclado.getBoundingClientRect();

//     // Posicionar el teclado
//     // Usamos 'transform' para no interferir con otras propiedades de posicionamiento
//     const top = rectAncla.top;
//     const left = rectAncla.left;

//     teclado.style.position = 'fixed';
//     teclado.style.top = `${top}px`;
//     teclado.style.left = `${left}px`;
//     teclado.style.transform = 'none'; // Resetear transform de arrastre

//     console.log(`   ✅ Teclado posicionado en: top=${top.toFixed(0)}px, left=${left.toFixed(0)}px`);
// }

/**
 * ✅ NUEVO v25.0: Activa el teclado para un input específico.
 * @param {string} inputSelector - El selector del campo de entrada.
 */
function activarTecladoParaInput(inputSelector, opciones = {}) {
    console.log(`⌨️ Activando teclado para: ${inputSelector}`);

    const input = document.querySelector(inputSelector);

    if (!input) {
        console.error(`❌ Input ${inputSelector} no encontrado.`);
        return false;
    }

    input.focus();

    setTimeout(() => {
        posicionarTecladoVirtual(
            inputSelector,
            opciones.anchorSelector || null
        );

        input.focus();
        input.select();
    }, 150);

    return true;
}

/**
 * ✅ NUEVO v25.0: Oculta el teclado virtual.
 */
function ocultarTecladoVirtual() {
    const teclado = document.getElementById('virtual-keyboard');
    if (teclado) {
        teclado.style.display = 'none';
        console.log('⌨️ Teclado virtual ocultado.');
    }
}