$(function () {
    $("#btnContinua01").prop("disabled", true);

    $("#rbBox").on("click", function () { seleccionVista("B"); });
    $("#rbRubro").on("click", function () { seleccionVista("R"); });
    seleccionVista("B"); //la vista arranca con los datos de BOX
});


function seleccionVista(vista) {
    //se debe modificar el texto del span por la vista de datos a presentar
    if (vista === "B") {
        //se debe presenar la grilla de 
        $("#titVista").val("Itinerario por BOX");
        datos = {};
        PostGenHtml(datos, presentarBoxDeProductosUrl, function (obj) {

            $("#divGrids").html(obj);

            CerrarWaiting();
        });
    }
    else {
        $("#titVista").val("Itinerario por RUBRO");
        datos = {};
        PostGenHtml(datos, presentarRubroDeProductosUrl, function (obj) {

            $("#divGrids").html(obj);

            CerrarWaiting();
        });
    }
}

function seleccionarRegistroTIBox(x) {
    InicializaControlesTI02()
    $("#tbAuPend tbody tr").each(function (index) {
        $(this).removeClass("selected-row");
    });

    $(x).addClass("selected-row");
    //presento datos para indicar seleccion
    var reg = parseInt(x.cells[0].innerText.trim());
    $("#txtData01").val($("#rTi" + reg).val());
    $("#txtData02").val(x.cells[2].innerText.trim()); //este input es comodin solo para visualizar. El nombre debo modificarlo
    $("#txtData03").val("");
    $("#txtData04").val("");
    //resguardo datos para mandar en el form
    $("#esbox").val(true);
    $("#esrubro").val(false);
    $("#boxid").val(x.cells[2].innerText.trim());
    $("#rubroid").val("%");

    $("#btnContinua02").prop("disabled", false);
}
function seleccionarRegistroTIRubro(x) {
    InicializaControlesTI02();
    $("#tbAuPend tbody tr").each(function (index) {
        $(this).removeClass("selected-row");
    });
    $(x).addClass("selected-row");


    var reg = parseInt(x.cells[0].innerText.trim());
    $("#txtData01").val($("#rTi" + reg).val());
    $("#txtData02").val($("#rrub_desc" + reg).val());
    $("#txtData03").val($("#rrubg_desc" + reg).val());
    $("#txtData04").val($("#rconteo" + reg).val());

    //resguardo datos para mandar en el form
    $("#esbox").val(false);
    $("#esrubro").val(true);
    $("#rubroid").val($("#rrub_id"+reg).val());
    $("#rubrogid").val($("#rrubg_id"+reg).val());
    $("#boxid").val("%");
    $("#btnContinua02").prop("disabled", false);
}

function InicializaControlesTI02() {
    $("#txtData01").val("");
    $("#txtData02").val("");
    $("#txtData03").val("");
    $("#txtData04").val("");

    $("#esrubro").val(false);
    $("#esbox").val(false);
    $("#boxid").val(null);
    $("#rubroid").val(null);
    $("#rubrogid").val(null);
    
}