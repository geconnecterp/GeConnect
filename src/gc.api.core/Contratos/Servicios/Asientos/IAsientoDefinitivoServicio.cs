using gc.api.core.Entidades;
using gc.infraestructura.Dtos.Asientos;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace gc.api.core.Contratos.Servicios.Asientos
{
    public interface IAsientoDefinitivoServicio : IServicio<EntidadBase>
    {
        /// <summary>
        /// Obtiene una lista de asientos definitivos según los filtros proporcionados.
        /// </summary>
        /// <param name="query">Filtros para la consulta de asientos.</param>
        /// <returns>Lista de asientos definitivos.</returns>
        List<AsientoDefGridDto> ObtenerAsientos(QueryAsiento query);
        /// <summary>
        /// Obtiene el detalle de un asiento definitivo específico.
        /// </summary>
        /// <param name="moviId">Identificador del movimiento del asiento.</param>
        /// <returns>Detalle del asiento definitivo.</returns>
        AsientoDetalleDto ObtenerAsientoDetalle(string moviId);
        bool VerificarFechaModificacion(int eje_nro, DateTime dia_fecha);
    }
}
