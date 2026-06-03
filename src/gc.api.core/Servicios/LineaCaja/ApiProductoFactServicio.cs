using gc.api.core.Constantes;
using gc.api.core.Contratos.Servicios.LineaCaja;
using gc.api.core.Interfaces.Datos;
using gc.infraestructura.Core.Interfaces;
using gc.infraestructura.Dtos.Cajas.Request;
using gc.infraestructura.Dtos.Cajas.Response;
using gc.infraestructura.Dtos.Gen;

using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Logging;

namespace gc.api.core.Servicios.LineaCaja
{
    public class ApiProductoFactServicio : CajaBaseServicio, IApiProductoFactServicio
    {
        public ApiProductoFactServicio(IUnitOfWork uow,ILoggerHelper logger) : base(uow,logger)
        {

        }

        public CalculaFilasResDto CalcularFilas(CalcularFilasReqDto req)
        {
            var sp = ConstantesGC.StoredProcedures.SP_CAJA_CALCULA_FILAS;

            var ps = new List<SqlParameter>() {
                new SqlParameter("@caja_id", req.caja_id),
                new SqlParameter("@usu_id", req.usu_id),
                new SqlParameter("@adm_id", req.adm_id),
                new SqlParameter("@lp_id", req.lp_id),
                new SqlParameter("@caja_nro_proceso", req.caja_nro_proceso),
                new SqlParameter("@caja_nro_cierre", req.caja_nro_cierre),
                new SqlParameter("@cta_id", req.cta_id),
                new SqlParameter("@ctac_dto", req.ctac_dto),
                new SqlParameter("@ctc_id", req.ctc_id),
                new SqlParameter("@tco_letra", req.tco_letra),
                new SqlParameter("@tco_id", req.tco_id),
                new SqlParameter("@tco_id_ori", req.tco_id_ori),
                new SqlParameter("@cm_compte_ori", req.cm_compte_ori),
                new SqlParameter("@afip_id", req.afip_id),
                new SqlParameter("@afip_desc", req.afip_desc),
                //new SqlParameter("@cta_ib_nro", req.cta_ib_nro),
                //new SqlParameter("@ib_id", req.ib_id),
                //new SqlParameter("@pib_cert", req.pib_cert),
                //new SqlParameter("@pib_cert_vto", req.pib_cert_vto ),
                //new SqlParameter("@piva_cert", req.piva_cert),
                //new SqlParameter("@piva_cert_vto", req.piva_cert_vto),
                new SqlParameter("@tot_rows", req.tot_rows),
                new SqlParameter("@tot_cantidad", req.tot_cantidad),
                new SqlParameter("@tot_pvta", req.tot_pvta),
                new SqlParameter("@json_p", req.json_p)
            };

            var res = _repository.EjecutarLstSpExt<CalculaFilasResDto>(sp, ps, true);
            return res.FirstOrDefault();
        }

        public List<CotizacionResDto> ObtenerCotizacion(CotizacionReqDto req)
        {
            var sp = ConstantesGC.StoredProcedures.SP_CAJA_COTIZACION;
            var ps = new List<SqlParameter>() {
                new SqlParameter("@cta_id", req.cta_id)
            };
            var res = _repository.EjecutarLstSpExt<CotizacionResDto>(sp, ps);
            return res;
        }

        public List<PrefacturaResDto> ObtenerPrefactura(PrefacturaReqDto req)
        {
            var sp = ConstantesGC.StoredProcedures.SP_CAJA_PREFACTURA;

            var ps = new List<SqlParameter>() {
                new SqlParameter("@sec_id", req.sec_id),
                new SqlParameter("@cta_id", req.cta_id),
                new SqlParameter("@documento", req.documento),
                new SqlParameter("@usada", req.usada)
            };
            var res = _repository.EjecutarLstSpExt<PrefacturaResDto>(sp, ps);
            return res;
        }

