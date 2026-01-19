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
    $(document).on("click", "#PBalanza", controlaBalanza);
    $(document).on("click", "#PConVto", controlaVencimiento);
    $(document).on("click", "#PMatPri", controlaMateriaPrima);
    $(document).on("change", "#up_id", controlaValorUpId);
    $(document).on("change","#iva_situacion",controlaSituacionIva)
    $(document).on("change", "#iva_situacion", controlaValorIva);

});

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
            //accionBotones(AbmAction.ALTA);
            //inicializaControlesTab03();
            //activarControles(true);
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
    } else if (btn === AbmAction.SUBMIT || btn === AbmAction.CANCEL) {   // (S)uccess - (C)ancel
        $("#btnFiltro").prop("disabled", false);
        $("#btnDetalle").prop("disabled", false);

        $("#BtnLiTab01").prop("disabled", false);
        $("#BtnLiTab02").prop("disabled", false);
        $("#BtnLiTab03").prop("disabled", false);
        $("#BtnLiTab01").removeClass("text-danger");
        $("#BtnLiTab02").removeClass("text-danger");
        $("#BtnLiTab03").removeClass("text-danger");

        if (btn === AbmAction.ALTA) {

        }
        else if (btn === AbmAction.CANCEL) {

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
    //se cambia el sentido del valor act para que se interprete
    //que si se activa los controles sea SI = TRUE y cuando sea False es que se ponen todos los controles en disabled
    if (act === true || act === false) {
        act = !act; //se cambia el sentido del valor ya que con true, se activa el disabled.-
        //se tiene en cuenta que tab es el que esta activo al momento de presionar el boton de acción
        switch (tabAbm) {
            case 1:
                //Linea 1
                //p_id NUNCA SE ACTIVA
                if (accion === AbmAction.MODIFICACION) {
                    $("#p_activo").prop("disabled", act);
                }
                $("#up_id").prop("disabled", act);
                //Linea 02
                $("#p_m_marca").prop("disabled", act);

                //SI EL UP_ID ES DISTINTO DE 07 SE PUEDE ACTIVAR EL CHECK DE BALANZA
                if ($("#up_Id option:selected").val() !== "07") {
                    $("#PBalanza").prop("disabled", act);
                    if (act === false && $("#PBalanza").is(":checked")) {
                        $("#p_balanza_dvto").prop("disabled", act);
                        $("#p_peso").prop("disabled", act);
                    }
                }
                //Linea 03
                $("#p_m_desc").prop("disabled", act);
                $("#PConVto").prop("disabled", act);
                if (act === false && $("#PConVto").is(":checked")) {
                    $("#p_con_vto_min").prop("disabled", act);
                }
                //Linea 04
                $("#p_m_capacidad").prop("disabled", act);
                $("#PAltaRotacion").prop("disabled", act);
                //Linea 05
                $("#p_id_prov").prop("disabled", act);
                $("#PMatPri").prop("disabled", act);
                if (act === false && $("#PMatPri").is(":checked")) { //este campo se activa cuando el check PMatPri es TRUE
                    $("#PElaboracion").prop("disabled", act);
                }
                //Linea 06
                $("#cta_lista").prop("disabled", act);
                $("#AdmMayExcluye").prop("disabled", act);
                $("#AdmMinExcluye").prop("disabled", act);
                //Linea 07
                $("#pg_id").prop("disabled", act);
                $("#PiAutoExluye").prop("disabled", act);
                $("#OcAutoExluye").prop("disabled", act);
                //Linea 08
                $("#rub_lista").prop("disabled", act);
                $("#iva_situacion").prop("disabled", act);
                $("#iva_alicuota").prop("disabled", act);
                controlaValorIva();
                //Linea 09
                $("#lp_id_default").prop("disabled", act);
                $("#in_alicuota").prop("disabled", act);
                //Linea 10
                $("#p_id_barrado_ean").prop("disabled", act);
                $("#p_unidad_pres_ean").prop("disabled", act);
                $("#p_unidad_x_bulto_ean").prop("disabled", act);
                $("#p_bulto_x_piso_ean").prop("disabled", act);
                $("#p_piso_x_pallet_ean").prop("disabled", act);
                //Linea 11
                $("#p_id_barrado_dun").prop("disabled", act);
                $("#p_unidad_pres_dun").prop("disabled", act);
                $("#p_unidad_x_bulto_dun").prop("disabled", act);
                $("#p_bulto_x_piso_dun").prop("disabled", act);
                $("#p_piso_x_pallet_dun").prop("disabled", act);
                //Linea 12
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
                break;
            default:
                return false;
        }

    }
}

//se debe enviar que operacion se esta confirmando
//enviando todos los campos de la entidad

function confirmarOperacionAbmProducto() {
    AbrirWaiting("Completando proceso...");

    let data = {};
    let act = "";
    let urlabm = "";

    // Determinar datos y acción según la pestaña activa
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

    // Agregar la acción al objeto data
    data.accion = act;

    // DEBUG: Verifica qué se está enviando
    console.log("Datos a enviar:", data);
    console.log("JSON stringified:", JSON.stringify(data));
    console.log("URL destino:", urlabm);

    // Realizar la petición AJAX
    $.ajax({
        url: urlabm,
        type: 'POST',
        contentType: 'application/json; charset=utf-8',
        dataType: 'json',
        data: JSON.stringify(data),
        success: function (obj) {
            CerrarWaiting();
            console.log("Respuesta exitosa:", obj); // DEBUG
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
                        //si elimina un producto arranca de nuevo por ahora
                        $("#btnCancel").trigger("click");
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
            var esAltaOModif = (accion === AbmAction.ALTA || accion === AbmAction.MODIFICACION);
           
            switch (tabAbm) {
                case 1:
                    // Para alta o modificación
                    EntidadSelect = AbmAction == AbmAction.ALTA ? obj.id : $("#p_id").val();
                default:
            }

            // Éxito
            AbrirMensaje("ATENCIÓN", obj.msg, function () {
                const grilla = tabAbm === 1 ? tabGrid01 : (tabAbm === 2 ? tabGrid02 : tabGrid03);
                dataBak = "";
                InicializaPantallaAbmProd(grilla);
                
                    switch (tabAbm) {
                        case 1:
                            // Limpiar estado de la pantalla
                            $("#divDetalle").collapse("hide");
                            $("#divpanel01").empty();
                            buscarProductos(1, function () {
                                // Buscar la fila con el ID del perfil
                                var $fila = $("#" + grilla + " tbody tr").filter(function () {
                                    return $(this).find("td:first").text().trim() === EntidadSelect;
                                   
                                }).first();

                                // Si se encuentra la fila, solo marcarla visualmente
                                if ($fila.length > 0) {
                                    // Remover selección previa
                                    $("#" + grilla + " tbody tr").removeClass("selectedEdit-row");

                                    // Marcar la fila
                                    $fila.addClass("selected-row");

                                    // Posicionar en el tope si existe la función
                                    if (typeof posicionarRegOnTop === 'function') {
                                        posicionarRegOnTop($fila);
                                    }


                                    // Activar grilla y estado final
                                    activarGrilla(grilla);
                                    $("#btnDetalle").prop("disabled", false);
                                    activarBotones(true);
                                }

                                // Resetear acción
                                accionBotones(AbmAction.CANCEL);
                            });

                            break;
                        case 2:
                            presentarBarrado();
                            break;
                        case 3:
                            presentarLimites();
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

function confirmarDatosTab01() {
    // ===== LÍNEA 01 =====
    var p_id = $("#p_id").val() || "";
    var p_activo = $("#p_activo option:selected").val() || "S";
    var up_id = $("#up_id option:selected").val() || "";
    var up_lista = $("#up_id option:selected").text() || "";
    var up_desc = up_lista.replace(/\(.*\)/, "").trim();

    // ===== LÍNEA 02 =====
    var p_m_marca = $("#p_m_marca").val() || "";
    var p_balanza = $("#PBalanza").is(":checked") ? "S" : "N";
    var p_balanza_dvto = parseInt($("#p_balanza_dvto").val()) || 0;
    var p_peso = parseFloat($("#p_peso").val()) || 0;

    // ===== LÍNEA 03 =====
    var p_m_desc = $("#p_m_desc").val() || "";
    var p_desc = $("#p_desc").val() || "";
    var p_con_vto = $("#PConVto").is(":checked") ? "S" : "N";
    var p_con_vto_min = parseInt($("#p_con_vto_min").val()) || 0;

    // ===== LÍNEA 04 =====
    var p_m_capacidad = $("#p_m_capacidad").val() || "";
    var p_alta_rotacion = $("#PAltaRotacion").is(":checked") ? "S" : "N";

    // ===== LÍNEA 05 =====
    var p_id_prov = $("#p_id_prov").val() || "";
    var p_materia_prima = $("#PMatPri").is(":checked") ? "S" : "N";
    var p_elaboracion = $("#PElaboracion").is(":checked") ? "S" : "N";

    // ===== LÍNEA 06 =====
    var cta_id = $("#cta_id").val() || "";
    var cta_lista = $("#cta_lista").text() || "";
    var cta_denominacion = cta_lista.replace(/\(.*\)/, "").trim();
    var adm_may_excluye = $("#AdmMayExcluye").is(":checked") ? "S" : "N";
    var adm_min_excluye = $("#AdmMinExcluye").is(":checked") ? "S" : "N";

    // ===== LÍNEA 07 =====
    var pg_id = $("#pg_id option:selected").val() || "";
    var pg_lista = $("#pg_id option:selected").text() || "";
    var pg_desc = pg_lista.replace(/\(.*\)/, "").trim(); // ✅ FALTABA

    // ✅ CORRECCIÓN: "excluye" no "exluye"
    var pi_auto_excluye = $("#PiAutoExluye").is(":checked") ? "S" : "N";
    var oc_auto_excluye = $("#OcAutoExluye").is(":checked") ? "S" : "N";

    // ===== LÍNEA 08 =====
    var rub_id = $("#rub_id").val() || "";
    var rub_lista = $("#rub_lista").text() || "";
    var rub_desc = rub_lista.replace(/\(.*\)/, "").trim();
    var iva_situacion = $("#iva_situacion option:selected").val() || "N";
    var iva_alicuota = parseFloat($("#iva_alicuota option:selected").val()) || 0;

    // ===== LÍNEA 09 =====
    var lp_id_default = $("#lp_id_default option:selected").val() || "";
    var in_alicuota = parseFloat($("#in_alicuota").val()) || 0;

    // ===== LÍNEA 10 =====
    var p_obs = $("#p_obs").val() || "";
    var p_actu = $("#p_actu").val() || null;

    // ===== CAMPOS OCULTOS =====
    var p_balanza_id = $("#p_balanza_id").val() || "";

    // ✅ Convertir a enteros
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

    // ===== OBJETO DE RETORNO (Orden alfabético recomendado) =====
    var data = {
        accion,                  // ✅ Se agregará después en confirmarOperacionAbmProducto
        adm_may_excluye,
        adm_min_excluye,
        cta_denominacion,
        cta_id,
        cta_lista,
        in_alicuota,
        iva_alicuota,
        iva_situacion,
        lp_id_default,
        oc_auto_excluye,        // ✅ CORREGIDO
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
        p_m_capacidad,          // ✅ Sin duplicar
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
        pg_desc,                 // ✅ AGREGADO
        pg_id,
        pg_lista,
        pi_auto_excluye,        // ✅ CORREGIDO
        rub_desc,
        rub_id,
        rub_lista,
        up_desc,
        up_id,
        up_lista
    };

    return data;
}

function confirmarDatosTab02() {
    var p_id_barrado = $("#p_id_barrado").val();
    var p_unidad_pres = $("#p_unidad_pres").val();
    var p_unidad_x_bulto = $("#p_unidad_x_bulto").val();
    var p_bulto_x_piso = $("#p_bulto_x_piso").val();
    var p_piso_x_pallet = $("#p_piso_x_pallet").val();
    var tba_id = $("#tba_id").val();
    var tba_desc = $("#tba_id option:selected").text();
    var tba_lista = tba_desc + "(" + tba_id + ")";

    var data = {
        p_id_barrado, p_unidad_pres, p_unidad_x_bulto, p_bulto_x_piso,
        p_piso_x_pallet, tba_id, tba_lista, tba_desc, accion: accion02
    };

    return data;
}

function confirmarDatosTab03() {
    var adm_id = $("#adm_id option:selected").val();
    var adm_nombre = $("#adm_id option:selected").text();
    var adm_lista = adm_nombre + " (" + adm_id + ")";

    var p_stk_min = $("#p_stk_min").val();
    var p_stk_max = $("#p_stk_max").val();

    var data = { adm_id, adm_nombre, adm_lista, p_stk_min, p_stk_max, accion: accion03 };
    return data;
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