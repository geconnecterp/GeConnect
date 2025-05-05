
//variables de inicialización de aplicación
var tipoCta = "T";
//Estas variables se asignan al selecciona la cuenta en el autocompletar
var consCta = "";
var consRrss = "";
var consProv = "";


$(function () {
    $("#lbRel01").text("CUENTA");

    $("#Rel01").on("keyup", function () { });
    //check generico REL01 activando componentes disables
    //este evento esta en sitegen de manera estandar pero neceistaba ahora que afecte a otros componenes
    //de la vista 
    $("#chkRel01").on("click", function () {
        if (isInitializing) return; // Evita la recursión
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
            inicializaPantallaConsulta();
        }
    });

    //control para manipular la paginacion
    $("#pagEstado").on("change", function () {
        var div = $("#divPaginacion");
        presentaPaginacion(div);
    });
    //variable destinada a manejar la paginacion
    funcCallBack = consultaCtaCte;


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
    $("#btnTraerOP").on("click", consultaOP);
    $("#btnTraerRP").on("click", consultaRP);
    
    

    $("#controlConsultaCambio" + nnControlCta01).on("change", function (evento) {
        evento.preventDefault();
        evento.stopPropagation();
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
        //ctrlconsultacambio();
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
                            var tipo = ""; if (item.tipo === 'P') { tipo = 'Proveedor' } else { tipo = 'Cliente' }
                            var texto = "" + item.descripcion + " ("+ item.id + ") ("+tipo + ")";
                            return { label: texto, value: item.descripcion, id: item.id, tipo: item.tipo };
                        }));
                    }
                })
            },
            minLength: 3,
            select: function (event, ui) {
                if ($("#Rel01List").has('option:contains("' + ui.item.id + '")').length === 0) {
                    $("#Rel01Item").val(ui.item.id);
                    var opc = "<option value=" + ui.item.id + ">" + ui.item.value + "</option>"
                    $("#Rel01List").append(opc);

                    $("#Rel01").prop("disabled", true);
                    $("#Rel01List").prop("disabled", true);
                    consCta = ui.item.id;
                    consRrss = ui.item.label;
                    consTipo = ui.item.tipo;
                    $("#controlConsultaCambio" + nnControlCta01).val(true);

                    //se presenta u ocultan pagos y recepciones
                    if (consTipo === "P") {
                        $("#BtnLiTab04").show();
                        $("#BtnLiTab05").show();
                    } else {
                        $("#BtnLiTab04").hide();
                        $("#BtnLiTab05").hide();
                    }

                    //no hacemos trigger como antes, ejecutamos el metodo aislado
                    ctrlconsultacambio();

                    consultaCtaCte(1); //se selecciona la cuenta la pagina a visualizar es la primera
                }
            }
        }).data("ui-autocomplete")._renderItem = function (ul, item) {

            let className = "autocomplete-tipo-default";

            if (item.tipo === "P") {
                className = "autocomplete-tipo-p";
            } else {
                className = "autocomplete-tipo-e";
            }

            return $("<li>")
                .addClass(className)
                .append($("<div>").text(item.label))
                .appendTo(ul);
        };
    });


    $("#btnCancel").on("click", function () {
        window.location.href = initConsultaUrl;
    });

    inicializaPantallaConsulta();
    $("#divFiltro").collapse("show");
});

function ControlaEstadoChkRel01() {
    if (isInitializing) return; // Evita la recursión
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
        inicializaPantallaConsulta();
    }
}
function ctrlconsultacambio() {
    window["AsignaDatosCuenta" + nnControlCta01]();

    //muestro el control
    $("#controlCta" + nnControlCta01).show("fast");
    //oculto el filtro
    $("#btnFiltro").trigger("click");
    presentarTabsConsulta();
}

