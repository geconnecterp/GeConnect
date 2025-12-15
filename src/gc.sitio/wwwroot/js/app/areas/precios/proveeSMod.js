let _etiquetaLoading = false;

$(function () {
    inicializaVista();
    inicializaEventosModif();


});

function inicializaVista() {

    // INICIALIZAMOS PANELES
    if ($("#divDetalle").is(":visible")) {
        $("#divDetalle").collapse("hide");
    }
    $("#divFiltro").collapse("show");

    // Inicializar campos de fecha con un período de 3 meses
    // Date2 se establece con la fecha actual
    const hoy = new Date();
    const tresMesesAtras = new Date();
    tresMesesAtras.setMonth(hoy.getMonth() - 3);

    // Formatear fechas a YYYY-MM-DD para input type="date"
    const formatearFecha = (fecha) => {
        const año = fecha.getFullYear();
        const mes = String(fecha.getMonth() + 1).padStart(2, '0');
        const dia = String(fecha.getDate()).padStart(2, '0');
        return `${año}-${mes}-${dia}`;
    };

   // $("#Date2").val(formatearFecha(hoy));
    $("#Date1").val(formatearFecha(tresMesesAtras));

    $("#btnImprimir").prop("disabled", true);


    setTimeout(() => {
        $("#chkDesdeHasta").trigger("click");
    }, 200);

}
function inicializaEventosModif() {
    // Configurar el evento click para el botón Buscar/Filtrar
    $("#btnBuscar").on("click", function () {
        buscarProductosPSMP(this);
    });

    $("#btnCancel").on("click", function () {
        window.location.href = homeProvSModiUrl;
    });

    $("#chkDesdeHasta").on("click", function () {
        if ($("#chkDesdeHasta").is(":checked")) {
            $("#Date1").prop("disabled", false);
            //$("#Date2").prop("disabled", false);
        } else {
            $("#Date1").prop("disabled", true);
            //$("#Date2").prop("disabled", true);
        }
    });

    $("#btnImprimir").on("click", imprimirReportePSMP);
}

function setBtnLoading($btn, loading, originalHtml) {
    if (!$btn || !$btn.length) return;

    if (loading) {
        $btn.prop("disabled", true).html('<span class="spinner-border spinner-border-sm me-1"></span>Buscando...');
    } else {
        $btn.prop("disabled", false).html(originalHtml ?? "Buscar");
    }
}

function buscarProductosPSMP(btn) {
    const usarFechas = $("#chkDesdeHasta").is(":checked");

    if (!usarFechas) {
        AbrirMensaje("Advertencia",
            "Para poder realizar la búsqueda, inicialmente, tiene que activar la fecha del filtro",
            function () { $("#msjModal").modal("hide"); },
            false, ["Aceptar"], "warn!", null);
        return;
    }

    if (_etiquetaLoading) return;
    _etiquetaLoading = true;

    const $btn = $(btn);
    const originalHtml = $btn.html();
    setBtnLoading($btn, true);

    const date1Value = $("#Date1").val();
    //const date2Value = $("#Date2").val();

    // Validar que las fechas no estén vacías
    if (!date1Value ) {
        AbrirMensaje("Advertencia",
            "Debe seleccionar la fecha desde para realizar la búsqueda",
            function () { $("#msjModal").modal("hide"); },
            false, ["Aceptar"], "warn!", null);
        setBtnLoading($btn, false, originalHtml);
        _etiquetaLoading = false;
        return;
    }

    // Convertir la fecha al formato ISO 8601 esperado por el servidor
    const fechaDesde = new Date(date1Value + 'T00:00:00');
    const fechaISO = fechaDesde.toISOString();
    //para la impresion
    const datos = {
        desde : fechaISO
    }

    $.ajax({
        url: obtenerProveedoresSinModificacionPrUrl,
        type: "POST",
        contentType: "application/json; charset=utf-8",
        dataType: "html",
        data: JSON.stringify(fechaISO),
        success: function (response) {
            $("#divDetalle").html(response).collapse("show");
            $("#divFiltro").collapse("hide");
            $("#btnImprimir").prop("disabled", false);
            cargarReporteEnArre(indexPrint, datos, "Reporte de Proveedores sin modificaciones de Precios");
        },
        error: function (xhr, status, error) {
            console.error("Error al obtener detalle de Proveedores:", error);
            console.error("Status:", status);
            console.error("Response:", xhr.responseText);
            console.error("Status Code:", xhr.status);

            let mensajeError = 'No se pudo obtener la información de Proveedore sin Modificación de Precios. Intente nuevamente.';

            // Intentar extraer mensaje de error del servidor
            if (xhr.responseJSON?.mensaje) {
                mensajeError = xhr.responseJSON.mensaje;
            } else if (xhr.status === 400) {
                mensajeError = 'Los datos enviados no son válidos. Verifique la fecha seleccionada.';
                if (xhr.responseText) {
                    try {
                        const errorObj = JSON.parse(xhr.responseText);
                        if (errorObj.errors) {
                            const errores = Object.values(errorObj.errors).flat();
                            mensajeError += '<br><small>' + errores.join('<br>') + '</small>';
                        }
                    } catch (e) {
                        console.error("Error al parsear respuesta de error:", e);
                    }
                }
            } else if (xhr.status === 401) {
                mensajeError = 'Su sesión ha expirado. Por favor, inicie sesión nuevamente.';
                setTimeout(() => {
                    window.location.reload();
                }, 2000);
            } else if (xhr.status === 500) {
                mensajeError = 'Error interno del servidor. Contacte al administrador.';
            }

            const htmlError = `<div class="alert alert-danger py-2 mb-0">
                <i class="bx bx-error-circle me-1"></i>${mensajeError}
            </div>`;

            $("#divDetalle").html(htmlError).collapse("show");
        },
        complete: function () {
            setBtnLoading($btn, false, originalHtml);
            _etiquetaLoading = false;
        }
    });
}

function imprimirReportePSMP() {
    let data = { modulo: "", parametros: [] }
    invocacionGestorDoc(data);
}