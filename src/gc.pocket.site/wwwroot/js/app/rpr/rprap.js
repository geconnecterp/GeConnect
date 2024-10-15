$(function () {

    $("#btnContinua01").prop("disabled", true);

    $("#btnContinua01").on("click", continuaAutorizacionPendiente);
});

function seleccionaRegistroAP(x) {
    $("#tbAuPend tbody tr").each(function (index) {
        $(this).removeClass("selected-row");
    });
    $(x).addClass("selected-row");
    var reg = parseInt(x.cells[0].innerText.trim());
    autoPendienteSelect = $("#rp" + reg).val();

    $("#btnContinua01").prop("disabled", false);
}

function continuaAutorizacionPendiente() {    
    window.location.href = autoPendienteResguardarUrl + "?rp=" + autoPendienteSelect;
}