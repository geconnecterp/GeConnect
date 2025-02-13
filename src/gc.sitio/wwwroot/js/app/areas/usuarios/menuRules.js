$(function () {
    $("#btnAbmNuevo").on("click", ejecutarAlta);
    $("#btnAbmModif").on("click", ejecutarModificacion);
    $("#btnAbmElimi").on("click", ejecutarBaja);

    $("#btnAbmCancelar").on("click", inicializaPantallaCtrlMenu);
    $("#btnAbmAceptar").on("click", confirmarOperacionCtrlMenu);

    $("#btnDetalle").on("mousedown", analizaEstadoBtnDetalle);

    $("#MenuId").on("change", buscarMenu);

    //$(document).on('changed.jstree', '#menu', function (e, mndata) {

    //    switch (mndata.action) {
    //        case "deselect_node":
    //            updateAssignedState(mndata.node, false);
    //            break;
    //        case "select_node":
    //            updateAssignedState(mndata.node, true);
    //            break;
    //        case "model":
    //            break;
    //        case "ready":
    //            break;
    //        default:
    //    }

    //});
});

// Función para actualizar el estado de 'asignado'
//function updateAssignedState(nodo, state) {
//    // Buscar el nodo en el menú y actualizar su estado
//    var nodoId = nodo.id;
//    jsonMenuActual.forEach(function (item) {
//        if (item.id === nodoId) {
//            item.asignado = state;
//            item.state.selected = state;

//            if (item.children.length > 0) {
//                item.children.forEach(function (child) {
//                    if (child.padre === nodoId) {
//                        child.asignado = state;
//                        child.state.selected = state;
//                    }
//                });
//            }
//        }
//    });
//}

function buscarMenu() {
    AbrirWaiting();
    var mnId = $("#MenuId option:selected").val();
    data = { menuId: mnId, perfil: $("#perfil_id").val() };
    //activamos los botones de confirmación o cancelacion.
    $("#btnAbmAceptar").prop("disabled", false);
    $("#btnAbmCancelar").prop("disabled", false);

    PostGen(data, obtenerMenuPerfilUrl, function (obj) {
        if (obj.error === true) {
            CerrarWaiting();
            AbrirMensaje("ALGO NO SALIO BIEN!", obj.msg, function () {
                $("#msjModal").modal("hide");
            }, false, ["CONTINUAR"], "error!", null);
        }
        else if (obj.warn === true) {
            CerrarWaiting();

            AbrirMensaje("ATENCIÓN", obj.msg, function () {
                $("#msjModal").modal("hide");
            }, false, ["CONTINUAR"], "warn!", null);
        }
        else {

            jsonMenuActual = $.parseJSON(obj.arbol);
            $("#menu").jstree("destroy").empty();
            $("#menu").jstree({
                "core": { "data": jsonMenuActual },
                "checkbox": {
                    "keep_selected_style": false
                },
                "plugins": ['checkbox']
            });

            CerrarWaiting();
        }
    });

}


function accionBotones(btn) {
    if (btn === AbmAction.ALTA ||
        btn === AbmAction.MODIFICACION ||
        btn === AbmAction.BAJA) {
        switch (tabMn) {
            case 1:
                accion = btn;
                break;
            case 2:
                accion02 = btn;
                break;
            case 3:
                accion03 = btn;
                break;
        }

        $("#btnFiltro").prop("disabled", true);
        $("#btnDetalle").prop("disabled", true);
        $("#BtnLiTab01").prop("disabled", true);
        $("#BtnLiTab02").prop("disabled", true);


        $("#btnAbmNuevo").prop("disabled", true);
        $("#btnAbmModif").prop("disabled", true);
        $("#btnAbmElimi").prop("disabled", true);

        if (tabMn === 1) {
            $("#btnAbmAceptar").prop("disabled", false);
            $("#btnAbmCancelar").prop("disabled", false);
        }
        if (tabMn === 2 && btn === AbmAction.MODIFICACION) {
            //desactivo el ddl
            $("#MenuId").prop("disabled", true);
        }
        $("#btnAbmAceptar").show();
        $("#btnAbmCancelar").show();
    } else if (btn === AbmAction.SUBMIT || btn === AbmAction.CANCEL) {   // (S)uccess - (C)ancel
        $("#btnFiltro").prop("disabled", false);
        $("#btnDetalle").prop("disabled", false);

        $("#BtnLiTab01").prop("disabled", false);
        $("#BtnLiTab02").prop("disabled", false);

        $("#BtnLiTab01").removeClass("text-danger");
        $("#BtnLiTab02").removeClass("text-danger");


        if (btn === AbmAction.ALTA) {

        }
        else if (btn === AbmAction.CANCEL) {

            activarBotones(false);
            activarControles(false);

            if (tabMn === 1) {
                $("#btnDetalle").prop("disabled", true);
                activarGrilla(Grids.GridPerfil);
            }

        }
    }
}


