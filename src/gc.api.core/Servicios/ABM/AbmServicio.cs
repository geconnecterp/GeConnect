using gc.api.core.Constantes;
using gc.api.core.Contratos.Servicios.ABM;
using gc.api.core.Entidades;
using gc.api.core.Interfaces.Datos;
using gc.infraestructura.Dtos.ABM;
using gc.infraestructura.Dtos.Gen;
using Microsoft.Data.SqlClient;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace gc.api.core.Servicios.ABM
{
    public class AbmServicio : Servicio<EntidadBase>, IAbmServicio
    {
        public AbmServicio(IUnitOfWork uow) : base(uow)
        {

        }
        public RespuestaDto ConfirmarABM(AbmGenDto abmGen)
        {
            string sp = ConstantesGC.StoredProcedures.SP_ABM_CONFIRMAR;
            var ps = new List<SqlParameter> {
                new SqlParameter("@objeto",abmGen.Objeto),
                new SqlParameter("@abm",abmGen.Abm),
                new SqlParameter("@json",abmGen.Json)
            };

            List<RespuestaDto> respuesta = _repository.EjecutarLstSpExt<RespuestaDto>(sp, ps, true);

            if (respuesta.Count == 0)
            {
                return new RespuestaDto() { resultado = -1, resultado_msj = "No se Recepcionó respuesta del proceso." };
            }
            return respuesta.First();
        }
    }
}
