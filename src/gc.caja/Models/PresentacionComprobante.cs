using gc.infraestructura.EntidadesComunes.Options;

namespace gc.caja.Models;

// Describe la presentación posterior a confirmar. No emite ni imprime en el controlador.
public sealed record PresentacionComprobante(string Tipo, bool GenerarReporte, string Mensaje)
{
    public static PresentacionComprobante Crear(string? ctrlId, TipoFact facturacion,
        bool esRecibo = false, bool reporteRecibo = false)
    {
        if (esRecibo)
            return new("RECIBO", reporteRecibo, "La cobranza fue registrada correctamente.");

        // El PV del servidor determina la emisión. El JSON local es respaldo si no hay ctrl_id.
        var controlador = ctrlId?.Trim();
        var tipo = string.IsNullOrEmpty(controlador)
            ? facturacion switch { TipoFact.FE => "FE", TipoFact.CF => "CF", _ => "OTRO" }
            : controlador switch { "-1" => "FE", "50" => "CF", _ => "OTRO" };

        return tipo switch
        {
            "FE" => new(tipo, true, "Comprobante emitido mediante facturación electrónica."),
            "CF" => new(tipo, false, "Comprobante emitido mediante controlador fiscal. Verifique la impresión en el equipo."),
            _ => new(tipo, false, "Comprobante emitido correctamente.")
        };
    }
}
