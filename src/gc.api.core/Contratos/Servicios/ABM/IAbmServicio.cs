using gc.api.core.Entidades;
using gc.infraestructura.Dtos.ABM;
using gc.infraestructura.Dtos.Gen;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace gc.api.core.Contratos.Servicios.ABM
{
    public interface IAbmServicio:IServicio<EntidadBase>
    {
        RespuestaDto ConfirmarABM(AbmGenDto abmGen);
    }
}
