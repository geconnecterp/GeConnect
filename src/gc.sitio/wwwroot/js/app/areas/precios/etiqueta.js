$(function () {
    InicializaPantallaEtiqueta();
});

function InicializaPantallaEtiqueta() {
    // INICIALIZAMOS PANELES
    if ($("#divDetalle").is(":visible")) {
        $("#divDetalle").collapse("hide");
    }
    $("#divFiltro").collapse("show");
}