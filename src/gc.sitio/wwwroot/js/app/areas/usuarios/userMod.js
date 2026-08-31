$(function () {

    $("#btnFiltro").on("click", function () {
        if ($("#divFiltro").hasClass("show")) {
            $("#divDetalle").collapse("hide");
        }
        activarGrilla(Grids.GridUser);
    });

    $("#btnDetalle").prop("disabled", true);

    $("#btnCancel").off("click.userInit").on("click.userInit", inicializarModuloUsuarios);

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
                            return normalizarClienteAutocomplete(item);
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
            },
            change: function (event, ui) {
                if (!ui.item) {
                    $("input#cta_id").val("");
                    $(this).removeClass("text-success text-danger");
                }
            }
        });
        aplicarRenderClienteAutocomplete($(this));
    });

    // El identificador de usuario se define manualmente en el alta y, por regla
    // de negocio, debe conservarse siempre en minúsculas.
    $(document).on("input", "#usu_id:not(:disabled)", function () {
        this.value = this.value.toLowerCase();
    });

    $(".inputEditable").on("keypress", analizaEnterInput);

    $("#BtnLiTab01").on("click", function () {
        tabAbm = 1;
        activarGrilla(Grids.GridUser);
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

    $(document).on("dblclick", "#tbGridUsers tbody tr", function () {
        x = $(this);
        regSelected = x;
        ejecutaDblClickGrid1(x);
    });

    InicializaPantallaUser(Grids.GridUser);
    $("#divFiltro").collapse("show");
});

function selectUserRegDbl(x, gridId) {
    $("#" + gridId + " tbody tr").each(function (index) {
        $(this).removeClass("selectedEdit-row");
    });
    $(x).addClass("selectedEdit-row");
    var id = x.find("td:nth-child(1)").text();
    //resguardo el usuario seleccionado
    usuSelect = id;

    //al hacer click sobre usuario siempre tiene que volver a tab 1
    $("#BtnLiTab01").trigger("click");

    //switch (tabAbm) {
    //    case 1:
    //se agrega por inyection el tab con los datos del producto
    EntidadEstado = x.find("td:nth-child(3)").text();
    var data = { id: id };
    EntidadSelect = id;

    // Resetear acciones antes de cargar nuevo usuario
    accion = "";
    accion02 = "";
    accion03 = "";
    accion04 = "";

    desactivarGrilla(gridId);
    //se busca el perfil
    buscarUserServer(data);
    //se busca los usuarios del perfil
    /*buscarUsuario(data);*/
    //se posiciona el registro seleccionado
    posicionarRegOnTop(x);
    //        break;
    //    default:
    //        //return false;
    //        break;
    //}


    //agrego el id en el control de busqueda simple y acciono el buscar.
    //$("#busquedaModal").modal("toggle");
    //$("input#Busqueda").val(id);
    //$("#btnBusquedaBase").trigger("click");
}

function presentaPerfilesUsuario() {

    PostGen({ usuId: usuSelect }, presentarPerfilUrl, function (obj) {
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
                    window.location.href = login;
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
            }).one("ready.jstree", function () {
                activarArbol("#divPerfiles", "#", false);
            });

            CerrarWaiting();
        }

    });
}

function presentaAdministracionesUsuario() {

    PostGen({ usuId: usuSelect }, presentarAdminsUrl, function (obj) {
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
                    window.location.href = login;
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
            }).one("ready.jstree", function () {
                activarArbol("#divAdmins", "#", false);
            });

            CerrarWaiting();
        }

    });
}

function presentaDerechosUsuario() {

    PostGen({ usuId: usuSelect }, presentarDerecsUrl, function (obj) {
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
                    window.location.href = login;
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
            }).one("ready.jstree", function () {
                activarArbol("#divDers", "#", false);
            });

            CerrarWaiting();
        }

    });
}

function InicializaPantallaUser(grilla) {
    if (!grilla) {
        grilla = Grids.GridUser;
    }

    if ($("#divDetalle").is(":visible")) {
        $("#divDetalle").collapse("hide");
    }

    $("#divpanel01").empty();
    $("#divGrilla, #divPaginacion").show();
    accion = "";
    accion02 = "";
    accion03 = "";
    accion04 = "";

    $("#tbGridUsers tbody tr").removeClass("selectedEdit-row");
    regSelected = null;
    usuSelect = "";

    $("#btnAbmAceptar, #btnAbmCancelar").prop("disabled", true).hide();
    $("#btnFiltro, #BtnLiTab01, #BtnLiTab02, #BtnLiTab03, #BtnLiTab04")
        .prop("disabled", false)
        .removeClass("text-danger");
    activarBotones(false);
    $("#btnDetalle").prop("disabled", true);

    CerrarWaiting();

    setTimeout(function () {
        activarGrilla("tbGridUsers");
    }, 200);
}

