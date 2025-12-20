using gc.infraestructura.Dtos.Gen;
using gc.infraestructura.Dtos.Inventario;

namespace gc.sitio.core.Servicios.Contratos
{
	public interface IInventarioServicio : IServicio<InventarioDto>
	{
		List<InventarioListaDto> GetInventarioLista(GetInventarioListaRequest request, string token);
		List<RubroEnInventarioDto> GetRubrosEnInventario(string inv_nro, string token, string usu_id = "%");
		List<UsuarioEnInventarioDto> GetUsuariosEnInventario(string inv_nro, string token);
		RespuestaGenerica<RespuestaDto> ConfirmarInventario(ConfirmarInventarioRequest request, string token);
	}
}
