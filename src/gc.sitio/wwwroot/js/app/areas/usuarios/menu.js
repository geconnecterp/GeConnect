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
        tabMn = 1;
        activarGrilla(Grids.GridPerfil);
    });
    $("#BtnLiTab02").on("click", function () {
        tabMn = 2;

        desactivarGrilla(Grids.GridPerfil);
        ejecutarAlta();
    });
    //$("#BtnLiTab03").on("click", function () { });

    $(document).on("dblclick", "#" + Grids.GridPerfil + " tbody tr", function () {
        x = $(this);
        //se resguarda el registro de la tabla
        regSelected = x;
        ejecutaDblClickGrid1(x);
    });

    inicializaPantallaCtrlMenu(Grids.GridPerfil);

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

    PostGenHtml(data, buscarPerfilesUrl, function (obj) {
        $("#divGrilla").html(obj);
        $("#divFiltro").collapse("hide")
        PostGen({}, buscarPerfilesMetadataURL, function (obj) {
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
    switch (tabMn) {
        case 1:
            $("#divpanel01").empty();
            if ($("#divDetalle").is(":visible")) {
                $("#divDetalle").collapse("hide");
            }
            $("#btnDetalle").prop("disabled", true);
            activarGrilla(gridId);
            activarBotones(false);
            break;
        case 2:
        case 3:
            break;
        default:
            return false;
    }

}

function ejecutaDblClickGrid1(x) {
    AbrirWaiting("Espere mientras se busca el producto seleccionado...");
    selectMnRegDbl(x, Grids.GridPerfil);
}

function selectMnRegDbl(x, gridId) {
    $("#" + gridId + " tbody tr").each(function (index) {
        $(this).removeClass("selectedEdit-row");
    });
    $(x).addClass("selectedEdit-row");
    var id = x.find("td:nth-child(1)").text();

    switch (tabMn) {
        case 1:
            //se agrega por inyection el tab con los datos del producto
            EntidadEstado = x.find("td:nth-child(3)").text();
            var data = { id: id };
            EntidadSelect = id;
            desactivarGrilla(gridId);
            //se busca el perfil
            buscarPerfilServer(data);
            //se busca los usuarios del perfil
            buscarUsuariosXPerfil(data);
            //se posiciona el registro seleccionado
            posicionarRegOnTop(x);
            break;        
        default:
            return false;
    }


    //agrego el id en el control de busqueda simple y acciono el buscar.
    //$("#busquedaModal").modal("toggle");
    //$("input#Busqueda").val(id);
    //$("#btnBusquedaBase").trigger("click");
}

function buscarUsuariosXPerfil(data) {
    PostGenHtml(data, buscarPerfilUsersUrl, function (obj) {
        $("#divPerfilUsers").html(obj);
    });
}

function buscarPerfilServer(data) {
    PostGenHtml(data, buscarPerfilUrl, function (obj) {
        $("#divpanel01").html(obj);
        //se procede a buscar la grilla de barrado

        $("#btnDetalle").prop("disabled", false);
        $("#divFiltro").collapse("hide");
        $("#divDetalle").collapse("show");

        //activar botones de acción
        activarBotones(true);

        if (EntidadEstado !== "S" && accion !== "") {
            $("#BtnLiTab02").prop("disabled", true);
            $("#BtnLiTab02").addClass("text-danger");
            $("#BtnLiTab03").prop("disabled", true);
            $("#BtnLiTab03").addClass("text-danger");

        }
        else {
            $("#BtnLiTab02").prop("disabled", false);
            $("#BtnLiTab02").removeClass("text-danger");
            $("#BtnLiTab03").prop("disabled", false);
            $("#BtnLiTab03").removeClass("text-danger");
        }

        CerrarWaiting();
    });
    return true;
}


function inicializaPantallaCtrlMenu(grilla) {
    if (grilla !== Grids.GridPerfil) {
        switch (tabMn) {
            case 1:
            case 2:
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