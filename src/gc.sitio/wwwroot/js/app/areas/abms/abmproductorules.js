$(function () {
    //configuraciones
    $("#lbRel01").text("PROVEEDOR");
    $("#lbRel02").text("RUBRO");

    $("#btnAbmNuevo").on("click", ejecutarAlta);
    $("#btnAbmModif").on("click", ejecutarModificacion);
    $("#btnAbmElimi").on("click", ejecutarBaja);

    $("#btnAbmCancelar").on("click", InicializaPantallaAbmProd);
    $("#btnAbmAceptar").on("click", confirmarOperacionAbmProducto);

    //balanza
    $(document).off("click", "#PBalanza").on("click", "#PBalanza", controlaBalanza);
    $(document).off("click", "#PConVto").on("click", "#PConVto", controlaVencimiento);
    $(document).off("click", "#PMatPri").on("click", "#PMatPri", controlaMateriaPrima);
    $(document).off("change", "#up_id").on("change", "#up_id", controlaValorUpId);
    $(document).off("change", "#iva_situacion").on("change","#iva_situacion",controlaSituacionIva)
});

//function controlaAplicarTodas() {
//    if ($("#aplica_todas").is(":checked")) {
//        $("#adm_id").prop("disabled", true);
//    }
//    else {
//        $("#adm_id").prop("disabled", false);
//    }
//}

function ejecutarBaja() {
    switch (tabAbm) {
        case 1:
        case 2:
            $("#divFiltro").collapse("hide");
            accionBotones(AbmAction.BAJA);
            break;
        case 3:
            AbrirMensaje("ATENCIÓN!", "No puede realizar la Baja de ningún Límite de Stock de producto. Solo puede Modificarlo.", function () {
                $("#msjModal").modal("hide");
            }, false, ["ACEPTAR"], "warn!", null);
            break;
        default:
            return false;
    }
}

function ejecutarModificacion() {
    $("#divFiltro").collapse("hide");
    accionBotones(AbmAction.MODIFICACION);
    activarControles(true);
}

function ejecutarAlta() {

    AbrirWaiting("Espere, se blanquea el formulario...");

    var data = {};
    switch (tabAbm) {
        case 1:
            PostGenHtml(data, nuevoProductoUrl, function (obj) {
                $("#divpanel01").html(obj);
                $("#btnDetalle").prop("disabled", false);
                $("#divFiltro").collapse("hide");
                $("#divDetalle").collapse("show");

                accionBotones(AbmAction.ALTA);
                activarControles(true);
                CerrarWaiting();
            });
            break;
        case 2:
            accionBotones(AbmAction.ALTA);
            inicializaControlesTab02();
            activarControles(true);
            CerrarWaiting();
            break;
        case 3:
            AbrirMensaje("ATENCIÓN!", "No puede realizar el alta de ningún Límite de Stock de producto. Solo puede Modificarlo.", function () {
                $("#msjModal").modal("hide");
            }, false, ["ACEPTAR"], "warn!", null);
            CerrarWaiting();
            break;
        default:
            return false;
    }
}

function controlaBalanza() {
    if ($(this).is(":checked")) {
        $("#p_balanza_dvto").prop("disabled", false);
        $("#p_peso").prop("disabled", false);
    }
    else {
        $("#p_balanza_dvto").prop("disabled", true);
        $("#p_peso").prop("disabled", true);
    }
}

function controlaVencimiento() {
    if ($(this).is(":checked")) {
        $("#p_con_vto_min").prop("disabled", false);
    }
    else {
        $("#p_con_vto_min").prop("disabled", true);
    }
}

function controlaMateriaPrima() {
    if ($(this).is(":checked")) {
        $("#PElaboracion").prop("disabled", false);
    }
    else {
        $("#PElaboracion").prop("disabled", true);
    }
}

function controlaSituacionIva() {
    if ($("#iva_situacion option:selected").val() === "G") {
        $("#iva_alicuota").prop("disabled", false);      
    }
    else {
        $("#iva_alicuota").prop("disabled", true);
        $("#iva_alicuota").val("0.00");
    }
}

