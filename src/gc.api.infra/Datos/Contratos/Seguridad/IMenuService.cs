using gc.api.core.Entidades;

namespace gc.api.infra.Datos.Contratos.Security
{
	public interface IMenuService
	{
		List<UsuarioMenu> GetMenuList(string usuId);
	}
}
