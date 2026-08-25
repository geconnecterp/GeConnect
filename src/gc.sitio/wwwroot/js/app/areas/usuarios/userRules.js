$(function () {
    //configuraciones

    $("#btnAbmNuevo").on("click", ejecutarAlta);
    $("#btnAbmModif").on("click", ejecutarModificacion);
    $("#btnAbmElimi").on("click", ejecutarBaja);

    $("#btnAbmCancelar").on("click", cancelarOperacionUsuario);
    $("#btnAbmAceptar").on("click", confirmarOperacionAbmUsuario);
    $(document).on("click", "#btnBlanquearClave", function () {
        confirmarOperacionSeguridadUsuario("BLANQUEAR");
    });
    $(document).on("click", "#btnDesbloquearUsuario", function () {
        confirmarOperacionSeguridadUsuario("DESBLOQUEAR");
    });

    // CORRECCIÓN: Cambiar de "mousedown" a "click" para mejor control
    $("#btnDetalle").on("click", function (e) {
        e.preventDefault();
        e.stopPropagation();

        var divVisible = $("#divDetalle").is(":visible");

        if (divVisible) {
            let grid = "";
            switch (tabAbm) {
                case 2:
                case 3:
                case 4:
                    grid = "otro"
                    break
                default:
                    grid = "tbGridUsers";
                    break;
            }
            // Si el detalle está visible, realizar cancelación
            InicializaPantallaUser(grid);
        } else {
            // Si no está visible, no hacer nada (se maneja en el dblclick)
            // El div se abrirá cuando se seleccione un usuario
        }
    });

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
    switch (tabAbm) {
        case 2:
            activarArbol("#divPerfiles", "#", true)
            break;
        case 3:
            activarArbol("#divAdmins", "#", true)
            break;
        case 4:
            activarArbol("#divDers", "#", true)
            break;
        default:
            break;
    }
}

function ejecutarAlta() {

    AbrirWaiting("Espere, se blanquea el formulario...");

    var data = {};
    switch (tabAbm) {
        case 1:
            PostGenHtml(data, nuevoUsuarioUrl, function (obj) {
                $("#divpanel01").html(obj);
                ////se procede a buscar la grilla de barrado
                //buscarBarrado(data);
                ////se procede a buscar la grilla de Sucursales
                //buscarLimite(data);

                $("#btnDetalle").prop("disabled", false);
                $("#divFiltro").collapse("hide");
                $("#divDetalle").collapse("show");
                $("#divGrilla, #divPaginacion").hide();

                accionBotones(AbmAction.ALTA);
                activarControles(true);

                CerrarWaiting();
            });

            break;

        default:
            return false;
    }
}

function controlaValorIva() {
    if ($("#iva_situacion option:selected").val() === "N") {
        $("#iva_alicuota").val("0.00");
        $("#iva_alicuota").prop("disabled", true);
    }
    else {
        $("#iva_alicuota").prop("disabled", false);
    }
}

function activarBotones(activar) {
    if (activar === true) {
        //el activarlos es activar BM
        $("#btnAbmModif").prop("disabled", false);
        if (tabAbm === 1) {
            $("#btnAbmNuevo").prop("disabled", false);
            $("#btnAbmElimi").prop("disabled", false);
        }
        else {
            $("#btnAbmNuevo").prop("disabled", true);
            $("#btnAbmElimi").prop("disabled", true);
        }

        $("#btnAbmAceptar").prop("disabled", true);
        $("#btnAbmCancelar").prop("disabled", true);
        $("#btnAbmAceptar").hide();
        $("#btnAbmCancelar").hide();
    }
    else {
        var hayListaUsuarios = $("#tbGridUsers").length > 0;
        $("#btnAbmNuevo").prop("disabled", !hayListaUsuarios);
        $("#btnAbmModif").prop("disabled", true);
        $("#btnAbmElimi").prop("disabled", true);

        $("#btnAbmAceptar").prop("disabled", true);
        $("#btnAbmCancelar").prop("disabled", true);
        $("#btnAbmAceptar").hide();
        $("#btnAbmCancelar").hide();
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
            case 4:
                accion04 = btn;
                break;
        }

        $("#btnFiltro").prop("disabled", true);
        $("#btnDetalle").prop("disabled", true);
        $("#BtnLiTab01").prop("disabled", true);
        $("#BtnLiTab02").prop("disabled", true);
        $("#BtnLiTab03").prop("disabled", true);
        $("#BtnLiTab04").prop("disabled", true);

        $("#btnAbmNuevo").prop("disabled", true);
        $("#btnAbmModif").prop("disabled", true);
        $("#btnAbmElimi").prop("disabled", true);

        $("#btnAbmAceptar").prop("disabled", false);
        $("#btnAbmCancelar").prop("disabled", false);
        $("#btnAbmAceptar").show();
        $("#btnAbmCancelar").show();
    } else if (btn === AbmAction.SUBMIT || btn === AbmAction.CANCEL) {   // (S)uccess - (C)ancel
        $("#btnFiltro").prop("disabled", false);
        $("#btnDetalle").prop("disabled", false);

        $("#BtnLiTab01").prop("disabled", false);
        $("#BtnLiTab02").prop("disabled", false);
        $("#BtnLiTab03").prop("disabled", false);
        $("#BtnLiTab04").prop("disabled", false);
        $("#BtnLiTab01").removeClass("text-danger");
        $("#BtnLiTab02").removeClass("text-danger");
        $("#BtnLiTab03").removeClass("text-danger");
        $("#BtnLiTab04").removeClass("text-danger");

        if (btn === AbmAction.ALTA) {

        }
        else if (btn === AbmAction.CANCEL) {

            activarBotones(false);
            activarControles(false);
            switch (tabAbm) {
                case 1:
                    $("#btnDetalle").prop("disabled", true);
                    activarGrilla(Grids.GridUser);
                    break;
                case 2:
                    $("#BtnLiTab02").trigger("click");

                    break;
                case 3:
                    $("#BtnLiTab03").trigger("click");
                    break;
                case 4:
                    $("#BtnLiTab04").trigger("click");
                    break;
                default:
            }
        }
    }
    actualizarAccionesSeguridadUsuario();
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
                if (accion == AbmAction.ALTA) {
                    $("#usu_id").prop("disabled", false);
                    $("#usu_bloqueado").prop("disabled", true);
                }
                //Linea 02
                $("#usu_apellidoynombre").prop("disabled", act);

                //Linea 03
                $("#tdoc_id").prop("disabled", act);
                $("#usu_documento").prop("disabled", act);

                //Linea 04
                $("#usu_email").prop("disabled", act);

                //Linea 05
                $("#usu_celu").prop("disabled", act);

                //Linea 06
                $("#cta_denominacion").prop("disabled", act);

                //hacemos el foco
                if (accion === AbmAction.ALTA) {
                    $("#usu_id").trigger("focus");
                }

                break;
            case 2:

                break;
            case 3:

                break;
            default:
                return false;
        }

    }
}