function inicializaPantallaConsulta() {
    if (isInitializing) return; // Evita la recursión
    isInitializing = true;

    var f = new Date();
    $("#fechaD").val(formatoFechaYMD(restarFecha(f, 365)));
    //definición de fecha para vencimientos
    f = new Date("01/01/1900");
    $("#cvfechaD").val(formatoFechaYMD(f));
    f = new Date("01/01/3500");
    $("#cvfechaH").val(formatoFechaYMD(f));

    f = new Date();
    $("#opfechaH").val(formatoFechaYMD(f));
    $("#rpfechaH").val(formatoFechaYMD(f));

    // Determinar la fecha desde (primer día del mes anterior)
    var primerDiaMesAnterior = new Date(f.getFullYear(), f.getMonth() - 1, 1);
    $("#opfechaD").val(formatoFechaYMD(primerDiaMesAnterior));
    $("#rpfechaD").val(formatoFechaYMD(primerDiaMesAnterior)); 

    $("#divpanel01").empty();
    $("#divPaginacion").empty();
    $("#divpanel02").empty();
    $("#divCmpteTot").empty().html("<span class='text - danger'>SIN REGISTROS</span>");
    $("#divCmpteDet").empty().html("<span class='text - danger'>SIN REGISTROS</span>");
    $("#divOpProv").empty().html("<span class='text - danger'>SIN REGISTROS</span>");
    $("#divOpProvDet").empty().html("<span class='text - danger'>SIN REGISTROS</span>");
    $("#divRpProv").empty().html("<span class='text - danger'>SIN REGISTROS</span>");
    $("#divRpProvDet").empty().html("<span class='text - danger'>SIN REGISTROS</span>");
    $("#BtnLiTab01").trigger("click");

    // Llama al evento click de #chkRel01 solo si no está ya en ejecución
    $("#chkRel01").trigger("click");

    isInitializing = false; // Restablece la bandera
}

function presentarTabsConsulta() {
    $("#consPaneles").show("fast");
    inicializaPantallaConsulta();
}
function ocultarTabsConsulta() {
    $("#consPaneles").hide("fast");
}


/**
 * Esta funcion se encarga de consultar la cuenta corriente
 * En esta busqueda de información de cuenta corriente el 
 * reporte se identifica como "Consulta de Cuenta Corriente" id=1
 * @param {any} pag
 */
function consultaCtaCte(pag) {
    AbrirWaiting();
    tabAbm = 1;
    ctaId = consCta;
    fechaD = $("#fechaD").val();
    userId = usuarioAuth;   //variable declarada en _layout
    let admId = administracion;

    //reporte #1
    cargarReporteEnArre(1, {ctaId,fechaD,userId}, "Informe de Cuenta Corriente", "Observación:", admId)

    var data1 = { ctaId: consCta, fechaD };

    var buscaNew = JSON.stringify(dataBak) != JSON.stringify(data)

    if (buscaNew === false) {
        //son iguales las condiciones cambia de pagina
        pagina = pag;
    }
    else {
        dataBak = data1;
        pagina = 1;
        pag = 1;
    }
    var sort = null;
    var sortDir = null

    var data2 = { sort, sortDir, pag, buscaNew }

    var data = $.extend({}, data1, data2);

    PostGenHtml(data, consultaCtaCteUrl, function (obj) {
        $("#divpanel01").html(obj);

        PostGen({}, buscarMetadataURL, function (obj) {
            if (obj.error === true) {
                AbrirMensaje("ATENCIÓN", obj.msg, function () {
                    $("#msjModal").modal("hide");
                    return true;
                }, false, ["Aceptar"], "error!", null);
            }
            else {
                totalRegs = obj.metadata.totalCount;
                pags = obj.metadata.totalPages;
                pagRegs = obj.metadata.pageSize;

                $("#pagEstado").val(true).trigger("change");
            }

        });

        CerrarWaiting();
    });
}


function consultaVencimiento() {
    tabAbm = 2;
    ctaId = consCta;
    fechaD = $("#cvfechaD").val();
    fechaH = $("#cvfechaH").val();
    userId = usuarioAuth;   //variable declarada en _layout
    let admId = administracion;

    //reporte #2
    cargarReporteEnArre(2, { ctaId, fechaD, fechaH, userId }, "Informe de Vencimiento de Comprobantes", "Observación:", admId)

    var data = { ctaId: consCta, fechaD, fechaH };
    AbrirWaiting("Espere un momento mientras se presenta los comprobantes con vencimiento...");
    PostGenHtml(data, consultaVencimientoUrl, function (obj) {
        $("#divpanel02").html(obj);
        CerrarWaiting();
    });
}

