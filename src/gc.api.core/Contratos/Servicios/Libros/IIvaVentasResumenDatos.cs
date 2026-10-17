using gc.infraestructura.Dtos.Libros;

namespace gc.api.core.Contratos.Servicios.Libros;

public interface IIvaVentasResumenDatos
{
    Task<List<IvaVentasResumenFila>> Consultar(IvaVentasResumenFiltro filtro, CancellationToken ct);
}
