using gc.infraestructura.Dtos;

namespace gc.sitio.core.Servicios.Contratos
{
	public interface ITipoAnticipoEmpleadoServicio : IServicio<TipoAnticipoEmpleadoDto>
	{
		List<TipoAnticipoEmpleadoDto> GetTipoAnticipoEmpleado(string token);
	}
}
