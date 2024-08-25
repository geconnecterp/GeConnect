$(document).ready(function () {
    //cargo el js para llamar desde esta funcion a la función de busqueda
    //verifico si se hace click en el boton buscar
    $("#btnBusquedaBase").on("click", function () {
        InicializaPantalla();
        buscarProducto();
        return true;
    });
    //input del control. Sirve para permitir inicializar pantalla.
    $("input#Busqueda").on("focus", function () {
        InicializaPantalla();
    });

    $("#estadoFuncion").on("change", verificaEstado); //este control debe ser insertado el mismo o similar para cada modulo.
    $("#CantProdRPR").on("change", function () {
        //aca se puede saber si tiene o no productos cargados
        var cant = $("#CantProdRPR").val();
        if (parseInt(cant) > 0) {
            $("#btnContinuarRpr").show("slow");
        }
        else {
            $("#btnContinuarRpr").hide("fast");
        }
    }); //este control sirve para verificar si hay registros o no y asi presentar o no el boton de avanzar
    

    $(".inputEditable").on("keypress", analizaEnterInput);
    $("#btnCargarProd").on("click", cargarProductos);
    $("#tbProdRPR").on("click", ".btnDelete", EliminarProducto)


    InicializaPantalla();
});

