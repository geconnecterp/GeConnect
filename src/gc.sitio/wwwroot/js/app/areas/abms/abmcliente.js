$(function () {
	$("#btnCancel").on("click", function () {
		$("#btnFiltro").trigger("click");
	});
    $("#pagEstado").on("change", function () {
        var div = $("#divPaginacion");
        presentaPaginacion(div);
    });
    $("#btnBuscar").on("click", function () { buscarClientes(pagina); });

    //codigo trasladado a siteGen

	InicializaPantallaAbmProd();
    /*    AbrirWaiting();*/
    funcCallBack = buscarClientes;
	return true;
});

function InicializaPantallaAbmProd() {
	var tb = $("#tbGridProd tbody tr");
	if (tb.length === 0) {
        $("#divFiltro").collapse("show")
	}

    $("#lbRel01").text("TIPO");
	$("#lbRel02").text("ZONA");

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
            buscarClientes(num);
        }
    });
    $("#pagEstado").val(false);
    $("#divFiltro").collapse("hide")
    return true;
}

function buscarClientes(pag) {
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
        buscar
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

function selectReg(e) {
    $("#tbGridProd tbody tr").each(function (index) {
        $(this).removeClass("selected-row");
    });
    $(e).addClass("selected-row");

    var cta_id = e.cells[0].innerText.trim();
    if (cta_id !== "") {
        BuscarCliente(cta_id);
    }
}

function BuscarCliente(ctaId) {
    var data = { ctaId };
    PostGenHtml(data, buscarClienteUrl, function (obj) {

    }, function (obj) {
        ControlaMensajeError(obj.message);
        CerrarWaiting();
    });
}