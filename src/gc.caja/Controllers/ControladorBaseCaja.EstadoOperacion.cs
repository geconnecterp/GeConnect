using gc.caja.core.Servicios.Implementacion.Cajas;
using gc.infraestructura.Dtos.Cajas;
using Microsoft.AspNetCore.Http;

namespace gc.caja.Controllers;

public partial class ControladorBaseCaja
{
    protected ReglasCreditoPago.Condiciones ObtenerCondicionesPago(CuentaDatosResultadoDto cliente)
    {
        var condiciones = ReglasCreditoPago.ObtenerCondiciones(cliente, mensaje =>
            _logger?.LogWarning("[PAGO][FP] Cuenta={Cuenta}. {Diagnostico}", cliente.cta_id, mensaje));
        _logger?.LogInformation("[PAGO][FP] Cuenta={Cuenta}; Registrado={Registrado}; Tope={Tope}; ChequeDias={Cheque}; DocumentoDias={Documento}",
            cliente.cta_id, condiciones.registrado, condiciones.tope, condiciones.dias_cheque, condiciones.dias_documento);
        return condiciones;
    }

    // Sólo snapshots de consultas/selecciones. Nunca identidad, respaldo local ni pagos aprobados.
    protected void InvalidarConsultasPago(bool incluirPendientesDiferidos = false)
    {
        var sesion = _context.HttpContext?.Session;
        if (sesion == null) return;
        foreach (var clave in sesion.Keys.Where(k => k.StartsWith("PAGO_CATALOGO:", StringComparison.Ordinal)).ToArray())
            sesion.Remove(clave);
        foreach (var clave in new[] { "CuentaCorrienteDelCliente", "CuentaCorrienteDelClienteSeleccionadaParaElCobro", "CC_CONSULTA_CUENTA", "FacturasSeleccionadasParaCobro" })
            sesion.Remove(clave);
        if (incluirPendientesDiferidos) sesion.Remove("FacturasPendientesActuales");
    }

    protected void RegistrarOperacionConfirmada()
    {
        InvalidarConsultasPago(incluirPendientesDiferidos: true);
        _context.HttpContext?.Session.SetString("CajaOperacionVersion", Guid.NewGuid().ToString("N"));
    }
}
