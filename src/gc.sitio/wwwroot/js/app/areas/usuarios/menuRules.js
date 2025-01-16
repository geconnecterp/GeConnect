
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
        switch (tabMn) {
            case 1:
                //Linea 1
                ////p_id NUNCA SE ACTIVA
                //if (accion === AbmAction.MODIFICACION) {
                //    $("#p_activo").prop("disabled", act);
                //}
                //$("#up_id").prop("disabled", act);
                ////Linea 02
                //$("#p_m_marca").prop("disabled", act);

                ////SI EL UP_ID ES DISTINTO DE 07 SE PUEDE ACTIVAR EL CHECK DE BALANZA
                //if ($("#up_Id option:selected").val() !== "07") {
                //    $("#PBalanza").prop("disabled", act);
                //    if (act === false && $("#PBalanza").is(":checked")) {
                //        $("#p_balanza_dvto").prop("disabled", act);
                //        $("#p_peso").prop("disabled", act);
                //    }
                //}
                ////Linea 03
                //$("#p_m_desc").prop("disabled", act);
                //$("#PConVto").prop("disabled", act);
                //if (act === false && $("#PConVto").is(":checked")) {
                //    $("#p_con_vto_min").prop("disabled", act);
                //}
                ////Linea 04
                //$("#p_m_capacidad").prop("disabled", act);
                //$("#PAltaRotacion").prop("disabled", act);
                ////Linea 05
                //$("#p_id_prov").prop("disabled", act);
                //$("#PMatPri").prop("disabled", act);
                //if (act === false && $("#PMatPri").is(":checked")) { //este campo se activa cuando el check PMatPri es TRUE
                //    $("#PElaboracion").prop("disabled", act);
                //}
                ////Linea 06
                //$("#cta_lista").prop("disabled", act);
                //$("#AdmMayExcluye").prop("disabled", act);
                //$("#AdmMinExcluye").prop("disabled", act);
                ////Linea 07
                //$("#pg_id").prop("disabled", act);
                //$("#PiAutoExluye").prop("disabled", act);
                //$("#OcAutoExluye").prop("disabled", act);
                ////Linea 08
                //$("#rub_lista").prop("disabled", act);
                //$("#iva_situacion").prop("disabled", act);
                //$("#iva_alicuota").prop("disabled", act);
                //controlaValorIva();
                ////Linea 09
                //$("#lp_id_default").prop("disabled", act);
                //$("#in_alicuota").prop("disabled", act);
                ////Linea 10
                //$("#p_id_barrado_ean").prop("disabled", act);
                //$("#p_unidad_pres_ean").prop("disabled", act);
                //$("#p_unidad_x_bulto_ean").prop("disabled", act);
                //$("#p_bulto_x_piso_ean").prop("disabled", act);
                //$("#p_piso_x_pallet_ean").prop("disabled", act);
                ////Linea 11
                //$("#p_id_barrado_dun").prop("disabled", act);
                //$("#p_unidad_pres_dun").prop("disabled", act);
                //$("#p_unidad_x_bulto_dun").prop("disabled", act);
                //$("#p_bulto_x_piso_dun").prop("disabled", act);
                //$("#p_piso_x_pallet_dun").prop("disabled", act);
                ////Linea 12
                //$("#p_obs").prop("disabled", act);

                ////hacemos el foco
                //if (accion === AbmAction.ALTA) {
                //    $("#up_id").trigger("focus");
                //}

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
            case 3:
                ////solo se puede modificar.
                //$("#adm_id").prop("disabled", true);
                //$("#p_stk_min").prop("disabled", act);
                //$("#p_stk_max").prop("disabled", act);
                break;
            default:
                return false;
        }

    }
}
