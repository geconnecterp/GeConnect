using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace gc.infraestructura.Dtos.Productos.Ofertas
{
    public class ProductoOfertaDto
    {
        public string p_id { get; set; } = string.Empty;
        public string p_desc { get; set; } = string.Empty;
        public decimal p_pcosto { get; set; }
        //se cargará al consultar un sp de precios
        public decimal p_mayorista { get; set; }
        //se cargará al consultar un sp de precios
        public decimal p_minorista { get; set; }
        //se cargar al consultar una fx de estados.
        public string p_estado { get; set; }=string.Empty;

    }
}
