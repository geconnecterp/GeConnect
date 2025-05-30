﻿$(function () {

    $(".barr").mask("00000", {
        reverse: true,
    });

    //valida que los datos ingresados en barral sean válidos
    $(document).on("blur", ".barr", function () {
        var value = parseInt($(this).val(), 10);
        if (value < 1 || value > 99999) {
            AbrirMensaje("Atención!!", "El valor ingresado no es valido", function () {
                $(this).val(1);
                $(this).trigger("focus");
                $("#msjModal").modal("hide");
                return true;
            }, false, ["Aceptar"], "warn!", null);
        }
    });

    //eventos para controlar el doble click de las tablas.
    $(document).on("dblclick", "#" + tabGrid01 + " tbody tr", function () {
        x = $(this);
        regSelected = x;
        ejecutaDblClickGrid1(x);
    });
    $(document).on("dblclick", "#" + tabGrid02 + " tbody tr", function () {
        x = $(this);
        ejecutaDblClickGrid2(x);
    });
    $(document).on("dblclick", "#" + tabGrid03 + " tbody tr", function () {
        x = $(this);
        ejecutaDblClickGrid3(x);
    });


    $("#btnFiltro").on("click", function () {
        if ($("#divFiltro").hasClass("show")) {
            $("#divDetalle").collapse("hide");
        }
    });

  
    
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
    //busqueda no gen de rubros
    $(document).on("keydown.autocomplete", "input#rub_lista", function () {
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
                $("input#rub_id").val(ui.item.id);
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
    $("#btnBuscar").on("click", function () {
       
        //es una busqueda por filtro. siempre sera pagina 1
        if (accion !== AbmAction.MODIFICACION && accion !== AbmAction.ALTA) {
            //es nueva la busqueda no resguardamos la busqueda anterior. es util para paginado
            dataBak = "";
            pagina = 1;
            buscarProductos(pagina);
        }
        else {
            buscarProductos(pagina, function () {
                if (accion === AbmAction.MODIFICACION) {
                    // Seleccionar el registro modificado
                    selectRegProd(regSelected, Grids.GridProductos);
                } else if (accion === AbmAction.ALTA) {
                    // Presentar solo el registro recién creado
                    presentarRegistroAlta(regSelected);
                }
            });
        }
        
    });

    $(".inputEditable").on("keypress", analizaEnterInput);

    $("#BtnLiTab01").on("click", function () {
        tabAbm = 1;
        activarGrilla(tabGrid01);        
        if (EntidadSelect !== "" && regSelected !== "") {
            activarBotones(true);
        }
    });

    $("#BtnLiTab02").on("click", presentarBarrado);
    $("#BtnLiTab03").on("click", presentarLimites);


    InicializaPantallaAbmProd("tbGridProd");

    $("#btnDetalle").on("mousedown", analizaEstadoBtnDetalle); 

    funcCallBack = buscarProductos;
    /*    AbrirWaiting();*/
    return true;
});

function analizaEstadoBtnDetalle() {
    var res = $("#divDetalle").hasClass("show");
    if (res === true) {
        selectRegProd(regSelected, tabGrid01);
    }
    return true;

}

function InicializaPantallaAbmProd(grilla) {
    if (grilla !== tabGrid01 || grilla !== tabGrid02 || grilla !== tabGrid03) {
        switch (tabAbm) {
            case 1:
                grilla = Grids.GridProductos;
                if ($("#divDetalle").is(":visible")) {
                    $("#divDetalle").collapse("hide");
                }
                break;
            case 2:
                grilla = Grids.GridBarrado;
                break;
            case 3:
                grilla = Grids.GridLimite;
            default:
                return false;
        }
    }
    nng = "#" + grilla;
    tb = $(nng + " tbody tr");
    if (tb.length === 0 ) {
        switch (tabAbm) {
            case 1:
                $("#divFiltro").collapse("show");
                break;
            case 2:
                presentarBarrado();
                break;
            case 3:
                break;
            default:
                return false;
        }
    }

    accionBotones(AbmAction.CANCEL);

    //borra seleccion de registro si hubiera cargdo algun grid
    //TODO: ESTO SOLO LO HACE SI ES LA GRILLA PPAL (TABAMB = 1)

    $("#" + grilla + " tbody tr").each(function (index) {
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

function buscarProductos(pag,callback) {
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

    if (buscaNew === false || callback!==undefined) {
        //son iguales las condiciones cambia de pagina
        pagina = pag;
        //esto lo hago para que en caso que se haya hecho una modificación, si no se tiene la busqueda anterior, se carga en dataBak = data1
        if (callback !== undefined && dataBak === "") {
            dataBak = data1;
        }
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
        // Ejecutar callback si está definido
        if (typeof callback === "function") {
            callback();
        }
    }, function (obj) {
        ControlaMensajeError(obj.message);
        CerrarWaiting();
    });
}

function buscarProductoServer(data) {
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

        if (EntidadEstado !== "S" && accion!=="") {
            $("#BtnLiTab02").prop("disabled", true);
            $("#BtnLiTab02").addClass("text-danger");
            $("#BtnLiTab03").prop("disabled", true);
            $("#BtnLiTab03").addClass("text-danger");

        }      

        CerrarWaiting();

    });
}

function selectRegProd(x, gridId) {
    //reinvoco para que me marque el registro 
    selectReg(x, gridId);
    //limpio el tab01 para que se seleccione el registro.
    //y desactivo el tab
    switch (tabAbm) {
        case 1:
            $("#divpanel01").empty();
            if ($("#divDetalle").is(":visible")) {
                $("#divDetalle").collapse("hide");
            }
            $("#btnDetalle").prop("disabled", true);
            activarGrilla(tabGrid01);
            activarBotones(false);
            break;
        case 2:
        case 3:
            break;
        default:
            return false;
    }
   
}

function ejecutaDblClickGrid1(x) {
    AbrirWaiting("Espere mientras se busca el producto seleccionado...");   
    selectAbmRegDbl(x, tabGrid01);
}
function ejecutaDblClickGrid2(x) {
    AbrirWaiting("Espere mientras se busca el Barrado seleccionado...");
    selectAbmRegDbl(x, tabGrid02);

}
function ejecutaDblClickGrid3(x) {
    AbrirWaiting("Espere mientras se busca el Limite de Stock seleccionado...");
    selectAbmRegDbl(x, tabGrid03);

}

function selectAbmRegDbl(x, gridId) {
    $("#" + gridId + " tbody tr").each(function (index) {
        $(this).removeClass("selectedEdit-row");
    });
    $(x).addClass("selectedEdit-row");
    var id = x.find("td:nth-child(1)").text();
   

    switch (tabAbm) {
        case 1:
            //se agrega por inyection el tab con los datos del producto
            EntidadEstado = x.find("td:nth-child(9)").text();
            var data = { p_id: id };
            EntidadSelect = id;
            desactivarGrilla(tabGrid01);
            buscarProductoServer(data);
            posicionarRegOnTop(x);
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
                    //activar botones de acción
                    activarBotones(true);

                    $("#BtnLiTab01").prop("disabled", true);
                    $("#BtnLiTab01").addClass("text-danger");
                    $("#BtnLiTab03").prop("disabled", true);
                    $("#BtnLiTab03").addClass("text-danger");
                }

            });
            break;
        case 3:
            //se busca  
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
        default:
            return false;
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
    AbrirWaiting("Buscando Barrados...");
    tabAbm = 2;
    desactivarGrilla(Grids.GridBarrado);
    InicializaPantallaAbmProd(Grids.GridBarrado);
    $("#divBarrado2").empty();
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

function presentarLimites() {
    //AbrirWaiting("Buscando Limites...");
    tabAbm = 3;
    desactivarGrilla(tabGrid01);
    InicializaPantallaAbmProd(tabGrid03);

    PostGenHtml({}, presentarLimitesUrl, function (obj) {
        $("#divLimite2").html(obj);

        var tb = $("#"+tabGrid03+" tbody tr");
        if (tb.length === 0) {
            $("#tab3l1").hide();          
        }
        else {
            $("#tab3l1").show();
        }

        CerrarWaiting();
    });
}


function buscarLimite(data) {
    PostGenHtml(data, buscarLimiteUrl, function (obj) {
        $("#divSucursal").html(obj);
    });
}

