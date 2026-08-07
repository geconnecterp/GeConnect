using gc.api.core.Contratos.Servicios.LineaCaja;
using gc.api.core.Entidades;
using gc.api.core.Interfaces.Datos;
using gc.infraestructura.Core.Interfaces;
using gc.infraestructura.Dtos.Cajas.Request;
using gc.infraestructura.Dtos.Cajas.Response;
using gc.infraestructura.Dtos.Gen;
using Microsoft.Data.SqlClient;
using System.Diagnostics;

namespace gc.api.core.Servicios.LineaCaja
{
    public class ApiAnulacionServicio : Servicio<EntidadBase>, IApiAnulacionServicio
    {
        private readonly ILoggerHelper _logger;

        public ApiAnulacionServicio(IUnitOfWork uow, ILoggerHelper logger) : base(uow)
        {
            _logger = logger;
        }

        public List<AnulacionCobranzaResponseDto> BuscarCobranzas(AnulacionCobranzaBuscarRequestDto request)
        {
            var sp = Constantes.ConstantesGC.StoredProcedures.SP_CAJA_B_COBRANZAS;

            var ps = new List<SqlParameter>
            {
                new ("@caja_nro_proceso", request.caja_nro_proceso),
                new ("@caja_nro_cierre", request.caja_nro_cierre),
                new ("@cta_id", request.cta_id),
                new ("@fecha", request.fecha),
                new ("@adm_id", request.adm_id),
                new ("@usu_id", request.usu_id)
            };

            _logger.Log(TraceEventType.Information, $"Anulacion cobranza: ejecutando {sp}. Cta={request.cta_id}; Fecha={request.fecha:yyyy-MM-dd}; Proceso={request.caja_nro_proceso}; Cierre={request.caja_nro_cierre}; Adm={request.adm_id}; Usuario={request.usu_id}");

            return _repository.EjecutarLstSpExt<AnulacionCobranzaResponseDto>(sp, ps, true);
        }

        public RespuestaDto AnularCobranza(AnulacionCobranzaConfirmarRequestDto request)
        {
            var sp = Constantes.ConstantesGC.StoredProcedures.SP_CAJA_OPE_ANULA_COBRANZA;

            var ps = new List<SqlParameter>
            {
                new ("@caja_id", request.caja_id),
                new ("@usu_id", request.usu_id),
                new ("@adm_id", request.adm_id),
                new ("@caja_nro_proceso_anu", request.caja_nro_proceso_anu),
                new ("@caja_nro_cierre_anu", request.caja_nro_cierre_anu),
                new ("@caja_nro_operacion_anu", request.caja_nro_operacion_anu),
                new ("@cta_id", request.cta_id),
                new ("@usu_id_autoriza", request.usu_id_autoriza)
            };

            _logger.Log(TraceEventType.Information, $"Anulacion cobranza: ejecutando {sp}. Cta={request.cta_id}; ProcesoAnula={request.caja_nro_proceso_anu}; CierreAnula={request.caja_nro_cierre_anu}; OperacionAnula={request.caja_nro_operacion_anu}; Caja={request.caja_id}; Adm={request.adm_id}; Usuario={request.usu_id}; Autoriza={request.usu_id_autoriza}");

            return _repository.EjecutarLstSpExt<RespuestaDto>(sp, ps, true).FirstOrDefault()
                ?? new RespuestaDto
                {
                    resultado = -1,
                    resultado_msj = "No se recibio respuesta del proceso de anulacion de cobranza."
                };
        }
    }
}
