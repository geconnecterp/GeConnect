var caja_nro_proceso_selected = null;
var caja_nro_cierre_selected = null;
var caja_id_selected = null;
var cierre_pendientes_bool = null;
var caja_nro_rend_selected = null;
var tcf_id_selected = null;
var rend_pendiente_selected = null;
var existe_edicion = false;
var fila_seleccionada_actual = null;
var fila_cierre_seleccionada_actual = null;
var guardando_importe = false;


$(function () {
    if ($("#divDetalle").is(":visible")) {
        $("#divDetalle").collapse("hide");
    }
    $("#divFiltro").collapse("show");

    $("#btnCancel").on("click", function () {
        window.location.href = homeCtlValoresUrl;
    });

    $("#lbSucursales").text("Sucursal"); 
    $("#lbDias").text("Día"); 

    $("#btnBuscar").on("click", function () {
        if (validarCamposSeleccionados()) {
            InicializarBusqueda();
        } else {
            AbrirMensaje("ATENCIÓN", "Debe seleccionar Sucursal y Día.", function () {
                $("#msjModal").modal("hide");
                return true;
            }, false, ["Aceptar"], "error!", null);
        }
    });

    $("#chkDias").on("click", function () {
        if ($("#chkDias").is(":checked")) {
            $("#listaDias").prop("disabled", false);
            $("#listaDias").trigger("focus");
        }
        else {
            $("#listaDias").prop("disabled", true).val("");
        }
    });
    $("#chkSucursales").prop("checked", true);
    $("#chkSucursales").prop("disabled", true);
    $("#chkSucursales").trigger('change');
    $("#listaSucursales").prop("disabled", false);

    $(document).on("change", "#listaSucursales", ControlalistaSucursalesSelected);
});

function InicializarBusqueda() {
    var sucDesc = $("#listaSucursales").find("option:selected").text();
    var sucId = $("#listaSucursales").find("option:selected").val();
    var diaId = $("#listaDias").find("option:selected").val();
    var data = { admDesc: sucDesc, admId: sucId, nroProceso: diaId };
    AbrirWaiting("Cargando datos de cierres...");
    PostGenHtml(data, cargarDatosDeCierresUrl, function (html) {
        $("#divDetalle").html(html);
        $("#divFiltro").collapse("hide");
        $("#divDetalle").collapse("show");
        InicializaEventosGrillaVtasPVCtlCierres();
        CerrarWaiting();
    });
}

function validarCamposSeleccionados() {
    let sucSeleccionada = $("#listaSucursales").val();
    let diaSeleccionado = $("#listaDias").val();
    if (sucSeleccionada == null || sucSeleccionada == undefined || sucSeleccionada == "")
        return false;
    if (diaSeleccionado == null || diaSeleccionado == undefined || diaSeleccionado == "")
        return false;
    return true;
}

function ControlalistaSucursalesSelected() {
    var item = $("#listaSucursales").val();
    var data = { suc_id: item };
    AbrirWaiting("Cargando datos de días...");
    PostGenHtml(data, obtenerDiasPorSucursalUrl, function (html) {
        CerrarWaiting();
        $("#divListaDias").html(html);
        $("#divDetalle").empty();
        setTimeout(function () {
            $("#chkDias").prop("disabled", false);
            $("#chkDias").trigger('change');
            $("#chkDias").prop("checked", true);
            $("#listaDias").prop("disabled", false);
            $("#listaDias").trigger('focus');
        }, 0);
    });
}

function SeleccionarPrimeraFilaCierres() {
    const $filas = $("#tbVtasPVCtlCierres tbody tr").not(".fila-vacia");

    if ($filas.length === 0) return;

    const $primera = $filas.first();

    // Guardar referencia
    fila_cierre_seleccionada_actual = $primera;

    // Marcar visualmente
    $("#tbVtasPVCtlCierres tbody tr").removeClass("selected-row");
    $primera.addClass("selected-row");

    // Ejecutar la lógica normal de selección
    ProcesarSeleccionFilaCierres($primera);
}


