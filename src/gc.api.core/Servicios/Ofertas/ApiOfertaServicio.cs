using gc.api.core.Constantes;
using gc.api.core.Contratos.Servicios.Ofertas;
using gc.api.core.Entidades;
using gc.api.core.Interfaces.Datos;
using gc.infraestructura.Dtos.ABM;
using gc.infraestructura.Dtos.Gen;
using gc.infraestructura.Dtos.Productos.Ofertas;
using Microsoft.Data.SqlClient;

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

        //public RespuestaDto ConfirmacionAltaOferta(AbmPlusGenDto req)
        //{
        //    var sp = ConstantesGC.StoredProcedures.SP_PROD_OFERTA_CARGA;

        //}
    }
}
