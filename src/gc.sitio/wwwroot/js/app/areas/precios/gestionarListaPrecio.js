var cta_id_seleccionada = "";
var lpId_Seleccionada = "";
$(function () {
    InicializaPantalla();
    InicializaEventos();

    // Seleccionar automáticamente la primera fila al iniciar
    SeleccionarPrimeraListaPrecio();
});

function InicializaPantalla() {
    $("#btnAbmElimi").hide();
    $("#btnAbmNuevo").hide();
    $("#btnFiltro").hide();
    $("#btnDetalle").hide();
}

function InicializaEventos() {
    $("#btnAbmCancelar").prop("disabled", true);
    $(document).off("click", "#btnAbmAceptar");
    $(document).on("click", "#btnAbmAceptar", ControlaBtnAbmAceptarClick);
    $(document).off("click", "#btnAbmModif");
    $(document).on("click", "#btnAbmModif", ControlaBtnAbmModifClick);
    $(document).off("click", "#btnAbmCancelar");
    $(document).on("click", "#btnAbmCancelar", ControlaBtnAbmCancelarClick);
    // Captura selección de fila y envía lp_id al backend
    $(document).off("click", "#tbGridListaPrecios tbody tr");
    $(document).on("click", "#tbGridListaPrecios tbody tr", function () {

        // Remover selección previa
        $("#tbGridListaPrecios tbody tr").removeClass("selected-row");

        // Marcar la fila actual
        $(this).addClass("selected-row");

        // Obtener el lp_id desde el atributo data
        let lpId = $(this).data("lp-id");
        let lpMgnPrincipal = $(this).data("lp-mgn-principal");

        if (!lpId) {
            lpId_Seleccionada = "";
            return;
        }
        else {
            lpId_Seleccionada = lpId;
        }

		// Enviar al backend para obtener los datos de la lista de precios seleccionada
        AbrirWaiting("Cargando información...");
        PostGenHtml({ lp_id: lpId }, cargarDatosDeListaDePrecioURL, function (obj) {
            $("#divDatosLP").html(obj);
            CerrarWaiting();
            setTimeout(() => {
                CargarInputMask();
            }, 200);
            return true
        });

        // Si es lista Asociada cargo la lista Rub/Cta de margenes
        if (lpMgnPrincipal && lpMgnPrincipal === "S") {
            $("#divRubrosProv").show();
            $("#divDatosRubrosProv").show();

            PostGenHtml({ lp_id: lpId }, cargarDatosDeListaDePrecioRubCtaURL, function (obj) {
                $("#divRubrosProv").html(obj);
                //deshabilitarYBlanquearActivables();
            });

            PostGenHtml({}, cargarDatosDeSeccionRubCtaURL, function (obj) {
                $("#divDatosRubrosProv").html(obj);

                const $mgn = $("#divDatosRubrosProv #Mgn");
                if ($mgn.length) {
                    CargarInputMaskDos();
                }

                cargarEventosSeccionDatosRubCta();
                return true;
            });
        }
        else {
            // Ocultar y limpiar la segunda columna
            $("#divRubrosProv").hide().empty();
            $("#divDatosRubrosProv").hide().empty();
        }

        $("#btnAbmModif").prop("disabled", false);
    });

    $(document).on("change", "#chkPorSectores", function () {

        if ($(this).is(":checked")) {
            // Mostrar Sector
            $("#contenedorSector").show();
            // Ocultar Rubros
            $("#contenedorRubros").hide();

            // Limpiar Rubros
            $("#listaRubros").val("");
        } else {
            // Mostrar Rubros
            $("#contenedorRubros").show();
            // Ocultar Sector
            $("#contenedorSector").hide();

            // Limpiar Sector
            $("#listaSectores").val("");
        }
    });
}

function ControlaBtnAbmAceptarClick() {
    AbrirMensaje("ATENCIÓN", `Se pueden generar modificaciones masivas de precios, en carga temporal ¿Esta seguro de continuar?`, function (e) {
        $("#msjModal").modal("hide");
        switch (e) {
            case "SI":
                handlerBtnAbmAceptarClick();
                break;
            case "NO":
                break;
            default: //NO
                break;
        }
        return true;

    }, true, ["Aceptar", "Cancelar"], "question!", null);
}