function ProcesarSeleccionFilaCierres($fila) {

    // Quitar selección previa
    $("#tbVtasPVCtlCierres tbody tr").removeClass("selected-row");

    // Marcar fila seleccionada
    $fila.addClass("selected-row");

    // Guardar referencia
    fila_cierre_seleccionada_actual = $fila;

    // Guardar valores seleccionados
    caja_nro_proceso_selected = $fila.data("caja-nro-proceso");
    caja_nro_cierre_selected = $fila.data("caja-nro-cierre");
    caja_id_selected = $fila.data("caja-id");
    cierre_pendientes_bool = $fila.data("pendientes-bool");

    // Habilitar / deshabilitar botón
    const habilitar = (cierre_pendientes_bool === true || cierre_pendientes_bool === "true" || cierre_pendientes_bool === "True");
    $("#btnConfirmacionContable").prop("disabled", !habilitar);

    // Cargar grilla de rendiciones
    if (caja_nro_proceso_selected) {
        CargarGrillaVtasPVCtlRend();
    }
}


function InicializaEventosGrillaVtasPVCtlCierres() {

    $(document).off("click", "#tbVtasPVCtlCierres tbody tr");
    $(document).on("click", "#tbVtasPVCtlCierres tbody tr", function (e) {

        if ($(e.target).is("button, a, .btn, i")) return;

        const $nuevaFila = $(this);

        // Si ya había una fila seleccionada y se intenta cambiar
        if (fila_cierre_seleccionada_actual && fila_cierre_seleccionada_actual[0] !== $nuevaFila[0]) {

            if (existe_edicion === true) {
                AbrirMensaje("ATENCIÓN", "Tiene cambios sin guardar en la grilla de rendiciones. Si cambia de cierre perderá los cambios realizados. ¿Desea continuar?", function (e) {
                    $("#msjModal").modal("hide");
                    switch (e) {
                        case "SI":
                            existe_edicion = false; // Se descartan cambios
                            ProcesarSeleccionFilaCierres($nuevaFila);
                            break;
                        case "NO":
                            break;
                        default: //NO
                            break;
                    }
                    return true;

                }, true, ["Aceptar", "Cancelar"], "question!", null);

                return; // Detener el click original
            }
        }

        // Si no hay edición pendiente o es la misma fila → continuar normalmente
        ProcesarSeleccionFilaCierres($nuevaFila);
    });

    $("#btnConfirmacionContable").prop("disabled", true);
    $("#btnConfirmarArqueo").prop("disabled", true);
    $("#btnAnularArqueo").prop("disabled", true);
    $("#btnAgregarArqueo").prop("disabled", true);
    $("#btnGuardarValores").prop("disabled", true);

    // 🔥 Seleccionar automáticamente la primera fila válida
    SeleccionarPrimeraFilaCierres();
}

function CargarGrillaVtasPVCtlRend() {
    if (!validarCierreSeleccionado()) {
        AbrirMensaje("ATENCIÓN", "Debe seleccionar un Cierre.", function () {
            $("#msjModal").modal("hide");
            return;
        }, false, ["Aceptar"], "error!", null);
    }
    else {
        var data = { nro_proceso: caja_nro_proceso_selected, nro_cierre: caja_nro_cierre_selected };
        AbrirWaiting("Cargando datos de rendición de Cierre seleccionado...");
        PostGenHtml(data, obtenerRendDeCierreSeleccionadoUrl, function (html) {
            CerrarWaiting();
            $("#divVtasPVCtlRend").html(html);
            InicializaEventosGrillaVtasPVCtlRend();
            // Seleccionar automáticamente la primera fila si existe
            SeleccionarPrimeraFilaRend();
        });
    }
}

function SeleccionarPrimeraFilaRend() {

    const $filas = $("#tbVtasPVCtlRend tbody tr").not(".fila-vacia");

    if ($filas.length === 0) return;

    const $primera = $filas.first();

    // Guardar como fila seleccionada actual
    fila_seleccionada_actual = $primera;

    // Marcar visualmente
    $("#tbVtasPVCtlRend tbody tr").removeClass("selected-row");
    $primera.addClass("selected-row");

    // Ejecutar la lógica normal de selección
    ProcesarSeleccionFila($primera);
}

