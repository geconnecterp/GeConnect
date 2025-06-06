﻿$(function () {
    $("#btnAbmNuevo").on("click", ejecutarAlta);
    $("#btnAbmModif").on("click", ejecutarModificacion);
    $("#btnAbmElimi").on("click", ejecutarBaja);

    $("#btnAbmCancelar").on("click", InicializaPantallaRepartidor);
    $("#btnAbmAceptar").on("click", confirmarOperacionAbmRepartidor);

    $("#btnDetalle").on("mousedown", analizaEstadoBtnDetalle);

   
});

function ejecutarBaja() {
    switch (tabAbm) {
        case 1:
            $("#divFiltro").collapse("hide");
            accionBotones(AbmAction.BAJA);
            break;
       
        default:
            return false;
    }
}

function ejecutarModificacion() {
    $("#divFiltro").collapse("hide");
    accionBotones(AbmAction.MODIFICACION);
    activarControles(true);
    //switch (tabAbm) {
    //    case 2:
    //        activarArbol("#divPerfiles", "#", true)
    //        break;
    //    case 3:
    //        activarArbol("#divAdmins", "#", true)
    //        break;
    //    case 4:
    //        activarArbol("#divDers", "#", true)
    //        break;
    //    default:
    //        break;
    //}
}

function ejecutarAlta() {

    AbrirWaiting("Espere, se blanquea el formulario...");

    var data = {};
    switch (tabAbm) {
        case 1:
            PostGenHtml(data, nuevoRepartidorUrl, function (obj) {
                $("#divpanel01").html(obj);
                ////se procede a buscar la grilla de barrado
                //buscarBarrado(data);
                ////se procede a buscar la grilla de Sucursales
                //buscarLimite(data);

                $("#btnDetalle").prop("disabled", false);
                $("#divFiltro").collapse("hide");
                $("#divDetalle").collapse("show");

                accionBotones(AbmAction.ALTA);
                activarControles(true);

                CerrarWaiting();
            });

            break;

        default:
            return false;
    }
}


function accionBotones(btn) {
    if (btn === AbmAction.ALTA ||
        btn === AbmAction.MODIFICACION ||
        btn === AbmAction.BAJA) {
        switch (tabAbm) {
            case 1:
                accion = btn;
                break;
            case 2:
                accion02 = btn;
                break;
            case 3:
                accion03 = btn;
                break;
        }

        $("#btnFiltro").prop("disabled", true);
        $("#btnDetalle").prop("disabled", true);
        $("#BtnLiTab01").prop("disabled", true);
        $("#BtnLiTab02").prop("disabled", true);


        $("#btnAbmNuevo").prop("disabled", true);
        $("#btnAbmModif").prop("disabled", true);
        $("#btnAbmElimi").prop("disabled", true);

        if (tabAbm === 1) {
            $("#btnAbmAceptar").prop("disabled", false);
            $("#btnAbmCancelar").prop("disabled", false);
        }
        if (tabAbm === 2 && btn === AbmAction.MODIFICACION) {
            //desactivo el ddl
            $("#MenuId").prop("disabled", true);
        }
        $("#btnAbmAceptar").show();
        $("#btnAbmCancelar").show();
    } else if (btn === AbmAction.SUBMIT || btn === AbmAction.CANCEL) {   // (S)uccess - (C)ancel
        $("#btnFiltro").prop("disabled", false);
        $("#btnDetalle").prop("disabled", false);

        $("#BtnLiTab01").prop("disabled", false);
        $("#BtnLiTab02").prop("disabled", false);

        $("#BtnLiTab01").removeClass("text-danger");
        $("#BtnLiTab02").removeClass("text-danger");


        if (btn === AbmAction.ALTA) {

        }
        else if (btn === AbmAction.CANCEL) {

            activarBotones2(false);
            activarControles(false);

            if (tabAbm === 1) {
                $("#btnDetalle").prop("disabled", true);
                activarGrilla(Grids.GridPerfil);
            }

        }
    }
}

function activarControles(act) {
    //se cambia el sentido del valor act para que se interprete
    //que si se activa los controles sea SI = TRUE y cuando sea False es que se ponen todos los controles en disabled
    if (act === true || act === false) {
        act = !act; //se cambia el sentido del valor ya que con true, se activa el disabled.-
        //se tiene en cuenta que tab es el que esta activo al momento de presionar el boton de acción
        switch (tabAbm) {
            case 1:

                //Linea 1
                if (accion === AbmAction.MODIFICACION) {
                 
                }
                $("#VeActivo").prop("disabled", act);

                //Linea 02
                $("#rp_nombre").prop("disabled", act);
                $("#rp_comision").mask("99.99", { reverse: true }).prop("disabled", act);                
                $("#rp_mail").prop("disabled", act);
                $("#rp_celu").mask("(000) 000-0000").prop("disabled", act);                
                break;
           
            default:
                return false;
        }

    }
}

