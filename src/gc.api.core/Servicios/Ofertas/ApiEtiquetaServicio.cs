using gc.api.core.Constantes;
using gc.api.core.Contratos.Servicios.Ofertas;
using gc.api.core.Entidades;
using gc.api.core.Interfaces.Datos;
using gc.infraestructura.Dtos.Productos.Etiqueta;
using log4net.Core;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Logging;

namespace gc.api.core.Servicios.Ofertas
{
    public class ApiEtiquetaServicio :Servicio<EntidadBase>, IApiEtiquetaServicio
    {
        private readonly ILogger<ApiEtiquetaServicio> _logger;
        public ApiEtiquetaServicio(IUnitOfWork uow,
            ILogger<ApiEtiquetaServicio> logger) :base(uow)
        {
            _logger = logger;
        }
        public List<CargaPreviaDto> ObtenerCargaPreviaUsuario(string adm_id)
        {
            var sp = ConstantesGC.StoredProcedures.SP_CARGA_PREVIA;

            var ps = new List<SqlParameter>();
            ps.Add(new SqlParameter("@adm_id", adm_id));

            var datos = _repository.EjecutarLstFunction<CargaPreviaDto>(sp, ps,true);

            return datos;
        }
    }
}