function InicializaEventosGrillaVtasPVCtlRend() {
    $(document).off("click", "#btnConfirmarArqueo");
    $(document).on("click", "#btnConfirmarArqueo", function (e) {
        EvaluarConfirmarCtlArqueo();
    });

    $(document).off("click", "#btnAnularArqueo");
    $(document).on("click", "#btnAnularArqueo", function (e) {
        EvaluarAnularCtlArqueo();
    });

    $(document).off("click", "#btnAgregarArqueo");
    $(document).on("click", "#btnAgregarArqueo", function (e) {
        EvaluarAgregarCtlArqueo();
    });

    $(document).off("click", "#tbVtasPVCtlRend tbody tr");
    $(document).on("click", "#tbVtasPVCtlRend tbody tr", function (e) {

        if ($(e.target).is("button, a, .btn, i")) return;

        const $nuevaFila = $(this);

        // Si ya había una fila seleccionada y se intenta cambiar
        if (fila_seleccionada_actual && fila_seleccionada_actual[0] !== $nuevaFila[0]) {

            if (existe_edicion === true) {
                AbrirMensaje("ATENCIÓN", "Tiene cambios sin guardar. Si cambia de fila perderá los cambios realizados. ¿Desea continuar?", function (e) {
                    $("#msjModal").modal("hide");
                    switch (e) {
                        case "SI":
                            existe_edicion = false; // Se descartan cambios
                            RestaurarValoresOriginalesEnPadre();
                            ProcesarSeleccionFila($nuevaFila);
                            break;
                        case "NO":
                            break;
                        default: //NO
                            break;
                    }
                    return true;

                }, true, ["Aceptar", "Cancelar"], "question!", null);

                return; // Detener el click original
            }
        }

        // Si no hay edición pendiente o es la misma fila → continuar normalmente
        ProcesarSeleccionFila($nuevaFila);
    });

    $("#btnConfirmarArqueo").prop("disabled", true);
    $("#btnAnularArqueo").prop("disabled", true);
    $("#btnAgregarArqueo").prop("disabled", true);
}

function EvaluarConfirmarCtlArqueo() {
    AbrirMensaje("ATENCIÓN", `¿Esta seguro que desea Confirmar el Arqueo?`, function (e) {
        $("#msjModal").modal("hide");
        switch (e) {
            case "SI":
                ConfirmarCtlArqueo();
                break;
            case "NO":
                break;
            default: //NO
                break;
        }
        return true;

    }, true, ["Aceptar", "Cancelar"], "question!", null);

}

function ConfirmarCtlArqueo() {
    var data = {
        caja_nro_proceso: caja_nro_proceso_selected,
        caja_nro_cierre: caja_nro_cierre_selected,
        caja_nro_rend: caja_nro_rend_selected,
        tcf_id: tcf_id_selected
    };
    AbrirWaiting("Confirmando arqueo...");
    PostGen(data, confirmarCtlArqueoUrl, function (obj) {
        CerrarWaiting();
        if (obj.error === true || obj.warn === true) {
            AbrirMensaje("ATENCIÓN", obj.msg, function () {
                $("#msjModal").modal("hide");
                return true;
            }, false, ["Aceptar"], "error!", null);
        }
        else {
            AbrirMensaje("ATENCIÓN", "Se ha confirmado el Arqueo de forma exitosa.", function () {
                $("#msjModal").modal("hide");
                CargarGrillaVtasPVCtlRend();
                return true;
            }, false, ["Aceptar"], "succ!", null);
        }
	});
}

function EvaluarAnularCtlArqueo() {
    AbrirMensaje("ATENCIÓN", `¿Esta seguro que desea Anular el Arqueo?`, function (e) {
        $("#msjModal").modal("hide");
        switch (e) {
            case "SI": 
                AnularCtlArqueo();
                break;
            case "NO":
                break;
            default: //NO
                break;
        }
        return true;
    }, true, ["Aceptar", "Cancelar"], "question!", null);

}

function AnularCtlArqueo() {
    var data = {
        caja_nro_proceso: caja_nro_proceso_selected,
        caja_nro_cierre: caja_nro_cierre_selected,
        caja_nro_rend: caja_nro_rend_selected,
        tcf_id: tcf_id_selected
    };
    AbrirWaiting("Anulando arqueo...");
    PostGen(data, anularCtlArqueoUrl, function (obj) {
        CerrarWaiting();
        if (obj.error === true || obj.warn === true) {
            AbrirMensaje("ATENCIÓN", obj.msg, function () {
                $("#msjModal").modal("hide");
                return true;
            }, false, ["Aceptar"], "error!", null);
        }
        else {
            AbrirMensaje("ATENCIÓN", "Se ha anulado el Arqueo de forma exitosa.", function () {
                $("#msjModal").modal("hide");
                CargarGrillaVtasPVCtlRend();
                return true;
            }, false, ["Aceptar"], "succ!", null);
        }
    });
}

