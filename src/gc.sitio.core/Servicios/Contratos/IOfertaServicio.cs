using gc.infraestructura.Dtos;
using gc.infraestructura.Dtos.Gen;
using gc.infraestructura.Dtos.Importacion;
using gc.infraestructura.Dtos.Productos.Ofertas;

namespace gc.sitio.core.Servicios.Contratos
{
	public interface IOfertaServicio 
	{
        Task<RespuestaGenerica<string>> ConocerEstadoOferta(string p_id,string admId,string pl_id, string tokenCookie);
        Task<RespuestaGenerica<CanalDto>> BuscarCanales(string token);
    }
}
