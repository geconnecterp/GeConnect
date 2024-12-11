using gc.infraestructura.Dtos;

namespace gc.sitio.core.Servicios.Contratos
{
	public interface IFormaDePagoServicio : IServicio<FormaDePagoDto>
	{
		List<FormaDePagoDto> GetFormaDePagoLista(string token);
	}
}