function controlaValorUpId() {
    if ($("#up_id option:selected").val() !== "07") {
        $("#PBalanza").prop("disabled", false);
    }
    else {
        $("#PBalanza").prop("checked", false);
        $("#PBalanza").prop("disabled", true);
    }
}

function activarBotones(activar) {
    if (activar === true) {
        $("#btnAbmNuevo").prop("disabled", false);
        $("#btnAbmModif").prop("disabled", false);
        $("#btnAbmElimi").prop("disabled", false);

        $("#btnAbmAceptar").prop("disabled", true);
        $("#btnAbmCancelar").prop("disabled", true);
        $("#btnAbmAceptar").hide();
        $("#btnAbmCancelar").hide();
    }
    else {
        $("#btnAbmNuevo").prop("disabled", false);
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
        }

        $("#btnFiltro").prop("disabled", true);
        $("#btnDetalle").prop("disabled", true);
        $("#BtnLiTab01").prop("disabled", true);
        $("#BtnLiTab02").prop("disabled", true);
        $("#BtnLiTab03").prop("disabled", true);

        $("#btnAbmNuevo").prop("disabled", true);
        $("#btnAbmModif").prop("disabled", true);
        $("#btnAbmElimi").prop("disabled", true);

        $("#btnAbmAceptar").prop("disabled", false);
        $("#btnAbmCancelar").prop("disabled", false);
        $("#btnAbmAceptar").show();
        $("#btnAbmCancelar").show();
    } else if (btn === AbmAction.SUBMIT || btn === AbmAction.CANCEL) {
        $("#btnFiltro").prop("disabled", false);
        $("#btnDetalle").prop("disabled", false);

        $("#BtnLiTab01").prop("disabled", false);
        $("#BtnLiTab02").prop("disabled", false);
        $("#BtnLiTab03").prop("disabled", false);
        $("#BtnLiTab01").removeClass("text-danger");
        $("#BtnLiTab02").removeClass("text-danger");
        $("#BtnLiTab03").removeClass("text-danger");

        if (btn === AbmAction.CANCEL) {
            activarBotones(false);
            activarControles(false);

            if (tabAbm === 1) {
                accion = "";
                $("#btnDetalle").prop("disabled", true);
                activarGrilla(tabGrid01);

                $("#BtnLiTab02").prop("disabled", false);
                $("#BtnLiTab02").removeClass("text-danger");
                $("#BtnLiTab03").prop("disabled", false);
                $("#BtnLiTab03").removeClass("text-danger");
            }
        }
    }
}