function consultaCmpteTotal() {
    ctaId = consCta; tabAbm = 3;
    relCuil = false;
    if ($("#relCuil").is(":checked")) {
        relCuil = true;
    }
    meses = $("#inMeses").val();

    userId = usuarioAuth;   //variable declarada en _layout
    let admId = administracion;

    //reporte #3
    cargarReporteEnArre(3, { ctaId, relCuil, meses, userId }, "Informe de Comprobantes Total", "Observación:", admId)

    var data = { ctaId: consCta, meses, relCuil };
    AbrirWaiting("Espere un momento mientras se presentan los Comprobantes del periodo seleccionado...");
    PostGenHtml(data, consultarCmpteTotalUrl, function (obj) {
        $("#divCmpteTot").html(obj);
        CerrarWaiting();
    });
}

function consultaCmpteDetalle(periodo) {
    fkey = periodo;
    ctaId = consCta;
    relCuil = false;
    if ($("#relCuil").is(":checked")) {
        relCuil = true;
    }
    mes = periodo;
    userId = usuarioAuth;   //variable declarada en _layout
    let admId = administracion;

    //reporte #4
    cargarReporteEnArre(4, { ctaId, relCuil, mes, userId }, "Informe de Comprobantes Detalle", "Observación:", admId)
 
    var data = { ctaId: consCta, mes, relCuil };
    AbrirWaiting("Espere un momento mientras se presenta el detalle del comprobante seleccionada...");
    PostGenHtml(data, consultarCmpteDetalleUrl, function (obj) {
        $("#divCmpteDet").html(obj);
        CerrarWaiting();
    });
}

function consultaOP() {
    tabAbm = 4;
    fechaD = $("#opfechaD").val();
    fechaH = $("#opfechaH").val();

    var data = { ctaId: consCta, fechaD, fechaH };
    AbrirWaiting("Espere un momento mientras se presenta las OP del periodo seleccionado...");
    PostGenHtml(data, consultarOPProvUrl, function (obj) {
        $("#divOpProv").html(obj);
        CerrarWaiting();
    });
}
function consultaOPPDetalle(cmptId) {
    fkey = cmptId;
    var data = { cmptId };

    userId = usuarioAuth;   //variable declarada en _layout
    let admId = administracion;

    //reporte #4
    cargarReporteEnArre(4, { ctaId, relCuil, mes, userId }, "Informe de Comprobantes Detalle", "Observación:", admId)

    AbrirWaiting("Espere un momento mientras se presenta el detalle de la OP seleccionada...");
    PostGenHtml(data, consultarOPProvDetUrl, function (obj) {
        $("#divOpProvDet").html(obj);
        CerrarWaiting();
    });
}

function consultaRP() {
    tabAbm = 5;
    fechaD = $("#rpfechaD").val();
    fechaH = $("#rpfechaH").val();

    var data = { ctaId: consCta, fechaD, fechaH };
    AbrirWaiting("Espere un momento mientras se presenta las recepciones del proveedor en el periodo seleccionado...");
    PostGenHtml(data, consultarRPProvUrl, function (obj) {
        $("#divRpProv").html(obj);
        CerrarWaiting();
    });
}

function consultarRPPDetalle(cmptId) {

    var data = { cmptId };
    AbrirWaiting("Espere un momento mientras se presenta el detalle de la Recepción seleccionada...");
    PostGenHtml(data, consultarRPProvDetUrl, function (obj) {
        $("#divRpProvDet").html(obj);
        CerrarWaiting();
    });
}

function SeleccionarPeriodo(x, grid) {
    var xx = $(x);
    selectReg(x, grid);
    valor = xx.find("td:nth-child(1)").text();

    switch (tabAbm) {
        //case 1:
        //    break;
        //case 2:
        //    break;
        case 3:
            consultaCmpteDetalle(valor);
            break;
        case 4:
            consultaOPPDetalle(valor);
            break;
        case 5:
            consultarRPPDetalle(valor);
            break;
        default:
            return false;
    }
    //se llama el detalle de comprobantes de un mes especifico
}

