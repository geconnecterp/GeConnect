$(function () {
    $("#btnContinua01").on("click", presentarDetalleUL);
    CargarRPRxUL();
});

function CargarRPRxUL() {

    AbrirWaiting("Espere, se buscan las ULs de la RTI...")
    var datos = {};
    PostGenHtml(datos, ObtenerDetalleUlsUrl, function (obj) {
        $("#gridDetalleULs").html(obj);
        CerrarWaiting();
        return true;
    });
    return true;
}

function seleccionaRegistroUL(x) {
    $("#tbRTRxUL tbody tr").each(function (index) {
        $(this).removeClass("selected-row");
    });
    $(x).addClass("selected-row");
    var reg = parseInt(x.cells[0].innerText.trim());
    ulSeleccionada = $("#ul" + reg).val();

    $("#btnContinua01").prop("disabled", false);
    return true;
}

function presentarDetalleUL() {
    if (typeof ulSeleccionada !== 'undefined') {

        window.location.href = CargarProductosUrl + "?ul=" + ulSeleccionada;
    }
    return true;
}