function activarControles(act) {
    if (act === true || act === false) {
        act = !act;
        switch (tabAbm) {
            case 1:
                if (accion === AbmAction.MODIFICACION) {
                    $("#p_activo").prop("disabled", act);
                }
                $("#up_id").prop("disabled", act);
                $("#p_m_marca").prop("disabled", act);

                if ($("#up_Id option:selected").val() !== "07") {
                    $("#PBalanza").prop("disabled", act);
                    if (act === false && $("#PBalanza").is(":checked")) {
                        $("#p_balanza_dvto").prop("disabled", act);
                        $("#p_peso").prop("disabled", act);
                    }
                }
                $("#p_m_desc").prop("disabled", act);
                $("#PConVto").prop("disabled", act);
                if (act === false && $("#PConVto").is(":checked")) {
                    $("#p_con_vto_min").prop("disabled", act);
                }
                $("#p_m_capacidad").prop("disabled", act);
                $("#PAltaRotacion").prop("disabled", act);
                $("#p_id_prov").prop("disabled", act);
                $("#PMatPri").prop("disabled", act);
                if (act === false && $("#PMatPri").is(":checked")) {
                    $("#PElaboracion").prop("disabled", act);
                }
                $("#cta_lista").prop("disabled", act);
                $("#AdmMayExcluye").prop("disabled", act);
                $("#AdmMinExcluye").prop("disabled", act);
                $("#pg_id").prop("disabled", act);
                $("#PiAutoExluye").prop("disabled", act);
                $("#OcAutoExluye").prop("disabled", act);
                $("#rub_lista").prop("disabled", act);
                $("#iva_situacion").prop("disabled", act);
                $("#iva_alicuota").prop("disabled", act);
                controlaSituacionIva();
                $("#lp_id_default").prop("disabled", act);
                $("#in_alicuota").prop("disabled", act);
                $("#p_id_barrado_ean").prop("disabled", act);
                $("#p_unidad_pres_ean").prop("disabled", act);
                $("#p_unidad_x_bulto_ean").prop("disabled", act);
                $("#p_bulto_x_piso_ean").prop("disabled", act);
                $("#p_piso_x_pallet_ean").prop("disabled", act);
                $("#p_id_barrado_dun").prop("disabled", act);
                $("#p_unidad_pres_dun").prop("disabled", act);
                $("#p_unidad_x_bulto_dun").prop("disabled", act);
                $("#p_bulto_x_piso_dun").prop("disabled", act);
                $("#p_piso_x_pallet_dun").prop("disabled", act);
                $("#p_obs").prop("disabled", act);

                //hacemos el foco
                if (accion === AbmAction.ALTA) {
                    $("#up_id").trigger("focus");
                }

                break;
            case 2:
                //la clave del barrado no se habilita
                if (accion02 === AbmAction.ALTA) {
                    $("#p_id_barrado").prop("disabled", act);
                }
                else {
                    $("#p_id_barrado").prop("disabled", true);
                }
                $("#tba_id").prop("disabled", act);
                $("#p_unidad_pres").prop("disabled", act);
                $("#p_unidad_x_bulto").prop("disabled", act);
                $("#p_bulto_x_piso").prop("disabled", act);
                $("#p_piso_x_pallet").prop("disabled", act);
                break;
            case 3:
                //solo se puede modificar.
                $("#adm_id").prop("disabled", true);
                $("#p_stk_min").prop("disabled", act);
                $("#p_stk_max").prop("disabled", act);
                $("#aplica_todas").prop("disabled", act);
                break;
            default:
                return false;
        }

    }
}

/**
 * ✅ OPTIMIZACIÓN: Función principal de confirmación con manejo mejorado de scroll
 */
function confirmarOperacionAbmProducto(e) {
    AbrirWaiting("Completando proceso...");

    let data = {};
    let act = "";
    let urlabm = "";

    switch (tabAbm) {
        case 1:
            data = confirmarDatosTab01();
            act = accion;
            urlabm = confirmarAbmProductoUrl;
            break;
        case 2:
            data = confirmarDatosTab02();
            act = accion02;
            urlabm = confirmarAbmBarradoUrl;
            break;
        case 3:
            data = confirmarDatosTab03();
            act = accion03;
            urlabm = confirmarAbmLimiteUrl;
            break;
        default:
            CerrarWaiting();
            return false;
    }

    data.accion = act;

    $.ajax({
        url: urlabm,
        type: 'POST',
        contentType: 'application/json; charset=utf-8',
        dataType: 'json',
        data: JSON.stringify(data),
        success: function (obj) {
            CerrarWaiting();
            
            if (obj.error === true) {
                AbrirMensaje("ALGO NO SALIO BIEN!", obj.msg, function () {
                    $("#msjModal").modal("hide");
                }, false, ["CONTINUAR"], "error!", null);
                return;
            }

            if (obj.warn === true) {
                AbrirMensaje("ATENCIÓN", obj.msg, function () {
                    if (obj.auth === true) {
                        window.location.href = login;
                    } else {
                        $("#msjModal").modal("hide");
                    }
                }, false, ["CONTINUAR"], "warn!", null);
                return;
            }

            if (accion === AbmAction.BAJA) {
                AbrirMensaje("ATENCIÓN", obj.msg, function () {
                    switch (tabAbm) {
                        case 1:
                            $("#btnCancel").trigger("click");
                            break;
                        case 2:
                            presentarBarrado();
                            break;
                        case 3:
                            presentarLimites();
                            break;
                    }
                }, false, ["CONTINUAR"], "succ!", null);
                return;
            }

            // Para alta o modificación
            let idEnt = "";
            switch (tabAbm) {
                case 1:
                    EntidadSelect = accion === AbmAction.ALTA ? obj.id : $("#p_id").val();
                    break;
                case 2:
                    idEnt = $("#p_id_barrado").val();
                    break;
                case 3:
                    idEnt = $("#adm_id option:selected").val();
                    break;
            }

            AbrirMensaje("ATENCIÓN", obj.msg, function () {
                const grilla = tabAbm === 1 ? tabGrid01 : (tabAbm === 2 ? tabGrid02 : tabGrid03);
                dataBak = "";
                
                // ✅ SOLUCIÓN: Usar requestAnimationFrame para asegurar que el DOM esté completamente renderizado
                switch (tabAbm) {
                    case 1:
                        procesarConfirmacionTab01(grilla);
                        break;
                    case 2:
                    case 3:
                        procesarConfirmacionTab2y3(grilla, idEnt, e);
                        break;
                }
                
                accion = "";
                $("#msjModal").modal("hide");
            }, false, ["CONTINUAR"], "succ!", null);
        },
        error: function (xhr, status, error) {
            CerrarWaiting();
            console.error("Error completo:", {
                status: xhr.status,
                statusText: xhr.statusText,
                responseText: xhr.responseText,
                error: error
            });

            const errorMsg = xhr.responseJSON?.msg || "Error al procesar la solicitud. Por favor, intente nuevamente.";
            AbrirMensaje("ERROR", errorMsg, function () {
                $("#msjModal").modal("hide");
            }, false, ["CONTINUAR"], "error!", null);
        }
    });
}

