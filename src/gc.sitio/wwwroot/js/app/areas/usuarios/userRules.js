$(function () {
    //configuraciones


    $("#btnAbmNuevo").on("click", ejecutarAlta);
    $("#btnAbmModif").on("click", ejecutarModificacion);
    $("#btnAbmElimi").on("click", ejecutarBaja);

    $("#btnAbmCancelar").on("click", InicializaPantallaUser);
    $("#btnAbmAceptar").on("click", confirmarOperacionAbmUsuario);

    $("#btnDetalle").on("mousedown", analizaEstadoBtnDetalle);
    ////balanza
    //$(document).on("click", "#PBalanza", controlaBalanza);
    //$(document).on("click", "#PConVto", controlaVencimiento);
    //$(document).on("click", "#PMatPri", controlaMateriaPrima);
    //$(document).on("change", "#Up_Id", controlaValorUpId);
    //$(document).on("change", "#iva_situacion", controlaValorIva);

});

function ejecutarBaja() {
    switch (tabAbm) {
        case 1:
        case 2:
            $("#divFiltro").collapse("hide");
            accionBotones(AbmAction.BAJA);
            break;
        case 3:
            AbrirMensaje("ATENCIÓN!", "No puede realizar la Baja de ningún Límite de Stock de producto. Solo puede Modificarlo.", function () {
                $("#msjModal").modal("hide");
            }, false, ["ACEPTAR"], "warn!", null);
            break;
        default:
            return false;
    }
}

function ejecutarModificacion() {
    $("#divFiltro").collapse("hide");
    accionBotones(AbmAction.MODIFICACION);
    activarControles(true);
    switch (tabAbm) {
        case 2:
            activarArbol("#divPerfiles", "#", true)
            break;
        case 3:
            activarArbol("#divAdmins", "#", true)
            break;
        case 4:
            activarArbol("#divDers", "#", true)
            break;
        default:
            break;
    }
}

