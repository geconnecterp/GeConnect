$(function () {
    //configuraciones


    $("#btnAbmNuevo").on("click", ejecutarAlta);
    $("#btnAbmModif").on("click", ejecutarModificacion);
    $("#btnAbmElimi").on("click", ejecutarBaja);

    $("#btnAbmCancelar").on("click", InicializaPantallaPlanCuenta);
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
    $("#divFiltro").collapse("hide");
    accionBotones(AbmAction.BAJA);
}

function ejecutarModificacion() {
    $("#divFiltro").collapse("hide");
    accionBotones(AbmAction.MODIFICACION);
    activarControles(true);
}

function ejecutarAlta() {

    AbrirWaiting("Espere, se inicializa el formulario");

    var data = {};

    PostGenHtml(data, nuevaCuentaUrl, function (obj) {
        $("#divpanel01").html(obj);

        $("#btnDetalle").prop("disabled", false);
        $("#divFiltro").collapse("hide");
        $("#divDetalle").collapse("show");

        accionBotones(AbmAction.ALTA);
        activarControles(true);

        CerrarWaiting();
    });

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
        //especificamos que Accion vamos a realizar.
        accion = btn;

        $("#btnFiltro").prop("disabled", true);
        $("#btnDetalle").prop("disabled", true);

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

        //Linea 1
        if (accion == AbmAction.ALTA) {
            $("#id_padre").show();
        }
        else {
            $("#id_padre").hide();
        }

        $("#ccb_id").prop("disabled", false);
        //Linea 02
        $("#ccb_desc").prop("disabled", act);

        //Linea 03 - siempre estara desactivado.
        //$("#ccb_id_padre").prop("disabled", act); 

        //Linea 04
        $("#HayAjusteInflacion").prop("disabled", act);

        //Linea 05
        $("#EsMovimiento").prop("disabled", act);


        //hacemos el foco
        if (accion === AbmAction.ALTA || accion === AbmAction.MODIFICACION) {
            $("#ccb_id").trigger("focus");
        }
    }
}

//se debe enviar que operacion se esta confirmando
//enviando todos los campos de la entidad

function confirmarOperacionAbmUsuario() {
    AbrirWaiting("Completando proceso...");
    var data = confirmarDatosTab01();
           
    urlabm =  confirmarAbmCuentaUrl;
            
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
                            buscarPlanCuenta(pagina);
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
    var ccb_id = $("#ccb_id").val();
    if (ccb_id === "") {
        AbrirMensaje("ALERTA", "El campo ID de la Cuenta es obligatorio", function () {
            $("#msjModal").modal("hide");
        }, false, ["CONTINUAR"], "error!", null);
        return false;
    }
    if (accion === AbmAction.ALTA) {
        var id_padre = $("#id_padre").val();
        ccb_id = id_padre + ccb_id.trim();        
    }
    var ccb_desc = $("#ccb_desc").val();
    var ccb_id_padre = $("#ccb_id_padre").val();
    var ccb_tipo = 'S';
    if ($("#EsMovimiento").is(":checked")) {
        usu_bloqueado = 'M';
    }

    var ccb_ajuste_inflacion = 'N';
    if ($("#HayAjusteInflacion").is(":checked")) {
        usu_bloqueado = 'S';
    }   

    var data = {
        ccb_id,//
        ccb_desc,//
        ccb_lista: ccb_desc + " (" + ccb_id + ")",//
        ccb_id_padre,//
        ccb_tipo,
        ccb_ajuste_inflacion,                
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