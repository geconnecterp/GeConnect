$(function () {

    $("#btnFiltro").on("click", function () {
        if ($("#divFiltro").hasClass("show")) {
            $("#divDetalle").collapse("hide");
        }
        activarGrilla(Grids.GridUser);
    });

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
        $("#divpanel01").empty();
        dataBak = "";
        //es una busqueda por filtro. siempre sera pagina 1
        pagina = 1;
        buscarUsers(pagina);
    });
    //callback para que funcione la paginación
    funcCallBack = buscarUsers;

    //busqueda no gen de proveedores
    $(document).on("keydown.autocomplete", "input#cta_denominacion", function () {
        $(this).autocomplete({
            source: function (request, response) {
                data = { search: request.term }
                $.ajax({
                    url: autoComRel01Url,
                    type: "POST",
                    dataType: "json",
                    data: data,
                    success: function (obj) {
                        response($.map(obj, function (item) {
                            var texto = item.cta_Denominacion;
                            return { label: texto, value: item.cta_Denominacion, id: item.cta_Id, habilitada: item.ctac_habilitada };
                        }));
                    }
                })
            },
            minLength: 3,
            select: function (event, ui) {
                $("input#cta_id").val(ui.item.id);
                if (ui.item.habilitada === 'S') {
                    $("input#cta_denominacion").removeClass("text-danger").addClass("text-success");
                }
                else {
                    $("input#cta_denominacion").removeClass("text-success").addClass("text-danger");

                }

                return true;
            }
        });
    });


    $(".inputEditable").on("keypress", analizaEnterInput);

    $("#BtnLiTab01").on("click", function () {
        tabAbm = 1;
        activarGrilla(Grids.GridUser);
        //$("#btnAbmNuevo").prop("disabled", false);
        //$("#btnAbmElimi").prop("disabled", false);       

    });
    $("#BtnLiTab02").on("click", function () {
        tabAbm = 2;
        desactivarGrilla(Grids.GridUser);
        activarBotones(true);
        presentaPerfilesUsuario();
    });
    $("#BtnLiTab03").on("click", function () {
        tabAbm = 3;
        desactivarGrilla(Grids.GridUser);
        activarBotones(true);

        presentaAdministracionesUsuario();
    });
    $("#BtnLiTab04").on("click", function () {
        tabAbm = 4;
        desactivarGrilla(Grids.GridUser);
        activarBotones(true);

        presentaDerechosUsuario();
    });

    $(document).on("dblclick", "#" + Grids.GridUser + " tbody tr", function () {
        x = $(this);
        //se resguarda el registro de la tabla
        regSelected = x;
        ejecutaDblClickGrid1(x);
    });

    InicializaPantallaUser(Grids.GridUser);
    //inicia la pantalla presentando la primer pagina de usuarios
    $("#btnBuscar").trigger("click");
});

