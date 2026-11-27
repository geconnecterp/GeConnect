using gc.infraestructura.Dtos.Cajas.Response;
using Microsoft.Extensions.Logging;

namespace gc.api.core.Servicios.Reportes;

public static class FacturaATicketPdf
{
    public static byte[] Generar(FeResDto cabecera, IReadOnlyList<FeDetResDto> productos,
        IReadOnlyList<FeIvaResDto> datosIva, IReadOnlyList<FePerResDto> percepciones,
        string? logoPath, string leyenda = "ORIGINAL", ILogger? logger = null) =>
        FacturaTicketPdf.Generar("A", cabecera, productos, percepciones, datosIva, logoPath, leyenda, logger);
}
