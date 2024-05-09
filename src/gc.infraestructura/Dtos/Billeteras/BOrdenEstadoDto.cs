using System;
using System.Collections.Generic;
using System.Text;

namespace gc.infraestructura.Dtos.Billeteras
{
    public partial class BOrdenEstadoDto
    {
        public BOrdenEstadoDto()
        {
            Boe_id = string.Empty;
            Boe_desc = string.Empty;
        }
        public string Boe_id { get; set; }
        public string Boe_desc { get; set; }

    }
}
