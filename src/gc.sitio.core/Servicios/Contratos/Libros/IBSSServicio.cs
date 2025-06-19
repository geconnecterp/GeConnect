using gc.infraestructura.Core.EntidadesComunes;
using gc.infraestructura.Dtos.Libros;
using gc.infraestructura.EntidadesComunes;

namespace gc.sitio.core.Servicios.Contratos.Libros
{
    public interface IBSSServicio
    {
        Task<(List<BSumaSaldoRegDto>,MetadataGrid)> ObtenerBalanceSumaSaldos(LibroFiltroDto request, string token);        
    }
}
