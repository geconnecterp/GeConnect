
function PostGenHtml(data, path, retorno) {
    PostGen(data, path, retorno, fnError, "HTML");
}
function PostGenHtml(data, path, retorno,fxError) {
    PostGen(data, path, retorno, fxError, "HTML");
}
function PostGen(data, path, retorno) {
    PostGen(data, path, retorno, fnError,"json");
}
function PostGen(data, path, retorno, fxError,datatype) {
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

function CerrarWaiting() {
    $('#wWaiting').fadeOut(0);
}