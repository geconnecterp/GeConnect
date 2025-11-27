using gc.api.core.Constantes;
using gc.api.core.Contratos.Servicios.Ofertas;
using gc.api.core.Entidades;
using gc.api.core.Interfaces.Datos;
using gc.infraestructura.Dtos.Productos.Precio;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace gc.api.core.Servicios.Ofertas
{
    public class ApiPrecioListaServicio : Servicio<EntidadBase>, IApiPrecioListaServicio
    {
        private readonly ILogger<ApiPrecioListaServicio> _logger;
        public ApiPrecioListaServicio(IUnitOfWork uow,
            ILogger<ApiPrecioListaServicio> logger) : base(uow)
        {
            _logger = logger;
        }

        public List<PrecioListaDto> ObtenerListaPrecios()
        {
            var sp = ConstantesGC.StoredProcedures.SP_IE_LISTA;
            var ps = new List<SqlParameter>();

            var regs = _repository.EjecutarLstSpExt<PrecioListaDto>(sp, ps,true);
            return [.. regs.Where(x=>!string.IsNullOrEmpty(x.lp_id))];
        }
    }
}
