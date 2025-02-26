var cuentaCCv2 = "";
var tipoCtav2 = "";
var callBackAnalizaInput = "";

$(function () {
    //variables globales del control
    
    $("#cuentaId").on("keyup", analizaInput);
    $("#btnBuscarCCv2").on("click", buscarCuentas);

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