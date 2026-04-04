using gc.api.core.Constantes;
using gc.api.core.Contratos.Servicios;
using gc.api.core.Contratos.Servicios.LineaCaja;
using gc.api.core.Entidades;
using gc.api.core.Interfaces.Datos;
using gc.infraestructura.Dtos.Cajas;
using gc.infraestructura.Dtos.Gen;
using Microsoft.Data.SqlClient;

namespace gc.api.core.Servicios.LineaCaja
{
    public class ApiCajaServicio : Servicio<EntidadBase>, IApiCajaServicio
    {
        public ApiCajaServicio(IUnitOfWork uow) : base(uow)
        {

        }

        public RespuestaDto ValidaIntegridadUsuarioCaja(CajaValidaReqDto req)
        {
            var sp = ConstantesGC.StoredProcedures.SP_CAJA_VALIDA_INTEGRIDAD;

            var ps = new List<SqlParameter>() {
                new SqlParameter("@usu_id", req.usu_id),
                new SqlParameter("@caja_id", req.caja_id),
                new SqlParameter("@adm_id", req.adm_id)
            };

            var res = _repository.EjecutarLstSpExt<RespuestaDto>(sp, ps);

            if (res != null && res.Count > 0)
            {
                return res[0];
            }
            return new() { resultado = -1, resultado_msj = "Hubo un error al validar la integridad del usuario en la caja." };
        }

        public RespuestaDto AperturaCaja(CajaValidaReqDto reqDto)
        {
            var sp = ConstantesGC.StoredProcedures.SP_CAJA_APERTURA;
            var ps = new List<SqlParameter>() {
                new SqlParameter("@usu_id", reqDto.usu_id),
                new SqlParameter("@caja_id", reqDto.caja_id),
                new SqlParameter("@adm_id", reqDto.adm_id)
            };
            var res = _repository.EjecutarLstSpExt<RespuestaDto>(sp, ps);
            if (res != null && res.Count > 0)
            {
                return res[0];
            }
            return new() { resultado = -1, resultado_msj = "Hubo un error al aperturar la caja." };
        }
    }
}
