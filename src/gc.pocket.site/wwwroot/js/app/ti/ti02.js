$(function () {
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
            if (obj.ok === true) {
                $("#divGrids").html(obj.GrillaDatos);
            }
            else if (obj.esError === true) {
                //mensaje
                AbrirMensaje("Algo pasó", obj.mensaje, function () {
                    $("#msjModal").modal("hide");
                    return true;
                }, false, ["Aceptar"], "error!", null);
            } else {
                //warn
                AbrirMensaje("Algo pasó", obj.mensaje, function () {
                    $("#msjModal").modal("hide");
                    return true;
                }, false, ["Aceptar"], "warn!", null);
            }
            CerrarWaiting();
        });
    }
    else {
        $("#titVista").val("Itinerario por RUBRO");
        datos = {};
        PostGenHtml(datos, presentarRubroDeProductosUrl, function (obj) {
            if (obj.ok === true) {
                $("#divGrids").html(obj.GrillaDatos);
            }
            else if (obj.esError === true) {
                //mensaje
                AbrirMensaje("Algo pasó", obj.mensaje, function () {
                    $("#msjModal").modal("hide");
                    return true;
                }, false, ["Aceptar"], "error!", null);
            } else {
                //warn
                AbrirMensaje("Algo pasó", obj.mensaje, function () {
                    $("#msjModal").modal("hide");
                    return true;
                }, false, ["Aceptar"], "warn!", null);
            }
            CerrarWaiting();
        });
    }
}

function seleccionarRegistroTIBox(x) {
    $("#tbAuPend tbody tr").each(function (index) {
        $(this).removeClass(".selected-row");
    });
    $(this).addClass(".selected-row");
}
function seleccionarRegistroTIRubro(x) {
    $("#tbAuPend tbody tr").each(function (index) {
        $(this).removeClass(".selected-row");
    });
    $(this).addClass(".selected-row");
}