$(function () {
    //configuraciones
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