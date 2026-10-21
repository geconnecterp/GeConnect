using gc.infraestructura.Dtos.Libros;

namespace gc.api.core.Contratos.Servicios.Libros;

public interface IIvaComprasResumenDatos
{
    Task<IvaComprasResumenLibro> Consultar(IvaComprasResumenFiltro filtro, CancellationToken ct);
}
