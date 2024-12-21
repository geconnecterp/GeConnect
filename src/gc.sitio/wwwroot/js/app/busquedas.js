$(function () {
    $("button[type='button'].close.buscAdv").on("click", function () {
        $("#busquedaModal").modal("toggle");

    })
    $("input#Busqueda").keypress(verificaTeclaDeBusqueda);
    $("input#Rel01").on("click", function () {
        $("input#Rel01").val("");
        $("#Rel01Item").val("");
    });
    $("input#Rel02").on("click", function () {
        $("input#Rel02").val("");
        $("#Rel02Item").val("");
    });

    //elimina item de la lista
    $("#Rel01List").on("dblclick", 'option', function () { $(this).remove(); })
    $("#Rel02List").on("dblclick", 'option', function () { $(this).remove(); })

    $("input").on("focus", function () { $(this).select(); })

    $("#btnBuscarProd").on("click", function () { busquedaAvanzadaProductos(pagina); })

    //le especifico dinamicamente cual sera el div a cargar el paginado
    $("#pagEstado").on("change", function () {
        var div = $("#divPaginacionAdv");
        presentaPaginacion(div);
    });

    /*    $("input[type='button'],.btn-grid").on("click", ordenarColumna);*/

    funcCallBack = busquedaAvanzadaProductos;

    return true;
});

function ordenarColumna(x) {
    AbrirWaiting();
    var sortdir = $(x).attr("sortDir");
    var sort = $(x).attr("sort");
    if (sortdir === "asc") {
        $(x).attr("sortDir", "desc");
        sortdir = "desc";
        var icon = $(x).find("i");
        $(icon).removeClass("bx-chevron-down-circle").addClass("bx-chevron-up-circle");
    }
    else {
        $(x).attr("sortDir", "asc");
        sortdir = "asc";
        var icon = $(x).find("i");
        $(icon).removeClass("bx-chevron-up-circle").addClass("bx-chevron-down-circle");
    }

    var data = $.extend({}, dataBak, { buscaNew: false, sort, sortDir: sortdir, pag: pagina });

    PostGenHtml(data, busquedaAvanzadaUrl, function (obj) {
        CerrarWaiting();
        $("#divBusquedaAvanzada").html(obj);
        return true;
    });
    return true;
}

function busquedaAvanzadaProductos(pag) {
    var ri01 = $("#Rel01Item").val();
    var ri02 = $("#Rel02Item").val();
    //activos
    var act = $("#chkActivos").is(":checked");
    //discontinuos
    var dis = $("#chkDisc").is(":checked");
    //inactivos
    var ina = $("#chkInact").is(":checked");
    var cstk = true;
    var sstk = true;
    if ($("#rdConStk").is(":checked") || $("#rdSinStk").is(":checked")) {
        if ($("#rdSinStk").is(":checked")) {
            sstk = true;
            cstk = false
        }
        else {
            sstk = false;
            cstk = true;
        }
    }
    var buscar = $("#Search").val();
    var data1 = {
        ri01, ri02, act, dis, ina, cstk, sstk, buscar
    };
    //si es distinto es TRUE    
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
    //{
    //    ri01, ri02, act, dis, ina, cstk, sstk,buscar,sort,sortDir,pag
    //};

    PostGenHtml(data, busquedaAvanzadaUrl, function (obj) {
        $("#divBusquedaAvanzada").html(obj);
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
    });

    return true;
}

