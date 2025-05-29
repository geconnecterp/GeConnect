/**
 * Seleccion de Valores
 * 
 * Este módulo se encarga de la seleccion de valores financieros
 */

$(function () {
    $(document).on("click", "#btnAceptarAgregarValor", btnAceptarAgregarValorValidar);
    //btnAceptarAgregarValor
});

function btnAceptarAgregarValorValidar() {
    console.log("btnAceptarAgregarValorValidar");
}

/*
p:
{ app, importe, valor_a_nombre_de, valores }
Donde:
    - app: Identificador del tipo de cuenta financiera.
    - importe: Importe saldo sugerido, este monto si es mayor que cero debe ser sugerido por la app al momento en que se seleccione un valor de una cuenta financiera
    - valor_a_nombre_de: es una ocurrencia string que debemos utilizar cuando se emiten cheques.
    - valores: datos ya cargados en origen, con la idea de hacer algún control.
*/
function invocarModalDeSeleccionDeValores(p) {
	var app = p.app;
    var data = { app };
    PostGenHtml(data, abrirComponenteDeSeleccionDeValoresUrl, function (obj) {
        $("#modalSeleccionValores").html(obj);
        $("#modalSeleccionValores").show();
        $("#seleccionDeValoresModal").modal("show");
    });
}

function seleccionarTipoFin(x) {
    seleccionarGrilla(x, 'tbTipoCuentaFin');
    var tcf_id = x.cells[1].innerText.trim();
    var data = { tcf_id };
    PostGenHtml(data, cargarCtaFinParaSeleccionDeValoresUrl, function (obj) {
        $("#divFinancieros").html(obj);
    });
    PostGenHtml(data, cargarSeccionEdicionEnSeleccionDeValoresUrl, function (obj) {
        $("#divSeccionEditable").html(obj);
    });
}

function seleccionarFinanciero(x) {
    var ctaf_id = x.cells[2].innerText.trim();
    var data = { ctaf_id };

    //TODO MARCE: Seguir aca, al seleccionar un elemento de la tabla financiero, debo actualizar el modelo de la seccion con los datos de ctaf_id, ctaf_deno y ban_razon_social
    //            para poder tenerlos y luego validarlos y armar, luego, el registro para almacenar
    seleccionarGrilla(x, 'tbFinanciero');
}

function seleccionarValoresEnCartera(x) {
    seleccionarGrilla(x, 'tbValoresEnCartera');
}

function seleccionarGrilla(x, grilla) {
    $("#" + grilla + " tbody tr").each(function (index) {
        $(this).removeClass("selected-row");
        $(this).removeClass("selectedEdit-row");
    });
    $(x).addClass("selected-row");
}