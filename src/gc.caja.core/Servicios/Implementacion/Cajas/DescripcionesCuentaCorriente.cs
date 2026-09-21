using gc.infraestructura.Dtos;
using gc.infraestructura.Dtos.Cajas.Response;

namespace gc.caja.core.Servicios.Implementacion.Cajas;

public static class DescripcionesCuentaCorriente
{
    public static void Completar(IEnumerable<CtaCteResponseDto> movimientos, IEnumerable<TipoComprobanteDto> tipos)
    {
        var descripciones = tipos.Where(t => !string.IsNullOrWhiteSpace(t.tco_id) && !string.IsNullOrWhiteSpace(t.tco_desc))
            .GroupBy(t => t.tco_id.Trim(), StringComparer.OrdinalIgnoreCase)
            .ToDictionary(g => g.Key, g => g.First().tco_desc.Trim(), StringComparer.OrdinalIgnoreCase);
        foreach (var movimiento in movimientos)
            if (string.IsNullOrWhiteSpace(movimiento.tco_desc) &&
                descripciones.TryGetValue(movimiento.tco_id?.Trim() ?? "", out var descripcion))
                movimiento.tco_desc = descripcion;
    }
}