function EvaluarAgregarCtlArqueo() {
    AbrirWaiting();
    var datos = {};
    PostGenHtml(datos, abrirModalAgregarMedioDePagoUrl, function (obj) {
        $("#divMedioDePagoAgregar").html(obj);
        const $modal = $("#modalMedioDePagoAgregar");

        $modal.modal({
            backdrop: 'static',
        });

        $modal.modal('show');

        // Cuando el modal termina de mostrarse
        $(document).on("shown.bs.modal", "#modalImportarArchivo", function () {
        });

        $(document).on("change", "#listaMedioDePago", function () {
        });

        $(document).off("click", "#btnAceptarAgregarTipoMedioDePago");
        $(document).on("click", "#btnAceptarAgregarTipoMedioDePago", function (e) {
            EvaluarAgregarTipoMedioDePago();
        });

        CerrarWaiting();
        return true
    });
}

function EvaluarAgregarTipoMedioDePago() {
    var tipoSelected = $("#listaMedioDePago").val();
    if (tipoSelected == null || tipoSelected == undefined || tipoSelected == "") {
        AbrirMensaje("ATENCIÓN", "Debe seleccionar un Tipo de Medio de Pago válido.", function () {
            $("#msjModal").modal("hide");
            return true;
        }, false, ["Aceptar"], "error!", null);
    }
    else {
        AgregarTipoMedioDePago(tipoSelected);
    }
}

function AgregarTipoMedioDePago(tipoSelected) {
    AbrirWaiting("Agregando medio de pago...");
    var data = {
        caja_nro_proceso: caja_nro_proceso_selected,
        caja_nro_cierre: caja_nro_cierre_selected,
        caja_nro_rend: caja_nro_rend_selected,
        tcf_id: tipoSelected
    };
    PostGen(data, agregarMedioDePagoUrl, function (obj) {
        CerrarWaiting();
        if (obj.error || obj.warn) {
            AbrirMensaje("ATENCIÓN", obj.msg, function () {
                $("#msjModal").modal("hide");
            }, false, ["Aceptar"], "error!", null);
        } else {
            // 🔥 Cerrar modal solo si todo salió bien
            $("#modalMedioDePagoAgregar").modal("hide");
            CargarGrillaVtasPVCtlRend();
        }
    });
}

function ProcesarSeleccionFila($fila) {

    // Quitar selección previa
    $("#tbVtasPVCtlRend tbody tr").removeClass("selected-row");

    // Marcar fila seleccionada
    $fila.addClass("selected-row");

    // Guardar referencia a la fila actual
    fila_seleccionada_actual = $fila;

    // 🔥 Guardar valores originales ANTES de recalcular nada
    const $tds = $fila.find("td");

    $fila.data("orig-rendido", $tds.eq(2).text());
    $fila.data("orig-arqueo", $tds.eq(3).text());
    $fila.data("orig-diferencia", $tds.eq(4).text());

    // Guardar valores seleccionados
    caja_nro_rend_selected = $fila.data("caja-nro-rend");
    tcf_id_selected = $fila.data("tcf-id");
    rend_pendiente_selected = String($fila.data("rend-pendiente")).toLowerCase() === "true";

    // Habilitar / deshabilitar botones
    const habilitar = rend_pendiente_selected === true;
    $("#btnConfirmarArqueo").prop("disabled", !habilitar);
    $("#btnAnularArqueo").prop("disabled", !habilitar);
    $("#btnAgregarArqueo").prop("disabled", !habilitar);

    // Cargar detalle
    if (caja_nro_rend_selected) {
        CargarGrillaVtasPVCtlRendDetalle();
    }
}

function RestaurarValoresOriginalesEnPadre() {
    if (!fila_seleccionada_actual) return;

    const $fila = fila_seleccionada_actual;
    const $tds = $fila.find("td");

    const origRendido = $fila.data("orig-rendido");
    const origArqueo = $fila.data("orig-arqueo");
    const origDiferencia = $fila.data("orig-diferencia");

    if (origRendido !== undefined) $tds.eq(2).text(origRendido);
    if (origArqueo !== undefined) $tds.eq(3).text(origArqueo);
    if (origDiferencia !== undefined) $tds.eq(4).text(origDiferencia);
}


