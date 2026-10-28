using gc.infraestructura.Dtos.Libros;

namespace gc.api.core.Contratos.Servicios.Libros;

public interface ICertificadosSufridosDatos
{
    Task<List<SufridoImpuesto>> Impuestos(CancellationToken ct);
    Task<List<CertificadoSufridoFila>> Consultar(CertificadosSufridosFiltro filtro, CancellationToken ct);
    Task<List<CertificadoSufridoPorCuitFila>> ConsultarPorCuit(CertificadosSufridosFiltro filtro, CancellationToken ct);
}
