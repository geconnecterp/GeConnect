$(function () {
    //configuraciones
    $("#lbRel01").text("PROVEEDOR");
    $("#lbRel02").text("RUBRO");

    $("#btnAbmNuevo").on("click", ejecutarAlta);
    $("#btnAbmModif").on("click", ejecutarModificacion);
    $("#btnAbmElimi").on("click", ejecutarBaja);

    $("#btnAbmCancelar").on("click", InicializaPantallaAbmProd);
    $("#btnAbmAceptar").on("click", confirmarOperacionAbmProducto);

    //balanza
    $(document).on("click", "#PBalanza", controlaBalanza);
    $(document).on("click", "#PConVto", controlaVencimiento);
    $(document).on("click", "#PMatPri", controlaMateriaPrima);
    $(document).on("change", "#Up_Id", controlaValorUpId);
    $(document).on("change", "#iva_situacion", controlaValorIva);

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
}

function ejecutarAlta() {

    AbrirWaiting("Espere, se blanquea el formulario...");

    var data = {};
    switch (tabAbm) {
        case 1:
            PostGenHtml(data, nuevoProductoUrl, function (obj) {
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
            accionBotones(AbmAction.ALTA);
            inicializaControlesTab02();
            activarControles(true);
            CerrarWaiting();

            break;
        case 3:
            AbrirMensaje("ATENCIÓN!", "No puede realizar el alta de ningún Límite de Stock de producto. Solo puede Modificarlo.", function () {
                $("#msjModal").modal("hide");
            }, false, ["ACEPTAR"], "warn!", null);
            //accionBotones(AbmAction.ALTA);
            //inicializaControlesTab03();
            //activarControles(true);
            CerrarWaiting();
            break;
        default:
            return false;
    }
}

function controlaBalanza() {
    if ($(this).is(":checked")) {
        $("#P_Balanza_Dvto").prop("disabled", false);
        $("#P_Peso").prop("disabled", false);
    }
    else {
        $("#P_Balanza_Dvto").prop("disabled", true);
        $("#P_Peso").prop("disabled", true);
    }
}

function controlaVencimiento() {
    if ($(this).is(":checked")) {
        $("#P_Con_Vto_Min").prop("disabled", false);
    }
    else {
        $("#P_Con_Vto_Min").prop("disabled", true);
    }
}

function controlaMateriaPrima() {
    if ($(this).is(":checked")) {
        $("#PElaboracion").prop("disabled", false);
    }
    else {
        $("#PElaboracion").prop("disabled", true);
    }
}

function controlaValorUpId() {
    if ($("#Up_Id option:selected").val() !== "07") {
        $("#PBalanza").prop("disabled", false);
    }
    else {
        $("#PBalanza").prop("checked", false);
        $("#PBalanza").prop("disabled", true);
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
        $("#btnAbmNuevo").prop("disabled", false);
        $("#btnAbmModif").prop("disabled", false);
        $("#btnAbmElimi").prop("disabled", false);


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
        }

        $("#btnFiltro").prop("disabled", true);
        $("#btnDetalle").prop("disabled", true);
        $("#BtnLiTab01").prop("disabled", true);
        $("#BtnLiTab02").prop("disabled", true);
        $("#BtnLiTab03").prop("disabled", true);

        $("#btnAbmNuevo").prop("disabled", true);
        $("#btnAbmModif").prop("disabled", true);
        $("#btnAbmElimi").prop("disabled", true);

        $("#btnAbmAceptar").prop("disabled", false);
        $("#btnAbmCancelar").prop("disabled", false);
        $("#btnAbmAceptar").show("slow");
        $("#btnAbmCancelar").show("slow");
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

            if (tabAbm === 1) {
                $("#btnDetalle").prop("disabled", true);
                activarGrilla(tabGrid01);
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
                //p_id NUNCA SE ACTIVA
                if (accion === AbmAction.MODIFICACION) {
                    $("#p_activo").prop("disabled", act);
                }
                $("#up_id").prop("disabled", act);
                //Linea 02
                $("#p_m_marca").prop("disabled", act);

                //SI EL UP_ID ES DISTINTO DE 07 SE PUEDE ACTIVAR EL CHECK DE BALANZA
                if ($("#up_Id option:selected").val() !== "07") {
                    $("#PBalanza").prop("disabled", act);
                    if (act === false && $("#PBalanza").is(":checked")) {
                        $("#p_balanza_dvto").prop("disabled", act);
                        $("#p_peso").prop("disabled", act);
                    }
                }
                //Linea 03
                $("#p_m_desc").prop("disabled", act);
                $("#PConVto").prop("disabled", act);
                if (act === false && $("#PConVto").is(":checked")) {
                    $("#p_con_vto_min").prop("disabled", act);
                }
                //Linea 04
                $("#p_m_capacidad").prop("disabled", act);
                $("#PAltaRotacion").prop("disabled", act);
                //Linea 05
                $("#p_id_prov").prop("disabled", act);
                $("#PMatPri").prop("disabled", act);
                if (act === false && $("#PMatPri").is(":checked")) { //este campo se activa cuando el check PMatPri es TRUE
                    $("#PElaboracion").prop("disabled", act);
                }
                //Linea 06
                $("#cta_lista").prop("disabled", act);
                $("#AdmMayExcluye").prop("disabled", act);
                $("#AdmMinExcluye").prop("disabled", act);
                //Linea 07
                $("#pg_id").prop("disabled", act);
                $("#PiAutoExluye").prop("disabled", act);
                $("#OcAutoExluye").prop("disabled", act);
                //Linea 08
                $("#rub_lista").prop("disabled", act);
                $("#iva_situacion").prop("disabled", act);
                $("#iva_alicuota").prop("disabled", act);
                controlaValorIva();
                //Linea 09
                $("#lp_id_default").prop("disabled", act);
                $("#in_alicuota").prop("disabled", act);
                //Linea 10
                $("#p_id_barrado_ean").prop("disabled", act);
                $("#p_unidad_pres_ean").prop("disabled", act);
                $("#p_unidad_x_bulto_ean").prop("disabled", act);
                $("#p_bulto_x_piso_ean").prop("disabled", act);
                $("#p_piso_x_pallet_ean").prop("disabled", act);
                //Linea 11
                $("#p_id_barrado_dun").prop("disabled", act);
                $("#p_unidad_pres_dun").prop("disabled", act);
                $("#p_unidad_x_bulto_dun").prop("disabled", act);
                $("#p_bulto_x_piso_dun").prop("disabled", act);
                $("#p_piso_x_pallet_dun").prop("disabled", act);
                //Linea 12
                $("#p_obs").prop("disabled", act);
                break;
            case 2:
                //la clave del barrado no se habilita
                if (accion02 === AbmAction.ALTA) {
                    $("#p_id_barrado").prop("disabled", act);
                }
                else {
                    $("#p_id_barrado").prop("disabled", true);
                }
                $("#tba_id").prop("disabled", act);
                $("#p_unidad_pres").prop("disabled", act);
                $("#p_unidad_x_bulto").prop("disabled", act);
                $("#p_bulto_x_piso").prop("disabled", act);
                $("#p_piso_x_pallet").prop("disabled", act);

                break;
            case 3:
                //solo se puede modificar.
                $("#adm_id").prop("disabled", true);
                $("#p_stk_min").prop("disabled", act);
                $("#p_stk_max").prop("disabled", act);
                break;
            default:
                return false;
        }

    }
}

//se debe enviar que operacion se esta confirmando
//enviando todos los campos de la entidad

function confirmarOperacionAbmProducto() {
    AbrirWaiting("Completando proceso...");
    var data = {};
    switch (tabAbm) {
        case 1:
            data = confirmarDatosTab01();
            break;
        case 2:
            data = confirmarDatosTab02();
            break;
        case 3:
            data = confirmarDatosTab03();
            break;
        default:
            return false;
    }
    urlabm = ""
    switch (tabAbm) {
        case 1:
            urlabm = confirmarAbmProductoUrl;
            break;
        case 2:
            urlabm = confirmarAbmBarradoUrl;
            break;
        case 3:
            urlabm = confirmarAbmLimiteUrl;
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
                var grilla = "tbGridProd";
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
                }
                InicializaPantallaAbmProd(grilla);
                $("#msjModal").modal("hide");
                activarGrilla(tabGrid01);
            }, false, ["CONTINUAR"], "succ!", null);
        }
    });
}