function CargarGrillaVtasPVCtlRendDetalle() {
    if (!validarRendSeleccionado()) {
        AbrirMensaje("ATENCIÓN", "Debe seleccionar un Medio de Pago.", function () {
            $("#msjModal").modal("hide");
            return;
        }, false, ["Aceptar"], "error!", null);
    }
    else {
        var data = {
            nro_proceso: caja_nro_proceso_selected,
            nro_cierre: caja_nro_cierre_selected,
            caja_nro_rend: caja_nro_rend_selected,
            tcf_id: tcf_id_selected,
            pendiente: rend_pendiente_selected
        };
        AbrirWaiting("Cargando datos de detalle de rendición de Cierre seleccionado...");
        PostGenHtml(data, obtenerDetalleDeRendDeCierreSeleccionadoUrl, function (html) {
            CerrarWaiting();
            $("#divVtasPVCtlRendDetalle").html(html);
            InicializaEventosGrillaVtasPVCtlRendDetalle();
        });
    }
}

function InicializaEventosGrillaVtasPVCtlRendDetalle() {
    if (rend_pendiente_selected === true) {
        $("#btnAgregarValor").prop("disabled", false);
    } else {
        $("#btnAgregarValor").prop("disabled", true);
    }
    $("#btnGuardarValores").prop("disabled", true);
    // Aplicar máscara a todos los inputs de importe
    getMaskForMoneyType(".input-importe");
    // Evitar eventos duplicados
    $(document).off("click", ".btnEditarValor");
    $(document).off("click", "#btnAgregarValor");
    $(document).off("click", "#btnGuardarValores");

    $(document).on("click", "#btnGuardarValores", function (e) {
        GuardarCtlDetalle();
    });

    $(document).on("click", "#btnAgregarValor", function (e) {
        CargaCtlNuevoItemDetalle();
    });

    // Delegación de eventos
    $(document).on("click", ".btnEditarValor", function (e) {
        e.stopPropagation(); // evita seleccionar la fila

        const $btn = $(this);

        const ins_id = $btn.data("ins-id");
        const tcf_id = $btn.data("tcf-id");
        const ins_detalle = $btn.data("ins-detalle");

        // Lógica de edición
        AbrirModalEditarValor(ins_id, tcf_id, ins_detalle);
    });

    // Evitar duplicados
    $(document).off("blur", ".input-importe");
    $(document).off("keydown", ".input-importe");

    // Guardar al salir
    $(document).on("blur", ".input-importe", function () {
        const $input = $(this);
        const valorOriginal = Number($input.data("original") ?? 0);
        const nuevoValor = Number($input.inputmask("unmaskedvalue") || 0);

        // Si no cambió, no hacemos nada
        if (valorOriginal === nuevoValor) {
            return;
        }

        GuardarImporteEditado($(this));
    });

    // Seleccionar texto al recibir foco (solo si viene del mouse)
    $(document).off("focus", ".input-importe");
    $(document).on("focus", ".input-importe", function (e) {
        const $input = $(this);

        // Si el foco viene por teclado (Enter, Tab, Arrow), NO seleccionar
        if ($input.data("keyboard-nav")) {
            $input.data("keyboard-nav", false); // limpiar flag
            return;
        }

        // Si viene del mouse → seleccionar todo
        setTimeout(() => {
            $input.select();
        }, 10);
    });

    // Enter / Escape
    $(document).on("keydown", ".input-importe", function (e) {

        const $input = $(this);
        const $inputs = $("#tbVtasPVCtlRendDetalle .input-importe");
        const index = $inputs.index(this);

        const keysNext = ["Enter", "Tab", "ArrowDown"];
        const keysPrev = ["ArrowUp"];

        // ESC → cancelar edición
        if (e.key === "Escape") {
            e.preventDefault();
            CancelarEdicion($input);
            return;
        }

        // ENTER / TAB / FLECHA ABAJO → guardar + mover
        if (keysNext.includes(e.key)) {
            e.preventDefault();

            // Guardar antes de moverse
            console.log(".input-importe -> keydown -> keysNext", $input);
            GuardarImporteEditado($input);

            const nextIndex = (index + 1) % $inputs.length; // wrap-around
            const $next = $inputs.eq(nextIndex);

            $next.focus().select();
            return;
        }

        // FLECHA ARRIBA → guardar + mover hacia arriba
        if (keysPrev.includes(e.key)) {
            e.preventDefault();

            console.log(".input-importe -> keydown -> keysPrev", $input);
            GuardarImporteEditado($input);

            const prevIndex = (index - 1 + $inputs.length) % $inputs.length; // wrap-around
            const $prev = $inputs.eq(prevIndex);

            $prev.focus().select();
            return;
        }

        // Para cualquier otra tecla → no hacemos nada especial
    });

    $(document).off("click", "#tbVtasPVCtlRendDetalle tbody tr");
    $(document).on("click", "#tbVtasPVCtlRendDetalle tbody tr", function (e) {

        if ($(e.target).is("button, a, .btn, i")) return;

        const $nuevaFila = $(this);

        ProcesarSeleccionFilaRendDetalle($nuevaFila);
    });
}

