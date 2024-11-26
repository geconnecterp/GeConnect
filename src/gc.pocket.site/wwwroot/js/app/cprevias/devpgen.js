function productosGridASTK() {
    var data = {};
    PostGenHtml(data, PresentarProductosSeleccionadosUrl, function (obj) {
        $("#divProdCargGrid").html(obj);
        var tb = $("#divProdCargGrid #tbProdCarg tbody td");
        if (tb.length <= 0) {
            $("#btnContinua01").hide("fast");
        } else {
            $("#btnContinua01").show("fast");
        }

        //if (typeof ocultarTrash !== 'undefined') {
        //    if (ocultarTrash === true) {
        //        //ocultamos la 8° columna
        //        $(".ocultar").toggle();
        //        $("#divRprGrid #tbProdRPR tbody td:nth-child(8)").toggle();
        //    }
        //}

        return true;
    }, function (obj) {
        ControlaMensajeError(obj.message);
        return true;
    });
}

function controlDeSigno() {
    if ($("#chkSigno").is(":checked")) {
        $("#lbSigno").removeClass("text-danger").addClass("text-success").empty();
        $("#lbSigno").html("<i class=\"bx bx-plus bx-lg\"></i>");
    }
    else {
        $("#lbSigno").removeClass("text-success").addClass("text-danger").empty();
        $("#lbSigno").html("<i class=\"bx bx-minus bx-lg\"></i>");
    }
}