$(function () {
    //check generico REL01 activando componentes disables
    $("#chkRel01").on("click", function () {
        if ($("#chkRel01").is(":checked")) {
            $("#Rel01").prop("disabled", false);
            $("#Rel01List").prop("disabled", false);
        }
        else {
            $("#Rel01").prop("disabled", true);
            $("#Rel01List").prop("disabled", true);
        }
    });

    //check generico REL02 activando componentes disables
    $("#chkRel02").on("click", function () {
        if ($("#chkRel02").is(":checked")) {
            $("#Rel02").prop("disabled", false);
            $("#Rel02List").prop("disabled", false);
        }
        else {
            $("#Rel02").prop("disabled", true);
            $("#Rel02List").prop("disabled", true);
        }
    });

});
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
    switch (Tipo) {
        case "info!":
            $("#msjTitulo").prop("class", "text-info");
            $("#msjIcono").html('<i class="bx bx-md bx-spin bx-info-circle text-info"></i>');
            break;
        case "warn!":
            $("#msjTitulo").prop("class", "text-warning");
            $("#msjIcono").html('<i class="bx bx-md bx-spin bx-error text-warning"></i>');
            break;
        case "error!":
            $("#msjTitulo").prop("class", "text-danger");
            $("#msjIcono").html('<i class="bx bx-md bx-spin bx-hand text-danger"></i>');
            break;
        case "succ!":
            $("#msjTitulo").prop("class", "text-success");
            $("#msjIcono").html('<i class="bx bx-md bx-spin bx-check text-success"></i>');
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