/**
 * ✅ OPTIMIZACIÓN: Procesar confirmación del Tab 1 con control de scroll mejorado
 */
function procesarConfirmacionTab01(grilla) {
    $("#divDetalle").collapse("hide");
    $("#divpanel01").empty();
    
    buscarProductos(1, function () {
        // Usar setTimeout para asegurar que el DOM se haya actualizado completamente
        setTimeout(function() {
            const $fila = $("#" + grilla + " tbody tr").filter(function () {
                return $(this).find("td:first").text().trim() === EntidadSelect;
            }).first();

            if ($fila.length > 0) {
                // Remover selección previa
                $("#" + grilla + " tbody tr").removeClass("selectedEdit-row selected-row");

                // Marcar la fila
                $fila.addClass("selected-row");

                // ✅ CLAVE: Usar requestAnimationFrame para posicionamiento suave
                requestAnimationFrame(function() {
                    posicionarRegOnTopMejorado($fila, ".table-wrapper");
                    
                    // ✅ Segundo frame para asegurar estabilidad visual
                    requestAnimationFrame(function() {
                        activarGrilla(grilla);
                        $("#btnDetalle").prop("disabled", false);
                        activarBotones(true);
                    });
                });
            }

            accionBotones(AbmAction.CANCEL);
        }, 150); // Delay mínimo para asegurar renderizado
    });
}

/**
 * ✅ OPTIMIZACIÓN: Procesar confirmación de Tabs 2 y 3 con control de scroll mejorado
 */
function procesarConfirmacionTab2y3(grilla, idEnt, e) {
    Entidad2Select = idEnt;
    InicializaPantallaAbmProd(grilla);
    
    const callback = function () {
        setTimeout(function() {
            const selector = tabAbm === 2 
                ? "#divBarrado2 table#" + grilla + " tbody tr"
                : "#divLimite2 table#" + grilla + " tbody tr";
            
            const $fila = $(selector).filter(function () {
                return $(this).find("td:first").text().trim() === Entidad2Select;
            }).first();

            if ($fila.length > 0) {
                $(selector).removeClass("selectedEdit-row selected-row");
                $fila.addClass("selected-row");

                // ✅ Prevenir propagación de eventos de scroll
                if (e) {
                    e.stopPropagation();
                    e.preventDefault();
                }

                requestAnimationFrame(function() {
                    posicionarRegOnTopMejorado($fila, ".table-wrapper");
                    
                    requestAnimationFrame(function() {
                        $("#btnDetalle").prop("disabled", false);
                        activarBotones(true);

                        const data = { p_id: EntidadSelect };
                        if (tabAbm === 2) {
                            buscarBarrado(data);
                        } else {
                            buscarLimite(data);
                        }
                    });
                });
            }

            accionBotones(AbmAction.CANCEL);
        }, 150);
    };

    if (tabAbm === 2) {
        presentarBarrado(callback);
    } else {
        presentarLimites(callback);
    }
}

