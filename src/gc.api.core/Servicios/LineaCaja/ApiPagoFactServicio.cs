using gc.api.core.Constantes;
using gc.api.core.Contratos.Servicios.LineaCaja;
using gc.api.core.Interfaces.Datos;
using gc.infraestructura.Core.Interfaces;
using gc.infraestructura.Dtos.Cajas.Request;
using gc.infraestructura.Dtos.Cajas.Response;
using gc.infraestructura.Dtos.Gen;

using Microsoft.Data.SqlClient;

namespace gc.api.core.Servicios.LineaCaja
{
    public class ApiPagoFactServicio : CajaBaseServicio, IApiPagoFactServicio
    {
        public ApiPagoFactServicio(IUnitOfWork uow,ILoggerHelper logger) : base(uow,logger)
        {

        }

        public List<ValoresInsResDto> ObtenerValoresIns(ValoresInsReqDto req)
        {
            var sp = ConstantesGC.StoredProcedures.SP_CAJA_VAL_INS;
            /*
              public string tcf_id { get; set; } = string.Empty;
        public string co_tipo { get; set; } = string.Empty;
        public string cta_id { get; set; } = string.Empty;
        public string adm_id { get; set; } = string.Empty;
             */
            var ps = new List<SqlParameter>() {
                new SqlParameter("@tcf_id", req.tcf_id),
                new SqlParameter("@co_tipo", req.co_tipo),
                new SqlParameter("@cta_id", req.cta_id),
                new SqlParameter("@adm_id", req.adm_id) 
            };
            var res = _repository.EjecutarLstSpExt<ValoresInsResDto>(sp, ps);
            return res;
        }

        public List<ValoresMPResDto> ObtenerValoresMP(ValoresMPReqDto req)
        {
            var sp = ConstantesGC.StoredProcedures.SP_CAJA_VAL_MP;
            /*
              public string co_tipo { get; set; } = string.Empty;
        public string cta_id { get; set; } = string.Empty;
        public string adm_id { get; set; } = string.Empty;
             */
            var ps = new List<SqlParameter>() {
                new SqlParameter("@co_tipo", req.co_tipo),
                new SqlParameter("@cta_id", req.cta_id),
                new SqlParameter("@adm_id", req.adm_id)
            };
            var res = _repository.EjecutarLstSpExt<ValoresMPResDto>(sp, ps);
            return res;
        }

        public List<ValoresNCResDto> ObtenerValoresNC(ValoresNCReqDto req)
        {
            var sp = ConstantesGC.StoredProcedures.SP_CAJA_VAL_NC;
            /*
               public string co_tipo { get; set; } = string.Empty;
        public string cta_id { get; set; } = string.Empty;
        public string adm_id { get; set; } = string.Empty;
             */
            var ps = new List<SqlParameter>() {
                new SqlParameter("@co_tipo", req.co_tipo),
                new SqlParameter("@cta_id", req.cta_id),
                new SqlParameter("@adm_id", req.adm_id)
            };
            var res = _repository.EjecutarLstSpExt<ValoresNCResDto>(sp, ps);
            return res;
        }

        public List<ValoresPendientesResDto> ObtenerValoresPendientes(ValoresPendientesReqDto req)
        {
            var sp = ConstantesGC.StoredProcedures.SP_CAJA_COTIZACION;
            /*
              public string co_tipo { get; set; } = string.Empty;
        public string cta_id { get; set; } = string.Empty;
        public string adm_id { get; set; } = string.Empty;
             */
            var ps = new List<SqlParameter>() {
                new SqlParameter("@co_tipo", req.co_tipo),
                new SqlParameter("@cta_id", req.cta_id),
                new SqlParameter("@adm_id", req.adm_id)
            };
            var res = _repository.EjecutarLstSpExt<ValoresPendientesResDto>(sp, ps);
            return res;
        }

        public RespuestaDto ConfirmarOperacionCaja(CajaOpeConfirmarReq req)
        {
            return OperacionConfirmacionBase(req);
        }
    }
}