function buscarProducto() {
    AbrirWaiting();
    var _post = busquedaProdBaseUrl;
    var valor = $("#Busqueda").val();
    var validarEstado = true;

    var datos = {};
    if (typeof validarEstado !== 'undefined') {
        datos = { busqueda: valor, validarEstado };
    }
    else {
        datos = { busqueda: valor };
    }

    PostGen(datos, _post, function (obj) {
        if (obj.error === true) {
            AbrirMensaje("ATENCIÓN", obj.msg, function () {
                productoBase = null;
                $("#estadoFuncion").val(false);
                $("#btnBusquedaBase").prop("disabled", false);
                $("#msjModal").modal("hide");
                $("#Busqueda").focus();
                return true;
            }, false, ["Aceptar"], "error!", null);
        }
        else if (obj.warn === true) {
            if (obj.producto.p_id === "0000-0000") {
                AbrirMensaje("ATENCIÓN", obj.msg, function () {
                    productoBase = null;
                    $("#estadoFuncion").val(false);
                    $("#btnBusquedaBase").prop("disabled", false);
                    $("#msjModal").modal("hide");
                    $("#Busqueda").focus();
                    return true;
                }, false, ["Aceptar"], "error!", null);
            }
            else if (obj.producto.p_id === "NO") {
                CerrarWaiting();

                if (funcionBusquedaAvanzada === true) {
                    AbrirMensaje("ATENCIÓN", "NO SE ENCONTRO EL PRODUCTO QUE INTENTO BUSCAR. SE ABRIRÁ LA BUSQUEDA AVANZADA.", function () {
                        $("#msjModal").modal("hide");
                        productoBase = null;
                        $("#estadoFuncion").val(false);
                        inicializaBusquedaAvanzada();
                        $("#busquedaModal").modal("toggle");
                        return true;
                    }, false, ["Aceptar"], "error!", null);

                    return true;
                }
                else {
                    AbrirMensaje("ATENCIÓN", "NO SE ENCONTRO EL PRODUCTO QUE INTENTO BUSCAR.", function () {
                        $("#msjModal").modal("hide");
                        $("#Busqueda").focus();
                        return true;
                    }, false, ["Aceptar"], "error!", null);

                }               
            } else {
                //encontro producto pero hay warning
                CerrarWaiting();
                AbrirMensaje("ATENCIÓN!", obj.msg, function (resp) {
                    if (resp === "SI") {
                        productoBase = obj.producto;
                        $("#estadoFuncion").val(true);
                        $("#estadoFuncion").trigger("change");
                        $("#msjModal").modal("hide");
                        var up = $("#txtUPEnComprobanteRP");
                        if (up) {
                            up.focus();
                        }
                        return true;
                    }
                    else {
                        //se deniega
                        productoBase = null;
                        $("#estadoFuncion").val(false);
                        $("#btnBusquedaBase").prop("disabled", false);
                        $("#msjModal").modal("hide");
                        $("#Busqueda").focus();
                        return true;
                    }
                },
                    true, ["Aceptar", "Denegar"], "Warning!", null);
            }
        }
        else {
            //encontro y se presenta
            productoBase = obj.producto;
            $("#estadoFuncion").val(true);
            $("#estadoFuncion").trigger("change");
            return true;
        }
    });
    return true;
}

function verificaTeclaDeBusqueda(e) {
    if (e.which == "13") {

        $("#btnBusquedaBase").trigger("click");
        $("#btnBusquedaBase").prop("disabled", true);
        return true;

    }
}

function selectRegDbl(x) {
    $("#tbGridProd tbody tr").each(function (index) {
        $(this).removeClass("selected-row");
    });
    $(x).addClass("selected-row");
    var id = x.cells[0].innerText.trim();

    //agrego el id en el control de busqueda simple y acciono el buscar.
    $("#busquedaModal").modal("toggle");
    $("input#Busqueda").val(id);
    $("#btnBusquedaBase").trigger("click");

}

function selectReg(x) {
    $("#tbGridProd tbody tr").each(function (index) {
        $(this).removeClass("selected-row");
    });
    $(x).addClass("selected-row");
}

function selectReg(x,gridId) {
    $("#"+gridId+" tbody tr").each(function (index) {
        $(this).removeClass("selected-row");
    });
    $(x).addClass("selected-row");
}

function inicializaBusquedaAvanzada() {
    //configurando proveedor
    if (typeof provUnico !== 'undefined' &&
        provUnico === true) {
        $("input#Rel01").val(provDesc).prop("disabled", true);
        $("input#Rel01Item").val(provId);
    }
    else {
        $("input#Rel01").val("").prop("disabled", false);
        $("input#Rel01Item").val("");
    }

    //configurando RUBROS
    if (typeof rubUnico !== 'undefined' &&
        rubUnico === true) {
        $("input#Rel02").val(rubDesc).prop("disabled", true);
        $("input#Rel02Item").val(rubId);
    }
    else {
        $("input#Rel02").val("").prop("disabled", false);
        $("input#Rel02Item").val("");
    }

    //configurando ESTADOS
    if (typeof estadoUnico !== 'undefined' &&
        estadoUnico === true) {
        $("#chkActivos").prop("checked", estActivo).prop("disabled", true);
        $("#chkDisc").prop("checked", estDiscon).prop("disabled", true);
        $("#chkInact").prop("checked", estInacti).prop("disabled", true);
    }
    else {
        $("#chkActivos").prop("checked", true).prop("disabled", false);
        $("#chkDisc").prop("checked", false).prop("disabled", false);
        $("#chkInact").prop("checked", false).prop("disabled", false);
    }
    return true;
}