function GuardarCtlDetalle() {
    var caja_nro_proceso = caja_nro_proceso_selected;
    var caja_nro_cierre= caja_nro_cierre_selected;
    var caja_nro_rend = caja_nro_rend_selected; 
    var tcf_id = tcf_id_selected;
    var data = { caja_nro_proceso, caja_nro_cierre, caja_nro_rend, tcf_id }
    AbrirWaiting("Guardando datos de Detalle de Arqueo...")
    PostGen(data, guardarCtlDetalleUrl, function (obj) {
        CerrarWaiting();
        if (obj.error === true || obj.warn === true) {
            AbrirMensaje("ATENCIÓN", obj.msg, function () {
                $("#msjModal").modal("hide");
                existe_edicion = false;
                CargarGrillaVtasPVCtlRend();
                CargarGrillaVtasPVCtlRendDetalle();
                return true;
            }, false, ["Aceptar"], "error!", null);
        }
        else {
            AbrirMensaje("ATENCIÓN", "Se han actualizado los datos del Detalle de Arqueo de forma exitosa.", function () {
                $("#msjModal").modal("hide");
                CargarGrillaVtasPVCtlRendDetalle();
                return true;

            }, false, ["Aceptar"], "succ!", null);
        }
    });
}

function CargaCtlNuevoItemDetalle() {
    var data = {
        caja_nro_proceso: caja_nro_proceso_selected,
        caja_nro_cierre: caja_nro_cierre_selected,
        caja_nro_rend: caja_nro_rend_selected,
    };
    AbrirWaiting("Agregando nuevo registro...");
    PostGen(data, cargaCtlNuevoItemDetalleUrl, function (obj) {
        CerrarWaiting();
        if (obj.error === true || obj.warn === true) {
            AbrirMensaje("ATENCIÓN", obj.msg, function () {
                $("#msjModal").modal("hide");
                return true;
            }, false, ["Aceptar"], "error!", null);
        }
        else {
            //Actualizamos la tabla de detale de rend.
            CargarGrillaVtasPVCtlRendDetalle();
        }
    });
}

function ProcesarSeleccionFilaRendDetalle($fila) {
    // Quitar selección previa
    $("#tbVtasPVCtlRendDetalle tbody tr").removeClass("selected-row");

    // Marcar fila seleccionada
    $fila.addClass("selected-row");
}

function RecalcularDiferenciaEnFila($input) {

    const $td = $input.closest("td");
    const $tr = $td.closest("tr");

    // Obtener importe OK (editado)
    const importeOk = parseFloat($input.inputmask("unmaskedvalue")) || 0;

    // Obtener importe Arqueo desde la celda correspondiente
    const textoArqueo = $tr.find("td").eq(2).text().trim(); // columna Arqueo
    const importeArqueo = parseFloat(textoArqueo.replace(/\./g, "").replace(",", ".")) || 0;

    // Calcular diferencia
    const dif = importeOk - importeArqueo;

    // Formatear diferencia
    const difFormateado = dif.toLocaleString("es-AR", {
        minimumFractionDigits: 2,
        maximumFractionDigits: 2
    });

    // Actualizar celda Dif (columna 3)
    $tr.find("td").eq(3).text(difFormateado);
}

