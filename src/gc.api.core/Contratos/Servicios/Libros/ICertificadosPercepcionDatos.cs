using gc.infraestructura.Dtos.Libros;

namespace gc.api.core.Contratos.Servicios.Libros;

public interface ICertificadosPercepcionDatos
{
    Task<List<PercepcionImpuesto>> Impuestos(CancellationToken ct);
    Task<List<CertificadoPercepcionFila>> Consultar(CertificadosPercepcionFiltro filtro, CancellationToken ct);
    IAsyncEnumerable<object?[]> Ddjj(CertificadosPercepcionDdjjPedido pedido, CancellationToken ct);
}
