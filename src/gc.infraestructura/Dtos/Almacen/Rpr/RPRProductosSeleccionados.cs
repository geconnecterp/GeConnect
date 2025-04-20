using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace gc.infraestructura.Dtos.Almacen.Rpr
{
    public class RPRProductosSeleccionados
    {
        public List<ProductoBusquedaDto> ProductosSeleccionados { get; set; } = new List<ProductoBusquedaDto>();
    }
}
