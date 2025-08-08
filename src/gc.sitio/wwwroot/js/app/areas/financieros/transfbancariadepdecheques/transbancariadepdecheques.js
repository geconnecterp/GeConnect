$(function () {
    if ($("#listaIntervalo")) {
        $(document).on("change", "#listaIntervalo", ControlalistaIntervaloSelected);
    }
    $(document).on("click", "#btnAgregarValorOrigen", btnAgregarValorOrigenValidar);
    $(document).on("click", "#btnAgregarValorDestino", btnAgregarValorDestinoValidar);

    $("#UpdateValores").on("change", function () {
        if ($(this).val() == 'true') {
            //Aca llamar a algun metodo para actualizar las grillas
            //CargarValoresDesdeObligYCredSeleccionados();
            //$("#CuentaObs").prop("disabled", false);
        }
    });

    InicializarCampos();
});

const TypeIntervalo = {
    24: '1',
    48: '2',
    72: '3',
    Otros: '4'
}

function InicializarCampos() {
    const fechaInput = document.getElementById("fechaAcreditacion");
    if (fechaInput) {
        fechaInput.addEventListener("change", function () {
            const [year, month, day] = this.value.split("-");
            const fechaSeleccionada = new Date(year, month - 1, day); // Evita el desfase
            const dia = fechaSeleccionada.getDay(); // 0 = domingo, 6 = sábado
            var now = moment().format('yyyy-MM-DD');

            if (dia === 0 || dia === 6) {
                //alert("No se pueden seleccionar fines de semana.");
                ControlaMensajeWarning("No se pueden seleccionar fines de semana.");
                $("#fechaAcreditacion").val(now);
            }
        });
    }
}

function ControlalistaIntervaloSelected(e) {
    var fecha = $("#fechaAcreditacion").val();
    const [year, month, day] = fecha.split("-");
    const fechaSeleccionada = new Date(year, month - 1, day); // Evita el desfase
    if (e.currentTarget.value != TypeIntervalo.Otros){
        var nuevaFecha = sumarDiasHabiles(fechaSeleccionada, e.currentTarget.value);
        $("#fechaAcreditacion").val(nuevaFecha);
    }
    else {
    }
}

function sumarDiasHabiles(fechaInicial, diasHabiles) {
    const resultado = new Date(fechaInicial);
    let diasSumados = 0;

    while (diasSumados < diasHabiles) {
        resultado.setDate(resultado.getDate() + 1);
        const diaSemana = resultado.getDay(); // 0 = domingo, 6 = sábado

        if (diaSemana !== 0 && diaSemana !== 6) {
            diasSumados++;
        }
    }

    return formatearFecha(resultado);
}

function formatearFecha(fecha) {
    const año = fecha.getFullYear();
    const mes = String(fecha.getMonth() + 1).padStart(2, '0'); // Meses van de 0 a 11
    const dia = String(fecha.getDate()).padStart(2, '0');

    return `${año}-${mes}-${dia}`;
}



function onChangeAcreditacion() {

}

//Abro modal de seleccion de valores
function btnAgregarValorOrigenValidar() {
    var app = $("#parametro_valores_origen").val();
    var importe = 0
    var valor_a_nombre_de = "";
    var valores = [];
    var data = { app, importe, valor_a_nombre_de, valores };
    invocarModalDeSeleccionDeValores(data);
}

function btnAgregarValorDestinoValidar() {
    var app = $("#parametro_valores_destino").val();
    var importe = 0
    var valor_a_nombre_de = "";
    var valores = [];
    var data = { app, importe, valor_a_nombre_de, valores };
    invocarModalDeSeleccionDeValores(data);
}