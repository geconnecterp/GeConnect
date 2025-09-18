using gc.api.core.Constantes;
using gc.api.core.Contratos.Servicios.Ofertas;
using gc.api.core.Entidades;
using gc.api.core.Interfaces.Datos;
using gc.infraestructura.Dtos.ABM;
using gc.infraestructura.Dtos.Gen;
using gc.infraestructura.Dtos.Productos.Ofertas;
using Microsoft.Data.SqlClient;
using Newtonsoft.Json;

namespace gc.api.core.Servicios.Ofertas
{
    public class ApiOfertaServicio : Servicio<EntidadBase>, IApiOfertaServicio
    {
        public ApiOfertaServicio(IUnitOfWork uow) : base(uow)
        {

        }

        public List<CanalDto> BuscarCanales()
        {
            var sp = ConstantesGC.StoredProcedures.SP_PROD_CANALES_LIST;

            var ps = new List<SqlParameter>();

            List<CanalDto> canales = _repository.EjecutarLstSpExt<CanalDto>(sp, ps);
            return canales;
        }

        public string ConocerEstadoOferta(string p_id, string admId, string lp_id)
        {
            var fx = $"select {ConstantesGC.StoredFunctions.FX_PROD_OFERTA}('{p_id}','{admId}','{lp_id}')";
            string estado = _repository.EjecutarFunctionScalar<string>(fx);
            return estado;
        }

        public RespuestaDto ConfirmacionAltaOferta(AbmPlusGenDto req, ParamOferta param)
        {
            string sp = ConstantesGC.StoredProcedures.SP_PROD_OFERTA_CARGA;

            var ps = new List<SqlParameter>
            {
                new SqlParameter("@oferta", param.Precio),
                new SqlParameter("@desde", param.Desde),
                new SqlParameter("@hasta", param.Hasta),
                new SqlParameter("@tope", param.TopeVta),
                new SqlParameter("@json_p", req.Json),
                new SqlParameter("@json_a", req.Json2),
                new SqlParameter("@usu_id", req.Usuario),
                new SqlParameter("@adm_id", req.Administracion),
            };


            List<RespuestaDto> resultado = _repository.EjecutarLstSpExt<RespuestaDto>(sp, ps, true);
            if (resultado != null && resultado.Count > 0)
            {
                return resultado[0];
            }
            return new() { resultado = -1, resultado_msj = "No se logro obtener el resultado del proceso. " };

        }

        public List<OfertaEstadoDto> ObtenerEstadoOfertaProducto(string p_id)
        {
            var sp = ConstantesGC.StoredProcedures.SP_PROD_OFERTA_ESTADO;
            var ps = new List<SqlParameter>
            {
                new SqlParameter("@p_id", p_id)
            };
            List<OfertaEstadoDto> estados = _repository.EjecutarLstSpExt<OfertaEstadoDto>(sp, ps, true);
            return estados;
        }

        public List<OfertaSinActivarDto> ObtenerOfertasSinActivar(string admId, string lp_id)
        {
            var sp = ConstantesGC.StoredProcedures.SP_PROD_OFERTA_SIN_ACTIVAR;
            var ps = new List<SqlParameter>
            {
                new SqlParameter("@adm_id_ofe", admId),
                new SqlParameter("@lp_id_ofe", lp_id)
            };
            List<OfertaSinActivarDto> ofertas = _repository.EjecutarLstSpExt<OfertaSinActivarDto>(sp, ps, true);
            return ofertas;
        }
    }
}
