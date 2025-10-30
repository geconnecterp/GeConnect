using gc.infraestructura.Core.EntidadesComunes;
using gc.infraestructura.Dtos.Gen;
using gc.infraestructura.Dtos.Productos.Presupuestos;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace gc.sitio.core.Servicios.Contratos
{
    public interface IPresupuestoServicio
    {
        Task<RespuestaGenerica<PresupuestoListDto>> BuscarPresupuestos(QueryFilters filtro, string token);
        Task<RespuestaGenerica<PresupuestoProductoDto>> ObtenerDetallePresupuesto(string id, string token);
        Task<RespuestaGenerica<PresupE>> ObtenerEstadosPresupuesto(string token);
        Task<RespuestaGenerica<PresupuestoDto>> ObtenerPresupuesto(string id, string token);
    }
}
