$(document).ready(function () {
    //cargo el js para llamar desde esta funcion a la función de busqueda
    //verifico si se hace click en el boton buscar
    $("#btnBusquedaBase").on("click", function () {
        InicializaPantalla();
        buscarProducto();
        return true;
    });

    $("#estadoFuncion").on("change", verificaEstado); //este control debe ser insertado el mismo o similar para cada modulo.

    $(".inputEditable").on("keypress", analizaEnterInput);
    $("input").on("focus", function () { $(this).select(); })
});

function analizaEnterInput(e) {
    if (e.which == "13") {
        //verifico cuantos input habilitados encuentro
        var $nextInput = $(this).nextAll("input:not(:disabled)");
        if ($nextInput.length) {
            $nextInput.first().focus();
            return true;
        }
    }
    return true;
}

function verificaEstado(e) {

    var res = $("#estadoFuncion").val();
    if (res === "true") {

        //traigo la variable productoBase e hidrato componentes
        var prod = productoBase;

        /*$("#P_id").val(prod.p_id);     */
        $("#Descipcion").val(prod.p_desc);
        $("#Rubro").val(prod.rub_desc);
        $("#estadoFuncion").val(false);
        $("#up").val(prod.p_unidad_pres).prop("disabled", false);
        $("#box").val(0).prop("disabled", false);
        $("#unid").val(0).prop("disabled", false);
        if (prod.p_con_vto !== "N") {
            var f = new Date();
            var month = ('0' + (f.getMonth() + 1)).slice(-2); // Asegura que el mes siempre tenga dos dígitos
            var day = ('0' + f.getDate()).slice(-2); // Asegura que el día siempre tenga dos dígitos
            var newfecha = f.getFullYear() + '-' + month + '-' + day;
            $("#fvto").prop("disabled", false).val(newfecha);
        }
        $("#up").focus();

        CerrarWaiting();

    }
    return true;
}

function InicializaPantalla() {
    $("#up").val(0).prop("disabled", false);
    $("#fvto").prop("disabled", true);
    $("#box").val(0).prop("disabled", true);
    $("#unid").val(0).prop("disabled", true);
    return true;
}