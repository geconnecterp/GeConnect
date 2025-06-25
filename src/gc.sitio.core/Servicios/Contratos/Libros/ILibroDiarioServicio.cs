using gc.infraestructura.Core.EntidadesComunes;
using gc.infraestructura.Dtos.Asientos;
using gc.infraestructura.Dtos.Libros;

namespace gc.sitio.core.Servicios.Contratos.Libros
{
    public interface ILibroDiarioServicio
    {
        Task<(List<AsientoDetalleLDDto>, MetadataGrid)> ObtenerAsientosLibroDiario(LDiarioRequest query, string token);
        Task<(List<LibroDiarioResumen>, MetadataGrid)> ObtenerAsientosLibroDiarioResumen(LDiarioRequest query, string token);
    }
}
