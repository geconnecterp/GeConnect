/**
 * Seleccion de Valores
 * 
 * Este módulo se encarga de la seleccion de valores financieros
 */

$(function () {

});

function invocarModalDeSeleccionDeValores() {
    var data = {};
    PostGenHtml(data, abrirComponenteDeSeleccionDeValoresUrl, function (obj) {
        $("#modalSeleccionValores").html(obj);
        $("#modalSeleccionValores").show();
        $("#seleccionDeValoresModal").modal("show");
    });
}

function seleccionarTipoFin(x) {
    seleccionarGrilla(x, 'tbTipoCuentaFin');
    //TODO MARCE: Continuar aca, buscar financiero desde tipo seleccionado
}

function seleccionarFinanciero(x) {
    seleccionarGrilla(x, 'tbFinanciero');
}

function seleccionarGrilla(x, grilla) {
    $("#" + grilla + " tbody tr").each(function (index) {
        $(this).removeClass("selected-row");
        $(this).removeClass("selectedEdit-row");
    });
    $(x).addClass("selected-row");
}