        public List<ProductoDatosResponseDto> ObtenerProductoDatos(ProductoDatosRequestDto req)
        {
            var sp = ConstantesGC.StoredProcedures.SP_CAJA_BPROD_D;
            var ps = new List<SqlParameter>() {
                new SqlParameter("@tipo_valor", req.tipo_valor),
                new SqlParameter("@valor", req.valor),
                new SqlParameter("@lp_id", req.lp_id),
                new SqlParameter("@adm_id", req.adm_id),
                new SqlParameter("@cantidad", req.cantidad),
                new SqlParameter("@bulto", req.bulto),
                new SqlParameter("@ctc_id", req.ctc_id),
                new SqlParameter("@cta_id", req.cta_id),
                new SqlParameter("@ctac_dto", req.ctac_dto)
            };
            var res = _repository.EjecutarLstSpExt<ProductoDatosResponseDto>(sp, ps);

            return res;

        }

        public RespuestaDto CrearPrefacturaDiferida(CajaPrefDiferidaReqDto req)
        {
            var sp = ConstantesGC.StoredProcedures.SP_CAJA_PREFACTURA_CARGA;

            var ps = new List<SqlParameter>()
            {
                new SqlParameter("@caja_id",req.Caja_Id),
                new SqlParameter("@usu_id",req.Usu_Id),
                new SqlParameter("@adm_id",req.Adm_Id),
                new SqlParameter("@lp_id",req.Lp_Id),
                new SqlParameter("@caja_nro_proceso",req.Caja_Nro_Proceso),
                new SqlParameter("@caja_nro_cierre",req.Caja_Nro_Cierre),
                new SqlParameter("@cta_id",req.Cta_Id),
                new SqlParameter("@tdoc_id",req.Tdoc_Id),
                new SqlParameter("@cta_documento",req.Cta_Documento),
                new SqlParameter("@cta_denominacion",req.Cta_Denominacion),
                new SqlParameter("@sec_id",req.Sec_Id),
                new SqlParameter("@json_p",req.Json_P)
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
                return res[0];
            }
        }

        public RespuestaDto CrearPagoDiferido(CajaOpeConfirmarReq req)
        {
            return OperacionConfirmacionBase(req);
        }

        #region Metodos invocados exclusivamente desde la api de reportes

        public List<FeResDto> ObtenerFE(FeReqDto req)
        {
            var sp = ConstantesGC.StoredProcedures.SP_CAJA_FE;
            var ps = new List<SqlParameter>() {
                new SqlParameter("@tco_id", req.tco_id),
                new SqlParameter("@cm_compte", req.cm_compte),
                new SqlParameter("@cm_repetido", req.cm_repetido)
            };

            var res = _repository.EjecutarLstSpExt<FeResDto>(sp, ps);
            return res;
        }

        public List<FeIvaResDto> ObtenerFEIva(FeReqDto req)
        {
            var sp = ConstantesGC.StoredProcedures.SP_CAJA_FE_IVA;
            var ps = new List<SqlParameter>() {
                new SqlParameter("@tco_id", req.tco_id),
                new SqlParameter("@cm_compte", req.cm_compte),
                new SqlParameter("@cm_repetido", req.cm_repetido)
            };

            var res = _repository.EjecutarLstSpExt<FeIvaResDto>(sp, ps);
            return res;
        }

        public List<FePerResDto> ObtenerFEPer(FeReqDto req)
        {
            var sp = ConstantesGC.StoredProcedures.SP_CAJA_FE_PER;
            var ps = new List<SqlParameter>() {
                new SqlParameter("@tco_id", req.tco_id),
                new SqlParameter("@cm_compte", req.cm_compte),
                new SqlParameter("@cm_repetido", req.cm_repetido)
            };

            var res = _repository.EjecutarLstSpExt<FePerResDto>(sp, ps);
            return res;
        }

        public List<FeDetResDto> ObtenerFEDetalle(FeReqDto req)
        {
            var sp = ConstantesGC.StoredProcedures.SP_CAJA_FE_D;
            var ps = new List<SqlParameter>() {
                new SqlParameter("@tco_id", req.tco_id),
                new SqlParameter("@cm_compte", req.cm_compte),
                new SqlParameter("@cm_repetido", req.cm_repetido)
            };

            var res = _repository.EjecutarLstSpExt<FeDetResDto>(sp, ps);
            return res;
        }

       
        #endregion

    }
}
