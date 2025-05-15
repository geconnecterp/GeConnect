using gc.infraestructura.Core.Responses;
using gc.infraestructura.Dtos.Asientos;
using gc.infraestructura.Dtos.Gen;

namespace gc.sitio.core.Servicios.Contratos.Asientos
{
    public interface IAsientoTemporalServicio
    {
        /// <summary>
        /// Obtiene una lista de asientos temporales según los filtros proporcionados.
        /// </summary>
        /// <param name="query">Filtros para la consulta de asientos.</param>
        /// <param name="token">Token de autenticación.</param>
        /// <returns>Respuesta genérica con la lista de asientos temporales.</returns>
        Task<RespuestaGenerica<AsientoGridDto>> ObtenerAsientos(QueryAsiento query, string token);
    }
}

