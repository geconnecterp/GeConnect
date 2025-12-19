using gc.api.core.Entidades;
using gc.infraestructura.Dtos.Inventario;

namespace gc.api.core.Contratos.Servicios
{
	public interface IInventarioServicio : IServicio<Inventario>
	{
		List<InventarioListaDto> GetInventarioLista(GetInventarioListaRequest request);
		List<RubroEnInventarioDto> GetRubrosEnInventario(string inv_nro, string usu_id = "%");
		List<UsuarioEnInventarioDto> GetUSuariosEnInventario(string inv_nro);
	}
}
