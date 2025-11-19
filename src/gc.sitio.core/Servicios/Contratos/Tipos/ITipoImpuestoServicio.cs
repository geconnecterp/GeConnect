using gc.infraestructura.Dtos;

namespace gc.sitio.core.Servicios.Contratos
{
	public interface ITipoImpuestoServicio : IServicio<TipoImpuestoDto>
	{
		List<TipoImpuestoDto> GetTiposDeImpuestos(string token);
	}
}