function buscarRepartidor(data) {
    PostGenHtml(data, buscarRepartidorUrl, function (obj) {
        $("#divpanel01").html(obj);
        //se procede a buscar la grilla de barrado

        $("#btnDetalle").prop("disabled", false);
        $("#divFiltro").collapse("hide");
        $("#divDetalle").collapse("show");

        //activar botones de acción
        activarBotones(true);

        //if (EntidadEstado !== "S" && accion !== "") {
        //    $("#BtnLiTab02").prop("disabled", true);
        //    $("#BtnLiTab02").addClass("text-danger");
        //    $("#BtnLiTab03").prop("disabled", true);
        //    $("#BtnLiTab03").addClass("text-danger");

        //}
        //else {
        //    $("#BtnLiTab02").prop("disabled", false);
        //    $("#BtnLiTab02").removeClass("text-danger");
        //    $("#BtnLiTab03").prop("disabled", false);
        //    $("#BtnLiTab03").removeClass("text-danger");
        //}

        CerrarWaiting();
    });
    return true;
}


function confirmarOperacionAbmRepartidor() {
    AbrirWaiting("Completando proceso...");
    var data = {};
    switch (tabAbm) {
        case 1:
            data = confirmarDatosTab01();
            break;       
        default:
            return false;
    }
    urlabm = ""
    switch (tabAbm) {
        case 1:
            urlabm = confirmarAbmRepartidorUrl;
            break;
        
        default:
    }
    PostGen(data, urlabm, function (obj) {
        if (obj.error === true) {
            CerrarWaiting();
            AbrirMensaje("ALGO NO SALIO BIEN!", obj.msg, function () {
                $("#msjModal").modal("hide");
            }, false, ["CONTINUAR"], "error!", null);
        }
        else if (obj.warn === true) {
            CerrarWaiting();

            AbrirMensaje("ATENCIÓN", obj.msg, function () {
                if (obj.auth === true) {
                    window.location.href = login;
                }
                else {
                    $("#msjModal").modal("hide");
                }
            }, false, ["CONTINUAR"], "warn!", null);

        }
        else {
            CerrarWaiting();
            AbrirMensaje("ATENCIÓN", obj.msg, function () {
                //todo fue bien, por lo que se deberia reinicializar la pantalla.
                var grilla = "";
                switch (tabAbm) {
                    case 1:
                        grilla = Grids.GridUser;
                        dataBak = "";
                        InicializaPantallaRepartidor(grilla);
                        break;

                    default:

                }

                switch (tabAbm) {
                    case 1:
                        if (accion !== AbmAction.BAJA) {
                            //se dió de alta o se modificó, se realiza la presentación del producto
                            if (accion === AbmAction.ALTA) {
                                EntidadSelect = obj.id;
                            }
                            //data = { p_id: EntidadSelect };
                            //buscarProductoServer(data);
                            InicializaFiltroAbmRepartidor(EntidadSelect);
                            $("#btnBuscar").trigger("click");
                            $("#divpanel01").empty();
                            //inicializamos la acción.
                            accion = "";
                        }
                        else {
                            //borramos el id del producto si se eliminó
                            EntidadSelect = "";
                            //VAMOS A EJECUTAR NUEVAMENTE EL BUSCAR
                            buscarRepartidores(pagina);
                        }
                        break;
                   
                    default:
                }




                $("#msjModal").modal("hide");
                //$("#msjModal").toggle();


            }, false, ["CONTINUAR"], "succ!", null);
        }
    });
}

function InicializaFiltroAbmRepartidor(id) {
    if ($("#chkDescr").is(":checked")) {
        $("#chkDescr").prop("checked", false);
        $("#Buscar").val("");
    }


    if (!$("#chkDesdeHasta").is(":checked")) {
        $("#chkDesdeHasta").prop("checked", true);
    }
    $("#Id").val(id);
    $("#Id2").val(id);

    //if ($("#chkRel01").is(":checked")) {
    //    $("#chkRel01").prop("checked", false);
    //    $("#Rel01").val("");
    //    $("#Rel01Item").val("");
    //    $("#Rel01List").empty();
    //}

    //if ($("#chkRel02").is(":checked")) {
    //    $("#chkRel02").prop("checked", false);
    //    $("#Rel02").val("");
    //    $("#Rel02Item").val("");
    //    $("#Rel02List").empty();
    //}
}
function analizaEstadoBtnDetalle() {
    tabMn = 1;
    InicializaPantallaRepartidor(Grids.GridRepartidor);
    return true;
}

function confirmarDatosTab01() {
    var rp_id = $("#rp_id").val();
    var rp_activo = "N";
    if ($("#RpActivo").is(":checked")) {
        rp_activo = "S";
    }

    var rp_nombre = $("#rp_nombre").val();
    var rp_comision = $("#rp_comision").val();
    var rp_mail = $("#rp_mail").val();
    var rp_celu = $("#rp_celu").val();

    var data = { rp_id, rp_activo, rp_nombre, rp_comision, rp_mail, rp_celu,accion };
    return data;
}