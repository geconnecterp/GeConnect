$(function () {
    $("#rdULsAlm").on("click", limpiaRbULFec);
    $("#rdULFec").on("click", ActivaRbULFec);
    CargaFechaCamposFecUL();
    ConsultarUL();
});

function limpiaRbULFec() {
    if ($("#rdULsAlm").is(":checked")) {
        $("#ulFecD").val("").prop("disabled",true);
        $("#ulFecH").val("").prop("disabled", true);
        ConsultarUL();

    }
}

function ActivaRbULFec() {
    if ($("#rdULFec").is(":checked")) {
        $("#ulFecD").prop("disabled", false);
        $("#ulFecH").prop("disabled", false);
        //CargaFechaCamposFecUL();
        ConsultarUL();

    }
}

function CargaFechaCamposFecUL() {

    var f = new Date();    
    $("#ulFecD").val(formatoFechaYMD(restarFecha(f, 7)));
    $("#ulFecH").val(formatoFechaYMD(f));
}

function ConsultarUL() {
    AbrirWaiting();
    var tipo = "";
    var desde = $("#ulFecD").val();
    var hasta = $("#ulFecH").val();
    if ($("#rdULFec").is(":checked")) {
        tipo = "F";
    }
    else {
        tipo = "A";
    }

    var data = { tipo, desde, hasta };
    PostGenHtml(data, consultaULUrl, function (obj) {
        CerrarWaiting();
        $("#divConsUL").html(obj);
    });
}