function ejecutarAlta() {

    AbrirWaiting("Espere, se blanquea el formulario...");

    var data = {};
    switch (tabAbm) {
        case 1:
            PostGenHtml(data, nuevoUsuarioUrl, function (obj) {
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

        default:
            return false;
    }
}

function controlaValorIva() {
    if ($("#iva_situacion option:selected").val() === "N") {
        $("#iva_alicuota").val("0.00");
        $("#iva_alicuota").prop("disabled", true);
    }
    else {
        $("#iva_alicuota").prop("disabled", false);
    }
}

function activarBotones(activar) {
    if (activar === true) {
        //el activarlos es activar BM
        $("#btnAbmModif").prop("disabled", false);
        if (tabAbm === 1) {
            $("#btnAbmNuevo").prop("disabled", false);
            $("#btnAbmElimi").prop("disabled", false);
        }
        else {
            $("#btnAbmNuevo").prop("disabled", true);
            $("#btnAbmElimi").prop("disabled", true);
        }

        $("#btnAbmAceptar").prop("disabled", true);
        $("#btnAbmCancelar").prop("disabled", true);
        $("#btnAbmAceptar").hide();
        $("#btnAbmCancelar").hide();
    }
    else {
        $("#btnAbmNuevo").prop("disabled", false);
        $("#btnAbmModif").prop("disabled", true);
        $("#btnAbmElimi").prop("disabled", true);

        $("#btnAbmAceptar").prop("disabled", true);
        $("#btnAbmCancelar").prop("disabled", true);
        $("#btnAbmAceptar").hide();
        $("#btnAbmCancelar").hide();
    }
}

function accionBotones(btn) {
    if (btn === AbmAction.ALTA ||
        btn === AbmAction.MODIFICACION ||
        btn === AbmAction.BAJA) {
        switch (tabAbm) {
            case 1:
                accion = btn;
                break;
            case 2:
                accion02 = btn;
                break;
            case 3:
                accion03 = btn;
                break;
            case 4:
                accion04 = btn;
                break;
        }

        $("#btnFiltro").prop("disabled", true);
        $("#btnDetalle").prop("disabled", true);
        $("#BtnLiTab01").prop("disabled", true);
        $("#BtnLiTab02").prop("disabled", true);
        $("#BtnLiTab03").prop("disabled", true);
        $("#BtnLiTab04").prop("disabled", true);

        $("#btnAbmNuevo").prop("disabled", true);
        $("#btnAbmModif").prop("disabled", true);
        $("#btnAbmElimi").prop("disabled", true);

        $("#btnAbmAceptar").prop("disabled", false);
        $("#btnAbmCancelar").prop("disabled", false);
        $("#btnAbmAceptar").show();
        $("#btnAbmCancelar").show();
    } else if (btn === AbmAction.SUBMIT || btn === AbmAction.CANCEL) {   // (S)uccess - (C)ancel
        $("#btnFiltro").prop("disabled", false);
        $("#btnDetalle").prop("disabled", false);

        $("#BtnLiTab01").prop("disabled", false);
        $("#BtnLiTab02").prop("disabled", false);
        $("#BtnLiTab03").prop("disabled", false);
        $("#BtnLiTab04").prop("disabled", false);
        $("#BtnLiTab01").removeClass("text-danger");
        $("#BtnLiTab02").removeClass("text-danger");
        $("#BtnLiTab03").removeClass("text-danger");
        $("#BtnLiTab04").removeClass("text-danger");

        if (btn === AbmAction.ALTA) {

        }
        else if (btn === AbmAction.CANCEL) {

            activarBotones(false);
            activarControles(false);
            switch (tabAbm) {
                case 1:
                    $("#btnDetalle").prop("disabled", true);
                    activarGrilla(Grids.GridUser);
                    break;
                case 2:
                    $("#BtnLiTab02").trigger("click");

                    break;
                case 3:
                    $("#BtnLiTab03").trigger("click");
                    break;
                case 4:
                    $("#BtnLiTab04").trigger("click");
                    break;
                default:
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
        switch (tabAbm) {
            case 1:
                //Linea 1
                if (accion == AbmAction.ALTA) {
                    $("#usu_id").prop("disabled", false);
                    $("#usu_bloqueado").prop("disabled", true);
                }
                //Linea 02
                $("#usu_apellidoynombre").prop("disabled", act);

                //Linea 03
                $("#tdoc_id").prop("disabled", act);
                $("#usu_documento").prop("disabled", act);

                //Linea 04
                $("#usu_email").prop("disabled", act);

                //Linea 05
                $("#usu_celu").prop("disabled", act);

                //Linea 06
                $("#cta_denominacion").prop("disabled", act);

                //Linea 07
                if (accion == AbmAction.MODIFICACION) {
                    $("#btnImpCard").prop("disabled", act);
                }


                //hacemos el foco
                if (accion === AbmAction.ALTA) {
                    $("#usu_id").trigger("focus");
                }

                break;
            case 2:

                break;
            case 3:

                break;
            default:
                return false;
        }

    }
}

//se debe enviar que operacion se esta confirmando
//enviando todos los campos de la entidad

function confirmarOperacionAbmUsuario() {
    AbrirWaiting("Completando proceso...");
    var data = {};
    switch (tabAbm) {
        case 1:            
            data = confirmarDatosTab01();
            break;
        case 2:
            accion02 = AbmAction.SUBMIT;
            data = confirmarDatosJsTree("#divPerfiles");
            break;
        case 3:
            accion03 = AbmAction.SUBMIT;
            data = confirmarDatosJsTree("#divAdmins");
            break;
        case 4:
            accion04 = AbmAction.SUBMIT;
            data = confirmarDatosJsTree("#divDers");
            break;
        default:
            return false;
    }
    urlabm = ""
    switch (tabAbm) {
        case 1:
            urlabm = confirmarAbmUsuarioUrl;
            break;
        case 2:
            urlabm = confirmarPerfsUserUrl;
            break;
        case 3:
            urlabm = confirmarAdmsUserUrl;
            break;
        case 4:
            urlabm = confirmarDersUserUrl;
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
                        grilla = Grids.GridUser;
                        dataBak = "";
                        InicializaPantallaUser(grilla);
                        break;

                    default:

                }

                switch (tabAbm) {
                    case 1:
                        if (accion !== AbmAction.BAJA) {
                            //se dió de alta o se modificó, se realiza la presentación del producto
                            if (accion === AbmAction.ALTA) {
                                EntidadSelect = obj.id;
                            }
                            //data = { p_id: EntidadSelect };
                            //buscarProductoServer(data);
                            InicializaFiltroAbmUsuario(EntidadSelect);
                            $("#btnBuscar").trigger("click");
                            $("#divpanel01").empty();
                            //inicializamos la acción.
                            accion = "";
                        }
                        else {
                            //borramos el id del producto si se eliminó
                            EntidadSelect = "";
                            //VAMOS A EJECUTAR NUEVAMENTE EL BUSCAR
                            buscarUsers(pagina);
                        }
                        break;
                    case 2:
                        $("#BtnLiTab02").trigger("click");
                        accionBotones(accion02);
                        break;
                    case 3:
                        $("#BtnLiTab03").trigger("click");
                        accionBotones(accion03);
                        break;
                    case 4:
                        $("#BtnLiTab04").trigger("click");
                        accionBotones(accion04);
                        break;
                    default:
                }




                $("#msjModal").modal("hide");
                //$("#msjModal").toggle();


            }, false, ["CONTINUAR"], "succ!", null);
        }
    });
}