function cancelarOperacionUsuario() {
    CerrarWaiting();

    $("#btnFiltro, #btnDetalle, #BtnLiTab01, #BtnLiTab02, #BtnLiTab03, #BtnLiTab04")
        .prop("disabled", false)
        .removeClass("text-danger");
    $("#btnAbmAceptar, #btnAbmCancelar").prop("disabled", true).hide();

    switch (tabAbm) {
        case 1:
            accion = "";
            activarControles(false);
            if (usuSelect) {
                desactivarGrilla(Grids.GridUser);
                buscarUserServer({ id: usuSelect });
            } else {
                InicializaPantallaUser(Grids.GridUser);
            }
            break;
        case 2:
            accion02 = "";
            activarArbol("#divPerfiles", "#", false, true);
            activarBotones(true);
            desactivarGrilla(Grids.GridUser);
            break;
        case 3:
            accion03 = "";
            activarArbol("#divAdmins", "#", false, true);
            activarBotones(true);
            desactivarGrilla(Grids.GridUser);
            break;
        case 4:
            accion04 = "";
            activarArbol("#divDers", "#", false, true);
            activarBotones(true);
            desactivarGrilla(Grids.GridUser);
            break;
    }
}

function inicializarModuloUsuarios() {
    AbrirWaiting("Inicializando Gestión de Usuarios...");
    PostGen({ moduleInstanceId }, inicializarModuloUrl, function (obj) {
        CerrarWaiting();
        if (obj.error === true || obj.warn === true) {
            AbrirMensaje("ATENCIÓN", obj.msg, function () {
                $("#msjModal").modal("hide");
            }, false, ["CONTINUAR"], obj.error === true ? "error!" : "warn!", null);
            return;
        }
        window.location.href = homeUser;
    });
}

// NUEVA FUNCIÓN: Simplificar la lógica de análisis del botón detalle
function analizaEstadoBtnDetalle() {
    // Esta función ahora solo verifica el estado
    // La lógica de cancelación se maneja en el evento click de userRules.js
    return $("#divDetalle").is(":visible");
}

function buscarUsers(pagina,callback) {
    AbrirWaiting();
    //desactivamos los botones de acción
    activarBotones(false);


    var buscar = $("#Buscar").val();
    var id = $("#Id").val();
    var id2 = $("#Id2").val();


    var data1 = {
        id, id2,
        buscar,
        moduleInstanceId,
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
        activarBotones(false);
        $("#divFiltro").collapse("hide")
        PostGen({ moduleInstanceId }, buscarMetadataURL, function (obj) {
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

        // Ejecutar callback si existe (para seleccionar registro después de buscar)
        if (callback && typeof callback === 'function') {
            callback();
        }
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

        $("#btnDetalle").prop("disabled", false);
        $("#divFiltro").collapse("hide");
        $("#divDetalle").collapse("show");
        $("#divGrilla, #divPaginacion").hide();

        //activar botones de acción
        activarBotones(true);
        actualizarAccionesSeguridadUsuario();

        // Verificar si hay una acción activa antes de deshabilitar tabs
        var hayAccionActiva = (accion !== "" && accion !== AbmAction.CANCEL) ||
            (accion02 !== "" && accion02 !== AbmAction.CANCEL) ||
            (accion03 !== "" && accion03 !== AbmAction.CANCEL) ||
            (accion04 !== "" && accion04 !== AbmAction.CANCEL);

        if (EntidadEstado !== "NO" && hayAccionActiva) {
            $("#BtnLiTab02").prop("disabled", true).addClass("text-danger");
            $("#BtnLiTab03").prop("disabled", true).addClass("text-danger");
            $("#BtnLiTab04").prop("disabled", true).addClass("text-danger");
        }
        else {
            $("#BtnLiTab02").prop("disabled", false).removeClass("text-danger");
            $("#BtnLiTab03").prop("disabled", false).removeClass("text-danger");
            $("#BtnLiTab04").prop("disabled", false).removeClass("text-danger");
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
    selectUserRegDbl(x, "tbGridUsers");
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
