﻿$(function () {

    $("#rbBox").on("click", function () { alert("Radio Button Box") });
    $("#rbProd").on("click", function () { alert("Radio Button Prod") });
    $("#rbRubro").on("click", function () { alert("Radio Button Rubro") });

    AbrirWaiting();
    presentaListaProducto();
});

function presentaListaProducto() {
    datos = {};
    PostGenHtml(datos, buscarListaProductosUrl, function (obj) {

        $("#divti03").html(obj);

        CerrarWaiting();
    });
}