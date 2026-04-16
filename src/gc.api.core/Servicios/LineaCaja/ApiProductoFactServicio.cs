using gc.api.core.Constantes;
using gc.api.core.Contratos.Servicios.LineaCaja;
using gc.api.core.Entidades;
using gc.api.core.Interfaces.Datos;
using gc.infraestructura.Dtos.Cajas.Request;
using Microsoft.Data.SqlClient;

namespace gc.api.core.Servicios.LineaCaja
{
    public class ApiProductoFactServicio : Servicio<EntidadBase>, IApiProductoFactServicio
    {
        public ApiProductoFactServicio(IUnitOfWork uow) : base(uow)
        {

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