//se debe enviar que operacion se esta confirmando
//enviando todos los campos de la entidad

function confirmarOperacionAbmUsuario() {
    AbrirWaiting("Completando proceso...");
    var data = {};
    var operacionConfirmada = "";
    switch (tabAbm) {
        case 1:
            data = confirmarDatosTab01();
            if (!data) {
                CerrarWaiting();
                return false;
            }
            accion = data.accion;
            operacionConfirmada = accion;
            break;
        case 2:
            operacionConfirmada = accion02;
            data = confirmarDatosJsTree("#divPerfiles");
            break;
        case 3:
            operacionConfirmada = accion03;
            data = confirmarDatosJsTree("#divAdmins");
            break;
        case 4:
            operacionConfirmada = accion04;
            data = confirmarDatosJsTree("#divDers");
            break;
        default:
            return false;
    }
    urlabm = ""
    switch (tabAbm) {
        case 1:
            urlabm = confirmarAbmUsuarioUrl;
            break;
        case 2:
            urlabm = confirmarPerfsUserUrl;
            break;
        case 3:
            urlabm = confirmarAdmsUserUrl;
            break;
        case 4:
            urlabm = confirmarDersUserUrl;
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
            var tabConfirmado = tabAbm;
            var logon = ($("#usu_id").val() || "").trim().toLowerCase();
            if (operacionConfirmada === AbmAction.ALTA && obj.id) {
                logon = String(obj.id).trim().toLowerCase();
            }

            AbrirMensaje("ATENCIÓN", obj.msg, function () {
                $("#msjModal").modal("hide");

                switch (tabConfirmado) {
                    case 1:
                        finalizarAbmUsuario(operacionConfirmada, logon);
                        break;
                    case 2:
                        accion02 = "";
                        finalizarRelacionUsuario("#divPerfiles", presentaPerfilesUsuario);
                        break;
                    case 3:
                        accion03 = "";
                        finalizarRelacionUsuario("#divAdmins", presentaAdministracionesUsuario);
                        break;
                    case 4:
                        accion04 = "";
                        finalizarRelacionUsuario("#divDers", presentaDerechosUsuario);
                        break;
                }
            }, false, ["CONTINUAR"], "succ!", null);
        }
    });
}

