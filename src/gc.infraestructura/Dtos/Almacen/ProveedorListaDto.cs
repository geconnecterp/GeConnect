using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace gc.infraestructura.Dtos.Almacen
{
    public class ProveedorListaDto : Dto
    {
        public string Cta_Id { get; set; }=string.Empty;
        public string Cta_Denominacion { get; set; } = string.Empty;
    }
}
