var cuentaCCv2 = "";
var tipoCtav2 = "";
var callBackAnalizaInput = "";

$(function () {
    //variables globales del control
    
    $("#cuentaId").on("keyup", analizaInput);
    $("#btnBuscarCCv2").on("click", buscarCuentas);


    //busqueda no gen de proveedores
    $(document).on("keydown.autocomplete", "input#cta_lista", function () {
        $(this).autocomplete({
            source: function (request, response) {
                data = { prefix: request.term }
                $.ajax({
                    url: autoComRel01Url,
                    type: "POST",
                    dataType: "json",
                    data: data,
                    success: function (obj) {
                        response($.map(obj, function (item) {
                            var texto = item.descripcion;
                            return { label: texto, value: item.descripcion, id: item.id };
                        }));
                    }
                })
            },
            minLength: 3,
            select: function (event, ui) {
                AbrirWaiting("Armando combo Familia. Espere...");
                $("input#cta_id").val(ui.item.id);
                var data = { cta_id: ui.item.id };
                PostGen(data, comboFamiliaUrl, function (obj) {
                    if (obj.error === true) {
                        CerrarWaiting();
                        AbrirMensaje("ATENCIÓN", obj.msg, function () {
                            $("#msjModal").modal("hide");
                        }, false, ["Entendido"], "error!", null);
                    }
                    else {
                        //armado del ddl de Familia
                        var combo = $("#pg_id");
                        combo.empty();
                        var opc = "<option value=''>Seleccionar...</option>";
                        combo.append(opc);
                        $.each(obj.lista, function (i, item) {
                            opc = "<option value='" + item.value + "'>" + item.text + "</option>";
                            combo.append(opc);
                        });
                        CerrarWaiting();
                    }
                });

                //var opc = "<option value=" + ui.item.id + ">" + ui.item.value + "</option>"
                //$("#Rel01List").append(opc);
                return true;
            }
        });
    });
});

function analizaInputBC() {
    //se pretende dejar la posibilidad de cargar en el metodo
    //toda aquella funcionalidad que se necesite ejecutar.
    if (callBackAnalizaInput != null) {
        callBackAnalizaInput();
    }
    return true;
}

function cargarTipoCuentaId(tc) {
    if (tc !== "" && tc !== undefined) {
        tipoCtav2 = tc;
    }
    else {
        AbrirMensaje("Atención", "No se especificó el Tipo de Cuenta a ser utilizada en la Busqueda de cuenta. Verifique", function () {
            $("#msjModal").modal("hide");
            return false;
        }, false, ["Aceptar"], "error!", null);
    }
}


function cargarEnCuentaId(cuenta) {
    if (cuenta !== "" && cuenta !== undefined) {
        cuentaCCv2 = cuenta;
        $("#cuentaId").val(cuentaCCv2);
        return true;
    }
    else {
        AbrirMensaje("Atención", "No se recepcionó la cuenta a ser inyectada en la Busqueda de cuenta. Verifique", function () {
            $("#msjModal").modal("hide");
            return false;
        }, false, ["Aceptar"], "error!", null);
    }
    return false;
}

function buscarCuentas() {
    cuentaCCv2 = $("#cuentaId").val();
    if (cuentaCCv2 === "") {
        return false;
    }
    var tipo = tipoCuenta;
    
    var datos = { cuenta, tipo };
    PostGen(datos, busquedacuentaUrl, function (obj) {
        if (obj.error === true) {
            AbrirMensaje("Atención", obj.msg, function () {
                $("#msjModal").modal("hide");
                return true;
            }, false, ["Aceptar"], "error!", null);
        } else if (obj.warn === true) {
            AbrirMensaje("Atención", obj.msg, function () {
                $("#msjModal").modal("hide");
                return true;
            }, false, ["Aceptar"], "warn!", null);
        } else if (obj.unico === true) {
            $("#rrss").val(obj.cuenta.cta_Denominacion);
           /* $("#Cuenta").val(obj.cuenta.cta_Id)         */ 
            return true;
        } else {
            MostrarCuentasModal(obj.cuenta);
        }
    });
}

function MostrarCuentasModal(cuentas) {

}