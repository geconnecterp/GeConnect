$(function () {
    $("#btnAbmNuevo").on("click", ejecutarAlta);
    $("#btnAbmModif").on("click", ejecutarModificacion);
    $("#btnAbmElimi").on("click", ejecutarBaja);

    $("#btnAbmCancelar").on("click", inicializaPantallaCtrlMenu);
    $("#btnAbmAceptar").on("click", confirmarOperacionAbmProducto);

    $("#btnDetalle").on("mousedown", analizaEstadoBtnDetalle);

    $("#MenuId").on("change", buscarMenu);

    $('#menu').on('changed.jstree', function (e, data) {

        if (data.selected && data.selected.length > 0) {
            data.selected.forEach(function (id) {
                updateAssignedState(id, true); // Marcar como asignado
            });
        }

        if (data.deselected && data.deselected.length > 0 ) {
            data.deselected.forEach(function (id) {
                updateAssignedState(id, false); // Marcar como no asignado
            });
        }
    });
});

// Función para actualizar el estado de 'asignado'
function updateAssignedState(id, state) {
    // Buscar el nodo en el menú y actualizar su estado
    jsonMenuActual.forEach(function (item) {
        if (item.id === id) {
            item.asignado = state;
        }
        if (item.children) {
            item.children.forEach(function (child) {
                if (child.id === id) {
                    child.asignado = state;
                }
            });
        }
    });
}

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
        $("#BtnLiTab03").prop("disabled", true);

        $("#btnAbmNuevo").prop("disabled", true);
        $("#btnAbmModif").prop("disabled", true);
        $("#btnAbmElimi").prop("disabled", true);

        if (tabMn === 1) {
            $("#btnAbmAceptar").prop("disabled", false);
            $("#btnAbmCancelar").prop("disabled", false);
        }
        $("#btnAbmAceptar").show();
        $("#btnAbmCancelar").show();
    } else if (btn === AbmAction.SUBMIT || btn === AbmAction.CANCEL) {   // (S)uccess - (C)ancel
        $("#btnFiltro").prop("disabled", false);
        $("#btnDetalle").prop("disabled", false);

        $("#BtnLiTab01").prop("disabled", false);
        $("#BtnLiTab02").prop("disabled", false);
        $("#BtnLiTab03").prop("disabled", false);
        $("#BtnLiTab01").removeClass("text-danger");
        $("#BtnLiTab02").removeClass("text-danger");
        $("#BtnLiTab03").removeClass("text-danger");

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
    $("#divFiltro").collapse("hide");
    accionBotones(AbmAction.MODIFICACION);
    activarControles(true);
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

function confirmarOperacionAbmProducto() {
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
                var grilla = "";
                switch (tabAbm) {
                    case 1:
                        grilla = tabGrid01;
                        break;
                    case 2:
                        grilla = tabGrid02;
                        break;
                    case 3:
                        grilla = tabGrid03;
                        break;
                    default:
                        return false;
                }
                dataBak = "";
                InicializaPantallaAbmProd(grilla);
                //if (tabAbm === 2 || tabAbm === 3) {
                //    activarGrilla(tabGrid01);
                //}
                if (accion !== AbmAction.BAJA) {
                    switch (tabAbm) {
                        case 1:
                            //se dió de alta o se modificó, se realiza la presentación del producto
                            if (accion === AbmAction.ALTA) {
                                EntidadSelect = obj.id;
                            }
                            //data = { p_id: EntidadSelect };
                            //buscarProductoServer(data);
                            InicializaFiltroAbmPerfil(EntidadSelect);
                            $("#btnBuscar").trigger("click");


                            break;
                        case 2:
                            presentarBarrado();
                            break;
                        case 3:
                            presentarLimites();
                        default:
                    }

                    //inicializamos la acción.
                    accion = "";
                }
                else {
                    //borramos el id del producto si se eliminó
                    EntidadSelect = "";
                    //VAMOS A EJECUTAR NUEVAMENTE EL BUSCAR
                    buscarProductos(pagina);

                }
                
                $("#msjModal").modal("hide");
                return true;

            }, false, ["CONTINUAR"], "succ!", null);
        }
    });
}

function analizaEstadoBtnDetalle() {
    var res = $("#divDetalle").hasClass("show");
    if (res === true) {
        selectRegPerfil(regSelected, Grids.GridPerfil);
    }
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
    var json = JSON.stringify(jsonMenuActual);
    var data = { json, menu_id: $("#MenuId option:selected").val(), perfil_id: $("#perfil_id").val() };

    return data;
}