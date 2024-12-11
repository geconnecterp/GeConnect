using gc.infraestructura.Dtos;

namespace gc.sitio.core.Servicios.Contratos
{
	public interface IDepartamentoServicio : IServicio<DepartamentoDto>
	{
		List<DepartamentoDto> GetDepartamentoPorProvinciaLista(string prov_id, string token);
	}
}
