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
    $(document).on("click", "#btnAbmModif", ControlaBtnAbmModifClick);
    $(document).on("click", "#btnAbmCancelar", ControlaBtnAbmCancelarClick);
    // Captura selección de fila y envía lp_id al backend
    $(document).on("click", "#tbGridListaPrecios tbody tr", function () {

        // Remover selección previa
        $("#tbGridListaPrecios tbody tr").removeClass("selected-row");

        // Marcar la fila actual
        $(this).addClass("selected-row");

        // Obtener el lp_id desde el atributo data
        let lpId = $(this).data("lp-id");
        let lpMgnPrincipal = $(this).data("lp-mgn-principal");

        if (!lpId) return;

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
                deshabilitarYBlanquearActivables();
            });

            PostGenHtml({}, cargarDatosDeSeccionRubCtaURL, function (obj) {
                $("#divDatosRubrosProv").html(obj);

                const $mgn = $("#divDatosRubrosProv #Mgn");
                if ($mgn.length) {
                    getMaskForMoneyType($mgn);
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

function habilitarActivables() {
    $(".activable").each(function () {
        $(this).prop("disabled", false);
    });
}

function deshabilitarYBlanquearActivables() {
    $(".activable").each(function () {
        const $el = $(this);

        // Siempre deshabilitar
        $el.prop("disabled", true);

        // Blanquear según tipo (excepto checkbox)
        if ($el.is("input[type='text']")) {

            if ($el.hasClass("lp-input")) {
                $el.val("0.00");   // valor default para inputs con máscara
            } else {
                $el.val("");       // input normal
            }

        } else if ($el.is("select")) {

            $el.val("");           // volver a "Seleccionar"

        } else if ($el.is("input[type='checkbox']")) {

            // ❌ NO cambiar estado
            // $el.prop("checked", false);  <-- eliminar esta línea

            // Solo deshabilitar
            // (ya está deshabilitado arriba)
        }
    });
}


function ControlaBtnAbmModifClick() {
    habilitarActivables();
    $("#btnAbmAceptar").prop("disabled", false);
    $("#btnAbmCancelar").prop("disabled", false);
    $("#btnAbmModif").prop("disabled", true);
}

function ControlaBtnAbmCancelarClick() {
    deshabilitarYBlanquearActivables();
    $("#btnAbmAceptar").prop("disabled", true);
    $("#btnAbmCancelar").prop("disabled", true);
    $("#btnAbmModif").prop("disabled", false);
}
function agregarItemRubroCta() { }

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
    getMaskForMoneyType("#divDatosLP .lp-input");
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
    $("#Rel01").on("click", function () { $(this).val(""); });
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
                        return { label: texto, value: item.descripcion, id: item.id, prov: item.provId };
                    }));
                }
            })
        },
        minLength: 3,
        select: function (event, ui) {
            // $("#razonsocial").val(ui.item.value);
            // $("#Cuenta").val(ui.item.id)
            var opc = "<option value=" + ui.item.id + ">" + ui.item.value + "</option>"
            return true;
        }
    });
}
