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
            PostGenHtml({ lp_id: lpId }, cargarDatosDeListaDePrecioRubCtaURL, function (obj) {
                $("#divRubrosProv").html(obj);
            });
        }
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