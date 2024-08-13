﻿$(document).ready(function () {

    //cargo el js para llamar desde esta funcion a la función de busqueda
    //verifico si se hace click en el boton buscar
    $("#btnBuscar").on("click", function () { buscarProducto(); }); 
    $("#estadoFuncion").on("change", verificaEstado); //este control debe ser insertado el mismo o similar para cada modulo.

//    $("#btnStkD").click(PresentarStkD);
    $("#btnStkBox").click(PresentarStkBox);
    $("#btnStkA").click(PresentarStkA);
    $("#btnMov").click(PresentarMov);
    $("#btnPr").click(PresentarLP);
});

function verificaEstado() {

    var res = $("#estadoFuncion").val();
    if (res === "true") {

        //traigo la variable productoBase e hidrato componentes
        var prod = productoBase;
        $("#Id").val(prod.p_Id);
        $("#Marca").val(prod.p_m_marca);
        $("#Descipcion").val(prod.p_desc);
        $("#Capacidad").val(prod.p_m_capacidad);
        $("#ProveedorId").val("??????");
        $("#Familia").val("???????");
        $("#Rubro").val(prod.rub_desc);     

        $("#estadoFuncion").val(false);

        infoProdStkD(prod.p_Id);
    }
}

function infoProdStkD(id) {
    var data = { };
    PostGenHtml(data, infoProdStkDUrl, function (obj) {
        $("#gridInfoProdStkD").html(obj);
        CerrarWaiting();
    }, function (obj) {
        ControlaMensajeError(obj.message);
        CerrarWaiting();

    });    
}

function PresentarStkBox() {
    var data = { };
    PostGenHtml(data, infoProdBoxUrl, function (obj) {
        $("#gridInfoProdStkBox").html(obj);
        CerrarWaiting();
    }, function (obj) {
        ControlaMensajeError(obj.message);
        CerrarWaiting();
    });
}
function PresentarStkA() {
    var data = { };
    PostGenHtml(data, infoProdStkAUrl, function (obj) {
        $("#gridInfoProdStkA").html(obj);
        CerrarWaiting();
    }, function (obj) {
        ControlaMensajeError(obj.message);
        CerrarWaiting();
    });
}
function PresentarMov() {
    var data = { };
    PostGenHtml(data, infoProdMovUrl, function (obj) {
        $("#gridInfoProdMov").html(obj);
        CerrarWaiting();
    }, function (obj) {
        ControlaMensajeError(obj.message);
        CerrarWaiting();
    });
}  
function PresentarLP() {
    var data = { };
    PostGenHtml(data, infoProdLPUrl, function (obj) {
        $("#gridInfoProdLP").html(obj);
        CerrarWaiting();
    }, function (obj) {
        ControlaMensajeError(obj.message);
        CerrarWaiting();
    });
}