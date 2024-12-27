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
        $("#P_Activo").prop("disabled", act);
        $("#Up_Id").prop("disabled", act);
        //Linea 02
        $("#P_M_Marca").prop("disabled", act);

        //SI EL UP_ID ES DISTINTO DE 07 SE PUEDE ACTIVAR EL CHECK DE BALANZA
        if ($("#Up_Id option:selected").val() !== "07") {
            $("#PBalanza").prop("disabled", act);
            if (act === false && $("#PBalanza").is(":checked")) {
                $("#P_Balanza_Dvto").prop("disabled", act);
                $("#P_Peso").prop("disabled", act);
            }
        }
        //Linea 03
        $("#P_Desc").prop("disabled", act);
        $("#PConVto").prop("disabled", act);
        if (act === false && $("#PConVto").is(":checked")) {
            $("#P_Con_Vto_Min").prop("disabled", act);
        }
        //Linea 04
        $("#P_M_Capacidad").prop("disabled", act);
        $("#PAltaRotacion").prop("disabled", act);
        //Linea 05
        $("#P_Id_Prov").prop("disabled", act);
        $("#PMatPri").prop("disabled", act);
        if (act === false && $("#PMatPri").is(":checked")) { //este campo se activa cuando el check PMatPri es TRUE
            $("#PElaboracion").prop("disabled", act);
        }
        //Linea 06
        $("#Cta_Lista").prop("disabled", act);
        $("#AdmMayExcluye").prop("disabled", act);
        $("#AdmMinExcluye").prop("disabled", act);
        //Linea 07
        $("#Pg_Id").prop("disabled", act);
        $("#PiAutoExluye").prop("disabled", act);
        $("#OcAutoExluye").prop("disabled", act);
        //Linea 08
        $("#Rub_Lista").prop("disabled", act);
        $("#Iva_Situacion").prop("disabled", act);
        $("#Iva_Alicuota").prop("disabled", act);
        //Linea 09
        $("#Lp_Id_Default").prop("disabled", act);
        $("#In_Alicuota").prop("disabled", act);
        //Linea 10
        $("#P_Obs").prop("disabled", act);
    }
}

function confirmarOperacionAbmProducto() {

}