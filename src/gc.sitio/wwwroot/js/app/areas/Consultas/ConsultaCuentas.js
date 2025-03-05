//variables de inicialización de aplicación
var tipoCta = "T";
//Estas variables se asignan al selecciona la cuenta en el autocompletar
var consCta = "";
var consRrss = "";
var consProv = "";


$(function () {
    $("#lbRel01").text("CUENTA");


    //check generico REL01 activando componentes disables
    //este evento esta en sitegen de manera estandar pero neceistaba ahora que afecte a otros componenes
    //de la vista 
    $("#chkRel01").on("click", function () {
        if ($("#chkRel01").is(":checked")) {
            $("#Rel01").prop("disabled", false);
            $("#Rel01List").prop("disabled", false);
            $("#Rel01").trigger("focus");
        }
        else {
            $("#Rel01").prop("disabled", true).val("");
            $("#Rel01List").prop("disabled", true).empty();
            ocultarTabsConsulta();
            window["inicializaCtrl" + nnControlCta01]();
        }
    });

    $("#btnFiltro").on("click", function () {
        if ($("#divFiltro").hasClass("show")) {
            $("#divFiltro").collapse("hide");
        }
        else {
            $("#divFiltro").collapse("show");
        }

    });

    $('#inMeses').val(12);
    // Aplicar la máscara para permitir solo números
    $('#inMeses').mask('00', {
        placeholder: '0',
        translation: {
            '0': { pattern: /[0-9]/, optional: true }
        }
    });

    // Validar el rango de 1 a 60
    $('#inMeses').on('blur', function () {
        var value = parseInt($(this).val(), 10);
        if (value < 1 || value > 60 || isNaN(value)) {
            AbrirMensaje("Atención!!", "Por favor, ingrese un valor entre 1 y 60", function () {
                $("#msjModal").modal("hide");
            }, false, ["Aceptar"], "warn!", null);
            $(this).val(''); // Limpiar el campo si el valor es inválido
        }
    });
    $('#inMeses').val(12);


    $("#btnTraerCtaCte").on("click", consultaCtaCte);
    $("#btnTraerVenc").on("click", consultaVencimiento);
    $("#btnTraerCmpts").on("click", consultaCmpteTotal);
    

    $("#controlConsultaCambio" + nnControlCta01).on("change", function () {
        //if (callbackConsultaCuenta !== null &&
        //    callbackConsultaCuenta !== "" &&
        //    callbackConsultaCuenta !== undefined) {
        // sirve para invocar a otra funcion componiendo el nombre con alguna variable
        window["AsignaDatosCuenta" + nnControlCta01]();

        //muestro el control
        $("#controlCta" + nnControlCta01).show("fast");
        //oculto el filtro
        $("#btnFiltro").trigger("click");
        presentarTabsConsulta();
        //inicio la primera presentacion de datos 
    });

    //se debe tapar el evento en siteGen del autocompletar del #Rel01
    $(document).on("keydown.autocompete", "input#Rel01", function () {
        //codigo generico para autocomplete 01
        $(this).autocomplete({
            source: function (request, response) {
                data = { cuenta: request.term, tipo: tipoCta, esAutoComp: true };

                $.ajax({
                    url: autoComRel01Url,
                    type: "POST",
                    dataType: "json",
                    data: data,
                    success: function (obj) {
                        response($.map(obj, function (item) {
                            var texto = item.descripcion;
                            return { label: texto, value: item.descripcion, id: item.id, prov: item.provId };
                        }));
                    }
                })
            },
            minLength: 4,
            select: function (event, ui) {
                if ($("#Rel01List").has('option:contains("' + ui.item.id + '")').length === 0) {
                    $("#Rel01Item").val(ui.item.id);
                    var opc = "<option value=" + ui.item.id + ">" + ui.item.value + "</option>"
                    $("#Rel01List").append(opc);

                    $("#Rel01").prop("disabled", true);
                    $("#Rel01List").prop("disabled", true);
                    consCta = ui.item.id;
                    consRrss = ui.item.label;
                    consProv = ui.item.prov;
                    $("#controlConsultaCambio" + nnControlCta01).val(true).trigger("change");
                    consultaCtaCte();
                }
                return true;
            }
        });
    });


    $("#btnCancel").on("click",)

    inicializaPantallaConsulta();
    $("#divFiltro").collapse("show");
});

function inicializaPantallaConsulta() {
    var f = new Date();
    $("#fechaD").val(formatoFechaYMD(restarFecha(f, 365)));
    f = new Date("01/01/1900");
    $("#cvfechaD").val(formatoFechaYMD(f));
    f = new Date("01/01/3500");
    $("#cvfechaH").val(formatoFechaYMD(f));

}

function presentarTabsConsulta() {
    $("#consPaneles").show("fast");
}
function ocultarTabsConsulta() {
    $("#consPaneles").hide("fast");
}
function consultaCtaCte() {
    ctaId = consCta;
    fechaD = $("#fechaD").val();
    var data = { ctaId: consCta, fechaD };
    AbrirWaiting();
    PostGenHtml(data, consultaCtaCteUrl, function (obj) {
        $("#divpanel01").html(obj);
        CerrarWaiting();
    });
}


function consultaVencimiento() {
    ctaId = consCta;
    fechaD = $("#cvfechaD").val();
    fechaH = $("#cvfechaH").val();
    var data = { ctaId: consCta, fechaD, fechaH };
    AbrirWaiting();
    PostGenHtml(data, consultaVencimientoUrl, function (obj) {
        $("#divpanel02").html(obj);
        CerrarWaiting();
    });
}

function consultaCmpteTotal() {
    ctaId = consCta;
    relCuil = false;
    if ($("#relCuil").is(":checked")) {
        relCuil = true;
    }
    meses = $("#inMeses").val();
    var data = { ctaId: consCta, meses, relCuil };
    AbrirWaiting();
    PostGenHtml(data, consultarCmpteTotalUrl, function (obj) {
        $("#divCmpteTot").html(obj);
        CerrarWaiting();
    });
}

function consultaCmpteDetalle(periodo) {
    ctaId = consCta;
    relCuil = false;
    if ($("#relCuil").is(":checked")) {
        relCuil = true;
    }
 
    var data = { ctaId: consCta, mes:periodo, relCuil };
    AbrirWaiting();
    PostGenHtml(data, consultarCmpteDetalleUrl, function (obj) {
        $("#divCmpteDet").html(obj);
        CerrarWaiting();
    });
}

function SeleccionarPeriodo(x, grid) {
    var xx = $(x);
    selectReg(x, grid);
    periodo = xx.find("td:nth-child(1)").text();

    //se llama el detalle de comprobantes de un mes especifico
    consultaCmpteDetalle(periodo);
}

