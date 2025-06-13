using gc.infraestructura.Core.EntidadesComunes;
using gc.infraestructura.Dtos.Asientos;
using gc.infraestructura.Dtos.Libros;
using gc.infraestructura.EntidadesComunes;

namespace gc.sitio.core.Servicios.Contratos.Libros
{
    public interface ILibroDiarioServicio
    {
        Task<(List<AsientoDetalleLDDto>, MetadataGrid)> ObtenerAsientosLibroDiario(LDiarioRequest query, string token);
    }
}
