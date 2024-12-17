using gc.infraestructura.Dtos;

namespace gc.sitio.core.Servicios.Contratos
{
	public interface IFinancieroServicio : IServicio<FinancieroDto>
	{
		List<FinancieroDto> GetFinancierosPorTipoCfLista(string tcf_id, string token);
	}
}
