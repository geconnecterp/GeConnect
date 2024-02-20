using System;
using System.Collections.Generic;
using System.Text;

namespace gc.infraestructura.Core.EntidadesComunes
{
    public class SolicitudFilters:QueryFilters
    {
        /// <summary>
        /// Todo si no se indica Estado, rol o algun texto en Search
        /// </summary>
        public new bool Todo { get { return EstadoId == default && string.IsNullOrWhiteSpace(Role) && string.IsNullOrWhiteSpace(Search) && NroOrden == default; } }
        public int EstadoId { get; set; }
        public string Role { get; set; }
        public int NroOrden { get; set; }
    }
}