function confirmarDatosTab01() {
    //linea 01
    var p_id = $("#p_id").val();
    var p_activo = $("#p_activo option:selected").val();
    var up_id = $("#up_id option:selected").val();
    var up_lista = $("#up_id option:selected").text();
    var up_desc = up_lista.replace("(" + up_id + ")", '');

    //linea 02
    var p_m_marca = $("#p_m_marca").val();
    var p_balanza = "N"
    if ($("#PBalanza").is(":checked")) { p_balanza = "S" }
    var p_balanza_dvto = $("#p_balanza_dvto").val();
    var p_peso = $("#p_peso").val();
    //linea 03
    var p_m_desc = $("#p_m_desc").val();
    var p_desc = $("#p_desc").val();
    var p_con_vto = "N";
    if ($("#PConVto").is(":checked")) { p_con_vto = "S" }
    var p_con_vto_min = $("#p_con_vto_min").val();
    //linea 04
    var p_m_capacidad = $("#p_m_capacidad").val();
    var p_alta_rotacion = "N";
    if ($("#PAltaRotacion").is(":checked")) { p_alta_rotacion = "S" }
    //Linea 05
    var p_id_prov = $("#p_id_prov").val();
    var p_materia_prima = "N";
    if ($("#PMatPri").is(":checked")) { p_materia_prima = "S" }
    var p_elaboracion = "N";
    if ($("#PElaboracion").is(":checked")) { p_elaboracion = "S" }
    //Linea 06
    var cta_id = $("#cta_id").val();
    var cta_lista = $("#cta_lista").text();
    var cta_denominacion = cta_lista.replace("(" + cta_id + ")", '');

    var adm_may_excluye = "N";
    if ($("#AdmMayExcluye").is(":checked")) { adm_may_excluye = "S" }
    var adm_min_excluye = "N";
    if ($("#AdmMinExcluye").is(":checked")) { adm_min_excluye = "S" }
    //Linea 07
    var pg_id = $("#pg_id option:selected").val();
    var pg_lista = $("#pg_id option:selected").text();

    var pi_auto_exluye = "N";
    if ($("#PiAutoExluye").is(":checked")) { pi_auto_exluye = "S" }
    var oc_auto_exluye = "N";
    if ($("#OcAutoExluye").is(":checked")) { oc_auto_exluye = "S" }
    //linea 08
    var rub_id = $("#rub_id").val();
    var rub_lista = $("#rub_lista").text();
    var rub_desc = rub_lista.replace("(" + rub_id + ")", '');

    var iva_situacion = $("#iva_situacion option:selected").val();
    var iva_alicuota = $("#iva_alicuota option:selected").val();
    //linea 09
    var lp_id_default = $("#lp_id_default option:selected").val();
    var in_alicuota = $("#in_alicuota").val();
    //Linea 10
    var p_obs = $("#p_obs").val();
    var p_actu = $("#p_actu").val();  //para que sirve este campo???

    ////contoles hidden
    //var p_alta = $("#p_alta").val();
    //var usu_id_alta = $("#usu_id_alta").val();
    //var p_modi = $("#p_modi").val();
    //var usu_id_modi = $("#usu_id_modi").val();
    var p_balanza_id = $("#p_balanza_id").val();
    var p_id_barrado_ean = $("#p_id_barrado_ean").val();
    var p_unidad_pres_ean = $("#p_unidad_pres_ean").val();
    var p_unidad_x_bulto_ean = $("#p_unidad_x_bulto_ean").val();
    var p_bulto_x_piso_ean = $("#p_bulto_x_piso_ean").val();
    var p_piso_x_pallet_ean = $("#p_piso_x_pallet_ean").val();
    var p_id_barrado_dun = $("#p_id_barrado_dun").val();
    var p_unidad_pres_dun = $("#p_unidad_pres_dun").val();
    var p_unidad_x_bulto_dun = $("#p_unidad_x_bulto_dun").val();
    var p_bulto_x_piso_dun = $("#p_bulto_x_piso_dun").val();
    var p_piso_x_pallet_dun = $("#p_piso_x_pallet_dun").val();

    var data = {
        p_id,//
        p_m_marca,//
        p_m_desc,//
        p_m_capacidad,
        p_desc,
        p_m_capacidad,//
        p_alta_rotacion,//
        p_id_prov,
        p_con_vto,//
        p_con_vto_min,//
        p_balanza_dvto,//
        p_balanza_id,//
        p_peso,//
        p_elaboracion,//
        p_materia_prima,//
        up_id,//
        up_desc,//
        up_lista,//
        rub_id,//
        rub_desc,//
        rub_lista,//
        cta_id,//
        cta_denominacion,//
        cta_lista,//
        pg_id,//
        pg_lista,//
        in_alicuota,//
        iva_alicuota,//
        iva_situacion,//
        p_actu,//
        p_activo,//
        p_balanza,//
        adm_min_excluye,//
        adm_may_excluye,//
        pi_auto_exluye,//
        oc_auto_exluye,//
        p_id_barrado_ean,//
        p_unidad_pres_ean,//
        p_unidad_x_bulto_ean,//
        p_bulto_x_piso_ean,//
        p_piso_x_pallet_ean,//
        p_id_barrado_dun,//
        p_unidad_pres_dun,//
        p_unidad_x_bulto_dun,//
        p_bulto_x_piso_dun,//
        p_piso_x_pallet_dun,//
        lp_id_default,//
        p_obs,//
        accion
    };

    return data;
}

function confirmarDatosTab02() {
    var p_id_barrado = $("#p_id_barrado").val();
    var p_unidad_pres = $("#p_unidad_pres").val();
    var p_unidad_x_bulto = $("#p_unidad_x_bulto").val();
    var p_bulto_x_piso = $("#p_bulto_x_piso").val();
    var p_piso_x_pallet = $("#p_piso_x_pallet").val();
    var tba_id = $("#tba_id").val();
    var tba_desc = $("#tba_id option:selected").text();
    var tba_lista = tba_desc + "(" + tba_id + ")";

    var data = {
        p_id_barrado, p_unidad_pres, p_unidad_x_bulto, p_bulto_x_piso,
        p_piso_x_pallet, tba_id, tba_lista, tba_desc, accion: accion02
    };

    return data;
}

function confirmarDatosTab03() {
    var adm_id = $("#adm_id option:selected").val();
    var adm_nombre = $("#adm_id option:selected").text();
    var adm_lista = adm_nombre + " (" + adm_id + ")";

    var p_stk_min = $("#p_stk_min").val();
    var p_stk_max = $("#p_stk_max").val();

    var data = { adm_id, adm_nombre, adm_lista, p_stk_min, p_stk_max, accion: accion03 };
    return data;
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