function selectUserRegDbl(x, gridId) {
    $("#" + gridId + " tbody tr").each(function (index) {
        $(this).removeClass("selectedEdit-row");
    });
    $(x).addClass("selectedEdit-row");
    var id = x.find("td:nth-child(1)").text();
    //resguardo el usuario seleccionado
    usuSelect = id;

    switch (tabAbm) {
        case 1:
            //se agrega por inyection el tab con los datos del producto
            EntidadEstado = x.find("td:nth-child(3)").text();
            var data = { id: id };
            EntidadSelect = id;
            desactivarGrilla(gridId);
            //se busca el perfil
            buscarUserServer(data);
            //se busca los usuarios del perfil
            /*buscarUsuario(data);*/
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

function presentaPerfilesUsuario() {

    PostGen({}, presentarPerfilUrl, function (obj) {
        if (obj.error === true) {
            CerrarWaiting();
            AbrirMensaje("ALGO NO SALIO BIEN!", obj.msg, function () {
                $("#msjModal").modal("hide");
            }, false, ["CONTINUAR"], "error!", null);
        }
        else if (obj.warn === true) {
            CerrarWaiting();

            AbrirMensaje("ATENCIÓN", obj.msg, function () {
                if (obj.auth === true) {
                    windows.location.href = login;
                }
                else {
                    $("#msjModal").modal("hide");
                }
            }, false, ["CONTINUAR"], "warn!", null);
        }
        else {

            jsonP = $.parseJSON(obj.arbol);
            $("#divPerfiles").jstree("destroy").empty();

            $("#divPerfiles").jstree({
                "core": { "data": jsonP },
                "checkbox": {
                    "keep_selected_style": false
                },
                "plugins": ['checkbox']
            });

            CerrarWaiting();
        }

    });
}

function presentaAdministracionesUsuario() {

    PostGen({}, presentarAdminsUrl, function (obj) {
        if (obj.error === true) {
            CerrarWaiting();
            AbrirMensaje("ALGO NO SALIO BIEN!", obj.msg, function () {
                $("#msjModal").modal("hide");
            }, false, ["CONTINUAR"], "error!", null);
        }
        else if (obj.warn === true) {
            CerrarWaiting();

            AbrirMensaje("ATENCIÓN", obj.msg, function () {
                if (obj.auth === true) {
                    windows.location.href = login;
                }
                else {
                    $("#msjModal").modal("hide");
                }
            }, false, ["CONTINUAR"], "warn!", null);
        }
        else {

            jsonA = $.parseJSON(obj.arbol);
            $("#divAdmins").jstree("destroy").empty();

            $("#divAdmins").jstree({
                "core": { "data": jsonA },
                "checkbox": {
                    "keep_selected_style": false
                },
                "plugins": ['checkbox']
            });

            CerrarWaiting();
        }

    });
}

function presentaDerechosUsuario() {

    PostGen({}, presentarDerecsUrl, function (obj) {
        if (obj.error === true) {
            CerrarWaiting();
            AbrirMensaje("ALGO NO SALIO BIEN!", obj.msg, function () {
                $("#msjModal").modal("hide");
            }, false, ["CONTINUAR"], "error!", null);
        }
        else if (obj.warn === true) {
            CerrarWaiting();

            AbrirMensaje("ATENCIÓN", obj.msg, function () {
                if (obj.auth === true) {
                    windows.location.href = login;
                }
                else {
                    $("#msjModal").modal("hide");
                }
            }, false, ["CONTINUAR"], "warn!", null);
        }
        else {

            jsonD = $.parseJSON(obj.arbol);
            $("#divDers").jstree("destroy").empty();

            $("#divDers").jstree({
                "core": { "data": jsonD },
                "checkbox": {
                    "keep_selected_style": false
                },
                "plugins": ['checkbox']
            });

            CerrarWaiting();
        }

    });
}

function InicializaPantallaUser(grilla) {
    //si no es una de las grillas deteminadas en el modulo, se asignará una grilla segun el tab que se encuentre.
    if (grilla !== Grids.GridUser) {
        switch (tabAbm) {
            case 1:
                grilla = Grids.GridUser;
                if ($("#divDetalle").is(":visible")) {
                    $("#divDetalle").collapse("hide");
                }
                break;
            case 2:
                activarArbol("#divPerfiles", "#", false, true);
                activarBotones(true);
                break;
            case 3:
                activarArbol("#divAdmins", "#", false, true);
                activarBotones(true);
                break;
            case 4:
                activarArbol("#divDers", "#", false, true);
                activarBotones(true);
                break;
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
            break;
        default:
            break;
    }

    //al inicio de todo se procede a la busqueda de los datos y carga de la grilla

    CerrarWaiting();
    return true;
}

function analizaEstadoBtnDetalle() {
    var res = $("#divDetalle").hasClass("show");
    if (res === true) {
        selectRegProd(regSelected, tabGrid01);
    }
    return true;

}

function buscarUsers(pagina) {
    AbrirWaiting();
    //desactivamos los botones de acción
    activarBotones(false);


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
function buscarUsuario(data) {
    PostGenHtml(data, buscarUserUrl, function (obj) {
        $("#divUsuario").html(obj);

        CerrarWaiting();
    });
}
function buscarUserServer(data) {
    PostGenHtml(data, buscarUserUrl, function (obj) {
        $("#divpanel01").html(obj);
        ////se procede a buscar la grilla de barrado
        //buscarBarrado(data);
        ////se procede a buscar la grilla de Sucursales
        //buscarLimite(data);

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
            $("#BtnLiTab04").prop("disabled", true);
            $("#BtnLiTab04").addClass("text-danger");
        }
        else {
            $("#BtnLiTab02").prop("disabled", false);
            $("#BtnLiTab02").removeClass("text-danger");
            $("#BtnLiTab03").prop("disabled", false);
            $("#BtnLiTab03").removeClass("text-danger");
            $("#BtnLiTab04").prop("disabled", false);
            $("#BtnLiTab04").removeClass("text-danger");
        }

        CerrarWaiting();

    });
}

function selectRegUser(x, gridId) {
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
            activarGrilla(Grids.GridUser);
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
    AbrirWaiting("Espere mientras se busca el Usuario seleccionado...");
    selectUserRegDbl(x, Grids.GridPerfil);
}

//function ejecutaDblClickGrid2(x) {
//    AbrirWaiting("Espere mientras se busca el Barrado seleccionado...");
//    selectAbmRegDbl(x, tabGrid02);

//}
//function ejecutaDblClickGrid3(x) {
//    AbrirWaiting("Espere mientras se busca el Limite de Stock seleccionado...");
//    selectAbmRegDbl(x, tabGrid03);
//}

function selectAbmRegDbl(x, gridId) {
    $("#" + gridId + " tbody tr").each(function (index) {
        $(this).removeClass("selectedEdit-row");
    });
    $(x).addClass("selectedEdit-row");
    var id = x.find("td:nth-child(1)").text();


    switch (tabAbm) {
        case 1:
            //se agrega por inyection el tab con los datos del producto
            EntidadEstado = x.find("td:nth-child(9)").text();
            var data = { p_id: id };
            EntidadSelect = id;
            desactivarGrilla(tabGrid01);
            buscarUserServer(data);
            posicionarRegOnTop(x);
            break;
        case 2:
            //se busca el dato del barral 
            var data = { barradoId: id };
            PostGen(data, buscarBarradoUrl, function (obj) {
                CerrarWaiting();
                if (obj.error === true) {
                    AbrirMensaje("¡¡Algo no fué bien!!", obj.msg, function () {
                        $("#msjModal").modal("hide");
                        return true;
                    }, false, ["Aceptar"], "error!", null);
                } else if (obj.warn === true) {
                    AbrirMensaje("ATENCIÓN", obj.msg, function () {
                        if (obj.auth === true) {
                            window.location.href = login;
                        } else {
                            $("#msjModal").modal("hide");
                        }
                        return true;
                    }, false, ["Aceptar"], "warn!", null);
                }
                else {
                    //se presentan los datos en los controles

                    $("#p_id").val(obj.datos.p_id);
                    $("#p_id_barrado").val(obj.datos.p_id_barrado);
                    $("#p_unidad_pres").val(obj.datos.p_unidad_pres);
                    $("#p_unidad_x_bulto").val(obj.datos.p_unidad_x_bulto);
                    $("#p_bulto_x_piso").val(obj.datos.p_bulto_x_piso);
                    $("#p_piso_x_pallet").val(obj.datos.p_piso_x_pallet);
                    $("#tba_id").val(obj.datos.tba_id);
                    //activar botones de acción
                    activarBotones(true);

                    $("#BtnLiTab01").prop("disabled", true);
                    $("#BtnLiTab01").addClass("text-danger");
                    $("#BtnLiTab03").prop("disabled", true);
                    $("#BtnLiTab03").addClass("text-danger");
                }

            });
            break;
        case 3:
            //se busca  
            var data = { barradoId: id };
            PostGen(data, buscarBarradoUrl, function (obj) {
                CerrarWaiting();
                if (obj.error === true) {
                    AbrirMensaje("¡¡Algo no fué bien!!", obj.msg, function () {
                        $("#msjModal").modal("hide");
                        return true;
                    }, false, ["Aceptar"], "error!", null);
                } else if (obj.warn === true) {
                    AbrirMensaje("ATENCIÓN", obj.msg, function () {
                        if (obj.auth === true) {
                            window.location.href = login;
                        } else {
                            $("#msjModal").modal("hide");
                        }
                        return true;
                    }, false, ["Aceptar"], "warn!", null);
                }
                else {
                    //se presentan los datos en los controles

                    $("#p_id").val(obj.datos.p_id);
                    $("#p_id_barrado").val(obj.datos.p_id_barrado);
                    $("#p_unidad_pres").val(obj.datos.p_unidad_pres);
                    $("#p_unidad_x_bulto").val(obj.datos.p_unidad_x_bulto);
                    $("#p_bulto_x_piso").val(obj.datos.p_bulto_x_piso);
                    $("#p_piso_x_pallet").val(obj.datos.p_piso_x_pallet);
                    $("#tba_id").val(obj.datos.tba_id);
                }

            });
            break;
        default:
            return false;
    }
}