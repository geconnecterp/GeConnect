using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace gc.infraestructura.EntidadesComunes
{
    /// <summary>
    /// Filtro para la búsqueda del Libro Mayor
    /// </summary>
    public class LibroFiltroDto
    {
        public int eje_nro { get; set; }
        public string ccb_id { get; set; } = string.Empty;
        public string ccb_desc { get; set; } = string.Empty;
        public bool incluirTemporales { get; set; }
        public bool rango { get; set; }
        public DateTime desde { get; set; }
        public DateTime hasta { get; set; } 
        public int Pagina { get; set; } = 1;
        public int Registros { get; set; } 
        public string Sort { get; set; } = "dia_fecha"; // Campo de ordenación
        public string SortDir { get; set; }=string.Empty;        
    }

    /// <summary>
    /// Filtro para la búsqueda del Libro Diario
    /// </summary>
    public class LDiarioFiltroDto
    {
        public int eje_nro { get; set; }
        public bool incluirTemporales { get; set; }
        public bool rango { get; set; }
        public string desde { get; set; } = string.Empty;
        public string hasta { get; set; } = string.Empty;
        public string movimientos { get; set; } = string.Empty;
        public int pagina { get; set; } = 1;
        public int itemsPorPagina { get; set; } = 50;
    }

}
