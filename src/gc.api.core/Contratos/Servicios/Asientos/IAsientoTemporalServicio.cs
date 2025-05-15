using gc.api.core.Entidades;
using gc.infraestructura.Dtos.Asientos;

namespace gc.api.core.Contratos.Servicios.Asientos
{
    public interface IAsientoTemporalServicio:IServicio<EntidadBase>
    {
        /// <summary>
        /// Obtiene una lista de asientos temporales según los filtros proporcionados.
        /// </summary>
        /// <param name="query">Filtros para la consulta de asientos.</param>
        /// <returns>Lista de asientos temporales.</returns>
        List<AsientoGridDto> ObtenerAsientos(QueryAsiento query);
    }
}