function InicializaFiltroAbmProducto(id) {
    if ($("#chkDescr").is(":checked")) {
        $("#chkDescr").prop("checked", false);
        $("#Buscar").val("");
    }

    if (!$("#chkDesdeHasta").is(":checked")) {
        $("#chkDesdeHasta").prop("checked", true);
    }
    $("#Id").val(id);
    $("#Id2").val(id);

    if ($("#chkRel01").is(":checked")) {
        $("#chkRel01").prop("checked", false);
        $("#Rel01").val("");
        $("#Rel01Item").val("");
        $("#Rel01List").empty();
    }

    if ($("#chkRel02").is(":checked")) {
        $("#chkRel02").prop("checked", false);
        $("#Rel02").val("");
        $("#Rel02Item").val("");
        $("#Rel02List").empty();
    }
}

/**
 * ✅ Recopila datos del Tab 1 (Producto)
 */
function confirmarDatosTab01() {
    var p_id = $("#p_id").val() || "";
    var p_activo = $("#p_activo option:selected").val() || "S";
    var up_id = $("#up_id option:selected").val() || "";
    var up_lista = $("#up_id option:selected").text() || "";
    var up_desc = up_lista.replace(/\(.*\)/, "").trim();

    var p_m_marca = $("#p_m_marca").val() || "";
    var p_balanza = $("#PBalanza").is(":checked") ? "S" : "N";
    var p_balanza_dvto = parseInt($("#p_balanza_dvto").val()) || 0;
    var p_peso = parseFloat($("#p_peso").val()) || 0;

    var p_m_desc = $("#p_m_desc").val() || "";
    var p_desc = $("#p_desc").val() || "";
    var p_con_vto = $("#PConVto").is(":checked") ? "S" : "N";
    var p_con_vto_min = parseInt($("#p_con_vto_min").val()) || 0;

    var p_m_capacidad = $("#p_m_capacidad").val() || "";
    var p_alta_rotacion = $("#PAltaRotacion").is(":checked") ? "S" : "N";

    var p_id_prov = $("#p_id_prov").val() || "";
    var p_materia_prima = $("#PMatPri").is(":checked") ? "S" : "N";
    var p_elaboracion = $("#PElaboracion").is(":checked") ? "S" : "N";

    var cta_id = $("#cta_id").val() || "";
    var cta_lista = $("#cta_lista").text() || "";
    var cta_denominacion = cta_lista.replace(/\(.*\)/, "").trim();
    var adm_may_excluye = $("#AdmMayExcluye").is(":checked") ? "S" : "N";
    var adm_min_excluye = $("#AdmMinExcluye").is(":checked") ? "S" : "N";

    var pg_id = $("#pg_id option:selected").val() || "";
    var pg_lista = $("#pg_id option:selected").text() || "";
    var pg_desc = pg_lista.replace(/\(.*\)/, "").trim();

    var pi_auto_excluye = $("#PiAutoExluye").is(":checked") ? "S" : "N";
    var oc_auto_excluye = $("#OcAutoExluye").is(":checked") ? "S" : "N";

    var rub_id = $("#rub_id").val() || "";
    var rub_lista = $("#rub_lista").text() || "";
    var rub_desc = rub_lista.replace(/\(.*\)/, "").trim();
    var iva_situacion = $("#iva_situacion option:selected").val() || "N";
    var iva_alicuota = parseFloat($("#iva_alicuota option:selected").val()) || 0;

    var lp_id_default = $("#lp_id_default option:selected").val() || "";
    var in_alicuota = parseFloat($("#in_alicuota").val()) || 0;

    var p_obs = $("#p_obs").val() || "";
    var p_actu = $("#p_actu").val() || null;

    var p_balanza_id = $("#p_balanza_id").val() || "";

    var p_id_barrado_ean = $("#p_id_barrado_ean").val() || "";
    var p_unidad_pres_ean = parseInt($("#p_unidad_pres_ean").val()) || 0;
    var p_unidad_x_bulto_ean = parseInt($("#p_unidad_x_bulto_ean").val()) || 0;
    var p_bulto_x_piso_ean = parseInt($("#p_bulto_x_piso_ean").val()) || 0;
    var p_piso_x_pallet_ean = parseInt($("#p_piso_x_pallet_ean").val()) || 0;

    var p_id_barrado_dun = $("#p_id_barrado_dun").val() || "";
    var p_unidad_pres_dun = parseInt($("#p_unidad_pres_dun").val()) || 0;
    var p_unidad_x_bulto_dun = parseInt($("#p_unidad_x_bulto_dun").val()) || 0;
    var p_bulto_x_piso_dun = parseInt($("#p_bulto_x_piso_dun").val()) || 0;
    var p_piso_x_pallet_dun = parseInt($("#p_piso_x_pallet_dun").val()) || 0;

    return {
        adm_may_excluye,
        adm_min_excluye,
        cta_denominacion,
        cta_id,
        cta_lista,
        in_alicuota,
        iva_alicuota,
        iva_situacion,
        lp_id_default,
        oc_auto_excluye,
        p_actu,
        p_activo,
        p_alta_rotacion,
        p_balanza,
        p_balanza_dvto,
        p_balanza_id,
        p_bulto_x_piso_dun,
        p_bulto_x_piso_ean,
        p_con_vto,
        p_con_vto_min,
        p_desc,
        p_elaboracion,
        p_id,
        p_id_barrado_dun,
        p_id_barrado_ean,
        p_id_prov,
        p_m_capacidad,
        p_m_desc,
        p_m_marca,
        p_materia_prima,
        p_obs,
        p_peso,
        p_piso_x_pallet_dun,
        p_piso_x_pallet_ean,
        p_unidad_pres_dun,
        p_unidad_pres_ean,
        p_unidad_x_bulto_dun,
        p_unidad_x_bulto_ean,
        pg_desc,
        pg_id,
        pg_lista,
        pi_auto_excluye,
        rub_desc,
        rub_id,
        rub_lista,
        up_desc,
        up_id,
        up_lista
    };
}

