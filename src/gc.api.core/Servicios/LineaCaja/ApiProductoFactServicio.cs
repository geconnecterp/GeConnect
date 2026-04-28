using gc.api.core.Constantes;
using gc.api.core.Contratos.Servicios.LineaCaja;
using gc.api.core.Entidades;
using gc.api.core.Interfaces.Datos;
using gc.infraestructura.Dtos.Cajas.Request;
using gc.infraestructura.Dtos.Cajas.Response;
using Microsoft.Data.SqlClient;

namespace gc.api.core.Servicios.LineaCaja
{
    public class ApiProductoFactServicio : Servicio<EntidadBase>, IApiProductoFactServicio
    {
        public ApiProductoFactServicio(IUnitOfWork uow) : base(uow)
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

            var res = _repository.EjecutarLstSpExt<CalculaFilasResDto>(sp, ps,true);
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


    }
}
