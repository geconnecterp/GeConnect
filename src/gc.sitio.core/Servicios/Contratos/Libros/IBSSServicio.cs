using gc.infraestructura.Core.EntidadesComunes;
using gc.infraestructura.Dtos.Libros;

namespace gc.sitio.core.Servicios.Contratos.Libros
{
    public interface IBSSServicio
    {
        Task<(List<BSumaSaldoRegDto>,MetadataGrid)> ObtenerBalanceSumaSaldos(BSSRequestDto request, string token);
    }
}
