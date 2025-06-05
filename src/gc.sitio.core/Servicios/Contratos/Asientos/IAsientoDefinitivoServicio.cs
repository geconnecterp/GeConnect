using gc.infraestructura.Core.EntidadesComunes;
using gc.infraestructura.Dtos.Asientos;
using gc.infraestructura.Dtos.Gen;

namespace gc.sitio.core.Servicios.Contratos.Asientos
{
    public interface IAsientoDefinitivoServicio
    {
        /// <summary>
        /// Obtiene una lista de asientos temporales según los filtros proporcionados.
        /// </summary>
        /// <param name="query">Filtros para la consulta de asientos.</param>
        /// <param name="token">Token de autenticación.</param>
        /// <returns>Respuesta genérica con la lista de asientos temporales.</returns>
        Task<(List<AsientoGridDto>, MetadataGrid)> ObtenerAsientos(QueryAsiento query, string token);
        
        Task<RespuestaGenerica<AsientoDetalleDto>> ObtenerAsientoDetalle(string moviId, string token);
       
    }
}