function InicializaFiltroAbmUsuario(id) {
    if ($("#chkDescr").is(":checked")) {
        $("#chkDescr").prop("checked", false);
        $("#Buscar").val("");
    }


    if (!$("#chkDesdeHasta").is(":checked")) {
        $("#chkDesdeHasta").prop("checked", true);
    }
    $("#Id").val(id);
    $("#Id2").val(id);

    //if ($("#chkRel01").is(":checked")) {
    //    $("#chkRel01").prop("checked", false);
    //    $("#Rel01").val("");
    //    $("#Rel01Item").val("");
    //    $("#Rel01List").empty();
    //}

    //if ($("#chkRel02").is(":checked")) {
    //    $("#chkRel02").prop("checked", false);
    //    $("#Rel02").val("");
    //    $("#Rel02Item").val("");
    //    $("#Rel02List").empty();
    //}
}

function confirmarDatosTab01() {
    //linea 01
    var usu_id = $("#usu_id").val();
    var usu_bloqueado = false;
    if ($("#usu_bloqueado").is(":checked")) {
        usu_bloqueado = true;
    }

    //linea 02
    var usu_apellidoynombre = $("#usu_apellidoynombre").val();

    //linea 03
    var tdoc_id = $("#tdoc_id option:selected").val();
    var usu_documento = $("#usu_documento").val();

    //linea 04
    var usu_email = $("#usu_email").val();

    //Linea 05
    var usu_celu = $("#usu_celu").val();

    //Linea 06
    var cta_id = $("#cta_id").val();
    var cta_denominacion = $("#cta_denominacion").val();



    var data = {
        usu_id,//
        usu_bloqueado,//
        usu_apellidoynombre,//
        tdoc_id,
        usu_documento,
        usu_email,//
        usu_celu,//
        cta_id,
        cta_denominacion,//        
        accion
    };

    return data;
}

function confirmarDatosJsTree(div) {

    var json = JSON.stringify($(div).jstree(true).get_json());

    return { json };
}



function inicializaControlesTab02() {
    $("#p_id_barrado").val("");
    $("#p_unidad_pres").val(0);
    $("#p_unidad_x_bulto").val(0);
    $("#p_bulto_x_piso").val(0);
    $("#p_piso_x_pallet").val(0);
    $("#tba_id").val("");

    if (!$("#tab2l1").is(":visible")) {
        $("#tab2l1").show();
        $("#tab2l2").show();
    }
}

function inicializaControlesTab03() {
    $("#adm_id").val("");
    $("#p_stk_min").val(0);
    $("#p_stk_max").val(0);

    if (!$("#tab3l1").is(":visible")) {
        $("#tab3l1").show();
    }
}

function activarArbol(div, node, activar, cancelar = false) {
    var nodo = $(div).jstree().get_node(node);
    if (activar === true) {
        $(div).jstree(true).enable_node(nodo);
        nodo.children.forEach(function (child_id) {
            activarArbol(div, child_id, activar);
        });
    }
    else {
        $(div).jstree(true).disable_node(nodo);
        //al cancelar se debe restituir los valores por defecto
        if (cancelar === true && nodo.id !== "00" && nodo.id !== "#") {
            if (nodo.data.asignado === true) {
                $(div).jstree(true).select_node(nodo);
            }
            else {
                $(div).jstree(true).deselect_node(node);
            }
        }
        nodo.children.forEach(function (child_id) {
            activarArbol(div, child_id, activar, cancelar);
        });
    }
}