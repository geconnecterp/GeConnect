using gc.api.core.Constantes;
using gc.api.core.Contratos.Servicios.ABM;
using gc.api.core.Entidades;
using gc.api.core.Interfaces.Datos;
using gc.infraestructura.Dtos.ABM;
using gc.infraestructura.Dtos.Gen;
using Microsoft.Data.SqlClient;

namespace gc.api.core.Servicios.ABM
{
    public class AbmServicio : Servicio<EntidadBase>, IAbmServicio
    {
        public AbmServicio(IUnitOfWork uow) : base(uow)
        {

        }
        public RespuestaDto ConfirmarABM(AbmGenDto abmGen)
        {
            //Agregar try...catch
            try
            {
				string sp = ConstantesGC.StoredProcedures.SP_ABM_CONFIRMAR;
				var ps = new List<SqlParameter> {
				new SqlParameter("@objeto",abmGen.Objeto),
				new SqlParameter("@abm",abmGen.Abm),
				new SqlParameter("@usu_id",abmGen.Usuario),
				new SqlParameter("@adm_id",abmGen.Administracion),
				new SqlParameter("@json",abmGen.Json),

			};

				List<RespuestaDto> respuesta = _repository.EjecutarLstSpExt<RespuestaDto>(sp, ps, true);

				if (respuesta.Count == 0)
				{
					return new RespuestaDto() { resultado = -1, resultado_msj = "No se Recepcionó respuesta del proceso." };
				}
				return respuesta.First();
			}
            catch (Exception ex)
            {
				return new RespuestaDto() { resultado = -1, resultado_msj = ex.Message };
			}
        }
    }
}
