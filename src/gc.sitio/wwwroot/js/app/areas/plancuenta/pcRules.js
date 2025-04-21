$(function () {
    //configuraciones


    $("#btnAbmNuevo").on("click", ejecutarAlta);
    $("#btnAbmModif").on("click", ejecutarModificacion);
    $("#btnAbmElimi").on("click", ejecutarBaja);

    $("#btnAbmCancelar").on("click", cancelarOperacionCuenta);
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
    if (EntidadSelect === undefined || EntidadSelect.ccb_id === "") {
        AbrirMensaje("ATENCIÓN!!", "AÚN NO SE HA SELECCIONADO NINGUNA CUENTA, ANTES DE DAR DE BAJA. VERIFIQUE.",
            function () {
                $("#msjModal").modal("hide");
            }, false, ["ACEPTAR"], "warn!", null);
        return false;
    }

    $("#divFiltro").collapse("hide");
    accionBotones(AbmAction.BAJA);
}

function ejecutarModificacion() {
    if (EntidadSelect === undefined || EntidadSelect.ccb_id === "") {
        AbrirMensaje("ATENCIÓN!!", "AÚN NO SE HA SELECCIONADO NINGUNA CUENTA, ANTES DE MODIFICAR. VERIFIQUE.",
            function () {
                $("#msjModal").modal("hide");
            }, false, ["ACEPTAR"], "warn!", null);
        return false;
    }

    $("#divFiltro").collapse("hide");
    accionBotones(AbmAction.MODIFICACION);
    activarControles(true);
}

function ejecutarAlta() {

    if (EntidadSelect === undefined || EntidadSelect.ccb_id === "") {
        AbrirMensaje("ATENCIÓN!!", "AÚN NO SE HA SELECCIONADO NINGUNA CUENTA. ANTES DE DAR DE ALTA, TIENE QUE INDICAR O SELECCIONAR QUIEN SERA EL PADRE DE LA CUENTA.",
            function () {
                $("#msjModal").modal("hide");
            }, false, ["ACEPTAR"], "warn!", null);
        return false;
    }

    AbrirWaiting("Espere, se inicializa el formulario");

    var data = {};

    PostGen(data, nuevaCuentaUrl, function (obj) {
        CerrarWaiting();
        if (obj.error === true) {
            AbrirMensaje("ALGO NO SALIO BIEN!", obj.msg, function () {
                $("#msjModal").modal("hide");
            }, false, ["CONTINUAR"], "error!", null);
            return false;
        } else if (obj.warn === true) {
            if (obj.auth === true) {
                window.location.href = homepc;
            }
            else {
                AbrirMensaje("ALGO NO SALIO BIEN!", obj.msg, function () {
                    $("#msjModal").modal("hide");
                }, false, ["CONTINUAR"], "warn!", null);
            }
            return false;
        } else {
            $("#divCuentaData").html(obj.cuenta);
            let largo = $("#id_padre").val().trim().length;
            let msg = "Ingrese " + (8 - largo) + " dígitos.";
            $("#ccb_id").attr("maxlength", 8 - largo);
            $("#ccb_id").attr("placeholder", msg);

            $("#btnDetalle").prop("disabled", false);
            $("#divFiltro").collapse("hide");
            $("#divDetalle").collapse("show");

            activarArbol("#divpanel01", "#", false, true);
            accionBotones(AbmAction.ALTA);
            activarControles(true);
        }

    });
    return true;
}

function cancelarOperacionCuenta() {
    if (accion === AbmAction.MODIFICACION && EntidadSelect !== undefined && EntidadSelect !== "") {
        $("#ccb_desc").val(EntidadSelect.ccb_desc);
        if (EntidadSelect.ccb_tipo === 'S') {
            $("#EsMovimiento").prop("checked", false);
        } else {
            $("#EsMovimiento").prop("checked", true);
        }

        if (EntidadSelect.ccb_ajuste_inflacion === 'S') {
            $("#HayAjusteInflacion").prop("checked", true);
        } else {
            $("#HayAjusteInflacion").prop("checked", false);
        }
    }
    activarArbol("#divpanel01", "#", true);
    accionBotones(AbmAction.CANCEL);
    activarControles(false);

    if (accion === AbmAction.MODIFICACION || accion === AbmAction.BAJA) {
        activarBotones(true);
    }
}

function activarBotones(activar) {
    if (activar === true) {
        //el activarlos es activar BM
        $("#btnAbmModif").prop("disabled", false);
        $("#btnAbmNuevo").prop("disabled", false);
        $("#btnAbmElimi").prop("disabled", false);


        $("#btnAbmAceptar").prop("disabled", true);
        $("#btnAbmCancelar").prop("disabled", true);
        $("#btnAbmAceptar").hide();
        $("#btnAbmCancelar").hide();
    }
    else {
        //el alta se activará cuando haya un elemento seleccionado en el arbol
        $("#btnAbmNuevo").prop("disabled", true);
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

        $("#ccb_id").prop("disabled", act);
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

    urlabm = confirmarAbmCuentaUrl;

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
                InicializaPantallaPlanCuenta();


                if (accion !== AbmAction.BAJA) {
                    //se dió de alta o se modificó, se realiza la presentación del producto
                    if (accion === AbmAction.ALTA) {
                        EntidadSelect = obj.id;
                    }
                                    
                    $("#divpanel01").empty();
                    //inicializamos la acción.
                    accion = "";
                }
                else {
                    //borramos el id del producto si se eliminó
                    EntidadSelect = "";
                  
                }                             
                const mainContent = $("main"); // Ajusta el selector según tu estructura HTML
                // Eliminar inert temporalmente
                mainContent.removeAttr("inert");
                // Realizar la acción (por ejemplo, forzar una búsqueda)
                $("#btnBuscar").trigger("click");
                // Restaurar inert
                mainContent.attr("inert", "true");


                $("#msjModal").modal("hide");
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
        ccb_tipo = 'M';
    }

    var ccb_ajuste_inflacion = 'N';
    if ($("#HayAjusteInflacion").is(":checked")) {
        ccb_ajuste_inflacion = 'S';
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

//function activarArbol(div, node, activar, cancelar = false) {
//    var nodo = $(div).jstree().get_node(node);
//    if (activar === true) {
//        $(div).jstree(true).enable_node(nodo);
//        nodo.children.forEach(function (child_id) {
//            activarArbol(div, child_id, activar);
//        });
//    }
//    else {
//        $(div).jstree(true).disable_node(nodo);
//        //al cancelar se debe restituir los valores por defecto
//        if (cancelar === true && nodo.id !== "00" && nodo.id !== "#") {
//            if (nodo.data.asignado === true) {
//                $(div).jstree(true).select_node(nodo);
//            }
//            else {
//                $(div).jstree(true).deselect_node(node);
//            }
//        }
//        nodo.children.forEach(function (child_id) {
//            activarArbol(div, child_id, activar, cancelar);
//        });
//    }
//}