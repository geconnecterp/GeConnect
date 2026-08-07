using gc.api.core.Contratos.Servicios.LineaCaja;
using gc.api.core.Entidades;
using gc.api.core.Interfaces.Datos;
using gc.infraestructura.Core.Interfaces;
using gc.infraestructura.Dtos.Cajas.Request;
using gc.infraestructura.Dtos.Cajas.Response;
using gc.infraestructura.Dtos.Gen;
using Microsoft.Data.SqlClient;
using Newtonsoft.Json;
using System.Diagnostics;

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

            _logger.Log(TraceEventType.Information, $"Rendiciones API Core: ejecutando {sp}. Adm={request.adm_id}; Tipo={request.tipo}");
            var result = _repository.EjecutarLstSpExt<RendicionResponseDto>(sp, ps, true);
            _logger.Log(TraceEventType.Information, $"Rendiciones API Core: {sp} devolvio {result?.Count ?? 0} instrumentos.");

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

            _logger.Log(TraceEventType.Information, $"Rendiciones API Core: ejecutando {sp}. Adm={request.adm_id}; Instrumento={request.ins_id}");
            var result = _repository.EjecutarLstSpExt<RendicionNominalResponseDto>(sp, ps, true);
            _logger.Log(TraceEventType.Information, $"Rendiciones API Core: {sp} devolvio {result?.Count ?? 0} nominaciones.");

            return result;
        }

        public RespuestaDto ConfirmarRendicion(RendicionCargaRequestDto request)
        {
            var sp = Constantes.ConstantesGC.StoredProcedures.SP_CAJA_REND_CARGA;

            _logger.Log(TraceEventType.Information, $"Rendiciones API Core: ejecutando {sp}. REQUEST={JsonConvert.SerializeObject(request)}");


            var ps = new List<SqlParameter>
            {
                new ("@caja_nro_proceso", request.caja_nro_proceso),
                new ("@caja_nro_cierre", request.caja_nro_cierre),
                new ("@caja_id", request.caja_id),
                new ("@usu_id", request.usu_id),
                new ("@adm_id", request.adm_id),
                new ("@json_rendiciones", request.json_rendiciones)
            };

            _logger.Log(TraceEventType.Information, $"Rendiciones API Core: ejecutando {sp}. Caja={request.caja_id}; Proceso={request.caja_nro_proceso}; Cierre={request.caja_nro_cierre}; Adm={request.adm_id}; Usuario={request.usu_id}; JsonRendiciones={request.json_rendiciones}");

            var result = _repository.EjecutarLstSpExt<RespuestaDto>(sp, ps, true).FirstOrDefault()
                ?? new RespuestaDto
                {
                    resultado = -1,
                    resultado_msj = "No se recibio respuesta del proceso de rendicion."
                };

            _logger.Log(TraceEventType.Information, $"Rendiciones API Core: {sp} response. Resultado={result.resultado}; ResultadoId={result.resultado_id}; Mensaje={result.resultado_msj}; SetFocus={result.resultado_setfocus}");

            return result;
        }
    }
}
