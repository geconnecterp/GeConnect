using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace gc.infraestructura.Dtos.Asientos
{
    /// <summary>
    /// DTO para verificar si un asiento puede ser modificado según su fecha
    /// </summary>
    public class VerificacionFechaDto
    {
        /// <summary>
        /// Número de ejercicio contable
        /// </summary>
        public int eje_nro { get; set; }

        /// <summary>
        /// Fecha del asiento a verificar
        /// </summary>
        public DateTime dia_fecha { get; set; }
    }
}
