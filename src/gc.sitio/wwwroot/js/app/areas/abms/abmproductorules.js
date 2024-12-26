$(function () {
    //configuraciones
    //$("#btnNuevo").on("click", function () {
    //    AbrirWaiting("Espere, se blanquea el formulario...");


    //    accionBotones("N");
    //    activarControles(true);

    //    CerrarWaiting();        
    //});


    $("#pMatPri").on("click", function () {
        if ($(this).is(":checked")) {
            $("#pElabora").prop("disabled", false);
            $("#pElabora").prop("checked", false);
        }
    })


    $("input#pBalanza").on("change", function () {
        if ($(this).is(":checked")) {
            $("input#P_Balanza_Dvto").prop("disabled", false);
            $("input#P_Peso").prop("disabled", false);
        }
        else {
            $("input#P_Balanza_Dvto").prop("disabled", true);
            $("input#P_Peso").prop("disabled", true);
        }
    });
});

function activarBotones(activar) {
    if (activar === true) {
        $("#btnAbmNuevo").prop("disabled", false);
        $("#btnAbmModif").prop("disabled", false);
        $("#btnAbmElimi").prop("disabled", false);

        $("#btnAbmAceptar").prop("disabled", true);
        $("#btnAbmCancelar").prop("disabled", true);
    }
    else {
        $("#btnAbmNuevo").prop("disabled", true);
        $("#btnAbmModif").prop("disabled", true);
        $("#btnAbmElimi").prop("disabled", true);

        $("#btnAbmAceptar").prop("disabled", true);
        $("#btnAbmCancelar").prop("disabled", true);
    }
}

function accionBotones(btn) {
    if (btn === 'N' || btn === 'E' || btn === 'D') {
        $("#btnAbmAceptar").prop("disabled", false);
        $("#btnAbmCancelar").prop("disabled", false);

        $("#btnAbmNuevo").prop("disabled", true);
        $("#btnAbmModif").prop("disabled", true);
        $("#btnAbmElimi").prop("disabled", true);
    } else if (btn === 'A' || btn === 'C') {
        $("#btnAbmAceptar").prop("disabled", true);
        $("#btnAbmCancelar").prop("disabled", true);

        $("#btnAbmNuevo").prop("disabled", false);
        $("#btnAbmModif").prop("disabled", false);
        $("#btnAbmElimi").prop("disabled", false);
    }
}

function activarControles(act) {
    if (act === true || act === false) {
        //Linea 1
        //p_id NUNCA SE ACTIVA
        $("#P_Activo").prop("disabled", act);
        $("#Up_Id").prop("disabled", act);
        //Linea 02
        $("#P_M_Marca").prop("disabled", act);
        $("#PBalanza").prop("disabled", act);
        $("#P_Balanza_Dvto").prop("disabled", act);
        $("#P_Peso").prop("disabled", act);
        //Linea 03
        $("#P_Desc").prop("disabled", act);
        $("#PConVto").prop("disabled", act);
        $("#P_Con_Vto_Min").prop("disabled", act);
        //Linea 04
        $("#P_M_Capacidad").prop("disabled", act);
        $("#PAltaRotacion").prop("disabled", act);
        //Linea 05
        $("#P_Id_Prov").prop("disabled", act);
        $("#PMatPri").prop("disabled", act);
        if (act === false) { //este campo se activa cuando el check PMatPri es TRUE
            $("#PElaboracion").prop("disabled", act);
        }
        //Linea 06
        $("#Cta_Lista").prop("disabled", act);
        $("#AdmMayExcluye").prop("disabled", act);
        $("#AdmMinExcluye").prop("disabled", act);
        //Linea 07
        $("#Pg_Id").prop("disabled", act);
        $("#PiAutoExluye").prop("disabled", act);
        //$("#LpIdDefault").prop("disabled", act);
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