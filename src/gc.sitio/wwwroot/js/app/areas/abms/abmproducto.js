$(function () {
    $("#btnFiltro").on("click", function () {
        if ($("#divFiltro").hasClass("show")) {
            $("#divDetalle").collapse("hide");
        }
    });
    //busqueda no gen de proveedores
    $(document).on("keydown.autocomplete", "input#Cta_Lista", function () {
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
                $("input#Cta_Id").val(ui.item.id);
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
                        var combo = $("#Pg_Id");
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
    //busqueda no gen de rubros
    $(document).on("keydown.autocomplete", "input#Rub_Lista", function () {
        $(this).autocomplete({
            source: function (request, response) {
                data = { prefix: request.term }
                $.ajax({
                    url: autoComRel02Url,
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
                $("input#Rub_Id").val(ui.item.id);
                return true;
            }
        });
    });

    $("#btnDetalle").prop("disabled", true);

    $("#btnCancel").on("click", function () {
        $("#btnFiltro").trigger("click");
    });
    $("#pagEstado").on("change", function () {
        var div = $("#divPaginacion");
        presentaPaginacion(div);
    });
    $("#btnBuscar").on("click", function () { buscarProductos(pagina); });

    $(".inputEditable").on("keypress", analizaEnterInput);

    $("#BtnLiTab02").on("click", presentarBarrado);

    //valida que los datos ingresados en barral 
    $("input .barr").on("blur", function () {
        var value = parseInt($(this).val(), 10);
        if (value < 1 || value > 9999) {
            AbrirMensaje("Atención!!", "El valor ingresado no es valido", function(){
                $(this).val(1);
                $(this).trigger("focus");
                $("#msjModal").modal("hide");
            }, ["Aceptar"], "warn!", null);
        }
    })
    
    InicializaPantallaAbmProd("tbGridProd");
    funcCallBack = buscarProductos;
/*    AbrirWaiting();*/
    return true;
});

function configuracionControlesAbmProd() {
    //Imp Int
    $("input#In_Alicuota").mask("000,000,000,000", { reverse: true });
    $("input#P_Balanza_Dvto").mask("000,000,000,000", { reverse: true });
    $("input#P_Con_Vto_Min").mask("000,000,000,000", { reverse: true });

    $("input#P_Peso").mask("000,000,000,000.000", { reverse: true });
    $("input#P_M_Capacidad").mask("000,000,000,000.000", { reverse: true });

    $("input .barr").mask("0000", {
        reverse: true,
        placeholder: '',
        translation: {
            '0': { pattern: /[0-9]/, optional: true }
        }
    });
}


function InicializaPantallaAbmProd(grilla) {   
    if (grilla !== tabGrid01 || grilla !== tabGrid02) {
        switch (tabAbm) {
            case 1:
                grilla = tabGrid01;
                break;
            case 2:
                grilla = tabGrid02;
                break;
            default:
                return false;
        }
    }

    var tb = $("#"+grilla+" tbody tr");
    if (tb.length === 0 && grilla ==="tbGridProd") {
        $("#divFiltro").collapse("show")
    } 

    $("#lbRel01").text("PROVEEDOR");
    $("#lbRel02").text("RUBRO");

    accionBotones("C");

    configuracionControlesAbmProd();

    $("#divDetalle").collapse("hide");

    //borra seleccion de registro si hubiera cargdo algun grid
    $("#" + grilla +" tbody tr").each(function (index) {
        $(this).removeClass("selectedEdit-row");
    });

    CerrarWaiting();
    return true;
}
 
function cargaPaginacion() {
    $("#divPaginacion").pagination({
        items: totalRegs,
        itemsOnPage: pagRegs,        
        cssStyle: "dark-theme",
        currentPage: pagina,
        onPageClick: function (num) {
            buscarProductos(num);
        }
    });
    $("#pagEstado").val(false);
    $("#divFiltro").collapse("hide")
    return true;
}

function buscarProductos(pag) {
    AbrirWaiting();

    //desactivamos los botones de acción
    activarBotones(false);


    var buscar = $("#Buscar").val();
    var id = $("#Id").val();
    var id2 = $("#Id2").val();
    var r01 = [];
    var r02 = [];    
    $("#Rel01List").children().each(function (i, item) { r01.push($(item).val()) });
    $("#Rel02List").children().each(function (i, item) { r02.push($(item).val()) });

    var data1 = {
        id, id2,
        rel01: r01,
        rel02: r02,
        rel03: [],
        "fechaD": null, //"0001-01-01T00:00:00",
        "fechaH": null, //"0001-01-01T00:00:00",
        buscar,
    };

    var buscaNew = JSON.stringify(dataBak) != JSON.stringify(data1)

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


    PostGenHtml(data, buscarUrl, function (obj) {
        $("#divGrilla").html(obj);
        $("#divFiltro").collapse("hide")
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
    }, function (obj) {
        ControlaMensajeError(obj.message);
        CerrarWaiting();
    });
}

function selectAbmRegDbl(x,gridId) {    
    AbrirWaiting("Espere mientras se busca el producto seleccionado...");
    $("#"+gridId+ " tbody tr").each(function (index) {
        $(this).removeClass("selectedEdit-row");
    });
    $(x).addClass("selectedEdit-row");
    var id = x.cells[0].innerText.trim();

    
    switch (tabAbm) {
        case 1:
            //se agrega por inyection el tab con los datos del producto
            var data = { p_id: id };
            PostGenHtml(data, buscarProdUrl, function (obj) {
                $("#divpanel01").html(obj);
                //se procede a buscar la grilla de barrado
                buscarBarrado(data);
                //se procede a buscar la grilla de Sucursales
                buscarLimite(data);

                $("#btnDetalle").prop("disabled", false);
                $("#divFiltro").collapse("hide");
                $("#divDetalle").collapse("show");

                //activar botones de acción
                activarBotones(true);

                CerrarWaiting();

            });
            break;
        case 2:
            //se busca el dato del barral 
            var data = { barradoId: id };
            PostGen(data, buscarBarradoUrl, function (obj) {
                CerrarWaiting();
                if (obj.error === true) {
                    AbrirMensaje("¡¡Algo no fué bien!!", obj.msg, function () {
                        $("#msjModal").modal("hide");
                        return true;
                    }, false, ["Aceptar"], "error!", null);
                } else if (obj.warn === true) {
                    AbrirMensaje("ATENCIÓN", obj.msg, function () {
                        if (obj.auth === true) {
                            window.location.href = login;
                        } else {
                            $("#msjModal").modal("hide");
                        }
                        return true;
                    }, false, ["Aceptar"], "warn!", null);
                }
                else {
                    //se presentan los datos en los controles
                    
                    $("#p_id").val(obj.datos.p_id);
                    $("#p_id_barrado").val(obj.datos.p_id_barrado); 
                    $("#p_unidad_pres").val(obj.datos.p_unidad_pres); 
                    $("#p_unidad_x_bulto").val(obj.datos.p_unidad_x_bulto);
                    $("#p_bulto_x_piso").val(obj.datos.p_bulto_x_piso);
                    $("#p_piso_x_pallet").val(obj.datos.p_piso_x_pallet); 
                    $("#tba_id").val(obj.datos.tba_id);                                                                   
                }

            });
            break;

    }
   

    //agrego el id en el control de busqueda simple y acciono el buscar.
    //$("#busquedaModal").modal("toggle");
    //$("input#Busqueda").val(id);
    //$("#btnBusquedaBase").trigger("click");
}

function buscarBarrado(data) {
    PostGenHtml(data, buscarBarradosUrl, function (obj) {
        $("#divBarrado").html(obj);
    });
}

function presentarBarrado() {
    tabAbm = 2;
    AbrirWaiting("Buscando Barrados...");
    PostGenHtml({}, presentarBarradoUrl, function (obj) {
        $("#divBarrado2").html(obj);

        var tb = $("#tbGridBarr tbody tr");
        if (tb.length === 0) {
            $("#tab2l1").hide();
            $("#tab2l2").hide();
        }
        else {
            $("#tab2l1").show();
            $("#tab2l2").show();
        }

        CerrarWaiting();
    });
}



function buscarLimite(data) {
    PostGenHtml(data, buscarLimiteUrl, function (obj) {
        $("#divSucursal").html(obj);
    });
}

