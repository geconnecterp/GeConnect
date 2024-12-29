$(function () {
    //configuraciones
    $("#btnAbmNuevo").on("click", function () {
        AbrirWaiting("Espere, se blanquea el formulario...");

        var data = {};
        PostGenHtml(data, nuevoProductoUrl, function (obj) {
            $("#divpanel01").html(obj);
            ////se procede a buscar la grilla de barrado
            //buscarBarrado(data);
            ////se procede a buscar la grilla de Sucursales
            //buscarLimite(data);

            $("#btnDetalle").prop("disabled", false);
            $("#divFiltro").collapse("hide");
            $("#divDetalle").collapse("show");

            accionBotones("A");
            activarControles(true);

            CerrarWaiting();
        });
    });

    $("#btnAbmModif").on("click", function () {
        $("#divFiltro").collapse("hide");
        accionBotones("M");
        activarControles(true);
    });

    $("#btnAbmCancelar").on("click", InicializaPantallaAbmProd);
    $("#btnAbmAceptar").on("click", confirmarOperacionAbmProducto);

    //balanza
    $(document).on("click", "#PBalanza", controlaBalanza);
    $(document).on("click", "#PConVto", controlaVencimiento);
    $(document).on("click", "#PMatPri", controlaMateriaPrima);
    $(document).on("change", "#Up_Id", controlaValorUpId);


});

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

function activarBotones(activar) {
    if (activar === true) {
        //el activarlos es activar BM
        $("#btnAbmNuevo").prop("disabled", true);
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
    if (btn === 'A' || btn === 'M' || btn === 'B') {
        accion = btn;
        $("#btnFiltro").prop("disabled", true);

        $("#btnAbmNuevo").prop("disabled", true);
        $("#btnAbmModif").prop("disabled", true);
        $("#btnAbmElimi").prop("disabled", true);

        $("#btnAbmAceptar").prop("disabled", false);
        $("#btnAbmCancelar").prop("disabled", false);
        $("#btnAbmAceptar").show("slow");
        $("#btnAbmCancelar").show("slow");
    } else if (btn === 'S' || btn === 'C') {   // (S)uccess - (C)ancel
        $("#btnFiltro").prop("disabled", false);
        if (btn === 'S') {

        }
        else if (btn === 'C') {
            $("#btnDetalle").prop("disabled", true);
            activarBotones(false);
        }
        //$("#btnAbmNuevo").prop("disabled", false);
        //$("#btnAbmModif").prop("disabled", true);
        //$("#btnAbmElimi").prop("disabled", true);

        //$("#btnAbmAceptar").prop("disabled", true);
        //$("#btnAbmCancelar").prop("disabled", true);
        //$("#btnAbmAceptar").hide("slow");
        //$("#btnAbmCancelar").hide("slow");
    }
}

function activarControles(act) {
    //se cambia el sentido del valor act para que se interprete
    //que si se activa los controles sea SI = TRUE y cuando sea False es que se ponen todos los controles en disabled
    if (act === true || act === false) {
        act = !act; //se cambia el sentido del valor ya que con true, se activa el disabled.-
        //Linea 1
        //p_id NUNCA SE ACTIVA
        if (accion === 'M') {
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
        //Linea 09
        $("#lp_id_default").prop("disabled", act);
        $("#in_alicuota").prop("disabled", act);
        //Linea 10
        $("#p_obs").prop("disabled", act);
    }
}

//se debe enviar que operacion se esta confirmando
//enviando todos los campos de la entidad

function confirmarOperacionAbmProducto() {
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

    //contoles hidden
    var p_alta = $("#p_alta").val();
    var usu_id_alta = $("#usu_id_alta").val();
    var p_modi = $("#p_modi").val();
    var usu_id_modi = $("#usu_id_modi").val();
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
        p_id,
        p_m_marca,
        p_m_desc,
        p_m_capacidad,
        p_desc,
        p_alta_rotacion,
        p_con_vto,
        p_con_vto_min,
        p_peso,
        p_elaboracion,
        p_materia_prima,
        up_id,
        up_desc,
        up_lista,
        rub_id,
        rub_desc,
        rub_lista,
        cta_id,
        cta_denominacion,
        cta_lista,
        pg_id,
        pg_lista,
        in_alicuota,
        iva_alicuota,
        iva_situacion,
        p_modi,
        p_actu,
        p_activo,
        p_balanza,
        adm_min_excluye,
        adm_may_excluye,
        pi_auto_exluye,
        oc_auto_exluye,
        p_id_barrado_ean,
        p_unidad_pres_ean,
        p_unidad_x_bulto_ean,
        p_bulto_x_piso_ean,
        p_piso_x_pallet_ean,
        p_id_barrado_dun,
        p_unidad_pres_dun,
        p_unidad_x_bulto_dun,
        p_bulto_x_piso_dun,
        p_piso_x_pallet_dun,
        lp_id_default,
        accion
    };
    AbrirWaiting("Completando proceso...");
    PostGen(data, confirmarAbmProductoUrl, function (obj) {
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
            CerrarWaiting();
            AbrirMensaje("ATENCIÓN", obj.msg, function () {
                //todo fue bien, por lo que se deberia reinicializar la pantalla.
                InicializaPantallaAbmProd();
                $("#msjModal").modal("hide");
            }, false, ["CONTINUAR"], "succ!", null);
        }
    });
}