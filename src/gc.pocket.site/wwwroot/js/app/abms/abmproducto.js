﻿$(function () {
    $("#btnCancel").on("click", function () {
        $("#btnFiltro").trigger("click");
    });
    $("#pagEstado").on("change", function () {
        var div = $("#divPaginacion");
        presentaPaginacion(div);
    });
    $("#btnBuscar").on("click", function () { buscarProductos(pagina); });

    //check generico REL02 activando componentes disables
    $("#chkRel02").on("click", function () {
        if ($("#chkRel02").is(":checked")) {
            $("#Rel02").prop("disabled", false);
            $("#Rel02List").prop("disabled", false);
        }
        else {
            $("#Rel02").prop("disabled", true);
            $("#Rel02List").prop("disabled", true);
        }
    })

    InicializaPantallaAbmProd();
/*    AbrirWaiting();*/
    return true;
});

function InicializaPantallaAbmProd() {   
    var tb = $("#tbGridProd tbody tr");
    if (tb.length === 0) {
        $("#collapseExample").collapse("show")
    } 

    $("#lbRel01").text("PROVEEDORES");
    $("#lbRel02").text("RUBRO");


    CerrarWaiting();
    return true;
}
 
//function cargaPaginacion() {
//    $("#divPaginacion").pagination({
//        items: totalRegs,
//        itemsOnPage: pagRegs,        
//        cssStyle: "dark-theme",
//        currentPage: pagina,
//        onPageClick: function (num) {
//            buscarProductos(num);
//        }
//    });
//    $("#pagEstado").val(false);
//    $("#collapseExample").collapse("hide")
//    return true;
//}

function buscarProductos(pag) {
    AbrirWaiting();
    var buscar = $("#Buscar").val();
    var id = $("#Id").val();
    var id2 = $("#Id2").val();
    if (buscar !== search) {
        pagina = 1;
        pag = 1;
        search = buscar;
    }
    else {
        pagina = pag;
    }

    var data = {
        id, id2,
        rel01: [],
        rel02: [],
        rel03: [],
        "fechaD": null, //"0001-01-01T00:00:00",
        "fechaH": null, //"0001-01-01T00:00:00",
        buscar,
        "sort": null,
        "sortDir": null,
        pag
    };

    PostGenHtml(data, buscarUrl, function (obj) {
        $("#divGrilla").html(obj);
        $("#collapseExample").collapse("hide")
        PostGen({}, buscarMetadataURL, function (obj) {
            if (obj.error === true) {
                AbrirMensaje("ATENCIÓN", obj.msg, function () {
                    $("#msjModal").modal("hide");
                    return true;
                }, false, ["Aceptar"], "error!", null);   
            }
            else {
                totalRegs = obj.metadata.totalCount;
                pags = obj.metadata.totalPages;
                pagRegs = obj.metadata.pageSize;

                $("#pagEstado").val(true).trigger("change");
            }

        });
        CerrarWaiting();
    }, function (obj) {
        ControlaMensajeError(obj.message);
        CerrarWaiting();
    });

}