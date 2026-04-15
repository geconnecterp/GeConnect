$(function () {
    if ($("#divDetalle").is(":visible")) {
        $("#divDetalle").collapse("hide");
    }
    $("#divFiltro").collapse("show");

    $("#btnCancel").on("click", function () {
        window.location.href = homeCtlValoresUrl;
    });

    $("#lbSucursales").text("Sucursal"); 
    $("#lbDias").text("Día"); 

    $("#btnBuscar").on("click", function () {
        if (validarCamposSeleccionados()) {
            InicializarBusqueda();
        } else {
            AbrirMensaje("ATENCIÓN", "Debe seleccionar Sucursal y Día.", function () {
                $("#msjModal").modal("hide");
                return true;
            }, false, ["Aceptar"], "error!", null);
        }
    });

    $("#chkDias").on("click", function () {
        if ($("#chkDias").is(":checked")) {
            $("#listaDias").prop("disabled", false);
            $("#listaDias").trigger("focus");
        }
        else {
            $("#listaDias").prop("disabled", true).val("");
        }
    });
    $("#chkSucursales").prop("checked", true);
    $("#chkSucursales").prop("disabled", true);
    $("#chkSucursales").trigger('change');
    $("#listaSucursales").prop("disabled", false);

    $(document).on("change", "#listaSucursales", ControlalistaSucursalesSelected);
});

function InicializarBusqueda() {
    var sucDesc = $("#listaSucursales").find("option:selected").text();
    var sucId = $("#listaSucursales").find("option:selected").val();
    var diaId = $("#listaDias").find("option:selected").val();
    var data = { admDesc: sucDesc, admId: sucId, diaId };
    AbrirWaiting("Cargando datos de cierres...");
    PostGenHtml(data, cargarDatosDeCierresUrl, function (html) {

    });
}

function validarCamposSeleccionados() {
    let sucSeleccionada = $("#listaSucursales").val();
    let diaSeleccionado = $("#listaDias").val();
    if (sucSeleccionada == null || sucSeleccionada == undefined || sucSeleccionada == "")
        return false;
    if (diaSeleccionado == null || diaSeleccionado == undefined || diaSeleccionado == "")
        return false;
    return true;
}

function ControlalistaSucursalesSelected() {
    var item = $("#listaSucursales").val();
    var data = { suc_id: item };
    AbrirWaiting("Cargando datos de días...");
    PostGenHtml(data, obtenerDiasPorSucursalUrl, function (html) {
        CerrarWaiting();
        $("#divDetalle").html(html);
        $("#divFiltro").collapse("hide");
        $("#divDetalle").collapse("show");
    });
}