function handlerBtnAbmAceptarClick() {
    AbrirWaiting("Registrando modificaciones en Lista de Precios...");
    let lpId = lpId_Seleccionada;
    let abm = 'M';
    let lpMgnPrincipal = $("#tbGridListaPrecios tr[data-lp-id='" + lpId + "']").data("lp-mgn-principal");

    if (lpMgnPrincipal == 'S') {
        let lpMargen = 0;
        let lpMgnPrincipalPorc = $("#lp_margen").inputmask('unmaskedvalue');
    }
    else {
        let lpMargen = $("#lp_margen").inputmask('unmaskedvalue');
        let lpMgnPrincipalPorc = 0;
    }
    let lpPrevisionTot = $("#lp_prevision_tot").inputmask('unmaskedvalue');
    let lpPrevisionPin = $("#lp_prevision_pin").inputmask('unmaskedvalue');
    var data = {
        abm,
        lpId,
        lpMargen,
        lpMgnPrincipal,
        lpMgnPrincipalPorc,
        lpPrevisionTot,
        lpPrevisionPin,
    };
    PostGen(data, registrarModificacionesEnListaDePreciosURL, function (obj) {
        CerrarWaiting();
        //Deberiamos actualizar la vista
        InicializaEventos();

        // Seleccionar automáticamente la primera fila al iniciar
        SeleccionarPrimeraListaPrecio();
    });
}

function habilitarActivables() {
    $(".activable").each(function () {
        const $el = $(this);

        // Habilitar edición
        $el.prop("disabled", false);
        $el.prop("readonly", false);

        // Si es un input con máscara, reactivar la máscara
        // if ($el.hasClass("lp-input")) {
        //     getMaskForMoneyType($el);
        // }
    });
    //CargarInputMask();
}

function deshabilitarYBlanquearActivables() {

    $(".activable").each(function () {
        const $el = $(this);

        // Siempre deshabilitar
        //$el.prop("disabled", true);
        $el.prop("readonly", true);

        // INPUT TEXT
        if ($el.is("input[type='text']")) {

            // Inputs con máscara (lp-input y lp-input-dos)
            if ($el.hasClass("lp-input") || $el.hasClass("lp-input-dos")) {
                $el.val("0.00");
            } else {
                $el.val("");
            }

            // Si es el autocomplete Rel01 → también blanquear hidden
            if ($el.attr("id") === "Rel01") {
                $("#Rel01Item").val("");
            }
        }

        // SELECT
        else if ($el.is("select")) {
            // Volver a "Seleccionar"
            $el.val("");
        }

        // CHECKBOX
        else if ($el.is("input[type='checkbox']")) {
            // NO cambiar estado, solo deshabilitar
        }

        // BOTONES
        else if ($el.is("button")) {
            // Deshabilitar botón
            $el.prop("disabled", true);
        }
    });
}



function ControlaBtnAbmModifClick() {
    habilitarActivables();
    $("#btnAbmAceptar").prop("disabled", false);
    $("#btnAbmCancelar").prop("disabled", false);
    $("#btnAbmModif").prop("disabled", true);

    $("#tbGridListaPrecios").addClass("tabla-bloqueada");
}

function ControlaBtnAbmCancelarClick() {
    deshabilitarYBlanquearActivables();
    $("#btnAbmAceptar").prop("disabled", true);
    $("#btnAbmCancelar").prop("disabled", true);
    $("#btnAbmModif").prop("disabled", false);

    $("#tbGridListaPrecios").removeClass("tabla-bloqueada");
}
function agregarItemRubroCta() {
    AbrirWaiting("Agregando registros...");
    var valorSeleccionado = "";
    var porSectores = $("#chkPorSectores").is(":checked");
    var ctaId = cta_id_seleccionada ? cta_id_seleccionada : "%";
    var valorSeleccionado = $("#contenedorSector:visible #listaSectores").val()
        || $("#contenedorRubros:visible #listaRubros").val();
    var mgn = $("#Mgn").inputmask('unmaskedvalue');
    var lpId = lpId_Seleccionada;
    var data = { lpId, valorSeleccionado, porSectores, ctaId, mgn };
    PostGen(data, agregarRegistrosUrl, function (obj) {
        CerrarWaiting();
        if (obj.error === true) {
            AbrirMensaje("ATENCIÓN", obj.mensaje, function () {
                $("#msjModal").modal("hide");
                return true;
            }, false, ["Aceptar"], "error!", null);
        }
        else {
            actualizarListaRubroCta(lpId_Seleccionada);
            setTimeout(() => {
                $("#listaSectores").val("");
                $("#listaRubros").val("");
                cta_id_seleccionada = "";
                $("#Rel01").val("");
                $("#Mgn").val("0.00");
            }, 100);
        }
    });
}