/**
 * ✅ Recopila datos del Tab 2 (Barrado)
 */
function confirmarDatosTab02() {
    var p_id_barrado = $("#p_id_barrado").val();
    var p_unidad_pres = $("#p_unidad_pres").val();
    var p_unidad_x_bulto = $("#p_unidad_x_bulto").val();
    var p_bulto_x_piso = $("#p_bulto_x_piso").val();
    var p_piso_x_pallet = $("#p_piso_x_pallet").val();
    var tba_id = $("#tba_id").val();
    var tba_desc = $("#tba_id option:selected").text();
    var tba_lista = tba_desc + "(" + tba_id + ")";

    return {
        p_id_barrado, 
        p_unidad_pres, 
        p_unidad_x_bulto, 
        p_bulto_x_piso,
        p_piso_x_pallet, 
        tba_id, 
        tba_lista, 
        tba_desc
    };
}

/**
 * ✅ Recopila datos del Tab 3 (Límite de Stock)
 */
function confirmarDatosTab03() {
    let adm_id = $("#adm_id option:selected").val();
    let adm_nombre = $("#adm_id option:selected").text();
    let adm_lista = adm_nombre + " (" + adm_id + ")";

    var p_stk_min = $("#p_stk_min").val();
    var p_stk_max = $("#p_stk_max").val();
    let aplica_todas = $("#aplica_todas").is(":checked");

    return { 
        adm_id, 
        adm_nombre, 
        adm_lista, 
        p_stk_min, 
        p_stk_max, 
        aplica_todas 
    };
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