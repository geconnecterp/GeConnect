using gc.infraestructura.Dtos.Libros;

namespace gc.api.core.Contratos.Servicios.Libros;

public interface IIvaVentasDatos
{
    Task<List<IvaVentasDetalleDto>> Consultar(IvaVentasFiltro filtro, bool completo, CancellationToken ct);
    Task<IvaVentasOpciones> Opciones(CancellationToken ct);
}
