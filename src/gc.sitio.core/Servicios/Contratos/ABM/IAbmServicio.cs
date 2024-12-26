using gc.infraestructura.Dtos;
using gc.infraestructura.Dtos.ABM;
using gc.infraestructura.Dtos.Gen;

namespace gc.sitio.core.Servicios.Contratos.ABM
{
    public interface IAbmServicio:IServicio<Dto>
    {
        Task<RespuestaGenerica<RespuestaDto>> AbmConfirmar(AbmGenDto abmGen,string token);
    }
}
