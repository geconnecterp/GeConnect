var caja_nro_proceso_selected = null;
var caja_nro_cierre_selected = null;
var caja_id_selected = null;
var cierre_pendientes_bool = null;

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
    var data = { admDesc: sucDesc, admId: sucId, nroProceso: diaId };
    AbrirWaiting("Cargando datos de cierres...");
    PostGenHtml(data, cargarDatosDeCierresUrl, function (html) {
        $("#divDetalle").html(html);
        $("#divFiltro").collapse("hide");
        $("#divDetalle").collapse("show");
        InicializaEventosGrillaVtasPVCtlCierres();
        CerrarWaiting();
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
        $("#divListaDias").html(html);
        $("#divDetalle").empty();
    });
}

function InicializaEventosGrillaVtasPVCtlCierres() {
    $(document).off("click", "#tbVtasPVCtlCierres tbody tr");
    $(document).on("click", "#tbVtasPVCtlCierres tbody tr", function (e) {

        if (!$(e.target).is("button, a, .btn, i")) {

            const $this = $(this);

            // Quitar selección previa
            $("#tbVtasPVCtlCierres tbody tr").removeClass("selected-row");

            // Marcar fila seleccionada
            $this.addClass("selected-row");

            // Guardar valor seleccionado
            caja_nro_proceso_selected = $this.data("caja-nro-proceso");
            caja_nro_cierre_selected = $this.data("caja-nro-cierre");
			caja_id_selected = $this.data("caja-id");   
            cierre_pendientes_bool = $this.data("pendientes-bool");   

            if (cierre_pendientes_bool === true || cierre_pendientes_bool === "true" || cierre_pendientes_bool === "True") {
                $("#btnConfirmacionContable").prop("disabled", false);
            } else {
                $("#btnConfirmacionContable").prop("disabled", true);
            }

            // Habilitar 
            if (caja_nro_proceso_selected) {
                CargarGrillaVtasPVCtlRend();
            }
        }
    });

    $("#btnConfirmacionContable").prop("disabled", true);
}

function CargarGrillaVtasPVCtlRend() {
    if (!validarCierreSeleccionado()) {
        AbrirMensaje("ATENCIÓN", "Debe seleccionar un Cierre.", function () {
            $("#msjModal").modal("hide");
            return;
        }, false, ["Aceptar"], "error!", null);
    }
    else {
        var data = { nro_proceso: caja_nro_proceso_selected, nro_cierre: caja_nro_cierre_selected };
        AbrirWaiting("Cargando datos de rendición de Cierre seleccionado...");
        PostGenHtml(data, obtenerRendDeCierreSeleccionadoUrl, function (html) {
            CerrarWaiting();
            $("#divVtasPVCtlRend").html(html);
            InicializaEventosGrillaVtasPVCtlRend();
        });
    }
}

function InicializaEventosGrillaVtasPVCtlRend() {
    $(document).off("click", "#tbVtasPVCtlRend tbody tr");
    $(document).on("click", "#tbVtasPVCtlRend tbody tr", function (e) {

        if (!$(e.target).is("button, a, .btn, i")) {

            const $this = $(this);

            // Quitar selección previa
            $("#tbVtasPVCtlRend tbody tr").removeClass("selected-row");

            // Marcar fila seleccionada
            $this.addClass("selected-row");

            // Guardar valor seleccionado
            caja_nro_proceso_selected = $this.data("caja-nro-proceso");
            caja_nro_cierre_selected = $this.data("caja-nro-cierre");
            caja_id_selected = $this.data("caja-id");
            cierre_pendientes_bool = $this.data("pendientes-bool");

            if (cierre_pendientes_bool === true || cierre_pendientes_bool === "true" || cierre_pendientes_bool === "True") {
                $("#btnConfirmacionContable").prop("disabled", false);
            } else {
                $("#btnConfirmacionContable").prop("disabled", true);
            }

            // Habilitar 
            if (caja_nro_proceso_selected) {
                CargarGrillaVtasPVCtlRend();
            }
        }
    });
}

function validarCierreSeleccionado() {
    if (caja_nro_proceso_selected == null || caja_nro_proceso_selected == undefined || caja_nro_proceso_selected == "")
        return false;
    if (caja_nro_cierre_selected == null || caja_nro_cierre_selected == undefined || caja_nro_cierre_selected == "")
        return false;
    return true;
}