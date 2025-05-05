using gc.infraestructura.Core.EntidadesComunes;
using gc.infraestructura.Dtos;

namespace gc.sitio.core.Servicios.Contratos
{
    public interface IServicio<T> where T : Dto
    {
        Task<(List<T>, MetadataGrid)> BuscarAsync(string token);
        Task<(List<T>, MetadataGrid)> BuscarAsync(QueryFilters filters, string token);
        Task<T> BuscarAsync(object id, string token);
        Task<T> BuscarUnoAsync(string token);
        Task<bool> AgregarAsync(T entidad, string token);
        Task<bool> ActualizarAsync(object id, T entidad, string token);
        Task<bool> EliminarAsync(object id, string token);
    }
}
