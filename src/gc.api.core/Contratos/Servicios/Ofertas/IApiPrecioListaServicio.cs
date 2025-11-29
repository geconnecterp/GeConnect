using gc.infraestructura.Core.EntidadesComunes;
using gc.infraestructura.Dtos.Productos.Precio;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace gc.api.core.Contratos.Servicios.Ofertas
{
    public interface IApiPrecioListaServicio
    {
        List<PrecioListaDto> ObtenerListaPrecios();
        List<PrecioListaDetalleDto> ObtenerDetallePrecios(QueryFilters filters);
    }
}