function finalizarAbmUsuario(operacion, logon) {
    accion = "";
    dataBak = "";
    InicializaPantallaUser(Grids.GridUser);

    if (operacion !== AbmAction.BAJA && logon) {
        $("#Id").val(logon);
        $("#Id2").val(logon);
    }

    buscarUsers(1, function () {
        if (operacion === AbmAction.BAJA || !logon) {
            return;
        }

        var $fila = $("#" + Grids.GridUser + " tbody tr").filter(function () {
            return $(this).find("td:first").text().trim().toLowerCase() === logon;
        }).first();

        if ($fila.length) {
            $("#" + Grids.GridUser + " tbody tr").removeClass("selected-row selectedEdit-row");
            $fila.addClass("selected-row");
            regSelected = $fila;
            usuSelect = logon;
            EntidadSelect = logon;
            posicionarRegOnTop($fila);
        }
    });
}

function finalizarRelacionUsuario(div, recargar) {
    $("#btnAbmAceptar, #btnAbmCancelar").prop("disabled", true).hide();
    $("#btnFiltro, #btnDetalle, #BtnLiTab01, #BtnLiTab02, #BtnLiTab03, #BtnLiTab04")
        .prop("disabled", false)
        .removeClass("text-danger");
    activarBotones(true);
    desactivarGrilla(Grids.GridUser);
    recargar();
}

