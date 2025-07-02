using gc.infraestructura.Core.EntidadesComunes;
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
        Task<(List<AsientoGridDto>, MetadataGrid)> ObtenerAsientos(QueryAsiento query, string token);

        /// <summary>
        /// Envía asientos temporales a contabilidad.
        /// </summary>
        /// <param name="asientoPasa">Datos necesarios para el traspaso del asiento.</param>
        /// <param name="token">Token de autenticación.</param>
        /// <returns>Resultado de la operación de traspaso a contabilidad.</returns>
        Task<RespuestaGenerica<RespuestaDto>> PasarAsientosAContabilidad(AsientoPasaDto asientoPasa, string token);

        Task<RespuestaGenerica<AsientoDetalleDto>> ObtenerAsientoDetalle(string moviId, string token);

       
    }
}

