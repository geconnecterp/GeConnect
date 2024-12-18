$(function () {

    $("#btnDetalle").prop("disabled", true);

    $("#btnCancel").on("click", function () {
        $("#btnFiltro").trigger("click");
    });
    $("#pagEstado").on("change", function () {
        var div = $("#divPaginacion");
        presentaPaginacion(div);
    });
    $("#btnBuscar").on("click", function () { buscarProductos(pagina); });

    $(".inputEditable").on("keypress", analizaEnterInput);

    $(".tristate").on("mousedown", function (e) {        
        var ctrl = $(this);
        e.preventDefault()
        setTimeout(function () {
            if (ctrl.prop('indeterminate')) {
                // Si está en estado indeterminado, lo marca
                ctrl.prop('checked', true).prop('indeterminate', false);
            } else if (ctrl.prop('checked')) {
                // Si está marcado, lo desmarca
                ctrl.prop('checked', false);
            } else {
                // Si está desmarcado, lo pone en estado indeterminado
                ctrl.prop('indeterminate', true);
            }
        }, 0); // Asegura que la ejecución se realice después del clic
    });

    /*        CODIGO TRASLADADO AL SITIGEN.JS            */

    ////check generico REL02 activando componentes disables
    //$("#chkRel02").on("click", function () {
    //    if ($("#chkRel02").is(":checked")) {
    //        $("#Rel02").prop("disabled", false);
    //        $("#Rel02List").prop("disabled", false);
    //    }
    //    else {
    //        $("#Rel02").prop("disabled", true);
    //        $("#Rel02List").prop("disabled", true);
    //    }
    //})

    InicializaPantallaAbmProd();
    funcCallBack = buscarProductos;
/*    AbrirWaiting();*/
    return true;
});

function InicializaPantallaAbmProd() {   
    var tb = $("#tbGridProd tbody tr");
    if (tb.length === 0) {
        $("#divFiltro").collapse("show")
    } 

    $("#lbRel01").text("PROVEEDOR");
    $("#lbRel02").text("RUBRO");


    CerrarWaiting();
    return true;
}
 
function cargaPaginacion() {
    $("#divPaginacion").pagination({
        items: totalRegs,
        itemsOnPage: pagRegs,        
        cssStyle: "dark-theme",
        currentPage: pagina,
        onPageClick: function (num) {
            buscarProductos(num);
        }
    });
    $("#pagEstado").val(false);
    $("#divFiltro").collapse("hide")
    return true;
}

function buscarProductos(pag) {
    AbrirWaiting();
    var buscar = $("#Buscar").val();
    var id = $("#Id").val();
    var id2 = $("#Id2").val();
    var r01 = [];
    var r02 = [];    
    $("#Rel01List").children().each(function (i, item) { r01.push($(item).val()) });
    $("#Rel02List").children().each(function (i, item) { r02.push($(item).val()) });

    var data1 = {
        id, id2,
        rel01: r01,
        rel02: r02,
        rel03: [],
        "fechaD": null, //"0001-01-01T00:00:00",
        "fechaH": null, //"0001-01-01T00:00:00",
        buscar,
    };

    var buscaNew = JSON.stringify(dataBak) != JSON.stringify(data1)

    if (buscaNew === false) {
        //son iguales las condiciones cambia de pagina
        pagina = pag;
    }
    else {
        dataBak = data1;
        pagina = 1;
        pag = 1;
    }

    var sort = null;
    var sortDir = null

    var data2 = { sort, sortDir, pag, buscaNew }

    var data = $.extend({}, data1, data2);


    PostGenHtml(data, buscarUrl, function (obj) {
        $("#divGrilla").html(obj);
        $("#divFiltro").collapse("hide")
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

function selectAbmRegDbl(x) {    
    AbrirWaiting("Espere mientras se busca el producto seleccionado...");
    $("#tbGridProd tbody tr").each(function (index) {
        $(this).removeClass("selectedEdit-row");
    });
    $(x).addClass("selectedEdit-row");
    var id = x.cells[0].innerText.trim();

    var data = { p_id: id };
    PostGenHtml(data, buscarProdUrl, function (obj) {
        $("#divpanel01").html(obj);
        $("#btnDetalle").prop("disabled", false);
        $("#divFiltro").collapse("hide");
        $("#divDetalle").collapse("show");
        
        CerrarWaiting();

    });

    //agrego el id en el control de busqueda simple y acciono el buscar.
    //$("#busquedaModal").modal("toggle");
    //$("input#Busqueda").val(id);
    //$("#btnBusquedaBase").trigger("click");
}
