using gc.infraestructura.Dtos.Cajas.Response;
using gc.infraestructura.Dtos.Gen;

namespace gc.caja.core.Servicios.Contratos.Cajas
{
    public interface ICtaCteServicio
    {
        Task<RespuestaGenerica<CtaCteResponseDto>> ObtenerCtaCte(string cta_id, string adm_id,string token);
    }
}
