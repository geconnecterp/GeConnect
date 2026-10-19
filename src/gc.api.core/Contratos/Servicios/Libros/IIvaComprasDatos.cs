using gc.infraestructura.Dtos.Libros;

namespace gc.api.core.Contratos.Servicios.Libros;

public interface IIvaComprasDatos
{
    Task<List<IvaComprasDetalleDto>> Consultar(IvaComprasFiltro filtro, bool completo, CancellationToken ct);
    Task<IvaComprasOpciones> Opciones(CancellationToken ct);
}
