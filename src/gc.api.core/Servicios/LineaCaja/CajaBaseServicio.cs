using gc.api.core.Constantes;
using gc.api.core.Entidades;
using gc.api.core.Interfaces.Datos;
using gc.infraestructura.Core.Interfaces;
using gc.infraestructura.Dtos.Cajas.Request;
using gc.infraestructura.Dtos.Gen;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace gc.api.core.Servicios.LineaCaja
{
    public class CajaBaseServicio: Servicio<EntidadBase>
    {
        private readonly ILoggerHelper _logger;
        public CajaBaseServicio(IUnitOfWork uow,ILoggerHelper logger) : base(uow)
        {
            _logger = logger;
        }


        protected RespuestaDto OperacionConfirmacionBase(CajaOpeConfirmarReq req)
        {
            var sp = ConstantesGC.StoredProcedures.SP_CAJA_OPE_CONFIRMAR;

            var json_sorteo = req.json_sorteo.Replace("\\", "");
            var json_union = req.json_union.Replace("\\", "");
            var json_valores = req.json_valores.Replace("\\", "");
            var json_subtotal = req.json_subtotal.Replace("\\", "");
            var json_p = req.json_p.Replace("\\", "");
            var json_cancela = req.json_cancela.Replace("\\", "");

            _logger.Log(System.Diagnostics.TraceEventType.Information,$"json_sorteo: {json_sorteo}");
            _logger.Log(System.Diagnostics.TraceEventType.Information,$"json_union: {json_union}");
            _logger.Log(System.Diagnostics.TraceEventType.Information,$"json_valores: {json_valores}");
            _logger.Log(System.Diagnostics.TraceEventType.Information,$"json_subtotal: {json_subtotal}");
            _logger.Log(System.Diagnostics.TraceEventType.Information,$"json_p: {json_p}");
            _logger.Log(System.Diagnostics.TraceEventType.Information,$"json_cancela: {json_cancela}");

            var ps = new List<SqlParameter>()
            {
                new SqlParameter("@caja_id", req.caja_id),
                new SqlParameter("@usu_id", req.usu_id),
                new SqlParameter("@adm_id", req.adm_id),
                new SqlParameter("@lp_id", req.lp_id),
                new SqlParameter("@caja_nro_proceso", req.caja_nro_proceso),
                new SqlParameter("@caja_nro_cierre", req.caja_nro_cierre),

                new SqlParameter("@usu_id_autoriza", req.caja_nro_cierre),

                new SqlParameter("@cta_id", req.cta_id),
                new SqlParameter("@ctac_dto", req.ctac_dto),
                new SqlParameter("@co_tipo", req.co_tipo),
                new SqlParameter("@ctc_id", req.ctc_id),
                new SqlParameter("@tco_letra", req.tco_letra),
                new SqlParameter("@tco_id_ori", req.tco_id_ori),
                new SqlParameter("@cm_compte_ori", req.cm_compte_ori),
                new SqlParameter("@afip_id", req.afip_id),
                new SqlParameter("@tdoc_id", req.tdoc_id),
                new SqlParameter("@cta_documento", req.cta_documento),
                new SqlParameter("@cta_denominacion", req.cta_denominacion),
                new SqlParameter("@cta_domicilio", req.cta_domicilio),
                new SqlParameter("@ve_id", req.ve_id),
                new SqlParameter("@json_p", json_p),
                new SqlParameter("@json_valores", json_valores),
                new SqlParameter("@json_cancela", json_cancela),
                new SqlParameter("@json_union", json_union),
                new SqlParameter("@json_subtotal", json_subtotal),
                new SqlParameter("@json_sorteo", json_sorteo)
            };

            var res = _repository.EjecutarLstSpExt<RespuestaDto>(sp, ps, true);
            if (!res.Any())
            {
                return new()
                {
                    resultado = -1,
                    resultado_msj = "No se logro obtener un resultado especifico para la operación. Intentelo nuevamente."
                };
            }
            else
            {
                _logger.Log(System.Diagnostics.TraceEventType.Information,$"OperacionConfirmacionBase Response : {JsonConvert.SerializeObject(res)}");
                return res[0];
            }
        }
    }
}
