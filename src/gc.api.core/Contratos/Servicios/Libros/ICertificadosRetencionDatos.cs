using gc.infraestructura.Dtos.Libros;

namespace gc.api.core.Contratos.Servicios.Libros;

public interface ICertificadosRetencionDatos
{
    Task<List<RetencionImpuesto>> Impuestos(CancellationToken ct);
    Task<List<CertificadoRetencionFila>> Consultar(CertificadosRetencionFiltro filtro, CancellationToken ct);
    IAsyncEnumerable<object?[]> Ddjj(CertificadosRetencionDdjjPedido pedido, CancellationToken ct);
}
