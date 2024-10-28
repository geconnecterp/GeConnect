$(function () {

    //cargo el js para llamar desde esta funcion a la función de busqueda
    //verifico si se hace click en el boton buscar
    $("#btnBusquedaBase").on("click", function () {
        buscarProducto();
        return true;
    });

    $("#estadoFuncion").on("change", verificaEstado); //este control debe ser insertado el mismo o similar para cada modulo.

    $("#btnStkD").on("click", PresentarStkD);
    $("#btnStkBox").on("click", PresentarStkBox);
    $("#btnStkA").on("click", PresentarStkA);
    $("#btnBuscarMov").on("click", PresentarMov);    
    $("#btnPr").on("click", PresentarLP);

    //asignamos valor a los controles de fecha
    var f = new Date();
    //var month = ('0' + (f.getMonth() + 1)).slice(-2); // Asegura que el mes siempre tenga dos dígitos
    //var day = ('0' + f.getDate()).slice(-2); // Asegura que el día siempre tenga dos dígitos
    //var newfecha = f.getFullYear() + '-' + month + '-' + day;

    $("#fdesde").val(formatoFechaYMD(restarFecha(f, 7)));
    $("#fhasta").val(formatoFechaYMD(f));
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
        if (prod.p_id_prov !== null) {
            $("#ProveedorId").val(prod.p_id_prov);
        }

        $("#Familia").val(prod.pg_desc);
        $("#Rubro").val(prod.rub_desc);

        $("#estadoFuncion").val(false);

        //PresentarStkD(prod.p_Id);
        $("#btnStkD").trigger("click");
        $("#btnBusquedaBase").prop("disabled", false);

    }
    return true;
}

function PresentarStkD(id) {
    AbrirWaiting();
    var data = {};
    PostGenHtml(data, infoProdStkDUrl, function (obj) {
        $("#gridInfoProdStkD").html(obj);
        CerrarWaiting();
    }, function (obj) {
        ControlaMensajeError(obj.message);
        CerrarWaiting();
    });
}

function PresentarStkBox() {
    AbrirWaiting();
    var data = {};
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
    AbrirWaiting();
    var data = {};
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
    //AbrirWaiting("Espere... se estan recuperando los datos.");
    var idtm = $("#TmId").find(":selected").val();
    var data = { idTm: idtm, fdesde:$("#fdesde").val(),fhasta:$("#fhasta").val()};
    PostGenHtml(data, infoProdMovUrl, function (obj) {
        $("#gridInfoProdMov").html(obj);
        CerrarWaiting();
        return true
    }, function (obj) {
        ControlaMensajeError(obj.message);
        CerrarWaiting();
        return true;
    });
    return true;
}
function PresentarLP() {
    AbrirWaiting();
    var data = {};
    PostGenHtml(data, infoProdLPUrl, function (obj) {
        $("#gridInfoProdLP").html(obj);
        CerrarWaiting();
    }, function (obj) {
        ControlaMensajeError(obj.message);
        CerrarWaiting();
    });
    return true;
}