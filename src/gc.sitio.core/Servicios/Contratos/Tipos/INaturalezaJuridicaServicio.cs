using gc.infraestructura.Dtos;

namespace gc.sitio.core.Servicios.Contratos
{
	public interface INaturalezaJuridicaServicio : IServicio<NaturalezaJuridicaDto>
	{
		List<NaturalezaJuridicaDto> GetNaturalezaJuridicaLista(string token);
	}
}
