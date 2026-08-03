using gc.api.core.Contratos.Servicios.LineaCaja;
using gc.api.core.Entidades;
using gc.api.core.Interfaces.Datos;
using gc.infraestructura.Core.Interfaces;
using gc.infraestructura.Dtos.Cajas.Request;
using gc.infraestructura.Dtos.Cajas.Response;
using gc.infraestructura.Dtos.Gen;
using Microsoft.Data.SqlClient;

namespace gc.api.core.Servicios.LineaCaja
{
    public class ApiRendicionServicio : Servicio<EntidadBase>, IApiRendicionServicio
    {
        private readonly ILoggerHelper _logger;

        public ApiRendicionServicio(IUnitOfWork uow, ILoggerHelper logger) : base(uow)
        {
            _logger = logger;

        }

        public List<RendicionResponseDto> ObtenerRendiciones(RendicionRequestDto request)
        {
            var sp = Constantes.ConstantesGC.StoredProcedures.SP_CAJA_REND;

            var ps = new List<SqlParameter>
            {
                new ("@adm_id", request.adm_id),
                new ("@tipo", request.tipo)
            };

            var result = _repository.EjecutarLstSpExt<RendicionResponseDto>(sp, ps, true);

            return result;
        }

        public List<RendicionNominalResponseDto> ObtenerNominaciones(RendicionNominalRequestDto request)
        {
            var sp = Constantes.ConstantesGC.StoredProcedures.SP_CAJA_REND_NOMIN;

            var ps = new List<SqlParameter>
            {
                new ("@adm_id", request.adm_id),
                new ("@ins_id", request.ins_id)
            };

            return _repository.EjecutarLstSpExt<RendicionNominalResponseDto>(sp, ps, true);
        }

        public RespuestaDto ConfirmarRendicion(RendicionCargaRequestDto request)
        {
            var sp = Constantes.ConstantesGC.StoredProcedures.SP_CAJA_REND_CARGA;

            var ps = new List<SqlParameter>
            {
                new ("@caja_nro_proceso", request.caja_nro_proceso),
                new ("@caja_nro_cierre", request.caja_nro_cierre),
                new ("@caja_id", request.caja_id),
                new ("@usu_id", request.usu_id),
                new ("@adm_id", request.adm_id),
                new ("@json_rendiciones", request.json_rendiciones)
            };

            return _repository.EjecutarLstSpExt<RespuestaDto>(sp, ps, true).FirstOrDefault()
                ?? new RespuestaDto
                {
                    resultado = -1,
                    resultado_msj = "No se recibió respuesta del proceso de rendición."
                };
        }
    }
}