function InicializaFiltroAbmUsuario(id) {
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

function confirmarDatosTab01() {
    //linea 01
    var usu_id = ($("#usu_id").val() || "").trim().toLowerCase();
    var usu_bloqueado = false;
    if ($("#usu_bloqueado").is(":checked")) {
        usu_bloqueado = true;
    }

    //linea 02
    var usu_apellidoynombre = ($("#usu_apellidoynombre").val() || "").trim();

    //linea 03
    var tdoc_id = $("#tdoc_id option:selected").val();
    var usu_documento = ($("#usu_documento").val() || "").trim();

    //linea 04
    var usu_email = ($("#usu_email").val() || "").trim().toLowerCase();

    //Linea 05
    var usu_celu = ($("#usu_celu").val() || "").trim();

    //Linea 06
    var cta_id = $("#cta_id").val();
    var cta_denominacion = ($("#cta_denominacion").val() || "").trim();

    if (!usu_id || usu_id.length > 10) {
        mostrarValidacionUsuario("El Logon es obligatorio y admite hasta 10 caracteres.", "#usu_id");
        return null;
    }

    if (accion !== AbmAction.BAJA && (!usu_apellidoynombre || usu_apellidoynombre.length > 50)) {
        mostrarValidacionUsuario("Apellido y Nombre es obligatorio y admite hasta 50 caracteres.", "#usu_apellidoynombre");
        return null;
    }

    if (usu_email && !/^[^\s@]+@[^\s@]+\.[^\s@]+$/.test(usu_email)) {
        mostrarValidacionUsuario("Ingrese un email válido.", "#usu_email");
        return null;
    }

    if (cta_denominacion && !cta_id) {
        mostrarValidacionUsuario("Seleccione el cliente desde la lista de búsqueda.", "#cta_denominacion");
        return null;
    }

    $("#usu_id").val(usu_id);
    $("#usu_email").val(usu_email);



    var data = {
        usu_id,//
        usu_bloqueado,//
        usu_apellidoynombre,//
        tdoc_id,
        usu_documento,
        usu_email,//
        usu_celu,//
        cta_id,
        cta_denominacion,//        
        accion
    };

    return data;
}

function mostrarValidacionUsuario(mensaje, selector) {
    AbrirMensaje("ATENCIÓN", mensaje, function () {
        $("#msjModal").modal("hide");
        $(selector).trigger("focus");
    }, false, ["CONTINUAR"], "warn!", null);
}

function actualizarAccionesSeguridadUsuario() {
    const usuarioActual = (typeof usuarioAuth === "undefined" ? "" : String(usuarioAuth)).trim().toLowerCase();
    const seleccionado = String(usuSelect || $("#usu_id").val() || "").trim().toLowerCase();
    const operacionAbmActiva = accion !== "" || accion02 !== "" || accion03 !== "" || accion04 !== "";
    const puedeOperar = seleccionado && seleccionado !== usuarioActual && !operacionAbmActiva;
    const bloqueado = $("#usu_bloqueado").is(":checked");
    // Durante un despliegue pueden convivir temporalmente una vista compilada anterior y
    // los archivos estáticos nuevos. En ese caso las acciones deben fallar cerradas, sin
    // interrumpir la carga de la ficha ni dejar abierto el indicador de espera.
    const permiteBlanquear = typeof puedeBlanquearClave !== "undefined" && puedeBlanquearClave === true;
    const permiteDesbloquear = typeof puedeDesbloquearUsuario !== "undefined" && puedeDesbloquearUsuario === true;

    $("#btnBlanquearClave")
        .prop("hidden", !permiteBlanquear)
        .prop("disabled", !(puedeOperar && permiteBlanquear));
    $("#btnDesbloquearUsuario")
        .prop("hidden", !permiteDesbloquear)
        .prop("disabled", !(puedeOperar && permiteDesbloquear && bloqueado));
}

function confirmarOperacionSeguridadUsuario(operacion) {
    const usuario = String(usuSelect || $("#usu_id").val() || "").trim();
    if (!usuario) {
        ControlaMensajeWarning("Seleccione un usuario para realizar la operación.");
        return;
    }

    const esBlanqueo = operacion === "BLANQUEAR";
    const titulo = esBlanqueo ? "Blanquear contraseña" : "Desbloquear usuario";
    const mensaje = esBlanqueo
        ? `¿Confirma el blanqueo de la contraseña del usuario ${usuario}? Deberá definir una contraseña nueva en su próximo ingreso.`
        : `¿Confirma el desbloqueo del usuario ${usuario}?`;

    AbrirMensaje(titulo, mensaje, function (respuesta) {
        $("#msjModal").modal("hide");
        // El modal compartido devuelve "SI" para su botón principal,
        // independientemente del texto visible configurado para ese botón.
        if (respuesta !== "SI") return;

        AbrirWaiting(esBlanqueo ? "Blanqueando contraseña..." : "Desbloqueando usuario...");
        PostGen({ usuId: usuario }, esBlanqueo ? blanquearClaveUrl : desbloquearUsuarioUrl, function (obj) {
            CerrarWaiting();
            if (obj.error) { ControlaMensajeError(obj.msg); return; }
            if (obj.warn) { ControlaMensajeWarning(obj.msg); return; }

            ControlaMensajeSuccess(obj.msg);
            if (!esBlanqueo) {
                $("#usu_bloqueado").prop("checked", false);
                const $fila = $("#tbGridUsers tbody tr").filter(function () {
                    return $(this).find("td:first").text().trim().toLowerCase() === usuario.toLowerCase();
                }).first();
                $fila.find("td:nth-child(3)").text("NO");
            }
            actualizarAccionesSeguridadUsuario();
        });
    }, true, ["Aceptar", "Cancelar"], "question!", null);
}

function confirmarDatosJsTree(div) {

    var json = JSON.stringify($(div).jstree(true).get_json());

    return { json, usuId: usuSelect };
}



function inicializaControlesTab02() {
    $("#p_id_barrado").val("");
    $("#p_unidad_pres").val(0);
    $("#p_unidad_x_bulto").val(0);
    $("#p_bulto_x_piso").val(0);
    $("#p_piso_x_pallet").val(0);
    $("#tba_id").val("");

    if (!$("#tab2l1").is(":visible")) {
        $("#tab2l1").show();
        $("#tab2l2").show();
    }
}

function inicializaControlesTab03() {
    $("#adm_id").val("");
    $("#p_stk_min").val(0);
    $("#p_stk_max").val(0);

    if (!$("#tab3l1").is(":visible")) {
        $("#tab3l1").show();
    }
}

function activarArbol(div, node, activar, cancelar = false) {
    var nodo = $(div).jstree().get_node(node);
    if (activar === true) {
        $(div).jstree(true).enable_node(nodo);
        nodo.children.forEach(function (child_id) {
            activarArbol(div, child_id, activar);
        });
    }
    else {
        $(div).jstree(true).disable_node(nodo);
        //al cancelar se debe restituir los valores por defecto
        if (cancelar === true && nodo.id !== "00" && nodo.id !== "#") {
            if (nodo.data.asignado === true) {
                $(div).jstree(true).select_node(nodo);
            }
            else {
                $(div).jstree(true).deselect_node(node);
            }
        }
        nodo.children.forEach(function (child_id) {
            activarArbol(div, child_id, activar, cancelar);
        });
    }
}
