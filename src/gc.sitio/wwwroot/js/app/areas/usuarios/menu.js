$(function () {

    $("#btnFiltro").on("click", function () {
        if ($("#divFiltro").hasClass("show")) {
            $("#divDetalle").collapse("hide");
        }
    });

    //inicialmente desactivamos el boton de detalle
    $("#btnDetalle").prop("disabled", true);
    $("#btnCancel").on("click", function () {
        $("#btnFiltro").trigger("click");
    });
    $("#pagEstado").on("change", function () {
        var div = $("#divPaginacion");
        presentaPaginacion(div);
    });

    $("#btnBuscar").on("click", function () {
        //es nueva la busqueda no resguardamos la busqueda anterior. es util para paginado
        dataBak = "";
        //es una busqueda por filtro. siempre sera pagina 1
        pagina = 1;
        buscarPerfiles(pagina);
    });

    funcCallBack = buscarPerfiles;

    $(".inputEditable").on("keypress", analizaEnterInput);

    $("#BtnLiTab01").on("click", function () {
        tabAbm = 1;
        activarGrilla(tabGrid01);
    });
    $("#BtnLiTab02").on("click", function () { });
    $("#BtnLiTab03").on("click", function () { });

    InicializaPantallaCtrlMenu(Grids.GridPerfil);

    return true;
});

function buscarPerfiles(pagina) {
    AbrirWaiting();

    activarBotones(false);

    var buscar = $("#Buscar").val();
    var id = $("#Id").val();
    var id2 = $("#Id2").val();

    var data1 = { id, id2, buscar };
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

function selectRegPerfil(x, gridId) {
    //reinvoco para que me marque el registro 
    selectReg(x, gridId);
    //limpio el tab01 para que se seleccione el registro.
    //y desactivo el tab
    switch (tabAbm) {
        case 1:
            $("#divpanel01").empty();
            if ($("#divDetalle").is(":visible")) {
                $("#divDetalle").collapse("hide");
            }
            $("#btnDetalle").prop("disabled", true);
            activarGrilla(tabGrid01);
            activarBotones(false);
            break;
        case 2:
        case 3:
            break;
        default:
            return false;
    }

}

function InicializaPantallaCtrlMenu(grilla) {
    if (grilla !== Grids.GridPerfil) {
        switch (tabMn) {
            case 1:
                grilla = Grids.GridPerfil;
                if ($("#divDetalle").is(":visible")) {
                    $("#divDetalle").collapse("hide");
                }
                break;
            //case 2:
            //    break;
            //case 3:
            //    break;
            default:
                return false;

        }
    }

    nng = "#" + grilla;
    tb = $(nng + " tbody tr");
    if (tb.length === 0) {
        switch (tabMn) {
            case 1:
                $("#divFiltro").collapse("show");
                break;
            //case 2:
            //    break;
            //case 3:
            //    break;
            default:
                return false;
        }
    }


    accionBotones(AbmAction.CANCEL);

    //borra seleccion de registro si hubiera cargdo algun grid
    $("#" + grilla + " tbody tr").each(function (index) {
        $(this).removeClass("selectedEdit-row");
    });

    CerrarWaiting();
    return true;
}