function activarControles(act) {
    //se cambia el sentido del valor act para que se interprete
    //que si se activa los controles sea SI = TRUE y cuando sea False es que se ponen todos los controles en disabled
    if (act === true || act === false) {
        act = !act; //se cambia el sentido del valor ya que con true, se activa el disabled.-
        //se tiene en cuenta que tab es el que esta activo al momento de presionar el boton de acción
        switch (tabMn) {
            case 1:

                //Linea 1
                if (accion === AbmAction.MODIFICACION) {
                    $("#Perfilactivo").prop("disabled", act);
                }
                //Linea 02
                $("#perfil_descripcion").prop("disabled", act);


                break;
            case 2:
                //la clave del barrado no se habilita
                //if (accion02 === AbmAction.ALTA) {
                //    $("#p_id_barrado").prop("disabled", act);
                //}
                //else {
                //    $("#p_id_barrado").prop("disabled", true);
                //}
                //$("#tba_id").prop("disabled", act);
                //$("#p_unidad_pres").prop("disabled", act);
                //$("#p_unidad_x_bulto").prop("disabled", act);
                //$("#p_bulto_x_piso").prop("disabled", act);
                //$("#p_piso_x_pallet").prop("disabled", act);

                break;

            default:
                return false;
        }

    }
}

function ejecutarAlta() {
    AbrirWaiting("Espere, se blanquea el formulario...");

    var data = {};
    switch (tabMn) {
        case 1:
            PostGenHtml(data, nuevoPerfilUrl, function (obj) {
                $("#divpanel01").html(obj);
                ////se procede a buscar la grilla de barrado
                //buscarBarrado(data);
                ////se procede a buscar la grilla de Sucursales
                //buscarLimite(data);

                $("#btnDetalle").prop("disabled", false);
                $("#divFiltro").collapse("hide");
                $("#divDetalle").collapse("show");

                accionBotones(AbmAction.ALTA);
                activarControles(true);

                CerrarWaiting();
            });

            break;
        case 2:

            $("#btnDetalle").prop("disabled", false);
            $("#divFiltro").collapse("hide");
            $("#divDetalle").collapse("show");

            accionBotones(AbmAction.ALTA);
            //activarControles(true);

            CerrarWaiting();
            break;

        default:
            return false;
    }
}

function ejecutarModificacion() {
    if ($("#MenuId option:selected").val() === "") {
        AbrirMensaje("AVISO!!", "ANTES DE REALIZAR LA MODIFICACIÓN DEL MENÚ, SELECCIONE UNO. GRACIAS.", function () {
            $("#msjModal").modal("hide");
        }, false, ["Continuar"], "warn!", null);
        return false;
    }
    $("#divFiltro").collapse("hide");
    accionBotones(AbmAction.MODIFICACION);
    activarControles(true);
    activarArbol("#", true);
}

function ejecutarBaja() {
    switch (tabMn) {
        case 1:
            $("#divFiltro").collapse("hide");
            accionBotones(AbmAction.BAJA);
            break;
        case 2:

            break;
        default:
            return false;
    }
}

function confirmarOperacionCtrlMenu() {
    AbrirWaiting("Completando proceso...");
    var data = {};
    switch (tabMn) {
        case 1:
            data = confirmarDatosTab01();
            break;
        case 2:
            data = confirmarDatosTab02();
            break;

        default:
            return false;
    }
    urlabm = ""
    switch (tabMn) {
        case 1:
            urlabm = confirmarAbmPerfilUrl;
            break;
        case 2:
            urlabm = confirmarMenuPerfilUrl;
            break;

        default:
    }
    PostGen(data, urlabm, function (obj) {
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
            CerrarWaiting();
            AbrirMensaje("ATENCIÓN", obj.msg, function () {
                //todo fue bien, por lo que se deberia reinicializar la pantalla.
                var grilla = Grids.GridPerfil;

                dataBak = "";
                inicializaPantallaCtrlMenu(grilla);

                //siempre sera ALTA
                EntidadSelect = $("#perfil_id").val();

                InicializaFiltroAbmPerfil(EntidadSelect);
                $("#MenuId").prop("disabled", false);
                $("#BtnLiTab01").trigger("click");
                $("#btnBuscar").trigger("click");


                $("#msjModal").modal("hide");
                //return true;

            }, false, ["CONTINUAR"], "succ!", null);
        }
    });
}

function analizaEstadoBtnDetalle() {
    tabMn = 1;
    inicializaPantallaCtrlMenu(Grids.GridPerfil);
    return true;
}

function confirmarDatosTab01() {
    var perfil_id = $("#perfil_id").val();
    var perfil_activo = "N"
    if ($("#Perfilactivo").is(":checked")) { perfil_activo = "S" }
    var perfil_descripcion = $("#perfil_descripcion").val();

    var data = { perfil_id, perfil_activo, perfil_descripcion, accion };
    return data;
}

function confirmarDatosTab02() {
    //var data1 = { menu_id: $("#MenuId option:selected").val(),   perfil_id: $("#perfil_id").val() }
    var json = JSON.stringify($("#menu").jstree(true).get_json());
    var data = { json, menu_id: $("#MenuId option:selected").val(), perfil_id: $("#perfil_id").val() };

    return data;
}

function InicializaFiltroAbmPerfil(id) {
    if ($("#chkDescr").is(":checked")) {
        $("#chkDescr").prop("checked", false);
        $("#Buscar").val("");
    }


    if (!$("#chkDesdeHasta").is(":checked")) {
        $("#chkDesdeHasta").prop("checked", true);
    }
    $("#Id").val(id);
    $("#Id2").val(id);
}

function activarArbol(node, activar) {
    var nodo = $("#menu").jstree().get_node(node);
    if (activar === true) {
        $("#menu").jstree(true).enable_node(nodo);
        nodo.children.forEach(function (child_id) {
            activarArbol(child_id, activar);
        });
    }
    else {
        $("#menu").jstree(true).disable_node(nodo);
        nodo.children.forEach(function (child_id) {
            activarArbol(child_id, activar);
        });
    }
}