function GuardarImporteEditado($input) {
    // Si ya estamos guardando, NO volver a entrar
    if (guardando_importe) {
        return;
    }
    guardando_importe = true;

    const $td = $input.closest("td");
    const valorOriginal = $input.data("original");
    const nuevoValor = $input.inputmask("unmaskedvalue");

    if (nuevoValor === "" || isNaN(nuevoValor)) {
        CancelarEdicion($input);
        return;
    }

    AbrirWaiting("Guardando importe...");
    var data = { ins_id: $td.data("ins-id"), importe: nuevoValor };
    PostGen(data, actualizarImporteEnItemDeDetalleDeArqueoUrl, function (obj) {
        CerrarWaiting();
        if (obj.error === true) {
            AbrirMensaje("ATENCIÓN", obj.msg, function () {
                $("#msjModal").modal("hide");
                CancelarEdicion($input);
                return true;
            }, false, ["Aceptar"], "error!", null);
        }
        else {
            // Recalcular diferencia en la fila
            RecalcularDiferenciaEnFila($input);
            ActualizarTotalesEnPadre();
            existe_edicion = true;
            $("#btnGuardarValores").prop("disabled", !existe_edicion);

            // 🔥 IMPORTANTE: actualizar el valor original
            $input.data("original", nuevoValor);
            guardando_importe = false;
        }
    });
}


function ActualizarTotalesEnPadre() {
    const $filas = $("#tbVtasPVCtlRendDetalle tbody tr").not(".fila-vacia");

    let totalRendido = 0;
    let totalArqueo = 0;
    let totalDiferencia = 0;

    $filas.each(function () {
        const $tr = $(this);

        // --- RENDIDO (columna 1) ---
        let $tdRendido = $tr.find("td").eq(1);
        let rendido;

        if ($tdRendido.find("input").length) {
            // Si tiene input → tomar valor del inputmask
            rendido = Number($tdRendido.find("input").inputmask("unmaskedvalue") || 0);
        } else {
            // Si no tiene input → tomar texto
            rendido = Number(
                $tdRendido.text().replace(/\./g, "").replace(",", ".") || 0
            );
        }

        // --- ARQUEO (columna 2) ---
        let arqueo = Number(
            $tr.find("td").eq(2).text().replace(/\./g, "").replace(",", ".") || 0
        );

        // --- DIFERENCIA (columna 3) ---
        let diferencia = Number(
            $tr.find("td").eq(3).text().replace(/\./g, "").replace(",", ".") || 0
        );

        totalRendido += rendido;
        totalArqueo += arqueo;
        totalDiferencia += diferencia;
    });

    // Actualizar la fila seleccionada en la tabla padre
    if (fila_seleccionada_actual) {
        const $tds = fila_seleccionada_actual.find("td");

        $tds.eq(2).text(FormatearPrecio(totalRendido));
        $tds.eq(3).text(FormatearPrecio(totalArqueo));
        $tds.eq(4).text(FormatearPrecio(totalDiferencia));
    }
}

function FormatearPrecio(valor) {
    return Number(valor).toLocaleString("en-US", {
        minimumFractionDigits: 2,
        maximumFractionDigits: 2
    });
}

function CancelarEdicion($input) {
    const original = $input.data("original");
    $input.val(original);
}

function AbrirModalEditarValor(ins_id, tcf_id, ins_detalle) {
}

function validarRendSeleccionado() {
    if (caja_nro_rend_selected == null || caja_nro_rend_selected == undefined || caja_nro_rend_selected == "")
        return false;
    if (tcf_id_selected == null || tcf_id_selected == undefined || tcf_id_selected == "")
        return false;
    return true;
}

function validarCierreSeleccionado() {
    if (caja_nro_proceso_selected == null || caja_nro_proceso_selected == undefined || caja_nro_proceso_selected == "")
        return false;
    if (caja_nro_cierre_selected == null || caja_nro_cierre_selected == undefined || caja_nro_cierre_selected == "")
        return false;
    return true;
}

function getMaskForMoneyType(selector) {
    $(selector).inputmask({
        alias: 'numeric',
        groupSeparator: '.',
        radixPoint: ',',
        digits: 2,
        digitsOptional: false,
        allowMinus: false,
        prefix: '',
        suffix: '',
        rightAlign: true,
        unmaskAsNumber: true
    });
}