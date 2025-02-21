using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace gc.infraestructura.Dtos.Almacen
{
    public class ClienteListaDto : ProveedorListaDto
    {
        public string Ctac_habilitada { get; set; }=String.Empty;
        public string Ctac_habilitada_desc { get; set; } = String.Empty;
    }
}
