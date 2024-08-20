$(document).ready(function () {
    //cargo el js para llamar desde esta funcion a la función de busqueda
    //verifico si se hace click en el boton buscar
    $("#btnBusquedaBase").on("click", function () { buscarProducto(); });
    $("#estadoFuncion").on("change", verificaEstado); //este control debe ser insertado el mismo o similar para cada modulo.
});

function verificaEstado() {

    var res = $("#estadoFuncion").val();
    if (res === "true") {

        //traigo la variable productoBase e hidrato componentes
        var prod = productoBase;
        /*$("#P_id").val(prod.p_id);     */ 
        $("#Descipcion").val(prod.p_desc);      
        $("#Rubro").val(prod.rub_desc);
        $("#up").val(prod.up_)
        $("#estadoFuncion").val(false);

        CerrarWaiting();
    }
    return true;
}