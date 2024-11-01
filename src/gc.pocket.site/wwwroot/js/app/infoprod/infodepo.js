$(function () {
    //cada vez que cambie el radiobutton agrupado en concepto, se asignará el valor en la variable global "concepto";
    $("input[name='concepto']").on("change", function () {
        concepto = $("input[name='concepto']:checked").val();
        buscarDepositoStkVal();
    });

    $("#btnBox").on("click", buscarboxlibres);
    $("#chkSoloLibres").on("change", buscarboxlibres);

    $("#btnStkD").on("click", buscarDepositoStk);
    $("#btnStkVal").on("click", buscarDepositoStkVal);


    buscarboxlibres();
});

function buscarboxlibres() {
    AbrirWaiting("Buscando información de BOX para presentar...");
    var soloLibres = true;
    if (!$("#chkSoloLibres").is(":checked")) {
        soloLibres = false;
    }
    var depo_id = $("#DepoId").find(":selected").val();
    var dato = {depo_id, soloLibres };
    PostGenHtml(dato, buscarBoxLibres, function (obj) {
        $("#gridBoxFree").html(obj);
        CerrarWaiting();
        return true;
    });
    return true;
}

function buscarDepositoStk() {
    AbrirWaiting("Buscando productos del Deposito para presentar...");
    var depo_id = $("#DepoId").find(":selected").val();
    var dato = {depo_id};
    PostGenHtml(dato, buscarDepoStk, function (obj) {
        $("#gridStkD").html(obj);
        CerrarWaiting();
        return true;
    });
    return true;
}

function buscarDepositoStkVal() {
    AbrirWaiting("Buscando información del STK Valorizado para presentar...");
    var depo_id = $("#DepoId").find(":selected").val();

    var dato = {depo_id, concepto };
    PostGenHtml(dato, buscarDepoStkVal, function (obj) {
        $("#gridStkVal").html(obj);
        CerrarWaiting();
        return true;
    });
    return true;
}