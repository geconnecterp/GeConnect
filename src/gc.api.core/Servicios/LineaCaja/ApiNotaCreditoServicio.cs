using gc.api.core.Contratos.Servicios.LineaCaja;
using gc.api.core.Entidades;
using gc.api.core.Interfaces.Datos;
using gc.infraestructura.Core.Interfaces;
using gc.infraestructura.Dtos.Cajas.Request;
using gc.infraestructura.Dtos.Cajas.Response;
using Microsoft.Data.SqlClient;

namespace gc.api.core.Servicios.LineaCaja
{
    public class ApiNotaCreditoServicio : Servicio<EntidadBase>, IApiNotaCreditoServicio
    {
        private readonly ILoggerHelper _logger;
        public ApiNotaCreditoServicio(IUnitOfWork uow, ILoggerHelper logger) : base(uow)
        {
            _logger = logger;
        }

        public List<NCValidaResponseDto> ValidarNC(NCValidaRequestDto request)
        {
            var sp = Constantes.ConstantesGC.StoredProcedures.SP_CAJA_NC_VALIDA;

            var ps= new List<SqlParameter>
            {
                new ("@tco_id", request.tco_id),
                new ("@cm_compte", request.cm_compte),
                new ("@caja_nro_proceso", request.caja_nro_proceso),
                new ("@caja_nro_cierre", request.caja_nro_cierre)
            };

            var result = _repository.EjecutarLstSpExt<NCValidaResponseDto>(sp, ps,true);
            
                return result;
            
        }

        public List<NCProductoBuscarResponseDto> BuscarProducto(NCProductoBuscarRequestDto request)
        {
            var sp = Constantes.ConstantesGC.StoredProcedures.SP_CAJA_NC_B_PRODUCTO;
            var ps = new List<SqlParameter>
            {
                new ("@tco_id", request.tco_id),
                new ("@cm_compte", request.cm_compte),
                new ("@cm_repetido", request.cm_repetido),
                new ("@adm_id", request.adm_id),
                new ("@valor", request.valor),
                new ("@cantidad", request.cantidad),
                new ("@json_p", request.json_p)
            };
            var result = _repository.EjecutarLstSpExt<NCProductoBuscarResponseDto>(sp, ps, true);

            return result;

        }
    }
}
