using gc.infraestructura.Dtos.Cajas.Response;
using Microsoft.Extensions.Logging;

namespace gc.api.core.Servicios.Reportes;

public static class FacturaBTicketPdf
{
    public const float AnchoMm = FacturaTicketPdf.AnchoMm;
    public const int CaracteresDescripcion = FacturaTicketPdf.CaracteresDescripcion;

    public static byte[] Generar(FeResDto cabecera, IReadOnlyList<FeDetResDto> productos,
        IReadOnlyList<FePerResDto> percepciones, string? logoPath, string leyenda = "ORIGINAL", ILogger? logger = null) =>
        FacturaTicketPdf.Generar("B", cabecera, productos, percepciones, Array.Empty<FeIvaResDto>(), logoPath, leyenda, logger);
}
