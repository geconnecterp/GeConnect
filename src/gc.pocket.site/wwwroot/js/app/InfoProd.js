$(function () {

    //cargo el js para llamar desde esta funcion a la función de busqueda
    //verifico si se hace click en el boton buscar
    $("#btnBusquedaBase").on("click", function () {
        buscarProducto();
        return true;
    }); 

    $("#estadoFuncion").on("change", verificaEstado); //este control debe ser insertado el mismo o similar para cada modulo.

    $("#btnStkD").on("click",PresentarStkD);
    $("#btnStkBox").on("click", PresentarStkBox);
    $("#btnStkA").on("click", PresentarStkA);
    $("#btnMov").on("click", PresentarMov);
    $("#btnPr").on("click", PresentarLP);
});

function verificaEstado() {

    var res = $("#estadoFuncion").val();
    if (res === "true") {

        //traigo la variable productoBase e hidrato componentes
        var prod = productoBase;
        $("#Id").val(prod.p_id);
        $("#Marca").val(prod.p_m_marca);
        $("#Descipcion").val(prod.p_desc);
        $("#Capacidad").val(prod.p_m_capacidad);
        $("#ProveedorId").val("??????");
        $("#Familia").val("???????");
        $("#Rubro").val(prod.rub_desc);     

        $("#estadoFuncion").val(false);

        //PresentarStkD(prod.p_Id);
        $("#btnStkD").trigger("click");
        $("#btnBusquedaBase").prop("disabled", false);

    }
    return true;
}

function PresentarStkD(id) {
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
    return true;
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
    return true;
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
    return true;
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
    return true;
}