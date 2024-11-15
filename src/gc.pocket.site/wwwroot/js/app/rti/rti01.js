$(function () {

    $("#btnrtiConf01").on("click", function () {
        if (typeof rtrModifica !== 'undefined') {
            if (rtrModifica === true) {
                window.location.href = CargaProductosUrl + "?rm=" + remitoSeleccionado + "&esUpdate=true";
            }
        } else {
            window.location.href = CargaProductosUrl + "?rm=" + remitoSeleccionado;
        }
    });

    AbrirWaiting();
    CargarndoRemitosPendientes();
})

function CargarndoRemitosPendientes() {
    var datos = {};
    PostGenHtml(datos, ObtenerRemitosPendientesURL, function (obj) {
        CerrarWaiting();
        $("#divGrRemitos").html(obj);
        return true;
    });
}


function seleccionaRegistroRP(x) {
    $("#tbRemPend tbody tr").each(function (index) {
        $(this).removeClass("selected-row");
    });
    $(x).addClass("selected-row");
    var reg = parseInt(x.cells[0].innerText.trim());
    remitoSeleccionado = x.cells[1].innerText.trim();

    $("#btnrtiConf01").prop("disabled", false);
}