function actualizarListaRubroCta(lpId) {
    PostGenHtml({ lp_id: lpId }, cargarDatosDeListaDePrecioRubCtaURL, function (obj) {
        $("#divRubrosProv").html(obj);
    });
}

function actualizarDatosComplementariosRubrosCta() {
    PostGenHtml({}, cargarDatosDeSeccionRubCtaURL, function (obj) {
        $("#divDatosRubrosProv").html(obj);

        const $mgn = $("#divDatosRubrosProv #Mgn");
        if ($mgn.length) {
            CargarInputMaskDos();
        }

        cargarEventosSeccionDatosRubCta();
        return true;
    });
}

function eliminarItemRubroCta(rubId, ctaId) {
    // Implement the logic to eliminate the item
}

function SeleccionarPrimeraListaPrecio() {

    // Obtener la primera fila real (que tenga data-lp-id)
    let $primeraFila = $("#tbGridListaPrecios tbody tr[data-lp-id]").first();

    if ($primeraFila.length === 0) return;

// Simular el click real
    $primeraFila.trigger("click");
}

function CargarInputMask() {
    // Aplica la máscara a todos los inputs numéricos del partial
    console.log("CargarInputMask");
    getMaskForMoneyType("#divDatosLP .lp-input");
}

function CargarInputMaskDos() {
    // Aplica la máscara a todos los inputs numéricos del partial
    console.log("CargarInputMaskDos");
    getMaskForMoneyType("#divDatosRubrosProv .lp-input-dos");
}

function getMaskForMoneyType(selector) {
    $(selector).inputmask({
        alias: 'numeric',
        groupSeparator: '',       // sin separador de miles
        radixPoint: '.',          // separador decimal
        digits: 2,
        digitsOptional: true,
        allowMinus: false,
        min: 0,
        max: 100,
        rightAlign: true,
        prefix: '',
        suffix: '',
        unmaskAsNumber: true
    });
}

function getMaskForMoneyType2(selector) {
    $(selector).inputmask({
        mask: "999.99",          // permite 3 enteros + 2 decimales
        placeholder: "0",
        greedy: false,
        rightAlign: true,
        radixPoint: ".",
        digits: 2,
        digitsOptional: true,
        allowMinus: false,
        min: 0,
        max: 100,
        unmaskAsNumber: true
    });
}

function cargarEventosSeccionDatosRubCta() {
    $("#Rel01").off("click");
    $("#Rel01").on("click", function () {
        $(this).val("");
        cta_id_seleccionada = "";
    });
    $("#Rel01").autocomplete({
        source: function (request, response) {

            data = { prefix: request.term }; /*Rel01*/

            $.ajax({
                url: autoComRel01Url,
                type: "POST",
                dataType: "json",
                data: data,
                success: function (obj) {
                    response($.map(obj, function (item) {
                        var texto = item.descripcion;
                        return {
                            label: texto,
                            value: item.descripcion,
                            id: item.id,
                            prov: item.provId
                        };
                    }));
                }
            })
        },
        minLength: 3,

        focus: function (event, ui) {
            // evita que el # aparezca mientras navegas con flechas
            const partes = ui.item.value.split("#");
            $("#Rel01").val(partes.join(" "));
            return false;
        },

        select: function (event, ui) {
            const partes = ui.item.value.split("#");
            const textoSinSeparador = partes.join(" ");

            // Mostrar SIN el "#"
            $("#Rel01").val(textoSinSeparador);
            cta_id_seleccionada = ui.item.id;
            var opc = "<option value=" + ui.item.id + ">" + textoSinSeparador + "</option>"

            event.preventDefault();
            return true;
        }
    }).autocomplete("instance")._renderItem = function (ul, item) {

        const partes = item.label.split("#");

        const ctaLista = partes[0];
        const tipoDesc = partes[1];

        return $("<li>")
            .append(
                `<div>
                <span style="font-weight:bold; font-size:14px;">
                    ${ctaLista}
                </span>
                <span style="font-size:13px; color:#555;">
                    ${tipoDesc}
                </span>
            </div>`
            )
            .appendTo(ul);
    };
}
