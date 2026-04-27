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
            /*
             estos son los parametros del sp
            @caja_id varchar(4), 
  @usu_id varchar(10), 
  @adm_id varchar(10),
  @lp_id char(2),
  @caja_nro_proceso varchar(15),
  @caja_nro_cierre int,

  @cta_id varchar(10),
  @ctac_dto decimal(5,2),
  @ctc_id char(2),

  @tco_letra varchar(1),
  @tco_id varchar(3),
  @tco_id_ori varchar(3),
  @cm_compte_ori varchar(3),

  @afip_id char(2),
  @afip_desc varchar(80),

  @cta_ib_nro varchar(15),
  @ib_id char(1), 

  @pib_cert char(1),
  @pib_cert_vto datetime,
  @piva_cert char(1),
  @piva_cert_vto datetime,

  @tot_rows smallint,
  @tot_cantidad decimal(15,3),
  @tot_pvta decimal(15,2),
  @json_p varchar(max))
             */

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
