using gc.api.core.Entidades;
using gc.infraestructura.Dtos.Asientos;
using gc.infraestructura.Dtos.Gen;

namespace gc.api.core.Contratos.Servicios.Asientos
{
    /// <summary>
    /// Define los métodos para la gestión de asientos temporales en el sistema.
    /// </summary>
    public interface IAsientoTemporalServicio : IServicio<EntidadBase>
    {
        /// <summary>
        /// Obtiene una lista de asientos temporales según los filtros proporcionados.
        /// </summary>
        /// <param name="query">Filtros para la consulta de asientos.</param>
        /// <returns>Lista de asientos temporales.</returns>
        List<AsientoGridDto> ObtenerAsientos(QueryAsiento query);

        /// <summary>
        /// Pasa los asientos temporales seleccionados a contabilidad.
        /// </summary>
        /// <param name="asientoPasa">Datos necesarios para el traspaso de asientos.</param>
        /// <returns>Resultado de la operación de traspaso.</returns>
        List<RespuestaDto> PasarAsientosTmpAContabilidad(AsientoPasaDto asientoPasa);

        /// <summary>
        /// Obtiene el detalle de un asiento temporal específico.
        /// </summary>
        /// <param name="moviId">Identificador del movimiento del asiento.</param>
        /// <returns>Detalle del asiento temporal.</returns>
        AsientoDetalleDto ObtenerAsientoDetalle(string moviId);
    }
}
