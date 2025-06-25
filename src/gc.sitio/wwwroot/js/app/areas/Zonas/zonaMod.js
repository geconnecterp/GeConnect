$(function () {

    $("#btnFiltro").on("click", function () {
        if ($("#divFiltro").hasClass("show")) {
            $("#divDetalle").collapse("hide");
        }
        activarGrilla(Grids.GridZona);
    });

    $("#btnDetalle").prop("disabled", true);
    $("#btnCancel").on("click", function () {
        window.location.href = homeZonaUrl;
    });
    $("#pagEstado").on("change", function () {
        var div = $("#divPaginacion");
        presentaPaginacion(div);
    });
    $("#btnBuscar").on("click", function () {

        //es nueva la busqueda no resguardamos la busqueda anterior. es util para paginado
        $("#divpanel01").empty();
        dataBak = "";
        //es una busqueda por filtro. siempre sera pagina 1
        pagina = 1;
        buscarZonaes(pagina);
    });
    //callback para que funcione la paginación
    funcCallBack = buscarZonaes;

    $(".inputEditable").on("keypress", analizaEnterInput);

    $("#BtnLiTab01").on("click", function () {
        tabAbm = 1;
        activarGrilla(Grids.GridZona);    

    });
   

    $(document).on("dblclick", "#" + Grids.GridZona + " tbody tr", function () {
        x = $(this);
        //se resguarda el registro de la tabla
        regSelected = x;
        ejecutaDblClickGrid1(x);
    });

    InicializaPantallaZona(Grids.GridZona);
    //inicia la pantalla presentando la primer pagina de usuarios
    //$("#btnBuscar").trigger("click");
});

function InicializaPantallaZona(grilla) {
    //si no es una de las grillas deteminadas en el modulo, se asignará una grilla segun el tab que se encuentre.

    switch (tabAbm) {
        case 1:
            grilla = Grids.GridZona;
            if ($("#divDetalle").is(":visible")) {
                $("#divDetalle").collapse("hide");
            }
            $("#divFilter").collapse("show");
            break;

        default:
            return false;
    }

    nng = "#" + grilla;
    tb = $(nng + " tbody tr");
    if (tb.length === 0) {
        switch (tabAbm) {
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
    //TODO: ESTO SOLO LO HACE SI ES LA GRILLA PPAL (TABAMB = 1)
    switch (tabAbm) {
        case 1:
            $("#" + grilla + " tbody tr").each(function (index) {
                $(this).removeClass("selectedEdit-row");
            });

            activarGrilla(grilla);
            break;
        default:
            break;
    }



    //al inicio de todo se procede a la busqueda de los datos y carga de la grilla

    CerrarWaiting();
    return true;
}

function selectRegZona(x, gridId) {
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
            activarGrilla(gridId);
            activarBotones(false);
            break;
        case 2:
            InicializaPantallaZona(Grids.GridZona);

            break;
        default:
            return false;
    }

}

function ejecutaDblClickGrid1(x) {
    AbrirWaiting("Espere mientras se busca el Zona solicitado...");
    selectZonaDbl(x, Grids.GridZona);
}

function selectZonaDbl(x, gridId) {
    $("#" + gridId + " tbody tr").each(function (index) {
        $(this).removeClass("selectedEdit-row");
    });
    $(x).addClass("selectedEdit-row");
    var id = x.find("td:nth-child(1)").text();

    switch (tabAbm) {
        case 1:
            //se agrega por inyection el tab con los datos del producto
            EntidadEstado = x.find("td:nth-child(3)").text();
            var data = { id: id };
            EntidadSelect = id;
            //desactivarGrilla(gridId);
            //se busca el perfil
            buscarZona(data);           
            //se posiciona el registro seleccionado
            posicionarRegOnTop(x);
            break;
        default:
            return false;
    }
}


function buscarZonaes(pagina) {
    AbrirWaiting();
    //desactivamos los botones de acción
    activarBotones2(false);


    var buscar = $("#Buscar").val();
    var id = $("#Id").val();
    var id2 = $("#Id2").val();


    var data1 = {
        id, id2,
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

