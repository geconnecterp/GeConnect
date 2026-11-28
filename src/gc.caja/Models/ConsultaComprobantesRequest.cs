using System.ComponentModel.DataAnnotations;
namespace gc.caja.Models.Facturacion;

// Identificación de sólo lectura. No comparte ni modifica el contexto de NC/facturación.
public sealed class ConsultaComprobantesRequest
{
    public string TcoId { get; set; } = string.Empty;
    public string PuntoVenta { get; set; } = string.Empty;
    public string Numero { get; set; } = string.Empty;
    [Range(0, 9)] public int? Repeticion { get; set; }
    public string Formato { get; set; } = string.Empty;
    public bool TryNormalizar(out string tipo, out string comprobante, out string error) =>
        new FacturaEmitidaRequest { TcoId = TcoId, PuntoVenta = PuntoVenta, Numero = Numero }
            .TryNormalizar(out tipo, out comprobante, out error);
}