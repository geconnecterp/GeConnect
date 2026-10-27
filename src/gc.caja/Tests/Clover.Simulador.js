// Se publica sólo desde el host aislado de Tests. No usa Checkout ni el SP de pago.
document.addEventListener('DOMContentLoaded', function () {
    const root = document.getElementById('modalDetalleClover');
    const transporte = CloverFixtures.crearTransporte(() => document.getElementById('escenarioSimulado').value);
    document.getElementById('avisoClover').textContent = 'SIMULACIÓN AISLADA. No se envía dinero, no se registra en Caja y no se usa el POS.';
    const vista = CloverPago.crearVista(root, transporte, function (resultado) {
        document.getElementById('resultadoSimulado').textContent =
            JSON.stringify({ accionPreparada: resultado.accion, saldoRestante: resultado.saldoRestante, pago: resultado.datosSP, recargo: resultado.recargo, simulacion: true }, null, 2);
    });
    document.getElementById('abrirSimulacion').addEventListener('click', function () {
        document.getElementById('resultadoSimulado').textContent = 'Sin registración de Caja.';
        vista.abrir({ instrumento: { ins_id: 'G00', ins_desc: 'Clover · prueba aislada' }, coTipo: document.getElementById('operacionSimulada').value, saldo: document.getElementById('saldoSimulado').value });
    });
    document.getElementById('responderSimulacion').addEventListener('click', () => transporte.estado.resolver?.());
    document.getElementById('reiniciarSimulacion').addEventListener('click', () => window.location.reload());
});
